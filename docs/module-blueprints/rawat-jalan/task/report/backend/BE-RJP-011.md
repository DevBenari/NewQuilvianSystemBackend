# Laporan Perubahan Backend — `BE-RJP-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-011` |
| Judul | Nama ruang tunggu prioritas |
| Slice | `MVP-1`, `EPIC RP-07` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-011` |
| Trace | `FR-RP-060`, `061`; `RJ-PRI-DEC-017`, `018` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` — `api-contract.md` *Nurse Station Queue dan Doctor Queue (diperbarui)*, *Patient Encounter (diperbarui)* |
| Dependency | `BE-RJP-005` ✅ |
| Klasifikasi | `LOW` — skor 3: diperiksa 3–8 (0), diubah 7 (1), logika rendah (0), kontrak aditif (1), keamanan (0), workflow (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | DTO `NurseStationQueueDtos`, `DoctorQueueDtos`, `PatientEncounterDtos`; controller `NurseStationQueueController`, `DoctorQueueController`, `PatientEncounterController`; `PriorityReservationOptions` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-19 |
| Status | ✅ Selesai — runtime 3/3 dua kali (nama bawaan dan nama dari konfigurasi) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-DTO-001` (field DTO baru), `QBE-CFG-001` (nama dari konfigurasi, bukan literal di controller) |
| Role access | Tidak berubah. Field baru mengikuti hak akses endpoint antrean yang sudah ada |
| Utang teknis yang disentuh | Ketiga controller memakai `ApplicationDbContext` langsung (legacy). Konfigurasi diambil per request dari `RequestServices` agar konstruktor lama tidak berubah |

---

## 1. Masalah yang diselesaikan

Antrean prioritas disembunyikan dari layar display (`BE-RJP-003`/`004`), sehingga perawat dan dokter perlu tahu pasien itu
menunggu di mana. PDF 2D–2E meminta petugas yang menghampiri pasien di ruang tunggu prioritas.

## 2. Proses bisnis

Setiap item antrean prioritas di worklist Nurse Station dan Doctor Queue membawa nama ruang tunggu prioritas dari
konfigurasi. Respons pendaftaran kunjungan juga membawanya, sehingga tiket kiosk dan layar admisi dapat mengarahkan
pasien ke ruang itu. Antrean Reguler membawa `null`.

Contoh dari uji: pasien Gold didaftarkan; respons pendaftaran, worklist perawat, dan worklist dokter menampilkan
"Ruang Tunggu Prioritas". Saat konfigurasi diganti menjadi "Lounge Prioritas Uji", ketiganya ikut berubah tanpa
perubahan kode. Pasien Reguler selalu `null`.

## 3. Perubahan yang dikerjakan

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/PriorityReservationOptions.cs` | Diperbarui | `ResolveWaitingLocationFor(bool isPriorityQueue)` |
| `Areas/HealthServices/RegistrationManagement/DTOS/NurseStationQueueDtos.cs` | Diperbarui | `NurseStationQueueResponse.PriorityWaitingLocationName` |
| `Areas/HealthServices/RegistrationManagement/DTOS/DoctorQueueDtos.cs` | Diperbarui | `DoctorQueueResponse.PriorityWaitingLocationName` (ikut diwarisi `DoctorPendingConsultationResponse`) |
| `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` | Diperbarui | `PatientEncounterQueueClassificationResponse.PriorityWaitingLocationName` |
| `Areas/HealthServices/RegistrationManagement/Controllers/NurseStationQueueController.cs` | Diperbarui | Mengisi field pada item daftar |
| `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs` | Diperbarui | Mengisi field di `MapResponsesAsync` (daftar antrean dan konsultasi tertunda) |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Diperbarui | Mengisi `queueClassification.priorityWaitingLocationName` |

| Aspek | Dampak |
| --- | --- |
| Kontrak | Field baru opsional pada tiga respons, aditif |
| Database | Tidak ada perubahan schema. `has-pending-model-changes` bersih |
| Konfigurasi | `HealthServices:Registration:PriorityReservation:PriorityWaitingRoomName` (bawaan "Ruang Tunggu Prioritas", kosong = bawaan) |

## 4. Dokumentasi endpoint

| Grup | Method | Path | Field baru | Status |
| --- | --- | --- | --- | --- |
| Nurse Station Queue | `GET` | `api/v1/health-services/registration-management/nurse-station-queues` | `items[].priorityWaitingLocationName` | Tersedia |
| Doctor Queue | `GET` | `api/v1/health-services/registration-management/doctor-queues` (dan daftar konsultasi tertunda) | `items[].priorityWaitingLocationName` | Tersedia |
| Patient Encounter | `POST` | `api/v1/health-services/registration-management/patient-encounters/admin`, `/`, `/kiosk` | `queueClassification.priorityWaitingLocationName` | Tersedia |

Hak akses endpoint tidak berubah.

## 5. Verifikasi

Data samaran: pasien `RJPR UJI WR-GOLD` (membership Gold disisipkan SQL data uji) dan `RJPR UJI WR-REG`, didaftarkan lewat
`POST /patient-encounters/admin`, lalu dipindah ke tahap dokter lewat panggil → mulai skrining → selesai skrining.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build, `has-pending-model-changes`, QBE Strict (7 berkas) | 0 error; bersih; VIOLATION 0 | `PASS` |
| Respons buat kunjungan | Gold "Ruang Tunggu Prioritas"; Reguler `null` | `PASS` |
| `R-RP-29` Nurse Station | Gold "Ruang Tunggu Prioritas"; Reguler `null` | `PASS` |
| `R-RP-29` Doctor Queue | Gold "Ruang Tunggu Prioritas"; Reguler `null` | `PASS` |
| Ketiganya dengan proses uji dijalankan `…PriorityWaitingRoomName="Lounge Prioritas Uji"` | Gold "Lounge Prioritas Uji"; Reguler `null` | `PASS` |

Skrip: `rt_rjp011.py` dan `rt_rjp011_room.py` di scratchpad sesi. Total 3/3 dua kali.

Catatan percobaan: start server pertama untuk uji konfigurasi gagal (`exit 127`) karena salah penulisan variabel
environment di shell uji; diulang dengan sintaks yang benar.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`. Tampilan teks di layar worklist diuji di `FE-RJP-007`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-29` (sisi API) | Terpenuhi | Nurse Station dan Doctor Queue |
| Antrean Reguler `null` | Terpenuhi | Semua skenario |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | — |
| Risiko tersisa | — |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `BE-RJP-012` (endpoint publik link konfirmasi) |
