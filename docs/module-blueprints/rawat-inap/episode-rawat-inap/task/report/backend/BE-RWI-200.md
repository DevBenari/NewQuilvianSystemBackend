# Laporan Perubahan Backend — `BE-RWI-200`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-200` |
| Judul | Nilai Kepercayaan dan ringkasan hak pasien |
| Slice | Slice B; gelombang `RWA-MVP-2` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-200` |
| Trace | `FR-RWA-110` s.d. `113`; `RWI-DEC-242`; `RWI-AC-362`; G-41; API 12.2 `/patient-rights`; validation `VAL-RWA-16`, `23`; data 20.4, 20.8 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-194` ✅ |
| Klasifikasi | `MEDIUM` — data keyakinan sensitif |
| Task mode | `BACKEND` |
| Target tulis | `InpAdmissionDocumentService.Content.cs`, `InpAdmissionPrefillService.cs`, `InpAdmissionWorkspaceQueryService.cs`, controller |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi dua episode pasien samaran **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-LOG-001` |
| Wewenang | Source: ya |

---

## 1. Kebutuhan

Nilai Kepercayaan diisi per episode, dengan butir dari dokumen lengkap terakhir pasien sebagai konsep.
Modul lain (keperawatan, dokter) membaca ringkasan nilai kepercayaan dan privasi untuk keselamatan (G-41).

## 2. Proses bisnis

1. **Isian bawaan.** Prefill Nilai Kepercayaan menawarkan butir dari dokumen `Completed` terakhir pasien
   pada episode lain (`PreviousBeliefItems`, `PreviousBeliefDocumentId`). Dokumen baru tetap `Draft`
   sampai ditandatangani ulang.
2. **Butir.** Maksimal 5 butir; butir ke-6 → `400` "Maksimal 5 butir.".
3. **Kunci.** Ditolak `422 INP-ADM-DOC-023` bila tidak ada butir, atau nama, jenis kelamin, hubungan,
   alamat penanda tangan, atau kota dan tanggal kosong. Slot wajib: pasien/keluarga.
4. **Ringkasan hak pasien.** `GET /patient-rights` (dijaga `InpatientEpisode : Read`, pola alias) hanya
   memuat dokumen `Completed` episode ini; versi `Superseded` tidak ikut. Contoh `SummaryText`: "Nilai
   kepercayaan: 2 butir; Privasi khusus: hanya 2 kerabat; privasi transportasi: Ya". Ringkasan yang sama
   tampil di header ruang kerja.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Validation `VAL-RWA-16`, `23`; permission matrix 10.2 (pengecualian `patient-rights`); data 20.4, 20.8.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpAdmissionDocumentService.Content.cs` | `ApplyBelief` (≤ 5), validasi kunci `023` |
| `Services/InpAdmissionPrefillService.cs` | `FindPreviousBeliefDocumentAsync` |
| `Services/InpAdmissionWorkspaceQueryService.cs` | `GetPatientRightsAsync`, `LoadPatientRightsAsync` |
| `Controllers/InpatientAdmissionDocumentController.cs` | `GET /patient-rights` dengan `[AccessPermission("InpatientEpisode", "Read")]` tanpa `[AccessAction]` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | API 12.2 `/patient-rights` |
| Database | Tabel `InpAdmissionBeliefItem` (`CK_…_ItemNo` 1–5) |
| Keamanan/Auth | Isi keyakinan tidak pernah masuk logger; ringkasan hanya untuk pemegang `InpatientEpisode : Read` |

## 4. Dokumentasi endpoint

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/admission-workspace/patient-rights` | Ringkasan Nilai Kepercayaan dan Permintaan Privasi `Completed` | `InpatientEpisode : Read` |

Dokumen Nilai Kepercayaan memakai endpoint dokumen umum ([`BE-RWI-193` bagian 4](BE-RWI-193.md#4-dokumentasi-endpoint)).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pola alias hak akses | Didukung `PermissionRegistryDescriptor` (kunci `InpatientEpisode : Read` didaftarkan controller episode) | `PASS` | Review source |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Dua episode pasien samaran | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Episode baru menampilkan butir episode lalu sebagai konsep; belum lengkap sampai ditandatangani ulang | Terpenuhi (source) | Prefill; dokumen baru selalu `Draft` |
| 2. Butir ke-6 `400` "Maksimal 5 butir."; kunci tanpa butir `422` | Terpenuhi (source) | `ApplyBelief`; `ValidateForLock` |
| 3. Nama, jenis kelamin, hubungan, alamat wajib saat kunci | Terpenuhi (source) | `ValidateForLock` (`023`) |
| 4. `/patient-rights` hanya `Completed`, tidak `Superseded` | Terpenuhi (source) | `LoadPatientRightsAsync` |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tinjauan privasi data keyakinan adalah gerbang produksi G-35 |
| Masalah yang diketahui | — |
| Risiko tersisa | G-35 |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | Berkas Workspace PPRI bersama (`??`) |
| Langkah berikutnya | Uji pengguna dengan dua episode |
