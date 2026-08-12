using System.Collections;
using UnityEngine;

public abstract class BasePopup : MonoBehaviour
{
    [Header("Base Popup")]
    [SerializeField] protected GameObject popupRoot;
    [SerializeField] private bool blockInput = true;
    [SerializeField] private bool closeOnAwake = true;

    [Header("Open Animation")]
    [SerializeField] private bool useOpenAnimation = true;
    [SerializeField] private float openDuration = 0.15f;
    [SerializeField] private Vector3 closedScale = new Vector3(0.85f, 0.85f, 1f);
    [SerializeField] private Vector3 overshootScale = new Vector3(1.08f, 1.08f, 1f);

    // Private Area
    private bool _isOpened;
    private bool _isInputBlocked;
    private Coroutine _openAnimCoroutine;
    private Vector3 _openedScale = Vector3.one;

    // Properties
    public bool IsOpened => _isOpened;

    #region R_Unity
    protected virtual void Awake()
    {
        if (closeOnAwake)
            CloseImmediate();
    }

    protected virtual void OnDestroy()
    {
        ReleaseInputBlock();
    }
    #endregion

    #region R_Public
    public virtual void Open()
    {
        if (_isOpened)
            return;

        _isOpened = true;

        if (popupRoot != null)
            popupRoot.SetActive(true);

        // 팝업 미만 잡 입력 방지
        if (blockInput && !_isInputBlocked)
        {
            UIInputBlocker.Instance?.AddBlock();
            _isInputBlocked = true;
        }

        // 열려있을 때 애니메이션 겹침 방지
        if (useOpenAnimation && popupRoot != null)
        {
            StopOpenAnimation();
            _openAnimCoroutine = StartCoroutine(OpenAnimationCoroutine());
        }

        OnOpened();
    }

    public virtual void Close()
    {
        if (!_isOpened)
            return;

        ReleaseInputBlock();

        _isOpened = false;

        StopOpenAnimation();

        if (popupRoot != null)
        {
            popupRoot.transform.localScale = _openedScale;
            popupRoot.SetActive(false);
        }

        OnClosed();
    }

    public void CloseImmediate()
    {
        // 바로 닫을 때 고려할 것, (기본적으로 위 Close 함수 따라가기)
        ReleaseInputBlock();

        _isOpened = false;

        StopOpenAnimation();

        if (popupRoot != null)
        {
            popupRoot.transform.localScale = _openedScale;
            popupRoot.SetActive(false);
        }

        OnClosed();
    }
    #endregion

    #region R_Protected
    // 외부에서 열고 닫을 때 고려할 거 대비
    protected virtual void OnOpened()
    {
    }
    protected virtual void OnClosed()
    {
    }

    protected void ReleaseInputBlock()
    {
        if (!_isInputBlocked)
            return;

        UIInputBlocker.Instance?.RemoveBlock();
        _isInputBlocked = false;
    }
    #endregion

    private void StopOpenAnimation()
    {
        if (_openAnimCoroutine == null)
            return;

        // 코루틴 종료 전에 popupRoot의 스케일을 openedScale로 설정해야 함.
        if (popupRoot != null)
            popupRoot.transform.localScale = _openedScale;

        // 코루틴 종료 및 비워주기
        StopCoroutine(_openAnimCoroutine);
        _openAnimCoroutine = null;
    }

    private IEnumerator OpenAnimationCoroutine()
    {
        Transform target = popupRoot.transform;
        target.localScale = closedScale;
        float elapsed = Const.ZEROF;

        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = openDuration > 0f ? Mathf.Clamp01(elapsed / openDuration) : 1f;

            Vector3 scale;
            // '띠요옹~' 펀치 애니 효과가 두번있음
            if (t < 0.625f)
            {
                float firstT = t / 0.625f;
                firstT = 1f - Mathf.Pow(1f - firstT, 3f);

                scale = Vector3.LerpUnclamped(closedScale, overshootScale, firstT);
            }
            else
            {
                float secondT = (t - 0.625f) / 0.375f;
                secondT = 1f - Mathf.Pow(1f - secondT, 3f);

                scale = Vector3.LerpUnclamped(overshootScale, _openedScale, secondT);
            }
            target.localScale = scale;

            yield return null;
        }

        target.localScale = _openedScale;
        _openAnimCoroutine = null;
    }
}