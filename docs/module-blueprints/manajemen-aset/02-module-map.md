# Manajemen Aset & Inventaris — Peta Modul

| Field | Value |
|---|---|
| Blueprint | `AST-BP-001` revision `1`, bentuk `COMPOSITE` (`AST-DEC-001`) |
| Map revision | `1` |
| Status | `approved` — Rizki, 2026-11-12 (`AST-DEC-051`) |
| Masukan | decision revision `4` (`AST-DEC-001..050`); capability map `1.0`; `AST-RCG-001` |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |

Berkas ini hanya memuat empat hal yang tidak boleh punya salinan di dalam sub-modul: registry
sub-modul, kepemilikan data, peta menu beserta urutan migration, dan pemetaan kemampuan.
Arsitektur, kontrak, dan PRD tinggal di masing-masing sub-modul.

---

## 1. Registry sub-modul

| Slug | Folder backend | Rumpun kemampuan | Peran pemilik | Gelombang rilis | Status |
|---|---|---|---|---|---|
| [`register`](register/blueprint-manifest.md) | `Areas/Corporate/AssetManagement/{MasterData,Register}` | Master klasifikasi, pencatatan, nomor, label, dokumen, penempatan, kondisi, pemanfaatan, impor data awal, dashboard, laporan register, linimasa aset | Petugas dan Kepala Bagian Aset | `MVP-0`, `MVP-1` | `approved` |
| [`pergerakan`](pergerakan/blueprint-manifest.md) | `Areas/Corporate/AssetManagement/Movement` | Mutasi, serah terima antar unit, peminjaman, pengembalian | Kepala Unit, Bagian Aset | `MVP-2` | `approved` |
| [`inventarisasi`](inventarisasi/blueprint-manifest.md) | `Areas/Corporate/AssetManagement/PhysicalInventory` | Jadwal opname, pemindaian, pencocokan, selisih, verifikasi, berita acara, tindak lanjut | Tim Inventarisasi, Kepala Bagian Aset | `MVP-3` | `approved` |
| [`nilai`](nilai/blueprint-manifest.md) | `Areas/Corporate/AssetManagement/Valuation` | Parameter nilai, buku nilai, penyusutan, perubahan nilai, kiriman ke Akuntansi, rekonsiliasi | Kepala Akuntansi, Petugas Aset | Tabel dasar `MVP-0`; fitur `MVP-4` | `approved` |
| [`penghapusan`](penghapusan/blueprint-manifest.md) | `Areas/Corporate/AssetManagement/Disposal` | Usulan, kelayakan, persetujuan, pemindahtanganan, berita acara, Pemulihan Aset | Tim Pemeriksa, Kepala Bagian Aset, Direktur | `MVP-5` | `approved` |

Status modul diturunkan: kelima sub-modul `approved` → modul **`approved`**.

**Prasyarat bersama sebelum model pertama (`QBE-MOD-002`, `QBE-MOD-003`).** Registry
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` belum memuat pemilik `AssetManagement`
maupun prefix `Ast`. Baris yang diusulkan, dikerjakan sebagai task backend pertama:

| Area | Owner | Category | Prefix | Lifecycle |
|---|---|---|---|---|
| Corporate | AssetManagement / Asset Management | BUSINESS DOMAIN / MODULE | `Ast` = *Asset* | `ACTIVE` |
| Corporate | AssetManagement / Register / Register Aset | BUSINESS DOMAIN / MODULE | `Ast` | `ACTIVE` |
| Corporate | AssetManagement / Movement / Pergerakan Aset | BUSINESS DOMAIN / MODULE | `Ast` | `ACTIVE` |
| Corporate | AssetManagement / PhysicalInventory / Inventarisasi Fisik | BUSINESS DOMAIN / MODULE | `Ast` | `ACTIVE` |
| Corporate | AssetManagement / Valuation / Nilai Aset | BUSINESS DOMAIN / MODULE | `Ast` | `ACTIVE` |
| Corporate | AssetManagement / Disposal / Penghapusan Aset | BUSINESS DOMAIN / MODULE | `Ast` | `ACTIVE` |

`AssetManagement/MasterData` memakai prefix `Mst`, mengikuti preseden `FinanceManagement/MasterData`
(`MstBankAccount`) yang tercakup baris `Mst`. Pemeriksaan ambiguitas pencocokan baris (preseden
`PatientRefund` 2026-10-22) dilakukan task registry itu.

---

## 2. Tabel kepemilikan data seluruh modul

### 2.1 Data milik modul lain — dipakai, tidak dibuat ulang

| Kelompok data | Pemilik | Dipakai sub-modul | Dibuat ulang | Cara pakai |
|---|---|---|---|---|
| Unit organisasi | HR MasterData (`MstOrganizationUnit`) | Kelimanya | **Tidak** | Dibaca (`AST-DEC-034`) |
| Ruangan klinis | HealthServices MasterData (`MstRoom`) | `register` | **Tidak** | Ditautkan opsional dari lokasi aset |
| Pegawai | HR (`MstEmployee`, `MstWorkforceProfile`) | Kelimanya | **Tidak** | Penanggung jawab, anggota tim |
| Penugasan pegawai ke unit | HR WorkforceCore (`WfpOrganizationAssignment`) | Kelimanya | **Tidak** | "Unit saya" dan Kepala Unit |
| Badan hukum, site | HR MasterData (`MstLegalEntity`, `MstHospitalSite`) | Kelimanya | **Tidak** | Disimpan di aset, tanpa pilihan di layar (`AST-DEC-035`) |
| Pusat biaya | HR MasterData (`MstCostCenter`) | `nilai` | **Tidak** | Pembebanan penyusutan |
| PO dan GR | Finance Purchasing (`FinPurchaseOrder`, `FinGoodsReceipt`) | `register` | **Tidak** | Rujukan bukti perolehan |
| Nomor bisnis | Platform (`NumNumberSeries`) | Kelimanya | **Tidak** | Lewat `NumberSeriesAllocator` |
| Kejadian keuangan, jurnal, saldo subledger | Akuntansi (`AccAccountingEvent`, `AccSubledgerBalance`) | `nilai` (pengirim tunggal) | **Tidak** | Dikirim lewat API Akuntansi |
| Aset yang dikembalikan pegawai keluar | HR Lifecycle (`TrxAssetReturn`) | Tidak dipakai MVP | **Tidak** | `AST-OQ-012`, `LATER SLICE` |

### 2.2 Data milik modul ini — satu pemilik per tabel

| Kelompok data | Tabel | Sub-modul pemilik | Ditulis sub-modul lain? |
|---|---|---|---|
| Master klasifikasi | `MstAssetCategory`, `MstAssetClassification`, `MstAssetGroup`, `MstAssetOwnershipType`, `MstAssetDocumentType`, `MstAssetCategoryDocumentRequirement`, `MstAssetLocation`, `MstAssetSetting` | `register` | Tidak |
| Aset (akar data) | `AstAsset` | `register` | Ya, **hanya lewat service milik `register`**: `AssetPlacementService` (pergerakan), `AssetConditionService` (inventarisasi), `AssetRecordService` (penghapusan) |
| Riwayat penempatan | `AstAssetPlacement` | `register` | Lewat `AssetPlacementService` |
| Penyerahan dan berita acara penyerahan | `AstAssetHandover`, `AstAssetHandoverItem` | `register` | Tidak |
| Laporan kondisi | `AstConditionReport` | `register` | Dibuat lewat `AssetConditionService` oleh pergerakan dan inventarisasi |
| Linimasa aset | `AstAssetHistory` | `register` | Ya, **hanya lewat `AssetHistoryService`** (`AST-DES-004`) |
| Dokumen kepemilikan | `AstAssetDocument` | `register` | Tidak |
| Lampiran berkas | `AstAttachment` | `register` | Lewat `AssetAttachmentService` (foto opname, berita acara) |
| Pemanfaatan | `AstUtilization`, `AstUtilizationItem` | `register` | Tidak |
| Impor data awal | `AstImportBatch`, `AstImportBatchRow` | `register` | Tidak |
| Mutasi | `AstTransfer`, `AstTransferItem` | `pergerakan` | Tidak |
| Peminjaman | `AstLoan`, `AstLoanItem`, `AstLoanExtension` | `pergerakan` | Tidak |
| Opname | `AstStockTakeSession`, `AstStockTakeTeamMember`, `AstStockTakeExpectedItem`, `AstStockTakeScan`, `AstStockTakeResult`, `AstStockTakeFollowUp` | `inventarisasi` | Tidak |
| Parameter nilai | `AstValuationParameter`, `AstDepreciationPolicy`, `AstSubledgerAccountSetup`, `AstUnitCostCenter` | `nilai` | Tidak |
| Buku nilai | `AstAssetBook`, `AstAssetValueEntry` | `nilai` | Dibuat lewat `AssetBookService` oleh register (persetujuan, impor) dan penghapusan (final, pemulihan) |
| Proses penyusutan | `AstDepreciationRun`, `AstDepreciationRunLine` | `nilai` | Tidak |
| Perubahan nilai | `AstValueChangeRequest` | `nilai` | Tidak |
| Kotak keluar ke Akuntansi | `AstAccountingOutbox` | `nilai` | Diisi lewat `AssetAccountingEventService` oleh register (hibah) dan penghapusan |
| Penghapusan | `AstDisposalProposal`, `AstDisposalItem`, `AstDisposalReviewer` | `penghapusan` | Tidak |
| Pemulihan | `AstAssetRestoration` | `penghapusan` | Tidak |

Jumlah tabel baru: **45** (`register` 20, `pergerakan` 5, `inventarisasi` 6, `nilai` 10, `penghapusan` 4).
Tidak ada tabel milik modul lain yang diubah oleh migration Aset. Satu-satunya perubahan di luar
modul adalah perluasan Akuntansi `DEP-AST-01` (`AST-DEC-047`), yang dikerjakan sebagai task modul
Akuntansi, bukan task Aset.

---

## 3. Peta butir menu dan urutan migration

### 3.1 Peta butir menu

Satu grup tingkat 0 pada `src/utils/menu-sidebar/menu-items.jsx`. Anak tingkat 0 memakai `subMenu`,
anak grup memakai `subItems`, daun memakai `pathname` (`getChildrenFromItem`). Letak grup di sidebar
masih `AST-OQ-015`; nama dan urutan butir mengikuti cakupan pemilik (`AST-FE-002`). Prefix route:
`/corporate/asset-management`.

```text
Manajemen Aset                                   (tingkat 0)
├── Dashboard Aset                               -> /corporate/asset-management
├── Master Klasifikasi Aset                      (grup)
│   ├── Kategori Aset                            -> /master-data/categories
│   ├── Kelompok Aset                            -> /master-data/groups
│   ├── Kode Inventaris                          -> /master-data/inventory-codes
│   ├── Jenis Kepemilikan                        -> /master-data/ownership-types
│   ├── Lokasi                                   -> /master-data/locations
│   ├── Klasifikasi Aset Medis & Non-Medis       -> /master-data/classifications
│   ├── Jenis Dokumen                            -> /master-data/document-types
│   └── Pengaturan Aset                          -> /master-data/settings
├── Registrasi & Inventaris                      (grup)
│   ├── Pencatatan Aset Baru                     -> /register/drafts
│   ├── Data Perolehan                           -> /register/acquisitions
│   ├── Nomor Inventaris                         -> /register/approvals
│   ├── Label Aset                               -> /register/labels
│   ├── Dokumen Kepemilikan                      -> /register/documents
│   ├── Daftar Inventaris                        -> /register/assets
│   └── Impor Data Awal                          -> /register/imports
├── Penempatan & Tanggung Jawab Aset             (grup)
│   ├── Penempatan Per Unit/Ruangan              -> /placement/units
│   ├── Penanggung Jawab Aset                    -> /placement/custodians
│   ├── Berita Acara Penyerahan                  -> /placement/handovers
│   └── Riwayat Lokasi & Penggunaan              -> /placement/history
├── Mutasi & Peminjaman Aset                     (grup)
│   ├── Pengajuan Pemindahan                     -> /movement/transfers
│   ├── Persetujuan Mutasi                       -> /movement/transfer-approvals
│   ├── Serah Terima Antar Unit                  -> /movement/transfer-receipts
│   ├── Peminjaman Sementara                     -> /movement/loans
│   └── Pengembalian Aset                        -> /movement/loan-returns
├── Kondisi & Status Aset                        (grup)
│   ├── Pemeriksaan Kondisi                      -> /condition/reports
│   ├── Aset Aktif                               -> /condition/active
│   ├── Rusak Ringan/Berat                       -> /condition/damaged
│   ├── Tidak Digunakan                          -> /condition/unused
│   ├── Hilang                                   -> /condition/lost
│   └── Riwayat Perubahan Status                 -> /condition/history
├── Inventaris & Pemeriksaan Fisik               (grup)
│   ├── Jadwal Inventaris                        -> /stock-take/schedules
│   ├── Pemeriksaan Fisik                        -> /stock-take/scan
│   ├── Pencocokan Data                          -> /stock-take/matching
│   ├── Selisih Inventaris                       -> /stock-take/discrepancies
│   ├── Verifikasi Hasil                         -> /stock-take/verification
│   └── Berita Acara                             -> /stock-take/reports
├── Pemanfaatan & Pengamanan Aset                (grup)
│   ├── Pengajuan Pemanfaatan                    -> /utilization/requests
│   ├── Kelengkapan Dokumen Kepemilikan          -> /security/documents
│   ├── Pengamanan Administrasi                  -> /security/administration
│   ├── Pemantauan Aset Menganggur               -> /security/idle
│   └── Pemanfaatan Sesuai Kewenangan            -> /utilization/approvals
├── Nilai Aset & Rekonsiliasi                    (grup)
│   ├── Harga Perolehan                          -> /valuation/acquisition-costs
│   ├── Nilai Buku                               -> /valuation/book-values
│   ├── Riwayat Perubahan Nilai                  -> /valuation/value-changes
│   ├── Data Penyusutan                          -> /valuation/depreciation
│   ├── Rekonsiliasi Aset dengan Akuntansi       -> /valuation/reconciliation
│   └── Parameter Nilai                          -> /valuation/parameters
├── Penghapusan & Pelepasan Aset                 (grup)
│   ├── Usulan Penghapusan                       -> /disposal/proposals
│   ├── Pemeriksaan Kelayakan Penghapusan        -> /disposal/feasibility
│   ├── Persetujuan                              -> /disposal/approvals
│   ├── Pemindahtanganan                         -> /disposal/transfers
│   ├── Berita Acara                             -> /disposal/reports
│   └── Riwayat Aset Dihapus                     -> /disposal/history
└── Laporan Manajemen Aset                       (grup)
    ├── Daftar Inventaris Per-unit               -> /reports/inventory-by-unit
    ├── Laporan Mutasi                           -> /reports/transfers
    ├── Kondisi Aset                             -> /reports/condition
    ├── Inventaris                               -> /reports/stock-take
    ├── Penghapusan                              -> /reports/disposal
    ├── Nilai Aset                               -> /reports/valuation
    └── History & Audit                          -> /reports/history
```

Semua `pathname` di atas relatif terhadap `/corporate/asset-management`. Butir tambahan di luar
daftar 11 menu pemilik — **Jenis Dokumen**, **Pengaturan Aset**, **Impor Data Awal**, dan **Parameter
Nilai** — ada karena tanpa layar itu keputusan `AST-RCG-G14`, `AST-DEC-023`, dan `AST-DEC-027` tidak
punya tempat di aplikasi. Butir 8.2 diberi nama "Kelengkapan Dokumen Kepemilikan" agar tidak
tertukar dengan butir 3.5: datanya sama (`AST-ASM-002`), layarnya menampilkan sisi pengamanan.

| Butir menu | Layar | Sub-modul | Butir hak akses |
|---|---|---|---|
| Dashboard Aset | `SCR-AST-REG-01` | `register` (kartu 1.4 dari `pergerakan`, 1.5 dari `inventarisasi`) | `AssetDashboard : Read` |
| Kategori Aset s.d. Pengaturan Aset | `SCR-AST-REG-02..09` | `register` | `AssetCategory`, `AssetGroup`, `AssetOwnershipType`, `AssetLocation`, `AssetClassification`, `AssetDocumentType`, `AssetSetting` `: Read` |
| Pencatatan Aset Baru s.d. Impor Data Awal | `SCR-AST-REG-10..16` | `register` | `AssetRegister : Read` / `AssetDocument : Read` / `AssetImport : Read` |
| Penempatan & Tanggung Jawab (4 butir) | `SCR-AST-REG-18..21` | `register` | `AssetPlacement : Read`, `AssetRegister : Read` |
| Mutasi & Peminjaman (5 butir) | `SCR-AST-MOV-01..05` | `pergerakan` | `AssetTransfer : Read`, `AssetLoan : Read` |
| Kondisi & Status (6 butir) | `SCR-AST-REG-22..27` | `register` | `AssetCondition : Read`, `AssetRegister : Read` |
| Inventaris & Pemeriksaan Fisik (6 butir) | `SCR-AST-INV-01..06` | `inventarisasi` | `AssetStockTake : Read` |
| Pemanfaatan & Pengamanan (5 butir) | `SCR-AST-REG-28..32` | `register` | `AssetUtilization : Read`, `AssetDocument : Read`, `AssetRegister : Read` |
| Nilai Aset & Rekonsiliasi (6 butir) | `SCR-AST-NLI-01..06` | `nilai` | `AssetBook : Read`, `AssetValueChange : Read`, `AssetDepreciationRun : Read`, `AssetReconciliation : Read`, `AssetValuationParameter : Read` |
| Penghapusan & Pelepasan (6 butir) | `SCR-AST-HPS-01..06` | `penghapusan` | `AssetDisposal : Read` |
| Laporan (7 butir) | `SCR-AST-REG-33..35`, `SCR-AST-MOV-08`, `SCR-AST-INV-07`, `SCR-AST-HPS-07`, `SCR-AST-NLI-07` | sesuai laporan | `AssetReport : Read` ditambah `: Read` resource sumbernya |

Layar anak tanpa butir menu (jalan masuknya disebut pada `03-frontend-architecture.md` tiap
sub-modul): detail aset `SCR-AST-REG-17`, detail mutasi `SCR-AST-MOV-06`, detail pinjaman
`SCR-AST-MOV-07`, detail sesi opname `SCR-AST-INV-08`, detail usulan penghapusan `SCR-AST-HPS-08`,
detail proses penyusutan `SCR-AST-NLI-08`.

### 3.2 Urutan migration lintas sub-modul

Semua migration bersifat menambah tabel baru (aditif), sehingga dapat diterapkan tanpa mematikan
layanan. Tidak ada pengisian data lama. Langkah mundur setiap migration adalah `Down()` yang menghapus
tabel yang ia buat, aman selama tabel itu belum berisi data produksi.

| Urutan | Migration (usulan nama) | Isi | Pemilik | Syarat |
|---:|---|---|---|---|
| 0 | — (bukan migration) | Baris registry bagian 1 | Task registry | — |
| 1 | `AddAssetManagementFoundation` | 8 master `register`; 12 tabel transaksi `register`; dari `nilai`: `AstValuationParameter`, `AstDepreciationPolicy`, `AstAssetBook`, `AstAssetValueEntry`, `AstAccountingOutbox` | `register` (+ `nilai` per `AST-DES-003`) | Urutan 0 |
| 2 | `AddAssetMovement` | 5 tabel `pergerakan` | `pergerakan` | Urutan 1 |
| 3 | `AddAssetPhysicalInventory` | 6 tabel `inventarisasi` | `inventarisasi` | Urutan 1 |
| 4 | `AddAssetValuationProcessing` | `AstDepreciationRun`, `AstDepreciationRunLine`, `AstValueChangeRequest`, `AstSubledgerAccountSetup`, `AstUnitCostCenter` | `nilai` | Urutan 1 |
| 5 | `AddAssetDisposal` | 4 tabel `penghapusan` | `penghapusan` | Urutan 1 dan 4 |
| L-1 | Migration modul Akuntansi (`DEP-AST-01`) | Kolom pusat biaya pada kejadian keuangan | Akuntansi | Sebelum pengiriman kejadian penyusutan dinyalakan; tidak menahan urutan 1–5 |

Urutan 2 dan 3 saling bebas; urutan 5 bergantung pada 4 karena penghapusan final mensyaratkan
penyusutan bulan sebelumnya sudah diproses (`AST-DES-013`).

**Mengapa sebagian tabel `nilai` masuk urutan 1** (`AST-DES-003`): pencatatan aset (`register`,
gelombang pertama) harus langsung menetapkan dikapitalisasi atau tidak dan membuat buku nilai,
sedangkan impor data awal membawa akumulasi penyusutan. Fitur yang terlihat pengguna (proses
penyusutan, perubahan nilai, rekonsiliasi) tetap gelombang `MVP-4` sesuai `AST-DEC-024`.

---

## 4. Pemetaan kemampuan ke sub-modul

Setiap kemampuan di `00-interview-decisions.md` bagian 1, ditambah lima kemampuan yang lahir dari
keputusan (`CAP-EXT-*`). Kemampuan tanpa pemilik: **nol**.

| Kemampuan | ID | Sub-modul pemilik | Catatan |
|---|---|---|---|
| Ringkasan aset, aset per unit, kondisi, aset rusak (dashboard) | `CAP-01.1`, `01.2`, `01.3`, `01.6` | `register` | — |
| Mutasi berjalan (dashboard) | `CAP-01.4` | `pergerakan` | Kartu di layar dashboard `register` |
| Inventaris tertunda (dashboard) | `CAP-01.5` | `inventarisasi` | `AST-DEC-031` |
| Master kategori, kelompok, kode inventaris, jenis kepemilikan, lokasi, klasifikasi | `CAP-02.1`–`02.6` | `register` | Kode inventaris = kode kelompok |
| Pencatatan, perolehan, nomor, label, dokumen, daftar | `CAP-03.1`–`03.6` | `register` | — |
| Penempatan, penanggung jawab, BA penyerahan, riwayat lokasi | `CAP-04.1`–`04.4` | `register` | — |
| Pengajuan, persetujuan, serah terima mutasi | `CAP-05.1`–`05.3` | `pergerakan` | — |
| Peminjaman, pengembalian | `CAP-05.4`, `05.5` | `pergerakan` | — |
| Pemeriksaan kondisi, status, riwayat status | `CAP-06.1`–`06.6` | `register` | — |
| Jadwal, pemeriksaan fisik, pencocokan, selisih, verifikasi, BA opname | `CAP-07.1`–`07.6` | `inventarisasi` | — |
| Pemanfaatan, kelengkapan dokumen, pengamanan, aset menganggur, kewenangan | `CAP-08.1`–`08.5` | `register` | — |
| Harga perolehan, nilai buku, riwayat nilai, penyusutan, rekonsiliasi | `CAP-09.1`–`09.5` | `nilai` | `09.3` bagian revaluasi `DEFERRED` (`AST-DEC-032`) |
| Usulan, kelayakan, persetujuan, pemindahtanganan, BA, riwayat dihapus | `CAP-10.1`–`10.6` | `penghapusan` | — |
| Daftar inventaris per unit, kondisi, History & Audit | `CAP-11.1`, `11.3`, `11.7` | `register` | — |
| Laporan mutasi | `CAP-11.2` | `pergerakan` | — |
| Laporan inventaris | `CAP-11.4` | `inventarisasi` | — |
| Laporan penghapusan | `CAP-11.5` | `penghapusan` | — |
| Laporan nilai aset | `CAP-11.6` | `nilai` | — |
| Impor data awal | `CAP-EXT-01` | `register` | `AST-DEC-023`, `037`, `050` |
| Parameter nilai | `CAP-EXT-02` | `nilai` | `AST-DEC-027` |
| Pemulihan Aset | `CAP-EXT-03` | `penghapusan` | `AST-DEC-028` |
| Perolehan hibah ke Akuntansi | `CAP-EXT-04` | `nilai` (dipicu `register`) | `AST-DEC-040` |
| Jenis dokumen dan pengaturan aset | `CAP-EXT-05` | `register` | `AST-RCG-G13`, `G14` |
| Revaluasi | `CAP-09.3` (sebagian) | `nilai` | **`DEFERRED`** — butuh kebijakan per kelompok, akun surplus, dan laporan penilai; selama MVP tidak ada pengganti karena revaluasi belum pernah dilakukan lewat sistem |
