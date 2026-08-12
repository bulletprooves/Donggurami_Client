using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextAsset localizationTable;

    [Header("Settings")]
    [SerializeField] private LanguageType defaultLanguage = LanguageType.English;

    public LanguageType CurrentLanguage => _curLng;
    public event Action OnLanguageChanged;

    // Private Area
    private LanguageType _curLng;
    private readonly Dictionary<string, string> _strByKey = new();

    #region R_Unity
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _curLng = (LanguageType)PlayerPrefs.GetInt(Const.LANGUAGE_PREFS_KEY, (int)defaultLanguage);

        LoadTable();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    #endregion

    #region R_Public
    public void SetKorean()
    {
        SetLanguage(LanguageType.Korean);
    }

    public void SetEnglish()
    {
        SetLanguage(LanguageType.English);
    }

    public string GetText(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        if (_strByKey.TryGetValue(key, out string value))
            return value;

        GameManager.Instance?.LogWarning($"Localization Key 없음: {key}");

        // 인겜용 표시
        return $"[Missing: {key}]";
    }

    public string GetText(string key, params object[] args)
    {
        string format = GetText(key);

        try
        {
            return string.Format(format, args);
        }
        catch (FormatException)
        {
            GameManager.Instance?.LogWarning($"Localization Format 오류. Key: {key}");
            return format;
        }
    }
    #endregion

    private void LoadTable()
    {
        _strByKey.Clear();

        if (localizationTable == null)
        {
            GameManager.Instance?.LogWarning("Localization CSV가 없다!!!!!");
            return;
        }

        string[] lines = localizationTable.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length <= Const.COUNT_MIN)
            return;

        int languageColumnIndex = GetLanguageColumnIndex(_curLng);

        for (int i = 1; i < lines.Length; i++)
        {
            // NOTE DAV: csv라 콤마, tsv였다면 \t
            string[] columns = lines[i].Split(',');

            if (columns.Length <= languageColumnIndex)
            {
                GameManager.Instance?.LogWarning($"CSV 열 개수가 부족합니다. Line: {i + 1}");

                continue;
            }

            string key = columns[0].Trim();

            if (string.IsNullOrWhiteSpace(key))
                continue;

            string value = columns[languageColumnIndex].Replace("\\n", "\n").Trim();

            if (_strByKey.ContainsKey(key))
                GameManager.Instance?.LogWarning($"중복 Localization Key입니다. Key: {key}");

            _strByKey[key] = value;
        }
    }

    private int GetLanguageColumnIndex(LanguageType language)
    {
        switch (language)
        {
            case LanguageType.Korean:
                return 1;

            case LanguageType.English:
                return 2;

            default:
                return 2;
        }
    }

    private void SetLanguage(LanguageType language)
    {
        GameManager.Instance?.Log($"언어 변경 시도: {_curLng} → {language}");

        if (_curLng == language)
            return;

        _curLng = language;

        LoadTable();
        SaveLanguage();

        GameManager.Instance?.Log($"언어 변경 완료: {_curLng} / 등록 이벤트: " +
            $"{OnLanguageChanged?.GetInvocationList().Length ?? 0}");

        OnLanguageChanged?.Invoke();
    }

    private void SaveLanguage()
    {
        PlayerPrefs.SetInt(Const.LANGUAGE_PREFS_KEY, (int)_curLng);

        PlayerPrefs.Save();
    }

}
