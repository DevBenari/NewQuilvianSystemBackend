using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Isian pencatatan High/Low Dependency (BE-RWI-234, API 14.3). Sumber pencatatan ditentukan
    /// server dari peran pencatat, tidak dikirim klien.
    /// </summary>
    /// <remarks>
    /// <para><b>Level</b>: 1 = High, 2 = Low. Nilai lain ditolak 400 <c>CARE_DEPENDENCY_LEVEL_INVALID</c>.</para>
    /// <para>
    /// <b>ExpectedSetAt</b>: waktu pencatatan terakhir yang dilihat layar (<c>CareDependencySetAt</c>
    /// pada detail episode), atau kosong bila layar melihat "belum dinilai". Bila nilainya sudah
    /// berubah, permintaan ditolak 409 <c>STALE_CARE_DEPENDENCY</c>.
    /// </para>
    /// </remarks>
    public class SetCareDependencyRequest
    {
        public int Level { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime? ExpectedSetAt { get; set; }
    }

    /// <summary>Satu baris riwayat High/Low Dependency, terbaru dulu.</summary>
    public class CareDependencyHistoryResponse
    {
        public Guid Id { get; set; }

        public int? PreviousLevel { get; set; }

        public string? PreviousLevelName { get; set; }

        public int NewLevel { get; set; }

        public string NewLevelName { get; set; } = string.Empty;

        public int Source { get; set; }

        public string SourceName { get; set; } = string.Empty;

        /// <summary>Nama pencatat, bukan Guid.</summary>
        public string? ActorName { get; set; }

        public DateTime RecordedAtUtc { get; set; }

        /// <summary>Catatan pencatat. Kolom sensitif; hanya bagi pembaca episode berwenang.</summary>
        public string? Note { get; set; }
    }
}
