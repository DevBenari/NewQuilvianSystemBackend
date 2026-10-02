using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Dua keadaan yang dapat disandang satu baris alokasi kantong darah, sesuai kamus data
    /// kontrak <c>v4</c> dan <c>02-backend-architecture.md</c> §546.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Baris alokasi adalah rekam sejarah, bukan penanda keadaan sekarang.</b> Pembatalan
    /// alokasi <b>tidak pernah</b> menghapus barisnya; ia hanya memindahkan
    /// <see cref="Active"/> → <see cref="Cancelled"/> lalu mengisi pelaku, waktu, dan alasan
    /// pembatalannya (<c>ARCH-BD-POS-03</c>). Karena itu satu kantong dapat memiliki banyak baris
    /// alokasi sepanjang hidupnya — misalnya dialokasikan, dibatalkan, lalu dialokasikan kembali —
    /// dan seluruhnya tetap terbaca.
    /// </para>
    ///
    /// <para>
    /// <b>Justru karena itu unique polos tidak dapat dipakai.</b> Yang dijaga bukan "satu baris
    /// alokasi per kantong", melainkan "paling banyak satu baris <see cref="Active"/> per kantong".
    /// Penjaganya index unik terfilter <c>(BloodUnitId) WHERE "AllocationStatus" = 0</c> — pola yang
    /// sama persis dengan penempatan berlaku pada <c>BE-BD-015</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Nilai angkanya mengikat.</b> Filter index menyebut <c>0</c> secara harfiah, sehingga
    /// <see cref="Active"/> wajib tetap bernilai <c>0</c>. Menukar urutan nilai enum ini akan
    /// membuat index menjaga keadaan yang salah tanpa satu pun pesan galat.
    /// </para>
    /// </remarks>
    public enum BbkAllocationStatus
    {
        /// <summary>Alokasi yang sedang berlaku. Paling banyak satu per kantong.</summary>
        [Display(Name = "Aktif")]
        Active = 0,

        /// <summary>Alokasi yang sudah dibatalkan. Tetap tersimpan sebagai riwayat.</summary>
        [Display(Name = "Dibatalkan")]
        Cancelled = 1
    }
}
