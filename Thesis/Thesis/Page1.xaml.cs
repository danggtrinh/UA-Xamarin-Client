using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Page1 : ContentPage
	{
        public string value { get; set; }
        public Page1 ()
		{
			
            this.BindingContext = this;
            this.value = "hello";
            InitializeComponent();
        }
	}
}