namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants
{
    /// <summary>
    /// Kode persetujuan SPPD pada workflow persetujuan umum (Workflow V2), kontrak <c>v28</c>
    /// (integration-contract bagian 25.1; <c>HRD-DEC-224</c>).
    /// </summary>
    public static class BusinessTravelWorkflowCodes
    {
        /// <summary>
        /// Jenis referensi instance workflow milik SPPD. Penangan siklus referensi SPPD hanya
        /// menangani nilai ini.
        /// </summary>
        public const string ReferenceType = "HRD_TRV_REQUEST";

        /// <summary>
        /// Jenis permohonan pada definisi workflow; tampil sebagai jenis dokumen di kotak masuk
        /// persetujuan. Sama dengan <see cref="ReferenceType"/> sesuai kontrak <c>v28</c>.
        /// </summary>
        public const string RequestType = ReferenceType;

        /// <summary>Kode definisi workflow "Persetujuan Perjalanan Dinas".</summary>
        public const string DefinitionCode = "BUSINESS_TRAVEL_REQUEST";

        /// <summary>Kode tahap bawaan: atasan langsung pemohon.</summary>
        public const string ManagerStepCode = "HRD-TRV-01";

        /// <summary>Kategori definisi workflow modul SDM.</summary>
        public const string WorkflowCategory = "HumanResource";

        /// <summary>
        /// Status definisi yang dipilih Workflow V2 saat membuat instance (bersama <c>IsActive</c>).
        /// </summary>
        public const string DefinitionActiveStatus = "Active";

        /// <summary>Tenggat tugas persetujuan atasan, dalam jam (<c>HRD-TF-075</c>).</summary>
        public const int ManagerStepSlaHours = 48;
    }
}
