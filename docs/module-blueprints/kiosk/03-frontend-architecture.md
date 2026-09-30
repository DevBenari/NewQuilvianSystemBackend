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
