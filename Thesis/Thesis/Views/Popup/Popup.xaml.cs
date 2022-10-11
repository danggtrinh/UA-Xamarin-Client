using Rg.Plugins.Popup.Services;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Popup
    {
        public Popup(ConnectType connectType = null)
        {
            InitializeComponent();
            BindingContext = new AddOrEditConnectionViewModel();
            if (connectType != null)
            {
                ((AddOrEditConnectionViewModel)BindingContext).ConnectType = connectType;
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private void AddConnection_Clicked(object sender, EventArgs e)
        {
            ConnectType connectType = ((AddOrEditConnectionViewModel)BindingContext).ConnectType;
            if (String.IsNullOrEmpty(connectType.ConnectionName))
            {
                DisplayAlert("Alarm", "Entry OPCUA Server Name can't be null", "OK");
            }
            else if (String.IsNullOrEmpty(connectType.ConnectionUrl))
            {
                DisplayAlert("Alarm", "Entry OPCUA Server URL can't be null", "OK");
            }
            else
            {
                MessagingCenter.Send(this, "AddOrEditConnection", connectType);
                PopupNavigation.Instance.PopAsync();
            }
        }

        private void Toggle_Toggled(object sender, ToggledEventArgs e)
        {
            if (Toggle.IsToggled == true)
            {
                User.IsVisible = true;
                Pass.IsVisible = true;
            }
            else
            {
                User.IsVisible = false;
                Pass.IsVisible = false;
            }
        }
    }
}