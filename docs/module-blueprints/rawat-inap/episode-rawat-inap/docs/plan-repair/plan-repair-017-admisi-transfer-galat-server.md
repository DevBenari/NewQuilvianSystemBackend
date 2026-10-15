# PLAN-REPAIR-017 — Perbaikan Galat Server 500 dan Tata Letak Tampilan Admisi Pasien Transfer Poliklinik

```yaml
plan_id: PLAN-REPAIR-EPS-017
issue: ../issue/issue-017-admisi-transfer-galat-server.md
status_rencana: DISETUJUI_DAN_DISELESAIKAN
tanggal_rencana: "2026-10-14"
diputuskan_oleh: "User / Product Owner"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab"
basis_source_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup Temuan | Ringkasan Tindakan | Area | Prioritas | Status | Rencana Bukti Verifikasi |
| --- | --- | --- | :---: | :---: | :---: | --- |
| `FIX-EPS-017-01` | `ISS-EPS-017-01` | Ubah proyeksi `EmergencyVisitId` pada query `outpatient` menjadi `(Guid?)null` agar tidak memicu `InvalidCastException` di Npgsql | BE | P0 (Fatal) | SELESAI | Pemanggilan `GET /api/v1/health-services/inpatient-management/admission-transfers/476acefd-5f52-4140-9bcd-3e6d6238c3f3?sourceType=Outpatient` mengembalikan HTTP 200 OK dengan payload valid dan `emergencyVisitId: null` |
| `FIX-EPS-017-02` | `ISS-EPS-017-02` | Ubah tipe `EmergencyVisitId` pada DTO `InpatientAdmissionTransferItemResponse` menjadi `Guid?` (nullable) | BE | P1 (High) | SELESAI | Kompilasi backend `dotnet build` sukses (0 Error) |
| `FIX-EPS-017-03` | `ISS-EPS-017-03` | Pertahankan rendering `InpatientAdmissionStepper` dan masukkan penanganan galat transfer ke dalam kartu alur (`flowPanel`) | FE | P1 (High) | SELESAI | Stepper 10 tahapan tetap dirender saat error/loading transfer; kartu error diintegrasikan ke dalam `flowPanel` |
| `FIX-EPS-017-04` | `ISS-EPS-017-04` | Rapikan container kartu error transfer dengan padding standar token Quilvian dan tombol aksi berjajar rapi (*flex button group*) | FE | P2 (Medium) | SELESAI | Tombol "Periksa Ulang" dan "Kembali ke Antrean" tertata rapi berdampingan dengan icon `RotateCcw` dan `ArrowLeft` |

**Ringkasan: 4 dari 4 perbaikan telah selesai dikerjakan dan diverifikasi.**

---

## 2. Solusi Terpilih Per Temuan

### A. Backend: Perbaikan Proyeksi Tipe Data `EmergencyVisitId` (`FIX-EPS-017-01` & `FIX-EPS-017-02`)

* **Analisis Pilihan Solusi:**
  * **Opsi 1 (Tetap `Guid` non-nullable dan lakukan cast SQL khusus):**  
    Memaksa nilai `Guid.Empty` diubah via function SQL extension `uuid_nil()` atau cast EF Core.  
    *Kelemahan:* Sangat rapuh, merusak semantik domain karena pasien rawat jalan poliklinik tidak pernah terdaftar pada kunjungan IGD (`EmgVisit`).
  * **Opsi 2 (Ubah menjadi `Guid?` nullable dan setel `(Guid?)null` untuk Poliklinik - TERPILIH):**  
    Mengubah definisi properti pada `InpatientAdmissionTransferItemResponse`:
    ```csharp
    public Guid? EmergencyVisitId { get; set; }
    ```
    Dan pada [InpAdmissionTransferService.Queries.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.Queries.cs):
    ```csharp
    // Untuk query emergency:
    EmergencyVisitId = (Guid?)v.Id,

    // Untuk query outpatient:
    EmergencyVisitId = (Guid?)null,
    ```
    *Keunggulan:* Sesuai 100% dengan prinsip domain rumah sakit, EF Core memproyeksikan `NULL` yang aman bagi Npgsql, dan otomatis menghilangkan `InvalidCastException`.

---

### B. Frontend: Restrukturisasi Layout Error State Transfer (`FIX-EPS-017-03` & `FIX-EPS-017-04`)

* **Analisis Pilihan Solusi:**
  * **Opsi 1 (Biarkan di luar stepper dan hanya tambahkan CSS container):**  
    Hanya membungkus alert di dalam div dengan border.  
    *Kelemahan:* Stepper tetap hilang, pengguna kehilangan navigasi tahapan wizard.
  * **Opsi 2 (Pertahankan Stepper dan masukkan pesan error ke dalam `flowPanel` - TERPILIH):**  
    Pindahkan percabangan error transfer ke dalam alur kartu wizard yang normal di bawah `<InpatientAdmissionStepper />`. Jika terjadi error pada pengambilan data transfer saat berada di langkah selain antrean awal, render kartu error terpusat yang elegan di dalam `flowPanel`:
    - Menampilkan kartu ber-border halus dengan `InformationAlert` varian danger.
    - Menampilkan pesan yang jelas dan informatif.
    - Menempatkan tombol *"Periksa Ulang"* (dengan icon `RotateCcw`) dan *"Kembali ke Antrean"* (dengan icon `ArrowLeft`) secara berdampingan (*inline-flex*) di bagian kanan bawah kartu.

---

## 3. Skema Tampilan Sebelum → Sesudah

### A. Tampilan Saat Masalah Terjadi (Sebelum Perbaikan)
```text
┌────────────────────────────────────────────────────────────────────────┐
│ [ Header Menu: Admisi Rawat Inap ]                                     │
├────────────────────────────────────────────────────────────────────────┤
│ ❌ Terjadi kesalahan pada server.                                      │
├────────────────────────────────────────────────────────────────────────┤
│ [ Periksa Ulang (Membentang Penuh 100% Layar)                        ] │
├────────────────────────────────────────────────────────────────────────┤
│ [ Kembali ke Antrean (Membentang Penuh 100% Layar)                   ] │
│                                                                        │
│ (Layar kosong putih ke bawah, Stepper 10 Langkah HILANG TOTAL)         │
└────────────────────────────────────────────────────────────────────────┘
```

### B. Tampilan Sesudah Perbaikan (Error State yang Anggun)
```text
┌────────────────────────────────────────────────────────────────────────┐
│ [ Header Menu: Admisi Rawat Inap ]                                     │
├────────────────────────────────────────────────────────────────────────┤
│ (1) Antrean  ─▶ (2) Verifikasi Pasien ─▶ (3) Pembayaran ... (10 Langkah)│  <-- Stepper TETAP ADA!
├────────────────────────────────────────────────────────────────────────┤
│ ┌─ Kartu Panel Alur (flowPanel) ─────────────────────────────────────┐ │
│ │                                                                    │ │
│ │  ⚠️ Gagal Mengambil Data Rujukan Pasien Transfer                   │ │
│ │  Terjadi kendala saat memuat data transfer dari server.            │ │
│ │                                                                    │ │
│ │  ────────────────────────────────────────────────────────────────  │ │
│ │                                 [ 🔄 Periksa Ulang ] [ ⬅ Kembali ] │ │  <-- Tertata rapi & proporsional
│ └────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

### C. Tampilan Normal Langkah 2 (Target Keberhasilan / Happy Path)
```text
┌────────────────────────────────────────────────────────────────────────┐
│ (1) Antrean ──▶ [2] Verifikasi Pasien ──▶ (3) Pembayaran ...           │
├────────────────────────────────────────────────────────────────────────┤
│ ┌─ Kartu Verifikasi Data Pasien Transfer ────────────────────────────┐ │
│ │ [BADGE: TRANSFER PASIEN POLIKLINIK]    No. Kunjungan: ENC-OPD-...  │ │
│ │                                                                    │ │
│ │ ┌─ Identitas Pasien ─────────────┐ ┌─ Instruksi Klinis & DPJP ───┐ │ │
│ │ │ Nama Pasien : Ratna Dewi Kusuma│ │ DPJP Dituju : dr. Dewi L.   │ │ │
│ │ │ No. RM      : 00-00-00-28      │ │ Alasan Ranap: Muntah berulang│ │ │
│ │ │ Jns Kelamin : Perempuan        │ │ Unit Tujuan : Rawat Inap    │ │ │
│ │ └────────────────────────────────┘ └─────────────────────────────┘ │ │
│ │                                                                    │ │
│ │  [ ⬅ Kembali ke Antrean ]                 [ Lanjut ke Pembayaran ➡]│ │
│ └────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Spesifikasi Kontrak API (Bergaya Swagger)

### Tag: `[Tags("InpatientAdmissionTransfer")]`

| Properti Endpoint | Keterangan |
| --- | --- |
| **HTTP Method** | `GET` |
| **Path** | `/api/v1/health-services/inpatient-management/admission-transfers/{id}` |
| **Deskripsi** | Mengambil detail satu rujukan disposisi transfer pasien IGD atau Poliklinik menuju rawat inap berdasarkan ID. |
| **Wewenang (Auth)** | `[AccessPermission("InpatientAdmissionTransfer", "Read")]` |
| **Query Parameter** | `sourceType` (string, optional: `"Emergency"` atau `"Outpatient"`) |
| **Status 200 OK Response** | `ApiResponse<InpatientAdmissionTransferItemResponse>` |

#### Respons JSON (Poliklinik):
```json
{
  "statusCode": 200,
  "message": "Detail rujukan transfer berhasil diambil.",
  "data": {
    "transferId": "476acefd-5f52-4140-9bcd-3e6d6238c3f3",
    "sourceType": "Outpatient",
    "emergencyVisitId": null,
    "emergencyVisitNumber": "",
    "sourceEncounterId": "b1b7021e-128a-4934-bc2c-e16e6d1f5e82",
    "sourceEncounterNumber": "ENC-OPD-202610-002",
    "resultEpisodeId": null,
    "patientId": "d5d7f02c-4379-4c6b-bf64-4e4f3134ee16",
    "patientName": "Ratna Dewi Kusuma",
    "medicalRecordNumber": "00-00-00-28",
    "gender": "Female",
    "birthDate": "1994-05-12T00:00:00Z",
    "decidedByDoctorId": "73c68326-1e68-45ec-b9a3-5d51965fe284",
    "decidedByDoctorName": "dr. Dewi Lestari",
    "dispositionReason": "Intake oral sulit karena muntah berulang, dehidrasi sedang",
    "patientCondition": "Compos mentis, turgor kulit menurun",
    "followUpInstruction": "Rehidrasi intravena dan observasi tanda vital",
    "recommendedServiceUnitId": "e2a14352-0c9f-4423-a5bc-b7008cf5aa11",
    "recommendedServiceUnitName": "Rawat Inap",
    "decidedAt": "2026-10-14T06:20:00Z",
    "waitingMinutes": 84,
    "isOverdue": true,
    "dispositionStatus": "Confirmed"
  }
}
```

---

## 5. Kriteria Penerimaan (Acceptance Criteria / DoD)

1. **AC-1 (Backend Bebas Galat Cast):** Pemanggilan `GetById` untuk transfer pasien Poliklinik tidak lagi melempar `InvalidCastException` dan mengembalikan respons HTTP 200 dengan `emergencyVisitId: null`.
2. **AC-2 (Kompilasi Backend Bersih):** Perintah `dotnet build NewQuilvianSystemBackend/QuilvianSystemBackend.csproj` menghasilkan 0 error.
3. **AC-3 (Konsistensi Stepper UI):** Stepper 10 tahapan admisi rawat inap tetap tampil utuh di bagian atas layar baik saat loading, sukses, maupun saat error terjadi.
4. **AC-4 (Perapihan Tata Letak Error):** Jika terjadi galat jaringan/server, pesan error dan tombol aksi tertata anggun di dalam wadah kartu dengan tombol flex yang proporsional.
5. **AC-5 (Transisi Langkah Mulus):** Saat tombol "Proses Admisi ->" ditekan pada pasien transfer Poliklinik, layar berhasil masuk ke Langkah 2 (*Verifikasi Pasien*) dan menampilkan ringkasan data rujukan pasien secara akurat.
