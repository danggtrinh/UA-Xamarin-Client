using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Entry = Microcharts.Entry;

namespace Thesis
{
    public class ChartVM : INotifyPropertyChanged
    {
        public Chart Data { get; private set; }
        public List<Entry> _entries = new List<Entry>();
        private SampleClient opcclient;

        public event PropertyChangedEventHandler PropertyChanged;

        public Chart Chart0 { get; set; }
        public string GraphNodeID { get; set; }
        public string GraphDate { get; set; }
        public string GraphValue { get; set; }
        public string GraphTime { get; set; }
        public bool vis { get; set; }

        public ChartVM(SampleClient _client, string _node)
        {
            opcclient = MainPage.OpcClient;
            if (_client != null)
            {
                if (_node != null)
                {
                    vis = true;
                    updatechart(_client, _node);
                }
                else
                {
                    vis = false;
                }
            }
            else
            {
                vis = false;
            }
        }

        private async void updatechart(SampleClient Client, string node)
        {
            while (true)
            {
                await Task.Delay(1000);
                string value = Client.VariableRead(node);
                float dataitem = float.Parse(value);

                //Random generator = new Random();
                //var color = String.Format("#{0:X6}", generator.Next(0x1000000));
                _entries.Add(new Entry(dataitem)
                {
                    Label = DateTime.Now.ToString("hh:mm:ss"),
                    Color = SKColor.Parse("#aafb2f"),
                    ValueLabel = dataitem.ToString(),
                });
                if (_entries.Count == 8)
                {
                    _entries.RemoveAt(0);
                }
                int index = node.IndexOf(';');
                string second = node.Substring(index + 1);
                this.GraphNodeID = second;
                this.GraphDate = DateTime.Now.ToString("MM/dd/yyyy");
                this.GraphValue = value;
                this.GraphTime = DateTime.Now.ToString("hh:mm:ss");

                this.Chart0 = new LineChart
                {
                    Entries = _entries,
                    LabelTextSize = 30,
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

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Chart0)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.GraphNodeID)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.GraphDate)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.GraphTime)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.GraphValue)));
            }
        }
    }
}