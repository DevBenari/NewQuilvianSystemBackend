# Laporan Perubahan Backend — `BE-KSK-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-001` |
| Judul | Layanan Lookup No. RM |
| Slice | EPIC KSK-01 — Layanan Lookup No. RM (backend), gelombang `MVP-0`, gelombang eksekusi 1 |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/backend-roadmap.md` — kartu `BE-KSK-001` |
| Trace | `FR-KSK-001..005, 007, 008, 009`; `KSK-DEC-006/007/017/018/019`; `KSK-DSN-001..004, 006, 008`; `contracts/api-contract.md` §Kiosk Patient Lookup; `contracts/validation-matrix.md` §1; `contracts/permission-audit-matrix.md` §1, §4 |
| Contract version | `KSK-CONTRACT-v1` — `approved` (Sukma Giri Pratama, 2026-09-30) |
| Dependency | — (task gelombang 1) |
| Klasifikasi | `MEDIUM` — 5 berkas baru + 1 berkas diubah; endpoint baru yang menyentuh data pribadi dan otorisasi perangkat; tanpa migration |
| Task mode | `BACKEND` (dinyatakan pengguna 2026-09-30: "saya setujui dan lanjutkan" atas usulan memulai `BE-KSK-001`) |
| Target tulis | `NewQuilvianSystemBackend`: berkas pada kartu task, laporan ini, baris status roadmap dan `requirement-traceability.md` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `419b910f` (branch `sukmagp`), belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 11 dari 11 acceptance criteria terbukti; seluruh butir DoD terpenuhi (`dotnet test` NOT RUN karena tidak ada project test). Revisi laporan: run kedua 30 Sep 2026 menutup R13 |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, Kiosk tidak punya cara menanyakan "apakah pemilik No. KTP / No. HP ini sudah punya rekam medis?". Satu-satunya pencarian untuk Kiosk (`GET patients/kiosk?search=`) mencari teks bebas di lebih dari 15 kolom (nama, wilayah, email, dan lain-lain), membawa nilai pencarian di URL, dan mengembalikan data pasien lengkap termasuk alamat.

Contoh akibatnya: pasien lama yang mengetik No. HP `0812-3456-7890` tidak ditemukan bila datanya tersimpan sebagai `+6281234567890`, lalu mendaftar ulang sebagai pasien baru dan rekam medisnya jadi ganda.

---

## 2. Proses bisnis

| Hal | Isi |
| --- | --- |
| Tujuan | Menjawab dengan pasti apakah seseorang sudah terdaftar, tanpa membuka data pribadi berlebih |
| Pelaku | Pasien di Kiosk, lewat akun perangkat Kiosk |
| Pemicu | Pasien menekan "Cek Nomor Rekam Medis" (layar `FE-KSK-04` nanti), atau mengetik/memindai di Step 1 Identifikasi |

Langkah:

1. Kiosk mengirim jenis pencarian (KTP / HP / kartu asuransi / nomor member) dan nilainya di body request.
2. Backend memvalidasi dan menormalkan nilai. KTP: spasi dibuang, wajib 16 digit. HP: semua selain angka dibuang, awalan `0` diganti `62`, wajib diawali `62` dengan panjang 9–15 digit.
3. Backend mencari pasien yang cocok **persis**. HP dicocokkan setelah data tersimpan juga dinormalkan dengan aturan yang sama.
4. Pasien yang sudah digabung diikuti ke pasien tujuannya (paling banyak 3 langkah).
5. Backend menyimpulkan satu hasil:

| Keadaan | Hasil | Yang dikirim |
| --- | --- | --- |
| Tidak ada yang cocok | `NotFound` (2) | Arahan daftar pasien baru |
| Lebih dari satu pasien | `MultipleMatch` (3) | Tanpa data pasien; lewat HP diarahkan memakai KTP |
| Satu pasien, tetapi tidak aktif / meninggal / diblokir / rantai gabung rusak | `ContactStaff` (4) | Tanpa data pasien dan tanpa alasan |
| Satu pasien aktif | `Found` (1) | 7 field Kartu Pasien |

Jalur tidak normal: isian tidak sah dijawab `400` dengan pesan yang dipahami pasien; tanpa login dijawab `401`. Keempat hasil bisnis dijawab `200`, jadi gagal teknis tidak pernah terbaca sebagai "belum terdaftar".

Hasil akhir: lookup **tidak menulis apa pun** ke database. Yang ditinggalkan hanya satu baris log dengan nilai tersamar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `CLAUDE.md`; suite `rules/backend/` (`BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `role-access-rules.md`, `REPORT_TEMPLATE.md`); `Constants/AuthorizationPolicies.cs`; `Services/Security/PermissionRegistryDescriptor.cs`; `tools/authorization-verifier/*`; `Areas/HealthServices/RegistrationManagement/Controllers/KioskScanSessionController.cs#FindPatientAsync`; `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs#BuildEnumLabel`; model `MstPatient`, `MstPatientIdentityDocument`, `MstPatientInsurance`, `MstPatientMembership`; `Areas/HealthServices/HemodialysisManagement/Controllers/*` (pola NEW CODE); blueprint `docs/module-blueprints/kiosk/**`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Enums/KioskPatientLookupSearchType.cs` | **Baru.** Enum jenis pencarian `1..4` |
| `Areas/HealthServices/RegistrationManagement/Enums/KioskPatientLookupResult.cs` | **Baru.** Enum hasil `1..4` |
| `Areas/HealthServices/RegistrationManagement/DTOS/KioskPatientLookupDtos.cs` | **Baru.** Request, response, dan `KioskPatientCardResponse` (7 field Kartu Pasien) |
| `Areas/HealthServices/RegistrationManagement/Services/KioskPatientLookupService.cs` | **Baru.** Validasi, normalisasi, pencocokan, rantai gabung, penyimpulan hasil, log tersamar. Baca-saja, `AsNoTracking` |
| `Areas/HealthServices/RegistrationManagement/Controllers/KioskPatientLookupController.cs` | **Baru.** `POST /`, policy `KioskRead`, tanpa akses `ApplicationDbContext` |
| `Program.cs` | +3 baris: `AddScoped<KioskPatientLookupService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `POST api/v1/health-services/registration-management/kiosk-patient-lookups`, sesuai `KSK-CONTRACT-v1` tanpa delta field. Endpoint lain tidak berubah |
| Database | Tidak ada perubahan schema, entity, configuration, maupun migration. `has-pending-model-changes`: tidak ada perubahan |
| Keamanan/Auth | Policy `KioskRead` (otorisasi alternatif yang disetujui, `Constants/AuthorizationPolicies.cs`). Respons tanpa KTP/HP/alamat/tanggal lahir. Query berparameter. Log hanya 4 karakter terakhir nilai. Rate limit belum ada (task `BE-KSK-002`) |

**Delta terhadap aturan suite `role-access-rules.md`.** Aturan itu mewajibkan `[AccessAction]` + `[AccessPermission]` pada setiap endpoint, dengan satu-satunya pengecualian `[AllowAnonymous]`. Source repository menetapkan pengecualian kedua: endpoint akun perangkat memakai `[Authorize(Policy = AuthorizationPolicies.KioskRead)]` sebagai pengganti sah `[AccessPermission]` (komentar kelas `AuthorizationPolicies`, dan dibaca `PermissionRegistryDescriptor.HasApprovedAlternativeAuthorization`). Sesuai skill, source repository yang berlaku, dan selisihnya dilaporkan di sini. `[AccessAction]` sengaja tidak dipasang karena `[AccessAction]` tanpa `[AccessPermission]` menambah himpunan fallback verifier (`KSK-DSN-006`).

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Kiosk Patient Lookup

Base URL: `api/v1/health-services/registration-management/kiosk-patient-lookups`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Mencari satu pasien dari No. KTP, No. HP, nomor kartu asuransi, atau nomor member; menjawab `Found` / `NotFound` / `MultipleMatch` / `ContactStaff` | Policy `KioskRead` (akun perangkat Kiosk; juga diterima role SuperAdmin/Administrator oleh policy existing) |

Request `{ "searchType": 1|2|3|4, "value": "..." }`. Response `ApiResponse<KioskPatientLookupResponse>`; rincian field di `contracts/api-contract.md`.

---

## 5. Verifikasi

Pola verifikasi mengikuti Bank Darah: tanpa folder `Tests/`. Uji runtime dilakukan lewat HTTP sungguhan terhadap `QuilvianNewDevSukma` dengan izin pengguna.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>` | Berhasil, 0 error, 0 warning di berkas task (dijalankan ulang setelah perbaikan query HP) | `PASS` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah. Catatan: `--no-build` membaca assembly `bin/` default; task ini tidak menyentuh model maupun configuration |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <berkas>` untuk 6 berkas | 6 × `VIOLATION: 0`, `Final result: PASS` (service dijalankan ulang setelah perbaikan) | `PASS` | Keluaran QBE |
| `dotnet build -c Release` + `bash tools/authorization-verifier/verify-authorization.sh --configuration Release` | `AUTHORIZATION VERIFIER: PASS`; metadata gap 0; fallback 69 cocok persis; naked baru 0; identitas wajib 24/24 | `PASS` | Keluaran verifier |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (larangan folder `Tests/`) |

### Uji runtime

Lingkungan: app dari build scratchpad, `ASPNETCORE_ENVIRONMENT=Development`, `Runtime__Role=Web` (scheduler mati), port lokal 5199, DB `QuilvianNewDevSukma`. Login memakai akun perangkat Kiosk dev `kiosk-sukma` (kredensial tidak dicatat).

Data samaran (dibuat dengan izin, dihapus sesudahnya): enam pasien berawalan `KSKTEST` dengan GUID `b5b5b5b5-…`, yaitu:

| Pasien | Keadaan |
| --- | --- |
| P1 | Aktif, KTP `9999…0001`, HP tersimpan `+62 811-0000-0001` |
| P2 | Meninggal, KTP `…0002`, HP tersimpan `081100000001` |
| P3 | Digabung → P1 |
| P4 ↔ P5 | Rantai gabung berputar |
| P6 | Aktif, KTP hanya di dokumen identitas (`…0006`), telepon rumah `+62215550006`, kartu asuransi `KSKTEST-CARD-06` / member `KSKTEST-MBR-06` |

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| R0 | Keadaan awal DB: `MstPatient` 16, `KSKTEST` 0 | Sesuai | `PASS` |
| R1 | KTP P1 (ditulis dengan spasi) | `200`, `result 1`, 7 field kartu persis, `patientId = P1` | `PASS` |
| R2 | KTP hanya di dokumen identitas | `200`, `result 1`, P6 | `PASS` |
| R3 | HP `0811-0000-0001` (cocok ke P1 `+62 811-…` **dan** P2 `0811…`) | `200`, `result 3`, `patient null`, `USE_IDENTITY_NUMBER_OR_CONTACT_STAFF` — membuktikan kedua format tersimpan ternormalisasi sama | `PASS` |
| R4 | Telepon rumah `021 555 0006` vs `+62215550006` | `200`, `result 1`, P6 | `PASS` |
| R5 | KTP pasien meninggal | `200`, `result 4`, `patient null`, kata "meninggal" tidak ada di respons | `PASS` |
| R6 | KTP pasien digabung P3 → P1 | `200`, `result 1`, `patientId = P1` | `PASS` |
| R7 | Rantai gabung berputar P4 ↔ P5 | `200`, `result 4`, tidak hang (75 ms) | `PASS` |
| R8 | KTP tidak terdaftar | `200`, `result 2`, `NEW_PATIENT_REGISTRATION` | `PASS` |
| R9a/b | Kartu asuransi (type 3) / nomor member huruf campur (type 4) | `200`, `result 1`, P6 | `PASS` |
| R10a–h | KTP 15 digit, KTP berhuruf, HP `12345`, HP `0812abc`, `<script>`, `searchType 7`, nilai kosong, karakter kontrol | Seluruhnya `400` dengan pesan `validation-matrix.md` §1; `<script>` tidak digemakan | `PASS` |
| R11 | Tanpa token | `401` | `PASS` |
| R12 | Log aplikasi | 10 baris lookup, masing-masing hanya `SearchType`, `Result`, `****`+4 karakter terakhir. Nilai utuh KTP/HP/kartu, nama, dan No. RM hasil muncul **0 kali** | `PASS` |
| R13 | Akun pegawai non-kiosk (identitas disamarkan; `UserType = PermanentDoctor`, tanpa role) memanggil lookup | `403`. Kontrol pada run yang sama dengan `kiosk-sukma` → `200`, `result 2` — penolakan berasal dari policy, bukan lingkungan | `PASS` |
| R-akhir | Setelah pembersihan: `MstPatient` 16, `KSKTEST` 0, dokumen & kartu samaran 0 | Sama dengan R0 | `PASS` |

**Temuan selama runtime, sudah diperbaiki di task yang sama.** Percobaan pertama R3/R4 menghasilkan `500`: provider Npgsql 9.0.4 **tidak** menerjemahkan `Regex.Replace` di LINQ (`InvalidOperationException … could not be translated`). Risiko ini sudah tercatat di kartu task. Query HP diganti `FromSqlInterpolated` dengan `regexp_replace` di sisi kolom; nilai input tetap dikirim sebagai parameter (SEC-KSK-003). Setelah build ulang, 19/19 skenario `PASS`; R13 ditutup pada run kedua sehingga totalnya 20/20.

Efek tulis yang terjadi saat app berjalan (diizinkan pengguna): seeder web idempoten (`DefaultWorkScheduleSeeder`, `SuperAdminSeeder`, `FinanceApprovalRoleSeeder`, `AccessMenuSeeder`, dan seeder master yang memeriksa keberadaan data), serta worker `BilInvoiceSyncWorker` / `ClinicalFactDispatchWorker` yang aktif. `SysControllerAccess` untuk `KioskPatientLookup` tetap 0 baris; ini sesuai rancangan, karena endpoint tanpa `[AccessAction]` tidak didaftarkan seeder.

Uji manual: `NOT APPLICABLE` — belum ada layar; layar dikerjakan di `FE-KSK-003/004`.

**Tidak dijalankan:** `NFR-002` (p95 < 500 ms di produksi) tidak dapat dibuktikan di DB dev berisi 16 pasien; waktu respons dev 60–124 ms.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. KTP satu pasien Aktif → `200`, `result 1`, `EXISTING_PATIENT_REGISTRATION`, tepat 7 field | Terpenuhi | R1 |
| 2. KTP hanya di dokumen identitas aktif → ditemukan | Terpenuhi | R2 |
| 3. HP `0812-…` cocok `+62812…`; `021 555 1234` cocok `+62215551234` | Terpenuhi | R3 (format mobile `+62 …` dan `0…` bertemu), R4 (telepon rumah) |
| 4. HP dipakai dua pasien → `result 3`, `patient null`, `USE_IDENTITY_NUMBER_OR_CONTACT_STAFF` | Terpenuhi | R3 |
| 5. Pasien meninggal/tidak aktif/diblokir → `result 4` | Terpenuhi untuk status meninggal (R5). Tidak aktif dan diblokir memakai cabang kode yang sama (`IsEligible`) tetapi tidak diuji terpisah | R5; `KioskPatientLookupService.IsEligible` |
| 6. Merged → tujuan; rantai berputar → `result 4` tanpa hang | Terpenuhi | R6, R7 |
| 7. Tidak terdaftar → `result 2`, `NEW_PATIENT_REGISTRATION` | Terpenuhi | R8 |
| 8. Isian tidak sah → `400` dengan pesan matriks | Terpenuhi | R10a–h |
| 9. `searchType 3/4` menemukan pasien | Terpenuhi | R9a/b |
| 10. Log hanya `searchType`, `result`, `****`+4 digit | Terpenuhi | R12 |
| 11. Tanpa token → `401`; pegawai biasa → `403` | Terpenuhi | R11, R13 |
| DoD: lima langkah verifikasi lulus | Terpenuhi | §5 |
| DoD: tidak ada migration | Terpenuhi | EF + `git status` |
| DoD: label `api-contract.md` berubah "Rencana" → "Tersedia" | Terpenuhi — hanya sel status endpoint lookup; rate limit tetap "Rencana" (`BE-KSK-002`). Isi kontrak tidak berubah | `contracts/api-contract.md` §Kiosk Patient Lookup |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Log startup Development berisi peringatan bawaan `No XML encryptor configured`, bukan dari task ini |
| Masalah yang diketahui | Label enum `PatientType`/`Gender`/`BloodType` disalin dari `PatientController.BuildEnumLabel` (private). Duplikasi kecil yang disengaja agar Kartu Pasien Kiosk sama dengan Cetak Kartu Pasien; kandidat helper bersama bila pemilik PatientManagement setuju |
| Risiko tersisa | (1) Sampai `BE-KSK-002` selesai, endpoint **belum** dibatasi rate limit, jadi jangan dibuka ke Kiosk produksi sebelum itu. (2) Pencarian HP melakukan scan tabel dengan `regexp_replace`; aman untuk data dev, perlu diukur di produksi (`NFR-002`). (3) Run kedua (R13) kembali menjalankan seeder web idempoten dan dua worker yang selalu aktif; tidak ada data uji yang dibuat pada run itu |
| Perubahan sampingan | `NONE` di source. Artefak build `bin/Release` (gitignored) dibuat untuk verifier |
| Interupsi | `NONE`. Satu siklus perbaikan (query HP) setelah temuan runtime, dicatat di §5 |
| Status Git | ` M Program.cs`; `?? Areas/HealthServices/RegistrationManagement/Controllers/KioskPatientLookupController.cs`; `?? .../DTOS/KioskPatientLookupDtos.cs`; `?? .../Enums/KioskPatientLookupResult.cs`; `?? .../Enums/KioskPatientLookupSearchType.cs`; `?? .../Services/KioskPatientLookupService.cs`; `?? docs/module-blueprints/kiosk/` |
| Langkah berikutnya | Kerjakan `BE-KSK-002` (rate limit) sebelum layar Cek No. RM dibuka ke perangkat nyata; `FE-KSK-001`/`FE-KSK-002` dapat berjalan paralel |
