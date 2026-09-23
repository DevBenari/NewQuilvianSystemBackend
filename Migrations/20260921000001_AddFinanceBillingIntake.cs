using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceBillingIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinBillingHandoffIntake",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HandoffType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceHandoffId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceHandoffKey = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "NEW"),
                    TargetEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinBillingHandoffIntake", x => x.Id);
                    table.CheckConstraint("CK_FinBillingHandoffIntake_HandoffType", "\"HandoffType\" IN ('AR','AP','COLLECTION','ADJUSTMENT')");
                    table.CheckConstraint("CK_FinBillingHandoffIntake_Status", "\"Status\" IN ('NEW','CONSUMED','ACKNOWLEDGED','ERROR')");
                });

            // Satu fakta Billing hanya boleh diolah satu kali (FIN-DES-008, FIN-DES-009).
            migrationBuilder.CreateIndex(
                name: "IX_FinBillingHandoffIntake_Identity",
                schema: "public",
                table: "FinBillingHandoffIntake",
                columns: new[] { "HandoffType", "SourceHandoffKey" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinBillingHandoffIntake_Status",
                schema: "public",
                table: "FinBillingHandoffIntake",
                columns: new[] { "Status", "HandoffType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinBillingHandoffIntake",
                schema: "public");
        }
    }
}
