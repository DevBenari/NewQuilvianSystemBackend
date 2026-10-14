namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Kueri pemeriksaan kelayakan admisi (<c>BE-RWI-230</c>, <c>ISSUE-EPS-010</c>).
    /// </summary>
    /// <remarks>
    /// <c>AdmissionReferralId</c> diisi jalur Kamar Pulih dengan permintaan yang sedang dipakai,
    /// supaya permintaan itu sendiri tidak terbaca sebagai penghalang. Jalur Pasien Lama
    /// mengosongkannya.
    /// </remarks>
    public class InpatientAdmissionEligibilityQuery
    {
        public Guid PatientId { get; set; }

        public Guid? AdmissionReferralId { get; set; }
    }

    /// <summary>
    /// Jawaban "bolehkah pasien ini dibukakan admisi baru sekarang", dibaca langkah 1 admisi
    /// sebelum petugas mengisi langkah berikutnya.
    /// </summary>
    /// <remarks>
    /// Setiap penghalang berasal dari aturan yang sama dengan penolakan tulisnya, lengkap dengan
    /// kalimatnya: episode yang hadir (<c>INV-INP-10</c>), permintaan Kamar Pulih yang menunggu
    /// (<c>VAL-RWF-87</c>), dan kunjungan rawat jalan aktif pendaftaran (RJ-DOC-REV-BE-007).
    /// Admisi lain yang masih <c>Draft</c> hanya peringatan, sesuai keputusan "Draft ganda bukan
    /// penolakan".
    /// </remarks>
    public class InpatientAdmissionEligibilityResponse
    {
        public Guid PatientId { get; set; }

        /// <summary><c>true</c> bila tidak ada satu pun penghalang.</summary>
        public bool CanContinue { get; set; }

        /// <summary>Kode penghalang pertama; <c>null</c> bila boleh lanjut.</summary>
        public string? BlockingCode { get; set; }

        /// <summary>Kalimat penghalang pertama, sama persis dengan kalimat penolakan tulisnya.</summary>
        public string? Message { get; set; }

        /// <summary>Seluruh penghalang, berurutan dari yang paling menentukan.</summary>
        public List<InpatientAdmissionEligibilityBlockerResponse> Blockers { get; set; } = new();

        /// <summary>Episode yang sedang hadir di ruangan; jalan keluarnya perpindahan tempat tidur.</summary>
        public InpatientAdmissionEligibilityEpisodeResponse? PresentEpisode { get; set; }

        /// <summary>Kunjungan rawat jalan yang masih menghalangi pendaftaran.</summary>
        public InpatientAdmissionEligibilityEncounterResponse? ActiveEncounter { get; set; }

        /// <summary>Admisi lain yang masih disiapkan; peringatan, bukan penghalang.</summary>
        public InpatientAdmissionEligibilityEpisodeResponse? DraftEpisode { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    public class InpatientAdmissionEligibilityBlockerResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

    public class InpatientAdmissionEligibilityEpisodeResponse
    {
        /// <summary>Hanya untuk tautan Detail Episode / Lanjutkan Admisi; tidak untuk ditampilkan.</summary>
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        /// <summary>Lokasi terbaca, misalnya <c>Kamar 02 BED 001</c>; hanya pada episode yang hadir.</summary>
        public string? LocationText { get; set; }

        /// <summary>Waktu admisi dibuka; hanya pada episode <c>Draft</c>.</summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>Episode <c>Draft</c> masih memegang pemesanan tempat tidur yang berlaku.</summary>
        public bool HasActiveReservation { get; set; }
    }

    public class InpatientAdmissionEligibilityEncounterResponse
    {
        public string EncounterNumber { get; set; } = string.Empty;

        public DateTime EncounterDate { get; set; }
    }
}
