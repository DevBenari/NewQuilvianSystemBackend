# Laporan Perubahan Backend — `BE-RWI-105`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-105` |
| Judul | Template resep milik dokter login (R9) |
| Slice | Gelombang 2 — `DOK-V2-3` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-105` |
| Trace | `FR-DOK-088`, `FR-DOK-089`, `FR-DOK-090`, `FR-DOK-091`; **`RWI-DEC-152`**; `RWI-DEC-122`; `VAL-DOK-56`, `56a`, `56c`, `57`; arsitektur 0.5 bagian 13 (R9) |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-099` ✅ |
| Klasifikasi | `HEAVY` — **mengubah perilaku poliklinik**; kepemilikan data dokter dan keselamatan butir resep |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement`, dokumentasi blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai berdasarkan validasi statis source; `dotnet build` dan **regresi poliklinik NOT RUN** atas instruksi pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / PharmacyManagement` |
| Registry/prefix | `Mst` `LEGACY` (`MstPrescriptionTemplate`), `Phm` `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `PrescriptionTemplateController` masih memakai `ApplicationDbContext` langsung untuk baca; tulis dan hapus kini lewat service |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001` |
| Hak akses baru | Nol — tetap `PrescriptionTemplate : Read/Create/Update/Delete` |
| Persetujuan perubahan poliklinik | **Sukma GP**, pemilik blueprint `rawat-jalan`, lewat **`RWI-DEC-152`** (16 September 2026) |
| Database | **Nol migration** — R9 adalah perubahan kode (arsitektur 0.5 bagian 13: "R1, R2, R9 — kembalikan kode, nol perubahan data") |

## 1. Masalah yang diperbaiki

`OwnerDoctorId` template resep dikirim dari luar, sehingga dr. Ahmad dapat membuat atau mengubah
template atas nama dr. Rina, dan template itu muncul di ruang kerja dr. Rina. Pemakaian template juga
gagal seluruhnya bila satu obat bermasalah, dan template kosong dapat disimpan.

## 2. Proses bisnis

1. dr. Rina membuat template "Pneumonia dewasa" (Ceftriaxone, Paracetamol, Omeprazole). Pemilik diambil
   dari **akun login** lewat `InpatientClinicalContextService.ResolveActorDoctorIdAsync`; `OwnerDoctorId`
   pada request tidak lagi wajib.
2. dr. Ahmad mengirim `OwnerDoctorId = dr. Rina` → `403` "Template hanya dapat dibuat atau diubah atas
   nama Anda sendiri." — nol perubahan. Mengubah atau menghapus template dr. Rina → `403` "Template ini
   milik dokter lain." Akun tanpa tautan dokter (perawat, farmasi) → `403`. **Berlaku juga di poliklinik.**
3. Template tanpa satu pun obat → `400` "Template harus berisi sekurang-kurangnya satu obat."
4. Dari ruang kerja rawat inap, dr. Rina membuka daftar `GET /prescription-templates?ownerScope=Mine` →
   hanya template miliknya, **tanpa** template Bersama milik dokter lain. Template yang dibuat dengan
   `ServiceContext = Inpatient` (atau dari resep rawat inap) tersimpan pribadi.
5. dr. Rina memakai template pada draft resep Budi (alergi Paracetamol; Omeprazole kosong di master) →
   `POST /{id}/apply`:
   - Ceftriaxone masuk tanpa tanda.
   - Paracetamol **masuk dengan tanda** `AllergyConflict`.
   - Omeprazole **tidak masuk** dan dilaporkan bertanda `Unavailable`.
   - Jawaban `200` dengan `HasFlaggedItems = true` dan pesan "Template diterapkan ke draft resep. Periksa
     butir yang bertanda bentrok alergi atau tidak tersedia."
6. dr. Rina menyelesaikan catatan dokter tanpa menghapus Paracetamol → validasi penyelesaian menolak:
   "Masih ada obat yang bentrok alergi atau tidak tersedia: Paracetamol 500 mg. Hapus atau ganti sebelum
   menyimpan." (`INPATIENT_PRESCRIPTION_FLAGGED_ITEMS`).
7. dr. Ahmad memakai template dr. Rina pada resep rawat inap, atau perawat memakai template apa pun →
   `403` "Template ini tidak dapat dipakai dari ruang kerja rawat inap."
8. **Poliklinik:** pemakaian template pada resep tanpa `InpEpisodeId` tetap boleh memakai template Bersama;
   daftar tanpa `ownerScope` tidak berubah; aturan butir bertanda saat simpan tidak menyala.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PrescriptionTemplateService.cs`, `PrescriptionTemplateController.cs`, `PrescriptionTemplateDtos.cs`,
`MstPrescriptionTemplate*.cs`, `PrescriptionValidationService.cs`, `TrxPatientAllergy.cs`,
`InsuranceCoverageService.cs`, `InpatientClinicalContextService.cs`, validation matrix 10 (`VAL-DOK-56*`,
`VAL-DOK-57`), permission-audit-matrix rawat inap.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/PrescriptionSafetyFlagEvaluator.cs` | Baru — penanda `AllergyConflict` (alergi aktif pasien menurut `DrugId` atau nama alergen terhadap nama dagang/generik) dan `Unavailable` (obat tidak ada, nonaktif, atau tidak dapat diresepkan) |
| `Areas/HealthServices/PharmacyManagement/Services/PrescriptionTemplateService.cs` | Pemilik dari akun login pada buat, buat-dari-resep, ubah; hapus hanya pemilik; pemindahan pemilik ditolak; template kosong ditolak; pakai pada resep rawat inap hanya template milik dokter login; pemakaian per butir dengan penanda (butir yang ditolak penjaminan juga ditandai `Unavailable`; racikan tanpa satu pun bahan tersedia tidak dibentuk); `PrescriptionTemplateForbiddenException` |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionTemplateController.cs` | `ownerScope=Mine`; seluruh tulis dan hapus lewat service dengan `User`; `403` ditangkap; pesan pakai membedakan hasil bertanda |
| `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionTemplateDtos.cs` | `OwnerDoctorId` tidak wajib; `ServiceContext`; `ApplyPrescriptionTemplateItemResult`; `HasFlaggedItems` dan `Items` pada jawaban pakai |
| `Areas/HealthServices/PharmacyManagement/Services/PrescriptionValidationService.cs` | `ValidateInpatientSafetyFlagsAsync` — draft resep rawat inap bertanda ditolak saat disimpan (`VAL-DOK-57`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `OwnerDoctorId` diabaikan bila sama dengan dokter login dan ditolak `403` bila berbeda; query baru `ownerScope`; isian baru `ServiceContext`; jawaban pakai bertambah `HasFlaggedItems` dan `Items`. **Perilaku poliklinik berubah**: akun tanpa tautan dokter tidak lagi dapat membuat template atas nama dokter, dan pemakaian template tidak lagi gagal seluruhnya karena satu butir |
| Database | Nol |
| Keamanan/Auth | Nol hak akses baru; kepemilikan dari data akun, bukan dari request. Menutup celah pembuatan template atas nama dokter lain |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Prescription Template

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | (diperluas) `ownerScope=Mine` hanya template milik dokter login; akun tanpa tautan dokter `403` | `PrescriptionTemplate : Read` |
| `POST` | `/` | (diubah) pemilik = dokter login; template kosong `400` | `PrescriptionTemplate : Create` |
| `POST` | `/from-prescription` | (diubah) pemilik = dokter login; dari resep rawat inap tersimpan pribadi | `PrescriptionTemplate : Create` |
| `PUT` | `/{id}` | (diubah) hanya pemilik; pemindahan pemilik ditolak | `PrescriptionTemplate : Update` |
| `POST` | `/{id}/apply` | (diubah) penanda per butir; resep rawat inap hanya template milik dokter login | `PrescriptionTemplate : Create` |
| `DELETE` | `/{id}` | (diubah) hanya pemilik | `PrescriptionTemplate : Delete` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–5 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| Seluruh pemanggil service template memakai tanda tangan baru | Hanya controller template; seluruh pemanggilan meneruskan `User` | `PASS` | Pencarian source |
| Aturan `VAL-DOK-57` hanya menyala pada draft rawat inap | Keluar lebih awal bila `InpEpisodeId` kosong atau status bukan `Draft` | `PASS` | `ValidateInpatientSafetyFlagsAsync` |
| Pemeriksaan statis ambiguitas tipe dan `using` hilang | Nol temuan pada kode task | `PASS` | Skrip analisis statis sesi ini |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik |
| Verifikasi proses bisnis runtime | Tidak dijalankan | `NOT RUN` | Menunggu build |
| **Regresi poliklinik** — alur template rawat jalan untuk pemiliknya sendiri | Tidak dijalankan | `NOT RUN` | Menunggu build — **wajib dijalankan sungguhan dan hasilnya ditempel sebelum rilis** (kartu roadmap, `RWI-DEC-152`) |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Template dibuat, diubah, dihapus hanya atas nama dokter login, termasuk poliklinik — `FR-DOK-088` | Terpenuhi (statis) | `RequireActorDoctorAsync` pada buat/buat-dari-resep/ubah; `DeleteAsync` memeriksa pemilik |
| 2. Ruang kerja rawat inap hanya menampilkan dan memakai template milik dokter login; perawat tidak memakai template — `FR-DOK-089` | Terpenuhi (statis) | `ownerScope=Mine`; `ApplyAsync` pada resep ber-`InpEpisodeId` → `403` bila bukan pemilik atau bukan dokter |
| 3. Template kosong ditolak — `FR-DOK-091` | Terpenuhi (statis) | `EnsureNotEmpty` → `400` |
| 4. Pemakaian menandai butir bentrok alergi atau tidak tersedia tanpa menggagalkan butir lain — `FR-DOK-090` | Terpenuhi (statis) | `PrescriptionSafetyFlagEvaluator`; butir alergi masuk bertanda, butir tidak tersedia dilewati dan dilaporkan |
| 5. Draft yang masih membawa butir bertanda ditolak saat disimpan | Terpenuhi (statis) | `PrescriptionValidationService.ValidateInpatientSafetyFlagsAsync` |
| 6. Regresi poliklinik hijau | **Belum** | `NOT RUN` atas instruksi pemilik; ditandai selesai atas instruksi pemilik |
| Laporan tracked merujuk `RWI-DEC-152` | Terpenuhi | Metadata, preflight |
| Roadmap dan traceability diperbarui | Terpenuhi | Kartu `BE-RWI-105`, baris `FR-DOK-088` s.d. `091` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Perilaku poliklinik berubah** (disetujui `RWI-DEC-152`): pengguna non-dokter yang selama ini membuat template atas nama dokter kini ditolak `403`, dan pemakaian template tidak lagi gagal seluruhnya. Task ditandai ✅ atas instruksi pemilik walaupun Definition of Done mensyaratkan regresi poliklinik hijau — regresi tetap wajib sebelum rilis |
| Masalah yang diketahui | Kamus data 0.5 tidak menyimpan penanda per butir, sehingga penanda dihitung ulang saat simpan. Akibatnya `VAL-DOK-57` berlaku pada **seluruh** butir draft rawat inap, termasuk butir bentrok alergi yang diketik manual — bukan hanya butir dari template. Pencocokan alergi berbasis `DrugId` atau nama, belum berbasis kelas obat |
| Risiko tersisa | Template poliklinik lama yang dibuat atas nama dokter lain tetap ada dan kini hanya dapat diubah pemilik tercatatnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` (`PrescriptionSafetyFlagEvaluator.cs`); berkas lain `M` |
| Langkah berikutnya | `dotnet build`; regresi alur template poliklinik oleh pemiliknya; uji skenario Budi (alergi + obat kosong) dan penolakan saat simpan |
