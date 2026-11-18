using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-363 — keselarasan keempat endpoint S-A8 dengan kontrak v27 §33 (route, verb, hak akses, kode status, DTO) dan matriks
// izin §31. Nama ruas dibandingkan dalam bentuk JSON camelCase, sama dengan yang diterima frontend.
public sealed class WorkforcePayrollDataContractTests
{
    private static readonly Type C = typeof(WorkforcePayrollDataController);

    private static string[] Ruas<T>() =>
        typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name)).OrderBy(x => x).ToArray();

    private static string[] Urut(params string[] x) => x.OrderBy(v => v).ToArray();

    [Fact]
    public void Controller_route_tag_dan_identitas_akses_sesuai_kontrak()
    {
        Assert.Equal("api/v1/corporate/human-resource/workforce-core/payroll-data", C.GetCustomAttribute<RouteAttribute>()!.Template);
        Assert.Equal(new[] { "Corporate / Human Resource / Workforce Core / Payroll Data" }, C.GetCustomAttribute<TagsAttribute>()!.Tags);
        var ac = C.GetCustomAttribute<AccessControllerAttribute>()!;
        Assert.Equal(("HUMAN_RESOURCE_WORKFORCE_CORE", "WorkforcePayrollData", "Corporate", true, false),
            (ac.ModuleCode, ac.ControllerName, ac.AreaName, ac.VisibleInRoleAccess, ac.IsSystemOnly));
        Assert.NotNull(C.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
    }

    // §33.1 + matriks izin §31: verb, path, hak, dan kode status yang dideklarasikan (403 hanya pada PUT yang mengembalikannya
    // sendiri untuk opsi setuju/verifikasi; 403 tanpa hak endpoint datang dari filter, sama dengan konvensi controller HR lain).
    [Theory]
    [InlineData(nameof(WorkforcePayrollDataController.Summary), "GET", "summary", "Read", new[] { 200 })]
    [InlineData(nameof(WorkforcePayrollDataController.List), "GET", null, "Read", new[] { 200 })]
    [InlineData(nameof(WorkforcePayrollDataController.Detail), "GET", "{workforceProfileId:guid}", "Update", new[] { 200, 404 })]
    [InlineData(nameof(WorkforcePayrollDataController.Save), "PUT", "{workforceProfileId:guid}", "Update", new[] { 200, 400, 403, 404 })]
    public void Endpoint_sesuai_kontrak(string metode, string verb, string? path, string hak, int[] kode)
    {
        var m = C.GetMethod(metode)!;
        var http = m.GetCustomAttributes<HttpMethodAttribute>().Single();
        Assert.Equal((verb, path), (http.HttpMethods.Single(), http.Template));

        var izin = m.GetCustomAttribute<AccessPermissionAttribute>()!;
        var aksi = m.GetCustomAttribute<AccessActionAttribute>()!;
        Assert.Equal(("WorkforcePayrollData", hak), ((string)izin.Arguments![0], (string)izin.Arguments[1]));
        Assert.Equal(hak, aksi.ActionName);                 // argumen ke-2 [AccessPermission] = nama [AccessAction]
        Assert.Equal(hak, aksi.AccessType);
        Assert.Equal($"{hak} Workforce Payroll Data", aksi.DisplayName);
        Assert.True(aksi.VisibleInRoleAccess);
        Assert.False(aksi.IsSystemOnly);

        Assert.Equal(kode, m.GetCustomAttributes<ProducesResponseTypeAttribute>().Select(x => x.StatusCode).OrderBy(x => x).ToArray());
        foreach (var p in m.GetCustomAttributes<ProducesResponseTypeAttribute>().Where(x => x.StatusCode != 200))
            Assert.Equal(typeof(ApiResponse<object>), p.Type);
    }

    [Fact]
    public void Tipe_respons_200_sesuai_kontrak()
    {
        Type Ok(string m) => C.GetMethod(m)!.GetCustomAttributes<ProducesResponseTypeAttribute>().Single(x => x.StatusCode == 200).Type;
        Assert.Equal(typeof(ApiResponse<WorkforcePayrollDataSummaryResponse>), Ok(nameof(WorkforcePayrollDataController.Summary)));
        Assert.Equal(typeof(ApiResponse<PagedResult<WorkforcePayrollDataItemResponse>>), Ok(nameof(WorkforcePayrollDataController.List)));
        Assert.Equal(typeof(ApiResponse<WorkforcePayrollDataDetailResponse>), Ok(nameof(WorkforcePayrollDataController.Detail)));
        Assert.Equal(typeof(ApiResponse<WorkforcePayrollDataSaveResponse>), Ok(nameof(WorkforcePayrollDataController.Save)));
    }

    // Hak Import versi lama dihapus (HRD-DEC-223): hanya Read dan Update yang terdaftar untuk WorkforcePayrollData.
    [Fact]
    public void Hanya_hak_Read_dan_Update()
    {
        var hak = C.GetMethods().Select(m => m.GetCustomAttribute<AccessActionAttribute>()).Where(a => a != null).Select(a => a!.ActionName).Distinct().OrderBy(x => x);
        Assert.Equal(new[] { "Read", "Update" }, hak);
        Assert.Equal(4, C.GetMethods().Count(m => m.GetCustomAttribute<AccessPermissionAttribute>() != null));
    }

    // §33.2 dan §33.3: nama ruas DTO. SaveResponse memuat juga workforceProfileId (gema path, dipakai frontend) yang tidak tercantum
    // di tabel §33.3 — dicatat untuk penyegaran kontrak pada pass blueprint.
    [Fact]
    public void Ruas_DTO_sesuai_kontrak()
    {
        Assert.Equal(Urut("search", "personType", "completeness", "pageNumber", "pageSize"), Ruas<WorkforcePayrollDataQuery>());
        var q = new WorkforcePayrollDataQuery();
        Assert.Equal((1, 25), (q.PageNumber, q.PageSize));

        Assert.Equal(Urut("workforceProfileId", "personType", "number", "name", "departmentName", "positionName", "hasApprovedSalaryAssignment",
            "hasPendingSalaryAssignment", "salaryStructureName", "salaryEffectiveStartDate", "bankStatus", "bankName", "accountNumberMasked", "isComplete", "issues"),
            Ruas<WorkforcePayrollDataItemResponse>());
        Assert.Equal(Urut("totalPeople", "withApprovedSalaryAssignment", "withVerifiedBankAccount", "complete"), Ruas<WorkforcePayrollDataSummaryResponse>());
        Assert.Equal(Urut("workforceProfileId", "personType", "number", "name", "departmentName", "positionName", "salary", "bankAccount"), Ruas<WorkforcePayrollDataDetailResponse>());
        Assert.Equal(Urut("id", "salaryStructureId", "salaryStructureCode", "salaryStructureName", "salaryGradeCode", "gradeMinimum", "gradeMaximum", "baseSalary",
            "effectiveStartDate", "isApproved", "approvedAt"), Ruas<WorkforcePayrollSalaryDetail>());
        Assert.Equal(Urut("id", "bankId", "bankName", "accountNumber", "accountHolderName", "isVerified"), Ruas<WorkforcePayrollBankDetail>());
        Assert.Equal(Urut("salary", "bankAccount", "approveSalaryAssignment", "markBankAccountVerified"), Ruas<UpdateWorkforcePayrollDataRequest>());
        Assert.Equal(Urut("salaryStructureId", "baseSalary", "effectiveStartDate"), Ruas<UpdateWorkforcePayrollSalaryRequest>());
        Assert.Equal(Urut("bankId", "accountNumber", "accountHolderName"), Ruas<UpdateWorkforcePayrollBankRequest>());
        Assert.Equal(Urut("workforceProfileId", "salaryAssignment", "bankAccount", "warnings"), Ruas<WorkforcePayrollDataSaveResponse>());

        var r = new UpdateWorkforcePayrollDataRequest();
        Assert.Equal((false, false), (r.ApproveSalaryAssignment, r.MarkBankAccountVerified));
        var s = new WorkforcePayrollDataSaveResponse();
        Assert.Equal(("Tidak diubah", "Tidak diubah"), (s.SalaryAssignment, s.BankAccount));
    }
}
