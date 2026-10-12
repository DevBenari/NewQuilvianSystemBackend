# ISSUE-007 — Perbaikan UAT Admisi Rawat Inap: Waktu Rujukan dan Integrasi Kebijakan Deposit

```yaml
issue_id: ISSUE-EPS-007
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 1 Jenis Kunjungan & Langkah 5 Deposit (FE-INP-03, FE-INP-20)"
sumber_laporan: "Laporan UAT Admisi Rawat Inap 12 Oktober 2026: 3 butir temuan (UAT-ADM-001, UAT-ADM-002, UAT-ADM-003)"
tanggal_issue: "2026-10-12"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "9a108c6422e12e06080ca7dfccee0063a57dfa04 (MHamzah)"
source_sha_frontend: "3f8cb2facce438969ffa95a0cd172b5ebde7a333 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-007-perbaikan-uat-admisi-rawat-inap.md
```

---

## 1. Ringkasan

Berdasarkan hasil pengujian penerimaan pengguna (*User Acceptance Testing* / UAT) tanggal 12 Oktober 2026 pada alur pendaftaran pasien baru Admisi Rawat Inap, proses inti admisi pasien dinyatakan berhasil lulus (*PASS*). Namun, ditemukan 3 (tiga) butir perbaikan operasional yang perlu segera ditindaklanjuti. Butir paling kritis adalah **UAT-ADM-002** (*Major / P1*), di mana layar Deposit menampilkan pesan statis *"Minimum kebijakan belum dapat ditampilkan"* karena frontend belum diintegrasikan dengan API kebijakan deposit backend yang sebenarnya sudah tersedia di `GET /api/v1/health-services/billing-management/billing/patient-funds/deposit-policies`. 

Terkait hal tersebut, butir **UAT-ADM-003** menuntut penyiapan data master kebijakan deposit sebesar **Rp10.000.000** untuk pasien umum/tunai di database Development/UAT tanpa memengaruhi konfigurasi penjamin lain. Sementara butir **UAT-ADM-001** merupakan perbaikan kenyamanan antarmuka (*UI/UX Improvement*) pada Langkah 1 (Jenis Kunjungan Rujukan), yaitu menambahkan tombol **Sekarang** agar petugas admisi dapat mengisi tanggal dan jam aktual secara instan dengan satu kali klik tanpa menghilangkan keleluasaan pengisian manual bila surat rujukan terbit pada waktu lampau. Seluruh temuan ini selaras dengan aturan bisnis deposit rumah sakit: pembayaran di bawah batas minimum tidak membatalkan proses rawat inap, dan kekurangan deposit akan ditagihkan secara akurat pada Workspace PPRI (Pelunasan Deposit).

---

## 2. Laporan Asli Pelapor

| No. Laporan | Ringkasan Keluhan Pelapor | Lokasi & Konteks |
| :---: | :--- | :--- |
| **UAT-ADM-001** | Pada field Tanggal & Jam Rujukan belum tersedia tombol **Sekarang** untuk mengisi waktu aktual otomatis, tetapi tetap memungkinkan input manual. | Langkah 1 — Jenis Kunjungan (Formulir Rujukan Eksternal) |
| **UAT-ADM-002** | Sistem menampilkan pesan *"Minimum kebijakan belum dapat ditampilkan"*. Frontend masih menggunakan pesan statis dan belum terintegrasi dengan API kebijakan deposit backend. | Langkah 5 — Deposit / Uang Muka Pasien |
| **UAT-ADM-003** | Siapkan konfigurasi minimum deposit **Rp10.000.000** untuk skenario pasien umum/tunai pada database Development/UAT menggunakan seeder idempotent atau SQL tanpa mengganggu penjamin lain. | Backend — Master Data `MstDepositPolicy` |

---

## 3. Ringkasan Temuan

| ID Temuan | No. Laporan | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-EPS-007-01` | UAT-ADM-001 | Field Tanggal & Jam Rujukan belum memiliki tombol pintas "Sekarang" | `DESIGN_CHANGE` | Frontend | Medium | `SUDAH-VERIFIKASI` di source | `FIX-EPS-007-01` |
| `ISS-EPS-007-02` | UAT-ADM-002 | Layar deposit menampilkan alert statis dan belum memanggil API kebijakan deposit | `BUG` / `CONTRACT_MISMATCH` | Frontend | High | `SUDAH-VERIFIKASI` di source | `FIX-EPS-007-02` |
| `ISS-EPS-007-03` | UAT-ADM-003 | Tabel `MstDepositPolicy` belum memiliki data seeder default Rp10 Juta untuk pasien umum/tunai UAT | `DATA` | Backend | High | `SUDAH-VERIFIKASI` di source & DB | `FIX-EPS-007-03` |
| `ISS-EPS-007-T1` | — | Validasi keterhubungan kekurangan deposit ke Workspace PPRI (Pelunasan Deposit) | `DESIGN_CHANGE` | Integrasi FE/BE | Medium | `SUDAH-VERIFIKASI` di source | `FIX-EPS-007-04` |

---

## 4. Rincian Per Temuan

### ISS-EPS-007-01 — Field Tanggal & Jam Rujukan belum memiliki tombol pintas "Sekarang"

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | UAT-ADM-001 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source `inpatient-admission-visit-type-step.jsx` dan `inpatient-admission-visit-category-step.jsx` |
| **Perbaikan** | `FIX-EPS-007-01` |

**Apa yang terjadi.**  
Ketika petugas admisi melayani pasien rujukan dari klinik atau rumah sakit lain, petugas harus memilih tanggal dan jam rujukan secara manual melalui kalender dan pemilih waktu. Pada banyak kasus kedatangan darurat atau rujukan baru terbit, petugas membutuhkan satu tombol cepat **"Sekarang"** untuk langsung menetapkan waktu saat ini, tanpa harus menggulir menit dan jam secara manual satu per satu.

**Kenapa terjadi.**  
Pada berkas formulir rujukan langkah 1 [inpatient-admission-visit-type-step.jsx](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-visit-type-step.jsx#L157-L167) dan [inpatient-admission-visit-category-step.jsx](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-visit-category-step.jsx#L168-L177), komponen input dibungkus menggunakan `<BaseDateTimeField>` standar:

```jsx
// inpatient-admission-visit-type-step.jsx:157-166
<BaseDateTimeField
  id="referral-date-time"
  name="referralDateTime"
  label="Tanggal & Jam Rujukan"
  value={referralForm.referralDateTime}
  required
  error={fieldErrors.referralDateTime}
  description="Waktu surat rujukan diterbitkan oleh fasilitas kesehatan perujuk"
  onChange={(e) => handleFieldChange("referralDateTime", e.target.value)}
/>
```

Komponen `<BaseDateTimeField>` pada [base-form-control.jsx](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/features/base-features/base-form-control.jsx#L593-L709) merender `FilterDatePicker` dan `FilterTimePicker` bersisian, namun belum menyediakan tombol aksi pintas waktu aktual (*quick action button*).

**Apakah ini menyimpang dari desain?**  
`DESIGN_CHANGE`. Skema tampilan awal `FE-INP-03` pada dokumen blueprint `05-skema-tampilan.md` hanya menspesifikasikan input waktu standar `[ Tanggal & Jam Rujukan * ]`. Permintaan tombol "Sekarang" merupakan perbaikan ergonomi dan kecepatan pelayanan (*ergonomics enhancement*) yang diajukan pengguna saat UAT.

**Dampak nyata.**  
Petugas admisi membutuhkan waktu sekitar 10–15 detik lebih lama per pasien hanya untuk memilih tanggal hari ini dan mencocokkan jam pada pemilih waktu, padahal antrean admisi rawat inap menuntut kecepatan pendaftaran di bawah 3 menit.

**Rekomendasi.**  
Menyediakan aksi pintas waktu sekarang:
- **Opsi 1 (Direkomendasikan)**: Menambahkan tombol pintas `Sekarang` tepat di samping/bawah kontrol waktu rujukan yang saat diklik mengisi nilai `referralDateTime` dengan string waktu lokal terformat `YYYY-MM-DDTHH:mm`. Input kalender dan jam tetap aktif sehingga petugas tetap dapat mengoreksi tanggal jika rujukan terbit 2 hari sebelumnya.
- **Opsi 2**: Menambahkan kapabilitas prop `showNowButton` langsung pada base component `BaseDateTimeField`.

---

### ISS-EPS-007-02 — Layar deposit menampilkan alert statis dan belum memanggil API kebijakan deposit

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | UAT-ADM-002 |
| **Jenis** | `BUG` / `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | High (UAT FAIL — Major / P1) |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source `inpatient-admission-deposit-step.jsx` dan `use-inpatient-admission-deposit.jsx` |
| **Perbaikan** | `FIX-EPS-007-02` |

**Apa yang terjadi.**  
Ketika petugas admisi memasuki Langkah 5 (Deposit / Uang Muka) untuk pasien umum/tunai, layar selalu menampilkan kotak informasi biru dengan judul *"Minimum kebijakan belum dapat ditampilkan"* dan pesan *"Minimum deposit menurut penjamin dan kelas perawatan belum dapat dibaca..."*. Petugas dan keluarga pasien tidak mengetahui berapa batas minimal uang muka yang ditentukan oleh manajemen rumah sakit untuk kelas kamar yang dipilih.

**Kenapa terjadi.**  
Pada [inpatient-admission-deposit-step.jsx](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx#L40-L48), terdapat komentar legasi yang menyatakan bahwa endpoint kebijakan deposit belum ada di backend per September 2026:

```jsx
// inpatient-admission-deposit-step.jsx:40-48
{/* Minimum kebijakan dibaca `FE-RWI-059` lewat `GET /deposit-policies`, dan
    endpoint itu belum ada di backend. Layar mengaku tidak tahu minimumnya
    alih-alih menampilkan angka yang dikarang — nol angka minimum boleh
    ditulis di kode layar. */}
<InformationAlert
  variant="info"
  title={deposit?.transfer ? "Deposit saat admisi transfer" : INPATIENT_DEPOSIT_STEP_TEXT.policyPendingTitle}
  message={deposit?.transfer ? "Deposit tidak menahan admisi. Jika keluarga menyerahkan uang muka..." : INPATIENT_DEPOSIT_STEP_TEXT.policyPendingMessage}
/>
```

Padahal di backend, API kebijakan deposit **sudah tersedia dan aktif** pada [BillingPatientFundsController.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs#L93-L106):

```csharp
[HttpGet("deposit-policies")]
[AccessAction("Read", "Read Deposit Policy", AccessType = AccessTypes.Read, SortOrder = 4)]
[AccessPermission("BillingDeposit", "Read")]
[ProducesResponseType(typeof(ApiResponse<DepositPolicyResponse>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetDepositPolicy(
    [FromQuery] Guid? guarantorId,
    [FromQuery] Guid? patientClassId,
    CancellationToken cancellationToken)
```

Frontend belum memiliki service pemanggil untuk endpoint ini dan hook `useInpatientAdmissionDeposit.jsx` belum menerima parameter `patientClassId` serta `guarantorId` yang sebenarnya sudah dipilih petugas pada Langkah 4 (Pembayaran).

**Apakah ini menyimpang dari desain?**  
`BUG` dan `CONTRACT_MISMATCH`. PRD Rawat Inap (`04-prd-to-mvp.md`) dan spesifikasi integrasi billing secara eksplisit menetapkan bahwa angka minimum deposit harus diambil dari master kebijakan deposit berdasarkan kombinasi penjamin dan kelas rawat.

**Dampak nyata.**  
Keluarga pasien umum tidak mengetahui informasi kewajiban uang muka awal, sehingga berpotensi membayar terlalu kecil atau tidak membayar sama sekali, yang kemudian membebani penagihan piutang rumah sakit saat pasien dirawat.

**Rekomendasi.**  
1. Buat method `getDepositPolicy(guarantorId, patientClassId)` pada service frontend (misalnya di `inpatient-admission-payment.service.js` atau service deposit tersendiri) yang memanggil `GET /v1/health-services/billing-management/billing/patient-funds/deposit-policies`.
2. Teruskan `patientClassId` dan `guarantorId` dari controller pembayaran ke hook `useInpatientAdmissionDeposit`.
3. Tampilkan kartu/ringkasan kebijakan deposit pada `InpatientAdmissionDepositStep`:
   - Batas Minimum Kebijakan: `Rp 10.000.000`
   - Uang Muka Diterima: `Rp 9.000.000`
   - Status / Selisih: *"Kekurangan Rp 1.000.000 (Dapat dilunasi di Workspace PPRI)"*
4. Pertahankan aturan invariant `RWI-DEC-095`: Tombol **Lanjut** tidak boleh dinonaktifkan meski nominal deposit di bawah minimum atau bahkan kosong (Rp 0).

---

### ISS-EPS-007-03 — Tabel `MstDepositPolicy` belum memiliki data seeder default Rp10 Juta untuk pasien umum/tunai UAT

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | UAT-ADM-003 |
| **Jenis** | `DATA` |
| **Area** | Backend |
| **Keparahan** | High |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source `BillingDepositService.cs` dan ketiadaan seeder `MstDepositPolicy` |
| **Perbaikan** | `FIX-EPS-007-03` |

**Apa yang terjadi.**  
Ketika API `GET /deposit-policies` dipanggil dengan parameter pasien umum/tunai (`guarantorId = null`), backend mengembalikan objek dengan `MinimumAmount = 0` dan `IsRequired = false` karena tabel `MstDepositPolicy` pada database Development/UAT belum berisi baris kebijakan untuk pasien tunai.

**Kenapa terjadi.**  
Logika pembacaan kebijakan pada [BillingDepositService.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs#L63-L114) mencari data aktif di `MstDepositPolicies` secara bertingkat:
1. Cocok penjamin + kelas rawat
2. Cocok penjamin saja
3. Cocok kelas rawat saja
4. Kebijakan umum default (`GuarantorId == null && PatientClassId == null`)
5. Jika tidak ada baris yang cocok, mengembalikan nilai default `MinimumAmount = 0m`.

Di seluruh codebase backend saat ini, belum ada seeder yang mengisi tabel `MstDepositPolicy`.

**Apakah ini menyimpang dari desain?**  
`DATA`. Struktur tabel dan model `MstDepositPolicy` sudah lengkap dan benar, namun data master untuk lingkungan pengujian belum disiapkan.

**Dampak nyata.**  
Pengujian integrasi UAT-ADM-002 akan selalu membaca angka Rp 0 jika data master kebijakan belum diisi di database UAT.

**Rekomendasi.**  
Siapkan konfigurasi kebijakan deposit default untuk pasien umum/tunai dengan nilai **Rp 10.000.000**:
- Sediakan script SQL idempotent untuk dieksekusi langsung pada database Development/Staging UAT.
- Tambahkan seeder C# idempotent pada backend (dibatasi hanya untuk lingkungan non-produksi).

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa tombol "Sekarang" sangat dibutuhkan pada tanggal rujukan padahal sudah ada kalender?  
> **J:** Pada alur pelayanan rumah sakit, pasien rawat inap rujukan sering datang bersamaan dengan surat rujukan faskes perujuk yang baru saja diterbitkan hari ini. Tombol "Sekarang" memangkas proses memilih tanggal dan jam secara manual menjadi satu kali klik instan. Petugas tetap dapat mengubah waktu tersebut secara manual jika tanggal surat rujukan berbeda (misalnya rujukan terbit 1–2 hari sebelumnya).

> **T:** Jika pasien umum hanya membayar deposit Rp 9.000.000 (kurang Rp 1.000.000 dari ketentuan Rp 10.000.000), apakah pendaftaran rawat inap boleh dilanjutkan?  
> **J:** **Ya, wajib tetap boleh dilanjutkan.** Sesuai keputusan bisnis `RWI-DEC-095`, deposit tidak pernah menahan admisi pasien dan keselamatan medis pasien adalah prioritas utama. Tombol "Lanjut" tidak boleh dinonaktifkan. Kekurangan deposit sebesar Rp 1.000.000 akan otomatis tercatat dan dapat ditindaklanjuti oleh petugas admisi melalui Workspace PPRI pada tab **Pelunasan Deposit**.

> **T:** Apakah pembuatan seeder kebijakan deposit Rp 10.000.000 untuk pasien umum/tunai akan memengaruhi pasien dengan jaminan Asuransi atau BPJS?  
> **J:** **Tidak.** Kebijakan deposit umum dikonfigurasikan dengan `GuarantorId = NULL` dan `PatientClassId = NULL`. Sesuai aturan prioritas pencocokan pada `BillingDepositService`, jika penjamin tertentu (misalnya BPJS atau Asuransi Rekanan) memiliki kebijakan khusus, sistem akan memprioritaskan kebijakan penjamin tersebut terlebih dahulu. Selain itu, pada alur admisi rawat inap (`FE-RWI-102`), langkah deposit otomatis dilewati (*bypass*) untuk pasien dengan penjamin Asuransi/Perusahaan yang memiliki pertanggungan penuh.

---

## 6. Temuan Tambahan

### ISS-EPS-007-T1 — Validasi keterhubungan kekurangan deposit ke Workspace PPRI (Pelunasan Deposit)

| Parameter | Keterangan |
| :--- | :--- |
| **Kenapa Dicantumkan** | Muncul pada Bagian 2 & 3 Laporan UAT ("Alur pelunasan di Workspace PPRI perlu diuji setelah perbaikan diterapkan") |
| **Jenis** | `DESIGN_CHANGE` / Verifikasi End-to-End |
| **Area** | Integrasi Frontend & Backend |
| **Keparahan** | Medium |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source [deposit-settlement-form.jsx](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/admission-workspace/sections/documents/deposit/deposit-settlement-form.jsx#L31-L54) |
| **Perbaikan** | `FIX-EPS-007-04` |

**Keterangan.**  
Komponen Workspace PPRI pada modul Rawat Inap sudah memiliki fitur formulir Pelunasan Deposit (`FE-INP-41`, `FE-RWI-220`). Formulir ini mengambil data kekurangan dari endpoint billing `summary/amounts` dan `episodes/{episodeId}/amounts`. Setelah perbaikan kebijakan deposit Rp 10 Juta dan penerimaan deposit Rp 9 Juta diterapkan, perlu diverifikasi secara menyeluruh bahwa:
1. Episode rawat inap mencatat riwayat penerimaan uang muka Rp 9.000.000.
2. Workspace PPRI menampilkan nominal kekurangan Rp 1.000.000 secara akurat.
3. Petugas admisi dapat mencetak/menerbitkan Surat Pernyataan Kesediaan Melunasi Deposit dengan batas jatuh tempo sesuai kebijakan (`FollowUpIntervalDays = 3` hari kerja).

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor
Tidak ada pertanyaan pending untuk pelapor; deskripsi ketiga butir UAT sudah sangat jelas dan berbukti konkret di source code.

### Untuk Pemilik Sistem (*System Owner*)

| No. | Keputusan yang Dibutuhkan | Pilihan | Rekomendasi | Status Keputusan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Lokasi penempatan tombol "Sekarang" pada formulir rujukan | A. Tombol teks kecil di header label field<br>B. Tombol di samping input kalender/waktu | **Pilihan B**: Tombol pintas berukuran sedang bersisian dengan pemilih waktu agar ergonomis bagi petugas | **DISETUJUI** (12-10-2026, Pilihan B) |
| **K-02** | Nilai parameter `FollowUpIntervalDays` untuk kebijakan deposit umum | A. 3 hari kerja (default sistem)<br>B. 1 hari kerja (H+1 admisi)<br>C. Saat pasien pulang (*discharge*) | **Pilihan A**: 3 hari kerja, memberi waktu yang wajar bagi keluarga pasien untuk melunasi deposit ke kasir | **DISETUJUI** (12-10-2026, Pilihan A) |

---

## 8. Catatan Pola & Pencegahan

1. **Pola "Endpoint Dianggap Belum Ada":**  
   Frontend menulis catatan legasi bahwa endpoint backend belum ada, padahal backend telah menyelesaikannya pada sprint terdahulu. Pencegahan: selalu lakukan *cross-check* kontrak API ke Swagger atau `Controllers/` backend sebelum menampilkan teks statis darurat pada UI.
2. **Prinsip Invariant `RWI-DEC-095`:**  
   Penerapan pembacaan nominal minimum deposit **TIDAK BOLEH** mengubah sifat tombol "Lanjut". Tombol lanjut harus tetap dapat diklik meski nominal kosong atau di bawah batas minimum.
3. **Idempotensi Seeder Database:**  
   Penyiapan data kebijakan deposit UAT wajib menggunakan klausa `ON CONFLICT DO UPDATE` atau pengecekan `IF NOT EXISTS` berdasarkan kode unik `DEP-POL-UMUM` agar aman dijalankan berulang kali tanpa membuat duplikasi baris.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | :--- | :---: |
| 2026-10-12 | Dokumen issue dibuat berdasarkan Laporan UAT Admisi Rawat Inap (UAT-ADM-001 s/d UAT-ADM-003) | AI Agent (`diagnose-module-issue`) |
| 2026-10-12 | Rencana perbaikan disetujui pemilik sistem; status berubah menjadi DALAM_PERBAIKAN | Pemilik Sistem (User) |
