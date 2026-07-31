using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initialiazeleaveCorePeriodBalanceLedgerFoundationV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_LeavePolicyId",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_LeaveTypeId_Year_IsActive_IsDelete",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_LeaveTypeId_Year",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_PeriodStartDate_PeriodEn~",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_EntitlementStatus_ExpiryDate_IsActive_I~",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_LeaveBalanceId",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_LeavePolicyId",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_WorkforceProfileId_LeaveTypeId_Entitlem~",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveBalanceId_TransactionDateTi~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_SourceType_SourceReferenceId_IsD~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_WorkforceProfileId_LeaveTypeId_T~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_AccrualStatus_AccrualDate_IsActive_IsDelete",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_LeaveBalanceId",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_LeaveEntitlementId",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_WorkforceProfileId_LeaveTypeId_AccrualPerio~",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveType_LeaveCategory",
                schema: "public",
                table: "MstLeaveType");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_EffectiveStartDate_EffectiveEndDate_IsActive~",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_EmployeeCategoryId_EmploymentTypeId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_LeaveTypeId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_LeaveTypeId_IsDefault_IsActive_IsDelete",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_LegalEntityId_HospitalSiteId_OrganizationUni~",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveEntitlementPolicy_EffectiveStartDate_EffectiveEndDa~",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveEntitlementPolicy_LeavePolicyId",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveCarryForwardPolicy_EffectiveStartDate_EffectiveEndD~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveCarryForwardPolicy_ExpiryMethod_IsPayoutAllowed_Exc~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveCarryForwardPolicy_LeaveEntitlementPolicyId",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.AlterColumn<decimal>(
                name: "UsedDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ReservedDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "RemainingDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "RecalledDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PendingDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBalanceDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<bool>(
                name: "IsLocked",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExpiredDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "EntitlementDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "EncashmentDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompensatoryDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CarriedForwardDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdjustmentDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AccruedDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "BalanceStatus",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<long>(
                name: "BalanceVersion",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CarryForwardExpiryDate",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReconciledAt",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastTransactionId",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastTransactionSequence",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveEntitlementPeriodId",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "ServiceMonthsAtGrant",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProratedEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<bool>(
                name: "IsProrated",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<decimal>(
                name: "CarryForwardEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AvailableFromDate",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CalculationVersion",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "EntitlementTransactionId",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "GrantDate",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionType",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ManualAdjustment",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "TransactionDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDateTime",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousReservedDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousOpeningBalanceDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousAvailableDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewUsedDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewReservedDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewAvailableDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Direction",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Credit",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<decimal>(
                name: "AccruedDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustmentDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CarryForwardDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CompensatoryDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveDate",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EncashmentDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EntitlementDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpiredDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalanceDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalTransactionId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PendingDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PostingBatchId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostingBatchType",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RecalledDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "TransactionSequence",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "UsedDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<bool>(
                name: "IsProrated",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<decimal>(
                name: "BalanceBeforeAccrual",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "BalanceAfterAccrual",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AccrualAmountDays",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "AccrualSequence",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "BalanceTransactionId",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveAccrualRunId",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ScheduledAccrualDate",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractTypeId",
                schema: "public",
                table: "MstLeavePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DayCalculationMethod",
                schema: "public",
                table: "MstLeavePolicy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ScheduledWorkDays");

            migrationBuilder.AddColumn<string>(
                name: "DeductionTiming",
                schema: "public",
                table: "MstLeavePolicy",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "OnApproval");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "public",
                table: "MstLeavePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmploymentStatusId",
                schema: "public",
                table: "MstLeavePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFallback",
                schema: "public",
                table: "MstLeavePolicy",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "NegativeBalanceLimitDays",
                schema: "public",
                table: "MstLeavePolicy",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                schema: "public",
                table: "MstLeavePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "public",
                table: "MstLeavePolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReservationTiming",
                schema: "public",
                table: "MstLeavePolicy",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "OnSubmit");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkLocationId",
                schema: "public",
                table: "MstLeavePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceTypeId",
                schema: "public",
                table: "MstLeavePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumBalanceDays",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldPrecision: 8,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualEntitlementDays",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldPrecision: 8,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AccrualAmountDays",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldPrecision: 8,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AccrualDayOfMonth",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AccrualMaximumPerPeriodDays",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccrualTiming",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "EndOfPeriod");

            migrationBuilder.AddColumn<string>(
                name: "FinalAccrualRule",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Prorated");

            migrationBuilder.AddColumn<string>(
                name: "FirstAccrualRule",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Prorated");

            migrationBuilder.AddColumn<string>(
                name: "GrantTiming",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "StartOfPeriod");

            migrationBuilder.AddColumn<string>(
                name: "PeriodBasis",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "CalendarYear");

            migrationBuilder.AlterColumn<decimal>(
                name: "PayoutMaximumDays",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldPrecision: 8,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumCarryForwardDays",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldPrecision: 8,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarryForwardExecutionTiming",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PeriodClose");

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationLeaveTypeId",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaximumCarryForwardPeriods",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "integer",
                nullable: true,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumCarryForwardDays",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoundingMethod",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.CreateTable(
                name: "TrxLeaveEntitlementPeriod",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PeriodName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PeriodBasis = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "CalendarYear"),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProcessingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingStartedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CloseReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReopenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReopenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReopenCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastReconciledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidationSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_TrxLeaveEntitlementPeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLeaveEntitlementPeriod_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveEntitlementPeriod_AspNetUsers_ProcessingStartedByUs~",
                        column: x => x.ProcessingStartedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveEntitlementPeriod_AspNetUsers_ReopenedByUserId",
                        column: x => x.ReopenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveEntitlementPeriod_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveEntitlementPeriod_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveEntitlementPeriod_MstLeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveEntitlementPeriod_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveEntitlementPeriod_MstOrganizationUnit_OrganizationU~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_LastTransactionId",
                schema: "public",
                table: "WfpLeaveBalance",
                column: "LastTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_LeaveEntitlementPeriodId_BalanceStatus_IsLo~",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "LeaveEntitlementPeriodId", "BalanceStatus", "IsLocked", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_LeavePolicyId_LeaveEntitlementPolicyId",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "LeavePolicyId", "LeaveEntitlementPolicyId" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_LeaveTypeId",
                schema: "public",
                table: "WfpLeaveBalance",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_LeaveTypeId_LeaveEntitle~",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "LeaveEntitlementPeriodId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"LeaveEntitlementPeriodId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_LeaveTypeId_Year_IsActiv~",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "Year", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_EntitlementTransactionId",
                schema: "public",
                table: "TrxLeaveEntitlement",
                column: "EntitlementTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveEntitlement",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_LeaveBalanceId_EntitlementYear_IsActive~",
                schema: "public",
                table: "TrxLeaveEntitlement",
                columns: new[] { "LeaveBalanceId", "EntitlementYear", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_LeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveEntitlement",
                column: "LeaveEntitlementPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_LeavePolicyId_LeaveEntitlementPolicyId",
                schema: "public",
                table: "TrxLeaveEntitlement",
                columns: new[] { "LeavePolicyId", "LeaveEntitlementPolicyId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_WorkforceProfileId_LeaveTypeId_LeaveEnt~",
                schema: "public",
                table: "TrxLeaveEntitlement",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "LeaveEntitlementPeriodId", "EntitlementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveBalanceId_TransactionSequen~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                columns: new[] { "LeaveBalanceId", "TransactionSequence" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"TransactionSequence\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveEntitlementPeriodId_Transac~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                columns: new[] { "LeaveEntitlementPeriodId", "TransactionStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_OriginalTransactionId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                column: "OriginalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_PostingBatchType_PostingBatchId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                columns: new[] { "PostingBatchType", "PostingBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_SourceType_SourceReferenceId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                columns: new[] { "SourceType", "SourceReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_WorkforceProfileId_LeaveTypeId_E~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_BalanceTransactionId",
                schema: "public",
                table: "TrxLeaveAccrual",
                column: "BalanceTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveAccrual",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_LeaveBalanceId_LeaveAccrualRunId_IsActive_I~",
                schema: "public",
                table: "TrxLeaveAccrual",
                columns: new[] { "LeaveBalanceId", "LeaveAccrualRunId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_LeaveEntitlementId_AccrualPeriodStartDate_A~",
                schema: "public",
                table: "TrxLeaveAccrual",
                columns: new[] { "LeaveEntitlementId", "AccrualPeriodStartDate", "AccrualPeriodEndDate", "AccrualSequence" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"LeaveEntitlementId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_WorkforceProfileId_LeaveTypeId_AccrualDate_~",
                schema: "public",
                table: "TrxLeaveAccrual",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "AccrualDate", "AccrualStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_ContractTypeId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_DepartmentId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_EffectiveStartDate_EffectiveEndDate",
                schema: "public",
                table: "MstLeavePolicy",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_EmployeeCategoryId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "EmployeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_EmploymentStatusId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "EmploymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_LeaveTypeId_Priority_IsFallback_IsDefault_Is~",
                schema: "public",
                table: "MstLeavePolicy",
                columns: new[] { "LeaveTypeId", "Priority", "IsFallback", "IsDefault", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_LegalEntityId_HospitalSiteId_OrganizationUni~",
                schema: "public",
                table: "MstLeavePolicy",
                columns: new[] { "LegalEntityId", "HospitalSiteId", "OrganizationUnitId", "DepartmentId", "PositionId", "WorkLocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_PositionId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_WorkforceTypeId_EmployeeCategoryId_Employmen~",
                schema: "public",
                table: "MstLeavePolicy",
                columns: new[] { "WorkforceTypeId", "EmployeeCategoryId", "EmploymentTypeId", "EmploymentStatusId", "ContractTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_WorkLocationId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveEntitlementPolicy_EffectiveStartDate_EffectiveEndDa~",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveCarryForwardPolicy_DestinationLeaveTypeId_ExpiryMet~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                columns: new[] { "DestinationLeaveTypeId", "ExpiryMethod", "ExcessBalanceAction" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveCarryForwardPolicy_EffectiveStartDate_EffectiveEndD~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_ClosedByUserId",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_DepartmentId",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_HospitalSiteId",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_LeaveTypeId_PeriodYear_IsActive_I~",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                columns: new[] { "LeaveTypeId", "PeriodYear", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_LegalEntityId_HospitalSiteId_Orga~",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                columns: new[] { "LegalEntityId", "HospitalSiteId", "OrganizationUnitId", "DepartmentId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_OrganizationUnitId",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_PeriodCode",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                column: "PeriodCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_PeriodStatus_IsLocked_IsActive_Is~",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                columns: new[] { "PeriodStatus", "IsLocked", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_ProcessingStartedByUserId",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                column: "ProcessingStartedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_ReopenedByUserId",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                column: "ReopenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlementPeriod_StartDate_EndDate_PeriodStatus",
                schema: "public",
                table: "TrxLeaveEntitlementPeriod",
                columns: new[] { "StartDate", "EndDate", "PeriodStatus" });

            migrationBuilder.AddForeignKey(
                name: "FK_MstLeaveCarryForwardPolicy_MstLeaveType_DestinationLeaveTyp~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                column: "DestinationLeaveTypeId",
                principalSchema: "public",
                principalTable: "MstLeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstLeavePolicy_MstContractType_ContractTypeId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "ContractTypeId",
                principalSchema: "public",
                principalTable: "MstContractType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstLeavePolicy_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstLeavePolicy_MstEmploymentStatus_EmploymentStatusId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "EmploymentStatusId",
                principalSchema: "public",
                principalTable: "MstEmploymentStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstLeavePolicy_MstPosition_PositionId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstLeavePolicy_MstWorkforceType_WorkforceTypeId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "WorkforceTypeId",
                principalSchema: "public",
                principalTable: "MstWorkforceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstLeavePolicy_MstWorkLocation_WorkLocationId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "WorkLocationId",
                principalSchema: "public",
                principalTable: "MstWorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxLeaveAccrual_TrxLeaveBalanceTransaction_BalanceTransacti~",
                schema: "public",
                table: "TrxLeaveAccrual",
                column: "BalanceTransactionId",
                principalSchema: "public",
                principalTable: "TrxLeaveBalanceTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxLeaveBalanceTransaction_TrxLeaveBalanceTransaction_Origi~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                column: "OriginalTransactionId",
                principalSchema: "public",
                principalTable: "TrxLeaveBalanceTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxLeaveBalanceTransaction_TrxLeaveEntitlementPeriod_LeaveE~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                column: "LeaveEntitlementPeriodId",
                principalSchema: "public",
                principalTable: "TrxLeaveEntitlementPeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxLeaveEntitlement_TrxLeaveBalanceTransaction_EntitlementT~",
                schema: "public",
                table: "TrxLeaveEntitlement",
                column: "EntitlementTransactionId",
                principalSchema: "public",
                principalTable: "TrxLeaveBalanceTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxLeaveEntitlement_TrxLeaveEntitlementPeriod_LeaveEntitlem~",
                schema: "public",
                table: "TrxLeaveEntitlement",
                column: "LeaveEntitlementPeriodId",
                principalSchema: "public",
                principalTable: "TrxLeaveEntitlementPeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpLeaveBalance_TrxLeaveBalanceTransaction_LastTransactionId",
                schema: "public",
                table: "WfpLeaveBalance",
                column: "LastTransactionId",
                principalSchema: "public",
                principalTable: "TrxLeaveBalanceTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpLeaveBalance_TrxLeaveEntitlementPeriod_LeaveEntitlementP~",
                schema: "public",
                table: "WfpLeaveBalance",
                column: "LeaveEntitlementPeriodId",
                principalSchema: "public",
                principalTable: "TrxLeaveEntitlementPeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstLeaveCarryForwardPolicy_MstLeaveType_DestinationLeaveTyp~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstLeavePolicy_MstContractType_ContractTypeId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstLeavePolicy_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstLeavePolicy_MstEmploymentStatus_EmploymentStatusId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstLeavePolicy_MstPosition_PositionId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstLeavePolicy_MstWorkforceType_WorkforceTypeId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstLeavePolicy_MstWorkLocation_WorkLocationId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxLeaveAccrual_TrxLeaveBalanceTransaction_BalanceTransacti~",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxLeaveBalanceTransaction_TrxLeaveBalanceTransaction_Origi~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxLeaveBalanceTransaction_TrxLeaveEntitlementPeriod_LeaveE~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxLeaveEntitlement_TrxLeaveBalanceTransaction_EntitlementT~",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxLeaveEntitlement_TrxLeaveEntitlementPeriod_LeaveEntitlem~",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpLeaveBalance_TrxLeaveBalanceTransaction_LastTransactionId",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpLeaveBalance_TrxLeaveEntitlementPeriod_LeaveEntitlementP~",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropTable(
                name: "TrxLeaveEntitlementPeriod",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_LastTransactionId",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_LeaveEntitlementPeriodId_BalanceStatus_IsLo~",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_LeavePolicyId_LeaveEntitlementPolicyId",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_LeaveTypeId",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_LeaveTypeId_LeaveEntitle~",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_LeaveTypeId_Year_IsActiv~",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_EntitlementTransactionId",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_LeaveBalanceId_EntitlementYear_IsActive~",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_LeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_LeavePolicyId_LeaveEntitlementPolicyId",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveEntitlement_WorkforceProfileId_LeaveTypeId_LeaveEnt~",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveBalanceId_TransactionSequen~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveEntitlementPeriodId_Transac~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_OriginalTransactionId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_PostingBatchType_PostingBatchId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_SourceType_SourceReferenceId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_WorkforceProfileId_LeaveTypeId_E~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_BalanceTransactionId",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_LeaveBalanceId_LeaveAccrualRunId_IsActive_I~",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_LeaveEntitlementId_AccrualPeriodStartDate_A~",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_WorkforceProfileId_LeaveTypeId_AccrualDate_~",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_ContractTypeId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_DepartmentId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_EffectiveStartDate_EffectiveEndDate",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_EmployeeCategoryId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_EmploymentStatusId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_LeaveTypeId_Priority_IsFallback_IsDefault_Is~",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_LegalEntityId_HospitalSiteId_OrganizationUni~",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_PositionId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_WorkforceTypeId_EmployeeCategoryId_Employmen~",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeavePolicy_WorkLocationId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveEntitlementPolicy_EffectiveStartDate_EffectiveEndDa~",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveCarryForwardPolicy_DestinationLeaveTypeId_ExpiryMet~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstLeaveCarryForwardPolicy_EffectiveStartDate_EffectiveEndD~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropColumn(
                name: "BalanceStatus",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropColumn(
                name: "BalanceVersion",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropColumn(
                name: "CarryForwardExpiryDate",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropColumn(
                name: "LastReconciledAt",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropColumn(
                name: "LastTransactionId",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropColumn(
                name: "LastTransactionSequence",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlementPeriodId",
                schema: "public",
                table: "WfpLeaveBalance");

            migrationBuilder.DropColumn(
                name: "AvailableFromDate",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropColumn(
                name: "CalculationVersion",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropColumn(
                name: "EntitlementTransactionId",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropColumn(
                name: "GrantDate",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveEntitlement");

            migrationBuilder.DropColumn(
                name: "AccruedDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "AdjustmentDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "AvailableDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "CarryForwardDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "CompensatoryDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "EncashmentDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "EntitlementDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "ExpiredDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "OpeningBalanceDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "OriginalTransactionId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "PendingDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "PostingBatchId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "PostingBatchType",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "RecalledDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "ReservedDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "TransactionSequence",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "UsedDelta",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "AccrualSequence",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropColumn(
                name: "BalanceTransactionId",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropColumn(
                name: "LeaveAccrualRunId",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropColumn(
                name: "ScheduledAccrualDate",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropColumn(
                name: "ContractTypeId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "DayCalculationMethod",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "DeductionTiming",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "EmploymentStatusId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "IsFallback",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "NegativeBalanceLimitDays",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "PositionId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "ReservationTiming",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "WorkLocationId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "WorkforceTypeId",
                schema: "public",
                table: "MstLeavePolicy");

            migrationBuilder.DropColumn(
                name: "AccrualDayOfMonth",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropColumn(
                name: "AccrualMaximumPerPeriodDays",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropColumn(
                name: "AccrualTiming",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropColumn(
                name: "FinalAccrualRule",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropColumn(
                name: "FirstAccrualRule",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropColumn(
                name: "GrantTiming",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropColumn(
                name: "PeriodBasis",
                schema: "public",
                table: "MstLeaveEntitlementPolicy");

            migrationBuilder.DropColumn(
                name: "CarryForwardExecutionTiming",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropColumn(
                name: "DestinationLeaveTypeId",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropColumn(
                name: "MaximumCarryForwardPeriods",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropColumn(
                name: "MinimumCarryForwardDays",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.DropColumn(
                name: "RoundingMethod",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy");

            migrationBuilder.AlterColumn<decimal>(
                name: "UsedDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "ReservedDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "RemainingDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "RecalledDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "PendingDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBalanceDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<bool>(
                name: "IsLocked",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExpiredDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "EntitlementDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "EncashmentDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompensatoryDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CarriedForwardDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AvailableDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdjustmentDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AccruedDays",
                schema: "public",
                table: "WfpLeaveBalance",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "ServiceMonthsAtGrant",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProratedEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<bool>(
                name: "IsProrated",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "CarryForwardEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalEntitlementDays",
                schema: "public",
                table: "TrxLeaveEntitlement",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionType",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "ManualAdjustment");

            migrationBuilder.AlterColumn<decimal>(
                name: "TransactionDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDateTime",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousReservedDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousOpeningBalanceDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousAvailableDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewUsedDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewReservedDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewAvailableDays",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Direction",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "Credit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsProrated",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "BalanceBeforeAccrual",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "BalanceAfterAccrual",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AccrualAmountDays",
                schema: "public",
                table: "TrxLeaveAccrual",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumBalanceDays",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualEntitlementDays",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "AccrualAmountDays",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "PayoutMaximumDays",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumCarryForwardDays",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_LeavePolicyId",
                schema: "public",
                table: "WfpLeaveBalance",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_LeaveTypeId_Year_IsActive_IsDelete",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "LeaveTypeId", "Year", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_LeaveTypeId_Year",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "Year" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_PeriodStartDate_PeriodEn~",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "WorkforceProfileId", "PeriodStartDate", "PeriodEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_EntitlementStatus_ExpiryDate_IsActive_I~",
                schema: "public",
                table: "TrxLeaveEntitlement",
                columns: new[] { "EntitlementStatus", "ExpiryDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_LeaveBalanceId",
                schema: "public",
                table: "TrxLeaveEntitlement",
                column: "LeaveBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_LeavePolicyId",
                schema: "public",
                table: "TrxLeaveEntitlement",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveEntitlement_WorkforceProfileId_LeaveTypeId_Entitlem~",
                schema: "public",
                table: "TrxLeaveEntitlement",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "EntitlementYear", "IsDelete" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveBalanceId_TransactionDateTi~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                columns: new[] { "LeaveBalanceId", "TransactionDateTime", "TransactionStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_SourceType_SourceReferenceId_IsD~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                columns: new[] { "SourceType", "SourceReferenceId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_WorkforceProfileId_LeaveTypeId_T~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "TransactionDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_AccrualStatus_AccrualDate_IsActive_IsDelete",
                schema: "public",
                table: "TrxLeaveAccrual",
                columns: new[] { "AccrualStatus", "AccrualDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_LeaveBalanceId",
                schema: "public",
                table: "TrxLeaveAccrual",
                column: "LeaveBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_LeaveEntitlementId",
                schema: "public",
                table: "TrxLeaveAccrual",
                column: "LeaveEntitlementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_WorkforceProfileId_LeaveTypeId_AccrualPerio~",
                schema: "public",
                table: "TrxLeaveAccrual",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "AccrualPeriodStartDate", "AccrualPeriodEndDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveType_LeaveCategory",
                schema: "public",
                table: "MstLeaveType",
                column: "LeaveCategory");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_EffectiveStartDate_EffectiveEndDate_IsActive~",
                schema: "public",
                table: "MstLeavePolicy",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_EmployeeCategoryId_EmploymentTypeId",
                schema: "public",
                table: "MstLeavePolicy",
                columns: new[] { "EmployeeCategoryId", "EmploymentTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_LeaveTypeId",
                schema: "public",
                table: "MstLeavePolicy",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_LeaveTypeId_IsDefault_IsActive_IsDelete",
                schema: "public",
                table: "MstLeavePolicy",
                columns: new[] { "LeaveTypeId", "IsDefault", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeavePolicy_LegalEntityId_HospitalSiteId_OrganizationUni~",
                schema: "public",
                table: "MstLeavePolicy",
                columns: new[] { "LegalEntityId", "HospitalSiteId", "OrganizationUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveEntitlementPolicy_EffectiveStartDate_EffectiveEndDa~",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveEntitlementPolicy_LeavePolicyId",
                schema: "public",
                table: "MstLeaveEntitlementPolicy",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveCarryForwardPolicy_EffectiveStartDate_EffectiveEndD~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveCarryForwardPolicy_ExpiryMethod_IsPayoutAllowed_Exc~",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                columns: new[] { "ExpiryMethod", "IsPayoutAllowed", "ExcessBalanceAction" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveCarryForwardPolicy_LeaveEntitlementPolicyId",
                schema: "public",
                table: "MstLeaveCarryForwardPolicy",
                column: "LeaveEntitlementPolicyId");
        }
    }
}
