using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
    [UxmlElement("color-selector")]
    public partial class ColorSelector : VisualElement
    {
        private const string ColorSelectorScrollClass = "color-selector-scroll-container";

        [UxmlAttribute]
        public List<Color> Colors
        {
            get => _colors;
            set
            {
                _colors = value;
                UpdateButtons();
            }
        }

        [UxmlAttribute]
        public int Animations
        {
            get => _animations;
            set
            {
                _animations = value;
                UpdateButtons();
            }
        }
        
        private ButtonManager _buttonManager;
        private readonly ScrollView _scrollView;
        private List<Color> _colors;
        private int _animations;
        
        public event Action<string, Color?> OnColorSelected;

        public ColorSelector()
        {
            _colors = new List<Color>();
            _animations = 0;
            _buttonManager = new ButtonManager();

            _buttonManager.OnButtonStateChanged += HandleButtonStateChanged;

            _scrollView = CreateScrollView();
            Add(_scrollView);
            _scrollView.RegisterCallback<ClickEvent>(evt => Debug.Log("Clicked"));

            GenerateButtons();
        }

        private void HandleButtonStateChanged(SelectorButton button)
        {
            if (button is DisableButton)
            {
                OnColorSelected?.Invoke("disable", null);
            } else if (button is ColorButton colorButton)
            {
                OnColorSelected?.Invoke("color", colorButton.ButtonColor);
            }
        }

        public void UpdateButtons()
        {
            _scrollView.Clear();
            _buttonManager = new ButtonManager();
            _buttonManager.OnButtonStateChanged += HandleButtonStateChanged;
            GenerateButtons();
        }

        private void GenerateButtons()
        {
            var disableButton = AddButton(new DisableButton());
            
            _buttonManager.SetActiveButton(disableButton);
            if (Colors != null)
            {
                foreach (var color in Colors)
                {
                    AddButton(new ColorButton(color));
                }
            }
            
            for (var i = 0; i < Animations; i++)
            {
                AddButton(new NumberButton(i + 1));
            }
        }

        private SelectorButton AddButton(SelectorButton button)
        {
            _scrollView.Add(button);
            _buttonManager.AddButton(button);
            return button;
        }

        private ScrollView CreateScrollView()
        {
            var scrollView = new ScrollView();
            scrollView.mode = ScrollViewMode.Horizontal;
            scrollView.pickingMode = PickingMode.Position;
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.AddToClassList(ColorSelectorScrollClass);
            return scrollView;
        }
    }

    public abstract class SelectorButton : Button
    {
        public string ButtonId { get; protected set; }
        protected VisualElement InternalElement { get; private set; }
        protected bool IsSelected { get; private set; }

        public event Action<SelectorButton> OnButtonSelected;
        public event Action<SelectorButton> OnButtonDeselected;

        protected SelectorButton()
        {
            HideDefaultBorder();
            InternalElement = new VisualElement();
            Add(InternalElement);
            SetupButton();

            this.clicked += HandleButtonClick;
        }

        internal void HideDefaultBorder()
        {
            style.borderBottomWidth = 0f;
            style.borderLeftWidth = 0f;
            style.borderRightWidth = 0f;
            style.borderTopWidth = 0f;
        }

        internal void ShowBorder()
        {
            style.borderBottomWidth = 8f;
            style.borderLeftWidth = 8f;
            style.borderRightWidth = 8f;
            style.borderTopWidth = 8f;
        }

        private void HandleButtonClick()
        {
            OnButtonSelected?.Invoke(this);
        }
        
        public virtual void SetSelected(bool selected)
        {
            IsSelected = selected;
            UpdateVisualState();

            if (selected)
            {
                OnButtonSelected?.Invoke(this);
            }
            else
            {
                OnButtonDeselected?.Invoke(this);
            }
        }
        
        protected abstract void SetupButton();
        protected abstract void UpdateVisualState();
    }

    internal class DisableButton : SelectorButton
    {
        private const string ExternalClass = "color-selector-ext-disable-button";
        private const string InternalClass = "color-selector-int-disable-button";

        public DisableButton()
        {
            ButtonId = "disable_button";
        }

        protected override void SetupButton()
        {
            style.backgroundColor = new StyleColor(Color.clear);
            AddToClassList(ExternalClass);
            InternalElement.AddToClassList(InternalClass);
        }

        protected override void UpdateVisualState()
        {
            if (IsSelected)
            {
                style.backgroundColor = new StyleColor(Color.white);
                InternalElement.style.unityBackgroundImageTintColor = new StyleColor(Color.red);
            }
            else
            {
                style.backgroundColor = new StyleColor(Color.clear);
                InternalElement.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            }
        }
    }

    internal class ColorButton : SelectorButton
    {
        private const string ExternalBaseClass = "color-selector-base-background";
        private const string ExternalClass = "color-selector-ext-color-button";
        private const string InternalClass = "color-selector-int-color-button";

        private Color _buttonColor;
        public Color ButtonColor
        {
            get => _buttonColor;
            set
            {
                _buttonColor = value;
                InternalElement.style.backgroundColor = new StyleColor(value);
            }
        }

        public ColorButton(Color color) : base()
        {
            ButtonColor = color;
            ButtonId = $"color_button_{color.r:F2}_{color.g:F2}_{color.b:F2}";
        }
        protected override void SetupButton()
        {
            Debug.Log(ButtonColor);
            AddToClassList(ExternalBaseClass);
            InternalElement.AddToClassList(InternalClass);
            InternalElement.style.backgroundColor = new StyleColor(_buttonColor);
        }
        protected override void UpdateVisualState()
        {
            if (IsSelected)
            {
                ShowBorder();
                AddToClassList(ExternalClass);
            }
            else
            {
                HideDefaultBorder();
                RemoveFromClassList(ExternalClass);
            }
        }
    }

    internal class NumberButton : SelectorButton
    {
        private const string ExternalBaseClass = "color-selector-base-background";
        private const string ButtonClass = "color-selector-int-number-button";
        private const string ExternalClass = "color-selector-ext-color-button";

        public NumberButton(int number)
        {
            ButtonId = $"number_button_{number}";
            var label = new Label(number.ToString());
            InternalElement.Add(label);
        }

        protected override void SetupButton()
        {
            AddToClassList(ExternalBaseClass);
            InternalElement.AddToClassList(ButtonClass);
        }

        protected override void UpdateVisualState()
        {
            if (IsSelected)
            {
                ShowBorder();
                AddToClassList(ExternalClass);
            }
            else
            {
                HideDefaultBorder();
                RemoveFromClassList(ExternalClass);
            }
        }
    }

    public class ButtonManager
    {
        public event Action<SelectorButton> OnButtonStateChanged;
        private readonly List<SelectorButton> _buttons = new();
        private SelectorButton _selectedButton;

        public void AddButton(SelectorButton button)
        {
            _buttons.Add(button);
            button.clicked += () =>
            {
                SetActiveButton(button);
                Debug.Log($"Selected Button in ButtonManager - ID: {button.ButtonId}"
                );
            };
        }

        public void SetActiveButton(SelectorButton button)
        {
            if (_selectedButton != null)
            {
                _selectedButton.SetSelected(false);
            }
            
            _selectedButton = button;
            button.SetSelected(true);
            
            OnButtonStateChanged?.Invoke(button);
        }
    }
}

    
