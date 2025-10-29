using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace MultiXIVLauncher.Utils
{
    public static class UIAnimationHelper
    {
        /// <summary>
        /// Animates the appearance of an element with a vertical slide and a gradual fade.
        /// </summary>
        public static void AnimateAppearance(FrameworkElement element, double fromY = 30)
        {
            element.Opacity = 0;
            element.RenderTransform = new TranslateTransform(0, fromY);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase()
            };
            var slideUp = new DoubleAnimation(fromY, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase()
            };

            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            element.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);

            if (element.Effect is DropShadowEffect shadow)
            {
                var shadowFade = new DoubleAnimation(0.9, 0.2, TimeSpan.FromMilliseconds(800))
                {
                    BeginTime = TimeSpan.FromMilliseconds(250),
                    EasingFunction = new QuadraticEase()
                };
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty, shadowFade);
            }
        }

        /// <summary>
        /// Animates the disappearance of an element with slide down + fade.
        /// </summary>
        public static void AnimateRemoval(FrameworkElement element, Action? onComplete = null)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase()
            };
            var slideDown = new DoubleAnimation(0, 30, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase()
            };

            fadeOut.Completed += (s, _) => onComplete?.Invoke();

            element.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            element.RenderTransform = new TranslateTransform();
            element.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideDown);

            if (element.Effect is DropShadowEffect shadow)
            {
                var shadowFade = new DoubleAnimation(shadow.Opacity, 0, TimeSpan.FromMilliseconds(250));
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty, shadowFade);
            }
        }
    }
}
