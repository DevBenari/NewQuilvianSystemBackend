# Laporan Perubahan Frontend — `FE-ACC-FIN-001-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-FIN-001-01` |
| Judul | Tambah Akun dengan kode otomatis |
| Slice | Gelombang pengiriman `FIN-2` |
| Roadmap | `roadmap/frontend-roadmap-finishing.md` revisi 2, kartu `FE-ACC-FIN-001-01` |
| Trace | ACC-FIN-001 (task analis COA); `FR-FIN-050`..`053`; `ACC-DEC-162`, `163`, `164`, `166`, `172`; `03-frontend-architecture.md` bagian 16.4, 16.10 |
| Contract version | `ACC-API-0.18` (`GET /chart-of-accounts/next-code`, `POST /chart-of-accounts`) — approved Rizki 25 November 2026 (`GATE-DESAIN-1125`) |
| Dependency | `BE-ACC-FIN-001-01` ✅ (25 November 2026, belum di-commit) |
| Task mode | `FRONTEND` — perintah Rizki 25 November 2026 "lanjjutkan" |
| Target tulis | Repository `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `a2ddb52d2` |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-8 terbukti 25 November 2026: uji layar Playwright agent terhadap `next dev` Rizki 6/6 + uji balapan 1/1, ESLint 0 galat (3 peringatan lama di baris yang tidak disentuh). `npm run build` `PASS` (Rizki, BUILD_ID 15:39:57 WIB). UAT belum dijalankan — milik tim UAT |

## 1. Masalah yang diperbaiki

Backend kini menyusun kode akun sendiri dan mewarisi jenis akun dari induk (`BE-ACC-FIN-001-01`), tetapi form
Tambah Akun masih meminta petugas mengetik kode dan memilih jenis bebas. Petugas tidak dapat melihat kode
yang akan terbit, dan dapat memilih jenis yang pasti ditolak backend.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Menambah akun tanpa mengetik kode; jenis akun anak mengikuti induk |
| Pelaku | Pemegang `ChartOfAccount : Create` |
| Pemicu | Daftar COA → Tambah |
| Langkah | 1) Pilih akun induk (atau jenis akun untuk akun tingkat 1). 2) Layar menampilkan usulan kode, tingkat, jenis terkunci, dan saldo normal usulan induk. 3) Isi nama dan sisanya. 4) Simpan. 5) Toast menyebut kode final dari respons |
| Jalur tidak normal | Pratinjau ditolak (mis. akar jenis Aset sudah ada) → pesan backend di bawah Kode Akun, Simpan mati dengan alasan yang sama; pratinjau sedang dimuat → "Menyusun kode otomatis...", Simpan mati; induk berganti cepat → respons induk lama dibuang; kode final berbeda dari usulan → toast menyebut keduanya |
| Hasil | Akun tersimpan berkode backend |

**Contoh dari data dev.** Induk 1-1000 Kas dan Setara Kas → Kode Akun `1-1005`, Jenis Akun "Aset" terkunci
("Mengikuti akun induk 1-1000."), Saldo Normal "Debit" ("Diusulkan dari akun induk 1-1000. Ganti bila akun ini
akun kontra, misalnya Akumulasi Penyusutan.").

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/corporate/accounting/accounting-chart-of-account-slice.jsx` | + thunk `getChartOfAccountNextCode` (pola `getChartOfAccountTree`: tanpa state store, `signal` untuk pembatalan; `accountType` hanya dikirim tanpa induk) |
| `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account-editor.jsx` | Pratinjau dipanggil saat badan hukum, induk, atau jenis (tanpa induk) berganti; hasil disimpan berkunci `badan hukum|induk|jenis` sehingga respons basi diabaikan dan permintaan lama dibatalkan. Nilai Kode, Jenis, dan Saldo Normal diturunkan saat render (tanpa `setState` di dalam effect). `flattenAccountTree` membawa `accountType`/`normalBalance` induk. Isian mode tambah: Kode Akun nonaktif berisi usulan; Jenis Akun terkunci bila berinduk; keterangan Saldo Normal usulan. Galat pratinjau digabung ke `fieldErrors.accountCode`. `submitBlockedReason` + penjaga di `handleSubmit`. Toast sukses memakai kode dari respons |
| `src/utils/corporate/accounting/chart-of-account/chart-of-account-utils.jsx` | `validateForm`: Kode Akun tidak wajib pada mode tambah; Jenis Akun wajib hanya tanpa induk. `buildPayload` (tambah): `accountCode` tidak dikirim, `accountType` `null` bila berinduk |
| `src/components/view/corporate/accounting/chart-of-account/form/chart-of-account-form-view.jsx` | `formProps.renderActions`: tombol Simpan mati beserta `title` alasan saat `submitBlockedReason` terisi — pola `event-type-form-view.jsx` |

Nol baris komentar baru. Mode Perbarui tidak berubah perilaku: kode tetap isian, tanpa pratinjau.

**Delta lingkup terhadap kartu.** Kartu menyebut `chart-of-account-constants.jsx` untuk keterangan isian;
keterangan baru diletakkan sebagai konstanta di hook, di samping `ACCOUNT_LEVEL_LOCKED_DESCRIPTION` yang sudah
ada di sana, karena dipakai hanya oleh hook itu. Berkas konstanta tidak disentuh.

### 3.2 Dampak

| Aspek | Dampak |
| --- | --- |
| Kontrak | `next-code` dan `POST` sesuai `ACC-API-0.18`. Field klasifikasi warisan pada respons pratinjau belum ada di backend (milik `001-02`) dan tidak dipakai layar ini |
| Toast sesudah simpan | Mengikuti konvensi editor Accounting: toast lalu kembali ke daftar. Toast terlihat sebentar sebelum perpindahan halaman (tertangkap pada L5) |

## 4. Gerbang keputusan base component

`UI GATE: 6 elemen — REUSE 5, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Kode Akun sebagai pratinjau | `BaseEditorField` teks `disabled` + `description` | Pola Tingkat Akun di form yang sama | REUSE | — |
| Jenis Akun terkunci | `BaseEditorField` select `disabled` + `description` | Sama | REUSE | — |
| Saldo Normal usulan | `BaseEditorField` select | Form yang sama | REUSE | Keterangan dinamis |
| Galat pratinjau | `fieldErrors` → `BaseFieldShell` | Galat validasi form yang ada | REUSE | — |
| Toast kode final | `ToastStack` lewat `createToast` | Editor yang sama | REUSE | — |
| Simpan mati saat pratinjau belum siap | `formProps.renderActions` + `BaseButton` + `editorStyles.editorActions` | `event-type-form-view.jsx` (Accounting) | COMPOSE | Lihat pilihan A |

**Pilihan A — mematikan tombol Simpan saja.**

1. **`renderActions` lewat `formProps` dengan `BaseButton` dan kelas `editorActions` base — direkomendasikan dan dipakai.** Sudah dipakai `event-type-form-view.jsx` di Accounting dan beberapa layar Aset; tampilan tombol identik dengan bawaan; base component tidak diubah.
2. EXTEND `BaseEditorForm` dengan prop `submitDisabled`. Lebih ringkas di pemakai, tetapi menyentuh base component yang dipakai puluhan form; risiko regresi dan butuh persetujuan.
3. Tombol tetap aktif, `handleSubmit` menolak dengan toast. Tanpa sentuhan tampilan, tetapi petugas baru tahu sesudah mengklik; tidak memenuhi AC-4 "Simpan mati". Dipakai hanya sebagai penjaga cadangan di hook.

## 5. Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Modul referensi visual | Form COA yang sama; pola tombol `event-type-form-view.jsx` |
| Grep pada diff: warna literal / ukuran px / `<button>` / `<table>` / `fw-` `fs-` / `!important` / `style={` | 0 / 0 / 0 / 0 / 0 / 0 / 0 |
| `globals.css` dan CSS fitur | Tidak disentuh |
| Aksesibilitas | Alasan Simpan mati tersedia di `title`; keterangan isian berupa teks; galat tampil sebagai teks galat isian |

## 6. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` 4 berkas tersentuh | 0 galat, 3 peringatan `react-hooks/set-state-in-effect` — seluruhnya pada efek lama (`setRouteError`, dua `setForm`) yang tidak disunting | `PASS` |
| Pemindaian baris komentar baru | 0 | `PASS` |
| `fin-001-01-20261125/03-uji-layar.mjs` L1–L6 (15.33:46–15.34:44 WIB, `next dev` Rizki port 3000, backend DLL 15:06:02) | 6/6 | `PASS` |
| `fin-001-01-20261125/04-uji-balapan.mjs` L7 (15.35 WIB, nol tulis) | 1/1 | `PASS` |
| `npm run build` | Dijalankan Rizki; `.next/BUILD_ID` 25 November 2026 15:39:57 WIB, sesudah suntingan source terakhir 15:30:21; chunk build memuat teks "Menyusun kode otomatis" | `PASS` |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — kesepakatan kerja Rizki tanpa unit test | — |

**MANUAL TEST** (Playwright msedge, layar sungguhan):

| Uji | Isi |
| --- | --- |
| L1 | Awal: Kode Akun nonaktif, placeholder "Pilih akun induk atau jenis akun", Simpan mati `title` "Pilih akun induk atau jenis akun agar kode akun dapat disusun." |
| L2 | Induk 1-1000 → `1-1005`, Jenis "Aset" terkunci, Saldo Normal "Debit" usulan, Simpan aktif (`L02-induk-1-1000.png`) |
| L3 | Saldo Normal diganti Kredit lalu induk berganti ke 1-2000, 1-1000, 1-3000 → Saldo tetap Kredit, kode akhir `1-3004` |
| L4 | Tanpa induk + jenis Aset → `409` tampil di bawah Kode Akun, Simpan mati, Jenis aktif (`L04-galat-pratinjau.png`) |
| L5 | Grup uji 1-5000 diaktifkan sementara; pratinjau ditiru `1-5001` (`route.fulfill`, simulasi balapan antarpetugas); Simpan diklik dua kali → **satu** `POST`, badan tanpa `accountCode` dan `accountType: null`; respons `201` `1-5002`; toast "Akun tersimpan dengan kode 1-5002 karena 1-5001 baru saja dipakai." (`L05-simpan.png`) |
| L6 | Perbarui 1-1003 (dibuka dari daftar → rincian → Perbarui): Kode Akun isian aktif "1-1003", nol panggilan pratinjau |
| L7 | Balapan sungguhan: pratinjau 1-1000 ditahan 4 detik, induk diganti 1-3000 sebelum tiba → saat memuat kode kosong, "Menyusun kode otomatis...", Simpan mati; permintaan 1-1000 `ERR_ABORTED`; kode akhir `1-3004` (`L07-memuat.png`, `L07-balapan.png`) |

Catatan kejujuran: putaran pertama L3 tidak benar-benar menguji balapan, karena skrip menunggu respons lambat
tiba sebelum mengganti induk. Hal itu ditemukan saat memeriksa skrip, lalu diganti uji L7 yang memang
mengganti induk sebelum respons tiba.

**Data dev.** Satu akun uji baru `1-5002 UJI COA Layar FE-ACC-FIN-001-01` di grup uji `1-5000`; keduanya
dinonaktifkan sesudah uji (tercatat di `03-uji-layar.json` bagian `persiapan`). Nol SQL.

## 7. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 Induk 1-1000 → "1-1005" + keterangan; simpan memakai kode dari respons (`F25`) | Terbukti | L2, L5 |
| AC-2 Jenis terkunci "Aset" bila berinduk; tanpa induk dapat dipilih dan pratinjau mengikuti (`F31`) | Terbukti | L2, L4 |
| AC-3 Saldo Normal dari induk; pilihan petugas tidak ditimpa | Terbukti | L2, L3 |
| AC-4 Pratinjau gagal → pesan backend apa adanya, Simpan mati (`F29`) | Terbukti | L4 |
| AC-5 Ganti induk cepat tidak meninggalkan kode lama | Terbukti | L7 |
| AC-6 Klik Simpan berulang hanya satu permintaan | Terbukti | L5 |
| AC-7 Perbarui Akun: kode tetap isian (`F33`) | Terbukti | L6 |
| AC-8 Lint nol galat | Terbukti | Bagian 6 |

**DoD.** AC-1..AC-8 terbukti; laporan; roadmap dan traceability diperbarui → ✅. `npm run build` `PASS` (Rizki).

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Rendah. Nilai Kode/Jenis/Saldo diturunkan dari keadaan, bukan disalin ke state, sehingga tidak ada penyelarasan ganda |
| Temuan, tidak diubah | Tiga peringatan lint lama `set-state-in-effect` di efek `setRouteError` dan `setForm` hook yang sama |
| Status Git frontend | `M` empat berkas di bagian 3.1; belum di-commit |
| Langkah berikutnya | `BE-ACC-FIN-001-02` (kolom klasifikasi; agent berhenti sebelum `dotnet ef migrations add`, migration oleh Rizki) |
