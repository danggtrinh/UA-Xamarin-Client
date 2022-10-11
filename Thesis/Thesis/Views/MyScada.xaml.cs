using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Entry = Microcharts.Entry;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyScada : TabbedPage, INotifyPropertyChanged
    {
        private SampleClient opcClient;
        private List<String> values;
        private List<String> nodeIdStrings;
        private List<Entry> Gauge1; 
        private List<Entry> Gauge2;
        public float Level1,Level2;
        public Chart Chart1 { get; set; }
        public Chart Chart2 { get; set; }
        private int max_tank = 1000, min_tank = 0;
        private string color1, color2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string textstring { get; set; }

        public MyScada()
        {
            InitializeComponent();

            opcClient = MainPage.OpcClient;
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    HomeTabRead();
                    PumpTabRead();
                    TankTabRead();
                    GateTabRead();
                    GaugeTank1();
                    GaugeTank2();
                    this.BindingContext = this;
                });
                return true;
            });
        }

        private void GaugeTank1()
        {
            var uplimit = opcClient.VariableRead("ns=3;s=\"block\".\"Tank1\".\"UppperLimit\"");
            var downlimit = opcClient.VariableRead("ns=3;s=\"block\".\"Tank1\".\"LowerLimit\"");
            var level = opcClient.VariableRead("ns=3;s=\"block\".\"Tank1\".\"Level\"");
            double up = Convert.ToDouble(uplimit);
            double down = Convert.ToDouble(downlimit);
            double le = Convert.ToDouble(level);
            Up1.Text = "Uppper Limit:" + uplimit.ToString();
            Le1.Text = "Level Tank :" + level.ToString();
            Do1.Text = "Lower Limit :" + downlimit.ToString();
            if (le >= up || le <= down)
            {
                color1 = "#c90c0c";
            }
            else if ((le > (up - 100)) || (le < (down + 100)))
            {
                color1 = "#f5e50f";
            }
            else { color1 = "#54d419"; }

            if (level != null | level != "")
            {
                Level1 = Convert.ToSingle(level);
                Gauge1 = new List<Entry>
            {
                    new Entry(Level1)
                {
                    Color=SKColor.Parse(color1),
                    Label = "Level",
                    ValueLabel = Level1.ToString(),
                },
                new Entry(max_tank-Level1)
                {
                    Color=SKColor.Parse("#ababab"),
                    Label = "",
                    ValueLabel =""
                }
            };
                Chart1 = new DonutChart()
                {
                    AnimationDuration = new TimeSpan(0),
                    IsAnimated = false,
                    Entries = Gauge1,
                    MinValue = min_tank,
                    MaxValue = max_tank,
                    BackgroundColor = SKColor.Empty,
                    LabelTextSize = 30f,
                    HoleRadius = 0,

                };

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Chart1)));

            }
        }

        private void GaugeTank2()
        {
            var uplimit = opcClient.VariableRead("ns=3;s=\"block\".\"Tank2\".\"UppperLimit\"");
            var downlimit = opcClient.VariableRead("ns=3;s=\"block\".\"Tank2\".\"LowerLimit\"");
            var level = opcClient.VariableRead("ns=3;s=\"block\".\"Tank2\".\"Level\"");
            double up = Convert.ToDouble(uplimit);
            double down = Convert.ToDouble(downlimit);
            double le = Convert.ToDouble(level);
            Up2.Text = "Uppper Limit:" + uplimit.ToString();
            Le2.Text = "Level Tank :" + level.ToString();
            Do2.Text = "Lower Limit :" + downlimit.ToString();
            if (le >= up || le <= down)
            {
                color2 = "#c90c0c";
            }
            else if ((le > (up - 100)) || (le < (down + 100)))
            {
                color2 = "#f5e50f";
            }
            else { color2 = "#54d419"; }

            if (level != null | level != "")
            {
                Level2 = Convert.ToSingle(level);
                Gauge2 = new List<Entry>
            {
                    new Entry(Level2)
                {
                    Color=SKColor.Parse(color2),
                    Label = "Level",
                    ValueLabel = Level2.ToString(),
                },
                new Entry(max_tank-Level2)
                {
                    Color=SKColor.Parse("#ababab"),
                    Label = "",
                    ValueLabel =""
                }
            };
                Chart2 = new DonutChart()
                {
                    AnimationDuration = new TimeSpan(0),
                    IsAnimated = false,
                    Entries = Gauge2,
                    MinValue = min_tank,
                    MaxValue = max_tank,
                    BackgroundColor = SKColor.Empty,
                    LabelTextSize = 30f,
                    HoleRadius = 0,

                };

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Chart1)));

            }
        }


        private void HomeTabRead()
        {
            //ToggleModeWrite
            if (Mode.IsToggled == true)
            {
                values = new List<string>();
                nodeIdStrings = new List<string>();
                values.Add("True");
                nodeIdStrings.Add("ns=3;s=\"block\".\"ManualAuto\"");
                try
                {
                    opcClient.VariableWrite(values, nodeIdStrings);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.ToString(), "OK");
                }
            }
            else
            {
                values = new List<string>();
                nodeIdStrings = new List<string>();
                values.Add("False");
                nodeIdStrings.Add("ns=3;s=\"block\".\"ManualAuto\"");
                try
                {
                    opcClient.VariableWrite(values, nodeIdStrings);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.ToString(), "OK");
                }
            }

            //ma=ManualAuto
            var ma = opcClient.VariableRead("ns=3;s=\"block\".\"ManualAuto\"");
            if (ma == "True")
            {
                MaAu.Text = "Auto";
            }
            else
            {
                MaAu.Text = "Manual";
            }

            //staled=StatusLed
            var staled = opcClient.VariableRead("ns=3;s=\"block\".\"StatusLed\"");
            if (staled == "True")
            {
                Statusled.Source = "pointgreen.png";
            }
            else
            {
                Statusled.Source = "pointoff.png";
            }

            //fauled=FaultLed
            var fauled = opcClient.VariableRead("ns=3;s=\"block\".\"FaultLed\"");
            if (fauled == "True")
            {
                Faultled.Source = "pointred.png";
            }
            else
            {
                Faultled.Source = "pointoff.png";
            }
        }

        private void PumpTabRead()
        {
            //run1=Run Pump1
            var run1 = opcClient.VariableRead("ns=3;s=\"block\".\"Pump1\".\"Run\"");
            if (run1 == "True")
            {
                Pump1.Source = "motoron.png";
            }
            else
            {
                Pump1.Source = "motoroff.png";
            }
            //run2=Run Pump2
            var run2 = opcClient.VariableRead("ns=3;s=\"block\".\"Pump2\".\"Run\"");
            if (run2 == "True")
            {
                Pump2.Source = "motoron.png";
            }
            else
            {
                Pump2.Source = "motoroff.png";
            }
        }

        private void TankTabRead()
        {
            //lev1=Level Water Tank1
            var lev1 = opcClient.VariableRead("ns=3;s=\"block\".\"Tank1\".\"Level\"");
            if (lev1 != null)
            {
                level1.Text = lev1;
            }
            else { return; }
            //lev2=Level Water Tank2
            var lev2 = opcClient.VariableRead("ns=3;s=\"block\".\"Tank2\".\"Level\"");
            if (lev2 != null)
            {
                level2.Text = lev2;
            }
            else { return; }
            //sta1=Status Tank1 0:Emty - 1:Normal - 2:Full
            var sta1 = opcClient.VariableRead("ns=3;s=\"block\".\"Tank1\".\"Status\"");
            if (sta1 == "0")
            {
                status1.Text = "Emty";
            }
            else if (sta1 == "1")
            {
                status1.Text = "Normal";
            }
            else if (sta1 == "2")
            {
                status1.Text = "Full";
            }
            else { return; }
            //sta2=Status Tank2 0:Emty - 1:Normal - 2:Full
            var sta2 = opcClient.VariableRead("ns=3;s=\"block\".\"Tank2\".\"Status\"");
            if (sta2 == "0")
            {
                status2.Text = "Emty";
            }
            else if (sta2 == "1")
            {
                status2.Text = "Normal";
            }
            else if (sta2 == "2")
            {
                status2.Text = "Full";
            }
            else { return; }
            //stava1=Status Valve1
            var stava1 = opcClient.VariableRead("ns=3;s=\"block\".\"Tank1\".\"Run\"");
            if (stava1 == "True")
            {
                statusvalve1.Source = "pointgreen.png";
            }
            else
            {
                statusvalve1.Source = "pointoff.png";
            }
            //stava2=Status Valve2
            var stava2 = opcClient.VariableRead("ns=3;s=\"block\".\"Tank2\".\"Run\"");
            if (stava2 == "True")
            {
                statusvalve2.Source = "pointgreen.png";
            }
            else
            {
                statusvalve2.Source = "pointoff.png";
            }
        }

        private void GateTabRead()
        {
            //staga1=Status Gate1
            var staga1 = opcClient.VariableRead("ns=3;s=\"block\".\"Gate1\".\"Status\"");
            if (staga1 == "True")
            {
                statusgate1.Text = "Opening";
            }
            else
            {
                statusgate1.Text = "Closing";
            }

            //staga2=Status Gate2
            var staga2 = opcClient.VariableRead("ns=3;s=\"block\".\"Gate2\".\"Status\"");
            if (staga2 == "True")
            {
                statusgate2.Text = "Opening";
            }
            else
            {
                statusgate2.Text = "Closing";
            }
        }

        private void Emer_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Emer\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
            values = new List<string>();
            values.Add("False");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Reset_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Reset\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
            values = new List<string>();
            values.Add("False");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Change_Date_Clicked(object sender, EventArgs e)
        {
            if (DT.Text != null)
            {
                values = new List<string>();
                nodeIdStrings = new List<string>();
                values.Add(DT.Text);
                nodeIdStrings.Add("ns=3;s=\"block\".\"Date\"");
                try
                {
                    opcClient.VariableWrite(values, nodeIdStrings);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.ToString(), "OK");
                }
            }
            else
            {
                DisplayAlert("Warning", "Type DateTime", "OK");
            }
        }

        private void Start_Pump1_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Pump1\".\"Start\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
            values = new List<string>();
            values.Add("False");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Stop_Pump1_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Pump1\".\"Stop\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
            values = new List<string>();
            values.Add("False");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Start_Pump2_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Pump2\".\"Start\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
            values = new List<string>();
            values.Add("False");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Stop_Pump2_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Pump2\".\"Stop\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
            values = new List<string>();
            values.Add("False");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Change_Tank1_Clicked(object sender, EventArgs e)
        {
            if (up1.Text != null && up1.Text != "")
            {
                values = new List<string>();
                nodeIdStrings = new List<string>();
                values.Add(up1.Text);
                nodeIdStrings.Add("ns=3;s=\"block\".\"Tank1\".\"UppperLimit\"");
                try
                {
                    opcClient.VariableWrite(values, nodeIdStrings);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.ToString(), "OK");
                }
            }
            if (low1.Text != null && low1.Text != "")
            {
                values = new List<string>();
                nodeIdStrings = new List<string>();
                values.Add(low1.Text);
                nodeIdStrings.Add("ns=3;s=\"block\".\"Tank1\".\"LowerLimit\"");
                try
                {
                    opcClient.VariableWrite(values, nodeIdStrings);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.ToString(), "OK");
                }
            }
        }

        private void Change_Tank2_Clicked(object sender, EventArgs e)
        {
            if (up2.Text != null && up2.Text != "")
            {
                values = new List<string>();
                nodeIdStrings = new List<string>();
                values.Add(up2.Text);
                nodeIdStrings.Add("ns=3;s=\"block\".\"Tank2\".\"UppperLimit\"");
                try
                {
                    opcClient.VariableWrite(values, nodeIdStrings);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.ToString(), "OK");
                }
            }

            if (low2.Text != null && low2.Text != "")
            {
                values = new List<string>();
                nodeIdStrings = new List<string>();
                values.Add(low2.Text);
                nodeIdStrings.Add("ns=3;s=\"block\".\"Tank2\".\"LowerLimit\"");
                try
                {
                    opcClient.VariableWrite(values, nodeIdStrings);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.ToString(), "OK");
                }
            }
        }

        private void Start_Valve1_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Tank1\".\"Start\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Start_Valve2_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Tank2\".\"Start\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Stop_Valve1_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Tank1\".\"Stop\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Stop_Valve2_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Tank2\".\"Stop\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Open_Gate1_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Gate1\".\"Open\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Open_Gate2_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Gate2\".\"Open\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Close_Gate1_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Gate1\".\"Close\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void Close_Gate2_Clicked(object sender, EventArgs e)
        {
            values = new List<string>();
            nodeIdStrings = new List<string>();
            values.Add("True");
            nodeIdStrings.Add("ns=3;s=\"block\".\"Gate2\".\"Close\"");
            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.ToString(), "OK");
            }
        }
    }
}