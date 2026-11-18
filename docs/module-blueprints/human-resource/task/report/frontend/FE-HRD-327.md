# Laporan Perubahan Frontend — `FE-HRD-327`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-327` |
| Judul | Verifikasi slice S-A8: navigasi, build, dan uji layar di browser |
| Slice | `S-A8` / Verifikasi — gelombang 3 |
| Roadmap | `roadmap/frontend-roadmap.md` bagian Slice S-A8, kartu `FE-HRD-327` |
| Trace | API Contract §33; Permission §31 (`WorkforcePayrollData : Read/Update`); `HRD-DEC-221` (pilihan master); pasangan backend `BE-HRD-363` |
| Contract version | `v27` bagian 33 — approved 16 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Verifikasi saja; tidak ada perubahan source |
| Dependency | `FE-HRD-325` ✅, `FE-HRD-326` ✅, `BE-HRD-363` ✅ |
| Klasifikasi | `LIGHT` — tanpa perubahan source; lint dan build seluruh repository, uji browser baca saja |
| Task mode | `FRONTEND` (instruksi pengguna 18 November 2026 "lanjut FE-HRD-327") |
| Target tulis | Laporan ini, roadmap, dan traceability di repository backend. Tidak ada berkas frontend yang ditulis |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `baad7dd62` (branch `YogaV2`) beserta perubahan `FE-HRD-325` dan `FE-HRD-326` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `a17a2bc3` (branch `yoga`) beserta perubahan `BE-HRD-360` s.d. `363` yang belum di-commit |
| Tanggal | 18 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terbukti; `npm run lint:errors` dan `npm run build` lulus; menu tampil dan hilang menurut hak; kedua layar diuji di desktop dan ponsel; alamat API yang dipanggil sama dengan `[Route]` backend |

---

## 1. Keadaan yang ditemukan di awal

Halaman daftar (`FE-HRD-325`) dan formulir ubah (`FE-HRD-326`) sudah terverifikasi di browser pada hari yang sama. Yang belum dibuktikan untuk slice ini adalah: butir menu benar-benar hilang bagi pengguna tanpa hak, lint dan build seluruh repository sesudah perbaikan `FE-HRD-326`, serta kecocokan alamat service dengan route backend.

---

## 2. Proses bisnis dari sisi pengguna

1. Pengguna yang memegang `WorkforcePayrollData : Read` melihat butir **Data Penggajian Pegawai** di **Sumber Daya Manusia › Administrasi Kepegawaian**, lalu membuka daftar.
2. Pengguna yang juga memegang `WorkforcePayrollData : Update` melihat kolom **Aksi** dengan tombol **Ubah** untuk membuka formulir.
3. Pengguna yang hanya memegang Read melihat daftar yang sama **tanpa** kolom Aksi, sehingga tidak dapat membuka formulir.
4. Pengguna tanpa hak `WorkforcePayrollData` tidak melihat butir menu itu sama sekali. Butir lain di grup yang sama, misalnya **Riwayat Kepegawaian**, tetap tampil. Bila alamat halaman dibuka langsung, backend menolak `403` dan layar menampilkan "Ups! Akses Ditolak" (dibuktikan `FE-HRD-325`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`src/utils/menu-sidebar/menu-items.jsx` (butir `corporateWorkforcePayrollData`); `src/lib/constants/hr/workforce-core/workforce-payroll-data-constants.js` (endpoint); `src/lib/axiosInstance/InstanceAxios.jsx` (`baseURL` dari `NEXT_PUBLIC_API_QUILVIAN`); controller backend `WorkforcePayrollDataController.cs`, `SalaryStructureController.cs`, `BankController.cs`.

### 3.2 Berkas yang berubah

`NOT APPLICABLE` — task verifikasi; tidak ada berkas frontend yang diubah. Berkas yang masih berubah di working tree milik `FE-HRD-325` dan `FE-HRD-326`.

### 3.3 Kepatuhan arsitektur frontend

Butir menu memakai `requiredPermission: { resource: "WorkforcePayrollData", action: "Read" }`, sama dengan pola menu lain. Endpoint dikumpulkan di satu berkas konstanta dan dipanggil lewat `InstanceAxios`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Sudah dibuktikan `FE-HRD-325` (daftar) dan `FE-HRD-326` (formulir) |
| Kosong | Sudah dibuktikan `FE-HRD-325` |
| Gagal | Sudah dibuktikan `FE-HRD-325` dan `FE-HRD-326` |
| Tanpa hak akses | Menu hilang tanpa `WorkforcePayrollData : Read`; kolom Aksi hilang tanpa Update (task ini); "Ups! Akses Ditolak" pada `403` (`FE-HRD-325`) |

---

## 5. Endpoint yang dikonsumsi

Dicocokkan dengan `[Route]` controller backend (AC4). `baseURL` Axios berisi akar API (`…/api`), sehingga path frontend `/v1/…` menjadi `api/v1/…` di backend.

#### Corporate / Human Resource / Workforce Core / Payroll Data

| Method | Path frontend | `[Route]` + atribut backend | Alamat yang dipanggil di browser | Hasil |
| --- | --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-core/payroll-data` | `api/v1/corporate/human-resource/workforce-core/payroll-data` + `[HttpGet]` | `https://localhost:7184/api/v1/corporate/human-resource/workforce-core/payroll-data` | `200` |
| `GET` | `…/payroll-data/summary` | `[HttpGet("summary")]` | `…/payroll-data/summary` | `200` |
| `GET` | `…/payroll-data/{workforceProfileId}` | `[HttpGet("{workforceProfileId:guid}")]` | `…/payroll-data/{workforceProfileId}` | `200` |
| `PUT` | `…/payroll-data/{workforceProfileId}` | `[HttpPut("{workforceProfileId:guid}")]` | Dibuktikan `FE-HRD-326` (task ini baca saja) | `200` |

#### Corporate / Human Resource / Master Data — pilihan formulir

| Method | Path frontend | `[Route]` + atribut backend | Alamat yang dipanggil di browser | Hasil |
| --- | --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/salary-structures/options` | `api/v1/corporate/human-resource/master-data/salary-structures` + `[HttpGet("options")]`, `SalaryStructure : Read` | `https://localhost:7184/api/v1/corporate/human-resource/master-data/salary-structures/options` | `200` |
| `GET` | `/v1/administrator/master-data/banks/options` | `api/v1/administrator/master-data/banks` + `[HttpGet("options")]`, `Bank : Read` | `https://localhost:7184/api/v1/administrator/master-data/banks/options` | `200` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` (seluruh repository, server lokal mati) | Selesai tanpa error (keluaran hanya baris perintah) | `PASS` (AC2) | `fe-lint-327.log` |
| `npm run build` (sesudah perbaikan `FE-HRD-326`) | "Compiled successfully in 76s"; 816/816 halaman termasuk `○ /hr/workforce-core/payroll-data`; standalone siap; tidak ada berkas tracked lain yang berubah | `PASS` (AC2) | `fe-build-327.log` |
| `check-css-usage.mjs` atas view daftar, kolom tabel, dan modal | "OK — 3 berkas diperiksa, 0 temuan." | `PASS` (AC3) | Keluaran perintah |
| Browser (`next dev` → backend lokal, devYoga; seluruh tulis diblokir) — superadmin | Butir menu ada; daftar dengan kolom Aksi, 25 tombol Ubah | `PASS` (AC1) | `fe-327.mjs`, `327-menu-superadmin.png` |
| — Peran disimulasikan: bukan superadmin, `WorkforcePayrollData : Read` tanpa Update | Butir menu ada; kolom No., Nama / Nomor, Penetapan Gaji Utama, Rekening Payroll, Kelengkapan — **tanpa** Aksi; 0 tombol Ubah | `PASS` (AC1) | `327-menu-read-saja.png` |
| — Peran disimulasikan: tanpa hak `WorkforcePayrollData` | 0 tautan dan 0 teks "Data Penggajian Pegawai" di menu; butir saudara "Riwayat Kepegawaian" tetap ada | `PASS` (AC1) | `327-menu-tanpa-hak.png` |
| — Daftar desktop 1440 px | Dokumen 1425 px, tidak melebar | `PASS` (AC3) | `327-daftar-desktop.png` |
| — Formulir desktop | Modal 430 px; bagian atas dan bawah (opsi) ditangkap; Batal menutup | `PASS` (AC3) | `327-formulir-desktop-atas.png`, `327-formulir-desktop-bawah.png` |
| — Daftar ponsel 390 px | Dokumen 375 px, tabel bergulir di dalam wadahnya | `PASS` (AC3) | `327-daftar-ponsel.png` |
| — Formulir ponsel | Modal 355 px, isian satu kolom, Simpan dan Batal penuh lebar; Batal menutup | `PASS` (AC3) | `327-formulir-ponsel-atas.png`, `327-formulir-ponsel-bawah.png` |
| — Alamat API yang dipanggil | Kelima `GET` menuju `https://localhost:7184/api/v1/…` sesuai tabel bagian 5, semuanya `200` | `PASS` (AC4) | `327-hasil.json` |

Uji manual: `PASS`.

**Tidak dijalankan:** uji dengan akun non-superadmin sungguhan; peran tanpa hak disimulasikan dengan menyuapkan `/v1/auth/permissions` (respons asli, `isSuperAdmin` dimatikan, pasangan hak dibuang), cara yang sama dengan verifikasi layar sebelumnya. Penolakan `403` backend untuk peran itu sudah dibuktikan di `BE-HRD-361` dengan akun non-superadmin. `npm run test:unit` seluruh repository tidak dijalankan; berkas uji slice 23/23 di `FE-HRD-326`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Butir menu **Data Penggajian Pegawai** hanya tampil bagi pemegang `WorkforcePayrollData : Read` | Terpenuhi | Superadmin dan Read-saja: tampil; tanpa hak: hilang |
| 2. `npm run lint:errors` dan `npm run build` lulus tanpa error baru | Terpenuhi | Keduanya keluar `0`; lint tanpa error di seluruh repository |
| 3. Setiap layar dibuka di browser pada lebar desktop dan ponsel, dengan tangkapan layar; pemeriksa `check-css-usage` 0 temuan | Terpenuhi | 6 tangkapan daftar/formulir; 3 tangkapan menu; 0 temuan CSS |
| 4. Alamat service dicocokkan dengan `[Route]` controller backend, bukan dengan service frontend sendiri | Terpenuhi | Bagian 5: route backend dan alamat yang sungguh dipanggil |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Temuan `FE-HRD-326` masih terbuka untuk UI Lead: `BaseCheckboxCard` varian `compact` menampilkan ✓ walau tidak dicentang (terlihat juga di `327-formulir-ponsel-bawah.png`) |
| Masalah yang diketahui | `NONE` untuk slice ini |
| Dependency backend | `BE-HRD-360` s.d. `BE-HRD-363` ✅ |
| Perubahan sampingan | `NONE` — uji baca saja; tidak ada data yang ditulis |
| Interupsi | `NONE` |
| Status Git | Tidak ada perubahan baru dari task ini. Working tree berisi perubahan `FE-HRD-325`/`326`: ` M src/app/hr/workforce-core/payroll-data/page.jsx` · ` M src/components/view/hr/workforce-core/workforce-payroll-data-edit-modal.jsx` · ` M src/lib/hooks/hr/workforce-core/use-workforce-payroll-data.js` · ` M src/utils/hr/workforce-core/workforce-payroll-data-utils.js` · ` M tests/unit/workforce-payroll-data.test.mjs` |
| Langkah berikutnya | Slice S-A8 selesai di backend dan frontend. Tersisa: keputusan UI Lead atas `BaseCheckboxCard` `compact`, penyegaran kontrak (`workforceProfileId` pada respons simpan) dan penutupan G3 pada pass blueprint, serta commit oleh pengguna |
