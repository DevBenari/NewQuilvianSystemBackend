# Laporan Perubahan Backend — `BE-HRD-398`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-398` |
| Judul | Segregasi verifikator, audit, dan masa berlaku dokumen |
| Slice | `S-FIN-M04` / Dokumen Personal (alias `FIN-HRD-04-BE-012`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-398` |
| Trace | `HRD-DEC-052`; instruksi Menu 04 `FIN-HRD-04-BE-012` |
| Contract version | Kontrak dokumen Workforce Core existing |
| Dependency | `BE-HRD-397` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Controllers/WfpDocumentController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001, QBE-LOG-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Larangan pembuat memverifikasi dokumennya sendiri (`SELF_VERIFICATION_PROHIBITED`), pencatatan verifikator dan waktunya, serta status H-30 dari tanggal server sudah ada. Celah: **dokumen yang sudah diverifikasi bisa diubah isinya tanpa mencabut verifikasi.** Contoh: STR diverifikasi petugas B dengan masa berlaku 2027, lalu petugas A mengubah tanggal kedaluwarsa menjadi 2030 — dokumen tetap tampil "terverifikasi" padahal B tidak pernah memeriksa tanggal baru.

## 2. Proses bisnis

1. Petugas A mengunggah dokumen → belum terverifikasi.
2. Petugas B (bukan pembuat) memverifikasi → `VerifiedByUserId` dan `VerifiedAt` tercatat. Dokumen rahasia hanya dapat diverifikasi pemegang hak rahasia (`BE-HRD-397`).
3. Bila jenis, nomor, tanggal terbit, tanggal kedaluwarsa, atau penerbit diubah, atau berkas diganti → status kembali **belum terverifikasi**, verifikator dan catatannya dikosongkan.
4. Status kedaluwarsa dan "kedaluwarsa dalam 30 hari" dihitung dari tanggal server saat dibaca.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpDocumentController.cs` (`UpdateDocument`, `VerifyDocument`, `DeleteDocumentFile`, `MapResponse`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/Controllers/WfpDocumentController.cs` | Perubahan isi dokumen mencabut verifikasi; verifikasi dokumen rahasia wajib hak rahasia |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; `IsVerified` kembali `false` sesudah isi diubah |
| Database | Tanpa migration |
| Keamanan/Auth | Verifikasi tidak lagi "menempel" pada data yang sudah berbeda |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Document

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/documents/{id}/verify` | Verifikasi oleh selain pembuat | `WorkforceDocument : Update` |
| `PUT` | `…/documents/{id}` | Ubah; perubahan isi mencabut verifikasi | `WorkforceDocument : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: pembuat memverifikasi | `403 SELF_VERIFICATION_PROHIBITED` | `PASS` (penelusuran) | `VerifyDocument` |
| Penelusuran: dokumen terverifikasi, tanggal kedaluwarsa diubah | `IsVerified = false`, verifikator kosong | `PASS` (penelusuran) | `substanceChanged` di `UpdateDocument` |
| Penelusuran: dokumen kedaluwarsa 10 hari lagi | `IsExpiringWithin30Days = true` (tanggal server) | `PASS` (penelusuran) | `MapResponse` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Verifikasi oleh pembuat ditolak `SELF_VERIFICATION_PROHIBITED` | Terpenuhi | `VerifyDocument` |
| (2) Aktor verifikasi tercatat | Terpenuhi | `VerifiedByUserId`, `VerifiedAt`, `UpdateBy`; dicabut bila isi berubah |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Pengunggah pengganti berkas (bukan pembuat awal) belum dicegah memverifikasi berkas yang ia unggah sendiri, karena tidak ada kolom "pengunggah berkas terakhir"; butuh migration bila ingin ditegakkan |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-399` |
