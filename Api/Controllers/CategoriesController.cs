using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<Application.DTOs.Response.CategoryResponse>>.SuccessResponse(categories));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Category not found"));
        }
        return Ok(ApiResponse<Application.DTOs.Response.CategoryResponse>.SuccessResponse(category));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        }

        var category = await _categoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, 
            ApiResponse<Application.DTOs.Response.CategoryResponse>.SuccessResponse(category, "Category created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed"));
        }

        var result = await _categoryService.UpdateAsync(id, request);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Category not found"));
        }
        return Ok(ApiResponse.SuccessResponse("Category updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Category not found"));
        }
        return Ok(ApiResponse.SuccessResponse("Category deleted successfully"));
    }
}
