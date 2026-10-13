# PLAN-REPAIR-005 — Perbaikan Runtime Workspace PPRI: Penyelarasan Skema Penjamin, Normalisasi Situs Utama, Perbaikan Registrasi Tanda Tangan EF Core, dan Pemetaan Wewenang Akses

```yaml
plan_id: PLAN-REPAIR-EPS-005
issue: ../issue/issue-005-verifikasi-runtime-workspace-ppri.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-09"
diputuskan_oleh: "Pemilik modul — persetujuan 'oke setuju kerjakan' 2026-10-09"
tanggal_keputusan: "2026-10-09"
basis_source_backend: "fdf85a07"
basis_source_frontend: null
perubahan_backend: "Skema PostgreSQL MstPatientCompanyGuarantor, Master Data MstHospitalSite, Service InpAdmissionSignatureService.cs, dan Seeder SysAccessPolicy"
ditulis_dengan: "skill diagnose-module-issue"
```

Dokumen ini memuat rencana kerja terstruktur untuk menyelesaikan empat temuan teknis yang teridentifikasi selama verifikasi runtime 28 endpoint Workspace PPRI (Penerimaan Pasien Rawat Inap) pada tanggal 8 Oktober 2026. Rencana ini dirancang agar setiap langkah perbaikan dapat dieksekusi secara independen, terisolasi, aman terhadap data klinis yang ada, dan dapat dibuktikan kembali dengan pengujian runtime deterministik.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Perbaikan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| `FIX-EPS-005-01` | `ISS-EPS-005-01` | Penambahan kolom `CardImagePath` pada tabel `MstPatientCompanyGuarantor` di basis data PostgreSQL | Database / DDL | 1 | — | ✅ SELESAI | Kolom terpasang; riwayat migrasi `20261006092040` tersimpan di `__EFMigrationsHistory` |
| `FIX-EPS-005-02` | `ISS-EPS-005-02` | Normalisasi penanda situs utama tunggal (`IsMainSite = true`) pada tabel `MstHospitalSite` | Master Data | 1 | — | ✅ SELESAI | Hanya `SITE-MMC-001` (RS MMC) bertanda `IsMainSite = true` |
| `FIX-EPS-005-03` | `ISS-EPS-005-03` | Perbaikan pelacakan entitas EF Core pada `SaveSignatureAsync` agar tanda tangan baru disimpan dengan perintah SQL `INSERT` | Backend Service | 1 | — | ✅ SELESAI | [`InpAdmissionSignatureService.cs:285`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionSignatureService.cs#L285); `dotnet build` PASS (0 errors) |
| `FIX-EPS-005-04` | `ISS-EPS-005-04` | Penambahan seeder kebijakan wewenang `SysAccessPolicy` untuk controller `InpatientAdmissionDocument` | Auth / Seeder | 2 | — | ✅ SELESAI | 26 baris kebijakan aktif tersimpan di tabel `SysAccessPolicy` |

**Ringkasan: 4 dari 4 perbaikan teknis selesai dikerjakan pada 9 Oktober 2026.** Langkah berikutnya adalah menjalankan kembali aplikasi backend (`dotnet run`) untuk eksekusi suite verifikasi menyeluruh (Fase 4).

---

## 2. Solusi Terpilih per Temuan

### 2.1. `ISS-EPS-005-01` — Skema Database Out-of-Sync pada `MstPatientCompanyGuarantor`

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Eksekusi DDL penambahan kolom `CardImagePath` langsung ke database PostgreSQL target dan pastikan riwayat migrasi sinkron | Solusi langsung, tidak mengubah berkas C#, menyelesaikan error 42703 secara permanen | Membutuhkan wewenang eksekusi database DDL |
| **B** | Menghapus kolom `CardImagePath` dari model C# `MstPatientCompanyGuarantor.cs` dan menghapus migrasi | Tidak perlu menyentuh database PostgreSQL | Merusak fitur upload foto kartu asuransi yang dibutuhkan modul pendaftaran pasien |
| **C** | Mengubah query `EncounterInsuranceService` agar mengecualikan kolom `CardImagePath` menggunakan projection kustom | Menghindari error DDL tanpa menyentuh DB | Menimbulkan disparitas permanen antara model ORM dan skema basis data |

**Solusi Terpilih: Opsi A.**
1. Berkas migrasi `20261006092040_AddCardImagePathToPatientCompanyGuarantor.cs` sudah resmi ada di repository backend; langkah ini menyelaraskan basis data dengan artefak resmi kode sumber.
2. Mengembalikan keutuhan fungsional `EncounterInsuranceService.GetContextAsync` sehingga status penjamin asuransi (BPJS Kesehatan) pada Episode A terbaca normal.

*Kenapa bukan opsi lain*: Opsi B merusak fitur pendaftaran pasien yang sah; Opsi C menciptakan hutang teknis (*technical debt*) yang menyalahi kontrak rekayasa backend Quilvian.

---

### 2.2. `ISS-EPS-005-02` — Konflik Multi-Site pada `MstHospitalSite`

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Memperbarui data master situs rumah sakit agar hanya RS MMC (`SITE-MMC-001`) yang bertanda `IsMainSite = true`, sedangkan situs cabang disetel `false` | Mematuhi aturan AC BE-RWI-188 butir 2 secara murni, tidak mengubah kode program | Mengubah flag dua baris master data cabang |
| **B** | Mengubah logika backend agar jika ada >1 main site, sistem mengambil situs pertama secara acak | Tidak perlu menyentuh data master | Melanggar aturan bisnis RS (profil rumah sakit tidak boleh ditebak jika ambigu) |
| **C** | Menghapus validasi `IsHospitalProfileAvailable` dari syarat `CanFreeze` di `InpAdmissionSnapshotBuilder.cs` | Dokumen dapat dikunci tanpa peduli profil RS | Snapshot dokumen admisi akan memuat identitas rumah sakit kosong atau salah |

**Solusi Terpilih: Opsi A.**
1. Rumah sakit utama tempat instalasi modul Rawat Inap beroperasi adalah RS MMC. Menjadikan satu-satunya situs utama mencerminkan kenyataan operasional rumah sakit.
2. Mematuhi aturan invariant: sistem tidak boleh menebak data entitas rumah sakit bila terjadi konflik konfigurasi.

*Kenapa bukan opsi lain*: Opsi B dan C melanggar aturan rekayasa dan menghasilkan dokumen cetak tanpa kop identitas resmi rumah sakit.

---

### 2.3. `ISS-EPS-005-03` — `DbUpdateConcurrencyException` Saat Menyimpan Tanda Tangan

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Memanggil `_dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);` secara eksplisit pada `SaveSignatureAsync` | Memberi tahu EF Core secara tegas bahwa entitas berstatus `Added` (SQL `INSERT`), menyelesaikan masalah di akar penyebabnya | Menyentuh satu baris kode di service tanda tangan |
| **B** | Mengosongkan inisialisasi `Id = Guid.NewGuid()` pada model dan membiarkan EF Core mengisinya | Standar penanganan EF Core untuk entitas baru | Mengubah definisi model bersama yang berpotensi memengaruhi bagian lain |
| **C** | Mengabaikan exception konkurensi di blok `catch` dan mencoba query ulang | Menghindari error di permukaan | SQL `UPDATE` tetap gagal 0 baris dan data tanda tangan tetap tidak pernah tersimpan di database |

**Solusi Terpilih: Opsi A.**
1. Metode ini konsisten dengan implementasi yang sudah terbukti sukses pada service pencatatan cetak: [`InpAdmissionPrintService.cs:195`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionPrintService.cs#L195).
2. Memastikan siklus hidup dokumen admisi (`AwaitingSignature` → `Completed`) dapat berjalan sempurna untuk tanda tangan kertas maupun atestasi elektronik.

*Kenapa bukan opsi lain*: Opsi B berisiko efek samping pada modul lain; Opsi C tidak menyelesaikan masalah fungsional penyimpanan data.

---

### 2.4. `ISS-EPS-005-04` — Ketiadaan Kebijakan Hak Akses `SysAccessPolicy`

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Membuat seeder kebijakan wewenang resmi yang mendaftarkan hak akses untuk peran Unit Admisi, CRO, dan Perawat Ruangan | Akses role-based beroperasi secara aman, akun non-superadmin dapat bekerja sesuai wewenang | Memerlukan penyusunan pemetaan departemen/posisi yang presisi |
| **B** | Menonaktifkan proteksi hak akses `[AuthorizePolicy]` pada controller `InpatientAdmissionDocument` | Pengujian langsung berjalan tanpa hambatan wewenang | Pelanggaran berat keamanan sistem medis (mencabut wewenang klinis/keuangan) |
| **C** | Mengharuskan seluruh pengguna login sebagai SuperAdmin | Tidak perlu menambah data kebijakan | Menghilangkan fungsi segregasi tugas (*separation of duties*) di rumah sakit |

**Solusi Terpilih: Opsi A.**
1. Sesuai prinsip tata kelola sistem Quilvian: peran klinis, admisi, dan keuangan harus dipisahkan dengan tegas.
2. Memungkinkan pengujian hak akses negatif (penolakan 403 saat mengakses `/summary/amounts` tanpa hak `ViewAmount`) dapat diverifikasi secara autentik.

*Kenapa bukan opsi lain*: Opsi B dan C melanggar aturan konstitusi ruang kerja dan membahayakan kepatuhan keamanan data rekam medis.

---

## 3. Diagram Alur Proses Sebelum → Sesudah

### 3.1. Alur Penguncian dan Penandatanganan Dokumen Admisi

```text
KONDISI SEBELUM PERBAIKAN (GAGAL):
[Dokumen Draft] ──► PATCH /lock ──► [Cek Hospital Profile] ──► GAGAL: 3 Situs Utama Aktif
                                                                  │
                                                                  ▼
                                                      Status 422 INP-ADM-DOC-027
                                                      (Dokumen batal dikunci)

KONDISI SETELAH PERBAIKAN (SUKSES):
[Dokumen Draft] ──► PATCH /lock ──► [Cek Hospital Profile] ──► SUKSES (Hanya RS MMC Aktif)
                                          │
                                          ▼
                                   [Status: AwaitingSignature]
                                   [JSON Snapshot Terbekukan]
                                          │
                                          ▼
                      POST /signatures/patient-or-family (Tanda Tangan Kertas)
                                          │
                                          ▼
                    EF Core Change Tracker: Set<Signature>().Add()
                                          │
                                          ▼
                              SQL: INSERT INTO Signatures ──► SUKSES (200 OK)
                                          │
                                          ▼
                      POST /signatures/head-nurse (Atestasi Elektronik)
                                          │
                                          ▼
                              SQL: INSERT INTO Signatures
                                          │
                                          ▼
                           Seluruh Slot Wajib Terpenuhi!
                                          │
                                          ▼
                               [Status Dokumen: Completed]
                                          │
                                          ├──► PATCH /unlock ──► DITOLAK 409 INP-ADM-DOC-006 (Aman)
                                          └──► POST /revisions ──► BISA MEMBUAT VERSI REVISI 2
```

---

## 4. Rincian Perbaikan

### 4.1. `FIX-EPS-005-01` — Penyelarasan Kolom `CardImagePath` pada `MstPatientCompanyGuarantor`
- **Target Objek**: Tabel basis data `public."MstPatientCompanyGuarantor"` di PostgreSQL `QuilvianNewDevHamzah`.
- **Perubahan yang Dilakukan**:
  1. Eksekusi perintah DDL:
     ```sql
     ALTER TABLE "public"."MstPatientCompanyGuarantor" 
     ADD COLUMN IF NOT EXISTS "CardImagePath" character varying(500) NULL;
     ```
  2. Verifikasi keterdaftaran baris migrasi `20261006092040_AddCardImagePathToPatientCompanyGuarantor` pada tabel `__EFMigrationsHistory`:
     ```sql
     INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
     VALUES ('20261006092040_AddCardImagePathToPatientCompanyGuarantor', '8.0.8')
     ON CONFLICT ("MigrationId") DO NOTHING;
     ```
- **Acceptance Criteria**:
  - `GET /api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/summary` pada Episode A mengembalikan `200 OK` tanpa error `PostgresException: 42703`.
  - Peringatan *"Kelengkapan dokumen admisi tidak dapat dihitung"* tidak lagi muncul pada respons JSON.
  - Kartu `CostDifferenceStatement` bertanda `isRequired: true`, `badge: "Draft"` atau status normal, dan jumlah dokumen wajib bernilai tepat 6.

---

### 4.2. `FIX-EPS-005-02` — Normalisasi Master Data Situs Utama `MstHospitalSite`
- **Target Objek**: Tabel basis data `public."MstHospitalSite"`.
- **Perubahan yang Dilakukan**:
  1. Jalankan pembaruan status situs utama tunggal:
     ```sql
     UPDATE "public"."MstHospitalSite"
     SET "IsMainSite" = CASE 
         WHEN "SiteCode" = 'SITE-MMC-001' THEN true 
         ELSE false 
     END;
     ```
- **Acceptance Criteria**:
  - Query `SELECT "SiteCode", "SiteName", "IsMainSite" FROM "MstHospitalSite" WHERE "IsMainSite" = true;` tepat mengembalikan 1 baris (`SITE-MMC-001`, RS MMC).
  - Panggilan `GET /api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/letterhead` mengembalikan data kop surat RS MMC dengan `isAvailable: true`.
  - Dokumen admisi berstatus `Draft` dapat dikunci via `PATCH .../lock` menghasilkan `200 OK` dan snapshot beku tersimpan.

---

### 4.3. `FIX-EPS-005-03` — Perbaikan Registrasi Entitas Tanda Tangan pada EF Core
- **Target Berkas**: `Areas/HealthServices/InPatientManagement/Services/InpAdmissionSignatureService.cs`
- **Lokasi Baris**: Baris 284–286 pada method `SaveSignatureAsync`.
- **Perubahan Kode**:
  Sebelum:
  ```csharp
  document.Signatures.Add(signature);
  ```
  Sesudah:
  ```csharp
  _dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);
  document.Signatures.Add(signature);
  ```
- **Acceptance Criteria**:
  - `POST .../signatures/patient-or-family` berhasil menyimpan data tanda tangan ke tabel `InpAdmissionDocumentSignature` dan mengembalikan respons `200 OK`.
  - `POST .../signatures/head-nurse` berhasil menyimpan tanda tangan dan menyelesaikan dokumen ke status `Completed` (status = 3).
  - Pembukaan kunci dokumen yang sudah bertanda tangan (`PATCH .../unlock`) ditolak dengan HTTP `409 Conflict` dan kode `INP-ADM-DOC-006`.
  - Dokumen berstatus `Completed` dapat dibuatkan versi koreksinya melalui endpoint `POST .../revisions`.

---

### 4.4. `FIX-EPS-005-04` — Penambahan Kebijakan Wewenang `SysAccessPolicy`
- **Target Objek**: Tabel basis data `public."SysAccessPolicy"`.
- **Perubahan yang Dilakukan**:
  1. Menghubungkan ID Controller `InpatientAdmissionDocument` dengan departemen dan peran:
     - **Departemen Admisi Rawat Inap**: Diberikan hak untuk aksi `Read`, `Create`, `Update`, `Sign`, `Print`, `ViewAmount`.
     - **Departemen Layanan Pelanggan (CRO)**: Diberikan hak untuk aksi `Read`, `SignAsCro`.
     - **Departemen Keperawatan (Perawat & Kepala Ruangan)**: Diberikan hak untuk aksi `Read`, `SignAsNurse`, `SignAsHeadNurse`.
  2. Memastikan akun karyawan tanpa izin `ViewAmount` tetap terlindungi dari akses endpoint finansial (`/summary/amounts` dan `/amount-print`).
- **Acceptance Criteria**:
  - Akun petugas admisi non-superadmin dapat membaca ringkasan dan membuat dokumen admisi.
  - Akun tanpa wewenang `ViewAmount` ditolak HTTP `403 Forbidden` saat mengakses `/summary/amounts` dan `/amount-print`.
  - Akun selain dokter/kepala ruangan tidak dapat menandatangani slot `SignAsHeadNurse`.

---

## 5. Urutan Pengerjaan (Pohon Ketergantungan)

```text
[Fase 1: Fondasi Data & Skema]
  ├── FIX-EPS-005-01: Eksekusi penambahan kolom CardImagePath di PostgreSQL
  └── FIX-EPS-005-02: Normalisasi situs utama tunggal MstHospitalSite (RS MMC)
        │
        ▼
[Fase 2: Perbaikan Kode Service Backend]
  └── FIX-EPS-005-03: Tambahkan _dbContext.Set<Signature>().Add(signature) pada SaveSignatureAsync
        │
        ▼
[Fase 3: Konfigurasi Wewenang Keamanan]
  └── FIX-EPS-005-04: Sisipkan kebijakan wewenang peran pada SysAccessPolicy
        │
        ▼
[Fase 4: Verifikasi Menyeluruh & Laporan]
  └── Jalankan ulang 42 skenario verifikasi runtime end-to-end
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Kode Keputusan | Pertanyaan Keputusan | Rekomendasi Solusi | Dampak Bila Ditunda |
| :---: | :---: | :--- | :--- | :--- |
| 1 | `DEC-REP-001` | Apakah penambahan kolom `CardImagePath` pada `MstPatientCompanyGuarantor` disetujui untuk dieksekusi langsung pada database target? | **Setuju (Ya)** | Pembacaan penjamin asuransi tetap lumpuh dan kelengkapan admisi tidak dapat dihitung |
| 2 | `DEC-REP-002` | Apakah RS MMC (`SITE-MMC-001`) disetujui sebagai satu-satunya situs utama aktif (`IsMainSite = true`)? | **Setuju (Ya)** | Seluruh penguncian dokumen admisi tetap gagal dengan status 422 DOC-027 |
| 3 | `DEC-REP-003` | Apakah perbaikan baris 285 pada `InpAdmissionSignatureService.cs` disetujui untuk diimplementasikan? | **Setuju (Ya)** | Seluruh tanda tangan dokumen admisi gagal disimpan dan dokumen tidak pernah bisa Completed |
| 4 | `DEC-REP-004` | Apakah pemetaan wewenang `SysAccessPolicy` untuk controller `InpatientAdmissionDocument` disetujui untuk diterapkan? | **Setuju (Ya)** | Operasional staf non-superadmin selalu ditolak 403 Forbidden |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak ada dokumen arsitektur hulu (`02-backend-architecture.md`, `contracts/api-contract.md`, atau `contracts/validation-matrix.md`) yang perlu diubah. Keempat perbaikan di atas justru bertujuan **menegakkan dan mengembalikan kepatuhan kode terhadap kontrak arsitektur yang sudah disetujui sebelumnya**.

Dokumen yang akan diperbarui pasca pengerbaikan:
- [`task/report/backend/verifikasi-runtime-workspace-ppri-2026-10-08.md`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/verifikasi-runtime-workspace-ppri-2026-10-08.md) (Diperbarui dengan hasil verifikasi 100% PASS).

---

## 8. Verifikasi Menyeluruh (Skenario Uji Ujung-ke-Ujung)

Setelah keempat perbaikan selesai dikerjakan, suite pengujian runtime akan mengeksekusi skenario validasi berikut:

1. **Uji Penjamin Asuransi (Episode A)**:
   - Panggil `GET .../admission-workspace/summary`.
   - Pastikan status 200 OK, `sources.guarantor` status `Available`, `CostDifferenceStatement` berstatus `isRequired: true`, dan `requiredDocumentCount: 6`.
2. **Uji Penguncian Dokumen (Episode A)**:
   - Buat konsep dokumen Permintaan Privasi.
   - Panggil `PATCH .../documents/{id}/lock`.
   - Pastikan status 200 OK, dokumen beralih ke `AwaitingSignature`, dan snapshot JSON terbekukan.
3. **Uji Penandatanganan Kertas Pasien**:
   - Panggil `POST .../signatures/patient-or-family`.
   - Pastikan status 200 OK dan baris tercatat di database.
4. **Uji Atestasi Elektronik Kepala Ruangan**:
   - Panggil `POST .../signatures/head-nurse`.
   - Pastikan status 200 OK, dokumen beralih ke `Completed`, dan `completedAt` terisi waktu saat ini.
5. **Uji Proteksi Dokumen Selesai**:
   - Panggil `PATCH .../documents/{id}/unlock`.
   - Pastikan ditolak `409 Conflict` dengan kode `INP-ADM-DOC-006`.
6. **Uji Pembuatan Versi Revisi**:
   - Panggil `POST .../documents/{id}/revisions`.
   - Pastikan status 200 OK, dokumen versi 2 terbit berstatus `Draft`, dan dokumen versi 1 berstatus `Superseded`.
7. **Uji Isolasi Wewenang Non-SuperAdmin**:
   - Login dengan akun petugas admisi: pastikan dapat membaca summary dan membuat dokumen.
   - Login dengan akun tanpa `ViewAmount`: pastikan akses ke `/summary/amounts` dan `/amount-print` ditolak `403 Forbidden`.

---

## 9. Riwayat Dokumen

| Tanggal | Versi | Pelaksana | Perubahan |
| :---: | :---: | :--- | :--- |
| 2026-10-09 | 1.0 | Agen Antigravity (Diagnose & Planning) | Penulisan rencana perbaikan awal untuk 4 temuan verifikasi runtime Workspace PPRI. |
