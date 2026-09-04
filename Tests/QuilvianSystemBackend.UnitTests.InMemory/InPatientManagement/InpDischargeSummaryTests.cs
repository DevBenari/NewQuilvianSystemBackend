using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-021</c> — resume pulang tersusun dan hanya DPJP yang menandatanganinya;
/// <c>BE-RWI-022</c> — koreksi resume menyimpan versi sebelumnya.
/// </summary>
/// <remarks>
/// <b>Kriteria 1 dan 2 milik <c>BE-RWI-022</c> mudah dikerjakan terbalik</b> menjadi "setiap
/// penyuntingan membuat versi". Itu akan membanjiri tabel versi dengan draf setengah jadi dan
/// membuat riwayat amandemen kehilangan artinya. Karena itu keduanya diuji berpasangan.
///
/// <para>
/// <b>Sesi koreksi disisipkan langsung.</b> Endpoint pembuka dan penutup sesi koreksi milik
/// <c>BE-RWI-030</c> dan belum ada. Yang dibutuhkan task ini hanya pembacaan keberadaan sesi
/// terbuka, sehingga barisnya disisipkan langsung di sini. Jalur endpoint-nya diuji ulang pada
/// task tersebut.
/// </para>
/// </remarks>
public sealed class InpDischargeSummaryTests
{
    [Fact]
    public async Task Kriteria1_ResumeDapatDisusunDanDiperbaruiSelagiBelumDitandatangani()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeDischargePendingAsync(world);

        var pertama = await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest
            {
                PrimaryDiagnosisText = "Demam berdarah dengue",
                FollowUpInstruction = "Kontrol tujuh hari lagi."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Success, pertama.Status);

        var kedua = await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest
            {
                PrimaryDiagnosisText = "Demam berdarah dengue derajat II",
                FollowUpInstruction = "Kontrol tiga hari lagi."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Success, kedua.Status);

        var resume = await world.DischargeService.GetSummaryAsync(episode.Id, includeRevisions: true);

        Assert.NotNull(resume);
        Assert.Equal("Demam berdarah dengue derajat II", resume!.PrimaryDiagnosisText);
        Assert.False(resume.IsSigned);

        // BE-RWI-022 kriteria 1 — penyuntingan resume yang belum ditandatangani TIDAK membuat
        // versi baru.
        Assert.Empty(resume.Revisions);
    }

    [Fact]
    public async Task Kriteria2_HanyaDpjpAktifYangDapatMenandatangani()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeDischargePendingAsync(world);
        var dokterJaga = await world.AddDoctorAsync("dr. Rina");

        await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam berdarah dengue" },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        var olehDokterLain = await world.DischargeService.SignSummaryAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.SupervisorUserId,
            dokterJaga.Id);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, olehDokterLain.Status);
        Assert.Equal(
            "Hanya DPJP episode ini yang dapat menandatangani resume.",
            olehDokterLain.Message);

        var olehBukanDokter = await world.DischargeService.SignSummaryAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, olehBukanDokter.Status);

        var olehDpjp = await world.DischargeService.SignSummaryAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, olehDpjp.Status);

        var resume = await world.DischargeService.GetSummaryAsync(episode.Id);

        Assert.NotNull(resume);
        Assert.True(resume!.IsSigned);
        Assert.Equal(world.Doctor.Id, resume.SignedByDoctorId);
        Assert.NotNull(resume.SignedAt);
    }

    [Fact]
    public async Task Kriteria3_ResumeYangSudahDitandatanganiTidakDapatDiubahLewatEndpointBiasa()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeDischargePendingAsync(world);

        await BuatResumeTertandatanganAsync(world, episode.Id);

        var hasil = await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Diagnosis lain" },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, hasil.Status);
        Assert.Contains("hanya dapat diubah lewat sesi koreksi", hasil.Message);
    }

    [Fact]
    public async Task Kriteria4_SatuEpisodePunyaPalingBanyakSatuResume()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeDischargePendingAsync(world);

        await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam berdarah dengue" },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam tifoid" },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        var jumlah = await world.DbContext.Set<InpDischargeSummary>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id);

        Assert.Equal(1, jumlah);
    }

    /// <remarks>
    /// Kewajiban privasi, bukan preferensi. Isi resume memuat diagnosis; bila ia bocor ke
    /// endpoint daftar, seluruh peran yang boleh melihat census ikut membacanya.
    /// </remarks>
    [Fact]
    public void Kriteria5_IsiResumeTidakIkutPadaEndpointDaftarMANaPun()
    {
        var kolomSensitif = new[]
        {
            "PrimaryDiagnosisText",
            "SecondaryDiagnosisText",
            "ProcedureSummary",
            "DischargeMedicationNote",
            "FollowUpInstruction",
            "ClinicalSummary"
        };

        var bentukDaftar = new[]
        {
            typeof(InpatientEpisodeListItemResponse),
            typeof(CensusItemResponse),
            typeof(IsolationMismatchItemResponse),
            typeof(InpatientEpisodeDetailResponse)
        };

        foreach (var bentuk in bentukDaftar)
        {
            var kolom = bentuk.GetProperties().Select(x => x.Name).ToList();

            foreach (var sensitif in kolomSensitif)
            {
                Assert.DoesNotContain(sensitif, kolom);
            }
        }
    }

    // =========================================================================
    // BE-RWI-022 — Versi resume
    // =========================================================================

    [Fact]
    public async Task Kriteria2_MengubahResumeTertandatanganLewatSesiKoreksiMenyimpanVersiSebelumnya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeDischargePendingAsync(world);

        await BuatResumeTertandatanganAsync(world, episode.Id);

        var sesi = await BukaSesiKoreksiAsync(world, episode.Id);

        var hasil = await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest
            {
                PrimaryDiagnosisText = "Demam tifoid",
                ClinicalSummary = "Diagnosis dibetulkan setelah hasil kultur keluar."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            world.Doctor.Id,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);
        Assert.Contains("Versi sebelumnya tersimpan", hasil.Message);

        var resume = await world.DischargeService.GetSummaryAsync(episode.Id, includeRevisions: true);

        Assert.NotNull(resume);
        Assert.Equal("Demam tifoid", resume!.PrimaryDiagnosisText);

        var versi = Assert.Single(resume.Revisions);

        Assert.Equal(1, versi.RevisionNumber);
        Assert.Equal("Demam berdarah dengue", versi.PrimaryDiagnosisText);
        Assert.Equal(sesi.Id, versi.CorrectionSessionId);
        Assert.Equal(world.Doctor.Id, versi.PreviousSignedByDoctorId);
        Assert.Equal(InpatientEpisodeTestWorld.SupervisorUserId, versi.SupersededByUserId);

        // Tanda tangan lama beserta waktunya tetap terbaca selamanya.
        Assert.NotEqual(default, versi.PreviousSignedAt);
    }

    [Fact]
    public async Task Kriteria2_SupervisorAdalahSatuSatunyaYangDapatMengubahResumeTertandatangan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeDischargePendingAsync(world);

        await BuatResumeTertandatanganAsync(world, episode.Id);
        await BukaSesiKoreksiAsync(world, episode.Id);

        var olehDpjp = await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam tifoid" },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, olehDpjp.Status);
        Assert.Contains("Hanya supervisor", olehDpjp.Message);
    }

    /// <remarks>
    /// Kriteria 3 diuji sebagaimana diminta roadmap: mencoba <c>PUT</c> dan <c>DELETE</c>
    /// langsung ke baris versi, lalu membuktikan keduanya <b>tidak ada</b>. Ketiadaan endpoint
    /// itulah bentuk penolakannya — api contract bagian 8 dan <c>RWI-DEC-057</c>.
    /// </remarks>
    [Fact]
    public void Kriteria3_TidakAdaEndpointYangDapatMengubahAtauMenghapusVersiResume()
    {
        var endpoints = typeof(InpatientDischargeController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName)
            .SelectMany(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>())
            .ToList();

        Assert.DoesNotContain(
            endpoints,
            x => (x.Template ?? string.Empty).Contains("revision", StringComparison.OrdinalIgnoreCase));

        Assert.Empty(endpoints.Where(x => x is HttpDeleteAttribute));
    }

    [Fact]
    public async Task Kriteria4_IncludeRevisionsMengembalikanVersiBerlakuBesertaDaftarVersiLamaUrutWaktu()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeDischargePendingAsync(world);

        await BuatResumeTertandatanganAsync(world, episode.Id);

        await BukaSesiKoreksiAsync(world, episode.Id);

        await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam tifoid" },
            InpatientEpisodeTestWorld.SupervisorUserId,
            world.Doctor.Id,
            actorIsSupervisor: true);

        // Resume dikoreksi kedua kalinya. Tanda tangan sebelumnya masih berlaku, sehingga
        // koreksi ini juga melahirkan satu versi lagi.
        await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam tifoid dengan komplikasi" },
            InpatientEpisodeTestWorld.SupervisorUserId,
            world.Doctor.Id,
            actorIsSupervisor: true);

        var tanpaVersi = await world.DischargeService.GetSummaryAsync(episode.Id);
        Assert.NotNull(tanpaVersi);
        Assert.Empty(tanpaVersi!.Revisions);

        var denganVersi = await world.DischargeService.GetSummaryAsync(
            episode.Id,
            includeRevisions: true);

        Assert.NotNull(denganVersi);
        Assert.Equal("Demam tifoid dengan komplikasi", denganVersi!.PrimaryDiagnosisText);
        Assert.Equal(2, denganVersi.Revisions.Count);

        Assert.Equal(1, denganVersi.Revisions[0].RevisionNumber);
        Assert.Equal("Demam berdarah dengue", denganVersi.Revisions[0].PrimaryDiagnosisText);

        Assert.Equal(2, denganVersi.Revisions[1].RevisionNumber);
        Assert.Equal("Demam tifoid", denganVersi.Revisions[1].PrimaryDiagnosisText);
    }

    [Fact]
    public async Task MenandatanganiResumeRujukanTanpaTujuanRujukanDitolak400()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenAndPlaceAsync(bed);

        await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.Referred },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam berdarah dengue" },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        var tanpaTujuan = await world.DischargeService.SignSummaryAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, tanpaTujuan.Status);
        Assert.Equal(
            "Tujuan rujukan wajib diisi untuk pasien yang dirujuk.",
            tanpaTujuan.Message);

        await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest
            {
                PrimaryDiagnosisText = "Demam berdarah dengue",
                ReferralDestination = "RSUD Provinsi"
            },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        var denganTujuan = await world.DischargeService.SignSummaryAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, denganTujuan.Status);
    }

    [Fact]
    public async Task ResumeHanyaDapatDisusunSetelahDpjpMenyatakanPasienBolehPulang()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam berdarah dengue" },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Contains("setelah DPJP menyatakan pasien boleh pulang", hasil.Message);
    }

    // =========================================================================
    // Pembantu
    // =========================================================================

    private static async Task<InpEpisode> BuatEpisodeDischargePendingAsync(
        InpatientEpisodeTestWorld world)
    {
        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, $"3-{Guid.NewGuid().ToString("N")[..3]}");

        var episode = await world.OpenAndPlaceAsync(bed);

        var decide = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, decide.Status);

        return episode;
    }

    private static async Task BuatResumeTertandatanganAsync(
        InpatientEpisodeTestWorld world,
        Guid episodeId)
    {
        var susun = await world.DischargeService.UpsertSummaryAsync(
            episodeId,
            new UpsertDischargeSummaryRequest
            {
                PrimaryDiagnosisText = "Demam berdarah dengue",
                FollowUpInstruction = "Kontrol tujuh hari lagi."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Success, susun.Status);

        var tandaTangan = await world.DischargeService.SignSummaryAsync(
            episodeId,
            null,
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, tandaTangan.Status);
    }

    /// <summary>
    /// Menyisipkan satu sesi koreksi terbuka. Endpoint pembukanya milik <c>BE-RWI-030</c>.
    /// </summary>
    private static async Task<InpCorrectionSession> BukaSesiKoreksiAsync(
        InpatientEpisodeTestWorld world,
        Guid episodeId)
    {
        var sesi = new InpCorrectionSession
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            SequenceNumber = 1,
            OpenedAt = DateTime.UtcNow,
            OpenedByUserId = InpatientEpisodeTestWorld.SupervisorUserId,
            OpenReason = "Diagnosis utama keliru, dibetulkan setelah hasil kultur keluar.",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = InpatientEpisodeTestWorld.SupervisorUserId
        };

        world.DbContext.Set<InpCorrectionSession>().Add(sesi);
        await world.DbContext.SaveChangesAsync();

        return sesi;
    }
}
