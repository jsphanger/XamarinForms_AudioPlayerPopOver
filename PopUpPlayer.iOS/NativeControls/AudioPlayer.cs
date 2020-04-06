using System;
using UIKit;
using CoreGraphics;
using System.Diagnostics;

namespace PopUpPlayer.iOS.CustomRenderers
{
    public class AudioPlayer : UIView
    {
        public override CGRect Frame { get => base.Frame; set => base.Frame = value; }

        public UIView PlayerView { get; set; }

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
            PlayerView = new UIView(new CGRect(0, 0, this.Frame.Width, this.Frame.Height));
            PlayerView.Layer.BorderWidth = 1;
            PlayerView.Layer.BorderColor = UIColor.DarkGray.CGColor;
            PlayerView.BackgroundColor = UIColor.LightGray;

            UITapGestureRecognizer playerTappedRecognizer = new UITapGestureRecognizer(PlayerTapped);
            PlayerView.AddGestureRecognizer(playerTappedRecognizer);
            PlayerView.UserInteractionEnabled = true;

            //Create a play button
            var image = new UIImage("xamarin_logo.png");
            PlayPauseButton = new UIImageView(image);
            PlayPauseButton.Frame = new CGRect(PlayerView.Bounds.X + 20, PlayerView.Bounds.Y, 50, 50);

            UITapGestureRecognizer playTapRecognizer = new UITapGestureRecognizer(PlayButtonTapped);
            PlayPauseButton.AddGestureRecognizer(playTapRecognizer);
            PlayPauseButton.UserInteractionEnabled = true;

            PlayerView.AddSubview(PlayPauseButton);

            //Add our mini view and playbutton
            AddSubview(PlayerView);
        }

        public void UpdatePlayerView()
        {
            UIView.Animate(1, () => {

                var heightChange = 50;

                if (!IsLargeView)
                    PlayerView.Frame = new CGRect(PlayerView.Frame.X, PlayerView.Frame.Y - heightChange, PlayerView.Frame.Width, PlayerView.Frame.Height + heightChange);
                else
                    PlayerView.Frame = new CGRect(PlayerView.Frame.X, PlayerView.Frame.Y + heightChange, PlayerView.Frame.Width, PlayerView.Frame.Height - heightChange);

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
