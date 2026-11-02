using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Pemeriksaan kelayakan admisi sebelum petugas mengisi langkah-langkah admisi
    /// (<c>BE-RWI-230</c>, <c>ISSUE-EPS-010</c>, <c>RWI-DEC-297</c>).
    /// </summary>
    /// <remarks>
    /// <b>Kenapa ada.</b> Seluruh aturan di bawah sudah lama ditegakkan server, tetapi hanya pada
    /// jalur tulis: episode yang hadir ditolak saat bed dipesan, kunjungan aktif ditolak saat
    /// kunjungan dibuat. Sejak titik tulis pindah ke langkah Pilih Bed, petugas baru mengetahuinya
    /// lima langkah sesudah memilih pasien, dan pada pasien yang sedang dirawat sistem sempat
    /// membuat kunjungan dan episode <c>Draft</c> sisa. Method ini membaca aturan yang sama — tanpa
    /// menyalinnya — supaya langkah 1 menolak dengan kalimat yang persis sama.
    ///
    /// <para>
    /// <b>Contoh.</b> Tn. Umar dirawat di Kamar 02 BED 001 sejak 6 Oktober. Petugas mencari No. RM-nya
    /// di langkah 1 dan memilihnya. Jawaban method ini <c>CanContinue = false</c> dengan kalimat
    /// "Tn. Umar sudah dirawat pada episode … di Kamar 02 BED 001. Bila memang pindah kamar, pakai
    /// perpindahan, bukan admisi baru." Layar berhenti di langkah 1 dan menawarkan perpindahan
    /// tempat tidur.
    /// </para>
    /// </remarks>
    public partial class InpEpisodeService
    {
        /// <summary>Kode penolakan pasien yang sedang hadir di ruangan (<c>INV-INP-10</c>).</summary>
        public const string PatientAlreadyAdmittedCode = "PATIENT_ALREADY_ADMITTED";

        /// <summary>Kode penghalang kunjungan rawat jalan yang masih aktif (RJ-DOC-REV-BE-007).</summary>
        public const string ActiveEncounterExistsCode = "ACTIVE_ENCOUNTER_EXISTS";

        /// <summary>
        /// Menilai apakah pasien boleh dibukakan admisi baru. Mengembalikan <c>null</c> bila
        /// pasien tidak ada atau tidak aktif.
        /// </summary>
        /// <remarks>
        /// Urutan penghalang mengikuti seberapa menentukan sebabnya: pasien yang sedang hadir
        /// tidak pernah dapat diadmisi ulang, permintaan Kamar Pulih menentukan jalur admisinya,
        /// dan kunjungan aktif dapat diselesaikan lebih dulu. Admisi <c>Draft</c> lain hanya
        /// peringatan. Draft yang sudah lewat batas digugurkan lebih dulu lewat jalur yang sama
        /// dengan daftar episode (<c>RWI-DEC-030</c>), sehingga Draft basi tidak pernah dilaporkan.
        /// </remarks>
        public async Task<InpatientAdmissionEligibilityResponse?> GetAdmissionEligibilityAsync(
            InpatientAdmissionEligibilityQuery query,
            CancellationToken cancellationToken = default)
        {
            var patientId = query.PatientId;

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == patientId && !x.IsDelete && x.IsActive, cancellationToken);

            if (!patientExists)
            {
                return null;
            }

            var response = new InpatientAdmissionEligibilityResponse { PatientId = patientId };

            // 1. INV-INP-10 — pasien yang sedang hadir. Jalan keluarnya perpindahan, bukan admisi baru.
            var present = await FindPresentEpisodeAsync(patientId, Guid.Empty, cancellationToken);

            if (present != null)
            {
                response.PresentEpisode = new InpatientAdmissionEligibilityEpisodeResponse
                {
                    EpisodeId = present.EpisodeId,
                    EpisodeNumber = present.EpisodeNumber,
                    LocationText = present.LocationText
                };
                AddBlocker(response, PatientAlreadyAdmittedCode, present.RejectionMessage);
            }

            // 2. VAL-RWF-87/88 — permintaan admisi Kamar Pulih yang menunggu.
            var referralCheck = await _admissionReferralService.CheckForAdmissionAsync(
                patientId,
                query.AdmissionReferralId,
                cancellationToken);

            if (!referralCheck.IsSuccess)
            {
                AddBlocker(
                    response,
                    referralCheck.Code ?? InpAdmissionReferralService.ReferralNotPendingCode,
                    referralCheck.Message ?? "Permintaan admisi tidak dapat dipakai.");
            }

            // 3. Kunjungan rawat jalan aktif — saklar, aturan, dan kalimat milik pendaftaran.
            if (OutpatientEncounterRules.IsActiveEncounterBlockEnabled(_configuration))
            {
                var encounter = await OutpatientEncounterRules.FindBlockingEncounterAsync(
                    _dbContext,
                    patientId,
                    cancellationToken);

                if (encounter != null)
                {
                    response.ActiveEncounter = new InpatientAdmissionEligibilityEncounterResponse
                    {
                        EncounterNumber = encounter.EncounterNumber,
                        EncounterDate = encounter.EncounterDate
                    };
                    AddBlocker(
                        response,
                        ActiveEncounterExistsCode,
                        OutpatientEncounterRules.BuildActiveEncounterMessage(encounter));
                }
            }

            // 4. Admisi lain yang masih disiapkan — peringatan, bukan penolakan.
            response.DraftEpisode = await FindOpenDraftAsync(patientId, cancellationToken);

            if (response.DraftEpisode?.CreatedAt is { } draftCreatedAt)
            {
                response.Warnings.Add(BuildOtherDraftWarning(
                    response.DraftEpisode.EpisodeNumber, draftCreatedAt,
                    await _episodeNumberService.HospitalTimeZoneAsync(cancellationToken)));
            }

            var first = response.Blockers.FirstOrDefault();
            response.CanContinue = first == null;
            response.BlockingCode = first?.Code;
            response.Message = first?.Message;

            return response;
        }

        /// <summary>
        /// Kalimat peringatan admisi <c>Draft</c> ganda, dipakai bersama oleh pembukaan admisi dan
        /// pemeriksaan kelayakan supaya keduanya tidak bercabang.
        /// </summary>
        /// <remarks>
        /// PLAN-REPAIR-EPS-041 FIX-14: waktu dibaca pada zona rumah sakit dan bulan berbahasa Indonesia.
        /// Contoh Draft dibuat 03:44 UTC → "sejak 26 Oktober 2026 pukul 10.44".
        /// </remarks>
        private static string BuildOtherDraftWarning(string episodeNumber, DateTime createdAtUtc, TimeZoneInfo timeZone)
        {
            var local = InpAdmissionText.ToLocal(createdAtUtc, timeZone);

            return "Pasien ini punya admisi lain yang sedang disiapkan, yaitu " +
                $"{episodeNumber} sejak {InpAdmissionText.FormatLongDate(local)} pukul {local:HH.mm}. " +
                "Lanjutkan bila memang admisi terpisah, atau batalkan yang lama lebih dulu.";
        }

        private static void AddBlocker(InpatientAdmissionEligibilityResponse response, string code, string message)
        {
            response.Blockers.Add(new InpatientAdmissionEligibilityBlockerResponse { Code = code, Message = message });
        }

        /// <summary>
        /// Admisi <c>Draft</c> tertua milik pasien yang masih berlaku. Draft yang sudah lewat batas
        /// digugurkan lebih dulu lewat <see cref="ExpireDraftIfDueAsync"/>.
        /// </summary>
        private async Task<InpatientAdmissionEligibilityEpisodeResponse?> FindOpenDraftAsync(
            Guid patientId,
            CancellationToken cancellationToken)
        {
            var drafts = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.EpisodeStatus == InpEpisodeStatus.Draft &&
                    !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.Id,
                    x.EpisodeNumber,
                    x.CreateDateTime,
                    LastTouchedAt = x.UpdateDateTime ?? x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            if (drafts.Count == 0)
            {
                return null;
            }

            var setting = await _settingService.GetEffectiveSettingAsync(cancellationToken);
            var cutoff = DateTime.UtcNow.AddHours(-setting.DraftEpisodeExpiryHours);

            foreach (var draft in drafts)
            {
                if (draft.LastTouchedAt <= cutoff)
                {
                    var tracked = await LoadEpisodeForWriteAsync(draft.Id, cancellationToken);

                    if (tracked == null ||
                        await ExpireDraftIfDueAsync(tracked, cancellationToken) ||
                        tracked.EpisodeStatus != InpEpisodeStatus.Draft)
                    {
                        continue;
                    }
                }

                var now = DateTime.UtcNow;
                var hasActiveReservation = await _dbContext.Set<InpBedReservation>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.EpisodeId == draft.Id &&
                        !x.IsDelete &&
                        x.ReservationStatus == InpBedReservationStatus.Active &&
                        x.ExpiresAt > now,
                        cancellationToken);

                return new InpatientAdmissionEligibilityEpisodeResponse
                {
                    EpisodeId = draft.Id,
                    EpisodeNumber = draft.EpisodeNumber,
                    CreatedAt = draft.CreateDateTime,
                    HasActiveReservation = hasActiveReservation
                };
            }

            return null;
        }
    }
}
