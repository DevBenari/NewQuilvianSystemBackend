# Laporan Perubahan Frontend — `FE-OPT-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-033` |
| Judul | E2E per UAT `UAT-01`..`26` |
| Slice | Seluruh layar Optik; penutup roadmap frontend |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) |
| Trace | `UAT-01`..`26` (`04-prd-to-mvp.md` bagian 18); `testing/acceptance-test-matrix.md` |
| Contract version | Seluruh kontrak `OPT-*-v1` dan delta `OPT-API-v1.1`, tanpa perubahan |
| Wewenang UI | Tidak ada perubahan layar pada task ini |
| Dependency | `FE-OPT-032` ✅ |
| Klasifikasi | `SMALL` — hanya berkas uji: 1 spec baru, 5 spec bertambah skenario, 6 spec bertambah label |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/tests/e2e`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010`..`032` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai untuk bukti layar dengan API tiruan: 24 dari 26 UAT lulus. `UAT-19` belum terpenuhi dan `UAT-26` belum dapat diuji. **Belum ada satu pun UAT yang dijalankan dari ujung ke ujung dengan backend dan data asli.** Belum di-commit |

---

## 1. Cara membaca hasil

Setiap UAT dibuktikan pada dua lapis yang terpisah, dan keduanya belum pernah dijalankan bersama:

| Lapis | Yang dibuktikan | Yang tidak dibuktikan |
| --- | --- | --- |
| **Layar** (E2E, API tiruan) | Kiriman layar persis, jawaban server tampil benar, tombol dan penolakan sesuai | Bahwa server benar-benar menjawab begitu |
| **Backend** (harness, laporan `BE-OPT-*`) | Aturan, stok, tagihan, dan status | Bahwa layar benar-benar memanggilnya |

Sambungan keduanya hanya diperiksa sebagian: bentuk kiriman ke backend lokal dengan id acak (`FE-OPT-010`..`032`). Alur tulis asli belum dapat dijalankan karena kode profesi RO/Sp.M kosong dan devYoga belum punya barang Optik, pesanan, serta akun petugas (bagian 5).

Seluruh skenario berlabel dijalankan dengan:

```text
node node_modules/@playwright/test/cli.js test tests/e2e/optical-*.spec.mjs --grep "UAT-" --reporter=line
```

---

## 2. Matriks UAT

| UAT | Jalur | Inti | Bukti layar | Bukti backend | Hasil |
| --- | --- | --- | --- | --- | --- |
| `UAT-01` | Berhasil | Kunjungan tampil di daftar | `optical-visit`: **UAT-01** (baru) | `BE-OPT-010` | Lulus |
| `UAT-02` | Gagal | Rujukan pasien lain ditolak | `optical-visit`: **UAT-02** (baru) | `BE-OPT-003` | Lulus |
| `UAT-03` | Berhasil | Tiga versi di riwayat; satu tarif | `optical-refraction-exam`: riwayat UAT-03 | `BE-OPT-011`, `031` (satu tarif) | Lulus; tarif di backend saja |
| `UAT-04` | Gagal | SIP berakhir, tanggal tampil | `optical-refraction-exam`: **UAT-04** (baru) | `BE-OPT-011` | Lulus |
| `UAT-05` | Berhasil | Transposisi resep luar | `optical-prescription`: resep luar UAT-05 | `BE-OPT-012` | Lulus |
| `UAT-06` | Gagal | Revisi resep terkunci | `optical-prescription`: revisi UAT-06 | `BE-OPT-012` | Lulus |
| `UAT-07` | Berhasil | Kirim ke bengkel | `optical-order`: kirim UAT-07 | `BE-OPT-034` (stok) | Lulus; stok di backend saja |
| `UAT-08` | Gagal | Uang muka kurang | `optical-order`: kirim UAT-07 | `BE-OPT-034` | Lulus |
| `UAT-09` | Gagal | Pesanan pertama belum lunas | `optical-order`: kirim UAT-07 | `BE-OPT-034` | Lulus |
| `UAT-10` | Berhasil | Penjualan langsung sampai serah | `optical-direct-sale`: form UAT-10, detail UAT-10 | `BE-OPT-042` (stok) | Lulus; stok di backend saja |
| `UAT-11` | Gagal | Lensa kontak tanpa resep | `optical-direct-sale`: form UAT-11, **UAT-11** (baru) | `BE-OPT-042` | Lulus |
| `UAT-12` | Berhasil | Serah; tagihan final | `optical-order-fulfillment`: serahkan UAT-12 | `BE-OPT-040` | Lulus |
| `UAT-13` | Gagal | Belum lunas ditolak | `optical-order-fulfillment`: serahkan UAT-12, UAT-13 | `BE-OPT-040` | Lulus |
| `UAT-14` | Berhasil | Batal lensa custom | `optical-order-fulfillment`: **UAT-14** (baru) | `BE-OPT-041` (stok, piutang) | Lulus; stok dan piutang di backend saja |
| `UAT-15` | Gagal | Batal langsung tidak bisa | `optical-order-fulfillment`: **UAT-15** (baru) | `BE-OPT-041` | Lulus; lihat catatan (1) |
| `UAT-16` | Berhasil | Keluhan sampai remake | `optical-warranty-claim`: alur UAT-16, putuskan UAT-16 | `BE-OPT-050` | Lulus; Rp 0 di backend saja |
| `UAT-17` | Gagal | Di luar garansi | `optical-warranty-claim`: putuskan UAT-17 | `BE-OPT-050` | Lulus |
| `UAT-18` | Berhasil | Lunas, kunjungan tetap terbuka | `optical-order-fulfillment`: **UAT-18** (baru) | `BE-OPT-030` | Lulus; lihat catatan (2) |
| `UAT-19` | Gagal | Diskon di atas batas menunggu persetujuan | — | — | **Belum terpenuhi** (`OPT-APR-004`, `POST-MVP`) |
| `UAT-20` | Berhasil | Visus dan resep di Rekam Medis | `optical-medical-record-uat`: **UAT-20** (baru) | `BE-OPT-013` (tanpa harga) | Lulus; lihat catatan (3) |
| `UAT-21` | Gagal | Tanpa izin Rekam Medis | `optical-medical-record-uat`: **UAT-21** (baru) | Belum: penjaga milik Rekam Medis | Layar lulus; penjaga asli belum terbukti |
| `UAT-22` | Berhasil | Perlu dihubungi | `optical-report`: pesanan terbuka UAT-22 | `BE-OPT-052` | Lulus |
| `UAT-23` | Gagal | Laporan ditolak | `optical-report`: izin UAT-23 | `BE-OPT-052` | Lulus |
| `UAT-24` | Berhasil | Ubah pengingat, riwayat | `optical-master-data`: pengaturan UAT-24 | `BE-OPT-005` (pembekuan nilai) | Lulus |
| `UAT-25` | Gagal | Batas tutup ditolak | `optical-master-data`: pengaturan UAT-25 | `BE-OPT-005`; backend lokal `FE-OPT-031` | Lulus |
| `UAT-26` | Berhasil | Barang Optik tidak tampil di peresepan | Tidak ada layar Optik | Tidak ada uji; `PrescribingDrugController` menyaring `IsPrescribable` | **Belum dapat diuji:** devYoga belum punya barangnya |

`UAT-27` (migration `Down`) di luar cakupan task ini: milik `BE-OPT-002`, dan laporannya mencatat belum dijalankan karena menunggu izin penerapan.

**Catatan.**

1. **`UAT-15`:** brief tidak menyediakan tombol Batal pada pesanan dalam pengerjaan, sehingga petugas tidak pernah melihat kalimat *"perlu persetujuan Kepala Optik"* dari layar. Kalimat itu hanya terbukti di harness `BE-OPT-041`. Yang dibuktikan layar: tombolnya tidak ada walau petugas berizin `Cancel`.
2. **`UAT-18`:** yang menentukan kunjungan tetap terbuka adalah Billing. Layar hanya membuktikan aksi tetap *Tandai Siap* dan tidak ada pemberitahuan final.
3. **`UAT-20`:** tanpa harga dibuktikan di harness, karena isi dokumen datang dari server. Skenario layar memakai bentuk jawaban `OptMedicalRecordDocumentSource`, sehingga yang dibuktikan adalah layar Rekam Medis menampilkan jenis Optik tanpa kode tambahan. Pengecekan `Rp` pada skenario itu hanya menjaga agar layar tidak menambah harga sendiri. Metadata backend lokal terbukti memuat kedua jenis.

---

## 3. Perubahan yang dikerjakan

Tidak ada perubahan pada `src/`. Sembilan skenario baru menutup celah, dan skenario lama diberi label agar dapat dijalankan dengan `--grep "UAT-"`.

| Berkas | Perubahan |
| --- | --- |
| `tests/e2e/optical-medical-record-uat.spec.mjs` | **Baru.** `UAT-20`, `21` |
| `tests/e2e/optical-visit-screen.spec.mjs` | **Diperbarui.** `UAT-01`, `02` |
| `tests/e2e/optical-refraction-exam-screen.spec.mjs` | **Diperbarui.** `UAT-04`; label `UAT-03` |
| `tests/e2e/optical-direct-sale-screen.spec.mjs` | **Diperbarui.** `UAT-11`; jawaban pilihan resep dapat diganti per skenario |
| `tests/e2e/optical-order-fulfillment-screen.spec.mjs` | **Diperbarui.** `UAT-14`, `15`, `18`; label `UAT-12`, `13` |
| `tests/e2e/optical-order-screen.spec.mjs` | **Diperbarui.** Label `UAT-07` |

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| E2E berlabel UAT, 2 putaran | 50 lulus, 0 gagal (25 × 2) | `PASS` | Keluaran Playwright |
| E2E seluruh layar Optik atas build produksi | 146 lulus, 0 gagal | `PASS` | Keluaran Playwright |
| Uji mutasi di level layar: tombol Batal dimunculkan pada pesanan dalam pengerjaan, lalu build ulang | `UAT-15` gagal (`Expected 0, Received 1`); berkas dipulihkan, `cmp` identik, build ulang | `PASS` | Keluaran Playwright |
| Backend lokal: `GET medical-records/filters/metadata` | `200`; jenis 45 *Pemeriksaan Refraksi* dan 46 *Resep Optik* ada, `isIntegrityEnforced` benar | `PASS` | Skrip di scratchpad |
| `npx eslint --quiet` atas seluruh spec Optik | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.394 lulus, 72 gagal dari 4.470; daftar 40 berkas gagal **identik** dengan sesudah `FE-OPT-032` (`diff` kosong), tanpa berkas Optik | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` | `Compiled successfully in 68s`; exit 0 | `PASS` | Keluaran build |

**Durasi.** Satu putaran E2E penuh memakan 6,9 menit, sedangkan putaran sebelumnya sekitar 2 menit. Tidak ada kegagalan; sebabnya berjalan sesudah dua build berturut-turut dan tidak diselidiki lebih jauh.

Uji manual: `NOT FEASIBLE` untuk alur UAT dengan backend asli (bagian 5).

**Tidak dijalankan:** harness backend pada task ini. Source backend tidak berubah sejak `BE-OPT-053` (239 lulus), sehingga hasilnya dirujuk dari laporan. Berkas Rekam Medis asli juga tidak dibuka, karena setiap pembukaan menulis jejak akses.

---

## 5. Yang menahan uji ujung-ke-ujung

| Penahan | Dampak | Pemilik |
| --- | --- | --- |
| Kode profesi `OpticalManagement:Practitioner` kosong | Semua akun ditolak `OPT-VAL-001`; pemeriksaan, resep, dan periksa ulang tidak bisa ditulis | Yoga Aji Pratama |
| Tidak ada barang `IsPrescribable = false` di master | Katalog kosong, sehingga pesanan tidak bisa dibuat; `UAT-26` tidak dapat diuji | Farmasi / Master Data |
| Belum ada pegawai RO/Sp.M dengan SIP + STR berlaku, dan akun petugas berizin Optik | Izin asli tertutup superadmin; `UAT-04`, `21`, `23` belum terbukti dengan akun asli | SDM / Admin akses |
| Depo Optik dan stok | Potong stok asli (`UAT-07`, `10`, `14`) belum teruji | Farmasi |
| Tarif barang Optik dan kebijakan diskon | Tagih dan kasir asli belum teruji | Billing |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seluruh UAT yang punya layar lulus | Terpenuhi untuk 24 UAT lewat API tiruan; `UAT-19` belum terpenuhi | Bagian 2 |
| UAT backend-saja dirujuk ke laporan BE | Terpenuhi, kecuali `UAT-26` yang belum punya uji | Bagian 2 |
| Laporan lengkap | Terpenuhi | Laporan ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Roadmap frontend selesai, tetapi modul belum siap dipakai.** Layar lengkap dan teruji sendiri, begitu pula backend, tetapi keduanya belum pernah dijalankan bersama pada alur tulis (bagian 5) |
| Peringatan | **`UAT-19` belum terpenuhi.** Kebijakan diskon langsung berlaku tanpa persetujuan (`OPT-APR-004`, `POST-MVP`); perlu diketahui saat Billing menyiapkan kebijakan |
| Peringatan | **`UAT-21` belum terbukti pada penjaga asli.** Penjaganya milik Rekam Medis; catatan `BE-OPT-013` menyebut pembuktiannya bersama pemilik modul itu |
| Perlu konfirmasi pemilik | Aksi **Daftarkan Barang** di luar brief (`FE-OPT-031`) |
| Dependency backend | Tautan remake ke pesanan asal (`ParentOrderId`) dan nama pemeriksa (`FE-OPT-030`) belum tersedia. Tidak menahan UAT |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task |
| Perubahan sampingan | `playwright-report/index.html` (ter-track) tertimpa oleh `playwright test --list` tanpa `--reporter=line`; dipulihkan dengan `git checkout -- playwright-report/index.html` dan tidak ada perubahan lain padanya. Satu mutasi uji dibuat dan dipulihkan. Tidak ada tulisan ke devYoga |
| Interupsi | Satu perintah melewati batas 10 menit dan selesai di latar; tidak diulang |
| Status Git | Di luar berkas `FE-OPT-010`..`032`: `??` `tests/e2e/optical-medical-record-uat.spec.mjs`; ` M` `menu-items.jsx`; `src/` tidak berubah |
| Langkah berikutnya | Roadmap frontend selesai. Berikutnya verifikasi kesiapan modul (`/qv-verify`) dan uji ujung-ke-ujung sesudah penahan bagian 5 terisi |
