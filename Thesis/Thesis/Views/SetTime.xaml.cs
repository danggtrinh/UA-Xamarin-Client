using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Opc.Ua;


namespace Thesis.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetTime : ContentPage,  INotifyPropertyChanged
    {

        public static SampleClient opcClient;
        
        private List<String> values;
        private List<String> nodeIdStrings;
        public event PropertyChangedEventHandler PropertyChanged;
        
        public SetTime()
        {
            opcClient = MainPage.OpcClient;
            //opcClient.session = MainPage.mySession;

            //day1Text.Text= "null";

            //try

            //{
            //    values = opcClient.ReadValues(nodeIdStrings);
            //    day1Text.Text = values.ElementAt<String>(0);
            //}
            //catch (Exception ex)
            //{
            //    DisplayAlert("", ex.Message, "Error");
            //}

          

            InitializeComponent();


            List<String> nodeIdStrings = new List<String>();
            List<String> values = new List<String>();
            string test = "ns = 3; s = \"DiagSoln_DiagnosticsData\".\"rdSysTime\".\"SysTime\"";
            Console.WriteLine(test);
            nodeIdStrings.Add("ns = 3; s = \"DiagSoln_DiagnosticsData\".\"CycleTime\".\"LongestCycleTime\"");
            try
            {
               
                values = opcClient.ReadValues(nodeIdStrings);
                
                string val = values.ElementAt<String>(0);
                Console.WriteLine(val);
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok con de");
            }





            //try
            //{

            //    var value = opcClient.VariableRead("ns = 3; s = \"DiagSoln_DiagnosticsData\".\"rdSysTime\".\"SysTime\"");
            //    string varReturn = value.ToString();
            //    System.Diagnostics.Debug.WriteLine(value);

            //}
            //catch (Exception ex)
            //{
            //    DisplayAlert("", ex.Message, "ok con de");
            //}





            //try
            //{
            //    Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            //    {
            //    //do something every 60 seconds
            //    Device.BeginInvokeOnMainThread(() =>
            //        {
            //            GetSystime();
            //        });
            //        return true; // runs again, or false to stop
            //});
            //    //List<String> nodeIdStrings = new List<String>();
            //    //List<String> values = new List<String>();
            //    //nodeIdStrings.Add("ns = 3; s = \"DiagSoln_DiagnosticsData\".\"rdSysTime\".\"SysTime\"");
            //}
            //catch(Exception ex)
            //{
            //    DisplayAlert("", ex.Message, "Error");
            //}
        }
        private void GetSystime()
        {
            var value = opcClient.VariableRead("ns = 3; s = \"DiagSoln_DiagnosticsData\".\"rdSysTime\".\"SysTime\"");

            System.Diagnostics.Debug.WriteLine(value);
        }
        private void syncBtn_Clicked(object sender, EventArgs e)
        {

        }
        //public static void StartTimer(TimeSpan interval, Func<bool> callback);
    }

}