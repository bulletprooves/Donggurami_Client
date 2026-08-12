using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateTrackPopupUI : BasePopup
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Config config;
    [SerializeField] private RadiusStepSliderUI radiusStepSlider;
    [SerializeField] private TMP_Dropdown instrumentDropdown;
    [SerializeField] private Button createButton;
    [SerializeField] private Button cancelButton;

    #region R_Override
    protected override void Awake()
    {
        base.Awake();

        if (createButton != null)
            createButton.onClick.AddListener(OnClickCreate);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Close);

        SetupInstrumentDropdown();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (createButton != null)
            createButton.onClick.RemoveListener(OnClickCreate);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Close);
    }

    public override void Open()
    {
        base.Open();

        if (radiusStepSlider != null)
            radiusStepSlider.ResetToDefault();
    }
    #endregion

    private void SetupInstrumentDropdown()
    {
        if (instrumentDropdown == null)
            return;

        instrumentDropdown.ClearOptions();

        string[] names = Enum.GetNames(typeof(InstrumentType));
        List<string> options = new List<string>(names);

        instrumentDropdown.AddOptions(options);
        instrumentDropdown.value = Const.ZERO;
        instrumentDropdown.RefreshShownValue();
    }

    #region R_Events or Callbacks
    private void OnClickCreate()
    {
        if (rhythmManager == null)
        {
            GameManager.Instance?.LogWarning($"{name}: RhythmManager is Not Assigned");
            return;
        }

        if (rhythmManager.IsStopped == false)
        {
            GameManager.Instance?.LogWarning("CircleTrack can only be created while stopped.");
            Close();
            return;
        }

        InstrumentType instrumentType = InstrumentType.GrandPiano;

        if (instrumentDropdown != null)
            instrumentType = (InstrumentType)instrumentDropdown.value;

        float radius = config != null ? config.DefaultTrackRadius : 8f;

        if (radiusStepSlider != null)
            radius = radiusStepSlider.CurrentRadius;

        CircleTrack createdTrack = rhythmManager.CreateTrack(
            radius,
            instrumentType
        );

        if (createdTrack != null)
            GameManager.Instance?.Log($"CircleTrack Created / Radius: {radius}");

        Close();
    }
    #endregion
}