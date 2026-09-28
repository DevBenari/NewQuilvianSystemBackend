namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Constants
{
    /// <summary>
    /// Butir hak akses modul Hemodialisa — <c>contracts/permission-audit-matrix.md</c> bagian 2,
    /// disalin apa adanya dari kolom Hak akses <c>contracts/api-contract.md</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa konstanta, bukan string lepas.</b> Pasangan <c>resource : action</c> wajib sama
    /// persis di tiga tempat: <c>ControllerName</c> pada <c>[AccessController]</c>, argumen ke-1
    /// <c>[AccessPermission]</c>, dan argumen ke-1 <c>[AccessAction]</c> terhadap argumen ke-2
    /// <c>[AccessPermission]</c>. Selisih satu huruf menghasilkan 403 permanen yang tidak dapat
    /// diperbaiki dari layar Akses Role (<c>rules/backend/role-access-rules.md</c> bagian 3).
    /// Memakai konstanta yang sama di ketiga tempat membuat selisih itu mustahil terjadi.
    /// </para>
    /// <para>
    /// <b>Satu Resource, satu controller.</b> <c>AccessMenuSeeder</c> mendaftarkan setiap action
    /// di bawah <c>ControllerName</c> milik kelas controller-nya. Resource yang ditaruh di
    /// controller milik Resource lain tidak pernah terdaftar dan tidak pernah dapat diberikan.
    /// Karena itu endpoint pada satu base URL — misalnya <c>hemodialysis-episodes</c> — dilayani
    /// beberapa controller, masing-masing memegang satu Resource.
    /// </para>
    /// <para>
    /// <b>Hak akses tidak ditentukan kode.</b> Daftar ini hanya menyatakan kemampuan apa yang ada.
    /// Siapa yang memegangnya diputuskan admin lewat layar Pengaturan → Manajemen Role → Akses
    /// Role. Tidak ada nama peran, departemen, maupun jabatan di modul ini.
    /// </para>
    /// </remarks>
    public static class HemodialysisPermissions
    {
        public const string ModuleCode = "HEALTH_SERVICE_HEMODIALYSIS_MANAGEMENT";
        public const string ModuleName = "Health Service Hemodialysis Management";
        public const string AreaName = "HealthServices";

        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";

        public static class Order
        {
            public const string Resource = "HemodialysisOrder";
            public const string Accept = "Accept";
            public const string Hold = "Hold";
            public const string Reject = "Reject";
            public const string Cancel = "Cancel";
        }

        public static class Episode
        {
            public const string Resource = "HemodialysisEpisode";
            public const string ChangeStatus = "ChangeStatus";
        }

        public static class Eligibility
        {
            public const string Resource = "HemodialysisEligibility";
            public const string Decide = "Decide";
        }

        public static class VascularAccess
        {
            public const string Resource = "HemodialysisVascularAccess";
        }

        public static class Serology
        {
            public const string Resource = "HemodialysisSerology";
        }

        public static class Isolation
        {
            public const string Resource = "HemodialysisIsolation";
            public const string Decide = "Decide";
        }

        public static class Prescription
        {
            public const string Resource = "HemodialysisPrescription";
            public const string Activate = "Activate";
            public const string Cancel = "Cancel";
        }

        public static class Schedule
        {
            public const string Resource = "HemodialysisSchedule";
            public const string Cancel = "Cancel";
        }

        public static class Session
        {
            public const string Resource = "HemodialysisSession";
            public const string CheckIn = "CheckIn";
            public const string DeclareReady = "DeclareReady";
            public const string Hold = "Hold";
            public const string OverrideChecklist = "OverrideChecklist";
            public const string Start = "Start";
            public const string Stop = "Stop";
            public const string Complete = "Complete";
            public const string SubmitDocumentation = "SubmitDocumentation";
            public const string RetryBillingHandoff = "RetryBillingHandoff";
        }

        public static class Observation
        {
            public const string Resource = "HemodialysisObservation";
        }

        public static class Medication
        {
            public const string Resource = "HemodialysisMedication";
            public const string Administer = "Administer";
        }

        public static class Complication
        {
            public const string Resource = "HemodialysisComplication";
        }

        public static class Record
        {
            public const string Resource = "HemodialysisRecord";
            public const string Finalize = "Finalize";
        }

        public static class UnitReadiness
        {
            public const string Resource = "HemodialysisUnitReadiness";
            public const string DeclareReady = "DeclareReady";
            public const string DeclareNotReady = "DeclareNotReady";
        }

        public static class Machine
        {
            public const string Resource = "HemodialysisMachine";
            public const string ChangeStatus = "ChangeStatus";
        }

        public static class Station
        {
            public const string Resource = "HemodialysisStation";
            public const string ChangeStatus = "ChangeStatus";
        }

        public static class Setting
        {
            public const string Resource = "HemodialysisSetting";
        }

        public static class ChecklistItem
        {
            public const string Resource = "HemodialysisChecklistItem";
            public const string SetOverridable = "SetOverridable";
        }
    }
}
