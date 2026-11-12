# Laporan Perubahan Frontend — `PAY-FE-15`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-15` |
| Judul | Tunjangan Transportasi pegawai |
| Slice | `MVP-1` — `EPIC PGJ-03`, layar `FE-PGJ-10` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-15` |
| Trace | FR-PGJ-021, FR-PGJ-023 (`CALC-01`); PRL-DEC-46; `../../../03-frontend-architecture.md` bagian 1, 2 (layar anak dari detail Profil Penggajian), 3.6, 6; `../../../contracts/validation-matrix.md` `CALC-01` |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12). Endpoint yang dikonsumsi sudah ada sebelum `PGJ-C1` (`WfpTransportAllowanceController`) |
| Wewenang UI | Isi layar, sumber data, hak akses tombol, dan bunyi peringatan dikunci `03-frontend-architecture.md`. Jalan masuk berupa layar anak dari detail Profil Penggajian (tanpa butir menu). Bentuk form (modal) dan tata letak `DEV_DISCRETION` lewat gerbang keputusan base component |
| Dependency | — (tanpa dependency backend). Daftar kebijakan diambil dari endpoint options kebijakan transport yang dirangkai di `PAY-FE-14` 🟡 |
| Klasifikasi | `MEDIUM` — satu layar anak (daftar + modal form + hapus) dan satu tombol masuk di layar induk; pola Payroll V2 yang sudah ada |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repo backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fc9e74d64` (branch `yasmina`), di atas perubahan `PAY-FE-14` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `6722d7ca` (branch `Yasmina`); controller tunjangan transport pegawai tidak diubah task backend mana pun |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai; ESLint 0 galat; uji unit tanpa kegagalan baru; skrip pemeriksa logika `PASS`; **`npm run build` dan `npm run dev` tidak dijalankan atas instruksi pemilik**; uji manual di browser menunggu pemilik |

---

## 1. Keadaan yang ditemukan di awal

- **Backend** sudah punya endpoint tunjangan transport per pegawai di
  `api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/transport-allowance`:
  metadata, ringkasan, daftar, detail, tambah, ubah, status, dan hapus. Hak aksesnya
  `WorkforceTransportAllowance : Read/Create/Update/Delete`. Tidak ada `/options`, wajar karena
  tunjangan ini sub-resource milik profil tenaga kerja.
- **Frontend** belum punya layarnya. Detail Profil Penggajian (`/payroll/workforce-compensations/[id]`,
  `[id]` = `workforceProfileId`) hanya punya tombol Kembali.
- **Perilaku backend yang menentukan rancangan:** `POST` menolak bila pegawai sudah punya tunjangan
  transport apa pun ("Transport allowance untuk workforce ini sudah tersedia."). Akibatnya, dua
  tunjangan beririsan tidak bisa dibuat lewat layar ini. Irisan hanya mungkin dari data lama yang
  sudah ganda, atau dari penyuntingan tanggal pada data lama itu.
- Respons tunjangan tidak membawa cara hitung kebijakannya. Cara hitung diambil dari endpoint options
  kebijakan transport, yang membawa `CalculationMethod`.

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Petugas Payroll yang memegang hak `WorkforceTransportAllowance` di Akses Role.

**Langkah:**

1. Petugas membuka Payroll → Penghasilan Pegawai → Profil Penggajian, lalu membuka detail seorang
   pegawai.
2. Di hero detail, petugas menekan **Tunjangan Transportasi**. Tombol ini hanya tampil bagi pemegang
   `WorkforceTransportAllowance : Read`.
3. Layar anak menampilkan nama dan nomor pegawai, ringkasan (total, aktif, nonaktif), saringan status
   dan jumlah baris, pencarian, serta tabel tunjangan beserta masa berlakunya.
4. Petugas menekan **+ Tambah Tunjangan Transportasi** (hak `Create`), lalu memilih kebijakan
   transport, status, nominal bulanan (0 = mengikuti kebijakan), batas bulanan pegawai, nominal per
   kehadiran, komponen payroll, masa berlaku, dan keterangan.
5. Bila kebijakan yang dipilih bukan Tetap Bulanan, form menampilkan: "Cara hitung kebijakan ini belum
   dihitung otomatis di run. Gunakan Penghasilan Variabel."
6. **Sebelum menyimpan:** bila isian membuat tunjangan bulanan tetap aktif beririsan dengan tunjangan
   bulanan aktif lain milik pegawai yang sama, peringatan kuning muncul di dalam form. Peringatan
   menyebut kebijakan dan masa tunjangan yang berbenturan, serta akibatnya: pegawai gagal dihitung di
   run (`CALC-01`). Peringatan tidak menahan simpan.
7. Petugas menyimpan, lalu modal tertutup, toast hijau muncul, dan daftar dimuat ulang. Baris dapat
   **Diubah** (hak `Update`) atau **Dihapus** (hak `Delete`, dengan konfirmasi).

**Contoh** (data samaran): pegawai punya tunjangan A, "Transport Bulanan", 1 Jan–30 Jun 2026, dan
tunjangan B dengan kebijakan yang sama mulai 1 Jul 2026 tanpa akhir (data lama). Bila petugas mengubah
tanggal selesai A menjadi kosong, form langsung menampilkan: "...Beririsan dengan: Transport Bulanan
(01/07/2026 s.d. sekarang)." Bila tetap disimpan, run akan menggagalkan pegawai ini dengan `CALC-01`.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Pegawai sudah punya tunjangan transport | Tombol Tambah dinonaktifkan, dengan keterangan bahwa sistem hanya menerima satu tunjangan per pegawai |
| Data lama sudah memuat dua tunjangan bulanan aktif yang beririsan | Peringatan kuning di atas daftar sejak layar dibuka |
| Kebijakan belum dipilih, nominal melebihi batas pegawai, tanggal terbalik | Ditolak di form sebelum dikirim, dengan pesan per isian |
| Belum ada kebijakan transport aktif | Form menampilkan arahan untuk membuat kebijakan di Data Induk |
| Server menolak simpan atau hapus | Pesan server tampil apa adanya di dalam modal; modal tetap terbuka |
| Pegawai tidak ditemukan atau nonaktif di HR | Pesan "Profil tenaga kerja tidak ditemukan." dari server tampil sebagai galat |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- **Governance:** `AGENTS.md` dan `CLAUDE.md` frontend; `rules/frontend/*` (arsitektur, katalog dan
  gerbang base component, checklist UI, template laporan).
- **Rujukan frontend:** `payroll-compensation-view.jsx`, `payroll-compensation-detail-view.jsx`,
  `payroll-compensation-table-columns.jsx`, `use-payroll-compensation-list.js`,
  `use-payroll-compensation-detail.js`, `payroll-compensation-service.js`,
  `payroll-manual-confirmation-modal.jsx` (pola modal form), `use-payroll-medical-fee.js` (pola
  toast), `payroll-deduction-table-columns.jsx` (pola tombol aksi per baris), `payroll-constants.js`,
  `payroll-common-utils.js`, `payroll-section.jsx`, `base-form-control.jsx`, `confirm-modal.jsx`,
  `information-alert.jsx`, `hero.jsx`, `base-detail-view.jsx`, `use-permission.jsx`,
  `input-normalizer-utils.jsx`.
- **Backend (baca-saja):** `WfpTransportAllowanceController.cs` (route, metadata, ringkasan, saringan,
  create, update, validasi, status), `WfpTransportAllowanceDtos.cs`,
  `WfpTransportAllowancePolicyDtos.cs` (opsi kebijakan), `PayrollCalculationService.cs` (aturan
  `CALC-01` dari `PAY-BE-18`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-transport-allowance-constants.js` | Baru. Endpoint, route, hak akses, status dan badge, saringan bawaan, dan seluruh salinan teks termasuk peringatan `CALC-01` |
| `src/utils/payroll/payroll-transport-allowance-utils.js` | Baru. Normalisasi respons dan opsi, form, validasi yang mencerminkan backend, payload, serta deteksi irisan (`rangesOverlap`, `isMonthlyCandidate`, `findOverlappingMonthlyAllowances`, `hasExistingMonthlyOverlap`) |
| `src/lib/services/payroll/payroll-transport-allowance-service.js` | Baru. Panggilan Axios lewat `InstanceAxios`, termasuk options kebijakan dan komponen payroll |
| `src/lib/hooks/payroll/use-payroll-transport-allowance.js` | Baru. Data, saringan, paginasi, modal form, hapus, toast, hak `Create`/`Update`/`Delete`; setiap request dibatalkan saat cleanup |
| `src/components/view/payroll/transport-allowance/payroll-transport-allowance-view.jsx` | Baru. Halaman daftar |
| `src/components/view/payroll/transport-allowance/payroll-transport-allowance-form-modal.jsx` | Baru. Modal form beserta peringatan `CALC-01` dan keterangan metode manual |
| `src/components/view/payroll/transport-allowance/payroll-transport-allowance-table-columns.jsx` | Baru. Kolom tabel dan tombol Ubah/Hapus yang dijaga hak |
| `src/app/payroll/workforce-compensations/[id]/transport-allowance/page.jsx` | Baru. Route tipis |
| `src/lib/hooks/payroll/use-payroll-compensation-detail.js` | Tambah `canOpenTransportAllowance` (`WorkforceTransportAllowance : Read`) dan `openTransportAllowance` |
| `src/components/view/payroll/compensation/payroll-compensation-detail-view.jsx` | Hero memakai `renderDetailActions`: Kembali + **Tunjangan Transportasi** |

### 3.3 Kepatuhan arsitektur frontend

- **Pola:** layar ini adalah sub-resource per pegawai, bukan master data mandiri. Karena itu ia
  mengikuti pola Payroll V2 (service → hook dengan state lokal → view), sama dengan Profil Penggajian,
  bukan standar tujuh berkas Redux master data.
- **Alur:** route → view → hook → service/utils/constants.
- **Tanpa tambahan arsitektur:** tidak ada CSS baru, instance Axios baru, maupun komponen baru. Hak akses
  memakai `usePermission`; tidak ada nama peran di kode.

**Gerbang keputusan base component:**

| Elemen | Base component | Status | Bukti |
| --- | --- | --- | --- |
| Hero, ringkasan, saringan, pencarian, tabel, paginasi | `Hero`, `SummaryCards`, `DataFilter`, `FilterSelect`, `DataTable`, `Pagination` | `REUSE` | `payroll-compensation-view.jsx` |
| Badge status | `StatusBadge` dengan konfigurasi baru (data, bukan komponen) | `REUSE` | Pola `PAYROLL_COMPENSATION_BADGE_CONFIG` |
| Tombol Tambah, Kembali, Ubah, Hapus, Tunjangan Transportasi | `BaseButton` | `REUSE` | `payroll-deduction-table-columns.jsx`, detail master data |
| Form tambah/ubah | `ConfirmModal size="lg"` berisi `BaseSelectField`/`BaseTextField`/`BaseDateField`/`BaseTextAreaField` | `REUSE` | `payroll-manual-confirmation-modal.jsx` |
| Konfirmasi hapus | `ConfirmModal` | `REUSE` | Pola baku |
| Peringatan `CALC-01`, irisan data lama, satu tunjangan per pegawai, metode manual | `InformationAlert` | `REUSE` | Dipakai di seluruh layar Payroll |
| Galat aksi dalam modal | `PayrollActionError` | `REUSE` | `payroll-section.jsx` |
| Toast hasil aksi | `ToastStack` | `REUSE` | `payroll-medical-fee-view.jsx` |
| Penolakan akses | `AccessDeniedGate` | `REUSE` | `payroll-compensation-view.jsx` |
| Tombol masuk di detail profil | `BaseDetailView` `renderDetailActions` | `REUSE` | Detail master data Payroll |

`UI GATE: PASS — seluruh elemen REUSE; 0 COMPOSE, 0 EXTEND, 0 NEW`

**Pilihan bentuk form** (`DEV_DISCRETION`; bukan elemen non-`REUSE`, dicatat agar keputusan
terlihat):

1. **(Dipakai)** modal `ConfirmModal`, seperti klaim jasa medis dan konfirmasi pembayaran. Petugas
   tetap melihat daftarnya, sehingga irisan dengan baris lain mudah dicocokkan.
2. Halaman tambah/ubah tersendiri. Ini perlu dua route tambahan dan pemeriksaan irisan memuat ulang
   seluruh tunjangan; tidak ada manfaat bagi form sependek ini.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Mengambil data tunjangan transportasi..."; ringkasan kerangka; tombol simpan "memuat" dan isian nonaktif selama pilihan dimuat |
| Kosong | "Belum ada tunjangan transportasi." dengan arahan menambah dari kebijakan yang aktif |
| Gagal | Kotak galat merah berisi pesan server apa adanya; galat simpan/hapus di dalam modal |
| Tanpa hak akses | Tombol masuk tidak tampil tanpa `Read`; tombol Tambah, Ubah, Hapus tidak tampil tanpa `Create`/`Update`/`Delete`; penolakan server ditangani `AccessDeniedGate` |
| Berisi | Ringkasan tiga kartu dan tabel: kebijakan, masa berlaku, nominal bulanan, batas pegawai, komponen, status |

## 5. Endpoint yang dikonsumsi

#### `Corporate / Payroll Management / Transport Allowance`

Base URL: `/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/transport-allowance`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status | `WorkforceTransportAllowance : Read` |
| `GET` | `/summary` | Tiga kartu ringkasan | `WorkforceTransportAllowance : Read` |
| `GET` | `/` | Tabel berhalaman, dan seluruh tunjangan pegawai (maks 100) untuk pemeriksaan irisan | `WorkforceTransportAllowance : Read` |
| `POST` | `/` | Tambah | `WorkforceTransportAllowance : Create` |
| `PUT` | `/{id}` | Ubah (status tunjangan ikut di sini) | `WorkforceTransportAllowance : Update` |
| `DELETE` | `/{id}` | Hapus (ditandai) | `WorkforceTransportAllowance : Delete` |

`GET /{id}` dan `PATCH /{id}/status` tidak dipakai. Form ubah diisi dari baris daftar yang sudah
lengkap, dan status diubah lewat `PUT`.

#### `Corporate / Payroll Management / Transport Allowance Policy`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/transport-allowance-policies/options` | Pilihan kebijakan dan cara hitungnya | `TransportAllowancePolicy : Read` |

Pilihan Komponen Payroll memakai `GET /v1/corporate/payroll-management/master-data/payroll-components/options`
yang sudah ada. Nama dan nomor pegawai di hero diambil dari detail Profil Penggajian yang sudah ada;
bila tidak berhak, judul tampil tanpa nama.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 10 berkas baru/berubah | 0 galat, 3 peringatan `react-hooks/set-state-in-effect` | `PASS` | Keluaran perintah |
| Peringatan yang sama pada hook rujukan Payroll (`use-payroll-compensation-list`, `use-payroll-medical-fee`) | 0 galat, 3 peringatan setara; peringatan di `use-payroll-compensation-detail.js` sudah ada sebelum perubahan | `EXISTING WARNING` | Keluaran perintah |
| `npm run test:unit` penuh | 4.800 uji, 4.753 lolos, 47 gagal; daftar 32 berkas gagal **identik** dengan baseline `PAY-FE-14` | `PASS` (tanpa galat baru) | `diff` daftar berkas gagal |
| Skrip pemeriksa logika sekali pakai (di luar repo): irisan | Tunjangan terbuka beririsan dengan A dan B; Mar–Mei hanya dengan A; 2025 tidak beririsan; status Suspended, kebijakan Per Kehadiran, tunjangan nonaktif, dan baris yang sedang diubah tidak dibandingkan; deteksi data lama ganda benar | `PASS` | Keluaran skrip |
| Skrip pemeriksa: validasi dan payload | Nominal melebihi batas ditolak; tanpa kebijakan ditolak; tanggal terbalik ditolak; nominal desimal `1250000.75` utuh; tambah tanpa `isActive`; ubah membawa saldo, pegawai, dan `isActive` | `PASS` | Keluaran skrip |
| Normalizer nominal | `400.000` → `400000`, `1.250.000,75` → `1250000.75`; `parseAmount` dibetulkan agar membaca bentuk kanonik ini | `PASS` | Keluaran skrip |
| Grep anti-regresi UI (`<button`, `btn-`, `<table`, `fw-`/`fs-`, inline style, warna literal, input mentah, CSS baru) | Bersih | `PASS` | Keluaran perintah |
| `npm run build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |

`AUTOMATED TEST: SKIPPED (opsional) — tidak menambah test baru; suite yang ada dijalankan dan logika diperiksa dengan skrip di luar repo.`

Uji manual: `NOT FEASIBLE` — agen tidak menjalankan `npm run dev` atas instruksi pemilik dan tidak
punya sesi login.

**Langkah uji untuk pemilik** (setelah `npm run dev`; akun dengan `WorkforceTransportAllowance`
dan `TransportAllowancePolicy : Read`):

1. Buka detail Profil Penggajian seorang pegawai. Harapan: tombol **Tunjangan Transportasi** tampil.
   Dengan akun tanpa `WorkforceTransportAllowance : Read`, tombol tidak tampil.
2. Untuk pegawai tanpa tunjangan, tambah tunjangan dari kebijakan Tetap Bulanan dengan nominal 0.
   Harapan: tersimpan; kolom Nominal Bulanan menampilkan "Mengikuti kebijakan".
3. Setelah tersimpan, tombol Tambah nonaktif dan keterangan "hanya satu tunjangan per pegawai" tampil.
4. Ubah tunjangan, pilih kebijakan Per Kehadiran. Harapan: keterangan metode manual tampil di form.
5. Isi Nominal Bulanan lebih besar dari Batas Bulanan Pegawai. Harapan: ditolak di form.
6. Pada pegawai uji yang punya dua tunjangan bulanan lama berurutan, kosongkan tanggal selesai yang
   pertama. Harapan: peringatan `CALC-01` muncul di form sebelum menyimpan; peringatan di atas daftar
   muncul setelah disimpan.
7. Hapus tunjangan. Harapan: konfirmasi menyebut nama kebijakan; setelah dikonfirmasi, daftar dimuat
   ulang.
8. Akun yang hanya punya `Read`. Harapan: daftar tampil tanpa tombol Tambah, Ubah, dan Hapus.

**Tidak dijalankan:** `npm run build`, `npm run dev`, uji e2e, uji manual di browser.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar dan form berjalan | Terpenuhi di source dan logika; jalannya di browser **belum** dibuktikan | Bagian 3.2, 6 |
| 2. Peringatan tampil sebelum menyimpan bila pegawai sudah punya transport bulanan aktif yang beririsan | Terpenuhi (statis dan skrip); peringatan di dalam form, ditambah peringatan data lama di atas daftar | `findOverlappingMonthlyAllowances`, form modal |
| 3. Tombol dijaga `WorkforceTransportAllowance : *` | Terpenuhi: masuk = `Read`, Tambah = `Create`, Ubah = `Update`, Hapus = `Delete` | Hook dan view |
| DoD: kriteria terbukti | Sebagian; uji manual **belum** | Bagian 6 |
| DoD: jalan masuk ada | Terpenuhi | Tombol di detail Profil Penggajian |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |
| Verifikasi roadmap: build | **Belum** | `NOT RUN` atas instruksi pemilik |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 3 peringatan ESLint `react-hooks/set-state-in-effect` dengan pola yang sama di seluruh hook Payroll V2; tidak diperbaiki di luar cakupan |
| Selisih desain dengan backend | Desain `FE-PGJ-10` mengandaikan pegawai bisa punya beberapa tunjangan, sedangkan backend menolak tunjangan kedua. Karena itu peringatan `CALC-01` hanya muncul pada penyuntingan data lama yang sudah ganda. Tombol Tambah dinonaktifkan dengan penjelasan, bukan dibiarkan berakhir 400. Bila pegawai memang perlu tunjangan berurutan (pindah kebijakan di tengah tahun), aturan satu-tunjangan-per-pegawai di backend perlu diputuskan pemilik |
| Masalah yang diketahui | (1) Peta cara hitung diambil dari kebijakan **aktif** saja; tunjangan yang memakai kebijakan nonaktif tidak dibandingkan, sejalan dengan mesin hitung. (2) Pemeriksaan irisan memakai paling banyak 100 tunjangan per pegawai. (3) Kolom saldo (accrued, used, paid, remaining) tidak bisa diubah dari layar ini; nilainya dibawa apa adanya saat ubah. (4) Pilihan Komponen Payroll memuat semua komponen aktif, termasuk jenis potongan, karena backend menerimanya |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Task ini: ` M` `payroll-compensation-detail-view.jsx`, ` M` `use-payroll-compensation-detail.js`; `??` route `[id]/transport-allowance/`, folder view `transport-allowance/`, konstanta, hook, service, dan utils transport. Perubahan `PAY-FE-14` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: `npm run build` dan langkah uji bagian 6. Task berikutnya: `PAY-FE-16` (Tunjangan tetap pada Struktur Gaji) |
