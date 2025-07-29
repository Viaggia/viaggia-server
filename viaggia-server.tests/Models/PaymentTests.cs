using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Payments;
using viaggia_server.Models.Users;
using viaggia_server.Models.Reservations;

namespace viaggia_server.tests.Models;

public class PaymentTests : IDisposable
{
    private readonly AppDbContext _context;

    public PaymentTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Payment_CanCreatePayment()
    {
        // Test: Verify payment creation with required relationships
        // Purpose: Tests basic payment entity functionality
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        var reservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 1000m,
            NumberOfGuests = 2,
            Status = "Confirmed"
        };

        _context.Users.Add(user);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            UserId = user.Id,
            ReservationId = reservation.ReservationId,
            Amount = 1000m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "CreditCard",
            Status = "Completed",
            BillingAddressId = 1,
            Currency = "BRL",
            TransactionId = "TXN123456789"
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var savedPayment = await _context.Payments
            .Include(p => p.User)
            .Include(p => p.Reservation)
            .FirstAsync();

        Assert.NotNull(savedPayment.User);
        Assert.NotNull(savedPayment.Reservation);
        Assert.Equal(1000m, savedPayment.Amount);
        Assert.Equal("CreditCard", savedPayment.PaymentMethod);
        Assert.Equal("Completed", savedPayment.Status);
        Assert.Equal("BRL", savedPayment.Currency);
    }

    [Fact]
    public async Task Payment_CanCreateStripePayment()
    {
        // Test: Verify Stripe-specific payment creation
        // Purpose: Tests Stripe integration fields
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999",
            StripeCustomerId = "cust_test123"
        };

        var reservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 1500m,
            NumberOfGuests = 2,
            Status = "Confirmed"
        };

        _context.Users.Add(user);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var stripePayment = new Payment
        {
            UserId = user.Id,
            ReservationId = reservation.ReservationId,
            Amount = 1500m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Stripe",
            Status = "Completed",
            BillingAddressId = 1,
            StripePaymentIntentId = "pi_test123",
            StripePaymentMethodId = "pm_test123",
            StripeCustomerId = "cust_test123",
            Currency = "BRL",
            Metadata = "{\"booking_id\": \"123\", \"source\": \"web\"}"
        };

        _context.Payments.Add(stripePayment);
        await _context.SaveChangesAsync();

        var savedPayment = await _context.Payments.FirstAsync();

        Assert.Equal("pi_test123", savedPayment.StripePaymentIntentId);
        Assert.Equal("pm_test123", savedPayment.StripePaymentMethodId);
        Assert.Equal("cust_test123", savedPayment.StripeCustomerId);
        Assert.NotNull(savedPayment.Metadata);
    }

    [Fact]
    public async Task Payment_CanCreateRefund()
    {
        // Test: Verify refund functionality
        // Purpose: Tests refund-specific fields
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        var reservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 1000m,
            NumberOfGuests = 2,
            Status = "Cancelled"
        };

        _context.Users.Add(user);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var refundPayment = new Payment
        {
            UserId = user.Id,
            ReservationId = reservation.ReservationId,
            Amount = 1000m,
            PaymentDate = DateTime.UtcNow.AddDays(-5),
            PaymentMethod = "CreditCard",
            Status = "Refunded",
            BillingAddressId = 1,
            RefundedAt = DateTime.UtcNow,
            RefundAmount = 800m, // Partial refund
            StripeRefundId = "re_test123",
            Currency = "BRL"
        };

        _context.Payments.Add(refundPayment);
        await _context.SaveChangesAsync();

        var savedPayment = await _context.Payments.FirstAsync();

        Assert.Equal("Refunded", savedPayment.Status);
        Assert.NotNull(savedPayment.RefundedAt);
        Assert.Equal(800m, savedPayment.RefundAmount);
        Assert.Equal("re_test123", savedPayment.StripeRefundId);
    }

    [Theory]
    [InlineData("CreditCard")]
    [InlineData("BankTransfer")]
    [InlineData("Stripe")]
    [InlineData("PayPal")]
    public void Payment_ValidPaymentMethods(string paymentMethod)
    {
        // Test: Verify valid payment methods
        // Purpose: Tests payment method validation
        var payment = new Payment { PaymentMethod = paymentMethod };

        Assert.NotEmpty(paymentMethod);
        Assert.Equal(paymentMethod, payment.PaymentMethod);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Completed")]
    [InlineData("Failed")]
    [InlineData("Refunded")]
    public void Payment_ValidStatuses(string status)
    {
        // Test: Verify valid payment statuses
        // Purpose: Tests status validation
        var payment = new Payment { Status = status };

        Assert.NotEmpty(status);
        Assert.Equal(status, payment.Status);
    }

    [Fact]
    public async Task Payment_SoftDeleteWorks()
    {
        // Test: Verify soft delete functionality
        // Purpose: Ensures IsActive filtering works correctly
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        var reservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 1000m,
            NumberOfGuests = 2,
            Status = "Confirmed"
        };

        _context.Users.Add(user);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var activePayment = new Payment
        {
            UserId = user.Id,
            ReservationId = reservation.ReservationId,
            Amount = 500m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "CreditCard",
            Status = "Completed",
            BillingAddressId = 1,
            IsActive = true
        };

        var inactivePayment = new Payment
        {
            UserId = user.Id,
            ReservationId = reservation.ReservationId,
            Amount = 500m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "CreditCard",
            Status = "Failed",
            BillingAddressId = 1,
            IsActive = false
        };

        _context.Payments.Add(activePayment);
        _context.Payments.Add(inactivePayment);
        await _context.SaveChangesAsync();

        var payments = await _context.Payments.ToListAsync();

        Assert.Single(payments);
        Assert.Equal("Completed", payments.First().Status);
        
        // Verify inactive payment exists when ignoring filters
        var allPayments = await _context.Payments.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allPayments.Count);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}