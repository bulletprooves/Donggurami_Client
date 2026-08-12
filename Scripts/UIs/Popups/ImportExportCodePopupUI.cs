using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImportExportCodePopupUI : BasePopup
{
    private enum CodePopupMode
    {
        Import,
        Export
    }

    [Header("References")]
    [SerializeField] private MusicSaveManager musicSaveManager;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button confirmButton;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TMP_InputField codeInputField;

    [Header("Texts")]
    [SerializeField] private string importTitle = "Import Code";
    [SerializeField] private string exportTitle = "Export Code";
    [SerializeField] private string importButtonLabel = "Import";
    [SerializeField] private string exportButtonLabel = "Copy";

    private CodePopupMode currentMode;

    #region R_Override
    protected override void Awake()
    {
        base.Awake();

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnClickConfirmButton);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnClickConfirmButton);
    }
    #endregion

    #region R_Public
    public void OpenImportMode()
    {
        currentMode = CodePopupMode.Import;

        if (titleText != null)
            titleText.text = importTitle;

        if (confirmButtonText != null)
            confirmButtonText.text = importButtonLabel;

        if (codeInputField != null)
        {
            codeInputField.text = string.Empty;
            codeInputField.interactable = true;
        }

        Open();
    }

    public void OpenExportMode()
    {
        currentMode = CodePopupMode.Export;

        if (titleText != null)
            titleText.text = exportTitle;

        if (confirmButtonText != null)
            confirmButtonText.text = exportButtonLabel;

        if (codeInputField != null)
        {
            string code = musicSaveManager != null
                ? musicSaveManager.ExportCode()
                : string.Empty;

            codeInputField.text = code;
            codeInputField.interactable = true;
        }

        Open();
    }
    #endregion

    #region R_Private
    private void OnClickConfirmButton()
    {
        switch (currentMode)
        {
            case CodePopupMode.Import:
                ImportCode();
                break;

            case CodePopupMode.Export:
                CopyCode();
                break;
        }
    }

    private void ImportCode()
    {
        if (musicSaveManager == null || codeInputField == null)
            return;

        string code = codeInputField.text;

        bool result = musicSaveManager.ImportCode(code);

        if (!result)
        {
            GameManager.Instance?.LogWarning("Import failed.");
            return;
        }

        GameManager.Instance?.Log("Import success.");

        Close();
    }

    private void CopyCode()
    {
        if (codeInputField == null)
            return;

        if (string.IsNullOrWhiteSpace(codeInputField.text))
            return;

        GUIUtility.systemCopyBuffer = codeInputField.text;

        GameManager.Instance?.Log("Export code copied.");
    }
    #endregion
}