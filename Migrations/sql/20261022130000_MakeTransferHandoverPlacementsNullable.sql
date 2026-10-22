-- Migrasi: 20261022130000_MakeTransferHandoverPlacementsNullable
-- Task: BE-RWI-241 (Transfer Pasien Masuk IGD/Rajal ke Ranap)
-- Keterangan: Mengubah kolom FromPlacementId dan ToPlacementId menjadi nullable,
-- serta memperbarui partial unique index UX_CliTransferHandover_ToPlacement.

ALTER TABLE public."CliTransferHandover" ALTER COLUMN "FromPlacementId" DROP NOT NULL;
ALTER TABLE public."CliTransferHandover" ALTER COLUMN "ToPlacementId" DROP NOT NULL;

DROP INDEX IF EXISTS public."UX_CliTransferHandover_ToPlacement";

CREATE UNIQUE INDEX "UX_CliTransferHandover_ToPlacement" 
ON public."CliTransferHandover" ("ToPlacementId") 
WHERE "ToPlacementId" IS NOT NULL;
