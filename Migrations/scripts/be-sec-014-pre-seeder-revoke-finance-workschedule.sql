-- =====================================================================================
-- BE-SEC-014 — PENCABUTAN PRA-SEEDER
-- Menonaktifkan hak tertidur Finance x Manajer Finance atas WorkSchedule.Update/Delete
--
-- STATUS BERKAS INI: RANCANGAN. BELUM DIJALANKAN. BELUM DIVALIDASI TERHADAP DATABASE.
--   Berkas ini berakhir dengan ROLLBACK. Apa adanya, ia TIDAK mengubah apa pun.
--
-- CARA MENJALANKAN
--   DBeaver: buka berkas ini, lalu Execute script (Alt+X) — bukan Execute statement.
--   psql   : psql -h <host> -p <port> -U <user> -d <db> -f <berkas ini>
--
--   Berkas ini sengaja TIDAK memakai meta-command psql (\set). Seluruh parameter
--   disimpan lewat set_config(), yang merupakan SQL biasa dan karena itu berjalan sama
--   di DBeaver maupun psql. Ini juga menghindari jebakan yang sering terlewat: psql
--   TIDAK menginterpolasi :'variabel' di dalam blok dollar-quoted ($$ ... $$), sehingga
--   penegasan di dalam DO tidak akan pernah membaca nilai yang dimaksud.
--
-- KENAPA HARUS DIJALANKAN SEBELUM SEEDER, BUKAN SESUDAH
--
--   Hari ini baris SysActionAccess untuk WorkSchedule.Update dan WorkSchedule.Delete
--   berstatus TERTUTUP, sehingga dua policy Finance yang menunjuknya tidak memberi hak
--   apa pun. Source BE-SEC-012 mendeklarasikan kembali kedua identitas itu, dan
--   AccessMenuSeeder MENGAKTIFKAN ULANG baris registry yang sudah ada — bukan membuat
--   baris baru (AccessMenuSeeder.cs:151-160). Begitu identitasnya hidup lagi, kedua
--   policy Finance itu seketika menjadi pemberian hak yang HIDUP, tanpa seorang pun
--   pernah menyetujuinya.
--
--   Sesudah seeder, pencabutan ini berubah sifat: dari "mencegah hak yang belum ada"
--   menjadi "mencabut hak yang sedang dipakai". Urutannya karena itu TIDAK BOLEH DIBALIK.
--
-- KEPUTUSAN PEMILIK YANG MENDASARI
--   Untuk master data, otorisasi mengikuti kepemilikan departemen. Jadwal kerja adalah
--   master data milik Human Resource. Finance x Manajer Finance TIDAK boleh
--   mempertahankan WorkSchedule.Update/Delete hanya karena policy warisan kebetulan ada.
--   Tidak ada izin Finance lain yang boleh disimpulkan dari keputusan ini.
--
-- YANG TIDAK DILAKUKAN BERKAS INI
--   - TIDAK menyentuh Human Resource x Manajer HR
--   - TIDAK menyentuh WorkSchedule.Read maupun WorkSchedule.Create milik siapa pun
--   - TIDAK menyentuh Shift, ShiftGroup, ShiftPattern, WorkCalendar, WorkScheduleAssignment
--   - TIDAK menyentuh KioskScanSession.Cancel, Queue.*, BillingItemCategory.* (di luar scope)
--   - TIDAK menghapus baris. Penonaktifan memakai IsActive, sesuai kebiasaan repository
--
-- PRINSIP
--   1. Tidak ada GUID departemen/posisi/action di berkas ini. Seluruh Id diturunkan dari
--      kunci bisnis. Satu-satunya GUID yang diketik manusia adalah UUID operator.
--   2. Kardinalitas ditegaskan TEPAT 2 baris. Bukan 2 = transaksi dibatalkan.
--   3. Satu transaksi. Gagal di tengah = tidak ada yang berubah.
--   4. UUID operator wajib diisi dan diverifikasi ada di AspNetUsers.
-- =====================================================================================

-- =====================================================================================
-- BAGIAN 1 — DRY RUN. Hanya membaca. Jalankan dan tinjau SEBELUM bagian 2.
--
-- Nama departemen dan posisi ditulis langsung di bagian ini supaya dapat dijalankan
-- sendirian tanpa transaksi. Bila ejaannya berbeda pada database ini, perbaiki di sini
-- DAN di bagian 2.1.
-- =====================================================================================

-- 1.1 Sasaran pencabutan. WAJIB tepat 2 baris: Update dan Delete.
--     Perhatikan: flag action TIDAK disaring, karena sasarannya justru policy yang
--     menunjuk identitas tertutup.
SELECT
    d."DepartmentName",
    p2."PositionName",
    c."ControllerName"  AS resource,
    a."ActionName"      AS action,
    pol."Id"            AS policy_id,
    pol."IsActive"      AS policy_aktif,
    a."IsActive"        AS action_aktif,
    a."IsDelete"        AS action_dihapus
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" = 'Finance'
  AND p2."PositionName"  = 'Manajer Finance'
  AND c."ControllerName" = 'WorkSchedule'
  AND a."ActionName" IN ('Update','Delete')
  AND pol."IsActive"
  AND NOT pol."IsDelete"
ORDER BY a."ActionName";

-- 1.2 Jaring pengaman: apa lagi yang dipegang pasangan ini pada keenam resource HR.
--     Baris-baris ini TIDAK akan disentuh.
SELECT
    c."ControllerName" AS resource,
    a."ActionName"     AS action,
    'TIDAK DISENTUH'   AS perlakuan
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" = 'Finance'
  AND p2."PositionName"  = 'Manajer Finance'
  AND pol."IsActive" AND NOT pol."IsDelete"
  AND NOT (c."ControllerName" = 'WorkSchedule' AND a."ActionName" IN ('Update','Delete'))
  AND c."ControllerName" IN (
        'WorkSchedule','Shift','ShiftGroup','ShiftPattern','WorkCalendar','WorkScheduleAssignment')
ORDER BY c."ControllerName", a."ActionName";

-- 1.3 Bukti bahwa Human Resource tidak berada dalam sasaran.
--     Seluruh baris di sini WAJIB tetap ada sesudah bagian 2 dijalankan.
SELECT
    d."DepartmentName",
    p2."PositionName",
    c."ControllerName" AS resource,
    a."ActionName"     AS action
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" ILIKE '%human resource%'
  AND c."ControllerName" = 'WorkSchedule'
  AND pol."IsActive" AND NOT pol."IsDelete"
ORDER BY p2."PositionName", a."ActionName";

-- =====================================================================================
-- BAGIAN 2 — PENULISAN.
--
-- Berakhir dengan ROLLBACK. Untuk benar-benar menerapkannya, seseorang harus:
--   (a) meninjau seluruh keluaran bagian 1 lebih dulu — 1.1 WAJIB tepat 2 baris;
--   (b) mengganti UUID operator pada bagian 2.1 dengan Guid pengguna yang bertanggung jawab;
--   (c) mengganti baris ROLLBACK di akhir dengan COMMIT.
--
-- Bila salah satu penegasan gagal, RAISE EXCEPTION membatalkan SELURUH transaksi dan
-- tidak ada satu baris pun yang berubah.
-- =====================================================================================

BEGIN;

-- 2.1 Parameter transaksi. Nilai bertahan sampai transaksi berakhir (is_local = true),
--     dan terbaca di dalam blok DO maupun DML di bawah.
--
--     >>> GANTI UUID DI BAWAH DENGAN GUID OPERATOR YANG SEBENARNYA <<<
SELECT set_config('be_sec_014.aktor',        '00000000-0000-0000-0000-000000000000', true);
SELECT set_config('be_sec_014.dept_finance', 'Finance',                              true);
SELECT set_config('be_sec_014.pos_manajer',  'Manajer Finance',                      true);

-- 2.2 Gerbang: operator wajib nyata.
DO $$
DECLARE
    aktor_teks text := current_setting('be_sec_014.aktor', true);
    aktor      uuid;
    jml        integer;
BEGIN
    IF aktor_teks IS NULL OR aktor_teks = '' THEN
        RAISE EXCEPTION 'Parameter be_sec_014.aktor belum disetel. Jalankan bagian 2.1 lebih dulu.';
    END IF;

    aktor := aktor_teks::uuid;

    IF aktor = '00000000-0000-0000-0000-000000000000'::uuid THEN
        RAISE EXCEPTION
            'UUID operator belum diisi. Ganti nilai be_sec_014.aktor pada bagian 2.1 dengan '
            'Guid pengguna yang bertanggung jawab atas pencabutan ini. Jejak audit tanpa '
            'pelaku tidak dapat dipertanggungjawabkan.';
    END IF;

    SELECT count(*) INTO jml FROM public."AspNetUsers" WHERE "Id" = aktor;

    IF jml <> 1 THEN
        RAISE EXCEPTION
            'Operator % tidak ditemukan pada AspNetUsers. Pakai Guid pengguna yang benar-benar ada.',
            aktor;
    END IF;

    RAISE NOTICE 'Operator terverifikasi: %', aktor;
END $$;

-- 2.3 Gerbang: kunci bisnis wajib menunjuk tepat satu departemen dan satu posisi.
DO $$
DECLARE
    nama_dept text := current_setting('be_sec_014.dept_finance', true);
    nama_pos  text := current_setting('be_sec_014.pos_manajer',  true);
    jml_dept  integer;
    jml_pos   integer;
BEGIN
    SELECT count(*) INTO jml_dept
      FROM public."MstDepartment"
     WHERE "DepartmentName" = nama_dept AND NOT "IsDelete";

    IF jml_dept <> 1 THEN
        RAISE EXCEPTION
            'Departemen "%" tidak menunjuk tepat satu baris (ditemukan %). '
            'Periksa ejaannya lewat bagian 2.2 skrip be-sec-014-current-holders-readonly-dbeaver.sql.',
            nama_dept, jml_dept;
    END IF;

    SELECT count(*) INTO jml_pos
      FROM public."MstPosition" p
      JOIN public."MstDepartment" d ON d."Id" = p."DepartmentId"
     WHERE d."DepartmentName" = nama_dept
       AND p."PositionName"   = nama_pos
       AND NOT p."IsDelete";

    IF jml_pos <> 1 THEN
        RAISE EXCEPTION
            'Posisi "%" pada departemen "%" tidak menunjuk tepat satu baris (ditemukan %).',
            nama_pos, nama_dept, jml_pos;
    END IF;

    RAISE NOTICE 'Kunci bisnis lulus: 1 departemen, 1 posisi.';
END $$;

-- 2.4 Sasaran dibekukan lebih dulu, supaya yang ditegaskan dan yang diubah
--     benar-benar himpunan yang sama.
CREATE TEMP TABLE be_sec_014_sasaran ON COMMIT DROP AS
SELECT pol."Id" AS policy_id, a."ActionName" AS action
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" = current_setting('be_sec_014.dept_finance')
  AND p2."PositionName"  = current_setting('be_sec_014.pos_manajer')
  AND c."ControllerName" = 'WorkSchedule'
  AND a."ActionName" IN ('Update','Delete')
  AND pol."IsActive"
  AND NOT pol."IsDelete";

-- 2.5 Penegasan kardinalitas: TEPAT 2, dan tepat satu Update serta satu Delete.
DO $$
DECLARE
    jml        integer;
    jml_update integer;
    jml_delete integer;
BEGIN
    SELECT count(*),
           count(*) FILTER (WHERE action = 'Update'),
           count(*) FILTER (WHERE action = 'Delete')
      INTO jml, jml_update, jml_delete
      FROM be_sec_014_sasaran;

    IF jml <> 2 OR jml_update <> 1 OR jml_delete <> 1 THEN
        RAISE EXCEPTION
            'KARDINALITAS TIDAK SESUAI: ditemukan % baris (Update=%, Delete=%), diharapkan '
            'tepat 2 (Update=1, Delete=1). Transaksi dibatalkan dan tidak ada yang berubah. '
            'Jangan memaksakan skrip ini: selisih jumlah berarti keadaan database berbeda '
            'dari bukti yang mendasari keputusan pemilik.',
            jml, jml_update, jml_delete;
    END IF;

    RAISE NOTICE 'Kardinalitas lulus: tepat 2 policy (Update=1, Delete=1).';
END $$;

-- 2.6 Pencabutan. Baris tidak dihapus, hanya dinonaktifkan.
UPDATE public."SysAccessPolicy" p
SET "IsActive"       = false,
    "UpdateDateTime" = now(),
    "UpdateBy"       = current_setting('be_sec_014.aktor')::uuid
FROM be_sec_014_sasaran s
WHERE p."Id" = s.policy_id;

-- 2.7 Penegasan sesudah tulis.
DO $$
DECLARE
    sisa    integer;
    hr_utuh integer;
BEGIN
    SELECT count(*) INTO sisa
      FROM public."SysAccessPolicy" p
      JOIN be_sec_014_sasaran s ON s.policy_id = p."Id"
     WHERE p."IsActive";

    IF sisa <> 0 THEN
        RAISE EXCEPTION 'Masih ada % policy sasaran yang aktif. Transaksi dibatalkan.', sisa;
    END IF;

    SELECT count(*) INTO hr_utuh
      FROM public."SysAccessPolicy" pol
      JOIN public."MstDepartment"       d ON d."Id" = pol."DepartmentId"
      JOIN public."SysActionAccess"     a ON a."Id" = pol."ActionAccessId"
      JOIN public."SysControllerAccess" c ON c."Id" = a."ControllerAccessId"
     WHERE d."DepartmentName" ILIKE '%human resource%'
       AND c."ControllerName" = 'WorkSchedule'
       AND pol."IsActive" AND NOT pol."IsDelete";

    RAISE NOTICE 'Pencabutan siap. Policy Human Resource atas WorkSchedule yang tetap aktif: %',
        hr_utuh;
END $$;

-- Ganti baris berikut dengan COMMIT; hanya bila seluruh NOTICE di atas benar.
ROLLBACK;

-- =====================================================================================
-- BAGIAN 3 — Verifikasi sesudah COMMIT
-- =====================================================================================

-- 3.1 WAJIB nol.
SELECT count(*) AS finance_workschedule_masih_aktif
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" = 'Finance'
  AND p2."PositionName"  = 'Manajer Finance'
  AND c."ControllerName" = 'WorkSchedule'
  AND a."ActionName" IN ('Update','Delete')
  AND pol."IsActive" AND NOT pol."IsDelete";

-- 3.2 WAJIB tepat 2 — bukti barisnya dinonaktifkan, bukan dihapus.
SELECT count(*) AS finance_workschedule_dinonaktifkan
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" = 'Finance'
  AND p2."PositionName"  = 'Manajer Finance'
  AND c."ControllerName" = 'WorkSchedule'
  AND a."ActionName" IN ('Update','Delete')
  AND NOT pol."IsActive" AND NOT pol."IsDelete";

-- =====================================================================================
-- BAGIAN 4 — ROLLBACK
--
-- Jalankan hanya bila pencabutan sudah COMMIT dan harus dibatalkan.
-- Mengembalikan baris yang dinonaktifkan bagian 2, dikenali dari UpdateBy sehingga
-- tidak menyentuh baris yang dinonaktifkan pekerjaan lain.
-- =====================================================================================

/*  ---------- ROLLBACK ----------

BEGIN;

-- Guid yang SAMA dengan yang dipakai saat pencabutan.
SELECT set_config('be_sec_014.aktor', '00000000-0000-0000-0000-000000000000', true);

UPDATE public."SysAccessPolicy" p
SET "IsActive"       = true,
    "UpdateDateTime" = now(),
    "UpdateBy"       = current_setting('be_sec_014.aktor')::uuid
FROM public."MstDepartment"        d,
     public."MstPosition"          p2,
     public."SysActionAccess"      a,
     public."SysControllerAccess"  c
WHERE d."Id"  = p."DepartmentId"
  AND p2."Id" = p."PositionId"
  AND a."Id"  = p."ActionAccessId"
  AND c."Id"  = a."ControllerAccessId"
  AND d."DepartmentName" = 'Finance'
  AND p2."PositionName"  = 'Manajer Finance'
  AND c."ControllerName" = 'WorkSchedule'
  AND a."ActionName" IN ('Update','Delete')
  AND NOT p."IsActive"
  AND NOT p."IsDelete"
  AND p."UpdateBy" = current_setting('be_sec_014.aktor')::uuid;

-- Ganti dengan COMMIT; hanya setelah bagian 3.1 kembali menunjukkan 2.
ROLLBACK;

    ---------- BATAS ROLLBACK ---------- */
