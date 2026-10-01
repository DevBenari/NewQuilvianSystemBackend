using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Seeder Master Data Standar Diagnosis Keperawatan Indonesia (SDKI),
    /// Standar Luaran Keperawatan Indonesia (SLKI), dan Standar Intervensi Keperawatan Indonesia (SIKI) resmi PPNI.
    /// Mengisi 10 diagnosis prioritas ruang rawat inap beserta etiologi, luaran, dan 4 pilar intervensi bawaannya.
    /// </summary>
    public static class MstNursingDiagnosisSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            CancellationToken ct = default)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedAsync(db, Guid.Empty, ct);
        }

        public static async Task SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            // 1. Seed Groups
            var groups = GetInitialGroups(actorUserId, now);
            foreach (var grp in groups)
            {
                var existingGrp = await db.MstNursingDiagnosisGroups
                    .FirstOrDefaultAsync(x => x.GroupCode == grp.GroupCode, ct);

                if (existingGrp == null)
                {
                    db.MstNursingDiagnosisGroups.Add(grp);
                }
            }
            await db.SaveChangesAsync(ct);

            // Reload groups for FK mapping
            var groupDict = await db.MstNursingDiagnosisGroups
                .ToDictionaryAsync(x => x.GroupCode, x => x.Id, ct);

            // 2. Seed Diagnoses with Outcomes, Etiologies, and Interventions
            var diagnoses = GetInitialDiagnoses(actorUserId, now, groupDict);
            foreach (var diag in diagnoses)
            {
                var existingDiag = await db.MstNursingDiagnoses
                    .Include(x => x.Etiologies)
                    .Include(x => x.Outcomes)
                    .Include(x => x.Interventions)
                    .FirstOrDefaultAsync(x => x.Code == diag.Code, ct);

                if (existingDiag == null)
                {
                    db.MstNursingDiagnoses.Add(diag);
                }
            }

            await db.SaveChangesAsync(ct);
        }

        private static List<MstNursingDiagnosisGroup> GetInitialGroups(Guid actorUserId, DateTime now)
        {
            return new List<MstNursingDiagnosisGroup>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    GroupCode = "KAT-FIS",
                    GroupName = "Fisiologis",
                    Description = "Diagnosis keperawatan yang mencakup fungsi respirasi, sirkulasi, nutrisi, eliminasi, dan termoregulasi.",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    GroupCode = "KAT-PSI",
                    GroupName = "Psikologis",
                    Description = "Diagnosis keperawatan yang mencakup aspek emosional, ansietas, konsep diri, dan koping.",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    GroupCode = "KAT-PRL",
                    GroupName = "Perilaku",
                    Description = "Diagnosis keperawatan yang mencakup kebiasaan hidup, kepatuhan terapi, dan pemeliharaan kesehatan.",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    GroupCode = "KAT-REL",
                    GroupName = "Relasional",
                    Description = "Diagnosis keperawatan yang mencakup interaksi sosial, komunikasi, dan hubungan keluarga.",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    GroupCode = "KAT-LING",
                    GroupName = "Lingkungan",
                    Description = "Diagnosis keperawatan yang mencakup keamanan, keselamatan, proteksi infeksi, dan risiko jatuh.",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                }
            };
        }

        private static List<MstNursingDiagnosis> GetInitialDiagnoses(
            Guid actorUserId,
            DateTime now,
            Dictionary<string, Guid> groupDict)
        {
            var list = new List<MstNursingDiagnosis>();

            Guid? GetGroupId(string code) => groupDict.TryGetValue(code, out var id) ? id : null;

            // 1. Nyeri Akut (D.0077)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-FIS"),
                    Code = "D.0077",
                    Name = "Nyeri Akut",
                    SubCategory = "Nyeri dan Kenyamanan",
                    Definition = "Pengalaman sensorik atau emosional yang berkaitan dengan kerusakan jaringan aktual atau fungsional dengan onset mendadak atau lambat dan berintensitas ringan hingga berat yang berlangsung kurang dari 3 bulan.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Agen pencedera fisiologis (mis. inflamasi, iskemia, neoplasma)",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Agen pencedera fisik (mis. abses, amputasi, luka bakar, trauma, prosedur operasi)",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.08066",
                    OutcomeName = "Tingkat Nyeri Menurun",
                    Expectation = "Keluhan nyeri menurun (skala <= 3/10 dalam 24 jam), ekspresi meringis menurun, gelisah menurun, kesulitan tidur menurun.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.08238",
                    InterventionName = "Manajemen Nyeri",
                    ActionDescription = "Identifikasi lokasi, karakteristik, durasi, frekuensi, kualitas, intensitas nyeri, dan skala nyeri secara berkala",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.08238",
                    InterventionName = "Manajemen Nyeri",
                    ActionDescription = "Berikan teknik nonfarmakologis untuk mengurangi rasa nyeri (relaksasi napas dalam, kompres hangat/dingin), atur posisi nyaman semi-Fowler",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.08238",
                    InterventionName = "Manajemen Nyeri",
                    ActionDescription = "Jelaskan penyebab, periode, dan pemicu nyeri, serta ajarkan teknik nonfarmakologis untuk meredakan nyeri",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.08238",
                    InterventionName = "Manajemen Nyeri",
                    ActionDescription = "Kolaborasi pemberian terapi analgetik sesuai advis dokter penanggung jawab pelayanan (DPJP)",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 2. Hipertermia (D.0130)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-FIS"),
                    Code = "D.0130",
                    Name = "Hipertermia",
                    SubCategory = "Termoregulasi",
                    Definition = "Suhu tubuh meningkat di atas rentang normal tubuh.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Proses penyakit (mis. infeksi bakteri, virus, atau parasit)",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Dehidrasi atau terpapar lingkungan panas",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.14134",
                    OutcomeName = "Termoregulasi Membaik",
                    Expectation = "Suhu tubuh dalam rentang normal (36.5 - 37.5 °C) dalam 24 jam, kulit merah menurun, takikardia menurun.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.15506",
                    InterventionName = "Manajemen Hipertermia",
                    ActionDescription = "Monitor suhu tubuh berkala per 2-4 jam, monitor warna dan suhu kulit, pantau tanda-tanda dehidrasi",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.15506",
                    InterventionName = "Manajemen Hipertermia",
                    ActionDescription = "Longgarkan atau lepas pakaian tebal, berikan kompres hangat pada dahi, leher, atau aksila",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.15506",
                    InterventionName = "Manajemen Hipertermia",
                    ActionDescription = "Anjurkan tirah baring dan tingkatkan asupan cairan peroral minimal 2000 ml/hari jika tidak ada kontraindikasi",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.15506",
                    InterventionName = "Manajemen Hipertermia",
                    ActionDescription = "Kolaborasi pemberian cairan dan elektrolit intravena serta obat antipiretik sesuai advis DPJP",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 3. Bersihan Jalan Napas Tidak Efektif (D.0001)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-FIS"),
                    Code = "D.0001",
                    Name = "Bersihan Jalan Napas Tidak Efektif",
                    SubCategory = "Respirasi",
                    Definition = "Ketidakmampuan membersihkan sekret atau obstruksi jalan napas untuk mempertahankan kepatenan jalan napas.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Spasme jalan napas atau hipersekresi jalan napas",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Sekresi yang tertahan atau proses infeksi saluran napas",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.01001",
                    OutcomeName = "Bersihan Jalan Napas Meningkat",
                    Expectation = "Batuk efektif meningkat, produksi sputum berkurang, mengi/ronki menurun, frekuensi napas membaik.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.01006",
                    InterventionName = "Latihan Batuk Efektif",
                    ActionDescription = "Monitor pola napas (frekuensi, kedalaman, usaha napas) dan monitor sputum (jumlah, warna, aroma)",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.01006",
                    InterventionName = "Latihan Batuk Efektif",
                    ActionDescription = "Posisikan pasien semi-Fowler atau Fowler, berikan minum air hangat, lakukan fisioterapi dada jika perlu",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.01006",
                    InterventionName = "Latihan Batuk Efektif",
                    ActionDescription = "Jelaskan tujuan dan prosedur batuk efektif, ajarkan teknik batuk efektif dan napas dalam",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.01006",
                    InterventionName = "Latihan Batuk Efektif",
                    ActionDescription = "Kolaborasi pemberian mukolitik, ekspektoran, atau terapi inhalasi/nebulizer sesuai advis DPJP",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 4. Pola Napas Tidak Efektif (D.0005)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-FIS"),
                    Code = "D.0005",
                    Name = "Pola Napas Tidak Efektif",
                    SubCategory = "Respirasi",
                    Definition = "Inspirasi dan/atau ekspirasi yang tidak memberikan ventilasi adekuat.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Hambatan upaya napas (mis. nyeri saat bernapas, kelemahan otot pernapasan)",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.01004",
                    OutcomeName = "Pola Napas Membaik",
                    Expectation = "Frekuensi pernapasan membaik (16-20 x/menit), kedalaman napas membaik, dispnea menurun, penggunaan otot bantu napas menurun.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.01011",
                    InterventionName = "Manajemen Jalan Napas",
                    ActionDescription = "Monitor frekuensi, irama, kedalaman, dan upaya napas, serta pantau saturasi oksigen (SpO2)",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.01011",
                    InterventionName = "Manajemen Jalan Napas",
                    ActionDescription = "Posisikan semi-Fowler atau Fowler untuk mengoptimalkan ekspansi dinding dada",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.01011",
                    InterventionName = "Manajemen Jalan Napas",
                    ActionDescription = "Ajarkan teknik relaksasi napas pursed-lip dan pembatasan aktivitas berat",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.01011",
                    InterventionName = "Manajemen Jalan Napas",
                    ActionDescription = "Kolaborasi pemberian bantuan terapi oksigenasi (nasal kanul/masker) sesuai advis DPJP",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 5. Risiko Jatuh (D.0143)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-LING"),
                    Code = "D.0143",
                    Name = "Risiko Jatuh",
                    SubCategory = "Keamanan dan Proteksi",
                    Definition = "Berisiko mengalami kerusakan fisik dan gangguan kesehatan akibat jatuh.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Kelemahan fisik, efek agen farmakologis (sedasi, anestesi), riwayat jatuh sebelumnya, usia lanjut",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.14138",
                    OutcomeName = "Tingkat Jatuh Menurun",
                    Expectation = "Tidak terjadi kejadian jatuh selama masa perawatan rawat inap.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.14540",
                    InterventionName = "Pencegahan Jatuh",
                    ActionDescription = "Identifikasi faktor risiko jatuh (skala Morse/Humpty Dumpty) setiap pergantian dinas",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.14540",
                    InterventionName = "Pencegahan Jatuh",
                    ActionDescription = "Pasang gelang kuning risiko jatuh, kunci roda tempat tidur, pasang pengaman tempat tidur (bed rails ganda)",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.14540",
                    InterventionName = "Pencegahan Jatuh",
                    ActionDescription = "Edukasi pasien dan keluarga agar memanggil perawat lewat bel bila membutuhkan bantuan mobilisasi/ke kamar mandi",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.14540",
                    InterventionName = "Pencegahan Jatuh",
                    ActionDescription = "Kolaborasi dengan tim fisioterapi untuk latihan mobilisasi dan adaptasi alat bantu jalan",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 6. Risiko Infeksi (D.0142)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-LING"),
                    Code = "D.0142",
                    Name = "Risiko Infeksi",
                    SubCategory = "Keamanan dan Proteksi",
                    Definition = "Berisiko mengalami peningkatan terserang organisme patogenik.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Prosedur invasif (terpasang jalur infus intravena, kateter urin, luka operasi)",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.14137",
                    OutcomeName = "Tingkat Infeksi Menurun",
                    Expectation = "Bebas tanda infeksi nosokomial (tidak ada demam, kemerahan, pus, atau flebitis pada area insersi).",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.14539",
                    InterventionName = "Pencegahan Infeksi",
                    ActionDescription = "Monitor tanda dan gejala infeksi lokal dan sistemik pada area luka dan tempat insersi infus",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.14539",
                    InterventionName = "Pencegahan Infeksi",
                    ActionDescription = "Terapkan kebersihan tangan (5 momen cuci tangan) dan rawat balutan infus/kateter dengan teknik aseptik sesuai SPO",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.14539",
                    InterventionName = "Pencegahan Infeksi",
                    ActionDescription = "Jelaskan tanda dan gejala infeksi kepada pasien serta ajarkan cara mencuci tangan yang benar",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.14539",
                    InterventionName = "Pencegahan Infeksi",
                    ActionDescription = "Kolaborasi pemberian terapi antimikroba/antibiotik sesuai indikasi DPJP",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 7. Defisit Nutrisi (D.0019)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-FIS"),
                    Code = "D.0019",
                    Name = "Defisit Nutrisi",
                    SubCategory = "Nutrisi dan Cairan",
                    Definition = "Asupan nutrisi tidak cukup untuk memenuhi kebutuhan metabolisme.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Ketidakmampuan menelan atau mencerna makanan, penurunan nafsu makan, mual dan muntah",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.03030",
                    OutcomeName = "Status Nutrisi Membaik",
                    Expectation = "Porsi makanan yang dihabiskan meningkat, nafsu makan membaik, berat badan stabil.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.03119",
                    InterventionName = "Manajemen Nutrisi",
                    ActionDescription = "Identifikasi status nutrisi, alergi makanan, dan monitor asupan makanan harian",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.03119",
                    InterventionName = "Manajemen Nutrisi",
                    ActionDescription = "Lakukan kebersihan mulut sebelum makan, sajikan makanan dalam keadaan hangat dan menarik",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.03119",
                    InterventionName = "Manajemen Nutrisi",
                    ActionDescription = "Anjurkan makan sedikit tapi sering, serta posisikan duduk saat makan",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.03119",
                    InterventionName = "Manajemen Nutrisi",
                    ActionDescription = "Kolaborasi dengan ahli gizi untuk penentuan diet dan kalori yang sesuai",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 8. Intoleransi Aktivitas (D.0056)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-FIS"),
                    Code = "D.0056",
                    Name = "Intoleransi Aktivitas",
                    SubCategory = "Aktivitas dan Istirahat",
                    Definition = "Ketidakcukupan energi untuk melakukan aktivitas sehari-hari.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Ketidakseimbangan antara suplai dan kebutuhan oksigen, tirah baring lama, kelemahan umum",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.05047",
                    OutcomeName = "Toleransi Aktivitas Meningkat",
                    Expectation = "Kemampuan melakukan aktivitas sehari-hari meningkat, keluhan lemas menurun.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.05178",
                    InterventionName = "Manajemen Energi",
                    ActionDescription = "Identifikasi gangguan fungsi tubuh yang mengakibatkan kelelahan dan monitor kelelahan fisik",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.05178",
                    InterventionName = "Manajemen Energi",
                    ActionDescription = "Sediakan lingkungan nyaman dan bantu aktivitas fisik yang belum dapat dilakukan mandiri",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.05178",
                    InterventionName = "Manajemen Energi",
                    ActionDescription = "Anjurkan tirah baring teratur dan lakukan aktivitas fisik secara bertahap",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.05178",
                    InterventionName = "Manajemen Energi",
                    ActionDescription = "Kolaborasi dengan ahli gizi tentang cara meningkatkan asupan energi makanan",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 9. Gangguan Integritas Kulit/Jaringan (D.0129)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-LING"),
                    Code = "D.0129",
                    Name = "Gangguan Integritas Kulit/Jaringan",
                    SubCategory = "Keamanan dan Proteksi",
                    Definition = "Kerusakan kulit (dermis dan/atau epidermis) atau jaringan (membran mukosa, kornea, fasia, otot, tendon, tulang, kartilago, kapsul sendi dan/atau ligamen).",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Penurunan mobilitas/tirah baring lama, kelembapan kulit berlebih, penekanan tonjolan tulang",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.14125",
                    OutcomeName = "Integritas Kulit dan Jaringan Meningkat",
                    Expectation = "Kerusakan jaringan menurun, kemerahan menurun, elastisitas kulit membaik.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.14564",
                    InterventionName = "Perawatan Integritas Kulit",
                    ActionDescription = "Identifikasi penyebab gangguan integritas kulit dan monitor adanya lecet/kemerahan pada area penekanan",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.14564",
                    InterventionName = "Perawatan Integritas Kulit",
                    ActionDescription = "Ubah posisi pasien miring kanan-kiri setiap 2 jam, gunakan kasur anti-dekubitus, pertahankan laken bersih dan kering",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.14564",
                    InterventionName = "Perawatan Integritas Kulit",
                    ActionDescription = "Anjurkan penggunaan pelembab kulit dan anjurkan minum air yang cukup",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.14564",
                    InterventionName = "Perawatan Integritas Kulit",
                    ActionDescription = "Kolaborasi dengan dokter dan tim perawatan luka untuk tindakan debridemen/balutan modern",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            // 10. Hipovolemia (D.0023)
            {
                var diag = new MstNursingDiagnosis
                {
                    Id = Guid.NewGuid(),
                    GroupId = GetGroupId("KAT-FIS"),
                    Code = "D.0023",
                    Name = "Hipovolemia",
                    SubCategory = "Nutrisi dan Cairan",
                    Definition = "Penurunan volume cairan intravaskular, interstisial, dan/atau intraselular.",
                    TerminologySystem = "SDKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                diag.Etiologies.Add(new MstNursingDiagnosisEtiology
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    EtiologyName = "Kehilangan cairan aktif (muntah, diare, perdarahan), kekurangan asupan cairan",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Outcomes.Add(new MstNursingDiagnosisOutcome
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    OutcomeCode = "L.03028",
                    OutcomeName = "Status Cairan Membaik",
                    Expectation = "Turgor kulit membaik, membran mukosa lembap, tekanan darah membaik, haluaran urine adekuat.",
                    TerminologySystem = "SLKI",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 1, // Observasi
                    InterventionCode = "I.03116",
                    InterventionName = "Manajemen Hipovolemia",
                    ActionDescription = "Periksa tanda dan gejala hipovolemia (nadi meningkat, TD menurun, turgor kulit menurun, rasa haus) serta catat intake-output",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 2, // Terapeutik
                    InterventionCode = "I.03116",
                    InterventionName = "Manajemen Hipovolemia",
                    ActionDescription = "Hitung kebutuhan cairan, pertahankan kepatenan jalur IV, berikan posisi modified Trendelenburg jika ada hipotensi",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 3, // Edukasi
                    InterventionCode = "I.03116",
                    InterventionName = "Manajemen Hipovolemia",
                    ActionDescription = "Anjurkan memperbanyak asupan cairan oral jika tidak ada kontraindikasi",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                diag.Interventions.Add(new MstNursingDiagnosisIntervention
                {
                    Id = Guid.NewGuid(),
                    NursingDiagnosisId = diag.Id,
                    PillarType = 4, // Kolaborasi
                    InterventionCode = "I.03116",
                    InterventionName = "Manajemen Hipovolemia",
                    ActionDescription = "Kolaborasi pemberian cairan intravena isotonis (NaCl 0.9%, Ringer Laktat) sesuai advis DPJP",
                    IsDefaultRecommendation = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });

                list.Add(diag);
            }

            return list;
        }
    }
}
