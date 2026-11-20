# Laporan Perubahan Backend — `BE-KSK-PO-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-009` |
| Judul | Daftar, ringkasan, detail, dan berkas untuk petugas |
| Slice | `EPIC KSK-PO-04` — layar petugas (`FR-KSK-PO-030`, `032`), gelombang teknis 5, rilis `MVP-3` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-009` |
| Trace | `KSK-DEC-050`; `KSK-PO-AS-002`; `KSK-DSN-PO-017`; `KSK-VAL-PO-068`, `069` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | `BE-KSK-PO-005` ✅ |
| Klasifikasi | `MEDIUM` — endpoint baca petugas dengan Resource izin baru dan pembukaan berkas privat; tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Controllers,DTOS,Services}`, `Program.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a86001e2` (`sukmagp`) |
| Tanggal | 2026-11-20 |
| Status | ✅ Selesai (2026-11-20) — semua kriteria terbukti di runtime. Aksi `Update` dan `Cancel` muncul di Akses Role bersama endpoint `BE-KSK-PO-010` dan terbukti di sana (A1). Sebelumnya 🟡 |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` (controller, service, DTO) dan `TOUCHED LEGACY` (`ReferralDocumentStorageService`: method `internal` baru `OpenStoredFile`, method lama tidak berubah) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), `QBE-API-001`/`QBE-DTO-001` (route `api/v1/...`, `ApiResponse<T>`, `PagedResult<T>`), `QBE-PERM-001` (`[AccessAction]` + `[AccessPermission("OnlineRegistration", "Read")]` pada setiap action, `ControllerName` sama persis) |
| Arketipe endpoint | Transaksi ber-lifecycle, permukaan baca: `GET /`, `GET /summary`, `GET /{id}`, sub-resource berkas. Sengaja tanpa `GET /options`, `PATCH /{id}/status`, dan `DELETE /{id}` (standar endpoint transaksi); aksi ubah/batal menyusul di `BE-KSK-PO-010` |
| Hardcode role | Tidak ada. Hak akses lewat Akses Role |

## 1. Masalah yang diperbaiki

Pendaftaran online sudah tersimpan, tetapi petugas admisi belum bisa melihatnya. Task ini membuat layar baca untuk petugas: daftar dengan filter tanggal kunjungan, status, dan pencarian; ringkasan jumlah per status; detail beserta riwayat; dan pembukaan berkas surat rujukan yang diunggah pasien (`KSK-DEC-050`).

## 2. Proses bisnis

1. Admin memberi jabatan petugas hak `Online Registration → Read Online Registration` di layar Pengaturan → Manajemen Role → Akses Role.
2. Petugas membuka daftar. Bawaannya tanggal kunjungan hari ini; rentang bisa diubah sampai 62 hari. Contoh: tanggal 21 Nov, status Menunggu Check-in, tampil 6 pendaftaran.
3. Petugas mencari dengan kode pendaftaran (cocok persis, contoh `K7QM-4TRZ` atau `k7qm4trz`), nomor pendaftaran, nama, No. RM, atau NIK (cocok sebagian). Kata kunci kurang dari 3 karakter ditolak "Kata kunci pencarian minimal 3 karakter."
4. Petugas membuka detail: data diri, kontak, penjamin (nomor kartu lama tersamar, kartu baru utuh), rujukan beserta daftar berkas, consent, dan riwayat (contoh "Didaftarkan online — Halaman Online — 20 Nov 09.08").
5. Petugas membuka berkas surat rujukan langsung dari detail.
6. Kode pendaftaran tidak pernah tampil, baik di daftar maupun detail. Petugas tanpa hak mendapat `403`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OutpatientPriorityReservationController.cs` dan `OutpatientPriorityReservationService.cs` (`GetPagedAsync`, `GetSummaryAsync`, `GetDetailAsync`, pengisian nama pelaku), `ReferralDocumentStorageService.OpenAsync` (pembukaan berkas privat), `PatientEncounterController` (respons berkas), `Seeders/AccessMenuSeeder.cs` dan `PermissionRegistryDescriptor` (aksi Akses Role dibaca dari action descriptor), `RoleAccessController` (fixture izin uji), model `RegOnlineRegistration*`, `PagedResult<T>`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Controllers/OnlineRegistrationController.cs` | **Baru.** `[AccessController(... "Online Registration", ControllerName = "OnlineRegistration", SortOrder = 10)]`; `GET`, `GET summary`, `GET {id}`, `GET {id}/referral-documents/{documentId}`, semuanya `Read` |
| `.../Services/OnlineRegistrationService.cs` | **Baru.** `GetPagedAsync`, `GetSummaryAsync`, `GetDetailAsync` (riwayat + nama pelaku + label jadwal), `OpenDocumentAsync`. "Hari ini" dari `TimeProvider` zona Asia/Jakarta |
| `.../DTOS/OnlineRegistrationDtos.cs` | **Baru.** `OnlineRegistrationListQuery`, `OnlineRegistrationListItemResponse`, `OnlineRegistrationDetailResponse`, `OnlineRegistrationNewInsuranceResponse`, `OnlineRegistrationDocumentResponse`, `OnlineRegistrationHistoryResponse`, `OnlineRegistrationSummaryResponse` |
| `.../Services/ReferralDocumentStorageService.cs` | Method `internal` baru `OpenStoredFile(storagePath)` dengan penjaga path yang sama dengan `OpenAsync`. `OpenAsync` tidak diubah |
| `Program.cs` | `AddScoped<OnlineRegistrationService>()` |

**Delta terhadap kontrak (aditif):**

| Kontrak | Source | Alasan |
| --- | --- | --- |
| Item daftar | Ditambah `endTime`, `patientModeName`, `patientType`, `paymentType`, `emailStatusName` | Label dan nilai enum untuk layar tanpa pemetaan ulang di frontend |
| Detail | Ditambah `genderName`, `clinicId`, `doctorScheduleId`, `sessionName`, `isReferral`, `documents[].pageOrder` | `clinicId` dan `doctorScheduleId` dibutuhkan form ubah jadwal (`BE-KSK-PO-010`/`FE-KSK-PO-008`) |
| Ringkasan | Ditambah `visitDateFrom`, `visitDateTo` (rentang yang dipakai) | Sama dengan ringkasan Reservasi Prioritas |
| — | Berkas yang hilang dari penyimpanan → `404`; root privat salah konfigurasi → `503` | Membedakan berkas tidak ada dengan masalah server |
| AC tambahan (1) aksi `Read`, `Update`, `Cancel` | Baru `Read` | Seeder membaca aksi dari endpoint yang ada. `Update` dan `Cancel` muncul otomatis saat endpoint `BE-KSK-PO-010` dibuat |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: grup *Online Registration*, empat endpoint `GET` sesuai `KSK-CONTRACT-v2` |
| Database | Hanya membaca. Tanpa migration. Resource `OnlineRegistration` dan aksi `Read` dibuat seeder saat aplikasi menyala (`SysControllerAccess`, `SysActionAccess`) |
| Keamanan/Auth | Data pribadi lengkap tampil untuk petugas ber-hak. Kode tidak pernah keluar; nomor kartu lama tersamar. Berkas hanya dibuka bila milik pendaftaran yang diminta, dengan `Cache-Control: no-store` |

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Online Registration

Base URL: `api/v1/health-services/registration-management/online-registrations`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `` | Daftar pendaftaran online | `OnlineRegistration : Read` | Query `visitDateFrom`, `visitDateTo`, `status`, `search` (min. 3), `pageNumber`, `pageSize` (bawaan 25, maks. 100) | `ApiResponse<PagedResult<OnlineRegistrationListItemResponse>>` |
| `GET` | `/summary` | Jumlah per status | `OnlineRegistration : Read` | Query `visitDateFrom`, `visitDateTo` | `ApiResponse<OnlineRegistrationSummaryResponse>` |
| `GET` | `/{id}` | Detail beserta riwayat dan daftar berkas | `OnlineRegistration : Read` | — | `ApiResponse<OnlineRegistrationDetailResponse>` |
| `GET` | `/{id}/referral-documents/{documentId}` | Membuka berkas surat rujukan | `OnlineRegistration : Read` | — | Berkas `application/pdf`, `image/jpeg`, atau `image/png` |

Contoh ringkasan `200` (data uji):

```json
{ "visitDateFrom": "2026-11-21", "visitDateTo": "2026-11-21", "awaitingCheckIn": 6, "checkedIn": 0, "registeredElsewhere": 0, "noShow": 0, "cancelled": 0, "total": 6 }
```

Kode status: `200` berhasil; `400` kata kunci kurang dari 3 karakter atau rentang tanggal terbalik/lebih dari 62 hari; `401` tanpa login; `403` tanpa hak `OnlineRegistration : Read`; `404` pendaftaran atau berkas tidak ditemukan; `503` penyimpanan berkas belum siap.

## 5. Verifikasi

Backend dari build saat ini dijalankan di `https://localhost:7185` terhadap `QuilvianNewDevSukma`. Fixture akun dibuat lewat API SuperAdmin, tanpa kueri database: unit `TEST-PO009 Admisi Uji 20261120094804` dengan jabatan `reader` (diberi `OnlineRegistration : Read` lewat `POST role-access/policies`) dan `noaccess` (tanpa hak), masing-masing satu pegawai dengan akun login. Id lookup diambil dari data pegawai yang ada dan dari `GET role-access/resources`. Data pendaftaran: `PO-20261120-0001` s.d. `0006` dari `BE-KSK-PO-005`.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | Build succeeded, 0 error | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 44 berkas, VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | PASS: metadata gap 0, fallback 68 tetap, endpoint telanjang baru 0, identitas wajib 24/24 | `PASS` | Keluaran verifier |
| A1 Akses Role sesudah seeder | Resource "Online Registration" dengan aksi `Read` | `PASS` (sebagian AC) | `po009_test.py` |
| B1–B2 otorisasi | Akun ber-hak `200` (6 pendaftaran uji); akun tanpa hak `403` dan tanpa login `401` untuk daftar, ringkasan, detail | `PASS` | `po009_test.py` |
| C1–C4 isi daftar | Tanpa kode maupun hash; 22 field kontrak ada; pasien lama memakai nama/No. RM/NIK master, pasien baru No. RM kosong dan NIK online; status dan flag aksi benar | `PASS` | `po009_test.py` |
| C5–C12 pencarian | 6/6 kode menemukan tepat satu pendaftaran (hash persis); kode huruf kecil tanpa tanda hubung tetap ketemu; potongan kode tidak cocok; nomor sebagian 6; nama sebagian tanpa beda huruf 5; No. RM dan NIK pasien lama 1; NIK pasien baru 1 | `PASS` | `po009_test.py` |
| C13–C17 validasi dan paginasi | Kata kunci 2 karakter `400` `KSK-VAL-PO-068`; filter status 6/0; rentang terbalik `400` dan 63 hari `400`, 62 hari `200` (`KSK-VAL-PO-069`); halaman 2 ukuran 2 dari 6 (3 halaman) | `PASS` | `po009_test.py` |
| D1–D2 ringkasan | 6 Menunggu Check-in, total 6; rentang terbalik `400` | `PASS` | `po009_test.py` |
| E1–E3 detail | Pendaftaran rujukan: tanggal lahir, jenis kelamin, kartu baru, faskes, 2 berkas, consent; riwayat `SUBMITTED` "Didaftarkan online" oleh "Halaman Online" dengan label jadwal; tanpa kode maupun hash | `PASS` | `po009_test.py` |
| E4–E6 berkas | Isi PDF sama persis byte demi byte, `application/pdf`, `no-store`; dibuka lewat pendaftaran lain `404`; tanpa hak `403` | `PASS` | `po009_test.py` |
| E7–E8 | Detail pasien lama: tanggal lahir dari master, tanpa kartu baru dan rujukan; id tak dikenal `404` | `PASS` | `po009_test.py` |
| Log backend uji | 0 baris `ERR`/exception | `PASS` | `be7185-po009.log` |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

Total runtime 32/32 PASS.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 18: petugas ber-hak membuka daftar, filter tanggal dan status, cari kode/nomor/nama/No. RM/NIK; kode tidak pernah tampil | Terpenuhi | B1, C1, C5–C14 |
| AC 18: petugas tanpa hak `403` | Terpenuhi | B2, E6 |
| Tambahan (1): Resource `OnlineRegistration` dengan aksi `Read`, `Update`, `Cancel` muncul di Akses Role | Terpenuhi | `Read` terbukti di task ini (A1); ketiga aksi terbukti sesudah endpoint `BE-KSK-PO-010` ada ([laporan](BE-KSK-PO-010.md) A1, 2026-11-20) |
| Tambahan (2): kode tidak pernah dikembalikan | Terpenuhi | C1, E3 |
| Tambahan (3): tanpa hak `403` | Terpenuhi | B2, E6 |
| DoD: QBE Strict, verifier PASS | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Yang belum | Tidak ada. Aksi `Update`/`Cancel` terbukti di `BE-KSK-PO-010` A1 |
| Data uji | Fixture `TEST-PO009` (unit, 2 jabatan, 2 pegawai dengan akun, 1 policy) **sengaja dipertahankan** untuk uji hak `Update`/`Cancel` di `BE-KSK-PO-010`; id di `po009_fixture.json` (scratchpad). Bersihkan sesudah `BE-KSK-PO-010`: hapus lunak dengan akhiran `-DEL-<stamp>` pada kode unit dan nonaktifkan akun |
| Risiko tersisa | Layar petugas menampilkan data pribadi lengkap; batasi hak `Read` ke jabatan admisi saja |
| Perubahan sampingan | `NONE` di repository. Skrip uji dan log di scratchpad |
| Status Git | Milik task ini: 3 berkas baru (controller, service, DTO) dan perubahan pada `ReferralDocumentStorageService.cs` serta `Program.cs`; ditambah perubahan task sebelumnya yang belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-KSK-PO-010` (ubah jadwal, ubah kontak, batal). Frontend `FE-KSK-PO-007` (layar petugas) kini memiliki backend baca |
