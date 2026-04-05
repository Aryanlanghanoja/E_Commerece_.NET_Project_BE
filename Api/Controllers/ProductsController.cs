using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var products = await _productService.GetAllAsync(categoryId, page, pageSize);
        return Ok(ApiResponse<Application.DTOs.Response.PagedResponse<Application.DTOs.Response.ProductResponse>>.SuccessResponse(products));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Product not found"));
        }
        return Ok(ApiResponse<Application.DTOs.Response.ProductResponse>.SuccessResponse(product));
    }

    [HttpGet("vendor/{vendorId}")]
    public async Task<IActionResult> GetByVendorId(int vendorId)
    {
        var currentUserId = GetUserIdFromToken();
        if (currentUserId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var products = await _productService.GetByVendorIdAsync(vendorId);
        return Ok(ApiResponse<IEnumerable<Application.DTOs.Response.ProductResponse>>.SuccessResponse(products));
    }

    [HttpPost]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        }

        var vendorId = GetUserIdFromToken() ?? 0;
        var product = await _productService.CreateAsync(request, vendorId);
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            ApiResponse<Application.DTOs.Response.ProductResponse>.SuccessResponse(product, "Product created successfully"));
    }

    [HttpPut]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed"));
        }

        var vendorId = GetUserIdFromToken() ?? 0;
        var result = await _productService.UpdateAsync(request, vendorId);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Product not found"));
        }
        return Ok(ApiResponse.SuccessResponse("Product updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var vendorId = GetUserIdFromToken() ?? 0;
        var result = await _productService.DeleteAsync(id, vendorId);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Product not found"));
        }
        return Ok(ApiResponse.SuccessResponse("Product deleted successfully"));
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
