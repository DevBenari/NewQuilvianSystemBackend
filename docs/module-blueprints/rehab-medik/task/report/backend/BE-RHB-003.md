# Laporan Perubahan Backend — `BE-RHB-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-003` |
| Judul | Jenis instrumen rehab di ClinicalManagement |
| Slice | S-A — Fondasi (`MVP-0`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-003` |
| Trace | RHB-DEC-017, RHB-DEC-018; `02-backend-architecture.md` §7.2 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 |
| Dependency | — (gelombang 1) |
| Klasifikasi | `LIGHT` — satu berkas enum, tanpa schema, tanpa endpoint |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs` + dokumen blueprint `rehab-medik` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`) |
| Tanggal | 2026-11-05 |
| Status | **Selesai** (2026-11-05). Build dijalankan owner dan sukses. Bukti ada di §5. |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` (`Cli`, `ACTIVE / LEGACY`) |
| Applicability | `TOUCHED LEGACY`, hanya menambah nilai enum di ujung |
| Registry | Baris `Cli` sudah ada. Tidak ada entity baru, sehingga QBE-MOD-002 tidak berlaku. |
| QBE yang berlaku | QBE-API-001 (tidak mengubah kontrak existing) |
| Persetujuan modul lain | Pemilik ClinicalManagement, sesuai RHB-DEC-018 (owner menyatakan dirinya pemilik atau sudah memperoleh persetujuan, 2026-11-05) |

## 1. Masalah yang diperbaiki

Mesin instrumen klinis memilih instrumen yang berlaku **per jenis per pasien**. Sebelum
perubahan ini hanya ada tujuh jenis, semuanya untuk keperawatan rawat inap. Akibatnya, komite
tidak punya "wadah" untuk mendaftarkan Indeks Barthel, Berg, TUG, Lawton, GUSS, atau FOIS. Kalau
dipaksakan ke jenis yang sudah ada (misalnya `PainScale`), Barthel dan Berg tidak bisa dibedakan
oleh mesin.

**Contoh.** Seorang fisioterapis ingin mengisi Berg Balance Scale. Tanpa jenis
`RehabBergBalanceScale`, sistem tidak punya cara menanyakan "versi Berg mana yang sudah disahkan
untuk pasien ini?".

## 2. Proses bisnis

1. Admin konfigurasi membuat instrumen baru, misalnya "Berg Balance Scale", dengan jenis
   `RehabBergBalanceScale` di layar instrumen klinis yang sudah ada.
2. Admin menyusun versi konsep (butir dan skor `sum`).
3. Komite medik mengesahkan versi itu. Pengesah harus berbeda dengan pengubah terakhir, dan aturan
   ini sudah ada di `ClinicalInstrumentService`.
4. Formulir asesmen rehab (task `BE-RHB-006` dan `BE-RHB-009`) meminta instrumen jenis itu untuk
   pasien, lalu mendapat versi yang sudah disahkan.

Jalur tidak normal: bila versi belum disahkan, instrumen tidak tersedia di lingkungan produksi.
Pengukuran tetap bisa dicatat sebagai pengukuran terstruktur (RHB-DEC-017).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs`
- Seluruh pemakaian `ClinicalInstrumentKind` (grep): `ClinicalInstrumentService.cs` (validasi
  `Enum.IsDefined` baris 194, resolusi baris 566/582), `NursingAssessmentDocumentService.cs`
  (`KindFor` dan percabangan `if`), `CaseManagementEvaluationService.cs`,
  `CliSurgicalSiteSurveillanceService.cs`, `ClinicalInstrumentDraftSeeder.cs`,
  `ClinicalInstrumentController.cs`, `PatientAssessmentController.cs`, `ClinicalInstrumentDtos.cs`
- FE (baca saja): `src/utils/health-services/clinical-management/clinical-instrument-utils.js`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs` | Tambah `RehabBarthelIndex = 8`, `RehabBergBalanceScale = 9`, `RehabTimedUpAndGo = 10`, `RehabLawtonIadl = 11`, `RehabGussSwallowing = 12`, `RehabFoisOralIntake = 13` di ujung enum, beserta komentar asal keputusan. Nilai 1–7 tidak berubah. |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. Endpoint `clinical-instruments` existing kini menerima nilai `instrumentKind` 8–13 karena validasinya `Enum.IsDefined`. Ini sesuai kontrak rehab v1 (§7.2). |
| Database | Tidak ada perubahan schema. Kolom `InstrumentKind` sudah `int`. Tidak ada migration. |
| Keamanan/Auth | `NOT APPLICABLE` — hak akses `ClinicalInstrumentConfiguration` tidak berubah |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint yang dibuat atau diubah.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review diff (`git diff --stat`) | 1 berkas, +27/−1, hanya enum | `PASS` | Diff pada §3.2 |
| Nilai lama tidak bergeser | Nilai 1–7 identik, nilai baru ditambah di ujung | `PASS` | Diff |
| Pencarian switch/peta tanpa `default` atas `ClinicalInstrumentKind` di BE | Tidak ditemukan switch atas jenis instrumen. `KindFor` memetakan `PatientAssessmentType` → jenis (arah sebaliknya), sehingga nilai baru tidak menyentuhnya. | `PASS` | grep §3.1 |
| Validasi pembuatan instrumen menerima nilai baru | `Enum.IsDefined(request.InstrumentKind)` menerima 8–13 | `PASS` (statis) | `ClinicalInstrumentService.cs:194` |
| Line ending berkas | Tetap CRLF, UTF-8 | `PASS` | `file` |
| `dotnet build` (Debug, dijalankan owner 2026-11-05) | Berhasil. Output yang ditempel owner terpotong di batas pesan; bagian yang terlihat hanya berisi warning legacy di modul lain, tanpa baris `error` dan tanpa warning pada `ClinicalInstrumentKind.cs`. | `PASS` | (1) `bin/Debug/net9.0/QuilvianSystemBackend.dll` ditulis 11:11:37, sesudah enum diubah pukul 10:38:43. Compiler tidak menulis DLL bila kompilasi gagal. (2) Keenam nama `RehabBarthelIndex`..`RehabFoisOralIntake` ditemukan di DLL tersebut (grep biner, masing-masing 1 kemunculan). |
| QBE conformance checker | — | `NOT RUN` | Menunggu izin owner |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `REQUIRED` — setelah build, buat satu instrumen konsep berjenis `RehabBergBalanceScale`
lewat `POST /api/v1/health-services/clinical-management/clinical-instruments`, lalu pastikan
respons 201 dan instrumen tersimpan dengan `InstrumentKind = 9`.

**Tidak dijalankan:** QBE checker dan uji manual API (menunggu izin owner). Uji manual API tidak termasuk kriteria penerimaan kartu ini, sehingga tidak memblokir ✅.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Nilai 1–7 tidak bergeser | Terpenuhi | Diff |
| (2) **r2:** endpoint `clinical-instruments` menerima jenis 8–13 tanpa perubahan lain di backend; label per jenis di backend ditambahkan bila ada | Terpenuhi (statis): validasi `Enum.IsDefined`, dan tidak ada daftar label di BE. Pilihan jenis di FE dipindah ke `FE-RHB-002` (roadmap r2). | §3.1, temuan §7 |
| DoD: build owner sukses | Terpenuhi | §5 |
| DoD: laporan menyebut penyetuju | Terpenuhi | Metadata (RHB-DEC-018) |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | **Temuan FE:** daftar `CLINICAL_INSTRUMENT_KIND_OPTIONS`/`LABELS` di FE juga tidak memuat `SurgicalSiteSurveillanceForm = 7` (selisih legacy, di luar scope). Supaya komite dapat memilih jenis rehab, FE perlu menambah nilai 8–13. Usulan: masukkan ke cakupan `FE-RHB-002` (konfigurasi layanan rehab) sebagai delta, atau task FE tersendiri. Keputusan ada pada owner. |
| Risiko tersisa | Bila FE tidak diperbarui, instrumen rehab hanya bisa dibuat lewat API langsung |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs`, `M docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (task sebelumnya), `?? docs/module-blueprints/rehab-medik/` |
| Langkah berikutnya | `BE-RHB-000` (checker QBE) untuk membuka `BE-RHB-001`. Sesudah itu `BE-RHB-005` boleh mulai, karena prasyarat `BE-RHB-003`-nya sudah ✅. |
