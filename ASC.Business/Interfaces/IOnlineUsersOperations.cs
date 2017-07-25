using System.Threading.Tasks;

namespace ASC.Business.Interfaces
{
    public interface IOnlineUsersOperations
    {
        Task CreateOnlineUserAsync(string name);
        Task DeleteOnlineUserAsync(string name);
        Task<bool> GetOnlineUserAsync(string name);
    }
}
