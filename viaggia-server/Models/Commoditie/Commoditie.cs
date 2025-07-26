using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Hotels;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Commodities
{
    public class Commoditie : ISoftDeletable
    {
        [Key]
        public int CommoditieId { get; set; }

        // Serviços padrões
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

        public bool IsActive { get; set; } = true;

        [Required]
        public int HotelId { get; set; }

        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; } = null!;


        // Lista de serviços personalizados adicionais
        public ICollection<CommoditieServices> CommoditiesServices { get; set; } = new List<CommoditieServices>();
        
    }
}