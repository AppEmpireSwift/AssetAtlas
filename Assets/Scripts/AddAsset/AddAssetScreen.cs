using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using MainScreen;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace AddAsset
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class AddAssetScreen : MonoBehaviour
    {
        [Header("Navigation Buttons")] [SerializeField]
        private Button _backButton;

        [SerializeField] private Button _personalAssetsButton;
        [SerializeField] private Button _businessAssetsButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _editButton;

        [Header("Colors")] [SerializeField] private Color _activeButtonColor = Color.white;
        [SerializeField] private Color _unactiveButtonColor = Color.gray;

        [Header("Asset Panels")] [SerializeField]
        private GameObject _personalAssets;

        [SerializeField] private GameObject _businessAssets;

        [Header("Input Fields")] [SerializeField]
        private TMP_InputField _nameInputField;

        [SerializeField] private TMP_InputField _descriptionInputField;
        [SerializeField] private TMP_InputField _costInputField;

        [Header("Asset Type Holders")] [SerializeField]
        private List<AssetTypeHolder> _assetTypeHolders;

        [Header("Animation Settings")] [SerializeField]
        private float _buttonScaleDuration = 0.2f;

        [SerializeField] private float _panelFadeDuration = 0.3f;

        public event Action OnBackButtonPressed;
        public event Action<AssetData> OnAssetSaved;
        public event Action<AssetData> OnAssetUpdated;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private AssetType _selectedAssetType;
        private AssetData _currentAssetData;
        private bool _isEditMode = false;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();

            ValidateReferences();

            SetupInputListeners();

            _saveButton.interactable = false;
            _editButton.interactable = false;
            _editButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            _screenVisabilityHandler.DisableScreen();
            ClearInputFields();
        }

        private void OnEnable()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(HandleBackButtonPressed);

            if (_personalAssetsButton != null)
                _personalAssetsButton.onClick.AddListener(OnPersonalButtonClicked);

            if (_businessAssetsButton != null)
                _businessAssetsButton.onClick.AddListener(OnBusinessButtonClicked);

            if (_saveButton != null)
                _saveButton.onClick.AddListener(HandleSaveButtonPressed);

            if (_editButton != null)
                _editButton.onClick.AddListener(HandleEditButtonPressed);

            if (_assetTypeHolders != null)
            {
                foreach (var holder in _assetTypeHolders)
                {
                    if (holder != null)
                        holder.Clicked += OnAssetTypeSelected;
                }
            }
        }

        private void OnDisable()
        {
            if (_backButton != null)
                _backButton.onClick.RemoveListener(HandleBackButtonPressed);

            if (_personalAssetsButton != null)
                _personalAssetsButton.onClick.RemoveListener(OnPersonalButtonClicked);

            if (_businessAssetsButton != null)
                _businessAssetsButton.onClick.RemoveListener(OnBusinessButtonClicked);

            if (_saveButton != null)
                _saveButton.onClick.RemoveListener(HandleSaveButtonPressed);

            if (_editButton != null)
                _editButton.onClick.RemoveListener(HandleEditButtonPressed);

            if (_assetTypeHolders != null)
            {
                foreach (var holder in _assetTypeHolders)
                {
                    if (holder != null)
                        holder.Clicked -= OnAssetTypeSelected;
                }
            }
        }

        private void ValidateReferences()
        {
            if (_backButton == null)
                Debug.LogError("Back Button is not assigned!");

            if (_personalAssetsButton == null || _businessAssetsButton == null)
                Debug.LogError("Personal or Business Assets Button is not assigned!");

            if (_nameInputField == null || _descriptionInputField == null || _costInputField == null)
                Debug.LogError("One or more input fields are not assigned!");

            if (_saveButton == null)
                Debug.LogError("Save Button is not assigned!");

            if (_editButton == null)
                Debug.LogError("Edit Button is not assigned!");

            if (_assetTypeHolders == null || _assetTypeHolders.Count == 0)
                Debug.LogError("Asset Type Holders are not assigned!");
        }

        private void SetupInputListeners()
        {
            if (_nameInputField != null)
                _nameInputField.onValueChanged.AddListener(_ => ValidateSaveButton());

            if (_descriptionInputField != null)
                _descriptionInputField.onValueChanged.AddListener(_ => ValidateSaveButton());

            if (_costInputField != null)
            {
                _costInputField.onValueChanged.AddListener(FormatCostInput);
                _costInputField.onValueChanged.AddListener(_ => ValidateSaveButton());
            }
        }

        private void FormatCostInput(string value)
        {
            string cleanedValue = new string(value.Where(c => char.IsDigit(c)).ToArray());

            if (string.IsNullOrEmpty(cleanedValue))
            {
                _costInputField.text = string.Empty;
                return;
            }

            _costInputField.text = $"${cleanedValue}";

            _costInputField.caretPosition = _costInputField.text.Length;
        }

        private void ValidateSaveButton()
        {
            bool isValid = !string.IsNullOrWhiteSpace(_nameInputField.text) &&
                           !string.IsNullOrWhiteSpace(_descriptionInputField.text) &&
                           !string.IsNullOrWhiteSpace(_costInputField.text.TrimStart('$')) &&
                           _selectedAssetType != AssetType.None;

            _saveButton.interactable = isValid;
        }

        private void HandleBackButtonPressed()
        {
            _backButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);

            _isEditMode = false;
            OnBackButtonPressed?.Invoke();
            Disable();
        }

        private void HandleSaveButtonPressed()
        {
            string costValue = _costInputField.text.TrimStart('$');
            int cost = int.Parse(costValue);

            if (_isEditMode && _currentAssetData != null)
            {
                var updatedAsset = new AssetData(
                    _selectedAssetType,
                    cost,
                    _nameInputField.text,
                    _descriptionInputField.text
                );

                OnAssetUpdated?.Invoke(updatedAsset);
            }
            else
            {
                var assetData = new AssetData(
                    _selectedAssetType,
                    cost,
                    _nameInputField.text,
                    _descriptionInputField.text
                );

                OnAssetSaved?.Invoke(assetData);
            }

            ClearInputFields();
            _isEditMode = false;
            _editButton.gameObject.SetActive(false);
            Disable();
        }

        private void HandleEditButtonPressed()
        {
            _editButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);

            SetEditMode(true);
        }

        private void SetEditMode(bool isEdit)
        {
            _isEditMode = isEdit;

            bool fieldsInteractable = isEdit;

            _nameInputField.interactable = fieldsInteractable;
            _descriptionInputField.interactable = fieldsInteractable;
            _costInputField.interactable = fieldsInteractable;

            foreach (var holder in _assetTypeHolders)
            {
                holder.SetInteractable(fieldsInteractable);
            }

            _saveButton.gameObject.SetActive(isEdit);

            if (isEdit)
            {
                _saveButton.transform.DOScale(1f, _buttonScaleDuration).From(0.9f);
                ValidateSaveButton();
            }
        }

        private void OnAssetTypeSelected(AssetType type)
        {
            _selectedAssetType = type;

            foreach (var holder in _assetTypeHolders)
            {
                if (holder.Type == type)
                {
                    holder.SetSelected();
                }
                else
                {
                    holder.SetUnselected();
                }
            }

            ValidateSaveButton();
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

        private void ClearInputFields()
        {
            if (_nameInputField != null)
                _nameInputField.text = string.Empty;

            if (_descriptionInputField != null)
                _descriptionInputField.text = string.Empty;

            if (_costInputField != null)
                _costInputField.text = string.Empty;

            if (_assetTypeHolders != null)
            {
                foreach (var holder in _assetTypeHolders)
                {
                    holder.SetUnselected();
                }
            }

            _selectedAssetType = AssetType.None;
            _saveButton.interactable = false;
            _currentAssetData = null;
            _isEditMode = false;
        }

        public void Enable()
        {
            _screenVisabilityHandler.EnableScreen();
            OnPersonalButtonClicked();

            _editButton.gameObject.SetActive(false);
            SetEditMode(true);
        }

        public void EnableForNewAsset()
        {
            Enable();
            _editButton.gameObject.SetActive(false);
            SetEditMode(true);
        }

        public void EnableForViewAsset(AssetData assetData)
        {
            Enable();
            LoadAssetData(assetData);

            _editButton.gameObject.SetActive(true);
            _editButton.interactable = true;
            SetEditMode(false);
        }

        public void Disable()
        {
            _screenVisabilityHandler.DisableScreen();
        }

        public AssetData GetCurrentAssetData()
        {
            return _currentAssetData;
        }

        private void LoadAssetData(AssetData assetData)
        {
            if (assetData == null)
                return;

            _currentAssetData = assetData;

            _nameInputField.text = assetData.Name;
            _descriptionInputField.text = assetData.Description;
            _costInputField.text = $"${assetData.Value}";

            _selectedAssetType = assetData.Type;

            foreach (var holder in _assetTypeHolders)
            {
                if (holder.Type == _selectedAssetType)
                {
                    holder.SetSelected();
                }
                else
                {
                    holder.SetUnselected();
                }
            }

            OnPersonalButtonClicked();
        }
    }
}