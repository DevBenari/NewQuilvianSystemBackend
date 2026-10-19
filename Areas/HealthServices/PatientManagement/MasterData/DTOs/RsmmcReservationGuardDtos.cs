namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    /// <summary>
    /// Keadaan guard reservasi nomor pasien normal beserta nomor yang <b>akan</b> diberikan
    /// pendaftaran berikutnya (`BE-PAT-MIG-003`, kontrak `1.2.1` B14.2, `PAT-GATE-005`).
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

        public bool OverallGuardReady { get; set; }

        public DateTime CheckedAtUtc { get; set; }
    }
}
