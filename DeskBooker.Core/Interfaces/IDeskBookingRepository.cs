using DeskBooker.Core.Domain;

namespace DeskBooker.Core.Interfaces
{
    public interface IDeskBookingRepository
    {
        void Save(DeskBooking deskBooking);
    }
}
