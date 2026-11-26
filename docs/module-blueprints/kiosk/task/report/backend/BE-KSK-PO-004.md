# Laporan Perubahan Backend — `BE-KSK-PO-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-004` |
| Judul | Opsi publik dan pencocokan pasien lama |
| Slice | `EPIC KSK-PO-02` — halaman publik backend (`FR-KSK-PO-010`, `011`, `014`), gelombang teknis 3, rilis `MVP-1` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-004` |
| Trace | `KSK-DEC-031`, `038`, `047`; `KSK-PO-FE-002`; `KSK-DSN-PO-005`, `006`, `013`; `KSK-VAL-PO-001` s.d. `004`, `010` s.d. `012`, `020`; `CAP-PO-02`, `03`, `09`, `11` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | `BE-KSK-PO-002` 🟡 (hitungan kuota dan aturan jadwal tersedia di source), `BE-KSK-PO-003` ✅ |
| Klasifikasi | `MEDIUM` — endpoint publik baru tanpa login, dua policy batas laju, pemakaian ulang logika pencocokan kiosk |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Controllers,DTOS,Enums,Services}`, `Helpers/`, `Program.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a86001e2` (`sukmagp`) |
| Tanggal | 2026-11-19 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di runtime |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` (controller, service, DTO, enum, helper) dan `TOUCHED LEGACY` (`QueueDisplayRuntimeController`, `PatientEncounterController`, `KioskPatientLookupService`, `DoctorScheduleQuotaService`) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`, seluruh logika di service), `QBE-API-001`/`QBE-DTO-001` (route `api/v1/...`, `ApiResponse<T>`, DTO respons field minimum), `QBE-LOG-001` (log tanpa data pribadi), `QBE-PERM-001` (endpoint publik sengaja tanpa `[AccessController]`, sama dengan `PriorityReservationArrivalController`; verifier otorisasi tetap PASS) |
| Hardcode role | Tidak ada. Endpoint publik tidak memakai role sama sekali |

## 1. Masalah yang diperbaiki

Halaman publik Pendaftaran Pasien Online butuh data untuk diisi pasien yang tidak login, tetapi semua endpoint pilihan yang ada (poli, jadwal, kuota, kartu asuransi, faskes) memerlukan login sebagai akun kiosk atau petugas (`CAP-PO-09`). Pencocokan pasien kiosk yang ada juga tidak bisa dibuka ke internet, karena tidak memeriksa tanggal lahir dan mengembalikan kartu pasien utuh (`CAP-PO-11`). Task ini membuat versi publik yang sempit untuk keduanya.

## 2. Proses bisnis

1. Pasien membuka halaman online dan memilih tanggal kunjungan. Hanya besok sampai 30 hari ke depan yang diterima. Contoh: memilih hari ini dijawab "Pendaftaran online hanya untuk besok sampai 30 hari ke depan. Untuk berobat hari ini, silakan daftar di kiosk rumah sakit."
2. Halaman menampilkan poli yang punya jadwal online pada tanggal itu, lalu jadwal dokternya beserta sisa kuota appointment. Jadwal yang tidak mengizinkan appointment **atau** pendaftaran kiosk tidak ditawarkan, karena check-in akan terjadi di kiosk (`KSK-DSN-PO-013`). Aturan "jadwal berlaku pada tanggal itu" sama persis dengan jalur pembuatan kunjungan.
3. Pasien lama mengisi NIK atau No. RM dan tanggal lahir, lalu menyelesaikan captcha. Bila cocok, halaman hanya menampilkan nama tersamar (contoh "R*** U** G* C*****") dan kartu asuransi/perusahaan miliknya dengan nomor tersamar (contoh "****1234"). Bila tidak cocok, salah tanggal lahir, pasien non-aktif, atau pasien sudah digabung, jawabannya **sama persis**: "Data tidak ditemukan. Periksa kembali isian Anda atau daftar di loket pendaftaran."
4. Jalur tidak normal:
   - Captcha kosong atau ditolak → `400` "Verifikasi keamanan gagal. Silakan ulangi."
   - Cloudflare tidak terjangkau → `503` "Layanan sedang tidak tersedia. Silakan coba lagi atau daftar di loket pendaftaran."
   - Lebih dari 20 pencocokan per jam dari satu jaringan → `429` "Terlalu banyak percobaan. Silakan coba lagi nanti."
   - Lebih dari 60 permintaan opsi per menit dari satu jaringan → `429` dengan pesan yang sama.
   - Fitur dimatikan (`OnlineRegistration:Enabled = false`) → `503`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PriorityReservationArrivalController.cs` (pola endpoint publik); `KioskPatientLookupService.cs` (pencarian NIK, rantai gabung, kelayakan); `QueueDisplayRuntimeController.cs:538` (penyamar nama); `PatientEncounterController.cs` (`IsDoctorScheduleValidForVisitDate`); `DoctorScheduleController.cs` (filter jadwal kiosk); `PatientInsuranceController.cs` (filter kartu yang dapat dipakai); model `MstDoctorSchedule`, `MstInsuranceProvider`, `MstReferralInstitution`, `MstPatientInsurance`, `MstPatientCompanyGuarantor`, `MstCompanyGuarantor`, `MstPatient`; `Program.cs` (rate limiter dan `OnRejected`); `tools/authorization-verifier/README.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/OnlineRegistrationPublicController.cs` | **Baru.** `[AllowAnonymous]`, tanpa `[AccessController]`, `[Tags("Health Services / Registration Management / Online Registration Public")]`, lima endpoint, `Cache-Control: no-store`, policy batas laju per action |
| `.../Services/OnlineRegistrationPublicService.cs` | **Baru.** Opsi poli, jadwal + sisa kuota, penyedia asuransi, faskes; pencocokan pasien lama dengan captcha; `FindExistingPatientIdAsync` dan `IsVisitDateAllowed` disiapkan untuk dipakai ulang `BE-KSK-PO-005` |
| `.../DTOS/OnlineRegistrationPublicDtos.cs` | **Baru.** DTO opsi dan pencocokan, field minimum |
| `.../Enums/OnlineRegistrationIdentityType.cs`, `OnlineRegistrationPayerKind.cs` | **Baru.** NIK/No. RM; Asuransi/Perusahaan |
| `Helpers/PatientNameMasker.cs` | **Baru.** Penyamar nama, dipindah apa adanya dari display antrean |
| `.../Controllers/QueueDisplayRuntimeController.cs` | `MaskPatientName` memanggil `PatientNameMasker.Mask`; perilaku sama (AC 7) |
| `.../Services/KioskPatientLookupService.cs` | Dua method publik baru: `FindPatientIdsByIdentityNumberAsync` dan `ResolveSingleEligiblePatientIdAsync`, memakai logika privat yang sudah ada. `LookupAsync` tidak berubah |
| `.../Services/DoctorScheduleQuotaService.cs` | `IsScheduleValidForVisitDate` (dipindah dari controller tanpa perubahan) |
| `.../Controllers/PatientEncounterController.cs` | `IsDoctorScheduleValidForVisitDate` kini memanggil `DoctorScheduleQuotaService.IsScheduleValidForVisitDate` |
| `Program.cs` | `AddScoped<OnlineRegistrationPublicService>`; policy `OnlineRegistrationPublicOptions` (per IP, 60/menit) dan `OnlineRegistrationPublicMatch` (per IP, 20/jam) dari konfigurasi; cabang `OnRejected` dengan pesan publik dan log tanpa IP |

**Delta terhadap desain (tidak mengubah kontrak):**

| Desain | Source | Alasan |
| --- | --- | --- |
| DTO di `OnlineRegistrationDtos.cs` | `OnlineRegistrationPublicDtos.cs` | DTO publik dipisah agar DTO petugas (`BE-KSK-PO-009`) tidak tercampur dengan kontrak tanpa login |
| "Path disamarkan di log" | Tidak diperlukan | Endpoint ini tidak membawa data rahasia di path; data pasien hanya lewat body `POST` |
| — | Endpoint opsi poli dan jadwal memvalidasi rentang tanggal H+1 s.d. H+30 | Mencegah halaman publik membaca jadwal di luar rentang pendaftaran (`KSK-VAL-PO-020`) |
| — | Dua enum kecil baru | Kontrak menyebut `identityType` 1/2 dan `payerKind` asuransi/perusahaan; dibuat sebagai enum bernama |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: grup *Online Registration Public*, lima endpoint sesuai `KSK-CONTRACT-v2`. Endpoint lain tidak berubah |
| Database | `NOT APPLICABLE` — hanya membaca, tanpa migration |
| Keamanan/Auth | Endpoint pertama di modul ini yang terbuka ke internet. Penjagaan: batas laju per IP, captcha pada pencocokan, `404` seragam, data keluar tersamar, `no-store`, log tanpa NIK/No. RM/tanggal lahir/IP. Akses internet dan penerusan IP asli tetap menunggu IT (`KSK-PO-OQ-010`) |

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Online Registration Public

Base URL: `api/v1/health-services/registration-management/online-registrations/public`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options/clinics?visitDate=` | Poli yang punya jadwal online pada tanggal itu (`id`, `name`) | Tanpa login, `OnlineRegistrationPublicOptions` |
| `GET` | `/options/doctor-schedules?clinicId=&visitDate=` | Jadwal dokter (`id`, `doctorName`, `startTime`, `endTime`, `sessionName`, `remainingAppointmentQuota`, `isFull`) | Tanpa login, `OnlineRegistrationPublicOptions` |
| `GET` | `/options/insurance-providers` | Penyedia asuransi aktif (`id`, `name`) | Tanpa login, `OnlineRegistrationPublicOptions` |
| `GET` | `/options/referral-institutions?search=` | Faskes perujuk, minimal 3 huruf, maks. 20 baris (`id`, `code`, `name`) | Tanpa login, `OnlineRegistrationPublicOptions` |
| `POST` | `/patient-match` | Pencocokan pasien lama (`maskedName`, `payers[]`) | Tanpa login, `OnlineRegistrationPublicMatch`, captcha |

Kode status: `200` berhasil; `400` isian tidak sah, tanggal di luar H+1 s.d. H+30, atau captcha ditolak; `404` data pasien tidak cocok (pesan seragam); `429` terlalu banyak percobaan; `503` captcha tidak dapat diperiksa atau fitur dimatikan.

## 5. Verifikasi

Backend baru dijalankan dari scratchpad di `https://localhost:7185` terhadap `QuilvianNewDevSukma`, dengan kunci uji resmi Cloudflare lewat variabel lingkungan (tidak ditulis ke berkas). Backend pengguna di 7184 tidak disentuh.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (scratchpad dan `-c Release`) | Build succeeded, 0 error | `PASS` | Keluaran build |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | PASS: metadata gap 0, endpoint telanjang baru 0, identitas wajib 24/24 | `PASS` | Keluaran verifier |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| A1–A3 opsi poli tanpa login; hari ini dan H+31 ditolak | `200`; `400`; `400` | `PASS` | `po004_test.py` |
| B1 jadwal publik ⊆ jadwal aktif + appointment + kiosk + berlaku di tanggal itu, dibandingkan dengan data petugas | 14 poli diperiksa (H+1, H+7), selisih 0 | `PASS` | `po004_test.py` |
| B2a–B2d dengan jadwal uji: tanpa izin kiosk tidak muncul; tanpa izin appointment tidak muncul; appointment + kiosk muncul dengan sisa kuota 2; tidak muncul di luar masa berlaku | 4/4 | `PASS` | `po004_test2.py filter` |
| C1–C5 field minimum (poli, jadwal, penyedia, faskes); faskes < 3 huruf `400`; maks. 20 baris | 5/5 | `PASS` | `po004_test.py` |
| D1 NIK + tanggal lahir benar | `200`, nama tersamar, respons hanya `maskedName` dan `payers`, NIK/No. RM/nama asli tidak muncul | `PASS` | `po004_test.py` |
| D1c pasien yang punya kartu asuransi | Kartu muncul dengan nomor `****…`, nomor utuh tidak ada di respons | `PASS` | `po004_test2.py filter` |
| D2–D3 No. RM bertanda hubung dan angka saja | `200` keduanya | `PASS` | `po004_test.py` |
| D4 tanggal lahir salah, NIK tak dikenal, No. RM tak dikenal | Ketiganya `404` dengan isi respons identik | `PASS` | `po004_test.py` |
| D5 NIK bukan 16 digit; D7 tanggal lahir di masa depan | `400` | `PASS` | `po004_test.py` |
| D6 captcha kosong | `400` "Verifikasi keamanan gagal. Silakan ulangi." | `PASS` | `po004_test.py` |
| D8 Cloudflare tidak terjangkau (`Turnstile__VerifyUrl` tak terjangkau) | `503`, `data` kosong | `PASS` | `po004_test2.py down` |
| E1 header | `Cache-Control: no-store` | `PASS` | `po004_test.py` |
| F1 batas laju pencocokan | `429` tepat pada permintaan ke-21 dalam satu jam | `PASS` | `po004_test.py` |
| AC 7 penyamar nama | `PatientNameMasker.Mask` identik dengan `MaskPatientName` lama untuk 14 contoh | `PASS` | Harness scratchpad |
| Log | 72 baris log terkait; 0 baris memuat NIK, tanggal lahir, atau nilai identitas; yang tercatat hanya hasil dan jenis identitas | `PASS` | Pemeriksaan log backend uji |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

**Data uji:** 3 jadwal dokter `TEST-PO004 …` untuk Poli Paru pada H+1 dibuat lewat API dan sudah dinonaktifkan. Pencocokan pasien hanya membaca data pasien uji (`RJPR UJI*`), tanpa menulis apa pun.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Endpoint dapat dipanggil tanpa token | Terpenuhi | A1, C3, D1 (klien tanpa login) |
| (2) Jadwal hanya yang `IsAllowAppointment` dan `IsAllowKioskRegistration`, dengan sisa kuota dari `BE-KSK-PO-002` | Terpenuhi | B1, B2a–B2c |
| (3) Respons hanya field minimum kontrak | Terpenuhi | C1–C5, D1 |
| (4) Cocok → nama dan nomor kartu tersamar; salah/non-aktif/gabung → `404` identik | Terpenuhi | D1, D1c, D4. Non-aktif dan gabung memakai `ResolveSingleEligiblePatientIdAsync`, yaitu aturan yang sama dengan Cek No. RM Kiosk |
| (5) Captcha gagal `400`; Cloudflare mati `503` | Terpenuhi | D6, D8 |
| (6) Batas 20/jam → `429` | Terpenuhi | F1 |
| (7) Display antrean tetap menyamarkan nama persis seperti sebelumnya | Terpenuhi | Harness AC 7 |
| DoD: QBE Strict dan verifier PASS | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Batas laju memakai IP koneksi. Di balik proxy tanpa penerusan IP asli, semua pasien terhitung satu IP (`KSK-PO-OQ-010`, IT) |
| Masalah yang diketahui | D4 tidak menguji pasien non-aktif/gabung secara langsung karena data dev tidak menyediakan pasien uji seperti itu yang aman dipakai. Jalurnya memakai method yang sama dengan Cek No. RM Kiosk yang sudah teruji (`KSK-AC-014`, `015`) |
| Risiko tersisa | Endpoint terbuka ke internet saat go-live. Isi kunci Turnstile produksi dan pastikan HTTPS keluar ke Cloudflare diizinkan (`KSK-PO-OQ-016`) |
| Perubahan sampingan | `NONE` di repository. Keluaran build, skrip uji, dan log berada di scratchpad; `bin/Release` tidak dilacak |
| Interupsi | Penghentian proses backend uji lewat pola nama proses ditolak sistem; diganti dengan menghentikan task latar belakangnya sendiri. Backend pengguna tidak tersentuh |
| Status Git | Milik task ini: 5 berkas diubah (`QueueDisplayRuntimeController.cs`, `PatientEncounterController.cs`, `KioskPatientLookupService.cs`, `DoctorScheduleQuotaService.cs`, `Program.cs`) dan 6 berkas baru (controller, service, DTO, dua enum, `Helpers/PatientNameMasker.cs`), ditambah perubahan task sebelumnya yang belum di-commit |
| Langkah berikutnya | `BE-KSK-PO-005` (kirim pendaftaran online). Frontend `FE-KSK-PO-001` kini dapat dimulai |
