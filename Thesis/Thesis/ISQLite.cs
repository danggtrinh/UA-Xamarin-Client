using SQLite;

namespace Thesis
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}