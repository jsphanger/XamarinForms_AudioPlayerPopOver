using System;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using PopUpPlayer.Droid.NativeControls;
using PopUpPlayer.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer_Droid))]
namespace PopUpPlayer.Droid.Renderers
{
    /// <summary>
    /// Overrides implementation of all tabbed pages in Xamarin.Forms
    /// </summary>
    /// <remarks>
    /// This should be isolated to only Render custom Tabbed Pages so it doesn't
    /// affect ALL tabbed pages.
    /// </remarks>
    public class TabbedPageRenderer_Droid : TabbedPageRenderer
    {
        /// <summary>
        /// Reference to our AudioPlayer View
        /// </summary>
        private Android.Views.View _audioPlayerView;

        /// <summary>
        /// Reference to the RelativeLayout added in the Parent Renderer. We intercept this in <see cref="ViewGroup.OnViewAdded(Android.Views.View)"/>
        /// </summary>
        private Android.Widget.RelativeLayout _relativeLayout;

        public TabbedPageRenderer_Droid(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);

            // This method can be called multiple times, but we only want to inflate the
            // audio player view and add it to the relative layout once.
            if (_audioPlayerView == null)
            {
                // Since it is null, we assume it has not been created and not added
                var inflater = Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;

                AudioPlayer audioPlayer = new AudioPlayer(Android.App.Application.Context);
                audioPlayer.Title = "Breakdown - Breaking Benjamin";
                audioPlayer.SubTitle = "Aurora";

                _audioPlayerView = audioPlayer.PlayerView;

                if (_relativeLayout != null)
                {
                    // Iterate through children of the RL and find the BottomNavigationView.
                    for (var i = 0; i < _relativeLayout.ChildCount; i++)
                    {
                        // if, and only if, we find a BottomNavigationView, we'll add our AudioPlayer above it.
                        var v = _relativeLayout.GetChildAt(i);
                        if (v is BottomNavigationView bnv)
                        {
                            var audioLayoutParams = new Android.Widget.RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
                            audioLayoutParams.AddRule(Android.Widget.LayoutRules.Above, bnv.Id);
                            _relativeLayout.AddView(_audioPlayerView, audioLayoutParams);
                            break;
                        }
                    }
                }
            }
        }

        public override void OnViewAdded(Android.Views.View child)
        {
            base.OnViewAdded(child);
            // we'll override the OnViewAdded and 'listen' for our Parent
            // to add the RelativeLayout.
            if (child is Android.Widget.RelativeLayout rl)
            {
                // Added, intercepted. Let's capture a reference.
                _relativeLayout = rl;
            }
        }
    }
}
