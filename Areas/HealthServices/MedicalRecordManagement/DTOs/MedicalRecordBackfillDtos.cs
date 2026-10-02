namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    /// <summary>
    /// Hasil penelaahan data lama sebelum pengisian dijalankan.
    ///
    /// Seluruh angka di sini diperoleh dengan **hanya membaca**. Tidak ada satu baris pun yang
    /// diubah. Tujuannya menjawab pertanyaan yang tidak dapat dijawab dari source code: berapa
    /// banyak catatan lama yang ada, dan akan menjadi apa masing-masing.
    /// </summary>
    public class MedicalRecordBackfillSurveyResponse
    {
        /// <summary>Waktu penelaahan dijalankan.</summary>
        public DateTime SurveyedAt { get; set; }

        /// <summary>Seluruh CPPT yang tersimpan, termasuk yang sudah terdaftar keutuhan.</summary>
        public int TotalProgressNote { get; set; }

        /// <summary>CPPT yang sudah punya baris keutuhan. Tidak akan disentuh pengisian.</summary>
        public int SudahTerdaftar { get; set; }

        /// <summary>CPPT yang belum punya baris keutuhan. Inilah yang akan diproses.</summary>
        public int BelumTerdaftar { get; set; }

        /// <summary>
        /// Akan ditandai terkunci tanpa tanda tangan, karena kunjungannya sudah selesai atau
        /// batal. Inilah angka yang akan muncul besar pada laporan kelengkapan sejak hari
        /// pertama.
        /// </summary>
        public int AkanTerkunciTanpaTandaTangan { get; set; }

        /// <summary>Akan tetap terbuka, karena kunjungannya masih berjalan.</summary>
        public int AkanTetapDraf { get; set; }

        /// <summary>Akan ditandai dibatalkan, karena catatannya memang sudah dibatalkan.</summary>
        public int AkanDitandaiDibatalkan { get; set; }

        /// <summary>
        /// CPPT yang penulisnya tidak tercatat. Barisnya tetap dibuat dengan penanda penulis
        /// tidak diketahui, tidak dilewati diam-diam.
        /// </summary>
        public int PenulisTidakDiketahui { get; set; }

        /// <summary>
        /// CPPT yang tidak melekat ke kunjungan mana pun. **Tidak dapat didaftarkan**, karena
        /// baris keutuhan mensyaratkan kunjungan sebagai pengelompokannya.
        /// </summary>
        public int TanpaKunjungan { get; set; }

        /// <summary>Catatan tertua yang akan diproses. Membantu menilai rentang waktunya.</summary>
        public DateTime? CatatanTertua { get; set; }

        public DateTime? CatatanTerbaru { get; set; }

        /// <summary>
        /// Perkiraan banyaknya potongan yang akan dijalankan, memakai ukuran potongan yang
        /// diminta. Membantu memperkirakan lama proses.
        /// </summary>
        public int PerkiraanJumlahPotongan { get; set; }

        /// <summary>
        /// Peringatan yang perlu dibaca sebelum menjalankan pengisian. Kosong bila tidak ada.
        /// </summary>
        public List<string> Peringatan { get; set; } = [];
    }

    /// <summary>
    /// Hasil satu kali penjalanan pengisian data lama.
    /// </summary>
    public class MedicalRecordBackfillRunResponse
    {
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }

        /// <summary>Benar bila dijalankan sebagai percobaan, tanpa menyimpan apa pun.</summary>
        public bool IsDryRun { get; set; }

        public int JumlahDiproses { get; set; }
        public int JumlahTerkunciTanpaTandaTangan { get; set; }
        public int JumlahTetapDraf { get; set; }
        public int JumlahDitandaiDibatalkan { get; set; }
        public int JumlahPenulisTidakDiketahui { get; set; }
        public int JumlahDilewatiTanpaKunjungan { get; set; }

        /// <summary>
        /// Benar bila masih ada sisa yang belum diproses. Pengisian dapat dijalankan lagi untuk
        /// melanjutkan.
        /// </summary>
        public bool MasihAdaSisa { get; set; }

        public int PerkiraanSisa { get; set; }
    }
}
