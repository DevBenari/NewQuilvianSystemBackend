namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Constants
{
    /// <summary>
    /// Butir hak akses modul Optik — <c>contracts/permission-audit-matrix.md</c> (<c>OPT-PERM-v1</c>)
    /// bagian 1, disalin apa adanya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa konstanta, bukan string lepas.</b> Pasangan <c>resource : action</c> wajib sama
    /// persis pada <c>ControllerName</c> di <c>[AccessController]</c>, <c>[AccessAction]</c>, dan
    /// <c>[AccessPermission]</c>. Selisih satu huruf menghasilkan 403 permanen yang tidak dapat
    /// diperbaiki dari layar Akses Role. Pola ini sama dengan modul MCU dan Hemodialisa.
    /// </para>
    /// <para>
    /// <b>Seluruh resource Optik dicatat sekarang</b> (<c>BE-OPT-002</c>), walau controller-nya
    /// dibangun bertahap. Konstanta di sini memastikan task berikutnya memakai ejaan yang sama.
    /// </para>
    /// <para>
    /// <b>Hak akses tidak ditentukan kode.</b> Siapa memegang apa diatur lewat
    /// <c>SysAccessPolicies</c> per departemen dan jabatan. Syarat tambahan — profesi RO/Sp.M dan
    /// izin praktik berlaku (<c>OPT-DEC-005</c>, <c>OPT-DEC-012</c>) — ditegakkan service.
    /// </para>
    /// </remarks>
    public static class OpticalPermissions
    {
        public const string ModuleCode = "HEALTH_SERVICE_OPTICAL_MANAGEMENT";
        public const string ModuleName = "Health Service Optical Management";
        public const string AreaName = "HealthServices";

        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";

        public static class Visit
        {
            public const string Resource = "OptVisit";
        }

        public static class RefractionExam
        {
            public const string Resource = "OptRefractionExam";
            public const string Finalize = "Finalize";
            public const string Revise = "Revise";
            public const string Cancel = "Cancel";
        }

        public static class Prescription
        {
            public const string Resource = "OptPrescription";
            public const string Revise = "Revise";
            /// <summary>Dicek service di dalam create/revise, bukan atribut endpoint (<c>OPT-DEC-045</c>).</summary>
            public const string OverrideRange = "OverrideRange";
        }

        public static class Order
        {
            public const string Resource = "OptOrder";
            public const string Bill = "Bill";
            public const string SendToWork = "SendToWork";
            public const string MarkReady = "MarkReady";
            public const string HandOver = "HandOver";
            public const string Cancel = "Cancel";
            public const string RequestCancel = "RequestCancel";
            public const string DecideCancel = "DecideCancel";
            public const string CloseNotCollected = "CloseNotCollected";
        }

        public static class WarrantyClaim
        {
            public const string Resource = "OptWarrantyClaim";
            public const string Examine = "Examine";
            public const string Decide = "Decide";
        }

        public static class ReferenceData
        {
            public const string Resource = "OptReferenceData";
            public const string UpdateSetting = "UpdateSetting";
        }

        public static class Report
        {
            public const string Resource = "OptReport";
        }
    }
}
