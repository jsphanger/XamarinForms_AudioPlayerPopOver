using System;
using UIKit;
using CoreGraphics;
using System.Diagnostics;

namespace PopUpPlayer.iOS.CustomRenderers
{
    public class AudioPlayer : UIView
    {
        //-- Items needed for small view
        public UIImageView PreviewImage { get; set; }
        public UIImageView PlayPauseButton { get; set; }
        public UILabel Title { get; set; }
        public UILabel SubTitle { get; set; }

        //-- Items needed for larger view
        public UIProgressView Timeline { get; set; }
        public UILabel TimePlayed { get; set; }
        public UILabel TimeLeft { get; set; }
        public UIImageView SkipForward { get; set; }
        public UIImageView SkipBackward { get; set; }
        public UISlider VolumeControl { get; set; }

        public bool IsLargeView { get; set; }

        public AudioPlayer() {
            Frame = UIScreen.MainScreen.Bounds;
            ClipsToBounds = false;

            DisplayMiniPlayerView();
        }
        public AudioPlayer(CGRect frame)
        {
            Frame = frame;
            ClipsToBounds = false;

            DisplayMiniPlayerView();
        }

        public void DisplayMiniPlayerView()
        {
            //Define our mini player view
            Layer.BorderWidth = 1;
            Layer.BorderColor = UIColor.DarkGray.CGColor;
            BackgroundColor = UIColor.LightGray;

            UITapGestureRecognizer playerTappedRecognizer = new UITapGestureRecognizer(PlayerTapped);
            AddGestureRecognizer(playerTappedRecognizer);
            UserInteractionEnabled = true;

            //Create a play button
            var image = new UIImage("xamarin_logo.png");
            PlayPauseButton = new UIImageView(image);
            PlayPauseButton.Frame = new CGRect(Bounds.X + 20, Bounds.Y, 50, 50);

            UITapGestureRecognizer playTapRecognizer = new UITapGestureRecognizer(PlayButtonTapped);
            PlayPauseButton.AddGestureRecognizer(playTapRecognizer);
            PlayPauseButton.UserInteractionEnabled = true;

            AddSubview(PlayPauseButton);
        }

        public void UpdatePlayerView()
        {
            Animate(1, () => {

                var heightChange = 50;

                if (!IsLargeView)
                    Frame = new CGRect(Frame.X, Frame.Y - heightChange, Frame.Width, Frame.Height + heightChange);
                else
                    Frame = new CGRect(Frame.X, Frame.Y + heightChange, Frame.Width, Frame.Height - heightChange);

                IsLargeView = !IsLargeView;
            });
        }

        void PlayButtonTapped(UITapGestureRecognizer recognizer)
        {
            Debug.WriteLine("Play button tapped.");
        }
        void PlayerTapped(UITapGestureRecognizer recognizer)
        {
            Debug.WriteLine("Player tapped.");

            //Push our modal view!
            UpdatePlayerView();
            
        }
    }
}
