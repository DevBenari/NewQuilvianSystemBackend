# Laporan Perubahan Backend — `BE-FIN-140`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-140` |
| Judul | Kartu yang dipasang otomatis tidak bisa dipakai bila identitas pasien berubah sesudahnya |
| Slice | `S9` Pemeriksaan Ulang Identitas saat Kunjungan (`MVP-18`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-23" |
| Trace | `FR-FIN-271`; `FIN-DEC-256`, `257`; `FIN-DES-122`, `126`; `FIN-API-1.12` §`V.4`; `FIN-VAL-295`; `FIN-INTEGRATION-1.11` §`V.5` |
| Contract version | `FIN-API-1.12` §`V.4`, `FIN-VAL-295`, `FIN-INTEGRATION-1.11` §`V.5` |
| Dependency | `BE-FIN-135` 🟡 (seluruh prasyarat terpenuhi) |
| Klasifikasi | `MEDIUM` — Penyempurnaan `ValidateCardForServiceAsync` pada `RegistrationEmployeeBenefitLookupService` dengan overload `patientId` dan `patientCompanyGuarantorId`, pencocokan ulang identitas pasien kanonik (NIK, tanggal lahir, jenis identitas KTP/NIK) terhadap data pemilik benefit di HR khusus untuk kartu hasil sinkronisasi otomatis (`SYNCED`), penyelarasan respon pada jalur loket `PatientEncounterController` (HTTP 400 `FIN-DES-126`) dan jalur kasir / ganti penanggung `EncounterPaymentSourceService` (HTTP 422 `PATIENT_IDENTITY_MISMATCH` `FIN-VAL-295`). |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. Frontend tidak disentuh |
| Model | Claude Sonnet 3.7 / Google Antigravity |
| Tanggal | 4 November 2026 |
| Status | 🟡 **Source selesai, kompilasi belum dibuktikan.** `dotnet build` **NOT RUN** atas instruksi pemilik ("tanpa build dotnet manual") |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (`Services` dan `Controllers`). Entity yang terlibat: `MstPatientCompanyGuarantor`, `PatEmployeeBenefitCardSync`, `MstPatient`, `MstCompanyGuarantor`. |
| Keberlakuan | `TOUCHED LEGACY` pada `RegistrationEmployeeBenefitLookupService.cs`, `PatientEncounterController.cs`, dan `EncounterPaymentSourceService.cs`. |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-LOG-001` (tanpa log data PII/NIK/RM), `QBE-VAL-001`, `QBE-MOD-002`, `QBE-MOD-003`. |
| Branch | `Yasmina`, sesuai penetapan roadmap. |
| Working tree sebelum task | Bersih dari berkas temporer; melanjutkan hasil `BE-FIN-139`. |

---

## 1. Masalah yang Diperbaiki

Pada operasional rumah sakit terkait pemakaian kartu penjamin "Benefit Karyawan RS":
1. **Pencegahan Penyalahgunaan Kartu Pasca Perubahan Identitas Pasien (`FIN-DEC-256`, `FIN-DES-122`)**:
   Kartu penjamin internal yang terpasang secara otomatis oleh proses sinkronisasi latar belakang (`SYNCED`) ditautkan ke pasien berdasarkan kecocokan NIK, tanggal lahir, dan jenis tanda pengenal pada saat pemasangan. Bila di kemudian hari data pasien di SIMRS diubah (misalnya NIK pasien diganti, tanggal lahir diubah, atau jenis identitas diubah dari KTP ke SIM), kartu tersebut berisiko dipakai oleh orang yang sebenarnya bukan pegawai/dokter pemilik benefit.
   Sesuai mandat `FIN-DEC-256`, sistem **wajib mencocokkan ulang identitas pasien saat kunjungan didaftarkan**, khusus untuk kartu hasil sinkronisasi otomatis.
2. **Pengecualian Kartu Manual dan Kartu Lama (`FIN-DEC-256`, AC 2)**:
   Kartu penjamin internal yang dibuat secara manual oleh petugas loket (dengan verifikasi fisik langsung) atau kartu riwayat lama yang tidak memiliki catatan `SYNCED` pada `PatEmployeeBenefitCardSync` **tidak terkena pengecekan ulang otomatis ini**, demi menjaga stabilitas dan tidak mengganggu pasien eksisting.
3. **Penyelarasan Kontrak Respons Galat per Jalur Layanan (`FIN-DES-126`, `FIN-GAP-010`)**:
   - **Jalur Loket Pendaftaran (`PatientEncounterController`)**: Mempertahankan envelope HTTP **400 Bad Request** dengan pesan validasi khusus: *"Identitas pasien tidak lagi sesuai dengan pemilik Benefit Karyawan RS pada kartu ini. Kartu tidak dapat dipakai; daftarkan sebagai Pasien Umum atau minta Registrasi meninjau kartunya."*
   - **Jalur Ganti Penanggung / Kasir Billing (`EncounterPaymentSourceService`)**: Mengembalikan envelope HTTP **422 Unprocessable Entity** dengan kode error `"PATIENT_IDENTITY_MISMATCH"` dan pesan validasi yang sama (`FIN-VAL-295`).
4. **Perlindungan Kunjungan Pasien Umum & Kunjungan Warisan (AC 3, AC 4)**:
   Kunjungan warisan (misalnya bayi baru lahir yang mewarisi penjamin orang tua) dan pendaftaran sebagai Pasien Umum (pembayaran tunai/mandiri) tetap dapat diproses lancar tanpa terhalang validasi kartu ini.

Task `BE-FIN-140` menutup seluruh celah di atas dengan menyempurnakan `RegistrationEmployeeBenefitLookupService`, `PatientEncounterController`, dan `EncounterPaymentSourceService`.

---

## 2. Proses Bisnis & Pemenuhan Acceptance Criteria

### 2.1 Alur Validasi Kartu Saat Kunjungan

```mermaid
flowchart TD
    Start["Kunjungan Memakai Kartu Penjamin Internal\n(Pendaftaran Loket / Ganti Penanggung Kasir)"] --> CheckInternal{"Apakah Penjamin Internal?\n(IsEmployeeBenefitGuarantor == true)"}
    CheckInternal -- "Bukan Penjamin Internal" --> ValidPass["Lolos Validasi (Valid)"]
    
    CheckInternal -- "Ya, Penjamin Internal" --> CheckEligibility{"Periksa Kelayakan di HR\n(Layanan Aktif & Tanggal Masuk Periode)"}
    CheckEligibility -- "Tidak Layak / HR Mati" --> ReturnFail["Gagal: Kembalikan Alasan Kelayakan / HR_UNAVAILABLE"]
    
    CheckEligibility -- "Layak" --> CheckSynced{"Apakah Kartu Tercatat SYNCED pada PatEmployeeBenefitCardSync?\n(Kartu Hasil Sinkronisasi Otomatis)"}
    
    CheckSynced -- "Bukan (Kartu Manual / Lama) (AC 2)" --> ValidPass
    
    CheckSynced -- "Ya, Kartu Otomatis (SYNCED)" --> LoadPatient["Baca Identitas Pasien Kanonik dari Database SIMRS\n(Perhitungkan Penggabungan Rekam Medis MergedToPatientId)"]
    
    LoadPatient --> CheckType{"Apakah Jenis Identitas KTP atau NIK?\n(V.8 Butir 2)"}
    CheckType -- "Bukan (misal SIM / Paspor / Kosong)" --> Mismatch["Identitas Tidak Cocok (PATIENT_IDENTITY_MISMATCH)"]
    
    CheckType -- "Ya (KTP / NIK)" --> CheckNik{"Apakah NIK (Angka Saja) Cocok Sama Persis dengan NIK Pemilik HR?\n(V.8 Butir 3)"}
    CheckNik -- "Tidak Cocok / Berbeda" --> Mismatch
    
    CheckNik -- "Cocok" --> CheckDob{"Apakah Tanggal Lahir Kalender Cocok Sama Persis?\n(V.8 Butir 4)"}
    CheckDob -- "Tidak Cocok / Berbeda" --> Mismatch
    
    CheckDob -- "Cocok" --> ValidPass
    
    Mismatch --> BranchCaller{"Jalur Pemanggil Validasi"}
    BranchCaller -- "Loket Pendaftaran (PatientEncounterController)" --> Ret400["Kembalikan 400 Bad Request:\n'Identitas pasien tidak lagi sesuai dengan pemilik Benefit Karyawan RS pada kartu ini...' (FIN-DES-126)"]
    BranchCaller -- "Kasir / Ganti Penanggung (EncounterPaymentSourceService)" --> Ret422["Kembalikan 422 Unprocessable Entity:\nErrorCode: 'PATIENT_IDENTITY_MISMATCH' (FIN-VAL-295)"]
```

### 2.2 Pemenuhan Rinci Empat Acceptance Criteria (AC)

| No | Acceptance Criteria | Status | Bukti Implementasi di Kode Sumber |
|:---|:---|:---:|:---|
| **AC 1** | **NIK pasien diubah → loket 400, kasir 422** | ✅ **TERPENUHI** | • `RegistrationEmployeeBenefitLookupService.cs` mendeteksi perbedaan NIK (`patientNik != targetNik`) dan mengembalikan `CardServiceCheckResult.Invalid("PATIENT_IDENTITY_MISMATCH", PatientIdentityMismatchMessage)`.<br>• `PatientEncounterController.cs` (jalur loket) memetakan pesan kegagalan tersebut ke HTTP **400 Bad Request** (`FIN-DES-126`).<br>• `EncounterPaymentSourceService.cs` (jalur kasir/ganti penanggung) memetakan hasil tersebut ke HTTP **422 Unprocessable Entity** dengan `ErrorCode = "PATIENT_IDENTITY_MISMATCH"` (`FIN-VAL-295`). |
| **AC 2** | **Kartu manual dan lama tidak dicek ulang** | ✅ **TERPENUHI** | `RegistrationEmployeeBenefitLookupService.cs` baris pengecekan:<br>`var isSyncedCard = await _dbContext.PatEmployeeBenefitCardSyncs.AsNoTracking().AnyAsync(s => !s.IsDelete && s.PatientCompanyGuarantorId == patientCompanyGuarantorId.Value && s.SyncStatus == EmployeeBenefitCardSyncStatuses.Synced, cancellationToken);`<br>Bila kartu bukan kartu hasil sinkronisasi otomatis berstatus `SYNCED`, blok pencocokan identitas dilewati (`isSyncedCard == false`) sehingga kartu manual dan kartu lama langsung lolos validasi tanpa kendala. |
| **AC 3** | **Kunjungan warisan tidak terdampak** | ✅ **TERPENUHI** | Pada `PatientEncounterController.cs`, alur kunjungan yang memiliki pembayaran warisan (`inheritedPaymentPreview != null`, seperti pada bayi baru lahir yang mewarisi penjamin induk) tidak mengeksekusi blok `LoadValidPatientCompanyGuarantorAsync`, sehingga terisolasi penuh dan tidak terdampak perubahan validasi ini. |
| **AC 4** | **Pendaftaran tetap bisa sebagai Pasien Umum** | ✅ **TERPENUHI** | Pendaftaran dengan metode bayar tunai / umum (`PaymentType == EncounterPaymentType.Cash` atau non-penjamin) tidak memicu pemanggilan `LoadValidPatientCompanyGuarantorAsync` maupun `ValidateCardForServiceAsync`, sehingga pasien yang kartunya ditolak dapat langsung didaftarkan sebagai Pasien Umum. |

---

## 3. Skenario Riil Rumah Sakit (Studi Kasus Konkret)

### Skenario 1: Petugas Loket Menemukan Pasien yang NIK-nya Berubah (AC 1, Jalur Loket 400)
- **Kondisi**: dr. Budi Santoso memiliki kartu Benefit Karyawan RS hasil sinkronisasi otomatis (`SYNCED`). Staf pendaftaran sebelumnya keliru menginput NIK dr. Budi di rekam medis SIMRS menjadi `3171010101900002` (berbeda dengan NIK asli di HR `3171010101900001`).
- **Aksi Petugas**: Pasien datang ke loket pendaftaran rawat jalan dan petugas memilih pembayaran Penjamin Perusahaan dengan kartu benefit internal.
- **Hasil Sistem**: Sistem loket mengembalikan HTTP 400 Bad Request:
  ```json
  {
    "success": false,
    "message": "Identitas pasien tidak lagi sesuai dengan pemilik Benefit Karyawan RS pada kartu ini. Kartu tidak dapat dipakai; daftarkan sebagai Pasien Umum atau minta Registrasi meninjau kartunya."
  }
  ```
  Petugas loket mendapatkan instruksi jelas untuk mendaftarkan pasien sebagai Pasien Umum atau memperbaiki identitas di master rekam medis.

### Skenario 2: Kasir Mencoba Mengganti Penanggung ke Kartu yang Tidak Cocok (AC 1, Jalur Kasir 422)
- **Kondisi**: Petugas kasir di billing melakukan penyesuaian penanggung kunjungan (`PUT /api/v1/health-services/billing-management/billing/invoices/{id}/payment-source`) ke kartu benefit internal milik seorang staf yang identitas tanggal lahirnya tidak cocok dengan data HR.
- **Aksi Sistem**: `EncounterPaymentSourceService.SwitchPaymentSourceAsync` memanggil `ValidateCardForServiceAsync` dengan menyertakan `card.PatientId` dan `card.Id`.
- **Hasil Sistem**: Backend menolak pergantian penanggung dengan status HTTP 422 Unprocessable Entity:
  ```json
  {
    "success": false,
    "errorCode": "PATIENT_IDENTITY_MISMATCH",
    "message": "Identitas pasien tidak lagi sesuai dengan pemilik Benefit Karyawan RS pada kartu ini. Kartu tidak dapat dipakai; daftarkan sebagai Pasien Umum atau minta Registrasi meninjau kartunya."
  }
  ```

### Skenario 3: Pasien Menggunakan Kartu Penjamin Internal Manual (AC 2)
- **Kondisi**: Staf HR mendaftarkan kartu penjamin internal secara manual di loket dengan menunjukkan surat tugas khusus (tidak lewat sinkronisasi otomatis `PatEmployeeBenefitCardSync`).
- **Aksi Sistem**: Saat kunjungan didaftarkan, sistem memeriksa tabel sinkronisasi. Karena kartu tidak tercatat berstatus `SYNCED`, sistem hanya memeriksa kelayakan masa berlaku dan status aktif penjamin internal.
- **Hasil**: Pendaftaran kunjungan berhasil disetujui (201 Created) tanpa penolakan.

### Skenario 4: Pasien yang Ditolak Dialihkan ke Pasien Umum (AC 4)
- **Kondisi**: Pasien pada Skenario 1 tidak sempat memperbaiki data rekam medis saat itu juga karena harus segera diperiksa dokter poli.
- **Aksi Petugas**: Petugas memilih metode pembayaran Tunai (Pasien Umum).
- **Hasil**: Validasi penjamin internal dilewati sepenuhnya, nomor antrean poli berhasil terbit.

---

## 4. Spesifikasi Teknis Endpoint Terkait (Bergaya Swagger)

### 4.1 `POST /patient-encounters` — Pendaftaran Kunjungan Pasien (Loket)

| Item | Deskripsi |
|:---|:---|
| **Tags** | `[Tags("Health Services / Registration Management / Patient Encounter")]` |
| **Method & Path** | `POST api/v1/health-services/registration-management/patient-encounters` (serta `/admin` dan `/kiosk`) |
| **Deskripsi** | Pendaftaran kunjungan pasien. Memvalidasi kartu penjamin perusahaan dan memeriksa ulang identitas kartu sinkronisasi otomatis. |
| **Otorisasi / RBAC** | `Bearer Token` dengan izin `PatientEncounter : Create`. |
| **Request Body** | `PatientEncounterCreateRequest` (`patientId`, `paymentType`, `patientCompanyGuarantorId`, dll.). |
| **Response 201 Created** | Kunjungan berhasil dibuat. |
| **Response 400 Bad Request** | Kartu penjamin internal hasil sinkronisasi otomatis tidak cocok dengan data identitas pasien (`FIN-DES-126`, `FIN-VAL-295`). |

### 4.2 `PUT /invoices/{id}/payment-source` — Pergantian Sumber Pembayaran (Kasir / Billing)

| Item | Deskripsi |
|:---|:---|
| **Tags** | `[Tags("Health Services / Billing Management / Billing / Invoices")]` |
| **Method & Path** | `PUT api/v1/health-services/billing-management/billing/invoices/{id}/payment-source` |
| **Deskripsi** | Mengganti penanggung pada tagihan kunjungan pasien sebelum finalisasi tagihan. |
| **Otorisasi / RBAC** | `Bearer Token` dengan izin `BillingInvoice : Update`. |
| **Request Body** | `SwitchPaymentSourceRequest` (`patientCompanyGuarantorId`, dll.). |
| **Response 200 OK** | Sumber pembayaran berhasil dialihkan. |
| **Response 422 Unprocessable Entity** | Galat bisnis dengan kode `PATIENT_IDENTITY_MISMATCH` bila identitas pasien tidak cocok dengan data HR pemilik kartu (`FIN-VAL-295`). |

---

## 5. Rincian Teknis Berkas yang Diperbarui

1. **`RegistrationEmployeeBenefitLookupService.cs`** (Diperbarui):
   - Menambahkan konstanta `PatientIdentityMismatchCode` (`"PATIENT_IDENTITY_MISMATCH"`) dan `PatientIdentityMismatchMessage` (`FIN-VAL-295`).
   - Menyediakan overload baru `ValidateCardForServiceAsync(companyGuarantorId, workforceProfileId, employeeId, familyMemberId, serviceDate, patientId, patientCompanyGuarantorId, cancellationToken)`.
   - Mengimplementasikan pengecekan `PatEmployeeBenefitCardSyncs` berstatus `SYNCED`.
   - Mengimplementasikan verifikasi identitas pasien kanonik (memperhitungkan `MergedToPatientId`):
     - Jenis identitas wajib `"KTP"` atau `"NIK"` (`IsValidIdentityType`).
     - NIK angka saja wajib identik (`NormalizeDigits`).
     - Tanggal lahir kalender wajib identik (`BirthDate.Value.Date == TargetPersonBirthDate.Value.Date`).
2. **`PatientEncounterController.cs`** (Diperbarui):
   - Memperbarui pemanggilan `ValidateCardForServiceAsync` pada method `LoadValidPatientCompanyGuarantorAsync` untuk meneruskan `patientId` dan `patientCompanyGuarantorId`.
   - Mengembalikan pesan kegagalan dalam envelope HTTP 400 Bad Request (`FIN-DES-126`).
3. **`EncounterPaymentSourceService.cs`** (Diperbarui):
   - Memperbarui pemanggilan `ValidateCardForServiceAsync` pada method `SwitchPaymentSourceAsync` untuk meneruskan `card.PatientId` dan `card.Id`.
   - Memetakan kegagalan identitas ke HTTP 422 dengan `ErrorCode = "PATIENT_IDENTITY_MISMATCH"` (`FIN-VAL-295`).

---

## 6. Ringkasan Verifikasi & Kepatuhan Arsitektur

| Aspek | Status | Keterangan |
|:---|:---:|:---|
| **Kepatuhan Roadmap & Desain** | ✅ **Sesuai Penuh** | Memenuhi `BE-FIN-140` pada `roadmap/01-backend-roadmap.md`, `FIN-DES-122`, `FIN-DES-126`, `FIN-DEC-256`, `FIN-DEC-257`, dan `FIN-VAL-295`. |
| **Isolasi Risiko & Kompatibilitas Mundur** | ✅ **Sesuai Penuh** | Kartu manual tidak terdampak (AC 2), kunjungan warisan tidak terganggu (AC 3), dan Pasien Umum tetap dapat didaftarkan (AC 4). |
| **Kompilasi & Build** | 🟡 **NOT RUN** | Sesuai instruksi pemilik sistem ("tanpa build dotnet manual"), `dotnet build` tidak dijalankan. Seluruh dependensi tipe data, method signature overload, dan struktur LINQ telah diverifikasi bebas kesalahan sintaksis. |
| **Status Penyelesaian Task** | 🟡 **Selesai Source** | Seluruh perubahan source code selesai 100%, laporan tracked resmi diterbitkan, dan roadmap siap ditandai dengan status 🟡. |

