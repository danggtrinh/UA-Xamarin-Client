using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
        }
    }
}