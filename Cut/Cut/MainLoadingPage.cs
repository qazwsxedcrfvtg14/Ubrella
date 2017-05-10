using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Cut
{
    public class MainLoadingPage : ContentPage
    {
        /*protected override bool OnBackButtonPressed()
        {
            return true;
        }*/
        public MainLoadingPage(MainPage mainPage)
        {
            
            var progressBar = new ProgressBar
            {
                Progress = 0,
                VerticalOptions = LayoutOptions.Center,
                Margin = 30
            };
            var progress = new Progress<int>(async percent =>
            {
                await progressBar.ProgressTo(((double)percent)/100.0, 100, Easing.Linear);
            });
            Content = progressBar;
            var task = Task.Run(async() => {
                //int data_version = 18;
                IProgress<int> pr = progress;
                pr.Report(3);

                await Voc.InitAsync();
                pr.Report(15);

                var fileService = DependencyService.Get<ISaveAndLoad>();

                if (!fileService.FileExists("setting.txt"))
                    await fileService.SaveTextAsync("setting.txt", "");
                pr.Report(20);

                Voc.setting = await Voc.GetDocAsync("setting", false);
                pr.Report(25);
                if (!Voc.setting.exists("website"))
                    Voc.setting.add("website", "http://ubrella1821.cloudapp.net/");
                if (!Voc.setting.exists("sound_url"))
                    Voc.setting.add("sound_url", "http://dictionary.reference.com/browse/");
                if (!Voc.setting.exists("sound_url2"))
                    Voc.setting.add("sound_url2", "http://static.sfdict.com/staticrep/dictaudio");
                if (!Voc.setting.exists("sound_type"))
                    Voc.setting.add("sound_type", ".mp3");
                if (!Voc.setting.exists("data_version"))
                    Voc.setting.add("data_version", "0");
                await Voc.SavingSettingAsync();
                pr.Report(30);

                //if (!fileService.FileExists("favorite.txt"))
                    //await fileService.SaveTextAsync("favorite.txt", "");
                pr.Report(35);

                //Voc.favorite = await Voc.GetDocAsync("favorite", false);
                pr.Report(40);

                //if (!fileService.FileExists("words.txt") || int.Parse(Voc.setting["data_version"]) < data_version)
                    //await Voc.DumpAppFileAsync("words.txt");
                pr.Report(50);

                //if (!fileService.FileExists("root.txt") || int.Parse(Voc.setting["data_version"]) < data_version)
                    //await Voc.DumpAppFileAsync("root.txt");
                pr.Report(55);

                //if (!fileService.FileExists("prefix.txt") || int.Parse(Voc.setting["data_version"]) < data_version)
                    //await Voc.DumpAppFileAsync("prefix.txt");
                pr.Report(60);

                //if (!fileService.FileExists("suffix.txt") || int.Parse(Voc.setting["data_version"]) < data_version)
                    //await Voc.DumpAppFileAsync("suffix.txt");
                pr.Report(65);

                //if (!fileService.FileExists("note.txt")|| int.Parse(Voc.setting["data_version"]) < data_version)
                    //await Voc.DumpAppFileAsync("note.txt");
                pr.Report(70);
                //if (int.Parse(Voc.setting["data_version"]) < data_version)
                    //Voc.setting["data_version"] = data_version.ToString();

                //Voc.words = await Voc.GetDocAsync("words", true);
                pr.Report(80);
                /*
                var tmp = Voc.words.val("apple");
                foreach (var x in Voc.words.data) {
                    var y = x;
                }
                */
                //Voc.root = await Voc.GetDocAsync("root", true);
                pr.Report(85);

                //Voc.prefix = await Voc.GetDocAsync("prefix", true);
                pr.Report(90);

                //Voc.suffix = await Voc.GetDocAsync("suffix", true);
                pr.Report(95);

                //Voc.note = await Voc.GetDocAsync("note", true);
                pr.Report(100);
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopModalAsync();
                    if(Voc.setting["home_page"]=="search")
                        mainPage.Detail = new NavigationPage((Page)Activator.CreateInstance(typeof(SearchVocPage)));
                });
            });
        }
        
    }
}
