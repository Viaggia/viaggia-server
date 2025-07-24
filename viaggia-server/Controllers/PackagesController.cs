using Microsoft.AspNetCore.Mvc;
using viaggia_server.Data;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Packages;
using viaggia_server.Models;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Repositories;
using ViaggiaServer.Models.Packages;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly IRepository<Package> _genericRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IWebHostEnvironment _environment;

        public PackagesController(
            IRepository<Package> genericRepo,
            IPackageRepository packageRepo,
            IWebHostEnvironment environment)
        {
            _genericRepository = genericRepo ?? throw new ArgumentNullException(nameof(genericRepo));
            _packageRepository = packageRepo ?? throw new ArgumentNullException(nameof(packageRepo));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Retrieves all active packages.
        /// </summary>
        /// <returns>A list of active packages.</returns>
        /// <response code="200">Returns the list of packages.</response>
        /// <response code="500">If an error occurs while retrieving packages.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPackages()
        {
            try
            {
                var packages = await _genericRepository.GetAllAsync();
                var packageDTOs = packages.Select(p => new PackageDTO
                {
                    PackageId = p.PackageId,
                    Name = p.Name,
                    Destination = p.Destination,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    IsActive = p.IsActive,
                    Medias = p.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl ?? string.Empty, // "??" = possible null reference
                        MediaType = m.MediaType ?? string.Empty //  "??" = possible null reference
                    }).ToList(),
                    PackageDates = p.PackageDates.Select(pd => new PackageDateDTO
                    {
                        PackageDateId = pd.PackageDateId,
                        StartDate = pd.StartDate,
                        EndDate = pd.EndDate
                    }).ToList()
                }).ToList();
                return Ok(new ApiResponse(true, "Packages retrieved successfully.", packageDTOs));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error retrieving packages: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves an active package by its ID.
        /// </summary>
        /// <param name="id">The ID of the package.</param>
        /// <returns>The package with the specified ID.</returns>
        /// <response code="200">Returns the package.</response>
        /// <response code="400">If the ID is invalid.</response>
        /// <response code="404">If the package is not found.</response>
        /// <response code="500">If an error occurs while retrieving the package.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPackageById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse(false, "Invalid package ID."));

            try
            {
                var package = await _genericRepository.GetByIdAsync(id);
                if (package == null)
                    return NotFound(new ApiResponse(false, $"Package with ID {id} not found."));

                // Carregar relacionamentos manualmente
                package.Medias = (await _packageRepository.GetPackageMediasAsync(id)).ToList();
                package.PackageDates = (await _packageRepository.GetPackageDatesAsync(id)).ToList();

                var packageDTO = new PackageDTO
                {
                    PackageId = package.PackageId,
                    Name = package.Name,
                    Destination = package.Destination,
                    Description = package.Description,
                    BasePrice = package.BasePrice,
                    IsActive = package.IsActive,
                    Medias = package.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl ?? string.Empty,
                        MediaType = m.MediaType ?? string.Empty 
                    }).ToList(),
                    PackageDates = package.PackageDates.Select(pd => new PackageDateDTO
                    {
                        PackageDateId = pd.PackageDateId,
                        StartDate = pd.StartDate,
                        EndDate = pd.EndDate
                    }).ToList()
                };
                return Ok(new ApiResponse(true, "Package retrieved successfully.", packageDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error retrieving package: {ex.Message}"));
            }
        }

        /// <summary>
        /// Creates a new package with media files.
        /// </summary>
        /// <param name="packageDTO">The package data with media files.</param>
        /// <returns>The created package.</returns>
        /// <response code="201">Returns the newly created package.</response>
        /// <response code="400">If the package data is invalid.</response>
        /// <response code="500">If an error occurs while creating the package.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePackage([FromForm] PackageCreateDTO packageDTO)
        {
            if (packageDTO == null || !ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid package data.", ModelState));

            try
            {
                var package = new Package
                {
                    Name = packageDTO.Name,
                    Destination = packageDTO.Destination,
                    Description = packageDTO.Description,
                    BasePrice = packageDTO.BasePrice,
                    IsActive = packageDTO.IsActive,
                    PackageDates = packageDTO.PackageDates.Select(pd => new PackageDate
                    {
                        StartDate = pd.StartDate,
                        EndDate = pd.EndDate
                    }).ToList()
                };

                // Salvar imagens localmente
                var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Packages");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in packageDTO.MediaFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var media = new Media
                        {
                            MediaUrl = $"/Uploads/Packages/{fileName}",
                            MediaType = file.ContentType,
                            PackageId = package.PackageId
                        };
                        await _packageRepository.AddMediaAsync(media);
                        package.Medias.Add(media);
                    }
                }

                await _genericRepository.AddAsync(package);
                await _genericRepository.SaveChangesAsync();

                var resultDTO = new PackageDTO
                {
                    PackageId = package.PackageId,
                    Name = package.Name,
                    Destination = package.Destination,
                    Description = package.Description,
                    BasePrice = package.BasePrice,
                    IsActive = package.IsActive,
                    Medias = package.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl =  m.MediaUrl ?? string.Empty,
                        MediaType = m.MediaType ?? string.Empty
                    }).ToList(),
                    PackageDates = package.PackageDates.Select(pd => new PackageDateDTO
                    {
                        PackageDateId = pd.PackageDateId,
                        StartDate = pd.StartDate,
                        EndDate = pd.EndDate
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetPackageById), new { id = package.PackageId },
                    new ApiResponse(true, "Package created successfully.", resultDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error creating package: {ex.Message}"));
            }
        }

        /// <summary>
        /// Updates an existing package.
        /// </summary>
        /// <param name="id">The ID of the package.</param>
        /// <param name="packageDTO">The updated package data.</param>
        /// <returns>The updated package.</returns>
        /// <response code="200">Returns the updated package.</response>
        /// <response code="400">If the package data or ID is invalid.</response>
        /// <response code="404">If the package is not found.</response>
        /// <response code="500">If an error occurs while updating the package.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePackage(int id, [FromForm] PackageUpdateDTO packageDTO)
        {
            if (id <= 0 || packageDTO == null || id != packageDTO.PackageId || !ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid package data or ID."));

            try
            {
                var package = await _genericRepository.GetByIdAsync(id);
                if (package == null)
                    return NotFound(new ApiResponse(false, $"Package with ID {id} not found."));

                // Carregar relacionamentos manualmente
                package.Medias = (await _packageRepository.GetPackageMediasAsync(id)).ToList();
                package.PackageDates = (await _packageRepository.GetPackageDatesAsync(id)).ToList();

                package.Name = packageDTO.Name;
                package.Destination = packageDTO.Destination;
                package.Description = packageDTO.Description;
                package.BasePrice = packageDTO.BasePrice ?? 0m;
                package.IsActive = packageDTO.IsActive;

                // Atualizar PackageDates
                var existingDates = package.PackageDates.ToList();
                package.PackageDates.Clear();
                foreach (var date in existingDates)
                {
                    await _genericRepository.SoftDeleteAsync<PackageDate>(date.PackageDateId);
                }
                package.PackageDates = packageDTO.PackageDates.Select(pd => new PackageDate
                {
                    StartDate = pd.StartDate,
                    EndDate = pd.EndDate,
                    PackageId = package.PackageId
                }).ToList();

                // Remover mídias especificadas
                foreach (var mediaId in packageDTO.MediaIdsToDelete)
                {
                    await _packageRepository.DeleteMediaAsync(mediaId);
                }

                // Adicionar novas mídias
                var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Packages");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in packageDTO.NewMediaFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var media = new Media
                        {
                            MediaUrl = $"/Uploads/Packages/{fileName}",
                            MediaType = file.ContentType,
                            PackageId = package.PackageId
                        };
                        await _packageRepository.AddMediaAsync(media);
                        package.Medias.Add(media);
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
                    IsActive = package.IsActive,
                    Medias = package.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        // Substitua a linha problemática por esta, usando o operador de coalescência nula para garantir que não haja atribuição de referência nula.
                        MediaUrl = m.MediaUrl ?? string.Empty,
                        MediaType = m.MediaType ?? string.Empty
                    }).ToList(),
                    PackageDates = package.PackageDates.Select(pd => new PackageDateDTO
                    {
                        PackageDateId = pd.PackageDateId,
                        StartDate = pd.StartDate,
                        EndDate = pd.EndDate
                    }).ToList()
                };

                return Ok(new ApiResponse(true, "Package updated successfully.", resultDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error updating package: {ex.Message}"));
            }
        }

        /// <summary>
        /// Soft deletes a package by its ID.
        /// </summary>
        /// <param name="id">The ID of the package.</param>
        /// <returns>Confirmation of deletion.</returns>
        /// <response code="204">Package soft deleted successfully.</response>
        /// <response code="400">If the ID is invalid.</response>
        /// <response code="404">If the package is not found.</response>
        /// <response code="500">If an error occurs while deleting the package.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeletePackage(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse(false, "Invalid package ID."));

            try
            {
                var deleted = await _genericRepository.SoftDeleteAsync(id);
                if (!deleted)
                    return NotFound(new ApiResponse(false, $"Package with ID {id} not found."));

                await _genericRepository.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error deleting package: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves all dates for a specific package.
        /// </summary>
        /// <param name="packageId">The ID of the package.</param>
        /// <returns>A list of dates for the package.</returns>
        /// <response code="200">Returns the list of package dates.</response>
        /// <response code="400">If the package ID is invalid.</response>
        /// <response code="404">If the package is not found.</response>
        /// <response code="500">If an error occurs while retrieving dates.</response>
        [HttpGet("{packageId}/dates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPackageDates(int packageId)
        {
            if (packageId <= 0)
                return BadRequest(new ApiResponse(false, "Invalid package ID."));

            try
            {
                var package = await _genericRepository.GetByIdAsync(packageId);
                if (package == null)
                    return NotFound(new ApiResponse(false, $"Package with ID {packageId} not found."));

                var dates = await _packageRepository.GetPackageDatesAsync(packageId);
                var dateDTOs = dates.Select(pd => new PackageDateDTO
                {
                    PackageDateId = pd.PackageDateId,
                    StartDate = pd.StartDate,
                    EndDate = pd.EndDate
                }).ToList();
                return Ok(new ApiResponse(true, "Package dates retrieved successfully.", dateDTOs));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error retrieving package dates: {ex.Message}"));
            }
        }
    }
}