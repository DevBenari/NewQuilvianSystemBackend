# Laporan Perubahan Backend — `BE-HRD-450`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-450` |
| Judul | Panel Dokumen Kedaluwarsa: SIP/STR, SPK/RKK, dan sertifikat pelatihan wajib |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-73` · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-450` |
| Trace | `FR-PKB-01` (panel 5); `HRD-DEC-235`; kamus data 30.2; `INT-HR-112`, `INT-HR-113`; `AT-HRD-PKB-06` |
| Contract version | `v29` — API 35.1 (`documentExpiry`); State 33.1 — **approved** 23 November 2026 |
| Dependency | `BE-HRD-448` ✅ |
| Klasifikasi | `MEDIUM` — tiga sumber data dari dua modul lain |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | `WorkforceCore/Services/HrdWorkforceDashboardService.cs`, `WorkforceCore/Constants/WorkforceDashboardPanels.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpetakan ke source; batas kelompok tiap sumber diverifikasi dari source dan dicatat; build 0 error, QBE Strict `PASS`; uji API DB dev `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` (membaca `CredentialingManagement` dan `LearningAndDevelopment`) |
| Keberlakuan | `NEW CODE`; tidak ada perubahan pada modul sumber |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-LOG-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Tidak ada satu tempat yang menunjukkan berapa SIP/STR, SPK/RKK, dan sertifikat pelatihan wajib yang sudah habis atau segera habis. Padahal dokumen yang habis menghentikan kewenangan praktik tenaga kesehatan.

## 2. Proses bisnis

Panel menggabungkan tiga sumber, lalu mengelompokkannya: **sudah habis**, **≤ 30 hari**, **31–60 hari**, **61–90 hari**.

| Sumber | Cara membaca | Pemetaan kelompok |
| --- | --- | --- |
| SIP/STR | Radar lisensi milik Kredensial (`GetLicenseExpiringRadarListAsync`) — sudah mengecualikan STR seumur hidup dan lisensi dicabut | `ExpiredH0` → habis; `H7`, `H30` → ≤ 30; `H60` → 31–60; `H90` → 61–90 |
| Sertifikat pelatihan wajib | Radar sertifikat milik Diklat (`GetExpiringRadarSummaryAsync`) | `OVERDUE_H0` → habis; `RADAR_H30_CRITICAL` → ≤ 30; `RADAR_H60` → 31–60; `RADAR_H90` → 61–90 |
| SPK/RKK | Kueri baca `TrxSpkRkkIssuance` berstatus `Active` yang berlaku sampai ≤ hari ini + 90 (belum ada radar SPK) | sisa hari ≤ 0 → habis; 1–30; 31–60; 61–90 |

**Contoh berangka (`AT-HRD-PKB-06`).** SIP dr. Dodi lewat 2 hari, STR seumur hidup perawat Sari, SPK `Active` sisa 20 hari, SPK `Revoked` sisa 10 hari, sertifikat BTCLS sisa 45 hari → habis 1 (SIP), ≤ 30 hari 1 (SPK aktif), 31–60 hari 1 (BTCLS). STR seumur hidup dan SPK `Revoked` tidak dihitung.

### Batas kelompok yang diverifikasi dari source (usulan desain butir 3)

| Sumber | Batas sebenarnya | Tanggal acuan | Bukti |
| --- | --- | --- | --- |
| Radar lisensi | ≤ 0 habis, ≤ 7, ≤ 30, ≤ 60, ≤ 90 — tepat 30/60/90 | `DateTime.UtcNow.Date` | `CredentialVerificationService.cs` baris 366–367, 500–504 |
| Radar sertifikat | `< 0` habis, ≤ 30, ≤ 60, sisanya ≤ 90 — **sertifikat yang habis hari ini masuk ≤ 30 hari**, bukan "habis" | `DateTime.UtcNow.Date` | `TrainingExpiryRadarService.cs` baris 46–79 |
| SPK/RKK (kueri baru) | ≤ 0 habis, 1–30, 31–60, 61–90 | Tanggal operasional WIB | `LoadDocumentRowsAsync` |

Sesuai kamus data 30.2, angka mengikuti radar sumber apa adanya. Perbedaan hari-H pada radar sertifikat **tidak** diperbaiki di sini karena radar itu milik `S-C2`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`CredentialingManagement/Services/CredentialVerificationService.cs`, `ICredentialVerificationService.cs`, `DTOs/CredentialLicenseDtos.cs` (`LicenseRadarAlertDto`), `Models/TrxSpkRkkIssuance.cs`; `LearningAndDevelopment/Services/TrainingExpiryRadarService.cs`, `ITrainingExpiryRadarService.cs`, `DTOs/MandatoryHospitalTrainingDtos.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdWorkforceDashboardService.cs` | `LoadDocumentRowsAsync` (tiga sumber), pemetaan tingkat radar, `BuildDocumentExpiry` (total dan `byType[]`), `MaskReferenceNumber` |
| `Areas/Corporate/HumanResource/WorkforceCore/Constants/WorkforceDashboardPanels.cs` | Kode kelompok dokumen, jenis dokumen, `DocumentBucketFromDays` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Ruas `documentExpiry.*` pada `/summary` |
| Database | Tidak ada; baca-saja, tidak menulis ke modul sumber |
| Keamanan/Auth | Nomor lisensi dan nomor SPK tidak dikirim di ringkasan; di daftar nama disamarkan (lihat `BE-HRD-451`) |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Workforce Dashboard

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/summary` | Ruas `documentExpiry` | `HrdWorkforceDashboard : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` penuh Paket B | 0 error; tanpa warning di berkas task | `PASS` | Log build 23 November 2026 |
| QBE Strict (19 berkas) | `PASS` | `PASS` | Keluaran checker |
| Penelusuran: radar lisensi baca-saja | Hanya kueri + pemetaan, tanpa `SaveChanges` | `PASS` | `GetLicenseExpiringRadarListAsync` |
| Penelusuran: radar sertifikat baca-saja | Hanya kueri + pemetaan | `PASS` | `GetExpiringRadarSummaryAsync` |
| Penelusuran: `byType` = total | Total dan per jenis dihitung dari daftar baris yang sama | `PASS` | `BuildDocumentExpiry` |
| Uji API `/summary` dibanding endpoint radar di DB dev | Belum dijalankan | `NOT RUN` | Butuh izin menyalakan backend |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Data `AT-HRD-PKB-06` → habis 1, ≤ 30 1, 31–60 1; STR seumur hidup dan SPK `Revoked` tidak dihitung | Terpenuhi | Radar lisensi mengecualikan `IsLifetimeStr`; SPK hanya `Active`; pemetaan kelompok |
| 2. Jumlah `byType[]` per kelompok = total kelompok | Terpenuhi | Satu daftar baris untuk keduanya |
| 3. Batas kelompok tiap sumber diverifikasi dan dicatat | Terpenuhi | Tabel batas di bagian 2 |
| 4. Dashboard tidak menulis ke modul sumber dan tidak menyimpan salinan | Terpenuhi | Seluruh pembacaan `AsNoTracking`/read-only |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | (1) Radar sertifikat menghitung hari-H sebagai ≤ 30 hari dan memakai tanggal UTC — butuh keputusan pemilik `S-C2` bila ingin diseragamkan. (2) Radar lisensi tidak menyaring `IsDelete`; dashboard mengikuti radar apa adanya |
| Risiko tersisa | Bila radar SPK/RKK kelak dibuat di modul Kredensial, dashboard wajib beralih memakainya (Integration 26 butir 3) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pengguna (`9062bd61`) |
| Langkah berikutnya | `BE-HRD-451` |
