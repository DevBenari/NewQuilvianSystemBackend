using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/human-resource/lifecycle-management/initial-contract-batches")]
    [Route("api/v1/corporate/human-resource/initial-contract-batches")]
    [Authorize]
    [Tags("Corporate / Human Resource / Lifecycle Management / Initial Contract Batch")]
    public class HrdInitialContractBatchController : ControllerBase
    {
        private readonly IHrdInitialContractBatchService _batchService;

        public HrdInitialContractBatchController(IHrdInitialContractBatchService batchService)
        {
            _batchService = batchService ?? throw new ArgumentNullException(nameof(batchService));
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] InitialContractBatchQuery query, CancellationToken cancellationToken)
        {
            var result = await _batchService.GetPagedAsync(query, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _batchService.GetByIdAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDraftBatch(
            [FromBody] CreateInitialContractBatchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.CreateDraftBatchAsync(request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:guid}/rows")]
        public async Task<IActionResult> AddOrUpdateRow(
            Guid id,
            [FromBody] AddInitialContractBatchRowRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.AddOrUpdateRowAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:guid}/rows/{rowId:guid}")]
        public async Task<IActionResult> DeleteRow(
            Guid id,
            Guid rowId,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.DeleteRowAsync(id, rowId, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:guid}/submit")]
        public async Task<IActionResult> SubmitBatch(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.SubmitBatchAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelBatch(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _batchService.CancelBatchAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
