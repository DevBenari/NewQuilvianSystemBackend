# Laporan Perubahan Frontend — `FE-RHB-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-009` |
| Judul | Status dan ulang handoff billing pada sesi terapi (bagian billing FE-RHB-10/11) |
| Slice | FS-D — Sesi dan billing |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-009` (r2) |
| Trace | FR-RHB-033..035; FEAT-032; RHB-DEC-021; UAT-08, UAT-11 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05. API: *Rehab Billing Handoff* (`BE-RHB-013` ✅) |
| Dependency | `FE-RHB-008` ✅, `BE-RHB-013` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). ESLint: 0 error, 0 warning. Uji browser + jaringan Playwright: 9/9, ditambah 1/1 sesudah perubahan kolom. Cek invoice di DB (READ ONLY): 1 fakta + 1 charge line per tindakan, tersinkron ke invoice. |

## 1. Masalah yang diselesaikan

Sejak `BE-RHB-013`, tindakan sesi final otomatis dikirim ke Billing. Petugas belum bisa melihat dari
layar apakah pengiriman berhasil, dan belum bisa mengulangnya bila gagal, tertunda, atau bila sesi
ditandatangani sebelum fitur ini ada.

**Contoh.** Kasir membuka sesi `RHB-SES-2026-000010`, yang ditandatangani sebelum penyerahan otomatis
tersedia. Bagian *Penyerahan ke Billing* menampilkan *Billing: Tidak Ditagihkan*, tindakan ALAT_1970
*Belum dibentuk*, dan petunjuk untuk menekan *Ulang*. Ia menekan *Ulang Penyerahan* (tidak sengaja dua
kali). Hanya satu permintaan terkirim, badge berubah menjadi *Billing: Terkirim*, tindakan *Terbentuk*,
dan di Billing terbentuk satu item tagihan saja.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Kartu roadmap: kolom/saringan status billing di FE-RHB-10 dan bagian billing di FE-RHB-11 (status per intervensi, galat, tombol Ulang untuk `RehabBillingHandoff:Retry`). Kolom dan saringan daftar sudah dibangun `FE-RHB-008`; task ini menambah bagian di FE-RHB-11. Letak bagian (di rincian sesi final) adalah `DEV_DISCRETION`. |
| Izin owner | "saya approve dan lanjutkan" (2026-11-09): ESLint dan uji browser (akun uji dinonaktifkan sesudahnya). Tanpa build, tanpa commit. |
| Keputusan UI | Tidak ada elemen `NEW` atau `EXTEND` (§3.2) |
| Server uji | Backend dijalankan task ini (build `BE-RHB-013`). `next dev` lama sudah berhenti, sehingga dijalankan ulang task ini. Keduanya dihentikan sesudah uji. |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

Semua jalur relatif terhadap `src/`.

| Berkas | Status | Isi |
| --- | --- | --- |
| `lib/services/health-services/rehabilitation-management/rehab-billing-handoff.service.js` | Baru | `GET /{id}/billing-handoff`, `POST /{id}/billing-handoff/retry` |
| `lib/hooks/.../sessions/use-rehab-session-billing.js` | Baru | Muat status (hanya bila diizinkan dan sesi final), `canRetry` (*Menunggu*, *Gagal*, atau sesi lama yang belum bertindakan), retry dengan penjaga sinkron (`inFlightRef`) |
| `components/features/.../sessions/rehab-session-fields.jsx` | Diperbarui | `RehabSessionBillingSection`: badge status, waktu kirim, galat, petunjuk backfill, tabel per intervensi (tindakan, jumlah, tindakan bertarif *Terbentuk*/*Belum dibentuk*), tombol *Ulang Penyerahan* |
| `components/view/.../sessions/rehab-session-view.jsx` | Diperbarui | Bagian *Penyerahan ke Billing* di rincian sesi final. Tampil bila `RehabBillingHandoff : Read`; tombol hanya bila `RehabBillingHandoff : Retry`. Sesudah ulang, rincian sesi dimuat ulang supaya badge di kepala ikut berubah. |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Badge status billing | `REUSE` | `StatusBadge` (nada *Gagal* = danger, dari `FE-RHB-008`) |
| 2 | Galat dan petunjuk | `REUSE` | `InformationAlert` |
| 3 | Rincian per intervensi | `REUSE` | `DataTable` |
| 4 | Tombol Ulang (nonaktif + "Mengirim..." selama permintaan) | `REUSE` | `BaseButton` (`loading`) |
| 5 | Bagian billing di rincian sesi | `COMPOSE` | `BaseDetailSection` + elemen 1–4 |

**Pilihan untuk elemen `COMPOSE` (direkomendasikan dan dipakai):**

1. **Rakit di rincian sesi dengan base component** *(direkomendasikan)*. Konsisten dengan bagian
   rincian lain, tanpa regresi.
2. Halaman atau modal billing tersendiri. Memisahkan peran kasir, tetapi menambah rute dan memisahkan
   konteks sesi.

`UI GATE: PASS — 0 NEW, 0 EXTEND, 0 WRAP, 1 COMPOSE (opsi 1).`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| Keadaan *Gagal* pada data nyata | Billing dev menerima tindakan tanpa tarif (delta `BE-RHB-013`), sehingga *Gagal* tidak muncul alami. Uji AC2 meniru *Gagal* pada **GET pertama** (respons server asli dengan status diganti). `POST retry` dan GET sesudahnya memakai server sungguhan. |
| Tombol Ulang juga untuk backfill | Ditampilkan pula untuk sesi final lama yang berstatus *Tidak ditagihkan* tetapi intervensinya belum bertindakan (AC4 `BE-RHB-013`) |
| Kolom "item tagihan" tidak ditampilkan | Penanda `IsBillingGenerated` milik jalur lama tidak diperbarui jalur fakta klinis, sehingga akan tampil "-" walau item sudah masuk invoice. Ditemukan saat uji dan dihapus; status invoice dibaca di modul Billing. |
| Hak akses | Bagian billing tidak memanggil API sama sekali tanpa `RehabBillingHandoff : Read` |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas folder rehab `view/sessions`, `features/sessions`, `hooks/sessions`, `services` | 0 error, 0 warning (diulang sesudah penghapusan kolom) |
| Grep anti-regresi UI (hex/`rgba`, `style={{`, `<button>`/`<table>` mentah) | 0. Tidak ada CSS baru. |
| Grep privasi (`console`, `localStorage`) | 0 |
| Review diff | 2 berkas baru, 2 berkas sesi `FE-RHB-008` diperbarui. Base component tidak berubah. |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser, jaringan, dan invoice (MANUAL TEST)

**Lingkungan (2026-11-09):**
- backend Debug (build `BE-RHB-013`) di `https://localhost:7184` dengan DB dev `QuilvianNewDevSukma`;
- `next dev` di port 3000;
- Playwright Chromium.

**Akun uji** berperan "kasir": `RehabSession : Read`, `RehabEpisode : Read`, `RehabBillingHandoff` (Read + Retry), tanpa hak mengubah sesi.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Setup: `RHB-SES-2026-000013` (UAT-08 `BE-RHB-013`) *Terkirim*; `RHB-SES-2026-000010` (final sebelum `BE-RHB-013`) *Tidak ditagihkan*, intervensi belum bertindakan | PASS |
| 2 | Daftar: saringan *Status Billing* = *Terkirim* memuat `000013`, semua baris berbadge *Terkirim* | PASS |
| 3 | **AC1 / UAT-08**: rincian `000013` → *Billing: Terkirim*, 2 tindakan *Terbentuk*, tanpa tombol Ulang | PASS |
| 4 | **UAT-11**: rincian `000010` dengan GET pertama ditiru *Gagal* → badge *Billing: Gagal*, pesan galat tanpa data klinis, tombol *Ulang Penyerahan* | PASS |
| 5 | **AC2**: klik ganda *Ulang Penyerahan* → hanya **1** `POST retry` | PASS |
| 6 | **AC2 / UAT-11**: setelah Ulang → *Billing: Terkirim*, tindakan *Terbentuk*, tombol hilang; server: *Terkirim* | PASS ×2 |
| 7 | `RehabBillingHandoff : Read` tanpa `Retry` → bagian tampil, tombol tidak tampil walau *Gagal* | PASS |
| 8 | Tanpa `RehabBillingHandoff` → bagian tidak tampil, 0 permintaan billing-handoff | PASS |
| 9 | Sesudah penghapusan kolom "item tagihan": bagian tetap tampil (3 kolom, *Terkirim*) | PASS |

**Cek invoice (DB, READ ONLY):** `000010` (ALAT_1970) dan `000013` (ALAT_1970, ALAT_1971) masing-masing
memiliki tepat 1 `CliClinicalMilestoneFact` dan 1 `BilChargeLine`, dengan `InvoiceSyncStatus = Synced`.
Klik ganda tidak menghasilkan tagihan ganda.

**Pembersihan.** Dicek di DB dev dengan kueri READ ONLY:
- 0 dari 42 akun `rhb-uji-*` yang masih aktif;
- kebijakan pasangan uji: 0 baris.

Backend dan `next dev` dihentikan.

**Residu data dev:** sesi lama `RHB-SES-2026-000010` (episode `RHB-EPS-2026-000006`) kini tertagih
lewat Ulang.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-08: status `Succeeded` tampil | Terpenuhi | §5 #3 |
| (2) UAT-11: `Failed` → Ulang → `Succeeded`, klik ganda tidak mengirim dua kali | Terpenuhi | §5 #4–6 (keadaan *Gagal* ditiru pada GET pertama, lihat §3.3) + cek invoice |
| Verifikasi: browser + cek invoice | Terpenuhi | §5 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Keadaan *Gagal* nyata | Baru teruji dengan tiruan; perilaku `retry` dari *Gagal* sama dengan dari *Menunggu* di server |
| Item tagihan di layar rehab | Tidak ditampilkan (lihat §3.3). Bila owner ingin status invoice tampil di layar sesi, perlu endpoint baca status sinkronisasi dari Billing (perubahan di luar Rehab). |
| Dependency backend | Tidak ada yang baru. Dengan task ini, seluruh task Rehab Medik Phase 1 selesai. |
