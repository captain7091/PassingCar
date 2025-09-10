using System;

namespace PassingCar.Models.API.Photo
{
    public class StoragePhoto
    {
        public string Photo { get; set; }
        public DateTime LastAccessTime { get; set; }
        public byte[] GetPhoto()
        {
            return string.IsNullOrEmpty(Photo) ? null : Convert.FromBase64String(Photo);
        }
    }
}
