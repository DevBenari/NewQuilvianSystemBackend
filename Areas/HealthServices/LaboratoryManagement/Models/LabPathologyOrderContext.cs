using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Konteks klinis sebuah pesanan Patologi Anatomi — ditulis <b>dokter pemesan</b>, dibaca
    /// patolog (<c>LAB-DEC-091</c>, <c>LAB-DC-049</c>, <c>INV-40</c>).
    ///
    /// <b>Kenapa ia bukan bagian dari laporan, padahal tampil pada layar yang sama.</b> Karena
    /// <b>penulisnya orang yang berbeda</b>. Dokter pemesan menuliskan apa yang ia temukan pada
    /// pasien; patolog menuliskan apa yang ia lihat pada jaringan. Menggabungkan keduanya ke satu
    /// entity berarti memberi keduanya satu hak akses — dan itu membuat <b>patolog dapat menulis
    /// riwayat penyakit pasien yang tidak pernah ia tanyakan</b>, lalu tulisan itu masuk rekam
    /// medis. Karena itu jalur tulisnya memakai <c>LabOrder : Update</c>, bukan
    /// <c>LabExamination : Update</c>.
    ///
    /// <b>Kenapa tabel tersendiri, bukan empat kolom pada <see cref="LabOrder"/>.</b>
    /// <see cref="LabOrder"/> dipakai <b>tiga</b> disiplin, dan keempat ruas ini hanya bermakna
    /// bagi Patologi Anatomi. Menempelkannya di sana berarti menambah kolom ke tabel yang sudah
    /// berisi data pada alur pemesanan yang <b>sedang dipakai petugas</b>
    /// (<c>ARCH-GAP-LAB-07</c>).
    ///
    /// <b>Seluruh ruasnya nullable pada tingkat kolom, dan itu disengaja.</b> Pesanan lama nol
    /// terdampak. Kewajiban isinya — mana yang harus diisi dokter pemesan — ditegakkan
    /// <b>aturan bisnis</b> pada jalur pemesanan, bukan oleh kolom.
    ///
    /// <b>Keempat ruasnya isi rekam medis.</b> Dilarang masuk logger dan dilarang muncul pada
    /// layar non-klinis (<c>LAB-PERM-v1</c> rev 7 bagian 9.5).
    /// </summary>
    public class LabPathologyOrderContext : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Pesanan yang dijelaskan. Unik di antara baris yang belum ditandai terhapus — satu
        /// pesanan tepat satu konteks.
        /// </summary>
        [Required]
        public Guid LabOrderId { get; set; }

        /// <summary>
        /// Diagnosa Awal dari dokter pemesan. Menjadi sumber pengisian otomatis ruas
        /// <c>Diagnosa Klinis</c> pada formulir Imunohistokimia.
        /// </summary>
        public string? InitialDiagnosis { get; set; }

        /// <summary>Riwayat penyakit yang relevan bagi pembacaan jaringan.</summary>
        public string? RelevantHistory { get; set; }

        /// <summary>
        /// Masa terakhir haid. <b>Hanya bermakna bagi sitologi ginekologi</b>, sehingga
        /// kewajibannya ditegakkan aturan bisnis per golongan — bukan oleh kolom ini.
        /// </summary>
        public DateOnly? LastMenstrualPeriod { get; set; }

        /// <summary>Keterangan klinis lain dari dokter pemesan.</summary>
        public string? ClinicalNote { get; set; }

        /// <summary>Pesanan yang dijelaskan.</summary>
        public LabOrder? LabOrder { get; set; }
    }
}
