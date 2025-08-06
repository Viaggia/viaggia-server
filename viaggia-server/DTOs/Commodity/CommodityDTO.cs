
using System.ComponentModel.DataAnnotations;


namespace viaggia_server.DTOs.Commodity
{
    public class CommodityDTO
    {
        [Required]
        public string? HotelName { get; set; }
        public int HotelId { get; set; }
        public bool HasParking { get; set; }
        public bool IsParkingPaid { get; set; }
        public decimal ParkingPrice { get; set; } // Added
        public bool HasBreakfast { get; set; }
        public bool IsBreakfastPaid { get; set; }
        public decimal BreakfastPrice { get; set; } // Added
        public bool HasLunch { get; set; }
        public bool IsLunchPaid { get; set; }
        public decimal LunchPrice { get; set; } // Added
        public bool HasDinner { get; set; }
        public bool IsDinnerPaid { get; set; }
        public decimal DinnerPrice { get; set; } // Added
        public bool HasSpa { get; set; }
        public bool IsSpaPaid { get; set; }
        public decimal SpaPrice { get; set; } // Added
        public bool HasPool { get; set; }
        public bool IsPoolPaid { get; set; }
        public decimal PoolPrice { get; set; } // Added
        public bool HasGym { get; set; }
        public bool IsGymPaid { get; set; }
        public decimal GymPrice { get; set; } // Added
        public bool HasWiFi { get; set; }
        public bool IsWiFiPaid { get; set; }
        public decimal WiFiPrice { get; set; } // Added
        public bool HasAirConditioning { get; set; }
        public bool IsAirConditioningPaid { get; set; }
        public decimal AirConditioningPrice { get; set; } // Added
        public bool HasAccessibilityFeatures { get; set; }
        public bool IsAccessibilityFeaturesPaid { get; set; }
        public decimal AccessibilityFeaturesPrice { get; set; } // Added
        public bool IsPetFriendly { get; set; }
        public bool IsPetFriendlyPaid { get; set; }
        public decimal PetFriendlyPrice { get; set; } // Added
        public bool IsActive { get; set; }

        public int CommodityId { get; set; }
        // Lista de serviços personalizados
        public List<CustomCommodityDTO> CustomCommodities { get; set; } = new List<CustomCommodityDTO>();
    }
}