using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-447 (kontrak v29 §35.1/35.2/35.6, VR-WF-29, INT-HR-114; HRD-DEC-229, HRD-DEC-235) — endpoint generik Kotak Masuk
// Terpadu tidak lagi mengubah permohonan milik modul: buat instance generik → 409; batal/kirim ulang instance engine → 409;
// instance engine V2 lama tetap dapat dibatalkan. "Permohonan Saya" mencocokkan akun ATAU pegawai pemohon dan menandai
// isModuleManaged. AT-HRD-WF-35, AT-HRD-WF-36.
public sealed class WorkflowGenericEndpointGuardTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _sari = Guid.NewGuid();
    private readonly Guid _budi = Guid.NewGuid();
    private readonly Guid _pegawaiBudi = Guid.NewGuid();

    public void Dispose() => _db.Dispose();

    // Instance milik modul (engine WorkflowService): punya definisi, penugasan tanpa ApproverEmployeeId.
    private Guid InstanceEngine(Guid pemohon, string status = WorkflowValueConstants.WorkflowStatus.InProgress)
        => Sisip(pemohon, null, Guid.NewGuid(), status, approverEmployeeId: null);

    // Instance engine V2 lama (POST /instances): tanpa definisi, penugasan mengisi ApproverEmployeeId.
    private Guid InstanceLama(Guid pemohonUser, Guid? pemohonPegawai, string status = "Pending")
        => Sisip(pemohonUser, pemohonPegawai, Guid.Empty, status, approverEmployeeId: Guid.NewGuid());

    private Guid Sisip(Guid userId, Guid? employeeId, Guid definisi, string status, Guid? approverEmployeeId)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        var step = Guid.NewGuid();
        ctx.TrxWorkflowInstances.Add(new TrxWorkflowInstance
        {
            Id = id, WorkflowDefinitionId = definisi, ReferenceType = "HRD_TRV_REQUEST", ReferenceId = Guid.NewGuid(), RequestNumber = "WF-UJI-" + id.ToString("N")[..6],
            RequestedByUserId = userId, RequestedByEmployeeId = employeeId, WorkflowStatus = status, SourceChannel = WorkflowValueConstants.SourceChannel.Web,
            StartedAt = DateTime.UtcNow, IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        ctx.TrxWorkflowStepInstances.Add(new TrxWorkflowStepInstance
        {
            Id = step, WorkflowInstanceId = id, StepOrder = 1, StepCodeSnapshot = "S1", StepNameSnapshot = "Atasan", StepTypeSnapshot = "Approval",
            ApprovalModeSnapshot = "Any", ApproverSourceSnapshot = "RequesterManager", StepStatus = "InProgress", IsCurrentStep = true, IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        ctx.TrxWorkflowApproverAssignments.Add(new TrxWorkflowApproverAssignment
        {
            WorkflowInstanceId = id, WorkflowStepInstanceId = step, AssignedApproverUserId = Guid.NewGuid(), ApproverEmployeeId = approverEmployeeId,
            ApproverSourceSnapshot = "RequesterManager", AssignmentStatus = "Available", IsCurrentAssignment = true, IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();
        return id;
    }

    private WorkflowInboxAndInstancesController Instansi(Guid user)
    {
        var ctx = _db.CreateContext();
        var c = new WorkflowInboxAndInstancesController(null!, new WorkflowInstanceCreationService(ctx, null!, null!, null!, null!));
        c.ControllerContext = new ControllerContext { HttpContext = Pelaku.Sebagai(user).HttpContext! };
        return c;
    }

    private WorkflowApprovalActionsController Aksi(Guid user)
    {
        var c = new WorkflowApprovalActionsController(null!, null!, new WorkflowResubmitService(_db.CreateContext()));
        c.ControllerContext = new ControllerContext { HttpContext = Pelaku.Sebagai(user).HttpContext! };
        return c;
    }

    private string Status(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == id).WorkflowStatus;
    }

    private static void Konflik(IActionResult hasil, string pesan)
    {
        var r = Assert.IsType<ConflictObjectResult>(hasil);
        var isi = Assert.IsType<ApiResponse<object>>(r.Value);
        Assert.Equal(pesan, isi.Message);
    }

    // AC1 — membuat instance generik → 409, tidak ada instance baru.
    [Fact]
    public async Task Buat_instance_generik_ditolak_409()
    {
        var hasil = await Instansi(_sari).CreateInstance(new CreateWorkflowInstanceRequestDto
        {
            RequestType = "LEAVE_REQUEST", ReferenceType = "LEAVE_REQUEST", ReferenceId = Guid.NewGuid()
        });

        Konflik(hasil, WorkflowInstanceOwnership.CreateFromModuleMessage);
        using var ctx = _db.CreateContext();
        Assert.Empty(ctx.TrxWorkflowInstances.AsNoTracking().ToList());
    }

    // AC1 — batal dan kirim ulang generik untuk permohonan modul → 409, data tidak berubah.
    [Fact]
    public async Task Batal_dan_kirim_ulang_generik_untuk_permohonan_modul_ditolak_409()
    {
        var berjalan = InstanceEngine(_sari);
        var revisi = InstanceEngine(_sari, "ReturnedForRevision");

        Konflik(await Instansi(_sari).CancelInstance(berjalan, new CancelWorkflowRequestDto { CancellationReason = "Uji batal" }),
            WorkflowInstanceOwnership.CancelFromModuleMessage);
        Konflik(await Aksi(_sari).Resubmit(revisi, new ResubmitWorkflowRequestDto { ResubmitComment = "Sudah diperbaiki" }),
            WorkflowInstanceOwnership.ResubmitFromModuleMessage);

        Assert.Equal(WorkflowValueConstants.WorkflowStatus.InProgress, Status(berjalan));
        Assert.Equal("ReturnedForRevision", Status(revisi));
    }

    // AC1 — instance engine V2 lama tetap dapat dibatalkan lewat endpoint generik (sampai tidak ada lagi, HRD-DEC-236).
    [Fact]
    public async Task Instance_engine_lama_tetap_dapat_dibatalkan()
    {
        var lama = InstanceLama(_budi, _pegawaiBudi);

        var hasil = await Instansi(_budi).CancelInstance(lama, new CancelWorkflowRequestDto { CancellationReason = "Tidak jadi" });

        Assert.IsType<OkObjectResult>(hasil);
        Assert.Equal("Cancelled", Status(lama));
    }

    // AC2 — pemohon tanpa data pegawai melihat permohonan modulnya; pengguna lain tidak; isModuleManaged sesuai jenis instance.
    [Fact]
    public async Task Permohonan_saya_mencocokkan_akun_pemohon_dan_menandai_milik_modul()
    {
        var sppd = InstanceEngine(_sari);
        InstanceLama(_budi, _pegawaiBudi);

        var milikSari = await DaftarAsync(_sari);
        var item = Assert.Single(milikSari.Items);
        Assert.Equal(sppd, item.Id);
        Assert.True(item.IsModuleManaged);

        var milikBudi = await DaftarAsync(_budi);
        var lama = Assert.Single(milikBudi.Items);
        Assert.False(lama.IsModuleManaged);

        Assert.Empty((await DaftarAsync(Guid.NewGuid())).Items);
    }

    // AC2 — tanpa akun maupun pegawai, tidak ada permohonan yang tampil (sebelumnya seluruh tiket ikut tampil).
    [Fact]
    public async Task Tanpa_akun_dan_pegawai_tidak_menampilkan_apa_pun()
    {
        InstanceEngine(_sari);
        InstanceLama(_budi, _pegawaiBudi);

        var hasil = await new WorkflowInstanceCreationService(_db.CreateContext(), null!, null!, null!, null!)
            .GetMyRequestsAsync(Guid.Empty, Guid.Empty, null, null, null, null, 1, 25);

        Assert.Empty(hasil.Items);
        Assert.Equal(0, hasil.TotalData);
    }

    // AC2 — detail instance memuat isModuleManaged.
    [Fact]
    public async Task Detail_memuat_isModuleManaged()
    {
        var engine = InstanceEngine(_sari);
        var lama = InstanceLama(_budi, _pegawaiBudi);
        var svc = new WorkflowInstanceCreationService(_db.CreateContext(), null!, null!, null!, null!);

        Assert.True((await svc.GetInstanceDetailAsync(engine))!.IsModuleManaged);
        Assert.False((await new WorkflowInstanceCreationService(_db.CreateContext(), null!, null!, null!, null!).GetInstanceDetailAsync(lama))!.IsModuleManaged);
    }

    private async Task<PagedResult<WorkflowInstanceV2ResponseDto>> DaftarAsync(Guid user)
    {
        var hasil = Assert.IsType<OkObjectResult>(await Instansi(user).GetMyRequests(null, null, null, null));
        return Assert.IsType<ApiResponse<PagedResult<WorkflowInstanceV2ResponseDto>>>(hasil.Value).Data!;
    }
}
