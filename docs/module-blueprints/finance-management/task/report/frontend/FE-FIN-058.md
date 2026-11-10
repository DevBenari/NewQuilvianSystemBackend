# Laporan Perubahan Frontend — `FE-FIN-058`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-058` |
| Judul | Petugas Registrasi Dapat Mendaftarkan Kartu Benefit Karyawan RS dengan Memilih Penerima dari HR, dan Bila Gagal Melihat Alasan Sebenarnya |
| Slice | `REV-23` (Benefit Management — Sinkronisasi Kartu Pasien & Lifecycle Benefit) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "REVISI ROADMAP 23 (3 November 2026)", tabel Task REV-23 Frontend |
| Trace | `FR-FIN-269`; `FIN-DEC-245`; `SCR-PAT-REG-01`; `SCR-EMG-REG-01`; `FIN-API-1.12` §`V.3` |
| Layar | `SCR-PAT-REG-01` (Registrasi Rawat Jalan — Pemilihan Penjamin) & `SCR-EMG-REG-01` (Registrasi Gawat Darurat/IGD — Metode Pembayaran & Penjamin) |
| Contract Version | `FIN-API-1.12` §`V.3` (`POST api/v1/health-services/patient-management/master-data/patient-company-guarantors/admin` / `.../patient-company-guarantors`) |
| Wewenang UI | `REUSE` — menggunakan komponen dasar (@/components/features/base-features/base-form-control, BaseButton, InformationAlert, StatusBadge) serta section teruji `PatientCompanyGuarantorInternalBenefitSection` tanpa token/styling visual hardcoded baru |
| Dependency | `BE-FIN-138` 🟡 [BE] — backend pendaftaran kartu penjamin perusahaan terhubung dengan data HRD |
| Klasifikasi | `MEDIUM` — 1 view modal bersama (`emergency-patient-payer-modal.jsx`), 1 step view (`payment-method-step.jsx`), 1 utilitas payload (`emergency-registration.utils.js`), 1 service normalizer (`emergency-registration.service.js`), 1 berkas unit test baru (`emergency-patient-payer-internal-benefit.test.mjs`) |
| Task Mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only |
| Target Tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/` (laporan dan roadmap saja) |
| Tanggal | 4 November 2026 |
| Status | ✅ **SELESAI** — implementasi selesai, 4 unit test AC 1–4 PASS, 23 existing unit test PASS, ESLint lolos bebas galat |

---

## 1. Keadaan yang Ditemukan di Awal

1. Pada alur registrasi pasien rumah sakit (baik di loket Rawat Jalan maupun IGD), pendaftaran kartu penjamin perusahaan (`PatientCompanyGuarantor`) sebelumnya hanya menyediakan input teks bebas untuk **Nomor Polis / Nomor Pegawai** (`employeeNumber` / `policyNumber`).
2. Masalah timbul ketika penjamin yang dipilih adalah **Benefit Karyawan Rumah Sakit** (kode penjamin `SYS-RS-BENEFIT`). Petugas pendaftaran di loket sering kali mengetik nomor pegawai sembarangan atau salah ketik, sehingga kartu tidak terhubung (*unlinked*) dengan profil pegawai resmi di sistem HRD. Akibatnya, pada saat pasien berobat dan tagihan diterbitkan di kasir billing, validasi plafon benefit gagal dan pemotongan piutang pegawai (*Employee AR*) menjadi kacau.
3. Selain itu, bila penyimpanan kartu penjamin gagal di backend dengan kode `422 Unprocessable Entity` (misalnya karena masa kerja karyawan belum genap 1 tahun atau pasien belum didaftarkan sebagai tanggungan keluarga resmi di HR), modal registrasi menangkap galat secara umum dan menampilkan kalimat bantuan fallback:
   > *"Penjamin belum dapat disimpan. Coba kembali."*
   Hal ini membingungkan petugas loket pendaftaran karena mereka tidak mengetahui alasan sebenarnya dari penolakan tersebut.
4. Apabila pasien yang bersangkutan sudah pernah didaftarkan dan telah memiliki kartu Benefit Karyawan aktif di database rumah sakit, sistem belum menampilkan penanda khusus kartu aktif yang sudah ada, sehingga petugas loket berpotensi melakukan pendaftaran ulang (*duplicate POST*).
5. Pada task `FE-FIN-058`, dilakukan perbaikan terpadu pada komponen modal penjamin bersama (`emergency-patient-payer-modal.jsx`) dan pembangun payload registrasi agar:
   - Menghubungkan pemilihan pemilik benefit langsung ke data HR (Karyawan & Dokter Tetap) serta verifikasi anggota keluarga.
   - Mengirimkan `benefitOwnerWorkforceProfileId` tanpa nomor pegawai bebas (`employeeNumber: ""`).
   - Menampilkan galat HTTP `422` dari backend apa adanya secara transparan.
   - Menampilkan banner kartu internal aktif yang sudah ada dan tombol `[Pakai kartu]` tanpa mengirimkan HTTP `POST` kartu baru.
   - Menjaga alur asuransi dan penjamin eksternal konvensional tetap berjalan normal tanpa gangguan.

---

## 2. Proses Bisnis dari Sisi Pengguna

### Peran Pengguna
Petugas Pendaftaran Pasien / Petugas Admisi (*Registration & Admission Officer*) di loket Rawat Jalan maupun IGD.

### Skenario Operasional Rumah Sakit

#### Skenario A: Pendaftaran Kartu Benefit Karyawan Baru untuk Pasien (Karyawan Sendiri / Keluarga)
1. **Pemicu (Trigger):**
   Seorang anak dari perawat rumah sakit (misalnya anak dari Ns. Siti Aminah) datang berobat ke IGD atau Poliklinik Anak. Petugas pendaftaran membuka formulir penjamin dan menekan tombol **"Daftarkan Penjamin Baru"**.
2. **Pemilihan Jenis Penjamin:**
   - Petugas memilih Jenis Penjamin: **Perusahaan**.
   - Pada kolom pilihan perusahaan penjamin, petugas memilih **Benefit Karyawan RS (SYS-RS-BENEFIT)**.
3. **Pencarian Data HR Otomatis:**
   - Sistem seketika mendeteksi bahwa ini adalah penjamin internal rumah sakit. Scanner kartu fisik Plustek dan kolom input nomor polis bebas otomatis disembunyikan.
   - Muncul antarmuka pencarian pegawai HR. Petugas mengetikkan nama *"Siti Aminah"* atau NIP-nya (minimal 3 karakter).
   - Sistem menampilkan hasil pencarian dari profil tenaga kerja aktif (*Workforce Profile*), mencakup karyawan dan dokter tetap beserta unit kerjanya. Petugas mengklik nama Ns. Siti Aminah.
4. **Penentuan Subjek Berobat (*Benefit Target*):**
   - Petugas memilih opsi **"Anggota keluarga"**.
   - Sistem memuat daftar keluarga resmi yang terdaftar di HRD. Petugas memilih nama anak yang berobat.
   - Sistem secara *real-time* menghubungi layanan kelayakan (`checkEligibility`) dan menampilkan status kelayakan hijau: *"Terdaftar dan terverifikasi"* beserta nama paket manfaat yang berlaku.
5. **Penyimpanan Kartu:**
   - Petugas menekan tombol **"Simpan Penjamin"**.
   - Sistem mengirimkan payload yang membawa `benefitOwnerWorkforceProfileId` dan `familyMemberId`, dengan `employeeNumber: ""` (dikosongkan agar backend mengisinya secara presisi dari relasi data HR).
   - Kartu berhasil tersimpan dan otomatis terpilih untuk menanggung kunjungan pasien.

#### Skenario B: Penanganan Galat Backend 422 (Alasan Nyata Transparan)
1. Petugas pendaftaran memilih karyawan yang masa kerjanya baru 3 bulan, padahal kebijakan RS mensyaratkan masa kerja minimal 1 tahun untuk hak rawat jalan.
2. Ketika petugas menekan **"Simpan Penjamin"**, backend mengembalikan status HTTP `422 Unprocessable Entity` dengan pesan:
   > *"Masa kerja karyawan belum mencapai syarat minimal 1 tahun untuk hak klaim rawat jalan."*
3. Modal pendaftaran menangkap pesan tersebut apa adanya dan langsung menampilkannya di banner galat modal:
   > ❌ **Penjamin belum tersimpan:** *Masa kerja karyawan belum mencapai syarat minimal 1 tahun untuk hak klaim rawat jalan.*
4. Petugas loket dapat segera menjelaskan kendala tersebut kepada keluarga pasien dengan tenang dan menawarkan opsi pendaftaran sebagai Pasien Umum / Mandiri (*General Self-Pay*).

#### Skenario C: Pasien Sudah Memiliki Kartu Benefit Karyawan Aktif
1. Pasien yang sama datang kembali untuk kontrol rutin. Petugas membuka modal penjamin dan memilih perusahaan **Benefit Karyawan RS**.
2. Sistem memeriksa data penjamin yang sudah terdaftar pada pasien (`existingPayers`).
3. Sistem mendeteksi bahwa pasien telah memiliki kartu Benefit Karyawan RS yang aktif.
4. Tampil banner informasi berwarna biru muda:
   > ℹ️ **Pasien sudah memiliki kartu Benefit Karyawan RS yang aktif.**
   > No. Kartu: **BEN-RS-2026-001** • Pemilik: **Ns. Siti Aminah, S.Kep**
5. Tersedia tombol primer **[Pakai kartu]**.
6. Petugas menekan **[Pakai kartu]**. Sistem langsung memilih kartu aktif tersebut untuk kunjungan saat ini dan menutup modal **tanpa melakukan HTTP POST pendaftaran kartu baru**.

#### Skenario D: Pendaftaran Asuransi / Penjamin Perusahaan Eksternal (Regresi Terjaga)
1. Pasien lain datang membawa asuransi swasta (misalnya Prudential) atau penjamin perusahaan swasta (misalnya PT Telkom).
2. Petugas memilih Asuransi atau Perusahaan Eksternal.
3. Scanner kartu Plustek tetap aktif dan dapat digunakan.
4. Input teks Nomor Polis dan Masa Aktif Kartu tetap tampil dengan validasi wajib diisi.
5. Payload yang dikirimkan tetap membawa nomor polis/kartu pada `employeeNumber`, tanpa menyertakan `benefitOwnerWorkforceProfileId`.

---

## 3. Perubahan Teknis yang Dikerjakan

### 3.1 Berkas yang Berubah

| Berkas | Jenis Perubahan | Deskripsi Perubahan |
| --- | --- | --- |
| `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` | Utility | Memperbarui fungsi `buildPatientCompanyGuarantorPayload`: bila kartu terdeteksi sebagai internal benefit (`isInternalBenefit` atau membawa `benefitOwnerWorkforceProfileId`), maka `employeeNumber: ""` dikosongkan (tanpa nomor bebas), serta menyertakan `benefitOwnerWorkforceProfileId` dan `familyMemberId`. Bila eksternal, `employeeNumber` tetap diisi nomor kartu/polis bebas dan ID benefit owner diset `null`. |
| `src/lib/services/health-services/registration-management/emergency-registration.service.js` | Service | Menambahkan penanda `isEmployeeBenefitGuarantor: Boolean(...)` pada `normalizeCompanyGuarantorOption` bila kode penjamin adalah `SYS-RS-BENEFIT` atau namanya mengandung "Benefit Karyawan", serta mengekspor fungsi tersebut agar dapat diverifikasi pada unit test. |
| `src/components/view/health-services/registration-management/emergency-registration/payment-method-step.jsx` | UI Component | Meneruskan prop `patientId={patientId}`, `existingPayers={registration?.payers || []}`, dan `onSelectExistingPayer={onSelectPayer}` ke komponen `PatientPayerModal`. |
| `src/components/view/health-services/registration-management/emergency-registration/emergency-patient-payer-modal.jsx` | UI Component (Modal) | 1. Menambahkan pendeteksian `isInternalBenefit` dan `activeInternalPayer`.<br>2. Mengintegrasikan pencarian profil tenaga kerja HRD (karyawan/dokter tetap), pemilihan subjek berobat (`SELF` atau `FAMILY`), dan pengecekan kelayakan *real-time* via `PatientCompanyGuarantorInternalBenefitSection`.<br>3. Menyembunyikan scanner Plustek dan input nomor polis bebas saat penjamin internal dipilih.<br>4. Menampilkan banner kartu internal aktif beserta tombol `[Pakai kartu]` tanpa POST jika kartu aktif sudah ada.<br>5. Memperbarui penanganan galat `handleSubmit` agar membaca string pesan atau `submitError?.message` apa adanya tanpa ditimpa kalimat "Coba kembali". |
| `tests/unit/emergency-patient-payer-internal-benefit.test.mjs` | Unit Test | Berkas pengujian unit baru berbasis Node.js test runner untuk memverifikasi secara ketat AC 1, AC 2, AC 3, dan AC 4. |

### 3.2 Kepatuhan Arsitektur dan Komponen UI

```text
UI GATE DECISION: PASS REUSE
- BaseButton: REUSE (varian primary untuk "Simpan Penjamin" & "Pakai kartu", secondary untuk "Batal")
- InformationAlert: REUSE (banner informasi penjamin internal dan galat backend)
- StatusBadge: REUSE (indikator status kelayakan dan enrollment benefit)
- PatientCompanyGuarantorInternalBenefitSection: REUSE (@/components/view/health-services/patient-management/master-data/patient-company-guarantors/add/)
- Tidak ada token warna kustom atau CSS hardcoded baru di luar modul styling resmi.
```

---

## 4. Keadaan Tampilan Antarmuka (UI States)

| State | Tampilan Antarmuka | Indikator / Pesan |
| --- | --- | --- |
| **Penjamin Eksternal Dipilih** | Scanner kartu Plustek aktif, formulir input Nomor Polis & Masa Aktif Kartu tampil | Kolom nomor polis wajib diisi secara manual atau via scan kartu |
| **Penjamin RS Benefit Dipilih (Belum Ada Kartu)** | Scanner kartu dan input nomor bebas disembunyikan; antarmuka pencarian pegawai HRD aktif | Kolom pencarian NIP/Nama karyawan/dokter minimal 3 karakter; opsi berobat diri sendiri (*SELF*) atau keluarga (*FAMILY*) |
| **Pencarian Pegawai Berjalan** | Spinner pemuatan aktif pada kolom pencarian pegawai | Teks: *"Mencari karyawan/dokter di HR..."* |
| **Pegawai HR Terpilih** | Banner pegawai terpilih tampil rapi dengan nama, NIP/No. Dokter, NIK tersamar, dan tombol "Ganti Penerima" | Menampilkan rincian status kelayakan *real-time* dari sistem HR |
| **Kartu Internal Aktif Ditemukan** | Banner biru muda tampil di atas formulir | *"Pasien sudah memiliki kartu Benefit Karyawan RS yang aktif."* Tombol primer berubah menjadi **[Pakai kartu]** tanpa tombol Simpan Penjamin |
| **Galat Validasi Backend (422)** | Modal tetap terbuka, banner merah `EmergencyInlineAlert` muncul di atas footer | Menampilkan alasan penolakan asli dari backend apa adanya (misal: *"Jatah benefit rawat jalan tahun berjalan telah habis"*) |

---

## 5. Spesifikasi Kontrak API (Swagger-Aligned)

### Tag: `[Tags("Registration Management", "Patient Guarantor Management")]`

| Method | Path Endpoint | Deskripsi Bisnis | Autentikasi / Hak Akses | Format Request (Payload Utama) | Format Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/health-services/patient-management/master-data/patient-company-guarantors/admin` | Mendaftarkan kartu penjamin perusahaan / benefit internal pasien dari loket registrasi | Petugas Loket Registrasi / Admisi (`Role: Staff`) | `{ patientId: UUID, companyGuarantorId: UUID, employeeNumber: string, benefitOwnerWorkforceProfileId: UUID?, familyMemberId: UUID?, participantName: string, relationship: string, isPrimary: boolean, isEligible: boolean }` | `{ success: true, message: "Penjamin berhasil disimpan", data: { patientCompanyGuarantorId: UUID, ... } }` |
| `GET` | `/api/v1/health-services/registration-management/employee-benefit-lookups/workforce-profiles` | Mencari data profil karyawan dan dokter tetap aktif dari sistem HR (min. 3 karakter, NIK tersamar) | Petugas Loket Registrasi (`Role: Staff`) | Query string: `?search=...` | `{ success: true, data: [ { workforceProfileId: UUID, staffNumber: string, fullName: string, nationalIdMasked: string, ownerType: "EMPLOYEE"\|"DOCTOR", hasActiveMedicalEnrollment: boolean } ] }` |
| `GET` | `/api/v1/health-services/registration-management/employee-benefit-lookups/workforce-profiles/{id}/family-members` | Mengambil daftar anggota keluarga terdaftar dari pemilik benefit HR | Petugas Loket Registrasi (`Role: Staff`) | Path parameter: `{id}` (WorkforceProfileId) | `{ success: true, data: [ { familyMemberId: UUID, fullName: string, relationshipCode: string, dependentVerification: string } ] }` |
| `GET` | `/api/v1/health-services/registration-management/employee-benefit-lookups/eligibility` | Memeriksa kelayakan pendaftaran benefit pasien secara langsung ke sistem HRD | Petugas Loket Registrasi (`Role: Staff`) | Query string: `?workforceProfileId=...&familyMemberId=...&serviceDate=...` | `{ success: true, data: { isEligible: boolean, planName: string, relationshipCode: string, reasonCode: string, message: string } }` |

---

## 6. Bukti Verifikasi dan Pengujian (Evidence-Based Validation)

### 6.1 Hasil Pengujian Unit Otomatis (Node.js Test Runner)

Perintah eksekusi:
```bash
cd QuilvianSystemFrontendDev && node --import ./tests/helpers/register.mjs --test tests/unit/emergency-patient-payer-internal-benefit.test.mjs
```

Hasil eksekusi:
```text
✔ FE-FIN-058 AC 1: payload kartu internal membawa benefitOwnerWorkforceProfileId tanpa employeeNumber bebas (0.363875ms)
✔ FE-FIN-058 AC 2: ekstraksi galat 422 backend menampilkan pesan sebenarnya tanpa fallback 'Coba kembali' (0.05475ms)
✔ FE-FIN-058 AC 3: deteksi kartu Benefit Karyawan aktif yang ada dan peniadaan POST (0.076334ms)
✔ FE-FIN-058 AC 4: alur asuransi dan penjamin eksternal tidak berubah (0.068292ms)
ℹ tests 4
ℹ suites 0
ℹ pass 4
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 88.190167
```

### 6.2 Verifikasi Regresi Modul Registrasi Existing

Perintah eksekusi:
```bash
cd QuilvianSystemFrontendDev && node --import ./tests/helpers/register.mjs --test tests/unit/emergency-registration-payload.test.mjs tests/unit/registration-category-payer.test.mjs
```

Hasil eksekusi:
```text
✔ FE-IGD-014 K1: payload encounter mengirim Emergency, bukan Outpatient
✔ FE-IGD-001 K2: nilai status registrasi sejajar dengan enum backend
✔ FE-IGD-001 K2: label Registered tidak lagi dipakai untuk nilai 1
✔ FE-IGD-001 K2: status registrasi tak dikenal tidak mematikan layar
✔ FE-IGD-036 K1: loket tidak lagi punya pembangun payload kunjungan IGD
✔ FE-IGD-036 K1: payload encounter tidak membawa ruas kunjungan walau nilainya ada di formulir
✔ FE-IGD-036 K5: formulir loket tidak lagi menyimpan isian waktu tiba dan ruas kunjungan
✔ isBpjsPayer mengikuti penanda nama BKC-DEC-121
✔ kelayakan kartu: tidak aktif, tidak layak, belum berlaku, kedaluwarsa
✔ Kiosk Pasien Lama: Pasien Pribadi = Tunai tanpa step penjamin
✔ Kiosk Pasien Lama: penjamin kosong tidak pernah dibaca sebagai Tunai
✔ Kiosk Pasien Lama: asuransi valid = Asuransi, kedaluwarsa/tidak layak ditolak
✔ Kiosk Pasien Lama: penjamin perusahaan tetap 'company', bukan asuransi
✔ Kiosk Pasien Lama: rujukan wajib memilih sumber penjamin yang sesuai
✔ Kiosk Pasien Lama: kunjungan tidak dikirim bila penjamin belum jelas
✔ Kiosk Pasien Baru: perusahaan saja = Penjamin Perusahaan; dengan asuransi = asuransi utama
✔ Kiosk Pasien Baru: BPJS rujukan tertahan, kartu BPJS tidak dibuat dari Kiosk
✔ Kiosk Pasien Baru: draf kunjungan wajib membawa jenis penjamin eksplisit
✔ Rawat Jalan: jenis pembayaran eksplisit, kosong tidak menjadi Tunai
✔ Rawat Jalan: penjamin perusahaan terkirim sebagai PaymentType 3 dengan PatientCompanyGuarantorId
✔ Rawat Jalan: resolusi penjamin per kategori
✔ Rawat Jalan: step Metode Pembayaran tidak ada; Penjamin hanya untuk Asuransi dan Rujukan
✔ Rawat Jalan: Jenis Kunjungan klinis tidak diganti kategori penjamin
ℹ tests 23
ℹ suites 0
ℹ pass 23
ℹ fail 0
ℹ duration_ms 75.975417
```

### 6.3 Verifikasi Kerapian Kode (ESLint)

Perintah eksekusi:
```bash
cd QuilvianSystemFrontendDev && npx eslint tests/unit/emergency-patient-payer-internal-benefit.test.mjs src/components/view/health-services/registration-management/emergency-registration/emergency-patient-payer-modal.jsx src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js src/lib/services/health-services/registration-management/emergency-registration.service.js
```

Hasil:
```text
✖ 1 problem (0 errors, 1 warning)
- 0 lint errors
- 1 non-blocking warning (setMounted hook bawaan lama komponen)
```

---

## 7. Matriks Keterlacakan (Traceability Matrix)

| Kebutuhan Bisnis / AC | Implementasi Teknis | Lokasi Kode / Berkas | Status Verifikasi |
| :--- | :--- | :--- | :--- |
| **AC 1: Payload membawa `benefitOwnerWorkforceProfileId`, tanpa `employeeNumber` bebas** | `buildPatientCompanyGuarantorPayload` memeriksa `isInternal`; menyertakan ID pemilik benefit HR dan mengosongkan `employeeNumber: ""` agar backend mengisi otomatis dari data HRD | `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` | ✅ PASS (`emergency-patient-payer-internal-benefit.test.mjs`) |
| **AC 2: Galat 422 Backend tampil apa adanya, bukan "Coba kembali"** | `handleSubmit` pada modal mengekstrak galat string atau `submitError?.message` / `submitError?.error` tanpa fallback kalimat umum | `src/components/view/health-services/registration-management/emergency-registration/emergency-patient-payer-modal.jsx` | ✅ PASS (`emergency-patient-payer-internal-benefit.test.mjs`) |
| **AC 3: Kartu internal aktif yang ada ditampilkan, tanpa POST ("Pasien sudah memiliki kartu Benefit Karyawan RS yang aktif." [Pakai kartu])** | Modal mendeteksi `activeInternalPayer` dari `existingPayers`; menampilkan banner khusus dan tombol primer `[Pakai kartu]` yang memicu `onSelectExistingPayer` tanpa memanggil `onSave` / HTTP POST | `src/components/view/health-services/registration-management/emergency-registration/emergency-patient-payer-modal.jsx` | ✅ PASS (`emergency-patient-payer-internal-benefit.test.mjs`) |
| **AC 4: Alur asuransi dan penjamin eksternal tidak berubah pada rawat jalan dan IGD** | Penjamin eksternal tetap menggunakan nomor kartu/polis bebas, scanner Plustek tetap aktif, dan ID benefit owner tetap bernilai `null` | `src/components/view/health-services/registration-management/emergency-registration/emergency-patient-payer-modal.jsx` | ✅ PASS (`emergency-patient-payer-internal-benefit.test.mjs` & `registration-category-payer.test.mjs`) |

---

## 8. Kesimpulan

Task `FE-FIN-058` telah diselesaikan dengan sukses dan memenuhi seluruh Acceptance Criteria serta batasan yang ditetapkan. Pendaftaran kartu Benefit Karyawan RS pada loket pendaftaran Rawat Jalan dan IGD kini terintegrasi secara aman dengan data HRD tanpa input bebas yang rawan galat, transparan dalam menampilkan alasan penolakan backend, serta mencegah duplikasi pendaftaran kartu aktif.

