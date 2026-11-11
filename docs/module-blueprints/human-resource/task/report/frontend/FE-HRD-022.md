# Laporan Perubahan Frontend — `FE-HRD-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-022` |
| Judul | Service API Axios dan Redux Slice untuk Dokumen SK Kepegawaian (Personnel Action Order) |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-048`, `HRD-DEC-050`, `HRD-DEC-056`, `HRD-DEC-057`, `BE-HRD-024`, `BE-HRD-025` |
| Contract version | Personnel Action Order API v1 (`HrdPersonnelActionOrderController.cs`) |
| Dependency | `BE-HRD-025` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/services/hr/personnel-action-order-service.js`, `src/lib/state/slice/hr/workforce-core/personnel-action-order-slice.js`, `src/lib/state/store.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Untuk memfasilitasi antarmuka Surat Keputusan (SK) Kepegawaian pada rumah sakit, modul frontend membutuhkan jembatan komunikasi data terstruktur (Service API) dan manajemen state terpusat (Redux Toolkit).

Task ini mengimplementasikan:
1. **`PersonnelActionOrderService`**: Service Axios yang terhubung langsung ke controller backend `/v1/corporate/human-resource/personnel-action-orders` untuk operasi pengambilan daftar SK, pembacaan detail, pembuatan draf, pembaruan, pengajuan persetujuan, persetujuan, penolakan, pembatalan, dan unggah berkas fisik hasil pindai (*scan*).
2. **`personnelActionOrderSlice`**: Redux slice dengan thunk asinkron terkelola yang menangani status *loading*, pesan *error* ramah pengguna, data *caching*, dan didaftarkan resmi pada root Redux store.

---

## 1. Berkas yang Dibuat dan Diperbarui

1. `src/lib/services/hr/personnel-action-order-service.js` — Service Axios lengkap dengan 9 fungsi API.
2. `src/lib/state/slice/hr/workforce-core/personnel-action-order-slice.js` — Redux slice dan 9 thunk terstruktur.
3. `src/lib/state/store.jsx` — Pendaftaran reducer `personnelActionOrder` ke store utama.

---

## 2. Bukti Pengujian Unit

```text
✔ FE-HRD-022: PersonnelActionOrderService memanggil endpoint canonical backend tanpa /v1/hr/ (1.1391ms)
✔ FE-HRD-022: Redux slice SK Kepegawaian terdaftar di store (0.43ms)
```
Status: **PASS (100%)**

---

## Pengerjaan Ulang — `PLAN-REPAIR-HRD-002` (3 November 2026)

| Field | Nilai |
| --- | --- |
| Pemicu | Laporan pemilik 03-11-2026 dan laporan live testing `LBT-WFC-19`, `-20`, `-22`, `-23`, `-25` |
| Issue / rencana | [issue-002](../../../docs/issue/issue-002-tampilan-workforce-core-tanpa-gaya.md) · [plan-repair-002](../../../docs/plan-repair/plan-repair-002-tampilan-workforce-core-tanpa-gaya.md) — `FIX-HRD-002-01`, `-02`, `-08` |
| Cakupan | Daftar SK (`/hr/workforce-core/personnel-actions`, juga alias `/hr/personnel-action-orders` milik `FE-HRD-023`) dan menu sidebar |
| Branch frontend | `HamzahV2`, basis `66b7e5600`; perubahan belum di-commit |

### Keadaan yang ditemukan

Saringan Status dan Kategori tidak pernah tampil karena `DataFilter` menerima `searchTerm` dan
`filterControls` yang bukan prop-nya (yang benar `searchKeyword` dan `children`). Kartu ringkasan
dihitung dari halaman yang sedang tampil, bukan `GET …/summary`. Lencana status dikirim dengan label
sebagai `status`, tombol "Lihat Rincian" memakai `variant="outline"` yang tidak dikenal, dan pembungkus
memakai kunci CSS Module `container`, `themeContainer`, dan `tableWrapper` yang tidak ada. Menu Dokumen
SK dan Batch Data Awal tidak ada di sidebar walaupun tercantum di tabel menu
`03-frontend-architecture.md`.

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/workforce-core/personnel-action-order-list-view.jsx` | Shell standar, `AccessDeniedGate`, saringan sebagai `children`, ringkasan dari `/summary`, tombol Buat SK di `Hero actions` |
| `src/components/view/hr/workforce-core/personnel-action-order-table-columns.jsx` (baru) | Kolom tabel terpisah dengan kelas sel standar dan `StatusBadge status/label` |
| `src/lib/services/hr/personnel-action-order-service.js` | `getSummary` |
| `src/utils/menu-sidebar/menu-items.jsx` | Menu "Dokumen SK Kepegawaian" dan "Batch Data Awal" |
| `src/utils/hr/workforce-core/workforce-core-display-utils.js`, `src/style/hr/workforce-core/workforce-core.module.css` (baru) | Utilitas dan CSS Module domain bersama |

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/workforce-core/workforce-core-cross-employee-list-view.jsx`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header + tombol Buat SK | `Hero` (`actions`) | `REUSE` | — |
| Kartu ringkasan | `SummaryGrid` | `REUSE` | Angka dari `/summary` |
| Cari + saringan | `DataFilter` + `FilterSelect` sebagai `children` | `REUSE` | Prop diperbaiki |
| Tabel | `DataTable` + `RegionPagination` + kelas sel `administrator-region-table.module.css` | `REUSE` | — |
| Lencana status & dokumen fisik | `StatusBadge` | `REUSE` | — |
| Galat / akses ditolak | `InformationAlert`, `AccessDeniedGate` | `REUSE` | — |

`UI GATE: PASS — 6 elemen REUSE, 0 NEW.`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view Administrasi Kepegawaian>
OK — 7 berkas diperiksa, 0 temuan.
$ grep anti-regresi #1, #3–#8, #10 (checklist G)  →  0 baris
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Daftar SK | `01-daftar-sk.png`, `02-daftar-sk-saring-dibatalkan.png`, ponsel `20-ponsel-daftar-sk.png` | Setara layar rekap; 390 px tanpa geser horizontal |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` scope Administrasi Kepegawaian, service, menu | 0 error | `PASS` |
| Test unit `workforce-core-*` | 22/22 lulus | `PASS` |
| Test unit sidebar (`master-data-sidebar-grouping`, `hemodialysis-sidebar-navigation`) | 6 gagal; juga gagal dengan `menu-items.jsx` versi HEAD | `EXISTING` |
| Seluruh test unit frontend | 4.126 lulus, 55 gagal; seluruh 55 sudah gagal pada HEAD | `EXISTING` |
| `npm run build` | "Compiled successfully" | `PASS` |
| Saring Status = Dibatalkan | Permintaan `orderStatus=Cancelled`, tabel tersaring | `PASS` |
| Menu sidebar | "Dokumen SK Kepegawaian" menyala pada daftar dan rincian SK | `PASS` |

Uji manual: `PASS` — Chrome headed pada build produksi port 3001. Dev server port 3000 gagal
membangkitkan path dinamis karena pool worker-nya mati setelah berjalan lama; bukan dari kode,
dibuktikan `npm run build` lulus. Screenshot ada di
`QuilvianSystemFrontendDev/test-with-agy/screenshots/workforce-core-1103/plan-repair-002/`.

### Status task

✅ tetap berlaku; seluruh acceptance criteria daftar SK kini terbukti di layar.
