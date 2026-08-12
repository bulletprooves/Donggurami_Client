using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BpmSliderUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Slider bpmSlider;
    [SerializeField] private TextMeshProUGUI bpmText;

    #region R_Unity
    private void Awake()
    {
        if (bpmSlider != null)
            bpmSlider.onValueChanged.AddListener(OnBpmSliderChanged);
    }

    private void Start()
    {
        if (rhythmManager == null || bpmSlider == null || bpmText == null)
            return;

        // NOTE: 슬라이더 나중에 꾸미기 필요함
        bpmSlider.wholeNumbers = false;
        bpmSlider.minValue = rhythmManager.MinBpm;
        bpmSlider.maxValue = rhythmManager.MaxBpm;
        bpmSlider.SetValueWithoutNotify(rhythmManager.Bpm);
        bpmText.text = Utility.ToString_ZeroPad(rhythmManager.Bpm, 3);
    }

    private void OnDestroy()
    {
        if (bpmSlider != null)
            bpmSlider.onValueChanged.RemoveListener(OnBpmSliderChanged);

        // 람다 쓰면 안됨. 다른 이벤트라 제거가 안됨.
        // bpmSlider.onValueChanged.AddListener(x => OnBpmSliderChanged(x));
    }
    #endregion

    private void OnBpmSliderChanged(float value)
    {
        if (rhythmManager == null)
            return;

        rhythmManager.SetBpm(Mathf.RoundToInt(value));
        bpmText.text = Utility.ToString_ZeroPad(rhythmManager.Bpm, 3);
    }
}