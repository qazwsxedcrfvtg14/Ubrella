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

namespace Cut
{
    public class SettingPage : ContentPage
    {
        StackLayout st = new StackLayout();
        Entry host_name = new Entry();
        Entry user_name = new Entry();
        ListView set_list = new ListView();
        ObservableCollection<string> set_list_items = new ObservableCollection<string> {
            "關於"
        };
        public SettingPage()
        {
            Title = "設定";
            Grid grid = new Grid
            {
                ColumnDefinitions = {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
                RowDefinitions = {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                },
                Padding = 5
            };
            grid.Children.Add(new Label { Text = "伺服器位置：", VerticalOptions = LayoutOptions.Center }, 0, 0);
            host_name.TextChanged += Host_name_TextChanged;
            host_name.Text = Voc.setting["website"];
            grid.Children.Add(host_name, 1, 0);
            st.Children.Add(grid);
            grid.Children.Add(new Label { Text = "使用者帳號：", VerticalOptions = LayoutOptions.Center }, 0, 1);
            user_name.TextChanged += Host_name_TextChanged1;
            user_name.Text = Voc.setting["user"];
            grid.Children.Add(user_name, 1, 1);
            st.Children.Add(grid);
            //st.Children.Add(new Label { Text = "(此位置留白，重開應用程式會回復預設)" });
            set_list.ItemSelected += Set_list_ItemSelected;
            set_list.ItemsSource = set_list_items;
            st.Children.Add(set_list);
            Content = st;
        }

        private void Host_name_TextChanged1(object sender, TextChangedEventArgs e)
        {
            Voc.setting["user"] = user_name.Text;
        }

        private async void Set_list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            string str;
            if (e != null)
                str = e.SelectedItem as string;
            else
                str = sender as string;
            if (str == null) return;
            set_list.SelectedItem = null;
            var fileService = DependencyService.Get<ISaveAndLoad>();
            if (str == "重置應用程式")
            {
                //if (rtp==nullptr) { ShowMsg("應用程式初始化中，請稍後重試。"); return; }
                var task=DisplayAlert("請等一下", "讀取中，請稍後", "了解");
                await fileService.SaveTextAsync("setting.txt", "");
                await fileService.SaveTextAsync("favorite.txt", "");
                await Voc.DumpAppFileAsync("words.txt");
                await Voc.DumpAppFileAsync("prefix.txt");
                await Voc.DumpAppFileAsync("suffix.txt");
                await Voc.DumpAppFileAsync("root.txt");
                await Voc.DumpAppFileAsync("note.txt");
                await fileService.SaveTextAsync("words_user.txt", "");
                await fileService.SaveTextAsync("prefix_user.txt", "");
                await fileService.SaveTextAsync("suffix_user.txt", "");
                await fileService.SaveTextAsync("root_user.txt", "");
                await fileService.SaveTextAsync("note_user.txt", "");
                Voc.setting = await Voc.GetDocAsync("setting.txt", false);
                Voc.favorite = await Voc.GetDocAsync("favorite.txt", false);
                Voc.note = await Voc.GetDocAsync("note.txt");
                Voc.words = await Voc.GetDocAsync("words.txt");
                Voc.prefix = await Voc.GetDocAsync("prefix.txt");
                Voc.suffix = await Voc.GetDocAsync("suffix.txt");
                Voc.root = await Voc.GetDocAsync("root.txt");
                if (!Voc.setting.exists("website"))
                    Voc.setting.add("website", "http://joe59491.azurewebsites.net");
                if (!Voc.setting.exists("sound_url"))
                    Voc.setting.add("sound_url", "http://dictionary.reference.com/browse/");
                if (!Voc.setting.exists("sound_url2"))
                    Voc.setting.add("sound_url2", "http://static.sfdict.com/staticrep/dictaudio");
                if (!Voc.setting.exists("sound_type"))
                    Voc.setting.add("sound_type", ".mp3");
                if (!Voc.setting.exists("data_version"))
                    Voc.setting.add("data_version", "0");
                await task;
                await DisplayAlert("完成", "設定完成", "了解");
            }
            else if (str == "更新單字庫")
            {
                var s = await Voc.GetAsync(Voc.setting["website"] + "/version.php");
                if (s == null || s == "")
                    goto network_error;
                if (int.Parse(Voc.setting["data_version"]) >= int.Parse(s))
                {
                    await DisplayAlert("錯誤", "已經是最新版了", "了解");
                    goto end;
                }
                var task = DisplayAlert("請等一下", "讀取中，請稍後", "了解");
                Voc.setting["data_version"] = s;
                s = await Voc.GetAsync(Voc.setting["website"] + "/words.php");
                if (s == null || s == "")
                    goto network_error;
                await fileService.SaveTextAsync("words.txt", s);
                Voc.words = await Voc.GetDocAsync("words.txt");
                s = await Voc.GetAsync(Voc.setting["website"] + "/prefix.php");
                if (s == null || s == "")
                    goto network_error;
                await fileService.SaveTextAsync("prefix.txt", s);
                Voc.prefix = await Voc.GetDocAsync("prefix.txt");
                s = await Voc.GetAsync(Voc.setting["website"] + "/suffix.php");
                if (s == null || s == "")
                    goto network_error;
                await fileService.SaveTextAsync("suffix.txt", s);
                Voc.suffix = await Voc.GetDocAsync("suffix.txt");
                s = await Voc.GetAsync(Voc.setting["website"] + "/root.php");
                if (s == null || s == "")
                    goto network_error;
                await fileService.SaveTextAsync("root.txt", s);
                Voc.root = await Voc.GetDocAsync("root.txt");
                s = await Voc.GetAsync(Voc.setting["website"] + "/note.php");
                if (s == null || s == "")
                    goto network_error;
                await fileService.SaveTextAsync("note.txt", s);
                Voc.note = await Voc.GetDocAsync("note.txt");
                await task;
                await DisplayAlert("完成", "設定完成，請重新啟動應用程式", "了解");
                goto end;
                network_error:;
                await DisplayAlert("錯誤", "網路錯誤", "了解");
                end:;
            }
            else if (str == "單字庫選項")
            {
                set_list_items.Clear();
                set_list_items.Add("更新單字庫");
                //set_list_items.Add("重置單字庫");
                set_list_items.Add("重置應用程式");
                set_list_items.Add("回設定主頁");
            }
            else if (str == "回設定主頁")
            {
                set_list_items.Clear();
                set_list_items.Add("聲音選項");
                set_list_items.Add("單字庫選項");
                set_list_items.Add("測驗選項");
                set_list_items.Add("啟動選項");
                set_list_items.Add("網路選項");
                set_list_items.Add("關於");
            }
            else if (str == "聲音選項")
            {
                set_list_items.Clear();
                set_list_items.Add("自動播放開啟");
                set_list_items.Add("自動播放關閉");
                set_list_items.Add("發音:Dictionary.com");
                set_list_items.Add("發音:Bing 美國");
                set_list_items.Add("發音:Bing 英國");
                set_list_items.Add("回設定主頁");
            }
            else if (str == "自動播放開啟")
            {
                Voc.setting["auto_play"] = "On";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "自動播放關閉")
            {
                Voc.setting["auto_play"] = "Off";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "發音:Dictionary.com")
            {
                Voc.setting["sound_url"] = "http://dictionary.reference.com/browse/";
                Voc.setting["sound_url2"] = "http://static.sfdict.com/staticrep/dictaudio";
                Voc.setting["sound_type"] = ".mp3";

                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "發音:Bing 美國")
            {
                Voc.setting["sound_url"] = "http://cn.bing.com/dict/search?mkt=zh-cn&q=";
                Voc.setting["sound_url2"] = "https://dictionary.blob.core.chinacloudapi.cn/media/audio/tom";
                Voc.setting["sound_type"] = ".mp3";



                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "發音:Bing 英國")
            {
                Voc.setting["sound_url"] = "http://cn.bing.com/dict/search?mkt=zh-cn&q=";
                Voc.setting["sound_url2"] = "https://dictionary.blob.core.chinacloudapi.cn/media/audio/george";
                Voc.setting["sound_type"] = ".mp3";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "關於")
            {

                //var package = Package::Current;
                //var packageId = package_id;
                //var version = packageId.Version;

                //return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                //var major = version.Major;

                //ShowMsg("版本號:ver " + IntToStr(version.Major) + "." + IntToStr(version.Minor) + "." + IntToStr(version.Build) + "." + IntToStr(version.Revision) );
                await DisplayAlert("版本號", "ver 1.2.39", "了解");
            }
            else if (str == "測驗選項")
            {
                set_list_items.Clear();
                set_list_items.Clear();
                set_list_items.Add("關閉測驗選項");
                var s = new StackLayout { Orientation = StackOrientation.Horizontal };
                var tp = new Label { Text = "選擇題選項數量(2~100)：" };
                var ss = Voc.setting["sellect_prob_cnt"] ;
                if (ss == ""||ss==null)
                    Voc.setting["sellect_prob_cnt"] = "5";
                var tmp = new Entry { Text = Voc.setting["sellect_prob_cnt"] };
                tmp.TextChanged += (sen, ex) => {
                    Voc.setting["sellect_prob_cnt"] = tmp.Text;
                };

                s.Children.Add(tp);
                s.Children.Add(tmp);
                st.Children.Add(s);
            }
            else if (str == "關閉測驗選項")
            {
                st.Children.RemoveAt(st.Children.Count - 1);

                Set_list_ItemSelected("回設定主頁", null);
            }
            else if (str == "啟動選項")
            {
                set_list_items.Clear();
                set_list_items.Add("啟動後進入首頁");
                set_list_items.Add("啟動後進入單字搜尋頁面");
                set_list_items.Add("回設定主頁");
            }
            else if (str == "網路選項")
            {
                set_list_items.Clear();
                set_list_items.Add("開啟網路音標");
                set_list_items.Add("關閉網路音標");
                set_list_items.Add("開啟網路發音");
                set_list_items.Add("關閉網路發音");
                set_list_items.Add("開啟網路圖片");
                set_list_items.Add("關閉網路圖片");
                set_list_items.Add("開啟網路例句");
                set_list_items.Add("關閉網路例句");
                set_list_items.Add("回設定主頁");
            }
            else if (str == "啟動後進入首頁")
            {
                Voc.setting["home_page"] = "home";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "啟動後進入單字搜尋頁面")
            {
                Voc.setting["home_page"] = "search";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "開啟網路音標")
            {
                Voc.setting["network_kk"] = "true";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "關閉網路音標")
            {
                Voc.setting["network_kk"] = "false";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "開啟網路發音")
            {
                Voc.setting["network_sound"] = "true";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "關閉網路發音")
            {
                Voc.setting["network_sound"] = "false";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "開啟網路圖片")
            {
                Voc.setting["network_picture"] = "true";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "關閉網路圖片")
            {
                Voc.setting["network_picture"] = "false";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "開啟網路例句")
            {
                Voc.setting["network_sent"] = "true";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else if (str == "關閉網路例句")
            {
                Voc.setting["network_sent"] = "false";
                await DisplayAlert("成功", "設定成功", "了解");
            }
            else
            {
                throw new Exception("Option 404 Error");
            }
        }

        private void Host_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            Voc.setting["website"] = host_name.Text;
        }
    }
}