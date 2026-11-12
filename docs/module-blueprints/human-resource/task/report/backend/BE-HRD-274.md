# Laporan Penyelesaian Task: BE-HRD-274 Service Sidang Pemeriksaan Berita Acara Pemeriksaan (BAP) Disiplin & Pembelaan Staf

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-274`
* **Nama Task**: Service Sidang Pemeriksaan Berita Acara Pemeriksaan (BAP) Disiplin & Pembelaan Staf
* **Modul / Sub-Modul**: Human Resource / Sub-modul 18: Hubungan Industrial, Disiplin & Terminasi
* **Slice Delivery**: `S-C5` (Kontrak API `v22`, Gelombang `MVP-54`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `IDisciplinaryInvestigationService.cs`
  - `DisciplinaryInvestigationService.cs`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `EmployeeRelationManagement` (Sub-modul 18: Hubungan Industrial & Terminasi)
* **Prefix Registry**: `Hrd` / `Trx` / `Mst` (Kepatuhan Kamus Data Bagian 23)
* **Klasifikasi Kode**: `NEW CODE & EXTENSION`
* **Wewenang**: Implementasi modul backend sesuai kontrak terkunci `v22` dan aturan bisnis `VR-EMP-01` s.d. `VR-EMP-25`.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung (`dotnet ef database update`) ke database server development atau master tanpa otorisasi tertulis terpisah.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam tata kelola kepegawaian rumah sakit, kepatuhan hubungan industrial dan pemutusan ikatan kerja memiliki dampak hukum, finansial, dan keselamatan operasional yang kritis:
1. **Prinsip Just Culture RS**: Membedakan pelanggaran tata tertib kerja menjadi Human Error (edukasi/pembinaan Sub-16), At-Risk Behavior (peringatan terukur), dan Reckless Misconduct (sanksi berat/terminasi).
2. **Kepatuhan Regulasi Ketenagakerjaan**: Penegakan masa berlaku SP 1-3 tepat 6 bulan kalender sesuai Pasal 52 PP No. 35 Tahun 2021, jaminan upah penuh 100% pada masa skorsing investigasi sesuai Pasal 157A UU Ketenagakerjaan, dan formula pesangon deterministik.
3. **Pusat Kendali Exit Clearance Multi-Departemen**: Penegakan gembok keamanan (*Hard-Block*) Sub-10 Payroll (`INT-HR-87`), memastikan hak gaji dan pesangon terakhir tidak dapat dicairkan sebelum 5 departemen (Unit Kerja, Umum/Aset, Farmasi/Alkes, Keuangan, IT) memberikan persetujuan bebas tanggungan fisik dan finansial.

---

## 4. Rincian Implementasi & Alur Teknis
* **Ringkasan Pekerjaan**: Digitalisasi administrasi persidangan BAP, penugasan tim pemeriksa independen (>= 2 orang), pencatatan lembar pembelaan tertulis staf terperiksa, dan rekomendasi tindakan sanksi.
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 23, dan matriks validasi Sub-modul 18.
* **Kriteria Penerimaan (Acceptance Criteria)**: BAP tersimpan dengan nomor BAP-HRD-yyyy-xxx, investigator >= 2 orang tanpa konflik (VR-EMP-04), pembelaan staf terisi wajib sebelum pengesahan Direktur SDM (VR-EMP-05).

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS**.
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1p (`AT-HRD-EMP-01` s.d. `AT-HRD-EMP-22`).


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok E — nomor dokumen dari angka acak |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | ✅ **SELESAI 10 November 2026** (dikerjakan ulang) |

### Masalah yang ditemukan audit

Nomor BAP dibentuk `BAP-HRD-{yyyy}-{acak 3 digit}`; peluang nomor kembar dalam satu tahun tinggi (hanya 900 kemungkinan).

### Perubahan yang dikerjakan

- Nomor dialokasikan `NumberSeriesAllocator` (deret `HRD_ER_INVESTIGATION_BAP`, reset tahunan, 4 digit). Bentuk `BAP-HRD-2026-0001`.
- Aktor deret dari email petugas dengan cadangan aktor sistem.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/DisciplinaryInvestigationService.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nomor BAP unik dan berurutan per tahun | Terpenuhi | index unik `BapNumber` sudah ada |

### Database dan migration

`NOT APPLICABLE` — tidak ada perubahan skema untuk task ini.

### Catatan penutup

- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
