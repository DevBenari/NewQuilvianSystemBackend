# Laporan Task `RJ-REG-REV-BE-007` — Penyimpanan General Consent RJ per kunjungan

| Field | Isi |
| --- | --- |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian 9.1 |
| Requirement | `RJ-REG-REV-015` |
| Keputusan | `RJ-REG-DEC-012` (amendemen 23 Okt 2026), wewenang `RJ-REG-DEC-001` |
| Tanggal | 23 Oktober 2026 |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Status | ✅ `TESTED` / `UAT_PENDING` |

## 1. Tujuan

Sebelum task ini, General Consent di Pendaftaran Rawat Jalan hanya ada di layar. Isian dan tanda tangan hilang begitu pendaftaran selesai. Rawat Inap sudah menyimpan persetujuannya per episode (`BE-RWI-205`), sedangkan Rawat Jalan belum punya tempat penyimpanan.

Task ini menambah satu tabel dan dua endpoint. Layar pendaftaran memanggil endpoint simpan sesudah kunjungan terbentuk, baik untuk jalur poliklinik maupun rujukan Laboratorium.

Contoh: Ny. Rina mendaftarkan anaknya sebagai pasien baru. Ia menandatangani 12 butir sebagai Orang Tua. Sesudah kunjungan `ENC-RSMMC-00372` terbentuk, layar mengirim isian ke `PUT …/00372/general-consent`. Backend menyimpan nama, NIK, hubungan, tanda tangan, waktu server, dan petugas.

## 2. Backend Governance Preflight

| Field | Isi |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (registry: `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` untuk enum, entity, configuration, DTO, service, dan migration. `TOUCHED LEGACY` untuk `PatientEncounterController`, `ApplicationDbContext`, dan `Program.cs` |
| QBE yang berlaku | `QBE-ENT-001` (`IdentityModel`), `QBE-NAM-002` (prefix `Reg`), `QBE-CFG-001`, `QBE-MOD-001`, `QBE-SVC-001` (context hanya di service), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-TXN-001` |
| Jenis capability | Transaksi, arketipe sub-proses ter-scope induk (dokumen tunggal per kunjungan, `transaction-endpoint-standard` §2.3) |
| Entity | `RegPatientGeneralConsent` → tabel `public."RegPatientGeneralConsent"`, DbSet `RegPatientGeneralConsents` |
| Migration | `20261023035602_AddRegPatientGeneralConsent`, diterapkan **hanya** ke `QuilvianNewDevSukma` atas izin user di sesi ini |

## 3. Endpoint

```http
GET /api/v1/health-services/registration-management/patient-encounters/{encounterId}/general-consent
PUT /api/v1/health-services/registration-management/patient-encounters/{encounterId}/general-consent
```

| Endpoint | Izin | `[AccessAction]` |
| --- | --- | --- |
| `GET` | `PatientEncounter : Read` | `Read` — "Read Patient Encounter" |
| `PUT` | `PatientEncounter : Create` | `Create` — "Save Outpatient General Consent" |

Kedua action memakai nama action yang sudah ada (`Read`, `Create`). Petugas pendaftaran yang sudah boleh membuat kunjungan langsung boleh menyimpan consent. Tidak ada centang baru di Akses Role.

Contoh request `PUT`:

```json
{
  "signerType": "Parent",
  "signerName": "Rina Lestari",
  "signerRelationshipText": "Ibu Kandung",
  "signerIdentityNumber": "3171000000000001",
  "signerPhoneNumber": "081234567890",
  "signerAddress": "Jl. Melati No. 1",
  "agreedClauses": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
  "signatureImageBase64": "data:image/png;base64,iVBORw0KGgo..."
}
```

Respons `200` (`data`): `consentId`, `encounterId`, `encounterNumber`, `patientId`, `signerType`, `signerTypeName`, `signerName`, `signerRelationshipText`, `signerIdentityNumber`, `signerPhoneNumber`, `signerAddress`, `agreedClauses`, `signatureImageBase64`, `signedAt` (UTC, server), `officerName`.

| Status | Kapan |
| --- | --- |
| `200` | Tersimpan, atau kunjungan sudah punya consent (consent lama dikembalikan, pesan "…isian tidak diubah") |
| `400` | Bukan kunjungan Rawat Jalan; kunjungan batal; `signerType` bukan `Self`/`Parent`/`Guardian`/`Spouse`/`Family`; nama < 3 atau > 200 karakter; klausul bukan tepat 1–12; tanda tangan kosong atau bukan PNG; No. HP bukan angka atau > 13 digit |
| `404` | Kunjungan tidak ada; `GET` sebelum consent tersimpan ("General Consent kunjungan ini belum tersimpan.") |
| `401` / `403` | Belum login / tanpa izin |

Aturan yang ditegakkan server:

- Waktu tanda tangan memakai waktu server. Petugas diambil dari pengguna yang login. Klien tidak mengirim keduanya.
- Satu kunjungan paling banyak satu consent yang hidup. Unique index terfilter `PatientEncounterId` (`IsDelete = false`) menjadi lapis kedua. Bila dua penyimpanan tiba bersamaan, yang kalah menerima consent pemenang.
- Consent yang sudah tersimpan tidak pernah ditimpa. Tombol *Coba lagi* di layar Selesai aman diulang.
- Log aplikasi `PatientEncounter.SaveGeneralConsent` hanya memuat `EncounterId`, `ConsentId`, dan `SignerType`, tanpa nama, NIK, maupun citra tanda tangan.

## 4. Berkas

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Enums/GeneralConsentSignerType.cs` | Baru. `Self=1`, `Parent=2`, `Guardian=3`, `Spouse=4`, `Family=5`, label sama dengan Rawat Inap |
| `Areas/HealthServices/RegistrationManagement/Models/RegPatientGeneralConsent.cs` | Baru. Entity `IdentityModel` |
| `Repositories/Configurations/HealthServices/RegPatientGeneralConsentConfiguration.cs` | Baru. Panjang kolom, `integer[]`, `text`, unique index terfilter, FK `Restrict` ke `RegPatientEncounter` dan `MstPatient` |
| `Areas/HealthServices/RegistrationManagement/DTOS/OutpatientGeneralConsentDtos.cs` | Baru. `OutpatientGeneralConsentSaveRequest`, `OutpatientGeneralConsentResponse` |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientGeneralConsentService.cs` | Baru. `GetAsync`, `SaveAsync`, validasi, penanganan unique violation |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Action `GetGeneralConsent` dan `SaveGeneralConsent`. Service diambil per request seperti `ReferralDocumentStorage`, sehingga konstruktor dan harness lama tidak berubah |
| `Repositories/ApplicationDbContext.cs` | DbSet `RegPatientGeneralConsents` |
| `Program.cs` | `AddScoped<OutpatientGeneralConsentService>()` |
| `Migrations/20261023035602_AddRegPatientGeneralConsent.cs` + `.Designer.cs` | Baru. Satu tabel, dua index, dua FK. `Down` = drop tabel baru saja |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Blok entity baru. EF juga menyusun ulang urutan satu index dan dua FK `PhmPrescriptionStockReservation` tanpa perubahan isi; migration `Up` tidak menyentuh tabel lain |

### Delta terhadap kartu task

| Hal | Kartu task | Implementasi | Alasan |
| --- | --- | --- | --- |
| Verb simpan | `POST …/general-consent` | `PUT …/general-consent` | `transaction-endpoint-standard` §2.3: dokumen tunggal milik induk memakai `GET`/`PUT` pada path tetap. Penyimpanan ulang tidak membuat dokumen kedua |
| Isian tidak sah pada kunjungan yang sudah ber-consent | — | `200` dengan consent lama | Consent yang ada dikembalikan lebih dulu; isian baru diabaikan dan tidak pernah menimpa |

## 5. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release -p:UseSharedCompilation=false -o <scratchpad>` | `0 Error`, 10 warning lama (tidak ada di berkas task ini) — `PASS` |
| `dotnet ef migrations has-pending-model-changes --configuration Release` | "No changes have been made to the model since the last migration." — `PASS` |
| `dotnet ef database update 20261023035602_AddRegPatientGeneralConsent` ke `QuilvianNewDevSukma` | "Applying migration … Done." Sebelumnya hanya migration ini yang `Pending` — `PASS` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (8 berkas) | `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `Final result: PASS` — `PASS` |
| `dotnet test` | `NOT RUN` — tidak ada project test (pola Bank Darah) |

Runtime HTTP terhadap aplikasi build scratchpad di `https://localhost:7185`, DB `QuilvianNewDevSukma`, akun SuperAdmin, data samaran `UJI GC …`:

| ID | Skenario | Hasil |
| --- | --- | --- |
| R0 | `GET` sebelum ada consent → `404` "…belum tersimpan." | `PASS` |
| R1 | `PUT` sah (`ENC-RSMMC-00372`) → `200`; `signerTypeName` "Orang Tua", klausul 1–12, `signedAt` server, `officerName` "SuperAdmin" | `PASS` |
| R2 | `GET` → isian, NIK, dan tanda tangan sama persis | `PASS` |
| R3 | `PUT` ulang dengan nama dan jenis lain → `200`, `consentId` sama, nama lama tetap, pesan "…isian tidak diubah" | `PASS` |
| R4a / R4b | Klausul 11 butir / berisi 13 → `400` | `PASS` |
| R5 | Nama 2 karakter → `400` | `PASS` |
| R6a / R6b | `signerType` "Boss" / "1" → `400` "Jenis penanda tangan tidak dikenal." | `PASS` |
| R7a / R7b / R7c | Tanda tangan kosong / PNG palsu / JPEG → `400` | `PASS` |
| R8a / R8b | No. HP berhuruf / 14 digit → `400` | `PASS` |
| R4z | Isian tidak sah tidak menyimpan apa pun (`GET` tetap `404`) | `PASS` |
| R9 | Kunjungan acak → `PUT` dan `GET` `404` "Kunjungan tidak ditemukan." | `PASS` |
| R10 | Kunjungan IGD → `400` | `PASS` |
| R11 | Kunjungan batal → `400` | `PASS` |
| R12 | Dua `PUT` paralel (barrier) pada `ENC-RSMMC-00373` → keduanya `200`, satu `consentId`, satu baris di DB | `PASS` |
| R13 | Tanpa login → `401` | `PASS` |
| R14 | DB: tepat satu baris per kunjungan uji; IGD dan kunjungan batal tanpa baris | `PASS` |
| Log | 5 entri `SaveGeneralConsent` di log aplikasi; tidak memuat nama, NIK, maupun data tanda tangan | `PASS` |

Total 24/24 `PASS`. Percobaan R12 pertama tidak sah karena satu login paralel gagal `500` (lihat risiko), lalu diulang dengan login lebih dulu dan barrier.

Keadaan DB sesudah run: 3 baris `RegPatientGeneralConsent` bernama `UJI GC …` pada `ENC-RSMMC-00371`, `00372`, dan `00373` (kunjungan uji lama di DB personal). Baris ini dibiarkan; hapus lewat SQL bila tidak diperlukan.

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Isian sah → `200`, tersimpan, terbaca ulang | Terpenuhi | R1, R2 |
| 2. Klausul, nama, jenis, tanda tangan, No. HP tidak sah → `400` | Terpenuhi | R4–R8, R4z |
| 3. Kunjungan tidak ada `404`; bukan RJ atau batal `400` | Terpenuhi | R9, R10, R11 |
| 4. Simpan ulang → `200` data lama, tidak ganda, tidak ditimpa | Terpenuhi | R3, R12, R14 |
| 5. `GET` sebelum ada consent → `404` | Terpenuhi | R0 |
| 6. Waktu tanda tangan dan petugas dari server | Terpenuhi | R1; request DTO tidak punya field keduanya |
| 7. Audit tanpa PII | Terpenuhi | Pemeriksaan log |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Uji `403` dengan akun tanpa izin belum dijalankan (akun uji SuperAdmin melewati pemeriksaan; metadata akses sudah dicocokkan huruf demi huruf). Citra tanda tangan disimpan sebagai teks di database, sama dengan Rawat Inap, bukan di penyimpanan berkas privat. Belum ada layar baca/cetak consent RJ di luar pendaftaran |
| `UNRELATED EXISTING ISSUE` | Dua login paralel akun yang sama → satu `500` (`AUTH_LOGIN`: "Gagal update LastLoginAt user … Optimistic concurrency failure"). Di luar scope |
| Migration / database | `20261023035602_AddRegPatientGeneralConsent` diterapkan ke `QuilvianNewDevSukma` saja. DB master/production menunggu pemilik rilis |
| Status Git | Berkas di atas `M`/`??`, belum di-commit |
| Task berikutnya | `RJ-REG-REV-FE-011` |
