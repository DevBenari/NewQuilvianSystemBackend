# Laporan Perubahan Backend — `BE-HRD-429`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-429` |
| Judul | Evaluasi kepatuhan persyaratan jabatan |
| Slice | `S-FIN-M09` / Persyaratan Jabatan (alias `FIN-HRD-09-BE-004`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-429` |
| Trace | Instruksi Menu 09 `FIN-HRD-09-BE-004` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-427` ✅, `BE-HRD-428` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `CompetencyController`, penanda `CompetencyComplianceExplicitPermissions.cs` (baru); laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build, QBE, registri lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/CompetencyAndCredential` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

`GET …/competencies/positions/{positionId}/compliance` sudah ada beserta parameter `asOfDate`, tetapi: siapa pun dengan `Competency : Read` dapat mengevaluasi pegawai mana pun (Kepala Unit melihat unit lain); bukti lisensi dihitung tanpa cek kedaluwarsa pada tanggal acuan; sertifikat tertolak tampil "Perlu Verifikasi"; bila ada beberapa bukti cocok, yang dipilih acak.

## 2. Proses bisnis

1. Tanpa `workforceProfileId` → daftar syarat jabatan saja.
2. Dengan `workforceProfileId` → dievaluasi hanya bila pemanggil memegang penanda baru `Competency : ReadAllUnitsCompliance`, atau profil itu dirinya sendiri, atau pegawai di unit yang dipimpinnya (beserta turunan). Di luar cakupan → `404`.
3. Bukti dinilai per tanggal acuan: sertifikat terbit ≤ tanggal acuan; tertolak diabaikan; terverifikasi & berlaku didahulukan; belum terverifikasi → *Perlu Verifikasi*; kedaluwarsa → *Belum Memenuhi*. Lisensi hanya dihitung bila ber-PSV dan berlaku pada tanggal acuan.

**Contoh.** Kepala Ruang ICU mengevaluasi perawat ICU → matriks tampil; mencoba perawat IGD → `404`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`CompetencyController.cs` (`GetPositionCompliance`), `HrdUnitScopeService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/CompetencyComplianceExplicitPermissions.cs` | Penanda `Competency : ReadAllUnitsCompliance` (baru) |
| `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Controllers/CompetencyController.cs` | `CanEvaluateProfileComplianceAsync`; bukti per tanggal acuan; urutan pemilihan bukti; sertifikat tertolak diabaikan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; profil di luar cakupan → `404` |
| Database | Tanpa migration |
| Keamanan/Auth | **Perubahan perilaku:** HR yang mengevaluasi seluruh unit perlu penanda `Read Position Compliance Of All Units` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Competency

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/master-data/competencies/positions/{positionId}/compliance?workforceProfileId=&asOfDate=` | Matriks kepatuhan per butir | `Competency : Read` (+ `ReadAllUnitsCompliance`) |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: sertifikat Pending | *Perlu Verifikasi*, bukan *Memenuhi* | `PASS` | cabang `!IsVerified` |
| Penelusuran: kepala unit membuka pegawai unit lain | `404` | `PASS` | `CanEvaluateProfileComplianceAsync` |
| Validator registri hak akses | `IsValid=True` (2.648 identitas) | `PASS` | skrip `dotnet fsi` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Bukti belum terverifikasi tidak dianggap memenuhi | Terpenuhi | status *Perlu Verifikasi*; lisensi wajib PSV & berlaku |
| (2) Kepala unit tidak melihat unit lain | Terpenuhi | cakupan manajerial + penanda semua-unit |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan untuk admin | Centang `Read Position Compliance Of All Units` untuk peran HR/Komite yang mengevaluasi seluruh RS |
| Sisa | Pencocokan sertifikat ke kompetensi masih berbasis kode/nama (tanpa relasi master langsung) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
