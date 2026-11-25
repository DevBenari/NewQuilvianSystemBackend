# Laporan Perubahan Backend — `BE-AST-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-026` |
| Judul | Akun kontrol dan pusat biaya |
| Slice | `NLI-S2` (gelombang `MVP-4`) |
| Roadmap | `nilai/roadmap/backend-roadmap.md` `REV-1`, kartu `BE-AST-026` |
| Trace | `AST-DEC-036`; `AST-DES-011`, `035`; `contracts/api-contract.md` 2.3, 2.4; `data/data-dictionary.md` 2.3; `contracts/validation-matrix.md` `AST-VAL-NLI-014`, `016` |
| Contract version | `AST-NLI-1.0.0` — `approved` 2026-11-12 (`AST-DEC-051`) |
| Dependency | `BE-AST-025` ✅ (tabel `AstSubledgerAccountSetup`, `AstUnitCostCenter`) |
| Klasifikasi | `STANDARD` — 1 service, 1 controller, DTO, kode galat; baca `AccChartOfAccounts` (Akuntansi) dan `MstCostCenter` (HR) tanpa menulis ke modul lain |
| Task mode | `BACKEND` — perintah Rizki 2026-11-24 "lanjutkan"; pasangan `FE-AST-019` |
| Target tulis | `Areas/Corporate/AssetManagement/Valuation/{Services,Controllers,DTOs}/`, `AssetManagementServiceCollectionExtensions.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `ce9e1245` (branch `rizkiG`) + `BE-AST-025` belum di-commit |
| Tanggal | 24 November 2026 |
| Status | ✅ SELESAI 24 November 2026 — ketiga kriteria terpetakan ke source dan dibuktikan runtime **34/34**; build Rizki DLL 10:18:59 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Valuation` |
| Pemilik / prefix registry | `AssetManagement / Valuation / Nilai Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-API-*`, `QBE-PERM-*` (resource baru `AssetSubledgerSetup` `Read`/`Update`, `[AccessAction]` = `[AccessPermission]`), `QBE-SVC-*` |
| Tidak berlaku | `QBE-DB-*` — tanpa migration (tabel dari `BE-AST-025`) |

---

## 1. Masalah yang diperbaiki

Proses penyusutan (`BE-AST-027`) harus tahu ke pusat biaya mana beban setiap aset dibebankan (`AST-DEC-036`), dan
saldo subledger (`BE-AST-030`) harus tahu akun kontrol Akuntansi tiap kelompok (`AST-DES-035`). Belum ada tempat
menetapkan keduanya.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Valuation/Services/AssetSubledgerSetupService.cs` | Baru — daftar dan tetapkan akun kontrol per kelompok (upsert); daftar pusat biaya per unit beserta sumbernya; petakan, ubah, hapus pemetaan; pilihan pusat biaya; `ResolveForUnitsAsync` publik untuk dipakai pratinjau penyusutan |
| `Valuation/Controllers/AssetSubledgerSetupController.cs` | Baru — route `valuation/`, resource `AssetSubledgerSetup`, 6 operasi |
| `Valuation/DTOs/AssetValuationDtos.cs` | + `SubledgerAccountSetupRequest/Response`, `SubledgerAccountRefResponse`, `UnitCostCenterQuery/Request/Response`, `CostCenterOptionResponse` |
| `Valuation/Services/AssetValuationRules.cs` | + kode `AST-VAL-NLI-017`, `018`, `028` dan pesannya |
| `AssetManagementServiceCollectionExtensions.cs` | + `AssetSubledgerSetupService` |

Tanpa baris komentar.

### 2.1 Aturan

| Aturan | Source |
| --- | --- |
| Kode akun harus **ada** di daftar akun badan hukum, **aktif**, dan **dapat diposting** (`AST-DES-035`: "kode harus ada di daftar akun Akuntansi"); akun induk ditolak karena tidak bisa menampung saldo | `SetAccountSetupAsync`, `LoadAccountsAsync` |
| Akun yang belum ditandai akun kontrol di Akuntansi tetap diterima, tetapi diberi peringatan; kelompok aktif tanpa akun kontrol diberi peringatan `AST-VAL-NLI-016` | `BuildAccountSetupsAsync`, `Describe` |
| Pusat biaya pembebanan (`AST-DES-011`): pemetaan bila ada; bila tidak, satu-satunya pusat biaya aktif milik unit; nol atau lebih dari satu → **Belum Jelas** (`AST-VAL-NLI-014` di pratinjau) | `ResolveForUnitsAsync` |
| Pemetaan ke pusat biaya yang kemudian dinonaktifkan HR → unit tetap Pemetaan tetapi **belum jelas** | `ResolveForUnitsAsync` (`IsUsable`) |
| Hapus pemetaan = hapus lunak; pemetaan ulang memulihkan baris yang sama (unik `OrganizationUnitId` tanpa filter) | `DeleteUnitCostCenterAsync`, `SetUnitCostCenterAsync` |

### 2.2 Delta terhadap kontrak

| Delta | Alasan |
| --- | --- |
| + `GET /unit-cost-centers/cost-center-options` (`AssetSubledgerSetup : Read`) | Pilihan pusat biaya HR (`/human-resource/.../cost-centers/options`) menuntut `CostCenter : Read`, sedangkan pengguna layar ini Kepala Akuntansi. Endpoint ini hanya membaca pusat biaya aktif badan hukum (maks. 500) |
| `GET /unit-cost-centers` menerima `search`, `unclearOnly` | Saringan "belum jelas" (`FE-AST-019` kriteria 2) |
| Respons akun kontrol memuat nama akun, `isKnown`, `isControlAccount`, `warnings`, `updatedAt` | Layar menampilkan nama akun dan catatan per kelompok |
| Satu controller, bukan bagian dari `AssetValuationParametersController` | Resource hak akses berbeda (`AssetSubledgerSetup`); sejalan dengan `BE-AST-015` yang memecah controller per resource |
| Service tersendiri, bukan `AssetValuationParameterService` | Menjaga ukuran service; `ResolveForUnitsAsync` akan dipanggil `AssetDepreciationRunService` |
| Kode galat baru `AST-VAL-NLI-017` (kode akun), `018` (pusat biaya nonaktif), `028` dipakai untuk benturan simpan bersamaan | Matriks validasi belum punya kode untuk kasus ini |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (5 berkas, diulang sesudah endpoint pilihan ditambahkan) | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` |
| Build | DLL 2026-11-24 10:18:59 (Rizki); backend jalan 10:20:20 | `PASS` |
| Runtime | **34/34** (putaran 2) — `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-026-20261124/ast026.mjs` → `ast026.json` | `PASS` |

**Putaran 1 — 33/34.** `SETUP-AKUN-AKUNTANSI` gagal karena skrip mengharapkan 200 padahal API daftar akun Akuntansi
membalas `201 Created`; ketiga akun berhasil dibuat. Bukti putaran 1: `ast026-putaran1.json`.

### 3.1 Data yang ditulis ke DB dev (keputusan Rizki 2026-11-24, permanen)

| Data | Isi |
| --- | --- |
| Daftar akun Akuntansi PT Metropolitan Medical Centre | `1-4000` Aset Tetap (induk, level 2 di bawah `1-0000`); `1-4001` Aset Tetap – Alat Medis (postable, akun kontrol, debit); `1-4002` Akumulasi Penyusutan – Alat Medis (postable, akun kontrol, kredit — kontra aset) |
| `AstSubledgerAccountSetup` | Kelompok `ALKMED` → `1-4001` / `1-4002`, tanpa akun penurunan nilai |
| Pusat biaya uji | Dibuat lalu **dihapus** di setiap putaran; pemetaan uji dihapus; daftar unit kembali seperti semula (5 unit Otomatis) |

### 3.2 Skenario runtime (putaran 2)

| Skenario | Hasil |
| --- | --- |
| Hak `AssetSubledgerSetup` = `Read`, `Update` | Lulus |
| Daftar akun kontrol: 7 kelompok; kelompok aktif tanpa akun kontrol membawa peringatan "tidak akan dikirim untuk rekonsiliasi" | Lulus |
| Kode wajib kosong → 400 `AST-NLI-FIELD`; kode tak dikenal `9-9999` → 400 `017` menyebut kode itu saja; akun induk `1-4000` → 400 `017`; kode > 50 → 400; kelompok tidak ada → 404 | Lulus |
| Simpan ALKMED dengan akun penurunan nilai `1-1004` (postable, bukan akun kontrol) → 200, peringatan "belum ditandai akun kontrol" | Lulus |
| Ubah tanpa akun penurunan nilai → 200, tanpa peringatan; daftar memuat nama akun dan waktu ubah | Lulus |
| Akun tanpa hak: daftar dan simpan akun kontrol → 403 | Lulus |
| Daftar unit awal: 5 unit Otomatis, masing-masing 1 pusat biaya aktif; saringan belum jelas = 0; pilihan pusat biaya = 5 | Lulus |
| Pusat biaya kedua untuk Rawat Inap → unit **Belum Jelas** "punya 2 pusat biaya aktif"; saringan belum jelas = 1; cari "rawat inap" | Lulus |
| Pemetaan tanpa pusat biaya → 400; unit tidak ada → 404; pusat biaya tidak ada → 404 | Lulus |
| Petakan ke pusat biaya uji → Pemetaan, jelas | Lulus |
| Pusat biaya uji dinonaktifkan HR → unit Pemetaan tetapi belum jelas "tidak aktif lagi"; petakan ulang ke pusat biaya nonaktif → 400 `018` | Lulus |
| Ubah pemetaan ke pusat biaya asli; hapus → kembali Otomatis; hapus lagi → 404; petakan ulang sesudah dihapus (pemulihan baris) → 200 | Lulus |
| Akun tanpa hak: daftar unit dan hapus pemetaan → 403 | Lulus |
| Rapikan: pemetaan dan pusat biaya uji dihapus; daftar unit sama dengan awal | Lulus |

### 3.3 Temuan di luar lingkup

Pusat biaya uji pada putaran 1 dan 2 sama-sama mendapat kode **`CC-MMC-00006`**: penomoran pusat biaya HR memakai ulang
nomor sesudah baris dihapus lunak, sehingga kode ganda bisa muncul. Milik modul HR; dicatat untuk pemiliknya.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Endpoint 2.3–2.4 | `AssetSubledgerSetupController` (+ pilihan pusat biaya, delta 2.2) | 3.2 | ✅ |
| 2. Kode akun divalidasi terhadap daftar akun Akuntansi (baca saja) | `LoadAccountsAsync` atas `AccChartOfAccounts` badan hukum | `017` untuk kode tak dikenal dan akun induk; simpan akun valid | ✅ |
| 3. Daftar unit "belum jelas" = tanpa pemetaan dengan 0 atau > 1 pusat biaya aktif | `ResolveForUnitsAsync` | Rawat Inap 2 pusat biaya → Belum Jelas; saringan `unclearOnly` | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, runtime | Terpenuhi |
| `AT-NLI-32` (galat pratinjau unit dua pusat biaya) | Bagian pratinjau milik `BE-AST-027`; dasar datanya (`ResolveForUnitsAsync`) sudah teruji di sini |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Menunggu izin Rizki |

---

## 5. Langkah berikutnya

- `FE-AST-019` diuji sesudah frontend di-build ulang (lihat laporannya).
- `BE-AST-027` memanggil `ResolveForUnitsAsync` untuk `AST-VAL-NLI-014` dan mengambil akun kontrol untuk peringatan `016`.
