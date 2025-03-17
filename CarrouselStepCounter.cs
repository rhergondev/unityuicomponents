using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
    [UxmlElement("carrousel-step-counter")]
    public partial class CarrouselStepCounter : VisualElement
    {
        private const string CarrouselContainerClass = "carrousel-counter-container";
        private const string CarrouselBulletClass = "carrousel-counter-bullet";
        private const string CarrouselBulletActiveClass = "carrousel-counter-bullet-active";
        private const string CarrouselLineClass = "carrousel-counter-line";
        
        private readonly VisualElement _carrouselContainer;
        private readonly List<VisualElement> _carrouselSteps;
        private int CurrentStep { get; set; }

        private int _stepCount = 3;

        private int _stepTarget;
        [UxmlAttribute]
        public int SelectedStep { get => _stepTarget;
            set
            {
                if (value >= 0 && _stepTarget != value)
                {
                    _stepTarget = value;
                    SetCurrentStep(_stepTarget);
                }
            }
        }
        
        [UxmlAttribute]
        public int StepCount { get => _stepCount;
            set
            {
                if (value > 0 && _stepCount != value)
                {
                    _stepCount = value;
                    CreateSteps(_stepCount);
                }
            }
        }
        
        public CarrouselStepCounter(){
            _carrouselContainer = new VisualElement();
            _carrouselContainer.AddToClassList(CarrouselContainerClass);
            _carrouselSteps = new List<VisualElement>();
            
            hierarchy.Add(_carrouselContainer);

            CreateSteps(StepCount);
        }
        
        private void CreateSteps(int steps){
            _carrouselContainer.Clear();
            _carrouselSteps.Clear();

            for (int i = 0; i < steps; i++)
            {
                var bullet = new VisualElement();
                bullet.AddToClassList(CarrouselBulletClass);
                
                var label = new Label((i + 1).ToString());
                bullet.Add(label);

                _carrouselSteps.Add(bullet);
                _carrouselContainer.Add(bullet);

                if (i < steps - 1)
                {
                    var line = new VisualElement();
                    line.AddToClassList(CarrouselLineClass);
                    _carrouselContainer.Add(line);
                }
            }
        }

        public void SetCurrentStep(int step)
        {
            if (step < 0 || step > _carrouselSteps.Count) return;

            for (var i = 0; i < _carrouselSteps.Count; i++)
            {
                if (i < step)
                {
                    _carrouselSteps[i].AddToClassList(CarrouselBulletActiveClass);
                }
                else
                {
                    _carrouselSteps[i].RemoveFromClassList(CarrouselBulletActiveClass);
                }
            }
            CurrentStep = step;
        }
    }
    
    
}
