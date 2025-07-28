using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Commodities;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IRepository<Hotel> _hotelRepository;
        private readonly IRepository<Commodity> _commoditieRepository;
        private readonly IHotelRepository _hotelRepositorySpecific; // só para Address e RoomType

        public HotelsController(
            IRepository<Hotel> hotelRepository,
            IRepository<Commodity> commoditieRepository,
            IHotelRepository hotelRepositorySpecific)
        {
            _hotelRepository = hotelRepository;
            _commoditieRepository = commoditieRepository;
            _hotelRepositorySpecific = hotelRepositorySpecific;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null)
                return NotFound();

            var address = await _hotelRepositorySpecific.GetAddressByHotelIdAsync(id);

            var commoditieList = await _commoditieRepository.GetAllAsync();
            var commoditie = commoditieList.FirstOrDefault(c => c.HotelId == id);

            var dto = new HotelDTO
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Description = hotel.Description,
                StarRating = hotel.StarRating,
                CheckInTime = hotel.CheckInTime,
                CheckOutTime = hotel.CheckOutTime,
                ContactPhone = hotel.ContactPhone,
                ContactEmail = hotel.ContactEmail,
                Street = address?.Street ?? "",
                City = address?.City ?? "",
                State = address?.State ?? "",
                ZipCode = address?.ZipCode ?? "",
                Commoditie = commoditie != null ? new CommoditieDTO
                {
                    HasBreakfast = commoditie.HasBreakfast,
                    HasParking = commoditie.HasParking,
                    HasSpa = commoditie.HasSpa,
                    HasPool = commoditie.HasPool,
                    IsActive = commoditie.IsActive
                } : new CommoditieDTO(),
                IsActive = hotel.IsActive
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Criar endereço via HotelRepository específico
                var address = new viaggia_server.Models.Addresses.Address
                {
                    Street = dto.Street,
                    City = dto.City,
                    State = dto.State,
                    ZipCode = dto.ZipCode,
                    IsActive = true
                };

                var createdAddress = await _hotelRepositorySpecific.AddAddressAsync(address);

                // Criar hotel usando o endereço criado
                var hotel = new Hotel
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    StarRating = dto.StarRating,
                    CheckInTime = dto.CheckInTime,
                    CheckOutTime = dto.CheckOutTime,
                    ContactPhone = dto.ContactPhone,
                    ContactEmail = dto.ContactEmail,
                    AddressId = createdAddress.AddressId,
                    IsActive = true
                };

                var createdHotel = await _hotelRepository.AddAsync(hotel);

                // Criar commoditie via repositório genérico
                if (dto.Commoditie != null)
                {
                    var commoditie = new Commodity
                    {
                        HotelId = createdHotel.HotelId,
                        HasBreakfast = dto.Commoditie.HasBreakfast,
                        HasParking = dto.Commoditie.HasParking,
                        HasSpa = dto.Commoditie.HasSpa,
                        HasPool = dto.Commoditie.HasPool,
                        IsActive = true
                    };
                    await _commoditieRepository.AddAsync(commoditie);
                }

                return CreatedAtAction(nameof(GetById), new { id = createdHotel.HotelId }, createdHotel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating hotel: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] CreateHotelDTO dto)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null)
                return NotFound();

            try
            {
                hotel.Name = dto.Name;
                hotel.Description = dto.Description;
                hotel.StarRating = dto.StarRating;
                hotel.CheckInTime = dto.CheckInTime;
                hotel.CheckOutTime = dto.CheckOutTime;
                hotel.ContactPhone = dto.ContactPhone;
                hotel.ContactEmail = dto.ContactEmail;

                await _hotelRepository.UpdateAsync(hotel);

                // Atualizar endereço via HotelRepository específico
                var address = await _hotelRepositorySpecific.GetAddressByHotelIdAsync(id);
                if (address != null)
                {
                    address.Street = dto.Street;
                    address.City = dto.City;
                    address.State = dto.State;
                    address.ZipCode = dto.ZipCode;

                    // Atualiza endereço
                    await _hotelRepositorySpecific.AddAddressAsync(address); // Se tiver UpdateAddressAsync, usar ele, senão terá que implementar
                }

                // Atualizar commoditie via repositório genérico
                var commoditieList = await _commoditieRepository.GetAllAsync();
                var commoditie = commoditieList.FirstOrDefault(c => c.HotelId == id);
                if (commoditie != null && dto.Commoditie != null)
                {
                    commoditie.HasBreakfast = dto.Commoditie.HasBreakfast;
                    commoditie.HasParking = dto.Commoditie.HasParking;
                    commoditie.HasSpa = dto.Commoditie.HasSpa;
                    commoditie.HasPool = dto.Commoditie.HasPool;

                    await _commoditieRepository.UpdateAsync(commoditie);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating hotel: {ex.Message}" });
            }
        }
    }
}