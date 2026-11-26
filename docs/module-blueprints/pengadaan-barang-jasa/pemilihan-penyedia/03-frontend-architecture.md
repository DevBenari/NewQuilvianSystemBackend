# Pemilihan Penyedia — Arsitektur Frontend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2, sub-modul `pemilihan-penyedia` (`PBJ-PS`) · status `approved` (`PBJ-DEC-095`, 2026-11-26) |
| Masukan | `02-backend-architecture.md`, `contracts/*` 1.0 (approved); `PBJ-FE-001`..`004`; `PBJ-DEC-074`, `079`, `080`, `085` |
| Repository | `V2QuilvianSystemFrontendDev` @`3e5386679` |
| Kewenangan UI | `PBJ-FE-001` nama menu SOURCE (**nama 11 submenu menunggu `PBJ-OQ-038`**); `PBJ-FE-004` standar layar daftar; rupa lain `PBJ-FE-002` `DEV_DISCRETION`; `PBJ-FE-003` invariant keamanan tampilan |

> **Desain frontend ini tertahan sebagian oleh `PBJ-OQ-038`.** Layar, isi, dan sumber datanya sudah dikunci; yang belum
> dikunci adalah label dan pemetaan 11 butir submenu SOURCE menu 4 dan 6 ke layar-layar di bawah. Pemetaan pada bagian 3
> adalah **usulan** berdasarkan urutan kemampuan K-05 dan K-07.

## 1. Lokasi kode

Mengikuti pola `perencanaan-permintaan` dan `manajemen-vendor`: route `src/app/corporate/procurement/sourcing/**`, view
`src/components/view/corporate/procurement/sourcing/`, hook `src/lib/hooks/corporate/procurement/use-package-*.jsx`,
`use-rfq-*.jsx`, `use-quotation-*.jsx`, `use-technical-evaluation-*.jsx`, `use-award-*.jsx`, service
`src/lib/services/corporate/procurement/package-service.js`, `rfq-service.js`, `quotation-service.js`,
`technical-evaluation-service.js`, `award-service.js`, `sourcing-master-data-service.js`. Komponen bersama dipakai ulang
(`PBJ-CAP-029`): `procurement-approval-panel`, `procurement-attachment-panel`, `procurement-document-tabs`,
`procurement-history-panel`, `procurement-list-filter`, `procurement-status-badge` (tambah `kind` paket, RFQ, penetapan).
`procurement-approval-service.js` dan konstanta jenis dokumen ditambah `PBJ_PACKAGE` dan `PBJ_PACKAGE_AWARD`.

## 2. Kebutuhan layar

| ID | Layar | Jenis | Pengguna utama |
| --- | --- | --- | --- |
| `FE-PBJ-PS-01` | Daftar Paket Pengadaan | Daftar | Tim Pengadaan |
| `FE-PBJ-PS-02` | Buat Paket (pilih baris PR) | Form | Tim Pengadaan |
| `FE-PBJ-PS-03` | Detail Paket (persiapan, RFQ, penawaran, evaluasi, penetapan, persetujuan, lampiran, riwayat) | Detail | Tim Pengadaan, penyetuju |
| `FE-PBJ-PS-04` | Daftar RFQ | Daftar | Tim Pengadaan |
| `FE-PBJ-PS-05` | Detail RFQ dan Catat Penawaran | Detail/form | Tim Pengadaan |
| `FE-PBJ-PS-06` | Antrean Evaluasi Teknis | Daftar | Evaluator |
| `FE-PBJ-PS-07` | Evaluasi Teknis Paket | Form | Evaluator |
| `FE-PBJ-PS-08` | Perbandingan Harga dan Negosiasi | Detail/form | Tim Pengadaan |
| `FE-PBJ-PS-09` | Daftar Penetapan Penyedia | Daftar | Tim Pengadaan, Keuangan |
| `FE-PBJ-PS-10` | Ajukan / Detail Penetapan | Form/detail | Tim Pengadaan, penyetuju |
| `FE-PBJ-PS-11` | Persetujuan Paket (bagian paket menu 11.3) | Daftar | Penyetuju |
| `FE-PBJ-PS-12` | Pengaturan: Metode Pengadaan | Master | Admin Pengadaan |
| `FE-PBJ-PS-13` | Pengaturan: Jenis Dokumen Paket | Master | Admin Pengadaan |
| `FE-PBJ-PS-14` | Pengaturan: Dokumen Wajib Paket | Master | Admin Pengadaan |
| `FE-PBJ-PS-15` | Ringkasan paket di Detail PR (`perencanaan-permintaan`) | Panel di layar yang sudah ada | Unit peminta |

## 3. Peta butir menu

Daftar induk seluruh modul ada di `../02-module-map.md` bagian 3.1. Butir milik sub-modul ini (label submenu **usulan**,
menunggu `PBJ-OQ-038`):

| Tingkat | Induk | Butir (usulan) | Route usulan | Layar | Hak akses |
| ---: | --- | --- | --- | --- | --- |
| 1 | Pengadaan Barang & Jasa | 4. Persiapan Pengadaan | — | — | Salah satu izin di bawah |
| 2 | 4 | Paket Pengadaan (spesifikasi, KAK/TOR, HPS, metode, checklist) | `/corporate/procurement/sourcing/packages` | `-01` (→ `-02`, `-03`) | `ProcurementPackage : Read` |
| 1 | Pengadaan Barang & Jasa | 6. Penawaran & Pemilihan Penyedia | — | — | Salah satu izin di bawah |
| 2 | 6 | Permintaan Penawaran (RFQ) | `/corporate/procurement/sourcing/rfqs` | `-04` (→ `-05`) | `ProcurementRfq : Read` |
| 2 | 6 | Evaluasi Teknis | `/corporate/procurement/sourcing/technical-evaluations` | `-06` (→ `-07`) | `ProcurementTechnicalEvaluation : Read` |
| 2 | 6 | Penetapan Penyedia | `/corporate/procurement/sourcing/awards` | `-09` (→ `-10`) | `ProcurementPackageAward : Read` |
| 2 | 11 | Persetujuan Paket (bagian paket dari 11.3) | `/corporate/procurement/approvals/packages` | `-11` | `ApprovalInbox : Read` |
| 2 | Pengaturan Pengadaan | Metode Pengadaan | `/corporate/procurement/settings/procurement-methods` | `-12` | `ProcurementMethod : Read` |
| 2 | Pengaturan Pengadaan | Jenis Dokumen Paket | `/corporate/procurement/settings/package-document-types` | `-13` | `ProcurementPackageDocumentType : Read` |
| 2 | Pengaturan Pengadaan | Dokumen Wajib Paket | `/corporate/procurement/settings/package-document-requirements` | `-14` | `ProcurementPackageDocumentRequirement : Read` |

**Layar anak:** `-02` dari tombol "+ Buat Paket" di `-01`; `-03` dari baris `-01`, `-04`, `-09`, `-11`; `-05` dari baris
`-04` dan tab RFQ `-03`; `-07` dari baris `-06`; `-08` dari tab Penawaran `-03`; `-10` dari tombol "Ajukan penetapan" di
`-08` dan baris `-09`; `-15` adalah panel di Detail PR yang sudah ada. Sebelas butir SOURCE menu 4 dan 6 dapat menjadi
butir sendiri yang membuka `-01`, `-03`, `-04`, `-06`, `-08`, atau `-09` dengan saringan/tab awal berbeda; pemetaan finalnya
setelah `PBJ-OQ-038`.

## 4. Skema fitur per layar

### 4.1 Daftar Paket — `FE-PBJ-PS-01`

```text
┌ Hero: "Paket Pengadaan" (tanpa tombol) ──────────────────────────────────────┐
├ Filter (satu baris, PBJ-FE-004): Tgl awal · Tgl akhir · Periode · Jenis       │
│   pengadaan · Tahun anggaran · Status · Cari · Jumlah baris · ↻ · [+ Buat Paket]│
├ Tabel: No. paket · Judul · Jenis · Metode · HPS* · Status · Penanggung jawab ─┤
└ Paginasi ─────────────────────────────────────────────────────────────────────┘
   * kosong bagi pengguna tanpa ReadPrice
```

| Wilayah | Isi | Sumber | Hak akses tombol | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Filter | Jenis pengadaan, Tahun anggaran (`PBJ-FE-004`) | `GET /procurement-packages/filters/metadata` | "+ Buat Paket" = `ProcurementPackage : Create` | — |
| Tabel | Paket; paket draf tampil "Draf" tanpa nomor | `GET /procurement-packages` | "Buka" = `Read` | "Belum ada paket pada saringan ini." / "Data paket gagal dimuat." + Coba lagi |

### 4.2 Buat Paket — `FE-PBJ-PS-02`

```text
┌ Hero: "Buat Paket" ───────────────────────────────────────────────────────────┐
├ Judul paket · (opsional) Rencana paket rujukan ───────────────────────────────┤
├ Filter baris: Tahun · Jenis pengadaan · Kategori · Unit · Cari ───────────────┤
├ Tabel baris PR siap paket: ☐ · No. PR · Unit · Barang · Jumlah beli · Harga*  │
└ [Batal] [Buat Paket] ─────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber | Hak akses | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Tabel | Baris PR `APPROVED` belum dipaketkan | `GET /procurement-packages/packageable-request-lines` | `Create` | "Tidak ada baris permintaan yang siap dipaketkan." |
| Simpan | Paket baru | `POST /procurement-packages` | `Create` | 409 `PBJ-VAL-121` → "Baris … sudah masuk paket lain", daftar dimuat ulang |

### 4.3 Detail Paket — `FE-PBJ-PS-03`

```text
┌ Hero: PKT-000012 · Kursi roda IGD & Rawat Inap · [status] · [Ajukan persiapan]┐
├ Tab: Persiapan │ RFQ │ Penawaran │ Penetapan │ Persetujuan │ Lampiran │ Riwayat│
├ Persiapan: tabel barang (spesifikasi, jumlah, asal PR, HPS*) · Metode +       │
│   saran · alasan · Checklist dokumen (✓/✗) · Evaluator · Kriteria teknis       │
└ Aksi: [Simpan] [Pecah paket] [Buka ulang persiapan] [Batalkan] [Catat No. PO]┘
```

| Wilayah | Isi | Sumber | Hak akses tombol | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Persiapan | Barang, sumber PR, HPS, metode tersalin, checklist | `GET /procurement-packages/{id}`, `GET /{id}/method-suggestion` | Simpan = `Update`; Ajukan/Tarik = `Submit`; Buka ulang = `Revise` | Checklist belum lengkap → daftar dokumen kurang |
| RFQ | RFQ paket ini | `GET /procurement-rfqs?packageId=` | "+ Buat RFQ" = `ProcurementRfq : Create` | "Belum ada RFQ." |
| Penawaran | Ringkasan penawaran → `-08` | `GET /procurement-quotations?packageId=` | `ProcurementQuotation : Read` | "Belum ada penawaran tercatat." |
| Penetapan | Penetapan hidup dan riwayatnya | `GET /procurement-package-awards?packageId=` | Cabut = `ProcurementPackageAward : Revoke` | — |
| Persetujuan | Linimasa persiapan dan penetapan | `GET /procurement-packages/{id}/approval` | — | "Belum diajukan." |
| Lampiran | Berkas per jenis dokumen | Komponen lampiran bersama (`PBJ_PACKAGE`) | Unggah = `ProcurementAttachment : Upload` | Lampiran harga tidak tampil bagi pengguna tanpa `ReadPrice` |
| Aksi | Pecah, batal, catat PO | `POST /{id}/split`, `/cancel`, `/purchase-order` | `Update`, `Cancel`, `RecordOrder` | 409 `PBJ-VAL-144`/`145`/`147` ditampilkan apa adanya |

### 4.4 Detail RFQ dan Catat Penawaran — `FE-PBJ-PS-05`

```text
┌ Hero: RFQ-000007 · PKT-000012 · batas 10-04-2027 16.00 · [status] ───────────┐
├ Undangan: Vendor · Dibatasi? alasan · Sudah membalas? ────────────────────────┤
├ Penawaran: Vendor · Diterima · Terlambat? · PPN% · Total akhir* · [Ubah]       │
├ Form catat: Vendor · Waktu diterima · PPN% · harga per barang · lampiran ─────┤
└ Aksi: [Kirim] [Perpanjang batas waktu] [Tutup] [Nyatakan gagal] [Unduh RFQ] ──┘
```

| Wilayah | Sumber | Hak akses tombol | Kosong / gagal |
| --- | --- | --- | --- |
| Undangan | `GET /procurement-rfqs/{id}`, `GET /procurement-rfqs/eligible-vendors?packageId=` | Ubah = `ProcurementRfq : Update`; Kirim = `Send` | "Tidak ada vendor yang layak untuk semua kategori paket ini." |
| Penawaran | `GET /procurement-quotations?rfqId=`, `POST /procurement-quotations` | Catat = `ProcurementQuotation : Create`; Batalkan catatan = `Cancel` | Penawaran terlambat ditandai "Terlambat — tidak sah" |
| Aksi | `POST /{id}/send`, `/extend-deadline`, `/close`, `/fail` | `Send`, `Update`, `Close` | 422 `PBJ-VAL-132` saat batas waktu lewat |

### 4.5 Evaluasi Teknis — `FE-PBJ-PS-06` dan `FE-PBJ-PS-07`

```text
┌ Hero: "Evaluasi Teknis" ─────────────────────────────────────────────────────┐
├ -06 Tabel: No. paket · Judul · Batas evaluasi · Selesai saya? ───────────────┤
├ -07 Per barang: spesifikasi · kriteria · per vendor: lampiran teknis ·       │
│      [Lulus | Tidak lulus + alasan]      (TANPA kolom harga sama sekali)     │
└ [Simpan hasil] ──────────────────────────────────────────────────────────────┘
```

| Wilayah | Sumber | Hak akses | Kosong / gagal |
| --- | --- | --- | --- |
| Antrean | `GET /technical-evaluations` | `ProcurementTechnicalEvaluation : Read` | "Tidak ada paket yang ditugaskan kepada Anda." |
| Form | `GET /technical-evaluations/{packageId}`, `PUT .../quotations/{quotationId}` | `Evaluate` | 403 `PBJ-VAL-135` → "Anda mencatat penawaran ini…" |

### 4.6 Perbandingan dan Negosiasi — `FE-PBJ-PS-08`; Penetapan — `FE-PBJ-PS-09`, `FE-PBJ-PS-10`

```text
┌ Perbandingan: Peringkat · Vendor · Sah? · Lulus? · Layak? · Total akhir ·    │
│   selisih vs HPS · [Negosiasi] · ★ terendah ────────────────────────────────── │
├ Negosiasi: putaran · tanggal · peserta · harga baru per barang · berita acara  │
├ Penetapan: vendor dipilih · alasan bukan terendah · alasan di atas HPS ·      │
│   alasan vendor dibatasi · [Ajukan penetapan]                                  │
└ Detail penetapan: status · peringkat tersimpan · linimasa · [Tarik] [Cabut] ──┘
```

| Wilayah | Sumber | Hak akses | Kosong / gagal |
| --- | --- | --- | --- |
| Perbandingan | `GET /procurement-quotations/comparison?packageId=` | `ProcurementQuotation : Read` | "Belum ada penawaran yang sah." |
| Negosiasi | `POST /procurement-quotations/{id}/negotiations` | `Negotiate` | 422 `PBJ-VAL-137`/`138` |
| Ajukan | `POST /procurement-package-awards` | `ProcurementPackageAward : Submit` | 422 `PBJ-VAL-139`..`143` per isian |
| Daftar `-09` | `GET /procurement-package-awards` (saringan Status, Tahun) | `Read` | "Belum ada penetapan." |

### 4.7 Layar lain

| Layar | Pola | Sumber |
| --- | --- | --- |
| `-04` Daftar RFQ | Daftar `PBJ-FE-004`, saringan Status, Paket | `GET /procurement-rfqs` |
| `-11` Persetujuan Paket | Kotak masuk `Wfl` disaring jenis dokumen `PBJ_PACKAGE`, `PBJ_PACKAGE_AWARD` (pola Antrean Persetujuan, `PBJ-DEC-058`) | `GET approval-inbox` |
| `-12`..`-14` Pengaturan | Kartu editor di atas daftar (pola `FE-PBJ-VD-001`) | Endpoint master masing-masing |
| `-15` Panel paket di Detail PR | Baris PR: nomor paket, status, vendor terpilih | `GET /procurement-packages/request-summary?purchaseRequestId=` (`ReadSummary`) |

## 5. Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` bagian 3: Tim Pengadaan mengerjakan `-01`..`-05`, `-08`..`-10`;
evaluator hanya `-06`, `-07`; penyetuju membaca `-03`, `-10` dan memutus di `-11`; Keuangan membaca `-09`; unit peminta hanya
`-15`. Tombol disembunyikan bila izinnya tidak ada; server tetap menolak (`PBJ-FE-003`).

## 6. Penanganan keadaan

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka tabel/form; tombol aksi nonaktif |
| Kosong | Kalimat pada tabel wilayah di atas |
| Gagal | Pesan + "Coba lagi"; galat 422 ditampilkan di isian terkait memakai `errors.code` |
| Data basi | 409 `PBJ-VAL-090` → "Data sudah diubah orang lain. Muat ulang?" lalu muat ulang |
| Pengiriman ganda | Tombol aksi status terkunci selama permintaan; `Idempotency-Key` per klik |
| Harga tertutup | Field harga `null` tampil "—" tanpa keterangan nilai; jangan menampilkan angka 0 |

## 7. Kewenangan UI

| Hal | Status |
| --- | --- |
| Nama dan pemetaan 11 submenu menu 4 dan 6 | **Menunggu `PBJ-OQ-038`** (`PBJ-FE-001`) |
| Baris filter dan hero tanpa tombol pada daftar | `PBJ-FE-004` (approved) |
| Pembagian tab Detail Paket, tata letak, warna, ikon | `DEV_DISCRETION` (`PBJ-FE-002`) |
| Harga tidak tampil bagi evaluator dan pengguna tanpa `ReadPrice` | Invariant (`PBJ-FE-003`, `PBJ-DEC-079`) |
