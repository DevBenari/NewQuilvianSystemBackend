# Laporan Perubahan Frontend — `FE-BKC-FIX-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-007` (ad-hoc, permintaan langsung pengguna — bukan task roadmap bernomor `FE-BKC-0xx`, dibuat sendiri untuk menjaga jejak laporan tetap tracked) |
| Judul | Tombol "Batal" pada form Buat Invoice Manual tidak lagi berpindah ke halaman daftar invoice — jadi reset form di tempat |
| Slice | Permintaan UX langsung pengguna atas form `FE-BKC-014` — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan kontrak API, murni perilaku klien |
| Wewenang UI | `REUSE` murni — memakai `setForm(buildEmptyForm())`/`setFieldErrors`/`setErrorMessage` yang sudah ada (pola yang sama dipakai `handleSubmit` setelah sukses), tidak menyentuh base component apa pun |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — 1 berkas diubah, mengganti isi satu `useCallback` |
| Task mode | `FRONTEND` — permintaan eksplisit pengguna |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/hooks/health-services/billing-management/billing-invoices/use-create-manual-invoice.js` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit) |
| Tanggal | 3 September 2026 |
| Status | Source selesai, lint bersih, **terverifikasi hidup** |

---

## 1. Keadaan yang ditemukan di awal

`handleCancel` (`use-create-manual-invoice.js`) sebelumnya `() => router.push(BILLING_INVOICE_ROUTES.list)`
— tombol "Batal" pada form "Buat Invoice Manual" membawa pengguna keluar ke halaman daftar invoice
("running invoice"). Pengguna meminta perilaku itu dihapus: tombol "Batal" TIDAK boleh lagi
berpindah halaman sama sekali, cukup mengosongkan/mereset pilihan form di tempat — konsisten
dengan halaman ini yang memang alat bantu testing untuk membuat beberapa invoice berturut-turut
(deskripsi halaman sendiri sudah menyebut "membuat invoice pertama... supaya bisa langsung
membuat invoice berikutnya").

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: siapa pun yang membuka "Buat Invoice Manual" dan ingin mengosongkan pilihan yang
sudah diisi tanpa keluar dari halaman.

**Langkah (sesudah perbaikan)**:

1. Pengguna mengisi sebagian/seluruh field (kunjungan, kategori, tarif, qty).
2. Klik "Batal".
3. Seluruh field kembali ke keadaan kosong/default (sama persis dengan `buildEmptyForm()` yang
   juga dipakai setelah submit sukses) — pengguna TETAP di halaman yang sama, tidak ada navigasi.

**Aturan yang berlaku**: `handleCancel` kini identik dengan bagian reset pada `handleSubmit`
setelah sukses (`setForm(buildEmptyForm())`, `setFieldErrors({})`, `setErrorMessage("")`) — hanya
tanpa efek samping dialog "Invoice Berhasil Dibuat" (karena memang tidak ada invoice yang dibuat).

**Jalur tidak normal**: `NOT APPLICABLE`.

**Hasil akhir**: "Batal" sekarang murni tombol reset lokal, bukan navigasi keluar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`use-create-manual-invoice.js` (`handleCancel`, `handleSubmit` sebagai referensi pola reset,
`buildEmptyForm`); `create-manual-invoice-view.jsx` (pemakaian `handleCancel` lewat prop
`onCancel` ke `BaseEditorView`/tombol "Batal" — dibaca saja, TIDAK diubah, karena kontraknya
tetap sama: sebuah handler tanpa argumen).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `use-create-manual-invoice.js` | `handleCancel`: dari `() => router.push(BILLING_INVOICE_ROUTES.list)` menjadi `() => { setForm(buildEmptyForm()); setFieldErrors({}); setErrorMessage(""); }` — tidak lagi memakai `router` |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Perilaku tombol "Batal" | `REUSE` | Memakai state setter yang sudah ada (`setForm`/`setFieldErrors`/`setErrorMessage`, pola identik dengan reset pasca-submit-sukses) — tidak ada base component baru/extend, `onCancel` tetap dikonsumsi `BaseEditorView` persis seperti sebelumnya |

**`UI GATE`**: tidak ada — murni perubahan isi handler, tidak menyentuh base component apa pun.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Klik "Batal" dengan field terisi | Seluruh field kembali kosong/default, tetap di halaman yang sama |
| Klik "Batal" dengan field sudah kosong | Tidak ada perubahan terlihat (sudah kosong), tetap di halaman yang sama |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada permintaan API, murni reset state klien.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint use-create-manual-invoice.js` | Berhasil tanpa error | `PASS` | Keluaran perintah kosong |
| Isi kunjungan + kategori, klik "Batal" | URL halaman TIDAK berubah (`STAYED_ON_SAME_PAGE = true`); field Pasien/Kunjungan kembali ke placeholder "Cari nama pasien, No. RM, atau No. Kunjungan..."; field Kategori Tarif kembali ke placeholder "Semua kategori" | `PASS` | URL sebelum/sesudah dan label trigger field diperiksa langsung dari DOM |
| Console error selama pengujian | Hanya noise pra-eksisting tidak terkait (CSP foto profil, dsb.) — tidak ada error baru dari perubahan ini | `PASS` | Console dipantau selama sesi Playwright |

Uji manual: `PASS`.

**Tidak dijalankan:** `npm run build`/`next build` penuh; component test (instruksi eksplisit
pengguna sepanjang sesi ini — tanpa file test).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tombol "Batal" tidak lagi berpindah/redirect ke halaman lain | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| Tombol "Batal" mengosongkan/mereset pilihan form | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| lint lulus | Terpenuhi | Lihat § 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Interupsi | `NONE` |
| Status Git | Modified (task ini): `use-create-manual-invoice.js`. Berkas lain pada working tree yang sama milik task-task sebelumnya (lihat laporan masing-masing). Belum staged/commit |
| Langkah berikutnya | `SELESAI` — permintaan pengguna terpenuhi dan terverifikasi hidup |
