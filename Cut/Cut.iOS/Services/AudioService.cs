using System;
using Xamarin.Forms;
using Cut.iOS;
using AudioToolbox;
using Foundation;
using AVFoundation;
using UIKit;

[assembly: Dependency(typeof(AudioService))]

namespace Cut.iOS
{
    public class AudioService : IAudio
    {
        public AudioService() { }
        public bool PlayMp3File(string uri)
        {
            //var window = UIApplication.SharedApplication.KeyWindow;
            //var vc = window.RootViewController;
            /*while (vc.PresentedViewController != null)
            {
                vc = vc.PresentedViewController;
            }*/
            //window.RootViewController = new AudioUIViewController { uri = uri };
            //window.SubviewAdded(new AVPlayerLayer());
            //window
            //vc.AddChildViewController();
            var url = NSUrl.FromString(uri);

            var _asset = AVAsset.FromUrl(url);
            var _playerItem = new AVPlayerItem(_asset);
            var _player = AVPlayer.FromPlayerItem(_playerItem);
            //var _playerLayer = AVPlayerLayer.FromPlayer(_player);
            //_playerLayer.Frame = View.Frame;
            //View.Layer.AddSublayer(_playerLayer);
            _player.Play();
            /*
            var avAsset = new AVUrlAsset(url);

            var playerItem = new AVPlayerItem(avAsset);
            Global._player = new AVPlayer(playerItem);
            //var tmp = Global._player.Status;
            Device.StartTimer(new TimeSpan(1000000), () => {
                if (Global._player == null) return false;
                if(Global._player.Status== AVPlayerStatus.ReadyToPlay)
                {
                    Global._player.Play();
                    return false;
                }
                return true;
            });
            */
            //AVPlayerStatus.ReadyToPlay
            //Global._player.Play();

            return true;
        }
    }
}