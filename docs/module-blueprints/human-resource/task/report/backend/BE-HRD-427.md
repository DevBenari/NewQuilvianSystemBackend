# Laporan Perubahan Backend — `BE-HRD-427`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-427` |
| Judul | STR/SIP dan PSV yang benar |
| Slice | `S-FIN-M09` / Data STR/ SIP (alias `FIN-HRD-09-BE-002`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-427` |
| Trace | M09-P0-02; instruksi Menu 09 `FIN-HRD-09-BE-002` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-426` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WfpCredentialLicenseController`, `CredentialVerificationService`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build, QBE, registri lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `CredentialingManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Create sudah `PendingVerification` dan renew mereset PSV, tetapi: (1) `PUT …/credential-licenses/{id}` masih menyalin `IsVerified`/`VerificationStatus` dari body, sehingga pemilik dokumen bisa memverifikasi sendiri lewat edit biasa; (2) `PATCH …/verify` menerima `VerificationStatus` bebas (bisa `IsVerified=false` + status `Verified`); (3) PSV menerima rujukan apa pun, dapat dijalankan pada lisensi yang dicabut/kedaluwarsa, dan menimpa jejak audit sebelumnya; (4) pendaftaran SIP tanpa nama faskes diisi otomatis "Rumah Sakit Quilvian Utama", sehingga SIP faskes lain tampak berlaku di RS ini. Laporan agent: [FIN-HRD-09-BE-KOMPETENSI-DAN-KREDENSIAL.md](FIN-HRD-09-BE-KOMPETENSI-DAN-KREDENSIAL.md).

## 2. Proses bisnis

1. Pegawai/HR mencatat lisensi → selalu `PendingVerification`. SIP wajib menyebut faskes sesuai dokumen.
2. Edit lisensi (`PUT`) tidak dapat mengubah status verifikasi. Bila nomor, jenis, penerbit, lokasi praktik, tanggal, atau berkas berubah pada lisensi yang sudah terverifikasi, verifikasi dan PSV dibatalkan otomatis → kembali menunggu verifikasi.
3. Verifikasi dokumen (`PATCH …/verify`) oleh petugas lain (bukan pemilik): status mengikuti keputusan (`Verified`/`Rejected`); dokumen wajib harus ada; penolakan wajib beralasan dan juga membatalkan PSV.
4. PSV (`POST /licenses/{id}/verify-psv`) oleh pemegang `ProfessionalLicense : VerifyPsv` yang bukan pemilik: rujukan wajib alamat http(s) portal sumber primer; lisensi dicabut ditolak; lisensi kedaluwarsa tidak dapat dinyatakan valid; SIP tanpa faskes ditolak. Jejak PSV ditambahkan (larik), bukan ditimpa.
5. Kelayakan permohonan kredensial dan gate klinis hanya menerima lisensi berjenis `SIP` (STR seumur hidup tidak dihitung sebagai SIP), lolos PSV, belum kedaluwarsa, dan mencatat faskes.

**Contoh.** dr. A mengunggah SIP lalu mengubah nomornya sesudah diverifikasi → status kembali *Menunggu Verifikasi* dan gate klinis menolak sampai PSV diulang.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpCredentialLicenseController.cs`, `ProfessionalLicenseController.cs`, `CredentialVerificationService.cs`, `ClinicalPrivilegeGateService.cs`, `CredentialingWorkflowService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/CredentialingManagement/Controllers/WfpCredentialLicenseController.cs` | PUT mengabaikan status verifikasi + reset otomatis bila data substantif berubah (`HasSubstantiveLicenseChange`); PATCH verify: status turunan keputusan, dokumen wajib, alasan penolakan, reset PSV saat ditolak |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/CredentialVerificationService.cs` | SIP wajib faskes (tanpa isian otomatis); PSV: rujukan http(s), tolak lisensi dicabut/kedaluwarsa/SIP tanpa faskes, jejak audit append (`AppendPsvAuditEntry`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; field `IsVerified`/`VerificationStatus`/`VerificationNotes` pada body PUT kini diabaikan; `VerificationStatus` pada body PATCH verify diabaikan; `SipFaskesName` wajib untuk SIP |
| Database | Tanpa migration (`PsvAuditLogJson` kini berisi larik) |
| Keamanan/Auth | Self-verification tertutup di PUT, PATCH verify, dan PSV |

## 4. Dokumentasi endpoint

#### Workforce Credential License

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/credential-licenses/{id}` | Ubah data (tanpa status verifikasi) | `WorkforceCredentialLicense : Update` |
| `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/credential-licenses/{id}/verify` | Verifikasi/tolak dokumen | `WorkforceCredentialLicense : Update` |
| `POST` | `/api/v1/corporate/hrd/credentialing/licenses` | Daftar lisensi (SIP wajib faskes) | `ProfessionalLicense : Create` |
| `POST` | `/api/v1/corporate/hrd/credentialing/licenses/{id}/verify-psv` | PSV berbukti | `ProfessionalLicense : VerifyPsv` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: PUT dengan `IsVerified=true` | Status tidak berubah | `PASS` | assignment dihapus |
| Penelusuran: ubah nomor SIP terverifikasi | Kembali `PendingVerification`, PSV batal | `PASS` | `HasSubstantiveLicenseChange` |
| Penelusuran: PSV oleh pemilik / rujukan bukan URL / lisensi dicabut | Ditolak | `PASS` | `PerformPrimarySourceVerificationAsync` |
| Penelusuran: SIP tanpa faskes | Pendaftaran ditolak; tidak lolos kelayakan permohonan | `PASS` | `RegisterLicenseAsync`, `CreateApplicationAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Pemohon tidak bisa verify sendiri atau spoof ID | Terpenuhi | anti-self di PATCH verify dan PSV; PUT tidak menyentuh status; ID rute = ID body pada PSV |
| (2) SIP kedaluwarsa/beda faskes gagal | Terpenuhi | kelayakan permohonan + gate menuntut SIP berlaku, ber-PSV, berfaskes; tanpa isian faskes otomatis |
| (3) STR seumur hidup tidak dianggap SIP | Terpenuhi | filter `LicenseType == "SIP"`; SIP seumur hidup ditolak saat daftar |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dampak frontend | Form SIP wajib mengisi nama faskes; tombol verifikasi tidak lagi mengirim status bebas; pesan reset verifikasi sesudah edit |
| Sisa | Kecocokan nama faskes dengan master site tidak dipaksakan (nama bebas); petugas PSV mencocokkan di portal |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
