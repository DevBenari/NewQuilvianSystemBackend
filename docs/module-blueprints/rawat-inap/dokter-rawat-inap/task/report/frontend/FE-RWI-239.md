# Laporan Perubahan Frontend — `FE-RWI-239`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-239` |
| Judul | Label cakupan resep menurut status |
| Slice | PLAN-REPAIR-DOK-006 — perbaikan kelas perawatan dan cakupan asuransi per item |
| Roadmap | [frontend-roadmap-kelas-perawatan.md](../../../roadmap/frontend-roadmap-kelas-perawatan.md), kartu `FE-RWI-239` |
| Trace | FIX-DOK-006-05; ISS-DOK-006-04; aturan kontrak nomor 6; [plan repair](../../../docs/plan-repair/plan-repair-006-kelas-perawatan-dan-cakupan-asuransi.md) bagian 4 |
| Contract version | [RWI-KELAS-006-1.0](../../../contracts/kelas-perawatan-repair-006.md), disetujui pemilik 2026-10-12; SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112` |
| Wewenang UI | DEV_DISCRETION untuk teks label; komponen existing dipakai ulang tanpa desain visual baru |
| Dependency | Tidak ada. Status `CoverageStatus` dan `IsCoverageApplicable` sudah dikirim backend; perilaku `NotCovered` valid dari ✅ BE-RWI-226 |
| Klasifikasi | `LIGHT` — satu helper shared, tanpa API/state/route baru |
| Task mode | `CROSS-REPO` — frontend target tulis; backend hanya laporan dan register |
| Target tulis | `QuilvianSystemFrontendDev/src/components/ui/doctor-clinical-base/`, `tests/unit/`; laporan ini dan register `dokter-rawat-inap` |
| Model | Implementasi: Codex berbasis GPT-6. Penutupan register: Claude Opus 5.5 |
| Commit frontend saat dikerjakan | Basis `affcfcb4ea9f5177b1ff9a3ca0c09d15183af248`; source di-commit pemilik pada `2d0a6890e`; HEAD saat penutupan `f4495aa14` (merge IGD/Finance, tidak menyentuh berkas task) |
| Commit backend yang dijadikan rujukan | Basis `d908988248f6652ed7f2ce84aae9eb3e38dd36a0`; HEAD saat penutupan `4ec02370` |
| Tanggal | 2026-10-13 |
| Status | ✅ SELESAI — seluruh acceptance criteria terpetakan ke source; lint, unit, dan build dijalankan; uji manual NOT FEASIBLE |

---

## 1. Keadaan yang ditemukan di awal

Helper `getDrugCoverageInfo` pada `doctor-prescription-presentation-utils.js` menyusun label dari persentase lebih dulu dan selalu mengembalikan `isCovered: true`. Akibatnya obat yang tidak ditanggung tampil sebagai `Cover 0%`, yang terbaca seperti ditanggung. Status `CoverageStatus` dan `IsCoverageApplicable` dari backend diabaikan.

---

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka katalog obat atau daftar pesanan resep pada ruang kerja dokter rawat inap, dokter IGD, atau antrean dokter rawat jalan.
2. Setiap obat menampilkan lencana cakupan yang kini dibaca dari status backend:

| Status backend | Lencana yang dilihat dokter |
| --- | --- |
| `Covered` | Ditanggung |
| `PartiallyCovered` | Ditanggung sebagian |
| `NotCovered` | Tidak Di-cover |
| `NeedApproval` | Perlu persetujuan |
| `ConfigurationMissing`, `Unknown`, atau status lain | Belum dapat ditentukan |
| `IsCoverageApplicable=false`, `NotApplicable`, `Cash`, `SelfPay` | Tanpa lencana asuransi |
| Snapshot lama yang hanya berisi angka, tanpa status | Belum dapat ditentukan |

3. Contoh: pasien kamar kelas 1, Parasetamol hanya punya aturan cakupan kelas 2. Backend mengirim `NotCovered`; dokter melihat "Tidak Di-cover" dan tetap dapat memilih obat bila tarif dan validasi klinis sah.
4. Ringkasan harga tetap memakai nominal backend (`coveredAmount`, `patientPayAmount`), sehingga obat tidak ditanggung menampilkan "Ditanggung Rp 0" pada pesanan berasuransi, bukan persentase yang menyesatkan.

Jalur tidak normal (data kosong, gagal memuat, tanpa hak akses) tidak berubah dan tetap ditangani panel katalog existing.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend dan backend; `rules/frontend/REPORT_TEMPLATE.md`; skill `build-module-frontend`; kontrak RWI-KELAS-006-1.0; plan dan issue 006; `doctor-prescription-presentation-utils.js`, `DoctorPrescriptionDrugRow.jsx`, `index.js` pada `src/components/ui/doctor-clinical-base/`; panel resep rawat jalan `doctor-queues/tabs/prescription/*`; daftar resep farmasi `prescription-list-detail-page.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/ui/doctor-clinical-base/doctor-prescription-presentation-utils.js` | `getDrugCoverageInfo` mengembalikan `null` bila cakupan tidak berlaku. Label dipetakan dari `coverageStatus` yang dinormalkan; `isCovered` hanya `true` untuk `Covered`/`PartiallyCovered`; angka tanpa status menjadi "Belum dapat ditentukan"; `coverageNote` ikut diteruskan |
| `tests/unit/inpatient-class-coverage.test.mjs` | Berkas baru bersama `FE-RWI-240`. Dua tes pertama milik task ini: enam status backend, cakupan tidak berlaku, dan snapshot tanpa keputusan |

### 3.3 Kepatuhan arsitektur frontend

Perubahan berada pada helper shared `doctor-clinical-base` yang sudah ada; tidak menambah state, service, atau komponen baru. Tidak ada kalkulasi aturan asuransi di frontend.

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Lencana cakupan | `REUSE` `DoctorPrescriptionCoverageBadges` | Hanya teks label yang berubah |
| Ringkasan harga | `REUSE` `DoctorPrescriptionPriceSummary` | Nominal tetap dari backend |

UI GATE: seluruh elemen `REUSE`; tidak ada keputusan `NEW`/`EXTEND` yang menunggu pemilik. Grep anti-regresi pada baris tambahan: tidak ada warna/ukuran literal, gaya inline, atau kontrol HTML mentah.

Pemakai helper yang diperiksa: rawat inap (`physician-workspace`), IGD (`doctor-emergency`), dan rawat jalan (`doctor-queues` regular panel melalui `DoctorPrescriptionDrugCatalogPanel` dan `DoctorPrescriptionPriceSummary`). Ketiganya memakai helper yang sama.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah; indikator memuat katalog existing |
| Kosong | Tidak berubah; pesan katalog kosong existing |
| Gagal | Tidak berubah; pesan dan retry katalog existing. Harga tidak tersedia tampil "Harga belum tersedia" |
| Tanpa hak akses | Tidak berubah; permission `PrescribingDrug : Read` existing |
| Cakupan belum pasti | Lencana "Belum dapat ditentukan", dibedakan dari penolakan pasti "Tidak Di-cover" |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Prescribing Drug

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs` | Katalog obat beserta `CoverageStatus`, `IsCoverageApplicable`, nominal tanggungan | `PrescribingDrug : Read` |
| `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs/{drugId}` | Detail obat dengan cakupan yang sama | `PrescribingDrug : Read` |

Kontrak response tidak berubah.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-class-coverage.test.mjs` | 5/5 lulus (2 milik task ini) | `PASS` | Dijalankan ulang pada sesi penutupan 13 Oktober 2026 |
| `npx eslint --quiet` pada 17 berkas FE repair `FE-RWI-239`/`FE-RWI-240` | Exit 0, tanpa error | `PASS` | Sesi penutupan; log `%TEMP%/quilvian-kelas-fe-eslint-closure.log` |
| Unit 14 berkas terkait admisi dan resep (rawat inap, rawat jalan, cakupan) | 92 tes: 87 lulus, 5 gagal | `EXISTING / ENVIRONMENT ISSUE` | Kelima kegagalan menguji berkas yang tidak berubah sejak basis atau isi yang sudah ada di basis (deposit `minimumAmount`, dialog pembatalan, pencarian kota, parity tab rawat jalan). Log `%TEMP%/quilvian-kelas-fe-unit-closure.log` |
| Pengujian terarah sesi implementasi | 39/39 lulus | `PASS` | 13 Oktober 2026, sebelum commit `2d0a6890e` |
| `node --max-old-space-size=8192 ./node_modules/next/dist/bin/next build` lalu `prepare-standalone` | Berhasil | `PASS` | Sesi implementasi 13 Oktober 2026; source task tidak berubah sesudahnya |
| Suite global | 2776 tes: 2742 lulus, 34 gagal. Snapshot HEAD pembanding: 2761 tes, 2732 lulus, 29 gagal | `EXISTING / ENVIRONMENT ISSUE` | Lima kegagalan tambahan berasal dari relokasi menu Transfer pada Workspace PPRI, bukan task ini; direproduksi ulang pada sesi penutupan: 21 tes, 16 lulus, 5 gagal |

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-class-coverage.test.mjs` — PASS (5/5).

Uji manual: `NOT FEASIBLE` — sesi tidak memiliki browser/aplikasi aktif, dan aplikasi yang berjalan belum memuat build baru.

**Tidak dijalankan:** build produksi ulang pada sesi penutupan, karena source task tidak berubah sejak build sesi implementasi dan merge `f4495aa14` tidak menyentuh berkas task.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Covered/Partial/NotCovered/NeedApproval/Unknown dibedakan | Terpenuhi | Peta `labels` dan `isCovered` pada `getDrugCoverageInfo`; tes enam status |
| Tanpa asuransi tidak diberi lencana pertanggungan | Terpenuhi | `isCoverageApplicable === false` serta `notapplicable`/`selfpay`/`cash` mengembalikan `null` |
| Data lama tanpa status tidak otomatis ditanggung karena nilai nol | Terpenuhi | Angka tanpa status → "Belum dapat ditentukan", `isCovered: false`; tes `coveragePercent: 0` dan `coveredAmount: 100` |
| Semua pemakai helper menerima hasil konsisten | Terpenuhi | Rawat inap, IGD, dan rawat jalan merender `DoctorPrescriptionCoverageBadges`/`DoctorPrescriptionPriceSummary` dari helper yang sama |
| DoD: source, validasi, laporan tracked, traceability | Terpenuhi | Bagian 6; [roadmap](../../../roadmap/frontend-roadmap-kelas-perawatan.md) dan [traceability](../../../roadmap/requirement-traceability-kelas-perawatan.md) |
| DoD: uji manual bila runtime tersedia | `NOT FEASIBLE` | Runtime tidak tersedia; tidak diklaim lulus |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Lima kegagalan unit lama pada berkas terkait; tidak disebabkan task ini |
| Masalah yang diketahui | `doctor-queues/tabs/prescription/doctor-prescription-shared.jsx` masih memuat salinan lokal `getCoverageInfo`, `CoverageBadges`, dan `PriceSummary` dengan logika lama, tetapi tidak diimpor di mana pun sehingga tidak tampil. Pembersihan kode mati di luar scope. Daftar resep farmasi memakai `getCoverageMeta` sendiri (modul Farmasi) dan tidak termasuk task ini |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | Laporan ditulis pada sesi penutupan terpisah; source tidak dikerjakan ulang |
| Status Git | Frontend bersih pada `f4495aa14`; source task sudah di-commit pemilik pada `2d0a6890e`. Laporan ini belum di-commit di backend |
| Langkah berikutnya | Uji visual katalog resep berasuransi di ketiga layar setelah aplikasi menjalankan build terbaru |
