using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Mengisi master lokasi penyimpanan obat (<see cref="MstDrugStorageLocation"/>) dari daftar
    /// gudang resmi rumah sakit. Seeder ini dipakai di pengembangan maupun produksi, sehingga
    /// identitas barisnya harus sama persis di kedua lingkungan.
    /// </summary>
    /// <remarks>
    /// <b>Mengapa kode baris berupa nomor urut, bukan singkatan nama.</b>
    ///
    /// Pemilik data meminta satu hal yang tegas: id dan nama baris tidak boleh berubah antara
    /// dev dan produksi. Karena itu <see cref="MstDrugStorageLocation.Id"/> tidak diundi, tetapi
    /// diturunkan secara deterministik dari <see cref="MstDrugStorageLocation.StorageLocationCode"/>
    /// lewat <see cref="DeterministicId"/>. Konsekuensinya kode adalah kunci identitas yang
    /// sesungguhnya, dan kode yang diturunkan dari nama akan ikut berubah setiap kali ejaan nama
    /// dikoreksi - dan ikut mengubah id baris yang sudah terlanjur dirujuk stok, mutasi, dan
    /// resep. Nomor urut pada daftar sumber tidak punya persoalan itu, jadi itulah yang dipakai:
    /// <c>GDG-001</c> sampai <c>GDG-123</c>. Nama boleh dikoreksi kapan saja tanpa memindahkan id.
    ///
    /// <b>Yang diisi seeder dan yang sengaja dibiarkan kosong.</b>
    ///
    /// Daftar sumber hanya memuat tiga hal: nama gudang, penanda apotek, dan cost center. Maka
    /// hanya itu yang ditulis. <see cref="MstDrugStorageLocation.ServiceUnitId"/>,
    /// <see cref="MstDrugStorageLocation.ClinicId"/>, dan <see cref="MstDrugStorageLocation.RoomId"/>
    /// dibiarkan <c>null</c> karena daftar sumber tidak menyebutkannya, dan menebaknya berarti
    /// mengarang tautan master. Selama ketiganya kosong, routing depo pada
    /// <c>PharmacyDepotRoutingService</c> akan menjawab <c>PHA_ROUTE_NOT_FOUND</c>, bukan
    /// <c>PHA_ROUTE_AMBIGUOUS</c> - gagal secara jujur, bukan salah memilih depo. Penautan ke unit
    /// layanan dan klinik adalah pekerjaan admin lewat layar master, bukan pekerjaan seeder.
    ///
    /// Penanda <c>Apotek?</c> pada daftar sumber dibaca sebagai kewenangan menyerahkan obat ke
    /// pasien, sehingga dipetakan ke <see cref="MstDrugStorageLocation.IsPharmacyLocation"/> dan
    /// <see cref="MstDrugStorageLocation.IsAllowDispensing"/>. Hanya tiga baris yang bertanda itu.
    /// Sisanya tetap boleh menerima dan memindahkan stok, tetapi tidak menyerahkan ke pasien.
    ///
    /// <b>Seeder tidak pernah menimpa.</b>
    ///
    /// Baris yang id-nya sudah ada dilewati apa adanya, termasuk bila admin sudah menyunting nama,
    /// menautkan unit layanan, atau menonaktifkannya. Seeder ini menambah yang belum ada; ia bukan
    /// alat untuk mengembalikan master ke kondisi pabrik.
    /// </remarks>
    public static class DrugStorageLocationSeeder
    {
        /// <summary>
        /// Pembeda ruang nama hash. Jangan pernah diubah: mengubahnya memindahkan seluruh id.
        /// </summary>
        private const string IdNamespace = "MstDrugStorageLocation";

        private sealed record SeedRow(
            int SortOrder,
            string Code,
            string Name,
            string? CostCenterName,
            bool IsPharmacy);

        private static readonly SeedRow[] Locations =
        {
        new(  1, "GDG-001", "ADMISSION RAWAT INAP", "UNIT PPRI", false),
        new(  2, "GDG-002", "AKUPUNTUR", "AKUPUNTUR", false),
        new(  3, "GDG-003", "ANAK", "ANAK", false),
        new(  4, "GDG-004", "ANDROLOGI", "ANDROLOGI", false),
        new(  5, "GDG-005", "APS / RETAIL", "APS / RETAIL", false),
        new(  6, "GDG-006", "Bank Darah", "LABORATORIUM", false),
        new(  7, "GDG-007", "BEDAH", "BEDAH", false),
        new(  8, "GDG-008", "BEDAH PLASTIK", "BEDAH PLASTIK", false),
        new(  9, "GDG-009", "CASE MANAGER", "CASE MANAGER", false),
        new( 10, "GDG-010", "CSR", "CSR", false),
        new( 11, "GDG-011", "CSSD", "LANTAI 2 KAMAR OPERASI", true ),
        new( 12, "GDG-012", "DEPARTEMEN DAN INSTALASI", "DEPARTEMEN DAN INSTALASI", false),
        new( 13, "GDG-013", "DEPARTEMEN KOMUNIKASI", "DEPARTEMEN KOMUNIKASI", false),
        new( 14, "GDG-014", "DEPARTEMEN PELAYANAN MEDIK", "DEPARTEMEN PELAYANAN MEDIK", false),
        new( 15, "GDG-015", "DEPARTEMEN PEMASARAN & KERJASA", "DEPARTEMEN PEMASARAN & KERJASA", false),
        new( 16, "GDG-016", "DEPARTEMEN PENUNJANG MEDIK", "DEPARTEMEN PENUNJANG MEDIK", false),
        new( 17, "GDG-017", "DEPARTEMEN KEPERAWATAN", "DEPARTEMEN KEPERAWATAN", false),
        new( 18, "GDG-018", "DIREKSI", "DIREKSI", false),
        new( 19, "GDG-019", "DUTY MANAGER", "Duty Manager", false),
        new( 20, "GDG-020", "EKSEKUTIF SALES INSURANCE", "SALES INSURANCE", false),
        new( 21, "GDG-021", "ENDOSCOPY", "LANTAI 2 KAMAR OPERASI", false),
        new( 22, "GDG-022", "FARMASI RAWAT INAP", "FARMASI RAWAT INAP", true ),
        new( 23, "GDG-023", "FARMASI RAWAT JALAN", "FARMASI RAWAT JALAN", true ),
        new( 24, "GDG-024", "FISIOTERAPI", "REHABILITAS MEDIK / FISIOTERAPI", false),
        new( 25, "GDG-025", "GIGI", "GIGI", false),
        new( 26, "GDG-026", "GIGI SPESIALIS LANTAI 7", "GIGI SPESIALIS LANTAI 7", false),
        new( 27, "GDG-027", "GIZI", "GIZI", false),
        new( 28, "GDG-028", "GUDANG FARMASI PUSAT", "UNIT GUDANG", false),
        new( 29, "GDG-029", "HEMODIALISA", "HEMODIALISA", false),
        new( 30, "GDG-030", "INSTALASI FARMASI", "FARMASI", false),
        new( 31, "GDG-031", "INSTALASI GAWAT DARURAT (IGD)", "INSTALASI GAWAT DARURAT (IGD)", false),
        new( 32, "GDG-032", "INSTALASI RAWAT INAP", "INSTALASI RAWAT INAP", false),
        new( 33, "GDG-033", "INSURANCE RELATIONS", "INSURANCE RELATIONS", false),
        new( 34, "GDG-034", "INTERNIS", "INTERNIS", false),
        new( 35, "GDG-035", "KASIR", "KASIR", false),
        new( 36, "GDG-036", "KATO OJIN", "GIGI SPESIALIS LANTAI 7", false),
        new( 37, "GDG-037", "KEDOKTERAN KELAUTAN", "KEDOKTERAN KELAUTAN", false),
        new( 38, "GDG-038", "KEDOKTERAN OLAHRAGA", "KEDOKTERAN OLAHRAGA", false),
        new( 39, "GDG-039", "KEDOKTERAN PENERBANGAN", "KEDOKTERAN PENERBANGAN", false),
        new( 40, "GDG-040", "KESEHATAN SDM", "KESEHATAN SDM", false),
        new( 41, "GDG-041", "KOMITE-KOMITE", "KOMITE - KOMITE", false),
        new( 42, "GDG-042", "KOMITE MEDIK", "KOMITE MEDIK", false),
        new( 43, "GDG-043", "KOMITE PMKP", "Panitia Mutu", false),
        new( 44, "GDG-044", "KOMITE PPI", "Panitia PPI", false),
        new( 45, "GDG-045", "KULIT & KELAMIN", "KULIT & KELAMIN", false),
        new( 46, "GDG-046", "LABORATORIUM", "LABORATORIUM", false),
        new( 47, "GDG-047", "LABORATORIUM BANK DARAH", "LABORATORIUM BANK DARAH", false),
        new( 48, "GDG-048", "LABORATORIUM MICROBIOLOGI", "LABORATORIUM MICRIOBIOLOGI", false),
        new( 49, "GDG-049", "LABORATORIUM PATOLOGI ANATOMI", "LABORATORIUM PATOLOGI ANATOMI", false),
        new( 50, "GDG-050", "LABORATORIUM PATOLOGI KLINIK", "LABORATORIUM PATOLOGI KLINIK", false),
        new( 51, "GDG-051", "LANTAI 2-CATH-LAB", "LANTAI 2 CATH-LAB", false),
        new( 52, "GDG-052", "LANTAI 2-ICCU", "LANTAI 2 ICCU", false),
        new( 53, "GDG-053", "LANTAI 2-ICU", "LANTAI 2 ICU", false),
        new( 54, "GDG-054", "LANTAI 2-ICU VIP", "LANTAI 2 ICU", false),
        new( 55, "GDG-055", "LANTAI 2-KAMAR OPERASI", "LANTAI 2 KAMAR OPERASI", false),
        new( 56, "GDG-056", "LANTAI 2-KEBIDANAN DAN KANDUNGAN", "LANTAI 2 KEBIDANAN DAN KANDUNGAN", false),
        new( 57, "GDG-057", "LANTAI 2-PERINA & PERINATALOGI", "LANTAI 2 PERINA & PERINATALOGI", false),
        new( 58, "GDG-058", "LANTAI 3", "LANTAI 3", false),
        new( 59, "GDG-059", "LANTAI 4", "LANTAI 4", false),
        new( 60, "GDG-060", "LANTAI 5", "LANTAI 5", false),
        new( 61, "GDG-061", "LANTAI 8", "LANTAI 8", false),
        new( 62, "GDG-062", "LANTAI 9", "LANTAI 9", false),
        new( 63, "GDG-063", "MATA", "MATA", false),
        new( 64, "GDG-064", "MEDICAL CHECK UP (MCU)", "MEDICAL CHECK UP (MCU)", false),
        new( 65, "GDG-065", "NEUROLOGI (SYARAF)", "NEUROLOGI (SYARAF)", false),
        new( 66, "GDG-066", "OBSGIN (KEBIDANAN & KANDUNGAN)", "OBSGIN (KEBIDANAN & KANDUNGAN)", false),
        new( 67, "GDG-067", "OLAH RAGA", "OLAH RAGA", false),
        new( 68, "GDG-068", "ONE DAY CARE", "ONE DAY CARE", false),
        new( 69, "GDG-069", "Outreach", "PENDAPATAN LAIN", false),
        new( 70, "GDG-070", "PANITIA-PANITIA", "PANITIA - PANITIA", false),
        new( 71, "GDG-071", "PANITIA PROMKES", "Panitia Promkes", false),
        new( 72, "GDG-072", "Poliklinik", "Poliklinik", false),
        new( 73, "GDG-073", "PRODUCT DEVELOPMENT", "PRODUCT DEVELOPMENT", false),
        new( 74, "GDG-074", "PSIKIATRI (KEDOKTERAN JIWA)", "PSIKIATRI (KEDOKTERAN JIWA)", false),
        new( 75, "GDG-075", "PSIKOLOGI", "PSIKOLOGI", false),
        new( 76, "GDG-076", "PULMONOLOGI (PARU)", "PULMONOLOGI (PARU)", false),
        new( 77, "GDG-077", "RADIOLOGI", "RADIOLOGI", false),
        new( 78, "GDG-078", "REHAB MEDIK", "REHABILITAS MEDIK / FISIOTERAPI", false),
        new( 79, "GDG-079", "SALES CORPORATE", "SALES CORPORATE", false),
        new( 80, "GDG-080", "SUPPLIER", null, false),
        new( 81, "GDG-081", "THT", "THT", false),
        new( 82, "GDG-082", "TIM CASEMIX", "Tim Casemix", false),
        new( 83, "GDG-083", "TIM-TIM", "TIM - TIM", false),
        new( 84, "GDG-084", "UMUM", "UMUM", false),
        new( 85, "GDG-085", "UNIT AKUNTANSI & PAJAK", "UNIT AKUNTANSI & PAJAK", false),
        new( 86, "GDG-086", "UNIT BUDGETING & COSTING", "UNIT BUDGETING & COSTING", false),
        new( 87, "GDG-087", "UNIT CLUSTER MANAGER", "UNIT CLUSTER MANAGER", false),
        new( 88, "GDG-088", "UNIT CRM", "UNIT VIP SERVICE / PHA", false),
        new( 89, "GDG-089", "UNIT DIGITAL MARKETING", "DIGITAL MARKETING", false),
        new( 90, "GDG-090", "UNIT DIKLAT", "UNIT DIKLAT", false),
        new( 91, "GDG-091", "UNIT FINANCIAL & BUSINESS ANALYS", "UNIT FINANCIAL & BUSINESS ANALYSIS", false),
        new( 92, "GDG-092", "UNIT GIZI MEDIK", "UNIT GIZI MEDIK", false),
        new( 93, "GDG-093", "UNIT GUDANG", "UNIT GUDANG", false),
        new( 94, "GDG-094", "UNIT GUDANG UMUM", "UNIT GUDANG", false),
        new( 95, "GDG-095", "UNIT HUMAN CAPITAL POLICY", "UNIT HUMAN CAPITAL POLICY", false),
        new( 96, "GDG-096", "UNIT HUMAN CAPITAL SUPPORT", "UNIT HUMAN CAPITAL SUPPORT", false),
        new( 97, "GDG-097", "UNIT KERJASAMA", "UNIT KERJASAMA", false),
        new( 98, "GDG-098", "UNIT KESEHATAN & KESELAMATAN KER", "Panitia P2K3", false),
        new( 99, "GDG-099", "UNIT KESEHATAN LINGKUNGAN", "UNIT KESEHATAN LINGKUNGAN", false),
        new(100, "GDG-100", "UNIT KESEHATAN SDM", "UNIT KESEHATAN SDM", false),
        new(101, "GDG-101", "UNIT LEGAL", "UNIT LEGAL", false),
        new(102, "GDG-102", "UNIT MANAJEMEN ASET", "MANAJEMEN ASSET", false),
        new(103, "GDG-103", "UNIT PDS", "UNIT PDS / KOMUNIKASI", false),
        new(104, "GDG-104", "UNIT PEMASARAN", "UNIT PEMASARAN", false),
        new(105, "GDG-105", "UNIT PEMBELIAN", "UNIT PEMBELIAN", false),
        new(106, "GDG-106", "UNIT PENERIMAAN", "UNIT PENERIMAAN", false),
        new(107, "GDG-107", "UNIT PENGELUARAN", "UNIT PENGELUARAN", false),
        new(108, "GDG-108", "UNIT PENUNJANG PELAYANAN PASIEN", "UNIT PENUNJANG PELAYANAN PASIEN", false),
        new(109, "GDG-109", "UNIT PENUNJANG UMUM", "UNIT PENUNJANG UMUM", false),
        new(110, "GDG-110", "UNIT PERANGKAT & JARINGAN", "UNIT PERANGKAT & JARINGAN", false),
        new(111, "GDG-111", "UNIT PPRI", "UNIT PPRI", false),
        new(112, "GDG-112", "UNIT REGISTRATION INFORMATION CEN", "UNIT REGISTRATION INFORMATION CEN", false),
        new(113, "GDG-113", "UNIT REKAM MEDIS", "UNIT REKAM MEDIS", false),
        new(114, "GDG-114", "UNIT SATPAM", "UNIT SATPAM", false),
        new(115, "GDG-115", "UNIT SATUAN PENGAWAS INTERNAL", "UNIT SATUAN PENGAWAS INTERNAL", false),
        new(116, "GDG-116", "UNIT SEKRETARIAT", "UNIT SEKRETARIAT", false),
        new(117, "GDG-117", "UNIT SISTEM INFORMASI & APLIKASI", "UNIT SISTEM INFORMASI & APLIKASI", false),
        new(118, "GDG-118", "UNIT TALENT MANAGEMENT", "UNIT TALENT MANAGEMENT", false),
        new(119, "GDG-119", "UNIT TEKNIK BIOMEDIK", "UNIT TEKNIK BIOMEDIK", false),
        new(120, "GDG-120", "UNIT TEKNIK UMUM", "UNIT TEKNIK UMUM", false),
        new(121, "GDG-121", "UNIT VIP SERVICE", "UNIT VIP SERVICE / PHA", false),
        new(122, "GDG-122", "UNIT WAREHOUSE & DATA ANALYSIS", "UNIT WAREHOUSE & DATA ANALYSIS", false),
        new(123, "GDG-123", "UPK", "FARMASI RAWAT INAP", false),
        };

        /// <summary>
        /// Id baris diturunkan dari kode, bukan diundi, supaya baris yang sama memiliki id yang
        /// sama di setiap lingkungan tanpa perlu menyalin data antar basis data.
        /// </summary>
        public static Guid DeterministicId(string storageLocationCode) =>
            new(SHA256.HashData(
                Encoding.UTF8.GetBytes($"{IdNamespace}:{storageLocationCode.Trim().ToUpperInvariant()}"))[..16]);

        public static async Task<DrugStorageLocationSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var result = new DrugStorageLocationSeedResult();

            var existingIds = await db.Set<MstDrugStorageLocation>()
                .AsNoTracking()
                .Select(x => x.Id)
                .ToListAsync(ct);

            var existing = existingIds.ToHashSet();
            var now = DateTime.UtcNow;

            foreach (var row in Locations)
            {
                var id = DeterministicId(row.Code);

                if (existing.Contains(id))
                {
                    result.Skipped++;
                    continue;
                }

                db.Set<MstDrugStorageLocation>().Add(new MstDrugStorageLocation
                {
                    Id = id,
                    StorageLocationCode = row.Code,
                    StorageLocationName = row.Name,
                    StorageLocationType = "General",
                    LocationGroupName = row.CostCenterName,
                    IsPharmacyLocation = row.IsPharmacy,
                    IsAllowDispensing = row.IsPharmacy,
                    IsAllowReceiving = true,
                    IsAllowTransferIn = true,
                    IsAllowTransferOut = true,
                    IsDefault = false,
                    IsMainWarehouse = false,
                    SortOrder = row.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.Inserted++;
            }

            if (result.Inserted > 0)
                await db.SaveChangesAsync(ct);

            return result;
        }
    }

    public class DrugStorageLocationSeedResult
    {
        public int Inserted { get; set; }

        public int Skipped { get; set; }
    }
}
