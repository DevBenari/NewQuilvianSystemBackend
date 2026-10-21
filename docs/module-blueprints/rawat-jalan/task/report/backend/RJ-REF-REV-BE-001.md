# Laporan Task Backend — `RJ-REF-REV-BE-001`..`003` (Zoom 13 Okt 2026)

Satu laporan untuk tiga task backend karena diverifikasi dalam satu run runtime yang sama.
Definisi task: [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian 8.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Tanggal | 16 Oktober 2026 |
| Branch / baseline | `sukmagp` @ `5a6de202`, working tree belum di-commit |
| Requirement | `RJ-REF-REV-001`..`005` |
| Keputusan | `RJ-REF-DEC-001`..`007` |
| Migration | `20261016043218_AddEncounterReferralSourceType` — diterapkan ke `QuilvianNewDevSukma` (izin pemilik 16 Okt 2026) |
| Status | `BE-001` ✅, `BE-002` ✅, `BE-003` ✅ (`TESTED`, UAT belum) |

## 2. Perubahan

| Task | Berkas | Isi |
| --- | --- | --- |
| `BE-001` | `RegistrationManagement/Enums/ReferralSourceType.cs` (baru) | `External = 1`, `Internal = 2` |
| `BE-001` | `Models/RegEncounterReferral.cs`, `Repositories/Configurations/HealthServices/RegEncounterReferralConfiguration.cs` | `SourceType` (default 1), `SourceDoctorCertificateId` + FK `Restrict` ke `CliDoctorCertificate`, index |
| `BE-001` | `Migrations/20261016043218_AddEncounterReferralSourceType*.cs`, snapshot | Aditif; SQL data menandai lengkap rujukan RJ eksternal lama yang memenuhi aturan baru (20 baris di DB uji). `Down()` menghapus kolom, tidak mengembalikan flag |
| `BE-001` | `Services/EncounterReferralService.cs` | Validasi sumber; rujukan internal (`PM-18..22`) dengan kunci `FOR UPDATE` dalam transaksi create; RJ eksternal: fasilitas wajib (`Pm03External`), nomor/diagnosa/alasan opsional; IGD tetap; `ComputeMissingFields` per sumber; `GetInternalLetterOptionsAsync`; `EncounterReferralConflictException` |
| `BE-001` | `Services/ReferralDocumentStorageService.cs` | Unggah ke rujukan internal ditolak `PM-22` |
| `BE-001` | `DTOS/EncounterReferralDtos.cs` | `sourceType`, `sourceDoctorCertificateId`, `sourceLetter`, `InternalReferralLetterOptionResponse` |
| `BE-001` | `Controllers/PatientEncounterController.cs` | `GET /patient-encounters/internal-referral-letters?patientId=&clinicId=` (`PatientEncounter : Read`); `catch EncounterReferralConflictException` → rollback + `409` |
| `BE-002` | `ClinicalManagement/Services/DoctorCertificateService.cs`, `Controllers/DoctorCertificateController.cs` | `IssueInternalReferralAsync` (DPJP, kunjungan RJ poli sudah ditangani, kunci kunjungan, anti-ganda per tujuan, nomor `SRJ`); validasi tujuan poli aktif ≠ poli kunjungan, diagnosa/alasan wajib; identitas/SIP/tanggal dari server; kelas perawatan `null`; ubah/batal surat terpakai `409`; `referral-targets` tipe rujukan hanya poliklinik; `UpdateAsync` menerima `ClaimsPrincipal` |
| `BE-003` | `Services/OutpatientEncounterDetailService.cs`, `Controllers/OutpatientEncounterController.cs`, `DTOS/OutpatientEncounterDetailDtos.cs`, `Services/OutpatientEncounterListService.cs` | Detail + `referral`, `referralLetters`; `GET /outpatient-encounters/{id}/referral-letters/{certificateId}` bercakupan, tertaut, hanya Terbit, `no-store`, audit |

## 3. Kontrak baru/berubah (aditif)

- `POST /patient-encounters/admin` — `referral.sourceType` (1/2, kosong = 1), `referral.sourceDoctorCertificateId`.
  Rujukan RJ eksternal tidak lagi mewajibkan `referralNumber`, `diagnosisId`, `referralReason`.
- `GET /patient-encounters/internal-referral-letters?patientId={guid}&clinicId={guid?}` → `InternalReferralLetterOptionResponse[]`.
- `GET /patient-encounters/{id}/referral` — tambahan `sourceType`, `sourceTypeName`, `sourceLetter`; `missingFields` dapat berisi `sourceLetter`.
- `GET /outpatient-encounters/{id}` — tambahan `referral`, `referralLetters[]`.
- `GET /outpatient-encounters/{id}/referral-letters/{certificateId}` → `DoctorCertificateResponse` (`OutpatientEncounter : Read`).
- `GET /doctor-certificates/referral-targets?certificateType=inpatientReferral` → hanya `kind = clinic`.
- Kode baru: `RJ-VAL-PM-18`..`22`; `RJ-VAL-PM-03` pesan eksternal.

## 4. Validasi

| Langkah | Hasil |
| --- | --- |
| `dotnet build -c Release` | `0 Error` |
| `dotnet ef migrations has-pending-model-changes` | Tidak ada perubahan tertunda |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `PASS` — 16 berkas, 0 violation |
| `dotnet ef database update` → `QuilvianNewDevSukma` | Migration diterapkan; sesudahnya 34/34 rujukan RJ `IsComplete = true`, `SourceType = 1` |
| `dotnet test` | `NOT RUN` — tidak ada project test (pola Bank Darah) |
| Runtime HTTP (backend uji `https://localhost:7185`) | **26/26 PASS** |

## 5. Runtime HTTP

Data samaran: pasien `REGREV UJI B04ADA` (A) dan `REGREV UJI B55E44` (B); sumber Poli Umum, tujuan Poli
Penyakit Dalam. Dokter `dr. UJI-RJDP Dokter` diaktifkan sementara dan email-nya diarahkan ke akun admin uji
supaya admin terbaca sebagai DPJP (SQL, dikembalikan di akhir run). Status kunjungan sumber dipaksa
`InConsultation` lalu `Completed` lewat SQL untuk mensimulasikan konsultasi.

| ID | Skenario | Hasil |
| --- | --- | --- |
| R0/S1 | Login, dokter uji aktif sementara, kunjungan sumber dibuat | PASS |
| R1 | Tujuan Surat Rujukan hanya poliklinik | PASS |
| R2 | Diagnosa kosong `400` | PASS |
| R3 | Tujuan = poli kunjungan `400` | PASS |
| R4 | Terbit: `SRJ-000004`, nama pasien dari master (klien kirim nama palsu), kelas `null`, tanggal hari ini | PASS |
| R5 | Retry ke tujuan sama `409`, tetap 1 surat | PASS |
| R6 | Surat kedua (Poli Jantung) terbit lalu dibatalkan | PASS |
| R7/R8 | Opsi surat internal hanya untuk pasien pemilik, tanpa surat batal | PASS |
| R9 | Surat pasien lain `PM-18` | PASS |
| R10 | Poli beda dari tujuan `PM-20` | PASS |
| R11 | Kunjungan internal: `SourceType=2`, surat tertaut, lengkap, nomor = `SRJ-…`, fasilitas `null` | PASS |
| R12 | Pakai ulang surat `PM-19`, tetap 1 kunjungan | PASS |
| R13 | Surat terpakai hilang dari opsi | PASS |
| R14 | Ubah/batal surat terpakai `409` | PASS |
| R15/R16 | Detail lanjutan & sumber memuat rujukan dan surat; surat batal `canPrint=false` | PASS |
| R17 | Cetak ulang surat resmi `200` (+ `Cache-Control: no-store`) | PASS |
| R18 | Surat batal `400` | PASS |
| R19/R24 | IDOR: surat tidak tertaut, acak, pasien lain `404` | PASS |
| R20/R21 | Unggah/koreksi rujukan internal `PM-22` | PASS |
| R22 | Eksternal tanpa fasilitas `PM-03` (pesan eksternal) | PASS |
| R23 | Eksternal tanpa nomor & tanpa unggahan tersimpan lengkap, `referralStatus = Complete` | PASS |

Keadaan DB sesudah run: semua kunjungan uji dibatalkan (`aktif tersisa = 0`), dokter uji dikembalikan ke nonaktif
dengan email semula. Surat `SRJ` uji tetap tersimpan sebagai histori (kunjungan sumbernya dibatalkan).

## 6. Tidak terbukti / risiko

- Uji `403` (akun tanpa izin, aktor bukan DPJP) hanya lewat kode — akun uji SuperAdmin.
- Cabang IGD (`EncounterType = Emergency`) dibuktikan lewat kode; tidak diuji runtime.
- Endpoint lama `GET /doctor-certificates*` belum punya otorisasi objek; tidak diubah dalam task ini.
- Fasilitas di luar master: `BLOCKED / OWNER_DECISION_REQUIRED` (bertentangan dengan `DEC-FRJ-001`).
