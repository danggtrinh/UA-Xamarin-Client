using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Opc.Ua;

namespace Thesis.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Memory : ContentPage
    {
        public static SampleClient opcClient;
        List<string[]> inputData = new List<string[]>();
        public Memory()
        {
            InitializeComponent();
            opcClient = MainPage.OpcClient;
            inputData = new List<string[]>();
            IList<object> outputValues;
            try
            {
                //Call the method
                outputValues = opcClient.CallMethod("ns=3;s=\"DiagSoln_instMethodGetMemory\".Method", "ns=3;s=\"DiagSoln_instMethodGetMemory", inputData);

                //if (outputValues != null)
                //{
                //    if (outputValues.Count > 0) //if the method does not return a value
                //    {
                //        for (int i = 0; i < OutputList.Count; i++)
                //        {
                //            string outstring = "";
                //            if (OutputList[i].DataType == "ByteString")
                //            {
                //                outstring = BitConverter.ToString((byte[])outputValues[i]).Replace("-", string.Empty);
                //            }
                //            else
                //            {
                //                outstring = outputValues[i].ToString();
                //            }
                //            OutputList[i].Value = outstring;
                //        }
                //    }
                //}
                //Success; Status = Good
                DisplayAlert("Success", "Method called successfully.", "OK");
            }
            catch (Exception ex)
            {
                //Message contains status 
                DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}