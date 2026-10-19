namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Konfigurasi Reservasi Prioritas, section <c>HealthServices:Registration:PriorityReservation</c>
    /// (rawat-jalan Amendment RP, <c>02-backend-architecture.md</c> RP.11).
    /// </summary>
    public class PriorityReservationOptions
    {
        public const string SectionName = "HealthServices:Registration:PriorityReservation";

        public const string DefaultPriorityWaitingRoomName = "Ruang Tunggu Prioritas";

        public bool Enabled { get; set; } = true;

        /// <summary>Batas hari ke depan untuk tanggal kunjungan (RJ-PRI-OQ-012, usulan bawaan 30).</summary>
        public int MaxDaysAhead { get; set; } = 30;

        /// <summary>Nama satu ruang tunggu prioritas untuk semua poli (RJ-PRI-DEC-018).</summary>
        public string PriorityWaitingRoomName { get; set; } = DefaultPriorityWaitingRoomName;

        /// <summary>Alamat dasar halaman publik konfirmasi; link = alamat ini + "/" + kode.</summary>
        public string PublicArrivalBaseUrl { get; set; } = "http://localhost:3000/priority-arrival";

        /// <summary>Batas permintaan link publik per menit per IP (RJ-PRI-OQ-003, NFR-RP-007).</summary>
        public int ArrivalLinkPermitPerMinute { get; set; } = 20;

        public PriorityReservationClosureOptions Closure { get; set; } = new();

        public string ResolvePriorityWaitingRoomName() =>
            string.IsNullOrWhiteSpace(PriorityWaitingRoomName)
                ? DefaultPriorityWaitingRoomName
                : PriorityWaitingRoomName.Trim();

        /// <summary>Penanda lokasi untuk respons antrean (RJ-PRI-DEC-018): terisi hanya untuk antrean prioritas.</summary>
        public string? ResolveWaitingLocationFor(bool isPriorityQueue) =>
            isPriorityQueue ? ResolvePriorityWaitingRoomName() : null;
    }

    /// <summary>Penutupan otomatis reservasi yang tidak datang (RJ-PRI-DEC-013).</summary>
    public class PriorityReservationClosureOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>Jam lokal WIB proses harian, format HH:mm.</summary>
        public string RunAtLocalTime { get; set; } = "00:05";
    }
}
