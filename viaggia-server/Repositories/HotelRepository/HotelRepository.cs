using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
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
            var result = await _context.Hotels.AddAsync(hotel);
            await _context.SaveChangesAsync();
            return result.Entity;
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
                .AnyAsync(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
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
            var result = await _context.RoomTypes.AddAsync(roomType);
            await _context.SaveChangesAsync();
            return result.Entity;
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
            var result = await _context.HotelDates.AddAsync(hotelDate);
            await _context.SaveChangesAsync();
            return result.Entity;
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
            var result = _context.Medias.Add(media);
            await _context.SaveChangesAsync();
            return result.Entity;

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
           var result = _context.Reviews.Add(review); // Adiciona uma nova avaliação
            await _context.SaveChangesAsync();
            return result.Entity;

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
            var result = _context.Packages.Add(package);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        
      

      
    }
}