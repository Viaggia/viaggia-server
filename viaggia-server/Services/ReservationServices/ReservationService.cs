using Stripe;
using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Repositories.ReservationRepository;
using viaggia_server.Repositories.Users;
using viaggia_server.Services.Email;

namespace viaggia_server.Services.Reservations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IEmailService _emailService;

        public ReservationService(
            IReservationRepository reservationRepo, 
            IUserRepository userRepository, 
            IHotelRepository hotelRepository, 
            IEmailService emailService)
        {
            _reservationRepository = reservationRepo;
            _userRepository = userRepository;
            _hotelRepository = hotelRepository;
            _emailService = emailService;
        }

        public async Task<List<ReservationDTO>> GetAllAsync()
        {
            var reservations = await _reservationRepository.GetAllAsync();
            return reservations.Select(r => new ReservationDTO
            {
                ReservationId = r.ReservationId,
                UserId = r.UserId,
                PackageId = r.PackageId,
                HotelId = r.HotelId,
                TotalPrice = r.TotalPrice,
                IsActive = r.IsActive
            }).ToList();
        }

        public async Task<ReservationDTO?> GetByIdAsync(int id)
        {
            var r = await _reservationRepository.GetByIdAsync(id);
            if (r == null) return null;

            return new ReservationDTO
            {
                ReservationId = r.ReservationId,
                UserId = r.UserId,
                PackageId = r.PackageId,
                HotelId = r.HotelId,
                TotalPrice = r.TotalPrice,
                IsActive = r.IsActive
            };
        }

        public async Task<ReservationDTO> CreateAsync(ReservationCreateDTO dto, Hotel hotel, User user)
        {
            var userId = await _userRepository.GetByIdAsync(dto.UserId);
            if (userId == null) throw new Exception("Cliente não encontrado");
            var HotelId = _hotelRepository.GetHotelByIdWithDetailsAsync(dto.HotelId);
            if (HotelId == null) throw new Exception("Hotel não encontrado");
            var PackageId = _hotelRepository.GetPackageByIdAsync(dto.PackageId);

            try
            {

            }
            catch (Exception ex)
            {

                var reservation = new Reservation
                {
                    UserId = dto.UserId,
                    PackageId = dto.PackageId,
                    HotelId = dto.HotelId,
                    TotalPrice = totalPrice,
                    NumberOfGuests = dto.NumberOfGuests,
                    Status = dto.Status
                };

                await _reservationRepository.AddAsync(reservation);
                await _reservationRepository.SaveChangesAsync();

                return await GetByIdAsync(reservation.ReservationId) ?? throw new Exception("Erro ao criar reserva.");
            }
        }

        public async Task<ReservationDTO> UpdateAsync(int id, ReservationUpdateDTO dto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) throw new ArgumentException("Reserva não encontrada.");

            reservation.UserId = dto.UserId;
            reservation.PackageId = dto.PackageId;
            reservation.HotelId = dto.HotelId;
            reservation.TotalPrice = dto.TotalPrice;
            reservation.IsActive = dto.IsActive;

            await _reservationRepository.UpdateAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            return await GetByIdAsync(id) ?? throw new Exception("Erro ao atualizar reserva.");
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var deleted = await _reservationRepository.SoftDeleteAsync(id);
            if (deleted)
                await _reservationRepository.SaveChangesAsync();

            return deleted;
        }
    }
}
