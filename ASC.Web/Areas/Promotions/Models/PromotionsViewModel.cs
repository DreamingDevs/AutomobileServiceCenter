using System.Collections.Generic;

namespace ASC.Web.Areas.Promotions.Models
{
    public class PromotionsViewModel
    {
        public List<PromotionViewModel> Promotions { get; set; }
        public PromotionViewModel PromotionInContext { get; set; }
        public bool IsEdit { get; set; }
    }
}
