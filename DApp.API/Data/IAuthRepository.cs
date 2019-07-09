using System.Threading.Tasks;
using DApp.API.Models;

namespace DApp.API.Data
{
    public interface IAuthRepository
    {
        Task<User> RegisterUser(User user, string password);
        Task<User> Login(string username, string password);
        Task<bool> UserExists(string username);
    }
}