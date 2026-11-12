# Laporan Penyelesaian Backend Task: BE-HRD-188

## 1. Identitas Task
* **ID Task**: `BE-HRD-188`
* **Nama Task**: Layanan pengajuan klaim koordinasi manfaat (COB) di portal ESS ber-SEP mandatory BPJS Kesehatan 19 karakter (`TrxBenefitClaim`, `BEN-B5`, `VR-BEN-16`..`18`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-38: Integrasi SIMRS Kasir, Buku Besar Plafon, Klaim COB & Kliring Exit Clearance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Pegawai rumah sakit atau anggota keluarganya yang terdaftar sebagai tanggungan sah seringkali menjalani perawatan darurat di fasilitas kesehatan lain (misalnya saat dinas luar kota, liburan, atau kondisi darurat yang jauh dari RS Quilvian). Sesuai dengan skema *Coordination of Benefit* (COB) yang diatur oleh BPJS Kesehatan, penjamin tingkat pertama (*primary payer*) adalah BPJS Kesehatan, sedangkan fasilitas asuransi internal atau komersial rumah sakit bertindak sebagai penjamin sekunder (*secondary payer*).

Fitur-fitur utama yang dibangun dalam task ini:
1. **Layanan Pengajuan Klaim COB Mandiri Pegawai (Portal ESS)**:
   - Pegawai dapat mengajukan klaim penggantian biaya (*reimbursement*) atas perawatan medis yang dijalani sendiri maupun oleh tanggungannya (`TrxBenefitClaim`).
2. **Kewajiban Nomor SEP BPJS Kesehatan 19 Digit Alfanumerik (`BEN-B5`, `VR-BEN-16`..`17`)**:
   - Untuk jenis layanan **Rawat Inap** (*Inpatient Care*), pengajuan klaim COB **wajib** menyertakan nomor Surat Eligibilitas Peserta (SEP) BPJS Kesehatan yang valid terdiri dari tepat 19 karakter alfanumerik (contoh: `0123R0010123V000001`).
   - Rawat Jalan (*Outpatient Care*) dapat bersifat opsional atau menyertakan rujukan faskes 1 jika disyaratkan.
3. **Validasi Neraca Tagihan Medis Kuitansi Rumah Sakit (`VR-BEN-18`)**:
   - Total tagihan kuitansi rumah sakit luar wajib memiliki keseimbangan matematis mutlak:
     $$\text{TotalHospitalBill} = \text{PrimaryPayerCoveredAmount} + \text{SecondaryClaimAmount} + \text{OutOfPocketExcessAmount}$$
   - Mencegah manipulasi angka ganda atau klaim palsu (*double-claiming*).
4. **Layanan Unggah Dokumen Bukti Perawatan (`BEN-B5`, `FR-BEN-18`)**:
   - Endpoint untuk mengunggah berkas kuitansi asli, resume medis, lembar SEP, dan rincian obat dalam format PDF/JPG/PNG dengan batas ukuran maksimal 5MB.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Model Basis Data Entity Framework Core
File: `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxBenefitClaim.cs`
* Properti ditambahkan/diperkaya:
  - `SepBpjsNumber`: Nomor SEP BPJS 19 digit (`VR-BEN-16`, `VR-BEN-17`).
  - `ServiceType`: Rawat Inap (`Inpatient`) atau Rawat Jalan (`Outpatient`).
  - `HospitalName`: Nama rumah sakit tempat perawatan dilakukan.
  - `TotalHospitalBill`: Total nominal tagihan kuitansi rumah sakit.
  - `PrimaryPayerCoveredAmount`: Jumlah yang ditanggung oleh BPJS Kesehatan.
  - `SecondaryClaimAmount`: Jumlah klaim yang diajukan ke tunjangan/asuransi internal RS.
  - `OutOfPocketExcessAmount`: Sisa biaya ekses yang dibayar mandiri oleh pegawai.
  - `ReceiptAttachmentPath`: Lokasi berkas bukti dokumen pendukung.

### B. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitInHouseAndClaimDtos.cs`
* `SubmitBenefitClaimRequestDto`: Payload pengajuan klaim COB dengan validasi atribut `[Required]`, `[StringLength]`, `[RegularExpression]`.
* `BenefitClaimResponseDto`: Format keluaran detail status klaim beserta breakdown keuangan.

### C. Logika Bisnis & Validasi Layanan
File: `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitClaimService.cs`
* Metode: `SubmitClaimAsync` & `UploadClaimReceiptAsync`.
* Validasi aturan bisnis `BEN-B5`, `VR-BEN-16` (SEP 19 karakter), `VR-BEN-18` (penjumlahan rincian biaya tepat sama dengan total tagihan).

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario Rawat Inap Darurat di Luar Kota**:
  - Dokter Budi mengalami demam berdarah saat menghadiri simposium kedokteran di Surabaya dan dirawat inap di RS Siloam Surabaya selama 4 hari.
  - Total tagihan rumah sakit: Rp 12.000.000.
  - BPJS Kesehatan (penjamin primer dengan SEP `0301R0011124V000123`) menanggung Rp 8.500.000 (tarif INA-CBG kelas 1).
  - Dokter Budi naik kelas ke VIP sehingga terdapat sisa tagihan Rp 3.500.000.
  - Dokter Budi mengajukan klaim COB melalui ESS RS Quilvian:
    * `ServiceType`: `Inpatient`
    * `SepBpjsNumber`: `0301R0011124V000123`
    * `TotalHospitalBill`: 12.000.000
    * `PrimaryPayerCoveredAmount`: 8.500.000
    * `SecondaryClaimAmount`: 3.000.000 (sesuai pagu plafon rawat inap eksternal grade Budi)
    * `OutOfPocketExcessAmount`: 500.000 (biaya selisih yang dibayar pribadi)
  - Sistem memvalidasi matematika $8.500.000 + 3.000.000 + 500.000 = 12.000.000$, menyimpan pengajuan berstatus `PendingVerification`, dan Dokter Budi mengunggah berkas kuitansi dan SEP resmi.

---

## 5. Spesifikasi Endpoint REST API
Grup Swagger Tag: `[Tags("HR Benefits - COB Claims & Settlement")]`

| Method | Path | Deskripsi | Otorisasi |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/hr/benefits/claims` | Mengajukan klaim koordinasi manfaat (COB) baru | Staf Rumah Sakit / ESS |
| `POST` | `/api/v1/hr/benefits/claims/{claimId}/receipt` | Mengunggah dokumen kuitansi & SEP (maks 5MB) | Staf Rumah Sakit / ESS |
| `GET` | `/api/v1/hr/benefits/claims/{claimId}` | Mengambil detail status klaim beserta breakdown | Staf Rumah Sakit / HR Admin |
| `GET` | `/api/v1/hr/benefits/claims/my-claims` | Riwayat klaim COB staf yang sedang login | Staf Rumah Sakit / ESS |

---

## 6. Verifikasi & Integritas Bukti
* **Kompilasi C#**: Bebas error dan peringatan tipe data.
* **Kepatuhan Audit**: Sesuai dengan aturan pelaporan klaim sekunder rumah sakit dan terhubung langsung ke log audit `[AUDIT-BEN-08]`.

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

Nomor klaim COB `CLM-COB-{yyyyMMdd}-{acak 4 digit}` memakai `new Random()`.

### Perubahan yang dikerjakan

- Deret `HRD_BENEFIT_COB_CLAIM`, reset harian, 4 digit; bentuk `CLM-COB-20261110-0001`. Aktor deret = `actorUserId` pemohon (cadangan aktor sistem).
- Definisi deret Benefit dikumpulkan di `BenefitManagement/Constants/BenefitNumberSeries.cs`.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitClaimService.cs`
- `Areas/Corporate/HumanResource/BenefitManagement/Constants/BenefitNumberSeries.cs` (baru)

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
| Nomor klaim COB unik dan berurutan | Terpenuhi | index unik berfilter `ClaimNumber` sudah ada |

### Database dan migration

`NOT APPLICABLE` — tidak ada perubahan skema untuk task ini.

### Catatan penutup

- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
