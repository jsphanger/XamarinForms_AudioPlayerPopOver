using System;
using System.Diagnostics;
using Android.Content;
using Android.Transitions;
using Android.Views;
using Android.Widget;
using Android.Util;
using PopUpPlayer.Droid.NativeControls;
using PopUpPlayer.Interfaces;
using Xamarin.Forms;
using static Android.Views.View;
using static Android.Views.GestureDetector;

[assembly: Dependency(typeof(AudioPlayer))]
namespace PopUpPlayer.Droid.NativeControls
{
    public class AudioPlayer : IAudioPlayer
    {
        private static AudioPlayer _instance;

        //Do these need to be public?
        public Context Context { get; private set; }
        public Android.Views.View PlayerView { get; private set; }

        //Our player controls
        private Android.Widget.RelativeLayout _playerLayout;
        private TextView _title, _subtitle, _topTitle, _timePlayed, _timeRemaining;
        private ImageView _albumArtwork;
        private Android.Widget.ImageButton _playPauseButton, _collapse, _previousTrack, _nextTrack, _skipBackwards, _skipForwards;
        private Android.Widget.ProgressBar _progress;

        //Exposed properties for manipulation by view model
        public string Title { get { return _title.Text; } set { _title.Text = value; }}
        public string SubTitle { get { return _subtitle.Text; } set { _subtitle.Text = value; } }
        public string TopTitle { get { return _topTitle.Text; } set { _topTitle.Text = value; } }
        public string TimePlayed { get { return _timePlayed.Text; } set { _timePlayed.Text = value; } }
        public string TimeRemaining { get { return _timeRemaining.Text; } set { _timeRemaining.Text = value; } }

        public bool IsPlaying { get; set; }
        private bool IsLargeView { get; set; }

        static AudioPlayer() { }

        public static AudioPlayer GetInstance(Context context = null)
        {
            if (_instance != null)
                return _instance;

            if (context == null)
                throw new Exception("Please provide a context.");

            _instance = new AudioPlayer();
            _instance.Context = context;

            var inflater = _instance.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            _instance.PlayerView = inflater.Inflate(Resource.Layout.AudioPlayer, null);

            //-- Init the main layout
            _instance._playerLayout = _instance.PlayerView.FindViewById<Android.Widget.RelativeLayout>(Resource.Id.rlPlayer);
            _instance._playerLayout.SetBackgroundColor(Android.Graphics.Color.Rgb(247, 247, 247));

            //-- Init the widgets in the layou
            _instance._title = _instance.PlayerView.FindViewById<TextView>(Resource.Id.tvTitle);
            _instance._subtitle = _instance.PlayerView.FindViewById<TextView>(Resource.Id.tvSubtitle);
            _instance._topTitle = _instance.PlayerView.FindViewById<TextView>(Resource.Id.tvTopTitle);
            _instance._timePlayed = _instance.PlayerView.FindViewById<TextView>(Resource.Id.tvTimePlayed);
            _instance._timeRemaining = _instance.PlayerView.FindViewById<TextView>(Resource.Id.tvTimeRemaining);
            _instance._albumArtwork = _instance.PlayerView.FindViewById<ImageView>(Resource.Id.ivAlbumArt);
            _instance._progress = _instance.PlayerView.FindViewById<Android.Widget.ProgressBar>(Resource.Id.pbProgress);
            _instance._playPauseButton = _instance.PlayerView.FindViewById<Android.Widget.ImageButton>(Resource.Id.ibPlayPause);
            _instance._previousTrack = _instance.PlayerView.FindViewById<Android.Widget.ImageButton>(Resource.Id.ibPreviousTrack);
            _instance._nextTrack = _instance.PlayerView.FindViewById<Android.Widget.ImageButton>(Resource.Id.ibNextTrack);
            _instance._skipBackwards = _instance.PlayerView.FindViewById<Android.Widget.ImageButton>(Resource.Id.ibSkipBackward);
            _instance._skipForwards = _instance.PlayerView.FindViewById<Android.Widget.ImageButton>(Resource.Id.ibSkipForward);
            _instance._collapse = _instance.PlayerView.FindViewById<Android.Widget.ImageButton>(Resource.Id.ibCollapse);

            //-- setup our listeners
            _instance._playerLayout.SetOnClickListener(new TapGestureRecognizer(_instance.UpdatePlayerView));
            _instance._collapse.SetOnClickListener(new TapGestureRecognizer(_instance.UpdatePlayerView));
            _instance._playPauseButton.SetOnClickListener(new TapGestureRecognizer(_instance.PlayButtonTapped));
            _instance._skipBackwards.SetOnClickListener(new TapGestureRecognizer(_instance.SkipBackwardsTapped));
            _instance._previousTrack.SetOnClickListener(new TapGestureRecognizer(_instance.PreviousTrackTapped));
            _instance._skipForwards.SetOnClickListener(new TapGestureRecognizer(_instance.SkipForwardsTapped));
            _instance._nextTrack.SetOnClickListener(new TapGestureRecognizer(_instance.NextTrackTapped));

            return _instance;
        }

        public void SetAlbumArtwork(string localResource)
        {
            var imageID = Context.Resources.GetIdentifier(localResource, "drawable", this.Context.PackageName);
            _albumArtwork.SetImageResource(imageID);
        }
        public void SetAlbumArtwork(Uri webURI)
        {
            //Set Album Artwork for uri
        }

        public void ShowTrack()
        {
            _instance.Title = "New Track...";
        }

        public void PlayButtonTapped()
        {
            if (IsPlaying)
            {
                //image should be play
                Debug.WriteLine("Pause button tapped.");
                _playPauseButton.SetImageResource(Resource.Drawable.ButtonPlay);
            }
            else
            {
                //image should be pause
                Debug.WriteLine("Play button tapped.");
                _playPauseButton.SetImageResource(Resource.Drawable.ButtonPause);
            }

            IsPlaying = !IsPlaying;
        }
        public void SkipBackwardsTapped()
        {
            Debug.WriteLine("Skip Backwards Tapped");
        }
        public void SkipForwardsTapped()
        {
            Debug.WriteLine("Skip Forward Tapped");
        }
        public void PreviousTrackTapped()
        {
            Debug.WriteLine("Previous Track Tapped");
        }
        public void NextTrackTapped()
        {
            Debug.WriteLine("Next Track Tapped");
        }

        //Main method used for transitioning between full screen and mini player regardless if it's from a swipe or click.        
        private void UpdatePlayerView()
        {
            Debug.WriteLine("Resizing player");

            TransitionManager.BeginDelayedTransition((ViewGroup)_instance._playerLayout.RootView);

            if (!IsLargeView)
            {
                //adjust the height of our parent view
                ViewGroup.LayoutParams frameParams = _instance._playerLayout.LayoutParameters;
                frameParams.Width = WindowManagerLayoutParams.MatchParent;
                frameParams.Height = WindowManagerLayoutParams.MatchParent;
                _instance._playerLayout.LayoutParameters = frameParams;

                //reposition the visible elements
                ViewGroup.LayoutParams albumArtworkParams = _instance._albumArtwork.LayoutParameters;
                Android.Widget.RelativeLayout.LayoutParams albumArtworkRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._albumArtwork.LayoutParameters;
                albumArtworkRelativeParams.RemoveRule(LayoutRules.AlignParentLeft);
                albumArtworkRelativeParams.AddRule(LayoutRules.Below, _instance._topTitle.Id);
                albumArtworkRelativeParams.LeftMargin = GetDPValue(20);
                albumArtworkRelativeParams.RightMargin = GetDPValue(20);
                albumArtworkParams.Width = _instance._playerLayout.Width;
                albumArtworkParams.Height = albumArtworkParams.Width;

                Android.Widget.RelativeLayout.LayoutParams titleRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._title.LayoutParameters;
                titleRelativeParams.RemoveRule(LayoutRules.LeftOf);
                titleRelativeParams.RemoveRule(LayoutRules.RightOf);
                titleRelativeParams.AddRule(LayoutRules.AlignBottom, _instance._albumArtwork.Id);
                titleRelativeParams.LeftMargin = GetDPValue(20);
                titleRelativeParams.RightMargin = GetDPValue(20);

                Android.Widget.RelativeLayout.LayoutParams subTitleRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._subtitle.LayoutParameters;
                subTitleRelativeParams.RemoveRule(LayoutRules.LeftOf);
                subTitleRelativeParams.RemoveRule(LayoutRules.RightOf);
                subTitleRelativeParams.LeftMargin = GetDPValue(20);
                subTitleRelativeParams.RightMargin = GetDPValue(20);

                Android.Widget.RelativeLayout.LayoutParams progressRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._progress.LayoutParameters;
                progressRelativeParams.RemoveRule(LayoutRules.AlignParentTop);
                progressRelativeParams.AddRule(LayoutRules.Below, _instance._subtitle.Id);
                progressRelativeParams.TopMargin = GetDPValue(10);
                progressRelativeParams.LeftMargin = GetDPValue(20);
                progressRelativeParams.RightMargin = GetDPValue(20);

                Android.Widget.RelativeLayout.LayoutParams playRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._playPauseButton.LayoutParameters;
                playRelativeParams.RemoveRule(LayoutRules.CenterVertical);
                playRelativeParams.RemoveRule(LayoutRules.AlignParentRight);
                playRelativeParams.AddRule(LayoutRules.Below, _instance._timeRemaining.Id);
                playRelativeParams.AddRule(LayoutRules.CenterInParent);
                playRelativeParams.TopMargin = GetDPValue(20);
                playRelativeParams.RightMargin = GetDPValue(0);
                playRelativeParams.LeftMargin = GetDPValue(0);
                _instance._playPauseButton.ScaleX = 2;
                _instance._playPauseButton.ScaleY = 2;

                //turn the other components visible
                _instance._collapse.Visibility = ViewStates.Visible;
                _instance._topTitle.Visibility = ViewStates.Visible;
                _instance._timePlayed.Visibility = ViewStates.Visible;
                _instance._timeRemaining.Visibility = ViewStates.Visible;

                _instance._skipBackwards.Visibility = ViewStates.Visible;
                _instance._previousTrack.Visibility = ViewStates.Visible;
                _instance._nextTrack.Visibility = ViewStates.Visible;
                _instance._skipForwards.Visibility = ViewStates.Visible;

                //remove the click listener from the player
                _instance._playerLayout.SetOnClickListener(null);
            }
            else
            {
                //reduce the height of our parent view
                ViewGroup.LayoutParams frameParams = _instance._playerLayout.LayoutParameters;
                frameParams.Width = WindowManagerLayoutParams.MatchParent;
                frameParams.Height = GetDPValue(50);
                _instance._playerLayout.LayoutParameters = frameParams;

                //reposition the elements that need to stay
                ViewGroup.LayoutParams albumArtworkParams = _instance._albumArtwork.LayoutParameters;
                Android.Widget.RelativeLayout.LayoutParams albumArtworkRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._albumArtwork.LayoutParameters;
                albumArtworkRelativeParams.RemoveRule(LayoutRules.Below);
                albumArtworkRelativeParams.AddRule(LayoutRules.AlignParentLeft);
                albumArtworkRelativeParams.TopMargin = GetDPValue(5);
                albumArtworkRelativeParams.LeftMargin = GetDPValue(0);
                albumArtworkRelativeParams.RightMargin = GetDPValue(10);
                albumArtworkParams.Width = GetDPValue(45);
                albumArtworkParams.Height = GetDPValue(45);

                Android.Widget.RelativeLayout.LayoutParams titleRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._title.LayoutParameters;
                titleRelativeParams.RemoveRule(LayoutRules.AlignBottom);
                titleRelativeParams.AddRule(LayoutRules.RightOf, _instance._albumArtwork.Id);
                titleRelativeParams.AddRule(LayoutRules.LeftOf, _instance._playPauseButton.Id);
                titleRelativeParams.LeftMargin = GetDPValue(0);
                titleRelativeParams.RightMargin = GetDPValue(0);

                Android.Widget.RelativeLayout.LayoutParams subTitleRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._subtitle.LayoutParameters;
                subTitleRelativeParams.AddRule(LayoutRules.RightOf, _instance._albumArtwork.Id);
                subTitleRelativeParams.AddRule(LayoutRules.LeftOf, _instance._playPauseButton.Id);
                subTitleRelativeParams.LeftMargin = GetDPValue(0);
                subTitleRelativeParams.RightMargin = GetDPValue(0);

                Android.Widget.RelativeLayout.LayoutParams progressRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._progress.LayoutParameters;
                progressRelativeParams.RemoveRule(LayoutRules.Below);
                progressRelativeParams.AddRule(LayoutRules.AlignParentTop);
                progressRelativeParams.TopMargin = GetDPValue(0);
                progressRelativeParams.LeftMargin = GetDPValue(0);
                progressRelativeParams.RightMargin = GetDPValue(0);

                Android.Widget.RelativeLayout.LayoutParams playRelativeParams = (Android.Widget.RelativeLayout.LayoutParams)_instance._playPauseButton.LayoutParameters;
                playRelativeParams.RemoveRule(LayoutRules.Below);
                playRelativeParams.RemoveRule(LayoutRules.CenterInParent);
                playRelativeParams.AddRule(LayoutRules.CenterVertical);
                playRelativeParams.AddRule(LayoutRules.AlignParentRight);
                playRelativeParams.TopMargin = GetDPValue(0);
                playRelativeParams.RightMargin = GetDPValue(10);
                playRelativeParams.LeftMargin = GetDPValue(10);
                _instance._playPauseButton.ScaleX = 1;
                _instance._playPauseButton.ScaleY = 1;

                //hide the other components
                _instance._collapse.Visibility = ViewStates.Invisible;
                _instance._topTitle.Visibility = ViewStates.Invisible;
                _instance._timePlayed.Visibility = ViewStates.Invisible;
                _instance._timeRemaining.Visibility = ViewStates.Invisible;

                _instance._skipBackwards.Visibility = ViewStates.Invisible;
                _instance._previousTrack.Visibility = ViewStates.Invisible;
                _instance._nextTrack.Visibility = ViewStates.Invisible;
                _instance._skipForwards.Visibility = ViewStates.Invisible;

                //re add the player listener
                _instance._playerLayout.SetOnClickListener(new TapGestureRecognizer(_instance.UpdatePlayerView));
            }

            IsLargeView = !IsLargeView;
        }

        //A helper method that returns a DP translated from the int size you want to use
        private int GetDPValue(int size)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, size, _instance.PlayerView.Resources.DisplayMetrics);
        }
    }

    public class TapGestureRecognizer : Java.Lang.Object, IOnClickListener
    {
        private Action tap;

        public TapGestureRecognizer(Action tapped) {
            this.tap = tapped;
        }

        public void OnClick(Android.Views.View v)
        {
            tap?.Invoke();
        }
    }
}
