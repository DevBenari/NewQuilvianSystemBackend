# Laporan Perubahan Frontend — `FE-RWI-048`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-048` |
| Judul | Resep dan tindakan dari satu layar |
| Slice | `DOK-MVP-FE` urutan 6 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap.md` §3 kartu `FE-RWI-048` |
| Trace | `FE-DOK-06`; `03-frontend-architecture.md` §3.6; `RUL-DOK-01`; `INV-DOK-09`; `INT-DOK-03`, `INT-DOK-06`, `INT-DOK-07` |
| Contract version | `0.3.0` — `approved` oleh Muhammad Hamzah, 3 September 2026 |
| Wewenang UI | `skema-tampilan-dokter-rawat-inap.md` §4–5, §11, §14, §16–20, §22 dan rules §1 roadmap |
| Dependency | `FE-RWI-043` ✅ selesai; `BE-RWI-050` ✅ selesai; `BE-RWI-051` 🟡 sebagian — uji percobaan ulang PostgreSQL dilewati atas keputusan pemilik 5 September 2026 |
| Klasifikasi | `MEDIUM` — satu base component baru, dua service diperluas, satu hook, satu utility, satu constant, empat komponen domain, satu stylesheet |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | Dikerjakan di atas `52b07d363e92525739fb2ad63075ec80f6d4e230`, branch `HamzahV2`. Source-nya kemudian **di-commit pemilik pekerjaan sendiri** sebagai `e194509dc`; agent tidak menjalankan satu pun tindakan Git |
| Commit backend yang dijadikan rujukan | `3a6373e90e5a590bfad1ba214c5c941e602fc245`, branch `MHamzah` |
| Tanggal | 8 September 2026 |
| Status | ✅ `SELESAI`. Kelima acceptance criteria terpetakan ke source yang ada dan seluruh validasi dijalankan. Butir DoD bukti visual dikecualikan atas keputusan pengguna bahwa e2e dan `.mjs` bukan gerbang selesai. Isu §6 percobaan ulang pengiriman tagihan **tetap terbuka** dan tidak dinyatakan lulus |

---

## 1. Keadaan yang ditemukan di awal

Tab **Resep & Tindakan** masih berupa kerangka. Di sisi service, `prescription.service.js` dan
`patient-procedure.service.js` sudah ada tetapi keduanya hanya melayani jalur rawat jalan: yang
tersedia adalah `getActivePrescriptionByConsultation` dan `getPatientProcedures`, bukan pembacaan
per perawatan rawat inap.

Dua hal yang berbahaya bila dibiarkan:

1. **Resep obat pulang tidak terbedakan.** Kolom `PrescriptionOrderType` sudah ada di backend sejak
   `BE-RWI-050` dengan tiga nilai — `Routine`, `Daily`, `Discharge` — tetapi tidak ada satu pun
   layar yang membacanya. Obat pulang yang terbaca sama dengan resep harian adalah kesalahan yang
   sampai ke pasien.
2. **Kegagalan penerbitan tagihan berpotensi menjatuhkan halaman.** `INV-DOK-09` menetapkan bahwa
   kegagalan Billing tidak membatalkan catatan klinis. Bila layar menerjemahkan kegagalan itu
   menjadi galat halaman, tindakan yang sudah benar-benar dikerjakan akan tampak seperti tidak
   pernah terjadi.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter yang berwenang atas pasien rawat inap.

**Kapan layar ini dibuka.** Ketika dokter perlu meresepkan obat atau mencatat tindakan, dan ketika
ia perlu memeriksa apakah resep yang sudah ditulis sudah sampai ke Farmasi.

**Langkah normalnya, berurutan:**

1. Dokter membuka tab **Resep & Tindakan**. Tab ini **satu**, bukan dua, dan di dalamnya ada dua
   segmen: **Resep** dan **Tindakan**. Masing-masing membawa angka jumlah barisnya.
2. **Segmen Resep** menampilkan tabel berkolom Tanggal, Jenis, Dokter, Status Resep, dan
   **Farmasi (hanya baca)**. Kolom Jenis membedakan **Rutin**, **Harian**, dan **Obat Pulang**
   dengan penanda berwarna sekaligus berlabel teks.
3. Kolom Farmasi menampilkan keadaan pemenuhan milik petugas Farmasi — misalnya "Dalam Antrean
   Farmasi" atau "Sudah Diserahkan". Sel itu bergaris putus-putus untuk menegaskan bahwa ia hanya
   dibaca. **Tidak ada tombol menandai obat sudah diserahkan di mana pun**, dan itu disengaja:
   kewenangan itu milik Farmasi.
4. **Segmen Tindakan** menampilkan Tindakan, Status Klinis, Dokter, Waktu, dan **Billing**. Status
   klinis dan keadaan tagihan berdiri di dua kolom terpisah.
5. Untuk membuat resep, dokter menekan **Buat Resep**. Modal terbuka dan meminta **catatan dokter
   yang menaunginya** lebih dulu, lalu jenis resep dan catatan klinis. Untuk tindakan, alurnya
   sama dengan pilihan tindakan dari master.

**Mengapa catatan dokter diminta lebih dulu.** Kontrak backend mewajibkan `ConsultationId` pada
setiap resep dan tindakan baru — resep menempel pada catatan dokter yang sah, bukan berdiri
sendiri. Layar meminta pilihan itu terang-terangan, karena menyembunyikan syarat tersebut di balik
tombol yang selalu gagal hanya akan membuat dokter mengira layarnya rusak. Bila perawatan itu
belum punya satu pun catatan dokter, modal mengatakannya dan menunjukkan ke mana harus pergi:
tab **Catatan Perkembangan**.

**Ketika tagihan gagal terbit.** Tindakan yang sudah **Selesai** tetap tertulis **Selesai**. Yang
berubah hanya penanda pada kolom Billing barisnya, berbunyi **"Belum terbit"**. Di bawah tabel
muncul satu keterangan: catatan klinisnya tetap tersimpan, dan penerbitan tagihan diulang pemilik
Billing — bukan dari layar ini. Yang tersedia hanya **Baca Ulang Status**. Halaman tetap dapat
dipakai sepenuhnya.

**Jalur tidak normal:**

- **Belum ada resep** dan **belum ada tindakan** adalah dua keadaan terpisah dengan kalimatnya
  masing-masing. Satu kosong tidak membuat yang lain ikut kosong.
- **Gagal memuat salah satu daftar** — hanya daftar itu yang menampilkan galat beserta **Coba
  Lagi**; segmen lainnya tetap terbaca.
- **Keadaan Farmasi tidak dikenal** — ditulis "Keadaan farmasi belum diketahui", **tidak pernah**
  dipalsukan menjadi "Sudah Diserahkan".
- **Tanpa hak tulis** — tombol Buat Resep dan Tambah Tindakan tidak ditampilkan, dan alasannya
  dikatakan sekali di atas daftar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/frontend-roadmap.md` kartu `FE-RWI-048`, rules §1.1–§1.4, dan §6 baris retry Billing
- `contracts/api-contract.md` §5, §6, dan §11
- `skema-tampilan-dokter-rawat-inap.md` §11 beserta §11.1 dan §11.2
- `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` (read-only)
- `Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs` (read-only)
- `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` (read-only)
- `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionDtos.cs` (read-only)
- Empat enum: `PrescriptionOrderType`, `PrescriptionStatus`, `PrescriptionFulfillmentStatus`,
  `PatientProcedureStatus` (read-only)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/ui/doctor-clinical-base/ClinicalSegmentedNav.jsx` | **Baru.** Navigasi segmen di dalam satu tab. Dirender sebagai `role="radiogroup"`, **bukan** `tablist` kedua — shell sudah memasang satu `tablist` untuk enam tab utama, dan menyarangkan tablist kedua membuat relasi tab/panel terbaca ganda oleh pembaca layar |
| `src/components/ui/doctor-clinical-base/clinical-segmented-nav.module.css` | **Baru.** Seluruh nilai visual memakai token |
| `src/components/ui/doctor-clinical-base/index.js` | `ClinicalSegmentedNav` diekspor |
| `src/lib/services/health-services/pharmacy-management/prescription.service.js` | Ditambah `getPrescriptionsByEpisode`. **Nol** fungsi tulis status penyerahan ditambahkan, dan itu disengaja |
| `src/lib/services/health-services/clinical-management/patient-procedure.service.js` | Ditambah `getPatientProceduresByEpisode` dan `createPatientProcedure` beserta kunci permintaan |
| `src/lib/constants/health-services/inpatient-management/inpatient-prescription-procedure-constants.jsx` | **Baru.** Empat enum, segmen, dan seluruh salinan teks |
| `src/utils/health-services/inpatient-management/inpatient-prescription-procedure-utils.jsx` | **Baru.** Normalisasi, penerjemah status, penentu keadaan tagihan per baris, dan penyusun kunci permintaan |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-procedure.jsx` | **Baru.** Membaca resep, tindakan, catatan dokter, dan master tindakan secara terpisah |
| `…/tabs/medication-procedure/prescription-procedure-tab.jsx` | Kerangka diganti isi sebenarnya |
| `…/tabs/medication-procedure/inpatient-prescription-list.jsx` | **Baru.** Tabel resep |
| `…/tabs/medication-procedure/inpatient-procedure-list.jsx` | **Baru.** Tabel tindakan beserta kolom Billing |
| `…/tabs/medication-procedure/create-order-modal.jsx` | **Baru.** Satu modal untuk kedua jenis, karena keduanya menuntut catatan dokter lebih dulu |
| `src/style/health-services/inpatient-management/physician-prescription-procedure.module.css` | **Baru.** |
| `tests/unit/inpatient-physician-clinical-tabs.test.mjs` | Empat uji khusus task ini ditambahkan |

### 3.3 Kepatuhan arsitektur frontend

Alur `constants → utils → service → hook → view` diikuti. Base yang dipakai ulang:
`ClinicalDataTable`, `ClinicalSectionPanel`, `ClinicalStatusBadge`, `ClinicalEmptyState`,
`ClinicalStateBoundary`, `ClinicalActionGuard`, `ClinicalSafetyAlert`, `ClinicalAuditBadge`.

Satu base baru dibuat, `ClinicalSegmentedNav`, sesuai penugasan roadmap §1.2 kepada task ini.
Gerbang keputusan membuktikan `ClinicalTabNav` tidak dapat dipakai: ia memasang
`role="tablist"` pada tingkat tab utama, dan menyarangkannya merusak relasi tab/panel yang sudah
dibangun `FE-RWI-043`.

Seluruh tabel memakai `ClinicalDataTable`, yang sudah memasang `data-flat-table="true"` sehingga
kontrak typography tabel ikut terpakai. Tidak ada `<table>` mentah yang ditulis.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membaca daftar resep..." dan "Membaca daftar tindakan..." berdiri sendiri-sendiri per segmen |
| Kosong | "Belum ada resep pada perawatan ini." dan "Belum ada tindakan pada perawatan ini." adalah dua kalimat berbeda; satu kosong tidak menular ke yang lain |
| Gagal | "Daftar resep tidak dapat dimuat" atau "Daftar tindakan tidak dapat dimuat" beserta **Coba Lagi**, hanya pada segmen yang gagal |
| Gagal menyimpan | Pesan galat muncul di bawah daftar; isian modal tetap utuh |
| Tanpa hak akses | Gerbang penolakan per segmen tanpa membocorkan data |
| Tanpa hak tulis | Tombol Buat Resep dan Tambah Tindakan tidak dirender; alasannya dikatakan sekali lewat penjaga kewenangan |
| Hanya baca | Kolom Farmasi dan kolom Billing selalu hanya baca, bagi peran mana pun |
| Kiriman ganda | Tombol konfirmasi modal nonaktif selama permintaan berjalan, dan kunci permintaan menjamin kiriman ulang tidak melahirkan baris kembar |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Pharmacy Management / Prescription

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/pharmacy-management/prescriptions/episodes/{episodeId}` | Membaca seluruh resep satu perawatan beserta jenis dan keadaan pemenuhannya | `Prescription : Read` |
| `POST` | `/v1/health-services/pharmacy-management/prescriptions` | Membuat resep beserta `InpEpisodeId`, jenis, dan kunci permintaan | `Prescription : Create` |

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-procedures/episodes/{episodeId}` | Membaca tindakan satu perawatan beserta keadaan penerbitan tagihannya | `PatientProcedure : Read` |
| `GET` | `/v1/health-services/clinical-management/patient-procedures/master-options` | Mengisi pilihan tindakan pada modal | `PatientProcedure : Read` |
| `POST` | `/v1/health-services/clinical-management/patient-procedures` | Mencatat rencana tindakan beserta `InpEpisodeId` dan kunci permintaan | `PatientProcedure : Create` |

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/doctor-consultations/episodes/{episodeId}/soap-timeline` | Mengisi pilihan **catatan dokter** pada modal, karena kontrak mewajibkan `ConsultationId` | `DoctorConsultation : Read` |

**Tidak ada satu pun endpoint tulis status penyerahan yang dipanggil, dan tidak ada yang
tersedia.** `api-contract.md` §6 menyatakannya disengaja, dan laporan `BE-RWI-050` mencatat bahwa
permukaan penyerahan obat memang tidak ada sama sekali di backend.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error | `PASS` | Keluaran `eslint . --quiet` kosong |
| `npm run test:unit` | 542 uji lulus, 0 gagal | `PASS` | `tests 542 / pass 542 / fail 0`; 21 di antaranya uji baru `tests/unit/inpatient-physician-clinical-tabs.test.mjs` |
| `npm run build` | Berhasil beserta `postbuild` | `PASS` | `✓ Compiled successfully in 33.2s` |
| Rutin, Harian, dan Obat Pulang menghasilkan tiga label berbeda | Ketiganya berbeda | `PASS` | `FE-RWI-048 K1` |
| Keadaan pemenuhan tidak dikenal tidak dipalsukan menjadi "Sudah Diserahkan" | Nilai `99` dan nilai kosong menghasilkan "Keadaan farmasi belum diketahui" | `PASS` | `FE-RWI-048 K3` |
| Tindakan `Completed` dengan tagihan gagal tetap berstatus Selesai | Status klinis "Selesai", Billing "Belum terbit" | `PASS` | `FE-RWI-048 K4` |
| Kiriman resep berulang memakai kunci yang sama; jenis berbeda memakai kunci berbeda | Keduanya sesuai | `PASS` | `FE-RWI-048 K5` |
| Pemindaian "Tandai Diserahkan" pada keempat berkas segmen | Nol hasil | `PASS` | `FE-RWI-048 K2` |
| Grep warna literal, `!important`, dan `<table>` mentah | Nol hasil | `PASS` | Grep checklist konsistensi UI |
| Verifikasi interaktif di peramban | Tidak dijalankan | `NOT RUN` | Lihat catatan di bawah |
| Percobaan ulang pengiriman tagihan | Tidak diuji | `NOT APPLICABLE` | Mutasinya belum dikontrak; lihat bagian 8 |

**Uji manual:** `NOT FEASIBLE`.

**Alasan konkret.** Tidak ada `playwright.config.*` di akar repository, sehingga
`npm run test:e2e` tidak dapat dijalankan tanpa menambah konfigurasi baru. Pengujian yang bermakna
juga menuntut data master resep, tindakan, dan tarif yang layak — tertahan `RWI-UI-GAP-007`.

**Tidak dijalankan:** `npm run test:e2e`, `npm run test:uat`, screenshot dua segmen tiga viewport,
dan uji mutasi percobaan ulang Billing. Yang terakhir memang **sengaja tidak diuji**, sesuai
roadmap Verification: "tidak menguji mutation retry Billing yang belum dikontrak".

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Resep obat pulang **terbedakan** dari resep harian pada daftar | Terpenuhi | Kolom Jenis merender `ClinicalStatusBadge` dengan label dan tone berbeda per `PrescriptionOrderType`. Uji `FE-RWI-048 K1` membuktikan ketiga labelnya berbeda |
| 2. **Tidak ada tombol menandai obat sudah diserahkan** di seluruh layar | Terpenuhi | Uji pemindaian `FE-RWI-048 K2` atas keempat berkas segmen mengembalikan nol hasil. Service resep pun tidak memuat satu pun fungsi tulis status pemenuhan |
| 3. Status pemenuhan tampil hanya-baca | Terpenuhi | Kolom Farmasi merender teks di dalam `span` bergaris putus-putus, tanpa kontrol apa pun. Nilai tak dikenal disebut "belum diketahui", tidak dinaikkan menjadi "Sudah Diserahkan" — uji `FE-RWI-048 K3` |
| 4. Kegagalan penerbitan tagihan tampil sebagai **penanda pada barisnya**, bukan galat halaman | Terpenuhi | `describeProcedureBilling` menghasilkan penanda per baris; `ClinicalStateBoundary` segmen tindakan hanya menampilkan galat ketika **pembacaan daftar** gagal, bukan ketika tagihan gagal terbit. Uji `FE-RWI-048 K4` membuktikan status klinis tetap "Selesai" |
| 5. Pengiriman resep berulang tidak melahirkan resep ganda di layar | Terpenuhi | Kunci permintaan diturunkan dari catatan dokter, jenis resep, dan perawatan — bukan diacak. Uji `FE-RWI-048 K5`. Tombol konfirmasi juga nonaktif selama permintaan berjalan |

### Butir Definition of Done

| Butir | Status |
| --- | --- |
| Kelima acceptance existing terbukti | Terpenuhi |
| State dan permission terbukti | Terpenuhi pada source dan tabel bagian 4; belum diverifikasi di peramban |
| Visual Acceptance Criteria terbukti | **Belum diverifikasi di peramban.** Dikecualikan atas keputusan pengguna bahwa e2e dan `.mjs` bukan gerbang selesai |
| Gate §4.1 relevan (butir 8, 13, 14, 19) | Terbukti pada source: final tanpa direct edit, fulfillment hanya baca tanpa Tandai Diserahkan, kegagalan Billing hanya per baris, tidak ada global finalize |
| Laporan memuat bukti dua segmen, galat per baris, dan idempotency | Terpenuhi pada bagian 6 dan 7 |
| Handoff `ClinicalSegmentedNav` | Terpenuhi — lihat bagian 8 |
| Backend `BE-RWI-050`/`BE-RWI-051` tidak dilepas | Terpenuhi. `BE-RWI-051` 🟡 dicatat apa adanya |
| Isu retry Billing dan `VAL-DOK-20` tetap dicatat sesuai §6 | Terpenuhi — lihat bagian 8 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` pada scope task ini. Satu uji struktural `FE-RWI-043` sempat gagal karena tab ini belum memakai penjaga kewenangan secara eksplisit; **diperbaiki** dengan menambahkan `ClinicalActionGuard` pada berkas tab, bukan dengan melonggarkan ujinya |
| Masalah yang diketahui | `api-contract.md` §5 dan §6 masih menandai pembacaan per episode sebagai **Rencana**, padahal keduanya sudah hidup di backend. Selisih dokumen dilaporkan, tidak diperbaiki dari sini |
| Dependency backend | `BE-RWI-050` ✅ selesai. `BE-RWI-051` 🟡 **sebagian** — butir integration test PostgreSQL untuk percobaan ulang belum terpenuhi, dilewati atas keputusan pemilik 5 September 2026. Dampaknya di layar: kunci permintaan sudah stabil dan teruji, tetapi perilakunya di bawah dua permintaan serentak belum dibuktikan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Saat pekerjaan agent selesai, berkas task ini muncul sebagai `??` dan `M` pada branch `HamzahV2`, dan **tidak ada tindakan Git yang dijalankan agent** — tanpa `git add`, commit, push, merge, maupun rebase. Pemilik pekerjaan kemudian meng-commit sendiri sebagai `e194509dc`, sehingga `git status --short` pada repository frontend kini bersih |
| Langkah berikutnya | Meminta kontrak percobaan ulang pengiriman tagihan dari pemilik Billing dan ClinicalManagement, lalu menjalankan verifikasi interaktif begitu data master tersedia |

### Isu §6 yang **tetap terbuka** dan tidak dinyatakan lulus

| Isu | Keadaan sekarang di layar | Yang dibutuhkan |
| --- | --- | --- |
| **Percobaan ulang pengiriman tagihan.** Skema §11.2 menggambar tombol **Coba Lagi** pada baris yang gagal, tetapi `api-contract.md` §5 hanya mengontrak create, execute, dan read; `INT-DOK-03` menyebut daftar percobaan ulang tanpa aksi frontend | Layar menampilkan kegagalan **per baris** dan menyediakan **Baca Ulang Status** saja — membaca ulang keadaan, bukan mengirim ulang. Tidak ada endpoint dispatch retry yang dikarang, dan `execute` tidak pernah dikirim ulang | Kontrak dan permission aksi retry dari pemilik Billing serta ClinicalManagement |
| **Peringatan obat pulang `VAL-DOK-20`** belum dikerjakan di backend menurut laporan `BE-RWI-050` | Layar tidak mengarang peringatan apa pun. Jenis **Obat Pulang** hanya dibedakan pada kolom Jenis | Penyelesaian `VAL-DOK-20` di backend |

### Handoff base component baru

| Base | Konsumen menurut roadmap §1.2 | Catatan pemakaian |
| --- | --- | --- |
| `ClinicalSegmentedNav` | `FE-RWI-049` **hanya bila** kelak memakai segmen | Props `segments` berisi `{key, label, badge}`, `activeSegment`, `onSegmentChange`, `ariaLabel`. `FE-RWI-049` pada revision ini memakai **dua section**, bukan segmen, sehingga base ini tidak dipakainya dan tidak ada dependency baru antar-cabang yang dibuat |

Sudah diekspor pada `src/components/ui/doctor-clinical-base/index.js`.
