namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class CreateOvertimePlanCommand
    {
        public string PlanTitle { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public DateOnly PlanStartDate { get; set; }
        public DateOnly PlanEndDate { get; set; }
        public DateTime PlanStartTime { get; set; }
        public DateTime PlanEndTime { get; set; }
        public string CompensationType { get; set; } = "FinancialPayout"; // FinancialPayout or CompensatoryTimeOff
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsGroupOvertime { get; set; } = false;
        public List<OvertimePlanStaffItemDto> StaffList { get; set; } = new();
    }

    public class OvertimePlanStaffItemDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string? EmployeeName { get; set; }
        public string? JobTitle { get; set; }
        public bool IsOvertimeExempt { get; set; } = false; // Manager level or above
        public DateTime PlannedStartAt { get; set; }
        public DateTime PlannedEndAt { get; set; }
        public int PlannedMinutes { get; set; } = 0;
        public string WorkDescription { get; set; } = string.Empty;
    }

    public class CancelOvertimePlanCommand
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class SubmitOvertimeRealizationCommand
    {
        public Guid? OvertimePlanId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string OvertimeCategory { get; set; } = "Planned"; // Planned or EmergencyCito
        public DateOnly WorkDate { get; set; }
        public DateTime ActualStartAt { get; set; }
        public DateTime ActualEndAt { get; set; }
        public int ActualMinutes { get; set; } = 0;
        public string CompensationType { get; set; } = "FinancialPayout";
        public bool IsOvertimeExempt { get; set; } = false;
        public string? EmergencyClinicalReferenceNo { get; set; }
        public string? ClinicalActionSummary { get; set; }
        public string? RealizationNotes { get; set; }
    }

    public class OvertimePlanResultDto
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PlanStatus { get; set; } = string.Empty;
        public decimal TotalEstimatedHours { get; set; }
        public int TotalStaffCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class OvertimePlanDetailDto
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PlanTitle { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public DateOnly PlanStartDate { get; set; }
        public DateOnly PlanEndDate { get; set; }
        public string CompensationType { get; set; } = "FinancialPayout";
        public string PlanStatus { get; set; } = "WaitingApproval";
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OvertimePlanStaffItemDto> StaffList { get; set; } = new();
    }

    public class OvertimePlanSummaryDto
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PlanTitle { get; set; } = string.Empty;
        public DateOnly PlanDate { get; set; }
        public string CompensationType { get; set; } = "FinancialPayout";
        public string PlanStatus { get; set; } = "WaitingApproval";
        public int TotalMinutes { get; set; }
        public int StaffCount { get; set; }
    }

    public class OvertimeValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; set; } = new();

        public void AddError(string error) => Errors.Add(error);
    }

    public class CreateOvertimePlanCommandValidator
    {
        public OvertimeValidationResult Validate(CreateOvertimePlanCommand command)
        {
            var result = new OvertimeValidationResult();

            if (string.IsNullOrWhiteSpace(command.PlanTitle))
                result.AddError("Judul rencana lembur wajib diisi.");
            else if (command.PlanTitle.Length > 200)
                result.AddError("Judul rencana lembur maksimal 200 karakter.");

            if (command.OrganizationUnitId == Guid.Empty)
                result.AddError("Unit kerja rumah sakit tidak valid atau telah dinonaktifkan."); // VR-OT-21

            if (string.IsNullOrWhiteSpace(command.Reason))
                result.AddError("Alasan lembur wajib diisi.");
            else if (command.Reason.Length > 2000)
                result.AddError("Alasan lembur maksimal 2000 karakter.");

            // VR-OT-01: Planned overtime must be in the future (H-1)
            if (command.PlanStartTime <= DateTime.UtcNow)
                result.AddError("Waktu mulai rencana lembur terencana harus di masa depan.");

            if (command.PlanEndTime <= command.PlanStartTime)
                result.AddError("Waktu selesai lembur harus lebih besar dari waktu mulai.");

            // VR-OT-22: Duration cannot exceed 24 hours
            if ((command.PlanEndTime - command.PlanStartTime).TotalHours > 24)
                result.AddError("Durasi lembur tidak valid: Melebihi total jam kalender dalam satu hari.");

            // VR-OT-20: Group overtime must have at least 1 employee
            if (command.IsGroupOvertime && (command.StaffList == null || command.StaffList.Count == 0))
                result.AddError("SPKL kelompok wajib memuat sekurang-kurangnya satu orang pegawai pelaksana.");

            // VR-OT-05: Managerial staff cannot choose FinancialPayout
            if (command.StaffList != null)
            {
                foreach (var staff in command.StaffList)
                {
                    if (staff.IsOvertimeExempt && command.CompensationType == "FinancialPayout")
                    {
                        result.AddError("Pejabat struktural/manajerial tidak berhak atas kompensasi uang lembur finansial (Overtime Exempt).");
                        break;
                    }
                }
            }

            return result;
        }
    }

    public class CancelOvertimePlanCommandValidator
    {
        public OvertimeValidationResult Validate(CancelOvertimePlanCommand command)
        {
            var result = new OvertimeValidationResult();
            if (string.IsNullOrWhiteSpace(command.Reason))
                result.AddError("Alasan pembatalan rencana lembur wajib diisi.");
            return result;
        }
    }

    public class SubmitOvertimeRealizationCommandValidator
    {
        public OvertimeValidationResult Validate(SubmitOvertimeRealizationCommand command)
        {
            var result = new OvertimeValidationResult();

            if (command.WorkforceProfileId == Guid.Empty)
                result.AddError("Identitas pegawai wajib diisi.");

            if (command.ActualEndAt <= command.ActualStartAt)
                result.AddError("Waktu selesai lembur aktual harus lebih besar dari waktu mulai.");

            // VR-OT-02: EmergencyCito requires clinical reference no & summary
            if (command.OvertimeCategory == "EmergencyCito")
            {
                if (string.IsNullOrWhiteSpace(command.EmergencyClinicalReferenceNo) ||
                    string.IsNullOrWhiteSpace(command.ClinicalActionSummary))
                {
                    result.AddError("Nomor Rekam Medis / Insiden dan ringkasan tindakan klinis darurat wajib diisi.");
                }

                // VR-OT-06: Submission deadline H+1 working day
                var maxSubmissionDate = command.WorkDate.ToDateTime(TimeOnly.MinValue).AddDays(2);
                if (DateTime.UtcNow > maxSubmissionDate)
                {
                    result.AddError("Batas waktu pelaporan klaim darurat medis pasca-dinas telah kedaluwarsa (maksimal H+1 hari kerja).");
                }
            }

            // VR-OT-05: Managerial staff cannot choose FinancialPayout
            if (command.IsOvertimeExempt && command.CompensationType == "FinancialPayout")
            {
                result.AddError("Pejabat struktural/manajerial tidak berhak atas kompensasi uang lembur finansial (Overtime Exempt).");
            }

            return result;
        }
    }
}
