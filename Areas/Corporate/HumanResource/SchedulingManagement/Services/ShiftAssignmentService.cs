using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    /// <summary>
    /// Service pengelolaan penugasan shift harian dan koreksi mundur jadwal (BE-HRD-097, HRD-DEC-102, INT-HR-36).
    /// </summary>
    public class ShiftAssignmentService
    {
        private readonly ShiftReplacementService _replacementService;
        private readonly ILogger<ShiftAssignmentService> _logger;

        public ShiftAssignmentService(
            ShiftReplacementService replacementService,
            ILogger<ShiftAssignmentService> logger)
        {
            _replacementService = replacementService ?? throw new ArgumentNullException(nameof(replacementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Melakukan koreksi mundur jadwal dinas (<= 3 hari kalender) dan memicu rekalkulasi absensi ke Sub-Modul 07 (INT-HR-36).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>> RetroactiveCorrectionAsync(
            Guid shiftAssignmentId,
            RetroactiveShiftCorrectionRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mengeksekusi koreksi mundur jadwal untuk shift {ShiftAssignmentId}", shiftAssignmentId);
            return await _replacementService.RetroactiveCorrectionAsync(shiftAssignmentId, req, currentUserId, cancellationToken);
        }

        /// <summary>
        /// Mengeksekusi penunjukan staf pengganti langsung pasca-terbit (BE-HRD-095, HRD-DEC-101).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<DirectShiftReplacementResponse>> DirectReplaceAsync(
            Guid shiftAssignmentId,
            DirectShiftReplacementRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mengeksekusi ganti petugas langsung untuk shift {ShiftAssignmentId}", shiftAssignmentId);
            return await _replacementService.ExecuteDirectReplacementAsync(shiftAssignmentId, req, currentUserId, cancellationToken);
        }
    }
}
