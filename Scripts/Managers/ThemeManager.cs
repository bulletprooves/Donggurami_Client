using UnityEngine;

[System.Serializable]
public class ThemeObjectEntry
{
    public WorldThemeType themeType;
    public GameObject themeObject;
}

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [SerializeField] private PortalLensManager portalLensManager;

    [Header("Themes")]
    [SerializeField] private WorldTheme[] themes;
    [SerializeField] private WorldThemeType defaultThemeType = WorldThemeType.Cosmos;
    [SerializeField] private ThemeObjectEntry[] themeObjects;

    private WorldTheme currentTheme;

    public WorldTheme CurrentTheme => currentTheme;

    #region R_Unity
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetTheme(defaultThemeType);
    }
    #endregion

    #region R_Public
    public void SetTheme(WorldThemeType themeType)
    {
        WorldTheme theme = FindTheme(themeType);

        if (theme == null)
        {
            GameManager.Instance?.LogWarning($"Theme not found: {themeType}");
            return;
        }

        currentTheme = theme;

        // 메서드로 빼놓음
        ApplyThemeObject(themeType); // 얘는 그냥 WorldTheme 스크립터블 사용 안함.
        ApplySkybox(theme);
        ApplyThemeToTracks(theme);
    }

    public void ApplyThemeToTrack(CircleTrack track)
    {
        if (track == null || currentTheme == null)
            return;

        track.ApplyTheme(currentTheme);

        portalLensManager.RefreshLensTarget(track);
    }
    #endregion

    private WorldTheme FindTheme(WorldThemeType themeType)
    {
        if (themes == null)
            return null;

        for (int i = 0; i < themes.Length; i++)
        {
            WorldTheme theme = themes[i];

            if (theme == null)
                continue;

            if (theme.themeType == themeType)
                return theme;
        }

        return null;
    }

    private void ApplySkybox(WorldTheme theme)
    {
        if (theme.skyboxMaterial == null)
            return;

        RenderSettings.skybox = theme.skyboxMaterial;
    }

    private void ApplyThemeToTracks(WorldTheme theme)
    {
        CircleTrack[] tracks = FindObjectsByType<CircleTrack>(FindObjectsSortMode.None);

        for (int i = 0; i < tracks.Length; i++)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            track.ApplyTheme(theme);
        }
    }

    private void ApplyThemeObject(WorldThemeType themeType)
    {
        if (themeObjects == null)
            return;

        for (int i = 0; i < themeObjects.Length; i++)
        {
            ThemeObjectEntry entry = themeObjects[i];

            if (entry == null || entry.themeObject == null)
                continue;

            bool isTargetTheme = entry.themeType == themeType;

            entry.themeObject.SetActive(isTargetTheme);
        }
    }
}