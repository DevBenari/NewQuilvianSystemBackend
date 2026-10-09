# Laporan Perubahan Backend — `RJ-DOC-REV-BE-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-014` |
| Judul | Simpan foto kartu penjamin perusahaan pasien |
| Slice | Amendment SK — scan kartu penjamin pada Pendaftaran Rawat Jalan |
| Roadmap | [`roadmap/doctor-consultation-roadmap.md`](../../../roadmap/doctor-consultation-roadmap.md) §15.1 |
| Trace | `RJ-DOC-DEC-040`..`044`, fakta `F-SK-2`, `F-SK-3` ([00-interview-decisions.md](../../../00-interview-decisions.md), *Amendment SK*) |
| Contract version | Delta pada endpoint `patient-company-guarantors` yang sudah ada (§4), dicatat di laporan ini |
| Dependency | `RJ-DOC-REV-BE-013` ✅ — memakai `PatientPayerCardImageService` yang sama |
| Klasifikasi | `MEDIUM` (skor 7: satu repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 2, database 2, keamanan 0, UI 0) |
| Task mode | `CROSS-REPO MODE` (`RJ-DOC-DEC-043`); task ini hanya menulis backend |
| Target tulis | Backend: model, configuration, DTO, controller penjamin perusahaan, service kartu, migration; izin migration `RJ-DOC-DEC-044` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b99fc41e` (perubahan belum di-commit) |
| Tanggal | 6 Oktober 2026 |
| Status | Selesai — build, EF, QBE, migration diterapkan ke `QuilvianNewDevSukma`, uji runtime `PASS` |

---

## 1. Masalah yang diperbaiki

Penjamin perusahaan pasien (`MstPatientCompanyGuarantor`) sama sekali tidak punya tempat untuk
gambar kartu. Kolom `GuaranteeDocumentPath` yang ada untuk surat jaminan, bukan kartu
(`F-SK-2`). Akibatnya, kartu pegawai/penjamin perusahaan yang dipindai di Pendaftaran Rawat Jalan
tidak bisa disimpan sama sekali.

---

## 2. Proses bisnis

Alurnya sama dengan kartu asuransi pada [`RJ-DOC-REV-BE-013`](RJ-DOC-REV-BE-013.md) §2:

1. Petugas memilih penjamin perusahaan pasien, lalu memindai kartunya.
2. Frontend mengirim gambar ke `PATCH /patient-company-guarantors/admin/{id}/card-image`.
3. Backend memvalidasi gambar, menyimpannya di `/uploads/patient-payer-cards/<id pasien>/company-<waktu>-<acak>.jpg`, lalu mengisi kolom baru `CardImagePath`.
4. Penjamin perusahaan baru dapat langsung membawa `cardImageBase64` saat dibuat.

**Aturan tambahan untuk penjamin perusahaan:** `PUT` tanpa `cardImagePath` **tidak** menghapus
gambar kartu yang sudah tersimpan. Form master data penjamin perusahaan yang lama belum mengenal
kolom ini. Tanpa aturan ini, setiap kali petugas menyunting data penjamin di master data, kartu
yang sudah dipindai akan terhapus diam-diam. Contoh: kartu tersimpan di `…/company-…949….jpg`, lalu
petugas mengubah catatan lewat `PUT`. Kartu tetap `…/company-…949….jpg` (C4).

Jalur tidak normal sama dengan `BE-013`: base64 rusak / bukan gambar / lebih dari 5 MB → `400`;
id tidak ada → `404`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientCompanyGuarantorController.cs`, `PatientCompanyGuarantorDtos.cs`,
`MstPatientCompanyGuarantor.cs`, `MstPatientCompanyGuarantorConfiguration.cs`, daftar migration dan
`ApplicationDbContextModelSnapshot.cs`, ditambah berkas `BE-013`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs` | Properti `CardImagePath` (`MaxLength 500`, nullable) |
| `Repositories/Configurations/HealthServices/MstPatientCompanyGuarantorConfiguration.cs` | Mapping `CardImagePath` `HasMaxLength(500)` |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientCompanyGuarantorDtos.cs` | `CardImagePath` pada response list/detail dan create; `CardImagePath` + `CardImageBase64` opsional pada request create/update |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientCompanyGuarantorController.cs` | Inject service; create/update menyimpan gambar; update mempertahankan kartu lama bila tidak dikirim; endpoint baru `PATCH {id}/card-image` dan `admin/{id}/card-image` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/PatientPayerCardImageService.cs` | Metode `UpdateCompanyGuarantorCardImageAsync` |
| `Migrations/20261006092040_AddCardImagePathToPatientCompanyGuarantor.cs` + `.Designer.cs` (baru) | `AddColumn CardImagePath character varying(500) null`; `Down` menghapus kolom |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | +4 baris properti `CardImagePath` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: field opsional `cardImageBase64`/`cardImagePath` pada request, `cardImagePath` pada response list/detail/create, endpoint baru `PATCH {id}/card-image` |
| Database | Satu kolom baru nullable `public."MstPatientCompanyGuarantor"."CardImagePath" varchar(500)`. Migration **diterapkan** ke `QuilvianNewDevSukma` (izin `RJ-DOC-DEC-044`) lewat script idempoten yang hanya memuat migration ini. Database lain belum |
| Keamanan/Auth | Endpoint baru: `[AccessAction("Update", ...)]` + `[AccessPermission("PatientCompanyGuarantor", "Update")]`; tanpa hardcode role. Risiko file `/uploads` publik sama dengan `BE-013` |

---

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Company Guarantor

Base path: `/api/v1/health-services/patient-management/master-data/patient-company-guarantors`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/`, `/admin`, `/{id}`, `/admin/{id}` | Response kini membawa `cardImagePath` | Tidak berubah |
| `POST` | `/`, `/kiosk`, `/admin` | `cardImageBase64` opsional; response create membawa `cardImagePath` | Tidak berubah |
| `PUT` | `/{id}`, `/admin/{id}` | `cardImageBase64` opsional mengganti kartu; tanpa `cardImagePath` kartu lama dipertahankan | `PatientCompanyGuarantor : Update` |
| `PATCH` | `/{id}/card-image`, `/admin/{id}/card-image` (**baru**) | Menyimpan atau mengganti gambar kartu hasil scan | `PatientCompanyGuarantor : Update` |

Body dan response `PATCH card-image` sama dengan `BE-013` §4. Pesan suksesnya:
"Gambar kartu patient company guarantor berhasil disimpan."

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release -p:UseSharedCompilation=false` | `0 Error(s)`, `239 Warning(s)`; nol warning pada berkas yang disentuh | `PASS` | Keluaran build |
| `dotnet ef migrations add AddCardImagePathToPatientCompanyGuarantor --configuration Release --no-build` | Migration hanya berisi `AddColumn CardImagePath`. Snapshot sempat ikut menukar urutan dua index `BilInvoice` (tanpa efek SQL); hunk itu dipulihkan, sehingga snapshot hanya +4 baris | `PASS` | Diff migration dan snapshot |
| `dotnet ef migrations list` terhadap `QuilvianNewDevSukma` | 19 migration modul lain berstatus *Pending*. Karena itu `database update` **tidak** dipakai (akan ikut menerapkan migration lain) | Catatan | Keluaran perintah |
| `dotnet ef migrations script 20261005090000_OptimizeRunningInvoiceQueries 20261006092040_AddCardImagePathToPatientCompanyGuarantor --idempotent`, dijalankan dengan `psql -v ON_ERROR_STOP=1` | `START TRANSACTION`, `DO`, `DO`, `COMMIT`; kolom ada (`character varying`, 500, nullable `YES`); `__EFMigrationsHistory` memuat `20261006092040_AddCardImagePathToPatientCompanyGuarantor`; 19 migration lain tetap *Pending* | `PASS` | Keluaran `psql` dan `information_schema.columns` |
| `dotnet ef migrations has-pending-model-changes --configuration Release --no-build` sesudah migration | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| QBE Strict × 9 berkas Amendment SK | `VIOLATION 0`, `REVIEW 0`, `PASS` | `PASS` | Keluaran checker |
| `AUTOMATED TEST` | `NOT APPLICABLE` — pola Bank Darah | `NOT RUN` | — |

### Uji runtime

Lingkungan, login, dan harness sama dengan [`RJ-DOC-REV-BE-013`](RJ-DOC-REV-BE-013.md) §5.

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| C1 | `POST` penjamin perusahaan dengan `cardImageBase64` | `200`, `cardImagePath` `/uploads/patient-payer-cards/<pasien>/company-….jpg` | `PASS` |
| C2 | Ambil file pada path publik | `200 image/jpeg` | `PASS` |
| C3 | `PATCH /admin/{id}/card-image` | `200`, path baru berbeda dari C1 | `PASS` |
| C4 | `PUT /admin/{id}` tanpa `cardImagePath` (hanya catatan berubah) | `200`; `cardImagePath` tetap = path C3 | `PASS` |
| C5 | `GET /patient-company-guarantors?patientId=` | `cardImagePath` = path C3 | `PASS` |
| C6 | Isi bukan gambar | `400` | `PASS` |
| C7 | Id tidak ada | `404` | `PASS` |

Hasil `BE-014`: **7/7 `PASS`**. Total Amendment SK backend: **21/21 `PASS`**.

**Keadaan sesudah run:** penjamin perusahaan uji (`UJI-SK-…-P`) dihapus lewat
`DELETE /admin/{id}` (`200`) dan tersisa sebagai baris soft-delete. Kolom `CardImagePath` tetap ada
di `QuilvianNewDevSukma`.

Uji manual: `NOT APPLICABLE` — layar diuji pada `RJ-DOC-REV-FE-014`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 1 (create dengan gambar → file + `cardImagePath`) untuk penjamin perusahaan | Terpenuhi | C1, C2 |
| AC 2 (`PATCH card-image` mengganti path) | Terpenuhi | C3, C5 |
| AC 3 (gambar tidak valid → `400`) | Terpenuhi | C6; validasi bersama `BE-013` R5–R8 |
| AC 4 (id tidak ada → `404`) | Terpenuhi | C7 |
| AC 5 (create tanpa gambar dan endpoint lain tidak berubah) | Terpenuhi | Diff aditif; `PUT` mempertahankan kartu (C4) |
| Migration hanya menambah kolom tersebut | Terpenuhi | Isi migration dan snapshot +4 baris |
| Build; migration ter-generate hanya kolom itu; runtime HTTP | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | File kartu lama tidak dihapus saat scan ulang (sama dengan `BE-013`) |
| Risiko tersisa | **Database lain belum dimigrasi.** Migration baru diterapkan ke `QuilvianNewDevSukma` saja; database lain (dev bersama, staging, produksi) butuh eksekusi terpisah. **Penerapan migration di luar urutan.** Migration ini diterapkan sementara 19 migration lebih lama masih *Pending*. EF menerapkan migration pending lain secara mandiri, dan tidak ada ketergantungan schema di antara keduanya. **Server dev pemilik.** Server di 7184 membaca model lama sampai di-restart, dan akan gagal start terhadap `QuilvianNewDevSukma` selama 19 migration itu belum diterapkan |
| Perubahan sampingan | Penukaran urutan index `BilInvoice` di snapshot hasil generator dipulihkan |
| Interupsi | `NONE` |
| Status Git | `M` controller asuransi & penjamin perusahaan, DTO asuransi & penjamin perusahaan, `MstPatientCompanyGuarantor.cs`, `MstPatientCompanyGuarantorConfiguration.cs`, `ApplicationDbContextModelSnapshot.cs`, `Program.cs`, `00-interview-decisions.md`, `doctor-consultation-roadmap.md`, `requirement-traceability.md`; `??` `PatientPayerCardImageDtos.cs`, `Services/`, dua berkas migration `20261006092040_*`, laporan `BE-013`/`BE-014`. `Storage/uploads/patient-photos/` dan `patient-qrcodes/00-00-00-18/` sudah ada sebelum task ini. Belum di-commit |
| Langkah berikutnya | `RJ-DOC-REV-FE-014` |
