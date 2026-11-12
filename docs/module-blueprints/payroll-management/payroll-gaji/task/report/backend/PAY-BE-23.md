# Laporan Perubahan Backend — `PAY-BE-23`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-23` |
| Judul | Rekap lembur per run |
| Slice | `MVP-1` — `EPIC PGJ-07` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-23` |
| Trace | FR-PGJ-060; PRL-DEC-42; `../../../contracts/api-contract.md` grup Payroll Run — tambahan (`GET /{id}/overtime-inputs`, `PayrollRunOvertimeInputQuery`, `PayrollRunOvertimeInputResponse`) |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | — |
| Klasifikasi | `LOW` — skor 3: repository 0, berkas diperiksa 0, berkas diubah 0 (3 berkas), logika bisnis 0 (murni baca), kontrak API 2 (satu endpoint baru), database 0, keamanan 1 (nominal lembur), UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `PayrollRunService.cs`, `PayrollRunsController.cs`, `PayrollRunDtos.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14`–`22` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai; tiga kriteria terbukti statis; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji API menunggu pemilik |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`); `PrlPayrollOvertimeInput` sudah ada |
| Keberlakuan | `TOUCHED LEGACY` — service, controller, dan DTO run yang sudah ada |
| QBE yang berlaku | `QBE-SVC-001` (akses data hanya di service), `QBE-NAM-001`; sub-resource read-only (standar transaksi) |
| Hak akses | `PayrollRun : Read` yang sudah ada (kontrak); tidak ada action baru. Tanpa `IsInRole`, nama peran, atau `UserType` |
| Hasil | Tidak ada pelanggaran pada diff; checker resmi `NOT RUN` (tanpa `pwsh`) |

## 1. Masalah yang diperbaiki

Lembur terverifikasi dari HR sudah diimpor ke run sebagai `PrlPayrollOvertimeInput` dan dihitung
menjadi komponen "Lembur". Tetapi petugas Payroll hanya melihat satu angka total per pegawai,
tidak bisa melihat rincian per tanggal yang membentuknya (PRL-DEC-42: lembur di Payroll baca-saja).

## 2. Proses bisnis

**Tujuan.** Petugas Payroll memeriksa lembur yang ikut dihitung di sebuah run, per pegawai per
tanggal, tanpa bisa mengubahnya. Perubahan lembur tetap di modul HR.

**Langkah:**

1. Petugas membuka run, lalu tab atau tautan rekap lembur.
2. Sistem menampilkan baris lembur yang diimpor ke run: nomor dan nama pegawai, tanggal, menit
   terverifikasi, pengali tarif, tarif per jam, dan nominal.
3. Petugas dapat mencari menurut nama atau nomor pegawai, dan berpindah halaman.

**Aturan baris:** hanya baris yang aktif dan tidak terhapus, milik pegawai run yang tidak terhapus.
Saringan ini sama dengan yang dipakai mesin hitung, sehingga jumlah `overtimeAmount` per pegawai
sama dengan komponen "Lembur" pegawai itu.

**Contoh** (data samaran): pegawai A lembur 2 kali di run Mei, yaitu 12 Mei 120 menit ×1,5 ×
Rp 40.000 = Rp 120.000 dan 19 Mei 60 menit ×2 × Rp 40.000 = Rp 80.000. Rekap menampilkan dua
baris berurutan menurut tanggal; komponen "Lembur" pegawai A = Rp 200.000.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Run tidak ada atau terhapus | 404 "Payroll run tidak ditemukan." |
| Run tanpa lembur | 200, daftar kosong |
| Pengguna tanpa `PayrollRun : Read` | 403 |
| `pageSize` lebih dari 200 | Dibatasi 200 |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PrlPayrollOvertimeInput.cs`, `PayrollCalculationService.cs` (saringan lembur yang dihitung),
`PayrollRunService.cs` (`GetStatusHistoryAsync` sebagai pola sub-resource baca), `PayrollRunsController.cs`,
`PayrollRunDtos.cs`, `ApplicationDbContext.cs`, `PagedResult.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollRunService.cs` | `GetOvertimeInputsAsync`: periksa run, saring, cari, urut (nama, tanggal), paginasi (maks 200) |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollRunsController.cs` | `GET /{id}/overtime-inputs` (`PayrollRun : Read`) |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollRunDtos.cs` | `PayrollRunOvertimeInputQuery`, `PayrollRunOvertimeInputResponse` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru; endpoint lama tidak berubah |
| Database | Tidak ada perubahan skema. Murni baca |
| Keamanan/Auth | Memakai `PayrollRun : Read` sesuai kontrak. **Temuan:** respons memuat nominal lembur per pegawai, sedangkan detail pegawai run (`GET /{id}/employees/{runEmployeeId}`) dijaga action yang lebih sempit, `PayrollRun : ReadEmployeeDetail`. Pengguna yang hanya boleh melihat daftar run kini dapat melihat nominal lembur per pegawai |

## 4. Dokumentasi endpoint

### `[Tags("Corporate / Payroll Management / Payroll Run")]`

Base URL: `api/v1/corporate/payroll-management/payroll-runs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode |
|---|---|---|---|---|---|---|
| `GET` | `/{id}/overtime-inputs` | Rekap lembur terverifikasi yang diimpor ke run | `PayrollRun : Read` | query `search`, `pageNumber`, `pageSize` | `PagedResult<PayrollRunOvertimeInputResponse>` | 200, 403, 404 |

**Contoh respons** (data samaran):

```json
{
  "pageNumber": 1,
  "pageSize": 25,
  "totalData": 2,
  "totalPage": 1,
  "items": [
    { "id": "…", "payrollRunEmployeeId": "…", "employeeNumber": "EMP-0001", "employeeName": "Pegawai Contoh A",
      "overtimeDate": "2026-05-12", "verifiedMinutes": 120, "rateMultiplier": 1.5, "hourlyRate": 40000.00, "overtimeAmount": 120000.00 },
    { "id": "…", "payrollRunEmployeeId": "…", "employeeNumber": "EMP-0001", "employeeName": "Pegawai Contoh A",
      "overtimeDate": "2026-05-19", "verifiedMinutes": 60, "rateMultiplier": 2.0, "hourlyRate": 40000.00, "overtimeAmount": 80000.00 }
  ]
}
```

**Delta terhadap kontrak `PGJ-C1`:** respons menambah `id` dan `payrollRunEmployeeId`, supaya
layar dapat menautkan baris ke detail pegawai run. Field kontrak lain tetap sama.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Kriteria 1: baris sama dengan lembur yang diimpor, dengan paginasi dan pencarian | Saringan sama dengan mesin hitung (`!IsDelete && IsActive`); pencarian nama/nomor; paginasi maks 200 | `PASS` (statis) | Review source |
| Kriteria 2: hak `PayrollRun : Read` | Atribut cocok; 22/22 endpoint controller run konsisten; satu nama tampilan untuk action `Read` | `PASS` | Skrip pemeriksa |
| Kriteria 3: run tanpa lembur | Query kosong → `items` kosong, `totalData` 0 | `PASS` (statis) | Review source |
| DbSet dan bentrok rute | `PrlPayrollOvertimeInputs` ada; satu-satunya rute `overtime-inputs` | `PASS` | `grep` |
| Keseimbangan kurung | 0 selisih di 3 berkas | `PASS` | Skrip penghitung |
| Uji API | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build.

**Langkah uji untuk pemilik** (DB uji, data samaran):

1. Pada run yang sudah mengimpor lembur, `GET /payroll-runs/{id}/overtime-inputs`. Harapan: jumlah
   `overtimeAmount` per pegawai = komponen "Lembur" pegawai itu di detail pegawai run.
2. `search` dengan sebagian nama pegawai. Harapan: hanya baris pegawai itu.
3. `pageSize=1`. Harapan: satu baris per halaman, `totalPage` = jumlah baris.
4. Run tanpa lembur. Harapan: daftar kosong.
5. Id run acak. Harapan: 404.
6. Akun tanpa `PayrollRun : Read`. Harapan: 403.

**Tidak dijalankan:** `dotnet build`, uji API, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Baris sama dengan lembur yang diimpor, dengan paginasi dan pencarian | Terpenuhi (statis); dibandingkan di DB uji menunggu | Bagian 5 |
| 2. Hak `PayrollRun : Read` | Terpenuhi (statis) | Bagian 5 |
| 3. Run tanpa lembur: daftar kosong | Terpenuhi (statis) | Bagian 5 |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan yang dicek; checker resmi `NOT RUN` | Bagian 5 |
| DoD: kriteria terbukti | Lewat statis; uji API **belum** | Langkah uji menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan teknis | Saringan sengaja disamakan dengan mesin hitung, bukan menampilkan semua baris impor, supaya rekap selalu cocok dengan angka "Lembur" di slip |
| **Perlu keputusan pemilik** | Hak akses endpoint ini mengikuti kontrak (`PayrollRun : Read`), tetapi lebih longgar daripada detail pegawai run (`PayrollRun : ReadEmployeeDetail`). Pilihan: (a) tetap `Read` sesuai kontrak; (b) ganti ke `ReadEmployeeDetail`, yaitu amandemen kontrak satu baris dan satu atribut. Sampai diputuskan, admin dapat membatasi lewat Akses Role dengan tidak memberi `PayrollRun : Read` kepada peran yang tidak boleh melihat nominal |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Task ini: `M` `PayrollRunService.cs` (bersama `PAY-BE-20`), `PayrollRunsController.cs` (bersama `PAY-BE-21`), `PayrollRunDtos.cs`; ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14`–`22` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: build seluruh perubahan `PAY-BE-14`–`23` dan langkah uji tiap laporan. Ini task backend terakhir di roadmap; berikutnya task frontend `PAY-FE-14`–`20` |
