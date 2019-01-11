using System;
using System.ComponentModel;
using Airbnb.Lottie;
using Foundation;
using Lottie.Forms;
using Lottie.Forms.EventArguments;
using Lottie.Forms.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(AnimationView), typeof(AnimationViewRenderer))]

namespace Lottie.Forms.iOS.Renderers
{
    public class AnimationViewRenderer : ViewRenderer<AnimationView, LOTAnimationView>
    {
        private LOTAnimationView _animationView;
        private UITapGestureRecognizer _gestureRecognizer;

        /// <summary>
        ///   Used for registration with dependency service
        /// </summary>
        public new static void Init()
        {
            // needed because of this linker issue: https://bugzilla.xamarin.com/show_bug.cgi?id=31076
#pragma warning disable 0219
            var dummy = new AnimationViewRenderer();
#pragma warning restore 0219
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AnimationView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                e.OldElement.OnPlay -= OnPlay;
                e.OldElement.OnPause -= OnPause;
                e.OldElement.OnPlayProgressSegment -= OnPlayProgressSegment;
                e.OldElement.OnPlayFrameSegment -= OnPlayFrameSegment;
            }

            if (e.NewElement == null) return;

            e.NewElement.OnPlay += OnPlay;
            e.NewElement.OnPause += OnPause;
            e.NewElement.OnPlayProgressSegment += OnPlayProgressSegment;
            e.NewElement.OnPlayFrameSegment += OnPlayFrameSegment;

            if (e.NewElement.Animation != null)
            {
                InitAnimationViewForElement(e.NewElement);
            }
        }

        private void OnPlay(object sender, EventArgs e)
        {
            _animationView?.PlayWithCompletion(PlaybackFinishedIfActually);
            Element.IsPlaying = true;
        }

        private void OnPlayProgressSegment(object sender, ProgressSegmentEventArgs e)
        {
            _animationView?.PlayFromProgress(e.From, e.To, PlaybackFinishedIfActually);
            Element.IsPlaying = true;
        }

        private void OnPlayFrameSegment(object sender, FrameSegmentEventArgs e)
        {
            _animationView?.PlayFromFrame(e.From, e.To, PlaybackFinishedIfActually);
            Element.IsPlaying = true;
        }

        private void OnPause(object sender, EventArgs e)
        {
            _animationView?.Pause();
            Element.IsPlaying = false;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Element == null)
                return;

            if (e.PropertyName == AnimationView.AnimationProperty.PropertyName && Element.Animation != null)
            {
                _animationView?.RemoveFromSuperview();
                _animationView?.RemoveGestureRecognizer(_gestureRecognizer);
                InitAnimationViewForElement(Element);
            }

            if (_animationView == null)
                return;

            if (e.PropertyName == AnimationView.ProgressProperty.PropertyName)
            {
                _animationView.AnimationProgress = Element.Progress;
            }

            if (e.PropertyName == AnimationView.LoopProperty.PropertyName)
            {
                _animationView.LoopAnimation = Element.Loop;
            }

            if (e.PropertyName == AnimationView.SpeedProperty.PropertyName)
            {
                _animationView.AnimationSpeed = Element.Speed;
            }
        }

        private void InitAnimationViewForElement(AnimationView theElement)
        {
            //_animationView = new LOTAnimationView(NSUrl.FromFilename(theElement.Animation))
            //{
            //    AutoresizingMask = UIViewAutoresizing.All,
            //    ContentMode = UIViewContentMode.ScaleAspectFit,
            //    LoopAnimation = theElement.Loop,
            //    AnimationSpeed = theElement.Speed
            //};

            _animationView = GetImageSourceBinding(theElement.Animation);

            _animationView.AutoresizingMask = UIViewAutoresizing.All;
            _animationView.ContentMode = UIViewContentMode.ScaleAspectFit;
            _animationView.LoopAnimation = theElement.Loop;
            _animationView.AnimationSpeed = theElement.Speed;

            _gestureRecognizer = new UITapGestureRecognizer(theElement.Click);
            _animationView.AddGestureRecognizer(_gestureRecognizer);

            Element.Duration = TimeSpan.FromMilliseconds(_animationView.AnimationDuration);

            if (theElement.AutoPlay)
            {
                _animationView.PlayWithCompletion(PlaybackFinishedIfActually);
            }

            if (_animationView != null)
            {
                SetNativeControl(_animationView);
                SetNeedsLayout();
            }
        }

        void PlaybackFinishedIfActually(bool animationFinished)
        {
            if (animationFinished)
            {
                Element?.PlaybackFinished();
            }
        }

        internal static LOTAnimationView GetImageSourceBinding(ImageSource source)
        {
            if (source == null)
            {
                return null;
            }

            if (source is UriImageSource uriImageSource)
            {
                var uri = uriImageSource.Uri?.OriginalString;
                if (string.IsNullOrWhiteSpace(uri))
                    return null;

                return new LOTAnimationView(NSUrl.FromString(uriImageSource.Uri.AbsoluteUri));
            }

            if (source is FileImageSource fileImageSource)
            {
                if (string.IsNullOrWhiteSpace(fileImageSource.File))
                    return null;

                return new LOTAnimationView(NSUrl.FromFilename(fileImageSource.File));

                //if (fileImageSource.File.StartsWith("/", StringComparison.InvariantCultureIgnoreCase) && File.Exists(fileImageSource.File))
                //    return new ImageSourceBinding(FFImageLoading.Work.ImageSource.Filepath, fileImageSource.File);

                //return new ImageSourceBinding(FFImageLoading.Work.ImageSource.CompiledResource, fileImageSource.File);
            }

            //if (source is StreamImageSource streamImageSource)
            //{
            //    return new ImageSourceBinding(streamImageSource.Stream);
            //}

            throw new NotImplementedException("ImageSource type not supported");
        }
    }
}
