namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants
{
    /// <summary>
    /// Butir hak akses modul Rehab Medik — <c>contracts/permission-audit-matrix.md</c> bagian 2,
    /// disalin apa adanya dari kolom Hak akses <c>contracts/api-contract.md</c> v1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa konstanta.</b> Pasangan <c>resource : action</c> wajib sama persis di tiga
    /// tempat: <c>ControllerName</c> pada <c>[AccessController]</c>, argumen ke-1
    /// <c>[AccessPermission]</c>, dan argumen ke-1 <c>[AccessAction]</c> terhadap argumen ke-2
    /// <c>[AccessPermission]</c>. Memakai konstanta yang sama mencegah 403 permanen karena selisih
    /// satu huruf.
    /// </para>
    /// <para>
    /// <b>Satu Resource, satu controller</b> — pola <c>HemodialysisPermissions</c>. Base URL
    /// <c>rehab-sessions</c> dilayani dua controller: <c>RehabSession</c> dan
    /// <c>RehabBillingHandoff</c>.
    /// </para>
    /// <para>
    /// <b>Hak akses tidak ditentukan kode.</b> Pemegangnya diputuskan admin di Pengaturan →
    /// Manajemen Role → Akses Role. Tidak ada nama peran, departemen, maupun jabatan di modul ini.
    /// </para>
    /// </remarks>
    public static class RehabilitationPermissions
    {
        public const string ModuleCode = "HEALTH_SERVICE_REHABILITATION_MANAGEMENT";
        public const string ModuleName = "Health Service Rehabilitation Management";
        public const string AreaName = "HealthServices";

        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Sign = "Sign";

        public static class Request
        {
            public const string Resource = "RehabRequest";
            public const string Return = "Return";
            public const string Disposition = "Disposition";
            public const string Cancel = "Cancel";
        }

        public static class Lookup
        {
            public const string Resource = "RehabLookup";
        }

        public static class Episode
        {
            public const string Resource = "RehabEpisode";
        }

        public static class MedicalAssessment
        {
            public const string Resource = "RehabMedicalAssessment";
        }

        public static class DisciplineAssessment
        {
            public const string Resource = "RehabDisciplineAssessment";
        }

        public static class Goal
        {
            public const string Resource = "RehabGoal";
        }

        /// <summary>
        /// Program terapi. Nama kelas sengaja bukan <c>Program</c> agar tidak tertukar dengan
        /// kelas entry point <c>Program</c>; nilai Resource tetap <c>RehabProgram</c>.
        /// </summary>
        public static class TherapyProgram
        {
            public const string Resource = "RehabProgram";
            public const string Activate = "Activate";
            public const string Stop = "Stop";
        }

        public static class Education
        {
            public const string Resource = "RehabEducation";
        }

        public static class Session
        {
            public const string Resource = "RehabSession";
        }

        public static class BillingHandoff
        {
            public const string Resource = "RehabBillingHandoff";
            public const string Retry = "Retry";
        }

        public static class ProgressReview
        {
            public const string Resource = "RehabProgressReview";
        }

        public static class DischargeSummary
        {
            public const string Resource = "RehabDischargeSummary";
        }

        public static class Configuration
        {
            public const string Resource = "RehabConfiguration";
        }
    }
}
