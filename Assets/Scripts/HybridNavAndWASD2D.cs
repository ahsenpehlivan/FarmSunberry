using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NavMeshAgent))]
public class HybridNavAndWASD2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float runMultiplier = 1.5f; // Shift ile koşma (isteğe bağlı)

    [Header("Click-to-Move")]
    [SerializeField] float clickSampleMaxDistance = 8f;
    [SerializeField] float stopThreshold = 0.05f;
    [SerializeField] LayerMask clickMask = ~0;   // sadece Ground layer'ını seçebilirsin

    [Header("Anim")]
    [SerializeField] Animator animator; // isMoving (Bool), moveX (Float), moveY (Float)

    Rigidbody2D rb;
    NavMeshAgent agent;
    Camera cam;

    Vector2 input, velocity, lastDir = Vector2.down; // idle yönü koru
    enum Mode { Keyboard, ClickPath }
    Mode mode = Mode.Keyboard;

    static readonly int IsMoving = Animator.StringToHash("isMoving");
    static readonly int MoveX    = Animator.StringToHash("moveX");
    static readonly int MoveY    = Animator.StringToHash("moveY");

    // ---- Güvenli durum kontrolü ve yardımcılar ----
    bool AgentReady => agent != null && agent.enabled && agent.isOnNavMesh;

    void SafeResetPath()
    {
        if (!AgentReady) return;
        if (!agent.isStopped) agent.isStopped = true;
        if (agent.hasPath) agent.ResetPath();
    }

    void SafeSetDestination(Vector3 worldPos)
    {
        if (!AgentReady) return;
        agent.isStopped = false;
        agent.SetDestination(worldPos);
    }
    // -----------------------------------------------

    void Awake()
    {
        rb    = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();
        cam   = Camera.main;

        if (cam == null)
            Debug.LogError("MainCamera tag'li kamera bulunamadı! (Camera.main null)");

        // 2D navmesh için
        agent.updateRotation = false;
        agent.updateUpAxis   = false;
        agent.updatePosition = false;   // pozisyonu RB2D ile taşıyoruz
        agent.speed = moveSpeed;

        rb.gravityScale = 0f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        // Ajanı başta NavMesh üzerine “oturt”
        if (agent != null && agent.enabled && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                transform.position = hit.position;
            }
            else
            {
                Debug.LogWarning("Agent NavMesh üzerinde değil ve yakın nokta bulunamadı.");
            }
        }

        if (agent != null) agent.nextPosition = transform.position;
    }

    void OnEnable()
    {
        // Bazen disable/enable döngülerinde pozisyon senkronu kaçabiliyor
        if (agent != null) agent.nextPosition = transform.position;
    }

    void Update()
    {
        if (agent != null) agent.nextPosition = transform.position;

        // WASD
        float speedMul = Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f;
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.sqrMagnitude > 0.01f)
        {
            if (mode != Mode.Keyboard) SafeResetPath();
            mode = Mode.Keyboard;
        }

        // Sol tık: hedef seç
        if (Input.GetMouseButtonDown(0)
            && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            && cam != null)
        {
            var screen = Input.mousePosition;
            var world  = cam.ScreenToWorldPoint(screen);
            world.z = transform.position.z;

            // (Opsiyonel) sadece belirli layer'a izin ver
            var hit2D = Physics2D.Raycast(world, Vector2.zero, 0.01f, clickMask);
            if (hit2D.collider != null) world = hit2D.point;

            if (NavMesh.SamplePosition(world, out var hit, clickSampleMaxDistance, NavMesh.AllAreas))
            {
                SafeSetDestination(hit.position);
                mode = Mode.ClickPath;
            }
            else
            {
                Debug.Log("Tıklanan konum NavMesh'e yakın değil.");
            }
        }

        // Sağ tık: rotayı iptal et
        if (Input.GetMouseButtonDown(1))
        {
            SafeResetPath();
            mode = Mode.Keyboard;
        }

        // Hız belirle
        Vector2 targetVel;
        if (mode == Mode.Keyboard)
        {
            targetVel = input.normalized * (moveSpeed * speedMul);
        }
        else
        {
            // AgentReady değilse güvenli şekilde düş
            if (AgentReady)
            {
                var desired = agent.desiredVelocity;
                targetVel = new Vector2(desired.x, desired.y);

                if (!agent.pathPending &&
                    agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, stopThreshold))
                {
                    SafeResetPath();
                    targetVel = Vector2.zero;
                    mode = Mode.Keyboard;
                }
            }
            else
            {
                // NavMesh yoksa akışın kopmaması için klavyeye dön
                targetVel = Vector2.zero;
                mode = Mode.Keyboard;
            }
        }

        // Basit yumuşatma (istersen kaldırabilirsin)
        velocity = Vector2.MoveTowards(velocity, targetVel, 20f * Time.deltaTime);

        // Anim
        if (animator)
        {
            bool moving = velocity.sqrMagnitude > 0.0001f;
            animator.SetBool(IsMoving, moving);

            if (moving)
            {
                lastDir = velocity.normalized;
                animator.SetFloat(MoveX, lastDir.x);
                animator.SetFloat(MoveY, lastDir.y);
            }
            else
            {
                // dururken de baktığı yönü korusun
                animator.SetFloat(MoveX, lastDir.x);
                animator.SetFloat(MoveY, lastDir.y);
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    void OnDisable()
    {
        velocity = Vector2.zero;
        SafeResetPath(); // <-- artık korumalı
    }
}
