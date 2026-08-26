using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprCase : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CaseNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid RequesterDoctorId { get; set; }
    public Guid PrimarySurgeonId { get; set; }
    public OprCaseType CaseType { get; set; }
    public OprPriority Priority { get; set; }
    public OprCaseStatus Status { get; set; } = OprCaseStatus.Requested;
    public OprCaseOutcome? Outcome { get; set; }
    public string Indication { get; set; } = string.Empty;
    public string? Laterality { get; set; }
    public int EstimatedMinutes { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? PreferredAt { get; set; }
    public int Version { get; set; }

    public MstPatient? Patient { get; set; }
    public TrxPatientEncounter? Encounter { get; set; }
    public MstDoctor? RequesterDoctor { get; set; }
    public MstDoctor? PrimarySurgeon { get; set; }
    public ICollection<OprCaseProcedure> Procedures { get; set; } = [];
    public ICollection<OprSchedule> Schedules { get; set; } = [];
    public ICollection<OprTeamMember> TeamMembers { get; set; } = [];
    public ICollection<OprSafetyChecklist> SafetyChecklists { get; set; } = [];
    public ICollection<OprStatusHistory> StatusHistories { get; set; } = [];
}
