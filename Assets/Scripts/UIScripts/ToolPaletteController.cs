using UnityEngine;
using UnityEngine.UIElements;

public class ToolPaletteController : MonoBehaviour
{
    private VisualElement _root;
    private VisualElement _bottomRightBtn;
    private VisualElement _palette;

    // seçili aracı highlight etmek için basit bir class adı
    private const string SelectedClass = "tool-selected";
    private VisualElement _currentSelected;

    void Awake()
    {
        var doc = GetComponent<UIDocument>();
        _root = doc.rootVisualElement;

        _bottomRightBtn = _root.Q<VisualElement>("BottomRightButton");
        _palette        = _root.Q<VisualElement>("ToolPalette");

        // Aç/Kapa
        _bottomRightBtn?.RegisterCallback<ClickEvent>(_ => TogglePalette());

        // Araç butonlarına tıklama
        var toolHoe   = _root.Q<VisualElement>("Tool_Hoe");
        var toolAxe   = _root.Q<VisualElement>("Tool_Axe");
        var toolWater = _root.Q<VisualElement>("Tool_Water");

        toolHoe?.RegisterCallback<ClickEvent>(_ => SelectTool(toolHoe, "Hoe"));
        toolAxe?.RegisterCallback<ClickEvent>(_ => SelectTool(toolAxe, "Axe"));
        toolWater?.RegisterCallback<ClickEvent>(_ => SelectTool(toolWater, "WateringCan"));

        // Palet açıkken dışarı tıklayınca kapat
        _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown);
    }

    private void TogglePalette()
    {
        if (_palette == null) return;
        _palette.style.display = 
            _palette.resolvedStyle.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SelectTool(VisualElement ve, string toolId)
    {
        // basit highlight
        if (_currentSelected != null) _currentSelected.RemoveFromClassList(SelectedClass);
        _currentSelected = ve;
        _currentSelected.AddToClassList(SelectedClass);

        // Tool seçimini burada oyun state'ine aktar (envanter, player controller vs.)
        Debug.Log("Selected tool: " + toolId);

        // seçince paleti kapatmak istersen:
        _palette.style.display = DisplayStyle.None;
    }

    private void OnRootPointerDown(PointerDownEvent evt)
    {
        if (_palette == null) return;
        if (_palette.resolvedStyle.display == DisplayStyle.None) return;

        // tıklama panelin dışında mı?
        var mousePos = evt.position;
        if (!_palette.worldBound.Contains(mousePos) &&
            !_bottomRightBtn.worldBound.Contains(mousePos))
        {
            _palette.style.display = DisplayStyle.None;
        }
    }
}
