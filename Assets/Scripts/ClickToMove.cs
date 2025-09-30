using UnityEngine;
using UnityEngine.UIElements;

public class ClickToMove : MonoBehaviour
{
    [Header("Hareket")]
    public float speed = 15f;
    public float stopThreshold = 0.03f;

    [Header("Animasyon")]
    public Animator animator;

    [Header("UI (opsiyonel)")]
    public UIDocument uiDocument;

    [Header("Davranış")]
    public bool chooseDominantAxisAtClick = true;

    [Tooltip("Bu sınıfa sahip UI öğeleri tıklanınca karakter hareketi engellenir.")]
    public string blockClass = "blocks-move";

    enum Phase { None, Horizontal, Vertical }
    Phase phase = Phase.None;
    Vector3 target;

    static readonly int IsMoving = Animator.StringToHash("isMoving");
    static readonly int MoveX    = Animator.StringToHash("moveX");
    static readonly int MoveY    = Animator.StringToHash("moveY");

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (uiDocument == null) uiDocument = FindObjectOfType<UIDocument>();
        target = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;

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

    // ---- UI kontrolü (iki katman: Pick + worldBound fallback) ----
    bool IsPointerOverUI()
    {
        var docs = FindObjectsOfType<UIDocument>();
        Vector3 mouse = Input.mousePosition;

        foreach (var doc in docs)
        {
            var root = doc?.rootVisualElement;
            var panel = root?.panel;
            if (panel == null) continue;

            // Panel koordinatlarına dönüştür
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, mouse);

            // 1) Derin pick + parent zinciri
            var ve = panel.Pick(panelPos);
            if (ve != null && HitByHierarchy(ve, root))
                return true;

            // 2) Fallback: blocks-move sınıfına sahip TÜM öğelerde worldBound testi
            // (Pick bazı durumlarda root/hiçbir şey döndürebilir)
            // Not: Query() runtime'da çalışıyor.
            var hits = root.Query<VisualElement>()
                           .Where(e => e.pickingMode != PickingMode.Ignore
                                    && e.ClassListContains(blockClass)
                                    && e.worldBound.Contains(panelPos))
                           .ToList();

            if (hits.Count > 0)
                return true;
        }
        return false;
    }

    bool HitByHierarchy(VisualElement ve, VisualElement root)
    {
        // root'a kadar çık ve interaktif/bloğu var mı bak
        while (ve != null && ve != root)
        {
            if (ve.pickingMode != PickingMode.Ignore)
            {
                if (IsInteractiveControl(ve)) return true;
                if (ve.ClassListContains(blockClass)) return true;
            }
            ve = ve.parent;
        }
        return false;
    }

    // Sık kullanılan runtime UI Toolkit kontrolleri
    bool IsInteractiveControl(VisualElement ve)
    {
        return ve is Button
            || ve is Toggle
            || ve is Slider
            || ve is TextField
            || ve is IntegerField
            || ve is FloatField
            || ve is DropdownField
            || ve is MinMaxSlider;
    }
}
