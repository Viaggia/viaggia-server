using Stripe;
using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Reserves;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Repositories.ReserveRepository;
using viaggia_server.Repositories.Users;
using viaggia_server.Services.Email;

namespace viaggia_server.Services.Reserves
{
    public class ReservesService : IReservesService
    {
        private readonly IReserveRepository _reserveRepository;
        private readonly IRepository<Reserve> _repository;
        private readonly IUserRepository _userRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IEmailService _emailService;

        public ReservesService(
            IReserveRepository reservationRepo,
            IRepository<Reserve> repository,
            IUserRepository userRepository,
            IHotelRepository hotelRepository,
            IEmailService emailService)
        {
            _reserveRepository = reservationRepo;
            _repository = repository;
            _userRepository = userRepository;
            _hotelRepository = hotelRepository;
            _emailService = emailService;
        }

        public async Task<List<ReserveDTO>> GetAllAsync()
        {
            try
            {
                var reservations = await _repository.GetAllAsync();
                return reservations.Select(r => new ReserveDTO
                {
                    ReserveId = r.ReserveId,
                    UserId = r.UserId,
                    PackageId = r.PackageId,
                    RoomTypeId = r.RoomTypeId,
                    HotelId = r.HotelId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = r.TotalPrice,
                    NumberOfPeople = r.NumberOfGuests,
                    Status = r.Status,
                    IsActive = r.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReserveDTO?> GetByIdAsync(int id)
        {
            try
            {
                var r = await _reserveRepository.GetByIdAsync(id);
                if (r == null) return null;

                return new ReserveDTO
                {
                    UserId = r.UserId,
                    PackageId = r.PackageId,
                    RoomTypeId = r.RoomTypeId,
                    HotelId = r.HotelId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = r.TotalPrice,
                    NumberOfPeople = r.NumberOfGuests,
                    Status = r.Status,
                    IsActive = r.IsActive
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ReserveDTO>> GetByUserIdAsync(int userId)
        {
            try
            {
                var reservations = await _reserveRepository.GetByUserIdAsync(userId);
                return reservations.Select(r => new ReserveDTO
                {
                    ReserveId = r.ReserveId,
                    UserId = r.UserId,
                    PackageId = r.PackageId,
                    RoomTypeId = r.RoomTypeId,
                    HotelId = r.HotelId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = r.TotalPrice,
                    NumberOfPeople = r.NumberOfGuests,
                    Status = r.Status,
                    IsActive = r.IsActive
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




        public async Task<ReserveDTO> CreateAsync(ReserveCreateDTO dto)
        {
            try
            {
                var userId = _userRepository.GetByIdAsync(dto.UserId);
                if (userId == null) throw new Exception("Cliente não encontrado");

                var CheckIn = dto.CheckInDate;
                var CheckOut = dto.CheckOutDate;

                int tempo = (CheckOut - CheckIn).Days;
                var totalPrice = dto.TotalPrice * tempo;


                var reservation = new Reserve
                {
                    UserId = dto.UserId,
                    PackageId = dto.PackageId,
                    RoomTypeId = dto.RoomTypeId,
                    HotelId = dto.HotelId,
                    TotalPrice = totalPrice,
                    NumberOfGuests = dto.NumberOfGuests,
                    Status = dto.Status
                };

                await _repository.AddAsync(reservation);
                await _repository.SaveChangesAsync();

                return await GetByIdAsync(reservation.ReserveId) ?? throw new Exception("Erro ao criar reserva.");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<ReserveDTO> UpdateAsync(int id, ReserveUpdateDTO dto)
        {
            var reservation = await _reserveRepository.GetByIdAsync(id);
            if (reservation == null) throw new ArgumentException("Reserva não encontrada.");

            reservation.UserId = dto.UserId;
            reservation.PackageId = dto.PackageId;
            reservation.RoomTypeId = dto.RoomTypeId;
            reservation.HotelId = dto.HotelId;
            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;
            reservation.TotalPrice = dto.TotalPrice;
            reservation.NumberOfGuests = dto.NumberOfPeople;
            reservation.Status = dto.Status;
            reservation.IsActive = dto.IsActive;

            await _repository.UpdateAsync(reservation);
            await _repository.SaveChangesAsync();

            return await GetByIdAsync(id) ?? throw new Exception("Erro ao atualizar reserva.");
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            try
            {
                var deleted = await _repository.SoftDeleteAsync(id);
                if (deleted)
                    await _repository.SaveChangesAsync();

                return deleted;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
