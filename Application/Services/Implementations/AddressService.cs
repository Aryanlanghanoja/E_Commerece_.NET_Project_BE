using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Application.Services.Implementations;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly ILogger<AddressService> _logger;

    public AddressService(IAddressRepository addressRepository, ILogger<AddressService> logger)
    {
        _addressRepository = addressRepository;
        _logger = logger;
    }

    public async Task<AddressResponse?> GetByIdAsync(int id)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        return address == null ? null : MapToResponse(address);
    }

    public async Task<IEnumerable<AddressResponse>> GetByUserIdAsync(int userId)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId);
        return addresses.Select(MapToResponse);
    }

    public async Task<AddressResponse> CreateAsync(CreateAddressRequest request, int userId)
    {
        var address = new Address
        {
            UserId = userId,
            Line1 = request.Line1,
            City = request.City,
            State = request.State,
            Pincode = request.Pincode,
            Country = request.Country,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _addressRepository.CreateAsync(address);
        address.Id = id;

        _logger.LogInformation("Address created: {AddressId} for user: {UserId}", id, userId);

        return MapToResponse(address);
    }

    public async Task<bool> UpdateAsync(int id, CreateAddressRequest request, int userId)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null)
        {
            throw new KeyNotFoundException("Address not found");
        }

        if (address.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own addresses");
        }

        address.Line1 = request.Line1;
        address.City = request.City;
        address.State = request.State;
        address.Pincode = request.Pincode;
        address.Country = request.Country;
        address.UpdatedAt = DateTime.UtcNow;

        var result = await _addressRepository.UpdateAsync(address);
        
        if (result)
            _logger.LogInformation("Address updated: {AddressId}", id);

        return result;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null)
        {
            throw new KeyNotFoundException("Address not found");
        }

        if (address.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own addresses");
        }

        var result = await _addressRepository.DeleteAsync(id);
        
        if (result)
            _logger.LogInformation("Address deleted: {AddressId}", id);

        return result;
    }

    private static AddressResponse MapToResponse(Address address)
    {
        return new AddressResponse
        {
            Id = address.Id,
            Line1 = address.Line1,
            City = address.City,
            State = address.State,
            Pincode = address.Pincode,
            Country = address.Country,
            CreatedAt = address.CreatedAt
        };
    }
}
