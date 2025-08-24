using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class TopDownController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public bool normalizeDiagonal = true;

    [Header("Mirroring")]
    public bool useFlipForWest = true;          // ← aynalamayı buradan aç/kapat
    public SpriteRenderer spriteRenderer;       // Inspector’dan atayabilirsin

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 _moveInput, _lastNonZero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        // SpriteRenderer aynı objede değilse çocuklarda ara ve bul:
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    // Player Input (Send Messages) → Move action’ı
    public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

    private void Update()
    {
        // Animator parametreleri
        animator.SetFloat("MoveX", _moveInput.x);
        animator.SetFloat("MoveY", _moveInput.y);
        float speed = _moveInput.sqrMagnitude;
        animator.SetFloat("Speed", speed);

        if (speed > 0.0001f) {
            _lastNonZero = _moveInput.normalized;
            animator.SetFloat("LastX", _lastNonZero.x);
            animator.SetFloat("LastY", _lastNonZero.y);
            animator.SetBool("IsMoving", true);
        } else {
            animator.SetBool("IsMoving", false);
        }

        // ← AYNALAMA (sola bakarken flipX=true)
        if (useFlipForWest && spriteRenderer != null)
        {
            // O anda yatay giriş varsa onu, yoksa son yönü baz al:
            float faceX = (Mathf.Abs(_moveInput.x) > 0.1f) ? _moveInput.x : _lastNonZero.x;
            spriteRenderer.flipX = faceX < -0.1f;  // sola bakıyorsa true
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;          // kapattıysan güvenli sıfırla
        }
    }

    private void FixedUpdate()
    {
        Vector2 v = _moveInput;
        if (normalizeDiagonal && v.sqrMagnitude > 1f) v = v.normalized;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.MovePosition(rb.position + v * moveSpeed * Time.fixedDeltaTime);
    }
}
