# Laporan Perubahan Backend — `PAY-BE-24`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-24` |
| Judul | Kelengkapan Data Payroll (daftar, ringkasan, impor profil) |
| Slice | Belum terdaftar sebagai EPIC (di luar `EPIC PGJ-01`–`07`); didaftarkan setelah source ditulis, lihat bagian 7 |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-24` |
| Trace | Perintah pemilik 2026-11-16 (Payroll hanya membaca data HRD; yang ditulis Payroll hanya profil payroll). **Belum ada FR-PGJ dan PRL-DEC bernomor** — tidak dikarang |
| Contract version | **Tidak tercakup `PGJ-C1`** (`approved`, 2026-11-12 tidak memuat grup `Payroll Data Readiness`). Kontrak amandemen belum ditulis — gerbang `G1` terbuka |
| Dependency | — (membaca `WfpSalaryAssignment` dan `WfpBankAccount` milik HRD yang sudah ada; tidak menunggu `BE-HRD-360`–`362`) |
| Klasifikasi | `MEDIUM` — tiga berkas baru, tiga endpoint, satu operasi tulis (profil payroll) dalam satu transaksi, tanpa entity baru dan tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `PayrollDataReadinessController.cs`, `PayrollDataReadinessService.cs`, `PayrollDataReadinessDtos.cs`, `Program.cs` (satu baris `AddScoped`); dokumen laporan dan penandaan status |
| Model | Source ditulis 2026-11-16 (model tidak dicatat saat itu); pendaftaran roadmap dan laporan ini: Claude Sonnet 5.5 |
| Commit backend | `dc723f0f` (branch `Yasmina`, 2026-11-16, `feat(payroll): add Payroll Data Readiness functionality`); HEAD saat laporan ditulis `b155d8e4` |
| Tanggal | 2026-11-16 (revisi 2026-11-17) |
| Revisi | 2026-11-17 — atas perintah pemilik: impor profil CSV dipensiunkan (`POST /import` = 410), profil payroll disiapkan otomatis dari data HRD saat payroll run dibuka atau dihitung, kelengkapan dipisah menjadi tiga dimensi. Lihat bagian 8 |
| Status | 🟡 2026-11-17 — revisi source selesai; **`dotnet build` revisi NOT RUN** (kode revisi belum pernah dikompilasi); uji API belum; checker QBE resmi belum; kontrak belum ada (gerbang `G1`); blocker khusus: tetapan PPh dan iuran tidak punya sumber sah. Build 2026-11-16 (laporan lisan pemilik, bukan bukti agen) mencakup versi sebelum revisi |

> **Revisi 2026-11-17.** Bagian 2 (langkah 4 dan aturan impor), 3.1, 4 (`POST /import`), 5 (langkah uji 4–10), dan 6
> (kriteria 4–6) menggambarkan desain 2026-11-16 dan **digantikan bagian 8** untuk perilaku sekarang. Bagian lama
> dipertahankan sebagai catatan desain.

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`); `PrlPayroll` milik Payroll dan sudah ada |
| Keberlakuan | `NEW CODE` — controller, service, dan DTO baru; tidak menyentuh kode legacy |
| QBE yang berlaku | `QBE-SVC-001` (akses data hanya di service; controller hanya memanggil `PayrollDataReadinessService`), `QBE-NAM-001`, `QBE-CODE-002` (tidak ada penomoran Count/Max+1) |
| Hak akses | `PayrollDataReadiness : Read` (dua `GET`) dan `PayrollDataReadiness : Import` (`POST`). Tanpa `IsInRole`, nama peran, atau `UserType` |
| Hasil | Tidak ada pelanggaran pada tinjauan source; checker QBE resmi `NOT RUN` |

## 1. Masalah yang diselesaikan

Layar Profil Penggajian bersumber dari tabel profil payroll (`PrlPayroll`), sehingga pegawai atau dokter
yang belum punya profil tidak pernah tampil. Petugas Payroll tidak punya cara melihat siapa yang belum siap
digaji dan apa yang kurang, serta tidak punya cara membuat profil payroll banyak orang sekaligus.

## 2. Proses bisnis

**Tujuan.** Petugas Payroll melihat seluruh pegawai dan dokter aktif beserta kelengkapan datanya, lalu
membuat profil payroll untuk orang yang data HRD-nya sudah lengkap.

**Batas kepemilikan (perintah pemilik 2026-11-16).** Payroll hanya **membaca** penetapan gaji utama dan rekening
payroll milik HRD. Satu-satunya yang ditulis Payroll adalah profil payroll. Melengkapi penetapan gaji dan rekening
dikerjakan di modul HR (halaman Data Penggajian Pegawai, slice S-A8).

**Langkah:**

1. Petugas membuka daftar. Sistem menggabungkan populasi (pegawai dan dokter aktif dari master HRD) dengan tiga
   himpunan: profil payroll, penetapan gaji utama aktif, dan rekening payroll aktif.
2. Setiap orang diberi status **Lengkap** hanya bila: profil payroll ada, aktif, eligible, dan berstatus `Active`;
   ada penetapan gaji utama aktif yang sudah disetujui HRD; dan rekening payroll aktif sudah terverifikasi HRD.
3. Yang belum lengkap membawa daftar alasan berbahasa Indonesia yang menyebut siapa yang melengkapinya
   (misalnya "Belum ada rekening payroll di HRD.").
4. Petugas mengirim impor (opsional uji kering dulu). Setiap baris membuat profil payroll baru dengan struktur, grade,
   penetapan gaji, dan rekening yang **diambil dari HRD**, ditambah tetapan PPh, iuran, dan Cost Center dari berkas.

**Aturan impor:**

| Aturan | Isi |
| --- | --- |
| Semua atau tidak sama sekali | Satu baris gagal membatalkan seluruh impor; tidak ada data tersimpan; respons 400 membawa `errors.failedRows` |
| Uji kering | `DryRun = true` menjalankan seluruh pemeriksaan dan menghitung hasil tanpa menyimpan |
| Batas | 1–200 baris per impor |
| Idempoten | Profil payroll yang sudah ada dilewati ("Sudah ada (dilewati)"); pajak, iuran, dan Cost Center dari berkas tidak dipakai |
| Syarat profil baru | Penetapan gaji utama dan rekening payroll sudah ada di HRD. Belum disetujui atau belum terverifikasi **tidak** menolak, hanya memberi peringatan pada baris |
| Pemilihan data HRD | Penetapan: tanggal mulai terbaru. Rekening: terverifikasi, lalu utama, lalu terbaru |
| Data HRD | Tidak pernah diubah |
| Log audit | `PayrollDataReadiness.Import` memuat jumlah baris, dibuat, dilewati, dan pelaku; **tanpa nominal pajak dan iuran** |

**Contoh** (data samaran): berkas 3 baris. Baris 1 pegawai `EMP-0001` lengkap di HRD, pajak Rp 250.000 dan iuran
Rp 150.000: profil dibuat. Baris 2 `EMP-0002` sudah punya profil: dilewati dengan peringatan. Baris 3 `EMP-0003`
belum punya rekening payroll di HRD: gagal. Hasilnya 400, `failedRows = [{ rowNumber: 3, reason: "Rekening payroll
belum ada di data HRD. Lengkapi di HRD lalu impor lagi." }]`, dan baris 1 **tidak** tersimpan.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Pengguna tanpa `PayrollDataReadiness : Read` | 403 pada kedua `GET` |
| Pengguna tanpa `PayrollDataReadiness : Import` | 403 pada `POST /import` |
| Berkas kosong, atau lebih dari 200 baris | 400 dengan pesan batas |
| Nomor kosong, tidak ditemukan, tidak aktif, atau cocok lebih dari satu orang | Baris gagal (400 seluruh impor) |
| Orang yang sama di dua baris | Baris kedua gagal "Orang ini sudah muncul di baris N." |
| Cost Center tidak ada atau tidak aktif | Baris gagal |
| Pajak atau iuran negatif | Baris gagal |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollDataReadinessController.cs` | Baru | `GET /`, `GET /summary`, `POST /import`; `[AccessController]` modul `CORPORATE_PAYROLL_DATA_READINESS`, `SortOrder = 65` |
| `Areas/Corporate/PayrollManagement/Services/PayrollDataReadinessService.cs` | Baru | `ListAsync`, `SummaryAsync`, `ImportAsync`; satu `SaveChanges` untuk impor |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollDataReadinessDtos.cs` | Baru | `PayrollDataReadinessQuery`, `...ItemResponse`, `...SummaryResponse`, `ImportPayrollDataRequest/Row`, `PayrollDataImportResult/RowResult/FailedRow`, `PayrollDataImportException` |

`Program.cs` baris 458 mendaftarkan service (`builder.Services.AddScoped<PayrollDataReadinessService>()`) — berkas
ketiga yang disentuh task ini selain tiga berkas di atas.

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint baru; **belum ada di `api-contract.md`** |
| Database | Tidak ada perubahan skema dan tidak ada migration. Menulis `PrlPayrolls` (insert saja); membaca `MstEmployees`, `MstDoctors`, `WfpSalaryAssignments`, `WfpBankAccounts`, `MstCostCenters` |
| Keamanan/Auth | Dua action baru (`Read`, `Import`) harus dicentang admin di Pengaturan > Manajemen Role > Akses Role. Respons daftar tidak memuat nominal gaji atau nomor rekening |
| Kinerja | Daftar dan ringkasan memuat seluruh populasi ke memori lalu menyaring, sengaja (dibatasi jumlah SDM rumah sakit; komentar di source). Perlu dipantau bila populasi membesar |

## 4. Dokumentasi endpoint

### `[Tags("Corporate / Payroll Management / Payroll Data Readiness")]`

Base URL: `api/v1/corporate/payroll-management/payroll-data-readiness`

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kelengkapan per pegawai dan dokter aktif | `PayrollDataReadiness : Read` | query `search`, `personType` (`Karyawan`/`Dokter`), `completeness` (`Lengkap`/`Belum lengkap`), `pageNumber`, `pageSize` (maks 100) | `PagedResult<PayrollDataReadinessItemResponse>` | 200, 403 |
| `GET` | `/summary` | Lima hitungan: total, punya profil, penetapan disetujui, rekening terverifikasi, lengkap | `PayrollDataReadiness : Read` | — | `PayrollDataReadinessSummaryResponse` | 200, 403 |
| `POST` | `/import` | Membuat profil payroll banyak orang dari data HRD (semua atau tidak sama sekali; `dryRun` untuk uji kering) | `PayrollDataReadiness : Import` | `ImportPayrollDataRequest { dryRun, rows[ { personNumber, costCenterCode?, taxAmount, insuranceAmount } ] }` | `PayrollDataImportResult` | 200, 400 (`errors.failedRows`), 403 |

**Contoh respons daftar** (data samaran):

```json
{
  "pageNumber": 1, "pageSize": 25, "totalData": 2, "totalPage": 1,
  "items": [
    { "workforceProfileId": "…", "personType": "Karyawan", "number": "EMP-0001", "name": "Pegawai Contoh A",
      "hasPayrollProfile": true, "hasApprovedSalaryAssignment": true, "hasPendingSalaryAssignment": false,
      "bankStatus": "Terverifikasi", "isComplete": true, "issues": [] },
    { "workforceProfileId": "…", "personType": "Dokter", "number": "DOC-0001", "name": "Dokter Contoh B",
      "hasPayrollProfile": false, "hasApprovedSalaryAssignment": false, "hasPendingSalaryAssignment": true,
      "bankStatus": "Belum ada", "isComplete": false,
      "issues": ["Profil payroll belum ada.", "Penetapan gaji utama belum disetujui HRD.", "Belum ada rekening payroll di HRD."] }
  ]
}
```

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil menurut pemilik, 2026-11-16 | `PASS (dilaporkan pemilik)` | Laporan lisan; tanpa keluaran tersimpan; bukan bukti agen |
| Review diff dan cakupan | Tiga berkas Payroll; tidak menyentuh HRD, tanpa migration | `PASS` | Tinjauan source |
| Controller hanya memanggil service | Tidak ada `ApplicationDbContext` di controller | `PASS` | Tinjauan source |
| Atribut akses | `AccessAction("Read")`/`AccessPermission("PayrollDataReadiness","Read")` dan `Import`/`Import` cocok; `ControllerName` = resource | `PASS` | Tinjauan source |
| Checker QBE resmi | Belum dijalankan | `NOT RUN` | — |
| Uji API | Belum dijalankan | `NOT RUN` | Langkah di bawah |
| Kontrak | `PGJ-C1` tidak memuat grup ini | `OPEN` | Gerbang `G1` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

**Langkah uji untuk pemilik** (DB uji, data samaran; sertakan akun dengan hak `PayrollDataReadiness : Read` dan `Import`):

1. `GET /` tanpa saringan. Harapan: seluruh pegawai dan dokter aktif tampil, termasuk yang belum punya profil.
2. `GET /?completeness=Belum lengkap` dan `?personType=Dokter`. Harapan: hanya baris yang sesuai.
3. `GET /summary`. Harapan: `totalPeople` sama dengan `totalData` langkah 1.
4. `POST /import` dengan `dryRun: true`, satu baris orang yang lengkap di HRD. Harapan: 200, `payrollProfilesCreated = 1`, tabel `PrlPayrolls` tidak bertambah.
5. Sama dengan `dryRun: false`. Harapan: satu profil baru; `GET /` menandai `hasPayrollProfile = true`.
6. Ulangi langkah 5. Harapan: 200, `payrollProfilesSkipped = 1`, tidak ada profil ganda.
7. Berkas dua baris: satu valid, satu orang tanpa rekening payroll di HRD. Harapan: 400, `errors.failedRows` memuat baris kedua, **baris pertama tidak tersimpan**.
8. Berkas 201 baris, lalu berkas kosong. Harapan: 400 masing-masing.
9. Akun tanpa `Read` dan akun tanpa `Import`. Harapan: 403.
10. Periksa log audit `PayrollDataReadiness.Import`. Harapan: tidak memuat nominal pajak atau iuran.

**Tidak dijalankan:** uji API, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar bersumber dari populasi pegawai dan dokter aktif, bukan dari profil payroll | Terpenuhi (tinjauan source); belum diuji di DB | Bagian 5 |
| 2. `IsComplete` hanya bila profil aktif, penetapan gaji utama disetujui, dan rekening terverifikasi; alasan menyebut HRD | Terpenuhi (tinjauan source) | Bagian 5 |
| 3. Ringkasan lima hitungan | Terpenuhi (tinjauan source) | Bagian 5 |
| 4. Impor semua atau tidak sama sekali; uji kering tidak menyimpan; idempoten; data HRD tidak diubah | Terpenuhi (tinjauan source); belum diuji | Langkah uji 4–8 |
| 5. Hak `Read` dan `Import`; tanpa hardcode peran | Terpenuhi (tinjauan source) | Bagian 5 |
| 6. Log audit tanpa nominal | Terpenuhi (tinjauan source) | Bagian 5 |
| DoD: build hijau | Terpenuhi menurut pemilik; tanpa bukti keluaran | Bagian 5 |
| DoD: QBE bersih | Checker resmi **belum** dijalankan | `NOT RUN` |
| DoD: kriteria terbukti | Statis saja; uji API **belum** | Langkah uji menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

Kriteria di atas **disusun dari perilaku source**, bukan dari kontrak yang disetujui, karena kontrak belum ada.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Pendaftaran terlambat | Source ditulis dan di-commit pada 2026-11-16 sebelum task ini didaftarkan di roadmap; ID `PAY-BE-24` dan statusnya diberikan setelahnya atas permintaan pemilik |
| Gerbang `G1` | Kontrak `PGJ-C1` tidak memuat grup `Payroll Data Readiness`. Perlu amandemen kontrak (grup API, matriks validasi `failedRows`, matriks hak akses `PayrollDataReadiness : Read/Import`, kamus data tanpa tabel baru, acceptance test) dan persetujuan pemilik. Itu pekerjaan `design-business-module` (model minimum Claude Opus 5), tidak dikerjakan di sini |
| Requirement dan keputusan | Belum ada FR-PGJ dan PRL-DEC bernomor untuk halaman ini; tidak dikarang |
| Urutan kerja | HR melengkapi dan menyetujui data penggajian (S-A8: `BE-HRD-360`–`362`) → Payroll impor profil → Simulasi. Data produksi per 2026-11-16: 0 penetapan gaji dan 0 rekening terverifikasi, sehingga impor baru berhasil setelah HR mengisinya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pemilik (`dc723f0f`). Tidak ada yang di-stage atau di-commit oleh pembuat laporan ini |
| Langkah berikutnya | Digantikan bagian 8.12 (revisi 2026-11-17): bangun Backend revisi, uji API delapan langkah di bagian 8.10, putuskan sumber tetapan PPh dan iuran, dan amandemen kontrak. Langkah uji 1–10 di bagian 5 menggambarkan desain impor yang sudah dipensiunkan |

---

## 8. Revisi 2026-11-17 — impor profil dipensiunkan, profil disiapkan otomatis dari data HRD

Bagian ini **menggantikan** bagian 2 (langkah 4 dan aturan impor), 3.1, 4 (`POST /import`), 5 (langkah uji 4–10), dan 6
(kriteria 4–6) untuk perilaku sekarang. Bagian itu dipertahankan sebagai catatan desain 2026-11-16.

### 8.1 Perintah pemilik dan perubahan keputusan

Pemilik pada 2026-11-17 meminta kebutuhan "Impor Profil Payroll" manual dihapus:

- HRD tetap sumber kebenaran untuk identitas, penetapan gaji utama, struktur dan grade, rekening payroll, dan penempatan.
- Payroll membaca data itu. Profil operasional internal (`PrlPayroll`) boleh tetap ada, tetapi **disiapkan otomatis**, bukan lewat CSV.
- Payroll tidak menyetujui gaji dan tidak memverifikasi rekening.

Ini **membalik** catatan 2026-11-16 ("impor di halaman ini TETAP ada"). Keputusan 2026-11-17 yang berlaku.

### 8.2 Backend Governance Preflight (revisi)

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`). Tidak ada entity baru, sehingga `QBE-MOD-002` tidak dipicu |
| Keberlakuan | `NEW CODE` (dua berkas baru) dan `TOUCHED LEGACY` (`PayrollRunService`, `PayrollCalculationService`, `Program.cs`) |
| QBE yang berlaku | `QBE-SVC-001` (akses data di service, controller tidak menyentuh konteks), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001` (log perubahan state memuat aktor), `QBE-TXN-001` (konsistensi lintas record dalam satu transaksi), `QBE-VAL-001`, `QBE-DTO-001` (tidak ada entity EF di kontrak), `QBE-AUD-001` |
| Hak akses | `GET` tetap `PayrollDataReadiness : Read`. Atribut `Import` pada endpoint yang dipensiunkan dipertahankan (preseden `PatientMembershipController`) supaya registri Akses Role tidak berubah mendadak; deskripsinya diubah menjadi "dihentikan" |
| Hasil | Tidak ada pelanggaran pada tinjauan source; checker QBE resmi `NOT RUN` |

### 8.3 Berkas

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollHrdSourceReader.cs` | Baru | Pembaca sumber HRD (baca-saja, `AsNoTracking`): populasi, penetapan gaji utama, rekening payroll, penempatan organisasi. `PayrollProfileRules`: aturan profil yang dipakai bersama |
| `Areas/Corporate/PayrollManagement/Services/PayrollProfileProvisioningService.cs` | Baru | Menyiapkan profil payroll otomatis; hanya membuat yang belum ada |
| `Areas/Corporate/PayrollManagement/Services/PayrollDataReadinessService.cs` | Ditulis ulang | Hanya baca. `ImportAsync`, pencarian impor, dan `LoggerService` dihapus. Satu fungsi `LoadReadinessAsync` untuk daftar dan ringkasan |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollDataReadinessDtos.cs` | Diubah | Field baru (additive); seluruh DTO impor dihapus |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollDataReadinessController.cs` | Diubah | `POST /import` menjawab 410 Gone tanpa membaca kiriman |
| `Areas/Corporate/PayrollManagement/Services/PayrollRunService.cs` | Diubah | Konstruktor, `CreateAsync`, `CalculateAsync`, dan satu fungsi bantu catatan |
| `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs` | Diubah (satu peringatan) | Peringatan bila tetapan PPh dan iuran sama-sama nol |
| `Program.cs` | Diubah | Dua `AddScoped` (`PayrollHrdSourceReader`, `PayrollProfileProvisioningService`) dan komentar |

Tanpa migration dan tanpa perubahan skema.

### 8.4 Skema alur final

```text
HRD: daftarkan pegawai/dokter ─> penetapan gaji utama ─> HRD menyetujui gaji ─> rekening payroll ─> HRD memverifikasi rekening
                                         │ (hanya dibaca Payroll)                         │
                                         ▼                                                 ▼
Payroll: GET /payroll-data-readiness (baca-saja, kondisi HARI INI)
   HrdComplete ── PayrollProfileStatus ── IsComplete ── TaxInsuranceStatus (terpisah)
                                         │
Payroll: POST /payroll-runs (buka run)  ─┬─> InTransactionAsync (Serializable + kunci advisory per periode)
Payroll: POST /payroll-runs/{id}/calculate ┘      │
                                                  ├─ EnsureProfilesAsync: buat PrlPayroll yang BELUM ada
                                                  │    (syarat: penetapan gaji utama yang berlaku pada periode DAN rekening payroll aktif;
                                                  │     profil yang sudah ada dalam status apa pun TIDAK disentuh)
                                                  ├─ SeedEmployeesAsync: populasi run dari profil Active + eligible (tidak berubah)
                                                  └─ CalculateRunAsync: gaji pokok dari penetapan HRD yang DISETUJUI; rekening belum
                                                     terverifikasi => penahanan pembayaran (tidak berubah)
```

### 8.5 Aturan penyiapan profil otomatis

| Aturan | Isi |
| --- | --- |
| Pemicu | Hanya `PayrollRunService.CreateAsync` dan `CalculateAsync` (dua metode yang dipanggil `PayrollRunsController`), di dalam transaksi `Serializable` dengan kunci advisory per periode atau per run yang sudah ada. **Tidak** dari `GET` |
| Kunci | `WorkforceProfileId`. Pegawai dan dokter tidak tertukar; `EmployeeId` hanya diisi untuk karyawan |
| Populasi | Pegawai dan dokter aktif dari master HRD (sama dengan kelengkapan data) |
| Hanya membuat | Orang yang sudah punya profil (belum dihapus, **apa pun statusnya**) dilewati sepenuhnya: tidak ditimpa, tidak diaktifkan kembali (`Suspended`, `OnHold`, `Terminated` tetap), referensinya tidak diperbarui |
| Syarat profil baru | Ada penetapan gaji utama aktif yang berlaku pada periode **dan** rekening payroll aktif yang berlaku. Persetujuan dan verifikasi tidak disyaratkan: gaji yang belum disetujui tetap menahan hitung, rekening yang belum terverifikasi tetap menahan pembayaran (perilaku mesin hitung tidak berubah) |
| Tanpa data dummy | Tanpa penetapan gaji atau rekening, profil tidak dibuat; orang itu tampil sebagai kekurangan HRD |
| Pemilihan sumber | Penetapan: yang disetujui didahulukan, lalu tanggal mulai terbaru. Rekening: terverifikasi, lalu utama, lalu terbaru (urutan sama dengan pemilih rekening mesin hitung). Penempatan: primer aktif, tanggal mulai terbaru |
| Idempoten | Pemanggilan ulang tidak membuat apa pun. Indeks unik `PrlPayroll(WorkforceProfileId)` dengan filter `IsDelete = false` di database menjamin tidak ada dua profil aktif untuk satu orang |
| Konkurensi | Transaksi `Serializable` + kunci advisory milik pemanggil. Dua proses pada periode yang sama diserialkan. Bila dua run pada periode **berbeda** menyiapkan orang yang sama pada detik yang sama, salah satu transaksi gagal (kegagalan serialisasi atau pelanggaran indeks unik) dan dapat diulang; tidak ada profil ganda yang tersimpan |
| Penyimpanan | Menyimpan sendiri sekali setelah menambah profil baru, supaya `SeedEmployeesAsync` (membaca tanpa pelacakan) langsung melihatnya. Wajib dipanggil di dalam transaksi (diperiksa; melempar bila tidak) |
| Data HRD | Hanya dibaca. Tidak ada `ApprovedAt`, `IsVerified`, penetapan gaji, atau rekening yang dibuat atau diubah (dikunci uji pada source) |
| Nilai finansial | Kolom kompatibilitas gaji, `TaxAmount`, dan `InsuranceAmount` tidak diisi (tetap nol): tidak ada sumber sah (lihat 8.8). Nol dianggap **belum ditetapkan** |
| Cost Center | Dari `CostCenterId` penempatan organisasi primer aktif HRD yang berlaku, hanya bila Cost Center itu masih aktif di master; selain itu kosong, tidak dikarang. `OrganizationAssignmentId` ikut diisi |
| Penanda asal | `PrlPayroll.Description` = "Disiapkan otomatis dari data HRD saat payroll run dipersiapkan." (kolom sudah ada) |
| Audit | `PayrollProfile.AutoProvisioned`: jumlah dibuat, sudah ada, dilewati, pemicu, periode, aktor. **Tanpa nama, nomor rekening, gaji, pajak, atau iuran** |
| Hasil pada run | Catatan di ringkasan validasi run (saat dibuka) atau peringatan (saat dihitung): jumlah profil yang disiapkan dan jumlah orang yang belum masuk karena HRD belum lengkap |

### 8.6 Kelengkapan: tiga dimensi (kontrak additive)

Satu fungsi (`LoadReadinessAsync`) dipakai daftar dan ringkasan, dengan sumber HRD dan aturan profil yang sama dengan
penyiapan otomatis. Pembacaan memakai **tanggal hari ini** untuk masa berlaku penetapan gaji dan rekening, sehingga
perubahan HRD tercermin pada pembacaan berikutnya.

| Field respons (baris) | Arti |
| --- | --- |
| `HrdComplete` (baru) | (1) Sumber HRD lengkap: penetapan gaji yang berlaku sudah disetujui **dan** rekening payroll sudah terverifikasi |
| `PayrollProfileStatus` (baru) | (2) `Siap`; `Akan disiapkan otomatis` (belum ada, tetapi gaji dan rekening sudah ada di HRD); `Menunggu data HRD` (belum ada dan belum dapat disiapkan); `Perlu perhatian` (ada tetapi tidak aktif, tidak eligible, atau bukan `Active`; tidak diaktifkan otomatis) |
| `IsComplete` (arti disempurnakan) | (3) Siap mengikuti proses Payroll: tidak ada penghalang. Benar bila `HrdComplete` dan profil `Siap` atau `Akan disiapkan otomatis` |
| `TaxInsuranceStatus` (baru) | `Sudah ditetapkan`, `Belum ditetapkan` (PPh dan iuran pada profil sama-sama nol), atau `Menunggu profil`. **Tidak** memengaruhi `IsComplete` dan bukan kekurangan HRD |
| `Notes` (baru) | Catatan yang tidak menghalangi (mis. "akan disiapkan otomatis", tetapan belum ditetapkan) |
| `Issues` | Penghalang saja; "Profil payroll belum ada." tidak lagi menjadi penghalang |
| Tetap | `HasPayrollProfile` (= profil `Siap`), `HasApprovedSalaryAssignment`, `HasPendingSalaryAssignment`, `BankStatus` |

Ringkasan menambah `HrdComplete`, `ProfilesToPrepare`, `ProfilesNeedAttention`, dan `TaxInsuranceNotSet`; lima hitungan lama
tetap. Filter `search`, `personType`, `completeness`, dan paginasi tidak berubah.

**Contoh baris** (data samaran): dokter dengan gaji disetujui dan rekening terverifikasi, tanpa profil:

```json
{ "personType": "Dokter", "hasPayrollProfile": false, "hrdComplete": true, "payrollProfileStatus": "Akan disiapkan otomatis",
  "taxInsuranceStatus": "Menunggu profil", "isComplete": true, "issues": [],
  "notes": ["Profil payroll akan disiapkan otomatis dari data HRD saat payroll run dipersiapkan. Tidak perlu impor.", "Tetapan PPh dan iuran pada profil yang disiapkan otomatis dimulai dari nol (belum ditetapkan)."] }
```

### 8.7 Endpoint setelah revisi

| Method | Path | Hasil |
| --- | --- | --- |
| `GET` | `/` | Tidak berubah (field baru additive) |
| `GET` | `/summary` | Tidak berubah (hitungan baru additive) |
| `POST` | `/import` | **410 Gone**, `ApiResponse` dengan pesan "Impor profil payroll sudah tidak tersedia…". Tidak membaca kiriman, tidak membuat atau mengubah data. Hak `Import` dipertahankan di atribut |

**Tidak berubah:** `POST payroll-runs/{id}/import-hrd` (kehadiran, cuti, lembur ke satu payroll run) adalah proses lain dan tetap ada.
Alur "Modern" lama (`PayrollRunModernController`, namespace HumanResource) membentuk populasi langsung dari profil
workforce dan penetapan gaji tanpa `PrlPayroll`, sehingga tidak terpengaruh dan tidak disentuh.

### 8.8 Sumber yang tidak tersedia (blocker khusus dependency)

| Data | Temuan | Keputusan |
| --- | --- | --- |
| `TaxAmount` dan `InsuranceAmount` | **Tidak ada sumber sah di HRD.** `PrlTax` hanya memuat status PTKP/NPWP dan `PrlInsurance` hanya tarif dan status kepesertaan; mesin hitung membaca nilai dari `PrlPayroll` (`taxAndInsuranceSource`, rumus PPh/BPJS belum disahkan, ditandai `needsConfirmation`). Sebelumnya nilai itu hanya masuk lewat berkas impor | Tidak diisi dan tidak diturunkan dari tarif (itu rumus baru). Nol dianggap **belum ditetapkan** dan ditandai di kelengkapan data, catatan run, dan peringatan mesin hitung (tidak mengubah angka maupun menahan pengajuan). **BLOCKER:** tidak ada jalur massal untuk mengisinya sekarang; satu-satunya jalur ialah `PUT` pada master profil (`WfpPayrollController`, hak `WorkforcePayrollProfile : Update`) per orang. Perlu keputusan pemilik: kebijakan PPh dan BPJS yang disahkan, atau layar pengelolaan tetapan |
| `CostCenterId` | Sumber HRD ada: `WfpOrganizationAssignment.CostCenterId`. Namun penempatan awal yang dibuat saat pegawai didaftarkan tidak mengisi Cost Center dan tidak disetujui | Dipakai bila ada; kosong bila tidak. Dampak: laporan beban gaji memuat orang itu tanpa Cost Center sampai HRD mengisinya. Bukan blocker |

### 8.9 Dampak

| Area | Dampak |
| --- | --- |
| Payroll Run | Pegawai dan dokter yang eligible di HRD masuk run tanpa CSV; satu profil per orang; penetapan belum disetujui tetap menjadi penahan hitung; rekening belum terverifikasi tetap menahan pembayaran. `CalculateAsync` hanya terjangkau bila run masih menerima input, sehingga run yang sudah disahkan dan snapshot-nya tidak berubah |
| Mesin hitung | Satu peringatan baru (tetapan PPh dan iuran nol). Tidak ada nilai atau rumus yang berubah; peringatan tidak menahan pengajuan (hanya `Blockers` dan `ErrorEmployeeCount` yang menahan) |
| Jasa medis | Dokter yang punya profil dikenali sebagai "penerima payroll" pada kandidat klaim (`NoPathYet`). Klaim tetap manual dan dijaga `MedicalFeePayrollPathEnabled`; jalur Finance AP yang sudah ada tidak berubah. Dokter kini mendapat profil bila HRD punya penetapan gaji dan rekening untuknya |
| Simulasi | Tidak berubah: kandidat tetap dari profil yang sudah ada. Orang yang baru akan disiapkan tidak ikut simulasi sampai run dibuka. **Tindak lanjut** (simulasi tidak boleh menulis) |
| Restitusi | Tidak berubah (membaca keberadaan profil) |
| Data historis | Tidak ada profil, run, atau snapshot yang dihapus atau diubah |

### 8.10 Validasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Tinjauan source statis (tipe, `using`, DI, nama DbSet, nama properti, tanda tangan `LoggerService`) terhadap source saat ini | Tidak ditemukan ketidakcocokan; tidak ada tabrakan nama tipe | `PASS` (statis) | Tinjauan |
| `dotnet build` | Tidak dijalankan (instruksi pemilik: tidak membangun otomatis). **Kode revisi belum pernah dikompilasi** | `NOT RUN` | — |
| Uji FE yang membaca source backend (`payroll-data-readiness.test.mjs`) | 17 uji, 17 lulus, 0 dilewati: nama field DTO, nilai status, 410 tanpa jalur tulis, readiness murni baca, penyiapan hanya membuat (tanpa `ApprovedAt`, `IsVerified`, pajak, iuran), urutan penyiapan sebelum seed di `CreateAsync` dan `CalculateAsync`, `import-hrd` ada, DI terdaftar, indeks unik ada | `PASS` | Keluaran perintah |
| Checker QBE resmi | Belum dijalankan | `NOT RUN` | — |
| Uji API dan database | Belum dijalankan | `NOT RUN` | Langkah uji di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

**Skenario yang ditangani di source (tinjauan statis; belum diuji runtime):**

| Skenario | Penanganan |
| --- | --- |
| TC-01 pegawai aktif tanpa profil | Tetap tampil (populasi dari HRD): `Akan disiapkan otomatis` bila gaji dan rekening ada, `Menunggu data HRD` bila tidak |
| TC-02 penetapan gaji belum ada | Penghalang "Belum ada penetapan gaji utama di HRD yang berlaku saat ini."; profil tidak dibuat |
| TC-03 gaji disetujui, rekening belum terverifikasi | `IsComplete` salah; profil boleh dibuat; mesin hitung menahan pembayaran (tidak berubah) |
| TC-04 gaji dan rekening memenuhi syarat | Profil dibuat otomatis saat run dibuka atau dihitung |
| TC-05 profil sudah ada | Dilewati; pemanggilan ulang tidak membuat apa pun |
| TC-06 profil `Suspended`/`OnHold`/`Terminated` | Dilewati sepenuhnya; kelengkapan: `Perlu perhatian`; tidak diaktifkan; `SeedEmployeesAsync` tetap menyaring `Active` |
| TC-07 data HRD berubah | Kelengkapan membaca HRD langsung; profil lama tidak disentuh; run yang sudah disahkan tidak terjangkau; snapshot utuh |
| TC-08 pegawai dan dokter | Kunci `WorkforceProfileId`; `EmployeeId` hanya untuk karyawan |
| TC-09 dua proses bersamaan | Serialisasi oleh transaksi dan kunci advisory; indeks unik; balapan lintas periode: satu gagal dan dapat diulang |
| TC-10 pajak atau iuran tanpa sumber | Tidak ada nilai dibuat; nol ditandai belum ditetapkan |
| TC-11 impor kehadiran, cuti, lembur | `import-hrd` tidak disentuh |
| TC-12 impor profil manual | `POST /import` = 410; layar tidak memanggilnya |

**Langkah uji untuk pemilik** (DB uji, data samaran; akun dengan `PayrollDataReadiness : Read` dan hak Payroll Run):

1. `GET /` dan `GET /summary`. Harapan: `totalPeople` sama; field baru terisi; `payrollProfileStatus` sesuai keadaan tiap orang.
2. `POST /import` dengan kiriman apa pun. Harapan: 410; tabel `PrlPayroll` tidak berubah.
3. Setelah HR melengkapi dan menyetujui gaji serta rekening satu orang: buka payroll run periode yang berlaku. Harapan: `PrlPayroll` bertambah satu untuk orang itu (`Description` bertanda otomatis, `TaxAmount` dan `InsuranceAmount` nol); orang itu masuk run; ringkasan validasi run memuat catatan; `ApprovedAt` dan `IsVerified` di HRD tidak berubah.
4. Hitung ulang run yang sama. Harapan: tidak ada profil baru atau ganda; orang tanpa gaji atau rekening di HRD tidak masuk dan tercatat di peringatan; peringatan "Tetapan PPh dan iuran … masih nol" per orang.
5. Orang dengan profil `Suspended`. Harapan: tidak diaktifkan; kelengkapan `Perlu perhatian`; tidak masuk run.
6. HR mengubah rekening atau menyetujui gaji. Harapan: `GET /` mencerminkannya; profil lama tidak berubah; run yang sudah disahkan tidak berubah.
7. `POST payroll-runs/{id}/import-hrd`. Harapan: tetap berfungsi.
8. Periksa log audit `PayrollProfile.AutoProvisioned`. Harapan: hanya hitungan, pemicu, periode, aktor.

### 8.11 Acceptance criteria roadmap yang tidak lagi berlaku (delta)

Kartu `PAY-BE-24` di roadmap masih memuat desain impor dan **tidak diubah** (wewenang roadmap hanya baris status). Delta:

| Kriteria kartu | Keadaan sekarang |
| --- | --- |
| 1. Daftar dari populasi | Tetap |
| 2. `isComplete` (profil aktif + gaji disetujui + rekening terverifikasi) | Diganti tiga dimensi (8.6); profil yang akan disiapkan otomatis tidak lagi menghalangi |
| 3. Ringkasan lima hitungan | Lima hitungan tetap, empat ditambahkan |
| 4. `POST /import` semua atau tidak sama sekali | **Dipensiunkan** (410). Digantikan penyiapan otomatis (8.5) |
| 5. Hak `Read` dan `Import` | `Read` tetap; `Import` hanya atribut pada endpoint yang dipensiunkan |
| 6. Log audit `PayrollDataReadiness.Import` | Diganti `PayrollProfile.AutoProvisioned`, tanpa data sensitif |

Amandemen kontrak (gerbang `G1`) harus memuat perubahan ini sebelum task dapat naik ke ✅.

### 8.12 Risiko, dependency, dan langkah berikutnya

| Hal | Isi |
| --- | --- |
| Kompilasi | Kode revisi ditulis tanpa `dotnet build`. Risiko kesalahan kompilasi tidak dapat dikesampingkan sebelum build dijalankan pemilik |
| Tetapan PPh dan iuran | Blocker khusus (8.8): tanpa kebijakan atau layar pengelolaan, seluruh profil otomatis berjalan dengan nol yang ditandai. Run yang dibayarkan sungguhan sebaiknya baru disahkan setelah ini diputuskan, karena PPh dan iuran akan tercatat nol |
| Balapan lintas periode | Satu dari dua permintaan dapat gagal dan perlu diulang (8.5); belum diuji |
| Simulasi | Tidak mencakup orang yang baru akan disiapkan (8.9) |
| Cost Center | Dapat kosong sampai HRD mengisinya di penempatan organisasi (8.8) |
| Hak akses | Atribut `Import` dipertahankan; admin tidak perlu mengubah apa pun |
| Perubahan sampingan | `NONE` |
| Status Git | Perubahan revisi belum di-stage dan belum di-commit |
| Langkah berikutnya | Pemilik: `dotnet build`, lalu uji API delapan langkah di atas; putuskan sumber tetapan PPh dan iuran; amandemen kontrak (`G1`). Status tetap 🟡 |
