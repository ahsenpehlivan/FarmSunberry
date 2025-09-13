using UnityEngine;
using UnityEngine.UIElements;

public class SafeAreaUITK : MonoBehaviour
{
    void Start()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        // UXML’deki #UpperBar elementini bul
        var upperBar = root.Q<VisualElement>("UpperBar");

        if (upperBar != null)
        {
            // Çentik olan cihazlarda güvenli alan
            Rect safeArea = Screen.safeArea;
            float topInset = Screen.height - safeArea.yMax;

            // üst barı biraz aşağıya kaydır
            upperBar.style.marginTop = topInset;
        }
    }
}
