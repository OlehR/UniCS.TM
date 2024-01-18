using ModelMID;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Front.ViewModels
{
    public class ClientDetailsVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
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
