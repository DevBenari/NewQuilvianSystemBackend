# Laporan Perubahan Backend — `RJ-DOC-REV-BE-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-013` |
| Judul | Simpan foto kartu asuransi pasien |
| Slice | Amendment SK — scan kartu penjamin pada Pendaftaran Rawat Jalan |
| Roadmap | [`roadmap/doctor-consultation-roadmap.md`](../../../roadmap/doctor-consultation-roadmap.md) §15.1 |
| Trace | `RJ-DOC-DEC-040`..`044`, fakta `F-SK-1`..`F-SK-4` ([00-interview-decisions.md](../../../00-interview-decisions.md), *Amendment SK*) |
| Contract version | Delta pada endpoint `patient-insurances` yang sudah ada (§4). Belum ada kontrak bertversi terpisah; delta dicatat di laporan ini |
| Dependency | — |
| Klasifikasi | Amendment SK dinilai `EPIC` (skor 13: dua repository 2, berkas diperiksa 2, berkas diubah 2, logika 1, kontrak API 2, database 2, keamanan 1, UI 1), lalu dipecah menjadi `BE-013`, `BE-014`, `FE-014`. `BE-013` sendiri `MEDIUM` (skor 6) |
| Task mode | `CROSS-REPO MODE` (`RJ-DOC-DEC-043`); task ini hanya menulis backend |
| Target tulis | Backend: `Areas/HealthServices/PatientManagement/MasterData/**`, `Program.cs`, laporan dan tanda status Amendment SK |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b99fc41e` (perubahan belum di-commit) |
| Tanggal | 6 Oktober 2026 |
| Status | Selesai — build, EF, QBE, dan uji runtime `PASS` |

---

## 1. Masalah yang diperbaiki

Kolom `MstPatientInsurance.CardImagePath` sudah ada sejak lama, tetapi hanya bisa diisi teks path.
Tidak ada cara mengirim gambar kartu hasil scan ke backend. Akibatnya, gambar kartu asuransi yang
dipindai petugas di Pendaftaran Rawat Jalan hanya hidup selama halaman terbuka. Begitu halaman
dimuat ulang, kartu harus dipindai lagi.

Foto scan identitas di kiosk tidak punya masalah ini. Kiosk mengirim gambar sebagai base64, lalu
backend menyimpannya sebagai file dan mencatat path publiknya pada data pasien (`F-SK-3`). Task
ini menerapkan pola yang sama untuk kartu asuransi.

---

## 2. Proses bisnis

**Pelaku:** petugas pendaftaran Rawat Jalan.

**Jalur normal — asuransi yang sudah tersimpan:**

1. Petugas memilih asuransi pasien pada tabel penjamin, lalu memindai kartunya dengan scanner Plustek.
2. Frontend mengirim gambar hasil scan ke `PATCH /patient-insurances/admin/{id}/card-image`.
3. Backend memeriksa gambar: harus base64 yang benar, harus gambar JPG/PNG yang bisa dibaca, dan
   maksimal 5 MB.
4. Backend menyimpan gambar sebagai JPEG di
   `/uploads/patient-payer-cards/<id pasien>/insurance-<waktu>-<acak>.jpg`, lalu mencatat path
   itu pada `CardImagePath` asuransi tersebut.
5. Pada kunjungan berikutnya, daftar asuransi pasien sudah membawa `cardImagePath`, sehingga
   kartu cukup ditampilkan tanpa scan ulang.

**Jalur normal — asuransi baru:** petugas mendaftarkan asuransi baru beserta hasil scan kartunya.
`POST /patient-insurances` menerima `cardImageBase64`, menyimpan file, dan mengisi `cardImagePath`
pada data yang baru dibuat.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Base64 rusak, misalnya `@@bukan-base64@@` | `400` "Format base64 gambar kartu tidak valid." |
| Base64 benar tetapi isinya bukan gambar | `400` "Gambar kartu tidak dapat dibaca. Pastikan hasil scan berupa gambar JPG atau PNG." |
| Gambar lebih dari 5 MB (contoh: 5 MB + 10 byte) | `400` "Ukuran gambar kartu melebihi batas 5 MB." Batas bisa diubah lewat `FileStorage:MaxPayerCardImageSizeMb` |
| Id asuransi tidak ada atau sudah dihapus | `404` "Patient insurance tidak ditemukan." |
| Create dengan gambar rusak | `400`; data asuransi **tidak** dibuat |
| Penyimpanan ke database gagal setelah file ditulis | File yang baru ditulis dihapus lagi supaya tidak tertinggal |

Scan ulang menghasilkan file baru dan path baru. File lama tidak dihapus (lihat §7).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` backend; aturan `rules/backend/` (TASK_RULES, TASK_CLASSIFICATION,
CROSS_REPO_RULES, API_RULES, DATABASE_RULES, REVIEW_RULES, REPORT_TEMPLATE, role-access-rules,
`engineering/BACKEND_ENGINEERING_CONTRACT.md`, `engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`);
`PatientInsuranceController.cs`, `PatientInsuranceDtos.cs`, `MstPatientInsurance.cs`,
`MstPatientInsuranceConfiguration.cs`; `PatientController.cs` (pola foto kiosk:
`SavePatientPhotoFileFromRequest`, `GetFileStoragePaths`); `Program.cs` (static files `/uploads`,
registrasi service); `WfpCertificationFileStorageService.cs` (pola service file); di frontend sebagai
rujukan konsumen: `emergency-registration.service.js`, `kiosk-new-patient-submit.helpers.jsx`,
`plustek-scanner-agent.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Services/PatientPayerCardImageService.cs` (baru) | Service yang memvalidasi base64, membaca gambar dengan ImageSharp, menyimpan JPEG kualitas 90 di storage `FileStorage`, dan mengganti `CardImagePath` asuransi. Resolusi folder storage sama dengan `UseStaticFiles` di `Program.cs`, sehingga file langsung tersaji di `/uploads` |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientPayerCardImageDtos.cs` (baru) | `UpdatePatientPayerCardImageRequest` (`cardImageBase64` wajib) dan `PatientPayerCardImageResponse` (`id`, `patientId`, `cardImagePath`) |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientInsuranceDtos.cs` | `CardImageBase64` opsional pada `CreatePatientInsuranceRequest` (ikut terwariskan ke update); `CardImagePath` pada `PatientInsuranceCreateResponse` |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientInsuranceController.cs` | Inject service; create dan update menyimpan gambar bila `cardImageBase64` diisi, dan menghapus file bila transaksi gagal; endpoint baru `PATCH {id}/card-image` dan `admin/{id}/card-image`; `cardImagePath` pada response create |
| `Program.cs` | `AddScoped<PatientPayerCardImageService>()` beserta `using` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Hanya aditif: field opsional `cardImageBase64` pada create/update, field `cardImagePath` pada response create, dan endpoint baru `PATCH {id}/card-image`. Pemanggil lama yang tidak mengirim `cardImageBase64` berperilaku sama (R11) |
| Database | Tidak ada perubahan schema; memakai kolom yang sudah ada `MstPatientInsurance.CardImagePath`. `dotnet ef migrations has-pending-model-changes` → tanpa perubahan |
| Keamanan/Auth | Endpoint baru memakai `[AccessAction("Update", ...)]` + `[AccessPermission("PatientInsurance", "Update")]`, sama dengan `PUT` dan `PATCH status`; tanpa hardcode role. File kartu tersaji tanpa login di `/uploads`, sama seperti foto pasien kiosk (lihat §7) |

---

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Insurance

Base path: `/api/v1/health-services/patient-management/master-data/patient-insurances`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` dan `/kiosk` | Membuat asuransi pasien. Field baru opsional `cardImageBase64`; bila diisi, `cardImagePath` terisi path file tersimpan. Response kini membawa `cardImagePath` | Tidak berubah: policy `KioskRead` (`[AccessAction]` Create) |
| `POST` | `/admin` | Sama dengan di atas untuk petugas | `PatientInsurance : Create` |
| `PUT` | `/{id}` dan `/admin/{id}` | Mengubah asuransi. `cardImageBase64` opsional mengganti gambar kartu | `PatientInsurance : Update` |
| `PATCH` | `/{id}/card-image` dan `/admin/{id}/card-image` (**baru**) | Menyimpan atau mengganti gambar kartu hasil scan | `PatientInsurance : Update` |

Contoh `PATCH /admin/{id}/card-image`:

```json
{ "cardImageBase64": "data:image/png;base64,iVBORw0KGgo..." }
```

Response `200`:

```json
{
  "data": {
    "id": "…",
    "patientId": "…",
    "cardImagePath": "/uploads/patient-payer-cards/d97dca54…/insurance-20261006093234578-789c….jpg"
  },
  "message": "Gambar kartu patient insurance berhasil disimpan."
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release -p:UseSharedCompilation=false -o <scratchpad>` | `0 Error(s)`, `239 Warning(s)`; nol warning pada berkas yang disentuh | `PASS` | Keluaran build |
| `dotnet build` Debug | Kompilasi berhasil; salin `bin/Debug/.../QuilvianSystemBackend.exe` gagal `MSB3021` karena dikunci server dev pemilik (PID 17380, port 7184). Bukan galat kode | `EXISTING / ENVIRONMENT ISSUE` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes --configuration Release --no-build` (sebelum `BE-014`) | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict` × 9 berkas Amendment SK | `VIOLATION 0`, `REVIEW 0`, `INFO 0`, `Final result: PASS` | `PASS` | Keluaran checker |
| `AUTOMATED TEST` | `NOT APPLICABLE` — pola Bank Darah, tanpa project test | `NOT RUN` | — |

### Uji runtime

Lingkungan: build Release di scratchpad, `Development`, `Runtime__Role=Web`, `http://localhost:5199`,
DB `QuilvianNewDevSukma`, login SuperAdmin seed (kredensial tidak dicetak), pasien `00-00-00-17`.
Script dan hasil: `rt_be013.py`, `rt_be013_result.json`, `rt_be013.log` di scratchpad.

**Harness:** source saat ini punya seeder Operating Room yang membaca tabel milik 19 migration modul
lain yang belum diterapkan di `QuilvianNewDevSukma`, sehingga app gagal start
(`42P01 relation "public.MstSurgicalPreparationItem" does not exist`). Untuk build uji saja,
`RunStartupSeederAsync` dibungkus `try/catch` sementara, lalu `Program.cs` langsung dipulihkan dari
cadangan. Diff `Program.cs` diverifikasi identik dengan sebelum harness. Satu-satunya seeder yang
dilewati adalah `SurgicalPreparationItemSeeder`, dan tidak ada kaitannya dengan task ini.

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| R0 | `PATCH card-image` tanpa login | `401` | `PASS` |
| R1 | `POST` asuransi dengan `cardImageBase64` (PNG 320×200) | `200`, `cardImagePath` `/uploads/patient-payer-cards/<pasien>/insurance-….jpg` | `PASS` |
| R2 | Ambil file pada path publik | `200 image/jpeg`, header JPEG `FF D8` | `PASS` |
| R3 | `PATCH /admin/{id}/card-image` dengan data URL PNG lain | `200`, path baru berbeda dari R1 | `PASS` |
| R4 | `GET /patient-insurances?patientId=` (dipakai Pendaftaran) | `cardImagePath` = path R3 | `PASS` |
| R5 | Base64 rusak | `400` "Format base64 gambar kartu tidak valid." | `PASS` |
| R6 | Base64 valid, isi bukan gambar | `400` "Gambar kartu tidak dapat dibaca…" | `PASS` |
| R7 | 5 MB + 10 byte | `400` "Ukuran gambar kartu melebihi batas 5 MB." | `PASS` |
| R8 | `cardImageBase64` kosong | `400` | `PASS` |
| R9 | Id tidak ada | `404` | `PASS` |
| R10 | Route non-admin `PATCH /{id}/card-image` | `200` | `PASS` |
| R11 | `POST` asuransi tanpa gambar (perilaku lama) | `200`, `cardImagePath` `null` | `PASS` |
| R12 | `POST` dengan gambar rusak | `400` | `PASS` |
| R13 | Data R12 tidak tersimpan | `totalData 0` | `PASS` |

Hasil `BE-013`: **14/14 `PASS`**. Skenario penjamin perusahaan (C1–C7) dicatat di laporan
[`RJ-DOC-REV-BE-014`](RJ-DOC-REV-BE-014.md).

**Keadaan sesudah run:**
- **Database:** dua asuransi uji (`UJI-SK-…-A`, `UJI-SK-…-B`) dihapus lewat `DELETE /admin/{id}` (`200`), sehingga tersisa sebagai baris soft-delete.
- **File:** file kartu uji tertulis di storage build uji (scratchpad), bukan di `Storage/uploads` repository.
- **Proses:** app uji di port 5199 masih berjalan. Penghentiannya ditolak permission mode dan diserahkan ke pemilik.

Uji manual: `NOT APPLICABLE` — layar diuji pada `RJ-DOC-REV-FE-014`.

**Tidak dijalankan:** uji hak akses dengan akun tanpa `PatientInsurance : Update`. Atribut yang dipakai
sama persis dengan `PUT` yang sudah berjalan, dan tidak ada akun uji yang sesuai.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Create dengan `cardImageBase64` menghasilkan file dan `cardImagePath` publik | Terpenuhi | R1, R2 |
| 2. `PATCH card-image` mengganti `cardImagePath` asuransi yang ada | Terpenuhi | R3, R4, R10 |
| 3. Base64 rusak, bukan gambar, atau melebihi batas ditolak `400` | Terpenuhi | R5–R8, R12, R13 |
| 4. Id tidak ada → `404` | Terpenuhi | R9 |
| 5. Create tanpa gambar dan endpoint lain tidak berubah | Terpenuhi | R11; diff hanya aditif (§3.3) |
| Build tanpa error baru; EF tanpa perubahan model; runtime HTTP terhadap `QuilvianNewDevSukma` | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | (1) **File lama tidak dihapus.** Scan ulang tidak menghapus file kartu lama, sehingga file tertinggal di storage. Ini disengaja supaya service tidak menghapus file yang path-nya diisi manual dari master data. (2) **`PUT` asuransi menimpa kolom kartu.** Perilaku lama tetap: `PUT` tanpa `cardImagePath` mengosongkan kolom itu. Form master data asuransi sudah mengirim field tersebut |
| Risiko tersisa | **File kartu bisa dibuka tanpa login.** File tersaji lewat static files `/uploads`, sama seperti foto pasien kiosk. Siapa pun yang tahu URL-nya dapat membukanya. Nama file memakai GUID acak, jadi URL sulit ditebak. Pengamanan lebih kuat butuh keputusan terpisah. **DB uji tertinggal 19 migration.** `QuilvianNewDevSukma` belum menerapkan 19 migration modul lain, sehingga build saat ini gagal start terhadap DB itu tanpa harness |
| Perubahan sampingan | Harness `try/catch` di `Program.cs` dipasang untuk build uji lalu dipulihkan; diff diverifikasi identik |
| Interupsi | Sesi terputus sekali saat cek EF/QBE. Dilanjutkan dari kondisi terverifikasi; cek dijalankan ulang |
| Status Git | Lihat laporan [`RJ-DOC-REV-BE-014`](RJ-DOC-REV-BE-014.md) §7 (working tree bersama Amendment SK) |
| Langkah berikutnya | `RJ-DOC-REV-FE-014` |
