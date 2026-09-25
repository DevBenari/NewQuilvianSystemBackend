using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    /// <summary>
    /// Satu baris riwayat penugasan dokter penanggung jawab IGD.
    /// </summary>
    /// <remarks>
    /// API 0.8.0 bagian 3.2. <c>doctorId</c> dan <c>assignedByUserId</c> tetap dikirim untuk
    /// pemrosesan; <c>doctorName</c> dan <c>assignedByName</c> ditambahkan untuk ditampilkan
    /// (<c>IGD-DEC-129</c>). Nama yang tidak tersedia dikirim kosong, bukan GUID.
    ///
    /// <para>
    /// <b>Tidak ada ruas <c>isActive</c></b> — penugasan yang sedang berjalan dikenali dari
    /// <see cref="EffectiveTo"/> yang kosong (<c>IGD-DEC-130</c>).
    /// </para>
    ///
    /// <para>
    /// <b><c>assignedByUserId</c> boleh <c>null</c> — <c>BE-IGD-048</c>, <c>IGD-DEC-136</c>.</b>
    /// Hanya terjadi pada baris hasil pengisian data lama ketika pelaku historisnya tidak dapat
    /// dibuktikan. Pada baris itu <c>assignedByName</c> selalu berisi teks tetap
    /// <c>"Data historis"</c>, dihasilkan proyeksi backend — bukan tebakan dari kolom audit lain.
    /// Untuk setiap penugasan baru, kedua ruas ini tetap terisi seperti sebelumnya.
    /// </para>
    /// </remarks>
    public class EmergencyDoctorAssignmentResponse
    {
        public Guid Id { get; set; }

        public Guid EmergencyVisitId { get; set; }

        public Guid DoctorId { get; set; }

        public string? DoctorName { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public Guid? AssignedByUserId { get; set; }

        public string? AssignedByName { get; set; }

        public string? AssignmentReason { get; set; }
    }

    /// <summary>
    /// Penetapan dokter penanggung jawab pertama pada satu kunjungan IGD.
    /// </summary>
    /// <remarks>
    /// <c>AssignedByUserId</c> sengaja <b>tidak</b> ada di sini: pelakunya diambil dari token
    /// pengguna aktif, bukan dari kiriman pemanggil (kamus data bagian 4, "Diisi sistem").
    /// Alasan juga tidak diminta — validation bagian 3 aturan 3 mewajibkannya hanya pada
    /// pengalihan.
    /// </remarks>
    public class AssignEmergencyDoctorRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        /// <summary>
        /// Waktu penugasan mulai berlaku. Kosong berarti waktu server saat ini. Tidak boleh
        /// mendahului waktu kedatangan pasien (validation bagian 3 aturan 4).
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }
    }

    /// <summary>
    /// Pengalihan dokter penanggung jawab ke dokter lain.
    /// </summary>
    /// <remarks>
    /// Menutup baris berjalan dan membuka baris baru dalam satu transaksi. Alasan
    /// <b>wajib</b> di sini, berbeda dengan penetapan pertama.
    /// </remarks>
    public class HandoverEmergencyDoctorRequest
    {
        [Required]
        public Guid DoctorId { get; set; }

        /// <summary>
        /// Alasan pengalihan. Wajib, tetapi kewajibannya ditegakkan service, bukan
        /// <c>[Required]</c>.
        /// </summary>
        /// <remarks>
        /// Tipenya SENGAJA <c>string?</c>, bukan <c>string</c>, dan <c>[Required]</c> sengaja
        /// tidak dipakai.
        ///
        /// <para>
        /// Proyek ini memakai <c>Nullable enable</c> tanpa menyetel
        /// <c>SuppressImplicitRequiredAttributeForNonNullableReferenceTypes</c>, sehingga
        /// ASP.NET Core MVC menambahkan <c>[Required]</c> IMPLISIT pada setiap properti
        /// reference type yang non-nullable. Digabung dengan <c>[ApiController]</c> yang
        /// memvalidasi ModelState secara otomatis, properti bertipe <c>string</c> akan ditolak
        /// lebih dulu dengan <c>ProblemDetails</c> generik untuk ruas yang tidak dikirim,
        /// bernilai <c>null</c>, maupun berisi string kosong. Mencabut atribut eksplisit saja
        /// TIDAK menghapus kewajiban implisit itu; tipenya yang harus nullable.
        /// </para>
        ///
        /// <para>
        /// Dengan <c>string?</c>, keempat bentuk masukan kosong - ruas tidak dikirim,
        /// <c>null</c>, string kosong, dan spasi saja - semuanya lolos binding lalu jatuh ke
        /// <c>string.IsNullOrWhiteSpace</c> pada service, dan menghasilkan pesan kontrak
        /// <i>"Alasan pengalihan dokter wajib diisi."</i> yang dituntut acceptance 4 serta
        /// validation bagian 3 aturan 3.
        /// </para>
        ///
        /// <para>
        /// <c>MaxLength</c> tetap dipakai karena 500 memang batas kolom, dan penolakannya tidak
        /// dituntut berpesan khusus oleh kontrak. Atribut itu mengabaikan <c>null</c>, jadi ia
        /// tidak menghidupkan kembali penolakan dini.
        /// </para>
        /// </remarks>
        [MaxLength(500)]
        public string? AssignmentReason { get; set; }

        /// <summary>
        /// Waktu pengalihan berlaku. Kosong berarti waktu server saat ini. Nilai ini menjadi
        /// waktu berakhir baris lama sekaligus waktu mulai baris baru, sehingga riwayatnya
        /// tidak pernah berlubang dan tidak pernah bertindih.
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }
    }
}
