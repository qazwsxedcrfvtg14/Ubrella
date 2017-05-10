using System;
using Xamarin.Forms;
using Cut.UWP;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

[assembly: Dependency(typeof(AudioService))]

namespace Cut.UWP
{
    public class AudioService : IAudio
    {
        public AudioService() { }

        public bool PlayMp3File(string uri)
        {
            var tmp = Windows.UI.Xaml.Application.Current.Resources;
            MediaElement _mediaPlayer = Global.Media;
            _mediaPlayer.AudioCategory = Windows.UI.Xaml.Media.AudioCategory.Media;
            _mediaPlayer.Source = new Uri(uri);
            //_mediaPlayer.Play();
            _mediaPlayer.AutoPlay = true;
            //var tmp = App.Current.Resources;
            //_mediaPlayer.Play();
            return true;
        }

        private void _mediaPlayer_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            (sender as MediaElement).Play();
        }
    }
}