namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    /// <summary>
    /// Keadaan guard reservasi nomor pasien normal beserta nomor yang <b>akan</b> diberikan
    /// pendaftaran berikutnya (`BE-PAT-MIG-003`, kontrak `1.2.1` B14.2, `PAT-GATE-005`). Sejak
    /// kontrak `1.3.0` (`BE-PAT-MIG-003A`) juga melaporkan perlindungan pendaftaran bayi.
    /// </summary>
    /// <remarks>
    /// Baca-saja: tidak ada nomor yang dicadangkan dan tidak ada data pasien pada keluaran.
    /// </remarks>
    public class RsmmcReservationGuardStatusResponse
    {
        public string Environment { get; set; } = string.Empty;

        /// <summary><c>ACTIVE</c>, <c>DISABLED</c>, <c>CONFIG_MISSING</c>, <c>CONFIG_MALFORMED</c>, atau <c>NOT_APPLICABLE</c>.</summary>
        public string GuardState { get; set; } = string.Empty;

        public bool GuardEnabled { get; set; }

        public bool GuardConfigurationValid { get; set; }

        public long PatientCodeFloor { get; set; }

        public string PatientCodeFloorFormatted { get; set; } = string.Empty;

        public long MrnFloor { get; set; }

        public string MrnFloorFormatted { get; set; } = string.Empty;

        public long? NextPatientCodeNumeric { get; set; }

        public string? NextPatientCodeFormatted { get; set; }

        public long? NextMrnNumeric { get; set; }

        public string? NextMrnFormatted { get; set; }

        public bool PatientCodeFloorSatisfied { get; set; }

        public bool MrnFloorSatisfied { get; set; }

        /// <summary>
        /// Cara alokasi nomor pendaftaran pasien normal (<c>POST /</c>, <c>/kiosk</c>, <c>/admin</c>):
        /// <c>RESERVED_FLOOR</c>, <c>FAIL_CLOSED</c>, atau <c>LEGACY</c> (`BE-PAT-MIG-003A`).
        /// </summary>
        public string NormalPatientCreateAllocationMode { get; set; } = string.Empty;

        /// <summary>
        /// Pendaftaran pasien normal tidak dapat memakai nomor cadangan migrasi: memakai lantai, atau
        /// ditolak sebelum alokasi.
        /// </summary>
        public bool NormalPatientCreateProtected { get; set; }

        /// <summary>Cara alokasi nomor pendaftaran bayi (<c>POST /newborns</c>), nilai sama seperti di atas.</summary>
        public string NewbornPatientCreateAllocationMode { get; set; } = string.Empty;

        /// <summary>Pendaftaran bayi tidak dapat memakai nomor cadangan migrasi (`PAT-DEC-072`).</summary>
        public bool NewbornPatientCreateProtected { get; set; }

        /// <summary>
        /// Guard aktif dengan konfigurasi sah, <b>kedua</b> jalur memakai alokasi berlantai, dan kedua
        /// lantai terpenuhi. Tidak pernah <c>true</c> bila jalur bayi tidak terlindungi.
        /// </summary>
        public bool OverallGuardReady { get; set; }

        public DateTime CheckedAtUtc { get; set; }
    }
}
