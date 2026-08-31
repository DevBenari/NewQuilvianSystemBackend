using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Mengisi status keutuhan awal untuk catatan klinis yang sudah tersimpan sebelum modul
    /// rekam medis ada (RM-DEC-014).
    ///
    /// PERUBAHAN CARA DARI RANCANGAN SEMULA. Arsitektur bagian 8.2 menyebut pengisian ini
    /// dikerjakan sebagai migration. Cara itu diganti menjadi service yang dijalankan
    /// terkendali, dengan alasan:
    ///
    /// <list type="bullet">
    /// <item>Jumlah barisnya tidak diketahui, sehingga perlu ditelaah lebih dulu tanpa
    /// mengubah apa pun. Migration tidak dapat ditelaah — ia langsung berjalan.</item>
    /// <item>Migration berjalan otomatis saat aplikasi naik, sehingga tidak dapat dipilih
    /// waktunya. Pengisian ini sebaiknya dijalankan ketika unit rekam medis sudah siap
    /// menerima angka besar pada laporan kelengkapan.</item>
    /// <item>Migration sulit dijalankan bertahap dan sulit dilanjutkan bila terhenti di
    /// tengah.</item>
    /// </list>
    ///
    /// Yang tidak berubah: aturan penentuan statusnya persis seperti RM-DEC-014.
    /// </summary>
    public class MedicalRecordBackfillService
    {
        private readonly ApplicationDbContext _dbContext;

        public MedicalRecordBackfillService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Status kunjungan yang dianggap sudah tidak berjalan lagi.
        ///
        /// Catatan pada kunjungan berstatus ini akan dikunci, karena memang tidak seharusnya
        /// diubah lagi.
        /// </summary>
        private static readonly EncounterStatus[] StatusKunjunganSelesai =
        [
            EncounterStatus.Completed,
            EncounterStatus.Cancelled,
            EncounterStatus.NoShow
        ];

        /// <summary>
        /// Menelaah data lama tanpa mengubah apa pun.
        ///
        /// Menjawab pertanyaan yang tidak dapat dijawab dari source code: berapa banyak catatan
        /// lama yang ada, dan akan menjadi apa masing-masing bila pengisian dijalankan.
        ///
        /// Aman dijalankan kapan saja, termasuk pada basis data yang sedang dipakai, karena
        /// hanya membaca.
        /// </summary>
        public async Task<MedicalRecordBackfillSurveyResponse> SurveyAsync(
            int batchSize = 500,
            CancellationToken cancellationToken = default)
        {
            var hasil = new MedicalRecordBackfillSurveyResponse
            {
                SurveyedAt = DateTime.UtcNow
            };

            var seluruhCppt = _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            hasil.TotalProgressNote = await seluruhCppt.CountAsync(cancellationToken);

            var sudahTerdaftarIds = _dbContext.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.DocumentKind == ClinicalDocumentKind.ProgressNote && !x.IsDelete)
                .Select(x => x.DocumentId);

            var belumTerdaftar = seluruhCppt
                .Where(x => !sudahTerdaftarIds.Contains(x.Id));

            hasil.BelumTerdaftar = await belumTerdaftar.CountAsync(cancellationToken);
            hasil.SudahTerdaftar = hasil.TotalProgressNote - hasil.BelumTerdaftar;

            hasil.TanpaKunjungan = await belumTerdaftar
                .CountAsync(x => x.EncounterId == null || x.EncounterId == Guid.Empty,
                            cancellationToken);

            hasil.PenulisTidakDiketahui = await belumTerdaftar
                .CountAsync(x => x.ProviderUserId == null || x.ProviderUserId == Guid.Empty,
                            cancellationToken);

            hasil.AkanDitandaiDibatalkan = await belumTerdaftar
                .CountAsync(x => x.IsCancel, cancellationToken);

            var dapatDiproses = belumTerdaftar
                .Where(x => x.EncounterId != null && x.EncounterId != Guid.Empty && !x.IsCancel);

            var kunjunganSelesaiIds = _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x => StatusKunjunganSelesai.Contains(x.EncounterStatus)
                            || x.CompletedAt != null
                            || x.IsCancel)
                .Select(x => x.Id);

            hasil.AkanTerkunciTanpaTandaTangan = await dapatDiproses
                .CountAsync(x => kunjunganSelesaiIds.Contains(x.EncounterId!.Value),
                            cancellationToken);

            hasil.AkanTetapDraf = await dapatDiproses
                .CountAsync(x => !kunjunganSelesaiIds.Contains(x.EncounterId!.Value),
                            cancellationToken);

            if (hasil.BelumTerdaftar > 0)
            {
                hasil.CatatanTertua = await belumTerdaftar
                    .MinAsync(x => (DateTime?)x.NoteDateTime, cancellationToken);
                hasil.CatatanTerbaru = await belumTerdaftar
                    .MaxAsync(x => (DateTime?)x.NoteDateTime, cancellationToken);
            }

            var akanDiproses = hasil.BelumTerdaftar - hasil.TanpaKunjungan;
            hasil.PerkiraanJumlahPotongan = batchSize > 0
                ? (int)Math.Ceiling(akanDiproses / (double)batchSize)
                : 0;

            SusunPeringatan(hasil);

            return hasil;
        }

        /// <summary>
        /// Menjalankan pengisian data lama, satu potongan setiap kali dipanggil.
        ///
        /// Dijalankan bertahap dengan sengaja: kunjungan lama dapat berjumlah sangat banyak,
        /// dan memprosesnya sekaligus akan menahan tabel terlalu lama. Pemanggilan berikutnya
        /// melanjutkan dari sisa yang belum diproses.
        /// </summary>
        /// <param name="isDryRun">
        /// Bila benar, seluruh perhitungan dijalankan tetapi tidak ada yang disimpan. Dipakai
        /// untuk membuktikan hasilnya sesuai harapan sebelum benar-benar dijalankan.
        /// </param>
        public async Task<MedicalRecordBackfillRunResponse> ExecuteBatchAsync(
            Guid actorUserId,
            DateTime nowUtc,
            int batchSize = 500,
            bool isDryRun = true,
            CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
                throw new InvalidOperationException("Ukuran potongan harus lebih dari nol.");

            var hasil = new MedicalRecordBackfillRunResponse
            {
                StartedAt = nowUtc,
                IsDryRun = isDryRun
            };

            var sudahTerdaftarIds = _dbContext.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.DocumentKind == ClinicalDocumentKind.ProgressNote && !x.IsDelete)
                .Select(x => x.DocumentId);

            var antrean = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !sudahTerdaftarIds.Contains(x.Id))
                .OrderBy(x => x.CreateDateTime)
                .Take(batchSize)
                .Select(x => new
                {
                    x.Id,
                    x.PatientId,
                    x.EncounterId,
                    x.ProviderUserId,
                    x.IsCancel,
                    x.CreateBy
                })
                .ToListAsync(cancellationToken);

            if (antrean.Count == 0)
            {
                hasil.FinishedAt = nowUtc;
                hasil.MasihAdaSisa = false;
                return hasil;
            }

            var encounterIds = antrean
                .Where(x => x.EncounterId.HasValue && x.EncounterId.Value != Guid.Empty)
                .Select(x => x.EncounterId!.Value)
                .Distinct()
                .ToList();

            var kunjungan = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x => encounterIds.Contains(x.Id))
                .Select(x => new { x.Id, x.EncounterStatus, x.CompletedAt, x.IsCancel })
                .ToListAsync(cancellationToken);

            var barisBaru = new List<TrxClinicalDocumentIntegrity>();

            foreach (var cppt in antrean)
            {
                hasil.JumlahDiproses++;

                // Catatan tanpa kunjungan tidak dapat didaftarkan: baris keutuhan mensyaratkan
                // kunjungan sebagai pengelompokannya. Dilewati, bukan digagalkan, dan dihitung
                // terbuka supaya jumlahnya diketahui.
                if (!cppt.EncounterId.HasValue || cppt.EncounterId.Value == Guid.Empty)
                {
                    hasil.JumlahDilewatiTanpaKunjungan++;
                    continue;
                }

                var penulisDiketahui = cppt.ProviderUserId.HasValue
                                       && cppt.ProviderUserId.Value != Guid.Empty;

                if (!penulisDiketahui)
                    hasil.JumlahPenulisTidakDiketahui++;

                var e = kunjungan.FirstOrDefault(x => x.Id == cppt.EncounterId.Value);

                var kunjunganSudahSelesai = e != null
                    && (StatusKunjunganSelesai.Contains(e.EncounterStatus)
                        || e.CompletedAt.HasValue
                        || e.IsCancel);

                ClinicalDocumentIntegrityStatus status;
                ClinicalDocumentLockTrigger? pemicu;

                if (cppt.IsCancel)
                {
                    status = ClinicalDocumentIntegrityStatus.Cancelled;
                    pemicu = ClinicalDocumentLockTrigger.DocumentCancelled;
                    hasil.JumlahDitandaiDibatalkan++;
                }
                else if (kunjunganSudahSelesai)
                {
                    status = ClinicalDocumentIntegrityStatus.LockedUnsigned;
                    pemicu = ClinicalDocumentLockTrigger.BackfillEncounterClosed;
                    hasil.JumlahTerkunciTanpaTandaTangan++;
                }
                else
                {
                    status = ClinicalDocumentIntegrityStatus.Draft;
                    pemicu = null;
                    hasil.JumlahTetapDraf++;
                }

                barisBaru.Add(new TrxClinicalDocumentIntegrity
                {
                    DocumentKind = ClinicalDocumentKind.ProgressNote,
                    DocumentId = cppt.Id,
                    PatientId = cppt.PatientId,
                    EncounterId = cppt.EncounterId.Value,
                    IntegrityStatus = status,
                    AuthorUserId = penulisDiketahui ? cppt.ProviderUserId!.Value : cppt.CreateBy,
                    IsAuthorKnown = penulisDiketahui,
                    LockedAt = status == ClinicalDocumentIntegrityStatus.Draft ? null : nowUtc,
                    LockTrigger = pemicu,
                    LockedEncounterClosedAt = kunjunganSudahSelesai ? e?.CompletedAt ?? nowUtc : null,
                    CreateDateTime = nowUtc,
                    CreateBy = actorUserId
                });
            }

            if (!isDryRun && barisBaru.Count > 0)
            {
                await _dbContext.Set<TrxClinicalDocumentIntegrity>()
                    .AddRangeAsync(barisBaru, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            hasil.FinishedAt = DateTime.UtcNow;
            hasil.MasihAdaSisa = antrean.Count == batchSize;

            hasil.PerkiraanSisa = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete && !sudahTerdaftarIds.Contains(x.Id),
                            cancellationToken);

            // Pada percobaan, sisa dihitung seolah potongan ini belum diproses, karena memang
            // tidak ada yang disimpan.
            if (!isDryRun)
                hasil.PerkiraanSisa = Math.Max(0, hasil.PerkiraanSisa);

            return hasil;
        }

        private static void SusunPeringatan(MedicalRecordBackfillSurveyResponse hasil)
        {
            if (hasil.BelumTerdaftar == 0)
            {
                hasil.Peringatan.Add(
                    "Tidak ada catatan lama yang perlu diisi. Pengisian tidak perlu dijalankan.");
                return;
            }

            if (hasil.AkanTerkunciTanpaTandaTangan > 0)
            {
                hasil.Peringatan.Add(
                    $"Sebanyak {hasil.AkanTerkunciTanpaTandaTangan:N0} catatan akan ditandai " +
                    "\"terkunci, tidak ditandatangani\". Angka ini akan muncul pada laporan " +
                    "kelengkapan sejak hari pertama. Beri tahu unit rekam medis lebih dulu, " +
                    "karena itu gambaran keadaan sekarang dan bukan kegagalan sistem baru.");
            }

            if (hasil.PenulisTidakDiketahui > 0)
            {
                hasil.Peringatan.Add(
                    $"Sebanyak {hasil.PenulisTidakDiketahui:N0} catatan tidak mencantumkan " +
                    "penulisnya. Barisnya tetap dibuat dengan penanda penulis tidak diketahui, " +
                    "tidak dilewati diam-diam.");
            }

            if (hasil.TanpaKunjungan > 0)
            {
                hasil.Peringatan.Add(
                    $"Sebanyak {hasil.TanpaKunjungan:N0} catatan tidak melekat ke kunjungan mana " +
                    "pun, sehingga tidak dapat didaftarkan dan tidak akan tunduk aturan " +
                    "penguncian. Keadaan ini perlu dinyatakan terbuka, bukan didiamkan.");
            }

            if (hasil.PerkiraanJumlahPotongan > 20)
            {
                hasil.Peringatan.Add(
                    $"Pengisian akan berjalan sekitar {hasil.PerkiraanJumlahPotongan:N0} potongan. " +
                    "Jalankan di luar jam sibuk dan pantau setiap potongannya.");
            }
        }
    }
}
