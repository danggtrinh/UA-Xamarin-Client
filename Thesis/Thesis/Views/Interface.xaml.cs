using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Interface : ContentPage
    {
        public static SampleClient myOpcClient;

        public Interface()
        {
            InitializeComponent();
        }
    }
}