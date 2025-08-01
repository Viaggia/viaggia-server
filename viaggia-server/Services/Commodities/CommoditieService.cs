using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using viaggia_server.Data;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Commodity;
using viaggia_server.Models.Commodities;
using viaggia_server.Services.Commodities;

namespace viaggia_server.Services
{
    public class CommoditieService : ICommoditieService
    {
        private readonly AppDbContext _context;

        public CommoditieService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CommoditieDTO>> GetAllAsync()
        {
            var commodities = await _context.Commodities
                .Where(c => c.IsActive)
                .Include(c => c.CommoditieServices.Where(s => s.IsActive))
                .ToListAsync();

            return commodities.Select(c => MapToDTO(c)).ToList();
        }

        public async Task<CommoditieDTO> GetByIdAsync(int id)
        {
            var commodity = await _context.Commodities
                .Include(c => c.CommoditieServices.Where(s => s.IsActive))
                .FirstOrDefaultAsync(c => c.CommoditieId == id && c.IsActive);

            if (commodity == null)
                return null!;

            return MapToDTO(commodity);
        }

        public async Task<CommoditieDTO?> GetByHotelIdAsync(int hotelId)
        {
            var commoditie = await _context.Commodities
                .Include(c => c.CommoditieServices) // Inclui os serviços personalizados
                .FirstOrDefaultAsync(c => c.HotelId == hotelId && c.IsActive);

            if (commoditie == null)
                return null;

            return new CommoditieDTO
            {
                
                HotelId = commoditie.HotelId,
                HasParking = commoditie.HasParking,
                IsParkingPaid = !commoditie.IsParkingPaid,
                HasBreakfast = commoditie.HasBreakfast,
                IsBreakfastPaid = !commoditie.IsBreakfastPaid,
                HasLunch = commoditie.HasLunch,
                IsLunchPaid = !commoditie.IsLunchPaid,
                HasDinner = commoditie.HasDinner,
                IsDinnerPaid = !commoditie.IsDinnerPaid,
                HasSpa = commoditie.HasSpa,
                IsSpaPaid = !commoditie.IsSpaPaid,
                HasPool = commoditie.HasPool,
                IsPoolPaid = !commoditie.IsPoolPaid,
                HasGym = commoditie.HasGym,
                IsGymPaid = !commoditie.IsGymPaid,
                HasWiFi = commoditie.HasWiFi,
                IsWiFiPaid = !commoditie.IsWiFiPaid,
                HasAirConditioning = commoditie.HasAirConditioning,
                IsAirConditioningPaid = !commoditie.IsAirConditioningPaid,
                HasAccessibilityFeatures = commoditie.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = !commoditie.IsAccessibilityFeaturesPaid,
                IsPetFriendly = commoditie.IsPetFriendly,
                IsPetFriendlyPaid = !commoditie.IsPetFriendlyPaid,
                IsActive = commoditie.IsActive,

                CommoditieServices = commoditie.CommoditieServices?.Select(s => new CommoditieServicesDTO
                {
                    Name = s.Name,
                    IsPaid = !s.IsPaid,
                    Description = s.Description,
                    IsActive = s.IsActive
                }).ToList() ?? new List<CommoditieServicesDTO>()
            };
        }

        public async Task<CommoditieDTO> CreateAsync(CreateCommoditieDTO createDto)
        {
            var commodity = new Commoditie
            {
                HotelId = createDto.HotelId,
                HasParking = createDto.HasParking,
                IsParkingPaid = !createDto.IsParkingPaid,
                HasBreakfast = createDto.HasBreakfast,
                IsBreakfastPaid = !createDto.IsBreakfastPaid,
                HasLunch = createDto.HasLunch,
                IsLunchPaid = !createDto.IsLunchPaid,
                HasDinner = createDto.HasDinner,
                IsDinnerPaid = !createDto.IsDinnerPaid,
                HasSpa = createDto.HasSpa,
                IsSpaPaid = !createDto.IsSpaPaid,
                HasPool = createDto.HasPool,
                IsPoolPaid = !createDto.IsPoolPaid,
                HasGym = createDto.HasGym,
                IsGymPaid = !createDto.IsGymPaid,
                HasWiFi = createDto.HasWiFi,
                IsWiFiPaid = !createDto.IsWiFiPaid,
                HasAirConditioning = createDto.HasAirConditioning,
                IsAirConditioningPaid = !createDto.IsAirConditioningPaid,
                HasAccessibilityFeatures = createDto.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = !createDto.IsAccessibilityFeaturesPaid,
                IsPetFriendly = createDto.IsPetFriendly,
                IsPetFriendlyPaid = !createDto.IsPetFriendlyPaid,
                IsActive = true,
            };

            // Adiciona serviços personalizados se houver
            if (createDto.CommoditieServices != null && createDto.CommoditieServices.Any())
            {
                commodity.CommoditieServices = createDto.CommoditieServices.Select(s => new CommoditieServices
                {
                    Name = s.Name,
                    IsPaid = !s.IsPaid,
                    Description = s.Description,
                    IsActive = true
                }).ToList();
            }

            await _context.Commodities.AddAsync(commodity);
            await _context.SaveChangesAsync();

            return MapToDTO(commodity);
        }

        public async Task<CommoditieDTO> UpdateAsync(int id, CreateCommoditieDTO updateDto)
        {
            var commodity = await _context.Commodities
                .Include(c => c.CommoditieServices)
                .FirstOrDefaultAsync(c => c.CommoditieId == id && c.IsActive);

            if (commodity == null)
                return null!;

            // Atualiza campos básicos
            commodity.HasParking = updateDto.HasParking;
            commodity.IsParkingPaid = !updateDto.IsParkingPaid;
            commodity.HasBreakfast = updateDto.HasBreakfast;
            commodity.IsBreakfastPaid = !updateDto.IsBreakfastPaid;
            commodity.HasLunch = updateDto.HasLunch;
            commodity.IsLunchPaid = !updateDto.IsLunchPaid;
            commodity.HasDinner = updateDto.HasDinner;
            commodity.IsDinnerPaid = !updateDto.IsDinnerPaid;
            commodity.HasSpa = updateDto.HasSpa;
            commodity.IsSpaPaid = !updateDto.IsSpaPaid;
            commodity.HasPool = updateDto.HasPool;
            commodity.IsPoolPaid = !updateDto.IsPoolPaid;
            commodity.HasGym = updateDto.HasGym;
            commodity.IsGymPaid = !updateDto.IsGymPaid;
            commodity.HasWiFi = updateDto.HasWiFi;
            commodity.IsWiFiPaid = !updateDto.IsWiFiPaid;
            commodity.HasAirConditioning = updateDto.HasAirConditioning;
            commodity.IsAirConditioningPaid = !updateDto.IsAirConditioningPaid;
            commodity.HasAccessibilityFeatures = updateDto.HasAccessibilityFeatures;
            commodity.IsAccessibilityFeaturesPaid = !updateDto.IsAccessibilityFeaturesPaid;
            commodity.IsPetFriendly = updateDto.IsPetFriendly;
            commodity.IsPetFriendlyPaid = !updateDto.IsPetFriendlyPaid;

            // Atualiza serviços personalizados
            if (updateDto.CommoditieServices != null)
            {
                var servicesToKeep = updateDto.CommoditieServices.Select(s => s.Name).ToHashSet();

                foreach (var existingService in commodity.CommoditieServices)
                {
                    if (!servicesToKeep.Contains(existingService.Name))
                    {
                        existingService.IsActive = false;
                    }
                }

                foreach (var newServiceDto in updateDto.CommoditieServices)
                {
                    var existingService = commodity.CommoditieServices.FirstOrDefault(s => s.Name == newServiceDto.Name);

                    if (existingService != null)
                    {
                        existingService.IsPaid = !newServiceDto.IsPaid;
                        existingService.IsActive = true;
                    }
                    else
                    {
                        commodity.CommoditieServices.Add(new CommoditieServices
                        {
                            Name = newServiceDto.Name,
                            IsPaid = !newServiceDto.IsPaid,
                            Description = newServiceDto.Description,
                            IsActive = true,
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return MapToDTO(commodity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var commodity = await _context.Commodities.FindAsync(id);

            if (commodity == null || !commodity.IsActive)
                return false;

            commodity.IsActive = false;
            _context.Commodities.Update(commodity);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            var commodity = await _context.Commodities.FindAsync(id);

            if (commodity == null)
                return false;

            commodity.IsActive = !commodity.IsActive;
            _context.Commodities.Update(commodity);

            return await _context.SaveChangesAsync() > 0;
        }

        private CommoditieDTO MapToDTO(Commoditie commodity)
        {
            return new CommoditieDTO
            {
                
                HotelId = commodity.HotelId,
                HasParking = commodity.HasParking,
                IsParkingPaid = !commodity.IsParkingPaid,
                HasBreakfast = commodity.HasBreakfast,
                IsBreakfastPaid = !commodity.IsBreakfastPaid,
                HasLunch = commodity.HasLunch,
                IsLunchPaid = !commodity.IsLunchPaid,
                HasDinner = commodity.HasDinner,
                IsDinnerPaid = !commodity.IsDinnerPaid,
                HasSpa = commodity.HasSpa,
                IsSpaPaid = !commodity.IsSpaPaid,
                HasPool = commodity.HasPool,
                IsPoolPaid = !commodity.IsPoolPaid,
                HasGym = commodity.HasGym,
                IsGymPaid = !commodity.IsGymPaid,
                HasWiFi = commodity.HasWiFi,
                IsWiFiPaid = !commodity.IsWiFiPaid,
                HasAirConditioning = commodity.HasAirConditioning,
                IsAirConditioningPaid = !commodity.IsAirConditioningPaid,
                HasAccessibilityFeatures = commodity.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = !commodity.IsAccessibilityFeaturesPaid,
                IsPetFriendly = commodity.IsPetFriendly,
                IsPetFriendlyPaid = !commodity.IsPetFriendlyPaid,
                IsActive = commodity.IsActive,
                CommoditieServices = commodity.CommoditieServices?
                    .Where(s => s.IsActive)
                    .Select(s => new CommoditieServicesDTO
                    {
                        Name = s.Name,
                        IsPaid = !s.IsPaid,
                        Description = s.Description,
                        IsActive = s.IsActive
                    }).ToList() ?? new List<CommoditieServicesDTO>()
            };
        }

      
    }
}