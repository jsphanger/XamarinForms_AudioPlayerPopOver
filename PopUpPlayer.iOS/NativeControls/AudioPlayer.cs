using UIKit;
using CoreGraphics;
using System.Diagnostics;
using PopUpPlayer.Interfaces;
using PopUpPlayer.iOS.NativeControls;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioPlayer))]
namespace PopUpPlayer.iOS.NativeControls
{
    public class AudioPlayer : UIView, IAudioPlayer 
    {
        private static AudioPlayer _instance;

        private CGRect _originalFrame;
        private CGRect _originalPreviewImageFrame;
        private CGRect _originalTitleFrame;
        private UIFont _originalTitleFont;
        private CGRect _originalSubTitleFrame;
        private UIFont _originalSubTitleFont;
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
        public UILabel TopTitle { get; set; }

        public UILabel TimePlayed { get; set; }
        public UILabel TimeLeft { get; set; }

        public UIImageView NextButton { get; set; }
        public UIImageView PreviousButton { get; set; }
        public UIImageView SkipForwardButton { get; set; }
        public UIImageView SkipBackwardButton { get; set; }

        public UISlider VolumeControl { get; set; }

        public bool IsLargeView { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsNextPrevEnabled { get; set; }
        public bool IsSkipEnabled { get; set; }

        private UITapGestureRecognizer ArrowTappedRecognizer { get; set; }
        private UITapGestureRecognizer PlayerTappedRecognizer { get; set; }
        private UISwipeGestureRecognizer PlayerSwipedRecognizer { get; set; }
        private UITapGestureRecognizer PlayPauseButtonTappedRecognizer { get; set; }
        private UITapGestureRecognizer NextButtonTappedRecognizer { get; set; }
        private UITapGestureRecognizer PreviousButtonTappedRecognizer { get; set; }
        private UITapGestureRecognizer SkipForwardButtonTappedRecognizer { get; set; }
        private UITapGestureRecognizer SkipBackwardButtonTappedRecognizer { get; set; }

        static AudioPlayer() { }

        public static AudioPlayer GetInstance(CGRect frame)
        {
            if (_instance != null)
                return _instance;

            _instance = new AudioPlayer();
            _instance.Frame = frame;
            _instance.ClipsToBounds = false;

            _instance.PlayImage = new UIImage("ButtonPlay");
            _instance.PauseImage = new UIImage("ButtonPause");

            _instance.IsNextPrevEnabled = false;
            _instance.IsSkipEnabled = true;

            _instance.CreatePlayerView();

            return _instance;
        }

        private void CreatePlayerView()
        {
            var miniPlayerHeight = 50.0f;
            var progressBarHeight = 2.0f;
            var titleFontSize = 16.0f;
            var fontSizeOffset = ((miniPlayerHeight / 2.0f) - titleFontSize) / 2.0f; //(container size - fontsize) / 2
            var miniPlayerControlSize = 32.0f;
            var generalControlSpacing = 10.0f;

            PlayerTappedRecognizer = new UITapGestureRecognizer(PlayerTapped);
            PlayerSwipedRecognizer = new UISwipeGestureRecognizer(PlayerSwiped);
            PlayerSwipedRecognizer.Direction = UISwipeGestureRecognizerDirection.Up;

            AddGestureRecognizer(PlayerTappedRecognizer);
            AddGestureRecognizer(PlayerSwipedRecognizer);
            UserInteractionEnabled = true;

            //-- Topbar
            FullPlayerCollapseArrow = new UIImageView(new CGRect(Frame.Width * 0.1f, _safeAreaInsets.Top + generalControlSpacing, 16, 16));
            FullPlayerCollapseArrow.Image = new UIImage("ButtonCollapse");
            FullPlayerCollapseArrow.Alpha = 0;

            TopTitle = new UILabel(new CGRect(0, _safeAreaInsets.Top + generalControlSpacing, UIScreen.MainScreen.Bounds.Width, 25));
            TopTitle.Font = UIFont.SystemFontOfSize(titleFontSize);
            TopTitle.Text = "Currently Playing";
            TopTitle.TextAlignment = UITextAlignment.Center;
            TopTitle.Alpha = 0;

            ArrowTappedRecognizer = new UITapGestureRecognizer(PlayerTapped);
            FullPlayerCollapseArrow.AddGestureRecognizer(ArrowTappedRecognizer);
            FullPlayerCollapseArrow.UserInteractionEnabled = true;

            //-- Preview
            var preview = new UIImage("AlbumArtwork.png");
            PreviewImage = new UIImageView(preview);
            PreviewImage.ContentMode = UIViewContentMode.ScaleAspectFit;
            PreviewImage.Frame = new CGRect(Bounds.X, Bounds.Y + progressBarHeight, miniPlayerHeight, Bounds.Height - progressBarHeight);

            //-- Info
            Title = new UILabel(new CGRect(PreviewImage.Bounds.X + PreviewImage.Bounds.Width + generalControlSpacing, Bounds.Y + fontSizeOffset, Frame.Width - PreviewImage.Bounds.Width - miniPlayerControlSize - generalControlSpacing, miniPlayerHeight / 2.0f));
            Title.Font = UIFont.BoldSystemFontOfSize(titleFontSize);
            Title.Text = "Breakdown - Breaking Benjamin";

            SubTitle = new UILabel(new CGRect(PreviewImage.Bounds.X + PreviewImage.Bounds.Width + generalControlSpacing, Title.Bounds.Bottom, Frame.Width - PreviewImage.Bounds.Width - miniPlayerControlSize - generalControlSpacing, miniPlayerHeight / 2.0f));
            SubTitle.Font = UIFont.SystemFontOfSize(titleFontSize);
            SubTitle.Text = "Aurora";

            Timeline = new UIProgressView(new CGRect(0, 0, Bounds.Width, progressBarHeight));
            Timeline.BackgroundColor = UIColor.Black;
            Timeline.ProgressTintColor = UIColor.Blue;
            Timeline.Progress = .4f;

            TimePlayed = new UILabel(new CGRect(0,0, miniPlayerControlSize, miniPlayerControlSize));
            TimePlayed.Font = UIFont.SystemFontOfSize(12);
            TimePlayed.Text = "1:25";
            TimePlayed.Alpha = 0;

            TimeLeft = new UILabel(new CGRect(0, 0, miniPlayerControlSize, miniPlayerControlSize));
            TimeLeft.Font = UIFont.SystemFontOfSize(12);
            TimeLeft.Text = "-2:10";
            TimeLeft.Alpha = 0;

            //-- Controls
            PlayPauseButton = new UIImageView(PlayImage);
            PlayPauseButton.Frame = new CGRect(Bounds.Width - miniPlayerControlSize - generalControlSpacing, (Bounds.Height - miniPlayerControlSize) / 2.0f, miniPlayerControlSize, miniPlayerControlSize);
            PlayPauseButton.ContentMode = UIViewContentMode.ScaleAspectFit;

            PlayPauseButtonTappedRecognizer = new UITapGestureRecognizer(PlayButtonTapped);
            PlayPauseButton.AddGestureRecognizer(PlayPauseButtonTappedRecognizer);
            PlayPauseButton.UserInteractionEnabled = true;

            if (IsNextPrevEnabled && IsSkipEnabled)
            {
                //** Previous Track
                var prev = new UIImage("ButtonPrevious");
                PreviousButton = new UIImageView(prev);
                PreviousButton.Frame = new CGRect(0, 0, miniPlayerControlSize, miniPlayerControlSize);
                PreviousButton.Alpha = 0;

                PreviousButtonTappedRecognizer = new UITapGestureRecognizer(PreviousButtonTapped);
                PreviousButton.AddGestureRecognizer(PreviousButtonTappedRecognizer);
                PreviousButton.UserInteractionEnabled = true;

                //** Next Track
                var next = new UIImage("ButtonNext");
                NextButton = new UIImageView(next);
                NextButton.Frame = new CGRect(0, 0, miniPlayerControlSize * 2, miniPlayerControlSize * 2);
                NextButton.Alpha = 0;

                NextButtonTappedRecognizer = new UITapGestureRecognizer(NextButtonTapped);
                NextButton.AddGestureRecognizer(NextButtonTappedRecognizer);
                NextButton.UserInteractionEnabled = true;

                AddSubview(PreviousButton);
                AddSubview(NextButton);

                //** Skip Backwards
                var skipBackward = new UIImage("ButtonSkipBack");
                SkipBackwardButton = new UIImageView(skipBackward);
                SkipBackwardButton.Frame = new CGRect(0, 0, miniPlayerControlSize * 2, miniPlayerControlSize * 2);
                SkipBackwardButton.Alpha = 0;

                SkipBackwardButtonTappedRecognizer = new UITapGestureRecognizer(SkipBackwardButtonTapped);
                SkipBackwardButton.AddGestureRecognizer(SkipBackwardButtonTappedRecognizer);
                SkipBackwardButton.UserInteractionEnabled = true;

                //** Skip Forwards
                var skipForward = new UIImage("ButtonSkipForward");
                SkipForwardButton = new UIImageView(skipForward);
                SkipForwardButton.Frame = new CGRect(0, 0, miniPlayerControlSize * 2, miniPlayerControlSize * 2);
                SkipForwardButton.Alpha = 0;

                SkipForwardButtonTappedRecognizer = new UITapGestureRecognizer(SkipForwardButtonTapped);
                SkipForwardButton.AddGestureRecognizer(SkipForwardButtonTappedRecognizer);
                SkipForwardButton.UserInteractionEnabled = true;

                AddSubview(SkipBackwardButton);
                AddSubview(SkipForwardButton);
            }
            else if (IsNextPrevEnabled)
            {
                //** Previous Track
                var prev = new UIImage("ButtonPrevious");
                PreviousButton = new UIImageView(prev);
                PreviousButton.Frame = new CGRect(0, 0, miniPlayerControlSize * 2, miniPlayerControlSize * 2);
                PreviousButton.Alpha = 0;

                PreviousButtonTappedRecognizer = new UITapGestureRecognizer(PreviousButtonTapped);
                PreviousButton.AddGestureRecognizer(PreviousButtonTappedRecognizer);
                PreviousButton.UserInteractionEnabled = true;

                //** Next Track
                var next = new UIImage("ButtonNext");
                NextButton = new UIImageView(next);
                NextButton.Frame = new CGRect(0, 0, miniPlayerControlSize * 2, miniPlayerControlSize * 2);
                NextButton.Alpha = 0;

                NextButtonTappedRecognizer = new UITapGestureRecognizer(NextButtonTapped);
                NextButton.AddGestureRecognizer(NextButtonTappedRecognizer);
                NextButton.UserInteractionEnabled = true;

                AddSubview(PreviousButton);
                AddSubview(NextButton);
            }
            else if (IsSkipEnabled)
            {
                //** Skip Backwards
                var skipBackward = new UIImage("ButtonSkipBack");
                SkipBackwardButton = new UIImageView(skipBackward);
                SkipBackwardButton.Frame = new CGRect(0, 0, miniPlayerControlSize * 2, miniPlayerControlSize * 2);
                SkipBackwardButton.Alpha = 0;

                SkipBackwardButtonTappedRecognizer = new UITapGestureRecognizer(SkipBackwardButtonTapped);
                SkipBackwardButton.AddGestureRecognizer(SkipBackwardButtonTappedRecognizer);
                SkipBackwardButton.UserInteractionEnabled = true;

                //** Skip Forwards
                var skipForward = new UIImage("ButtonSkipForward");
                SkipForwardButton = new UIImageView(skipForward);
                SkipForwardButton.Frame = new CGRect(0, 0, miniPlayerControlSize * 2, miniPlayerControlSize * 2);
                SkipForwardButton.Alpha = 0;

                SkipForwardButtonTappedRecognizer = new UITapGestureRecognizer(SkipForwardButtonTapped);
                SkipForwardButton.AddGestureRecognizer(SkipForwardButtonTappedRecognizer);
                SkipForwardButton.UserInteractionEnabled = true;

                AddSubview(SkipBackwardButton);
                AddSubview(SkipForwardButton);
            }

            AddSubview(TopTitle);
            AddSubview(FullPlayerCollapseArrow);

            AddSubview(PreviewImage);

            AddSubview(Timeline);
            AddSubview(TimePlayed);
            AddSubview(TimeLeft);

            AddSubview(Title);
            AddSubview(SubTitle);

            AddSubview(PlayPauseButton);
            
            _originalFrame = new CGRect(Frame.X, Frame.Y, Frame.Width, Frame.Height);
            _originalPreviewImageFrame = PreviewImage.Frame;
            _originalTitleFrame = Title.Frame;
            _originalTitleFont = Title.Font;
            _originalSubTitleFrame = SubTitle.Frame;
            _originalSubTitleFont = SubTitle.Font;
            _originalTimelineFrame = Timeline.Frame;
            _originalPlayButtonFrame = PlayPauseButton.Frame;
        }
        private void UpdatePlayerView()
        {
            Animate(.3, () => {

                if (!IsLargeView)
                {
                    var framePaddingLeft = Superview.Frame.Width * 0.1f; // Padding left for the frame
                    var previewFrameWidth = Superview.Frame.Width * .8f; // Take up 80% of the overall frame width
                    var itemsThatNeedPadding = 4;
                    var overallSpaceAvailable = Superview.Frame.Height - ( Superview.Frame.Height * (itemsThatNeedPadding / 100.0f) * Subviews.Length ); 

                    //Expand the frame.  Make the miniplayer invisible and the fullplayer visable
                    Frame = new CGRect(Frame.X, 0, Frame.Width, Superview.Frame.Height);

                    //-- Spacing Calculations
                    var previewImagePadding = (overallSpaceAvailable * .07f);
                    var titlePadding = (overallSpaceAvailable * .03f);

                    //-- TopBar
                    FullPlayerCollapseArrow.Alpha = 1;
                    TopTitle.Alpha = 1;

                    //-- Preview
                    PreviewImage.Frame = new CGRect(framePaddingLeft, previewImagePadding + FullPlayerCollapseArrow.Frame.Y + FullPlayerCollapseArrow.Frame.Height, previewFrameWidth, previewFrameWidth);

                    //-- Info
                    Title.Frame = new CGRect(framePaddingLeft, previewImagePadding + PreviewImage.Frame.Y + PreviewImage.Frame.Height, previewFrameWidth, 25);
                    Title.Font = UIFont.BoldSystemFontOfSize(22);

                    SubTitle.Frame = new CGRect(framePaddingLeft, Title.Frame.Y + Title.Frame.Height, previewFrameWidth, 25);
                    SubTitle.Font = UIFont.SystemFontOfSize(16);

                    Timeline.Frame = new CGRect(framePaddingLeft,
                                                titlePadding + SubTitle.Frame.Y + SubTitle.Frame.Height,
                                                previewFrameWidth, 5.0f);

                    TimePlayed.Frame = new CGRect(framePaddingLeft,
                                                  Timeline.Frame.Y + Timeline.Frame.Height,
                                                  40, 40);

                    TimePlayed.Alpha = 1;

                    TimeLeft.Frame = new CGRect(framePaddingLeft + Timeline.Frame.Width - 40,
                                                Timeline.Frame.Y + Timeline.Frame.Height,
                                                40, 40);
                    TimeLeft.Alpha = 1;

                    //-- Controls
                    var playButtonHeight = PlayPauseButton.Frame.Height * 2; // 2x the current size
                    var otherButtonHeight = PlayPauseButton.Frame.Height;
                    PlayPauseButton.Frame = new CGRect(framePaddingLeft + (previewFrameWidth - playButtonHeight) / 2.0f,
                                                       TimeLeft.Frame.Y + TimeLeft.Frame.Height + titlePadding,
                                                       playButtonHeight, playButtonHeight);

                    if (IsNextPrevEnabled && IsSkipEnabled)
                    {
                        //** Previous Track
                        PreviousButton.Frame = new CGRect(framePaddingLeft + ((PlayPauseButton.Frame.X - framePaddingLeft) / 2.0f),
                                                          PlayPauseButton.Frame.Y + (otherButtonHeight / 2.0f),
                                                          otherButtonHeight, otherButtonHeight);
                        PreviousButton.Alpha = 1;

                        //** Skip Backwards
                        SkipBackwardButton.Frame = new CGRect(framePaddingLeft,
                                                          PlayPauseButton.Frame.Y + (otherButtonHeight / 2.0f),
                                                          otherButtonHeight, otherButtonHeight);
                        SkipBackwardButton.Alpha = 1;


                        //** Next Track
                        NextButton.Frame = new CGRect(PlayPauseButton.Frame.X + playButtonHeight + (PlayPauseButton.Frame.X - PreviousButton.Frame.X - otherButtonHeight),
                                                      PlayPauseButton.Frame.Y + (otherButtonHeight / 2.0f),
                                                      otherButtonHeight, otherButtonHeight);
                        NextButton.Alpha = 1;

                        //** Skip Forwards
                        SkipForwardButton.Frame = new CGRect(PlayPauseButton.Frame.X + playButtonHeight + (PlayPauseButton.Frame.X - SkipBackwardButton.Frame.X - otherButtonHeight),
                                                      PlayPauseButton.Frame.Y + (otherButtonHeight / 2.0f),
                                                      otherButtonHeight, otherButtonHeight);
                        SkipForwardButton.Alpha = 1;
                    }
                    else if (IsNextPrevEnabled)
                    {
                        //** Previous Track
                        PreviousButton.Frame = new CGRect(framePaddingLeft + ((PlayPauseButton.Frame.X - framePaddingLeft) / 2.0f) - (otherButtonHeight / 2.0f),
                                                          PlayPauseButton.Frame.Y + (otherButtonHeight / 2.0f),
                                                          otherButtonHeight, otherButtonHeight);
                        PreviousButton.Alpha = 1;

                        //** Next Track
                        NextButton.Frame = new CGRect(PlayPauseButton.Frame.X + playButtonHeight + (PlayPauseButton.Frame.X - PreviousButton.Frame.X - otherButtonHeight),
                                                      PlayPauseButton.Frame.Y + (otherButtonHeight / 2.0f),
                                                      otherButtonHeight, otherButtonHeight);
                        NextButton.Alpha = 1;
                    }
                    else if (IsSkipEnabled)
                    {
                        //** Skip Backwards
                        SkipBackwardButton.Frame = new CGRect(framePaddingLeft + ((PlayPauseButton.Frame.X - framePaddingLeft) / 2.0f) - (otherButtonHeight / 2.0f),
                                                          PlayPauseButton.Frame.Y + (otherButtonHeight / 2.0f),
                                                          otherButtonHeight, otherButtonHeight);
                        SkipBackwardButton.Alpha = 1;

                        //** Skip Forwards
                        SkipForwardButton.Frame = new CGRect(PlayPauseButton.Frame.X + playButtonHeight + (PlayPauseButton.Frame.X - SkipBackwardButton.Frame.X - otherButtonHeight),
                                                      PlayPauseButton.Frame.Y + (otherButtonHeight / 2.0f),
                                                      otherButtonHeight, otherButtonHeight);
                        SkipForwardButton.Alpha = 1;
                    }


                    //remove any recognizers
                    RemoveGestureRecognizer(PlayerTappedRecognizer);
                    PlayerSwipedRecognizer.Direction = UISwipeGestureRecognizerDirection.Down;
                }
                else
                {
                    //Reduce back down to miniplayer
                    Frame = _originalFrame;
                    PreviewImage.Frame = _originalPreviewImageFrame;
                    Title.Frame = _originalTitleFrame;
                    Title.Font = _originalTitleFont;
                    SubTitle.Frame = _originalSubTitleFrame;
                    SubTitle.Font = _originalSubTitleFont;
                    Timeline.Frame = _originalTimelineFrame;
                    PlayPauseButton.Frame = _originalPlayButtonFrame;

                    FullPlayerCollapseArrow.Alpha = 0;
                    TopTitle.Alpha = 0;

                    if (IsNextPrevEnabled && IsSkipEnabled)
                    {
                        PreviousButton.Alpha = 0;
                        NextButton.Alpha = 0;
                        SkipBackwardButton.Alpha = 0;
                        SkipForwardButton.Alpha = 0;
                    }
                    else if (IsNextPrevEnabled)
                    {
                        PreviousButton.Alpha = 0;
                        NextButton.Alpha = 0;
                    }
                    else if (IsSkipEnabled)
                    {
                        SkipBackwardButton.Alpha = 0;
                        SkipForwardButton.Alpha = 0;
                    }

                    AddGestureRecognizer(PlayerTappedRecognizer);
                    PlayerSwipedRecognizer.Direction = UISwipeGestureRecognizerDirection.Up;
                }

                IsLargeView = !IsLargeView;
            });
        }

        public void ShowTrack()
        {
            _instance.Title.Text = "New Track...";
        }

        void PlayButtonTapped(UITapGestureRecognizer recognizer)
        {
            if (IsPlaying)
            {
                //image should be play
                Debug.WriteLine("Pause button tapped.");
                PlayPauseButton.Image = PlayImage;
            }
            else
            {
                //image should be pause
                Debug.WriteLine("Play button tapped.");
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
        void PlayerSwiped(UISwipeGestureRecognizer recognizer)
        {
            Debug.WriteLine("Player swipped.");

            UpdatePlayerView();
        }

        void NextButtonTapped(UITapGestureRecognizer recognizer)
        {
            Debug.WriteLine("Next button tapped.");
        }
        void PreviousButtonTapped(UITapGestureRecognizer recognizer)
        {
            Debug.WriteLine("Previous button tapped.");
        }
        void SkipForwardButtonTapped(UITapGestureRecognizer recognizer)
        {
            Debug.WriteLine("Skip forward tapped");
        }
        void SkipBackwardButtonTapped(UITapGestureRecognizer recognizer)
        {
            Debug.WriteLine("Skip backwards tapped");
        }
    }
}
