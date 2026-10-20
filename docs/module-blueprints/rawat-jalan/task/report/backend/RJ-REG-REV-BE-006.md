# Laporan Task `RJ-REG-REV-BE-006` — Kuota terpakai jadwal dokter per tanggal kunjungan

| Field | Isi |
| --- | --- |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) |
| Requirement | `RJ-REG-REV-013` |
| Keputusan | `RJ-REG-DEC-010` (amendemen 15 Okt 2026), wewenang `RJ-REG-DEC-001` |
| Tanggal | 15 Oktober 2026 |
| Status | ✅ `TESTED` / `UAT_PENDING` |

## 1. Tujuan

Layar pendaftaran Rawat Jalan perlu tahu jadwal dokter mana yang kuotanya sudah penuh pada tanggal kunjungan, supaya kartunya bisa dinonaktifkan. Sebelum task ini, backend hanya memeriksa kuota saat kunjungan disimpan. Daftar jadwal yang dipakai layar (`doctor-schedules/admin`) hanya memuat batas kuota, tanpa jumlah pasien yang sudah terdaftar.

Contoh: jadwal dr. Arif punya kuota 18 pasien. Bila pada 15 Okt sudah ada 18 antrean aktif, endpoint ini mengembalikan `isFullForWalkIn = true` dan `isFullForAppointment = true`, lalu layar menonaktifkan kartu dr. Arif.

## 2. Backend Governance Preflight

| Field | Isi |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (registry: `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` untuk service dan DTO baru; `TOUCHED LEGACY` untuk `PatientEncounterController` dan `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001` (hitungan di service, bukan context di controller), `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-MOD-001` |
| Entity / migration | Tidak ada. Tidak ada perubahan skema |
| Jenis capability | Transaksi, arketipe read-only (bukan master data) |

## 3. Endpoint

```http
GET /api/v1/health-services/registration-management/patient-encounters/doctor-schedule-quotas?visitDate=2026-10-15&clinicId={guid}
```

| Parameter | Wajib | Isi |
| --- | --- | --- |
| `visitDate` | Ya | Tanggal kunjungan, format `yyyy-MM-dd` |
| `clinicId` | Tidak | Batasi ke jadwal satu poliklinik |

Izin: `PatientEncounter : Read` (`[AccessAction("Read", ...)]` + `[AccessPermission("PatientEncounter", "Read")]`). Tidak ada action baru yang perlu dicentang di Akses Role.

Contoh respons `200`:

```json
{
  "data": [
    {
      "doctorScheduleId": "9666efba-fcb5-b1f4-00e1-7c409463c1b0",
      "maxPatientQuota": 18,
      "maxWalkInQuota": 4,
      "maxAppointmentQuota": 14,
      "usedTotal": 1,
      "usedWalkIn": 0,
      "usedAppointment": 1,
      "isFullForWalkIn": false,
      "isFullForAppointment": false
    }
  ],
  "message": "Kuota jadwal dokter berhasil diambil."
}
```

| Status | Kapan |
| --- | --- |
| `200` | Berhasil; daftar jadwal aktif (tidak dihapus, `IsActive`) beserta pemakaiannya |
| `400` | `visitDate` kosong atau bukan `yyyy-MM-dd` |
| `401` / `403` | Belum login / tanpa izin `PatientEncounter : Read` |

Aturan penuh: kuota `0` berarti tanpa batas. `isFullForWalkIn` bernilai benar bila kuota total **atau** kuota walk-in sudah tercapai. `isFullForAppointment` bernilai benar bila kuota total **atau** kuota appointment sudah tercapai.

## 4. Berkas

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/DoctorScheduleQuotaService.cs` | Baru. Satu-satunya penghitung antrean aktif per jadwal dan tanggal (total, walk-in, appointment), aturan penuh, dan daftar kuota untuk endpoint |
| `Areas/HealthServices/RegistrationManagement/DTOs/DoctorScheduleQuotaDtos.cs` | Baru. `DoctorScheduleQuotaQuery`, `DoctorScheduleQuotaResponse` |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Action `GetDoctorScheduleQuotas`. `ValidateDoctorScheduleQuotaAsync` kini memakai service yang sama, dengan urutan pemeriksaan dan kalimat penolakan yang tidak berubah. Konstruktor mendapat parameter opsional (pola harness lama tetap sah) |
| `Program.cs` | `AddScoped<DoctorScheduleQuotaService>()` |

Delta kontrak: tidak ada. Endpoint sesuai kartu task.

## 5. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build -c Release` | `0 Error` — `PASS` |
| Build ke scratchpad (`-p:UseSharedCompilation=false -o <scratchpad>`) | `0 Error` — `PASS` |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." — `PASS` |
| QBE Strict (4 berkas tersentuh) | 0 violation, 0 review — `PASS` |
| `dotnet test` | `NOT RUN` — tidak ada project test (pola Bank Darah) |

Runtime HTTP terhadap aplikasi build scratchpad di `https://localhost:7185`, DB `QuilvianNewDevSukma`, akun SuperAdmin:

| ID | Skenario | Hasil |
| --- | --- | --- |
| R0 | `visitDate=2026-10-15` → `200`, 63 jadwal | `PASS` |
| R1 | Jadwal `9666…` tanggal 15 Okt: 1 antrean appointment aktif, sama dengan isi DB | `PASS` |
| R2 | Belum penuh → kedua `isFull*` `false` | `PASS` |
| R3 | Jadwal `2e80…` tanggal 20 Okt: 3 antrean batal, 0 aktif → `usedTotal = 0` | `PASS` |
| R4 | `clinicId` menyaring jadwal satu poli (28 jadwal; jadwal poli lain tidak ikut) | `PASS` |
| R5 | Jadwal `0c34…` tanggal 14 Okt: 1 walk-in aktif | `PASS` |
| R6 | `visitDate=15-10-2026` → `400` "Tanggal kunjungan wajib berformat yyyy-MM-dd." | `PASS` |
| R7 | Tanpa `visitDate` → `400` | `PASS` |
| R8 | Tanpa login → `401` | `PASS` |
| R9 | `MaxWalkInQuota` sementara 1 (1 walk-in aktif) → hanya `isFullForWalkIn` `true` | `PASS` |
| R10 | `MaxPatientQuota` sementara 1 → kedua `isFull*` `true` | `PASS` |
| R11 | `MaxPatientQuota` sementara 0 → tidak penuh | `PASS` |
| R12 | Data jadwal dikembalikan ke 16/8/8 dan diverifikasi ulang lewat API dan SQL | `PASS` |

Total 13/13 `PASS`. R9–R11 memakai perubahan sementara `MstDoctorSchedule` atas izin user (15 Okt 2026). Keadaan DB sesudah run sama dengan sebelumnya.

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Antrean aktif = `MaxPatientQuota` → kedua `isFull*` `true` | Terpenuhi | R10 |
| 2. Kuota walk-in penuh, total belum → hanya `isFullForWalkIn` | Terpenuhi | R9; jalur appointment memakai aturan simetris di `IsFullForAppointment` |
| 3. Antrean batal/terhapus tidak dihitung | Terpenuhi | R3 |
| 4. Kuota `0` tidak pernah penuh | Terpenuhi | R11 |
| 5. `visitDate` salah/kosong → `400`; tanpa izin → `403` | `400` terpenuhi (R6, R7). `403` lewat metadata akses standar, belum diuji dengan akun tanpa izin | R6, R7, kode |
| 6. Validasi simpan identik | Terpenuhi lewat review kode: filter antrean, urutan pemeriksaan, dan kalimat penolakan sama persis. Simpan kunjungan pada jadwal penuh belum diuji runtime | diff `ValidateDoctorScheduleQuotaAsync` |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Uji `403` dengan akun tanpa izin dan simpan kunjungan saat penuh belum dijalankan (masuk UAT, sama seperti task revisi lain). `OutpatientAppointmentService` (reschedule) masih punya hitungan kuota sendiri; itu di luar scope task ini dan dicatat sebagai kandidat penyatuan |
| Migration / database | Tidak ada migration. Hanya perubahan data sementara untuk uji, sudah dikembalikan |
| Status Git | Berkas di atas `M`/`??`, belum di-commit |
| Task berikutnya | `RJ-REG-REV-FE-009` |
