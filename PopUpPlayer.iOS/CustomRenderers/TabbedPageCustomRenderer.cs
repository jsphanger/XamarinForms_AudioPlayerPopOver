using System;
using PopUpPlayer.iOS.CustomRenderers;
using PopUpPlayer.iOS.NativeControls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly:ExportRenderer(typeof(TabbedPage), typeof(TabbedPageCustomRenderer))]
namespace PopUpPlayer.iOS.CustomRenderers
{
    public class TabbedPageCustomRenderer : TabbedRenderer
    {
        private AudioPlayer _audioPlayer;

        public TabbedPageCustomRenderer() { }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if(e.NewElement != null)
                e.NewElement.SizeChanged += TabbedPage_SizeChanged;

            if (e.OldElement != null)
                e.OldElement.SizeChanged -= TabbedPage_SizeChanged;
        }

        private void TabbedPage_SizeChanged(object sender, EventArgs e)
        {
            if (_audioPlayer == null)
            {
                var tabBarFrame = TabBar.Frame;
                var screenBounds = UIScreen.MainScreen.Bounds;

                var rect = new CoreGraphics.CGRect(0, tabBarFrame.Y - 50, screenBounds.Width, 50);
                _audioPlayer = AudioPlayer.GetInstance(rect);
                _audioPlayer.BackgroundColor = UIColor.FromRGB(247, 247, 247);

                View.AddSubview(_audioPlayer);
            }
        }
    }
}
