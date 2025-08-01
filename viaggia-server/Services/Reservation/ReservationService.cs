using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Reservations;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Reservations;

namespace viaggia_server.Services.Reservations
{
    public class ReservationService : IReservationService
    {
        private readonly IRepository<Reservation> _genericRepository;

        public ReservationService(IRepository<Reservation> genericRepo, IReservationRepository reservationRepo)
        {
            _genericRepository = genericRepo;
        }

        public async Task<List<ReservationDTO>> GetAllAsync()
        {
            var reservations = await _genericRepository.GetAllAsync();
            return reservations.Select(r => new ReservationDTO
            {
                ReservationId = r.ReservationId,
                UserId = r.UserId,
                PackageId = r.PackageId,
                RoomTypeId = r.RoomTypeId,
                HotelId = r.HotelId,
                TotalPrice = r.TotalPrice,
                NumberOfGuests = r.NumberOfGuests,
                IsActive = r.IsActive
            }).ToList();
        }

        public async Task<ReservationDTO?> GetByIdAsync(int id)
        {
            var r = await _genericRepository.GetByIdAsync(id);
            if (r == null) return null;

            return new ReservationDTO
            {
                ReservationId = r.ReservationId,
                UserId = r.UserId,
                PackageId = r.PackageId,
                RoomTypeId = r.RoomTypeId,
                HotelId = r.HotelId,
                TotalPrice = r.TotalPrice,
                NumberOfGuests = r.NumberOfGuests,
                IsActive = r.IsActive
            };
        }

        public async Task<ReservationDTO> CreateAsync(ReservationCreateDTO dto)
        {
            var reservation = new Reservation
            {
                UserId = dto.UserId,
                PackageId = dto.PackageId,
                RoomTypeId = dto.RoomTypeId,
                HotelId = dto.HotelId,
                TotalPrice = dto.TotalPrice,
                NumberOfGuests = dto.NumberOfGuests,
                IsActive = dto.IsActive
            };

            await _genericRepository.AddAsync(reservation);
            await _genericRepository.SaveChangesAsync();

            return await GetByIdAsync(reservation.ReservationId) ?? throw new Exception("Erro ao criar reserva.");
        }

        public async Task<ReservationDTO> UpdateAsync(int id, ReservationUpdateDTO dto)
        {
            var reservation = await _genericRepository.GetByIdAsync(id);
            if (reservation == null) throw new ArgumentException("Reserva não encontrada.");

            reservation.UserId = dto.UserId;
            reservation.PackageId = dto.PackageId;
            reservation.RoomTypeId = dto.RoomTypeId;
            reservation.HotelId = dto.HotelId;
            reservation.TotalPrice = dto.TotalPrice;
            reservation.NumberOfGuests = dto.NumberOfGuests;
            reservation.IsActive = dto.IsActive;

            await _genericRepository.UpdateAsync(reservation);
            await _genericRepository.SaveChangesAsync();

            return await GetByIdAsync(id) ?? throw new Exception("Erro ao atualizar reserva.");
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var deleted = await _genericRepository.SoftDeleteAsync(id);
            if (deleted)
                await _genericRepository.SaveChangesAsync();

            return deleted;
        }
    }
}
