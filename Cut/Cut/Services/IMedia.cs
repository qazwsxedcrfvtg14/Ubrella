using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using XLabs.Platform.Services.Media;

namespace Cut
{
    public interface IMedia
    {
        Task<Size> GetSize(byte[] media);
    }
}