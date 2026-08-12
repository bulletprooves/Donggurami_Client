using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldThemeDropdownUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private ThemeManager themeManager;

    private void Awake()
    {
        SetupDropdown();

        if (dropdown != null)
            dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void SetupDropdown()
    {
        if (dropdown == null)
            return;

        dropdown.ClearOptions();

        string[] names = Enum.GetNames(typeof(WorldThemeType));
        dropdown.AddOptions(new List<string>(names));

        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private void OnValueChanged(int index)
    {
        if (themeManager == null)
            return;

        WorldThemeType themeType = (WorldThemeType)index;

        themeManager.SetTheme(themeType);
    }
}