using System;
using UnityEngine;

public class XPSystem : MonoBehaviour
{
    [Header("Seviye & XP")]
    [SerializeField] int startLevel = 1;
    [SerializeField] int startXP = 0;

    [Header("Eğri Ayarı (XP gereksinimi)")]
    [SerializeField] int baseXP = 100;        // seviye 1 için taban
    [SerializeField] float growth = 1.5f;     // 1.3–1.7 arası mobil için ideal

    public int Level { get; private set; }
    public int CurrentXP { get; private set; }

    public event Action<int,int,int> OnXPChanged; // (current, needed, level)
    public event Action<int> OnLevelUp;           // newLevel

    void Awake()
    {
        Level = Mathf.Max(1, startLevel);
        CurrentXP = Mathf.Max(0, startXP);
        RaiseXPChanged();
    }

    public int XPNeededForLevel(int level)
    {
        // Basit bir eğri: baseXP * level^growth
        return Mathf.Max(1, Mathf.RoundToInt(baseXP * Mathf.Pow(level, growth)));
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        CurrentXP += amount;

        // Birden fazla seviye atlamayı da destekle
        while (CurrentXP >= XPNeededForLevel(Level))
        {
            CurrentXP -= XPNeededForLevel(Level);
            Level++;
            OnLevelUp?.Invoke(Level);
        }

        RaiseXPChanged();
    }

    public void SetXP(int absoluteXP)
    {
        CurrentXP = Mathf.Max(0, absoluteXP);
        while (CurrentXP >= XPNeededForLevel(Level))
        {
            CurrentXP -= XPNeededForLevel(Level);
            Level++;
            OnLevelUp?.Invoke(Level);
        }
        RaiseXPChanged();
    }

    void RaiseXPChanged()
    {
        OnXPChanged?.Invoke(CurrentXP, XPNeededForLevel(Level), Level);
    }
}
