using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("TrxAttendanceCorrectionDetail", Schema = "public")]
    public class TrxAttendanceCorrectionDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AttendanceCorrectionRequestId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string DataType { get; set; } = "String";
        // String, Date, DateTime, Time, Integer, Decimal, Boolean, Guid.

        [MaxLength(2000)]
        public string? OriginalValue { get; set; }

        [MaxLength(2000)]
        public string? RequestedValue { get; set; }

        [MaxLength(2000)]
        public string? ApprovedValue { get; set; }

        [Required]
        [MaxLength(30)]
        public string DetailStatus { get; set; } = "Requested";
        // Requested, Approved, Rejected, Applied.

        [MaxLength(1000)]
        public string? Reason { get; set; }

        public bool IsApplied { get; set; } = false;
        public DateTime? AppliedAt { get; set; }
        public Guid? AppliedByUserId { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public TrxAttendanceCorrectionRequest? AttendanceCorrectionRequest { get; set; }
        public ApplicationUser? AppliedByUser { get; set; }
    }
}
