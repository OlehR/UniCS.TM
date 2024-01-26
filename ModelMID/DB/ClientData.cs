namespace ModelMID.DB
{
    public enum eTypeDataClient
    {
        BarCode = 1,
        Phone = 2
    }
    public class ClientData
    {
        public eTypeDataClient TypeData { get; set; }
        public long CodeClient { get; set; }
        public string Data { get; set; }
    }
}
