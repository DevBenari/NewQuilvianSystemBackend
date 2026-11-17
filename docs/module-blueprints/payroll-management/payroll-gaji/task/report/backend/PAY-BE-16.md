# Laporan Perubahan Backend — `PAY-BE-16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-16` |
| Judul | Kelola baris tunjangan struktur |
| Slice | `MVP-1` — `EPIC PGJ-01` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-16` |
| Trace | FR-PGJ-001, 002, 003; PRL-DEC-02, 13; `../../../contracts/api-contract.md` grup Salary Structure Allowance; `../../../contracts/validation-matrix.md` bagian 1 (`SA-01`–`SA-07`); `../../../contracts/state-transition-matrix.md` bagian 2 |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | `PAY-BE-14` 🟡 — tabel `MstSalaryStructureAllowance` sudah diterapkan ke database oleh pemilik (2026-11-12) |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1 (9–20), berkas diubah 1 (4), logika bisnis 1, kontrak API 2 (endpoint baru), database 0 (tanpa perubahan skema), keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: service, controller, DTO baru dan `Program.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14` dan `PAY-BE-15` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai dan lolos pemeriksaan statis; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji API menunggu pemilik |

---

## 1. Masalah yang diperbaiki

Tabel tunjangan per struktur sudah ada sejak `PAY-BE-14`, tetapi belum ada cara mengisinya.
Petugas Payroll tidak bisa memasang Tunjangan Jabatan atau Tunjangan Keahlian pada Struktur Gaji,
sehingga mesin hitung (`PAY-BE-17`) belum punya data untuk dihitung.

## 2. Proses bisnis

**Tujuan.** Petugas Payroll mengatur tunjangan tetap per jenis untuk setiap Struktur Gaji
(PRL-DEC-02).

**Pelaku.** Petugas yang diberi hak `SalaryStructureAllowance` oleh admin di layar Akses Role.

**Pemicu dan prasyarat.** Struktur Gaji dan Jenis Tunjangan sudah ada dan aktif.

**Langkah:**

1. Petugas membuka daftar tunjangan sebuah struktur (`GET`).
2. Petugas menambah satu jenis tunjangan dengan satu cara hitung (`POST`):
   - **nominal tetap**, misalnya Tunjangan Jabatan Rp 750.000 per bulan; atau
   - **persentase gaji pokok**, misalnya Tunjangan Keahlian 5%.
3. Petugas mengubah cara hitung, nilai, catatan, atau status aktif (`PUT`) dengan menyertakan token
   versi terakhir yang ia lihat.
4. Petugas menghapus baris (`DELETE`). Baris hanya ditandai terhapus agar jejaknya tetap ada.

**Aturan:**

| Kode | Aturan | Contoh |
| --- | --- | --- |
| `SA-01` | Cara hitung hanya `FixedAmount` atau `PercentageOfBaseSalary` | `PerAttendance` ditolak 400 |
| `SA-02` | Nominal tetap wajib lebih dari 0 | Rp 0 ditolak 400 |
| `SA-03` | Persentase di atas 0 dan paling tinggi 100 | 150% ditolak 400 |
| `SA-04` | Satu jenis tunjangan sekali saja per struktur | Tunjangan Jabatan kedua ditolak 409 |
| `SA-05` | Struktur dan jenis tunjangan aktif saat menambah | Struktur nonaktif ditolak 400 |
| `SA-06` | Jenis tunjangan yang dipakai kebijakan Shift, Siaga, atau Risiko aktif tidak boleh dipasang | Tunjangan Shift Malam ditolak 400 |
| `SA-07` | Token versi harus sama dengan versi terbaru | Dua petugas mengubah baris yang sama: yang kedua ditolak 409 |

Kolom yang tidak dipakai cara hitung terpilih disimpan 0. **Contoh:** baris diubah dari nominal
Rp 750.000 menjadi 5%; kolom nominal menjadi 0, sehingga tidak ada angka lama yang menyesatkan.

**Jalur tidak normal:**

- **Dua petugas menambah jenis yang sama hampir bersamaan:** pemeriksaan aplikasi bisa lolos untuk
  keduanya. Index unik di database menolak salah satunya, dan penolakan itu dipetakan menjadi 409
  `SA-04`, bukan galat server.
- **Jenis tunjangan baris ini kemudian dinonaktifkan di master:** baris tetap bisa diubah selama
  jenisnya tidak diganti. Mengganti ke jenis lain hanya boleh ke jenis yang aktif.
- **Struktur atau baris tidak ditemukan:** 404.

**Hasil akhir.** Setiap struktur memuat daftar tunjangan tetap yang siap dibaca mesin hitung.
Belum ada perubahan pada hasil gaji sampai `PAY-BE-17` selesai.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PayrollControllerSupport.cs`, `PayrollVersionToken.cs`, `PayrollExceptions.cs`,
`PayrollRunDtos.cs` (eksepsi bentrok), `PayrollReportsController.cs` (pola controller V2),
`PayrollRestitutionService.cs` (pola logger), `Services/Logging/LoggerService.cs`,
`MstShiftAllowancePolicy.cs`, `MstOnCallAllowancePolicy.cs`, `MstHazardAllowancePolicy.cs`,
`MstAllowanceType.cs`, `ApplicationDbContext.cs` (nama DbSet),
`Services/Security/PermissionRegistryDescriptor.cs` (penemuan hak akses lewat refleksi),
`AccPostingRuleService.cs` (pola `PostgresException`), `SalaryStructureController.cs` (bentrok
rute), kontrak `PGJ-C1`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollSalaryStructureAllowanceDtos.cs` | **Baru.** `CreateSalaryStructureAllowanceRequest`, `UpdateSalaryStructureAllowanceRequest` (+`ExpectedRowVersion`), `SalaryStructureAllowanceResponse` |
| `Areas/Corporate/PayrollManagement/Services/PayrollSalaryStructureAllowanceService.cs` | **Baru.** List, create, update, delete (ditandai); `SA-01`–`SA-07`; pelanggaran index unik dipetakan ke 409; log audit tanpa nominal |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollSalaryStructureAllowancesController.cs` | **Baru.** Empat endpoint, `ControllerName = "SalaryStructureAllowance"`, tanpa akses `ApplicationDbContext` |
| `Program.cs` | `AddScoped<PayrollSalaryStructureAllowanceService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat endpoint baru sesuai `api-contract.md`; status berubah dari **Rencana (belum tersedia)** menjadi tersedia setelah build |
| Database | `NOT APPLICABLE` — memakai tabel dari `PAY-BE-14`, tanpa migration baru |
| Keamanan/Auth | Resource baru `SalaryStructureAllowance` dengan action `Read`, `Create`, `Update`, `Delete`; muncul otomatis di layar Akses Role. Admin harus memberikannya ke peran Payroll sebelum layar dipakai. Nominal tidak ditulis ke log |

## 4. Dokumentasi endpoint

#### Corporate / Payroll Management / Master Data / Salary Structure Allowance

Base URL: `api/v1/corporate/payroll-management/master-data/salary-structures/{salaryStructureId}/allowances`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Melihat tunjangan tetap satu struktur, urut nama jenis | `SalaryStructureAllowance : Read` |
| `POST` | `/` | Menambah satu jenis tunjangan | `SalaryStructureAllowance : Create` |
| `PUT` | `/{id}` | Mengubah cara hitung, nilai, catatan, atau status aktif | `SalaryStructureAllowance : Update` |
| `DELETE` | `/{id}?expectedRowVersion=…` | Menghapus baris (ditandai) | `SalaryStructureAllowance : Delete` |

Kode status: 200 berhasil; 400 isian melanggar `SA-01`, `SA-02`, `SA-03`, `SA-05`, atau `SA-06`;
403 tanpa hak; 404 struktur atau baris tidak ditemukan; 409 jenis ganda (`SA-04`) atau data sudah
diubah petugas lain (`SA-07`).

**Selisih bunyi pesan `SA-07`.** Kontrak menulis "Data ini baru saja diubah petugas lain. Muat
ulang lalu coba lagi." Implementasi memakai pembantu token versi bersama Payroll, sehingga bunyinya
"Baris tunjangan ini sudah diperbarui pihak lain. Muat ulang lalu coba lagi." Artinya sama, dan
kodenya tetap 409. Diusulkan kontrak disesuaikan pada revisi berikutnya.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Kecocokan atribut hak akses empat endpoint | Argumen `AccessPermission` = `ControllerName` dan = argumen `AccessAction`; tipe akses hanya `Read`/`Create`/`Update`/`Delete` | `PASS` | Skrip pemeriksa atribut |
| Bentrok rute dengan `SalaryStructureController` | Tidak ada; controller itu hanya punya `{id}/status` di bawah `{id}` | `PASS` | `grep` rute |
| `QBE-SVC-001`, `QBE-CODE-002/003`, `QBE-NAM-001` pada controller baru | 0 kecocokan | `PASS` | `grep` pola checker |
| `QBE-ENT-001`/`CFG-001`/`MOD-002` | Tidak berlaku: tidak ada entity baru pada task ini | `NOT APPLICABLE` | — |
| Pola tangkap `PostgresException` dan `PostgresErrorCodes.UniqueViolation` | Sama dengan pola yang sudah terkompilasi di `AccPostingRuleService.cs` | `PASS` | Pembandingan source |
| Keseimbangan kurung empat berkas | Seimbang | `PASS` | Skrip penghitung |
| Checker QBE resmi | Tidak dijalankan; agen tidak punya `pwsh` | `NOT RUN` | — |
| Uji API manual | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build dan pemberian hak akses.

**Langkah uji untuk pemilik** (Swagger, data uji samaran; beri hak `SalaryStructureAllowance`
lebih dulu):

1. `POST` jenis "Tunjangan Jabatan", `FixedAmount`, `amount` 750000. Harapan: 200, `rowVersion`
   terisi.
2. Ulangi langkah 1. Harapan: 409 `SA-04`.
3. `POST` "Tunjangan Keahlian", `PercentageOfBaseSalary`, `percentage` 150. Harapan: 400 `SA-03`.
4. `POST` yang sama dengan `percentage` 5. Harapan: 200, `amount` 0.
5. `POST` jenis yang dipakai kebijakan Shift aktif. Harapan: 400 `SA-06`.
6. `PUT` baris langkah 1 dengan `expectedRowVersion` usang. Harapan: 409.
7. `PUT` dengan `expectedRowVersion` terbaru, `amount` 800000. Harapan: 200.
8. `DELETE` baris langkah 4. Harapan: 200; `GET` tidak lagi memuatnya.
9. `GET` pada struktur yang tidak ada. Harapan: 404.

**Tidak dijalankan:** `dotnet build`, uji API, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Empat endpoint sesuai kontrak dengan `ControllerName = "SalaryStructureAllowance"` | Terpenuhi (statis) | Controller bagian 3.2; pemeriksa atribut |
| 2. `SA-01`–`SA-07` berlaku, termasuk tolak jenis ganda (409) dan jenis yang dipakai kebijakan Shift/Siaga/Risiko aktif (400) | Terpenuhi (statis); bunyi `SA-07` berselisih kecil | Service; bagian 4 |
| 3. Hapus hanya menandai `IsDelete` | Terpenuhi | `DeleteAsync` mengisi `IsDelete`, `DeleteDateTime`, `DeleteBy` |
| 4. Log aksi tulis tanpa nominal | Terpenuhi | Payload `AuditAsync` hanya ID, cara hitung, status, aktor |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan baris; checker resmi `NOT RUN` | Bagian 5 |
| DoD: kriteria terbukti | Terbukti statis; **bukti runtime belum** | Uji API menunggu |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `RowVersion` pada respons dihitung di sisi aplikasi pada proyeksi akhir query (`PayrollVersionToken.From`); EF Core mengizinkannya di proyeksi teratas. Bila build atau runtime menolaknya, ubah ke dua langkah (ambil tanggal lalu hitung) |
| Masalah yang diketahui | Selisih bunyi pesan `SA-07` (bagian 4) |
| Risiko tersisa | Tanpa hak akses baru, layar `PAY-FE-16` akan menerima 403 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Task ini: `??` tiga berkas baru (DTO, service, controller), `M` `Program.cs`; ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14` dan `PAY-BE-15` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: build, beri hak akses, jalankan sembilan langkah uji. Task yang kini terbuka: `PAY-BE-17` (hitung tunjangan dari struktur). `PAY-FE-16` menunggu `PAY-BE-15` dan `PAY-BE-16` ✅ |
