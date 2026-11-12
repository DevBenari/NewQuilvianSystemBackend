using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan bisnis pengelolaan batch formasi awal rumah sakit (BE-HRD-033).
    /// Memenuhi HRD-DEC-070, HRD-DEC-064 (T10), FR-HRD-101, VR-WFP-11 s.d. VR-WFP-14, NFR-011, dan NFR-012.
    /// </summary>
    public sealed class HrdInitialFormationBatchService : IHrdInitialFormationBatchService
    {
        private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB (VR-WFP-14)

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberAllocator;
        private readonly IHrdWorkforcePlanningWorkflowIntegrationService _workflowIntegrationService;
        private readonly ILogger<HrdInitialFormationBatchService> _logger;

        public HrdInitialFormationBatchService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberAllocator,
            IHrdWorkforcePlanningWorkflowIntegrationService workflowIntegrationService,
            ILogger<HrdInitialFormationBatchService> logger)
        {
            _dbContext = dbContext;
            _numberAllocator = numberAllocator;
            _workflowIntegrationService = workflowIntegrationService;
            _logger = logger;
        }

        public byte[] GenerateTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template Formasi Awal");

            worksheet.Cell(1, 1).Value = "OrganizationUnitId";
            worksheet.Cell(1, 2).Value = "PositionId";
            worksheet.Cell(1, 3).Value = "FormationQuantity";
            worksheet.Cell(1, 4).Value = "EffectiveStartDate";

            // Baris petunjuk contoh
            worksheet.Cell(2, 1).Value = "00000000-0000-0000-0000-000000000000";
            worksheet.Cell(2, 2).Value = "00000000-0000-0000-0000-000000000000";
            worksheet.Cell(2, 3).Value = 5;
            worksheet.Cell(2, 4).Value = DateTime.UtcNow.ToString("yyyy-MM-dd");

            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<List<InitialFormationBatchRowValidationResponse>> ValidateRowsAsync(
            List<InitialFormationBatchRowRequest> rows,
            Guid? currentBatchId = null,
            CancellationToken cancellationToken = default)
        {
            if (rows == null || rows.Count == 0)
            {
                return new List<InitialFormationBatchRowValidationResponse>();
            }

            var unitIds = rows.Select(r => r.OrganizationUnitId).Where(id => id != Guid.Empty).Distinct().ToList();
            var positionIds = rows.Select(r => r.PositionId).Where(id => id != Guid.Empty).Distinct().ToList();

            var unitMap = await _dbContext.MstOrganizationUnits.AsNoTracking()
                .Where(u => unitIds.Contains(u.Id) && !u.IsDelete)
                .ToDictionaryAsync(u => u.Id, u => u.UnitName, cancellationToken);

            var positionMap = await _dbContext.MstPositions.AsNoTracking()
                .Where(p => positionIds.Contains(p.Id) && !p.IsDelete)
                .ToDictionaryAsync(p => p.Id, p => p.PositionName, cancellationToken);

            // Formasi existing di buku formasi (VR-WFP-11)
            var existingFormations = await _dbContext.HrdPositionFormations.AsNoTracking()
                .Where(f => !f.IsDelete && unitIds.Contains(f.OrganizationUnitId) && positionIds.Contains(f.PositionId))
                .Select(f => new { f.OrganizationUnitId, f.PositionId })
                .Distinct()
                .ToListAsync(cancellationToken);

            var existingFormationSet = new HashSet<string>(
                existingFormations.Select(f => $"{f.OrganizationUnitId}:{f.PositionId}"),
                StringComparer.OrdinalIgnoreCase);

            // Formasi pada batch lain yang sedang aktif (Draft, Submitted, Approved, Applied)
            var otherBatchRowsQuery = _dbContext.HrdInitialFormationBatchRows.AsNoTracking()
                .Where(r => !r.IsDelete &&
                            unitIds.Contains(r.OrganizationUnitId) &&
                            positionIds.Contains(r.PositionId) &&
                            r.Batch!.BatchStatus != InitialFormationBatchStatus.Rejected &&
                            r.Batch.BatchStatus != InitialFormationBatchStatus.Cancelled);

            if (currentBatchId.HasValue && currentBatchId.Value != Guid.Empty)
            {
                otherBatchRowsQuery = otherBatchRowsQuery.Where(r => r.BatchId != currentBatchId.Value);
            }

            var otherBatchRows = await otherBatchRowsQuery
                .Select(r => new { r.OrganizationUnitId, r.PositionId })
                .Distinct()
                .ToListAsync(cancellationToken);

            var otherBatchSet = new HashSet<string>(
                otherBatchRows.Select(r => $"{r.OrganizationUnitId}:{r.PositionId}"),
                StringComparer.OrdinalIgnoreCase);

            var seenInCurrentBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<InitialFormationBatchRowValidationResponse>();

            foreach (var row in rows)
            {
                var pairKey = $"{row.OrganizationUnitId}:{row.PositionId}";
                var unitName = unitMap.TryGetValue(row.OrganizationUnitId, out var uName) ? uName : null;
                var posName = positionMap.TryGetValue(row.PositionId, out var pName) ? pName : null;

                bool isValid = true;
                string? validationMessage = null;

                if (row.OrganizationUnitId == Guid.Empty || unitName == null)
                {
                    isValid = false;
                    validationMessage = "Unit organisasi tidak ditemukan atau tidak aktif.";
                }
                else if (row.PositionId == Guid.Empty || posName == null)
                {
                    isValid = false;
                    validationMessage = "Jabatan tidak ditemukan atau tidak aktif.";
                }
                else if (row.FormationQuantity <= 0)
                {
                    isValid = false;
                    validationMessage = "Jumlah formasi harus lebih besar dari 0.";
                }
                else if (row.EffectiveStartDate == default)
                {
                    isValid = false;
                    validationMessage = "Tanggal mulai berlaku wajib diisi.";
                }
                else if (seenInCurrentBatch.Contains(pairKey))
                {
                    isValid = false;
                    validationMessage = "Duplikasi pasangan unit organisasi dan jabatan dalam satu batch.";
                }
                else if (existingFormationSet.Contains(pairKey))
                {
                    isValid = false;
                    validationMessage = "Pasangan unit dan jabatan ini sudah memiliki formasi di sistem (VR-WFP-11).";
                }
                else if (otherBatchSet.Contains(pairKey))
                {
                    isValid = false;
                    validationMessage = "Pasangan unit dan jabatan sudah terdapat dalam batch lain yang sedang aktif (VR-WFP-11).";
                }

                if (row.OrganizationUnitId != Guid.Empty && row.PositionId != Guid.Empty)
                {
                    seenInCurrentBatch.Add(pairKey);
                }

                results.Add(new InitialFormationBatchRowValidationResponse
                {
                    OrganizationUnitId = row.OrganizationUnitId,
                    OrganizationUnitName = unitName,
                    PositionId = row.PositionId,
                    PositionName = posName,
                    FormationQuantity = row.FormationQuantity,
                    EffectiveStartDate = row.EffectiveStartDate,
                    IsValid = isValid,
                    ValidationMessage = validationMessage
                });
            }

            return results;
        }

        public async Task<PagedResult<InitialFormationBatchListItemResponse>> GetPagedListAsync(
            InitialFormationBatchPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _dbContext.HrdInitialFormationBatches.AsNoTracking()
                .Where(b => !b.IsDelete);

            if (query.HospitalSiteId.HasValue && query.HospitalSiteId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(b => b.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.Status.HasValue)
            {
                baseQuery = baseQuery.Where(b => b.BatchStatus == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                baseQuery = baseQuery.Where(b =>
                    b.BatchNumber.ToLower().Contains(search) ||
                    b.SourceDocumentReference.ToLower().Contains(search) ||
                    (b.Notes != null && b.Notes.ToLower().Contains(search)));
            }

            var totalData = await baseQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 100);

            var batches = await baseQuery
                .OrderByDescending(b => b.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var siteIds = batches.Select(b => b.HospitalSiteId).Distinct().ToList();
            var sites = await _dbContext.MstHospitalSites.AsNoTracking()
                .Where(s => siteIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.SiteName, cancellationToken);

            var userIds = batches.Where(b => b.SubmittedByUserId.HasValue)
                .Select(b => b.SubmittedByUserId!.Value)
                .Distinct()
                .ToList();

            var users = userIds.Count > 0
                ? await _dbContext.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? string.Empty, cancellationToken)
                : new Dictionary<Guid, string>();

            var items = batches.Select(b => new InitialFormationBatchListItemResponse
            {
                Id = b.Id,
                BatchNumber = b.BatchNumber,
                HospitalSiteId = b.HospitalSiteId,
                HospitalSiteName = sites.TryGetValue(b.HospitalSiteId, out var sName) ? sName : null,
                SourceDocumentReference = b.SourceDocumentReference,
                SourceDocumentFileName = b.SourceDocumentFileName,
                BatchStatus = b.BatchStatus,
                TotalRows = b.TotalRows,
                ValidRows = b.ValidRows,
                InvalidRows = b.InvalidRows,
                SubmittedAt = b.SubmittedAt,
                SubmittedByUserName = b.SubmittedByUserId.HasValue && users.TryGetValue(b.SubmittedByUserId.Value, out var uName) ? uName : null,
                AppliedAt = b.AppliedAt,
                CreatedAt = b.CreateDateTime,
                RowVersion = b.RowVersion
            }).ToList();

            return new PagedResult<InitialFormationBatchListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<InitialFormationBatchResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialFormationBatches.AsNoTracking()
                .Include(b => b.HospitalSite)
                .Include(b => b.Rows)
                    .ThenInclude(r => r.OrganizationUnit)
                .Include(b => b.Rows)
                    .ThenInclude(r => r.Position)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return null;
            }

            var userIds = new List<Guid>();
            if (batch.SubmittedByUserId.HasValue) userIds.Add(batch.SubmittedByUserId.Value);
            if (batch.ApprovedByUserId.HasValue) userIds.Add(batch.ApprovedByUserId.Value);

            var users = userIds.Count > 0
                ? await _dbContext.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? string.Empty, cancellationToken)
                : new Dictionary<Guid, string>();

            return new InitialFormationBatchResponse
            {
                Id = batch.Id,
                BatchNumber = batch.BatchNumber,
                HospitalSiteId = batch.HospitalSiteId,
                HospitalSiteName = batch.HospitalSite?.SiteName,
                SourceDocumentReference = batch.SourceDocumentReference,
                SourceDocumentPath = batch.SourceDocumentPath,
                SourceDocumentFileName = batch.SourceDocumentFileName,
                BatchStatus = batch.BatchStatus,
                TotalRows = batch.TotalRows,
                ValidRows = batch.ValidRows,
                InvalidRows = batch.InvalidRows,
                Notes = batch.Notes,
                WorkflowInstanceId = batch.WorkflowInstanceId,
                SubmittedAt = batch.SubmittedAt,
                SubmittedByUserName = batch.SubmittedByUserId.HasValue && users.TryGetValue(batch.SubmittedByUserId.Value, out var sUser) ? sUser : null,
                ApprovedAt = batch.ApprovedAt,
                ApprovedByUserName = batch.ApprovedByUserId.HasValue && users.TryGetValue(batch.ApprovedByUserId.Value, out var aUser) ? aUser : null,
                AppliedAt = batch.AppliedAt,
                CreatedAt = batch.CreateDateTime,
                RowVersion = batch.RowVersion,
                Rows = batch.Rows.Where(r => !r.IsDelete).Select(r => new InitialFormationBatchRowResponse
                {
                    Id = r.Id,
                    OrganizationUnitId = r.OrganizationUnitId,
                    OrganizationUnitCode = r.OrganizationUnit?.UnitCode,
                    OrganizationUnitName = r.OrganizationUnit?.UnitName,
                    PositionId = r.PositionId,
                    PositionCode = r.Position?.PositionCode,
                    PositionName = r.Position?.PositionName,
                    FormationQuantity = r.FormationQuantity,
                    EffectiveStartDate = r.EffectiveStartDate,
                    IsValid = r.IsValid,
                    ValidationMessage = r.ValidationMessage,
                    CreatedFormationId = r.CreatedFormationId
                }).ToList()
            };
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> CreateDraftAsync(
            CreateInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pembuatan batch tidak boleh kosong.");
            }

            var siteExists = await _dbContext.MstHospitalSites.AsNoTracking()
                .AnyAsync(s => s.Id == request.HospitalSiteId && !s.IsDelete, cancellationToken);

            if (!siteExists)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Rumah sakit tidak ditemukan atau tidak aktif.");
            }

            if (string.IsNullOrWhiteSpace(request.SourceDocumentReference))
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Nomor atau rujukan dokumen sumber SK wajib diisi.");
            }

            string batchNumber;
            try
            {
                batchNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: WorkforcePlanningValueConstants.NumberSeriesKeys.InitialFormationBatch,
                    Prefix: "IFB",
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: 6,
                    ActorUserId: currentUserId,
                    Instant: DateTimeOffset.UtcNow), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Alokasi nomor bisnis InitialFormationBatch gagal, menggunakan nomor darurat.");
                batchNumber = $"IFB-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            }

            var validations = await ValidateRowsAsync(request.Rows, null, cancellationToken);
            var totalRows = validations.Count;
            var validRows = validations.Count(v => v.IsValid);
            var invalidRows = validations.Count(v => !v.IsValid);

            var batch = new HrdInitialFormationBatch
            {
                Id = Guid.NewGuid(),
                BatchNumber = batchNumber,
                HospitalSiteId = request.HospitalSiteId,
                SourceDocumentReference = request.SourceDocumentReference.Trim(),
                Notes = request.Notes?.Trim(),
                BatchStatus = InitialFormationBatchStatus.Draft,
                TotalRows = totalRows,
                ValidRows = validRows,
                InvalidRows = invalidRows,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = currentUserId,
                IsDelete = false
            };

            foreach (var val in validations)
            {
                batch.Rows.Add(new HrdInitialFormationBatchRow
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.Id,
                    OrganizationUnitId = val.OrganizationUnitId,
                    PositionId = val.PositionId,
                    FormationQuantity = val.FormationQuantity,
                    EffectiveStartDate = val.EffectiveStartDate,
                    IsValid = val.IsValid,
                    ValidationMessage = val.ValidationMessage,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId,
                    IsDelete = false
                });
            }

            _dbContext.HrdInitialFormationBatches.Add(batch);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Batch formasi awal {BatchNumber} berhasil dibuat sebagai Draf dengan {TotalRows} baris.", batch.BatchNumber, totalRows);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Draf batch formasi awal berhasil dibuat.", StatusCodes.Status201Created);
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> UpdateDraftAsync(
            Guid id,
            UpdateInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pembaruan batch tidak boleh kosong.");
            }

            var batch = await _dbContext.HrdInitialFormationBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            // Kriteria 8: Hanya boleh diubah selagi Draft atau Returned
            if (batch.BatchStatus != InitialFormationBatchStatus.Draft &&
                batch.BatchStatus != InitialFormationBatchStatus.Returned)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Batch berstatus {batch.BatchStatus} tidak dapat diubah. Hanya draf atau batch yang dikembalikan yang dapat diedit.");
            }

            // Konkurensi token (VR-WFP-409)
            if (batch.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data batch telah diubah oleh pengguna lain. Silakan muat ulang halaman.");
            }

            var validations = await ValidateRowsAsync(request.Rows, id, cancellationToken);
            var totalRows = validations.Count;
            var validRows = validations.Count(v => v.IsValid);
            var invalidRows = validations.Count(v => !v.IsValid);

            // Hapus baris lama
            _dbContext.HrdInitialFormationBatchRows.RemoveRange(batch.Rows);
            batch.Rows.Clear();

            // Tambahkan baris baru
            foreach (var val in validations)
            {
                batch.Rows.Add(new HrdInitialFormationBatchRow
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.Id,
                    OrganizationUnitId = val.OrganizationUnitId,
                    PositionId = val.PositionId,
                    FormationQuantity = val.FormationQuantity,
                    EffectiveStartDate = val.EffectiveStartDate,
                    IsValid = val.IsValid,
                    ValidationMessage = val.ValidationMessage,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId,
                    IsDelete = false
                });
            }

            batch.SourceDocumentReference = request.SourceDocumentReference.Trim();
            batch.Notes = request.Notes?.Trim();
            batch.TotalRows = totalRows;
            batch.ValidRows = validRows;
            batch.InvalidRows = invalidRows;
            batch.RowVersion = Guid.NewGuid();
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = currentUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Batch formasi awal {BatchNumber} berhasil diperbarui dengan {TotalRows} baris.", batch.BatchNumber, totalRows);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Draf batch formasi awal berhasil diperbarui.");
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> UploadSourceDocumentAsync(
            Guid id,
            IFormFile file,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialFormationBatches
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            if (batch.BatchStatus != InitialFormationBatchStatus.Draft &&
                batch.BatchStatus != InitialFormationBatchStatus.Returned)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Unggah scan dokumen hanya diperbolehkan untuk batch berstatus Draft atau Returned (status saat ini: {batch.BatchStatus}).");
            }

            if (file == null || file.Length == 0)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Berkas scan SK formasi tidak boleh kosong.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedFileExtensions.Contains(extension))
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Format berkas tidak diizinkan. Hanya format berkas PDF, JPG, JPEG, atau PNG yang diperbolehkan (VR-WFP-14).");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Ukuran berkas scan SK melebihi batas maksimum 10 MB (VR-WFP-14).");
            }

            var uploadFolder = Path.Combine(AppContext.BaseDirectory, "Uploads", "WorkforcePlanning", "InitialFormationBatches", id.ToString());
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var safeOriginalName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid():N}_{safeOriginalName}";
            var fullPath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            batch.SourceDocumentFileName = safeOriginalName;
            batch.SourceDocumentPath = fullPath;
            batch.RowVersion = Guid.NewGuid();
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = currentUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scan SK formasi resmi berhasil diunggah untuk batch {BatchNumber}: {FileName}", batch.BatchNumber, safeOriginalName);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Berkas scan SK formasi berhasil diunggah.");
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> GetSourceDocumentStreamAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialFormationBatches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null || string.IsNullOrWhiteSpace(batch.SourceDocumentPath) || !File.Exists(batch.SourceDocumentPath))
            {
                return null;
            }

            var extension = Path.GetExtension(batch.SourceDocumentPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            var stream = File.OpenRead(batch.SourceDocumentPath);
            var fileName = batch.SourceDocumentFileName ?? Path.GetFileName(batch.SourceDocumentPath);

            return (stream, contentType, fileName);
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> SubmitAsync(
            Guid id,
            SubmitInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pengajuan batch tidak boleh kosong.");
            }

            var batch = await _dbContext.HrdInitialFormationBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            if (batch.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict, "Data batch telah diubah oleh pengguna lain.");
            }

            if (batch.BatchStatus != InitialFormationBatchStatus.Draft &&
                batch.BatchStatus != InitialFormationBatchStatus.Returned)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Batch berstatus {batch.BatchStatus} tidak dapat diajukan. Hanya status Draft atau Returned yang sah untuk pengajuan.");
            }

            // Kriteria 4: Validasi scan berkas fisik (VR-WFP-13)
            if (string.IsNullOrWhiteSpace(batch.SourceDocumentPath))
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Dokumen scan SK formasi resmi wajib diunggah sebelum pengajuan (VR-WFP-13).");
            }

            // Kriteria 4: Validasi baris data (VR-WFP-12)
            if (batch.TotalRows == 0)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Batch formasi awal tidak memiliki baris data (VR-WFP-12).");
            }

            if (batch.InvalidRows > 0)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Terdapat {batch.InvalidRows} baris formasi yang tidak valid. Perbaiki seluruh baris sebelum mengajukan (VR-WFP-12).");
            }

            // Kriteria 2: Revalidasi langsung ke database untuk memastikan tidak ada konflik baru (VR-WFP-11)
            var rowRequests = batch.Rows.Where(r => !r.IsDelete).Select(r => new InitialFormationBatchRowRequest
            {
                OrganizationUnitId = r.OrganizationUnitId,
                PositionId = r.PositionId,
                FormationQuantity = r.FormationQuantity,
                EffectiveStartDate = r.EffectiveStartDate
            }).ToList();

            var validations = await ValidateRowsAsync(rowRequests, batch.Id, cancellationToken);
            if (validations.Any(v => !v.IsValid))
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Terdapat baris formasi yang bertentangan dengan formasi aktif atau batch lain (VR-WFP-11).");
            }

            batch.BatchStatus = InitialFormationBatchStatus.Submitted;
            batch.SubmittedAt = DateTime.UtcNow;
            batch.SubmittedByUserId = currentUserId;
            batch.RowVersion = Guid.NewGuid();
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = currentUserId;

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                batch.Notes = request.Notes.Trim();
            }

            // Kriteria 5: Panggil workflow T10
            var workflowResult = await _workflowIntegrationService.StartWorkflowAsync(new StartWorkforcePlanningWorkflowRequest
            {
                WorkflowDefinitionCode = "HRD_T10_INITIAL_FORMATION",
                ReferenceType = WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch,
                ReferenceId = batch.Id,
                DocumentNumber = batch.BatchNumber,
                SubmitComment = request.Notes ?? "Pengajuan Batch Formasi Awal",
                AutoSubmit = true
            }, cancellationToken);

            if (workflowResult.Success && workflowResult.Data != null)
            {
                batch.WorkflowInstanceId = workflowResult.Data.Id;
                batch.WorkflowDefinitionId = workflowResult.Data.WorkflowDefinitionId;
            }
            else
            {
                _logger.LogWarning(
                    "Workflow T10 tidak dapat dimulai untuk batch {BatchNumber}: {Message}. Dokumen tetap berstatus Submitted.",
                    batch.BatchNumber,
                    workflowResult.Message);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Batch formasi awal {BatchNumber} berhasil diajukan (Submitted) ke workflow T10.", batch.BatchNumber);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Batch formasi awal berhasil diajukan ke workflow persetujuan T10.");
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> CancelAsync(
            Guid id,
            CancelInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pembatalan tidak boleh kosong.");
            }

            var batch = await _dbContext.HrdInitialFormationBatches
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            if (batch.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict, "Data batch telah diubah oleh pengguna lain.");
            }

            // Kriteria 7: Hanya dari Draft atau Returned yang boleh dibatalkan; Applied tidak dapat diubah apa pun
            if (batch.BatchStatus == InitialFormationBatchStatus.Applied)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Batch formasi awal yang sudah diterapkan (Applied) tidak dapat dibatalkan.");
            }

            if (batch.BatchStatus != InitialFormationBatchStatus.Draft &&
                batch.BatchStatus != InitialFormationBatchStatus.Returned)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Batch berstatus {batch.BatchStatus} tidak dapat dibatalkan. Pembatalan hanya sah untuk status Draft atau Returned.");
            }

            batch.BatchStatus = InitialFormationBatchStatus.Cancelled;
            batch.IsCancel = true;
            batch.CancelDateTime = DateTime.UtcNow;
            batch.CancelBy = currentUserId;
            batch.RowVersion = Guid.NewGuid();
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = currentUserId;

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                batch.Notes = string.IsNullOrWhiteSpace(batch.Notes)
                    ? $"[Dibatalkan]: {request.Reason.Trim()}"
                    : $"{batch.Notes}\n[Dibatalkan]: {request.Reason.Trim()}";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Batch formasi awal {BatchNumber} berhasil dibatalkan.", batch.BatchNumber);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Batch formasi awal berhasil dibatalkan.");
        }
    }
}
