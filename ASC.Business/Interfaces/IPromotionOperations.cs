using ASC.Models.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASC.Business.Interfaces
{
    public interface IPromotionOperations
    {
        Task CreatePromotionAsync(Promotion promotion);
        Task<Promotion> UpdatePromotionAsync(string rowKey, Promotion promotion);
        Task<List<Promotion>> GetAllPromotionsAsync();
    }
}