using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixFinSupplierPayableMovementForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_schema = 'public'
          AND table_name = 'FinSupplierPayableMovement' 
          AND constraint_name = 'FK_FinSupplierPayableMovement_FinSupplierPayable_SupplierPayab~'
    ) THEN
        ALTER TABLE public.""FinSupplierPayableMovement"" 
        DROP CONSTRAINT ""FK_FinSupplierPayableMovement_FinSupplierPayable_SupplierPayab~"";
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_schema = 'public'
          AND table_name = 'FinSupplierPayableMovement' 
          AND constraint_type = 'FOREIGN KEY'
          AND (constraint_name = 'FK_FinSupplierPayableMovement_FinSupplierPayable_SupplierPayableId'
               OR constraint_name = 'FK_FinSupplierPayableMovement_FinSupplierPayable_SupplierPayabl')
    ) THEN
        ALTER TABLE public.""FinSupplierPayableMovement""
        ADD CONSTRAINT ""FK_FinSupplierPayableMovement_FinSupplierPayable_SupplierPayableId""
        FOREIGN KEY (""SupplierPayableId"") REFERENCES public.""FinSupplierPayable"" (""Id"") ON DELETE RESTRICT;
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
