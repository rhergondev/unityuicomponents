using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{

    [UxmlElement("secondary-button")]
    public partial class SecondaryButton : VisualElement
    {
        private const string ContainerClass = "primary-button-container";
        private const string ButtonClass = "primary-button-main";
        private const string ActiveButtonClass = "primary-button-active";

        private readonly VisualElement _mainContainer;
        private readonly Button _button;
        private SimpleButtonState _state;
        
        private LocalizedString _localizedText;
        private string _tableReference;
        private string _entryReference;


        public event Action<SimpleButtonState> OnStateChanged;

        [UxmlAttribute]
        public SimpleButtonState State
        {
            get => _state;
            set => SetState(value);
        }

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

        public SecondaryButton()
        {
            _mainContainer = new VisualElement();
            _button = new Button();
            _mainContainer.AddToClassList(ContainerClass);
            _button.AddToClassList(ButtonClass);

            _mainContainer.Add(_button);

            _mainContainer.RegisterCallback<PointerDownEvent>((evt) => SetState(SimpleButtonState.Active));
            _mainContainer.RegisterCallback<PointerUpEvent>((evt) => SetState(SimpleButtonState.Default));

            hierarchy.Add(_mainContainer);

            SetState(SimpleButtonState.Default);
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

        private void UpdateButtonVisuals()
        {
            switch (_state)
            {
                case SimpleButtonState.Default:
                    if (_mainContainer.ClassListContains(ActiveButtonClass))
                    {
                        _mainContainer.RemoveFromClassList(ActiveButtonClass);
                    }
                    break;
                case SimpleButtonState.Active:
                    _mainContainer.AddToClassList(ActiveButtonClass);
                    break;
            }
        }
    }

}
