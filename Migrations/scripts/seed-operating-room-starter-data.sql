-- =====================================================================================
-- Data awal modul Operasi
--
-- Menyiapkan rantai data yang diwajibkan modul Operasi: tenaga, kamar operasi, pasien
-- beserta kunjungan dan tindakan bedahnya, consent, bahan habis pakai, serta dua akun
-- login untuk dokter anestesi dan perawat.
--
-- SIFAT
--   Dijalankan manual. Tidak ikut migration dan tidak ikut startup aplikasi, sehingga
--   tidak pernah masuk ke lingkungan mana pun tanpa seseorang menjalankannya sendiri.
--
--   Aman diulang. Seluruh perintah memakai ON CONFLICT DO NOTHING dengan Id tetap,
--   sehingga menjalankannya dua kali tidak menggandakan apa pun dan tidak menimpa baris
--   yang sudah disunting orang.
--
-- YANG PERLU DIKETAHUI SEBELUM MENJALANKAN
--   Isi data ini disusun agar tampak seperti data sungguhan: "Kamar Operasi 1",
--   "Kasa Steril", dan seterusnya. Konsekuensinya baris-baris ini TIDAK dapat dibedakan
--   dari data rumah sakit yang asli, baik oleh orang maupun oleh query. Jangan
--   menjalankannya pada basis data yang memuat data pasien sungguhan.
--
--   Susunan kamar operasi, daftar bahan, dan identitas tenaga di sini bukan berasal dari
--   rumah sakit. Ganti isinya dengan data yang sebenarnya sebelum dipakai melayani pasien.
--
-- AKUN YANG DIBUAT
--   opr.anestesi   dokter anestesi
--   opr.perawat    perawat kamar operasi
--
--   Keduanya memakai kata sandi yang SAMA PERSIS dengan akun superadmin, karena
--   password hash-nya disalin dari baris superadmin. Tidak ada kata sandi baru yang perlu
--   diingat, dan tidak ada kata sandi yang ditulis di dalam berkas ini.
--
--   Keduanya diberi peran SuperAdmin supaya lolos pemeriksaan izin tanpa menyiapkan
--   pemetaan role lebih dulu. Itu memadai untuk pengembangan; untuk pemakaian sungguhan
--   berikan izin lewat role masing-masing profesi, bukan dengan menjadikannya super admin.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f seed-operating-room-starter-data.sql
-- =====================================================================================

BEGIN;

-- Berhenti lebih awal bila akun superadmin tidak ada, karena password hash dan
-- pencatat audit diambil dari sana.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN') THEN
        RAISE EXCEPTION
            'Akun superadmin tidak ditemukan. Jalankan aplikasi sekali agar SuperAdminSeeder membuatnya, lalu ulangi skrip ini.';
    END IF;
END $$;

-- Id tetap. Nilainya sengaja mudah dikenali saat menelusuri data.
-- a1... tenaga dan organisasi | b1... pelayanan | c1... transaksi pasien
CREATE TEMPORARY TABLE _ids ON COMMIT DROP AS
SELECT
    'a1000000-0000-4000-8000-000000000001'::uuid AS workforce_type,
    'a1000000-0000-4000-8000-000000000002'::uuid AS employee_category,
    'a1000000-0000-4000-8000-000000000003'::uuid AS employment_type,
    'a1000000-0000-4000-8000-000000000004'::uuid AS employment_status,
    'a1000000-0000-4000-8000-000000000005'::uuid AS profession_doctor,
    'a1000000-0000-4000-8000-000000000006'::uuid AS profession_nurse,
    'a1000000-0000-4000-8000-000000000010'::uuid AS department,
    'a1000000-0000-4000-8000-000000000011'::uuid AS position_doctor,
    'a1000000-0000-4000-8000-000000000012'::uuid AS position_nurse,
    'a1000000-0000-4000-8000-000000000020'::uuid AS wfp_surgeon,
    'a1000000-0000-4000-8000-000000000021'::uuid AS wfp_anesthetist,
    'a1000000-0000-4000-8000-000000000022'::uuid AS wfp_scrub,
    'a1000000-0000-4000-8000-000000000023'::uuid AS wfp_circulating,
    'a1000000-0000-4000-8000-000000000030'::uuid AS doctor_surgeon,
    'a1000000-0000-4000-8000-000000000031'::uuid AS doctor_anesthetist,
    'a1000000-0000-4000-8000-000000000040'::uuid AS employee_scrub,
    'a1000000-0000-4000-8000-000000000041'::uuid AS employee_circulating,
    'b1000000-0000-4000-8000-000000000001'::uuid AS unit_ok,
    'b1000000-0000-4000-8000-000000000002'::uuid AS unit_ward,
    'b1000000-0000-4000-8000-000000000010'::uuid AS room_ok1,
    'b1000000-0000-4000-8000-000000000020'::uuid AS drug_category,
    'b1000000-0000-4000-8000-000000000021'::uuid AS drug_gauze,
    'b1000000-0000-4000-8000-000000000022'::uuid AS drug_mesh,
    'b1000000-0000-4000-8000-000000000030'::uuid AS procedure_appendectomy,
    'b1000000-0000-4000-8000-000000000031'::uuid AS procedure_herniotomy,
    'c1000000-0000-4000-8000-000000000001'::uuid AS patient,
    'c1000000-0000-4000-8000-000000000002'::uuid AS encounter,
    'c1000000-0000-4000-8000-000000000003'::uuid AS queue,
    'c1000000-0000-4000-8000-000000000004'::uuid AS consultation,
    'c1000000-0000-4000-8000-000000000005'::uuid AS patient_procedure_a,
    'c1000000-0000-4000-8000-000000000006'::uuid AS patient_procedure_b,
    'c1000000-0000-4000-8000-000000000007'::uuid AS consent_surgery,
    'c1000000-0000-4000-8000-000000000008'::uuid AS consent_anesthesia,
    'c1000000-0000-4000-8000-000000000010'::uuid AS user_anesthetist,
    'c1000000-0000-4000-8000-000000000011'::uuid AS user_nurse,
    (SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN') AS actor,
    now() AT TIME ZONE 'utc' AS ts;

-- =====================================================================================
-- 1. Klasifikasi ketenagakerjaan
-- =====================================================================================

INSERT INTO "MstWorkforceType" (
    "Id", "WorkforceTypeCode", "WorkforceTypeName", "IsInternal", "IsClinical",
    "SortOrder", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy",
    "CancelBy", "IsCancel", "IsDelete")
SELECT workforce_type, 'TNG-MEDIS', 'Tenaga Medis', true, true, 1, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstEmployeeCategory" (
    "Id", "EmployeeCategoryCode", "EmployeeCategoryName", "IsClinical",
    "RequiresCredentialing", "SortOrder", "IsActive", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT employee_category, 'KAT-KLINIS', 'Tenaga Klinis', true, true, 1, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstEmploymentType" (
    "Id", "EmploymentTypeCode", "EmploymentTypeName", "IsPermanent", "IsContractBased",
    "RequiresContractEndDate", "IsPayrollEligible", "IsBenefitEligible", "SortOrder",
    "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
    "IsCancel", "IsDelete")
SELECT employment_type, 'PEG-TETAP', 'Pegawai Tetap', true, false, false, true, true, 1,
    true, ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstEmploymentStatus" (
    "Id", "EmploymentStatusCode", "EmploymentStatusName", "IsActiveEmployment",
    "IsSchedulable", "IsPayrollEligible", "IsTerminalStatus", "SortOrder", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT employment_status, 'STS-AKTIF', 'Aktif', true, true, true, false, 1, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstProfession" (
    "Id", "ProfessionCode", "ProfessionName", "ProfessionGroup", "IsClinicalProfession",
    "RequiresCredentialing", "RequiresLicense", "IsActive", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT profession_doctor, 'PRF-DOKTER', 'Dokter', 'Medis', true, true, true, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstProfession" (
    "Id", "ProfessionCode", "ProfessionName", "ProfessionGroup", "IsClinicalProfession",
    "RequiresCredentialing", "RequiresLicense", "IsActive", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT profession_nurse, 'PRF-PERAWAT', 'Perawat', 'Keperawatan', true, true, true, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 2. Organisasi
-- =====================================================================================

INSERT INTO "MstDepartment" (
    "Id", "DepartmentCode", "DepartmentName", "IsActive", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT department, 'DEP-OK', 'Instalasi Bedah Sentral', true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstPosition" (
    "Id", "DepartmentId", "PositionCode", "PositionName", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT position_doctor, department, 'POS-DOKTER-OK', 'Dokter Kamar Operasi', true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstPosition" (
    "Id", "DepartmentId", "PositionCode", "PositionName", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT position_nurse, department, 'POS-PERAWAT-OK', 'Perawat Kamar Operasi', true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 3. Profil tenaga
--
-- UserType 3 = PermanentDoctor, 2 = Employee.
-- =====================================================================================

INSERT INTO "MstWorkforceProfile" (
    "Id", "ProfileCode", "UserType", "DisplayName", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT wfp_surgeon, 'WFP-00001', 3, 'dr. Andi Prasetya, Sp.B', true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstWorkforceProfile" (
    "Id", "ProfileCode", "UserType", "DisplayName", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT wfp_anesthetist, 'WFP-00002', 3, 'dr. Sinta Rahmawati, Sp.An', true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstWorkforceProfile" (
    "Id", "ProfileCode", "UserType", "DisplayName", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT wfp_scrub, 'WFP-00003', 2, 'Ns. Budi Santoso', true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstWorkforceProfile" (
    "Id", "ProfileCode", "UserType", "DisplayName", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT wfp_circulating, 'WFP-00004', 2, 'Ns. Dewi Lestari', true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 4. Dokter
--
-- Religion, MaritalStatus, BloodType, PracticeType, CredentialingStatus, dan
-- ClinicalPrivilegeStatus diisi 0 atau 1 sebagai nilai netral; ubah bila datanya diketahui.
-- =====================================================================================

INSERT INTO "MstDoctor" (
    "Id", "WorkforceProfileId", "DoctorCode", "DoctorNumber", "FullName",
    "Religion", "MaritalStatus", "BloodType", "WorkforceTypeId", "EmployeeCategoryId",
    "EmploymentTypeId", "EmploymentStatusId", "ProfessionId", "PracticeType",
    "CredentialingStatus", "ClinicalPrivilegeStatus", "IsAvailableForAppointment",
    "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
    "IsCancel", "IsDelete")
SELECT doctor_surgeon, wfp_surgeon, 'DR-00001', 'SIP-00001', 'dr. Andi Prasetya, Sp.B',
    0, 0, 0, workforce_type, employee_category, employment_type, employment_status,
    profession_doctor, 1, 0, 0, true, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstDoctor" (
    "Id", "WorkforceProfileId", "DoctorCode", "DoctorNumber", "FullName",
    "Religion", "MaritalStatus", "BloodType", "WorkforceTypeId", "EmployeeCategoryId",
    "EmploymentTypeId", "EmploymentStatusId", "ProfessionId", "PracticeType",
    "CredentialingStatus", "ClinicalPrivilegeStatus", "IsAvailableForAppointment",
    "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
    "IsCancel", "IsDelete")
SELECT doctor_anesthetist, wfp_anesthetist, 'DR-00002', 'SIP-00002',
    'dr. Sinta Rahmawati, Sp.An',
    0, 0, 0, workforce_type, employee_category, employment_type, employment_status,
    profession_doctor, 1, 0, 0, true, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 5. Perawat
--
-- IdentityNumber dibatasi 16 karakter, mengikuti panjang NIK.
-- =====================================================================================

INSERT INTO "MstEmployee" (
    "Id", "WorkforceProfileId", "EmployeeCode", "EmployeeNumber", "FullName", "BirthDate",
    "Religion", "MaritalStatus", "BloodType", "IdentityType", "IdentityNumber", "Email",
    "PrimaryDepartmentId", "PrimaryPositionId", "WorkforceTypeId", "EmployeeCategoryId",
    "EmploymentTypeId", "EmploymentStatusId", "JoinDate", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT employee_scrub, wfp_scrub, 'PEG-00001', 'NIP-00001', 'Ns. Budi Santoso',
    DATE '1990-03-14', 0, 0, 0, 'KTP', '3273010301900001',
    'budi.santoso@rs.example.id', department, position_nurse, workforce_type,
    employee_category, employment_type, employment_status, ts, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstEmployee" (
    "Id", "WorkforceProfileId", "EmployeeCode", "EmployeeNumber", "FullName", "BirthDate",
    "Religion", "MaritalStatus", "BloodType", "IdentityType", "IdentityNumber", "Email",
    "PrimaryDepartmentId", "PrimaryPositionId", "WorkforceTypeId", "EmployeeCategoryId",
    "EmploymentTypeId", "EmploymentStatusId", "JoinDate", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT employee_circulating, wfp_circulating, 'PEG-00002', 'NIP-00002', 'Ns. Dewi Lestari',
    DATE '1992-07-22', 0, 0, 0, 'KTP', '3273016207920002',
    'dewi.lestari@rs.example.id', department, position_nurse, workforce_type,
    employee_category, employment_type, employment_status, ts, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 6. Unit layanan dan kamar operasi
--
-- RoomType 8 = OperatingRoom. Penjadwalan menyaring TEPAT pada nilai ini; kamar bertipe
-- lain ditolak dengan pesan "Ruang operasi tidak ditemukan atau tidak aktif" walaupun
-- kamarnya ada dan aktif.
-- =====================================================================================

INSERT INTO "MstServiceUnit" (
    "Id", "ServiceUnitCode", "ServiceUnitName", "ServiceUnitType",
    "IsAvailableForRegistration", "IsAvailableForKiosk", "IsAvailableForAppointment",
    "IsQueueRequired", "IsDoctorRequired", "IsScreeningRequired", "SortOrder", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT unit_ok, 'UNIT-OK', 'Instalasi Bedah Sentral', 2,
    false, false, false, false, true, false, 1, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstServiceUnit" (
    "Id", "ServiceUnitCode", "ServiceUnitName", "ServiceUnitType",
    "IsAvailableForRegistration", "IsAvailableForKiosk", "IsAvailableForAppointment",
    "IsQueueRequired", "IsDoctorRequired", "IsScreeningRequired", "SortOrder", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT unit_ward, 'UNIT-RANAP-BEDAH', 'Rawat Inap Bedah', 2,
    false, false, false, false, true, false, 2, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstRoom" (
    "Id", "ServiceUnitId", "RoomCode", "RoomName", "RoomType", "Capacity",
    "IsForMale", "IsForFemale", "IsForNewborn", "IsIsolationRoom", "IsIntensiveCare",
    "IsOdcRoom", "IsAvailableForAdmission", "SortOrder", "IsActive", "CreateDateTime",
    "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT room_ok1, unit_ok, 'OK-01', 'Kamar Operasi 1', 8, 1,
    true, true, false, false, false, false, false, 1, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 7. Bahan habis pakai
--
-- Modul Operasi membaca item material dari master farmasi, bukan dari master miliknya.
-- =====================================================================================

INSERT INTO "MstDrugCategory" (
    "Id", "DrugCategoryCode", "DrugCategoryName", "DrugCategoryType", "IsAntibiotic",
    "IsNarcotic", "IsPsychotropic", "IsHighAlert", "IsChronicDiseaseDrug", "IsVaccine",
    "IsConsumable", "SortOrder", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy",
    "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT drug_category, 'KAT-BHP', 'Bahan Habis Pakai Bedah', 'Consumable',
    false, false, false, false, false, false, true, 1, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstDrug" (
    "Id", "DrugCategoryId", "DrugCode", "DrugName", "IsFormulary", "IsGeneric",
    "IsAntibiotic", "IsNarcotic", "IsPsychotropic", "IsHighAlert", "IsChronicDiseaseDrug",
    "IsVaccine", "IsConsumable", "IsCompoundIngredientAllowed", "IsStockManaged",
    "IsBatchTracked", "IsExpiryDateTracked", "IsAllowFractionalDispense",
    "IsNeedPrescription", "IsPrescribable", "IsNeedApproval", "SortOrder", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT drug_gauze, drug_category, 'BHP-00001', 'Kasa Steril 10x10 cm',
    true, true, false, false, false, false, false, false, true, false, true, true, true,
    false, false, false, false, 1, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstDrug" (
    "Id", "DrugCategoryId", "DrugCode", "DrugName", "IsFormulary", "IsGeneric",
    "IsAntibiotic", "IsNarcotic", "IsPsychotropic", "IsHighAlert", "IsChronicDiseaseDrug",
    "IsVaccine", "IsConsumable", "IsCompoundIngredientAllowed", "IsStockManaged",
    "IsBatchTracked", "IsExpiryDateTracked", "IsAllowFractionalDispense",
    "IsNeedPrescription", "IsPrescribable", "IsNeedApproval", "SortOrder", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT drug_mesh, drug_category, 'IMP-00001', 'Mesh Hernia Polypropylene',
    true, false, false, false, false, false, false, false, true, false, true, true, true,
    false, false, false, true, 2, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 8. Master tindakan
--
-- IsSurgery menandai tindakan sebagai tindakan bedah pada master.
-- =====================================================================================

INSERT INTO "MstProcedure" (
    "Id", "ProcedureCode", "ProcedureName", "ProcedureType", "IsDoctorAction",
    "IsNursingAction", "IsSurgery", "IsLaboratory", "IsRadiology", "IsTherapy",
    "IsNeedDoctor", "IsNeedApproval", "IsCoveredByInsuranceDefault",
    "IsAvailableForOutpatient", "IsAvailableForInpatient", "IsAvailableForEmergency",
    "EstimatedDurationMinutes", "SortOrder", "IsActive", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT procedure_appendectomy, 'TND-00001', 'Apendektomi', 'Surgery',
    true, false, true, false, false, false, true, false, true, false, true, true,
    60, 1, true, ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MstProcedure" (
    "Id", "ProcedureCode", "ProcedureName", "ProcedureType", "IsDoctorAction",
    "IsNursingAction", "IsSurgery", "IsLaboratory", "IsRadiology", "IsTherapy",
    "IsNeedDoctor", "IsNeedApproval", "IsCoveredByInsuranceDefault",
    "IsAvailableForOutpatient", "IsAvailableForInpatient", "IsAvailableForEmergency",
    "EstimatedDurationMinutes", "SortOrder", "IsActive", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT procedure_herniotomy, 'TND-00002', 'Herniotomi', 'Surgery',
    true, false, true, false, false, false, true, false, true, false, true, true,
    90, 2, true, ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 9. Pasien beserta kunjungan dan tindakannya
--
-- RegisteredByUserId ber-foreign key ke AspNetUsers dan tidak boleh kosong.
-- =====================================================================================

INSERT INTO "MstPatient" (
    "Id", "PatientCode", "MedicalRecordNumber", "PatientType", "PatientStatus",
    "RegistrationSource", "FullName", "Religion", "MaritalStatus", "BloodType",
    "IsMember", "IsNewborn", "IsDeceased", "IsActive", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT patient, 'PSN-00001', 'RM-000001', 1, 1, 2, 'Slamet Riyadi', 0, 0, 0,
    false, false, false, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "TrxPatientEncounter" (
    "Id", "EncounterNumber", "PatientId", "ServiceUnitId", "EncounterDate",
    "EncounterType", "VisitType", "RegistrationSource", "EncounterStatus", "PaymentType",
    "IsReferral", "IsReferralRequired", "IsReferralVerified", "IsNewPatient",
    "IsFromKiosk", "IsWalkIn", "IsAppointment", "IsScreeningRequired", "IsQueueRequired",
    "IsDoctorRequired", "RegisteredAt", "RegisteredByUserId", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT encounter, 'KJG-000001', patient, unit_ok, ts, 2, 1, 2, 1, 1,
    false, false, false, true, false, true, false, false, true, true,
    ts, actor, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "TrxQueue" (
    "Id", "EncounterId", "PatientId", "ServiceUnitId", "QueueDate", "QueueNumber",
    "QueueCode", "QueueStatus", "NurseCallAttemptCount", "DoctorCallAttemptCount",
    "SkipCount", "RequeueCount", "IsPriorityQueue", "IsFromKiosk", "IsWalkIn",
    "IsAppointment", "IsScreeningRequired", "IsDoctorRequired", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT queue, encounter, patient, unit_ok, ts, 1, 'A-001', 1, 0, 0, 0, 0,
    false, false, true, false, false, true, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "TrxDoctorConsultation" (
    "Id", "ConsultationNumber", "EncounterId", "QueueId", "PatientId", "DoctorId",
    "ServiceUnitId", "ConsultationDateTime", "ConsultationStatus",
    "IsVitalSignCopiedFromAssessment", "DiagnosisCount", "HasPrimaryDiagnosis",
    "ProcedureCount", "HasProcedure", "PrescriptionCount", "HasPrescription",
    "SupportingOrderCount", "HasSupportingOrder", "MedicalCertificateCount",
    "ClinicalDocumentCount", "ConsentCount", "IsActive", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT consultation, 'KSL-000001', encounter, queue, patient, doctor_surgeon, unit_ok,
    ts, 1, true, 0, false, 2, true, 0, false, 0, false, 0, 0, 2, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- IsSurgeryRelated WAJIB true. Tanpa itu tindakan tidak muncul sebagai pilihan saat
-- membuat kasus operasi, dan pesannya hanya berbunyi "Tindakan tidak ditemukan, tidak
-- aktif, atau bukan tindakan operasi."
INSERT INTO "TrxPatientProcedure" (
    "Id", "EncounterId", "ConsultationId", "PatientId", "DoctorId", "ServiceUnitId",
    "ProcedureId", "ProcedureCodeSnapshot", "ProcedureNameSnapshot", "ProcedureMasterType",
    "IsFromMasterProcedure", "IsPrimaryProcedure", "IsEmergencyProcedure",
    "IsSurgeryRelated", "IsPackageProcedure", "ProcedureSource", "ProcedureStatus",
    "ProcedureDateTime", "Quantity", "UnitPrice", "TotalPrice", "IsFreeOfCharge",
    "IsBillable", "IsCoveredByInsurance", "CoverageStatus", "CoveragePercent",
    "CoveredAmount", "PatientPayAmount", "IsNeedApproval", "IsApproved", "IsExecuted",
    "IsBillingGenerated", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy",
    "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT patient_procedure_a, encounter, consultation, patient, doctor_surgeon, unit_ok,
    procedure_appendectomy, 'TND-00001', 'Apendektomi', 'Master',
    true, true, false, true, false, 1, 1, ts, 1, 0, 0, false, true, false, 'None', 0, 0, 0,
    false, false, false, false, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "TrxPatientProcedure" (
    "Id", "EncounterId", "ConsultationId", "PatientId", "DoctorId", "ServiceUnitId",
    "ProcedureId", "ProcedureCodeSnapshot", "ProcedureNameSnapshot", "ProcedureMasterType",
    "IsFromMasterProcedure", "IsPrimaryProcedure", "IsEmergencyProcedure",
    "IsSurgeryRelated", "IsPackageProcedure", "ProcedureSource", "ProcedureStatus",
    "ProcedureDateTime", "Quantity", "UnitPrice", "TotalPrice", "IsFreeOfCharge",
    "IsBillable", "IsCoveredByInsurance", "CoverageStatus", "CoveragePercent",
    "CoveredAmount", "PatientPayAmount", "IsNeedApproval", "IsApproved", "IsExecuted",
    "IsBillingGenerated", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy",
    "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT patient_procedure_b, encounter, consultation, patient, doctor_surgeon, unit_ok,
    procedure_herniotomy, 'TND-00002', 'Herniotomi', 'Master',
    true, false, false, true, false, 1, 1, ts, 1, 0, 0, false, true, false, 'None', 0, 0, 0,
    false, false, false, false, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 10. Consent
--
-- Persiapan menahan kasus di status Terjadwal sampai consent operasi (tipe 3) dan
-- anestesi (tipe 4) sah. Status 2 = Signed.
-- =====================================================================================

INSERT INTO "TrxPatientConsent" (
    "Id", "ConsentNumber", "PatientId", "EncounterId", "ConsentType", "ConsentStatus",
    "ConsentMethod", "ConsentTitle", "IsDiagnosisExplained", "IsProcedureExplained",
    "IsRiskExplained", "IsAlternativeExplained", "IsPatientUnderstood", "IsPatientAgreed",
    "IsEmergencyConsent", "IsHighRiskConsent", "IsLegalDocument", "IsPartOfMedicalRecord",
    "SignerType", "SignerName", "IsSignerPatient", "IsSignerLegalRepresentative",
    "ConsentDateTime", "SignedAt", "IsVerified", "IsApproved", "IsRejected",
    "IsWithdrawn", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy",
    "CancelBy", "IsCancel", "IsDelete")
SELECT consent_surgery, 'CNS-000001', patient, encounter, 3, 2, 1,
    'Persetujuan Tindakan Operasi',
    true, true, true, true, true, true, false, true, true, true,
    1, 'Slamet Riyadi', true, false, ts, ts, true, true, false, false, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "TrxPatientConsent" (
    "Id", "ConsentNumber", "PatientId", "EncounterId", "ConsentType", "ConsentStatus",
    "ConsentMethod", "ConsentTitle", "IsDiagnosisExplained", "IsProcedureExplained",
    "IsRiskExplained", "IsAlternativeExplained", "IsPatientUnderstood", "IsPatientAgreed",
    "IsEmergencyConsent", "IsHighRiskConsent", "IsLegalDocument", "IsPartOfMedicalRecord",
    "SignerType", "SignerName", "IsSignerPatient", "IsSignerLegalRepresentative",
    "ConsentDateTime", "SignedAt", "IsVerified", "IsApproved", "IsRejected",
    "IsWithdrawn", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy",
    "CancelBy", "IsCancel", "IsDelete")
SELECT consent_anesthesia, 'CNS-000002', patient, encounter, 4, 2, 1,
    'Persetujuan Tindakan Anestesi',
    true, true, true, true, true, true, false, true, true, true,
    1, 'Slamet Riyadi', true, false, ts, ts, true, true, false, false, true,
    ts, actor, actor, actor, actor, false, false
FROM _ids ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 11. Akun login
--
-- PasswordHash, SecurityStamp, dan ConcurrencyStamp disalin dari baris superadmin,
-- sehingga kedua akun ini memakai kata sandi yang sama persis dengan superadmin dan
-- tidak ada kata sandi yang perlu ditulis di dalam berkas ini.
--
-- WorkforceProfileId adalah yang menentukan boleh atau tidaknya seseorang memberi
-- sign-off kesiapan; DoctorId hanya dipakai saat membuat permintaan operasi. Menautkan
-- salah satunya saja membuat sebagian aksi ditolak dengan pesan yang terlihat seperti
-- masalah izin, padahal bukan.
-- =====================================================================================

INSERT INTO "AspNetUsers" (
    "Id", "UserCode", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
    "DisplayName", "UserType", "WorkforceProfileId", "DoctorId", "IsActive",
    "MustChangePassword", "IsGeolocationBypassEnabled",
    "IsFingerprintRegistrationEnabled", "CreateDateTime")
SELECT i.user_anesthetist, 'USR-00101', 'opr.anestesi', 'OPR.ANESTESI',
    'opr.anestesi@rs.example.id', 'OPR.ANESTESI@RS.EXAMPLE.ID', true,
    s."PasswordHash", gen_random_uuid()::text, gen_random_uuid()::text,
    false, false, true, 0, 'dr. Sinta Rahmawati, Sp.An', 1,
    i.wfp_anesthetist, i.doctor_anesthetist, true, false, false, false,
    i.ts
FROM _ids i
JOIN "AspNetUsers" s ON s."NormalizedUserName" = 'SUPERADMIN'
-- Tanpa target kolom, supaya bentrok pada indeks unik mana pun ikut dilewati,
-- termasuk bila akun dengan nama sama sudah dibuat lebih dulu oleh seeder.
ON CONFLICT DO NOTHING;

INSERT INTO "AspNetUsers" (
    "Id", "UserCode", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
    "DisplayName", "UserType", "WorkforceProfileId", "DoctorId", "IsActive",
    "MustChangePassword", "IsGeolocationBypassEnabled",
    "IsFingerprintRegistrationEnabled", "CreateDateTime")
SELECT i.user_nurse, 'USR-00102', 'opr.perawat', 'OPR.PERAWAT',
    'opr.perawat@rs.example.id', 'OPR.PERAWAT@RS.EXAMPLE.ID', true,
    s."PasswordHash", gen_random_uuid()::text, gen_random_uuid()::text,
    false, false, true, 0, 'Ns. Budi Santoso', 1,
    i.wfp_scrub, NULL, true, false, false, false,
    i.ts
FROM _ids i
JOIN "AspNetUsers" s ON s."NormalizedUserName" = 'SUPERADMIN'
-- Tanpa target kolom, supaya bentrok pada indeks unik mana pun ikut dilewati,
-- termasuk bila akun dengan nama sama sudah dibuat lebih dulu oleh seeder.
ON CONFLICT DO NOTHING;

-- Peran SuperAdmin, mengikuti peran akun superadmin yang sudah ada.
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT i.user_anesthetist, r."Id"
FROM _ids i
JOIN "AspNetRoles" r ON r."NormalizedName" = 'SUPERADMIN'
ON CONFLICT DO NOTHING;

INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT i.user_nurse, r."Id"
FROM _ids i
JOIN "AspNetRoles" r ON r."NormalizedName" = 'SUPERADMIN'
ON CONFLICT DO NOTHING;

-- Akun superadmin ditautkan ke dokter bedah agar dapat membuat permintaan operasi dan
-- memberi sign-off dokter bedah. Tautan yang sudah terisi tidak ditimpa.
UPDATE "AspNetUsers" u
SET "DoctorId" = i.doctor_surgeon
FROM _ids i
WHERE u."NormalizedUserName" = 'SUPERADMIN' AND u."DoctorId" IS NULL;

UPDATE "AspNetUsers" u
SET "WorkforceProfileId" = i.wfp_surgeon
FROM _ids i
WHERE u."NormalizedUserName" = 'SUPERADMIN' AND u."WorkforceProfileId" IS NULL;

COMMIT;

-- =====================================================================================
-- Ringkasan
-- =====================================================================================

SELECT 'Akun' AS jenis, u."UserName" AS nama,
    CASE WHEN u."WorkforceProfileId" IS NULL THEN 'BELUM tertaut tenaga' ELSE 'tertaut tenaga' END AS keterangan
FROM "AspNetUsers" u
WHERE u."NormalizedUserName" IN ('SUPERADMIN', 'OPR.ANESTESI', 'OPR.PERAWAT')
UNION ALL
SELECT 'Kamar operasi', r."RoomName", r."RoomCode" FROM "MstRoom" r WHERE r."RoomType" = 8
UNION ALL
SELECT 'Tindakan siap operasi', p."ProcedureNameSnapshot", p."ProcedureCodeSnapshot"
FROM "TrxPatientProcedure" p WHERE p."IsSurgeryRelated" AND p."IsActive" AND NOT p."IsDelete";
