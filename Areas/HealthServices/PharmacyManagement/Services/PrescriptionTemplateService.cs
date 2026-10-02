using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Penolakan kewenangan pada template resep — dijawab <c>403</c> oleh controller.
    /// </summary>
    /// <remarks>
    /// Dipisah dari <see cref="InvalidOperationException"/> yang dipakai service ini untuk kesalahan
    /// isian (<c>400</c>), supaya penolakan kepemilikan tidak terbaca sebagai kesalahan pengisian.
    /// </remarks>
    public sealed class PrescriptionTemplateForbiddenException : Exception
    {
        public PrescriptionTemplateForbiddenException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Template resep milik dokter — <c>BE-RWI-105</c>, migration <c>R9</c> (kode saja),
    /// <c>RWI-DEC-122</c>, <c>RWI-DEC-135</c>, <c>RWI-DEC-152</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Berlaku untuk semua pemakai, termasuk poliklinik dan Farmasi</b> (<c>FR-DOK-088</c>):
    /// pemilik template selalu dokter yang tertaut akun login, pada buat, buat-dari-resep, dan ubah.
    /// Permintaan yang menyebut dokter lain sebagai pemilik ditolak <c>403</c> tanpa menyimpan apa pun;
    /// mengubah dan menghapus hanya oleh pemiliknya; pemindahan pemilik ditolak. Perubahan perilaku
    /// poliklinik ini disetujui pemilik blueprint <c>rawat-jalan</c>, Sukma GP, lewat
    /// <c>RWI-DEC-152</c>.
    /// </para>
    /// <para>
    /// <b>Berlaku hanya pada ruang kerja rawat inap</b> (<c>FR-DOK-089</c>): template yang dibuat dari
    /// sana tersimpan pribadi, dan pemakaian pada resep berkonteks rawat inap hanya template milik
    /// dokter login oleh dokter. Fitur Bersama poliklinik tetap hidup.
    /// </para>
    /// <para>
    /// <b>Pemakaian tidak lagi gagal seluruhnya</b> (<c>FR-DOK-090</c>). Setiap butir diperiksa ulang
    /// terhadap alergi aktif pasien dan ketersediaan obat. Butir bentrok alergi tetap masuk draft dengan
    /// penanda; butir yang obatnya tidak tersedia tidak masuk draft dan dilaporkan bertanda, sehingga
    /// butir lain tetap masuk. Draft rawat inap yang masih membawa butir bermasalah ditolak saat disimpan
    /// (<c>VAL-DOK-57</c>, di <see cref="PrescriptionValidationService"/>).
    /// </para>
    /// <para>
    /// <b>Kewenangan dari data, bukan dari nama peran.</b> "Dokter login" adalah dokter yang tertaut
    /// akun lewat <see cref="InpatientClinicalContextService.ResolveActorDoctorIdAsync"/>; hak akses
    /// <c>PrescriptionTemplate : *</c> tetap diatur layar Akses Role.
    /// </para>
    /// </remarks>
    public class PrescriptionTemplateService
    {
        private const string PenolakanBukanAtasNamaSendiri =
            "Template hanya dapat dibuat atau diubah atas nama Anda sendiri.";

        private const string PenolakanMilikDokterLain = "Template ini milik dokter lain.";

        private const string PenolakanPakaiRawatInap =
            "Template ini tidak dapat dipakai dari ruang kerja rawat inap.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InsuranceCoverageService _coverageService;
        private readonly PrescriptionAggregateService _aggregateService;
        private readonly InpatientClinicalContextService _clinicalContextService;

        public PrescriptionTemplateService(
            ApplicationDbContext dbContext,
            InsuranceCoverageService coverageService,
            PrescriptionAggregateService aggregateService,
            InpatientClinicalContextService clinicalContextService)
        {
            _dbContext = dbContext;
            _coverageService = coverageService;
            _aggregateService = aggregateService;
            _clinicalContextService = clinicalContextService;
        }

        /// <summary>
        /// Dokter yang tertaut akun login, atau kosong. Dipakai controller untuk penyaring
        /// <c>ownerScope=Mine</c>.
        /// </summary>
        public Task<Guid?> ResolveActorDoctorIdAsync(
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
            => _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

        public async Task<MstPrescriptionTemplate> CreateAsync(
            CreatePrescriptionTemplateRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // VAL-DOK-56. Pemilik dari akun login; penyebutan dokter lain ditolak, nol baris tersimpan.
            var ownerDoctorId = await RequireActorDoctorAsync(user, actorUserId, cancellationToken);

            if (request.OwnerDoctorId != Guid.Empty && request.OwnerDoctorId != ownerDoctorId)
                throw new PrescriptionTemplateForbiddenException(PenolakanBukanAtasNamaSendiri);

            await ValidateDoctorAsync(ownerDoctorId, cancellationToken);
            EnsureNotEmpty(request.Items, request.Compounds);
            await ValidateTemplateContentAsync(request.Items, request.Compounds, cancellationToken);

            var now = DateTime.UtcNow;
            var entity = new MstPrescriptionTemplate
            {
                Id = Guid.NewGuid(),
                TemplateCode = await GenerateCodeAsync(now, cancellationToken),
                TemplateName = request.TemplateName.Trim(),
                TemplateCategory = Normalize(request.TemplateCategory),
                Description = Normalize(request.Description),
                OwnerDoctorId = ownerDoctorId,
                // RWI-DEC-135 butir (3): dari ruang kerja rawat inap tersimpan pribadi.
                IsShared = !IsInpatientContext(request.ServiceContext) && request.IsShared,
                IsFavorite = request.IsFavorite,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            ApplyGraph(entity, request.Items, request.Compounds, actorUserId, now);
            RebuildCounts(entity);
            _dbContext.Set<MstPrescriptionTemplate>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<MstPrescriptionTemplate> UpdateAsync(
            Guid id,
            UpdatePrescriptionTemplateRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstPrescriptionTemplate>()
                .Include(x => x.Items)
                .Include(x => x.Compounds).ThenInclude(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Template resep tidak ditemukan.");

            var actorDoctorId = await RequireActorDoctorAsync(user, actorUserId, cancellationToken);

            // VAL-DOK-56a — hanya pemilik.
            if (entity.OwnerDoctorId != actorDoctorId)
                throw new PrescriptionTemplateForbiddenException(PenolakanMilikDokterLain);

            // VAL-DOK-56 — pemindahan pemilik ditolak.
            if (request.OwnerDoctorId != Guid.Empty && request.OwnerDoctorId != actorDoctorId)
                throw new PrescriptionTemplateForbiddenException(PenolakanBukanAtasNamaSendiri);

            EnsureNotEmpty(request.Items, request.Compounds);
            await ValidateTemplateContentAsync(request.Items, request.Compounds, cancellationToken);

            var now = DateTime.UtcNow;
            entity.TemplateName = request.TemplateName.Trim();
            entity.TemplateCategory = Normalize(request.TemplateCategory);
            entity.Description = Normalize(request.Description);
            entity.IsShared = !IsInpatientContext(request.ServiceContext) && request.IsShared;
            entity.IsFavorite = request.IsFavorite;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            foreach (var item in entity.Items.Where(x => !x.IsDelete)) SoftDelete(item, actorUserId, now);
            foreach (var compound in entity.Compounds.Where(x => !x.IsDelete))
            {
                foreach (var item in compound.Items.Where(x => !x.IsDelete)) SoftDelete(item, actorUserId, now);
                SoftDelete(compound, actorUserId, now);
            }

            ApplyGraph(entity, request.Items, request.Compounds, actorUserId, now);
            RebuildCounts(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        /// <summary>
        /// Menghapus template — hanya pemiliknya (<c>VAL-DOK-56a</c>). Mengembalikan <c>false</c> bila
        /// template tidak ditemukan.
        /// </summary>
        public async Task<bool> DeleteAsync(
            Guid id,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstPrescriptionTemplate>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return false;

            var actorDoctorId = await RequireActorDoctorAsync(user, actorUserId, cancellationToken);

            if (entity.OwnerDoctorId != actorDoctorId)
                throw new PrescriptionTemplateForbiddenException(PenolakanMilikDokterLain);

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<MstPrescriptionTemplate> CreateFromPrescriptionAsync(
            CreateTemplateFromPrescriptionRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var prescription = await _dbContext.Set<PhmPrescription>()
                .Include(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .Include(x => x.Compounds.Where(c => !c.IsDelete && !c.IsCancel && c.IsActive))
                    .ThenInclude(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .FirstOrDefaultAsync(x => x.Id == request.PrescriptionId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Resep tidak ditemukan.");

            // FR-DOK-088. Pemilik tidak lagi diambil dari dokter penulis resep, melainkan dari akun
            // login — CreateAsync yang menentukannya. Resep rawat inap menghasilkan template pribadi.
            var dto = new CreatePrescriptionTemplateRequest
            {
                TemplateName = request.TemplateName,
                TemplateCategory = request.TemplateCategory,
                Description = request.Description,
                OwnerDoctorId = Guid.Empty,
                IsShared = request.IsShared,
                IsFavorite = request.IsFavorite,
                ServiceContext = prescription.InpEpisodeId.HasValue ? InpatientServiceContext : request.ServiceContext,
                Items = prescription.Items.OrderBy(x => x.SortOrder).Select(x => new PrescriptionTemplateItemRequest
                {
                    DrugId = x.DrugId,
                    Dose = x.Dose,
                    DoseUnitMeasurementId = x.DoseUnitMeasurementId,
                    FrequencyCode = x.FrequencyCode,
                    FrequencyText = x.FrequencyText,
                    FrequencyPerDay = x.FrequencyPerDay,
                    DurationValue = x.DurationValue,
                    DurationUnit = x.DurationUnit,
                    IsAsNeeded = x.IsAsNeeded,
                    AdministrationTime = x.AdministrationTime,
                    Signa = x.Signa,
                    AdministrationInstruction = x.AdministrationInstruction,
                    DoctorNote = x.DoctorNote,
                    Quantity = x.Quantity,
                    DispenseUnitMeasurementId = x.DispenseUnitMeasurementId,
                    SortOrder = x.SortOrder
                }).ToList(),
                Compounds = prescription.Compounds.OrderBy(x => x.SortOrder).Select(x => new PrescriptionTemplateCompoundRequest
                {
                    CompoundName = x.CompoundName,
                    CompoundForm = x.CompoundForm,
                    TotalPackage = x.TotalPackage,
                    PackageUnitMeasurementId = x.PackageUnitMeasurementId,
                    DosePerUse = x.DosePerUse,
                    DoseUnitMeasurementId = x.DoseUnitMeasurementId,
                    FrequencyCode = x.FrequencyCode,
                    FrequencyText = x.FrequencyText,
                    FrequencyPerDay = x.FrequencyPerDay,
                    DurationValue = x.DurationValue,
                    DurationUnit = x.DurationUnit,
                    IsAsNeeded = x.IsAsNeeded,
                    AdministrationTime = x.AdministrationTime,
                    Signa = x.Signa,
                    CompoundingInstruction = x.CompoundingInstruction,
                    AdministrationInstruction = x.AdministrationInstruction,
                    DoctorNote = x.DoctorNote,
                    SortOrder = x.SortOrder,
                    Items = x.Items.OrderBy(i => i.SortOrder).Select(i => new PrescriptionTemplateCompoundItemRequest
                    {
                        DrugId = i.DrugId,
                        AmountPerPackage = i.AmountPerPackage,
                        TotalQuantity = i.TotalQuantity,
                        QuantityUnitMeasurementId = i.QuantityUnitMeasurementId,
                        IngredientInstruction = i.IngredientInstruction,
                        SortOrder = i.SortOrder
                    }).ToList()
                }).ToList()
            };

            return await CreateAsync(dto, user, actorUserId, cancellationToken);
        }

        public async Task<ApplyPrescriptionTemplateResponse> ApplyAsync(
            Guid templateId,
            ApplyPrescriptionTemplateRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await _aggregateService.EnsureEditableAsync(request.PrescriptionId, cancellationToken);

            var template = await _dbContext.Set<MstPrescriptionTemplate>()
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.Drug)
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.DoseUnitMeasurement)
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.DispenseUnitMeasurement)
                .Include(x => x.Compounds.Where(c => !c.IsDelete && c.IsActive)).ThenInclude(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.Drug)
                .FirstOrDefaultAsync(x => x.Id == templateId && x.IsActive && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Template resep tidak ditemukan atau tidak aktif.");

            var prescription = await _dbContext.Set<PhmPrescription>()
                .FirstAsync(x => x.Id == request.PrescriptionId && !x.IsDelete, cancellationToken);

            // VAL-DOK-56c. Resep berkonteks rawat inap: hanya dokter, hanya template miliknya sendiri.
            // Perawat tidak tertaut dokter, sehingga berhenti di sini (RWI-DEC-122 butir 3).
            if (prescription.InpEpisodeId.HasValue)
            {
                var actorDoctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

                if (!actorDoctorId.HasValue || template.OwnerDoctorId != actorDoctorId.Value)
                    throw new PrescriptionTemplateForbiddenException(PenolakanPakaiRawatInap);
            }

            var drugIds = template.Items.Select(x => x.DrugId)
                .Concat(template.Compounds.SelectMany(c => c.Items.Select(i => i.DrugId)))
                .ToList();

            var penanda = await PrescriptionSafetyFlagEvaluator.EvaluateAsync(
                _dbContext, prescription.PatientId, drugIds, cancellationToken);

            var now = DateTime.UtcNow;
            var hasilButir = new List<ApplyPrescriptionTemplateItemResult>();
            var addedRegular = 0;
            var addedCompound = 0;
            var addedIngredient = 0;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (request.ReplaceExisting)
                await SoftDeletePrescriptionContentAsync(prescription.Id, actorUserId, now, cancellationToken);

            foreach (var source in template.Items.OrderBy(x => x.SortOrder))
            {
                var flags = new List<string>(penanda.TryGetValue(source.DrugId, out var f) ? f : new List<string>());
                var drug = source.Drug;

                InsuranceCoverageResult? coverage = null;

                if (drug != null && !flags.Contains(PrescriptionSafetyFlags.Unavailable))
                {
                    coverage = await _coverageService.ResolveDrugAsync(prescription.EncounterId, drug.Id, source.Quantity, prescription.PrescriptionDateTime, cancellationToken);
                    if (!coverage.IsValid) flags.Add(PrescriptionSafetyFlags.Unavailable);
                }
                else if (!flags.Contains(PrescriptionSafetyFlags.Unavailable))
                {
                    flags.Add(PrescriptionSafetyFlags.Unavailable);
                }

                // FR-DOK-090. Butir yang obatnya tidak tersedia tidak masuk draft dan tidak menggagalkan
                // butir lain; ia dilaporkan bertanda supaya dokter menggantinya.
                if (drug == null || coverage == null || flags.Contains(PrescriptionSafetyFlags.Unavailable))
                {
                    hasilButir.Add(new ApplyPrescriptionTemplateItemResult
                    {
                        DrugId = source.DrugId,
                        DrugName = drug?.DrugName ?? string.Empty,
                        Flags = flags
                    });
                    continue;
                }

                var item = new PhmPrescriptionItem { Id = Guid.NewGuid(), PrescriptionId = prescription.Id, DrugId = drug.Id, CreateDateTime = now, CreateBy = actorUserId, IsActive = true };
                CopyTemplateItem(item, source, drug, coverage);
                _dbContext.Set<PhmPrescriptionItem>().Add(item);
                addedRegular++;

                hasilButir.Add(new ApplyPrescriptionTemplateItemResult
                {
                    PrescriptionItemId = item.Id,
                    DrugId = drug.Id,
                    DrugName = drug.DrugName,
                    Flags = flags
                });
            }

            foreach (var source in template.Compounds.OrderBy(x => x.SortOrder))
            {
                var bahanTersedia = new List<(MstPrescriptionTemplateCompoundItem Source, MstDrug Drug, InsuranceCoverageResult Coverage, List<string> Flags)>();

                foreach (var sourceItem in source.Items.OrderBy(x => x.SortOrder))
                {
                    var flags = new List<string>(penanda.TryGetValue(sourceItem.DrugId, out var f) ? f : new List<string>());
                    var drug = sourceItem.Drug;

                    if (drug != null && !flags.Contains(PrescriptionSafetyFlags.Unavailable))
                    {
                        var coverage = await _coverageService.ResolveDrugAsync(prescription.EncounterId, drug.Id, sourceItem.TotalQuantity, prescription.PrescriptionDateTime, cancellationToken);

                        if (coverage.IsValid)
                        {
                            bahanTersedia.Add((sourceItem, drug, coverage, flags));
                            continue;
                        }
                    }

                    if (!flags.Contains(PrescriptionSafetyFlags.Unavailable))
                        flags.Add(PrescriptionSafetyFlags.Unavailable);

                    hasilButir.Add(new ApplyPrescriptionTemplateItemResult
                    {
                        DrugId = sourceItem.DrugId,
                        DrugName = drug?.DrugName ?? string.Empty,
                        IsCompoundIngredient = true,
                        CompoundName = source.CompoundName,
                        Flags = flags
                    });
                }

                // Racikan tanpa satu pun bahan yang tersedia tidak dibentuk; bahannya sudah dilaporkan.
                if (bahanTersedia.Count == 0)
                    continue;

                var compound = new PhmPrescriptionCompound
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescription.Id,
                    CompoundName = source.CompoundName,
                    CompoundForm = source.CompoundForm,
                    TotalPackage = source.TotalPackage,
                    PackageUnitMeasurementId = source.PackageUnitMeasurementId,
                    DosePerUse = source.DosePerUse,
                    DoseUnitMeasurementId = source.DoseUnitMeasurementId,
                    FrequencyCode = source.FrequencyCode,
                    FrequencyText = source.FrequencyText,
                    FrequencyPerDay = source.FrequencyPerDay,
                    DurationValue = source.DurationValue,
                    DurationUnit = source.DurationUnit,
                    IsAsNeeded = source.IsAsNeeded,
                    AdministrationTime = source.AdministrationTime,
                    Signa = source.Signa,
                    CompoundingInstruction = source.CompoundingInstruction,
                    AdministrationInstruction = source.AdministrationInstruction,
                    DoctorNote = source.DoctorNote,
                    SortOrder = source.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.Set<PhmPrescriptionCompound>().Add(compound);
                addedCompound++;

                foreach (var bahan in bahanTersedia)
                {
                    var item = new PhmPrescriptionCompoundItem { Id = Guid.NewGuid(), PrescriptionCompoundId = compound.Id, DrugId = bahan.Drug.Id, CreateDateTime = now, CreateBy = actorUserId, IsActive = true };
                    CopyTemplateCompoundItem(item, bahan.Source, bahan.Drug, bahan.Coverage);
                    _dbContext.Set<PhmPrescriptionCompoundItem>().Add(item);
                    addedIngredient++;

                    hasilButir.Add(new ApplyPrescriptionTemplateItemResult
                    {
                        PrescriptionCompoundId = compound.Id,
                        PrescriptionCompoundItemId = item.Id,
                        DrugId = bahan.Drug.Id,
                        DrugName = bahan.Drug.DrugName,
                        IsCompoundIngredient = true,
                        CompoundName = source.CompoundName,
                        Flags = bahan.Flags
                    });
                }
            }

            template.UsageCount += 1;
            template.LastUsedAt = now;
            template.UpdateDateTime = now;
            template.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            var aggregate = await _aggregateService.RebuildAsync(prescription.Id, actorUserId, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ApplyPrescriptionTemplateResponse
            {
                TemplateId = template.Id,
                PrescriptionId = prescription.Id,
                AddedRegularItemCount = addedRegular,
                AddedCompoundCount = addedCompound,
                AddedCompoundIngredientCount = addedIngredient,
                TotalPrice = aggregate.TotalPrice,
                CoveredAmount = aggregate.CoveredAmount,
                PatientPayAmount = aggregate.PatientPayAmount,
                IsNeedApproval = aggregate.IsNeedApproval,
                IsApproved = aggregate.IsApproved,
                HasFlaggedItems = hasilButir.Any(x => x.Flags.Count > 0),
                Items = hasilButir
            };
        }

        /// <summary>Nilai <c>ServiceContext</c> ruang kerja rawat inap.</summary>
        private const string InpatientServiceContext = "Inpatient";

        private static bool IsInpatientContext(string? serviceContext) =>
            string.Equals(serviceContext?.Trim(), InpatientServiceContext, StringComparison.OrdinalIgnoreCase);

        private async Task<Guid> RequireActorDoctorAsync(ClaimsPrincipal? user, Guid actorUserId, CancellationToken cancellationToken)
        {
            // VAL-DOK-56 — akun tanpa tautan dokter tidak dapat menyebut pemilik siapa pun.
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

            if (!doctorId.HasValue || doctorId.Value == Guid.Empty)
                throw new PrescriptionTemplateForbiddenException(PenolakanBukanAtasNamaSendiri);

            return doctorId.Value;
        }

        private static void EnsureNotEmpty(List<PrescriptionTemplateItemRequest>? items, List<PrescriptionTemplateCompoundRequest>? compounds)
        {
            // VAL-DOK-56b, FR-DOK-091.
            if ((items == null || items.Count == 0) && (compounds == null || compounds.Count == 0))
                throw new InvalidOperationException("Template harus berisi sekurang-kurangnya satu obat.");
        }

        private async Task ValidateDoctorAsync(Guid doctorId, CancellationToken ct)
        {
            if (!await _dbContext.Set<MstDoctor>().AnyAsync(x => x.Id == doctorId && x.IsActive && !x.IsDelete, ct))
                throw new InvalidOperationException("Dokter pemilik template tidak ditemukan atau tidak aktif.");
        }

        private async Task ValidateTemplateContentAsync(List<PrescriptionTemplateItemRequest> items, List<PrescriptionTemplateCompoundRequest> compounds, CancellationToken ct)
        {
            var drugIds = items.Select(x => x.DrugId).Concat(compounds.SelectMany(x => x.Items.Select(i => i.DrugId))).Distinct().ToList();
            var validCount = await _dbContext.Set<MstDrug>().CountAsync(x => drugIds.Contains(x.Id) && x.IsActive && x.IsPrescribable && !x.IsDelete, ct);
            if (validCount != drugIds.Count) throw new InvalidOperationException("Terdapat obat template yang tidak ditemukan, tidak aktif, atau tidak dapat diresepkan.");
            if (compounds.Any(c => c.Items.Count == 0)) throw new InvalidOperationException("Setiap racikan template wajib memiliki minimal satu bahan.");
        }

        private void ApplyGraph(MstPrescriptionTemplate template, List<PrescriptionTemplateItemRequest> items, List<PrescriptionTemplateCompoundRequest> compounds, Guid actor, DateTime now)
        {
            foreach (var x in items) template.Items.Add(new MstPrescriptionTemplateItem
            {
                Id = Guid.NewGuid(),
                PrescriptionTemplateId = template.Id,
                DrugId = x.DrugId,
                Dose = x.Dose,
                DoseUnitMeasurementId = x.DoseUnitMeasurementId,
                FrequencyCode = Normalize(x.FrequencyCode),
                FrequencyText = Normalize(x.FrequencyText),
                FrequencyPerDay = x.FrequencyPerDay,
                DurationValue = x.DurationValue,
                DurationUnit = Normalize(x.DurationUnit),
                IsAsNeeded = x.IsAsNeeded,
                AdministrationTime = Normalize(x.AdministrationTime),
                Signa = Normalize(x.Signa),
                AdministrationInstruction = Normalize(x.AdministrationInstruction),
                DoctorNote = Normalize(x.DoctorNote),
                Quantity = x.Quantity,
                DispenseUnitMeasurementId = x.DispenseUnitMeasurementId,
                SortOrder = x.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor
            });
            foreach (var x in compounds)
            {
                var c = new MstPrescriptionTemplateCompound
                {
                    Id = Guid.NewGuid(),
                    PrescriptionTemplateId = template.Id,
                    CompoundName = x.CompoundName.Trim(),
                    CompoundForm = Normalize(x.CompoundForm),
                    TotalPackage = x.TotalPackage,
                    PackageUnitMeasurementId = x.PackageUnitMeasurementId,
                    DosePerUse = x.DosePerUse,
                    DoseUnitMeasurementId = x.DoseUnitMeasurementId,
                    FrequencyCode = Normalize(x.FrequencyCode),
                    FrequencyText = Normalize(x.FrequencyText),
                    FrequencyPerDay = x.FrequencyPerDay,
                    DurationValue = x.DurationValue,
                    DurationUnit = Normalize(x.DurationUnit),
                    IsAsNeeded = x.IsAsNeeded,
                    AdministrationTime = Normalize(x.AdministrationTime),
                    Signa = Normalize(x.Signa),
                    CompoundingInstruction = Normalize(x.CompoundingInstruction),
                    AdministrationInstruction = Normalize(x.AdministrationInstruction),
                    DoctorNote = Normalize(x.DoctorNote),
                    SortOrder = x.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actor
                };
                foreach (var i in x.Items) c.Items.Add(new MstPrescriptionTemplateCompoundItem
                {
                    Id = Guid.NewGuid(),
                    PrescriptionTemplateCompoundId = c.Id,
                    DrugId = i.DrugId,
                    AmountPerPackage = i.AmountPerPackage,
                    TotalQuantity = i.TotalQuantity,
                    QuantityUnitMeasurementId = i.QuantityUnitMeasurementId,
                    IngredientInstruction = Normalize(i.IngredientInstruction),
                    SortOrder = i.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actor
                });
                template.Compounds.Add(c);
            }
        }

        private static void RebuildCounts(MstPrescriptionTemplate x)
        {
            x.RegularItemCount = x.Items.Count(i => !i.IsDelete && i.IsActive);
            x.CompoundCount = x.Compounds.Count(c => !c.IsDelete && c.IsActive);
            x.CompoundIngredientCount = x.Compounds.Where(c => !c.IsDelete && c.IsActive).Sum(c => c.Items.Count(i => !i.IsDelete && i.IsActive));
            x.TotalItemCount = x.RegularItemCount + x.CompoundIngredientCount;
        }

        private async Task<string> GenerateCodeAsync(DateTime now, CancellationToken ct)
        {
            var prefix = $"RXT-{now:yyyyMMdd}";
            var count = await _dbContext.Set<MstPrescriptionTemplate>().CountAsync(x => x.TemplateCode.StartsWith(prefix), ct);
            return $"{prefix}-{count + 1:D5}";
        }

        private async Task SoftDeletePrescriptionContentAsync(Guid prescriptionId, Guid actor, DateTime now, CancellationToken ct)
        {
            var items = await _dbContext.Set<PhmPrescriptionItem>().Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete).ToListAsync(ct);
            foreach (var x in items) SoftDelete(x, actor, now);
            var compounds = await _dbContext.Set<PhmPrescriptionCompound>().Include(x => x.Items).Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete).ToListAsync(ct);
            foreach (var c in compounds) { foreach (var i in c.Items.Where(x => !x.IsDelete)) SoftDelete(i, actor, now); SoftDelete(c, actor, now); }
        }

        private static void CopyTemplateItem(PhmPrescriptionItem e, MstPrescriptionTemplateItem s, MstDrug d, InsuranceCoverageResult c)
        {
            e.DrugCodeSnapshot = d.DrugCode; e.DrugNameSnapshot = d.DrugName; e.GenericNameSnapshot = d.GenericName; e.DrugCategoryNameSnapshot = d.DrugCategory?.DrugCategoryName;
            e.DrugFormSnapshot = d.DrugForm; e.StrengthSnapshot = d.Strength; e.RouteSnapshot = d.Route; e.IsFormularySnapshot = d.IsFormulary; e.IsGenericSnapshot = d.IsGeneric;
            e.IsAntibioticSnapshot = d.IsAntibiotic; e.IsNarcoticSnapshot = d.IsNarcotic; e.IsPsychotropicSnapshot = d.IsPsychotropic; e.IsHighAlertSnapshot = d.IsHighAlert;
            e.Dose = s.Dose; e.DoseUnitMeasurementId = s.DoseUnitMeasurementId; e.DoseUnitNameSnapshot = s.DoseUnitMeasurement?.MeasurementName; e.DoseUnitSymbolSnapshot = s.DoseUnitMeasurement?.MeasurementSymbol;
            e.FrequencyCode = s.FrequencyCode; e.FrequencyText = s.FrequencyText; e.FrequencyPerDay = s.FrequencyPerDay; e.DurationValue = s.DurationValue; e.DurationUnit = s.DurationUnit;
            e.IsAsNeeded = s.IsAsNeeded; e.AdministrationTime = s.AdministrationTime; e.Signa = s.Signa; e.AdministrationInstruction = s.AdministrationInstruction; e.DoctorNote = s.DoctorNote;
            e.Quantity = s.Quantity; e.DispenseUnitMeasurementId = s.DispenseUnitMeasurementId; e.DispenseUnitNameSnapshot = s.DispenseUnitMeasurement?.MeasurementName; e.DispenseUnitSymbolSnapshot = s.DispenseUnitMeasurement?.MeasurementSymbol; e.SortOrder = s.SortOrder;
            ApplyCoverage(e, c);
        }

        private static void CopyTemplateCompoundItem(PhmPrescriptionCompoundItem e, MstPrescriptionTemplateCompoundItem s, MstDrug d, InsuranceCoverageResult c)
        {
            e.DrugCodeSnapshot = d.DrugCode; e.DrugNameSnapshot = d.DrugName; e.GenericNameSnapshot = d.GenericName; e.DrugCategoryNameSnapshot = d.DrugCategory?.DrugCategoryName;
            e.DrugFormSnapshot = d.DrugForm; e.StrengthSnapshot = d.Strength; e.RouteSnapshot = d.Route; e.IsFormularySnapshot = d.IsFormulary; e.IsGenericSnapshot = d.IsGeneric;
            e.IsAntibioticSnapshot = d.IsAntibiotic; e.IsNarcoticSnapshot = d.IsNarcotic; e.IsPsychotropicSnapshot = d.IsPsychotropic; e.IsHighAlertSnapshot = d.IsHighAlert;
            e.AmountPerPackage = s.AmountPerPackage; e.TotalQuantity = s.TotalQuantity; e.QuantityUnitMeasurementId = s.QuantityUnitMeasurementId;
            e.QuantityUnitNameSnapshot = s.QuantityUnitMeasurement?.MeasurementName; e.QuantityUnitSymbolSnapshot = s.QuantityUnitMeasurement?.MeasurementSymbol;
            e.IngredientInstruction = s.IngredientInstruction; e.SortOrder = s.SortOrder; ApplyCoverage(e, c);
        }

        private static void ApplyCoverage(PhmPrescriptionItem e, InsuranceCoverageResult c)
        { e.TariffId = c.TariffId; e.InsuranceTariffId = c.InsuranceTariffId; e.InsuranceCoverageRuleId = c.InsuranceCoverageRuleId; e.HospitalUnitPrice = c.HospitalUnitPrice; e.ContractUnitPrice = c.ContractUnitPrice; e.UnitPrice = c.UnitPrice; e.TotalPrice = c.TotalPrice; e.PricingSource = c.PricingSource; e.IsCoverageApplicable = c.IsCoverageApplicable; e.IsCoveredByInsurance = c.IsCovered; e.CoverageStatus = c.CoverageStatus; e.CoveragePercent = c.CoveragePercent; e.CoveredAmount = c.CoveredAmount; e.PatientPayAmount = c.PatientPayAmount; e.CoPaymentAmount = c.CoPaymentAmount; e.IsNeedApproval = c.IsNeedApproval; e.IsApproved = !c.IsNeedApproval; e.IsNeedGuaranteeLetter = c.IsNeedGuaranteeLetter; e.IsAllowExcessPaymentByPatient = c.IsAllowExcessPaymentByPatient; e.CoverageNote = c.CoverageNote; }
        private static void ApplyCoverage(PhmPrescriptionCompoundItem e, InsuranceCoverageResult c)
        { e.TariffId = c.TariffId; e.InsuranceTariffId = c.InsuranceTariffId; e.InsuranceCoverageRuleId = c.InsuranceCoverageRuleId; e.HospitalUnitPrice = c.HospitalUnitPrice; e.ContractUnitPrice = c.ContractUnitPrice; e.UnitPrice = c.UnitPrice; e.TotalPrice = c.TotalPrice; e.PricingSource = c.PricingSource; e.IsCoverageApplicable = c.IsCoverageApplicable; e.IsCoveredByInsurance = c.IsCovered; e.CoverageStatus = c.CoverageStatus; e.CoveragePercent = c.CoveragePercent; e.CoveredAmount = c.CoveredAmount; e.PatientPayAmount = c.PatientPayAmount; e.CoPaymentAmount = c.CoPaymentAmount; e.IsNeedApproval = c.IsNeedApproval; e.IsApproved = !c.IsNeedApproval; e.IsNeedGuaranteeLetter = c.IsNeedGuaranteeLetter; e.IsAllowExcessPaymentByPatient = c.IsAllowExcessPaymentByPatient; e.CoverageNote = c.CoverageNote; }

        private static void SoftDelete(IdentityModel x, Guid actor, DateTime now) { x.IsDelete = true; x.DeleteDateTime = now; x.DeleteBy = actor; x.UpdateDateTime = now; x.UpdateBy = actor; }
        private static string? Normalize(string? x) => string.IsNullOrWhiteSpace(x) ? null : x.Trim();
    }
}
