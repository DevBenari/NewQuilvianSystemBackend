# Laporan Perubahan Backend — `BE-HRD-430`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-430` |
| Judul | Eligibility aplikasi kredensial dan penunjukan Mitra Bestari |
| Slice | `S-FIN-M09` / Status Kredensial (alias `FIN-HRD-09-BE-005`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-430` |
| Trace | M09-P0-05, M09-P0-06; instruksi Menu 09 `FIN-HRD-09-BE-005` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-427` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `CredentialingWorkflowService`, `CredentialingWorklistQueryService`, `SpkRkkIssuanceController`, `CredentialingDomainExceptions.cs` (baru); laporan, roadmap, traceability |
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

Kelayakan permohonan hanya mencari satu SIP ber-PSV apa pun (tanpa faskes/profesi), syarat wajib profesi lain tidak diperiksa, nomor permohonan `Count+1`, dan dua pengajuan paralel dapat lolos cek "masih ada permohonan berjalan". Panel reviewer disimpan di `ComplianceResultJson` yang ditimpa saat pleno; reviewer tidak dicek profesi/akun, dan penunjukan bisa dilakukan pada status apa pun.

## 2. Proses bisnis

1. Pengajuan (`POST {H}/applications`) diserialkan per pegawai (kunci transaksi). Lolos bila: SIP relevan (berjenis SIP, aktif, ber-PSV, berlaku, berfaskes, seprofesi bila master lisensi terikat profesi) **dan** seluruh syarat wajib profesi bertipe lisensi/sertifikat (master `MstCredentialingRequirement`) didukung bukti terverifikasi yang berlaku. Nomor `APP-CRED-yyyyMM-NNNN` dari `NumberSeriesAllocator`.
2. Penunjukan panel (`POST …/assign-reviewers`) hanya pada status `Submitted`/`InReview`: minimal 2 reviewer berbeda, bukan pemohon, profil aktif, **seprofesi** dengan pemohon, dan memiliki akun aktif (penilaian dikirim dari akun sendiri).
3. Setiap reviewer disimpan sebagai satu baris `TrxCredentialingCommitteeReview` (peran `PeerReviewer`, nomor `CRV-RSMMC-NNNNNN`). Penunjukan ulang menandai anggota yang dikeluarkan `Withdrawn`; anggota yang sudah menilai tidak dapat dikeluarkan (`409`).
4. Pleno tidak lagi menghapus panel (panel bukan di JSON).

**Contoh.** Perawat dengan SIP faskes lain yang belum ber-PSV → `422`; reviewer bidan untuk pemohon dokter → `422`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`CredentialingWorkflowService.cs`, `CredentialingWorklistQueryService.cs`, `TrxCredentialingCommitteeReview` + konfigurasinya, `SpkRkkIssuanceController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/CredentialingWorkflowService.cs` | Kunci per pegawai/permohonan; SIP relevan; syarat wajib profesi; nomor allocator; panel persisten di `TrxCredentialingCommitteeReview`; validasi profesi/akun/status |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/CredentialingDomainExceptions.cs` | `CredentialingConflictException` (→ `409`) |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/CredentialingWorklistQueryService.cs` | Konstanta peran/status panel; `PeerReviewEvidence`; jumlah reviewer dari baris panel |
| `Areas/Corporate/HumanResource/CredentialingManagement/Controllers/SpkRkkIssuanceController.cs` | Pemetaan `403`/`409`/`422` pada aksi tulis |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; Kode galat aksi tulis kredensial kini: `403` aktor bukan pemilik peran, `409` status tidak mengizinkan aksi, `422` validasi bisnis (sebelumnya `400` untuk semuanya) |
| Database | Tanpa migration — memakai tabel existing `TrxCredentialingCommitteeReview` |
| Keamanan/Auth | Hak existing `SpkRkkIssuance : CreateApplication` / `AssignReviewer` |

## 4. Dokumentasi endpoint

#### HR Credentialing - Committees, Peer Review & SPK-RKK Issuance

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/hrd/credentialing/applications` | Ajukan permohonan | `SpkRkkIssuance : CreateApplication` |
| `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id}/assign-reviewers` | Tetapkan panel | `SpkRkkIssuance : AssignReviewer` |
| `GET` | `/api/v1/corporate/hrd/credentialing/applications/{id}` | Detail + nama panel | `SpkRkkIssuance : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: SIP ber-PSV tetapi tanpa faskes / beda profesi | `422` | `PASS` | filter SIP relevan |
| Penelusuran: dua pengajuan paralel | Yang kedua `409` | `PASS` | kunci `HRD_CRED_APPLICATION_{profil}` |
| Penelusuran: reviewer beda profesi / pemohon sendiri | `422` | `PASS` | validasi panel |
| Penelusuran: pleno sesudah penunjukan | Baris panel tetap | `PASS` | panel di tabel, bukan JSON |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Lisensi tidak relevan tidak meloloskan aplikasi | Terpenuhi | SIP relevan + syarat wajib profesi |
| (2) Reviewer salah profesi/diri sendiri ditolak | Terpenuhi | validasi panel |
| (3) Panel reviewer tetap ada setelah pleno | Terpenuhi | baris `TrxCredentialingCommitteeReview` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Asumsi | Reviewer wajib memiliki akun aktif dan seprofesi (profesi dari Dokter/Pegawai/Pengguna Eksternal); dapat dilonggarkan pemilik |
| Data lama | Permohonan `InReview` yang panelnya hanya tersimpan di JSON lama perlu ditetapkan ulang panelnya |
| Dampak frontend | Tangani `409`/`422` (sebelumnya `400`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
