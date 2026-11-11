# Laporan Perubahan Backend — `BE-HRD-098`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-098` |
| Judul | Service & Endpoint Penutupan Periode Roster oleh HRD Payroll (`Closed` Lock) |
| Slice | S-B4 (SchedulingManagement) |
| Milestone | MVP-19 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-102, HRD-TF-052 (d), 02-backend-architecture.md Bagian 19.9.4, State Transition Matrix v10 Bagian 14.1, AT-HRD-SCH-18 |
| Dependency | `BE-HRD-094` ✅ |
| Klasifikasi | `CORE PAYROLL PERIOD LOCK & FINANCIAL GOVERNANCE` |
| Task Mode | `BACKEND` |
| Tanggal | 05 November 2026 |
| Status | Selesai (`PASS`) — Roslyn Build 0 Error |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Pada akhir siklus bulan kerja rumah sakit (misal tanggal 25 setiap bulan), Tim HRD Payroll melakukan penutupan buku (*cut-off*) untuk menghitung total jam kerja, lembur, dan tunjangan dinas yang akan dibayarkan ke rekening tenaga medis. Jika jadwal kerja masih dapat diubah setelah cut-off penggajian dilakukan, perhitungan gaji rumah sakit akan mengalami selisih finansial yang fatal (*payroll discrepancy*).

Sebelumnya, terdapat dua celah serius:
1. **Bug `HRD-TF-052` (d)**: Sistem mengizinkan periode roster yang masih berstatus `Draft` (belum pernah terbit dan belum pernah dijalankan staf) untuk ditutup langsung, sehingga mengacaukan status siklus hidup dokumen.
2. **Ketiadaan Penguncian Mutlak (*Immutable Lock*)**: Belum ada gerbang validasi ketat yang menolak mutlak seluruh operasi tulis, penambahan alokasi, pengubahan shift, maupun penggantian staf saat status periode telah menjadi `Closed`.

Task `BE-HRD-098` menyelesaikan kedua masalah ini secara tuntas:
- **Penolakan Penutupan Status `Draft`**: Metode penutupan roster menolak keras penutupan periode yang masih berstatus `Draft` (menutup bug `HRD-TF-052` d). Periode wajib berstatus `Published` terlebih dahulu.
- **Pencatatan Audit Trail Penutupan**: Saat ditutup oleh petugas HRD Payroll, sistem mencatat `RosterStatus = "Closed"`, `IsLocked = true`, `ClosedAt = UtcNow`, dan `ClosedByUserId = currentUserId`.
- **Penguncian Mutlak Seluruh Operasi Tulis (HTTP 403 Forbidden)**: Seluruh endpoint mutasi (tambah alokasi staf, auto-populate staf unit, salin periode lalu, terapkan pola shift, terbitkan roster, ganti petugas langsung, dan koreksi mundur) secara konsisten menguji status `IsLocked || RosterStatus == "Closed"` dan mengembalikan respon penolakan mutlak **HTTP 403 Forbidden**.

---

## 2. Alur Penutupan Roster dan Penegakan Penguncian Mutlak

```mermaid
flowchart TD
    subgraph PenutupanRoster["1. Penutupan Resmi oleh HRD Payroll"]
        A["Petugas HRD Payroll Eksekusi Cut-off"] --> B["POST /roster-periods/{id}/close"]
        B --> C{"Status Periode Roster Saat Ini?"}
        C -->|Draft| D["Tolak (HTTP 409 Conflict): Periode Draft Tidak Boleh Ditutup (HRD-TF-052 d)"]
        C -->|Published| E["Ubah Status: RosterStatus = 'Closed', IsLocked = True"]
        E --> F["Catat Waktu & Petugas: ClosedAt = UtcNow, ClosedByUserId = UserId"]
        F --> G["Commit ke Database & Kembalikan Response 200 OK"]
    end

    subgraph PercobaanMutasi["2. Percobaan Mutasi Setelah Ditutup (Closed)"]
        H["Pengguna Mencoba: Tambah Shift / Ganti Petugas / Koreksi Mundur"] --> I{"Cek: IsLocked || RosterStatus == 'Closed'?"}
        I -->|Ya (Terkunci)| J["Tolak Mutlak (HTTP 403 Forbidden): Periode Roster Telah Ditutup (HRD-DEC-102)"]
        I -->|Tidak| K["Lanjutkan Operasi"]
    end
```

---

## 3. Komponen Perubahan yang Diterapkan

1. **Metode Service (`RosterPeriodService.CloseRosterPeriodAsync`)**:
   - Memvalidasi bahwa periode yang berstatus `Draft` ditolak untuk ditutup (HTTP 409 Conflict).
   - Memutasi status menjadi `Closed` dan mengunci flag `IsLocked = true`.
   - Mengisi timestamp `ClosedAt`, `LockedAt`, dan ID user penutup `ClosedByUserId`, `LockedByUserId`.
2. **Penegakan Gerbang Penguncian Mutlak (HTTP 403 Forbidden)**:
   - Diterapkan pada seluruh metode operasional di `RosterPeriodService`:
     - `AddAssignmentAsync` → HTTP 403 Forbidden bila closed.
     - `PopulateEligibleStaffAsync` → HTTP 403 Forbidden bila closed.
     - `CopyPreviousPeriodAsync` → HTTP 403 Forbidden bila closed.
     - `ApplyShiftPatternAsync` → HTTP 403 Forbidden bila closed.
     - `PublishRosterPeriodAsync` → HTTP 403 Forbidden bila closed.
   - Diterapkan pada seluruh metode mutasi di `ShiftReplacementService`:
     - `ExecuteDirectReplacementAsync` → HTTP 403 Forbidden bila closed.
     - `RetroactiveCorrectionAsync` → HTTP 403 Forbidden bila closed.
     - `CreateReplacementAsync` & `ApproveReplacementAsync` → HTTP 403 Forbidden bila closed.
3. **Endpoint Controller (`RosterPeriodController.cs`)**:
   - `POST /api/corporate/human-resource/scheduling/roster-periods/{id}/close` dengan atribut otorisasi `[AccessPermission("RosterPeriod", "Update")]`.

---

## 4. Spesifikasi Endpoint API Bergaya Swagger

| Method | Endpoint Path | Deskripsi | Auth / Role | Status |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/corporate/human-resource/scheduling/roster-periods/{id}/close` | Menutup periode roster secara final saat cut-off penggajian dan mengunci dari seluruh mutasi | HRD Payroll / Update | 200 OK / 409 Conflict |

---

## 5. Bukti Verifikasi dan Pengujian

- **Kompilasi Roslyn**: Lulus `dotnet build NewQuilvianSystemBackend/QuilvianSystemBackend.csproj /t:Compile` dengan **0 Error(s)**.
- **Kasus Uji Penerimaan (`AT-HRD-SCH-18`)**:
  - Terbukti penutupan periode berstatus `Draft` ditolak sistem (`HRD-TF-052` d).
  - Terbukti periode berubah menjadi `Closed` dan mencatat `ClosedByUserId` serta `ClosedAt`.
  - Terbukti seluruh upaya penambahan, pengubahan, penggantian, dan koreksi mundur pada periode `Closed` ditolak mutlak dengan kode status HTTP 403 Forbidden.
