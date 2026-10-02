using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-102 dan BE-RWI-103 / migration R6. Lima tabel sliding scale milik
    /// <c>PharmacyManagement</c> (<c>RWI-DEC-147</c>) — kamus data 0.5 bagian 13.4 s.d. 13.8: template,
    /// versi template, rentang, order per pasien, dan versi order.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tabel lahir kosong. <b>Tanpa satu pun versi <c>Approved</c>, sliding scale tidak dapat dipesan</b>
    /// (arsitektur 0.5 bagian 11.9), dan pengesahan menunggu nama pengesah isi protokol
    /// <c>RWI-OQ-097</c>.
    /// </para>
    /// <para>
    /// Mundur menghapus kelima tabel, tetapi <b>ditolak</b> bila sudah ada order sliding scale: menghapusnya
    /// berarti menghapus riwayat dosis insulin pasien.
    /// </para>
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260916006000_AddSlidingScale")]
    public partial class AddSlidingScale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhmSlidingScaleTemplate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_PhmSlidingScaleTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhmSlidingScaleTemplateVersion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    VersionStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    GlucoseUnit = table.Column<int>(type: "integer", nullable: false),
                    DefinitionHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PhmSlidingScaleTemplateVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleTemplateVersion_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleTemplateVersion_AspNetUsers_LastModifiedByUs~",
                        column: x => x.LastModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleTemplateVersion_PhmSlidingScaleTemplate_Temp~",
                        column: x => x.TemplateId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhmSlidingScaleOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CheckFrequencyCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StoppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StoppedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StopReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PhmSlidingScaleOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrder_AspNetUsers_StoppedByUserId",
                        column: x => x.StoppedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrder_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrder_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrder_PhmPrescriptionItem_PrescriptionItemId",
                        column: x => x.PrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "PhmPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrder_PhmPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "PhmPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrder_PhmSlidingScaleTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrder_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhmSlidingScaleOrderVersion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    TemplateVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAdjusted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AdjustmentReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrderedByDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_PhmSlidingScaleOrderVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrderVersion_AspNetUsers_OrderedByUserId",
                        column: x => x.OrderedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrderVersion_MstDoctor_OrderedByDoctorId",
                        column: x => x.OrderedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrderVersion_PhmSlidingScaleOrder_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleOrderVersion_PhmSlidingScaleTemplateVersion_~",
                        column: x => x.TemplateVersionId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleTemplateVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhmSlidingScaleRange",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    LowerBoundInclusive = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    UpperBoundExclusive = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    DoseUnits = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    InstructionText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RequiresPhysicianNotification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PhmSlidingScaleRange", x => x.Id);
                    table.CheckConstraint("CK_PhmSlidingScaleRange_DoseUnits", "\"DoseUnits\" >= 0");
                    table.CheckConstraint("CK_PhmSlidingScaleRange_SingleOwner", "num_nonnulls(\"TemplateVersionId\", \"OrderVersionId\") = 1");
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleRange_PhmSlidingScaleOrderVersion_OrderVersi~",
                        column: x => x.OrderVersionId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleOrderVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleRange_PhmSlidingScaleTemplateVersion_Templat~",
                        column: x => x.TemplateVersionId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleTemplateVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleTemplate_TemplateCode",
                schema: "public",
                table: "PhmSlidingScaleTemplate",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleTemplateVersion_ApprovedByUserId",
                schema: "public",
                table: "PhmSlidingScaleTemplateVersion",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleTemplateVersion_LastModifiedByUserId",
                schema: "public",
                table: "PhmSlidingScaleTemplateVersion",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleTemplateVersion_Template_Approved",
                schema: "public",
                table: "PhmSlidingScaleTemplateVersion",
                column: "TemplateId",
                unique: true,
                filter: "\"VersionStatus\" = 2 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleTemplateVersion_TemplateId_VersionNumber",
                schema: "public",
                table: "PhmSlidingScaleTemplateVersion",
                columns: new[] { "TemplateId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_EncounterId",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_InpEpisodeId",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_OrderNumber",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_OrderStatus",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "OrderStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_PatientId",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_PrescriptionId",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_PrescriptionItemId",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "PrescriptionItemId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_StoppedByUserId",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "StoppedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrder_TemplateId",
                schema: "public",
                table: "PhmSlidingScaleOrder",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrderVersion_OrderedByDoctorId",
                schema: "public",
                table: "PhmSlidingScaleOrderVersion",
                column: "OrderedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrderVersion_OrderedByUserId",
                schema: "public",
                table: "PhmSlidingScaleOrderVersion",
                column: "OrderedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrderVersion_OrderId_VersionNumber",
                schema: "public",
                table: "PhmSlidingScaleOrderVersion",
                columns: new[] { "OrderId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleOrderVersion_TemplateVersionId",
                schema: "public",
                table: "PhmSlidingScaleOrderVersion",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleRange_OrderVersionId_SortOrder",
                schema: "public",
                table: "PhmSlidingScaleRange",
                columns: new[] { "OrderVersionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleRange_TemplateVersionId_SortOrder",
                schema: "public",
                table: "PhmSlidingScaleRange",
                columns: new[] { "TemplateVersionId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_order integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah_order
                    FROM public.""PhmSlidingScaleOrder"";

                    IF jumlah_order > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-103: rollback ditolak. % order sliding scale sudah tercatat; menghapus tabel menghapus riwayat dosis insulin pasien.',
                            jumlah_order;
                    END IF;
                END $$;");

            migrationBuilder.DropTable(
                name: "PhmSlidingScaleRange",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmSlidingScaleOrderVersion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmSlidingScaleOrder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmSlidingScaleTemplateVersion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmSlidingScaleTemplate",
                schema: "public");
        }
    }
}
