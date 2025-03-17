using UnityEngine;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
   [UxmlElement("bouncy-slider")]
    public partial class BouncySlider : VisualElement
    {
        #region Constants
        
        private const string BouncySliderClass = "bouncy-slider";
        private const string BouncySliderContainerClass = "bouncy-slider-container";
        private const string BouncySliderDetailLeftClass = "bouncy-slider-detail-left";
        private const string BouncySliderDetailRightClass = "bouncy-slider-detail-right";
        
        #endregion
        
        #region Properties
        
        [UxmlAttribute("icon-left-fast")] private Sprite IconLeftFast { get; set; }
        [UxmlAttribute("icon-left-slow")] private Sprite IconLeftSlow { get; set; }
        [UxmlAttribute("icon-center")] private Sprite IconCenter { get; set; }
        [UxmlAttribute("icon-right-slow")] private Sprite IconRightSlow { get; set; }
        [UxmlAttribute("icon-right-fast")] private Sprite IconRightFast { get; set; }
        
        #endregion

        private IVisualElementScheduledItem _bounceAnimation;
        private float _animationDuration = 0.1f;
        private float _elapsedTime;

        private Slider _slider;
        private VisualElement _sliderContainer;
        private VisualElement _dragger;
        
        private VisualElement _leftFillBar;
        private VisualElement _rightFillBar;
        
        public event System.Action<float> OnValueChanged;
        
        public float CurrentValue => _slider?.value ?? 0f;

        

        public BouncySlider()
        {
            _sliderContainer = new VisualElement();
            _sliderContainer.AddToClassList(BouncySliderContainerClass);
            hierarchy.Add(_sliderContainer);

            _slider = new Slider(-2, 2, SliderDirection.Horizontal);
            _slider.value = 0;
            _slider.AddToClassList(BouncySliderClass);
            _sliderContainer.Add(_slider);

            _slider.schedule.Execute(() =>
            {
                var m_Tracker = _slider.Q<VisualElement>("unity-tracker");
                
                var sliderDetailLeft = new VisualElement();
                sliderDetailLeft.AddToClassList(BouncySliderDetailLeftClass);
                sliderDetailLeft.pickingMode = PickingMode.Ignore;
                m_Tracker.Add(sliderDetailLeft);
                
                _dragger = _slider.Q<VisualElement>("unity-dragger");
                _dragger.style.backgroundImage = new StyleBackground(Background.FromSprite(IconCenter));  

                var centerContainer = new VisualElement();
                centerContainer.style.position = Position.Absolute;
                centerContainer.style.width = new StyleLength(Length.Percent(100));
                centerContainer.style.height = new StyleLength(Length.Percent(100));
                m_Tracker.Add(centerContainer);

                _leftFillBar = new VisualElement();
                _leftFillBar.style.position = Position.Absolute;
                _leftFillBar.style.right = new StyleLength(Length.Percent(50));
                _leftFillBar.style.width = new StyleLength(0f);
                _leftFillBar.style.height = new StyleLength(Length.Percent(100));
                _leftFillBar.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 1f));
                centerContainer.Add(_leftFillBar);

                _rightFillBar = new VisualElement();
                _rightFillBar.style.position = Position.Absolute;
                _rightFillBar.style.left = new StyleLength(Length.Percent(50));
                _rightFillBar.style.width = new StyleLength(0f);
                _rightFillBar.style.height = new StyleLength(Length.Percent(100));
                _rightFillBar.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 1f));
                centerContainer.Add(_rightFillBar);

                _slider.RegisterValueChangedCallback(evt => UpdateProgressBars(evt.newValue));
       
                var sliderDetailRight = new VisualElement();
                sliderDetailRight.AddToClassList(BouncySliderDetailRightClass);
                sliderDetailRight.pickingMode = PickingMode.Ignore;
                m_Tracker.Add(sliderDetailRight);
                
                SetupDragger();
            });

            

            _slider.schedule.Execute(() =>
            {
                _slider.RegisterValueChangedCallback(evt =>
                {
                        ManageDraggerIcon(evt.newValue);
                        OnValueChanged?.Invoke(evt.newValue);
                });
            });
        }

        private void UpdateProgressBars(float value)
        {
            float normalizedValue = Mathf.Abs(value) / _slider.highValue; 
            float fillWidth = normalizedValue * 50; 

            if (value > 0)
            {
                _rightFillBar.style.width = new StyleLength(Length.Percent(fillWidth));
                _leftFillBar.style.width = new StyleLength(0f);
            }
            else if (value < 0)
            {
                _leftFillBar.style.width = new StyleLength(Length.Percent(fillWidth));
                _rightFillBar.style.width = new StyleLength(0f);
            }
            else
            {
                _leftFillBar.style.width = new StyleLength(0f);
                _rightFillBar.style.width = new StyleLength(0f);
            }

        }

        private void ManageDraggerIcon(float evtNewValue)
        {
            var changepoint = _slider.highValue / 2;

            if (evtNewValue == 0)
            {
                _dragger.style.backgroundImage = new StyleBackground(Background.FromSprite(IconCenter)); 
            } else if (evtNewValue > 0 && evtNewValue < changepoint)
            {
                _dragger.style.backgroundImage = new StyleBackground(Background.FromSprite(IconRightSlow)); 
            } else if (evtNewValue >= changepoint)
            {
                _dragger.style.backgroundImage = new StyleBackground(Background.FromSprite(IconRightFast)); 
            } else if (evtNewValue < 0 && evtNewValue > -changepoint)
            {
                _dragger.style.backgroundImage = new StyleBackground(Background.FromSprite(IconLeftSlow)); 
            } else if (evtNewValue <= -changepoint)
            {
                _dragger.style.backgroundImage = new StyleBackground(Background.FromSprite(IconLeftFast));
            }
        }
        
        private void SetupDragger()
        {
            Vector2 dragStartPos = Vector2.zero;
            float startValue = 0;
            bool isDragging = false;
            
            var draggerContainer = _slider.Q<VisualElement>("unity-drag-container");
            
            draggerContainer.RegisterCallback<PointerDownEvent>(evt =>
            {
                Debug.Log("Dragger pressed");
                isDragging = true;
                dragStartPos = evt.position;
                _slider.CapturePointer(evt.pointerId);
            }, TrickleDown.TrickleDown);

            draggerContainer.RegisterCallback<PointerUpEvent>(evt =>
            {
                isDragging = false;
                _slider.ReleasePointer(evt.pointerId);
                Debug.Log("Dragger released");
                StartBounceBackAnimation();
            }, TrickleDown.TrickleDown);
        }

        private void StartBounceBackAnimation()
        {
            _elapsedTime = 0;
            float startValue = _slider.value;

            _bounceAnimation = schedule.Execute(() =>
            {
                _elapsedTime += Time.deltaTime;
                float t = _elapsedTime / _animationDuration;

                if (t >= 1f)
                {
                    _slider.value = 0f;
                    _bounceAnimation.Pause();
                    return;
                }

                _slider.value = Mathf.Lerp(startValue, 0f, t);
            }).Every(0);
        }
    }
}

