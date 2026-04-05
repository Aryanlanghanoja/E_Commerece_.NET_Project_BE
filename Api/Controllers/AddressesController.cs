using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(IAddressService addressService, ILogger<AddressesController> logger)
    {
        _addressService = addressService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var addresses = await _addressService.GetByUserIdAsync(userId.Value);
        return Ok(ApiResponse<IEnumerable<Application.DTOs.Response.AddressResponse>>.SuccessResponse(addresses));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var address = await _addressService.GetByIdAsync(id);
        if (address == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Address not found"));
        }

        return Ok(ApiResponse<Application.DTOs.Response.AddressResponse>.SuccessResponse(address));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressRequest request)
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

        var address = await _addressService.CreateAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetById), new { id = address.Id },
            ApiResponse<Application.DTOs.Response.AddressResponse>.SuccessResponse(address, "Address created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateAddressRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed"));
        }

        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var result = await _addressService.UpdateAsync(id, request, userId.Value);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Address not found"));
        }
        return Ok(ApiResponse.SuccessResponse("Address updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var result = await _addressService.DeleteAsync(id, userId.Value);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Address not found"));
        }
        return Ok(ApiResponse.SuccessResponse("Address deleted successfully"));
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
