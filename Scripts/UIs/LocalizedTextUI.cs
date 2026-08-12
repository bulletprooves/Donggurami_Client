using TMPro;
using UnityEngine;

// 언어 설정에 따라 변경돼야 하는 텍스트에
// 키 값을 추가하여 붙여두시오

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTextUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string localizationKey;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI targetText;    // 그냥 확인용이므로 비워두셈. Awkae에서 알아서 찾음

    private bool _isSubscribed;

    #region R_Unity
    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        Subscribe();
        RefreshText();
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshText();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
    #endregion

    #region R_Public
    public void SetKey(string key)
    {
        localizationKey = key;
        RefreshText();
    }

    public void RefreshText()
    {
        // NOTE DAV: 이벤트 콜백으로도 사용함, 런타임에서 변경되니까

        if (targetText == null)
            return;

        if (LocalizationManager.Instance == null)
            return;

        targetText.text = LocalizationManager.Instance.GetText(localizationKey);
    }
    #endregion

    private void Subscribe()
    {
        if (_isSubscribed)
            return;

        if (LocalizationManager.Instance == null)
            return;

        LocalizationManager.Instance.OnLanguageChanged += RefreshText;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed)
            return;

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshText;

        _isSubscribed = false;
    }
}