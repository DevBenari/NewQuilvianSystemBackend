namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants
{
    public static class SchedulingRequestValueConstants
    {
        public static class Workflow
        {
            public const string ScheduleChangeCode = "SCHEDULE_CHANGE_REQUEST";
            public const string ScheduleChangeReferenceType = "SCHEDULE_CHANGE_REQUEST";
            public const string ShiftSwapCode = "SHIFT_SWAP_REQUEST";
            public const string ShiftSwapReferenceType = "SHIFT_SWAP_REQUEST";
        }

        public static class ScheduleChangeStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string UnderReview = "UnderReview";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string Applied = "Applied";
        }

        public static class ScheduleChangeType
        {
            public const string ScheduleChange = "ScheduleChange";
            public const string ShiftChange = "ShiftChange";
            public const string DayOffChange = "DayOffChange";
            public const string TemporarySchedule = "TemporarySchedule";

            public static readonly string[] All =
            {
                ScheduleChange,
                ShiftChange,
                DayOffChange,
                TemporarySchedule
            };
        }

        public static class ShiftSwapStatus
        {
            public const string Draft = "Draft";
            public const string PendingTarget = "PendingTarget";
            public const string TargetAccepted = "TargetAccepted";
            public const string TargetRejected = "TargetRejected";
            public const string PendingApproval = "PendingApproval";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string Applied = "Applied";
        }

        public static class ShiftRevisionChangeType
        {
            public const string Replacement = "Replacement";
            public const string RetroactiveCorrection = "RetroactiveCorrection";
            public const string AddShift = "AddShift";
            public const string RemoveShift = "RemoveShift";
        }

        public static class OnCallCallStatus
        {
            public const string Sent = "Sent";
            public const string Accepted = "Accepted";
            public const string Declined = "Declined";
            public const string Expired = "Expired";
            public const string Completed = "Completed";
        }

        // Kode aturan pemeriksaan roster yang ditambah amandemen A52. Kode lain (B1, B2, B4–B7,
        // W1, W4, W5) masih tertulis literal di RosterValidationService.
        public static class RosterRuleCode
        {
            // B3: pegawai tidak punya penempatan aktif di unit roster pada tanggal shift;
            // penempatan Temporary di unit lain mengalahkan penempatan utama.
            public const string NoActivePlacementInUnit = "B3";

            // B8: pegawai sedang tugas belajar Active pada tanggal shift. Tidak dapat diturunkan.
            public const string ActiveStudyAssignment = "B8";

            // W2: hari kerja berturut-turut melebihi MaxConsecutiveWorkDays.
            public const string ConsecutiveWorkDays = "W2";

            // W3: shift malam berturut-turut melebihi MaxConsecutiveNightShifts.
            public const string ConsecutiveNightShifts = "W3";

            // W6: jumlah tenaga pada shift kurang dari kuota standar ketenagaan Active.
            public const string StaffingQuotaShortage = "W6";

            // W7: kebutuhan kualifikasi/skill-mix shift tidak terpenuhi.
            public const string SkillMixNotMet = "W7";
        }

        public static readonly string[] SortDirections = { "asc", "desc" };
        public static readonly int[] PageSizes = { 10, 25, 50, 100 };
    }
}
