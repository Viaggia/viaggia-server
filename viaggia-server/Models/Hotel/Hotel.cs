using System.ComponentModel.DataAnnotations;

namespace viaggia_server.Models.Hotel
{
    public class Hotel
    {
        [Key]
        public int HotelId { get; set; }

        [Required]
        public string? SurnameHotel{ get; set; }
        [Required]
        public string? SocialRaison { get; set; }
        [Required]
        public string? Cnpj {  get; set; }
        [Required]
        public string? Address{ get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? Country { get; set; }
        [Required]
        public string? PostalCode { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        public string? Email { get; set; }
    }
}
