using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    public partial class EmergencyDispositionService
    {
        public sealed record HasilPermintaanRawatInap<T>(T? Data, int StatusCode, string? Penolakan)
        {
            public bool Berhasil => Penolakan == null;

            public static HasilPermintaanRawatInap<T> Ok(T data, int statusCode = StatusCodes.Status200OK)
                => new(data, statusCode, null);

            public static HasilPermintaanRawatInap<T> Gagal(int statusCode, string penolakan)
                => new(default, statusCode, penolakan);
        }

        private sealed record DpjpIgdAktif(Guid DoctorId, string? DoctorName, bool HasAccount);

        private const int BatasTeksKlinis = 2000;
        private const int BatasAlasan = 500;
        private const int BatasPilihanDokter = 50;

        private const string PesanFormTidakAda = "Form Permintaan Rawat Inap tidak ditemukan.";
        private const string PesanTindakLanjutTidakAda = "Tindak lanjut tidak ditemukan.";
        private const string PesanTindakLanjutKosong = "DispositionId wajib diisi.";
        private const string PesanKunjunganTidakAda = "Kunjungan IGD tidak ditemukan.";
        private const string PesanTindakLanjutBukanRawatInap = "Form Permintaan Rawat Inap hanya dapat dibuat dari tindak lanjut rawat inap yang sudah dikonfirmasi dokter.";
        private const string PesanKunjunganTanpaPasien = "Kunjungan ini belum punya pasien dan encounter. Lengkapi pendaftaran lebih dulu.";
        private const string PesanFormAktifAda = "Kunjungan ini sudah punya Form Permintaan Rawat Inap yang aktif.";
        private const string PesanPermintaanAdmisiLain = "Pasien sudah punya permintaan admisi yang menunggu petugas admisi.";
        private const string PesanBukanDraf = "Form hanya dapat diubah saat masih draf.";
        private const string PesanLamaRawat = "Estimasi lama rawat harus antara 1 sampai 90 hari.";
        private const string PesanIndikasiPanjang = "Indikasi klinis paling panjang 2.000 karakter.";
        private const string PesanRencanaPanjang = "Rencana perawatan paling panjang 2.000 karakter.";
        private const string PesanKelasTidakBerlaku = "Kelas perawatan yang dipilih tidak berlaku untuk rawat inap.";
        private const string PesanUnitTidakAktif = "Unit layanan yang dipilih tidak ditemukan atau tidak aktif.";
        private const string PesanDokterTidakAktif = "Dokter usulan tidak ditemukan atau tidak aktif.";
        private const string PesanDokterTidakBerjadwal = "Dokter usulan tidak berjadwal di unit yang dipilih.";
        private const string PesanIsianBelumLengkap = "Lengkapi unit layanan, kelas perawatan, DPJP usulan, estimasi lama rawat, dan indikasi klinis sebelum meminta tanda tangan.";
        private const string PesanTanpaDpjp = "Pasien belum punya DPJP IGD. Tetapkan DPJP lebih dulu.";
        private const string PesanBukanDpjpAktif = "Hanya DPJP IGD aktif pasien ini yang dapat menandatangani atau mengembalikan form.";
        private const string PesanTidakMenungguTtd = "Form ini tidak sedang menunggu tanda tangan.";
        private const string PesanCatatanKembali = "Catatan pengembalian wajib diisi, paling panjang 500 karakter.";
        private const string PesanAlasanBatal = "Alasan pembatalan wajib diisi, paling panjang 500 karakter.";
        private const string PesanSudahDibatalkan = "Form ini sudah dibatalkan.";
        private const string PesanVersiBasi = "Form sudah diubah orang lain. Muat ulang lalu coba lagi.";
        private const string PesanTindakLanjutTidakBerlaku = "Tindak lanjut rawat inap sudah tidak berlaku. Form tidak dapat ditandatangani.";
        private const string PesanDataPasienGagal = "Data pasien tidak dapat dibaca saat ini. Coba tandatangani lagi.";
        private const string PesanAdmisiSudahDibuka = "Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD.";
        private const string PesanJalankanTanpaForm = "Tindak lanjut rawat inap dijalankan oleh petugas admisi. Lengkapi Form Permintaan Rawat Inap dan minta tanda tangan DPJP agar pasien masuk Antrean Transfer IGD/Poli.";
        private const string PesanJalankanLewatAdmisi = "Pasien sudah masuk Antrean Transfer IGD/Poli. Tindak lanjut ini dijalankan otomatis saat petugas admisi memproses admisinya.";
        private const string PesanTujuanBukanAntrean = "Tindak lanjut ini tidak dapat masuk Antrean Transfer IGD/Poli: jenis tindak lanjutnya harus aktif dan unit tujuannya harus unit rawat inap. Perbaiki tindak lanjut lebih dulu.";
        private const string PesanAdmisiDibukaDariTindakLanjut = "Admisi rawat inap sudah dibuka dari tindak lanjut ini. Batalkan admisi di Rawat Inap lebih dulu.";
        private const string PesanCetakBelumTtd = "Form Permintaan Rawat Inap hanya dapat dicetak sesudah ditandatangani.";
        private const string SumberTransferIgd = "Emergency";

        private static readonly JsonSerializerOptions OpsiSalinanBeku = new(JsonSerializerDefaults.Web);
        private static readonly TimeZoneInfo ZonaWaktuCetak = TentukanZonaWaktuCetak();
        private static readonly string[] NamaBulanCetak =
        [
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        ];

        public async Task<IReadOnlyList<EmergencyAdmissionRequestSummary>> AmbilPermintaanRawatInapPerKunjunganAsync(
            Guid emergencyVisitId,
            CancellationToken cancellationToken = default)
        {
            var data = await ProyeksiRingkasan(_dbContext.Set<EmgAdmissionRequest>()
                    .AsNoTracking()
                    .Where(x => x.EmergencyVisitId == emergencyVisitId && !x.IsDelete)
                    .OrderByDescending(x => x.CreateDateTime))
                .ToListAsync(cancellationToken);

            var sudahDiadmisi = await AmbilTindakLanjutSudahDiadmisiAsync(
                data.Where(x => x.Status == EmergencyAdmissionRequestStatus.Signed).Select(x => x.DispositionId).ToList(),
                cancellationToken);

            foreach (var item in data)
                item.DisplayStatus = TampilanStatus(item.Status, sudahDiadmisi.Contains(item.DispositionId));

            return data;
        }

        public async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> AmbilPermintaanRawatInapAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var form = await MuatFormAsync(id, false, cancellationToken);
            if (form == null)
                return HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>.Gagal(
                    StatusCodes.Status404NotFound, PesanFormTidakAda);

            return HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>.Ok(
                await BentukResponsAsync(form, actorUserId, cancellationToken));
        }

        public async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> BuatPermintaanRawatInapAsync(
            CreateEmergencyAdmissionRequestRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.DispositionId == Guid.Empty)
                return Gagal(StatusCodes.Status400BadRequest, PesanTindakLanjutKosong);

            var disposition = await _dbContext.Set<EmgDisposition>()
                .AsNoTracking()
                .Include(x => x.DispositionType)
                .FirstOrDefaultAsync(x => x.Id == request.DispositionId && !x.IsDelete, cancellationToken);
            if (disposition == null)
                return Gagal(StatusCodes.Status404NotFound, PesanTindakLanjutTidakAda);

            var visit = await _dbContext.Set<EmgVisit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == disposition.EmergencyVisitId && !x.IsDelete, cancellationToken);
            if (visit == null)
                return Gagal(StatusCodes.Status404NotFound, PesanKunjunganTidakAda);

            if (disposition.DispositionType?.RequiresAdmissionRequest != true
                || disposition.DispositionStatus != EmergencyDispositionStatus.Confirmed)
                return Gagal(StatusCodes.Status409Conflict, PesanTindakLanjutBukanRawatInap);

            if (!await TindakLanjutMasukAntreanAsync(disposition.Id, cancellationToken))
                return Gagal(StatusCodes.Status409Conflict, PesanTujuanBukanAntrean);

            if (!visit.PatientId.HasValue || !visit.EncounterId.HasValue)
                return Gagal(StatusCodes.Status409Conflict, PesanKunjunganTanpaPasien);

            var now = DateTime.UtcNow;
            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await KunciKunjunganAsync(visit.Id, cancellationToken);

            var adaFormAktif = await _dbContext.Set<EmgAdmissionRequest>()
                .AnyAsync(x => x.EmergencyVisitId == visit.Id
                    && !x.IsDelete
                    && (x.Status == EmergencyAdmissionRequestStatus.Draft
                        || x.Status == EmergencyAdmissionRequestStatus.AwaitingSignature),
                    cancellationToken);
            if (adaFormAktif)
                return Gagal(StatusCodes.Status409Conflict, PesanFormAktifAda);

            var formTerkirim = await _dbContext.Set<EmgAdmissionRequest>()
                .AsNoTracking()
                .Where(x => x.EmergencyVisitId == visit.Id
                    && !x.IsDelete
                    && x.Status == EmergencyAdmissionRequestStatus.Signed)
                .Select(x => x.DispositionId)
                .ToListAsync(cancellationToken);
            var terkirimSudahDiadmisi = await AmbilTindakLanjutSudahDiadmisiAsync(formTerkirim, cancellationToken);
            if (formTerkirim.Any(x => !terkirimSudahDiadmisi.Contains(x)))
                return Gagal(StatusCodes.Status409Conflict, PesanFormAktifAda);

            var adaPermintaanAdmisi = await _dbContext.Set<InpAdmissionReferral>()
                .AsNoTracking()
                .AnyAsync(x => x.PatientId == visit.PatientId.Value
                    && x.Status == InpAdmissionReferralStatus.Pending
                    && !x.IsDelete,
                    cancellationToken);
            if (adaPermintaanAdmisi)
                return Gagal(StatusCodes.Status409Conflict, PesanPermintaanAdmisiLain);

            var form = new EmgAdmissionRequest
            {
                EmergencyVisitId = visit.Id,
                DispositionId = disposition.Id,
                EncounterId = visit.EncounterId.Value,
                PatientId = visit.PatientId.Value,
                Status = EmergencyAdmissionRequestStatus.Draft,
                RequestedServiceUnitId = disposition.DestinationServiceUnitId,
                EstimatedLengthOfStayDays = 3,
                ClinicalIndication = Potong(Rapikan(disposition.DispositionReason), BatasTeksKlinis),
                CarePlanInstruction = Potong(Rapikan(disposition.FollowUpInstruction), BatasTeksKlinis),
                CreateBy = actorUserId,
                CreateDateTime = now
            };
            _dbContext.Set<EmgAdmissionRequest>().Add(form);
            TambahKejadian(form, EmergencyAdmissionRequestEventType.Opened, null,
                EmergencyAdmissionRequestStatus.Draft, actorUserId, now, null);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaksi.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (AdalahBentrokFormAktif(ex))
            {
                return Gagal(StatusCodes.Status409Conflict, PesanFormAktifAda);
            }

            var tersimpan = await MuatFormAsync(form.Id, false, cancellationToken);
            return HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>.Ok(
                await BentukResponsAsync(tersimpan!, actorUserId, cancellationToken),
                StatusCodes.Status201Created);
        }

        public async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> SimpanDrafPermintaanAsync(
            Guid id,
            UpdateEmergencyAdmissionRequestRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var form = await MuatFormAsync(id, true, cancellationToken);
            if (form == null)
                return Gagal(StatusCodes.Status404NotFound, PesanFormTidakAda);

            if (request == null)
                return Gagal(StatusCodes.Status400BadRequest, PesanIsianBelumLengkap);

            var indikasi = Rapikan(request.ClinicalIndication);
            var rencana = Rapikan(request.CarePlanInstruction);
            var penolakanIsian = await ValidasiIsianAsync(
                request.RequestedServiceUnitId,
                request.RequestedPatientClassId,
                request.ProposedInpatientDoctorId,
                request.EstimatedLengthOfStayDays,
                indikasi,
                rencana,
                cancellationToken);
            if (penolakanIsian != null)
                return Gagal(StatusCodes.Status400BadRequest, penolakanIsian);

            if (request.RowVersion != form.RowVersion)
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);

            if (form.Status != EmergencyAdmissionRequestStatus.Draft)
                return Gagal(StatusCodes.Status409Conflict, PesanBukanDraf);

            form.RequestedServiceUnitId = KosongkanGuid(request.RequestedServiceUnitId);
            form.RequestedPatientClassId = KosongkanGuid(request.RequestedPatientClassId);
            form.ProposedInpatientDoctorId = KosongkanGuid(request.ProposedInpatientDoctorId);
            form.EstimatedLengthOfStayDays = request.EstimatedLengthOfStayDays;
            form.IsCito = request.IsCito;
            form.NeedsIntensiveCare = request.NeedsIntensiveCare;
            form.NeedsIsolation = request.NeedsIsolation;
            form.NeedsOxygenSupport = request.NeedsOxygenSupport;
            form.NeedsSpecialMonitoring = request.NeedsSpecialMonitoring;
            form.ClinicalIndication = indikasi;
            form.CarePlanInstruction = rencana;
            TandaiBerubah(form, actorUserId, DateTime.UtcNow);

            return await SimpanLaluBentukAsync(form, actorUserId, cancellationToken);
        }

        public async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> MintaTandaTanganAsync(
            Guid id,
            EmergencyAdmissionRequestRowVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var form = await MuatFormAsync(id, true, cancellationToken);
            if (form == null)
                return Gagal(StatusCodes.Status404NotFound, PesanFormTidakAda);

            if (!form.RequestedServiceUnitId.HasValue
                || !form.RequestedPatientClassId.HasValue
                || !form.ProposedInpatientDoctorId.HasValue
                || string.IsNullOrWhiteSpace(form.ClinicalIndication))
                return Gagal(StatusCodes.Status400BadRequest, PesanIsianBelumLengkap);

            var penolakanIsian = await ValidasiIsianAsync(
                form.RequestedServiceUnitId,
                form.RequestedPatientClassId,
                form.ProposedInpatientDoctorId,
                form.EstimatedLengthOfStayDays,
                form.ClinicalIndication,
                form.CarePlanInstruction,
                cancellationToken);
            if (penolakanIsian != null)
                return Gagal(StatusCodes.Status400BadRequest, penolakanIsian);

            if (request == null || request.RowVersion != form.RowVersion)
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);

            if (form.Status != EmergencyAdmissionRequestStatus.Draft)
                return Gagal(StatusCodes.Status409Conflict, PesanBukanDraf);

            var dpjp = await AmbilDpjpIgdAktifAsync(form.EmergencyVisitId, cancellationToken);
            if (dpjp == null)
                return Gagal(StatusCodes.Status409Conflict, PesanTanpaDpjp);
            if (!dpjp.HasAccount)
                return Gagal(StatusCodes.Status409Conflict,
                    $"DPJP IGD ({dpjp.DoctorName}) belum punya akun untuk menandatangani. Alihkan DPJP atau hubungi admin.");

            var now = DateTime.UtcNow;
            form.Status = EmergencyAdmissionRequestStatus.AwaitingSignature;
            form.SignatureRequestedAt = now;
            form.SignatureRequestedByUserId = actorUserId;
            TandaiBerubah(form, actorUserId, now);
            TambahKejadian(form, EmergencyAdmissionRequestEventType.SignatureRequested,
                EmergencyAdmissionRequestStatus.Draft, EmergencyAdmissionRequestStatus.AwaitingSignature,
                actorUserId, now, null);

            return await SimpanLaluBentukAsync(form, actorUserId, cancellationToken);
        }

        public async Task<IReadOnlyList<EmergencyAdmissionRequestSummary>> AmbilPerluTandaTanganAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var doctorId = await AmbilDokterAkunAsync(actorUserId, cancellationToken);
            if (!doctorId.HasValue)
                return [];

            var penugasanBerjalan = _dbContext.Set<EmgDoctorAssignment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.EffectiveTo == null && x.DoctorId == doctorId.Value);

            var data = await ProyeksiRingkasan(_dbContext.Set<EmgAdmissionRequest>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete
                        && x.Status == EmergencyAdmissionRequestStatus.AwaitingSignature
                        && penugasanBerjalan.Any(p => p.EmergencyVisitId == x.EmergencyVisitId))
                    .OrderByDescending(x => x.IsCito)
                    .ThenBy(x => x.SignatureRequestedAt))
                .ToListAsync(cancellationToken);

            foreach (var item in data)
                item.DisplayStatus = TampilanStatus(item.Status);

            return data;
        }

        public async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> KembalikanPermintaanAsync(
            Guid id,
            ReturnEmergencyAdmissionRequestRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var form = await MuatFormAsync(id, true, cancellationToken);
            if (form == null)
                return Gagal(StatusCodes.Status404NotFound, PesanFormTidakAda);

            if (!await AkunAdalahDpjpAktifAsync(actorUserId, form.EmergencyVisitId, cancellationToken))
                return Gagal(StatusCodes.Status403Forbidden, PesanBukanDpjpAktif);

            var catatan = Rapikan(request?.Note);
            if (catatan == null || catatan.Length > BatasAlasan)
                return Gagal(StatusCodes.Status400BadRequest, PesanCatatanKembali);

            if (request!.RowVersion != form.RowVersion)
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);

            if (form.Status != EmergencyAdmissionRequestStatus.AwaitingSignature)
                return Gagal(StatusCodes.Status409Conflict, PesanTidakMenungguTtd);

            var now = DateTime.UtcNow;
            form.Status = EmergencyAdmissionRequestStatus.Draft;
            TandaiBerubah(form, actorUserId, now);
            TambahKejadian(form, EmergencyAdmissionRequestEventType.Returned,
                EmergencyAdmissionRequestStatus.AwaitingSignature, EmergencyAdmissionRequestStatus.Draft,
                actorUserId, now, catatan);

            return await SimpanLaluBentukAsync(form, actorUserId, cancellationToken);
        }

        public async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> BatalkanPermintaanAsync(
            Guid id,
            CancelEmergencyAdmissionRequestRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var form = await MuatFormAsync(id, true, cancellationToken);
            if (form == null)
                return Gagal(StatusCodes.Status404NotFound, PesanFormTidakAda);

            var alasan = Rapikan(request?.Reason);
            if (alasan == null || alasan.Length > BatasAlasan)
                return Gagal(StatusCodes.Status400BadRequest, PesanAlasanBatal);

            if (request!.RowVersion != form.RowVersion)
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);

            if (form.Status == EmergencyAdmissionRequestStatus.Cancelled)
                return Gagal(StatusCodes.Status409Conflict, PesanSudahDibatalkan);

            if (form.Status == EmergencyAdmissionRequestStatus.Signed)
                return await BatalkanPermintaanTerkirimAsync(form, alasan, actorUserId, cancellationToken);

            var statusSebelum = form.Status;
            var now = DateTime.UtcNow;
            form.Status = EmergencyAdmissionRequestStatus.Cancelled;
            form.CancelledAt = now;
            form.CancelledByUserId = actorUserId;
            form.CancellationReason = alasan;
            TandaiBerubah(form, actorUserId, now);
            TambahKejadian(form, EmergencyAdmissionRequestEventType.Cancelled,
                statusSebelum, EmergencyAdmissionRequestStatus.Cancelled, actorUserId, now, alasan);

            return await SimpanLaluBentukAsync(form, actorUserId, cancellationToken);
        }

        public async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> TandatanganiPermintaanAsync(
            Guid id,
            EmergencyAdmissionRequestRowVersionRequest request,
            string? idempotencyKey,
            string? deviceInfo,
            string? ipAddress,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var form = await MuatFormAsync(id, false, cancellationToken);
            if (form == null)
                return Gagal(StatusCodes.Status404NotFound, PesanFormTidakAda);

            var kunci = Rapikan(idempotencyKey);
            if (AdalahPengulanganTandaTangan(form, kunci, actorUserId))
                return HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>.Ok(
                    await BentukResponsAsync(form, actorUserId, cancellationToken));

            var dokterAkun = await AmbilDokterAkunAsync(actorUserId, cancellationToken);
            var dpjp = await AmbilDpjpIgdAktifAsync(form.EmergencyVisitId, cancellationToken);
            if (!dokterAkun.HasValue || dpjp == null || dpjp.DoctorId != dokterAkun.Value)
                return Gagal(StatusCodes.Status403Forbidden, PesanBukanDpjpAktif);

            if (request == null || request.RowVersion != form.RowVersion)
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);

            if (form.Status != EmergencyAdmissionRequestStatus.AwaitingSignature)
                return Gagal(StatusCodes.Status409Conflict, PesanTidakMenungguTtd);

            if (!await TindakLanjutMasihTerkonfirmasiAsync(form.DispositionId, cancellationToken))
                return Gagal(StatusCodes.Status409Conflict, PesanTindakLanjutTidakBerlaku);

            if (!await TindakLanjutMasukAntreanAsync(form.DispositionId, cancellationToken))
                return Gagal(StatusCodes.Status409Conflict, PesanTujuanBukanAntrean);

            if (!form.RequestedServiceUnitId.HasValue
                || !form.RequestedPatientClassId.HasValue
                || !form.ProposedInpatientDoctorId.HasValue)
                return Gagal(StatusCodes.Status400BadRequest, PesanIsianBelumLengkap);

            var now = DateTime.UtcNow;
            EmergencyAdmissionRequestSnapshot salinan;
            try
            {
                salinan = await BentukSalinanBekuAsync(form, actorUserId, dpjp, now, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Salinan beku Form Permintaan Rawat Inap {FormId} gagal dibentuk.", form.Id);
                return Gagal(StatusCodes.Status503ServiceUnavailable, PesanDataPasienGagal);
            }

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await KunciKunjunganAsync(form.EmergencyVisitId, cancellationToken);

            var terkunci = await _dbContext.Set<EmgAdmissionRequest>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (terkunci == null)
                return Gagal(StatusCodes.Status404NotFound, PesanFormTidakAda);

            if (AdalahPengulanganTandaTangan(terkunci, kunci, actorUserId))
                return await BentukHasilTersimpanAsync(id, actorUserId, cancellationToken);

            var dpjpTerkunci = await AmbilDpjpIgdAktifAsync(terkunci.EmergencyVisitId, cancellationToken);
            if (dpjpTerkunci == null || dpjpTerkunci.DoctorId != dokterAkun.Value)
                return Gagal(StatusCodes.Status403Forbidden, PesanBukanDpjpAktif);

            if (terkunci.RowVersion != request.RowVersion)
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);

            if (terkunci.Status != EmergencyAdmissionRequestStatus.AwaitingSignature)
                return Gagal(StatusCodes.Status409Conflict, PesanTidakMenungguTtd);

            if (!await TindakLanjutMasihTerkonfirmasiAsync(terkunci.DispositionId, cancellationToken))
                return Gagal(StatusCodes.Status409Conflict, PesanTindakLanjutTidakBerlaku);

            terkunci.Status = EmergencyAdmissionRequestStatus.Signed;
            terkunci.SignedAt = now;
            terkunci.SignedByUserId = actorUserId;
            terkunci.SignedByDoctorId = dokterAkun.Value;
            terkunci.SignerNameSnapshot = salinan.Signer.DoctorName;
            terkunci.SignerPositionSnapshot = salinan.Signer.PositionName;
            terkunci.SignatureDeviceInfo = Potong(Rapikan(deviceInfo), 250);
            terkunci.SignatureIpAddress = Potong(Rapikan(ipAddress), 64);
            terkunci.SnapshotJson = JsonSerializer.Serialize(salinan, OpsiSalinanBeku);
            TandaiBerubah(terkunci, actorUserId, now);
            TambahKejadian(terkunci, EmergencyAdmissionRequestEventType.Signed,
                EmergencyAdmissionRequestStatus.AwaitingSignature, EmergencyAdmissionRequestStatus.Signed,
                actorUserId, now, null);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaksi.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);
            }

            return await BentukHasilTersimpanAsync(id, actorUserId, cancellationToken);
        }

        public async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestPrintResponse>> AmbilDataCetakPermintaanAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var form = await _dbContext.Set<EmgAdmissionRequest>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new { x.Id, x.EmergencyVisitId, x.Status, x.SnapshotJson })
                .FirstOrDefaultAsync(cancellationToken);
            if (form == null)
                return HasilPermintaanRawatInap<EmergencyAdmissionRequestPrintResponse>.Gagal(
                    StatusCodes.Status404NotFound, PesanFormTidakAda);

            var salinan = form.Status == EmergencyAdmissionRequestStatus.Signed
                ? BacaSalinanBeku(form.SnapshotJson)
                : null;
            if (salinan == null)
                return HasilPermintaanRawatInap<EmergencyAdmissionRequestPrintResponse>.Gagal(
                    StatusCodes.Status409Conflict, PesanCetakBelumTtd);

            return HasilPermintaanRawatInap<EmergencyAdmissionRequestPrintResponse>.Ok(new EmergencyAdmissionRequestPrintResponse
            {
                Id = form.Id,
                EmergencyVisitId = form.EmergencyVisitId,
                Origin = salinan.Origin,
                EmergencyVisitNumber = salinan.EmergencyVisitNumber,
                EncounterNumber = salinan.EncounterNumber,
                CapturedAt = salinan.CapturedAt,
                Patient = salinan.Patient,
                Allergies = salinan.Allergies,
                Payer = salinan.Payer,
                Diagnoses = salinan.Diagnoses,
                RequestedServiceUnitName = salinan.RequestedServiceUnitName,
                RequestedPatientClassName = salinan.RequestedPatientClassName,
                ProposedInpatientDoctorName = salinan.ProposedInpatientDoctorName,
                EstimatedLengthOfStayDays = salinan.EstimatedLengthOfStayDays,
                IsCito = salinan.IsCito,
                SpecialNeeds = KodeKebutuhanKhusus(
                    salinan.NeedsIntensiveCare,
                    salinan.NeedsIsolation,
                    salinan.NeedsOxygenSupport,
                    salinan.NeedsSpecialMonitoring),
                ClinicalIndication = salinan.ClinicalIndication,
                CarePlanInstruction = salinan.CarePlanInstruction,
                Signer = salinan.Signer,
                SignatureStatement = KalimatTandaTangan(salinan.Signer),
                PrintedAt = DateTime.UtcNow
            });
        }

        public Task<bool> ButuhFormPermintaanRawatInapAsync(Guid dispositionId, CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<EmgDisposition>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == dispositionId
                    && x.DispositionType != null
                    && x.DispositionType.RequiresAdmissionRequest,
                    cancellationToken);
        }

        public async Task<string> AlasanJalankanRawatInapDitolakAsync(
            Guid dispositionId,
            CancellationToken cancellationToken = default)
        {
            var adaFormTerkirim = await _dbContext.Set<EmgAdmissionRequest>()
                .AsNoTracking()
                .AnyAsync(x => x.DispositionId == dispositionId
                    && !x.IsDelete
                    && x.Status == EmergencyAdmissionRequestStatus.Signed,
                    cancellationToken);

            return adaFormTerkirim ? PesanJalankanLewatAdmisi : PesanJalankanTanpaForm;
        }

        public async Task<string?> BatalkanPermintaanKarenaTindakLanjutAsync(
            EmgDisposition disposition,
            string alasan,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await KunciKunjunganAsync(disposition.EmergencyVisitId, cancellationToken);
            await KunciTindakLanjutAsync(disposition.Id, cancellationToken);

            if ((await AmbilTindakLanjutSudahDiadmisiAsync([disposition.Id], cancellationToken)).Count > 0)
                return PesanAdmisiDibukaDariTindakLanjut;

            var daftarForm = await _dbContext.Set<EmgAdmissionRequest>()
                .Where(x => x.DispositionId == disposition.Id
                    && !x.IsDelete
                    && x.Status != EmergencyAdmissionRequestStatus.Cancelled)
                .ToListAsync(cancellationToken);

            var catatan = Potong(Rapikan(alasan), BatasAlasan)!;
            var now = DateTime.UtcNow;
            foreach (var form in daftarForm)
            {
                var statusSebelum = form.Status;
                form.Status = EmergencyAdmissionRequestStatus.Cancelled;
                form.CancelledAt = now;
                form.CancelledByUserId = actorUserId;
                form.CancellationReason = catatan;
                TandaiBerubah(form, actorUserId, now);
                TambahKejadian(form, EmergencyAdmissionRequestEventType.AutoCancelled,
                    statusSebelum, EmergencyAdmissionRequestStatus.Cancelled, actorUserId, now, catatan);
            }

            return null;
        }

        public async Task<IReadOnlyList<EmergencyInpatientDoctorOption>> AmbilPilihanDpjpRawatInapAsync(
            Guid? serviceUnitId,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var kata = Rapikan(search)?.ToLower();
            var dokterAktif = _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete);
            if (kata != null)
                dokterAktif = dokterAktif.Where(x => x.FullName.ToLower().Contains(kata));

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
            {
                var dokterBerjadwal = KueriDokterBerjadwalDiUnit(serviceUnitId.Value);
                if (await dokterBerjadwal.AnyAsync(cancellationToken))
                {
                    return await dokterAktif
                        .Where(x => dokterBerjadwal.Contains(x.Id))
                        .OrderBy(x => x.FullName)
                        .Take(BatasPilihanDokter)
                        .Select(x => new EmergencyInpatientDoctorOption
                        {
                            DoctorId = x.Id,
                            DoctorName = x.FullName,
                            SpecialistName = x.SpecialistName,
                            IsScheduledInUnit = true
                        })
                        .ToListAsync(cancellationToken);
                }
            }

            return await dokterAktif
                .OrderBy(x => x.FullName)
                .Take(BatasPilihanDokter)
                .Select(x => new EmergencyInpatientDoctorOption
                {
                    DoctorId = x.Id,
                    DoctorName = x.FullName,
                    SpecialistName = x.SpecialistName,
                    IsScheduledInUnit = false
                })
                .ToListAsync(cancellationToken);
        }

        public static async Task<EmergencyAdmissionRequestSnapshot?> BacaRingkasanPermintaanRawatInapAsync(
            ApplicationDbContext dbContext,
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var json = await dbContext.Set<EmgAdmissionRequest>()
                .AsNoTracking()
                .Where(x => x.Id == requestId
                    && !x.IsDelete
                    && x.Status == EmergencyAdmissionRequestStatus.Signed)
                .Select(x => x.SnapshotJson)
                .FirstOrDefaultAsync(cancellationToken);

            return BacaSalinanBeku(json);
        }

        private static IQueryable<TrxPatientDiagnosis> KueriDiagnosisTerhitung(
            IQueryable<TrxPatientDiagnosis> sumber,
            Guid? encounterId)
        {
            return sumber.Where(x =>
                x.EncounterId == encounterId
                && !x.IsDelete
                && !x.IsCancel
                && (x.IsActive || x.DiagnosisStatus == PatientDiagnosisStatus.Resolved)
                && (x.DiagnosisStatus == PatientDiagnosisStatus.Active || x.DiagnosisStatus == PatientDiagnosisStatus.Resolved)
                && (x.DiagnosisType == PatientDiagnosisType.Primary
                    || x.DiagnosisType == PatientDiagnosisType.Secondary
                    || x.DiagnosisType == PatientDiagnosisType.WorkingDiagnosis
                    || x.DiagnosisType == PatientDiagnosisType.FinalDiagnosis)
                && (x.DiagnosisMasterType == "ICD10"
                    || x.DiagnosisMasterType == "ICD-10"
                    || (x.IcdVersion != null && x.IcdVersion.Contains("ICD-10"))
                    || (x.Diagnosis != null && (x.Diagnosis.IcdVersion.Contains("ICD-10") || x.Diagnosis.DiagnosisType == "ICD10")))
                && x.DiagnosisMasterType != "ICD9"
                && (!x.ConsultationId.HasValue
                    || (x.Consultation != null
                        && !x.Consultation.IsDelete
                        && !x.Consultation.IsCancel
                        && x.Consultation.ConsultationStatus != DoctorConsultationStatus.Cancelled)));
        }

        private static HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse> Gagal(int statusCode, string penolakan)
            => HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>.Gagal(statusCode, penolakan);

        private static bool AdalahBentrokFormAktif(DbUpdateException exception)
            => AdalahBentrokIndeks(exception, "UX_EmgAdmissionRequest_Visit_Open");

        private static bool AdalahBentrokIndeks(DbUpdateException exception, string namaIndeks)
            => exception.InnerException is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation
                && string.Equals(postgres.ConstraintName, namaIndeks, StringComparison.Ordinal);

        private static bool AdalahPengulanganTandaTangan(EmgAdmissionRequest form, string? kunci, Guid actorUserId)
            => kunci != null
                && form.Status == EmergencyAdmissionRequestStatus.Signed
                && form.SignedByUserId == actorUserId;

        private Task<bool> TindakLanjutMasukAntreanAsync(Guid dispositionId, CancellationToken cancellationToken)
        {
            return _dbContext.Set<EmgDisposition>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == dispositionId
                    && x.DispositionType != null
                    && x.DispositionType.IsActive
                    && !x.DispositionType.IsDelete
                    && x.DispositionType.RequiresAdmissionRequest
                    && x.DestinationServiceUnit != null
                    && !x.DestinationServiceUnit.IsDelete
                    && x.DestinationServiceUnit.ServiceUnitType == ServiceUnitType.Inpatient,
                    cancellationToken);
        }

        private async Task<HashSet<Guid>> AmbilTindakLanjutSudahDiadmisiAsync(
            IReadOnlyCollection<Guid> dispositionIds,
            CancellationToken cancellationToken)
        {
            if (dispositionIds.Count == 0)
                return [];

            var hasil = await _dbContext.RegPatientEncounters
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.AdmissionTransferId.HasValue
                    && dispositionIds.Contains(x.AdmissionTransferId.Value)
                    && x.AdmissionTransferSourceType == SumberTransferIgd)
                .Select(x => x.AdmissionTransferId ?? Guid.Empty)
                .ToListAsync(cancellationToken);

            return hasil.ToHashSet();
        }

        private Task<bool> TindakLanjutMasihTerkonfirmasiAsync(Guid dispositionId, CancellationToken cancellationToken)
        {
            return _dbContext.Set<EmgDisposition>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == dispositionId
                    && !x.IsDelete
                    && x.DispositionStatus == EmergencyDispositionStatus.Confirmed,
                    cancellationToken);
        }

        private async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> BentukHasilTersimpanAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var tersimpan = await MuatFormAsync(id, false, cancellationToken);
            if (tersimpan == null)
                return Gagal(StatusCodes.Status404NotFound, PesanFormTidakAda);

            return HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>.Ok(
                await BentukResponsAsync(tersimpan, actorUserId, cancellationToken));
        }

        private async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> BatalkanPermintaanTerkirimAsync(
            EmgAdmissionRequest form,
            string alasan,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await KunciKunjunganAsync(form.EmergencyVisitId, cancellationToken);
            await KunciTindakLanjutAsync(form.DispositionId, cancellationToken);

            if ((await AmbilTindakLanjutSudahDiadmisiAsync([form.DispositionId], cancellationToken)).Count > 0)
                return Gagal(StatusCodes.Status409Conflict, PesanAdmisiSudahDibuka);

            var now = DateTime.UtcNow;
            form.Status = EmergencyAdmissionRequestStatus.Cancelled;
            form.CancelledAt = now;
            form.CancelledByUserId = actorUserId;
            form.CancellationReason = alasan;
            TandaiBerubah(form, actorUserId, now);
            TambahKejadian(form, EmergencyAdmissionRequestEventType.CancelledAfterSend,
                EmergencyAdmissionRequestStatus.Signed, EmergencyAdmissionRequestStatus.Cancelled,
                actorUserId, now, alasan);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaksi.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);
            }

            return await BentukHasilTersimpanAsync(form.Id, actorUserId, cancellationToken);
        }

        private async Task<EmergencyAdmissionRequestSnapshot> BentukSalinanBekuAsync(
            EmgAdmissionRequest form,
            Guid actorUserId,
            DpjpIgdAktif dpjp,
            DateTime waktu,
            CancellationToken cancellationToken)
        {
            if (form.Patient == null)
                throw new InvalidOperationException("Data pasien Form Permintaan Rawat Inap tidak terbaca.");

            var penanda = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => x.Id == actorUserId)
                .Select(x => new
                {
                    x.DisplayName,
                    PositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Akun penanda tangan tidak terbaca.");

            return new EmergencyAdmissionRequestSnapshot
            {
                CapturedAt = waktu,
                Origin = "IGD",
                EmergencyVisitNumber = form.EmergencyVisit?.EmergencyVisitNumber,
                EncounterNumber = form.Encounter?.EncounterNumber,
                Patient = BentukPasien(form.Patient, form.PatientId, waktu),
                Allergies = await BacaAlergiAsync(form.PatientId, cancellationToken),
                Payer = await BacaPenjaminAsync(form.EncounterId, cancellationToken),
                Diagnoses = await BacaDiagnosisAsync(form.EncounterId, cancellationToken),
                RequestedServiceUnitName = form.RequestedServiceUnit?.ServiceUnitName,
                RequestedPatientClassName = form.RequestedPatientClass?.PatientClassName,
                ProposedInpatientDoctorName = form.ProposedInpatientDoctor?.FullName,
                EstimatedLengthOfStayDays = form.EstimatedLengthOfStayDays,
                IsCito = form.IsCito,
                NeedsIntensiveCare = form.NeedsIntensiveCare,
                NeedsIsolation = form.NeedsIsolation,
                NeedsOxygenSupport = form.NeedsOxygenSupport,
                NeedsSpecialMonitoring = form.NeedsSpecialMonitoring,
                ClinicalIndication = form.ClinicalIndication,
                CarePlanInstruction = form.CarePlanInstruction,
                Signer = new EmergencyAdmissionRequestSignerInfo
                {
                    DoctorName = Potong(Rapikan(penanda.DisplayName) ?? Rapikan(dpjp.DoctorName), 200),
                    PositionName = Potong(Rapikan(penanda.PositionName), 200),
                    SignedAt = waktu
                }
            };
        }

        private static string KalimatTandaTangan(EmergencyAdmissionRequestSignerInfo penanda)
        {
            var kalimat = $"Ditandatangani secara elektronik oleh {penanda.DoctorName}";
            if (!penanda.SignedAt.HasValue)
                return kalimat;

            var waktuUtc = penanda.SignedAt.Value.Kind == DateTimeKind.Local
                ? penanda.SignedAt.Value.ToUniversalTime()
                : DateTime.SpecifyKind(penanda.SignedAt.Value, DateTimeKind.Utc);
            var lokal = TimeZoneInfo.ConvertTimeFromUtc(waktuUtc, ZonaWaktuCetak);
            return $"{kalimat}, {lokal.Day} {NamaBulanCetak[lokal.Month - 1]} {lokal.Year} {lokal:HH}.{lokal:mm} WIB";
        }

        private static TimeZoneInfo TentukanZonaWaktuCetak()
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Jakarta", out var zona))
                return zona;

            if (TimeZoneInfo.TryFindSystemTimeZoneById("SE Asia Standard Time", out zona))
                return zona;

            return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        }

        private async Task<HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>> SimpanLaluBentukAsync(
            EmgAdmissionRequest form,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Gagal(StatusCodes.Status409Conflict, PesanVersiBasi);
            }

            var tersimpan = await MuatFormAsync(form.Id, false, cancellationToken);
            return HasilPermintaanRawatInap<EmergencyAdmissionRequestResponse>.Ok(
                await BentukResponsAsync(tersimpan!, actorUserId, cancellationToken));
        }

        private async Task<EmgAdmissionRequest?> MuatFormAsync(Guid id, bool lacak, CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<EmgAdmissionRequest>()
                .Include(x => x.EmergencyVisit)
                .Include(x => x.Encounter)
                .Include(x => x.Patient)
                .Include(x => x.RequestedServiceUnit)
                .Include(x => x.RequestedPatientClass)
                .Include(x => x.ProposedInpatientDoctor)
                .Include(x => x.Events)
                .Where(x => x.Id == id && !x.IsDelete);

            return lacak
                ? await query.FirstOrDefaultAsync(cancellationToken)
                : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<EmergencyAdmissionRequestResponse> BentukResponsAsync(
            EmgAdmissionRequest form,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var salinan = BacaSalinanBeku(form.SnapshotJson);
            var dpjp = await AmbilDpjpIgdAktifAsync(form.EmergencyVisitId, cancellationToken);
            var dokterAkun = await AmbilDokterAkunAsync(actorUserId, cancellationToken);

            var sudahDiadmisi = form.Status == EmergencyAdmissionRequestStatus.Signed
                && (await AmbilTindakLanjutSudahDiadmisiAsync([form.DispositionId], cancellationToken)).Count > 0;

            var response = new EmergencyAdmissionRequestResponse
            {
                Id = form.Id,
                EmergencyVisitId = form.EmergencyVisitId,
                EmergencyVisitNumber = form.EmergencyVisit?.EmergencyVisitNumber,
                DispositionId = form.DispositionId,
                EncounterId = form.EncounterId,
                EncounterNumber = form.Encounter?.EncounterNumber,
                Status = form.Status,
                DisplayStatus = TampilanStatus(form.Status, sudahDiadmisi),
                RequestedServiceUnit = form.RequestedServiceUnit == null
                    ? null
                    : new EmergencyAdmissionRequestReference { Id = form.RequestedServiceUnit.Id, Name = form.RequestedServiceUnit.ServiceUnitName },
                RequestedPatientClass = form.RequestedPatientClass == null
                    ? null
                    : new EmergencyAdmissionRequestReference { Id = form.RequestedPatientClass.Id, Name = form.RequestedPatientClass.PatientClassName },
                ProposedInpatientDoctor = form.ProposedInpatientDoctor == null
                    ? null
                    : new EmergencyAdmissionRequestReference { Id = form.ProposedInpatientDoctor.Id, Name = form.ProposedInpatientDoctor.FullName },
                EstimatedLengthOfStayDays = form.EstimatedLengthOfStayDays,
                IsCito = form.IsCito,
                NeedsIntensiveCare = form.NeedsIntensiveCare,
                NeedsIsolation = form.NeedsIsolation,
                NeedsOxygenSupport = form.NeedsOxygenSupport,
                NeedsSpecialMonitoring = form.NeedsSpecialMonitoring,
                SpecialNeeds = KodeKebutuhanKhusus(form),
                ClinicalIndication = form.ClinicalIndication,
                CarePlanInstruction = form.CarePlanInstruction,
                ActiveIgdDoctor = dpjp == null
                    ? null
                    : new EmergencyAdmissionRequestDoctorInfo { DoctorId = dpjp.DoctorId, DoctorName = dpjp.DoctorName, HasAccount = dpjp.HasAccount },
                CanSign = form.Status == EmergencyAdmissionRequestStatus.AwaitingSignature
                    && dpjp != null
                    && dokterAkun.HasValue
                    && dokterAkun.Value == dpjp.DoctorId,
                SignatureRequestedAt = form.SignatureRequestedAt,
                Signer = form.SignedAt.HasValue
                    ? new EmergencyAdmissionRequestSignerInfo
                    {
                        DoctorName = form.SignerNameSnapshot,
                        PositionName = form.SignerPositionSnapshot,
                        SignedAt = form.SignedAt
                    }
                    : null,
                CancelledAt = form.CancelledAt,
                CancellationReason = form.CancellationReason,
                CreateDateTime = form.CreateDateTime,
                RowVersion = form.RowVersion
            };

            if (salinan != null)
            {
                response.Patient = salinan.Patient;
                response.Allergies = salinan.Allergies;
                response.Payer = salinan.Payer;
                response.Diagnoses = salinan.Diagnoses;
            }
            else
            {
                response.Patient = BentukPasien(form.Patient, form.PatientId, DateTime.UtcNow);
                response.Allergies = await BacaAlergiAsync(form.PatientId, cancellationToken);
                response.Payer = await BacaPenjaminAsync(form.EncounterId, cancellationToken);
                response.Diagnoses = await BacaDiagnosisAsync(form.EncounterId, cancellationToken);
            }

            response.Events = await BentukKejadianAsync(form.Events, cancellationToken);
            return response;
        }

        private static IQueryable<EmergencyAdmissionRequestSummary> ProyeksiRingkasan(IQueryable<EmgAdmissionRequest> query)
        {
            return query.Select(x => new EmergencyAdmissionRequestSummary
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                EmergencyVisitNumber = x.EmergencyVisit != null ? x.EmergencyVisit.EmergencyVisitNumber : null,
                DispositionId = x.DispositionId,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : null,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                Status = x.Status,
                RequestedServiceUnitName = x.RequestedServiceUnit != null ? x.RequestedServiceUnit.ServiceUnitName : null,
                RequestedPatientClassName = x.RequestedPatientClass != null ? x.RequestedPatientClass.PatientClassName : null,
                IsCito = x.IsCito,
                SignatureRequestedAt = x.SignatureRequestedAt,
                CancellationReason = x.CancellationReason,
                CreateDateTime = x.CreateDateTime,
                RowVersion = x.RowVersion
            });
        }

        private static List<string> KodeKebutuhanKhusus(EmgAdmissionRequest form)
            => KodeKebutuhanKhusus(
                form.NeedsIntensiveCare,
                form.NeedsIsolation,
                form.NeedsOxygenSupport,
                form.NeedsSpecialMonitoring);

        private static List<string> KodeKebutuhanKhusus(
            bool intensif,
            bool isolasi,
            bool oksigen,
            bool pemantauan)
        {
            var kode = new List<string>();
            if (intensif)
                kode.Add("IntensiveCare");
            if (isolasi)
                kode.Add("Isolation");
            if (oksigen)
                kode.Add("OxygenSupport");
            if (pemantauan)
                kode.Add("SpecialMonitoring");
            return kode;
        }

        private static EmergencyAdmissionRequestDisplayStatus TampilanStatus(
            EmergencyAdmissionRequestStatus status,
            bool sudahDiadmisi = false)
        {
            return status switch
            {
                EmergencyAdmissionRequestStatus.AwaitingSignature => EmergencyAdmissionRequestDisplayStatus.AwaitingSignature,
                EmergencyAdmissionRequestStatus.Signed => sudahDiadmisi
                    ? EmergencyAdmissionRequestDisplayStatus.Completed
                    : EmergencyAdmissionRequestDisplayStatus.Sent,
                EmergencyAdmissionRequestStatus.Cancelled => EmergencyAdmissionRequestDisplayStatus.Cancelled,
                _ => EmergencyAdmissionRequestDisplayStatus.Draft
            };
        }

        private async Task<string?> ValidasiIsianAsync(
            Guid? serviceUnitId,
            Guid? patientClassId,
            Guid? doctorId,
            int lamaRawat,
            string? indikasi,
            string? rencana,
            CancellationToken cancellationToken)
        {
            if (lamaRawat < 1 || lamaRawat > 90)
                return PesanLamaRawat;

            if (indikasi != null && indikasi.Length > BatasTeksKlinis)
                return PesanIndikasiPanjang;

            if (rencana != null && rencana.Length > BatasTeksKlinis)
                return PesanRencanaPanjang;

            var kelasId = KosongkanGuid(patientClassId);
            if (kelasId.HasValue)
            {
                var kelasBerlaku = await _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == kelasId.Value && x.IsActive && !x.IsDelete && x.IsForInpatient,
                        cancellationToken);
                if (!kelasBerlaku)
                    return PesanKelasTidakBerlaku;
            }

            var unitId = KosongkanGuid(serviceUnitId);
            if (unitId.HasValue)
            {
                var unitAktif = await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == unitId.Value && x.IsActive && !x.IsDelete, cancellationToken);
                if (!unitAktif)
                    return PesanUnitTidakAktif;
            }

            var dokterId = KosongkanGuid(doctorId);
            if (dokterId.HasValue)
            {
                var dokterAktif = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == dokterId.Value && x.IsActive && !x.IsDelete, cancellationToken);
                if (!dokterAktif)
                    return PesanDokterTidakAktif;

                if (unitId.HasValue)
                {
                    var dokterBerjadwal = KueriDokterBerjadwalDiUnit(unitId.Value);
                    if (await dokterBerjadwal.AnyAsync(cancellationToken)
                        && !await dokterBerjadwal.AnyAsync(x => x == dokterId.Value, cancellationToken))
                        return PesanDokterTidakBerjadwal;
                }
            }

            return null;
        }

        private IQueryable<Guid> KueriDokterBerjadwalDiUnit(Guid serviceUnitId)
        {
            return _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => x.ServiceUnitId == serviceUnitId
                    && x.IsActive
                    && !x.IsDelete
                    && x.ScheduleStatus == DoctorScheduleStatus.Active
                    && _dbContext.Set<MstDoctor>().Any(d => d.Id == x.DoctorId && d.IsActive && !d.IsDelete))
                .Select(x => x.DoctorId)
                .Distinct();
        }

        private async Task<DpjpIgdAktif?> AmbilDpjpIgdAktifAsync(Guid emergencyVisitId, CancellationToken cancellationToken)
        {
            var penugasan = await _dbContext.Set<EmgDoctorAssignment>()
                .AsNoTracking()
                .Where(x => x.EmergencyVisitId == emergencyVisitId && !x.IsDelete && x.EffectiveTo == null)
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(x => new { x.DoctorId, DoctorName = x.Doctor != null ? x.Doctor.FullName : null })
                .FirstOrDefaultAsync(cancellationToken);
            if (penugasan == null)
                return null;

            var punyaAkun = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .AnyAsync(x => x.DoctorId == penugasan.DoctorId && x.IsActive, cancellationToken);

            return new DpjpIgdAktif(penugasan.DoctorId, penugasan.DoctorName, punyaAkun);
        }

        private async Task<Guid?> AmbilDokterAkunAsync(Guid actorUserId, CancellationToken cancellationToken)
        {
            if (actorUserId == Guid.Empty)
                return null;

            return await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => x.Id == actorUserId && x.IsActive)
                .Select(x => x.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<bool> AkunAdalahDpjpAktifAsync(Guid actorUserId, Guid emergencyVisitId, CancellationToken cancellationToken)
        {
            var dokterAkun = await AmbilDokterAkunAsync(actorUserId, cancellationToken);
            if (!dokterAkun.HasValue)
                return false;

            var dpjp = await AmbilDpjpIgdAktifAsync(emergencyVisitId, cancellationToken);
            return dpjp != null && dpjp.DoctorId == dokterAkun.Value;
        }

        private async Task<EmergencyAdmissionRequestAllergyInfo> BacaAlergiAsync(Guid patientId, CancellationToken cancellationToken)
        {
            var alergi = await _patientAllergyQueryService.GetActiveAlertsAsync(patientId, cancellationToken);
            return new EmergencyAdmissionRequestAllergyInfo
            {
                State = alergi.Count > 0 ? EmergencyAdmissionAllergyState.Recorded : EmergencyAdmissionAllergyState.NotRecorded,
                Items = alergi.Select(x => new EmergencyAdmissionRequestAllergyItem
                {
                    AllergenName = x.AllergenName,
                    Category = x.AllergyCategory,
                    Severity = x.Severity,
                    Reaction = x.ReactionDescription ?? x.ReactionType
                }).ToList()
            };
        }

        private async Task<EmergencyAdmissionRequestPayerInfo> BacaPenjaminAsync(Guid encounterId, CancellationToken cancellationToken)
        {
            var konteks = await _encounterInsuranceService.GetContextAsync(encounterId, null, cancellationToken);
            if (!konteks.IsValid)
                return new EmergencyAdmissionRequestPayerInfo { ErrorMessage = konteks.ErrorMessage };

            string? hakKelas = null;
            if (konteks.PatientInsuranceId.HasValue)
            {
                hakKelas = await _dbContext.Set<MstPatientInsurance>()
                    .AsNoTracking()
                    .Where(x => x.Id == konteks.PatientInsuranceId.Value)
                    .Select(x => x.ClassName)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return new EmergencyAdmissionRequestPayerInfo
            {
                PaymentTypeName = konteks.PaymentTypeName,
                PayerName = konteks.InsuranceProviderName ?? konteks.CompanyGuarantorName ?? konteks.PaymentSourceName,
                ClassEntitlementText = hakKelas
            };
        }

        private async Task<List<EmergencyAdmissionRequestDiagnosisItem>> BacaDiagnosisAsync(Guid encounterId, CancellationToken cancellationToken)
        {
            return await KueriDiagnosisTerhitung(_dbContext.Set<TrxPatientDiagnosis>().AsNoTracking(), encounterId)
                .OrderByDescending(x => x.IsPrimary || x.DiagnosisType == PatientDiagnosisType.Primary)
                .ThenByDescending(x => x.DiagnosisDateTime)
                .Select(x => new EmergencyAdmissionRequestDiagnosisItem
                {
                    Code = x.DiagnosisCode,
                    Name = x.DiagnosisName,
                    Type = x.DiagnosisType,
                    IsPrimary = x.IsPrimary || x.DiagnosisType == PatientDiagnosisType.Primary
                })
                .ToListAsync(cancellationToken);
        }

        private async Task<List<EmergencyAdmissionRequestEventItem>> BentukKejadianAsync(
            IEnumerable<EmgAdmissionRequestEvent> kejadian,
            CancellationToken cancellationToken)
        {
            var urut = kejadian.Where(x => !x.IsDelete).OrderBy(x => x.OccurredAt).ToList();
            var pelakuIds = urut.Select(x => x.ActorUserId).Distinct().ToList();
            var namaPelaku = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => pelakuIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName })
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            return urut.Select(x => new EmergencyAdmissionRequestEventItem
            {
                EventType = x.EventType,
                FromStatus = x.FromStatus,
                ToStatus = x.ToStatus,
                ActorUserId = x.ActorUserId,
                ActorName = namaPelaku.TryGetValue(x.ActorUserId, out var nama) ? nama : null,
                OccurredAt = x.OccurredAt,
                Note = x.Note
            }).ToList();
        }

        private static EmergencyAdmissionRequestPatientInfo BentukPasien(MstPatient? pasien, Guid patientId, DateTime sekarang)
        {
            return new EmergencyAdmissionRequestPatientInfo
            {
                PatientId = patientId,
                FullName = pasien?.FullName ?? string.Empty,
                MedicalRecordNumber = pasien?.MedicalRecordNumber,
                Gender = pasien?.Gender switch
                {
                    Gender.Male => "Laki-laki",
                    Gender.Female => "Perempuan",
                    _ => "Tidak diketahui"
                },
                BirthDate = pasien?.BirthDate,
                AgeText = TeksUmur(pasien?.BirthDate, sekarang)
            };
        }

        private static string? TeksUmur(DateTime? tanggalLahir, DateTime sekarang)
        {
            if (!tanggalLahir.HasValue)
                return null;

            var lahir = tanggalLahir.Value.Date;
            var hariIni = sekarang.Date;
            if (lahir > hariIni)
                return null;

            var tahun = hariIni.Year - lahir.Year;
            if (lahir.AddYears(tahun) > hariIni)
                tahun--;
            if (tahun >= 1)
                return $"{tahun} tahun";

            var bulan = (hariIni.Year - lahir.Year) * 12 + hariIni.Month - lahir.Month;
            if (lahir.AddMonths(bulan) > hariIni)
                bulan--;
            if (bulan >= 1)
                return $"{bulan} bulan";

            return $"{(hariIni - lahir).Days} hari";
        }

        private static EmergencyAdmissionRequestSnapshot? BacaSalinanBeku(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<EmergencyAdmissionRequestSnapshot>(json, OpsiSalinanBeku);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private void TambahKejadian(
            EmgAdmissionRequest form,
            EmergencyAdmissionRequestEventType jenis,
            EmergencyAdmissionRequestStatus? dari,
            EmergencyAdmissionRequestStatus ke,
            Guid actorUserId,
            DateTime waktu,
            string? catatan)
        {
            _dbContext.Set<EmgAdmissionRequestEvent>().Add(new EmgAdmissionRequestEvent
            {
                AdmissionRequestId = form.Id,
                EventType = jenis,
                FromStatus = dari,
                ToStatus = ke,
                ActorUserId = actorUserId,
                OccurredAt = waktu,
                Note = catatan,
                CreateBy = actorUserId,
                CreateDateTime = waktu
            });
        }

        private static void TandaiBerubah(EmgAdmissionRequest form, Guid actorUserId, DateTime waktu)
        {
            form.RowVersion = Guid.NewGuid();
            form.UpdateBy = actorUserId;
            form.UpdateDateTime = waktu;
        }

        private async Task KunciKunjunganAsync(Guid emergencyVisitId, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            var kunci = $"EMG_ADMISSION_REQUEST_{emergencyVisitId}";
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtext({kunci})::bigint)",
                cancellationToken);
        }

        private async Task KunciTindakLanjutAsync(Guid dispositionId, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM public.\"EmgDisposition\" WHERE \"Id\" = {dispositionId} FOR UPDATE",
                cancellationToken);
        }

        private static string? Rapikan(string? teks)
        {
            var hasil = teks?.Trim();
            return string.IsNullOrEmpty(hasil) ? null : hasil;
        }

        private static string? Potong(string? teks, int batas)
            => teks == null || teks.Length <= batas ? teks : teks[..batas];

        private static Guid? KosongkanGuid(Guid? nilai)
            => nilai.HasValue && nilai.Value != Guid.Empty ? nilai : null;
    }
}
