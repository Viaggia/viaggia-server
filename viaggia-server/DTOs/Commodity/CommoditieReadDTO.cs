using viaggia_server.DTOs.Commodity;

namespace viaggia_server.DTOs.Commoditie
{
    public class CommoditieReadDTO
    {
        public int CommoditieId { get; set; }
       
        public string HotelName { get; set; } = string.Empty;

        public bool HasParking { get; set; }
        public bool IsParkingPaid { get; set; }

        public bool HasBreakfast { get; set; }
        public bool IsBreakfastPaid { get; set; }

        public bool HasLunch { get; set; }
        public bool IsLunchPaid { get; set; }

        public bool HasDinner { get; set; }
        public bool IsDinnerPaid { get; set; }

        public bool HasSpa { get; set; }
        public bool IsSpaPaid { get; set; }

        public bool HasPool { get; set; }
        public bool IsPoolPaid { get; set; }

        public bool HasGym { get; set; }
        public bool IsGymPaid { get; set; }

        public bool HasWiFi { get; set; }
        public bool IsWiFiPaid { get; set; }

        public bool HasAirConditioning { get; set; }
        public bool IsAirConditioningPaid { get; set; }

        public bool HasAccessibilityFeatures { get; set; }
        public bool IsAccessibilityFeaturesPaid { get; set; }

        public bool IsPetFriendly { get; set; }
        public bool IsPetFriendlyPaid { get; set; }

        public bool IsActive { get; set; }

        public List<CustomCommodityDTO> CustomCommodities { get; set; } = new();
    }
}
