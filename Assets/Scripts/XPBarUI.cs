using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] XPSystem xpSystem;
    [SerializeField] Slider slider;         // Min=0, Max=1
    [SerializeField] TMP_Text levelText;    // "Lv 1"
    [SerializeField] TMP_Text xpText;       // "0 / 100 XP"

    [Header("Animasyon")]
    [SerializeField] float fillSpeed = 2f;  // 1–4 arası akıcı

    float target;

    void OnEnable()
    {
        if (xpSystem != null) xpSystem.OnXPChanged += HandleXPChanged;
        // İlk çizim
        if (xpSystem != null)
            HandleXPChanged(xpSystem.CurrentXP, xpSystem.XPNeededForLevel(xpSystem.Level), xpSystem.Level);
    }

    void OnDisable()
    {
        if (xpSystem != null) xpSystem.OnXPChanged -= HandleXPChanged;
    }

    void HandleXPChanged(int current, int needed, int level)
    {
        target = needed > 0 ? (float)current / needed : 0f;

        if (levelText) levelText.text = $"Lv {level}";
        if (xpText)    xpText.text    = $"{current} / {needed} XP";

        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        if (slider == null) yield break;

        while (Mathf.Abs(slider.value - target) > 0.001f)
        {
            slider.value = Mathf.MoveTowards(slider.value, target, fillSpeed * Time.deltaTime);
            yield return null;
        }
        slider.value = target; // tam oturt
    }
}
