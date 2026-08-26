using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprCaseProcedure : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public Guid PatientProcedureId { get; set; }
    public bool IsPrimary { get; set; }
    public int Sequence { get; set; }
    public OprCase? OprCase { get; set; }
    public TrxPatientProcedure? PatientProcedure { get; set; }
}
