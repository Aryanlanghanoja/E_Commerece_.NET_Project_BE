using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Payment not found"));
        }
        return Ok(ApiResponse<Application.DTOs.Response.PaymentResponse>.SuccessResponse(payment));
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        var payment = await _paymentService.GetByOrderIdAsync(orderId);
        if (payment == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Payment not found"));
        }
        return Ok(ApiResponse<Application.DTOs.Response.PaymentResponse>.SuccessResponse(payment));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        }

        var payment = await _paymentService.CreateAsync(request);
        return Ok(ApiResponse<Application.DTOs.Response.PaymentResponse>.SuccessResponse(payment, "Payment initiated successfully"));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status, [FromQuery] string? transactionId = null)
    {
        if (string.IsNullOrEmpty(status))
        {
            return BadRequest(ApiResponse.ErrorResponse("Status is required"));
        }

        var result = await _paymentService.UpdateStatusAsync(id, status, transactionId);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Payment not found"));
        }
        return Ok(ApiResponse.SuccessResponse("Payment status updated successfully"));
    }
}
