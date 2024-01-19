using ModelMID;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Front.ViewModels
{
    public class ClientDetailsVM: ViewModelBase
    {
        public Client Client { get; set; }
        public ClientDetailsVM()
        {
            Global.OnClientChanged += (pClient) =>
            {
                Client = pClient;
                OnPropertyChanged(nameof(Client));
            };
        }
    }
}
