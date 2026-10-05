using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class SetFinPurchasingIdempotencyRecordDefaultValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    -- CancelBy
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
          AND table_name = 'FinPurchasingIdempotencyRecord' 
          AND column_name = 'CancelBy'
    ) THEN
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ADD COLUMN ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid;
    END IF;

    -- CancelDateTime
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
          AND table_name = 'FinPurchasingIdempotencyRecord' 
          AND column_name = 'CancelDateTime'
    ) THEN
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ADD COLUMN ""CancelDateTime"" timestamp with time zone NULL;
    END IF;

    -- DeleteBy
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
          AND table_name = 'FinPurchasingIdempotencyRecord' 
          AND column_name = 'DeleteBy'
    ) THEN
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ADD COLUMN ""DeleteBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid;
    END IF;

    -- DeleteDateTime
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
          AND table_name = 'FinPurchasingIdempotencyRecord' 
          AND column_name = 'DeleteDateTime'
    ) THEN
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ADD COLUMN ""DeleteDateTime"" timestamp with time zone NULL;
    END IF;

    -- IsCancel
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
          AND table_name = 'FinPurchasingIdempotencyRecord' 
          AND column_name = 'IsCancel'
    ) THEN
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ADD COLUMN ""IsCancel"" boolean NOT NULL DEFAULT false;
    ELSE
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ALTER COLUMN ""IsCancel"" SET DEFAULT false;
    END IF;

    -- IsDelete
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
          AND table_name = 'FinPurchasingIdempotencyRecord' 
          AND column_name = 'IsDelete'
    ) THEN
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ADD COLUMN ""IsDelete"" boolean NOT NULL DEFAULT false;
    ELSE
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ALTER COLUMN ""IsDelete"" SET DEFAULT false;
    END IF;

    -- UpdateBy
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
          AND table_name = 'FinPurchasingIdempotencyRecord' 
          AND column_name = 'UpdateBy'
    ) THEN
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ADD COLUMN ""UpdateBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid;
    END IF;

    -- UpdateDateTime
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
          AND table_name = 'FinPurchasingIdempotencyRecord' 
          AND column_name = 'UpdateDateTime'
    ) THEN
        ALTER TABLE public.""FinPurchasingIdempotencyRecord"" 
        ADD COLUMN ""UpdateDateTime"" timestamp with time zone NULL;
    END IF;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
