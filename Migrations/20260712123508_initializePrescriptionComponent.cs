using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializePrescriptionComponent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstInsuranceTariff_MstDrug_DrugId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropForeignKey(
                name: "FK_MstInsuranceTariff_MstProcedure_ProcedureId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropForeignKey(
                name: "FK_MstInsuranceTariff_MstTariffCategory_TariffCategoryId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_AspNetUsers_CancelledByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_AspNetUsers_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstCompanyGuarantor_CompanyGua~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstPatientCompanyGuarantor_Pat~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstPatientMembership_PatientMe~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_CancelledByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_EligibilityReferenceNumber_IsD~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_CoveragePriority_I~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_IsPrimary_IsActive~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_GuarantorType_GuarantorStatus_~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_IsEligibilityRequired_IsEligib~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_IsNeedApproval_IsNeedGuarantee~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientId_GuarantorType_IsDele~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_IsEligibilityRequired_IsEligibilityComp~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_IsInsurancePatient_IsCompanyPatient_IsM~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_ServiceUnitId_ClinicId_DoctorId_Encount~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_MstTariffCategory_IsRegistrationFee_IsAdministrationFee_IsC~",
                schema: "public",
                table: "MstTariffCategory");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_ExternalClassCode",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_ExternalServiceCode",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_IsRoomCharge_IsAdministrationFee_IsRegistrationFe~",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_ServiceUnitId_PatientClassId_IsActive_IsDelete",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_DrugId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_ExternalClassCode",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_ExternalServiceCode",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_DrugId_PatientClassI~",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_InsuranceTariffCode_~",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_PatientClassId_IsAct~",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_ProcedureId_PatientC~",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_ProcedureId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_TariffCategoryId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceCoverageRule_BenefitPlanCode",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_BenefitPlanCod~",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_CoverageStatus~",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_ItemType_IsAct~",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropIndex(
                name: "IX_MstDrugCategory_IsCoveredByInsuranceDefault_IsActive_IsDele~",
                schema: "public",
                table: "MstDrugCategory");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_IsCoveredByInsuranceDefault_IsNeedApproval_IsActive~",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_IsNeedPrescription_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "AnnualLimitAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "BenefitSnapshotJson",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CheckMethod",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CoPaymentAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CoPaymentPercent",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CoveragePercent",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "DeductibleAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "EligibilityCheckedAt",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "EligibilityReferenceNumber",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "EstimatedCoveredAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "EstimatedPatientPayAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "GuarantorNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "GuarantorRole",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "GuarantorStatus",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "GuarantorType",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "HasPreviousClaim",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "HasSpecialExclusion",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsAllowExcessPaymentByPatient",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsCardActive",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsEligibilityRequired",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsInWaitingPeriod",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsNeedApproval",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsNeedGuaranteeLetter",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsNeedReferralLetter",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsPremiumPaid",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "ManualCheckResultJson",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "PreviousClaimNote",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "RemainingLimitAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "RoomLimitPerDayAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "SpecialExclusionNote",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "UsedLimitAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "VerificationNote",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "VerificationOfficerName",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "WaitingPeriodUntilDate",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "EligibilityCheckedAt",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "EligibilityReferenceNumber",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsCompanyPatient",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsEligibilityCompleted",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsEligibilityRequired",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsInsurancePatient",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsMembershipPatient",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsMixedPayment",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "PrimaryGuarantorNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "PrimaryGuarantorTypeSnapshot",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsCoveredByInsuranceDefault",
                schema: "public",
                table: "MstTariffCategory");

            migrationBuilder.DropColumn(
                name: "CompanyPrice",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "InsurancePrice",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "MemberPrice",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "DrugId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "IsConsumable",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "IsDrug",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "IsLaboratory",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "IsProcedure",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "IsRadiology",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "IsRoomCharge",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "IsSurgeryRelated",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "PatientClassName",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "ProcedureId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "TariffCategoryId",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "IsCovered",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropColumn(
                name: "IsExcluded",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropColumn(
                name: "PatientClassName",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropColumn(
                name: "ConversionFactor",
                schema: "public",
                table: "MstDrugUnitConversion");

            migrationBuilder.DropColumn(
                name: "IsCoveredByInsuranceDefault",
                schema: "public",
                table: "MstDrugCategory");

            migrationBuilder.DropColumn(
                name: "BaseUnit",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "CompanyPrice",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "DefaultPrice",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "DispenseUnit",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "InsurancePrice",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "MemberPrice",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "RoomName",
                schema: "public",
                table: "MstDoctorSchedule");

            migrationBuilder.RenameColumn(
                name: "VerificationReferenceNumber",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                newName: "PaymentSourceNameSnapshot");

            migrationBuilder.RenameColumn(
                name: "EncounterGuarantorNumber",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                newName: "PaymentSourceNumber");

            migrationBuilder.RenameColumn(
                name: "CoveragePriority",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                newName: "PaymentType");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterGuarantorNumber",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                newName: "IX_TrxPatientEncounterGuarantor_PaymentSourceNumber");

            migrationBuilder.RenameColumn(
                name: "IsCoveredByInsuranceDefault",
                schema: "public",
                table: "MstDrug",
                newName: "IsPrescribable");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEligible",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstTariffCategory",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSurgery",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRoomCharge",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRegistrationFee",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRadiology",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsProcedure",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPharmacy",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPackage",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsLaboratory",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsConsultationFee",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAdministrationFee",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstTariff",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTaxable",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSurgeryRelated",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRoomCharge",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRegistrationFee",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPackageTariff",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedDoctor",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedApproval",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsConsultationFee",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAdministrationFee",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "TariffId",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsingContractPrice",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedApproval",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedGuaranteeLetter",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedApproval",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAllowExcessPaymentByPatient",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientClassId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MstPrescriptionTemplate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TemplateCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OwnerDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsShared = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegularItemCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompoundCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompoundIngredientCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalItemCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPrescriptionTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplate_MstDoctor_OwnerDoctorId",
                        column: x => x.OwnerDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescription",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientInsuranceId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentTypeSnapshot = table.Column<int>(type: "integer", nullable: false),
                    PatientClassNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentSourceNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InsuranceProviderNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PolicyNumberSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BenefitPlanCodeSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BenefitPlanNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PrescriptionStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    FulfillmentStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PrescriptionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingGeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentCompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadyForPharmacyAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PharmacyQueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    PharmacyQueuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PharmacyVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PharmacyVerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreparationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyToDispenseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DispensedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DispensedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DoctorInstruction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PharmacyNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RegularItemCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompoundCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompoundIngredientCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalItemCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CoveredAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PatientPayAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_AspNetUsers_DispensedByUserId",
                        column: x => x.DispensedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_AspNetUsers_PaymentCompletedByUserId",
                        column: x => x.PaymentCompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_AspNetUsers_PharmacyVerifiedByUserId",
                        column: x => x.PharmacyVerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_MstInsuranceProvider_InsuranceProviderId",
                        column: x => x.InsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_MstPatientInsurance_PatientInsuranceId",
                        column: x => x.PatientInsuranceId,
                        principalSchema: "public",
                        principalTable: "MstPatientInsurance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescription_TrxPatientEncounterGuarantor_PaymentSourceId",
                        column: x => x.PaymentSourceId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounterGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPrescriptionTemplateCompound",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompoundName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompoundForm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalPackage = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PackageUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    DosePerUse = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DoseUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    FrequencyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrequencyText = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FrequencyPerDay = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    DurationValue = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    DurationUnit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsAsNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    AdministrationTime = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Signa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompoundingInstruction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdministrationInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DoctorNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPrescriptionTemplateCompound", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateCompound_MstMeasurement_DoseUnitMeas~",
                        column: x => x.DoseUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateCompound_MstMeasurement_PackageUnitM~",
                        column: x => x.PackageUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateCompound_MstPrescriptionTemplate_Pre~",
                        column: x => x.PrescriptionTemplateId,
                        principalSchema: "public",
                        principalTable: "MstPrescriptionTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPrescriptionTemplateItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dose = table.Column<decimal>(type: "numeric", nullable: false),
                    DoseUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    FrequencyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrequencyText = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FrequencyPerDay = table.Column<decimal>(type: "numeric", nullable: true),
                    DurationValue = table.Column<decimal>(type: "numeric", nullable: true),
                    DurationUnit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsAsNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    AdministrationTime = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Signa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdministrationInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DoctorNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    DispenseUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPrescriptionTemplateItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateItem_MstMeasurement_DispenseUnitMeas~",
                        column: x => x.DispenseUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateItem_MstMeasurement_DoseUnitMeasurem~",
                        column: x => x.DoseUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateItem_MstPrescriptionTemplate_Prescri~",
                        column: x => x.PrescriptionTemplateId,
                        principalSchema: "public",
                        principalTable: "MstPrescriptionTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionCompound",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompoundName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompoundForm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalPackage = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 1m),
                    PackageUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    PackageUnitNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PackageUnitSymbolSnapshot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DosePerUse = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 1m),
                    DoseUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoseUnitNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DoseUnitSymbolSnapshot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FrequencyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrequencyText = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FrequencyPerDay = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    DurationValue = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    DurationUnit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsAsNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    AdministrationTime = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Signa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompoundingInstruction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdministrationInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DoctorNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IngredientCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CoveredAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PatientPayAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionCompound", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompound_MstMeasurement_DoseUnitMeasurementId",
                        column: x => x.DoseUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompound_MstMeasurement_PackageUnitMeasureme~",
                        column: x => x.PackageUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompound_TrxPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "TrxPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceTariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceCoverageRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    GenericNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DrugCategoryNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DrugFormSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StrengthSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RouteSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsFormularySnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsGenericSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsAntibioticSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsNarcoticSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsPsychotropicSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsHighAlertSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    Dose = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 1m),
                    DoseUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoseUnitNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DoseUnitSymbolSnapshot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FrequencyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrequencyText = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FrequencyPerDay = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    DurationValue = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    DurationUnit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsAsNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    AdministrationTime = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Signa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdministrationInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DoctorNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 1m),
                    DispenseUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    DispenseUnitNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DispenseUnitSymbolSnapshot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    HospitalUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    ContractUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PricingSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "HospitalTariff"),
                    IsCoverageApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    IsCoveredByInsurance = table.Column<bool>(type: "boolean", nullable: false),
                    CoverageStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "NotApplicable"),
                    CoveragePercent = table.Column<decimal>(type: "numeric(7,2)", nullable: false, defaultValue: 0m),
                    CoveredAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PatientPayAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CoPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false),
                    CoverageNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionItem_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionItem_MstInsuranceCoverageRule_InsuranceCover~",
                        column: x => x.InsuranceCoverageRuleId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceCoverageRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionItem_MstInsuranceTariff_InsuranceTariffId",
                        column: x => x.InsuranceTariffId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionItem_MstMeasurement_DispenseUnitMeasurementId",
                        column: x => x.DispenseUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionItem_MstMeasurement_DoseUnitMeasurementId",
                        column: x => x.DoseUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionItem_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionItem_TrxPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "TrxPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPrescriptionTemplateCompoundItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionTemplateCompoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountPerPackage = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    QuantityUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    IngredientInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPrescriptionTemplateCompoundItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateCompoundItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateCompoundItem_MstMeasurement_Quantity~",
                        column: x => x.QuantityUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPrescriptionTemplateCompoundItem_MstPrescriptionTemplate~",
                        column: x => x.PrescriptionTemplateCompoundId,
                        principalSchema: "public",
                        principalTable: "MstPrescriptionTemplateCompound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionCompoundItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionCompoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceTariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceCoverageRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    GenericNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DrugCategoryNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DrugFormSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StrengthSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RouteSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsFormularySnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsGenericSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsAntibioticSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsNarcoticSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsPsychotropicSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsHighAlertSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    AmountPerPackage = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 1m),
                    TotalQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 1m),
                    QuantityUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityUnitNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QuantityUnitSymbolSnapshot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IngredientInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HospitalUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    ContractUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PricingSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "HospitalTariff"),
                    IsCoverageApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    IsCoveredByInsurance = table.Column<bool>(type: "boolean", nullable: false),
                    CoverageStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "NotApplicable"),
                    CoveragePercent = table.Column<decimal>(type: "numeric(7,2)", nullable: false, defaultValue: 0m),
                    CoveredAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PatientPayAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CoPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false),
                    CoverageNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionCompoundItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompoundItem_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompoundItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompoundItem_MstInsuranceCoverageRule_Insura~",
                        column: x => x.InsuranceCoverageRuleId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceCoverageRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompoundItem_MstInsuranceTariff_InsuranceTar~",
                        column: x => x.InsuranceTariffId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_QuantityUnitMeas~",
                        column: x => x.QuantityUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompoundItem_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionCompoundItem_TrxPrescriptionCompound_Prescri~",
                        column: x => x.PrescriptionCompoundId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionCompound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "EncounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_InsuranceProviderId_BenefitPla~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "InsuranceProviderId", "BenefitPlanCodeSnapshot", "IsPolicyActive", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientId_PaymentType_IsActive~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "PatientId", "PaymentType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_RoomId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_ServiceUnitId_ClinicId_RoomId_DoctorId_~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "ServiceUnitId", "ClinicId", "RoomId", "DoctorId", "EncounterDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_EffectiveStartDate_EffectiveEndDate_IsActive_IsDe~",
                schema: "public",
                table: "MstTariff",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_InsuranceProviderId_BenefitPlanCode_IsE~",
                schema: "public",
                table: "MstPatientInsurance",
                columns: new[] { "InsuranceProviderId", "BenefitPlanCode", "IsEligible", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_PatientId_InsuranceProviderId_BenefitPl~",
                schema: "public",
                table: "MstPatientInsurance",
                columns: new[] { "PatientId", "InsuranceProviderId", "BenefitPlanCode", "EffectiveStartDate", "EffectiveEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_TariffId_BenefitPlan~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "TariffId", "BenefitPlanCode", "PatientClassId", "Priority", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceTariffCode",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "InsuranceTariffCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_BenefitPlanCod~",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                columns: new[] { "InsuranceProviderId", "BenefitPlanCode", "PatientClassId", "ItemType", "Priority", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_PatientClassId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsNeedApproval_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsNeedApproval", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsNeedPrescription_IsPrescribable_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsNeedPrescription", "IsPrescribable", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplate_IsFavorite_OwnerDoctorId_IsDelete",
                schema: "public",
                table: "MstPrescriptionTemplate",
                columns: new[] { "IsFavorite", "OwnerDoctorId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplate_IsShared_IsActive_IsDelete",
                schema: "public",
                table: "MstPrescriptionTemplate",
                columns: new[] { "IsShared", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplate_OwnerDoctorId_TemplateName_IsDelete",
                schema: "public",
                table: "MstPrescriptionTemplate",
                columns: new[] { "OwnerDoctorId", "TemplateName", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplate_TemplateCategory_IsActive_IsDelete",
                schema: "public",
                table: "MstPrescriptionTemplate",
                columns: new[] { "TemplateCategory", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplate_TemplateCode",
                schema: "public",
                table: "MstPrescriptionTemplate",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateCompound_DoseUnitMeasurementId",
                schema: "public",
                table: "MstPrescriptionTemplateCompound",
                column: "DoseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateCompound_PackageUnitMeasurementId",
                schema: "public",
                table: "MstPrescriptionTemplateCompound",
                column: "PackageUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateCompound_PrescriptionTemplateId",
                schema: "public",
                table: "MstPrescriptionTemplateCompound",
                column: "PrescriptionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateCompound_PrescriptionTemplateId_Sort~",
                schema: "public",
                table: "MstPrescriptionTemplateCompound",
                columns: new[] { "PrescriptionTemplateId", "SortOrder", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateCompoundItem_DrugId",
                schema: "public",
                table: "MstPrescriptionTemplateCompoundItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateCompoundItem_PrescriptionTemplateCo~1",
                schema: "public",
                table: "MstPrescriptionTemplateCompoundItem",
                columns: new[] { "PrescriptionTemplateCompoundId", "SortOrder", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateCompoundItem_PrescriptionTemplateCom~",
                schema: "public",
                table: "MstPrescriptionTemplateCompoundItem",
                column: "PrescriptionTemplateCompoundId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateCompoundItem_QuantityUnitMeasurement~",
                schema: "public",
                table: "MstPrescriptionTemplateCompoundItem",
                column: "QuantityUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateItem_DispenseUnitMeasurementId",
                schema: "public",
                table: "MstPrescriptionTemplateItem",
                column: "DispenseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateItem_DoseUnitMeasurementId",
                schema: "public",
                table: "MstPrescriptionTemplateItem",
                column: "DoseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateItem_DrugId",
                schema: "public",
                table: "MstPrescriptionTemplateItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionTemplateItem_PrescriptionTemplateId",
                schema: "public",
                table: "MstPrescriptionTemplateItem",
                column: "PrescriptionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_BillingId",
                schema: "public",
                table: "TrxPrescription",
                column: "BillingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_CancelledByUserId",
                schema: "public",
                table: "TrxPrescription",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_ClinicId",
                schema: "public",
                table: "TrxPrescription",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription",
                column: "ConsultationId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsCancel\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_DispensedByUserId",
                schema: "public",
                table: "TrxPrescription",
                column: "DispensedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_DoctorId",
                schema: "public",
                table: "TrxPrescription",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_DoctorId_PrescriptionDateTime_PrescriptionS~",
                schema: "public",
                table: "TrxPrescription",
                columns: new[] { "DoctorId", "PrescriptionDateTime", "PrescriptionStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_EncounterId",
                schema: "public",
                table: "TrxPrescription",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_EncounterId_PrescriptionStatus_PaymentStatu~",
                schema: "public",
                table: "TrxPrescription",
                columns: new[] { "EncounterId", "PrescriptionStatus", "PaymentStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_InsuranceProviderId",
                schema: "public",
                table: "TrxPrescription",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PatientId",
                schema: "public",
                table: "TrxPrescription",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PatientId_PrescriptionDateTime_IsDelete",
                schema: "public",
                table: "TrxPrescription",
                columns: new[] { "PatientId", "PrescriptionDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PatientInsuranceId",
                schema: "public",
                table: "TrxPrescription",
                column: "PatientInsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PaymentCompletedByUserId",
                schema: "public",
                table: "TrxPrescription",
                column: "PaymentCompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PaymentSourceId",
                schema: "public",
                table: "TrxPrescription",
                column: "PaymentSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PharmacyQueueId",
                schema: "public",
                table: "TrxPrescription",
                column: "PharmacyQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PharmacyVerifiedByUserId",
                schema: "public",
                table: "TrxPrescription",
                column: "PharmacyVerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PrescriptionNumber",
                schema: "public",
                table: "TrxPrescription",
                column: "PrescriptionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_PrescriptionStatus_PaymentStatus_Fulfillmen~",
                schema: "public",
                table: "TrxPrescription",
                columns: new[] { "PrescriptionStatus", "PaymentStatus", "FulfillmentStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_ServiceUnitId",
                schema: "public",
                table: "TrxPrescription",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_SubmittedByUserId",
                schema: "public",
                table: "TrxPrescription",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompound_DoseUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompound",
                column: "DoseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompound_PackageUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompound",
                column: "PackageUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompound_PrescriptionId",
                schema: "public",
                table: "TrxPrescriptionCompound",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompound_PrescriptionId_IsNeedApproval_IsApp~",
                schema: "public",
                table: "TrxPrescriptionCompound",
                columns: new[] { "PrescriptionId", "IsNeedApproval", "IsApproved", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompound_PrescriptionId_SortOrder_IsDelete",
                schema: "public",
                table: "TrxPrescriptionCompound",
                columns: new[] { "PrescriptionId", "SortOrder", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_ApprovedByUserId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_DrugId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_InsuranceCoverageRuleId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "InsuranceCoverageRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_InsuranceTariffId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "InsuranceTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_PrescriptionCompoundId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "PrescriptionCompoundId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_PrescriptionCompoundId_IsNeedAp~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                columns: new[] { "PrescriptionCompoundId", "IsNeedApproval", "IsApproved", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_PrescriptionCompoundId_SortOrde~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                columns: new[] { "PrescriptionCompoundId", "SortOrder", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_QuantityUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "QuantityUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_TariffId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_ApprovedByUserId",
                schema: "public",
                table: "TrxPrescriptionItem",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_DispenseUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionItem",
                column: "DispenseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_DoseUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionItem",
                column: "DoseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_DrugId",
                schema: "public",
                table: "TrxPrescriptionItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_InsuranceCoverageRuleId",
                schema: "public",
                table: "TrxPrescriptionItem",
                column: "InsuranceCoverageRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_InsuranceTariffId",
                schema: "public",
                table: "TrxPrescriptionItem",
                column: "InsuranceTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_PrescriptionId",
                schema: "public",
                table: "TrxPrescriptionItem",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_PrescriptionId_IsNeedApproval_IsApprove~",
                schema: "public",
                table: "TrxPrescriptionItem",
                columns: new[] { "PrescriptionId", "IsNeedApproval", "IsApproved", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_PrescriptionId_SortOrder_IsDelete",
                schema: "public",
                table: "TrxPrescriptionItem",
                columns: new[] { "PrescriptionId", "SortOrder", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionItem_TariffId",
                schema: "public",
                table: "TrxPrescriptionItem",
                column: "TariffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstInsuranceCoverageRule_MstPatientClass_PatientClassId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "PatientClassId",
                principalSchema: "public",
                principalTable: "MstPatientClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstRoom_RoomId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "RoomId",
                principalSchema: "public",
                principalTable: "MstRoom",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstInsuranceCoverageRule_MstPatientClass_PatientClassId",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstRoom_RoomId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropTable(
                name: "MstPrescriptionTemplateCompoundItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPrescriptionTemplateItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionCompoundItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPrescriptionTemplateCompound",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionCompound",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPrescriptionTemplate",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescription",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_InsuranceProviderId_BenefitPla~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientId_PaymentType_IsActive~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_RoomId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_ServiceUnitId_ClinicId_RoomId_DoctorId_~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_EffectiveStartDate_EffectiveEndDate_IsActive_IsDe~",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientInsurance_InsuranceProviderId_BenefitPlanCode_IsE~",
                schema: "public",
                table: "MstPatientInsurance");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientInsurance_PatientId_InsuranceProviderId_BenefitPl~",
                schema: "public",
                table: "MstPatientInsurance");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_TariffId_BenefitPlan~",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceTariff_InsuranceTariffCode",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_BenefitPlanCod~",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropIndex(
                name: "IX_MstInsuranceCoverageRule_PatientClassId",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_IsNeedApproval_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropIndex(
                name: "IX_MstDrug_IsNeedPrescription_IsPrescribable_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug");

            migrationBuilder.DropColumn(
                name: "RoomId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "public",
                table: "MstInsuranceTariff");

            migrationBuilder.DropColumn(
                name: "PatientClassId",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "public",
                table: "MstInsuranceCoverageRule");

            migrationBuilder.RenameColumn(
                name: "PaymentType",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                newName: "CoveragePriority");

            migrationBuilder.RenameColumn(
                name: "PaymentSourceNumber",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                newName: "EncounterGuarantorNumber");

            migrationBuilder.RenameColumn(
                name: "PaymentSourceNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                newName: "VerificationReferenceNumber");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPatientEncounterGuarantor_PaymentSourceNumber",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                newName: "IX_TrxPatientEncounterGuarantor_EncounterGuarantorNumber");

            migrationBuilder.RenameColumn(
                name: "IsPrescribable",
                schema: "public",
                table: "MstDrug",
                newName: "IsCoveredByInsuranceDefault");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEligible",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualLimitAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BenefitSnapshotJson",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckMethod",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CoPaymentAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoPaymentPercent",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoveragePercent",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeductibleAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EligibilityCheckedAt",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityReferenceNumber",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCoveredAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedPatientPayAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuarantorNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuarantorRole",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GuarantorStatus",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GuarantorType",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasPreviousClaim",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasSpecialExclusion",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowExcessPaymentByPatient",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCardActive",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibilityRequired",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInWaitingPeriod",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNeedApproval",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNeedGuaranteeLetter",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNeedReferralLetter",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPremiumPaid",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManualCheckResultJson",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousClaimNote",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingLimitAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoomLimitPerDayAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialExclusionNote",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UsedLimitAmount",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNote",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationOfficerName",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaitingPeriodUntilDate",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EligibilityCheckedAt",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityReferenceNumber",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompanyPatient",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibilityCompleted",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibilityRequired",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInsurancePatient",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMembershipPatient",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMixedPayment",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryGuarantorNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryGuarantorTypeSnapshot",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstTariffCategory",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSurgery",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRoomCharge",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRegistrationFee",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRadiology",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsProcedure",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPharmacy",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPackage",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsLaboratory",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsConsultationFee",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAdministrationFee",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "IsCoveredByInsuranceDefault",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstTariff",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTaxable",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSurgeryRelated",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRoomCharge",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRegistrationFee",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPackageTariff",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedDoctor",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedApproval",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsConsultationFee",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAdministrationFee",
                schema: "public",
                table: "MstTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<decimal>(
                name: "CompanyPrice",
                schema: "public",
                table: "MstTariff",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsurancePrice",
                schema: "public",
                table: "MstTariff",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberPrice",
                schema: "public",
                table: "MstTariff",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                schema: "public",
                table: "MstTariff",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TariffId",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsingContractPrice",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedApproval",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<Guid>(
                name: "DrugId",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConsumable",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDrug",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLaboratory",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProcedure",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRadiology",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRoomCharge",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSurgeryRelated",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PatientClassName",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcedureId",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TariffCategoryId",
                schema: "public",
                table: "MstInsuranceTariff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedGuaranteeLetter",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsNeedApproval",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAllowExcessPaymentByPatient",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "IsCovered",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExcluded",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PatientClassName",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionFactor",
                schema: "public",
                table: "MstDrugUnitConversion",
                type: "numeric(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCoveredByInsuranceDefault",
                schema: "public",
                table: "MstDrugCategory",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseUnit",
                schema: "public",
                table: "MstDrug",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompanyPrice",
                schema: "public",
                table: "MstDrug",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultPrice",
                schema: "public",
                table: "MstDrug",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DispenseUnit",
                schema: "public",
                table: "MstDrug",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsurancePrice",
                schema: "public",
                table: "MstDrug",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MemberPrice",
                schema: "public",
                table: "MstDrug",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomName",
                schema: "public",
                table: "MstDoctorSchedule",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_CancelledByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EligibilityReferenceNumber_IsD~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "EligibilityReferenceNumber", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_CoveragePriority_I~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "EncounterId", "CoveragePriority", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_IsPrimary_IsActive~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "EncounterId", "IsPrimary", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_GuarantorType_GuarantorStatus_~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "GuarantorType", "GuarantorStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_IsEligibilityRequired_IsEligib~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "IsEligibilityRequired", "IsEligible", "GuarantorStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_IsNeedApproval_IsNeedGuarantee~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "IsNeedApproval", "IsNeedGuaranteeLetter", "IsNeedReferralLetter" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientCompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientId_GuarantorType_IsDele~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "PatientId", "GuarantorType", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_IsEligibilityRequired_IsEligibilityComp~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "IsEligibilityRequired", "IsEligibilityCompleted", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_IsInsurancePatient_IsCompanyPatient_IsM~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "IsInsurancePatient", "IsCompanyPatient", "IsMixedPayment", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_ServiceUnitId_ClinicId_DoctorId_Encount~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "ServiceUnitId", "ClinicId", "DoctorId", "EncounterDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariffCategory_IsRegistrationFee_IsAdministrationFee_IsC~",
                schema: "public",
                table: "MstTariffCategory",
                columns: new[] { "IsRegistrationFee", "IsAdministrationFee", "IsConsultationFee", "IsRoomCharge", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ExternalClassCode",
                schema: "public",
                table: "MstTariff",
                column: "ExternalClassCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ExternalServiceCode",
                schema: "public",
                table: "MstTariff",
                column: "ExternalServiceCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_IsRoomCharge_IsAdministrationFee_IsRegistrationFe~",
                schema: "public",
                table: "MstTariff",
                columns: new[] { "IsRoomCharge", "IsAdministrationFee", "IsRegistrationFee", "IsConsultationFee", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ServiceUnitId_PatientClassId_IsActive_IsDelete",
                schema: "public",
                table: "MstTariff",
                columns: new[] { "ServiceUnitId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_DrugId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_ExternalClassCode",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "ExternalClassCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_ExternalServiceCode",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "ExternalServiceCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_DrugId_PatientClassI~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "DrugId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_InsuranceTariffCode_~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "InsuranceTariffCode", "ExternalClassCode", "BenefitPlanCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_PatientClassId_IsAct~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_ProcedureId_PatientC~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "ProcedureId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_ProcedureId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_TariffCategoryId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "TariffCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_BenefitPlanCode",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "BenefitPlanCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_BenefitPlanCod~",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                columns: new[] { "InsuranceProviderId", "BenefitPlanCode", "ItemType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_CoverageStatus~",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                columns: new[] { "InsuranceProviderId", "CoverageStatus", "IsNeedApproval", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_ItemType_IsAct~",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                columns: new[] { "InsuranceProviderId", "ItemType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugCategory_IsCoveredByInsuranceDefault_IsActive_IsDele~",
                schema: "public",
                table: "MstDrugCategory",
                columns: new[] { "IsCoveredByInsuranceDefault", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsCoveredByInsuranceDefault_IsNeedApproval_IsActive~",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsCoveredByInsuranceDefault", "IsNeedApproval", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsNeedPrescription_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsNeedPrescription", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_MstInsuranceTariff_MstDrug_DrugId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "DrugId",
                principalSchema: "public",
                principalTable: "MstDrug",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstInsuranceTariff_MstProcedure_ProcedureId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "ProcedureId",
                principalSchema: "public",
                principalTable: "MstProcedure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstInsuranceTariff_MstTariffCategory_TariffCategoryId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "TariffCategoryId",
                principalSchema: "public",
                principalTable: "MstTariffCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_AspNetUsers_CancelledByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "CancelledByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_AspNetUsers_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "VerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstCompanyGuarantor_CompanyGua~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "CompanyGuarantorId",
                principalSchema: "public",
                principalTable: "MstCompanyGuarantor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstPatientCompanyGuarantor_Pat~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientCompanyGuarantorId",
                principalSchema: "public",
                principalTable: "MstPatientCompanyGuarantor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstPatientMembership_PatientMe~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientMembershipId",
                principalSchema: "public",
                principalTable: "MstPatientMembership",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
