using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPopupUI : BasePopup
{
    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI[] labelTexts;

    [Header("Labels")]
    [SerializeField] private string title = "Menu";
    [SerializeField] private string[] labels;

    public void OnClick_OpenURL_str(string url)
    {
        // 현재 인스펙터에서 사용 중인 url: 유튜브, 링크드인, 내 웹사이트   
        Application.OpenURL(url);
    }

    #region R_Override
    protected override void Awake()
    {
        base.Awake();

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        RefreshTexts();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    protected override void OnOpened()
    {
        base.OnOpened();

        RefreshTexts();
    }
    #endregion

    private void RefreshTexts()
    {
        if (titleText != null)
            titleText.text = title;

        if (labelTexts == null || labels == null)
            return;

        labels = new string[3]
    {
            "Thx for playing my game!\n" +
            "This game is currently in beta testing.\n" +
            "If you find any bugs or have any feedback or suggestions,\n" +
            "plz feel free to contact me at the email address below.\n" +
            "Thank you!\n" +
            "dongun1115@gmail.com", // 여기까지 첫번째 설명 라벨
        "All UI vector Icons designed by UIcons from Flaticon", // 여기까지 두번째
        "Programmed by David Gam (@bulletprooves, YouTube)" // 여기까지 세번째

    }
        ;

        int count = Mathf.Min(labelTexts.Length, labels.Length);

        for (int i = 0; i < count; i++)
        {
            if (labelTexts[i] == null)
                continue;

            labelTexts[i].text = labels[i];
        }
    }
}