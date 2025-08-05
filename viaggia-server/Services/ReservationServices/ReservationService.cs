using Stripe;
using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Reserves;
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

        public async Task<List<Reserve>> GetAllAsync()
        {
            try
            {
                var reservations = await _reservationRepository.GetAllAsync();
                return reservations.Select(r => new Reserve
                {
                    ReserveId = r.ReserveId,
                    UserId = r.UserId,
                    PackageId = r.PackageId,
                    RoomTypeId = r.RoomTypeId,
                    HotelId = r.HotelId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = r.TotalPrice,
                    NumberOfGuests = r.NumberOfGuests,
                    Status = r.Status,
                    IsActive = r.IsActive
                }).ToList();

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Reserve> GetByIdAsync(int id)
        {
            try
            {
                var r = await _reservationRepository.GetReservationByIdAsync(id);
                if (r == null || r.ReserveId != id) throw new Exception("Reserve Don't Found");

                return new Reserve
                {
                    UserId = r.UserId,
                    PackageId = r.PackageId,
                    RoomTypeId = r.RoomTypeId,
                    HotelId = r.HotelId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = r.TotalPrice,
                    NumberOfGuests = r.NumberOfGuests,
                    Status = r.Status,
                    IsActive = r.IsActive
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Reserve> CreateAsync(ReservesCreateDTO dto)
        {
            try
            {
                // Validação de datas
                if (dto.CheckInDate >= dto.CheckOutDate)
                    throw new ArgumentException("A data de check-out deve ser posterior à data de check-in.");

                // Validação do usuário
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null)
                    throw new Exception("Cliente não encontrado.");

                // Validação do hotel
                var hotel = await _hotelRepository.GetByIdHotel(dto.HotelId);
                if (hotel == null)
                    throw new Exception("Hotel não encontrado.");

                var reservation = new Reserve
                {
                    UserId = dto.UserId,
                    PackageId = dto.PackageId, // pode ser null
                    RoomTypeId = dto.RoomTypeId,
                    HotelId = dto.HotelId,
                    TotalPrice = dto.TotalPrice,
                    NumberOfGuests = dto.NumberOfGuests,
                    Status = dto.Status ?? "Pendente", // fallback se quiser
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _reservationRepository.AddAsync(reservation);
                await _reservationRepository.SaveChangesAsync();

                return await GetByIdAsync(reservation.ReserveId);
            }
            catch (Exception ex)
            {
                // Aqui você pode logar o erro com mais detalhes se quiser
                throw new Exception($"Erro ao criar reserva: {ex.Message}");
            }
        }


        public async Task<Reserve> UpdateAsync(int id, ReservationUpdateDTO dto)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null) throw new ArgumentException("Reserva não encontrada.");

                reservation.UserId = dto.UserId;
                reservation.PackageId = dto.PackageId;
                reservation.RoomTypeId = dto.RoomTypeId;
                reservation.HotelId = dto.HotelId;
                reservation.CheckInDate = dto.CheckInDate;
                reservation.CheckOutDate = dto.CheckOutDate;
                reservation.TotalPrice = dto.TotalPrice;
                reservation.NumberOfGuests = dto.NumberOfGuests;
                reservation.Status = dto.Status;
                reservation.IsActive = dto.IsActive;

                await _reservationRepository.UpdateAsync(reservation);
                await _reservationRepository.SaveChangesAsync();

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            try
            {
                var deleted = await _reservationRepository.SoftDeleteAsync(id);
                if (deleted)
                    await _reservationRepository.SaveChangesAsync();
                return deleted;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
