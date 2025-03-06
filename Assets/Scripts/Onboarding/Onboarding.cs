using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Onboarding
{
    public class Onboarding : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _steps;
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private Ease _enterEase = Ease.OutQuad;
        [SerializeField] private Ease _exitEase = Ease.InQuad;

        private int _currentIndex = 0;
        private CanvasGroup[] _canvasGroups;

        private void Awake()
        {
            _canvasGroups = new CanvasGroup[_steps.Count];
            for (int i = 0; i < _steps.Count; i++)
            {
                _canvasGroups[i] = _steps[i].GetComponent<CanvasGroup>();
                
                if (_canvasGroups[i] == null)
                {
                    _canvasGroups[i] = _steps[i].AddComponent<CanvasGroup>();
                }
            }

            if (PlayerPrefs.HasKey("Onboarding"))
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                ShowOnboarding();
            }
        }

        private void ShowOnboarding()
        {
            _currentIndex = 0;
            
            for (int i = 0; i < _steps.Count; i++)
            {
                _canvasGroups[i].alpha = 0;
                _steps[i].SetActive(false);
            }

            _steps[_currentIndex].SetActive(true);
            AnimateStepEnter(_currentIndex);
        }

        public void ShowNextStep()
        {
            AnimateStepExit(_currentIndex);

            _currentIndex++;
            
            if (_currentIndex < _steps.Count)
            {
                _steps[_currentIndex].SetActive(true);
                AnimateStepEnter(_currentIndex);
            }
            else
            {
                PlayerPrefs.SetInt("Onboarding", 1);
                gameObject.SetActive(false);
            }
        }

        private void AnimateStepEnter(int index)
        {
            _steps[index].transform.localScale = Vector3.zero;
            _canvasGroups[index].alpha = 0;

            Sequence enterSequence = DOTween.Sequence();
            enterSequence.Append(_steps[index].transform.DOScale(1f, _animationDuration).SetEase(_enterEase));
            enterSequence.Join(_canvasGroups[index].DOFade(1f, _animationDuration));
        }

        private void AnimateStepExit(int index)
        {
            Sequence exitSequence = DOTween.Sequence();
            exitSequence.Append(_steps[index].transform.DOScale(0f, _animationDuration).SetEase(_exitEase));
            exitSequence.Join(_canvasGroups[index].DOFade(0f, _animationDuration));
            exitSequence.OnComplete(() => _steps[index].SetActive(false));
        }
    }
}