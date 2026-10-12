# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-019` |
| Judul | Step Data Rujukan petugas |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Trace | `RJ-DOC-DEC-071`..`075`; `03` *PM-FE.2*, *PM-FE.6*; FR-PM-08..10 |
| Contract version | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) |
| Dependency | `RJ-DOC-REV-BE-019` ✅, `RJ-DOC-REV-BE-021` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-DOC-DEC-083`) |
| Commit frontend | `de323430` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Keadaan awal

Jenis Kunjungan *Rujukan / Tindak Lanjut* hanya mengubah `visitType`. Poliklinik dipilih di Data Kunjungan, payload mengirim `isReferral: false`, dan tidak ada isian rujukan maupun unggah surat.

## 2. Proses dari sisi pengguna

1. Di **Data Kunjungan**, petugas memilih *Rujukan / Tindak Lanjut*. Bar step langsung menjadi 7 langkah dengan **Data Rujukan** di posisi 3. Pilihan Poliklinik dan Jadwal Dokter hilang, diganti keterangan bahwa unit tujuan dipilih di Data Rujukan. Tombol berubah menjadi *Lanjut ke Data Rujukan*.
2. Kembali ke *Umum* sesudah ada isian rujukan memunculkan konfirmasi **"Ganti ke kunjungan Umum?"**. Bila disetujui, isian rujukan dan surat dikosongkan.
3. **Data Rujukan**:
   - **A. Identitas Rujukan**
     - No. Rujukan.
     - Tanggal & jam rujukan, terisi waktu saat step dibuka dan tidak boleh di masa depan.
     - Fasilitas Perujuk, pilihan yang dapat dicari di server.
     - Dokter Perujuk (opsional), disaring menurut fasilitas.
     - Fasilitas bermitra memunculkan alert **"Fasilitas Perujuk Bermitra dengan Rumah Sakit"**.
   - **B. Tujuan & Alasan**
     - Unit Tujuan berisi seluruh poliklinik, Laboratorium, dan **Radiologi (belum tersedia)** yang nonaktif.
     - Bila tujuan poliklinik, Jadwal Dokter dipilih seperti `FE-017`. Poli tanpa jadwal pada tanggal kunjungan menampilkan alert dan tombol **Lihat Jadwal Praktik**, yang membuka popup jadwal seluruh hari (hari, dokter, jam, sesi, ruang).
     - Bila tujuan Laboratorium, petugas memilih Unit Laboratorium. Alert menjelaskan bahwa rujukan Lab hanya untuk hari ini dengan pembayaran tunai atau asuransi.
     - Diagnosa ICD-10 dicari di server (minimal 2 huruf).
     - Catatan diagnosa opsional.
     - Alasan rujukan wajib.
   - **C. Dokumen**: *Pilih Berkas* menerima 1–10 berkas PDF/JPG/PNG, masing-masing ≤ 5 MB. Daftar berkas menampilkan nama, ukuran, dan tombol *Hapus*.
4. Pembayaran, General Consent, dan Verifikasi memakai nomor langkah yang bergeser (4, 5, 6). Verifikasi menampilkan kartu **Data Rujukan**.
5. Submit:
   - **Poliklinik**: `POST /patient-encounters/admin` dengan blok `referral` (satu transaksi), lalu unggah surat.
   - **Laboratorium**: `POST /lab-patient-registrations/external-referral` dengan `idempotencyKey`, lalu `PUT …/referral`, lalu unggah surat.
6. Layar **Selesai** menampilkan status penyimpanan rujukan:
   - Sedang menyimpan.
   - **"Data rujukan tersimpan"**.
   - **"Rincian/surat rujukan belum tersimpan"** dengan tombol **Coba lagi**. Kunjungan tetap terdaftar dan ditandai *Rujukan belum lengkap*. Coba lagi hanya mengirim bagian yang belum tersimpan.

Pendaftaran *Umum* dan Pendaftaran IGD tidak berubah.

## 3. Perubahan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/…/outpatient-registration.constants.js` | Step `REFERRAL = 7`, `OUTPATIENT_REFERRAL_REGISTRATION_STEPS`, URL rujukan/berkas/registrasi Lab, enum unit tujuan, batas berkas, pesan `RJ-VAL-PM-03..08/12/13/15`, `REFERRAL_FOLLOW_UP_STATUS`, nilai default form rujukan |
| `src/utils/…/outpatient-registration/outpatient-referral.utils.js` | **Baru** — fungsi murni: urutan step, validasi rujukan dan berkas, pilihan unit tujuan, payload create/registrasi Lab/`PUT`, baris popup jadwal |
| `src/lib/services/…/outpatient-registration.service.js` | `registerOutpatientLabReferral`, `upsertOutpatientEncounterReferral`, `uploadOutpatientReferralDocuments` (multipart `files`) |
| `src/lib/state/slice/…/outpatient-registration-slice.jsx` | Thunk `submitOutpatientLabReferral`, `completeOutpatientReferralFollowUp`; state `referralFollowUp`. Berkas tidak masuk state |
| `src/lib/hooks/…/use-outpatient-visit-options.js` | Opsi `includeAllClinics`; `allClinics`, `allSchedules` |
| `src/lib/hooks/…/use-outpatient-registration.js` | Alur Rujukan, berkas lokal, ganti Jenis Kunjungan, submit poli/Lab, Coba lagi, kembali mengikuti bar step |
| `src/components/view/…/outpatient-referral-step.jsx` | **Baru** — step Data Rujukan |
| `src/components/view/…/outpatient-practice-schedule-modal.jsx` | **Baru** — popup jadwal praktik |
| `src/components/view/…/outpatient-visit-step.jsx` | Mode Rujukan, konfirmasi ganti ke Umum, `ScheduleSelect` diekspor + prop opsional `emptyAction` |
| `src/components/view/…/outpatient-verification-step.jsx`, `outpatient-general-consent-step.jsx` | Nomor langkah dinamis; ringkasan Data Rujukan dan urutan penyimpanan |
| `src/components/view/…/outpatient-registration-page.jsx` | Bar step dinamis, step Data Rujukan, notice penyimpanan rujukan, baris slip Lab |
| `src/components/view/…/emergency-registration/payment-method-step.jsx` | Prop opsional `stepNumber` (default 3, IGD tidak berubah) |
| `src/style/…/outpatient-registration.module.css` | Kelas `referral*` memakai token |

### 3.2 Keputusan base component

**UI GATE: 12 elemen — REUSE 9, EXTEND 1, COMPOSE 2, WRAP 0, NEW 0**

| Kebutuhan UI | Base component | Status |
| --- | --- | --- |
| Bar step dinamis | `EmergencyRegistrationStepper` (urutan berbasis index) | REUSE |
| Select dapat dicari server-side (fasilitas, dokter, unit Lab, diagnosa) | `EmergencySelectField` + `useSelectResource` (resource `referralInstitutions`, `referralDoctors`, `serviceUnits`, `diagnoses`) | REUSE |
| Unit Tujuan + opsi nonaktif | `EmergencySelectField` / `FilterSelect` (`option.disabled`) | REUSE |
| Jadwal Dokter | `ScheduleSelect` dari `FE-017` | EXTEND — prop opsional `emptyAction`; tanpa prop perilaku sama |
| Teks, textarea | `EmergencyTextField` / `BaseTextField` | REUSE |
| Alert mitra, Lab, gagal simpan | `EmergencyInlineAlert` | REUSE |
| Popup jadwal praktik | `ConfirmModal` (`variant="info"`, `hideCancel`) + `DataTable` | REUSE |
| Konfirmasi ganti ke Umum | `ConfirmModal` | REUSE |
| Tombol | `BaseButton`, tombol step yang ada | REUSE |
| Kartu Verifikasi | `verificationCard` / `DetailRow` yang ada | REUSE |
| Tanggal + jam rujukan | `FilterDatePicker` (`max`) + `FilterTimePicker` (24 jam) | COMPOSE |
| Unggah surat | `<input type="file" hidden>` + `BaseButton` + daftar berkas | COMPOSE — tidak ada base upload |

Pilihan untuk elemen COMPOSE (tidak memerlukan keputusan pengguna):

1. Tanggal + jam:
   - **(Rekomendasi, dipakai)** Rangkai dua picker yang ada. Konsisten secara visual dan tanpa risiko regresi.
   - Buat date-time picker baru. Biaya lebih besar dan berpotensi berbeda gaya.
2. Unggah:
   - **(Rekomendasi, dipakai)** Input berkas asli yang dipicu `BaseButton`. Dapat dioperasikan dengan keyboard dan tidak menyentuh komponen bersama.
   - Pakai `UploadPhotoField`. Komponen itu khusus foto dan berkas tunggal, jadi tidak cocok.

### 3.3 Delta terhadap desain

| Hal | Desain | Implementasi | Alasan |
| --- | --- | --- | --- |
| Jenis Kunjungan Rujukan | "Rujukan" | Opsi lama *Rujukan / Tindak Lanjut* (`VisitType.FollowUp`, `RJ-DOC-DEC-036`) membuka alur Rujukan; payload `isReferral: true` | Opsi itu dipakai bersama IGD dan tidak diubah |
| Alert mitra | `role="alert"` (PM-FE.6) | `EmergencyInlineAlert tone="info"` → `role="status"` | Bukan peringatan galat; tetap diumumkan pembaca layar |
| Keterangan Radiologi | Opsi nonaktif + keterangan | Label "Radiologi (belum tersedia)" | `FilterSelect` tidak menampilkan `description` |
| Poli wajib dokter tanpa jadwal pada tanggal itu | — | Tidak dapat lanjut; pesan meminta ubah tanggal di Data Kunjungan | Backend menolak kunjungan tanpa dokter untuk poli seperti ini |

## 4. State yang ditangani

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat unit tujuan...", spinner pada select |
| Kosong | "Fasilitas tidak ditemukan", "Belum ada dokter perujuk untuk fasilitas ini", "Diagnosa tidak ditemukan", "Poli ini belum punya jadwal dokter aktif" |
| Gagal | "Gagal memuat fasilitas perujuk. Coba lagi", alert unit tujuan gagal dimuat + *Coba lagi*, pesan backend pada submit |
| Validasi | Pesan `RJ-VAL-PM-03..08/12/13/15` dari validation matrix |
| Submit ganda | Tombol terkunci; kunjungan yang sudah terbentuk tidak dikirim ulang; jalur Lab memakai `idempotencyKey` |
| Gagal sesudah kunjungan terbentuk | Notice merah + *Coba lagi* di layar Selesai |
| Privasi | Berkas dan isian rujukan tidak disimpan di Redux maupun `localStorage` |

## 5. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/referral-institutions/options` | Fasilitas perujuk + `isPartner` | `ReferralInstitution : Read` |
| `GET` | `/v1/health-services/master-data/referral-doctors/options?referralInstitutionId=` | Dokter perujuk | `ReferralDoctor : Read` |
| `GET` | `/v1/health-services/master-data/service-units/options?serviceUnitType=4` | Unit Laboratorium | `ServiceUnit : Read` |
| `GET` | `/v1/health-services/master-data/diagnoses/options?search=` | Diagnosa ICD-10 | `Diagnosis : Read` |
| `POST` | `/v1/health-services/registration-management/patient-encounters/admin` | Kunjungan poli + blok `referral` | `PatientEncounter : Create` |
| `POST` | `/v1/health-services/laboratory-management/lab-patient-registrations/external-referral` | Kunjungan Lab | `LabPatientRegistration : Create` |
| `PUT` | `/v1/…/patient-encounters/{id}/referral` | Rincian rujukan jalur Lab | `PatientEncounter : Update` |
| `POST` | `/v1/…/patient-encounters/{id}/referral/documents` | Unggah surat | `PatientEncounter : Update` |

## 6. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` berkas yang berubah | exit 0, tanpa temuan | `PASS` |
| `npm run build` | exit 0 (dijalankan ulang sesudah perbaikan label Radiologi) | `PASS` |
| Uji fungsi murni (`node --import ./tests/helpers/register.mjs`, skrip scratchpad) | **12/12 PASS** | `PASS` |
| Uji layar Playwright: build produksi `localhost:3100`, API 7184 dialihkan ke backend uji 7185, data `PMTEST` | **17/17 PASS** | `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| R1 | Umum: bar step 6 langkah, Poliklinik ada | PASS |
| R2 | Rujukan: bar step 7 langkah dengan Data Rujukan; Poliklinik hilang dari Data Kunjungan | PASS |
| R3 | `RJ-AC-PM-04`: step Data Rujukan, jam terisi waktu sekarang, Unit Tujuan di step ini | PASS |
| R5a / R5 | `RJ-AC-PM-05`: `PMTEST-PKM` tanpa alert; `PMTEST-KSS` → alert mitra | PASS |
| R7 | `RJ-AC-PM-07`: Radiologi nonaktif "(belum tersedia)" | PASS |
| R6 | `RJ-AC-PM-06`: Breast Clinic tanpa jadwal → *Lihat Jadwal Praktik* → popup "Jadwal Praktik Breast Clinic" | PASS |
| R4 | `RJ-AC-PM-08`: identitas, unit tujuan, diagnosa/alasan, surat, format berkas → tidak dapat lanjut | PASS |
| R8 | Rujukan → Umum: konfirmasi, isian dikosongkan, bar step kembali 6 | PASS |
| R9 | Verifikasi: kartu Data Rujukan, "1 berkas", LANGKAH 6 | PASS |
| R10 | Unggah gagal (simulasi `500`) → kunjungan terbentuk, *Coba lagi* tampil, rujukan `Incomplete` (`documents`) | PASS |
| R11 | *Coba lagi* → rujukan poli lengkap: `targetUnitType` 1, diagnosa, 1 surat, `isComplete` | PASS |
| R12 | Surat diunduh lewat endpoint berizin: `200`, `application/pdf`, `no-store` | PASS |
| R13 | Slip Selesai memuat Poliklinik | PASS |
| R14 | Unit tujuan Laboratorium → Unit Laboratorium + keterangan; tanpa jadwal dokter | PASS |
| R15 | Rujukan Lab hari ini: registrasi Lab + `PUT` + unggah → `targetUnitType` 2, `isComplete` | PASS |
| R16 | Pendaftaran IGD: bar step tanpa Data Rujukan | PASS |

Uji fungsi murni mencakup:

- `RJ-VAL-PM-15`: tanggal mendatang untuk Lab ditolak; penjamin perusahaan ditolak, tunai/asuransi boleh.
- `RJ-VAL-PM-07`: jam rujukan di masa depan ditolak.
- Radiologi ditolak; dokter perujuk opsional.
- Batas berkas 5 MB / 10 berkas.
- Bentuk ketiga payload.
- Kembali dari Pembayaran ke Data Rujukan.
- Urutan popup jadwal.

Dua kunjungan uji dibatalkan lewat `PATCH …/admin/{id}/cancel` (`200`): `d47a4651-302e-4e82-b458-bbbbc404b81f` (poli) dan `a92eebc9-1621-4be8-bb20-a568a6bf40a5` (Lab).

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru di repository; uji fungsi murni dijalankan dari scratchpad.`

## 7. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `RJ-AC-PM-04`, `05`, `06`, `07`, `08` | Terpenuhi | R3, R5, R6, R7, R4 |
| 2. Rujukan poli lengkap tersimpan dan surat terunduh lewat endpoint berizin | Terpenuhi | R11, R12 |
| 3. Rujukan Lab hari ini tersimpan; tanggal mendatang/penjamin perusahaan dicegah (`RJ-VAL-PM-15`) | Terpenuhi — simpan lewat layar (R15); pencegahan dibuktikan lewat uji fungsi murni yang dipanggil hook di langkah Data Rujukan dan Pembayaran (tidak diuji lewat klik kalender/penjamin perusahaan) | R15; uji fungsi |
| 4. Unggah gagal → Selesai menampilkan Coba lagi, dan coba lagi berhasil | Terpenuhi | R10, R11 |
| 5. Pendaftaran Umum dan IGD tidak berubah | Terpenuhi | R1, R16; `PaymentMethodStep` default `stepNumber = 3` |
| 6. Lint, build | Terpenuhi | Bagian 6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Master Diagnosis berisi nama ICD berbahasa Inggris (contoh "Scarlet fever"), sehingga pencarian kata Indonesia seperti "demam" tidak menemukan hasil. Ini data, bukan kode |
| Dependency backend | Tidak ada yang terbuka |
| Perubahan sampingan | `PaymentMethodStep` mendapat prop opsional `stepNumber`; `ScheduleSelect` diekspor dengan prop opsional `emptyAction`. Default keduanya tidak berubah |
| Lingkungan uji | Build produksi 3100 dan backend uji 7185 dari scratchpad; dev server 3000/7184 milik pengguna tidak dipakai |

## 9. Revisi 9 Oktober 2026 (permintaan Sukma Giri)

| Permintaan | Perubahan | Berkas |
| --- | --- | --- |
| Toast pendaftaran RJ memakai base component | Notifikasi cari pasien diteruskan ke `ToastStack` bawaan lewat prop opsional `onToast` di `PatientSelectionStep`. IGD tetap memakai toast lamanya (prop tidak dikirim) | `patient-selection-step.jsx`, `outpatient-registration-page.jsx` |
| Kartu jadwal dokter terpilih seperti kartu jadwal Kiosk, versi ringkas | Kartu baru: foto (cadangan inisial bila gagal dimuat), nama, spesialis, badge Aktif (`StatusBadge`), Jam Praktik, Hari, Poliklinik. Sesi, Ruangan, dan Estimasi dihapus sesuai permintaan lanjutan | `outpatient-schedule-card.jsx` (baru), `outpatient-visit-step.jsx`, normalizer jadwal (`doctorPhotoUrl`, `estimatedServiceMinutes`, `getSchedulePracticeDayLabel`), CSS `scheduleCard*` |
| Dokter perujuk dapat diketik manual atau dipilih | Nama yang tidak ada di daftar ditawarkan sebagai opsi `+ Tambah "…"`. Memilihnya menyimpan dokter ke master Dokter Perujuk pada fasilitas terpilih (`POST /referral-doctors`), lalu langsung memilihnya. Kunjungan tetap hanya menyimpan ID dokter perujuk (kontrak tidak berubah). Opsi tambah hanya tampil bila sesi memegang `ReferralDoctor : Create` | `referral-doctor-select-field.jsx` (baru, dipakai juga modal `FE-020`), `createOutpatientReferralDoctor` |
| Diagnosa di samping Unit Tujuan, Catatan Diagnosa satu baris penuh | Baris 1: Unit Tujuan + Diagnosa ICD-10; Unit Laboratorium di baris sendiri bila dipilih; Catatan Diagnosa textarea penuh seperti Alasan Rujukan. Tata letak yang sama diterapkan di modal Lengkapi/Koreksi | `outpatient-referral-step.jsx`, `outpatient-encounter-referral-modal.jsx` |

**UI GATE revisi: 5 elemen — REUSE 2 (`ToastStack`, `StatusBadge`), COMPOSE 3 (kartu jadwal, opsi tambah dokter perujuk, tata letak B), NEW 0.**

| Verifikasi | Hasil |
| --- | --- |
| `npx eslint` berkas yang berubah | 0 error. Warning `patient-selection-step.jsx` sama dengan `HEAD` (5) |
| `npm run build` | exit 0 |
| Uji layar revisi (build 3100 → backend uji 7185) | **6/6 PASS**: T1 toast bawaan; T2 kartu tanpa Sesi/Ruangan/Estimasi; T3 dokter manual tersimpan ke master (`200`) dan terpilih; T4 pilih dari daftar tetap bisa; T5 IGD tetap toast lama; T6 tata letak wilayah B |
| Regresi | `FE-019` **17/17 PASS**, `FE-020` **8/8 PASS** |

Dokter perujuk uji yang dibuat di T3 dihapus (`DELETE /referral-doctors/{id}`, `200`). Kunjungan uji regresi dibatalkan.

Catatan: dokter perujuk manual menjadi data master baru. Tanpa izin `ReferralDoctor : Create`, petugas hanya bisa memilih dari daftar. Step Data Rujukan Kiosk tidak diubah (akun Kiosk tidak boleh menambah master).

### 9.1 Step Selesai disamakan dengan Kiosk (9 Oktober 2026)

Step Selesai Pendaftaran RJ tidak lagi memakai kartu selebrasi IGD (`RegistrationSuccessStep`). Penggantinya `outpatient-completed-step.jsx` (baru) dengan tata letak tiket Kiosk:

- Header "Pendaftaran selesai".
- Nomor antrean besar dengan nama poli. Untuk rujukan Laboratorium, yang ditampilkan nomor kunjungan.
- Panel "Ringkasan kunjungan" berisi status dan sembilan butir: nama, No. RM, No. kunjungan, poli/unit, dokter, tanggal, jadwal praktik, penjamin, ruangan.

Versi petugas tidak memakai hitung mundur, dan menyediakan tombol **Cetak Slip** (`onPrint` route bila ada, selain itu cetak peramban) serta **Daftarkan Pasien Lain**. Sebelumnya tombol Cetak Slip tidak berfungsi karena route tidak mengirim `onPrint`. IGD tetap memakai `RegistrationSuccessStep`.

UI GATE: REUSE 1 (`BaseButton`), COMPOSE 1 (tiket bertoken), NEW 0.

| Verifikasi | Hasil |
| --- | --- |
| `npx eslint` folder `outpatient-registration` | exit 0 |
| `npm run build` | exit 0 |
| Uji layar step Selesai (pendaftaran Umum sampai Selesai) | **3/3 PASS** (D1 tata letak, D2 sembilan butir, D3 Daftarkan Pasien Lain). Kunjungan uji dibatalkan |
| Regresi `FE-019` | **17/17 PASS** |

### 9.2 Penyesuaian lanjutan (9 Oktober 2026)

| Permintaan | Perubahan |
| --- | --- |
| Hilangkan toast saat pasien ditemukan | Toast sukses "Pasien ditemukan" tidak diteruskan ke `ToastStack` di Pendaftaran RJ (pasien sudah terlihat di kartu review). Toast gagal/peringatan tetap tampil. IGD tidak berubah |
| Hapus tombol Cetak Slip di step Selesai | Tombol dan prop `onPrint` dihapus; tersisa **Daftarkan Pasien Lain** |
| Kartu Jadwal Dokter dan Jenis Kunjungan sejajar di bawah | Baris memakai `align-items: stretch`; kartu jadwal dan kartu Jenis Kunjungan mengisi tinggi kolom |

Verifikasi: ESLint exit 0, `npm run build` exit 0, uji layar **6/6 PASS** (D1–D6, termasuk D4 tanpa toast sukses, D5 sisi bawah sejajar 615,2 vs 615,2 px, D6 tanpa Cetak Slip), uji revisi **6/6 PASS**. Kunjungan uji dibatalkan.
