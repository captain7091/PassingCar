using SQLite;

namespace PassingCar.LocalDatabase
{
    public class UserPhoto
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Base64Photo { get; set; }
    }
}
