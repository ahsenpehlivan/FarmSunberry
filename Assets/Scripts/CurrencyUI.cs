using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private CurrencyType type = CurrencyType.Dollar;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image icon; // opsiyonel: animasyonda büyütürüz
    [SerializeField] private bool compactFormat = true; // 1.2K, 3.4M gibi

    private void OnEnable()
    {
        if (CurrencyManager.I != null)
        {
            CurrencyManager.I.OnCurrencyChanged += HandleChanged;
            // Açılışta bir kere doldur
            HandleChanged(type, CurrencyManager.I.Get(type));
        }
    }

    private void OnDisable()
    {
        if (CurrencyManager.I != null)
            CurrencyManager.I.OnCurrencyChanged -= HandleChanged;
    }

    private void HandleChanged(CurrencyType changedType, int value)
    {
        if (changedType != type) return;
        amountText.text = compactFormat ? ToCompact(value) : value.ToString("N0"); // 1.234 gibi ayırabilir
        StopAllCoroutines();
        StartCoroutine(PopAnim());
    }

    private IEnumerator PopAnim()
    {
        // Icon varsa onu, yoksa yazıyı büyüt küçült
        Transform t = icon != null ? icon.rectTransform : amountText.rectTransform;
        Vector3 baseScale = t.localScale;
        Vector3 upScale = baseScale * 1.15f;

        float dur = 0.08f;
        float time = 0f;
        while (time < dur)
        {
            time += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(baseScale, upScale, time / dur);
            yield return null;
        }
        time = 0f;
        while (time < dur)
        {
            time += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(upScale, baseScale, time / dur);
            yield return null;
        }
        t.localScale = baseScale;
    }

    // 1.2K, 3.4M gibi kısaltılmış görünüm
    private string ToCompact(long n)
    {
        if (n >= 1_000_000_000) return (n / 1_000_000_000f).ToString("0.#") + "B";
        if (n >= 1_000_000)     return (n / 1_000_000f).ToString("0.#") + "M";
        if (n >= 1_000)         return (n / 1_000f).ToString("0.#") + "K";
        return n.ToString();
    }
}
