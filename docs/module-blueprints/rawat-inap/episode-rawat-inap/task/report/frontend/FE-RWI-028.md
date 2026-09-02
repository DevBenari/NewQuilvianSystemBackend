# Laporan Perubahan Frontend — `FE-RWI-028`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-028` |
| Judul | Persetujuan rawat inap dapat dicetak |
| Slice | Halaman cetak persetujuan per episode (`FE-INP-18`) beserta langkah 8 alur admisi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 4, entri `FE-RWI-028` |
| Trace | `RWI-DEC-077`; `RWI-DEC-035` isi minimal; `03-frontend-architecture.md` `FE-INP-18` dan 3A.8; `05-skema-tampilan.md` bagian 3.10, bagian 22, bagian 4.3 privasi, bagian 4.4 keputusan reuse |
| Contract version | API `0.4.0`. Kontrak dibaca dari source backend `InpatientEpisodeController.cs` dan `PatientEncounterController.cs` |
| Wewenang UI | Tata letak formulir `DEV_DISCRETION`. Batas yang tetap dipatuhi: ketiga isi minimal `RWI-DEC-035` wajib tercetak, dan layar dilarang menyatakan persetujuan tersimpan atau tertanda tangan |
| Dependency | `FE-RWI-027` (**selesai 1 September 2026**). Tidak ada endpoint baru |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 1, database 0, keamanan/auth 1, UI/workflow 2 |
| Task mode | `FRONTEND` — backend strict read-only, kecuali laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; `NewQuilvianSystemBackend` **hanya** untuk laporan ini beserta tautan buktinya |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `55f91b6cd`, branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `85439d32a884adcde3067304774151b317b058a2`, branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI.** Kelima acceptance criteria terpenuhi dan dibuktikan runtime di peramban. `npm run lint` `0 errors`; `npm run build` `✓ Compiled successfully`; verifikasi peramban `37/37 PASS` |

---

## 1. Keadaan yang ditemukan di awal

Formulir persetujuan rawat inap **belum ada sama sekali**. `05-skema-tampilan.md` bagian
22.3 menyatakannya apa adanya: *"Source saat ini belum memiliki route atau komponen"*.

Yang sudah ada dan dipakai ulang:

| Yang sudah ada | Bukti |
| --- | --- |
| Pola cetak berbasis `@media print` | `src/style/health-services/registration-management/doctor-queues/doctor-certificate.module.css` baris 897, memakai `body * { visibility: hidden }` lalu memunculkan kembali wilayah cetaknya |
| Nama rumah sakit sebagai constant | `PATIENT_CARD_COPY.hospitalName` pada constants kiosk |
| Gerbang akses berbasis jawaban server | `AccessDeniedGate` beserta `getAccessDeniedMessage` yang mengenali 401 dan 403 |

Celah yang menghalangi: `normalizeEpisodeDetail` tidak mengeluarkan `encounterId`, padahal
penjamin — isi wajib formulir — hanya tersimpan pada kunjungan yang menjadi jangkar episode,
bukan pada episodenya. Celah itu ditutup pada task ini bersama `FE-RWI-027`.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas admisi rawat inap.

**Kapan layar ini dibuka.** Dua jalur masuk, dan keduanya menampilkan formulir yang sama:

1. **Dari alur admisi** — langkah ke-8, sesudah admisi dikunci pada langkah Konfirmasi.
2. **Dari Detail Episode** — tombol **Cetak Persetujuan**, menuju
   `/health-services/inpatient-management/episodes/{id}/consent-print`.

**Langkah yang dilakukan berurutan:**

1. Layar menampilkan pratinjau formulir satu halaman berisi kop rumah sakit, judul
   **PERSETUJUAN UMUM RAWAT INAP**, tiga kolom identitas, tiga butir isi persetujuan, ruang
   penerima informasi, dan dua ruang tanda tangan.
2. Di atas pratinjau selalu ada kalimat berikon: *"Formulir ini dicetak, tidak disimpan
   sistem. Lembar bertanda tangan disimpan sesuai tata kelola berkas rekam medis."*
3. Petugas menekan **Cetak**. Dialog cetak peramban terbuka dan **hanya formulirnya** yang
   masuk ke kertas — menu samping, kop aplikasi, tombol, dan seluruh peringatan layar tidak
   ikut tercetak.
4. Petugas menyerahkan lembar itu untuk ditandatangani pasien atau keluarganya, lalu
   menyimpannya sesuai tata kelola berkas rekam medis.

**Contoh konkret isi yang tercetak.** Untuk pasien Sari Dewi dengan penjamin BPJS:

```text
                    RS METROPOLITAN MEDICAL CENTRE
                   PERSETUJUAN UMUM RAWAT INAP
              General Consent — Pelayanan Rawat Inap

Nama Pasien      Sari Dewi        Unit Layanan   Ruang Melati    Cara Bayar   Asuransi
No. Rekam Medis  00123456         Kelas Perawatan Kelas 1        Penjamin     BPJS Kesehatan
Tanggal Lahir    12 April 1988    DPJP           dr. Andi W...   Nomor polis  000123456789
Jenis Kelamin    Perempuan        No. Episode    EP-2026-000204  Tanggal      01 September 2026
```

**Jalur tidak normal:**

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Sedang memuat | Kalimat *"Mengambil data episode untuk formulir persetujuan..."* |
| Data wajib hilang — misalnya penjamin tidak terbaca | Peringatan kuning *"Cetak ditahan sampai data wajib formulir terbaca lengkap"* beserta **daftar bidang yang belum terbaca**; tombol Cetak dinonaktifkan; tombol **Coba Lagi** tersedia |
| Gagal memuat episode | Kalimat merah beserta tombol **Coba Lagi** |
| Detail pasien gagal dibaca | Peringatan kuning; formulir **tetap** dapat dicetak karena nama dan nomor rekam medis sudah datang dari detail episode |
| Tanpa hak akses | Seluruh isi halaman diganti layar **Ups! Akses Ditolak**; formulir tidak pernah terender |
| Nomor episode tidak terbaca dari alamat | Kalimat yang menunjuk Detail Episode atau alur admisi sebagai jalan masuk yang benar |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa diperiksa |
| --- | --- |
| `NewQuilvianSystemBackend/.../Controllers/PatientEncounterController.cs` | Bentuk dan hak akses `GET /patient-encounters/admin/{id}` |
| `NewQuilvianSystemBackend/.../DTOS/PatientEncounterDtos.cs` | Kolom `PatientEncounterDetailResponse` dan `PatientEncounterPaymentResponse` yang menyimpan snapshot penjamin |
| `NewQuilvianSystemBackend/.../Controllers/InpatientEpisodeController.cs` | Hak akses `GET /episodes/{id}` |
| `src/style/health-services/registration-management/doctor-queues/doctor-certificate.module.css` | Pola cetak yang sudah terbukti di repository |
| `src/components/features/base-features/access-denied-gate.jsx` dan `src/utils/access-denied-utils.jsx` | Cara penolakan akses ditegakkan |
| `src/lib/constants/kiosk/registration/patient-card/kiosk-patient-card-print-constants.jsx` | Nama rumah sakit yang sudah menjadi constant |
| `05-skema-tampilan.md` bagian 3.10, 22, 4.3, 4.4 | Kerangka formulir, aturan yang mengikat, privasi, dan keputusan reuse |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Ditambah `INPATIENT_CONSENT_ITEMS` — ketiga isi minimal `RWI-DEC-035` disalin apa adanya dari blueprint — dan `INPATIENT_CONSENT_COPY` |
| `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` | Ditambah `buildInpatientEpisodeConsentPrintRoute` |
| `src/utils/health-services/inpatient-management/inpatient-consent-utils.jsx` | **Baru.** Penyusun isi formulir dan pemeriksa kelengkapan data wajib. Tidak memanggil API sama sekali |
| `src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx` | Ditambah `buildInpatientPayerSummaryFromEncounter`, membaca penjamin dari snapshot kunjungan. Bentuk keluarannya sama persis dengan `buildInpatientPayerSummary` supaya satu komponen formulir melayani kedua jalur masuk |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | `normalizeEpisodeDetail` kini juga mengeluarkan `encounterId` |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-encounter.service.js` | Ditambah `fetchInpatientEncounterDetail` memakai varian `/admin` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-consent-print.jsx` | **Baru.** Controller halaman cetak: tiga pembacaan, nol penulisan |
| `src/components/view/health-services/inpatient-management/inpatient-consent-form.jsx` | **Baru.** Template formulir yang dipakai kedua jalur masuk |
| `src/components/view/health-services/inpatient-management/inpatient-consent-print-view.jsx` | **Baru.** Halaman cetak per episode |
| `src/components/view/health-services/inpatient-management/inpatient-admission-print-steps.jsx` | **Baru.** Langkah 8 di dalam alur admisi (berbagi berkas dengan langkah 9 milik `FE-RWI-029`) |
| `src/app/health-services/inpatient-management/episodes/[id]/consent-print/page.jsx` | **Baru.** Route tipis: hanya membaca parameter dan metadata |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Ditambah tombol **Cetak Persetujuan** — jalur masuk kedua |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Langkah `consent-print` dipasangkan ke komponennya |
| `src/style/health-services/inpatient-management/inpatient-consent-print.module.css` | **Baru.** Seluruh nilai visual dari token; blok `@media print` mengikuti pola `doctor-certificate.module.css` |

### 3.3 Kepatuhan arsitektur frontend

```text
URL (/episodes/{id}/consent-print)
  -> page.jsx (entry point + metadata saja)
  -> inpatient-consent-print-view.jsx (komposisi)
  -> use-inpatient-consent-print.jsx (controller)
  -> inpatient-episode.service.js + inpatient-admission-encounter.service.js + inpatient-admission-patient.service.js
  -> InstanceAxios
  -> inpatient-consent-utils.jsx (fungsi murni)
  -> inpatient-consent-form.jsx (tampilan murni)
```

Komponen formulir sengaja dibuat **tanpa state dan tanpa akses API**. `RWI-DEC-077` memilih
cetak tanpa menyimpan, dan cara paling andal menjaga keputusan itu tetap benar adalah dengan
tidak memberi komponen tersebut jalan untuk menulis ke mana pun.

**Keputusan komponen baru.** `05-skema-tampilan.md` bagian 4.4 sudah menetapkannya:
*"Cetak persetujuan — pola print ada; isi persetujuan Rawat Inap belum ada → Buat template
domain baru di atas shell cetak existing."* Gerbang keputusan base component dijalankan dan
membenarkannya: `ls src/components/features/base-features/` tidak memuat satu pun template
surat, dan `BasePatientCard` dikunci pada dimensi kartu 85,6 mm × 54 mm sehingga tidak dapat
menampung formulir A4. Yang **tidak** dibuat baru adalah mekanika cetaknya — itu dipinjam
apa adanya dari `doctor-certificate.module.css`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kalimat *"Mengambil data episode untuk formulir persetujuan..."* |
| Kosong | Bidang yang tidak terbaca ditampilkan sebagai tanda hubung; bila termasuk data wajib, cetak ditahan beserta daftar bidangnya |
| Gagal | Kalimat merah beserta tombol **Coba Lagi** — skema tampilan 22.2 |
| Tanpa hak akses | Seluruh isi halaman diganti **Ups! Akses Ditolak**; formulir tidak pernah terender |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/episodes/{id}` | Nomor episode, unit layanan, kelas perawatan, DPJP, nama pasien, nomor rekam medis, dan identitas kunjungan | `InpatientEpisode : Read` |

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/registration-management/patient-encounters/admin/{id}` | Penjamin yang tercatat pada kunjungan — cara bayar, nama penjamin, nomor polis atau nomor karyawan | `PatientEncounter : Read` |

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patients/admin/{id}` | Tanggal lahir dan jenis kelamin. **Pelengkap**, bukan data wajib: kegagalannya dilaporkan tanpa menahan pencetakan | `Patient : Read` |

**Nol operasi tulis.** Tidak ada `POST`, `PUT`, `PATCH`, maupun `DELETE` di seluruh jalur ini,
dan tidak ada penyimpanan di peramban. Dibuktikan pada bagian 6.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | `0 errors`, 571 warning | `PASS` | Sama persis dengan garis dasar; nol pada berkas task ini |
| `npm run build` | `✓ Compiled successfully in 29.7s` | `PASS` | Route `/health-services/inpatient-management/episodes/[id]/consent-print` terdaftar sebagai `ƒ` pada keluaran build |
| `npm run test:unit` dan `node --test` | Tidak dijalankan | `NOT RUN` | Pengujian berbasis `.mjs` dilewati atas **instruksi eksplisit pengguna** pada sesi ini |
| **Verifikasi peramban Edge** | **37/37 PASS** | `PASS` | Rincian di bawah |

**Catatan waktu pengukuran.** Angka `npm run lint` dan `npm run build` di atas diukur ketika
seluruh perubahan task ini sudah selesai. Sesudah itu, working tree frontend menerima
**pekerjaan paralel yang bukan milik task ini** (implementasi pembatalan admisi). Lint ulang
yang discope hanya ke berkas milik task ini dijalankan sesudah pekerjaan paralel itu masuk dan
tetap `0 problems`. Angka lint dan build untuk keseluruhan repository perlu diukur ulang oleh
pemilik pekerjaan paralel tersebut, bukan diklaim di laporan ini.

### 6.1 Verifikasi manual di peramban

**Uji manual: `PASS`.**

| Skenario | Hasil |
| --- | --- |
| Formulir memuat nama, nomor rekam medis, tanggal lahir, penjamin, nomor polis, unit, kelas, DPJP, nomor episode, dan tanggal | `PASS` |
| Ketiga isi minimal `RWI-DEC-035` tercetak lengkap | `PASS` |
| Layar tidak memuat kata "sudah tersimpan" maupun "sudah ditandatangani", dan memuat kalimat "dicetak, tidak disimpan sistem" | `PASS` |
| Keterangan kebutuhan isolasi **tidak** ikut tercetak | `PASS` |
| Nol operasi tulis di seluruh halaman cetak | `PASS` |
| Nol salinan formulir disimpan di peramban | `PASS` |
| Tombol **Cetak** aktif ketika data wajib lengkap | `PASS` |
| Penjamin tidak terbaca → cetak dinonaktifkan, daftar bidang yang kurang tampil, **Coba Lagi** tersedia | `PASS` |
| Server menjawab 403 → **Ups! Akses Ditolak** menggantikan seluruh halaman dan formulir tidak terender | `PASS` |
| Formulir dapat dicapai dari alur admisi langkah 8 | `PASS` |
| Jalur pasien baru menawarkan **Lanjut ke Kartu Pasien**; jalur pasien lama menutup alur dengan **Selesai** | `PASS` |
| Melewati langkah tanpa mencetak tidak membatalkan apa pun | `PASS` |
| Saat media cetak diaktifkan, hanya formulir yang terlihat; menu samping `visibility: hidden` | `PASS` |
| Nol `pageerror` | `PASS` |

Metodenya sama dengan laporan `FE-RWI-027` bagian 6.1: Microsoft Edge lewat Playwright,
cookie sesi dicetak lokal, dan **seluruh** panggilan `/v1/**` dilayani stub sehingga nol
permintaan menyentuh backend bersama.

### 6.2 Checklist konsistensi UI

Keenam grep anti-regresi bersih pada `inpatient-consent-print.module.css` dan seluruh berkas
JSX baru:

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal pada stylesheet baru | kosong — seluruhnya `var(--color-*)` |
| `font-size` / `font-weight` / `line-height` di luar token | kosong |
| `<button>` mentah atau kelas `.btn` Bootstrap | kosong |
| `<table>` tanpa `data-flat-table` | kosong |
| Utility typography Bootstrap di dalam blok tabel | kosong |
| `!important` baru | kosong |
| Blok `prefers-color-scheme` | kosong |

**Tidak dijalankan:** seluruh pengujian berbasis `.mjs` — `npm run test:unit`, `node --test`,
dan `npm run test:e2e` — dilewati atas instruksi eksplisit pengguna pada sesi ini. Verifikasi
peramban pada 6.1 adalah uji manual berbantuan peramban; skripnya tidak ditambahkan ke
repository dan sudah dihapus setelah verifikasi selesai.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Formulir memuat identitas pasien, penjamin, unit layanan, kelas, DPJP, nomor episode, dan tanggal | Terpenuhi | `inpatient-consent-form.jsx` bagian identitas; verifikasi peramban baris "028-AC1" |
| 2. Ketiga isi minimal `RWI-DEC-035` tercetak | Terpenuhi | `INPATIENT_CONSENT_ITEMS`; verifikasi peramban baris "028-AC2" |
| 3. Layar **tidak** menyatakan persetujuan tersimpan atau tertanda tangan | Terpenuhi | `INPATIENT_CONSENT_COPY.noStorageNotice`; verifikasi peramban baris "028-AC3", ditambah bukti nol operasi tulis dan nol salinan di peramban |
| 4. Halaman cetak tidak dapat dibuka tanpa hak akses | Terpenuhi | `AccessDeniedGate` membungkus seluruh isi halaman; verifikasi peramban baris "028-AC4" dengan server menjawab 403 |
| 5. Dapat dicapai dari alur admisi **dan** dari detail episode | Terpenuhi | Langkah 8 pada `inpatient-admission-print-steps.jsx` dan tombol **Cetak Persetujuan** pada `inpatient-episode-detail-view.jsx`; verifikasi peramban baris "028-AC5" |

**Definition of Done — "kelima kriteria lulus; laporan menegaskan nol penyimpanan":**

- Kelima kriteria **lulus**.
- **Nol penyimpanan ditegaskan dan dibuktikan:** tidak ada satu pun `POST`, `PUT`, `PATCH`,
  atau `DELETE` di sepanjang jalur cetak; tidak ada `localStorage` maupun `sessionStorage`
  yang menyimpan isi formulir; dan ruang penerima informasi dirender sebagai garis kosong,
  bukan kolom yang dapat diketik — kolom yang dapat diketik akan membuat petugas mengira
  isinya ikut tersimpan.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tanggal yang tercetak adalah **tanggal formulir dibuat dan ditandatangani**, yaitu hari ini — bukan tanggal admisi. Lembar persetujuan ditandatangani pada saat itu juga, dan tanggal pada lembar itu wajib menyebut kapan tanda tangannya diberikan |
| Masalah yang diketahui | `RWI-CAP-031` dan `DEC-INP-003` **tetap terbuka** — keputusan ini tidak menutup keduanya, sesuai 3A.8 |
| Masalah yang diketahui | Nomor kartu penjamin tampil utuh pada formulir. Ini disengaja: bagian 6 blueprint melarangnya pada daftar dan secara tegas mengizinkannya pada formulir cetak |
| Masalah yang diketahui | Tombol lama di sekitar **Cetak Persetujuan** pada Detail Episode masih memakai kelas Bootstrap `btn btn-outline-primary btn-sm`. Warisan itu tidak disentuh karena perbaikannya milik task repair tersendiri; tombol baru memakai `BaseButton` |
| Dependency backend | `NONE`. Ketiga endpoint sudah ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas yang **diubah task ini**: `inpatient-admission-view.jsx`, `inpatient-episode-detail-view.jsx`, `inpatient-admission-flow-constants.jsx`, `inpatient-episode-constants.jsx`, `inpatient-admission-encounter.service.js`, `inpatient-admission.module.css`, `inpatient-admission-encounter-utils.jsx`, `inpatient-episode-utils.jsx`, ditambah sembilan berkas baru dan satu route baru. Working tree juga memuat **pekerjaan paralel yang bukan milik task ini** — implementasi pembatalan admisi pada `inpatient-episode-worklist-view.jsx`, `use-inpatient-episode-worklist.jsx`, `use-inpatient-episode-detail.jsx`, serta bagian pembatalan pada `inpatient-episode-detail-view.jsx`, `inpatient-episode-utils.jsx`, dan `inpatient-episode-constants.jsx`. Pekerjaan itu tidak disentuh, tidak dibatalkan, dan tidak diklaim di sini. Tidak ada stage, commit, maupun push |
| Langkah berikutnya | `FE-RWI-033` dapat menautkan halaman ini ke navigasi bila memang dikehendaki; saat ini ia hanya dicapai dari alur admisi dan Detail Episode sesuai skema tampilan 22.3 |
