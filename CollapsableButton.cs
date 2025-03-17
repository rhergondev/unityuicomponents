using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
    [UxmlElement("collapsable-button")]
    public partial class CollapsableButton : VisualElement
    {
        public enum ButtonState
        {
            Inactive,
            Active,
        }
        public enum IsElementOn
        {
            On,
            Off,
        }

        public enum ButtonExpandedState
        {
            Expanded,
            Collapsed,
        }

        private VisualElement _associatedContent;
        private string _contentId;
        
        private const string ButtonClass = "collapsable-button";
        private const string ButtonCompactClass = "collapsable-button-compact";
        private const string ButtonClassActive = "collapsable-button-active";
        private const string ButtonImageClass = "collapsable-button-image-container";
        private const string ButtonImageClassActive = "collapsable-button-image-container-compact";
        
        private const string ElementShown = "element-shown-base";
        private const string ElementHidden = "element-hidden";

        private readonly Button _button;
        private readonly VisualElement _imageContainer;
        private Sprite _originalIcon;
        private Sprite _alternativeIcon;
        private bool _isActive = false;

        public TemplateContainer _parentContainer;
        
        public bool IsAssociatedContentHidden {get; private set;}

        private ButtonState _currentState = ButtonState.Inactive;
        private IsElementOn _currentIsElementOn = IsElementOn.Off;
        private ButtonExpandedState _expandedState = ButtonExpandedState.Expanded;
    
        public event Action OnClicked;

        [UxmlAttribute]
        public string AssociatedContentId
        {
            get => _contentId;
            set
            {
                _contentId = value;
                FindAndAssociateContent();
            }
        }

        [UxmlAttribute]
        private Sprite Icon
        {
            get => _originalIcon;
            set
            {
                _originalIcon = value;
                _imageContainer.style.backgroundImage = new StyleBackground(value); 
            }
        }

        [UxmlAttribute]
        private Sprite AlternativeIcon
        {
            get => _alternativeIcon;
            set => _alternativeIcon = value;
        }

        [UxmlAttribute]
        public ButtonState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                UpdateButtonState(_currentState);
            }
        }

        [UxmlAttribute]
        public IsElementOn IsElementOnState
        {
            get => _currentIsElementOn;
            set
            {
                _currentIsElementOn = value;
                UpdateElementOnState(_currentIsElementOn);
            }
        }
        

        [UxmlAttribute]
        public ButtonExpandedState ClickState
        {
            get => _expandedState;
            set => UpdateButtonExpandedState(value);
        }
        

        public CollapsableButton()
        {
            _button = new Button();
            _imageContainer = new VisualElement();
            _imageContainer.AddToClassList(ButtonImageClass);
            
            _button.Add(_imageContainer);
            this.Add(_button);
            
            _button.AddToClassList(ButtonClass);
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            _button.clicked += OnButtonClicked;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            FindAndAssociateContent();
            UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnButtonClicked()
        {
            ToggleButtonSize();
            UpdateButtonState(_currentState == ButtonState.Inactive ? ButtonState.Active : ButtonState.Inactive);
            if (IsAssociatedContentHidden)
            {
                ShowAssociatedContent();
            }
            else
            {
                HideAssociatedContent();
            }
            OnClicked?.Invoke();
        }

        public void ToggleButtonSize()
        {
            UpdateButtonExpandedState(_expandedState == ButtonExpandedState.Collapsed ? ButtonExpandedState.Expanded : ButtonExpandedState.Collapsed);
        }
        
        private void FindAndAssociateContent()
        {
            if (string.IsNullOrEmpty(_contentId)) return;
            
            _parentContainer = this.parent as TemplateContainer;
            _associatedContent = this.parent?.Q<VisualElement>(_contentId);
            if (_associatedContent != null)
            {
                _associatedContent.style.display = DisplayStyle.None;
                _associatedContent.AddToClassList(ElementShown);
                _associatedContent.AddToClassList(ElementHidden);
                IsAssociatedContentHidden = true;
            }
        }

        public void HideAssociatedContent()
        {
            if (_associatedContent == null) return;
            _associatedContent.AddToClassList(ElementHidden);
            this.schedule.Execute(() =>
            {
                _associatedContent.style.display = DisplayStyle.None;
            }).StartingIn(400);
            IsAssociatedContentHidden = true;
        }

        public void ShowAssociatedContent()
        {
            if (_associatedContent == null) return;
            _associatedContent.RemoveFromClassList(ElementHidden);
            this.schedule.Execute(() =>
            {
                _associatedContent.style.display = DisplayStyle.Flex;
            }).StartingIn(400);
            IsAssociatedContentHidden = false;
        }

        private void UpdateButtonState(ButtonState newState)
        {
            _currentState = newState;
            switch (_currentState)
            {
                case ButtonState.Inactive:
                    _button.RemoveFromClassList(ButtonClassActive);
                    _imageContainer.RemoveFromClassList(ButtonImageClassActive);
                    _isActive = false;
                    break;
                case ButtonState.Active:
                    _button.AddToClassList(ButtonClassActive);
                    _imageContainer.AddToClassList(ButtonImageClassActive);
                    _isActive = true;
                    break;
            }
        }
        
        private void UpdateElementOnState(IsElementOn newState)
        {
            _currentIsElementOn = newState;
            switch (_currentIsElementOn)
            {
                case IsElementOn.On:
                    _imageContainer.style.backgroundImage = new StyleBackground(_alternativeIcon);
                    break;
                case IsElementOn.Off:
                    _imageContainer.style.backgroundImage = new StyleBackground(_originalIcon);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void UpdateButtonExpandedState(ButtonExpandedState newState)
        {
            _expandedState = newState;
            switch (_expandedState)
            {
                case ButtonExpandedState.Expanded:
                    _button.RemoveFromClassList(ButtonCompactClass);
                    break;
                case ButtonExpandedState.Collapsed:
                    _button.AddToClassList(ButtonCompactClass);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}


