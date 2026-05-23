using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Models;
using System.Reflection.Emit;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SysAppVersion> SysAppVersions { get; set; }
        public DbSet<SysApplicationModule> SysApplicationModules { get; set; }
        public DbSet<SysControllerAccess> SysControllerAccesses { get; set; }
        public DbSet<SysActionAccess> SysActionAccesses { get; set; }
        public DbSet<SysAccessPolicy> SysAccessPolicies { get; set; }

        public DbSet<ApplicationUserFingerprintCredential> ApplicationUserFingerprintCredentials { get; set; }
        public DbSet<ApplicationUserOrganization> ApplicationUserOrganizations { get; set; }

        public DbSet<MstCountry> MstCountries { get; set; }
        public DbSet<MstProvince> MstProvinces { get; set; }
        public DbSet<MstCity> MstCities { get; set; }
        public DbSet<MstDistrict> MstDistricts { get; set; }
        public DbSet<MstPostalCode> MstPostalCodes { get; set; }

        public DbSet<MstWorkforceProfile> MstWorkforceProfiles { get; set; }
        public DbSet<MstWorkforceRequirement> MstWorkforceRequirements { get; set; }
        public DbSet<MstDepartment> MstDepartments { get; set; }
        public DbSet<MstPosition> MstPositions { get; set; }
        public DbSet<MstCompetency> MstCompetencies { get; set; }
        public DbSet<MstPositionCompetencyRequirement> MstPositionCompetencyRequirements { get; set; }        

        public DbSet<MstEmployee> MstEmployees { get; set; }        
        public DbSet<MstDoctor> MstDoctors { get; set; }
        public DbSet<MstExternalUser> MstExternalUsers { get; set; }

        public DbSet<MstWorkSchedule> MstWorkSchedules { get; set; }
        public DbSet<EmpAttendance> EmpAttendances { get; set; }        

        public DbSet<WfpOrganizationAssignment> WfpOrganizationAssignments { get; set; }
        public DbSet<WfpBankAccount> WfpBankAccounts { get; set; }
        public DbSet<WfpDocument> WfpDocuments { get; set; }
        public DbSet<WfpEducation> WfpEducations { get; set; }
        public DbSet<WfpTrainingRecord> WfpTrainingRecords { get; set; }
        public DbSet<WfpCertification> WfpCertifications { get; set; }
        public DbSet<WfpCredentialLicense> WfpCredentialLicenses { get; set; }
        public DbSet<WfpClinicalPrivilege> WfpClinicalPrivileges { get; set; }
        public DbSet<WfpHealthRecord> WfpHealthRecords { get; set; }
        public DbSet<WfpTransportAllowancePolicy> WfpTransportAllowancePolicies { get; set; }
        public DbSet<WfpTransportAllowance> WfpTransportAllowances { get; set; }
        public DbSet<WfpTransportAllowanceTransaction> WfpTransportAllowanceTransactions { get; set; }
        public DbSet<WfpPayroll> WfpPayrolls { get; set; }
        public DbSet<WfpTax> WfpTaxes { get; set; }
        public DbSet<WfpInsurance> WfpInsurances { get; set; }       
        public DbSet<WfpWorkScheduleAssignment> WfpWorkScheduleAssignments { get; set; }
        public DbSet<WfpLeaveBalance> WfpLeaveBalances { get; set; }
        public DbSet<WfpLeaveRequest> WfpLeaveRequests { get; set; }
        public DbSet<WfpOvertimeRequest> WfpOvertimeRequests { get; set; }
        public DbSet<WfpOnboardingChecklist> WfpOnboardingChecklists { get; set; }
        public DbSet<WfpOnboardingTask> WfpOnboardingTasks { get; set; }
        public DbSet<WfpOffboardingChecklist> WfpOffboardingChecklists { get; set; }
        public DbSet<WfpOffboardingTask> WfpOffboardingTasks { get; set; }
        public DbSet<WfpEmploymentHistory> WfpEmploymentHistories { get; set; }
        public DbSet<WfpContractHistory> WfpContractHistories { get; set; }
        public DbSet<WfpCompetencyAssessment> WfpCompetencyAssessments { get; set; }
        public DbSet<WfpPerformanceReview> WfpPerformanceReviews { get; set; }
        public DbSet<WfpPerformanceReviewDetail> WfpPerformanceReviewDetails { get; set; }
        public DbSet<WfpDisciplinaryAction> WfpDisciplinaryActions { get; set; }
        public DbSet<WfpComplianceAlert> WfpComplianceAlerts { get; set; }
        public DbSet<WfpComplianceAlertLog> WfpComplianceAlertLogs { get; set; }
        public DbSet<WfpScheduleChangeRequest> WfpScheduleChangeRequests { get; set; }
        public DbSet<WfpShiftSwapRequest> WfpShiftSwapRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.UserCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DisplayName)
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(x => x.UserType)
                    .HasConversion<int>();

                entity.Property(x => x.ProfilePhotoPath)
                    .HasMaxLength(500);

                entity.Property(x => x.GeolocationBypassReason)
                    .HasMaxLength(250);

                entity.Property(x => x.GeolocationBypassUntil)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.EmployeeId)
                    .IsRequired(false);

                entity.Property(x => x.DoctorId)
                    .IsRequired(false);

                entity.Property(x => x.ExternalUserId)
                    .IsRequired(false);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.HasOne(x => x.Employee)
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Doctor)
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ExternalUser)
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(x => x.ExternalUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.UserAccount)
                    .HasForeignKey<ApplicationUser>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryPosition)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserCode)
                    .IsUnique();

                entity.HasIndex(x => x.UserType);

                entity.HasIndex(x => x.EmployeeId)
                    .IsUnique()
                    .HasFilter("\"EmployeeId\" IS NOT NULL");

                entity.HasIndex(x => x.DoctorId)
                    .IsUnique()
                    .HasFilter("\"DoctorId\" IS NOT NULL");

                entity.HasIndex(x => x.ExternalUserId)
                    .IsUnique()
                    .HasFilter("\"ExternalUserId\" IS NOT NULL");

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"WorkforceProfileId\" IS NOT NULL");

                entity.HasIndex(x => new { x.PrimaryDepartmentId, x.PrimaryPositionId });

                entity.HasIndex(x => x.IsActive);

                entity.HasIndex(x => x.IsGeolocationBypassEnabled);

                entity.Property(x => x.IsFingerprintRegistrationEnabled)
                    .HasDefaultValue(false);

                entity.Property(x => x.FingerprintRegistrationReason)
                    .HasMaxLength(250);

                entity.Property(x => x.FingerprintRegistrationEnabledAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.HasIndex(x => x.IsFingerprintRegistrationEnabled);
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsSystemRole)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreateDateTime)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            builder.Entity<SysAppVersion>(entity =>
            {
                entity.ToTable("SysAppVersion", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.AppName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.BackendVersion)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ApiVersion)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.FrontendMinimumVersion)
                    .HasMaxLength(50);

                entity.Property(x => x.FrontendRecommendedVersion)
                    .HasMaxLength(50);

                entity.Property(x => x.ReleaseName)
                    .HasMaxLength(200);

                entity.Property(x => x.IsLatest)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.ReleaseDateTime)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.CreateDateTime)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasIndex(x => x.IsLatest);
                entity.HasIndex(x => x.IsActive);
                entity.HasIndex(x => x.BackendVersion);
                entity.HasIndex(x => x.ApiVersion);
            });

            builder.Entity<SysApplicationModule>(entity =>
            {
                entity.ToTable("SysApplicationModule", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ModuleCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ModuleName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.AreaName)
                    .HasMaxLength(100);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(x => x.ModuleCode)
                    .IsUnique();

                entity.HasIndex(x => x.IsActive);
            });

            builder.Entity<SysControllerAccess>(entity =>
            {
                entity.ToTable("SysControllerAccess", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ControllerName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.DisplayName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.RoutePath)
                    .HasMaxLength(250);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.VisibleInRoleAccess)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsSystemOnly)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.Module)
                    .WithMany(x => x.Controllers)
                    .HasForeignKey(x => x.ModuleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.ModuleId, x.ControllerName })
                    .IsUnique();

                entity.HasIndex(x => x.ControllerName);
                entity.HasIndex(x => x.IsActive);
                entity.HasIndex(x => x.VisibleInRoleAccess);
                entity.HasIndex(x => x.IsSystemOnly);
            });

            builder.Entity<SysActionAccess>(entity =>
            {
                entity.ToTable("SysActionAccess", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ActionName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.DisplayName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.HttpMethod)
                    .HasMaxLength(20);

                entity.Property(x => x.RoutePath)
                    .HasMaxLength(250);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.AccessType)
                    .HasMaxLength(50)
                    .HasDefaultValue("Read");

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.VisibleInRoleAccess)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsSystemOnly)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.ControllerAccess)
                    .WithMany(x => x.Actions)
                    .HasForeignKey(x => x.ControllerAccessId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.ControllerAccessId, x.ActionName })
                    .IsUnique();

                entity.HasIndex(x => x.ActionName);
                entity.HasIndex(x => x.AccessType);
                entity.HasIndex(x => x.IsActive);
                entity.HasIndex(x => x.VisibleInRoleAccess);
                entity.HasIndex(x => x.IsSystemOnly);
            });

            builder.Entity<SysAccessPolicy>(entity =>
            {
                entity.ToTable("SysAccessPolicy", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.IsAllowed)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ControllerAccess)
                    .WithMany()
                    .HasForeignKey(x => x.ControllerAccessId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ActionAccess)
                    .WithMany()
                    .HasForeignKey(x => x.ActionAccessId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.DepartmentId,
                    x.PositionId,
                    x.ControllerAccessId,
                    x.ActionAccessId
                }).IsUnique();

                entity.HasIndex(x => new
                {
                    x.DepartmentId,
                    x.PositionId,
                    x.IsAllowed,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.ControllerAccessId,
                    x.ActionAccessId,
                    x.IsAllowed,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<ApplicationUserFingerprintCredential>(entity =>
            {
                entity.ToTable("AspNetUserFingerprint", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.FingerPosition)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.TemplateFormat)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.TemplateVersion)
                    .HasMaxLength(50);

                entity.Property(x => x.TemplateDataEncrypted)
                    .HasColumnType("bytea")
                    .IsRequired();

                entity.Property(x => x.TemplateHash)
                    .HasMaxLength(128);

                entity.Property(x => x.DeviceId)
                    .HasMaxLength(100);

                entity.Property(x => x.DeviceModel)
                    .HasMaxLength(100);

                entity.Property(x => x.SampleFormat)
                    .HasMaxLength(50);

                entity.Property(x => x.IsPrimary)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.RegisteredAt)
                    .HasColumnType("timestamp with time zone");

                entity.Property(x => x.RegisteredIpAddress)
                    .HasMaxLength(100);

                entity.Property(x => x.RegisteredUserAgent)
                    .HasMaxLength(500);

                entity.Property(x => x.RevokedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RevokedReason)
                    .HasMaxLength(250);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.FingerprintCredentials)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany()
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Doctor)
                    .WithMany()
                    .HasForeignKey(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserId);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.EmployeeId);

                entity.HasIndex(x => x.DoctorId);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.FingerPosition
                })
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.IsPrimary
                })
                .IsUnique()
                .HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

                entity.HasIndex(x => x.TemplateHash);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<ApplicationUserOrganization>(entity =>
            {
                entity.ToTable("AspNetUserOrganization", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.IsPrimary)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.DepartmentPositions)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserId);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.DepartmentId,
                    x.PositionId,
                    x.EffectiveStartDate
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.DepartmentId,
                    x.PositionId
                });

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.DepartmentId,
                    x.PositionId
                }).IsUnique();
            });

            builder.Entity<MstCountry>(entity =>
            {
                entity.ToTable("MstCountry", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.CountryCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.CountryName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.PhoneCode)
                    .HasMaxLength(10);

                entity.Property(x => x.IsDefault)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasIndex(x => x.CountryCode)
                    .IsUnique();

                entity.HasIndex(x => x.CountryName);

                entity.HasIndex(x => new
                {
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstProvince>(entity =>
            {
                entity.ToTable("MstProvince", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.CountryId)
                    .IsRequired();

                entity.Property(x => x.ProvinceCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.ProvinceName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.Country)
                    .WithMany(x => x.Provinces)
                    .HasForeignKey(x => x.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.CountryId,
                    x.ProvinceCode
                }).IsUnique();

                entity.HasIndex(x => new
                {
                    x.CountryId,
                    x.ProvinceName
                });

                entity.HasIndex(x => new
                {
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstCity>(entity =>
            {
                entity.ToTable("MstCity", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ProvinceId)
                    .IsRequired();

                entity.Property(x => x.CityCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.CityName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.CityType)
                    .HasMaxLength(30);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.Province)
                    .WithMany(x => x.Cities)
                    .HasForeignKey(x => x.ProvinceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.ProvinceId,
                    x.CityCode
                }).IsUnique();

                entity.HasIndex(x => new
                {
                    x.ProvinceId,
                    x.CityName
                });

                entity.HasIndex(x => x.CityType);

                entity.HasIndex(x => new
                {
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstDistrict>(entity =>
            {
                entity.ToTable("MstDistrict", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.CityId)
                    .IsRequired();

                entity.Property(x => x.DistrictCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.DistrictName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.City)
                    .WithMany(x => x.Districts)
                    .HasForeignKey(x => x.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.CityId,
                    x.DistrictCode
                }).IsUnique();

                entity.HasIndex(x => new
                {
                    x.CityId,
                    x.DistrictName
                });

                entity.HasIndex(x => new
                {
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstPostalCode>(entity =>
            {
                entity.ToTable("MstPostalCode", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DistrictId)
                    .IsRequired();

                entity.Property(x => x.PostalCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.VillageName)
                    .HasMaxLength(150);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.District)
                    .WithMany(x => x.PostalCodes)
                    .HasForeignKey(x => x.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.PostalCode);

                entity.HasIndex(x => new
                {
                    x.DistrictId,
                    x.PostalCode
                });

                entity.HasIndex(x => new
                {
                    x.DistrictId,
                    x.VillageName
                });

                entity.HasIndex(x => new
                {
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstWorkforceProfile>(entity =>
            {
                entity.ToTable("MstWorkforceProfile", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ProfileCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.UserType)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(x => x.DisplayName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(200);

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(30);

                entity.Property(x => x.WhatsAppNumber)
                    .HasMaxLength(30);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.PrimaryDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryPosition)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ProfileCode)
                    .IsUnique();

                entity.HasIndex(x => x.UserType);

                entity.HasIndex(x => x.DisplayName);

                entity.HasIndex(x => x.Email);

                entity.HasIndex(x => new
                {
                    x.UserType,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstWorkforceRequirement>(entity =>
            {
                entity.ToTable("MstWorkforceRequirement", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserType)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(x => x.RequirementCategory)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.RequirementCode)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.RequirementName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.IsRequired)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsMultipleAllowed)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsFileRequired)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsNumberRequired)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsIssueDateRequired)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsExpiredDateRequired)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsVerificationRequired)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsProfileRequired)
                    .HasDefaultValue(false);

                entity.Property(x => x.TargetEntityName)
                    .HasMaxLength(100);

                entity.Property(x => x.SortOrder)
                    .HasDefaultValue(0);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasIndex(x => new
                {
                    x.UserType,
                    x.RequirementCategory,
                    x.RequirementCode,
                    x.TargetEntityName
                })
                .IsUnique();

                entity.HasIndex(x => new
                {
                    x.UserType,
                    x.RequirementCategory,
                    x.TargetEntityName,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstDepartment>(entity =>
            {
                entity.ToTable("MstDepartment", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DepartmentCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DepartmentName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasIndex(x => x.DepartmentCode)
                    .IsUnique();

                entity.HasIndex(x => x.DepartmentName);
            });

            builder.Entity<MstPosition>(entity =>
            {
                entity.ToTable("MstPosition", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DepartmentId)
                    .IsRequired();

                entity.Property(x => x.PositionCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.PositionName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.Department)
                    .WithMany(x => x.Positions)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.DepartmentId, x.PositionCode })
                    .IsUnique();

                entity.HasIndex(x => new { x.DepartmentId, x.PositionName });
            });

            builder.Entity<WfpOrganizationAssignment>(entity =>
            {
                entity.ToTable("WfpOrganizationAssignment", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsPrimary)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.OrganizationAssignments)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.DepartmentId,
                    x.PositionId,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.IsPrimary
                })
                .HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false")
                .IsUnique();
            });

            builder.Entity<WfpBankAccount>(entity =>
            {
                entity.ToTable("WfpBankAccount", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.BankName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.AccountNumber)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.AccountHolderName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.BankBranch)
                    .HasMaxLength(100);

                entity.Property(x => x.IsPrimary)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.BankAccounts)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.AccountNumber,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.IsPrimary
                })
                .HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false")
                .IsUnique();
            });

            builder.Entity<WfpDocument>(entity =>
            {
                entity.ToTable("WfpDocument", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.RequirementCode)
                    .HasMaxLength(100);

                entity.Property(x => x.DocumentType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.DocumentName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.DocumentNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.IssueDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ExpiredDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.Documents)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.RequirementCode,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.DocumentType,
                    x.DocumentNumber,
                    x.IsDelete
                });
            });

            builder.Entity<WfpEducation>(entity =>
            {
                entity.ToTable("WfpEducation", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.RequirementCode)
                    .HasMaxLength(100);

                entity.Property(x => x.EducationLevel)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.InstitutionName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Major)
                    .HasMaxLength(150);

                entity.Property(x => x.CertificateNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.Educations)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.RequirementCode,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.EducationLevel,
                    x.GraduationYear,
                    x.IsDelete
                });
            });

            builder.Entity<WfpTrainingRecord>(entity =>
            {
                entity.ToTable("WfpTrainingRecord", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.RequirementCode)
                    .HasMaxLength(100);

                entity.Property(x => x.TrainingType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.TrainingName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Organizer)
                    .HasMaxLength(200);

                entity.Property(x => x.Location)
                    .HasMaxLength(200);

                entity.Property(x => x.StartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.CertificateNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.CreditPoint)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.TrainingRecords)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.RequirementCode,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.TrainingType,
                    x.StartDate,
                    x.IsDelete
                });
            });

            builder.Entity<WfpCertification>(entity =>
            {
                entity.ToTable("WfpCertification", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.RequirementCode)
                    .HasMaxLength(100);

                entity.Property(x => x.CertificationType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.CertificationName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Issuer)
                    .HasMaxLength(200);

                entity.Property(x => x.CertificateNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.IssueDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.ExpiredDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.IsLifetime)
                    .HasDefaultValue(false);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.Certifications)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.RequirementCode,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.CertificationName,
                    x.CertificateNumber,
                    x.IsDelete
                });
            });

            builder.Entity<WfpCredentialLicense>(entity =>
            {
                entity.ToTable("WfpCredentialLicense", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.RequirementCode)
                    .HasMaxLength(100);

                entity.Property(x => x.LicenseType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.LicenseNumber)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Issuer)
                    .HasMaxLength(200);

                entity.Property(x => x.IssueDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.ExpiredDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.PracticeLocation)
                    .HasMaxLength(200);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.VerificationStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(CredentialVerificationStatus.Unverified)
                    .IsRequired();

                entity.Property(x => x.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(x => x.VerifiedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.VerifiedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.VerificationNote)
                    .HasMaxLength(250);

                entity.Property(x => x.RejectedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.RejectedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RejectedReason)
                    .HasMaxLength(250);

                entity.Property(x => x.RevokedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.RevokedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RevokedReason)
                    .HasMaxLength(250);

                entity.Property(x => x.IsPrimary)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.CredentialLicenses)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.VerifiedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.VerifiedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.RejectedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RevokedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.RevokedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.VerifiedByUserId);

                entity.HasIndex(x => x.RejectedByUserId);

                entity.HasIndex(x => x.RevokedByUserId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.LicenseType,
                    x.LicenseNumber
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.LicenseType,
                    x.IsPrimary
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IsPrimary\" = true");

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.VerificationStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.IsVerified,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.LicenseType,
                    x.ExpiredDate,
                    x.VerificationStatus
                });

                entity.HasIndex(x => new
                {
                    x.ExpiredDate,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpClinicalPrivilege>(entity =>
            {
                entity.ToTable("WfpClinicalPrivilege", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.CredentialLicenseId)
                    .IsRequired(false);

                entity.Property(x => x.DepartmentId)
                    .IsRequired(false);

                entity.Property(x => x.PositionId)
                    .IsRequired(false);

                entity.Property(x => x.PrivilegeCode)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.PrivilegeName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.PrivilegeType)
                    .HasConversion<int>()
                    .HasDefaultValue(ClinicalPrivilegeType.CorePrivilege)
                    .IsRequired();

                entity.Property(x => x.ClinicalScope)
                    .HasMaxLength(100);

                entity.Property(x => x.SpecialtyName)
                    .HasMaxLength(150);

                entity.Property(x => x.SubSpecialtyName)
                    .HasMaxLength(150);

                entity.Property(x => x.ProcedureGroup)
                    .HasMaxLength(150);

                entity.Property(x => x.ProcedureName)
                    .HasMaxLength(200);

                entity.Property(x => x.PracticeLocation)
                    .HasMaxLength(200);

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.PrivilegeStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(ClinicalPrivilegeStatus.PendingApproval)
                    .IsRequired();

                entity.Property(x => x.IsTemporary)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsEmergencyPrivilege)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsSupervisionRequired)
                    .HasDefaultValue(false);

                entity.Property(x => x.SupervisorUserId)
                    .IsRequired(false);

                entity.Property(x => x.GrantedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.GrantedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.GrantNote)
                    .HasMaxLength(250);

                entity.Property(x => x.RejectedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.RejectedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RejectedReason)
                    .HasMaxLength(250);

                entity.Property(x => x.SuspendedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.SuspendedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.SuspensionReason)
                    .HasMaxLength(250);

                entity.Property(x => x.RevokedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.RevokedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RevokedReason)
                    .HasMaxLength(250);

                entity.Property(x => x.LastReviewDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.NextReviewDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.SupportingFilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.SupportingFileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.ClinicalPrivileges)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CredentialLicense)
                    .WithMany()
                    .HasForeignKey(x => x.CredentialLicenseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.SupervisorUser)
                    .WithMany()
                    .HasForeignKey(x => x.SupervisorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.GrantedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.GrantedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.RejectedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.SuspendedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.SuspendedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RevokedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.RevokedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.CredentialLicenseId);

                entity.HasIndex(x => x.DepartmentId);

                entity.HasIndex(x => x.PositionId);

                entity.HasIndex(x => x.SupervisorUserId);

                entity.HasIndex(x => x.GrantedByUserId);

                entity.HasIndex(x => x.RejectedByUserId);

                entity.HasIndex(x => x.SuspendedByUserId);

                entity.HasIndex(x => x.RevokedByUserId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.PrivilegeCode
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.PrivilegeStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.PrivilegeType,
                    x.PrivilegeStatus
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.DepartmentId,
                    x.PositionId,
                    x.PrivilegeStatus
                });

                entity.HasIndex(x => new
                {
                    x.CredentialLicenseId,
                    x.PrivilegeStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.EffectiveStartDate,
                    x.EffectiveEndDate
                });

                entity.HasIndex(x => new
                {
                    x.EffectiveEndDate,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpHealthRecord>(entity =>
            {
                entity.ToTable("WfpHealthRecord", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.RequirementCode)
                    .HasMaxLength(100);

                entity.Property(x => x.HealthRecordType)
                    .HasConversion<int>()
                    .HasDefaultValue(HealthRecordType.Unknown)
                    .IsRequired();

                entity.Property(x => x.RecordDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.ResultStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(HealthRecordResultStatus.Unknown)
                    .IsRequired();

                entity.Property(x => x.ProviderName)
                    .HasMaxLength(200);

                entity.Property(x => x.ExpiredDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.IsFitToWork)
                    .IsRequired(false);

                entity.Property(x => x.FitToWorkRestrictionNote)
                    .HasMaxLength(250);

                entity.Property(x => x.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(x => x.VerifiedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.VerifiedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.VerificationNote)
                    .HasMaxLength(250);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.HealthRecords)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.VerifiedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.VerifiedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.VerifiedByUserId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.HealthRecordType,
                    x.RecordDate,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.HealthRecordType,
                    x.ResultStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.IsVerified,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.IsFitToWork,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.HealthRecordType,
                    x.ExpiredDate,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.ExpiredDate,
                    x.IsVerified,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ExpiredDate,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpTransportAllowance>(entity =>
            {
                entity.ToTable("WfpTransportAllowance", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.AllowanceMode)
                    .HasMaxLength(50)
                    .HasDefaultValue("None")
                    .IsRequired();

                entity.Property(x => x.IsEligible)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsRegularTransportEligible)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsNightTransportEligible)
                    .HasDefaultValue(false);

                entity.Property(x => x.MonthlyAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.DailyAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.NightAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.IsProrated)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsTaxable)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsPayrollComponent)
                    .HasDefaultValue(true);

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.TransportAllowance)
                    .HasForeignKey<WfpTransportAllowance>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TransportAllowancePolicy)
                    .WithMany()
                    .HasForeignKey(x => x.TransportAllowancePolicyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => x.TransportAllowancePolicyId);

                entity.HasIndex(x => new
                {
                    x.AllowanceMode,
                    x.IsEligible,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => x.IsNightTransportEligible);
            });
            
            builder.Entity<WfpTransportAllowanceTransaction>(entity =>
            {
                entity.ToTable("WfpTransportAllowanceTransaction", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.TransactionDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.PeriodYearMonth)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.AllowanceType)
                    .HasMaxLength(50)
                    .HasDefaultValue("Regular")
                    .IsRequired();

                entity.Property(x => x.Amount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.IsGeneratedFromAttendance)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsNightShift)
                    .HasDefaultValue(false);

                entity.Property(x => x.TransactionStatus)
                    .HasMaxLength(50)
                    .HasDefaultValue("Draft")
                    .IsRequired();

                entity.Property(x => x.Notes)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.TransportAllowanceTransactions)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TransportAllowance)
                    .WithMany()
                    .HasForeignKey(x => x.TransportAllowanceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TransportAllowancePolicy)
                    .WithMany()
                    .HasForeignKey(x => x.TransportAllowancePolicyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Attendance)
                    .WithMany()
                    .HasForeignKey(x => x.AttendanceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.TransportAllowanceId);

                entity.HasIndex(x => x.TransportAllowancePolicyId);

                entity.HasIndex(x => x.AttendanceId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.PeriodYearMonth,
                    x.AllowanceType,
                    x.TransactionStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.TransactionDate,
                    x.TransactionStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => x.IsNightShift);
            });

            builder.Entity<WfpPayroll>(entity =>
            {
                entity.ToTable("WfpPayroll", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.PayrollGroup)
                    .HasMaxLength(50)
                    .HasDefaultValue("Default");

                entity.Property(x => x.PaymentMethod)
                    .HasMaxLength(50)
                    .HasDefaultValue("BankTransfer");

                entity.Property(x => x.BasicSalary)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.FixedAllowance)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.FixedDeduction)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.IsOvertimeEligible)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsPayrollActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.Payroll)
                    .HasForeignKey<WfpPayroll>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryBankAccount)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryBankAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => x.PrimaryBankAccountId);

                entity.HasIndex(x => new
                {
                    x.PayrollGroup,
                    x.IsPayrollActive,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpTax>(entity =>
            {
                entity.ToTable("WfpTax", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.NpwpNumber)
                    .HasMaxLength(30);

                entity.Property(x => x.TaxStatus)
                    .HasMaxLength(50)
                    .HasDefaultValue("TK0");

                entity.Property(x => x.IsTaxed)
                    .HasDefaultValue(true);

                entity.Property(x => x.TaxCalculationMethod)
                    .HasMaxLength(50)
                    .HasDefaultValue("Gross");

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.Tax)
                    .HasForeignKey<WfpTax>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => x.NpwpNumber);

                entity.HasIndex(x => new
                {
                    x.TaxStatus,
                    x.IsTaxed,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpInsurance>(entity =>
            {
                entity.ToTable("WfpInsurance", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.IsBpjsKesehatanEnabled)
                    .HasDefaultValue(false);

                entity.Property(x => x.BpjsKesehatanNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.IsBpjsKetenagakerjaanEnabled)
                    .HasDefaultValue(false);

                entity.Property(x => x.BpjsKetenagakerjaanNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.IsPrivateInsuranceEnabled)
                    .HasDefaultValue(false);

                entity.Property(x => x.PrivateInsuranceProvider)
                    .HasMaxLength(100);

                entity.Property(x => x.PrivateInsuranceNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.Insurance)
                    .HasForeignKey<WfpInsurance>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => x.BpjsKesehatanNumber);

                entity.HasIndex(x => x.BpjsKetenagakerjaanNumber);

                entity.HasIndex(x => new
                {
                    x.IsBpjsKesehatanEnabled,
                    x.IsBpjsKetenagakerjaanEnabled,
                    x.IsPrivateInsuranceEnabled,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstEmployee>(entity =>
            {
                entity.ToTable("MstEmployee", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.EmployeeCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.EmployeeNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.NickName)
                    .HasMaxLength(100);

                entity.Property(x => x.BirthPlace)
                    .HasMaxLength(100);

                entity.Property(x => x.BirthDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.Gender)
                    .HasConversion<int>()
                    .IsRequired(false);

                entity.Property(x => x.Religion)
                    .HasConversion<int>()
                    .HasDefaultValue(Religion.Unknown)
                    .IsRequired();

                entity.Property(x => x.MaritalStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(MaritalStatus.Unknown)
                    .IsRequired();

                entity.Property(x => x.BloodType)
                    .HasConversion<int>()
                    .HasDefaultValue(BloodType.Unknown)
                    .IsRequired();

                entity.Property(x => x.IdentityType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.IdentityNumber)
                    .HasMaxLength(16)
                    .IsRequired();

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(13);

                entity.Property(x => x.WhatsAppNumber)
                    .HasMaxLength(13);

                entity.Property(x => x.Email)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Address)
                    .HasMaxLength(500);

                entity.Property(x => x.PrimaryDepartmentId)
                    .IsRequired();

                entity.Property(x => x.PrimaryPositionId)
                    .IsRequired();

                entity.Property(x => x.EmployeeStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(EmployeeStatus.Active)
                    .IsRequired();

                entity.Property(x => x.ProfessionType)
                    .HasConversion<int>()
                    .HasDefaultValue(EmployeeProfessionType.GeneralStaff)
                    .IsRequired();

                entity.Property(x => x.EmploymentType)
                    .HasConversion<int>()
                    .HasDefaultValue(EmploymentType.Contract)
                    .IsRequired();

                entity.Property(x => x.GradeLevel)
                    .HasMaxLength(50);

                entity.Property(x => x.WorkLocation)
                    .HasMaxLength(50);

                entity.Property(x => x.JoinDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.ProbationEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ContractStartDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ContractEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ResignDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ResignReason)
                    .HasMaxLength(250);

                entity.Property(x => x.EmergencyContactName)
                    .HasMaxLength(200);

                entity.Property(x => x.EmergencyContactRelation)
                    .HasMaxLength(50);

                entity.Property(x => x.EmergencyContactPhone)
                    .HasMaxLength(13);

                entity.Property(x => x.EmergencyContactAddress)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.Employee)
                    .HasForeignKey<MstEmployee>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryPosition)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Country)
                    .WithMany()
                    .HasForeignKey(x => x.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Province)
                    .WithMany()
                    .HasForeignKey(x => x.ProvinceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.City)
                    .WithMany()
                    .HasForeignKey(x => x.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.District)
                    .WithMany()
                    .HasForeignKey(x => x.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PostalCode)
                    .WithMany()
                    .HasForeignKey(x => x.PostalCodeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique();

                entity.HasIndex(x => x.EmployeeCode)
                    .IsUnique();

                entity.HasIndex(x => x.EmployeeNumber)
                    .IsUnique()
                    .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => x.IdentityNumber)
                    .IsUnique()
                    .HasFilter("\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

                entity.HasIndex(x => x.Email)
                    .IsUnique()
                    .HasFilter("\"Email\" IS NOT NULL AND \"IsDelete\" = false");

                entity.HasIndex(x => x.PhoneNumber);

                entity.HasIndex(x => x.WhatsAppNumber);

                entity.HasIndex(x => x.FullName);

                entity.HasIndex(x => new
                {
                    x.PrimaryDepartmentId,
                    x.PrimaryPositionId
                });

                entity.HasIndex(x => new
                {
                    x.PrimaryDepartmentId,
                    x.PrimaryPositionId,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.CountryId,
                    x.ProvinceId,
                    x.CityId,
                    x.DistrictId,
                    x.PostalCodeId
                });

                entity.HasIndex(x => new
                {
                    x.EmployeeStatus,
                    x.ProfessionType,
                    x.EmploymentType,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => x.Religion);

                entity.HasIndex(x => x.MaritalStatus);

                entity.HasIndex(x => x.BloodType);

                entity.HasIndex(x => x.IsActive);
            });

            builder.Entity<MstDoctor>(entity =>
            {
                entity.ToTable("MstDoctor", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.DoctorCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DoctorNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.NickName)
                    .HasMaxLength(100);

                entity.Property(x => x.BirthPlace)
                    .HasMaxLength(100);

                entity.Property(x => x.BirthDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Gender)
                    .HasConversion<int>()
                    .IsRequired(false);

                entity.Property(x => x.Religion)
                    .HasConversion<int>()
                    .HasDefaultValue(Religion.Unknown)
                    .IsRequired();

                entity.Property(x => x.MaritalStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(MaritalStatus.Unknown)
                    .IsRequired();

                entity.Property(x => x.BloodType)
                    .HasConversion<int>()
                    .HasDefaultValue(BloodType.Unknown)
                    .IsRequired();

                entity.Property(x => x.IdentityType)
                    .HasMaxLength(50);

                entity.Property(x => x.IdentityNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(30);

                entity.Property(x => x.WhatsAppNumber)
                    .HasMaxLength(30);

                entity.Property(x => x.Email)
                    .HasMaxLength(200);

                entity.Property(x => x.Address)
                    .HasMaxLength(500);

                entity.Property(x => x.DoctorStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(DoctorStatus.Active)
                    .IsRequired();

                entity.Property(x => x.DoctorType)
                    .HasConversion<int>()
                    .HasDefaultValue(DoctorType.GeneralPractitioner)
                    .IsRequired();

                entity.Property(x => x.PracticeType)
                    .HasConversion<int>()
                    .HasDefaultValue(DoctorPracticeType.FullTime)
                    .IsRequired();

                entity.Property(x => x.EmploymentType)
                    .HasConversion<int>()
                    .HasDefaultValue(EmploymentType.Contract)
                    .IsRequired();

                entity.Property(x => x.SpecialistName)
                    .HasMaxLength(100);

                entity.Property(x => x.SubSpecialistName)
                    .HasMaxLength(100);

                entity.Property(x => x.MedicalStaffGroup)
                    .HasMaxLength(100);

                entity.Property(x => x.GradeLevel)
                    .HasMaxLength(50);

                entity.Property(x => x.WorkLocation)
                    .HasMaxLength(50);

                entity.Property(x => x.JoinDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ProbationEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ContractStartDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ContractEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ResignDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ResignReason)
                    .HasMaxLength(250);

                entity.Property(x => x.CredentialingDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.IsAvailableForAppointment)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.Doctor)
                    .HasForeignKey<MstDoctor>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryPosition)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Country)
                    .WithMany()
                    .HasForeignKey(x => x.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Province)
                    .WithMany()
                    .HasForeignKey(x => x.ProvinceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.City)
                    .WithMany()
                    .HasForeignKey(x => x.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.District)
                    .WithMany()
                    .HasForeignKey(x => x.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PostalCode)
                    .WithMany()
                    .HasForeignKey(x => x.PostalCodeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique();

                entity.HasIndex(x => x.DoctorCode)
                    .IsUnique();

                entity.HasIndex(x => x.DoctorNumber)
                    .IsUnique()
                    .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => x.IdentityNumber)
                    .IsUnique()
                    .HasFilter("\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

                entity.HasIndex(x => x.Email)
                    .HasFilter("\"Email\" IS NOT NULL");

                entity.HasIndex(x => x.PhoneNumber);

                entity.HasIndex(x => x.WhatsAppNumber);

                entity.HasIndex(x => x.FullName);

                entity.HasIndex(x => new
                {
                    x.PrimaryDepartmentId,
                    x.PrimaryPositionId
                });

                entity.HasIndex(x => new
                {
                    x.PrimaryDepartmentId,
                    x.PrimaryPositionId,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.CountryId,
                    x.ProvinceId,
                    x.CityId,
                    x.DistrictId,
                    x.PostalCodeId
                });

                entity.HasIndex(x => new
                {
                    x.DoctorStatus,
                    x.DoctorType,
                    x.PracticeType,
                    x.EmploymentType,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.SpecialistName,
                    x.SubSpecialistName,
                    x.MedicalStaffGroup
                });

                entity.HasIndex(x => x.IsAvailableForAppointment);

                entity.HasIndex(x => x.IsActive);
            });

            builder.Entity<MstExternalUser>(entity =>
            {
                entity.ToTable("MstExternalUser", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.ExternalCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ExternalUserType)
                    .HasConversion<int>()
                    .HasDefaultValue(ExternalUserType.Unknown)
                    .IsRequired();

                entity.Property(x => x.ExternalUserStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(ExternalUserStatus.Active)
                    .IsRequired();

                entity.Property(x => x.EngagementType)
                    .HasConversion<int>()
                    .HasDefaultValue(ExternalEngagementType.ContractBased)
                    .IsRequired();

                entity.Property(x => x.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.CompanyName)
                    .HasMaxLength(200);

                entity.Property(x => x.CompanyCode)
                    .HasMaxLength(100);

                entity.Property(x => x.JobTitle)
                    .HasMaxLength(100);

                entity.Property(x => x.ContactPersonName)
                    .HasMaxLength(200);

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(30);

                entity.Property(x => x.WhatsAppNumber)
                    .HasMaxLength(30);

                entity.Property(x => x.Email)
                    .HasMaxLength(200);

                entity.Property(x => x.Address)
                    .HasMaxLength(500);

                entity.Property(x => x.IdentityType)
                    .HasMaxLength(50);

                entity.Property(x => x.IdentityNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.TaxNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.BusinessLicenseNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.WorkLocation)
                    .HasMaxLength(50);

                entity.Property(x => x.ContractStartDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ContractEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.AccessStartDate)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.AccessEndDate)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.AccessPurpose)
                    .HasMaxLength(250);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.ExternalUser)
                    .HasForeignKey<MstExternalUser>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryPosition)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Country)
                    .WithMany()
                    .HasForeignKey(x => x.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Province)
                    .WithMany()
                    .HasForeignKey(x => x.ProvinceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.City)
                    .WithMany()
                    .HasForeignKey(x => x.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.District)
                    .WithMany()
                    .HasForeignKey(x => x.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PostalCode)
                    .WithMany()
                    .HasForeignKey(x => x.PostalCodeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique();

                entity.HasIndex(x => x.ExternalCode)
                    .IsUnique();

                entity.HasIndex(x => x.FullName);

                entity.HasIndex(x => x.CompanyName);

                entity.HasIndex(x => x.CompanyCode);

                entity.HasIndex(x => x.Email);

                entity.HasIndex(x => x.PhoneNumber);

                entity.HasIndex(x => x.WhatsAppNumber);

                entity.HasIndex(x => new
                {
                    x.ExternalUserType,
                    x.ExternalUserStatus,
                    x.EngagementType,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.PrimaryDepartmentId,
                    x.PrimaryPositionId
                });

                entity.HasIndex(x => new
                {
                    x.CountryId,
                    x.ProvinceId,
                    x.CityId,
                    x.DistrictId,
                    x.PostalCodeId
                });

                entity.HasIndex(x => new
                {
                    x.ContractStartDate,
                    x.ContractEndDate,
                    x.AccessStartDate,
                    x.AccessEndDate
                });

                entity.HasIndex(x => x.IsActive);
            });

            builder.Entity<EmpAttendance>(entity =>
            {
                entity.ToTable("EmpAttendance", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserId)
                    .IsRequired();

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.Property(x => x.WorkScheduleId)
                    .IsRequired(false);

                entity.Property(x => x.WorkScheduleAssignmentId)
                    .IsRequired(false);

                entity.Property(x => x.AttendanceDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.CheckInAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired();

                entity.Property(x => x.CheckOutAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.WorkStartTime)
                    .HasColumnType("time without time zone")
                    .IsRequired(false);

                entity.Property(x => x.WorkEndTime)
                    .HasColumnType("time without time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsOvernightSchedule)
                    .HasDefaultValue(false);

                entity.Property(x => x.ScheduledCheckInAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.ScheduledCheckOutAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CheckInToleranceMinutes)
                    .HasDefaultValue(0);

                entity.Property(x => x.CheckOutToleranceMinutes)
                    .HasDefaultValue(0);

                entity.Property(x => x.IsLate)
                    .HasDefaultValue(false);

                entity.Property(x => x.LateMinutes)
                    .HasDefaultValue(0);

                entity.Property(x => x.AttendanceStatus)
                    .HasMaxLength(50)
                    .HasDefaultValue("Present")
                    .IsRequired();

                entity.Property(x => x.UserType)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(x => x.CheckInSource)
                    .HasMaxLength(50)
                    .HasDefaultValue("Login")
                    .IsRequired();

                entity.Property(x => x.CheckOutSource)
                    .HasMaxLength(50);

                entity.Property(x => x.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("CheckedIn")
                    .IsRequired();

                entity.Property(x => x.GeofenceBypassReason)
                    .HasMaxLength(250);

                entity.Property(x => x.CheckInIpAddress)
                    .HasMaxLength(100);

                entity.Property(x => x.CheckOutIpAddress)
                    .HasMaxLength(100);

                entity.Property(x => x.CheckInUserAgent)
                    .HasMaxLength(500);

                entity.Property(x => x.CheckOutUserAgent)
                    .HasMaxLength(500);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany()
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkSchedule)
                    .WithMany()
                    .HasForeignKey(x => x.WorkScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkScheduleAssignment)
                    .WithMany()
                    .HasForeignKey(x => x.WorkScheduleAssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.AttendanceDate
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => x.EmployeeId);

                entity.HasIndex(x => x.DoctorId);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.WorkScheduleId);

                entity.HasIndex(x => x.WorkScheduleAssignmentId);

                entity.HasIndex(x => x.AttendanceDate);

                entity.HasIndex(x => x.Status);

                entity.HasIndex(x => x.AttendanceStatus);

                entity.HasIndex(x => x.IsLate);

                entity.HasIndex(x => x.IsOvernightSchedule);

                entity.HasIndex(x => x.IsGeofenceBypassed);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.AttendanceDate,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkScheduleId,
                    x.AttendanceDate,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkScheduleAssignmentId,
                    x.AttendanceDate,
                    x.IsDelete
                });
            });

            builder.Entity<MstWorkSchedule>(entity =>
            {
                entity.ToTable("MstWorkSchedule", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ScheduleCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ScheduleName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.ScheduleType)
                    .HasMaxLength(50)
                    .HasDefaultValue("Shift")
                    .IsRequired();

                entity.Property(x => x.WorkStartTime)
                    .HasColumnType("time without time zone")
                    .IsRequired();

                entity.Property(x => x.WorkEndTime)
                    .HasColumnType("time without time zone")
                    .IsRequired();

                entity.Property(x => x.IsOvernight)
                    .HasDefaultValue(false);

                entity.Property(x => x.CheckInToleranceMinutes)
                    .HasDefaultValue(0);

                entity.Property(x => x.CheckOutToleranceMinutes)
                    .HasDefaultValue(0);

                entity.Property(x => x.IsDefault)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasIndex(x => x.ScheduleCode)
                    .IsUnique();

                entity.HasIndex(x => x.ScheduleName);

                entity.HasIndex(x => x.ScheduleType);

                entity.HasIndex(x => new
                {
                    x.ScheduleType,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.IsDefault,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpWorkScheduleAssignment>(entity =>
            {
                entity.ToTable("WfpWorkScheduleAssignment", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.WorkScheduleId)
                    .IsRequired();

                entity.Property(x => x.ScheduleDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.IsOffDay)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsOvertimePlanned)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsOnCall)
                    .HasDefaultValue(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.WorkScheduleAssignments)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkSchedule)
                    .WithMany()
                    .HasForeignKey(x => x.WorkScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.WorkScheduleId);

                entity.HasIndex(x => x.ScheduleDate);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ScheduleDate,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkScheduleId,
                    x.ScheduleDate,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ScheduleDate
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            });

            builder.Entity<WfpLeaveBalance>(entity =>
            {
                entity.ToTable("WfpLeaveBalance", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.LeaveYear)
                    .IsRequired();

                entity.Property(x => x.LeaveType)
                    .HasConversion<int>()
                    .HasDefaultValue(LeaveType.AnnualLeave)
                    .IsRequired();

                entity.Property(x => x.OpeningBalance)
                    .HasColumnType("numeric(6,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.EntitledDays)
                    .HasColumnType("numeric(6,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.UsedDays)
                    .HasColumnType("numeric(6,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.PendingDays)
                    .HasColumnType("numeric(6,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.RemainingDays)
                    .HasColumnType("numeric(6,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.LeaveBalances)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.LeaveYear,
                    x.LeaveType
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.LeaveYear,
                    x.LeaveType,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.LeaveYear,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpLeaveRequest>(entity =>
            {
                entity.ToTable("WfpLeaveRequest", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.LeaveBalanceId)
                    .IsRequired(false);

                entity.Property(x => x.LeaveType)
                    .HasConversion<int>()
                    .HasDefaultValue(LeaveType.AnnualLeave)
                    .IsRequired();

                entity.Property(x => x.StartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EndDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.TotalDays)
                    .HasColumnType("numeric(6,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.IsHalfDay)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsDeductBalance)
                    .HasDefaultValue(true);

                entity.Property(x => x.Reason)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.ApprovalStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(LeaveApprovalStatus.PendingApproval)
                    .IsRequired();

                entity.Property(x => x.RequestedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.ApprovedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.ApprovedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.ApprovalNote)
                    .HasMaxLength(250);

                entity.Property(x => x.RejectedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.RejectedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RejectedReason)
                    .HasMaxLength(250);

                entity.Property(x => x.CancelledByUserId)
                    .IsRequired(false);

                entity.Property(x => x.CancelledAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelReason)
                    .HasMaxLength(250);

                entity.Property(x => x.AttachmentPath)
                    .HasMaxLength(500);

                entity.Property(x => x.AttachmentContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.LeaveRequests)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.LeaveBalance)
                    .WithMany(x => x.LeaveRequests)
                    .HasForeignKey(x => x.LeaveBalanceId)
                    .OnDelete(DeleteBehavior.Restrict);                

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.LeaveBalanceId);

                entity.HasIndex(x => x.ApprovedByUserId);

                entity.HasIndex(x => x.RejectedByUserId);

                entity.HasIndex(x => x.CancelledByUserId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.LeaveType,
                    x.ApprovalStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.StartDate,
                    x.EndDate,
                    x.ApprovalStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.LeaveType,
                    x.ApprovalStatus,
                    x.StartDate,
                    x.EndDate
                });

                entity.HasIndex(x => new
                {
                    x.RequestedAt,
                    x.ApprovalStatus,
                    x.IsDelete
                });
            });

            builder.Entity<WfpOvertimeRequest>(entity =>
            {
                entity.ToTable("WfpOvertimeRequest", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.AttendanceId)
                    .IsRequired(false);

                entity.Property(x => x.WorkScheduleAssignmentId)
                    .IsRequired(false);

                entity.Property(x => x.WorkScheduleId)
                    .IsRequired(false);

                entity.Property(x => x.OvertimeDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.ScheduledStartTime)
                    .HasColumnType("time without time zone")
                    .IsRequired(false);

                entity.Property(x => x.ScheduledEndTime)
                    .HasColumnType("time without time zone")
                    .IsRequired(false);

                entity.Property(x => x.ActualCheckInAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.ActualCheckOutAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.StartTime)
                    .HasColumnType("time without time zone")
                    .IsRequired();

                entity.Property(x => x.EndTime)
                    .HasColumnType("time without time zone")
                    .IsRequired();

                entity.Property(x => x.IsOvernight)
                    .HasDefaultValue(false);

                entity.Property(x => x.TotalMinutes)
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(x => x.Reason)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.ApprovalStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(OvertimeApprovalStatus.PendingApproval)
                    .IsRequired();

                entity.Property(x => x.RequestedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.ApprovedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.ApprovedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.ApprovalNote)
                    .HasMaxLength(250);

                entity.Property(x => x.RejectedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.RejectedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RejectedReason)
                    .HasMaxLength(250);

                entity.Property(x => x.CancelledByUserId)
                    .IsRequired(false);

                entity.Property(x => x.CancelledAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelReason)
                    .HasMaxLength(250);

                entity.Property(x => x.IsPayrollProcessed)
                    .HasDefaultValue(false);

                entity.Property(x => x.PayrollProcessedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.PayrollProcessedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.PayrollPeriodCode)
                    .HasMaxLength(50);

                entity.Property(x => x.AttachmentPath)
                    .HasMaxLength(500);

                entity.Property(x => x.AttachmentContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.OvertimeRequests)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Attendance)
                    .WithMany()
                    .HasForeignKey(x => x.AttendanceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkScheduleAssignment)
                    .WithMany()
                    .HasForeignKey(x => x.WorkScheduleAssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkSchedule)
                    .WithMany()
                    .HasForeignKey(x => x.WorkScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.RejectedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CancelledByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CancelledByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PayrollProcessedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.PayrollProcessedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.AttendanceId);

                entity.HasIndex(x => x.WorkScheduleAssignmentId);

                entity.HasIndex(x => x.WorkScheduleId);

                entity.HasIndex(x => x.ApprovedByUserId);

                entity.HasIndex(x => x.RejectedByUserId);

                entity.HasIndex(x => x.CancelledByUserId);

                entity.HasIndex(x => x.PayrollProcessedByUserId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.OvertimeDate,
                    x.ApprovalStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.OvertimeDate,
                    x.StartTime,
                    x.EndTime
                })
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.ApprovalStatus,
                    x.IsPayrollProcessed,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.PayrollPeriodCode,
                    x.IsPayrollProcessed,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkScheduleId,
                    x.OvertimeDate,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.AttendanceId,
                    x.IsDelete
                });
            });

            builder.Entity<WfpOnboardingChecklist>(entity =>
            {
                entity.ToTable("WfpOnboardingChecklist", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.OnboardingType)
                    .HasConversion<int>()
                    .HasDefaultValue(OnboardingType.Unknown)
                    .IsRequired();

                entity.Property(x => x.StartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.TargetCompletionDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.CompletedDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Status)
                    .HasConversion<int>()
                    .HasDefaultValue(OnboardingStatus.Draft)
                    .IsRequired();

                entity.Property(x => x.AssignedToUserId)
                    .IsRequired(false);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.OnboardingChecklists)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.AssignedToUser)
                    .WithMany()
                    .HasForeignKey(x => x.AssignedToUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.AssignedToUserId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.Status,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.OnboardingType,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.Status,
                    x.TargetCompletionDate,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.Status
                })
                .HasFilter("\"IsDelete\" = false");
            });

            builder.Entity<WfpOnboardingTask>(entity =>
            {
                entity.ToTable("WfpOnboardingTask", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.OnboardingChecklistId)
                    .IsRequired();

                entity.Property(x => x.TaskCode)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.TaskName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.TaskCategory)
                    .HasConversion<int>()
                    .HasDefaultValue(OnboardingTaskCategory.Other)
                    .IsRequired();

                entity.Property(x => x.IsRequired)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsCompleted)
                    .HasDefaultValue(false);

                entity.Property(x => x.CompletedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CompletedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.SortOrder)
                    .HasDefaultValue(0);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.OnboardingChecklist)
                    .WithMany(x => x.Tasks)
                    .HasForeignKey(x => x.OnboardingChecklistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CompletedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CompletedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.OnboardingChecklistId);

                entity.HasIndex(x => x.CompletedByUserId);

                entity.HasIndex(x => new
                {
                    x.OnboardingChecklistId,
                    x.TaskCategory,
                    x.IsCompleted,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.OnboardingChecklistId,
                    x.TaskCode
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.OnboardingChecklistId,
                    x.SortOrder
                });

                entity.HasIndex(x => new
                {
                    x.IsRequired,
                    x.IsCompleted,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpOffboardingChecklist>(entity =>
            {
                entity.ToTable("WfpOffboardingChecklist", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.OffboardingType)
                    .HasConversion<int>()
                    .HasDefaultValue(OffboardingType.Unknown)
                    .IsRequired();

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.StartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.CompletedDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Status)
                    .HasConversion<int>()
                    .HasDefaultValue(OffboardingStatus.Draft)
                    .IsRequired();

                entity.Property(x => x.Reason)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.OffboardingChecklists)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.Status,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.OffboardingType,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.Status,
                    x.EffectiveEndDate,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.Status
                })
                .HasFilter("\"IsDelete\" = false");
            });

            builder.Entity<WfpOffboardingTask>(entity =>
            {
                entity.ToTable("WfpOffboardingTask", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.OffboardingChecklistId)
                    .IsRequired();

                entity.Property(x => x.TaskCode)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.TaskName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.TaskCategory)
                    .HasConversion<int>()
                    .HasDefaultValue(OffboardingTaskCategory.Other)
                    .IsRequired();

                entity.Property(x => x.IsRequired)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsCompleted)
                    .HasDefaultValue(false);

                entity.Property(x => x.CompletedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CompletedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.SortOrder)
                    .HasDefaultValue(0);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.OffboardingChecklist)
                    .WithMany(x => x.Tasks)
                    .HasForeignKey(x => x.OffboardingChecklistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CompletedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CompletedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.OffboardingChecklistId);

                entity.HasIndex(x => x.CompletedByUserId);

                entity.HasIndex(x => new
                {
                    x.OffboardingChecklistId,
                    x.TaskCategory,
                    x.IsCompleted,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.OffboardingChecklistId,
                    x.TaskCode
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.OffboardingChecklistId,
                    x.SortOrder
                });

                entity.HasIndex(x => new
                {
                    x.IsRequired,
                    x.IsCompleted,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpEmploymentHistory>(entity =>
            {
                entity.ToTable("WfpEmploymentHistory", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.HistoryType)
                    .HasConversion<int>()
                    .HasDefaultValue(EmploymentHistoryType.Unknown)
                    .IsRequired();

                entity.Property(x => x.OldStatus)
                    .HasMaxLength(100);

                entity.Property(x => x.NewStatus)
                    .HasMaxLength(100);

                entity.Property(x => x.EffectiveDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.ApprovedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.EmploymentHistories)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.OldDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.OldDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.NewDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.NewDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.OldPosition)
                    .WithMany()
                    .HasForeignKey(x => x.OldPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.NewPosition)
                    .WithMany()
                    .HasForeignKey(x => x.NewPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.HistoryType);

                entity.HasIndex(x => x.EffectiveDate);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.HistoryType,
                    x.EffectiveDate,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.NewDepartmentId,
                    x.NewPositionId
                });

                entity.HasIndex(x => x.ApprovedByUserId);
            });

            builder.Entity<WfpContractHistory>(entity =>
            {
                entity.ToTable("WfpContractHistory", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.ContractNumber)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.ContractType)
                    .HasConversion<int>()
                    .HasDefaultValue(ContractHistoryType.Unknown)
                    .IsRequired();

                entity.Property(x => x.StartDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ContractStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(ContractHistoryStatus.Draft)
                    .IsRequired();

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.ContractHistories)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.ContractType);

                entity.HasIndex(x => x.ContractStatus);

                entity.HasIndex(x => x.EndDate);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ContractNumber,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ContractStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.StartDate,
                    x.EndDate
                });
            });

            builder.Entity<MstCompetency>(entity =>
            {
                entity.ToTable("MstCompetency", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.CompetencyCode)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.CompetencyName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.CompetencyCategory)
                    .HasConversion<int>()
                    .HasDefaultValue(CompetencyCategory.Other)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasIndex(x => x.CompetencyCode)
                    .IsUnique();

                entity.HasIndex(x => x.CompetencyName);

                entity.HasIndex(x => x.CompetencyCategory);

                entity.HasIndex(x => new
                {
                    x.CompetencyCategory,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<MstPositionCompetencyRequirement>(entity =>
            {
                entity.ToTable("MstPositionCompetencyRequirement", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.PositionId)
                    .IsRequired();

                entity.Property(x => x.CompetencyId)
                    .IsRequired();

                entity.Property(x => x.IsRequired)
                    .HasDefaultValue(true);

                entity.Property(x => x.MinimumLevel)
                    .HasConversion<int>()
                    .HasDefaultValue(CompetencyLevel.Basic)
                    .IsRequired();

                entity.Property(x => x.IsCertificationRequired)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsTrainingRequired)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.Position)
                    .WithMany(x => x.CompetencyRequirements)
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Competency)
                    .WithMany(x => x.PositionRequirements)
                    .HasForeignKey(x => x.CompetencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.PositionId);

                entity.HasIndex(x => x.CompetencyId);

                entity.HasIndex(x => new
                {
                    x.PositionId,
                    x.CompetencyId
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.PositionId,
                    x.IsRequired,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.CompetencyId,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.MinimumLevel,
                    x.IsCertificationRequired,
                    x.IsTrainingRequired
                });
            });

            builder.Entity<WfpCompetencyAssessment>(entity =>
            {
                entity.ToTable("WfpCompetencyAssessment", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.CompetencyId)
                    .IsRequired();

                entity.Property(x => x.AssessmentDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.CompetencyLevel)
                    .HasConversion<int>()
                    .HasDefaultValue(CompetencyLevel.None)
                    .IsRequired();

                entity.Property(x => x.ResultStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(CompetencyAssessmentResultStatus.NotAssessed)
                    .IsRequired();

                entity.Property(x => x.AssessedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.ExpiredDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.CompetencyAssessments)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Competency)
                    .WithMany(x => x.CompetencyAssessments)
                    .HasForeignKey(x => x.CompetencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.AssessedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.AssessedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.CompetencyId);

                entity.HasIndex(x => x.AssessedByUserId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.CompetencyId,
                    x.AssessmentDate,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.CompetencyId,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ResultStatus,
                    x.IsVerified,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.ExpiredDate,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpPerformanceReview>(entity =>
            {
                entity.ToTable("WfpPerformanceReview", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.ReviewPeriod)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ReviewDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.ReviewerUserId)
                    .IsRequired();

                entity.Property(x => x.ReviewType)
                    .HasConversion<int>()
                    .HasDefaultValue(PerformanceReviewType.Unknown)
                    .IsRequired();

                entity.Property(x => x.TotalScore)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.FinalRating)
                    .HasConversion<int>()
                    .HasDefaultValue(PerformanceFinalRating.NotRated)
                    .IsRequired();

                entity.Property(x => x.ReviewStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(PerformanceReviewStatus.Draft)
                    .IsRequired();

                entity.Property(x => x.StrengthNotes)
                    .HasMaxLength(1000);

                entity.Property(x => x.ImprovementNotes)
                    .HasMaxLength(1000);

                entity.Property(x => x.RecommendationNotes)
                    .HasMaxLength(1000);

                entity.Property(x => x.IsFinalized)
                    .HasDefaultValue(false);

                entity.Property(x => x.FinalizedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.PerformanceReviews)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ReviewerUser)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.ReviewerUserId);

                entity.HasIndex(x => x.ReviewPeriod);

                entity.HasIndex(x => x.ReviewDate);

                entity.HasIndex(x => x.ReviewType);

                entity.HasIndex(x => x.ReviewStatus);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ReviewPeriod,
                    x.ReviewType,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ReviewStatus,
                    x.IsFinalized,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.ReviewerUserId,
                    x.ReviewDate,
                    x.IsDelete
                });
            });

            builder.Entity<WfpPerformanceReviewDetail>(entity =>
            {
                entity.ToTable("WfpPerformanceReviewDetail", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.PerformanceReviewId)
                    .IsRequired();

                entity.Property(x => x.CriteriaCode)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.CriteriaName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Score)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.Weight)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.WeightedScore)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.PerformanceReview)
                    .WithMany(x => x.Details)
                    .HasForeignKey(x => x.PerformanceReviewId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.PerformanceReviewId);

                entity.HasIndex(x => x.CriteriaCode);

                entity.HasIndex(x => new
                {
                    x.PerformanceReviewId,
                    x.CriteriaCode
                })
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.PerformanceReviewId,
                    x.IsDelete
                });
            });

            builder.Entity<WfpDisciplinaryAction>(entity =>
            {
                entity.ToTable("WfpDisciplinaryAction", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.ActionType)
                    .HasConversion<int>()
                    .HasDefaultValue(DisciplinaryActionType.Unknown)
                    .IsRequired();

                entity.Property(x => x.IncidentDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.IssuedDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.SeverityLevel)
                    .HasConversion<int>()
                    .HasDefaultValue(DisciplinarySeverityLevel.Low)
                    .IsRequired();

                entity.Property(x => x.Reason)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.IssuedByUserId)
                    .IsRequired();

                entity.Property(x => x.EffectiveUntil)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.ActionStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(DisciplinaryActionStatus.Draft)
                    .IsRequired();

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.DisciplinaryActions)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.IssuedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.IssuedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.IssuedByUserId);

                entity.HasIndex(x => x.ActionType);

                entity.HasIndex(x => x.SeverityLevel);

                entity.HasIndex(x => x.ActionStatus);

                entity.HasIndex(x => x.IncidentDate);

                entity.HasIndex(x => x.IssuedDate);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ActionType,
                    x.IssuedDate,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.ActionStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.SeverityLevel,
                    x.ActionStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.EffectiveUntil,
                    x.ActionStatus,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<WfpComplianceAlert>(entity =>
            {
                entity.ToTable("WfpComplianceAlert", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.SourceEntityName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.SourceEntityId)
                    .IsRequired();

                entity.Property(x => x.AlertType)
                    .HasConversion<int>()
                    .HasDefaultValue(ComplianceAlertType.Unknown)
                    .IsRequired();

                entity.Property(x => x.AlertTitle)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.AlertMessage)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.DueDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.AlertStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(ComplianceAlertStatus.Open)
                    .IsRequired();

                entity.Property(x => x.SeverityLevel)
                    .HasConversion<int>()
                    .HasDefaultValue(ComplianceAlertSeverityLevel.Low)
                    .IsRequired();

                entity.Property(x => x.IsResolved)
                    .HasDefaultValue(false);

                entity.Property(x => x.ResolvedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.ResolvedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.ComplianceAlerts)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ResolvedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ResolvedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.ResolvedByUserId);

                entity.HasIndex(x => x.SourceEntityName);

                entity.HasIndex(x => x.SourceEntityId);

                entity.HasIndex(x => x.AlertType);

                entity.HasIndex(x => x.AlertStatus);

                entity.HasIndex(x => x.SeverityLevel);

                entity.HasIndex(x => x.DueDate);

                entity.HasIndex(x => new
                {
                    x.SourceEntityName,
                    x.SourceEntityId,
                    x.AlertType,
                    x.DueDate
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.AlertStatus,
                    x.IsResolved,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.DueDate,
                    x.AlertStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.DueDate,
                    x.SeverityLevel,
                    x.AlertStatus,
                    x.IsResolved,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.SourceEntityName,
                    x.SourceEntityId,
                    x.IsDelete
                });
            });

            builder.Entity<WfpComplianceAlertLog>(entity =>
            {
                entity.ToTable("WfpComplianceAlertLog", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ComplianceAlertId)
                    .IsRequired();

                entity.Property(x => x.LogType)
                    .HasConversion<int>()
                    .HasDefaultValue(ComplianceAlertLogType.Created)
                    .IsRequired();

                entity.Property(x => x.OldStatus)
                    .HasConversion<int>()
                    .IsRequired(false);

                entity.Property(x => x.NewStatus)
                    .HasConversion<int>()
                    .IsRequired(false);

                entity.Property(x => x.LogMessage)
                    .HasMaxLength(1000);

                entity.Property(x => x.PerformedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.PerformedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.ComplianceAlert)
                    .WithMany(x => x.Logs)
                    .HasForeignKey(x => x.ComplianceAlertId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PerformedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.PerformedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ComplianceAlertId);

                entity.HasIndex(x => x.PerformedByUserId);

                entity.HasIndex(x => x.LogType);

                entity.HasIndex(x => x.PerformedAt);

                entity.HasIndex(x => new
                {
                    x.ComplianceAlertId,
                    x.LogType,
                    x.PerformedAt,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.PerformedByUserId,
                    x.PerformedAt,
                    x.IsDelete
                });
            });

            builder.Entity<WfpScheduleChangeRequest>(entity =>
            {
                entity.ToTable("WfpScheduleChangeRequest", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.CurrentWorkScheduleAssignmentId)
                    .IsRequired(false);

                entity.Property(x => x.RequestedScheduleDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.RequestedWorkScheduleId)
                    .IsRequired(false);

                entity.Property(x => x.RequestType)
                    .HasConversion<int>()
                    .HasDefaultValue(ScheduleChangeRequestType.Unknown)
                    .IsRequired();

                entity.Property(x => x.Reason)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.ApprovalStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(ScheduleRequestApprovalStatus.Pending)
                    .IsRequired();

                entity.Property(x => x.RequestedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.Property(x => x.ApprovedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.ApprovedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RejectedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.RejectedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RejectedReason)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithMany(x => x.ScheduleChangeRequests)
                    .HasForeignKey(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CurrentWorkScheduleAssignment)
                    .WithMany()
                    .HasForeignKey(x => x.CurrentWorkScheduleAssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RequestedWorkSchedule)
                    .WithMany()
                    .HasForeignKey(x => x.RequestedWorkScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.RejectedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId);

                entity.HasIndex(x => x.CurrentWorkScheduleAssignmentId);

                entity.HasIndex(x => x.RequestedWorkScheduleId);

                entity.HasIndex(x => x.RequestType);

                entity.HasIndex(x => x.ApprovalStatus);

                entity.HasIndex(x => x.RequestedScheduleDate);

                entity.HasIndex(x => x.RequestedAt);

                entity.HasIndex(x => x.ApprovedByUserId);

                entity.HasIndex(x => x.RejectedByUserId);

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.RequestedScheduleDate,
                    x.ApprovalStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.RequestType,
                    x.ApprovalStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.CurrentWorkScheduleAssignmentId,
                    x.ApprovalStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.WorkforceProfileId,
                    x.RequestedScheduleDate,
                    x.RequestType,
                    x.ApprovalStatus
                })
                .HasFilter("\"ApprovalStatus\" = 1 AND \"IsDelete\" = false");
            });

            builder.Entity<WfpShiftSwapRequest>(entity =>
            {
                entity.ToTable("WfpShiftSwapRequest", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.RequesterWorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.TargetWorkforceProfileId)
                    .IsRequired();

                entity.Property(x => x.RequesterScheduleAssignmentId)
                    .IsRequired();

                entity.Property(x => x.TargetScheduleAssignmentId)
                    .IsRequired();

                entity.Property(x => x.Reason)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.ApprovalStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(ScheduleRequestApprovalStatus.Pending)
                    .IsRequired();

                entity.Property(x => x.RequestedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.Property(x => x.ApprovedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.ApprovedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RejectedByUserId)
                    .IsRequired(false);

                entity.Property(x => x.RejectedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.RejectedReason)
                    .HasMaxLength(250);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.DeleteDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CancelDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.RequesterWorkforceProfile)
                    .WithMany(x => x.RequestedShiftSwapRequests)
                    .HasForeignKey(x => x.RequesterWorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TargetWorkforceProfile)
                    .WithMany(x => x.TargetShiftSwapRequests)
                    .HasForeignKey(x => x.TargetWorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RequesterScheduleAssignment)
                    .WithMany()
                    .HasForeignKey(x => x.RequesterScheduleAssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TargetScheduleAssignment)
                    .WithMany()
                    .HasForeignKey(x => x.TargetScheduleAssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.RejectedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.RequesterWorkforceProfileId);

                entity.HasIndex(x => x.TargetWorkforceProfileId);

                entity.HasIndex(x => x.RequesterScheduleAssignmentId);

                entity.HasIndex(x => x.TargetScheduleAssignmentId);

                entity.HasIndex(x => x.ApprovalStatus);

                entity.HasIndex(x => x.RequestedAt);

                entity.HasIndex(x => x.ApprovedByUserId);

                entity.HasIndex(x => x.RejectedByUserId);

                entity.HasIndex(x => new
                {
                    x.RequesterWorkforceProfileId,
                    x.TargetWorkforceProfileId,
                    x.ApprovalStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.RequesterScheduleAssignmentId,
                    x.TargetScheduleAssignmentId,
                    x.ApprovalStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.RequesterWorkforceProfileId,
                    x.RequesterScheduleAssignmentId,
                    x.ApprovalStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.TargetWorkforceProfileId,
                    x.TargetScheduleAssignmentId,
                    x.ApprovalStatus,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.RequesterScheduleAssignmentId,
                    x.TargetScheduleAssignmentId,
                    x.ApprovalStatus
                })
                .HasFilter("\"ApprovalStatus\" = 1 AND \"IsDelete\" = false");
            });
        }
    }
}
