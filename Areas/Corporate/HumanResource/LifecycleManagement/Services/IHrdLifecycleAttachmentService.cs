using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdLifecycleAttachmentService
    {
        Task<(bool IsSuccess, string? ErrorMessage, string? StoragePath, string? FileName, string? ContentType, long? FileSize)> ValidateAndSaveAttachmentAsync(
            IFormFile file,
            string subFolder,
            CancellationToken cancellationToken = default);

        Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> GetAttachmentStreamAsync(
            string storagePath,
            CancellationToken cancellationToken = default);
    }
}
