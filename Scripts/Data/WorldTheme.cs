using UnityEngine;

public enum WorldThemeType
{
    Cosmos,
    Pirate,
    Neon,
    Block,
    Circus,
    Under,
}

[CreateAssetMenu(menuName = "Donggurami/World Theme")]
public class WorldTheme : ScriptableObject
{
    public WorldThemeType themeType;

    [Header("Skybox")]
    public Material skyboxMaterial;

    [Header("Note")]
    public bool isSingleNoteSprite; // true: noteSprite만 사용, false: noteBodySprite, noteHeadSprite, noteBottomSprite 사용
    public Sprite noteSprite;       // 하나짜리
    public Sprite noteBodySprite;   // ---\
    public Sprite noteHeadSprite;   // ---->  이거 세개가 한 세트
    public Sprite noteBottomSprite; // ---/

    [Header("Circle Track")]
    public GameObject centerPrefab;

    [Header("Arrow")]
    public GameObject arrowPrefab;
}