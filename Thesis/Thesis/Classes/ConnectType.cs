using SQLite;

namespace Thesis
{
    public class ConnectType
    {
        [PrimaryKey]
        [AutoIncrement]
        public int ConnectionId { get; set; }

        public string ConnectionName { get; set; }
        public string ConnectionUrl { get; set; }
        public bool ConnectionBool { get; set; }
        public string ConnectionUser { get; set; }
        public string ConnectionPass { get; set; }

        public ConnectType(int id, string name, string url, bool userauthor, string username, string password)
        {
            ConnectionId = id;
            ConnectionName = name;
            ConnectionUrl = url;
            ConnectionBool = userauthor;
            ConnectionUser = username;
            ConnectionPass = password;
        }

        public ConnectType()
        {
        }
    }
}