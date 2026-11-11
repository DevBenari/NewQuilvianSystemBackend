using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261109170000_AddEmployeeRelationAndExitClearance")]
    public partial class AddEmployeeRelationAndExitClearance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. trx_disciplinary_incident_reports
            migrationBuilder.CreateTable(
                name: "trx_disciplinary_incident_reports",
                schema: "public",
                columns: table => new
                {
                    incident_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    incident_location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    just_culture_classification = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "HumanError"),
                    description = table.Column<string>(type: "text", nullable: false),
                    reporter_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reporter_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kmkp_audit_reference_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Reported"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_disciplinary_incident_reports", x => x.incident_report_id);
                });

            // 2. trx_disciplinary_investigation_records
            migrationBuilder.CreateTable(
                name: "trx_disciplinary_investigation_records",
                schema: "public",
                columns: table => new
                {
                    investigation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bap_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    hearing_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    investigator_names = table.Column<string>(type: "text", nullable: false),
                    witness_statements = table.Column<string>(type: "text", nullable: true),
                    evidence_document_urls = table.Column<string>(type: "text", nullable: true),
                    employee_defense_statement = table.Column<string>(type: "text", nullable: true),
                    investigation_findings = table.Column<string>(type: "text", nullable: false),
                    recommended_action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Draft"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_disciplinary_investigation_records", x => x.investigation_id);
                    table.ForeignKey(
                        name: "FK_trx_disciplinary_investigation_records_trx_disciplinary_incident_reports",
                        column: x => x.incident_report_id,
                        principalSchema: "public",
                        principalTable: "trx_disciplinary_incident_reports",
                        principalColumn: "incident_report_id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3. trx_disciplinary_sanctions
            migrationBuilder.CreateTable(
                name: "trx_disciplinary_sanctions",
                schema: "public",
                columns: table => new
                {
                    sanction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    investigation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sanction_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sanction_level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    issued_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    violation_description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Active"),
                    is_appealed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    appeal_statement = table.Column<string>(type: "text", nullable: true),
                    appeal_evidence_urls = table.Column<string>(type: "text", nullable: true),
                    appeal_resolution = table.Column<string>(type: "text", nullable: true),
                    appealed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_disciplinary_sanctions", x => x.sanction_id);
                    table.ForeignKey(
                        name: "FK_trx_disciplinary_sanctions_trx_disciplinary_investigation_records",
                        column: x => x.investigation_id,
                        principalSchema: "public",
                        principalTable: "trx_disciplinary_investigation_records",
                        principalColumn: "investigation_id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 4. trx_disciplinary_suspensions
            migrationBuilder.CreateTable(
                name: "trx_disciplinary_suspensions",
                schema: "public",
                columns: table => new
                {
                    suspension_id = table.Column<Guid>(type: "uuid", nullable: false),
                    investigation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    suspension_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actual_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    wage_payment_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PaidFull"),
                    is_system_access_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_physical_access_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Active"),
                    reinstatement_notes = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_disciplinary_suspensions", x => x.suspension_id);
                    table.ForeignKey(
                        name: "FK_trx_disciplinary_suspensions_trx_disciplinary_investigation_records",
                        column: x => x.investigation_id,
                        principalSchema: "public",
                        principalTable: "trx_disciplinary_investigation_records",
                        principalColumn: "investigation_id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 5. trx_bipartite_negotiation_forums
            migrationBuilder.CreateTable(
                name: "trx_bipartite_negotiation_forums",
                schema: "public",
                columns: table => new
                {
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    register_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    meeting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dispute_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    management_representatives = table.Column<string>(type: "text", nullable: false),
                    union_representatives = table.Column<string>(type: "text", nullable: false),
                    agenda_description = table.Column<string>(type: "text", nullable: false),
                    minutes_of_meeting = table.Column<string>(type: "text", nullable: false),
                    joint_agreement_document_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    registration_disnaker_number = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Scheduled"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_bipartite_negotiation_forums", x => x.forum_id);
                });

            // 6. trx_severance_calculations
            migrationBuilder.CreateTable(
                name: "trx_severance_calculations",
                schema: "public",
                columns: table => new
                {
                    calculation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    separation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_monthly_wage = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    service_years = table.Column<int>(type: "integer", nullable: false),
                    service_months = table.Column<int>(type: "integer", nullable: false),
                    severance_pay_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    service_award_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    rights_compensation_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    separation_allowance_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    total_severance_gross = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    tax_deduction = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    total_severance_net = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Draft"),
                    is_approved_by_finance_director = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    finance_approval_notes = table.Column<string>(type: "text", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_severance_calculations", x => x.calculation_id);
                });

            // 7. trx_exit_clearance_processes
            migrationBuilder.CreateTable(
                name: "trx_exit_clearance_processes",
                schema: "public",
                columns: table => new
                {
                    clearance_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    separation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    initiated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    target_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actual_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    overall_clearance_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "InProgress"),
                    is_final_payroll_unlocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    finalization_notes = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_exit_clearance_processes", x => x.clearance_process_id);
                });

            // 8. trx_exit_clearance_checklists
            migrationBuilder.CreateTable(
                name: "trx_exit_clearance_checklists",
                schema: "public",
                columns: table => new
                {
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clearance_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    item_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_returned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    outstanding_liability_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    approver_notes = table.Column<string>(type: "text", nullable: true),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_exit_clearance_checklists", x => x.checklist_id);
                    table.ForeignKey(
                        name: "FK_trx_exit_clearance_checklists_trx_exit_clearance_processes",
                        column: x => x.clearance_process_id,
                        principalSchema: "public",
                        principalTable: "trx_exit_clearance_processes",
                        principalColumn: "clearance_process_id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indexes
            migrationBuilder.CreateIndex(name: "IX_trx_disciplinary_incident_reports_incident_number", schema: "public", table: "trx_disciplinary_incident_reports", column: "incident_number", unique: true);
            migrationBuilder.CreateIndex(name: "IX_trx_disciplinary_incident_reports_employee_id", schema: "public", table: "trx_disciplinary_incident_reports", column: "employee_id");
            migrationBuilder.CreateIndex(name: "IX_trx_disciplinary_investigation_records_bap_number", schema: "public", table: "trx_disciplinary_investigation_records", column: "bap_number", unique: true);
            migrationBuilder.CreateIndex(name: "IX_trx_disciplinary_sanctions_sanction_number", schema: "public", table: "trx_disciplinary_sanctions", column: "sanction_number", unique: true);
            migrationBuilder.CreateIndex(name: "IX_trx_disciplinary_sanctions_valid_until", schema: "public", table: "trx_disciplinary_sanctions", column: "valid_until");
            migrationBuilder.CreateIndex(name: "IX_trx_disciplinary_suspensions_suspension_number", schema: "public", table: "trx_disciplinary_suspensions", column: "suspension_number", unique: true);
            migrationBuilder.CreateIndex(name: "IX_trx_disciplinary_suspensions_expected_end_date", schema: "public", table: "trx_disciplinary_suspensions", column: "expected_end_date");
            migrationBuilder.CreateIndex(name: "IX_trx_bipartite_negotiation_forums_register_number", schema: "public", table: "trx_bipartite_negotiation_forums", column: "register_number", unique: true);
            migrationBuilder.CreateIndex(name: "IX_trx_severance_calculations_separation_id", schema: "public", table: "trx_severance_calculations", column: "separation_id");
            migrationBuilder.CreateIndex(name: "IX_trx_exit_clearance_processes_separation_id", schema: "public", table: "trx_exit_clearance_processes", column: "separation_id");
            migrationBuilder.CreateIndex(name: "IX_trx_exit_clearance_processes_employee_id", schema: "public", table: "trx_exit_clearance_processes", column: "employee_id");
            migrationBuilder.CreateIndex(name: "IX_trx_exit_clearance_checklists_clearance_process_id", schema: "public", table: "trx_exit_clearance_checklists", column: "clearance_process_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "trx_exit_clearance_checklists", schema: "public");
            migrationBuilder.DropTable(name: "trx_exit_clearance_processes", schema: "public");
            migrationBuilder.DropTable(name: "trx_severance_calculations", schema: "public");
            migrationBuilder.DropTable(name: "trx_bipartite_negotiation_forums", schema: "public");
            migrationBuilder.DropTable(name: "trx_disciplinary_suspensions", schema: "public");
            migrationBuilder.DropTable(name: "trx_disciplinary_sanctions", schema: "public");
            migrationBuilder.DropTable(name: "trx_disciplinary_investigation_records", schema: "public");
            migrationBuilder.DropTable(name: "trx_disciplinary_incident_reports", schema: "public");
        }
    }
}
