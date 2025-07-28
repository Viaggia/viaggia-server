using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Hotels;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;

namespace viaggia_server.Repositories.Hotel
{
    public class HotelRepository : IHotelRepository
    {
        private readonly AppDbContext _context;

        public HotelRepository(AppDbContext context)
        {
            _context = context;
        }

        // Adiciona um novo endereço
        public async Task<Address> AddAddressAsync(Address address)
        {
            var result = await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        // Busca um endereço por ID
        public async Task<Address?> GetAddressByIdAsync(int addressId)
        {
            return await _context.Addresses.FindAsync(addressId);
        }

        // Retorna o endereço de um hotel específico (relacionamento 1:1)
        public async Task<Address?> GetAddressByHotelIdAsync(int hotelId)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Address)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);

            return hotel?.Address;
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
            return await _context.RoomTypes.FindAsync(roomTypeId);
        }

        // Retorna todos os tipos de quarto de um hotel específico
        public async Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId)
        {
            return await _context.RoomTypes
                .Where(rt => rt.HotelId == hotelId)
                .ToListAsync();
        }

        public async Task<HotelDate> AddHotelDateAsync(HotelDate hotelDate)
        {
            var result = await _context.HotelDates.AddAsync(hotelDate);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<HotelDate?> GetHotelDateByIdAsync(int hotelDateId)
        {
            return await _context.HotelDates.FindAsync(hotelDateId);
        }

        public async Task<IEnumerable<HotelDate>> GetHotelDatesAsync(int hotelId)
        {
            return await _context.HotelDates
                .Where(rt => rt.HotelId == hotelId)
                .ToListAsync();
        }

        public async Task<Media> AddMediaAsync(Media media)
        {
            var result = await _context.Medias.AddAsync(media);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public Task<bool> DeleteMediaAsync(int mediaId)
        {
            var media = _context.Medias.FindAsync(mediaId);
            if (media == null)
            {
                return Task.FromResult(false);
            }
            media.Result.IsActive = false; // Soft delete
            _context.Medias.Update(media.Result);
            return _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);

        }

        public async Task<IEnumerable<Media>> GetHotelMediasAsync(int hotelId)
        {
            return await _context.Medias
                 .Where(m => m.HotelId == hotelId && m.IsActive)
                 .ToListAsync();
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            var result = await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            return _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.IsActive);
        }

        public async Task<IEnumerable<Review>> GetHotelReviewsAsync(int hotelId)
        {
            return await _context.Reviews
                .Where(r => r.HotelId == hotelId && r.IsActive)
                .ToListAsync();
        }

        public async Task<Package> AddPackageAsync(Package package)
        {
            var result = await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Package?> GetPackageByIdAsync(int packageId)
        {
            return await _context.Packages
                .Include(p => p.PackageDates)
                .Include(p => p.Medias)
                .FirstOrDefaultAsync(p => p.PackageId == packageId && p.IsActive);
        }

        public async Task<IEnumerable<Package>> GetHotelPackagesAsync(int hotelId)
        {
            return await _context.Packages
                .Where(p => p.HotelId == hotelId && p.IsActive)
                .Include(p => p.PackageDates)
                .Include(p => p.Medias)
                .ToListAsync();
        }

        public Task<Commoditie> AddCommoditieAsync(Commoditie commoditie)
        {
            var result = _context.Commodities.Add(commoditie);
            _context.SaveChanges();
            return Task.FromResult(result.Entity);

        }

        public Task<Commoditie?> GetCommoditieByIdAsync(int commoditieId)
        {
           var commoditie = _context.Commodities
                .FirstOrDefaultAsync(c => c.CommoditieId == commoditieId && c.IsActive);
            return commoditie;


        }

        public Task<Commoditie?> GetCommoditieByHotelIdAsync(int hotelId)
        {
           var commoditie = _context.Commodities
                .FirstOrDefaultAsync(c => c.HotelId == hotelId && c.IsActive);
            return commoditie;
        }

        public Task<IEnumerable<Commoditie>> GetAllCommoditiesAsync() 
        {
            var commodities = _context.Commodities
                .Where(c => c.IsActive)
                .ToListAsync();
            return commodities.ContinueWith(task => (IEnumerable<Commoditie>)task.Result); // Retorna todas as commodities ativas
        }

        public async Task<bool> UpdateCommoditieAsync(Commoditie commoditie)
        {
            var existingCommoditie = await _context.Commodities
                .FirstOrDefaultAsync(c => c.CommoditieId == commoditie.CommoditieId && c.IsActive);
            if (existingCommoditie == null)
            {
                return false; // Commoditie não encontrada
            }
            existingCommoditie.IsActive = commoditie.IsActive; // Atualiza o status de atividade
            _context.Commodities.Update(existingCommoditie);
            return await _context.SaveChangesAsync() > 0; // Retorna true se a atualização foi bem-sucedida
        }

        public Task<bool> DeleteCommoditieAsync(int commoditieId)
        {
           var commoditie = _context.Commodities.FindAsync(commoditieId);
            if (commoditie == null)
            {
                return Task.FromResult(false); // Commoditie não encontrada
            }
            commoditie.Result.IsActive = false; // Soft delete
            _context.Commodities.Update(commoditie.Result);
            return _context.SaveChangesAsync().ContinueWith(t => t.Result > 0); // Retorna true se a exclusão foi bem-sucedida
        }

        public async Task<IEnumerable<Commoditie>> GetHotelCommoditiesAsync(int hotelId)
        {
            return await _context.Commodities
                .Where(c => c.HotelId == hotelId && c.IsActive)
                .ToListAsync();
        }
    }
}