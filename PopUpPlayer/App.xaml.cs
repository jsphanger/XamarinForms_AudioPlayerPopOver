using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PopUpPlayer.Services;
using PopUpPlayer.Views;
using PopUpPlayer.Interfaces;

namespace PopUpPlayer
{
    public partial class App : Application
    {
        public static IAudioPlayer AudioPlayer { get { return DependencyService.Get<IAudioPlayer>(); } }

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
