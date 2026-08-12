using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadiusStepSliderUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Config config;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;

    [SerializeField] private string format = "{0:0}ly";

    private float[] radiusSteps = { 4f, 8f, 16f, 32f, 64f };
    private int defaultIndex = 1;

    public float CurrentRadius
    {
        get
        {
            if (radiusSteps == null || radiusSteps.Length == Const.ZERO)
                return config != null ? config.DefaultTrackRadius : Const.ZERO;

            int index = Mathf.RoundToInt(slider != null ? slider.value : defaultIndex);
            return radiusSteps[Mathf.Clamp(index, Const.ZERO, radiusSteps.Length - 1)];
        }
    }

    #region R_Unity
    private void Awake()
    {
        LoadConfigAtStartup();

        if (slider == null || radiusSteps == null || radiusSteps.Length == Const.ZERO)
            return;

        defaultIndex = Mathf.Clamp(defaultIndex, Const.ZERO, radiusSteps.Length - 1);

        slider.minValue = Const.ZERO;
        slider.maxValue = radiusSteps.Length - 1;
        slider.wholeNumbers = true;
        slider.SetValueWithoutNotify(defaultIndex);
        slider.onValueChanged.AddListener(OnSliderValueChanged);

        RefreshText();
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
    #endregion

    private void LoadConfigAtStartup()
    {
        if (config == null)
            return;

        IReadOnlyList<float> configuredSteps = config.TrackRadiusSteps;

        if (configuredSteps == null || configuredSteps.Count == Const.ZERO)
            return;

        radiusSteps = new float[configuredSteps.Count];

        for (int i = 0; i < configuredSteps.Count; i++)
        {
            radiusSteps[i] = configuredSteps[i];
        }

        defaultIndex = config.DefaultTrackRadiusIndex;
    }

    private void RefreshText()
    {
        if (valueText == null || radiusSteps == null || radiusSteps.Length == Const.ZERO)
            return;

        valueText.text = string.Format(format, CurrentRadius);
    }

    public void ResetToDefault()
    {
        if (slider == null)
            return;

        defaultIndex = Mathf.Clamp(defaultIndex, Const.ZERO, radiusSteps.Length - 1);

        slider.SetValueWithoutNotify(defaultIndex);
        RefreshText();
    }

    #region R_Callback
    private void OnSliderValueChanged(float value)
    {
        RefreshText();
    }
    #endregion
}