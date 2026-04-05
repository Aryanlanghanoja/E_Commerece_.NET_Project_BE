using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var orders = await _orderService.GetByCustomerIdAsync(userId.Value, page, pageSize);
        return Ok(ApiResponse<Application.DTOs.Response.PagedResponse<Application.DTOs.Response.OrderResponse>>.SuccessResponse(orders));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Order not found"));
        }

        if (order.CustomerId != userId.Value && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(ApiResponse<Application.DTOs.Response.OrderResponse>.SuccessResponse(order));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        }

        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var order = await _orderService.CreateAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponse<Application.DTOs.Response.OrderResponse>.SuccessResponse(order, "Order created successfully"));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed"));
        }

        var result = await _orderService.UpdateStatusAsync(id, request);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Order not found"));
        }
        return Ok(ApiResponse.SuccessResponse("Order status updated successfully"));
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var result = await _orderService.CancelAsync(id, userId.Value);
        if (!result)
        {
            return BadRequest(ApiResponse.ErrorResponse("Failed to cancel order"));
        }
        return Ok(ApiResponse.SuccessResponse("Order cancelled successfully"));
    }

    private int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
