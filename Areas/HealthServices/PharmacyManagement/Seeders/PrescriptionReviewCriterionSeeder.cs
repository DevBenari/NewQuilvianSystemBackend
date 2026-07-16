using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Seeders
{
    public static class PrescriptionReviewCriterionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, Guid actorUserId, CancellationToken ct = default)
        {
            var definitions = new[]
            {
                D("ADM_PATIENT_IDENTITY", "Identitas pasien lengkap dan sesuai", PrescriptionReviewCategory.Administrative, 10, true, true),
                D("ADM_DOCTOR_IDENTITY", "Identitas dokter dan unit pelayanan tersedia", PrescriptionReviewCategory.Administrative, 20, true, true),
                D("ADM_PRESCRIPTION_DATE", "Tanggal dan nomor resep tersedia", PrescriptionReviewCategory.Administrative, 30, true, true),
                D("PHA_DRUG_NAME_STRENGTH", "Nama obat, bentuk dan kekuatan sediaan jelas", PrescriptionReviewCategory.Pharmaceutical, 10, true, true),
                D("PHA_DOSE_QUANTITY", "Dosis, jumlah dan satuan obat jelas", PrescriptionReviewCategory.Pharmaceutical, 20, true, true),
                D("PHA_ADMINISTRATION", "Aturan dan cara penggunaan jelas", PrescriptionReviewCategory.Pharmaceutical, 30, true, true),
                D("CLI_CORRECT_DOSE", "Ketepatan dosis, frekuensi dan durasi", PrescriptionReviewCategory.Clinical, 10, true, true),
                D("CLI_DUPLICATION", "Tidak terdapat duplikasi terapi", PrescriptionReviewCategory.Clinical, 20, true, true),
                D("CLI_POLYPHARMACY", "Polifarmasi telah dinilai", PrescriptionReviewCategory.Clinical, 30, true, true),
                D("CLI_ALLERGY", "Alergi pasien telah diperiksa", PrescriptionReviewCategory.Clinical, 40, true, true, PrescriptionIssueSeverity.HardStop, true),
                D("CLI_CONTRAINDICATION", "Kontraindikasi telah diperiksa", PrescriptionReviewCategory.Clinical, 50, true, true, PrescriptionIssueSeverity.HardStop),
                D("CLI_DRUG_INTERACTION", "Interaksi obat telah diperiksa", PrescriptionReviewCategory.Clinical, 60, true, true, PrescriptionIssueSeverity.Warning, true),
                D("CMP_SOURCE_STRENGTH", "Strength dan satuan sumber racikan valid", PrescriptionReviewCategory.CompoundFormula, 10, false, true, PrescriptionIssueSeverity.HardStop),
                D("CMP_UNIT_CONVERSION", "Konversi satuan racikan valid", PrescriptionReviewCategory.CompoundFormula, 20, false, true, PrescriptionIssueSeverity.HardStop),
                D("CMP_CRUSHABILITY", "Sediaan sumber boleh digerus atau dibuka", PrescriptionReviewCategory.CompoundFormula, 30, false, true, PrescriptionIssueSeverity.HardStop),
                D("CMP_COMPATIBILITY", "Kompatibilitas dan stabilitas formula telah dinilai", PrescriptionReviewCategory.CompoundFormula, 40, false, true, PrescriptionIssueSeverity.HardStop),
                D("CMP_PACKAGING_BUD", "Kemasan, penyimpanan dan BUD dapat ditentukan", PrescriptionReviewCategory.CompoundFormula, 50, false, true)
            };

            var existingRows = await db.Set<MstPrescriptionReviewCriterion>()
                .ToListAsync(ct);
            var existing = existingRows.ToDictionary(
                x => x.CriterionCode,
                StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;
            foreach (var d in definitions)
            {
                if (existing.TryGetValue(d.Code, out var entity))
                {
                    entity.CriterionName = d.Name; entity.Category = d.Category;
                    entity.SortOrder = d.Order; entity.IsApplicableToRegular = d.Regular;
                    entity.IsApplicableToCompound = d.Compound; entity.DefaultSeverity = d.Severity;
                    entity.IsSystemCheckSupported = d.SystemSupported; entity.IsRequired = true;
                    entity.IsActive = true; entity.IsDelete = false; entity.UpdateDateTime = now; entity.UpdateBy = actorUserId;
                }
                else
                {
                    db.Set<MstPrescriptionReviewCriterion>().Add(new MstPrescriptionReviewCriterion
                    {
                        Id = Guid.NewGuid(), CriterionCode = d.Code, CriterionName = d.Name,
                        Category = d.Category, SortOrder = d.Order,
                        IsApplicableToRegular = d.Regular, IsApplicableToCompound = d.Compound,
                        DefaultSeverity = d.Severity, IsSystemCheckSupported = d.SystemSupported,
                        IsRequired = true, IsActive = true, CreateDateTime = now, CreateBy = actorUserId
                    });
                }
            }
            await db.SaveChangesAsync(ct);
        }

        private static Definition D(string code, string name, PrescriptionReviewCategory category, int order, bool regular, bool compound, PrescriptionIssueSeverity severity = PrescriptionIssueSeverity.Warning, bool systemSupported = false)
            => new(code, name, category, order, regular, compound, severity, systemSupported);
        private sealed record Definition(string Code, string Name, PrescriptionReviewCategory Category, int Order, bool Regular, bool Compound, PrescriptionIssueSeverity Severity, bool SystemSupported);
    }
}
