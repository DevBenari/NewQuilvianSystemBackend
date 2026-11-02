using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public interface IHrdEmployee360Service
    {
        Task<Employee360ServiceResult> GetEmployee360Async(
            Guid workforceProfileId,
            ClaimsPrincipal userPrincipal,
            Guid callerUserId,
            CancellationToken ct = default);
    }

    public class Employee360ServiceResult
    {
        public bool Success { get; set; }
        public bool IsForbidden { get; set; }
        public bool IsNotFound { get; set; }
        public string? Message { get; set; }
        public Employee360Response? Data { get; set; }

        public static Employee360ServiceResult Ok(Employee360Response data) =>
            new() { Success = true, Data = data };

        public static Employee360ServiceResult NotFound(string message) =>
            new() { Success = false, IsNotFound = true, Message = message };

        public static Employee360ServiceResult Forbidden(string message) =>
            new() { Success = false, IsForbidden = true, Message = message };
    }
}
