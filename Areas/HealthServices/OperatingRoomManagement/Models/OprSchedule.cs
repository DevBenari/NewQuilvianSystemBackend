using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprSchedule : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int BufferBeforeMinutes { get; set; }
    public int BufferAfterMinutes { get; set; }
    public int Revision { get; set; }
    public bool IsCurrent { get; set; }
    public string? ChangeReason { get; set; }
    public Guid ChangedByUserId { get; set; }
    public OprCase? OprCase { get; set; }
    public MstRoom? Room { get; set; }
    public ICollection<OprTeamMember> TeamMembers { get; set; } = [];
}
