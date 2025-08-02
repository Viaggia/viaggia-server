using System.Text.Json;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.HotelFilterDTO;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.RoomTypeEnums;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;

namespace viaggia_server.Services.HotelServices
{
    public class HotelServices : IHotelServices
    {
        private readonly IRepository<Hotel> _genericRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<HotelServices> _logger;

        public HotelServices(
            IRepository<Hotel> genericRepository,
            IHotelRepository hotelRepository,
            IWebHostEnvironment environment,
            ILogger<HotelServices> logger)
        {
            _genericRepository = genericRepository;
            _hotelRepository = hotelRepository;
            _environment = environment;
            _logger = logger;
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

                // Add Media
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

                // Add Room Types
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


                // Fetch the created hotel with related data
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

        // Outros métodos permanecem iguais
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
                    var reviews = (await _hotelRepository.GetReviewsByHotelIdAsync(hotel.HotelId)).ToList();
                    var packages = (await _hotelRepository.GetPackagesByHotelIdAsync(hotel.HotelId)).ToList();
                    var commodities = (await _hotelRepository.GetCommoditiesByHotelIdAsync(hotel.HotelId)).ToList();
                    var commoditieServices = (await _hotelRepository.GetCommoditieServicesByHotelIdAsync(hotel.HotelId)).ToList();

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
                            Rating = r.Rating,
                            Comment = r.Comment,
                            CreatedAt = r.CreatedAt
                        }).ToList(),
                        Packages = packages.Select(p => new PackageDTO
                        {
                            PackageId = p.PackageId,
                            Name = p.Name,
                            Description = p.Description,
                            BasePrice = p.BasePrice,
                            IsActive = p.IsActive
                        }).ToList(),
                        Commodities = commodities.Select(c => new CommoditieDTO
                        {
                            HotelId = c.HotelId,
                            HasParking = c.HasParking,
                            IsParkingPaid = c.IsParkingPaid,
                            HasBreakfast = c.HasBreakfast,
                            IsBreakfastPaid = c.IsBreakfastPaid,
                            HasLunch = c.HasLunch,
                            IsLunchPaid = c.IsLunchPaid,
                            HasDinner = c.HasDinner,
                            IsDinnerPaid = c.IsDinnerPaid,
                            HasSpa = c.HasSpa,
                            IsSpaPaid = c.IsSpaPaid,
                            HasPool = c.HasPool,
                            IsPoolPaid = c.IsPoolPaid,
                            HasGym = c.HasGym,
                            IsGymPaid = c.IsGymPaid,
                            HasWiFi = c.HasWiFi,
                            IsWiFiPaid = c.IsWiFiPaid,
                            HasAirConditioning = c.HasAirConditioning,
                            IsAirConditioningPaid = c.IsAirConditioningPaid,
                            HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                            IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                            IsPetFriendly = c.IsPetFriendly,
                            IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                            IsActive = c.IsActive
                        }).ToList(),
                        CommoditieServices = commoditieServices.Select(cs => new CommoditieServicesDTO
                        {
                            Name = cs.Name,
                            IsPaid = cs.IsPaid,
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
                var reviews = (await _hotelRepository.GetReviewsByHotelIdAsync(hotel.HotelId)).ToList();
                var packages = (await _hotelRepository.GetPackagesByHotelIdAsync(hotel.HotelId)).ToList();
                var commodities = (await _hotelRepository.GetCommoditiesByHotelIdAsync(hotel.HotelId)).ToList();
                var commoditieServices = (await _hotelRepository.GetCommoditieServicesByHotelIdAsync(hotel.HotelId)).ToList();

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
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    }).ToList(),
                    Packages = packages.Select(p => new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        IsActive = p.IsActive
                    }).ToList(),
                    Commodities = commodities.Select(c => new CommoditieDTO
                    {
                        HotelId = c.HotelId,
                        HasParking = c.HasParking,
                        IsParkingPaid = c.IsParkingPaid,
                        HasBreakfast = c.HasBreakfast,
                        IsBreakfastPaid = c.IsBreakfastPaid,
                        HasLunch = c.HasLunch,
                        IsLunchPaid = c.IsLunchPaid,
                        HasDinner = c.HasDinner,
                        IsDinnerPaid = c.IsDinnerPaid,
                        HasSpa = c.HasSpa,
                        IsSpaPaid = c.IsSpaPaid,
                        HasPool = c.HasPool,
                        IsPoolPaid = c.IsPoolPaid,
                        HasGym = c.HasGym,
                        IsGymPaid = c.IsGymPaid,
                        HasWiFi = c.HasWiFi,
                        IsWiFiPaid = c.IsWiFiPaid,
                        HasAirConditioning = c.HasAirConditioning,
                        IsAirConditioningPaid = c.IsAirConditioningPaid,
                        HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                        IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                        IsPetFriendly = c.IsPetFriendly,
                        IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                        IsActive = c.IsActive
                    }).ToList(),
                    CommoditieServices = commoditieServices.Select(cs => new CommoditieServicesDTO
                    {
                        Name = cs.Name,
                        IsPaid = cs.IsPaid,
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

        public async Task<Hotel> UpdateHotelAsync(UpdateHotelDto updateHotelDto)
        {
            var hotel = await _genericRepository.GetByIdAsync(updateHotelDto.HotelId);
            if (hotel == null)
                throw new Exception("Hotel not found.");

            hotel.Name = updateHotelDto.Name ?? hotel.Name;
            hotel.Cnpj = updateHotelDto.Cnpj ?? hotel.Cnpj;
            hotel.Street = updateHotelDto.Street ?? hotel.Street;
            hotel.City = updateHotelDto.City ?? hotel.City;
            hotel.State = updateHotelDto.State ?? hotel.State;
            hotel.ZipCode = updateHotelDto.ZipCode ?? hotel.ZipCode;
            hotel.Description = updateHotelDto.Description ?? hotel.Description;
            hotel.CheckInTime = updateHotelDto.CheckInTime ?? hotel.CheckInTime;
            hotel.CheckOutTime = updateHotelDto.CheckOutTime ?? hotel.CheckOutTime;
            hotel.ContactPhone = updateHotelDto.ContactPhone ?? hotel.ContactPhone;
            hotel.ContactEmail = updateHotelDto.ContactEmail ?? hotel.ContactEmail;
            hotel.StarRating = updateHotelDto.StarRating != 0 ? updateHotelDto.StarRating : hotel.StarRating;
            hotel.IsActive = updateHotelDto.IsActive;

            return await _genericRepository.UpdateAsync(hotel);
        }

        public async Task<bool> SoftDeleteHotelAsync(int id)
        {
            var hotel = await _genericRepository.GetByIdAsync(id);
            if (hotel == null)
                return false;

            hotel.IsActive = false;
            await _genericRepository.UpdateAsync(hotel);
            return true;
        }

        public async Task<IEnumerable<Media>> GetMediaByHotelIdAsync(int hotelId)
        {
            return await _hotelRepository.GetMediasByHotelIdAsync(hotelId);
        }

        //public async Task<Media?> GetMediaByIdAsync(int mediaId)
        //{
        //    return await _hotelRepository.GetMediaByIdAsync(mediaId);
        //}

        public async Task<Media> AddMediaToHotelAsync(Media media)
        {
            return await _hotelRepository.AddMediaAsync(media);
        }

        //public async Task<bool> SoftDeleteMediaAsync(int mediaId)
        //{
        //    return await _hotelRepository.SoftDeleteMediaAsync(mediaId);
        //}

        public async Task<ApiResponse<double>> GetHotelAverageRatingAsync(int hotelId)
        {
            try
            {
                var reviews = await _hotelRepository.GetReviewsByHotelIdAsync(hotelId);
                if (!reviews.Any())
                    return new ApiResponse<double>(true, "No reviews found.", 0);

                var averageRating = reviews.Average(r => r.Rating);
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
                _logger.LogInformation("Filtering hotels with Commodities: {Commodities}, CommoditieServices: {CommoditieServices}, RoomTypes: {RoomTypes}",
                    string.Join(", ", filter.Commodities), string.Join(", ", filter.CommoditieServices), string.Join(", ", filter.RoomTypes));

                var hotels = await _hotelRepository.GetHotelsWithRelatedDataAsync();
                var filteredHotels = hotels.AsQueryable();

                // Filter by Commodities (all specified commodities must be true)
                if (filter.Commodities != null && filter.Commodities.Any())
                {
                    foreach (var commodity in filter.Commodities)
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

                // Filter by CommoditieServices (all specified services must exist)
                if (filter.CommoditieServices != null && filter.CommoditieServices.Any())
                {
                    foreach (var service in filter.CommoditieServices)
                    {
                        filteredHotels = filteredHotels.Where(h => h.CommoditieServices.Any(cs => cs.IsActive && cs.Name.ToLower() == service.ToLower()));
                    }
                }

                // Filter by RoomTypes (all specified room types must exist)
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

                var hotelDTOs = filteredHotels.Select(hotel => new HotelDTO
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
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    }).ToList(),
                    Packages = hotel.Packages.Select(p => new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        IsActive = p.IsActive
                    }).ToList(),
                    Commodities = hotel.Commodities.Select(c => new CommoditieDTO
                    {
                        HotelId = c.HotelId,
                        HasParking = c.HasParking,
                        IsParkingPaid = c.IsParkingPaid,
                        HasBreakfast = c.HasBreakfast,
                        IsBreakfastPaid = c.IsBreakfastPaid,
                        HasLunch = c.HasLunch,
                        IsLunchPaid = c.IsLunchPaid,
                        HasDinner = c.HasDinner,
                        IsDinnerPaid = c.IsDinnerPaid,
                        HasSpa = c.HasSpa,
                        IsSpaPaid = c.IsSpaPaid,
                        HasPool = c.HasPool,
                        IsPoolPaid = c.IsPoolPaid,
                        HasGym = c.HasGym,
                        IsGymPaid = c.IsGymPaid,
                        HasWiFi = c.HasWiFi,
                        IsWiFiPaid = c.IsWiFiPaid,
                        HasAirConditioning = c.HasAirConditioning,
                        IsAirConditioningPaid = c.IsAirConditioningPaid,
                        HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                        IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                        IsPetFriendly = c.IsPetFriendly,
                        IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                        IsActive = c.IsActive
                    }).ToList(),
                    CommoditieServices = hotel.CommoditieServices.Select(cs => new CommoditieServicesDTO
                    {
                        Name = cs.Name,
                        IsPaid = cs.IsPaid,
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
    }
}