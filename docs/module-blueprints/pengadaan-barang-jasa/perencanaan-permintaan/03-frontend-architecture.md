# Perencanaan & Permintaan — Arsitektur Frontend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · `perencanaan-permintaan` · status `approved` (`PBJ-DEC-034`) |
| Repository | `V2QuilvianSystemFrontendDev` @`763d665b` (Next.js App Router, JavaScript/JSX, Redux Toolkit, Axios) |
| Masukan | `PBJ-FE-001`..`003`; `contracts/api-contract.md` 1.0; `contracts/permission-audit-matrix.md` 1.0 |

Dokumen ini mengunci isi layar dan sumber datanya, bukan rupanya. Warna, jarak, ikon, component library,
serta pilihan tab/modal/drawer adalah `DEV_DISCRETION` (`PBJ-FE-002`), mengikuti design token dan base
component Quilvian yang ada.

## 1. Lokasi kode (konvensi `FE/AGENTS.md`)

| Jenis | Lokasi |
| --- | --- |
| Route | `src/app/corporate/procurement/**/page.jsx` |
| View | `src/components/view/corporate/procurement/**` |
| Service API | `src/lib/services/corporate/procurement/*.js` memakai `InstanceAxios` (`src/lib/axiosInstance/InstanceAxios`) |
| Hook | `src/lib/hooks/corporate/procurement/*.jsx` |
| Slice Redux | `src/lib/state/slice/corporate/procurement/*` (hanya untuk saringan dan cache daftar; detail dokumen dibaca ulang dari API) |
| Konstanta label status | `src/lib/constants/corporate/procurement/*.jsx` |
| Menu | `src/utils/menu-sidebar/menu-items.jsx` (`subMenu` tingkat 0, `subItems` grup) |

## 2. Kebutuhan layar

| ID | Layar | Submenu SOURCE | Gelombang |
| --- | --- | --- | --- |
| `FE-PBJ-PP-01` | Daftar Rencana Kebutuhan | 2.1 | `MVP-2` |
| `FE-PBJ-PP-02` | Form/Detail Rencana Kebutuhan | anak 2.1 | `MVP-2` |
| `FE-PBJ-PP-03` | Konsolidasi Kebutuhan Unit | 2.2 | `MVP-4` |
| `FE-PBJ-PP-04` | Daftar Pagu | 2.3 | `MVP-3` |
| `FE-PBJ-PP-05` | Form/Detail Pagu | anak 2.3 | `MVP-3` |
| `FE-PBJ-PP-06` | Daftar Rencana Paket | 2.4 | `MVP-4` |
| `FE-PBJ-PP-07` | Form/Detail Rencana Paket | anak 2.4 | `MVP-4` |
| `FE-PBJ-PP-08` | Revisi Perencanaan (daftar revisi rencana dan pagu + banding versi) | 2.5 | `MVP-2` |
| `FE-PBJ-PP-09` | Daftar Pengajuan (PR unit saya) | 3.1 | `MVP-1` |
| `FE-PBJ-PP-10` | Form/Detail PR | anak 3.1 | `MVP-1` |
| `FE-PBJ-PP-11` | Antrean Verifikasi | 3.2, 11.2 | `MVP-1` |
| `FE-PBJ-PP-12` | Ruang Kerja Verifikasi PR (cek stok, selesaikan, kembalikan, tolak) | anak 3.2 | `MVP-1` |
| `FE-PBJ-PP-13` | Daftar Hasil Cek Stok | 3.3 | `MVP-1` |
| `FE-PBJ-PP-14` | Permintaan Mendesak (daftar PR mendesak lintas status) | 3.4 | `MVP-1` |
| `FE-PBJ-PP-15` | Riwayat Pengajuan | 3.5 | `MVP-1` |
| `FE-PBJ-PP-16` | Pengajuan Menunggu Persetujuan | 1.2 | `MVP-1` |
| `FE-PBJ-PP-17` | Antrean Persetujuan Pengadaan (pintu ke kotak masuk `Wfl`, disaring kategori Procurement) | 11.1 | `MVP-1` |
| `FE-PBJ-PP-18` | Katalog Barang/Jasa (daftar + form) | Pengaturan (`PBJ-DEC-032`) | `MVP-0` |
| `FE-PBJ-PP-19` | Kategori dan Sumber Dana | Pengaturan (`PBJ-DEC-032`) | `MVP-0` |

## 3. Peta butir menu

Peta seluruh modul ada di [`../02-module-map.md`](../02-module-map.md) bagian 3. Butir milik sub-modul ini:

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses |
| --- | :---: | --- | --- | --- | --- |
| Pengadaan Barang & Jasa | 0 | — | — | — | — |
| 1. Dashboard Pengadaan | 1 | Pengadaan | — | — | — |
| Pengajuan Menunggu Persetujuan | 2 | 1 | `/corporate/procurement/dashboard/pending-approvals` | `FE-PBJ-PP-16` | `ProcurementRequest : Read` |
| 2. Rencana Pengadaan | 1 | Pengadaan | — | — | — |
| Rencana Kebutuhan Tahunan | 2 | 2 | `/corporate/procurement/planning/need-plans` | `FE-PBJ-PP-01` | `ProcurementNeedPlan : Read` |
| Konsolidasi Kebutuhan Unit | 2 | 2 | `/corporate/procurement/planning/consolidation` | `FE-PBJ-PP-03` | `ProcurementPackagePlan : Read` |
| Rencana Anggaran | 2 | 2 | `/corporate/procurement/planning/budgets` | `FE-PBJ-PP-04` | `ProcurementBudget : Read` |
| Rencana Paket Pengadaan | 2 | 2 | `/corporate/procurement/planning/package-plans` | `FE-PBJ-PP-06` | `ProcurementPackagePlan : Read` |
| Revisi Perencanaan | 2 | 2 | `/corporate/procurement/planning/revisions` | `FE-PBJ-PP-08` | `ProcurementNeedPlan : Read` |
| 3. Permintaan Pengadaan | 1 | Pengadaan | — | — | — |
| Pengajuan Pembelian Barang & Jasa | 2 | 3 | `/corporate/procurement/requests` | `FE-PBJ-PP-09` | `ProcurementRequest : Read` |
| Verifikasi Kebutuhan | 2 | 3 | `/corporate/procurement/requests/verification` | `FE-PBJ-PP-11` | `ProcurementRequestVerification : Read` |
| Pemeriksaan Stock | 2 | 3 | `/corporate/procurement/requests/stock-checks` | `FE-PBJ-PP-13` | `ProcurementRequestVerification : Read` |
| Permintaan Mendesak | 2 | 3 | `/corporate/procurement/requests/urgent` | `FE-PBJ-PP-14` | `ProcurementRequest : Read` |
| Riwayat Pengajuan | 2 | 3 | `/corporate/procurement/requests/history` | `FE-PBJ-PP-15` | `ProcurementRequest : Read` |
| 11. Persetujuan & Laporan Pengadaan | 1 | Pengadaan | — | — | — |
| Antrean persetujuan | 2 | 11 | `/corporate/procurement/approvals/inbox` | `FE-PBJ-PP-17` | `ApprovalInbox : Read` |
| Validasi Pengajuan | 2 | 11 | `/corporate/procurement/approvals/validation` | `FE-PBJ-PP-11` (route kedua, layar sama) | `ProcurementRequestVerification : Read` |
| Pengaturan Pengadaan | 1 | Pengadaan | — | — | — |
| Katalog Barang/Jasa | 2 | Pengaturan | `/corporate/procurement/settings/items` | `FE-PBJ-PP-18` | `ProcurementItem : Read` |
| Kategori & Sumber Dana | 2 | Pengaturan | `/corporate/procurement/settings/references` | `FE-PBJ-PP-19` | `ProcurementItem : Read` |

**Layar anak** (tanpa butir menu):
- `FE-PBJ-PP-02` dari `-01` (tombol Buat / klik baris);
- `-05` dari `-04`;
- `-07` dari `-06` dan `-03` (tombol "Buat rencana paket dari pilihan");
- `-10` dari `-09`, `-14`, `-15`, `-16`;
- `-12` dari `-11` dan `-13`.

Route anak: `/new` dan `/[id]` di bawah route induknya.

**Catatan "Validasi Pengajuan" (11.2):** submenu ini dan 3.2 menunjuk layar yang sama, karena keduanya satu
kemampuan (aturan "satu kemampuan, satu tempat"). Bila pemilik ingin 11.2 berbeda (mis. validasi anggaran oleh
Keuangan), itu keputusan baru.

Pendaftaran butir menu menjadi acceptance criteria task layar pertama setiap kelompok, bukan pekerjaan terpisah
yang menganggur.

## 4. Skema fitur per layar

### 4.1 Daftar Pengajuan — `FE-PBJ-PP-09`

Layar `-11`, `-13`, `-14`, `-15`, `-16`, `-01`, `-04`, `-06`, dan `-18` berbentuk sama. Perbedaannya hanya pada
saringan bawaan dan kolom, yang dicatat pada bagian 4.6.

```text
+- Pengajuan Pembelian Barang & Jasa ------------------------ FE-PBJ-PP-09 -+
| [cari nomor PR]  [Unit v] [Status v] [Tahun v] [Mendesak v]  [+ Buat PR]  |
+---------------------------------------------------------------------------+
| No. PR | Unit | Tgl Ajuan | Dibutuhkan | Nilai Beli | Tanda | Status |     |
| PR-…   | IGD  | …         | …          | Rp …       | ⚑ ⚠   | chip   |[>]  |
+---------------------------------------------------------------------------+
| memuat -> kerangka baris                                                  |
| kosong -> "Belum ada pengajuan pada saringan ini."     [Atur ulang]       |
| gagal  -> "Data pengajuan gagal dimuat."               [Coba lagi]        |
+- Halaman 1 dari n ---------------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Saringan | Nomor, unit (hanya unit dalam cakupan), status, tahun, urgensi | `GET /purchase-requests` query | `ProcurementRequest : Read` | — |
| Tombol Buat PR | Ke `-10` mode baru | — | `ProcurementRequest : Create` | Disembunyikan bila tidak berhak |
| Tabel | Nomor (atau "Draf"), unit, tanggal, nilai beli (`purchaseTotal`, atau `requestedTotal` sebelum verifikasi), tanda Mendesak / Di luar rencana / Melebihi pagu, status berlabel teks | `PagedResult<PurchaseRequestListItem>` | — | Lihat skema |

Status ditampilkan sebagai teks label dari `state-transition-matrix.md`, tidak hanya warna (BRD B10).

### 4.2 Form/Detail PR — `FE-PBJ-PP-10`

```text
+- PR-000031 · IGD · Menunggu Persetujuan ------------------- FE-PBJ-PP-10 -+
| Mendesak ⚑  Di luar rencana (2 baris)  Melebihi pagu Rp 15.000.000        |
| [Simpan] [Ajukan] [Batalkan] [Tarik]                                      |
+---------------------------------------------------------------------------+
| Kepala: unit, tahun, sumber dana, tanggal dibutuhkan, urgensi, alasan     |
| Baris: barang (katalog), spesifikasi, jumlah, satuan, harga perkiraan,    |
|        baris rencana + sisa, dalam/luar rencana, alasan luar rencana,     |
|        stok tersedia, dipenuhi stok, kebutuhan beli, nilai beli           |
| Ringkasan: total diminta, total beli, sisa pagu per kategori              |
| Lampiran | Persetujuan (langkah, penyetuju, keputusan) | Riwayat          |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala dan tanda | Nomor, unit, status, tanda | `GET /purchase-requests/{id}` | `ProcurementRequest : Read` | 404 → "Pengajuan tidak ditemukan atau bukan milik unit Anda." |
| Tombol Simpan | Hanya `DRAFT`/`RETURNED` dan pengguna pembuat | `PUT /{id}` | `ProcurementRequest : Update` | 409 basi → "Data sudah diubah orang lain." + [Muat ulang] |
| Tombol Ajukan | Idem | `POST /{id}/submit` + `Idempotency-Key` | `ProcurementRequest : Submit` | 422 → pesan per baris di dekat isian yang salah |
| Tombol Batalkan / Tarik | Sesuai status | `POST /{id}/cancel` | `ProcurementRequest : Cancel` | Konfirmasi wajib + alasan |
| Pilih barang | Pencarian katalog aktif | `GET /master-data/items/options` | `ProcurementItem : Read` | Kosong → "Barang tidak ada di katalog. Minta Tim Pengadaan menambahkannya." |
| Baris rencana + sisa | Pilihan baris rencana dan sisanya | `GET /purchase-requests/plan-line-options` | `ProcurementRequest : Create` | Tidak ada rencana → "Tidak ada rencana untuk barang ini; baris akan ditandai di luar rencana." |
| Sisa pagu | Per kategori | `GET /budgets/remaining` | `ProcurementRequest : Read` | Gagal dimuat → ringkasan disembunyikan; tidak menghalangi pengajuan |
| Lampiran | Daftar, unggah, unduh | `GET/POST /attachments` | `ProcurementAttachment : Read/Upload` | Kosong → "Belum ada lampiran." |
| Persetujuan | Langkah dan keputusan | Detail instance `Wfl` lewat `workflowInstanceId` (endpoint `Wfl` existing) | `ApprovalInbox : Read` atau baca PR | Belum diajukan → "Belum masuk persetujuan." |
| Riwayat | Linimasa | `GET /purchase-requests/{id}/history` | `ProcurementRequest : Read` | — |

Angka hasil hitungan (sisa rencana, kebutuhan beli, nilai beli, tanda) **hanya ditampilkan dari respons
server**. Frontend boleh menampilkan perkiraan saat mengetik, dengan label "perkiraan".

### 4.3 Ruang Kerja Verifikasi — `FE-PBJ-PP-12`

```text
+- Verifikasi PR-000031 · IGD ------------------------------- FE-PBJ-PP-12 -+
| [Simpan cek stok] [Selesaikan verifikasi] [Kembalikan] [Tolak]            |
+---------------------------------------------------------------------------+
| Baris | Barang | Diminta | Stok tersedia [ ] | Dipenuhi stok [ ] | Sumber |
|       |        |         |                   |                   | Beli   |
+---------------------------------------------------------------------------+
| Ringkasan: nilai beli, lewat pagu, jalur persetujuan (normal/mendesak)    |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Isian cek stok | Per baris | `PUT /purchase-request-verifications/{id}/stock-checks` | `ProcurementRequestVerification : Verify` | 422 → pesan di baris |
| Selesaikan | — | `POST /{id}/complete` + `Idempotency-Key` | idem | 409 `PBJ-VAL-091` → "Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator." |
| Kembalikan / Tolak | Alasan wajib | `POST /{id}/return`, `/{id}/reject` | idem | — |

Tombol verifikasi **disembunyikan** bila pengguna adalah pembuat PR.

### 4.4 Rencana Kebutuhan — `FE-PBJ-PP-02` dan Revisi — `FE-PBJ-PP-08`

`-02`: kepala (unit, tahun, versi, status), baris (barang, jumlah, harga, bulan, sumber dana, alasan), dan kolom
baca "Sudah dipakai PR" serta "Dialokasikan paket". Tombol: Simpan, Ajukan, Buat Revisi (`ProcurementNeedPlan : Revise`).

`-08`: daftar versi rencana dan pagu dengan alasan revisi, serta panel banding per baris
(`GET /need-plans/{id}/compare/{otherId}`): jumlah dan nilai sebelum → sesudah.

### 4.5 Konsolidasi — `FE-PBJ-PP-03`

Tabel per barang: total, rincian unit (bisa dibuka), sudah dialokasikan, sisa. Kotak centang per baris unit
dan isian jumlah. Tombol "Buat rencana paket dari pilihan" (`ProcurementPackagePlan : Create`) membuka `-07`.
Sumber: `GET /plan-consolidations`. Bila kosong: "Belum ada rencana kebutuhan yang disetujui untuk tahun ini."

### 4.6 Perbedaan layar berbentuk daftar

| Layar | Saringan bawaan | Kolom khusus | Sumber |
| --- | --- | --- | --- |
| `-11` Antrean Verifikasi | `SUBMITTED` | Umur antrean, mendesak di atas | `GET /purchase-request-verifications` |
| `-13` Hasil Cek Stok | Baris yang sudah dicek | Barang, stok tersedia, dipenuhi, sumber, pemeriksa, waktu | `GET /purchase-request-verifications/stock-checks` |
| `-14` Mendesak | `urgencyLevel = URGENT`, semua status | Batas waktu, sisa hari | `GET /purchase-requests` |
| `-15` Riwayat | Semua status final dan berjalan | Aksi terakhir, pelaku | `GET /purchase-requests` + `/{id}/history` |
| `-16` Menunggu Persetujuan | `PENDING_APPROVAL` | Langkah saat ini | `GET /purchase-requests` |
| `-17` Antrean Persetujuan | Tugas pengguna di `Wfl`, kategori Procurement | Jenis dokumen, nominal, SLA | Kotak masuk `Wfl` existing (`api/v1/corporate/human-resource/approval-inbox`) |

## 5. Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 3. Frontend
menyembunyikan tombol yang tidak berhak (`PBJ-FE-003`), dan backend tetap menjadi penjaga.

## 6. Penanganan keadaan

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka baris pada daftar; tombol aksi nonaktif selama permintaan berjalan |
| Kosong | Kalimat per layar (bagian 4) dengan tombol atur ulang saringan |
| Gagal | Pesan + [Coba lagi]; tanpa jejak teknis atau token |
| Data basi (409 `PBJ-VAL-090`) | Pesan + [Muat ulang]; isian pengguna tidak dibuang diam-diam |
| Pengiriman ganda | Tombol dikunci setelah ditekan; `Idempotency-Key` dibuat sekali per niat aksi dan dipakai ulang saat diulang |
| Di luar cakupan (404) | "Tidak ditemukan atau bukan milik unit Anda." |
| Konfirmasi | Wajib untuk Ajukan, Selesaikan verifikasi, Tolak, Batalkan, Tarik; alasan wajib untuk Kembalikan, Tolak, Batalkan |

## 7. Kewenangan UI

| ID | Area | Status |
| --- | --- | --- |
| `PBJ-FE-001` | Nama menu dan submenu SOURCE | `approved` |
| `PBJ-FE-002` | Rupa, tata letak, tab/modal/drawer, komponen | `DEV_DISCRETION` |
| `PBJ-FE-003` | Tombol tersembunyi bila tidak berhak; nominal tidak dihitung frontend sebagai nilai final | Invariant |
| `PBJ-DEC-032` | Grup "Pengaturan Pengadaan" di luar 11 menu | `approved` |
