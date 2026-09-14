using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyGuarantorAndItemPayerFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilInvoiceItemBillingDisposition",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Disposition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "INCLUDED"),
                    DecisionSource = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "AUTO"),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BilInvoiceItemBillingDisposition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilInvoiceItemBillingDisposition_BilInvoiceItem_InvoiceItem~",
                        column: x => x.InvoiceItemId,
                        principalSchema: "public",
                        principalTable: "BilInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilInvoiceItemPayerAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterGuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayerKind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "CASH"),
                    AssignmentSource = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "AUTO"),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BilInvoiceItemPayerAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilInvoiceItemPayerAssignment_BilInvoiceItem_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalSchema: "public",
                        principalTable: "BilInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilInvoiceItemPayerAssignment_RegPatientEncounterGuarantor_~",
                        column: x => x.EncounterGuarantorId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounterGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilInvoicePayerChangeCommand",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousPayerKind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NewPayerKind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PreviousPayerNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    NewPayerNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PreviousCalculationVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewCalculationVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResetAssignmentCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CausationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_BilInvoicePayerChangeCommand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilInvoicePayerChangeCommand_BilCalculationVersion_NewCalcu~",
                        column: x => x.NewCalculationVersionId,
                        principalSchema: "public",
                        principalTable: "BilCalculationVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilInvoicePayerChangeCommand_BilCalculationVersion_Previous~",
                        column: x => x.PreviousCalculationVersionId,
                        principalSchema: "public",
                        principalTable: "BilCalculationVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilInvoicePayerChangeCommand_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilPaymentReminder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageTemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProviderReferenceMasked = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilPaymentReminder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilPaymentReminder_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstCompanyGuarantorCoverageRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RuleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Tariff"),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    BenefitPlanCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BenefitPlanName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EmployeeGrade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CoverageStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Covered"),
                    CoveragePercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 100m),
                    MaxCoverageAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CoPaymentPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CoPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MaxQuantityPerVisit = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    MaxQuantityPerMonth = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    MaxAmountPerVisit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxAmountPerMonth = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ApprovalInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BillingInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstCompanyGuarantorCoverageRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorCoverageRule_MstCompanyGuarantor_Company~",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorCoverageRule_MstDrugCategory_DrugCategor~",
                        column: x => x.DrugCategoryId,
                        principalSchema: "public",
                        principalTable: "MstDrugCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorCoverageRule_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorCoverageRule_MstPatientClass_PatientClas~",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorCoverageRule_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorCoverageRule_MstTariffCategory_TariffCat~",
                        column: x => x.TariffCategoryId,
                        principalSchema: "public",
                        principalTable: "MstTariffCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorCoverageRule_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstCompanyGuarantorReimbursementRoute",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "SELF"),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstCompanyGuarantorReimbursementRoute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorReimbursementRoute_MstCompanyGuarantor_C~",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstCompanyGuarantorReimbursementRoute_MstInsuranceProvider_~",
                        column: x => x.InsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItemBillingDisposition_ActiveItem",
                schema: "public",
                table: "BilInvoiceItemBillingDisposition",
                column: "InvoiceItemId",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItemBillingDisposition_Disposition",
                schema: "public",
                table: "BilInvoiceItemBillingDisposition",
                column: "Disposition");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItemBillingDisposition_IsActive",
                schema: "public",
                table: "BilInvoiceItemBillingDisposition",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItemPayerAssignment_ActiveItem",
                schema: "public",
                table: "BilInvoiceItemPayerAssignment",
                column: "InvoiceItemId",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItemPayerAssignment_EncounterGuarantorId",
                schema: "public",
                table: "BilInvoiceItemPayerAssignment",
                column: "EncounterGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItemPayerAssignment_IsActive",
                schema: "public",
                table: "BilInvoiceItemPayerAssignment",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItemPayerAssignment_PayerKind",
                schema: "public",
                table: "BilInvoiceItemPayerAssignment",
                column: "PayerKind");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoicePayerChangeCommand_CorrelationId",
                schema: "public",
                table: "BilInvoicePayerChangeCommand",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoicePayerChangeCommand_EncounterId",
                schema: "public",
                table: "BilInvoicePayerChangeCommand",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoicePayerChangeCommand_IdempotencyKey",
                schema: "public",
                table: "BilInvoicePayerChangeCommand",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoicePayerChangeCommand_InvoiceId",
                schema: "public",
                table: "BilInvoicePayerChangeCommand",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoicePayerChangeCommand_NewCalculationVersionId",
                schema: "public",
                table: "BilInvoicePayerChangeCommand",
                column: "NewCalculationVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoicePayerChangeCommand_PreviousCalculationVersionId",
                schema: "public",
                table: "BilInvoicePayerChangeCommand",
                column: "PreviousCalculationVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_BilPaymentReminder_InvoiceId",
                schema: "public",
                table: "BilPaymentReminder",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BilPaymentReminder_PatientId",
                schema: "public",
                table: "BilPaymentReminder",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_BenefitPlanCode",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "BenefitPlanCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_Company_RuleCode",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                columns: new[] { "CompanyGuarantorId", "RuleCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_CompanyGuarantorId",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_DrugCategoryId",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "DrugCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_DrugId",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_EffectiveEndDate",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "EffectiveEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_EffectiveStartDate",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "EffectiveStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_EmployeeGrade",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "EmployeeGrade");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_IsActive",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_ItemType",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "ItemType");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_PatientClassId",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_Priority",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_ProcedureId",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_TariffCategoryId",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "TariffCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorCoverageRule_TariffId",
                schema: "public",
                table: "MstCompanyGuarantorCoverageRule",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorReimbursementRoute_Default",
                schema: "public",
                table: "MstCompanyGuarantorReimbursementRoute",
                column: "CompanyGuarantorId",
                unique: true,
                filter: "\"IsDefault\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorReimbursementRoute_InsuranceProviderId",
                schema: "public",
                table: "MstCompanyGuarantorReimbursementRoute",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantorReimbursementRoute_RouteType",
                schema: "public",
                table: "MstCompanyGuarantorReimbursementRoute",
                column: "RouteType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilInvoiceItemBillingDisposition",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilInvoiceItemPayerAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilInvoicePayerChangeCommand",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilPaymentReminder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCompanyGuarantorCoverageRule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCompanyGuarantorReimbursementRoute",
                schema: "public");
        }
    }
}
