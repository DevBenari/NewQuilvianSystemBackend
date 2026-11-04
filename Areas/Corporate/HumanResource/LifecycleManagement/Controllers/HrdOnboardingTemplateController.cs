using System;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/corporate/human-resource/onboarding-templates")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Onboarding Templates")]
    public class HrdOnboardingTemplateController : ControllerBase
    {
        private readonly IHrdOnboardingTemplateService _templateService;

        public HrdOnboardingTemplateController(IHrdOnboardingTemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OnboardingTemplateListItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Onboarding Templates", Description = "Melihat daftar template onboarding", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnboardingTemplates", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] OnboardingTemplateQuery query, CancellationToken cancellationToken)
        {
            var result = await _templateService.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OnboardingTemplateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Onboarding Template Detail", Description = "Melihat detail template onboarding", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OnboardingTemplates", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _templateService.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OnboardingTemplateResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Onboarding Template", Description = "Membuat template onboarding", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("OnboardingTemplates", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateOnboardingTemplateRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId(User);
            var result = await _templateService.CreateAsync(request, userId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OnboardingTemplateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Onboarding Template", Description = "Memperbarui template onboarding", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OnboardingTemplates", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOnboardingTemplateRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId(User);
            var result = await _templateService.UpdateAsync(id, request, userId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Delete", "Delete Onboarding Template", Description = "Menghapus template onboarding", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("OnboardingTemplates", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId(User);
            var result = await _templateService.DeleteAsync(id, userId, cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(RecruitmentResult<T> result)
        {
            if (result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message));
            }

            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message, result.Errors));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }
    }
}
