using System;
using System.Globalization;
using Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    public class EntryPlane : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptitonText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _openButton;

        public event Action<EntryPlane> DataDeleted;
        public event Action<EntryPlane> DataOpened;

        public bool IsActive { get; private set; }
        public AssetData AssetData { get; private set; }

        private void OnEnable()
        {
            _deleteButton.onClick.AddListener(OnDeleteClicked);
            _openButton.onClick.AddListener(OnOpenClicked);
        }

        private void OnDisable()
        {
            _deleteButton.onClick.RemoveListener(OnDeleteClicked);
            _openButton.onClick.RemoveListener(OnOpenClicked);
        }

        public void Enable(AssetData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            IsActive = true;
            gameObject.SetActive(true);

            AssetData = data;
            _nameText.text = AssetData.Name;
            _descriptitonText.text = AssetData.Description;
            _costText.text = AssetData.Value.ToString();
        }

        public void Disable()
        {
            IsActive = false;
            gameObject.SetActive(false);

            _nameText.text = string.Empty;
            _descriptitonText.text = string.Empty;
            _costText.text = string.Empty;
            AssetData = null;
        }

        private void OnDeleteClicked()
        {
            DataDeleted?.Invoke(this);
        }

        private void OnOpenClicked()
        {
            DataOpened?.Invoke(this);
        }
    }
}