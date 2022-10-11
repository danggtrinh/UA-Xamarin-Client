using Plugin.Share;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage
    {
        public AboutPage()
        {
            InitializeComponent();
            //Open.GestureRecognizers.Add(new TapGestureRecognizer
            //{
            //    Command = new Command(() => OpenBrowser()),
            //});
        }

        //public ICommand ClickCommand => new Command(OpenBrowser);

        //private void OpenBrowser()
        //{
        //    CrossShare.Current.OpenBrowser("http://dee.hcmut.edu.vn/");
        //}

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
        }
    }
}