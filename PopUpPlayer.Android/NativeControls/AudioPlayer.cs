using System;
using System.Diagnostics;
using Android.Content;
using Android.Views;
using Android.Widget;
using static Android.Views.View;

namespace PopUpPlayer.Droid.NativeControls
{
    public class AudioPlayer
    {
        public Context Context { get; }
        public View PlayerView { get; private set; }

        private TextView _title, _subtitle;
        private ImageView _albumArtwork;
        private ImageButton _playPauseButton;

        public string Title { get { return _title.Text; } set { _title.Text = value; }}
        public string SubTitle { get { return _subtitle.Text; } set { _subtitle.Text = value; } }
        public ImageView AlbumArtwork { get { return _albumArtwork; } }
        public ImageButton PlayPauseButton { get { return _playPauseButton; } }

        public bool IsPlaying { get; set; }

        public AudioPlayer(Context context) {
            Context = context;

            SetUIView();
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

        private void SetUIView()
        {
            var inflater = Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            PlayerView = inflater.Inflate(Resource.Layout.AudioPlayer, null);

            _title = PlayerView.FindViewById<TextView>(Resource.Id.tvTitle);
            _subtitle = PlayerView.FindViewById<TextView>(Resource.Id.tvSubtitle);
            _albumArtwork = PlayerView.FindViewById<ImageView>(Resource.Id.ivAlbumArt);
            _playPauseButton = PlayerView.FindViewById<ImageButton>(Resource.Id.ibPlayPause);

            //-- setup our listeners
            _playPauseButton.SetOnClickListener(new AudioPlayerPlayPauseListener(PlayButtonTapped));
        }
    }

    public class AudioPlayerPlayPauseListener : Java.Lang.Object, IOnClickListener
    {
        private Action playButtonTapped;

        public AudioPlayerPlayPauseListener(Action playButtonTapped)
        {
            this.playButtonTapped = playButtonTapped;
        }

        public void OnClick(View v)
        {
            playButtonTapped?.Invoke();
        }
    }
}
