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
                {
                    var existingDraftVersion = await dbContext.Set<CliClinicalInstrumentVersion>()
                        .FirstOrDefaultAsync(x => x.Id == baseline.VersionId && x.VersionStatus == ClinicalInstrumentVersionStatus.Draft, cancellationToken);
                    if (existingDraftVersion != null)
                    {
                        var updatedJson = ClinicalInstrumentDefinitionEngine.Normalize(baseline.Definition);
                        var updatedHash = ClinicalInstrumentDefinitionEngine.Hash(updatedJson);
                        if (existingDraftVersion.DefinitionHash != updatedHash)
                        {
                            existingDraftVersion.DefinitionJson = updatedJson;
                            existingDraftVersion.DefinitionHash = updatedHash;
                            existingDraftVersion.UpdateDateTime = now;
                            existingDraftVersion.UpdateBy = actorUserId;
                            existingDraftVersion.LastModifiedAt = now;
                            existingDraftVersion.LastModifiedByUserId = actorUserId;
                            ditambah++;
                        }
                    }
                    continue;
                }

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
                "FALL_RISK_CHILD", "Risiko Jatuh Anak — Humpty Dumpty (draft)", ClinicalInstrumentKind.FallRiskScale, 0, 216,
                "Penilaian risiko jatuh pasien anak (< 18 tahun) menggunakan skala baku Humpty Dumpty Fall Scale.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "HUMPTY_DUMPTY",
                            Label = "Penilaian Risiko Jatuh Anak (Humpty Dumpty)",
                            Items =
                            {
                                Single("HUMP_AGE", "Usia Pasien",
                                    ("UNDER_3", "< 3 tahun", 4),
                                    ("3_TO_7", "3 - 7 tahun", 3),
                                    ("7_TO_13", "7 - 13 tahun", 2),
                                    ("OVER_13", "≥ 13 tahun", 1)),
                                Single("HUMP_GENDER", "Jenis Kelamin",
                                    ("MALE", "Laki-laki", 2),
                                    ("FEMALE", "Perempuan", 1)),
                                Single("HUMP_DIAGNOSIS", "Diagnosis Medis",
                                    ("NEUROLOGICAL", "Kelainan neurologi", 4),
                                    ("OXYGENATION", "Perubahan oksigenasi / gangguan pernapasan / dehidrasi / anemia / sinkop", 3),
                                    ("PSYCHOLOGICAL", "Masalah perilaku / psikis", 2),
                                    ("OTHER_DX", "Diagnosis medis lain", 1)),
                                Single("HUMP_COGNITIVE", "Gangguan Kognitif",
                                    ("UNAWARE", "Tidak sadar akan keterbatasan diri", 3),
                                    ("FORGETS", "Lupa akan keterbatasan diri", 2),
                                    ("AWARE", "Mengetahui kemampuan diri", 1)),
                                Single("HUMP_ENVIRONMENT", "Faktor Lingkungan",
                                    ("FALL_HISTORY_BED", "Riwayat jatuh / bayi di tempat tidur khusus / boks", 4),
                                    ("AID_REGULAR_BED", "Pasien memakai alat bantu / bayi di ranjang standar", 3),
                                    ("REGULAR_BED", "Pasien di tempat tidur standar tanpa bantuan", 2),
                                    ("OUTPATIENT", "Area rawat jalan / di luar ranjang", 1)),
                                Single("HUMP_SURGERY", "Pembedahan / Sedasi / Anestesi",
                                    ("WITHIN_24H", "Dalam 24 jam terakhir", 3),
                                    ("WITHIN_48H", "Dalam 48 jam terakhir", 2),
                                    ("OVER_48H_NONE", "> 48 jam / Tidak ada tindakan pembedahan", 1)),
                                Single("HUMP_MEDICATION", "Penggunaan Obat",
                                    ("MULTIPLE_SEDATIVES", "Bermacam obat (sedatif, hipnotik, antikonvulsan, laksatif, diuretik, narkotik)", 3),
                                    ("SINGLE_SEDATIVE", "Salah satu dari obat di atas", 2),
                                    ("OTHER_NONE", "Obat lain / Tanpa obat berisiko", 1))
                            }
                        },
                        new()
                        {
                            Code = "INTERVENSI",
                            Label = "Checklist Intervensi Pencegahan Jatuh (SOP Rumah Sakit)",
                            Items =
                            {
                                Bool("INT_ORIENT_ROOM", "Orientasikan ruangan dan letak bel panggil darurat"),
                                Bool("INT_BED_LOCKED", "Posisi tempat tidur terendah dan roda terkunci aman"),
                                Bool("INT_BED_RAILS", "Pasang pagar pengaman tempat tidur (bed rails) di kedua sisi"),
                                Bool("INT_YELLOW_SIGN", "Pasang penanda visual risiko jatuh (segitiga kuning pada ranjang/pintu)"),
                                Bool("INT_YELLOW_WRIST", "Pasang GELANG KUNING Risiko Jatuh pada pergelangan tangan (Wajib Risiko Tinggi)"),
                                Bool("INT_FAMILY_EDU", "Edukasi pencegahan jatuh kepada pasien dan keluarga/penunggu"),
                                Bool("INT_ASSIST_AMB", "Bantu dan dampingi saat mobilisasi dan ke toilet"),
                                Bool("INT_MONITOR_2H", "Pantau kondisi pasien secara berkala tiap 2 jam (Wajib Risiko Tinggi)"),
                                Text("INT_NOTE", "Catatan Tindakan Tambahan")
                            }
                        }
                    },
                    Scoring = new() { Method = "sum" },
                    Bands =
                    {
                        Band("LOW", "Rendah", 0, 12, false, "LowRisk"),
                        Band("HIGH", "Tinggi", 12, null, true, "HighRisk")
                    },
                    ReviewFlags =
                    {
                        "Skala baku Humpty Dumpty Fall Scale untuk pasien anak (< 18 tahun) dengan 7 parameter penilaian.",
                        PenandaUsia
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000002"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000002"),
                "FALL_RISK_ADULT", "Risiko Jatuh Dewasa — Morse (draft)", ClinicalInstrumentKind.FallRiskScale, 216, 720,
                "Penilaian risiko jatuh pasien dewasa (18–59 tahun) menggunakan skala baku Morse Fall Scale.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "MORSE", Label = "Morse Fall Scale",
                            Items =
                            {
                                Single("MORSE_HISTORY", "Riwayat jatuh (3 bulan terakhir)", ("NO", "Tidak (Tidak pernah jatuh)", 0), ("YES", "Ya (Pernah jatuh dalam 3 bulan terakhir)", 25)),
                                Single("MORSE_SECONDARY_DX", "Diagnosis sekunder (≥ 2 diagnosis medis)", ("NO", "Tidak (Hanya 1 diagnosis medis)", 0), ("YES", "Ya (Ada 2 atau lebih diagnosis medis)", 15)),
                                Single("MORSE_AMBULATORY_AID", "Alat bantu jalan", ("NONE", "Tidak ada / tirah baring / kursi roda / dibantu perawat", 0), ("CRUTCH_CANE_WALKER", "Kruk / tongkat / walker", 15), ("FURNITURE", "Berpegangan pada perabot / dinding", 30)),
                                Single("MORSE_IV", "Terpasang infus / heparin lock", ("NO", "Tidak", 0), ("YES", "Ya (Terpasang terapi intravena)", 20)),
                                Single("MORSE_GAIT", "Cara berjalan / gaya berjalan (gait)", ("NORMAL", "Normal / tirah baring / kursi roda / imobil", 0), ("WEAK", "Lemah (langkah pendek, diseret)", 10), ("IMPAIRED", "Terganggu (langkah goyah, hilang keseimbangan)", 20)),
                                Single("MORSE_MENTAL", "Status mental", ("ORIENTED", "Sadar akan kemampuan diri", 0), ("FORGETS_LIMITATIONS", "Lupa keterbatasan diri / overestimasi kemampuan", 15))
                            }
                        },
                        new()
                        {
                            Code = "INTERVENSI",
                            Label = "Checklist Intervensi Pencegahan Jatuh (SOP Rumah Sakit)",
                            Items =
                            {
                                Bool("INT_ORIENT_ROOM", "Orientasikan ruangan dan letak bel panggil darurat"),
                                Bool("INT_BED_LOCKED", "Posisi tempat tidur terendah dan roda terkunci aman"),
                                Bool("INT_BED_RAILS", "Pasang pagar pengaman tempat tidur (bed rails) di kedua sisi"),
                                Bool("INT_YELLOW_SIGN", "Pasang penanda visual risiko jatuh (segitiga kuning pada ranjang/pintu)"),
                                Bool("INT_YELLOW_WRIST", "Pasang GELANG KUNING Risiko Jatuh pada pergelangan tangan (Wajib Risiko Tinggi)"),
                                Bool("INT_FAMILY_EDU", "Edukasi pencegahan jatuh kepada pasien dan keluarga/penunggu"),
                                Bool("INT_ASSIST_AMB", "Bantu dan dampingi saat mobilisasi dan ke toilet"),
                                Bool("INT_MONITOR_2H", "Pantau kondisi pasien secara berkala tiap 2 jam (Wajib Risiko Tinggi)"),
                                Text("INT_NOTE", "Catatan Tindakan Tambahan")
                            }
                        }
                    },
                    Scoring = new() { Method = "sum" },
                    Bands =
                    {
                        Band("LOW", "Rendah", 0, 25, false, "LowRisk"),
                        Band("MEDIUM", "Sedang", 25, 45, false, "MediumRisk"),
                        Band("HIGH", "Tinggi", 45, null, true, "HighRisk")
                    },
                    ReviewFlags =
                    {
                        "Pita skor diselaraskan: Rendah [0, 25), Sedang [25, 45), Tinggi [45, null) sesuai standar KARS/SKP 6.",
                        PenandaUsia
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000003"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000003"),
                "FALL_RISK_ELDERLY", "Risiko Jatuh Lansia — Ontario Modified Stratify Sydney (draft)", ClinicalInstrumentKind.FallRiskScale, 720, null,
                "Penilaian risiko jatuh pasien geriatri/lansia (≥ 60 tahun) menggunakan skala baku Ontario Modified Stratify - Sydney Scoring.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "SYDNEY",
                            Label = "Penilaian Risiko Jatuh Geriatri (Ontario Modified Stratify - Sydney Scoring)",
                            Items =
                            {
                                Single("SYD_HISTORY", "Riwayat Jatuh",
                                    ("NO", "Tidak ada riwayat jatuh", 0),
                                    ("YES", "Ada riwayat jatuh saat masuk atau dalam 1 bulan terakhir", 6)),
                                Single("SYD_MENTAL", "Status Mental / Kognitif",
                                    ("ORIENTED", "Sadar penuh / orientasi baik", 0),
                                    ("CONFUSED", "Agitasi, bingung, disorientasi, atau demensia", 14)),
                                Single("SYD_VISION", "Penglihatan (Vision)",
                                    ("NORMAL", "Penglihatan normal / tanpa keluhan", 0),
                                    ("IMPAIRED", "Gangguan penglihatan / memakai kacamata / katarak", 1)),
                                Single("SYD_TOILETING", "Kebiasaan Berkemih / Urgensi",
                                    ("NORMAL", "Pola berkemih normal / teratur", 0),
                                    ("URGENT_INCONTINENCE", "Sering berkemih / tergesa-gesa / inkontinensia urin", 2)),
                                Single("SYD_MOBILITY", "Transfer & Mobilitas (Tempat Tidur ke Kursi)",
                                    ("INDEPENDENT", "Mandiri (bisa bangkit & berjalan stabil tanpa bantuan)", 0),
                                    ("ASSISTANCE", "Membutuhkan bantuan 1 orang / walker / tongkat", 3),
                                    ("DEPENDENT", "Tergantung penuh / butuh bantuan 2 orang", 3))
                            }
                        },
                        new()
                        {
                            Code = "INTERVENSI",
                            Label = "Checklist Intervensi Pencegahan Jatuh (SOP Rumah Sakit)",
                            Items =
                            {
                                Bool("INT_ORIENT_ROOM", "Orientasikan ruangan dan letak bel panggil darurat"),
                                Bool("INT_BED_LOCKED", "Posisi tempat tidur terendah dan roda terkunci aman"),
                                Bool("INT_BED_RAILS", "Pasang pagar pengaman tempat tidur (bed rails) di kedua sisi"),
                                Bool("INT_YELLOW_SIGN", "Pasang penanda visual risiko jatuh (segitiga kuning pada ranjang/pintu)"),
                                Bool("INT_YELLOW_WRIST", "Pasang GELANG KUNING Risiko Jatuh pada pergelangan tangan (Wajib Risiko Tinggi)"),
                                Bool("INT_FAMILY_EDU", "Edukasi pencegahan jatuh kepada pasien dan keluarga/penunggu"),
                                Bool("INT_ASSIST_AMB", "Bantu dan dampingi saat mobilisasi dan ke toilet"),
                                Bool("INT_MONITOR_2H", "Pantau kondisi pasien secara berkala tiap 2 jam (Wajib Risiko Tinggi)"),
                                Text("INT_NOTE", "Catatan Tindakan Tambahan")
                            }
                        }
                    },
                    Scoring = new() { Method = "sum" },
                    Bands =
                    {
                        Band("LOW", "Rendah", 0, 6, false, "LowRisk"),
                        Band("MEDIUM", "Sedang", 6, 17, false, "MediumRisk"),
                        Band("HIGH", "Tinggi", 17, null, true, "HighRisk")
                    },
                    ReviewFlags =
                    {
                        "Pita skor diselaraskan: Rendah [0, 6), Sedang [6, 17), Tinggi [17, null) menutup lubang skor V1.",
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
                        new()
                        {
                            Code = "SUMBER_DATA", Label = "Sumber Data Pasien",
                            Items =
                            {
                                Single("SD_SOURCE", "Sumber data", ("PATIENT", "Pasien", null), ("OTHER", "Orang lain", null)),
                                Text("SD_OTHER_NAME", "Nama orang lain / pemberi informasi"),
                                Single("SD_RELATION", "Hubungan keluarga", ("SUAMI", "Suami", null), ("ISTRI", "Istri", null), ("ORANG_TUA", "Orang tua", null), ("ANAK", "Anak", null), ("KAKAK", "Kakak", null), ("ADIK", "Adik", null), ("OTHER", "Lainnya", null)),
                                Text("SD_RELATION_OTHER", "Hubungan lainnya"),
                                Text("SD_BELIEFS", "Nilai kepercayaan / budaya / spiritual"),
                                Multi("SD_PSYCHOLOGY", "Kondisi psikologis", ("TENANG", "Tenang"), ("CEMAS", "Cemas"), ("TAKUT", "Takut"), ("MARAH", "Marah"), ("SEDIH", "Sedih"), ("BUNUH_DIRI", "Kecenderungan bunuh diri"), ("OTHER", "Lain-lain")),
                                Single("SD_FAMILY_RELATION", "Hubungan antar anggota keluarga", ("BAIK", "Baik / Harmonis", null), ("TIDAK_BAIK", "Tidak baik / Renggang", null)),
                                Multi("SD_RESIDENCE", "Tempat tinggal", ("RUMAH_PRIBADI", "Rumah pribadi"), ("KONTRAK", "Kontrak / Sewa"), ("RUMAH_KELUARGA", "Bersama keluarga"), ("PANTI_JOMPO", "Panti jompo")),
                                Multi("SD_FUNCTIONAL_DIS", "Gangguan fungsional", ("BUTA", "Penglihatan / Buta"), ("TULI", "Pendengaran / Tuli"), ("DAYA_INGAT", "Daya ingat"), ("LEMAH_GERAK", "Kelemahan anggota gerak")),
                                Text("SD_RELEVANT_NOTE", "Catatan relevan (riwayat jatuh di rumah, kebiasaan)", "PsychosocialNote")
                            }
                        },
                        new()
                        {
                            Code = "KONDISI_UMUM", Label = "Kondisi Umum",
                            Items =
                            {
                                Text("KU_CHIEF_COMPLAINT", "Keluhan utama", "ChiefComplaint"),
                                Text("KU_ILLNESS_HISTORY", "Riwayat penyakit sekarang", "CurrentIllnessHistory"),
                                Text("KU_MEDICATION_HISTORY", "Riwayat obat", "MedicationHistory"),
                                Single("KU_CONSCIOUSNESS", "Kesadaran", "ConsciousnessStatus", ("ComposMentis", "Compos Mentis", null), ("Apatis", "Apatis", null), ("Somnolen", "Somnolen", null), ("Sopor", "Sopor", null), ("Coma", "Koma", null)),
                                Bool("KU_ALLERGY", "Ada riwayat alergi", "HasAllergy"),
                                Text("KU_ALLERGY_NOTE", "Catatan rincian alergi (obat, makanan, udara)", "AllergyNote")
                            }
                        },
                        new()
                        {
                            Code = "PERNAPASAN", Label = "Pernapasan",
                            Items =
                            {
                                Bool("RESP_DIFFICULTY", "Kesulitan bernapas"),
                                Bool("RESP_O2_USAGE", "Memakai terapi oksigen", "IsUsingOxygen"),
                                Number("RESP_O2_FLOW", "Aliran oksigen (Liter/Menit)", "OxygenFlowRate"),
                                Single("RESP_O2_DEVICE", "Jenis alat bantu oksigen", "OxygenSupportType", ("NasalCannula", "Nasal kanul", null), ("SimpleMask", "Simple mask", null), ("NonRebreathingMask", "Non-rebreathing mask", null), ("VenturiMask", "Venturi mask", null), ("Other", "Lainnya", null)),
                                Bool("RESP_COUGH", "Batuk produktif"),
                                Single("RESP_PATTERN", "Pola pernapasan", ("REGULAR", "Regular", null), ("IRREGULAR", "Irregular", null), ("TACHYPNEA", "Takipnea", null), ("BRADYPNEA", "Bradipnea", null), ("KUSSMAUL", "Kussmaul", null), ("CHEYNE_STOKES", "Cheyne-Stokes", null), ("OTHER", "Lainnya", null)),
                                Multi("RESP_SYMPTOMS", "Gejala / keluhan pernapasan", ("DYSPNEA", "Dyspnea"), ("ORTHOPNEA", "Orthopnea"), ("CYANOSIS", "Sianosis"), ("WHEEZING", "Wheezing"), ("STRIDOR", "Stridor"), ("NONE", "Tidak ada")),
                                Text("RESP_NOTE", "Catatan pernapasan")
                            }
                        },
                        new()
                        {
                            Code = "INTEGRITAS_KULIT", Label = "Integritas Kulit",
                            Items =
                            {
                                Bool("SKIN_IMPAIRED", "Integritas kulit terganggu"),
                                Number("SKIN_BRADEN_SCORE", "Skor skala Braden dekubitus"),
                                Multi("SKIN_CONDITION", "Kondisi kulit pasien", ("NORMAL", "Normal / Utuh"), ("RASH", "Rash (Ruam)"), ("SCAR", "Parut (Jaringan Parut)"), ("BRUISE", "Memar (Lebam)"), ("CYANOTIC", "Sianotik (Kebiruan)"), ("SWEATING", "Berkeringat banyak / Basah"), ("DECUBITUS", "Luka tekan / Dekubitus")),
                                Single("SKIN_DECUBITUS_STG", "Stadium luka tekan dekubitus", ("NONE", "Tidak ada", null), ("STAGE_1", "Stadium 1 (Eritema non-blanchable)", null), ("STAGE_2", "Stadium 2 (Hilang sebagian lapisan kulit)", null), ("STAGE_3", "Stadium 3 (Hilang seluruh lapisan kulit)", null), ("STAGE_4", "Stadium 4 (Hilang jaringan hingga otot/tulang)", null), ("UNSTAGEABLE", "Unstageable / Tidak dapat ditentukan", null)),
                                Text("SKIN_NOTE", "Lokasi luka dan catatan integritas kulit")
                            }
                        },
                        new()
                        {
                            Code = "SKRINING_NUTRISI", Label = "Skrining Nutrisi",
                            Items =
                            {
                                Single("NUT_APPETITE", "Nafsu makan", "AppetiteStatus", ("Normal", "Normal", null), ("Decreased", "Menurun", null), ("Increased", "Meningkat", null), ("Poor", "Sangat buruk", null)),
                                Bool("NUT_NAUSEA", "Mual", "HasNausea"),
                                Bool("NUT_VOMITING", "Muntah", "HasVomiting"),
                                Single("NUT_MST_WT_LOSS", "Penurunan berat badan 3-6 bulan terakhir (MST Butir 1)", ("NO", "Tidak ada penurunan BB (Skor 0)", 0), ("UNSURE", "Ragu-ragu / Tidak yakin (Skor 2)", 2), ("KG_1_5", "Turun 1 - 5 kg (Skor 1)", 1), ("KG_6_10", "Turun 6 - 10 kg (Skor 2)", 2), ("KG_11_15", "Turun 11 - 15 kg (Skor 3)", 3), ("KG_OVER_15", "Turun > 15 kg (Skor 4)", 4)),
                                Single("NUT_MST_INTAKE", "Penurunan asupan makan 1 minggu terakhir (MST Butir 2)", ("NO", "Tidak (Skor 0)", 0), ("YES", "Ya (Skor 1)", 1)),
                                Bool("NUT_MST_SEVERE", "Pasien menderita penyakit berat / kritis (ICU/Keganasan/Stroke)"),
                                Multi("NUT_METABOLIC", "Gangguan metabolisme / komorbid gizi", ("DM", "Diabetes Melitus"), ("HT", "Hipertensi"), ("DISLIPIDEMIA", "Dislipidemia"), ("CKD", "Penyakit Ginjal Kronis"), ("OBESITAS", "Obesitas"), ("MALNUTRISI", "Malnutrisi")),
                                Single("NUT_RISK", "Risiko gizi", "NutritionRiskStatus", ("NoRisk", "Tidak berisiko", null), ("LowRisk", "Risiko rendah", null), ("MediumRisk", "Risiko sedang", null), ("HighRisk", "Risiko tinggi", null)),
                                Number("NUT_RISK_SCORE", "Total skor skrining gizi (MST)", "NutritionRiskScore")
                            }
                        },
                        new()
                        {
                            Code = "ELIMINASI", Label = "Eliminasi",
                            Items =
                            {
                                Bool("ELIM_URINE_PROB", "Ada masalah perkemihan (BAK)"),
                                Multi("ELIM_URINE_ISSUES", "Jenis masalah BAK", ("STRIKTUR", "Striktur Uretra"), ("RETENSI", "Retensi Urin"), ("INKONTINENSIA", "Inkontinensia Urin"), ("DIALISIS", "Dialisis"), ("DISURIA", "Disuria / Nyeri BAK")),
                                Text("ELIM_URINE_COLOR", "Warna urin / BAK"),
                                Bool("ELIM_CATHETER", "Terpasang kateter urin"),
                                Single("ELIM_CATHETER_TYPE", "Jenis kateter urin", ("FOLEY", "Foley Catheter", null), ("SILICONE", "Silicone 100%", null), ("CONDOM", "Condom Catheter", null), ("SUPRAPUBIC", "Suprapubik", null), ("OTHER", "Lainnya", null)),
                                Text("ELIM_CATHETER_SIZE", "Ukuran kateter (contoh: 16 Fr, 18 Fr)"),
                                Text("ELIM_CATHETER_DATE", "Tanggal pemasangan kateter (YYYY-MM-DD)"),
                                Bool("ELIM_DEFEC_PROB", "Ada masalah defekasi (BAB)"),
                                Multi("ELIM_DEFEC_ISSUES", "Jenis masalah BAB", ("STOMA", "Stoma / Kolostomi"), ("ATRESIA_ANI", "Atresia Ani"), ("KONSTIPASI", "Konstipasi / Sembelit"), ("INKONTINENSIA_ALVI", "Inkontinensia Alvi"), ("DIARE", "Diare"), ("MELENA", "Melena / Feses Berdarah")),
                                Text("ELIM_NOTE", "Catatan eliminasi")
                            }
                        },
                        new()
                        {
                            Code = "KETERGANTUNGAN", Label = "Ketergantungan",
                            Items =
                            {
                                Single("DEP_MOBILITY", "Mobilisasi", ("MANDIRI", "Mandiri", null), ("DIBANTU", "Dibantu sebagian", null), ("TERGANTUNG_PENUH", "Tergantung penuh", null)),
                                Single("DEP_HYGIENE", "Kebersihan diri / Personal hygiene", ("MANDIRI", "Mandiri", null), ("DIBANTU", "Dibantu sebagian", null), ("TERGANTUNG_PENUH", "Tergantung penuh", null)),
                                Single("DEP_TOILETING", "Toileting (BAB & BAK)", ("MANDIRI", "Mandiri", null), ("DIBANTU", "Dibantu sebagian", null), ("TERGANTUNG_PENUH", "Tergantung penuh", null)),
                                Single("DEP_DRESSING", "Berpakaian", ("MANDIRI", "Mandiri", null), ("DIBANTU", "Dibantu sebagian", null), ("TERGANTUNG_PENUH", "Tergantung penuh", null)),
                                Single("DEP_FEEDING", "Makan dan minum", ("MANDIRI", "Mandiri", null), ("DIBANTU", "Dibantu sebagian", null), ("TERGANTUNG_PENUH", "Tergantung penuh", null)),
                                Multi("DEP_MOBILITY_AID", "Alat bantu aktivitas", ("WHEELCHAIR", "Kursi roda"), ("CANE", "Tongkat"), ("WALKER", "Walker"), ("PENYANGGA", "Penyangga tubuh"), ("GIGI_PALSU", "Gigi palsu"), ("KACAMATA", "Kacamata"), ("PENDENGARAN", "Alat bantu dengar"), ("BED_REST", "Tirah baring total")),
                                Bool("DEP_ALERT_DPJP", "Notifikasi lapor dokter DPJP (aktif jika >= 5 aktivitas tergantung penuh)"),
                                Text("DEP_NOTE", "Catatan ketergantungan")
                            }
                        },
                        new()
                        {
                            Code = "STATUS_FUNGSIONAL", Label = "Status Fungsional",
                            Items =
                            {
                                Single("FUNC_BARTHEL_BOWEL", "1. Mengontrol BAB (Defekasi)", ("KONTINIUM", "Terkontrol / Mandiri (Skor 2)", 2), ("KADANG", "Kadang inkontinensia / Butuh bantuan (Skor 1)", 1), ("INKONTINEN", "Inkontinensia / Tergantung (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_BLADD", "2. Mengontrol BAK (Miksi)", ("KONTINIUM", "Terkontrol / Mandiri (Skor 2)", 2), ("KADANG", "Kadang inkontinensia / Butuh bantuan (Skor 1)", 1), ("INKONTINEN", "Inkontinensia / Pakai kateter (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_GROOM", "3. Perawatan diri (Cuci muka, sisir rambut, sikat gigi)", ("MANDIRI", "Mandiri (Skor 1)", 1), ("DIBANTU", "Butuh pertolongan orang lain (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_TOIL", "4. Penggunaan toilet (Pergi, lepas celana, siram, pakai celana)", ("MANDIRI", "Mandiri (Skor 2)", 2), ("DIBANTU", "Butuh pertolongan sebagian (Skor 1)", 1), ("TERGANTUNG", "Tergantung penuh (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_FEED", "5. Makan", ("MANDIRI", "Mandiri (Skor 2)", 2), ("DIBANTU", "Butuh pertolongan memotong makanan (Skor 1)", 1), ("TERGANTUNG", "Tergantung penuh / Lewat NGT (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_TRANS", "6. Transfer (Pindah dari tempat tidur ke kursi & sebaliknya)", ("MANDIRI", "Mandiri (Skor 3)", 3), ("BANTUAN_MINIMAL", "Bantuan minimal 1 orang (Skor 2)", 2), ("DUDUK", "Bisa duduk dengan bantuan fisik (Skor 1)", 1), ("TERGANTUNG", "Tergantung penuh / Tidak seimbang (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_MOBIL", "7. Mobilitas (Berjalan di permukaan datar)", ("MANDIRI", "Mandiri > 50 meter (Skor 3)", 3), ("DIBANTU", "Berjalan dengan bantuan 1 orang (Skor 2)", 2), ("KURSI_RODA", "Berjalan dengan kursi roda (Skor 1)", 1), ("IMOBIL", "Imobil / Tirah baring (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_DRESS", "8. Berpakaian", ("MANDIRI", "Mandiri memakai baju & sepatu (Skor 2)", 2), ("DIBANTU", "Sebagian dibantu (Skor 1)", 1), ("TERGANTUNG", "Tergantung penuh (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_STAIR", "9. Naik turun tangga", ("MANDIRI", "Mandiri (Skor 2)", 2), ("DIBANTU", "Butuh bantuan / Pengawasan (Skor 1)", 1), ("TIDAK_MAMPU", "Tidak mampu (Skor 0)", 0)),
                                Single("FUNC_BARTHEL_BATH", "10. Mandi", ("MANDIRI", "Mandiri (Skor 1)", 1), ("DIBANTU", "Tergantung / Dibantu (Skor 0)", 0)),
                                Single("FUNC_STATUS", "Status fungsional", "FunctionalStatus", ("Independent", "Mandiri", null), ("NeedPartialAssistance", "Butuh bantuan sebagian", null), ("FullyDependent", "Tergantung penuh", null)),
                                Text("FUNC_NOTE", "Catatan status fungsional", "FunctionalNote")
                            }
                        }
                    },
                    ReviewFlags =
                    {
                        "Susunan delapan bagian mengikuti RWI-DEC-141 dan diselaraskan penuh dengan butir klinis V1 (RLN3-CAP-05).",
                        "Isian wajib mengikuti kebijakan klinis; requiredItemCodes dikosongkan untuk fleksibilitas perawat bangsal."
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000005"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000005"),
                "PAIN_MONITORING", "Monitoring Nyeri (draft)", ClinicalInstrumentKind.PainScale, null, null,
                "Formulir Monitoring Nyeri terpadu mencakup derajat nyeri (NRS/Wong-Baker/CPOT/FLACC), skor sedasi POSS, karakteristik PQRST, intervensi farmakologi & non-farmakologi, serta interval kajian ulang otomatis.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "NYERI_SKALA",
                            Label = "Derajat & Skala Nyeri",
                            Items =
                            {
                                Single("PAIN_STATE", "Status evaluasi nyeri", "PainAssessmentState",
                                    ("NoPain", "Tidak nyeri", null),
                                    ("HasPain", "Ada nyeri", null),
                                    ("UnableToAssess", "Tidak dapat dinilai", null)),
                                Single("PAIN_TOOL", "Metode / alat ukur klinis yang digunakan",
                                    ("NRS", "Numeric Rating Scale (NRS) — Pasien dewasa sadar & kooperatif", null),
                                    ("WONG_BAKER", "Wong-Baker FACES Pain Scale — Pasien anak (> 3 tahun) & geriatri", null),
                                    ("CPOT", "Critical-Care Pain Observation Tool (CPOT) — Pasien ICU / koma / ventilator", null),
                                    ("FLACC", "FLACC Behavioral Scale — Bayi / anak (< 3 tahun)", null)),
                                Number("PAIN_SCALE", "Skala intensitas nyeri (0–10)", "PainScale"),
                                Single("PAIN_SEDATION", "Skor Sedasi POSS (Pasero Opioid-Induced Sedation Scale)",
                                    ("POSS_0", "0: Tidur, mudah dibangunkan", null),
                                    ("POSS_1", "1: Sadar penuh dan waspada", null),
                                    ("POSS_2", "2: Mengantuk ringan, mudah dibangunkan", null),
                                    ("POSS_3", "3: Sering mengantuk, tertidur saat diajak bicara (Waspada overdosis/depresi napas)", null),
                                    ("POSS_4", "4: Somnolen, sulit atau tidak dapat dibangunkan (Bahaya depresi napas)", null))
                            }
                        },
                        new()
                        {
                            Code = "NYERI_PQRST",
                            Label = "Karakteristik Klinis Nyeri (PQRST)",
                            Items =
                            {
                                Text("PAIN_LOCATION", "Lokasi anatomis nyeri", "PainLocation"),
                                Single("PAIN_RADIATION", "Penjalaran nyeri",
                                    ("NO", "Tidak menjalar (terlokalisir)", null),
                                    ("YES", "Ya, menjalar ke bagian tubuh lain", null)),
                                Single("PAIN_QUALITY_SEL", "Kualitas / karakter sensasi nyeri", "PainQuality",
                                    ("TERTUSUK", "Tertusuk-tusuk / seperti jarum", null),
                                    ("BERDENYUT", "Berdenyut-denyut", null),
                                    ("TERBAKAR", "Panas terbakar", null),
                                    ("TUMPUL", "Tumpul / pegal / linu", null),
                                    ("MELILIT", "Kram / melilit / kolik", null),
                                    ("MENUSUK", "Tajam menusuk", null),
                                    ("TERIRIS", "Teriris / sayatan / perih", null),
                                    ("OTHER", "Lainnya", null)),
                                Single("PAIN_TRIGGER_SEL", "Faktor pencetus / provokasi", "PainTrigger",
                                    ("GERAK", "Saat bergerak / mobilisasi", null),
                                    ("BATUK", "Saat batuk / nafas dalam", null),
                                    ("TEKANAN", "Sentuhan / tekanan fisik", null),
                                    ("SPONTAN", "Spontan / terus-menerus tanpa pemicu", null),
                                    ("PASCA_BEDAH", "Luka operasi / pasca tindakan invasif", null),
                                    ("OTHER", "Lainnya", null)),
                                Single("PAIN_FREQUENCY", "Frekuensi & durasi nyeri", "PainFrequency",
                                    ("HILANG_TIMBUL", "Hilang timbul (intermiten)", null),
                                    ("TERUS_MENERUS", "Terus-menerus menetap (konstan)", null),
                                    ("MENDADAK", "Mendadak tajam (akut paroksismal)", null))
                            }
                        },
                        new()
                        {
                            Code = "NYERI_INTERVENSI",
                            Label = "Rencana & Intervensi Manajemen Nyeri",
                            Items =
                            {
                                Multi("PAIN_NON_PHARM", "Intervensi non-farmakologi",
                                    ("RELAKSASI", "Relaksasi nafas dalam"),
                                    ("KOMPRES_HANGAT", "Kompres hangat"),
                                    ("KOMPRES_DINGIN", "Kompres dingin"),
                                    ("POSISI", "Pengaturan posisi tidur / semifowler"),
                                    ("MASASE", "Masase / pijat lembut"),
                                    ("MUSIK_DISTRAKSI", "Terapi musik / distraksi verbal"),
                                    ("TENS", "Stimulasi saraf transkutan (TENS)"),
                                    ("EDUKASI", "Edukasi manajemen nyeri kepada pasien/keluarga")),
                                Bool("PAIN_PHARM_GIVEN", "Pemberian terapi analgetik farmakologi"),
                                Text("PAIN_MED_NAME", "Nama obat analgetik yang diberikan"),
                                Text("PAIN_MED_DOSE", "Dosis & takaran obat analgetik"),
                                Single("PAIN_MED_ROUTE", "Rute pemberian obat analgetik",
                                    ("ORAL", "Oral (per oral)", null),
                                    ("IV", "Intravena (IV)", null),
                                    ("IM", "Intramuskular (IM)", null),
                                    ("SC", "Subkutan (SC)", null),
                                    ("TOPIKAL", "Topikal / transdermal", null),
                                    ("REKTAL", "Rektal / supositoria", null),
                                    ("INHALASI", "Inhalasi", null)),
                                Text("PAIN_INTERVENTION", "Rangkuman tindakan intervensi keperawatan", "PainManagement"),
                                Text("PAIN_NOTE", "Catatan respon klinis pasien pasca intervensi", "PainNote")
                            }
                        }
                    },
                    RequiredItemCodes = { "PAIN_STATE" },
                    ReassessmentMinutes = 60,
                    ReviewFlags =
                    {
                        "Interval kajian ulang nyeri baku ditetapkan 60 menit sesuai standar KARS/SOP RS."
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000006"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000006"),
                "EDUCATION_ASSESSMENT", "Assesment Edukasi", ClinicalInstrumentKind.EducationAssessmentForm, null, null,
                "Formulir pengkajian kesiapan edukasi, pelaksanaan metode/media, dan evaluasi pemahaman pasien (standar KARS HPK/KE dan V1 RLN3-CAP-08).",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "EDU_KESIAPAN",
                            Label = "Pengkajian Kesiapan & Kemampuan Belajar Pasien",
                            Items =
                            {
                                Single("EDU_LANG", "Bahasa Sehari-hari",
                                    ("ID", "Indonesia", null),
                                    ("DAERAH", "Bahasa Daerah", null),
                                    ("ASING", "Bahasa Asing / Inggris", null),
                                    ("OTHER", "Lainnya", null)),
                                Bool("EDU_TRANSLATOR", "Kebutuhan Penerjemah Bahasa / Isyarat"),
                                Bool("EDU_LITERACY", "Kemampuan Membaca dan Menulis (Literasi)"),
                                Single("EDU_EDUCATION", "Tingkat Pendidikan Formal",
                                    ("TIDAK_SEKOLAH", "Tidak Sekolah", null),
                                    ("SD", "SD", null),
                                    ("SMP", "SMP", null),
                                    ("SMA", "SMA / Sederajat", null),
                                    ("DIPLOMA", "Diploma (D3/D4)", null),
                                    ("SARJANA", "Sarjana (S1/S2/S3)", null)),
                                Single("EDU_LEARNING_STYLE", "Gaya Belajar yang Disukai",
                                    ("VISUAL", "Visual (Melihat Gambar / Video)", null),
                                    ("AUDITORI", "Auditori (Mendengarkan Penjelasan)", null),
                                    ("KINESTETIK", "Kinestetik (Demonstrasi / Praktik)", null),
                                    ("BACA_TULIS", "Membaca / Menulis", null),
                                    ("KOMBINASI", "Kombinasi Multimedia", null)),
                                Text("EDU_BELIEFS", "Nilai Kepercayaan / Budaya / Spiritual"),
                                Multi("EDU_BARRIERS", "Hambatan Proses Belajar",
                                    ("NONE", "Tidak Ada Hambatan"),
                                    ("BAHASA", "Kendala Bahasa"),
                                    ("BUDAYA", "Faktor Budaya / Nilai"),
                                    ("EMOSIONAL", "Emosional / Sangat Cemas / Depresi"),
                                    ("FISIK", "Fisik Lemah / Nyeri Hebat / Gangguan Penglihatan / Pendengaran"),
                                    ("KOGNITIF", "Kognitif / Gangguan Memori / Daya Ingat Menurun")),
                                Bool("EDU_WILLINGNESS", "Pasien / Keluarga Bersedia Menerima Edukasi"),
                                Multi("EDU_NEEDS", "Kebutuhan Topik Edukasi Pasien",
                                    ("PENYAKIT", "Diagnosis & Proses Penyakit"),
                                    ("OBAT", "Penggunaan Obat & Efek Samping"),
                                    ("PERAWATAN", "Perawatan Luka & Mandiri"),
                                    ("NUTRISI", "Diet Gizi & Nutrisi"),
                                    ("REHABILITASI", "Rehabilitasi & Mobilisasi Fisik"),
                                    ("MANAJEMEN_NYERI", "Manajemen & Pengendalian Nyeri"),
                                    ("PENCEGAHAN_INFEKSI", "Cuci Tangan & Pencegahan Infeksi (PPI)"),
                                    ("PENCEGAHAN_JATUH", "Pencegahan Risiko Pasien Jatuh"),
                                    ("PENGGUNAAN_ALAT", "Penggunaan Alat Medis"),
                                    ("OTHER", "Topik Lainnya")),
                                Text("EDU_NEEDS_OTHER", "Kebutuhan Edukasi Spesifik Lainnya")
                            }
                        },
                        new()
                        {
                            Code = "EDU_PELAKSANAAN",
                            Label = "Pelaksanaan & Metode Pemberian Edukasi",
                            Items =
                            {
                                Multi("EDU_RECIPIENT", "Penerima Edukasi",
                                    ("PATIENT", "Pasien"),
                                    ("FAMILY", "Keluarga / Kerabat"),
                                    ("CAREGIVER", "Caregiver / Pendamping Khusus")),
                                Text("EDU_FAMILY_NAME", "Nama Keluarga / Wali Pendamping"),
                                Single("EDU_METHOD", "Metode Penyampaian Edukasi",
                                    ("TANYA_JAWAB", "Wawancara & Tanya Jawab", null),
                                    ("CERAMAH", "Ceramah & Diskusi Dua Arah", null),
                                    ("DEMONSTRASI", "Demonstrasi & Simulasi Praktik", null),
                                    ("KOMBINASI", "Kombinasi Ceramah dan Praktik", null)),
                                Multi("EDU_MEDIA", "Media / Sarana Edukasi",
                                    ("LEAFLET", "Buku / Leaflet / Lembar Informasi"),
                                    ("AUDIO_VISUAL", "Video / Audio Visual"),
                                    ("ALAT_PERAGA", "Alat Peraga / Lembar Balik / Phantom"),
                                    ("LISAN", "Penjelasan Lisan Langsung")),
                                Number("EDU_DURATION", "Durasi Edukasi (Menit)"),
                                Text("EDU_MATERIAL", "Rincian Materi Pokok yang Disampaikan")
                            }
                        },
                        new()
                        {
                            Code = "EDU_EVALUASI",
                            Label = "Evaluasi Pemahaman & Verifikasi KARS (Teach-Back)",
                            Items =
                            {
                                Single("EDU_UNDERSTANDING", "Tingkat Pemahaman Penerima Edukasi",
                                    ("BAIK", "Baik (Mengerti Penuh & Mampu Menjelaskan Kembali / Teach-Back)", null),
                                    ("CUKUP", "Cukup (Memahami Sebagian Materi, Butuh Penguatan)", null),
                                    ("KURANG", "Kurang (Belum Memahami Materi, Wajib Re-Edukasi)", null)),
                                Single("EDU_RESULT", "Tindak Lanjut Hasil Evaluasi",
                                    ("MENGERTI", "Sudah Mengerti (Edukasi Selesai)", null),
                                    ("RE_DEMONSTRASI", "Mampu Re-Demonstrasi Praktik Mandiri", null),
                                    ("RE_EDUKASI", "Perlu Jadwal Re-Edukasi Lanjutan", null)),
                                Text("EDU_NOTE", "Catatan Edukasi Terpadu", "EducationNote")
                            }
                        }
                    },
                    RequiredItemCodes = { "EDU_LANG", "EDU_UNDERSTANDING" },
                    ReviewFlags =
                    {
                        "Instrumen Asesmen Edukasi selaras dengan standar KARS (Bab HPK & KE) dan mengadopsi V1 RLN3-CAP-08."
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000007"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000007"),
                "DISCHARGE_PLANNING", "Perencanaan Pulang", ClinicalInstrumentKind.DischargePlanningForm, null, null,
                "Formulir perencanaan pemulangan (Discharge Planning) 8 kelompok berstandar KARS ARK 3 / ARK 4 dan V1 rencana pulang.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        new()
                        {
                            Code = "DP_KRITERIA",
                            Label = "1. Skrining Kriteria Pemulangan Pasien",
                            Items =
                            {
                                Bool("DP_KRIT_USIA", "Usia lebih dari 65 tahun"),
                                Bool("DP_KRIT_SUICIDE", "Riwayat percobaan bunuh diri / psikiatri"),
                                Bool("DP_KRIT_CRIME", "Korban kekerasan / kasus kriminal / penelantaran"),
                                Bool("DP_KRIT_MOBILITY", "Keterbatasan mobilitas fisik"),
                                Bool("DP_KRIT_CONTINUED_CARE", "Perawatan dan pengobatan lanjutan kompleks"),
                                Bool("DP_KRIT_ADL", "Memerlukan bantuan untuk aktivitas sehari-hari (ADL)")
                            }
                        },
                        new()
                        {
                            Code = "DP_CAREGIVER",
                            Label = "2. Caregiver & Kesiapan Perawatan di Rumah",
                            Items =
                            {
                                Bool("DP_LIVING_ALONE", "Pasien tinggal sendiri setelah keluar RS"),
                                Text("DP_CAREGIVER_NAME", "Nama penanggung jawab / caregiver utama di rumah"),
                                Text("DP_CAREGIVER_PHONE", "Nomor telepon / kontak caregiver")
                            }
                        },
                        new()
                        {
                            Code = "DP_HOME_ENV",
                            Label = "3. Lingkungan Fisik Rumah (Faktor Keselamatan)",
                            Items =
                            {
                                Single("DP_BEDROOM_FLOOR", "Letak kamar tidur pasien di rumah",
                                    ("LANTAI_1", "Lantai 1", null),
                                    ("LANTAI_2", "Lantai 2", null),
                                    ("OTHER", "Lainnya", null)),
                                Single("DP_LIGHTING", "Kondisi penerangan di rumah",
                                    ("CUKUP", "Cukup Terang", null),
                                    ("KURANG", "Kurang / Gelap", null)),
                                Single("DP_BATHROOM_DIST", "Jarak kamar tidur ke kamar mandi",
                                    ("DEKAT", "< 5 Meter", null),
                                    ("JAUH", "≥ 5 Meter", null)),
                                Single("DP_TOILET_TYPE", "Jenis WC / jamban di rumah",
                                    ("DUDUK", "WC Duduk", null),
                                    ("JONGKOK", "WC Jongkok", null))
                            }
                        },
                        new()
                        {
                            Code = "DP_EQUIPMENT",
                            Label = "4. Peralatan Medis & Alat Bantu di Rumah",
                            Items =
                            {
                                Bool("DP_MED_EQUIP_USED", "Memerlukan peralatan medis di rumah (kateter, NGT, O2, stoma)"),
                                Text("DP_MED_EQUIP_NOTE", "Rincian peralatan medis yang digunakan"),
                                Bool("DP_MOBILITY_AID", "Memerlukan alat bantu mobilitas (kursi roda, walker, tongkat)"),
                                Text("DP_MOBILITY_AID_NOTE", "Rincian alat bantu yang diperlukan")
                            }
                        },
                        new()
                        {
                            Code = "DP_HOMECARE",
                            Label = "5. Kebutuhan Layanan Home Care / Rawat Lanjut",
                            Items =
                            {
                                Bool("DP_HOMECARE_NEEDED", "Memerlukan bantuan perawatan khusus di rumah (home care)"),
                                Text("DP_HOMECARE_NOTE", "Rincian kebutuhan home care / kunjungan rumah")
                            }
                        },
                        new()
                        {
                            Code = "DP_TRANSPORT",
                            Label = "6. Transportasi Kepulangan Pasien",
                            Items =
                            {
                                Single("DP_TRANSPORT_TYPE", "Moda transportasi kepulangan yang digunakan",
                                    ("PRIBADI", "Kendaraan Pribadi (Mobil / Motor)", null),
                                    ("UMUM", "Transportasi Umum / Taksi", null),
                                    ("AMBULANS_TRANSPORT", "Ambulans Transport (Stabil)", null),
                                    ("AMBULANS_MEDIS", "Ambulans Medis / ICU Berpendamping", null)),
                                Text("DP_TRANSPORT_NOTE", "Catatan khusus transportasi kepulangan")
                            }
                        },
                        new()
                        {
                            Code = "DP_FOLLOWUP",
                            Label = "7. Rencana Kontrol & Edukasi Lanjutan",
                            Items =
                            {
                                Text("DP_FOLLOWUP_PLAN", "Rencana kontrol dokter DPJP / poliklinik"),
                                Text("DP_MED_EDUCATION", "Edukasi obat pulang dan kepatuhan terapi")
                            }
                        },
                        new()
                        {
                            Code = "DP_PLAN_STATUS",
                            Label = "8. Status Rencana & Resume Pemulangan",
                            Items =
                            {
                                Text("DP_PLAN_STATUS_NOTE", "Catatan resume perencanaan pulang perawat", "NurseNote")
                            }
                        }
                    },
                    RequiredItemCodes = { "DP_TRANSPORT_TYPE" },
                    ReviewFlags =
                    {
                        "Instrumen Perencanaan Pulang selaras dengan standar KARS (Bab ARK 3 & ARK 4) dan mengadopsi V1 rencana pulang."
                    }
                });

            yield return new Baseline(
                Guid.Parse("c1a1f000-0107-4a01-9b01-000000000008"), Guid.Parse("c1a1f000-0107-4a01-9b02-000000000008"),
                "CASE_MANAGEMENT_CHECKLIST", "Checklist Evaluasi Awal MPP", ClinicalInstrumentKind.CaseManagementChecklist, null, null,
                "Formulir evaluasi awal Manajer Pelayanan Pasien (MPP) 8 bagian berstandar KARS PAP 2.1 & TKRS serta V1 evaluasi awal.",
                new ClinicalInstrumentDefinition
                {
                    Sections =
                    {
                        TextSection("MPP_SCREENING", "1. Identifikasi / Skrining Pasien"),
                        TextSection("MPP_PROBLEM", "2. Identifikasi Masalah Pasien & Keluarga"),
                        TextSection("MPP_GOAL", "3. Harapan / Sasaran Asuhan Manajer Pelayanan"),
                        TextSection("MPP_PLAN", "4. Perencanaan Pelayanan & Kolaborasi Klinis"),
                        TextSection("MPP_SUPPORT", "5. Dukungan Sosial & Sistem Keluarga"),
                        TextSection("MPP_FINANCIAL", "6. Aspek Finansial & Jaminan Pembiayaan"),
                        TextSection("MPP_LEGAL", "7. Aspek Legal & Etika Pelayanan"),
                        TextSection("MPP_DISCHARGE", "8. Perencanaan Pemulangan (Discharge Planning)")
                    },
                    ReviewFlags =
                    {
                        "Formulir evaluasi awal MPP 8 bagian selaras dengan standar KARS (Bab PAP 2.1) dan mengadopsi V1 evaluasi awal."
                    }
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
