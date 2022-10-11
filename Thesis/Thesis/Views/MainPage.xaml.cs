using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Opc.Ua.Client;
using Opc.Ua;
using Thesis.Views;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private StackLayout stacklayout = new StackLayout();
        private static LabelViewModel textInfo = new LabelViewModel();
        public  static SampleClient OpcClient = new SampleClient(textInfo);
        public  static Session mySession;
        private string endpointUrl = null;
        private string User = null;
        private string Pass = null;
        private bool UserAuthor;

        public MainPage()
        {
           
            BindingContext = textInfo;
            InitializeComponent();
            BindingContext = new ConnectionListViewModel();
       
            MessagingCenter.Subscribe<Popup, ConnectType>(this, "AddOrEditConnection",
               (page, connectType) =>
               {
                   UserAuthor = connectType.ConnectionBool;
                   User = connectType.ConnectionUser;
                   Pass = connectType.ConnectionPass;
               });
        }

        private async void OnConnect(object sender, EventArgs e)
        {
           OpcClient.KeepAliveNotification += new KeepAliveEventHandler(Notification_KeepAlive);
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            ConnectType connectType = ((ConnectionListViewModel)BindingContext).Connections.Where(emp => emp.ConnectionId == (int)tappedEventArgs.Parameter).FirstOrDefault();
            endpointUrl = connectType.ConnectionUrl;
            if (endpointUrl != null)
            {
                bool connectToServer = true;
                await PopupNavigation.Instance.PushAsync(new ActivityIndicatorPage());

                await Task.Run(() => OpcClient.CreateCertificate());

                if (OpcClient.haveAppCertificate == false)
                {
                    connectToServer = await DisplayAlert("Warning", "missing application certificate, \nusing unsecure connection. \nDo you want to continue?", "Yes", "No");
                }

                if (connectToServer == true)
                {
                    var connectionStatus = await Task.Run(() => OpcClient.OpcClient(endpointUrl, UserAuthor, User, Pass));

                    if (connectionStatus == SampleClient.ConnectionStatus.Connected)
                    {
                        //Extract the session object for further direct session interactions
                        mySession = OpcClient.session;



                        AppMasterDetailPageDetail.connecttype = connectType;    
                        AppMasterDetailPageDetail.datetime = DateTime.Now.ToString();

                        await PopupNavigation.Instance.PopAsync();//turn off Activity Indicator
                        Tree tree;
                        tree = OpcClient.GetRootNode(textInfo);
                        SetTime.opcClient = OpcClient;
                        AppMasterDetailPageMaster.OpcClient_Master = OpcClient;
                        AppMasterDetailPage.tree_controlPage = tree;
                        AppMasterDetailPage.sampleClient_controlPage = OpcClient;
                        Page ControlPage = new AppMasterDetailPage();
                        NavigationPage.SetHasBackButton(ControlPage, false);
                        await Navigation.PushAsync(ControlPage);

                    }
                    else
                    {
                        await PopupNavigation.Instance.PopAllAsync();
                        await DisplayAlert("Warning", "Cannot connect to an OPC UA server", "Ok");
                        //await Navigation.PushAsync(new AlertPage("Cannot connect to an OPC UA server"));
                        //await Application.Current.MainPage.Navigation.PushAsync(new Popup());
                    }
                }
                else
                {
                    await PopupNavigation.Instance.PopAsync();
                }
            }
            else
            {
                await Navigation.PushAsync(new AlertPage("Server endpoint URL cannot be null"));

                //await DisplayAlert("Warning", "Server endpoint URL cannot be null", "Ok");
            }
        }

        private async void AddNewConnection_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new Popup());
        }

        private async void TapGestureRecognizer_Tapped_Edit(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            ConnectType connectType = ((ConnectionListViewModel)BindingContext).Connections.Where(emp => emp.ConnectionId == (int)tappedEventArgs.Parameter).FirstOrDefault();
            await PopupNavigation.Instance.PushAsync(new Popup(connectType));
        }

        private void TapGestureRecognizer_Tapped_Remove(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            ConnectType connectType = ((ConnectionListViewModel)BindingContext).Connections.Where(emp => emp.ConnectionId == (int)tappedEventArgs.Parameter).FirstOrDefault();

            ((ConnectionListViewModel)BindingContext).Connections.Remove(connectType);
        }

        private async void ToolbarItem_Clicked_About(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AboutPage());
        }

        private async void ToolbarItem_Clicked_Help(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new HelpPage());
        }

        private async void Notification_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            try
            {
                // check for events from discarded sessions.
                if (!Object.ReferenceEquals(sender, mySession))
                {
                    return;
                }

                // check for disconnected session.
                if (!ServiceResult.IsGood(e.Status))
                {
                    // try reconnecting using the existing session state
                    mySession.Reconnect();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Keep Alive: Error", ex.Message, "OK");
            }
        }
    }
}