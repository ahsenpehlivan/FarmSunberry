using UnityEngine;

public class ClickToMove : MonoBehaviour
{
    [Header("Hareket")]
    public float speed = 15f;
    public float stopThreshold = 0.03f;

    [Header("Animasyon")]
    public Animator animator;

    [Header("Davranış")]
    public bool chooseDominantAxisAtClick = true;

    enum Phase { None, Horizontal, Vertical }
    Phase phase = Phase.None;
    Vector3 target;

    static readonly int IsMoving = Animator.StringToHash("isMoving");
    static readonly int MoveX = Animator.StringToHash("moveX");
    static readonly int MoveY = Animator.StringToHash("moveY");

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        target = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target = new Vector3(w.x, w.y, transform.position.z);

            if (chooseDominantAxisAtClick)
            {
                phase = Mathf.Abs(target.x - transform.position.x) >= Mathf.Abs(target.y - transform.position.y)
                      ? Phase.Horizontal
                      : Phase.Vertical;
            }
            else
            {
                phase = Phase.Horizontal;
            }
        }

        Vector3 pos = transform.position;
        Vector2 dir = Vector2.zero;

        bool xDone = Mathf.Abs(target.x - pos.x) <= stopThreshold;
        bool yDone = Mathf.Abs(target.y - pos.y) <= stopThreshold;

        if (xDone && yDone)
            phase = Phase.None;

        if (phase == Phase.Horizontal && !xDone)
        {
            float sign = Mathf.Sign(target.x - pos.x);
            dir = new Vector2(sign, 0f);
            float newX = Mathf.MoveTowards(pos.x, target.x, speed * Time.deltaTime);
            transform.position = new Vector3(newX, pos.y, pos.z);

            if (Mathf.Abs(target.x - newX) <= stopThreshold)
                phase = Phase.Vertical;
        }
        else if (phase == Phase.Vertical && !yDone)
        {
            float sign = Mathf.Sign(target.y - pos.y);
            dir = new Vector2(0f, sign);
            float newY = Mathf.MoveTowards(pos.y, target.y, speed * Time.deltaTime);
            transform.position = new Vector3(pos.x, newY, pos.z);

            if (Mathf.Abs(target.y - newY) <= stopThreshold)
                phase = Phase.None;
        }

        bool moving = dir != Vector2.zero;

        if (animator != null)
        {
            animator.SetBool(IsMoving, moving);
            if (moving)
            {
                animator.SetFloat(MoveX, dir.x);
                animator.SetFloat(MoveY, dir.y);
            }
        }
    }
}
