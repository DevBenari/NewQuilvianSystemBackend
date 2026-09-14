using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>
    /// Mengalokasikan satu kantong darah ke satu baris kebutuhan order (<c>DEC-BD-003</c>,
    /// <c>DEC-BD-007</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yang diterima hanya tujuan alokasinya.</b> Status kantong, pelaku, dan waktu
    /// <b>tidak</b> pernah datang dari body: status dihitung backend dari matriks perpindahan,
    /// pelaku diambil dari akun yang login, dan waktu dari jam server. Body yang boleh menentukan
    /// status akan membuat gerbang penyimpanan dapat dilewati hanya dengan mengirim angka.
    /// </para>
    /// <para>
    /// <b>Pasien tidak diminta di sini.</b> Pasien tujuan ditentukan oleh
    /// <see cref="BloodOrderLineId"/> — baris kebutuhan itu sudah menunjuk ordernya, dan order
    /// menunjuk pasiennya. Meminta pasien di body membuka jalan bagi kantong yang dialokasikan ke
    /// baris order pasien A tetapi tercatat atas nama pasien B.
    /// </para>
    /// </remarks>
    public class AllocateUnitRequest
    {
        /// <summary>Baris kebutuhan order yang menjadi tujuan alokasi.</summary>
        [Required]
        public Guid BloodOrderLineId { get; set; }

        /// <summary>
        /// Token konkurensi kantong yang dipegang layar. Bila kantong sudah berubah di tangan
        /// petugas lain, alokasi ditolak <c>409</c> sebelum satu baris pun ditulis.
        /// </summary>
        public int? Version { get; set; }
    }

    /// <summary>
    /// Membatalkan alokasi kantong yang keliru, sebelum kantong diberikan (<c>DEC-BD-029</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Namanya diambil langsung dari kontrak <c>v4</c>, bukan dari konvensi source.</b>
    /// <c>contracts/api-contract.md</c> menyebut <c>CancelWithReasonRequest</c> pada <b>tiga</b>
    /// endpoint sekaligus — pembatalan order (baris 37), pembatalan permintaan PMI (baris 88), dan
    /// pembatalan alokasi (baris 106) — sehingga kontrak memang memaksudkan <b>satu bentuk
    /// bersama</b>, bukan satu DTO per aggregate. Bentuknya pun benar-benar sama untuk ketiganya:
    /// satu kode alasan terkendali, ditambah token konkurensi aggregate yang bersangkutan.
    /// </para>
    /// <para>
    /// <b>Dua aggregate lain masih memakai nama per-aggregate, dan itu penyimpangan yang sudah
    /// ada sebelum task ini.</b> <c>CancelBloodOrderRequest</c> (<c>BE-BD-003</c>) dan
    /// <c>CancelProviderRequestRequest</c> (<c>BE-BD-004</c>) berbentuk identik dengan DTO ini
    /// tetapi bernama lain. Keduanya milik task yang sudah selesai dan <b>tidak</b> disentuh di
    /// sini; penyeragamannya dicatat sebagai delta pada laporan <c>BE-BD-006</c> untuk diputuskan
    /// pemilik kontrak. Yang dipastikan task ini hanya satu hal: endpoint yang dilahirkannya
    /// memakai nama yang kontrak sebut.
    /// </para>
    /// <para>
    /// <b>Tidak ada field teks bebas untuk alasannya</b> (<c>INV-BD-016</c>, <c>VAL-BD-016</c>).
    /// Yang diterima hanya kode alasan dari <c>MstBloodBankReason</c> berkategori
    /// <c>AllocationCancellation</c>; teksnya disalin backend saat pembatalan supaya riwayat lama
    /// tidak berubah makna ketika master disunting.
    /// </para>
    /// <para>
    /// <b>Nol field yang boleh dipercaya dari klien selain kode alasan.</b> Pelaku, waktu, status
    /// tujuan, dan teks alasan seluruhnya ditentukan server.
    /// </para>
    /// </remarks>
    public class CancelWithReasonRequest
    {
        /// <summary>Kode alasan terkendali dari <c>MstBloodBankReason</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>Token konkurensi kantong yang dipegang layar.</summary>
        public int? Version { get; set; }
    }

    /// <summary>
    /// Satu baris riwayat alokasi kantong: disiapkan untuk baris kebutuhan siapa, sejak kapan, oleh
    /// siapa, dan — bila dibatalkan — dengan alasan apa.
    /// </summary>
    /// <remarks>
    /// Baris berstatus <c>Cancelled</c> <b>tetap</b> terbaca di sini. Itulah gunanya: pertanyaan
    /// "kantong ini pernah disiapkan untuk siapa saja" hanya dapat dijawab bila percobaan yang
    /// dibatalkan pun tersimpan (<c>ARCH-BD-POS-03</c>).
    /// </remarks>
    public class BloodUnitAllocationDto
    {
        public Guid Id { get; set; }
        public Guid BloodUnitId { get; set; }

        public Guid BloodOrderLineId { get; set; }

        /// <summary>Order asal baris kebutuhan, beserta nomor bisnisnya.</summary>
        public Guid? BloodOrderId { get; set; }
        public string? OrderNumber { get; set; }

        /// <summary>Nomor urut baris kebutuhan di dalam ordernya.</summary>
        public int? LineSequence { get; set; }

        public Guid? BloodComponentId { get; set; }
        public string? BloodComponentCode { get; set; }
        public string? BloodComponentName { get; set; }

        /// <summary>Pasien tujuan, dibaca lewat baris kebutuhan — tidak disalin ke tabel alokasi.</summary>
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }

        public BbkAllocationStatus AllocationStatus { get; set; }
        public string AllocationStatusLabel { get; set; } = string.Empty;

        public Guid AllocatedByUserId { get; set; }
        public DateTime AllocatedAt { get; set; }

        public string? CancelReasonCode { get; set; }
        public string? CancelReasonNote { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
