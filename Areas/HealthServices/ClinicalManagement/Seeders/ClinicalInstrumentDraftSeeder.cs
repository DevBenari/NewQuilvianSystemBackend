using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Seeders
{
    /// <summary>
    /// Mengisi versi <b>Draft</b> instrumen dan formulir klinis awal — <c>BE-RWI-107</c> kriteria 3 dan 4,
    /// <c>RWI-DEC-124</c> butir 4, arsitektur 0.4 bagian 11.10.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Tidak satu versi pun disahkan di sini.</b> Seluruh versi lahir <c>Draft</c>. Pasien sungguhan
    /// baru boleh dikaji dengan versi yang disahkan pemilik klinis lewat
    /// <c>PATCH clinical-instruments/versions/{id}/approve</c>.
    /// </para>
    /// <para>
    /// <b>Batas yang bertabrakan ditandai, tidak dibereskan.</b> Tabrakan dan kekosongan pada definisi V1
    /// (<c>RWI-FACT-027</c>) ditulis ke <c>reviewFlags</c> definisi. Selama penanda masih ada, pengesahan
    /// ditolak. Contoh: keterangan layar Morse V1 menulis "25-50 Sedang" dan "≥50 Tinggi", sehingga
    /// draft memuat Sedang <c>[25, 51)</c> dan Tinggi <c>[50, null)</c> — skor 50 masuk dua kategori, dan
    /// definisi ini juga gagal validasi pita. Admin konfigurasi dan komite keperawatan yang memutuskan
    /// batasnya, bukan seeder.
    /// </para>
    /// <para>
    /// <b>Hanya menambah.</b> Instrumen yang kodenya sudah ada — termasuk yang sudah diubah pengguna — tidak
    /// pernah ditimpa. Pengubah terakhir versi seed adalah akun SuperAdmin, sehingga pengesahnya kelak
    /// pasti orang lain (<c>VAL-KEP-20a</c>).
    /// </para>
    /// </remarks>
    public static class ClinicalInstrumentDraftSeeder
    {
        public sealed record SeedResult(int InstrumentsAdded, IReadOnlyList<string> ReviewFlags);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ClinicalInstrumentDraftSeeder));

            if (!(configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true))
            {
                logger.LogInformation("Seeder draft instrumen klinis dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var systemUserId = await dbContext.Users.AsNoTracking()
                .Where(x => x.NormalizedUserName == "SUPERADMIN" || x.NormalizedEmail == "SUPERADMIN@ADMIN.COM")
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (systemUserId == Guid.Empty)
            {
                logger.LogWarning("Seeder draft instrumen klinis dilewati: akun SuperAdmin belum ada.");
                return;
            }

            var hasil = await SeedAsync(dbContext, systemUserId, cancellationToken);

            logger.LogInformation("Seeder draft instrumen klinis: {Jumlah} instrumen baru.", hasil.InstrumentsAdded);

            foreach (var penanda in hasil.ReviewFlags)
                logger.LogWarning("Penanda tinjauan instrumen klinis: {Penanda}", penanda);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var ada = await dbContext.Set<CliClinicalInstrument>().AsNoTracking()
                .Select(x => x.Code)
                .ToListAsync(cancellationToken);

            var ditambah = 0;
            var penanda = new List<string>();

            foreach (var baseline in Baselines())
            {
                if (ada.Contains(baseline.Code, StringComparer.OrdinalIgnoreCase))
                    continue;

                var json = ClinicalInstrumentDefinitionEngine.Normalize(baseline.Definition);

                dbContext.Set<CliClinicalInstrument>().Add(new CliClinicalInstrument
                {
                    Id = baseline.InstrumentId,
                    Code = baseline.Code,
                    Name = baseline.Name,
                    InstrumentKind = baseline.Kind,
                    TargetMinAgeMonths = baseline.MinAge,
                    TargetMaxAgeMonths = baseline.MaxAge,
                    Description = baseline.Description,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                dbContext.Set<CliClinicalInstrumentVersion>().Add(new CliClinicalInstrumentVersion
                {
                    Id = baseline.VersionId,
                    InstrumentId = baseline.InstrumentId,
                    VersionNumber = 1,
                    VersionStatus = ClinicalInstrumentVersionStatus.Draft,
                    DefinitionJson = json,
                    DefinitionHash = ClinicalInstrumentDefinitionEngine.Hash(json),
                    LastModifiedByUserId = actorUserId,
                    LastModifiedAt = now,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                ditambah++;
                penanda.AddRange(baseline.Definition.ReviewFlags.Select(f => $"{baseline.Code}: {f}"));

                // Kesalahan validasi ikut dilaporkan sebagai penanda, bukan diperbaiki.
                penanda.AddRange(ClinicalInstrumentDefinitionEngine.Validate(baseline.Definition).Select(e => $"{baseline.Code}: validasi — {e}"));
            }

            if (ditambah > 0)
                await dbContext.SaveChangesAsync(cancellationToken);

            return new SeedResult(ditambah, penanda);
        }

        // =====================================================================
        // Baseline — seluruh angka berasal dari V1 (RWI-FACT-027) atau skala baku bernama, dan ditandai
        // =====================================================================

        private sealed record Baseline(
            Guid InstrumentId,
            Guid VersionId,
            string Code,
            string Name,
            ClinicalInstrumentKind Kind,
            int? MinAge,
            int? MaxAge,
            string Description,
            ClinicalInstrumentDefinition Definition);

        private const string PenandaUsia =
            "Batas usia instrumen risiko jatuh adalah usulan seeder (anak < 216 bulan, dewasa 216–719 bulan, lansia ≥ 720 bulan); V1 tidak menyimpan batas usia.";

        private const string PenandaButirV1 =
            "Butir penilaian V1 dibaca dari master indikator backend V1 yang tidak tersedia pada repository ini (RWI-FACT-027: backend V1 UNKNOWN); butir wajib disusun admin konfigurasi.";

        private static IEnumerable<Baseline> Baselines()
        {
            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000001"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000001"),
                "FALL_RISK_CHILD", "Risiko Jatuh Anak (draft dari V1)", ClinicalInstrumentKind.FallRiskScale, 0, 216,
                "Draft dari form V1 'Penilaian Resiko Jatuh Anak' tanpa nama instrumen.",
                new ClinicalInstrumentDefinition
                {
                    Sections = { new() { Code = "PENILAIAN", Label = "Penilaian" } },
                    Scoring = new() { Method = "sum" },
                    Bands =
                    {
                        Band("LOW", "Rendah", 0, 25, false, "LowRisk"),
                        Band("MEDIUM", "Sedang", 25, 45, false, "MediumRisk"),
                        Band("HIGH", "Tinggi", 45, null, true, "HighRisk")
                    },
                    ReviewFlags =
                    {
                        "Batas bertabrakan pada V1: komentar kode baris 219–220 menulis 'Sedang 25–50 (overlap dengan tinggi di 45–50)', sedangkan kode baris 223–224 dan keterangan layar baris 491–493 memakai Sedang 25–44. Draft mengikuti kode dan layar; batas final ditetapkan pemilik klinis.",
                        "V1 tidak menyebut nama instrumen anak; tetapkan instrumen yang dipakai.",
                        PenandaButirV1,
                        PenandaUsia
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000002"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000002"),
                "FALL_RISK_ADULT", "Risiko Jatuh Dewasa — Morse (draft dari V1)", ClinicalInstrumentKind.FallRiskScale, 216, 720,
                "Draft dari form V1 'Penilaian Resiko Jatuh (Morse)'.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "MORSE", Label = "Morse Fall Scale",
                            Items =
                            {
                                Single("MORSE_HISTORY", "Riwayat jatuh (3 bulan terakhir)", ("NO", "Tidak", 0), ("YES", "Ya", 25)),
                                Single("MORSE_SECONDARY_DX", "Diagnosis sekunder", ("NO", "Tidak", 0), ("YES", "Ya", 15)),
                                Single("MORSE_AMBULATORY_AID", "Alat bantu jalan", ("NONE", "Tidak ada / tirah baring / dibantu perawat", 0), ("CRUTCH_CANE_WALKER", "Kruk / tongkat / walker", 15), ("FURNITURE", "Berpegangan pada perabot", 30)),
                                Single("MORSE_IV", "Terpasang infus / heparin lock", ("NO", "Tidak", 0), ("YES", "Ya", 20)),
                                Single("MORSE_GAIT", "Cara berjalan", ("NORMAL", "Normal / tirah baring / kursi roda", 0), ("WEAK", "Lemah", 10), ("IMPAIRED", "Terganggu", 20)),
                                Single("MORSE_MENTAL", "Status mental", ("ORIENTED", "Sadar akan kemampuan diri", 0), ("FORGETS_LIMITATIONS", "Lupa keterbatasan diri", 15))
                            }
                        }
                    },
                    Scoring = new() { Method = "sum" },
                    Bands =
                    {
                        Band("LOW", "Rendah", 0, 25, false, "LowRisk"),
                        Band("MEDIUM", "Sedang", 25, 51, false, "MediumRisk"),
                        Band("HIGH", "Tinggi", 50, null, true, "HighRisk")
                    },
                    ReviewFlags =
                    {
                        "Batas bertabrakan pada V1: kode baris 215–216 menetapkan Sedang 25–49 dan Tinggi ≥ 50, sedangkan keterangan layar baris 501–502 menulis '25-50 Sedang' dan '≥50 Tinggi' — skor 50 masuk dua kategori. Draft menyalin keterangan layar apa adanya dan TIDAK lolos validasi pita sampai batasnya ditetapkan.",
                        "Butir disusun dari skala Morse baku karena butir V1 dibaca dari master indikator backend V1 yang tidak tersedia; verifikasi nilai butir terhadap SOP rumah sakit.",
                        "Butir 'Alat bantu jalan' bertumpang dengan isian alat bantu pada bagian Ketergantungan Kajian Umum (INV-KEP-04, RWI-DEC-141 butir 3); tentukan satu sumber.",
                        PenandaUsia
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000003"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000003"),
                "FALL_RISK_ELDERLY", "Risiko Jatuh Lansia — Ontario Modified Stratify Sydney (draft dari V1)", ClinicalInstrumentKind.FallRiskScale, 720, null,
                "Draft dari form V1 'Ontario Modified Stratify - Sydney Scoring'.",
                new ClinicalInstrumentDefinition
                {
                    Sections = { new() { Code = "PENILAIAN", Label = "Penilaian" } },
                    Scoring = new() { Method = "sum" },
                    Bands =
                    {
                        Band("LOW", "Rendah", 0, 6, false, "LowRisk"),
                        Band("MEDIUM", "Sedang", 6, 17, false, "MediumRisk")
                    },
                    ReviewFlags =
                    {
                        "Batas berlubang pada V1: keterangan layar baris 483–484 hanya menulis 0–5 Rendah dan 6–16 Sedang tanpa kategori Tinggi, sedangkan kode baris 219 memetakan seluruh skor ≥ 6 ke Sedang. Skor di atas 16 tidak punya kategori; batas final ditetapkan pemilik klinis.",
                        PenandaButirV1,
                        PenandaUsia
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000004"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000004"),
                "GENERAL_NURSING_ASSESSMENT", "Kajian Umum Keperawatan Rawat Inap (draft)", ClinicalInstrumentKind.GeneralNursingAssessmentForm, null, null,
                "Delapan bagian RWI-DEC-141. Tanda vital ditunjuk lewat VitalSignId, tidak disalin.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new() { Code = "SUMBER_DATA", Label = "Sumber Data Pasien", Items = { Multi("SD_SOURCE", "Sumber data", ("PATIENT", "Pasien"), ("FAMILY", "Keluarga"), ("OTHER", "Lainnya")), Text("SD_RELEVANT_NOTE", "Catatan relevan (riwayat jatuh di rumah, kebiasaan)") } },
                        new()
                        {
                            Code = "KONDISI_UMUM", Label = "Kondisi Umum",
                            Items =
                            {
                                Text("KU_CHIEF_COMPLAINT", "Keluhan utama", "ChiefComplaint"),
                                Text("KU_ILLNESS_HISTORY", "Riwayat penyakit sekarang", "CurrentIllnessHistory"),
                                Text("KU_MEDICATION_HISTORY", "Riwayat obat", "MedicationHistory"),
                                Single("KU_CONSCIOUSNESS", "Kesadaran", "ConsciousnessStatus", ("ComposMentis", "Compos Mentis", null), ("Apatis", "Apatis", null), ("Somnolen", "Somnolen", null), ("Sopor", "Sopor", null), ("Coma", "Koma", null)),
                                Bool("KU_OXYGEN", "Memakai oksigen", "IsUsingOxygen"),
                                Single("KU_OXYGEN_TYPE", "Jenis alat bantu oksigen", "OxygenSupportType", ("NasalCannula", "Nasal kanul", null), ("SimpleMask", "Simple mask", null), ("NonRebreathingMask", "Non-rebreathing mask", null), ("VenturiMask", "Venturi mask", null), ("Other", "Lainnya", null)),
                                Bool("KU_ALLERGY", "Ada alergi", "HasAllergy"),
                                Text("KU_ALLERGY_NOTE", "Catatan alergi", "AllergyNote"),
                                Text("KU_PSYCHOSOCIAL", "Psikososial", "PsychosocialNote")
                            }
                        },
                        new() { Code = "PERNAPASAN", Label = "Pernapasan", Items = { Text("RESP_NOTE", "Catatan pernapasan") } },
                        new() { Code = "INTEGRITAS_KULIT", Label = "Integritas Kulit", Items = { Text("SKIN_NOTE", "Catatan integritas kulit") } },
                        new()
                        {
                            Code = "SKRINING_NUTRISI", Label = "Skrining Nutrisi",
                            Items =
                            {
                                Single("NUT_APPETITE", "Nafsu makan", "AppetiteStatus", ("Normal", "Normal", null), ("Decreased", "Menurun", null), ("Increased", "Meningkat", null), ("Poor", "Sangat buruk", null)),
                                Bool("NUT_NAUSEA", "Mual", "HasNausea"),
                                Bool("NUT_VOMITING", "Muntah", "HasVomiting"),
                                Single("NUT_RISK", "Risiko gizi", "NutritionRiskStatus", ("NoRisk", "Tidak berisiko", null), ("LowRisk", "Risiko rendah", null), ("MediumRisk", "Risiko sedang", null), ("HighRisk", "Risiko tinggi", null)),
                                Number("NUT_RISK_SCORE", "Skor skrining gizi", "NutritionRiskScore")
                            }
                        },
                        new() { Code = "ELIMINASI", Label = "Eliminasi", Items = { Bool("ELIM_CATHETER", "Kateter urin"), Text("ELIM_NOTE", "Catatan eliminasi") } },
                        new() { Code = "KETERGANTUNGAN", Label = "Ketergantungan", Items = { Multi("DEP_MOBILITY_AID", "Alat bantu mobilitas", ("WHEELCHAIR", "Kursi roda"), ("CANE", "Tongkat"), ("WALKER", "Walker")), Text("DEP_NOTE", "Catatan ketergantungan") } },
                        new()
                        {
                            Code = "STATUS_FUNGSIONAL", Label = "Status Fungsional",
                            Items =
                            {
                                Single("FUNC_STATUS", "Status fungsional", "FunctionalStatus", ("Independent", "Mandiri", null), ("NeedPartialAssistance", "Butuh bantuan sebagian", null), ("FullyDependent", "Tergantung penuh", null)),
                                Text("FUNC_NOTE", "Catatan status fungsional", "FunctionalNote")
                            }
                        }
                    },
                    ReviewFlags =
                    {
                        "Susunan delapan bagian mengikuti RWI-DEC-141; isian bagian Pernapasan, Integritas Kulit, Eliminasi, dan Ketergantungan belum dipetakan dari label V1 (RLN3-CAP-05) dan hanya berupa catatan.",
                        "Isian wajib belum ditetapkan (RWI-OQ-057); requiredItemCodes sengaja kosong."
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000005"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000005"),
                "PAIN_MONITORING", "Monitoring Nyeri (draft)", ClinicalInstrumentKind.PainScale, null, null,
                "Formulir Monitoring Nyeri PRD bagian 30. Keadaan nyeri dan skala disimpan pada kolom pengkajian.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "NYERI", Label = "Monitoring Nyeri",
                            Items =
                            {
                                Single("PAIN_STATE", "Status nyeri", "PainAssessmentState", ("NoPain", "Tidak nyeri", null), ("HasPain", "Ada nyeri", null), ("UnableToAssess", "Tidak dapat dinilai", null)),
                                Number("PAIN_SCALE", "Skala nyeri (0–10)", "PainScale"),
                                Text("PAIN_LOCATION", "Lokasi", "PainLocation"),
                                Text("PAIN_QUALITY", "Karakter", "PainQuality"),
                                Text("PAIN_TRIGGER", "Faktor pencetus", "PainTrigger"),
                                Text("PAIN_INTERVENTION", "Intervensi", "PainManagement"),
                                Text("PAIN_NOTE", "Catatan", "PainNote")
                            }
                        }
                    },
                    RequiredItemCodes = { "PAIN_STATE" },
                    ReviewFlags =
                    {
                        "Interval kajian ulang nyeri belum ditetapkan (gate G-04); reassessmentMinutes sengaja kosong sehingga waktu kajian ulang belum dihitung.",
                        "Skala per kelompok usia (wajah untuk anak, perilaku untuk pasien tidak sadar) belum ditetapkan; draft memakai satu formulir numerik."
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000006"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000006"),
                "EDUCATION_ASSESSMENT", "Assesment Edukasi (draft)", ClinicalInstrumentKind.EducationAssessmentForm, null, null,
                "Formulir PRD bagian 31 — tidak hanya satu EducationNote.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "EDUKASI", Label = "Assesment Edukasi",
                            Items =
                            {
                                Multi("EDU_RECIPIENT", "Penerima edukasi", ("PATIENT", "Pasien"), ("FAMILY", "Keluarga"), ("CAREGIVER", "Caregiver")),
                                Text("EDU_NEEDS", "Kebutuhan edukasi"),
                                Text("EDU_BARRIERS", "Hambatan"),
                                Text("EDU_MATERIAL", "Materi"),
                                Text("EDU_METHOD", "Metode"),
                                Text("EDU_UNDERSTANDING", "Evaluasi pemahaman"),
                                Text("EDU_NOTE", "Catatan edukasi", "EducationNote")
                            }
                        }
                    },
                    ReviewFlags = { "Isian dari PRD bagian 31 sebagai usulan gate G-01; isian V1 (RLN3-CAP-08) belum dipetakan." }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000007"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000007"),
                "DISCHARGE_PLANNING", "Perencanaan Pulang (draft)", ClinicalInstrumentKind.DischargePlanningForm, null, null,
                "Delapan kelompok PRD bagian 34. Tidak menutup episode.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        TextSection("DP_NEEDS", "Kebutuhan Pulang"),
                        TextSection("DP_CAREGIVER", "Caregiver / Pendamping"),
                        TextSection("DP_FOLLOWUP", "Kontrol / Follow-up"),
                        TextSection("DP_MEDICATION_EDUCATION", "Obat & Edukasi"),
                        TextSection("DP_EQUIPMENT", "Peralatan / Home Care"),
                        TextSection("DP_TRANSPORT", "Transportasi"),
                        TextSection("DP_BARRIERS", "Hambatan"),
                        new() { Code = "DP_PLAN_STATUS", Label = "Status Rencana", Items = { Text("DP_PLAN_STATUS_NOTE", "Status rencana (informatif)", "NurseNote") } }
                    },
                    ReviewFlags = { "'Status Rencana' hanya isian informatif (gate G-10); butir tiap kelompok belum ditetapkan." }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000008"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000008"),
                "CASE_MANAGEMENT_CHECKLIST", "Checklist Evaluasi Awal MPP (draft)", ClinicalInstrumentKind.CaseManagementChecklist, null, null,
                "Delapan bagian PRD bagian 33.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        TextSection("MPP_SCREENING", "Identifikasi / Skrining"),
                        TextSection("MPP_PROBLEM", "Identifikasi Masalah"),
                        TextSection("MPP_GOAL", "Harapan / Sasaran"),
                        TextSection("MPP_PLAN", "Perencanaan Pelayanan"),
                        TextSection("MPP_SUPPORT", "Dukungan"),
                        TextSection("MPP_FINANCIAL", "Aspek Finansial"),
                        TextSection("MPP_LEGAL", "Aspek Legal"),
                        TextSection("MPP_DISCHARGE", "Discharge Planning")
                    },
                    ReviewFlags = { "Butir checklist dari master /ChecklistItem V1 belum dipetakan (gate G-07); setiap bagian baru berupa catatan." }
                });
        }

        private static ClinicalInstrumentBandDefinition Band(string code, string label, decimal? min, decimal? max, bool alert, string fallRisk) =>
            new() { Code = code, Label = label, MinInclusive = min, MaxExclusive = max, IsAlert = alert, MappedFallRiskStatus = fallRisk };

        private static ClinicalInstrumentItemDefinition Single(string code, string label, params (string Code, string Label, decimal? Score)[] options) =>
            Single(code, label, null, options);

        private static ClinicalInstrumentItemDefinition Single(string code, string label, string? binding, params (string Code, string Label, decimal? Score)[] options) =>
            new()
            {
                Code = code,
                Label = label,
                Type = "single",
                Binding = binding,
                Options = options.Select(o => new ClinicalInstrumentOptionDefinition { Code = o.Code, Label = o.Label, Score = o.Score }).ToList()
            };

        private static ClinicalInstrumentItemDefinition Multi(string code, string label, params (string Code, string Label)[] options) =>
            new()
            {
                Code = code,
                Label = label,
                Type = "multi",
                Options = options.Select(o => new ClinicalInstrumentOptionDefinition { Code = o.Code, Label = o.Label }).ToList()
            };

        private static ClinicalInstrumentItemDefinition Text(string code, string label, string? binding = null) =>
            new() { Code = code, Label = label, Type = "text", Binding = binding };

        private static ClinicalInstrumentItemDefinition Bool(string code, string label, string? binding = null) =>
            new() { Code = code, Label = label, Type = "boolean", Binding = binding };

        private static ClinicalInstrumentItemDefinition Number(string code, string label, string? binding = null) =>
            new() { Code = code, Label = label, Type = "number", Binding = binding };

        private static ClinicalInstrumentSectionDefinition TextSection(string code, string label) =>
            new() { Code = code, Label = label, Items = { Text(code + "_NOTE", label) } };
    }
}
