using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
    public enum ButtonState
    {
        Default,
        Working,
        Active,
        Error
    }
    
    [UxmlElement("single-button")]
    public partial class SingleButton : VisualElement
    {
        private const string ButtonClass = "single_button_base_state";
        private const string ButtonImageClass = "single_button_image";
        
        private readonly Button _button;
        private readonly VisualElement _imageContainer;
        private readonly Label _label;
        private ButtonState _state;
        
        public event Action<ButtonState> OnStateChanged;

        [UxmlAttribute]
        public ButtonState State
        {
            get =>  _state;
            set => SetState(value);
        }
        
        [UxmlAttribute]
        public Sprite Icon
        {
            get => _imageContainer.style.backgroundImage.value.sprite;
            set
            {
                _imageContainer.style.backgroundImage = new StyleBackground(value);
                _button.MarkDirtyRepaint();
            } 
        }
        
        [UxmlAttribute]
        public String Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        public SingleButton()
        {
            _button = new Button();
            _button.AddToClassList(ButtonClass);
            
            hierarchy.Add(_button);
            
            _imageContainer = new VisualElement();
            _imageContainer.AddToClassList(ButtonImageClass);
            _button.Add(_imageContainer);

            _label = new Label();
            _button.Add(_label);
            
            SetState(ButtonState.Default);
        }

        public void SetState(ButtonState newState)
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
                case ButtonState.Default:
                    SetDefaultStyle();
                    break;
                case ButtonState.Working:
                    ChangeButtonColor("Yellow", Color.yellow);
                    break;
                case ButtonState.Active:
                    ChangeButtonColor("Green", Color.green);
                    break;
                case ButtonState.Error:
                    ChangeButtonColor("Orange", Color.red);
                    break;
            }
        }
        
        private void SetDefaultStyle()
        {
            _button.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0)); // Transparente
            _button.style.borderBottomColor = Color.white;
            _button.style.borderTopColor = Color.white;
            _button.style.borderLeftColor = Color.white;
            _button.style.borderRightColor = Color.white;
        }


        private void ChangeButtonColor(String color, Color fallbackColor)
        {
            var colorAsset = Resources.Load<EPistemeColors>("Data/EPistemeColors");
            _button.style.backgroundColor = colorAsset.GetColor(color, fallbackColor);
            _button.style.borderBottomColor = colorAsset.GetColor(color, fallbackColor);
            _button.style.borderTopColor = colorAsset.GetColor(color, fallbackColor);
            _button.style.borderLeftColor = colorAsset.GetColor(color, fallbackColor);
            _button.style.borderRightColor = colorAsset.GetColor(color, fallbackColor);
        }
    }
    
}
