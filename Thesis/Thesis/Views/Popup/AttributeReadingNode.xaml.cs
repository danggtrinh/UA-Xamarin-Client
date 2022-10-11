using Opc.Ua;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AttributeReadingNode
    {
        public AttributeReadingNode(ListNode listnode, string value, string datatype, VariableNode variableNode)
        {
            InitializeComponent();

            NodeName.Text = ":  " + listnode.NodeName;
            NodeId.Text = ":  " + listnode.id;
            NodeClass.Text = ":  " + listnode.nodeClass;
            AccessLevel.Text = ":  " + listnode.accessLevel;
            EventNotifier.Text = ":  " + listnode.eventNotifier;
            Executable.Text = ":  " + listnode.executable;
            Children.Text = ":  " + listnode.children.ToString();
            Value.Text = ":  " + value;
            Datatype.Text = ":  " + datatype;

            ValueRank.Text = ":  " + variableNode.ValueRank.ToString();
            MinimumSampling.Text = ":  " + variableNode.MinimumSamplingInterval.ToString();
            Historizing.Text = ":  " + variableNode.Historizing.ToString();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
        }
    }
}