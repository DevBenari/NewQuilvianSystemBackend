using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Repositories;
using static QuilvianSystemBackend.Tests.HealthServices.EmergencyInstallationManagement.EmergencyControllerTestWorld;

namespace QuilvianSystemBackend.Tests.HealthServices.EmergencyInstallationManagement;

/// <summary>
/// Membuktikan bahwa lima titik tulis <c>VisitStatus</c> yang tersisa — tiga pada observasi,
/// satu pada resusitasi, satu pada tindak lanjut — kini melewati penjaga
/// <c>TryApplyVisitStatus</c>, dan bahwa penyelesaian kunjungan ikut melewatinya.
/// </summary>
/// <remarks>
/// Task <c>BE-IGD-021</c> dan <c>BE-IGD-022</c>. Requirement <c>FR-IGD-015</c>, keputusan
/// <c>IGD-CONF-05</c>, uji <c>AT-IGD-089</c>.
///
/// <para>
/// Kontrak: state-transition-matrix bagian 1, hash
/// <c>a41efd8d9adc87e1cf1eec2a9397b3521fdc0ebf935ccf0a19a5aa975b6c7c75</c> — disetujui
/// <c>IGD-DEC-093</c>; validation-matrix bagian 6.
/// </para>
///
/// <para>
/// <b>Pelajaran <c>BE-IGD-016</c> yang diterapkan di sini:</b> setiap jalur diuji dari
/// controller-nya sendiri, bukan hanya lewat penjaga. Sebuah status yang dapat berubah dari
/// lebih dari satu endpoint pernah membuat perbaikan pada satu jalur terlihat selesai
/// sementara jalur kedua masih bocor.
/// </para>
/// </remarks>
public class EmergencyVisitStatusWritePathTests
{
    private static readonly Guid Pelaku = Guid.NewGuid();

    private static async Task<EmgObservation> SimpanObservasiAsync(
        ApplicationDbContext context,
        Guid visitId,
        EmergencyObservationStatus status)
    {
        var observation = new EmgObservation
        {
            Id = Guid.NewGuid(),
            EmergencyVisitId = visitId,
            ObservationNumber = $"OBS{Guid.NewGuid():N}"[..12],
            ObservationStatus = status,
            IsDelete = false,
        };

        context.Set<EmgObservation>().Add(observation);
        await context.SaveChangesAsync();
        return observation;
    }

    private static async Task<EmgResuscitation> SimpanResusitasiAsync(
        ApplicationDbContext context,
        Guid visitId,
        EmergencyResuscitationStatus status)
    {
        var resuscitation = new EmgResuscitation
        {
            Id = Guid.NewGuid(),
            EmergencyVisitId = visitId,
            ResuscitationNumber = $"RES{Guid.NewGuid():N}"[..12],
            ResuscitationStatus = status,
            IsDelete = false,
        };

        context.Set<EmgResuscitation>().Add(resuscitation);
        await context.SaveChangesAsync();
        return resuscitation;
    }

    private static async Task<EmgDisposition> SimpanTindakLanjutAsync(
        ApplicationDbContext context,
        Guid visitId,
        EmergencyDispositionStatus status)
    {
        var disposition = new EmgDisposition
        {
            Id = Guid.NewGuid(),
            EmergencyVisitId = visitId,
            DispositionTypeId = Guid.NewGuid(),
            DispositionStatus = status,
            IsDelete = false,
        };

        context.Set<EmgDisposition>().Add(disposition);
        await context.SaveChangesAsync();
        return disposition;
    }

    private static async Task<EmergencyVisitStatus> BacaStatusKunjunganAsync(
        ApplicationDbContext context,
        Guid visitId)
        => (await context.Set<EmgVisit>().AsNoTracking().FirstAsync(x => x.Id == visitId))
            .VisitStatus;

    // =================================================================================
    // BE-IGD-021 — tiga titik tulis pada observasi
    // =================================================================================

    [Fact]
    public async Task Observasi_MenjadiActive_MemindahkanKunjunganKeUnderObservation()
    {
        using var context = BuatContext("igd-obs");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Triaged);

        // Observasi dimulai Active. Tidak ada status lain yang boleh berpindah ke Active
        // menurut EmergencyObservationService.CanTransition, sehingga inilah satu-satunya
        // jalan yang benar-benar dapat ditempuh menuju percabangan UnderObservation.
        var observation = await SimpanObservasiAsync(context, visit.Id, EmergencyObservationStatus.Active);
        var controller = BuatObservationController(context, Pelaku);

        var result = await controller.UpdateObservationStatus(
            observation.Id,
            new UpdateEmergencyObservationObservationStatusRequest
            {
                ObservationStatus = EmergencyObservationStatus.Active
            });

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));
        Assert.Equal(EmergencyVisitStatus.UnderObservation, await BacaStatusKunjunganAsync(context, visit.Id));
    }

    /// <summary>
    /// Acceptance butir 3: observasi yang dieskalasi tetap boleh mengembalikan kunjungan ke
    /// <c>InTreatment</c>. Itu transisi yang sah menurut kontrak, dan penjaga tidak boleh
    /// menutupnya hanya karena arahnya terlihat mundur.
    /// </summary>
    [Fact]
    public async Task Observasi_Dieskalasi_MengembalikanKunjunganKeInTreatment()
    {
        using var context = BuatContext("igd-obs");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.UnderObservation);
        var observation = await SimpanObservasiAsync(context, visit.Id, EmergencyObservationStatus.Active);
        var controller = BuatObservationController(context, Pelaku);

        var result = await controller.UpdateObservationStatus(
            observation.Id,
            new UpdateEmergencyObservationObservationStatusRequest
            {
                ObservationStatus = EmergencyObservationStatus.Escalated,
                Notes = "Kondisi memburuk, kembali ke penanganan aktif."
            });

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));
        Assert.Equal(EmergencyVisitStatus.InTreatment, await BacaStatusKunjunganAsync(context, visit.Id));

        var tersimpan = await context.Set<EmgObservation>().AsNoTracking()
            .FirstAsync(x => x.Id == observation.Id);
        Assert.Equal("Kondisi memburuk, kembali ke penanganan aktif.", tersimpan.EscalationReason);
    }

    [Fact]
    public async Task Observasi_Selesai_MemindahkanKunjunganKeAwaitingDisposition()
    {
        using var context = BuatContext("igd-obs");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.UnderObservation);
        var observation = await SimpanObservasiAsync(context, visit.Id, EmergencyObservationStatus.Active);
        var controller = BuatObservationController(context, Pelaku);

        var result = await controller.UpdateObservationStatus(
            observation.Id,
            new UpdateEmergencyObservationObservationStatusRequest
            {
                ObservationStatus = EmergencyObservationStatus.Completed
            });

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));
        Assert.Equal(EmergencyVisitStatus.AwaitingDisposition, await BacaStatusKunjunganAsync(context, visit.Id));
    }

    /// <summary>
    /// Inti <c>BE-IGD-021</c>. Sebelum perbaikan, menyelesaikan observasi pada kunjungan yang
    /// sudah ditutup akan <b>membuka kembali</b> kunjungan itu menjadi
    /// <c>AwaitingDisposition</c> tanpa satu pun pemeriksaan.
    /// </summary>
    [Theory]
    [InlineData(EmergencyVisitStatus.Completed)]
    [InlineData(EmergencyVisitStatus.Cancelled)]
    [InlineData(EmergencyVisitStatus.Disposed)]
    public async Task Observasi_PadaKunjunganTertutup_Ditolak409DanStatusTidakBergerak(
        EmergencyVisitStatus statusKunjungan)
    {
        using var context = BuatContext("igd-obs");
        var visit = await SimpanKunjunganAsync(context, statusKunjungan);
        var observation = await SimpanObservasiAsync(context, visit.Id, EmergencyObservationStatus.Active);
        var controller = BuatObservationController(context, Pelaku);

        var result = await controller.UpdateObservationStatus(
            observation.Id,
            new UpdateEmergencyObservationObservationStatusRequest
            {
                ObservationStatus = EmergencyObservationStatus.Completed
            });

        Assert.Equal(StatusCodes.Status409Conflict, KodeStatus((IActionResult)result));
        Assert.Equal(statusKunjungan, await BacaStatusKunjunganAsync(context, visit.Id));

        // Penolakan tidak boleh meninggalkan observasi yang terlanjur berubah status.
        var tersimpan = await context.Set<EmgObservation>().AsNoTracking()
            .FirstAsync(x => x.Id == observation.Id);
        Assert.Equal(EmergencyObservationStatus.Active, tersimpan.ObservationStatus);
    }

    /// <summary>
    /// <c>Cancelled</c> tidak memetakan ke status kunjungan mana pun, jadi kunjungan yang
    /// sudah ditutup pun tetap boleh membatalkan observasinya.
    /// </summary>
    [Fact]
    public async Task Observasi_Dibatalkan_TidakMenyentuhStatusKunjungan()
    {
        using var context = BuatContext("igd-obs");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Completed);
        var observation = await SimpanObservasiAsync(context, visit.Id, EmergencyObservationStatus.Active);
        var controller = BuatObservationController(context, Pelaku);

        var result = await controller.UpdateObservationStatus(
            observation.Id,
            new UpdateEmergencyObservationObservationStatusRequest
            {
                ObservationStatus = EmergencyObservationStatus.Cancelled
            });

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));
        Assert.Equal(EmergencyVisitStatus.Completed, await BacaStatusKunjunganAsync(context, visit.Id));
    }

    // =================================================================================
    // BE-IGD-021 — titik tulis pada resusitasi
    // =================================================================================

    [Fact]
    public async Task Resusitasi_Dimulai_MemindahkanKunjunganKeInTreatmentDanMengisiWaktuMulai()
    {
        using var context = BuatContext("igd-res");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Triaged);
        var resuscitation = await SimpanResusitasiAsync(context, visit.Id, EmergencyResuscitationStatus.Planned);
        var controller = BuatResuscitationController(context, Pelaku);

        var result = await controller.UpdateResuscitationStatus(
            resuscitation.Id,
            new UpdateEmergencyResuscitationResuscitationStatusRequest
            {
                ResuscitationStatus = EmergencyResuscitationStatus.InProgress
            });

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));

        var tersimpan = await context.Set<EmgVisit>().AsNoTracking().FirstAsync(x => x.Id == visit.Id);
        Assert.Equal(EmergencyVisitStatus.InTreatment, tersimpan.VisitStatus);
        Assert.NotNull(tersimpan.TreatmentStartedAt);
    }

    [Fact]
    public async Task Resusitasi_PadaKunjunganSelesai_Ditolak409DanResusitasiTidakBerjalan()
    {
        using var context = BuatContext("igd-res");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Completed);
        var resuscitation = await SimpanResusitasiAsync(context, visit.Id, EmergencyResuscitationStatus.Planned);
        var controller = BuatResuscitationController(context, Pelaku);

        var result = await controller.UpdateResuscitationStatus(
            resuscitation.Id,
            new UpdateEmergencyResuscitationResuscitationStatusRequest
            {
                ResuscitationStatus = EmergencyResuscitationStatus.InProgress
            });

        Assert.Equal(StatusCodes.Status409Conflict, KodeStatus((IActionResult)result));

        var tersimpanVisit = await context.Set<EmgVisit>().AsNoTracking().FirstAsync(x => x.Id == visit.Id);
        Assert.Equal(EmergencyVisitStatus.Completed, tersimpanVisit.VisitStatus);
        Assert.Null(tersimpanVisit.TreatmentStartedAt);

        var tersimpanResus = await context.Set<EmgResuscitation>().AsNoTracking()
            .FirstAsync(x => x.Id == resuscitation.Id);
        Assert.Equal(EmergencyResuscitationStatus.Planned, tersimpanResus.ResuscitationStatus);
    }

    /// <summary>
    /// Resusitasi pada kunjungan yang sudah <c>InTreatment</c> adalah transisi ke status yang
    /// sama — diterima penjaga sebagai idempoten, dan tetap mengisi waktu mulai penanganan.
    /// </summary>
    [Fact]
    public async Task Resusitasi_PadaKunjunganYangSudahInTreatment_TetapMengisiWaktuMulai()
    {
        using var context = BuatContext("igd-res");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.InTreatment);
        var resuscitation = await SimpanResusitasiAsync(context, visit.Id, EmergencyResuscitationStatus.Planned);
        var controller = BuatResuscitationController(context, Pelaku);

        var result = await controller.UpdateResuscitationStatus(
            resuscitation.Id,
            new UpdateEmergencyResuscitationResuscitationStatusRequest
            {
                ResuscitationStatus = EmergencyResuscitationStatus.InProgress
            });

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));

        var tersimpan = await context.Set<EmgVisit>().AsNoTracking().FirstAsync(x => x.Id == visit.Id);
        Assert.Equal(EmergencyVisitStatus.InTreatment, tersimpan.VisitStatus);
        Assert.NotNull(tersimpan.TreatmentStartedAt);
    }

    // =================================================================================
    // BE-IGD-021 — titik tulis pada tindak lanjut
    // =================================================================================

    [Fact]
    public async Task TindakLanjut_Dieksekusi_MemindahkanKunjunganKeDisposed()
    {
        using var context = BuatContext("igd-dis");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.AwaitingDisposition);
        var disposition = await SimpanTindakLanjutAsync(context, visit.Id, EmergencyDispositionStatus.Confirmed);
        var controller = BuatDispositionController(context, Pelaku);

        var result = await controller.UpdateDispositionStatus(
            disposition.Id,
            new UpdateEmergencyDispositionDispositionStatusRequest
            {
                DispositionStatus = EmergencyDispositionStatus.Executed
            });

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));

        var tersimpanVisit = await context.Set<EmgVisit>().AsNoTracking().FirstAsync(x => x.Id == visit.Id);
        Assert.Equal(EmergencyVisitStatus.Disposed, tersimpanVisit.VisitStatus);

        // BE-IGD-008 — Disposed bukan penyelesaian klinis, jadi waktu selesai tetap kosong.
        Assert.Null(tersimpanVisit.VisitCompletedAt);
    }

    [Fact]
    public async Task TindakLanjut_DieksekusiPadaKunjunganSelesai_Ditolak409DanTetapConfirmed()
    {
        using var context = BuatContext("igd-dis");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Completed);
        var disposition = await SimpanTindakLanjutAsync(context, visit.Id, EmergencyDispositionStatus.Confirmed);
        var controller = BuatDispositionController(context, Pelaku);

        var result = await controller.UpdateDispositionStatus(
            disposition.Id,
            new UpdateEmergencyDispositionDispositionStatusRequest
            {
                DispositionStatus = EmergencyDispositionStatus.Executed
            });

        Assert.Equal(StatusCodes.Status409Conflict, KodeStatus((IActionResult)result));
        Assert.Equal(EmergencyVisitStatus.Completed, await BacaStatusKunjunganAsync(context, visit.Id));

        var tersimpan = await context.Set<EmgDisposition>().AsNoTracking()
            .FirstAsync(x => x.Id == disposition.Id);
        Assert.Equal(EmergencyDispositionStatus.Confirmed, tersimpan.DispositionStatus);
        Assert.Null(tersimpan.ExecutedAt);
    }

    /// <summary>
    /// Tindak lanjut yang hanya dikonfirmasi tidak menyentuh status kunjungan sama sekali,
    /// sehingga kunjungan mana pun boleh menerimanya.
    /// </summary>
    [Fact]
    public async Task TindakLanjut_Dikonfirmasi_TidakMenyentuhStatusKunjungan()
    {
        using var context = BuatContext("igd-dis");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Triaged);
        var disposition = await SimpanTindakLanjutAsync(context, visit.Id, EmergencyDispositionStatus.Draft);
        var controller = BuatDispositionController(context, Pelaku);

        var result = await controller.UpdateDispositionStatus(
            disposition.Id,
            new UpdateEmergencyDispositionDispositionStatusRequest
            {
                DispositionStatus = EmergencyDispositionStatus.Confirmed
            });

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));
        Assert.Equal(EmergencyVisitStatus.Triaged, await BacaStatusKunjunganAsync(context, visit.Id));
    }

    // =================================================================================
    // BE-IGD-022 — penyelesaian kunjungan
    // =================================================================================

    [Fact]
    public async Task Penyelesaian_KunjunganDisposedTanpaKewajibanTersisa_Berhasil()
    {
        using var context = BuatContext("igd-visit");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Disposed);
        var controller = BuatVisitController(context, Pelaku);

        var result = await controller.Complete(visit.Id, new CompleteVisitRequest());

        Assert.Equal(StatusCodes.Status200OK, KodeStatus((IActionResult)result));

        var tersimpan = await context.Set<EmgVisit>().AsNoTracking().FirstAsync(x => x.Id == visit.Id);
        Assert.Equal(EmergencyVisitStatus.Completed, tersimpan.VisitStatus);
        Assert.NotNull(tersimpan.VisitCompletedAt);
    }

    /// <summary>
    /// Acceptance butir 2. Kunjungan yang sudah <c>Completed</c> gagal pada aturan 1 closure
    /// gate — status wajib <c>Disposed</c> — sehingga pesannya tetap pesan kontrak, bukan
    /// pesan penjaga transisi.
    /// </summary>
    [Fact]
    public async Task Penyelesaian_KunjunganYangSudahSelesai_Ditolak409()
    {
        using var context = BuatContext("igd-visit");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Completed);
        var controller = BuatVisitController(context, Pelaku);

        var result = await controller.Complete(visit.Id, new CompleteVisitRequest());

        Assert.Equal(StatusCodes.Status409Conflict, KodeStatus((IActionResult)result));
        Assert.Equal(
            "Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan.",
            Pesan((IActionResult)result));
    }

    /// <summary>
    /// Acceptance butir 3: keempat pemeriksaan closure gate tetap berjalan dan pesannya tidak
    /// berubah sedikit pun setelah penjaga disisipkan.
    /// </summary>
    [Fact]
    public async Task Penyelesaian_MasihAdaObservasiAktif_Ditolak409DenganPesanKontrak()
    {
        using var context = BuatContext("igd-visit");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Disposed);
        await SimpanObservasiAsync(context, visit.Id, EmergencyObservationStatus.Active);
        var controller = BuatVisitController(context, Pelaku);

        var result = await controller.Complete(visit.Id, new CompleteVisitRequest());

        Assert.Equal(StatusCodes.Status409Conflict, KodeStatus((IActionResult)result));
        Assert.Equal("Masih ada observasi yang belum diselesaikan.", Pesan((IActionResult)result));
        Assert.Equal(EmergencyVisitStatus.Disposed, await BacaStatusKunjunganAsync(context, visit.Id));
    }

    [Fact]
    public async Task Penyelesaian_KunjunganTidakDitemukan_Menghasilkan404()
    {
        using var context = BuatContext("igd-visit");
        var controller = BuatVisitController(context, Pelaku);

        var result = await controller.Complete(Guid.NewGuid(), new CompleteVisitRequest());

        Assert.Equal(StatusCodes.Status404NotFound, KodeStatus((IActionResult)result));
    }
}
