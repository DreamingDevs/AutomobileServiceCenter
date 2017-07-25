using Microsoft.AspNetCore.Mvc;
using ASC.Web.Controllers;
using ASC.Business.Interfaces;
using ASC.Web.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using System.Collections.Generic;
using ASC.Utilities;
using ASC.Models.Models;
using System;
using ASC.Models.BaseTypes;
using ASC.Web.Areas.Promotions.Models;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using ASC.Web.ServiceHub;

namespace ASC.Web.Areas.Promotions.Controllers
{
    [Area("Promotions")]
    public class PromotionsController : BaseController
    {
        private readonly IPromotionOperations _promotionOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;
        private readonly IConnectionManager _signalRConnectionManager;
        public PromotionsController(IPromotionOperations promotionOperations, 
            IMapper mapper, 
            IMasterDataCacheOperations masterData,
            IConnectionManager signalRConnectionManager)
        {
            _promotionOperations = promotionOperations;
            _mapper = mapper;
            _masterData = masterData;
            _signalRConnectionManager = signalRConnectionManager;
        }

        [HttpGet]
        public async Task<IActionResult> Promotion()
        {
            var promotions = await _promotionOperations.GetAllPromotionsAsync();
            var promotionsViewModel = _mapper.Map<List<Promotion>, List<PromotionViewModel>>(promotions);

            // Get All Master Keys and hold them in ViewBag for Select tag
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.PromotionTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.PromotionType.ToString()).ToList();

            // Hold all Promotions in session
            HttpContext.Session.SetSession("Promotions", promotionsViewModel);

            return View(new PromotionsViewModel
            {
                Promotions = promotionsViewModel == null ? null : promotionsViewModel.ToList(),
                IsEdit = false,
                PromotionInContext = new PromotionViewModel()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Promotion(PromotionsViewModel promotions)
        {
            promotions.Promotions = HttpContext.Session.GetSession<List<PromotionViewModel>>("Promotions");
            if (!ModelState.IsValid)
            {
                return View(promotions);
            }

            var promotion = _mapper.Map<PromotionViewModel, Promotion>(promotions.PromotionInContext);
            if (promotions.IsEdit)
            {
                // Update Promotion
                await _promotionOperations.UpdatePromotionAsync(promotions.PromotionInContext.RowKey, promotion);
            }
            else
            {
                // Insert Promotion
                promotion.RowKey = Guid.NewGuid().ToString();
                await _promotionOperations.CreatePromotionAsync(promotion);

                if (!promotion.IsDeleted)
                {
                    // Broadcast the message to all clients asscoaited with new promotion
                    _signalRConnectionManager.GetHubContext<ServiceMessagesHub>()
                        .Clients
                        .All
                        .publishPromotion(promotion);
                }
            }

            return RedirectToAction("Promotion");
        }

        [HttpGet]
        public async Task<IActionResult> Promotions()
        {
            var promotions = await _promotionOperations.GetAllPromotionsAsync();
            var filteredPromotions = new List<Promotion>();
            if(promotions != null)
            {
                filteredPromotions = promotions.Where(p => !p.IsDeleted).OrderByDescending(p => p.Timestamp).ToList();
            }

            return View(filteredPromotions);
        }
    }
}
