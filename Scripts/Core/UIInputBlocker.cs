using UnityEngine;

public class UIInputBlocker : MonoBehaviour
{
    // Private Area
    private int _blockCount;

    // Properties
    public static UIInputBlocker Instance { get; private set; } // 싱글톤 인스턴스
    public bool IsBlocked => _blockCount > Const.ZERO;

    #region R_Unity
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 근데 이짓거리 나중에 둘중 뭘 삭제할지 고민해야 할지도?
            Destroy(gameObject);
            Debug.LogWarning("Multiple instances of UIInputBlocker detected. Destroying duplicate.");
            return;
        }

        Instance = this;
    }
    #endregion

    #region R_Public
    public void AddBlock()
    {
        _blockCount++;
    }

    public void RemoveBlock()
    {
        _blockCount--;

        if (_blockCount < Const.ZERO)
            _blockCount = Const.ZERO;
    }
    #endregion
}