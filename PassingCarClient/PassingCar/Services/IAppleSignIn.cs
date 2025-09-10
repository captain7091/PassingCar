using PassingCar.Models.Apple;
using System.Threading.Tasks;

namespace PassingCar.Services
{
    public interface IAppleSignIn
    {
        Task<AppleAccount> SignInWithAppleAsync();
    }
}
