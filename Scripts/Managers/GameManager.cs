using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool isDebugLog = true;

    public bool IsDebugLog => isDebugLog;

    #region R_Unity
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region R_Public
    public void Log(object message)
    {
        if (!isDebugLog)
            return;

        Debug.Log(message);
    }

    public void LogWarning(object message)
    {
        if (!isDebugLog)
            return;

        Debug.LogWarning(message);
    }

    public void LogError(object message)
    {
        Debug.LogError(message);
    }
    #endregion
}