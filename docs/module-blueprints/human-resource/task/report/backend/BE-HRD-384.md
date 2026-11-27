# Laporan Perubahan Backend — `BE-HRD-384`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-384` |
| Judul | Template naskah SURAT TUGAS dari pengaturan yang memuat daftar peserta |
| Slice | `S-FIN-ADM` / Surat Tugas (alias `ST-IMP (template, BUG-HRD-ADM-02)`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.1, kartu `BE-HRD-384` |
| Trace | `BUG-HRD-ADM-02`; baseline `BE-HRD-202`, `BE-HRD-203` |
| Contract version | Kontrak Administrasi Kepegawaian (`docs/update/administrasi-kepegawaian`); endpoint baru dicatat sebagai delta |
| Dependency | `BE-HRD-382` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `AssignmentLetterService.RenderLetterHtml`, `HrLetterheadResolver`, template tugas pada `EmployeeDocumentIssuanceService`, verifikasi publik; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a7c37c56` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build, QBE lulus; Migration `20261120093000_AddHrdAssignmentLetter` ditulis, sudah diterapkan 21 November 2026 ke DB dev `QuilvianNewDevHamzah` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `HrServiceManagement` |
| Keberlakuan | `NEW CODE` (Surat Tugas, registri) + `TOUCHED LEGACY` (Surat Keterangan) |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Template tugas pada Surat Keterangan berjudul "SURAT PERINTAH TUGAS" dengan kop, alamat, dan telepon RS tertanam di HTML, untuk satu pegawai; tidak ada naskah yang diterbitkan dari aggregate Surat Tugas.

## 2. Proses bisnis

1. Saat diterbitkan, naskah **SURAT TUGAS** dirakit server: kop (nama, alamat, kontak) dan kota dari Master Lokasi RS; nomor resmi; dasar; **tabel peserta** (No, Nama, NIP/Kode, Jabatan, Unit, Peran); kegiatan, uraian tugas, tujuan, periode WIB; penandatangan = pengesah (nama & jabatan dibekukan saat pengesahan); QR berisi URL verifikasi.
2. Seluruh nilai di-encode HTML; naskah disimpan (`DocumentSnapshotHtml`) dan diunduh lewat `GET …/{id}/document` berizin — tanpa URL berkas statis.
3. QR diverifikasi lewat endpoint publik yang sama dengan Surat Keterangan; nama peserta disamarkan; surat dibatalkan dinyatakan tidak berlaku.
4. Template tugas pada jalur Surat Keterangan kini juga berjudul SURAT TUGAS dan memakai kop dari pengaturan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EmployeeDocumentIssuanceService.cs` (template bawaan), `HrPublicDocumentVerificationController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/HrServiceManagement/Services/AssignmentLetterService.cs` | `RenderLetterHtml` (tabel peserta, kop, QR data URI) |
| `Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs` | Verifikasi publik Surat Tugas; judul template tugas "SURAT TUGAS" dengan token kop |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET …/assignment-letters/{id}/document`; verifikasi publik menerima hash Surat Tugas |
| Database | Kolom naskah pada tabel baru |
| Keamanan/Auth | Unduh naskah dibatasi cakupan baca Surat Tugas |

## 4. Dokumentasi endpoint

#### HR Services - Assignment Letters (Surat Tugas)

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/assignment-letters/{id}/document` | Naskah SURAT TUGAS | `AssignmentLetter : Read` |
| `GET` | `/api/v1/public/hr-documents/verify/{hash}` | Verifikasi QR | anonim |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error; himpunan warning (tanpa nomor baris) sama dengan sebelum perubahan — tambahan 6 instans berasal dari commit pengguna `a7c37c56` pada `HrdInitialPlacementBatchService.cs`, bukan dari perubahan ini | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (18 berkas slice Administrasi Kepegawaian) | 18 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: naskah memuat tabel peserta dan kop dari pengaturan | Ya | `PASS` | `RenderLetterHtml` |
| Penelusuran: identitas RS belum diatur | Penerbitan `422` | `PASS` | `HrLetterheadResolver` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Jenis tugas selalu memakai template SURAT TUGAS | Terpenuhi | naskah aggregate + judul template tugas |
| (2) Kop, alamat, penandatangan dari pengaturan | Terpenuhi | Master Lokasi RS + pengesah |
| (3) Daftar peserta tercetak sebagai tabel | Terpenuhi | tabel peserta pada naskah |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Prasyarat data | Master Lokasi RS terisi; `FileStorage:PublicBaseUrl` atau `HumanResource:DocumentVerificationBaseUrl` diatur agar QR dapat dibuka |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |


## Pembaruan 21 November 2026 — Status Migration

Migration `20261120093000_AddHrdAssignmentLetter` sudah diterapkan ke database pengembangan `QuilvianNewDevHamzah`: pemilik menjalankan `dotnet ef database update --no-build` ("No migrations were applied. The database is already up to date.") dan `dotnet ef migrations list --no-build` terhadap `QuilvianNewDevHamzah` tidak menunjukkan satu pun migration `Pending`. Pernyataan "migration belum dijalankan" dan "wajib dijalankan sebelum dipakai" pada bagian di atas sudah tidak berlaku untuk DB dev tersebut; database lain (staging/production) tetap wajib menjalankannya sebelum fitur dipakai. Status task tetap ✅. Uji API di DB dev tetap `NOT RUN`.
