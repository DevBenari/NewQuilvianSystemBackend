# Laporan Perubahan Backend — `PAY-BE-15`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-15` |
| Judul | Batas struktur di dalam grade |
| Slice | `MVP-0` — `EPIC PGJ-02` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-15` |
| Trace | FR-PGJ-010, 011; PRL-DEC-12; `../../../contracts/validation-matrix.md` bagian 2 (`SR-01`–`SR-04`); `../../../contracts/api-contract.md` bagian 2 "Master struktur dan grade — validasi tambahan" |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1 (9–20), berkas diubah 1 (4), logika bisnis 1 (sedang), kontrak API 1 (perilaku endpoint yang ada bertambah penolakan), database 0, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: satu service baru, dua controller master (legacy), `Program.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai dan lolos pemeriksaan statis; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji API manual menunggu pemilik |

---

## 1. Masalah yang diperbaiki

Rentang gaji pada Grade Gaji tidak membatasi apa pun. Struktur Gaji dapat disimpan dengan batas
atas melebihi grade-nya, dan grade dapat dipersempit walau strukturnya sudah keluar dari rentang
baru.

**Contoh sebelum perubahan:** Grade GR-05 berbatas Rp 6.000.000 sampai Rp 9.000.000. Struktur
"GR-05 Cabang A" tersimpan dengan batas atas Rp 10.000.000, tanpa penolakan. Plafon grade menjadi
tidak berarti.

## 2. Proses bisnis

**Tujuan.** Batas gaji struktur selalu berada di dalam rentang grade-nya (PRL-DEC-12).

**Pelaku.** Petugas Payroll menyimpan Struktur Gaji (`SalaryStructure : Create` / `Update`).
Petugas HRD atau Payroll mengubah Grade Gaji (`SalaryGrade : Update`).

**Langkah: menyimpan Struktur Gaji (tambah atau ubah)**

1. Petugas mengisi grade, batas bawah, batas atas, dan gaji bawaan struktur.
2. Validasi lama berjalan lebih dulu, tanpa perubahan: grade wajib ada dan aktif, gaji bawaan di
   dalam batas struktur sendiri, nama tidak ganda.
3. Validasi baru memeriksa terhadap grade:
   - batas bawah struktur tidak lebih rendah dari batas bawah grade (`SR-01`);
   - batas atas struktur tidak melebihi batas atas grade (`SR-02`);
   - gaji bawaan berada di dalam rentang gabungan grade dan struktur (`SR-03`).
4. Bila lolos, struktur tersimpan seperti biasa.

**Langkah: mengubah Grade Gaji**

1. Petugas mengubah rentang grade.
2. Validasi lama berjalan (minimum ≤ tengah ≤ maksimum, dan lainnya).
3. Validasi baru mencari struktur **aktif** di bawah grade itu yang akan keluar dari rentang baru.
   Bila ada, perubahan ditolak dengan daftar kode strukturnya (`SR-04`).

**Contoh berangka:**

| Kasus | Hasil |
| --- | --- |
| Grade Rp 6.000.000–9.000.000; struktur batas atas Rp 10.000.000 | Ditolak 400: "Batas atas gaji struktur (Rp 10.000.000) melebihi batas atas grade GR-05 (Rp 9.000.000)." |
| Grade Rp 6.000.000–9.000.000; struktur batas bawah Rp 5.500.000 | Ditolak 400 `SR-01` |
| Grade Rp 6.000.000–9.000.000; struktur tanpa batas, gaji bawaan Rp 9.500.000 | Ditolak 400: "Gaji pokok bawaan Rp 9.500.000 berada di luar rentang Rp 6.000.000 – Rp 9.000.000." |
| Grade Rp 6.000.000–9.000.000; struktur Rp 6.500.000–8.500.000, bawaan Rp 7.000.000 | Tersimpan |
| Grade dipersempit ke Rp 6.000.000–8.500.000; ada struktur aktif berbatas atas Rp 9.000.000 | Ditolak 409: "Rentang baru membuat 1 struktur aktif keluar batas: SST-RSMMC-…. Sesuaikan struktur dulu." |

**Jalur tidak normal.**

- **Grade yang rentangnya belum diisi** (batas atas 0, nilai bawaan kolom) tidak membatasi apa pun.
  Tanpa pengecualian ini, setiap struktur di bawah grade yang belum diisi rentangnya akan ditolak.
  Ini tafsiran teknis atas nilai bawaan, bukan aturan bisnis baru (lihat bagian 7).
- **Struktur yang sudah melanggar sebelum task ini** tidak diubah. Ia baru ditolak saat disimpan
  ulang, dan ikut tercantum bila grade-nya diubah. Pemilik dapat menemukannya lebih dulu dengan SQL
  pra-cek di `../../../02-backend-architecture.md` bagian 8.
- **Lebih dari 10 struktur terdampak `SR-04`**: pesan menyebut 10 kode pertama ditambah
  "dan n lainnya".

**Hasil akhir.** Tidak ada Struktur Gaji baru atau yang diubah yang berada di luar rentang
grade-nya, dan rentang grade tidak dapat dipersempit diam-diam.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`SalaryStructureController.cs` (create, update, status, `ValidateRequestAsync`),
`SalaryGradeController.cs` (update, `ValidateRequestAsync`), DTO struktur dan grade,
`MstSalaryGrade.cs`, `MstSalaryStructure.cs`, `PayrollReportService.cs` (pola service),
`Program.cs` (pendaftaran service Payroll), `Responses/ApiResponse.cs`,
`contracts/validation-matrix.md`, `contracts/api-contract.md`, kartu roadmap `PAY-BE-15`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollSalaryRangeRuleService.cs` | **Baru.** `ValidateStructureAsync` (`SR-01`–`SR-03`) dan `ValidateGradeChangeAsync` (`SR-04`); hanya membaca (`AsNoTracking`); hasil berupa `PayrollSalaryRangeCheck` (status 400 atau 409 beserta pesan); format rupiah `id-ID` |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/SalaryStructureController.cs` | Inject service; panggil `ValidateStructureAsync` sesudah validasi lama pada `POST /` dan `PUT /{id}`; tolak 400 |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/SalaryGradeController.cs` | Inject service; panggil `ValidateGradeChangeAsync` sesudah validasi lama pada `PUT /{id}`; tolak 409 |
| `Program.cs` | `AddScoped<PayrollSalaryRangeRuleService>()` di blok layanan Payroll |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request dan response tidak berubah. Penolakan baru: 400 pada simpan struktur, 409 pada ubah grade, sesuai `api-contract.md` bagian 2 |
| Database | `NOT APPLICABLE` — tidak ada perubahan skema atau migration |
| Keamanan/Auth | `NOT APPLICABLE` — atribut hak akses kedua controller tidak berubah |

## 4. Dokumentasi endpoint

#### Corporate / Payroll Management / Master Data / Salary Structure

Base URL: `api/v1/corporate/payroll-management/master-data/salary-structures` (alias lama
`api/v1/corporate/human-resource/master-data/salary-structures` ikut terdampak)

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat struktur; kini ditolak 400 bila batas atau gaji bawaan keluar rentang grade | `SalaryStructure : Create` |
| `PUT` | `/{id}` | Mengubah struktur; penolakan yang sama | `SalaryStructure : Update` |

#### Corporate / Payroll Management / Master Data / Salary Grade

Base URL: `api/v1/corporate/payroll-management/master-data/salary-grades`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/{id}` | Mengubah grade; kini ditolak 409 bila struktur aktif keluar dari rentang baru | `SalaryGrade : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Kode lain yang membuat kedua controller secara manual (konstruktor berubah) | Tidak ada | `PASS` | `grep "new SalaryStructureController(\|new SalaryGradeController("` |
| `QBE-SVC-001`, `QBE-CODE-002/003`, `QBE-NAM-001` pada baris baru controller | 0 kecocokan `ApplicationDbContext`, `Generate*Code/Number`, Count/Max+1, `Trx` | `PASS` | `git diff -U0` disaring pola checker |
| Akses data hanya di service | Query grade dan struktur berada di `PayrollSalaryRangeRuleService`, bukan di controller | `PASS` | Review diff |
| Keseimbangan kurung empat berkas | Seimbang | `PASS` | Skrip penghitung |
| Pesan sesuai `validation-matrix.md` bagian 2 | `SR-01`–`SR-04` sama bunyinya, nilai rupiah terisi | `PASS` | Review diff |
| Checker QBE resmi | Tidak dijalankan; agen tidak punya `pwsh` | `NOT RUN` | — |
| Uji API manual lewat Swagger | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build.

**Langkah uji untuk pemilik** (Swagger, data uji samaran):

1. Buat atau pilih grade dengan rentang Rp 6.000.000–9.000.000.
2. `POST` struktur pada grade itu dengan `maximumBaseSalary` 10000000. Harapan: 400 `SR-02`.
3. `POST` struktur dengan `minimumBaseSalary` 6500000, `maximumBaseSalary` 8500000,
   `defaultBaseSalary` 7000000. Harapan: 200.
4. `PUT` struktur langkah 3 dengan `defaultBaseSalary` 9500000 dan batas dikosongkan. Harapan: 400
   `SR-03`.
5. `PUT` grade dengan `maximumSalary` 8000000 (dan tengah ≤ 8.000.000). Harapan: 409 `SR-04`
   menyebut kode struktur langkah 3.
6. `PUT` grade dengan `maximumSalary` 9000000. Harapan: 200.

**Tidak dijalankan:** `dotnet build`, uji API, dan checker QBE resmi. Build dilarang instruksi
pemilik pada task ini; uji API membutuhkan aplikasi berjalan.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Grade Rp 6.000.000–9.000.000, struktur batas atas Rp 10.000.000 ditolak 400 `SR-02` | Terpenuhi (statis) | `ValidateStructureAsync` cabang batas atas; uji API menunggu |
| 2. Grade dipersempit ke Rp 8.500.000 saat ada struktur aktif berbatas Rp 9.000.000 ditolak 409 `SR-04` dengan kode struktur | Terpenuhi (statis) | `ValidateGradeChangeAsync`; `Conflict(...)` di `SalaryGradeController` |
| 3. Simpan di dalam rentang tetap berhasil | Terpenuhi (statis) | Service mengembalikan `Valid`; alur simpan lama tidak berubah |
| 4. Tidak ada baris akses `ApplicationDbContext` baru di controller (`QBE-SVC-001`) | Terpenuhi | Bagian 5 |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan baris yang dicek; checker resmi `NOT RUN` | Bagian 5 |
| DoD: kriteria terbukti | Terbukti statis; **bukti runtime belum** | Uji API menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Keputusan teknis: grade dengan batas atas 0 dianggap belum punya rentang dan tidak membatasi struktur. Bila pemilik ingin grade tanpa rentang justru menolak semua struktur, aturan ini diubah |
| Masalah yang diketahui | Mengaktifkan kembali struktur lewat `PATCH /{id}/status` tidak diperiksa terhadap grade, karena kontrak `PGJ-C1` hanya mewajibkan pemeriksaan pada simpan struktur dan ubah grade. Struktur nonaktif yang melanggar dapat diaktifkan kembali tanpa penolakan; diusulkan masuk revisi kontrak berikutnya |
| Risiko tersisa | Struktur lama yang sudah melanggar baru ketahuan saat diubah; jalankan SQL pra-cek lebih dulu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan task ini: `M` `SalaryStructureController.cs`, `SalaryGradeController.cs`, `Program.cs`; `??` `PayrollSalaryRangeRuleService.cs`; ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14` yang belum di-commit masih ada di working tree. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` dan enam langkah uji di bagian 5, lalu tanda task naik ke ✅. Task berikutnya yang terbuka: `PAY-BE-16`, `PAY-BE-19`, `PAY-BE-22`, `PAY-BE-23`. Layar frontend `PAY-FE-16` menunggu `PAY-BE-15` dan `PAY-BE-16` ✅ |
