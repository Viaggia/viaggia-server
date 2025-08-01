using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Hotels;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;

namespace viaggia_server.Repositories.HotelRepository
{
    public class HotelRepository : IHotelRepository
    {
        private readonly AppDbContext _context;

        public HotelRepository(AppDbContext context)
        {
            _context = context;
        }


        // Criar um hotel
        public async Task<Hotel?> CreateAsync(Hotel hotel)
        {
            await _context.Hotels.AddAsync(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }

        // Aguarda a aprovação do hotel
        public async Task<Hotel> StatusHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                //hotel.Status = HotelStatus.Approved;
                await _context.SaveChangesAsync();
            }
            return hotel;
        }

        // Reactive hotel by ID
        public async Task<bool> ReactivateAsync(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                hotel.IsActive = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Verifica se um hotel já existe pelo nome
        public async Task<bool> NameExistsAsync(string name)
        {
            var exists = await _context.Hotels
                .AnyAsync(h => h.Name.ToLower() == name.ToLower());
            return exists;
        }

        // Verifica se um hotel já existe pelo CNPJ
        public async Task<bool> CnpjExistsAsync(string? cnpj)
        {
            if (string.IsNullOrEmpty(cnpj))
                return false;
            var exists = await _context.Hotels
                .AnyAsync(h => h.Cnpj.Equals(cnpj, StringComparison.OrdinalIgnoreCase));
            return exists;
        }


        // Adiciona um novo tipo de quarto
        public async Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType)
        {
            await _context.RoomTypes.AddAsync(roomType);
            await _context.SaveChangesAsync();
            return roomType;
        }

        // Busca um tipo de quarto por ID
        public async Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId)
        {
            return await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId && rt.IsActive);
        }

        // Retorna todos os tipos de quarto de um hotel específico
        public async Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId)
        {
            return await _context.RoomTypes
                 .Where(rt => rt.HotelId == hotelId && rt.IsActive)
                .ToListAsync();
        }

        // Retorna todas as datas de um hotel específico
        public async Task<IEnumerable<HotelDate>> GetHotelDatesAsync(int hotelId)
        {
            return await _context.HotelDates
                .Where(hd => hd.HotelId == hotelId)
                .ToListAsync();
        }

        // Busca uma data específica de um hotel por ID
        public async Task<HotelDate?> GetHotelDateByIdAsync(int hotelDateId)
        {
            return await _context.HotelDates
                .FirstOrDefaultAsync(hd => hd.HotelDateId == hotelDateId && hd.IsActive);
        }

        // Adiciona uma nova data de hotel
        public async Task<HotelDate> AddHotelDateAsync(HotelDate hotelDate)
        {
            await _context.HotelDates.AddAsync(hotelDate);
            await _context.SaveChangesAsync();
            return hotelDate;
        }

     
        public async Task<IEnumerable<Media>> GetMediasByHotelIdAsync(int hotelId) // Retorna todas as mídias de um hotel específico
        {
            var medias = await _context.Medias
                .Where(m => m.HotelId == hotelId)
                .ToListAsync();
            return medias;
        }

        public async Task<Media?> GetMediaByIdAsync(int mediaId) // Busca uma mídia específica por ID
        {
            var media = await _context.Medias
                .FirstOrDefaultAsync(m => m.MediaId == mediaId);
            return media;
        }

        public async Task<Media> AddMediaAsync(Media media) // Adiciona uma nova mídia
        {
            await _context.Medias.AddAsync(media);
            await _context.SaveChangesAsync();
            return media;

        }

        public async Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId) // Retorna todas as avaliações de um hotel específico
        {
            var reviews = await _context.Reviews
                .Where(r => r.HotelId == hotelId && r.IsActive)
                .ToListAsync();
            return reviews;
        }

        public async Task<Review?> GetReviewByIdAsync(int reviewId) // Busca uma avaliação específica por ID
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.IsActive);
            return review;
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return review;

        }

        public async Task<IEnumerable<Package>> GetPackagesByHotelIdAsync(int hotelId) // Retorna todos os pacotes de um hotel específico
        {
            var packages = await _context.Packages
                .Where(p => p.HotelId == hotelId && p.IsActive)
                .ToListAsync();
            return packages;
        }

        public async Task<Package?> GetPackageByIdAsync(int packageId) // Busca um pacote específico por ID
        {
           return await _context.Packages
                .FirstOrDefaultAsync(p => p.PackageId == packageId && p.IsActive);
        }

        public async  Task<Package> AddPackageAsync(Package package) // Adiciona um novo pacote
        {
            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<IEnumerable<Commoditie>> GetCommoditiesByHotelIdAsync(int hotelId) // Retorna todas as comodidades de um hotel específico
        {
            return await _context.Commodities
                .Where(c => c.HotelId == hotelId && c.IsActive)
                .ToListAsync();
        }

        public async Task<Commoditie?> GetCommoditieByIdAsync(int commoditieId)
        {
            return await _context.Commodities
                .Include(c => c.CommoditieServices)
                .FirstOrDefaultAsync(c => c.CommoditieId == commoditieId && c.IsActive);
        }

        public async Task<Commoditie> AddCommoditieAsync(Commoditie commoditie)
        {
            // Adiciona primeiro a commoditie principal
            await _context.Commodities.AddAsync(commoditie);
            await _context.SaveChangesAsync();

            // Se houver serviços personalizados, adiciona-os vinculando corretamente o CommoditieId
            if (commoditie.CommoditieServices != null && commoditie.CommoditieServices.Any())
            {
                foreach (var service in commoditie.CommoditieServices)
                {
                    service.CommoditieId = commoditie.CommoditieId;
                    service.HotelId = commoditie.HotelId;
                    await _context.CommoditieServices.AddAsync(service);
                }
                await _context.SaveChangesAsync();
            }

            return commoditie;
        }

        public async Task<IEnumerable<CommoditieServices>> GetCommoditieServicesByHotelIdAsync(int hotelId)
        {
            return await _context.CommoditieServices
                .Where(cs => cs.HotelId == hotelId && cs.IsActive)
                .ToListAsync();
        }

        public async  Task<CommoditieServices?> GetCommoditieServiceByIdAsync(int commoditieServiceId)
        {
            return await _context.CommoditieServices
                .FirstOrDefaultAsync(cs => cs.CommoditieServicesId == commoditieServiceId && cs.IsActive);
        }

        public async Task<CommoditieServices> AddCommoditieServiceAsync(CommoditieServices commoditieService)
        {
            await _context.CommoditieServices.AddAsync(commoditieService);
            await _context.SaveChangesAsync();
            return commoditieService;
        }

        public async Task<bool> SoftDeleteMediaAsync(int mediaId)
        {
            var media = await _context.Medias.FindAsync(mediaId);
            if (media != null)
            {
                media.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                return true;
            }
            return false;


        }

        public async Task<Hotel?> GetHotelByNameAsync(string name)
        {
            return await _context.Hotels
                .FirstOrDefaultAsync(h => h.Name.ToLower() == name.ToLower());
        }
    }
}