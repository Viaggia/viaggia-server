using viaggia_server.Models.Addresses;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Data;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Models.HotelDates;

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
    }
}