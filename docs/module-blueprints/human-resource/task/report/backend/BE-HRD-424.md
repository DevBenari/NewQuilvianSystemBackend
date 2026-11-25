# Laporan Perubahan Backend — `BE-HRD-424`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-424` |
| Judul | Integrasi lintas domain dan perlindungan data kontrak |
| Slice | `S-FIN-M08` / Lintas Submenu (alias `FIN-HRD-08-BE-009`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.5, kartu `BE-HRD-424` |
| Trace | Instruksi Menu 08 `FIN-HRD-08-BE-009` |
| Contract version | Kontrak Kontrak Kerja existing (`BE-HRD-065`, `BE-HRD-080`) |
| Dependency | `BE-HRD-417` ✅, `BE-HRD-412` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `HrdEmploymentContractService`, kontrak service-nya, controller kontrak, `HrdUnitScopeService`, penanda `EmploymentContractExplicitPermissions.cs` (baru); laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build, QBE, registri lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `LifecycleManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Fallback departemen, penguncian struktur gaji ke grade tawaran, dan filter `hospitalSiteId` pada ringkasan sudah ada. Celah: daftar, detail, cetak, dan unduh scan kontrak tidak dibatasi cakupan — siapa pun dengan hak `Read` melihat kontrak (termasuk gaji di data cetak) seluruh rumah sakit.

## 2. Proses bisnis

1. Pemegang penanda `WorkforceEmploymentContract : ReadAllUnits` (dicentang admin di Akses Role) melihat kontrak seluruh RS.
2. Selain itu, cakupan = kontrak milik sendiri + kontrak pegawai yang penempatan organisasinya (hari ini) berada di unit yang dipimpin pengguna beserta unit turunannya.
3. Kontrak di luar cakupan → tidak ada di daftar dan `404` pada detail/cetak/unduh.
4. Penanda semua-unit milik jadwal/presensi tidak membuka kontrak (cakupan memakai `GetManagerialReadScopeAsync` baru).
5. Handoff non-renew ke Menu 12 tetap satu kali (`BE-HRD-366`, idempoten per pemberhentian).

**Contoh.** Kepala Ruang ICU melihat kontrak perawat ICU dan kontraknya sendiri; kontrak staf Farmasi tidak muncul.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdEmploymentContractService.cs` (empat aksi baca), `HrdUnitScopeService.cs`, controller kontrak, handoff non-renew.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/LifecycleManagement/EmploymentContractExplicitPermissions.cs` | Penanda `ReadAllUnits` (baru) |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdUnitScopeService.cs` | `GetManagerialReadScopeAsync` (unit dipimpin + turunan, tanpa penanda domain lain) |
| `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentContractService.cs` | `ApplyContractReadScopeAsync` pada daftar, detail, cetak, unduh |
| `Areas/Corporate/HumanResource/LifecycleManagement/Services/IHrdEmploymentContractService.cs` | Parameter `ClaimsPrincipal? user` opsional |
| `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/HrdEmploymentContractController.cs` | Meneruskan `User` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; data di luar cakupan tidak tampil / `404` |
| Database | Tanpa migration |
| Keamanan/Auth | **Perubahan perilaku:** peran yang dulu melihat semua kontrak kini perlu penanda `Read Employment Contracts Of All Units` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Lifecycle Management / Employment Contracts

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/employment-contracts` | Daftar sesuai cakupan | `WorkforceEmploymentContract : Read` (+ `ReadAllUnits`) |
| `GET` | `/api/v1/corporate/human-resource/employment-contracts/{id}` | Detail sesuai cakupan | idem |
| `GET` | `/api/v1/corporate/human-resource/employment-contracts/{id}/print-data` | Data cetak sesuai cakupan | idem |
| `GET` | `/api/v1/corporate/human-resource/employment-contracts/{id}/download-scan` | Unduh scan sesuai cakupan | idem |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (berkas Menu 08) | 9 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: kepala unit membuka kontrak unit lain | `404` | `PASS` | `ApplyContractReadScopeAsync` |
| Penelusuran: pegawai membuka kontraknya sendiri | Berhasil | `PASS` | cabang pemilik |
| Validator registri hak akses | `IsValid=True` (penanda baru terdaftar) | `PASS` | skrip `dotnet fsi` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Peran lintas unit tidak dapat GET kontrak di luar scope | Terpenuhi | cakupan pada empat aksi baca |
| (2) Handoff non-renew diterima Menu 12 tanpa duplikasi | Terpenuhi | handoff idempoten (`BE-HRD-366`/`369`) |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan untuk admin | Centang `Read Employment Contracts Of All Units` untuk peran HR yang mengelola kontrak seluruh RS |
| Catatan rute | Rute cetak/unduh mengikuti controller existing; lihat Swagger tag di atas |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
