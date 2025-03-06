using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject _settingsCanvas;
    [SerializeField] private GameObject _privacyCanvas;
    [SerializeField] private GameObject _termsCanvas;
    [SerializeField] private GameObject _contactCanvas;
    [SerializeField] private GameObject _versionCanvas;
    [SerializeField] private TMP_Text _versionText;

    [Header("Animation Settings")] [SerializeField]
    private float _animationDuration = 0.3f;

    [SerializeField] private Ease _animationEase = Ease.OutQuad;

    private CanvasGroup _settingsCanvasGroup;
    private CanvasGroup _privacyCanvasGroup;
    private CanvasGroup _termsCanvasGroup;
    private CanvasGroup _contactCanvasGroup;
    private CanvasGroup _versionCanvasGroup;

    private string _version = "Application version:\n";

    private void Awake()
    {
        _settingsCanvasGroup = _settingsCanvas.GetComponent<CanvasGroup>();
        _privacyCanvasGroup = _privacyCanvas.GetComponent<CanvasGroup>();
        _termsCanvasGroup = _termsCanvas.GetComponent<CanvasGroup>();
        _contactCanvasGroup = _contactCanvas.GetComponent<CanvasGroup>();
        _versionCanvasGroup = _versionCanvas.GetComponent<CanvasGroup>();

        DeactivateAllCanvases();
        SetVersion();
        SetCanvasGroupState(_settingsCanvasGroup, false);
    }

    private void DeactivateAllCanvases()
    {
        SetCanvasGroupState(_privacyCanvasGroup, false);
        SetCanvasGroupState(_termsCanvasGroup, false);
        SetCanvasGroupState(_contactCanvasGroup, false);
        SetCanvasGroupState(_versionCanvasGroup, false);
    }

    private void SetCanvasGroupState(CanvasGroup canvasGroup, bool isActive)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isActive ? 1f : 0f;
            canvasGroup.interactable = isActive;
            canvasGroup.blocksRaycasts = isActive;
        }
    }

    private void AnimateCanvasGroup(CanvasGroup canvasGroup, bool show)
    {
        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);

            canvasGroup.DOFade(show ? 1f : 0f, _animationDuration)
                .SetEase(_animationEase)
                .OnUpdate(() =>
                {
                    canvasGroup.interactable = show && canvasGroup.alpha > 0.5f;
                    canvasGroup.blocksRaycasts = show && canvasGroup.alpha > 0.5f;
                })
                .OnComplete(() =>
                {
                    if (!show)
                    {
                        canvasGroup.gameObject.SetActive(false);
                    }
                });
        }
    }

    private void SetVersion()
    {
        _versionText.text = _version + Application.version;
    }

    public void ShowSettings()
    {
        DeactivateAllCanvases();
        AnimateCanvasGroup(_settingsCanvasGroup, true);
    }

    public void ShowPrivacy()
    {
        DeactivateAllCanvases();
        AnimateCanvasGroup(_privacyCanvasGroup, true);
    }

    public void ShowTerms()
    {
        DeactivateAllCanvases();
        AnimateCanvasGroup(_termsCanvasGroup, true);
    }

    public void ShowContact()
    {
        DeactivateAllCanvases();
        AnimateCanvasGroup(_contactCanvasGroup, true);
    }

    public void ShowVersion()
    {
        DeactivateAllCanvases();
        AnimateCanvasGroup(_versionCanvasGroup, true);
    }

    public void RateUs()
    {
#if UNITY_IOS
        Device.RequestStoreReview();
#endif
    }

    public void CloseSettings()
    {
        AnimateCanvasGroup(_settingsCanvasGroup, false);
    }

    public void ClosePrivacy()
    {
        AnimateCanvasGroup(_privacyCanvasGroup, false);
    }

    public void CloseTerms()
    {
        AnimateCanvasGroup(_termsCanvasGroup, false);
    }

    public void CloseContact()
    {
        AnimateCanvasGroup(_contactCanvasGroup, false);
    }

    public void CloseVersion()
    {
        AnimateCanvasGroup(_versionCanvasGroup, false);
    }
}