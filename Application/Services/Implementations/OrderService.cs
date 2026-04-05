using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Domain.Enums;
using ECommerceApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Application.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IProductRepository productRepository,
        IAddressRepository addressRepository,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
        _addressRepository = addressRepository;
        _logger = logger;
    }

    public async Task<OrderResponse?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return null;

        var response = MapToResponse(order);
        response.Items = (await _orderItemRepository.GetByOrderIdAsync(id))
            .Select(MapItemToResponse).ToList();

        return response;
    }

    public async Task<PagedResponse<OrderResponse>> GetByCustomerIdAsync(int customerId, int page = 1, int pageSize = 20)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId, page, pageSize);
        var totalCount = await _orderRepository.GetTotalCountByCustomerIdAsync(customerId);

        var responses = orders.Select(o =>
        {
            var response = MapToResponse(o);
            response.Items = _orderItemRepository.GetByOrderIdAsync(o.Id).Result
                .Select(MapItemToResponse).ToList();
            return response;
        }).ToList();

        return new PagedResponse<OrderResponse>
        {
            Items = responses,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, int customerId)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Order must have at least one item");
        }

        var address = await _addressRepository.GetByIdAsync(request.AddressId);
        if (address == null || address.UserId != customerId)
        {
            throw new KeyNotFoundException("Invalid address");
        }

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {item.ProductId} not found");
            }

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ItemCount = item.Quantity,
                PurchasePrice = product.Price,
                CreatedAt = DateTime.UtcNow
            });

            totalAmount += product.Price * item.Quantity;
        }

        var order = new Order
        {
            CustomerId = customerId,
            AddressId = request.AddressId,
            ArrivingTime = request.ArrivingTime,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow
        };

        var orderId = await _orderRepository.CreateAsync(order);
        order.Id = orderId;

        foreach (var item in orderItems)
        {
            item.OrderId = orderId;
        }
        await _orderItemRepository.CreateBulkAsync(orderItems);

        _logger.LogInformation("Order created: {OrderId} for customer: {CustomerId}", orderId, customerId);

        var response = MapToResponse(order);
        response.Items = orderItems.Select(MapItemToResponse).ToList();
        return response;
    }

    public async Task<bool> UpdateStatusAsync(int id, UpdateOrderStatusRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
        {
            throw new InvalidOperationException("Invalid order status");
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        var result = await _orderRepository.UpdateAsync(order);
        
        if (result)
            _logger.LogInformation("Order status updated: {OrderId} to {Status}", id, status);

        return result;
    }

    public async Task<bool> CancelAsync(int id, int customerId)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (order.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own orders");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be cancelled");
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        var result = await _orderRepository.UpdateAsync(order);
        
        if (result)
            _logger.LogInformation("Order cancelled: {OrderId}", id);

        return result;
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            AddressId = order.AddressId,
            ArrivingTime = order.ArrivingTime,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };
    }

    private static OrderItemResponse MapItemToResponse(OrderItem item)
    {
        return new OrderItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ItemCount = item.ItemCount,
            PurchasePrice = item.PurchasePrice
        };
    }
}
