# Laporan Perubahan Backend — `BE-LAB-88`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-88` |
| Judul | Spesifik Specimen pada respons specimen dan penggantian berdasar selisih |
| Slice | Susulan `MVP-7` — penutup `FE-LAB-32` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6ap |
| Trace | `LAB-DEC-167` (atas [`LAB-REQ-015`](../../../approval-requests/2026-10-06-permintaan-spesifik-specimen-pada-respons-specimen.md)); `LAB-DEC-098`, `LAB-DEC-107`, `LAB-DEC-112`; `VAL-105`, `VAL-150` |
| Contract version | `LAB-API-v1` **`r39`** bagian 34 dan `LAB-VAL-v1` **`r16`** bagian 18 — keduanya `approved` 2026-10-06 oleh Yoga Aji Pratama |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), 12 berkas diperiksa (1), 3 berkas diubah (0), logika sedang (1), kontrak API berubah aditif (2), perilaku persistence yang sudah ada (1), keamanan (0), UI (0) |
| Task mode | `BACKEND` — atas instruksi pemilik modul sesudah `LAB-REQ-015` disetujui |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/LaboratoryManagement/**` dan `docs/module-blueprints/laboratorium/**` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6924b689` (branch `yoga`, upstream `origin/yoga`) |
| Tanggal | 2026-10-06 |
| Status | ✅ **`SELESAI`** — ketujuh AC terbukti: harness **20/20**; terjemahan SQL dan data asli terbukti **baca-saja langsung ke PostgreSQL dev** (3/3); **lewat HTTP terhadap aplikasi berjalan 5/5**; build 0 error; nol migration, nol izin baru, nol endpoint baru milik task ini. Startup dev sempat terhalang migration modul lain; atas instruksi user **hanya** `20261001043557_AddMstDiagnosisGroup` diterapkan ke devYoga (lihat 5) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `LaboratoryManagement / Laboratory` — `Lab`, `ACTIVE` (registry 2026-09-02) |
| Keberlakuan | `TOUCHED LEGACY` pada `LabSpecimenService` dan `LabSpecimenCorrectionService`; `NEW CODE` pada kelas DTO `LabSpecimenDetailItem` |
| Entity / migration baru | Nol — tabel `LabSpecimenDetail` beserta indeks unik `IX_LabSpecimenDetail_SpecimenId_DetailTypeId` sudah ada |
| QBE yang berlaku | `QBE-SVC-001` (logika di service; controller tidak berubah), `QBE-API-001` (envelope dan route tetap), `QBE-DTO-001` (DTO, bukan entity), `QBE-VAL-001` (`VAL-150`), `QBE-TXN-001` (koreksi tetap dalam transaksinya), `QBE-LOG-001` (audit koreksi dan jejak ruas tetap beraktor), `QBE-PERM-001` (metadata akses tidak berubah) |
| Pengecualian QBE | `NONE` |

---

## 1. Masalah yang diperbaiki

Verifikasi `FE-LAB-32` di peramban (2026-10-02 dan 2026-10-06) menemukan bahwa koreksi Informasi Specimen
dari Halaman Hasil Mikrobiologi tidak aman bagi pilihan **Spesifik Specimen**:

| # | Masalah | Akibat bagi petugas |
| ---: | --- | --- |
| 1 | `LabSpecimenResponse` tidak membawa Spesifik Specimen yang tercatat | Layar tidak dapat mencentang pilihan yang sudah ada — formulir selalu kosong |
| 2 | `detailTypeIds` **menggantikan seluruh** pilihan | Mencentang satu kotak menghapus pilihan lain tanpa peringatan |
| 3 | Koreksi ditolak **seluruhnya** bila satu id yang dikirim sudah nonaktif | Contoh: wadah mencatat *Darah arteri* dan *Arterial cord blood specimen*; kepala instalasi menonaktifkan yang kedua. Petugas mengoreksi **volume** saja dengan mengirim ulang kedua pilihan → `422`, padahal volumenya yang salah |
| 4 | Penggantian menghapus lalu menambah ulang **semua** baris | Nama snapshot rincian yang tidak diubah tertimpa nama data induk terbaru — nama yang kelak diperbaiki mengubah arti bahan yang sudah tercatat |

---

## 2. Proses bisnis

1. **Pelaku:** petugas laboratorium pemegang `LabSpecimen : Update`, dari Halaman Hasil Mikrobiologi.
2. **Membaca:** layar memuat wadah pesanan lewat `GET /lab-specimens/by-order/{labOrderId}`. Setiap wadah
   kini menyebut Spesifik Specimen yang tercatat — **nama sebagaimana tercatat** dan apakah data induknya
   masih aktif.
3. **Mengoreksi:** layar mengirim himpunan Spesifik Specimen yang diinginkan. Backend membandingkannya
   dengan yang tercatat:

   | Id dikirim | Perlakuan |
   | --- | --- |
   | Sudah tercatat | **Dibiarkan** — baris, nama snapshot, dan keadaan nonaktifnya tidak disentuh |
   | Tercatat, tidak dikirim | Dilepas |
   | Baru | Ditambah dengan nama data induk saat ini; **wajib aktif** (`VAL-150`) |

4. **Jejak:** satu baris jejak berisi daftar nama lama dan baru; **nol** baris bila himpunannya sama.
5. **Jalur tidak normal:**
   - menambah rincian nonaktif atau id yang tidak ada → `422` *"Rincian specimen ini sudah tidak dipakai lagi
     dan tidak dapat ditambahkan."*;
   - rincian yang sama dua kali → `422` `VAL-105` (tetap);
   - hasil sudah Final → `422` `VAL-109` (tetap, sebelum apa pun dibaca);
   - `detailTypeIds` tidak dikirim → Spesifik Specimen tidak disentuh (21.4, tetap).

**Contoh berangka.** Wadah `X` mencatat {*Darah arteri*, *Arterial cord blood specimen*}; yang kedua kini
nonaktif.

| Kiriman | Hasil |
| --- | --- |
| `volumeAmount: 3.25`, `detailTypeIds: [arteri, tali]` | `200`; 1 ruas (volume); rincian tetap dua baris yang sama |
| `detailTypeIds: [tali, arteri]` saja | `200`; 0 ruas; nol jejak; `UpdateDateTime` tidak bergeser |
| `detailTypeIds: [arteri]` | `200`; tali pusat dilepas; jejak *"Arterial cord blood specimen, Darah arteri"* → *"Darah arteri"* |
| lalu `detailTypeIds: [arteri, tali]` | `422` `VAL-150` — yang nonaktif tidak dapat dicentang ulang |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`;
`rules/backend/` suite skill; `LabSpecimenController.cs`; `LabSpecimenService.cs`; `LabSpecimenCorrectionService.cs`;
`LabFieldChangeRecorder.cs`; `LabSpecimenDtos.cs`; `LabSpecimenCorrectionDtos.cs`; `Models/LabSpecimenDetailType.cs`
(`LabSpecimenDetail`); `LabSpecimenDetailTypeConfiguration.cs`; kontrak `r26` 21.4/21.5 dan `r39` 34.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabSpecimenDtos.cs` | Ruas `SpecimenDetails` (`List<LabSpecimenDetailItem>?`) pada `LabSpecimenResponse`; kelas baru `LabSpecimenDetailItem` (`LabSpecimenDetailTypeId`, `DetailName`, `IsActive`) |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` | `GetByOrderAsync` memproyeksikan sub-koleksi `LabSpecimenDetails` per wadah: `!IsDelete`, urut `DetailNameSnapshot`, `IsActive` = data induk aktif dan tidak terhapus |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenCorrectionService.cs` | `ApplyDetailsAsync`: penggantian berdasar selisih; `VAL-150` hanya untuk tambahan; nilai kembali = jumlah rincian yang benar-benar ditambah atau dilepas (sebelumnya jumlah himpunan, sehingga kiriman tanpa perubahan pun menggeser `UpdateDateTime`) |

`MapResponse` (respons tindakan siklus hidup) dan daftar berhalaman `GET /` **tidak** diubah — ruasnya
`null` (*tidak dimuat*), dibedakan dari `[]` (*nol rincian*).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif**: `specimenDetails` pada `GET /lab-specimens/by-order/{labOrderId}`. **Satu perubahan perilaku** yang disetujui: `PATCH /{id}/correction` menerima kiriman ulang pilihan lama yang nonaktif. Respons `PATCH` tetap `ApiResponse<object>` berisi pesan — sesuai source sejak `BE-LAB-57`; dokumen 21.4 dikoreksi `r39` 34.1 |
| Database | `NOT APPLICABLE` — nol perubahan schema/entity, nol migration. Perilaku persistence: baris yang dipertahankan tidak lagi dihapus-dan-ditulis-ulang |
| Keamanan/Auth | `NOT APPLICABLE` — hak akses kedua endpoint tidak berubah (`LabSpecimen : Read` / `: Update`); ruas baru bukan identitas pasien |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-specimens/by-order/{labOrderId}` | Wadah satu pesanan — kini beserta `specimenDetails` (`labSpecimenDetailTypeId`, `detailName` snapshot, `isActive`) | `LabSpecimen : Read` |
| `PATCH` | `/lab-specimens/{id}/correction` | Koreksi Informasi Specimen; Spesifik Specimen diganti berdasar selisih, tambahan wajib aktif (`VAL-150`) | `LabSpecimen : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (server dimatikan lebih dulu) | **0 error**, 242 warning; nol warning dari ketiga berkas yang disentuh | `PASS` | Keluaran build |
| `.csproj` | Tidak berubah — nol pustaka baru | `PASS` | `git diff` |
| Harness `BE-LAB-88` (InMemory, `Microsoft.EntityFrameworkCore.InMemory` 9.0.18) | **20 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Baca-saja langsung ke PostgreSQL dev (`GetByOrderAsync` dengan Npgsql) | **3 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Lewat HTTP terhadap aplikasi berjalan — percobaan pertama | Aplikasi gagal start: `IcdDiagnosisGroupSeeder` → `42P01 relation "public.MstDiagnosisGroup" does not exist`. Migration `20261001043557_AddMstDiagnosisGroup` (modul lain, `27fd8fb4`, ikut merge `6924b689`) belum diterapkan ke devYoga | `EXISTING / ENVIRONMENT ISSUE` | Log startup; tidak terkait berkas task ini |
| Penerapan migration itu saja (instruksi user 2026-10-06) | Skrip `migrations script 20261001040508_AddCliDoctorCertificate 20261001043557_AddMstDiagnosisGroup` — murni aditif (kolom nullable `MstDiagnosis.DiagnosisGroupId`, tabel `MstDiagnosisGroup`, indeks, FK). Preflight: tabel belum ada, kolom belum ada, riwayat 275. **Uji kering** naik lalu turun dalam satu transaksi lalu `ROLLBACK`: sidik `md5` 23.169 baris `MstDiagnosis` kembali persis. Eksekusi: riwayat **275 → 276**, baris `MstDiagnosis` tetap 23.169. **15 migration modul lain tetap tertunda**, sesuai pilihan user | `PASS` | Keluaran program Npgsql |
| Startup sesudahnya | `/health` `200`, nol Fatal; 6 Error = penjadwal Absensi (`FK_HrdAttendanceProcessingRun_AspNetUsers_TriggeredByUserId`) yang sudah dikenal | `EXISTING / ENVIRONMENT ISSUE` | Log startup |
| **Lewat HTTP, superadmin** | **5/5**: H1 `by-order` `LAB-RSMMC-000014` membawa *Arterial cord blood specimen* dan *Darah arteri* (aktif) pada specimen uji, wadah lain `[]`; H2 urut nama; H3 `LAB-RSMMC-000001` ketiga wadah `[]`; H4 `GET /` → `specimenDetails` `null`; H5 `PATCH` himpunan sama urutan dibalik → `200` *"Nol ruas yang berubah."*, jejak tetap 3, rincian identik | `PASS` | Respons HTTP |

**Harness — 20/20.**

| Kelompok | Skenario | Hasil |
| --- | --- | --- |
| (a) | `by-order`: dua rincian sama persis dengan baris tabel, urut nama | `PASS` |
| (a) | Nama = snapshot *Darah arteri*, **bukan** nama data induk hari ini *Darah arteri (diperbaiki)* | `PASS` |
| (a) | `isActive` = keadaan data induk hari ini (tali pusat `false`, arteri `true`) | `PASS` |
| (b) | Wadah tanpa rincian → `[]`, bukan `null` | `PASS` |
| (g) | `MapResponse` siklus hidup → `specimenDetails` `null`, ruas lain tetap | `PASS` |
| (d) | Volume + himpunan sama yang memuat pilihan **nonaktif** → `200` (dulu `422`) | `PASS` |
| (c) | Baris rincian tidak berubah — Id dan snapshot sama persis; nol jejak Spesifik Specimen; jejak volume tetap | `PASS` ×2 |
| — | Himpunan sama, urutan lain, tanpa ruas lain → 0 ruas, nol jejak, `UpdateDateTime` tetap | `PASS` |
| (e) | Menambah rincian nonaktif → `422` dengan pesan `r16` persis; ditolak sebelum menulis | `PASS` ×2 |
| (e) | Id yang tidak ada di data induk → `422` | `PASS` |
| — | `VAL-105` rincian ganda tetap `422`; `detailTypeIds` tidak dikirim → tidak disentuh | `PASS` ×2 |
| (f) | Melepas satu dari dua → arteri tetap baris dan snapshot yang sama; **satu** jejak lama → baru | `PASS` ×2 |
| (f) | Pilihan nonaktif yang sudah dilepas tidak dapat dicentang ulang → `422` | `PASS` |
| — | Menambah rincian aktif → snapshot nama Indonesia saat ini; jejak *"Darah arteri"* → *"Darah arteri, Darah vena"*; `by-order` membaca keadaan terbaru | `PASS` ×3 |

**PostgreSQL dev, baca-saja — 3/3.** Hanya `GetByOrderAsync` dipanggil; nol tulis.

| Skenario | Hasil |
| --- | --- |
| `LAB-RSMMC-000014`: specimen uji `14e5794d…` membawa `Arterial cord blood specimen` dan `Darah arteri`, keduanya aktif; wadah lain `[]` | `PASS` |
| `LAB-RSMMC-000001` (tanpa rincian): ketiga wadah `[]` | `PASS` |
| Terjemahan: **satu** perintah SQL — `LEFT JOIN` subkueri `LabSpecimenDetail` ⨝ `LabSpecimenDetailType` dengan `IsActive AND NOT IsDelete`; nol evaluasi sisi klien | `PASS` |

Uji manual: `NOT APPLICABLE` — task backend; layar milik sisa `FE-LAB-32`.

**Tidak dijalankan:**
- **Butir (d)–(e) terhadap database dev** — menuntut menonaktifkan data induk Spesifik Specimen bersama;
  dibuktikan di harness sesuai kartu task.
- **Analyzer** — build memakai `-p:RunAnalyzers=False` (build penuh dengan analyzer melewati batas waktu);
  warning compiler tetap disaring.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (a) `by-order` memuat `specimenDetails` sama persis dengan baris `LabSpecimenDetail` wadah itu, nama = snapshot | Terpenuhi | Harness (a); PostgreSQL dev |
| (b) Wadah tanpa rincian → `[]` | Terpenuhi | Harness (b); PostgreSQL dev |
| (c) Koreksi volume dengan himpunan yang sama → nol jejak Spesifik Specimen; baris dan snapshot tidak berubah | Terpenuhi | Harness (c) |
| (d) Rincian tercatat yang kemudian dinonaktifkan tetap dapat dikirim ulang | Terpenuhi | Harness (d) |
| (e) Menambah rincian nonaktif → `422` `VAL-150` | Terpenuhi | Harness (e) |
| (f) Melepas satu dari dua → satu baris jejak berisi nama lama dan baru | Terpenuhi | Harness (f) |
| (g) Respons siklus hidup tidak berubah bentuk selain ruas `null` | Terpenuhi | Harness (g); daftar berhalaman tidak memproyeksikan ruas ini |
| DoD — build hijau (`-p:RunAnalyzers=False`) | Terpenuhi | 0 error |
| DoD — laporan `BE-LAB-88.md`; kolom *Status* 34.1 diperbarui | Terpenuhi | Berkas ini; `api-contract.md` 34.1 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 242 warning compiler bawaan repository; nol dari berkas yang disentuh |
| Masalah yang diketahui | **15 migration modul lain masih tertunda** di `QuilvianNewDevYoga` (Keuangan ×11, IGD, Klinik, Pembelian ×2); layar modul-modul itu dapat gagal di dev sampai diterapkan. Hanya `AddMstDiagnosisGroup` yang diterapkan, atas instruksi user, karena itu satu-satunya yang menghalangi startup |
| Risiko tersisa | **Rendah.** Konsumen lama mengabaikan ruas baru. Perilaku yang berubah hanya melonggarkan penolakan atas pilihan lama yang nonaktif, sesuai `LAB-DEC-167` |
| Perubahan sampingan | `NONE` di repository. Harness dan pemeriksaan PostgreSQL berada di scratchpad sesi, di luar repository |
| Interupsi | `NONE` |
| Status Git | Source: ` M` `LabSpecimenDtos.cs`, `LabSpecimenService.cs`, `LabSpecimenCorrectionService.cs`. Dokumen: laporan ini, `api-contract.md`, `backend-roadmap.md`, `frontend-roadmap.md`, `traceability.md`, berkas `LAB-REQ-015`, beserta perubahan dokumen persetujuan `LAB-DEC-167` dan `FE-LAB-42` yang belum ter-commit. **Nol operasi Git dijalankan** |
| Langkah berikutnya | Sisa `FE-LAB-32`: formulir koreksi diisi dari `specimenDetails` dan nilai tersimpan lainnya — aplikasi dev kini dapat start untuk verifikasi layarnya |
