using Stripe;
using viaggia_server.DTOs.Payments;

namespace viaggia_server.Services.Payment
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Cria um Payment Intent no Stripe para processar o pagamento
        /// </summary>
        Task<PaymentIntentDTO> CreatePaymentIntentAsync(int id);
    }
}
