using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Hasil validasi rentang: rentang yang sudah terurut dari batas terendah, atau kalimat penolakan.
    /// </summary>
    public sealed record SlidingScaleRangeValidationResult(
        bool IsValid,
        string? ErrorMessage,
        IReadOnlyList<SlidingScaleRangeRequest> OrderedRanges);

    /// <summary>
    /// Penjaga keselamatan terpenting protokol sliding scale — <c>BE-RWI-102</c> kriteria 2,
    /// <c>FR-DOK-094</c>, <c>VAL-DOK-54</c>, <c>VAL-DOK-54a</c>, <c>VAL-DOK-54b</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Aturannya.</b> Rentang pada satu versi harus menutup <b>seluruh</b> nilai gula darah yang
    /// mungkin: tepat satu rentang terbuka ke bawah, tepat satu terbuka ke atas, dan setiap batas atas
    /// sama persis dengan batas bawah rentang berikutnya. Bertumpuk berarti satu nilai gula darah
    /// menghasilkan dua dosis; berlubang berarti ada nilai tanpa dosis.
    /// </para>
    /// <para>
    /// <b>Contoh penolakan.</b> Rentang 200–260 dan 250–299 → "Rentang 200–260 dan 250–299 bertumpuk."
    /// Versi yang dimulai dari 150 tanpa rentang di bawahnya → "…Tambahkan rentang untuk nilai di bawah
    /// 150." Rentang 150–200 lalu 210–250 → "…Tambahkan rentang untuk nilai 200 sampai 210."
    /// </para>
    /// <para>
    /// Dipakai bersama oleh versi template dan versi order, karena keduanya menyimpan bentuk rentang
    /// yang sama pada satu tabel. Nol angka klinis ditanam di sini.
    /// </para>
    /// </remarks>
    public static class SlidingScaleRangeValidator
    {
        public static SlidingScaleRangeValidationResult Validate(
            IReadOnlyCollection<SlidingScaleRangeRequest>? ranges,
            BloodGlucoseUnit? glucoseUnit)
        {
            // VAL-DOK-54b — satuan.
            if (!glucoseUnit.HasValue || !Enum.IsDefined(typeof(BloodGlucoseUnit), glucoseUnit.Value))
                return Gagal("Dosis tidak boleh negatif dan satuan gula darah wajib dipilih.");

            if (ranges == null || ranges.Count == 0)
                return Gagal("Rentang sliding scale wajib diisi sekurang-kurangnya satu.");

            // VAL-DOK-54b — dosis.
            if (ranges.Any(x => x.DoseUnits < 0))
                return Gagal("Dosis tidak boleh negatif dan satuan gula darah wajib dipilih.");

            if (ranges.Any(x => x.LowerBoundInclusive.HasValue && x.UpperBoundExclusive.HasValue &&
                                x.LowerBoundInclusive.Value >= x.UpperBoundExclusive.Value))
            {
                return Gagal("Batas bawah setiap rentang harus lebih kecil dari batas atasnya.");
            }

            var terurut = ranges
                .OrderBy(x => x.LowerBoundInclusive.HasValue ? 1 : 0)
                .ThenBy(x => x.LowerBoundInclusive ?? 0)
                .ThenBy(x => x.UpperBoundExclusive ?? decimal.MaxValue)
                .ToList();

            var terbukaBawah = terurut.Where(x => !x.LowerBoundInclusive.HasValue).ToList();
            var terbukaAtas = terurut.Where(x => !x.UpperBoundExclusive.HasValue).ToList();

            if (terbukaBawah.Count > 1)
                return Bertumpuk(terbukaBawah[0], terbukaBawah[1]);

            if (terbukaAtas.Count > 1)
                return Bertumpuk(terbukaAtas[0], terbukaAtas[1]);

            // VAL-DOK-54a — tidak ada rentang terbuka di bawah atau di atas.
            if (terbukaBawah.Count == 0)
            {
                var terendah = terurut.Min(x => x.LowerBoundInclusive!.Value);
                return Gagal($"Rentang belum menutup seluruh nilai gula darah. Tambahkan rentang untuk nilai di bawah {Angka(terendah)}.");
            }

            if (terbukaAtas.Count == 0)
            {
                var tertinggi = terurut.Max(x => x.UpperBoundExclusive!.Value);
                return Gagal($"Rentang belum menutup seluruh nilai gula darah. Tambahkan rentang untuk nilai di atas {Angka(tertinggi)}.");
            }

            for (var i = 1; i < terurut.Count; i++)
            {
                var sebelumnya = terurut[i - 1];
                var sekarang = terurut[i];

                // Rentang terbuka ke atas yang bukan rentang terakhir menimpa seluruh rentang sesudahnya.
                if (!sebelumnya.UpperBoundExclusive.HasValue)
                    return Bertumpuk(sebelumnya, sekarang);

                var batasAtas = sebelumnya.UpperBoundExclusive.Value;
                var batasBawah = sekarang.LowerBoundInclusive!.Value;

                if (batasAtas > batasBawah)
                    return Bertumpuk(sebelumnya, sekarang);

                if (batasAtas < batasBawah)
                {
                    return Gagal($"Rentang belum menutup seluruh nilai gula darah. Tambahkan rentang untuk nilai {Angka(batasAtas)} sampai {Angka(batasBawah)}.");
                }
            }

            return new SlidingScaleRangeValidationResult(true, null, terurut);
        }

        /// <summary>
        /// Membentuk baris rentang siap simpan dari rentang yang sudah divalidasi dan terurut.
        /// </summary>
        public static List<PhmSlidingScaleRange> BuildRows(
            IReadOnlyList<SlidingScaleRangeRequest> orderedRanges,
            Guid? templateVersionId,
            Guid? orderVersionId,
            Guid actorUserId,
            DateTime now)
        {
            return orderedRanges
                .Select((x, i) => new PhmSlidingScaleRange
                {
                    Id = Guid.NewGuid(),
                    TemplateVersionId = templateVersionId,
                    OrderVersionId = orderVersionId,
                    LowerBoundInclusive = x.LowerBoundInclusive,
                    UpperBoundExclusive = x.UpperBoundExclusive,
                    DoseUnits = x.DoseUnits,
                    InstructionText = string.IsNullOrWhiteSpace(x.InstructionText) ? null : x.InstructionText.Trim(),
                    RequiresPhysicianNotification = x.RequiresPhysicianNotification,
                    SortOrder = i + 1,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                })
                .ToList();
        }

        /// <summary>
        /// Mengubah baris rentang tersimpan menjadi bentuk permintaan, terurut dari batas terendah.
        /// </summary>
        public static List<SlidingScaleRangeRequest> ToRequests(IEnumerable<PhmSlidingScaleRange> rows)
        {
            return rows
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .Select(x => new SlidingScaleRangeRequest
                {
                    LowerBoundInclusive = x.LowerBoundInclusive,
                    UpperBoundExclusive = x.UpperBoundExclusive,
                    DoseUnits = x.DoseUnits,
                    InstructionText = x.InstructionText,
                    RequiresPhysicianNotification = x.RequiresPhysicianNotification
                })
                .ToList();
        }

        /// <summary>
        /// Apakah dua definisi rentang sama persis — dipakai menentukan <c>IsAdjusted</c> pada order.
        /// </summary>
        public static bool AreEquivalent(
            IReadOnlyList<SlidingScaleRangeRequest> a,
            IReadOnlyList<SlidingScaleRangeRequest> b)
        {
            return Canonical(a) == Canonical(b);
        }

        /// <summary>
        /// SHA-256 heksadesimal atas definisi yang disahkan (satuan dan seluruh rentang terurut).
        /// </summary>
        public static string ComputeDefinitionHash(
            BloodGlucoseUnit glucoseUnit,
            IReadOnlyList<SlidingScaleRangeRequest> orderedRanges)
        {
            var teks = $"unit={(int)glucoseUnit}\n{Canonical(orderedRanges)}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(teks));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public static string Label(SlidingScaleRangeRequest x)
        {
            var bawah = x.LowerBoundInclusive.HasValue ? Angka(x.LowerBoundInclusive.Value) : "…";
            var atas = x.UpperBoundExclusive.HasValue ? Angka(x.UpperBoundExclusive.Value) : "…";
            return $"{bawah}–{atas}";
        }

        private static string Canonical(IReadOnlyList<SlidingScaleRangeRequest> ranges)
        {
            var sb = new StringBuilder();
            foreach (var x in ranges)
            {
                sb.Append(x.LowerBoundInclusive?.ToString("0.00", CultureInfo.InvariantCulture) ?? "null").Append('|')
                  .Append(x.UpperBoundExclusive?.ToString("0.00", CultureInfo.InvariantCulture) ?? "null").Append('|')
                  .Append(x.DoseUnits.ToString("0.00", CultureInfo.InvariantCulture)).Append('|')
                  .Append(x.RequiresPhysicianNotification ? '1' : '0').Append('|')
                  .Append((x.InstructionText ?? string.Empty).Trim())
                  .Append('\n');
            }
            return sb.ToString();
        }

        private static string Angka(decimal value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture);

        private static SlidingScaleRangeValidationResult Bertumpuk(SlidingScaleRangeRequest a, SlidingScaleRangeRequest b) =>
            Gagal($"Rentang {Label(a)} dan {Label(b)} bertumpuk. Setiap nilai gula darah harus jatuh ke tepat satu rentang.");

        private static SlidingScaleRangeValidationResult Gagal(string message) =>
            new(false, message, Array.Empty<SlidingScaleRangeRequest>());
    }
}
