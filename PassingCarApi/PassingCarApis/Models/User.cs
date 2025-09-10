using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace PassingCarApis.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? FacebookId { get; set; }
        public string? GoogleId { get; set; }
        public RegistrationType RegistrationType { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public byte[]? Photo { get; set; }
        public string? HashedPassword { get; set; }
        public bool Uber { get; set; }
        public bool Customer { get; set; }
        public bool JuridicPerson { get; set; }
        public JuridicDetails? JuridicDetails { get; set; }
        public AddressInfo? Address { get; set; }
        public PersonalInfo? PersonalInfo { get; set; }
        public int? Range { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public User() { }
        public User(UserDB user)
        {
            Id = user.Id;
            FacebookId = user.FacebookId;
            GoogleId = user.GoogleId;
            RegistrationType = user.RegistrationType;
            Name = user.Name;
            Surname = user.Surname;
            PhoneNumber = user.PhoneNumber;
            Email = user.Email;
            EmailVerified = user.EmailVerified;
            Photo = user.Photo;
            HashedPassword = user.HashedPassword;
            Uber = user.Uber;
            Customer = user.Customer;
            JuridicPerson = user.JuridicPerson;
            JuridicDetails = string.IsNullOrEmpty(user.JuridicDetails) ? null : JsonConvert.DeserializeObject<JuridicDetails>(user.JuridicDetails);
            Address = string.IsNullOrEmpty(user.Address) ? null : JsonConvert.DeserializeObject<AddressInfo>(user.Address);
            PersonalInfo = string.IsNullOrEmpty(user.PersonalInfo) ? null : JsonConvert.DeserializeObject<PersonalInfo>(user.PersonalInfo);
            Range = user.Range;
            CreatedAt = user.CreatedAt;
            ModifiedAt = user.ModifiedAt;
        }
    }
    public class UserExtended : User
    {
        public double? Rating { get; set; }
    }
    public enum RegistrationType
    {
        PassingCar,
        Google,
        Facebook
    }
    public class JuridicDetails
    {
        public string? CompanyName { get; set; }
        public string? NIF { get; set; }
        public bool IsShippingCompany { get; set; }
        public AddressInfo? AddressInfo { get; set; }
    }
    public class AddressInfo
    {
        public string? City { get; set; }
        public string? ZipCode { get; set; }
        public string? Address { get; set; }
        public string? Provincie { get; set; }
    }
    public class PersonalInfo
    {
        public string? Genre { get; set; }
        public string? IBAN { get; set; }
        public DateTime Birthday { get; set; }
        public string? DNI { get; set; }
    }
    [Table("[User]")]
    public class UserDB
    {
        public int Id { get; set; }
        public string? FacebookId { get; set; }
        public string? GoogleId { get; set; }
        public RegistrationType RegistrationType { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public byte[]? Photo { get; set; }
        public string? HashedPassword { get; set; }
        public bool Uber { get; set; }
        public bool Customer { get; set; }
        public bool JuridicPerson { get; set; }
        public string? JuridicDetails { get; set; }
        public string? Address { get; set; }
        public string? PersonalInfo { get; set; }
        public int? Range { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public UserDB() { }
        public UserDB(User user)
        {
            Id = user.Id;
            FacebookId = user.FacebookId;
            GoogleId = user.GoogleId;
            RegistrationType = user.RegistrationType;
            Name = user.Name;
            Surname = user.Surname;
            PhoneNumber = user.PhoneNumber;
            Email = user.Email;
            EmailVerified = user.EmailVerified;
            Photo = user.Photo;
            HashedPassword = user.HashedPassword;
            Uber = user.Uber;
            Customer = user.Customer;
            JuridicPerson = user.JuridicPerson;
            JuridicDetails = JsonConvert.SerializeObject(user.JuridicDetails);
            Address = JsonConvert.SerializeObject(user.Address);
            PersonalInfo = JsonConvert.SerializeObject(user.PersonalInfo);
            Range = user.Range;
            
            // Fix DateTime validation to prevent SqlDateTime overflow
            var minDate = new DateTime(1753, 1, 1);
            var maxDate = new DateTime(9999, 12, 31, 23, 59, 59);
            
            // Validate CreatedAt
            if (user.CreatedAt < minDate || user.CreatedAt > maxDate)
            {
                CreatedAt = DateTime.Now;
            }
            else
            {
                CreatedAt = user.CreatedAt;
            }
            
            // Validate ModifiedAt
            if (user.ModifiedAt.HasValue)
            {
                if (user.ModifiedAt.Value < minDate || user.ModifiedAt.Value > maxDate)
                {
                    ModifiedAt = DateTime.Now;
                }
                else
                {
                    ModifiedAt = user.ModifiedAt.Value;
                }
            }
            else
            {
                ModifiedAt = DateTime.Now;
            }
        }
    }
}
