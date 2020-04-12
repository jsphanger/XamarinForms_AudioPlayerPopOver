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

[assembly: Dependency(typeof(AudioPlayer))]
namespace PopUpPlayer.Droid.NativeControls
{
    public class AudioPlayer : IAudioPlayer
    {
        private static AudioPlayer _instance;

        public Context Context { get; private set; }
        public Android.Views.View PlayerView { get; private set; }

        private Android.Widget.RelativeLayout _playerLayout;
        private TextView _title, _subtitle;
        private ImageView _albumArtwork;
        private Android.Widget.ImageButton _playPauseButton;

        public string Title { get { return _title.Text; } set { _title.Text = value; }}
        public string SubTitle { get { return _subtitle.Text; } set { _subtitle.Text = value; } }
        public ImageView AlbumArtwork { get { return _albumArtwork; } }
        public Android.Widget.ImageButton PlayPauseButton { get { return _playPauseButton; } }

        public bool IsPlaying { get; set; }
        public bool IsLargeView { get; set; }

        static AudioPlayer() { }

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

            _instance._playerLayout = _instance.PlayerView.FindViewById<Android.Widget.RelativeLayout>(Resource.Id.rlPlayer);
            _instance._playerLayout.SetBackgroundColor(Android.Graphics.Color.Rgb(247, 247, 247));

            _instance._title = _instance.PlayerView.FindViewById<TextView>(Resource.Id.tvTitle);
            _instance._subtitle = _instance.PlayerView.FindViewById<TextView>(Resource.Id.tvSubtitle);
            _instance._albumArtwork = _instance.PlayerView.FindViewById<ImageView>(Resource.Id.ivAlbumArt);
            _instance._playPauseButton = _instance.PlayerView.FindViewById<Android.Widget.ImageButton>(Resource.Id.ibPlayPause);

            //-- setup our listeners
            _instance._playerLayout.SetOnClickListener(new AudioPlayerPressListener(_instance.UpdatePlayerView));
            _instance._playPauseButton.SetOnClickListener(new AudioPlayerPlayPauseListener(_instance.PlayButtonTapped));

            return _instance;
        }
        private void UpdatePlayerView()
        {
            Debug.WriteLine("Player Tapped.");

            TransitionManager.BeginDelayedTransition((ViewGroup)_instance._playerLayout.RootView);

            if (!IsLargeView)
            {
                //make the playerLayout animate to full size.
                ViewGroup.LayoutParams par = _instance._playerLayout.LayoutParameters;
                par.Width = WindowManagerLayoutParams.MatchParent;
                par.Height = WindowManagerLayoutParams.MatchParent;
                _instance._playerLayout.LayoutParameters = par;
            }
            else
            {
                ViewGroup.LayoutParams par = _instance._playerLayout.LayoutParameters;
                par.Width = WindowManagerLayoutParams.MatchParent;
                par.Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 50, _instance.PlayerView.Resources.DisplayMetrics);
                _instance._playerLayout.LayoutParameters = par;
            }

            IsLargeView = !IsLargeView;
        }
    }

    public class AudioPlayerPressListener : Java.Lang.Object, IOnClickListener
    {
        private Action playerTapped;

        public AudioPlayerPressListener(Action playerTapped) {
            this.playerTapped = playerTapped;
        }

        public void OnClick(Android.Views.View v)
        {
            playerTapped?.Invoke();
        }
    }

    public class AudioPlayerPlayPauseListener : Java.Lang.Object, IOnClickListener
    {
        private Action playButtonTapped;

        public AudioPlayerPlayPauseListener(Action playButtonTapped)
        {
            this.playButtonTapped = playButtonTapped;
        }

        public void OnClick(Android.Views.View v)
        {
            playButtonTapped?.Invoke();
        }
    }
}
