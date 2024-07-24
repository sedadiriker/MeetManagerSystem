using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string email, string password);
        Task<bool> UserExistsAsync(string email);
        Task CreateUserAsync(User user);
    }
}
