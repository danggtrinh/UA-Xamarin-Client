using Microcharts;
using Opc.Ua;
using Opc.Ua.Client;
using Rg.Plugins.Popup.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Entry = Microcharts.Entry;
using Orientation = Microcharts.Orientation;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MonitorPage : ContentPage
    {
        //private string value;
        public static string nodeid;
        private SampleClient opcClient;
        private MonitoredItem myMonitoredItem;
        private Subscription mySubscription;
        private Int16 itemCount;
        public ObservableCollection<MonitorNodeType> Monitors { get; set; } = new ObservableCollection<MonitorNodeType>();

        public MonitorPage()
        {
            
            //BindingContext = new MonitorListViewModel();
            this.BindingContext = this;
            
            opcClient = MainPage.OpcClient;
            OnSubcription();
            MessagingCenter.Subscribe<MonitorPage, string>(this, "FlagUnSub",
          (sender, arg) =>
          {
              opcClient.RemoveSubscription(mySubscription);
              mySubscription = null;
              itemCount = 0;
              opcClient = null;
              nodeid = null;
          });
            InitializeComponent();
        }

        public void OnSubcription()
        {

            if (myMonitoredItem != null)
            {
                try
                {
                    myMonitoredItem = opcClient.RemoveMonitoredItem(mySubscription, myMonitoredItem);
                }
                catch
                {
                    //ignore
                    ;
                }
            }

            try
            {
                if (nodeid == null || nodeid == "") { return; }
                else
                {
                    itemCount++;
                    string monitoredItemName = "myItem" + itemCount.ToString();
                    if (mySubscription == null)
                    {
                        mySubscription = opcClient.Subscribe(2000);
                    }

                    myMonitoredItem = opcClient.AddMonitoredItem(mySubscription, nodeid, monitoredItemName, 1);
                    opcClient.ItemChangedNotification += new MonitoredItemNotificationEventHandler(Notification_MonitoredItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex.ToString());
            }
        }

        private void Notification_MonitoredItem(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
            if (notification == null)
            {
                return;
            }
            else
            {
                //value = notification.Value.WrappedValue.ToString();
                MonitorNodeType monitorType = new MonitorNodeType();
                monitorType.MonitorName += monitoredItem.DisplayName;
                monitorType.MonitorValue += Utils.Format("{0}", notification.Value.WrappedValue.ToString());
                monitorType.MonitorSourceT += notification.Value.SourceTimestamp.ToString("hh:mm:ss");
                monitorType.MonitorServerT += notification.Value.ServerTimestamp.ToString("hh:mm:ss");
                monitorType.MonitorID = Monitors.Count + 1;
                int tmp = 0;
                for (int a = 0; a < Monitors.Count; a++)
                {
                    if (monitorType.MonitorName == Monitors[a].MonitorName)
                    {
                        Monitors.RemoveAt(a);
                        Monitors.Add(monitorType);
                        tmp = 1;
                    }
                }
                if (tmp == 0)
                {
                    Monitors.Add(monitorType);
                }
            }
        }

   
        private void TapGestureRecognizer_Tapped_Remove(object sender, EventArgs e)
        {
            try
            {
                TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
                MonitorNodeType monitorType = ((MonitorPage)BindingContext).Monitors.Where(emp => emp.MonitorID == (int)tappedEventArgs.Parameter).FirstOrDefault();

                ((MonitorPage)BindingContext).Monitors.Remove(monitorType);
                MessagingCenter.Send<MonitorPage, string>(this, "FlagUnSub", "UnSubTrue");
            }
            catch
            {
                return;
            }
        }

        private async void ToolbarItem_Clicked_About(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AboutPage());
        }

        private async void ToolbarItem_Clicked_Help(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new HelpPage());
        }
    }
}