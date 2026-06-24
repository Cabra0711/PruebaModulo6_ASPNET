using RentingBooking.Models;

namespace RentingBooking.Models;

public class DashboardViewModel
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public double OccupancyRate { get; set; }
    public IEnumerable<Property> Properties { get; set; } = new List<Property>();
    public IEnumerable<Booking> RecentBookings { get; set; } = new List<Booking>();
    public Guid? SelectedPropertyId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
