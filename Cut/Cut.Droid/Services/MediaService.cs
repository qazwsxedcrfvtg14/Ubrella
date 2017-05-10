using System;
using Xamarin.Forms;
using Cut.Droid;
using Android.Media;
using System.Threading.Tasks;
using XLabs.Platform.Services.Media;
using Android.App;
using Android.Graphics;
using Android.Content.Res;

[assembly: Dependency(typeof(MediaService))]

namespace Cut.Droid
{
    public class MediaService : IMedia
    {
        public MediaService() { }

        public async Task<Size> GetSize(byte[] imageBytes)
        {

            BitmapFactory.Options Options = new BitmapFactory.Options { InSampleSize = 1 };
            await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length, Options);
            return new Size((double)Options.OutWidth, (double)Options.OutHeight);
        }
    }
}