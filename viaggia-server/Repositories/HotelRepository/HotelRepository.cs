using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Addresses;
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
            var hotelDate = await _context.HotelDates
                .FirstOrDefaultAsync(hd => hd.HotelDateId == hotelDateId);
            return hotelDate;
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
            var result = await _context.Reviews.AddAsync(review); //    Adiciona uma nova avaliação
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

        public async Task<IEnumerable<Commoditie>> GetCommoditiesByHotelIdAsync(int hotelId) // Retorna todas as comodidades de um hotel específico
        {
            return await _context.Commodities
                .Where(c => c.HotelId == hotelId && c.IsActive)
                .ToListAsync();


        }

        public async Task<Commoditie?> GetCommoditieByIdAsync(int commoditieId) // Busca uma comodidade específica por ID
        {
            return await _context.Commodities
                .FirstOrDefaultAsync(c => c.CommoditieId == commoditieId && c.IsActive);
        }

        public async Task<Commoditie> AddCommoditieAsync(Commoditie commoditie) // Adiciona uma nova comodidade
        {
            var result = _context.Commodities.Add(commoditie);
            await _context.SaveChangesAsync();
            return result.Entity;

        }

        public async Task<IEnumerable<CommoditieServices>> GetCommoditieServicesByHotelIdAsync(int hotelId) // Retorna todos os serviços de comodidades de um hotel específico
        {
            return await _context.CommoditieServices
                .Where(cs => cs.HotelId == hotelId && cs.IsActive)
                .ToListAsync();
        }

        public async Task<CommoditieServices?> GetCommoditieServiceByIdAsync(int commoditieServicesId) // Busca um serviço de comodidade específico por ID
        {
           return await _context.CommoditieServices
                .FirstOrDefaultAsync(cs => cs.CommoditieServicesId == commoditieServicesId && cs.IsActive);

        }

        public async Task<CommoditieServices> AddCommoditieServiceAsync(CommoditieServices commoditieService) // Adiciona um novo serviço de comodidade
        {
            var result = _context.CommoditieServices.Add(commoditieService);
            await _context.SaveChangesAsync();
            return result.Entity;

        }

        public async Task<IEnumerable<Address>> GetAddressesByHotelIdAsync(int hotelId)
        {
          return await _context.Addresses
                .Where(a => a.HotelId == hotelId && a.IsActive)
                .ToListAsync();
        }

        public async Task<Hotel?> GetHotelWithDetailsByIdAsync(int id)
        {
            return await _context.Hotels
                .Include(h => h.RoomTypes)
                .Include(h => h.HotelDates)
                .Include(h => h.Addresses)
                .Include(h => h.Medias)
                .Include(h => h.Reviews)
                .Include(h => h.Packages)
                .Include(h => h.Commodities)
                .Include(h => h.CommoditieServices)
                .FirstOrDefaultAsync(h => h.HotelId == id && h.IsActive);
        }

        public async Task<List<Hotel>> GetAllHotelsWithDetailsAsync()
        {
            return await _context.Hotels
                .Include(h => h.RoomTypes)
                .Include(h => h.HotelDates)
                .Include(h => h.Addresses)
                .Include(h => h.Medias)
                .Include(h => h.Reviews)
                .Include(h => h.Packages)
                .Include(h => h.Commodities)
                .Include(h => h.CommoditieServices)
                .Where(h => h.IsActive)
                .ToListAsync();
        }


        public async Task<Hotel?> GetHotelByIdWithDetailsAsync(int hotelId)
        {
            return await _context.Hotels
                .Include(h => h.RoomTypes)
                .Include(h => h.HotelDates)
                .Include(h => h.Medias)
                .Include(h => h.Reviews)
                .Include(h => h.Addresses)
                .Include(h => h.Packages)
                .Include(h => h.Commodities)
                .Include(h => h.CommoditieServices)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);
        }
    }
}