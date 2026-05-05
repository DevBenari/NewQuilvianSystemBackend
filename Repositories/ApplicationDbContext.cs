using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Models;

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

        public DbSet<DctLicense> DctLicenses { get; set; }
        public DbSet<DctPracticeProfile> DctPracticeProfiles { get; set; }
        public DbSet<DctFeeProfile> DctFeeProfiles { get; set; }

        public DbSet<ExtUserContract> ExtUserContracts { get; set; }
        public DbSet<ExtUserDocument> ExtUserDocuments { get; set; }

        public DbSet<EmpAttendance> EmpAttendances { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.UserCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.UserType)
                    .IsRequired();

                entity.Property(x => x.BirthDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.IdentityNumber)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(30);

                entity.Property(x => x.ProfilePhotoPath)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.MustChangePassword)
                    .HasDefaultValue(true);

                entity.Property(x => x.LastLoginAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.AccessValidUntil)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.Property(x => x.CreateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.UpdateDateTime)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

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

                entity.HasIndex(x => x.UserCode)
                    .IsUnique();

                entity.HasIndex(x => x.IdentityNumber);

                entity.HasIndex(x => x.DepartmentId);

                entity.HasIndex(x => x.PositionId);

                entity.HasIndex(x => x.UserType);

                entity.HasIndex(x => x.EmployeeId);

                entity.HasIndex(x => x.DoctorId);

                entity.HasIndex(x => x.ExternalUserId);

                entity.HasIndex(x => x.IsActive);
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

                entity.Property(x => x.SortOrder)
                    .HasDefaultValue(0);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasIndex(x => x.ModuleCode)
                    .IsUnique();

                entity.HasIndex(x => x.ModuleName);
            });

            builder.Entity<SysControllerAccess>(entity =>
            {
                entity.ToTable("SysControllerAccess", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ModuleId)
                    .IsRequired();

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

                entity.Property(x => x.SortOrder)
                    .HasDefaultValue(0);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.Module)
                    .WithMany(x => x.Controllers)
                    .HasForeignKey(x => x.ModuleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.ModuleId, x.ControllerName })
                    .IsUnique();

                entity.HasIndex(x => x.DisplayName);
            });

            builder.Entity<SysActionAccess>(entity =>
            {
                entity.ToTable("SysActionAccess", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ControllerAccessId)
                    .IsRequired();

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

                entity.Property(x => x.SortOrder)
                    .HasDefaultValue(0);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreateDateTime)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(x => x.IsDelete)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsCancel)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.ControllerAccess)
                    .WithMany(x => x.Actions)
                    .HasForeignKey(x => x.ControllerAccessId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.ControllerAccessId, x.ActionName })
                    .IsUnique();

                entity.HasIndex(x => x.DisplayName);
            });

            builder.Entity<SysAccessPolicy>(entity =>
            {
                entity.ToTable("SysAccessPolicy", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DepartmentId)
                    .IsRequired();

                entity.Property(x => x.PositionId)
                    .IsRequired();

                entity.Property(x => x.ControllerAccessId)
                    .IsRequired();

                entity.Property(x => x.ActionAccessId)
                    .IsRequired();

                entity.Property(x => x.IsAllowed)
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

                entity.HasIndex(x => x.DepartmentId);
                entity.HasIndex(x => x.PositionId);
                entity.HasIndex(x => x.ControllerAccessId);
                entity.HasIndex(x => x.ActionAccessId);
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

            builder.Entity<MstEmployee>(entity =>
            {
                entity.ToTable("MstEmployee", "public");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EmployeeCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.EmployeeNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.AttendanceNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.NickName)
                    .HasMaxLength(100);

                entity.Property(x => x.BirthDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.BirthPlace)
                    .HasMaxLength(100);

                entity.Property(x => x.Religion)
                    .HasMaxLength(50);

                entity.Property(x => x.MaritalStatus)
                    .HasMaxLength(50);

                entity.Property(x => x.BloodType)
                    .HasMaxLength(50);

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

                entity.Property(x => x.Province)
                    .HasMaxLength(100);

                entity.Property(x => x.City)
                    .HasMaxLength(100);

                entity.Property(x => x.District)
                    .HasMaxLength(100);

                entity.Property(x => x.Village)
                    .HasMaxLength(100);

                entity.Property(x => x.PostalCode)
                    .HasMaxLength(20);

                entity.Property(x => x.DepartmentId)
                    .IsRequired();

                entity.Property(x => x.PositionId)
                    .IsRequired();

                entity.Property(x => x.EmployeeStatus)
                    .IsRequired();

                entity.Property(x => x.EmploymentType)
                    .HasMaxLength(50);

                entity.Property(x => x.GradeLevel)
                    .HasMaxLength(50);

                entity.Property(x => x.WorkLocation)
                    .HasMaxLength(50);

                entity.Property(x => x.ResignReason)
                    .HasMaxLength(250);

                entity.Property(x => x.EmergencyContactName)
                    .HasMaxLength(200);

                entity.Property(x => x.EmergencyContactRelation)
                    .HasMaxLength(50);

                entity.Property(x => x.EmergencyContactPhone)
                    .HasMaxLength(30);

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

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.EmployeeCode)
                    .IsUnique();

                entity.HasIndex(x => x.EmployeeNumber)
                    .IsUnique();

                entity.HasIndex(x => x.AttendanceNumber);
                entity.HasIndex(x => x.FullName);
                entity.HasIndex(x => x.Email);
                entity.HasIndex(x => x.IdentityNumber);
                entity.HasIndex(x => x.DepartmentId);
                entity.HasIndex(x => x.PositionId);
                entity.HasIndex(x => x.EmployeeStatus);
                entity.HasIndex(x => x.IsActive);
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
                    .IsRequired();

                entity.Property(x => x.IdentityType)
                    .HasMaxLength(50);

                entity.Property(x => x.IdentityNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.BirthDate)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(x => x.BirthPlace)
                    .HasMaxLength(100);

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

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Position)
                    .WithMany()
                    .HasForeignKey(x => x.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.DoctorCode)
                    .IsUnique();

                entity.HasIndex(x => x.FullName);
                entity.HasIndex(x => x.DoctorType);
                entity.HasIndex(x => x.IdentityNumber);
                entity.HasIndex(x => x.DepartmentId);
                entity.HasIndex(x => x.PositionId);
                entity.HasIndex(x => x.IsActive);
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

                entity.Property(x => x.PersonType)
                    .HasMaxLength(50)
                    .IsRequired();

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

                entity.HasIndex(x => new { x.UserId, x.AttendanceDate })
                    .IsUnique();

                entity.HasIndex(x => x.EmployeeId);

                entity.HasIndex(x => x.DoctorId);

                entity.HasIndex(x => x.AttendanceDate);

                entity.HasIndex(x => x.Status);
            });
        }
    }
}
