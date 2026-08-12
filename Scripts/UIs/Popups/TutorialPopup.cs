using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupUI : BasePopup
{
    [Header("References")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject nextButtonRoot;
    [SerializeField] private GameObject doneButtonRoot;
    [SerializeField] private TextMeshProUGUI previousText;
    [SerializeField] private TextMeshProUGUI nextText;
    [SerializeField] private TextMeshProUGUI pageText;
    [SerializeField] private GameObject[] pagePanels;

    [Header("Settings")]
    [SerializeField] private bool openOnStart = true;

    [Header("Dim Overlay")]
    [SerializeField] private FocusOverlayUI focusOverlayUI;
    [SerializeField] private RectTransform[] highlightTargets;

    // Private Area
    private int _currentPageIndex;

    #region R_Override
    protected override void Awake()
    {
        base.Awake();

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        _currentPageIndex = Const.ZERO;
    }

    private void Start()
    {
        if (openOnStart)
            Open();

        RefreshPage();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (previousButton != null)
            previousButton.onClick.RemoveListener(PreviousPage);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);
    }

    protected override void OnOpened()
    {
        base.OnOpened();

        _currentPageIndex = Const.ZERO;

        RefreshPage();
    }

    protected override void OnClosed()
    {
        base.OnClosed();

        if (focusOverlayUI != null)
            focusOverlayUI.Hide();
    }
    #endregion

    #region R_Public
    public void NextPage()
    {
        if (pagePanels == null || pagePanels.Length == Const.ZERO)
            return;

        _currentPageIndex++;

        if (_currentPageIndex >= pagePanels.Length)
            _currentPageIndex = pagePanels.Length - 1;

        RefreshPage();
    }

    public void PreviousPage()
    {
        if (pagePanels == null || pagePanels.Length == Const.ZERO)
            return;

        _currentPageIndex--;

        if (_currentPageIndex < Const.ZERO)
            _currentPageIndex = Const.ZERO;

        RefreshPage();
    }
    #endregion

    private void RefreshPage()
    {
        if (pagePanels != null)
        {
            for (int i = 0; i < pagePanels.Length; i++)
            {
                if (pagePanels[i] == null)
                    continue;

                pagePanels[i].SetActive(i == _currentPageIndex);
            }
        }

        if (pageText != null)
        {
            int totalPageCount = (pagePanels != null) ? pagePanels.Length : Const.ZERO;

            pageText.text = $"Page {_currentPageIndex + 1}/{totalPageCount}";
        }

        if (previousButton != null)
        {
            bool isPrevAble = _currentPageIndex > Const.ZERO;
            previousButton.interactable = isPrevAble;
            previousText.color = isPrevAble ? Color.white : Color.gray;
        }

        if (nextButton != null && pagePanels != null)
        {
            bool isNextAble = _currentPageIndex < pagePanels.Length - 1;
            nextButton.interactable = isNextAble;
            nextText.color = isNextAble ? Color.white : Color.gray;

            // 튜토리얼 다 봤으면 Next 가려버려~
            nextButtonRoot.SetActive(isNextAble);
            doneButtonRoot.SetActive(!isNextAble);
        }

        if (focusOverlayUI != null && highlightTargets != null)
        {
            if (_currentPageIndex >= Const.ZERO && _currentPageIndex < highlightTargets.Length)
                focusOverlayUI.Show(highlightTargets[_currentPageIndex]);
            else
                focusOverlayUI.Hide();
        }
    }
}