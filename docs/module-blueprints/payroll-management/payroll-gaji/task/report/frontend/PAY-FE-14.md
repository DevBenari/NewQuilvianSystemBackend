# Laporan Perubahan Frontend — `PAY-FE-14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-14` |
| Judul | Kebijakan Tunjangan Transportasi |
| Slice | `MVP-1` — `EPIC PGJ-03`, layar `FE-PGJ-09` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-14` |
| Trace | FR-PGJ-020, FR-PGJ-024 (keterangan `CALC-02`); PRL-DEC-46; `../../../03-frontend-architecture.md` bagian 1, 2, 3.6, 6, 7 |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12). Endpoint yang dikonsumsi sudah ada sebelum `PGJ-C1` (`WfpTransportAllowancePolicyController`) |
| Wewenang UI | Isi layar, sumber data, hak akses tombol, dan bunyi keterangan dikunci `03-frontend-architecture.md`. Letak butir menu di grup "Kebijakan Tunjangan" dikunci `02-module-map.md`. Nama butir, bentuk form, dan tata letak `DEV_DISCRETION` lewat gerbang keputusan base component |
| Dependency | — (tanpa dependency backend; sembilan endpoint `TransportAllowancePolicy` sudah ada) |
| Klasifikasi | `MEDIUM` — satu fitur master data penuh (tujuh berkas source, lima route, dua registrasi) dengan pola rujukan yang sudah ada; satu elemen `COMPOSE` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repo backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fc9e74d64` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `6722d7ca` (branch `Yasmina`), dengan perubahan `PAY-BE-14`–`23` yang belum di-commit (tidak menyentuh controller kebijakan transport) |
| Tanggal | 2026-11-12 |
| Status | ✅ — source selesai; ESLint 0 galat; uji unit Payroll lolos; agen tidak menjalankan `npm run build` dan `npm run dev` atas instruksi pemilik; pada 2026-11-13 pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen) lalu menginstruksikan status naik ke ✅; **`npm run build` tetap `NOT RUN`** |

---

## 1. Keadaan yang ditemukan di awal

- Backend sudah punya sembilan endpoint baku master data untuk kebijakan transport di
  `api/v1/corporate/payroll-management/transport-allowance-policies`, dengan hak
  `TransportAllowancePolicy : Read/Create/Update/Delete`.
- Frontend belum punya layar atau slice untuk kebijakan transport. Pencarian `transport-allowance`
  di `src/` hanya menemukan field ringkasan pegawai milik HR.
- Grup menu Payroll "Kebijakan Tunjangan" sudah memuat Tunjangan Shift, Siaga, dan Risiko. Ketiganya
  berbentuk fitur master data baku (tujuh berkas plus route), sehingga Tunjangan Risiko
  (`payroll/master-data/hazard-allowance-policy`) menjadi rujukan langsung.
- Sejak `PAY-BE-18`, run hanya menghitung otomatis cara hitung `FixedMonthly`. Metode lain mendapat
  peringatan `CALC-02` saat run dihitung, tetapi petugas belum diberi tahu ketika menyusun
  kebijakannya.

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Petugas Payroll yang memegang hak `TransportAllowancePolicy` di Akses Role.

**Langkah:**

1. Petugas membuka Payroll → Data Induk → Kebijakan Tunjangan → **Tunjangan Transportasi**.
   Butir menu hanya tampil bagi pemegang `TransportAllowancePolicy : Read`.
2. Layar daftar menampilkan ringkasan (total, aktif, nonaktif, masuk payroll, berbasis kehadiran,
   kena pajak), saringan baku master data, dan tabel kebijakan.
3. Petugas menekan **+ Tambah Kebijakan Tunjangan Transportasi** (hanya tampil bagi pemegang
   `Create`), lalu mengisi nama, cara hitung, nominal bulanan, batas bulanan, serta pilihan prorata,
   pajak, masuk payroll, dan cakupan (legal entity, fasilitas, grade, komponen payroll).
4. Bila cara hitung dipilih selain **Tetap Bulanan**, keterangan kuning muncul di atas form:
   "Metode ini belum dihitung otomatis di run. Gunakan Penghasilan Variabel."
5. Setelah disimpan, petugas diarahkan ke halaman detail. Di sana tombol **Perbarui** (hak
   `Update`) dan **Hapus** (hak `Delete`) tersedia, dan keterangan yang sama tampil bila cara
   hitungnya bukan Tetap Bulanan.
6. Status aktif diubah lewat saklar **Status Aktif** pada form Perbarui. Standar master data
   melarang tombol Aktifkan/Nonaktifkan di halaman detail.

**Contoh:** petugas membuat "Transport Bulanan Staf" dengan cara hitung Tetap Bulanan, nominal
Rp 400.000, batas Rp 300.000, dan diprorata. Saat run dihitung (`PAY-BE-18`), pegawai yang punya
tunjangan transport dari kebijakan ini menerima paling banyak Rp 300.000 sebulan, diprorata hari
berlaku.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Tetap Bulanan dengan nominal 0 | Form menolak sebelum dikirim: "Nominal Bulanan wajib lebih dari 0 untuk cara hitung Tetap Bulanan." |
| Per Kehadiran dengan nominal per kehadiran 0 | Ditolak: "Nominal per Kehadiran wajib lebih dari 0 untuk cara hitung Per Kehadiran." |
| Tanggal selesai sebelum tanggal mulai | Ditolak pada field Selesai Berlaku |
| Nama kebijakan sudah dipakai | Backend menolak; pesannya tampil apa adanya sebagai toast merah |
| Hapus kebijakan yang sudah dipakai tunjangan transport pegawai | Backend menolak 400; modal tetap terbuka dan pesannya tampil apa adanya |
| Legal entity diganti | Pilihan fasilitas dikosongkan dan dipersempit ke fasilitas milik legal entity itu |
| Fasilitas dipilih lebih dulu | Legal entity pemiliknya terisi otomatis |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- **Governance:** `AGENTS.md` dan `CLAUDE.md` frontend; `rules/frontend/frontend-architecture.md`,
  `master-data-feature-standard.md`, `base-component-catalog.md`, `base-component-decision-gate.md`,
  `ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`.
- **Rujukan frontend:** 14 berkas fitur `hazard-allowance-policy`; `base-editor-view.jsx`,
  `base-detail-view.jsx`, `information-alert.jsx`, `use-permission.jsx`, `menu-items.jsx`,
  `store.jsx`, `master-data-filter-constants.jsx`, `field-normalizer-config.jsx`,
  `input-normalizer-utils.jsx`; slice opsi `legal-entity`, `hospital-site`, `employee-grade`,
  `payroll-component`; contoh `afterHero` di `employee-profile-change-on-behalf-view.jsx` dan
  `afterForm` di `lab-value-bound-form-view.jsx`.
- **Backend (baca-saja):** `WfpTransportAllowancePolicyController.cs` (route, metadata, ringkasan,
  saringan, validasi, label metode, respons create/update/status/delete),
  `WfpTransportAllowancePolicyDtos.cs`, DTO opsi `LegalEntity`, `HospitalSite`, `EmployeeGrade`,
  `PayrollComponent`, `Program.cs` (opsi JSON).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/master-data/transport-allowance-policy/transport-allowance-policy-constants.jsx` | Baru. `TRANSPORT_ALLOWANCE_POLICY_CONFIG` tunggal sesuai kontrak backend: endpoint, 21 field form, kolom, ringkasan, detail, saringan (`unsupportedFilterKeys` beserta alasannya), validasi tambahan yang mencerminkan backend, pembentuk payload, reset fasilitas saat legal entity diganti, label metode, bunyi keterangan `CALC-02`, dan pasangan hak akses tombol |
| `src/lib/state/slice/payroll/master-data/master-data-transport-allowance-policy-slice.jsx` | Baru. Sembilan thunk lewat `InstanceAxios`, disalin dari rujukan |
| `src/utils/payroll/master-data/transport-allowance-policy/transport-allowance-policy-utils.jsx` | Baru. Disalin dari rujukan; logika `currencyCode` khas Risiko dibuang (backend transport tidak punya mata uang); tambah `isTransportAllowanceManualMethod` |
| `src/lib/hooks/payroll/master-data/transport-allowance-policy/use-master-data-transport-allowance-policy.jsx` | Baru. Pemuatan jenis tunjangan khas Risiko dibuang; tambah `canCreate` (`usePermission`); cleanup `abort` untuk metadata |
| `…/use-master-data-transport-allowance-policy-detail.jsx` | Baru. Tambah `canUpdate`, `canDelete`, dan `showManualMethodNotice` |
| `…/use-master-data-transport-allowance-policy-editor.jsx` | Baru. Sumber pilihan diganti menjadi legal entity, fasilitas (dipersempit legal entity), grade pegawai, dan komponen payroll, dengan parameter baku `{ onlyActive: true, pageNumber: 1, pageSize: 100 }`; memilih fasilitas mengisi legal entity; tambah `showManualMethodNotice` |
| `src/components/view/payroll/master-data/transport-allowance-policy/master-data-transport-allowance-policy-view.jsx` | Baru. Tombol Tambah hanya bila `canCreate` |
| `…/add/transport-allowance-policy-form-view.jsx` | Baru. Keterangan `CALC-02` di slot `afterHero` |
| `…/detail/transport-allowance-policy-detail-view.jsx` | Baru. Perbarui/Hapus dijaga hak; keterangan `CALC-02`; bunyi konfirmasi hapus menyebut sebab penolakan yang sebenarnya |
| `src/app/payroll/master-data/transport-allowance-policy/**` (5 berkas route) | Baru. Route tipis; judul halaman |
| `src/lib/state/store.jsx` | Registrasi reducer `masterDataTransportAllowancePolicy` |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir "Tunjangan Transportasi" di grup "Kebijakan Tunjangan", `requiredPermission: { resource: "TransportAllowancePolicy", action: "Read" }` |

### 3.3 Kepatuhan arsitektur frontend

- **Bentuk:** tujuh berkas source plus dua registrasi, persis standar master data. Strukturnya
  disalin dari rujukan, lalu diisi ulang; tidak ada factory, generator, atau hook master data
  generik.
- **Alur:** route → view → hook → slice/utils/constants, sama dengan rujukan.
- **Tanpa tambahan arsitektur:** tidak ada CSS Module baru, tidak ada instance Axios baru, dan
  semua request lewat `InstanceAxios` dengan `signal` serta cleanup `abort`.
- **Cacat rujukan yang sengaja tidak ditiru:**
  1. Relasi `legalEntities` dan `employeeCategories` di konstanta Risiko tidak pernah dimuat
     editor-nya (cacat "`optionMap` kosong" pada standar bagian 8). Di transport, keempat relasi
     benar-benar dimuat dari thunk `/options` masing-masing.
  2. Nominal di form Risiko terkirim sebagai teks angka. Di transport, field nominal diberi
     `valueType: "decimal"`, sehingga utils yang sama mengirimnya sebagai angka.
- **Hak akses tombol (baru dibanding rujukan):** memakai `usePermission` yang sudah dipakai
  layar Payroll lain. Tidak ada nama peran di kode.

**Gerbang keputusan base component:**

| Elemen | Base component | Status | Bukti |
| --- | --- | --- | --- |
| Hero, ringkasan, saringan, pencarian, tabel, paginasi, badge status | `Hero`, `SummaryCards`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `DataTable`, `RegionPagination`, `StatusBadge` | `REUSE` | Sama dengan `master-data-hazard-allowance-policy-view.jsx` |
| Tombol Tambah, Perbarui, Hapus, Kembali | `BaseButton` | `REUSE` | Sama dengan rujukan; hanya dibungkus syarat hak akses |
| Halaman detail dan konfirmasi hapus | `BaseDetailView` (+ `ConfirmModal` di dalamnya) | `REUSE` | Sama dengan rujukan |
| Form tambah/perbarui | `BaseEditorView` | `REUSE` | Sama dengan rujukan |
| Penolakan akses | `AccessDeniedGate` | `REUSE` | Sama dengan rujukan |
| Keterangan `CALC-02` bersyarat | `InformationAlert` di slot `afterHero` | `COMPOSE` | `afterHero` dipakai di `employee-profile-change-on-behalf-view.jsx` dengan `InformationAlert variant="warning"`; `BaseDetailView` juga punya `afterHero` |

**Pilihan untuk elemen `COMPOSE` (keterangan `CALC-02`):**

1. **(Direkomendasikan, dipakai)** `InformationAlert variant="warning"` di slot `afterHero` milik
   `BaseEditorView` dan `BaseDetailView`, hanya tampil bila cara hitung bukan Tetap Bulanan.
   - Konsistensi: sama persis dengan pemakaian di layar HR yang sudah ada.
   - Risiko regresi: nol, karena base component tidak diubah.
   - Biaya: belasan baris.
   - Kekurangan: letaknya di atas form, bukan tepat di bawah isian Cara Hitung. Isian itu sengaja
     diletakkan di urutan kedua supaya tetap berdekatan.
2. `description` dinamis pada isian Cara Hitung, yang berubah menurut nilai form.
   - Konsistensi: tampil sebagai teks bantu abu-abu, sehingga kurang menonjol untuk peringatan.
   - Risiko regresi: rendah.
   - Biaya: daftar field harus dibangun ulang setiap kali nilai berubah.
3. `afterForm` di bawah form.
   - Konsistensi: ada preseden (`lab-value-bound-form-view.jsx`).
   - Kekurangan: letaknya jauh dari isian dan baru terlihat setelah menggulir.

`UI GATE: PASS — 0 NEW, 0 EXTEND, 1 COMPOSE (tanpa perubahan base component), sisanya REUSE`

Catatan 2026-11-13: pemilik menguji layar di peramban lewat `npm run dev` dan melaporkan lulus; tidak ada screenshot yang disimpan, sehingga `VISUAL CHECK: NOT FEASIBLE` di atas tetap berlaku bagi agen.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan "Mengambil data kebijakan tunjangan transportasi..."; detail "Mengambil detail kebijakan tunjangan transportasi..."; tombol simpan "Menyimpan..." / "Memperbarui..." |
| Kosong | "Data kebijakan tunjangan transportasi tidak ditemukan." dengan saran "Coba gunakan filter lain atau tambahkan data baru." |
| Gagal | Kotak galat merah berisi pesan dari server apa adanya; toast merah untuk gagal simpan/hapus |
| Tanpa hak akses | Butir menu tidak tampil tanpa `Read`; tombol Tambah/Perbarui/Hapus tidak tampil tanpa `Create`/`Update`/`Delete`; penolakan server ditangani `AccessDeniedGate` |
| Berisi | Ringkasan enam kartu, tabel dengan kolom Kode, Nama, Cara Hitung, Nominal Bulanan, Batas Bulanan, Masuk Payroll, Status |

## 5. Endpoint yang dikonsumsi

#### `Corporate / Payroll Management / Transport Allowance Policy`

Base URL: `/v1/corporate/payroll-management/transport-allowance-policies`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan cara hitung, urutan, jumlah baris | `TransportAllowancePolicy : Read` |
| `GET` | `/summary` | Enam kartu ringkasan | `TransportAllowancePolicy : Read` |
| `GET` | `/` | Tabel daftar | `TransportAllowancePolicy : Read` |
| `GET` | `/options` | Thunk tersedia untuk layar lain (`PAY-FE-15`) | `TransportAllowancePolicy : Read` |
| `GET` | `/{id}` | Halaman detail dan isi form Perbarui | `TransportAllowancePolicy : Read` |
| `POST` | `/` | Tambah kebijakan | `TransportAllowancePolicy : Create` |
| `PUT` | `/{id}` | Perbarui kebijakan, termasuk status aktif | `TransportAllowancePolicy : Update` |
| `PATCH` | `/{id}/status` | Thunk tersedia; respons backend `data: null`, sehingga reducer memakai nilai dari permintaan | `TransportAllowancePolicy : Update` |
| `DELETE` | `/{id}` | Hapus (ditandai) | `TransportAllowancePolicy : Delete` |

Pilihan relasi pada form memakai `/options` milik Legal Entity, Hospital Site, Employee Grade, dan
Payroll Component yang sudah ada.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 14 berkas baru, `store.jsx`, `menu-items.jsx` | 0 galat, 2 peringatan `react-hooks/set-state-in-effect` | `PASS` | Keluaran perintah |
| Peringatan yang sama pada rujukan `hazard-allowance-policy` | 0 galat, 2 peringatan di baris yang setara | `EXISTING WARNING` | Keluaran perintah |
| Uji unit Payroll dan filter menu (`tests/unit/payroll-*.test.mjs`, `menu-permission-filter.test.mjs`) | 111 uji, 110 lolos, 1 gagal (`M0` Bank Darah) | `PASS` untuk Payroll | Keluaran perintah; `payroll-module-contract` lolos dengan butir menu baru |
| `M0` Bank Darah dijalankan terhadap `menu-items.jsx` versi HEAD | Tetap gagal dengan cara yang sama | `EXISTING / ENVIRONMENT ISSUE` | Salinan HEAD di scratchpad |
| `npm run test:unit` penuh | 4.800 uji, 4.753 lolos, 47 gagal; tidak ada yang menyebut transport atau payroll | `PASS` (tanpa galat baru) | Keluaran perintah |
| `master-data-sidebar-grouping` (4 gagal) terhadap menu versi HEAD | Tetap 4 gagal | `EXISTING / ENVIRONMENT ISSUE` | Salinan HEAD di scratchpad |
| Skrip pemeriksa utils sekali pakai (di luar repo) | Nominal terkirim angka 400000; relasi kosong `null`; `currencyCode` tidak terkirim; Tetap Bulanan 0 dan Per Kehadiran 0 ditolak; tanggal terbalik ditolak; penanda metode manual benar untuk 5 nilai; ganti legal entity mengosongkan fasilitas; `isActive` hanya saat update | `PASS` | Keluaran skrip |
| Grep anti-regresi UI (`<button`, `btn-`, `<table`, `fw-`/`fs-`, inline style, warna literal, CSS baru) | Bersih; satu `fs-4` pada ikon menu mengikuti semua butir saudaranya | `PASS` | Keluaran perintah |
| `npm run build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| `npm run dev` dan uji manual oleh pemilik (2026-11-13) | Dilaporkan berhasil dan seluruh uji lulus. Tidak dirinci: layar dan akun yang dipakai, hasil per langkah uji; tidak ada screenshot tersimpan | `OWNER-REPORTED` — tidak diverifikasi agen; bukan pengganti `npm run build` | Pesan pemilik |

`AUTOMATED TEST: SKIPPED (opsional) — tidak menambah test baru; suite yang ada dijalankan (lihat di atas).`

Uji manual: `NOT FEASIBLE` — agen tidak menjalankan `npm run dev` atas instruksi pemilik, dan tidak
punya sesi login untuk memeriksa layar.

**Langkah uji untuk pemilik** (setelah `npm run dev`, akun dengan hak `TransportAllowancePolicy`):

1. Buka Payroll → Data Induk → Kebijakan Tunjangan → Tunjangan Transportasi. Harapan: ringkasan dan
   tabel tampil; saringan status, jumlah baris, dan pencarian mengubah isi tabel; tombol reset
   mengembalikan saringan.
2. Tambah kebijakan Tetap Bulanan dengan nominal 0. Harapan: ditolak di form. Isi Rp 400.000 lalu
   simpan. Harapan: pindah ke detail tanpa keterangan kuning.
3. Perbarui kebijakan itu, ganti cara hitung ke Per Kehadiran. Harapan: keterangan kuning muncul di
   atas form. Simpan; detail juga menampilkan keterangan itu.
4. Pilih legal entity, lalu buka pilihan fasilitas. Harapan: hanya fasilitas milik legal entity itu.
5. Matikan Status Aktif lewat form Perbarui. Harapan: badge di daftar menjadi Nonaktif.
6. Hapus kebijakan yang belum dipakai pegawai. Harapan: kembali ke daftar. Coba hapus yang sudah
   dipakai. Harapan: modal tetap terbuka dengan pesan penolakan dari server.
7. Masuk dengan akun yang hanya punya `TransportAllowancePolicy : Read`. Harapan: butir menu tampil,
   tetapi tombol Tambah, Perbarui, dan Hapus tidak tampil.
8. Akun tanpa `Read`. Harapan: butir menu tidak tampil.

Uji manual oleh pemilik (2026-11-13): `OWNER-REPORTED`, lulus. Pemilik menjalankan `npm run dev` dan melaporkan seluruh uji lulus; hasil per langkah tidak dirinci dan tidak diverifikasi agen. `npm run build` tetap `NOT RUN`.

**Tidak dijalankan oleh agen:** `npm run build`, `npm run dev`, uji e2e, uji manual di browser.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar, detail, tambah, ubah, status, hapus berjalan | Terpenuhi di source dan logika (status lewat saklar form Perbarui sesuai standar master data); pengujian pemilik: lulus (2026-11-13, tanpa rincian) | Bagian 3.2, 6 |
| 2. Cara hitung selain "Bulanan tetap" menampilkan keterangan "Metode ini belum dihitung otomatis di run. Gunakan Penghasilan Variabel." | Terpenuhi (form dan detail). Label backend untuk metode ini adalah "Tetap Bulanan" | Form view, detail view; skrip pemeriksa |
| 3. Butir menu dijaga `TransportAllowancePolicy : Read`; tombol tulis dijaga `Create`, `Update`, `Delete` | Terpenuhi | `menu-items.jsx`; `canCreate`, `canUpdate`, `canDelete` |
| 4. Empat keadaan (memuat, kosong, gagal, berisi) ada | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | Bagian 4 |
| DoD: kriteria terbukti | Terpenuhi menurut pernyataan pemilik (2026-11-13); build produksi belum (`NOT RUN`) | `OWNER-REPORTED`; bagian 6 |
| DoD: butir menu terdaftar | Terpenuhi | `menu-items.jsx` baris 1591–1597 |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |
| Verifikasi roadmap: build | **Belum** | `NOT RUN` atas instruksi pemilik |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 2 peringatan ESLint `react-hooks/set-state-in-effect`, warisan pola resolusi token dari rujukan; tidak diperbaiki di luar cakupan |
| Masalah yang diketahui | (1) Pilihan Komponen Payroll memuat semua komponen aktif, termasuk jenis potongan, karena backend menerimanya; tidak dibatasi supaya tidak mengarang aturan. (2) Rujukan Risiko masih punya cacat relasi kosong dan nominal terkirim sebagai teks; dicatat, tidak diperbaiki karena di luar cakupan. (3) Blueprint menulis "Bulanan tetap", sedangkan backend memberi label "Tetap Bulanan"; layar memakai label backend |
| Dependency backend | `NONE`. Controller transport memakai pengecualian `QBE-CODE-002` bertenggat (`PAY-BE-13`); tidak mempengaruhi layar |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M src/lib/state/store.jsx`, ` M src/utils/menu-sidebar/menu-items.jsx`, `??` enam folder/berkas fitur `transport-allowance-policy`. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Status ✅ atas instruksi pemilik (2026-11-13). Sisa: `npm run build` produksi (`NOT RUN`) bila pemilik menginginkannya sebagai bukti tambahan. |
