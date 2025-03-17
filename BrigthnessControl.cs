using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
    [UxmlElement("brightness-control")]
    public partial class BrigthnessControl : VisualElement
    {
        #region Constants
        
        // USS classes to attach to different elements
        private const string BrightnessSliderClass = "brightness-slider";
        private const string BrightnessSliderContainerClass = "brightness-slider-container";
        private const string BrightnessSliderIconLowClass = "brightness-slider-icon-low";
        private const string BrightnessSliderIconHighClass = "brightness-slider-icon-high";
        private const string BrightnessSliderDetailOnClass = "brightness-slider-detail-on";
        private const string BrightnessSliderDetailOffClass = "brightness-slider-detail-off";
        private const string BrightnessSliderFillBarClass = "brightness-slider-progress-bar";
        
        #endregion
        
        #region Private Fields
        
        private VisualElement _sliderContainer;
        private VisualElement _sliderFillBar;
        private Slider _slider;
        
        #endregion
        
        #region Public Properties and Events

        [UxmlAttribute("min-value")]
        public float MinValue
        {
            get => _slider?.lowValue ?? 0f;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                if (_slider != null)
                {
                    _slider.lowValue = value;
                }
            }
        }

        [UxmlAttribute("max-value")]
        public float MaxValue
        {
            get => _slider?.highValue ?? 1f;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                if (_slider != null)
                {
                    _slider.highValue = value;
                }
            }
        }

        [UxmlAttribute("value")]
        public float Value
        {
            get => _slider?.value ?? MinValue;
            set
            {
                if (_slider != null)
                {
                    _slider.value = Mathf.Clamp(value, MinValue, MaxValue);
                }
            }
        }

        
        public event EventHandler<float> OnBrightnessChanged;
        
        #endregion

        #region Constructor
        
        public BrigthnessControl()
        {
            InitializeLayout();
        }
        
        #endregion

        #region Initialization Methods
        private void InitializeLayout()
        {
            CreateSliderContainer();
            AddLowBrigthnessIcon();
            InitializeSlider();
            AddHighBrigthnessIcon();
        }

        /// <summary>
        /// Creates the Slider and attaches it to its container
        /// </summary>
        private void CreateSliderContainer()
        {
            _sliderContainer = new VisualElement();
            _sliderContainer.AddToClassList(BrightnessSliderContainerClass);
            hierarchy.Add(_sliderContainer);
        }

        /// <summary>
        /// Adds the Icon for low Brigthness to the left of the Slider
        /// </summary>
        private void AddLowBrigthnessIcon()
        {
            var sliderIconLow = new VisualElement();
            sliderIconLow.AddToClassList(BrightnessSliderIconLowClass);
            _sliderContainer.Add(sliderIconLow);
        }

        /// <summary>
        /// Adds the Icon for High Brigthness to the right of the Slider
        /// </summary>
        private void AddHighBrigthnessIcon()
        {
            var sliderIconHigh = new VisualElement();
            sliderIconHigh.AddToClassList(BrightnessSliderIconHighClass);
            _sliderContainer.Add(sliderIconHigh);
        }
        
        #endregion
        
        #region Slider Setup

        /// <summary>
        /// Initializes and configures the Main Slider 
        /// </summary>
        private void InitializeSlider()
        {
            _slider = new Slider(MinValue, MaxValue, SliderDirection.Horizontal);
            _slider.AddToClassList(BrightnessSliderClass);
            _slider.value = Value;
            _sliderContainer.Add(_slider);

            _slider.schedule.Execute(SetupSliderElements);
            _slider.schedule.Execute(SetupSliderCallbacks);
        }
        
        /// <summary>
        /// Configures the Slider's Visual elements
        /// </summary>
        private void SetupSliderElements()
        {
            var mTracker = _slider.Q<VisualElement>("unity-tracker");

            SetupGradientBackground(mTracker);
            SetupSliderDetails(mTracker);
        }
        
        #endregion

        #region Visual Elements Setup
        
        private void SetupSliderDetails(VisualElement mTracker)
        {
            AddDetailElements(mTracker, BrightnessSliderDetailOnClass);
            SetupProgressBar();
            AddDetailElements(mTracker, BrightnessSliderDetailOffClass);
        }

        /// <summary>
        /// Sets up a little detail to the tracker
        /// </summary>
        /// <param name="mTracker">Reference to the Slider Tracker</param>
        /// <param name="className">Name of a USS class that styles the detail</param>
        private static void AddDetailElements(VisualElement mTracker, string className)
        {
            var detail = new VisualElement();
            detail.AddToClassList(className);
            detail.pickingMode = PickingMode.Ignore;
            mTracker.Add(detail);
        }
        
        /// <summary>
        /// Adds a gradient background to the tracker so the opacity for details does not overlap
        /// </summary>
        /// <param name="mTracker"></param>
        private static void SetupGradientBackground(VisualElement mTracker)
        {
            var gradientBackground = new OpacityQuad
            {
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    right = 0,
                    top = 0,
                    bottom = 0,
                    color = new StyleColor(new Color(1, 1, 1, 0.25f))
                }
            };
            mTracker.Add(gradientBackground);
        }
        
        private void SetupProgressBar()
        {
            var mDragger = _slider.Q<VisualElement>("unity-dragger");
            _sliderFillBar = new VisualElement();
            _sliderFillBar.AddToClassList(BrightnessSliderFillBarClass);
            _sliderFillBar.pickingMode = PickingMode.Ignore;
            _sliderFillBar.style.position = Position.Absolute;
            mDragger.Add(_sliderFillBar);
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Manages Slider callbacks
        /// </summary>
        private void SetupSliderCallbacks()
        {
            _slider.RegisterValueChangedCallback(evt =>
            {
                if (_sliderFillBar != null)
                {
                    OnSliderValueChanged(evt.newValue);
                }
            });
        }
        
        private void OnSliderValueChanged(float newValue)
        {
            OnBrightnessChanged?.Invoke(this, newValue);
        }
        
        #endregion
    }

    /// <summary>
    /// A Custom VisualElement that allows to have a small invisible area at the end of tracker
    /// This is done to avoid the tracker with opacity overlapping with the Details
    /// </summary>
    public class OpacityQuad : VisualElement
    {
        public OpacityQuad()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        private static readonly Vertex[] KVertices = new Vertex[8];
        private static readonly ushort[] KIndices = { 0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4 };

        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            Rect r = contentRect;
            if (r.width < 0.01f || r.height < 0.01f)
                return;

            Color mainColor = new Color(1, 1, 1, 0.25f);
            Color transparentColor = new Color(1, 1, 1, 0f);

            const float left = 0;
            var right = r.width;
            const float top = 0;
            var bottom = r.height;
            var splitPoint = right - (right * 0.0395f);

            KVertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
            KVertices[1].position = new Vector3(left, top, Vertex.nearZ);
            KVertices[2].position = new Vector3(splitPoint, top, Vertex.nearZ);
            KVertices[3].position = new Vector3(splitPoint, bottom, Vertex.nearZ);

            KVertices[0].tint = mainColor;
            KVertices[1].tint = mainColor;
            KVertices[2].tint = mainColor;
            KVertices[3].tint = mainColor;

            KVertices[4].position = new Vector3(splitPoint, bottom, Vertex.nearZ);
            KVertices[5].position = new Vector3(splitPoint, top, Vertex.nearZ);
            KVertices[6].position = new Vector3(right, top, Vertex.nearZ);
            KVertices[7].position = new Vector3(right, bottom, Vertex.nearZ);

            KVertices[4].tint = transparentColor;
            KVertices[5].tint = transparentColor;
            KVertices[6].tint = transparentColor;
            KVertices[7].tint = transparentColor;

            var mwd = mgc.Allocate(KVertices.Length, KIndices.Length);
            mwd.SetAllVertices(KVertices);
            mwd.SetAllIndices(KIndices);
        }
    }
}