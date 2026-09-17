using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>
    /// Mengalihkan kantong <c>PendingReview</c> ke pasien lain (<c>DEC-BD-019</c>,
    /// <c>DEC-BD-028</c>, <c>DEC-BD-043</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pasien tujuan tidak diminta di body, dan itu bukan penyederhanaan.</b> Pengalihan adalah
    /// <i>alokasi dengan nama lain</i> (<c>INV-BD-028</c>), sehingga tujuannya dinyatakan dengan
    /// cara yang sama persis seperti alokasi: satu <see cref="BloodOrderLineId"/>. Baris kebutuhan
    /// itu sudah menunjuk ordernya, dan order menunjuk pasiennya. Meminta <c>PatientId</c> di body
    /// membuka jalan bagi kantong yang terikat pada baris order pasien A tetapi tercatat atas nama
    /// pasien B — dua catatan yang berselisih pada rekam klinis yang sama.
    /// </para>
    /// <para>
    /// <b>Alasan wajib dari daftar terkendali</b> (<c>VAL-BD-016</c>, <c>INV-BD-016</c>). Tidak ada
    /// field teks bebas: teks alasannya disalin backend dari <c>MstBloodBankReason</c> saat
    /// keputusan diambil, supaya riwayat lama tidak berubah makna ketika master disunting
    /// (<c>INV-BD-035</c>).
    /// </para>
    /// <para>
    /// <b>Nol field yang boleh dipercaya dari klien selain kedua isian ini.</b> Pelaku diambil dari
    /// akun yang login, waktu dari jam server, status tujuan dari matriks perpindahan, dan bukti
    /// kecocokan mana yang gugur dihitung backend.
    /// </para>
    /// </remarks>
    public class ReallocateUnitRequest
    {
        /// <summary>Baris kebutuhan order milik pasien tujuan pengalihan.</summary>
        [Required]
        public Guid BloodOrderLineId { get; set; }

        /// <summary>Kode alasan terkendali dari <c>MstBloodBankReason</c>.</summary>
        /// <remarks>
        /// <b>Sengaja tanpa <c>[Required]</c> maupun <c>[MaxLength]</c>.</b> Validasi model otomatis
        /// ASP.NET menolak body lebih dulu dengan <c>ProblemDetails</c> generik, sehingga kode
        /// <c>VAL-BD-016</c> dan pesan kanonisnya tidak pernah terkirim — terbukti runtime pada
        /// <c>AC-BD-025</c>. Service menolak kode kosong maupun kode yang tidak ada di master
        /// dengan <c>400 VAL-BD-016</c> secara <i>fail-closed</i>.
        /// </remarks>
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>
        /// Token konkurensi kantong yang dipegang layar. Bila kantong sudah berubah di tangan
        /// petugas lain, pengalihan ditolak <c>409</c> sebelum satu baris pun ditulis.
        /// </summary>
        public int? Version { get; set; }
    }

    /// <summary>
    /// Menyelesaikan kantong <c>PendingReview</c> ke salah satu status akhir yang
    /// <b>mengeluarkan</b> kantong dari peredaran: dikembalikan ke PMI, atau dinyatakan tidak layak
    /// (<c>DEC-BD-019</c>, <c>DEC-BD-043</c>, <c>DEC-BD-045</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Namanya diambil langsung dari kontrak <c>v4</c>.</b> <c>contracts/api-contract.md</c>
    /// menyebut <c>ResolveWithReasonRequest</c> pada <b>dua</b> endpoint sekaligus —
    /// <c>return-to-provider</c> (baris 115) dan <c>mark-not-usable</c> (baris 116) — sehingga
    /// kontrak memang memaksudkan satu bentuk bersama. Bentuknya pun benar-benar sama: satu kode
    /// alasan terkendali ditambah token konkurensi kantong.
    /// </para>
    /// <para>
    /// <b>Satu DTO, tetap dua endpoint dan dua butir hak akses.</b> Yang dibagi hanya bentuk body;
    /// penjaganya tetap <c>BloodUnit : ResolveReturn</c> dan <c>BloodUnit : ResolveNotUsable</c>
    /// secara terpisah (<c>INV-BD-034</c>), dan kategori alasan yang sah berbeda untuk keduanya.
    /// Menyatukan endpoint-nya menjadi satu tindakan bersaklar akan membuat satu butir hak akses
    /// menjaga dua tindakan yang justru sengaja dipisah.
    /// </para>
    /// <para>
    /// <b>Tidak ada field teks bebas untuk alasannya</b> (<c>VAL-BD-016</c>). Teksnya disalin
    /// backend dari master saat keputusan diambil.
    /// </para>
    /// </remarks>
    public class ResolveWithReasonRequest
    {
        /// <summary>Kode alasan terkendali dari <c>MstBloodBankReason</c>.</summary>
        /// <remarks>
        /// <b>Sengaja tanpa <c>[Required]</c> maupun <c>[MaxLength]</c>.</b> Validasi model otomatis
        /// ASP.NET menolak body lebih dulu dengan <c>ProblemDetails</c> generik, sehingga kode
        /// <c>VAL-BD-016</c> dan pesan kanonisnya tidak pernah terkirim — terbukti runtime pada
        /// <c>AC-BD-025</c>. Service menolak kode kosong maupun kode yang tidak ada di master
        /// dengan <c>400 VAL-BD-016</c> secara <i>fail-closed</i>.
        /// </remarks>
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>Token konkurensi kantong yang dipegang layar.</summary>
        public int? Version { get; set; }
    }
}
