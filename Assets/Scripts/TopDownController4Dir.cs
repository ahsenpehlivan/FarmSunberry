using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class TopDownController4Dir : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public bool normalizeDiagonal = true;   // çaprazlarda hız sabit kalsın
    public bool useFlipForWest = true;      // sol klibin yoksa E'yi mirrorda kullan

    [Header("Refs (auto)")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Vector2 _moveInput;
    private Vector2 _lastNonZero; // idle bakış yönü için

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Input System: Player/Move (Vector2) action'ına bağlı
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        // Animator parametreleri (4 yön Blend Tree MoveX/MoveY + Idle için LastX/LastY)
        animator.SetFloat("MoveX", _moveInput.x);
        animator.SetFloat("MoveY", _moveInput.y);

        float speed = _moveInput.sqrMagnitude;
        animator.SetFloat("Speed", speed);

        if (speed > 0.0001f)
        {
            _lastNonZero = _moveInput.normalized;
            animator.SetFloat("LastX", _lastNonZero.x);
            animator.SetFloat("LastY", _lastNonZero.y);
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        // Sol klibin yoksa flipX ile aynala (yürürken yatay baskınsa onu, duruyorsa son yönü kullan)
        if (useFlipForWest)
        {
            float faceX = (Mathf.Abs(_moveInput.x) > 0.1f) ? _moveInput.x : _lastNonZero.x;
            spriteRenderer.flipX = faceX < -0.1f;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        Vector2 v = _moveInput;
        if (normalizeDiagonal && v.sqrMagnitude > 1f) v = v.normalized;

        rb.MovePosition(rb.position + v * moveSpeed * Time.fixedDeltaTime);
    }
}
