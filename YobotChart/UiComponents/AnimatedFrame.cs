using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using YobotChart.Pages;

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
            Default = this;
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

            Navigated += AnimatedFrame_Navigated;
        }

        public static AnimatedFrame Default { get; set; }

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

        public void AnimateNavigateBack()
        {
            var ui = (UIElement)Content;
            if (ui != null)
            {
                Storyboard.SetTarget(_opacityOutAnimation, ui);

                _fadeOutStoryboard.Completed += OnSbOnCompleted;
                _fadeOutStoryboard.Begin();

                void OnSbOnCompleted(object obj, EventArgs args)
                {
                    InnerAnimateNavigateBack();
                    _fadeOutStoryboard.Completed -= OnSbOnCompleted;
                }
            }
            else
            {
                InnerAnimateNavigateBack();
            }
        }

        private void InnerAnimateNavigate(UIElement uiElement)
        {
            NavigationService.Navigate(uiElement);
        }

        private void InnerAnimateNavigateBack()
        {
            NavigationService.GoBack();
        }

        private void AnimatedFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var uiElement = (UIElement)Content;
            var endOpacity = uiElement.Opacity;
            var originTransform = uiElement.RenderTransform;
            uiElement.RenderTransformOrigin = new Point(0.5, 0.5);
            Storyboard.SetTarget(_opacityInAnimation, uiElement);
            Storyboard.SetTarget(_scaleXInAnimation, uiElement);
            Storyboard.SetTarget(_scaleYInAnimation, uiElement);
            if (uiElement.RenderTransform.GetType() != typeof(ScaleTransform))
                uiElement.RenderTransform = new ScaleTransform();

            _fadeInStoryboard.Completed += OnSbOnCompleted;
            _fadeInStoryboard.Begin();

            void OnSbOnCompleted(object obj, EventArgs args)
            {
                uiElement.RenderTransform = originTransform;
                _fadeInStoryboard.Completed -= OnSbOnCompleted;
            }

            if (Content is DashBoardPage)
            {
                ClearHistory();
            }
        }
        private void ClearHistory()
        {
            if (!this.CanGoBack && !this.CanGoForward)
            {
                return;
            }

            var entry = this.RemoveBackEntry();
            while (entry != null)
            {
                Console.WriteLine("Removed " + entry.Name);
                entry = this.RemoveBackEntry();
            }

            //this.Navigate(new PageFunction<string> { RemoveFromJournal = true });
        }
    }
}
