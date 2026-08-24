# Roadmap Delivery Backend — Modul Rawat Inap

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
entity_prefix: Inp
roadmap_revision: 2
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061), jabatan formal belum diisi"
  - "Clinical governance: sebagian terisi (RWI-DEC-064)"
  - "Security/Privacy: OPEN"
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), lewat RWI-DEC-067; sinkronisasi revision 2 lewat RWI-DEC-074"
approved_at: "2026-08-24"
input_revisions:
  blueprint-manifest.md: 4
  00-interview-decisions.md: 6
  01-existing-capability-map.md: 1.2
  02-backend-architecture.md: 0.4
  04-prd-to-mvp.md: 0.4.0
artifact_hashes:
  blueprint-manifest.md: "07f4ed008a53bab5186e0de059ab593b48966ef684d9702216354ba9891ebba0"
  contracts/api-contract.md: "a451e778e37a6596977ce6c2c9e24bc1548cd9dd4efa9a63e642ba02539b709b"
  contracts/state-transition-matrix.md: "35e8e769461a05b32da5d9e6d11ef92dc45c254b2c1a7d4eb08d228a5d9c1fc7"
  contracts/validation-matrix.md: "6ff47efa675605e78bcdb8836fb636bd8744a1c07f2522508aa64261fd3f838d"
  contracts/permission-audit-matrix.md: "50a48e990ac9aaf1d97fc6f7448fd60f513292fd7da717faaaba2eced4d4e19b"
  contracts/integration-contract.md: "e6e86731ae4da27f482e6f659336a74cb0d2d9465f6a04e26fa7bcc6ac331fe1"
  testing/acceptance-test-matrix.md: "357cb6ca9b35b9c2a2ce55597dd2cad5c68bd132c4d40a903f07e4d693b3a45c"
contract_versions:
  - "API 0.4.0"
  - "State transition 0.4.0"
  - "Validation 0.4.0"
  - "Integration 0.4.0"
  - "Permission/Audit 0.4.0"
  - "Acceptance test 0.4.0"
  - "PRD ke MVP 0.4.0"
source_commits:
  backend: "5afb54bd75281648010e50ef14f43ca1f80d8efd"
  frontend: "dec4fdeff07c3c96ad9f07f41f184c54cf771371"
task_count: 33
```

---

## 0. Peringatan yang tidak boleh dilewati

> **Roadmap ini berstatus `APPROVED` sejak 2026-08-24.**
>
> `blueprint-manifest.md` revision `3` disetujui **Muhammad Hamzah** lewat `RWI-DEC-067`, dan
> lifecycle registry modul dinaikkan `PLANNED` → `ACTIVE` lewat `RWI-DEC-068`. Penulisan source
> code dibuka, **satu task per pengerjaan**, mengikuti urutan dependency pada bagian 3.
>
> Yang dibuka: menulis `.cs` untuk task yang dependency-nya sudah selesai. Yang **tetap belum**
> boleh: menerapkan migration ke database selain lokal, dan memulai `BE-RWI-006` sebelum
> `FE-RWI-001` terbukti rilis. Lihat bagian 5 untuk gerbang yang masih terbuka.

Tiga hal yang membedakan modul ini dari modul lain yang pernah dikerjakan:

| Hal | Keadaannya | Akibatnya pada roadmap |
| --- | --- | --- |
| Folder `Areas/HealthServices/InPatientManagement/` | **Tidak ada sama sekali** | Tidak ada satu pun task berjenis "perbaiki". Seluruhnya `MISSING / NEW`, kecuali satu perubahan perilaku pada `BedController` |
| Tidak ada catatan penghunian tempat tidur di sistem hari ini | `RWI-FACT-011`, capability map | Tidak ada migrasi data lama. Tabel lahir kosong, sehingga pembuatan index cepat dan aman |
| Backend hanya punya **satu** berkas test | `QuilvianSystemBackend.Tests/BillingManagement/BillingModuleFoundationTests.cs` | Test bukan pekerjaan terpisah di akhir. `RWI-DEC-051` mewajibkannya menempel pada tiap task |

---

## 1. Cara membaca roadmap ini

Pekerjaan dipecah menjadi **slice**, bukan lapisan teknis. Satu slice adalah satu hasil yang dapat
dirasakan petugas dan dapat diperiksa benar atau salahnya. "Petugas dapat menempatkan pasien dan
sistem menolak tempat tidur yang sudah terisi" adalah slice; "buat semua model" bukan.

Setiap task memakai ID tetap `BE-RWI-nnn`. ID tidak pernah dipakai ulang walaupun task dibatalkan,
supaya rujukan pada laporan lama tidak berubah arti.

Istilah yang dipakai berulang:

| Istilah | Arti |
| --- | --- |
| *Kelayakan Penempatan* | Pemeriksaan berisi delapan aturan yang dipanggil sebelum menempatkan dan sebelum memindahkan pasien. Mengembalikan **daftar aturan yang gagal**, bukan boleh atau tidak |
| *Salinan status tempat tidur* | `MstBed.BedStatus`. Sejak `RWI-DEC-039` ia **bukan** sumber kebenaran; sumbernya adalah `InpBedPlacement` |
| *Kepergian fisik* | Pasien sudah meninggalkan ruangan. Melepas tempat tidur seketika **tanpa** menutup episode |
| *Unique index parsial* | Index unik yang hanya berlaku pada baris yang memenuhi syarat tertentu, misalnya hanya baris penempatan yang masih aktif |

---

## 2. Keadaan awal yang menentukan urutan

| Fakta | Bukti | Akibat pada urutan |
| --- | --- | --- |
| Modul `InPatientManagement` berstatus `PLANNED` pada registry | `RWI-FACT-002` | Status itu hanya memberi hak penamaan. Ia **tidak** memberi izin implementasi |
| Kamar dan tempat tidur belum tentu terisi, dan penandanya belum tentu benar | `RWI-DEC-063`, target 22 Agustus 2026 | `S1` ke atas tidak dapat diuji sama sekali. Ini gerbang implementasi yang masih terbuka |
| Tombol aktif/nonaktif tempat tidur di frontend selalu gagal 404 | `RWI-CON-TRC-001`, `RWI-DEC-049` | `BE-RWI-006` mencabut wewenang admin atas `Reserved` dan `Occupied`. Bila tombol nonaktif masih rusak saat itu, admin kehilangan **satu-satunya** cara menutup tempat tidur rusak |
| Empat modul tetangga tidak punya test regresi | `RWI-DEC-051`, `RWI-RISK-002` | `BE-RWI-032` bukan pekerjaan penutup yang boleh dilewati |

Fakta ketiga yang paling mudah terlewat, jadi contohnya ditulis di sini:

> **Contoh:** `BE-RWI-006` selesai hari Senin. Sejak saat itu admin tidak lagi dapat menyetel
> tempat tidur menjadi `Reserved` atau `Occupied` lewat layar master. Selasa pagi tempat tidur
> `MELATI-03-B` patah dan harus ditutup. Admin membuka layar master, menekan tombol nonaktifkan —
> dan menerima galat 404, karena tombol itu memanggil endpoint yang tidak pernah ada. Tempat tidur
> patah itu tetap muncul pada pencarian tempat tidur kosong, dan pasien berikutnya ditempatkan di
> sana.
>
> Karena itu `FE-RWI-001` pada roadmap frontend adalah **prasyarat lintas repository** bagi
> `BE-RWI-006`, bukan pekerjaan sejajar yang boleh menyusul.

---

## 3. Slice dan milestone

| Slice | Hasil yang dapat diperiksa | Gelombang PRD | Task |
| --- | --- | --- | --- |
| **S0 — Modul benar-benar berdiri** | Tabel ada, master terisi, service terdaftar, endpoint master dapat dipanggil | `MVP-0` | ✅ `BE-RWI-001`; `BE-RWI-002`; ✅ `BE-RWI-003`; `BE-RWI-004` s.d. `BE-RWI-006` |
| **S1 — Petugas dapat membuka admisi dan memesan tempat tidur** | Episode `Draft` lahir bernomor, pemesanan mengunci 2 jam dan gugur sendiri | `MVP-1` | `BE-RWI-007` s.d. `BE-RWI-010` |
| **S2 — Pasien punya lokasi, dan penempatan yang tidak layak ditolak** | Tempat tidur ganda mustahil; jenis kelamin dan isolasi menolak | `MVP-1` | `BE-RWI-011` s.d. `BE-RWI-015` |
| **S3 — Sistem dapat menjawab siapa dirawat di mana** | Census dan lama dirawat | `MVP-1` | `BE-RWI-016` |
| **S4 — Penanggung jawab dan perpindahan** | Riwayat DPJP berperiode, perpindahan utuh | `MVP-2` | `BE-RWI-017` s.d. `BE-RWI-019` |
| **S5 — Pasien dapat dinyatakan boleh pulang** | Keputusan pulang, resume, tanda tangan, versi resume | `MVP-3` | `BE-RWI-020` s.d. `BE-RWI-022` |
| **S6 — Episode dapat ditutup dan tempat tidur kembali kosong** | Lima syarat penutupan, jalan keluar supervisor, kepergian fisik | `MVP-3` | `BE-RWI-023` s.d. `BE-RWI-027` |
| **S7 — Riwayat, daftar pantau, dan koreksi** | Riwayat status tidak dapat dihapus; empat daftar pantau; sesi koreksi | `MVP-4` | `BE-RWI-028` s.d. `BE-RWI-030` |
| **S8 — Bayi baru lahir** | Boks bayi sebagai tempat tidur, hubungan bayi dan ibu | `MVP-4` | `BE-RWI-031` |
| **S9 — Kesiapan sebelum sign-off** | Test regresi modul tetangga, bukti penerimaan lengkap | — | `BE-RWI-032`, `BE-RWI-033` |

### Urutan dependency

```text
BE-RWI-001 (dua tabel master)  ✅ SELESAI
   └── BE-RWI-002 (seeder master) ──┐
   └── BE-RWI-003 (11 tabel + 4 unique index parsial)  ✅ SELESAI
          └── BE-RWI-004 (DI 6 service + setting + nomor episode)
                 ├── BE-RWI-005 (controller master) ──────────────┤
                 │                                                │
                 ├── BE-RWI-007 (buka admisi) ── BE-RWI-008 (ubah/batal/kedaluwarsa Draft)
                 │        └── BE-RWI-009 (daftar & detail episode)
                 │        └── BE-RWI-010 (pemesanan + available-beds + bed-board)
                 │               └── BE-RWI-011 (penempatan + INV-INP-02)
                 │                      ├── BE-RWI-012 (INV-INP-10)
                 │                      ├── BE-RWI-013 (jenis kelamin + boks bayi)   EPIC RI-34 A
                 │                      ├── BE-RWI-014 (atribut isolasi + GUARD-INP-04) EPIC RI-34 B
                 │                      │      └── BE-RWI-015 (aturan 7-8 + daftar pantau)
                 │                      └── BE-RWI-016 (census + lama dirawat)
                 │                             ├── BE-RWI-017 (DPJP) ── BE-RWI-019 (perpindahan)
                 │                             └── BE-RWI-018 (perawat)
                 │                                    └── BE-RWI-020 (keputusan pulang)
                 │                                           └── BE-RWI-021 (resume + tanda tangan)
                 │                                                  └── BE-RWI-022 (versi resume)
                 │                                    BE-RWI-023 (daftar periksa)
                 │                                    BE-RWI-024 (kelayakan keuangan)
                 │                                           └── BE-RWI-025 (closure-readiness + tutup)
                 │                                                  ├── BE-RWI-026 (override)
                 │                                                  └── BE-RWI-027 (kepergian fisik)
                 │                                                         └── BE-RWI-028 (riwayat status)
                 │                                                         └── BE-RWI-029 (4 daftar pantau + selisih)
                 │                                                         └── BE-RWI-030 (sesi koreksi)
                 │                                                         └── BE-RWI-031 (boks bayi + ibu)
                 └── BE-RWI-006 (BedController tolak Reserved/Occupied)  ← butuh FE-RWI-001 lebih dulu

BE-RWI-032 (test regresi modul tetangga) — menempel pada BE-RWI-006, wajib selesai bersamanya
BE-RWI-033 (bukti penerimaan) — paling akhir
```

**Yang boleh paralel.** Setelah `BE-RWI-004` selesai, empat jalur berikut tidak saling bergantung
dan boleh dikerjakan orang berbeda: `BE-RWI-005`, `BE-RWI-007`, `BE-RWI-006` bersama `BE-RWI-032`,
dan penyiapan data master oleh Tim Master Data. Setelah `BE-RWI-011` selesai, `BE-RWI-012`,
`BE-RWI-013`, dan `BE-RWI-014` juga tidak saling bergantung.

**Paralel backend–frontend** diizinkan hanya untuk endpoint yang kontraknya sudah terkunci pada
`API 0.3.0` beserta hash di metadata. Karena seluruh endpoint modul ini berstatus **Rencana (belum
tersedia)**, frontend hanya boleh mendahului backend pada layar master dan pada perbaikan
`FE-RWI-001`. Selebihnya menunggu endpointnya benar-benar ada.

---

## 4. Task

### ✅ `BE-RWI-001` — Dua tabel master Rawat Inap ada di database

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 24 Agustus 2026.** Keempat acceptance criteria dan seluruh butir DoD terbukti. Bukti: [laporan](../task/report/backend/be-rwi-001-tabel-master-rawat-inap.md). Migration **belum** diterapkan ke database mana pun selain Postgres lokal sekali pakai |
| **Outcome** | Sistem punya tempat menyimpan angka batas waktu dan daftar butir administrasi, sehingga tidak ada satu pun angka yang perlu ditanam di kode |
| **Trace** | `RWI-DEC-008`, `RWI-DEC-026`, `RWI-DEC-032`; `02-backend-architecture.md` §4.12, §4.13, §7.1 langkah 1; `erd/02-inpatient-configuration.md` |
| **Reuse** | Pola `MstEmergencySetting` pada `Areas/HealthServices/MasterData/Models/`. Bentuk kolom audit, soft delete, dan konfigurasi EF mengikuti preseden itu apa adanya |
| **Scope** | `MstInpatientSetting.cs`, `MstInpatientClearanceItem.cs`; dua konfigurasi EF; dua `DbSet` pada `ApplicationDbContext`; migration `CreateInpatientMasterTables` |
| **Dependency** | — |
| **Acceptance criteria** | 1. `MstInpatientSetting` memuat kedelapan kolom pada arsitektur §8.1 dengan tipe sesuai kamus data. 2. `MstInpatientClearanceItem` memuat `ItemCode` unik dan `IsMandatory`. 3. Migration maju dan mundur berhasil pada database lokal. 4. Tidak ada tabel modul lain yang tersentuh |
| **Verification** | Uji migration maju dan mundur; bandingkan bentuk kolom terhadap DDL pada `erd/data-dictionary.md` |
| **Risk/blocker** | Migration **tidak boleh** diterapkan ke database mana pun selain lokal tanpa izin tertulis. Owner: Backend/API |
| **DoD** | Dua model, dua konfigurasi, dua `DbSet`, satu migration; uji maju-mundur lulus; build lulus; laporan menyatakan migration belum diterapkan di luar lokal |

---

### `BE-RWI-002` — Data master awal terisi tanpa menebak isi khas rumah sakit

| Field | Isi |
| --- | --- |
| **Outcome** | Modul dapat dinyalakan di lingkungan pengembangan tanpa satu pun layar menampilkan daftar pilihan kosong, dan tanpa seeder ikut mengarang kamar atau tempat tidur yang isinya khas tiap rumah sakit |
| **Trace** | `RWI-DEC-048`; `02-backend-architecture.md` §8.1, §8.2, §8.4; `RWI-AC-108` s.d. `RWI-AC-110` |
| **Reuse** | Pola `PrescriptionReviewCriterionSeeder.cs` dan `Icd10DiagnosisSeeder.cs` pada `Areas/HealthServices/PharmacyManagement/Seeders/`, termasuk cara seeder dipanggil saat aplikasi menyala |
| **Scope** | `InpatientMasterDataSeeder` pada `Areas/HealthServices/MasterData/Seeders/`; pendaftarannya |
| **Dependency** | `BE-RWI-001` |
| **Acceptance criteria** | 1. `MstInpatientSetting` berisi tepat satu baris berkode `DEFAULT` dengan kedelapan nilai pada §8.1. 2. `MstInpatientClearanceItem` berisi tiga butir `ADM-DOC`, `RETURN-ITEM`, `DISCHARGE-MED`, dengan `DISCHARGE-MED` bertanda tidak wajib. 3. Menjalankan seeder dua kali **tidak** menghasilkan data ganda. 4. Seeder **menolak berjalan** di lingkungan produksi. 5. Seeder tidak pernah membuat baris `MstRoom` maupun `MstBed` |
| **Verification** | Integration test: jalankan seeder dua kali lalu hitung barisnya; test yang membuktikan seeder berhenti saat lingkungan produksi; test yang membuktikan `MstBed` tidak bertambah |
| **Risk/blocker** | `InitialAssessmentTargetHours` dan `ProgressNoteVerificationTargetHours` bersumber dari `RWI-RULE-021` yang **belum final secara klinis**. Keduanya di-seed sebagai nilai bawaan yang dapat diubah admin, dan laporan wajib menyebut bahwa angkanya belum disahkan pemilik klinis. Owner: Product/Domain |
| **DoD** | Seeder idempotent dan menolak produksi; ketiga test lulus; laporan mencantumkan isi yang di-seed apa adanya beserta catatan dua angka yang belum final |

---

### ✅ `BE-RWI-003` — Sebelas tabel transaksi beserta empat penjaga keunikannya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 24 Agustus 2026.** Kelima acceptance criteria dan seluruh butir DoD terbukti. Bukti: [laporan](../task/report/backend/be-rwi-003-tabel-transaksi-rawat-inap.md). Migration **belum** diterapkan ke database mana pun selain Postgres lokal sekali pakai |
| **Outcome** | Fondasi data seluruh modul berdiri, dan empat keadaan mustahil benar-benar dijadikan mustahil oleh database, bukan hanya oleh kode |
| **Trace** | `02-backend-architecture.md` §3, §4.1 s.d. §4.11, §6, §7.1 langkah 3 dan 4; `INV-INP-01` s.d. `INV-INP-10`; `erd/01-inpatient-episode.md`; `erd/data-dictionary.md` |
| **Reuse** | Pola tabel transaksi berawalan `Trx` pada `EmergencyInstallationManagement`, termasuk kolom audit dan soft delete. Bentuk unique index parsial mengikuti preseden yang sudah dipakai project |
| **Scope** | Sebelas model `Inp*`; sebelas konfigurasi EF pada `Repositories/Configurations/HealthService/InPatientManagement/`; sebelas `DbSet`; enum `InpIsolationSource`, `InpBedPlacementEndReason`, `InpEpisodeStatus`; migration `CreateInpatientTransactionTables` |
| **Dependency** | `BE-RWI-001` |
| **Catatan revision `4`** | `RWI-DEC-073` menempatkan kolom `TrxPatientEncounter.OriginEncounterId` sebagai pekerjaan **modul IGD**, bukan modul ini. Acceptance criteria nomor 5 di bawah karena itu **tetap utuh dan tetap dapat diuji**: migration task ini tidak boleh menyentuh satu kolom pun milik tabel modul lain |
| **Acceptance criteria** | 1. Kesebelas tabel terbentuk sesuai kamus data, termasuk enam kolom kebutuhan isolasi pada `InpEpisode`. 2. **Empat unique index parsial** terbentuk: penempatan aktif per tempat tidur, pemesanan aktif per tempat tidur, DPJP aktif per episode, dan episode hadir per pasien. 3. `InpEpisodeStatus` memuat tepat **lima** nilai; `InCare` tidak ada. 4. Migration maju dan mundur berhasil. 5. Tidak ada kolom tabel modul lain yang berubah |
| **Verification** | Uji migration maju-mundur; **empat** test yang masing-masing mencoba menyisipkan baris kedua yang melanggar satu index parsial dan membuktikan database menolaknya; unit test yang menghitung jumlah nilai enum status |
| **Risk/blocker** | Index parsial adalah satu-satunya pertahanan terhadap tabrakan dua petugas. Bila dialek database yang dipakai tidak mendukungnya, **berhenti dan naikkan ke pemilik arsitektur** — jangan diganti dengan pemeriksaan di kode saja, karena pemeriksaan di kode dapat dilewati dua transaksi bersamaan. Owner: Backend/API |
| **DoD** | Sebelas tabel, empat index parsial, satu migration; keempat test penolakan lulus; uji maju-mundur lulus; kamus data dan kenyataan database cocok kolom demi kolom |

---

### `BE-RWI-004` — Enam service terdaftar dan angka pengaturan terbaca dari master

| Field | Isi |
| --- | --- |
| **Outcome** | Controller yang dibuat task berikutnya benar-benar dapat dijalankan, dan seluruh angka batas waktu dibaca dari master — bukan ditanam di kode |
| **Trace** | `02-backend-architecture.md` §4.14 s.d. §4.19, §7.1 langkah 6; `RWI-DEC-008`; `RWI-AC-003` |
| **Reuse** | `builder.Services.AddScoped<TService>()` pada `Program.cs`, pola yang sudah dipakai puluhan service lain. `InpEpisodeNumberService` mengikuti `EmergencyDocumentNumberService` yang sudah ada |
| **Scope** | Kerangka enam service; `InpSettingService` dan `InpEpisodeNumberService` **terisi penuh**; empat service lain baru kerangkanya; pendaftaran pada `Program.cs` |
| **Dependency** | `BE-RWI-003` |
| **Acceptance criteria** | 1. Aplikasi menyala tanpa galat. 2. Keenam service dapat diminta dari container. 3. `InpSettingService` membaca baris `DEFAULT`; bila baris itu belum ada, ia mengembalikan nilai bawaan **dan** mencatat peringatan. 4. Nomor episode memakai awalan dari `MstInpatientSetting.EpisodeNumberPrefix`, bukan huruf yang ditanam di kode. 5. Dua permintaan nomor bersamaan tidak menghasilkan nomor kembar |
| **Verification** | Test aktivasi yang meminta keenam service dari container; unit test tiga kasus `InpSettingService` — baris ada, baris tidak ada, nilai diubah admin; test dua permintaan nomor bersamaan |
| **Risk/blocker** | Bila `InpSettingService` diam-diam memakai nilai bawaan di produksi tanpa peringatan, angka yang salah akan terpakai berbulan-bulan tanpa ada yang tahu. Peringatan itu **bukan** hiasan. Owner: Backend/API |
| **DoD** | Enam service terdaftar; dua terisi penuh; test aktivasi dan tiga unit test lulus; build lulus |

---

### `BE-RWI-005` — Admin dapat mengubah pengaturan dan butir administrasi lewat layar

| Field | Isi |
| --- | --- |
| **Outcome** | Batas waktu pemesanan, ambang daftar pantau, dan daftar butir administrasi dapat diubah admin tanpa satu baris kode pun disentuh, dan nilai barunya berlaku pada pembacaan berikutnya |
| **Trace** | `RWI-DEC-008`, `RWI-DEC-026`, `RWI-DEC-032`; api contract `0.3.0` bagian Inpatient Setting dan Inpatient Clearance Item (8 endpoint); validation matrix bagian pengaturan; `RWI-AC-003`, `RWI-AC-105` s.d. `RWI-AC-107` |
| **Reuse** | Pola controller CRUD master data yang sudah ada pada `Areas/HealthServices/MasterData/Controllers/`. Tidak memakai service, sesuai konvensi project untuk CRUD sederhana |
| **Scope** | `InpatientSettingController.cs`, `InpatientClearanceItemController.cs`; DTO keduanya; butir hak akses `InpatientSetting` dan `InpatientClearanceItem` pada `AccessMenuSeeder` |
| **Dependency** | `BE-RWI-004` |
| **Acceptance criteria** | 1. Kedelapan endpoint sesuai api contract `0.3.0` bentuk dan hak aksesnya. 2. Mengubah `BedReservationMinutes` membuat pemesanan **berikutnya** memakai nilai baru; pemesanan yang sudah berjalan tidak berubah. 3. Menambah baris pengaturan kedua ditolak. 4. Menonaktifkan butir wajib tidak menghapus penandaan yang sudah ada pada episode lama. 5. Butir dengan `ItemCode` kembar ditolak. 6. Tanpa hak akses, ditolak 403 |
| **Verification** | Integration test per endpoint; test yang mengubah angka lalu membuktikan pemesanan lama tidak ikut berubah; test butir kembar |
| **Risk/blocker** | Butir hak akses baru didaftarkan otomatis `AccessMenuSeeder` saat aplikasi menyala. Bila seeder itu tidak dijalankan di lingkungan uji, seluruh test akses akan gagal karena alasan yang salah. Owner: Backend/API |
| **DoD** | Dua controller, delapan endpoint, hak akses terdaftar; keenam kriteria lulus; api contract diperbarui dari "Rencana" menjadi tersedia |

---

### `BE-RWI-006` — Status terisi dan dipesan hanya lahir dari modul Rawat Inap

| Field | Isi |
| --- | --- |
| **Outcome** | Admin master data tidak lagi dapat menyetel tempat tidur menjadi `Reserved` atau `Occupied` lewat layar master, sehingga satu-satunya sumber kebenaran penghunian adalah catatan penempatan. Admin tetap dapat menutup tempat tidur rusak lewat `Cleaning`, `Maintenance`, `Blocked`, dan `Inactive` |
| **Trace** | `RWI-DEC-039`, `RWI-RULE-027`, `RWI-DEC-062`; api contract `0.3.0` bagian 7; validation matrix bagian 8; `EPIC RI-32`; `RWI-AC-060`, `RWI-AC-061` |
| **Reuse** | `BedController.UpdateBedAvailability` yang sudah ada pada baris 514–519. Tidak ada endpoint baru dan tidak ada perubahan kolom |
| **Scope** | `Areas/HealthServices/MasterData/Controllers/BedController.cs` saja, hanya badan aksi `/availability` |
| **Dependency** | `BE-RWI-004`; **dan `FE-RWI-001` pada roadmap frontend wajib sudah selesai** |
| **Acceptance criteria** | 1. Mengirim `Reserved` atau `Occupied` ditolak 422 dengan pesan persis seperti validation matrix bagian 8. 2. Mengirim `Cleaning`, `Maintenance`, `Blocked`, `Inactive` tetap diterima. 3. Tempat tidur yang sedang ditempati **tidak** dapat disetel `Maintenance` selama penempatannya masih aktif. 4. Jalur lama untuk keempat nilai yang masih diizinkan tidak berubah bentuk balasannya |
| **Verification** | Integration test per nilai status; **test regresi** yang membuktikan layar master tempat tidur yang sudah ada tetap berfungsi untuk keempat nilai yang diizinkan |
| **Risk/blocker** | **Ini satu-satunya perubahan perilaku pada modul milik pihak lain.** `RWI-DEC-062` sudah memberi persetujuan pemiliknya. Risiko sisanya teknis: bila `FE-RWI-001` belum selesai, admin kehilangan satu-satunya cara menutup tempat tidur rusak — lihat contoh pada bagian 2. Owner: Backend/API bersama Product/Domain |
| **DoD** | Perubahan perilaku selesai; keempat kriteria lulus; test regresi lulus; `FE-RWI-001` terbukti sudah rilis; laporan menyatakan ini perubahan perilaku, bukan penambahan fitur |

---

### `BE-RWI-007` — Petugas admisi dapat membuka admisi dan episode lahir bernomor

| Field | Isi |
| --- | --- |
| **Outcome** | Satu pasien terdaftar dapat dijadikan pasien rawat inap. Episode lahir berstatus `Draft` dengan nomor yang terbaca manusia, menempel pada satu kunjungan, dan sudah punya DPJP sejak detik pertama |
| **Trace** | `RWI-DEC-009`, `RWI-DEC-011`, `RWI-DEC-041`; `INV-INP-03`, `INV-INP-04`; api contract `POST /episodes`; state matrix bagian episode; validation matrix bagian 1; `RWI-AC-001`, `RWI-AC-004` s.d. `RWI-AC-006` |
| **Reuse** | `TrxPatientEncounter` dipakai apa adanya sebagai jangkar; tidak ada tabel kunjungan tandingan. Pola `EmergencyVisitService` untuk membuka dokumen bernomor |
| **Scope** | `InpEpisodeService.OpenAdmissionAsync` dan `ApplyStatusChangeAsync`; `InpatientEpisodeController` aksi `POST /`; DTO `OpenAdmissionRequest` dan `InpatientEpisodeDetailResponse`; penulisan baris `InpStatusHistory` pertama |
| **Dependency** | `BE-RWI-004` |
| **Acceptance criteria** | 1. Episode lahir `Draft` dengan nomor berawalan dari master. 2. Membuka admisi tanpa DPJP ditolak 400; `INV-INP-03` tidak pernah dilanggar. 3. Membuka admisi pada kunjungan yang sudah punya episode ditolak 409; `INV-INP-04` tidak pernah dilanggar. 4. Untuk pasien yang datang langsung, kunjungan bertipe rawat inap dibuat otomatis. 5. Setiap perubahan status menulis satu baris `InpStatusHistory` **di dalam transaksi yang sama**. 6. Membuka admisi untuk pasien yang punya episode `Draft` lain **berhasil**, disertai peringatan — bukan penolakan |
| **Verification** | Integration test keenam kriteria; satu test yang memaksa kegagalan di tengah transaksi dan membuktikan episode maupun baris riwayat sama-sama tidak tersimpan |
| **Risk/blocker** | `ApplyStatusChangeAsync` harus menjadi **satu-satunya** tempat status berubah. Bila satu controller saja menyetel `EpisodeStatus` langsung, riwayat status berlubang dan seluruh laporan pengecualian ikut salah. Tegakkan lewat review, bukan harapan. Owner: Backend/API |
| **DoD** | Endpoint sesuai kontrak `0.3.0`; keenam kriteria lulus; test transaksi gagal lulus; api contract diperbarui |

---

### `BE-RWI-008` — Admisi dapat diperbaiki, dibatalkan, dan gugur sendiri bila ditinggalkan

| Field | Isi |
| --- | --- |
| **Outcome** | Isian admisi yang salah dapat dibetulkan selagi masih `Draft`; admisi yang batal dapat ditutup rapi beserta pemesanannya; dan admisi yang ditinggalkan tidak menyandera tempat tidur selamanya |
| **Trace** | `RWI-DEC-010`, `RWI-DEC-030`; `RWI-RULE-004`, `RWI-RULE-022`; api contract `PUT /episodes/{id}` dan `PATCH /episodes/{id}/cancel`; state matrix; `RWI-AC-007` s.d. `RWI-AC-010`, `RWI-AC-090` s.d. `RWI-AC-092` |
| **Reuse** | Pola kedaluwarsa **dihitung saat dibaca**, tanpa program penjadwal — sama seperti kedaluwarsa pemesanan pada `RWI-RULE-002` |
| **Scope** | `InpEpisodeService.UpdateAdmissionAsync`, `CancelAdmissionAsync`, dan perhitungan kedaluwarsa `Draft`; aksi controller terkait |
| **Dependency** | `BE-RWI-007` |
| **Acceptance criteria** | 1. Mengubah isian episode yang bukan `Draft` ditolak. 2. Pembatalan sebelum ada catatan klinis berhasil dan melepas pemesanan serta penempatan dalam satu tindakan utuh. 3. Pembatalan setelah `Admitted` hanya oleh supervisor atau kepala ruangan; peran lain ditolak 403. 4. Episode `Draft` yang ditinggalkan lebih dari `DraftEpisodeExpiryHours` terbaca `Cancelled` pada pembacaan berikutnya, **tanpa** program penjadwal. 5. Kunjungan yang ikut lahir bersama episode itu ikut dibatalkan. 6. Batas jamnya dapat diubah admin dan berlaku pada pembacaan berikutnya |
| **Verification** | Integration test keenam kriteria; test dua pembacaan pada waktu berbeda yang membuktikan tidak ada penjadwal yang dijalankan |
| **Risk/blocker** | Kedaluwarsa yang dihitung saat dibaca berarti episode `Draft` basi tetap ada di tabel sampai seseorang membacanya. Laporan yang menghitung baris langsung dari tabel tanpa melewati service akan salah hitung. Owner: Backend/API |
| **DoD** | Tiga kemampuan selesai; keenam kriteria lulus; api contract diperbarui |

---

### `BE-RWI-009` — Daftar dan detail episode dapat dibaca dan disaring

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas dapat menemukan episode yang dicarinya tanpa menebak, dan melihat satu episode utuh beserta DPJP aktif, perawat aktif, dan lokasi terkininya |
| **Trace** | Api contract `GET /episodes`, `/{id}`, `/summary`, `/filters/metadata`; permission matrix `InpatientEpisode : Read`; privasi pada `03-frontend-architecture.md` bagian 6 |
| **Reuse** | Pola query `AsNoTracking` dengan projection langsung ke DTO, dan pola `filters/metadata` yang sudah dipakai modul lain |
| **Scope** | `InpEpisodeService` bagian baca; empat aksi `InpatientEpisodeController`; DTO daftar, detail, ringkasan, dan metadata penyaring |
| **Dependency** | `BE-RWI-007` |
| **Acceptance criteria** | 1. Daftar dapat disaring unit layanan, status, rentang tanggal, dan nama pasien. 2. Detail menampilkan DPJP aktif, perawat aktif, dan lokasi terkini yang dibaca dari `InpBedPlacement` — **bukan** dari kolom lokasi pada episode. 3. Ringkasan menghitung jumlah per status. 4. Kolom sensitif tidak ikut pada daftar, hanya pada detail bagi peran yang berhak. 5. Tanpa hak akses, ditolak 403 |
| **Verification** | Integration test per penyaring; test yang membuktikan daftar tidak memuat kolom sensitif; test 403 |
| **Risk/blocker** | Godaan menyimpan "lokasi terakhir" sebagai kolom pada episode akan muncul di sini karena query-nya lebih murah. Arsitektur **melarangnya** — lokasi selalu dibaca dari catatan penempatan. Owner: Backend/API |
| **DoD** | Empat endpoint sesuai kontrak; kelima kriteria lulus; api contract diperbarui |

---

### `BE-RWI-010` — Tempat tidur dapat dicari dan dipesan, dan pemesanan gugur sendiri

| Field | Isi |
| --- | --- |
| **Outcome** | Dua petugas tidak lagi merebut tempat tidur yang sama. Tempat tidur terkunci 2 jam untuk satu episode, lalu bebas sendiri bila pasiennya tidak kunjung datang |
| **Trace** | `RWI-DEC-008`; `RWI-RULE-001`, `RWI-RULE-002`; `INV-INP-02` sebagian; api contract `/available-beds`, `/bed-board`, `POST /reservations`, `PATCH /reservations/{id}/cancel`; `RWI-AC-001` s.d. `RWI-AC-003` |
| **Reuse** | `MstBed`, `MstRoom`, `MstServiceUnit`, `MstPatientClass` dipakai apa adanya lewat Id. Tidak ada master baru |
| **Scope** | `InpBedOccupancyService` bagian pencarian dan pemesanan; `EvaluatePlacementEligibility` **aturan 1 sampai 3** saja; empat aksi `InpatientBedOccupancyController` |
| **Dependency** | `BE-RWI-004`; data master kamar dan tempat tidur sudah terisi |
| **Acceptance criteria** | 1. Tempat tidur berstatus `Reserved` tidak muncul pada pencarian tempat tidur kosong. 2. Pemesanan pukul 09:15 masih mengunci pada pembacaan 11:14 dan sudah bebas pada pembacaan 11:16, **tanpa** program penjadwal. 3. Batas 2 jam dapat diubah admin dan nilai barunya dipakai pemesanan berikutnya. 4. Memesan tempat tidur yang sudah dipesan episode lain ditolak 409. 5. Memesan tempat tidur berstatus `Maintenance` ditolak 422 dengan pesan yang menyebut keadaan tempat tidurnya. 6. Papan ketersediaan mengelompokkan per unit layanan dan kamar |
| **Verification** | Integration test keenam kriteria; test dua pembacaan waktu berbeda; test unique index parsial pemesanan aktif |
| **Risk/blocker** | Bila master tempat tidur belum terisi, seluruh test task ini gagal karena alasan yang salah — bukan karena kodenya salah. Pastikan `RWI-DEC-063` sudah tuntas lebih dulu. Owner: Backend/API bersama Tim Master Data |
| **DoD** | Empat endpoint sesuai kontrak; keenam kriteria lulus; api contract diperbarui |

---

### `BE-RWI-011` — Pasien punya lokasi, dan tempat tidur ganda mustahil terjadi

| Field | Isi |
| --- | --- |
| **Outcome** | Pasien yang sampai di kamar tercatat lokasinya, episode menjadi aktif, dan dua petugas yang menekan tombol pada saat hampir bersamaan tidak pernah menghasilkan dua penempatan di satu tempat tidur |
| **Trace** | `RWI-DEC-021`, `RWI-DEC-039`, `RWI-DEC-072`; `RWI-RULE-015`, `RWI-RULE-027`, `RWI-RULE-029` aturan 8; `INV-INP-01`, `INV-INP-02`; api contract `0.4.0` `POST /placements`; `RWI-AC-059`, `RWI-AC-062`, `RWI-AC-147` |
| **Reuse** | Unique index parsial dari `BE-RWI-003`. Salinan `MstBed.BedStatus` ditulis lewat jalur yang sama dengan modul master, bukan lewat SQL langsung |
| **Scope** | `InpBedOccupancyService.PlacePatientAsync`; pemanggilan `InpEpisodeService.ApplyStatusChangeAsync` menjadi `Admitted`; penulisan salinan `MstBed.BedStatus` **dalam transaksi yang sama** |
| **Dependency** | `BE-RWI-010` |
| **Acceptance criteria** | 1. Setelah penempatan, sistem menjawab siapa menempati dan sejak jam berapa. 2. **Dua transaksi bersamaan pada satu tempat tidur:** satu berhasil, satu ditolak 409, dan tepat **satu** baris penempatan aktif tersimpan. 3. Bila penulisan salinan status gagal, catatan penempatan juga tidak tersimpan dan episode tetap `Draft`. 4. Keadaan tempat tidur diperiksa **ulang** saat penempatan, bukan hanya saat pemesanan. 5. Penolakan penempatan **tidak** menghapus isian admisi yang sudah diisi. 6. Pemesanan milik episode ini yang masih berlaku dipakai, bukan ditolak. 7. `RWI-AC-147` — untuk jalur datang langsung dan poliklinik, waktu mulai penempatan tetap waktu penempatan dibuat dan tidak menunggu apa pun |
| **Verification** | Integration test dua transaksi bersamaan — ini yang paling penting dan tidak boleh dilewati; test kegagalan di tengah transaksi; test yang membuktikan isian admisi utuh setelah penolakan |
| **Catatan revision `4`** | `RWI-DEC-072` menambah aturan 9 pada Kelayakan Penempatan — penempatan pasien asal IGD menunggu event `Tiba` milik IGD. Aturan itu **tidak menyala pada task ini**, karena hanya berlaku bila `TrxPatientEncounter.OriginEncounterId` terisi, dan jalur itu adalah `INP-S09` yang di luar MVP. Yang wajib dikerjakan di sini hanya kriteria 7 sebagai penjaga |
| **Risk/blocker** | Pemeriksaan "tempat tidur kosong" di dalam kode **tidak cukup** — dua transaksi dapat sama-sama lolos pemeriksaan sebelum salah satunya menyimpan. Penguncian baris ditambah unique index parsial adalah pertahanan sebenarnya. Owner: Backend/API |
| **DoD** | Endpoint sesuai kontrak; keenam kriteria lulus; test tabrakan lulus; api contract diperbarui |

---

### `BE-RWI-012` — Satu pasien tidak pernah tercatat dirawat di dua tempat

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas yang mengira pasien adalah pasien baru tidak dapat membuat penempatan kedua. Pesan penolakannya langsung memberi tahu bahwa yang dibutuhkan adalah perpindahan, bukan admisi baru |
| **Trace** | `RWI-DEC-054`; `RWI-RULE-035`; `INV-INP-10`; `02-backend-architecture.md` §1.5; `FR-RI-148`; `RWI-AC-116`, `RWI-AC-117`; `UAT-26` |
| **Reuse** | Unique index parsial atas `PatientId` untuk episode yang hadir, dibuat pada `BE-RWI-003` |
| **Scope** | Pemeriksaan `INV-INP-10` di dalam `InpEpisodeService` sebelum penempatan; pesan penolakan yang menyebut nomor episode dan lokasi |
| **Dependency** | `BE-RWI-011` |
| **Acceptance criteria** | 1. Menempatkan pasien yang sudah punya episode `Admitted` ditolak 409, disertai **nomor episode dan lokasi** yang sedang ditempati. 2. Membuka admisi untuk pasien yang punya episode `Draft` lain tetap **berhasil** disertai peringatan. 3. Menempatkan pasien yang episode lamanya `DischargePending` dan kepergiannya **belum** dicatat ditolak 409. 4. Menempatkan pasien yang episode lamanya `DischargePending` tetapi kepergiannya **sudah** dicatat **berhasil** |
| **Verification** | Integration test keempat kriteria; kriteria 3 dan 4 wajib berpasangan dalam satu berkas test supaya batasnya terbaca jelas oleh pembaca berikutnya |
| **Risk/blocker** | Kriteria 4 adalah kebalikan kriteria 3 dan sama pentingnya: yang pertama mencegah data ganda, yang kedua mencegah pasien tertahan oleh urusan administrasi. Menguji salah satunya saja menghasilkan rasa aman yang palsu. Owner: Backend/API |
| **DoD** | Pemeriksaan aktif; keempat kriteria lulus; pesan penolakan sesuai validation matrix |

---

### `BE-RWI-013` — Kamar tidak pernah menjadi campur laki-laki dan perempuan

| Field | Isi |
| --- | --- |
| **Outcome** | Sistem menolak menempatkan pasien pada tempat tidur atau kamar yang secara privasi tidak layak baginya, walaupun petugas memaksa. Bayi pada boks bayi dikecualikan dari kedua sisi pemeriksaan |
| **Trace** | `RWI-DEC-064`, `RWI-DEC-066`; `RWI-RULE-012` bagian B; `EPIC RI-34`, `FR-RI-154` s.d. `FR-RI-157`; validation matrix bagian 4; test matrix bagian 2A.1 dan 2A.2; `RWI-AC-128` s.d. `RWI-AC-133`; `UAT-29`, `UAT-30` |
| **Reuse** | Kolom `MstBed.IsForMale`, `IsForFemale`, `IsForNewborn`, dan `RoomId` **sudah ada** di source hari ini — terbukti pada `MstBed.cs` baris 14, 29, 31, 33. Tidak ada kolom baru pada modul mana pun |
| **Scope** | `EvaluatePlacementEligibility` **aturan 4, 5, dan 6** beserta dua pengecualian boks bayi; penyaring pada `GET /available-beds` |
| **Dependency** | `BE-RWI-011` |
| **Acceptance criteria** | 1. Penempatan pasien perempuan ke tempat tidur bertanda hanya laki-laki ditolak 422. 2. Penempatan ke kamar yang sudah dihuni jenis kelamin berbeda ditolak 422, dan **pesannya menyebut nama kamarnya**. 3. Pasien berikutnya berjenis kelamin sama **diterima** — aturannya menolak pencampuran, bukan menolak kamar berpenghuni. 4. Jenis kelamin belum tercatat: hanya boleh ke tempat tidur yang menerima keduanya **dan** kamar yang belum berpenghuni; gagal salah satu saja ditolak. 5. Bayi laki-laki ke boks bayi di kamar ibunya **berhasil**. 6. Penghuni boks bayi **tidak dihitung** saat memeriksa pencampuran. 7. Kamar berisi satu tempat tidur tidak pernah tersentuh aturan pencampuran |
| **Verification** | Integration test ketujuh kriteria; kriteria 5 dan 6 wajib berpasangan supaya sifat dua arahnya terbukti; test yang membuktikan hasil `available-beds` dan hasil penolakan **sama** — penyaring dan penolak tidak boleh berbeda jawaban |
| **Risk/blocker** | Aturan 6 diperiksa dari **penghuni yang sedang ada**, bukan dari penanda `MstRoom`. `RWI-DEC-066` menolak kolom "boleh campur" secara tegas, dan itu terkunci pada `blueprint-manifest.md` bagian 8 butir 7. Menambahkannya bukan keputusan pelaksana. Owner: Backend/API bersama Product/Domain |
| **DoD** | Tiga aturan dan dua pengecualian aktif; ketujuh kriteria lulus; validation matrix dan kenyataan pesan cocok kata demi kata |

---

### `BE-RWI-014` — Kebutuhan isolasi tercatat pada episode dengan pemiliknya jelas

| Field | Isi |
| --- | --- |
| **Outcome** | Sistem akhirnya punya tempat untuk mencatat bahwa seorang pasien membutuhkan isolasi. Petugas admisi boleh merekam keterangan dokter pengirim supaya penempatan tidak menunggu pengkajian klinis, tetapi keputusan klinisnya tetap milik DPJP dan dapat dibedakan dari catatan awal |
| **Trace** | `RWI-DEC-065`; `RWI-RULE-012` bagian A aturan 1 s.d. 4; `GUARD-INP-04`; api contract `PATCH /episodes/{id}/isolation-requirement`; permission matrix; validation matrix bagian 4A; `FR-RI-158`, `FR-RI-159`; `RWI-AC-136`, `RWI-AC-137`, `RWI-AC-139`; `UAT-32` |
| **Reuse** | Enam kolom dan enum `InpIsolationSource` sudah dibuat pada `BE-RWI-003`. Pola penjaga kewenangan mengikuti `GUARD-INP-01` yang ditulis di dalam service, bukan di mesin hak akses |
| **Scope** | `InpEpisodeService.SetIsolationRequirementAsync` beserta `GUARD-INP-04`; aksi `PATCH /{id}/isolation-requirement`; DTO `SetIsolationRequirementRequest`; hak akses `InpatientEpisode : SetIsolation` |
| **Dependency** | `BE-RWI-011` |
| **Acceptance criteria** | 1. Petugas admisi menyalakan selagi `Draft`: tersimpan dengan `IsolationSource = AdmissionRecord`, `IsolationSetByUserId` terisi, `IsolationSetByDoctorId` **kosong**. 2. DPJP aktif mengubah setelah `Admitted`: tersimpan dengan `IsolationSource = ClinicalDecision` dan `IsolationSetByDoctorId` terisi. 3. Dokter yang **bukan** DPJP aktif ditolak 403 — membuktikan `GUARD-INP-04`. 4. Petugas admisi mengubah setelah `Admitted` ditolak 403 — wewenangnya berhenti, tidak berlaku selamanya. 5. Menyalakan tanpa mengisi keterangan ditolak 400. 6. Peran di luar admisi dan dokter ditolak 403 oleh mesin hak akses, sebelum service dijalankan |
| **Verification** | Integration test keenam kriteria; kriteria 3 dan 4 wajib berpasangan supaya terlihat bahwa yang membedakan adalah **status episode**, bukan sekadar peran |
| **Risk/blocker** | Mesin hak akses menjawab `SetIsolation` dengan "boleh" untuk petugas admisi **dan** untuk dokter mana pun. Yang membedakan adalah status episode dan siapa DPJP aktifnya. Bila `GUARD-INP-04` dilupakan, dokter jaga mana pun dapat mengubah keputusan pengendalian infeksi milik DPJP lain. Owner: Backend/API bersama Clinical governance |
| **DoD** | Endpoint dan penjaga aktif; keenam kriteria lulus; permission matrix dan kenyataan cocok; api contract diperbarui |

---

### `BE-RWI-015` — Kapasitas isolasi terjaga dari dua arah, tanpa menahan pencatatan klinis

| Field | Isi |
| --- | --- |
| **Outcome** | Pasien yang membutuhkan isolasi hanya boleh di tempat tidur isolasi, dan tempat tidur isolasi tidak terpakai sia-sia oleh pasien biasa. Ketika kondisi klinis berubah di tengah perawatan, pencatatannya **tidak pernah ditahan** — yang muncul adalah daftar pantau |
| **Trace** | `RWI-DEC-064`, `RWI-DEC-065` aturan 5 s.d. 7; `RWI-RULE-012` bagian A; api contract `GET /monitoring/isolation-mismatch`; `FR-RI-160`, `FR-RI-161`; test matrix bagian 2A.3 dan 2A.5; `RWI-AC-134`, `RWI-AC-135`, `RWI-AC-138`; `UAT-31`, `UAT-33` |
| **Reuse** | Kolom `MstBed.IsIsolationBed` **sudah ada** — terbukti pada `MstBed.cs` baris 35 |
| **Scope** | `EvaluatePlacementEligibility` **aturan 7 dan 8**; daftar pantau penempatan tidak sesuai pada `InpCensusQueryService`; aksi `GET /monitoring/isolation-mismatch` |
| **Dependency** | `BE-RWI-014` |
| **Acceptance criteria** | 1. Pasien butuh isolasi ke tempat tidur bukan isolasi ditolak 422 dengan pesan yang menyebut kebutuhan isolasinya. 2. Pasien **tidak** butuh isolasi ke tempat tidur isolasi ditolak 422 dengan pesan berbeda yang menyebut kapasitas isolasi. 3. Pasien butuh isolasi ke tempat tidur isolasi **berhasil**. 4. Menyalakan kebutuhan isolasi saat pasien berada di tempat tidur biasa **diterima, tidak ditolak**, dan episodenya muncul pada daftar pantau. 5. Setelah dipindahkan ke tempat tidur isolasi, episode itu **hilang** dari daftar pantau. 6. Kebalikannya juga bekerja: mematikan kebutuhan isolasi saat pasien di tempat tidur isolasi memunculkan episode pada daftar pantau. 7. Daftar pantau yang kosong mengembalikan daftar kosong, bukan galat |
| **Verification** | Integration test ketujuh kriteria; kriteria 1 dan 2 wajib memeriksa **isi pesannya**, bukan hanya kode 422, karena keduanya berkode sama tetapi artinya berlawanan |
| **Risk/blocker** | Kriteria 4 adalah yang paling mudah dikerjakan terbalik. Menahan pencatatan klinis demi menjaga aturan penempatan adalah urutan yang salah: fakta klinis dicatat lebih dulu, lalu sistem menunjukkan penempatannya perlu dibetulkan. Owner: Backend/API bersama Clinical governance |
| **DoD** | Dua aturan dan satu daftar pantau aktif; ketujuh kriteria lulus; api contract diperbarui |

---

### `BE-RWI-016` — Sistem dapat menjawab siapa dirawat, di mana, dan sudah berapa hari

| Field | Isi |
| --- | --- |
| **Outcome** | Perawat membuka satu layar dan langsung tahu seluruh pasien yang sedang dirawat beserta lokasi, DPJP, perawat penanggung jawab, dan lama dirawatnya |
| **Trace** | `RWI-DEC-027`; `RWI-RULE-019`; api contract bagian Census (3 endpoint); `FR-RI-113` s.d. `FR-RI-115`; `RWI-AC-064`; `UAT-05`, `UAT-06` |
| **Reuse** | Query `AsNoTracking` dengan projection langsung ke DTO. Census **tidak** disimpan sebagai tabel; selalu dihitung dari penempatan yang masih aktif |
| **Scope** | `InpCensusQueryService` bagian census dan lama dirawat; tiga aksi `InpatientCensusController` |
| **Dependency** | `BE-RWI-011` |
| **Acceptance criteria** | 1. Census menampilkan episode `Admitted` dan `DischargePending` saja; dari lima episode berstatus berbeda, census memuat tepat dua. 2. Lama dirawat dihitung dari **selisih tanggal** dengan hasil paling sedikit 1 hari: masuk 21 September 22:30 dan pulang 22 September 06:00 menghasilkan **1 hari**, bukan 0. 3. Lama dirawat bertambah pada pergantian tanggal, bukan setiap genap 24 jam. 4. Pasien yang kepergiannya sudah dicatat **tidak** muncul pada census. 5. Ringkasan menghitung per unit layanan dan per kelas |
| **Verification** | Unit test perhitungan lama dirawat tiga kasus batas; integration test census lima status; test pasien yang sudah pergi |
| **Risk/blocker** | Kriteria 4 baru dapat diuji penuh setelah `BE-RWI-027`. Sampai saat itu, tulis test-nya dengan menyetel kolom kepergian langsung dan tandai bahwa jalur endpoint-nya diuji ulang pada `BE-RWI-027`. Owner: Backend/API |
| **DoD** | Tiga endpoint sesuai kontrak; kelima kriteria lulus; unit test perhitungan lulus; api contract diperbarui |

---

### `BE-RWI-017` — Sistem dapat menjawab siapa DPJP pada tanggal tertentu

| Field | Isi |
| --- | --- |
| **Outcome** | DPJP berbentuk riwayat berperiode, bukan satu kolom yang ditimpa. Ketika auditor bertanya siapa yang berwenang pada 22 September, sistem masih dapat menjawabnya pada 25 September |
| **Trace** | `RWI-DEC-022`, `RWI-DEC-024`; `RWI-RULE-016`; `GUARD-INP-01`; api contract `POST` dan `GET /episodes/{id}/doctor-assignments`; `FR-RI-116` s.d. `FR-RI-118`; `UAT-07` |
| **Reuse** | Bentuk berperiode yang sama dengan `InpBedPlacement`. Unique index parsial DPJP aktif per episode dibuat pada `BE-RWI-003` |
| **Scope** | `InpEpisodeService` bagian penugasan DPJP; dua aksi controller; `GUARD-INP-01` sebagai method yang dapat dipanggil ulang oleh task perpindahan |
| **Dependency** | `BE-RWI-016` |
| **Acceptance criteria** | 1. Riwayat berperiode: dr. Andi 21–23 September, dr. Rina 23–25 September, dan pada 25 September sistem masih dapat menjawab siapa yang berwenang pada 22 September. 2. Satu episode aktif punya **tepat satu** DPJP aktif; percobaan membuat penugasan kedua tanpa menutup yang pertama ditolak. 3. Pengalihan tanpa alasan ditolak 400. 4. Pengalihan hanya oleh kepala ruangan atau supervisor; peran lain ditolak 403 |
| **Verification** | Integration test keempat kriteria; test unique index parsial DPJP aktif |
| **Risk/blocker** | Godaan menyimpan `CurrentDoctorId` sebagai kolom pada episode akan muncul karena query-nya lebih murah. `blueprint-manifest.md` bagian 8 butir 4 **mengunci** bentuk berperiode; menggantinya menghapus riwayat yang dibutuhkan resume dan billing. Owner: Backend/API |
| **DoD** | Dua endpoint sesuai kontrak; keempat kriteria lulus; `GUARD-INP-01` dapat dipanggil ulang; api contract diperbarui |

---

### `BE-RWI-018` — Perawat penanggung jawab tercatat, dan ketiadaannya tidak menahan apa pun

| Field | Isi |
| --- | --- |
| **Outcome** | Kepala ruangan dapat menugaskan perawat penanggung jawab, riwayatnya tersimpan berperiode, dan episode yang belum punya perawat tetap berjalan — hanya muncul pada daftar pantau |
| **Trace** | `RWI-DEC-032`; `RWI-RULE-023`; api contract `POST` dan `GET /episodes/{id}/nurse-assignments`; `FR-RI-119` |
| **Reuse** | Bentuk berperiode yang sama dengan penugasan DPJP dari `BE-RWI-017` |
| **Scope** | `InpEpisodeService` bagian penugasan perawat; dua aksi controller |
| **Dependency** | `BE-RWI-016` |
| **Acceptance criteria** | 1. Penugasan menutup penugasan sebelumnya dan membuka yang baru. 2. Episode **boleh** berjalan tanpa perawat penanggung jawab; selama itu tidak ada satu pun tindakan yang tertahan. 3. Episode tanpa perawat muncul pada daftar pantau kepala ruangan. 4. Riwayat perawat terbaca urut. 5. Penugasan hanya oleh kepala ruangan atau supervisor |
| **Verification** | Integration test kelima kriteria; kriteria 2 wajib membuktikan bahwa penempatan, perpindahan, dan keputusan pulang semuanya tetap berhasil tanpa perawat |
| **Risk/blocker** | Kriteria 2 mudah dikerjakan terbalik menjadi "wajib ada perawat sebelum episode aktif". `RWI-DEC-032` memilih **tidak menahan**, karena penugasan perawat sering menyusul beberapa menit setelah pasien tiba. Owner: Backend/API |
| **DoD** | Dua endpoint sesuai kontrak; kelima kriteria lulus; api contract diperbarui |

---

### `BE-RWI-019` — Pasien dapat berpindah tanpa episode terputus

| Field | Isi |
| --- | --- |
| **Outcome** | Perpindahan menutup penempatan lama dan membuka yang baru dalam satu tindakan utuh. Tidak pernah ada satu saat pun pasien tercatat tanpa tempat tidur, dan kelas tagihan mengikuti kamar yang ditempati |
| **Trace** | `RWI-DEC-012`, `RWI-DEC-013`, `RWI-DEC-014`, `RWI-DEC-023`; `RWI-RULE-006` s.d. `RWI-RULE-008`; `INV-INP-07`; `GUARD-INP-01`; api contract `POST /placements/transfer`; `FR-RI-120` s.d. `FR-RI-123`, `FR-RI-162`; `RWI-AC-133`; `UAT-08`, `UAT-09` |
| **Reuse** | `EvaluatePlacementEligibility` dipanggil **utuh** — kedelapan aturannya, termasuk jenis kelamin dan isolasi. Tidak ada daftar aturan kedua yang ditulis khusus untuk perpindahan |
| **Scope** | `InpBedOccupancyService.TransferAsync`; aksi `POST /placements/transfer` |
| **Dependency** | `BE-RWI-017`; `BE-RWI-013` dan `BE-RWI-015` agar kedelapan aturan sudah lengkap |
| **Acceptance criteria** | 1. Perpindahan menghasilkan dua baris penempatan; yang lama punya waktu berakhir dan alasan `Transfer`. 2. Bila pembukaan penempatan baru gagal, penempatan lama **tidak jadi** ditutup; pasien tetap di tempat semula. 3. Kelas yang ditagihkan mengikuti kamar tujuan; riwayat menunjukkan 2 hari kelas 2 dan 2 hari kelas 1. 4. Dokter yang bukan DPJP aktif ditolak 403, **tanpa** kolom keterangan yang dapat dipakai melewatinya. 5. Perpindahan tanpa alasan medis ditolak 400. 6. Perpindahan ke kamar yang sudah dihuni jenis kelamin berbeda ditolak 422 dengan kode dan pesan **sama persis** seperti penempatan |
| **Verification** | Integration test keenam kriteria; kriteria 2 wajib memaksa kegagalan di tengah transaksi; kriteria 6 wajib menjalankan ulang skenario `UAT-29` lewat jalur perpindahan |
| **Risk/blocker** | Menulis daftar aturan kedua khusus perpindahan adalah kesalahan yang paling mahal di modul ini: dua daftar akan berselisih dalam hitungan minggu, dan jalur perpindahan justru yang paling sering dipakai petugas yang terburu-buru. Owner: Backend/API |
| **DoD** | Endpoint sesuai kontrak; keenam kriteria lulus; terbukti hanya ada **satu** daftar aturan di seluruh source; api contract diperbarui |

---

### `BE-RWI-020` — DPJP dapat menyatakan pasien boleh pulang

| Field | Isi |
| --- | --- |
| **Outcome** | Episode berpindah ke `DischargePending` atas keputusan DPJP, dengan cara pulang yang dipilih sadar. Tempat tidur **belum** dilepas pada langkah ini |
| **Trace** | `RWI-DEC-016`, `RWI-DEC-017`; `RWI-RULE-010`, `RWI-RULE-011`; `GUARD-INP-02`; api contract `POST /discharges/{episodeId}/decide`; state matrix |
| **Reuse** | `ApplyStatusChangeAsync` dari `BE-RWI-007`, sehingga riwayat status tertulis otomatis |
| **Scope** | `InpDischargeService.DecideDischargeAsync`; aksi controller; `GUARD-INP-02` |
| **Dependency** | `BE-RWI-018` |
| **Acceptance criteria** | 1. Hanya DPJP aktif yang dapat memutuskan; peran lain dan dokter lain ditolak 403. 2. Lima cara pulang dikenali sesuai `RWI-RULE-011`. 3. Episode menjadi `DischargePending` dan tempat tidur **tetap** terisi. 4. Pasien masih muncul pada census. 5. Keputusan menulis satu baris riwayat status |
| **Verification** | Integration test kelima kriteria; test yang membuktikan `MstBed.BedStatus` tidak berubah pada langkah ini |
| **Risk/blocker** | Dua cara pulang — meninggal dan kabur — sisi klinisnya **masih terbuka** pada `RWI-OQ-039` dan `RWI-DEC-059`, menunggu pemilik klinis. Keduanya tetap dikenali sistem, tetapi laporan wajib menyebut bahwa aturan klinisnya belum disahkan. Owner: Product/Domain bersama Clinical governance |
| **DoD** | Endpoint sesuai kontrak; kelima kriteria lulus; laporan menyebut dua cara pulang yang aturan klinisnya belum final; api contract diperbarui |

---

### `BE-RWI-021` — Resume pulang tersusun dan hanya DPJP yang menandatanganinya

| Field | Isi |
| --- | --- |
| **Outcome** | Episode punya catatan resmi yang tertandatangani, dan tanda tangan itu benar-benar berarti karena hanya DPJP aktif yang dapat membubuhkannya |
| **Trace** | `RWI-DEC-016`; `GUARD-INP-03`; api contract `GET`, `PUT`, dan `PATCH .../summary`; privasi pada `03-frontend-architecture.md` bagian 6; `UAT-10` |
| **Reuse** | Pola dokumen bertanda tangan pada modul klinis yang sudah ada, untuk bentuk kolom penanda tangan dan waktunya |
| **Scope** | `InpDischargeService` bagian resume; tiga aksi controller; DTO resume |
| **Dependency** | `BE-RWI-020` |
| **Acceptance criteria** | 1. Resume dapat disusun dan diperbarui selagi belum ditandatangani. 2. Hanya DPJP aktif yang dapat menandatangani; peran lain ditolak 403 — membuktikan `GUARD-INP-03`. 3. Resume yang sudah ditandatangani **tidak** dapat diubah lewat endpoint biasa. 4. Satu episode punya paling banyak satu resume yang berlaku. 5. Isi resume tidak ikut pada endpoint daftar mana pun |
| **Verification** | Integration test kelima kriteria; test privasi yang memeriksa bahwa payload daftar episode dan census tidak memuat isi resume |
| **Risk/blocker** | Kriteria 5 adalah kewajiban privasi, bukan preferensi. Isi resume memuat diagnosis. Bila ia bocor ke endpoint daftar, seluruh peran yang boleh melihat census ikut melihatnya. Owner: Backend/API bersama Security/Privacy — **pemilik privasi belum ditunjuk**, jadi keputusannya mengikuti aturan yang sudah tertulis, bukan menunggu |
| **DoD** | Tiga endpoint sesuai kontrak; kelima kriteria lulus; test privasi lulus; api contract diperbarui |

---

### `BE-RWI-022` — Koreksi resume menyimpan versi sebelumnya

| Field | Isi |
| --- | --- |
| **Outcome** | Mengubah resume yang sudah ditandatangani adalah amandemen rekam medis, bukan penyuntingan biasa. Versi lama beserta nama penandatangannya tetap dapat dibaca selamanya |
| **Trace** | `RWI-DEC-057`; `FR-RI-153`; `RWI-AC-124` s.d. `RWI-AC-126`; `UAT-27` |
| **Reuse** | Tabel `InpDischargeSummaryRevision` dibuat pada `BE-RWI-003` |
| **Scope** | Penyalinan versi di dalam `InpDischargeService`; parameter `includeRevisions` pada `GET .../summary` |
| **Dependency** | `BE-RWI-021` |
| **Acceptance criteria** | 1. Menyunting resume yang **belum** ditandatangani **tidak** membuat versi baru. 2. Mengubah resume yang sudah ditandatangani lewat sesi koreksi menyimpan salinan versi sebelumnya. 3. Versi yang tersimpan **tidak dapat** diubah maupun dihapus. 4. `GET .../summary?includeRevisions=true` mengembalikan versi berlaku beserta daftar versi lama urut waktu |
| **Verification** | Integration test keempat kriteria; kriteria 3 wajib mencoba `PUT` dan `DELETE` langsung ke baris versi dan membuktikan keduanya ditolak |
| **Risk/blocker** | Kriteria 1 dan 2 mudah dikerjakan terbalik menjadi "setiap penyuntingan membuat versi". Itu akan membanjiri tabel versi dengan draf setengah jadi dan membuat riwayat amandemen kehilangan artinya. Owner: Backend/API |
| **DoD** | Penyalinan versi aktif; keempat kriteria lulus; api contract diperbarui |

---

### `BE-RWI-023` — Daftar periksa administrasi dapat ditandai dan bersifat menahan

| Field | Isi |
| --- | --- |
| **Outcome** | Butir administrasi yang wajib benar-benar menahan penutupan episode, dan daftarnya dapat diubah admin tanpa menyentuh kode |
| **Trace** | `RWI-DEC-026`, `RWI-DEC-033`; `RWI-RULE-018`, `RWI-RULE-024`; api contract `GET .../clearance` dan `POST .../clearance/{itemId}/mark` |
| **Reuse** | `MstInpatientClearanceItem` dan seedernya dari `BE-RWI-001` dan `BE-RWI-002` |
| **Scope** | `InpDischargeService` bagian daftar periksa; dua aksi controller |
| **Dependency** | `BE-RWI-005` |
| **Acceptance criteria** | 1. Daftar butir menampilkan seluruh butir aktif beserta status penandaannya. 2. Menandai butir menyimpan pelaku dan waktunya. 3. Butir wajib yang belum ditandai menahan penutupan. 4. Butir tidak wajib yang belum ditandai **tidak** menahan. 5. Butir yang dinonaktifkan admin setelah episode berjalan tidak lagi menahan, dan penandaan lamanya tidak hilang |
| **Verification** | Integration test kelima kriteria; kriteria 5 wajib menonaktifkan butir di tengah episode berjalan dan memeriksa keduanya |
| **Risk/blocker** | Butir `DISCHARGE-MED` obat pulang ditandai **manual** pada MVP, karena modul Farmasi di luar scope — `DEC-INP-001`. Jangan membuat penandaan otomatis yang menebak. Owner: Backend/API |
| **DoD** | Dua endpoint sesuai kontrak; kelima kriteria lulus; api contract diperbarui |

---

### `BE-RWI-024` — Kasir dapat menandai kelayakan keuangan

| Field | Isi |
| --- | --- |
| **Outcome** | Gerbang keuangan punya sumber data yang jelas walaupun modul Billing belum punya kemampuan transaksi, dan setiap penandaan meninggalkan pelaku, waktu, dan catatan |
| **Trace** | `RWI-DEC-015`, `RWI-DEC-040`; `RWI-RULE-009`, `RWI-RULE-028`; api contract `POST .../financial-clearance`; `RWI-RISK-003` |
| **Reuse** | Tidak ada — ini konsep baru yang sengaja dimiliki Rawat Inap **sementara**, sampai `BillingManagement` punya kemampuan transaksi |
| **Scope** | `InpDischargeService` bagian kelayakan keuangan; satu aksi controller |
| **Dependency** | `BE-RWI-005` |
| **Acceptance criteria** | 1. Tiga nilai dikenali: `Pending`, `Cleared`, `Blocked`. 2. Penandaan wajib disertai catatan; tanpa catatan ditolak 400. 3. Pelaku dan waktu tersimpan. 4. Hanya peran kasir atau billing yang dapat menandai. 5. Hanya `Cleared` yang membuka penutupan |
| **Verification** | Integration test kelima kriteria |
| **Risk/blocker** | **`RWI-RISK-003` diterima secara sadar:** penandaan manual berarti kelayakan keuangan bergantung pada disiplin petugas, bukan pada angka tagihan yang sebenarnya. Ini sementara. Ketika `BillingManagement` operasional, topik ini kembali sebagai Amendment Pass. Laporan wajib menyebut risiko ini. Owner: Product/Domain |
| **DoD** | Endpoint sesuai kontrak; kelima kriteria lulus; laporan menyebut `RWI-RISK-003` secara eksplisit; api contract diperbarui |

---

### `BE-RWI-025` — Kelima syarat penutupan diperiksa dan dilaporkan satu per satu

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas yang gagal menutup episode tahu **persis** syarat mana yang belum terpenuhi, bukan menerima satu kalimat umum. Setelah kelimanya terpenuhi, episode ditutup dan tempat tidur kembali kosong |
| **Trace** | `RWI-DEC-016`; `RWI-RULE-010`; api contract `GET .../closure-readiness` dan `POST .../close`; `UAT-11`; `RWI-AC-064` |
| **Reuse** | `ApplyStatusChangeAsync`; pola pelepasan tempat tidur dari `InpBedOccupancyService` |
| **Scope** | `InpDischargeService.EvaluateClosureReadinessAsync` dan `CloseEpisodeAsync`; dua aksi controller |
| **Dependency** | `BE-RWI-022`, `BE-RWI-023`, `BE-RWI-024` |
| **Acceptance criteria** | 1. `closure-readiness` mengembalikan **kelima** syarat beserta tanda sudah atau belum, bukan boolean tunggal. 2. Penutupan dengan salah satu syarat belum terpenuhi ditolak 422 disertai daftar syarat yang kurang. 3. Penutupan yang lolos mengubah episode menjadi `Closed` dan melepas tempat tidur dalam satu transaksi. 4. Setelah penutupan, tempat tidur terbaca `Available` pada pencarian berikutnya. 5. Penutupan menulis satu baris riwayat status |
| **Verification** | Integration test kelima kriteria; test yang menutup episode lalu mencari tempat tidur kosong dan menemukannya |
| **Risk/blocker** | Kriteria 1 sering dikerjakan sebagai boolean karena lebih sederhana. Layar kemudian tidak dapat memberi tahu petugas apa yang harus dikejar, dan petugas menebak. Bentuk daftar adalah kontrak, bukan preferensi. Owner: Backend/API |
| **DoD** | Dua endpoint sesuai kontrak; kelima kriteria lulus; api contract diperbarui |

---

### `BE-RWI-026` — Jalan keluar supervisor sempit dan selalu tercatat

| Field | Isi |
| --- | --- |
| **Outcome** | Pasien yang harus segera pulang tidak tertahan urusan kasir, tetapi setiap penutupan yang menembus gerbang keuangan tertinggal jejaknya dan muncul pada laporan pengecualian |
| **Trace** | `RWI-DEC-015`; `RWI-RULE-009`; api contract `POST .../close-with-override`; `UAT-12`, `UAT-13` |
| **Reuse** | Jalur penutupan dari `BE-RWI-025`; hanya gerbang keuangannya yang dilewati |
| **Scope** | `InpDischargeService.CloseWithOverrideAsync`; satu aksi controller; hak akses `InpatientEpisode : CloseOverride` |
| **Dependency** | `BE-RWI-025` |
| **Acceptance criteria** | 1. Hanya supervisor yang dapat memanggilnya. 2. Alasan wajib; tanpa alasan ditolak 400. 3. Jalan keluar ini menembus **hanya** syarat keuangan; empat syarat lain tetap menahan. 4. Episode ditandai `IsClosedWithoutFinancialClearance`. 5. Episode itu muncul pada daftar pantau penutupan menembus gerbang keuangan |
| **Verification** | Integration test kelima kriteria; kriteria 3 wajib mencoba menembus dengan resume yang **belum** ditandatangani dan membuktikan tetap ditolak |
| **Risk/blocker** | Kriteria 3 adalah inti task ini. Jalan keluar yang menembus semua syarat sekaligus akan menjadi jalur normal dalam hitungan minggu, dan kelima syarat kehilangan arti. Owner: Backend/API bersama Product/Domain |
| **DoD** | Endpoint sesuai kontrak; kelima kriteria lulus; api contract diperbarui |

---

### `BE-RWI-027` — Tempat tidur bebas sejak pasien meninggalkan kamar

| Field | Isi |
| --- | --- |
| **Outcome** | Tempat tidur tidak lagi tertahan berjam-jam menunggu urusan administrasi selesai. Begitu pasien benar-benar pergi, tempat tidur boleh dipesan pasien berikutnya — walaupun episodenya belum ditutup |
| **Trace** | `RWI-DEC-055`; `RWI-RULE-036`; `INV-INP-01` yang dilonggarkan; api contract `POST .../record-departure`; `FR-RI-149` s.d. `FR-RI-151`; `RWI-AC-118` s.d. `RWI-AC-121`; `UAT-24`, `UAT-25` |
| **Reuse** | Pelepasan tempat tidur memakai jalur yang sama dengan penutupan episode, dengan alasan berakhir `PatientDeparted` |
| **Scope** | `InpDischargeService.RecordPatientDepartureAsync`; satu aksi controller; hak akses `InpatientDischarge : RecordDeparture` |
| **Dependency** | `BE-RWI-025` |
| **Acceptance criteria** | 1. Mencatat kepergian melepas tempat tidur seketika; tempat tidur muncul pada pencarian berikutnya. 2. Episode tetap `DischargePending` dan tetap muncul pada daftar pantau penutupan tertunda. 3. Pasien yang sudah pergi **tidak** muncul di census dan **tidak** dapat dipindahkan. 4. Menutup episode tanpa mencatat kepergian tetap berhasil; tempat tidur dilepas saat penutupan. 5. Kepergian **tidak** menulis baris riwayat status — kepergian fisik bukan perubahan status. 6. Mencatat kepergian pada episode `Admitted` ditolak 422. 7. Mencatat dua kali ditolak 409. 8. Waktu kepergian mendahului keputusan pulang ditolak 400. 9. Bila pelepasan tempat tidur gagal, kolom kepergian pada episode juga tidak terisi |
| **Verification** | Integration test kesembilan kriteria; kriteria 5 wajib menghitung baris riwayat sebelum dan sesudah |
| **Risk/blocker** | Kriteria 5 melawan intuisi. `RWI-DEC-009` mengunci **lima** nilai status, dan kepergian fisik sengaja tidak dijadikan status keenam — ia adalah fakta yang dicatat, bukan tahapan yang dilalui. Menambah status keenam melanggar butir yang terkunci pada `blueprint-manifest.md` bagian 8. Owner: Backend/API |
| **DoD** | Endpoint sesuai kontrak; kesembilan kriteria lulus; api contract diperbarui |

---

### `BE-RWI-028` — Riwayat status terbaca lengkap dan tidak dapat dihapus

| Field | Isi |
| --- | --- |
| **Outcome** | Setiap perpindahan status meninggalkan jejak yang tidak dapat diubah siapa pun, lengkap dengan pelaku dan waktunya. Auditor dapat menelusuri satu episode dari lahir sampai tutup |
| **Trace** | `RWI-DEC-009`; `NFR-003`; api contract `GET /episodes/{id}/status-history`; `UAT-17` |
| **Reuse** | Baris riwayat sudah ditulis sejak `BE-RWI-007` lewat `ApplyStatusChangeAsync`. Task ini menambahkan pembacaannya dan membuktikan sifat tidak dapat diubahnya |
| **Scope** | Aksi `GET /{id}/status-history`; penjagaan agar tabel riwayat hanya menerima penambahan |
| **Dependency** | `BE-RWI-027` |
| **Acceptance criteria** | 1. Riwayat terbaca urut waktu beserta pelaku, status asal, status tujuan, dan alasan bila ada. 2. Tidak ada satu pun endpoint yang dapat mengubah atau menghapus baris riwayat. 3. Perubahan yang **dihitung sistem** — misalnya `Draft` gugur sendiri — tercatat sebagai tindakan sistem, bukan menuduh orang terakhir yang membuka layar. 4. Riwayat tetap terbaca setelah episode `Closed` |
| **Verification** | Integration test keempat kriteria; kriteria 2 wajib mencoba `PUT` dan `DELETE` langsung dan membuktikan keduanya tidak tersedia; kriteria 3 memeriksa isi kolom pelaku pada episode yang gugur sendiri |
| **Risk/blocker** | Kriteria 3 adalah masalah keadilan, bukan teknis. Mencatat kedaluwarsa otomatis atas nama pengguna yang kebetulan membaca akan membuat laporan pengecualian menuduh orang yang tidak melakukan apa-apa. Owner: Backend/API |
| **DoD** | Endpoint sesuai kontrak; keempat kriteria lulus; api contract diperbarui |

---

### `BE-RWI-029` — Empat daftar pantau dan satu laporan selisih tersedia

| Field | Isi |
| --- | --- |
| **Outcome** | Kepala ruangan dan supervisor punya daftar yang menunjukkan apa yang perlu ditindaklanjuti, dan admin punya laporan yang menemukan tempat tidur yang statusnya tidak cocok dengan penghuninya |
| **Trace** | `RWI-DEC-032`, `RWI-DEC-039`; `RWI-RULE-023`, `RWI-RULE-027`; api contract bagian Monitoring (5 endpoint); `RWI-FE-002`; `RWI-AC-063`; `UAT-21` |
| **Reuse** | `InpCensusQueryService`; daftar pantau isolasi sudah dibuat pada `BE-RWI-015` |
| **Scope** | Empat aksi `InpatientMonitoringController` yang belum ada: penutupan tertunda, penutupan menembus gerbang, episode tanpa perawat, dan laporan selisih tempat tidur |
| **Dependency** | `BE-RWI-027` |
| **Acceptance criteria** | 1. Daftar penutupan tertunda menampilkan episode `DischargePending` yang melewati `PendingClosureThresholdHours`. 2. Ambangnya dapat diubah admin dan berlaku pada pembacaan berikutnya. 3. Daftar penutupan menembus gerbang menampilkan episode bertanda `IsClosedWithoutFinancialClearance`. 4. Daftar episode tanpa perawat menampilkan episode aktif tanpa penugasan perawat aktif. 5. Laporan selisih menampilkan tempat tidur yang salinan statusnya tidak cocok dengan catatan penempatan. 6. Daftar yang kosong mengembalikan daftar kosong, bukan galat |
| **Verification** | Integration test keenam kriteria; kriteria 5 wajib membuat selisih **secara sengaja** lewat perubahan langsung di database uji lalu membuktikan laporan menemukannya |
| **Risk/blocker** | Kriteria 5 adalah satu-satunya pengawas atas satu-satunya arah tulis lintas modul. Bila laporan ini tidak pernah dibaca siapa pun, `MstBed.BedStatus` akan menyimpang diam-diam. Ini soal proses, bukan kode — laporan wajib menyebutnya. Owner: Backend/API bersama Product/Domain |
| **DoD** | Empat endpoint sesuai kontrak; keenam kriteria lulus; api contract diperbarui |

---

### `BE-RWI-030` — Kesalahan catatan dapat dibetulkan tanpa membongkar episode

| Field | Isi |
| --- | --- |
| **Outcome** | Supervisor dapat membetulkan kesalahan pada episode yang sudah ditutup, tanpa tempat tidur ikut terganggu, tanpa hari rawat bertambah, dan dengan setiap perubahan meninggalkan jejak |
| **Trace** | `RWI-DEC-028`, `RWI-DEC-057`; `RWI-RULE-020`; api contract `POST` dan `PATCH .../correction-sessions`; `UAT-14`, `UAT-15` |
| **Reuse** | `InpCorrectionSession` dari `BE-RWI-003`; penyalinan versi resume dari `BE-RWI-022` |
| **Scope** | `InpEpisodeService` bagian sesi koreksi; dua aksi controller; hak akses `InpatientEpisode : Reopen` |
| **Dependency** | `BE-RWI-028` |
| **Acceptance criteria** | 1. Hanya supervisor yang dapat membuka sesi koreksi. 2. Status episode tetap `Closed` sepanjang sesi berjalan. 3. Tempat tidur **tidak** dikembalikan dan hari rawat **tidak** bertambah. 4. Satu episode punya paling banyak satu sesi terbuka. 5. Menutup sesi menyimpan daftar perubahannya. 6. Koreksi resume yang sudah ditandatangani menyimpan versi lamanya |
| **Verification** | Integration test keenam kriteria; kriteria 3 wajib memeriksa lama dirawat sebelum dan sesudah sesi |
| **Risk/blocker** | Godaan menjadikan "sedang dikoreksi" sebagai status episode keenam akan muncul. `blueprint-manifest.md` bagian 8 butir 5 **menguncinya** sebagai konsep tersendiri, karena menambah status melanggar `RWI-DEC-009` dan `RWI-AC-004`. Owner: Backend/API |
| **DoD** | Dua endpoint sesuai kontrak; keenam kriteria lulus; api contract diperbarui |

---

### `BE-RWI-031` — Bayi baru lahir punya episode sendiri di boks kamar ibunya

| Field | Isi |
| --- | --- |
| **Outcome** | Bayi mendapat episode dan kunjungan sendiri, boks bayi diperlakukan sebagai tempat tidur, dan sistem dapat menjawab bayi siapa yang berada di boks kamar mana |
| **Trace** | `RWI-DEC-020`, `RWI-DEC-056`; `RWI-RULE-014`; `FR-RI-146`, `FR-RI-147`, `FR-RI-152`; `RWI-AC-122`, `RWI-AC-123`; `UAT-22`, `UAT-28` |
| **Reuse** | `MstBed.IsForNewborn` **sudah ada** — terbukti pada `MstBed.cs` baris 33. Kolom `MotherEpisodeId` dibuat pada `BE-RWI-003`. Pengecualian boks bayi sudah aktif sejak `BE-RWI-013` |
| **Scope** | Pengisian dan pembacaan `MotherEpisodeId`; penyesuaian census agar menampilkan ibu dan bayi sebagai dua baris |
| **Dependency** | `BE-RWI-029` |
| **Acceptance criteria** | 1. Bayi mendapat episode dan kunjungan sendiri, ditempatkan di boks bertanda `IsForNewborn`. 2. Census menampilkan **dua** baris: ibu dan bayinya. 3. Menutup episode ibu **tidak** menutup episode bayi dan **tidak** melepas boks bayi. 4. Episode bayi dapat menyimpan rujukan ke episode ibunya, dan sistem dapat menjawab bayi siapa yang ada di boks kamar tertentu. 5. `MotherEpisodeId` boleh kosong, dan **tidak boleh** menunjuk episode milik pasien yang sama |
| **Verification** | Integration test kelima kriteria; kriteria 5 wajib mencoba menunjuk episode pasien yang sama dan membuktikan ditolak |
| **Risk/blocker** | Kriteria 3 adalah yang paling mudah dikerjakan terbalik menjadi "menutup ibu menutup bayinya". Bayi sering pulang pada hari yang berbeda dari ibunya, dan episode yang tertutup paksa akan menghapus hari rawat bayi dari tagihan. Owner: Backend/API |
| **DoD** | Kelima kriteria lulus; census terbukti menampilkan dua baris; api contract tidak berubah |

---

### `BE-RWI-032` — Empat modul tetangga terbukti tidak rusak

| Field | Isi |
| --- | --- |
| **Outcome** | Perubahan perilaku pada `BedController` terbukti tidak merusak jalur yang sudah dipakai poliklinik, IGD, dan farmasi — dibuktikan test, bukan diyakini |
| **Trace** | `RWI-DEC-051`, `RWI-DEC-062`; `RWI-RISK-002`; `NFR-008`; `RWI-AC-114`; `testing/acceptance-test-matrix.md` bagian 12 |
| **Reuse** | Kerangka `QuilvianSystemBackend.Tests` yang sudah ada, mengikuti pola `BillingModuleFoundationTests.cs` — satu-satunya berkas test backend hari ini |
| **Scope** | Berkas test regresi baru untuk jalur `MstBed` yang dipakai modul lain; **tidak** menyentuh source modul tetangga |
| **Dependency** | `BE-RWI-006` — dikerjakan bersamanya, bukan sesudahnya |
| **Acceptance criteria** | 1. Layar master tempat tidur tetap berfungsi untuk `Cleaning`, `Maintenance`, `Blocked`, dan `Inactive`. 2. Jalur pemakaian `MstBed` oleh modul lain yang tidak menyetel status tetap berjalan. 3. Test gagal bila perubahan `BedController` melebihi kesepakatan — yaitu bila ia mulai menolak nilai yang seharusnya masih diizinkan. 4. Test dijalankan pada rangkaian yang sama dengan test modul Rawat Inap |
| **Verification** | Jalankan seluruh rangkaian test sebelum dan sesudah `BE-RWI-006`, lampirkan keluarannya apa adanya |
| **Risk/blocker** | **`RWI-RISK-002` diterima secara sadar:** tidak ada satu pun test yang menjaga jalur poliklinik, IGD, dan farmasi hari ini. Task ini menutup lubang itu **hanya** untuk jalur `MstBed` yang benar-benar disentuh — bukan untuk seluruh modul tetangga. Jangan melebarkan scope-nya diam-diam. Owner: Backend/API |
| **DoD** | Test regresi ada dan lulus; keluaran sebelum dan sesudah dilampirkan; laporan menyatakan cakupannya terbatas pada jalur `MstBed` |

---

### `BE-RWI-033` — Bukti penerimaan lengkap dan traceability tertutup

| Field | Isi |
| --- | --- |
| **Outcome** | Setiap acceptance criteria pada decision log punya bukti test yang menunjuk padanya, dan setiap endpoint pada api contract sudah berubah dari "Rencana" menjadi tersedia. Modul siap dinilai `/qv-verify` |
| **Trace** | `RWI-AC-001` s.d. `RWI-AC-139`; `testing/acceptance-test-matrix.md` `0.3.0` seluruh bagian; `requirement-traceability.md` pada folder ini |
| **Reuse** | Matriks acceptance test yang sudah ada; task ini **memeriksa** kelengkapannya, bukan menulis ulang |
| **Scope** | Pemutakhiran `contracts/api-contract.md` kolom status; pemutakhiran `roadmap/requirement-traceability.md`; laporan penutup |
| **Dependency** | Seluruh task `BE-RWI-001` s.d. `BE-RWI-032` |
| **Acceptance criteria** | 1. Ke-49 endpoint **baru** pada api contract berstatus tersedia, atau punya alasan tertulis kenapa belum; baris ke-50 `PATCH /beds/{id}/availability` dinilai terpisah sebagai perubahan perilaku. 2. Seluruh 139 acceptance criteria punya penunjuk ke test yang membuktikannya, atau tertulis alasan kenapa belum dapat diuji. 3. Ke-33 skenario UAT punya pasangannya. 4. Tidak ada satu pun butir traceability yang berbunyi "menyusul" |
| **Verification** | Pemeriksaan silang antara decision log, api contract, test matrix, dan berkas test yang benar-benar ada di repository |
| **Risk/blocker** | Task ini sering diperlakukan sebagai formalitas dan dikerjakan asal lengkap. Ia justru satu-satunya tempat lubang cakupan ketahuan sebelum modul dipakai pasien sungguhan. Owner: Backend/API bersama Product/Domain |
| **DoD** | Keempat kriteria lulus; api contract dan traceability mutakhir; modul siap masuk `/qv-verify` |

---

## 5. Gerbang yang masih terbuka

| Gerbang | Keadaannya | Menahan |
| --- | --- | --- |
| ~~**Approval blueprint**~~ | **DICABUT 2026-08-24** oleh `RWI-DEC-067`. Disetujui Muhammad Hamzah | — |
| Kesiapan data master | Penanggung jawab ditetapkan `RWI-DEC-063`, target 22 Agustus 2026. Sejak revision `3` penandanya harus **benar**, bukan sekadar terisi | `BE-RWI-010` ke atas tidak dapat diuji |
| `FE-RWI-001` perbaikan tombol tempat tidur | Lintas repository | `BE-RWI-006` |
| ~~Registry lifecycle~~ | **DICABUT 2026-08-24** oleh `RWI-DEC-068`. Modul naik `PLANNED` → `ACTIVE`; `QBE-MOD-002` tidak lagi menahan pembuatan entity `Inp*` | — |
| `RWI-RULE-021` batas waktu klinis | Menunggu pemilik klinis | Tidak menahan MVP; menahan pemakaian untuk pasien sungguhan |
| `RWI-RULE-025` persetujuan umum | `DEC-INP-003`, menunggu pemilik hukum | Sama |
| Masa simpan riwayat | `RWI-OQ-035`, sudah dijawab `RWI-DEC-060`, menunggu pemilik hukum | Sama |

---

## 6. Yang sengaja tidak ada di roadmap ini

| Yang tidak dikerjakan | Alasan | Decision ID |
| --- | --- | --- |
| Pengkajian, catatan dokter, CPPT, tindakan, visite | Slice di luar scope MVP | `DEC-INP-001` |
| Resep rawat inap dan obat pulang | Terikat konsultasi; di luar scope | `DEC-INP-001` |
| Serah terima IGD ke rawat inap | Di luar scope | `DEC-INP-002` |
| Persetujuan umum rawat inap | Di luar scope | `DEC-INP-003` |
| Pengiriman SATUSEHAT | Di luar scope | `DEC-INP-005` |
| Serah terima klinis antar shift | Di luar scope | `DEC-INP-006` |
| Tabel riwayat kebutuhan isolasi | `RWI-DEC-065` menyebutnya **atribut**, bukan riwayat. Yang tersimpan hanya nilai berlaku | `RWI-DEC-065` |
| Kolom "boleh campur" pada `MstRoom` | Ditolak tegas; aturan diperiksa dari penghuni yang sedang ada | `RWI-DEC-066` |
| Status episode keenam untuk kepergian atau koreksi | Melanggar lima nilai yang dikunci | `RWI-DEC-009` |

Ketiadaan kesembilan butir itu adalah **keadaan yang disengaja**, bukan cakupan yang terlupa.
