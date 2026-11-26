# Pengadaan Barang & Jasa — Peta Modul

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 |
| Bentuk | `COMPOSITE` (`PBJ-DEC-004`, `USER_CONFIRMED`) |
| Status dokumen | `approved` untuk bagian `perencanaan-permintaan` (`PBJ-DEC-034`) dan `manajemen-vendor` (`PBJ-DEC-062`); bagian `pemilihan-penyedia` `draft` (26 November 2026) |
| Masukan | `00-interview-decisions.md` (`PBJ-DEC-001`..`030`), `01-existing-capability-map.md` revisi 1, `02-requirement-completeness-assessment.md` revisi 2 |
| SHA | BE `100c011d`, FE `763d665b` |
| Tanggal | 12 November 2026 |

Dokumen ini hanya memuat empat hal yang tidak boleh punya lebih dari satu salinan di dalam modul:
registry sub-modul, kepemilikan data, peta menu beserta urutan migration, dan pemetaan kemampuan.
Arsitektur, kontrak, dan PRD tinggal di folder masing-masing sub-modul.

---

## 1. Registry sub-modul

| Slug | Prefix ID dokumen | Rumpun kemampuan | Pemilik | Status desain | Keterangan |
| --- | --- | --- | --- | --- | --- |
| `perencanaan-permintaan` | `PBJ-PP` | K-01..K-04, ditambah fondasi modul (katalog, lampiran, riwayat dokumen, perluasan approval) | Pengguna sesi (Product Owner Pengadaan) | `approved` (2026-11-12, `PBJ-DEC-034`) | Dirancang pertama (`PBJ-DEC-018`) |
| `pemilihan-penyedia` | `PBJ-PS` | K-05, K-07 | Pengguna sesi | `draft` (2026-11-26) | Gerbang kelengkapan revisi 6 `READY_FOR_DOMAIN_DESIGN`; `PBJ-OQ-005` menjadi nilai konfigurasi (`PBJ-DEC-069`). Registry submodule `Sourcing` **belum ada** (prasyarat implementasi) |
| `manajemen-vendor` | `PBJ-VD` | K-06, K-11 (manual; evaluasi per kontrak `LATER SLICE`) | Pengguna sesi | `approved` (2026-11-17, `PBJ-DEC-062`) | Registry submodule `Vendor` `ACTIVE` (`PBJ-DEC-063`) |
| `kontrak-pelaksanaan` | `PBJ-KP` | K-08..K-10 | Pengguna sesi | `NOT_STARTED` | Bergantung pada `pemilihan-penyedia` dan PO BLM (`PBJ-DEC-011`) |

**Status modul (diturunkan):** `partial` — dua sub-modul `approved`, satu `draft` (`pemilihan-penyedia`), satu belum dimulai.

**Prefix entity:** `Prc` (*Procurement*) dan Area `Areas/Corporate/ProcurementManagement/` **terdaftar
`ACTIVE`** di registry pada 12 November 2026 (`PBJ-DEC-035`): baris `ProcurementManagement / Procurement`
beserta submodule `Planning`, `Request`, `Common`. Folder `MasterData` memakai baris `Mst`.

## 2. Kepemilikan data seluruh modul

| Kelompok data | Pemilik | Dipakai sub-modul | Dibuat ulang di Pengadaan |
| --- | --- | --- | --- |
| Pengguna, peran, izin | Platform Authorization | Semua | Tidak |
| Departemen/unit dan penempatan pengguna (`MstDepartment`, `AspNetUserOrganization`) | HR Organisasi / Workforce | Semua | Tidak (`PBJ-DEC-020`) |
| Mesin approval: definisi, langkah, matriks, instance, kotak masuk (`MstWorkflowDefinition`, `MstWorkflowStep`, `MstApprovalMatrix`, `TrxWorkflowInstance`) | WorkflowManagement (`Wfl`) | Semua | Tidak; **diperluas** (`PBJ-DEC-019`) |
| Deret nomor dokumen (`NumNumberSeries`) | Platform / NumberSeries | Semua | Tidak |
| Satuan (`MstMeasurement`) dan obat (`MstDrug`) | Health Services Master Data | `perencanaan-permintaan` | Tidak |
| Identitas vendor dan status blokir (`MstSupplier`, termasuk `IsBlacklisted`) | Administrator Master Data | `manajemen-vendor`, `pemilihan-penyedia` | Tidak (`PBJ-DEC-010`, `021`). `IsBlacklisted` hanya ditulis lewat usulan Pengadaan yang disetujui (`PBJ-DEC-060`) |
| Rekening vendor | Administrator Master Data / Finance | `manajemen-vendor` (tampil tersamarkan) | Tidak (`PBJ-DEC-015`) |
| PO, GR, faktur, pembayaran | BLM / Finance Management | `kontrak-pelaksanaan`, laporan | Tidak (`PBJ-DEC-001`) |
| Katalog barang/jasa, kategori, jenis pengadaan, sumber dana | `perencanaan-permintaan` | Semua | Ya, baru (`PBJ-DEC-022`, `024`) |
| Lampiran berkas Pengadaan | `perencanaan-permintaan` | Semua | Ya, baru (`PBJ-DEC-023`) |
| Riwayat dokumen Pengadaan (aksi, perubahan status, alasan) | `perencanaan-permintaan` | Semua | Ya, baru |
| Rencana kebutuhan tahunan dan revisinya | `perencanaan-permintaan` | `pemilihan-penyedia` (baca) | Ya, baru |
| Rencana paket | `perencanaan-permintaan` | `pemilihan-penyedia` (rujukan saja, `PBJ-DEC-082`) | Ya, baru (`PBJ-DEC-028`) |
| Pagu anggaran dan komitmen | `perencanaan-permintaan` | `pemilihan-penyedia` dan `kontrak-pelaksanaan` menulis komitmen **lewat service milik `perencanaan-permintaan`** | Ya, baru (`PBJ-DEC-009`) |
| Permintaan pengadaan (PR) dan hasil cek stok | `perencanaan-permintaan` | `pemilihan-penyedia` (baca baris siap paket; menandai dan melepas baris per paket lewat `MarkLinesPackagedAsync`/`ReleaseLinesAsync` milik pemilik, `PBJ-DEC-071`) | Ya, baru; `PrcPurchaseRequestLine` diperbarui (`PackageId`) |
| Paket pengadaan, spesifikasi, HPS, checklist, evaluator, RFQ, penawaran, evaluasi teknis, negosiasi, penetapan (`PrcPackage*`, `PrcRfq*`, `PrcQuotation*`, `PrcTechnical*`, `PrcNegotiation*`, `PrcPackageAward`) | `pemilihan-penyedia` | `kontrak-pelaksanaan` (baca paket `ORDERED`) | Ya, baru (`draft` 26 November 2026) |
| Konfigurasi pemilihan: metode pengadaan, jenis dokumen paket, dokumen wajib paket (`MstProcurementMethod`, `MstProcurementPackageDocument*`) | `pemilihan-penyedia` | — | Ya, baru (`draft`); **bukan** kewajiban dokumen legalitas vendor milik `manajemen-vendor` |
| Data pendamping vendor: profil, kategori, dokumen legalitas, status Dibatasi, usulan blokir, penilaian (`PrcVendor*`) dan konfigurasinya (jenis dokumen, dokumen wajib, kriteria) | `manajemen-vendor` | `pemilihan-penyedia` (baca kelayakan lewat service pemilik) | Ya, baru (`approved` 17 November 2026, `PBJ-DEC-062`) |
| SPK, kontrak, termin, addendum, monitoring, pemeriksaan, BAST, NCR | `kontrak-pelaksanaan` | Laporan | Ya, baru (belum dirancang) |

Aturan lintas sub-modul: sub-modul lain **tidak boleh** menulis langsung ke tabel milik
`perencanaan-permintaan`. Mereka memanggil service pemiliknya, yang menjaga invariant. Contoh: komitmen
pagu ditulis lewat `ProcurementBudgetCommitmentService`.

## 3. Peta butir menu dan urutan migration

### 3.1 Butir menu

Nama 11 menu dan 61 submenu mengikuti SOURCE (`PBJ-FE-001`). Route di bawah adalah **usulan**; rupa
navigasi tetap `DEV_DISCRETION`. Butir menu didaftarkan di `FE/src/utils/menu-sidebar/menu-items.jsx`.

```text
Pengadaan Barang & Jasa                         /corporate/procurement
├── 1. Dashboard Pengadaan                      (tingkat modul; POST-MVP)
├── 2. Rencana Pengadaan                        perencanaan-permintaan
├── 3. Permintaan Pengadaan                     perencanaan-permintaan
├── 4. Persiapan Pengadaan                      pemilihan-penyedia
├── 5. Data Penyedia/ Vendor                    manajemen-vendor
├── 6. Penawaran & Pemilihan Penyedia           pemilihan-penyedia
├── 7. Pesanan & Kontrak                        kontrak-pelaksanaan
├── 8. Monitoring Pelaksanaan Pengadaan         kontrak-pelaksanaan
├── 9. Serah Terima Hasil                       kontrak-pelaksanaan
├── 10. Evaluasi Penyedia                       manajemen-vendor
├── 11. Persetujuan & Laporan Pengadaan         tingkat modul
└── Pengaturan Pengadaan (Katalog, Kategori, Sumber Dana)   perencanaan-permintaan — PBJ-DEC-032
```

| Menu (SOURCE) | Submenu (SOURCE) | Sub-modul pemilik | Route usulan | Gelombang |
| --- | --- | --- | --- | --- |
| 1 | 1.1 Ringkasan, 1.3 Pengadaan Berjalan, 1.4 Realisasi Anggaran, 1.5 Pengadaan Tertunda | `perencanaan-permintaan` (`DEFERRED`) | `/corporate/procurement/dashboard/*` | `POST-MVP` |
| 1 | 1.2 Pengajuan Menunggu Persetujuan | `perencanaan-permintaan` | `/corporate/procurement/dashboard/pending-approvals` | `MVP-1` |
| 2 | 2.1 Rencana Kebutuhan Tahunan | `perencanaan-permintaan` | `/corporate/procurement/planning/need-plans` | `MVP-2` |
| 2 | 2.2 Konsolidasi Kebutuhan Unit | `perencanaan-permintaan` | `/corporate/procurement/planning/consolidation` | `MVP-4` |
| 2 | 2.3 Rencana Anggaran | `perencanaan-permintaan` | `/corporate/procurement/planning/budgets` | `MVP-3` |
| 2 | 2.4 Rencana Paket Pengadaan | `perencanaan-permintaan` | `/corporate/procurement/planning/package-plans` | `MVP-4` |
| 2 | 2.5 Revisi Perencanaan | `perencanaan-permintaan` | `/corporate/procurement/planning/revisions` | `MVP-2` |
| 3 | 3.1 Pengajuan Pembelian Barang & Jasa | `perencanaan-permintaan` | `/corporate/procurement/requests` | `MVP-1` |
| 3 | 3.2 Verifikasi Kebutuhan | `perencanaan-permintaan` | `/corporate/procurement/requests/verification` | `MVP-1` |
| 3 | 3.3 Pemeriksaan Stock | `perencanaan-permintaan` | `/corporate/procurement/requests/stock-checks` | `MVP-1` |
| 3 | 3.4 Permintaan Mendesak | `perencanaan-permintaan` | `/corporate/procurement/requests/urgent` | `MVP-1` |
| 3 | 3.5 Riwayat Pengajuan | `perencanaan-permintaan` | `/corporate/procurement/requests/history` | `MVP-1` |
| 4, 6 | 11 submenu (nama menunggu `PBJ-OQ-038`) | `pemilihan-penyedia` | `/corporate/procurement/sourcing/packages`, `/rfqs`, `/technical-evaluations`, `/awards` (usulan, `pemilihan-penyedia/03-frontend-architecture.md` bagian 3) | `MVP-PS1`..`MVP-PS3` |
| 5 | 5.1 Daftar vendor, 5.2 Kategori, 5.3 Dokumen Legalitas, 5.4 Rekening, 5.5 Status, 5.6 Riwayat | `manajemen-vendor` | `/corporate/procurement/vendors`, `/categories`, `/documents`, `/bank-accounts`, `/status`, `/history` | `MVP-V0`..`MVP-V2` |
| 10 | Satu submenu penilaian berkala (nama menunggu `PBJ-OQ-033`) | `manajemen-vendor` | `/corporate/procurement/vendors/evaluations` | `MVP-V3` |
| 10 | Empat submenu lain (evaluasi kontrak, keterlambatan, NCR, rekomendasi — nama `PBJ-OQ-033`) | `manajemen-vendor` (`LATER SLICE` `S-V7`) | belum dirancang | `POST-MVP` |
| 7, 8, 9 | 18 submenu | `kontrak-pelaksanaan` | belum dirancang | — |
| 11 | 11.1 Antrean persetujuan | `perencanaan-permintaan` (memakai kotak masuk `Wfl`) | `/corporate/procurement/approvals/inbox` | `MVP-1` |
| 11 | 11.2 Validasi Pengajuan | `perencanaan-permintaan` (= daftar verifikasi 3.2, disaring untuk validator) | `/corporate/procurement/approvals/validation` | `MVP-1` |
| 11 | 11.3 Persetujuan Paket & Kontrak | `pemilihan-penyedia` (bagian paket) / `kontrak-pelaksanaan` (bagian kontrak, belum dirancang) | `/corporate/procurement/approvals/packages` | `MVP-PS1` |
| — | Pengaturan Pengadaan: Metode Pengadaan, Jenis Dokumen Paket, Dokumen Wajib Paket | `pemilihan-penyedia` | `/corporate/procurement/settings/procurement-methods`, `/package-document-types`, `/package-document-requirements` | `MVP-PS0` |
| 11 | 11.4 Laporan Pembelian, 11.5 Laporan Realisasi, 11.6 Laporan Audit | `perencanaan-permintaan` (`DEFERRED`) | `/corporate/procurement/reports/*` | `POST-MVP` |
| — | Pengaturan Pengadaan (katalog, kategori, jenis, sumber dana) | `perencanaan-permintaan` | `/corporate/procurement/settings/*` | `MVP-0` |
| — | Pengaturan Pengadaan: Jenis Dokumen Legalitas, Dokumen Wajib per Kategori, Kriteria Penilaian | `manajemen-vendor` | `/corporate/procurement/settings/vendor-document-types`, `/category-document-requirements`, `/evaluation-criteria` | `MVP-V0` |

**`PBJ-DEC-032`:** grup tambahan "Pengaturan Pengadaan" di luar 11 menu disetujui untuk katalog dan
referensi; 61 submenu SOURCE tetap utuh.

### 3.2 Urutan migration lintas sub-modul

| Urutan | Migration (nama usulan) | Sub-modul | Isi | Syarat |
| ---: | --- | --- | --- | --- |
| 1 | `AddWorkflowStepActivationCondition` | `perencanaan-permintaan` (menyentuh `Wfl`) | Satu kolom baru di `MstWorkflowStep` | Persetujuan pemilik `Wfl` (`PBJ-DEC-026`) |
| 2 | `AddProcurementFoundation` | `perencanaan-permintaan` | Master Pengadaan, katalog, lampiran, riwayat dokumen | Registry `PBJ-OQ-002` |
| 3 | `AddProcurementPurchaseRequest` | `perencanaan-permintaan` | PR dan barisnya | Urutan 2 |
| 4 | `AddProcurementNeedPlan` | `perencanaan-permintaan` | Rencana kebutuhan dan barisnya | Urutan 2 |
| 5 | `AddProcurementBudget` | `perencanaan-permintaan` | Pagu dan komitmen | Urutan 2 |
| 6 | `AddProcurementPackagePlan` | `perencanaan-permintaan` | Rencana paket dan barisnya | Urutan 4 |
| 7 | `AddProcurementVendorManagement` | `manajemen-vendor` | 3 master vendor + 6 tabel `PrcVendor*`; tanpa perubahan `MstSupplier` | Urutan 2 (kategori); registry `Vendor` `ACTIVE` (`PBJ-DEC-063`) |
| 8 | `AddProcurementSourcingMasterData` | `pemilihan-penyedia` | 3 master pemilihan | Registry `Sourcing` `ACTIVE` (prasyarat untuk 10; master memakai baris `Mst`) |
| 9 | `AddProcurementPurchaseRequestLinePackage` | `pemilihan-penyedia` (menyentuh tabel `perencanaan-permintaan`) | `PackageId`, `PackagedAt` di `PrcPurchaseRequestLine` | Urutan 3; persetujuan pemilik `perencanaan-permintaan` (pengguna sesi) |
| 10 | `AddProcurementSourcing` | `pemilihan-penyedia` | 14 tabel `Prc*` Sourcing | Urutan 8, 9; registry `Sourcing` `ACTIVE` |
| 11+ | belum dirancang | `kontrak-pelaksanaan` | — | Desain sub-modul |

FK dari PR ke baris rencana dibuat lewat kunci baris rencana (`NeedPlanLineKey`) tanpa FK fisik (lihat
arsitektur sub-modul), sehingga urutan 3 tidak bergantung pada urutan 4.

## 4. Pemetaan kemampuan ke sub-modul

| Kemampuan | ID | Sub-modul pemilik | Status |
| --- | --- | --- | --- |
| Rencana kebutuhan, konsolidasi, rencana paket, revisi | K-01 | `perencanaan-permintaan` | Dirancang |
| Rencana anggaran | K-02 | `perencanaan-permintaan` | Dirancang |
| PR, verifikasi, mendesak, riwayat | K-03 | `perencanaan-permintaan` | Dirancang |
| Cek stok manual | K-04 | `perencanaan-permintaan` | Dirancang; integrasi stok `DEFERRED` (`PBJ-OQ-008`) |
| Persiapan pengadaan | K-05 | `pemilihan-penyedia` | Dirancang (`draft`, 26 November 2026) |
| Data vendor | K-06 | `manajemen-vendor` | Dirancang (`approved`) |
| RFQ sampai penetapan penyedia | K-07 | `pemilihan-penyedia` | Dirancang (`draft`, 26 November 2026) |
| SPK, kontrak, termin, addendum, arsip | K-08 | `kontrak-pelaksanaan` | Belum dirancang |
| Monitoring pelaksanaan | K-09 | `kontrak-pelaksanaan` | Belum dirancang |
| Pemeriksaan, BAST, NCR, penyelesaian | K-10 | `kontrak-pelaksanaan` | Belum dirancang |
| Evaluasi vendor | K-11 | `manajemen-vendor` | Dirancang untuk penilaian berkala manual (`approved`); evaluasi per kontrak `DEFERRED` ke `S-V7` (`PBJ-DEC-044`) |
| Persetujuan bertingkat | K-12 | Mesin `Wfl` + penangan per sub-modul; layar 1.2, 11.1, 11.2 milik `perencanaan-permintaan`; 11.3 milik sub-modul pemilik dokumennya | Dirancang untuk PR, rencana, rencana paket, pagu |
| Dashboard dan laporan | K-13 | `perencanaan-permintaan` | `DEFERRED` ke `POST-MVP`: butuh data seluruh sub-modul. Pengganti selama MVP: daftar PR dan pagu dengan saringan |

Tidak ada kemampuan tanpa pemilik.
