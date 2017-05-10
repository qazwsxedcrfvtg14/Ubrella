using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Threading.Tasks;
using XLabs.Platform.Services.Media;
using XLabs.Ioc;
using XLabs.Platform.Device;
using XLabs.Forms.Controls;
using XLabs.Enums;
using Tesseract;
namespace Cut
{
    public class PhotoPage : ContentPage
    {

        private readonly ITesseractApi _tesseractApi;
        Image image= new Image
        {
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Start,
        };
        IEnumerable<Tesseract.Result> res;
        Grid grid = new Grid
        {
            HorizontalOptions = LayoutOptions.Center,
        };
        Size imgsz=Size.Zero;
        public PhotoPage()
        {
            _tesseractApi = Resolver.Resolve<ITesseractApi>();
            Setup();
            Title = "圖片掃描";
            var text = new Label { };
            grid.Children.Add(image);
            
            image.SizeChanged += Image_SizeChanged;
            Content = new ScrollView
            {
                HorizontalOptions = LayoutOptions.Center,
                Content = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    Children = {
                        new Button {
                            Text ="Take Picture",
                            Command =new Command(async () => {
                                var media=await TakePicture();
                                if(media==null)
                                    return;
                                image.Source = ImageSource.FromStream(() => media.Source);
                                text.Text="Running";
                                var res=await TesseractRun(media);
                                text.Text=media.Path;
                            })
                        },
                        new Button {
                            Text ="Select Image",
                            Command =new Command(async () => {
                                var media=await SelectPicture();
                                if(media==null)
                                    return;
                                image.Source = ImageSource.FromStream(() => media.Source);
                                text.Text="Running";
                                res=await TesseractRun(media);
                                text.Text=media.Path;
                                Image_SizeChanged(null,null);
                            })
                        },
                        text,
                        grid
                    }
                }
            };
        }

        private void Image_SizeChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (res == null) return;
            if (image == null) return;
            if (image.Height < 0 || image.Width < 0||imgsz.IsZero) return;
            while (grid.Children.Count > 1) grid.Children.RemoveAt(grid.Children.Count - 1);
            double dy = image.Height / imgsz.Height;
            double dx = image.Width / imgsz.Width;
            foreach (var x in res) {
                var box = new BoxView
                {
                    HeightRequest = x.Box.Height * dy,
                    WidthRequest = x.Box.Width * dx,
                    Margin = new Thickness(x.Box.X * dx, x.Box.Y * dy, 0, 0),
                    Color = Color.FromRgba(127, 127, 127, 127),
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Start,
                };
                var tgr = new TapGestureRecognizer();
                tgr.Tapped += Tgr_Tapped;
                box.GestureRecognizers.Add(tgr);
                grid.Children.Add(box);
            }
        }

        private async void Tgr_Tapped(object sender, EventArgs e)
        {
            int i = 1;
            foreach (var x in res)
            {
                if (grid.Children[i] == sender) {
                    string s = x.Text;
                    int be = 0;
                    for (; be < s.Length; be++)
                        if ((s[be] >= 'a' && s[be] <= 'z') || (s[be] >= 'A' && s[be] <= 'Z'))
                            break;
                    int ed = s.Length-1;
                    for (; ed >=0; ed--)
                        if ((s[ed] >= 'a' && s[ed] <= 'z') || (s[ed] >= 'A' && s[ed] <= 'Z'))
                            break;
                    if (be >= ed) return;
                    await Navigation.PushAsync(new SingleVocPage(s.Substring(be,ed-be+1)));
                    return;
                }
                i++;
            }
            //throw new NotImplementedException();
        }

        async Task<IEnumerable<Tesseract.Result>> TesseractRun(MediaFile photo)
        {
            if (!_tesseractApi.Initialized)
                await _tesseractApi.Init("eng");
            
            if (photo != null)
            {
                // When setting an ImageSource using a stream, 
                // the stream gets closed, so to avoid that I backed up
                // the image into a byte array with the following code:
                var imageBytes = new byte[photo.Source.Length];
                photo.Source.Position = 0;
                photo.Source.Read(imageBytes, 0, (int)photo.Source.Length);
                photo.Source.Position = 0;
                if (imageBytes == null)
                    return null;
                var tessResult = await _tesseractApi.SetImage(imageBytes);
                //_tesseractApi.Option
                if (tessResult)
                {
                    imgsz = await DependencyService.Get<IMedia>().GetSize(imageBytes);
                    return _tesseractApi.Results(PageIteratorLevel.Word);
                    /*string ret = "";
                    foreach (var x in _tesseractApi.Results(PageIteratorLevel.Word)) {
                        ret += "[" + x.Box.X.ToString()+","+x.Box.Y.ToString() + "," + x.Box.Height.ToString() + "," 
                            +x.Box.Width.ToString()+ "]" + x.Text+"\n";
                    }

                    //var tmp = _tesseractApi.Results(PageIteratorLevel.Block);
                    //_recognizedTextLabel.Text = _tesseractApi.Text;
                    //return _tesseractApi.Text;
                    return ret;*/
                }
            }
            return null;
        }
        /// <summary>
        /// The _scheduler.
        /// </summary>
        private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        /// <summary>
        /// The picture chooser.
        /// </summary>
        private IMediaPicker _mediaPicker;

        /// <summary>
        /// The image source.
        /// </summary>
        private ImageSource _imageSource;

        /// <summary>
        /// The video info.
        /// </summary>
        private string _videoInfo;
        
        private string _status;


        ////private CancellationTokenSource cancelSource;
        

        /// <summary>
        /// Gets or sets the image source.
        /// </summary>
        /// <value>The image source.</value>
        public ImageSource ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                image.Source = _imageSource;
                //SetProperty(ref _imageSource, value);
            }
        }

        /// <summary>
        /// Gets or sets the video info.
        /// </summary>
        /// <value>The video info.</value>
        public string VideoInfo
        {
            get
            {
                return _videoInfo;
            }
            set
            {
                _videoInfo = value;
                //SetProperty(ref _videoInfo, value);
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status
        {
            get { return _status; }
            private set {
                _status = value;
                //SetProperty(ref _status, value);
            }
        }

        /// <summary>
        /// Setups this instance.
        /// </summary>
        private void Setup()
        {
            if (_mediaPicker != null)
            {
                return;
            }

            var device = Resolver.Resolve<IDevice>();

            ////RM: hack for working on windows phone? 
            _mediaPicker = DependencyService.Get<IMediaPicker>() ?? device.MediaPicker;
        }

        /// <summary>
        /// Takes the picture.
        /// </summary>
        /// <returns>Take Picture Task.</returns>
        private async Task<MediaFile> TakePicture()
        {
            Setup();

            ImageSource = null;

            return await _mediaPicker.TakePhotoAsync(new CameraMediaStorageOptions { DefaultCamera = CameraDevice.Front, MaxPixelDimension = 400 }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Status = t.Exception.InnerException.ToString();
                }
                else if (t.IsCanceled)
                {
                    Status = "Canceled";
                }
                else
                {
                    var mediaFile = t.Result;

                    //ImageSource = ImageSource.FromStream(() => mediaFile.Source);

                    return mediaFile;
                }

                return null;
            }, _scheduler);
        }

        /// <summary>
        /// Selects the picture.
        /// </summary>
        /// <returns>Select Picture Task.</returns>
        private async Task<MediaFile> SelectPicture()
        {
            Setup();
            ImageSource = null;
            try
            {
                var mediaFile = await _mediaPicker.SelectPhotoAsync(new CameraMediaStorageOptions
                {
                    DefaultCamera = CameraDevice.Front,
                    MaxPixelDimension = 400
                });
                //ImageSource = ImageSource.FromStream(() => mediaFile.Source);
                return mediaFile;
            }
            catch (System.Exception ex)
            {
                Status = ex.Message;
            }
            return null;
        }
    }
}