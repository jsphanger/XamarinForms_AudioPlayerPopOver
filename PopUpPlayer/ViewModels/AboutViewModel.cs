using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PopUpPlayer.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            ChangeTrack = new Command(() => App.AudioPlayer.ShowTrack());
        }

        public ICommand ChangeTrack { get; }
    }
}