# Laporan Perubahan Frontend — `FE-OPT-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-042` |
| Judul | E2E penjamin dan Daftarkan Barang |
| Slice | Amendment kesiapan — layar `FE-OPT-N02`, `N13` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r3) — Rincian `FE-OPT-042` |
| Trace | `FR-OPT-012`, `112`; `OPT-DEC-078`, `080`; `OPT-AC-036`, `038`; `UAT-30`, `31`, `34`, `35`; risiko `R6` |
| Contract version | `OPT-VAL-v1.2` bagian 8, brief r3 §5.2, §5.12 — `approved` sisi Optik, Yoga Aji Pratama 2026-11-06 |
| Wewenang UI | Tidak ada perubahan layar |
| Approval task | Yoga Aji Pratama, 2026-11-06 ("sudah disetujui hamzah dan yasmina kerjakan") |
| Dependency | `FE-OPT-010` ✅, `031` ✅, `BE-OPT-061` ✅ |
| Klasifikasi | `LIGHT` — 2 berkas uji, tanpa kode produksi |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/tests/e2e/`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `811f22159` (branch `YogaV2`) + `FE-OPT-040`, `041` di working tree |
| Tanggal | 2026-11-06 |
| Status | Selesai di E2E API tiruan. Belum dengan backend asli. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Layar penjamin dan Daftarkan Barang sudah ada, tetapi buktinya belum per UAT. Uji penjamin yang ada hanya membuktikan penolakan, karena saat itu Registrasi selalu menolak. Keterangan penimpaan profil pada Daftarkan Barang juga belum diperiksa.

---

## 2. Proses bisnis dari sisi pengguna

| UAT | Alur | Hasil yang dibuktikan |
| --- | --- | --- |
| `UAT-30` | Petugas memilih Penjamin Perusahaan, memilih kartu, lalu Daftarkan | Tanpa kartu, kiriman ditahan. Dengan kartu, kiriman membawa `paymentType: 3`, `patientCompanyGuarantorId`, dan kunci idempoten, lalu layar membuka detail |
| `UAT-31` | Server menolak | Pesan tampil apa adanya, isian tidak hilang, tanpa Coba lagi |
| `UAT-34` | Admin Optik memilih barang lewat Daftarkan Barang lalu menyimpan | `PUT item-profiles/{drugId}` terkirim, dan katalog dimuat ulang |
| `UAT-35` | Admin Optik mencari barang | Pencarian terkirim dengan `isPrescribable=false` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`optical-visit-register-view.jsx`, `use-optical-visit-register.jsx`, `optical-item-profile-modal.jsx`, `optical-master-data-constants.js`, spec E2E kunjungan dan data master.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `tests/e2e/optical-visit-screen.spec.mjs` | Skenario baru `UAT-30`; skenario penolakan diberi label `UAT-31` |
| `tests/e2e/optical-master-data-screen.spec.mjs` | Skenario Daftarkan Barang diberi label `UAT-34`, `35`, `OPT-AC-038`, dan memeriksa keterangan penimpaan |

Kode produksi tidak berubah, karena uji tidak menemukan selisih.

### 3.3 Kepatuhan arsitektur frontend

`NOT APPLICABLE`: hanya berkas uji.

---

## 4. State yang ditangani di layar

| Keadaan | Bukti |
| --- | --- |
| Kartu belum dipilih: kiriman ditahan | E2E `UAT-30` |
| Penolakan server: pesan apa adanya, isian tetap | E2E `UAT-31` |
| Barang dan kategori wajib | E2E Daftarkan Barang |

---

## 5. Endpoint yang dikonsumsi

Tidak ada perubahan. Grup `Opt Visit` (`POST /visits`), `Opt Reference Data` (`PUT /item-profiles/{drugId}`), dan `Health Services / Master Data / Drug` (`GET /options`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| E2E seluruh layar Optik atas build produksi | 152 lulus, 0 gagal | `PASS` | Keluaran Playwright |
| Lint, build, uji unit | Sama dengan `FE-OPT-041`, pada working tree yang sama | `PASS` | Laporan `FE-OPT-041` |

**Uji manual: `NOT FEASIBLE`.** Mendaftarkan kunjungan menulis ke devYoga di luar wewenang task ini. devYoga juga belum punya barang `IsPrescribable = false` dan kartu penjamin uji.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `UAT-30`: Penjamin Perusahaan terkirim dan kunjungan tampil | Terpenuhi di E2E API tiruan | Skenario `UAT-30` |
| (2) `UAT-31`: pesan penolakan server tampil apa adanya | Terpenuhi | Skenario `UAT-31` |
| (3) `UAT-34`: barang terdaftar tampil di katalog | Terpenuhi sebagian: katalog dimuat ulang sesudah simpan. Barang yang tampil berasal dari API tiruan | Skenario Daftarkan Barang |
| (4) `UAT-35`: pencarian terkirim dengan `isPrescribable=false` | Terpenuhi | Skenario Daftarkan Barang |
| (5) `OPT-AC-038`: keterangan penimpaan tampil | Terpenuhi | Skenario Daftarkan Barang |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `OPT-AC-038` menyebut peringatan saat memilih barang berprofil. Yang ada di layar adalah keterangan tetap di modal, dan ini disetujui apa adanya oleh `OPT-DEC-080` |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | `UAT-34`, `35` hanya terbukti dengan API tiruan sampai `B2` terisi |
| Perubahan sampingan | `NONE`. `test-results/` dan `playwright-report/` tidak berubah |
| Interupsi | `NONE` |
| Status Git | `M` 2 berkas `tests/e2e/`, ditambah perubahan task lain yang belum di-commit |
| Langkah berikutnya | Uji alur asli sesudah data `B1`..`B4` terisi |
