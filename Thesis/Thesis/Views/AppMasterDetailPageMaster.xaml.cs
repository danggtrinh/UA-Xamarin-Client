using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Thesis.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppMasterDetailPageMaster : ContentPage
    {
        public ListView ListView;
      

        public AppMasterDetailPageMaster()
        {
            InitializeComponent();

            BindingContext = new AppMasterDetailPageMasterViewModel();
            ListView = MenuItemsListView;
           
        }

        private class AppMasterDetailPageMasterViewModel : INotifyPropertyChanged
        {
            private SampleClient opcClient;
            public ObservableCollection<AppMasterDetailPageMenuItem> MenuItems { get; set; }

            public AppMasterDetailPageMasterViewModel()
            {
                opcClient = MainPage.OpcClient;
                if (opcClient.session.Endpoint.EndpointUrl == "opc.tcp://192.168.0.2:4840/")
                {
                    MenuItems = new ObservableCollection<AppMasterDetailPageMenuItem>(new[]
                    {
                    new AppMasterDetailPageMenuItem { Id = 0,Icon="home.png", Title = "Status" ,TargetType=typeof(AppMasterDetailPageDetail)},
                    new AppMasterDetailPageMenuItem { Id = 1,Icon="connector.png", Title = "Browse" ,TargetType=typeof(TreeView)},
                    new AppMasterDetailPageMenuItem { Id = 2,Icon="hardware.png", Title = "Monitor", TargetType=typeof(MonitorPage)},
                     new AppMasterDetailPageMenuItem { Id = 3,Icon="manufacturing.png", Title = "Station", TargetType=typeof(MyScada)},
                    });
                }
                else
                {
                    MenuItems = new ObservableCollection<AppMasterDetailPageMenuItem>(new[]
                  {
                    new AppMasterDetailPageMenuItem { Id = 0, Title = "Status" ,TargetType=typeof(AppMasterDetailPageDetail)},
                    new AppMasterDetailPageMenuItem { Id = 1, Title = "Browse" ,TargetType=typeof(TreeView)},
                    new AppMasterDetailPageMenuItem { Id = 2, Title = "Monitor", TargetType=typeof(MonitorPage)},
                     new AppMasterDetailPageMenuItem {Id = 5,Title = "DiagnosticBuffer", TargetType=typeof(DiagnosticBuffer)},
                    new AppMasterDetailPageMenuItem {Id = 3,Title = "SetTime", TargetType=typeof(SetTime)},
                    new AppMasterDetailPageMenuItem {Id = 4,Title = "Interface", TargetType=typeof(Interface)},
                   
                    new AppMasterDetailPageMenuItem {Id = 5,Title = "Memory", TargetType=typeof(Memory)}
                    });
                }
              
            }

            #region INotifyPropertyChanged Implementation

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion INotifyPropertyChanged Implementation
        }

        public static SampleClient OpcClient_Master { get; set; }

        private void Disconnect_Clicked(object sender, EventArgs e)
        {
            OpcClient_Master.Disconnect(OpcClient_Master.session);
            App.Current.MainPage = new NavigationPage(new MainPage());
        }
    }
}