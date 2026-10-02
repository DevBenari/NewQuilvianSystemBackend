namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Peran seorang dokter pada satu baris penugasan episode rawat inap. Lahir
    /// <c>BE-RWI-074</c> menyerap <c>RWI-DEC-099</c>.
    /// </summary>
    /// <remarks>
    /// <b>Nilai <c>0</c> sengaja tidak dipakai.</b> Baris lama yang terisi nilai bawaan
    /// database bernilai <c>1</c>, bukan <c>0</c>, sehingga tidak ada baris yang dapat
    /// disalahartikan sebagai "peran belum ditetapkan". Setiap baris punya peran yang
    /// eksplisit — <c>02-backend-architecture.md</c> revision <c>0.7</c> bagian 0.2.
    ///
    /// <para>
    /// <b>Hanya <see cref="Dpjp"/> yang membawa kewenangan keputusan.</b> Keempat penjaga
    /// <c>GUARD-INP-01</c> sampai <c>GUARD-INP-04</c> berbunyi <c>AssignmentRole = Dpjp</c>,
    /// bukan sekadar "punya penugasan aktif". Konsulen dan dokter jaga boleh menulis dokumen
    /// klinis, tetapi tidak boleh memutuskan pulang, menandatangani resume, memindahkan
    /// pasien, maupun mengubah kebutuhan isolasi — <c>permission-audit-matrix.md</c> bagian
    /// 4-A.1.
    /// </para>
    /// </remarks>
    public enum InpDoctorAssignmentRole
    {
        /// <summary>
        /// Dokter penanggung jawab pelayanan. <b>Tepat satu</b> yang aktif per episode,
        /// dijaga unique index parsial <c>IX_InpDoctorAssignment_EpisodeId_ActiveDpjp</c>.
        /// </summary>
        Dpjp = 1,

        /// <summary>
        /// Konsulen yang dilibatkan kepala ruangan atau supervisor. Boleh banyak dan boleh
        /// bersamaan: satu pasien dapat dikonsultasikan ke penyakit dalam, bedah, dan
        /// anestesi pada hari yang sama.
        /// </summary>
        Consultant = 2,

        /// <summary>
        /// Dokter jaga yang dipanggil kepala ruangan atau supervisor. Boleh banyak dan boleh
        /// bersamaan, mengikuti pergantian shift.
        /// </summary>
        OnCallDoctor = 3
    }
}
