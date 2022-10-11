using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace Thesis
{
    public class ConnectionListViewModel
    {
        public string NameConnect { get; set; }
        public string UrlConnect { get; set; }
        public ObservableCollection<ConnectType> Connections { get; set; }
        public ICommand AddCommand => new Command(AddConnectionName);

        public ConnectionListViewModel()
        {
            Connections = new ObservableCollection<ConnectType>()
            {
                new ConnectType(1,"ServerA","opc.tcp://192.168.1.1:4852",false,"",""),
                new ConnectType(2,"ServerB","opc.tcp://192.168.0.1:4840",false,"",""),
                new ConnectType(3,"ServerC","opc.tcp://localhost:53530/OPCUA/SimulationServer",false,"",""),
               
            };

            MessagingCenter.Subscribe<Popup, ConnectType>(this, "AddOrEditConnection",
                (page, connectType) =>
                {
                    if (connectType.ConnectionId == 0)
                    {
                        connectType.ConnectionId = Connections.Count + 1;
                        Connections.Add(connectType);
                    }
                    else
                    {
                        ConnectType connectTypeToEdit = Connections.Where(emp => emp.ConnectionId == connectType.ConnectionId).FirstOrDefault();
                        int newIndex = Connections.IndexOf(connectTypeToEdit);
                        Connections.Remove(connectTypeToEdit);
                        Connections.Add(connectType);
                        int oldIndex = Connections.IndexOf(connectType);
                        Connections.Move(oldIndex, newIndex);
                    }
                }
                );
        }

        public void AddConnectionName()
        {
            //Connections.Add(new ConnectType { ConnectionName = NameConnect, ConnectionUrl = UrlConnect });
            //await App.Current.MainPage.DisplayAlert("Alert", NameConnect, "OK");
        }
    }
}