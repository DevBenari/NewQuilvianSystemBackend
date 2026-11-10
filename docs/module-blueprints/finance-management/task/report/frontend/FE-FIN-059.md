# Laporan Perubahan Frontend — `FE-FIN-059`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-059` |
| Judul | Tab Penjamin Perusahaan Menunjukkan Apakah Benefit RS Pasien Benar-Benar Aktif Menurut HR |
| Slice | `REV-23` (Benefit Management — Sinkronisasi Kartu Pasien & Lifecycle Benefit) / `S8` Tab Penjamin Perusahaan |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "REVISI ROADMAP 23 (3 November 2026)", tabel Task REV-23 Frontend |
| Trace | `FR-FIN-270`; `FIN-DEC-242`; `FIN-DES-119`; `SCR-BRS-05`; `FIN-API-1.12` §`V.2` field `employeeBenefitStatus` |
| Layar | `SCR-BRS-05` (Tab Penjamin Perusahaan pada Detail Pasien `/health-services/patient-management/master-data/patients/{id}`) & Detail Kartu Penjamin Pasien (`/health-services/patient-management/master-data/patient-company-guarantors/{id}`) |
| Contract Version | `FIN-API-1.12` §`V.2` (`GET /api/v1/health-services/patient-management/master-data/patient-company-guarantors`) |
| Wewenang UI | `REUSE` — menggunakan pill styling `@/style/health-services/master-data/patients/patient-profile-detail.css` dan komponen dasar Quilvian tanpa kelas visual atau token styling hardcoded baru |
| Dependency | `BE-FIN-132` 🟡 [BE] — backend telah mengembalikan field aditif `isEmployeeBenefitGuarantor` dan `employeeBenefitStatus` |
| Klasifikasi | `MEDIUM` — 1 view detail pasien (`patients-detail-view.jsx`), 1 utilitas baris detail kartu (`patient-company-guarantor-detail-utils.jsx`), 1 utilitas display resolver (`patient-company-guarantor-display-utils.jsx`), 1 berkas unit test baru (`patient-company-guarantor-employee-benefit-status.test.mjs`) |
| Task Mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only |
| Target Tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/` (laporan dan roadmap saja) |
| Tanggal | 4 November 2026 |
| Status | ✅ **SELESAI** — implementasi selesai, 3 unit test AC 1–3 PASS, regresi unit test existing PASS, ESLint lolos bebas galat |

---

## 1. Keadaan yang Ditemukan di Awal

1. Pada layar Detail Pasien (`SCR-BRS-05`), tab **Penjamin Perusahaan** (`companyGuarantors`) sebelumnya hanya menampilkan data statis kartu penjamin: Perusahaan, Kode Penjamin, No Karyawan, Periode (berdasarkan tanggal statis lokal), dan status Kelayakan boolean biasa (`Eligible` / `Tidak`).
2. Masalah timbul pada kartu internal **Benefit Karyawan RS** (`SYS-RS-BENEFIT`). Kartu penjamin internal di tabel lokal pasien (`MstPatientCompanyGuarantor`) tidak menyimpan tanggal kedaluwarsa secara statis, karena masa berlaku dan keaktifan hak benefit dikelola secara dinamis di sistem SDM/HRD.
3. Akibatnya:
   - Ketika benefit karyawan di sistem HR sudah kedaluwarsa (misalnya masa berlakunya berakhir 31 Oktober 2026), kartu pasien di tab Penjamin Perusahaan tetap terlihat aktif secara keliru (*false active*).
   - Ketika karyawan baru didaftarkan namun tanggal berlakunya baru di masa depan (misalnya 1 Desember 2026), kartu tampak seolah-olah sudah dapat dipakai.
   - Ketika seorang pegawai mengundurkan diri (*resign*) atau tidak eligible, petugas pendaftaran dan admisi rumah sakit tidak dapat melihat status sebenarnya dari data HR secara langsung di layar pasien.
4. Melalui task backend `BE-FIN-132`, endpoint daftar dan detail kartu penjamin telah dilengkapi dengan dua field aditif berbasis pembacaan langsung ke HR (*read-through*):
   - `isEmployeeBenefitGuarantor`: penanda bahwa kartu adalah penjamin Benefit Karyawan internal.
   - `employeeBenefitStatus`: memuat `state`, `label` bahasa Indonesia, `enrollmentNumber`, `planName`, `effectiveStartDate`, `effectiveEndDate`, dan `message`.
5. Frontend pada task `FE-FIN-059` ditugaskan untuk menyajikan status hidup ini secara transparan pada antarmuka tab Penjamin Perusahaan detail pasien dan detail kartu, memastikan setiap *state* memiliki label baku, menjamin `HR_UNAVAILABLE` tidak pernah tampil aktif (*fail-closed*), serta memastikan kartu non-internal (asuransi/perusahaan eksternal) tetap tampil normal tanpa regresi.

---

## 2. Proses Bisnis dari Sisi Pengguna

### Peran Pengguna
1. **Petugas Admisi / Pendaftaran Pasien (*Admission & Registration Officer*)**: memverifikasi keabsahan kartu penjamin sebelum pasien diarahkan ke unit pelayanan medis.
2. **Petugas Verifikasi Jaminan / Kasir Rumah Sakit (*Billing & Guarantee Verifier*)**: memeriksa keaktifan hak benefit karyawan rumah sakit saat proses verifikasi penjaminan dan tagihan.
3. **Staf SDM / HRD**: mengecek kartu penjamin pegawai atau keluarganya yang terdaftar di sistem rekam medis.

### Skenario Operasional Rumah Sakit

#### Skenario 1: Verifikasi Kartu Benefit yang Kedaluwarsa (Contoh Kasus Rendy)
1. **Pemicu (Trigger):**
   Rendy, seorang karyawan rumah sakit, datang membawa kartu benefit internal ke loket pendaftaran pada tanggal `2 November 2026`. Hak program benefit rawat jalan periode sebelumnya telah berakhir pada `31 Oktober 2026`.
2. **Pemeriksaan pada Layar Detail Pasien (`SCR-BRS-05`):**
   - Petugas membuka rekam medis Rendy pada menu **Master Data Pasien** (`/health-services/patient-management/master-data/patients/{id}`).
   - Petugas mengklik tab **Penjamin Perusahaan**.
3. **Tampilan Status Benefit Hidup:**
   - Kolom **Status Benefit** menampilkan pill merah tegas berlabel **"Kedaluwarsa"** (`state: ENDED`).
   - Kolom **Periode** menampilkan tanggal berakhir hak benefit: `31 Okt 2026`.
   - Kolom **No. Ref / Enrollment** menampilkan nomor pendaftaran resmi di HRD: `BEN000123`.
4. **Tindakan Petugas:**
   Petugas langsung menginformasikan kepada Rendy bahwa jatah benefit periode 2025/2026 telah berakhir dan belum ada perpanjangan dari HRD, sehingga untuk kunjungan hari ini diarahkan menggunakan pembayaran umum mandiri atau menunggu konfirmasi perpanjangan HRD.

#### Skenario 2: Verifikasi Kartu Benefit Aktif (Contoh Dokter Tetap dr. Sinta)
1. dr. Sinta (dokter tetap rumah sakit) berobat ke poli spesialis.
2. Petugas membuka tab Penjamin Perusahaan pada rekam medis dr. Sinta.
3. Sistem menampilkan pill hijau berlabel **"Aktif"** (`state: ACTIVE`) pada kolom **Status Benefit**, lengkap dengan nomor pendaftaran benefit `BEN000456` dan periode berlaku hingga `31 Des 2026`.
4. Petugas dengan yakin melanjutkan registrasi menggunakan penjamin Benefit Karyawan tanpa keraguan penolakan billing di kasir.

#### Skenario 3: Verifikasi Kartu yang Belum Mulai Berlaku
1. Staf baru (Budi) telah disetujui pendaftaran benefitnya di HRD, namun tanggal mulai berlakunya adalah `1 Desember 2026`. Budi datang pada `4 November 2026`.
2. Tab Penjamin Perusahaan menampilkan pill kuning berlabel **"Belum berlaku"** (`state: NOT_YET_EFFECTIVE`).
3. Petugas memberikan penjelasan bahwa hak benefit baru dapat digunakan mulai tanggal 1 Desember 2026.

#### Skenario 4: Fail-Closed Saat Sistem HR Tidak Dapat Diakses (`HR_UNAVAILABLE`)
1. Terjadi gangguan koneksi jaringan sesaat antara server rekam medis dan basis data HR.
2. Sistem backend mengembalikan `state: "HR_UNAVAILABLE"`, `label: "Tidak dapat dibaca"`.
3. Tab Penjamin Perusahaan menampilkan status berwarna abu-abu netral berlabel **"Tidak dapat dibaca"** (bukan hijau aktif).
4. Prinsip keselamatan operasional (*Fail-Closed*): Sistem **tidak pernah** mengasumsikan kartu aktif palsu (*false positive*) ketika status HR tidak dapat dibaca, mencegah klaim ilegal atau defisit biaya rumah sakit.

#### Skenario 5: Kartu Perusahaan Eksternal / Swasta (Kompatibilitas Penuh)
1. Pasien umum membawa kartu penjamin dari PT Telkom Indonesia atau PT PLN.
2. Kolom **Status Benefit** menampilkan `-` (karena status benefit HR hanya berlaku untuk penjamin internal Benefit Karyawan RS).
3. Seluruh kolom data perusahaan eksternal (Perusahaan, Kode, No Karyawan, Periode, Kelayakan) tetap tampil seperti biasa tanpa ada pergeseran fungsi.

---

## 3. Perubahan Teknis yang Dikerjakan

### 3.1 Berkas yang Berubah

| Berkas | Jenis Perubahan | Deskripsi Perubahan |
| --- | --- | --- |
| `src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-display-utils.jsx` | Utility | 1. Mendefinisikan konstanta status: `EMPLOYEE_BENEFIT_STATUS_STATES`, `EMPLOYEE_BENEFIT_STATUS_LABELS`, dan `EMPLOYEE_BENEFIT_STATUS_TONES`.<br>2. Mengimplementasikan fungsi resolver murni `resolveEmployeeBenefitStatus(item)`: mengevaluasi penjamin internal vs eksternal, memetakan 7 keadaan status hidup, memastikan `HR_UNAVAILABLE` fail-closed (tidak pernah aktif), serta mengembalikan `null` untuk kartu non-internal. |
| `src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-detail-utils.jsx` | Utility | Memperbarui `buildPatientCompanyGuarantorDetailRows`: menyisipkan baris informasi detail **Status Benefit (HR)** dan **No. Enrollment Benefit** pada section `benefit` secara kondisional hanya bila kartu adalah kartu penjamin internal RS. |
| `src/components/view/health-services/patient-management/master-data/patients/detail/patients-detail-view.jsx` | UI View | 1. Mengimpor `resolveEmployeeBenefitStatus` dari display-utils.<br>2. Menambahkan kolom **"Status Benefit"** pada tabel resource `companyGuarantors` yang me-render pill status berwarna (`is-success`, `is-warning`, `is-danger`, atau abu-abu default) untuk kartu internal, dan `-` untuk kartu non-internal.<br>3. Menambahkan kolom **"No. Ref / Enrollment"** untuk menampilkan nomor pendaftaran benefit resmi dari HR.<br>4. Memperbarui kolom **"Periode"** agar mengutamakan rentang tanggal berlaku benefit dari HR bila tanggal lokal kosong. |
| `tests/unit/patient-company-guarantor-employee-benefit-status.test.mjs` | Unit Test | Berkas pengujian unit baru berbasis Node.js test runner untuk menguji AC 1, AC 2, dan AC 3 secara mendalam. |

### 3.2 Kepatuhan Arsitektur dan Komponen UI

```text
UI GATE DECISION: PASS REUSE
- wfp-pill: REUSE (@/style/health-services/master-data/patients/patient-profile-detail.css & wfp-workforce-detail.css)
  - is-success: hijau untuk status ACTIVE ("Aktif")
  - is-warning: kuning untuk status NOT_YET_EFFECTIVE ("Belum berlaku") dan NOT_LINKED ("Belum tertaut")
  - is-danger: merah untuk status ENDED ("Kedaluwarsa") dan NOT_ELIGIBLE ("Tidak layak")
  - default/secondary: abu-abu untuk status NOT_ACTIVE ("Belum aktif") dan HR_UNAVAILABLE ("Tidak dapat dibaca")
- Table Resource: REUSE struktur tabel master data patient resource yang ada tanpa styling kustom baru.
- Tidak ada token warna hardcoded atau CSS visual baru yang ditambahkan.
```

---

## 4. Keadaan Tampilan Antarmuka (UI States)

| State Benefit HR (`state`) | Label Bahasa Indonesia | Tampilan Visual (Tone) | Makna Operasional Rumah Sakit |
| :--- | :--- | :--- | :--- |
| `ACTIVE` | **Aktif** | Pill Hijau (`is-success`) | Hak benefit karyawan sah dan dapat langsung digunakan untuk menjamin kunjungan. |
| `NOT_YET_EFFECTIVE` | **Belum berlaku** | Pill Kuning (`is-warning`) | Pendaftaran benefit telah disetujui, namun tanggal mulai berlaku masih di masa depan. |
| `ENDED` | **Kedaluwarsa** | Pill Merah (`is-danger`) | Masa berlaku benefit telah berakhir di HR; kartu tidak dapat dipakai sebelum ada perpanjangan. |
| `NOT_ACTIVE` | **Belum aktif** | Pill Abu-abu Default | Pengajuan benefit masih berstatus Draft atau Menunggu Persetujuan di HRD. |
| `NOT_ELIGIBLE` | **Tidak layak** | Pill Merah (`is-danger`) | Pemilik benefit telah resign, berstatus dokter tamu, atau tanggungan keluarga belum diverifikasi. |
| `NOT_LINKED` | **Belum tertaut** | Pill Kuning (`is-warning`) | Kartu penjamin internal anomali yang belum terhubung dengan profil pegawai resmi di HR. |
| `HR_UNAVAILABLE` | **Tidak dapat dibaca** | Pill Abu-abu Default | *Fail-closed*: data HR tidak dapat diakses; kartu **tidak pernah** dianggap aktif. |
| *(Kartu Non-Internal)* | `-` | Teks Biasa (`-`) | Kartu penjamin eksternal swasta/asuransi; tidak terpengaruh status benefit HR. |

---

## 5. Spesifikasi Kontrak API (Swagger-Aligned)

### Tag: `[Tags("Patient Management", "Patient Company Guarantor")]`

| Method | Path Endpoint | Deskripsi Bisnis | Autentikasi / Hak Akses | Format Response (Field Tambahan Aditif) |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/patient-management/master-data/patient-company-guarantors` | Mengambil daftar kartu penjamin perusahaan pasien beserta evaluasi status benefit hidup dari HR untuk kartu internal | Petugas Admisi / Staff (`PatientCompanyGuarantor:Read`) | `{ success: true, data: { items: [ { patientCompanyGuarantorId: UUID, companyGuarantorName: string, isEmployeeBenefitGuarantor: boolean, employeeBenefitStatus: { state: string, label: string, enrollmentNumber: string, planName: string, effectiveStartDate: string, effectiveEndDate: string, message: string } \| null, ... } ] } }` |
| `GET` | `/api/v1/health-services/patient-management/master-data/patient-company-guarantors/{id}` | Mengambil informasi detail satu kartu penjamin perusahaan pasien lengkap dengan status kelayakan hidup dari HR | Petugas Admisi / Staff (`PatientCompanyGuarantor:Read`) | `{ success: true, data: { patientCompanyGuarantorId: UUID, isEmployeeBenefitGuarantor: boolean, employeeBenefitStatus: { state: string, label: string, enrollmentNumber: string, planName: string, effectiveStartDate: string, effectiveEndDate: string, message: string } \| null, ... } }` |

---

## 6. Bukti Verifikasi dan Pengujian (Evidence-Based Validation)

### 6.1 Hasil Pengujian Unit Otomatis (Node.js Test Runner)

Perintah eksekusi:
```bash
cd QuilvianSystemFrontendDev && node --import ./tests/helpers/register.mjs --test tests/unit/patient-company-guarantor-employee-benefit-status.test.mjs
```

Hasil eksekusi:
```text
✔ FE-FIN-059 AC 1: setiap state benefit hidup dari HR memiliki label bahasa Indonesia dan tone yang tepat (0.451583ms)
✔ FE-FIN-059 AC 2: HR_UNAVAILABLE tidak pernah tampil sebagai aktif (fail-closed) (0.058459ms)
✔ FE-FIN-059 AC 3: kartu non-internal tampil seperti sekarang (tanpa perubahan) (0.38175ms)
ℹ tests 3
ℹ suites 0
ℹ pass 3
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 75.092791
```

### 6.2 Pengujian Regresi Lintas Modul

Perintah eksekusi bersama suite test penjamin sebelumnya:
```bash
cd QuilvianSystemFrontendDev && node --import ./tests/helpers/register.mjs --test tests/unit/patient-company-guarantor-employee-benefit-status.test.mjs tests/unit/emergency-patient-payer-internal-benefit.test.mjs
```

Hasil:
```text
✔ FE-FIN-058 AC 1: payload kartu internal membawa benefitOwnerWorkforceProfileId tanpa employeeNumber bebas (0.38ms)
✔ FE-FIN-058 AC 2: ekstraksi galat 422 backend menampilkan pesan sebenarnya tanpa fallback 'Coba kembali' (0.058292ms)
✔ FE-FIN-058 AC 3: deteksi kartu Benefit Karyawan aktif yang ada dan peniadaan POST (0.081541ms)
✔ FE-FIN-058 AC 4: alur asuransi dan penjamin eksternal tidak berubah (0.071ms)
✔ FE-FIN-059 AC 1: setiap state benefit hidup dari HR memiliki label bahasa Indonesia dan tone yang tepat (0.622875ms)
✔ FE-FIN-059 AC 2: HR_UNAVAILABLE tidak pernah tampil sebagai aktif (fail-closed) (0.180833ms)
✔ FE-FIN-059 AC 3: kartu non-internal tampil seperti sekarang (tanpa perubahan) (0.693583ms)
ℹ tests 7
ℹ suites 0
ℹ pass 7
ℹ fail 0
```

### 6.3 Verifikasi Kerapian Kode (ESLint)

Perintah eksekusi:
```bash
cd QuilvianSystemFrontendDev && npx eslint tests/unit/patient-company-guarantor-employee-benefit-status.test.mjs src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-display-utils.jsx src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-detail-utils.jsx src/components/view/health-services/patient-management/master-data/patients/detail/patients-detail-view.jsx
```

Hasil:
```text
✖ 5 problems (0 errors, 5 warnings)
- 0 lint errors
- 5 non-blocking warnings bawaan lama komponen memoization & photo
```

---

## 7. Matriks Keterlacakan (Traceability Matrix)

| Kebutuhan Bisnis / AC | Implementasi Teknis | Lokasi Kode / Berkas | Status Verifikasi |
| :--- | :--- | :--- | :--- |
| **AC 1: Setiap `state` punya label bahasa Indonesia** | `resolveEmployeeBenefitStatus` memetakan seluruh 7 keadaan (`ACTIVE`, `NOT_YET_EFFECTIVE`, `ENDED`, `NOT_ACTIVE`, `NOT_ELIGIBLE`, `NOT_LINKED`, `HR_UNAVAILABLE`) ke label baku bahasa Indonesia dan tone styling visual | `src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-display-utils.jsx` | ✅ PASS (`patient-company-guarantor-employee-benefit-status.test.mjs`) |
| **AC 2: `HR_UNAVAILABLE` tidak pernah tampil sebagai aktif** | `resolveEmployeeBenefitStatus` menetapkan `tone: ""` (abu-abu/secondary) dan `isActiveBenefit: false` secara tegas ketika state adalah `HR_UNAVAILABLE` | `src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-display-utils.jsx` | ✅ PASS (`patient-company-guarantor-employee-benefit-status.test.mjs`) |
| **AC 3: Kartu non-internal tampil seperti sekarang** | `resolveEmployeeBenefitStatus` mengembalikan `null` untuk kartu penjamin eksternal; kolom Status Benefit dan No. Enrollment menampilkan tanda `-`, dan baris detail kartu tidak menyisipkan ruas benefit HR | `src/components/view/health-services/patient-management/master-data/patients/detail/patients-detail-view.jsx` & `patient-company-guarantor-detail-utils.jsx` | ✅ PASS (`patient-company-guarantor-employee-benefit-status.test.mjs`) |

---

## 8. Kesimpulan

Task `FE-FIN-059` telah diselesaikan secara tuntas dan memenuhi seluruh kriteria penerimaan. Tab Penjamin Perusahaan pada rekam medis pasien kini menyajikan status keaktifan benefit karyawan secara akurat berdasarkan pembacaan data langsung dari sistem HR, melindungi rumah sakit dari potensi klaim benefit kedaluwarsa, dan menerapkan prinsip *fail-closed* secara konsisten.

