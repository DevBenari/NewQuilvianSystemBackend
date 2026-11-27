# Laporan Perubahan Frontend — `FE-KSK-PO-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-002` |
| Judul | Langkah Data Pasien, Jenis Pasien, Data Kunjungan, Penjamin |
| Slice | `EPIC KSK-PO-05` — halaman publik frontend (`FR-KSK-PO-041`, `042`), gelombang 2, rilis `MVP-1` |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-002` |
| Trace | `KSK-DEC-029`, `031`, `035`, `043`, `047`; `KSK-PO-FE-001`, `002`; `KSK-DSN-PO-004`, `012`, `013` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`), grup *Online Registration Public* `GET options/*`, `POST patient-match`; validation matrix `KSK-VAL-PO-001` s.d. `012`, `020` s.d. `026` (sisi FE) |
| Wewenang UI | `KSK-PO-FE-001` (urutan langkah sama dengan menu RJ), `KSK-PO-FE-002` (hanya data tersamar), `KSK-PO-FE-004` (tata letak `DEV_DISCRETION`) |
| Dependency | `FE-KSK-PO-001` ✅ (dan `BE-KSK-PO-004` ✅ untuk data) |
| Klasifikasi | `MEDIUM` — halaman publik baru dan perubahan tiga komponen bersama RJ/IGD lewat prop opsional |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: view, hook, util, konstanta, dan style halaman online; prop opsional pada tiga komponen RJ/IGD; laporan dan baris status di blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `508c8beb8` (`sukmagpV2`) + perubahan `FE-KSK-PO-001` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `93f0fa1b` (`sukmagp`) + perubahan `BE-KSK-PO-008`/`010` yang belum di-commit |
| Tanggal | 2026-11-20; rework 2026-11-23 (format No. RM, bagian 9) |
| Status | ✅ Selesai — AC 1, AC 2 (tampilan), dan kriteria tambahan 1–3 terbukti di browser |

## 1. Keadaan yang ditemukan di awal

`/online-registration` dari `FE-KSK-PO-001` baru berisi kerangka halaman dan widget captcha percobaan. Langkah-langkah RJ yang akan dipakai ulang ternyata terikat ke endpoint petugas di tiga tempat, sehingga tidak bisa dipasang apa adanya di halaman tanpa login:

| Komponen RJ | Ketergantungan ke endpoint petugas | Akibat bila dipakai apa adanya |
| --- | --- | --- |
| `ScheduleSelect` di `outpatient-visit-step.jsx` | `useOutpatientScheduleQuotas` memanggil `GET patient-encounters/doctor-schedule-quotas` | Request tanpa token → `401` → pengunjung dialihkan ke login |
| `PaymentMethodStep` (dibungkus `OutpatientPayerStep`) | Selalu merender `PatientPayerModal`, yang memuat penyedia asuransi, penjamin perusahaan, dan Benefit Karyawan dari endpoint petugas | Tombol "Daftarkan Penjamin Baru" membuka modal petugas |
| `EmergencyDateField` | Tidak meneruskan `min`/`max` ke `FilterDatePicker` | Tanggal hari ini tetap dapat dipilih |

Langkah Data Pasien RJ (`PatientSelectionStep`, 2.018 baris) adalah pencarian bebas petugas. Rencana `03-frontend-architecture.md` PO-FE.4 sejak awal menetapkan langkah ini dibuat baru untuk halaman publik.

## 2. Proses bisnis dari sisi pengguna

1. Pasien membuka `/online-registration`. Seperti di menu RJ, layar pertama menanyakan **Pasien Lama** atau **Pasien Baru**. Bar langkah baru tampil sesudah salah satu dipilih.
2. **Pasien Lama** memilih NIK atau No. Rekam Medis, mengisi nomornya dan tanggal lahir, lalu menyelesaikan verifikasi keamanan. Tombol **Periksa Data** baru aktif sesudah captcha selesai. Bila cocok, layar hanya menampilkan nama tersamar, misalnya "Nama pasien: R***** L**** 2***". Bila tidak cocok, pesannya selalu sama: "Data tidak ditemukan. Periksa kembali isian Anda atau daftar di loket pendaftaran." Token captcha hanya berlaku sekali, sehingga percobaan berikutnya menunggu token baru.
3. **Pasien Baru** mengisi NIK, nama lengkap, tanggal lahir, dan jenis kelamin. Tidak ada isian lain (`KSK-DSN-PO-004`).
4. Keduanya mengisi **No. WhatsApp atau Email**. Contoh: `0812-3456-7890` diterima, `0812` ditolak dengan "No. WhatsApp tidak valid. Contoh: 081234567890.", dan `nama@contoh` ditolak dengan "Alamat email tidak valid."
5. **Jenis Pasien** memakai layar RJ apa adanya: Pasien Pribadi, Pasien Asuransi, atau Pasien Rujukan. Pilihan ini menentukan urutan langkah berikutnya, sama dengan menu RJ:
   - Pribadi → Data Kunjungan (biaya Tunai, tanpa langkah Penjamin).
   - Asuransi → Penjamin → Data Kunjungan.
   - Rujukan → Data Kunjungan → Data Rujukan → Penjamin.
6. **Data Kunjungan**: kalender hanya membuka besok sampai 30 hari ke depan. Contoh pada 20 Nov 2026: tanggal 20 Nov dan tombol "Hari ini" nonaktif, 21 Nov sampai 20 Des aktif, dan 21 Des nonaktif. Sesudah tanggal dipilih, daftar poli dimuat dari endpoint publik. Sesudah poli dipilih, kartu jadwal dokter tampil dengan baris "Sisa kuota 12". Jadwal yang penuh tampil nonaktif dengan tombol "Penuh".
7. **Penjamin** (Asuransi): tabel menampilkan kartu milik pasien dengan nomor tersamar. Pasien juga dapat menekan **Tambah Kartu Asuransi Baru** dan mengisi penyedia, nomor kartu/polis, nama pemegang, serta hubungan. Kartu baru itu masuk tabel dan langsung terpilih, tetapi hanya disimpan di memori sampai pendaftaran dikirim (`FE-KSK-PO-003`).
8. Langkah Data Rujukan, General Consent, Verifikasi, dan Selesai milik `FE-KSK-PO-003`. Untuk sementara langkah itu hanya menampilkan "Langkah ini disiapkan pada tahap berikutnya…" dengan tombol Kembali. Tidak ada data yang dikirim.
9. Muat ulang halaman berarti mulai dari awal. Tidak ada isian yang disimpan di URL maupun browser storage.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`outpatient-registration-page.jsx`, `outpatient-visit-step.jsx`, `outpatient-visit-type-step.jsx`, `outpatient-payer-step.jsx`, `outpatient-schedule-card.jsx`, `outpatient-practice-schedule-modal.jsx`, `use-outpatient-registration.js`, `use-outpatient-visit-options.js`, `use-outpatient-schedule-quotas.js`, `outpatient-registration.utils.js`, `outpatient-referral.utils.js`, `registration-category-utils.js`, `payment-method-step.jsx`, `emergency-patient-payer-modal.jsx`, `patient-payer-table.jsx`, `patient-entry-choice-step.jsx`, `patient-selection-step.jsx`, `emergency-registration-fields.jsx`, `emergency-registration-stepper.jsx`, `filter-date-picker.jsx`, `filter-select.jsx`, `base-text-field.jsx`, `confirm-modal.jsx`. Di backend: `OnlineRegistrationPublicDtos.cs`, enum online, `OnlineRegistrationPublicService.cs` dan `.Submit.cs` (aturan NIK, No. RM, tanggal lahir, No. WhatsApp, email, dan penjamin).

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/app/online-registration/page.jsx` | Diubah | Merender `OnlineRegistrationPage`; widget captcha percobaan FE-001 dilepas |
| `src/components/view/online-registration/online-registration-page.jsx` | Baru | Wizard publik: pilihan Pasien Lama/Baru, lalu langkah RJ yang dipakai ulang lewat props dan langkah sementara untuk `FE-KSK-PO-003` |
| `src/components/view/online-registration/online-registration-patient-step.jsx` | Baru | Langkah Data Pasien publik (pasien lama + captcha + nama tersamar; pasien baru; kontak) |
| `src/components/view/online-registration/online-registration-new-insurance-modal.jsx` | Baru | Modal kartu asuransi baru, empat isian, penyedia dari endpoint publik |
| `src/components/view/online-registration/online-registration-view.jsx` | Diubah | Prop `showStepper`; bar langkah baru tampil sesudah Pasien Lama/Baru dipilih |
| `src/lib/hooks/online-registration/use-online-registration.js` | Baru | Controller langkah: form di memori, pencocokan pasien, token captcha sekali pakai, aturan penjamin RJ (`resolveOutpatientPayer`), navigasi langkah RJ |
| `src/lib/hooks/online-registration/use-online-registration-visit-options.js` | Baru | Poli dan jadwal dari endpoint publik dalam bentuk yang sama dengan `useOutpatientVisitOptions`, plus `fullScheduleIds` dari `isFull` |
| `src/utils/online-registration/online-registration.utils.js` | Baru | Rentang H+1 s.d. H+30, validasi `KSK-VAL-PO-001` s.d. `009` dan `026`, normalisasi No. WhatsApp (cermin backend), pemetaan poli/jadwal/kartu ke bentuk RJ, pesan galat |
| `src/lib/constants/online-registration/online-registration.constants.js` | Diubah | Enum penjamin dan jenis bayar, opsi identitas dan jenis kelamin, teks pilihan Pasien Lama/Baru, batas isian, nilai awal form, pesan validation matrix |
| `src/style/online-registration/online-registration.module.css` | Diubah | `.panel` menjadi wadah polos (langkah RJ sudah berbentuk kartu); `.captchaRow` |
| `emergency-registration/emergency-registration-fields.jsx` | Diubah (prop opsional) | `EmergencyDateField`: `min`, `max`, `required` diteruskan ke `FilterDatePicker`; bawaan kosong |
| `outpatient-registration/outpatient-visit-step.jsx` | Diubah (prop opsional) | `minVisitDate`, `maxVisitDate`, `description`, `noClinicMessage`, `showAppointmentNotice`; `ScheduleSelect` menerima `fullScheduleIds` (bila ada, hook kuota petugas tidak mengirim request); kartu jadwal menampilkan `quotaLabel` bila ada |
| `emergency-registration/payment-method-step.jsx` | Diubah (prop opsional) | `payerModal`, `addPayerLabel`, `canAddPayer`, `selectedPayerDescription`; tanpa prop, `PatientPayerModal` dan teks bawaan tetap |

Tidak ada perubahan backend, migration, atau database.

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Penerapan |
| --- | --- |
| View tidak memanggil Axios | View memanggil `useOnlineRegistration`; request lewat `online-registration-public.service.js` (`_skipAuthRefresh`) |
| Hook membersihkan request | `AbortController` pada opsi poli/jadwal, pencocokan, dan penyedia asuransi; dibatalkan saat unmount atau input berganti |
| Utility murni | Validasi dan pemetaan di `utils/online-registration`, tanpa React dan tanpa request |
| Tanpa arsitektur paralel | Bentuk data `visitOptions`, `registration`, dan nama isian form sama dengan RJ, sehingga komponen RJ dipakai tanpa salinan |
| Privasi (PO-FE.7) | Tanpa Redux, URL, maupun storage; `BaseTextField` memakai `persist` bawaan `false`; tanpa `console` |

### 3.4 Gerbang keputusan base component

UI GATE: 8 elemen — REUSE 3, EXTEND 3, COMPOSE 2, WRAP 0, NEW 0

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Pilihan Pasien Lama/Baru | `PatientEntryChoiceStep` | `emergency-registration/patient-entry-choice-step.jsx`, dipakai menu RJ | REUSE | Prop `eyebrow/title/description/options` dengan kalimat untuk pasien |
| Bar langkah | `EmergencyRegistrationStepper` | dipakai RJ dan FE-001 | REUSE | Tanpa perubahan |
| Jenis Pasien | `OutpatientVisitTypeStep` | `outpatient-registration/outpatient-visit-type-step.jsx` | REUSE | Tanpa perubahan |
| Kalender H+1 s.d. H+30 | `EmergencyDateField` → `FilterDatePicker` (`min`/`max` sudah ada) | `base-features/filter-date-picker.jsx:414-468` | EXTEND | Teruskan `min`/`max` opsional |
| Data Kunjungan | `OutpatientVisitStep` + `ScheduleSelect` | dipakai RJ dan Data Rujukan RJ | EXTEND | Prop opsional sumber data publik (lihat 3.2) |
| Penjamin | `OutpatientPayerStep` → `PaymentMethodStep` | dipakai IGD dan RJ | EXTEND | Modal pengganti dan label lewat prop opsional |
| Langkah Data Pasien | Kartu langkah RJ, `EmergencyTextField/SelectField/DateField`, `TurnstileWidget`, `EmergencyInlineAlert`, `BaseButton` | direncanakan PO-FE.4 | COMPOSE | View baru, tanpa komponen baru |
| Kartu asuransi baru | `ConfirmModal` + `EmergencySelectField/TextField` | `base-features/confirm-modal.jsx` | COMPOSE | Modal empat isian |

Pilihan yang disajikan untuk elemen Penjamin, satu-satunya elemen dengan risiko regresi:

- **A. EXTEND `PaymentMethodStep` dengan modal pengganti lewat prop (rekomendasi, dijalankan).** Tampilan, tabel kartu, dan sumber penjamin rujukan sama persis dengan RJ. Default tidak berubah, sehingga IGD dan RJ tidak terdampak. Kolom Nama Peserta dan Berlaku s.d. tampil "-" karena data publik hanya memuat nomor tersamar.
- **B. Susun langkah Penjamin publik sendiri.** Tanpa risiko regresi, tetapi tampilannya menyimpang dari RJ dan bertentangan dengan butir reuse di roadmap.

Tidak ada elemen `NEW`, dan tidak ada `EXTEND` yang mengubah perilaku bawaan. Karena itu tidak ada keputusan yang perlu menunggu pemilik. Tombol langkah memakai kelas langkah RJ (`primaryButton`/`secondaryButton`) supaya seragam dengan langkah RJ yang dipakai ulang di wizard yang sama.

## 4. State yang ditangani di layar

| Keadaan | Penanganan |
| --- | --- |
| Memuat | Pilihan poli "Memuat poliklinik...", kartu jadwal "Memuat jadwal dokter...", tombol "Memeriksa..." saat pencocokan, penyedia asuransi "Memuat penyedia asuransi..." |
| Kosong | Tidak ada poli: "Belum ada jadwal online pada tanggal ini. Pilih tanggal lain." Pasien baru/tanpa kartu: tabel kosong, tombol Tambah Kartu Asuransi Baru tetap ada |
| Gagal | Poli/jadwal: pesan + [Coba lagi]; penyedia asuransi: pesan + [Coba lagi]; isian tidak hilang |
| `404` pencocokan | Pesan umum `KSK-VAL-PO-004` |
| `429` | "Terlalu banyak percobaan. Silakan coba lagi nanti." |
| `400`/`503` captcha | Pesan dari backend (`KSK-VAL-PO-010`/`011`) |
| Captcha belum siap | Komponen FE-001: "Verifikasi keamanan belum siap, muat ulang halaman."; tombol Periksa Data tetap nonaktif |
| Data basi | Isian identitas diubah sesudah cocok → hasil cocok batal dan harus diperiksa ulang; poli/jadwal yang hilang dari daftar dilepas otomatis |
| Kirim ganda | Periksa Data terkunci selama request; token sekali pakai |

## 5. Endpoint yang dikonsumsi

| Endpoint | Pemakaian |
| --- | --- |
| `GET /api/v1/health-services/registration-management/online-registrations/public/options/clinics?visitDate=` | Daftar poli Data Kunjungan |
| `GET …/public/options/doctor-schedules?clinicId=&visitDate=` | Kartu jadwal, sisa kuota, penanda penuh |
| `GET …/public/options/insurance-providers` | Modal kartu asuransi baru (dimuat saat modal dibuka) |
| `POST …/public/patient-match` | Pasien lama: `{ identityType, identityValue, birthDate, turnstileToken }` → `{ maskedName, payers[] }` |

Uji browser mencatat seluruh request ke backend: 8 request, semuanya ke grup `/online-registrations/public`. Tidak ada request ke endpoint petugas (kuota, data pegawai, penjamin).

## 6. Verifikasi

Lingkungan uji: dev server frontend di port 3000 dan backend uji di `https://localhost:7185` (build Release dari scratchpad, kunci uji Turnstile Cloudflare). Request browser ke 7184 dialihkan ke 7185. Keduanya saya nyalakan sendiri, lalu saya matikan sesudah uji. Backend user di 7184 tidak disentuh. Data pasien lama memakai pasien uji BE-005 di `QuilvianNewDevSukma`, dan identitasnya tidak dicetak di log uji.

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` pada 12 berkas JS/JSX yang disentuh (folder online-registration + 3 komponen bersama) | PASS — 0 error, 0 warning |
| `npm run lint:errors` | PASS — 0 error |
| `npm run build` | PASS — 854/854 halaman, `ƒ /online-registration` |
| Playwright halaman publik, HP 375 × 800, tiga jenis pasien (`public-flow.cjs`) | **38/38 PASS** |
| Playwright regresi menu RJ petugas, 1366 px, satu pasien lama per jenis pasien sampai Verifikasi, tanpa menyimpan (`rj-regression.cjs`) | **13/13 PASS** |
| Grep konsistensi UI (`ui-consistency-checklist.md`) | Bersih: tanpa hex/rgba, tanpa `!important`, tanpa `fw-*/fs-*`, tanpa `<table>` mentah, tanpa inline style; ukuran huruf hanya token FE-001; `<button>` mentah hanya tombol langkah berkelas langkah RJ |
| Grep privasi (`console`, `localStorage`, `sessionStorage`) di view/hook/util/service online | 0 hasil |

Rincian uji halaman publik:

| Kelompok | Butir |
| --- | --- |
| T0 | Pilihan Pasien Lama/Baru tampil lebih dulu, bar langkah belum tampil; HP tanpa gulir mendatar |
| T1 Pasien Baru + Pribadi | Urutan langkah; isian kosong menampilkan lima pesan validasi; NIK 15 digit, No. WA pendek, dan email tanpa domain sah ditolak; tanggal lahir besok nonaktif; hari ini dan tombol "Hari ini" nonaktif, besok aktif, H+30 aktif, H+31 nonaktif; poli dan kartu jadwal "Sisa kuota 12" dari endpoint publik; info appointment petugas tidak tampil; menuju General Consent; URL tanpa query dan storage tanpa NIK/nama/No. WA |
| T2 Pasien Lama + Asuransi | Tanpa General Consent; Periksa Data nonaktif sampai captcha; tanggal lahir salah → pesan umum `404`; token sekali pakai; cocok → nama tersamar 17 karakter dan NIK/No. RM tidak tampil sebagai teks; urutan Penjamin → Data Kunjungan; kartu milik pasien bernomor tersamar; tanpa kartu → pesan; kartu baru kosong ditolak `KSK-VAL-PO-026`; kartu baru masuk tabel dan terpilih; Data Kunjungan → Verifikasi; Kembali dua langkah mempertahankan kartu terpilih |
| T3 Pasien Baru + Rujukan | Urutan 8 langkah sama dengan RJ; Data Kunjungan tanpa poli; tanpa tanggal → pesan `KSK-VAL-PO-020`; menuju Data Rujukan |
| P | Storage akhir tanpa data pasien; semua request hanya grup publik; tidak pernah dialihkan ke `/login`; konsol tanpa pelanggaran CSP |

Rincian regresi menu RJ petugas: login petugas; Pribadi dengan kalimat Data Kunjungan bawaan; `ScheduleSelect` tetap memanggil `doctor-schedule-quotas`, dan kartu tanpa baris sisa kuota; kalender RJ tetap bisa memilih hari ini; tanggal mendatang menampilkan "Pendaftaran untuk tanggal mendatang"; Pribadi → Verifikasi. Asuransi: tombol "Daftarkan Penjamin Baru" dan "Ringkasan data yang masuk ke payload encounter." tetap ada, dan modal petugas "Daftarkan Penjamin" terbuka dengan request penyedia petugas; Penjamin → Data Kunjungan → Verifikasi. Rujukan: kalimat bawaan, tanpa poli, lanjut ke Data Rujukan. Tidak ada `POST patient-encounters`, dan halaman tanpa galat. IGD memakai `PaymentMethodStep` dan `EmergencyDateField` tanpa prop baru, sehingga perilakunya sama dengan nilai bawaan yang dibuktikan regresi RJ di atas. Layar IGD tidak diuji terpisah.

Run pertama gagal karena skrip, bukan kode. Ada tiga sebab: selektor `text=` bentrok dengan teks keterangan, klik dilakukan sebelum hidrasi sesudah muat ulang, dan kalender tetap di bulan Desember. Sesudah skrip diperbaiki, hasilnya seperti tabel. Uji pertama juga menemukan satu cacat tampilan nyata: nama tersamar terpecah menjadi tiga baris karena CSS `.inlineAlert strong { display: block }`. Cacat itu sudah diperbaiki, lalu uji diulang.

```
MANUAL TEST: Playwright — halaman publik 38/38 PASS, regresi RJ petugas 13/13 PASS
AUTOMATED TEST: SKIPPED (opsional) — tes otomatis baru tidak diwajibkan test-policy; bukti perilaku dari uji browser di atas
```

Skrip dan tangkapan layar ada di scratchpad sesi (`fe-po002/public-flow.cjs`, `rj-regression.cjs`, `t0-choice-375.png`, `t1-visit-375.png`, `t2-matched-375.png`, `t2-payer-375.png`, `t3-referral-375.png`) dan tidak di-commit.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Bukti | Status |
| --- | --- | --- |
| AC 1 (tampilan): halaman tanpa login; urutan langkah Pribadi, Asuransi, Rujukan = menu RJ | P3; T1a, T2f, T3a (judul langkah dari `getOutpatientRegistrationSteps`) | ✅ |
| AC 2 (tampilan): pasien lama cocok → hanya nama dan kartu tersamar; tidak cocok → pesan seragam | T2c, T2e, T2g | ✅ |
| Tambahan 1: menu RJ petugas berperilaku sama | Regresi 13/13 | ✅ |
| Tambahan 2: tanpa data pasien di URL maupun browser storage | T1k, P1 | ✅ |
| Tambahan 3: tanggal hari ini tidak dapat dipilih | T1g | ✅ |
| DoD: tabel keputusan base component dan baris `UI GATE` | Bagian 3.4 | ✅ |

## 8. Catatan penutup

- **Penjamin Pasien Rujukan** (pilihan sumber BPJS/asuransi swasta/perusahaan/Pribadi-Tunai) memakai logika RJ apa adanya. Langkah itu baru dapat dicapai sesudah Data Rujukan, jadi uji layarnya masuk `FE-KSK-PO-003`. Untuk sumber Penjamin Perusahaan, tombol tambah kartu disembunyikan, karena penjamin perusahaan baru tidak tersedia online (`KSK-DSN-PO-012`).
- Langkah Data Rujukan, General Consent, Verifikasi, dan Selesai sementara menampilkan langkah "disiapkan pada tahap berikutnya". `FE-KSK-PO-003` menggantinya dengan isi yang sebenarnya. Halaman belum boleh dibuka untuk umum sebelum `FE-KSK-PO-003` selesai.
- `hasReferralData` sementara selalu `false`, karena isian rujukan belum ada. `FE-KSK-PO-003` mengisinya supaya konfirmasi "isian rujukan akan dikosongkan" milik RJ berlaku.
- Uji browser `FE-KSK-PO-001` (`browser-check.cjs`) mengharapkan bar langkah dan widget captcha tampil saat halaman dibuka. Sejak task ini keduanya baru tampil sesudah pilihan Pasien Lama/Baru, mengikuti alur RJ. Pemuatan captcha tanpa pelanggaran CSP kini dibuktikan T2b dan P4.
- Pesan "Penjamin belum valid" di langkah Penjamin tetap tampil sampai tombol Lanjut ditekan lagi, walau kartu sudah dipilih. Ini perilaku bawaan `PaymentMethodStep` RJ/IGD dan tidak diubah.
- Catatan go-live dari `FE-KSK-PO-001` tetap berlaku: `NEXT_PUBLIC_TURNSTILE_SITE_KEY` belum ada di `build.args` Docker (`KSK-PO-OQ-016`).
- Tidak ada commit, push, migration, atau eksekusi database.

**Task berikutnya:** `FE-KSK-PO-003` (Data Rujukan, General Consent, Verifikasi + Kirim, Selesai). Dependency `FE-KSK-PO-002` ✅ dan `BE-KSK-PO-005` ✅ sudah terpenuhi.

## 9. Rework 2026-11-23 — format No. Rekam Medis

**Masalah dari pemilik (Sukma, 2026-11-23).** Di langkah Data Pasien untuk pasien lama, pilihan Jenis Identitas "No. Rekam Medis" menerima angka polos seperti `00000001`. No. RM rumah sakit ditulis per 2 digit bertanda hubung, misalnya `00-00-00-01`, sama dengan pola RJ (`toMedicalRecordSearch` di `outpatient-registration.utils.js`). Pasien yang menyalin No. RM dari kartu berobat melihat bentuk yang berbeda.

**Perilaku sesudah perbaikan.** Saat pasien mengetik `00000001`, isian langsung tampil `00-00-00-01`. Huruf, spasi, titik, dan tanda hubung yang diketik sendiri dibuang lalu disusun ulang per 2 digit. Batasnya tetap 20 digit (`KSK-VAL-PO-002`), sehingga isian paling panjang 29 karakter. NIK tidak berubah: 16 digit angka tanpa format. Contoh pada isian menjadi "Contoh: 00-00-00-01".

**Kontrak.** Tidak ada delta. Backend `NormalizeMedicalRecordNumber` sudah membuang spasi, titik, dan tanda hubung sebelum validasi dan pencocokan, sehingga nilai berformat diterima apa adanya. Validator frontend `validateOnlineMedicalRecordNumber` juga sudah membuang tanda hubung.

| Berkas | Perubahan |
| --- | --- |
| `src/utils/online-registration/online-registration.utils.js` | Fungsi baru `formatOnlineMedicalRecordNumber` (angka saja, paling banyak 20 digit, digabung per 2 digit dengan `-`) |
| `src/lib/constants/online-registration/online-registration.constants.js` | Batas `MEDICAL_RECORD_INPUT_MAX_LENGTH: 29` (20 digit + 9 tanda hubung) |
| `src/components/view/online-registration/online-registration-patient-step.jsx` | Field `identityValue`: `numberOnly` hanya untuk NIK; No. RM diformat lewat `onValueChange` → `setValue`; `maxLength` No. RM 29; placeholder "Contoh: 00-00-00-01" |

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Isian No. RM | `REUSE` `EmergencyTextField` (`BaseTextField`) dengan `onValueChange` yang sudah ada; tanpa komponen atau props base baru | `base-text-field.jsx` (`onValueChange`, `numberOnly`, `maxLength`) |

`UI GATE: PASS` — tidak ada elemen `NEW` maupun `EXTEND`.

| Verifikasi | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint 3 berkas yang diubah | 0 error, 0 warning | `PASS` | Keluaran eslint |
| Uji browser HP 375 px, dev server 3000, request 7184 dialihkan ke 7185 | 11/11 PASS: NIK tetap angka; ganti ke No. RM mengosongkan isian dan placeholder `00-00-00-01`; ketik `00000001` → `00-00-00-01`; hapus satu digit → `00-00-00-0`; huruf dan tanda hubung ketikan dibuang; tempel `12.34 56-78` → `12-34-56-78`; batas 20 digit (29 karakter); No. RM pasien uji diketik tanpa tanda hubung tampil berformat; Periksa Data dengan nilai berformat → "Data pasien ditemukan" (request membawa nilai berformat); No. RM tidak tampil sebagai teks; tanpa `pageerror` | `PASS` | `scratchpad/fe-po002/mrn-format.cjs`, `mrn-format-375.png` |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — repository tidak memakai test runner untuk komponen ini | — | `test-policy.md` |

Risiko: kursor melompat ke akhir isian bila pasien menyunting digit di tengah, karena nilai disusun ulang setiap ketikan. No. RM pendek, jadi dampaknya kecil. Status task tetap ✅. Tidak ada commit.
