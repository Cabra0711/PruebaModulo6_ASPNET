using RentingBooking.Enum;

namespace RentingBooking.Models;

public class KycVerification : BaseEntity
{
    public Guid UserId { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public KycStatus Status { get; set; } = KycStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime? VerifiedAt { get; set; } = DateTime.UtcNow;
    public virtual User User { get; set; } = null!;

}