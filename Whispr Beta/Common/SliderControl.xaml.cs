using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/*
 * Note about SliderControl namespace:
 * 
 * If namespace WhisprBeta.Common is used, then Visual Studio 2013 XAML designer will throw an error.
 * It seems like if a UserControl is in a different namespace than the parent XAML, then the designer in
 * parent XAML will not be able to access properties of the UserControl. The project will still compile
 * and run just fine, but the visual part of the XAML designer will not function.
 * 
 * Having other controls in Common namespace is fine, because none of their properties are set inside of
 * XAML designer.
 */
namespace WhisprBeta
{
    public partial class SliderControl
    {
        public delegate void SliderValueChangedEventHandler(int value);
        public event SliderValueChangedEventHandler ValueChanged;

        public delegate void SliderInstantValueChangedEventHandler(int value);
        public event SliderInstantValueChangedEventHandler InstantValueChanged;

        public SliderControl()
        {
            InitializeComponent();
            UnitOfMeasure = "";
            MinimumValue = 0;
            MaximumValue = 100;
            Value = 0;
            Step = 1;
            RefreshUi();
        }

        /// <summary>
        /// Text that is displayed on the right side of the slider value text.
        /// For example "100 m" or "3 min".
        /// Default "" (empty).
        /// </summary>
        private string unitOfMeasure = string.Empty;
        public string UnitOfMeasure
        {
            get { return unitOfMeasure; }
            set
            {
                unitOfMeasure = value;
                RefreshUi();
            }
        }

        /// <summary>
        /// Slider minimum value. Default 0 (zero).
        /// </summary>
        public int MinimumValue
        {
            get { return (int)Slider.Minimum; }
            set
            {
                Slider.Minimum = value;
                RefreshUi();
            }
        }

        /// <summary>
        /// Slider maximum value. Default 100.
        /// </summary>
        public int MaximumValue
        {
            get { return (int)Slider.Maximum; }
            set
            {
                Slider.Maximum = value;
                RefreshUi();
            }
        }

        /// <summary>
        /// Current slider value. Default 0 (zero).
        /// </summary>
        public int Value
        {
            // Addint 0.5 to slider value makes it nicer to use.
            // otherwise, to get the highest slider value user would have to
            // scroll the slider until the very very end of the slider, even if
            // the size of one unit is large. To understand better, try wide slider
            // with only a few values, like from 0 to 3. All the numbers have
            // big area on slider, except the last one (3), which activates only
            // in the very end of the slider. Adding 0.5 to slider value fixes this
            // problem.
            get { return (int)(Slider.Value + 0.5); }
            set
            {
                Slider.Value = value;
                RefreshUi();
            }
        }

        /// <summary>
        /// Minimum amount by which the slider will change value. Default 1.
        /// </summary>
        public int Step
        {
            get { return (int)Slider.SmallChange; }
            set
            {
                Slider.SmallChange = value;
                Slider.LargeChange = value;
            }
        }

        /// <summary>
        /// The minimum value change threshold for firing intstant value changed event
        /// </summary>
        public double InstantChangeThreshold
        {
            get { return instantChangeThreshold; }
            set { instantChangeThreshold = value; }
        }

        private void RefreshUi()
        {
            SetText(Value);
        }

        private void SetText(int value)
        {
            Text.Text = value + " " + unitOfMeasure;
        }

        private void OnValueChanged(int value)
        {
            if (ValueChanged != null)
            {
                ValueChanged(value);
            }
        }

        private double prevSliderValue;
        private double instantChangeThreshold = 1.0;

        private void UpdateInstantValue()
        {
            if (InstantValueChanged != null)
            {
                double currentSliderValue = Slider.Value;
                if (Math.Abs(currentSliderValue - prevSliderValue) >= instantChangeThreshold)
                {
                    prevSliderValue = currentSliderValue;
                    InstantValueChanged((int)Slider.Value);
                }
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // User is dragging the slider - update the value text
            // but don't fire the value changed event yet, because otherwise it would be firing continuously
            // as user drags the slider. Value changed event should be fired in slider.ManipulationCompleted
            // event handler.
            RefreshUi();

            // However, if someone subscribed to instant value updates, fire that too
            UpdateInstantValue();
        }

        private void slider_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // User stopped dragging the slider - fire value changed event
            OnValueChanged((int)((Slider)sender).Value);
        }
    }
}
