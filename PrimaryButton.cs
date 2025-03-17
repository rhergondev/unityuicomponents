using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
    public enum SimpleButtonState
    {
        Default,
        Active,
    }

    [UxmlElement("primary-button")]
    public partial class PrimaryButton : VisualElement
    {
        private const string ContainerClass = "primary-button-container";
        private const string ButtonClass = "primary-button-main";
        private const string ShadowClass = "primary-button-shadow";
        private const string ActiveButtonClass = "primary-button-active";
        private const string ActiveShadowClass = "primary-button-shadow-active";

        private readonly VisualElement _mainContainer;
        private readonly Button _button;
        private readonly VisualElement _shadow;
        
        private LocalizedString _localizedText;
        private string _tableReference;
        private string _entryReference;

        private SimpleButtonState _state;

        public event Action<SimpleButtonState> OnStateChanged;
        
        [UxmlAttribute]
        public String Text
        {
            get => _button.text;
            set => _button.text = value;
        }

        [UxmlAttribute]
        public string TableReference
        {
            get => _tableReference;
            set
            {
                _tableReference = value;
                UpdateLocalizedString();
            }
        }
        
        [UxmlAttribute]
        public string EntryReference
        {
            get => _entryReference;
            set
            {
                _entryReference = value;
                UpdateLocalizedString();
            }
        }

        [UxmlAttribute]
        public SimpleButtonState State
        {
            get => _state;
            set => SetState(value);
        }

        public PrimaryButton()
        {
            _mainContainer = new VisualElement();
            _button = new Button();
            _shadow = new VisualElement();
            _mainContainer.AddToClassList(ContainerClass);
            _button.AddToClassList(ButtonClass);
            _shadow.AddToClassList(ShadowClass);

            _mainContainer.Add(_shadow);
            _mainContainer.Add(_button);

            hierarchy.Add(_mainContainer);
            
            _button.RegisterCallback<GeometryChangedEvent>(OnButtonChangedGeometry);

            SetState(SimpleButtonState.Default);
        }

        private void OnButtonChangedGeometry(GeometryChangedEvent evt)
        {
            _shadow.style.width = _button.layout.width;
        }

        public void SetState(SimpleButtonState newState)
        {
            if (_state != newState)
            {
                _state = newState;
                UpdateButtonVisuals();
                OnStateChanged?.Invoke(_state);
            }
        }

        private void UpdateLocalizedString()
        {
            if (_localizedText != null)
            {
                _localizedText.StringChanged += OnLocalizedStringChanged;
            }

            if (!string.IsNullOrEmpty(_tableReference) && !string.IsNullOrEmpty(_entryReference))
            {
                _localizedText = new LocalizedString(_tableReference, _entryReference);
                _localizedText.StringChanged += OnLocalizedStringChanged;

                UpdateButtonText();
            }
        }

        private void OnLocalizedStringChanged(string value)
        {
            UpdateButtonText();
        }

        private void UpdateButtonText()
        {
            _button.text = _localizedText.GetLocalizedString();
        }

        private void UpdateButtonVisuals()
        {
            switch (_state)
            {
                case SimpleButtonState.Default:
                    if (_mainContainer.ClassListContains(ActiveButtonClass))
                    {
                        _mainContainer.RemoveFromClassList(ActiveButtonClass);
                    }
                    if (_shadow.ClassListContains(ActiveShadowClass))
                    {
                        _shadow.RemoveFromClassList(ActiveShadowClass);
                    }
                    break;
                case SimpleButtonState.Active:
                    _mainContainer.AddToClassList(ActiveButtonClass);
                    _shadow.AddToClassList(ActiveShadowClass);
                    break;
            }
        }
    }

}
