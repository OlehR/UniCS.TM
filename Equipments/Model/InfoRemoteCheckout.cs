using ModelMID;
using Utils;
using System.Collections.ObjectModel;

namespace Equipments.Model
{
    public class InfoRemoteCheckout
    {
        public eStateMainWindows StateMainWindows { get; set; } = eStateMainWindows.NotDefine;
        public string TransleteStateMainWindows { get { return StateMainWindows.GetDescription(); } }
        public int RemoteIdWorkPlace { get; set; } = Global.IdWorkPlace;
        public eTypeAccess TypeAccess { get; set; } = eTypeAccess.NoDefine;
        public string TextInfo { get; set; } = string.Empty;
        public string UserBarcode { get; set; } = string.Empty;
        public ObservableCollection<Price> RemoteCigarettesPrices { get; set; } = new();
        public Price SelectRemoteCigarettesPrice { get; set; } = null;
        public int QuantityCigarettes { get; set; } = 1;
    }
}
