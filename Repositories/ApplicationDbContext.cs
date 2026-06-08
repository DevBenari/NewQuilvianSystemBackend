using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region GLOBAL
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
        public DbSet<MstBank> MstBanks { get; set; }
        #endregion GLOBAL

        #region CORPORATE
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
        #endregion CORPORATE

        #region HEALTH SERVICE
        public DbSet<MstKioskDevice> MstKioskDevices { get; set; }
        public DbSet<MstIdentityScannerProfile> MstIdentityScannerProfiles { get; set; }
        public DbSet<MstServiceUnit> MstServiceUnits { get; set; }
        public DbSet<MstClinic> MstClinics { get; set; }
        public DbSet<MstPatientClass> MstPatientClasses { get; set; }
        public DbSet<MstTariffCategory> MstTariffCategories { get; set; }
        public DbSet<MstTariff> MstTariffs { get; set; }
        public DbSet<MstRoom> MstRooms { get; set; }
        public DbSet<MstBed> MstBeds { get; set; }
        public DbSet<MstMembershipTier> MstMembershipTiers { get; set; }
        public DbSet<MstPatient> MstPatients { get; set; }
        public DbSet<MstPatientMembership> MstPatientMemberships { get; set; }
        public DbSet<MstPatientIdentityDocument> MstPatientIdentityDocuments { get; set; }
        public DbSet<MstPatientRelationship> MstPatientRelationships { get; set; }
        public DbSet<MstPatientEmergencyContact> MstPatientEmergencyContacts { get; set; }
        public DbSet<MstInsuranceProvider> MstInsuranceProviders { get; set; }
        public DbSet<MstPatientInsurance> MstPatientInsurances { get; set; }
        public DbSet<MstCompanyGuarantor> MstCompanyGuarantors { get; set; }
        public DbSet<MstPatientCompanyGuarantor> MstPatientCompanyGuarantors { get; set; }
        public DbSet<MstPaymentMethod> MstPaymentMethods { get; set; }
        public DbSet<MstBillingItemCategory> MstBillingItemCategories { get; set; }
        public DbSet<MstProcedure> MstProcedures { get; set; }        
        public DbSet<MstDiagnosisChapter> MstDiagnosisChapters { get; set; }
        public DbSet<MstDiagnosis> MstDiagnoses { get; set; }
        public DbSet<MstMeasurement> MstMeasurements { get; set; }
        public DbSet<MstMeasurementConversion> MstMeasurementConversions { get; set; }
        public DbSet<MstDrugUnitConversion> MstDrugUnitConversions { get; set; }
        public DbSet<MstDrugStorageLocation> MstDrugStorageLocations { get; set; }
        public DbSet<MstDrugStockPolicy> MstDrugStockPolicies { get; set; }
        public DbSet<MstDrugCategory> MstDrugCategories { get; set; }
        public DbSet<MstDrug> MstDrugs { get; set; }
        public DbSet<MstInsuranceCoverageRule> MstInsuranceCoverageRules { get; set; }
        public DbSet<MstInsuranceTariff> MstInsuranceTariffs { get; set; }
        public DbSet<MstDoctorSchedule> MstDoctorSchedules { get; set; }
        public DbSet<MstDoctorServiceRule> MstDoctorServiceRules { get; set; }
        public DbSet<TrxKioskScanSession> TrxKioskScanSessions { get; set; }
        public DbSet<TrxPatientEncounter> TrxPatientEncounters { get; set; }
        public DbSet<TrxPatientEncounterGuarantor> TrxPatientEncounterGuarantors { get; set; }
        public DbSet<TrxQueue> TrxQueues { get; set; }
        public DbSet<TrxPatientAssessment> TrxPatientAssessments { get; set; }
        public DbSet<TrxDoctorConsultation> TrxDoctorConsultations { get; set; }
        public DbSet<TrxPatientDiagnosis> TrxPatientDiagnoses { get; set; }
        public DbSet<TrxPatientProcedure> TrxPatientProcedures { get; set; }
        public DbSet<TrxPatientAllergy> TrxPatientAllergies { get; set; }
        public DbSet<TrxPatientMedicalHistory> TrxPatientMedicalHistories { get; set; }
        public DbSet<TrxPatientFamilyHistory> TrxPatientFamilyHistories { get; set; }
        public DbSet<TrxPatientVitalSign> TrxPatientVitalSigns { get; set; }
        public DbSet<TrxPatientClinicalDocument> TrxPatientClinicalDocuments { get; set; }
        public DbSet<TrxPatientConsent> TrxPatientConsents { get; set; }
        public DbSet<TrxMedicalCertificate> TrxMedicalCertificates { get; set; }
        public DbSet<TrxClinicalNoteAttachment> TrxClinicalNoteAttachments { get; set; }
        #endregion HEALTH SERVICE

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}