using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppMasterDetailPage : MasterDetailPage
    {
        private static LabelViewModel textInfo = new LabelViewModel();
        public static Tree tree_controlPage;
        public static SampleClient sampleClient_controlPage;

        public AppMasterDetailPage()
        {
            InitializeComponent();
            MasterPage.ListView.ItemSelected += ListView_ItemSelected;
            BindingContext = textInfo;
            //xtree = tree;
            //sampleClient = sample;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as AppMasterDetailPageMenuItem;
            if (item == null)
                return;
            if (item.Id == 1)
            {
                Page treeViewRoot = new TreeView(tree_controlPage, sampleClient_controlPage);
                treeViewRoot.Title = "/Root";
                Detail = new NavigationPage(treeViewRoot);
                IsPresented = false;
            }
            else
            {
                var page = (Page)Activator.CreateInstance(item.TargetType);
                page.Title = item.Title;

                Detail = new NavigationPage(page);
                IsPresented = false;
            }

            MasterPage.ListView.SelectedItem = null;
        }
    }
}