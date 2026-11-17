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
| Tanggal | 2026-11-16 |
| Status | 🟡 — source selesai dan sudah di-commit pemilik; **`dotnet build` berhasil menurut pemilik (2026-11-16, laporan lisan, tanpa keluaran tersimpan; bukan bukti agen)**; uji API belum; checker QBE resmi belum dijalankan; kontrak belum ada |

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
| Langkah berikutnya | Pemilik: jalankan uji API di atas; putuskan penulisan kontrak amandemen. Setelah itu `PAY-BE-24` dapat naik ke ✅ |
