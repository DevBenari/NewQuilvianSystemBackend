# Modul Operasi — Peta Kemampuan yang Sudah Ada

| Field | Nilai |
|---|---|
| Blueprint ID | `operations` |
| Revision | `2` |
| Status | `audited-current` |
| Decision input | `00-interview-decisions.md`, revision 5 |
| Backend branch/SHA | `Ikbal` / `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend branch/SHA | `Ikbalv2` / `400104f2a0f3239c14c40f5905b419977a538450` |
| Contract version | Capability Evidence Contract dari skill `trace-existing-capabilities` |

## Kesimpulan

Belum ada Modul Operasi yang dapat dijalankan dari awal sampai akhir. Backend sudah menyediakan fondasi pasien, encounter, dokter, unit layanan, ruang operasi, katalog tindakan, consent operasi/anestesi, tarif, dan tindakan pasien. Fondasi tersebut harus digunakan ulang agar Modul Operasi tidak membuat data pasien, dokter, ruang, tindakan, atau tarif kedua.

Yang masih belum tersedia adalah inti workflow kamar operasi: permintaan operasi khusus, jadwal yang mengunci ruang dan seluruh tim, persiapan/checklist, pencatatan anestesi dan pelaksanaan operasi, pemakaian bahan/implant, recovery, serah terima, catatan operasi final beserta addendum, serta laporan operasi.

Frontend belum mempunyai route atau halaman Operasi yang dapat dipakai. File `dataOperasi.jsx` berisi data statis, sedangkan `status-operasi.jsx` menyatakan dirinya sebagai komponen placeholder. Keduanya bukan bukti workflow yang berjalan.

## Batas Audit

Audit dibatasi pada kemampuan yang diperlukan oleh keputusan `OPS-DEC-001` sampai `OPS-DEC-012`:

1. Identitas pasien dan episode/encounter.
2. Dokter, tim, unit layanan, dan ruang operasi.
3. Katalog tindakan, tindakan pasien, consent, dan status klinis.
4. Penjadwalan sumber daya.
5. Dokumentasi operasi, anestesi, checklist, recovery, dan serah terima.
6. Pemakaian obat, bahan, implant, serta integrasi stok dan billing.
7. Otorisasi, audit, frontend, dan laporan.

Audit tidak menguji database berjalan, isi data rumah sakit, deployment, atau integrasi eksternal. Source aplikasi hanya dibaca dan tidak diubah.

Impact scan terbatas setelah decision revision 5 tidak mengubah klasifikasi capability. Klarifikasi `Completed`, relasi satu atau lebih tindakan, handover, dan addendum mempertegas gap yang sudah tercatat pada `OPS-CAP-005`, `OPS-CAP-011`, `OPS-CAP-013`, dan `OPS-CAP-014`.

## Capability Map

| ID | Kebutuhan | Pemilik existing | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `OPS-CAP-001` | Identitas pasien dan encounter asal | Patient/Registration Management | `NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs#TrxPatientEncounter@767470f` dan `PatientEncounterController.cs#PatientEncounterController@767470f` | Ready to reuse | Modul Operasi cukup menyimpan referensi encounter dan pasien | Duplikasi encounter akan memecah rekam perjalanan pasien |
| `OPS-CAP-002` | Master unit dan ruang operasi | Health Services Master Data | `ServiceUnitType.cs#OperatingRoom@767470f`, `RoomType.cs#OperatingRoom@767470f`, `RoomController.cs#RoomController@767470f` | Ready to reuse | Filter unit/ruang aktif bertipe `OperatingRoom` | Status aktif master belum berarti ruang bebas pada jam tertentu |
| `OPS-CAP-003` | Identitas dokter dan tenaga klinis | HR Workforce | `DoctorController.cs#DoctorController@767470f` | Reuse with adapter | Gunakan dokter existing untuk dokter bedah/anestesi; anggota tim lain perlu resolver workforce sesuai peran | Belum terbukti bahwa privilege klinis operasi divalidasi saat penugasan |
| `OPS-CAP-004` | Katalog tindakan operasi | Health Services Master Data | `MstProcedure.cs#IsSurgery@767470f`, `ProcedureController.cs#ProcedureController@767470f` | Ready to reuse | Pilih procedure aktif dengan `IsSurgery=true` | Master tindakan tidak mewakili satu episode operasi pasien |
| `OPS-CAP-005` | Permintaan/tindakan operasi pasien | Clinical Management | `TrxPatientProcedure.cs#IsSurgeryRelated/ProcedureStatus@767470f`, `PatientProcedureController.cs#approve/execute/cancel@767470f` | Extend | Gunakan `TrxPatientProcedure` sebagai referensi order klinis, lalu hubungkan ke aggregate Operasi; jangan memaksakan seluruh workflow perioperatif ke status generik | Status generik tidak memuat kesiapan bersama, ruang, tim, recovery, atau serah terima |
| `OPS-CAP-006` | Persetujuan operasi dan anestesi | Clinical Management | `PatientConsentType.cs#Surgery/Anesthesia@767470f`, `TrxPatientConsent.cs#TrxPatientConsent@767470f`, `PatientConsentController.cs#sign/verify/approve@767470f` | Reuse with adapter | Tautkan consent operasi dan anestesi ke permintaan operasi dan cek status valid sebelum mulai | Jalur darurat dan kewajiban pelengkapan pascatindakan perlu kontrak Operasi |
| `OPS-CAP-007` | Kalender operasi dan pencegahan benturan ruang/tim | Belum ada | `MstDoctorSchedule.cs#MstDoctorSchedule@767470f` mewajibkan `ClinicId` dan berisi kuota praktik; bukan kalender operasi | Missing | Dibutuhkan jadwal operasi berbasis kasus, rentang waktu, ruang, dan beberapa anggota tim dengan pemeriksaan benturan atomik | Memakai jadwal praktik akan menghasilkan konflik sumber daya yang tidak terdeteksi |
| `OPS-CAP-008` | Checklist dan persetujuan kesiapan bersama | Belum ada | Tidak ditemukan model/controller Health Services untuk checklist perioperatif pada SHA audit | Missing | Dibutuhkan checklist versi rumah sakit serta sign-off dokter bedah, dokter anestesi, dan perawat | Operasi dapat dimulai tanpa bukti kesiapan lengkap |
| `OPS-CAP-009` | Jalur darurat dan penggeseran jadwal elektif | Belum ada | Tidak ditemukan lifecycle Operasi khusus pada SHA audit | Missing | Catat alasan bypass, bagian yang tertunda, penanggung jawab, prioritas, dan riwayat reschedule | Audit klinis tidak dapat membedakan bypass sah dari kelalaian |
| `OPS-CAP-010` | Catatan anestesi dan pemantauan intraoperatif | Belum ada | `PatientConsentType.Anesthesia` hanya consent; tidak ditemukan anesthesia record pada SHA audit | Missing | Dibutuhkan record anestesi, pelaksana, waktu, observasi, obat, dan kejadian | Consent anestesi tidak boleh dianggap sebagai catatan pemberian anestesi |
| `OPS-CAP-011` | Catatan operasi final dan addendum | Clinical documentation parsial | `TrxPatientProcedure.cs#ClinicalNote/ResultNote/ComplicationNote@767470f` | Extend | Catatan singkat dapat menjadi referensi, tetapi dibutuhkan dokumen operasi yang dapat disahkan dan hanya dikoreksi melalui addendum | Edit langsung dapat menghilangkan isi catatan klinis sebelumnya |
| `OPS-CAP-012` | Pemakaian obat, bahan, alat, dan implant | Pharmacy/Master Data parsial | `MstDrugStorageLocation.cs#OperatingRoom@767470f`; tidak ditemukan transaksi pemakaian implant Operasi | Missing | Operasi mencatat pemakaian aktual, batch/serial dan pengguna; stok tetap diproses pemilik persediaan | Tidak ada traceability implant dan stok dapat berbeda dari pemakaian pasien |
| `OPS-CAP-013` | Recovery dan izin keluar ruang pemulihan | Belum ada | Tidak ditemukan recovery record atau keputusan discharge anestesi pada SHA audit | Missing | Dibutuhkan observasi recovery, keputusan dokter anestesi, tujuan, waktu, dan kondisi pasien | Pasien dapat berpindah tanpa bukti kelayakan dan serah terima |
| `OPS-CAP-014` | Serah terima pasien | Belum ada | Tidak ditemukan handoff perioperatif pada SHA audit | Missing | Catat unit asal/tujuan, pemberi, penerima, waktu, kondisi, dan instruksi | Tanggung jawab klinis saat perpindahan menjadi ambigu |
| `OPS-CAP-015` | Tarif tindakan operasi | Health Services Master Data | `MstTariff.cs#IsSurgeryRelated@767470f`, `TariffController.cs#TariffController@767470f` | Ready to reuse | Referensikan tarif aktif; jangan membuat master tarif Operasi baru | Tarif master belum membuktikan bahwa transaksi billing sudah terbentuk |
| `OPS-CAP-016` | Transaksi tagihan operasi | Billing/Clinical parsial | `TrxPatientProcedure.cs#BillingItemId/IsBillingGenerated@767470f`; tidak ditemukan aggregate transaksi billing yang dimiliki Modul Operasi | Unknown | Kontrak handoff billing dan idempotency harus ditetapkan setelah capability Billing transaksi tersedia/diaudit | Tagihan dapat hilang atau tercatat ganda |
| `OPS-CAP-017` | Otorisasi aktor sesuai keputusan | Shared authorization parsial | Controller existing memakai `AccessPermission`; belum ada permission khusus Operasi | Missing | Dibutuhkan permission terpisah untuk pemohon, koordinator, bedah, anestesi, perawat, pembatalan, pengesahan, dan addendum | Permission generik `Update` terlalu luas untuk keputusan klinis sensitif |
| `OPS-CAP-018` | Dashboard, halaman workflow, dan laporan Operasi | Frontend belum tersedia | `QuilvianSystemFrontendDev/src/utils/dataOperasi.jsx#dataruangOperasi@400104f`, `src/components/features/status/status-operasi.jsx#StatusOperasiBaseComponent@400104f` | Missing | Bangun route dan state nyata setelah kontrak backend terkunci; data statis bukan sumber data | UI dapat terlihat selesai walaupun tidak terhubung ke workflow backend |
| `OPS-CAP-019` | Konsumen frontend tindakan pasien | Frontend Clinical Management | `src/lib/services/health-services/clinical-management/patient-procedure.service.js#BASE_URL@400104f` | Reuse with adapter | Service hanya meliputi baca/select/remove draft; tindakan workflow lain belum dikonsumsi | Frontend dan backend belum mempunyai perjalanan Operasi end-to-end |

## Perjalanan Ujung-ke-Ujung As-Is

### Yang sudah dapat dilakukan

1. Admin dapat menyiapkan unit layanan dan ruang bertipe kamar operasi.
2. Admin dapat menyiapkan dokter, katalog tindakan yang ditandai sebagai operasi, dan tarif terkait operasi.
3. Dalam konteks konsultasi, backend dapat membuat tindakan pasien yang ditandai terkait operasi.
4. Backend dapat mencatat consent bertipe operasi atau anestesi dan mengubahnya melalui status tanda tangan, verifikasi, serta persetujuan.

### Titik berhenti

Setelah order tindakan dan consent tersedia, belum ada proses yang membuat kasus masuk ke daftar Operasi, memilih ruang dan tim tanpa benturan, menjalankan checklist, mencatat operasi/anestesi, memproses recovery, atau menyerahkan pasien. Karena itu kemampuan existing belum membentuk Modul Operasi.

**Contoh:** Dokter membuat tindakan “Apendektomi” dan consent sudah `Approved`. Sistem existing dapat menyimpan kedua catatan tersebut. Namun belum ada kalender yang memastikan Ruang Operasi 1, dokter bedah, dokter anestesi, dan perawat semuanya tersedia pukul 10.00–12.00.

## Kontrak API As-Is yang Material

### Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Membaca tindakan pasien | `PatientProcedure : Read` | Query filter/paging | Daftar `PatientProcedureResponse` |
| `POST` | `/` | Membuat tindakan pasien | `PatientProcedure : Create` | `CreatePatientProcedureRequest` | Data tindakan yang dibuat |
| `PATCH` | `/{id}/approve` | Menyetujui tindakan | `PatientProcedure : Update` | Body persetujuan | Status tindakan terbaru |
| `PATCH` | `/{id}/execute` | Menandai tindakan dijalankan | `PatientProcedure : Update` | Body eksekusi | Status tindakan terbaru |
| `PATCH` | `/{id}/cancel` | Membatalkan tindakan | `PatientProcedure : Update` | Alasan pembatalan | Status tindakan terbaru |

Endpoint generik ini belum menerapkan kewenangan khusus dokter bedah/anestesi dari decision log Operasi.

### Health Services / Clinical Management / Patient Consent

Base URL: `api/v1/health-services/clinical-management/patient-consents`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/` | Membuat consent operasi atau anestesi | `PatientConsent : Create` | `CreatePatientConsentRequest` | Consent yang dibuat |
| `PATCH` | `/{id}/sign` | Mencatat penandatanganan | `PatientConsent : Update` | Data tanda tangan | Status consent terbaru |
| `PATCH` | `/{id}/verify` | Memverifikasi consent | `PatientConsent : Update` | Catatan verifikasi | Status consent terbaru |
| `PATCH` | `/{id}/approve` | Menyetujui consent | `PatientConsent : Update` | Catatan persetujuan | Status consent terbaru |
| `PATCH` | `/{id}/withdraw` | Menarik persetujuan | `PatientConsent : Update` | Alasan penarikan | Status consent terbaru |

### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/options` | Memilih ruang aktif, termasuk kamar operasi | `Room : Read` | Query filter | Daftar pilihan ruang |
| `POST` | `/` | Membuat master ruang | `Room : Create` | `CreateRoomRequest` | Ruang yang dibuat |
| `PUT` | `/{id}` | Memperbarui master ruang | `Room : Update` | `UpdateRoomRequest` | Ruang terbaru |

### Health Services / Master Data / Doctor Schedule

Base URL: `api/v1/health-services/master-data/doctor-schedules`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/options` | Membaca pilihan jadwal praktik dokter | `DoctorSchedule : Read` | Query filter | Daftar jadwal praktik |
| `POST` | `/` | Membuat jadwal praktik dokter | `DoctorSchedule : Create` | `CreateDoctorScheduleRequest` | Jadwal praktik yang dibuat |

Endpoint ini bukan kalender operasi karena request mewajibkan klinik dan berisi kuota appointment/walk-in/kiosk.

## Konflik dan Unknown

1. **Jadwal dokter versus jadwal operasi:** nama “Doctor Schedule” terlihat relevan, tetapi perilakunya mengatur praktik klinik. Menggunakannya sebagai jadwal operasi akan menjadi konflik domain.
2. **Billing transaksi:** field penanda billing ada pada tindakan pasien, tetapi audit terbatas belum menemukan owner transaksi billing yang dapat menerima handoff Operasi. Status tetap `Unknown`.
3. **Privilege klinis:** master dokter tersedia, tetapi enforcement bahwa dokter memiliki privilege bedah/anestesi yang sesuai belum terbukti dalam alur Operasi.
4. **Frontend:** `menuOperasi` hanya dikenali sebagai bentuk nested menu; tidak ditemukan definisi menu dan route Operasi yang dapat dicapai.

## Rekomendasi Berbasis Audit

1. Gunakan ulang pasien/encounter, master ruang, master dokter, procedure, consent, dan tarif.
2. Hubungkan kasus Operasi ke `TrxPatientProcedure`; jangan menduplikasi order tindakan klinis.
3. Rancang lifecycle Operasi baru untuk kebutuhan yang tidak dapat ditampung status tindakan generik.
4. Jangan gunakan `MstDoctorSchedule` sebagai jadwal kasus operasi. Kalender operasi membutuhkan reservasi rentang waktu untuk ruang dan banyak anggota tim.
5. Jangan memakai `dataOperasi.jsx` sebagai kontrak. Frontend dibangun setelah API dan state transition Operasi disetujui.

Dokumen ini tidak menetapkan entity, tabel, endpoint, atau arsitektur target baru.

## Pemicu Impact Scan

Peta ini harus ditandai stale dan diperiksa ulang apabila:

- SHA backend atau frontend berubah;
- Billing transaksi, persediaan/implant, clinical privilege, atau inpatient handoff selesai dikembangkan;
- decision log Operasi direvisi;
- ditemukan branch lain yang menjadi source of truth;
- runtime database menunjukkan konfigurasi yang berbeda dari source.
