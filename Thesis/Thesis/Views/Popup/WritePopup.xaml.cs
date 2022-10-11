using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WritePopup
    {
        private SampleClient opcClient;
        private ListNode selected;
        private string datatype;

        public WritePopup(SampleClient Client, ListNode id, string _datatype)
        {
            InitializeComponent();
            opcClient = Client;
            selected = id;
            TitleNode.Text = "Change value of : " + id.NodeName;
            datatype = _datatype;
            if (datatype == "Boolean")
            {
                FixType.IsVisible = false;
                BooleanType.IsVisible = true;
            }
            else
            {
                FixType.IsVisible = true;
                BooleanType.IsVisible = false;
            }
        }

        private void Button_Clicked_Cancel(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
        }

        private void Button_Clicked_Change(object sender, EventArgs e)
        {
            string idnode = selected.id;
            List<String> values = new List<string>();
            List<String> nodeIdStrings = new List<string>();
            nodeIdStrings.Add(idnode);

            if (datatype == "Boolean")
            {
                if (TrueCheck.IsChecked == true)
                {
                    values.Add("true");
                }
                if (FalseCheck.IsChecked == true)
                {
                    values.Add("false");
                }
            }
            else
            {
                values.Add(ValueChange.Text);
            }
            values.Add(ValueChange.Text);

            try
            {
                opcClient.VariableWrite(values, nodeIdStrings);
            }
            catch (Exception ex)
            {
                DisplayAlert("Alarm", "Can't Write" + ex.ToString(), "OK");
            }
            PopupNavigation.Instance.PopAllAsync();
        }

        private void TrueCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (TrueCheck.IsChecked == true)
            {
                FalseCheck.IsChecked = false;
            }
        }

        private void FalseCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (FalseCheck.IsChecked == true)
            {
                TrueCheck.IsChecked = false;
            }
        }
    }
}