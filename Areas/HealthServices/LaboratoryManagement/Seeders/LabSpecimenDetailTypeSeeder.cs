using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Reflection;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders
{
    /// <summary>
    /// Mengisi data induk Spesifik Specimen dari berkas CSV <b>tertanam</b>
    /// (<c>LAB-DEC-129</c>, <c>LAB-DEC-130</c>, <c>BE-LAB-55</c>).
    ///
    /// <b>1.767 baris dari <c>LAB-EVD-007</c></b>: 1.601 aktif, dan 166 berkonfidensi
    /// <c>Rendah</c> ter-seed <b>nonaktif</b>. Kepala instalasi mengaktifkan yang ternyata
    /// dibutuhkan — memangkas daftar yang sudah jalan, bukan menghadapi halaman kosong.
    ///
    /// <b>Kenapa berkas tertanam, bukan daftar di dalam kode.</b> 1.767 baris data di dalam
    /// berkas C# adalah berkas yang nol dapat ditinjau siapa pun, dan setiap pembaruan dataset
    /// menjadi diff raksasa. Tertanam di dalam DLL berarti ia <b>tidak dapat hilang saat
    /// deployment</b>, dan nol langkah "jangan lupa salin berkasnya" yang dapat terlewat.
    /// </summary>
    public static class LabSpecimenDetailTypeSeeder
    {
        private const string ResourceSuffix = "lab-specimen-detail-types.csv";

        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("LabSpecimenDetailTypeSeeder");

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                logger.LogInformation("Seeder Spesifik Specimen dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var specimenTypes = await dbContext.LabSpecimenTypes
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => x.SpecimenTypeCode, x => x.Id, cancellationToken);

            if (specimenTypes.Count == 0)
            {
                logger.LogWarning(
                    "Seeder Spesifik Specimen dilewati: data induk jenis specimen masih kosong. " +
                    "LabSpecimenTypeSeeder harus berjalan lebih dulu.");
                return;
            }

            var rows = ReadEmbeddedRows(logger);

            if (rows.Count == 0)
                return;

            // Kode yang SUDAH ADA dibaca lebih dulu, dan hanya yang belum ada yang disisipkan.
            var kodeAda = await dbContext.LabSpecimenDetailTypes
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.DetailTypeCode)
                .ToListAsync(cancellationToken);

            var sudahAda = new HashSet<string>(kodeAda, StringComparer.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;
            var baru = new List<LabSpecimenDetailType>();
            var urutan = 0;
            var jenisTidakDikenal = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                urutan++;

                if (sudahAda.Contains(row.DetailTypeCode))
                    continue;

                if (!specimenTypes.TryGetValue(row.SpecimenTypeCode, out var specimenTypeId))
                {
                    jenisTidakDikenal.Add(row.SpecimenTypeCode);
                    continue;
                }

                baru.Add(new LabSpecimenDetailType
                {
                    LabSpecimenTypeId = specimenTypeId,
                    DetailTypeCode = row.DetailTypeCode,
                    DetailTypeNameEn = row.NameEn,

                    // Nama Indonesia SENGAJA dibiarkan kosong. Ia diisi bertahap kepala
                    // instalasi (LAB-DEC-131), dan seeder nol boleh menebaknya.
                    DetailTypeNameId = null,

                    SubTypeName = string.IsNullOrWhiteSpace(row.SubTypeName) ? null : row.SubTypeName,
                    SnomedCode = string.IsNullOrWhiteSpace(row.SnomedCode) ? null : row.SnomedCode,
                    SortOrder = urutan,
                    IsActive = row.IsActive,
                    CreateDateTime = now
                });
            }

            if (jenisTidakDikenal.Count > 0)
            {
                logger.LogWarning(
                    "Seeder Spesifik Specimen melewati baris dengan kode jenis specimen tak dikenal: {Kode}.",
                    string.Join(", ", jenisTidakDikenal));
            }

            if (baru.Count == 0)
            {
                logger.LogInformation("Spesifik Specimen sudah lengkap; nol baris baru ditambahkan.");
                return;
            }

            // ============================================================
            // DI SINI LETAK BAHAYANYA, DAN IA NOL MENIMBULKAN GALAT.
            //
            // Seeder ini HANYA MENYISIPKAN. Ia nol memperbarui baris yang sudah ada.
            //
            // Seeder yang "menyegarkan" isinya setiap aplikasi menyala akan MENGHAPUS SELURUH
            // pekerjaan penerjemahan kepala instalasi pada setiap restart — DetailTypeNameId
            // yang sudah susah payah diisi kembali menjadi kosong, dan nol galat muncul.
            // Baru ketahuan berminggu-minggu kemudian, ketika seseorang bertanya kenapa
            // terjemahannya hilang lagi.
            // ============================================================
            await dbContext.LabSpecimenDetailTypes.AddRangeAsync(baru, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Seeder Spesifik Specimen menambahkan {Jumlah} baris ({Aktif} aktif, {Nonaktif} nonaktif).",
                baru.Count,
                baru.Count(x => x.IsActive),
                baru.Count(x => !x.IsActive));
        }

        private static List<CsvRow> ReadEmbeddedRows(ILogger logger)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(ResourceSuffix, StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                logger.LogError(
                    "Berkas data Spesifik Specimen tidak tertanam di dalam assembly. " +
                    "Periksa entri EmbeddedResource pada csproj.");
                return [];
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream is null)
            {
                logger.LogError("Berkas data Spesifik Specimen tidak dapat dibuka.");
                return [];
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);

            var hasil = new List<CsvRow>();
            var nomorBaris = 0;

            while (reader.ReadLine() is { } line)
            {
                nomorBaris++;

                if (nomorBaris == 1 || string.IsNullOrWhiteSpace(line))
                    continue;

                var kolom = ParseCsvLine(line);

                if (kolom.Count < 6)
                {
                    logger.LogWarning("Baris {Nomor} berkas Spesifik Specimen dilewati: kolomnya kurang.", nomorBaris);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(kolom[0]) || string.IsNullOrWhiteSpace(kolom[2]))
                    continue;

                hasil.Add(new CsvRow(
                    kolom[0].Trim().ToUpperInvariant(),
                    kolom[1].Trim(),
                    kolom[2].Trim(),
                    kolom[3].Trim().ToUpperInvariant(),
                    kolom[4].Trim(),
                    !string.Equals(kolom[5].Trim(), "false", StringComparison.OrdinalIgnoreCase)));
            }

            return hasil;
        }

        /// <summary>
        /// Pembaca CSV seadanya, dan itu cukup: hanya menangani tanda kutip ganda beserta
        /// pelolosannya, sebab hanya itu yang dipakai berkas ini.
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            var hasil = new List<string>();
            var buffer = new StringBuilder();
            var dalamKutip = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (dalamKutip)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            buffer.Append('"');
                            i++;
                        }
                        else
                        {
                            dalamKutip = false;
                        }
                    }
                    else
                    {
                        buffer.Append(c);
                    }

                    continue;
                }

                switch (c)
                {
                    case '"':
                        dalamKutip = true;
                        break;
                    case ',':
                        hasil.Add(buffer.ToString());
                        buffer.Clear();
                        break;
                    default:
                        buffer.Append(c);
                        break;
                }
            }

            hasil.Add(buffer.ToString());

            return hasil;
        }

        private sealed record CsvRow(
            string DetailTypeCode,
            string SnomedCode,
            string NameEn,
            string SpecimenTypeCode,
            string SubTypeName,
            bool IsActive);
    }
}
