using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Models.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ASC.Business
{
    public class PromotionOperations : IPromotionOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public PromotionOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreatePromotionAsync(Promotion promotion)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<Promotion>().AddAsync(promotion);
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task<List<Promotion>> GetAllPromotionsAsync()
        {
            var promotions = await _unitOfWork.Repository<Promotion>().FindAllAsync();
            return promotions.ToList();
        }

        public async Task<Promotion> UpdatePromotionAsync(string rowKey, Promotion promotion)
        {
            var originalPromotion = await _unitOfWork.Repository<Promotion>().FindAsync(promotion.PartitionKey, rowKey);
            if(originalPromotion != null)
            {
                originalPromotion.Header = promotion.Header;
                originalPromotion.Content = promotion.Content;
                originalPromotion.IsDeleted = promotion.IsDeleted;
            }
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<Promotion>().UpdateAsync(originalPromotion);
                _unitOfWork.CommitTransaction();
            }
            return originalPromotion;
        }
    }
}
