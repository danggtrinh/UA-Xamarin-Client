using Thesis.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Thesis
{
    public partial class App : Application
    {
        private static DBItemController dbcontroller;

        public App()
        {
            try
            {
                InitializeComponent();
                MainPage = new NavigationPage(new MainPage());
                //MainPage = new NavigationPage(new MyScada());
            }
            catch {; }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static DBItemController Dbcontroller
        {
            get
            {
                if (dbcontroller == null)
                {
                    dbcontroller = new DBItemController();
                }
                return dbcontroller;
            }
        }
    }
}