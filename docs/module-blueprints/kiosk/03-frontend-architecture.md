# Kiosk — Arsitektur Frontend

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` r1 |
| Status | `approved` — Sukma Giri Pratama, 30 Sep 2026 |
| Owner | Sukma Giri Pratama |
| `input_revision` | `00-interview-decisions.md` r2; `contracts/*` `KSK-CONTRACT-v1` |
| SHA acuan | FE `4ec51b0b` |
| Stack | Next.js App Router, JavaScript/JSX, Redux Toolkit, `InstanceAxios` existing |

Hierarki kewenangan: keamanan/privasi/invariant → PRD (brief UI yang disetujui) → konvensi project → `DEV_DISCRETION` (`00-interview-decisions.md` §7).

## 1. Kebutuhan layar

| ID | Layar | Status | Route | Requirement |
| --- | --- | --- | --- | --- |
| `FE-KSK-01` | Beranda Kiosk | Diperbarui — tile Cek No. RM diberi route | `/kiosk` | KSK-RM-001 |
| `FE-KSK-02` | Cek Nomor Rekam Medis | **Baru** | `/kiosk/registration/medical-record-check` (`DEV_DISCRETION` dalam `/kiosk/registration/**`, `KSK-UI-002`) | KSK-RM-001..009, KSK-UX-001..003 |
| `FE-KSK-03` | Pendaftaran Pasien Lama (8 step) | Diperbarui | `/kiosk/registration/old-patient` | KSK-OLD-001..010, KSK-GUA-001/002 |
| `FE-KSK-04` | Cek Jadwal Dokter | Diperbarui — CSS dropdown | `/kiosk/registration/doctor-schedule` | KSK-SCH-001 |
| — | Pendaftaran Pasien Baru | Sudah ada — hanya tujuan CTA | `/kiosk/registration/new-patient` | KSK-RM-008 |

## 2. Peta butir menu

Kiosk **tidak memakai sidebar** (`src/utils/menu-sidebar/menu-items.jsx`). Jalan masuk seluruh layar Kiosk adalah **tile** pada `SERVICE_ITEMS` di `src/components/view/kiosk/kiosk-home-view.jsx`.

```text
Beranda Kiosk (/kiosk)                                    <- FE-KSK-01
├── Pendaftaran Pasien Baru        -> /kiosk/registration/new-patient          (sudah ada)
├── Pendaftaran Pasien Lama        -> /kiosk/registration/old-patient          FE-KSK-03
├── Check-In Janji Temu            -> (notice, di luar scope)
├── Cek Jadwal Dokter              -> /kiosk/registration/doctor-schedule      FE-KSK-04
├── Cetak Kartu Pasien             -> /kiosk/registration/patient-card/print   (sudah ada)
└── Cek Nomor Rekam Medis          -> /kiosk/registration/medical-record-check FE-KSK-02 (Baru: notice diganti route)
```

| Butir (tile) | Tingkat | Induk | `route` | Layar | Penjaga | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Cek Nomor Rekam Medis | 1 | Beranda Kiosk | `/kiosk/registration/medical-record-check` | `FE-KSK-02` | Login perangkat Kiosk (`isKioskUser`) + policy `KioskRead` di backend | Diperbarui |
| Pendaftaran Pasien Lama | 1 | Beranda Kiosk | `/kiosk/registration/old-patient` | `FE-KSK-03` | sama | Sudah ada |
| Cek Jadwal Dokter | 1 | Beranda Kiosk | `/kiosk/registration/doctor-schedule` | `FE-KSK-04` | sama | Sudah ada |

`FE-KSK-03` juga dimasuki dari `FE-KSK-02` (tombol "Lanjut Pendaftaran Pasien Lama"). Pendaftaran Pasien Baru juga dimasuki dari `FE-KSK-02` ("Daftar Sebagai Pasien Baru").

## 3. Skema fitur per layar

### `FE-KSK-02` — Cek Nomor Rekam Medis

```text
+- Cek Nomor Rekam Medis ------------------------------------ FE-KSK-02 -+
| [< Beranda]                                   perangkat · jam         |
+-----------------------------------------------------------------------+
| Cari menggunakan:   (o) No. KTP    ( ) No. HP                          |
| [ 327xxxxxxxxxxxxx                     ]   <- satu isian sesuai pilihan|
| pesan validasi di bawah isian                                          |
|                         [ Cek Nomor Rekam Medis ]                     |
+-----------------------------------------------------------------------+
| HASIL (salah satu):                                                    |
| memeriksa  -> tombol "Memeriksa Data...", isian terkunci               |
| ditemukan  -> "Pasien Ditemukan" + [Kartu Pasien]                      |
|               [ Lanjut Pendaftaran Pasien Lama ]                       |
| belum ada  -> "Pasien Belum Terdaftar" + subteks                       |
|               [ Daftar Sebagai Pasien Baru ]                           |
| ganda (HP) -> "Data Perlu Diverifikasi" + [ Cari dengan No. KTP ]      |
| petugas    -> "Data Perlu Diverifikasi" + [ Kembali ke Beranda ]      |
| gagal      -> "Data Belum Dapat Diperiksa" + [ Coba Lagi ]             |
+-----------------------------------------------------------------------+
| peringatan timeout (15 detik terakhir): "Sesi akan berakhir" [Lanjutkan]|
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Penjaga | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Pilihan metode | KTP / HP | — | — | Default KTP |
| Isian | Satu isian; KTP `inputMode="numeric"` maks. 16 digit | — | — | Pesan `KSK-VAL-002..006` |
| Tombol cek | Mengirim lookup | `POST kiosk-patient-lookups` `searchType` 1/2 | `KioskRead` | Terkunci selama memeriksa (`KSK-VAL-008`) |
| Kartu Pasien | `BasePatientCard` existing | `data.patient` | — | Hanya tampil saat `result = 1` |
| Hasil lain | Judul + subteks + tombol | `data.result` | — | Teks persis `validation-matrix.md` §2 |
| Gagal | Pesan + Coba Lagi | status `429`/`5xx`/timeout | — | **Tidak pernah** menampilkan tombol Pasien Baru |

### `FE-KSK-03` — Pendaftaran Pasien Lama

Bar langkah (poliklinik): `Identifikasi → Review Data → Tujuan Layanan → Jenis Kunjungan → Pembayaran → Layanan & Dokter → Konfirmasi → Cetak Antrean`.
Bar langkah (Laboratorium, setelah dipilih di Step 3): `Identifikasi → Review Data → Tujuan Layanan → Selesai`.

| Step | Wilayah | Isi | Sumber data | Perubahan dari hari ini |
| --- | --- | --- | --- | --- |
| 1 Identifikasi | Kolom cari + pemindai | Klasifikasi input (`validation-matrix.md` §4) | `POST kiosk-patient-lookups` (KTP/HP/kartu), `GET patients/kiosk?search=` (nama/RM), `GET patients/kiosk/{id}` (QR) | **Tidak lagi membentuk sesi kiosk.** KTP/HP tidak lagi lewat URL. Bila datang dari `FE-KSK-02`, pasien sudah terpilih dan pasien cukup menekan Lanjut. |
| 2 Review Data | Data pasien | Nama lengkap (tidak dipotong), tanggal lahir **bulan pendek** (`12 Sep 1990`) | `GET patients/kiosk/{id}` | Format tanggal (`KSK-DEC-015`) |
| 3 Tujuan Layanan | Poliklinik / Laboratorium (+ surat dokter) | Pilihan | `POST kiosk-scan-sessions/kiosk/scan-result` saat memilih | **Pindah** dari step pertama; sesi dibentuk di sini (`KSK-DEC-014`) |
| 4 Jenis Kunjungan | Umum / Rujukan | — | — | Tidak berubah |
| 5 Pembayaran | Tunai / Asuransi / Perusahaan; **Pilih Penjamin Utama** bila Kondisi C | Kartu penjamin milik pasien | `GET patient-insurances/kiosk/options`, `GET patient-company-guarantors/kiosk/options` | Tombol "Jadikan Utama" **dihapus**; Kondisi C wajib memilih satu; pilihan Perusahaan menghasilkan `paymentType = 3` |
| 6 Layanan & Dokter | Poli, dokter, jadwal | Existing | Existing | Tidak berubah |
| 7 Konfirmasi | Ringkasan: pasien, tujuan layanan, jenis kunjungan, pembayaran/penjamin, layanan, dokter, jadwal; tanggal lahir bulan pendek | — | `POST patient-encounters/kiosk` | Tambah baris Tujuan Layanan; penjamin perusahaan tampil |
| 8 Cetak Antrean | Tiket | Hasil create encounter | — | Tidak berubah |

Skema Pilih Penjamin Utama (Kondisi C, di dalam Step 5):

```text
+- Pilih Penjamin Utama ---------------------------------------------+
| ( ) Asuransi    Allianz Indonesia        No. kartu ****1234        |
| ( ) Perusahaan  PT ABC Indonesia         No. pegawai ****88        |
| tidak ada yang terpilih otomatis                                   |
|                                              [ Lanjutkan ]         |
+-------------------------------------------------------------------+
```

### `FE-KSK-04` — Cek Jadwal Dokter

Tidak ada perubahan isi. Perbaikan hanya CSS lokal `style/kiosk/registration/doctor-schedule/kiosk-doctor-schedule.module.css`: panel filter diberi stacking context di atas grid card (misalnya `position: relative; z-index` lebih tinggi dari card, dan tidak ada `overflow: hidden` yang memotong menu). **Komponen dasar `filter-select.jsx` tidak diubah** (dipakai 33 berkas, `KSK-CAP-040`). Tekniknya `DEV_DISCRETION` (`KSK-UI-005`).

## 4. Rencana file

| File | Status | Isi perubahan |
| --- | --- | --- |
| `src/app/kiosk/registration/medical-record-check/page.jsx` | Baru | Page tipis, merender client view |
| `src/components/view/kiosk/registration/medical-record-check/kiosk-medical-record-check-view.jsx` | Baru | Layar `FE-KSK-02` |
| `src/lib/hooks/kiosk/registration/medical-record-check/use-kiosk-medical-record-check.jsx` | Baru | Keadaan `IDLE…ERROR`, abort request lama, handoff |
| `src/lib/services/kiosk/registration/kiosk-patient-lookup.service.js` | Baru | `lookupKioskPatient({ searchType, value, signal })` memakai `InstanceAxios` existing, timeout 20 detik |
| `src/lib/helpers/kiosk/registration/kiosk-patient-lookup.helpers.js` | Baru | Normalisasi & validasi KTP/HP (cermin `validation-matrix.md`), klasifikasi input Step 1 (`KSK-DSN-009`) — fungsi murni |
| `src/lib/constants/kiosk/registration/kiosk-medical-record-check.constants.js` | Baru | Teks judul/subteks/tombol (`validation-matrix.md` §2), enum angka `searchType`/`result` |
| `src/lib/state/slice/health-services/registration-management/kiosk-patient-handoff-slice.jsx` + daftar di `src/lib/state/store.jsx` | Baru / Diperbarui | Menyimpan **hanya** `patientId` di memori; aksi `setHandoffPatient`, `consumeHandoffPatient`, `clearHandoff`. Store tidak memakai persistence. |
| `src/lib/hooks/kiosk/use-kiosk-inactivity-timeout.jsx` | Baru | 120 detik + peringatan 15 detik; jeda selama ada request berjalan; konstanta bawaan dapat ditimpa env `NEXT_PUBLIC_KIOSK_IDLE_SECONDS` (`KSK-GAP-009`) |
| `src/components/view/kiosk/kiosk-home-view.jsx` | Diperbarui | Tile `medicalRecordCheck`: hapus `notice`, tambah `route` |
| `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx` | Diperbarui | `OLD_PATIENT_STEP_ITEMS` urutan baru; step awal `find`; `handleReviewContinue` → `serviceTarget`; `handleSelectServiceTarget` membentuk sesi; `handleResetAll` → `find`; baca handoff Redux; perbaiki normalisasi `company → insurance` pada `handlePaymentContinue`; pasang inactivity timeout |
| `src/lib/hooks/kiosk/registration/kiosk-service-target-rules.js` | Diperbarui | `buildIdentifiedPatientScanSessionPayload` menerima hasil pindai tersimpan; tetap objek kosong untuk poliklinik (`KSK-DSN-007`) |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-find.jsx` | Diperbarui | Hapus `createOldPatientScanSession`; klasifikasi input; simpan hasil pindai di memori; dukung pasien terisi dari handoff |
| `.../kiosk-old-patient-step-service-target.jsx` | Diperbarui | Tombol "ganti layanan" ke Identifikasi dihapus (urutannya kini sesudah Review); state memuat saat sesi dibentuk |
| `.../kiosk-old-patient-step-payment.jsx` | Diperbarui | Hapus "Jadikan Utama"; tambah Pilih Penjamin Utama Kondisi C; Perusahaan menjadi pilihan sah |
| `.../kiosk-old-patient-step-review.jsx`, `.../kiosk-old-patient-step-confirm.jsx` | Diperbarui | `formatShortDateId`; Konfirmasi menampilkan tujuan layanan dan penjamin |
| `.../kiosk-old-patient-view.jsx` | Diperbarui | Header per step mengikuti urutan baru |
| `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js` | Diperbarui | Hapus `console.log` URL/diagnostik/body; `createOldPatientEncounter` mendukung `paymentKey = company` → `paymentType = 3` + `patientCompanyGuarantorId` |
| `src/style/kiosk/registration/doctor-schedule/kiosk-doctor-schedule.module.css` | Diperbarui | Stacking context panel filter |
| `src/style/kiosk/registration/kiosk-medical-record-check.module.css` | Baru | Gaya `FE-KSK-02`, memakai token/base style kiosk existing |

## 5. Penanganan keadaan

| Keadaan | Cek No. RM | Pasien Lama |
| --- | --- | --- |
| Memuat | Tombol "Memeriksa Data..." + isian terkunci | Tombol aksi step terkunci; label proses |
| Kosong | `NOT_FOUND` = hasil bisnis, bukan layar kosong | Step 5 tanpa asuransi/perusahaan → hanya Tunai |
| Gagal | `ERROR` + Coba Lagi; tidak pernah ke Pasien Baru | Pesan di step itu; tidak maju |
| Data basi | Mengubah isian atau metode membuang hasil sebelumnya dan membatalkan request berjalan | Detail pasien dimuat ulang dari backend saat handoff |
| Kirim ganda | Satu request aktif (`AbortController`) | Tombol Tujuan Layanan dan Daftar terkunci selama request |
| Timeout 120 detik | `SESSION_CLEARED` → Beranda | `SESSION_CLEARED` → Beranda; ditunda selama request berjalan |

## 6. Aksi per peran

Satu peran saja: pasien memakai akun perangkat Kiosk. Seluruh tombol pada skema di atas terbuka bagi peran itu; tidak ada tombol yang disembunyikan per peran (`contracts/permission-audit-matrix.md` §2).

## 7. Kewenangan UI

| Area | Mengikat | `DEV_DISCRETION` |
| --- | --- | --- |
| Teks judul, subteks, tombol hasil lookup | PRD + `validation-matrix.md` §2 (`KSK-UI-001`) | — |
| Dua metode + satu isian | PRD §9 (`KSK-UI-003`) | Bentuk kontrol (radio, segmented) |
| Route `FE-KSK-02` | Di bawah `/kiosk/registration/**`, tanpa KTP/HP/`patientId` di query (`KSK-UI-002`) | Nama segmen |
| Pilih Penjamin Utama | Pilihan tunggal, tanpa preselect (`KSK-UI-004`, `KSK-GAP-010`) | Gaya kartu |
| Dropdown jadwal dokter | AC-SCH-001/002, tanpa mengubah komponen dasar | Teknik CSS |
| Peringatan timeout | 15 detik + tombol Lanjutkan (`KSK-UI-006`) | Modal atau banner |
| Warna, ikon, jarak | — | Mengikuti token kiosk existing |

## 8. Privasi di frontend

| Aturan | Penerapan |
| --- | --- |
| Tanpa KTP/HP di URL | Lookup memakai POST body; handoff lewat Redux; tidak ada `?patientId=` untuk handoff Cek No. RM |
| Tanpa browser storage | Tidak ada `localStorage`/`sessionStorage` untuk data pasien |
| Tanpa console | `console.log` data pasien dihapus di service Kiosk |
| Pembersihan | `SESSION_CLEARED` membuang pasien, hasil pindai, draf, dan handoff |


---

# Amendment PO — Frontend Pendaftaran Pasien Online dan Check-in Kiosk

| Field | Nilai |
| --- | --- |
| Revisi blueprint | `2` — `approved` — Sukma Giri Pratama, 19 Nov 2026 (`KSK-DEC-054`) |
| Kontrak | `KSK-CONTRACT-v2` (`approved`) |
| SHA acuan | FE `5f18e9f04` |
| Wewenang UI | `KSK-PO-FE-001` s.d. `005` (§13.8 decision log) |

## PO-FE.1 Kebutuhan layar

| ID | Layar | Status | Route | Requirement |
| --- | --- | --- | --- | --- |
| `FE-KSK-PO-01` | Pendaftaran Pasien Online (publik, tanpa login) | **Baru** | `/online-registration` (`DEV_DISCRETION`, `KSK-PO-FE-004`) | AC 1, 2, 4, 5, 6, 14, 15, 16 |
| `FE-KSK-PO-02` | Check-in Pasien Online (kiosk) beserta Lupa Kode | **Baru** | `/kiosk/registration/online-check-in` (`DEV_DISCRETION` di bawah `/kiosk/registration/**`) | AC 7, 8, 9, 10, 13, 17 |
| `FE-KSK-PO-03` | Daftar Pendaftaran Online (petugas) | **Baru** | `/health-services/registration-management/online-registrations` | AC 18 |
| `FE-KSK-PO-04` | Detail Pendaftaran Online (petugas) beserta Ubah Jadwal, Ubah Kontak, Batal | **Baru** | `/health-services/registration-management/online-registrations/[id]` | AC 18, 19, 20 |
| `FE-KSK-01` | Beranda Kiosk | Diperbarui — kartu "Check-in Pasien Online" diberi route | `/kiosk` | `KSK-DEC-036` |

## PO-FE.2 Peta butir menu

```text
Sidebar petugas
└── Registration Management (grup yang sudah ada)
    ├── Pendaftaran Pasien                -> .../outpatient-registration      (sudah ada)
    ├── Reservasi Prioritas               -> .../priority-reservations        (sudah ada)
    └── Pendaftaran Online                -> .../online-registrations         FE-KSK-PO-03 (Baru)
            └── (layar anak) Detail       -> .../online-registrations/[id]    FE-KSK-PO-04

Beranda Kiosk (/kiosk)                                                         FE-KSK-01
└── Check-in Pasien Online (kartu)        -> /kiosk/registration/online-check-in   FE-KSK-PO-02 (notice diganti route)

Tanpa menu (publik)
└── /online-registration                                                       FE-KSK-PO-01
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Pendaftaran Online | 2 | Grup Registration Management yang memuat "Reservasi Prioritas" | `/health-services/registration-management/online-registrations` | `FE-KSK-PO-03` | `OnlineRegistration : Read` (`requiredPermission`) | Baru |
| Check-in Pasien Online (kartu kiosk) | 1 | Beranda Kiosk | `/kiosk/registration/online-check-in` | `FE-KSK-PO-02` | Login perangkat kiosk + `kioskCanAppointment` (kartu `enabledType: "appointment"`) + `KioskRead` di backend | Diperbarui |

Layar tanpa butir menu:

| Layar | Jenis | Jalan masuk |
| --- | --- | --- |
| `FE-KSK-PO-04` | Layar anak | Tombol baris di `FE-KSK-PO-03` |
| `FE-KSK-PO-01` | Halaman publik | Tautan atau kode QR yang dibagikan rumah sakit (situs web, media sosial, poster). Route didaftarkan sebagai jalur publik di `src/proxy.js` (`PUBLIC_PATH_PREFIXES`), `src/components/providers/root-layout-shell.jsx`, dan `src/components/ui/auth-wrapper.jsx` (`CAP-PO-21`) |

Nama butir, ikon, dan urutan di dalam grup tetap `DEV_DISCRETION`. Letak butir di grup yang sama dengan "Reservasi Prioritas" sudah dikunci (`KSK-PO-FE-005`, `KSK-DEC-054`).

## PO-FE.3 Skema fitur per layar

### `FE-KSK-PO-01` Pendaftaran Pasien Online (publik)

Langkah sama dengan menu Pendaftaran Pasien RJ (`KSK-DEC-029`). Bentuk penunjuk langkah (stepper) `DEV_DISCRETION`.

```text
+- Pendaftaran Pasien Online ------------------------------- FE-KSK-PO-01 -+
| Logo RS                                               [langkah 1..7]    |
+-------------------------------------------------------------------------+
| 1 Data Pasien   ( ) Pasien lama  ( ) Pasien baru                        |
|   lama: [NIK / No. RM v] [nomor] [tgl lahir]      -> "Si** Ra****"      |
|   baru: [NIK] [nama lengkap] [tgl lahir] [jenis kelamin]                |
|   [No. WhatsApp atau email]                                             |
| 2 Jenis Pasien  ( ) Pribadi ( ) Asuransi ( ) Rujukan                    |
| 3 Data Kunjungan [tanggal H+1..H+30] [poli v] [kartu jadwal + sisa kuota]|
| 4 Data Rujukan (Rujukan) [faskes] [tgl surat] [no.] [dokter] [berkas x3] |
| 5 Penjamin      [kartu milik pasien, tersamar] / [kartu baru] / Tunai   |
| 6 General Consent (pasien baru) [klausul] [penanda tangan] [tanda tangan]|
| 7 Verifikasi    ringkasan + [captcha] [Kirim]                           |
+-------------------------------------------------------------------------+
| Selesai: No. PO-261125-0007   KODE  K7QM-4TRZ                           |
|   Rabu 25 Nov 2026, Poli Anak, dr. Sari, 08.00–12.00                    |
|   "Kode sudah dikirim ke email Anda" / "Email gagal terkirim"           |
|   [Kirim kode ke WhatsApp saya]  [Salin kode]                           |
+-------------------------------------------------------------------------+
```

Urutan langkah 3–5 mengikuti jenis pasien persis seperti menu RJ (`F-PO-2`): Asuransi → Penjamin → Data Kunjungan; Pribadi → Data Kunjungan; Rujukan → Data Kunjungan → Data Rujukan → Penjamin.

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Data Pasien (lama) | Hasil cocok: nama tersamar | `POST /public/patient-match` | Tanpa login + captcha | Tidak cocok → "Data tidak ditemukan. Periksa kembali isian Anda atau daftar di loket pendaftaran." `429` → "Terlalu banyak percobaan. Silakan coba lagi nanti." |
| Data Kunjungan — poli | Daftar poli | `GET /public/options/clinics` | Tanpa login | Kosong → "Belum ada jadwal online pada tanggal ini. Pilih tanggal lain." Gagal → "Data gagal dimuat." [Coba lagi] |
| Data Kunjungan — jadwal | Kartu jadwal: dokter, jam, sesi, sisa kuota, penanda penuh | `GET /public/options/doctor-schedules` | Tanpa login | Kosong → "Tidak ada jadwal dokter pada poli dan tanggal ini." Jadwal penuh tampil tetapi tidak dapat dipilih |
| Data Rujukan | Pencarian faskes, isian surat, unggah berkas | `GET /public/options/referral-institutions` | Tanpa login | Pencarian kosong → "Faskes tidak ditemukan." |
| Penjamin | Kartu milik pasien (dari hasil cocok), kartu baru, Tunai | `patient-match` (`payers[]`), `GET /public/options/insurance-providers` | Tanpa login | Tanpa kartu → hanya kartu baru dan Tunai |
| General Consent | Klausul dan tanda tangan | Komponen consent RJ (statis) | — | — |
| Verifikasi + Kirim | Ringkasan, widget captcha, tombol Kirim | `POST /public` | Tanpa login + captcha | Widget gagal dimuat → tombol Kirim nonaktif + "Verifikasi keamanan belum siap, muat ulang halaman." Penolakan → pesan dari validation matrix, data isian tetap |
| Selesai | Nomor, kode, ringkasan, status email, tombol WhatsApp | Respons `POST /public` | — | — |

### `FE-KSK-PO-02` Check-in Pasien Online (kiosk)

```text
+- Check-in Pasien Online ---------------------------------- FE-KSK-PO-02 -+
| [Saya punya kode]                     [Lupa Kode]                        |
|  kode [K7QM-4TRZ] + tgl lahir          [NIK / No. RM v][nomor] + tgl lahir|
|                     [Periksa]                                            |
+--------------------------------------------------------------------------+
| Ringkasan: Si** Ra****, Rabu 25 Nov, Poli Anak, dr. Sari, 08.00–12.00    |
|            Penjamin: Tunai / Asuransi •••• 1234    Rujukan: Puskesmas X  |
| (pasien baru) Pindai KTP/KIA -> NIK terkunci -> cocok / tidak cocok      |
|                     [Check-in Sekarang]                                  |
+--------------------------------------------------------------------------+
| Tiket: A-07 — Poli Anak — dr. Sari   [Cetak ulang]   kembali ke beranda  |
+--------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Pilihan cara | Kode atau Lupa Kode | — | Akun kiosk | — |
| Verifikasi | Isian + tombol Periksa | `POST /online-registrations/kiosk/check-in/verify` | `KioskRead` | `404` → "Data tidak sesuai atau pendaftaran sudah tidak berlaku. Periksa kembali atau hubungi petugas." `409` → "Check-in hanya dapat dilakukan pada {tanggal}." |
| Ringkasan | Data paket check-in | Respons verifikasi | `KioskRead` | — |
| Pindai kartu (pasien baru) | Langkah pindai kiosk Pasien Baru dengan NIK terkunci, pilihan kartu hanya KTP dan KIA | Agen Plustek lokal; `findExistingKioskPatientByIdentity`; `POST patients/kiosk` dan dokumen identitas bila belum ada | `KioskRead` | NIK tidak terbaca → "NIK tidak terbaca. Pindai ulang kartu, atau ke loket pendaftaran." Beda → "Data kartu identitas tidak sesuai dengan pendaftaran online. Silakan ke loket pendaftaran." |
| Check-in | Tombol Check-in Sekarang | `POST patient-encounters/kiosk` dengan `onlineRegistrationId` + token; kartu asuransi baru lebih dulu lewat `POST patient-insurances/kiosk` | `KioskRead` | `409` jadwal → "Jadwal dokter berubah, silakan ke loket pendaftaran." Token habis → "Waktu check-in habis. Silakan ulangi dari awal." |
| Tiket | Nomor antrean, poli, dokter | Respons kunjungan; cetak lewat `thermal-ticket-print.utils.js` (`CAP-PO-25`) | — | Printer gagal → tiket tampil di layar + "Ambil tiket di menu Cetak Antrean" |

Batas waktu tanpa sentuhan memakai `use-kiosk-inactivity-timeout` yang ada. Sesi dibersihkan setelah tiket atau setelah batas waktu.

### `FE-KSK-PO-03` Daftar Pendaftaran Online (petugas)

Pola layar sama dengan Daftar Reservasi Prioritas (`CAP-PO-35`).

```text
+- Pendaftaran Online --------------------------------------- FE-KSK-PO-03 -+
| [Menunggu 40] [Kunjungan 12] [Jalur Lain 3] [Tidak Datang 5] [Batal 6]  |
| [cari kode / nomor / nama / No. RM / NIK] [tgl dari–sampai] [Status v]   |
+--------------------------------------------------------------------------+
| No. | Tgl kunjungan | Pasien | No. RM/"Pasien baru" | Poli | Dokter |     |
| Jenis | Kontak | Status chip | [Detail]                                  |
+--------------------------------------------------------------------------+
| memuat -> kerangka baris                                                 |
| kosong -> "Belum ada pendaftaran online pada saringan ini." [Atur ulang] |
| gagal  -> "Data gagal dimuat." [Coba lagi]                               |
+- Halaman 1 dari n --------------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Jumlah per status | `GET /online-registrations/summary` | `OnlineRegistration : Read` | Gagal → angka "—" dan tombol coba lagi |
| Filter | Rentang tanggal kunjungan (bawaan hari ini s.d. 30 hari), status, pencarian | — | — | Pencarian < 3 karakter → "Kata kunci pencarian minimal 3 karakter." |
| Tabel | Kolom `OnlineRegistrationListItemResponse` | `GET /online-registrations` | `OnlineRegistration : Read` | Kosong/gagal seperti gambar |
| Akses | Seluruh layar | — | `OnlineRegistration : Read` | Tanpa hak → `AccessDeniedGate` |

### `FE-KSK-PO-04` Detail Pendaftaran Online (petugas)

```text
+- PO-261125-0007 — Pasien baru: Contoh Pasien ------------- FE-KSK-PO-04 -+
| Menunggu Check-in   Rabu 25 Nov 2026   Poli Anak   dr. Sari 08.00–12.00  |
| [Ubah Jadwal] [Ubah Kontak] [Batalkan]                                   |
+---------------+----------------------------------------------------------+
| Pasien        | NIK, nama, tgl lahir, jenis kelamin / No. RM             |
| Kunjungan     | jenis pasien, penjamin, kartu                            |
| Rujukan       | faskes, nomor, tanggal, dokter, berkas [Buka]            |
| Kontak        | No. WA / email, status email                             |
| Riwayat       | linimasa kejadian, nilai lama -> baru, petugas, waktu    |
+---------------+----------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala dan bagian isi | Detail | `GET /online-registrations/{id}` | `OnlineRegistration : Read` | Gagal → seluruh layar diganti pesan + [Coba lagi]; `404` → "Pendaftaran tidak ditemukan." |
| Tombol Ubah Jadwal | Pilih tanggal dan jadwal di poli yang sama, dengan sisa kuota | `GET /patient-encounters/doctor-schedule-quotas` (endpoint petugas yang ada) lalu `PUT /{id}/schedule` | `OnlineRegistration : Update` | Disembunyikan bila tanpa hak atau `canChangeSchedule = false`. `409` → pesan validation matrix, data dimuat ulang |
| Tombol Ubah Kontak | Isian No. WA/email | `PUT /{id}/contact` | `OnlineRegistration : Update` | Disembunyikan bila tanpa hak atau status akhir |
| Tombol Batalkan | Konfirmasi + alasan wajib | `POST /{id}/cancel` | `OnlineRegistration : Cancel` | Disembunyikan bila tanpa hak atau status akhir |
| Berkas rujukan | Buka berkas | `GET /{id}/referral-documents/{documentId}` | `OnlineRegistration : Read` | Gagal → "Berkas tidak dapat dibuka." |
| Riwayat | Linimasa | Bagian `histories[]` detail | `OnlineRegistration : Read` | Tidak pernah kosong (minimal `SUBMITTED`) |

Bentuk Ubah Jadwal, Ubah Kontak, dan Batalkan (modal, drawer, atau halaman) `DEV_DISCRETION`. Konfirmasi batal memakai `ConfirmModal` yang ada.

## PO-FE.4 Rencana file

| File | Status | Isi |
| --- | --- | --- |
| `src/app/online-registration/page.jsx`, `layout.jsx` | Baru | Halaman publik, tanpa navbar/sidebar (pola `src/app/priority-arrival/`) |
| `src/components/view/online-registration/**` | Baru | View publik dan langkah yang tidak ada padanannya (Data Pasien publik, Selesai publik) |
| `src/lib/hooks/online-registration/use-online-registration.js` | Baru | State langkah, validasi, kirim |
| `src/lib/services/online-registration/online-registration-public.service.js` | Baru | Panggilan endpoint publik (`InstanceAxios` tanpa token) |
| `src/lib/constants/online-registration/online-registration.constants.js` | Baru | Urutan langkah (diturunkan dari `outpatient-registration.constants.js`), pesan |
| Komponen langkah RJ murni (`outpatient-visit-type-step`, `outpatient-payer-step`, `outpatient-general-consent-step`, `outpatient-schedule-card`, `outpatient-practice-schedule-modal`) | Dipakai ulang | Data diberikan lewat props dari hook publik. `outpatient-visit-step`, `outpatient-referral-step`, `outpatient-verification-step` mendapat varian props sumber data, bukan salinan (`CAP-PO-22`) |
| `src/app/kiosk/registration/online-check-in/page.jsx` | Baru | Route kiosk |
| `src/components/view/kiosk/registration/online-check-in/**` | Baru | Pilihan cara, verifikasi, ringkasan, pindai, tiket |
| `src/lib/hooks/kiosk/registration/use-kiosk-online-check-in.js` | Baru | Alur check-in, pembersihan sesi |
| `src/lib/services/kiosk/registration/kiosk-online-check-in.service.js` | Baru | Verifikasi dan pembuatan kunjungan |
| `kiosk-new-patient-step-scan-identity.jsx` | Diperbarui | Prop opsional `lockedFieldNames` (bawaan kosong) dan `allowedIdentityTypes` (bawaan semua); kiosk Pasien Baru tidak berubah (`CAP-PO-37`) |
| `src/components/view/kiosk/kiosk-home-view.jsx` | Diperbarui | `onlineCheckIn` mendapat route |
| `src/app/health-services/registration-management/online-registrations/page.jsx`, `[id]/page.jsx` | Baru | Layar petugas |
| `src/components/view/health-services/registration-management/online-registrations/**` | Baru | Daftar, kolom tabel, detail, modal aksi |
| `src/lib/state/slice/health-services/registration-management/online-registration-slice.js` | Baru | Pola `priority-reservation-slice` |
| `src/lib/services/health-services/registration-management/online-registration.service.js` | Baru | Endpoint petugas |
| `src/lib/hooks/health-services/registration-management/online-registrations/*` | Baru | Daftar dan detail, `usePermission("OnlineRegistration", …)` |
| `src/lib/constants/health-services/registration-management/online-registration-constants.js` | Baru | Resource, aksi, status, route |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui | Butir "Pendaftaran Online" |
| `src/proxy.js`, `src/components/providers/root-layout-shell.jsx`, `src/components/ui/auth-wrapper.jsx` | Diperbarui | `/online-registration` sebagai jalur publik |
| `next.config.js` | Diperbarui | `https://challenges.cloudflare.com` pada `script-src` dan `frame-src` |
| `.env` contoh | Diperbarui | `NEXT_PUBLIC_TURNSTILE_SITE_KEY` (site key bukan rahasia) |

Penentuan `REUSE`/`EXTEND`/`COMPOSE`/`WRAP`/`NEW` per elemen dijalankan saat build lewat gerbang keputusan base component (`build-module-frontend` langkah 3).

## PO-FE.5 Penanganan keadaan

| Keadaan | Halaman publik | Kiosk | Layar petugas |
| --- | --- | --- | --- |
| Memuat | Kerangka pada daftar poli/jadwal; tombol terkunci saat kirim | Tombol "Memeriksa..." / "Memproses..." | Kerangka baris |
| Kosong | Pesan per wilayah (PO-FE.3) | — | "Belum ada pendaftaran online pada saringan ini." |
| Gagal | Pesan + [Coba lagi]; isian tidak hilang | Pesan + kembali ke langkah sebelumnya | Pesan + [Coba lagi] |
| Data basi | Kuota dibaca ulang saat Kirim; `409` kuota → kembali ke Data Kunjungan dengan pesan | Token 15 menit; habis → ulangi dari awal | `409` `RowVersion` → muat ulang detail |
| Kirim ganda | Tombol terkunci + kunci pasien di server | Tombol terkunci + kunci pasien di server | Tombol terkunci |

## PO-FE.6 Kewenangan UI

| Area | Mengikat | `DEV_DISCRETION` |
| --- | --- | --- |
| Urutan langkah halaman online | Sama dengan menu RJ (`KSK-PO-FE-001`) | Bentuk stepper |
| Data yang tampil di halaman publik | Nama dan nomor kartu tersamar saja (`KSK-PO-FE-002`) | — |
| Isi email dan WhatsApp | Kode, tanggal, poli, dokter; tanpa NIK/No. RM/penjamin (`KSK-PO-FE-003`) | Kalimat |
| Route publik, tata letak halaman online, layar check-in | Mengikuti konvensi frontend dan gaya kiosk (`KSK-PO-FE-004`) | Route, tata letak |
| Layar petugas | Isi dan aksi `KSK-DEC-050` s.d. `052` (`KSK-PO-FE-005`) | Tata letak, bentuk modal; letak menu dikunci saat approval |
| Warna, ikon, jarak, tipografi | Token CSS; tidak ada warna literal (memori `warna-css-wajib-token`) | Pilihan token |

## PO-FE.7 Privasi di frontend

| Aturan | Penerapan |
| --- | --- |
| Tanpa data pasien di URL | Seluruh identitas lewat body `POST` |
| Tanpa browser storage untuk data pasien | Draf halaman online hanya di memori; muat ulang halaman = mulai ulang |
| Kode hanya ditampilkan sekali | Tidak disimpan di storage; tombol Salin dan WhatsApp memakai nilai di memori |
| Tanpa console | Tidak ada `console.log` isian pasien |
| Kiosk | Sesi dibersihkan setelah tiket, Batal, atau batas waktu |
