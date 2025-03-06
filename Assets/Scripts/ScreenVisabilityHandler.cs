using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenVisabilityHandler : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void DisableScreen()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }

    public void EnableScreen()
    {
        _canvasGroup.interactable = true;
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    public void SetTransperent()
    {
        _canvasGroup.interactable = false;
      //  _canvasGroup.alpha = 0.035f;
        _canvasGroup.blocksRaycasts = false;
    }
}