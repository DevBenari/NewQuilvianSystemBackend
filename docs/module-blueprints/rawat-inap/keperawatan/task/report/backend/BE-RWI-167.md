# Laporan Perubahan Backend — `BE-RWI-167`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-167` |
| Judul | Master jenis alat medis |
| Slice | `MVP-2` / `RWF-W4` — Pemakaian Alat |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-167` |
| Trace | `FR-RWF-060`, `FR-RWF-061`, `FR-RWF-068`; `RWI-DEC-179`, `RWI-DEC-180`, `RWI-DEC-193`; `AC-RWF-060` |
| Contract version | `keperawatan` `0.6.0` **`approved`** (`RWI-DEC-221`): API 8.2; backend 12.7 |
| Dependency | `BE-RWI-172` [EPS] ✅ — model `MstMedicalEquipment`, enum satuan dan pembulatan, kolom `MstTariff.MedicalEquipmentId`, isian tarif |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 0, logika 1, kontrak API 2, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/MasterData/**`, `Program.cs` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, build akhir, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Kelima acceptance criteria terpetakan ke source; build akhir `PASS` |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MasterData` (milik seluruh tim, `RWI-DEC-193`) |
| Prefix registry | `Mst` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (service, controller, DTO); model sudah lahir di `BE-RWI-172` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DEL-001`, `QBE-CODE-004` (kode unik) |
| Wewenang | Source: ya. Task ini tidak mengubah skema. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Tabel `MstMedicalEquipment` sudah ada sejak `BE-RWI-172`, tetapi tidak ada layar maupun endpoint untuk mengelolanya. Admin tidak bisa mendaftarkan "Ventilator" beserta satuan tagih dan pembulatannya, sehingga perawat tidak punya pilihan alat saat mencatat pemakaian dan Billing tidak punya acuan tarif alat.

## 2. Proses bisnis

**Tujuan.** Admin Master Data mengelola jenis alat medis besar beserta satuan tagih dan pembulatan; tarif per kelas memakai master tarif yang sama dengan tindakan dan obat.

**Pelaku.** Admin Master Data pemegang `MedicalEquipment : Read/Create/Update/Delete`; pemilik tarif mengesahkan satuan dan pembulatan (`02-backend-architecture.md` 12.12).

**Langkah utama.**

1. Admin membuat jenis alat: kode, nama, kategori (teks bebas), satuan tagih (`PerUse`, `PerHour`, `PerDay`), pembulatan (bawaan pembulatan ke atas), keterangan.
2. Sistem menormalkan kode ke huruf besar dan menolak kode ganda.
3. Admin membuat tarif per kelas lewat endpoint master tarif dengan field `MedicalEquipmentId` (dibangun `BE-RWI-172`); endpoint tarif menolak alat nonaktif.
4. Alat aktif muncul pada pilihan alat (`/options`) yang dipakai layar Pemakaian Alat.
5. Alat yang tidak dipakai lagi dinonaktifkan; alat yang sudah dirujuk tarif atau pemakaian tidak dapat dihapus.

**Contoh.** Admin membuat "VENT-01 — Ventilator", satuan per hari, pembulatan ke atas. Lalu tarif kelas 2 Rp 1.200.000 per hari dibuat di master tarif. Pemakaian 2 hari 3 jam kelak dihitung 3 hari.

**Aturan bisnis.**

| Aturan | Akibat |
| --- | --- |
| Kode alat unik | `409` `MST-EQP-001` |
| Satuan atau pembulatan diubah selama ada pemakaian `Running` | `422` `MST-EQP-002` |
| Alat nonaktif dirujuk tarif baru | Ditolak endpoint master tarif (`TariffController`, `BE-RWI-172`) |
| Alat sudah dirujuk tarif atau pemakaian dihapus | `400`, disarankan nonaktifkan |
| Versi baris berubah saat diubah | `409` |

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| Aktif | Nonaktifkan | Nonaktif | `MedicalEquipment : Update` | — |
| Nonaktif | Aktifkan | Aktif | `MedicalEquipment : Update` | — |
| Aktif/Nonaktif | Hapus (soft delete) | Terhapus | `MedicalEquipment : Delete` | Belum dirujuk tarif maupun pemakaian |

**Hasil akhir.** Pilihan alat aktif tersedia bagi Pemakaian Alat; tarif alat dikelola di master tarif yang sama.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 8.2, `MstMedicalEquipment`, `TariffController` (validasi `MedicalEquipmentId`, `:861-865`), `SurgicalPreparationItemService` sebagai pembanding master data, standar endpoint master data, dan aturan hak akses.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/DTOs/MedicalEquipmentDtos.cs` | Baru. Request, query, response, opsi, ringkasan, metadata |
| `Areas/HealthServices/MasterData/Services/MedicalEquipmentService.cs` | Baru. Ringkasan, opsi aktif, daftar berhalaman, detail, simpan dengan kunci baris, status, hapus lunak; pemeriksaan `MST-EQP-001`/`002` |
| `Areas/HealthServices/MasterData/Controllers/MedicalEquipmentController.cs` | Baru. Sembilan endpoint baseline master data |
| `Program.cs` | Registrasi `MedicalEquipmentService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint API 8.2 ditambah empat endpoint baseline standar master data (`filters/metadata`, `summary`, `options`, `DELETE`) sebagai delta kontrak. `options` dipakai layar Pemakaian Alat |
| Database | `NOT APPLICABLE` — tabel dan kolom tarif sudah dibuat `BE-RWI-172` (migration `20261005033044_AddRawatInapFinishing`, sudah diterapkan) |
| Keamanan/Auth | Resource baru `MedicalEquipment` (tanpa awalan `Mst`) di modul `HEALTH_SERVICE_MASTER_DATA`; kemampuan `Read`, `Create`, `Update`, `Delete` lahir dari atribut |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Medical Equipment

Base URL: `api/v1/health-services/master-data/medical-equipments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi filter dan opsi enum | `MedicalEquipment : Read` | — | `MedicalEquipmentFilterMetadata` |
| `GET` | `/summary` | Jumlah total, aktif, nonaktif | `MedicalEquipment : Read` | — | `MedicalEquipmentSummary` |
| `GET` | `/options` | Pilihan alat aktif untuk Pemakaian Alat | `MedicalEquipment : Read` | Query `search` | `List<MedicalEquipmentOption>` |
| `GET` | `/` | Daftar berhalaman dengan saringan aktif | `MedicalEquipment : Read` | `MedicalEquipmentQuery` | `PagedResult<MedicalEquipmentResponse>` |
| `GET` | `/{id}` | Detail | `MedicalEquipment : Read` | — | `MedicalEquipmentResponse` |
| `POST` | `/` | Menambah jenis alat | `MedicalEquipment : Create` | `CreateMedicalEquipmentRequest` | `MedicalEquipmentResponse` (`201`) |
| `PUT` | `/{id}` | Mengubah jenis alat | `MedicalEquipment : Update` | `UpdateMedicalEquipmentRequest` (dengan `RowVersion`) | `MedicalEquipmentResponse` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `MedicalEquipment : Update` | `MedicalEquipmentStatusRequest` | `MedicalEquipmentResponse` |
| `DELETE` | `/{id}` | Hapus lunak bila belum dirujuk | `MedicalEquipment : Delete` | — | `MedicalEquipmentResponse` |

Kode status: `400` isian tidak sah atau alat masih dirujuk; `404` tidak ditemukan; `409` `MST-EQP-001` kode ganda atau versi berubah; `422` `MST-EQP-002` satuan diubah saat alat dipakai.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir; nol warning di berkas baru |
| Pemeriksaan statis atribut hak akses | Sembilan endpoint cocok `MedicalEquipment` ↔ action | `PASS` | Skrip pemeriksa atribut |
| Verifikasi proses bisnis — kode ganda, satuan saat `Running`, opsi aktif | Sesuai aturan bagian 2 | `PASS` | `MedicalEquipmentService.SaveAsync`, `OptionsAsync` |
| Regresi tarif tindakan dan obat (`RWI-DEC-193`) | `TariffController` tidak diubah task ini; validasi alat aktif sudah ada di `:861-865` | `PASS` (penelusuran source) | `git diff` kosong untuk `TariffController.cs` |
| Uji API HTTP | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. "Ventilator" per hari dengan pembulatan ke atas tersimpan | Terpenuhi | `SaveAsync` menyimpan `ChargeUnit`/`RoundingRule`; bawaan `CeilingWholeUnit` |
| 2. Kode ganda → 409 `MST-EQP-001` | Terpenuhi | `SaveAsync` (pemeriksaan kode dan tangkapan unique `23505`) |
| 3. Alat nonaktif tidak muncul pada pilihan alat aktif dan tidak dapat dirujuk tarif baru | Terpenuhi | `OptionsAsync` (`IsActive`); `TariffController.cs:861-865` |
| 4. Tarif per kelas untuk alat aktif dapat dibuat lewat endpoint master tarif; tarif tindakan dan obat lama tidak berubah | Terpenuhi | Isian tarif `BE-RWI-172`; tidak ada perubahan `TariffController` pada task ini |
| 5. Baris registry `MedicalEquipment : Read/Create/Update` lahir dari atribut endpoint | Terpenuhi | `MedicalEquipmentController` (ditambah `Delete`) |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5 |
| DoD: verifikasi API runtime | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satuan dan pembulatan tiap jenis alat wajib disahkan pemilik tarif sebelum dipakai pasien sungguhan |
| Masalah yang diketahui | Isi `filters/metadata` masih minimal dibanding standar master data (belum memuat `CreateFields`/`UpdateFields`, `QueryParameters` bertipe, `DateFormat`, `ResetButtonLabel`; `SortOptions` belum berbentuk `Value`/`Label`). Dilaporkan sebagai kekurangan sesuai standar, tidak menahan kriteria task |
| Risiko tersisa | Belum diuji lewat HTTP |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`); berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `FE-RWI-187` (layar master alat dan isian tarif) |
