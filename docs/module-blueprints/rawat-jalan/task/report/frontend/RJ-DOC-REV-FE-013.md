# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-013` |
| Judul | Layar Pendaftaran Pasien Rawat Jalan oleh petugas |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `14.1` |
| Trace | `RJ-DOC-DEC-034`..`039` (`00-interview-decisions.md`, *Amendment PR*) |
| Kontrak | Endpoint yang sudah ada, tanpa perubahan: `POST /patient-encounters/admin`, `GET /clinics/admin/options`, `GET /doctor-schedules/admin`, serta endpoint pasien dan penjamin Pendaftaran IGD |
| Dependency | Tidak ada task backend. Source backend tidak diubah |
| Wewenang | `RJ-DOC-DEC-038` (Sukma Giri, 6 Okt 2026) |
| Repository / branch | `V2QuilvianSystemFrontendDev` @ `sukmagpV2` (`5189175bf`). Belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 6 Oktober 2026 |
| Status | ✅ SELESAI |

## 1. Perubahan

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `src/app/health-services/registration-management/outpatient-registration/page.jsx` | Baru | Route tipis |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-registration-page.jsx` | Baru | Komposisi halaman. Memakai stepper, pilihan jenis pasien, langkah pasien, langkah pembayaran, dan langkah selesai milik IGD |
| `.../outpatient-registration/outpatient-visit-step.jsx` | Baru | Langkah 2 *Data Kunjungan*: tanggal kunjungan, poliklinik, kartu jadwal dokter, jenis kunjungan, keluhan utama. Keadaan: belum pilih poliklinik, memuat, gagal + Coba lagi, kosong, berisi |
| `.../outpatient-registration/outpatient-verification-step.jsx` | Baru | Langkah 4: ringkasan pasien, kunjungan (walk-in/appointment), pembayaran, konfirmasi wajib |
| `.../outpatient-registration/index.js` | Baru | Barrel export |
| `src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-registration.js` | Baru | Controller alur. Kontrak handler sama dengan `useEmergencyRegistration`. Validasi kunjungan (`RJ-DOC-DEC-035`, `037`), satu `POST /patient-encounters/admin`, reset saat halaman dibuka |
| `.../outpatient-registration/use-outpatient-visit-options.js` | Baru | Opsi poliklinik dan jadwal per poliklinik; hasil disimpan bersama kunci permintaan, request dibatalkan saat berganti |
| `src/lib/services/health-services/registration-management/outpatient-registration.service.js` | Baru | `fetchOutpatientClinicOptions`, `fetchOutpatientDoctorSchedules`, `createOutpatientPatientEncounter` lewat `InstanceAxios` |
| `src/lib/state/slice/health-services/registration-management/outpatient-registration-slice.jsx` | Baru | Slice terpisah dengan bentuk state IGD. Pasien dan penjamin memakai service IGD |
| `src/lib/constants/.../outpatient-registration/outpatient-registration.constants.js` | Baru | Endpoint, langkah, enum jadwal, default form |
| `src/utils/.../outpatient-registration/outpatient-registration.utils.js` | Baru | Normalisasi poliklinik/jadwal, saringan jadwal per tanggal, payload encounter, format tanggal |
| `src/lib/state/store.jsx` | Diperbarui | Reducer `outpatientRegistration` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui | Butir *Pendaftaran Pasien* paling atas pada menu Rawat Jalan, `requiredPermission` `PatientEncounter : Create` |
| `.../emergency-registration/patient-selection-step.jsx` | Diperbarui | Prop opsional `flowLabel` (default `"IGD"`) pada satu kalimat bantuan |
| `.../emergency-registration/payment-method-step.jsx` | Diperbarui | Prop opsional `description`, `payerTableDescription`; default = teks IGD lama |
| `.../emergency-registration/registration-success-step.jsx` | Diperbarui | Prop opsional `title`, `description`, `slipTitle`, `slipNotice`, `qrFallbackCode`, `extraTicketRows`, `nextSteps`; default = teks IGD lama. Empat langkah berikutnya kini dirender dari data, ikon dan teks sama |

Diff berkas lama: 5 berkas `+154 / -138`. Berkas baru: 11.

**Delta terhadap rencana awal:** jadwal dokter dibaca dari `GET /doctor-schedules/admin`, bukan
`admin/options`. Uji layar membuktikan `admin/options` tidak membawa masa berlaku jadwal, sehingga
jadwal yang sudah kedaluwarsa (dr. Ranger Biru, berakhir 31 Jul 2026) tampil lalu ditolak backend
"Jadwal dokter tidak sesuai dengan tanggal kunjungan." Daftar admin membawa `EffectiveStartDate`/
`EffectiveEndDate` dengan izin `DoctorSchedule : Read` yang sama.

## 2. Gerbang keputusan base component

`UI GATE: 9 elemen — REUSE 4, EXTEND 3, COMPOSE 2, WRAP 0, NEW 0`

Modul referensi visual: Pendaftaran Pasien IGD (`RJ-DOC-DEC-034`).

| Kebutuhan UI | Kandidat | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Header + shell halaman | class `pageShell`/`pageHeader` IGD | `emergency-registration.module.css` | REUSE | Sama dengan IGD |
| Pilih jenis pasien | `PatientEntryChoiceStep` | prop `eyebrow` sudah ada | REUSE | eyebrow Rawat Jalan |
| Stepper | `EmergencyRegistrationStepper` | prop `steps`, `ariaLabel` sudah ada | REUSE | Langkah RJ |
| Cari/input pasien | `PatientSelectionStep` | satu teks "IGD" tertanam | EXTEND | Prop opsional `flowLabel` |
| Data Kunjungan | `EmergencyDateField`, `EmergencySelectField` (`FilterSelect`), `EmergencyTextField` (`BaseTextField`), kartu `visitTypeCard`, `EmergencyInlineAlert`, `BaseButton` | dipakai `emergency-visit-step.jsx` | COMPOSE | Tanpa CSS baru |
| Pembayaran | `PaymentMethodStep` | dua teks IGD tertanam | EXTEND | Prop opsional teks |
| Verifikasi | `verificationGrid`/`detailList`, `BaseCheckboxCard`, `EmergencyInlineAlert` | dipakai `verification-step.jsx` | COMPOSE | Komponen langkah RJ |
| Selesai + slip | `RegistrationSuccessStep` | teks dan langkah IGD tertanam | EXTEND | Prop opsional, baris slip tambahan |
| Butir menu | `menu-items.jsx` + `requiredPermission` | dipakai Daftar Pasien Rawat Jalan | REUSE | — |

Pilihan yang disajikan:

- **Langkah pasien, pembayaran, selesai — A (rekomendasi, dipakai):** extend komponen IGD dengan prop opsional berdefault teks lama. Tampilan identik dan IGD tidak berubah. **B:** salin komponen ke folder RJ; tampilan sama hari ini tetapi menyimpang saat IGD diubah.
- **Slice — A (rekomendasi, dipakai):** slice terpisah berbentuk sama. **B:** pakai slice IGD; dua halaman saling menimpa state.

EXTEND tidak mengubah perilaku default; Pendaftaran IGD diuji tetap berlabel IGD (R13).

## 3. Validasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint` pada seluruh berkas task | `0 error, 0 warning` (run pertama: 4 error `react/jsx-key` dan 6 warning baru, diperbaiki). Warning lama `patient-selection-step.jsx` (5, `react-hooks/*`) sudah ada sebelum task |
| `npm run build` | `PASS` (Next.js 16.2.12), dijalankan ulang sesudah perbaikan terakhir; route `/health-services/registration-management/outpatient-registration` terbentuk |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — test-policy; tidak menambah framework test |

### Checklist konsistensi UI (grep pada diff)

| Pemeriksaan | Hasil |
| --- | --- |
| Hex/rgb, `!important`, inline style baru | Tidak ada |
| CSS baru | Tidak ada; seluruh class dari stylesheet IGD |
| `<button>` mentah | Hanya pola tombol langkah dan kartu pilihan IGD yang sama (`primaryButton`, `secondaryButton`, `visitTypeCard`); tombol Coba lagi memakai `BaseButton` |
| Teks | Bahasa Indonesia |

### Verifikasi manual (Playwright, `ui_fe013.cjs`, `ui_fe013_more.cjs`)

Lingkungan: `next dev` di `localhost:3000` (source `sukmagpV2` + perubahan ini), backend build `HEAD`
`5c14a592` tanpa perubahan di `https://localhost:7184`, DB `QuilvianNewDevSukma`
(`BlockActiveEncounter = true`). Login lewat layar sebagai SuperAdmin seed. Pasien uji
`KSKTEST-RM-07`. Hari uji Selasa 6 Okt 2026.

| ID | Skenario | Hasil |
| --- | --- | --- |
| R1 | Butir *Pendaftaran Pasien* tampil di menu Rawat Jalan | `PASS` |
| R2 | Halaman: judul dan eyebrow Rawat Jalan | `PASS` |
| R3 | Pasien lama dicari lewat No. RM, masuk Data Kunjungan | `PASS` |
| R4 | Tanpa poliklinik: petunjuk jadwal; request `clinics/admin/options` | `PASS` |
| R5 | Lanjut tanpa poliklinik ditolak "Pilih poliklinik tujuan." | `PASS` |
| R6 | Poli Umum hari ini: jadwal kedaluwarsa tidak tampil; keadaan kosong menyebut dokter wajib | `PASS` |
| R6b | Request `GET /doctor-schedules/admin?clinicId=…` | `PASS` |
| R7 | `IsDoctorRequired`: lanjut tanpa jadwal ditolak | `PASS` |
| R7b | Ganti poliklinik ke Poli Penyakit Dalam: jadwal dr. Bagus tampil | `PASS` |
| R8 | Verifikasi: poliklinik, dokter, walk-in | `PASS` |
| R9 | `POST /admin` `200`: `encounterType 1`, walk-in, `visitType 2`, tunai, `doctorScheduleId` terisi | `PASS` |
| R10 | Layar selesai: slip Rawat Jalan, No. Kunjungan `ENC-RSMMC-00206`, No. Antrean `I002` | `PASS` |
| R11 | Tanggal 7 Okt: info appointment, hanya jadwal Rabu (dr. Rendy 08:00–12:00) | `PASS` |
| R14 | Penolakan backend (kunjungan aktif `ENC-RSMMC-00206`) tampil di Verifikasi | `PASS` |
| R12 | Sesudah kunjungan lama dibatalkan, kirim ulang: `200`, `visitDate 2026-10-07`, appointment, `registrationSource 3` | `PASS` |
| R13 | Pendaftaran IGD tetap berlabel IGD | `PASS` |
| R15 | Asuransi: `paymentType 2`, `patientInsuranceId` terisi | `PASS` |
| R16 | Penjamin perusahaan: `paymentType 3`, `patientCompanyGuarantorId` terisi; verifikasi "Corporate" | `PASS` (percobaan pertama memilih penjamin `KSKTEST-EMP-02` yang kedaluwarsa 29 Sep 2026; backend menolak dan pesannya tampil) |
| R17–R18 | Pasien baru lewat Input Manual tersimpan, lalu `POST /admin` `200`, `isNewPatient true` | `PASS` |

Total `28` cek `PASS` (R3b pada skrip hanya penanda, tidak dihitung).

Basis data sesudah uji: kunjungan `ENC-RSMMC-00205`..`00210` (`EncounterType 1`, `ClinicId` dan
`DoctorScheduleId` terisi, antrean terbentuk) seluruhnya dibatalkan lewat
`PATCH /patient-encounters/admin/{id}/cancel`.

## 4. Acceptance criteria

| AC | Bukti | Status |
| --- | --- | --- |
| 1. Pasien lama dan baru; encounter Outpatient berklinik dengan antrean | R3, R9, R10, R17, R18; DB | ✅ |
| 2. Jadwal wajib hanya bila `IsDoctorRequired` | R7; `validateVisit` | ✅ (seluruh poliklinik di DB uji `IsDoctorRequired = true`; cabang opsional hanya terbukti lewat kode) |
| 3. Hari ini walk-in, mendatang appointment | R9, R11, R12 | ✅ |
| 4. Tunai, asuransi, penjamin perusahaan | R9, R15, R16 | ✅ |
| 5. Penolakan backend tampil di Verifikasi | R14, R16 | ✅ |
| 6. Pendaftaran IGD tidak berubah | R13; default prop = teks lama; ESLint/build | ✅ |

## 5. Data uji tersisa

Pasien `00-00-00-17` "KSKTEST FE013 Pasien Baru" tetap ada di master pasien (R17), beserta berkas QR yang dibuat backend di `Storage/uploads/patient-qrcodes/00-00-00-17/` (untracked). Tidak ada endpoint
penghapusan yang dipakai; hapus manual bila tidak diinginkan.

## 6. Risiko tersisa

- Helper `toNullableGuid` modul IGD menolak GUID yang bukan RFC 4122 (contoh ID jadwal seed
  `…-6c40-…`) dan mengganti nilainya dengan kosong tanpa galat. RJ memakai pemeriksa sendiri. IGD tidak
  diubah karena di luar wewenang task, tetapi berpotensi kena untuk penjamin dengan ID seperti itu.
- IGD mengirim `visitDate` sebagai `new Date().toISOString()` (UTC). Sebelum pukul 07.00 WIB tanggalnya
  bergeser ke hari sebelumnya. RJ mengirim tanggal tanpa jam. IGD tidak diubah.
- Endpoint `GET /doctor-schedules/admin/options` tidak membawa masa berlaku jadwal; konsumen lain yang
  memakainya dapat menampilkan jadwal kedaluwarsa.
- Kuota jadwal tidak ditampilkan; jadwal penuh baru diketahui saat submit (pesan backend tampil).

## 7. Revisi 6 Okt 2026 — permintaan pemilik (`RJ-DOC-DEC-039`)

Permintaan: poliklinik pada Data Kunjungan hanya yang tersedia pada hari itu dan tanpa kode; tombol
tanpa ikon; tombol kembali bergaris warna primary. Pemilik memilih cakupan **Rawat Jalan saja**.

| Berkas | Perubahan |
| --- | --- |
| `outpatient-registration.service.js` | `fetchOutpatientDoctorSchedules` memuat seluruh jadwal aktif semua poliklinik (berhalaman, `pageSize` 100, batas 20 halaman), tidak lagi per poliklinik |
| `use-outpatient-visit-options.js` | Poliklinik dan jadwal dimuat sekali; poliklinik yang ditawarkan = yang punya jadwal berlaku pada tanggal kunjungan; jadwal per poliklinik disaring dari data yang sama. Satu keadaan memuat/gagal + Coba lagi |
| `use-outpatient-registration.js` | Poliklinik yang tidak buka pada tanggal baru dilepas otomatis, begitu juga jadwalnya |
| `outpatient-registration.utils.js` | Label poliklinik = nama saja |
| `outpatient-visit-step.jsx` | Pilihan poliklinik nonaktif + peringatan "Tidak ada poliklinik yang buka pada …" bila kosong; tombol Kembali tanpa ikon, bergaris primary |
| `outpatient-verification-step.jsx` | Tombol Kembali tanpa ikon, bergaris primary |
| `outpatient-registration-page.jsx` | Mengirim `showButtonIcons={false}` dan `backButtonClassName` ke langkah bersama |
| `style/.../outpatient-registration/outpatient-registration.module.css` | Baru: `button.backButton { border-color: var(--color-primary) }` (token, tanpa `!important`) |
| `patient-selection-step.jsx`, `payment-method-step.jsx`, `registration-success-step.jsx` | Prop opsional `showButtonIcons` (default `true`) dan `backButtonClassName` (default kosong). Default = tampilan IGD |

Ikon yang dihapus di Rawat Jalan: panah `<` pada tombol kembali, `←` pada "Kembali ke Pencarian",
`+` pada "Daftarkan Penjamin Baru", ikon printer pada "Cetak Slip". Tidak diubah: kartu pilihan
(jenis pasien, jadwal, jenis kunjungan, metode pembayaran) beserta tanda centangnya, karena itu
penanda pilihan, bukan ikon tombol; serta tombol tutup `×` yang hanya berisi ikon.

`UI GATE` revisi: tombol kembali — EXTEND (prop opsional pada 3 komponen IGD, default tidak berubah)
plus satu class CSS module RJ; tidak ada komponen baru.

**Validasi:** ESLint `0 error, 0 warning`; `npm run build` `PASS`. Uji layar `ui_fe013_r2.cjs` terhadap
server dev pemilik (`localhost:3000`, backend `7184`, `QuilvianNewDevSukma`) `20/20 PASS`:

| ID | Skenario | Hasil |
| --- | --- | --- |
| C1 | Selasa 6 Okt: pilihan = Klinik Fisioterapi, Poli Bedah, Poli Jantung, Poli Kulit dan Kelamin, Poli Penyakit Dalam, Poli THT (sesuai DB) | `PASS` |
| C2 | Label tanpa kode | `PASS` |
| C3–C4 | Satu request jadwal tanpa `clinicId`; ganti ke 7 Okt mengganti daftar (Poli Anak, Gigi, Kandungan, Saraf, Umum) tanpa request baru | `PASS` |
| C5–C6 | Poli Umum 7 Okt memuat dr. Rendy; kembali ke 6 Okt, Poli Umum dilepas otomatis | `PASS` |
| B1–B11 | Langkah 1–5: tidak ada tombol aksi berikon; seluruh tombol kembali (`Kembali`, `Kembali ke Pilih Jenis Pasien`, `Kembali ke Pencarian`, `Cari Pasien Lain`) bergaris `rgb(8, 155, 171)` | `PASS` |
| S1–S2 | Pendaftaran tetap berhasil (`ENC-RSMMC-00212`), lalu dibatalkan | `PASS` |
| I1 | IGD: tombol kembali tetap berikon `<` | `PASS` |

Data uji: `ENC-RSMMC-00212` dibatalkan lewat endpoint aplikasi.

