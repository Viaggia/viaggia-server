using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commodity;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Reviews;
using Media = viaggia_server.Models.Medias.Media;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;

namespace viaggia_server.Services.HotelServices
{
    public class HotelServices : IHotelServices
    {
        private readonly IRepository<Hotel> _genericRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<HotelServices> _logger;

        public HotelServices(
            IRepository<Hotel> genericRepository,
            IHotelRepository hotelRepository,
            IReviewRepository reviewRepository,
            IWebHostEnvironment environment,
            ILogger<HotelServices> logger)
        {
            _genericRepository = genericRepository;
            _hotelRepository = hotelRepository;
            _reviewRepository = reviewRepository;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ApiResponse<List<HotelDTO>>> GetAllHotelAsync()
        {
            try
            {
                var hotels = await _genericRepository.GetAllAsync();
                var hotelDTOs = new List<HotelDTO>();
                foreach (var hotel in hotels)
                {
                    var roomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(hotel.HotelId)).ToList();
                    var medias = (await _hotelRepository.GetMediasByHotelIdAsync(hotel.HotelId)).ToList();
                    var reviews = (await _reviewRepository.GetReviewsByHotelIdAsync(hotel.HotelId)).ToList();
                    var packages = (await _hotelRepository.GetPackagesByHotelIdAsync(hotel.HotelId)).ToList();
                    var commodities = (await _hotelRepository.GetCommodityByHotelIdAsync(hotel.HotelId)).ToList();
                    var customCommodities = (await _hotelRepository.GetCustomCommodityByHotelIdAsync(hotel.HotelId)).ToList();

                    var dto = new HotelDTO
                    {
                        HotelId = hotel.HotelId,
                        Name = hotel.Name,
                        Cnpj = hotel.Cnpj,
                        Street = hotel.Street,
                        City = hotel.City,
                        State = hotel.State,
                        ZipCode = hotel.ZipCode,
                        Description = hotel.Description,
                        StarRating = hotel.StarRating,
                        CheckInTime = hotel.CheckInTime,
                        CheckOutTime = hotel.CheckOutTime,
                        ContactPhone = hotel.ContactPhone,
                        ContactEmail = hotel.ContactEmail,
                        IsActive = hotel.IsActive,
                        AverageRating = hotel.AverageRating,
                        RoomTypes = roomTypes.Select(rt => new HotelRoomTypeDTO
                        {
                            RoomTypeId = rt.RoomTypeId,
                            Name = rt.Name,
                            Description = rt.Description,
                            Price = rt.Price,
                            Capacity = rt.Capacity,
                            BedType = rt.BedType,
                            TotalRooms = rt.TotalRooms,
                            AvailableRooms = rt.AvailableRooms,
                            IsActive = rt.IsActive
                        }).ToList(),
                        Medias = medias.Select(m => new MediaDTO
                        {
                            MediaId = m.MediaId,
                            MediaUrl = m.MediaUrl,
                            MediaType = m.MediaType
                        }).ToList(),
                        Reviews = reviews.Select(r => new ReviewDTO
                        {
                            ReviewId = r.ReviewId,
                            UserId = r.UserId,
                            ReviewType = r.ReviewType,
                            HotelId = r.HotelId,
                            Rating = r.Rating,
                            Comment = r.Comment,
                            CreatedAt = r.CreatedAt,
                            IsActive = r.IsActive
                        }).ToList(),
                        Packages = packages.Select(p => new PackageDTO
                        {
                            PackageId = p.PackageId,
                            Name = p.Name,
                            Description = p.Description,
                            BasePrice = p.BasePrice,
                            IsActive = p.IsActive
                        }).ToList(),
                        Commodities = commodities.Select(c => new CommodityDTO
                        {
                            HotelId = c.HotelId,
                            HasParking = c.HasParking,
                            IsParkingPaid = c.IsParkingPaid,
                            ParkingPrice = c.ParkingPrice,
                            HasBreakfast = c.HasBreakfast,
                            IsBreakfastPaid = c.IsBreakfastPaid,
                            BreakfastPrice = c.BreakfastPrice,
                            HasLunch = c.HasLunch,
                            IsLunchPaid = c.IsLunchPaid,
                            LunchPrice = c.LunchPrice,
                            HasDinner = c.HasDinner,
                            IsDinnerPaid = c.IsDinnerPaid,
                            DinnerPrice = c.DinnerPrice,
                            HasSpa = c.HasSpa,
                            IsSpaPaid = c.IsSpaPaid,
                            SpaPrice = c.SpaPrice,
                            HasPool = c.HasPool,
                            IsPoolPaid = c.IsPoolPaid,
                            PoolPrice = c.PoolPrice,
                            HasGym = c.HasGym,
                            IsGymPaid = c.IsGymPaid,
                            GymPrice = c.GymPrice,
                            HasWiFi = c.HasWiFi,
                            IsWiFiPaid = c.IsWiFiPaid,
                            WiFiPrice = c.WiFiPrice,
                            HasAirConditioning = c.HasAirConditioning,
                            IsAirConditioningPaid = c.IsAirConditioningPaid,
                            AirConditioningPrice = c.AirConditioningPrice,
                            HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                            IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                            AccessibilityFeaturesPrice = c.AccessibilityFeaturesPrice,
                            IsPetFriendly = c.IsPetFriendly,
                            IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                            PetFriendlyPrice = c.PetFriendlyPrice,
                            IsActive = c.IsActive
                        }).ToList(),
                        CustomCommodities = customCommodities.Select(cs => new CustomCommodityDTO
                        {
                            CustomCommodityId = cs.CustomCommodityId,
                            HotelId = cs.HotelId,
                            Name = cs.Name,
                            IsPaid = cs.IsPaid,
                            Price = cs.Price,
                            Description = cs.Description,
                            IsActive = cs.IsActive
                        }).ToList()
                    };

                    hotelDTOs.Add(dto);
                }

                return new ApiResponse<List<HotelDTO>>(true, "Hotels retrieved successfully.", hotelDTOs);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error retrieving hotels: {Message}", innerMessage);
                return new ApiResponse<List<HotelDTO>>(false, $"Error retrieving hotels: {innerMessage}");
            }
        }

        public async Task<ApiResponse<HotelDTO>> GetHotelByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid hotel ID: {Id}", id);
                    return new ApiResponse<HotelDTO>(false, "Invalid hotel ID.");
                }

                var hotel = await _genericRepository.GetByIdAsync(id);
                if (hotel == null)
                {
                    _logger.LogWarning("Hotel not found for ID: {Id}", id);
                    return new ApiResponse<HotelDTO>(false, "Hotel not found.");
                }

                var roomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(hotel.HotelId)).ToList();
                var medias = (await _hotelRepository.GetMediasByHotelIdAsync(hotel.HotelId)).ToList();
                var reviews = (await _reviewRepository.GetReviewsByHotelIdAsync(hotel.HotelId)).ToList();
                var packages = (await _hotelRepository.GetPackagesByHotelIdAsync(hotel.HotelId)).ToList();
                var commodities = (await _hotelRepository.GetCommodityByHotelIdAsync(hotel.HotelId)).ToList();
                var customCommodities = (await _hotelRepository.GetCustomCommodityByHotelIdAsync(hotel.HotelId)).ToList();

                var dto = new HotelDTO
                {
                    HotelId = hotel.HotelId,
                    Name = hotel.Name,
                    Cnpj = hotel.Cnpj,
                    Street = hotel.Street,
                    City = hotel.City,
                    State = hotel.State,
                    ZipCode = hotel.ZipCode,
                    Description = hotel.Description,
                    StarRating = hotel.StarRating,
                    CheckInTime = hotel.CheckInTime,
                    CheckOutTime = hotel.CheckOutTime,
                    ContactPhone = hotel.ContactPhone,
                    ContactEmail = hotel.ContactEmail,
                    IsActive = hotel.IsActive,
                    AverageRating = hotel.AverageRating,
                    RoomTypes = roomTypes.Select(rt => new HotelRoomTypeDTO
                    {
                        RoomTypeId = rt.RoomTypeId,
                        Name = rt.Name,
                        Description = rt.Description,
                        Price = rt.Price,
                        Capacity = rt.Capacity,
                        BedType = rt.BedType,
                        TotalRooms = rt.TotalRooms,
                        AvailableRooms = rt.AvailableRooms,
                        IsActive = rt.IsActive
                    }).ToList(),
                    Medias = medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).ToList(),
                    Reviews = reviews.Select(r => new ReviewDTO
                    {
                        ReviewId = r.ReviewId,
                        UserId = r.UserId,
                        ReviewType = r.ReviewType,
                        HotelId = r.HotelId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        IsActive = r.IsActive
                    }).ToList(),
                    Packages = packages.Select(p => new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        IsActive = p.IsActive
                    }).ToList(),
                    Commodities = commodities.Select(c => new CommodityDTO
                    {
                        HotelId = c.HotelId,
                        HasParking = c.HasParking,
                        IsParkingPaid = c.IsParkingPaid,
                        ParkingPrice = c.ParkingPrice,
                        HasBreakfast = c.HasBreakfast,
                        IsBreakfastPaid = c.IsBreakfastPaid,
                        BreakfastPrice = c.BreakfastPrice,
                        HasLunch = c.HasLunch,
                        IsLunchPaid = c.IsLunchPaid,
                        LunchPrice = c.LunchPrice,
                        HasDinner = c.HasDinner,
                        IsDinnerPaid = c.IsDinnerPaid,
                        DinnerPrice = c.DinnerPrice,
                        HasSpa = c.HasSpa,
                        IsSpaPaid = c.IsSpaPaid,
                        SpaPrice = c.SpaPrice,
                        HasPool = c.HasPool,
                        IsPoolPaid = c.IsPoolPaid,
                        PoolPrice = c.PoolPrice,
                        HasGym = c.HasGym,
                        IsGymPaid = c.IsGymPaid,
                        GymPrice = c.GymPrice,
                        HasWiFi = c.HasWiFi,
                        IsWiFiPaid = c.IsWiFiPaid,
                        WiFiPrice = c.WiFiPrice,
                        HasAirConditioning = c.HasAirConditioning,
                        IsAirConditioningPaid = c.IsAirConditioningPaid,
                        AirConditioningPrice = c.AirConditioningPrice,
                        HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                        IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                        AccessibilityFeaturesPrice = c.AccessibilityFeaturesPrice,
                        IsPetFriendly = c.IsPetFriendly,
                        IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                        PetFriendlyPrice = c.PetFriendlyPrice,
                        IsActive = c.IsActive
                    }).ToList(),
                    CustomCommodities = customCommodities.Select(cs => new CustomCommodityDTO
                    {
                        CustomCommodityId = cs.CustomCommodityId,
                        HotelId = cs.HotelId,
                        Name = cs.Name,
                        IsPaid = cs.IsPaid,
                        Price = cs.Price,
                        Description = cs.Description,
                        IsActive = cs.IsActive
                    }).ToList()
                };

                return new ApiResponse<HotelDTO>(true, "Hotel retrieved successfully.", dto);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error retrieving hotel with ID {Id}: {Message}", id, innerMessage);
                return new ApiResponse<HotelDTO>(false, $"Error retrieving hotel: {innerMessage}");
            }
        }

        public async Task<ApiResponse<Hotel>> CreateHotelAsync(CreateHotelDTO createHotelDto, List<CreateHotelRoomTypeDTO> roomTypes)
        {
            if (createHotelDto == null)
            {
                _logger.LogError("CreateHotelDTO is null");
                return new ApiResponse<Hotel>(false, "Invalid payload.");
            }

            try
            {
                _logger.LogInformation("Processing CreateHotelDTO: {Dto}", JsonSerializer.Serialize(createHotelDto));

                var hotelExists = await _hotelRepository.NameExistsAsync(createHotelDto.Name);
                if (hotelExists)
                {
                    _logger.LogWarning("Hotel name already exists: {Name}", createHotelDto.Name);
                    return new ApiResponse<Hotel>(false, "Hotel name already exists.");
                }

                var cnpjExists = await _hotelRepository.CnpjExistsAsync(createHotelDto.Cnpj);
                if (cnpjExists)
                {
                    _logger.LogWarning("CNPJ already exists: {Cnpj}", createHotelDto.Cnpj);
                    return new ApiResponse<Hotel>(false, "CNPJ already exists.");
                }

                var hotel = new Hotel
                {
                    Name = createHotelDto.Name,
                    Cnpj = createHotelDto.Cnpj,
                    Street = createHotelDto.Street,
                    City = createHotelDto.City,
                    State = createHotelDto.State,
                    ZipCode = createHotelDto.ZipCode,
                    Description = createHotelDto.Description,
                    StarRating = createHotelDto.StarRating,
                    CheckInTime = createHotelDto.CheckInTime,
                    CheckOutTime = createHotelDto.CheckOutTime,
                    ContactPhone = createHotelDto.ContactPhone,
                    ContactEmail = createHotelDto.ContactEmail,
                    IsActive = createHotelDto.IsActive
                };

                var createdHotel = await _genericRepository.AddAsync(hotel);
                _logger.LogInformation("Hotel created with ID: {HotelId}", createdHotel.HotelId);

                if (createHotelDto.MediaFiles != null && createHotelDto.MediaFiles.Any())
                {
                    var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Hotel");
                    Directory.CreateDirectory(uploadPath);

                    foreach (var file in createHotelDto.MediaFiles)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!new[] { ".jpg", ".jpeg", ".png", ".mp4" }.Contains(ext) || file.Length > 10 * 1024 * 1024)
                        {
                            _logger.LogWarning("Invalid media file: {FileName}", file.FileName);
                            continue;
                        }

                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        await _hotelRepository.AddMediaAsync(new Media
                        {
                            MediaUrl = $"/Uploads/Hotel/{fileName}",
                            MediaType = file.ContentType,
                            HotelId = createdHotel.HotelId,
                            IsActive = true
                        });
                        _logger.LogInformation("Media added for hotel ID: {HotelId}, File: {FileName}", createdHotel.HotelId, fileName);
                    }
                }

                if (roomTypes != null && roomTypes.Any())
                {
                    foreach (var roomTypeDto in roomTypes)
                    {
                        if (!Enum.IsDefined(typeof(RoomTypeEnum), roomTypeDto.Name))
                        {
                            _logger.LogWarning("Invalid RoomTypeEnum value: {Name}", roomTypeDto.Name);
                            continue;
                        }

                        var roomType = new HotelRoomType
                        {
                            Name = roomTypeDto.Name,
                            Description = roomTypeDto.Description,
                            Price = roomTypeDto.Price,
                            Capacity = roomTypeDto.Capacity,
                            BedType = roomTypeDto.BedType,
                            TotalRooms = roomTypeDto.TotalRooms,
                            AvailableRooms = roomTypeDto.TotalRooms,
                            IsActive = true,
                            HotelId = createdHotel.HotelId
                        };
                        await _hotelRepository.AddRoomTypeAsync(roomType);
                        _logger.LogInformation("Room type added: {Name}, TotalRooms: {TotalRooms}, HotelId: {HotelId}",
                            roomType.Name, roomType.TotalRooms, createdHotel.HotelId);
                    }
                }
                else
                {
                    _logger.LogWarning("No room types provided for hotel ID: {HotelId}", createdHotel.HotelId);
                }

                var hotelWithDetails = await _genericRepository.GetByIdAsync(createdHotel.HotelId);
                if (hotelWithDetails != null)
                {
                    hotelWithDetails.RoomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(hotelWithDetails.HotelId)).ToList();
                    hotelWithDetails.Medias = (await _hotelRepository.GetMediasByHotelIdAsync(hotelWithDetails.HotelId)).ToList();
                    _logger.LogInformation("Hotel details fetched with {RoomCount} room types", hotelWithDetails.RoomTypes.Count);
                }

                return new ApiResponse<Hotel>(true, "Hotel created successfully.", hotelWithDetails ?? createdHotel);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error creating hotel: {Message}", innerMessage);
                return new ApiResponse<Hotel>(false, $"Error creating hotel: {innerMessage}");
            }
        }

        public async Task<ApiResponse<HotelDTO>> UpdateHotelAsync(UpdateHotelDto updateHotelDto, List<CreateHotelRoomTypeDTO>? roomTypes)
        {
            try
            {
                var hotel = await _genericRepository.GetByIdAsync(updateHotelDto.HotelId);
                if (hotel == null || !hotel.IsActive)
                {
                    _logger.LogWarning("Hotel not found or inactive for ID: {HotelId}", updateHotelDto.HotelId);
                    return new ApiResponse<HotelDTO>(false, "Hotel not found or inactive.");
                }

                // Validate Name and CNPJ if provided
                if (!string.IsNullOrEmpty(updateHotelDto.Name) && updateHotelDto.Name != hotel.Name)
                {
                    var nameExists = await _hotelRepository.NameExistsAsync(updateHotelDto.Name);
                    if (nameExists)
                    {
                        _logger.LogWarning("Hotel name already exists: {Name}", updateHotelDto.Name);
                        return new ApiResponse<HotelDTO>(false, "Hotel name already exists.");
                    }
                    hotel.Name = updateHotelDto.Name;
                }

                if (!string.IsNullOrEmpty(updateHotelDto.Cnpj) && updateHotelDto.Cnpj != hotel.Cnpj)
                {
                    var cnpjExists = await _hotelRepository.CnpjExistsAsync(updateHotelDto.Cnpj);
                    if (cnpjExists)
                    {
                        _logger.LogWarning("CNPJ already exists: {Cnpj}", updateHotelDto.Cnpj);
                        return new ApiResponse<HotelDTO>(false, "CNPJ already exists.");
                    }
                    hotel.Cnpj = updateHotelDto.Cnpj;
                }

                // Update other fields if provided
                hotel.Street = updateHotelDto.Street ?? hotel.Street;
                hotel.City = updateHotelDto.City ?? hotel.City;
                hotel.State = updateHotelDto.State ?? hotel.State;
                hotel.ZipCode = updateHotelDto.ZipCode ?? hotel.ZipCode;
                hotel.Description = updateHotelDto.Description ?? hotel.Description;
                hotel.StarRating = updateHotelDto.StarRating != 0 ? updateHotelDto.StarRating : hotel.StarRating;
                hotel.CheckInTime = updateHotelDto.CheckInTime ?? hotel.CheckInTime;
                hotel.CheckOutTime = updateHotelDto.CheckOutTime ?? hotel.CheckOutTime;
                hotel.ContactPhone = updateHotelDto.ContactPhone ?? hotel.ContactPhone;
                hotel.ContactEmail = updateHotelDto.ContactEmail ?? hotel.ContactEmail;
                hotel.IsActive = updateHotelDto.IsActive;

                await _genericRepository.UpdateAsync(hotel);
                _logger.LogInformation("Hotel updated with ID: {HotelId}", hotel.HotelId);

                // Handle media file uploads
                if (updateHotelDto.MediaFiles != null && updateHotelDto.MediaFiles.Any())
                {
                    var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Hotel");
                    Directory.CreateDirectory(uploadPath);

                    foreach (var file in updateHotelDto.MediaFiles)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!new[] { ".jpg", ".jpeg", ".png", ".mp4" }.Contains(ext) || file.Length > 10 * 1024 * 1024)
                        {
                            _logger.LogWarning("Invalid media file: {FileName}", file.FileName);
                            continue;
                        }

                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        await _hotelRepository.AddMediaAsync(new Media
                        {
                            MediaUrl = $"/Uploads/Hotel/{fileName}",
                            MediaType = file.ContentType,
                            HotelId = hotel.HotelId,
                            IsActive = true
                        });
                        _logger.LogInformation("Media added for hotel ID: {HotelId}, File: {FileName}", hotel.HotelId, fileName);
                    }
                }

                // Handle room types if provided
                if (roomTypes != null && roomTypes.Any())
                {
                    var existingRoomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(hotel.HotelId)).ToList();
                    foreach (var roomTypeDto in roomTypes)
                    {
                        if (!Enum.IsDefined(typeof(RoomTypeEnum), roomTypeDto.Name))
                        {
                            _logger.LogWarning("Invalid RoomTypeEnum value: {Name}", roomTypeDto.Name);
                            continue;
                        }

                        var existingRoomType = existingRoomTypes.FirstOrDefault(rt => rt.Name == roomTypeDto.Name && rt.IsActive);
                        if (existingRoomType != null)
                        {
                            // Update existing room type
                            existingRoomType.Description = roomTypeDto.Description ?? existingRoomType.Description;
                            existingRoomType.Price = roomTypeDto.Price != 0 ? roomTypeDto.Price : existingRoomType.Price;
                            existingRoomType.Capacity = roomTypeDto.Capacity != 0 ? roomTypeDto.Capacity : existingRoomType.Capacity;
                            existingRoomType.BedType = roomTypeDto.BedType ?? existingRoomType.BedType;
                            existingRoomType.TotalRooms = roomTypeDto.TotalRooms != 0 ? roomTypeDto.TotalRooms : existingRoomType.TotalRooms;
                            existingRoomType.AvailableRooms = roomTypeDto.TotalRooms != 0 ? roomTypeDto.TotalRooms : existingRoomType.AvailableRooms;
                            await _genericRepository.SaveChangesAsync();
                            _logger.LogInformation("Room type updated: {Name}, HotelId: {HotelId}", existingRoomType.Name, hotel.HotelId);
                        }
                        else
                        {
                            // Add new room type
                            var roomType = new HotelRoomType
                            {
                                Name = roomTypeDto.Name,
                                Description = roomTypeDto.Description,
                                Price = roomTypeDto.Price,
                                Capacity = roomTypeDto.Capacity,
                                BedType = roomTypeDto.BedType,
                                TotalRooms = roomTypeDto.TotalRooms,
                                AvailableRooms = roomTypeDto.TotalRooms,
                                IsActive = true,
                                HotelId = hotel.HotelId
                            };
                            await _hotelRepository.AddRoomTypeAsync(roomType);
                            _logger.LogInformation("Room type added: {Name}, TotalRooms: {TotalRooms}, HotelId: {HotelId}",
                                roomType.Name, roomType.TotalRooms, hotel.HotelId);
                        }
                    }
                }

                // Fetch updated hotel with related data
                var hotels = await _hotelRepository.GetHotelsWithRelatedDataAsync();
                var updatedHotel = hotels.FirstOrDefault(h => h.HotelId == hotel.HotelId && h.IsActive);
                if (updatedHotel == null)
                {
                    _logger.LogWarning("Updated hotel not found for ID: {HotelId}", hotel.HotelId);
                    return new ApiResponse<HotelDTO>(false, "Updated hotel not found.");
                }

                // Map to HotelDTO
                var hotelDto = new HotelDTO
                {
                    HotelId = updatedHotel.HotelId,
                    Name = updatedHotel.Name,
                    Cnpj = updatedHotel.Cnpj,
                    Street = updatedHotel.Street,
                    City = updatedHotel.City,
                    State = updatedHotel.State,
                    ZipCode = updatedHotel.ZipCode,
                    Description = updatedHotel.Description,
                    StarRating = updatedHotel.StarRating,
                    CheckInTime = updatedHotel.CheckInTime,
                    CheckOutTime = updatedHotel.CheckOutTime,
                    ContactPhone = updatedHotel.ContactPhone,
                    ContactEmail = updatedHotel.ContactEmail,
                    IsActive = updatedHotel.IsActive,
                    AverageRating = updatedHotel.AverageRating,
                    RoomTypes = updatedHotel.RoomTypes.Select(rt => new HotelRoomTypeDTO
                    {
                        RoomTypeId = rt.RoomTypeId,
                        Name = rt.Name,
                        Description = rt.Description,
                        Price = rt.Price,
                        Capacity = rt.Capacity,
                        BedType = rt.BedType,
                        TotalRooms = rt.TotalRooms,
                        AvailableRooms = rt.AvailableRooms,
                        IsActive = rt.IsActive
                    }).ToList(),
                    Medias = updatedHotel.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).ToList(),
                    Reviews = updatedHotel.Reviews.Select(r => new ReviewDTO
                    {
                        ReviewId = r.ReviewId,
                        UserId = r.UserId,
                        ReviewType = r.ReviewType,
                        HotelId = r.HotelId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        IsActive = r.IsActive
                    }).ToList(),
                    Packages = updatedHotel.Packages.Select(p => new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        IsActive = p.IsActive
                    }).ToList(),
                    Commodities = updatedHotel.Commodities.Select(c => new CommodityDTO
                    {
                        HotelId = c.HotelId,
                        HasParking = c.HasParking,
                        IsParkingPaid = c.IsParkingPaid,
                        ParkingPrice = c.ParkingPrice,
                        HasBreakfast = c.HasBreakfast,
                        IsBreakfastPaid = c.IsBreakfastPaid,
                        BreakfastPrice = c.BreakfastPrice,
                        HasLunch = c.HasLunch,
                        IsLunchPaid = c.IsLunchPaid,
                        LunchPrice = c.LunchPrice,
                        HasDinner = c.HasDinner,
                        IsDinnerPaid = c.IsDinnerPaid,
                        DinnerPrice = c.DinnerPrice,
                        HasSpa = c.HasSpa,
                        IsSpaPaid = c.IsSpaPaid,
                        SpaPrice = c.SpaPrice,
                        HasPool = c.HasPool,
                        IsPoolPaid = c.IsPoolPaid,
                        PoolPrice = c.PoolPrice,
                        HasGym = c.HasGym,
                        IsGymPaid = c.IsGymPaid,
                        GymPrice = c.GymPrice,
                        HasWiFi = c.HasWiFi,
                        IsWiFiPaid = c.IsWiFiPaid,
                        WiFiPrice = c.WiFiPrice,
                        HasAirConditioning = c.HasAirConditioning,
                        IsAirConditioningPaid = c.IsAirConditioningPaid,
                        AirConditioningPrice = c.AirConditioningPrice,
                        HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                        IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                        AccessibilityFeaturesPrice = c.AccessibilityFeaturesPrice,
                        IsPetFriendly = c.IsPetFriendly,
                        IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                        PetFriendlyPrice = c.PetFriendlyPrice,
                        IsActive = c.IsActive
                    }).ToList(),
                    CustomCommodities = updatedHotel.CustomCommodities.Select(cs => new CustomCommodityDTO
                    {
                        CustomCommodityId = cs.CustomCommodityId,
                        HotelId = cs.HotelId,
                        Name = cs.Name,
                        IsPaid = cs.IsPaid,
                        Price = cs.Price,
                        Description = cs.Description,
                        IsActive = cs.IsActive
                    }).ToList()
                };

                return new ApiResponse<HotelDTO>(true, "Hotel updated successfully.", hotelDto);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error updating hotel with ID {HotelId}: {Message}", updateHotelDto.HotelId, innerMessage);
                return new ApiResponse<HotelDTO>(false, $"Error updating hotel: {innerMessage}");
            }
        }


        public async Task<ApiResponse<double>> GetHotelAverageRatingAsync(int hotelId)
        {
            try
            {
                var averageRating = await _reviewRepository.CalculateHotelAverageRatingAsync(hotelId);
                var hotel = await _genericRepository.GetByIdAsync(hotelId);
                if (hotel != null)
                {
                    hotel.AverageRating = averageRating;
                    await _genericRepository.UpdateAsync(hotel);
                }
                return new ApiResponse<double>(true, "Average rating retrieved successfully.", averageRating);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new ApiResponse<double>(false, $"Error retrieving average rating: {innerMessage}");
            }
        }

        public async Task<ApiResponse<IEnumerable<PackageDTO>>> GetPackagesByHotelIdAsync(int hotelId)
        {
            try
            {
                var packages = await _hotelRepository.GetPackagesByHotelIdAsync(hotelId);
                var packageDTOs = packages.Select(p => new PackageDTO
                {
                    PackageId = p.PackageId,
                    Name = p.Name,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    IsActive = p.IsActive
                }).ToList();

                return new ApiResponse<IEnumerable<PackageDTO>>(true, "Packages retrieved successfully.", packageDTOs);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new ApiResponse<IEnumerable<PackageDTO>>(false, $"Error retrieving packages: {innerMessage}");
            }
        }

        public async Task<ApiResponse<List<HotelDTO>>> FilterHotelsAsync(HotelFilterDTO filter)
        {
            try
            {
                _logger.LogInformation("Filtering hotels with Commodities: {Commodities}, CustomCommodities: {CustomCommodities}, RoomTypes: {RoomTypes}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, MinCapacity: {MinCapacity}",
                    string.Join(", ", filter.Commodity ?? new List<string>()),
                    string.Join(", ", filter.CustomCommodity ?? new List<string>()),
                    string.Join(", ", filter.RoomTypes ?? new List<string>()),
                    filter.MinPrice, filter.MaxPrice, filter.MinCapacity);

                var filteredHotels = (await _hotelRepository.GetHotelsWithRelatedDataAsync()).AsQueryable();

                if (filter.Commodity != null && filter.Commodity.Any())
                {
                    foreach (var commodity in filter.Commodity)
                    {
                        switch (commodity.ToLower())
                        {
                            case "haswifi":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasWiFi));
                                break;
                            case "haspool":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasPool));
                                break;
                            case "hasgym":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasGym));
                                break;
                            case "hasparking":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasParking));
                                break;
                            case "hasbreakfast":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasBreakfast));
                                break;
                            case "haslunch":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasLunch));
                                break;
                            case "hasdinner":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasDinner));
                                break;
                            case "hasspa":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasSpa));
                                break;
                            case "hasairconditioning":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasAirConditioning));
                                break;
                            case "hasaccessibilityfeatures":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.HasAccessibilityFeatures));
                                break;
                            case "ispetfriendly":
                                filteredHotels = filteredHotels.Where(h => h.Commodities.Any(c => c.IsActive && c.IsPetFriendly));
                                break;
                            default:
                                _logger.LogWarning("Invalid commodity: {Commodity}", commodity);
                                break;
                        }
                    }
                }

                if (filter.CustomCommodity != null && filter.CustomCommodity.Any())
                {
                    foreach (var service in filter.CustomCommodity)
                    {
                        filteredHotels = filteredHotels.Where(h => h.CustomCommodities.Any(cs => cs.IsActive && cs.Name.ToLower() == service.ToLower()));
                    }
                }

                if (filter.RoomTypes != null && filter.RoomTypes.Any())
                {
                    foreach (var roomType in filter.RoomTypes)
                    {
                        if (Enum.TryParse<RoomTypeEnum>(roomType, true, out var parsedRoomType))
                        {
                            filteredHotels = filteredHotels.Where(h => h.RoomTypes.Any(rt => rt.IsActive && rt.Name == parsedRoomType));
                        }
                        else
                        {
                            _logger.LogWarning("Invalid room type: {RoomType}", roomType);
                        }
                    }
                }

                if (filter.MinCapacity.HasValue)
                {
                    filteredHotels = filteredHotels.Where(h => h.RoomTypes.Any(rt => rt.IsActive && rt.Capacity >= filter.MinCapacity.Value));
                }

                // Materialize the query to avoid assignment issues in EF Core
                var hotelsList = await filteredHotels.ToListAsync();

                if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue)
                {
                    hotelsList = hotelsList.Where(h =>
                    {
                        var roomPrices = h.RoomTypes.Where(rt => rt.IsActive).Select(rt => rt.Price).ToList();
                        if (!roomPrices.Any())
                            return false;

                        var minRoomPrice = roomPrices.Min();
                        var maxRoomPrice = roomPrices.Max();

                        decimal commodityCost = 0;
                        var commodities = h.Commodities.FirstOrDefault(c => c.IsActive);
                        if (commodities != null)
                        {
                            if (commodities.IsParkingPaid) commodityCost += commodities.ParkingPrice;
                            if (commodities.IsBreakfastPaid) commodityCost += commodities.BreakfastPrice;
                            if (commodities.IsLunchPaid) commodityCost += commodities.LunchPrice;
                            if (commodities.IsDinnerPaid) commodityCost += commodities.DinnerPrice;
                            if (commodities.IsSpaPaid) commodityCost += commodities.SpaPrice;
                            if (commodities.IsPoolPaid) commodityCost += commodities.PoolPrice;
                            if (commodities.IsGymPaid) commodityCost += commodities.GymPrice;
                            if (commodities.IsWiFiPaid) commodityCost += commodities.WiFiPrice;
                            if (commodities.IsAirConditioningPaid) commodityCost += commodities.AirConditioningPrice;
                            if (commodities.IsAccessibilityFeaturesPaid) commodityCost += commodities.AccessibilityFeaturesPrice;
                            if (commodities.IsPetFriendlyPaid) commodityCost += commodities.PetFriendlyPrice;
                        }

                        decimal customCommodityCost = h.CustomCommodities
                            .Where(cs => cs.IsActive && cs.IsPaid)
                            .Sum(cs => cs.Price);

                        var minTotalPrice = minRoomPrice + commodityCost + customCommodityCost;
                        var maxTotalPrice = maxRoomPrice + commodityCost + customCommodityCost;

                        bool meetsMinPrice = !filter.MinPrice.HasValue || minTotalPrice >= filter.MinPrice.Value;
                        bool meetsMaxPrice = !filter.MaxPrice.HasValue || maxTotalPrice <= filter.MaxPrice.Value;

                        return meetsMinPrice && meetsMaxPrice;
                    }).ToList();
                }

                var hotelDTOs = hotelsList.Select(hotel => new HotelDTO
                {
                    HotelId = hotel.HotelId,
                    Name = hotel.Name,
                    Cnpj = hotel.Cnpj,
                    Street = hotel.Street,
                    City = hotel.City,
                    State = hotel.State,
                    ZipCode = hotel.ZipCode,
                    Description = hotel.Description,
                    StarRating = hotel.StarRating,
                    CheckInTime = hotel.CheckInTime,
                    CheckOutTime = hotel.CheckOutTime,
                    ContactPhone = hotel.ContactPhone,
                    ContactEmail = hotel.ContactEmail,
                    IsActive = hotel.IsActive,
                    AverageRating = hotel.AverageRating,
                    RoomTypes = hotel.RoomTypes.Select(rt => new HotelRoomTypeDTO
                    {
                        RoomTypeId = rt.RoomTypeId,
                        Name = rt.Name,
                        Description = rt.Description,
                        Price = rt.Price,
                        Capacity = rt.Capacity,
                        BedType = rt.BedType,
                        TotalRooms = rt.TotalRooms,
                        AvailableRooms = rt.AvailableRooms,
                        IsActive = rt.IsActive
                    }).ToList(),
                    Medias = hotel.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).ToList(),
                    Reviews = hotel.Reviews.Select(r => new ReviewDTO
                    {
                        ReviewId = r.ReviewId,
                        UserId = r.UserId,
                        ReviewType = r.ReviewType,
                        HotelId = r.HotelId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        IsActive = r.IsActive
                    }).ToList(),
                    Packages = hotel.Packages.Select(p => new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        IsActive = p.IsActive
                    }).ToList(),
                    Commodities = hotel.Commodities.Select(c => new CommodityDTO
                    {
                        HotelId = c.HotelId,
                        HasParking = c.HasParking,
                        IsParkingPaid = c.IsParkingPaid,
                        ParkingPrice = c.ParkingPrice,
                        HasBreakfast = c.HasBreakfast,
                        IsBreakfastPaid = c.IsBreakfastPaid,
                        BreakfastPrice = c.BreakfastPrice,
                        HasLunch = c.HasLunch,
                        IsLunchPaid = c.IsLunchPaid,
                        LunchPrice = c.LunchPrice,
                        HasDinner = c.HasDinner,
                        IsDinnerPaid = c.IsDinnerPaid,
                        DinnerPrice = c.DinnerPrice,
                        HasSpa = c.HasSpa,
                        IsSpaPaid = c.IsSpaPaid,
                        SpaPrice = c.SpaPrice,
                        HasPool = c.HasPool,
                        IsPoolPaid = c.IsPoolPaid,
                        PoolPrice = c.PoolPrice,
                        HasGym = c.HasGym,
                        IsGymPaid = c.IsGymPaid,
                        GymPrice = c.GymPrice,
                        HasWiFi = c.HasWiFi,
                        IsWiFiPaid = c.IsWiFiPaid,
                        WiFiPrice = c.WiFiPrice,
                        HasAirConditioning = c.HasAirConditioning,
                        IsAirConditioningPaid = c.IsAirConditioningPaid,
                        AirConditioningPrice = c.AirConditioningPrice,
                        HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                        IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                        AccessibilityFeaturesPrice = c.AccessibilityFeaturesPrice,
                        IsPetFriendly = c.IsPetFriendly,
                        IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                        PetFriendlyPrice = c.PetFriendlyPrice,
                        IsActive = c.IsActive
                    }).ToList(),
                    CustomCommodities = hotel.CustomCommodities.Select(cs => new CustomCommodityDTO
                    {
                        CustomCommodityId = cs.CustomCommodityId, // Added for completeness
                        HotelId = cs.HotelId, // Added for completeness
                        Name = cs.Name,
                        IsPaid = cs.IsPaid,
                        Price = cs.Price,
                        Description = cs.Description,
                        IsActive = cs.IsActive
                    }).ToList()
                }).ToList();

                return new ApiResponse<List<HotelDTO>>(true, "Hotels retrieved successfully.", hotelDTOs);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error filtering hotels: {Message}", innerMessage);
                return new ApiResponse<List<HotelDTO>>(false, $"Error retrieving hotels: {innerMessage}");
            }
        }

        public async Task<ApiResponse<List<HotelRoomTypeDTO>>> GetAvailableRoomsAsync(int hotelId, int numberOfPeople, DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                if (hotelId <= 0)
                {
                    _logger.LogWarning("Invalid hotel ID: {Id}", hotelId);
                    return new ApiResponse<List<HotelRoomTypeDTO>>(false, "Invalid hotel ID.");
                }

                if (numberOfPeople <= 0)
                {
                    _logger.LogWarning("Invalid number of people: {NumberOfPeople}", numberOfPeople);
                    return new ApiResponse<List<HotelRoomTypeDTO>>(false, "Number of people must be greater than 0.");
                }

                if (checkInDate >= checkOutDate)
                {
                    _logger.LogWarning("Invalid date range: CheckInDate {CheckInDate} must be before CheckOutDate {CheckOutDate}", checkInDate, checkOutDate);
                    return new ApiResponse<List<HotelRoomTypeDTO>>(false, "Check-in date must be before check-out date.");
                }

                var roomTypes = await _hotelRepository.GetAvailableRoomTypesAsync(hotelId, numberOfPeople, checkInDate, checkOutDate);
                var roomTypeDTOs = roomTypes.Select(rt => new HotelRoomTypeDTO
                {
                    RoomTypeId = rt.RoomTypeId,
                    Name = rt.Name,
                    Description = rt.Description,
                    Price = rt.Price,
                    Capacity = rt.Capacity,
                    BedType = rt.BedType,
                    TotalRooms = rt.TotalRooms,
                    AvailableRooms = rt.AvailableRooms,
                    IsActive = rt.IsActive
                }).ToList();

                return new ApiResponse<List<HotelRoomTypeDTO>>(true, "Available rooms retrieved successfully.", roomTypeDTOs);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error retrieving available rooms for HotelId {HotelId}: {Message}", hotelId, innerMessage);
                return new ApiResponse<List<HotelRoomTypeDTO>>(false, $"Error retrieving available rooms: {innerMessage}");
            }
        }

        public async Task<ApiResponse<ReviewDTO>> AddHotelReviewAsync(CreateReviewDTO reviewDto)
        {
            try
            {
                if (reviewDto.HotelId <= 0)
                {
                    _logger.LogWarning("Invalid hotel ID: {HotelId}", reviewDto.HotelId);
                    return new ApiResponse<ReviewDTO>(false, "Invalid hotel ID.");
                }

                if (reviewDto.UserId <= 0)
                {
                    _logger.LogWarning("Invalid user ID: {UserId}", reviewDto.UserId);
                    return new ApiResponse<ReviewDTO>(false, "Invalid user ID.");
                }

                if (reviewDto.ReviewType != "Hotel")
                {
                    _logger.LogWarning("Invalid review type: {ReviewType}. Must be 'Hotel'.", reviewDto.ReviewType);
                    return new ApiResponse<ReviewDTO>(false, "Review type must be 'Hotel'.");
                }

                var hotel = await _genericRepository.GetByIdAsync(reviewDto.HotelId.Value);
                if (hotel == null || !hotel.IsActive)
                {
                    _logger.LogWarning("Hotel not found or inactive for HotelId: {HotelId}", reviewDto.HotelId);
                    return new ApiResponse<ReviewDTO>(false, "Hotel not found or inactive.");
                }

                var review = new Review
                {
                    UserId = reviewDto.UserId,
                    ReviewType = reviewDto.ReviewType,
                    HotelId = reviewDto.HotelId,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createdReview = await _reviewRepository.CreateReviewAsync(review);

                // Update hotel's average rating
                var averageRating = await _reviewRepository.CalculateHotelAverageRatingAsync(reviewDto.HotelId.Value);
                hotel.AverageRating = averageRating;
                await _genericRepository.UpdateAsync(hotel);

                var reviewDtoResult = new ReviewDTO
                {
                    ReviewId = createdReview.ReviewId,
                    UserId = createdReview.UserId,
                    ReviewType = createdReview.ReviewType,
                    HotelId = createdReview.HotelId,
                    Rating = createdReview.Rating,
                    Comment = createdReview.Comment,
                    CreatedAt = createdReview.CreatedAt,
                    IsActive = createdReview.IsActive
                };

                return new ApiResponse<ReviewDTO>(true, "Review created successfully.", reviewDtoResult);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error creating review for HotelId {HotelId}: {Message}", reviewDto.HotelId, innerMessage);
                return new ApiResponse<ReviewDTO>(false, $"Error creating review: {innerMessage}");
            }
        }

        public async Task<ApiResponse<List<ReviewDTO>>> GetHotelReviewsAsync(int hotelId)
        {
            try
            {
                if (hotelId <= 0)
                {
                    _logger.LogWarning("Invalid hotel ID: {HotelId}", hotelId);
                    return new ApiResponse<List<ReviewDTO>>(false, "Invalid hotel ID.");
                }

                var reviews = await _reviewRepository.GetReviewsByHotelIdAsync(hotelId);
                var reviewDTOs = reviews.Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    ReviewType = r.ReviewType,
                    HotelId = r.HotelId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsActive = r.IsActive
                }).ToList();

                return new ApiResponse<List<ReviewDTO>>(true, "Reviews retrieved successfully.", reviewDTOs);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error retrieving reviews for HotelId {HotelId}: {Message}", hotelId, innerMessage);
                return new ApiResponse<List<ReviewDTO>>(false, $"Error retrieving reviews: {innerMessage}");
            }
        }

        public async Task<ApiResponse<ReviewDTO>> UpdateHotelReviewAsync(int reviewId, CreateReviewDTO reviewDto)
        {
            try
            {
                if (reviewId <= 0)
                {
                    _logger.LogWarning("Invalid review ID: {ReviewId}", reviewId);
                    return new ApiResponse<ReviewDTO>(false, "Invalid review ID.");
                }

                if (reviewDto.HotelId <= 0)
                {
                    _logger.LogWarning("Invalid hotel ID: {HotelId}", reviewDto.HotelId);
                    return new ApiResponse<ReviewDTO>(false, "Invalid hotel ID.");
                }

                if (reviewDto.UserId <= 0)
                {
                    _logger.LogWarning("Invalid user ID: {UserId}", reviewDto.UserId);
                    return new ApiResponse<ReviewDTO>(false, "Invalid user ID.");
                }

                if (reviewDto.ReviewType != "Hotel")
                {
                    _logger.LogWarning("Invalid review type: {ReviewType}. Must be 'Hotel'.", reviewDto.ReviewType);
                    return new ApiResponse<ReviewDTO>(false, "Review type must be 'Hotel'.");
                }

                var existingReview = await _reviewRepository.GetReviewByIdAsync(reviewId);
                if (existingReview == null || !existingReview.IsActive)
                {
                    _logger.LogWarning("Review not found or inactive for ReviewId: {ReviewId}", reviewId);
                    return new ApiResponse<ReviewDTO>(false, "Review not found or inactive.");
                }

                if (existingReview.UserId != reviewDto.UserId)
                {
                    _logger.LogWarning("User {UserId} is not authorized to update ReviewId: {ReviewId}", reviewDto.UserId, reviewId);
                    return new ApiResponse<ReviewDTO>(false, "User is not authorized to update this review.");
                }

                var hotel = await _genericRepository.GetByIdAsync(reviewDto.HotelId.Value);
                if (hotel == null || !hotel.IsActive)
                {
                    _logger.LogWarning("Hotel not found or inactive for HotelId: {HotelId}", reviewDto.HotelId);
                    return new ApiResponse<ReviewDTO>(false, "Hotel not found or inactive.");
                }

                var review = new Review
                {
                    ReviewId = reviewId,
                    UserId = reviewDto.UserId,
                    ReviewType = reviewDto.ReviewType,
                    HotelId = reviewDto.HotelId,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    CreatedAt = existingReview.CreatedAt,
                    IsActive = true
                };

                var updated = await _reviewRepository.UpdateReviewAsync(review);
                if (!updated)
                {
                    return new ApiResponse<ReviewDTO>(false, "Failed to update review.");
                }

                var averageRating = await _reviewRepository.CalculateHotelAverageRatingAsync(reviewDto.HotelId.Value);
                hotel.AverageRating = averageRating;
                await _genericRepository.UpdateAsync(hotel);

                var reviewDtoResult = new ReviewDTO
                {
                    ReviewId = review.ReviewId,
                    UserId = review.UserId,
                    ReviewType = review.ReviewType,
                    HotelId = review.HotelId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    IsActive = review.IsActive
                };

                return new ApiResponse<ReviewDTO>(true, "Review updated successfully.", reviewDtoResult);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error updating review with ReviewId {ReviewId}: {Message}", reviewId, innerMessage);
                return new ApiResponse<ReviewDTO>(false, $"Error updating review: {innerMessage}");
            }
        }

        public async Task<ApiResponse<bool>> RemoveHotelReviewAsync(int reviewId)
        {
            try
            {
                if (reviewId <= 0)
                {
                    _logger.LogWarning("Invalid review ID: {ReviewId}", reviewId);
                    return new ApiResponse<bool>(false, "Invalid review ID.");
                }

                var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
                if (review == null || !review.IsActive)
                {
                    _logger.LogWarning("Review not found or inactive for ReviewId: {ReviewId}", reviewId);
                    return new ApiResponse<bool>(false, "Review not found or inactive.");
                }

                var deleted = await _reviewRepository.SoftDeleteReviewAsync(reviewId);
                if (!deleted)
                {
                    return new ApiResponse<bool>(false, "Failed to delete review.");
                }

                var averageRating = await _reviewRepository.CalculateHotelAverageRatingAsync(review.HotelId.Value);
                var hotel = await _genericRepository.GetByIdAsync(review.HotelId.Value);
                if (hotel != null)
                {
                    hotel.AverageRating = averageRating;
                    await _genericRepository.UpdateAsync(hotel);
                }

                return new ApiResponse<bool>(true, "Review deleted successfully.", true);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Error deleting review with ReviewId {ReviewId}: {Message}", reviewId, innerMessage);
                return new ApiResponse<bool>(false, $"Error deleting review: {innerMessage}");
            }
        }

        public Task<bool> SoftDeleteHotelAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}