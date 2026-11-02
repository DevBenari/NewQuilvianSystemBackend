# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-023` |
| Judul | Sembunyikan info tanggungan asuransi di Resep RJ (`RJ-DOC-DEC-089`) |
| Slice | Amendment SF — Katalog Resep dari stok Farmasi Rawat Jalan |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian 21.2 |
| Trace | `RJ-DOC-DEC-089`, `RJ-DOC-DEC-090` |
| Contract version | Tidak ada perubahan kontrak. Ruas coverage `prescribing-drugs` tetap dikirim backend dan hanya tidak ditampilkan |
| Wewenang UI | Menyembunyikan label tanggungan dan pembatasan asuransi di tab Resep RJ; harga tampil sebagai "Harga satuan" (`RJ-DOC-DEC-089`) |
| Dependency | `RJ-DOC-REV-FE-022` ✅ |
| Klasifikasi | `LIGHT` — skor 3: satu repository (0), 9–20 berkas diperiksa (1), 4–8 berkas diubah (1), logika sederhana (0), tanpa kontrak (0), tanpa database (0), keamanan tidak berubah (0), satu workflow (1) |
| Task mode | `FRONTEND` |
| Target tulis | `V2QuilvianSystemFrontendDev/src/**` tab Resep RJ dan komponen bersamanya; laporan dan tautan bukti di blueprint `rawat-jalan` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `e3e5adc57` (branch `sukmagpV2`) + perubahan `RJ-DOC-REV-FE-022` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `0415bfc0` + perubahan `RJ-DOC-REV-BE-022` yang belum di-commit |
| Tanggal | 2026-10-28 |
| Status | ✅ Selesai, belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Data asuransi belum benar, tetapi tab Resep RJ sudah menampilkan hasil hitungan tanggungannya. Contoh pada ETOPUL 100 MG INJ:

- "Estimasi pasien Rp 0"
- "Ditanggung Rp 216.667"
- badge "Perlu persetujuan" dan "Perlu surat jaminan"

Angka seperti ini dapat menyesatkan dokter dan pasien. Info tanggungan muncul di enam tempat:

| Tempat | Info tanggungan yang tampil |
| --- | --- |
| Katalog Obat Biasa | Harga, badge Dicover/Perlu persetujuan, badge pembatasan |
| Katalog Racikan | Sama dengan katalog Obat Biasa |
| Form obat terpilih | Chip Ditanggung/Tidak Ditanggung |
| Tabel *Resep yang Dipilih* | Label "Estimasi pasien" |
| Modal *Detail* obat | Badge coverage, badge pembatasan, dan peringatan coverage |

---

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka tab **Resep** untuk pasien berasuransi.
2. Katalog menampilkan **Harga satuan Rp … per <satuan>**, tanpa estimasi pasien, nilai tanggungan, maupun badge
   pembatasan. Chip stok (`RJ-DOC-REV-FE-022`) tetap tampil.
3. Setelah dokter memilih obat, form kanan menampilkan "Harga: Rp … / <satuan>" tanpa chip Ditanggung/Tidak Ditanggung.
4. Tabel *Resep yang Dipilih* menampilkan "Subtotal", bukan "Estimasi pasien".
5. Modal *Detail* obat hanya menampilkan informasi klinis.
6. Setelah data asuransi benar, ubah `OUTPATIENT_PRESCRIPTION_SHOW_INSURANCE_COVERAGE` menjadi `true`. Seluruh info di
   atas tampil kembali tanpa perubahan lain.

Rawat Inap dan IGD tidak berubah. Komponen bersama baru menyembunyikan coverage bila menerima `showCoverage={false}`,
dan hanya layar RJ yang mengirim nilai itu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DoctorPrescriptionDrugRow.jsx`, `DoctorDrugDetailModal.jsx`, `DoctorPrescriptionDrugCatalogPanel.jsx`,
`doctor-prescription-presentation-utils.js`, `doctor-prescription-regular-panel.jsx`,
`doctor-prescription-compound-panel.jsx`, `doctor-prescription-tab.jsx`, `prescription.constants.js`, serta pemakai
komponen bersama di Rawat Inap (`prescription-builder-panel.jsx`, `prescription-drug-catalog-panel.jsx`) dan IGD
(`emergency-prescription-builder-panel.jsx`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/pharmacy-management/prescription.constants.js` | Konstanta `OUTPATIENT_PRESCRIPTION_SHOW_INSURANCE_COVERAGE = false` beserta catatan `RJ-DOC-DEC-089` |
| `src/components/ui/doctor-clinical-base/DoctorPrescriptionDrugRow.jsx` | `DoctorPrescriptionPriceSummary` menerima `showCoverage` (bawaan `true`). Bila `false`: tanpa estimasi pasien dan Ditanggung Rp; label "Harga satuan" di katalog dan "Subtotal" di order |
| `src/components/ui/doctor-clinical-base/DoctorPrescriptionDrugCatalogPanel.jsx` | Prop `showCoverage` (bawaan `true`); badge coverage dan pembatasan disembunyikan bila `false`; diteruskan ke ringkasan harga |
| `src/components/ui/doctor-clinical-base/DoctorDrugDetailModal.jsx` | Prop `showCoverage` (bawaan `true`); badge coverage, pembatasan, dan peringatan coverage disembunyikan bila `false` |
| `.../prescription/doctor-prescription-regular-panel.jsx` | Katalog, chip form obat terpilih, dan ringkasan harga tabel *Resep yang Dipilih* mengikuti konstanta |
| `.../prescription/doctor-prescription-compound-panel.jsx` | Katalog Racikan mengikuti konstanta |
| `.../prescription/doctor-prescription-tab.jsx` | Modal Detail obat mengikuti konstanta |

### 3.3 Kepatuhan arsitektur frontend

Tidak ada komponen, state, service, atau CSS baru. Tiga komponen bersama hanya mendapat prop opsional dengan bawaan
perilaku lama.

**Keputusan base component**

UI GATE: 4 elemen — REUSE 1, EXTEND 3, COMPOSE 0, WRAP 0, NEW 0

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Harga tanpa estimasi tanggungan | `DoctorPrescriptionPriceSummary` | Dipakai katalog RJ, tabel order RJ, dan `DoctorDrugCatalogModal` | EXTEND | Prop opsional `showCoverage`, bawaan `true` |
| Katalog tanpa badge | `DoctorPrescriptionDrugCatalogPanel` | Hanya dipakai RJ (Obat Biasa dan Racikan) | EXTEND | Prop opsional `showCoverage`, bawaan `true` |
| Modal detail tanpa badge | `DoctorDrugDetailModal` | Dipakai RJ, Rawat Inap, dan IGD | EXTEND | Prop opsional `showCoverage`, bawaan `true` |
| Chip form obat terpilih | Chip `counterChip_*` di view RJ | `doctor-prescription-regular-panel.jsx` | REUSE | Ditampilkan bersyarat lewat konstanta |

Pilihan untuk ketiga elemen `EXTEND`:

- **A. Prop opsional `showCoverage`, bawaan `true` — dijalankan.** Rawat Inap dan IGD tidak berubah; cukup membalik satu
  konstanta untuk memulihkan.
- **B. Hapus kode coverage langsung di komponen bersama.** Rawat Inap dan IGD ikut berubah di luar keputusan, dan
  memulihkannya butuh menulis ulang kode.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah dari `RJ-DOC-REV-FE-022` |
| Kosong | Tidak berubah |
| Gagal | Tidak berubah |
| Tanpa hak akses | Tidak berubah |
| Pasien berasuransi | Harga satuan saja; tanpa label tanggungan dan pembatasan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Prescribing Drug

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs` | Katalog; ruas coverage diterima tetapi tidak ditampilkan | `PrescribingDrug : Read` |
| `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs/{drugId}` | Modal Detail obat | `PrescribingDrug : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 9 berkas yang berubah | Tanpa temuan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 3.970 test: 3.929 lulus, 41 gagal; daftar kegagalan identik dengan run sebelum task ini | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar kegagalan kosong |
| `npm run build` | `Compiled successfully` | `PASS` | Keluaran perintah |
| Grep anti-regresi pada baris tambahan | 0 temuan untuk semua pola | `PASS` | Keluaran perintah |
| Uji layar Playwright | **6/6 PASS** | `PASS` | `pw_fe023.cjs`, `shots/fe023-*.png` di scratchpad sesi |
| Uji kontrol: konstanta sementara `true` | C1–C5 menampilkan kembali Ditanggung, Estimasi pasien, Perlu persetujuan, Perlu approval, Perlu surat jaminan, dan peringatan coverage; konstanta dikembalikan ke `false` dan uji utama diulang `6/6 PASS` | `PASS` | Keluaran perintah |

Uji layar: FE dev `localhost:3000`, API dialihkan ke backend uji `7185`, konsultasi tertunda pasien uji
"UJI GC PASIEN DAA8". Pasien uji membayar tunai, jadi respons katalog dan detail disuntik ruas coverage lengkap di
peramban. Tujuannya agar setiap label pasti punya data untuk tampil. Pilih hanya mengisi form lokal; tidak ada obat
yang ditambahkan ke resep.

| ID | Skenario | Hasil |
| --- | --- | --- |
| C1 | Katalog Obat Biasa tanpa label tanggungan/pembatasan | PASS |
| C2 | Harga tampil "Harga satuan Rp 497 per tab" | PASS |
| C3 | Form obat terpilih tanpa chip Ditanggung/Tidak Ditanggung | PASS |
| C4 | Modal Detail tanpa badge coverage/pembatasan dan peringatan coverage | PASS |
| C5 | Katalog Racikan tanpa label tanggungan/pembatasan | PASS |
| C6 | Chip stok `FE-022` tetap tampil | PASS |

MANUAL TEST: PASS — katalog, pencarian, Pilih, modal Detail, dan perpindahan ke Racikan diuji di layar.

AUTOMATED TEST: SKIPPED (opsional) — perubahan berupa tampilan bersyarat.

**Tidak dijalankan:**

- Tabel *Resep yang Dipilih* tidak diuji di layar, karena menambah obat ke resep menulis draft pasien uji. Dibuktikan
  lewat kode: `showCoverage` diteruskan ke `DoctorPrescriptionPriceSummary`, sehingga label menjadi "Subtotal".
- Rawat Inap dan IGD tidak diuji di layar. Dibuktikan lewat kode: keduanya tidak mengirim `showCoverage`, jadi bawaan
  `true` berlaku.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pasien berasuransi: katalog, form, dan modal detail tidak menampilkan Ditanggung/Tidak Ditanggung, Dicover, Estimasi pasien, Ditanggung Rp, Perlu persetujuan, Perlu surat jaminan | Terpenuhi | C1, C3, C4, C5 |
| 2. Harga tampil "Harga satuan Rp … per <satuan>" | Terpenuhi | C2 |
| 3. Membalik konstanta menampilkan kembali seluruh info | Terpenuhi | Uji kontrol |
| 4. Katalog Rawat Inap tidak berubah | Terpenuhi lewat kode | Rawat Inap memakai panel sendiri dan tidak mengirim `showCoverage`; bawaan prop `true` |
| 5. ESLint tanpa error baru; `npm run build` `PASS` | Terpenuhi | Tabel verifikasi |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Chip "Penjamin: …" di kepala tab tetap tampil. Chip itu nama penjamin kunjungan, bukan hasil hitungan tanggungan, sehingga di luar `RJ-DOC-DEC-089` |
| Dependency backend | `NONE` — backend tidak berubah |
| Perubahan sampingan | `NONE` — konstanta yang sempat dibalik untuk uji kontrol sudah dikembalikan ke `false` dan diverifikasi |
| Interupsi | `NONE` |
| Status Git | 9 berkas `M` di `src/` (termasuk 3 berkas `RJ-DOC-REV-FE-022`): `DoctorDrugDetailModal.jsx`, `DoctorPrescriptionDrugCatalogPanel.jsx`, `DoctorPrescriptionDrugRow.jsx`, `doctor-prescription-presentation-utils.js`, `doctor-prescription-compound-panel.jsx`, `doctor-prescription-regular-panel.jsx`, `doctor-prescription-tab.jsx`, `prescription.constants.js`, `use-doctor-drug-catalog.js` |
| Langkah berikutnya | Bila data asuransi sudah benar, ubah konstanta menjadi `true` |
