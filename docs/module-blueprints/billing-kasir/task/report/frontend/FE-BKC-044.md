# Laporan Perubahan Frontend — `FE-BKC-044`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-044` |
| Judul | Review Variance Dua Hasil & Modal Penyelesaian Tindak Lanjut (`ResolveFollowUpModal`) |
| Slice | Gelombang `MVP-34` — Shift Kasir: Penanganan Status Tindak Lanjut dan Resolusi Selisih (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `Gelombang MVP-34`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BKC-044` |
| Trace | `BKC-DEC-124`, `BKC-DEC-125`, `BKC-DEC-126`, `BP-005`, `RULE-011` (dokumen `Shift Kasir (3).md`) |
| Contract version | `BIL-API-1.6` (draft) |
| Wewenang UI | Dua opsi hasil review ("Terverifikasi" vs "Perlu Tindak Lanjut"), tombol aksi "Selesaikan Tindak Lanjut", dan format pesan validasi `"{Nama Field} wajib diisi."` terikat keputusan bisnis (`BKC-DEC-124`, `BKC-DEC-125`, `BKC-DEC-126`). Tata letak modal mengikuti pola modal cashier-shift existing (`COMPOSE`). Styling detail `DEV_DISCRETION` |
| Dependency | `[BE] BE-BKC-078` — Selesai pada kode backend (endpoint `POST /shifts/{id}/resolve-follow-up` dan parameter `Outcome` pada review). Bukti: [BE-BKC-078.md](../backend/BE-BKC-078.md); `FE-BKC-043` — Selesai pada kode frontend (badge dan opsi saringan). Bukti: [FE-BKC-043.md](FE-BKC-043.md) |
| Klasifikasi | `LIGHT` — antarmuka shift kasir; 5 berkas diubah, 1 modal baru (`resolve-follow-up-modal.jsx`), 1 unit test baru; nol migration; nol arsitektur state baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: `cashier-shift-slice.jsx`, `review-variance-modal.jsx`, `resolve-follow-up-modal.jsx`, `use-cashier-shift.js`, `cashier-shift-history.jsx`, `cashier-shift-view.jsx`, `tests/unit/cashier-shift-resolve-follow-up.test.mjs` |
| Tanggal | 2026-09-25 |
| Status | 🟡 **SEBAGIAN — source code dan validasi unit test selesai 100% (6/6 tests PASS).** Kontrol radio evaluasi dua hasil pada modal review, modal penyelesaian tindak lanjut dengan catatan verifikasi wajib, tombol aksi riwayat shift, Redux thunk, dan hook integrasi telah terpasang dan lulus eslint (0 error). Menunggu verifikasi visual interaktif bersama backend aktif. |

---

## 1. Keadaan Awal Sebelum Perubahan

1. **Modal Review Variance Selalu Mengakhiri Shift sebagai `REVIEWED`:**
   Pada implementasi awal, `ReviewVarianceModal` hanya menerima input teks resolusi selisih kas (`resolution`). Ketika disubmit, backend langsung mengubah status shift menjadi `REVIEWED` dan mencabut status selisih kas. Tidak ada opsi bagi Kepala Kasir/Supervisor untuk menandai bahwa selisih kas tersebut masih memerlukan investigasi lanjutan, audit fisik ulang, atau pelaporan berjenjang (`BKC-DEC-124`).
2. **Ketiadaan Modal dan Alur Penyelesaian Shift Berstatus `PERLU_TINDAK_LANJUT`:**
   Ketika backend menyediakan status transisi `PERLU_TINDAK_LANJUT` (`BE-BKC-078`), frontend belum memiliki modal atau alur aksi untuk menyelesaikan status tersebut. Akibatnya, shift yang berstatus `PERLU_TINDAK_LANJUT` akan tertahan selamanya di tabel riwayat dan terus memblokir pembukaan shift kasir baru tanpa jalan keluar di antarmuka web (`BKC-DEC-125`).
3. **Format Pesan Validasi Form Belum Memenuhi Aturan Baku:**
   Sesuai `BKC-DEC-126`, format pesan error validasi field wajib di seluruh sistem harus mengikuti format baku `"{Nama Field} wajib diisi."` (contoh: `"Catatan verifikasi wajib diisi."`). Klien harus memastikan validasi lokal mencerminkan aturan ini sebelum pengiriman data ke server.

---

## 2. Proses Bisnis dari Sisi Pengguna (UX Rumah Sakit)

### Skenario 1: Review Variance dengan Pilihan Tindak Lanjut (`BKC-DEC-124`)
1. Kasir menutup shift dengan selisih kas (misalnya fisik kas kurang Rp50.000 dari hitungan sistem). Status shift menjadi `CLOSED_WITH_VARIANCE`.
2. Kasir melapor ke Kepala Kasir / Supervisor.
3. Kepala Kasir membuka tab **"Riwayat Shift"** atau menekan tombol **"Review Variance Shift"** pada panel Aksi Kepala Kasir.
4. Modal `ReviewVarianceModal` terbuka.
5. Kepala Kasir memeriksa perbandingan kas sistem vs kas fisik yang tercatat.
6. Kepala Kasir mengisi catatan resolusi dan memilih **Hasil Evaluasi**:
   - **Terverifikasi (Selesai):** Selisih kas wajar/terkonfirmasi, shift langsung selesai (`REVIEWED`), pemblokiran kasir dilepas.
   - **Perlu Tindak Lanjut:** Selisih kas mencurigakan atau memerlukan audit fisik lebih mendalam. Status shift berpindah menjadi `PERLU_TINDAK_LANJUT`, dan pemblokiran pembukaan shift baru bagi kasir/register tetap aktif.
7. Kepala Kasir menekan tombol **"Simpan Hasil Review"**.

### Skenario 2: Penyelesaian Tindak Lanjut oleh Supervisor (`BKC-DEC-125`, `BKC-DEC-126`)
1. Setelah investigasi fisik atau konfirmasi audit selesai (misal uang terselip ditemukan di laci lain atau disetor manual), Kepala Kasir membuka tab **"Riwayat Shift"**.
2. Kepala Kasir menyaring status riwayat berdasarkan **"Perlu Tindak Lanjut"**.
3. Pada baris shift terkait, muncul tombol aksi berwarna kuning **"Selesaikan Tindak Lanjut"**.
4. Kepala Kasir mengklik tombol tersebut. Modal khusus `ResolveFollowUpModal` terbuka.
5. Modal menampilkan ringkasan data shift: Nomor Shift, Nama Kasir, dan Nominal Selisih Kas yang tercatat.
6. Kepala Kasir mengisi field **"Catatan Verifikasi"** (maksimal 500 karakter).
   - Bila field ini dikosongkan lalu tombol submit ditekan, form menampilkan pesan error validasi baku:
     > *"Catatan verifikasi wajib diisi."*
7. Kepala Kasir memasukkan catatan hasil penelusuran (misalnya: *"Selisih Rp50.000 telah disetor manual oleh kasir setelah penghitungan ulang fisik laci kasir B."*) dan menekan **"Selesaikan Tindak Lanjut"**.
8. Sistem mengirim permintaan ke backend dengan header `Idempotency-Key`.
9. Status shift berubah menjadi `REVIEWED`, notifikasi sukses ditampilkan, modal menutup, dan tabel riwayat termuat ulang secara otomatis. Pemblokiran kasir kini telah lepas sepenuhnya.

---

## 3. Gerbang Keputusan Base Component (UI Gate)

| Elemen UI | Sumber Komponen | Keputusan | Bukti Pemakaian & Justifikasi |
| --- | --- | :---: | --- |
| Modal Frame & Header/Footer | `react-bootstrap/Modal`, `react-bootstrap/Form` | `REUSE` | Pola modal kanonik seluruh antarmuka Quilvian. Menggunakan styling `region-modal`. |
| Pilihan Radio Hasil Review | `react-bootstrap/Form.Check` | `COMPOSE` | Digunakan pada `review-variance-modal.jsx` dengan tipe `radio`, dibungkus dalam container styling bordered lembut agar mudah dipilih pengguna tablet/desktop. |
| Ringkasan Informasi Shift | Native HTML `<div>` + design tokens | `COMPOSE` | Menampilkan nomor shift, nama kasir, dan format mata uang `Intl.NumberFormat` IDR untuk selisih kas pada `resolve-follow-up-modal.jsx`. |
| Input Catatan Verifikasi | `@/components/features/base-features/base-form-control` (`BaseTextAreaField`) | `REUSE` | Komponen standar textarea dengan label, batas `maxLength={500}`, error feedback, dan helper text. |
| Banner Peringatan & Petunjuk | `@/components/features/base-features/information-alert` | `REUSE` | Digunakan untuk informasi peran Supervisor, peringatan `expectedRowVersion` (concurrency guard), dan error general. |
| Tombol Aksi | `@/components/features/base-features/base-button` | `REUSE` | `BaseButton` varian `warning` (Selesaikan Tindak Lanjut) pada tabel riwayat, varian `secondary` (Batal), dan `primary` (Submit) pada modal. |

> **Kesimpulan UI Gate:** Seluruh elemen UI dibangun menggunakan 100% `REUSE` dan `COMPOSE` dari komponen dasar kanonik `base-features`. Tidak ada ketergantungan library pihak ketiga baru atau modifikasi CSS global.

---

## 4. Rincian Perubahan Source Code

### 4.1 Berkas yang Diubah / Dibuat

| Berkas | Jenis | Perubahan |
| --- | :---: | --- |
| `src/lib/state/slice/health-services/billing-management/cashier-shift-slice.jsx` | Edit | 1. Memperbarui payload `reviewCashierShiftVariance` agar menyertakan parameter `outcome`.<br/>2. Menambahkan createAsyncThunk `resolveCashierShiftFollowUp` yang memanggil `POST /shifts/{id}/resolve-follow-up` dengan header `Idempotency-Key`.<br/>3. Menambahkan reducer builder cases untuk pending, fulfilled, dan rejected `resolveCashierShiftFollowUp`. |
| `src/components/view/health-services/billing-management/cashier-shift/review-variance-modal.jsx` | Edit | Menambahkan grup radio pemilihan hasil evaluasi review: "Terverifikasi (Selesai)" (value: `VERIFIED`) dan "Perlu Tindak Lanjut" (value: `NEEDS_FOLLOW_UP`), terikat ke `form.outcome`. |
| `src/components/view/health-services/billing-management/cashier-shift/resolve-follow-up-modal.jsx` | Baru | Komponen modal baru untuk penyelesaian tindak lanjut. Menampilkan banner penjelasan wewenang supervisor, ringkasan shift tertahan (nomor shift, kasir, nominal selisih berformat Rupiah), concurrency warning, dan textarea `verificationNote` (required, maxLength 500) dengan pesan validasi baku. |
| `src/lib/hooks/health-services/billing-management/cashier-shift/use-cashier-shift.js` | Edit | 1. Memperbarui `buildReviewForm` dan `openReview` dengan inisialisasi default `outcome: "VERIFIED"`.<br/>2. Menambahkan state manajemen penyelesaian: `resolveFollowUpOpen`, `resolveFollowUpTarget`, `resolveFollowUpForm`, `resolveFollowUpFieldErrors`.<br/>3. Menambahkan handler: `openResolveFollowUp`, `closeResolveFollowUp`, `handleResolveFollowUpChange`, dan `confirmResolveFollowUp`.<br/>4. Menegakkan validasi field wajib: `"Catatan verifikasi wajib diisi."` (`BKC-DEC-126`).<br/>5. Menangani dispatch `resolveCashierShiftFollowUp`, notifikasi toast, dan auto-refresh riwayat. |
| `src/components/view/health-services/billing-management/cashier-shift/cashier-shift-history.jsx` | Edit | 1. Menerima prop baru `onResolveFollowUp`.<br/>2. Menambahkan tombol aksi "Selesaikan Tindak Lanjut" (variant `warning`, size `xs`) pada kolom Aksi ketika status baris adalah `perlu_tindak_lanjut`.<br/>3. Menyesuaikan lebar kolom aksi menjadi `minWidth: 165px`. |
| `src/components/view/health-services/billing-management/cashier-shift/cashier-shift-view.jsx` | Edit | 1. Mengimpor komponen `ResolveFollowUpModal`.<br/>2. Meneruskan prop `onResolveFollowUp={(target) => shift.openResolveFollowUp(target)}` ke `<CashierShiftHistory />`.<br/>3. Merender `<ResolveFollowUpModal />` terhubung dengan state dan handler dari hook. |
| `tests/unit/cashier-shift-resolve-follow-up.test.mjs` | Baru | Pengujian unit otomatis (Node.js test runner) yang memverifikasi 6 kriteria penerimaan teknis untuk alur review variance dan modal penyelesaian tindak lanjut. |

---

## 5. Endpoint & Kontrak Integrasi (Gaya OpenAPI / Swagger)

### 5.1 Endpoint Penyelesaian Tindak Lanjut Shift

`[Tags("Billing Management - Cashier Shift")]`

| Properti | Nilai |
| --- | --- |
| **Method** | `POST` |
| **Path** | `/api/v1/health-services/billing-management/cashier/shifts/{id}/resolve-follow-up` |
| **Deskripsi** | Menyelesaikan shift kasir yang berada dalam status `PERLU_TINDAK_LANJUT` menjadi `REVIEWED` dengan catatan verifikasi wajib dari Supervisor/Kepala Kasir. |
| **Otorisasi** | `[AccessAction(AccessPermission.BillingCashierShiftReview)]` |
| **Headers** | `Idempotency-Key`: UUID v4 unik |

#### Request Body (`application/json`)
```json
{
  "expectedRowVersion": "AAAAAAAAB9M=",
  "verificationNote": "Selisih Rp50.000 telah disetor manual oleh kasir setelah penghitungan ulang fisik laci kasir B.",
  "correlationId": "optional-uuid",
  "causationId": "optional-uuid"
}
```

#### Validasi Field Wajib
| Field | Aturan | Pesan Validasi Baku |
| --- | --- | --- |
| `verificationNote` | Required, string, maxLength 500 | `"Catatan verifikasi wajib diisi."` |

#### Response (`200 OK`)
Mengembalikan model `BilCashierShiftResponse` dengan status shift terkini: `"REVIEWED"`.

---

## 6. Verifikasi dan Bukti Kriteria Penerimaan

| Kriteria Penerimaan (Acceptance Criteria) | Target Pengujian | Hasil Pengujian | Status |
| --- | --- | --- | :---: |
| AC-1: Modal review variance menampilkan pilihan hasil evaluasi `VERIFIED` dan `NEEDS_FOLLOW_UP` | `review-variance-modal.jsx` | Kontrol radio dengan `value="VERIFIED"` dan `value="NEEDS_FOLLOW_UP"` terikat ke `form.outcome` | `PASS` |
| AC-2: Baris riwayat berstatus `perlu_tindak_lanjut` menampilkan tombol aksi "Selesaikan Tindak Lanjut" | `cashier-shift-history.jsx` | Tombol render saat `status === "perlu_tindak_lanjut"`, memicu `onResolveFollowUp` | `PASS` |
| AC-3: Modal `ResolveFollowUpModal` berdiri mandiri dengan ringkasan shift dan batas catatan 500 karakter | `resolve-follow-up-modal.jsx` | Menampilkan shiftNumber, cashierName, formatted variance, dan textarea `maxLength: 500` | `PASS` |
| AC-4: Validasi wajib form menghasilkan pesan error baku persis `BKC-DEC-126` | `use-cashier-shift.js` | Divalidasi dengan pesan `"Catatan verifikasi wajib diisi."` saat dikosongkan | `PASS` |
| AC-5: Async thunk Redux memanggil endpoint dengan header `Idempotency-Key` | `cashier-shift-slice.jsx` | Thunk `resolveCashierShiftFollowUp` memanggil `/resolve-follow-up` dengan header idempotensi | `PASS` |
| AC-6: Integrasi komponen pada view utama terhubung lengkap | `cashier-shift-view.jsx` | `ResolveFollowUpModal` terpasang, handler `onResolveFollowUp` tersambung ke `CashierShiftHistory` | `PASS` |
| Pengujian Unit Otomatis | `cashier-shift-resolve-follow-up.test.mjs` | 6 tests passed, 0 failed (durasi 124 ms) | `PASS` |
| Pemeriksaan Gaya Kode (Linter) | `npx eslint` | 0 errors, 0 warnings pada seluruh berkas yang disentuh | `PASS` |

---

## 7. Acceptance Criteria & Definition of Done

| Butir Definition of Done | Status | Bukti / Rujukan |
| --- | :---: | --- |
| Modal review mendukung dua hasil evaluasi ("Terverifikasi" vs "Perlu Tindak Lanjut") | Terpenuhi | `review-variance-modal.jsx:136-193` |
| Komponen modal baru `ResolveFollowUpModal` terpasang dan beroperasi | Terpenuhi | `resolve-follow-up-modal.jsx:24-184` |
| Validasi catatan verifikasi wajib mengikuti format baku `BKC-DEC-126` | Terpenuhi | `use-cashier-shift.js:321` (`"Catatan verifikasi wajib diisi."`) |
| Tombol aksi "Selesaikan Tindak Lanjut" muncul di riwayat shift status terkait | Terpenuhi | `cashier-shift-history.jsx:137-147` |
| Integrasi thunk Redux dengan endpoint backend dan header `Idempotency-Key` | Terpenuhi | `cashier-shift-slice.jsx:167-184` |
| Unit test terpasang dan lulus 100% | Terpenuhi | `tests/unit/cashier-shift-resolve-follow-up.test.mjs` (6/6 PASS) |
| Konsistensi Base Component | Terpenuhi | 100% `REUSE` / `COMPOSE` dari `base-features` |
| Laporan tracked tersedia | Terpenuhi | Berkas laporan ini |
