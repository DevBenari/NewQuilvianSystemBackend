using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/human-resource/lifecycle-management/initial-contract-batches")]
    [Route("api/v1/corporate/human-resource/initial-contract-batches")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
        moduleName: "Human Resource Lifecycle Management",
        displayName: "Batch Kontrak Awal",
        AreaName = "Corporate",
        ControllerName = "WorkforceInitialContractBatch",
        Description = "Pengisian massal kontrak awal pegawai existing",
        SortOrder = 34)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Initial Contract Batch")]
    public class HrdInitialContractBatchController : ControllerBase
    {
        private readonly IHrdInitialContractBatchService _batchService;

        public HrdInitialContractBatchController(IHrdInitialContractBatchService batchService)
        {
            _batchService = batchService ?? throw new ArgumentNullException(nameof(batchService));
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Batch Kontrak Awal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInitialContractBatch", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] InitialContractBatchQuery query, CancellationToken cancellationToken)
        {
            var result = await _batchService.GetPagedAsync(query, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Batch Kontrak Awal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInitialContractBatch", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _batchService.GetByIdAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [AccessAction("Create", "Membuat Batch Kontrak Awal", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceInitialContractBatch", "Create")]
        public async Task<IActionResult> CreateDraftBatch(
            [FromBody] CreateInitialContractBatchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.CreateDraftBatchAsync(request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:guid}/rows")]
        [AccessAction("Update", "Mengubah Baris Batch Kontrak Awal", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceInitialContractBatch", "Update")]
        public async Task<IActionResult> AddOrUpdateRow(
            Guid id,
            [FromBody] AddInitialContractBatchRowRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.AddOrUpdateRowAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:guid}/rows/{rowId:guid}")]
        [AccessAction("Update", "Mengubah Baris Batch Kontrak Awal", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceInitialContractBatch", "Update")]
        public async Task<IActionResult> DeleteRow(
            Guid id,
            Guid rowId,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.DeleteRowAsync(id, rowId, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Mengajukan Batch Kontrak Awal", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceInitialContractBatch", "Submit")]
        public async Task<IActionResult> SubmitBatch(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.SubmitBatchAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Membatalkan Batch Kontrak Awal", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceInitialContractBatch", "Cancel")]
        public async Task<IActionResult> CancelBatch(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.CancelBatchAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
