# Laporan Perubahan Frontend — `FE-RWI-029`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-029` |
| Judul | Kartu pasien tercetak pada jalur pasien baru |
| Slice | Langkah **Kartu Pasien** — langkah terakhir jalur pasien baru pada alur admisi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 4, entri `FE-RWI-029` |
| Trace | `03-frontend-architecture.md` 3A.2 langkah 9 dan catatan 3A.3 *"Kartu Pasien tidak ada pada jalur pasien lama"*; `05-skema-tampilan.md` bagian 3.11; `IA-INP-01` |
| Contract version | `NOT APPLICABLE` — langkah ini tidak memanggil endpoint apa pun. Data pasien sudah dibaca langkah sebelumnya |
| Wewenang UI | `DEV_DISCRETION` penuh |
| Dependency | `FE-RWI-027` (**selesai 1 September 2026**) |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 0, kontrak API 0, database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `FRONTEND` — backend strict read-only, kecuali laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; `NewQuilvianSystemBackend` **hanya** untuk laporan ini beserta tautan buktinya |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `55f91b6cd`, branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `85439d32a884adcde3067304774151b317b058a2`, branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI.** Ketiga acceptance criteria terpenuhi dan dibuktikan runtime di peramban. `npm run lint` `0 errors`; `npm run build` `✓ Compiled successfully`; verifikasi peramban `37/37 PASS` |

---

## 1. Keadaan yang ditemukan di awal

Langkah **Kartu Pasien** sudah tercantum sebagai langkah ke-9 pada
`INPATIENT_ADMISSION_NEW_PATIENT_STEPS` sejak `FE-RWI-022`, dan memang **tidak** ada pada
`INPATIENT_ADMISSION_EXISTING_PATIENT_STEPS`. Kerangkanya sudah benar; yang belum ada adalah
isinya. Slug `patient-card` jatuh ke komponen `PendingStep` yang hanya menampilkan kalimat
*"Isi operasional langkah ini akan dilengkapi oleh task lanjutan"*.

Komponen kartunya sendiri sudah ada dan siap dipakai ulang:

| Yang sudah ada | Bukti |
| --- | --- |
| Kartu identitas pasien standar | `src/components/features/base-features/base-patient-card.jsx`, sudah dipakai kiosk pada `kiosk-patient-card-step-preview.jsx` |
| Ukuran cetak kartu 85,6 mm × 54 mm | `kiosk-patient-card-print.module.css` baris 2062, blok `@media print` yang menyasar kelas kartunya sendiri sehingga ikut terpakai di mana pun kartu itu dirender |

Risiko yang disebut roadmap — *"menyalin komponen cetak alih-alih memakainya ulang akan
melahirkan dua bentuk kartu"* — dihindari dengan memakai `BasePatientCard` apa adanya.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas admisi rawat inap.

**Kapan layar ini dibuka.** Langkah terakhir jalur **pasien baru**, sesudah langkah cetak
persetujuan. Pada jalur pasien lama langkah ini **tidak ada** — pasien lama sudah memiliki
kartunya, dan cetak ulang dilakukan lewat layar cetak kartu yang sudah ada.

**Langkah yang dilakukan berurutan:**

1. Petugas menekan **Lanjut ke Kartu Pasien** dari langkah cetak persetujuan.
2. Layar menampilkan tanda selesai berwarna hijau: *"Episode EP-2026-000204 menunggu
   konfirmasi kedatangan di Papan Tempat Tidur."*
3. Di bawahnya tampil pratinjau kartu pasien berisi nama, nomor rekam medis, kode pasien,
   jenis kelamin, dan kode QR — persis kartu yang selama ini dicetak kiosk.
4. Petugas menekan **Cetak Kartu**. Dialog cetak peramban terbuka dan hanya kartunya yang
   masuk ke kertas, pada ukuran 85,6 mm × 54 mm.
5. Petugas menyerahkan kartu kepada pasien, lalu memilih salah satu dari:
   - **Buka Papan Tempat Tidur** — menuju layar tempat kedatangan pasien nanti
     dikonfirmasi, memenuhi `IA-INP-01`;
   - **Admisi Baru** — mengosongkan alur dan kembali ke layar pembuka.

**Contoh konkret.** Ibu Sari Dewi baru pertama kali berobat. Sesudah admisinya dikunci dan
persetujuannya dicetak, petugas mencetak kartunya dan menyerahkannya. Ibu Sari pulang dari
meja admisi membawa kartunya, tanpa petugas berpindah ke aplikasi kiosk.

**Jalur tidak normal:**

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Data pasien belum terbaca di layar | Peringatan kuning; tombol **Cetak Kartu** dinonaktifkan, dan petugas diarahkan mencetak ulang dari layar cetak kartu pasien |
| Petugas melewati langkah ini tanpa mencetak | Kalimat biru menegaskan bahwa melewatinya **tidak** membatalkan admisi yang sudah terbentuk, dan kartu dapat dicetak ulang kapan saja |
| Nomor episode belum terbaca | Kalimat selesai memakai bentuk umum tanpa menyebut nomor episode, bukan menampilkan nomor kosong |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa diperiksa |
| --- | --- |
| `src/components/features/base-features/base-patient-card.jsx` | Bentuk `patient` yang diterima dan kelas cetak yang dibawanya |
| `src/components/view/kiosk/registration/patient-card/print/kiosk-patient-card-step-preview.jsx` | Cara kiosk memakai komponen yang sama |
| `src/style/kiosk/registration/patient-card/kiosk-patient-card-print.module.css` | Blok `@media print` yang mengunci ukuran kartu |
| `src/utils/kiosk/registration/patient-card/kiosk-patient-card-print-utils.jsx` | `buildPatientCardData` dan bidang yang dibacanya |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` | Sumber data pasien yang sudah tersedia di alur |
| `src/lib/constants/health-services/inpatient-management/inpatient-bed-board-constants.jsx` | Route Papan Tempat Tidur |
| `05-skema-tampilan.md` bagian 3.11 dan 3.12 | Kerangka layar dan pola kata tombol |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Ditambah `INPATIENT_PATIENT_CARD_MESSAGES` berisi tiga kalimat tetap langkah ini |
| `src/components/view/health-services/inpatient-management/inpatient-admission-print-steps.jsx` | **Baru.** Memuat `InpatientAdmissionPatientCardStep` (berbagi berkas dengan langkah 8 milik `FE-RWI-028`, karena keduanya adalah langkah cetak yang berdampingan) |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Langkah `patient-card` dipasangkan ke komponennya; **Admisi Baru** ditautkan ke `flow.resetEntryMode` |
| `src/style/health-services/inpatient-management/inpatient-consent-print.module.css` | Kelas `printStage` dan `printArea` dipakai bersama langkah cetak persetujuan supaya hanya kartunya yang masuk ke kertas |

### 3.3 Kepatuhan arsitektur frontend

Langkah ini murni presentasi. Tidak ada hook baru, tidak ada service baru, dan tidak ada
utility baru: data pasien sudah dipegang `useInpatientAdmissionPatient` di level view sejak
langkah Pendaftaran, dan nomor episode datang dari controller Konfirmasi.

`BasePatientCard` dipakai **apa adanya** lewat props yang sudah ada. Tidak ada satu baris pun
markup kartu yang disalin ke folder rawat inap.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` — langkah ini tidak memanggil API; data pasien sudah dibaca langkah sebelumnya |
| Kosong | Peringatan kuning *"Data pasien belum terbaca di layar ini, sehingga kartu belum dapat dicetak"* beserta jalan keluarnya |
| Gagal | `NOT APPLICABLE` — tidak ada permintaan yang dapat gagal di langkah ini |
| Tanpa hak akses | `NOT APPLICABLE` — tidak ada permintaan yang dapat ditolak. Hak akses sudah ditegakkan pada langkah-langkah sebelumnya yang memang menyentuh server |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — langkah ini tidak memanggil endpoint apa pun. Roadmap sudah menyatakannya:
scope-nya adalah langkah Kartu Pasien, dan komponennya dipakai ulang dari cetak kartu kiosk.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | `0 errors`, 571 warning | `PASS` | Sama persis dengan garis dasar; nol pada berkas task ini |
| `npm run build` | `✓ Compiled successfully in 29.7s` | `PASS` | Keluaran perintah |
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
| Kartu memuat nama dan nomor rekam medis pasien yang baru didaftarkan | `PASS` — terbaca `NAMA PASIEN Sari Dewi`, `NO. REKAM MEDIS 00123456`, `KODE PASIEN 00123456`, `JENIS KELAMIN Perempuan` |
| Nomor episode ikut disebut pada layar penutup | `PASS` — `EP-2026-000204` |
| Layar menyatakan bahwa melewati langkah ini tidak membatalkan admisi | `PASS` |
| Tersedia **Buka Papan Tempat Tidur** dan **Admisi Baru** | `PASS` |
| **Jalur pasien lama tidak memiliki langkah ini** | `PASS` — membuka `?entry=existing&step=patient-card` dialihkan ke langkah pertama jalur itu (`step=existing-patient`), dan teks *"Cetak Kartu Pasien"* tidak muncul di mana pun |
| Jalur pasien lama menutup alur dengan **Selesai** pada langkah cetak persetujuan | `PASS` |
| Nol `pageerror` | `PASS` |

Metodenya sama dengan laporan `FE-RWI-027` bagian 6.1: Microsoft Edge lewat Playwright,
cookie sesi dicetak lokal, dan seluruh panggilan `/v1/**` dilayani stub. Alurnya disusuri
betulan dari langkah Pembayaran sampai langkah Kartu Pasien.

### 6.2 Checklist konsistensi UI

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal pada stylesheet baru | kosong |
| Typography yang menimpa komponen shared | kosong |
| `<button>` mentah atau kelas `.btn` Bootstrap | kosong |
| `<table>` tanpa `data-flat-table` | kosong |
| Utility typography Bootstrap di dalam blok tabel | kosong |
| `!important` baru | kosong |

Perbandingan berdampingan dengan modul referensi: kartu yang dirender langkah ini identik
dengan yang dirender kiosk, karena keduanya memakai `BasePatientCard` dan stylesheet yang
sama. Tidak ada perbedaan visual yang tersisa.

**Tidak dijalankan:** seluruh pengujian berbasis `.mjs` — `npm run test:unit`, `node --test`,
dan `npm run test:e2e` — dilewati atas instruksi eksplisit pengguna pada sesi ini. Verifikasi
peramban pada 6.1 adalah uji manual berbantuan peramban; skripnya tidak ditambahkan ke
repository dan sudah dihapus setelah verifikasi selesai.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kartu tercetak berisi data pasien yang baru didaftarkan | Terpenuhi | `InpatientAdmissionPatientCardStep` merender `BasePatientCard` dengan `patient.selectedPatient`; verifikasi peramban baris "029-AC1" |
| 2. Langkah ini **tidak** ada pada jalur pasien lama | Terpenuhi | `INPATIENT_ADMISSION_EXISTING_PATIENT_STEPS` tidak memuat slug `patient-card`; verifikasi peramban baris "029-AC2" membuktikan alamat langkah itu dialihkan pada jalur pasien lama |
| 3. Melewatinya tidak membatalkan admisi yang sudah terbentuk | Terpenuhi | `INPATIENT_PATIENT_CARD_MESSAGES.skipSafe`; langkah ini tidak memanggil satu pun operasi tulis, sehingga tidak ada jalan baginya untuk membatalkan apa pun; verifikasi peramban baris "029-AC3" |

**Definition of Done — "ketiga kriteria lulus":** terpenuhi seluruhnya.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bidang **TIPE PASIEN** pada kartu terbaca tanda hubung, dan **GOLONGAN DARAH** terbaca `0`, karena `PatientDetailResponse` tidak selalu mengisi keduanya. Perilaku ini sama dengan kartu yang dicetak kiosk dan bukan akibat perubahan ini |
| Masalah yang diketahui | Kartu dicetak dari data pasien yang tersimpan di layar. Bila halaman dimuat ulang pada langkah ini, data pasien tetap terbaca karena `patientId` tersimpan pada URL — berbeda dari nomor episode yang hanya hidup selama sesi |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas yang **diubah task ini**: `inpatient-admission-view.jsx`, `inpatient-episode-detail-view.jsx`, `inpatient-admission-flow-constants.jsx`, `inpatient-episode-constants.jsx`, `inpatient-admission-encounter.service.js`, `inpatient-admission.module.css`, `inpatient-admission-encounter-utils.jsx`, `inpatient-episode-utils.jsx`, ditambah sembilan berkas baru dan satu route baru. Working tree juga memuat **pekerjaan paralel yang bukan milik task ini** — implementasi pembatalan admisi pada `inpatient-episode-worklist-view.jsx`, `use-inpatient-episode-worklist.jsx`, `use-inpatient-episode-detail.jsx`, serta bagian pembatalan pada `inpatient-episode-detail-view.jsx`, `inpatient-episode-utils.jsx`, dan `inpatient-episode-constants.jsx`. Pekerjaan itu tidak disentuh, tidak dibatalkan, dan tidak diklaim di sini. Tidak ada stage, commit, maupun push |
| Langkah berikutnya | Slice **F9 alur admisi** dan **F10 cetak** kini tertutup seluruhnya. Yang berikutnya adalah `FE-RWI-030` s.d. `FE-RWI-032` pada slice F11 |
