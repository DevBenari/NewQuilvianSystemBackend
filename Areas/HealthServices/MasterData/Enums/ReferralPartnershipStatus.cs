namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Status kerja sama fasilitas perujuk (<c>DEC-FRJ-001</c>). <b>Tidak dipersistensi</b>:
    /// selalu dihitung dari <c>IsPartner</c>, <c>IsActive</c>, dan perjanjian yang tersimpan
    /// terhadap satu tanggal acuan, supaya status tidak pernah basi ketika kontrak berakhir.
    /// </summary>
    /// <remarks>
    /// Urutan penentuan satu baris: <see cref="NonPartner"/> → <see cref="Inactive"/> →
    /// <see cref="Active"/> → <see cref="NotYetEffective"/> → <see cref="Expired"/> →
    /// <see cref="NoAgreement"/>. Hanya <see cref="Active"/> yang boleh dipilih pada transaksi
    /// rujukan baru.
    /// </remarks>
    public enum ReferralPartnershipStatus
    {
        /// <summary>Mitra aktif dengan perjanjian yang berlaku pada tanggal acuan.</summary>
        Active = 1,

        /// <summary>Mitra yang dinonaktifkan secara administratif.</summary>
        Inactive = 2,

        /// <summary>Seluruh perjanjian sudah berakhir dan belum ada perpanjangan.</summary>
        Expired = 3,

        /// <summary>Belum ada perjanjian berlaku, tetapi ada perjanjian yang akan dimulai.</summary>
        NotYetEffective = 4,

        /// <summary>Ditandai mitra tetapi belum punya perjanjian sama sekali (data lama).</summary>
        NoAgreement = 5,

        /// <summary>Data lama bukan mitra. Dipertahankan untuk histori, tidak dapat dipilih.</summary>
        NonPartner = 6
    }
}
