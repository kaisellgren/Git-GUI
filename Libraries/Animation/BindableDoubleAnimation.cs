using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace GG.Libraries.Animation
{
    public class BindableDoubleAnimation : DoubleAnimationBase
    {
        DoubleAnimation internalAnimation;

        public DoubleAnimation InternalAnimation { get { return internalAnimation; } }

        public double To
        {
            get { return (double) GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        /// <summary>
        /// Dependency backing property for the <see cref="To"/> property.
        /// </summary>
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(double), typeof(BindableDoubleAnimation), new UIPropertyMetadata(0d, new PropertyChangedCallback((s, e) =>
            {
                BindableDoubleAnimation sender = (BindableDoubleAnimation) s;
                sender.internalAnimation.To = (double) e.NewValue;
            })));


        public double From
        {
            get { return (double) GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        /// <summary>
        /// Dependency backing property for the <see cref="From"/> property.
        /// </summary>
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(double), typeof(BindableDoubleAnimation), new UIPropertyMetadata(0d, new PropertyChangedCallback((s, e) =>
            {
                BindableDoubleAnimation sender = (BindableDoubleAnimation) s;
                sender.internalAnimation.From = (double) e.NewValue;
            })));


        public BindableDoubleAnimation()
        {
            internalAnimation = new DoubleAnimation();
        }

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            return internalAnimation.GetCurrentValue(defaultOriginValue, defaultDestinationValue, animationClock);
        }

        protected override Freezable CreateInstanceCore()
        {
            return internalAnimation.Clone(); ;
        }
    }
}