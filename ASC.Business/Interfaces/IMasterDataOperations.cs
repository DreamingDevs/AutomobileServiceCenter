using ASC.Models.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASC.Business.Interfaces
{
    public interface IMasterDataOperations
    {
        Task<List<MasterDataKey>> GetAllMasterKeysAsync();
        Task<List<MasterDataKey>> GetMaserKeyByNameAsync(string name);
        Task<bool> InsertMasterKeyAsync(MasterDataKey key);
        Task<bool> UpdateMasterKeyAsync(string orginalPartitionKey, MasterDataKey key);

        Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key);
        Task<List<MasterDataValue>> GetAllMasterValuesAsync();
        Task<MasterDataValue> GetMasterValueByNameAsync(string key, string name);
        Task<bool> InsertMasterValueAsync(MasterDataValue value);
        Task<bool> UpdateMasterValueAsync(string originalPartitionKey, string originalRowKey, MasterDataValue value);

        Task<bool> UploadBulkMasterData(List<MasterDataValue> value);
    }
}
