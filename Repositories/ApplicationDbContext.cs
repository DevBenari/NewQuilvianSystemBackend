using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Employee.Models;
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

        public DbSet<MstEmployee> MstEmployees { get; set; }        
        public DbSet<MstDoctor> MstDoctors { get; set; }
        public DbSet<MstExternalUser> MstExternalUsers { get; set; }

        public DbSet<EmpPayrollProfile> EmpPayrollProfiles { get; set; }
        public DbSet<EmpTaxProfile> EmpTaxProfiles { get; set; }
        public DbSet<EmpInsuranceProfile> EmpInsuranceProfiles { get; set; }
        public DbSet<EmpBankAccount> EmpBankAccounts { get; set; }
        public DbSet<EmpDocument> EmpDocuments { get; set; }

        public DbSet<WfpOrganizationAssignment> WfpOrganizationAssignments { get; set; }
        public DbSet<WfpBankAccount> WfpBankAccounts { get; set; }        
        public DbSet<WfpDocument> WfpDocuments { get; set; }
        public DbSet<WfpEducation> WfpEducations { get; set; }
        public DbSet<WfpTrainingRecord> WfpTrainingRecords { get; set; }
        public DbSet<WfpCertification> WfpCertifications { get; set; }
        public DbSet<WfpCredentialLicense> WfpCredentialLicenses { get; set; }

        public DbSet<EmpTransportAllowanceProfile> EmpTransportAllowanceProfiles { get; set; }
        public DbSet<EmpTransportAllowancePolicy> EmpTransportAllowancePolicies { get; set; }
        public DbSet<EmpTransportAllowanceTransaction> EmpTransportAllowanceTransactions { get; set; }       

        public DbSet<DctLicense> DctLicenses { get; set; }
        public DbSet<DctPracticeProfile> DctPracticeProfiles { get; set; }
        public DbSet<DctFeeProfile> DctFeeProfiles { get; set; }

        public DbSet<ExtUserContract> ExtUserContracts { get; set; }
        public DbSet<ExtUserDocument> ExtUserDocuments { get; set; }

        public DbSet<EmpAttendance> EmpAttendances { get; set; }
        public DbSet<MstWorkSchedule> MstWorkSchedules { get; set; }

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

                entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Doctor)
                    .WithMany()
                    .HasForeignKey(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ExternalUser)
                    .WithMany()
                    .HasForeignKey(x => x.ExternalUserId)
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

                entity.HasIndex(x => x.EmployeeId);

                entity.HasIndex(x => x.DoctorId);

                entity.HasIndex(x => x.ExternalUserId);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.UserAccount)
                    .HasForeignKey<ApplicationUser>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"WorkforceProfileId\" IS NOT NULL");

                entity.HasIndex(x => new { x.PrimaryDepartmentId, x.PrimaryPositionId });

                entity.HasIndex(x => x.IsActive);

                entity.HasIndex(x => x.IsGeolocationBypassEnabled);                
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
                entity.ToTable("ApplicationUserFingerprintCredential", "public");

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
                    x.RequirementCode
                })
                .IsUnique();

                entity.HasIndex(x => new
                {
                    x.UserType,
                    x.RequirementCategory,
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
                    .WithMany(x => x.CredentialLicenses)
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
                    x.LicenseType,
                    x.LicenseNumber,
                    x.IsDelete
                });
            });

            builder.Entity<MstEmployee>(entity =>
            {
                entity.ToTable("MstEmployee", "public");

                entity.HasKey(x => x.Id);

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

                entity.Property(x => x.Gender)
                    .HasConversion<int>()
                    .IsRequired(false);

                entity.Property(x => x.BirthPlace)
                    .HasMaxLength(100);

                entity.Property(x => x.BirthDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.Religion)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasDefaultValue(Religion.Unknown)
                    .IsRequired();

                entity.Property(x => x.MaritalStatus)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasDefaultValue(MaritalStatus.Unknown)
                    .IsRequired();

                entity.Property(x => x.BloodType)
                    .HasConversion<string>()
                    .HasMaxLength(50)
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

                entity.Property(x => x.EmployeeStatus)
                    .HasConversion<int>();

                entity.Property(x => x.ProfessionType)
                    .HasConversion<int>();

                entity.Property(x => x.EmploymentType)
                    .HasMaxLength(50)
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

                // Department / Position
                entity.HasOne(x => x.PrimaryDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryPosition)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Region
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

                // Transport allowance
                entity.HasOne(x => x.TransportAllowanceProfile)
                    .WithOne(x => x.Employee)
                    .HasForeignKey<EmpTransportAllowanceProfile>(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(x => x.TransportAllowanceTransactions)
                    .WithOne(x => x.Employee)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
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
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => x.Religion);

                entity.HasIndex(x => x.MaritalStatus);

                entity.HasIndex(x => x.BloodType);

                entity.HasIndex(x => x.IsActive);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.Employee)
                    .HasForeignKey<MstEmployee>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"WorkforceProfileId\" IS NOT NULL");
            });

            builder.Entity<EmpPayrollProfile>(entity =>
            {
                entity.ToTable("EmpPayrollProfile", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EmployeeId)
                    .IsRequired();

                entity.Property(x => x.PayrollNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.SalaryType)
                    .HasMaxLength(50);

                entity.Property(x => x.BasicSalary)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.FixedAllowance)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.MealAllowance)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.TransportAllowance)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.PositionAllowance)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.OtherAllowance)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.FixedDeduction)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.IsOvertimeEligible)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsPayrollActive)
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

                entity.HasOne(x => x.Employee)
                    .WithOne(x => x.PayrollProfile)
                    .HasForeignKey<EmpPayrollProfile>(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.EmployeeId)
                    .IsUnique();

                entity.HasIndex(x => x.PayrollNumber);
            });

            builder.Entity<EmpTaxProfile>(entity =>
            {
                entity.ToTable("EmpTaxProfile", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EmployeeId)
                    .IsRequired();

                entity.Property(x => x.TaxNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.PtkpStatus)
                    .HasMaxLength(50);

                entity.Property(x => x.TaxRegisteredName)
                    .HasMaxLength(200);

                entity.Property(x => x.TaxRegisteredAddress)
                    .HasMaxLength(500);

                entity.Property(x => x.IsPph21Active)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsTaxPaidByCompany)
                    .HasDefaultValue(false);

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

                entity.HasOne(x => x.Employee)
                    .WithOne(x => x.TaxProfile)
                    .HasForeignKey<EmpTaxProfile>(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.EmployeeId)
                    .IsUnique();

                entity.HasIndex(x => x.TaxNumber);
            });

            builder.Entity<EmpInsuranceProfile>(entity =>
            {
                entity.ToTable("EmpInsuranceProfile", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EmployeeId)
                    .IsRequired();

                entity.Property(x => x.BpjsHealthNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.BpjsEmploymentNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.PrivateInsuranceName)
                    .HasMaxLength(100);

                entity.Property(x => x.PrivateInsuranceNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.IsBpjsHealthActive)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsBpjsEmploymentActive)
                    .HasDefaultValue(false);

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

                entity.HasOne(x => x.Employee)
                    .WithOne(x => x.InsuranceProfile)
                    .HasForeignKey<EmpInsuranceProfile>(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.EmployeeId)
                    .IsUnique();

                entity.HasIndex(x => x.BpjsHealthNumber);
                entity.HasIndex(x => x.BpjsEmploymentNumber);
            });

            builder.Entity<EmpBankAccount>(entity =>
            {
                entity.ToTable("EmpBankAccount", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EmployeeId)
                    .IsRequired();

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

                entity.HasOne(x => x.Employee)
                    .WithMany(x => x.BankAccounts)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.EmployeeId);
                entity.HasIndex(x => x.AccountNumber);
            });

            builder.Entity<EmpDocument>(entity =>
            {
                entity.ToTable("EmpDocument", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EmployeeId)
                    .IsRequired();

                entity.Property(x => x.DocumentType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.DocumentName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.DocumentNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.FileContentType)
                    .HasMaxLength(100);

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

                entity.HasOne(x => x.Employee)
                    .WithMany(x => x.Documents)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.EmployeeId);
                entity.HasIndex(x => x.DocumentType);
                entity.HasIndex(x => x.DocumentNumber);
            });

            builder.Entity<EmpTransportAllowanceProfile>(entity =>
            {
                entity.ToTable("EmpTransportAllowanceProfile", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EmployeeId)
                    .IsRequired();

                entity.Property(x => x.AllowanceMode)
                    .HasMaxLength(50)
                    .HasDefaultValue("None")
                    .IsRequired();

                entity.Property(x => x.MonthlyAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.DailyAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.NightAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.IsEligible)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsNightTransportEligible)
                    .HasDefaultValue(false);

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

                entity.HasOne(x => x.Employee)
                    .WithOne(x => x.TransportAllowanceProfile)
                    .HasForeignKey<EmpTransportAllowanceProfile>(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.EmployeeId)
                    .IsUnique()
                    .HasFilter("\"IsDelete\" = false");

                entity.HasIndex(x => new
                {
                    x.IsEligible,
                    x.IsNightTransportEligible,
                    x.IsActive,
                    x.IsDelete
                });

                entity.HasIndex(x => x.AllowanceMode);

                entity.HasIndex(x => new
                {
                    x.EffectiveStartDate,
                    x.EffectiveEndDate,
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<EmpTransportAllowancePolicy>(entity =>
            {
                entity.ToTable("EmpTransportAllowancePolicy", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.PolicyCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.PolicyName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.AllowanceMode)
                    .HasMaxLength(50)
                    .HasDefaultValue("DailyAttendance")
                    .IsRequired();

                entity.Property(x => x.DefaultMonthlyAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.DefaultDailyAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.DefaultNightAmount)
                    .HasColumnType("numeric(18,2)")
                    .HasDefaultValue(0);

                entity.Property(x => x.NightStartTime)
                    .HasColumnType("time without time zone")
                    .IsRequired(false);

                entity.Property(x => x.NightEndTime)
                    .HasColumnType("time without time zone")
                    .IsRequired(false);

                entity.Property(x => x.RequireAttendance)
                    .HasDefaultValue(true);

                entity.Property(x => x.ExcludeIfAbsent)
                    .HasDefaultValue(true);

                entity.Property(x => x.ExcludeIfLeave)
                    .HasDefaultValue(true);

                entity.Property(x => x.ExcludeIfHoliday)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsTaxable)
                    .HasDefaultValue(true);

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

                entity.HasIndex(x => x.PolicyCode)
                    .IsUnique();

                entity.HasIndex(x => x.PolicyName);

                entity.HasIndex(x => x.AllowanceMode);

                entity.HasIndex(x => new
                {
                    x.IsActive,
                    x.IsDelete
                });
            });

            builder.Entity<EmpTransportAllowanceTransaction>(entity =>
            {
                entity.ToTable("EmpTransportAllowanceTransaction", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EmployeeId)
                    .IsRequired();

                entity.Property(x => x.TransactionDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.PeriodYearMonth)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.AllowanceType)
                    .HasMaxLength(50)
                    .HasDefaultValue("Daily")
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

                entity.HasOne(x => x.Employee)
                    .WithMany(x => x.TransportAllowanceTransactions)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TransportAllowanceProfile)
                    .WithMany()
                    .HasForeignKey(x => x.TransportAllowanceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TransportAllowancePolicy)
                    .WithMany()
                    .HasForeignKey(x => x.TransportAllowancePolicyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.PeriodYearMonth,
                    x.TransactionStatus,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.EmployeeId,
                    x.PeriodYearMonth,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.TransactionDate,
                    x.AllowanceType,
                    x.IsDelete
                });

                entity.HasIndex(x => new
                {
                    x.EmployeeId,
                    x.TransactionDate,
                    x.AllowanceType,
                    x.IsNightShift,
                    x.IsDelete
                });

                entity.HasIndex(x => x.TransportAllowanceProfileId);

                entity.HasIndex(x => x.TransportAllowancePolicyId);

                entity.HasIndex(x => x.AttendanceId);

                entity.HasIndex(x => x.ShiftId);
            });                      

            builder.Entity<MstDoctor>(entity =>
            {
                entity.ToTable("MstDoctor", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DoctorCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.DoctorType)
                    .HasConversion<int>();

                entity.Property(x => x.BirthPlace)
                    .HasMaxLength(100);

                entity.Property(x => x.BirthDate)
                    .HasColumnType("date")
                    .IsRequired(false);

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

                entity.Property(x => x.SpecialistName)
                    .HasMaxLength(100);

                entity.Property(x => x.SubSpecialistName)
                    .HasMaxLength(100);

                entity.Property(x => x.MedicalStaffGroup)
                    .HasMaxLength(100);

                entity.Property(x => x.JoinDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ContractStartDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.ContractEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.IsAvailableForAppointment)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.HasOne(x => x.PrimaryDepartment)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PrimaryPosition)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.DoctorCode)
                    .IsUnique();

                entity.HasIndex(x => x.IdentityNumber);

                entity.HasIndex(x => x.Email);

                entity.HasIndex(x => new { x.PrimaryDepartmentId, x.PrimaryPositionId });

                entity.HasIndex(x => x.DoctorType);

                entity.HasIndex(x => x.IsActive);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.Doctor)
                    .HasForeignKey<MstDoctor>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"WorkforceProfileId\" IS NOT NULL");
            });

            builder.Entity<DctLicense>(entity =>
            {
                entity.ToTable("DctLicense", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DoctorId)
                    .IsRequired();

                entity.Property(x => x.LicenseType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.LicenseNumber)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.IssuedBy)
                    .HasMaxLength(200);

                entity.Property(x => x.FilePath)
                    .HasMaxLength(500);

                entity.Property(x => x.IsPrimary)
                    .HasDefaultValue(false);

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

                entity.HasOne(x => x.Doctor)
                    .WithMany(x => x.Licenses)
                    .HasForeignKey(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.DoctorId);
                entity.HasIndex(x => x.LicenseType);
                entity.HasIndex(x => x.LicenseNumber);
            });

            builder.Entity<DctPracticeProfile>(entity =>
            {
                entity.ToTable("DctPracticeProfile", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DoctorId)
                    .IsRequired();

                entity.Property(x => x.PolyclinicName)
                    .HasMaxLength(100);

                entity.Property(x => x.PracticeNote)
                    .HasMaxLength(500);

                entity.Property(x => x.DefaultConsultationDurationMinutes)
                    .HasDefaultValue(15);

                entity.Property(x => x.MaxPatientPerSession)
                    .HasDefaultValue(0);

                entity.Property(x => x.AllowOnlineAppointment)
                    .HasDefaultValue(true);

                entity.Property(x => x.AllowWalkInAppointment)
                    .HasDefaultValue(true);

                entity.Property(x => x.AllowTelemedicine)
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

                entity.HasOne(x => x.Doctor)
                    .WithOne(x => x.PracticeProfile)
                    .HasForeignKey<DctPracticeProfile>(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.DoctorId)
                    .IsUnique();
            });

            builder.Entity<DctFeeProfile>(entity =>
            {
                entity.ToTable("DctFeeProfile", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DoctorId)
                    .IsRequired();

                entity.Property(x => x.ConsultationFee)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.FollowUpFee)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.TelemedicineFee)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.DoctorSharePercentage)
                    .HasColumnType("numeric(5,2)");

                entity.Property(x => x.FeeCalculationType)
                    .HasMaxLength(50);

                entity.Property(x => x.IsFeeActive)
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

                entity.HasOne(x => x.Doctor)
                    .WithOne(x => x.FeeProfile)
                    .HasForeignKey<DctFeeProfile>(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.DoctorId)
                    .IsUnique();
            });

            builder.Entity<MstExternalUser>(entity =>
            {
                entity.ToTable("MstExternalUser", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExternalCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ExternalUserType)
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

                entity.Property(x => x.IdentityNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.TaxNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.BusinessLicenseNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.ExternalStatus)
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

                entity.HasIndex(x => x.ExternalCode)
                    .IsUnique();

                entity.HasIndex(x => x.FullName);
                entity.HasIndex(x => x.ExternalUserType);
                entity.HasIndex(x => x.CompanyName);
                entity.HasIndex(x => x.CompanyCode);
                entity.HasIndex(x => x.Email);
                entity.HasIndex(x => x.IsActive);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.Property(x => x.WorkforceProfileId)
                    .IsRequired(false);

                entity.HasOne(x => x.WorkforceProfile)
                    .WithOne(x => x.ExternalUser)
                    .HasForeignKey<MstExternalUser>(x => x.WorkforceProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WorkforceProfileId)
                    .IsUnique()
                    .HasFilter("\"WorkforceProfileId\" IS NOT NULL");
            });

            builder.Entity<ExtUserContract>(entity =>
            {
                entity.ToTable("ExtUserContract", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExternalUserId)
                    .IsRequired();

                entity.Property(x => x.ContractNumber)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.ContractName)
                    .HasMaxLength(200);

                entity.Property(x => x.ContractType)
                    .HasMaxLength(100);

                entity.Property(x => x.ContractValue)
                    .HasColumnType("numeric(18,2)");

                entity.Property(x => x.PaymentTerm)
                    .HasMaxLength(50);

                entity.Property(x => x.ScopeOfWork)
                    .HasMaxLength(500);

                entity.Property(x => x.FilePath)
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

                entity.HasOne(x => x.ExternalUser)
                    .WithMany(x => x.Contracts)
                    .HasForeignKey(x => x.ExternalUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ExternalUserId);
                entity.HasIndex(x => x.ContractNumber);
                entity.HasIndex(x => x.ContractType);
            });

            builder.Entity<ExtUserDocument>(entity =>
            {
                entity.ToTable("ExtUserDocument", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExternalUserId)
                    .IsRequired();

                entity.Property(x => x.DocumentType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.DocumentName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.DocumentNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.FilePath)
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

                entity.HasOne(x => x.ExternalUser)
                    .WithMany(x => x.Documents)
                    .HasForeignKey(x => x.ExternalUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ExternalUserId);
                entity.HasIndex(x => x.DocumentType);
                entity.HasIndex(x => x.DocumentNumber);
            });

            builder.Entity<EmpAttendance>(entity =>
            {
                entity.ToTable("EmpAttendance", "public");

                entity.HasKey(x => x.Id);

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

                entity.Property(x => x.AttendanceStatus)
                    .HasMaxLength(50)
                    .HasDefaultValue("Present");

                entity.Property(x => x.GeofenceBypassReason)
                    .HasMaxLength(250);                

                entity.Property(x => x.CheckInSource)
                    .HasMaxLength(50)
                    .HasDefaultValue("Login");

                entity.Property(x => x.CheckOutSource)
                    .HasMaxLength(50);

                entity.Property(x => x.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("CheckedIn");

                entity.Property(x => x.CheckInIpAddress)
                    .HasMaxLength(100);

                entity.Property(x => x.CheckOutIpAddress)
                    .HasMaxLength(100);

                entity.Property(x => x.CheckInUserAgent)
                    .HasMaxLength(500);

                entity.Property(x => x.CheckOutUserAgent)
                    .HasMaxLength(500);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.WorkSchedule)
                    .WithMany()
                    .HasForeignKey(x => x.WorkScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.UserId, x.AttendanceDate })
                    .IsUnique();

                entity.HasIndex(x => x.EmployeeId);

                entity.HasIndex(x => x.DoctorId);

                entity.HasIndex(x => x.WorkScheduleId);

                entity.HasIndex(x => x.AttendanceDate);

                entity.HasIndex(x => x.Status);

                entity.HasIndex(x => x.AttendanceStatus);

                entity.HasIndex(x => x.IsLate);

                entity.HasIndex(x => x.IsGeofenceBypassed);
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

                entity.Property(x => x.WorkStartTime)
                    .HasColumnType("time without time zone")
                    .IsRequired();

                entity.Property(x => x.WorkEndTime)
                    .HasColumnType("time without time zone")
                    .IsRequired();

                entity.Property(x => x.EffectiveStartDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.EffectiveEndDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.IsDefault)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ScheduleCode)
                    .IsUnique();

                entity.HasIndex(x => x.UserId);

                entity.HasIndex(x => x.UserType);

                entity.HasIndex(x => new { x.DepartmentId, x.PositionId });

                entity.HasIndex(x => x.IsDefault);

                entity.HasIndex(x => x.IsActive);
            });            
        }
    }
}
