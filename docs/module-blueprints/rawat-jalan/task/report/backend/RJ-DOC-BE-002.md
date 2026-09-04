# Laporan Perubahan Backend — `RJ-DOC-BE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-BE-002` |
| Judul | Jadikan authoritative finalization validation mengikat |
| Slice | Doctor / Rawat Jalan Clinical — penyelesaian konsultasi |
| Roadmap | [roadmap/doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `4.1` |
| Trace | `RJ-DOC-DEC-004`; `RJ-DOC-CAP-014` |
| Contract version | `RJ-DOC-COMPLETION-001@1.0.0` — `FROZEN`, **tidak diubah**; bagian `1.5` dan `1.6` |
| Dependency | `RJ-DOC-INT-001` `FROZEN`; `RJ-DOC-BE-001` `COMPLETE` (working tree) |
| Klasifikasi | `MEDIUM` — skor `6`: repository `0`, berkas diperiksa `1`, berkas diubah `1`, logika bisnis `1`, kontrak API `1`, database `1`, keamanan/auth `1`, UI/workflow `0` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/**`, `Tests/QuilvianSystemBackend.Tests/**`, `docs/module-blueprints/rawat-jalan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `801a4f52459e1251ec9bb03c1abfe5e17dd3639c` cabang `sukmagp`, **ditambah working tree `RJ-DOC-BE-001` yang belum di-commit** |
| Tanggal | `2026-08-31` |
| Status | `COMPLETE` |

---

## 1. Masalah yang diperbaiki

`RJ-DOC-BE-001` sudah menyatukan jalur penyelesaian sehingga pemeriksaan kelayakan klinis kini
benar-benar dijalankan. Yang belum diperiksa adalah **keutuhan pesanan klinis itu sendiri**.

Dua keadaan dapat lolos sebelum task ini, dan keduanya tidak terlihat oleh dokter:

1. **Tindakan yang berstatus dibatalkan tetapi barisnya masih aktif.** Penyaring baris aktif hanya
   melihat penanda batal, bukan status tindakannya. Baris seperti ini ikut terhitung sebagai
   tindakan konsultasi dan dibawa ke hilir seolah sah.
2. **Resep atau tindakan yang menempel pada kunjungan yang berbeda dari konsultasinya.** Keduanya
   menyimpan nomor kunjungan dan nomor konsultasi secara terpisah, sehingga keduanya dapat
   berbeda. Akibatnya nyata: fakta klinis yang diterbitkan saat penyelesaian memakai nomor
   kunjungan milik resep, sehingga **tagihan mendarat pada kunjungan pasien yang salah**.

Contoh konkret. Resep pasien A tercatat dengan nomor konsultasi milik pasien A tetapi nomor
kunjungan milik pasien B. Sebelum perbaikan, konsultasi pasien A dapat diselesaikan dan fakta
resepnya dikirim ke kunjungan pasien B. Sesudah perbaikan, penyelesaian ditolak beserta
keterangan baris mana yang bermasalah.

Selain itu, task ini **membuktikan** — bukan sekadar mengasumsikan — bahwa penolakan validasi
tidak meninggalkan jejak apa pun yang terlanjur tersimpan, termasuk catatan antrean dan
penguncian catatan klinis yang ditulis sebelum validasi berjalan.

---

## 2. Proses bisnis

**Tujuan.** Memastikan konsultasi hanya dapat selesai ketika backend menyatakan ia memang layak
selesai.

**Pelaku.** Dokter pemeriksa.

**Pemicu.** Dokter menekan *Selesai Konsultasi*, dari layar antrean maupun dari layar konsultasi.

**Langkah berurutan.**

1. Sistem menjalankan pemeriksaan kelayakan pada satu tempat yang sama untuk kedua jalur.
2. Bila ditemukan **kesalahan**, penyelesaian ditolak beserta daftar apa yang perlu diperbaiki dan
   di tab mana. Tidak ada satu pun perubahan yang tersimpan.
3. Bila hanya ditemukan **peringatan**, penyelesaian tertahan sampai dokter mengakuinya satu per
   satu. Server tidak pernah mengakuinya sendiri.
4. Bila bersih, konsultasi diselesaikan seperti biasa.

**Aturan yang membedakan pembuatan pesanan dari pengerjaannya** — keputusan pemilik
`RJ-DOC-DEC-004`:

| Keadaan | Menahan penyelesaian? |
| --- | --- |
| Pesanan laboratorium sudah tersimpan, specimen belum diambil | **Tidak.** Itu pekerjaan unit laboratorium |
| Pesanan radiologi sudah tersimpan, pemeriksaan belum dikerjakan | **Tidak** |
| Resep sudah sah, obat belum diserahkan farmasi | **Tidak** |
| Tidak ada pesanan penunjang sama sekali | **Tidak.** Keduanya opsional |
| Tindakan berstatus dibatalkan tetapi barisnya masih aktif | **Ya** |
| Resep atau tindakan menempel pada kunjungan yang berbeda | **Ya** |
| Dokumentasi klinis wajib belum lengkap | **Ya** |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`contracts/doctor-consultation-contracts.md`; `roadmap/doctor-consultation-roadmap.md`;
`task/report/backend/RJ-DOC-BE-001.md`; `00-interview-decisions.md`; `MODULE-STATUS.md`;
`ConsultationValidationService.cs`; `PrescriptionValidationService.cs`;
`ConsultationFinalizationService.cs`; `ConsultationFinalizationDtos.cs`;
`DoctorQueueController.cs`; `ClinicalDocumentIntegrityService.cs`; `PatientProcedureStatus.cs`;
`TrxPatientProcedure.cs`; `TrxPrescription.cs`; `TrxPrescriptionItem.cs`; `LabOrder.cs`;
`TrxPatientProcedureConfiguration.cs`; `TrxPrescriptionConfiguration.cs`;
`TrxPrescriptionItemConfiguration.cs`; `MstDoctorConfiguration.cs`;
`Tests/QuilvianSystemBackend.Tests/Infrastructure/**`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationValidationService.cs` | `ValidateProceduresAsync` menerima nomor kunjungan konsultasi dan menambahkan dua pemeriksaan: `INCONSISTENT_PROCEDURE_STATUS` dan `PROCEDURE_ENCOUNTER_MISMATCH`. Keduanya masuk ke dalam perulangan yang sudah ada, sehingga **tidak ada query tambahan** |
| `Areas/HealthServices/PharmacyManagement/Services/PrescriptionValidationService.cs` | `ValidateForConsultationAsync` menerima nomor kunjungan konsultasi dan menambahkan `PRESCRIPTION_ENCOUNTER_MISMATCH`. Pemeriksaan memakai data yang sudah dimuat, sehingga **tidak ada query tambahan** |
| `Tests/QuilvianSystemBackend.Tests/Infrastructure/ControllerTestHarness.cs` | Ditambahkan `BuatHttpContextSuperAdmin` — konteks uji untuk pengujian yang menyasar perilaku sesudah penyaringan kepemilikan data. Bersifat penambahan; metode yang sudah ada tidak disentuh |
| `Tests/QuilvianSystemBackend.Tests/ClinicalManagement/DoctorConsultationValidationTests.cs` | Berkas baru — `14` uji acceptance |

Tidak ada service baru. Tidak ada validator duplikat. `ConsultationValidationService` tetap
menjadi satu-satunya sumber kebenaran, dipakai oleh endpoint pratinjau maupun oleh finalisasi.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada route, DTO, maupun bentuk response yang berubah. Bertambah tiga kode issue di dalam struktur `ConsultationFinalizationIssueResponse` yang sudah dibekukan. Kontrak `RJ-DOC-COMPLETION-001@1.0.0` **tidak diubah** |
| Database | `NOT APPLICABLE`. Tidak ada perubahan schema, tidak ada entity baru, tidak ada migration dibuat maupun diterapkan |
| Keamanan/Auth | Model authorization tidak diubah. Validasi berjalan di server dan tidak dapat dilewati lewat payload: `AcknowledgedWarningKeys` hanya dapat mengakui peringatan, tidak pernah menghilangkan kesalahan. Aktor tetap diambil dari konteks autentikasi. Validator tidak menyentuh satu pun nilai finansial |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru maupun berubah bentuknya. Perilaku validasi berlaku pada dua permukaan
yang sudah ada dan sudah didokumentasikan pada laporan `RJ-DOC-BE-001`:

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/complete` | Menyelesaikan konsultasi; menolak `400` beserta rincian bila belum layak | `DoctorConsultation : Update` |
| `GET` | `/{id}/finalization-validation` | Pratinjau kelayakan; memakai validator yang sama | `DoctorConsultation : Read` |

#### Health Services / Registration Management / Doctor Queue

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/finish-consultation` | Menyelesaikan konsultasi dari layar antrean; memakai validator yang sama, bukan validasi yang lebih longgar | `DoctorQueue : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | Berhasil, `0 Error(s)`; tidak ada peringatan pada berkas yang diubah | `PASS` | Keluaran perintah |
| `dotnet test QuilvianSystemBackend.Tests` — seluruh project | `155` lulus, `0` gagal, `0` dilewati | `PASS` | Keluaran perintah |
| `A` SOAP belum lengkap menahan finalisasi | Empat kode `MISSING_*` terbukti; tanpa transisi state | `PASS` | `SoapBelumLengkap_MenahanFinalisasiTanpaMengubahState` |
| `B` Diagnosis utama belum ada | `MISSING_PRIMARY_DIAGNOSIS` terbukti | `PASS` | `DiagnosisUtamaBelumAda_MenahanFinalisasi` |
| `C` Jumlah tindakan tidak valid | `INVALID_PROCEDURE_QUANTITY` terbukti | `PASS` | `JumlahTindakanTidakValid_MenahanFinalisasi` |
| `C2` Tindakan dibatalkan tetapi masih aktif | `INCONSISTENT_PROCEDURE_STATUS` terbukti | `PASS` | `TindakanBerstatusDibatalkanTetapiMasihAktif_MenahanFinalisasi` |
| `C3` Tindakan menempel kunjungan lain | `PROCEDURE_ENCOUNTER_MISMATCH` terbukti | `PASS` | `TindakanMenempelKunjunganLain_MenahanFinalisasi` |
| `D` Resep kosong | `EMPTY_PRESCRIPTION` terbukti | `PASS` | `ResepKosong_MenahanFinalisasi` |
| `D2` Resep menempel kunjungan lain | `PRESCRIPTION_ENCOUNTER_MISMATCH` terbukti | `PASS` | `ResepMenempelKunjunganLain_MenahanFinalisasi` |
| `E` Peringatan menahan lalu lolos setelah diakui | Tertahan dengan `ErrorCount = 0` dan `RequiresWarningAcknowledgement`; lolos setelah `IssueKey` dikirim | `PASS` | `PeringatanBelumDiakui_MenahanFinalisasiLaluLolosSetelahDiakui` |
| `E2` Acknowledgement `IssueKey` lain tidak meloloskan | Tetap tertahan | `PASS` | `AcknowledgementIssueKeyLain_TidakMeloloskanFinalisasi` |
| `F` Order Lab belum dikerjakan tidak menahan | Konsultasi selesai; order tetap `Requested` | `PASS` | `OrderLabBelumDikerjakan_TidakMenahanFinalisasi` |
| `F2` Tanpa order penunjang tidak menahan | Konsultasi selesai | `PASS` | `TanpaOrderPenunjang_TidakMenahanFinalisasi` |
| `12` Jalur antrean memakai validator sama | `400` beserta payload validasi terstruktur | `PASS` | `JalurAntrean_MemakaiValidatorYangSamaDanMenolakKonsultasiTidakLayak` |
| `G` Penolakan tidak meninggalkan catatan/penguncian | Dokumen tetap draf, catatan antrean tidak tersimpan | `PASS` | `JalurAntreanDitolak_TidakMeninggalkanCatatanMaupunPenguncianDokumen` |
| `12b` Jalur antrean yang layak tetap berhasil | Konsultasi `Completed`, kunjungan `ConsultationCompleted` | `PASS` | `JalurAntrean_KonsultasiLayakTetapBerhasilDiselesaikan` |

Uji manual: `NOT FEASIBLE` — memerlukan lingkungan berjalan beserta frontend.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `dotnet test QuilvianSystemBackend.BillingTests` | Fixture-nya menerapkan `Database.Migrate()` ke basis data pengembangan bersama. Project-nya tetap terbukti ikut ter-compile lewat build solution |
| Migration | Task ini tidak mengubah schema |

Basis data uji adalah SQLite di dalam memori (`TestDatabase`), yang tidak pernah menyentuh basis
data mana pun yang tercatat di `appsettings`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `ErrorCount > 0` menolak finalisasi dengan `400` beserta payload `ConsultationFinalizationValidationResponse` | Terpenuhi | `JalurAntrean_MemakaiValidatorYangSamaDanMenolakKonsultasiTidakLayak` beserta enam uji penolakan lainnya |
| 2. Warning yang belum di-acknowledge menolak finalisasi | Terpenuhi | `PeringatanBelumDiakui_...`, `AcknowledgementIssueKeyLain_...` |
| 3. Konsultasi `Completed`/`Cancelled` tidak dapat difinalisasi ulang | Terpenuhi | Sudah terbukti pada `RJ-DOC-BE-001` (`PenyelesaianKedua_...`) dan tetap lulus pada regresi |
| DoD — tidak ada jalan pintas finalisasi yang melewati validasi | Terpenuhi | Kedua permukaan memakai `ConsultationValidationService` yang sama; jalur antrean terbukti menolak |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build solution memunculkan `17` peringatan pada berkas yang tidak disentuh task ini. Berkas yang diubah tidak memunculkan peringatan |
| Masalah yang diketahui | **Temuan pada `TrxPatientProcedure.Quantity`.** Kolom ini memiliki nilai bawaan `1` di database. EF menghilangkan properti bernilai default CLR dari perintah `INSERT`, sehingga menyisipkan baris dengan `Quantity = 0` justru tersimpan sebagai `1`. Aturan `INVALID_PROCEDURE_QUANTITY` karena itu **tidak dapat dipicu dari penyisipan baris baru**, hanya dari pembaruan. Aturannya sendiri benar dan tetap berguna; uji `C` memakai jalur pembaruan agar benar-benar menguji aturannya. Tidak diperbaiki dari task ini karena mengubah pemetaan kolom berdampak lintas modul dan bukan cakupan `RJ-DOC-BE-002` |
| Risiko tersisa | `1` `ExpectedUpdatedAt` masih opsional dan TOCTOU belum ditutup — `RJ-DOC-BE-003`. `2` Penguncian catatan sesudah penyelesaian yang **berhasil** belum ditinjau menyeluruh — `RJ-DOC-BE-004`; task ini hanya membuktikan penguncian tidak tertinggal ketika penyelesaian **gagal**. `3` Fakta klinis yang gagal diserahkan belum dapat ditemukan kembali — `RJ-DOC-BE-005`. `4` Audit trail belum diperluas — `RJ-DOC-BE-006`; `RJ-DOC-CAP-025` sengaja tetap `PARTIAL` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `2` berkas source berubah, `1` berkas uji baru, `1` berkas infrastruktur uji ditambah metode, ditambah dokumentasi blueprint. Berkas staged yang tidak berkaitan — `Tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratoryAuthorityTests.cs` dan `agents/rules/**` — **tidak disentuh**. Working tree `RJ-DOC-BE-001` tetap utuh |
| Langkah berikutnya | `RJ-DOC-BE-003` — idempotency dan concurrency finalisasi. Wewenang implementasinya belum diberikan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` (`Cli`), `PharmacyManagement` (`Phm`), `RegistrationManagement` (`Reg`) |
| Submodule | `NOT APPLICABLE` |
| Status registry | Ketiganya `ACTIVE / LEGACY` dan sudah terdaftar |
| Keberlakuan | `TOUCHED LEGACY` — tidak ada entity operasional baru, tidak ada tabel baru, tidak ada rename |
| QBE yang berlaku | `QBE-VAL-001` invarian bisnis divalidasi di server dan tidak dapat dilewati; `QBE-SVC-001` seluruh logika berada di service, bukan controller; `QBE-API-001` boundary dan status response existing dipakai apa adanya; `QBE-DTO-001` tidak ada entity EF yang menjadi kontrak API; `QBE-TXN-001` penolakan tidak meninggalkan state parsial; `QBE-PERM-001` metadata Access tidak diubah |
| QBE yang **tidak** berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-DB-*`, `QBE-LOG-001` — tidak ada entity, prefix, kode bisnis, pekerjaan database, maupun perluasan audit pada task ini |
