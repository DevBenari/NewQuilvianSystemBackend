using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    public class ConsultationValidationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PrescriptionValidationService _prescriptionValidationService;

        public ConsultationValidationService(ApplicationDbContext dbContext, PrescriptionValidationService prescriptionValidationService)
        {
            _dbContext = dbContext;
            _prescriptionValidationService = prescriptionValidationService;
        }

        public async Task<ConsultationFinalizationValidationResponse> ValidateAsync(Guid consultationId, CancellationToken cancellationToken = default)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == consultationId && !x.IsDelete, cancellationToken);

            var issues = new List<ConsultationFinalizationIssueResponse>();

            if (consultation == null)
            {
                issues.Add(Issue("CONSULTATION_NOT_FOUND", ConsultationValidationSeverity.Error, "Konsultasi dokter tidak ditemukan.", "Consultation", "soap"));
                return Build(consultationId, issues);
            }

            if (consultation.ConsultationStatus == DoctorConsultationStatus.Completed)
                issues.Add(Issue("CONSULTATION_ALREADY_COMPLETED", ConsultationValidationSeverity.Error, "Konsultasi dokter sudah diselesaikan.", "Consultation", "soap"));
            if (consultation.ConsultationStatus == DoctorConsultationStatus.Cancelled)
                issues.Add(Issue("CONSULTATION_CANCELLED", ConsultationValidationSeverity.Error, "Konsultasi dokter sudah dibatalkan.", "Consultation", "soap"));

            // BE-RWI-046 / VAL-DOK-12. Catatan harian rawat inap dinilai dengan aturan yang
            // berbeda dari catatan poliklinik, dan itu disengaja. Menuntut keempat bagian SOAP
            // beserta diagnosis utama pada setiap catatan harian membuat dokter menulis kalimat
            // kosong demi lolos validasi - itu menurunkan mutu rekam medis, bukan menaikkannya.
            // Diagnosis kerja pasien rawat inap hidup pada kajian medis, bukan diulang setiap
            // hari pada catatan perkembangan.
            //
            // Rawat jalan, medical check-up, dan IGD tidak tersentuh: penyaringnya adalah
            // keberadaan konteks perawatan pada catatan itu sendiri, kolom yang hanya terisi
            // bagi catatan yang lahir di atas perawatan rawat inap - INV-DOK-01, RWI-AC-143.
            // BE-RWI-142 / K1 (30-09-2026) MENGGANTIKAN aturan di atas untuk penyelesaian catatan
            // rawat inap. Pemilik memutuskan SOAP rawat inap mengikuti V1: keempat bagian terisi dan
            // minimal satu diagnosa ICD-10 dengan tepat satu Diagnosa Utama, dijaga di backend —
            // penjagaan di layar saja terbukti bisa bocor (catatan terkunci tanpa diagnosa terkode,
            // rencana-kerja/soap/soap.md Rev 2 C3). Aturan draf tidak berubah: menyimpan draf tetap
            // cukup dengan satu bagian terisi, karena itu diperiksa di jalur simpan, bukan di sini.
            if (consultation.InpEpisodeId.HasValue)
            {
                ValidateSoap(consultation, issues);
                await ValidateInpatientDiagnosisAsync(consultation, issues, cancellationToken);
            }
            else
            {
                ValidateSoap(consultation, issues);
                ValidateDiagnosis(consultation, issues);
            }
            issues.AddRange(await _prescriptionValidationService.ValidateForConsultationAsync(
                consultationId, consultation.EncounterId, cancellationToken));
            await ValidateProceduresAsync(consultationId, consultation.EncounterId, issues, cancellationToken);

            return Build(consultationId, issues);
        }

        private static void ValidateSoap(TrxDoctorConsultation c, List<ConsultationFinalizationIssueResponse> issues)
        {
            if (string.IsNullOrWhiteSpace(c.Subjective) && string.IsNullOrWhiteSpace(c.ChiefComplaint))
                issues.Add(Issue("MISSING_SUBJECTIVE", ConsultationValidationSeverity.Error, "Subjective atau keluhan utama wajib diisi.", "SOAP", "soap", "Subjective"));
            if (string.IsNullOrWhiteSpace(c.Objective) && string.IsNullOrWhiteSpace(c.PhysicalExamination))
                issues.Add(Issue("MISSING_OBJECTIVE", ConsultationValidationSeverity.Error, "Objective atau pemeriksaan fisik wajib diisi.", "SOAP", "soap", "Objective"));
            if (string.IsNullOrWhiteSpace(c.Assessment))
                issues.Add(Issue("MISSING_ASSESSMENT", ConsultationValidationSeverity.Error, "Assessment wajib diisi.", "SOAP", "soap", "Assessment"));
            if (string.IsNullOrWhiteSpace(c.Plan))
                issues.Add(Issue("MISSING_PLAN", ConsultationValidationSeverity.Error, "Plan wajib diisi.", "SOAP", "soap", "Plan"));
        }

        /// <summary>
        /// Syarat diagnosa penyelesaian SOAP rawat inap — <c>BE-RWI-142</c>, keputusan K1.
        /// </summary>
        /// <remarks>
        /// Dibaca dari baris <c>TrxPatientDiagnosis</c> milik catatan itu, bukan dari ringkasan pada
        /// catatan, supaya tidak bergantung pada urutan penyegaran ringkasan. Kalimatnya sama dengan
        /// banner di layar. Contoh: catatan dengan J18.0 (Utama) dan E86 lolos; catatan tanpa diagnosa
        /// ditolak <c>MISSING_ICD10_DIAGNOSIS</c>; catatan dengan dua diagnosa tanpa Utama ditolak
        /// <c>MISSING_PRIMARY_DIAGNOSIS</c>.
        /// </remarks>
        private async Task ValidateInpatientDiagnosisAsync(
            TrxDoctorConsultation c,
            List<ConsultationFinalizationIssueResponse> issues,
            CancellationToken cancellationToken)
        {
            var diagnosa = await _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    x.ConsultationId == c.Id &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled)
                .Select(x => new { x.IsPrimary })
                .ToListAsync(cancellationToken);

            if (diagnosa.Count == 0)
            {
                issues.Add(Issue("MISSING_ICD10_DIAGNOSIS", ConsultationValidationSeverity.Error, "Silakan pilih minimal satu diagnosa ICD-10 sebelum menyelesaikan SOAP.", "Diagnosis", "diagnosis"));
                return;
            }

            var jumlahUtama = diagnosa.Count(x => x.IsPrimary);

            if (jumlahUtama == 0)
                issues.Add(Issue("MISSING_PRIMARY_DIAGNOSIS", ConsultationValidationSeverity.Error, "Tentukan satu Diagnosa Utama sebelum menyelesaikan SOAP.", "Diagnosis", "diagnosis"));
            else if (jumlahUtama > 1)
                issues.Add(Issue("MULTIPLE_PRIMARY_DIAGNOSIS", ConsultationValidationSeverity.Error, "Diagnosa Utama hanya boleh satu.", "Diagnosis", "diagnosis"));
        }

        private static void ValidateDiagnosis(TrxDoctorConsultation c, List<ConsultationFinalizationIssueResponse> issues)
        {
            if (!c.HasPrimaryDiagnosis || c.DiagnosisCount <= 0)
                issues.Add(Issue("MISSING_PRIMARY_DIAGNOSIS", ConsultationValidationSeverity.Error, "Diagnosis utama wajib tersedia sebelum konsultasi diselesaikan.", "Diagnosis", "diagnosis"));
        }

        /// <summary>
        /// Memvalidasi tindakan yang menempel pada konsultasi.
        ///
        /// Dua pemeriksaan terakhir ditambahkan <c>RJ-DOC-BE-002</c> untuk memenuhi kontrak
        /// <c>RJ-DOC-COMPLETION-001@1.0.0</c> bagian 1.6 — *clinical order state tidak
        /// authoritative*. Keduanya memakai state yang sudah ada; tidak ada status baru.
        /// </summary>
        private async Task ValidateProceduresAsync(
            Guid consultationId,
            Guid expectedEncounterId,
            List<ConsultationFinalizationIssueResponse> issues,
            CancellationToken cancellationToken)
        {
            var procedures = await _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .Where(x => x.ConsultationId == consultationId && !x.IsDelete && !x.IsCancel && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var item in procedures)
            {
                if (item.Quantity <= 0)
                    issues.Add(Issue("INVALID_PROCEDURE_QUANTITY", ConsultationValidationSeverity.Error, $"Jumlah tindakan {item.ProcedureNameSnapshot} harus lebih dari 0.", "Procedure", "procedure", "Quantity", "PatientProcedure", item.Id));
                if (item.IsBillable && !item.IsFreeOfCharge && !item.TariffId.HasValue)
                    issues.Add(Issue("MISSING_PROCEDURE_TARIFF", ConsultationValidationSeverity.Error, $"Tarif tindakan {item.ProcedureNameSnapshot} belum tersedia.", "Procedure", "procedure", "TariffId", "PatientProcedure", item.Id));
                if (item.IsNeedApproval && !item.IsApproved)
                    issues.Add(Issue("UNAPPROVED_PROCEDURE", ConsultationValidationSeverity.Error, $"Tindakan {item.ProcedureNameSnapshot} membutuhkan approval.", "Procedure", "procedure", null, "PatientProcedure", item.Id));

                // Baris tindakan yang berstatus dibatalkan tetapi tidak ditandai batal adalah
                // keadaan yang tidak dapat dipastikan: ia lolos penyaring baris aktif, ikut
                // terhitung sebagai tindakan konsultasi, dan akan dibawa ke hilir seolah sah.
                if (item.ProcedureStatus == PatientProcedureStatus.Cancelled)
                    issues.Add(Issue("INCONSISTENT_PROCEDURE_STATUS", ConsultationValidationSeverity.Error, $"Status tindakan {item.ProcedureNameSnapshot} dibatalkan tetapi barisnya masih aktif.", "Procedure", "procedure", "ProcedureStatus", "PatientProcedure", item.Id));

                // Tindakan menyimpan kunjungan dan konsultasi secara terpisah, sehingga keduanya
                // dapat berbeda. Bila berbeda, tindakan ini bukan milik kunjungan yang sedang
                // diselesaikan dan tidak boleh ikut difinalisasi.
                if (expectedEncounterId != Guid.Empty && item.EncounterId != expectedEncounterId)
                    issues.Add(Issue("PROCEDURE_ENCOUNTER_MISMATCH", ConsultationValidationSeverity.Error, $"Tindakan {item.ProcedureNameSnapshot} tidak menempel pada kunjungan yang sama dengan konsultasinya.", "Procedure", "procedure", "EncounterId", "PatientProcedure", item.Id));
            }
        }

        private static ConsultationFinalizationValidationResponse Build(Guid consultationId, List<ConsultationFinalizationIssueResponse> issues)
        {
            var sections = issues
                .GroupBy(x => new { x.Section, x.TabKey })
                .Select(g => new ConsultationFinalizationSectionResponse
                {
                    Section = g.Key.Section,
                    TabKey = g.Key.TabKey,
                    ErrorCount = g.Count(x => x.Severity == ConsultationValidationSeverity.Error),
                    WarningCount = g.Count(x => x.Severity == ConsultationValidationSeverity.Warning),
                    InformationCount = g.Count(x => x.Severity == ConsultationValidationSeverity.Information),
                    Issues = g.ToList()
                })
                .ToList();

            var errors = issues.Count(x => x.Severity == ConsultationValidationSeverity.Error);
            var warnings = issues.Count(x => x.Severity == ConsultationValidationSeverity.Warning);

            return new ConsultationFinalizationValidationResponse
            {
                ConsultationId = consultationId,
                CanFinalize = errors == 0,
                RequiresWarningAcknowledgement = warnings > 0,
                ErrorCount = errors,
                WarningCount = warnings,
                InformationCount = issues.Count(x => x.Severity == ConsultationValidationSeverity.Information),
                Sections = sections
            };
        }

        private static ConsultationFinalizationIssueResponse Issue(string code, ConsultationValidationSeverity severity, string message, string section, string tabKey, string? field = null, string? entityType = null, Guid? entityId = null)
        {
            return new ConsultationFinalizationIssueResponse
            {
                Code = code,
                Severity = severity,
                Message = message,
                Section = section,
                TabKey = tabKey,
                Field = field,
                EntityType = entityType,
                EntityId = entityId,
                IssueKey = $"{code}:{entityType ?? section}:{entityId?.ToString() ?? "general"}"
            };
        }
    }
}
