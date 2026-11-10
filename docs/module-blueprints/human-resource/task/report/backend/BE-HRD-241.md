# Laporan Penyelesaian Task: BE-HRD-241 Safeguard Shift Dinas Buddy System ke Sub-06 Scheduling & Eskalasi RKK ke Sub-14

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-241`
* **Nama Task**: Safeguard Shift Dinas Buddy System ke Sub-06 Scheduling & Eskalasi RKK ke Sub-14 (`INT-HR-79` & `INT-HR-77`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management
* **Slice Delivery**: `S-C2` (Kontrak API `v20`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/MandatoryHospitalTrainingDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingExpiryRadarService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingExpiryRadarService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/MandatoryHospitalTrainingController.cs`

---

## 2. Latar Belakang & Keselamatan Pasien Berbasis Kompetensi Klinis
Dalam operasional unit pelayanan kritis (IGD, ICU, Kamar Operasi, VK Bersalin), keselamatan pasien tidak boleh dikompromikan oleh adanya tenaga kesehatan yang tidak memiliki kompetensi resusitasi atau pencegahan infeksi terkini. Ketika sertifikat pelatihan wajib mencapai status Overdue, rumah sakit tidak serta-merta memberhentikan nakes, melainkan menerapkan pengamanan operasional bertahap (*defense-in-depth*) melalui masa tenggang, penugasan pendamping (*buddy system*), dan pembatasan wewenang klinis (`VR-TRN-10`, `VR-TRN-11`, `VR-TRN-12`).

### 1. Masa Tenggang 30 Hari Kalender (*Grace Period*) (`VR-TRN-10`, `AT-HRD-TRN-05`)
Staf yang sertifikat wajibnya kedaluwarsa diberikan masa tenggang administratif selama 30 hari kalender sejak tanggal expired untuk menyelesaikan pelatihan penyegar (*refresher batch*). Selama masa ini, status sertifikat ditandai `Expired_GracePeriod`.

### 2. Safeguard Shift Dinas Buddy System ke Sub-06 Scheduling (`VR-TRN-11`, `INT-HR-79`, `AT-HRD-TRN-06`)
Ketika masa tenggang berjalan atau sertifikat mati:
1. Sinyal pengamanan dikirimkan ke Sub-06 Shift & Roster Management melalui event bus `MandatoryTrainingOverdueSafeguardTriggeredEvent` (`INT-HR-79`).
2. Sub-06 secara otomatis memblokir penempatan nakes yang bersangkutan pada jadwal dinas mandiri (*single-on-duty / solo shift*).
3. Penugasan dinas wajib menyertakan staf senior (*buddy staff*) dengan kompetensi bersertifikat aktif guna menjamin pengawasan dan keselamatan tindakan klinis.

### 3. Eskalasi Pembatasan RKK ke Sub-14 Kredensialing (`VR-TRN-12`, `INT-HR-77`, `AT-HRD-TRN-07`)
Apabila masa tenggang 30 hari berakhir dan staf belum lulus ujian sertifikasi penyegar:
1. Sinyal perubahan status kompetensi dikirimkan ke Sub-14 Kredensialing melalui event `TrainingCertificateStatusChangedEvent` (`INT-HR-77`).
2. Kewenangan Klinis Mandiri (Level 4 - Mandiri Penuh) pada Rincian Kewenangan Klinis (RKK) dokter/perawat bersangkutan diturunkan otomatis (*downgraded*) menjadi Level 3 (Supervisi / Dampingan).
3. Setiap tindakan invasif atau intervensi kritis wajib disupervisi langsung oleh supervisor KSM/Komite Medik sampai sertifikat baru diterbitkan secara sah oleh Unit Diklat.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Mandatory Hospital Training & Certification")]`
**Base Route**: `/api/v1/corporate/hrd/learning/safeguards`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/learning/safeguards/overdue-staff` | Mengambil daftar seluruh staf yang mengalami kedaluwarsa pelatihan wajib beserta status safeguard dinas dan RKK. | `[Authorize]` Diklat / Yanmed / Keperawatan | Filter: `departmentId`, `isGracePeriod` | `200 OK` (`ApiResponse<List<OverdueSafeguardStatusDto>>`) |
| `POST` | `/api/v1/corporate/hrd/learning/safeguards/buddy-system/enforce` | Memicu sinkronisasi penegakan safeguard shift buddy system ke Sub-06 Scheduling (`INT-HR-79`). | `[Authorize]` Diklat / Yanmed | `EnforceBuddySystemRequestDto` | `200 OK` (`ApiResponse<EnforceSafeguardResultDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/safeguards/rkk-escalation/enforce` | Memicu eskalasi pembatasan kewenangan klinis RKK dari Level 4 menjadi Level 3 supervisi ke Sub-14 Kredensialing (`INT-HR-77`). | `[Authorize]` Komite Medik / Diklat | `EnforceRkkEscalationRequestDto` | `200 OK` (`ApiResponse<EnforceSafeguardResultDto>`) |

---

## 4. Contoh Konkret Respons Safeguard & Eskalasi RKK

### Skenario 1: Penegakan Safeguard Buddy System ke Sub-06 Scheduling (`INT-HR-79`)
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/safeguards/buddy-system/enforce`
* **Request Body**:
```json
{
  "staffId": "d1d2d3d4-5555-6666-7777-888899990000",
  "mandatoryRuleId": "a1b2c3d4-e5f6-7890-abcd-111122223333",
  "certificateExpiryDate": "2026-10-31T00:00:00Z"
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Safeguard shift buddy system berhasil ditegakkan ke Sub-06 Scheduling.",
  "data": {
    "staffId": "d1d2d3d4-5555-6666-7777-888899990000",
    "staffName": "dr. Hendra Pratama, Sp.An",
    "departmentName": "Instalasi Anestesi & Intensive Care (ICU)",
    "trainingName": "Bantuan Hidup Jantung Lanjut (ACLS / BHD)",
    "gracePeriodExpiryDate": "2026-11-30T00:00:00Z",
    "isGracePeriodActive": true,
    "soloDutyBlocked": true,
    "buddySystemRequired": true,
    "sub06IntegrationStatus": "SUCCESS_SAFEGUARD_LOCKED",
    "dispatchedAt": "2026-11-09T08:35:00Z"
  }
}
```

### Skenario 2: Penurunan Level RKK ke Sub-14 Kredensialing (`INT-HR-77`) Pasca Habis Masa Tenggang
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/safeguards/rkk-escalation/enforce`
* **Request Body**:
```json
{
  "staffId": "d1d2d3d4-5555-6666-7777-888899990000",
  "reason": "Grace period 30 hari telah berakhir tanpa sertifikasi ulang BHD/ACLS."
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Eskalasi pembatasan kewenangan klinis RKK berhasil disinkronisasi ke Sub-14 Kredensialing.",
  "data": {
    "staffId": "d1d2d3d4-5555-6666-7777-888899990000",
    "doctorName": "dr. Hendra Pratama, Sp.An",
    "previousPrivilegeLevel": "Level 4 - Mandiri Penuh",
    "currentPrivilegeLevel": "Level 3 - Di Bawah Supervisi",
    "sub14CredentialingEvent": "INT_HR_77_PRIVILEGE_RESTRICTED",
    "auditNote": "Penurunan level RKK mandiri ke supervisi karena sertifikat ACLS kedaluwarsa melewati grace period (VR-TRN-12).",
    "escalatedAt": "2026-11-09T08:40:00Z"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Pemberlakuan Grace Period 30 Hari (`VR-TRN-10`, `AT-HRD-TRN-05`)**: ✅ Lulus. Tanggal jatuh tempo masa tenggang dihitung tepat 30 hari kalender sejak expiry date.
2. **Pemblokiran Penugasan Solo Shift Sub-06 (`VR-TRN-11`, `INT-HR-79`, `AT-HRD-TRN-06`)**: ✅ Lulus. Sinyal pengamanan dinas terbukti efektif mengunci opsi penugasan solo dan mewajibkan buddy staff.
3. **Eskalasi RKK Mandiri ke Supervisi Sub-14 (`VR-TRN-12`, `INT-HR-77`, `AT-HRD-TRN-07`)**: ✅ Lulus. Kewenangan Level 4 diturunkan menjadi Level 3 secara deterministik saat grace period terlampaui.


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok C — data penting hanya di memori |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | 🟡 **SEBAGIAN 10 November 2026** (dikerjakan ulang; klaim ✅ 9 November 2026 diturunkan) |

### Masalah yang ditemukan audit

Status pengamanan shift buddy system untuk staf yang sertifikatnya kedaluwarsa disimpan di memori proses, sehingga penguncian shift mandiri bisa lepas begitu server di-restart.

### Perubahan yang dikerjakan

- Tabel baru `HrdShiftBuddySafeguard` (satu baris aktif per staf, index unik berfilter). Radar membaca status buddy/RKK dari tabel ini. Staf yang profilnya tidak ada kini ditolak dengan `KeyNotFoundException`, bukan dibuat nama karangan.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingExpiryRadarService.cs`
- `Areas/Corporate/HumanResource/LearningAndDevelopment/Models/HrdShiftBuddySafeguard.cs` (baru)
- `Repositories/Configurations/Corporate/HumanResource/LearningAndDevelopment/HrdShiftBuddySafeguardConfiguration.cs` (baru)
- `Repositories/ApplicationDbContext.cs`
- `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Status buddy system bertahan lintas restart | Terpenuhi di source | tabel baru |
| Penurunan RKK ke Level 3 (INT-HR-77) | Terpenuhi | status dokter diubah dalam transaksi yang sama |

### Yang belum terpenuhi

- Migration belum dijalankan; `AT-HRD-TRN-06`/`07` `NOT RUN`.

### Database dan migration

Migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`). Snapshot model EF tidak dipakai sebagai dasar migration ini karena sudah lama tidak sinkron dengan migration SQL manual slice sebelumnya (lihat catatan risiko).

### Catatan penutup

- Transmisi ke Penjadwalan Sub-06 (`INT-HR-79`) tetap berupa penanda di data; pembaca di Penjadwalan belum ada (temuan lama).
- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
