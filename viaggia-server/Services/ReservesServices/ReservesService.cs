using Stripe;
using viaggia_server.DTOs.Reserve;
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
                    HotelId = r.HotelId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalPrice = r.TotalPrice,
                    NumberOfPeople = r.NumberOfGuests,
                    Status = r.Status,
                    IsActive = r.IsActive,
                    ReserveRooms = r.ReserveRooms.Select(rr => new ReserveRoomDTO
                    {
                        RoomTypeId = rr.RoomTypeId,
                        RoomTypeName = rr.RoomType.Name.ToString(), // ou outro campo que prefira
                        Quantity = rr.Quantity
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<ReserveDTO?> GetByIdAsync(int id)
        {
            var reservation = await _reserveRepository.GetByIdAsync(id);
            if (reservation == null) return null;

            return new ReserveDTO
            {
                ReserveId = reservation.ReserveId,
                UserId = reservation.UserId,
                PackageId = reservation.PackageId,
                HotelId = reservation.HotelId,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                TotalPrice = reservation.TotalPrice,
                NumberOfPeople = reservation.NumberOfGuests,
                Status = reservation.Status,
                IsActive = reservation.IsActive,
                ReserveRooms = reservation.ReserveRooms.Select(rr => new ReserveRoomDTO
                {
                    RoomTypeId = rr.RoomTypeId,
                    RoomTypeName = rr.RoomType.Name.ToString()  ,  // Caso RoomType seja carregado
                    Quantity = rr.Quantity
                }).ToList()
            };
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
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null) throw new Exception("Cliente não encontrado");

                if (dto.ReserveRooms == null || !dto.ReserveRooms.Any())
                    throw new Exception("Nenhum quarto foi selecionado para a reserva.");

                var stayDays = (dto.CheckOutDate - dto.CheckInDate).Days;
                if (stayDays <= 0)
                    throw new Exception("A data de check-out deve ser posterior à data de check-in.");

                decimal totalPrice = 0;
                var reserveRooms = new List<ReserveRoom>();

                foreach (var rr in dto.ReserveRooms)
                {
                    var roomType = await _hotelRepository.GetRoomTypeByIdAsync(rr.RoomTypeId);
                    if (roomType == null)
                        throw new Exception($"Tipo de quarto com ID {rr.RoomTypeId} não encontrado.");

                    if (roomType. AvailableRooms < rr.Quantity)
                        throw new Exception($"Quartos insuficientes disponíveis para o tipo {roomType.Name}.");

                    // Atualiza disponibilidade
                    roomType.AvailableRooms -= rr.Quantity;
                    await _hotelRepository.UpdateRoomTypeAsync(roomType);

                    // Calcula preço
                    totalPrice += roomType.Price * rr.Quantity * stayDays;

                    reserveRooms.Add(new ReserveRoom
                    {
                        RoomTypeId = rr.RoomTypeId,
                        Quantity = rr.Quantity
                    });
                }

                var reservation = new Reserve
                {
                    UserId = dto.UserId,
                    HotelId = dto.HotelId,
                    PackageId = dto.PackageId,
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
                    NumberOfGuests = dto.NumberOfGuests,
                    Status = dto.Status,
                    TotalPrice = totalPrice,
                    IsActive = true,
                    ReserveRooms = reserveRooms
                };

                await _repository.AddAsync(reservation);
                await _repository.SaveChangesAsync();

                await _emailService.SendApprovedReserve(reservation);

                return await GetByIdAsync(reservation.ReserveId)
                    ?? throw new Exception("Erro ao criar reserva.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar reserva: {ex.Message}");
            }
        }


        public async Task<ReserveDTO> UpdateAsync(int id, ReserveUpdateDTO dto)
        {
            var reservation = await _reserveRepository.GetByIdAsync(id);
            if (reservation == null) throw new ArgumentException("Reserva não encontrada.");

            // Atualiza os dados básicos da reserva
            reservation.UserId = dto.UserId;
            reservation.PackageId = dto.PackageId;
            reservation.HotelId = dto.HotelId;
            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;
            reservation.NumberOfGuests = dto.NumberOfGuests;
            reservation.Status = dto.Status;
            reservation.IsActive = dto.IsActive;

            // Calcula os dias de estadia
            var stayDays = (dto.CheckOutDate - dto.CheckInDate).Days;
            if (stayDays <= 0)
                throw new Exception("A data de check-out deve ser posterior à data de check-in.");

            // Atualizar ReserveRooms e ajustar disponibilidade
            if (dto.ReserveRooms == null || !dto.ReserveRooms.Any())
                throw new Exception("Nenhum quarto foi selecionado para a reserva.");

            // Restaurar disponibilidade dos quartos atuais (liberar os quartos reservados antes)
            foreach (var existingRoom in reservation.ReserveRooms)
            {
                var currentRoomType = await _hotelRepository.GetRoomTypeByIdAsync(existingRoom.RoomTypeId);
                if (currentRoomType != null)
                {
                    currentRoomType.AvailableRooms += existingRoom.Quantity;
                    await _hotelRepository.UpdateRoomTypeAsync(currentRoomType);
                }
            }

            // Remover os ReserveRooms antigos da reserva
            reservation.ReserveRooms.Clear();

            decimal totalPrice = 0;
            var newReserveRooms = new List<ReserveRoom>();

            // Para cada quarto no DTO, verificar disponibilidade e atualizar
            foreach (var rr in dto.ReserveRooms)
            {
                var roomType = await _hotelRepository.GetRoomTypeByIdAsync(rr.RoomTypeId);
                if (roomType == null)
                    throw new Exception($"Tipo de quarto com ID {rr.RoomTypeId} não encontrado.");

                if (roomType.AvailableRooms < rr.Quantity)
                    throw new Exception($"Quartos insuficientes disponíveis para o tipo {roomType.Name}.");

                // Atualiza disponibilidade (deduz)
                roomType.AvailableRooms -= rr.Quantity;
                await _hotelRepository.UpdateRoomTypeAsync(roomType);

                // Calcula preço
                totalPrice += roomType.Price * rr.Quantity * stayDays;

                // Cria nova reserva de quartos
                newReserveRooms.Add(new ReserveRoom
                {
                    RoomTypeId = rr.RoomTypeId,
                    Quantity = rr.Quantity,
                    ReserveId = reservation.ReserveId
                });
            }

            // Atualiza os quartos da reserva e preço total
            reservation.ReserveRooms = newReserveRooms;
            reservation.TotalPrice = totalPrice;

            // Atualiza a reserva
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
