using UnityEngine;

public class NoteView : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Transform body;
    [SerializeField] private GameObject sharpIcon;
    [SerializeField] private GameObject flatIcon;
    [SerializeField] private GameObject partHeadRoot;
    [SerializeField] private GameObject partNeck;
    [SerializeField] private GameObject partBodyRoot;

    [Header("Icon Position")]
    [SerializeField] private float iconXOffsetMultiplier = 1.2f;
    [SerializeField] private float iconYOffsetMultiplier = 0.5f;

    [Header("Theme")]
    [SerializeField] private SpriteRenderer NoteSpriteRenderer;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private SpriteRenderer headSpriteRenderer;
    [SerializeField] private SpriteRenderer bttmSpriteRenderer;

    public void SetSize(float width, float outwardLength)
    {
        if (body != null)
            body.localScale = new Vector3(width, outwardLength, Const.IDENTITY);

        Vector3 iconLocalPos = new Vector3(width * iconXOffsetMultiplier, outwardLength * iconYOffsetMultiplier, Const.ZEROF);

        if (sharpIcon != null)
            sharpIcon.transform.localPosition = iconLocalPos;

        if (flatIcon != null)
            flatIcon.transform.localPosition = iconLocalPos;

        if ((partHeadRoot != null) && (partBodyRoot != null) && (partNeck != null))
        {
            partHeadRoot.transform.localPosition = iconLocalPos;
            partBodyRoot.transform.localPosition = Vector2.one;
        }
        else
        {
            partHeadRoot.SetActive(false);
            partBodyRoot.SetActive(false);
            partNeck.SetActive(false);
        }
    }

    public void SetAccidental(NoteAccidental accidental)
    {
        if (sharpIcon != null)
            sharpIcon.SetActive(accidental == NoteAccidental.Sharp);

        if (flatIcon != null)
            flatIcon.SetActive(accidental == NoteAccidental.Flat);
    }

    public void ApplyTheme(WorldTheme theme)
    {
        if (theme == null)
            return;

        if (theme.isSingleNoteSprite)
        {
            if (NoteSpriteRenderer != null && theme.noteSprite != null)
                NoteSpriteRenderer.sprite = theme.noteSprite;
        }
        else
        {
            //if (bodySpriteRenderer != null && theme.noteBodySprite != null)
            //    bodySpriteRenderer.sprite = theme.noteBodySprite;
            //if (headSpriteRenderer != null && theme.noteHeadSprite != null)
            //    headSpriteRenderer.sprite = theme.noteHeadSprite;
            //if (bttmSpriteRenderer != null && theme.noteBottomSprite != null)
            //    bttmSpriteRenderer.sprite = theme.noteBottomSprite;
            bodySpriteRenderer.sprite = (bodySpriteRenderer != null && theme.noteBodySprite != null) ? theme.noteBodySprite : null;
            headSpriteRenderer.sprite = (headSpriteRenderer != null && theme.noteHeadSprite != null) ? theme.noteHeadSprite : null;
            bttmSpriteRenderer.sprite = (bttmSpriteRenderer != null && theme.noteBottomSprite != null) ? theme.noteBottomSprite : null;
        }

        NoteSpriteRenderer.gameObject.SetActive(theme.isSingleNoteSprite);
        bodySpriteRenderer.gameObject.SetActive(!theme.isSingleNoteSprite);
        headSpriteRenderer.gameObject.SetActive(!theme.isSingleNoteSprite);
        bttmSpriteRenderer.gameObject.SetActive(!theme.isSingleNoteSprite);

        // NRE 뜸
        //bodySpriteRenderer.sprite = (bodySpriteRenderer != null && theme.noteBodySprite != null) ? theme.noteBodySprite : null;
        //headSpriteRenderer.sprite = (headSpriteRenderer != null && theme.noteHeadSprite != null) ? theme.noteHeadSprite : null;
        //bottomSpriteRenderer.sprite = (bottomSpriteRenderer != null && theme.noteBottomSprite != null) ? theme.noteBottomSprite : null;

    }
}