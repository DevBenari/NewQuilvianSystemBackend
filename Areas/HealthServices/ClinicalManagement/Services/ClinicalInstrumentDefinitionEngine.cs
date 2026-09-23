using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Mesin definisi instrumen klinis berversi: normalisasi, hash, validasi, dan perhitungan skor —
    /// <c>BE-RWI-107</c>, <c>BE-RWI-109</c>, kamus data 0.4 bagian 11.5, <c>RWI-DEC-124</c>, <c>RWI-DEC-136</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nol angka klinis di sini.</b> Batas kategori, nilai butir, dan isian wajib seluruhnya dibaca
    /// dari definisi versi. Yang ditanam di kode hanya aturan bentuk: pita tidak bertumpuk, tidak
    /// berlubang, dan menutup skor terendah sampai tertinggi yang mungkin.
    /// </para>
    /// <para>
    /// <b>Contoh pita yang ditolak.</b> Rendah <c>[0, 25)</c>, Sedang <c>[25, 51)</c>, Tinggi <c>[50, null)</c> —
    /// skor 50 masuk Sedang dan Tinggi sekaligus → "Rentang kategori tidak boleh bertumpuk atau
    /// berlubang. Periksa batas Sedang dan Tinggi." Rendah <c>[0, 6)</c>, Sedang <c>[6, 17)</c> pada
    /// instrumen yang skor tertingginya 30 → skor 17–30 tidak punya kategori, juga ditolak.
    /// </para>
    /// <para>
    /// <b>Isian skala yang belum dijawab tidak pernah dihitung nol.</b> Skor total baru terbit bila
    /// seluruh isian berskor sudah dijawab; sebelum itu skornya kosong. Tanpa aturan ini, formulir
    /// Resiko Jatuh yang belum diisi akan terbaca "Rendah" — persis cacat "belum dikaji terbaca normal"
    /// yang ditutup <c>BE-RWI-106</c>.
    /// </para>
    /// </remarks>
    public static class ClinicalInstrumentDefinitionEngine
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        private static readonly HashSet<string> JenisIsian = new(StringComparer.Ordinal)
        {
            "boolean", "single", "multi", "text", "number", "date"
        };

        /// <summary>
        /// Kolom <c>TrxPatientAssessment</c> yang boleh menjadi tempat simpan isian — kamus data 0.4
        /// bagian 11.1. Isian lain disimpan sebagai jawaban JSON.
        /// </summary>
        public static readonly IReadOnlySet<string> KolomYangBolehDiikat = new HashSet<string>(StringComparer.Ordinal)
        {
            "ChiefComplaint", "CurrentIllnessHistory", "MedicationHistory", "ConsciousnessStatus",
            "IsUsingOxygen", "OxygenSupportType", "OxygenFlowRate", "HasAllergy", "AllergyNote",
            "AppetiteStatus", "HasNausea", "HasVomiting", "NutritionRiskStatus", "NutritionRiskScore",
            "FunctionalStatus", "FunctionalNote", "PsychosocialNote", "NurseNote", "FallRiskNote",
            "PainAssessmentState", "PainScale", "PainTrigger", "PainQuality", "PainLocation",
            "PainFrequency", "PainManagement", "PainNote", "EducationNote"
        };

        /// <summary>Membaca definisi JSON; <c>null</c> beserta sebabnya bila bentuknya tidak dapat dibaca.</summary>
        public static ClinicalInstrumentDefinition? Parse(string? json, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Definisi formulir kosong.";
                return null;
            }

            try
            {
                var hasil = JsonSerializer.Deserialize<ClinicalInstrumentDefinition>(json, JsonOptions);

                if (hasil == null)
                    error = "Definisi formulir tidak dapat dibaca.";

                return hasil;
            }
            catch (JsonException ex)
            {
                error = "Definisi formulir bukan JSON yang sah: " + ex.Message;
                return null;
            }
        }

        /// <summary>JSON ternormalisasi — urutan kunci tetap, tanpa spasi — yang disimpan dan di-hash.</summary>
        public static string Normalize(ClinicalInstrumentDefinition definition) =>
            JsonSerializer.Serialize(definition, JsonOptions);

        /// <summary>SHA-256 heksadesimal huruf kecil atas JSON ternormalisasi.</summary>
        public static string Hash(string normalizedJson) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedJson))).ToLowerInvariant();

        /// <summary>
        /// Menegakkan <c>VAL-KEP-19a</c> dan <c>VAL-KEP-19b</c>. Mengembalikan daftar kesalahan; kosong
        /// berarti sah.
        /// </summary>
        public static List<string> Validate(ClinicalInstrumentDefinition definition)
        {
            var kesalahan = new List<string>();
            var kodeBagian = new HashSet<string>(StringComparer.Ordinal);
            var kodeIsian = new HashSet<string>(StringComparer.Ordinal);

            if (definition.Sections == null || definition.Sections.Count == 0)
                kesalahan.Add("Definisi formulir tidak sah: minimal satu bagian.");

            foreach (var bagian in definition.Sections ?? new())
            {
                if (string.IsNullOrWhiteSpace(bagian.Code) || !kodeBagian.Add(bagian.Code))
                    kesalahan.Add($"Definisi formulir tidak sah: kode bagian \"{bagian.Code}\" kosong atau ganda.");

                foreach (var isian in bagian.Items ?? new())
                {
                    if (string.IsNullOrWhiteSpace(isian.Code) || !kodeIsian.Add(isian.Code))
                        kesalahan.Add($"Definisi formulir tidak sah: kode isian \"{isian.Code}\" kosong atau ganda.");

                    if (!JenisIsian.Contains(isian.Type ?? string.Empty))
                        kesalahan.Add($"Definisi formulir tidak sah: jenis isian \"{isian.Type}\" pada {isian.Code} tidak dikenal.");

                    if ((isian.Type == "single" || isian.Type == "multi") && (isian.Options == null || isian.Options.Count == 0))
                        kesalahan.Add($"Definisi formulir tidak sah: isian {isian.Code} bertipe {isian.Type} tanpa pilihan.");

                    if (isian.Options != null)
                    {
                        var kodePilihan = new HashSet<string>(StringComparer.Ordinal);

                        foreach (var pilihan in isian.Options)
                        {
                            if (string.IsNullOrWhiteSpace(pilihan.Code) || !kodePilihan.Add(pilihan.Code))
                                kesalahan.Add($"Definisi formulir tidak sah: pilihan \"{pilihan.Code}\" pada {isian.Code} kosong atau ganda.");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(isian.Binding) && !KolomYangBolehDiikat.Contains(isian.Binding))
                        kesalahan.Add($"Definisi formulir tidak sah: isian {isian.Code} diikat ke kolom \"{isian.Binding}\" yang tidak diizinkan.");
                }
            }

            foreach (var wajib in definition.RequiredItemCodes ?? new())
            {
                if (!kodeIsian.Contains(wajib))
                    kesalahan.Add($"Definisi formulir tidak sah: isian wajib \"{wajib}\" tidak ada pada formulir.");
            }

            if (definition.ReassessmentMinutes.HasValue && definition.ReassessmentMinutes.Value <= 0)
                kesalahan.Add("Definisi formulir tidak sah: interval kajian ulang harus lebih dari 0 menit.");

            var berskor = definition.Scoring != null;

            if (berskor && !string.Equals(definition.Scoring!.Method, "sum", StringComparison.Ordinal))
                kesalahan.Add($"Definisi formulir tidak sah: cara hitung \"{definition.Scoring.Method}\" tidak dikenal; revision ini hanya mengenal sum.");

            if (!berskor && definition.Bands is { Count: > 0 })
                kesalahan.Add("Definisi formulir tidak sah: pita kategori tanpa cara hitung skor.");

            if (berskor)
                kesalahan.AddRange(ValidateBands(definition));

            return kesalahan;
        }

        private static IEnumerable<string> ValidateBands(ClinicalInstrumentDefinition definition)
        {
            const string bertumpuk = "Rentang kategori tidak boleh bertumpuk atau berlubang.";
            var pita = (definition.Bands ?? new()).ToList();

            if (pita.Count == 0)
            {
                yield return "Instrumen berskor wajib punya pita kategori.";
                yield break;
            }

            var kodePita = new HashSet<string>(StringComparer.Ordinal);

            foreach (var p in pita)
            {
                if (string.IsNullOrWhiteSpace(p.Code) || !kodePita.Add(p.Code))
                    yield return $"Definisi formulir tidak sah: kode pita \"{p.Code}\" kosong atau ganda.";

                if (p.MinInclusive.HasValue && p.MaxExclusive.HasValue && p.MinInclusive.Value >= p.MaxExclusive.Value)
                    yield return $"{bertumpuk} Pita {p.Label} batas bawahnya tidak lebih kecil dari batas atasnya.";

                if (!string.IsNullOrWhiteSpace(p.MappedFallRiskStatus) &&
                    !Enum.TryParse<FallRiskStatus>(p.MappedFallRiskStatus, false, out _))
                    yield return $"Definisi formulir tidak sah: kategori risiko jatuh \"{p.MappedFallRiskStatus}\" pada pita {p.Label} tidak dikenal.";
            }

            var urut = pita.OrderBy(x => x.MinInclusive ?? decimal.MinValue).ToList();

            for (var i = 1; i < urut.Count; i++)
            {
                var sebelum = urut[i - 1];
                var sesudah = urut[i];

                if (!sebelum.MaxExclusive.HasValue || !sesudah.MinInclusive.HasValue ||
                    sebelum.MaxExclusive.Value != sesudah.MinInclusive.Value)
                {
                    yield return $"{bertumpuk} Periksa batas {sebelum.Label} dan {sesudah.Label}.";
                }
            }

            var (terendah, tertinggi) = ScoreRange(definition);
            var pertama = urut[0];
            var terakhir = urut[^1];

            if (pertama.MinInclusive.HasValue && pertama.MinInclusive.Value > terendah)
                yield return $"{bertumpuk} Skor terendah yang mungkin ({terendah.ToString(CultureInfo.InvariantCulture)}) tidak punya kategori.";

            if (terakhir.MaxExclusive.HasValue && terakhir.MaxExclusive.Value <= tertinggi)
                yield return $"{bertumpuk} Skor tertinggi yang mungkin ({tertinggi.ToString(CultureInfo.InvariantCulture)}) tidak punya kategori.";
        }

        /// <summary>Skor terendah dan tertinggi yang mungkin dari seluruh isian berskor.</summary>
        public static (decimal Min, decimal Max) ScoreRange(ClinicalInstrumentDefinition definition)
        {
            decimal min = 0, max = 0;

            foreach (var isian in ScoredItems(definition))
            {
                var nilai = isian.Options!.Select(o => o.Score ?? 0m).ToList();

                if (isian.Type == "single")
                {
                    min += nilai.Min();
                    max += nilai.Max();
                }
                else
                {
                    min += nilai.Where(x => x < 0).Sum();
                    max += nilai.Where(x => x > 0).Sum();
                }
            }

            return (min, max);
        }

        /// <summary>Isian pilihan yang punya sekurang-kurangnya satu nilai skor.</summary>
        public static IEnumerable<ClinicalInstrumentItemDefinition> ScoredItems(ClinicalInstrumentDefinition definition) =>
            (definition.Sections ?? new())
                .SelectMany(s => s.Items ?? new())
                .Where(i => (i.Type == "single" || i.Type == "multi") &&
                            i.Options != null && i.Options.Any(o => o.Score.HasValue));

        /// <summary>
        /// Menghitung skor dan pita dari jawaban. Jawaban yang tidak cocok dengan definisi dikembalikan
        /// sebagai kesalahan — <c>400</c> "pita instrumen tidak dapat dihitung dari jawaban".
        /// </summary>
        public static InstrumentScoreResult Score(ClinicalInstrumentDefinition definition, IReadOnlyDictionary<string, JsonElement> responses)
        {
            var hasil = new InstrumentScoreResult();
            var isianMenurutKode = (definition.Sections ?? new())
                .SelectMany(s => s.Items ?? new())
                .Where(i => !string.IsNullOrWhiteSpace(i.Code))
                .GroupBy(i => i.Code!)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            foreach (var (kode, nilai) in responses)
            {
                if (!isianMenurutKode.TryGetValue(kode, out var isian))
                {
                    hasil.Errors.Add($"Isian {kode} tidak ada pada versi instrumen ini.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(isian.Binding))
                    hasil.Errors.Add($"Isian {kode} disimpan pada kolom {isian.Binding}, bukan pada jawaban JSON.");

                var galat = CheckShape(isian, nilai);

                if (galat != null)
                    hasil.Errors.Add(galat);
            }

            if (definition.Scoring == null)
                return hasil;

            decimal total = 0;
            var lengkap = true;

            foreach (var isian in ScoredItems(definition))
            {
                if (!responses.TryGetValue(isian.Code!, out var nilai) || IsEmpty(nilai))
                {
                    lengkap = false;
                    hasil.UnansweredScoredItems.Add(isian.Label ?? isian.Code!);
                    continue;
                }

                var dipilih = nilai.ValueKind == JsonValueKind.Array
                    ? nilai.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!).ToList()
                    : nilai.ValueKind == JsonValueKind.String ? new List<string> { nilai.GetString()! } : new List<string>();

                total += isian.Options!.Where(o => dipilih.Contains(o.Code!)).Sum(o => o.Score ?? 0m);
            }

            if (!lengkap || hasil.Errors.Count > 0)
                return hasil;

            hasil.TotalScore = total;

            var pita = (definition.Bands ?? new()).FirstOrDefault(p =>
                (!p.MinInclusive.HasValue || total >= p.MinInclusive.Value) &&
                (!p.MaxExclusive.HasValue || total < p.MaxExclusive.Value));

            if (pita != null)
            {
                hasil.BandCode = pita.Code;
                hasil.BandLabel = pita.Label;
                hasil.IsAlertBand = pita.IsAlert;

                if (Enum.TryParse<FallRiskStatus>(pita.MappedFallRiskStatus, false, out var status))
                    hasil.MappedFallRiskStatus = status;
            }
            else
            {
                hasil.Errors.Add($"Skor {total.ToString(CultureInfo.InvariantCulture)} tidak jatuh ke pita kategori mana pun.");
            }

            return hasil;
        }

        /// <summary>Kode isian wajib yang masih kosong, beserta labelnya.</summary>
        public static List<string> MissingRequiredItems(
            ClinicalInstrumentDefinition definition,
            IReadOnlyDictionary<string, JsonElement> responses,
            Func<string, bool> isBoundColumnFilled)
        {
            var isian = (definition.Sections ?? new())
                .SelectMany(s => s.Items ?? new())
                .Where(i => !string.IsNullOrWhiteSpace(i.Code))
                .GroupBy(i => i.Code!)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var kosong = new List<string>();

            foreach (var kode in definition.RequiredItemCodes ?? new())
            {
                if (!isian.TryGetValue(kode, out var item))
                    continue;

                var terisi = !string.IsNullOrWhiteSpace(item.Binding)
                    ? isBoundColumnFilled(item.Binding)
                    : responses.TryGetValue(kode, out var nilai) && !IsEmpty(nilai);

                if (!terisi)
                    kosong.Add(item.Label ?? kode);
            }

            return kosong;
        }

        public static Dictionary<string, JsonElement> ParseResponses(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, JsonElement>(StringComparer.Ordinal);

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonOptions)
                       ?? new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            }
            catch (JsonException)
            {
                return new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            }
        }

        public static bool IsEmpty(JsonElement nilai) => nilai.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.Null => true,
            JsonValueKind.String => string.IsNullOrWhiteSpace(nilai.GetString()),
            JsonValueKind.Array => nilai.GetArrayLength() == 0,
            _ => false
        };

        private static string? CheckShape(ClinicalInstrumentItemDefinition isian, JsonElement nilai)
        {
            if (IsEmpty(nilai))
                return null;

            var kodeSah = new HashSet<string>((isian.Options ?? new()).Select(o => o.Code ?? string.Empty), StringComparer.Ordinal);

            return isian.Type switch
            {
                "boolean" when nilai.ValueKind is not (JsonValueKind.True or JsonValueKind.False) =>
                    $"Isian {isian.Code} harus ya/tidak.",
                "number" when nilai.ValueKind != JsonValueKind.Number =>
                    $"Isian {isian.Code} harus angka.",
                "single" when nilai.ValueKind != JsonValueKind.String || !kodeSah.Contains(nilai.GetString()!) =>
                    $"Pilihan isian {isian.Code} tidak ada pada instrumen ini.",
                "multi" when nilai.ValueKind != JsonValueKind.Array ||
                             nilai.EnumerateArray().Any(e => e.ValueKind != JsonValueKind.String || !kodeSah.Contains(e.GetString()!)) =>
                    $"Pilihan isian {isian.Code} tidak ada pada instrumen ini.",
                "text" or "date" when nilai.ValueKind != JsonValueKind.String =>
                    $"Isian {isian.Code} harus teks.",
                _ => null
            };
        }
    }
}
