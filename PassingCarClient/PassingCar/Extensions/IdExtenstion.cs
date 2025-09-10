using Newtonsoft.Json;
using PassingCar.IntegrationsWithApi;
using PassingCar.LocalDatabase;
using PassingCar.Models.API.Photo;


namespace PassingCar.Extensions
{
    public static class IdExtenstion
    {
        // Add memory cache for better performance
        private static readonly Dictionary<int, byte[]> _userPhotoCache = new();
        private static readonly Dictionary<int, byte[]> _adsPhotoCache = new();
        private static readonly object _cacheLock = new();

        public static async Task<byte[]> GetUserPhoto(this int Id)
        {
            try
            {
                // Check memory cache first
                lock (_cacheLock)
                {
                    if (_userPhotoCache.TryGetValue(Id, out byte[] cachedPhoto))
                    {
                        return cachedPhoto;
                    }
                }

                UserPhoto userPhoto = null;
                try
                {
                    userPhoto = await App.LocalDatabase.GetUserPhoto(Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (userPhoto != null && userPhoto.Id >= 0)
                {
                    var photoBytes = Convert.FromBase64String(userPhoto.Base64Photo);

                    // Cache the result
                    lock (_cacheLock)
                    {
                        _userPhotoCache[Id] = photoBytes;
                    }

                    return photoBytes;
                }
                else
                {
                    GetPhotoResponse photo = await Api.GetUserPhoto(new GetPhotoRequest()
                    {
                        Id = Id,
                    });
                    if (photo != null && photo.Success)
                    {
                        _ = await App.LocalDatabase.SaveUserPhoto(new UserPhoto()
                        {
                            Base64Photo = Convert.ToBase64String(photo.Photo),
                            Id = Id
                        });

                        // Cache the result
                        lock (_cacheLock)
                        {
                            _userPhotoCache[Id] = photo.Photo;
                        }

                        return photo.Photo;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public static async Task<byte[]> GetAdsPhoto(this int Id)
        {
            try
            {
                // Check memory cache first
                lock (_cacheLock)
                {
                    if (_adsPhotoCache.TryGetValue(Id, out byte[] cachedPhoto))
                    {
                        return cachedPhoto;
                    }
                }

                AdsPhoto adsPhoto = null;
                try
                {
                    adsPhoto = await App.LocalDatabase.GetAdsPhoto(Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (adsPhoto != null && adsPhoto.Id >= 0)
                {
                    var photoBytes = Convert.FromBase64String(adsPhoto.Base64Photo);

                    // Cache the result
                    lock (_cacheLock)
                    {
                        _adsPhotoCache[Id] = photoBytes;
                    }

                    return photoBytes;
                }
                else
                {
                    GetPhotoResponse photo = await Api.GetAdsPhoto(new GetPhotoRequest()
                    {
                        Id = Id,
                    });
                    if (photo != null && photo.Success)
                    {
                        _ = await App.LocalDatabase.SaveAdsPhoto(new AdsPhoto()
                        {
                            Base64Photo = Convert.ToBase64String(photo.Photo),
                            Id = Id
                        });

                        // Cache the result
                        lock (_cacheLock)
                        {
                            _adsPhotoCache[Id] = photo.Photo;
                        }

                        return photo.Photo;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        private static async Task UpdateUserPhoto(int userId, string key)
        {
            GetPhotoResponse photo = await Api.GetUserPhoto(new GetPhotoRequest()
            {
                Id = userId,
            });
            byte[] userPhoto = photo != null ? photo.Photo : (new byte[0]);
            await SecureStorage.SetAsync(key, JsonConvert.SerializeObject(new StoragePhoto()
            {
                Photo = (userPhoto != null && userPhoto.Length > 0) ? Convert.ToBase64String(userPhoto) : string.Empty,
                LastAccessTime = DateTime.Now
            }));
        }
        private static async Task UpdateAdsPhoto(int adsId, string key)
        {
            GetPhotoResponse photo = await Api.GetAdsPhoto(new GetPhotoRequest()
            {
                Id = adsId,
            });
            byte[] adsPhoto = photo != null ? photo.Photo : (new byte[0]);
            await SecureStorage.SetAsync(key, JsonConvert.SerializeObject(new StoragePhoto()
            {
                Photo = (adsPhoto != null && adsPhoto.Length > 0) ? Convert.ToBase64String(adsPhoto) : string.Empty,
                LastAccessTime = DateTime.Now
            }));
        }
    }
}
