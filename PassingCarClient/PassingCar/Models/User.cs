using Dapper.Contrib.Extensions;
using System;

namespace PassingCar.Models
{
    [Table("[User]")]
    public class User
    {
        public int Id { get; set; }
        public string FacebookId { get; set; }
        public string GoogleId { get; set; }
        public string AppleId { get; set; }
        public RegistrationType RegistrationType { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public byte[] Photo { get; set; }
        public string HashedPassword { get; set; }
        public bool Uber { get; set; }
        public bool Customer { get; set; }
        public bool JuridicPerson { get; set; }
        public JuridicDetails JuridicDetails { get; set; }
        public AddressInfo Address { get; set; }
        public PersonalInfo PersonalInfo { get; set; }
        public int? Range { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
    public class UserExtended : User
    {
        public double? Rating { get; set; }
    }
    public enum RegistrationType
    {
        PassingCar,
        Google,
        Facebook,
        Apple
    }
    public class JuridicDetails
    {
        public string CompanyName { get; set; }
        public string NIF { get; set; }
        public bool IsShippingCompany { get; set; }
        public AddressInfo AddressInfo { get; set; }
    }
    public class AddressInfo
    {
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public string Provincie { get; set; }
    }
    public class PersonalInfo
    {
        public string Genre { get; set; }
        public DateTime Birthday { get; set; }
        public string DNI { get; set; }
        public string IBAN { get; set; }
    }
}
