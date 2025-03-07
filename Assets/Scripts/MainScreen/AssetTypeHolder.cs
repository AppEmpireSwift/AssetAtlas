using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    public class AssetTypeHolder : MonoBehaviour
    {
        [SerializeField] private AssetType _type;
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _unselectedColor;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;

        public event Action<AssetType> Clicked;

        public bool IsSelected { get; private set; }
        public AssetType Type => _type;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        public void SetSelected()
        {
            _button.image.color = _selectedColor;
            _text.color = _selectedColor;
            IsSelected = true;
        }

        public void SetUnselected()
        {
            _button.image.color = _unselectedColor;
            _text.color = _unselectedColor;
            IsSelected = false;
        }

        private void OnButtonClicked()
        {
            Clicked?.Invoke(_type);
        }

        public void SetInteractable(bool fieldsInteractable)
        {
            if (fieldsInteractable)
            {
                SetSelected();
                return;
            }

            SetUnselected();
        }
    }

    public enum AssetType
    {
        RealEstate,
        Transport,
        Valuables,
        Electronics,
        Personal,
        Hobby,
        Furniture,
        OtherPersonal,
        FixedAssets,
        ComRealEstate,
        Vehicles,
        IntellectualProperty,
        Supplies,
        FinancialAssets,
        Contracts,
        OtherBusiness,
        None
    }
}