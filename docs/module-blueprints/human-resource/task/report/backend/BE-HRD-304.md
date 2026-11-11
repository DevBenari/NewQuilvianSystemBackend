# Laporan Penyelesaian Task: BE-HRD-304 Instansiasi Tiket Workflow Permohonan ESS Mandiri & Validasi Entitas Asal (WorkflowInstanceCreationService - VR-WF-01)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-304`
* **Nama Task**: Instansiasi Tiket Workflow Permohonan ESS Mandiri & Validasi Entitas Asal (WorkflowInstanceCreationService - VR-WF-01)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 21: Workflow Management & Unified Approval Inbox V2 Hardening
* **Slice Delivery**: `S-A7-V2` (Kontrak API `v24`, Gelombang `MVP-60`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `Implementasi WorkflowInstanceCreationService untuk pendaftaran tiket persetujuan mandiri dari modul ESS. Melakukan validasi eksistensi dan integritas entitas transaksi asal (cuti, lembur, tukar shift, restitusi) sesuai aturan VR-WF-01, menerbitkan nomor tiket alur kerja unik, serta menginisiasi tahapan step 1 dan SLA.`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `WorkflowManagement` (Sub-modul 21: ESS & Unified Approval Inbox V2)
* **Prefix Registry**: `Hrd` / `Trx` / `Wfp` (Kepatuhan Kamus Data Bagian 24 & Konstitusi Repositori)
* **Klasifikasi Kode**: `NEW CODE & EXTENSION`
* **Wewenang**: Implementasi backend sesuai kontrak terkunci `v24` dan aturan bisnis `VR-WF-01` s.d. `VR-WF-25`.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung (`dotnet ef database update`) ke database server development atau master tanpa otorisasi tertulis terpisah.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam ekosistem rumah sakit modern, koordinasi perizinan kerja, pergeseran shift jaga nakes, klaim biaya, dan persetujuan operasional memiliki dampak langsung terhadap keselamatan pasien dan keberlanjutan layanan medis:
1. **Pencegahan Gangguan Operasional Layanan (WF-B1, VR-WF-08)**: Penentuan rantai persetujuan berjenjang secara dinamis multi-dimensi berdasarkan unit kerja pemohon, tingkat jabatan, dan ambang batas finansial untuk memastikan pengawasan manajerial yang tepat sasaran tanpa birokrasi manual.
2. **Integritas Tata Kelola & Anti Self-Approval Mutlak (WF-B2, HRD-DEC-183, VR-WF-02)**: Larangan mutlak bagi staf/pejabat struktural menyetujui permohonannya sendiri. Sistem secara otomatis mengalihkan tugas persetujuan (skip-level routing) ke atasan struktural di atasnya untuk menjaga independensi dan akuntabilitas.
3. **Fleksibilitas Operasional Plh Berbatas Waktu (WF-B3, HRD-DEC-184, VR-WF-03, VR-WF-09)**: Pendelegasian wewenang persetujuan kepada pejabat pengganti (Plh) berbatas waktu saat pejabat definitif berhalangan/cuti, dengan penandaan visual badge *Delegated* dan pencatatan jejak audit ganda (*dual audit logging*).
4. **Penegakan Disiplin SLA & Larangan Auto-Approve Mutlak (WF-B4, HRD-DEC-185, VR-WF-04)**: Rumah sakit memberlakukan kebijakan **Zero Auto-Approval Policy**. Permohonan yang habis batas waktunya dilarang keras disetujui otomatis oleh sistem karena berisiko fatal bagi keselamatan jadwal bangsal; sistem menandai tiket overdue dan mengeskalasikannya ke atasan lebih tinggi.
5. **Keputusan Tri-State & Perlindungan Hak Staf (WF-B5, HRD-DEC-186, VR-WF-05)**: Opsi otorisasi tri-state (*Approve*, *Reject* wajib alasan bermakna minimal 15 karakter, dan *Return for Revision* untuk perbaikan draf data dalam 7 hari tanpa membatalkan tiket).
6. **Persetujuan Massal Bersyarat Aman (WF-B6, HRD-DEC-187, VR-WF-07)**: Kemudahan bulk approval hanya diizinkan untuk permohonan berisiko rendah yang telah memenuhi **100% indikator validasi HIJAU** serta diverifikasi dengan otorisasi PIN keamanan.
7. **Sinkronisasi Transaksional Idempoten Domain Hilir (WF-B8, INT-HR-93, INT-HR-94)**: Penyelesaian tiket secara atomik dan idempoten memicu eksekusi pemotongan saldo cuti di Sub-08, penguncian roster dinas di Sub-06, pencatatan lembur di Sub-09, dan pencairan klaim di Sub-11.

---

## 4. Rincian Implementasi & Alur Teknis
* **Ringkasan Pekerjaan**: Implementasi WorkflowInstanceCreationService untuk pendaftaran tiket persetujuan mandiri dari modul ESS. Melakukan validasi eksistensi dan integritas entitas transaksi asal (cuti, lembur, tukar shift, restitusi) sesuai aturan VR-WF-01, menerbitkan nomor tiket alur kerja unik, serta menginisiasi tahapan step 1 dan SLA.
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 24, keputusan bisnis HRD-DEC-182 s.d. HRD-DEC-187, asumsi HRD-ASM-67 s.d. HRD-ASM-70, dan matriks validasi Sub-modul 21.
* **Kriteria Penerimaan (Acceptance Criteria)**: Seluruh method service, model, DTO, background worker, dan controller terimplementasi rapi, terisolasi dalam transaksi database ACID, dan lolos uji kompilasi serta conformance check.

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS** (0 Violation).
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1r (`AT-HRD-WF-01`).


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

Nomor permohonan workflow `WF-{TIPE}-{yyyyMMdd}-{acak 4 digit}` memakai `new Random()`.

### Perubahan yang dikerjakan

- Deret `HRD_WF_INSTANCE`, reset harian, 4 digit. Karena awalan provider maksimal 15 karakter sedangkan nama tipe bisa panjang, deret dialokasikan dengan awalan `WF` lalu nama tipe disisipkan: `WF-LEAVEREQUEST-20261110-0001`. Aktor deret = `currentUserId`.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowInstanceCreationService.cs`
- `Areas/Corporate/HumanResource/WorkflowManagement/Constants/WorkflowNumberSeries.cs` (baru)

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
| Nomor permohonan workflow unik dan berurutan | Terpenuhi | satu deret harian untuk seluruh tipe permohonan |

### Database dan migration

`NOT APPLICABLE` — tidak ada perubahan skema untuk task ini.

### Catatan penutup

- `TrxWorkflowInstance.WorkflowNumber` belum punya index unik di konfigurasi legacy; tidak ditambahkan pada task ini karena tabel itu menampung nomor lama bergaya acak yang berpotensi kembar — penambahan index unik perlu pembersihan data lebih dulu (temuan untuk pemilik).
- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
