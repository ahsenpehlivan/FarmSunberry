using UnityEngine;
using System;

public enum CurrencyType { Dollar, Gold }

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager I;

    [Header("Starting Values")]
    [SerializeField] private int dollars = 0;
    [SerializeField] private int gold = 0;

    public event Action<CurrencyType, int> OnCurrencyChanged;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        Load();
        // İlk açılışta UI’ları güncelle
        OnCurrencyChanged?.Invoke(CurrencyType.Dollar, dollars);
        OnCurrencyChanged?.Invoke(CurrencyType.Gold, gold);
    }

    // ----- Public API -----
    public int Get(CurrencyType type) => type == CurrencyType.Dollar ? dollars : gold;

    public void Set(CurrencyType type, int amount)
    {
        amount = Mathf.Max(0, amount);
        if (type == CurrencyType.Dollar) dollars = amount; else gold = amount;
        OnCurrencyChanged?.Invoke(type, Get(type));
        Save();
    }

    public void Add(CurrencyType type, int amount)
    {
        if (amount == 0) return;
        int cur = Get(type);
        Set(type, cur + amount);
    }

    public bool Spend(CurrencyType type, int amount)
    {
        if (amount <= 0) return true;
        int cur = Get(type);
        if (cur < amount) return false;
        Set(type, cur - amount);
        return true;
    }

    // ----- Save / Load (basit) -----
    private void Save()
    {
        PlayerPrefs.SetInt("currency_dollar", dollars);
        PlayerPrefs.SetInt("currency_gold", gold);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        dollars = PlayerPrefs.GetInt("currency_dollar", dollars);
        gold    = PlayerPrefs.GetInt("currency_gold", gold);
    }

#if UNITY_EDITOR
    // Editor’da değer değişince UI güncelle
    void OnValidate()
    {
        if (Application.isPlaying) return;
        OnCurrencyChanged?.Invoke(CurrencyType.Dollar, dollars);
        OnCurrencyChanged?.Invoke(CurrencyType.Gold, gold);
    }
#endif
}
