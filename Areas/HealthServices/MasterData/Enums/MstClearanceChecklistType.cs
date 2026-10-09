namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Jenis daftar periksa butir administrasi Rawat Inap — kontrak <c>episode-rawat-inap</c>
    /// <c>0.11.0</c> backend 13.9 (<c>BE-RWI-185</c>, migration <c>E9</c>, <c>RWI-DEC-241</c>).
    /// </summary>
    /// <remarks>
    /// Penutupan episode hanya membaca <see cref="EpisodeClosure"/>, sedangkan Serah Terima Pasien
    /// Baru hanya membaca <see cref="NewPatientHandover"/> (<c>INV-RWA-12</c>). Butir lama menjadi
    /// <see cref="EpisodeClosure"/> lewat nilai bawaan kolom, sehingga perilaku penutupan tidak
    /// berubah selama belum ada butir jenis lain.
    /// </remarks>
    public enum MstClearanceChecklistType
    {
        /// <summary>Butir daftar periksa penutupan episode — perilaku lama.</summary>
        EpisodeClosure = 1,

        /// <summary>Butir "Ceklist Serah Terima Pasien Baru" pada Workspace PPRI.</summary>
        NewPatientHandover = 2
    }
}
