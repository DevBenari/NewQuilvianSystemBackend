using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// <c>BE-HRD-382</c> (20 November 2026): Surat Tugas multi-pegawai — tabel <c>HrdAssignmentLetter</c> dan peserta
    /// <c>HrdAssignmentLetterParticipant</c> (keputusan pemilik <c>ADM-Q-03</c> = A, satu surat bertabel peserta).
    /// Ditulis manual dan idempoten (pola <c>20261118100000_AddHrdOvertimeSetting</c>) karena snapshot model tidak
    /// sinkron dengan migration SDM manual.
    /// </summary>
    /// <remarks>Ditulis atas izin pemilik 20 November 2026 ("migration boleh ditulis"); BELUM dijalankan ke database mana pun.</remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261120093000_AddHrdAssignmentLetter")]
    public partial class AddHrdAssignmentLetter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS public.""HrdAssignmentLetter"" (
    ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
    ""LetterNumber"" character varying(100) NULL,
    ""ActivityName"" character varying(200) NOT NULL,
    ""DutyCategory"" character varying(50) NOT NULL,
    ""BasisDescription"" character varying(1000) NULL,
    ""BasisDocumentReference"" character varying(500) NULL,
    ""TaskDescription"" character varying(2000) NOT NULL,
    ""Destination"" character varying(255) NOT NULL,
    ""StartAt"" timestamptz NOT NULL,
    ""EndAt"" timestamptz NOT NULL,
    ""RequestingOrganizationUnitId"" uuid NULL,
    ""LetterStatus"" character varying(30) NOT NULL DEFAULT 'Draft',
    ""SubmittedByUserId"" uuid NULL,
    ""SubmittedAt"" timestamptz NULL,
    ""VerifiedByUserId"" uuid NULL,
    ""VerifiedAt"" timestamptz NULL,
    ""VerificationNote"" character varying(1000) NULL,
    ""RevisionNote"" character varying(1000) NULL,
    ""EndorsedByUserId"" uuid NULL,
    ""EndorsedAt"" timestamptz NULL,
    ""EndorsementNote"" character varying(1000) NULL,
    ""SignerName"" character varying(200) NULL,
    ""SignerTitle"" character varying(200) NULL,
    ""IssuedByUserId"" uuid NULL,
    ""IssuedAt"" timestamptz NULL,
    ""RejectedByUserId"" uuid NULL,
    ""RejectedAt"" timestamptz NULL,
    ""RejectionReason"" character varying(1000) NULL,
    ""CancelledByUserId"" uuid NULL,
    ""CancelledAt"" timestamptz NULL,
    ""CancellationReason"" character varying(1000) NULL,
    ""ExecutionStartedByUserId"" uuid NULL,
    ""ExecutionStartedAt"" timestamptz NULL,
    ""CompletedByUserId"" uuid NULL,
    ""CompletedAt"" timestamptz NULL,
    ""ResultReport"" character varying(2000) NULL,
    ""ResultEvidenceReference"" character varying(500) NULL,
    ""VerificationHash"" character varying(128) NULL,
    ""DocumentSnapshotHtml"" text NULL,
    ""IsActive"" boolean NOT NULL DEFAULT true,
    ""CreateDateTime"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""CreateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ""UpdateDateTime"" timestamptz NULL,
    ""UpdateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ""DeleteDateTime"" timestamptz NULL,
    ""DeleteBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ""CancelDateTime"" timestamptz NULL,
    ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ""IsCancel"" boolean NOT NULL DEFAULT false,
    ""IsDelete"" boolean NOT NULL DEFAULT false,
    CONSTRAINT ""PK_HrdAssignmentLetter"" PRIMARY KEY (""Id""),
    CONSTRAINT ""CK_HrdAssignmentLetter_Period"" CHECK (""EndAt"" > ""StartAt"")
);

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_HrdAssignmentLetter_LetterNumber""
    ON public.""HrdAssignmentLetter"" (""LetterNumber"")
    WHERE ""LetterNumber"" IS NOT NULL AND ""IsDelete"" = false;
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_HrdAssignmentLetter_VerificationHash""
    ON public.""HrdAssignmentLetter"" (""VerificationHash"")
    WHERE ""VerificationHash"" IS NOT NULL AND ""IsDelete"" = false;
CREATE INDEX IF NOT EXISTS ""IX_HrdAssignmentLetter_LetterStatus_StartAt""
    ON public.""HrdAssignmentLetter"" (""LetterStatus"", ""StartAt"");
CREATE INDEX IF NOT EXISTS ""IX_HrdAssignmentLetter_RequestingOrganizationUnitId""
    ON public.""HrdAssignmentLetter"" (""RequestingOrganizationUnitId"");

CREATE TABLE IF NOT EXISTS public.""HrdAssignmentLetterParticipant"" (
    ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
    ""AssignmentLetterId"" uuid NOT NULL,
    ""WorkforceProfileId"" uuid NOT NULL,
    ""ParticipantRole"" character varying(100) NULL,
    ""DisplayNameSnapshot"" character varying(200) NOT NULL,
    ""ProfileCodeSnapshot"" character varying(50) NULL,
    ""PositionNameSnapshot"" character varying(200) NULL,
    ""UnitNameSnapshot"" character varying(200) NULL,
    ""OfficialDutyRequestId"" uuid NULL,
    ""ScheduleConflictNote"" character varying(500) NULL,
    ""IsActive"" boolean NOT NULL DEFAULT true,
    ""CreateDateTime"" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""CreateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ""UpdateDateTime"" timestamptz NULL,
    ""UpdateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ""DeleteDateTime"" timestamptz NULL,
    ""DeleteBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ""CancelDateTime"" timestamptz NULL,
    ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ""IsCancel"" boolean NOT NULL DEFAULT false,
    ""IsDelete"" boolean NOT NULL DEFAULT false,
    CONSTRAINT ""PK_HrdAssignmentLetterParticipant"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_HrdAssignmentLetterParticipant_HrdAssignmentLetter_AssignmentLetterId""
        FOREIGN KEY (""AssignmentLetterId"") REFERENCES public.""HrdAssignmentLetter"" (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_HrdAssignmentLetterParticipant_MstWorkforceProfile_WorkforceProfileId""
        FOREIGN KEY (""WorkforceProfileId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_HrdAssignmentLetterParticipant_AssignmentLetterId_WorkforceProfileId""
    ON public.""HrdAssignmentLetterParticipant"" (""AssignmentLetterId"", ""WorkforceProfileId"")
    WHERE ""IsDelete"" = false;
CREATE INDEX IF NOT EXISTS ""IX_HrdAssignmentLetterParticipant_WorkforceProfileId""
    ON public.""HrdAssignmentLetterParticipant"" (""WorkforceProfileId"");
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS public.""HrdAssignmentLetterParticipant"";
DROP TABLE IF EXISTS public.""HrdAssignmentLetter"";");
        }
    }
}
