# Inventarisasi Fisik — Arsitektur Frontend

| Field | Value |
|---|---|
| Sub-modul | `inventarisasi` — `AST-INV-1.0.0` `draft` |
| Route dasar | `src/app/corporate/asset-management/stock-take/` dan `/reports/stock-take` |
| Komponen baru | Pemindai QR `src/components/view/corporate/asset-management/qr-scanner.jsx` — memakai kamera browser; pilihan pustaka pemindai (pustaka terpasang `react-qr-scanner` masih alpha, `TS-18`) adalah keputusan teknis task frontend dan dicatat di laporannya |

## 1. Kebutuhan layar

| ID | Layar | Menu | Endpoint |
|---|---|---|---|
| `SCR-AST-INV-01` | Jadwal Inventaris | 7.1 | `GET/POST/PUT /physical-inventory/stock-takes` |
| `SCR-AST-INV-02` | Pemeriksaan Fisik (layar pemindaian, ramah HP/tablet) | 7.2 | `/{id}/start`, `/{id}/scans`, `/{id}/findings`, `/{id}/expected-items`, `GET /register/assets/by-number/{nomor}` |
| `SCR-AST-INV-03` | Pencocokan Data | 7.3 | `/{id}/close-counting`, `/{id}/reopen-counting`, `/{id}/results` |
| `SCR-AST-INV-04` | Selisih Inventaris (hasil bukan Sesuai + usulan) | 7.4 | `/{id}/results?category=…`, `/physical-inventory/follow-ups` |
| `SCR-AST-INV-05` | Verifikasi Hasil | 7.5 | `/{id}/submit`, `/{id}/verify` |
| `SCR-AST-INV-06` | Berita Acara | 7.6 | `GET /{id}/document` |
| `SCR-AST-INV-07` | Laporan Inventaris | 11.4 | `/physical-inventory/reports/stock-takes` |
| `SCR-AST-INV-08` | Detail Sesi (layar anak) | — | `GET /{id}` |

## 2. Peta butir menu

Peta seluruh modul: [`../02-module-map.md`](../02-module-map.md) 3.1. Butir milik sub-modul ini: grup
**Inventaris & Pemeriksaan Fisik** (6 butir) dan **Laporan Inventaris**. `SCR-AST-INV-08` dibuka dari baris
`SCR-AST-INV-01`, `07`; layar `02..06` menerima `?sessionId=` dan menampilkan pemilih sesi bila kosong.

## 3. Skema fitur

### 3.1 Pemeriksaan Fisik — `SCR-AST-INV-02`

```text
+- AST-OPN-000007 IGD - Pemindaian ---------------------- SCR-AST-INV-02 -+
| Dipindai 210 / 312     Ruangan saat ini [Resusitasi v]                 |
| +--------------------------------------------------------------------+ |
| |                 [ area kamera pemindai QR ]                        | |
| +--------------------------------------------------------------------+ |
| atau ketik nomor [AST-....-......] [Cari]                              |
| Hasil: AST-ALKMED-000123 Monitor Pasien  tercatat: Resusitasi, Aktif   |
| Kondisi saat ini (Aktif | Rusak Ringan | Rusak Berat | Tidak Digunakan) |
| [Foto opsional]  Catatan [      ]                    [Simpan pindaian] |
| [+ Temuan tidak terdaftar]        [Lihat yang belum dipindai (102)]    |
+-------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Penghitung | Diharapkan vs dipindai | `GET /{id}` | `AssetStockTake : Read` | — |
| Kamera | Membaca QR → teks nomor | Komponen pemindai (tanpa jaringan) | — | Izin kamera ditolak/HTTPS tidak ada → "Kamera tidak dapat dipakai. Ketik nomor aset." |
| Hasil cari | Data catatan aset | `GET /register/assets/by-number/{nomor}` | `AssetRegister : Read` | 404 → tombol "Catat sebagai temuan" |
| Simpan | Pindaian | `POST /{id}/scans` | `AssetStockTake : Scan` | 403 → "Anda bukan anggota tim sesi ini." |
| Temuan | Formulir keterangan + foto | `POST /{id}/findings` | `AssetStockTake : Scan` | — |
| Belum dipindai | Daftar | `GET /{id}/expected-items?scanned=false` | `AssetStockTake : Read` | "Semua aset sudah dipindai." |

Layar harus dapat dipakai satu tangan di HP (lebar ≥ 360 px). Tata letak rinci `DEV_DISCRETION`.

### 3.2 Pencocokan, Selisih, Verifikasi — `SCR-AST-INV-03..05`

Bentuk daftar bersama (`../register/03-frontend-architecture.md` 3.1) dengan kolom: Nomor, Nama,
Kategori (chip), Tercatat (ruangan, kondisi), Ditemukan (ruangan, kondisi), Catatan tim, Usulan.

| Tombol | Hak akses | Tampil bila |
|---|---|---|
| Tutup pemindaian / Buka kembali | `AssetStockTake : Close` | Ketua tim; status `Berjalan` / `Pencocokan` |
| Ubah catatan | `AssetStockTake : Close` | `Pencocokan` |
| Ajukan verifikasi | `AssetStockTake : Submit` | Ketua tim; `Pencocokan` |
| Verifikasi / Kembalikan | `AssetStockTake : Verify` | `MenungguVerifikasi`; pengguna bukan anggota tim |
| Proses usulan / Abaikan | `AssetStockTakeFollowUp : Process` / `Dismiss` | Usulan `Usulan` |

Kosong: `INV-04` "Tidak ada selisih pada sesi ini."

## 4. Aksi per peran

Dari [`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 1.

## 5. Data dan status

Label kategori dan status dari `asset-status-labels.js`. Angka penghitung dari backend, bukan hitungan
browser.

## 6. Penanganan keadaan

Sama dengan `register` bagian 6. Tambahan untuk pemindaian: simpan pindaian menonaktifkan tombol sampai
selesai; pindaian yang gagal terkirim tetap terlihat di layar dengan tombol "Kirim ulang" (tanpa
penyimpanan luring).

## 7. Kewenangan UI

| Keputusan | Owner | Status |
|---|---|---|
| Pemindaian lewat kamera browser + input manual cadangan | Rizki | `approved` (`AST-FE-003`) |
| Pustaka pemindai | Task frontend | `DEV_DISCRETION` dengan syarat: berjalan di Chrome/Safari HP, tanpa layanan luar |
| Tata letak | — | `DEV_DISCRETION` |
