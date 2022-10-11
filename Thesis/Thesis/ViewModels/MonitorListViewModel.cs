using Microcharts;
using Opc.Ua;
using Opc.Ua.Client;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Entry = Microcharts.Entry;
using Orientation = Microcharts.Orientation;

namespace Thesis
{
    public class MonitorListViewModel //: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static string nodeid;
        private SampleClient opcClient;
        private MonitoredItem myMonitoredItem;
        private Subscription mySubscription;
        private Int16 itemCount;
        public ObservableCollection<MonitorNodeType> Monitors { get; set; } = new ObservableCollection<MonitorNodeType>();
        public Chart Chart0{ get; set; }
        public Chart Chart1 { get; set; }
        public List<Entry> _entries0;// = new List<Entry>();
        public List<Entry> _entries1;// = new List<Entry>();
        private string value;

        public MonitorListViewModel()
        {
            
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
            // UpdateChart();

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
                value = notification.Value.WrappedValue.ToString();
                UpdateChart();
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

        private void UpdateChart()
        {
            float dataitem = float.Parse(value);
            _entries0.Add(new Entry(dataitem)
            {
                Label = DateTime.Now.ToString("hh:mm:ss"),
                Color = SKColor.Parse("#aafb2f"),
                ValueLabel = value.ToString(),
            });
            //if (_entries0.Count == 8)
            //{
            //    _entries0.RemoveAt(0);
            //}
           
            this.Chart0 = new LineChart
            {
                Entries = _entries0,
                LabelTextSize = 30,
                LineSize = 8,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                IsAnimated = false,
                AnimationDuration = new TimeSpan(0),
                BackgroundColor = SKColors.Transparent
                                          ,
                PointMode = PointMode.Square,
                PointSize = 20,
                LineMode = LineMode.Straight
            };
            System.Diagnostics.Debug.WriteLine("done");

        }
    }
}