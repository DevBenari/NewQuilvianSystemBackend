# Nilai Aset — Arsitektur Frontend

| Field | Value |
|---|---|
| Sub-modul | `nilai` — `AST-NLI-1.0.0` `draft` |
| Route dasar | `src/app/corporate/asset-management/valuation/` dan `/reports/valuation` |
| Komponen | Komponen dasar `register`; format rupiah memakai komponen `src/components/ui/currency/` yang sudah ada |

Seluruh layar sub-modul ini memuat rupiah dan hanya terjangkau peran ber-hak nilai.

## 1. Kebutuhan layar

| ID | Layar | Menu | Endpoint |
|---|---|---|---|
| `SCR-AST-NLI-01` | Harga Perolehan | 9.1 | `GET /valuation/books/acquisition-costs` |
| `SCR-AST-NLI-02` | Nilai Buku | 9.2 | `GET /valuation/books`, `/{assetId}` |
| `SCR-AST-NLI-03` | Riwayat Perubahan Nilai (pengajuan + entri) | 9.3 | `/valuation/value-changes`, `GET /valuation/books/{assetId}/entries` |
| `SCR-AST-NLI-04` | Data Penyusutan (daftar proses, pratinjau, proses) | 9.4 | `/valuation/depreciation-runs`, `/preview` |
| `SCR-AST-NLI-05` | Rekonsiliasi Aset dengan Akuntansi | 9.5 | `/valuation/reconciliation`, `/valuation/accounting-outbox` |
| `SCR-AST-NLI-06` | Parameter Nilai (parameter, kebijakan kelompok, akun kontrol, pusat biaya) | tambahan | 2.1–2.4 |
| `SCR-AST-NLI-07` | Laporan Nilai Aset | 11.6 | `/valuation/reports/valuation`, `/export` |
| `SCR-AST-NLI-08` | Detail Proses Penyusutan (layar anak) | — | `GET /depreciation-runs/{id}`, `/lines` |

## 2. Peta butir menu

Peta seluruh modul: [`../02-module-map.md`](../02-module-map.md) 3.1. Butir milik sub-modul ini: grup
**Nilai Aset & Rekonsiliasi** (6 butir) dan **Laporan Nilai Aset**. `SCR-AST-NLI-08` dibuka dari baris
`SCR-AST-NLI-04`. Bagian "Nilai" pada Detail Aset (`SCR-AST-REG-17`) menautkan ke `SCR-AST-NLI-02`/`03`.

## 3. Skema fitur

### 3.1 Data Penyusutan — `SCR-AST-NLI-04`

```text
+- Data Penyusutan ------------------------------------------------ SCR-AST-NLI-04 -+
| Periode berikut: Mei 2026                [Buka pratinjau]                        |
+-----------------------------------------------------------------------------------+
| Pratinjau Mei 2026: 1.200 aset · Rp 187.450.000 · penyesuaian Rp 2.700.000        |
| Galat (0)   Peringatan (1): kelompok TI tanpa akun kontrol                        |
| Kelompok | Pusat biaya | Jumlah aset | Penyusutan | Penyesuaian                   |
|                                                       [Proses Mei 2026]           |
+-----------------------------------------------------------------------------------+
| Riwayat: Periode | Aset | Total | Diproses oleh/pada | Status kirim [⋮ detail]    |
+-----------------------------------------------------------------------------------+
```

| Wilayah | Sumber | Hak akses | Kosong / gagal |
|---|---|---|---|
| Pratinjau | `GET /preview?period=` | `AssetDepreciationRun : Read` | Galat → tombol Proses nonaktif; daftar galat dengan tautan ke `SCR-AST-NLI-06` |
| Proses | `POST /depreciation-runs` | `AssetDepreciationRun : Process` | 409 → pesan urutan atau sudah diproses |
| Riwayat | `GET /depreciation-runs` | `AssetDepreciationRun : Read` | "Belum ada penyusutan yang diproses." |
| Status kirim | Ringkasan kotak keluar per proses | `AssetAccountingOutbox : Read` | Ditampilkan "Ditahan" bila kode belum dinyalakan |

Konfirmasi sebelum memproses memakai `confirm-modal.jsx` dan menyebut periode serta total.

### 3.2 Rekonsiliasi — `SCR-AST-NLI-05`

Tabel per akun kontrol: kode akun, saldo menurut Aset, status kirim, versi; tombol **[Kirim ulang]**
(`AssetReconciliation : Send`) dan tautan **[Buka rekonsiliasi di Akuntansi]** (`/corporate/accounting/…`,
terlihat bila pengguna punya hak Akuntansi). Di bawahnya daftar kotak keluar bersaringan status dengan
tombol **[Coba ulang]** (`AssetAccountingOutbox : Retry`) pada baris `Failed`.

### 3.3 Parameter Nilai — `SCR-AST-NLI-06`

Empat bagian: (1) parameter berlaku + yang akan berlaku + riwayat; (2) kebijakan per kelompok; (3) akun
kontrol per kelompok; (4) pusat biaya per unit, dengan saringan "belum jelas". Setiap penyimpanan menampilkan
"Berlaku mulai {bulan}". Hak akses `: Update` resource masing-masing; yang lain hanya lihat.

### 3.4 Layar daftar lain (`01`, `02`, `03`, `07`)

Bentuk daftar bersama `register` dengan kolom rupiah. `SCR-AST-NLI-03` memuat tombol Ajukan
(`AssetValueChange : Create`), Setujui/Tolak (`Approve`, disembunyikan bagi pengaju), Batalkan.

## 4. Aksi per peran

Dari [`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 1.

## 5. Data dan status

Rupiah diformat komponen mata uang yang ada; angka berasal dari backend tanpa perhitungan ulang di browser.

## 6. Penanganan keadaan

Sama dengan `register` bagian 6. Proses penyusutan: tombol nonaktif selama permintaan; setelah berhasil,
pratinjau berpindah ke periode berikutnya.

## 7. Kewenangan UI

Tata letak, grafik nilai: `DEV_DISCRETION`. Rupiah hanya untuk peran ber-hak: wajib (`AST-FE-004`).
