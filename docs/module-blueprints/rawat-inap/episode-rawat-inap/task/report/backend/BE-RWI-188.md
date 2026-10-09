# Laporan Perubahan Backend — `BE-RWI-188`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-188` |
| Judul | Service baca profil rumah sakit (HR Master Data) |
| Slice | Slice A; gelombang `RWA-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-188` |
| Trace | `FR-RWA-126`; `RWI-DEC-247`, `264`, `266`; `NFR-RWA-11`; backend 13.6, 13.8.4; `INT-RWA-12` |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`); perubahan service modul lain disetujui `RWI-DEC-266` |
| Dependency | — |
| Klasifikasi | `LIGHT` — satu service baca di modul lain |
| Task mode | `BACKEND` |
| Target tulis | `Areas/Corporate/HumanResource/MasterData/Organization/Services/` (folder baru), `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Pembacaan pada data situs uji **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / HR Master Data — Organization (pemilik `MstHospitalSite`) |
| Prefix registry | `Mst` — `ACTIVE` (area Corporate tercakup di salinan registry repo) |
| Keberlakuan | `NEW CODE` (service saja); tidak ada entity baru |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-MOD-001` |
| Wewenang | Service baca disetujui `RWI-DEC-266`; `HospitalSiteController` tidak disentuh |

---

## 1. Kebutuhan

Kop surat seluruh cetakan Workspace PPRI harus dibaca dari profil situs rumah sakit utama, bukan
ditanam di kode (`RWI-DEC-247`, kelemahan V1).

## 2. Proses bisnis

1. `GetMainSiteProfileAsync()` membaca situs aktif bertanda `IsMainSite`: nama, kode, baris alamat beserta
   nama wilayah, telepon, email, dan zona waktu.
2. Tidak ada situs utama aktif → `IsAvailable = false` dengan alasan "Belum ada situs rumah sakit utama
   yang aktif…". Lebih dari satu → `IsAvailable = false` dengan alasan "Lebih dari satu situs rumah sakit
   utama aktif; profil tidak ditebak…". Tidak ada nilai bawaan yang dikarang.
3. Pemakai (Workspace PPRI) mencetak kop tanpa identitas bila profil tidak tersedia, dan memakai
   `Asia/Jakarta` bila zona waktu kosong.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstHospitalSite` beserta relasi wilayah; `HospitalSiteController` (tidak diubah).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Corporate/HumanResource/MasterData/Organization/Services/HospitalSiteProfileQueryService.cs` | Baru — `GetMainSiteProfileAsync`, kelas `HospitalSiteProfile` |
| `Program.cs` | `AddScoped<HospitalSiteProfileQueryService>` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru maupun berubah |
| Database | Tidak ada |
| Keamanan/Auth | Petugas Workspace PPRI tidak perlu hak `HospitalSite : Read` (`RWI-DEC-257`) |

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Kop surat dibuka lewat `GET …/admission-workspace/letterhead` (`BE-RWI-193`).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight; batas modul | Service baca di folder pemilik; `AsNoTracking` | `PASS` | Review source |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Pembacaan data situs uji (nol, satu, dua situs utama) | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Nama, kode, alamat beserta wilayah, telepon, email, zona waktu situs utama aktif | Terpenuhi (source) | `GetMainSiteProfileAsync` |
| 2. Nol atau lebih dari satu situs utama → `IsAvailable = false` beserta alasan, tanpa menebak | Terpenuhi (source) | Dua cabang penolakan |
| 3. Tidak ada tabel/endpoint HR berubah | Terpenuhi | Diff hanya folder service baru |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kop surat kosong bila master situs belum ditandai satu situs utama aktif |
| Masalah yang diketahui | — |
| Risiko tersisa | — |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | `??` `Areas/Corporate/HumanResource/MasterData/Organization/Services/`; `M` `Program.cs` |
| Langkah berikutnya | Pastikan tepat satu situs utama aktif di lingkungan uji |
