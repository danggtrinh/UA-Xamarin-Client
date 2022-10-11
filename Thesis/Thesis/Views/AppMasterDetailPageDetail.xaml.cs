using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppMasterDetailPageDetail : ContentPage
    {
        public static string datetime { get; set; }
        public static ConnectType connecttype { get; set; }

        public AppMasterDetailPageDetail()
        {
            InitializeComponent();
            servername.Text = connecttype.ConnectionName;
            serveruri.Text = "Uri:" + connecttype.ConnectionUrl;
            connectionstatus.Text = "Connected";
            connectedsince.Text = datetime;
        }

        private async void ToolbarItem_Clicked_About(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AboutPage());
        }

        private async void ToolbarItem_Clicked_Help(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new HelpPage());
        }
    }
}