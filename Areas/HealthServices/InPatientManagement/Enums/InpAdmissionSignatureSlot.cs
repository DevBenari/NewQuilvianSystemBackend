namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Slot tanda tangan dokumen admisi — backend 13.9 (<c>BE-RWI-192</c>). Slot wajib per jenis
    /// ditetapkan service; slot lain ditolak <c>422 INP-ADM-DOC-031</c>.
    /// </summary>
    public enum InpAdmissionSignatureSlot
    {
        /// <summary>Pasien atau keluarga — selalu catatan kertas selama <i>fail-closed</i>.</summary>
        PatientOrFamily = 1,

        /// <summary>Admission / Petugas PPRI.</summary>
        AdmissionOfficer = 2,

        /// <summary>CRO (<i>Customer Relation Officer</i>).</summary>
        CustomerRelationOfficer = 3,

        /// <summary>Perawat penerima — hanya sesudah pasien menempati bed (<c>INV-RWA-11</c>).</summary>
        ReceivingNurse = 4,

        /// <summary>Kepala Ruangan — hanya pemegang <c>SignAsHeadNurse</c> (<c>RWI-DEC-238</c>).</summary>
        HeadNurse = 5
    }
}
