using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprTeamMember : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid WorkforceId { get; set; }
    public OprTeamRole Role { get; set; }
    public bool IsLead { get; set; }
    public OprCredentialCheckStatus CredentialCheckStatus { get; set; } = OprCredentialCheckStatus.Pending;
    public DateTime? CredentialCheckedAt { get; set; }
    public bool IsCurrent { get; set; } = true;
    public OprCase? OprCase { get; set; }
    public OprSchedule? Schedule { get; set; }
    public MstWorkforceProfile? Workforce { get; set; }
}
