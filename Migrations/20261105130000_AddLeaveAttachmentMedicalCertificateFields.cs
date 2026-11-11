using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261105130000_AddLeaveAttachmentMedicalCertificateFields")]
    public partial class AddLeaveAttachmentMedicalCertificateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'TrxLeaveRequestAttachment' AND column_name = 'IsMedicalCertificate'
    ) THEN
        ALTER TABLE public.""TrxLeaveRequestAttachment"" 
            ADD COLUMN ""IsMedicalCertificate"" boolean NOT NULL DEFAULT false;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'TrxLeaveRequestAttachment' AND column_name = 'IsConfidentialMedical'
    ) THEN
        ALTER TABLE public.""TrxLeaveRequestAttachment"" 
            ADD COLUMN ""IsConfidentialMedical"" boolean NOT NULL DEFAULT true;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'TrxLeaveRequestAttachment' AND column_name = 'DoctorName'
    ) THEN
        ALTER TABLE public.""TrxLeaveRequestAttachment"" 
            ADD COLUMN ""DoctorName"" character varying(200) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'TrxLeaveRequestAttachment' AND column_name = 'DoctorSipNumber'
    ) THEN
        ALTER TABLE public.""TrxLeaveRequestAttachment"" 
            ADD COLUMN ""DoctorSipNumber"" character varying(100) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'TrxLeaveRequestAttachment' AND column_name = 'FaskesName'
    ) THEN
        ALTER TABLE public.""TrxLeaveRequestAttachment"" 
            ADD COLUMN ""FaskesName"" character varying(255) NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' AND table_name = 'TrxLeaveRequestAttachment' AND column_name = 'MedicalDiagnosisIcd10'
    ) THEN
        ALTER TABLE public.""TrxLeaveRequestAttachment"" 
            ADD COLUMN ""MedicalDiagnosisIcd10"" character varying(100) NULL;
    END IF;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMedicalCertificate",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "IsConfidentialMedical",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "DoctorSipNumber",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "FaskesName",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "MedicalDiagnosisIcd10",
                schema: "public",
                table: "TrxLeaveRequestAttachment");
        }
    }
}
