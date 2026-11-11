using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    /// <summary>
    /// Query indikator ketepatan tanda tangan ringkasan pulang Rawat Inap (`BE-49`, API `0.4.0` C1).
    /// </summary>
    public class DischargeSummaryTimelinessPagedQuery
    {
        /// <summary>Tanggal awal periode (kalender WIB, inklusif), menurut jam pasien pulang. Wajib.</summary>
        public DateOnly? PeriodFrom { get; set; }

        /// <summary>Tanggal akhir periode (kalender WIB, inklusif). Wajib; rentang paling lama 366 hari.</summary>
        public DateOnly? PeriodTo { get; set; }

        /// <summary>Hanya dokter ini. Dokter yang tidak dikenal menghasilkan daftar kosong.</summary>
        public Guid? DoctorId { get; set; }

        /// <summary>Hanya dokter dengan profil HR ini (`MstDoctor.WorkforceProfileId`).</summary>
        public Guid? WorkforceProfileId { get; set; }

        public int Page { get; set; } = 1;

        /// <summary>1 sampai 100.</summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Hitungan satu dokter (atau total). Penyebut adalah <see cref="AssessedCount"/>;
    /// <see cref="NotYetDueCount"/> tidak termasuk penyebut (`RM-DEC-106`).
    /// </summary>
    public class DischargeSummaryTimelinessCounts
    {
        /// <summary>Penyebut: seluruh ringkasan yang dinilai, yaitu semua selain yang belum jatuh tempo.</summary>
        public int AssessedCount { get; set; }

        /// <summary>Ditandatangani paling lama 24 jam sesudah pasien pulang (tanda tangan sebelum pulang = 0 jam).</summary>
        public int Within24h { get; set; }

        /// <summary>Lebih dari 24 jam sampai 48 jam, termasuk yang belum ditandatangani dan sudah tertunda selama itu.</summary>
        public int Between24And48h { get; set; }

        /// <summary>Lebih dari 48 jam, termasuk yang belum ditandatangani dan sudah tertunda selama itu.</summary>
        public int Over48h { get; set; }

        /// <summary>Bagian dari <see cref="Between24And48h"/> yang belum ditandatangani saat ditarik.</summary>
        public int UnsignedBetween24And48h { get; set; }

        /// <summary>Bagian dari <see cref="Over48h"/> yang belum ditandatangani saat ditarik.</summary>
        public int UnsignedOver48h { get; set; }

        /// <summary>Belum ditandatangani dan baru tertunda 24 jam atau kurang; tidak masuk penyebut.</summary>
        public int NotYetDueCount { get; set; }
    }

    /// <summary>Satu baris indikator per dokter. Tidak pernah memuat identitas pasien (`RM-DEC-101`).</summary>
    public class DischargeSummaryTimelinessItem : DischargeSummaryTimelinessCounts
    {
        /// <summary>Kosong = "Tanpa DPJP aktif": ringkasan belum ditandatangani dan episode tanpa DPJP aktif (`DS-R6-03`).</summary>
        public Guid? DoctorId { get; set; }

        /// <summary>Kunci profil HR dokter, untuk dipakai OPPE.</summary>
        public Guid? WorkforceProfileId { get; set; }

        public string DoctorName { get; set; } = string.Empty;
    }

    /// <summary>Keterangan mutu data yang wajib ikut setiap tarikan (`RM-AC-R20-08`).</summary>
    public class DischargeSummaryTimelinessDataNotes
    {
        /// <summary>Jumlah ringkasan yang jam mulainya memakai waktu tutup episode karena waktu keluar ruangan kosong (`RM-DEC-107`).</summary>
        public int ClosedAtFallbackCount { get; set; }

        /// <summary>Jumlah ringkasan yang ditandatangani sebelum pasien keluar; dihitung 0 jam.</summary>
        public int SignedBeforeDepartureCount { get; set; }

        /// <summary>Benar bila masih ada ringkasan belum ditandatangani pada periode ini, sehingga angka dapat berubah (`RM-DEC-106`).</summary>
        public bool PeriodStillChanging { get; set; }

        /// <summary>Batas pelacakan tanda tangan ulang pada data lama (`RM-DEC-105`).</summary>
        public string ResignTrackingNote { get; set; } = string.Empty;
    }

    /// <summary>Hasil indikator ketepatan ringkasan pulang per dokter (API `0.4.0` C1).</summary>
    public class DischargeSummaryTimelinessPagedResult : PagedResult<DischargeSummaryTimelinessItem>
    {
        public DateOnly PeriodFrom { get; set; }

        public DateOnly PeriodTo { get; set; }

        /// <summary>Waktu penarikan (UTC); dasar hitungan ringkasan yang belum ditandatangani.</summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>Jumlah seluruh dokter yang lolos saringan, bukan hanya halaman ini.</summary>
        public DischargeSummaryTimelinessCounts Totals { get; set; } = new();

        public DischargeSummaryTimelinessDataNotes DataNotes { get; set; } = new();
    }

    public class DischargeSummaryTimelinessBucketDefinition
    {
        public string Code { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class DischargeSummaryTimelinessFilterMetadataResponse
    {
        public List<DischargeSummaryTimelinessBucketDefinition> BucketDefinitions { get; set; } = new();

        /// <summary>Rentang periode terpanjang per tarikan (`DS-R6-01`).</summary>
        public int MaxPeriodDays { get; set; }
    }
}
