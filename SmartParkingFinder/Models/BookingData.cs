namespace SmartParkingFinder.Models
{
    public class BookingData   // ⚠️ static NAHI hona chahiye
    {
        public string SlotName { get; set; }
        public string Location { get; set; }
        public bool IsBooked { get; set; }
    }
}