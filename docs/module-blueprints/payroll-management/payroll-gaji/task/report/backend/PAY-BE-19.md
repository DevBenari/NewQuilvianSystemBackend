# Laporan Perubahan Backend — `PAY-BE-19`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-19` |
| Judul | Input variabel manual |
| Slice | `MVP-1` — `EPIC PGJ-04` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-19` |
| Trace | FR-PGJ-030, 031, 032, 034, 035; PRL-DEC-07, 10, 31, 38, 43; `../../../contracts/api-contract.md` grup Payroll Variable Input; `../../../contracts/state-transition-matrix.md` bagian 1; `../../../contracts/validation-matrix.md` bagian 3; `../../../contracts/permission-audit-matrix.md` |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | `PAY-BE-14` 🟡 (kolom keputusan, konstanta jenis input, sumber `PayrollManualInput`; migration sudah diterapkan pemilik) |
| Klasifikasi | `HIGH` — skor 8: repository 0, berkas diperiksa 2, berkas diubah 1 (3 baru + 2 diubah), logika bisnis 2 (siklus status, impor atomik, aksi massal), kontrak API 2 (11 endpoint baru), database 0 (tanpa skema baru), keamanan 1 (8 hak akses baru), UI 0 |
| Task mode | `BACKEND` |
| Target tulis | Tiga berkas baru (service, controller, DTO); `PayrollManualInputTypes.cs`; `Program.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14`–`18` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai; atribut hak akses 11/11 konsisten; matriks transisi dan contoh impor terbukti lewat simulasi; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji API menunggu pemilik |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`); entity yang dipakai `PrlPayrollVariableInput` sudah ada |
| Keberlakuan | `NEW CODE` (service, controller, DTO baru); tidak ada entity atau migration baru |
| QBE yang berlaku | `QBE-SVC-001` (akses data hanya di service), `QBE-CODE-002`/`003`/`006` (nomor dari `NumberSeriesAllocator`, bukan Count/Max+1), `QBE-NAM-001` (nama class), standar endpoint transaksi (aksi `POST /{id}/<aksi>`, suntingan `PATCH /{id}/content`, tanpa `DELETE`/`options`/`status` generik) |
| Hak akses | `[AccessController]` `ControllerName = "PayrollVariableInput"`; 8 action; tanpa `IsInRole`, nama peran, atau `UserType` |
| Hasil | Tidak ada pelanggaran pada diff; checker resmi `NOT RUN` (tanpa `pwsh`) |

## 1. Masalah yang diperbaiki

Run gaji belum punya jalan resmi untuk memasukkan insentif bulanan, bonus, dan honor tugas
tambahan. Tabel `PrlPayrollVariableInput` sudah ada, tetapi isinya hanya dari sumber otomatis
(jasa medis, cicilan piutang, data cuti). Petugas tidak bisa membuat, mengimpor, mengajukan, atau
menyetujui penghasilan variabel sendiri.

## 2. Proses bisnis

**Tujuan.** Petugas Payroll mencatat penghasilan variabel per pegawai di dalam satu run, lalu
memutuskannya lewat siklus ajukan–setujui sebelum run dihitung (PRL-DEC-07).

**Pelaku.** Petugas Payroll, dengan hak `PayrollVariableInput` yang diberikan admin di Akses Role.
Pengaju boleh menyetujui inputnya sendiri (PRL-DEC-10, 43).

**Siklus status** (`state-transition-matrix.md` bagian 1):

| Dari | Aksi | Ke | Catatan |
| --- | --- | --- | --- |
| — | Buat / impor | `Draft` | Nomor `PVI-yyyyMM-nnnn` dari number series |
| `Draft` | Ubah isi | `Draft` | Token versi diperiksa |
| `Draft` | Ajukan | `Submitted` | Isi diperiksa ulang (komponen bisa sudah dinonaktifkan) |
| `Submitted` | Setujui | `Approved` | Pengaju = penyetuju sah |
| `Submitted` | Tolak | `Rejected` | Alasan wajib; final |
| `Draft`, `Submitted`, `Approved` | Batalkan | `Cancelled` | Alasan wajib; final |
| `Approved` | Run disahkan | `Applied` | **Bukan task ini** — `PAY-BE-20` |

Aksi ke status yang sudah berlaku (misalnya menyetujui input `Approved`) dikembalikan apa adanya
tanpa perubahan. Seluruh aksi petugas hanya boleh selama run berstatus `Draft`, `CollectingInput`,
atau `Calculated`.

**Contoh berangka** (kontrak): petugas mengimpor 120 insentif Mei. Baris 17 menyebut pegawai yang
tidak ada di run dan baris 64 bernominal 0. Seluruh impor ditolak; tidak ada baris yang tersimpan
dan tidak ada nomor yang terbit. Setelah dua baris diperbaiki, 120 baris masuk sebagai `Draft`.
Petugas mengajukan semuanya (`bulk-submit`), menyetujui 118 (`bulk-approve`), dan menolak 2 dengan
alasan "nominal salah".

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Run sudah diajukan atau lebih jauh | 400 `VI-01` "Run ini sudah tidak menerima penghasilan variabel." |
| Pegawai tidak ada di run | 400 `VI-02` |
| Komponen nonaktif, terhapus, atau bukan `Earning` | 400 `VI-03` |
| Nominal ≤ 0 | 400 `VI-04` |
| Jenis bukan Insentif, Bonus, atau Honor Tugas Tambahan | 400 `VI-05` |
| Honor tugas tambahan tanpa catatan tugas | 400 `VI-06` |
| Tolak atau batalkan tanpa alasan | 400 `VI-07` |
| Impor dengan satu atau lebih baris salah | 400 `VI-08`, daftar baris dan sebabnya di `errors` |
| Transisi tidak sah (misalnya menyetujui `Draft`, mengubah `Approved`) | 409 `VI-09` "Input berstatus {status} tidak dapat {aksi}." |
| Token versi tidak cocok | 409 `VI-12` "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi." |
| Aksi massal, sebagian input gagal | 200; input yang sah tetap diproses; yang gagal dilaporkan per input |
| Nomor gagal diterbitkan | 409 dengan sebab dari number series |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PrlPayrollVariableInput.cs` dan configuration-nya (index unik `InputNumber`), `PayrollStatusCatalog.cs`,
`PayrollManualInputTypes.cs`, `PayrollRunStatuses.cs` (`AcceptsInputs`), `PrlPayrollRunEmployee.cs`,
`PrlPayrollRun.cs`, `MstPayrollComponent.cs`, `NumberSeriesAllocator.cs` dan `NumberAllocationRequest.cs`,
`FinancePayrollDeductionBatchService.cs` (pola alokasi nomor), `PayrollMedicalFeeIntakeService.cs`
(pola pembuatan input), `PayrollHrdImportService.cs` (memastikan impor HRD tidak menyentuh input
manual), `PayrollCalculationService.cs` (status yang dihitung), `PayrollControllerSupport.cs`,
`PayrollVersionToken.cs`, `ApiResponse.cs`, `PagedResult.cs`, `PermissionRegistryDescriptor.cs`
(perlakuan action berulang), `PayrollSalaryStructureAllowanceService.cs` dan controller-nya (pola
`PAY-BE-16`).

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollVariableInputService.cs` | Baru | Daftar dan detail; buat; impor atomik; ubah isi; ajukan, setujui, tolak, batalkan; aksi massal; validasi `VI-01`–`VI-09` dan `VI-12`; alokasi nomor `PVI`; log tanpa nominal. 636 baris |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollVariableInputsController.cs` | Baru | 11 endpoint; impor menangkap `VI-08` agar daftar baris ikut di `errors`. 139 baris |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollVariableInputDtos.cs` | Baru | 13 DTO kontrak dan `PayrollVariableInputImportException`. 133 baris |
| `Areas/Corporate/PayrollManagement/Constants/PayrollManualInputTypes.cs` | Diperbarui | Kelas `PayrollManualInputStatuses` (enam status), supaya `PAY-BE-20` memakai nilai yang sama |
| `Program.cs` | Diperbarui | `AddScoped<PayrollVariableInputService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 11 endpoint baru sesuai kontrak (bagian 4). Bentuk endpoint lama tidak berubah |
| Database | Tidak ada perubahan skema. Baris baru di `PrlPayrollVariableInput` dengan `SourceType = PayrollManualInput`; baris deret `PRL_VARIABLE_INPUT` dibuat otomatis oleh alokator pada nomor pertama tiap bulan |
| Keamanan/Auth | 8 hak akses baru di modul `CORPORATE_PAYROLL_VARIABLE_INPUT`. Admin harus memberikannya di Akses Role sebelum layar dapat dipakai |

## 4. Dokumentasi endpoint

### `[Tags("Corporate / Payroll Management / Payroll Variable Input")]`

Base URL: `api/v1/corporate/payroll-management/payroll-runs/{runId}/variable-inputs`

| Method | Path | Kegunaan | Hak akses (`AccessType`) | Request | Response | Kode |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar input manual di run | `PayrollVariableInput : Read` (Read) | query `PayrollVariableInputQuery` | `PagedResult<PayrollVariableInputResponse>` | 200, 403, 404 |
| `GET` | `/{id}` | Detail satu input | `PayrollVariableInput : Read` (Read) | — | `PayrollVariableInputResponse` | 200, 403, 404 |
| `POST` | `/` | Buat satu input `Draft` | `PayrollVariableInput : Create` (Create) | `CreatePayrollVariableInputRequest` | `PayrollVariableInputResponse` | 200, 400, 403, 404, 409 |
| `POST` | `/import` | Impor banyak baris, semua atau tidak sama sekali | `PayrollVariableInput : Import` (Create) | `ImportPayrollVariableInputRequest` | `PayrollVariableInputImportResult` | 200, 400 (`VI-08`), 403, 404 |
| `PATCH` | `/{id}/content` | Ubah isi input `Draft` | `PayrollVariableInput : Update` (Update) | `UpdatePayrollVariableInputContentRequest` | `PayrollVariableInputResponse` | 200, 400, 403, 404, 409 |
| `POST` | `/{id}/submit` | Ajukan | `PayrollVariableInput : Submit` (Update) | `PayrollVariableInputActionRequest` | `PayrollVariableInputResponse` | 200, 400, 403, 404, 409 |
| `POST` | `/{id}/approve` | Setujui | `PayrollVariableInput : Approve` (Update) | `PayrollVariableInputActionRequest` | `PayrollVariableInputResponse` | 200, 400, 403, 404, 409 |
| `POST` | `/{id}/reject` | Tolak dengan alasan | `PayrollVariableInput : Reject` (Update) | `PayrollVariableInputDecisionRequest` | `PayrollVariableInputResponse` | 200, 400, 403, 404, 409 |
| `POST` | `/{id}/cancel` | Batalkan dengan alasan | `PayrollVariableInput : Cancel` (Update) | `PayrollVariableInputDecisionRequest` | `PayrollVariableInputResponse` | 200, 400, 403, 404, 409 |
| `POST` | `/bulk-submit` | Ajukan banyak input | `PayrollVariableInput : Submit` (Update) | `PayrollVariableInputBulkRequest` | `PayrollVariableInputBulkResult` | 200, 400, 403, 404 |
| `POST` | `/bulk-approve` | Setujui banyak input | `PayrollVariableInput : Approve` (Update) | `PayrollVariableInputBulkRequest` | `PayrollVariableInputBulkResult` | 200, 400, 403, 404 |

**Contoh impor gagal** (400):

```json
{
  "success": false,
  "statusCode": 400,
  "message": "Impor dibatalkan. 2 baris bermasalah; tidak ada baris yang tersimpan.",
  "data": null,
  "errors": {
    "createdCount": 0,
    "failedRows": [
      { "rowNumber": 17, "reason": "Pegawai EMP-0999 tidak ada di run periode ini." },
      { "rowNumber": 64, "reason": "Nominal harus lebih dari nol." }
    ]
  }
}
```

**Delta terhadap kontrak `PGJ-C1`** (penambahan, bukan perubahan bentuk):

| Delta | Alasan |
| --- | --- |
| `PayrollVariableInputResponse` menambah `payrollComponentId` dan `attachmentPath` | Form ubah isi membutuhkan id komponen dan lampiran yang sedang tersimpan; kontrak hanya memuat kode dan nama |
| Daftar baris gagal impor dikirim di `errors` respons 400 | Kontrak menyebut `VI-08` "beserta daftar baris dan sebabnya" tanpa menetapkan letaknya; `errors` adalah tempat baku `ApiResponse` untuk rincian galat |
| Run di luar status penerima input: 400 `VI-01` | `validation-matrix.md` menulis 400, sedangkan baris terakhir tabel transisi tidak sah di `state-transition-matrix.md` menggolongkannya ke 409 `VI-09`. Aturan yang lebih khusus (`VI-01`) dipakai; selisih ini perlu diputuskan pemilik kontrak |
| Pesan `VI-09` memakai label status Indonesia (Draf, Diajukan, Disetujui, Ditolak, Dibatalkan, Diterapkan) | Kontrak menulis `{status}` tanpa menetapkan bahasanya |
| `expectedRowVersion` kosong berarti tanpa pemeriksaan versi | Sama dengan `PayrollVersionToken.EnsureMatches` di endpoint V2 lain |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Kecocokan atribut hak akses per endpoint (`ControllerName` = argumen 1 `[AccessPermission]`; argumen 2 = argumen 1 `[AccessAction]`; `AccessType` sesuai matriks) | 11/11 cocok | `PASS` | Skrip pemeriksa |
| Action berulang (`Read`, `Submit`, `Approve` di dua endpoint) | Registry hanya mendaftarkan sekali (`actionSeen.Add`) | `PASS` | `PermissionRegistryDescriptor.cs` baris 497–520 |
| Matriks transisi: 6 status × 4 aksi pada run `Calculated` | Sesuai matriks, 0 selisih; idempoten untuk status tujuan yang sama | `PASS` | Skrip simulasi |
| Run `Submitted` lalu aksi petugas | 400 `VI-01` | `PASS` | Skrip simulasi |
| Contoh impor 120 baris dengan dua baris salah | Ditolak, 2 baris gagal, 0 tersimpan | `PASS` | Skrip simulasi |
| `VI-03` komponen potongan, `VI-06` honor tanpa alasan | Ditolak dengan pesan kontrak | `PASS` | Skrip simulasi |
| Bunyi pesan `VI-01`–`VI-09`, `VI-12` | Sama dengan `validation-matrix.md` bagian 3 | `PASS` | Review source |
| Keseimbangan kurung | 0 selisih di 4 berkas | `PASS` | Skrip penghitung |
| Bentrok rute | Tidak ada rute lain di `.../payroll-runs/{runId}/variable-inputs` | `PASS` | `grep` |
| QBE di controller | 0 akses `ApplicationDbContext`, 0 Count/Max+1 | `PASS` | `grep` |
| Impor HRD tidak menghapus input manual | Integrasi cuti memakai sumbernya sendiri | `PASS` | Review `PayrollHrdImportService.cs` |
| Uji API | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build dan setelah admin memberi hak
`PayrollVariableInput` di Akses Role.

**Langkah uji untuk pemilik** (Swagger, DB uji, run berstatus `Draft` atau `Calculated`):

1. `POST /` dengan komponen `Earning` aktif, jenis `Incentive`, Rp 1.000.000. Harapan: 200,
   status `Draft`, nomor `PVI-202611-0001`, `availableActions` = update, submit, cancel.
2. Ulangi dengan jenis `AdditionalDutyHonorarium` tanpa `notes`. Harapan: 400 `VI-06`.
3. `POST /import` dengan tiga baris, satu di antaranya bernominal 0. Harapan: 400 `VI-08`, `errors`
   menunjuk baris itu, dan `GET /` tidak bertambah.
4. Perbaiki baris itu dan impor ulang. Harapan: `createdCount` = 3.
5. `POST /bulk-submit` untuk ketiganya, lalu `POST /bulk-approve` dengan akun yang sama. Harapan:
   semuanya berhasil; `approvedByName` sama dengan `submittedByName`.
6. `POST /{id}/approve` pada input `Draft`. Harapan: 409 `VI-09`.
7. `PATCH /{id}/content` dengan `expectedRowVersion` lama. Harapan: 409 `VI-12`.
8. `POST /{id}/reject` tanpa alasan. Harapan: 400 `VI-07`.
9. Akun tanpa hak `PayrollVariableInput : Approve` memanggil `approve`. Harapan: 403.
10. Ajukan run lewat endpoint run, lalu coba `POST /`. Harapan: 400 `VI-01`.

**Tidak dijalankan:** `dotnet build`, uji API, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Sebelas endpoint sesuai kontrak dengan `ControllerName = "PayrollVariableInput"` | Terpenuhi (statis), dengan delta tambahan bagian 4 | Bagian 4 dan 5 |
| 2. Transisi sah dan tidak sah sesuai matriks; aksi ke status yang sama idempoten | Terpenuhi (simulasi) | Bagian 5 |
| 3. Impor 120 baris dengan 2 baris salah: 400 `VI-08`, tidak ada baris tersimpan | Terpenuhi (simulasi); validasi selesai sebelum nomor diterbitkan dan sebelum simpan | Bagian 5; `ImportAsync` |
| 4. Nomor `PVI-...` dari number series, bukan Count/Max+1 | Terpenuhi (statis) | `AllocateNumberAsync` memakai `NumberSeriesAllocator` |
| 5. Pengaju boleh menyetujui inputnya sendiri | Terpenuhi (statis) | Tidak ada pemeriksaan pengaju ≠ penyetuju |
| 6. `SourceType = PayrollManualInput` | Terpenuhi (statis) | `NewInput`; daftar dan aksi hanya menyentuh sumber ini |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan yang dicek; checker resmi `NOT RUN` | Bagian 5 |
| DoD: kriteria terbukti | Lewat statis dan simulasi; uji API **belum** | Langkah uji menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan teknis | (1) Impor memvalidasi semua baris lebih dulu, lalu menerbitkan nomor, lalu menyimpan semua dalam satu `SaveChanges`. Dengan begitu impor yang ditolak tidak menghanguskan nomor. (2) Aksi massal menyimpan per input; penolakan selalu terjadi sebelum input diubah, sehingga input yang gagal tidak ikut tersimpan. (3) Pengajuan memeriksa ulang isi karena komponen bisa dinonaktifkan sesudah input dibuat. (4) Kolom pengguna (`SubmittedByUserId` dan seterusnya) diisi `null` bila identitas petugas kosong agar FK tidak dilanggar |
| **Risiko sela sampai `PAY-BE-20`** | Mesin hitung sekarang menghitung input berstatus `Submitted` (`CountableInputStatuses` di `PayrollCalculationService`). Akibatnya, input manual yang baru diajukan dan belum disetujui **ikut masuk gaji** bila run dihitung. Membatasi hitungan ke `Approved`/`Applied` adalah cakupan `PAY-BE-20`. Jangan menghitung run produksi dengan input manual sebelum `PAY-BE-20` selesai. **Pembaruan 2026-11-12:** ditutup di source oleh `PAY-BE-20` ([laporan](PAY-BE-20.md)); berlaku setelah build |
| Masalah yang diketahui | (1) Impor menerbitkan nomor satu per satu (satu transaksi alokator per baris); impor ribuan baris akan lambat. Kontrak tidak menetapkan batas baris, dan batas tidak dikarang. (2) Bila simpan gagal sesudah nomor terbit, nomornya hangus. Ini sesuai desain alokator (`02-backend-architecture.md` bagian 4.2) |
| Perlu keputusan pemilik kontrak | Kode run di luar status penerima input: 400 (`VI-01`) atau 409 (`VI-09`). Implementasi memakai 400 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Task ini: `??` tiga berkas baru; `M` `Program.cs`; `PayrollManualInputTypes.cs` (masih `??` sejak `PAY-BE-14`); ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14`–`18` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: build, beri hak akses, langkah uji bagian 5. Task berikutnya: `PAY-BE-20` (hanya input disetujui yang dihitung, penjaga pengajuan run, `Applied`/`Cancelled` mengikuti run) |
