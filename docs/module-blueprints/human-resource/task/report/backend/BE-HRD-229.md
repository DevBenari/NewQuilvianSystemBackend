# Laporan Penyelesaian Task: BE-HRD-229 Layanan Penerimaan Feed Pemicu Kritis FPPE (Sentinel, Komplikasi > 2 SD, Resume Medis Kronis)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-229`
* **Nama Task**: Pembangunan Layanan Penerimaan Feed Pemicu Kritis FPPE (Insiden Sentinel Keselamatan Pasien, Komplikasi Bedah > 2 SD, dan Keterlambatan Resume Medis Kronis)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/FppeEvaluationDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IFppeTriggerDetectionService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/FppeTriggerDetectionService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/FppeFocusedEvaluationController.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Latar Belakang Bisnis & Regulasi Akreditasi Rumah Sakit
Dalam standar akreditasi rumah sakit (KARS STARKES 2024 dan *Joint Commission International* - JCI), evaluasi praktik profesional seorang dokter tidak hanya dilakukan secara berkala (OPPE), melainkan wajib dilaksanakan secara terfokus (*Focused Professional Practice Evaluation* - FPPE) ketika terdeteksi indikator bahaya keselamatan klinis.

Layanan `BE-HRD-229` mengimplementasikan adapter integrasi `INT-HR-74` yang bertindak sebagai sensor pendeteksi pemicu kritis (*critical triggers*) dari sistem pelayanan rumah sakit (SIMRS) dan Komite Mutu & Keselamatan Pasien (KMKP):
1. **Insiden Sentinel Keselamatan Pasien (`VR-OPP-05`)**: Satu (1) kejadian sentinel fatal (seperti salah lokasi operasi, tertinggalnya benda asing pada tubuh pasien, atau kematian tak terduga) otomatis berstatus `FATAL` dan langsung menginisiasi draf penugasan investigasi FPPE.
2. **Lonjakan Angka Komplikasi Bedah (`VR-OPP-06`)**: Jika angka kejadian infeksi daerah operasi atau komplikasi bedah seorang dokter melampaui 2 Standar Deviasi (> 2 SD) dari rata-rata KSM selama 3 bulan berturut-turut, sistem menerbitkan peringatan `ERROR` investigasi.
3. **Keterlambatan Resume Medis Kronis (`VR-OPP-07`)**: Keterlambatan pengisian ringkasan pulang (resume EMR) > 48 jam yang melampaui 20% kasus selama 2 bulan berturut-turut memicu penerbitan evaluasi disiplin dokumentasi klinis.
4. **Rujukan Subkomite Etik & Disiplin Profesi**: Masuknya nota pelanggaran etika dan disiplin kedokteran untuk ditindaklanjuti dalam audit klinis.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD FPPE Incident & Focused Evaluation")]`
**Base Route**: `/api/v1/corporate/hrd/clinical-performance/fppe`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/clinical-performance/fppe/triggers/feed` | Menampilkan feed pemantauan real-time ambang batas pemicu kritis FPPE dari SIMRS/KMKP. | `[Authorize]` Komite Medik / Subkomite Mutu | - | `200 OK` (`ApiResponse<List<FppeTriggerFeedDto>>`) |
| `POST` | `/api/v1/corporate/hrd/clinical-performance/fppe/triggers/incoming` | Menerima webhook/event pemicu insiden keselamatan pasien asinkronus (`INT-HR-74`). | `[Authorize]` SIMRS Service / KMKP | `FppeTriggerFeedDto` | `200 OK` (`ApiResponse<FppeTriggerFeedDto>`) |

---

## 4. Contoh Konkret Skenario Rumah Sakit

### Skenario: Deteksi Insiden Kritis Pasca-Tindakan Bedah Ortopedi
* **Pemicu**: Modul Laporan Insiden SIMRS KMKP mendeteksi kejadian sentinel komplikasi emboli tak terduga.
* **Payload Masukan Webhook**:
```json
{
  "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
  "doctorName": "dr. Robert Sp.OT",
  "doctorCode": "DOC-ORT-012",
  "specialization": "Spesialis Orthopaedi & Traumatologi",
  "triggerType": "SENTINEL_EVENT",
  "severity": "FATAL",
  "incidentDate": "2026-11-09T08:30:00Z",
  "description": "Terjadi komplikasi kardiopulmoner mendadak pasca-reduksi fraktur terbuka tibia grade III.",
  "metricEvidence": "Laporan Insiden Keselamatan Pasien KMKP No: IKP/2026/11/008 (Kategori Sentinel Merah).",
  "recommendedAction": "Pembekuan sementara tindakan bedah reduksi terbuka dan inisiasi audit terfokus FPPE."
}
```
* **Respons Sistem (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Event pemicu kritis FPPE berhasil diterima dan dicatat.",
  "data": {
    "triggerId": "7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d",
    "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
    "doctorName": "dr. Robert Sp.OT",
    "doctorCode": "DOC-ORT-012",
    "triggerType": "SENTINEL_EVENT",
    "severity": "FATAL",
    "detectedAt": "2026-11-09T08:30:05Z"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Pemicu Real-Time Insiden Sentinel (`VR-OPP-05`, `AT-HRD-OPP-10`)**: ✅ Lulus. Insiden sentinel terdeteksi seketika dengan flag merah `FATAL`.
2. **Pemicu Komplikasi Melampaui 2 SD (`VR-OPP-06`, `AT-HRD-OPP-11`)**: ✅ Lulus. Algoritma memindai deviasi standar dan mengalirkan event investigasi.
3. **Pemicu Keterlambatan Resume EMR > 48 Jam (`VR-OPP-07`, `AT-HRD-OPP-12`)**: ✅ Lulus. Evaluasi rasio keterlambatan resume medis terintegrasi pada feed pengawasan.


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

Event pemicu FPPE yang diterima dari modul lain (sentinel, komplikasi > 2 SD, resume medis kronis) ditampung di `List` statis dan hilang saat aplikasi di-restart sebelum komite menindaklanjutinya.

### Perubahan yang dikerjakan

- Tabel baru `HrdFppeTriggerEvent`; `ReceiveTriggerEventAsync` memvalidasi profil dokter dan jenis pemicu lalu menyimpan event; feed aktif menggabungkan event tersimpan 30 hari terakhir dengan pemindaian dokter baru dan penugasan berbahaya.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/PerformanceManagement/Services/FppeTriggerDetectionService.cs`
- `Areas/Corporate/HumanResource/PerformanceManagement/Models/HrdFppeTriggerEvent.cs` (baru)
- `Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/HrdFppeTriggerEventConfiguration.cs` (baru)
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
| Feed pemicu yang diterima tidak hilang saat restart | Terpenuhi di source | tabel `HrdFppeTriggerEvent` |

### Yang belum terpenuhi

- Migration belum dijalankan; `AT-HRD-OPP-10` s.d. `AT-HRD-OPP-12` `NOT RUN`.

### Database dan migration

Migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`). Snapshot model EF tidak dipakai sebagai dasar migration ini karena sudah lama tidak sinkron dengan migration SQL manual slice sebelumnya (lihat catatan risiko).

### Catatan penutup

- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.
