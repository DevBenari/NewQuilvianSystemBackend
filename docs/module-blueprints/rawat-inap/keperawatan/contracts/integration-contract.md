# Integration Contract — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | **`0.5.0`** — bagian 8, `draft` |
| `last_changed_in` | **`0.5.0`** — `INT-KEP-07` s.d. `17`. Sebelumnya `0.3.0`; tidak bergerak pada `0.4.0` |
| Compatibility impact | `0.3.0`: satu integrasi baru **`INT-KEP-06`** — keutuhan dan koreksi dokumen keperawatan kepada `MedicalRecordManagement`, sesuai `RWI-DEC-091`. Lima integrasi lama tidak berubah |
| Status | **`draft`** untuk `0.5.0` |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.3`; `PRD-RWI-FINAL-001` v1.0.0; decision log `13` |
| Keputusan yang mengikat | `RWI-DEC-091`, `RWI-FACT-016`, `RM-DEC-019` (milik `MedicalRecordManagement`) |
| Tanggal | 2 September 2026 |

---

## 0. Kenapa dokumen ini yang paling berisi di sub-modul ini

Sub-modul ini **tidak memiliki satu tabel pun**. Hampir seluruh wujudnya adalah integrasi:
membaca konteks dari episode, menulis ke tabel milik `ClinicalManagement`, memicu rujukan ke
Gizi, dan mengirim pemicu tagihan ke Billing.

---

## 1. `INT-KEP-01` — Pelonggaran konteks klinis rawat inap ★ **penghalang utama**

| Field | Isinya |
| --- | --- |
| Arah | Rawat Inap **meminta** perubahan pada `ClinicalManagement` |
| Bentuk | Sinkron, di dalam proses yang sama. Bukan pesan, bukan antrean |
| Pemilik perubahan | `ClinicalManagement` — Muhammad Hamzah, disetujui `RWI-DEC-062` |
| Yang diminta | `ValidateCreateWithoutQueueAsync` menerima encounter yang punya `InpEpisode` berstatus `Admitted`, setara dengan cara `EmgVisit` dipakai untuk IGD |
| Yang dibaca | `InpEpisode` — `EncounterId`, `EpisodeStatus` |
| Yang **tidak** berubah | Nol kolom. Nol tabel. Perilaku rawat jalan dan medical check-up tetap sama persis |
| Idempotency | Tidak berlaku — ini pemeriksaan baca |
| Timeout | Pembacaan lokal satu database; tidak ada panggilan jaringan |
| Bila gagal | Pengkajian ditolak `422` beserta `VAL-KEP-01`/`02`/`03`. Tidak ada keadaan setengah jadi |
| Traceability | `PRD-RWI-FINAL-001` bagian 30.3; `RWI-DEC-080`; `AC-CAP012-01` |

**Ini satu-satunya penghalang yang menahan seluruh sub-modul.** Selama `INT-KEP-01` belum ada,
tidak satu pun dari lima kemampuan dapat dipakai untuk pasien rawat inap.

---

## 2. `INT-KEP-02` — Konteks episode bagi ruang kerja

| Field | Isinya |
| --- | --- |
| Arah | **Baca** dari `episode-rawat-inap` |
| Yang dibaca | Pasien, lokasi terkini, DPJP, perawat penanggung jawab, status episode, lama dirawat |
| Endpoint | `GET /episodes/{id}`, `GET /census` |
| Frekuensi | Setiap kali ruang kerja dibuka dan sebelum setiap tulisan |
| Bila gagal | Ruang kerja menampilkan keadaan gagal beserta tombol coba lagi. **Tidak** menampilkan formulir kosong yang seolah siap diisi |
| Arah tulis | **Tidak ada.** Sub-modul ini tidak pernah mengubah episode |

---

## 3. `INT-KEP-03` — Catatan keperawatan ke CPPT

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** ke `ClinicalManagement` |
| Tabel | `TrxPatientIntegratedProgressNote`, `ProfessionType` = perawat |
| Perubahan yang dibutuhkan | **Nol.** Seluruh kolom penghubungnya sudah nullable |
| Pemilik kontrak CPPT | Sub-modul `dokter-rawat-inap` (`CAP-021`) |
| Kebijakan tampil | PRD `CAP-014` aturan 4: catatan keperawatan tampil pada lini masa klinis **sesuai kebijakan**. Kebijakannya belum ditetapkan; sampai itu terjadi, catatan tetap tersimpan dan tetap terbaca dari ruang kerja keperawatan |

---

## 4. `INT-KEP-04` — Rujukan gizi

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** pemicu ke modul Gizi; **baca** status dan ringkasannya |
| Keadaan modul tujuan | **`PLANNED`** — belum ada |
| Yang berlaku sementara | Hasil skrining gizi tersimpan pada pengkajian (`NutritionRiskStatus`, `NutritionRiskScore` — **kolom yang sudah ada**). `VAL-KEP-10` memunculkan saran, bukan penolakan |
| Kapan integrasi sungguhan dibuat | Setelah modul Gizi berdiri. `CAP-027` karena itu **ditunda** pada `04-prd-to-mvp.md` bagian 8 |
| Larangan | Sub-modul ini **MUST NOT** membuat tabel asuhan gizi sendiri — PRD 23.1 |

---

## 5. `INT-KEP-05` — Pemicu tagihan tindakan

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** ke `BillingManagement` |
| Idempotency | Wajib. Kunci disimpan pada `TrxNursingIntervention.IdempotencyKey` |
| Bila gagal | `BillingDispatchStatus` menjadi `Failed`. **Catatan klinisnya tetap tersimpan** — `AC-CAP014-02` |
| Percobaan ulang | Dijalankan terpisah; tidak menyentuh catatan klinis |
| Rekonsiliasi | Daftar tindakan berstatus `Failed` dapat dibaca lewat `GET /{id}/billing-dispatch` |
| Keadaan modul tujuan | `BillingManagement` belum punya kemampuan transaksi. Sampai itu ada, `BillingDispatchStatus` tetap `Pending` dan tidak ada yang hilang |

---

## 6. Integrasi yang **belum dapat ditulis**

| Integrasi | Kenapa |
| --- | --- |
| Pemakaian alat ke persediaan dan Billing (`CAP-016`) | **Kemampuannya `DEFERRED`** lewat `RWI-DEC-089`. Selain kepemilikan tabelnya yang sengaja ditunda, lawan integrasinya pun belum berwujud: `RWI-FACT-015` membuktikan tidak ada modul persediaan/aset di `Areas/`. Kontraknya ditulis setelah modul itu ada |

---

## 7. `INT-KEP-06` — Keutuhan dan koreksi dokumen keperawatan ★ **baru pada `0.3.0`**

| Field | Isinya |
| --- | --- |
| Produsen dan pemilik | **`MedicalRecordManagement`** |
| Konsumen | `ClinicalManagement` dan ruang kerja keperawatan |
| Arah | Rawat Inap **memakai** mesin yang sudah ada, dan **meminta satu perluasan penegakan** |
| Bentuk | Sinkron, di dalam transaksi yang sama dengan finalisasi dokumen |
| Tujuan bisnis | Menandatangani, mengunci, dan **mengoreksi** pengkajian serta catatan tindakan keperawatan tanpa menimpa isi aslinya |
| Keadaan modul tujuan | **Sudah ada dan sudah dipakai.** Catatan terpadu sudah mendaftar ke mesin ini, dan mesinnya sudah mengenal profesi perawat — `RWI-FACT-016` |

### 7.1 Yang diminta

| Hal | Isinya |
| --- | --- |
| Perubahan model | **Nol.** Tabel keutuhan, addendum, dan pendelegasian penulis dipakai apa adanya |
| Nilai enum baru | **Nol.** `Assessment` dan `Procedure` sudah bernomor pada `ClinicalDocumentKind` |
| Pendaftaran | Pengkajian didaftarkan sebagai `Assessment` saat berpindah ke `Completed`; catatan tindakan sebagai `Procedure` saat berpindah ke `Finalized` — keduanya **dalam transaksi yang sama** dengan finalisasinya |
| Bila pendaftaran gagal | Finalisasi ikut batal. Tidak boleh ada dokumen final yang tidak dapat dikoreksi — celah itulah yang ditemukan `RWI-FACT-014` pada dokumen dokter |
| **Perubahan perilaku yang diminta** | Menambahkan `Assessment` dan `Procedure` ke daftar jenis yang **ditegakkan**. Hari ini daftar itu hanya berisi `ProgressNote` sesuai `RM-DEC-019` |

### 7.2 Kenapa perluasan penegakan itu wajib, bukan sekadar rapi

Pembacaan source menemukan dua hal yang bila digabung menghasilkan jebakan diam:

| Temuan | Akibatnya |
| --- | --- |
| `RegisterAsync` **tidak menyaring** jenis dokumen | Pendaftaran pengkajian dan tindakan **berhasil** hari ini juga |
| `EnsureMutableAsync` **membiarkan lewat** jenis yang belum ditegakkan | Penguncian **tidak berlaku**. Dokumen final tetap dapat disunting |

Gabungannya: bila dibangun apa adanya, pengkajian akan **terlihat terdaftar** pada mesin keutuhan sementara
kuncinya tidak pernah menutup. Seluruh mesin status pada
[`state-transition-matrix.md`](./state-transition-matrix.md) kehilangan penjaganya tanpa satu pun pesan
error muncul. Kegagalan yang tidak berbunyi adalah kegagalan yang paling mahal di rekam medis.

### 7.3 Keadaan

| Field | Nilai |
| --- | --- |
| Butir terbuka | **`RWI-OQ-051`** |
| Pemilik jawaban | Pemilik `MedicalRecordManagement`, **belum dinyatakan** |
| Memblokir desain | **Tidak.** Bentuk kontraknya sudah dapat dikunci sekarang |
| Memblokir implementasi | **Ya** — `BE-RWI-057` dan `BE-RWI-062` |
| Bila ditolak | Kembali ke `/qv-grill`. **Jangan** membangun penjaga penguncian sendiri di `ClinicalManagement`; itu mesin koreksi tandingan yang dilarang `RWI-DEC-087` |

---

## 8. Perubahan pada `contract_version` `0.5.0` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status `draft`.** Seluruh integrasi di bawah **internal**; tidak ada sistem luar. Aplikasi memakai satu
`ApplicationDbContext`, sehingga "satu transaksi" lintas `ClinicalManagement` dan `PharmacyManagement` adalah satu
transaksi database sungguhan, bukan koordinasi antarlayanan.

### 8.0 Ringkasan

| ID | Arah | Modul lawan | Pemakai | Pola | Keadaan |
| --- | --- | --- | --- | --- | --- |
| `INT-KEP-07` | Baca | `InPatientManagement`, `ClinicalManagement` | Seluruh jalur tulis perawat dan MPP | Sinkron | `Extend` — pemeriksaan unit sudah ada untuk perawat (`RWI-DEC-100`), belum untuk MPP |
| `INT-KEP-08` | Baca-tulis | `PharmacyManagement` resep → MAR | Pembentukan dosis | Sinkron saat MAR dibuka + terjadwal | `Missing` |
| `INT-KEP-09` | Tulis, dipicu penghentian butir | `PharmacyManagement` | Resep Harian dokter | Sinkron, satu transaksi | `Missing` — pasangan `INT-DOK-16` |
| `INT-KEP-10` | Baca | `PharmacyManagement` MAR → `ClinicalManagement` cairan | Intake obat | Sinkron | `Missing` |
| `INT-KEP-11` | Baca-tulis | `PharmacyManagement` order, `ClinicalManagement` GDS | Pelaksanaan sliding scale | Sinkron, satu transaksi | `Missing` — pasangan `INT-DOK-18` |
| `INT-KEP-12` | Tulis | `MedicalRecordManagement` | Evaluasi Awal | Sinkron, satu transaksi | `Missing` — butuh jenis dokumen baru |
| `INT-KEP-13` | Tulis | `ClinicalManagement` alergi | Dugaan reaksi obat | Sinkron | `Extend` |
| `INT-KEP-14` | Baca | `BillingManagement` | Menu Tagihan Pasien | Sinkron | `Missing` — kontrak diminta |
| `INT-KEP-15` | Tulis, dipicu penutupan | `PharmacyManagement` MAR | `episode-rawat-inap` | Sinkron, satu transaksi dengan `INT-DOK-13` | `Missing` |
| `INT-KEP-16` | Baca | `ClinicalManagement` konfigurasi klinis | Formulir pengkajian, poliklinik | Sinkron | `Missing`; perubahan perilaku bagi `rawat-jalan` |
| `INT-KEP-17` | Frontend | Gizi, Hemodialisa, Bank Darah, Rehab Medik, Kamar Operasi, pemakaian alat | Menu Penunjang Medis, Pemakaian Alat, Pemesanan Ruangan Bedah | Permukaan saja | "Integrasi belum tersedia" |

### 8.1 `INT-KEP-07` — Unit penempatan bersama perawat dan MPP

| Hal | Isinya |
| --- | --- |
| Yang dipakai | `InpatientClinicalContextService.IsEmployeeAssignedToUnitAsync(employeeId, serviceUnitId, at)` — metode baru berisi logika `IsNurseOnDutyAtUnitAsync` yang sudah ada |
| Unit yang diperiksa | `InpEpisode.ServiceUnitId` **saat simpan**, bukan salinan pada dokumen — pasien pindah unit, kewenangan ikut pindah (`AC-KEP-049`) |
| Identitas | `ApplicationUser.EmployeeId` dari akun login; tanpa `EmployeeId` → `403` (`AC-KEP-046`) |
| Perbedaan MPP | MPP diperiksa **dua** hal: hak `CaseManagementEvaluation : Create`/`Update` **dan** penempatan unit — `RWI-DEC-131` |
| Kegagalan membaca penempatan | `403` beserta pesan "Kewenangan tidak dapat diperiksa, coba lagi". **Tidak** jatuh ke izinkan |
| Contoh | Ns. Dewi MPP ditempatkan di Melati. Budi di Melati → diterima. Budi dipindah ke Anggrek pukul 14.00; Dewi menyimpan konsep 14.10 → `403` |

### 8.2 `INT-KEP-08` — Pembentukan dosis dari resep aktif

| Hal | Isinya |
| --- | --- |
| Pemicu | (1) `GET medication-administrations/episodes/{id}` sebelum membaca; (2) `MedicationDoseSchedulerHostedService` tiap 15 menit untuk episode `Admitted` |
| Yang dibaca | `PhmPrescriptionItem` aktif, disetujui untuk diberikan, `IsStopped = false`, `IsAsNeeded = false`, `DoseKind = Fixed`, `FrequencyCode` punya `PhmMedicationScheduleTime` untuk unit episode atau bawaan |
| Yang ditulis | Dosis `Due` untuk setiap slot dari waktu sekarang sampai `DoseGenerationHorizonHours`, tidak sebelum waktu resep berlaku |
| Idempotensi | Unique `(PrescriptionItemId, ScheduledAt)`; pelanggaran unique ditelan sebagai "sudah ada" |
| Kegagalan | Hosted service mencatat galat per episode dan melanjutkan episode berikutnya; percobaan berikut 15 menit lagi. MAR yang dibuka tetap memanggil pembentukan, sehingga tidak ada dosis yang hilang karena galat terjadwal |
| Pertanyaan status resep | Status resep yang dianggap "boleh diberikan" mengikuti mesin status `PhmPrescription` yang disepakati `dokter-rawat-inap` `0.6.0` bagian 8 |
| Contoh | Ceftriaxone `q12h` mulai Senin 10.00, jadwal bawaan 08.00/20.00, cakrawala 24 jam → Senin 20.00, Selasa 08.00 `Due` |

### 8.3 `INT-KEP-09` — Penghentian butir resep membatalkan dosis `Due`

| Hal | Isinya |
| --- | --- |
| Pemanggil | `InpatientPrescriptionService.StopItemAsync` milik `dokter-rawat-inap` — `INT-DOK-16` |
| Metode | `MedicationAdministrationService.CancelDueDosesForItemAsync(itemId, stoppedAt, "resep dihentikan")` |
| Yang disentuh | Dosis `Due` dengan `ScheduledAt ≥ stoppedAt`, **termasuk** yang `Pending` cek ganda — cek ganda yang belum selesai berarti obat belum boleh dianggap diberikan |
| Tidak disentuh | `Administered`, `Held`, `Refused`, `Missed`, `Due` sebelum `stoppedAt` |
| Transaksi | Satu transaksi dengan penghentian butir. Gagal → penghentian batal |

### 8.4 `INT-KEP-10` — Intake obat dari dosis MAR

| Hal | Isinya |
| --- | --- |
| Pembaca | `DailyMonitoringService.RecordFluidAsync` |
| Yang dibaca | Dosis dengan `Id` yang dikirim: status `Administered`, episode sama, belum punya entri aktif |
| Volume | **Diketik perawat**, termasuk pelarut — `RWI-DEC-149` (2). MAR tidak menyediakan volume |
| Koreksi dosis setelah tertaut | `MedicationAdministrationService.CorrectAsync` memanggil `DailyMonitoringService.FlagLinkedFluidEntryAsync(administrationId)` dalam transaksi yang sama bila status baru bukan `Administered` → `DoseCorrectionFlaggedAt` diisi — usulan `G-26` |
| Arah ketergantungan | `ClinicalManagement` membaca `PharmacyManagement` lewat service; `PharmacyManagement` hanya memanggil satu metode penanda. Tidak ada salinan data dosis di tabel cairan selain FK |

### 8.5 `INT-KEP-11` — Pelaksanaan sliding scale

| Hal | Isinya |
| --- | --- |
| Langkah dalam satu transaksi | (1) baca order `Active` dan versi berlakunya (`INT-DOK-18`); (2) tulis GDS baru lewat `DailyMonitoringService.RecordGlucoseAsync`, **atau** baca GDS bangsal terpilih; (3) bandingkan satuan; (4) cari rentang yang memuat nilai; (5) tulis dosis MAR `SlidingScale` — pakai slot `Due` bila dikirim, jika tidak baris baru; (6) tulis `PhmSlidingScaleExecution` |
| Idempotensi | `Idempotency-Key` wajib dan unique; kiriman ulang mengembalikan pelaksanaan yang sama beserta dosis yang sama — `RWI-AC-220` |
| Kegagalan di langkah mana pun | Seluruh transaksi batal: **tidak ada** GDS, dosis, maupun pelaksanaan tersimpan. Pesan "Pencatatan sliding scale gagal disimpan. Belum ada yang tercatat; ulangi." — `RWI-DEC-148` (c) |
| Rentang 0 unit | Dosis MAR `Held` beralasan "GDS di bawah rentang pemberian" — usulan gate `G-22` |
| Instruksi rentang | `InstructionText` rentang ditampilkan kepada perawat; notifikasi ke dokter **tidak** dikirim sistem — gate `G-24` |
| High-alert | Insulin `IsHighAlert` → dosis `Pending`; pelaksanaan tetap `Recorded` karena perhitungan sudah terjadi, dan riwayat pelaksanaan menampilkan status cek ganda dosisnya |

### 8.6 `INT-KEP-12` — Evaluasi Awal ke mesin keutuhan dokumen

| Hal | Isinya |
| --- | --- |
| Diminta dari | Pemilik `MedicalRecordManagement` — **Yoga Aji Pratama** |
| Yang diminta | Nilai enum `ClinicalDocumentKind.CaseManagementEvaluation = 14`, masuk daftar jenis yang ditegakkan `EnsureMutableAsync`, dan boleh menerima addendum |
| Pemakaian | `RegisterAsync` saat konsep dibuat, `SignAsync` saat selesai, `LockOpenDocumentsForEncounterAsync` saat episode ditutup — mesin yang sudah ada, pemanggil baru |
| Selama belum disetujui | Evaluasi Awal dapat dibangun dan diselesaikan, tetapi **addendum dan penguncian konsep saat penutupan tertahan**; endpoint addendum menjawab `501` "Addendum Evaluasi Awal belum tersedia" |
| Pemberitahuan | Wajib dikirim bersama pemberitahuan `INT-DOK-13` agar pemilik membaca satu paket |

### 8.7 `INT-KEP-13` — Dugaan reaksi obat

| Hal | Isinya |
| --- | --- |
| Yang ditulis | Satu `TrxPatientAllergy` berkepastian `Suspected`, kategori `Drug`, `DrugId` dosis, `SourceMedicationAdministrationId`, `InpEpisodeId` |
| Baris MAR | **Tidak diubah** — `AC-MVP-029` |
| Penerima pemberitahuan | Tidak ada push notification pada rilis ini (`RWI-DOK-RQG-001`). Dugaan tampil pada peringatan alergi aktif kepala konteks pasien bagi DPJP, perawat, dan apoteker, serta daftar pantau "Dugaan reaksi obat belum diverifikasi" yang memakai saringan **yang sudah ada** `GET patient-allergies?certainty=Suspected&isVerified=false&serviceUnitId=` — nol endpoint baru. Penerima notifikasi aktif **usulan** DPJP aktif dan apoteker — gate `G-15` |
| Verifikasi | Jalur `PATCH patient-allergies/{id}/verify` yang sudah ada |

### 8.8 `INT-KEP-14` — Ringkasan tagihan dari Billing

| Hal | Isinya |
| --- | --- |
| Diminta dari | Pemilik `BillingManagement`; pekerjaan penyediaannya ditempatkan di pemilik Billing |
| Bentuk | `api-contract.md` bagian 7.14 |
| Timeout dan gagal | Batas tunggu 5 detik; gagal → pesan kegagalan apa adanya, tanpa angka nol dan tanpa data tiruan |
| Cache | Tidak ada. Setiap buka menu membaca ulang |
| Selama belum ada | "Integrasi belum tersedia" — `RWI-DEC-108` |

### 8.9 `INT-KEP-15` — Penutupan episode membatalkan dosis yang belum waktunya

| Hal | Isinya |
| --- | --- |
| Diminta dari | `episode-rawat-inap` kontrak `0.9.0` — langkah keempat pada transaksi `INT-DOK-13` |
| Metode | `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync(episodeId, closedAt)` |
| Yang disentuh | Dosis `Due` dengan `ScheduledAt > closedAt` → `Cancelled` beralasan "perawatan ditutup" |
| Tidak disentuh | Dosis `Due` dengan `ScheduledAt ≤ closedAt` — tetap `Due`, hanya-baca, tampil "Tidak dicatat sebelum perawatan ditutup" |
| Peringatan sebelum menutup | Jumlah dosis `Due` yang sudah lewat jadwal ditampilkan sebagai peringatan pada layar penutupan milik `episode-rawat-inap` — **tidak** menahan penutupan (`RWI-DEC-129`) |
| Idempotensi | Hanya menyentuh `Due` masa depan; menjalankan ulang tidak mengubah apa pun |
| Hosted service | Tidak membentuk dosis untuk episode selain `Admitted` |

### 8.10 `INT-KEP-16` — Konfigurasi klinis dipakai formulir

| Hal | Isinya |
| --- | --- |
| Pembaca | `NursingAssessmentDocumentService`, `CaseManagementEvaluationService`, `GET clinical-instruments/resolve` |
| Pemilihan instrumen | Jenis dokumen → `InstrumentKind`; usia pasien pada `ClinicalDateTime` dalam bulan → instrumen yang rentang usianya memuat |
| Pengaturan lingkungan | `ClinicalConfiguration:AllowDraftVersionsForTesting` pada `appsettings`; **wajib `false`** di produksi. Bila `true`, response `resolve` menandai `IsDraftAllowedInThisEnvironment` dan layar menampilkan pita "Lingkungan uji — versi belum disahkan" |
| Dampak ke poliklinik | Langkah migration K2 mengganti perhitungan risiko jatuh **rawat inap** saja. Jalur poliklinik dan IGD tetap memakai perhitungan lama sampai pemilik `rawat-jalan` memutuskan; perubahan ini **wajib diberitahukan** kepada pemilik `rawat-jalan` sebelum K2 dirilis |

### 8.11 `INT-KEP-17` — Menu yang backend-nya belum tersedia

| Menu | Keadaan source `df3679c0` | Perilaku rilis ini | Dasar |
| --- | --- | --- | --- |
| Penunjang Medis → Gizi | `NutritionManagement` **ada** di source | "Integrasi belum tersedia" | `RWI-DEC-113`; pilihan pemilik 15 September 2026 |
| Penunjang Medis → Bank Darah | `BloodBankManagement` **ada** | Sama | Sama; transfusi `DEFERRED` |
| Penunjang Medis → Hemodialisa, Rehab Medik | Tidak ditemukan | Sama | `RWI-DEC-113` |
| Pemesanan Ruangan Bedah | `OperatingRoomManagement` **ada** | Sama | `RWI-DEC-108` |
| Pemakaian Alat | Tidak ada | Sama | `RWI-DEC-089` |
| Transfer Pasien → Serah Terima Klinis | Perpindahan tempat tidur **ada** (`CAP-017`); serah terima klinis antarunit tidak ada | Perpindahan tempat tidur memakai `episode-rawat-inap` apa adanya; bagian serah terima klinis "Integrasi belum tersedia" dan tidak disimpan ke mana pun | `RWI-DEC-113` |
| Tagihan Pasien | Kontrak belum ada | Sama sampai `INT-KEP-14` | `RWI-DEC-137` |

> Tiga modul yang ternyata sudah ada di source dicatat sebagai **temuan non-blocking** untuk `grill-me` berikutnya:
> apakah integrasinya ditarik ke rilis ini atau tetap ditunda. Pilihan pemilik pada pass ini: tetap ditunda.

### 8.12 Integrasi yang sengaja tidak dibuat pada `0.5.0`

| Tidak dibuat | Alasan |
| --- | --- |
| GDS dari hasil laboratorium | `RWI-DEC-148` |
| Intake darah dari catatan transfusi | Transfusi `DEFERRED` — `RWI-DEC-149` (5) |
| Tagihan strip glukometer | Gate `G-28` |
| Handover shift | `DEFERRED` — `RWI-DEC-145` |
| Salinan data episode, dosis, atau order ke tabel keperawatan | `INV-KEP-04` |
