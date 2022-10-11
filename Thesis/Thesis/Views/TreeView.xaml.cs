using Microcharts;
using Opc.Ua;
using Rg.Plugins.Popup.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Entry = Microcharts.Entry;
using Orientation = Microcharts.Orientation;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreeView : TabbedPage, INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<ListNode> nodes = new ObservableCollection<ListNode>();
        public static SampleClient opcClient;
        private Tree storedTree;

        public List<Entry> _entries;//= new List<Entry>();
        public Chart Chart0 { get; set; }
        public bool vis { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private float dataitem, prevalue;
        #endregion Fields

        #region TreeView

        public TreeView(Tree tree, SampleClient client)
        {
            InitializeComponent();
            BindingContext = nodes;
            storedTree = tree;
            opcClient = client;
            DisplayNodes();

        }

        #endregion TreeView

        #region DisplayNodes

        private void DisplayNodes()
        {
            nodes.Clear();

            foreach (var node in storedTree.currentView)
            {
                nodes.Add(node);
            }

            //defined in XAML to follow
            treeView.ItemsSource = null;
            treeView.ItemsSource = nodes;
        }

        private async void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            treeView.SelectedItem = null; // deselect row
            ListNode selected = e.SelectedItem as ListNode;

            if (selected.children == true)
            {
                storedTree = opcClient.GetChildren(selected.id);

                Page treeViewPage = new TreeView(storedTree, opcClient);
                treeViewPage.Title = this.Title + "/" + selected.NodeName;
                await Navigation.PushAsync(treeViewPage);
            }
        }

        #endregion DisplayNodes

        #region Read/Write

        public async void OnRead(object sender, EventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                var selected = menu.CommandParameter as ListNode;
                var value = opcClient.VariableRead(selected.id);
                List<string> datatype = new List<string>();
                VariableNode variablenode = new VariableNode();
                opcClient.Read_Datatype(opcClient, selected.id, out datatype, out variablenode);
                await PopupNavigation.Instance.PushAsync(new AttributeReadingNode(selected, value, datatype[0], variablenode));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void OnWrite(object sender, EventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                var selected = menu.CommandParameter as ListNode;
                List<string> datatype = new List<string>();
                VariableNode variablenode = new VariableNode();
                opcClient.Read_Datatype(opcClient, selected.id, out datatype, out variablenode);
                await PopupNavigation.Instance.PushAsync(new WritePopup(opcClient, selected, datatype[0]));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Read/Write

        #region Subcription
        int cnt = 0;
        public void OnSubcription(object sender, EventArgs e)
        {
            cnt++;
            if (cnt == 4)
            {
                try
                {
                    var menu = sender as MenuItem;
                    var selected = menu.CommandParameter as ListNode;
                    MonitorPage.nodeid = selected.id;
                    //MonitorListViewModel.nodeid = selected.id;
                    Page monitorPage = new MonitorPage();
                    Navigation.PushAsync(monitorPage);
                    cnt = 0;

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
           
          
        }

        #endregion Subcription

        #region Graph

        private void OnGraph(object sender, EventArgs e)
        {
            var menu = sender as MenuItem;
            var selected = menu.CommandParameter as ListNode;
            List<string> datatype = new List<string>();
            VariableNode variablenode = new VariableNode();
            opcClient.Read_Datatype(opcClient, selected.id, out datatype, out variablenode);
            if (datatype[0] == "Float" || datatype[0] == "Byte" || datatype[0] == "Int16" || datatype[0] == "Int32" || datatype[0] == "Int64" || datatype[0] == "SByte" || datatype[0] == "Double" || datatype[0] == "UInt16" || datatype[0] == "UInt32" || datatype[0] == "UInt64")
            {
                _entries = new List<Entry>();
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        updatechart(selected.id.ToString());
                        BindingContext = this;
                    });
                    return true;
                });
                CurrentPage = Children[1];
            }
            else
            {
                return;
            }
        }

        private void updatechart(string node)
        {
            string value = opcClient.VariableRead(node);
            dataitem = float.Parse(value);
            if (prevalue != dataitem)
            {
                prevalue = dataitem;
                int index = node.IndexOf(';');
                string second = node.Substring(index + 1);
                GraphNodeID.Text = "NodeID : " + second;
                GraphDate.Text = "Date : " + DateTime.Now.ToString("MM /dd/yyyy");
                GraphValue.Text = "Value : " + value;
                GraphTime.Text = "Time : " + DateTime.Now.ToString("hh:mm:ss");

                _entries.Add(new Entry(dataitem)
                {
                    Label = DateTime.Now.ToString("hh:mm:ss"),
                    Color = SKColor.Parse("#aafb2f"),
                    ValueLabel = dataitem.ToString(),
                });
                Chart_Read.WidthRequest += 100;
                Chart_Read.Chart = new LineChart
                {
                    Entries = _entries,
                    LabelTextSize = 30,
                    LabelOrientation = Orientation.Horizontal,
                    ValueLabelOrientation = Orientation.Horizontal,
                    IsAnimated = false,
                    AnimationDuration = new TimeSpan(0),
                    BackgroundColor = SKColors.Empty,
                    PointMode = PointMode.Square,
                    PointSize = 20,
                    LineMode = LineMode.Straight
                };
            }
            else
            {
                return;
            }
                //Random generator = new Random();
                //var color = String.Format("#{0:X6}", generator.Next(0x1000000));
               
                //if (_entries.Count == 8)
                //{
                //    _entries.RemoveAt(0);
                //}
        }
        #endregion Graph

        #region MenuItems

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();

            if (BindingContext == null)
            {
                return;
            }

            ViewCell viewCell = sender as ViewCell;
            var item = viewCell.BindingContext as ListNode;
            viewCell.ContextActions.Clear();

            if (item != null)
            {
                if (item.nodeClass == "Variable")
                {
                    MenuItem WNode = new MenuItem { Text = "Write" };
                    MenuItem RNode = new MenuItem { Text = "Read" };
                    MenuItem Graph = new MenuItem { Text = "Graph" };
                    MenuItem Sub = new MenuItem { Text = "Sub" };
                    //viewCell.ContextActions.Add(new MenuItem() {Text = "Read"});

                    viewCell.ContextActions.Add(WNode);
                    viewCell.ContextActions.Add(RNode);
                    viewCell.ContextActions.Add(Graph);
                    viewCell.ContextActions.Add(Sub);
                    foreach (var action in viewCell.ContextActions)
                    {
                        action.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                        RNode.Clicked += OnRead;
                        WNode.Clicked += OnWrite;
                        Sub.Clicked += OnSubcription;
                        Graph.Clicked += OnGraph;
                    }
                }
            }
        }

        #endregion MenuItems

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