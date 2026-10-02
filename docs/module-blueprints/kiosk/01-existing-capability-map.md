# Kiosk — Peta Kemampuan Existing (Existing Capability Map)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` |
| Revision peta | `1` |
| Status | `final` — seluruh conflict/unknown ditutup oleh `KSK-DEC-013..019` (00-interview-decisions r2 §11), kecuali `KSK-UNK-003/004` yang diteruskan ke build/desain |
| Tanggal audit | 30 September 2026 |
| SHA backend | `419b910fca5188285946850a95976ebff83ae8ce` (branch `sukmagp`) |
| SHA frontend | `4ec51b0bf5e724e899b95f118e273351437b71e7` |
| Masukan | [00-interview-decisions.md](00-interview-decisions.md) r1 (`KSK-DEC-001..012`), [evidence/2026-09-30-prd-revisi-kiosk.md](evidence/2026-09-30-prd-revisi-kiosk.md) |
| Kontrak lintas modul terkait | `RWI-ENC-PAYER-001` v1.0.0 `APPROVED` (rawat-inap) |

## 1. Batas audit

Audit ini hanya membaca source (tanpa database, tanpa runtime) pada empat rumpun `KSK-DEC-004` beserta keamanannya:

| Rumpun | Klaster yang ditelusuri |
| --- | --- |
| Lookup No. RM | Identity/Master Owner (`MstPatient`), Authorization (`KioskRead`), Documentation (Kartu Pasien), Audit/Log |
| Flow Pasien Lama | Workflow/Status (step), Episode (`TrxKioskScanSession`, `RegPatientEncounter`) |
| Penjamin Utama | Financial (`RegPatientEncounterGuarantor`, `EncounterPaymentType`) |
| Jadwal Dokter | Komponen UI dasar (`FilterSelect`) |

Tidak diaudit: isi data di database (misalnya variasi format No. HP yang tersimpan), perilaku runtime di perangkat Kiosk, dan alur Pasien Baru di luar titik masuk CTA.

**Pemicu impact scan:** perubahan pada `PatientController.cs`, `PatientEncounterController.cs`, `KioskScanSessionController.cs`, `MstPatientConfiguration.cs`, `Program.cs` (policy/rate limit/log), atau folder FE `components/view/kiosk/**`, `lib/hooks/kiosk/**`, `lib/services/kiosk/**`, `components/features/base-features/filter-select.jsx`.

## 2. Peta kemampuan

Status memakai tepat satu nilai: Ready to reuse, Reuse with adapter, Extend, Repair, Missing, Conflict, Unknown.

### 2.1 Lookup No. RM

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `KSK-CAP-001` | Kolom KTP & HP pasien | PatientManagement (`Pat`/`Mst`) | BE `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs#IdentityNumber,PhoneNumber@419b910f` | Ready to reuse | `IdentityNumber` maks. 50, `PhoneNumber` maks. 30 | — |
| `KSK-CAP-002` | Keunikan KTP | PatientManagement | BE `Repositories/Configurations/HealthServices/MstPatientConfiguration.cs#HasIndex(IdentityNumber).IsUnique().HasFilter("IsDelete = false")@419b910f`; validasi create `PatientController.cs#ValidateRequest` (tolak KTP duplikat) | Ready to reuse | Di antara pasien yang belum dihapus, **satu KTP hanya dimiliki satu pasien**. Kasus `KSK-DEC-007` (KTP cocok ke >1 pasien) secara skema tidak mungkin terjadi, kecuali lewat status pasien (lihat `KSK-UNK-001`). | Rendah |
| `KSK-CAP-003` | Pencarian KTP/HP persis, jawaban FOUND/NOT_FOUND/MULTIPLE | Registration (Kiosk) | BE `PatientController.cs#GetPatientsForKiosk@419b910f` → `GetPatients` dengan `Contains` pada 15+ kolom (nama, RM, KTP, HP, email, wilayah…) | Missing | Endpoint kiosk yang ada bersifat free-text, mengembalikan daftar berhalaman, dan tidak membedakan "tidak ditemukan" dari "cocok ganda". Perlu endpoint lookup baru. | Tinggi: memakai endpoint lama = enumerasi via nama/wilayah, melanggar KSK-RM-002 |
| `KSK-CAP-004` | Normalisasi No. HP | PatientManagement | BE `PatientController.cs#Create/Update` → `PhoneNumber = NormalizeNullableString(...)` (trim saja); pola digit-saja ada di HR `WfpEmergencyContactController.cs#NormalizePhone` | Missing | HP pasien disimpan **apa adanya**. `0812-3456-7890`, `+6281234567890`, `081234567890` bisa tersimpan sebagai tiga bentuk berbeda, dan tidak ada kolom ternormalisasi. Lookup persis butuh normalisasi di sisi query atau kolom turunan. Menjawab `KSK-ASM-003`: **tidak ada standar backend**. | Tinggi untuk jalur HP (`KSK-UNK-002`) |
| `KSK-CAP-005` | Index untuk HP | PatientManagement | BE `MstPatientConfiguration.cs#HasIndex(PhoneNumber)@419b910f` | Reuse with adapter | Index ada di kolom mentah, tetapi tidak terpakai bila query menormalisasi kolom lewat fungsi. | Performa |
| `KSK-CAP-006` | Otorisasi perangkat Kiosk | Platform Authorization | BE `Program.cs#AddPolicy(AuthorizationPolicies.KioskRead)@419b910f` | Ready to reuse | Policy menerima role `SuperAdmin`/`Administrator`/`Kiosk` atau klaim perangkat kiosk. Endpoint baru cukup memakai `[Authorize(Policy = "KioskRead")]`. | — |
| `KSK-CAP-007` | Rate limit per perangkat | Platform | BE seluruh repo: tidak ada `AddRateLimiter` / `EnableRateLimiting`; target `net9.0` (`QuilvianSystemBackend.csproj`) | Missing | Middleware rate limit bawaan ASP.NET Core tersedia di framework `net9.0` tanpa package baru. Partisi per akun perangkat perlu klaim identitas perangkat (`kioskDeviceId`/user id). | Sedang: fitur platform pertama, dampaknya lintas modul |
| `KSK-CAP-008` | Data Kartu Pasien | Kiosk (FE) | FE `components/features/base-features/base-patient-card.jsx#BasePatientCard@4ec51b0b`; dipakai `kiosk-patient-card-step-preview.jsx` | Ready to reuse | Menjawab `KSK-ASM-002`. Field kartu: **Nama Pasien, No. RM, Kode Pasien, Tipe Pasien, Jenis Kelamin, Golongan Darah**, ditambah QR berisi nama + No. RM. Tanggal lahir, alamat, KTP, dan HP **tidak** ada di kartu. | Golongan darah termasuk data kesehatan tetapi memang bagian kartu existing (KSK-RM-007 mengizinkan) |
| `KSK-CAP-009` | Respons data minimal | Kiosk | BE `PatientController.cs#GetPatientById` (`kiosk/{id}`) → `PatientDetailResponse` (alamat, KTP, HP, data kelahiran, catatan) | Missing | Endpoint kiosk existing mengembalikan detail lengkap. Lookup baru butuh DTO khusus berisi field Kartu Pasien saja (SEC-KSK-004). | Tinggi bila endpoint existing dipakai ulang |
| `KSK-CAP-010` | Log backend tanpa KTP/HP | Platform | BE `Program.cs#UseSerilogRequestLogging` → `BuildHttpDisplayMessage` memakai `Request.Path` saja | Ready to reuse | Log request **tidak** memuat query string maupun body. Endpoint baru tetap wajib menyamarkan nilai pada log miliknya sendiri. | — |
| `KSK-CAP-011` | Menu Cek No. RM | Kiosk (FE) | FE `components/view/kiosk/kiosk-home-view.jsx#SERVICE_ITEMS.medicalRecordCheck@4ec51b0b` | Extend | Tile sudah ada (judul, ikon, `enabledType: "always"`) tetapi hanya `notice`. Perlu `route` dan halaman baru. | — |

### 2.2 Flow Pasien Lama

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `KSK-CAP-020` | Urutan 8 step | Kiosk (FE) | FE `lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx#OLD_PATIENT_STEP_ITEMS, OLD_PATIENT_LAB_STEP_ITEMS@4ec51b0b` | Extend | Urutan sekarang `serviceTarget → find → review → type → payment → service → confirm → ticket`. Step awal `useState(OLD_PATIENT_STEPS.serviceTarget)`, `handleResetAll` kembali ke `serviceTarget`, dan `handleReviewContinue` langsung ke `type`. | — |
| `KSK-CAP-021` | Sesi kiosk sebagai alat identifikasi | Registration | FE `components/view/kiosk/registration/old-patient/kiosk-old-patient-step-find.jsx#processQrValue@4ec51b0b` membuat sesi saat kartu dipindai, lalu memakai `scanSessionResult.patientId` untuk menemukan pasien; BE `KioskScanSessionController.cs#scan-result` (`FindPatientAsync`) | Conflict | Lihat `KSK-CONF-002`. Sesi bukan hanya catatan untuk Laboratorium; ia **juga cara Step 1 mengenali pasien dari kartu**, dan ID-nya ditempel ke encounter poliklinik (`kioskScanSessionId`). | Tinggi — salah desain = sesi ganda atau kunjungan tanpa sesi |
| `KSK-CAP-022` | Sesi lab ditulis sekali | Laboratorium / Registration | BE `KioskScanSessionController.cs` (`scan-result` hanya create; `TargetService`, `HasPhysicianRequest` diisi saat create; tidak ada endpoint ubah); `KioskEncounterClosureService.cs` menyaring kunjungan berdasarkan `TargetService` | Ready to reuse | Sesuai `KSK-DEC-002`: kontrak tidak diubah. Jalur "buat sesi saat Review selesai bila belum ada" sudah ada di `handleReviewContinue`. | — |
| `KSK-CAP-023` | Handoff pasien antar-halaman tanpa URL | Kiosk (FE) | FE `use-kiosk-old-patient-registration.jsx#OLD_PATIENT_DEEP_LINK_PARAM` — deep link existing dari Pasien Baru memakai `?patientId=` di URL; `kiosk-new-patient-view.jsx:1062` | Extend | Pola existing melanggar `KSK-DEC-012` (tanpa `patientId` di URL). Handoff Cek No. RM butuh state memori (Redux/context). Deep link Pasien Baru sendiri di luar scope. | Sedang |
| `KSK-CAP-024` | Pencarian di Step 1 tanpa KTP/HP di URL & console | Kiosk (FE) | FE `lib/services/kiosk/registration/kiosk-old-patient-registration.service.js#requestJson` (`console.log` URL, diagnostic, **response body**) dan `searchOldPatients` (GET `?search=<teks>`) | Repair | Step 1 mengirim KTP/HP sebagai query string dan mencetak body respons (berisi KTP/HP/alamat) ke console browser. Bertentangan dengan PRIV-2/3 dan SEC-KSK-005 pada layar yang masuk scope. Lihat `KSK-CONF-004`. | Sedang |
| `KSK-CAP-025` | Pembersihan data sesi | Kiosk (FE) | FE `use-kiosk-old-patient-registration.jsx#resetRegistrationDraft, handleResetAll`; state hanya di `useState` (tidak ada `localStorage`/`sessionStorage` untuk data pasien) | Extend | Bersih saat selesai/Home/unmount. **Inactivity timeout tidak ada** (`KSK-DEC-010`). | — |
| `KSK-CAP-026` | Nama > 3 kata | PatientManagement / Kiosk | BE `MstPatient.FullName` maks. 200; FE tidak ada logika potong per kata (`rg` "split/words/slice(0, 3)" nol hasil di folder kiosk); pemotongan visual hanya `text-overflow: ellipsis` di CSS | Ready to reuse | Regresi visual belum dibuktikan di layar nyata (`KSK-UNK-003`). | Rendah |
| `KSK-CAP-027` | Bulan pendek pada tanggal lahir | Kiosk (FE) | FE `kiosk-old-patient-step-review.jsx:146` dan `kiosk-old-patient-step-confirm.jsx:326` memakai `formatDateId` (`month: "long"`); `formatShortDateId` (`month: "short"`) hanya dipakai untuk `issuedAt` kartu | Conflict | Lihat `KSK-CONF-003`. Klaim PRD KSK-BASE-002 ("existing sudah bulan pendek") tidak cocok dengan source. | Rendah |

### 2.3 Penjamin Utama

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `KSK-CAP-030` | Satu penanggung per kunjungan | Registration | BE `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs#PaymentType, PaymentSource@419b910f` dan `RegPatientEncounterGuarantor` (satu-ke-satu, `PatientInsuranceId` **atau** `PatientCompanyGuarantorId`, snapshot) | Ready to reuse | Menjawab `KSK-ASM-001`: **ya**, tidak butuh kolom baru. `KSK-DEC-008` (penjamin hanya untuk kunjungan) terpenuhi cukup dengan mengirim penjamin terpilih saat create encounter. | — |
| `KSK-CAP-031` | Kiosk boleh membuat kunjungan dengan Penjamin Perusahaan | Registration | BE `PatientEncounterController.cs#CreateEncounterForKiosk` → `allowCompanyGuarantor: false`; validasi menolak "Tipe pembayaran Penjamin Perusahaan hanya tersedia pada registrasi petugas."; kontrak `rawat-inap/episode-rawat-inap/contracts/encounter-company-guarantor-contract.md` §7 | Conflict | Lihat `KSK-CONF-001`. **Blocker utama rumpun Penjamin.** | Tinggi |
| `KSK-CAP-032` | Pilih penjamin di step Pembayaran | Kiosk (FE) | FE `kiosk-old-patient-step-payment.jsx` (tab Asuransi/Perusahaan, `selectedCompany`, `setPatient*PrimaryFromKiosk`); `use-kiosk-old-patient-registration.jsx#handlePaymentContinue` mengubah `company` → `insurance`; `kiosk-old-patient-registration.service.js#createOldPatientEncounter` melempar error untuk `company` | Repair | Hari ini pasien yang memilih Perusahaan **gagal di Konfirmasi**: `paymentKey` diubah menjadi `insurance` tanpa `selectedInsurance`, sehingga muncul error "Asuransi pasien belum dipilih". Tidak ada layar "Pilih Penjamin Utama" untuk Kondisi C. | Tinggi — defect existing |
| `KSK-CAP-033` | "Jadikan Utama" level pasien | PatientManagement | BE `PatientInsuranceController`/`PatientCompanyGuarantorController.cs#PATCH kiosk/{id}/primary@419b910f`; FE `setPatientInsurancePrimaryFromKiosk`, `setPatientCompanyGuarantorPrimaryFromKiosk` | Conflict | Bertentangan dengan `KSK-DEC-008` (Kiosk tidak boleh mengubah default pasien). Endpoint tetap ada; pemakaiannya di Kiosk harus dilepas. Apakah endpoint route `kiosk/` ditutup = keputusan desain (`KSK-OQ-006`). | Sedang |
| `KSK-CAP-034` | Daftar asuransi & perusahaan milik pasien | PatientManagement | BE `patient-insurances/kiosk/options`, `patient-company-guarantors/kiosk/options` (`KioskRead`); FE `OLD_PATIENT_ENDPOINTS.patientInsurances/patientCompanies` | Ready to reuse | Cukup untuk mendeteksi Kondisi A/B/C. | — |

### 2.4 Cek Jadwal Dokter

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `KSK-CAP-040` | Dropdown di atas Card Dokter | Kiosk (FE) / komponen dasar | FE `components/features/base-features/filter-select.jsx` + `style/components/features/base-features/filter-select.module.css` (`.root { position: relative; z-index: 1 }`, menu `position: absolute; z-index: 10001`, tanpa portal); `style/kiosk/registration/doctor-schedule/kiosk-doctor-schedule.module.css` (`.scheduleCard` `transform` saat hover, panel `backdrop-filter`) | Repair | Dugaan kuat: menu terjebak di stacking context `.root` (z-index 1), sehingga kalah oleh card yang membentuk stacking context sendiri. `FilterSelect`/`ResourceFilterSelect` dipakai di **33 berkas**, jadi perbaikan di komponen dasar berdampak luas. Perbaikan lokal di CSS jadwal dokter lebih aman. | Sedang (blast radius) |

## 3. Kontrak as-is yang relevan

### `GET /api/v1/health-services/patient-management/master-data/patients/kiosk`

| Bagian | Isi |
| --- | --- |
| Auth | `KioskRead` |
| Query | `search` (free-text), `startDate`, `endDate`, `customPeriod`, `defaultMembershipTierId`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` |
| 200 | `ApiResponse<ResponsePatientPagedResult>` — daftar pasien lengkap berhalaman |
| Catatan | Tidak cocok untuk KSK-RM-002: mencari di nama, wilayah, email, dll., dan nilai pencarian masuk URL. |

### `GET /api/v1/health-services/patient-management/master-data/patients/kiosk/{id}`

| Bagian | Isi |
| --- | --- |
| Auth | `KioskRead` |
| 200 | `ApiResponse<PatientDetailResponse>` — termasuk `Address`, `IdentityNumber`, `PhoneNumber`, data kelahiran, `Notes` |
| 404 | `Patient tidak ditemukan.` |
| Catatan | Dipakai untuk hidrasi Step 1/2. Terlalu lebar untuk layar Cek No. RM. |

### `POST /api/v1/health-services/registration-management/patient-encounters/kiosk`

| Bagian | Isi |
| --- | --- |
| Auth | `KioskRead` |
| Body | `PatientEncounterCreateRequest` — `patientId`, `paymentType` (1 Tunai, 2 Asuransi, 3 Penjamin Perusahaan), `patientInsuranceId`, `patientCompanyGuarantorId`, `kioskScanSessionId`, dll. |
| 400 | `paymentType = 3` → "Tipe pembayaran Penjamin Perusahaan hanya tersedia pada registrasi petugas." |
| Catatan | `POST /admin` (`PatientEncounter : Create`) menerima nilai 3. |

### `POST /api/v1/health-services/registration-management/kiosk-scan-sessions/kiosk/scan-result`

| Bagian | Isi |
| --- | --- |
| Auth | `KioskRead` |
| Body | `rawScanText`, `identityNumber`, `cardNumber`, `memberNumber`, `insuranceCardNumber`, `fullName`, `isManualInput`, `targetService` (angka), `hasPhysicianRequest` |
| 200 | Sesi baru; `patientId` terisi bila `FindPatientAsync` cocok (KTP/kartu asuransi/member — **bukan** No. RM) |
| Catatan | Tidak ada jalur ubah; `targetService` ditulis sekali. |

## 4. Ketidakcocokan frontend/backend

| ID | Ketidakcocokan |
| --- | --- |
| `KSK-MM-001` | FE menganggap backend encounter "hanya menerima Tunai atau Asuransi" (pesan di `createOldPatientEncounter`). Backend sudah mengenal `CompanyGuarantor = 3`, tetapi sengaja menolaknya di route kiosk. Pesan FE benar secara hasil, keliru secara alasan. |
| `KSK-MM-002` | FE step Pembayaran menawarkan tab Perusahaan yang **tidak bisa** menghasilkan kunjungan (`KSK-CAP-032`). |

## 5. Conflict

| ID | Conflict | Pihak | Pilihan yang terlihat (bukan keputusan) |
| --- | --- | --- | --- |
| `KSK-CONF-001` | PRD KSK-GUA-001 menuntut pasien dapat memilih **Perusahaan** sebagai penjamin utama di Kiosk. Kontrak approved `RWI-ENC-PAYER-001` v1.0.0 §7 (owner Muhammad Hamzah) menyatakan route kiosk **tidak** menerima Penjamin Perusahaan "supaya wewenang kiosk tidak ikut meluas". | Kiosk (Sukma) ↔ Rawat Inap (Muhammad Hamzah) | (a) Amendment `RWI-ENC-PAYER-001` → v1.1.0 membuka `CompanyGuarantor` pada route kiosk, butuh persetujuan Muhammad Hamzah; (b) Kiosk hanya menampilkan pilihan, dan bila Perusahaan dipilih pasien diarahkan ke petugas; (c) Tunda rumpun Penjamin sampai amendment. |
| `KSK-CONF-002` | `KSK-DEC-002` (tunda sesi sampai Tujuan Layanan) bertabrakan dengan fakta bahwa sesi dipakai Step 1 untuk mengenali pasien dari kartu dan ditempel ke encounter poliklinik. | Kiosk ↔ Laboratorium | (a) Step 1 mengenali kartu lewat pencarian biasa (tanpa sesi); hasil pindaian disimpan di memori; sesi dibentuk sekali di Step 3 untuk **semua** tujuan (poli & lab); (b) Sesi tanpa target tetap dibentuk di Step 1, lalu sesi kedua bertarget dibentuk bila Lab dipilih — berisiko satu pasien dua sesi; (c) Sesi di Step 1 hanya untuk poli, lab dibentuk di Step 3 — ketahuan poli/lab baru di Step 3, jadi tidak bisa dipilih di Step 1. Pilihan (a) paling selaras dengan `KSK-INV-007`. |
| `KSK-CONF-003` | PRD KSK-BASE-002 menyebut bulan pendek sebagai perilaku existing, tetapi Review Data dan Konfirmasi Pasien Lama menampilkan bulan panjang ("September"). Kartu Pasien tidak menampilkan tanggal lahir sama sekali. | PRD ↔ source | (a) Ubah Review/Konfirmasi ke bulan pendek (sesuai PRD); (b) Pertahankan bulan panjang dan koreksi PRD. |
| `KSK-CONF-004` | Step 1 Identifikasi (masuk scope) mengirim KTP/HP lewat query string dan mencetak body respons pasien ke console browser, bertentangan dengan PRIV-2/3 dan SEC-KSK-005. | Kiosk | (a) Perbaiki di rumpun Flow Pasien Lama (hapus `console.log`, pindahkan pencarian identitas ke endpoint lookup POST); (b) Catat sebagai tech-debt di luar revisi ini. |

## 6. Unknown

| ID | Hal yang belum diketahui | Cara menutup |
| --- | --- | --- |
| `KSK-UNK-001` | Pasien berstatus `Inactive`, `Deceased`, `Blacklisted`, atau `Merged` (`PatientStatus`, `MergedToPatientId`) masih memegang KTP/HP. Apakah Cek No. RM menganggapnya "ditemukan", "tidak ditemukan", atau "hubungi petugas"? | Keputusan bisnis (grill-me closure) |
| `KSK-UNK-002` | Variasi format `MstPatient.PhoneNumber` yang benar-benar tersimpan (spasi, strip, `+62`, `62`, `0`). Menentukan apakah normalisasi di query cukup atau perlu kolom turunan + backfill. | Query baca-saja ke `QuilvianNewDevSukma` — **butuh izin eksplisit** (eksekusi database) |
| `KSK-UNK-003` | Tampilan nama > 3 kata di kartu/review pada resolusi Kiosk nyata. | Verifikasi runtime saat build FE |
| `KSK-UNK-004` | Partisi rate limit: klaim mana yang stabil per perangkat (`kioskDeviceId` vs user id akun perangkat). | Trace `ResolveKioskLoginContextAsync` saat desain |

## 7. Pertanyaan penutup

> **Status penutupan (30 Sep 2026):** 1 → `KSK-DEC-013`; 2 → `KSK-DEC-014`; 3 → `KSK-DEC-015`; 4 → `KSK-DEC-016`; 5 → `KSK-DEC-017`; 6 → query baca-saja dijalankan, `KSK-FACT-006..008`, `KSK-DEC-018/019`.

1. **`KSK-CONF-001`** — jalur mana untuk penjamin Perusahaan di Kiosk? Pemilik: Sukma, dengan persetujuan Muhammad Hamzah bila memilih amendment.
2. **`KSK-CONF-002`** — kapan sesi kiosk dibentuk untuk poliklinik dan laboratorium?
3. **`KSK-CONF-003`** — bulan pendek atau panjang di Review/Konfirmasi?
4. **`KSK-CONF-004`** — perbaiki kebocoran KTP/HP di Step 1 dalam revisi ini atau tidak?
5. **`KSK-UNK-001`** — perilaku lookup untuk pasien non-aktif/meninggal/diblokir/digabung.
6. **`KSK-UNK-002`** — izin menjalankan query baca-saja ke `QuilvianNewDevSukma` untuk melihat variasi format HP.

## 8. Ringkasan per rumpun

| Rumpun | Kesiapan dari sisi bukti |
| --- | --- |
| Lookup No. RM | Bisa didesain. Endpoint, DTO minimal, normalisasi HP, dan rate limit adalah pekerjaan baru. Jalur HP menunggu `KSK-UNK-002`; perilaku status pasien menunggu `KSK-UNK-001`. |
| Flow Pasien Lama | Bisa didesain setelah `KSK-CONF-002` diputuskan. |
| Penjamin Utama | **Terblokir** oleh `KSK-CONF-001`. Defect existing `KSK-CAP-032` tetap terjadi sampai diputuskan. |
| Jadwal Dokter | Siap didesain dan dikerjakan mandiri (FE saja). |
