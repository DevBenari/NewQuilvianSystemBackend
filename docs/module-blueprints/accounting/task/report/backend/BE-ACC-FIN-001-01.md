# Laporan Perubahan Backend — `BE-ACC-FIN-001-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-FIN-001-01` |
| Judul | Kode otomatis dan jenis warisan |
| Slice | Gelombang pengiriman `FIN-2` |
| Roadmap | `roadmap/backend-roadmap-finishing.md` revisi 2, kartu `BE-ACC-FIN-001-01` |
| Trace | ACC-FIN-001 (task analis COA 25 November 2026); `FR-FIN-050`, `051`, `052`, `053`, `058`; `ACC-DEC-162`, `163`, `164`, `166`, `170`, `172` |
| Contract version | `ACC-API-0.18` (`GET /chart-of-accounts/next-code`, `POST`, `PUT`); `ACC-VALIDATION-0.13` `VAL-COA-01`..`06`, `13`..`15`; `ACC-PERMISSION-0.9` — approved Rizki 25 November 2026 (`GATE-DESAIN-1125`) |
| Dependency | — (tidak ada prasyarat teknis) |
| Klasifikasi | `STANDARD` — satu endpoint baru, dua endpoint diperluas, nol migration, nol hak baru |
| Task mode | `BACKEND` — perintah Rizki 25 November 2026 "yaa mulai kerjakan" sesudah roadmap revisi 2 |
| Target tulis | `MasterData/ChartOfAccount/` (service, berkas `partial` baru, DTO, controller); laporan ini; baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `0f67c5c2` (branch `rizkiG`; working tree berisi dokumen desain dan roadmap 25 November yang belum di-commit) |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-9 `RUNTIME VERIFIED` 25 November 2026: uji API `01-uji-api.mjs` 14/15 + uji tambahan `02-uji-jenis-tak-dikenal.mjs` 2/2 (A3 gagal hanya pada asersi pesan jalur query; penolakan `400` benar, jalur body terbukti di B2). Batas kapasitas 9/999/99 `SOURCE VERIFIED` (sesuai kartu). Build `PASS` (Rizki, DLL 15:06:02 WIB); QBE Strict `PASS`. UAT belum dijalankan — milik tim UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `MasterData/ChartOfAccount` |
| Pemilik / prefix | Rizki / `Acc` — registry backend baris 42, `ACTIVE` |
| Keberlakuan | `NEW CODE` (`AccChartOfAccountService.Code.cs`, action `GetNextCode`, dua DTO) dan `TOUCHED LEGACY` (`CreateAsync`, `UpdateAsync`, `PeriksaIsianDasar`, `CreateChartOfAccountRequest`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001`, `QBE-LOG-001` (log `ChartOfAccount.Create` lama tetap) |
| Tidak berlaku | `QBE-DB-*`, `QBE-ENT-*`, `QBE-CFG-*`, `QBE-PAGE-001` — tidak ada entity, konfigurasi, migration, atau daftar berhalaman baru |
| Standar endpoint | Master data. Sembilan endpoint baseline **tidak** dilengkapi pada task ini: controller COA lama memakai `activate`/`deactivate`, bukan `PATCH /{id}/status`, tanpa `filters/metadata`, `summary`, dan `DELETE` — kekurangan lama, di luar cakupan kartu, dicatat di bagian 7 |
| Role access | `[AccessAction("Create", "Create Chart of Account", …)]` + `[AccessPermission("ChartOfAccount", "Create")]` — argumen pertama sama dengan `ControllerName = "ChartOfAccount"`, argumen kedua sama dengan Action. Nol hardcode peran |

## 1. Masalah yang diperbaiki

Petugas harus mengetik sendiri kode akun dan memilih jenis akun bebas dari induknya. Akibatnya kode
tidak berpola, dua petugas dapat memilih kode yang sama, dan akun anak dapat berjenis lain dari
induknya (`C-05`). Keputusan `ACC-DEC-163`..`166`, `170`, `172` menetapkan kode disusun backend dan
jenis anak diwarisi.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Kode akun baru disusun backend menurut pola yang ada; jenis anak selalu sama dengan induknya |
| Pelaku | Pemegang `ChartOfAccount : Create` (pratinjau, tambah); `ChartOfAccount : Update` (ubah) |
| Pemicu | Layar Tambah Akun (`FE-ACC-FIN-001-01`) memanggil pratinjau saat induk atau jenis dipilih, lalu menyimpan tanpa kode; skrip impor mengirim kode sendiri |
| Langkah | 1) Pratinjau: periksa badan hukum, induk, induk bertransaksi, jenis, tingkat; susun kode tanpa kunci. 2) Simpan: pemeriksaan yang sama, lalu di dalam transaksi ambil kunci penomoran, susun kode final, simpan, commit |
| Jalur tidak normal | Akar jenis sudah ada → `409`; nomor penuh / kode terlalu panjang → `409`; jenis kosong untuk akun tingkat 1 atau tidak dikenal → `400`; jenis berbeda dari induk → `409`; kode bentrok pada detik simpan → `409`, simpan ulang |
| Hasil | Akun tersimpan dengan kode final; pesan "Akun berhasil ditambahkan dengan kode {kode}." |

**Contoh dari data dev 25 November 2026.** Induk 1-1000 memuat 1-1002, 1-1003, 1-1004, dan 1002 (di luar
pola). Pratinjau memberi `1-1005`. Induk 1-0000 memuat empat grup → `1-5000`. Induk 3-0000 memuat 3-1000
dan 3-3000 → `3-4000` (celah 3-2000 tidak diisi).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MasterData/ChartOfAccount/Services/AccChartOfAccountService.cs` | Kata kunci `partial`. `CreateAsync`: kode boleh kosong; pemeriksaan kode kembar hanya untuk kode manual; jenis lewat `TentukanJenis`; transaksi + kunci + penyusunan kode di dalamnya; tangkapan `DbUpdateException` indeks unik → `409`; pesan sukses memuat kode. `UpdateAsync`: induk baru berjenis lain → `409`. `PeriksaIsianDasar`: parameter `kodeBolehKosong` (bawaan `false`, perilaku `UpdateAsync` tidak berubah) |
| `MasterData/ChartOfAccount/Services/AccChartOfAccountService.Code.cs` | **Baru.** `GetNextCodeAsync`, `TentukanJenis`, `JenisBedaInduk`, `NamaJenis`, `KunciPenomoranAsync`, `SusunKodeAsync`, `UsulkanKode`, `NomorTerbesar`, `KodePenuh`, `MelanggarIndexKodeAkun`; konstanta `NamaIndexKodeAkun`, `PanjangKodeMaksimum` |
| `MasterData/ChartOfAccount/DTOs/ChartOfAccountDtos.cs` | `CreateChartOfAccountRequest.AccountCode` → `string?` (tanpa `[Required]`, tetap `[MaxLength(20)]`); `AccountType` → `AccountType?`; + `ChartOfAccountNextCodeQuery`, `ChartOfAccountNextCodeResponse` |
| `MasterData/ChartOfAccount/Controllers/ChartOfAccountController.cs` | + `GET next-code` |

Nol baris komentar baru. Tiga baris komentar lama di `CreateAsync` ("Akun baru belum punya turunan…")
ikut masuk blok `try` dan hanya bergeser indentasinya; isinya tidak disunting. Nol entity, migration,
konfigurasi EF, hak akses, atau baris `Program.cs` baru.

### 3.2 Aturan penyusunan kode (`02-backend-architecture.md` bagian 26.6)

| Kode induk | Bentuk | Pencarian nomor terbesar | Batas | Kunci |
| --- | --- | --- | --- | --- |
| Tanpa induk | `{jenis}-0000` | Ada atau tidak | Satu | `ACC_COA_KODE_{LE}_JENIS_{jenis}` |
| `^[1-5]-0000$` | `{d}-N000` | `^{d}-([1-9])000$` | 9 | sama |
| `^[1-5]-[1-9]000$` | `{d}-{N}NNN` | `^{d}-{N}([0-9]{3})$` | 999 | `ACC_COA_KODE_{LE}_INDUK_{id}` |
| Selain itu | `{induk}.NN` | `^{induk}\.([0-9]{2})$` | 99; panjang ≤ 20 | sama |

Seluruh kode akun badan hukum yang belum dihapus (aktif maupun nonaktif) dibaca sekali lalu dicocokkan
di memori; kode induk di-`Regex.Escape`. Kunci `pg_advisory_xact_lock(hashtext(...))` hanya diambil bila
kode disusun otomatis dan provider Npgsql, lalu lepas saat transaksi selesai.

### 3.3 Dampak

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `ACC-API-0.18` untuk bagian kode dan jenis. Field `InheritedReportClassification`, `InheritedCurrentAssetCategory`, `InheritedFromAccountCode` pada `ChartOfAccountNextCodeResponse` **belum** ada — kolom klasifikasinya baru lahir di `BE-ACC-FIN-001-02`, dan field itu ditambahkan di sana |
| Kompatibilitas | Permintaan lama yang mengirim kode dan jenis sama dengan induk tetap `201`. Pesan sukses berubah dari "Akun berhasil ditambahkan." menjadi "… dengan kode {kode}." — layar lama hanya menampilkan pesan |
| Database | `NOT APPLICABLE` untuk schema. Menulis baris `AccChartOfAccount` seperti sebelumnya |
| Log | `ChartOfAccount.Create` lama; muatan request (kode kosong bila otomatis), pesan hasil memuat kode final |

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Master Data / Chart of Account

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/master-data/chart-of-accounts/next-code?LegalEntityId=&ParentAccountId=&AccountType=` | Pratinjau kode, tingkat, jenis, usulan saldo normal | `ChartOfAccount : Create` |
| `POST` | `/api/v1/corporate/accounting/master-data/chart-of-accounts` | Tambah akun; `AccountCode` dan `AccountType` opsional | `ChartOfAccount : Create` |
| `PUT` | `/api/v1/corporate/accounting/master-data/chart-of-accounts/{id}` | Ubah akun; + `409` induk berjenis lain | `ChartOfAccount : Update` |

Contoh respons pratinjau (induk 1-1000):

```json
{ "accountCode": "1-1005", "accountLevel": 3, "accountType": 1, "parentAccountCode": "1-1000", "parentAccountName": "Kas dan Setara Kas", "suggestedNormalBalance": 1 }
```

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (4 berkas) | `Files evaluated: 4`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Pemindaian baris komentar baru pada diff dan berkas baru | 0 baris komentar baru (3 baris lama bergeser indentasi) | `PASS` |
| `dotnet build -p:RunAnalyzers=false` | Dijalankan Rizki; DLL 25 November 2026 15:06:02 WIB, sesudah suntingan source terakhir 14:55:07; DLL memuat `GetNextCodeAsync` dan rute `next-code` | `PASS` |
| `fin-001-01-20261125/01-uji-api.mjs` A1–A15 (15.21:25–15.21:34 WIB; izin uji tulis Rizki 25 November 2026) | 14 PASS, 1 FAIL (A3 sub-cek `accountType=0` lewat query: `400` benar, tetapi pesannya dari validasi model ASP.NET "One or more validation errors occurred.", bukan pesan service — asersi skrip terlalu ketat) | `PASS` dengan catatan |
| `fin-001-01-20261125/02-uji-jenis-tak-dikenal.mjs` B1–B2 (15.22:02 WIB, nol tulis) | B1 query `accountType=0` → `400`; B2 `POST` body `accountType` 0 dan 9 → `400` "Jenis akun tidak dikenal." dari service; jumlah akun 47 sebelum = 47 sesudah | `PASS` |

**Skrip uji.** Menulis ke DB dev lewat API, **nol SQL**. Seluruh akun uji diletakkan di bawah satu grup uji
baru (`1-5000 UJI COA Grup`, sekaligus bukti aturan tingkat 2) supaya bagan akun asli tidak tercampur, lalu
dinonaktifkan di akhir (A15). Kode yang terpakai permanen: `1-5000`, `1-5001`, `UJI-COA-LAMA`, `9901`,
`9901.01`..`.03`, `UJI-PANJANG-12345678`. Cara menjalankan: `NODE_TLS_REJECT_UNAUTHORIZED=0 node 01-uji-api.mjs`
dengan env `QUILVIAN_SUPERADMIN_*` dan `QUILVIAN_ASET_APPROVER_*`.

## 6. Acceptance criteria

| Kriteria | Status | Rencana bukti runtime |
| --- | --- | --- |
| AC-1 `POST` tanpa kode → kode otomatis, pesan memuat kode (`F25`) | `RUNTIME VERIFIED` | A1 (pratinjau 1-1005), A4 (201 `1-5000`, "Akun berhasil ditambahkan dengan kode 1-5000."), A5 (`1-5001`) |
| AC-2 Pratinjau `1-5000`, `3-4000`, `1-3001.01`; akar kedua `409` (`F26`, `F29`) | `RUNTIME VERIFIED` | A2; A3 p3a `409` pesan kontrak |
| AC-3 Kode manual dipertahankan; anaknya `{kode}.NN` (`F27`) | `RUNTIME VERIFIED` | A7 (`9901` → `9901.01`, pratinjau = simpan) |
| AC-4 Dua `POST` serentak → dua kode berurutan, nol `409` (`F28`) | `RUNTIME VERIFIED` | A8 (201 `9901.03` dan 201 `9901.02`) |
| AC-5 Kode hasil > 20 karakter → `409` (`F30`); batas 9/999/99 | `RUNTIME VERIFIED` (A9); batas kapasitas `SOURCE VERIFIED` (`UsulkanKode`: `>= 9`, `>= 999`, `>= 99`) | A9 |
| AC-6 Jenis diwarisi; berbeda → `409`; tanpa induk kosong/`0` → `400` (`F31`) | `RUNTIME VERIFIED` | A4 (jenis 1 diwarisi), A6 j1 `409`, A3 p3b `400`, B1, B2 |
| AC-7 Pindah induk berjenis lain → `409` (`F32`); kode belum `Posted` dapat diubah, sesudahnya terkunci (`F33`) | `RUNTIME VERIFIED` | A11; A12 (`1-5001` → `1-5001X` → `1-5001`; 1-1003 `409`) |
| AC-8 `next-code` tanpa `ChartOfAccount : Create` → `403` (`F40`) | `RUNTIME VERIFIED` | A13 (akun penyetuju uji, `403`) |
| AC-9 Permintaan lama (kode + jenis sama) tetap `201` | `RUNTIME VERIFIED` | A6 j2 (`UJI-COA-LAMA`) |
| Negatif kode 21 karakter `400`, kode kembar `409`, induk bertransaksi `409` | `RUNTIME VERIFIED` | A10; A3 p3d |
| Regresi `/tree`, `/options` | `RUNTIME VERIFIED` | A14 (keduanya `200`) |

**DoD.** AC-1..AC-9 terbukti; akun uji dinonaktifkan (A15, 8/8); laporan ditulis; roadmap dan traceability diperbarui → ✅.

**Keadaan DB dev sesudah uji.** Delapan akun uji nonaktif di bawah grup baru `1-5000 UJI COA Grup`: `1-5000`, `1-5001`, `UJI-COA-LAMA`, `9901`, `9901.01`, `9901.02`, `9901.03`, `UJI-PANJANG-12345678`. Bersaldo nol, tidak dipakai jurnal, keterangan berisi alasan uji. Akibatnya pratinjau grup Aset berikutnya kini `1-6000`.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan, tidak diubah | Controller COA belum memenuhi sembilan endpoint baseline master data (`filters/metadata`, `summary`, `PATCH /{id}/status`, `DELETE`); pola lama `activate`/`deactivate`. Di luar cakupan kartu |
| Risiko | Sedang-rendah. (1) Transaksi baru di `CreateAsync` — kegagalan selain indeks unik tetap dilempar seperti sebelumnya, transaksi dibatalkan lewat `DisposeAsync`. (2) Kunci per jenis untuk akar dan tingkat 2 lebih lebar dari "per induk", sengaja karena kode tingkat 2 unik per jenis (`ACC-DEC-172` butir 6) |
| Status Git | `M` `AccChartOfAccountService.cs`, `ChartOfAccountDtos.cs`, `ChartOfAccountController.cs`; `??` `AccChartOfAccountService.Code.cs`; laporan ini; dokumen blueprint pass 25 November |
| Langkah berikutnya | `FE-ACC-FIN-001-01` (Tambah Akun dengan kode otomatis) |
