# MCU (Medical Check-Up) — Peta Kemampuan Existing

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` |
| Revision | `1` — audit penuh pertama |
| Status | `draft` |
| Jenis audit | Audit penuh, **read-only**. Nol baris source diubah, nol akses basis data |
| Masukan keputusan | [00-interview-decisions.md](00-interview-decisions.md) revision 1 (`MCU-DEC-001`..`MCU-DEC-021`) |
| Backend SHA | `4d3e4f98aca0d9cbf6d5e0b62070d94d4760cfec` (branch `yoga`). *Working tree* memuat 48 berkas belum di-commit; bukti di bawah dibaca dari berkas di disk |
| Frontend v2 SHA | `7fb1e1d3eb18df33a893cf2cb1a5cc7c6914acb9` (branch `YogaV2`). *Working tree* memuat 18 berkas belum di-commit |
| Frontend v1 SHA | `7210f5f4db3a0c1f51446249e5c35a57eedb081c` (`C:\Users\Admin\QuilvianSystemFrontendDev`) — referensi tampilan saja (`MCU-DEC-016`) |
| Kontrak yang berlaku | Belum ada kontrak MCU |
| Tanggal audit | 2026-10-20 |

**Singkatan bukti.** `BE` = `NewQuilvianSystemBackend`, `FE` = `QuilvianSystemFrontendDev` (v2),
`FE1` = frontend v1. Bila SHA tidak ditulis, yang berlaku adalah SHA pada tabel di atas.

> **Cara membaca dokumen ini.** Dokumen ini menjawab *apa yang sudah ada di sistem hari ini*,
> bukan *bagaimana MCU nanti dibangun*. Setiap baris membawa lokasi bukti supaya bisa diperiksa
> ulang. Status *Ready to reuse* hanya diberikan bila kode, hak akses, dan konsumennya terlihat.
> Tidak ada satu pun yang dijalankan sungguhan pada audit ini.

---

## 1. Ringkasan untuk pembaca umum

| No | Temuan | Artinya bagi MCU |
| ---: | --- | --- |
| 1 | Jenis kunjungan **Medical Check-Up sudah dikenal** sistem, dan endpoint pembuat kunjungan menerimanya | Check-in MCU bisa membuat kunjungan lewat jalur yang sudah ada |
| 2 | **Belum ada satu pun layar** di v2 yang membuat kunjungan MCU. Kiosk mendaftarkan pasien ke "Klinik Medical Check Up" sebagai kunjungan **rawat jalan** | Kunjungan MCU hari ini tidak pernah lahir dari aplikasi; pintu kiosk (13E) bertentangan dengan `MCU-DEC-017` |
| 3 | "Perjanjian" (booking) rawat jalan yang ada **langsung membuat kunjungan** pada saat booking | Bertentangan dengan `MCU-DEC-003` (kunjungan baru dibuat saat check-in). Booking MCU harus dibangun sendiri |
| 4 | **Penjamin perusahaan sudah lengkap**: master perusahaan beserta nomor dan tanggal kontrak, data karyawan-pasien, aturan cakupan, dan dokumen tagihan perusahaan | Peserta perusahaan (`MCU-DEC-004`) bisa memakai data yang ada |
| 5 | Gerbang lunas Lab **sudah meloloskan** pasien berpenjamin perusahaan dan **menahan** pasien tunai MCU yang belum lunas | Persis `MCU-DEC-004`. Radiologi **tidak punya** gerbang lunas sama sekali |
| 6 | Lab dan Radiologi **menagih sendiri per pemeriksaan** — Lab saat spesimen diterima, Radiologi saat gambar diambil | Bila paket ditagih satu baris, Lab/Rad akan menagih **lagi**. Inilah risiko tagih dobel (`MCU-DEC-005`) |
| 7 | Master tarif punya penanda **"tarif paket"**, tetapi hanya penanda; **tidak ada** isi paket, komponen, nilai pengembalian, maupun harga khusus per perusahaan | Master Paket MCU harus dibangun |
| 8 | Billing **sudah punya** penambahan baris tagihan dari katalog tarif, pengembalian dana per baris dengan persetujuan, dan serah tagihan ke piutang | Tagihan paket dan refund dapat memakai jalur Billing |
| 9 | Skrining perawat (nurse station) **tidak membatasi jenis kunjungan**; antrean dibuat menurut klinik | Skrining MCU kemungkinan cukup memakai klinik MCU — belum dibuktikan berjalan |
| 10 | **Tidak ada pengirim WhatsApp maupun email** di backend. v1 mengirim WhatsApp langsung dari browser ke server eksternal | Tombol Kirim (`MCU-DEC-019`) tertahan sampai platform menyediakannya |
| 11 | MCU v1 **tidak mengenal paket**: order MCU v1 = daftar pemeriksaan Lab satuan dengan jumlah harga per item, memakai backend lama (`/LabBooking/pagedLabMCU`) yang **tidak ada** di backend v2 | Menegaskan `MCU-DEC-016`: v1 hanya referensi tampilan |
| 12 | Registry prefix modul **belum punya** prefix MCU | Model MCU baru tertahan aturan `QBE-MOD-002` sampai prefix didaftarkan |

---

## 2. Batas audit

| Klaster | Yang diperiksa | Cara |
| --- | --- | --- |
| Identity/Master | Pasien, penjamin perusahaan, tarif, prosedur Lab/Rad | Baca model + controller master |
| Episode/Transaction | Kunjungan, booking rawat jalan, check-in, kiosk | `PatientEncounterController`, `OutpatientAppointmentService`, enum Registrasi |
| Workflow/Status | Status kunjungan, antrean perawat | Enum + `NurseStationQueueController` |
| Order/Result | Pesanan Lab/Rad, penerbitan tagihan Lab/Rad | `LabOrderService`, `RadOrderController`, `BillingSourceContract` |
| Financial | Gerbang lunas, baris tagihan katalog, refund, piutang perusahaan, PPN | `LabPaymentClearanceRules`, `BillingInvoicesController`, `BillingFinancialExceptionsController` |
| Documentation | Skrining, dokumen klinis | `PatientAssessmentController`; capability map Laboratorium `CAP-P19-*` |
| Authorization | Pola izin | `[AccessPermission]` pada controller yang disentuh |
| External Integration | WhatsApp/email | `rg` paket dan klien; capability map `platform-notification` |
| Frontend | Menu/route MCU v2 dan v1, kiosk, modal WhatsApp | `rg` pada kedua repository frontend |

**Tidak diaudit:** isi laporan (butir 14, `LATER SLICE`), MCU karyawan rumah sakit (di luar
scope), dan seluruh perilaku berjalan (tidak ada akses basis data atau server pada audit ini).

---

## 3. Tabel kemampuan

### 3.1 Kunjungan, booking, check-in, kiosk

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `MCU-CAP-01` | Jenis kunjungan Medical Check-Up | Registrasi | `BE/Areas/HealthServices/RegistrationManagement/Enums/EncounterType.cs#MedicalCheckup=4`; `MasterData/Enums/ClinicType.cs#MedicalCheckup=7`, `ServiceUnitType.cs#MedicalCheckup=7`, `DoctorServiceRuleType.cs#MedicalCheckup=6` | `Ready to reuse` | — | Rendah |
| `MCU-CAP-02` | Membuat kunjungan `MedicalCheckup` saat check-in (`MCU-DEC-003`) | Registrasi | `PatientEncounterController.cs` `POST admin` (`:621`, izin `PatientEncounter : Create`) dan `POST kiosk` (`:636`) → `CreateEncounterCoreAsync`; validasi jenis hanya `Enum.IsDefined` (`:1751`); cabang khusus hanya `Emergency`, `Inpatient`, dan `Outpatient` (`:791`, `:1817`, `:2068`) | `Reuse with adapter` | Adapternya: MCU memanggil pembuat kunjungan saat check-in dengan `EncounterType = MedicalCheckup`. Untuk `MedicalCheckup` kelas pasien **tidak** ditentukan otomatis seperti rawat jalan (`ResolvePatientClassAsync` `:2060`) — perlu diputuskan saat desain | Sedang — jalur ini belum pernah dipakai untuk MCU (`MCU-UNK-01`) |
| `MCU-CAP-03` | Layar pembuat kunjungan MCU | Registrasi / MCU | FE v2: nol layar mengirim `encounterType = 4`; nilai itu hanya ada di konstanta IGD, Lab, Radiologi (`emergency-registration.constants.js:147`, `lab-clinical-pathology-result-rules.js:516`) | `Missing` | Layar check-in/pendaftaran MCU milik modul MCU | Sedang |
| `MCU-CAP-04` | Booking tanpa kunjungan, berstatus *Terjadwal*, dengan kuota harian (`MCU-DEC-003`, `012`) | MCU | Booking yang ada = kunjungan ber-`IsAppointment` yang **sudah dibuat saat booking**, khusus `EncounterType.Outpatient` dan kuota per jadwal dokter (`OutpatientAppointmentService.cs:296`, `:359`, `:572-573`) | `Missing` | Tidak dapat dipakai: bentuknya bertentangan dengan `MCU-DEC-003` (lihat `MCU-CONFLICT-005`) | Sedang |
| `MCU-CAP-05` | Check-in mencatat waktu hadir | Registrasi | `PatientEncounterController.cs#CheckInEncounter` `PATCH {id}/check-in` (`:1337`) mengisi `CheckedInAt`, izin `PatientEncounter : Update` | `Reuse with adapter` | Untuk MCU, check-in **membuat** kunjungan, bukan mengubahnya. Endpoint ini paling jauh dipakai sesudah kunjungan lahir | Rendah |
| `MCU-CAP-06` | Pintu kiosk tanpa booking menghasilkan kunjungan MCU (`MCU-DEC-017`) | Kiosk | FE kiosk menampilkan "Klinik Medical Check Up" sebagai poliklinik (`kiosk/task/report/frontend/FE-KSK-001.md`); kunjungan kiosk tercatat `Outpatient` (`laboratorium/00-interview-decisions.md` F27-4); `KioskServiceTarget` hanya `OutpatientClinic`, `Laboratory`, `Other` | `Conflict` | Lihat `MCU-CONFLICT-006` | Sedang — kunjungan MCU dari kiosk saat ini tercatat sebagai rawat jalan |
| `MCU-CAP-07` | Tidak hadir otomatis (`MCU-DEC-014`) | MCU | Kunjungan punya `NoShowAt`/`NoShowReason` dan `KioskEncounterClosureHostedService` menutup kunjungan kiosk | `Missing` | Yang dibutuhkan menutup **order booking MCU**, bukan kunjungan. Pola layanan latar (*hosted service*) dapat ditiru | Rendah |

### 3.2 Penjamin perusahaan dan harga

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `MCU-CAP-08` | Master perusahaan penjamin dengan masa kontrak | Administrator Master Data | `BE/Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs` — `ContractNumber`, `ContractStartDate`, `ContractEndDate`, `IsNeedGuaranteeLetter`, `PicEmail`, `PicWhatsAppNumber`, `BillingMethod`, `CreditLimitAmount`; `CompanyGuarantorController` | `Ready to reuse` | — | Rendah |
| `MCU-CAP-09` | Peserta sebagai karyawan perusahaan | Patient Management | `PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs` — `EmployeeNumber`, `EffectiveStartDate/EndDate`, `GuaranteeDocumentPath`, `IsEligible` | `Ready to reuse` | — | Rendah |
| `MCU-CAP-10` | Kunjungan berpenjamin perusahaan | Registrasi | `RegPatientEncounter.PaymentType` (`EncounterPaymentType.CompanyGuarantor = 3`); `RegPatientEncounterGuarantor` dengan snapshot perusahaan dan masa berlaku; kiosk menerima penjamin perusahaan sejak `BE-KSK-003` (`PatientEncounterController.cs:645`) | `Ready to reuse` | — | Rendah |
| `MCU-CAP-11` | Surat jaminan wajib sebelum terjamin (`MCU-DEC-004`) | Registrasi | `PatientEncounterGuarantorStatus.NeedGuaranteeLetter = 6`; `MstCompanyGuarantor.IsNeedGuaranteeLetter` | `Unknown` | Status ada, tetapi gerbang lunas Lab **tidak membacanya**: semua `CompanyGuarantor` lolos (`MCU-CAP-17`). Apakah status ini pernah ditegakkan — `MCU-UNK-02` | Sedang |
| `MCU-CAP-12` | Harga paket khusus per perusahaan dengan masa berlaku (`MCU-DEC-020`) | MCU / Billing | `MstCompanyGuarantor.IsUsingCompanyTariffBook` hanya dibaca untuk tampilan (`EncounterInsuranceService.cs:199`); `MstCompanyGuarantorCoverageRule` menyimpan **persen tanggungan**, bukan harga; `MstInsuranceTariff.ContractPrice` khusus **asuransi**, bukan perusahaan | `Missing` | Harga per perusahaan belum punya tempat | Sedang — salah harga ke klien korporat |
| `MCU-CAP-13` | Master tarif sebagai harga umum paket | Master Data | `MasterData/Models/MstTariff.cs` — `NormalPrice`, `EffectiveStartDate/EndDate`, `IsTaxable`, **`IsPackageTariff`**; `TariffController` hanya menyimpan dan menyaring penanda itu (`:125`, `:374`, `:702`) | `Reuse with adapter` | Satu baris `MstTariff` ber-`IsPackageTariff` dapat menjadi **harga umum** paket yang ditagih Billing. Isi paket tetap milik MCU | Rendah |

### 3.3 Master paket, komponen, persyaratan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `MCU-CAP-14` | Paket berisi komponen dan nilai pengembalian (`MCU-DEC-005`, `015`) | MCU | Nol model paket/komponen/bundel di `BE/Areas` dan `BE/Models` (`rg "class \w*(TariffBook|Package|Paket|Bundle)"` kosong) | `Missing` | — | — |
| `MCU-CAP-15` | Komponen Lab dan Radiologi merujuk katalog yang ada | Master Data / Lab / Rad | `MasterData/Models/MstProcedure.cs` — `IsLaboratory`, `LabDiscipline`, `IsRadiology`; pesanan Lab menerima daftar `ProcedureId` (`CreateLabOrderByExaminationsRequest.Examinations`), pesanan Rad menerima `ProcedureId` + `ModalityId` (`CreateRadOrderRequest`) | `Ready to reuse` | Komponen paket cukup menunjuk `MstProcedure` | Rendah |
| `MCU-CAP-16` | Persyaratan pemeriksaan per komponen (`MCU-DEC-013`) | MCU | `MstProcedure` tidak punya ruas persiapan; `RadOrder.RequiresPreparation` hanya penanda di pesanan; `MstRadSafetyRequirement` adalah syarat keselamatan radiasi, bukan persiapan pasien | `Missing` | — | Rendah |

### 3.4 Pesanan Lab/Rad dan gerbang lunas

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `MCU-CAP-17` | Gerbang lunas Lab: perorangan wajib lunas, perusahaan lolos (`MCU-DEC-004`) | Laboratorium | `LaboratoryManagement/Services/LabPaymentClearanceRules.cs#ReadAsync` — `Insurance`/`CompanyGuarantor` → `Guaranteed`; tunai diukur dari invoice per **kunjungan**; hanya `Inpatient`/`Emergency` yang `Deferred`. Dipakai `LabOrderService#StartProcessAsync:1400-1405` (`PUT {id}/start-process`) | `Ready to reuse` | Lunas dihitung per kunjungan, sehingga baris paket yang sudah dibayar ikut meloloskan kunjungan MCU | Rendah — tetapi lihat `MCU-CONFLICT-007` |
| `MCU-CAP-18` | Gerbang lunas Radiologi | Radiologi | `RadOrderService.cs:435` memakai `LabPaymentClearanceRules` **hanya untuk hitungan ringkasan**; nol pemeriksaan lunas pada `accept`/`start`/`complete` (`RadOrderController.cs:346-373`) | `Missing` | Radiologi mengerjakan peserta tunai yang belum bayar. Ini perilaku modul Radiologi (paritas v1), bukan milik MCU | Sedang — butir 11 tidak berlaku di Radiologi hari ini |
| `MCU-CAP-19` | Membuat pesanan Lab dari kunjungan MCU | Laboratorium | `LabOrderService#CreateByExaminationsAsync:1040`; nol pembatasan `EncounterType`; nol pemeriksaan `MstProcedure.IsAvailableFor*` di Lab/Rad | `Ready to reuse` | — | Rendah. Belum pernah dijalankan untuk MCU (`UNK-P19-01` Laboratorium) |
| `MCU-CAP-20` | Membuat pesanan Radiologi dari kunjungan MCU | Radiologi | `RadOrderController` `POST` (`:301`), `CreateRadOrderRequest` — `EncounterId`, `ProcedureId`, `ModalityId`, `InstructingDoctorId` | `Reuse with adapter` | Perlu memilih modalitas per komponen. Jenis kunjungan hanya dipakai sebagai penyaring daftar (`RadOrderService.cs:880-885`), tidak sebagai larangan saat membuat pesanan | Rendah |
| `MCU-CAP-21` | Tanda *bagian paket MCU* pada pesanan Lab/Rad (`MCU-DEC-005`) | Lab / Rad | `LabOrder` dan `RadOrder` tidak punya ruas asal/paket/sumber (daftar ruas lengkap dibaca dari model) | `Missing` | Inti `MCU-COORD-002` | **Tinggi** — tanpa tanda, tagih dobel |
| `MCU-CAP-22` | Lab/Rad **tidak** menagih komponen paket | Lab / Rad / Billing | Lab menerbitkan fakta tagih `LaboratoryCharge` saat spesimen (`LabSpecimenService.cs:1689-1740`); Radiologi `RadiologyCharge` saat akuisisi (`RadStudyService.cs:1500`); kontrak `BillingSourceContract.cs:39-52` | `Conflict` | Lihat `MCU-CONFLICT-007` | **Tinggi** |
| `MCU-CAP-23` | Status kelengkapan komponen dibaca MCU (`MCU-DEC-009`) | Lab / Rad | Status pesanan Lab (`LabOrderStatus`, `CompleteAsync`) dan Rad (`complete`) tersedia per pesanan | `Reuse with adapter` | MCU membaca status pesanan turunannya; caranya ditetapkan saat desain | Rendah |

### 3.5 Billing

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `MCU-CAP-24` | Tagihan kunjungan MCU berjenis `MCU`, kena PPN | Billing | `BillingInvoiceService.cs:2552` `MedicalCheckup → "MCU"`; `BKC-DEC-083` | `Ready to reuse` | — | Rendah |
| `MCU-CAP-25` | Menambah satu baris paket ke tagihan (`MCU-DEC-005`) | Billing | `BillingInvoicesController` `POST invoices/catalog-charges` (`:324`, izin `BillingInvoice : Create`) — `AddCatalogChargeRequest { EncounterId, TariffId, Quantity, CorrelationId, CausationId }` | `Reuse with adapter` | Butuh `MstTariff` paket (`MCU-CAP-13`). Harga khusus perusahaan tidak terbaca jalur ini (`MCU-CAP-12`) | Sedang |
| `MCU-CAP-26` | Biaya administrasi untuk MCU | Billing | `AdministrationFeeServiceTypes` hanya `RAJAL`, `IGD`, `OTC`, `RANAP` (`BE-BKC-026`) | `Unknown` | Apakah MCU dikenai biaya administrasi belum diputuskan (`MCU-Q-05`) | Rendah |
| `MCU-CAP-27` | Pengembalian dana per komponen dan batal penuh (`MCU-DEC-015`, `021`) | Billing | `BillingFinancialExceptionsController` (base `.../billing/financial-exceptions`) — `GET /api/v1/.../billing/invoices/{id}/refundable-items` (`:121`), `POST /api/v1/.../billing/invoices/{id}/refunds` (`:155`), `POST refunds/{id}/approve` (`:235`); model `BilRefundCase`, `BilRefundLine` | `Reuse with adapter` | Refund menerima baris terpilih (`SelectedBillingItemIds`) **dan** nominal (`RequestedAmount`), dan setiap baris membawa `RefundableAmount` (`BillingRefundDtos.cs:5-29`). Nilai pengembalian komponen dikirim sebagai nominal sebagian dari baris paket; apakah validasinya menerima nominal di bawah nilai baris belum dibaca (`MCU-UNK-04`). **Pembaruan 2026-10-21:** terbukti **tidak** — `RequestedAmount` diabaikan, nominal selalu baris penuh; status efektif menjadi `Extension` di Billing (`MCU-EVD-002` 2.2, `MCU-DEC-043`) | Sedang |
| `MCU-CAP-28` | Tagihan kolektif ke perusahaan | Billing | `BillingCompanyGuarantorInvoiceDocumentService`; `BilArHandoff` (serah ke piutang); `BilInvoiceItemPayerAssignment` | `Reuse with adapter` | Tagihan tetap per kunjungan lalu diserahkan ke piutang perusahaan; rekap per rombongan MCU belum ada | Rendah |

### 3.6 Skrining, hasil, pengiriman

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `MCU-CAP-29` | Skrining perawat untuk kunjungan MCU (`MCU-DEC-018`) | Rawat Jalan / Registrasi | Antrean dibuat menurut klinik/unit, tidak menurut jenis kunjungan (`PatientEncounterController.cs:793` `isQueueRequired = !isEmergency && (clinic?.IsQueueRequired ?? serviceUnit.IsQueueRequired)`); `NurseStationQueueController` `start-screening` (`:246`), `finish-screening` (`:277`) tanpa penyaring jenis kunjungan | `Reuse with adapter` | Kunjungan MCU harus memakai klinik MCU yang `IsQueueRequired`; MCU membaca *skrining selesai* dari status antrean/kunjungan. Belum dijalankan (`MCU-UNK-05`) | Sedang |
| `MCU-CAP-30` | Isian *Pemeriksaan MCU*, resume, approve, verifikasi, koreksi berversi (`MCU-DEC-007`, `008`, `011`) | MCU | Nol model. `TrxPatientAssessment` milik Clinical Management untuk pengkajian; `TrxPatientClinicalDocument` punya status review/verify/approve tetapi satu izin `Update` untuk semuanya (`LAB-CONFLICT-016`) | `Missing` | Dokumen klinis **tidak** cocok untuk resume: verifikasi-sendiri dan izin tunggal bertentangan dengan `MCU-DEC-008` | Sedang |
| `MCU-CAP-31` | Hasil MCU tampil di rekam medis | Rekam Medis | Linimasa membaca `TrxPatientClinicalDocument` (`MedicalRecordTimelineService.cs:398-405`); rekam medis mencatat MCU sebagai produsen hasil yang menyusul (`rekam-medis/00-interview-decisions.md:81`) | `Unknown` | Titik sentuh untuk desain; bukan blocker | Rendah |
| `MCU-CAP-32` | Kirim berkas lewat WhatsApp/email dari server (`MCU-DEC-019`) | Platform | `NTF-CAP-18` (`platform-notification/01-existing-capability-map.md:128`): nol SMTP/MailKit/SendGrid/gerbang WhatsApp di BE; `rg` ulang pada SHA ini juga nol | `Missing` | Inti `MCU-COORD-005` | Tinggi bagi butir 12 |
| `MCU-CAP-33` | Komponen kirim WhatsApp di frontend | FE | FE v2 `components/features/whast-app/wa-modal.jsx` mengimpor `@/lib/state/slice/whastApp-api/whatsAppSlice` yang **tidak ada** di v2, dan nol berkas v2 mengimpor modal ini. FE1 `lib/state/slice/whastApp-api/` mengirim PDF **dari browser** ke `http://103.153.60.136:3000` | `Conflict` | Lihat `MCU-CONFLICT-008` | **Tinggi** — privasi |

### 3.7 Frontend dan tata kelola

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `MCU-CAP-34` | Menu dan layar MCU di v2 | FE MCU | Nol route `app/**/medical-check*`/`mcu` di FE v2. `src/utils/config.jsx:138` memuat menu *Pasien Perjanjian MCU* → `/perjanjian/MCU`, tetapi route itu tidak ada dan `utils/config` tidak diimpor berkas mana pun | `Missing` | Menu `config.jsx` adalah sisa salinan v1, bukan menu hidup | Rendah |
| `MCU-CAP-35` | Referensi tampilan v1 | FE1 | `app/Medical-check-up/` (list, order tambah/ubah, pendaftaran), `components/view/MCU/` (4.441 baris). Order MCU = `BaseOrderPenunjang labType="MCU"` dengan jumlah harga per pemeriksaan (`baseOrderPenunjang.jsx:415-418`); data dari `/LabBooking/pagedLabMCU`, `/LabBookingDetail/paged`, `/PendaftaranPasienMCU` (backend lama) | `Reuse with adapter` | Hanya tampilan: daftar per kunjungan, modal billing, modal unggah surat jaminan. Endpoint v1 tidak ada di BE v2 (`rg "LabBooking"` nol) | Rendah |
| `MCU-CAP-36` | Label jenis kunjungan MCU di daftar Lab/Rad | Lab / Rad | `LabFilterMetadataFactory.cs:411` "Medical Check Up"; FE `rad-patient-list-rules.js:140`, `lab-monitoring-rules.js:297` | `Ready to reuse` | — | Rendah |
| `MCU-CAP-37` | Pola izin | Platform | `[AccessPermission("<Resource>", "<Action>")]` + `[AccessAction]` per endpoint; kebijakan per departemen/posisi | `Ready to reuse` | Resource MCU baru didaftarkan saat implementasi | Rendah |
| `MCU-CAP-38` | Prefix kepemilikan modul MCU | Tata kelola | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:47-69` — nol prefix MCU | `Missing` | Wajib didaftarkan sebelum model pertama (`QBE-MOD-002`, `QBE-MOD-003`) | Memblokir implementasi BE |

---

## 4. Pertentangan

Penomoran melanjutkan `MCU-CONFLICT-004` pada decision log.

| ID | Pertentangan | Contoh | Keputusan yang bertentangan |
| --- | --- | --- | --- |
| `MCU-CONFLICT-005` | Booking rawat jalan **membuat kunjungan saat booking**; MCU memutuskan kunjungan baru dibuat **saat check-in** | Bila booking MCU meniru perjanjian rawat jalan, Budi yang booking 27 Oktober sudah punya kunjungan sejak 23 Oktober, dan tagihannya terbuka meski ia tidak datang | `MCU-DEC-003` lawan `MCU-CAP-04`. **Bukan** cacat Registrasi; artinya booking MCU tidak memakai perjanjian rawat jalan |
| `MCU-CONFLICT-006` | Kiosk mendaftarkan "Klinik Medical Check Up" sebagai kunjungan **rawat jalan** | Siti mendaftar MCU di kiosk → tercatat `Outpatient` di klinik MCU, tidak muncul di Daftar Pasien MCU, dan rekap kunjungan MCU kehilangan Siti | `MCU-DEC-017` lawan `MCU-CAP-06`. Pemilik `kiosk` |
| `MCU-CONFLICT-007` | Lab dan Radiologi **menagih per pemeriksaan secara otomatis**; MCU memutuskan paket ditagih **satu baris** | Budi membayar Paket Basic Rp 350.000. Saat spesimen diterima Lab menerbitkan Darah Lengkap Rp 150.000 dan Gula Darah Puasa Rp 60.000; saat Thorax diambil Radiologi menerbitkan Rp 200.000. Tagihan menjadi Rp 760.000, dan karena gerbang lunas dihitung per kunjungan, kunjungan Budi kembali *belum lunas* | `MCU-DEC-005` lawan `MCU-CAP-22`. Pemilik Lab, Radiologi, Billing (`MCU-COORD-002`) |
| `MCU-CONFLICT-008` | Pengiriman WhatsApp v1 berjalan **dari browser ke server eksternal** tanpa melewati backend | Berkas hasil Siti dibuat di browser petugas lalu dikirim langsung ke `103.153.60.136:3000`; backend tidak tahu apa yang terkirim, sehingga riwayat pengiriman (`MCU-DEC-019`) dan pembatasan ringkasan perusahaan (`MCU-DEC-010`) tidak dapat ditegakkan | `MCU-DEC-010`, `MCU-DEC-019` lawan `MCU-CAP-33`. Pola v1 **tidak boleh** ditiru |

---

## 5. Unknown

| ID | Hal | Yang dibutuhkan untuk menjawab |
| --- | --- | --- |
| `MCU-UNK-01` | Pembuatan kunjungan `MedicalCheckup` lewat `POST admin` berjalan sungguhan (kelas pasien, klinik, antrean) | Satu panggilan API ke basis data pengembangan — **penulisan**, butuh izin pemilik modul |
| `MCU-UNK-02` | Apakah status `NeedGuaranteeLetter` pernah menahan apa pun | Telusur pemakai status penjamin di Registrasi dan Billing |
| `MCU-UNK-04` | Bentuk permintaan sudah memuat nominal (`RequestedAmount`); yang belum dibaca adalah validasi nominal sebagian di service refund | Baca service di balik `POST invoices/{id}/refunds` — **ditutup 2026-10-21: tidak didukung** (`MCU-EVD-002` 2.2) |
| `MCU-UNK-05` | Skrining perawat untuk kunjungan MCU berjalan sungguhan dan status *skrining selesai* dapat dibaca | Uji berjalan sesudah `MCU-UNK-01` |
| `MCU-UNK-06` | Data master: apakah klinik "Klinik Medical Check Up" dan unit layanan MCU sudah ada di basis data, dan `IsQueueRequired`-nya | Kueri baca-saja ke basis data |

---

## 6. Kontrak as-is yang disentuh

#### `[Tags("Health Services / Registration Management / Patient Encounter")]`

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/admin` | Membuat kunjungan; menerima `EncounterType = MedicalCheckup` | `PatientEncounter : Create` | `PatientEncounterCreateRequest` | `PatientEncounterCreateResponse` |
| `POST` | `/kiosk` | Membuat kunjungan dari kiosk | Kebijakan kiosk | `PatientEncounterCreateRequest` | `PatientEncounterCreateResponse` |
| `PATCH` | `/admin/{id}/check-in` | Mengisi waktu check-in | `PatientEncounter : Update` | - | `object` |
| `PATCH` | `/admin/{id}/cancel` | Membatalkan kunjungan | `PatientEncounter : Update` | Alasan | `object` |

#### `[Tags("Health Services / Laboratory Management / Lab Order")]` — titik sentuh

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses | Catatan |
| --- | --- | --- | --- | --- |
| `POST` | `/by-examinations` | Membuat pesanan per pemeriksaan | Izin pesanan Lab | Tanpa ruas asal/paket |
| `PUT` | `/{id}/start-process` | Proses Pemeriksaan; **`409`** bila kunjungan tunai belum lunas | Izin pesanan Lab | `LabPaymentClearanceRules` |

#### `[Tags("Health Services / Radiology Management / Rad Order")]` — titik sentuh

Base URL: `api/v1/health-services/radiology-management/rad-orders`

| Method | Path | Kegunaan | Catatan |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pesanan | `EncounterId`, `ProcedureId`, `ModalityId`; tanpa ruas asal/paket |
| `PUT` | `/{id}/accept`, `/start`, `/complete` | Alur kerja | **Tanpa** gerbang lunas |

#### `[Tags("Health Services / Billing Management / Billing")]` — titik sentuh

Base URL: `api/v1/health-services/billing-management`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/billing/invoices/catalog-charges` | Menambah baris tagihan dari `MstTariff` | `BillingInvoice : Create` |
| `GET` | `/billing/invoices/{id}/refundable-items` | Baris yang dapat dikembalikan | Izin Billing |
| `POST` | `/billing/invoices/{id}/refunds` | Mengajukan pengembalian dana | Izin Billing |
| `POST` | `/billing/financial-exceptions/refunds/{id}/approve` | Menyetujui pengembalian | Izin Billing |

---

## 7. Pertanyaan penutup untuk `/grill-me` (closure pass)

> **Status 2026-10-20:** ketujuh pertanyaan dan `MCU-CONFLICT-005`..`008` sudah ditutup pada closure pass — `MCU-DEC-022`..`MCU-DEC-028` di [00-interview-decisions.md](00-interview-decisions.md) revision 2.

| ID | Pertanyaan | Kenapa perlu | Memblokir |
| --- | --- | --- | --- |
| `MCU-Q-01` | **Bagaimana mencegah tagih dobel** (`MCU-CONFLICT-007`)? Pilihan yang terlihat: Lab/Rad **tidak menerbitkan** fakta tagih untuk pesanan bertanda paket; **atau** tetap menerbitkan dengan harga nol; **atau** Billing membatalkan baris turunan paket | Menentukan siapa yang berubah: Lab/Rad atau Billing | `DESIGN` |
| `MCU-Q-02` | **Pintu kiosk** (`MCU-CONFLICT-006`): kiosk membuat kunjungan `MedicalCheckup` sendiri, atau kiosk hanya membuat order MCU *Hadir* lalu MCU yang membuat kunjungan? | Menentukan pemilik pembuatan kunjungan dan perubahan di modul kiosk | `DESIGN` |
| `MCU-Q-03` | **Radiologi tanpa gerbang lunas** (`MCU-CAP-18`): apakah butir 11 menuntut Radiologi menahan peserta MCU tunai yang belum bayar, atau cukup karena MCU sendiri baru membuat pesanan Rad sesudah lunas? | Menentukan apakah Radiologi perlu berubah | `DESIGN` |
| `MCU-Q-04` | **Harga khusus perusahaan** (`MCU-CAP-12`): disimpan di master MCU (paket × perusahaan × masa berlaku), atau sebagai perluasan master tarif Billing? | Menentukan pemilik data harga | `DESIGN` |
| `MCU-Q-05` | Apakah kunjungan MCU dikenai **biaya administrasi** (`MCU-CAP-26`)? | `AdministrationFeeServiceTypes` belum punya `MCU` | `IMPLEMENTATION` |
| `MCU-Q-06` | **Pengiriman WhatsApp** (`MCU-CONFLICT-008`): rilis pertama menunggu platform, atau rilis pertama hanya *ambil langsung* + catatan manual sambil platform menyusul? | Tombol Kirim bergantung kemampuan yang belum ada | `IMPLEMENTATION` |
| `MCU-Q-07` | Bolehkah satu panggilan API ke basis data pengembangan dipakai untuk menjawab `MCU-UNK-01` dan `MCU-UNK-05`? | Tanpa layar pembuat kunjungan MCU, bukti berjalan hanya lewat API | Penutupan unknown |

---

## 8. Pemicu impact scan berikutnya

Peta ini dianggap basi dan harus dipindai ulang terbatas bila salah satu berikut berubah:

- `PatientEncounterController.cs` jalur pembuatan kunjungan atau `ResolvePatientClassAsync`;
- `LabPaymentClearanceRules.cs`, `LabSpecimenService.cs` (penerbitan tagih), `RadStudyService.cs`
  (penerbitan tagih), atau gerbang lunas ditambahkan di Radiologi;
- `LabOrder`/`RadOrder` mendapat ruas asal/sumber;
- `MstTariff`, `MstCompanyGuarantor`, atau `MstCompanyGuarantorCoverageRule` berubah;
- `BillingFinancialExceptionsController` (refund) atau `AdministrationFeeServiceTypes` berubah;
- pengirim WhatsApp/email muncul di backend;
- registry prefix modul mendapat entri MCU;
- kiosk mulai mengirim `encounterType = 4`.
