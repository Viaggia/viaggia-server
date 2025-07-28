namespace viaggia_server.DTOs.Commodities
{
    public class CreateCommoditieDTO
    {
        public int HotelId { get; set; }

        public bool HasParking { get; set; }
        public bool IsParkingFree { get; set; }

        public bool HasBreakfast { get; set; }
        public bool IsBreakfastFree { get; set; }

        public bool HasLunch { get; set; }
        public bool IsLunchFree { get; set; }

        public bool HasDinner { get; set; }
        public bool IsDinnerFree { get; set; }

        public bool HasSpa { get; set; }
        public bool IsSpaFree { get; set; }

        public bool HasPool { get; set; }
        public bool IsPoolFree { get; set; }

        public bool HasGym { get; set; }
        public bool IsGymFree { get; set; }

        public bool HasWiFi { get; set; }
        public bool IsWiFiFree { get; set; }

        public bool HasAirConditioning { get; set; }
        public bool IsAirConditioningFree { get; set; }

        public bool HasAccessibilityFeatures { get; set; }
        public bool IsAccessibilityFeaturesFree { get; set; }

        public bool IsPetFriendly { get; set; }
        public bool IsPetFriendlyFree { get; set; }

        // Serviços extras personalizados
        public List<CommoditieServicesDTO> CommoditiesServices { get; set; } = new();
    }
}
