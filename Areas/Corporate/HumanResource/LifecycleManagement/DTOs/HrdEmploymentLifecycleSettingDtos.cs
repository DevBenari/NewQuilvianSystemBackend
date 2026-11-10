using System;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class HrdEmploymentLifecycleSettingResponse
    {
        public Guid Id { get; set; }
        public Guid HospitalSiteId { get; set; }
        public string HospitalSiteName { get; set; } = string.Empty;
        public int ContractReminderDays { get; set; } = 60;
        public int ContractEscalationDays { get; set; } = 14;
        public int ProbationReminderDays { get; set; } = 14;
        public int LateStartFlagDays { get; set; } = 3;
        public Guid RowVersion { get; set; }
    }

    public sealed class UpdateEmploymentLifecycleSettingRequest
    {
        [Required]
        [Range(14, 180, ErrorMessage = "ContractReminderDays harus antara 14 sampai 180 hari (VR-ONB-80).")]
        public int ContractReminderDays { get; set; } = 60;

        [Required]
        [Range(1, 179, ErrorMessage = "ContractEscalationDays harus antara 1 hari sampai kurang dari ContractReminderDays (VR-ONB-80).")]
        public int ContractEscalationDays { get; set; } = 14;

        [Required]
        [Range(1, 60, ErrorMessage = "ProbationReminderDays harus antara 1 sampai 60 hari (VR-ONB-80).")]
        public int ProbationReminderDays { get; set; } = 14;

        [Required]
        [Range(1, 30, ErrorMessage = "LateStartFlagDays harus antara 1 sampai 30 hari (VR-ONB-80).")]
        public int LateStartFlagDays { get; set; } = 3;

        public Guid? RowVersion { get; set; }
    }
}
