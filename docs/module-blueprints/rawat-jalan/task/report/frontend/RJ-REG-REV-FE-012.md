# Laporan Perubahan Frontend — `RJ-REG-REV-FE-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-012` (ID usulan; belum tercatat di roadmap) |
| Judul | Pendaftaran Rawat Jalan: tiga kategori pasien dan step Penjamin kontekstual, step Metode Pembayaran dihapus |
| Slice | Pendaftaran Pasien Rawat Jalan oleh petugas |
| Roadmap | Belum ada baris roadmap. Atas pilihan pemilik (Sukma, 27 Okt 2026) laporan ditulis tanpa mengubah roadmap |
| Trace | Prompt pemilik 27 Okt 2026 (aturan A–F). Membalik sebagian `RJ-DOC-DEC-034` (urutan wizard ala IGD dengan Metode Pembayaran) dan memperluas `RJ-REG-DEC-011` (step Jenis Kunjungan). Keputusan pemilik: step **Penjamin** kontekstual; BPJS hanya dari kartu tersimpan |
| Contract version | Kontrak backend saat ini: `POST /patient-encounters/admin`, `POST /lab-patient-registrations/external-referral`, `EncounterPaymentType` 1/2/3. Tidak ada kontrak baru |
| Wewenang UI | Step Jenis Kunjungan, Metode Pembayaran, Verifikasi, dan Selesai Pendaftaran RJ. `PaymentMethodStep` IGD hanya ditambah prop opsional |
| Dependency | Tidak ada task backend |
| Klasifikasi | `HEAVY` — wizard berubah, komponen bersama IGD diperluas, payload kunjungan berubah |
| Task mode | `FRONTEND` (backend hanya dibaca; satu-satunya tulisan di backend adalah laporan ini) |
| Target tulis | `V2QuilvianSystemFrontendDev` (`src/`, `tests/unit/`) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `3e0aa3d8a637891429a93b4875ac34eda1a29c87` (`sukmagpV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `380b992c74510284a92e8afc868b66f20783df44` (`sukmagp`) |
| Tanggal | 27 Oktober 2026 |
| Status | 🟡 Sebagian — kode dan uji unit selesai; `npm run build` dan uji layar **tidak dijalankan** (dilarang oleh prompt) |

---

## 1. Keadaan yang ditemukan di awal

- Bar step: Cari Pasien → Jenis Kunjungan (Umum / Rujukan-Tindak Lanjut) → Data Kunjungan → (Data Rujukan) → **Metode Pembayaran** → General Consent → Verifikasi → Selesai.
- `paymentCategory` bawaan `CASH` (dari konstanta IGD dan slice RJ), dan `buildOutpatientEncounterPayload` serta `buildLabReferralRegistrationPayload` memakai `|| PAYMENT_CATEGORY.CASH`. Penjamin yang tidak dipilih terkirim sebagai Tunai.
- Rujukan selalu dipetakan ke `VisitType = 2` (`RJ-DOC-DEC-036`) dengan `isReferral` dan blok `referral`.

---

## 2. Proses bisnis dari sisi pengguna

Petugas memilih pasien, lalu **Jenis Pasien**: Pasien Pribadi, Pasien Asuransi, atau Pasien Rujukan. Tidak ada pilihan otomatis.

| Kategori | Bar step | Penjamin kunjungan |
| --- | --- | --- |
| Pasien Pribadi | Pasien → Jenis Pasien → Data Kunjungan → (Consent) → Verifikasi → Selesai | Tunai (`PaymentType = 1`), tampil "Pribadi / Tunai" |
| Pasien Asuransi | Pasien → Jenis Pasien → **Penjamin** → Data Kunjungan → (Consent) → Verifikasi → Selesai | Kartu asuransi = 2; penjamin perusahaan/Benefit Karyawan = 3 |
| Pasien Rujukan | Pasien → Jenis Pasien → Data Kunjungan (tanggal) → Data Rujukan → **Penjamin** → (Consent) → Verifikasi → Selesai | Sumber dipilih: BPJS Kesehatan / JKN (2), Asuransi Swasta (2), Penjamin Perusahaan (3), Pribadi / Tunai (1) |

Contoh: pasien rujukan dari puskesmas yang memakai BPJS → pilih **Pasien Rujukan**, isi Data Rujukan, lalu pada step Penjamin pilih **BPJS Kesehatan / JKN**. Tabel hanya memuat kartu yang namanya memuat BPJS/JKN/INA-CBG/KIS (`BKC-DEC-121`). Tampil catatan bahwa verifikasi kepesertaan dan SEP belum terhubung ke sistem. Bila kartu belum ada, petugas dapat **Daftarkan Penjamin Baru** (verifikasi manual oleh petugas); server tetap memeriksa kelayakan dan masa berlaku.

Jalur tidak normal:

- Kartu tidak aktif, belum layak, belum berlaku, atau kedaluwarsa pada tanggal kunjungan ditolak dengan pesan yang jelas.
- Kartu yang tidak sesuai sumber (mis. kartu swasta pada sumber BPJS) ditolak: "Kartu yang dipilih tidak sesuai dengan jenis penjamin rujukan".
- Rujukan Laboratorium: sumber Penjamin Perusahaan nonaktif (`RJ-VAL-PM-15`).
- Mengganti kategori dari Rujukan yang berisi data memunculkan dialog "Ganti kategori pasien?".
- Verifikasi menampilkan Jenis Pasien, Penjamin Rujukan, Jenis Penjamin, Penjamin, No. Kartu, dan Berlaku s.d.; tombol **Selesaikan Pendaftaran** nonaktif selama penjamin belum jelas.
- Klik beruntun pada Selesaikan Pendaftaran tidak mengirim dua kali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Hook, page, step, utils, constants, dan slice Pendaftaran RJ; `PaymentMethodStep` beserta pemakainya (`emergency-registration-page.jsx`); `PatientEncounterController` (validasi Tunai/Asuransi/Perusahaan, `PaymentMethodId` opsional untuk Tunai); `EncounterPaymentSourceService` (`FinancingChoice ?? "GENERAL_SELF_PAY"`); `PatientInsuranceResponse.ProviderType`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/registration-management/outpatient-registration/outpatient-registration.constants.js` | Step `PAYMENT` → `PAYER` (nilai tetap 3, judul "Penjamin"); tiga bar step per kategori; nilai bawaan `registrationCategory`, `referralPayerSource`, `paymentCategory = ""` |
| `src/utils/health-services/registration-management/outpatient-registration/outpatient-registration.utils.js` | `resolveExplicitOutpatientPaymentType`, `resolveOutpatientPayer`, `getOutpatientVisitTypeForCategory`; payload tanpa `|| CASH` |
| `src/utils/health-services/registration-management/outpatient-registration/outpatient-referral.utils.js` | Bar step per kategori; payload Lab tanpa `|| CASH` |
| `src/lib/state/slice/health-services/registration-management/outpatient-registration-slice.jsx` | `paymentCategory` awal kosong; nilai tidak dikenal tidak menjadi Tunai |
| `src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-registration.js` | Kategori, sumber penjamin rujukan, navigasi berdasarkan bar step, `handlePayerStepNext`, guard kirim ganda |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-visit-type-step.jsx` | Tiga kartu kategori; konfirmasi keluar Rujukan |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-payer-step.jsx` | **Baru** — WRAP di atas `PaymentMethodStep`: tanpa kartu Tunai/Asuransi, pilihan sumber penjamin rujukan, filter BPJS |
| `src/components/view/health-services/registration-management/emergency-registration/payment-method-step.jsx` | Prop opsional `title`, `showPaymentCategoryCards`, `allowSelfPayFinancing`, `showWorkforceFinancing`, `showPayerSection`, `payerFilter`, `cashSummaryTitle/Description`, `actionMessageTitle`, `children`. Nilai bawaan = tampilan IGD |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-registration-page.jsx` | Step Penjamin, label step |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-verification-step.jsx` | Ringkasan Penjamin; kirim hanya bila penjamin terselesaikan |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-visit-step.jsx`, `outpatient-referral-step.jsx` | Label tombol lanjut |
| `src/utils/shared/registration-category-utils.js`, `tests/unit/registration-category-payer.test.mjs` | **Baru**, bersama Kiosk (lihat `FE-KSK-017`) |

### 3.3 Kepatuhan arsitektur frontend

Aturan penjamin berupa fungsi murni di `utils`; hook mengatur form dan Redux; view hanya merender. `PaymentMethodStep` diperluas lewat prop opsional, bukan disalin.

**UI GATE: 5 elemen — REUSE 3, EXTEND 1, COMPOSE 0, WRAP 1, NEW 0**

| Kebutuhan UI | Kandidat | Status | Keputusan |
| --- | --- | --- | --- |
| Tiga kartu kategori | `visitTypeCard` / `visitTypeChoiceGrid` | REUSE | Tetap |
| Step Penjamin | `PaymentMethodStep` IGD | EXTEND | Prop opsional; IGD tidak terdampak |
| Pembungkus step Penjamin RJ | `OutpatientPayerStep` | WRAP | Konfigurasi khusus RJ (sumber rujukan, filter BPJS) |
| Pilihan sumber penjamin rujukan | `visitTypeCard` | REUSE | Tetap |
| Konfirmasi keluar Rujukan | `ConfirmModal` | REUSE | Tetap |

Pilihan untuk baris EXTEND/WRAP:

- **A. Extend `PaymentMethodStep` dengan prop opsional + pembungkus RJ — Rekomendasi, dipilih.** Panel Benefit Karyawan, scan kartu, Edit Penjamin, dan modal daftar penjamin ikut terpakai; nilai bawaan menjaga IGD.
- **B. Komponen penjamin RJ baru.** Menduplikasi ±700 baris logika Benefit Karyawan dan kartu; risiko menyimpang dari IGD. Perlu persetujuan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel penjamin dalam keadaan memuat; tombol Jenis Pasien "Memuat..." saat memuat penjamin |
| Kosong | Tabel penjamin kosong; untuk BPJS: catatan verifikasi + tombol Daftarkan Penjamin Baru |
| Gagal | "Penjamin gagal dimuat" / "Penjamin belum valid" / galat backend di Verifikasi |
| Tanpa hak akses | Tidak berubah (`PatientEncounter : Create`; Edit Penjamin `PatientInsurance : Update`) |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru.

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/patient-encounters/admin` | Membuat kunjungan; `paymentType` selalu eksplisit, perusahaan = 3 + `patientCompanyGuarantorId` | `PatientEncounter : Create` |

#### Health Services / Patient Management / Master Data / Patient Insurance

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`/`POST` | `/v1/health-services/patient-management/master-data/patient-insurances` (alur yang ada) | Daftar dan pendaftaran kartu asuransi/BPJS pasien | Tidak berubah |

#### Health Services / Patient Management / Master Data / Patient Company Guarantor

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`/`POST` | `/v1/health-services/patient-management/master-data/patient-company-guarantors` (alur yang ada) | Kartu perusahaan pasien | Tidak berubah |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` berkas berubah | 0 error; tidak ada warning baru dibanding `HEAD` | `PASS` | Keluaran perintah |
| `npx eslint --rule no-undef` berkas berubah | 0 temuan | `PASS` | Keluaran perintah |
| `node --test tests/unit/registration-category-payer.test.mjs` | 16/16 lulus (bar step, payload 1/2/3, resolusi penjamin, `VisitType` tetap) | `PASS` | Keluaran perintah |
| Uji unit IGD/RJ terkait (`emergency-registration-*`, `outpatient-referral-revision`, `outpatient-membership-label`, `patient-workforce-status`) | Lulus | `PASS` | Keluaran perintah |
| Seluruh `tests/unit` | 3803/3845; 42 gagal di modul yang tidak disentuh | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |

Uji manual: `NOT FEASIBLE` — prompt melarang `npm run dev`/`npm run build`.

**Tidak dijalankan:** `npm run build`, `npm run dev`, uji browser, `dotnet build`, migration — dilarang oleh prompt.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tiga kategori di Pendaftaran RJ | Terpenuhi (statis) | `outpatient-visit-type-step.jsx` |
| Step Metode Pembayaran tidak tampil | Terpenuhi | Uji "step Metode Pembayaran tidak ada" |
| Pribadi = Tunai tanpa step tambahan | Terpenuhi | Bar step Pribadi tanpa `PAYER`; uji resolusi |
| Asuransi = 2, perusahaan = 3 | Terpenuhi | Uji payload `paymentType = 3` |
| Rujukan menyimpan data rujukan; penjamin dipilih tersendiri | Terpenuhi (statis) | `buildOutpatientReferralCreateFields` tetap; step Penjamin sesudah Data Rujukan |
| Tanpa penjamin tidak terkirim sebagai Tunai | Terpenuhi | Uji payload melempar galat |
| `VisitType` tidak diganti kategori penjamin | Terpenuhi | Uji `getOutpatientVisitTypeForCategory` |
| IGD tidak berubah | Terpenuhi (statis) | Prop bawaan `PaymentMethodStep`; uji IGD lulus |
| `npm run build` dan uji layar | **Belum terpenuhi** | Dilarang oleh prompt |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | (1) Belum ada integrasi VClaim/SEP/kepesertaan BPJS. (2) `RJ-DOC-DEC-034` perlu dicatat ulang oleh pemilik sebagai amandemen |
| Dependency backend | Tidak ada. `PaymentType` bawaan Tunai di DTO tetap; frontend selalu mengirim nilai eksplisit |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `FE-KSK-017` §8 (satu working tree untuk kedua task) |
| Langkah berikutnya | Jalankan `npm run build`, lalu uji layar: Pribadi, Asuransi (asuransi, perusahaan, Benefit Karyawan), Rujukan (BPJS, swasta, perusahaan, pribadi, Laboratorium), ganti kategori, dan klik ganda Selesaikan Pendaftaran; pastikan Pendaftaran IGD tetap menampilkan Metode Pembayaran |
