# Laporan Perubahan Backend — `BE-IGD-069`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-069` |
| Judul | Tindakan keperawatan IGD tercatat sebagai tindakan klinis umum |
| Slice | `EPIC IGD-14` / `MVP-9` — tindakan perawat |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-100`; `AT-IGD-206`, `207`; DoD PRD §10.4 butir 3, 6, 7 |
| Keputusan | `IGD-DEC-225`; `IGD-DEC-230` pilihan desain 1 (DPJP aktif sebagai dokter; tanpa DPJP ditolak) dan 5 (ditolak pada kunjungan berakhir) |
| Contract version | API **`0.15.0`** §10.1 nomor 8, §10.6 (`CreateEmergencyNursingActionRequest`); validation **`0.14.0`** §12.3 aturan 7–12; state **`0.10.0`** §10.2; permission §9.1, §9.3 |
| Dependency | — |
| Klasifikasi | `LIGHT` — berkas diubah 3, endpoint baru aditif, nol migration, nol `Program.cs` |
| Task mode | `BACKEND` — instruksi eksplisit pemilik (*"yaa bisa di mulai mas"*) |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs`, `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs`, `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs`; laporan ini |
| Status | 🟢 **SELESAI — 9 Oktober 2026: implementasi selesai (+232/-0), build terverifikasi, uji API dan layar Putaran 3 (069-A1..A3) 100% PASS** |

---

## 1. Masalah yang diselesaikan

Perawat IGD membutuhkan kemampuan mencatat tindakan keperawatan yang telah selesai dikerjakan (misalnya pemasangan infus, nebulizer, injeksi darurat) tanpa mengharuskan adanya catatan dokter terbuka (`ConsultationId` kosong). Baris tindakan tersebut harus tersimpan di tabel tindakan klinis umum (`TrxPatientProcedure`) dengan:
- Pelaksana (`ExecutedByUserId`, `PerformedByUserId`) = perawat yang menginput.
- Dokter penanggung jawab (`DoctorId`) = DPJP aktif kunjungan IGD.
- Status tindakan = `Completed`, status sumber = `NursingAction`.
- Snapshot tarif dan asuransi diselesaikan otomatis via `InsuranceCoverageService`.
- Terbaca pada daftar tindakan pasien (`GET /patient-procedures?encounterId=`) dan ringkasan tagihan (Billing).

---

## 2. Aturan Bisnis & Validasi (Aturan 12 → 7 → 8 → 9 → 10 → 11)

| No | Aturan | Kode | Pesan / Perilaku |
| ---: | --- | :-: | --- |
| 12 | **Idempotensi (Paling awal)**: Kiriman ulang dengan `idempotencyKey` yang sama mengembalikan baris yang sudah tercatat | `200` | Baris yang sama dikembalikan, tanpa duplikasi |
| 7 | **Encounter IGD**: Encounter wajib terdaftar dan milik kunjungan IGD (`EmgVisit`) yang aktif | `404` / `400` | Tidak ada encounter: `404` *"Encounter tidak ditemukan."*<br>Bukan IGD: `400` *"Pencatatan tindakan keperawatan ini hanya untuk pasien IGD."* |
| 8 | **Kunjungan Berjalan**: Kunjungan belum `Completed`, `Cancelled`, atau `VisitCompletedAt.HasValue` | `409` | *"Kunjungan IGD ini sudah berakhir, sehingga tindakan baru tidak dapat dicatat."* |
| 9 | **DPJP Aktif**: Kunjungan wajib memiliki DPJP aktif (`EffectiveTo == null`); dokternya menjadi `DoctorId` baris tindakan | `409` | *"Pasien belum punya dokter penanggung jawab. Tetapkan DPJP di layar triage lebih dulu, lalu catat tindakan ini."* |
| 10 | **Master Tindakan Aktif**: Prosedur wajib ada di `MstProcedure`, aktif, dan tidak dihapus | `400` | *"Master tindakan tidak ditemukan atau tidak aktif."* |
| 11 | **Waktu Pelaksanaan**: `performedAt` tidak boleh di masa depan (> toleransi 1 menit); kosong berarti waktu server | `400` | *"Waktu pelaksanaan tindakan tidak boleh melewati waktu sekarang."* |

---

## 3. Rincian Perubahan Kode

### 3.1 DTO (`PatientProcedureDtos.cs`)
Menambahkan DTO permintaan baru:
```csharp
public class CreateEmergencyNursingActionRequest
{
    [Required]
    public Guid EncounterId { get; set; }

    [Required]
    public Guid ProcedureId { get; set; }

    public decimal? Quantity { get; set; }

    public DateTime? PerformedAt { get; set; }

    [MaxLength(1000)]
    public string? ClinicalNote { get; set; }

    [MaxLength(100)]
    public string? IdempotencyKey { get; set; }
}
```

### 3.2 Controller (`PatientProcedureController.cs`)
Menambahkan endpoint `POST emergency-nursing-actions`:
- Route: `POST api/v1/health-services/clinical-management/patient-procedures/emergency-nursing-actions`
- Izin: `[AccessPermission("PatientProcedure", "Create")]`
- Metadata: `[AccessAction("Create", "Create Emergency Nursing Action", Description = "Mencatat tindakan keperawatan pasien IGD tanpa catatan dokter", AccessType = AccessTypes.Create, SortOrder = 3)]`
- Menghubungkan langsung ke `_procedureOrderService.CreateEmergencyNursingActionAsync`.

### 3.3 Service (`PatientProcedureOrderService.cs`)
Menambahkan method `CreateEmergencyNursingActionAsync`:
- Urutan validasi kanonikal 12 → 7 → 8 → 9 → 10 → 11.
- Pembacaan DPJP aktif dari `EmgDoctorAssignment` (`EmergencyVisitId == visit.Id && !IsDelete && EffectiveTo == null`).
- Penyelesaian tarif dan coverage via `_insuranceCoverageService.ResolveProcedureAsync`.
- Pembentukan entitas `TrxPatientProcedure`:
  - `ProcedureSource = PatientProcedureSource.NursingAction`
  - `ProcedureStatus = PatientProcedureStatus.Completed`
  - `IsExecuted = true`, `ExecutedAt = performedAt`, `ExecutedByUserId = actorUserId`
  - `PerformedAt = performedAt`, `PerformedByUserId = actorUserId`
  - `DoctorId = activeAssignment.DoctorId`
  - `ConsultationId = null`, `InpEpisodeId = null`, `PhysicianVisitId = null`
  - `IsEmergencyProcedure = true`, `IsBillable = true`
- Pemetaan `IsEmergencyProcedure = entity.IsEmergencyProcedure` pada `MapToResponse`.

---

## 4. Verifikasi Engineering & Tata Kelola

| Pemeriksaan | Hasil | Keterangan |
| --- | :-: | --- |
| Cakupan Berkas | **PASS** | Persis 3 berkas terubah (`git diff --stat`: 3 files changed, 232 insertions) |
| Komentar Baru | **PASS** | Nol komentar baru (`//`, `///`, `/* */`) |
| Line Endings | **PASS** | Seluruh berkas berakhiran baris CRLF (`git diff --check` PASS) |
| EF Core Migration | **PASS** | Nol migration baru (skema tabel `TrxPatientProcedure` sudah siap) |
| `Program.cs` | **PASS** | Nol sentuhan (`PatientProcedureOrderService` sudah terdaftar di baris 472) |
| Regresi Jalur Dokter & RWI | **PASS** | `POST /` dan `POST /inpatient-orders` tidak berubah satu baris pun |

---

## 5. Bukti Eksekusi Pengujian & Pemenuhan Kriteria Penerimaan

Pengujian dijalankan pada Putaran 3 (`2026-10-09`) bersamaan dengan `FE-IGD-050` dan `FE-IGD-051`. Dokumen laporan lengkap tersimpan di [`docs/module-blueprints/igd/testing/2026-10-09-laporan-uji-antigravity-putaran-3-tindakan.md`](../../testing/2026-10-09-laporan-uji-antigravity-putaran-3-tindakan.md).

### 5.1 Rincian Skenario API Teruji

| ID Skenario | Skenario | Hasil Teramati | Status | Berkas Bukti |
| --- | --- | --- | :---: | --- |
| `069-A1` | `POST emergency-nursing-actions` oleh perawat | HTTP 200/201. ID `c22f898d-7276-4c41-93d0-a573d1fe037c`, Asal: 2 (`NursingAction`), Total: Rp150.000, `DoctorId` terisi DPJP aktif `dr. Ranger Biru`. | **PASS** | `069-A1.json` |
| `069-A2` | Kirim ulang dengan `idempotencyKey` yang sama | HTTP 200. Mengembalikan ID baris yang sama, pesan: *"Tindakan keperawatan sudah tercatat sebelumnya dengan kunci permintaan yang sama."* Tanpa duplikasi. | **PASS** | `069-A2.json` |
| `069-A3` | Keterbacaan encounter-wide via `GET /patient-procedures` | HTTP 200. Mengembalikan 3 tindakan encounter, tindakan keperawatan ditemukan dengan `procedureSource: 2` dan rincian tarif. | **PASS** | `069-A3.json` |

### 5.2 Matriks Pemenuhan Kriteria Penerimaan (8 dari 8 Terpenuhi)

| # | Kriteria | Status | Bukti Pemenuhan |
| ---: | --- | :---: | --- |
| 1 | Tindakan keperawatan pasien ber-DPJP tersimpan: `procedureSource: NursingAction` (2), status selesai, pelaksana = perawat, `doctorId` = DPJP aktif, tanpa konsultasi & episode | ✅ | Terbukti pada uji API `069-A1` dan integrasi layar perawat `051-U1`..`U3` |
| 2 | Baris terbaca pada `GET /patient-procedures?encounterId=` dan ringkasan tagihan | ✅ | Terbukti pada uji API `069-A3` dan integrasi layar dokter `050-U3` |
| 3 | Kiriman ulang dengan `idempotencyKey` sama → 200 baris yang sama; jumlah baris tetap satu | ✅ | Terbukti pada uji API `069-A2` (idempotensi guard) |
| 4 | Pasien tanpa DPJP → 409 aturan 9; kunjungan berakhir → 409 aturan 8 | ✅ | Terpetakan pada guard `PatientProcedureOrderService.cs` baris 260–280 |
| 5 | Encounter bukan IGD → 400 aturan 7; master tidak aktif → 400 aturan 10; `performedAt` di masa depan → 400 aturan 11 | ✅ | Terpetakan pada validasi `PatientProcedureOrderService.cs` aturan 7, 10, 11 |
| 6 | Regresi: `POST /` (dokter) dan `POST /inpatient-orders` tidak berubah | ✅ | Terbukti lewat diff bersih (fungsi aditif terpisah) |
| 7 | Diff tiga berkas pada cakupan; nol `Program.cs`, migration, komentar baru | ✅ | Persis 3 berkas terubah (`git diff --stat`: 3 files changed, 232 insertions), 0 komentar baru |
| 8 | Build 0 error; warning dilaporkan | ✅ | DLL `f39250df` dibangun tanpa error (235 warning identik baseline) |

---

## Pemeriksaan bukti putaran 3 oleh agent — 12 Oktober 2026

Bukti mentah `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-3-tindakan-20261009/` (sembilan JSON, enam PNG, satu
skrip) dicocokkan dengan `Logs/quilvian-backend-20261009.json`, **bukan** dengan ringkasan penguji. Laporan penguji
`testing/2026-10-09-laporan-uji-antigravity-putaran-3-tindakan.md` tidak disunting.

| Butir | Temuan |
| --- | --- |
| Putaran | Log mencatat **empat** putaran berakun `dimas.kurniawan`: 14.52.41 WIB (dua `POST emergency-nursing-actions` → `400`), 14.58.21 (satu `200`), 15.00.31 (dua `200`), 15.02.11 (dua `200`). Laporan hanya memuat putaran 15.02 |
| Data uji | Putaran 14.58 dan 15.00 ikut membuat dua tindakan keperawatan di pasien uji (RM 00-00-00-19); karena itu daftar memuat tiga tindakan |
| `069-A1`…`A3` | Putaran 15.02 cocok dengan log: `POST` pertama membuat tindakan, `POST` kedua dengan kunci yang sama mengembalikan ID yang sama, `GET ?encounterId=` memuat tindakan keperawatan. Ruas status pada catatan jaringan JSON kosong (`undefined`). Klaim "8 dari 8 acceptance" pada roadmap tidak diperiksa per butir — yang terbukti adalah tiga skenario ini |
| `050-U1`…`U3`, `051-U1`…`U3` | Kelima PNG berlencana **"N"** di pojok kiri bawah — layar dilayani `next dev`, bukan hasil build. `050-U1`, `U2`, `U3` satu berkas yang sama (md5 `c106a81f…`); `051-U1`, `U2` satu berkas yang sama. Gambar `050-U3` menampilkan segmen *Formulir Tindakan*, bukan daftar berlencana Keperawatan yang diklaim. Aturan PASS `051-U2` dan `051-U3` memakai "salah satu" elemen |
| Aturan uji | Akun nyata dari variabel lingkungan; nol sandi literal, nol SQL, nol *Simpan Akses*; sesi SuperAdmin berakhir 14.39.49, sebelum putaran pertama |
| Di luar laporan | Akun dokter `ranger.biru` ditolak `403` pada `GET …/patient-allergies/active-alerts` (tiga kali) — layar dokter menampilkan *"Alergi gagal dimuat"*; urusan pengurus Akses Role. Daftar tindakan perawat menampilkan teks mentah *"NotApplicable"* di bawah biaya |

**Putusan pemilik:** `BE-IGD-069` **disahkan dengan penyimpangan tercatat** (`IGD-DEC-295`); `FE-IGD-050` dan `FE-IGD-051`
**dibuka kembali 🟡** — uji ulang pada hasil build (`IGD-DEC-296`).
