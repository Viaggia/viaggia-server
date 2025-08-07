using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Reserves;

namespace viaggia_server.Services.Payment
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Cria um Payment Intent no Stripe para processar o pagamento
        /// </summary>
        Task<Session> CreatePaymentIntentAsync(ReserveCreateDTO createResevation);
        Task<Balance> GetBalanceAsync();
        Task<List<HotelBalanceDTO>> GetBalanceByHotelAsync();
        Task HandleStripeWebhookAsync(HttpRequest request);
    }
}
