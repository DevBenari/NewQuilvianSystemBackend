# Laporan Perubahan Frontend — `FE-BKC-043`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-043` |
| Judul | Badge Status `PERLU_TINDAK_LANJUT`, Opsi Saringan Riwayat, & Penanganan Pesan Blokir Buka Shift |
| Slice | Gelombang `MVP-34` — Shift Kasir: Penanganan Status Tindak Lanjut dan Resolusi Selisih (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `Gelombang MVP-34`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BKC-043` |
| Trace | `BKC-DEC-123`, `BKC-DEC-124`, `BKC-DEC-126`, `RULE-012`, `RULE-013` (dokumen `Shift Kasir (3).md`) |
| Contract version | `BIL-API-1.6` (draft) |
| Wewenang UI | Label status `"Perlu Tindak Lanjut"` dan pesan error terikat keputusan bisnis (`BKC-DEC-124`, `BKC-DEC-126`). Penempatan alert peringatan mengikuti `DEV_DISCRETION` |
| Dependency | `[BE] BE-BKC-077` — Selesai pada kode backend (blocking pembukaan shift baru untuk kasir/register ber-selisih kas). Bukti: [BE-BKC-077.md](../backend/BE-BKC-077.md) |
| Klasifikasi | `LIGHT` — antarmuka shift kasir; 3 berkas diubah, 1 unit test baru; nol migration; nol arsitektur state baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: `cashier-shift-constants.js`, `use-cashier-shift.js`, `open-shift-modal.jsx`, `tests/unit/cashier-shift-blocking-and-status.test.mjs` |
| Tanggal | 2026-09-25 |
| Status | 🟡 **SEBAGIAN — source code dan validasi unit test selesai 100% (4/4 tests PASS).** Badge status "Perlu Tindak Lanjut", opsi saringan riwayat shift, pemetaan error HTTP 409 di hook `useCashierShift`, dan rendering komponen `InformationAlert` pada modal buka shift telah terpasang. Menunggu verifikasi visual interaktif bersama backend aktif. |

---

## 1. Keadaan Awal Sebelum Perubahan

1. **Kosakata Status Kasir Belum Lengkap:**
   Konfigurasi lencana `CASHIER_SHIFT_STATUS_BADGE_CONFIG` dan opsi saringan `CASHIER_SHIFT_STATUS_OPTIONS` di `cashier-shift-constants.js` hanya mengenal 6 status awal: `OPEN`, `HANDED_OVER`, `CLOSED`, `CLOSED_WITH_VARIANCE`, `REVIEWED`, dan `REOPENED`. Status baru `PERLU_TINDAK_LANJUT` per `BKC-DEC-124` belum terdaftar, sehingga bila data riwayat shift mengembalikan status tersebut, badge berisiko tampil tanpa label baku atau jatuh ke fallback.
2. **Penyampaian Error Pembukaan Shift Tertahan Masih Kurang Informatif di Modal:**
   Pada hook `useCashierShift.js` fungsi `confirmOpenShift`, saat server menolak pembukaan shift dengan HTTP 409 Conflict (*"Kasir atau register masih memiliki shift yang menunggu review selisih kas."* per `BKC-DEC-123`), error hanya diteruskan ke fungsi umum `handleActionError` yang melempar toast notification.
3. **Ketiadaan Banner Peringatan pada Modal:**
   Komponen `OpenShiftModal.jsx` belum merender alert penolakan secara kontekstual di dalam badan modal. Kasir yang fokus mengisi field modal berpotensi bingung jika modal tidak memberikan feedback visual langsung di dekat area input form.

---

## 2. Proses Bisnis dari Sisi Pengguna (UX Rumah Sakit)

### Skenario 1: Kasir Tertahan Blokir Selisih Kas Belum Direview (`BKC-DEC-123`)
1. Kasir membuka halaman Shift Kasir (`/health-services/billing-management/cashier/shifts`).
2. Kasir menekan tombol **"+ Buka Shift Kasir"**.
3. Kasir memilih register (loket) dan memasukkan kas pembukaan (misal Rp100.000), lalu menekan tombol **"Buka Shift"**.
4. Jika kasir atau loket tersebut sebelumnya menutup shift dengan selisih kas yang belum tuntas direview oleh supervisor (status `CLOSED_WITH_VARIANCE` atau `PERLU_TINDAK_LANJUT`), backend mengembalikan HTTP 409 Conflict:
   > *"Kasir atau register masih memiliki shift yang menunggu review selisih kas."*
5. **Respons UI Baru:**
   - Modal Buka Shift **tidak** tertutup secara tiba-tiba.
   - Di bagian atas badan modal muncul banner peringatan kuning `InformationAlert` (variant `warning`) dengan pesan yang persis dari backend.
   - Kasir memahami bahwa loket atau dirinya harus menyelesaikan review selisih kas shift terdahulu bersama Kepala Kasir sebelum shift baru diizinkan dibuka.

### Skenario 2: Inspeksi Riwayat Shift Berstatus `PERLU_TINDAK_LANJUT` (`BKC-DEC-124`)
1. Supervisor/Kepala Kasir membuka tab **"Riwayat Shift"**.
2. Pada dropdown saringan status, supervisor kini melihat opsi baru: **"Perlu Tindak Lanjut"**.
3. Memilih opsi tersebut memfilter tabel hanya untuk shift yang memerlukan tindak lanjut investigasi selisih kas.
4. Pada kolom "Shift & Status", baris shift menampilkan badge berlabel jelas **"Perlu Tindak Lanjut"** dengan warna peringatan (*warning* / amber) yang konsisten dengan desain Quilvian.

---

## 3. Gerbang Keputusan Base Component (UI Gate)

| Elemen UI | Sumber Komponen | Keputusan | Bukti Pemakaian & Justifikasi |
| --- | --- | :---: | --- |
| Lencana Status Shift | `@/components/features/base-features/status-badge` | `REUSE` | Digunakan di `cashier-shift-history.jsx`, `cashier-shift-view.jsx`, dan `other-shift-preview.jsx` dengan konfigurasi badge `CASHIER_SHIFT_STATUS_BADGE_CONFIG`. |
| Saringan Status Riwayat | `@/components/features/base-features/filter-select` | `REUSE` | Menggunakan base component `FilterSelect` pada `cashier-shift-history.jsx` dengan data array `CASHIER_SHIFT_STATUS_OPTIONS`. |
| Banner Peringatan Blokir | `@/components/features/base-features/information-alert` | `REUSE` | Komponen standar informasi peringatan, digunakan pada `OpenShiftModal` dengan varian `warning` untuk pesan `fieldErrors.general`. |
| Field Form Modal | `@/components/features/base-features/base-form-control` | `REUSE` | `BaseSelectField` dan `BaseTextField` yang sudah ada pada `open-shift-modal.jsx`. |
| Tombol Aksi Modal | `@/components/features/base-features/base-button` | `REUSE` | `BaseButton` varian `secondary` (Batal) dan `primary` (Buka Shift). |

> **Kesimpulan UI Gate:** Seluruh elemen UI menggunakan 100% `REUSE` dari komponen dasar kanonik `base-features`. Tidak ada pembuatan komponen visual baru (`NEW`) atau ekstensi ad-hoc (`EXTEND`).

---

## 4. Rincian Perubahan Source Code

### 4.1 Berkas yang Diperiksa
- `src/lib/hooks/health-services/billing-management/cashier-shift/cashier-shift-constants.js`
- `src/lib/hooks/health-services/billing-management/cashier-shift/use-cashier-shift.js`
- `src/components/view/health-services/billing-management/cashier-shift/open-shift-modal.jsx`
- `src/components/view/health-services/billing-management/cashier-shift/cashier-shift-history.jsx`
- `src/components/view/health-services/billing-management/cashier-shift/cashier-shift-view.jsx`

### 4.2 Berkas yang Diubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/billing-management/cashier-shift/cashier-shift-constants.js` | 1. Menambahkan entri `perlu_tindak_lanjut: { label: "Perlu Tindak Lanjut", className: "region-status-warning" }` pada objek `CASHIER_SHIFT_STATUS_BADGE_CONFIG`.<br/>2. Menambahkan `{ value: "PERLU_TINDAK_LANJUT", label: "Perlu Tindak Lanjut" }` pada array `CASHIER_SHIFT_STATUS_OPTIONS`. |
| `src/lib/hooks/health-services/billing-management/cashier-shift/use-cashier-shift.js` | 1. Pada `confirmOpenShift`: Menangkap `err` dengan status 409 (`Number(err?.statusCode) === 409`), mengekstrak pesan `err?.message`, dan memetakan ke `openFieldErrors.general`.<br/>2. Pada `handleOpenFormChange`: Membersihkan `previous.general` saat pengguna mengubah input field apapun. |
| `src/components/view/health-services/billing-management/cashier-shift/open-shift-modal.jsx` | 1. Mengimpor komponen `InformationAlert` dari `@/components/features/base-features/information-alert`.<br/>2. Merender `<InformationAlert variant="warning" message={fieldErrors.general} />` di dalam `Modal.Body` saat `fieldErrors.general` ada. |
| `tests/unit/cashier-shift-blocking-and-status.test.mjs` | Berkas pengujian unit baru berbasis Node.js test runner untuk memverifikasi pendaftaran konfigurasi badge status, opsi saringan riwayat, integrasi `InformationAlert` pada modal, dan pemetaan error 409 pada hook. |

---

## 5. Verifikasi dan Bukti Kriteria Penerimaan

| Kriteria Penerimaan (Acceptance Criteria) | Target Pengujian | Hasil Pengujian | Status |
| --- | --- | --- | :---: |
| AC-1: Status `PERLU_TINDAK_LANJUT` terdaftar dengan label dan styling yang benar | `CASHIER_SHIFT_STATUS_BADGE_CONFIG.perlu_tindak_lanjut` | Label: `"Perlu Tindak Lanjut"`, className: `"region-status-warning"` | `PASS` |
| AC-2: Opsi saringan status riwayat memuat opsi "Perlu Tindak Lanjut" | `CASHIER_SHIFT_STATUS_OPTIONS` | Terdapat elemen `{ value: "PERLU_TINDAK_LANJUT", label: "Perlu Tindak Lanjut" }` | `PASS` |
| AC-3: Modal Buka Shift merender alert peringatan saat ditolak backend | `open-shift-modal.jsx` | Merender `InformationAlert` varian `warning` dengan teks `fieldErrors.general` | `PASS` |
| AC-4: Hook memetakan penolakan HTTP 409 ke state error modal | `use-cashier-shift.js` | `confirmOpenShift` menangkap 409 dan mengisi `openFieldErrors.general` dengan pesan backend | `PASS` |
| AC-5: Unit test otomatis | `tests/unit/cashier-shift-blocking-and-status.test.mjs` | 4 tests passed, 0 failed (durasi 145 ms) | `PASS` |

---

## 6. Acceptance Criteria & Definition of Done

| Butir Definition of Done | Status | Bukti / Rujukan |
| --- | :---: | --- |
| Entri status baru terdaftar pada konstanta badge dan opsi saringan | Terpenuhi | `cashier-shift-constants.js:12,21` |
| Penanganan error 409 terpasang di hook `useCashierShift` | Terpenuhi | `use-cashier-shift.js:196-203` |
| Modal Buka Shift menampilkan alert blocking informatif | Terpenuhi | `open-shift-modal.jsx:45-50` |
| Unit test terpasang dan lulus 100% | Terpenuhi | `tests/unit/cashier-shift-blocking-and-status.test.mjs` (4/4 PASS) |
| Konsistensi Base Component | Terpenuhi | 100% `REUSE` dari `base-features` |
| Laporan tracked tersedia | Terpenuhi | Berkas laporan ini |
