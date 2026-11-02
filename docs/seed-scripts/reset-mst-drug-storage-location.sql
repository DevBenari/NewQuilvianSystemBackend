-- ==============================================================================
-- Quilvian Hospital System - Pengosongan Master Lokasi Penyimpanan Obat
-- Modul: Farmasi / Master Data
-- Target Tabel: public."MstDrugStorageLocation"
-- ==============================================================================
--
-- Dijalankan SATU KALI sebelum DrugStorageLocationSeeder dinyalakan pertama kali,
-- untuk membuang isi master lokasi lama yang dibuat coba-coba selama pengembangan.
-- Setelah master terisi dari seeder, skrip ini tidak boleh dijalankan lagi.
--
-- MENGAPA ADA PENJAGA DI DALAMNYA
--
-- Lokasi penyimpanan dirujuk banyak tabel lain: penempatan kantong darah, sumber
-- stok kamar operasi, kebijakan stok obat, reservasi dan penyerahan resep. Menghapus
-- baris yang masih dirujuk berarti menggantung catatan stok pada lokasi yang sudah
-- tidak ada - kekeliruan yang jauh lebih mahal daripada master yang masih kotor.
--
-- Karena itu skrip ini TIDAK menghapus apa pun bila masih ada satu saja baris di
-- tabel lain yang menunjuk ke master ini. Ia berhenti dengan pesan yang menyebut
-- tabel dan kolomnya, supaya yang menghapus tahu persis apa yang menghalangi.
--
-- Referensi dicari sendiri dari katalog foreign key, bukan dari daftar tabel yang
-- ditulis tangan, supaya tabel baru yang menunjuk ke master ini ikut terdeteksi
-- tanpa skrip ini perlu diperbarui.
--
-- CARA PAKAI
--
--   psql -h <host> -d <database> -f reset-mst-drug-storage-location.sql
--
-- Periksa baris keluaran pertama: pastikan nama basis data yang tercetak benar-benar
-- basis data yang Anda maksud sebelum melanjutkan.
-- ==============================================================================

\echo '--- Target eksekusi ---'
SELECT
    current_database() AS basis_data,
    coalesce(host(inet_server_addr()), 'socket lokal') AS host,
    inet_server_port() AS port,
    current_user AS dijalankan_oleh;

SELECT count(*) AS jumlah_lokasi_sekarang FROM public."MstDrugStorageLocation";

BEGIN;

DO $$
DECLARE
    fk          record;
    penghalang  text := '';
    jumlah      bigint;
BEGIN
    FOR fk IN
        SELECT
            src_ns.nspname  AS src_schema,
            src.relname     AS src_table,
            src_col.attname AS src_column
        FROM pg_constraint c
        JOIN pg_class     src     ON src.oid = c.conrelid
        JOIN pg_namespace src_ns  ON src_ns.oid = src.relnamespace
        JOIN pg_class     tgt     ON tgt.oid = c.confrelid
        JOIN pg_namespace tgt_ns  ON tgt_ns.oid = tgt.relnamespace
        JOIN unnest(c.conkey) WITH ORDINALITY AS k(attnum, ord) ON true
        JOIN pg_attribute src_col
             ON src_col.attrelid = c.conrelid AND src_col.attnum = k.attnum
        WHERE c.contype = 'f'
          AND tgt_ns.nspname = 'public'
          AND tgt.relname = 'MstDrugStorageLocation'
          AND src.relname <> 'MstDrugStorageLocation'
    LOOP
        EXECUTE format(
            'SELECT count(*) FROM %I.%I WHERE %I IS NOT NULL',
            fk.src_schema, fk.src_table, fk.src_column)
        INTO jumlah;

        IF jumlah > 0 THEN
            penghalang := penghalang
                || format(E'\n  - %I.%I.%I : %s baris',
                          fk.src_schema, fk.src_table, fk.src_column, jumlah);
        END IF;
    END LOOP;

    IF penghalang <> '' THEN
        RAISE EXCEPTION
            'Pengosongan dibatalkan. Master lokasi penyimpanan obat masih dirujuk:%s%s',
            penghalang,
            E'\n\nSelesaikan dulu data yang merujuk itu, atau pindahkan ke lokasi '
            'yang benar, sebelum mengosongkan master.';
    END IF;
END
$$;

DELETE FROM public."MstDrugStorageLocation";

SELECT count(*) AS jumlah_lokasi_setelah FROM public."MstDrugStorageLocation";

COMMIT;

\echo '--- Selesai. Nyalakan Seeders:RunDrugStorageLocationSeed lalu jalankan backend. ---'
