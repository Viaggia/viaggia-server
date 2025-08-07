using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Packages;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Repositories;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly IRepository<Package> _genericRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PackagesController> _logger;

        public PackagesController(
            IRepository<Package> genericRepo,
            IPackageRepository packageRepo,
            IWebHostEnvironment environment,
            ILogger<PackagesController> logger)
        {
            _genericRepository = genericRepo ?? throw new ArgumentNullException(nameof(genericRepo));
            _packageRepository = packageRepo ?? throw new ArgumentNullException(nameof(packageRepo));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize(Roles = "SERVICE_PROVIDER,ADMIN, ATTENDANT,CLIENT")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPackages()
        {
            try
            {
                var packages = await _genericRepository.GetAllAsync();
                var packageDTOs = new List<PackageDTO>();
                foreach (var p in packages)
                {
                    // Load related data (Medias and PackageDates)
                    p.Medias = (await _packageRepository.GetPackageMediasAsync(p.PackageId)).ToList();
                    p.PackageDates = (await _packageRepository.GetPackageDatesAsync(p.PackageId)).ToList();
                    var hotel = await _genericRepository.GetByIdAsync<Hotel>(p.HotelId);
                    packageDTOs.Add(new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Destination = p.Destination,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        HotelId = p.HotelId,
                        HotelName = hotel?.Name ?? string.Empty,
                        IsActive = p.IsActive,
                        Medias = p.Medias.Select(m => new MediaDTO
                        {
                            MediaId = m.MediaId,
                            MediaUrl = m.MediaUrl,
                            MediaType = m.MediaType
                        }).Distinct().ToList(),
                        PackageDates = p.PackageDates.Select(pd => new PackageDateDTO
                        {
                            PackageDateId = pd.PackageDateId,
                            StartDate = pd.StartDate.ToString("dd/MM/yyyy"),
                            EndDate = pd.EndDate.ToString("dd/MM/yyyy")
                        }).Distinct().ToList()
                    });
                }
                return Ok(new ApiResponse<List<PackageDTO>>(true, "Packages retrieved successfully.", packageDTOs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving packages");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<PackageDTO>>(false, $"Error retrieving packages: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SERVICE_PROVIDER,ADMIN, ATTENDANT")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPackageById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<PackageDTO>(false, "Invalid package ID."));

            try
            {
                var package = await _genericRepository.GetByIdAsync(id);
                if (package == null)
                    return NotFound(new ApiResponse<PackageDTO>(false, $"Package with ID {id} not found."));

                package.Medias = (await _packageRepository.GetPackageMediasAsync(id)).ToList();
                package.PackageDates = (await _packageRepository.GetPackageDatesAsync(id)).ToList();
                var hotel = await _genericRepository.GetByIdAsync<Hotel>(package.HotelId);

                var packageDTO = new PackageDTO
                {
                    PackageId = package.PackageId,
                    Name = package.Name,
                    Destination = package.Destination,
                    Description = package.Description,
                    BasePrice = package.BasePrice,
                    HotelId = package.HotelId,
                    HotelName = hotel?.Name ?? string.Empty,
                    IsActive = package.IsActive,
                    Medias = package.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).Distinct().ToList(),
                    PackageDates = package.PackageDates.Select(pd => new PackageDateDTO
                    {
                        PackageDateId = pd.PackageDateId,
                        StartDate = pd.StartDate.ToString("dd/MM/yyyy"),
                        EndDate = pd.EndDate.ToString("dd/MM/yyyy")
                    }).Distinct().ToList()
                };
                return Ok(new ApiResponse<PackageDTO>(true, "Package retrieved successfully.", packageDTO));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving package with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PackageDTO>(false, $"Error retrieving package: {ex.Message}"));
            }
        }

        [HttpGet("my-packages")]
        [Authorize(Roles = "SERVICE_PROVIDER,ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyPackages()
        {
            try
            {
                // Get authenticated user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogError("User not authenticated for retrieving packages.");
                    return Unauthorized(new ApiResponse<List<PackageDTO>>(false, "User not authenticated."));
                }

                var packages = await _packageRepository.GetByUserIdAsync(userId);
                var packageDTOs = new List<PackageDTO>();
                foreach (var p in packages)
                {
                    var hotel = await _genericRepository.GetByIdAsync<Hotel>(p.HotelId);
                    packageDTOs.Add(new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Destination = p.Destination,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        HotelId = p.HotelId,
                        HotelName = hotel?.Name ?? string.Empty,
                        IsActive = p.IsActive,
                        Medias = p.Medias.Select(m => new MediaDTO
                        {
                            MediaId = m.MediaId,
                            MediaUrl = m.MediaUrl,
                            MediaType = m.MediaType
                        }).Distinct().ToList(),
                        PackageDates = p.PackageDates.Select(pd => new PackageDateDTO
                        {
                            PackageDateId = pd.PackageDateId,
                            StartDate = pd.StartDate.ToString("dd/MM/yyyy"),
                            EndDate = pd.EndDate.ToString("dd/MM/yyyy")
                        }).Distinct().ToList()
                    });
                }

                _logger.LogInformation("Retrieved {Count} packages for UserId {UserId}", packageDTOs.Count, userId);
                return Ok(new ApiResponse<List<PackageDTO>>(true, "User packages retrieved successfully.", packageDTOs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving packages for UserId {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<PackageDTO>>(false, $"Error retrieving packages: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "SERVICE_PROVIDER,ADMIN")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePackage([FromForm] PackageCreateDTO packageDTO)
        {
            if (packageDTO == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Invalid package data: {Errors}", string.Join(", ", errors));
                return BadRequest(new ApiResponse<PackageDTO>(false, "Invalid package data.", null, errors));
            }

            if (packageDTO.BasePrice <= 0)
            {
                _logger.LogWarning("BasePrice must be greater than 0.");
                return BadRequest(new ApiResponse<PackageDTO>(false, "BasePrice must be greater than 0."));
            }

            try
            {
                // Get authenticated user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogError("User not authenticated for package creation.");
                    return Unauthorized(new ApiResponse<PackageDTO>(false, "User not authenticated."));
                }

                _logger.LogInformation("Received StartDate: {StartDate}, EndDate: {EndDate}", packageDTO.StartDate, packageDTO.EndDate);

                // Validate file types and sizes
                if (packageDTO.MediaFiles != null && packageDTO.MediaFiles.Any())
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    foreach (var file in packageDTO.MediaFiles)
                    {
                        if (file.Length > 5 * 1024 * 1024) // 5MB limit
                            return BadRequest(new ApiResponse<PackageDTO>(false, "File size exceeds 5MB."));
                        var extension = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                            return BadRequest(new ApiResponse<PackageDTO>(false, $"Invalid file type: {extension}. Allowed types: {string.Join(", ", allowedExtensions)}"));
                    }
                }

                var hotelId = await _packageRepository.GetHotelIdByNameAsync(packageDTO.HotelName);
                if (hotelId == null)
                    return BadRequest(new ApiResponse<PackageDTO>(false, $"Hotel with name {packageDTO.HotelName} not found or inactive."));

                var hotel = await _genericRepository.GetByIdAsync<Hotel>(hotelId.Value);
                if (hotel == null || !hotel.IsActive)
                    return BadRequest(new ApiResponse<PackageDTO>(false, $"Hotel with name {packageDTO.HotelName} not found or inactive."));

                // Validate and parse dates
                if (!DateTime.TryParseExact(packageDTO.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                {
                    _logger.LogWarning("Invalid StartDate format: {StartDate}", packageDTO.StartDate);
                    return BadRequest(new ApiResponse<PackageDTO>(false, $"Invalid StartDate format: {packageDTO.StartDate}. Expected DD/MM/YYYY."));
                }
                if (!DateTime.TryParseExact(packageDTO.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    _logger.LogWarning("Invalid EndDate format: {EndDate}", packageDTO.EndDate);
                    return BadRequest(new ApiResponse<PackageDTO>(false, $"Invalid EndDate format: {packageDTO.EndDate}. Expected DD/MM/YYYY."));
                }
                if (endDate < startDate)
                {
                    _logger.LogWarning("Invalid date range: StartDate={StartDate}, EndDate={EndDate}", startDate, endDate);
                    return BadRequest(new ApiResponse<PackageDTO>(false, "EndDate must be greater than or equal to StartDate."));
                }

                var package = new Package
                {
                    Name = packageDTO.Name,
                    Destination = packageDTO.Destination,
                    Description = packageDTO.Description,
                    BasePrice = packageDTO.BasePrice,
                    HotelId = hotelId.Value,
                    UserId = userId, // Set creator
                    IsActive = packageDTO.IsActive,
                    PackageDates = new List<PackageDate>(),
                    Medias = new List<Media>()
                };

                // Add package to context
                await _genericRepository.AddAsync(package);
                await _genericRepository.SaveChangesAsync();

                // Add single PackageDate
                var packageDate = new PackageDate
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PackageId = package.PackageId,
                    IsActive = true
                };
                await _packageRepository.AddPackageDateAsync(packageDate);

                // Process file uploads
                var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Packages");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in packageDTO.MediaFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var media = new Media
                        {
                            MediaUrl = $"/Uploads/Packages/{fileName}",
                            MediaType = file.ContentType,
                            PackageId = package.PackageId,
                            IsActive = true
                        };
                        await _packageRepository.AddMediaAsync(media);
                    }
                }

                var resultDTO = new PackageDTO
                {
                    PackageId = package.PackageId,
                    Name = package.Name,
                    Destination = package.Destination,
                    Description = package.Description,
                    BasePrice = package.BasePrice,
                    HotelId = package.HotelId,
                    HotelName = hotel.Name,
                    IsActive = package.IsActive,
                    Medias = package.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).Distinct().ToList(),
                    PackageDates = package.PackageDates.Select(pd => new PackageDateDTO
                    {
                        PackageDateId = pd.PackageDateId,
                        StartDate = pd.StartDate.ToString("dd/MM/yyyy"),
                        EndDate = pd.EndDate.ToString("dd/MM/yyyy")
                    }).Distinct().ToList()
                };

                _logger.LogInformation("Package created by UserId {UserId}: PackageId {PackageId}", userId, package.PackageId);
                return CreatedAtAction(nameof(GetPackageById), new { id = package.PackageId },
                    new ApiResponse<PackageDTO>(true, "Package created successfully.", resultDTO));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating package for UserId {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PackageDTO>(false, $"Error creating package: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SERVICE_PROVIDER,ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePackage(int id, [FromForm] PackageUpdateDTO packageDTO)
        {
            if (id <= 0 || packageDTO == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Invalid package data for ID {Id}: {Errors}", id, string.Join(", ", errors));
                return BadRequest(new ApiResponse<PackageDTO>(false, "Invalid package data or ID.", null, errors));
            }

            if (packageDTO.BasePrice <= 0)
            {
                _logger.LogWarning("BasePrice must be greater than 0.");
                return BadRequest(new ApiResponse<PackageDTO>(false, "BasePrice must be greater than 0."));
            }

            try
            {
                // Get authenticated user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogError("User not authenticated for updating package ID {Id}", id);
                    return Unauthorized(new ApiResponse<PackageDTO>(false, "User not authenticated."));
                }

                var package = await _genericRepository.GetByIdAsync(id);
                if (package == null)
                    return NotFound(new ApiResponse<PackageDTO>(false, $"Package with ID {id} not found."));

                // Check if user owns the package
                if (package.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to update package ID {Id} owned by UserId {OwnerId}", userId, id, package.UserId);
                    return Forbid();
                }

                var hotelId = await _packageRepository.GetHotelIdByNameAsync(packageDTO.HotelName);
                if (hotelId == null)
                    return BadRequest(new ApiResponse<PackageDTO>(false, $"Hotel with name {packageDTO.HotelName} not found or inactive."));

                var hotel = await _genericRepository.GetByIdAsync<Hotel>(hotelId.Value);
                if (hotel == null || !hotel.IsActive)
                    return BadRequest(new ApiResponse<PackageDTO>(false, $"Hotel with name {packageDTO.HotelName} not found or inactive."));

                package.Medias = (await _packageRepository.GetPackageMediasAsync(id)).ToList();
                package.PackageDates = (await _packageRepository.GetPackageDatesAsync(id)).ToList();

                // Update package fields
                package.Name = packageDTO.Name;
                package.Destination = packageDTO.Destination;
                package.Description = packageDTO.Description;
                package.BasePrice = packageDTO.BasePrice;
                package.HotelId = hotelId.Value;
                package.IsActive = packageDTO.IsActive;

                // Update PackageDates only if both StartDate and EndDate are provided
                if (!string.IsNullOrWhiteSpace(packageDTO.StartDate) && !string.IsNullOrWhiteSpace(packageDTO.EndDate))
                {
                    if (!DateTime.TryParseExact(packageDTO.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                    {
                        _logger.LogWarning("Invalid StartDate format: {StartDate}", packageDTO.StartDate);
                        return BadRequest(new ApiResponse<PackageDTO>(false, $"Invalid StartDate format: {packageDTO.StartDate}. Expected DD/MM/YYYY."));
                    }
                    if (!DateTime.TryParseExact(packageDTO.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                    {
                        _logger.LogWarning("Invalid EndDate format: {EndDate}", packageDTO.EndDate);
                        return BadRequest(new ApiResponse<PackageDTO>(false, $"Invalid EndDate format: {packageDTO.EndDate}. Expected DD/MM/YYYY."));
                    }
                    if (endDate < startDate)
                    {
                        _logger.LogWarning("Invalid date range: StartDate={StartDate}, EndDate={EndDate}", startDate, endDate);
                        return BadRequest(new ApiResponse<PackageDTO>(false, "EndDate must be greater than or equal to StartDate."));
                    }

                    // Soft-delete existing PackageDates
                    foreach (var date in package.PackageDates.ToList())
                    {
                        await _genericRepository.SoftDeleteAsync<PackageDate>(date.PackageDateId);
                        _logger.LogInformation("Soft-deleted PackageDate: PackageDateId={PackageDateId}", date.PackageDateId);
                    }

                    // Add new PackageDate
                    var packageDate = new PackageDate
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        PackageId = package.PackageId,
                        IsActive = true
                    };
                    await _packageRepository.AddPackageDateAsync(packageDate);
                    package.PackageDates.Clear();
                    package.PackageDates.Add(packageDate);
                    _logger.LogInformation("Added PackageDate for update: PackageId={PackageId}, StartDate={StartDate}, EndDate={EndDate}", package.PackageId, startDate, endDate);
                }
                else if (string.IsNullOrWhiteSpace(packageDTO.StartDate) != string.IsNullOrWhiteSpace(packageDTO.EndDate))
                {
                    _logger.LogWarning("Both StartDate and EndDate must be provided or both must be empty.");
                    return BadRequest(new ApiResponse<PackageDTO>(false, "Both StartDate and EndDate must be provided or both must be empty."));
                }

                // Delete specified medias
                foreach (var mediaId in packageDTO.MediaIdsToDelete)
                {
                    await _packageRepository.DeleteMediaAsync(mediaId);
                    _logger.LogInformation("Deleted Media: MediaId={MediaId}", mediaId);
                }

                // Add new medias
                var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Packages");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in packageDTO.NewMediaFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var media = new Media
                        {
                            MediaUrl = $"/Uploads/Packages/{fileName}",
                            MediaType = file.ContentType,
                            PackageId = package.PackageId,
                            IsActive = true
                        };
                        await _packageRepository.AddMediaAsync(media);
                        package.Medias.Add(media);
                        _logger.LogInformation("Added Media for update: PackageId={PackageId}, MediaUrl={MediaUrl}", package.PackageId, media.MediaUrl);
                    }
                }

                await _genericRepository.UpdateAsync(package);
                await _genericRepository.SaveChangesAsync();

                var resultDTO = new PackageDTO
                {
                    PackageId = package.PackageId,
                    Name = package.Name,
                    Destination = package.Destination,
                    Description = package.Description,
                    BasePrice = package.BasePrice,
                    HotelId = package.HotelId,
                    HotelName = hotel.Name,
                    IsActive = package.IsActive,
                    Medias = package.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).Distinct().ToList(),
                    PackageDates = package.PackageDates.Select(pd => new PackageDateDTO
                    {
                        PackageDateId = pd.PackageDateId,
                        StartDate = pd.StartDate.ToString("dd/MM/yyyy"),
                        EndDate = pd.EndDate.ToString("dd/MM/yyyy")
                    }).Distinct().ToList()
                };

                _logger.LogInformation("Package ID {Id} updated by UserId {UserId}", id, userId);
                return Ok(new ApiResponse<PackageDTO>(true, "Package updated successfully.", resultDTO));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating package ID {Id} for UserId {UserId}", id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PackageDTO>(false, $"Error updating package: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SERVICE_PROVIDER,ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeletePackage(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(false, "Invalid package ID."));

            try
            {
                // Get authenticated user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogError("User not authenticated for deleting package ID {Id}", id);
                    return Unauthorized(new ApiResponse<object>(false, "User not authenticated."));
                }

                var package = await _genericRepository.GetByIdAsync(id);
                if (package == null)
                    return NotFound(new ApiResponse<object>(false, $"Package with ID {id} not found."));

                // Check if user owns the package
                if (package.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to delete package ID {Id} owned by UserId {OwnerId}", userId, id, package.UserId);
                    return Forbid();
                }

                var deleted = await _genericRepository.SoftDeleteAsync(id);
                if (!deleted)
                    return NotFound(new ApiResponse<object>(false, $"Package with ID {id} not found."));

                await _genericRepository.SaveChangesAsync();
                _logger.LogInformation("Soft-deleted package ID {Id} by UserId {UserId}", id, userId);
                return Ok(new ApiResponse<object>(true, $"Package with ID {id} was deactivated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting package ID {Id} for UserId {UserId}", id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>(false, $"Error deleting package: {ex.Message}"));
            }
        }

        [HttpPatch("{id}/reactivate")]
        [Authorize(Roles = "SERVICE_PROVIDER,ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReactivatePackage(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(false, "Invalid package ID."));

            try
            {
                var reactivated = await _packageRepository.ReactivateAsync(id);
                if (!reactivated)
                    return NotFound(new ApiResponse<object>(false, $"Package with ID {id} not found."));

                _logger.LogInformation("Reactivated package with ID {Id}", id);
                return Ok(new ApiResponse<object>(true, $"Package with ID {id} was reactivated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating package with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>(false, $"Error reactivating package: {ex.Message}"));
            }
        }

        [HttpGet("{packageId}/dates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPackageDates(int packageId)
        {
            if (packageId <= 0)
                return BadRequest(new ApiResponse<List<PackageDateDTO>>(false, "Invalid package ID."));

            try
            {
                var package = await _genericRepository.GetByIdAsync(packageId);
                if (package == null)
                    return NotFound(new ApiResponse<List<PackageDateDTO>>(false, $"Package with ID {packageId} not found."));

                var dates = await _packageRepository.GetPackageDatesAsync(packageId);
                var dateDTOs = dates.Select(pd => new PackageDateDTO
                {
                    PackageDateId = pd.PackageDateId,
                    StartDate = pd.StartDate.ToString("dd/MM/yyyy"),
                    EndDate = pd.EndDate.ToString("dd/MM/yyyy")
                }).Distinct().ToList();
                _logger.LogInformation("Retrieved {Count} dates for PackageId {PackageId}", dateDTOs.Count, packageId);
                return Ok(new ApiResponse<List<PackageDateDTO>>(true, "Package dates retrieved successfully.", dateDTOs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving package dates for PackageId {PackageId}", packageId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<PackageDateDTO>>(false, $"Error retrieving package dates: {ex.Message}"));
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchPackages([FromQuery] string destination, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            if (string.IsNullOrWhiteSpace(destination) || string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                return BadRequest(new ApiResponse<List<PackageDTO>>(false, "Invalid destination or date range."));

            if (!DateTime.TryParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStartDate))
                return BadRequest(new ApiResponse<List<PackageDTO>>(false, $"Invalid startDate format: {startDate}. Expected DD/MM/YYYY."));
            if (!DateTime.TryParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
                return BadRequest(new ApiResponse<List<PackageDTO>>(false, $"Invalid endDate format: {endDate}. Expected DD/MM/YYYY."));
            if (parsedEndDate < parsedStartDate)
                return BadRequest(new ApiResponse<List<PackageDTO>>(false, "endDate must be greater than or equal to startDate."));

            try
            {
                var packages = await _packageRepository.SearchPackagesByDestinationAndDateAsync(destination, parsedStartDate, parsedEndDate);
                var packageDTOs = new List<PackageDTO>();
                foreach (var p in packages)
                {
                    var hotel = await _genericRepository.GetByIdAsync<Hotel>(p.HotelId);
                    packageDTOs.Add(new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Destination = p.Destination,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        HotelId = p.HotelId,
                        HotelName = hotel?.Name ?? string.Empty,
                        IsActive = p.IsActive,
                        Medias = p.Medias.Select(m => new MediaDTO
                        {
                            MediaId = m.MediaId,
                            MediaUrl = m.MediaUrl,
                            MediaType = m.MediaType
                        }).Distinct().ToList(),
                        PackageDates = p.PackageDates.Select(pd => new PackageDateDTO
                        {
                            PackageDateId = pd.PackageDateId,
                            StartDate = pd.StartDate.ToString("dd/MM/yyyy"),
                            EndDate = pd.EndDate.ToString("dd/MM/yyyy")
                        }).Distinct().ToList()
                    });
                }
                _logger.LogInformation("Retrieved {Count} packages for destination {Destination} and date range {StartDate} to {EndDate}", packageDTOs.Count, destination, parsedStartDate, parsedEndDate);
                return Ok(new ApiResponse<List<PackageDTO>>(true, "Packages retrieved successfully.", packageDTOs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving packages for destination {Destination}", destination);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<PackageDTO>>(false, $"Error retrieving packages: {ex.Message}"));
            }
        }
    }
}