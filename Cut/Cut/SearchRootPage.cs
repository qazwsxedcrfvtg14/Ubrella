using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Cut
{
    public class SearchRootPage : ContentPage
    {
        
        public class wod
        {
            public string voc { get; private set; }
            public string exp { get; private set; }
            public wod(string _voc, string _exp)
            {
                voc = _voc;
                exp = _exp;
            }
        }
        ListView VocList;
        ObservableCollection<wod> VocList_Items;
        SearchBar input_voc;
        public SearchRootPage()
        {
            Title = "部首查詢";
            input_voc = new SearchBar()
            {
                Placeholder = "請輸入欲查詢之部首，亦可用?或*代表任意或單個字母"
            };
            VocList = new ListView();
            VocList_Items = new ObservableCollection<wod>();
            VocList.ItemsSource = VocList_Items;
            var customCell = new DataTemplate(typeof(stcell));
            customCell.SetBinding(stcell.vocProperty, "voc");
            customCell.SetBinding(stcell.expProperty, "exp");
            VocList.ItemTemplate = customCell;
            VocList.ItemSelected += Lis_ItemSelectedAsync;
            VocList.HasUnevenRows = true;
            input_voc.TextChanged += (sender, args)=>{
                VocList_Items.Clear();
                foreach (var voc in Voc.Match_rot(input_voc.Text + "*"))
                    VocList_Items.Add(new wod(voc, Voc.GetRootExp(voc)));
            };
            /*input_voc.SearchButtonPressed += async (sender, args) => {
                if (input_voc.Text == "") return;
                await Navigation.PushAsync(new SingleVocPage(input_voc.Text));
            };*/
            VocList_Items.Clear();
            foreach (var voc in Voc.Match_rot(input_voc.Text + "*"))
                VocList_Items.Add(new wod(voc, Voc.GetRootExp(voc)));
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
                await Navigation.PushAsync(new SingleRootPage(item.voc));
                VocList.SelectedItem = null;
            }
        }
    }
}
