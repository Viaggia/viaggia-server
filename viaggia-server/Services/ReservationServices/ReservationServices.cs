using viaggia_server.DTOs.ReservationDTO;
using viaggia_server.Repositories.ReservationRepository;
using viaggia_server.Models.Reservations;

namespace viaggia_server.Services.ReservationServices
{
    public class ReservationServices :IReservationServices
    {
        private readonly IConfiguration _configuration;
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<ReservationServices> _logger;
        
        public ReservationServices(IConfiguration configuration, IReservationRepository reservationRepository, ILogger<ReservationServices> logger)
        {
            _configuration = configuration;
            _reservationRepository = reservationRepository;
            _logger = logger;
        }
        public Task<CreateReservation> CreateReservationAsync(Reservation createReservation)
        {
            throw new Exception("An error occurred while creating the reservation.");
            //try
            //{
            //    var reservation = new CreateReservation
            //    {
            //        UserId = createReservation.UserId,
            //        ReservationId = createReservation.ReservationId,
            //        HotelId = createReservation.HotelId,
            //        CheckInDate = createReservation.CheckInDate,
            //        CheckOutDate = createReservation.CheckOutDate,
            //        TotalPrice = createReservation.TotalPrice,
            //    };
            //    return _reservationRepository.CreateReservationAsync(reservation);
            //}
            //catch (Exception ex)
            //{
            //    // Handle exceptions, log errors, etc.
            //    throw new Exception("An error occurred while creating the reservation.", ex);
            //}
        }
        public Task<CreateReservation> GetReservationByIdAsync(CreateReservation createReservation)
        {
            try
            {
                //return reservationRepository.GetReservationByIdAsync();
            }
            catch(Exception ex)
            {
                // Handle exceptions, log errors, etc.
            }
                throw new Exception("An error occurred while retrieving the reservation.");
        }
        public Task<IEnumerable<CreateReservation>> GetAllReservationsAsync()
        {
            try
            {
                //return reservationRepository.GetAllReservationsAsync();   
            }
            catch(Exception ex)
            {
                // Handle exceptions, log errors, etc.
            }
                throw new Exception("An error occurred while retrieving all reservations.");
        }
    }
}
