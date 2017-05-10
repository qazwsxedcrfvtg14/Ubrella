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
    public class SearchVocPage : ContentPage
    {
        SearchBar input_voc;
        ListView VocList;
        ObservableCollection<wod> VocList_Items;
        bool islading = false;
        string target = "";
        public SearchVocPage()
        {
            Title = "單字查詢";
            input_voc = new SearchBar {
                FontSize=18
            };
            input_voc.Placeholder = "請輸入中文或英文，亦可用?或*代表任意或單個字母";
            VocList = new ListView();
            VocList_Items = new ObservableCollection<wod>();
            VocList.ItemsSource = VocList_Items;
            var customCell = new DataTemplate(typeof(stcell));
            customCell.SetBinding(stcell.vocProperty, "voc");
            customCell.SetBinding(stcell.expProperty, "exp");
            VocList.ItemTemplate = customCell;
            VocList.ItemSelected += Lis_ItemSelectedAsync;
            VocList.HasUnevenRows = true;
            Device.StartTimer(new TimeSpan(1000000), () => {
                if (this == null)
                    return false;
                var q = input_voc.Text;
                if (q == null) q = "";
                if (target == q)
                    return true;
                islading = true;
                target = q;
                Task.Run(() =>
                {
                    var ve = Voc.Match(q + "*");
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (target != input_voc.Text||q!=target) return;
                        VocList_Items.Clear();
                        foreach (var voc in ve)
                            VocList_Items.Add(new wod(voc, Voc.GetExpSimple(Voc.words.val(voc))));
                        islading = false;
                    });
                });
                return true;
            });
            input_voc.SearchButtonPressed += async (sender, args) => {
                if (input_voc.Text == "") return;
                string s = input_voc.Text;
                for (int i = 0; i < s.Length; i++)
                    if (s[i] == '*' || s[i] == '^' || s[i] == '?')
                        return;
                await Navigation.PushAsync(new SingleVocPage(input_voc.Text));
            };
            VocList_Items.Clear();
            foreach (var voc in Voc.Match(input_voc.Text + "*"))
                VocList_Items.Add(new wod(voc, Voc.GetExpSimple(Voc.words.val(voc))));
            VocList.ItemAppearing += (sender, e) =>{
                if (islading || VocList_Items.Count == 0)
                    return;
                if (e.Item!=null&&(e.Item as wod).voc == VocList_Items[VocList_Items.Count - 1].voc){
                    LoadMoreItems();
                }
            };
            Content = new StackLayout
            {
                Children = {
                    input_voc,
                    VocList
                    
                }
            };
        }

        private async void Lis_ItemSelectedAsync(object sender, SelectedItemChangedEventArgs e)
        {
            var item = (e.SelectedItem as wod);
            if (item != null)
            {
                await Navigation.PushAsync(new SingleVocPage(item.voc));
                VocList.SelectedItem = null;
            }
        }
        private void LoadMoreItems()
        {
            islading = true;
            int cnt = VocList_Items.Count;
            string st = VocList_Items[cnt - 1].voc;
            var q = input_voc.Text;
            Task.Run(() =>
            {
                var ve=Voc.Match(input_voc.Text + "*", cnt,st);
                Device.BeginInvokeOnMainThread(() => {
                    if (q != input_voc.Text) return;
                    foreach (var voc in ve)
                        VocList_Items.Add(new wod(voc, Voc.GetExpSimple(Voc.words.val(voc))));
                    islading = false;
                });
            });
        }
    }
}
