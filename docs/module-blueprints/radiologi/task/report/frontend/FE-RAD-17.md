# Laporan Perubahan Frontend — `FE-RAD-17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-17` |
| Judul | Detail Pasien Radiologi, aksi baris v1, dan penanda fitur v1 yang belum tersedia |
| Slice | Paritas v1, Fase 4a + 4b — `RAD-EVD-001` bagian 7 |
| Trace | `RAD-DEC-021` (ikut v1), `RAD-DEC-022` (penanda "Fitur belum tersedia") |
| Contract | `RAD-API-001` revision 15 (`BE-RAD-20`) |
| Task mode | `FRONTEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `478f08aa9`, branch `YogaV2` |
| Tanggal | 2026-10-15 |
| Status | **Selesai — batas verifikasi.** API tiruan 63/63; terhadap devYoga menunggu migration |

---

## 1. Alur v1 yang dipasang

| Layar | Alur |
| --- | --- |
| Daftar Pasien — aksi baris | **Konfirmasi** (membuka Detail Pasien, seperti v1), Buka Pemeriksaan (V2), Batalkan Order, **Kirim Hasil**, **Print Hasil**, **Rincian Pasien**, **Print Label** |
| Riwayat — aksi baris | **Detail**, **Cetak Hasil** (persis v1) |
| Detail Pasien Radiologi (`rad-patients/[radOrderId]`) | Informasi Pasien; Informasi Order + tarif + **Petugas Radiologi**; tabel Cito / Pemeriksaan / No. Foto / Foto PACS / **Dokter Pemeriksa** / Hasil (→ konsol) / Cetak Film / Aksi; Print Label, Rincian Pasien, Simpan, **Konfirmasi** (simpan penunjukan lalu `accept`) |
| Kirim Hasil | `wa.me/62…?text=…` dengan pesan v1; tanpa nomor → peringatan |
| Print Hasil | Hanya **versi berlaku yang sudah dirilis** (`readDisplayContent`, `RAD-DEC-003`); tanpa rilis → "Hasil belum tersedia" |
| Rincian Pasien | Nota v1 (RS MMC, NOTA PASIEN RADIOLOGI, info pasien, Pemeriksaan + tarif, TOTAL, tanda tangan Petugas Radiologi & Kasir). Tarif kosong → "Belum diatur", tidak pernah 0 |
| Print Label | Label v1: No. RM, nama, umur, tanggal, No. Order, pemeriksaan, kode order |

## 2. Penanda "Fitur belum tersedia" (`RAD-DEC-022`)

Satu daftar: `src/lib/constants/health-services/radiology-management/rad-pending-feature-constants.jsx`. Setiap butir
membawa alur v1-nya; hapus butirnya saat fitur dibangun.

| Tempat | Butir |
| --- | --- |
| Menu | Laporan (Statistik Pemeriksaan, Dokter Pengirim, Pasien Perusahaan), Pemeriksaan (Harian, Per Item), Setup (Kategori, Parameter, Tarif Tindakan, Template Hasil) → `/radiology-management/pending/[feature]` |
| Daftar Pasien / Riwayat | Tab **Pasien Luar** — tidak meminta ke server |
| Detail Pasien | Foto Radiologi (PACS), Cetak Film, Unggah Gambar, Gunakan Hasil AI, Ubah Pemeriksaan — kolom berlencana, aksi terkunci berlabel |

## 3. Berkas

Baru: `rad-patients/[radOrderId]/page.jsx`, `pending/[feature]/page.jsx`, `rad-patient-detail-view.jsx`,
`shared/rad-pending-feature{,-view}.jsx`, `use-rad-patient-detail.jsx`, `use-rad-row-actions.jsx`,
`rad-patient-print-rules.js`, `rad-pending-feature-constants.jsx`, `rad-patient-detail.module.css`,
`tests/unit/rad-patient-print-rules.test.mjs`. Diubah: `rad-patient-list-view.jsx`, `use-rad-patient-list.jsx`,
`rad-patient-list-rules.js`, `rad-order.service.js` (`assignRadOrder`), `radiology-constants.jsx`
(`patientDetail`), `health-service-select-resources.js` (`radExaminerDoctors`, `radRadiographers`), `menu-items.jsx`.

## 4. Validasi

| Bukti | Hasil |
| --- | --- |
| Uji unit Radiologi baru (print, list, dashboard) | 23/23 lulus |
| ESLint seluruh folder Radiologi | 0 error; 9 warning lama, nol dari berkas baru |
| `npm run build` | Lulus |
| Layar standalone 3710, API tiruan penuh | **63/63 lulus** — termasuk Detail (simpan dokter lalu `accept`, 409 tetap membuka modal), Rincian/Print Hasil di jendela cetak, WhatsApp ke `phone=6281211112222` dengan pesan v1, tab Pasien Luar tanpa permintaan, halaman menu bertanda, aksi Riwayat |

## 5. Batas verifikasi

~~Belum dibuka terhadap devYoga: dua migration Radiologi belum diterapkan.~~ Kedua migration diterapkan 2026-10-15
dan layar diuji terhadap devYoga (bagian 6.1). Sisa batas: panel Gambar Radiologi menunggu migration
`AddRadOrderImage`.

---

## 6. Lanjutan 2026-10-15

### 6.1 Uji layar dengan data devYoga

Backend lokal tersambung ke devYoga, izin disimulasikan tepat 13 izin *Penunjang Medis / Radiografer*
(`isSuperAdmin: false`), tulis selain penugasan pesanan uji diblokir. **17/17 lulus**: Beranda, Daftar (tab
Perjanjian), Rincian Pasien (tarif "Belum diatur"), Print Hasil menolak tanpa bacaan rilis, Kirim Hasil ke nomor
pasien uji, Detail (Petugas terisi pengguna login, dokter yang dipilih = yang tersimpan), Riwayat → Detail, halaman
menu bertanda. Penugasan pesanan uji `d27c12cc…` dikembalikan kosong sesudahnya.

### 6.2 Gambar Radiologi (`BE-RAD-22`)

Panel **Gambar Radiologi** di Detail Pasien menggantikan penanda "Unggah Gambar — Fitur belum tersedia": unggah
banyak JPG/PNG (validasi di layar: format, 10 MB, 20 gambar), pratinjau dari endpoint berizin (object URL dilepas
saat keluar), buka gambar, hapus dengan konfirmasi. Berkas: `rad-order-images-panel.jsx`, `use-rad-order-images.jsx`,
`rad-image-rules.js` (+ uji 3), service gambar di `rad-order.service.js`.

Validasi: uji unit Radiologi 26/26; lint bersih; build lulus; layar API tiruan **69/69** (termasuk unggah dua gambar,
PDF ditolak tanpa permintaan, hapus). Terhadap devYoga menunggu migration `AddRadOrderImage`.
