namespace viaggia_server.Services.ReservationServices
{
    public class StayCalculator
    {
        public DateTime CheckIn { get; }
        public DateTime CheckOut { get; }

        public StayCalculator(DateTime checkIn, DateTime checkOut) 
        {
            if (CheckIn == DateTime.MinValue) throw new ArgumentException("Check-in date is required.");
            if (CheckOut == DateTime.MinValue) throw new ArgumentException("Check-out date is required.");
            if (CheckOut <= checkIn) throw new ArgumentException("Check-out must be after check-in");

            CheckIn = checkIn;
            CheckOut = checkOut;
        }

        public int CalculateDays(DateTime CheckIn, DateTime CheckOut)
        {
            // Normaliza as datas para apenas data + hora de corte
            var totalDays = (CheckOut - CheckIn).Days;

            return Math.Max(totalDays, 1);
        }
    }
}
