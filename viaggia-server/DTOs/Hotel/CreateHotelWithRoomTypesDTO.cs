using viaggia_server.DTOs.Hotels;

namespace viaggia_server.DTOs.Hotel
{
    public class CreateHotelWithRoomTypesDTO : CreateHotelDTO
    {
        public List<CreateHotelRoomTypeDTO> RoomTypes { get; set; }
    }

}