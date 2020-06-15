using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YobotChart.UiComponents
{
    public class AnimatedFrame : Frame
    {
        private readonly DoubleAnimation _opacityInAnimation;
        private readonly DoubleAnimation _scaleXInAnimation;
        private readonly DoubleAnimation _scaleYInAnimation;

        private readonly DoubleAnimation _opacityOutAnimation;
        private readonly Storyboard _fadeOutStoryboard;
        private readonly Storyboard _fadeInStoryboard;

        public AnimatedFrame()
        {
            _fadeInStoryboard = new Storyboard { Name = "FadeInStoryboard" };
            _opacityInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                BeginTime = TimeSpan.Zero,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            Storyboard.SetTargetProperty(_opacityInAnimation, new PropertyPath(OpacityProperty));

            _scaleXInAnimation = new DoubleAnimation
            {
                From = 0.95,
                To = 1,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                BeginTime = TimeSpan.Zero,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            _scaleYInAnimation = _scaleXInAnimation.Clone();
            Storyboard.SetTargetProperty(_scaleXInAnimation, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(_scaleYInAnimation, new PropertyPath("RenderTransform.ScaleY"));

            _fadeInStoryboard.Children.Add(_opacityInAnimation);
            _fadeInStoryboard.Children.Add(_scaleXInAnimation);
            _fadeInStoryboard.Children.Add(_scaleYInAnimation);

            _fadeOutStoryboard = new Storyboard { Name = "FadeOutStoryboard" };
            _opacityOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                BeginTime = TimeSpan.Zero,
                Duration = TimeSpan.FromMilliseconds(100)
            };
            _fadeOutStoryboard.Children.Add(_opacityOutAnimation);
            Storyboard.SetTargetProperty(_opacityOutAnimation, new PropertyPath(OpacityProperty));
        }

        public void AnimateNavigate(UIElement uiElement)
        {
            if (uiElement == Content) return;
            var ui = (UIElement)Content;
            if (ui != null)
            {
                Storyboard.SetTarget(_opacityOutAnimation, ui);

                _fadeOutStoryboard.Completed += OnSbOnCompleted;
                _fadeOutStoryboard.Begin();

                void OnSbOnCompleted(object obj, EventArgs args)
                {
                    InnerAnimateNavigate(uiElement);
                    _fadeOutStoryboard.Completed -= OnSbOnCompleted;
                }
            }
            else
            {
                InnerAnimateNavigate(uiElement);
            }
        }

        private void InnerAnimateNavigate(UIElement uiElement)
        {
            var endOpacity = uiElement.Opacity;
            var originTransform = uiElement.RenderTransform;
            uiElement.RenderTransformOrigin = new Point(0.5, 0.5);
            Storyboard.SetTarget(_opacityInAnimation, uiElement);
            Storyboard.SetTarget(_scaleXInAnimation, uiElement);
            Storyboard.SetTarget(_scaleYInAnimation, uiElement);
            if (uiElement.RenderTransform.GetType() != typeof(ScaleTransform))
                uiElement.RenderTransform = new ScaleTransform();
            NavigationService.Navigate(uiElement);

            _fadeInStoryboard.Completed += OnSbOnCompleted;
            _fadeInStoryboard.Begin();

            void OnSbOnCompleted(object sender, EventArgs args)
            {
                uiElement.RenderTransform = originTransform;
                _fadeInStoryboard.Completed -= OnSbOnCompleted;
            }
        }
    }
}
