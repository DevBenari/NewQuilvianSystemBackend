using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeNurseStationCluster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstNurseStationCluster",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClusterCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClusterName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LocationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsAvailableForRegistrationQueue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForScreening = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForDisplay = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstNurseStationCluster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstNurseStationCluster_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstNurseStationClusterClinic",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseStationClusterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstNurseStationClusterClinic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstNurseStationClusterClinic_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstNurseStationClusterClinic_MstNurseStationCluster_NurseSt~",
                        column: x => x.NurseStationClusterId,
                        principalSchema: "public",
                        principalTable: "MstNurseStationCluster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstNurseStationClusterStaff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseStationClusterId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanCallQueue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CanStartScreening = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CanTransferQueue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstNurseStationClusterStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstNurseStationClusterStaff_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstNurseStationClusterStaff_MstNurseStationCluster_NurseSta~",
                        column: x => x.NurseStationClusterId,
                        principalSchema: "public",
                        principalTable: "MstNurseStationCluster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstNurseStationClusterStaff_MstWorkforceProfile_WorkforcePr~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstQueueDisplayDevice",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseStationClusterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DisplayDeviceType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LayoutType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LocationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MacAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PairingCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EnableVoiceCalling = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ShowPatientName = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ShowDoctorName = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ShowClinicName = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RefreshIntervalSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    LastOnlineDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstQueueDisplayDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstQueueDisplayDevice_MstNurseStationCluster_NurseStationCl~",
                        column: x => x.NurseStationClusterId,
                        principalSchema: "public",
                        principalTable: "MstNurseStationCluster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstQueueDisplayDevice_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationCluster_ClusterCode",
                schema: "public",
                table: "MstNurseStationCluster",
                column: "ClusterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationCluster_ClusterName",
                schema: "public",
                table: "MstNurseStationCluster",
                column: "ClusterName");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationCluster_IsAvailableForRegistrationQueue_IsAv~",
                schema: "public",
                table: "MstNurseStationCluster",
                columns: new[] { "IsAvailableForRegistrationQueue", "IsAvailableForScreening", "IsAvailableForDisplay", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationCluster_ServiceUnitId",
                schema: "public",
                table: "MstNurseStationCluster",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationCluster_ServiceUnitId_ClusterName",
                schema: "public",
                table: "MstNurseStationCluster",
                columns: new[] { "ServiceUnitId", "ClusterName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationCluster_ServiceUnitId_IsActive_IsDelete",
                schema: "public",
                table: "MstNurseStationCluster",
                columns: new[] { "ServiceUnitId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterClinic_ClinicId",
                schema: "public",
                table: "MstNurseStationClusterClinic",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterClinic_ClinicId_IsActive_IsDelete",
                schema: "public",
                table: "MstNurseStationClusterClinic",
                columns: new[] { "ClinicId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterClinic_NurseStationClusterId",
                schema: "public",
                table: "MstNurseStationClusterClinic",
                column: "NurseStationClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterClinic_NurseStationClusterId_ClinicId~",
                schema: "public",
                table: "MstNurseStationClusterClinic",
                columns: new[] { "NurseStationClusterId", "ClinicId", "IsDelete" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterClinic_NurseStationClusterId_IsActive~",
                schema: "public",
                table: "MstNurseStationClusterClinic",
                columns: new[] { "NurseStationClusterId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaff_EmployeeId",
                schema: "public",
                table: "MstNurseStationClusterStaff",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaff_EmployeeId_IsActive_IsDelete",
                schema: "public",
                table: "MstNurseStationClusterStaff",
                columns: new[] { "EmployeeId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaff_NurseStationClusterId",
                schema: "public",
                table: "MstNurseStationClusterStaff",
                column: "NurseStationClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaff_NurseStationClusterId_EmployeeI~",
                schema: "public",
                table: "MstNurseStationClusterStaff",
                columns: new[] { "NurseStationClusterId", "EmployeeId", "IsDelete" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaff_NurseStationClusterId_IsActive_~",
                schema: "public",
                table: "MstNurseStationClusterStaff",
                columns: new[] { "NurseStationClusterId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstNurseStationClusterStaff_WorkforceProfileId",
                schema: "public",
                table: "MstNurseStationClusterStaff",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueDisplayDevice_DeviceToken",
                schema: "public",
                table: "MstQueueDisplayDevice",
                column: "DeviceToken");

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueDisplayDevice_DisplayCode",
                schema: "public",
                table: "MstQueueDisplayDevice",
                column: "DisplayCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueDisplayDevice_IsActive_IsDelete",
                schema: "public",
                table: "MstQueueDisplayDevice",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueDisplayDevice_NurseStationClusterId",
                schema: "public",
                table: "MstQueueDisplayDevice",
                column: "NurseStationClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueDisplayDevice_NurseStationClusterId_DisplayDeviceTy~",
                schema: "public",
                table: "MstQueueDisplayDevice",
                columns: new[] { "NurseStationClusterId", "DisplayDeviceType", "LayoutType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueDisplayDevice_PairingCode",
                schema: "public",
                table: "MstQueueDisplayDevice",
                column: "PairingCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueDisplayDevice_ServiceUnitId",
                schema: "public",
                table: "MstQueueDisplayDevice",
                column: "ServiceUnitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstNurseStationClusterClinic",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstNurseStationClusterStaff",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstQueueDisplayDevice",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstNurseStationCluster",
                schema: "public");
        }
    }
}
