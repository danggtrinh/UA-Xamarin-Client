using SQLite;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Thesis
{
    public class DBItemController
    {
        private static object locker = new object();
        private SQLiteConnection database;

        public DBItemController()
        {
            this.database = DependencyService.Get<ISQLite>().GetConnection();
            this.database.CreateTable<ConnectType>();
        }

        public IEnumerator<ConnectType> GetDBItems()
        {
            lock (locker)
            {
                if (this.database.Table<ConnectType>().Count() == 0)
                {
                    return null;
                }
                else
                {
                    return this.database.Table<ConnectType>().GetEnumerator();
                }
            }
        }

        public int SaveDBItem(ConnectType item)
        {
            lock (locker)
            {
                if (item.ConnectionId != 0)
                {
                    this.database.Update(item);
                    return item.ConnectionId;
                }
                else
                {
                    return this.database.Insert(item);
                }
            }
        }

        public int DeleteDBItem(int id)
        {
            lock (locker)
            {
                return this.database.Delete<DBItem>(id);
            }
        }
    }
}