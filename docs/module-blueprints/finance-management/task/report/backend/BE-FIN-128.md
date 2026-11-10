# Laporan Perubahan Backend — `BE-FIN-128`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-128` |
| Judul | Hanya karyawan dan dokter tetap yang dapat menjadi penerima Benefit RS; dokter tamu ditolak di pencarian, enrollment, kelayakan, dan pemakaian plafon |
| Slice | `S1` Tata Kelola Pemilik Benefit RS (`MVP-17`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-23" |
| Trace | `FR-FIN-264`; `FIN-DEC-240`, `247`, `254`; `FIN-DES-123`; `FIN-VAL-1.14` `FIN-VAL-293`; `FIN-API-1.12` V.4 |
| Contract version | `FIN-API-1.12`, `FIN-VAL-1.14` — approved 3 November 2026 |
| Dependency | Tidak ada (Gelombang 1) |
| Klasifikasi | `MEDIUM` — skor 7 (repo 0, berkas diperiksa 2, berkas diubah 1, logika 2, kontrak 1, database 0, keamanan 1, workflow 0) |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. Frontend tidak disentuh |
| Model | Gemini 2.5 Pro |
| Commit backend saat dikerjakan | `304b9fa9` |
| Tanggal | 3 November 2026 |
| Status | 🟡 **Source selesai, kompilasi belum dibuktikan.** `dotnet build` **NOT RUN** atas instruksi pemilik ("tanpa build dotnet manual") |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module | `Corporate` / `HumanResource` / `BenefitManagement`. Service yang disentuh: `WorkforceBenefitOwnerResolver`, `HrBenefitConstants`, `EmployeeBenefitEligibilityService` |
| Keberlakuan | `TOUCHED LEGACY` pada tiga berkas service/constants HR. Tidak ada penambahan entity atau tabel baru |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-CODE-002` |
| Branch | `Yasmina`, sesuai penetapan roadmap |
| Working tree sebelum task | Memuat pekerjaan lain yang **tidak disentuh**: `FinanceReceivableInvoiceBatch*`, `FinanceReceiptService.cs`, `Program.cs`, `PayrollManagement/*` |

## 1. Masalah yang diperbaiki

Sebelumnya, pada commit `e5fc5888` (20 Oktober 2026), jenis SDM `GuestDoctor` (dokter tamu) sempat ditambahkan ke dalam daftar pemilik benefit di `WorkforceBenefitOwnerPolicy` dan `WorkforceBenefitOwnerResolver`, serta filter `UserType` dihapus dari pencarian SDM (`SearchWorkforceAsync`). Akibatnya, dokter tamu dapat dipilih sebagai penerima benefit rumah sakit dan berpotensi diterbitkan enrollment serta kartu `SYS-RS-BENEFIT`.

Menurut kebijakan HR dan keputusan bisnis yang disetujui (`FIN-DEC-240`, `FIN-DEC-247`, `FIN-DEC-254`):
1. **Pemilik Benefit RS hanya karyawan (`Employee`) dan dokter tetap (`PermanentDoctor`)**.
2. Dokter tamu (`GuestDoctor`) berstatus External User dan **TIDAK berhak** menerima Benefit RS.
3. Dokter tamu harus ditolak secara konsisten di seluruh titik masuk: pencarian SDM penerima benefit, pembuatan enrollment, pengecekan kelayakan, dan pemakaian plafon.
4. Pada pencarian SDM (`SearchWorkforceAsync`), SDM tanpa profil workforce atau yang berstatus batal (`IsCancel`) tidak boleh muncul, dan ID peran (`EmployeeId`/`DoctorId`) tidak boleh lagi disubstitusikan sebagai `WorkforceProfileId`.

## 2. Proses bisnis

1. **Pencarian Penerima Benefit (`SearchWorkforceAsync`)**:
   - Petugas HR mengetik nama/nomor staf (minimal 3 karakter).
   - Sistem melakukan *inner join* ke `MstWorkforceProfiles` dengan menyaring `UserType` hanya `Employee` dan `PermanentDoctor`.
   - Baris SDM yang berstatus `IsCancel` atau `IsDelete` pada entitas peran maupun profil langsung disaring keluar.
   - Dokter tamu (`GuestDoctor`) otomatis tidak muncul di hasil pencarian.
   - SDM yang tidak memiliki baris `MstWorkforceProfile` tidak dimunculkan; `WorkforceProfileId` selalu diisi dengan ID profil `MstWorkforceProfile.Id` yang sah, bukan ID peran dokter/karyawan.

2. **Resolusi Identitas Pemilik (`WorkforceBenefitOwnerResolver`)**:
   - Ketika pendaftaran enrollment atau pengecekan kelayakan memanggil `ResolveByWorkforceProfileAsync`, sistem memeriksa `WorkforceBenefitOwnerPolicy.AllowedUserTypes`.
   - Jika profil berjenis `GuestDoctor` (atau jenis lain selain `Employee` dan `PermanentDoctor`), resolver langsung menolak dengan hasil gagal:
     - `ReasonCode`: `BenefitEligibilityReasonCodes.OwnerTypeNotAllowed` (`OWNER_TYPE_NOT_ALLOWED`).
     - Pesan: *"Jenis SDM ini tidak termasuk penerima benefit kesehatan menurut kebijakan HR."* (`FIN-VAL-293`).
   - Dokter tetap (`PermanentDoctor`) yang tidak memiliki baris `MstEmployee` tetap diterima dengan `OwnerType = DOCTOR`, `DoctorId` terisi, dan `EmployeeId = null`.

### Contoh Konkret Skenario Rumah Sakit:
- **Kasus 1: "Dr. Budi" (Dokter Tamu / GuestDoctor)**:
  - Petugas HR mencari "Budi" pada formulir pendaftaran benefit → Dr. Budi **tidak muncul** dalam daftar pilihan penerima.
  - Jika dicoba mendaftarkan enrollment Dr. Budi secara langsung via API menggunakan `WorkforceProfileId` miliknya → sistem merespons HTTP 422 Unprocessable Entity dengan kode alasan `OWNER_TYPE_NOT_ALLOWED` dan pesan *"Jenis SDM ini tidak termasuk penerima benefit kesehatan menurut kebijakan HR."*
  - Kartu penjamin internal `SYS-RS-BENEFIT` tidak pernah diterbitkan untuk Dr. Budi.
- **Kasus 2: "dr. Andi" (Dokter Tetap / PermanentDoctor tanpa baris Karyawan)**:
  - Petugas HR mencari "Andi" → dr. Andi **muncul** di hasil pencarian dengan `OwnerType = DOCTOR` dan `NationalId` tersamar.
  - Resolver memproses profil dr. Andi → diterima sah dengan `OwnerType = DOCTOR`, `DoctorId = <guid>`, `EmployeeId = null`.
  - Pendaftaran enrollment dan kelayakan benefit berhasil diproses.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa
- `AGENTS.md`; `rules/backend/` (`TASK_RULES`, `BACKEND_ENGINEERING_CONTRACT`, `TEST_POLICY.md`);
- `HrBenefitConstants.cs`; `WorkforceBenefitOwnerResolver.cs`; `EmployeeBenefitEligibilityService.cs`; `EmployeeBenefitEnrollmentService.cs`;
- `Tests/QuilvianSystemBackend.PharmacyTests/WorkforceBenefitOwnerAcceptanceTests.cs` (baris 150 dan 240);
- Kontrak `FIN-VAL-1.14` (`FIN-VAL-293`), `FIN-API-1.12` V.4.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/BenefitManagement/Constants/HrBenefitConstants.cs` | Memperbarui `WorkforceBenefitOwnerPolicy.AllowedUserTypes`: menghapus `UserType.GuestDoctor` sehingga hanya memuat `UserType.Employee` dan `UserType.PermanentDoctor`. |
| `Areas/Corporate/HumanResource/BenefitManagement/Services/WorkforceBenefitOwnerResolver.cs` | Mengaktifkan penegakan `WorkforceBenefitOwnerPolicy.AllowedUserTypes` di dalam `LoadAsync`: profil dengan `UserType` di luar kebijakan (seperti `GuestDoctor`) langsung mengembalikan `WorkforceOwnerResolution.Fail` dengan `OwnerTypeNotAllowed` dan pesan `FIN-VAL-293`. Menghapus `UserType.GuestDoctor` dari percabangan `PermanentDoctor` dan dari method `ConflictFor`. |
| `Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitEligibilityService.cs` | Pada `SearchWorkforceAsync`: mengubah kueri join `MstWorkforceProfiles` menjadi *inner join* (menghilangkan `DefaultIfEmpty()`), menyaring `p.UserType == UserType.Employee` untuk karyawan dan `p.UserType == UserType.PermanentDoctor` untuk dokter, menyaring `!e.IsCancel`, `!d.IsCancel`, dan `!p.IsCancel`, serta mengisi `WorkforceProfileId` murni dari `p.Id` tanpa fallback ke ID peran `e.Id` atau `d.Id`. |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `FIN-VAL-293`: Respon 422 `OWNER_TYPE_NOT_ALLOWED` ditegakkan pada pembuatan enrollment dan pengecekan kelayakan jika profil adalah dokter tamu. Endpoint `SearchWorkforceAsync` menyaring dokter tamu dan SDM batal secara server-side. |
| Database | `NOT APPLICABLE`. Tidak ada penambahan kolom, tabel, maupun migration database. |
| Keamanan/Auth | Tidak ada izin akses baru. Perlindungan otorisasi level data: hanya SDM yang memenuhi kebijakan internal HR yang dapat mengakses dan menerima alokasi plafon benefit RS. |

## 4. Dokumentasi Komponen & Validasi

#### Matriks Validasi Penerima Benefit (`FIN-VAL-293`)
- **Titik Periksa**: Pendaftaran Enrollment, Validasi Kelayakan, Pencarian SDM Penerima Benefit.
- **Kondisi Pelanggaran**: Profil SDM memiliki `UserType == GuestDoctor` atau jenis lain di luar `WorkforceBenefitOwnerPolicy.AllowedUserTypes`.
- **Status HTTP**: `422 Unprocessable Entity` (atau penolakan resolusi identitas).
- **Kode Alasan**: `OWNER_TYPE_NOT_ALLOWED`.
- **Pesan**: *"Jenis SDM ini tidak termasuk penerima benefit kesehatan menurut kebijakan HR."*

## 5. Verifikasi

| Skenario atau pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik: tanpa build dotnet manual |
| QBE preflight / conformance | Sesuai | `PASS` | Sesuai `BACKEND_ENGINEERING_CONTRACT.md` (`TOUCHED LEGACY`) |
| Review diff dan scope | Tiga berkas source berubah tepat sesuai task card | `PASS` | `git diff` atas `HrBenefitConstants.cs`, `WorkforceBenefitOwnerResolver.cs`, `EmployeeBenefitEligibilityService.cs` |
| Verifikasi kontrak validasi | Menegakkan `FIN-VAL-293` dengan kode `OWNER_TYPE_NOT_ALLOWED` | `PASS` (telaah kode) | `WorkforceBenefitOwnerResolver.cs` |
| Verifikasi AC (1): GuestDoctor ditolak | Profil `GuestDoctor` mengembalikan `OWNER_TYPE_NOT_ALLOWED` | `PASS` (telaah kode) | `WorkforceBenefitOwnerResolver.LoadAsync` memeriksa `AllowedUserTypes` |
| Verifikasi AC (2): Pencarian saring dokter tamu & SDM batal | `SearchWorkforceAsync` hanya menyaring `PermanentDoctor` & `Employee`, mengecek `!IsCancel` | `PASS` (telaah kode) | `EmployeeBenefitEligibilityService.SearchWorkforceAsync` |
| Verifikasi AC (3): Dokter tetap tanpa EmployeeId diterima | Dokter tetap diterima dengan `DoctorId` terisi dan `EmployeeId = null` | `PASS` (telaah kode) | `WorkforceBenefitOwnerResolver.LoadAsync` cabang `PermanentDoctor` |
| Verifikasi AC (4): SDM tanpa profil workforce tidak muncul | Kueri pencarian menggunakan *inner join* ke `MstWorkforceProfiles` | `PASS` (telaah kode) | Menghapus `DefaultIfEmpty()` pada join profil |
| Kesesuaian Test 19 Okt yang ada | Selaras dengan `WorkforceBenefitOwnerAcceptanceTests.cs` baris 150 & 240 | `PASS` (telaah kode) | Logika resolver dan pencarian memulihkan ekspektasi test |
| Verifikasi runtime | Tidak ada server atau database yang dijalankan | `NOT RUN` | Sesuai aturan non-runtime |
| Automated test | `AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | — | — |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Profil `GuestDoctor` → `OWNER_TYPE_NOT_ALLOWED` | Terpenuhi menurut source; belum terbukti runtime | `WorkforceBenefitOwnerResolver.cs` memeriksa `WorkforceBenefitOwnerPolicy.AllowedUserTypes` |
| (2) Pencarian tidak menampilkan dokter tamu dan SDM batal | Terpenuhi menurut source; belum terbukti runtime | `EmployeeBenefitEligibilityService.cs` memfilter `UserType == PermanentDoctor`, `!IsCancel` |
| (3) Dokter tetap tanpa `EmployeeId` tetap diterima | Terpenuhi menurut source; belum terbukti runtime | Cabang `PermanentDoctor` mengisi `DoctorId` dan `EmployeeId = null` |
| (4) SDM tanpa profil workforce tidak tampil di pencarian | Terpenuhi menurut source; belum terbukti runtime | Inner join ke `MstWorkforceProfiles`, tidak mengisi `WorkforceProfileId` dengan ID peran |
| Syarat Rilis `R-1` (`FIN-DEC-254`) | **Tercatat sebagai prasyarat rilis** | Pemilik HR wajib membersihkan data historis dokter tamu sebelum rilis ke staging/produksi |
| DoD: build lulus | **Belum terpenuhi** | `dotnet build` NOT RUN atas instruksi pemilik ("tanpa build dotnet manual") |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ini berstatus 🟡 dan belum boleh ditandai ✅ sebelum `dotnet build` dijalankan atas instruksi pemilik dan lulus. |
| Syarat Rilis `R-1` | Sesuai `FIN-DEC-254`, rilis fitur ini ke lingkungan staging/produksi menunggu pemeriksaan dan pembersihan data historis dokter tamu oleh tim HR. |
| Risiko tersisa | Kompilasi belum dibuktikan kompiler; seluruh perubahan telah diverifikasi statis terhadap tipe dan signature C# 9+ / .NET 9. |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | (1) Pemilik dapat menjalankan `dotnet build` bila ingin mengangkat task ke ✅; (2) Melanjutkan ke task berikutnya pada roadmap Gelombang 1 (`BE-FIN-130` atau `BE-FIN-131`). |

`git status --short` di akhir pekerjaan (perubahan task ini ditandai **[task]**):

```text
 M Areas/Corporate/HumanResource/BenefitManagement/Constants/HrBenefitConstants.cs             [task]
 M Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitEligibilityService.cs [task]
 M Areas/Corporate/HumanResource/BenefitManagement/Services/WorkforceBenefitOwnerResolver.cs     [task]
 M docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md                       [task]
?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-128.md                   [task]
```

