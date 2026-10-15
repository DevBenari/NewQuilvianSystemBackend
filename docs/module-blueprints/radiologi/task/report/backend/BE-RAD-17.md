# Laporan Perubahan Backend — `BE-RAD-17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-17` |
| Judul | Empat transisi study: tandai perlu diulang, tahan, lanjutkan, batalkan |
| Slice | `MVP-3` — alur pemeriksaan; penutup temuan `FE-RAD-09` bagian 4.1 |
| Roadmap | `roadmap/backend-roadmap.md` bagian 6b. Task dibuka atas instruksi pemilik modul Yoga Aji Pratama, 2026-10-14 |
| Trace | `RAD-STATE-001` bagian 2; `RAD-API-001` bagian 4; `FE-RAD-09` bagian 4.1 |
| Contract version | `RAD-API-001` revision 12 dan `RAD-PERM-001` revision 10 — amandemennya ditulis task ini atas instruksi pemilik modul 2026-10-14 |
| Dependency | Tidak ada. Kolom yang dipakai sudah ada sejak migration awal |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 1, kontrak API 2, database 0, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Tests/QuilvianSystemBackend.RadiologyTests/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `49c6a692`, branch `yoga` |
| Tanggal | 2026-10-14 |
| Status | **Selesai.** Build lulus; 34/34 uji lulus; lima permintaan HTTP ke backend lokal sesuai harapan tanpa menulis data |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `RadiologyManagement` / — |
| Pemilik & prefix pada registry | `Radiology`, prefix `Rad` |
| Status registry | **`ACTIVE`** — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` untuk empat method service, empat action controller, dan `RadStudyTransitionRequest`; `TOUCHED LEGACY` untuk `RadStudyResponse` dan `MapStudy` |
| QBE ID yang berlaku | `QBE-SVC-001` (controller hanya memanggil service), `QBE-API-001` (`ApiResponse<T>`, kode status bagian 4 kontrak), `QBE-PERM-001` (`[AccessAction]` + `[AccessPermission]`), `QBE-VAL-001` (alasan wajib, penjaga status), `QBE-LOG-001` (`RadTransitionHistory` dengan pelaku), `QBE-DTO-001` (balasan lewat DTO), `QBE-DEL-001` (pembatalan beralasan dan beraktor) |
| Wewenang database | **Tidak dipakai.** Tidak ada perubahan schema, entity, maupun migration |

---

## 1. Masalah yang diperbaiki

`RAD-STATE-001` bagian 2 menjanjikan empat tindakan pada study radiologi yang tidak pernah punya
jalan di API:

| Tindakan | Akibat sebelum task ini |
| --- | --- |
| Tandai perlu diulang | Status `RepeatRequired` tidak pernah ditulis di mana pun. Citra yang ditolak tidak dapat dibedakan antara "belum diputuskan" dan "sudah diputuskan diulang" |
| Tahan | Bila pasien harus keluar ruangan sebentar (misalnya ke toilet atau menunggu hasil lab), petugas tidak punya cara menandainya. Study tampak menganggur tanpa sebab |
| Lanjutkan | Tidak ada, karena menahan pun tidak ada |
| Batalkan | Study yang batal dikerjakan (misalnya pasien menolak) tetap tercatat berjalan selamanya, menumpuk di daftar kerja |

Layar `FE-RAD-09` sengaja tidak membuat tombolnya dan menuliskan temuan ini pada laporannya
bagian 4.1. Pemilik modul memutuskan endpointnya ditambahkan.

---

## 2. Proses bisnis

**Pelaku.** Radiografer dan petugas radiologi pada konsol study.

**Alur normal — menahan lalu melanjutkan.**

1. Study sudah lolos gerbang keselamatan (`SafetyCleared`); pasien minta ke toilet.
2. Petugas menekan **Tahan Study**, alasan opsional. Status menjadi `OnHold`, dan status asal
   (`SafetyCleared`) disimpan di `StatusBeforeHold`.
3. Pasien kembali; petugas menekan **Lanjutkan Study**. Status kembali tepat ke `SafetyCleared`,
   bukan ditebak.
4. Petugas menekan **Mulai Pengambilan Citra**. Gerbang keselamatan **dinilai ulang penuh** saat
   itu, sehingga perubahan aturan selama jeda tidak terlewat.

**Alur normal — membatalkan.** Pasien menolak pemeriksaan sebelum disinari. Petugas menekan
**Batalkan Study** dan **wajib** menulis alasan. Status menjadi `Cancelled`, alasan tersimpan di
`ClosureReason`. Pesanannya tetap terbuka, dan pemeriksaan dapat direncanakan ulang sebagai study
baru.

**Alur normal — menandai perlu diulang.** Citra dinilai tidak layak (`QualityRejected`). Petugas
menekan **Tandai Perlu Diulang**. Status menjadi `RepeatRequired`. Study pengulangan tetap dibuat
lewat `POST /{id}/repeat` yang sudah ada, lengkap dengan sebab dan alasannya.

**Jalur tidak normal.**

| Keadaan | Jawaban | Contoh |
| --- | --- | --- |
| Menahan study yang acquisition-nya sedang berjalan | `409 RAD_INVALID_TRANSITION` | Mesin sudah menyala; yang benar diselesaikan atau dihentikan lewat `abort-acquisition` |
| Membatalkan tanpa alasan, atau alasan hanya spasi | `400 RAD_REASON_REQUIRED` | — |
| Membatalkan study yang pernah menyinari pasien | `409` | Walaupun statusnya tampak awal, `AcquisitionStartedAt` terisi → ditolak, agar jejak paparan tidak hilang |
| Melanjutkan study yang tidak ditahan | `409` | Dua petugas menekan bersamaan; yang kedua ditolak |
| Study tidak ditemukan | `404 RAD_STUDY_NOT_FOUND` | — |
| Dua petugas mengubah study yang sama bersamaan | `409 RAD_CONCURRENCY_CONFLICT` | Penjaga `Version` yang sudah ada |

**Status yang boleh ditahan dan dibatalkan** — "beberapa status" pada kontrak dibaca sebagai
seluruh status **sebelum** acquisition:

| Status | Tahan | Batalkan |
| --- | :---: | :---: |
| `Planned`, `PatientVerified`, `SafetyCleared` | Ya | Ya |
| `OnHold` | — | Ya |
| `AcquisitionStarted` dan sesudahnya | Tidak | Tidak |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` (TASK, API, DATABASE,
REVIEW, CLASSIFICATION, REPORT); `RadStudyController.cs`; `RadStudyService.cs`;
`RadOrderService.cs` (hold/resume/cancel pesanan sebagai pembanding); `RadOperationResult.cs`;
`RadStudy.cs`; `RadStudyConfiguration.cs`; `RadTransitionHistoryConfiguration.cs`;
`RadiologyDtos.cs`; `RadiologyExplicitPermissions.cs`; `contracts/state-transition-matrix.md`;
`contracts/api-contract.md`; `contracts/permission-audit-matrix.md`; laporan `FE-RAD-09`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RadiologyManagement/Controllers/RadStudyController.cs` | Empat action: `MarkRepeatRequired`, `Hold`, `Resume`, `Cancel` |
| `Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs` | Empat method transisi, dua daftar status sah, pembantu `Bersihkan`; `MapStudy` membawa `StatusBeforeHold` dan `ClosureReason` |
| `Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs` | `RadStudyTransitionRequest` (`Reason`, maks. 1.000); dua field tambahan pada `RadStudyResponse` |
| `Tests/QuilvianSystemBackend.RadiologyTests/` (baru) | Proyek uji SQLite: `RadiologyTests.csproj`, `Infrastructure/TestDatabase.cs`, `RadStudyTransitionTests.cs` |
| `docs/module-blueprints/radiologi/contracts/api-contract.md` | Revision 12 — amandemen dan empat baris endpoint |
| `docs/module-blueprints/radiologi/contracts/permission-audit-matrix.md` | Revision 10 — amandemen dan empat baris izin |
| `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` | Blok task bagian 6b dan baris status bagian 0 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat endpoint baru; dua field tambahan pada `RadStudyResponse`. Tidak ada yang dihapus atau diubah — aman bagi pemanggil lama |
| Database | `NOT APPLICABLE`. Kolom `StatusBeforeHold` dan `ClosureReason` sudah ada di tabel `RadStudy` sejak migration awal. Tidak ada migration |
| Keamanan/Auth | Dua hak akses baru: `RadStudy : Hold` (tahan dan lanjutkan) dan `RadStudy : Cancel`. `mark-repeat-required` memakai `RadStudy : Repeat` yang sudah ada. Terdaftar otomatis dari atribut; **belum diberikan ke jabatan mana pun** |

---

## 4. Dokumentasi endpoint

#### Health Services / Radiology Management / Rad Study

Base URL: `api/v1/health-services/radiology-management/rad-studies`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/mark-repeat-required` | Menandai study yang mutunya ditolak sebagai perlu diulang | `RadStudy : Repeat` |
| `POST` | `/{id}/hold` | Menahan study sebelum pengambilan citra | `RadStudy : Hold` |
| `POST` | `/{id}/resume` | Melanjutkan study yang ditahan ke status asalnya | `RadStudy : Hold` |
| `POST` | `/{id}/cancel` | Membatalkan study sebelum pengambilan citra; alasan wajib | `RadStudy : Cancel` |

Request seluruhnya `RadStudyTransitionRequest` (`{ "reason": "..." }`, body boleh kosong kecuali
pembatalan). Response `ApiResponse<RadStudyResponse>`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.RadiologyTests -p:RunAnalyzers=False` (termasuk build proyek utama) | 34/34 lulus, build tanpa error | `PASS` | Keluaran perintah, 2026-10-14 |
| Tandai perlu diulang dari `QualityRejected`, beserta riwayat dan pelaku | Lulus | `PASS` | `TandaiPerluDiulang_DariMutuDitolak_…` |
| Tandai lalu buat study pengulangan; study asal tetap `RepeatRequired` | Lulus | `PASS` | `TandaiPerluDiulang_LaluBuatPengulangan_…` |
| Tandai dari 4 status lain → `409`, nol riwayat | Lulus | `PASS` | Teori, 4 kasus |
| Tahan lalu lanjutkan dari 3 status awal → kembali ke status asal | Lulus | `PASS` | Teori, 3 kasus |
| Tahan dari 6 status tak sah → `409` | Lulus | `PASS` | Teori, 6 kasus |
| Batal tanpa alasan (`null`, kosong, spasi) → `400`, status tetap | Lulus | `PASS` | Teori, 3 kasus |
| Batal dari `OnHold` → `Cancelled`, `ClosureReason` terisi, `StatusBeforeHold` dikosongkan | Lulus | `PASS` | `Batalkan_StudyYangDitahan_…` |
| Batal sesudah acquisition (7 status) dan penjaga `AcquisitionStartedAt` → `409` | Lulus | `PASS` | 8 kasus |
| Controller: `409` / `400` / `200`; route dan pasangan hak akses keempat action | Lulus | `PASS` | 5 kasus |
| HTTP ke backend lokal (superadmin), study acak: `hold`, `resume`, `mark-repeat-required`, `cancel` beralasan → `404 RAD_STUDY_NOT_FOUND`; `cancel` tanpa alasan → `400 RAD_REASON_REQUIRED` | 5/5 sesuai | `PASS` | Nol baris ditulis — semua ditolak sebelum menyentuh data |
| Kueri baca-saja `SysActionAccess` devYoga | `Hold` dan `Cancel` terdaftar, aktif, rute benar; `0` policy | `PASS` | Transaksi `default_transaction_read_only=on` |

Uji manual: `PASS` lewat layar `FE-RAD-09` dengan API tiruan (24/24, lihat laporan `FE-RAD-09`
bagian 9). Transisi sungguhan terhadap study nyata di devYoga **tidak dijalankan** — lihat di bawah.

**Tidak dijalankan:** transisi yang benar-benar mengubah study di devYoga. Database itu dipakai
bersama, dan tidak ada instruksi untuk menulis data uji radiologi. Logikanya sudah dibuktikan
terhadap SQLite dengan konfigurasi EF yang sama.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `QualityRejected` → `RepeatRequired` | Terpenuhi | Uji bagian 5 |
| (2) Tahan dari status sebelum acquisition, status asal disimpan | Terpenuhi | Uji bagian 5 |
| (3) Lanjutkan kembali ke status asal | Terpenuhi | Uji bagian 5 |
| (4) Batal sebelum acquisition, alasan wajib, ditolak bila acquisition pernah dimulai | Terpenuhi | Uji bagian 5 |
| (5) Transisi tidak sah `409`, alasan kosong `400` | Terpenuhi | Uji controller dan HTTP lokal |
| (6) Setiap transisi tercatat di `RadTransitionHistory` beserta pelaku | Terpenuhi | Uji riwayat |
| DoD: kontrak dan matriks izin diperbarui | Terpenuhi | `RAD-API-001` r12, `RAD-PERM-001` r10 |
| DoD: `FE-RAD-09` dapat menyambungkannya | Terpenuhi | Laporan `FE-RAD-09` bagian 9 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Peringatan paket `SixLabors.ImageSharp` (NU1902/NU1903) sudah ada sebelumnya; tidak ada warning kompilasi baru pada berkas Radiologi |
| Masalah yang diketahui | Transisi study lama (`verify-patient`, `complete-acquisition`, `abort-acquisition`, `decide-quality`, `repeat`) membalas transisi tidak sah dengan `400`, bukan `409` seperti kontrak bagian 4. Sengaja tidak disentuh: mengubahnya menggeser perilaku endpoint yang sudah dipakai layar |
| Risiko tersisa | Sejak 2026-10-14 (instruksi pemilik modul) jabatan *Penunjang Medis / Radiografer* memegang 13 izin kerja Radiologi termasuk `Hold`, `Cancel`, `Repeat`. *Kepala Instalasi Radiologi* dan jabatan lain belum memegang izin Radiologi apa pun |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` tiga berkas source Radiologi, `M` dokumen kontrak/roadmap Radiologi dan Rekam Medis (task status hari ini); proyek uji baru berada di `Tests/` yang diabaikan `.gitignore` — perlu `git add -f` bila ingin di-commit |
| Langkah berikutnya | **Terlaksana 2026-10-14:** izin diberikan ke jabatan Radiografer dan tahan–lanjutkan–batal teruji dari layar terhadap study uji di devYoga, 11/11 (laporan `FE-RAD-09` bagian 11.4). Berikutnya: isi katalog pemeriksaan Radiologi yang sungguhan |
