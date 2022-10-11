using SQLite;

namespace Thesis
{
    public class DBItem
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public string Label { get; set; }
        public double Value { get; set; }
    }
}