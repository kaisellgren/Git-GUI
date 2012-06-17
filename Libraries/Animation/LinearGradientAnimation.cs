using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GG.Libraries.Animation
{
    public class LinearGradientAnimation : AnimationTimeline
    {
        #region Dependency Properties

        public static readonly DependencyProperty FromProperty =
             DependencyProperty.Register("From",
                                         typeof(LinearGradientBrush),
                                         typeof(LinearGradientAnimation),
                                         new PropertyMetadata(null, AnimationFunction_Changed));

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To",
                                        typeof(LinearGradientBrush),
                                        typeof(LinearGradientAnimation),
                                        new PropertyMetadata(null, AnimationFunction_Changed));

        public static readonly DependencyProperty ByProperty =
            DependencyProperty.Register("By",
                                        typeof(LinearGradientBrush),
                                        typeof(LinearGradientAnimation),
                                        new PropertyMetadata(null, AnimationFunction_Changed));

        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction",
                                        typeof(IEasingFunction),
                                        typeof(LinearGradientAnimation));

        private static void AnimationFunction_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LinearGradientAnimation linearGradientAnimation = (LinearGradientAnimation) d;
            linearGradientAnimation._isAnimationFunctionValid = false;
        }

        #endregion // Dependency Properties

        #region Private Fields

        private LinearGradientBrush[] _keyValues;
        private AnimationType _animationType;
        private bool _isAnimationFunctionValid;

        #endregion // Private Fields

        #region Constructor

        public LinearGradientAnimation()
        {
        }

        public LinearGradientAnimation(LinearGradientBrush toValue, Duration duration)
            : this()
        {
            this.To = toValue;
            base.Duration = duration;
        }

        public LinearGradientAnimation(LinearGradientBrush toValue, Duration duration, FillBehavior fillBehavior)
            : this()
        {
            this.To = toValue;
            base.Duration = duration;
            base.FillBehavior = fillBehavior;
        }

        public LinearGradientAnimation(LinearGradientBrush fromValue, LinearGradientBrush toValue, Duration duration)
            : this()
        {
            this.From = fromValue;
            this.To = toValue;
            base.Duration = duration;
        }

        public LinearGradientAnimation(LinearGradientBrush fromValue, LinearGradientBrush toValue, Duration duration, FillBehavior fillBehavior)
            : this()
        {
            this.From = fromValue;
            this.To = toValue;
            base.Duration = duration;
            base.FillBehavior = fillBehavior;
        }

        #endregion // Constructor

        #region Properties

        public LinearGradientBrush From
        {
            get { return (LinearGradientBrush) base.GetValue(LinearGradientAnimation.FromProperty); }
            set { base.SetValue(LinearGradientAnimation.FromProperty, value); }
        }

        public LinearGradientBrush To
        {
            get { return (LinearGradientBrush) base.GetValue(LinearGradientAnimation.ToProperty); }
            set { base.SetValue(LinearGradientAnimation.ToProperty, value); }
        }

        public LinearGradientBrush By
        {
            get { return (LinearGradientBrush) base.GetValue(LinearGradientAnimation.ByProperty); }
            set { base.SetValue(LinearGradientAnimation.ByProperty, value); }
        }

        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction) base.GetValue(LinearGradientAnimation.EasingFunctionProperty); }
            set { base.SetValue(LinearGradientAnimation.EasingFunctionProperty, value); }
        }

        public bool IsAdditive
        {
            get { return (bool) base.GetValue(AnimationTimeline.IsAdditiveProperty); }
            set { base.SetValue(AnimationTimeline.IsAdditiveProperty, value); }
        }

        public bool IsCumulative
        {
            get { return (bool) base.GetValue(AnimationTimeline.IsCumulativeProperty); }
            set { base.SetValue(AnimationTimeline.IsCumulativeProperty, value); }
        }

        #endregion // Properties

        #region Private Methods

        protected LinearGradientBrush GetCurrentValueCore(LinearGradientBrush defaultOriginValue, LinearGradientBrush defaultDestinationValue, AnimationClock animationClock)
        {
            if (!this._isAnimationFunctionValid)
            {
                this.ValidateAnimationFunction();
            }
            double num = animationClock.CurrentProgress.Value;
            IEasingFunction easingFunction = this.EasingFunction;
            if (easingFunction != null)
            {
                num = easingFunction.Ease(num);
            }
            LinearGradientBrush brush = null;
            LinearGradientBrush brush2 = null;
            LinearGradientBrush value = null;
            LinearGradientBrush value2 = null;
            switch (this._animationType)
            {
                case AnimationType.Automatic:
                    brush = defaultOriginValue;
                    brush2 = defaultDestinationValue;
                    value = GetDefaultLinearGradientBrush(brush);
                    value2 = GetDefaultLinearGradientBrush(brush);
                    break;
                case AnimationType.From:
                    brush = this._keyValues[0];
                    brush2 = defaultDestinationValue;
                    value = GetDefaultLinearGradientBrush(brush);
                    value2 = GetDefaultLinearGradientBrush(brush);
                    break;
                case AnimationType.To:
                    brush = defaultOriginValue;
                    brush2 = this._keyValues[0];
                    value = GetDefaultLinearGradientBrush(brush2);
                    value2 = GetDefaultLinearGradientBrush(brush2);
                    break;
                case AnimationType.By:
                    brush2 = this._keyValues[0];
                    value2 = defaultOriginValue;
                    value = GetDefaultLinearGradientBrush(brush2);
                    value2 = GetDefaultLinearGradientBrush(brush2);
                    break;
                case AnimationType.FromTo:
                    brush = this._keyValues[0];
                    brush2 = this._keyValues[1];
                    value = GetDefaultLinearGradientBrush(brush);
                    value2 = GetDefaultLinearGradientBrush(brush);
                    if (this.IsAdditive)
                    {
                        value2 = defaultOriginValue;
                    }
                    break;
                case AnimationType.FromBy:
                    brush = this._keyValues[0];
                    brush2 = AddLinearGradientBrush(this._keyValues[0], this._keyValues[1]);
                    value = GetDefaultLinearGradientBrush(brush);
                    value2 = GetDefaultLinearGradientBrush(brush);
                    if (this.IsAdditive)
                    {
                        value2 = defaultOriginValue;
                    }
                    break;
            }

            if (this.IsCumulative)
            {
                double num2 = (double) (animationClock.CurrentIteration - 1).Value;
                if (num2 > 0.0)
                {
                    LinearGradientBrush value3 = SubtractLinearGradientBrush(brush2, brush);
                    value = ScaleLinearGradientBrush(value3, num2);
                }
            }
            LinearGradientBrush returnBrush = AddLinearGradientBrush(value2, AddLinearGradientBrush(value, InterpolateGradientBrush(brush, brush2, num)));
            return returnBrush;
        }

        private LinearGradientBrush GetDefaultLinearGradientBrush(LinearGradientBrush brush)
        {
            LinearGradientBrush returnBrush = new LinearGradientBrush();
            returnBrush.StartPoint = default(Point);
            returnBrush.EndPoint = default(Point);
            for (int i = 0; i < brush.GradientStops.Count; i++)
            {
                returnBrush.GradientStops.Add(new GradientStop(default(Color), default(double)));
            }
            return returnBrush;
        }

        private void ValidateAnimationFunction()
        {
            this._animationType = AnimationType.Automatic;
            this._keyValues = null;
            if (this.From != null)
            {
                if (this.To != null)
                {
                    this._animationType = AnimationType.FromTo;
                    this._keyValues = new LinearGradientBrush[2];
                    this._keyValues[0] = this.From;
                    this._keyValues[1] = this.To;
                }
                else
                {
                    if (this.By != null)
                    {
                        this._animationType = AnimationType.FromBy;
                        this._keyValues = new LinearGradientBrush[2];
                        this._keyValues[0] = this.From;
                        this._keyValues[1] = this.By;
                    }
                    else
                    {
                        this._animationType = AnimationType.From;
                        this._keyValues = new LinearGradientBrush[1];
                        this._keyValues[0] = this.From;
                    }
                }
            }
            else
            {
                if (this.To != null)
                {
                    this._animationType = AnimationType.To;
                    this._keyValues = new LinearGradientBrush[1];
                    this._keyValues[0] = this.To;
                }
                else
                {
                    if (this.By != null)
                    {
                        this._animationType = AnimationType.By;
                        this._keyValues = new LinearGradientBrush[1];
                        this._keyValues[0] = this.By;
                    }
                }
            }
            this._isAnimationFunctionValid = true;
        }

        #endregion // Private Methods

        #region AnimationTimeline / Freezable Members

        public override Type TargetPropertyType
        {
            get { return typeof(LinearGradientBrush); }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (defaultOriginValue == null)
            {
                throw new ArgumentNullException("defaultOriginValue");
            }
            if (defaultDestinationValue == null)
            {
                throw new ArgumentNullException("defaultDestinationValue");
            }
            return this.GetCurrentValue((LinearGradientBrush) defaultOriginValue, (LinearGradientBrush) defaultDestinationValue, animationClock);
        }

        public LinearGradientBrush GetCurrentValue(LinearGradientBrush defaultOriginValue, LinearGradientBrush defaultDestinationValue, AnimationClock animationClock)
        {
            base.ReadPreamble();
            if (animationClock == null)
            {
                throw new ArgumentNullException("animationClock");
            }
            if (animationClock.CurrentState == ClockState.Stopped)
            {
                return defaultDestinationValue;
            }
            return this.GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
        }

        public new LinearGradientAnimation Clone()
        {
            return (LinearGradientAnimation) base.Clone();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LinearGradientAnimation();
        }

        #endregion // AnimationTimeline / Freezable Members

        #region Helper Methods

        internal static LinearGradientBrush AddLinearGradientBrush(LinearGradientBrush brush1, LinearGradientBrush brush2)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = AddPoint(brush1.StartPoint, brush2.StartPoint);
            gradientBrush.EndPoint = AddPoint(brush1.EndPoint, brush2.EndPoint);
            for (int i = 0; i < brush1.GradientStops.Count; i++)
            {
                GradientStop gradientStop1 = brush1.GradientStops[i];
                GradientStop gradientStop2 = brush2.GradientStops[i];

                Color color = AddColor(gradientStop1.Color, gradientStop2.Color);
                double offset = AddDouble(gradientStop1.Offset, gradientStop2.Offset);

                gradientBrush.GradientStops.Add(new GradientStop(color, offset));
            }
            return gradientBrush;
        }
        internal static LinearGradientBrush SubtractLinearGradientBrush(LinearGradientBrush brush1, LinearGradientBrush brush2)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = SubtractPoint(brush1.StartPoint, brush2.StartPoint);
            gradientBrush.EndPoint = SubtractPoint(brush1.EndPoint, brush2.EndPoint);
            for (int i = 0; i < brush1.GradientStops.Count; i++)
            {
                GradientStop gradientStop1 = brush1.GradientStops[i];
                GradientStop gradientStop2 = brush2.GradientStops[i];

                Color color = SubtractColor(gradientStop1.Color, gradientStop2.Color);
                double offset = SubtractDouble(gradientStop1.Offset, gradientStop2.Offset);

                gradientBrush.GradientStops.Add(new GradientStop(color, offset));
            }
            return gradientBrush;
        }
        internal static LinearGradientBrush ScaleLinearGradientBrush(LinearGradientBrush value, double factor)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = ScalePoint(value.StartPoint, factor);
            gradientBrush.EndPoint = ScalePoint(value.EndPoint, factor);
            for (int i = 0; i < value.GradientStops.Count; i++)
            {
                Color color = ScaleColor(value.GradientStops[i].Color, factor);
                double offset = ScaleDouble(value.GradientStops[i].Offset, factor);
                gradientBrush.GradientStops.Add(new GradientStop(color, offset));
            }
            return gradientBrush;
        }
        internal static LinearGradientBrush InterpolateGradientBrush(LinearGradientBrush from, LinearGradientBrush to, double progress)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = InterpolatePoint(from.StartPoint, to.StartPoint, progress);
            gradientBrush.EndPoint = InterpolatePoint(from.EndPoint, to.EndPoint, progress);
            for (int i = 0; i < from.GradientStops.Count; i++)
            {
                Color color = InterpolateColor(from.GradientStops[i].Color, to.GradientStops[i].Color, progress);
                double offset = InterpolateDouble(from.GradientStops[i].Offset, to.GradientStops[i].Offset, progress);
                gradientBrush.GradientStops.Add(new GradientStop(color, offset));
            }
            return gradientBrush;
        }
        internal static Color AddColor(Color value1, Color value2)
        {
            return value1 + value2;
        }
        internal static Color SubtractColor(Color value1, Color value2)
        {
            return value1 - value2;
        }
        internal static Color ScaleColor(Color value, double factor)
        {
            return value * (float) factor;
        }
        internal static Color InterpolateColor(Color from, Color to, double progress)
        {
            return from + (to - from) * (float) progress;
        }

        internal static double AddDouble(double value1, double value2)
        {
            return value1 + value2;
        }
        internal static double SubtractDouble(double value1, double value2)
        {
            return value1 - value2;
        }
        internal static double ScaleDouble(double value, double factor)
        {
            return value * factor;
        }
        internal static double InterpolateDouble(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }

        internal static Point AddPoint(Point value1, Point value2)
        {
            return new Point(value1.X + value2.X, value1.Y + value2.Y);
        }
        internal static Point SubtractPoint(Point value1, Point value2)
        {
            return new Point(value1.X - value2.X, value1.Y - value2.Y);
        }
        internal static Point ScalePoint(Point value, double factor)
        {
            return new Point(value.X * factor, value.Y * factor);
        }
        internal static Point InterpolatePoint(Point from, Point to, double progress)
        {
            return from + (to - from) * progress;
        }

        #endregion // Helper Methods
    }

    internal enum AnimationType : byte
    {
        Automatic,
        From,
        To,
        By,
        FromTo,
        FromBy
    }
}