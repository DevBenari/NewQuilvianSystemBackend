using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-023</c> — daftar periksa administrasi dapat ditandai dan bersifat menahan;
/// <c>BE-RWI-024</c> — kasir dapat menandai kelayakan keuangan.
/// </summary>
/// <remarks>
/// <b>`RWI-RISK-003` diterima secara sadar.</b> Penandaan kelayakan keuangan bersifat manual;
/// nilainya bergantung pada disiplin petugas kasir, bukan pada angka tagihan yang sebenarnya.
/// Test di sini membuktikan aturannya bekerja — bukan bahwa angkanya benar.
/// </remarks>
public sealed class InpClearanceAndFinancialTests
{
    // =========================================================================
    // BE-RWI-023 — Daftar periksa administrasi
    // =========================================================================

    [Fact]
    public async Task Kriteria1Dan2_DaftarMenampilkanButirAktifDanPenandaanMenyimpanPelakuSertaWaktunya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var wajib = await world.AddClearanceItemAsync("Berkas administrasi lengkap", sortOrder: 1);
        var opsional = await world.AddClearanceItemAsync(
            "Kuesioner kepuasan",
            isMandatory: false,
            sortOrder: 2);

        var episode = await world.OpenDraftEpisodeAsync();

        var sebelum = await world.DischargeService.GetClearanceChecklistAsync(episode.Id);

        Assert.NotNull(sebelum);
        Assert.Equal(2, sebelum!.TotalItem);
        Assert.Equal(0, sebelum.TotalMarked);
        Assert.Equal(1, sebelum.TotalBlocking);
        Assert.All(sebelum.Items, x => Assert.False(x.IsMarked));

        var sebelumTandai = DateTime.UtcNow;

        var tandai = await world.DischargeService.MarkClearanceItemAsync(
            episode.Id,
            wajib.Id,
            new MarkClearanceItemRequest { Note = "Berkas diserahkan keluarga." },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, tandai.Status);

        var sesudah = await world.DischargeService.GetClearanceChecklistAsync(episode.Id);

        var butirWajib = sesudah!.Items.Single(x => x.ItemId == wajib.Id);

        Assert.True(butirWajib.IsMarked);
        Assert.Equal(InpatientEpisodeTestWorld.ActorUserId, butirWajib.MarkedByUserId);
        Assert.InRange(butirWajib.MarkedAt!.Value, sebelumTandai, DateTime.UtcNow);
        Assert.Equal("Berkas diserahkan keluarga.", butirWajib.Note);
        Assert.False(butirWajib.IsBlocking);

        Assert.Equal(0, sesudah.TotalBlocking);
        Assert.False(sesudah.Items.Single(x => x.ItemId == opsional.Id).IsMarked);
    }

    /// <remarks>
    /// Kriteria 3 dan 4 berpasangan: yang menahan hanyalah butir <b>wajib</b>. Butir tidak
    /// wajib yang belum ditandai tidak boleh ikut menahan — bila ia menahan, seluruh butir
    /// akan diperlakukan sama dan penanda <c>IsMandatory</c> kehilangan artinya.
    /// </remarks>
    [Fact]
    public async Task Kriteria3Dan4_HanyaButirWajibYangMenahanPenutupan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var wajib = await world.AddClearanceItemAsync("Berkas administrasi lengkap");
        await world.AddClearanceItemAsync("Kuesioner kepuasan", isMandatory: false);

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed, markMandatoryClearance: false);

        // Butir wajib belum ditandai — penutupan ditolak, dan pesannya menyebut butirnya.
        var ditolak = await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, ditolak.Status);
        Assert.Contains("Berkas administrasi lengkap", ditolak.Message);

        // Butir wajib ditandai; butir tidak wajib sengaja dibiarkan kosong.
        await world.DischargeService.MarkClearanceItemAsync(
            episode.Id,
            wajib.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        var diterima = await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, diterima.Status);
    }

    /// <remarks>
    /// Kriteria 5 wajib menonaktifkan butir <b>di tengah episode berjalan</b> dan memeriksa
    /// keduanya: butirnya tidak lagi menahan, dan penandaan lamanya tidak hilang.
    /// </remarks>
    [Fact]
    public async Task Kriteria5_ButirYangDinonaktifkanTidakLagiMenahanDanPenandaanLamanyaTidakHilang()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var ditandai = await world.AddClearanceItemAsync("Surat keterangan dirawat", sortOrder: 1);
        var belumDitandai = await world.AddClearanceItemAsync("Obat pulang diserahkan", sortOrder: 2);

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed, markMandatoryClearance: false);

        await world.DischargeService.MarkClearanceItemAsync(
            episode.Id,
            ditandai.Id,
            new MarkClearanceItemRequest { Note = "Surat sudah diserahkan." },
            InpatientEpisodeTestWorld.ActorUserId);

        var sebelum = await world.DischargeService.GetClearanceChecklistAsync(episode.Id);
        Assert.Equal(1, sebelum!.TotalBlocking);

        // Admin menonaktifkan kedua butir di tengah episode berjalan.
        var trackedDitandai = await world.DbContext.Set<MstInpatientClearanceItem>()
            .FirstAsync(x => x.Id == ditandai.Id);
        var trackedBelum = await world.DbContext.Set<MstInpatientClearanceItem>()
            .FirstAsync(x => x.Id == belumDitandai.Id);

        trackedDitandai.IsActive = false;
        trackedBelum.IsActive = false;
        await world.DbContext.SaveChangesAsync();

        var sesudah = await world.DischargeService.GetClearanceChecklistAsync(episode.Id);

        // Butir yang pernah ditandai TETAP muncul beserta penandaannya.
        var barisLama = sesudah!.Items.Single(x => x.ItemId == ditandai.Id);
        Assert.True(barisLama.IsMarked);
        Assert.False(barisLama.IsActive);
        Assert.Equal("Surat sudah diserahkan.", barisLama.Note);
        Assert.False(barisLama.IsBlocking);

        // Butir yang tidak pernah ditandai hilang dari daftar, dan tidak lagi menahan.
        Assert.DoesNotContain(sesudah.Items, x => x.ItemId == belumDitandai.Id);
        Assert.Equal(0, sesudah.TotalBlocking);

        var tutup = await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, tutup.Status);
    }

    [Fact]
    public async Task MenandaiButirYangSudahTidakAktifDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var item = await world.AddClearanceItemAsync("Butir lama", isActive: false);
        var episode = await world.OpenDraftEpisodeAsync();

        var hasil = await world.DischargeService.MarkClearanceItemAsync(
            episode.Id,
            item.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
    }

    // =========================================================================
    // BE-RWI-024 — Kelayakan keuangan
    // =========================================================================

    /// <remarks>
    /// Kriteria 1 dan 3: ketiga nilai dikenali, dan setiap penandaan menyimpan pelaku, waktu,
    /// serta catatannya. Riwayatnya bersifat menambah — nilai dapat berpindah bolak-balik dan
    /// setiap perpindahan tersimpan.
    /// </remarks>
    [Fact]
    public async Task Kriteria1Dan3_TigaNilaiDikenaliDanSetiapPenandaanTersimpanBesertaPelakunya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await world.OpenDraftEpisodeAsync();

        var awal = await world.DischargeService.GetFinancialClearanceAsync(episode.Id);

        Assert.NotNull(awal);
        Assert.Equal((int)InpFinancialClearanceStatus.Pending, awal!.CurrentStatus);
        Assert.False(awal.IsCleared);
        Assert.Empty(awal.History);

        foreach (var nilai in new[]
        {
            InpFinancialClearanceStatus.Blocked,
            InpFinancialClearanceStatus.Cleared,
            InpFinancialClearanceStatus.Blocked
        })
        {
            var hasil = await world.DischargeService.MarkFinancialClearanceAsync(
                episode.Id,
                new MarkFinancialClearanceRequest
                {
                    ClearanceStatus = (int)nilai,
                    Note = $"Ditandai {nilai}."
                },
                InpatientEpisodeTestWorld.SupervisorUserId,
                actorIsCashierOrBilling: true);

            Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);
        }

        var sesudah = await world.DischargeService.GetFinancialClearanceAsync(episode.Id);

        Assert.Equal(3, sesudah!.History.Count);
        Assert.Equal((int)InpFinancialClearanceStatus.Blocked, sesudah.CurrentStatus);
        Assert.False(sesudah.IsCleared);

        Assert.Equal(new[] { 1, 2, 3 }, sesudah.History.Select(x => x.SequenceNumber));

        foreach (var entry in sesudah.History)
        {
            Assert.Equal(InpatientEpisodeTestWorld.SupervisorUserId, entry.MarkedByUserId);
            Assert.NotEqual(default, entry.MarkedAt);
            Assert.False(string.IsNullOrWhiteSpace(entry.Note));

            // RWI-RISK-003 — setiap baris wajib menyatakan bahwa nilainya ditandai orang,
            // bukan dihitung sistem dari angka tagihan.
            Assert.True(entry.IsManualMarking);
        }
    }

    [Fact]
    public async Task Kriteria2_PenandaanTanpaCatatanDitolak400()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await world.OpenDraftEpisodeAsync();

        var hasil = await world.DischargeService.MarkFinancialClearanceAsync(
            episode.Id,
            new MarkFinancialClearanceRequest
            {
                ClearanceStatus = (int)InpFinancialClearanceStatus.Cleared,
                Note = "   "
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsCashierOrBilling: true);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Equal(
            "Catatan wajib diisi saat menandai kelayakan keuangan.",
            hasil.Message);
    }

    [Fact]
    public async Task Kriteria4_HanyaPeranKasirAtauBillingYangDapatMenandai()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await world.OpenDraftEpisodeAsync();

        var hasil = await world.DischargeService.MarkFinancialClearanceAsync(
            episode.Id,
            new MarkFinancialClearanceRequest
            {
                ClearanceStatus = (int)InpFinancialClearanceStatus.Cleared,
                Note = "Menurut saya sudah lunas."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsCashierOrBilling: false);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, hasil.Status);
        Assert.Equal(
            "Hanya petugas kasir atau billing yang dapat menandai kelayakan keuangan.",
            hasil.Message);

        var tersimpan = await world.DbContext.Set<InpFinancialClearance>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id);

        Assert.Equal(0, tersimpan);
    }

    /// <remarks>
    /// Kriteria 5. <c>Pending</c> dan <c>Blocked</c> sama-sama menahan; hanya <c>Cleared</c>
    /// yang membuka penutupan.
    /// </remarks>
    [Fact]
    public async Task Kriteria5_HanyaClearedYangMembukaPenutupan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed, markFinancialCleared: false);

        // Pending — menahan.
        var saatPending = await world.DischargeService.EvaluateClosureReadinessAsync(episode.Id);
        var syaratKeuangan = saatPending!.Conditions.Single(x => x.Code == "FINANCIAL_CLEARED");

        Assert.False(syaratKeuangan.IsSatisfied);
        Assert.False(saatPending.IsReady);

        // Blocked — tetap menahan.
        await world.DischargeService.MarkFinancialClearanceAsync(
            episode.Id,
            new MarkFinancialClearanceRequest
            {
                ClearanceStatus = (int)InpFinancialClearanceStatus.Blocked,
                Note = "Masih ada sisa tagihan."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsCashierOrBilling: true);

        var saatBlocked = await world.DischargeService.EvaluateClosureReadinessAsync(episode.Id);
        Assert.False(saatBlocked!.IsReady);

        // Cleared — membuka.
        await world.DischargeService.MarkFinancialClearanceAsync(
            episode.Id,
            new MarkFinancialClearanceRequest
            {
                ClearanceStatus = (int)InpFinancialClearanceStatus.Cleared,
                Note = "Tagihan sudah lunas."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsCashierOrBilling: true);

        var saatCleared = await world.DischargeService.EvaluateClosureReadinessAsync(episode.Id);
        Assert.True(saatCleared!.IsReady);
    }
}
