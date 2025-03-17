using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
    [UxmlElement("multi-dependant-button-four")]
    public partial class MultiDependantButtonFour : VisualElement
    {
        private const string ContainerClass = "multi-dependant-button-container";
        private const string ButtonSmallClass = "multi-dependant-rounded-button";
        private const string ButtonLargeClass = "multi-dependant-streched-button";
        private const string ButtonActiveClass = "multi-depentdant-button-active";
        private const int ButtonCount = 4;
        
        private readonly VisualElement _container;
        private readonly List<Button> _buttons;
        private int _activeButtonIndex = -1;
        
        public Button LeftButton => _buttons[0];
        public Button LeftHoldButton => _buttons[1];
        public Button RightHoldButton => _buttons[2];
        public Button RightButton => _buttons[3];

        #region Properties
        
        public int ActiveButtonIndex => _activeButtonIndex;

        [UxmlAttribute] public Sprite Icon1 { get => GetButtonIcon(0); set => SetButtonIcon(0, value); }
        [UxmlAttribute] public Sprite Icon2 { get => GetButtonIcon(1); set => SetButtonIcon(1, value); }
        [UxmlAttribute] public Sprite Icon3 { get => GetButtonIcon(2); set => SetButtonIcon(2, value); }
        [UxmlAttribute] public Sprite Icon4 { get => GetButtonIcon(3); set => SetButtonIcon(3, value); }

        #endregion

        #region Constructor and Initialization
        
        public MultiDependantButtonFour()
        {
            _container = CreateContainer();
            _buttons = new List<Button>(ButtonCount);
            
            InitializeButtons();
        }
        
        private VisualElement CreateContainer()
        {
            var container = new VisualElement();
            container.AddToClassList(ContainerClass);
            hierarchy.Add(container);
            return container;
        }

        private void InitializeButtons()
        {
            for (var i = 0; i < ButtonCount; i++)
            {
                var button = CreateButton(i);
                _buttons.Add(button);
                _container.Add(button);
            }
        }

        private Button CreateButton(int index)
        {
            var button = new Button();
            button.AddToClassList(GetButtonClass(index));
            return button;
        }

        private string GetButtonClass(int index) => 
            index is 0 or 3 ? ButtonSmallClass : ButtonLargeClass;

        #endregion

        #region Public Methods

        public Button GetButton(int index)
        {
            return IsValidIndex(index) ? _buttons[index] : null;
        }

        public void SetActiveButton(int index)
        {
            if (!IsValidIndex(index)) return;
            
            DeactivateAllButtons();

            if (index == -1)
            {
                _activeButtonIndex = index;
                return;
            }
            if (_activeButtonIndex != index)
            {
                ActivateButton(index);
            }
            _activeButtonIndex = index;
        }

        #endregion

        #region Private Methods

        private bool IsValidIndex(int index) => 
            index >= -1 && index < _buttons.Count;

        private void DeactivateAllButtons()
        {
            foreach (var button in _buttons)
            {
                button.RemoveFromClassList(ButtonActiveClass);
            }
        }

        private void ActivateButton(int index)
        {
            _buttons[index].AddToClassList(ButtonActiveClass);
        }
        
        public int GetButtonIndex(Button button)
        {
            return _buttons.IndexOf(button);
        }


        private Sprite GetButtonIcon(int index)
        {
            return IsValidIndex(index) 
                ? _buttons[index].style.backgroundImage.value.sprite 
                : null;
        }
        
        private void SetButtonIcon(int index, Sprite sprite)
        {
            if (IsValidIndex(index))
            {
                _buttons[index].style.backgroundImage = new StyleBackground(sprite);
            }
        }

        #endregion
    }
}
