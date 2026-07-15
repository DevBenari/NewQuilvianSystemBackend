using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnPrescriptionCompound : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedActiveAmount",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CalculatedActiveUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculatedActiveUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculatedActiveUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CalculationMode",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CalculationNote",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculationStatus",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IngredientRole",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowFractionalSourceSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsQuantitySufficientToFinal",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PricingQuantity",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SourceContentQuantity",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceContentUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceContentUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceContentUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceStrengthMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceStrengthUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceStrengthUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SourceStrengthValue",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetConcentrationUnit",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetValue",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TheoreticalSourceQuantity",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VerifiedSourceQuantity",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CalculationMode",
                schema: "public",
                table: "TrxPrescriptionCompound",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalQuantity",
                schema: "public",
                table: "TrxPrescriptionCompound",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinalQuantityMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompound",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalQuantityUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompound",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalQuantityUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompound",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_CalculatedActiveUnitMeasurement~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "CalculatedActiveUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_SourceContentUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "SourceContentUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_SourceStrengthMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "SourceStrengthMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompoundItem_TargetUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "TargetUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionCompound_FinalQuantityMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompound",
                column: "FinalQuantityMeasurementId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPrescriptionCompound_MstMeasurement_FinalQuantityMeasure~",
                schema: "public",
                table: "TrxPrescriptionCompound",
                column: "FinalQuantityMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_CalculatedActive~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "CalculatedActiveUnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_SourceContentUni~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "SourceContentUnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_SourceStrengthMe~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "SourceStrengthMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_TargetUnitMeasur~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem",
                column: "TargetUnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPrescriptionCompound_MstMeasurement_FinalQuantityMeasure~",
                schema: "public",
                table: "TrxPrescriptionCompound");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_CalculatedActive~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_SourceContentUni~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_SourceStrengthMe~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPrescriptionCompoundItem_MstMeasurement_TargetUnitMeasur~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescriptionCompoundItem_CalculatedActiveUnitMeasurement~",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescriptionCompoundItem_SourceContentUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescriptionCompoundItem_SourceStrengthMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescriptionCompoundItem_TargetUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescriptionCompound_FinalQuantityMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompound");

            migrationBuilder.DropColumn(
                name: "CalculatedActiveAmount",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "CalculatedActiveUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "CalculatedActiveUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "CalculatedActiveUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "CalculationMode",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "CalculationNote",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "CalculationStatus",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "IngredientRole",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "IsAllowFractionalSourceSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "IsQuantitySufficientToFinal",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "PricingQuantity",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "SourceContentQuantity",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "SourceContentUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "SourceContentUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "SourceContentUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "SourceStrengthMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "SourceStrengthUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "SourceStrengthUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "SourceStrengthValue",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "TargetConcentrationUnit",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "TargetUnitMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "TargetUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "TargetUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "TargetValue",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "TheoreticalSourceQuantity",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "VerifiedSourceQuantity",
                schema: "public",
                table: "TrxPrescriptionCompoundItem");

            migrationBuilder.DropColumn(
                name: "CalculationMode",
                schema: "public",
                table: "TrxPrescriptionCompound");

            migrationBuilder.DropColumn(
                name: "FinalQuantity",
                schema: "public",
                table: "TrxPrescriptionCompound");

            migrationBuilder.DropColumn(
                name: "FinalQuantityMeasurementId",
                schema: "public",
                table: "TrxPrescriptionCompound");

            migrationBuilder.DropColumn(
                name: "FinalQuantityUnitNameSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompound");

            migrationBuilder.DropColumn(
                name: "FinalQuantityUnitSymbolSnapshot",
                schema: "public",
                table: "TrxPrescriptionCompound");
        }
    }
}
