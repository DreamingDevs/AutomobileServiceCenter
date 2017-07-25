using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Models.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class OnlineUsersOperations : IOnlineUsersOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public OnlineUsersOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateOnlineUserAsync(string name)
        {
            using (_unitOfWork)
            {
                var user = await _unitOfWork.Repository<OnlineUser>().FindAllByPartitionKeyAsync(name);
                if (user.Any())
                {
                    var updateUser = user.FirstOrDefault();
                    updateUser.IsDeleted = false;
                    await _unitOfWork.Repository<OnlineUser>().UpdateAsync(updateUser);
                }
                else
                {
                    await _unitOfWork.Repository<OnlineUser>().AddAsync(new OnlineUser(name) { IsDeleted = false });
                }
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task DeleteOnlineUserAsync(string name)
        {
            using (_unitOfWork)
            {
                var user = await _unitOfWork.Repository<OnlineUser>().FindAllByPartitionKeyAsync(name);
                if (user.Any())
                {
                    await _unitOfWork.Repository<OnlineUser>().DeleteAsync(user.ToList().FirstOrDefault());
                }
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task<bool> GetOnlineUserAsync(string name)
        {
             var user = await _unitOfWork.Repository<OnlineUser>().FindAllByPartitionKeyAsync(name);
            return user.Any() && user.FirstOrDefault().IsDeleted != true;
        }
    }
}
