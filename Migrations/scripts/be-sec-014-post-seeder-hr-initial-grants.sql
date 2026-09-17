-- =====================================================================================
-- BE-SEC-014 — PEMBERIAN HAK AWAL HR (SESUDAH SEEDER)
-- Enam resource master data kepegawaian, matriks yang disetujui pemilik sistem
--
-- STATUS BERKAS INI: RANCANGAN. BELUM DIJALANKAN. BELUM DIVALIDASI TERHADAP DATABASE.
--   Berkas ini berakhir dengan ROLLBACK. Apa adanya, ia TIDAK mengubah apa pun.
--
-- CARA MENJALANKAN
--   DBeaver: Execute script (Alt+X). psql: -f <berkas ini>.
--   Tidak memakai meta-command psql. Parameter disimpan lewat set_config() supaya
--   terbaca sama di kedua alat, termasuk di dalam blok DO.
--
-- KENAPA HARUS SESUDAH SEEDER
--   Sepuluh identitas BE-SEC-012 dan empat identitas BE-SEC-013 baru ada di database
--   setelah AccessMenuSeeder berjalan pada source yang memuat keduanya. Dijalankan
--   sebelum itu, sambungan ke baris registry kosong dan skrip ini menyisipkan NOL baris.
--   Itu bukan kegagalan senyap: bagian 3.1 memang dirancang membatalkan transaksi.
--
-- MATRIKS YANG DISETUJUI PEMILIK SISTEM
--
--   Aturan umum master data: otorisasi mengikuti kepemilikan departemen.
--     Manajer : Read, Create, Update, Delete
--     Staff   : Read, Create — tanpa Update, tanpa Delete
--
--   Resource yang dicakup:
--     WorkSchedule, Shift, ShiftGroup, ShiftPattern, WorkCalendar, WorkScheduleAssignment
--
--   Human Resource x Manajer HR        -> Read, Create, Update, Delete  (keenam resource)
--   Human Resource x posisi staff      -> Read, Create                  (keenam resource)
--   Finance                            -> TIDAK SATU PUN diberikan di sini
--
-- YANG TIDAK DILAKUKAN BERKAS INI
--   - TIDAK memberi hak apa pun kepada Finance
--   - TIDAK menebak posisi mana yang "staff" — lihat bagian 2
--   - TIDAK membuat baris ganda; penyisipan memakai kunci alami
--   - TIDAK menyentuh resource di luar keenam nama di atas
--   - TIDAK menyentuh KioskScanSession.Cancel, Queue.*, BillingItemCategory.* (di luar scope)
--   - TIDAK menghapus baris apa pun
--
-- IDEMPOTEN
--   Dijalankan dua kali menghasilkan keadaan yang sama. Penyisipan disaring
--   NOT EXISTS atas kunci alami (DepartmentId, PositionId, ControllerAccessId,
--   ActionAccessId), sehingga baris yang sudah ada tidak digandakan.
-- =====================================================================================

-- =====================================================================================
-- BAGIAN 1 — Prasyarat registry. BACA-SAJA. Jalankan lebih dulu.
--
-- Keenam resource beserta actionnya WAJIB sudah terdaftar dan aktif. Bila ada yang
-- 'BELUM SIAP', AccessMenuSeeder belum berjalan pada source BE-SEC-012 + BE-SEC-013.
-- =====================================================================================

WITH wajib(resource, action) AS (
    SELECT r, a
    FROM unnest(ARRAY['WorkSchedule','Shift','ShiftGroup','ShiftPattern',
                      'WorkCalendar','WorkScheduleAssignment']) r
    CROSS JOIN unnest(ARRAY['Read','Create','Update','Delete']) a
)
SELECT
    w.resource,
    w.action,
    CASE
        WHEN a."Id" IS NULL                       THEN 'BELUM SIAP - tidak terdaftar'
        WHEN a."IsDelete"                         THEN 'BELUM SIAP - dihapus'
        WHEN NOT a."IsActive"                     THEN 'BELUM SIAP - nonaktif'
        WHEN NOT c."IsActive" OR c."IsDelete"     THEN 'BELUM SIAP - resource tertutup'
        ELSE 'siap'
    END AS status
FROM wajib w
LEFT JOIN public."SysControllerAccess" c ON c."ControllerName" = w.resource
LEFT JOIN public."SysActionAccess"     a ON a."ControllerAccessId" = c."Id"
                                        AND a."ActionName" = w.action
ORDER BY w.resource, w.action;

-- =====================================================================================
-- BAGIAN 2 — POSISI STAFF HR. WAJIB DIISI PEMILIK SISTEM.
--
-- Skrip ini TIDAK menebak nama posisi staff. Jalankan bagian 2.1 skrip
-- be-sec-014-current-holders-readonly-dbeaver.sql lebih dulu, tunjukkan keluarannya
-- kepada pemilik sistem, lalu tuliskan nama posisi yang DISETUJUI sebagai staff pada
-- daftar di bagian 3.2.
--
-- Dibiarkan kosong, skrip ini tetap berjalan dan hanya memberi hak kepada Manajer HR.
-- Itu keadaan yang aman dan disengaja: lebih baik kurang memberi daripada mengarang
-- penerima.
-- =====================================================================================

-- =====================================================================================
-- BAGIAN 3 — PENULISAN.
--
-- Berakhir dengan ROLLBACK. Untuk benar-benar menerapkannya:
--   (a) bagian 1 WAJIB seluruhnya 'siap';
--   (b) isi UUID operator pada bagian 3.1;
--   (c) isi daftar posisi staff yang disetujui pada bagian 3.2;
--   (d) tinjau dry run bagian 3.6;
--   (e) ganti ROLLBACK di akhir dengan COMMIT.
-- =====================================================================================

BEGIN;

-- 3.1 Parameter transaksi.
--     >>> GANTI UUID DI BAWAH DENGAN GUID OPERATOR YANG SEBENARNYA <<<
SELECT set_config('be_sec_014.aktor',       '00000000-0000-0000-0000-000000000000', true);
SELECT set_config('be_sec_014.dept_hr',     'Human Resource',                       true);
SELECT set_config('be_sec_014.pos_manajer', 'Manajer HR',                           true);

-- 3.2 Posisi staff HR yang DISETUJUI pemilik sistem.
--     Kosongkan bila belum disetujui — Manajer HR tetap dilayani.
--     Tambahkan satu baris VALUES per posisi, persis seperti ejaan pada MstPosition.
CREATE TEMP TABLE be_sec_014_pos_staff (position_name text PRIMARY KEY) ON COMMIT DROP;

INSERT INTO be_sec_014_pos_staff (position_name)
SELECT v FROM (VALUES
    -- ('Staff HR'),
    -- ('Admin HR'),
    (NULL::text)
) s(v)
WHERE v IS NOT NULL;

-- 3.3 Gerbang: operator wajib nyata.
DO $$
DECLARE
    aktor_teks text := current_setting('be_sec_014.aktor', true);
    aktor      uuid;
    jml        integer;
BEGIN
    IF aktor_teks IS NULL OR aktor_teks = '' THEN
        RAISE EXCEPTION 'Parameter be_sec_014.aktor belum disetel. Jalankan bagian 3.1 lebih dulu.';
    END IF;

    aktor := aktor_teks::uuid;

    IF aktor = '00000000-0000-0000-0000-000000000000'::uuid THEN
        RAISE EXCEPTION
            'UUID operator belum diisi. Ganti nilai be_sec_014.aktor pada bagian 3.1 dengan '
            'Guid pengguna yang bertanggung jawab atas pemberian hak ini.';
    END IF;

    SELECT count(*) INTO jml FROM public."AspNetUsers" WHERE "Id" = aktor;

    IF jml <> 1 THEN
        RAISE EXCEPTION 'Operator % tidak ditemukan pada AspNetUsers.', aktor;
    END IF;

    RAISE NOTICE 'Operator terverifikasi: %', aktor;
END $$;

-- 3.4 Gerbang: kunci bisnis HR wajib menunjuk tepat satu baris masing-masing,
--     dan setiap posisi staff yang disebut wajib benar-benar ada pada departemen itu.
DO $$
DECLARE
    nama_dept text := current_setting('be_sec_014.dept_hr', true);
    nama_pos  text := current_setting('be_sec_014.pos_manajer', true);
    jml       integer;
    hilang    text;
BEGIN
    SELECT count(*) INTO jml
      FROM public."MstDepartment"
     WHERE "DepartmentName" = nama_dept AND NOT "IsDelete";
    IF jml <> 1 THEN
        RAISE EXCEPTION 'Departemen "%" tidak menunjuk tepat satu baris (ditemukan %).',
            nama_dept, jml;
    END IF;

    SELECT count(*) INTO jml
      FROM public."MstPosition" p
      JOIN public."MstDepartment" d ON d."Id" = p."DepartmentId"
     WHERE d."DepartmentName" = nama_dept AND p."PositionName" = nama_pos AND NOT p."IsDelete";
    IF jml <> 1 THEN
        RAISE EXCEPTION 'Posisi manajer "%" pada "%" tidak menunjuk tepat satu baris (ditemukan %).',
            nama_pos, nama_dept, jml;
    END IF;

    SELECT string_agg(s.position_name, ', ' ORDER BY s.position_name) INTO hilang
      FROM be_sec_014_pos_staff s
     WHERE NOT EXISTS (
        SELECT 1
          FROM public."MstPosition" p
          JOIN public."MstDepartment" d ON d."Id" = p."DepartmentId"
         WHERE d."DepartmentName" = nama_dept
           AND p."PositionName"   = s.position_name
           AND NOT p."IsDelete");

    IF hilang IS NOT NULL THEN
        RAISE EXCEPTION
            'Posisi staff berikut tidak ditemukan pada departemen "%": %. '
            'Pakai ejaan persis dari bagian 2.1 skrip pembacaan.', nama_dept, hilang;
    END IF;

    SELECT count(*) INTO jml FROM be_sec_014_pos_staff;
    RAISE NOTICE 'Kunci bisnis lulus. Posisi staff yang disetujui: %', jml;
END $$;

-- 3.5 Matriks sasaran, diturunkan dari kunci bisnis. Tidak ada GUID yang diketik.
CREATE TEMP TABLE be_sec_014_target ON COMMIT DROP AS
WITH dept AS (
    SELECT "Id"
      FROM public."MstDepartment"
     WHERE "DepartmentName" = current_setting('be_sec_014.dept_hr') AND NOT "IsDelete"
),
peran AS (
    -- Manajer: seluruh empat action.
    SELECT p."Id" AS position_id, p."PositionName", a AS action
      FROM public."MstPosition" p
      JOIN dept d ON d."Id" = p."DepartmentId"
      CROSS JOIN unnest(ARRAY['Read','Create','Update','Delete']) a
     WHERE p."PositionName" = current_setting('be_sec_014.pos_manajer')
       AND NOT p."IsDelete"
    UNION ALL
    -- Staff: hanya Read dan Create.
    SELECT p."Id", p."PositionName", a
      FROM public."MstPosition" p
      JOIN dept d ON d."Id" = p."DepartmentId"
      JOIN be_sec_014_pos_staff s ON s.position_name = p."PositionName"
      CROSS JOIN unnest(ARRAY['Read','Create']) a
     WHERE NOT p."IsDelete"
),
sumber AS (
    SELECT r FROM unnest(ARRAY['WorkSchedule','Shift','ShiftGroup','ShiftPattern',
                               'WorkCalendar','WorkScheduleAssignment']) r
)
SELECT
    d."Id"    AS department_id,
    pr.position_id,
    pr."PositionName",
    c."Id"    AS controller_access_id,
    a."Id"    AS action_access_id,
    c."ControllerName" AS resource,
    a."ActionName"     AS action
FROM dept d
CROSS JOIN peran pr
JOIN sumber s ON true
JOIN public."SysControllerAccess" c ON c."ControllerName" = s.r
                                   AND c."IsActive" AND NOT c."IsDelete"
JOIN public."SysActionAccess"     a ON a."ControllerAccessId" = c."Id"
                                   AND a."ActionName" = pr.action
                                   AND a."IsActive" AND NOT a."IsDelete";

-- 3.6 DRY RUN — kandidat yang akan disisipkan dan yang sudah ada.
--     Tinjau angka ini sebelum mengganti ROLLBACK dengan COMMIT.
SELECT
    t."PositionName",
    t.resource,
    t.action,
    CASE WHEN EXISTS (
        SELECT 1 FROM public."SysAccessPolicy" p
         WHERE p."DepartmentId"       = t.department_id
           AND p."PositionId"         = t.position_id
           AND p."ControllerAccessId" = t.controller_access_id
           AND p."ActionAccessId"     = t.action_access_id
    ) THEN 'sudah ada' ELSE 'AKAN DISISIPKAN' END AS rencana
FROM be_sec_014_target t
ORDER BY t."PositionName", t.resource, t.action;

SELECT
    count(*)                                                     AS total_sasaran,
    count(*) FILTER (WHERE NOT ada)                              AS akan_disisipkan,
    count(*) FILTER (WHERE ada)                                  AS sudah_ada
FROM (
    SELECT EXISTS (
        SELECT 1 FROM public."SysAccessPolicy" p
         WHERE p."DepartmentId"       = t.department_id
           AND p."PositionId"         = t.position_id
           AND p."ControllerAccessId" = t.controller_access_id
           AND p."ActionAccessId"     = t.action_access_id
    ) AS ada
    FROM be_sec_014_target t
) x;

-- 3.7 Gerbang: seluruh 24 kombinasi manajer wajib terbentuk (6 resource x 4 action).
--     Bila kurang, registry belum lengkap dan penyisipan dibatalkan.
DO $$
DECLARE
    jml_manajer integer;
BEGIN
    SELECT count(*) INTO jml_manajer
      FROM be_sec_014_target
     WHERE "PositionName" = current_setting('be_sec_014.pos_manajer');

    IF jml_manajer <> 24 THEN
        RAISE EXCEPTION
            'Sasaran Manajer HR berjumlah %, diharapkan 24 (6 resource x 4 action). '
            'Registry belum lengkap — jalankan AccessMenuSeeder pada source BE-SEC-012 + '
            'BE-SEC-013 lebih dulu, lalu ulangi bagian 1.', jml_manajer;
    END IF;

    RAISE NOTICE 'Gerbang registry lulus: 24 sasaran Manajer HR terbentuk.';
END $$;

-- 3.8 Penyisipan. Hanya kunci alami yang belum ada.
--
--     "UpdateBy", "DeleteBy", dan "CancelBy" WAJIB disebut. Ketiganya uuid NOT NULL pada
--     SysAccessPolicy dan — berbeda dari tabel lain di repository ini — TIDAK memiliki
--     default database (lihat 20260516060830_initializeSetup.cs bagian CreateTable
--     "SysAccessPolicy"). Menghilangkannya membuat penyisipan gagal dengan
--     'null value in column "UpdateBy" violates not-null constraint'.
--
--     Nilainya Guid.Empty, mengikuti konvensi repository untuk baris SysAccessPolicy yang
--     baru dibuat: RoleAccessController.cs menyetel CreateBy = pengguna saat ini, lalu
--     UpdateBy = DeleteBy = CancelBy = Guid.Empty. Hanya CreateBy yang memuat pelaku.
INSERT INTO public."SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "IsDelete", "IsCancel", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy")
SELECT
    gen_random_uuid(), t.department_id, t.position_id,
    t.controller_access_id, t.action_access_id,
    true, true, false, false, now(), current_setting('be_sec_014.aktor')::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid
FROM be_sec_014_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
     WHERE p."DepartmentId"       = t.department_id
       AND p."PositionId"         = t.position_id
       AND p."ControllerAccessId" = t.controller_access_id
       AND p."ActionAccessId"     = t.action_access_id);

-- 3.9 Penegasan sesudah tulis.
DO $$
DECLARE
    belum      integer;
    finance    integer;
    ganda      integer;
BEGIN
    -- (a) seluruh sasaran wajib terbentuk
    SELECT count(*) INTO belum
      FROM be_sec_014_target t
     WHERE NOT EXISTS (
        SELECT 1 FROM public."SysAccessPolicy" p
         WHERE p."DepartmentId"       = t.department_id
           AND p."PositionId"         = t.position_id
           AND p."ControllerAccessId" = t.controller_access_id
           AND p."ActionAccessId"     = t.action_access_id
           AND p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete");

    IF belum > 0 THEN
        RAISE EXCEPTION '% sasaran belum terbentuk sesudah penyisipan. Transaksi dibatalkan.', belum;
    END IF;

    -- (b) Finance tidak boleh memperoleh apa pun pada keenam resource
    SELECT count(*) INTO finance
      FROM public."SysAccessPolicy" pol
      JOIN public."MstDepartment"       d ON d."Id" = pol."DepartmentId"
      JOIN public."SysActionAccess"     a ON a."Id" = pol."ActionAccessId"
      JOIN public."SysControllerAccess" c ON c."Id" = a."ControllerAccessId"
     WHERE d."DepartmentName" ILIKE '%finance%'
       AND c."ControllerName" IN ('WorkSchedule','Shift','ShiftGroup','ShiftPattern',
                                  'WorkCalendar','WorkScheduleAssignment')
       AND a."ActionName" IN ('Update','Delete')
       AND pol."IsAllowed" AND pol."IsActive" AND NOT pol."IsDelete";

    IF finance > 0 THEN
        RAISE EXCEPTION
            'Ditemukan % policy Update/Delete aktif milik departemen Finance pada resource HR. '
            'Pencabutan pra-seeder belum dijalankan, atau ada pemberian hak lain. '
            'Transaksi dibatalkan.', finance;
    END IF;

    -- (c) tidak boleh ada kunci alami ganda
    SELECT count(*) INTO ganda
      FROM (
        SELECT 1
          FROM public."SysAccessPolicy"
         GROUP BY "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId"
        HAVING count(*) > 1
      ) g;

    IF ganda > 0 THEN
        RAISE EXCEPTION '% kunci alami ganda pada SysAccessPolicy. Transaksi dibatalkan.', ganda;
    END IF;

    RAISE NOTICE 'Seluruh penegasan lulus: sasaran terbentuk, Finance bersih, tidak ada duplikat.';
END $$;

-- Ganti baris berikut dengan COMMIT; hanya bila seluruh NOTICE di atas benar.
ROLLBACK;

-- =====================================================================================
-- BAGIAN 4 — Verifikasi sesudah COMMIT
-- =====================================================================================

-- 4.1 Hak efektif HR atas keenam resource.
SELECT
    d."DepartmentName",
    p2."PositionName",
    c."ControllerName" AS resource,
    string_agg(a."ActionName", ', ' ORDER BY a."ActionName") AS action
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" ILIKE '%human resource%'
  AND c."ControllerName" IN ('WorkSchedule','Shift','ShiftGroup','ShiftPattern',
                             'WorkCalendar','WorkScheduleAssignment')
  AND pol."IsAllowed" AND pol."IsActive" AND NOT pol."IsDelete"
  AND a."IsActive" AND NOT a."IsDelete"
  AND c."IsActive" AND NOT c."IsDelete"
GROUP BY d."DepartmentName", p2."PositionName", c."ControllerName"
ORDER BY p2."PositionName", c."ControllerName";

-- 4.2 WAJIB nol baris — Finance tidak memegang Update/Delete pada resource HR.
SELECT d."DepartmentName", p2."PositionName", c."ControllerName", a."ActionName"
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" ILIKE '%finance%'
  AND c."ControllerName" IN ('WorkSchedule','Shift','ShiftGroup','ShiftPattern',
                             'WorkCalendar','WorkScheduleAssignment')
  AND a."ActionName" IN ('Update','Delete')
  AND pol."IsAllowed" AND pol."IsActive" AND NOT pol."IsDelete";

-- =====================================================================================
-- BAGIAN 5 — ROLLBACK
--
-- Menonaktifkan hanya baris yang dibuat skrip ini, dikenali dari CreateBy dan waktu.
-- Baris tidak dihapus, supaya jejaknya tetap dapat diperiksa.
-- =====================================================================================

/*  ---------- ROLLBACK ----------

BEGIN;

-- Guid yang SAMA dengan yang dipakai saat pemberian hak.
SELECT set_config('be_sec_014.aktor', '00000000-0000-0000-0000-000000000000', true);

UPDATE public."SysAccessPolicy" p
SET "IsActive"       = false,
    "UpdateDateTime" = now(),
    "UpdateBy"       = current_setting('be_sec_014.aktor')::uuid
FROM public."MstDepartment"        d,
     public."SysActionAccess"      a,
     public."SysControllerAccess"  c
WHERE d."Id" = p."DepartmentId"
  AND a."Id" = p."ActionAccessId"
  AND c."Id" = a."ControllerAccessId"
  AND d."DepartmentName" ILIKE '%human resource%'
  AND c."ControllerName" IN ('WorkSchedule','Shift','ShiftGroup','ShiftPattern',
                             'WorkCalendar','WorkScheduleAssignment')
  AND p."CreateBy" = current_setting('be_sec_014.aktor')::uuid
  AND p."IsActive";

-- Ganti dengan COMMIT; hanya setelah bagian 4.1 kembali ke keadaan sebelum pemberian hak.
ROLLBACK;

    ---------- BATAS ROLLBACK ---------- */
