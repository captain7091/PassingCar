using SQLite;

namespace PassingCar.LocalDatabase
{
    public class LocalFavorites
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int AdsId { get; set; }
    }
}
