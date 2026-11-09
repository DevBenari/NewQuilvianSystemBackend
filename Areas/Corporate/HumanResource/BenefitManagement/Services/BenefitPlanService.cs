using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Layanan administrasi master paket manfaat 3-lapisan, matriks kelayakan grade staf RS, dan evaluasi eligibilitas terpadu (BE-HRD-177, BE-HRD-178, BE-HRD-180, HRD-DEC-134, FR-BEN-01 s.d. FR-BEN-05).
    /// </summary>
    public class BenefitPlanService : IBenefitPlanService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BenefitPlanService> _logger;

        public BenefitPlanService(
            ApplicationDbContext dbContext,
            ILogger<BenefitPlanService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<BenefitPlanResponseDto>> GetBenefitPlansAsync(
            BenefitPlanFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.MstBenefitPlans
                .AsNoTracking()
                .Include(p => p.BenefitType)
                .Include(p => p.GradeEligibilities.Where(ge => !ge.IsDelete))
                .Where(p => !p.IsDelete);

            if (!string.IsNullOrWhiteSpace(filter.BenefitCategory))
                query = query.Where(p => p.BenefitCategory == filter.BenefitCategory);

            if (!string.IsNullOrWhiteSpace(filter.CoverageTier))
                query = query.Where(p => p.CoverageTier == filter.CoverageTier);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string s = filter.Search.Trim().ToLower();
                query = query.Where(p => p.BenefitPlanCode.ToLower().Contains(s) ||
                                         p.BenefitPlanName.ToLower().Contains(s) ||
                                         (p.ProviderName != null && p.ProviderName.ToLower().Contains(s)));
            }

            var plans = await query.OrderBy(p => p.SortOrder).ThenBy(p => p.BenefitPlanCode).ToListAsync(cancellationToken);
            return plans.Select(MapToResponseDto).ToList();
        }

        public async Task<BenefitPlanResponseDto?> GetBenefitPlanByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var plan = await _dbContext.MstBenefitPlans
                .AsNoTracking()
                .Include(p => p.BenefitType)
                .Include(p => p.GradeEligibilities.Where(ge => !ge.IsDelete))
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            return plan == null ? null : MapToResponseDto(plan);
        }

        public async Task<BenefitPlanResponseDto> CreateBenefitPlanAsync(
            CreateBenefitPlanRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            bool exists = await _dbContext.MstBenefitPlans
                .AnyAsync(p => p.BenefitPlanCode == request.BenefitPlanCode && !p.IsDelete, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"Kode paket manfaat '{request.BenefitPlanCode}' sudah terdaftar dalam sistem.");
            }

            var plan = new MstBenefitPlan
            {
                Id = Guid.NewGuid(),
                BenefitTypeId = request.BenefitTypeId,
                LegalEntityId = request.LegalEntityId,
                HospitalSiteId = request.HospitalSiteId,
                BenefitPlanCode = request.BenefitPlanCode.Trim().ToUpperInvariant(),
                BenefitPlanName = request.BenefitPlanName.Trim(),
                BenefitCategory = request.BenefitCategory,
                CoverageTier = request.CoverageTier,
                ProviderName = request.ProviderName,
                ExternalPlanCode = request.ExternalPlanCode,
                PolicyNumber = request.PolicyNumber,
                CoverageType = request.CoverageType,
                CoverageLimitAmount = request.CoverageLimitAmount,
                HospitalDiscountPercentage = request.HospitalDiscountPercentage,
                RequiresBPJSPrimaryCOB = request.RequiresBPJSPrimaryCOB,
                MinimumServiceMonths = request.MinimumServiceMonths,
                Description = request.Description,
                IsDefault = request.IsDefault,
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.MstBenefitPlans.AddAsync(plan, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Master paket manfaat {PlanCode} ({PlanName}) berhasil dibuat oleh user {UserId}.",
                plan.BenefitPlanCode, plan.BenefitPlanName, userId);

            return MapToResponseDto(plan);
        }

        public async Task<BenefitPlanResponseDto> UpdateBenefitPlanAsync(
            Guid id,
            UpdateBenefitPlanRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var plan = await _dbContext.MstBenefitPlans
                .Include(p => p.BenefitType)
                .Include(p => p.GradeEligibilities.Where(ge => !ge.IsDelete))
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Paket manfaat dengan ID {id} tidak ditemukan.");
            }

            plan.BenefitPlanName = request.BenefitPlanName.Trim();
            plan.BenefitCategory = request.BenefitCategory;
            plan.CoverageTier = request.CoverageTier;
            plan.ProviderName = request.ProviderName;
            plan.CoverageLimitAmount = request.CoverageLimitAmount;
            plan.HospitalDiscountPercentage = request.HospitalDiscountPercentage;
            plan.RequiresBPJSPrimaryCOB = request.RequiresBPJSPrimaryCOB;
            plan.MinimumServiceMonths = request.MinimumServiceMonths;
            plan.Description = request.Description;
            plan.IsActive = request.IsActive;
            plan.UpdateDateTime = DateTime.UtcNow;
            plan.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Master paket manfaat {PlanCode} ({PlanName}) berhasil diperbarui oleh user {UserId}.",
                plan.BenefitPlanCode, plan.BenefitPlanName, userId);

            return MapToResponseDto(plan);
        }

        public async Task<List<BenefitGradeEligibilityResponseDto>> GetGradeEligibilitiesAsync(
            Guid? benefitPlanId,
            string? gradeLevelCode,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.HrdBenefitGradeEligibilities
                .AsNoTracking()
                .Include(ge => ge.BenefitPlan)
                .Where(ge => !ge.IsDelete);

            if (benefitPlanId.HasValue)
                query = query.Where(ge => ge.BenefitPlanId == benefitPlanId.Value);

            if (!string.IsNullOrWhiteSpace(gradeLevelCode))
                query = query.Where(ge => ge.GradeLevelCode == gradeLevelCode);

            var list = await query.OrderBy(ge => ge.GradeLevelCode).ToListAsync(cancellationToken);
            return list.Select(MapGradeEligibilityToDto).ToList();
        }

        public async Task<BenefitGradeEligibilityResponseDto> AddGradeEligibilityAsync(
            CreateBenefitGradeEligibilityRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var plan = await _dbContext.MstBenefitPlans
                .FirstOrDefaultAsync(p => p.Id == request.BenefitPlanId && !p.IsDelete, cancellationToken);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Paket manfaat dengan ID {request.BenefitPlanId} tidak ditemukan.");
            }

            bool exists = await _dbContext.HrdBenefitGradeEligibilities
                .AnyAsync(ge => ge.BenefitPlanId == request.BenefitPlanId &&
                                ge.GradeLevelCode == request.GradeLevelCode &&
                                ge.EmploymentTypeCode == request.EmploymentTypeCode &&
                                !ge.IsDelete, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"Aturan kelayakan untuk Paket '{plan.BenefitPlanCode}' dan Grade '{request.GradeLevelCode}' ({request.EmploymentTypeCode}) sudah ada.");
            }

            var eligibility = new HrdBenefitGradeEligibility
            {
                Id = Guid.NewGuid(),
                BenefitPlanId = request.BenefitPlanId,
                GradeLevelCode = request.GradeLevelCode.Trim().ToUpperInvariant(),
                EmploymentTypeCode = request.EmploymentTypeCode.Trim(),
                SpecificAnnualLimit = request.SpecificAnnualLimit,
                InpatientRoomClass = request.InpatientRoomClass.Trim(),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdBenefitGradeEligibilities.AddAsync(eligibility, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Aturan kelayakan grade {Grade} ({EmploymentType}) untuk paket {PlanCode} berhasil ditambahkan.",
                eligibility.GradeLevelCode, eligibility.EmploymentTypeCode, plan.BenefitPlanCode);

            eligibility.BenefitPlan = plan;
            return MapGradeEligibilityToDto(eligibility);
        }

        public async Task<BenefitGradeEligibilityResponseDto> UpdateGradeEligibilityAsync(
            Guid id,
            UpdateBenefitGradeEligibilityRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var eligibility = await _dbContext.HrdBenefitGradeEligibilities
                .Include(ge => ge.BenefitPlan)
                .FirstOrDefaultAsync(ge => ge.Id == id && !ge.IsDelete, cancellationToken);

            if (eligibility == null)
            {
                throw new KeyNotFoundException($"Aturan kelayakan grade dengan ID {id} tidak ditemukan.");
            }

            eligibility.SpecificAnnualLimit = request.SpecificAnnualLimit;
            eligibility.InpatientRoomClass = request.InpatientRoomClass.Trim();
            eligibility.IsActive = request.IsActive;
            eligibility.UpdateDateTime = DateTime.UtcNow;
            eligibility.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Aturan kelayakan grade {Grade} ({Id}) berhasil diperbarui.", eligibility.GradeLevelCode, id);
            return MapGradeEligibilityToDto(eligibility);
        }

        public async Task<bool> DeleteGradeEligibilityAsync(
            Guid id,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var eligibility = await _dbContext.HrdBenefitGradeEligibilities
                .FirstOrDefaultAsync(ge => ge.Id == id && !ge.IsDelete, cancellationToken);

            if (eligibility == null)
            {
                throw new KeyNotFoundException($"Aturan kelayakan grade dengan ID {id} tidak ditemukan.");
            }

            eligibility.IsDelete = true;
            eligibility.DeleteDateTime = DateTime.UtcNow;
            eligibility.DeleteBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Aturan kelayakan grade {Id} dinonaktifkan/dihapus oleh user {UserId}.", id, userId);
            return true;
        }

        public async Task<int> SeedDefaultHospitalBenefitPlansAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            // Pastikan master BenefitType default ada
            var defaultType = await _dbContext.MstBenefitTypes
                .FirstOrDefaultAsync(t => !t.IsDelete, cancellationToken);

            if (defaultType == null)
            {
                defaultType = new MstBenefitType
                {
                    Id = Guid.NewGuid(),
                    BenefitTypeCode = "HLTH-INS",
                    BenefitTypeName = "Kesehatan & Asuransi RS",
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.MstBenefitTypes.AddAsync(defaultType, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            int addedCount = 0;

            // 1. BEN-BPJS-STD: Wajib Pemerintah
            var planBpjs = await _dbContext.MstBenefitPlans
                .FirstOrDefaultAsync(p => p.BenefitPlanCode == "BEN-BPJS-STD" && !p.IsDelete, cancellationToken);

            if (planBpjs == null)
            {
                planBpjs = new MstBenefitPlan
                {
                    Id = Guid.NewGuid(),
                    BenefitTypeId = defaultType.Id,
                    BenefitPlanCode = "BEN-BPJS-STD",
                    BenefitPlanName = "Paket Wajib Regulasi BPJS (Ketenagakerjaan & Kesehatan PPU)",
                    BenefitCategory = "BPJS_Mandatory",
                    CoverageTier = "Basic",
                    ProviderName = "BPJS Ketenagakerjaan & BPJS Kesehatan",
                    CoverageType = "Family",
                    CoverageLimitAmount = 0m, // Tanpa batas pagu (sesuai tarif INA-CBGs)
                    HospitalDiscountPercentage = 0m,
                    RequiresBPJSPrimaryCOB = false,
                    MinimumServiceMonths = 0, // Aktif H+0 Onboarding untuk JKK & JKM
                    Description = "Paket perlindungan dasar jaminan sosial ketenagakerjaan dan kesehatan wajib bagi seluruh staf rumah sakit.",
                    IsDefault = true,
                    SortOrder = 1,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.MstBenefitPlans.AddAsync(planBpjs, cancellationToken);
                addedCount++;
            }

            // 2. BEN-PVT-EXEC: Asuransi Swasta Tambahan Executive
            var planExec = await _dbContext.MstBenefitPlans
                .FirstOrDefaultAsync(p => p.BenefitPlanCode == "BEN-PVT-EXEC" && !p.IsDelete, cancellationToken);

            if (planExec == null)
            {
                planExec = new MstBenefitPlan
                {
                    Id = Guid.NewGuid(),
                    BenefitTypeId = defaultType.Id,
                    BenefitPlanCode = "BEN-PVT-EXEC",
                    BenefitPlanName = "Asuransi Swasta Tambahan Executive (Direksi & Dokter Spesialis)",
                    BenefitCategory = "Private_Insurance",
                    CoverageTier = "Executive",
                    ProviderName = "Prudential / Mandiri Inhealth Executive",
                    CoverageType = "Family",
                    CoverageLimitAmount = 50000000m, // Pagu tambahan Rp 50jt/thn
                    HospitalDiscountPercentage = 0m,
                    RequiresBPJSPrimaryCOB = true, // Mandatory COB BPJS (BEN-B5)
                    MinimumServiceMonths = 3,
                    Description = "Asuransi komersial tambahan VIP/VVIP untuk Direksi dan Dokter Spesialis dengan skema koordinasi manfaat (COB).",
                    IsDefault = false,
                    SortOrder = 2,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.MstBenefitPlans.AddAsync(planExec, cancellationToken);
                addedCount++;
            }

            // 3. BEN-PVT-STAFF: Asuransi Swasta Tambahan Staf Klinis & Penunjang
            var planStaff = await _dbContext.MstBenefitPlans
                .FirstOrDefaultAsync(p => p.BenefitPlanCode == "BEN-PVT-STAFF" && !p.IsDelete, cancellationToken);

            if (planStaff == null)
            {
                planStaff = new MstBenefitPlan
                {
                    Id = Guid.NewGuid(),
                    BenefitTypeId = defaultType.Id,
                    BenefitPlanCode = "BEN-PVT-STAFF",
                    BenefitPlanName = "Asuransi Swasta Tambahan Staf (Klinis & Penunjang Medis)",
                    BenefitCategory = "Private_Insurance",
                    CoverageTier = "Silver",
                    ProviderName = "Mandiri Inhealth Silver",
                    CoverageType = "Family",
                    CoverageLimitAmount = 20000000m, // Pagu tambahan Rp 20jt/thn
                    HospitalDiscountPercentage = 0m,
                    RequiresBPJSPrimaryCOB = true,
                    MinimumServiceMonths = 3,
                    Description = "Asuransi komersial tambahan Kelas 1 untuk perawat, bidan, radiografer, analis lab, dan penunjang medis.",
                    IsDefault = false,
                    SortOrder = 3,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.MstBenefitPlans.AddAsync(planStaff, cancellationToken);
                addedCount++;
            }

            // 4. BEN-INH-SUBSIDY: Subsidi Berobat Internal RS Sendiri
            var planInhouse = await _dbContext.MstBenefitPlans
                .FirstOrDefaultAsync(p => p.BenefitPlanCode == "BEN-INH-SUBSIDY" && !p.IsDelete, cancellationToken);

            if (planInhouse == null)
            {
                planInhouse = new MstBenefitPlan
                {
                    Id = Guid.NewGuid(),
                    BenefitTypeId = defaultType.Id,
                    BenefitPlanCode = "BEN-INH-SUBSIDY",
                    BenefitPlanName = "Subsidi Fasilitas Berobat Internal RS Quilvian",
                    BenefitCategory = "InHouse_HospitalCare",
                    CoverageTier = "Gold",
                    ProviderName = "Rumah Sakit Quilvian",
                    CoverageType = "Family",
                    CoverageLimitAmount = 10000000m,
                    HospitalDiscountPercentage = 30.00m, // Diskon 30% jasa medis staf RS (FR-BEN-02)
                    RequiresBPJSPrimaryCOB = false,
                    MinimumServiceMonths = 3,
                    Description = "Fasilitas subsidi berobat langsung di poliklinik dan rawat inap RS Quilvian dengan diskon tarif staf dan plafon tahunan non-negatif.",
                    IsDefault = true,
                    SortOrder = 4,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.MstBenefitPlans.AddAsync(planInhouse, cancellationToken);
                addedCount++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Seeding Matriks Grade Eligibilities jika belum ada
            await SeedGradeEligibilitiesIfMissing(planBpjs, planExec, planStaff, planInhouse, userId, cancellationToken);

            _logger.LogInformation("Penyemaian default master paket manfaat RS selesai. {Count} paket diproses.", addedCount);
            return addedCount;
        }

        private async Task SeedGradeEligibilitiesIfMissing(
            MstBenefitPlan bpjs,
            MstBenefitPlan exec,
            MstBenefitPlan staff,
            MstBenefitPlan inhouse,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var existingGe = await _dbContext.HrdBenefitGradeEligibilities
                .Where(ge => !ge.IsDelete)
                .ToListAsync(cancellationToken);

            void EnsureGe(Guid planId, string grade, string empType, decimal? limit, string roomClass)
            {
                if (!existingGe.Any(g => g.BenefitPlanId == planId && g.GradeLevelCode == grade && g.EmploymentTypeCode == empType))
                {
                    _dbContext.HrdBenefitGradeEligibilities.Add(new HrdBenefitGradeEligibility
                    {
                        Id = Guid.NewGuid(),
                        BenefitPlanId = planId,
                        GradeLevelCode = grade,
                        EmploymentTypeCode = empType,
                        SpecificAnnualLimit = limit,
                        InpatientRoomClass = roomClass,
                        IsActive = true,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = userId
                    });
                }
            }

            // BPJS Wajib: Berlaku untuk semua grade
            EnsureGe(bpjs.Id, "GRADE-ALL", "All", null, "Kelas 1");

            // Executive: Khusus Grade-I (Dokter Spesialis / Direksi)
            EnsureGe(exec.Id, "GRADE-I", "Permanent", 50000000m, "VVIP");
            EnsureGe(exec.Id, "GRADE-I", "Contract", 40000000m, "VIP");

            // Staff Swasta: Grade-II, Grade-III, Grade-IV
            EnsureGe(staff.Id, "GRADE-II", "Permanent", 20000000m, "Kelas 1");
            EnsureGe(staff.Id, "GRADE-II", "Contract", 15000000m, "Kelas 1");
            EnsureGe(staff.Id, "GRADE-III", "Permanent", 15000000m, "Kelas 1");
            EnsureGe(staff.Id, "GRADE-IV", "Permanent", 10000000m, "Kelas 2");

            // Subsidi Berobat RS: Seluruh Grade
            EnsureGe(inhouse.Id, "GRADE-I", "Permanent", 15000000m, "VVIP");
            EnsureGe(inhouse.Id, "GRADE-II", "Permanent", 10000000m, "Kelas 1");
            EnsureGe(inhouse.Id, "GRADE-III", "Permanent", 7500000m, "Kelas 1");
            EnsureGe(inhouse.Id, "GRADE-IV", "Permanent", 5000000m, "Kelas 2");

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<EmployeeBenefitEligibilityEvaluationResultDto> EvaluateEmployeePlanEligibilityAsync(
            EvaluateEmployeeBenefitEligibilityRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var emp = await _dbContext.MstEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDelete, cancellationToken);

            if (emp == null)
            {
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");
            }

            int serviceMonths = request.SimulatedServiceMonths ??
                Math.Max(0, (int)((DateTime.UtcNow - emp.JoinDate).TotalDays / 30.4375));

            bool isProbation = serviceMonths < 3;
            string empGrade = "GRADE-II"; // Default perawat jika profil grade belum terspesifikasi
            string empType = "Permanent";

            var allPlans = await _dbContext.MstBenefitPlans
                .AsNoTracking()
                .Include(p => p.GradeEligibilities.Where(ge => !ge.IsDelete))
                .Where(p => p.IsActive && !p.IsDelete)
                .OrderBy(p => p.SortOrder)
                .ToListAsync(cancellationToken);

            var eligible = new List<EligiblePlanSummaryDto>();
            var ineligible = new List<IneligiblePlanSummaryDto>();

            foreach (var plan in allPlans)
            {
                // Aturan BEN-B1: BPJS Mandatory aktif H+0 Onboarding
                if (plan.BenefitCategory == "BPJS_Mandatory")
                {
                    eligible.Add(new EligiblePlanSummaryDto
                    {
                        PlanId = plan.Id,
                        PlanCode = plan.BenefitPlanCode,
                        PlanName = plan.BenefitPlanName,
                        BenefitCategory = plan.BenefitCategory,
                        CoverageTier = plan.CoverageTier,
                        AnnualLimitAmount = plan.CoverageLimitAmount,
                        InpatientRoomClass = "Kelas 1",
                        HospitalDiscountPercentage = plan.HospitalDiscountPercentage,
                        RequiresBPJSPrimaryCOB = plan.RequiresBPJSPrimaryCOB,
                        IsActive = true
                    });
                    continue;
                }

                // Cek masa tunggu probation (BEN-B1, VR-BEN-12)
                if (serviceMonths < plan.MinimumServiceMonths)
                {
                    ineligible.Add(new IneligiblePlanSummaryDto
                    {
                        PlanId = plan.Id,
                        PlanCode = plan.BenefitPlanCode,
                        PlanName = plan.BenefitPlanName,
                        IneligibilityReason = $"Dalam masa percobaan ({serviceMonths}/{plan.MinimumServiceMonths} bulan). Paket ditahan hingga kelulusan probation (BEN-B1)."
                    });
                    continue;
                }

                // Cek kelayakan grade
                var matchedGe = plan.GradeEligibilities.FirstOrDefault(ge =>
                    (ge.GradeLevelCode == empGrade || ge.GradeLevelCode == "GRADE-ALL") &&
                    (ge.EmploymentTypeCode == empType || ge.EmploymentTypeCode == "All") &&
                    ge.IsActive);

                if (matchedGe == null)
                {
                    ineligible.Add(new IneligiblePlanSummaryDto
                    {
                        PlanId = plan.Id,
                        PlanCode = plan.BenefitPlanCode,
                        PlanName = plan.BenefitPlanName,
                        IneligibilityReason = $"Grade kepegawaian staf ({empGrade}) tidak termasuk dalam matriks kelayakan paket ini."
                    });
                    continue;
                }

                decimal? effectiveLimit = matchedGe.SpecificAnnualLimit ?? plan.CoverageLimitAmount;

                eligible.Add(new EligiblePlanSummaryDto
                {
                    PlanId = plan.Id,
                    PlanCode = plan.BenefitPlanCode,
                    PlanName = plan.BenefitPlanName,
                    BenefitCategory = plan.BenefitCategory,
                    CoverageTier = plan.CoverageTier,
                    AnnualLimitAmount = effectiveLimit,
                    InpatientRoomClass = matchedGe.InpatientRoomClass,
                    HospitalDiscountPercentage = plan.HospitalDiscountPercentage,
                    RequiresBPJSPrimaryCOB = plan.RequiresBPJSPrimaryCOB,
                    IsActive = true
                });
            }

            return new EmployeeBenefitEligibilityEvaluationResultDto
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.FullName,
                EmployeeNik = emp.EmployeeNumber,
                GradeLevelCode = empGrade,
                EmploymentTypeCode = empType,
                JoinDate = emp.JoinDate,
                ActualServiceMonths = serviceMonths,
                IsProbation = isProbation,
                EligiblePlans = eligible,
                IneligiblePlans = ineligible,
                Message = isProbation
                    ? $"Pegawai berada dalam masa percobaan ({serviceMonths} bulan). Hanya jaminan dasar BPJS Ketenagakerjaan/Kesehatan yang aktif. Paket asuransi swasta & subsidi RS berstatus ditahan (PendingActivation)."
                    : $"Evaluasi sukses: Pegawai berhak atas {eligible.Count} paket manfaat aktif sesuai grade {empGrade}."
            };
        }

        private static BenefitPlanResponseDto MapToResponseDto(MstBenefitPlan p)
        {
            return new BenefitPlanResponseDto
            {
                Id = p.Id,
                BenefitTypeId = p.BenefitTypeId,
                BenefitTypeName = p.BenefitType?.BenefitTypeName,
                BenefitPlanCode = p.BenefitPlanCode,
                BenefitPlanName = p.BenefitPlanName,
                BenefitCategory = p.BenefitCategory,
                CoverageTier = p.CoverageTier,
                ProviderName = p.ProviderName,
                ExternalPlanCode = p.ExternalPlanCode,
                PolicyNumber = p.PolicyNumber,
                CoverageType = p.CoverageType,
                CurrencyCode = p.CurrencyCode,
                CoverageLimitAmount = p.CoverageLimitAmount,
                HospitalDiscountPercentage = p.HospitalDiscountPercentage,
                RequiresBPJSPrimaryCOB = p.RequiresBPJSPrimaryCOB,
                MinimumServiceMonths = p.MinimumServiceMonths,
                Description = p.Description,
                IsDefault = p.IsDefault,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                GradeEligibilities = p.GradeEligibilities.Select(MapGradeEligibilityToDto).ToList()
            };
        }

        private static BenefitGradeEligibilityResponseDto MapGradeEligibilityToDto(HrdBenefitGradeEligibility ge)
        {
            return new BenefitGradeEligibilityResponseDto
            {
                Id = ge.Id,
                BenefitPlanId = ge.BenefitPlanId,
                BenefitPlanCode = ge.BenefitPlan?.BenefitPlanCode,
                BenefitPlanName = ge.BenefitPlan?.BenefitPlanName,
                GradeLevelCode = ge.GradeLevelCode,
                EmploymentTypeCode = ge.EmploymentTypeCode,
                SpecificAnnualLimit = ge.SpecificAnnualLimit,
                InpatientRoomClass = ge.InpatientRoomClass,
                IsActive = ge.IsActive
            };
        }
    }
}
