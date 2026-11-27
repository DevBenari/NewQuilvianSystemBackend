# Laporan Perubahan Backend — `FIN-HRD-REC-WIZ-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Instruksi | `FIN-HRD-REC-WIZ-01` (Master Instruksi Multi-Step Pendaftaran Pelamar) |
| Judul | Kesiapan & Perbaikan Terarah Backend Wizard Pendaftaran Pelamar |
| Slice canonical existing | `S-D2` — Rekrutmen & Seleksi (pelengkap terarah tanpa mengubah `BE-HRD-054` / `BE-HRD-055` menjadi task selesai paralel) |
| Trace keputusan | `REG-WIZ-01` s.d. `REG-WIZ-10`; rujukan existing `BE-HRD-054`, `BE-HRD-055`, `MSL-15` tahap 2 |
| Baseline HEAD | `6945b70d0f5ff35520f1789b7d5e7acbd3ecae48` (branch `MHamzah`, perubahan lokal belum di-commit) |
| Mode & target tulis | `BACKEND` — `Areas/Corporate/HumanResource/RecruitmentManagement/{DTOs,Services}` dan laporan ini |
| Entity / Migration baru | Tidak ada (`NO MIGRATION`, tanpa DDL/DML SQL langsung) |
| Tanggal | 25 November 2026 |
| Status | ✅ **IMPLEMENTED (Gap Terkait Wizard)** — kompilasi, QBE Strict, dan 6/6 uji API terisolasi `PASS`; eksekusi `POST /apply` pada lowongan `Published` nyata di DB dev terblokir data (`0` lowongan `Published`), dan kebijakan multi-STR di luar urutan terbaru tercatat `PENDING_OWNER_DECISION` |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `RecruitmentManagement` |
| Tabel & entitas existing | `HrdJobVacancy`, `HrdCandidate`, `HrdCandidateApplication`, `HrdCandidateDocument`, `MstEmployee`, `NumberSeries` |
| Pembebasan controller legacy | `JobVacancyController`, `CandidateController`, `CandidateScreeningController` tetap mempertahankan pembungkus `ApiResponse<T>` dan otorisasi `AccessPermission` existing |
| Aturan QBE terkait | `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001`, generator nomor canonical (`NumberSeriesAllocator`) |

---

## 1. Tujuan & Masalah yang Diperbaiki

Untuk mendukung wizard pendaftaran pelamar 5 langkah (`Step 1` Lowongan s.d. `Step 5` Review & Kirim Lamaran) secara aman tanpa menambah endpoint, tabel, ataupun enum status baru, audit kode baseline pada `RecruitmentManagement` menemukan lima celah nyata yang harus ditutup agar kompatibel mundur dan aman dari kondisi balapan (*race condition*) serta penyalahgunaan parameter berkas:

1. **Ringkasan lowongan kurang informatif (`Step 1`)**: `GET /vacancies` dan `GET /vacancies/{id}` belum mengembalikan nama jabatan, nama unit organisasi, nama lokasi RS, dan deskripsi persyaratan pada list item, sehingga layar hanya menerima GUID relasi.
2. **Penomoran kandidat & lamaran memakai `CountAsync() + 1`**: pada request paralel, dua pendaftaran serentak berisiko menghasilkan kode kandidat/lamaran kembar atau bentrok indeks unik.
3. **Konflik database menjadi HTTP `500` & validasi biodata tidak merata**: pelanggaran keunikan NIK / pasangan kandidat–lowongan / konkurensi `RowVersion` belum tertangkap rapi sebagai `409 Conflict`, serta `BirthDate` masa depan atau kolom wajib berkas kosong belum dicek di `CandidateService`.
4. **Keamanan & idempotensi dokumen (`Step 4`)**: `UploadDocumentAsync` belum memverifikasi kepemilikan lampiran fisik melalui `HrAttachmentStorage.Open(..., "recruitment-candidate-document")` dan `OwnerUserId`, tidak membaca ukuran berkas nyata di penyimpanan, membiarkan `ValidFrom > ValidUntil`, serta mengizinkan metadata duplikat saat petugas menekan tombol coba lagi (*retry*) memakai `key` lampiran yang sama.
5. **Ketidakpastian urutan dokumen STR pada seleksi (`EvaluateScreeningAsync`)**: kueri tidak memasang pengurutan deterministik sebelum memilih STR kandidat.

**Skenario nyata rumah sakit**: Saat dua staf rekrutmen membuka wizard secara bersamaan di RS MMC untuk mendaftarkan pelamar IGD baru atau mantan pegawai (*rehire*), sistem wajib menerbitkan dua nomor urut `CAN-yyyyMMdd-NNNN` berbeda lewat `NumberSeriesAllocator`, menolak registrasi NIK kembar dengan pesan `409 Conflict` yang jelas, serta memastikan petugas lain tidak dapat mencantumkan `filePath` palsu atau kunci berkas milik unggahan yang tidak sah.

---

## 2. Alur Proses Bisnis Backend

1. **Step 1 — Baca Lowongan Aktif**: Client memanggil `GET /vacancies?status=Published` dan `GET /vacancies/{id}`. `JobVacancyService` menyertakan relasi `Requisition -> Position / OrganizationUnit / HospitalSite` dan mengisi properti nullable `PositionName`, `OrganizationUnitName`, `HospitalSiteName`, serta `Description` tanpa mengubah field lama.
2. **Step 2 — Pemeriksaan NIK & Rehire**: Client memanggil `GET /candidates?searchNik=<16 digit>` untuk memeriksa apakah NIK sudah terdaftar, lalu bila pengguna mengonfirmasi pelamar existing memanggil `GET /candidates/{id}` (yang sekaligus mengembalikan `IsRehire` dan `PreviousEmployeeNumber` dari `MstEmployee` bila relevan).
3. **Step 3 — Checkpoint Persisten (`POST /candidates` atau `PUT /candidates/{id}`)**:
   - `GetUserId` membaca klaim `HrActorClaims.GetUserId(user)` dengan fallback `"sub"`, menolak `401` bila kosong.
   - `ValidateProfileFields` memeriksa isian wajib yang di-trim, panjang `Gender <= 10`, dan `BirthDate` pada rentang `1900-01-01` s.d. tanggal hari ini WIB (`Asia/Jakarta`).
   - `RegisterAsync` mencocokkan NIK di `MstEmployee` hanya sebagai indikasi `IsRehire` (tanpa auto-create akun/pegawai), mengalokasikan kode `CAN-yyyyMMdd-NNNN` memakai `NumberSeriesAllocator` (`HRD_CANDIDATE_CODE`, reset harian WIB), dan menyimpan kandidat berstatus `Registered`.
   - `UpdateAsync` mengecek `RowVersion` ekspektasi; bentrok indeks unik maupun `DbUpdateConcurrencyException` dipetakan aman ke `409 Conflict`.
4. **Step 4 — Unggah Berkas & Metadata Dokumen (`IsVerified=false`)**:
   - Berkas fisik dikirim ke `POST /candidates/documents/attachments` (`HrAttachmentStorage` kategori `recruitment-candidate-document`).
   - Metadata dikirim ke `POST /candidates/{id}/documents` dengan `ApplicationId = null`.
   - `UploadDocumentAsync` memvalidasi `Enum.IsDefined(DocumentType)`, `DocumentTitle`, `ValidFrom <= ValidUntil`, memeriksa `key` lewat `_attachmentStorage.Open(cleanPath, "recruitment-candidate-document")` termasuk mencocokkan `attachment.OwnerUserId == userId` (`403` bila bukan pengunggah), memvalidasi ukuran fisik (`<= 10 MB`), dan mengembalikan baris eksisting secara **idempoten** (`200 OK`) jika `(candidateId, filePath)` sudah tercatat dan tidak dihapus.
5. **Step 5 — Konfirmasi & Pengajuan Lamaran (`POST /candidates/{id}/apply`)**:
   - Jalur tanpa lowongan tidak memanggil `/apply` sama sekali.
   - Jalur melamar memanggil `POST /candidates/{id}/apply`: memverifikasi lowongan berstatus `Published` dan belum melewati `ClosingDate`, menolak duplikasi pasangan kandidat–lowongan (`409 Conflict`), mengalokasikan nomor `APP-yyyyMMdd-NNNN` lewat `NumberSeriesAllocator` (`HRD_CANDIDATE_APPLICATION`), dan mengubah kandidat `Registered` menjadi `Active`.

---

## 3. Perubahan Kode & Audit Dampak

### 3.1 Berkas Backend yang Diubah (`NewQuilvianSystemBackend`)

| Berkas | Ringkasan Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/RecruitmentManagement/DTOs/JobVacancyDtos.cs` | Menambahkan properti opsional (nullable) `PositionName`, `OrganizationUnitName`, `HospitalSiteName` ke `JobVacancyResponse` & `JobVacancyListItemResponse`, serta `Description` ke `JobVacancyListItemResponse` (backward-compatible). |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/JobVacancyService.cs` | Menambahkan `.Include(v => v.Requisition).ThenInclude(...)` untuk `Position`, `OrganizationUnit`, `HospitalSite` pada `GetPagedAsync` & `GetByIdAsync` dan memetakan nilainya. |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/CandidateService.cs` | Menyuntikkan `NumberSeriesAllocator` & `HrAttachmentStorage`; mengganti `CountAsync() + 1` dengan allocator atomik harian WIB (`HRD_CANDIDATE_CODE` & `HRD_CANDIDATE_APPLICATION`); menambah `ValidateProfileFields`; menangani `DbUpdateException` (`PostgresErrorCodes.UniqueViolation`) & `DbUpdateConcurrencyException` menjadi `409`; memperketat `UploadDocumentAsync` (ownership `OwnerUserId`, validasi `key`, ukuran file fisik `<= 10 MB`, `ValidFrom <= ValidUntil`, dan idempotensi `(candidateId, filePath)`). |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/CandidateScreeningService.cs` | Menambahkan `.OrderByDescending(d => d.CreateDateTime).ThenByDescending(d => d.Id)` saat membaca `HrdCandidateDocuments` pada `EvaluateScreeningAsync` agar pemilihan STR deterministik mengacu unggahan terbaru. |

**Berkas yang direuse tanpa perubahan**: `CandidateController.cs`, `JobVacancyController.cs`, `CandidateDtos.cs`, seluruh model entitas/EF Core, dan `HrAttachmentStorage.cs`.

---

## 4. Dokumentasi Endpoint (Bergaya Swagger)

### `[Tags("Corporate / Human Resource / Recruitment Management / Job Vacancy")]`
Base URL: `api/v1/corporate/human-resource/recruitment/vacancies`

| Method | Path | Deskripsi | Auth (`AccessPermission`) | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar lowongan kerja terpaginasi (+ `positionName`, `organizationUnitName`, `hospitalSiteName`, `description`) | `JobVacancy : Read` | Query `status=Published`, `search`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<JobVacancyListItemResponse>>` (`200`) |
| `GET` | `/{id}` | Detail lowongan beserta relasi requisition | `JobVacancy : Read` | Route `{id:guid}` | `ApiResponse<JobVacancyResponse>` (`200`, `404`) |

### `[Tags("Corporate / Human Resource / Recruitment Management / Candidate")]`
Base URL: `api/v1/corporate/human-resource/recruitment/candidates`

| Method | Path | Deskripsi | Auth (`AccessPermission`) | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kandidat terpaginasi (`searchNik` untuk exact match di UI) | `Candidate : Read` | Query `searchNik`, `search`, `status`, `isRehire`, paging | `ApiResponse<PagedResult<CandidateListItemResponse>>` (`200`) |
| `GET` | `/{id}` | Detail profil, `rowVersion`, daftar lamaran, dokumen (urut terbaru), & `previousEmployeeNumber` | `Candidate : Read` | Route `{id:guid}` | `ApiResponse<CandidateResponse>` (`200`, `404`) |
| `POST` | `/` | Registrasi profil kandidat baru (`Status=Registered`, cek Rehire `MstEmployee`, kode `CAN-yyyyMMdd-NNNN`) | `Candidate : Create` | `RegisterCandidateRequest` | `ApiResponse<CandidateResponse>` (`200`, `400`, `401`, `409`) |
| `PUT` | `/{id}` | Perbarui kontak/biodata dengan pemeriksaan `rowVersion` | `Candidate : Update` | `UpdateCandidateRequest` (+ `rowVersion`) | `ApiResponse<CandidateResponse>` (`200`, `400`, `404`, `409`) |
| `POST` | `/documents/attachments` | Unggah berkas fisik multipart (`file`, maks 10 MB) ke penyimpanan terlindung | `Candidate : UploadDocument` | `multipart/form-data` (`file`) | `ApiResponse<HrStoredAttachment>` (`201`, `400`, `403`) |
| `POST` | `/{id}/documents` | Simpan metadata dokumen ke kandidat (`applicationId: null`, `isVerified: false`), idempoten per `(candidateId, filePath)` | `Candidate : UploadDocument` | `UploadCandidateDocumentRequest` | `ApiResponse<CandidateDocumentResponse>` (`200`, `400`, `403`, `404`) |
| `GET` | `/documents/attachments/download` | Unduh lampiran terotorisasi | `Candidate : Read` + cek kepemilikan storage | Query `key` | `FileStreamResult` (`200`, `403`, `404`) |
| `POST` | `/{id}/apply` | Ajukan lamaran ke lowongan `Published` (kode `APP-yyyyMMdd-NNNN`) | `Candidate : Update` | `ApplyVacancyRequest` (`vacancyId`, `notes?`) | `ApiResponse<CandidateApplicationResponse>` (`200`, `400`, `404`, `409`, `422`) |

---

## 5. Hasil Verifikasi Backend

| Pemeriksaan / Skenario | Hasil | Status | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` pada 4 berkas `.cs` | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0` | `PASS` | Dijalankan ulang pasca-edit akhir (`Final result: PASS`) |
| `dotnet build .\QuilvianSystemBackend.csproj --no-restore -t:Compile /nr:false -v minimal` | `Build succeeded. 0 Error(s)`, `6 Warning(s)` (baseline paket `SixLabors.ImageSharp` 3.1.11) | `PASS` | Dijalankan ulang pasca-edit akhir (`00:00:02.68`) |
| `dotnet build -o <artifact-scratch>/be-run` | `0 Error(s)` dalam `7.20s` untuk daemon terisolasi port `7194` | `PASS` | Dipakai melayani seluruh pengujian API & Chromium UAT |
| `wiz-1125-be-api-test.mjs` — `BE-API-01` (validasi NIK & `BirthDate` masa depan) | `badNik=400`, `futureBirth=400` | `PASS` | `be-api-results.json` (`https://localhost:7194`) |
| `wiz-1125-be-api-test.mjs` — `BE-API-02` (`UAT-09` balapan NIK paralel `POST /candidates`) | Request paralel menghasilkan `1x 200` + `1x 409`, `0` HTTP 500, kode `CAN-20261125-0001` | `PASS` | `be-api-results.json` |
| `wiz-1125-be-api-test.mjs` — `BE-API-03` (`UAT-19` deteksi `MstEmployee` Rehire) | NIK `999911••••••0006` terdeteksi `isRehire=true`, `prevEmp=RSMMC-20260105006` (`CAN-20261125-0003`) tanpa mutasi master | `PASS` | `be-api-results.json` |
| `wiz-1125-be-api-test.mjs` — `BE-API-04` (`UAT-20` konkurensi `RowVersion`) | Update v1 `200`, update dengan `rowVersion` lama `409` | `PASS` | `be-api-results.json` |
| `wiz-1125-be-api-test.mjs` — `BE-API-05` (`UAT-13`, `UAT-15` keamanan & idempotensi berkas) | Path palsu `400`, tanggal terbalik `400`, upload nyata `201` + retry `POST /documents` key sama tetap `1` baris (`isVerified=false`) | `PASS` | `be-api-results.json` |
| `wiz-1125-be-api-test.mjs` — `BE-API-06` (`UAT-10`, `UAT-15` RBAC API) | Asesor `403/403/403` (List/Detail/Download); Recruiter `200` List, `403/403/403` Create/Doc/Attach | `PASS` | `be-api-results.json` |

---

## 6. Blocker, Kebijakan Terbuka, & Catatan Penutup

- **Blocker data dev DB (`QuilvianNewDevHamzah`)**: Database dev memiliki **0 Headcount Request, 0 Job Requisition, 0 Job Vacancy**. Tanpa otorisasi pembuatan master rekrutmen, pengujian integrasi hidup `POST /candidates/{id}/apply` dan balapan `UAT-09` pada `/apply` terhadap baris `HrdJobVacancy` nyata berstatus `BLOCKED` pada layer DB dev (mekanisme backend `ApplyVacancyAsync`, `NumberSeriesAllocator`, dan `409`/`422` diverifikasi lewat source & kompilasi).
- **Kebijakan bisnis belum terkunci (`PENDING_OWNER_DECISION`)**: `CandidateScreeningService.EvaluateScreeningAsync` kini deterministik memilih STR unggahan terbaru (`CreateDateTime DESC, Id DESC`). Apabila kebijakan rumah sakit menghendaki prioritas STR berdasarkan kriteria lain (misalnya STR yang masa berlakunya paling panjang meski ada STR lain yang lebih baru atau STR terverifikasi lebih dulu), perlu keputusan pemilik proses HRD sebelum logika prioritas tambahan diterapkan.
- **AUTOMATED TEST**: Backend repository tetap tidak memelihara proyek `.Tests.csproj` sesuai `TEST_POLICY.md`; pengujian otomatis API dilakukan melalui harness eksternal `test-with-agy/scripts/wiz-1125-be-api-test.mjs` terhadap proses build terbaru di port `7194`.
- **Memory sync**: `MEMORY_PENDING_SYNC` — berkas `docs/module-blueprints/human-resource/memory.md` belum tersedia dan tidak ada penulisan ke GitHub.
