using System;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemListView : ContentPage
    {
        private ObservableCollection<DBItem> items = new ObservableCollection<DBItem>();

        public ItemListView()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            // var enumerator = App.Dbcontroller.GetDBItems();
            //if (enumerator == null)
            //{
            //    App.Dbcontroller.SaveDBItem(new DBItem { Id = 0, Label = "blabla 1", Value = 200 });
            //    App.Dbcontroller.SaveDBItem(new DBItem { Id = 0, Label = "blabla 2", Value = 250 });
            //    App.Dbcontroller.SaveDBItem(new DBItem { Id = 0, Label = "blabla 3", Value = 30 });
            //    App.Dbcontroller.SaveDBItem(new DBItem { Id = 0, Label = "blabla 4", Value = 230 });
            //    enumerator = App.Dbcontroller.GetDBItems();
            //}
            //while (enumerator.MoveNext())
            //{
            //    this.items.Add(enumerator.Current);

            //}
            ListviewItems.ItemsSource = this.items;
        }

        private void OnDelete(Object sender, System.EventArgs e)
        {
            var item = (MenuItem)sender;
            var model = (DBItem)item.CommandParameter;
            this.items.Remove(model);
            //App.Dbcontroller.DeleteDBItem(model.Id);
        }
    }
}