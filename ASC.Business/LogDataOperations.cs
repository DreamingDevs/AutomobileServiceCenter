using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Models.Models;
using System;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class LogDataOperations : ILogDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public LogDataOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateExceptionLogAsync(string id, string message, string stacktrace)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ExceptionLog>().AddAsync(new ExceptionLog()
                {
                    RowKey = id,
                    PartitionKey = "Exception",
                    Message = message,
                    Stacktrace = stacktrace
                });

                _unitOfWork.CommitTransaction();
            }
        }

        public async Task CreateLogAsync(string category, string message)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<Log>().AddAsync(new Log()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = category,
                    Message = message
                });

                _unitOfWork.CommitTransaction();
            }
        }

        public async Task CreateUserActivityAsync(string email, string action)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<UserActivity>().AddAsync(new UserActivity()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = email,
                    Action = action
                });

                _unitOfWork.CommitTransaction();
            }
        }
    }
}
