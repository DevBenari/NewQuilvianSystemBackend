-- =====================================================================================
-- Akun login demo tim kamar operasi
--
-- Membuat empat akun: dokter bedah, dokter anestesi, perawat instrumen, dan perawat
-- sirkuler. Masing-masing tertaut ke profil tenaganya sendiri, sehingga sign-off tiga
-- peran dapat diuji sebagaimana mestinya.
--
-- SIFAT
--   Dijalankan manual. Aman diulang: memakai Id tetap dan ON CONFLICT DO NOTHING,
--   sehingga menjalankannya dua kali tidak menggandakan dan tidak menimpa apa pun.
--
-- PRASYARAT
--   seed-operating-room-starter-data.sql harus sudah dijalankan lebih dulu, karena
--   profil tenaga WFP-00001 sampai WFP-00004 dan kedua baris dokter berasal dari sana.
--   Skrip ini berhenti dengan pesan jelas bila prasyarat itu belum ada.
--
-- KATA SANDI
--   Password hash disalin dari akun Rendi Pangalila, sesuai permintaan. Tidak ada kata
--   sandi yang ditulis di dalam berkas ini.
--
--   Akun yang sudah ada sebelumnya IKUT diselaraskan kata sandinya. Tanpa itu skrip
--   tampak berhasil padahal kata sandinya masih yang lama, karena INSERT di bawah
--   melewati baris yang sudah ada.
--
--   Konsekuensinya: keempat akun demo memakai kata sandi yang sama persis dengan milik
--   Rendi. Bila kata sandi demo ini dibagikan kepada orang lain, kata sandi akun Rendi
--   ikut diketahui. Ganti kata sandi akun-akun ini lewat aplikasi bila kelak dipakai
--   lebih dari sekadar pengujian di mesin sendiri.
--
-- HAK AKSES
--   Keempat akun TIDAK diberi peran SuperAdmin. UserType-nya diisi sesuai profesinya,
--   dan izinnya kelak datang dari SysAccessPolicy per Departemen dan Jabatan.
--
--   Selama pemetaan izin itu belum dibuat, akun-akun ini hanya dapat membuka halaman bila
--   Security:Authorization:Enabled bernilai false. Itu memang keadaan sementara yang
--   sedang berlaku; begitu izin per jabatan ditetapkan, nyalakan kembali saklarnya.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f seed-operating-room-demo-users.sql
-- =====================================================================================

BEGIN;

-- Berhenti bila akun sumber kata sandi tidak ditemukan atau ternyata lebih dari satu,
-- supaya tidak ada akun yang diam-diam memakai kata sandi orang yang keliru.
DO $$
DECLARE
    jumlah integer;
BEGIN
    SELECT COUNT(*) INTO jumlah
    FROM "AspNetUsers"
    WHERE "DisplayName" ILIKE '%rendi%pangalila%'
       OR "UserName"    ILIKE '%rendi%pangalila%'
       OR "Email"       ILIKE '%rendi%pangalila%';

    IF jumlah = 0 THEN
        RAISE EXCEPTION
            'Akun Rendi Pangalila tidak ditemukan. Periksa ejaan nama pada AspNetUsers, atau ubah pola pencarian di skrip ini.';
    END IF;

    IF jumlah > 1 THEN
        RAISE EXCEPTION
            'Ditemukan % akun yang cocok dengan Rendi Pangalila. Persempit pencariannya agar tidak salah menyalin kata sandi.', jumlah;
    END IF;
END $$;

-- Berhenti bila profil tenaga belum ada, karena tanpa tautan itu akun yang terbentuk
-- tidak akan dapat memberi sign-off apa pun.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "MstWorkforceProfile" WHERE "ProfileCode" = 'WFP-00001') THEN
        RAISE EXCEPTION
            'Profil tenaga WFP-00001 belum ada. Jalankan seed-operating-room-starter-data.sql lebih dulu.';
    END IF;
END $$;

CREATE TEMPORARY TABLE _src ON COMMIT DROP AS
SELECT
    u."PasswordHash"                    AS password_hash,
    now() AT TIME ZONE 'utc'            AS ts
FROM "AspNetUsers" u
WHERE u."DisplayName" ILIKE '%rendi%pangalila%'
   OR u."UserName"    ILIKE '%rendi%pangalila%'
   OR u."Email"       ILIKE '%rendi%pangalila%';

-- Empat akun. UserType 3 = PermanentDoctor, 2 = Employee.
CREATE TEMPORARY TABLE _akun ON COMMIT DROP AS
SELECT * FROM (VALUES
    ('d1000000-0000-4000-8000-000000000001'::uuid, 'opr.dokter',   'USR-00201',
     'dr. Andi Prasetya, Sp.B',    3, 'WFP-00001', 'DR-00001'),
    ('d1000000-0000-4000-8000-000000000002'::uuid, 'opr.anestesi', 'USR-00202',
     'dr. Sinta Rahmawati, Sp.An', 3, 'WFP-00002', 'DR-00002'),
    ('d1000000-0000-4000-8000-000000000003'::uuid, 'opr.perawat',  'USR-00203',
     'Ns. Budi Santoso',           2, 'WFP-00003', NULL),
    ('d1000000-0000-4000-8000-000000000004'::uuid, 'opr.perawat2', 'USR-00204',
     'Ns. Dewi Lestari',           2, 'WFP-00004', NULL)
) AS t(id, user_name, user_code, display_name, user_type, profile_code, doctor_code);

INSERT INTO "AspNetUsers" (
    "Id", "UserCode", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
    "DisplayName", "UserType", "WorkforceProfileId", "DoctorId", "IsActive",
    "MustChangePassword", "IsGeolocationBypassEnabled",
    "IsFingerprintRegistrationEnabled", "CreateDateTime")
SELECT
    a.id, a.user_code, a.user_name, UPPER(a.user_name),
    a.user_name || '@rs.example.id', UPPER(a.user_name || '@rs.example.id'), true,
    s.password_hash, gen_random_uuid()::text, gen_random_uuid()::text,
    false, false, true, 0,
    a.display_name, a.user_type,
    w."Id",
    d."Id",
    true, false, false, false, s.ts
FROM _akun a
CROSS JOIN _src s
JOIN "MstWorkforceProfile" w ON w."ProfileCode" = a.profile_code
LEFT JOIN "MstDoctor" d      ON d."DoctorCode" = a.doctor_code
-- Tanpa target kolom, supaya bentrok pada indeks unik mana pun ikut dilewati,
-- termasuk bila akun dengan nama yang sama sudah pernah dibuat seeder.
ON CONFLICT DO NOTHING;

-- Menyelaraskan kata sandi akun yang SUDAH ADA sebelum skrip ini dijalankan.
--
-- Ini diperlukan karena INSERT di atas memakai ON CONFLICT DO NOTHING: akun yang sudah
-- dibuat lebih dulu — misalnya oleh OperatingRoomDemoSeeder dengan kata sandi SuperAdmin —
-- dilewati seluruhnya, termasuk kata sandinya. Tanpa langkah ini, skrip tampak berhasil
-- tetapi kata sandinya bukan yang diharapkan, dan login gagal tanpa keterangan.
--
-- SecurityStamp ikut diganti supaya token dan cookie lama untuk akun-akun ini tidak lagi
-- berlaku, sebagaimana perilaku ASP.NET Identity saat kata sandi berubah.
--
-- Hanya menyentuh empat akun demo yang namanya tercantum pada _akun. Akun lain, termasuk
-- akun sumber kata sandinya sendiri, tidak pernah diubah.
UPDATE "AspNetUsers" u
SET "PasswordHash"    = s.password_hash,
    "SecurityStamp"   = gen_random_uuid()::text,
    "ConcurrencyStamp" = gen_random_uuid()::text
FROM _akun a
CROSS JOIN _src s
WHERE UPPER(a.user_name) = u."NormalizedUserName";

-- Melengkapi tautan pada akun yang sudah terlanjur ada sebelum skrip ini dijalankan.
-- Hanya kolom yang masih kosong yang diisi; yang sudah terisi tidak pernah ditimpa.
UPDATE "AspNetUsers" u
SET "WorkforceProfileId" = w."Id"
FROM _akun a
JOIN "MstWorkforceProfile" w ON w."ProfileCode" = a.profile_code
WHERE UPPER(a.user_name) = u."NormalizedUserName"
  AND u."WorkforceProfileId" IS NULL;

UPDATE "AspNetUsers" u
SET "DoctorId" = d."Id"
FROM _akun a
JOIN "MstDoctor" d ON d."DoctorCode" = a.doctor_code
WHERE UPPER(a.user_name) = u."NormalizedUserName"
  AND a.doctor_code IS NOT NULL
  AND u."DoctorId" IS NULL;

COMMIT;

-- =====================================================================================
-- Ringkasan
-- =====================================================================================

SELECT
    u."UserName",
    u."DisplayName",
    CASE u."UserType" WHEN 2 THEN 'Employee' WHEN 3 THEN 'PermanentDoctor'
         WHEN 1 THEN 'SuperAdmin' ELSE u."UserType"::text END      AS user_type,
    w."DisplayName"                                                AS profil_tenaga,
    COALESCE(d."FullName", '-')                                    AS tertaut_dokter,
    CASE WHEN u."WorkforceProfileId" IS NULL
         THEN 'BELUM dapat memberi sign-off'
         ELSE 'siap memberi sign-off sesuai perannya' END           AS keterangan
FROM "AspNetUsers" u
LEFT JOIN "MstWorkforceProfile" w ON w."Id" = u."WorkforceProfileId"
LEFT JOIN "MstDoctor" d           ON d."Id" = u."DoctorId"
WHERE u."NormalizedUserName" IN
    ('OPR.DOKTER', 'OPR.ANESTESI', 'OPR.PERAWAT', 'OPR.PERAWAT2')
ORDER BY u."UserName";
