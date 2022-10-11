namespace Thesis
{
    public class MonitorNodeType
    {
        public int MonitorID { get; set; }
        public string MonitorName { get; set; }
        public string MonitorValue { get; set; }
        public string MonitorSourceT { get; set; }
        public string MonitorServerT { get; set; }

        public MonitorNodeType(int id, string name, string value, string source, string server)
        {
            MonitorID = id;
            MonitorName = name;
            MonitorValue = value;
            MonitorSourceT = source;
            MonitorServerT = server;
        }

        public MonitorNodeType()
        {
        }
    }
}