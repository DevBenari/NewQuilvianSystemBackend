namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Constants
{
    /// <summary>
    /// Butir hak akses modul MCU — <c>contracts/permission-audit-matrix.md</c> (<c>MCU-PERM-v1</c>)
    /// bagian 1 dan 2, disalin apa adanya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa konstanta, bukan string lepas.</b> Pasangan <c>resource : action</c> wajib sama
    /// persis pada <c>ControllerName</c> di <c>[AccessController]</c>, <c>[AccessAction]</c>, dan
    /// <c>[AccessPermission]</c>. Selisih satu huruf menghasilkan 403 permanen yang tidak dapat
    /// diperbaiki dari layar Akses Role. Pola ini sama dengan modul Hemodialisa.
    /// </para>
    /// <para>
    /// <b>Seluruh resource MCU dicatat sekarang</b> (<c>BE-MCU-005</c>), walau controller-nya
    /// dibangun bertahap. Sebuah resource baru terdaftar di registry ketika controller pemiliknya
    /// lahir; konstanta di sini hanya memastikan task berikutnya memakai ejaan yang sama.
    /// </para>
    /// <para>
    /// <b>Hak akses tidak ditentukan kode.</b> Siapa memegang apa diatur lewat
    /// <c>SysAccessPolicies</c> per departemen dan jabatan. Satu-satunya pengecualian adalah
    /// syarat <i>dokter MCU yang ditetapkan</i>, yang ditegakkan service di atas hak akses ini.
    /// </para>
    /// </remarks>
    public static class MedicalCheckupPermissions
    {
        public const string ModuleCode = "HEALTH_SERVICE_MEDICAL_CHECKUP_MANAGEMENT";
        public const string ModuleName = "Health Service Medical Checkup Management";
        public const string AreaName = "HealthServices";

        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";

        public static class Package
        {
            public const string Resource = "McuPackage";
        }

        public static class ReferenceData
        {
            public const string Resource = "McuReferenceData";
        }

        public static class Schedule
        {
            public const string Resource = "McuSchedule";
            public const string ApproveQuotaOverride = "ApproveQuotaOverride";
        }

        public static class Order
        {
            public const string Resource = "McuOrder";
            public const string Cancel = "Cancel";
            public const string CheckIn = "CheckIn";
            public const string Import = "Import";
            public const string Recommend = "Recommend";
            public const string ReassignDoctor = "ReassignDoctor";
        }

        public static class Examination
        {
            public const string Resource = "McuExamination";
            public const string Start = "Start";
            public const string AddComponent = "AddComponent";
            public const string CancelComponent = "CancelComponent";
        }

        public static class Resume
        {
            public const string Resource = "McuResume";
            public const string Approve = "Approve";
            public const string Verify = "Verify";
            public const string Reopen = "Reopen";
        }

        public static class Delivery
        {
            public const string Resource = "McuDelivery";
            public const string ManageConsent = "ManageConsent";
        }

        public static class MedicalFeeSource
        {
            public const string Resource = "McuMedicalFeeSource";
        }
    }
}
