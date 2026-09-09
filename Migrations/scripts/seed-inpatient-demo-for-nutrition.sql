-- =====================================================================================
-- Pasien rawat inap contoh untuk menguji modul Gizi
--
-- MENGAPA ADA
--   Basis data pengembangan belum memuat satu pun episode rawat inap aktif, sehingga
--   Daftar Pasien Gizi tampil kosong dan batch produksi tidak dapat dibuat. Skrip ini
--   menyiapkan dua pasien yang sedang dirawat agar alur Gizi dapat dijalankan.
--
-- YANG PERLU DISADARI
--   Episode ini dibuat MENEMBUS LAYAR, langsung ke basis data. Ia berguna untuk mencoba
--   alur Gizi, tetapi TIDAK membuktikan bahwa pendaftaran rawat inap lewat modul Rawat
--   Inap berfungsi. Pembuktian itu tetap harus dilakukan sendiri.
--
--   Seluruh baris berkode DEMO-RANAP sehingga mudah dikenali dan dibersihkan.
--
-- SIFAT
--   Aman diulang: Id tetap dan ON CONFLICT DO NOTHING. Menolak berjalan bila akun
--   superadmin belum ada, karena dipakai sebagai pencatat.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <database> -f seed-inpatient-demo-for-nutrition.sql
-- =====================================================================================

BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN') THEN
        RAISE EXCEPTION 'Akun superadmin tidak ditemukan; dibutuhkan sebagai pencatat.';
    END IF;
END $$;

CREATE TEMPORARY TABLE _ids ON COMMIT DROP AS
SELECT
    'f1000000-0000-4000-8000-000000000001'::uuid AS unit_ranap,
    'f1000000-0000-4000-8000-000000000002'::uuid AS room,
    'f1000000-0000-4000-8000-000000000011'::uuid AS bed_a,
    'f1000000-0000-4000-8000-000000000012'::uuid AS bed_b,
    'f2000000-0000-4000-8000-000000000001'::uuid AS patient_a,
    'f2000000-0000-4000-8000-000000000002'::uuid AS patient_b,
    'f3000000-0000-4000-8000-000000000001'::uuid AS encounter_a,
    'f3000000-0000-4000-8000-000000000002'::uuid AS encounter_b,
    'f4000000-0000-4000-8000-000000000001'::uuid AS episode_a,
    'f4000000-0000-4000-8000-000000000002'::uuid AS episode_b,
    (SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN') AS actor,
    (SELECT "Id" FROM "MstPatientClass" WHERE NOT "IsDelete" ORDER BY "PatientClassCode" LIMIT 1) AS patient_class,
    (SELECT "Id" FROM "MstDoctor" WHERE "IsActive" AND NOT "IsDelete" ORDER BY "DoctorCode" LIMIT 1) AS doctor,
    now() AT TIME ZONE 'utc' AS ts;

-- ------------------------------------------------------------------ unit dan kamar

INSERT INTO "MstServiceUnit" ("Id", "ServiceUnitCode", "ServiceUnitName",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT unit_ranap, 'DEMO-RANAP-SU', 'Rawat Inap Demo',
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- RoomType 2 = InpatientRoom.
INSERT INTO "MstRoom" ("Id", "ServiceUnitId", "RoomCode", "RoomName", "RoomType",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT room, unit_ranap, 'DEMO-RANAP-R1', 'Melati 1', 2,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstBed" ("Id", "RoomId", "BedCode", "BedName",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, i.room, v.code, v.name, i.ts, i.actor, i.actor, i.actor, i.actor, false, false
FROM _ids i, (VALUES
    ('f1000000-0000-4000-8000-000000000011'::uuid, 'DEMO-RANAP-B1', 'Bed 1'),
    ('f1000000-0000-4000-8000-000000000012'::uuid, 'DEMO-RANAP-B2', 'Bed 2')
) AS v(id, code, name)
ON CONFLICT ("Id") DO NOTHING;

-- --------------------------------------------------------------- pasien dan kunjungan

INSERT INTO "MstPatient" ("Id", "PatientCode", "MedicalRecordNumber", "FullName",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, v.code, v.rm, v.nama, i.ts, i.actor, i.actor, i.actor, i.actor, false, false
FROM _ids i, (VALUES
    ('f2000000-0000-4000-8000-000000000001'::uuid, 'DEMO-RANAP-PT1', 'RM-900001', 'Sri Wahyuni'),
    ('f2000000-0000-4000-8000-000000000002'::uuid, 'DEMO-RANAP-PT2', 'RM-900002', 'Bambang Sutrisno')
) AS v(id, code, rm, nama)
ON CONFLICT ("Id") DO NOTHING;

-- EncounterType 2 = Inpatient.
INSERT INTO "TrxPatientEncounter" ("Id", "EncounterNumber", "PatientId", "ServiceUnitId",
    "EncounterDate", "EncounterType", "RegisteredAt", "RegisteredByUserId",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, v.nomor, v.pasien, i.unit_ranap, i.ts, 2, i.ts, i.actor,
    i.ts, i.actor, i.actor, i.actor, i.actor, false, false
FROM _ids i, (VALUES
    ('f3000000-0000-4000-8000-000000000001'::uuid, 'DEMO-RANAP-ENC1', 'f2000000-0000-4000-8000-000000000001'::uuid),
    ('f3000000-0000-4000-8000-000000000002'::uuid, 'DEMO-RANAP-ENC2', 'f2000000-0000-4000-8000-000000000002'::uuid)
) AS v(id, nomor, pasien)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------------ episode rawat inap

-- EpisodeStatus 1 = Admitted; inilah yang membuat pasien masuk Daftar Pasien Gizi.
INSERT INTO "InpEpisode" ("Id", "EpisodeNumber", "EncounterId", "PatientId", "ServiceUnitId",
    "PatientClassId", "EpisodeStatus", "AdmittedAt", "RequiresIsolation", "DischargeType",
    "IsClosedWithoutFinancialClearance", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, v.nomor, v.encounter, v.pasien, i.unit_ranap, i.patient_class, 1,
    i.ts - interval '1 day', false, 0, false, true,
    i.ts, i.actor, i.actor, i.actor, i.actor, false, false
FROM _ids i, (VALUES
    ('f4000000-0000-4000-8000-000000000001'::uuid, 'DEMO-RANAP-EP1',
     'f3000000-0000-4000-8000-000000000001'::uuid, 'f2000000-0000-4000-8000-000000000001'::uuid),
    ('f4000000-0000-4000-8000-000000000002'::uuid, 'DEMO-RANAP-EP2',
     'f3000000-0000-4000-8000-000000000002'::uuid, 'f2000000-0000-4000-8000-000000000002'::uuid)
) AS v(id, nomor, encounter, pasien)
ON CONFLICT ("Id") DO NOTHING;

-- EndDateTime dibiarkan kosong; itulah yang menandai penempatan masih berjalan.
INSERT INTO "InpBedPlacement" ("Id", "EpisodeId", "BedId", "RoomId", "ServiceUnitId",
    "PatientClassId", "SequenceNumber", "StartDateTime", "PlacedByUserId", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, v.episode, v.bed, i.room, i.unit_ranap, i.patient_class, 1,
    i.ts - interval '1 day', i.actor, true,
    i.ts, i.actor, i.actor, i.actor, i.actor, false, false
FROM _ids i, (VALUES
    ('f5000000-0000-4000-8000-000000000001'::uuid,
     'f4000000-0000-4000-8000-000000000001'::uuid, 'f1000000-0000-4000-8000-000000000011'::uuid),
    ('f5000000-0000-4000-8000-000000000002'::uuid,
     'f4000000-0000-4000-8000-000000000002'::uuid, 'f1000000-0000-4000-8000-000000000012'::uuid)
) AS v(id, episode, bed)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "InpDoctorAssignment" ("Id", "EpisodeId", "DoctorId", "SequenceNumber",
    "StartDateTime", "AssignedByUserId", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, v.episode, i.doctor, 1, i.ts - interval '1 day', i.actor, true,
    i.ts, i.actor, i.actor, i.actor, i.actor, false, false
FROM _ids i, (VALUES
    ('f6000000-0000-4000-8000-000000000001'::uuid, 'f4000000-0000-4000-8000-000000000001'::uuid),
    ('f6000000-0000-4000-8000-000000000002'::uuid, 'f4000000-0000-4000-8000-000000000002'::uuid)
) AS v(id, episode)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;

-- =====================================================================================
-- Ringkasan: persis yang akan tampil di Daftar Pasien Gizi
-- =====================================================================================

SELECT p."FullName" AS pasien, p."MedicalRecordNumber" AS no_rm,
       r."RoomName" AS ruang, b."BedName" AS bed, d."FullName" AS dpjp,
       e."EpisodeStatus" AS status
FROM "InpEpisode" e
JOIN "MstPatient" p ON p."Id" = e."PatientId"
LEFT JOIN "InpBedPlacement" pl ON pl."EpisodeId" = e."Id" AND pl."EndDateTime" IS NULL AND NOT pl."IsDelete"
LEFT JOIN "MstRoom" r ON r."Id" = pl."RoomId"
LEFT JOIN "MstBed" b ON b."Id" = pl."BedId"
LEFT JOIN "InpDoctorAssignment" da ON da."EpisodeId" = e."Id" AND da."EndDateTime" IS NULL AND NOT da."IsDelete"
LEFT JOIN "MstDoctor" d ON d."Id" = da."DoctorId"
WHERE e."EpisodeStatus" IN (1, 2) AND NOT e."IsDelete"
ORDER BY r."RoomName", b."BedName";
