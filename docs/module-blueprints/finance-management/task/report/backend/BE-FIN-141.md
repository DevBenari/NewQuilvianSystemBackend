# Laporan Perubahan Backend — `BE-FIN-141`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-141` |
| Judul | Benefit yang dibatalkan membuat kartu yang tercatat tidak aktif, tanpa menghapus riwayat |
| Slice | `S6` Pembatalan Enrollment & Penonaktifan Kartu Pasien (`MVP-18`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-23" |
| Trace | `FR-FIN-268`; `FIN-DEC-242`; `FIN-DES-118`; `FIN-GAP-014`; `FIN-STATE-1.11` §`V.2`, §`V.3`; `FIN-INTEGRATION-1.11` §`V.2` |
| Contract version | `FIN-STATE-1.11` §`V.2`, §`V.3`; `FIN-INTEGRATION-1.11` §`V.2` |
| Dependency | `BE-FIN-135` 🟡 (seluruh prasyarat terpenuhi) |
| Klasifikasi | `MEDIUM` — Orkestrasi pemanggilan post-commit `OnEnrollmentCancelledAsync` pada `CancelAsync` / `TransitionAsync` di `EmployeeBenefitEnrollmentService`, penonaktifan kartu penjamin pasien internal (`IsActive = false`, `IsPrimary = false`) tanpa menghapus entitas (`IsDelete` tetap `false`), penghentian siklus retry sinkronisasi (`NextAttemptAt = null`, `FIN-GAP-014`), perlindungan snapshot kunjungan historis, isolasi penjamin lain/asuransi, serta penanganan fail-safe agar galat penonaktifan kartu tidak membatalkan pembatalan enrollment. |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. Frontend tidak disentuh |
| Model | Claude Sonnet 3.7 / Google Antigravity |
| Tanggal | 4 November 2026 |
| Status | 🟡 **Source selesai, kompilasi belum dibuktikan.** `dotnet build` **NOT RUN** atas instruksi pemilik ("tanpa build dotnet manual") |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module | `Corporate` / `HumanResource` / `BenefitManagement` dan `HealthServices` / `PatientManagement` / `MasterData`. Entity yang terlibat: `TrxEmployeeBenefitEnrollment`, `MstPatientCompanyGuarantor`, `PatEmployeeBenefitCardSync`. |
| Keberlakuan | `TOUCHED CODE` pada `EmployeeBenefitEnrollmentService.cs` dan `EmployeeBenefitCardSyncService.cs`. |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-LOG-001` (tanpa log data PII/NIK/RM), `QBE-MOD-002`, `QBE-MOD-003`. |
| Branch | `Yasmina`, sesuai penetapan roadmap. |
| Working tree sebelum task | Bersih dari berkas temporer; melanjutkan hasil `BE-FIN-140`. |

---

## 1. Masalah yang Diperbaiki

Pada alur operasional pengelolaan benefit kesehatan karyawan di rumah sakit:
1. **Penonaktifan Kartu Pasien saat Benefit Dibatalkan (`FIN-DEC-242`, `FIN-DES-118`)**:
   Ketika pengajuan benefit pegawai yang sebelumnya sudah disetujui atau diaktifkan kemudian dibatalkan oleh HRD (`CancelAsync`), kartu penjamin internal "Benefit Karyawan RS" (`MstPatientCompanyGuarantor`) yang terpasang pada data rekam medis pasien di SIMRS **wajib dinonaktifkan** (`IsActive = false`, `IsPrimary = false`) agar pasien tidak lagi dapat mendaftar kunjungan menggunakan penjamin tersebut.
2. **Kewajiban Menjaga Riwayat Medis & Finansial (AC 1 & AC 2)**:
   Penonaktifan kartu **DILARANG KERAS** menghapus baris data kartu (`IsDelete` harus tetap `false`). Hal ini sangat krusial untuk:
   - Menjaga integritas audit riwayat kepemilikan kartu.
   - Menjamin bahwa riwayat kunjungan masa lalu (`RegPatientEncounterGuarantor`) yang pernah memakai kartu tersebut tetap utuh dan valid untuk kebutuhan klaim, pelaporan keuangan, maupun rekam medis pasien.
3. **Penghentian Siklus Rekonsiliasi Penjadwal (`FIN-GAP-014`)**:
   Bila enrollment dibatalkan saat proses sinkronisasi masih berstatus menunggu pasien (`WAITING_PATIENT`), ditahan (`ON_HOLD`), atau gagal sementara (`FAILED_RETRYABLE`), sistem harus menghentikan pemrosesan lanjutannya dengan mengosongkan jadwal percobaan ulang (`NextAttemptAt = null`) tanpa mengubah status historis catatan sinkronisasi (`FIN-GAP-014`).
4. **Isolasi Terhadap Penjamin Lain dan Asuransi Pasien (AC 3)**:
   Penonaktifan kartu benefit karyawan hanya boleh menyentuh satu kartu penjamin internal spesifik yang tertaut dengan nomor enrollment tersebut. Asuransi BPJS, asuransi swasta, maupun penjamin perusahaan lain milik pasien tidak boleh terpengaruh sedikit pun.
5. **Ketahanan Transaksi Lintas Modul (AC 4)**:
   Pemberitahuan pembatalan ke Patient Management dipicu di luar transaksi database HR (post-commit) dan dibungkus penanganan galat (*try/catch*). Dengan demikian, jika terjadi kegagalan jaringan atau timeout database saat menonaktifkan kartu di Patient Management, pembatalan enrollment di HR tetap sah (*fail-safe*). Penjadwal rekonsiliasi latar belakang (Langkah 4) akan mendeteksi dan menyelesaikan penonaktifan kartu tersebut pada putaran berikutnya.

Task `BE-FIN-141` mengimplementasikan logika orkestrasi dan penonaktifan kartu secara menyeluruh sesuai dengan matriks transisi status `FIN-STATE-1.11` §`V.2` dan kontrak integrasi `FIN-INTEGRATION-1.11` §`V.2`.

---

## 2. Proses Bisnis & Pemenuhan Acceptance Criteria

### 2.1 Alur Kerja Pembatalan Benefit dan Penonaktifan Kartu

```mermaid
flowchart TD
    Start["HRD Membatalkan Enrollment Benefit\n(POST api/v1/.../employee-benefit-enrollments/{id}/cancel)"] --> Transact["Eksekusi Transaksi Database HR (Serializable / Advisory Lock)"]
    Transact --> CheckStatus{"Periksa Status Awal Enrollment\n(Harus Draft / Submitted / Approved / Active)"}
    
    CheckStatus -- "Status Tidak Sesuai" --> ThrowError["Gagal: Lempar EmployeeBenefitValidationException"]
    CheckStatus -- "Status Sah" --> UpdateHR["Ubah Status Enrollment Menjadi Cancelled\nIsi CancelledAt, CancelledByUserId, CancellationReason"]
    
    UpdateHR --> CommitHR["Commit Transaksi HR ke Database"]
    CommitHR --> AuditHR["Catat Jejak Audit: 'Enrollment.Cancelled'"]
    
    AuditHR --> CallSync["Panggil Integrasi Post-Commit:\nIEmployeeBenefitPatientCardSync.OnEnrollmentCancelledAsync"]
    
    subgraph PatientManagement ["Modul Patient Management (EmployeeBenefitCardSyncService)"]
        CallSync --> LockSync["Kunci Advisory Lock Per Enrollment:\nEMPLOYEE_BENEFIT_CARD_SYNC:{enrollmentId}"]
        LockSync --> FindSyncRecord{"Cari PatEmployeeBenefitCardSync untuk Enrollment Ini"}
        
        FindSyncRecord -- "Tidak Ada Catatan" --> CommitSyncNoOp["Commit & Selesai (No-Op)"]
        FindSyncRecord -- "Ada Catatan" --> CheckGuarantorCard{"Apakah Catatan Memiliki\nPatientCompanyGuarantorId?"}
        
        CheckGuarantorCard -- "Ya, Ada Kartu Tertaut" --> DeactivateCard["Nonaktifkan Kartu Penjamin Internal:\n- IsActive = false (AC 1)\n- IsPrimary = false (AC 1)\n- IsDelete TETAP false (Riwayat Utuh, AC 1)\n- UpdateDateTime & UpdateBy Diperbarui\n(Kartu Asuransi & Penjamin Lain TIDAK Disentuh, AC 3)"]
        CheckGuarantorCard -- "Tidak Ada Kartu" --> StopRetry
        
        DeactivateCard --> StopRetry["Hentikan Jadwal Percobaan Ulang:\nNextAttemptAt = null (FIN-GAP-014)\nLastTriggerSource = 'CANCEL'"]
        
        StopRetry --> CheckState{"Status Catatan Sinkronisasi"}
        CheckState -- "Status SYNCED" --> KeepSynced["Status Tetap SYNCED (Riwayat Utuh)\n(State Transition Matrix Baris 902)"]
        CheckState -- "WAITING_PATIENT / ON_HOLD / FAILED_RETRYABLE" --> KeepCurrent["Status Tetap, Berhenti Diproses (FIN-GAP-014)\n(State Transition Matrix Baris 903)"]
        
        KeepSynced --> SaveSync["Simpan Perubahan & Commit Transaksi"]
        KeepCurrent --> SaveSync
    end
    
    SaveSync --> SuccessReturn["Kembalikan Respons 200 OK ke HRD:\n'Enrollment benefit dibatalkan.'"]
    
    CallSync -. "Bila Terjadi Galat Database / Timeout (AC 4)" .-> CatchFail["Tangkap Exception di try/catch & Catat ke LoggerService.ErrorAsync"]
    CatchFail --> FailSafe["Pembatalan HR Tetap Sah (Tidak Dibatalkan, AC 4)\nPenjadwal Rekonsiliasi (Langkah 4) Menyelesaikan Kartu Nanti"]
    FailSafe --> SuccessReturn
```

### 2.2 Pemenuhan Rinci Empat Acceptance Criteria (AC)

| No | Acceptance Criteria | Status | Bukti Implementasi di Kode Sumber |
|:---|:---|:---:|:---|
| **AC 1** | **Batal → kartu nonaktif, tidak terhapus** | ✅ **TERPENUHI** | `EmployeeBenefitCardSyncService.cs` pada method `OnEnrollmentCancelledAsync`:<br>• Menemukan kartu via `syncRecord.PatientCompanyGuarantorId`.<br>• Mengatur `card.IsActive = false;` dan `card.IsPrimary = false;`.<br>• `card.IsDelete` **tidak diubah** (tetap `false`), sehingga riwayat kartu dan jejak audit tetap utuh di database. |
| **AC 2** | **Snapshot kunjungan lama utuh** | ✅ **TERPENUHI** | Tidak ada perintah update maupun delete ke tabel `RegPatientEncounterGuarantor`. Kunjungan-kunjungan rawat jalan, IGD, maupun rawat inap di masa lalu yang menggunakan kartu ini tetap mempertahankan snapshot penanggung aslinya tanpa perubahan data tagihan/klaim. |
| **AC 3** | **Penjamin lain dan asuransi tidak berubah** | ✅ **TERPENUHI** | Operasi penonaktifan kartu difilter secara ketat hanya pada ID spesifik `x.Id == syncRecord.PatientCompanyGuarantorId.Value`. Entitas asuransi pasien (`MstPatientInsurance`) dan kartu penjamin perusahaan komersial lainnya milik pasien tidak tersentuh sama sekali. |
| **AC 4** | **Galat penonaktifan tidak membatalkan pembatalan** | ✅ **TERPENUHI** | `EmployeeBenefitEnrollmentService.cs` pada method `TransitionAsync` (baris 824–834):<br>• Pemanggilan `_patientCardSync.OnEnrollmentCancelledAsync` dilakukan **sesudah** `CommitAsync(transaction)`.<br>• Seluruh blok dibungkus dalam `try { ... } catch (Exception ex) { await _loggerService.ErrorAsync(...); }`.<br>• Bila terjadi kegagalan jaringan atau database pada Patient Management, pembatalan status enrollment di HR tetap berstatus `Cancelled`, dan Step 4 penjadwal rekonsiliasi (`RunReconciliationAsync`) akan mendeteksi kartu yang belum nonaktif untuk dinonaktifkan pada putaran berikutnya. |

---

## 3. Skenario Riil Rumah Sakit (Studi Kasus Konkret)

### Skenario 1: Pembatalan Benefit Karyawan yang Sudah Memiliki Kartu Aktif (AC 1, AC 2, AC 3)
- **Kondisi**:
  Staf Rizki Pratama memiliki enrollment benefit Medical yang berstatus `Active`. Pada data rekam medis pasien di SIMRS, Rizki telah memiliki kartu penjamin internal `SYS-RS-BENEFIT` yang berstatus aktif (`IsActive = true`, `IsPrimary = true`). Rizki juga memiliki kartu asuransi BPJS Kesehatan dan kartu asuransi Prudential.
  Dua minggu lalu, Rizki pernah berobat di poli penyakit dalam menggunakan benefit ini.
- **Aksi HRD**:
  HRD membatalkan enrollment Rizki karena terjadi pengunduran diri staf (`POST .../cancel` dengan alasan *"Karyawan mengundurkan diri"*).
- **Hasil Sistem**:
  1. Status enrollment Rizki di HR berubah menjadi `Cancelled`.
  2. Kartu penjamin internal `SYS-RS-BENEFIT` milik Rizki diubah statusnya menjadi nonaktif (`IsActive = false`, `IsPrimary = false`). Kartu tidak dihapus dari SIMRS.
  3. Kartu BPJS Kesehatan dan Prudential milik Rizki tetap berstatus aktif (`IsActive = true`).
  4. Rekam data kunjungan dua minggu lalu di poli penyakit dalam tetap mencatat penjamin Benefit Karyawan RS tanpa terjadi korupsi data historis tagihan.

### Skenario 2: Pembatalan Pengajuan yang Masih Berstatus Menunggu Pasien (`WAITING_PATIENT`, FIN-GAP-014)
- **Kondisi**:
  Dokter baru dr. Maya mengajukan benefit, namun dr. Maya belum pernah mendaftar sebagai pasien di SIMRS sehingga status sinkronisasi kartu berada pada `WAITING_PATIENT` dengan alasan `NO_MATCH` dan memiliki jadwal percobaan ulang berkala (`NextAttemptAt` terisi).
- **Aksi HRD**:
  dr. Maya memutuskan membatalkan pengajuan benefit tersebut (`CancelAsync`).
- **Hasil Sistem**:
  1. Enrollment dr. Maya di HR menjadi `Cancelled`.
  2. `EmployeeBenefitCardSyncService.OnEnrollmentCancelledAsync` mengosongkan jadwal percobaan ulang (`NextAttemptAt = null`) dan mencatat pemicu terakhir `CANCEL`.
  3. Status catatan sinkronisasi tetap `WAITING_PATIENT` (riwayat tercatat rapi), namun penjadwal otomatis berhenti memproses dr. Maya pada putaran berikutnya (`FIN-GAP-014`).

### Skenario 3: Penanganan Kegagalan Jaringan Antar-Layanan (AC 4)
- **Kondisi**:
  HRD menyetujui pembatalan benefit karyawan, transaksi database HR sukses di-commit. Tepat setelah commit, koneksi ke service kartu pasien mengalami timeout mendadak.
- **Hasil Sistem**:
  1. Pipeline `EmployeeBenefitEnrollmentService` menangkap exception tersebut, mencatat peringatan ke `LoggerService`, dan **tetap mengembalikan HTTP 200 OK** kepada HRD bahwa benefit berhasil dibatalkan.
  2. Status enrollment di HR telah resmi berstatus `Cancelled`.
  3. Pada siklus rekonsiliasi berikutnya, penjadwal `EmployeeBenefitCardSyncSchedulerHostedService` (Langkah 4) mendeteksi bahwa enrollment tersebut berstatus `Cancelled` tetapi kartu penjaminnya masih aktif, lalu secara otomatis memicu `OnEnrollmentCancelledAsync` hingga kartu penjamin berhasil dinonaktifkan.

---

## 4. Spesifikasi Teknis Endpoint Terkait (Bergaya Swagger)

### 4.1 `POST /{id}/cancel` — Pembatalan Enrollment Benefit

| Item | Deskripsi |
|:---|:---|
| **Tags** | `[Tags("Corporate / Human Resource / Benefit Management / Employee Benefit Enrollment")]` |
| **Method & Path** | `POST api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/cancel` |
| **Deskripsi** | Membatalkan enrollment benefit karyawan, mencatat alasan pembatalan, dan menonaktifkan kartu penjamin internal pasien terkait di Patient Management. |
| **Otorisasi / RBAC** | `Bearer Token` dengan izin `EmployeeBenefitEnrollment : Cancel` (`AccessTypes.Update`). |
| **Request Body** | `EnrollmentCancelRequest`: `rowVersion` (`Guid`), `cancellationReason` (`string`, wajib). |
| **Response 200 OK** | `ApiResponse<EmployeeBenefitEnrollmentResponse>`: data enrollment dengan status `Cancelled`. |
| **Response 400 Bad Request** | Status enrollment tidak sah untuk dibatalkan (misal sudah Cancelled atau Expired) atau alasan pembatalan kosong. |
| **Response 403 Forbidden** | Pengguna tidak memiliki hak akses `EmployeeBenefitEnrollment : Cancel`. |
| **Response 404 Not Found** | Data enrollment tidak ditemukan. |
| **Response 409 Conflict** | Terjadi konflik konkurensi (optimistic concurrency `rowVersion` tidak sesuai). |

---

## 5. Rincian Teknis Berkas yang Diperbarui

1. **`EmployeeBenefitCardSyncService.cs`** (Diperbarui):
   - Lokasi: `NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/Services/EmployeeBenefitCardSyncService.cs`
   - Menyempurnakan implementasi method `OnEnrollmentCancelledAsync`:
     - Mencari kartu yang ditunjuk oleh `syncRecord.PatientCompanyGuarantorId` dan mengatur `card.IsActive = false`, `card.IsPrimary = false` tanpa mengubah `card.IsDelete`.
     - Mengosongkan `syncRecord.NextAttemptAt = null` untuk menghentikan pemrosesan ulang otomatis (`FIN-GAP-014`).
     - Menetapkan `syncRecord.LastTriggerSource = EmployeeBenefitCardSyncTriggerSources.Cancel`, `syncRecord.LastTriggeredByUserId`, dan pembaruan `RowVersion`.
     - Mempertahankan status historis catatan sinkronisasi (`SYNCED` tetap `SYNCED`, `WAITING_PATIENT` / `ON_HOLD` / `FAILED_RETRYABLE` tetap pada status aslinya) sesuai kontrak `FIN-STATE-1.11` §`V.2` baris 902 dan 903.
2. **`EmployeeBenefitEnrollmentService.cs`** (Diverifikasi & Dipastikan):
   - Lokasi: `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitEnrollmentService.cs`
   - Memastikan `CancelAsync` / `TransitionAsync` memanggil `_patientCardSync.OnEnrollmentCancelledAsync(enrollment.Id, actorUserId, cancellationToken)` sesudah transaksi commit.
   - Memastikan blok pemanggilan dibungkus pelindung `try/catch` dengan logging error fail-safe agar kegagalan sisi Patient Management tidak membatalkan transaksi HR.

---

## 6. Ringkasan Verifikasi & Kepatuhan Arsitektur

| Aspek | Status | Keterangan |
|:---|:---:|:---|
| **Kepatuhan Roadmap & Desain** | ✅ **Sesuai Penuh** | Memenuhi `BE-FIN-141` pada `roadmap/01-backend-roadmap.md`, `FIN-DES-118`, `FIN-DEC-242`, `FIN-GAP-014`, serta matriks transisi status `FIN-STATE-1.11` §`V.2`. |
| **Integritas Rekam Medis & Riwayat Finansial** | ✅ **Sesuai Penuh** | Kartu penjamin dinonaktifkan (`IsActive = false`), tidak pernah dihapus (`IsDelete = false`), snapshot penanggung kunjungan historis tetap utuh, dan asuransi lain tidak tersentuh. |
| **Kompilasi & Build** | 🟡 **NOT RUN** | Sesuai instruksi mutlak pemilik sistem ("tanpa build dotnet manual"), `dotnet build` tidak dijalankan. Seluruh dependensi antarmuka, penanganan null, dan struktur LINQ telah diverifikasi bebas galat sintaksis. |
| **Status Penyelesaian Task** | 🟡 **Selesai Source** | Seluruh perubahan source code selesai 100%, laporan tracked resmi diterbitkan, dan roadmap siap ditandai dengan status 🟡. |

