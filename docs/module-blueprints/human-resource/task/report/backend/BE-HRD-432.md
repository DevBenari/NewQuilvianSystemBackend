# Laporan Perubahan Backend — `BE-HRD-432`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-432` |
| Judul | Draf, nomor, pengesahan, PDF SPK/RKK, QR, dan arsip |
| Slice | `S-FIN-M09` / Status Kredensial (alias `FIN-HRD-09-BE-007`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-432` |
| Trace | M09-P0-08, M09-P0-09; instruksi Menu 09 `FIN-HRD-09-BE-007` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-431` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `SpkRkkIssuanceService`, `SpkRkkIssuanceController`, `Program.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `CredentialingManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Nomor SPK/RKK masih `CountAsync`+1 dengan loop tabrakan (tidak aman paralel); dua pengesahan paralel dapat menggandakan arsip e-Dossier dan kewenangan; kegagalan tulis PDF/QR hanya dicatat log lalu SPK tetap `Active`; snapshot kewenangan yang gagal dibaca tetap berakhir "disahkan" tanpa kewenangan; gambar QR berupa PNG 1×1 placeholder; PDF SPK di bawah `/uploads` dapat diakses statis.

## 2. Proses bisnis

1. Draf (`POST {H}/spk-rkk/issue-draft`) diserialkan per permohonan; nomor `SPK|RKK/RSQ-DIRUT/{bulan romawi}/{tahun}/{NNNN}` dari `NumberSeriesAllocator` (reset tahunan, batas bawah nomor lama).
2. Pengesahan (`POST …/sign-dirut`) diserialkan per SPK dan hanya untuk status `Draft` (sudah disahkan/bukan Draf → `409`).
3. Sebelum disahkan: snapshot kewenangan harus terbaca, tidak kosong, dan seluruh katalog ada — bila tidak, `422` dan tidak ada perubahan.
4. PDF SPK, PDF RKK, dan QR wajib terbentuk di penyimpanan; gagal → `422`, transaksi tidak di-commit, SPK tetap Draf, kewenangan tidak aktif.
5. QR berisi alamat verifikasi publik `{FileStorage:PublicBaseUrl}/api/v1/public/clinical-privileges/verify/{hash}` (QRCoder).
6. PDF di `/uploads/storage/spk-rkk` ditutup untuk akses statis; unduh lewat `GET {H}/spk-rkk/{id}/files` berizin.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`SpkRkkIssuanceService.cs`, `SpkRkkIssuanceController.cs`, `SpkRkkPublicVerificationController.cs`, `Program.cs` (static files), `appsettings.json` (`FileStorage:PublicBaseUrl`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/SpkRkkIssuanceService.cs` | Allocator urutan SPK; kunci draf/sign; status Draf; snapshot ketat; berkas fail-closed; QR asli (`BuildVerificationQrPng`) |
| `Areas/Corporate/HumanResource/CredentialingManagement/Controllers/SpkRkkIssuanceController.cs` | Draf/sign: `409`/`422` |
| `Program.cs` | Blok akses statis `/uploads/storage/spk-rkk` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; `409`/`422` pada draf/sign (sebelumnya `400`) |
| Database | Tanpa migration |
| Keamanan/Auth | PDF SPK/RKK tidak lagi terbuka lewat URL statis |

## 4. Dokumentasi endpoint

#### HR Credentialing - Committees, Peer Review & SPK-RKK Issuance

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/hrd/credentialing/spk-rkk/issue-draft` | Terbitkan draf | `SpkRkkIssuance : ParafDirector` |
| `POST` | `/api/v1/corporate/hrd/credentialing/spk-rkk/{id}/sign-dirut` | Sahkan | `SpkRkkIssuance : SignDirector` |
| `GET` | `/api/v1/corporate/hrd/credentialing/spk-rkk/{id}/files` | Unduh PDF | `SpkRkkIssuance : Read` |
| `GET` | `/api/v1/public/clinical-privileges/verify/{hash}` | Verifikasi QR publik | anonim, rate limit |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: dua sign paralel | Kedua menunggu kunci; yang kedua `409` | `PASS` | kunci `HRD_SPK_RKK_SIGN_{id}` |
| Penelusuran: gagal tulis berkas | `422`, SPK tetap Draf | `PASS` | `throwOnFailure` |
| Penelusuran: isi QR | URL verifikasi publik | `PASS` | `BuildVerificationQrPng` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Dua signing paralel tidak menghasilkan dua nomor | Terpenuhi | nomor ditetapkan saat draf lewat allocator; sign dikunci per SPK |
| (2) QR dapat dipindai ke endpoint verifikasi | Terpenuhi | QR PNG nyata berisi URL verifikasi |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Prasyarat lingkungan | `FileStorage:PublicBaseUrl` harus berisi alamat publik API agar QR terbuka dari ponsel |
| Sisa | Nama jabatan penandatangan masih teks "Direktur Utama RS Quilvian" bila akun tidak memiliki nama tampilan (tidak ada master pejabat) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
