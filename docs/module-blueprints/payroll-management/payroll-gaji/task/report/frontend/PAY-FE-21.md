# Laporan Perubahan Frontend — `PAY-FE-21`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-21` |
| Judul | Kelengkapan Data Payroll |
| Slice | Belum terdaftar sebagai EPIC (di luar `EPIC PGJ-01`–`07`); didaftarkan setelah source ditulis, lihat bagian 7. Layar tidak ada di `03-frontend-architecture.md` (`FE-PGJ-01`–`10`) |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-21` |
| Trace | Perintah pemilik 2026-11-16 (Payroll hanya membaca data HRD; yang ditulis Payroll hanya profil payroll). **Belum ada FR-PGJ dan PRL-DEC bernomor** — tidak dikarang |
| Contract version | **Tidak tercakup `PGJ-C1`**. Endpoint dibaca langsung dari source backend (`PAY-BE-24`); kontrak amandemen belum ada — gerbang `G1` terbuka |
| Wewenang UI | Tidak ada skema layar yang disetujui di blueprint. Isi layar, kolom, dan susunan ditentukan saat penulisan source dari pola layar Payroll yang ada; tidak ada keputusan UI pemilik yang tercatat |
| Dependency | `PAY-BE-24` 🟡 — source di-commit pemilik dan `dotnet build` berhasil menurut pemilik, tetapi uji API belum |
| Klasifikasi | `MEDIUM` — satu halaman daftar baca dengan ringkasan dan filter, satu modal impor dua tahap, satu butir menu, sembilan berkas source baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repository backend |
| Model | Source ditulis 2026-11-16 (model tidak dicatat saat itu); pendaftaran roadmap dan laporan ini: Claude Sonnet 5.5 |
| Commit frontend | `9bb9916a5` dan `925a63be2` (branch `yasmina`, 2026-11-16) memuat halaman dan utilitas bersama; HEAD saat laporan ditulis `925a63be2` |
| Commit backend yang dijadikan rujukan | `dc723f0f` (`PAY-BE-24`) |
| Tanggal | 2026-11-16 |
| Status | 🟡 — source selesai dan sudah di-commit pemilik; ESLint 0 galat (2 peringatan `set-state-in-effect`) dan 16 dari 16 uji unit lulus (dijalankan agen 2026-11-16); `check-css-usage.mjs` 0 temuan; **`npm run build` berhasil menurut pemilik (2026-11-16, laporan lisan, tanpa keluaran tersimpan; bukan bukti agen)**; layar belum dibuka di peramban dan belum diuji manual |

---

## 1. Keadaan yang ditemukan di awal

- Layar Profil Penggajian (`/payroll/workforce-compensations`) bersumber dari profil payroll, sehingga pegawai atau
  dokter yang belum punya profil tidak tampil dan tidak ada tempat membuat profil banyak orang.
- Backend (`PAY-BE-24`) menyediakan `GET /`, `GET /summary`, dan `POST /import` di
  `api/v1/corporate/payroll-management/payroll-data-readiness` dengan hak `PayrollDataReadiness : Read` dan `Import`.
- Perintah pemilik 2026-11-16: Payroll hanya membaca data HRD; satu-satunya yang ditulis adalah profil payroll.

## 2. Proses bisnis

**Tujuan.** Petugas Payroll melihat siapa yang belum siap digaji beserta alasannya, lalu mengimpor profil payroll untuk
orang yang data HRD-nya sudah lengkap.

**Langkah:**

1. Petugas membuka **Payroll > Penghasilan Pegawai > Kelengkapan Data**.
2. Layar menampilkan lima kartu ringkasan (total, punya profil, penetapan gaji disetujui, rekening terverifikasi,
   lengkap) dan tabel pegawai/dokter aktif dengan status tiap kelengkapan dan daftar alasan.
3. Petugas menyaring menurut Jenis Penerima, Kelengkapan Data, dan kata kunci.
4. Petugas dengan hak `Import` menekan tombol impor, memilih berkas CSV (kolom wajib Nomor Pegawai/Dokter; opsional
   Kode Cost Center, PPh Bulanan, BPJS Bulanan), menekan **Periksa Data** (uji kering, belum menyimpan), lalu
   **Simpan Data**.
5. Bila ada baris yang ditolak, tidak ada yang tersimpan dan modal menampilkan nomor baris beserta sebabnya.

**Jalur tidak normal:**

| Keadaan | Hasil di layar |
| --- | --- |
| 403 saat memuat daftar | Layar akses ditolak (`AccessDeniedGate`) |
| Gagal memuat | Peringatan galat di atas tabel |
| Tidak ada hasil | Keadaan kosong tabel |
| Isian berkas tidak terbaca (PPh bukan angka, nomor notasi ilmiah, judul kolom wajib hilang) | Ditahan di peramban per baris, tidak dikirim |
| Impor ditolak server (400) | Daftar baris gagal dengan nomor baris dan sebab; tidak ada data tersimpan |
| Pengguna tanpa hak `Import` | Tombol impor tidak tampil |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/app/payroll/data-readiness/page.jsx` | Baru | Route `/payroll/data-readiness` |
| `src/components/view/payroll/data-readiness/payroll-data-readiness-view.jsx` | Baru | Hero, kartu ringkasan, catatan, filter, tabel |
| `src/components/view/payroll/data-readiness/payroll-data-readiness-table-columns.jsx` | Baru | Kolom tabel dan lencana status |
| `src/components/view/payroll/data-readiness/payroll-data-import-modal.jsx` | Baru | Modal impor dua tahap |
| `src/lib/constants/payroll/payroll-data-readiness-constants.js` | Baru | Endpoint, teks, opsi filter |
| `src/lib/hooks/payroll/use-payroll-data-readiness.js` | Baru | Daftar, ringkasan, filter, `canImport` |
| `src/lib/hooks/payroll/use-payroll-data-import.js` | Baru | Baca CSV, uji kering, simpan |
| `src/lib/services/payroll/payroll-data-readiness-service.js` | Baru | `list`, `summary`, `importRows` |
| `src/utils/payroll/payroll-data-readiness-utils.js` | Baru | Normalizer, parser CSV, peringatan impor |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah | Butir "Kelengkapan Data" di grup "Penghasilan Pegawai", dijaga `PayrollDataReadiness : Read` |
| `tests/unit/payroll-data-readiness.test.mjs` | Baru | 16 uji, termasuk pencocokan ke source backend |

Utilitas generik dipakai dari `src/utils/shared/api-payload-utils.js` (dipindah untuk memutus kopling dengan halaman HR).

### 3.2 Tabel keputusan base component

| Elemen | Status | Dasar |
| --- | --- | --- |
| Hero, ringkasan, filter, tabel, paginasi, peringatan, toast, penjaga akses, modal konfirmasi, tombol | `REUSE` | Komponen `base-features` yang sama dengan layar Payroll lain |
| Modal impor | `COMPOSE` dari `ConfirmModal` dan field dasar | Pola impor dan `confirmProps.disabled` dikunci uji |

`UI GATE`: tidak ada elemen `NEW`. Tabel ini disusun dari tinjauan source untuk laporan ini, bukan dari tabel keputusan
yang dibuat sebelum JSX ditulis (task didaftarkan setelah source ada).

## 4. Dokumentasi layar

| Wilayah | Isi | Sumber data | Hak akses |
| --- | --- | --- | --- |
| Kepala | Judul "Kelengkapan Data Payroll" dan deskripsi | Teks tetap | `PayrollDataReadiness : Read` (butir menu) |
| Kartu ringkasan | Lima hitungan | `GET /summary` | `Read` |
| Catatan | Payroll hanya membaca data HRD; yang dibuat lewat impor hanya profil payroll | Teks tetap | — |
| Filter | Kata kunci, Jenis Penerima, Kelengkapan Data, jumlah baris | Opsi tetap | `Read` |
| Tabel | Kolom No.; Pegawai / Dokter (nama, nomor, jenis); Profil Payroll; Penetapan Gaji Utama (HRD); Rekening Payroll (HRD); Kelengkapan (lencana dan daftar alasan). Tanpa nominal gaji dan nomor rekening | `GET /` | `Read` |
| Tombol impor | Membuka modal impor | — | `PayrollDataReadiness : Import` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint pada 10 berkas feature | 0 galat, 2 peringatan `react-hooks/set-state-in-effect` pada hook baru (pola sama dengan hook Payroll lain) | `PASS` | Dijalankan agen 2026-11-16 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-data-readiness.test.mjs` | 16 uji, 16 lulus, 0 gagal | `PASS` | Dijalankan agen 2026-11-16 |
| `check-css-usage.mjs` pada tiga berkas view | `OK — 3 berkas diperiksa, 0 temuan` | `PASS` | Dijalankan agen 2026-11-16 |
| Pencocokan ke source backend | Uji membaca controller dan service backend: kolom impor, resource RBAC, rute, batas baris, batas kepemilikan, dan log tanpa nominal | `PASS` | Uji nomor 182, 207, 233 |
| `npm run build` | Berhasil menurut pemilik, 2026-11-16 | `PASS (dilaporkan pemilik)` | Laporan lisan; tanpa keluaran tersimpan; bukan bukti agen |
| Pemeriksaan visual dan screenshot | Layar belum dibuka | `NOT FEASIBLE` oleh agen | — |
| Uji manual | Belum dijalankan | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: node --test tests/unit/payroll-data-readiness.test.mjs — PASS (16/16)`

Suite penuh tidak dijalankan pada pendaftaran ini; baseline 51 gagal di modul lain (16 November 2026) tidak diukur ulang.

**Langkah uji untuk pemilik** (login dengan hak `PayrollDataReadiness : Read` dan `Import`):

1. Buka **Payroll > Penghasilan Pegawai > Kelengkapan Data**. Harapan: lima kartu terisi; tabel menampilkan pegawai dan dokter aktif, termasuk yang belum punya profil.
2. Saring Jenis Penerima = Dokter, lalu Kelengkapan = Belum lengkap, lalu kata kunci sebagian nama. Harapan: hasil menyempit sesuai; tombol atur ulang mengembalikan.
3. Ubah jumlah baris dan pindah halaman. Harapan: halaman benar, tidak ada baris ganda.
4. Buka impor, unduh/lihat templat, pilih berkas satu baris orang yang lengkap di HRD, tekan Periksa Data. Harapan: hasil uji kering; tidak ada data tersimpan.
5. Tekan Simpan Data. Harapan: toast sukses; daftar dimuat ulang; orang itu `hasPayrollProfile`.
6. Impor ulang berkas yang sama. Harapan: baris "Sudah ada (dilewati)" dengan peringatan.
7. Berkas dua baris: satu valid, satu orang tanpa rekening di HRD. Harapan: daftar baris gagal; baris valid tidak tersimpan.
8. Berkas dengan PPh "abc". Harapan: ditahan di layar sebelum dikirim.
9. Akun tanpa `Import`. Harapan: tombol impor tidak tampil. Akun tanpa `Read`: butir menu tidak tampil; membuka alamat langsung menampilkan akses ditolak.

**Tidak dijalankan:** pemeriksaan visual, uji manual langkah 1–9, suite penuh.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar dan ringkasan memuat seluruh pegawai dan dokter aktif dengan filter dan paginasi | Terpetakan ke source; belum diuji manual | Bagian 3, 5 |
| 2. Status kelengkapan dan alasan tampil per orang | Terpetakan ke source; belum diuji manual | Bagian 3, 5 |
| 3. Impor dua tahap (uji kering lalu simpan); baris gagal tampil dengan nomor dan sebab | Terpetakan ke source; unit lulus; belum diuji manual | Bagian 5 |
| 4. Butir menu dijaga `Read`; tombol impor dijaga `Import` | Terpetakan ke source dan uji kontrak | Bagian 5 |
| 5. Empat keadaan (memuat, kosong, gagal, berisi) | Terpetakan ke source; belum dilihat | Bagian 5 |
| DoD: ESLint 0 galat | Terpenuhi | Bagian 5 |
| DoD: build | Terpenuhi menurut pemilik; tanpa bukti keluaran | Bagian 5 |
| DoD: pemeriksaan visual dan uji manual | **Belum terpenuhi** | `NOT FEASIBLE` / `NOT RUN` |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

Kriteria di atas **disusun dari perilaku source**, bukan dari kontrak atau skema layar yang disetujui, karena keduanya belum ada.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Pendaftaran terlambat | Source ditulis dan di-commit pada 2026-11-16 sebelum task ini didaftarkan; ID `PAY-FE-21` dan statusnya diberikan setelahnya atas permintaan pemilik. Gerbang keputusan base component dan skema layar tidak dijalankan sebelum penulisan |
| Gerbang `G1` | Kontrak `PGJ-C1` dan `03-frontend-architecture.md` tidak memuat layar ini. Perlu amandemen kontrak dan skema layar, lalu persetujuan pemilik (`design-business-module`, model minimum Claude Opus 5) |
| Requirement dan keputusan | Belum ada FR-PGJ dan PRL-DEC bernomor; tidak dikarang |
| Urutan kerja | HR melengkapi dan menyetujui data penggajian (S-A8) → Payroll impor profil di layar ini → Simulasi. Per 2026-11-16 data produksi belum memiliki penetapan gaji dan rekening terverifikasi, sehingga impor ditolak sampai HR mengisinya |
| Hak akses baru | Admin perlu mencentang `PayrollDataReadiness : Read` dan `Import` di Pengaturan > Manajemen Role > Akses Role; daftar izin dimuat sekali per sesi (login ulang) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pemilik. Tidak ada yang di-stage atau di-commit oleh pembuat laporan ini |
| Langkah berikutnya | Pemilik: uji manual langkah 1–9 setelah `PAY-BE-24` lolos uji API; putuskan penulisan kontrak amandemen. Setelah itu `PAY-FE-21` dapat naik ke ✅ |
