# Laporan Perubahan Backend — `BE-HRD-386`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-386` |
| Judul | Verifikasi backend Administrasi Kepegawaian: matriks akses, kontrak, build, dan regresi SK serta Perubahan Data |
| Slice | `S-FIN-ADM` / Verifikasi (alias `REG-IMP`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.1, kartu `BE-HRD-386` |
| Trace | Baseline `BE-HRD-024`, `BE-HRD-028`; matriks peran `02_ALUR_BISNIS_DAN_KEPUTUSAN.md` bagian 2 |
| Contract version | Kontrak Administrasi Kepegawaian (`docs/update/administrasi-kepegawaian`); endpoint baru dicatat sebagai delta |
| Dependency | `BE-HRD-384` ✅, `BE-HRD-385` ✅ |
| Klasifikasi | `LIGHT` — verifikasi |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | Kode slice `S-FIN-ADM`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a7c37c56` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | 🟡 Sebagian — matriks akses ditelusuri di source, build/QBE lulus, kontrak cocok dengan service frontend; uji per peran berhak/tidak berhak di runtime `NOT RUN` (butuh migration dijalankan dan wewenang uji DB dev) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `HrServiceManagement` |
| Keberlakuan | `NEW CODE` (Surat Tugas, registri) + `TOUCHED LEGACY` (Surat Keterangan) |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Verifikasi gerbang slice Administrasi Kepegawaian.

## 2. Proses bisnis

**Matriks akses aksi tulis (ditegakkan backend).**

| Aksi | Hak Akses Role | Penjaga tambahan di server |
| --- | --- | --- |
| Susun/ubah/ajukan Surat Tugas | `AssignmentLetter : Create/Update/Submit` | Peserta di unit yang dipimpin (kecuali `ManageAllUnits`); hanya pembuat |
| Verifikasi HRD | `AssignmentLetter : Verify` | Bukan pembuat/pengaju |
| Pengesahan | `AssignmentLetter : Endorse` | Bukan pembuat/pengaju/verifikator |
| Terbit | `AssignmentLetter : Issue` | Hanya dari Disahkan |
| Pelaksanaan/selesai | `AssignmentLetter : Report` | Pembuat atau peserta; sejak tanggal mulai |
| Batal | `AssignmentLetter : Cancel` | Pembuat/penerbit; sesudah terbit hanya sebelum mulai |
| Ajukan Surat Keterangan | `EmployeeDocumentIssuance : Request` | Diri sendiri/unit dipimpin (kecuali `AccessAllEmployees`) |
| C&B / terbit / cabut Surat Keterangan | `EmployeeDocumentIssuance : ApproveDraft/Issue/Revoke` | Anti self-approval; nama peran tertanam dihapus |
| SK Kepegawaian | `PersonnelActionOrder : …` (task `BE-HRD-407`–`412`) | Maker ≠ checker; kunci per SK |
| Perubahan Data | hak existing (`BE-HRD-392`) | `ApplyAsync` dikunci + baca ulang status |

**Kontrak frontend.** `hr-service.js` (`/v1/corporate/hrd/services/documents`: request, cb-approve, authorize-issue, queue, detail, download, revoke) cocok dengan controller. `official-duty-service.js` masih memakai endpoint dinas luar satu pegawai — endpoint `assignment-letters` dan `administrative-documents` adalah delta baru untuk frontend.

**Regresi.** SK tidak mengubah master sebelum tanggal efektif: worker menerapkan SK `Scheduled` hanya pada tanggal efektif WIB dengan klaim per SK (`BE-HRD-411`). Perubahan Data diterapkan sekali: kunci `HRD_PROFILE_CHANGE_APPLY_{id}` + baca ulang status (`BE-HRD-392`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Controller dan service `HrServiceManagement`, `HrdPersonnelActionOrderService`, `EmployeeProfileChangeService`, frontend `hr-service.js`, `official-duty-service.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source khusus task ini; verifikasi source |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Delta: `assignment-letters`, `administrative-documents` |
| Database | Migration `20261120093000` belum dijalankan |
| Keamanan/Auth | Penanda baru `AssignmentLetter : ManageAllUnits`, `EmployeeDocumentIssuance : AccessAllEmployees` |

## 4. Dokumentasi endpoint

Lihat laporan `BE-HRD-381` s.d. `BE-HRD-385`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error; himpunan warning (tanpa nomor baris) sama dengan sebelum perubahan — tambahan 6 instans berasal dari commit pengguna `a7c37c56` pada `HrdInitialPlacementBatchService.cs`, bukan dari perubahan ini | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (18 berkas slice Administrasi Kepegawaian) | 18 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Uji aksi tulis per peran berhak/tidak berhak | Belum dijalankan | `NOT RUN` | Butuh migration dijalankan + wewenang uji DB dev |
| Validator registri hak akses | `IsValid=True` (2.660 identitas, 0 endpoint tanpa izin) | `PASS` | skrip `dotnet fsi` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Setiap aksi tulis diuji dengan peran berhak dan tidak berhak | Belum | matriks ditelusuri di source; uji runtime `NOT RUN` |
| (2) Build dan QBE lulus; kontrak dicocokkan dengan service frontend | Terpenuhi | build 0 error, QBE 18 berkas PASS; pencocokan route di atas |
| (3) SK tidak mengubah master sebelum tanggal efektif; Perubahan Data diterapkan sekali | Terpenuhi | `BE-HRD-411`, `BE-HRD-392` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Alasan 🟡 | Kriteria (1) menuntut uji runtime per peran |
| Langkah berikutnya | 21 task legacy 🟡 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
