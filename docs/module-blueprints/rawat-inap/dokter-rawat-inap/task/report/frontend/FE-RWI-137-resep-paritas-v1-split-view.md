# Laporan Perubahan Frontend — `FE-RWI-137-resep-paritas-v1-split-view`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-137` |
| Judul | Paritas V1 Tab Resep Dokter Rawat Inap — split-view tanpa modal katalog, racikan dua kolom, rekonsiliasi sebagai mode ketiga |
| Modul | Rawat Inap — Dokter Rawat Inap, Tab **Resep** |
| Rencana kerja | [`rencana-kerja/resep/resep.md`](../../../roadmap/rencana-kerja/resep/resep.md) — Tahap 4 (implementasi) |
| Bukti V1 | `QuilvianV1/QuilvianSystemFrontendDev/captures/dokter-rawat-inap/05-resep/01..06-*.png`; `src/components/features/Resep-obat/medication-form.jsx` dan `racikan-form.jsx` (V1, baca saja) |
| Trace | `FE-RWI-071`, `FE-RWI-072`, `FE-RWI-095`; `FR-DOK-086`, `FR-DOK-088` s.d. `093`; `BE-RWI-050`, `BE-RWI-099`, `BE-RWI-101`, `BE-RWI-105`, `BE-RWI-127`; `VAL-DOK-52a`, `VAL-DOK-57` |
| Task mode | `FRONTEND` (backend hanya diverifikasi, tidak diubah) |
| Target tulis | `QuilvianSystemFrontendDev` — source dan test; laporan di repository backend ini |
| Commit saat dikerjakan | frontend `6ebec95f355ae17d6cc56585f2135d0e588af024` (branch `HamzahV2`, belum di-commit); backend `a497de0d0eec5facecc9d5d282bb2800bc90d124` |
| Model | Claude Opus 5.5 |
| Tanggal | 29 September 2026 |
| Status | ✅ Selesai di tingkat source — lint bersih, test unit terkait lulus; `npm run build` dan uji runtime `NOT RUN` (alasan di bagian 5) |

---

## 1. Kondisi awal yang ditemukan

Bagian 7 rencana kerja sudah berbunyi *"DISETUJUI & SELESAI DIIMPLEMENTASIKAN"*, tetapi source tidak
mendukung klaim itu. Yang ditemukan pada 29 September 2026 sebelum pekerjaan dimulai:

| Klaim rencana kerja | Kenyataan di source |
| --- | --- |
| Split-view dua kolom, modal katalog dihapus | Buat Resep masih memakai `DoctorDrugCatalogButton` + `DoctorDrugCatalogModal`; tidak ada kolom Daftar Obat maupun form Detail Obat |
| Banner teal, `Gunakan Template`, badge `Asuransi` | Tidak ada banner; tidak ada badge penjamin |
| Tiga pill `Resep` / `Obat Racikan` / `Rekonsiliasi Obat` | Dua sub-navigasi (`Obat Resep`, `Obat Racikan`); rekonsiliasi berupa sub-tab sendiri |
| Racikan dua kolom | `prescription-compound-builder.jsx` (belum di-commit) masih memakai modal katalog |
| Test `inpatient-prescription-compound-builder` 3/3, parity 7/7 | Test tersebut hanya mencocokkan string pada source yang memakai desain modal lama |

Selain itu ditemukan **empat cacat nyata** yang tidak tercatat di rencana kerja:

1. **Bahan racikan hilang saat disimpan.** Racikan dikirim dengan kunci `ingredients`, padahal
   `AutosavePrescriptionCompoundRequest` membaca `Items`. Model binder mengabaikannya tanpa galat,
   sehingga racikan tersimpan tanpa satu pun bahan. Baris bukti `FE-RWI-095` bagian 5 yang menyatakan
   *"Bentuk items/compounds tetap sesuai kontrak — PASS"* karena itu keliru.
2. **Resep kedua dari catatan SOAP yang sama dibuang server.** Kunci idempotensi hanya dibentuk dari
   episode + catatan dokter + jenis resep. Dokter yang menulis resep kedua (jenis sama) dari catatan
   visite yang sama menerima resep pertama kembali (`200`, *"Resep sudah tercatat sebelumnya"*),
   draft-nya dikosongkan, dan isinya hilang tanpa kabar.
3. **Instruksi dokter tidak pernah terkirim.** Kolom "Instruksi Dokter / Petugas Farmasi" diisi di
   layar tetapi `doctorInstruction` tidak ada di payload.
4. **Panel Rekonsiliasi tidak dapat dipakai.** `ConfirmModal` dipanggil dengan `open` (prop yang
   benar `show`) sehingga modal keputusan tidak pernah muncul; `ClinicalActionGuard` dipanggil dengan
   `canWrite` (prop yang benar `allowed`) sehingga tombol keputusan selalu terkunci;
   `ClinicalStateBoundary` diberi prop `state` yang tidak dikenal sehingga keadaan memuat/kosong tidak
   pernah tampil; `ClinicalStatusBadge` diberi *children* padahal membaca `label`, sehingga badge kosong.

---

## 2. Proses bisnis sesudah perubahan

**Pelaku.** DPJP / dokter berpenugasan aktif pada lembar kerja rawat inap.

1. Dokter membuka tab **Resep → Buat Resep**. Banner teal "Resep Obat" menampilkan tombol
   **Gunakan Template** dan badge penjamin (`Asuransi: <nama>` atau `Penjamin: <jenis bayar>`),
   dibaca dari respons katalog obat untuk kunjungan itu.
2. Di bawah banner, dokter memilih **Jenis Resep** (Harian/Rutin/Obat Pulang) dan **Catatan Dokter
   Pengait (SOAP)**. Bila episode belum punya SOAP, tombol simpan terkunci beserta alasannya.
3. Dokter memilih salah satu dari tiga pill:
   - **Resep** — kolom kiri *Daftar Obat* langsung memuat halaman pertama formularium (20 obat,
     tanpa wajib mengetik dua huruf), dengan penghitung "Halaman 1 • Total N obat", badge
     "N Ditampilkan" / "Ada Lebih Banyak", dan tombol **Pilih**. Kolom kanan *Detail Obat*: Nama Obat
     (baca saja), harga & status tanggungan, Jumlah Obat, **Signa [ ] x [ ]**, Signa Tambahan,
     Catatan, Estimasi Pemberian, Cara Pemakaian, lalu **Reset Obat** / **Tambahkan**.
   - **Obat Racikan** — kolom kiri *Buat Racikan Baru (ID)*: Nama Racikan*, Bentuk Racikan (daftar V1
     `MF Cream` … `MF pulv dtd no. da incap`), Jumlah Racikan, Signa Racikan*, Signa Tambahan, daftar
     *Obat dalam Racikan* dengan isian dosis per bungkus, lalu **Reset Form** / **Jadikan Racikan**.
     Kolom kanan *Daftar Obat* hanya memuat obat yang diizinkan sebagai bahan racikan, tombol
     **+ Bahan**.
   - **Rekonsiliasi Obat** — panel oranye *Rekonsiliasi Obat Admisi* dengan badge "N Obat", tombol
     **Refresh**, tabel obat bawaan, dan aksi **Lanjutkan** / **Ubah Aturan Pakai** /
     **Tunda / Hentikan**, masing-masing dengan konfirmasi dan catatan opsional.
4. Butir dan racikan terkumpul di kartu **Resep yang Dipilih** (badge Obat Biasa / Racikan / Total,
   tabel No–Kategori–Detail–Jumlah–Signa–Biaya–Aksi, estimasi total) beserta Catatan Klinis dan
   Instruksi Farmasi, lalu **Bersihkan Draf**, **Simpan sebagai Template Pribadi**, atau
   **Simpan Draft Resep**.
5. Simpan mengirim header + butir + racikan dalam satu `POST /prescriptions` (transaksi atomis
   backend). Draft dikosongkan dan draft baru mendapat token idempotensi baru.

**Jalur tidak normal.**

- Obat atau bahan bentrok alergi → baris ditandai merah, alert keselamatan tampil, tombol simpan
  terkunci sampai diganti (`VAL-DOK-57`).
- Obat yang sudah ada di draft → tombol katalog berubah "Dipilih"; penambahan ganda ditolak dengan pesan.
- Konteks kunjungan belum siap → katalog tidak dipanggil, pesan jujur ditampilkan (mempertahankan `ISS-02`).
- Katalog gagal dimuat → pesan galat + tombol **Coba Lagi**.
- Keputusan rekonsiliasi atas resep yang sudah aktif → pesan `409` dari server (`VAL-DOK-52a`).

**Keputusan desain yang disengaja.** Obat bawaan yang dilanjutkan **tidak** ditambahkan ke Resep yang
Dipilih. Backend `BE-RWI-101` sudah menyalinnya ke draft resep rawat inap dokter di server dalam
transaksi yang sama dengan keputusannya; menambahkannya lagi ke draft lokal akan membuat obat itu
tercatat dua kali saat draft disimpan. Panel menyatakan hal ini secara eksplisit dan menandai baris
yang sudah "Tersalin ke draft resep".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas baru

| Berkas | Isi |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-prescription-builder-utils.js` | Fungsi murni: form V1 → `AutosavePrescriptionItemRequest` / `AutosavePrescriptionCompoundRequest` / `PrescriptionTemplateCompoundRequest`; parser signa, estimasi pemberian, dan kekuatan sediaan; validasi form; kunci idempotensi per draft; template → draft; ringkasan dan counter katalog |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-drug-catalog.jsx` | Katalog inline: muat halaman pertama segera, pencarian debounce 350 ms, paginasi 20, filter bahan racikan, konteks penjamin, reset saat kunjungan berganti |
| `.../tabs/prescription/prescription-drug-catalog-panel.jsx` | Kotak "Daftar Obat" V1 (pencarian, counter, tabel, Pilih / + Bahan, Info) |
| `.../tabs/prescription/prescription-regular-drug-form.jsx` | Kotak "Detail Obat" V1 |
| `.../tabs/prescription/prescription-selected-items-card.jsx` | Kartu "Resep yang Dipilih" V1 |
| `tests/unit/inpatient-prescription-builder-utils.test.mjs` | 9 test perilaku untuk pemetaan payload (termasuk kunci `items` dan idempotensi) |

### 3.2 Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `.../tabs/prescription/prescription-builder-panel.jsx` | Ditulis ulang: banner, badge penjamin, tiga pill, split-view per mode (tetap terpasang saat berpindah pill agar isian tidak hilang), kartu Resep yang Dipilih, ubah butir |
| `.../tabs/prescription/prescription-compound-builder.jsx` | Ditulis ulang menjadi split-view V1; bahan dari katalog inline; bentuk racikan V1; peringatan alergi per bahan |
| `.../tabs/prescription/prescription-reconciliation-panel.jsx` | Ditulis ulang menjadi panel oranye bertabel; perbaikan empat prop komponen dasar (bagian 1 butir 4) |
| `.../tabs/prescription/prescription-templates-panel.jsx` | Header "Daftar Template Resep (nama dokter)", **Buat Template Baru**, pencarian nama, kartu "Template: …" |
| `.../tabs/prescription/prescription-history-panel.jsx` | Kolom No, No Resep, Tanggal & Waktu, Dokter Peresep, Jumlah Item |
| `.../tabs/prescription/inpatient-prescription-tab.jsx` | Rekonsiliasi dipindah ke Buat Resep; sub-tab: Buat Resep, Template Resep, History Resep, Resep Harian, Sliding Scale |
| `src/lib/hooks/.../use-inpatient-prescription-tab.jsx` | Pencarian obat lama dipindah ke hook katalog; butir dibentuk dari form V1; payload lewat util baru; `doctorInstruction` dikirim; token idempotensi per draft; tanpa `setState` sinkron di effect |
| `src/lib/constants/.../inpatient-prescription-constants.jsx` | Mode builder, pilihan Cara Pemakaian dan Bentuk Racikan V1, ukuran halaman katalog, teks layar; `SEGMENT.RECONCILIATION` dihapus |
| `src/utils/.../inpatient-prescription-utils.jsx` | `getFlaggedItemNames` ikut menyebut bahan racikan yang bentrok alergi |
| `src/style/.../physician-prescription.module.css` | Gaya V1 memakai token desain yang ada (`--color-primary`, `--color-success`, `--color-warning`, dst.) |
| `tests/unit/inpatient-prescription-parity.test.mjs`, `inpatient-prescription-compound-builder.test.mjs`, `inpatient-final-consistency.test.mjs` | Assertion string yang mengunci desain modal lama diganti kontrak V1; perubahan diberi catatan bertanggal |

### 3.3 Pemetaan form V1 ke kontrak backend

| Isian V1 | Field backend |
| --- | --- |
| Jumlah Obat | `Quantity` |
| Signa `a x b` | `FrequencyText = "axb"`, `FrequencyPerDay = a`, `Dose = b` |
| Signa Tambahan | `Signa` |
| Catatan | `DoctorNote` |
| Estimasi Pemberian `7 hari` | `DurationValue = 7`, `DurationUnit = "Hari"`; teks yang tidak terbaca diteruskan ke `DoctorNote` |
| Cara Pemakaian | `AdministrationInstruction` |
| Signa Racikan / Signa Tambahan | `FrequencyText` / `Signa` gabungan "3 x 1 • Sesudah makan" |
| Bahan dengan kekuatan diketahui, `mg/bks` | `Items[]` → `CalculationMode = TargetDosePerUnit`, `TargetValue`, `TargetUnitName`; jumlah tablet dihitung ulang `CompoundCalculationService` |
| Bahan tanpa kekuatan, `satuan/bks` | `Items[]` → `CalculationMode = LegacySourceQuantity`, `AmountPerPackage`, `TotalQuantity` |

### 3.4 Dampak kontrak API, database, keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada kontrak baru. Seluruh endpoint sudah ada: `GET /clinical-management/prescribing-drugs` (catatan: rencana kerja menulis `/pharmacy-management/prescribing-drugs` — keliru), `POST /pharmacy-management/prescriptions`, `GET …/prescriptions/episodes/{id}?period=`, template, dan rekonsiliasi |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Tidak berubah. Tombol tulis tetap dijaga `writeAccess.allowed`; rekonsiliasi kini benar-benar memakai `allowed` sehingga keputusan server tetap menjadi penentu akhir |

### 3.5 Verifikasi backend (rencana kerja bab 6.1)

| Butir | Hasil | Bukti |
| --- | --- | --- |
| 1. Atomisitas `POST /prescriptions` | Sudah ada — header, butir, racikan, dan bahan dalam satu transaksi | `PrescriptionController.cs:341-408`, `PrescriptionWorkspaceService.ApplyDraftContentAsync` |
| 2. Filter `period` resep harian tanpa `422` | Sudah ada — `Today/ThisWeek/ThisMonth/Range`; `422` hanya untuk `Range` tanpa `from`/`to` | `PrescriptionController.cs:445-494`, `InpatientPrescriptionService.ResolvePeriod` |
| 3. Keputusan rekonsiliasi menyalin obat ke draft | Sudah ada — butir draft dibuat server dalam transaksi keputusan | `MedicationReconciliationService.cs` (decide, `AddDraftItemAsync`) |
| 4. `dotnet build … --no-incremental` | `NOT RUN` — tidak ada perubahan source backend pada task ini | — |

---

## 4. Tabel keputusan base component

| Elemen UI | Keputusan | Alasan |
| --- | --- | --- |
| Tombol, field teks, select, textarea | `REUSE` | `BaseButton`, `BaseTextField`, `BaseNativeSelectField`, `BaseTextAreaField` |
| Alert keselamatan, guard aksi, state boundary, badge | `REUSE` | `ClinicalSafetyAlert`, `ClinicalActionGuard` (`allowed`), `ClinicalStateBoundary`, `ClinicalStatusBadge` (`label`) |
| Harga, coverage, restriksi obat di katalog | `REUSE` | `DoctorPrescriptionPriceSummary`, `DoctorPrescriptionCoverageBadges`, `DoctorPrescriptionRestrictionBadges` |
| Info klinis obat, catatan umum, modal template | `REUSE` | `DoctorDrugDetailModal`, `DoctorPrescriptionNotes`, `DoctorPrescriptionTemplateCreateModal` |
| Modal konfirmasi rekonsiliasi & hapus template | `REUSE` | `ConfirmModal` (`show`, `loading`, `hideCancel`) |
| Kotak Daftar Obat inline | `CREATE (lokal)` | Modal katalog bersama tidak dapat dipakai inline; baris tabelnya ditulis ulang dengan badge bersama. Tidak dipromosikan ke base karena hanya rawat inap yang memakai tata letak ini |
| Input signa `[ ] x [ ]` | `CREATE (lokal)` | Tidak ada base field berpasangan; `DoctorPrescriptionDrugRow` menyatukan signa di sel tabel, bukan form |
| Pill tiga mode | `CREATE (lokal)` | `ClinicalSegmentedNav` tidak mendukung warna per pill (cyan/hijau/oranye) yang menjadi ciri V1 |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` seluruh berkas terdampak (termasuk test) | exit `0`, 0 error, 0 peringatan | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` tiga berkas test resep | 19/19 lulus | `PASS` | Keluaran perintah |
| Suite unit penuh `tests/unit/*.test.mjs` | 2084 test, 2079 lulus, 5 gagal | `PASS` untuk berkas terdampak | Kelima kegagalan (`accounting-reconciliation`, `hemodialysis-sidebar-navigation`, `menu-permission-filter`, `petty-cash-finance-separation`) membaca `menu-items.jsx` / modul lain yang tidak disentuh task ini — `EXISTING` |
| Semua named import pada 14 berkas terdampak ada di modul tujuannya | 0 masalah | `PASS` | Skrip pemeriksa import di scratchpad |
| `npm run build` | Tidak dijalankan | `NOT RUN` | `next dev` milik pemilik sedang berjalan pada repository yang sama (PID 11268); build akan menimpa `.next` yang dipakainya. Build dijalankan pemilik sendiri |
| Uji runtime di peramban (pilih obat, simpan, racikan, rekonsiliasi) | Tidak dijalankan | `NOT RUN` | Menunggu pemilik; dicatat sesuai keputusan pemilik bahwa uji e2e bukan gerbang selesai |

Perintah baku `npm run lint:errors` dan `npm run test:unit` tidak dipakai karena rusak oleh
lingkungan (`EXISTING / ENVIRONMENT ISSUE`); penggantinya dijalankan seperti di atas.

---

## 6. Acceptance criteria (rencana kerja bab 6.2)

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Banner teal "Resep Obat", `Gunakan Template`, badge `Asuransi` | Terpenuhi | `prescription-builder-panel.jsx` — `builderBanner`, `describePayerBadge` |
| Tiga pill segmented Resep / Obat Racikan / Rekonsiliasi | Terpenuhi | `INPATIENT_PRESCRIPTION_BUILDER_MODES`, `modePill_*` |
| Split-view Resep: Daftar Obat kiri tanpa modal, Detail Obat kanan | Terpenuhi | `PrescriptionDrugCatalogPanel` + `PrescriptionRegularDrugForm`; tidak ada `DoctorDrugCatalogModal` |
| Split-view Racikan: form kiri, Daftar Obat kanan dengan `+ Bahan` | Terpenuhi | `prescription-compound-builder.jsx` |
| Panel oranye Rekonsiliasi dengan Lanjutkan / Ubah Aturan Pakai | Terpenuhi (tanpa injeksi lokal — lihat bagian 2) | `prescription-reconciliation-panel.jsx` |
| Kartu Resep yang Dipilih: badge, tabel, tiga tombol footer, hapus/ubah | Terpenuhi | `prescription-selected-items-card.jsx` |
| Template Resep sesuai `04-template-resep.png` | Terpenuhi | `prescription-templates-panel.jsx` |
| History Resep sesuai wireframe | Terpenuhi | `prescription-history-panel.jsx` |
| Resep Harian | Tidak diubah — capture V1 `06` hanya berisi "Informasi dokter tidak tersedia"; panel `FE-RWI-071` sudah memuat filter periode | — |
| Katalog termuat tanpa mengetik 2 huruf | Terpenuhi | `use-inpatient-drug-catalog.jsx` |
| Unit test lulus tanpa regresi | Terpenuhi untuk berkas terdampak | Bagian 5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di luar cakupan | **Panel Sliding Scale memiliki salah-prop yang sama dengan panel Rekonsiliasi** — `ConfirmModal open=` (4 modal), `ClinicalActionGuard canWrite=` (2), `ClinicalStateBoundary state=`, `ClinicalStatusBadge` dengan *children*, dan `BaseTextField`/`BaseTextAreaField` dengan prop `label` langsung, bukan `field`. Akibatnya modal pesan/ubah/hentikan order tidak pernah terbuka dan tombolnya selalu terkunci. **Tidak diperbaiki di sini**: ±15 titik pada UI dosis insulin (obat high-alert) di luar cakupan `resep.md`, dan tidak dapat diuji langsung. Disarankan menjadi task tersendiri (`FE-RWI-138`) |
| Koreksi dokumen | (1) `FE-RWI-095` bagian 5 baris "Bentuk items/compounds tetap sesuai kontrak — PASS" keliru. (2) Rencana kerja bab 3.1 menulis katalog di `/pharmacy-management/prescribing-drugs`; route sebenarnya `/clinical-management/prescribing-drugs`, dan informasi klinis di `/master-data/drugs/{id}/clinical-information` |
| Stok obat | V1 menampilkan "Stok"; respons `PrescribingDrugResponse` tidak membawa stok, jadi layar menampilkan harga dan status tanggungan saja — tidak dikarang |
| Status Git | Seluruh perubahan belum di-commit. Working tree juga memuat perubahan modul keperawatan (discharge-planning, pain-monitoring, education-assessment, `patient-procedure.service.js`, dll.) dari pekerjaan lain — tidak disentuh task ini |
| Langkah berikutnya | (1) Pemilik menjalankan `npm run build` setelah `next dev` dihentikan. (2) Uji runtime: pilih obat → simpan → cek butir & racikan di History Resep dan di farmasi; resep kedua dari SOAP yang sama harus terbit sebagai resep baru. (3) Putuskan task perbaikan panel Sliding Scale |
