using Stripe.Checkout;
using viaggia_server.DTOs.ReservationDTO;

namespace viaggia_server.Services.Payment
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Cria um Payment Intent no Stripe para processar o pagamento
        /// </summary>
        Task<Session> CreatePaymentIntentAsync(CreateReservationDTO createResevation);
    }
}
