using Stripe.Checkout;
using viaggia_server.DTOs.Reserves;

namespace viaggia_server.Services.Payment
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Cria um Payment Intent no Stripe para processar o pagamento
        /// </summary>
        Task<Session> CreatePaymentIntentAsync(ReservesCreateDTO createResevation);

        Task HandleStripeWebhookAsync(HttpRequest request);
    }
}
