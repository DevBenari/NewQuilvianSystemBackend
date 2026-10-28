# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-022` |
| Judul | Stok di katalog Resep RJ |
| Slice | Amendment SF — Katalog Resep dari stok Farmasi Rawat Jalan |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian 21.2 |
| Trace | `RJ-DOC-DEC-087`, `RJ-DOC-DEC-088`; kontrak bagian 21.0 |
| Contract version | `GET prescribing-drugs?fromPharmacyStock=true` sesuai bagian 21.0 roadmap, disetujui `RJ-DOC-DEC-087`; diimplementasikan `RJ-DOC-REV-BE-022` |
| Wewenang UI | Menampilkan stok per obat dan nama depo, menonaktifkan tombol Pilih saat stok habis, mengganti teks katalog (`RJ-DOC-DEC-087`). Tanpa komponen baru |
| Dependency | `RJ-DOC-REV-BE-022` ✅ `2026-10-28` |
| Klasifikasi | `MEDIUM` — skor 5: dua repository (2), 9–20 berkas diperiksa (1), 3 berkas diubah (0), logika sederhana (0), memakai kontrak backend yang ada (1), tanpa database (0), keamanan tidak berubah (0), satu workflow (1) |
| Task mode | `CROSS-REPO MODE`, target tulis frontend (task ini) |
| Target tulis | `V2QuilvianSystemFrontendDev/src/**` untuk katalog Resep RJ; laporan dan tautan bukti di blueprint `rawat-jalan` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `e3e5adc57` (branch `sukmagpV2`) |
| Commit backend yang dijadikan rujukan | `0415bfc0` + perubahan `RJ-DOC-REV-BE-022` yang belum di-commit |
| Tanggal | 2026-10-28 |
| Status | ✅ Selesai, belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Panel *Daftar Obat* pada tab Resep Dokter Rawat Jalan (Obat Biasa dan Racikan) memuat seluruh master obat lewat
`useDoctorDrugCatalog`. Dokter tidak melihat apakah obat tersedia di Farmasi. Tombol **Pilih** hanya mengikuti kesiapan
tarif dan satuan, sehingga obat yang stoknya habis tetap dapat dipilih.

---

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka tab **Resep** pada kunjungan Rawat Jalan.
2. Di atas tabel tampil chip **Depo: FARMASI RAWAT JALAN**, sehingga dokter tahu asal stok.
3. Di bawah nama tiap obat tampil chip stok hijau, misalnya **Stok 1.290 tab**. Angkanya adalah stok yang dapat
   dilayankan: saldo dikurangi stok yang sedang ditahan resep lain.
4. Obat yang stoknya habis tetap tampil dengan chip merah **Stok habis**. Baris dipudarkan, tombol **Pilih** nonaktif,
   dan alasannya tertulis di bawah nama obat, contoh "Stok obat di FARMASI RAWAT JALAN habis."
5. Mode **Obat Racikan** memakai sumber yang sama; tombolnya **+ Bahan** dan ikut nonaktif saat stok habis.
6. Bila depo kunjungan tidak dapat ditentukan, panel menampilkan pesan dari backend, contoh "Depo Farmasi untuk
   layanan pasien belum dikonfigurasi.", beserta tombol **Coba Lagi**. Daftar master tidak ditampilkan sebagai gantinya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `DoctorPrescriptionDrugCatalogPanel.jsx`; `doctor-prescription-presentation-utils.js`;
`doctor-prescription.module.css`; `use-doctor-drug-catalog.js`; `prescribing-drug.service.js`;
`doctor-prescription-regular-panel.jsx`; `doctor-prescription-compound-panel.jsx`; `doctor-prescription-tab.jsx`;
`doctor-prescription-shared.jsx`; `use-doctor-prescription.js`; `use-inpatient-drug-catalog.jsx` dan
`prescription-drug-catalog-panel.jsx` Rawat Inap (pemakai util bersama); `tests/unit/outpatient-prescription-parity.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/pharmacy-management/use-doctor-drug-catalog.js` | Mengirim `fromPharmacyStock: true`; menyimpan `stockLocationName` dari respons; pesan gagal bawaan "Daftar obat stok Farmasi tidak dapat dimuat." |
| `src/components/ui/doctor-clinical-base/doctor-prescription-presentation-utils.js` | `getDrugPrescriptionAvailability` menutup pilihan bila `isStockAvailable === false` (nilai `null` dari katalog Rawat Inap/IGD tidak berpengaruh); util baru `describeDrugStock` untuk label stok |
| `src/components/ui/doctor-clinical-base/DoctorPrescriptionDrugCatalogPanel.jsx` | Chip depo di penghitung, chip stok per baris, `canSelect` ikut stok, teks katalog menyebut stok Farmasi, `propTypes.stockLocationName` |

### 3.3 Kepatuhan arsitektur frontend

- Tidak ada state, service, atau komponen baru. Hook dan service katalog yang ada diperluas lewat satu parameter.
- Tidak ada CSS baru. Chip memakai kelas yang sudah ada di modul panel (`counterChip_success`, `counterChip_danger`,
  `counterChip_primary`).

**Keputusan base component**

UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol Pilih nonaktif saat stok habis | `BaseButton` | Sudah dipakai panel dengan `disabled` dan `title` | REUSE | `disabled` ikut `canSelect` |
| Chip stok per baris | Kelas `counterChip_success`/`counterChip_danger` panel | `doctor-prescription.module.css` baris `.counterChip_*`, dipakai penghitung panel dan kartu resep | REUSE | Chip di `catalogDrugBadges`, tanpa kolom baru agar panel sempit tidak melebar |
| Nama depo | Kelas `counterChip_primary` | Dipakai kartu *Resep yang Dipilih* | REUSE | Chip di `catalogCounterBadges` |
| Pesan stok habis dan galat routing | `catalogUnavailableText`, `catalogMessage` + `BaseButton` Coba Lagi | Sudah ada di panel | REUSE | Pesan dari backend ditampilkan apa adanya |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat daftar obat stok Farmasi..." |
| Kosong | "Tidak ada obat di stok Farmasi yang sesuai dengan pencarian." |
| Gagal | Pesan backend (contoh "Depo Farmasi untuk layanan pasien belum dikonfigurasi.") dengan tombol **Coba Lagi** |
| Tanpa hak akses | Tidak berubah: pesan `403` backend tampil di panel dengan **Coba Lagi** |
| Stok habis | Chip **Stok habis**, baris dipudarkan, Pilih nonaktif, alasan di bawah nama obat |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Prescribing Drug

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs?encounterId=&fromPharmacyStock=true` | Katalog Obat Biasa dan Bahan Racikan beserta stok depo | `PrescribingDrug : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 3 berkas yang berubah | exit 0, tanpa temuan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | exit 0 | `PASS` | Keluaran perintah |
| `npm run test:unit` | Skrip gagal mencari `tests/unit/*.test.mjs`: Node `v20.20.0` tidak mengurai glob pada `--test` | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` (glob diurai shell) | 3.970 test: 3.929 lulus, 41 gagal. Seluruh kegagalan ada pada berkas yang tidak disentuh task ini; satu-satunya di `outpatient-prescription-parity` memeriksa teks `RESEP OBAT` pada `doctor-prescription-tab.jsx` yang tidak diubah | `EXISTING / ENVIRONMENT ISSUE` | Log di scratchpad sesi |
| `npm run build` | `Compiled successfully`, 526 halaman, `prepare-standalone` selesai | `PASS` | Keluaran perintah |
| Grep anti-regresi pada baris tambahan (warna literal, typography, `<button>`, `<table>`, utility Bootstrap, `!important`, inline style) | 0 temuan untuk semua pola | `PASS` | Keluaran perintah |
| Uji layar Playwright | **10/10 PASS** (tabel di bawah) | `PASS` | `pw_fe022.cjs`, tangkapan layar `shots/fe022-*.png` di scratchpad sesi |

Uji layar memakai FE dev `localhost:3000`, panggilan `7184` dialihkan ke backend uji `7185` (`RJ-DOC-REV-BE-022`),
DB `QuilvianNewDevSukma`, konsultasi tertunda pasien uji "UJI GC PASIEN DAA8" (kunjungan 23 Okt). Tidak ada tombol
Pilih/Simpan yang diklik. Stok `0` dan galat routing disimulasikan dengan mengubah respons di peramban, karena seluruh
obat sengaja berstok (`RJ-DOC-DEC-086`). Sesudah uji, query DB memastikan tidak ada resep, mutasi, maupun reservasi baru.

| ID | Skenario | Hasil |
| --- | --- | --- |
| S1 | Katalog Obat Biasa mengirim `fromPharmacyStock=true` | PASS |
| S2 | Chip **Depo: FARMASI RAWAT JALAN** tampil | PASS |
| S3 | Chip stok 3 baris sama dengan `availableStockQuantity` respons | PASS |
| S4 | Pilih aktif untuk obat berstok | PASS |
| S5 | Cari "paracetamol" → PARACETAMOL 500 MG TABLET, **Stok 1.290 tab** | PASS |
| S6 | Stok `0` → chip **Stok habis**, Pilih nonaktif, `title` dan alasan "Stok obat di FARMASI RAWAT JALAN habis." | PASS |
| S7 | Baris lain yang berstok tetap dapat dipilih | PASS |
| S8 | Galat routing `400` → pesan depo dan **Coba Lagi** | PASS |
| S9 | **Coba Lagi** memuat ulang katalog berstok | PASS |
| S10 | Mode Obat Racikan: `fromPharmacyStock=true`, chip depo dan stok tampil | PASS |

MANUAL TEST: PASS — kontrol yang diubah (pencarian, tombol Pilih/+ Bahan, Coba Lagi, perpindahan mode) diuji di layar;
klik Pilih pada obat berstok sengaja tidak dilakukan agar draft resep pasien uji tidak berubah.

AUTOMATED TEST: SKIPPED (opsional) — perubahan berupa komposisi tampilan dan satu util label; suite yang ada dijalankan
(lihat tabel).

**Tidak dijalankan:** perbandingan berdampingan dengan Rawat Inap — panel Rawat Inap tidak memuat stok.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Baris menampilkan stok sesuai respons backend | Terpenuhi | S3, S5 |
| 2. Stok `0` → Pilih nonaktif, `title` menjelaskan stok habis | Terpenuhi | S6, S7 |
| 3. Racikan memakai sumber yang sama | Terpenuhi | S10 |
| 4. Galat routing tampil sebagai pesan katalog dengan Coba Lagi | Terpenuhi | S8, S9 |
| 5. ESLint tanpa error baru; `npm run build` `PASS` | Terpenuhi | Tabel verifikasi |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Badge Next "1 Issue" pada tangkapan layar adalah artefak uji (WebSocket SignalR tidak dialihkan ke 7185) |
| Masalah yang diketahui | Skrip `npm run test:unit` memakai glob yang tidak didukung Node `v20.20.0` di mesin ini; 41 test gagal pada modul lain. Keduanya tidak diubah |
| Dependency backend | `RJ-DOC-REV-BE-022` ✅. Backend dev `7184` perlu di-restart memuat build baru; sebelum itu parameter `fromPharmacyStock` diabaikan dan chip stok tidak tampil |
| Perubahan sampingan | `NONE` — line ending CRLF `DoctorPrescriptionDrugCatalogPanel.jsx` sempat berubah ke LF oleh skrip edit dan dikembalikan sebelum validasi akhir |
| Interupsi | `NONE` |
| Status Git | `M src/components/ui/doctor-clinical-base/DoctorPrescriptionDrugCatalogPanel.jsx`, `M src/components/ui/doctor-clinical-base/doctor-prescription-presentation-utils.js`, `M src/lib/hooks/health-services/pharmacy-management/use-doctor-drug-catalog.js` |
| Langkah berikutnya | Restart backend 7184; owner menguji stok `0` lewat penyesuaian stok manual (`RJ-DOC-DEC-086`); commit oleh owner |
