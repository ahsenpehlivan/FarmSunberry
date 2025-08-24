using UnityEngine;
using UnityEngine.AI;          // NavMeshAgent
using UnityEngine.InputSystem; // Mouse/Touch
using UnityEngine.EventSystems;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator))]
public class ClickToMoveNavMesh2D : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public SpriteRenderer spriteRenderer;

    [Header("NavMesh")]
    public float sampleMaxDistance = 2f; // hedefi navmesh'e yaklaştırma yarıçapı

    [Header("Anim/Flip")]
    public bool useFlipForWest = true;   // W yönü için East klibini aynala

    [Header("Touch/UI")]
    public bool ignoreUITouches = true;  // UI üzerindeki dokunuşları yok say

    private NavMeshAgent agent;
    private Animator animator;
    private Vector2 lastDir;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (cam == null) cam = Camera.main;
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        // 2D için şart
        agent.updateRotation = false;
        agent.updateUpAxis   = false;
    }

    void Update()
    {
        // 1) Tap/Click yakala (mouse + tek parmak touch)
        if (TryGetTapScreenPosition(out Vector2 screenPos, ignoreUITouches))
        {
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
            w.z = transform.position.z;

            // 2) Hedefi navmesh'e "snap" et ve yola çık
            if (NavMesh.SamplePosition(w, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(w);
        }

        // 3) Animasyon parametreleri (agent.velocity)
        Vector2 v = agent.velocity;
        float speed = v.sqrMagnitude;
        Vector2 dir = speed > 0.0001f ? v.normalized : lastDir;

        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetFloat("Speed", speed);

        if (speed > 0.0001f)
        {
            lastDir = dir;
            animator.SetFloat("LastX", lastDir.x);
            animator.SetFloat("LastY", lastDir.y);
            animator.SetBool("IsMoving", true);
        }
        else animator.SetBool("IsMoving", false);

        // 4) W yönü aynalama (E klibini W'de kullanıyorsan)
        if (useFlipForWest && spriteRenderer != null)
        {
            float faceX = Mathf.Abs(v.x) > 0.05f ? v.x : lastDir.x;
            spriteRenderer.flipX = faceX < -0.05f;
        }
    }

    // Mouse + Touch için ortak TAP yakalama (Input System)
    static bool TryGetTapScreenPosition(out Vector2 pos, bool blockUI)
    {
        // Mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pos = Mouse.current.position.ReadValue();
            if (blockUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return false;
            return true;
        }
        // Touch
        if (Touchscreen.current != null)
        {
            var t = Touchscreen.current.primaryTouch;
            if (t.press.wasPressedThisFrame)
            {
                pos = t.position.ReadValue();
                if (blockUI && EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(t.touchId.ReadValue()))
                    return false;
                return true;
            }
        }
        pos = default; return false;
    }
}
