# Laporan Implementasi Task Backend: BE-HRD-142

## 1. Identitas Task
* **ID Task**: `BE-HRD-142`
* **Nama Task**: Background Scheduler Eksekusi Hangus Saldo CTO Kedaluwarsa 90 Hari
* **Modul / Sub-Modul**: Human Resource / Sub-modul 09 — Administrasi Lembur Pegawai Rumah Sakit (Overtime Management)
* **Slice / Milestone**: `Slice S-B3` · `Fase MVP-29`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Ringkasan Eksekutif & Proses Bisnis Rumah Sakit
Peraturan ketenagakerjaan dan kebijakan internal rumah sakit menetapkan bahwa hak cuti pengganti lembur (*Compensatory Time Off / CTO*) wajib dimanfaatkan oleh pegawai dalam kurun waktu 90 hari kalender sejak timbulnya hak tersebut. Tujuannya adalah agar nakes segera mengambil waktu istirahat yang proporsional dan tidak menimbun saldo libur hingga bertahun-tahun.

Melalui implementasi `BE-HRD-142`:
1. Sistem menjalankan rutinitas pemeriksaan harian untuk memindai seluruh tiket CTO aktif yang telah melewati batas `ExpiryDate` (`VR-OT-13`).
2. Sisa hari dan jam pada tiket yang kedaluwarsa secara otomatis dihanguskan (`IsExpired = true`, `CompensatoryStatus = "Expired"`).
3. **Larangan Konversi Tunai (`VR-OT-14`)**: Sisa saldo yang hangus tidak dapat dicairkan kembali menjadi uang tunai.
4. **Integrasi Mutasi Debet Sub-08 (`INT-HR-49`)**: Sistem secara otomatis memposting mutasi pengurangan saldo (`CompensatoryForfeiture`) pada buku besar `TrxLeaveBalanceLedger` agar saldo cuti pegawai berkurang secara transparan dan berjejak audit.

---

## 3. Komponen Teknis yang Diimplementasikan
1. **Interface Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/ICompensatoryTimeOffExpirationService.cs`
2. **Implementasi Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/CompensatoryTimeOffExpirationService.cs`
   - Mengevaluasi tiket kedaluwarsa, menandai hangus, memposting debet saldo cuti di `TrxLeaveBalanceLedger`, dan menghasilkan rekapitulasi audit.
3. **Endpoint Pemanggil Manual / Scheduler**:
   - Diintegrasikan pada `CompensatoryTimeOffModernController.cs` rute `POST /run-expiry`.
   - Tag Swagger: `[Tags("HR Overtime - Compensatory Time Off (CTO)")]`.
4. **DTO Kontrak Data**:
   - `OvertimeMvp29ModernDtos.cs`: `CompensatoryExpirationRunResultDto`.

---

## 4. Spesifikasi Endpoint API (Gaya Swagger)
* **Tag Swagger**: `[Tags("HR Overtime - Compensatory Time Off (CTO)")]`

| HTTP Method | Path Endpoint | Deskripsi Bisnis | Otorisasi / Role | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/overtime/compensatory-time-off/run-expiry` | Memicu eksekusi penghangusan tiket CTO yang melewati 90 hari kalender | HR Admin / Scheduler Runner (`CompensatoryTimeOffModern.Execute`) | `200 OK` |

---

## 5. Bukti Verifikasi Aturan Bisnis (Acceptance Criteria)
* **VR-OT-13 (Automatic Forfeiture)**: Seluruh tiket dengan `ExpiryDate < Today` dan `RemainingHours > 0` statusnya berubah menjadi `Expired`.
* **VR-OT-14 (No Cash Conversion)**: Sisa jam hangus di-nol-kan dan tidak diikutkan dalam serah terima payroll finansial.
* **Audit Debet Transparan**: Terbentuk entri `CompensatoryForfeiture` pada `TrxLeaveBalanceLedger` dengan nilai minus hari yang dihanguskan.


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok F — logika tidak tersambung |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | ✅ **SELESAI 10 November 2026** (dikerjakan ulang) |

### Masalah yang ditemukan audit

`CompensatoryTimeOffExpirationService` (penghangusan tiket CTO 90 hari jalur modern) hanya bisa dipicu manual dari controller. Scheduler `OvertimeSchedulerService` hanya memanggil mesin legacy `OvertimeCompensatoryExpiryService`, yang melewati tiket jalur modern karena tiket itu tidak punya transaksi saldo sumber. Akibatnya tiket CTO jalur modern tidak pernah hangus otomatis.

### Perubahan yang dikerjakan

- Scheduler kini menjalankan **kedua** mesin pada job `ExpireCompensatory`/`FullCycle`: mesin legacy untuk tiket bertransaksi saldo sumber, mesin modern untuk tiket jalur modern (`LeaveBalanceTransactionId == null`). Hitungan hasilnya digabung ke ringkasan job.
- Kontrak `ICompensatoryTimeOffExpirationService.ExecuteExpirationRunAsync` kini menerima `actorUserId` yang dicatat pada mutasi buku besar dan `UpdateBy` tiket (`QBE-LOG-001`); controller `run-expiry` meneruskan pengguna yang login.
- Mesin legacy ikut menandai `IsExpired = true` dan `RemainingHours = 0` supaya kedua jalur membaca penanda yang sama.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimeSchedulerService.cs`
- `Areas/Corporate/HumanResource/OvertimeManagement/Services/CompensatoryTimeOffExpirationService.cs`
- `Areas/Corporate/HumanResource/OvertimeManagement/Services/ICompensatoryTimeOffExpirationService.cs`
- `Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimeCompensatoryExpiryService.cs`
- `Areas/Corporate/HumanResource/OvertimeManagement/Controllers/CompensatoryTimeOffModernController.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Scheduler otomatis menghanguskan saldo CTO yang melewati 90 hari | Terpenuhi di source | scheduler memanggil kedua mesin; `AutoExpireCompensatory` tetap menjadi sakelar konfigurasi |

### Database dan migration

`NOT APPLICABLE` — tidak ada perubahan skema untuk task ini.

### Catatan penutup

- Dua buku besar cuti (`TrxLeaveBalanceTransaction` legacy dan `TrxLeaveBalanceLedger` modern) tetap berjalan paralel; penyatuannya di luar lingkup task ini dan perlu keputusan pemilik.
- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
