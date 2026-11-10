using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;
using P = QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Constants.OpticalPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>Hasil aturan isian resep: galat keras pertama, atau daftar ruas yang hanya melewati rentang.</summary>
    public sealed record OptPrescriptionRuleResult(string? Code, string? Message, IReadOnlyList<string> OutOfRange)
    {
        public bool HasError => Code is not null;

        /// <summary>Kalimat <c>OPT-VAL-004</c> untuk ruas pertama yang melewati rentang.</summary>
        public string? OutOfRangeMessage => OutOfRange.Count == 0
            ? null
            : $"{OutOfRange[0]} di luar rentang {OptPrescriptionRules.RangeText(OutOfRange[0])}; hanya Sp.M yang dapat mengonfirmasi.";
    }

    /// <summary>
    /// Aturan isian resep (<c>OPT-DEC-045</c>, <c>OPT-VAL-010</c>..<c>018</c>) dan transposisi silinder plus → minus
    /// (<c>OPT-VAL-016</c>). Cermin <c>optical-prescription-rules.js</c> (<c>FE-OPT-001</c>): kalimat dan pembagian
    /// "galat keras" vs "hanya di luar rentang" sama persis.
    /// </summary>
    /// <remarks>
    /// <b>Galat keras</b> (<c>400</c>, tidak bisa dikonfirmasi siapa pun): bukan kelipatan 0,25, CYL plus, aturan AXIS,
    /// PD di luar 50..80, kelengkapan lensa kontak, dan BC/DIA di luar pengaturan. <b>Di luar rentang</b> (boleh
    /// dikonfirmasi Sp.M dengan alasan): SPH/power di luar ±20,00, CYL di bawah −6,00, ADD di luar +0,75..+3,50.
    /// </remarks>
    public static class OptPrescriptionRules
    {
        public const string SphLabel = "SPH";
        public const string CylLabel = "CYL";
        public const string AddLabel = "ADD";
        public const string ContactLensPowerLabel = "Power lensa kontak";

        public const string SphMessage = "SPH harus kelipatan 0,25 antara −20,00 dan +20,00.";
        public const string CylMessage = "CYL harus notasi minus, kelipatan 0,25, antara −6,00 dan 0,00.";
        public const string AxisRequiredMessage = "AXIS wajib diisi bila ada silinder.";
        public const string AxisRangeMessage = "AXIS harus 0–180 dan hanya diisi bila ada silinder.";
        public const string AddMessage = "ADD harus kelipatan 0,25 antara +0,75 dan +3,50.";
        public const string PdMessage = "PD harus antara 50 dan 80 mm.";
        public const string ContactLensPowerMessage = "Power lensa kontak harus kelipatan 0,25 antara −20,00 dan +20,00.";
        public const string ContactLensCompleteMessage = "Resep lensa kontak wajib berisi power, BC, DIA, dan merk/jenis.";
        public const string NoEyeMessage = "Isi ukuran minimal satu mata.";

        public static string RangeText(string label) => label switch
        {
            CylLabel => "−6,00..0,00",
            AddLabel => "+0,75..+3,50",
            _ => "−20,00..+20,00",
        };

        /// <summary>Memeriksa isi resep sesuai jenisnya. BC/DIA dibandingkan dengan rentang pengaturan Optik.</summary>
        public static OptPrescriptionRuleResult Validate(OptPrescriptionType type, OptPrescriptionValues v, OptSetting setting)
        {
            var luar = new List<string>();
            var galat = type == OptPrescriptionType.ContactLens
                ? ValidateContactLens(v, setting, luar)
                : ValidateSpectacle(v, luar);

            return galat is { } g
                ? new OptPrescriptionRuleResult(g.Code, g.Message, Array.Empty<string>())
                : new OptPrescriptionRuleResult(null, null, luar);
        }

        private static (string Code, string Message)? ValidateSpectacle(OptPrescriptionValues v, List<string> luar)
        {
            var od = IsUsed(v.Od);
            var os = IsUsed(v.Os);
            if (!od && !os)
                return (OptErrorCodes.InvalidRequest, NoEyeMessage);

            return (od ? ValidateSpectacleEye("kanan", v.Od!, luar) : null)
                ?? (os ? ValidateSpectacleEye("kiri", v.Os!, luar) : null)
                ?? ValidatePd(v.PdDistance)
                ?? ValidatePd(v.PdNear);
        }

        private static (string Code, string Message)? ValidateSpectacleEye(string mata, OptLensPowerDto eye, List<string> luar)
        {
            if (eye.Sph is not { } sph)
                return (OptErrorCodes.Val010, $"SPH mata {mata} wajib diisi; tulis 0 untuk plano.");
            if (!IsQuarter(sph))
                return (OptErrorCodes.Val010, SphMessage);
            if (Math.Abs(sph) > 20m)
                Add(luar, SphLabel);

            return ValidateCylAxis(eye.Cyl, eye.Axis, luar) ?? ValidateAdd(eye.Add, luar);
        }

        private static (string Code, string Message)? ValidateCylAxis(decimal? cyl, int? axis, List<string> luar)
        {
            if (cyl is { } c)
            {
                if (c > 0m || !IsQuarter(c))
                    return (OptErrorCodes.Val011, CylMessage);
                if (c < -6m)
                    Add(luar, CylLabel);
            }

            var adaSilinder = cyl is { } s && s != 0m;
            if (axis is null)
                return adaSilinder ? (OptErrorCodes.Val012, AxisRequiredMessage) : null;
            return axis < 0 || axis > 180 || !adaSilinder ? (OptErrorCodes.Val013, AxisRangeMessage) : null;
        }

        private static (string Code, string Message)? ValidateAdd(decimal? add, List<string> luar)
        {
            if (add is not { } a)
                return null;
            if (!IsQuarter(a))
                return (OptErrorCodes.Val014, AddMessage);
            if (a < 0.75m || a > 3.5m)
                Add(luar, AddLabel);
            return null;
        }

        private static (string Code, string Message)? ValidatePd(decimal? pd) =>
            pd is { } x && (x < 50m || x > 80m || decimal.Round(x, 1) != x) ? (OptErrorCodes.Val015, PdMessage) : null;

        private static (string Code, string Message)? ValidateContactLens(OptPrescriptionValues v, OptSetting setting, List<string> luar)
        {
            var od = IsUsed(v.ContactLensOd);
            var os = IsUsed(v.ContactLensOs);
            if (!od && !os)
                return (OptErrorCodes.InvalidRequest, NoEyeMessage);
            if (string.IsNullOrWhiteSpace(v.ContactLensBrandOrType))
                return (OptErrorCodes.Val017, ContactLensCompleteMessage);

            return (od ? ValidateContactLensEye(v.ContactLensOd!, setting, luar) : null)
                ?? (os ? ValidateContactLensEye(v.ContactLensOs!, setting, luar) : null);
        }

        private static (string Code, string Message)? ValidateContactLensEye(OptContactLensEyeDto eye, OptSetting setting, List<string> luar)
        {
            if (eye.Power is not { } power || eye.BaseCurve is not { } bc || eye.Diameter is not { } dia)
                return (OptErrorCodes.Val017, ContactLensCompleteMessage);
            if (!IsQuarter(power))
                return (OptErrorCodes.Val010, ContactLensPowerMessage);
            if (Math.Abs(power) > 20m)
                Add(luar, ContactLensPowerLabel);

            var cylAxis = ValidateCylAxis(eye.Cyl, eye.Axis, luar);
            if (cylAxis is not null)
                return cylAxis;

            if (bc < setting.ContactLensBaseCurveMin || bc > setting.ContactLensBaseCurveMax || decimal.Round(bc, 2) != bc)
                return (OptErrorCodes.Val018, $"BC harus antara {Format2(setting.ContactLensBaseCurveMin)} dan {Format2(setting.ContactLensBaseCurveMax)}.");
            if (dia < setting.ContactLensDiameterMin || dia > setting.ContactLensDiameterMax || decimal.Round(dia, 2) != dia)
                return (OptErrorCodes.Val018, $"DIA harus antara {Format2(setting.ContactLensDiameterMin)} dan {Format2(setting.ContactLensDiameterMax)}.");
            return null;
        }

        /// <summary>
        /// Transposisi resep luar kacamata (<c>OPT-VAL-016</c>): mata ber-CYL plus menjadi SPH + CYL / −CYL × (AXIS ± 90).
        /// AXIS ≤ 90 ditambah 90, selain itu dikurangi 90 (sama dengan <c>transposeToMinusCylinder</c>).
        /// <b>Contoh:</b> +1,00 / +0,50 × 90 → +1,50 / −0,50 × 180. Mengembalikan galat bila CYL plus tanpa AXIS sah.
        /// </summary>
        public static (string Code, string Message)? TransposeForCopy(OptPrescriptionValues v, out bool isTransposed, out string? originalText)
        {
            isTransposed = false;
            originalText = null;

            var asli = new List<string>();
            if (IsUsed(v.Od)) asli.Add("OD " + Notation(v.Od!.Sph, v.Od.Cyl, v.Od.Axis));
            if (IsUsed(v.Os)) asli.Add("OS " + Notation(v.Os!.Sph, v.Os.Cyl, v.Os.Axis));

            foreach (var eye in new[] { v.Od, v.Os })
            {
                if (eye?.Cyl is not { } cyl || cyl <= 0m)
                    continue;
                if (eye.Axis is not { } axis)
                    return (OptErrorCodes.Val012, AxisRequiredMessage);
                if (axis < 0 || axis > 180)
                    return (OptErrorCodes.Val013, AxisRangeMessage);

                eye.Sph = (eye.Sph ?? 0m) + cyl;
                eye.Cyl = -cyl;
                eye.Axis = axis <= 90 ? axis + 90 : axis - 90;
                isTransposed = true;
            }

            if (isTransposed)
                originalText = string.Join("; ", asli);
            return null;
        }

        /// <summary>Batas berlaku: tanggal terbit ditambah usia resep (bulan). Berlaku sampai tanggal itu (inklusif).</summary>
        public static DateOnly ValidUntil(DateOnly issuedDate, int validityMonths) => issuedDate.AddMonths(validityMonths);

        /// <summary>Tampilan ukur bertanda dengan koma: −1,50 / +0,75 / 0,00 (sama dengan <c>formatOpticalPower</c>).</summary>
        public static string Format(decimal value)
        {
            var nilai = decimal.Round(value, 2);
            var angka = Format2(Math.Abs(nilai));
            return nilai == 0m ? angka : (nilai < 0m ? "−" : "+") + angka;
        }

        /// <summary>Notasi satu mata: "+1,00 / +0,50 × 90" (sama dengan <c>formatPrescriptionNotation</c>).</summary>
        public static string Notation(decimal? sph, decimal? cyl, int? axis)
        {
            var teks = sph is { } s ? Format(s) : "—";
            if (cyl is { } c && c != 0m)
                teks += $" / {Format(c)} × {axis}";
            return teks;
        }

        /// <summary>Ringkasan satu versi untuk daftar dan pilihan pesanan, contoh "OD −1,50 / −0,50 × 180; OS −1,75".</summary>
        public static string Summary(OptPrescriptionType type, OptPrescriptionVersion v)
        {
            var bagian = new List<string>();
            if (type == OptPrescriptionType.ContactLens)
            {
                void Lensa(string mata, decimal? power, decimal? cyl, int? axis, decimal? bc, decimal? dia)
                {
                    if (power is null && bc is null && dia is null) return;
                    var teks = $"{mata} {Notation(power, cyl, axis)}";
                    if (bc is { } b) teks += $" BC {Format2(b)}";
                    if (dia is { } d) teks += $" DIA {Format2(d)}";
                    bagian.Add(teks);
                }

                Lensa("OD", v.OdClPower, v.OdClCyl, v.OdClAxis, v.OdClBaseCurve, v.OdClDiameter);
                Lensa("OS", v.OsClPower, v.OsClCyl, v.OsClAxis, v.OsClBaseCurve, v.OsClDiameter);
                if (!string.IsNullOrWhiteSpace(v.ContactLensBrandOrType))
                    bagian.Add(string.IsNullOrWhiteSpace(v.ContactLensReplacement)
                        ? v.ContactLensBrandOrType!
                        : $"{v.ContactLensBrandOrType} ({v.ContactLensReplacement})");
            }
            else
            {
                void Kacamata(string mata, decimal? sph, decimal? cyl, int? axis, decimal? add)
                {
                    if (sph is null && cyl is null && add is null) return;
                    var teks = $"{mata} {Notation(sph, cyl, axis)}";
                    if (add is { } a) teks += $" ADD {Format(a)}";
                    bagian.Add(teks);
                }

                Kacamata("OD", v.OdSph, v.OdCyl, v.OdAxis, v.OdAdd);
                Kacamata("OS", v.OsSph, v.OsCyl, v.OsAxis, v.OsAdd);
                if (v.PdDistance is { } pd)
                    bagian.Add($"PD {pd.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',')}");
            }

            return string.Join("; ", bagian);
        }

        private static bool IsUsed(OptLensPowerDto? e) => e is not null && (e.Sph is not null || e.Cyl is not null || e.Axis is not null || e.Add is not null);

        private static bool IsUsed(OptContactLensEyeDto? e) =>
            e is not null && (e.Power is not null || e.Cyl is not null || e.Axis is not null || e.BaseCurve is not null || e.Diameter is not null);

        private static bool IsQuarter(decimal value) => decimal.Remainder(value * 4m, 1m) == 0m;

        private static void Add(List<string> luar, string label)
        {
            if (!luar.Contains(label)) luar.Add(label);
        }

        private static string Format2(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
    }

    /// <summary>
    /// Resep Optik (<c>BE-OPT-012</c>, <c>OPT-API-v1</c> grup Opt Prescription, <c>OPT-STATE-v1</c> bagian 2,
    /// <c>OPT-VAL-v1</c> bagian 2).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Tiga jalan masuk:</b> resep internal dari pemeriksaan final (<c>OPT-DEC-039</c>: Sp.M pun memakai form ini),
    /// salin resep luar dengan transposisi (<c>OPT-DEC-017</c>, <c>045</c>), dan revisi berversi selama belum terkunci
    /// (<c>OPT-DEC-007</c>, <c>011</c>). Versi tidak pernah diubah; revisi menambah versi.
    /// </para>
    /// <para>
    /// <b>Terkunci dan kedaluwarsa adalah sifat turunan.</b> <see cref="OptPrescription.LockedAt"/> diisi pesanan saat
    /// masuk pengerjaan (task pesanan), dan <see cref="OptPrescription.ValidUntil"/> dibekukan dari pengaturan saat
    /// resep dibuat. <see cref="CheckUsableForOrderAsync"/> dipakai pesanan untuk <c>OPT-VAL-023</c>.
    /// </para>
    /// <para>
    /// <b>Lewat rentang</b> hanya boleh oleh Sp.M yang memegang <c>OptPrescription : OverrideRange</c>, dengan alasan
    /// (<c>OPT-VAL-004</c>, <c>005</c>). Hak itu dicek di sini lewat <see cref="HasOverrideRangePermissionAsync"/>.
    /// </para>
    /// <para>
    /// <b>Kunci Rekam Medis</b> (<c>BE-OPT-013</c>, <c>OPT-DEC-050</c>): setiap versi resep didaftarkan sebagai dokumen
    /// <see cref="ClinicalDocumentKind.OpticalPrescription"/> bertanda tangan pada <c>SaveChanges</c> yang sama.
    /// </para>
    /// </remarks>
    public class OptPrescriptionService
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;

        public const string IdempotencyKeyRequiredMessage = "Header Idempotency-Key wajib diisi agar Simpan dua kali tidak membuat resep ganda.";
        public const string NotFoundMessage = "Resep tidak ditemukan.";
        public const string LockedMessage = "Resep terkunci karena lensa sudah dipesan.";
        public const string RevisionReasonMessage = "Tuliskan alasan revisi.";
        public const string OverrideReasonMessage = "Tuliskan alasan konfirmasi ukuran di luar rentang.";
        public const string ExternalIncompleteMessage = "Lengkapi penulis, asal fasilitas, dan tanggal resep luar.";
        public const string FutureDateMessage = "Tanggal resep tidak boleh melewati hari ini.";
        public const string StaleMessage = "Resep sudah diubah petugas lain. Muat ulang halaman.";
        public const string ContactLensExpiredMessage = "Resep lensa kontak kedaluwarsa, lakukan pemeriksaan ulang.";

        private readonly ApplicationDbContext _dbContext;
        private readonly OptPractitionerGuard _guard;
        private readonly OptNumberService _numberService;
        private readonly OptVisitService _visitService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly ClinicalDocumentIntegrityService _integrity;
        private readonly TimeProvider _time;

        public const ClinicalDocumentKind DocumentKind = ClinicalDocumentKind.OpticalPrescription;

        public OptPrescriptionService(
            ApplicationDbContext dbContext,
            OptPractitionerGuard guard,
            OptNumberService numberService,
            OptVisitService visitService,
            IHttpContextAccessor httpContextAccessor,
            AccessPermissionService accessPermissionService,
            ClinicalDocumentIntegrityService integrity,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _guard = guard;
            _numberService = numberService;
            _visitService = visitService;
            _httpContextAccessor = httpContextAccessor;
            _accessPermissionService = accessPermissionService;
            _integrity = integrity;
            _time = timeProvider ?? TimeProvider.System;
        }

        // ---------------------------------------------------------------- baca

        public async Task<PagedResult<OptPrescriptionResponse>> GetListAsync(OptPrescriptionListQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);
            var today = DateOnly.FromDateTime(Today());

            var rows = _dbContext.OptPrescriptions.AsNoTracking().Where(x => !x.IsDelete);
            if (query.PatientId is { } patientId) rows = rows.Where(x => x.PatientId == patientId);
            if (query.EncounterId is { } encounterId) rows = rows.Where(x => x.EncounterId == encounterId);
            if (query.PrescriptionType is { } type) rows = rows.Where(x => x.PrescriptionType == type);
            if (query.ValidOnly == true) rows = rows.Where(x => x.ValidUntil >= today);

            var total = await rows.CountAsync(cancellationToken);
            var page = await rows
                .OrderByDescending(x => x.IssuedDate).ThenByDescending(x => x.PrescriptionNumber)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);

            var currents = await CurrentVersionsAsync(page, cancellationToken);
            var names = await NamesAsync(currents.Values.Select(v => v.AuthorWorkforceProfileId), cancellationToken);

            return new PagedResult<OptPrescriptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = page.Select(x => Project(x, currents.TryGetValue(x.Id, out var v) ? [v] : [], names, today, includeAll: false)).ToList(),
            };
        }

        public async Task<OptResult<OptPrescriptionResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var prescription = await _dbContext.OptPrescriptions.AsNoTracking()
                .Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return prescription is null
                ? OptResult<OptPrescriptionResponse>.NotFound(NotFoundMessage)
                : OptResult<OptPrescriptionResponse>.Ok(await ProjectAsync(prescription, cancellationToken));
        }

        /// <summary>Resep pasien yang masih berlaku hari ini (tanggal WIB), terbaru dulu.</summary>
        public async Task<List<OptPrescriptionOptionResponse>> GetOptionsAsync(Guid patientId, OptPrescriptionType? type, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(Today());
            var rows = await _dbContext.OptPrescriptions.AsNoTracking()
                .Where(x => x.PatientId == patientId && !x.IsDelete && x.ValidUntil >= today && (type == null || x.PrescriptionType == type))
                .OrderByDescending(x => x.IssuedDate).ThenByDescending(x => x.PrescriptionNumber)
                .ToListAsync(cancellationToken);
            var currents = await CurrentVersionsAsync(rows, cancellationToken);

            return rows.Select(x => new OptPrescriptionOptionResponse
            {
                Id = x.Id,
                PrescriptionNumber = x.PrescriptionNumber,
                PrescriptionType = x.PrescriptionType,
                Origin = x.Origin,
                IssuedDate = x.IssuedDate,
                ValidUntil = x.ValidUntil,
                IsLocked = x.LockedAt != null,
                CurrentVersionNo = x.CurrentVersionNo,
                Summary = currents.TryGetValue(x.Id, out var v) ? OptPrescriptionRules.Summary(x.PrescriptionType, v) : string.Empty,
            }).ToList();
        }

        /// <summary>Apakah pengguna saat ini boleh mengonfirmasi ukuran di luar rentang (layar menampilkan isian alasan).</summary>
        public async Task<OptResult<OptRangeOverrideEligibilityResponse>> GetRangeOverrideEligibilityAsync(Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (practitioner.Kind == OptResultKind.Unavailable)
                return OptResult<OptRangeOverrideEligibilityResponse>.Unavailable(practitioner.Code!, practitioner.Message!);

            var response = !practitioner.IsSuccess
                ? new OptRangeOverrideEligibilityResponse { Reason = practitioner.Message }
                : !practitioner.Value!.IsOphthalmologist
                    ? new OptRangeOverrideEligibilityResponse { Reason = "Hanya Sp.M yang dapat mengonfirmasi ukuran di luar rentang." }
                    : new OptRangeOverrideEligibilityResponse { CanOverrideRange = true };
            return OptResult<OptRangeOverrideEligibilityResponse>.Ok(response);
        }

        /// <summary>
        /// Kelayakan resep untuk pesanan baru (<c>OPT-VAL-023</c>, <c>OPT-DEC-023</c>, <c>046</c>). Dipakai service pesanan.
        /// </summary>
        public async Task<OptResult<OptPrescriptionResponse>> CheckUsableForOrderAsync(
            Guid prescriptionId, Guid patientId, OptPrescriptionType requiredType, CancellationToken cancellationToken = default)
        {
            var prescription = await _dbContext.OptPrescriptions.AsNoTracking()
                .Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, cancellationToken);
            if (prescription is null)
                return OptResult<OptPrescriptionResponse>.NotFound(NotFoundMessage);
            if (prescription.PatientId != patientId)
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.InvalidRequest, "Resep bukan milik pasien pesanan ini.");
            if (prescription.PrescriptionType != requiredType)
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.InvalidRequest, "Jenis resep tidak sesuai dengan barang yang dipesan.");

            if (prescription.ValidUntil < DateOnly.FromDateTime(Today()))
                return OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.Val023, prescription.PrescriptionType == OptPrescriptionType.ContactLens
                    ? ContactLensExpiredMessage
                    : $"Resep berusia lebih dari {prescription.ValidityMonthsSnapshot} bulan, lakukan pemeriksaan ulang.");

            return OptResult<OptPrescriptionResponse>.Ok(await ProjectAsync(prescription, cancellationToken));
        }

        // ---------------------------------------------------------------- tindakan

        /// <summary>Resep internal dari pemeriksaan final. Satu resep per jenis per pemeriksaan; koreksi lewat revisi.</summary>
        public async Task<OptResult<OptPrescriptionResponse>> CreateInternalAsync(OptPrescriptionCreateRequest request, Guid actorUserId, Guid? idempotencyKey, CancellationToken cancellationToken = default)
        {
            var awal = await CheckIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (awal is not null)
                return awal;

            if (request.RefractionExamId is not { } examId || examId == Guid.Empty || request.PrescriptionType is not { } type)
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih pemeriksaan dan jenis resep.");

            var exam = await _dbContext.OptRefractionExams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == examId && !x.IsDelete, cancellationToken);
            if (exam is null)
                return OptResult<OptPrescriptionResponse>.NotFound("Pemeriksaan refraksi tidak ditemukan.");
            if (exam.Status != OptRefractionExamStatus.Final)
                return OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.InvalidState, "Simpan final pemeriksaan sebelum membuat resep.");

            var ganda = await _dbContext.OptPrescriptions.AsNoTracking()
                .Where(x => x.RefractionExamId == examId && x.PrescriptionType == type && !x.IsDelete)
                .Select(x => x.PrescriptionNumber)
                .FirstOrDefaultAsync(cancellationToken);
            if (ganda is not null)
                return OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.InvalidState,
                    $"Resep {OptLabels.PrescriptionType(type).ToLower()} dari pemeriksaan ini sudah ada ({ganda}). Gunakan revisi bila ada koreksi.");

            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!practitioner.IsSuccess)
                return Relay(practitioner);

            var setting = await FindSettingAsync(exam.EncounterId, cancellationToken);
            if (setting is null)
                return SettingMissing();

            var keputusan = await DecideAsync(type, request, setting, practitioner.Value!, cancellationToken);
            if (keputusan.Failure is not null)
                return keputusan.Failure;

            var today = DateOnly.FromDateTime(Today());
            return await CreateAsync(new OptPrescription
            {
                PatientId = exam.PatientId,
                EncounterId = exam.EncounterId,
                RefractionExamId = exam.Id,
                PrescriptionType = type,
                Origin = OptPrescriptionOrigin.Internal,
                IssuedDate = today,
                ValidityMonthsSnapshot = setting.PrescriptionValidityMonths,
                ValidUntil = OptPrescriptionRules.ValidUntil(today, setting.PrescriptionValidityMonths),
                IdempotencyKey = idempotencyKey,
            }, request, keputusan.OverrideReason, isTransposed: false, originalText: null, practitioner.Value!, actorUserId, cancellationToken);
        }

        /// <summary>Salin resep luar; silinder plus ditransposisi dan teks aslinya disimpan.</summary>
        public async Task<OptResult<OptPrescriptionResponse>> CopyExternalAsync(OptPrescriptionCopyExternalRequest request, Guid actorUserId, Guid? idempotencyKey, CancellationToken cancellationToken = default)
        {
            var awal = await CheckIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (awal is not null)
                return awal;

            if (request.EncounterId is not { } encounterId || encounterId == Guid.Empty || request.PrescriptionType is not { } type)
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih kunjungan dan jenis resep.");

            var encounter = await _visitService.FindOpticalEncounterAsync(encounterId, cancellationToken);
            if (encounter is null)
                return OptResult<OptPrescriptionResponse>.NotFound("Kunjungan Optik tidak ditemukan.");

            var penulis = request.ExternalAuthorName?.Trim();
            var fasilitas = request.ExternalFacilityName?.Trim();
            if (string.IsNullOrEmpty(penulis) || string.IsNullOrEmpty(fasilitas) || request.IssuedDate is not { } issued)
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.Val019, ExternalIncompleteMessage);
            var today = DateOnly.FromDateTime(Today());
            if (issued > today)
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.Val020, FutureDateMessage);

            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!practitioner.IsSuccess)
                return Relay(practitioner);

            var setting = await FindSettingAsync(encounterId, cancellationToken);
            if (setting is null)
                return SettingMissing();

            var isTransposed = false;
            string? originalText = null;
            if (type == OptPrescriptionType.Spectacle)
            {
                var galat = OptPrescriptionRules.TransposeForCopy(request, out isTransposed, out originalText);
                if (galat is { } g)
                    return OptResult<OptPrescriptionResponse>.Invalid(g.Code, g.Message);
            }

            var keputusan = await DecideAsync(type, request, setting, practitioner.Value!, cancellationToken);
            if (keputusan.Failure is not null)
                return keputusan.Failure;

            var penulisKecil = penulis.ToLower();
            var fasilitasKecil = fasilitas.ToLower();
            var ganda = await _dbContext.OptPrescriptions.AsNoTracking()
                .Where(x => x.PatientId == encounter.PatientId && x.Origin == OptPrescriptionOrigin.External && x.PrescriptionType == type
                    && x.IssuedDate == issued && !x.IsDelete
                    && x.ExternalAuthorName!.ToLower() == penulisKecil && x.ExternalFacilityName!.ToLower() == fasilitasKecil)
                .Select(x => x.PrescriptionNumber)
                .FirstOrDefaultAsync(cancellationToken);
            if (ganda is not null)
                return OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.InvalidState,
                    $"Resep luar ini sudah disalin ({ganda}). Gunakan revisi bila ada koreksi.");

            return await CreateAsync(new OptPrescription
            {
                PatientId = encounter.PatientId,
                EncounterId = encounter.Id,
                PrescriptionType = type,
                Origin = OptPrescriptionOrigin.External,
                ExternalAuthorName = penulis,
                ExternalFacilityName = fasilitas,
                IssuedDate = issued,
                ValidityMonthsSnapshot = setting.PrescriptionValidityMonths,
                ValidUntil = OptPrescriptionRules.ValidUntil(issued, setting.PrescriptionValidityMonths),
                IdempotencyKey = idempotencyKey,
            }, request, keputusan.OverrideReason, isTransposed, originalText, practitioner.Value!, actorUserId, cancellationToken);
        }

        /// <summary>Revisi resep yang belum terkunci; menambah versi dengan alasan (<c>OPT-VAL-021</c>, <c>022</c>).</summary>
        public async Task<OptResult<OptPrescriptionResponse>> ReviseAsync(Guid id, OptPrescriptionReviseRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var prescription = await _dbContext.OptPrescriptions
                .Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (prescription is null)
                return OptResult<OptPrescriptionResponse>.NotFound(NotFoundMessage);
            if (prescription.LockedAt is not null)
                return OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.Val022, LockedMessage);
            if (request.ExpectedVersion is null)
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.InvalidRequest, "ExpectedVersion wajib diisi.");
            if (request.ExpectedVersion != prescription.Version)
                return OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.ConcurrencyConflict, StaleMessage);
            if (string.IsNullOrWhiteSpace(request.RevisionReason))
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.Val021, RevisionReasonMessage);

            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!practitioner.IsSuccess)
                return Relay(practitioner);

            var setting = await FindSettingAsync(prescription.EncounterId, cancellationToken);
            if (setting is null)
                return SettingMissing();

            var keputusan = await DecideAsync(prescription.PrescriptionType, request, setting, practitioner.Value!, cancellationToken);
            if (keputusan.Failure is not null)
                return keputusan.Failure;

            var now = Now();
            var versi = BuildVersion(prescription.PrescriptionType, request, keputusan.OverrideReason, false, null, practitioner.Value!, actorUserId, now);
            versi.PrescriptionId = prescription.Id;
            versi.VersionNo = prescription.CurrentVersionNo + 1;
            versi.RevisionReason = request.RevisionReason!.Trim();
            // Lewat DbSet: Id Guid sudah terisi, sehingga lewat koleksi saja EF menganggapnya baris lama.
            _dbContext.OptPrescriptionVersions.Add(versi);

            prescription.CurrentVersionNo = versi.VersionNo;
            prescription.Version += 1;
            prescription.UpdateDateTime = now;
            prescription.UpdateBy = actorUserId;

            var gagal = await SignAsync(prescription, versi.Id, actorUserId, now, cancellationToken);
            if (gagal is not null)
                return gagal;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.ConcurrencyConflict, StaleMessage);
            }

            return OptResult<OptPrescriptionResponse>.Ok(await ProjectAsync(prescription, cancellationToken));
        }

        // ---------------------------------------------------------------- kewenangan

        /// <summary>Hak <c>OptPrescription : OverrideRange</c> pengguna saat ini. Seam uji.</summary>
        protected virtual async Task<bool> HasOverrideRangePermissionAsync(CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true
                && await _accessPermissionService.HasAccessAsync(user, P.Prescription.Resource, P.Prescription.OverrideRange);
        }

        private sealed record Decision(OptResult<OptPrescriptionResponse>? Failure, string? OverrideReason);

        private async Task<Decision> DecideAsync(
            OptPrescriptionType type, OptPrescriptionValues values, OptSetting setting, OptPractitioner practitioner, CancellationToken cancellationToken)
        {
            var hasil = OptPrescriptionRules.Validate(type, values, setting);
            if (hasil.HasError)
                return new Decision(OptResult<OptPrescriptionResponse>.Invalid(hasil.Code!, hasil.Message!), null);
            if (hasil.OutOfRange.Count == 0)
                return new Decision(null, null);

            var boleh = practitioner.IsOphthalmologist && await HasOverrideRangePermissionAsync(cancellationToken);
            if (!boleh)
                return new Decision(OptResult<OptPrescriptionResponse>.Forbidden(OptErrorCodes.Val004, hasil.OutOfRangeMessage!), null);
            if (string.IsNullOrWhiteSpace(values.RangeOverrideReason))
                return new Decision(OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.Val005, OverrideReasonMessage), null);

            return new Decision(null, values.RangeOverrideReason.Trim());
        }

        private static OptResult<OptPrescriptionResponse> Relay(OptResult<OptPractitioner> failure) => failure.Kind switch
        {
            OptResultKind.Unavailable => OptResult<OptPrescriptionResponse>.Unavailable(failure.Code!, failure.Message!),
            _ => OptResult<OptPrescriptionResponse>.Forbidden(failure.Code!, failure.Message!),
        };

        private static OptResult<OptPrescriptionResponse> SettingMissing() =>
            OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.SettingMissing, "Pengaturan Optik belum tersedia. Hubungi Admin Optik.");

        // ---------------------------------------------------------------- pembantu

        private DateTime Now() => _time.GetUtcNow().UtcDateTime;

        /// <summary>Mendaftarkan versi sebagai dokumen RM bertanda tangan, di SaveChanges yang sama dengan resepnya.</summary>
        private async Task<OptResult<OptPrescriptionResponse>?> SignAsync(OptPrescription prescription, Guid versionId, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
        {
            var http = _httpContextAccessor.HttpContext;
            try
            {
                await _integrity.RegisterSignedAsync(DocumentKind, versionId, prescription.PatientId, prescription.EncounterId, actorUserId,
                    http?.Request.Headers.UserAgent.ToString(), http?.Connection.RemoteIpAddress?.ToString(), now, cancellationToken);
                return null;
            }
            catch (InvalidOperationException ex)
            {
                // Belum ada yang tersimpan; perubahan yang terlacak dibuang agar tidak ikut tersimpan belakangan.
                _dbContext.ChangeTracker.Clear();
                return OptResult<OptPrescriptionResponse>.Conflict(OptErrorCodes.MedicalRecordRegistrationFailed,
                    $"Resep tidak tersimpan karena pendaftaran ke rekam medis gagal: {ex.Message}");
            }
        }

        private DateTime Today() => OptPractitionerLicenseReader.ToOperationalDate(_time.GetUtcNow());

        /// <summary>Pengaturan unit Optik kunjungan; bila unit itu belum punya, pengaturan Optik tertua.</summary>
        private async Task<OptSetting?> FindSettingAsync(Guid encounterId, CancellationToken cancellationToken)
        {
            var encounter = await _visitService.FindOpticalEncounterAsync(encounterId, cancellationToken);
            var settings = _dbContext.OptSettings.AsNoTracking().Where(x => !x.IsDelete);
            return (encounter is null ? null : await settings.FirstOrDefaultAsync(x => x.ServiceUnitId == encounter.ServiceUnitId, cancellationToken))
                ?? await settings.OrderBy(x => x.CreateDateTime).FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<OptResult<OptPrescriptionResponse>> CreateAsync(
            OptPrescription prescription,
            OptPrescriptionValues values,
            string? overrideReason,
            bool isTransposed,
            string? originalText,
            OptPractitioner practitioner,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var now = Now();
            prescription.PrescriptionNumber = await _numberService.AllocatePrescriptionNumberAsync(actorUserId, cancellationToken);
            prescription.CurrentVersionNo = 1;
            prescription.CreateDateTime = now;
            prescription.CreateBy = actorUserId;

            var versi = BuildVersion(prescription.PrescriptionType, values, overrideReason, isTransposed, originalText, practitioner, actorUserId, now);
            versi.VersionNo = 1;
            prescription.Versions.Add(versi);

            _dbContext.OptPrescriptions.Add(prescription);
            var gagal = await SignAsync(prescription, versi.Id, actorUserId, now, cancellationToken);
            if (gagal is not null)
                return gagal;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Dua kiriman serentak berkunci sama: indeks unik menolak yang kalah, lalu data pemenangnya dikembalikan.
                // Kegagalan lain tidak punya data pemenang, sehingga tetap diteruskan apa adanya.
                _dbContext.ChangeTracker.Clear();
                if (await FindReplayAsync(prescription.IdempotencyKey!.Value, cancellationToken) is { } pemenang)
                    return pemenang;
                throw;
            }

            return OptResult<OptPrescriptionResponse>.Ok(await ProjectAsync(prescription, cancellationToken));
        }

        /// <summary>
        /// Gerbang kunci idempoten (<c>OPT-DEC-077</c>), diperiksa paling awal agar kiriman ulang tidak ditolak oleh hasil
        /// kiriman pertamanya sendiri. Kosong berarti lanjut membuat resep baru.
        /// </summary>
        private async Task<OptResult<OptPrescriptionResponse>?> CheckIdempotencyKeyAsync(Guid? idempotencyKey, CancellationToken cancellationToken)
        {
            if (idempotencyKey is not { } kunci || kunci == Guid.Empty)
                return OptResult<OptPrescriptionResponse>.Invalid(OptErrorCodes.InvalidRequest, IdempotencyKeyRequiredMessage);

            return await FindReplayAsync(kunci, cancellationToken);
        }

        private async Task<OptResult<OptPrescriptionResponse>?> FindReplayAsync(Guid kunci, CancellationToken cancellationToken)
        {
            var id = await _dbContext.OptPrescriptions.AsNoTracking()
                .Where(x => x.IdempotencyKey == kunci)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (id is null)
                return null;

            var hasil = await GetDetailAsync(id.Value, cancellationToken);
            if (hasil.Value is { } resep)
                resep.IsReplay = true;
            return hasil;
        }

        private static OptPrescriptionVersion BuildVersion(
            OptPrescriptionType type,
            OptPrescriptionValues v,
            string? overrideReason,
            bool isTransposed,
            string? originalText,
            OptPractitioner practitioner,
            Guid actorUserId,
            DateTime now)
        {
            var versi = new OptPrescriptionVersion
            {
                IsTransposed = isTransposed,
                OriginalNotationText = originalText,
                RangeOverrideReason = overrideReason,
                AuthorWorkforceProfileId = practitioner.WorkforceProfileId,
                AuthorLicenseNumberSnapshot = practitioner.SipNumber,
                AuthoredAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId,
            };

            if (type == OptPrescriptionType.ContactLens)
            {
                versi.OdClPower = v.ContactLensOd?.Power;
                versi.OdClCyl = v.ContactLensOd?.Cyl;
                versi.OdClAxis = v.ContactLensOd?.Axis;
                versi.OdClBaseCurve = v.ContactLensOd?.BaseCurve;
                versi.OdClDiameter = v.ContactLensOd?.Diameter;
                versi.OsClPower = v.ContactLensOs?.Power;
                versi.OsClCyl = v.ContactLensOs?.Cyl;
                versi.OsClAxis = v.ContactLensOs?.Axis;
                versi.OsClBaseCurve = v.ContactLensOs?.BaseCurve;
                versi.OsClDiameter = v.ContactLensOs?.Diameter;
                versi.ContactLensBrandOrType = v.ContactLensBrandOrType?.Trim();
                versi.ContactLensReplacement = string.IsNullOrWhiteSpace(v.ContactLensReplacement) ? null : v.ContactLensReplacement.Trim();
            }
            else
            {
                versi.OdSph = v.Od?.Sph;
                versi.OdCyl = v.Od?.Cyl;
                versi.OdAxis = v.Od?.Axis;
                versi.OdAdd = v.Od?.Add;
                versi.OsSph = v.Os?.Sph;
                versi.OsCyl = v.Os?.Cyl;
                versi.OsAxis = v.Os?.Axis;
                versi.OsAdd = v.Os?.Add;
                versi.PdDistance = v.PdDistance;
                versi.PdNear = v.PdNear;
            }

            return versi;
        }

        private async Task<Dictionary<Guid, OptPrescriptionVersion>> CurrentVersionsAsync(IReadOnlyCollection<OptPrescription> prescriptions, CancellationToken cancellationToken)
        {
            var ids = prescriptions.Select(x => x.Id).ToList();
            var versions = await _dbContext.OptPrescriptionVersions.AsNoTracking()
                .Where(v => ids.Contains(v.PrescriptionId) && !v.IsDelete)
                .ToListAsync(cancellationToken);
            return prescriptions
                .Select(p => versions.FirstOrDefault(v => v.PrescriptionId == p.Id && v.VersionNo == p.CurrentVersionNo))
                .Where(v => v is not null)
                .ToDictionary(v => v!.PrescriptionId, v => v!);
        }

        private async Task<Dictionary<Guid, string>> NamesAsync(IEnumerable<Guid> workforceProfileIds, CancellationToken cancellationToken)
        {
            var ids = workforceProfileIds.Distinct().ToList();
            return await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }

        private async Task<OptPrescriptionResponse> ProjectAsync(OptPrescription prescription, CancellationToken cancellationToken)
        {
            var versions = prescription.Versions.Where(v => !v.IsDelete).ToList();
            var names = await NamesAsync(versions.Select(v => v.AuthorWorkforceProfileId), cancellationToken);
            return Project(prescription, versions, names, DateOnly.FromDateTime(Today()), includeAll: true);
        }

        private static OptPrescriptionResponse Project(
            OptPrescription x,
            IReadOnlyCollection<OptPrescriptionVersion> versions,
            IReadOnlyDictionary<Guid, string> names,
            DateOnly today,
            bool includeAll)
        {
            var semua = versions.OrderBy(v => v.VersionNo).Select(v => ProjectVersion(x.PrescriptionType, v, names)).ToList();
            return new OptPrescriptionResponse
            {
                Id = x.Id,
                PrescriptionNumber = x.PrescriptionNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                RefractionExamId = x.RefractionExamId,
                PrescriptionType = x.PrescriptionType,
                PrescriptionTypeLabel = OptLabels.PrescriptionType(x.PrescriptionType),
                Origin = x.Origin,
                OriginLabel = OptLabels.PrescriptionOrigin(x.Origin),
                ExternalAuthorName = x.ExternalAuthorName,
                ExternalFacilityName = x.ExternalFacilityName,
                IssuedDate = x.IssuedDate,
                ValidityMonthsSnapshot = x.ValidityMonthsSnapshot,
                ValidUntil = x.ValidUntil,
                IsExpired = x.ValidUntil < today,
                IsLocked = x.LockedAt != null,
                LockedAt = x.LockedAt,
                LockedByOrderId = x.LockedByOrderId,
                CurrentVersionNo = x.CurrentVersionNo,
                Version = x.Version,
                Current = semua.FirstOrDefault(v => v.VersionNo == x.CurrentVersionNo),
                Versions = includeAll ? semua : new List<OptPrescriptionVersionResponse>(),
            };
        }

        private static OptPrescriptionVersionResponse ProjectVersion(OptPrescriptionType type, OptPrescriptionVersion v, IReadOnlyDictionary<Guid, string> names)
        {
            var kontak = type == OptPrescriptionType.ContactLens;
            return new OptPrescriptionVersionResponse
            {
                Id = v.Id,
                VersionNo = v.VersionNo,
                Od = kontak ? null : new OptLensPowerDto { Sph = v.OdSph, Cyl = v.OdCyl, Axis = v.OdAxis, Add = v.OdAdd },
                Os = kontak ? null : new OptLensPowerDto { Sph = v.OsSph, Cyl = v.OsCyl, Axis = v.OsAxis, Add = v.OsAdd },
                PdDistance = v.PdDistance,
                PdNear = v.PdNear,
                ContactLensOd = kontak ? new OptContactLensEyeDto { Power = v.OdClPower, Cyl = v.OdClCyl, Axis = v.OdClAxis, BaseCurve = v.OdClBaseCurve, Diameter = v.OdClDiameter } : null,
                ContactLensOs = kontak ? new OptContactLensEyeDto { Power = v.OsClPower, Cyl = v.OsClCyl, Axis = v.OsClAxis, BaseCurve = v.OsClBaseCurve, Diameter = v.OsClDiameter } : null,
                ContactLensBrandOrType = v.ContactLensBrandOrType,
                ContactLensReplacement = v.ContactLensReplacement,
                IsTransposed = v.IsTransposed,
                OriginalNotationText = v.OriginalNotationText,
                RangeOverrideReason = v.RangeOverrideReason,
                Summary = OptPrescriptionRules.Summary(type, v),
                AuthorWorkforceProfileId = v.AuthorWorkforceProfileId,
                AuthorName = names.GetValueOrDefault(v.AuthorWorkforceProfileId, string.Empty),
                AuthorLicenseNumberSnapshot = v.AuthorLicenseNumberSnapshot,
                AuthoredAt = v.AuthoredAt,
                RevisionReason = v.RevisionReason,
            };
        }
    }
}
