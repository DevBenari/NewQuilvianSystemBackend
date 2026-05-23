using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddPerformanceReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpPerformanceReview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewPeriod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "date", nullable: false),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalScore = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    FinalRating = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReviewStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StrengthNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImprovementNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecommendationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsFinalized = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_WfpPerformanceReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpPerformanceReview_AspNetUsers_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpPerformanceReview_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpPerformanceReviewDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriteriaCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CriteriaName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Score = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    Weight = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    WeightedScore = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WfpPerformanceReviewDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpPerformanceReviewDetail_WfpPerformanceReview_Performance~",
                        column: x => x.PerformanceReviewId,
                        principalSchema: "public",
                        principalTable: "WfpPerformanceReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_ReviewDate",
                schema: "public",
                table: "WfpPerformanceReview",
                column: "ReviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_ReviewerUserId",
                schema: "public",
                table: "WfpPerformanceReview",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_ReviewerUserId_ReviewDate_IsDelete",
                schema: "public",
                table: "WfpPerformanceReview",
                columns: new[] { "ReviewerUserId", "ReviewDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_ReviewPeriod",
                schema: "public",
                table: "WfpPerformanceReview",
                column: "ReviewPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_ReviewStatus",
                schema: "public",
                table: "WfpPerformanceReview",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_ReviewType",
                schema: "public",
                table: "WfpPerformanceReview",
                column: "ReviewType");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_WorkforceProfileId",
                schema: "public",
                table: "WfpPerformanceReview",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_WorkforceProfileId_ReviewPeriod_Review~",
                schema: "public",
                table: "WfpPerformanceReview",
                columns: new[] { "WorkforceProfileId", "ReviewPeriod", "ReviewType", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReview_WorkforceProfileId_ReviewStatus_IsFina~",
                schema: "public",
                table: "WfpPerformanceReview",
                columns: new[] { "WorkforceProfileId", "ReviewStatus", "IsFinalized", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReviewDetail_CriteriaCode",
                schema: "public",
                table: "WfpPerformanceReviewDetail",
                column: "CriteriaCode");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReviewDetail_PerformanceReviewId",
                schema: "public",
                table: "WfpPerformanceReviewDetail",
                column: "PerformanceReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReviewDetail_PerformanceReviewId_CriteriaCode",
                schema: "public",
                table: "WfpPerformanceReviewDetail",
                columns: new[] { "PerformanceReviewId", "CriteriaCode" },
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpPerformanceReviewDetail_PerformanceReviewId_IsDelete",
                schema: "public",
                table: "WfpPerformanceReviewDetail",
                columns: new[] { "PerformanceReviewId", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpPerformanceReviewDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpPerformanceReview",
                schema: "public");
        }
    }
}
