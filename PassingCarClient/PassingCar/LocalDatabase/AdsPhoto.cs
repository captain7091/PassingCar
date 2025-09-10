using SQLite;

namespace PassingCar.LocalDatabase
{
    public class AdsPhoto
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Base64Photo { get; set; }
    }
}
