using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnQueueDisplay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastErrorMessage",
                schema: "public",
                table: "MstQueueDisplayDevice",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOfflineDateTime",
                schema: "public",
                table: "MstQueueDisplayDevice",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastErrorMessage",
                schema: "public",
                table: "MstQueueDisplayDevice");

            migrationBuilder.DropColumn(
                name: "LastOfflineDateTime",
                schema: "public",
                table: "MstQueueDisplayDevice");
        }
    }
}
