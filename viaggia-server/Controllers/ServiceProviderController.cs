using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commodity;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reserves;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Repositories.ReserveRepository;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SERVICE_PROVIDER")]
    public class ServiceProviderController : ControllerBase
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IReserveRepository _reserveRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Commodity> _commodityRepository;
        private readonly IRepository<CustomCommodity> _customCommodityRepository;
        private readonly ILogger<ServiceProviderController> _logger;

        public ServiceProviderController(
            IHotelRepository hotelRepository,
            IReserveRepository reserveRepository,
            IRepository<Package> packageRepository,
            IRepository<Commodity> commodityRepository,
            IRepository<CustomCommodity> customCommodityRepository,
            ILogger<ServiceProviderController> logger)
        {
            _hotelRepository = hotelRepository;
            _reserveRepository = reserveRepository;
            _packageRepository = packageRepository;
            _commodityRepository = commodityRepository;
            _customCommodityRepository = customCommodityRepository;
            _logger = logger;
        }

        [HttpGet("hotel")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> GetHotel()
        {
            var hotelIdClaim = User.FindFirst("HotelId")?.Value;
            if (string.IsNullOrEmpty(hotelIdClaim) || !int.TryParse(hotelIdClaim, out var hotelId))
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            if (hotel == null)
                return NotFound(new ApiResponse<object>(false, "Hotel não encontrado."));

            var hotelDto = new HotelDTO
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Cnpj = hotel.Cnpj,
                Street = hotel.Street,
                City = hotel.City,
                State = hotel.State,
                ZipCode = hotel.ZipCode,
                Description = hotel.Description,
                StarRating = hotel.StarRating,
                CheckInTime = hotel.CheckInTime,
                CheckOutTime = hotel.CheckOutTime,
                ContactPhone = hotel.ContactPhone,
                ContactEmail = hotel.ContactEmail,
                IsActive = hotel.IsActive,
                AverageRating = hotel.AverageRating,
                RoomTypes = hotel.RoomTypes.Select(rt => new HotelRoomTypeDTO
                {
                    RoomTypeId = rt.RoomTypeId,
                    Name = rt.Name,
                    Description = rt.Description,
                    Price = rt.Price,
                    Capacity = rt.Capacity,
                    BedType = rt.BedType,
                    TotalRooms = rt.TotalRooms,
                    AvailableRooms = rt.AvailableRooms,
                    IsActive = rt.IsActive
                }).ToList(),
                CommodityRead = hotel.Commodities.Select(c => new CommodityReadDTO
                {
                    CommodityId = c.CommodityId,
                    HotelId = c.HotelId,
                    HotelName = c.Hotel?.Name ?? string.Empty,
                    HasParking = c.HasParking,
                    IsParkingPaid = c.IsParkingPaid,
                    ParkingPrice = c.ParkingPrice,
                    IsActive = c.IsActive,
                    CustomCommodities = c.CustomCommodities.Select(cs => new CustomCommodityDTO
                    {
                        CustomCommodityId = cs.CustomCommodityId,
                        Name = cs.Name,
                        Description = cs.Description,
                        IsPaid = cs.IsPaid,
                        Price = cs.Price,
                        IsActive = cs.IsActive,
                        HotelId = cs.HotelId,
                        CommodityId = cs.CommodityId
                    }).ToList()
                }).ToList(),
                CustomCommodities = hotel.CustomCommodities.Select(cs => new CustomCommodityDTO
                {
                    CustomCommodityId = cs.CustomCommodityId,
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = cs.Price,
                    IsActive = cs.IsActive,
                    HotelId = cs.HotelId,
                    CommodityId = cs.CommodityId
                }).ToList(),
                Packages = hotel.Packages.Select(p => new PackageDTO
                {
                    PackageId = p.PackageId,
                    Name = p.Name,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    IsActive = p.IsActive
                }).ToList(),
                Reserves = hotel.Reserves.Select(r => new ReserveDTO
                {
                    ReserveId = r.ReserveId,
                    UserId = r.UserId,
                    HotelId = r.HotelId,
                    RoomTypeId = r.RoomTypeId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    NumberOfPeople = r.NumberOfPeople,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    IsActive = r.IsActive
                }).ToList()
            };

            return Ok(new ApiResponse<HotelDTO>(true, "Hotel recuperado com sucesso.", hotelDto));
        }

        [HttpPut("hotel")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> UpdateHotel([FromBody] UpdateHotelDto dto)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(false, "Dados inválidos.", null, ModelState));

            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            if (hotel == null)
                return NotFound(new ApiResponse<object>(false, "Hotel não encontrado."));

            hotel.Name = dto.Name;
            hotel.Cnpj = dto.Cnpj;
            hotel.Street = dto.Street;
            hotel.City = dto.City;
            hotel.State = dto.State;
            hotel.ZipCode = dto.ZipCode;
            hotel.Description = dto.Description;
            hotel.StarRating = dto.StarRating;
            hotel.CheckInTime = dto.CheckInTime;
            hotel.CheckOutTime = dto.CheckOutTime;
            hotel.ContactPhone = dto.ContactPhone;
            hotel.ContactEmail = dto.ContactEmail;
            hotel.IsActive = dto.IsActive;

            await _hotelRepository.UpdateAsync(hotel);
            return Ok(new ApiResponse<object>(true, "Hotel atualizado com sucesso."));
        }

        [HttpGet("reservations")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> GetReservations()
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var reservations = await _reserveRepository.GetByHotelIdAsync(hotelId);
            var reservationDtos = reservations.Select(r => new ReserveDTO
            {
                ReserveId = r.ReserveId,
                UserId = r.UserId,
                HotelId = r.HotelId,
                RoomTypeId = r.RoomTypeId,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfRooms = r.NumberOfRooms,
                NumberOfPeople = r.NumberOfPeople,
                TotalPrice = r.TotalPrice,
                Status = r.Status,
                IsActive = r.IsActive
            }).ToList();

            return Ok(new ApiResponse<List<ReserveDTO>>(true, "Reservas recuperadas com sucesso.", reservationDtos));
        }

        [HttpPut("reservations/{id}")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReserveUpdateDTO dto)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var reservation = await _reserveRepository.GetByIdAsync(id);
            if (reservation == null || reservation.HotelId != hotelId)
                return NotFound(new ApiResponse<object>(false, "Reserva não encontrada ou não pertence ao hotel."));

            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;
            reservation.NumberOfRooms = dto.NumberOfRooms;
            reservation.NumberOfPeople = dto.NumberOfPeople;
            reservation.TotalPrice = dto.TotalPrice;
            reservation.Status = dto.Status;

            await _reserveRepository.UpdateAsync(reservation);
            return Ok(new ApiResponse<object>(true, "Reserva atualizada com sucesso."));
        }

        [HttpDelete("reservations/{id}")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var reservation = await _reserveRepository.GetByIdAsync(id);
            if (reservation == null || reservation.HotelId != hotelId)
                return NotFound(new ApiResponse<object>(false, "Reserva não encontrada ou não pertence ao hotel."));

            reservation.IsActive = false;
            await _reserveRepository.UpdateAsync(reservation);
            return Ok(new ApiResponse<object>(true, "Reserva cancelada com sucesso."));
        }

        [HttpGet("packages")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> GetPackages()
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var packages = await _packageRepository.GetAllAsync();
            var packageDtos = packages.Where(p => p.HotelId == hotelId).Select(p => new PackageDTO
            {
                PackageId = p.PackageId,
                Name = p.Name,
                Description = p.Description,
                BasePrice = p.BasePrice,
                IsActive = p.IsActive
            }).ToList();

            return Ok(new ApiResponse<List<PackageDTO>>(true, "Pacotes recuperados com sucesso.", packageDtos));
        }

        [HttpPost("commodities")]
        [Consumes("multipart/form-data")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> CreateCommodity([FromForm] CreateCommodityDTO dto)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            if (hotel == null || dto.HotelName != hotel.Name)
                return BadRequest(new ApiResponse<object>(false, "Nome do hotel não corresponde ao hotel associado."));

            var commodity = new Commodity
            {
                HotelId = hotelId,
                HasParking = dto.HasParking,
                IsParkingPaid = dto.IsParkingPaid,
                ParkingPrice = dto.ParkingPrice,
                // ... map other commodity fields ...
                IsActive = dto.IsActive,
                CustomCommodities = dto.CustomCommodities?.Select(cs => new CustomCommodity
                {
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = (decimal)cs.Price,
                    IsActive = cs.IsActive,
                    HotelId = hotelId
                }).ToList() ?? new List<CustomCommodity>()
            };

            var result = await _commodityRepository.AddAsync(commodity);
            foreach (var customCommodity in commodity.CustomCommodities)
            {
                customCommodity.CommodityId = result.CommodityId;
                await _customCommodityRepository.AddAsync(customCommodity);
            }

            var reloadedCommodity = await _commodityRepository.GetByIdAsync(result.CommodityId);
            var response = new CommodityReadDTO
            {
                CommodityId = reloadedCommodity.CommodityId,
                HotelId = reloadedCommodity.HotelId,
                HotelName = hotel.Name,
                HasParking = reloadedCommodity.HasParking,
                IsParkingPaid = reloadedCommodity.IsParkingPaid,
                ParkingPrice = reloadedCommodity.ParkingPrice,
                // ... map other commodity fields ...
                IsActive = reloadedCommodity.IsActive,
                CustomCommodities = reloadedCommodity.CustomCommodities.Select(cs => new CustomCommodityDTO
                {
                    CustomCommodityId = cs.CustomCommodityId,
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = cs.Price,
                    IsActive = cs.IsActive,
                    HotelName = hotel.Name,
                    HotelId = cs.HotelId,
                    CommodityId = cs.CommodityId
                }).ToList()
            };

            return Ok(new ApiResponse<CommodityReadDTO>(true, "Commodity criada com sucesso.", response));
        }

        [HttpPut("commodities/{id}")]
        [Consumes("multipart/form-data")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> UpdateCommodity(int id, [FromForm] UpdateCommodityDTO dto)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var commodity = await _commodityRepository.GetByIdAsync(id);
            if (commodity == null || commodity.HotelId != hotelId)
                return NotFound(new ApiResponse<object>(false, "Commodity não encontrada ou não pertence ao hotel."));

            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            if (hotel == null || dto.HotelName != hotel.Name)
                return BadRequest(new ApiResponse<object>(false, "Nome do hotel não corresponde ao hotel associado."));

            commodity.HasParking = dto.HasParking;
            commodity.IsParkingPaid = dto.IsParkingPaid;
            commodity.ParkingPrice = dto.ParkingPrice;
            // ... map other commodity fields ...
            commodity.IsActive = dto.IsActive;

            await _commodityRepository.UpdateAsync(commodity);
            return Ok(new ApiResponse<object>(true, "Commodity atualizada com sucesso."));
        }

        [HttpDelete("commodities/{id}")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> DeleteCommodity(int id)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var commodity = await _commodityRepository.GetByIdAsync(id);
            if (commodity == null || commodity.HotelId != hotelId)
                return NotFound(new ApiResponse<object>(false, "Commodity não encontrada ou não pertence ao hotel."));

            await _commodityRepository.SoftDeleteAsync(id);
            return Ok(new ApiResponse<object>(true, "Commodity desativada com sucesso."));
        }
    }
}