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
--   Human Resource x Staff HR          -> Read, Create                  (keenam resource)
--
--   Finance: skrip ini tidak memberi hak apa pun kepada departemen mana pun selain
--   Human Resource. Itu pernyataan tentang APA YANG DILAKUKAN skrip ini, BUKAN kebijakan
--   bahwa Finance tidak boleh memegang apa-apa. Satu-satunya pembatasan Finance yang
--   disetujui pemilik adalah Finance x Manajer Finance TIDAK mempertahankan
--   WorkSchedule.Update dan WorkSchedule.Delete — dua hak tertidur yang dicabut skrip
--   be-sec-014-pre-seeder-revoke-finance-workschedule.sql. Izin Finance selain itu,
--   termasuk WorkSchedule.Read/Create dan seluruh izin pada kelima resource lain, berada
--   DI LUAR keputusan pemilik dan tidak disentuh maupun dipersoalkan di sini.
--
--   MATRIKS FINAL — DIPUTUSKAN PEMILIK SISTEM 17 SEPTEMBER 2026 (BE-SEC-016)
--
--     Audit baca-saja mengonfirmasi Departemen Human Resource hanya memiliki dua posisi:
--     Manajer HR dan Staff HR. Gerbang "daftar posisi staff belum disetujui" yang dibuka
--     BE-SEC-014 karena itu DITUTUP.
--
--       Manajer HR : 6 resource x 4 action = 24 kunci alami
--       Staff HR   : 6 resource x 2 action = 12 kunci alami
--       TOTAL                              = 36 kunci alami
--
--     36 adalah CAKUPAN AKHIR yang dituntut, bukan jumlah INSERT. Kunci alami yang sudah
--     ada dan masih efektif dipertahankan apa adanya; hanya yang belum ada yang disisipkan.
--
-- YANG TIDAK DILAKUKAN BERKAS INI
--   - TIDAK memberi hak apa pun kepada Finance, dan TIDAK mencabut maupun melarang izin
--     Finance selain WorkSchedule.Update/Delete milik Manajer Finance
--   - TIDAK memberi Update maupun Delete kepada Staff HR
--   - TIDAK menebak posisi staff — namanya ditetapkan pemilik, lihat bagian 3.2
--   - TIDAK menghidupkan ulang policy yang kunci alaminya ada tetapi nonaktif/dihapus;
--     keadaan seperti itu membatalkan transaksi (bagian 3.9a)
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
-- SUDAH DIPUTUSKAN — 17 September 2026. Bagian ini tidak lagi menunggu siapa pun.
--
-- Bagian 2.1 skrip be-sec-014-current-holders-readonly-dbeaver.sql sudah dijalankan dan
-- keluarannya ditinjau pemilik sistem. Departemen Human Resource hanya memiliki dua
-- posisi, dan pemilik menetapkan keduanya secara final:
--
--   Manajer HR -> Read, Create, Update, Delete
--   Staff HR   -> Read, Create   (TANPA Update, TANPA Delete)
--
-- Nama yang disetujui itu dikodekan pada bagian 3.2. Jangan menambah posisi lain.
-- Operator tetap DIANJURKAN menjalankan ulang bagian 2.1 sebelum COMMIT untuk memastikan
-- ejaan pada database belum berubah sejak audit — gerbang 3.4 memang membatalkan transaksi
-- bila berubah, tetapi lebih baik diketahui lebih dulu.
-- =====================================================================================

-- =====================================================================================
-- BAGIAN 3 — PENULISAN.
--
-- Berakhir dengan ROLLBACK. Untuk benar-benar menerapkannya:
--   (a) bagian 1 WAJIB seluruhnya 'siap';
--   (b) isi UUID operator pada bagian 3.1;
--   (c) tinjau dry run bagian 3.6 — total sasaran WAJIB 36 (24 manajer + 12 staff);
--   (d) ganti ROLLBACK di akhir dengan COMMIT.
--
-- Daftar posisi staff pada bagian 3.2 SUDAH terisi sesuai keputusan pemilik dan tidak
-- perlu diubah.
-- =====================================================================================

BEGIN;

-- 3.1 Parameter transaksi.
--     >>> GANTI UUID DI BAWAH DENGAN GUID OPERATOR YANG SEBENARNYA <<<
SELECT set_config('be_sec_014.aktor',       '00000000-0000-0000-0000-000000000000', true);
SELECT set_config('be_sec_014.dept_hr',     'Human Resource',                       true);
SELECT set_config('be_sec_014.pos_manajer', 'Manajer HR',                           true);

-- 3.2 Posisi staff HR yang DISETUJUI pemilik sistem.
--
--     DIPUTUSKAN FINAL oleh pemilik sistem, 17 September 2026 (BE-SEC-016). Gerbang yang
--     sebelumnya terbuka pada BE-SEC-014 — "daftar posisi staff HR belum disetujui" —
--     ditutup di sini. Audit baca-saja terhadap database mengonfirmasi Departemen
--     Human Resource hanya memiliki dua posisi: Manajer HR dan Staff HR.
--
--     Matriks final:
--       Human Resource x Manajer HR -> Read, Create, Update, Delete
--       Human Resource x Staff HR   -> Read, Create   (TANPA Update, TANPA Delete)
--
--     JANGAN menambah posisi lain di sini. Setiap baris tambahan adalah pemberian hak
--     yang tidak pernah disetujui pemilik.
CREATE TEMP TABLE be_sec_014_pos_staff (position_name text PRIMARY KEY) ON COMMIT DROP;

INSERT INTO be_sec_014_pos_staff (position_name)
SELECT v FROM (VALUES
    ('Staff HR')
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

-- 3.4 Gerbang: kunci bisnis HR wajib menunjuk TEPAT SATU baris masing-masing.
--
--     KENAPA PENCARIAN MEMAKAI NAMA, BUKAN CODE
--
--       Pada repository ini kunci kanonik yang benar-benar dijamin unik oleh database
--       adalah CODE, bukan nama:
--
--         MstDepartment."DepartmentCode"               -> indeks UNIQUE (tanpa filter)
--         MstPosition ("DepartmentId","PositionCode")  -> indeks UNIQUE (tanpa filter)
--         MstDepartment."DepartmentName"               -> indeks BIASA, TIDAK unik
--         MstPosition ("DepartmentId","PositionName")  -> indeks BIASA, TIDAK unik
--
--       Meski begitu, keputusan pemilik sistem dan audit baca-saja yang mendasarinya
--       sama-sama dinyatakan dalam NAMA ('Human Resource', 'Manajer HR', 'Staff HR').
--       Mengganti pencarian menjadi berbasis code berarti memakai kunci yang tidak
--       pernah ditinjau maupun disetujui pemilik. Karena itu pencarian berbasis nama
--       DIPERTAHANKAN, dan ketidakunikannya ditutup dengan cara lain: setiap pencarian
--       di bawah menegaskan TEPAT SATU baris, sehingga nama yang ambigu membatalkan
--       transaksi alih-alih diam-diam memilih salah satu baris.
--
--       Code TIDAK diabaikan: ia dibaca ulang sebagai silang-periksa dan dicetak ke
--       NOTICE supaya operator dapat mencocokkannya dengan keluaran bagian 2.1 dan 2.3
--       skrip pembacaan sebelum COMMIT. Tidak ada konvensi kunci baru yang diperkenalkan.
DO $$
DECLARE
    nama_dept   text := current_setting('be_sec_014.dept_hr', true);
    nama_pos    text := current_setting('be_sec_014.pos_manajer', true);
    jml         integer;
    kode_dept   text;
    kode_pos    text;
    rincian     text;
BEGIN
    -- (a) Departemen wajib tepat satu.
    SELECT count(*) INTO jml
      FROM public."MstDepartment"
     WHERE "DepartmentName" = nama_dept AND NOT "IsDelete";
    IF jml <> 1 THEN
        RAISE EXCEPTION 'Departemen "%" tidak menunjuk tepat satu baris (ditemukan %).',
            nama_dept, jml;
    END IF;

    SELECT "DepartmentCode" INTO kode_dept
      FROM public."MstDepartment"
     WHERE "DepartmentName" = nama_dept AND NOT "IsDelete";

    -- (b) Manajer wajib tepat satu pada departemen itu.
    SELECT count(*) INTO jml
      FROM public."MstPosition" p
      JOIN public."MstDepartment" d ON d."Id" = p."DepartmentId"
     WHERE d."DepartmentName" = nama_dept AND p."PositionName" = nama_pos AND NOT p."IsDelete";
    IF jml <> 1 THEN
        RAISE EXCEPTION 'Posisi manajer "%" pada "%" tidak menunjuk tepat satu baris (ditemukan %).',
            nama_pos, nama_dept, jml;
    END IF;

    SELECT p."PositionCode" INTO kode_pos
      FROM public."MstPosition" p
      JOIN public."MstDepartment" d ON d."Id" = p."DepartmentId"
     WHERE d."DepartmentName" = nama_dept AND p."PositionName" = nama_pos AND NOT p."IsDelete";

    -- (c) SETIAP posisi staff wajib menunjuk TEPAT SATU baris — bukan sekadar "ada".
    --     Nol baris berarti salah ejaan; lebih dari satu berarti nama ambigu. Keduanya
    --     membatalkan transaksi.
    SELECT string_agg(x.position_name || ' (ditemukan ' || x.n || ')', ', ' ORDER BY x.position_name)
      INTO rincian
      FROM (
        SELECT s.position_name,
               (SELECT count(*)
                  FROM public."MstPosition" p
                  JOIN public."MstDepartment" d ON d."Id" = p."DepartmentId"
                 WHERE d."DepartmentName" = nama_dept
                   AND p."PositionName"   = s.position_name
                   AND NOT p."IsDelete") AS n
          FROM be_sec_014_pos_staff s
      ) x
     WHERE x.n <> 1;

    IF rincian IS NOT NULL THEN
        RAISE EXCEPTION
            'Posisi staff berikut tidak menunjuk tepat satu baris pada departemen "%": %. '
            'Pakai ejaan persis dari bagian 2.1 skrip pembacaan.', nama_dept, rincian;
    END IF;

    SELECT count(*) INTO jml FROM be_sec_014_pos_staff;
    RAISE NOTICE 'Kunci bisnis lulus. Departemen "%" (code %), manajer "%" (code %), posisi staff disetujui: %.',
        nama_dept, kode_dept, nama_pos, kode_pos, jml;
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

-- 3.7 GERBANG MATRIKS PEMILIK — kardinalitas final wajib tepat 24 + 12 = 36.
--
--     Manajer HR : 6 resource x 4 action (Read/Create/Update/Delete) = 24
--     Staff HR   : 6 resource x 2 action (Read/Create)               = 12
--     TOTAL                                                          = 36
--
--     Gerbang ini juga membuktikan secara eksplisit bahwa TIDAK ADA sasaran
--     Update/Delete untuk Staff HR pada keenam resource — bukan menyimpulkannya dari
--     jumlah, melainkan menghitung langsung pasangan terlarangnya.
DO $$
DECLARE
    jml_manajer integer;
    jml_staff   integer;
    jml_total   integer;
    staff_ud    integer;
    luar_matriks integer;
    rincian     text;
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

    -- (b) Staff: tepat 12, yaitu 6 resource x Read/Create.
    SELECT count(*) INTO jml_staff
      FROM be_sec_014_target t
      JOIN be_sec_014_pos_staff s ON s.position_name = t."PositionName";

    IF jml_staff <> 12 THEN
        RAISE EXCEPTION
            'Sasaran Staff HR berjumlah %, diharapkan 12 (6 resource x Read/Create). '
            'Periksa keluaran dry-run 3.6 sebelum melanjutkan.', jml_staff;
    END IF;

    -- (c) TIDAK BOLEH ADA sasaran Update/Delete untuk posisi staff mana pun.
    SELECT count(*),
           string_agg(t."PositionName" || '.' || t.resource || '.' || t.action, ', '
                      ORDER BY t."PositionName" || '.' || t.resource || '.' || t.action)
      INTO staff_ud, rincian
      FROM be_sec_014_target t
      JOIN be_sec_014_pos_staff s ON s.position_name = t."PositionName"
     WHERE t.action IN ('Update','Delete');

    IF staff_ud <> 0 THEN
        RAISE EXCEPTION
            'DILARANG: ditemukan % sasaran Update/Delete untuk posisi staff -> %. '
            'Pemilik sistem menetapkan Staff HR hanya Read dan Create. Transaksi dibatalkan.',
            staff_ud, rincian;
    END IF;

    -- (d) Total wajib 36, dan tidak boleh ada posisi di luar manajer + staff yang disetujui.
    SELECT count(*) INTO jml_total FROM be_sec_014_target;

    IF jml_total <> 36 THEN
        RAISE EXCEPTION
            'Total sasaran berjumlah %, diharapkan 36 (24 manajer + 12 staff).', jml_total;
    END IF;

    SELECT count(*) INTO luar_matriks
      FROM be_sec_014_target t
     WHERE t."PositionName" <> current_setting('be_sec_014.pos_manajer')
       AND NOT EXISTS (SELECT 1 FROM be_sec_014_pos_staff s WHERE s.position_name = t."PositionName");

    IF luar_matriks <> 0 THEN
        RAISE EXCEPTION
            'Ditemukan % sasaran untuk posisi DI LUAR matriks yang disetujui. Transaksi dibatalkan.',
            luar_matriks;
    END IF;

    RAISE NOTICE 'Gerbang matriks pemilik lulus: 24 manajer + 12 staff = 36 sasaran; nol Update/Delete untuk staff.';
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
--     Baris yang benar-benar disisipkan direkam lewat RETURNING, supaya bagian 3.9 dapat
--     membuktikan bahwa tidak ada satu pun izin DI LUAR matriks 36 kunci yang dibuat
--     skrip ini — bukan menyimpulkannya, melainkan memeriksa daftarnya.
CREATE TEMP TABLE be_sec_014_tersisip ON COMMIT DROP AS
WITH ins AS (
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
           AND p."ActionAccessId"     = t.action_access_id)
    RETURNING "Id" AS policy_id, "DepartmentId" AS department_id, "PositionId" AS position_id,
              "ControllerAccessId" AS controller_access_id, "ActionAccessId" AS action_access_id
)
SELECT * FROM ins;

-- 3.9 Penegasan sesudah tulis — MASIH DI DALAM TRANSAKSI.
--
--     Membuktikan keadaan akhir, bukan sekadar bahwa INSERT berjalan:
--       (a) seluruh 36 sasaran efektif   (b) Manajer HR 24/24   (c) Staff HR 12/12
--       (d) Staff HR Update/Delete = 0   (f) nol baris di luar
--       (e) Finance x Manajer Finance WorkSchedule.Update/Delete = 0 — HANYA itu
--       matriks 36 kunci                 (g) nol kunci alami ganda
DO $$
DECLARE
    belum       integer;
    cov_manajer integer;
    cov_staff   integer;
    staff_ud    integer;
    finance     integer;
    ganda       integer;
    luar        integer;
    rincian     text;
BEGIN
    -- (a) seluruh sasaran wajib EFEKTIF (IsAllowed + IsActive + NOT IsDelete).
    --
    --     Ini sekaligus gerbang gagal-tertutup untuk kunci alami yang SUDAH ADA tetapi
    --     tidak aman dipakai ulang. Bagian 3.8 sengaja melewati baris yang kunci alaminya
    --     sudah ada — termasuk yang nonaktif atau soft-deleted — sehingga baris seperti itu
    --     akan muncul di sini sebagai sasaran yang belum efektif dan MEMBATALKAN transaksi.
    --     Skrip ini TIDAK PERNAH menghidupkan ulang policy yang pernah dicabut; keputusan
    --     itu milik pemilik sistem, bukan skrip.
    SELECT count(*),
           string_agg(t."PositionName" || '.' || t.resource || '.' || t.action, ', '
                      ORDER BY t."PositionName" || '.' || t.resource || '.' || t.action)
      INTO belum, rincian
      FROM be_sec_014_target t
     WHERE NOT EXISTS (
        SELECT 1 FROM public."SysAccessPolicy" p
         WHERE p."DepartmentId"       = t.department_id
           AND p."PositionId"         = t.position_id
           AND p."ControllerAccessId" = t.controller_access_id
           AND p."ActionAccessId"     = t.action_access_id
           AND p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete");

    IF belum > 0 THEN
        RAISE EXCEPTION
            '% sasaran belum efektif sesudah penyisipan -> %. Kemungkinan besar kunci '
            'alaminya sudah ada tetapi nonaktif atau sudah dihapus, sehingga 3.8 melewatinya '
            'dan skrip ini menolak menghidupkannya diam-diam. Transaksi dibatalkan.',
            belum, rincian;
    END IF;

    -- (b) Manajer HR wajib 24 / 24 efektif.
    SELECT count(*) INTO cov_manajer
      FROM be_sec_014_target t
      JOIN public."SysAccessPolicy" p
        ON p."DepartmentId"       = t.department_id
       AND p."PositionId"         = t.position_id
       AND p."ControllerAccessId" = t.controller_access_id
       AND p."ActionAccessId"     = t.action_access_id
     WHERE t."PositionName" = current_setting('be_sec_014.pos_manajer')
       AND p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete";

    IF cov_manajer <> 24 THEN
        RAISE EXCEPTION 'Cakupan Manajer HR = % / 24. Transaksi dibatalkan.', cov_manajer;
    END IF;

    -- (c) Staff HR wajib 12 / 12 efektif.
    SELECT count(*) INTO cov_staff
      FROM be_sec_014_target t
      JOIN be_sec_014_pos_staff s ON s.position_name = t."PositionName"
      JOIN public."SysAccessPolicy" p
        ON p."DepartmentId"       = t.department_id
       AND p."PositionId"         = t.position_id
       AND p."ControllerAccessId" = t.controller_access_id
       AND p."ActionAccessId"     = t.action_access_id
     WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete";

    IF cov_staff <> 12 THEN
        RAISE EXCEPTION 'Cakupan Staff HR = % / 12. Transaksi dibatalkan.', cov_staff;
    END IF;

    -- (d) Staff HR TIDAK BOLEH memegang Update/Delete efektif pada keenam resource.
    --     Dibaca dari database, bukan dari matriks sasaran: kalau hak itu ada dari sumber
    --     lain pun, transaksi ini tetap dibatalkan.
    SELECT count(*) INTO staff_ud
      FROM public."SysAccessPolicy" pol
      JOIN public."MstPosition"         p2 ON p2."Id" = pol."PositionId"
      JOIN public."MstDepartment"       d  ON d."Id"  = pol."DepartmentId"
      JOIN public."SysActionAccess"     a  ON a."Id"  = pol."ActionAccessId"
      JOIN public."SysControllerAccess" c  ON c."Id"  = a."ControllerAccessId"
      JOIN be_sec_014_pos_staff s ON s.position_name = p2."PositionName"
     WHERE d."DepartmentName" = current_setting('be_sec_014.dept_hr')
       AND c."ControllerName" IN ('WorkSchedule','Shift','ShiftGroup','ShiftPattern',
                                  'WorkCalendar','WorkScheduleAssignment')
       AND a."ActionName" IN ('Update','Delete')
       AND pol."IsAllowed" AND pol."IsActive" AND NOT pol."IsDelete";

    IF staff_ud <> 0 THEN
        RAISE EXCEPTION
            'DILARANG: Staff HR memegang % izin Update/Delete efektif pada resource HR. '
            'Pemilik sistem menetapkan Staff HR hanya Read dan Create. Transaksi dibatalkan.',
            staff_ud;
    END IF;

    -- (b) SATU-SATUNYA pembatasan Finance yang disetujui pemilik:
    --
    --       Finance x Manajer Finance, resource WorkSchedule, action Update dan Delete.
    --
    --     Itu dua hak tertidur yang ditemukan audit baca-saja dan dicabut skrip
    --     be-sec-014-pre-seeder-revoke-finance-workschedule.sql. Penegasan di sini hanya
    --     memastikan pencabutan itu memang sudah dijalankan sebelum pemberian hak HR.
    --
    --     LINGKUPNYA SENGAJA SEMPIT. Skrip ini TIDAK memeriksa, TIDAK melarang, dan TIDAK
    --     menyimpulkan apa pun tentang:
    --       - WorkSchedule.Read maupun WorkSchedule.Create milik Finance;
    --       - izin Finance pada Shift, ShiftGroup, ShiftPattern, WorkCalendar,
    --         maupun WorkScheduleAssignment;
    --       - posisi Finance selain Manajer Finance.
    --     Seluruhnya berada DI LUAR keputusan pemilik dan dibiarkan apa adanya.
    --
    --     Nama departemen dicocokkan PERSIS ('Finance'), bukan dengan ILIKE '%finance%',
    --     supaya departemen lain yang kebetulan memuat kata itu tidak ikut terjaring.
    SELECT count(*) INTO finance
      FROM public."SysAccessPolicy" pol
      JOIN public."MstDepartment"       d  ON d."Id"  = pol."DepartmentId"
      JOIN public."MstPosition"         p2 ON p2."Id" = pol."PositionId"
      JOIN public."SysActionAccess"     a  ON a."Id"  = pol."ActionAccessId"
      JOIN public."SysControllerAccess" c  ON c."Id"  = a."ControllerAccessId"
     WHERE d."DepartmentName"  = 'Finance'
       AND p2."PositionName"   = 'Manajer Finance'
       AND c."ControllerName"  = 'WorkSchedule'
       AND a."ActionName" IN ('Update','Delete')
       AND pol."IsAllowed" AND pol."IsActive" AND NOT pol."IsDelete";

    IF finance > 0 THEN
        RAISE EXCEPTION
            'Ditemukan % policy WorkSchedule.Update/Delete aktif milik Finance x Manajer Finance. '
            'Pencabutan pra-seeder belum dijalankan. Jalankan '
            'be-sec-014-pre-seeder-revoke-finance-workschedule.sql lebih dulu. '
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

    -- (f) SETIAP baris yang disisipkan skrip ini wajib berada di dalam matriks 36 kunci.
    --     Dibaca dari daftar RETURNING bagian 3.8, sehingga yang diperiksa adalah baris
    --     yang benar-benar dibuat — bukan perkiraan.
    SELECT count(*) INTO luar
      FROM be_sec_014_tersisip i
     WHERE NOT EXISTS (
        SELECT 1 FROM be_sec_014_target t
         WHERE t.department_id       = i.department_id
           AND t.position_id         = i.position_id
           AND t.controller_access_id = i.controller_access_id
           AND t.action_access_id     = i.action_access_id);

    IF luar > 0 THEN
        RAISE EXCEPTION
            'Ditemukan % baris yang disisipkan DI LUAR matriks 36 kunci yang disetujui. '
            'Transaksi dibatalkan.', luar;
    END IF;

    RAISE NOTICE 'Seluruh penegasan lulus: Manajer HR 24/24, Staff HR 12/12, Staff HR Update/Delete = 0, '
                 'Finance x Manajer Finance WorkSchedule.Update/Delete = 0, tidak ada duplikat, '
                 'dan % baris disisipkan — seluruhnya di dalam matriks 36 kunci.',
                 (SELECT count(*) FROM be_sec_014_tersisip);
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

-- 4.2 WAJIB nol baris — SATU-SATUNYA pembatasan Finance yang disetujui pemilik:
--     Finance x Manajer Finance tidak lagi memegang WorkSchedule.Update/Delete.
--
--     Query ini TIDAK memeriksa izin Finance yang lain. WorkSchedule.Read, Create, serta
--     seluruh izin Finance pada Shift, ShiftGroup, ShiftPattern, WorkCalendar, dan
--     WorkScheduleAssignment berada DI LUAR keputusan pemilik dan tidak dipersoalkan.
SELECT d."DepartmentName", p2."PositionName", c."ControllerName", a."ActionName"
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" = 'Finance'
  AND p2."PositionName"  = 'Manajer Finance'
  AND c."ControllerName" = 'WorkSchedule'
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
