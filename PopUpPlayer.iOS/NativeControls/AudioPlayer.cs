using System;
using UIKit;
using CoreGraphics;
using System.Diagnostics;

namespace PopUpPlayer.iOS.CustomRenderers
{
    public class AudioPlayer : UIView
    {
        private CGRect _originalFrame;
        private CGRect _originalPreviewImageFrame;
        private CGRect _originalTitleFrame;
        private CGRect _originalSubTitleFrame;
        private CGRect _originalTimelineFrame;
        private CGRect _originalPlayButtonFrame;

        private UIEdgeInsets _safeAreaInsets { get { return UIApplication.SharedApplication.Windows[0].SafeAreaInsets; } }

        //-- Items needed for small view and large view
        public UIImageView PreviewImage { get; set; }

        public UIImageView PlayPauseButton { get; set; }
        public UIImage PlayImage { get; set; }
        public UIImage PauseImage { get; set; }

        public UILabel Title { get; set; }
        public UILabel SubTitle { get; set; }
        public UIProgressView Timeline { get; set; }

        //-- Items needed for large view
        public UIImageView FullPlayerCollapseArrow { get; set; }
        public UILabel TimePlayed { get; set; }
        public UILabel TimeLeft { get; set; }
        public UIImageView SkipForward { get; set; }
        public UIImageView SkipBackward { get; set; }
        public UISlider VolumeControl { get; set; }

        public bool IsLargeView { get; set; }
        public bool IsPlaying { get; set; }

        public AudioPlayer() {

            Frame = UIScreen.MainScreen.Bounds;
            ClipsToBounds = false;

            PlayImage = new UIImage("ButtonPlay");
            PauseImage = new UIImage("ButtonPause");

            CreatePlayerView();
        }
        public AudioPlayer(CGRect frame)
        {
            Frame = frame;
            ClipsToBounds = false;

            PlayImage = new UIImage("ButtonPlay");
            PauseImage = new UIImage("ButtonPause");

            CreatePlayerView();
        }

        public void CreatePlayerView()
        {
            //Set overall properties
            BackgroundColor = UIColor.FromRGB(247, 247, 247);

            UITapGestureRecognizer playerTappedRecognizer = new UITapGestureRecognizer(PlayerTapped);
            AddGestureRecognizer(playerTappedRecognizer);
            UserInteractionEnabled = true;

            //-- Add Collapsible Arrow (full player view)
            FullPlayerCollapseArrow = new UIImageView(new CGRect(Frame.Width * 0.1f, _safeAreaInsets.Top, 32, 32));
            FullPlayerCollapseArrow.Image = new UIImage("ButtonCollapse");

            UITapGestureRecognizer arrowTappedRecognizer = new UITapGestureRecognizer(PlayerTapped);
            FullPlayerCollapseArrow.AddGestureRecognizer(arrowTappedRecognizer);
            FullPlayerCollapseArrow.UserInteractionEnabled = true;

            FullPlayerCollapseArrow.Alpha = 0;

            AddSubview(FullPlayerCollapseArrow);

            //-- Create progress bar
            Timeline = new UIProgressView(new CGRect(0, 0, Bounds.Width, 5.0f));
            Timeline.BackgroundColor = UIColor.Black;
            Timeline.ProgressTintColor = UIColor.Blue;
            Timeline.Progress = .4f;

            //-- Create a preview image
            var preview = new UIImage("AlbumArtwork.png");
            PreviewImage = new UIImageView(preview);
            PreviewImage.Frame = new CGRect(Bounds.X, Bounds.Y + Timeline.Bounds.Height, 50, Bounds.Height - Timeline.Bounds.Height);

            //-- Create a play button
            PlayPauseButton = new UIImageView(PlayImage);
            PlayPauseButton.Frame = new CGRect(Bounds.Width - 50, (Bounds.Height - 32) / 2.0f, 32, 32);

            UITapGestureRecognizer playTapRecognizer = new UITapGestureRecognizer(PlayButtonTapped);
            PlayPauseButton.AddGestureRecognizer(playTapRecognizer);
            PlayPauseButton.UserInteractionEnabled = true;

            //-- Title
            Title = new UILabel(new CGRect(PreviewImage.Bounds.X + PreviewImage.Bounds.Width + 10, Bounds.Y, Frame.Width - PreviewImage.Bounds.Width - PlayPauseButton.Bounds.Width - 20, 25));
            Title.Text = "Breakdown - Breaking Benjamin";

            //-- Subtitle
            SubTitle = new UILabel(new CGRect(PreviewImage.Bounds.X + PreviewImage.Bounds.Width + 10, Bounds.Height / 2.0f, Frame.Width - PreviewImage.Bounds.Width - PlayPauseButton.Bounds.Width - 20, 25));
            SubTitle.Text = "Aurora";

            AddSubview(Timeline);
            AddSubview(PreviewImage);
            AddSubview(Title);
            AddSubview(SubTitle);
            AddSubview(PlayPauseButton);

            _originalFrame = new CGRect(Frame.X, Frame.Y, Frame.Width, Frame.Height);
            _originalPreviewImageFrame = PreviewImage.Frame;
            _originalTitleFrame = Title.Frame;
            _originalSubTitleFrame = SubTitle.Frame;
            _originalTimelineFrame = Timeline.Frame;
            _originalPlayButtonFrame = PlayPauseButton.Frame;
        }

        public void UpdatePlayerView()
        {
            Animate(.5, () => {

                if (!IsLargeView)
                {
                    var frameLeft = Frame.Width * 0.1f;
                    var previewFrameWidth = Frame.Width * .8f;
                    var playButtonHeight = 96.0f;

                    //Expand the frame.  Make the miniplayer invisible and the fullplayer visable
                    Frame = new CGRect(Frame.X, 0, Frame.Width, Superview.Frame.Height);
                    
                    PreviewImage.Frame = new CGRect(frameLeft, FullPlayerCollapseArrow.Frame.Y + FullPlayerCollapseArrow.Frame.Height, previewFrameWidth, previewFrameWidth);
                    Title.Frame = new CGRect(frameLeft, PreviewImage.Frame.Y + PreviewImage.Frame.Height, previewFrameWidth, 25);
                    SubTitle.Frame = new CGRect(frameLeft, Title.Frame.Y + Title.Frame.Height, previewFrameWidth, 25);
                    Timeline.Frame = new CGRect(frameLeft, SubTitle.Frame.Y + SubTitle.Frame.Height, previewFrameWidth, 5.0f);
                    PlayPauseButton.Frame = new CGRect( frameLeft + (previewFrameWidth - playButtonHeight) / 2.0f, Timeline.Frame.Y + Timeline.Frame.Height + 25, playButtonHeight, playButtonHeight);

                    FullPlayerCollapseArrow.Alpha = 1;
                }
                else
                {
                    //Reduce back down to miniplayer
                    Frame = _originalFrame;
                    PreviewImage.Frame = _originalPreviewImageFrame;
                    Title.Frame = _originalTitleFrame;
                    SubTitle.Frame = _originalSubTitleFrame;
                    Timeline.Frame = _originalTimelineFrame;
                    PlayPauseButton.Frame = _originalPlayButtonFrame;

                    FullPlayerCollapseArrow.Alpha = 0;
                }

                IsLargeView = !IsLargeView;
            });
        }

        void PlayButtonTapped(UITapGestureRecognizer recognizer)
        {
            Debug.WriteLine("Play button tapped.");

            if (IsPlaying)
            {
                //image should be play
                PlayPauseButton.Image = PlayImage;
            }
            else
            {
                //image should be pause
                PlayPauseButton.Image = PauseImage;
            }

            IsPlaying = !IsPlaying;
        }
        void PlayerTapped(UITapGestureRecognizer recognizer)
        {
            Debug.WriteLine("Player tapped.");

            //Push our modal view!
            UpdatePlayerView();
            
        }
    }
}
