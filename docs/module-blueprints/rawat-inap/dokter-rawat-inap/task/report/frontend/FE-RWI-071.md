# Laporan Perubahan Frontend — `FE-RWI-071`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-071` |
| Judul | Tab Resep: Buat Resep, Template Resep pribadi, History Resep, Resep Harian (`FE-DOK-10` pada `FE-DOK-09`) |
| Slice | Gelombang 2 — `PRD-RWI-V2-001`, `EPIC DOK-13` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) — kartu `FE-RWI-071` |
| Trace | `FR-DOK-086`, `FR-DOK-088`, `FR-DOK-089`, `FR-DOK-090`, `FR-DOK-091`; `RWI-DEC-121`, `RWI-DEC-122`, `RWI-DEC-134`, `RWI-DEC-135`, `RWI-DEC-152`; `VAL-DOK-56`, `VAL-DOK-56a`, `VAL-DOK-56c`, `VAL-DOK-57`; `BE-RWI-099` [BE], `BE-RWI-105` [BE] |
| Contract version | `0.6.0` API resep, API resep harian, dan API template resep |
| Wewenang UI | `FE-DOK-10` sebagai tab Resep tersendiri di dalam kerangka `FE-DOK-09` |
| Dependency | `FE-RWI-067` ✅ selesai 17 September 2026; `BE-RWI-099` [BE] ✅ selesai 16 September 2026; `BE-RWI-105` [BE] ✅ selesai 17 September 2026 |
| Klasifikasi | `MEDIUM` — pemisahan tab Resep dari Tindakan, formulir resep rawat inap terintegrasi pencarian formularium, isolasi template pribadi (`ownerScope=Mine`), penanda keselamatan alergi & obat tidak tersedia, serta Resep Harian per periode waktu |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Tanggal | 17 September 2026 |
| Status | ✅ **Selesai 17 September 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-6) terbukti pada source code. |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum task ini dijalankan:
1. **Resep dan Tindakan Masih Tergabung dalam Satu Tab Legacy (`FE-DOK-06`):**
   Tab Resep belum berdiri sendiri sebagai `FE-DOK-10`. Layar rawat inap lama menggabungkan Resep dan Tindakan dalam tab segmen ganda sederhana yang hanya memuat tabel resep tanpa alur pembuatan resep yang memadai.
2. **Ketiadaan Formulir Buat Resep Terstruktur (`AC-1`):**
   Dokter rawat inap hanya disuguhi modal kecil pembuatan resep tanpa fasilitas pencarian formularium master obat, tanpa pengaturan dosis, frekuensi, signa, maupun racikan terperinci.
3. **Ketiadaan Integrasi Template Resep Pribadi (`AC-2`, `FR-DOK-089`):**
   Dokter tidak dapat melihat atau mengelola template resep miliknya sendiri di ruang kerja rawat inap. Tidak ada filter `ownerScope=Mine`, sehingga berisiko menampilkan template bersama atau template dokter lain yang tidak relevan.
4. **Ketiadaan Penanda Keselamatan Klinis saat Menerapkan Template (`AC-3`, `FR-DOK-090`):**
   Saat template diterapkan, tidak ada deteksi bentrok alergi aktif pasien atau pengecekan ketersediaan obat di apotek yang menandai butir bermasalah tanpa menggagalkan butir obat lain.
5. **Celah Penyimpanan Draf yang Membawa Butir Bertanda (`AC-4`, `VAL-DOK-57`):**
   Belum ada validasi pencegahan simpan draf di frontend saat draf membawa obat bentrok alergi atau obat yang tidak tersedia di apotek.
6. **Ketiadaan Monitoring Resep Harian Berdasarkan Periode (`AC-5`, `FR-DOK-086`, `BE-RWI-099`):**
   Dokter yang melakukan visite pagi tidak memiliki layar monitoring obat berjalan yang menyaring periode waktu (Hari Ini, Minggu Ini, Bulan Ini, dan Rentang Tanggal), tidak dapat melihat obat yang telah dihentikan beserta alasannya, dan tidak dapat membedakan resep obat pulang (`Discharge`).
7. **Penerimaan Template Kosong Tanpa Validasi (`AC-6`, `FR-DOK-091`):**
   Tidak ada validasi pencegahan pembuatan template resep tanpa obat.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter Penanggung Jawab Pelayanan (DPJP), Dokter Konsulen, dan Dokter Jaga yang merawat pasien di rawat inap.

**Kapan tab dibuka.** Saat dokter membuka Ruang Kerja Dokter Rawat Inap (`FE-DOK-09`), memilih pasien, lalu menekan tab **Resep** (`FE-DOK-10`).

**Alur proses bisnis bertahap:**

### 2.1 Buat Resep Rawat Inap (`AC-1`, `FR-DOK-086`, PRD bagian 19)
1. Dokter memilih jenis resep: **Harian**, **Rutin**, atau **Obat Pulang** (`PrescriptionOrderType`).
2. Dokter memilih catatan perkembangan dokter (SOAP) yang menaungi resep tersebut (`ConsultationId`), sesuai kontrak wajib backend.
3. Di kolom kiri, dokter mengetik nama obat pada kolom pencarian formularium.
4. Hasil pencarian menampilkan nama obat dagang, nama generik, kekuatan dosis, dan sediaan. Dokter menekan **"+ Tambah ke Resep"**.
5. Obat masuk ke kolom draf resep di sebelah kanan. Dokter dapat menyesuaikan dosis, frekuensi (misal: `3x1`), aturan pakai / signa (misal: `Sesudah makan`), dan jumlah yang diresepkan.
6. Dokter menekan **"Simpan Draft Resep"** untuk mengirim resep ke farmasi/sistem rawat inap.

### 2.2 Template Resep Pribadi Dokter Login (`AC-2`, `FR-DOK-089`)
1. Dokter berpindah ke sub-tab **Template Resep**.
2. Sistem memanggil backend dengan parameter `ownerScope=Mine` (`BE-RWI-105`), sehingga daftar yang tampil **hanya template resep milik dokter akun login**, bukan template dokter lain atau template poliklinik bersama.
3. Setiap kartu template menampilkan nama template, kode, kategori, deskripsi, serta ringkasan komposisi (jumlah obat umum dan racikan).
4. Dokter dapat menekan tombol **"Terapkan ke Resep"** atau menghapus template miliknya sendiri.

### 2.3 Penanda Keselamatan saat Menerapkan Template (`AC-3`, `FR-DOK-090`)
1. Pasien Budi memiliki riwayat alergi tercatat: *Paracetamol*. Apotek saat ini kehabisan stok: *Omeprazole*.
2. Dokter menerapkan template "Terapi Infeksi & Demam" yang berisi Ceftriaxone, Paracetamol, dan Omeprazole.
3. Saat diterapkan:
   - **Ceftriaxone** masuk ke draf resep normal tanpa penanda.
   - **Paracetamol** masuk ke draf resep dengan penanda bahaya merah: `[AllergyConflict]` ("Bentrok Alergi Pasien: Paracetamol").
   - **Omeprazole** dilaporkan tidak tersedia dan tidak dimasukkan ke draf.
4. Sistem menampilkan notifikasi: *"Template diterapkan ke draft resep. Periksa obat yang bertanda bentrok alergi atau tidak tersedia."* Proses penerapan **tidak gagal secara keseluruhan**, melainkan menandai butir bermasalah secara transparan.

### 2.4 Pencegahan Penyimpanan Draf yang Masih Membawa Butir Bertanda (`AC-4`, `VAL-DOK-57`)
1. Dokter kembali ke draf resep yang masih memuat Paracetamol bertanda bentrok alergi.
2. Banner peringatan klinis `ClinicalSafetyAlert tone="danger"` menyala di atas formulir: *"Resep Membawa Obat Bertanda Bahaya: Masih ada obat yang bentrok dengan riwayat alergi pasien atau tidak tersedia di apotek. Hapus atau ganti obat tersebut sebelum menyimpan draft resep (VAL-DOK-57)."*
3. Tombol **"Simpan Draft Resep"** otomatis terkunci (`disabled`). Jika dokter mencoba menyimpan, sistem menampilkan galat penolakan dan mewajibkan penghapusan atau penggantian obat bentrok alergi terlebih dahulu.

### 2.5 Resep Harian & Monitoring Obat Berjalan (`AC-5`, `FR-DOK-086`, `BE-RWI-099`)
1. Dokter visite pagi membuka sub-tab **Resep Harian**.
2. Tersedia bilah saringan periode: **Hari Ini** (`Today`), **Minggu Ini** (`ThisWeek`), **Bulan Ini** (`ThisMonth`), dan **Rentang Tanggal** (`Range`).
3. Sistem memuat daftar resep pada periode tersebut:
   - Kartu resep memaparkan nomor resep, waktu penerbitan (WIB), nama dokter penulis, dan lencana jenis resep.
   - Resep obat pulang ditandai khusus dengan lencana hijau **"Obat Pulang"** (`Discharge`).
   - Butir obat yang telah dihentikan ditandai dengan latar belakang merah dan lencana **"Dihentikan"**, lengkap dengan catatan informatif: *"Dihentikan oleh dr. Rina pada 17/09/2026 10:15 WIB. Alasan: Pasien mual/alergi."*
   - Racikan ditampilkan beserta nama formula, jumlah bungkus, signa, dan rincian bahan-bahannya.

### 2.6 Penolakan Template Kosong (`AC-6`, `FR-DOK-091`)
1. Dokter mencoba membuat template baru dari draf resep yang tidak memiliki satu pun obat atau racikan.
2. Sistem menolak dengan pesan validasi yang jelas: *"Template harus berisi sekurang-kurangnya satu obat (FR-DOK-091)."*

---

## 3. Gerbang Keputusan Base Component (`base-component-decision-gate.md`)

```
UI GATE: 8 elemen — REUSE 7, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti Source | Status | Rekomendasi |
| :--- | :--- | :--- | :---: | :--- |
| Sub-navigasi Tab Resep | `ClinicalSegmentedNav` | `src/components/ui/doctor-clinical-base.jsx` | `REUSE` | Digunakan untuk 4 bagian: Buat Resep, Template Resep, History Resep, Resep Harian, lengkap dengan counter badge. |
| Banner Peringatan Keselamatan | `ClinicalSafetyAlert` | `src/components/ui/doctor-clinical-base.jsx` | `REUSE` | Tone `danger` untuk bentrok alergi & pencegahan simpan, tone `info` untuk sukses penerapan. |
| Badge Status Resep & Penghentian | `ClinicalStatusBadge` | `src/components/ui/doctor-clinical-base.jsx` | `REUSE` | Lencana jenis resep (`Rutin`, `Harian`, `Obat Pulang`), status draf, dan lencana `Dihentikan`. |
| State Boundary Asinkron | `ClinicalStateBoundary` | `src/components/ui/doctor-clinical-base.jsx` | `REUSE` | Mengelola pemuatan, pesan kosong, galat, dan otorisasi per sub-panel. |
| Pengunci Akses Penulisan | `ClinicalActionGuard` | `src/components/ui/doctor-clinical-base.jsx` | `REUSE` | Menonaktifkan tombol tulis bila akun dokter tidak berwenang atau episode berstatus tutup. |
| Modal Konfirmasi / Simpan Template | `ConfirmModal` | `src/components/features/base-features/confirm-modal/` | `REUSE` | Dialog pembuatan template pribadi dan konfirmasi hapus template. |
| Kontrol Form & Filter | `BaseInputField`, `BaseNativeSelectField`, `BaseButton` | `src/components/features/base-features/` | `REUSE` | Kontrol masukan baku formulir resep dan filter periode. |
| Panel Penyusun Resep & Pencarian Obat | Panel Klinis Resep | Komposisi form resep rawat jalan | `COMPOSE` | Merangkai pencarian obat master dengan kartu draf butir resep berpenanda alergi. |

---

## 4. Berkas yang Diubah dan Dibuat

### 4.1 Frontend Repository (`QuilvianSystemFrontendDev`)

| Berkas | Status | Deskripsi Perubahan |
| --- | :---: | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-prescription-constants.jsx` | Baru | Mendefinisikan enum segmen Tab Resep, periode Resep Harian (`Today`, `ThisWeek`, `ThisMonth`, `Range`), penanda keselamatan (`AllergyConflict`, `Unavailable`), dan teks pesan peringatan/validasi. |
| `src/utils/health-services/inpatient-management/inpatient-prescription-utils.jsx` | Baru | Menyediakan utilitas evaluasi bentrok alergi (`checkAllergyConflict`), normalisasi resep harian & racikan (`normalizeDailyPrescriptions`), deteksi draf bertanda (`hasFlaggedItemsInDraft`), dan pemformatan keterangan obat dihentikan (`formatStoppedInfo`). |
| `src/lib/services/health-services/pharmacy-management/prescription-template.service.js` | Modifikasi | Menambahkan fungsi CRUD: `createPrescriptionTemplate`, `updatePrescriptionTemplate`, dan `deletePrescriptionTemplate`. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx` | Baru | Hook terpadu pengelola draf resep, pencarian obat formularium, isolasi template pribadi dokter (`ownerScope=Mine`), penandaan bentrok alergi, penyimpanan draf bersyarat (`VAL-DOK-57`), dan pembacaan resep harian per periode. |
| `src/style/health-services/inpatient-management/physician-prescription.module.css` | Baru | Stylesheet modular untuk tata letak kisi formulir penyusun resep dua kolom, kartu template, filter periode, dan kartu resep harian dengan penanda obat dihentikan. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-builder-panel.jsx` | Baru | Panel formulir pembuatan resep rawat inap dengan pencarian formularium, penanda bentrok alergi, modal simpan template baru, dan aksi simpan draf. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-templates-panel.jsx` | Baru | Panel daftar template pribadi dokter login (`ownerScope=Mine`), tombol terapkan ke draf, dan tombol hapus template. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-history-panel.jsx` | Baru | Panel tabel riwayat resep episode dengan status resep klinis dan status Farmasi baca-saja. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-daily-panel.jsx` | Baru | Panel Resep Harian dengan filter periode (Hari Ini, Minggu Ini, Bulan Ini, Rentang), kartu resep, penanda obat dihentikan, dan obat racikan. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/inpatient-prescription-tab.jsx` | Baru | Komponen kontainer utama Tab Resep `FE-DOK-10` dengan navigasi `ClinicalSegmentedNav`. |
| `src/components/view/health-services/inpatient-management/physician-workspace/components/physician-workspace-tabs.jsx` | Modifikasi | Menghubungkan tab key `prescription` ke `InpatientPrescriptionTab`. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/doctor-inpatient-view.jsx` | Modifikasi | Mengarahkan konfigurasi `TAB_COMPONENTS.prescription` ke `InpatientPrescriptionTab`. |

---

## 5. Dokumentasi Endpoint Bergaya Swagger

#### Health Services / Pharmacy Management / Prescription

| Method | Path | Kegunaan | Auth / Hak Akses | Request / Response |
| --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Mengambil resep harian dengan filter periode (`Today`, `ThisWeek`, `ThisMonth`, `Range`), butir obat aktif, butir dihentikan, dan racikan | `Prescription : Read` | Query: `period`, `from`, `to`, `orderType`<br/>Response: `PagedResult<InpatientPrescriptionListItem>` |
| `POST` | `/` | Membuat resep rawat inap baru (draf/aktif) | `Prescription : Create` | Body: `CreatePrescriptionRequest` (berisi `episodeId`, `consultationId`, `orderType`, `items`, `compounds`)<br/>Response: `PrescriptionResponse` |

#### Health Services / Pharmacy Management / Prescription Template

| Method | Path | Kegunaan | Auth / Hak Akses | Request / Response |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Mengambil template resep pribadi dokter login dengan filter `ownerScope=Mine` (FR-DOK-089) | `PrescriptionTemplate : Read` | Query: `ownerScope=Mine`, `pageSize=50`<br/>Response: `PagedResult<PrescriptionTemplateResponse>` |
| `GET` | `/{id}` | Mengambil rincian butir obat dan racikan suatu template resep | `PrescriptionTemplate : Read` | Path: `id:guid`<br/>Response: `PrescriptionTemplateDetailResponse` |
| `POST` | `/` | Membuat template resep pribadi baru berkonteks rawat inap (FR-DOK-088) | `PrescriptionTemplate : Create` | Body: `CreatePrescriptionTemplateRequest` (`serviceContext: "Inpatient"`, `items`, `compounds`)<br/>Response: `PrescriptionTemplateResponse` |
| `DELETE` | `/{id}` | Menghapus template resep pribadi (hanya dokter pemilik) | `PrescriptionTemplate : Delete` | Path: `id:guid`<br/>Response: `200 OK` |
| `POST` | `/{id}/apply` | Menerapkan template ke resep draf dengan evaluasi penanda keselamatan | `PrescriptionTemplate : Create` | Path: `id:guid`, Body: `ApplyPrescriptionTemplateRequest`<br/>Response: `ApplyPrescriptionTemplateResponse` (`hasFlaggedItems`, `items`) |

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti Implementasi pada Kode Sumber |
| --- | :---: | --- |
| **AC-1** (`FR-DOK-086`, PRD bag. 19) | ✅ | Buat Resep bekerja melalui `useInpatientPrescriptionTab` dan `prescription-builder-panel.jsx`, mengaitkan `consultationId`, `orderType`, item obat formularium, signa, dosis, dan menyimpan via `createPrescription`. |
| **AC-2** (`FR-DOK-089`) | ✅ | Daftar template resep di rawat inap dipanggil secara khusus dengan parameter `ownerScope: "Mine"` pada `getPrescriptionTemplates`, sehingga hanya memuat template milik dokter akun login. |
| **AC-3** (`FR-DOK-090`) | ✅ | Penerapan template mengevaluasi bentrok alergi via `checkAllergyConflict` dan penanda ketersediaan; butir yang bentrok masuk dengan flag `AllergyConflict` dan peringatan bahaya tanpa menggagalkan penerapan butir lainnya. |
| **AC-4** (`VAL-DOK-57`) | ✅ | Fungsi `saveDraftPrescription` memeriksa `hasFlaggedItemsInDraft(draft)`. Jika draf masih membawa obat bertanda alergi / tidak tersedia, tombol simpan dinonaktifkan dan sistem menampilkan penolakan `VAL-DOK-57`. |
| **AC-5** (`FR-DOK-086`, `BE-RWI-099`) | ✅ | Sub-tab Resep Harian (`prescription-daily-panel.jsx`) menyediakan filter periode `Hari Ini`, `Minggu Ini`, `Bulan Ini`, dan `Rentang Tanggal`; menampilkan butir aktif, butir dihentikan (`isStopped`, waktu, penghenti, alasan), racikan dan lencana khusus `Obat Pulang` (`Discharge`). |
| **AC-6** (`FR-DOK-091`) | ✅ | Fungsi `saveDraftAsTemplate` memvalidasi kelengkapan template: jika draf tidak memiliki item obat maupun racikan, sistem menolak pembuatan template dengan pesan `templateEmptyWarning`. |
| **DoD**: Lint & Build Bersih | ✅ | `npm run lint` PASS (0 error) dan `npm run build` PASS (standalone build sukses). |
| **DoD**: Dokumentasi Terperbarui | ✅ | Laporan tracked ini dibuat, kartu roadmap `frontend-roadmap-v2.md` dan matriks `requirement-traceability-v2.md` diperbarui. |
