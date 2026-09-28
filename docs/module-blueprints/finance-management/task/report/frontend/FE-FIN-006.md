# Laporan Perubahan Frontend: Task FE-FIN-006

## 1. Ringkasan Eksekutif
- **Task ID**: `FE-FIN-006`
- **Nama Modul**: `Finance Management`
- **Nama Task**: Pemantauan Fakta Billing Masuk & Antrean Kejadian Accounting
- **Tanggal Selesai**: 23 September 2026
- **Status Akhir**: ✅ **SELESAI (DONE)**
- **Wewenang**: Product Owner (`FIN-DEC-024`..`FIN-DEC-029`)
- **Kontrak Terkunci**: `FIN-API-1.0` (Billing Intake & Accounting Events), `FIN-PERM-1.0`
- **Repositori Target**: `QuilvianSystemFrontendDev`
- **Branch**: `yasmina`

---

## 2. Latar Belakang & Kebutuhan Bisnis
Dalam arsitektur modul keuangan terpadu Quilvian (`FIN-BP-001` Rev 2, `03-frontend-architecture.md` §3.5), terdapat dua aliran integrasi kritis yang memerlukan antarmuka pemantauan terpusat:
1. **Penerimaan Fakta Billing (Intake)** (`FR-FIN-011`, `FR-FIN-012`, `UAT-04`):
   Fakta piutang baru dari modul Billing ditampung dan diolah menjadi kartu piutang (AR). Petugas Finance memerlukan layar untuk melihat status pengolahan (`NEW`, `CONSUMED`, `ACKNOWLEDGED`, `ERROR`), memicu sinkronisasi fakta baru (`POST /billing-intake/sync`), dan melakukan pengulangan proses untuk baris yang gagal diolah (`POST /billing-intake/{id}/process`).
2. **Antrean Kejadian Subledger ke Accounting (Outbox)** (`FR-FIN-074`, `UAT-17`..`UAT-19`):
   Setiap transaksi bernilai finansial di Finance (penerbitan piutang, pelunasan kasir, setoran bank, koreksi, dan penghapusan) memicu event ke Accounting Outbox. Petugas memerlukan visibilitas atas antrean kejadian, muatan JSON data (yang dijamin steril dari data pasien), dan riwayat percobaan pengiriman (`attempts`).

### Invariant Kunci yang Dipatuhi:
- **Tanpa Tombol Kirim (`EPIC FIN-12`)**: Layar antrean kejadian murni berupa pemantauan baca-saja (`GET`). Tidak ada tombol kirim/publish manual karena pengiriman diatur oleh mekanisme integrasi Accounting.
- **Pembedaan Tegas Status Tertahan**: Membedakan secara visual dan semantik antara `HELD_FOR_FINALIZATION` (menunggu finalisasi tagihan di Billing), `HELD` (tertahan menunggu pembukaan periode akuntansi), dan `FAILED` (gagal kirim teknis), karena ketiga status tersebut menuntut tindakan tindak lanjut pengguna yang berbeda.
- **Nol Perhitungan Klien**: Nilai uang, status, dan metrik ringkasan dibaca apa adanya dari response backend tanpa perhitungan atau pembulatan di sisi klien.

---

## 3. Komponen dan Berkas yang Dibuat / Diubah

| No | Tipe | Berkas | Deskripsi |
|---|---|---|---|
| 1 | **Constants** | `src/lib/constants/finance/monitoring/monitoring-constants.jsx` | Konfigurasi endpoint, opsi status, warna/badge, opsi filter tipe kejadian, dan deskripsi petunjuk tindakan status. |
| 2 | **Utils** | `src/utils/finance/monitoring/monitoring-utils.jsx` | Helper formatting uang IDR, tanggal-waktu, pretty print JSON payload/komponen, dan metadata status. |
| 3 | **Redux Slice** | `src/lib/state/slice/finance/monitoring/finance-monitoring-slice.jsx` | Thunks untuk Billing Intake (metadata, summary, list, detail, sync, process) dan Accounting Events (metadata, summary, list, detail), state filter, pagination, dan toast. |
| 4 | **Store** | `src/lib/state/store.jsx` | Pendaftaran `financeMonitoring: financeMonitoringReducer` ke configureStore. |
| 5 | **Custom Hooks** | `src/lib/hooks/finance/monitoring/use-finance-billing-intake.jsx` | Hook terenkapsulasi untuk data fetching, filter, paginasi, sinkronisasi, dan pengolahan ulang baris fakta masuk. |
| 6 | **Custom Hooks** | `src/lib/hooks/finance/monitoring/use-finance-accounting-events.jsx` | Hook terenkapsulasi untuk pemantauan antrean kejadian akuntansi outbox baca-saja. |
| 7 | **Table Columns** | `src/components/view/finance/monitoring/billing-intake/billing-intake-table-columns.jsx` | Kolom tabel fakta billing, badge status, retry count, pesan galat, dan tombol aksi (Rincian, Proses/Ulangi). |
| 8 | **Modal** | `src/components/view/finance/monitoring/billing-intake/billing-intake-detail-modal.jsx` | Modal dialog rincian fakta masuk, korelasi ID, entitas piutang terbentuk, pesan error, dan pemicu proses langsung. |
| 9 | **Table Columns** | `src/components/view/finance/monitoring/accounting-events/accounting-events-table-columns.jsx` | Kolom tabel kejadian akuntansi dengan badge status tegas membedakan `HELD_FOR_FINALIZATION`, `HELD`, dan `FAILED`. |
| 10 | **Modal** | `src/components/view/finance/monitoring/accounting-events/accounting-event-detail-modal.jsx` | Modal rincian kejadian akuntansi, JSON Viewer untuk `PayloadJson` & `ComponentsJson`, dan tabel riwayat percobaan kirim (`attempts`). |
| 11 | **View** | `src/components/view/finance/monitoring/finance-monitoring-view.jsx` | Tampilan utama pemantauan terpadu dengan navigasi tab "Fakta Billing Masuk" & "Antrean Kejadian Accounting", hero, summary cards, filter, table, pagination, dan toast stack. |
| 12 | **Client Route** | `src/app/finance/monitoring/monitoring-client.jsx` | Client component Next.js App Router. |
| 13 | **Server Route** | `src/app/finance/monitoring/page.jsx` | Server page Next.js dengan metadata SEO. |
| 14 | **Menu Sidebar** | `src/utils/menu-sidebar/menu-items.jsx` | Penambahan butir menu "Pemantauan Finance" dengan icon `RiHistoryLine` di bawah grup menu `corporateFinance`. |

---

## 4. Gerbang Keputusan Base Component (`UI Gate`)

Sesuai langkah 3 `SKILL.md` (Base Component Decision Gate):

| Komponen / Elemen UI | Sumber Base Component | Status | Rekomendasi & Catatan |
|---|---|---|---|
| Header & Hero | `src/components/features/base-features/hero.jsx` | `REUSE` | Menggunakan Hero standar dengan tombol aksi sinkronisasi fakta. |
| Summary Metric Cards | `src/components/features/base-features/summary-grid.jsx` | `REUSE` | Kartu metrik ringkasan dengan tone visual yang sesuai status. |
| Filter Data | `src/components/features/base-features/data-filter.jsx` | `REUSE` | Filter bar terintegrasi dengan tombol reset. |
| Dropdown Select | `src/components/features/base-features/filter-select.jsx` | `REUSE` | Pilihan status dan ukuran halaman. |
| Data Table | `src/components/features/base-features/data-table.jsx` | `REUSE` | Tabel data terstruktur dengan loading & empty state. |
| Paginasi Halaman | `src/components/features/pagination/pagination.jsx` | `REUSE` | Navigasi halaman, batas rekaman, dan ukuran halaman. |
| Toast Feedback | `src/components/features/base-features/toast-stack.jsx` | `REUSE` | Penampil umpan balik pesan sukses/galat dari Redux. |
| Modal Detail Dialog | Standard accessible dialog overlay | `REUSE` | Mengikuti pola modal detail yang konsisten di modul Finance. |

---

## 5. Bukti Validasi

### 5.1 Validasi Linter
- **Perintah**: `npm run lint:errors`
- **Direktori**: `QuilvianSystemFrontendDev`
- **Hasil**: **PASS (0 errors, 0 warnings)**

### 5.2 Validasi Production Build
- **Perintah**: `npm run build`
- **Direktori**: `QuilvianSystemFrontendDev`
- **Hasil**: **PASS (Exit code 0)**
  - Halaman `/finance/monitoring` berhasil dikompilasi dan digenerasi ke static/client pages bundle.
  - Skrip standalone runtime `prepare-standalone.mjs` berhasil dijalankan.

### 5.3 Verifikasi Kepatuhan Acceptance Criteria
1. **Pemantauan Fakta Billing (`UAT-04`)**:
   - Menampilkan daftar fakta masuk dengan kolom status, percobaan, dan pesan galat.
   - Tombol "Sinkronkan Fakta Billing" memicu `POST /billing-intake/sync`.
   - Tombol "Proses / Ulangi" memicu `POST /billing-intake/{id}/process` untuk baris `ERROR` atau `NEW`.
2. **Pemantauan Antrean Kejadian Accounting (`UAT-17`..`UAT-19`)**:
   - Pembedaan status tertahan:
     - `HELD_FOR_FINALIZATION`: Tone warna peringatan (Amber), label "Tertahan: Finalisasi Billing", petunjuk tindakan koordinasi dengan staf Billing.
     - `HELD`: Tone warna sekunder (Slate), label "Tertahan: Periode Akuntansi", petunjuk tindakan menunggu pembukaan periode jurnal.
     - `FAILED`: Tone warna bahaya (Merah), label "Gagal Kirim", petunjuk tindakan investigasi teknis/finance.
   - Tanpa tombol kirim: Tidak ada tombol submit/publish ke Accounting (`EPIC FIN-12`).
   - Modal detail menyajikan muatan JSON rapi dan riwayat percobaan pengiriman (`attempts`).
3. **Navigasi & Menu (`FIN-DEC-024`, `FIN-DEC-025`)**:
   - Rute aktif di `/finance/monitoring`.
   - Menu "Pemantauan Finance" terpasang di bawah grup `corporateFinance`.

---

## 6. Kesimpulan & Penandaan Roadmap
Seluruh kriteria penerimaan untuk `FE-FIN-006` telah dipenuhi dengan bukti build dan linter yang lulus tanpa error. Task ini siap ditandai sebagai `✅ Selesai (23 September 2026)` pada roadmap.
