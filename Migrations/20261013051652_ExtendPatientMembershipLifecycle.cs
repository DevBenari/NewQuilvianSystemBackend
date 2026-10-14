using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class ExtendPatientMembershipLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MembershipStatus",
                schema: "public",
                table: "MstPatientMembership",
                type: "integer",
                nullable: false,
                defaultValue: 7,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivatedAt",
                schema: "public",
                table: "MstPatientMembership",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancellationCategory",
                schema: "public",
                table: "MstPatientMembership",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                schema: "public",
                table: "MstPatientMembership",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmountSnapshot",
                schema: "public",
                table: "MstPatientMembership",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "FeeChargeReferenceId",
                schema: "public",
                table: "MstPatientMembership",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaymentDueAt",
                schema: "public",
                table: "MstPatientMembership",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStartDate",
                schema: "public",
                table: "MstPatientMembership",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousPatientMembershipId",
                schema: "public",
                table: "MstPatientMembership",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegistrationType",
                schema: "public",
                table: "MstPatientMembership",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                schema: "public",
                table: "MstPatientMembership",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualFeeAmount",
                schema: "public",
                table: "MstMembershipTier",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PatMembershipStatusHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldNotes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    NewNotes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_PatMembershipStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatMembershipStatusHistory_MstPatientMembership_PatientMemb~",
                        column: x => x.PatientMembershipId,
                        principalSchema: "public",
                        principalTable: "MstPatientMembership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_MembershipStatus_ExpiredDate",
                schema: "public",
                table: "MstPatientMembership",
                columns: new[] { "MembershipStatus", "ExpiredDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_MembershipStatus_PaymentDueAt",
                schema: "public",
                table: "MstPatientMembership",
                columns: new[] { "MembershipStatus", "PaymentDueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_MembershipStatus_PeriodStartDate",
                schema: "public",
                table: "MstPatientMembership",
                columns: new[] { "MembershipStatus", "PeriodStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_PreviousPatientMembershipId",
                schema: "public",
                table: "MstPatientMembership",
                column: "PreviousPatientMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_PatMembershipStatusHistory_PatientMembershipId_OccurredAt",
                schema: "public",
                table: "PatMembershipStatusHistory",
                columns: new[] { "PatientMembershipId", "OccurredAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_MstPatientMembership_MstPatientMembership_PreviousPatientMe~",
                schema: "public",
                table: "MstPatientMembership",
                column: "PreviousPatientMembershipId",
                principalSchema: "public",
                principalTable: "MstPatientMembership",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstPatientMembership_MstPatientMembership_PreviousPatientMe~",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropTable(
                name: "PatMembershipStatusHistory",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientMembership_MembershipStatus_ExpiredDate",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientMembership_MembershipStatus_PaymentDueAt",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientMembership_MembershipStatus_PeriodStartDate",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientMembership_PreviousPatientMembershipId",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "CancellationCategory",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "FeeAmountSnapshot",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "FeeChargeReferenceId",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "PaymentDueAt",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "PeriodStartDate",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "PreviousPatientMembershipId",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "RegistrationType",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropColumn(
                name: "AnnualFeeAmount",
                schema: "public",
                table: "MstMembershipTier");

            migrationBuilder.AlterColumn<int>(
                name: "MembershipStatus",
                schema: "public",
                table: "MstPatientMembership",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 7);
        }
    }
}
