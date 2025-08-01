using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
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
        private readonly IWebHostEnvironment _environment;

        public HotelServices(
            IRepository<Hotel> genericRepository,
            IHotelRepository hotelRepository,
            IWebHostEnvironment environment)
        {
            _genericRepository = genericRepository;
            _hotelRepository = hotelRepository;
            _environment = environment;
        }

        // GET: api/hotel
        public async Task<ApiResponse<List<HotelDTO>>> GetAllHotelAsync()
        {
            try
            {
                var hotels = await _genericRepository.GetAllAsync();
                var hotelDTOs = new List<HotelDTO>();
                foreach (var hotel in hotels)
                {
                    var roomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(hotel.HotelId)).ToList();
                    var hotelDates = (await _hotelRepository.GetHotelDatesAsync(hotel.HotelId)).ToList();
                    var medias = (await _hotelRepository.GetMediasByHotelIdAsync(hotel.HotelId)).ToList();
                    var reviews = (await _hotelRepository.GetReviewsByHotelIdAsync(hotel.HotelId)).ToList();
                    var packages = (await _hotelRepository.GetPackagesByHotelIdAsync(hotel.HotelId)).ToList();

                    var dto = new HotelDTO
                    {
                        HotelId = hotel.HotelId,
                        Name = hotel.Name,
                        Description = hotel.Description,
                        StarRating = hotel.StarRating,
                        CheckInTime = hotel.CheckInTime,
                        CheckOutTime = hotel.CheckOutTime,
                        ContactPhone = hotel.ContactPhone,
                        ContactEmail = hotel.ContactEmail,
                        IsActive = hotel.IsActive,
                        RoomTypes = roomTypes.Select(rt => new HotelRoomTypeDTO
                        {
                            RoomTypeId = rt.RoomTypeId,
                            Name = rt.Name,
                            Price = rt.Price,
                            Capacity = rt.Capacity,
                            BedType = rt.BedType,
                            IsActive = rt.IsActive
                        }).ToList(),
                        HotelDates = hotelDates.Select(hd => new HotelDateDTO
                        {
                            HotelDateId = hd.HotelDateId,
                            StartDate = hd.StartDate,
                            EndDate = hd.EndDate,
                            AvailableRooms = hd.AvailableRooms,
                            IsActive = hd.IsActive
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
                        AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0

                    };

                    hotelDTOs.Add(dto);
                }

                return new ApiResponse<List<HotelDTO>>(true, "Hotels retrieved successfully.", hotelDTOs);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new ApiResponse<List<HotelDTO>>(false, $"Erro ao buscar hotéis: {innerMessage}");
            }
        }

        // GET: api/hotel/{id}
        public async Task<ApiResponse<HotelDTO>> GetHotelByIdAsync(int id)
        {
            if (id <= 0)
                return new ApiResponse<HotelDTO>(false, "Invalid hotel ID.");

            try
            {
                var hotel = await _genericRepository.GetByIdAsync(id);
                if (hotel == null)
                    return new ApiResponse<HotelDTO>(false, $"Hotel with ID {id} not found.");

                hotel.RoomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(id)).ToList();
                hotel.HotelDates = (await _hotelRepository.GetHotelDatesAsync(id)).ToList();
                hotel.Medias = (await _hotelRepository.GetMediasByHotelIdAsync(id)).ToList();
                hotel.Reviews = (await _hotelRepository.GetReviewsByHotelIdAsync(id)).ToList();
                hotel.Packages = (await _hotelRepository.GetPackagesByHotelIdAsync(id)).ToList();
                hotel.AverageRating = hotel.Reviews.Any() ? hotel.Reviews.Average(r => r.Rating) : 0;

                var hotelDTO = new HotelDTO
                {
                    HotelId = hotel.HotelId,
                    Name = hotel.Name,
                    Description = hotel.Description,
                    StarRating = hotel.StarRating,
                    CheckInTime = hotel.CheckInTime,
                    CheckOutTime = hotel.CheckOutTime,
                    ContactPhone = hotel.ContactPhone,
                    ContactEmail = hotel.ContactEmail,
                    IsActive = hotel.IsActive,
                    RoomTypes = hotel.RoomTypes.Select(rt => new HotelRoomTypeDTO
                    {
                        RoomTypeId = rt.RoomTypeId,
                        Name = rt.Name,
                        Price = rt.Price,
                        Capacity = rt.Capacity,
                        BedType = rt.BedType,
                        IsActive = rt.IsActive
                    }).ToList(),
                    HotelDates = hotel.HotelDates.Select(hd => new HotelDateDTO
                    {
                        HotelDateId = hd.HotelDateId,
                        StartDate = hd.StartDate,
                        EndDate = hd.EndDate,
                        AvailableRooms = hd.AvailableRooms,
                        IsActive = hd.IsActive
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
                    AverageRating = hotel.AverageRating
                };

                return new ApiResponse<HotelDTO>(true, "Hotel retrieved successfully.", hotelDTO);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new ApiResponse<HotelDTO>(false, $"Erro ao buscar hotel: {innerMessage}");
            }
        }

        // POST: api/hotel/create-json
        public async Task<ApiResponse<Hotel>> CreateHotelAsync(CreateHotelDTO createHotelDto)
        {
            if (createHotelDto == null)
                return new ApiResponse<Hotel>(false, "Invalid payload.");

            try
            {
                var hotelExists = await _hotelRepository.NameExistsAsync(createHotelDto.Name);
                if (hotelExists)
                    return new ApiResponse<Hotel>(false, "Hotel name already exists.");

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
                    IsActive = createHotelDto.IsActive,
                    AverageRating = createHotelDto.AverageRating
                };

                var createdHotel = await _genericRepository.AddAsync(hotel);

                // Adicionar datas
                if (createHotelDto.HotelDates != null)
                {
                    foreach (var hdDto in createHotelDto.HotelDates)
                    {
                        var hotelDate = new HotelDate
                        {
                            StartDate = hdDto.StartDate,
                            EndDate = hdDto.EndDate,
                            AvailableRooms = hdDto.AvailableRooms,
                            HotelId = createdHotel.HotelId,
                            IsActive = hdDto.IsActive
                        };
                        await _hotelRepository.AddHotelDateAsync(hotelDate);
                    }
                }

                // Adicionar tipos de quarto
                if (createHotelDto.RoomTypes != null)
                {
                    foreach (var rtDto in createHotelDto.RoomTypes)
                    {
                        var roomType = new HotelRoomType
                        {
                            Name = rtDto.Name,
                            Price = rtDto.Price,
                            Capacity = rtDto.Capacity,
                            BedType = rtDto.BedType,
                            IsActive = rtDto.IsActive,
                            HotelId = createdHotel.HotelId
                        };
                        await _hotelRepository.AddRoomTypeAsync(roomType);
                    }
                }

                // Adicionar pacotes
                if (createHotelDto.Packages != null)
                {
                    foreach (var packageDto in createHotelDto.Packages)
                    {
                        var package = new Package
                        {
                            Name = packageDto.Name,
                            Description = packageDto.Description,
                            BasePrice = packageDto.BasePrice,
                            IsActive = packageDto.IsActive,
                            HotelId = createdHotel.HotelId
                        };
                        await _hotelRepository.AddPackageAsync(package);
                    }
                }

                // Adicionar avaliações
                if (createHotelDto.Reviews != null)
                {
                    foreach (var reviewDto in createHotelDto.Reviews)
                    {
                        var review = new Review
                        {
                            Rating = reviewDto.Rating,
                            Comment = reviewDto.Comment,
                            CreatedAt = DateTime.UtcNow,
                            HotelId = createdHotel.HotelId
                        };
                        await _hotelRepository.AddReviewAsync(review);
                    }
                }

                return new ApiResponse<Hotel>(true, "Hotel criado com sucesso.", createdHotel);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return new ApiResponse<Hotel>(false, $"Erro ao criar hotel: {innerMessage}");
            }
        }

        // POST: api/hotel/upload-media
        public async Task<ApiResponse<object>> UploadHotelMediaAsync(int hotelId, IFormFile[] mediaFiles)
        {
            try
            {
                if (mediaFiles == null || !mediaFiles.Any())
                    return new ApiResponse<object>(false, "No media files submitted.");

                var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Hotel");
                Directory.CreateDirectory(uploadPath);

                foreach (var file in mediaFiles)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(ext) || file.Length > 5 * 1024 * 1024)
                        return new ApiResponse<object>(false, "Invalid media file.");

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    await _hotelRepository.AddMediaAsync(new Media
                    {
                        MediaUrl = $"/Uploads/Hotel/{fileName}",
                        MediaType = file.ContentType,
                        HotelId = hotelId
                    });
                }

                return new ApiResponse<object>(true, "Media uploaded successfully.");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return new ApiResponse<object>(false, $"Erro ao criar media: {innerMessage}");
            }
        }

        public async Task<ApiResponse<object>> UpdateHotelAsync(int id, HotelDTO hotelDto)
        {
            if (hotelDto == null || id != hotelDto.HotelId)
                return new ApiResponse<object>(false, "Dados inválidos.");

            var existingHotel = await _genericRepository.GetByIdAsync(id);
            if (existingHotel == null)
                return new ApiResponse<object>(false, "Hotel não encontrado.");

            existingHotel.Name = hotelDto.Name;
            existingHotel.Description = hotelDto.Description;
            existingHotel.IsActive = hotelDto.IsActive;

            await _genericRepository.UpdateAsync(existingHotel);
            return new ApiResponse<object>(true, "Hotel atualizado com sucesso.");
        }

        public async Task<ApiResponse<object>> DeleteHotelAsync(int id)
        {
            var deleted = await _genericRepository.SoftDeleteAsync(id);
            if (deleted)
                return new ApiResponse<object>(true, "Hotel excluído com sucesso.");
            else
                return new ApiResponse<object>(false, "Hotel não encontrado.");
        }

        public async Task<IEnumerable<Hotel>> GetAllHotelsAsync()
        {
            try
            {
                var hotels = await _genericRepository.GetAllAsync();
                return hotels;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao buscar hotéis: {ex.Message}", ex);
            }
        }

        Task<Hotel?> IHotelServices.GetHotelByIdAsync(int id)
        {
            var hotelTask = _genericRepository.GetByIdAsync(id);
            if (hotelTask == null)
                return Task.FromResult<Hotel?>(null);

            return hotelTask;
        }

        public Task<Hotel> AddHotelAsync(Hotel hotel)
        {
            try
            {
                if (hotel == null)
                    throw new ArgumentNullException(nameof(hotel), "Hotel cannot be null.");
                // Check if hotel with the same name already exists
                var existingHotel = _genericRepository.GetAllAsync().Result.FirstOrDefault(h => h.Name == hotel.Name);
                if (existingHotel != null)
                    throw new InvalidOperationException("Hotel with the same name already exists.");
                return _genericRepository.AddAsync(hotel);

            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao adicionar hotel: {ex.Message}", ex);
            }
        }
        public Task<Hotel> UpdateHotelAsync(Hotel hotel)
        {
            try
            {
                if (hotel == null)
                    throw new ArgumentNullException(nameof(hotel), "Hotel cannot be null.");
                // Check if hotel exists
                var existingHotel = _genericRepository.GetByIdAsync(hotel.HotelId).Result;
                if (existingHotel == null)
                    throw new KeyNotFoundException($"Hotel with ID {hotel.HotelId} not found.");
                // Update hotel properties
                existingHotel.Name = hotel.Name;
                existingHotel.Description = hotel.Description;
                existingHotel.StarRating = hotel.StarRating;
                existingHotel.CheckInTime = hotel.CheckInTime;
                existingHotel.CheckOutTime = hotel.CheckOutTime;
                existingHotel.ContactPhone = hotel.ContactPhone;
                existingHotel.ContactEmail = hotel.ContactEmail;
                existingHotel.IsActive = hotel.IsActive;
                return _genericRepository.UpdateAsync(existingHotel);

            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao atualizar hotel: {ex.Message}", ex);

            }
        }

        public Task<bool> SoftDeleteHotelAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid hotel ID.", nameof(id));

                return _genericRepository.SoftDeleteAsync(id);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao excluir hotel: {ex.Message}", ex);
            }
        }

        public Task<bool> SaveChangesAsync()
        {
            try
            {
                return _genericRepository.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao salvar alterações: {ex.Message}", ex);
            }
        }

        public Task<IEnumerable<MediaDTO>> GetMediaByHotelIdAsync(int hotelId)
        {
            try
            {
                if (hotelId <= 0)
                    throw new ArgumentException("Invalid hotel ID.", nameof(hotelId));

                var mediasTask = _hotelRepository.GetMediasByHotelIdAsync(hotelId);
                if (mediasTask == null)
                    return Task.FromResult<IEnumerable<MediaDTO>>(new List<MediaDTO>());

                var medias = mediasTask.Result; // Await or access the result of the Task
                if (medias == null || !medias.Any())
                    return Task.FromResult<IEnumerable<MediaDTO>>(new List<MediaDTO>());

                return Task.FromResult(medias.Select(m => new MediaDTO
                {
                    MediaId = m.MediaId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType
                }));
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao buscar mídias do hotel: {ex.Message}", ex);
            }
        }

        public Task<MediaDTO?> GetMediaByIdAsync(int mediaId)
        {

            return _hotelRepository.GetMediaByIdAsync(mediaId)
                .ContinueWith(task =>
                {
                    if (task.Result == null)
                        return null;
                    return new MediaDTO
                    {
                        MediaId = task.Result.MediaId,
                        MediaUrl = task.Result.MediaUrl,
                        MediaType = task.Result.MediaType
                    };
                });

        }

        public Task<MediaDTO> AddMediaToHotelAsync(int hotelId, MediaDTO mediaDto)
        {
            try
            {
                if (mediaDto == null)
                    throw new ArgumentNullException(nameof(mediaDto), "Media cannot be null.");

                // Example logic for adding media to a hotel
                var media = new Media
                {
                    MediaUrl = mediaDto.MediaUrl,
                    MediaType = mediaDto.MediaType,
                    HotelId = hotelId
                };

                var addedMedia = _hotelRepository.AddMediaAsync(media).Result;

                return Task.FromResult(new MediaDTO
                {
                    MediaId = addedMedia.MediaId,
                    MediaUrl = addedMedia.MediaUrl,
                    MediaType = addedMedia.MediaType
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao adicionar mídia ao hotel: {ex.Message}", ex);
            }
        }

        public Task<bool> SoftDeleteMediaAsync(int mediaId)
        {
            var mediaTask = _hotelRepository.SoftDeleteMediaAsync(mediaId);
            if (mediaTask == null)
                return Task.FromResult(false);
            return mediaTask;
        }

        public async Task<Hotel> UpdateHotelAsync(UpdateHotelDto updateHotelDto)
        {
            try
            {

                if (updateHotelDto == null)
                    throw new ArgumentNullException(nameof(updateHotelDto), "UpdateHotelDto cannot be null.");
                var existingHotel = await _genericRepository.GetByIdAsync(updateHotelDto.HotelId);
                if (existingHotel == null)
                    throw new KeyNotFoundException($"Hotel with ID {updateHotelDto.HotelId} not found.");
                existingHotel.Name = updateHotelDto.Name;
                existingHotel.Description = updateHotelDto.Description;
                existingHotel.StarRating = updateHotelDto.StarRating;
                existingHotel.CheckInTime = updateHotelDto.CheckInTime;
                existingHotel.CheckOutTime = updateHotelDto.CheckOutTime;
                existingHotel.ContactPhone = updateHotelDto.ContactPhone;
                existingHotel.ContactEmail = updateHotelDto.ContactEmail;
                existingHotel.IsActive = updateHotelDto.IsActive;
                return await _genericRepository.UpdateAsync(existingHotel);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao atualizar hotel: {ex.Message}", ex);
            }

        }
        Task<IEnumerable<Media>> IHotelServices.GetMediaByHotelIdAsync(int hotelId)
        {
            try
            {
                if (hotelId <= 0)
                    throw new ArgumentException("Invalid hotel ID.", nameof(hotelId));
                var mediasTask = _hotelRepository.GetMediasByHotelIdAsync(hotelId);
                if (mediasTask == null)
                    return Task.FromResult<IEnumerable<Media>>(new List<Media>());
                return mediasTask;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao buscar mídias do hotel: {ex.Message}", ex);

            }
        }

        Task<Media?> IHotelServices.GetMediaByIdAsync(int mediaId)
        {
            try
            {
                if (mediaId <= 0)
                    throw new ArgumentException("Invalid media ID.", nameof(mediaId));
                var mediaTask = _hotelRepository.GetMediaByIdAsync(mediaId);
                if (mediaTask == null)
                    return Task.FromResult<Media?>(null);
                return mediaTask;

            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao buscar mídia: {ex.Message}", ex);
            }
        }

        public Task<Media> AddMediaToHotelAsync(Media mediaDto)
        {
            try
            {
                if (mediaDto == null)
                    throw new ArgumentNullException(nameof(mediaDto), "Media cannot be null.");
                // Example logic for adding media to a hotel
                var media = new Media
                {
                    MediaUrl = mediaDto.MediaUrl,
                    MediaType = mediaDto.MediaType,
                    HotelId = mediaDto.HotelId
                };
                return _hotelRepository.AddMediaAsync(media);

            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Erro ao adicionar mídia ao hotel: {ex.Message}", ex);

            }

        }
    }

}