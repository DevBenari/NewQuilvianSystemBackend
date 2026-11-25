# Laporan Perubahan Frontend — `PAY-FE-FIX-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-022` (ad-hoc lanjutan seri `PAY-FE-FIX`); pasangan `PAY-BE-FIX-021` dan `PAY-BE-FIX-022` |
| Judul | Modal Tambah Tunjangan: Jenis Tunjangan lalu Kebijakan Tunjangan, nilai hanya baca dari Backend; panel besar dan CTA transportasi dihapus |
| Slice | Payroll Management → Detail Profil Penggajian → Tunjangan Pegawai |
| Branch / HEAD | FE `yasmina` `ab5843be747a32b04c90ac1a63236835234149d6` (dikonfirmasi pemilik) |
| Klasifikasi / mode | `HEAVY` / `FRONTEND` |
| Tanggal | 23 November 2026 |
| Status | 🟡 Source, tes teks, dan lint selesai; **tampilan belum dilihat** (`VISUAL CHECK: NOT FEASIBLE`), build tidak dijalankan. **Bergantung pada DDL `PAY-BE-FIX-021` dan backend yang dibuild** |

## 1. Hasil untuk pengguna

- Tombol **+ Tambah Tunjangan** membuka modal: **Pegawai** (hanya baca), **Grade Gaji** (hanya baca, dari penetapan gaji utama), **Jenis Tunjangan** (semua jenis aktif; yang belum dapat ditetapkan tetap terlihat, nonaktif, beserta alasan dari Backend), **Kebijakan Tunjangan** (hanya kebijakan yang cocok dengan jenis dan Grade Gaji; terkunci sampai jenis dipilih), lalu **Nominal/Persentase, Metode, Batas Maksimum, Masa Berlaku** hanya baca dari hasil resolusi Backend.
- Tidak ada isian nominal, persentase, kategori, metode, atau tanggal. Simpan hanya mengirim `allowanceTypeId` dan `allowancePolicyId`.
- Ganti jenis: kebijakan dan preview lama dihapus, permintaan lama dibatalkan, kebijakan dimuat ulang. Satu kebijakan sah dipilih otomatis (nilai tetap dari Backend). Tanpa kebijakan: pesan pendek pada area dropdown ("Belum tersedia kebijakan aktif untuk jenis tunjangan dan Grade Gaji ini."), tombol simpan nonaktif.
- Masa berlaku ditulis eksplisit; tanpa tanggal akhir tampil "Tanpa tanggal akhir" (tidak diasumsikan sah).
- Tanpa izin `ViewAmount`: "Tidak ditampilkan" (bukan Rp 0). Galat tiap tahap punya Coba lagi; 403 tanpa Coba lagi. **Batal selalu menutup modal tanpa permintaan simpan** (kecuali sedang menyimpan).
- **Dihapus:** panel "Belum ada jenis tunjangan yang dapat ditetapkan langsung kepada pegawai." beserta tombol Buka Master Tunjangan/Muat ulang, alert Transportasi dan tombol "Buka Penetapan Transportasi Pegawai", tombol **Kelola Transportasi** pada kepala bagian, **seluruh layar transportasi terpisah** (route, tampilan, hook, service, util, konstanta; 8 berkas), serta util/konstanta/CSS/hook yang hanya melayaninya.
- Penetapan transportasi **lama** yang masih aktif tampil di daftar dengan tombol **Nonaktifkan** (alasan wajib; memakai `PATCH .../transport-allowance/{id}/status`, izin `WorkforceTransportAllowance : Update`). Data tidak dihapus.
- Form **Ubah** tetap untuk penetapan langsung yang sudah ada (jenis hanya baca). Penetapan "Dari Kebijakan" tidak dapat diubah (Backend tidak menawarkan aksi Ubah).

## 2. Berkas

Baru: `src/components/view/payroll/compensation/payroll-workforce-allowance-policy-modal.jsx`, `src/lib/hooks/payroll/use-payroll-workforce-allowance-policy-assignment.js`, `src/utils/payroll/payroll-allowance-policy-assignment-utils.js`, `tests/unit/payroll-allowance-policy-assignment-ui.test.mjs`.
Diubah: `payroll-workforce-allowance-form-modal.jsx` (kini khusus ubah), `payroll-workforce-allowance-section.jsx`, `use-payroll-workforce-allowance.js`, `payroll-workforce-allowance-service.js` (4 pemanggilan), `payroll-workforce-allowance-constants.js`, `payroll-workforce-allowance-utils.js` (10 fungsi tak terpakai dihapus; `validateAllowanceForm` tidak lagi memanggil fungsi yang dihapus), `payroll-workforce-allowance.module.css` (kelas panel kosong dihapus), `tests/unit/payroll-workforce-allowance.test.mjs`, `payroll-allowance-policy-grade-sync.test.mjs`.

## 3. Penghapusan transportasi terpisah (kondisi henti dinilai ulang)

| Butir | Keputusan |
| --- | --- |
| Tombol Kelola Transportasi di kepala bagian, CTA di modal | **Dihapus** |
| Route `/payroll/workforce-compensations/[id]/transport-allowance` dan view/hook/service/util/konstanta-nya | **Dihapus** (8 berkas, tidak ada impor tersisa menurut pencarian `src` dan `tests`) |
| Kemampuan menonaktifkan penetapan lama (alasan route sebelumnya dipertahankan) | **Dipindahkan** ke daftar Tunjangan Pegawai (tombol Nonaktifkan); pembayaran penetapan lama yang masih aktif tetap berjalan lewat mesin payroll yang tidak diubah untuk jalur itu |
| Membuat penetapan transportasi baru di jalur lama | **Dinonaktifkan di Backend** (409, lihat `PAY-BE-FIX-022`) |
| Mengubah tarif/tanggal penetapan lama dari UI | **Tidak ada lagi** (hanya Nonaktifkan; `PUT` masih ada di API) — **NEEDS CONFIRMATION** |
| Transportasi per kehadiran/per hari baru | **Tidak dapat dibuat lagi** (belum didukung alur umum) — **NEEDS CONFIRMATION** |

## 4. Gerbang UI

Semua elemen `REUSE` (`ConfirmModal`, `BaseSelectField`, `BaseTextField`, `InformationAlert`, `PayrollActionError`, `payroll-workforce-allowance.module.css` yang sudah ada); tidak ada komponen/CSS baru. `UI GATE: PASS (semua REUSE)`. `check-css-usage.mjs` pada 3 berkas JSX: `OK — 3 berkas diperiksa, 0 temuan`.

## 5. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test` pada `payroll-workforce-allowance`, `payroll-allowance-policy-grade-sync`, `payroll-allowance-policy-assignment`, `payroll-allowance-policy-assignment-ui`, `payroll-compensation-profile` | **69 lulus, 0 gagal** (tes teks/util; bukan tampilan) — dijalankan dari root workspace |
| `payroll-*.test.mjs` seluruhnya (sesudah penghapusan layar) | 487 lulus, **3 gagal** (sama dengan sebelum penghapusan) di luar berkas yang saya ubah: `payroll-module-contract` (menu di luar `/payroll/`, komponen mentah di `workforce-config-status-header.jsx`) dan `payroll-run-screens` (butir menu Payroll Run). **Belum dibandingkan terhadap HEAD bersih** |
| `npx eslint` berkas baru/diubah | 0 temuan (catatan: konfigurasi tidak menandai fungsi yang tak terdefinisi; kesalahan itu tertangkap oleh tes, bukan lint) |
| Build / dev server / tampilan / uji manual | `NOT RUN` / `VISUAL CHECK: NOT FEASIBLE` / `MANUAL TEST: NOT FEASIBLE` |

## 6. Risiko dan batas

- Tampilan modal, perilaku dropdown, dan balapan permintaan belum dilihat di peramban.
- Urutan deploy: DDL `PAY-BE-FIX-021` → backend `PAY-BE-FIX-021/022` → frontend. Tanpa data kebijakan umum (belum ada layar pengelola; template SQL di `PAY-BE-FIX-021` §5.1) dropdown kebijakan kosong.
- Izin: dropdown memakai `WorkforceAllowanceAssignment` Read; akun tanpa izin Tambah tidak melihat tombol.
