namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Konfigurasi antrean Admisi Rawat Jalan, section
    /// <c>HealthServices:Registration:AdmissionQueue</c> (blueprint Cetak Antrean Admisi v1.1
    /// bagian 5 dan 6).
    /// </summary>
    /// <remarks>
    /// Bawaan bagian 5: satu kelompok <c>Admisi Rawat Jalan Reguler</c> berawalan <c>A</c> yang
    /// dilayani satu loket. Rumah sakit yang memisahkan loket menurut penjamin menambah kelompok
    /// di sini; kiosk menampilkan pilihan hanya bila kelompok aktif lebih dari satu.
    /// </remarks>
    public class AdmissionQueueOptions
    {
        public const string SectionName = "HealthServices:Registration:AdmissionQueue";

        public const string DefaultGroupCode = "REGULER";
        public const string DefaultGroupName = "Admisi Rawat Jalan Reguler";
        public const string DefaultGroupPrefix = "A";
        public const string DefaultCounterCode = "LOKET-1";
        public const string DefaultCounterName = "Loket 1";
        public const string DefaultWaitingAreaName = "area Admisi Rawat Jalan";

        /// <summary>Jumlah digit nomor tampil, contoh 3 menghasilkan <c>A-015</c>.</summary>
        public const int DisplaySequenceDigits = 3;

        public bool Enabled { get; set; } = true;

        /// <summary>Nama rumah sakit pada kop tiket thermal. Kosong berarti kop memakai milik kiosk.</summary>
        public string? HospitalName { get; set; }

        /// <summary>Tempat menunggu yang dicetak pada tiket.</summary>
        public string WaitingAreaName { get; set; } = DefaultWaitingAreaName;

        /// <summary>
        /// Rumah sakit melayani BPJS. Bila <c>false</c>, kelompok yang ditandai
        /// <see cref="AdmissionQueueGroupOptions.RequiresBpjsService"/> tidak ditampilkan (UAT-14).
        /// </summary>
        public bool IsBpjsServiceAvailable { get; set; } = true;

        public List<AdmissionQueueGroupOptions> Groups { get; set; } = new();

        public List<AdmissionQueueCounterOptions> Counters { get; set; } = new();

        /// <summary>Kelompok yang aktif dan berlaku bagi rumah sakit ini; bawaan satu kelompok reguler.</summary>
        public IReadOnlyList<AdmissionQueueGroupOptions> ResolveActiveGroups()
        {
            var groups = Groups
                .Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.Code))
                .Where(x => IsBpjsServiceAvailable || !x.RequiresBpjsService)
                .ToList();

            if (groups.Count > 0)
                return groups;

            return new List<AdmissionQueueGroupOptions>
            {
                new()
                {
                    Code = DefaultGroupCode,
                    Name = DefaultGroupName,
                    Prefix = DefaultGroupPrefix,
                    IsActive = true
                }
            };
        }

        public AdmissionQueueGroupOptions? FindActiveGroup(string? code) =>
            ResolveActiveGroups().FirstOrDefault(x =>
                string.Equals(x.Code.Trim(), code?.Trim(), StringComparison.OrdinalIgnoreCase));

        /// <summary>Loket aktif; bawaan satu loket yang melayani seluruh kelompok.</summary>
        public IReadOnlyList<AdmissionQueueCounterOptions> ResolveActiveCounters()
        {
            var counters = Counters
                .Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.Code))
                .ToList();

            if (counters.Count > 0)
                return counters;

            return new List<AdmissionQueueCounterOptions>
            {
                new()
                {
                    Code = DefaultCounterCode,
                    Name = DefaultCounterName,
                    IsActive = true
                }
            };
        }

        public AdmissionQueueCounterOptions? FindActiveCounter(string? code) =>
            ResolveActiveCounters().FirstOrDefault(x =>
                string.Equals(x.Code.Trim(), code?.Trim(), StringComparison.OrdinalIgnoreCase));

        public string ResolveWaitingAreaName() =>
            string.IsNullOrWhiteSpace(WaitingAreaName) ? DefaultWaitingAreaName : WaitingAreaName.Trim();

        /// <summary>Pemeriksaan saat aplikasi mulai: kode dan awalan kelompok aktif tidak boleh kembar.</summary>
        public bool IsValid()
        {
            var groups = ResolveActiveGroups();
            var counters = ResolveActiveCounters();

            if (groups.Any(x => string.IsNullOrWhiteSpace(x.Prefix) ||
                                x.Prefix.Trim().Length > 5 ||
                                !x.Prefix.Trim().All(char.IsLetter)))
                return false;

            if (groups.Select(x => x.Code.Trim().ToUpperInvariant()).Distinct().Count() != groups.Count)
                return false;

            if (groups.Select(x => x.Prefix.Trim().ToUpperInvariant()).Distinct().Count() != groups.Count)
                return false;

            return counters.Select(x => x.Code.Trim().ToUpperInvariant()).Distinct().Count() == counters.Count;
        }
    }

    public class AdmissionQueueGroupOptions
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Awalan nomor tampil, 1–5 huruf, contoh <c>A</c> atau <c>B</c>.</summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>Keterangan singkat pada tombol pilihan kiosk.</summary>
        public string? Description { get; set; }

        /// <summary>Kelompok khusus loket BPJS; disembunyikan bila rumah sakit tidak melayani BPJS.</summary>
        public bool RequiresBpjsService { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class AdmissionQueueCounterOptions
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Kelompok yang dilayani loket ini. Kosong berarti seluruh kelompok aktif.</summary>
        public List<string> GroupCodes { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public bool Serves(string groupCode) =>
            GroupCodes.Count == 0 ||
            GroupCodes.Any(x => string.Equals(x.Trim(), groupCode.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
