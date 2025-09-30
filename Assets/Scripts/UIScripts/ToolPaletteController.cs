using UnityEngine;
using UnityEngine.UIElements;

public class TopBarToolbarController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] float animationDuration = 0.18f;   // saniye
    [SerializeField] int itemSize = 64;                 // 64px ikon
    [SerializeField] int gap = 8;                       // aralar 8px
    [SerializeField] int padding = 8;                   // üst-alt 8px (ToolPalette’de verdik)

    VisualElement bottomRightButton;
    VisualElement toolPalette;

    bool isOpen = false;
    IVisualElementScheduledItem animHandle;
    float animStartTime;
    float startHeight, targetHeight;
    float startOpacity, targetOpacity;

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        bottomRightButton = root.Q<VisualElement>("BottomRightButton");
        toolPalette = root.Q<VisualElement>("ToolPalette");

        // Güvenlik
        if (bottomRightButton == null || toolPalette == null)
        {
            Debug.LogError("UI query failed: BottomRightButton or ToolPalette not found.");
            return;
        }

        // Başlangıç kapalı
        toolPalette.style.display = DisplayStyle.None;
        toolPalette.style.opacity = 0f;
        toolPalette.style.height = 0;

        bottomRightButton.RegisterCallback<ClickEvent>(OnBottomRightClick);
    }

    void OnDisable()
    {
        if (bottomRightButton != null)
            bottomRightButton.UnregisterCallback<ClickEvent>(OnBottomRightClick);
        animHandle?.Pause();
        animHandle = null;
    }

    void OnBottomRightClick(ClickEvent evt)
    {
        TogglePalette();
    }

    void TogglePalette()
    {
        // İçerik yüksekliğini hesapla: N adet ikon, aralarda (N-1)*gap, üst-alt padding*2
        int n = toolPalette.childCount; // 3 (Hoe, Axe, Water)
        int contentHeight = (n * itemSize) + ((n - 1) * gap) + (padding * 2);

        // Hedef değerler
        isOpen = !isOpen;

        // Display'yi hemen açmazsak ölçüm yapamayız; anim başladıktan sonra kapatırsak flicker olmaz
        if (isOpen && toolPalette.style.display == DisplayStyle.None)
            toolPalette.style.display = DisplayStyle.Flex;

        startHeight = toolPalette.resolvedStyle.height; // mevcut px
        targetHeight = isOpen ? contentHeight : 0f;

        startOpacity = toolPalette.resolvedStyle.opacity;
        targetOpacity = isOpen ? 1f : 0f;

        animStartTime = Time.realtimeSinceStartup;

        // Her karede yükseklik & opacity lerp
        animHandle?.Pause();
        animHandle = toolPalette.schedule.Execute(Animate).Every(16); // ~60 FPS
    }

    void Animate()
    {
        float t = Mathf.InverseLerp(animStartTime, animStartTime + animationDuration, Time.realtimeSinceStartup);
        t = Mathf.Clamp01(t);

        // yumuşak ease-out
        float eased = 1f - Mathf.Pow(1f - t, 3f);

        float h = Mathf.Lerp(startHeight, targetHeight, eased);
        float o = Mathf.Lerp(startOpacity, targetOpacity, eased);

        toolPalette.style.height = h;
        toolPalette.style.opacity = o;

        // Bitti mi?
        if (t >= 1f)
        {
            animHandle?.Pause();
            animHandle = null;

            if (!isOpen)
            {
                // kapanınca tıklamayı bloklamasın
                toolPalette.style.display = DisplayStyle.None;
            }
        }
    }
}
