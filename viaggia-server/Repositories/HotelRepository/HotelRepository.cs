using viaggia_server.Models.Addresses;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<bool> NameExistsAsync(string name)
        {
            var exists = await _context.Hotels
                .AnyAsync(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return exists;
        }

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
    }
}