# Laporan Perubahan Frontend — `FE-KSK-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-017` (ID usulan; belum tercatat di roadmap) |
| Judul | Tiga kategori pasien Kiosk dan penjamin kontekstual, step Metode Pembayaran dihapus |
| Slice | Kiosk Pasien Lama dan Kiosk Pasien Baru |
| Roadmap | Belum ada baris roadmap. Atas pilihan pemilik (Sukma, 27 Okt 2026) laporan ditulis tanpa mengubah roadmap |
| Trace | Prompt pemilik 27 Okt 2026 (aturan A–F). Mengubah perilaku `FE-KSK-008` (penjamin utama), `FE-KSK-010` (masa berlaku), `FE-KSK-014` (Data Rujukan), `FE-KSK-016` (scan kartu). Keputusan pemilik: step **Penjamin** kontekstual; BPJS hanya dari kartu tersimpan |
| Contract version | Kontrak backend saat ini (`PatientEncounterCreateRequest`, `EncounterPaymentType` 1/2/3, `RWI-ENC-PAYER-001 1.1.0`). Tidak ada kontrak baru |
| Wewenang UI | Ubah step Jenis Pasien dan step Pembayaran di dua alur Kiosk; tidak mengubah alur lain |
| Dependency | Tidak ada task backend. Route kiosk sudah menerima Penjamin Perusahaan (`BE-KSK-003` ✅) |
| Klasifikasi | `HEAVY` — 2 alur, 1 helper bersama baru, payload kunjungan berubah |
| Task mode | `FRONTEND` (backend hanya dibaca; satu-satunya tulisan di backend adalah laporan ini) |
| Target tulis | `V2QuilvianSystemFrontendDev` (`src/`, `tests/unit/`) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `3e0aa3d8a637891429a93b4875ac34eda1a29c87` (`sukmagpV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `380b992c74510284a92e8afc868b66f20783df44` (`sukmagp`) |
| Tanggal | 27 Oktober 2026 |
| Status | 🟡 Sebagian — kode dan uji unit selesai; `npm run build` dan uji layar akun Kiosk **tidak dijalankan** (dilarang oleh prompt) |

---

## 1. Keadaan yang ditemukan di awal

- Step Jenis Pasien hanya punya **Pasien Umum** dan **Pasien Rujukan**, lalu selalu disusul step **Pembayaran** (Tunai/Asuransi).
- Pasien Lama mulai dengan `paymentKey = "cash"`, dan `createOldPatientEncounter` memakai `paymentKey || "cash"`. Backend juga memakai Tunai sebagai nilai bawaan `PaymentType`, sehingga penjamin yang kosong tersimpan diam-diam sebagai Tunai.
- Pasien Baru tidak dapat memakai perusahaan sebagai penjamin kunjungan: service melempar galat "Backend … membutuhkan PatientInsuranceId", padahal route kiosk sudah menerima `PaymentType = 3` sejak `BE-KSK-003`.
- **Regresi merge** di `use-kiosk-old-patient-registration.jsx` (commit `77c4eafc7`): `OLD_PATIENT_LAB_MODES`, step `labAttendance`, dan bar step Laboratorium hilang. Memilih Laboratorium memicu `ReferenceError` sesudah sesi kiosk dibentuk, dan jalur Lab tanpa pesanan dikirim ke Layanan & Dokter. Terbukti dengan `eslint --rule no-undef` pada versi `HEAD` (baris 445, 455, 506).

---

## 2. Proses bisnis dari sisi pengguna

Pasien memilih salah satu dari tiga kartu. Tidak ada kartu yang terpilih otomatis.

| Kategori | Pasien Lama | Pasien Baru | Penjamin kunjungan |
| --- | --- | --- | --- |
| **Pasien Pribadi** | Jenis Pasien → Layanan & Dokter (Lab: → Konfirmasi) | Jenis Pasien → Data Pasien → Layanan & Dokter | Tunai (`PaymentType = 1`), tampil "Pribadi / Tunai"; cara bayar dipilih kasir |
| **Pasien Asuransi** | Jenis Pasien → **Penjamin** → Layanan & Dokter | Jenis Pasien → Data Pasien → **Penjamin** → Layanan | Asuransi = 2, perusahaan = 3 |
| **Pasien Rujukan** | Jenis Pasien → Data Rujukan → **Penjamin** → Layanan | Jenis Pasien → Data Pasien → Data Rujukan → **Penjamin** → Layanan | Sesuai sumber yang dipilih |

Pada step Penjamin Pasien Rujukan, pasien memilih sumber lebih dulu:

| Sumber | Yang terjadi |
| --- | --- |
| BPJS Kesehatan / JKN | Pasien Lama: hanya kartu asuransi tersimpan yang namanya memuat BPJS/JKN/INA-CBG/KIS (aturan `BKC-DEC-121`). Tidak ada tombol Tambah. Bila tidak ada kartu: "Belum ada kartu BPJS … Silakan ke petugas pendaftaran", lanjut ditolak. Pasien Baru: selalu tertahan dengan pesan yang sama |
| Asuransi Swasta | Kartu asuransi non-BPJS; boleh tambah kartu baru dari master |
| Penjamin Perusahaan | Kartu perusahaan; `PaymentType = 3` |
| Pribadi / Tunai | Tunai; status dan data rujukan tetap terkirim |

Jalur tidak normal:

- Kartu tidak aktif, belum dinyatakan layak, belum berlaku, atau kedaluwarsa tampil dengan tombol **Tidak Dapat Dipakai** dan pesan penolakan.
- Memilih penyedia BPJS dari master di Kiosk ditolak: "Kartu BPJS Kesehatan / JKN tidak dapat didaftarkan dari Kiosk…".
- Mengganti kategori dari Rujukan yang sudah berisi data membuka dialog **"Ganti kategori pasien?"**; Batal mempertahankan data.
- Konfirmasi menampilkan Jenis Pasien, Jenis Penjamin, dan Penjamin. Bila penjamin belum jelas, tombol simpan nonaktif dan catatan footer menjelaskan alasannya.
- Jalur Laboratorium: kartu Pasien Rujukan nonaktif ("Pasien rujukan Laboratorium belum dapat didaftarkan dari kiosk…"), karena blok rujukan Kiosk selalu menuju poliklinik (`targetUnitType = 1`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Hook, view, step, service, helper, dan constant Kiosk Pasien Lama/Baru; `PatientEncounterController` (`ValidateCreateRequestAsync`, `LoadValidPatientInsuranceAsync`, `LoadValidPatientCompanyGuarantorAsync`, metadata enum); `EncounterPaymentType`, `VisitType`, `PatientEncounterGuarantorType`, `MstInsuranceProvider`, `MstPatientInsurance`, `AdministrationFeeCalculationService.IsPackageSystemGuarantor`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/shared/registration-category-utils.js` | **Baru**, dipakai bersama Kiosk dan RJ — kategori, sumber penjamin rujukan, label, pesan, `isBpjsPayer`, kelayakan kartu |
| `src/lib/helpers/kiosk/registration/kiosk-registration-payer.helpers.js` | **Baru** — `resolveOldPatientPayer`, `resolveNewPatientPayer`, penjamin utama Pasien Baru |
| `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx` | Kategori, step `payer`, bar step dinamis, konfirmasi keluar Rujukan, metadata enum dimuat di hook. **Konstanta Lab yang hilang dipulihkan** |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-type.jsx` | Tiga kartu; tanpa pilihan otomatis |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-payment.jsx` | Layar Tunai/Asuransi dihapus; mode `insurance`/`referral`, sumber penjamin, filter BPJS, tolak kartu tidak layak |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-view.jsx` | Step Penjamin, dialog konfirmasi, header/footer |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-confirm.jsx` | Ringkasan penjamin; simpan hanya bila penjamin terselesaikan |
| `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js` | `buildOldPatientEncounterEnumMaps`; `createOldPatientEncounter` tanpa `|| "cash"` |
| `src/lib/hooks/kiosk/registration/use-kiosk-new-patient-registration.jsx` | Kategori, step `payer`, `getNewPatientStepItems`, sumber penjamin rujukan, guard kirim ganda |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-type.jsx` | Tiga kartu |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-payment.jsx` | Layar Tunai/Asuransi dihapus; perusahaan boleh jadi penjamin kunjungan; tolak BPJS |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-view.jsx` | Step Penjamin, navigasi kembali per kategori, dialog konfirmasi, master pasien tidak dibuat bila penjamin belum jelas |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-confirm.jsx` | Ringkasan penjamin; "Asal Rujukan" membaca `referralInstitutionName` |
| `src/lib/helpers/kiosk/registration/kiosk-new-patient-submit.helpers.jsx` | Draf wajib `paymentTypeKey` eksplisit |
| `src/lib/services/kiosk/registration/kiosk-new-patient-registration.service.js` | `company` → `PaymentType = 3` + `patientCompanyGuarantorId` |
| `src/lib/constants/kiosk/registration/kiosk-new-patient-payment.constants.js`, `kiosk-new-patient-registration.constants.js`, `kiosk-old-patient-service-target.constants.js` | Opsi Tunai/Asuransi dan `STEP_ITEMS` dihapus; `referralPayerSource`; pesan Lab |
| `src/components/view/kiosk/registration/shared/kiosk-referral-step.jsx` | Label "Lanjut Pilih Penjamin" |
| `src/utils/kiosk/registration/kiosk-referral-utils.jsx` | `hasKioskReferralData` |
| `src/style/kiosk/registration/kiosk-old-patient-view.module.css`, `kiosk-new-patient-view.module.css` | Grid 3 dan 4 kartu, aksen Asuransi; token saja |
| `tests/unit/registration-category-payer.test.mjs` | **Baru** — 16 uji aturan penjamin (bersama RJ) |

### 3.3 Kepatuhan arsitektur frontend

Aturan penjamin berupa fungsi murni di `utils/shared` dan `lib/helpers/kiosk`, mengikuti preseden `utils/shared/insurance-card-scan-utils`. View tidak memanggil API langsung. Tidak ada Axios instance, slice, atau base component baru.

**UI GATE: 5 elemen — REUSE 3, EXTEND 1, COMPOSE 1, WRAP 0, NEW 0**

| Kebutuhan UI | Kandidat | Status | Keputusan |
| --- | --- | --- | --- |
| Tiga kartu kategori | Kartu Jenis Pasien Kiosk | EXTEND | Kelas varian `isOldPatientTypeInsurance`/`isRegistrationTypeInsurance` dan grid 3 kolom; kartu lama tidak berubah |
| Empat kartu sumber penjamin rujukan | Kartu Pembayaran Kiosk | COMPOSE | Kartu yang ada + grid 4 kolom |
| Daftar dan editor kartu penjamin | Workspace penjamin `FE-KSK-008` | REUSE | Tetap |
| Konfirmasi keluar dari Rujukan | `ConfirmModal` | REUSE | Sama dengan Pendaftaran RJ |
| Pemberitahuan BPJS | `infoAlert` / pesan inline Kiosk | REUSE | Tetap |

Pilihan untuk baris EXTEND (sudah diputuskan pemilik lewat pertanyaan "Posisi UI"):

- **A. Step Penjamin kontekstual + varian kartu — Rekomendasi, dipilih.** Workspace penjamin Kiosk dipakai ulang; risiko regresi rendah.
- **B. Panel penjamin tertanam di bawah kartu kategori.** Layar sentuh menjadi padat; perubahan komponen lebih besar.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat data penjamin pasien..." |
| Kosong | "Belum ada asuransi tersimpan" / "Belum ada kartu BPJS Kesehatan / JKN" + arahan ke petugas |
| Gagal | "Data penjamin pasien tidak dapat dimuat." / pesan backend di Konfirmasi |
| Tanpa hak akses | Tidak berubah: non-akun Kiosk dialihkan seperti sebelumnya |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru; route lama dipakai dengan isi `paymentType` yang kini selalu eksplisit.

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/registration-management/patient-encounters/filters/metadata` | Enum `PaymentType` (1/2/3) | `KioskRead` |
| `POST` | `/v1/health-services/registration-management/patient-encounters` dan `/kiosk` | Membuat kunjungan | `KioskRead` |

#### Health Services / Patient Management / Master Data / Patient Insurance

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patient-insurances/options` | Kartu asuransi pasien (termasuk BPJS) | Sesuai controller, tidak berubah |

#### Health Services / Patient Management / Master Data / Patient Company Guarantor

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patient-company-guarantors/options` | Kartu perusahaan pasien | Sesuai controller, tidak berubah |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 31 berkas berubah + 4 berkas baru | 0 error; warning per berkas sama atau lebih sedikit dari `HEAD` (dibandingkan lewat `--stdin`) | `PASS` | Keluaran perintah |
| `npx eslint --rule no-undef` berkas berubah | 0 temuan; aturan yang sama menemukan regresi Lab di `HEAD` | `PASS` | Keluaran perintah |
| `node --test tests/unit/registration-category-payer.test.mjs` | 16/16 lulus | `PASS` | Keluaran perintah |
| Uji unit berkas yang mengimpor modul berubah (11 berkas) | 127/128; 1 gagal `FE-CRM-007` membaca `new-patient-form.jsx` yang tidak diubah | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| Seluruh `tests/unit` | 3803/3845; 42 gagal, semuanya di modul yang tidak disentuh | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npm run test:unit` | Glob tidak di-expand di shell Windows ini | `EXISTING / ENVIRONMENT ISSUE` | "Could not find '…\*.test.mjs'" |
| Grep anti-regresi CSS (warna literal, typography, `!important`) pada baris baru | Kosong | `PASS` | Keluaran perintah |

Uji manual: `NOT FEASIBLE` — prompt melarang `npm run dev`/`npm run build`, dan uji layar butuh akun perangkat Kiosk.

**Tidak dijalankan:** `npm run build`, `npm run dev`, uji browser, `dotnet build`, migration — dilarang oleh prompt.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tiga kategori di Kiosk Pasien Lama dan Pasien Baru | Terpenuhi (statis) | `kiosk-*-step-type.jsx` |
| Step Metode Pembayaran tidak tampil | Terpenuhi (statis) | `getOldPatientStepItems`, `getNewPatientStepItems` |
| Pribadi = Tunai tanpa step tambahan | Terpenuhi | Uji "Pasien Pribadi = Tunai" |
| Asuransi = 2, perusahaan = 3 | Terpenuhi | Uji "penjamin perusahaan tetap 'company'", service Pasien Baru |
| Rujukan menyimpan data rujukan dan memilih penjamin sendiri | Terpenuhi (statis) | `isReferral` dari kategori; step Penjamin sesudah Data Rujukan |
| Rujukan tanpa penjamin tidak terkirim sebagai Tunai | Terpenuhi | Uji `createOldPatientEncounter` menolak sebelum POST |
| Enum dan `VisitType` backend tidak berubah | Terpenuhi | Tidak ada perubahan backend |
| Jalur Lab, consent, antrean tidak rusak | Terpenuhi (statis) + regresi Lab dipulihkan | Bar step Lab, Konfirmasi Lab tanpa Layanan |
| `npm run build` dan uji browser | **Belum terpenuhi** | Dilarang oleh prompt |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning ESLint lama tetap (`react-hooks/*`, `no-img-element`); tidak ada warning baru |
| Masalah yang diketahui | (1) Belum ada integrasi VClaim/SEP/kepesertaan BPJS; BPJS dikenali dari nama penyedia. (2) Kiosk tetap membuat kartu asuransi swasta baru dengan `isEligible: true` (perilaku `FE-KSK-010`); keputusan kebijakan terpisah. (3) Rujukan Laboratorium dari Kiosk belum didukung |
| Dependency backend | Tidak ada. `PatientEncounterCreateRequest.PaymentType` tetap bernilai bawaan Tunai; frontend selalu mengirim nilai eksplisit |
| Perubahan sampingan | Akhir baris berkas yang ditulis ulang dikembalikan ke CRLF agar sama dengan working tree |
| Interupsi | `NONE` |
| Status Git | 31 berkas `M` di `src/`, 4 berkas baru (`registration-category-utils.js`, `kiosk-registration-payer.helpers.js`, `outpatient-payer-step.jsx`, `registration-category-payer.test.mjs`); backend: hanya laporan ini dan `RJ-REG-REV-FE-012.md` |
| Langkah berikutnya | Jalankan `npm run build` lalu uji layar dengan akun perangkat Kiosk: Pribadi, Asuransi (asuransi dan perusahaan), Rujukan (BPJS, swasta, perusahaan, pribadi), jalur Lab, dan dialog ganti kategori |
