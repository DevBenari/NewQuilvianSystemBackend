# Laporan Perubahan Frontend — `PAY-FE-FIX-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-002` (ad-hoc, bukan kartu roadmap) |
| Judul | Detail Profil Penggajian: kepala halaman hanya Kembali, Kembali selalu ke tabel, dan bagian "Tunjangan Pegawai" di bagian bawah |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas perintah pemilik; roadmap dan traceability tidak disentuh (preseden `PAY-FE-FIX-001`) |
| Trace | Perintah pemilik 2026-11-17 (master prompt "Penambahan Section Tunjangan Pegawai pada Detail Profil Penggajian"); branch Frontend `yasmina` dikonfirmasi pemilik (prompt menulis `yasmin`, 404 di GitHub; checkout lokal `yasmina`) |
| Dependency | Backend `PAY-BE-FIX-002` (source selesai, **belum dibuild**, **tabel `PrlWorkforceAllowance` belum ada**). Tanpa itu daftar dan ringkasan menjawab galat dan section menampilkan Coba lagi |
| Task mode | `FRONTEND` (Backend read-only) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-17 |
| Status | 🟡 **Source selesai; lint dan uji unit lulus. `npm run build`, `npm run dev`, dan uji peramban NOT RUN** (dilarang pada task ini). `VISUAL CHECK: NOT FEASIBLE` — server dev tidak dijalankan, jadi tampilan, modal, dan responsif belum pernah dilihat |

---

## 1. Yang dikerjakan

- **FE-001/FE-010 Kepala halaman.** Tombol "Tunjangan Transportasi" dihapus dari `renderDetailActions`; sisa "Kembali" dan "Coba Lagi" (hanya bila API gagal). `goBack` kini selalu `router.push(PAYROLL_ROUTES.compensations)`; `router.back()` dan cabang `window.history` dihapus. Filter tabel Profil Penggajian disimpan di state layar daftar (bukan URL), jadi tidak ada query yang dapat dibawa.
- **FE-002 Section "Tunjangan Pegawai"** di bagian paling bawah detail (sesudah "Kesiapan Data"): judul, deskripsi, ringkasan (Tunjangan Aktif, Menunggu Persetujuan, Total Estimasi / Bulan, Sumber), tabel (No., Jenis Tunjangan, Sumber, Metode, Nominal/Persentase, Berlaku, Status, Aksi), tombol "+ Tambah Tunjangan" dan "Kelola Transportasi". Berdiri sendiri dari profil payroll (kuncinya `workforceProfileId`).
- **FE-003/FE-004 Modal Tambah/Ubah.** `ConfirmModal` lebar 840px dua kolom (satu kolom di bawah 640px). Pegawai, kategori, dan metode hanya-baca. Jenis tunjangan dari registry pilihan `allowanceTypes` (endpoint options Master Tunjangan yang sudah ada, `onlyActive`, berhalaman 25, bercari); jenis `PolicyBased`/`ManualInput` atau kategori kebijakan dinonaktifkan di pilihan. Nominal/persen bawaan master mengisi isian. Jenis tidak dapat diganti saat ubah.
- **FE-005 Sumber dibedakan.** "Dari Struktur Gaji" hanya baca dengan aksi "Lihat Struktur" (ke pengelolaan Struktur Gaji); "Khusus Pegawai" dengan aksi dari `availableActions` Backend (Ubah, Ajukan, Setujui, Tolak, Nonaktifkan); "Berbasis Kebijakan" (transportasi) dengan aksi "Kelola Transportasi" ke fitur existing bagi pemegang `WorkforceTransportAllowance : Read`. Tunjangan variabel per periode dijelaskan lewat catatan dari Backend, tidak disimpan sebagai tunjangan berulang.
- **FE-007 Pengambilan data.** Ringkasan, daftar, dan master masing-masing punya loading, kosong, galat, dan Coba lagi. Setelah simpan atau aksi berhasil, ringkasan dan daftar dimuat ulang dari Backend (tanpa hitung ulang di layar, tanpa reload penuh). Galat API tidak ditampilkan sebagai data kosong.
- **FE-008 Aksi dan status.** Ajukan/Setujui lewat dialog konfirmasi; Tolak/Nonaktifkan lewat dialog dengan alasan wajib. Tidak ada tombol hapus. 409 memuat ulang daftar dan menutup form bila barisnya basi.
- **Privasi.** Nominal, persen, dan total tampil "Tidak ditampilkan" bila Backend mengirim null (tanpa `ViewAmount`); tidak pernah Rp 0.

## 2. File

Baru: `src/lib/constants/payroll/payroll-workforce-allowance-constants.js`, `src/utils/payroll/payroll-workforce-allowance-utils.js`, `src/lib/services/payroll/payroll-workforce-allowance-service.js`, `src/lib/hooks/payroll/use-payroll-workforce-allowance.js`, `src/components/view/payroll/compensation/payroll-workforce-allowance-section.jsx`, `...-form-modal.jsx`, `...-table-columns.jsx`, `src/style/payroll/payroll-workforce-allowance.module.css`, `tests/unit/payroll-workforce-allowance.test.mjs`.
Diubah: `src/components/view/payroll/compensation/payroll-compensation-detail-view.jsx`, `src/lib/hooks/payroll/use-payroll-compensation-detail.js`, `tests/unit/payroll-compensation-profile.test.mjs` (asersi tombol transportasi diganti asersi kepala halaman baru).

## 3. Tabel keputusan base component (`UI GATE`)

`UI GATE: 10 elemen — REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0` (CSS Module baru hanya untuk lebar dan tata letak modal).

| Kebutuhan UI | Base | Status | Sumber gaya |
| --- | --- | --- | --- |
| Kepala halaman | `BaseDetailView` (`renderDetailActions`) | REUSE | bawaan |
| Bagian bertajuk | `PayrollSection` (pembungkus Payroll yang sudah ada) | REUSE | `payroll-run.module.css` |
| Ringkasan | `PayrollInfoGrid`, `PayrollNoticeList` | REUSE | `payroll-run.module.css` |
| Tabel | `DataTable` | REUSE | `administrator-region-table.module.css` |
| Status | `StatusBadge` + konfigurasi badge | REUSE | bawaan |
| Tombol aksi | `BaseButton` | REUSE | bawaan |
| Modal | `ConfirmModal` (+ `requireReason` untuk tolak/nonaktifkan) | REUSE | CSS Module domain (urutan 3, lihat di bawah) |
| Isian | `BaseSelectField`, `BaseTextField`, `BaseDateField`, `BaseTextAreaField` | REUSE | bawaan |
| Peringatan dan galat | `InformationAlert`, `PayrollActionError` | REUSE | bawaan |
| Notifikasi | `ToastStack` | REUSE | bawaan |

Urutan gaya untuk modal: (a) bawaan tidak cukup karena `modal-confirm.css` mengunci dialog 430px dan `size="lg"` tidak berpengaruh; (b) tidak ada CSS Module tetangga untuk modal dua kolom lebar; (c) satu CSS Module `payroll-workforce-allowance.module.css` memakai pola `.modal.modal` yang sama dengan `workforce-payroll-data-edit-modal.module.css` (tanpa `!important`, seluruh nilai token).

## 4. Bagian 3.4 — pemeriksaan UI

| Pemeriksaan | Hasil |
| --- | --- |
| `check-css-usage.mjs` pada 4 berkas JSX yang diubah | `OK — 4 berkas diperiksa, 0 temuan.` |
| Grep anti-regresi (Tailwind, `!important`, `style={{}}`, literal warna) pada diff | 0 temuan; uji menjaga CSS bebas `!important` dan literal warna |
| ESLint pada 9 berkas | 0 galat, 0 peringatan |
| Impor internal | 0 tidak terselesaikan |
| Screenshot | **Tidak ada.** `VISUAL CHECK: NOT FEASIBLE` — `npm run dev` dilarang pada task ini |
| `MANUAL TEST` | **NOT FEASIBLE** — alasan sama |

## 5. Validasi

| Command | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-workforce-allowance.test.mjs tests/unit/payroll-compensation-profile.test.mjs` | **PASS** — 43 dari 43, 0 dilewati (termasuk pencocokan rute, izin, nama aksi, status, dan properti DTO dengan source Backend) |
| Suite unit penuh | 5.068 uji, 5.016 lulus, **52 gagal** — jumlah dan sifat sama dengan sebelum task ini (modul lain: berkas hilang dan sejenisnya); 0 kegagalan baru pada Payroll/tunjangan |
| `npm run build` | **NOT RUN** |
| `npm run dev` / uji peramban | **NOT RUN** |

Pemeriksaan statis atas TC: TC-01 (`router.push` ke tabel, tanpa `router.back`), TC-02 (tombol transportasi tidak ada di kepala), TC-03 (section terakhir), TC-04/05 (opsi dari master, jenis kebijakan dinonaktifkan), TC-06 (section tidak bergantung profil payroll), TC-17/18 (aksi mengikuti `availableActions` dan nominal null), TC-19 (galat tidak jadi data kosong), TC-21 (daftar dimuat ulang dari Backend). **Belum dibuktikan saat dijalankan.**

## 6. Risiko, delta, dan dependency

- **Dependency Backend:** DDL `PrlWorkforceAllowance` harus diterapkan sebelum Backend dan Frontend dirilis (lihat `PAY-BE-FIX-002` bagian 6). Tanpa itu section menampilkan galat dengan Coba lagi.
- **Delta terhadap prompt:** pilihan jenis memakai endpoint options Master Tunjangan yang sudah ada, yang tidak mengirim `RequiresApproval`/`MaximumAmount`; karena itu status persetujuan dan batas maksimum diputuskan Backend dan ditampilkan sesudah simpan. Pengguna butuh izin `AllowanceType : Read` agar pilihan termuat (pesan jelas bila tidak).
- **Prompt menyebut `GET /options` di `payroll-management/master-data/allowance-types`:** registry pilihan memakai alias `human-resource/master-data/allowance-types` (Backend melayani keduanya).
- **Daftar dimuat satu halaman (maks 100 baris);** bila lebih, tampil peringatan. Tabel per pegawai jarang melewati batas itu.
- **Tampilan belum pernah dilihat:** lebar modal, urutan kolom tabel pada layar sempit, dan penumpukan tombol aksi per baris perlu dilihat pemilik.
- Aksi "Lihat Struktur" membuka detail Struktur Gaji lewat UUID (diterima fallback rute); halaman itu menegakkan izinnya sendiri.
- `git status`: tidak ada yang di-stage atau di-commit. Perubahan tab Pajak (tugas sebelumnya) masih berada di working tree yang sama.
