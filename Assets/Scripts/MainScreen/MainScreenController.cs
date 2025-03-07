using System;
using System.Collections.Generic;
using System.Linq;
using AddAsset;
using Assets;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using SaveSystem;

namespace MainScreen
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class MainScreenController : MonoBehaviour
    {
        [Header("Colors")] [SerializeField] private Color _activeButtonColor = Color.white;
        [SerializeField] private Color _unactiveButtonColor = Color.gray;

        [Header("Asset Type Holders")] [SerializeField]
        private List<AssetTypeHolder> _assetTypeHolders;

        [Header("Buttons")] [SerializeField] private Button _personalAssetsButton;
        [SerializeField] private Button _businessAssetsButton;
        [SerializeField] private Button _addAssetButton;

        [Header("Panels")] [SerializeField] private GameObject _personalAssets;
        [SerializeField] private GameObject _businessAssets;
        [SerializeField] private GameObject _emptyPanel;
        [SerializeField] private GameObject _nonSelectedPanel;
        [SerializeField] private GameObject _entryHolder;
        [SerializeField] private AddAssetScreen _addAssetScreen;

        [Header("Entry Planes")] [SerializeField]
        private List<EntryPlane> _entryPlanes;

        [Header("Animation Settings")] [SerializeField]
        private float _buttonScaleDuration = 0.2f;

        [SerializeField] private float _panelFadeDuration = 0.3f;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private List<AssetData> _assetDatas = new List<AssetData>();
        private AssetType _currentlySelectedType = AssetType.None;
        private DataSaver _dataSaver;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _dataSaver = new DataSaver();

            LoadSavedData();
        }

        private void OnEnable()
        {
            if (_personalAssetsButton != null)
                _personalAssetsButton.onClick.AddListener(OnPersonalButtonClicked);

            if (_businessAssetsButton != null)
                _businessAssetsButton.onClick.AddListener(OnBusinessButtonClicked);

            _addAssetButton.onClick.AddListener(OnAddAssetClicked);

            _addAssetScreen.OnBackButtonPressed += Enable;
            _addAssetScreen.OnAssetSaved += SaveAsset;
            _addAssetScreen.OnAssetUpdated += UpdateAsset;

            if (_assetTypeHolders != null)
            {
                foreach (var assetTypeHolder in _assetTypeHolders)
                {
                    if (assetTypeHolder != null)
                    {
                        assetTypeHolder.Clicked += ShowEntries;
                        assetTypeHolder.Clicked += type => _currentlySelectedType = type;
                    }
                }
            }

            foreach (var entryPlane in _entryPlanes)
            {
                if (entryPlane != null)
                {
                    entryPlane.DataDeleted += DeleteAsset;
                    entryPlane.DataOpened += EditAsset;
                }
            }
        }

        private void OnDisable()
        {
            if (_personalAssetsButton != null)
                _personalAssetsButton.onClick.RemoveListener(OnPersonalButtonClicked);

            if (_businessAssetsButton != null)
                _businessAssetsButton.onClick.RemoveListener(OnBusinessButtonClicked);

            _addAssetButton.onClick.RemoveListener(OnAddAssetClicked);

            _addAssetScreen.OnBackButtonPressed -= Enable;
            _addAssetScreen.OnAssetSaved -= SaveAsset;
            _addAssetScreen.OnAssetUpdated -= UpdateAsset;

            if (_assetTypeHolders != null)
            {
                foreach (var assetTypeHolder in _assetTypeHolders)
                {
                    if (assetTypeHolder != null)
                    {
                        assetTypeHolder.Clicked -= ShowEntries;
                        assetTypeHolder.Clicked -= type => _currentlySelectedType = type;
                    }
                }
            }

            foreach (var entryPlane in _entryPlanes)
            {
                if (entryPlane != null)
                {
                    entryPlane.DataDeleted -= DeleteAsset;
                    entryPlane.DataOpened -= EditAsset;
                }
            }
        }

        public void Enable()
        {
            _screenVisabilityHandler.EnableScreen();
            VerifyInputedAssets();

            if (_currentlySelectedType != AssetType.None)
            {
                ShowEntries(_currentlySelectedType);
            }
        }

        public void Disable()
        {
            _screenVisabilityHandler.DisableScreen();
        }

        private void SaveAsset(AssetData newAsset)
        {
            _assetDatas.Add(newAsset);
            SaveDataToDisk();
            VerifyInputedAssets();
            Enable();
        }

        private void UpdateAsset(AssetData updatedAsset)
        {
            int index = _assetDatas.IndexOf(_addAssetScreen.GetCurrentAssetData());
            if (index >= 0)
            {
                _assetDatas[index] = updatedAsset;
                SaveDataToDisk();
                VerifyInputedAssets();

                if (_currentlySelectedType != AssetType.None)
                {
                    ShowEntries(_currentlySelectedType);
                }
            }

            Enable();
        }

        private void EditAsset(EntryPlane entryPlane)
        {
            if (entryPlane?.AssetData == null)
                return;

            Disable();
            _addAssetScreen.EnableForViewAsset(entryPlane.AssetData);
        }

        private void DeleteAsset(EntryPlane entryPlane)
        {
            if (entryPlane?.AssetData == null)
                return;

            AssetType deletedAssetType = entryPlane.AssetData.Type;

            _assetDatas.Remove(entryPlane.AssetData);
            SaveDataToDisk();

            entryPlane.Disable();

            VerifyInputedAssets();

            bool anyRemainingAssetsOfSameType = _assetDatas.Any(d => d.Type == deletedAssetType);

            if (_assetDatas.Count > 0)
            {
                if (deletedAssetType == _currentlySelectedType)
                {
                    if (anyRemainingAssetsOfSameType)
                    {
                        ShowEntries(deletedAssetType);
                        ToggleNotSelectedPlane(false);
                    }
                    else
                    {
                        _entryHolder.SetActive(false);
                        ToggleNotSelectedPlane(true);
                    }
                }

                ToggleEmptyPlane(false);
            }
            else
            {
                _entryHolder.SetActive(false);
                ToggleNotSelectedPlane(false);
                ToggleEmptyPlane(true);
                _currentlySelectedType = AssetType.None;
            }
        }

        private void OnPersonalButtonClicked()
        {
            AnimateButtonSelection(_personalAssetsButton, _businessAssetsButton);

            TransitionPanels(_personalAssets, _businessAssets);
        }

        private void OnBusinessButtonClicked()
        {
            AnimateButtonSelection(_businessAssetsButton, _personalAssetsButton);

            TransitionPanels(_businessAssets, _personalAssets);
        }

        private void AnimateButtonSelection(Button activeButton, Button inactiveButton)
        {
            activeButton.image.DOColor(_activeButtonColor, _buttonScaleDuration);
            inactiveButton.image.DOColor(_unactiveButtonColor, _buttonScaleDuration);

            activeButton.transform.DOScale(1.1f, _buttonScaleDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => activeButton.transform.DOScale(1f, _buttonScaleDuration / 2));

            inactiveButton.transform.DOScale(1f, _buttonScaleDuration);
        }

        private void TransitionPanels(GameObject activePanel, GameObject inactivePanel)
        {
            inactivePanel.transform.DOScale(0.9f, _panelFadeDuration);
            inactivePanel.GetComponent<CanvasGroup>().DOFade(0, _panelFadeDuration)
                .OnComplete(() => inactivePanel.SetActive(false));

            activePanel.SetActive(true);
            activePanel.transform.DOScale(1f, _panelFadeDuration).From(0.9f);

            CanvasGroup activeCanvasGroup = activePanel.GetComponent<CanvasGroup>();
            if (activeCanvasGroup == null)
                activeCanvasGroup = activePanel.AddComponent<CanvasGroup>();

            activeCanvasGroup.DOFade(1, _panelFadeDuration).From(0);
        }

        private void VerifyInputedAssets()
        {
            if (_assetDatas == null || _assetDatas.Count <= 0)
            {
                DiactivateAllHolders();
                ToggleEmptyPlane(true);
                ToggleNotSelectedPlane(false);
                return;
            }

            HashSet<AssetType> inputedTypes = new HashSet<AssetType>(
                _assetDatas.Select(asset => asset.Type).Distinct()
            );

            if (_assetTypeHolders != null)
            {
                foreach (var assetTypeHolder in _assetTypeHolders)
                {
                    if (assetTypeHolder != null && inputedTypes.Contains(assetTypeHolder.Type))
                        assetTypeHolder.SetSelected();
                    else
                        assetTypeHolder.SetUnselected();
                }
            }

            ToggleEmptyPlane(false);
            ToggleNotSelectedPlane(true);
        }

        private void ShowEntries(AssetType type)
        {
            var typeEntries = _assetDatas
                .Where(d => d.Type == type)
                .ToList();

            ToggleEmptyPlane();

            if (!typeEntries.Any())
            {
                _entryHolder.SetActive(false);
                ToggleNotSelectedPlane(_assetDatas.Count > 0);
                return;
            }

            foreach (var entryPlane in _entryPlanes)
            {
                entryPlane.Disable();
            }

            _entryHolder.SetActive(true);

            for (int i = 0; i < typeEntries.Count && i < _entryPlanes.Count; i++)
            {
                var availablePlane = _entryPlanes[i];
                availablePlane.Enable(typeEntries[i]);
                ToggleNotSelectedPlane(false);
            }
        }

        private void ToggleEmptyPlane(bool forceState = false)
        {
            if (_emptyPanel != null)
            {
                bool shouldShow = forceState || _assetDatas.Count <= 0;

                CanvasGroup canvasGroup = _emptyPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = _emptyPanel.AddComponent<CanvasGroup>();

                _emptyPanel.SetActive(true);

                canvasGroup.DOFade(shouldShow ? 1f : 0f, _panelFadeDuration)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => { _emptyPanel.SetActive(shouldShow); });
            }
        }

        private void ToggleNotSelectedPlane(bool status)
        {
            if (_nonSelectedPanel != null)
            {
                CanvasGroup canvasGroup = _nonSelectedPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = _nonSelectedPanel.AddComponent<CanvasGroup>();

                _nonSelectedPanel.SetActive(true);

                canvasGroup.DOFade(status ? 1f : 0f, _panelFadeDuration)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => { _nonSelectedPanel.SetActive(status); });
            }
        }

        private void DisableAllEntries()
        {
            foreach (var entryPlane in _entryPlanes)
            {
                entryPlane?.Disable();
            }

            _entryHolder.transform.DOScale(0, 0.2f)
                .OnComplete(() =>
                {
                    _entryHolder.SetActive(false);
                    _entryHolder.transform.localScale = Vector3.one;
                });
        }

        private void DiactivateAllHolders()
        {
            foreach (var assetTypeHolder in _assetTypeHolders)
            {
                assetTypeHolder?.SetUnselected();
            }
        }

        private void OnAddAssetClicked()
        {
            Disable();
            _addAssetScreen.EnableForNewAsset();
        }

        #region Data Persistence

        private void LoadSavedData()
        {
            var loadedData = _dataSaver.LoadData();
            if (loadedData != null)
            {
                _assetDatas = loadedData;
            }
            else
            {
                _assetDatas = new List<AssetData>();
            }

            DiactivateAllHolders();
            OnPersonalButtonClicked();
            DisableAllEntries();
            VerifyInputedAssets();
            ToggleEmptyPlane();
            ToggleNotSelectedPlane(_assetDatas.Count > 0);
        }

        private void SaveDataToDisk()
        {
            try
            {
                _dataSaver.SaveData(_assetDatas);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        #endregion
    }
}