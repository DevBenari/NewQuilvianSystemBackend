# Pergerakan Aset — Arsitektur Frontend

| Field | Value |
|---|---|
| Sub-modul | `pergerakan` — `AST-MOV-1.0.0` `draft` |
| Route dasar | `src/app/corporate/asset-management/movement/` dan `/reports/transfers` |
| Komponen dasar | Sama dengan `../register/03-frontend-architecture.md`; ditambah komponen pilih-aset bersama `src/components/view/corporate/asset-management/asset-picker.jsx` (Baru, dibuat task `register`, dipakai ulang di sini) |

## 1. Kebutuhan layar

| ID | Layar | Menu | Endpoint |
|---|---|---|---|
| `SCR-AST-MOV-01` | Pengajuan Pemindahan (daftar mutasi saya + formulir) | 5.1 | `GET/POST /movement/transfers` |
| `SCR-AST-MOV-02` | Persetujuan Mutasi (antrean `awaitingMe`) | 5.2 | `GET /movement/transfers?awaitingMe=true`, `/approve-unit`, `/approve-asset` |
| `SCR-AST-MOV-03` | Serah Terima Antar Unit (menunggu diterima unit saya) | 5.3 | `GET /movement/transfers?status=DisetujuiBagianAset&targetOrganizationUnitId=…`, `/receive`, `/reject-receipt` |
| `SCR-AST-MOV-04` | Peminjaman Sementara | 5.4 | `GET/POST /movement/loans`, `/decide`, `/pickup`, `/extensions` |
| `SCR-AST-MOV-05` | Pengembalian Aset | 5.5 | `GET /movement/loans?status=Dipinjam,MenungguKonfirmasiKembali`, `/return`, `/confirm-return` |
| `SCR-AST-MOV-06` | Detail Mutasi (layar anak) | — | `GET /movement/transfers/{id}`, `/document` |
| `SCR-AST-MOV-07` | Detail Pinjaman (layar anak) | — | `GET /movement/loans/{id}`, `/document` |
| `SCR-AST-MOV-08` | Laporan Mutasi | 11.2 | `GET /movement/reports/transfers`, `/export` |

## 2. Peta butir menu

Peta seluruh modul: [`../02-module-map.md`](../02-module-map.md) 3.1. Butir milik sub-modul ini: grup
**Mutasi & Peminjaman Aset** (5 butir) dan **Laporan Mutasi**. Layar anak `SCR-AST-MOV-06` dibuka dari baris
`SCR-AST-MOV-01..03`, `08`, dan linimasa Detail Aset; `SCR-AST-MOV-07` dari `SCR-AST-MOV-04`, `05`.

## 3. Skema fitur

### 3.1 Daftar (dipakai `SCR-AST-MOV-01..05`, `08`)

Bentuk daftar bersama `register` 3.1 dengan kolom: Nomor, Unit asal → tujuan (atau pemilik → peminjam),
Jumlah aset, Status (chip; pinjaman terlambat bertanda **Terlambat**), Tanggal, Menunggu siapa.

| Wilayah | Sumber | Hak akses | Kosong / gagal |
|---|---|---|---|
| Tabel | Endpoint daftar layar | `AssetTransfer : Read` / `AssetLoan : Read` | `MOV-02`: "Tidak ada mutasi yang menunggu persetujuan Anda." · `MOV-03`: "Tidak ada aset yang menunggu diterima unit Anda." · `MOV-05`: "Tidak ada pinjaman yang perlu dikembalikan." |
| Tambah | Formulir | `AssetTransfer : Create` / `AssetLoan : Create` | — |

### 3.2 Formulir mutasi — `SCR-AST-MOV-01`

```text
+- Ajukan Mutasi ------------------------------------------------ SCR-AST-MOV-01 -+
| Unit asal: IGD (otomatis dari aset pertama)    Unit tujuan* [v]               |
| Alasan* (Kebutuhan layanan | Koreksi mutasi | Lainnya)  Catatan* [        ]   |
| Ruangan tujuan [v opsional]   Penanggung jawab [v opsional]                   |
| Aset* [ + pilih aset ]  (pilih-aset menyembunyikan aset terkunci/hilang/rusak) |
|  AST-ALKMED-000123  Monitor Pasien  Aktif   [hapus]                          |
|                                                        [Ajukan]               |
+-------------------------------------------------------------------------------+
```

| Wilayah | Sumber | Hak akses | Gagal |
|---|---|---|---|
| Pilih aset | `GET /register/assets?organizationUnitId=…&recordStatus=Terdaftar&lockFree=true` | `AssetRegister : Read` | 409 per aset menampilkan sebab dari `AST-VAL-MOV-001..006` |
| Ajukan | `POST /movement/transfers` | `AssetTransfer : Create` | Galat per isian |

### 3.3 Detail mutasi — `SCR-AST-MOV-06`

```text
+- AST-MUT-000045 - IGD -> ICU ---------------------------------- SCR-AST-MOV-06 -+
| DisetujuiBagianAset   Diajukan: Petugas A 12/04  Unit: Kepala IGD 12/04        |
| Bagian Aset: Kepala BA 13/04   Menunggu: Kepala Unit ICU                       |
| [Setujui] [Tolak] [Terima] [Tolak terima] [Batalkan] [Cetak berita acara]      |
+---------------------------------------------------------------------------------+
| Aset | Kondisi diajukan | Kondisi diterima                                     |
+---------------------------------------------------------------------------------+
```

| Tombol | Hak akses | Tampil bila |
|---|---|---|
| Setujui / Tolak (unit) | `AssetTransfer : ApproveUnit` | Status `Diajukan` dan pengguna Kepala Unit asal, bukan pengaju |
| Setujui / Tolak (Bagian Aset) | `AssetTransfer : ApproveAsset` | Status `DisetujuiUnitAsal`, bukan pengaju |
| Terima / Tolak terima | `AssetTransfer : Receive` | Status `DisetujuiBagianAset` dan pengguna Kepala Unit tujuan |
| Batalkan | `AssetTransfer : Cancel` | Belum `Diserahterimakan`, pengguna pengaju atau Kepala Bagian Aset |
| Cetak berita acara | `AssetTransfer : Read` | `Diserahterimakan` |

Tombol yang tidak berhak **disembunyikan**. Ketentuan "tampil bila" memakai data respons
(`AvailableActions` dihitung backend), bukan tebakan frontend.

### 3.4 Pinjaman — `SCR-AST-MOV-04`, `05`, `07`

Bentuk sama dengan 3.2–3.3. Tombol: Setujui/Tolak (`AssetLoan : Approve`), Ambil (`Receive`), Kembalikan
(`Return`), Konfirmasi kembali + kondisi per aset (`ConfirmReturn`), Minta perpanjangan (`Extend`),
Putuskan perpanjangan (`Approve`), Batalkan (`Cancel`).

## 4. Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 1.

## 5. Data dan status

Label status dari `asset-status-labels.js` (dibuat `register`). **Terlambat** ditampilkan dari field
turunan `isOverdue` pada `AssetLoanResponse`, bukan dihitung ulang di browser.

## 6. Penanganan keadaan

Sama dengan `register` bagian 6. Tambahan: setelah `409 AST-VAL-MOV-016`, detail dimuat ulang dan tombol
yang tidak lagi berlaku disembunyikan.

## 7. Kewenangan UI

Rupa daftar, panel detail, tab: `DEV_DISCRETION`. `AvailableActions` dari backend: wajib (`AGENTS.md`
"Alur Kerja dan Aturan Bisnis").
