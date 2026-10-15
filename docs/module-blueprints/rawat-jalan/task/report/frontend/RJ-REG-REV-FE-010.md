# Laporan Task `RJ-REG-REV-FE-010` — Step Jenis Kunjungan

| Field | Isi |
| --- | --- |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) |
| Requirement | `RJ-REG-REV-014` |
| Keputusan | `RJ-REG-DEC-011` (amendemen 15 Okt 2026), wewenang `RJ-REG-DEC-001` |
| Dependency | — (frontend saja; payload `visitType` dan backend tidak berubah) |
| Tanggal | 15 Oktober 2026 |
| Status | ✅ `TESTED` / `UAT_PENDING` |

## 1. Hasil untuk pengguna

Pendaftaran Rawat Jalan kini punya step tersendiri **Jenis Kunjungan** tepat sesudah Cari/Input Pasien. Petugas memilih *Umum* atau *Rujukan / Tindak Lanjut* lebih dulu, lalu mengisi Data Kunjungan. Data Kunjungan hanya memuat tanggal, poliklinik, dan jadwal dokter.

| Jenis | Urutan step |
| --- | --- |
| Umum | Cari/Input Pasien → **Jenis Kunjungan** → Data Kunjungan → Metode Pembayaran → General Consent → Verifikasi & Konfirmasi → Selesai |
| Rujukan / Tindak Lanjut | Cari/Input Pasien → **Jenis Kunjungan** → Data Kunjungan → Data Rujukan → Metode Pembayaran → General Consent → Verifikasi & Konfirmasi → Selesai |

Contoh: petugas memilih *Rujukan / Tindak Lanjut* di step 2. Bar step langsung menampilkan *Data Rujukan* sesudah Data Kunjungan, dan tombol Data Kunjungan berubah menjadi *Lanjut ke Data Rujukan*. Bila petugas kembali dan mengganti ke *Umum* padahal data rujukan sudah diisi, layar meminta konfirmasi karena data rujukan akan dikosongkan.

Atas masukan user selama pengerjaan, kartu jadwal dokter di Data Kunjungan juga disesuaikan:

- Kartu memakai lebar penuh, sehingga tidak ada ruang kosong di kanan bekas kolom Jenis Kunjungan. Setiap baris, termasuk baris terakhir, melebar memenuhi kolom.
- Foto dokter setinggi isi kartu: atasnya sejajar nama dan bawahnya sejajar tombol *Pilih Jadwal*.

## 2. Berkas

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../outpatient-registration.constants.js` | `OUTPATIENT_REGISTRATION_STEP.VISIT_TYPE = 8`; step *Jenis Kunjungan* di kedua bar step. Bar Rujukan disusun menurut nilai step, bukan indeks array |
| `src/lib/hooks/.../use-outpatient-registration.js` | Lanjut dari pasien (4 jalur) membuka `VISIT_TYPE`; `handleVisitTypeStepNext` baru menuju `VISIT` |
| `src/components/view/.../outpatient-visit-type-step.jsx` | Baru. Kartu Umum / Rujukan-Tindak Lanjut, konfirmasi Rujukan → Umum, tombol Kembali / *Lanjut ke Data Kunjungan* |
| `src/components/view/.../outpatient-visit-step.jsx` | Kolom Jenis Kunjungan beserta modal konfirmasinya dihapus; label `LANGKAH {stepNumber}` dinamis |
| `src/components/view/.../outpatient-registration-page.jsx` | Render step `VISIT_TYPE`; nomor step untuk Data Kunjungan |
| `src/style/.../outpatient-registration.module.css` | `.visitTypeChoiceGrid` baru; aturan kolom lama (`scheduleVisitTypeRow`, `visitTypeColumn`, `visitTypeStack`, `visitTypeCompact`) dihapus karena tidak dipakai; `.doctorCardGrid` menjadi flex-wrap; `.doctorCardPhoto` setinggi isi kartu |

Delta kontrak: tidak ada. Backend dan payload tidak berubah.

## 3. Keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kartu Umum / Rujukan | `REUSE` | `.visitTypeCard` IGD yang sebelumnya dipakai di Data Kunjungan |
| Modal konfirmasi | `REUSE` | `ConfirmModal` |
| Kerangka step | `COMPOSE` | `stepUnifiedCard`, `stepCardHeader`, `stepActions` yang dipakai step lain |
| Grid kartu jadwal, foto | `COMPOSE` | Kelas fitur sendiri (FE-005), token design system |

`UI GATE: PASS` — tidak ada komponen baru di base-features dan tidak ada perubahan perilaku default base component. Bentuk visual mengikuti `DEV_DISCRETION` pada kartu task.

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint 5 berkas JS/JSX tersentuh | exit 0 — `PASS` |
| `npm run build` (sesudah perubahan terakhir) | "Compiled successfully", tanpa `Failed` — `PASS` |
| Grep anti-regresi CSS baru (warna literal, `rgba`, `!important`) | kosong — `PASS` |
| AUTOMATED TEST | `SKIPPED (opsional)` — perilaku diuji di layar |

Uji layar Playwright: FE dev `localhost:3000`, API `7184` dialihkan ke backend uji `https://localhost:7185`, DB `QuilvianNewDevSukma`, akun SuperAdmin, pasien `REGREV LAYAR 2508`.

| ID | Skenario | Hasil |
| --- | --- | --- |
| V1 | Bar step Umum: Cari/Input Pasien → Jenis Kunjungan → Data Kunjungan → Metode Pembayaran | `PASS` |
| V2 | Lanjut dari pasien membuka Jenis Kunjungan (LANGKAH 2) | `PASS` |
| V3 | Pilih Rujukan menambah *Data Rujukan* di bar step | `PASS` |
| V4 | Kembali ke Umum tanpa data rujukan: tanpa konfirmasi, *Data Rujukan* hilang | `PASS` |
| V5 | Data Kunjungan = LANGKAH 3 | `PASS` |
| V6 | Data Kunjungan tidak memuat pilihan Jenis Kunjungan | `PASS` |
| V11 | Kartu jadwal (4 kartu, Poli Penyakit Dalam) memenuhi lebar setiap baris, ruang kosong kanan 0 px | `PASS` |
| V7 | Kembali dari Data Kunjungan membuka Jenis Kunjungan | `PASS` |
| V8 | Pilihan Umum tetap tersimpan | `PASS` |
| V9 | Rujukan: Data Kunjungan menampilkan *Lanjut ke Data Rujukan* | `PASS` |
| V10 | Tidak ada error halaman | `PASS` |

Total 11/11 `PASS`.

Uji tata letak headless dengan CSS asli (`globals.css` + modul RJ), lebar 320, 375, 768, 1024, 1440, dan 1920 px, kartu berfoto dan berinisial: bawah foto sama dengan bawah tombol *Pilih Jadwal* (selisih 0 px) dan atas foto sama dengan atas nama (selisih 0 px) — `PASS`.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Bar step menampilkan Jenis Kunjungan sebagai step 2 (Umum dan Rujukan) | Terpenuhi | V1, V2, V3 |
| 2. Lanjut dari pasien membuka Jenis Kunjungan | Terpenuhi | V2 |
| 3. Data Kunjungan tanpa pilihan Jenis Kunjungan | Terpenuhi | V6 |
| 4. Rujukan menambah Data Rujukan; Rujukan → Umum dengan data rujukan meminta konfirmasi | Penambahan step terpenuhi (V3, V4). Konfirmasi dipindah utuh dari Data Kunjungan (logika `hasReferralData` sama), belum diklik di layar dengan data rujukan terisi | V3, V4, kode |
| 5. Kembali dari Data Kunjungan membuka Jenis Kunjungan | Terpenuhi | V7, V8 |
| 6. Pendaftaran Umum dan Rujukan terkirim dengan `visitType` benar | Terpenuhi lewat kode: payload dan `handleVisitTypeChange` tidak diubah. Submit penuh tidak dijalankan pada uji ini | kode |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Dengan 7 step, judul di bar step terpotong ("Cari / Inpu…") pada layar 1440 px yang memuat sidebar; di layar lebih lebar judul tampil penuh. Kartu terakhir yang sendirian di barisnya kini melebar selebar kolom. Submit penuh dan konfirmasi Rujukan → Umum dengan data terisi masuk UAT |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??`, belum di-commit |
| Langkah berikutnya | UAT petugas pendaftaran |
