using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Menyusun usulan isian resume pulang dari data klinis yang sudah ada, beserta label
    /// sumbernya. <c>BE-RWI-086</c> menyerap <c>RWI-DEC-112</c> dan <c>FR-RI-197</c>.
    /// </summary>
    /// <remarks>
    /// <b>Service ini tidak pernah menyimpan apa pun.</b> Tidak ada satu pun pemanggilan
    /// <c>SaveChanges</c> di sini, dan itu bukan kebetulan melainkan inti keputusannya. Resume
    /// pulang adalah dokumen bertanda tangan: tanda tangan dokter berarti dokter menyatakan
    /// isinya benar. Menyimpan usulan mesin sebagai isi resume tanpa dokter membacanya berarti
    /// meminjam tanda tangan itu untuk kalimat yang tidak pernah ia tulis — <c>RWI-DEC-112</c>.
    ///
    /// <para>
    /// <b>Satu sumber yang gagal tidak menggagalkan usulan lainnya.</b> Setiap sumber dibaca di
    /// dalam <c>try</c>-nya sendiri; kegagalan mana pun menghasilkan satu isian berstatus
    /// <see cref="PrefillSourceStatus.Unavailable"/> beserta keterangannya, dan bagian lain
    /// tetap diusulkan — roadmap <c>BE-RWI-086</c> acceptance criteria 3. Kegagalan pembacaan
    /// <b>tidak pernah</b> dilaporkan sebagai <see cref="PrefillSourceStatus.Empty"/>: "tidak
    /// ada bahan" dan "bahan tidak terbaca" menuntun dokter pada dua keputusan berbeda.
    /// </para>
    ///
    /// <para>
    /// <b>Usulan tidak pernah diperbarui sendiri.</b> Bila sumbernya berubah sesudah dokter
    /// menyalin usulan ke draf resume, drafnya tidak ikut berubah. Dokter menekan "Isi dari data
    /// klinis" lagi bila memang menghendakinya — <c>RWI-DEC-129</c> (7).
    /// </para>
    /// </remarks>
    public class InpDischargeSummaryPrefillService
    {
        private readonly ApplicationDbContext _dbContext;

        public InpDischargeSummaryPrefillService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Banyaknya butir yang ikut disebutkan pada satu isian usulan.</summary>
        /// <remarks>
        /// Usulan adalah bahan, bukan salinan rekam medis. Episode yang panjang dapat memuat
        /// puluhan tindakan; menyalin seluruhnya membuat kotak isian tidak terbaca dan dokter
        /// menghapusnya begitu saja — yang berarti usulannya justru tidak terpakai.
        /// </remarks>
        private const int BatasButirPerIsian = 20;

        /// <summary>
        /// Menyusun usulan isian resume untuk satu episode. Mengembalikan <c>null</c> bila
        /// episodenya tidak ada.
        /// </summary>
        public async Task<DischargeSummaryPrefillResponse?> BuildPrefillAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.EncounterId, x.EpisodeNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
            {
                return null;
            }

            // Acceptance criteria 4 — usulan hanya dibentuk untuk resume yang BELUM
            // ditandatangani. Resume yang sudah bertanda tangan adalah rekam medis; menawarkan
            // usulan di atasnya mengundang penimpaan dokumen yang sudah sah.
            var signedAt = await _dbContext.Set<InpDischargeSummary>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .Select(x => x.SignedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var response = new DischargeSummaryPrefillResponse
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                IsSummarySigned = signedAt != null
            };

            if (signedAt != null)
            {
                response.Message =
                    "Resume pulang episode ini sudah ditandatangani, sehingga usulan isian " +
                    "tidak dibentuk.";
                return response;
            }

            response.PrimaryDiagnosisText = await UkurAsync(
                response,
                "PrimaryDiagnosisText",
                ct => BacaDiagnosisAsync(episode.EncounterId, episodeId, utama: true, ct),
                cancellationToken);

            response.SecondaryDiagnosisText = await UkurAsync(
                response,
                "SecondaryDiagnosisText",
                ct => BacaDiagnosisAsync(episode.EncounterId, episodeId, utama: false, ct),
                cancellationToken);

            response.ProcedureSummary = await UkurAsync(
                response,
                "ProcedureSummary",
                ct => BacaTindakanSelesaiAsync(episodeId, ct),
                cancellationToken);

            response.DischargeMedicationNote = await UkurAsync(
                response,
                "DischargeMedicationNote",
                ct => BacaObatPulangAsync(episodeId, ct),
                cancellationToken);

            response.ImportantFindingsSummary = await UkurAsync(
                response,
                "ImportantFindingsSummary",
                ct => BacaPemeriksaanPentingAsync(episodeId, ct),
                cancellationToken);

            response.EducationSummary = await UkurAsync(
                response,
                "EducationSummary",
                ct => BacaEdukasiAsync(episodeId, ct),
                cancellationToken);

            // Tiga isian yang SENGAJA tidak diusulkan. Ketiganya adalah penilaian dokter atas
            // keseluruhan perawatan, bukan kumpulan fakta yang dapat disusun ulang dari tabel —
            // 02-backend-architecture.md bagian 11.5.5.
            response.ClinicalSummary = TidakDiusulkan(
                "Ringkasan Perawatan adalah penilaian dokter, bukan kumpulan data.");
            response.DischargeConditionNote = TidakDiusulkan(
                "Kondisi Saat Pulang dinilai dokter pada saat pasien pulang.");
            response.FollowUpInstruction = TidakDiusulkan(
                "Rencana Kontrol adalah keputusan dokter, bukan turunan data yang sudah ada.");

            return response;
        }

        // =====================================================================
        // Pembacaan per sumber
        // =====================================================================

        private async Task<DischargeSummaryPrefillFieldResponse> BacaDiagnosisAsync(
            Guid encounterId,
            Guid episodeId,
            bool utama,
            CancellationToken cancellationToken)
        {
            var baris = await _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    (x.InpEpisodeId == episodeId || x.EncounterId == encounterId) &&
                    !x.IsDelete &&
                    x.IsPrimary == utama &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled)
                .OrderByDescending(x => x.DiagnosisDateTime)
                .Take(BatasButirPerIsian)
                .Select(x => new
                {
                    x.DiagnosisCode,
                    x.DiagnosisName,
                    x.DiagnosisDateTime,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null
                })
                .ToListAsync(cancellationToken);

            if (baris.Count == 0)
            {
                return Kosong(utama
                    ? "Belum ada diagnosis utama yang tercatat pada perawatan ini."
                    : "Belum ada diagnosis sekunder yang tercatat pada perawatan ini.");
            }

            return new DischargeSummaryPrefillFieldResponse
            {
                Value = string.Join(
                    "\n",
                    baris.Select(x => $"{x.DiagnosisCode} — {x.DiagnosisName}")),
                SourceStatus = (int)PrefillSourceStatus.Available,
                SourceStatusName = PrefillSourceStatus.Available.ToString(),
                Sources = baris
                    .Select(x => new DischargeSummaryPrefillSourceResponse
                    {
                        Label = $"Diagnosis {x.DiagnosisCode}",
                        Detail = x.DoctorName,
                        RecordedAt = x.DiagnosisDateTime
                    })
                    .ToList()
            };
        }

        private async Task<DischargeSummaryPrefillFieldResponse> BacaTindakanSelesaiAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            var baris = await _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .Where(x =>
                    x.InpEpisodeId == episodeId &&
                    !x.IsDelete &&
                    x.ProcedureStatus == PatientProcedureStatus.Completed)
                .OrderBy(x => x.CompletedAt ?? x.ProcedureDateTime)
                .Take(BatasButirPerIsian)
                .Select(x => new
                {
                    x.ProcedureCodeSnapshot,
                    x.ProcedureNameSnapshot,
                    Waktu = x.CompletedAt ?? x.ProcedureDateTime,
                    Pelaksana = x.Doctor != null ? x.Doctor.FullName : null
                })
                .ToListAsync(cancellationToken);

            if (baris.Count == 0)
            {
                return Kosong("Belum ada tindakan selesai yang tercatat pada perawatan ini.");
            }

            return new DischargeSummaryPrefillFieldResponse
            {
                Value = string.Join(
                    "\n",
                    baris.Select(x =>
                        $"{x.ProcedureNameSnapshot} ({x.Waktu:dd/MM/yyyy})")),
                SourceStatus = (int)PrefillSourceStatus.Available,
                SourceStatusName = PrefillSourceStatus.Available.ToString(),
                Sources = baris
                    .Select(x => new DischargeSummaryPrefillSourceResponse
                    {
                        Label = $"Tindakan {x.ProcedureCodeSnapshot}",
                        Detail = x.Pelaksana,
                        RecordedAt = x.Waktu
                    })
                    .ToList()
            };
        }

        private async Task<DischargeSummaryPrefillFieldResponse> BacaObatPulangAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            var resep = _dbContext.Set<PhmPrescription>()
                .AsNoTracking()
                .Where(x =>
                    x.InpEpisodeId == episodeId &&
                    !x.IsDelete &&
                    x.PrescriptionOrderType == PrescriptionOrderType.Discharge);

            var baris = await (from butir in _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                               join induk in resep on butir.PrescriptionId equals induk.Id
                               where !butir.IsDelete
                               orderby induk.PrescriptionDateTime
                               select new
                               {
                                   butir.DrugNameSnapshot,
                                   butir.DoseUnitSymbolSnapshot,
                                   butir.Dose,
                                   butir.FrequencyText,
                                   induk.PrescriptionNumber,
                                   induk.PrescriptionDateTime,
                                   DoctorName = induk.Doctor != null ? induk.Doctor.FullName : null
                               })
                              .Take(BatasButirPerIsian)
                              .ToListAsync(cancellationToken);

            if (baris.Count == 0)
            {
                return Kosong("Belum ada resep obat pulang pada perawatan ini.");
            }

            return new DischargeSummaryPrefillFieldResponse
            {
                Value = string.Join(
                    "\n",
                    baris.Select(x =>
                        ($"{x.DrugNameSnapshot} {x.Dose}{x.DoseUnitSymbolSnapshot} " +
                         $"{x.FrequencyText}").Trim())),
                SourceStatus = (int)PrefillSourceStatus.Available,
                SourceStatusName = PrefillSourceStatus.Available.ToString(),
                Sources = baris
                    .Select(x => new DischargeSummaryPrefillSourceResponse
                    {
                        Label = $"Resep pulang {x.PrescriptionNumber}",
                        Detail = x.DoctorName,
                        RecordedAt = x.PrescriptionDateTime
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Pemeriksaan Penting. Dibaca dari bacaan radiologi yang sudah dirilis.
        /// </summary>
        /// <remarks>
        /// <b>Bagian laboratorium belum dapat dibaca, dan itu dilaporkan apa adanya.</b>
        /// <c>02-backend-architecture.md</c> bagian 11.5.5 menyebut sumbernya "hasil
        /// laboratorium/radiologi final yang ditandai kritis atau abnormal". Pada repository
        /// ini, <c>LabExamination</c> belum menyimpan nilai hasil maupun penandaan kritis —
        /// yang tersimpan baru keadaan pemeriksaannya. Karena itu bagian laboratorium selalu
        /// disebut sebagai sumber yang belum tersedia, bukan dihitung nol diam-diam.
        /// </remarks>
        private async Task<DischargeSummaryPrefillFieldResponse> BacaPemeriksaanPentingAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            const string KeteranganLab =
                "Sumber laboratorium belum tersedia: nilai hasil dan penandaan kritis belum " +
                "tersimpan pada LabExamination.";

            var pesanan = _dbContext.Set<RadOrder>()
                .AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete);

            var baris = await (from versi in _dbContext.Set<RadReportVersion>().AsNoTracking()
                               join laporan in _dbContext.Set<RadReport>().AsNoTracking()
                                   on versi.RadReportId equals laporan.Id
                               join order in pesanan on laporan.RadOrderId equals order.Id
                               where !versi.IsDelete
                                     && !laporan.IsDelete
                                     && versi.ReleasedAt != null
                                     && versi.VersionNumber == laporan.CurrentVersionNumber
                               orderby versi.ReleasedAt
                               select new
                               {
                                   laporan.ReportNumber,
                                   versi.Impression,
                                   versi.ReleasedAt
                               })
                              .Take(BatasButirPerIsian)
                              .ToListAsync(cancellationToken);

            if (baris.Count == 0)
            {
                var kosong = Kosong(
                    "Belum ada bacaan radiologi final pada perawatan ini. " + KeteranganLab);
                kosong.Sources.Add(new DischargeSummaryPrefillSourceResponse
                {
                    Label = "Hasil Laboratorium",
                    Detail = KeteranganLab
                });
                return kosong;
            }

            var hasil = new DischargeSummaryPrefillFieldResponse
            {
                Value = string.Join(
                    "\n",
                    baris.Select(x => $"{x.ReportNumber}: {x.Impression}")),
                SourceStatus = (int)PrefillSourceStatus.Available,
                SourceStatusName = PrefillSourceStatus.Available.ToString(),
                Note = KeteranganLab,
                Sources = baris
                    .Select(x => new DischargeSummaryPrefillSourceResponse
                    {
                        Label = $"Bacaan Radiologi {x.ReportNumber}",
                        RecordedAt = x.ReleasedAt
                    })
                    .ToList()
            };

            hasil.Sources.Add(new DischargeSummaryPrefillSourceResponse
            {
                Label = "Hasil Laboratorium",
                Detail = KeteranganLab
            });

            return hasil;
        }

        private async Task<DischargeSummaryPrefillFieldResponse> BacaEdukasiAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            var baris = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x =>
                    x.InpEpisodeId == episodeId &&
                    !x.IsDelete &&
                    x.AssessmentStatus == PatientAssessmentStatus.Completed &&
                    x.EducationNote != null &&
                    x.EducationNote != "")
                .OrderBy(x => x.CreateDateTime)
                .Take(BatasButirPerIsian)
                .Select(x => new
                {
                    x.EducationNote,
                    x.AssessmentType,
                    x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            if (baris.Count == 0)
            {
                return Kosong(
                    "Belum ada catatan edukasi pada pengkajian yang sudah selesai.");
            }

            return new DischargeSummaryPrefillFieldResponse
            {
                Value = string.Join("\n", baris.Select(x => x.EducationNote)),
                SourceStatus = (int)PrefillSourceStatus.Available,
                SourceStatusName = PrefillSourceStatus.Available.ToString(),
                Sources = baris
                    .Select(x => new DischargeSummaryPrefillSourceResponse
                    {
                        Label = $"Pengkajian {x.AssessmentType}",
                        RecordedAt = x.CreateDateTime
                    })
                    .ToList()
            };
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Menjalankan satu pembacaan sumber, mencatat waktunya, dan menangkap kegagalannya
        /// supaya sumber lain tetap diusulkan.
        /// </summary>
        /// <remarks>
        /// <b>Pengukuran waktu wajib, bukan hiasan.</b> <c>NFR-027</c> menyebut batas 5 detik
        /// per sumber sebagai <b>angka usulan desain</b> yang dikonfirmasi saat approval.
        /// Tanpa angka nyata, batas itu tidak pernah dapat dinilai — dan yang biasanya terjadi
        /// berikutnya adalah angkanya diam-diam diubah agar cocok dengan kenyataan. Angkanya
        /// dilaporkan apa adanya dan dibawa kembali ke pemilik bila jauh melampaui batas.
        ///
        /// <para>
        /// <b><see cref="Exception"/> ditangkap seluas mungkin, dan itu disengaja.</b> Yang
        /// dijaga di sini bukan satu jenis kegagalan tertentu melainkan sifat "satu sumber gagal
        /// tidak menggagalkan yang lain". Menyempitkannya ke jenis tertentu berarti jenis
        /// kegagalan berikutnya — yang belum terpikirkan — akan menjatuhkan seluruh usulan.
        /// <see cref="OperationCanceledException"/> tetap dilewatkan: pembatalan permintaan
        /// bukan kegagalan sumber.
        /// </para>
        /// </remarks>
        private async Task<DischargeSummaryPrefillFieldResponse> UkurAsync(
            DischargeSummaryPrefillResponse response,
            string fieldName,
            Func<CancellationToken, Task<DischargeSummaryPrefillFieldResponse>> pembaca,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var hasil = await pembaca(cancellationToken);
                stopwatch.Stop();

                response.SourceTimings.Add(new DischargeSummaryPrefillTimingResponse
                {
                    Field = fieldName,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    IsSuccessful = true
                });

                return hasil;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                stopwatch.Stop();

                response.SourceTimings.Add(new DischargeSummaryPrefillTimingResponse
                {
                    Field = fieldName,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    IsSuccessful = false
                });

                return new DischargeSummaryPrefillFieldResponse
                {
                    Value = null,
                    SourceStatus = (int)PrefillSourceStatus.Unavailable,
                    SourceStatusName = PrefillSourceStatus.Unavailable.ToString(),
                    // Pesan galat aslinya TIDAK dikirim ke pemanggil: ia dapat memuat nama
                    // tabel, kolom, dan potongan data. Yang dibutuhkan dokter hanyalah
                    // mengetahui bahwa bagian ini perlu ia isi sendiri.
                    Note = "Sumber data untuk isian ini gagal dibaca. Isi bagian ini sendiri " +
                           "lalu periksa kembali sebelum menandatangani.",
                    ErrorType = exception.GetType().Name
                };
            }
        }

        private static DischargeSummaryPrefillFieldResponse Kosong(string keterangan)
            => new()
            {
                Value = null,
                SourceStatus = (int)PrefillSourceStatus.Empty,
                SourceStatusName = PrefillSourceStatus.Empty.ToString(),
                Note = keterangan
            };

        private static DischargeSummaryPrefillFieldResponse TidakDiusulkan(string alasan)
            => new()
            {
                Value = null,
                SourceStatus = (int)PrefillSourceStatus.Empty,
                SourceStatusName = PrefillSourceStatus.Empty.ToString(),
                Note = alasan
            };
    }
}
