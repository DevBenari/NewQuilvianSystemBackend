using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddDoctorScheduleAndDoctorServiceRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstDoctorSchedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScheduleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ScheduleType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    PracticeDay = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PracticeDate = table.Column<DateTime>(type: "date", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time without time zone", nullable: false),
                    IsOvernight = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SessionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PracticeLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RoomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaxPatientQuota = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxAppointmentQuota = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxWalkInQuota = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EstimatedServiceMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    IsAllowWalkIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowKioskRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTelemedicineAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSubstituteSchedule = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SubstituteDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduleStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstDoctorSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDoctorSchedule_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorSchedule_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorSchedule_MstDoctor_SubstituteDoctorId",
                        column: x => x.SubstituteDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorSchedule_MstRoom_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "MstRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorSchedule_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDoctorServiceRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RuleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RuleType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAllowWalkIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowKioskRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowTelemedicine = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedReferral = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPrimaryForClinic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDefaultForClinic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DailyQuotaLimit = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RuleStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstDoctorServiceRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDoctorServiceRule_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorServiceRule_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorServiceRule_MstPatientClass_PatientClassId",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorServiceRule_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorServiceRule_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorServiceRule_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDoctorServiceRule_MstTariffCategory_TariffCategoryId",
                        column: x => x.TariffCategoryId,
                        principalSchema: "public",
                        principalTable: "MstTariffCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_ClinicId",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_DoctorId",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_DoctorId_PracticeDay_ScheduleStatus_IsAct~",
                schema: "public",
                table: "MstDoctorSchedule",
                columns: new[] { "DoctorId", "PracticeDay", "ScheduleStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_DoctorId_ServiceUnitId_ClinicId_Practice~1",
                schema: "public",
                table: "MstDoctorSchedule",
                columns: new[] { "DoctorId", "ServiceUnitId", "ClinicId", "PracticeDay", "StartTime", "EndTime" },
                unique: true,
                filter: "\"PracticeDate\" IS NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_DoctorId_ServiceUnitId_ClinicId_PracticeD~",
                schema: "public",
                table: "MstDoctorSchedule",
                columns: new[] { "DoctorId", "ServiceUnitId", "ClinicId", "PracticeDate", "StartTime", "EndTime" },
                unique: true,
                filter: "\"PracticeDate\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_EffectiveStartDate_EffectiveEndDate_IsAct~",
                schema: "public",
                table: "MstDoctorSchedule",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_IsAllowWalkIn_IsAllowAppointment_IsAllowK~",
                schema: "public",
                table: "MstDoctorSchedule",
                columns: new[] { "IsAllowWalkIn", "IsAllowAppointment", "IsAllowKioskRegistration", "IsTelemedicineAvailable", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_PracticeDate",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "PracticeDate");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_PracticeDay",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "PracticeDay");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_RoomId",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_ScheduleCode",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "ScheduleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_ScheduleName",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "ScheduleName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_ScheduleStatus",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "ScheduleStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_ScheduleType",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "ScheduleType");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_ServiceUnitId",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_ServiceUnitId_ClinicId_PracticeDate_Sched~",
                schema: "public",
                table: "MstDoctorSchedule",
                columns: new[] { "ServiceUnitId", "ClinicId", "PracticeDate", "ScheduleStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_ServiceUnitId_ClinicId_PracticeDay_Schedu~",
                schema: "public",
                table: "MstDoctorSchedule",
                columns: new[] { "ServiceUnitId", "ClinicId", "PracticeDay", "ScheduleStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorSchedule_SubstituteDoctorId",
                schema: "public",
                table: "MstDoctorSchedule",
                column: "SubstituteDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_ClinicId",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_DoctorId",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_DoctorId_ServiceUnitId_ClinicId_RuleTy~",
                schema: "public",
                table: "MstDoctorServiceRule",
                columns: new[] { "DoctorId", "ServiceUnitId", "ClinicId", "RuleType", "RuleStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_DoctorId_ServiceUnitId_ClinicId_Tariff~",
                schema: "public",
                table: "MstDoctorServiceRule",
                columns: new[] { "DoctorId", "ServiceUnitId", "ClinicId", "TariffCategoryId", "TariffId", "ProcedureId", "PatientClassId" },
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_DoctorId_TariffCategoryId_TariffId_Pat~",
                schema: "public",
                table: "MstDoctorServiceRule",
                columns: new[] { "DoctorId", "TariffCategoryId", "TariffId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_EffectiveStartDate_EffectiveEndDate_Is~",
                schema: "public",
                table: "MstDoctorServiceRule",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_IsAllowWalkIn_IsAllowAppointment_IsAll~",
                schema: "public",
                table: "MstDoctorServiceRule",
                columns: new[] { "IsAllowWalkIn", "IsAllowAppointment", "IsAllowKioskRegistration", "IsAllowTelemedicine", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_IsNeedReferral_IsNeedApproval_RuleStat~",
                schema: "public",
                table: "MstDoctorServiceRule",
                columns: new[] { "IsNeedReferral", "IsNeedApproval", "RuleStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_IsPrimaryForClinic_IsDefaultForClinic_~",
                schema: "public",
                table: "MstDoctorServiceRule",
                columns: new[] { "IsPrimaryForClinic", "IsDefaultForClinic", "PriorityLevel", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_PatientClassId",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_ProcedureId",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_RuleCode",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "RuleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_RuleName",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "RuleName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_RuleStatus",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "RuleStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_RuleType",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "RuleType");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_ServiceUnitId",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_ServiceUnitId_ClinicId_TariffCategoryI~",
                schema: "public",
                table: "MstDoctorServiceRule",
                columns: new[] { "ServiceUnitId", "ClinicId", "TariffCategoryId", "TariffId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_TariffCategoryId",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "TariffCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDoctorServiceRule_TariffId",
                schema: "public",
                table: "MstDoctorServiceRule",
                column: "TariffId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDoctorSchedule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDoctorServiceRule",
                schema: "public");
        }
    }
}
