using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Domain.Enums;
using ECommerceApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Application.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<PaymentResponse?> GetByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        return payment == null ? null : MapToResponse(payment);
    }

    public async Task<PaymentResponse?> GetByOrderIdAsync(int orderId)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        return payment == null ? null : MapToResponse(payment);
    }

    public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (!Enum.TryParse<PaymentMode>(request.PaymentMode, true, out var paymentMode))
        {
            throw new InvalidOperationException("Invalid payment mode");
        }

        var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);
        if (existingPayment != null)
        {
            throw new InvalidOperationException("Payment already exists for this order");
        }

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = order.TotalAmount,
            PaymentMode = paymentMode,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _paymentRepository.CreateAsync(payment);
        payment.Id = id;

        _logger.LogInformation("Payment initiated: {PaymentId} for order: {OrderId}", id, request.OrderId);

        return MapToResponse(payment);
    }

    public async Task<bool> UpdateStatusAsync(int id, string status, string? transactionId = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null)
        {
            throw new KeyNotFoundException("Payment not found");
        }

        if (!Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
        {
            throw new InvalidOperationException("Invalid payment status");
        }

        payment.Status = paymentStatus;
        payment.TransactionId = transactionId;
        
        if (paymentStatus == PaymentStatus.Success)
        {
            payment.PaidAt = DateTime.UtcNow;
        }
        
        payment.UpdatedAt = DateTime.UtcNow;

        var result = await _paymentRepository.UpdateAsync(payment);
        
        if (result)
        {
            _logger.LogInformation("Payment status updated: {PaymentId} to {Status}", id, status);
        }

        return result;
    }

    private static PaymentResponse MapToResponse(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMode = payment.PaymentMode.ToString(),
            Status = payment.Status.ToString(),
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };
    }
}
