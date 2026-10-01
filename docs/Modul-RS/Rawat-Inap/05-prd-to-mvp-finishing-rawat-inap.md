# PRD to MVP — Finishing Rawat Inap (Paritas Bisnis V1)

| Field | Nilai |
|---|---|
| PRD ID | `PRD-RWI-FINISHING-001` |
| Versi | `0.1` |
| Status | **`DRAFT` — menunggu keputusan pemilik produk.** Dokumen ini belum menjadi approval desain dan belum mengganti `baseline_requirement` blueprint |
| Tanggal | 1 Oktober 2026 |
| Modul | Rawat Inap (`InPatientManagement`), dengan modul pendukung: Billing, Kamar Operasi, Gizi, Bank Darah, Laboratorium, Radiologi, Farmasi, Clinical Management |
| Pemilik produk | Product/Domain Owner Rawat Inap (ditetapkan `RWI-DEC-061`); pemilik modul Billing untuk bagian tagihan |
| Dasar dokumen | Audit paritas V1 vs Final tanggal 30 September 2026, capture V1 di `QuilvianV1/QuilvianSystemFrontendDev/captures/`, dan pembacaan source pada SHA di bagian 1.2 |
| Hubungan dokumen | **Melengkapi**, tidak menggantikan, `PRD-RWI-V2-001` (`04-prd-to-mvp-final.md`). PRD V2 mengatur tata letak ruang kerja; PRD ini mengatur kemampuan bisnis yang belum berfungsi |
| Jalur pengesahan | `grill-me` (Amendment Pass) → `requirement-completeness-gate` → amandemen blueprint lewat `design-business-module` → `plan-module-delivery` → `build-module-backend` / `build-module-frontend` |

---

## 0. Cara membaca dokumen ini

Dokumen ini menjawab satu pertanyaan: **apa yang harus selesai supaya Rawat Inap QuilvianFinal siap dipakai rumah sakit dengan bisnis yang sama dengan V1, tetapi tanpa kelemahan V1.**

Istilah yang dipakai:

| Istilah | Arti |
|---|---|
| V1 | QuilvianV1, sistem lama yang alur bisnisnya sudah disetujui client. **Hanya dibaca**, tidak diubah |
| Final | QuilvianFinal, sistem baru yang akan dipakai rumah sakit. Satu-satunya tempat perubahan kode |
| Bangsal | Ruang rawat inap tempat perawat dan dokter bekerja |
| OK | Kamar operasi (instalasi bedah) |
| Folio | Catatan tagihan berjalan milik Billing per kunjungan, sebelum menjadi invoice |
| Invoice | Tagihan resmi yang dibayar pasien atau penjamin di kasir |
| Clearance | Izin pulang dari kasir: tagihan sudah beres atau dijamin |
| *Placeholder* | Layar yang hanya bertuliskan "Integrasi belum tersedia" dan tidak menyimpan apa pun |

Label status kemampuan memakai taksonomi baku Quilvian:

| Label | Arti untuk pembaca umum |
|---|---|
| `READY TO REUSE` | Sudah ada dan bisa dipakai apa adanya |
| `REUSE WITH ADAPTER` | Sudah ada di modul lain, tinggal disambungkan |
| `EXTEND` | Sudah ada sebagian, perlu ditambah |
| `REPAIR` | Sudah ada tetapi salah atau rusak |
| `MISSING` | Belum ada sama sekali |
| `CONFLICT` | V1 dan Final berbeda arti, atau ada dua sumber yang saling bertentangan |
| `UNKNOWN` | Belum bisa dipastikan tanpa uji di aplikasi berjalan |

---

## 1. Latar belakang

### 1.1 Masalah

| Sistem | Kelebihan | Kelemahan |
|---|---|---|
| V1 | Alur bisnis sudah cocok dengan kebutuhan operasional dan sudah dipresentasikan ke client | Backend rapuh. Contoh nyata: tagihan pemakaian alat dibuat tanpa harga (bagian 13.2) |
| Final | Backend lebih kuat: audit, versi data, hak akses, idempotensi | Banyak alur bisnis V1 belum berfungsi: menu masih *placeholder*, tagihan rawat inap tidak masuk kasir |

Roadmap Rawat Inap di Final sudah menandai seluruh 128 task backend ✅. Audit menunjukkan tanda ✅ itu belum berarti fungsi bisnisnya berjalan. Contoh: task pengiriman event ke Billing ✅ karena status pesan berubah "Published", padahal pesannya tidak pernah sampai ke Billing.

### 1.2 Versi source yang diaudit

| Repository | Branch | SHA |
|---|---|---|
| `NewQuilvianSystemBackend` (Final) | `MHamzah` | `c8e99ce5` |
| `QuilvianSystemFrontendDev` (Final) | `HamzahV2` | `7a83e8574` |
| `QuilvianSystemBackendDev` (V1) | `QuilvianSta` | `4be1499c` |
| `QuilvianSystemFrontendDev` (V1) | `MHamzah` | `86408f245` |

Bila SHA saat implementasi berbeda, temuan di bagian 2 wajib dicek ulang sebelum dijadikan task.

### 1.3 Tujuan produk

1. Perawat dan dokter bisa menjalankan **seluruh delapan menu keperawatan V1** tanpa ada menu kosong, kecuali Rehab Medik yang modulnya belum ada.
2. **Setiap layanan yang diberikan kepada pasien rawat inap muncul di tagihan kasir secara otomatis**, dengan harga dari master tarif, bukan diketik ulang.
3. Pasien hanya bisa pulang fisik bila kasir sudah memberi izin, atau supervisor memakai jalur darurat yang tercatat.
4. Kelemahan V1 yang sudah terbukti **tidak ikut disalin**.

---

## 2. Ringkasan hasil audit

### 2.1 Temuan per kemampuan

| No | Kemampuan | V1 | Final saat ini | Status |
|---:|---|---|---|---|
| 1 | Webhook izin pulang dari kasir | — | Bisa dipanggil **tanpa login** (`[AllowAnonymous]`), tanpa tanda tangan atau kunci rahasia | `REPAIR` — keamanan |
| 2 | Sumber status izin pulang | Satu | Tiga tempat: `InpFinancialClearance`, `InpEpisode.ClearanceStatus`, `BilInpatientClearanceHandoff` | `CONFLICT` |
| 3 | Sinyal izin pulang Billing → Rawat Inap | Ada | Billing tidak pernah mengirim. Tombol pulang fisik hanya terbuka lewat supervisor override | `REPAIR` |
| 4 | Invoice rawat inap terbentuk | Otomatis dari setiap layanan | Tidak pernah terbentuk otomatis. Hanya terbentuk bila kasir menambah biaya manual | `REPAIR` |
| 5 | Tindakan, lab, radiologi, obat, konsul ke invoice | Otomatis | Tercatat di folio, tetapi jembatan ke invoice menolak semua kunjungan selain rawat jalan (`NOT_OUTPATIENT`) | `EXTEND` |
| 6 | Tarif kamar | Kamar × jumlah hari | **Sudah ada** di Billing (`BKC-DEC-043`): dihitung ulang dari data penempatan bed dan master kebijakan tarif kamar. Tetapi baru tampil bila invoice rawat inap sudah ada (nomor 4) | `READY TO REUSE` setelah nomor 4 diperbaiki |
| 7 | Hitungan tarif kamar kedua | — | Ada layanan kedua `InpatientRoomChargeCalculationService` dengan aturan jam tertanam di kode dan tarif karangan Rp 500.000 / Rp 1.500.000. Tidak dipakai, kecuali satu endpoint yang tidak dipanggil siapa pun | `CONFLICT` |
| 8 | Biaya administrasi | Tampil di Tagihan | **Sudah ada** di Billing dengan kebijakan dan batas maksimum. Hanya dihitung bila invoice punya minimal satu item | `READY TO REUSE` + aturan tepi perlu dicek |
| 9 | Pengiriman event Rawat Inap ke Billing (outbox) | — | Pengirim hanya menulis log lalu menandai "Published" | `REPAIR` |
| 10 | Tagihan Pasien di layar perawat | Rincian per item dengan rupiah | Hanya total dari folio, tanpa tarif kamar. Menampilkan rupiah, padahal `RWI-DEC-160` melarang perawat melihat rupiah | `EXTEND` + `CONFLICT` |
| 11 | Penunjang: Lab dan Radiologi oleh perawat | Pesan + hasil | Hanya baca. Tombol pesan dikunci "menunggu BE-RWI-104", padahal `BE-RWI-104` sudah ✅ | `REPAIR` |
| 12 | Penunjang: Gizi | Pesan | *Placeholder*, padahal modul Gizi Final sudah lengkap | `REUSE WITH ADAPTER` |
| 13 | Penunjang: Bank Darah | Pesan | *Placeholder*, padahal modul Bank Darah Final sudah lengkap | `REUSE WITH ADAPTER` |
| 14 | Penunjang: Hemodialisa | Pesan | Sudah berfungsi untuk dokter dan perawat | `READY TO REUSE` |
| 15 | Penunjang: Rehab Medik | Pesan | *Placeholder*; tidak ada modul backend | `MISSING` — ditunda |
| 16 | Pemesanan Ruangan Bedah (Bedah Operasi, Bedah Obgyn) | Ada | *Placeholder*, padahal modul Kamar Operasi Final sudah lengkap. Belum ada jenis kasus Obgyn/SC dan jenis anestesi saat pemesanan | `REUSE WITH ADAPTER` + `EXTEND` |
| 17 | Catatan Pra-Operasi di bangsal | Ada | Tidak ada. OK hanya punya checklist keselamatan WHO | `MISSING` |
| 18 | Serah terima pasien dari OK kembali ke bangsal | Lewat form Transfer | OK bisa mengirim serah terima ke unit tujuan, tetapi Rawat Inap tidak membacanya | `MISSING` (sambungan) |
| 19 | Biaya operasi ke tagihan | Dibuat saat booking | Pengiriman OK → Billing ditahan: "kontraknya belum tersedia" | `MISSING` |
| 20 | Catatan Keperawatan (6 sub-menu) | Ada | Arti berbeda: Final hanya catatan naratif CPPT. Sebagian isi V1 tersebar di menu lain | `CONFLICT` |
| 21 | Observasi WSD per shift | Ada | Hanya kategori cairan "Drain/WSD" berisi volume | `EXTEND` |
| 22 | Efek Samping Obat (ESO) | Ada | Backend ada, layar tidak ada | `REUSE WITH ADAPTER` |
| 23 | Diet Medis di bangsal | Ada | Backend Gizi punya diet per pasien, bangsal tidak punya layar | `REUSE WITH ADAPTER` |
| 24 | Pemakaian Alat (alat besar) | Ada, dengan tagihan | *Placeholder*; tidak ada master alat medis bertarif; ditunda `RWI-DEC-089` | `MISSING` |
| 25 | Katalog tindakan rawat inap | — | Memakai filter rawat jalan (`IsAvailableForOutpatient`), sehingga tindakan khusus rawat inap tidak muncul | `REPAIR` |
| 26 | Hak lihat rupiah di ringkasan Billing | — | Ditentukan dari nama peran (`Contains("Admin")`, `"Cashier"`) | `REPAIR` |
| 27 | Serah terima klinis saat transfer | 9 bagian klinis | Hanya pindah bed; bagian klinis *placeholder* (`RWI-DEC-113`) | `MISSING` |
| 28 | Resume Medis ODC | Ada | *Placeholder* | `MISSING` |
| 29 | Transfusi darah | Komponen ada, tetapi dimatikan dari menu V1 | Tidak ada | `MISSING` — ditunda |

### 2.2 Bukti source

Format bukti: `repository/path:baris@SHA`.

| No | Bukti |
|---:|---|
| 1 | `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeClearanceController.cs:49-53@c8e99ce5` |
| 2 | `.../InPatientManagement/Services/InpatientClearanceGateService.cs:154@c8e99ce5`; `.../InPatientManagement/Services/InpDischargeService.Closure.cs:303@c8e99ce5`; `.../BillingManagement/Operational/Services/PatientBillingSummaryService.cs:88@c8e99ce5` |
| 3 | Pencarian `webhook`, `InpFinancialClearance`, `ClearanceGate` di `Areas/HealthServices/BillingManagement/**` mengembalikan nol hasil @c8e99ce5 |
| 4 | `.../BillingManagement/Billing/Services/BillingInvoiceService.cs:1528-1547@c8e99ce5` — satu-satunya tempat invoice dibuat |
| 5 | `.../BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs:111-112@c8e99ce5` |
| 6 | `.../BillingManagement/Billing/Services/BillingCalculationService.cs:196-198, 612-741@c8e99ce5` |
| 7 | `.../BillingManagement/Billing/Services/InpatientRoomChargeCalculationService.cs:242, 427-466@c8e99ce5` |
| 8 | `.../BillingManagement/Billing/Services/BillingCalculationService.cs:189-195, 515-574@c8e99ce5` |
| 9 | `.../InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs:129-145@c8e99ce5` |
| 10 | `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx:218@7a83e8574` |
| 11 | `.../nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx:189-205@7a83e8574` |
| 12–15 | `.../nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx:35-39@7a83e8574`; `.../physician-workspace/tabs/supporting-service/supporting-service-tab.jsx:164-167@7a83e8574` |
| 16 | `.../nursing-workspace/components/nursing-workspace-sections.jsx:90-99@7a83e8574`; `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/DTOs/OperatingRoomCaseDtos.cs:24-38@c8e99ce5` |
| 18 | `.../OperatingRoomManagement/Services/OperatingRoomRecoveryService.cs:246-376@c8e99ce5`; pencarian `OprHandover` di `InPatientManagement` nol hasil |
| 19 | `.../OperatingRoomManagement/Services/OperatingRoomIntegrationService.cs:31-40@c8e99ce5` |
| 20 | `.../nursing-workspace/sections/nursing-care/tabs/nursing-narrative-tab.jsx:81@7a83e8574` |
| 21 | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Enums/FluidSourceCategory.cs:35-36@c8e99ce5` |
| 22 | `.../ClinicalManagement/Controllers/PatientAllergyController.cs:451-464@c8e99ce5` |
| 24 | `.../nursing-workspace/components/nursing-workspace-sections.jsx:75-84@7a83e8574`; `NewQuilvianSystemBackend/Areas/HealthServices/MasterData/Models/MstTariff.cs@c8e99ce5` (hanya `ProcedureId` dan `DrugId`) |
| 25 | `.../ClinicalManagement/Controllers/PatientProcedureController.cs:128-136@c8e99ce5`; `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx:15@7a83e8574` |
| 26 | `.../BillingManagement/Billing/Controllers/InpatientClearanceController.cs:131-133@c8e99ce5` |
| 27 | `.../nursing-workspace/sections/transfer/nursing-transfer-form-panel.jsx:1-34@7a83e8574` |
| 28 | `.../physician-workspace/tabs/resume/resume-odc-panel.jsx@7a83e8574` |

---

## 3. Batas MVP Finishing

| Batas | Isi |
|---|---|
| **Titik mulai** | Pasien rawat inap berstatus `Admitted` dan menempati bed di bangsal |
| **Titik akhir** | Pasien pulang fisik, seluruh layanan selama dirawat tercantum di invoice kasir, dan invoice ditutup |
| **Di dalam MVP** | Kemampuan `P0` dan `P1` di bagian 5 |
| **Boleh menyusul dalam MVP bila waktu cukup** | Kemampuan `P2` |
| **Di luar MVP** | Kemampuan `DEFERRED` di bagian 5.4 |

**Definisi "selesai" untuk pemilik produk:** satu pasien uji bisa dirawat tiga hari dan menerima tindakan, lab, obat, pemakaian alat, serta operasi. Setelah itu kasir melihat semuanya di satu invoice tanpa mengetik ulang, lalu pasien dipulangkan lewat izin kasir. Skenarionya ada di `UAT-RWF-01`.

---

## 4. Prinsip produk

| ID | Prinsip | Contoh penerapan |
|---|---|---|
| `PR-RWF-01` | Kemampuan bisnis V1 dipertahankan, cara simpan V1 tidak disalin | Menu Pemakaian Alat V1 dipertahankan, tetapi tagihannya wajib berharga dan tidak menghapus tagihan modul lain |
| `PR-RWF-02` | Pakai modul Final yang sudah ada sebelum membuat yang baru | Pemesanan Gizi memakai modul Gizi Final, bukan tabel baru di Rawat Inap |
| `PR-RWF-03` | Tagih saat layanan benar-benar terjadi | Operasi ditagih saat kasus OK berstatus `Completed`, bukan saat dipesan seperti V1 |
| `PR-RWF-04` | Harga selalu dari master tarif di server | Layar tidak pernah mengirim harga; tidak ada angka tarif tertanam di kode |
| `PR-RWF-05` | Satu fakta, satu tempat simpan | Status izin pulang hanya punya satu sumber |
| `PR-RWF-06` | Pelaku diambil dari akun login | Nama perawat pengirim pra-operasi tidak diketik manual |
| `PR-RWF-07` | Hak akses lewat permission, bukan nama peran | `InpatientBillingOperational : ViewBillingDetails`, bukan "peran berisi kata Admin" |
| `PR-RWF-08` | Tidak ada data tiruan | Menu yang belum siap tetap *placeholder*, tanpa contoh data dan tanpa pesan berhasil palsu (`RWI-DEC-108`) |
| `PR-RWF-09` | ✅ hanya bila terbukti di aplikasi berjalan | Task billing baru selesai bila tagihan benar-benar tampil di layar kasir |

---

## 5. Kemampuan dalam MVP

### 5.1 `P0` — menahan go-live

| ID | Kemampuan | Status awal | Temuan |
|---|---|---|---|
| `CAP-RWF-01` | Gerbang pulang aman dengan satu sumber izin pulang | `REPAIR` + `CONFLICT` | 1, 2 |
| `CAP-RWF-02` | Invoice rawat inap terbentuk otomatis dan menerima seluruh layanan klinis | `REPAIR` + `EXTEND` | 4, 5, 6, 8 |
| `CAP-RWF-03` | Sinyal izin pulang dari Billing ke Rawat Inap | `REPAIR` | 3 |
| `CAP-RWF-04` | Pembersihan jalur tagihan ganda dan pengiriman palsu | `CONFLICT` + `REPAIR` | 7, 9 |

### 5.2 `P1` — wajib dalam MVP Finishing

| ID | Kemampuan | Status awal | Temuan |
|---|---|---|---|
| `CAP-RWF-05` | Tagihan Pasien di bangsal dengan susunan rincian seperti V1 | `EXTEND` + `CONFLICT` | 10 |
| `CAP-RWF-06` | Penunjang Medis lengkap dari bangsal: Lab, Radiologi, Gizi, Bank Darah | `REPAIR` + `REUSE WITH ADAPTER` | 11, 12, 13 |
| `CAP-RWF-07` | Pemesanan Ruangan Bedah: Bedah Operasi dan Bedah Obgyn | `REUSE WITH ADAPTER` + `EXTEND` | 16 |
| `CAP-RWF-08` | Pasien operasi terhubung ke bangsal: status, pra-operasi, serah terima, tagihan | `MISSING` | 17, 18, 19 |
| `CAP-RWF-09` | Catatan Keperawatan dengan susunan 6 sub-menu V1 | `CONFLICT` | 20 |
| `CAP-RWF-10` | Observasi WSD per shift | `EXTEND` | 21 |
| `CAP-RWF-11` | Layar Efek Samping Obat | `REUSE WITH ADAPTER` | 22 |
| `CAP-RWF-12` | Diet Medis di bangsal | `REUSE WITH ADAPTER` | 23 |
| `CAP-RWF-13` | Pemakaian Alat (alat medis besar) dengan tagihan | `MISSING` | 24 |
| `CAP-RWF-14` | Katalog tindakan khusus rawat inap | `REPAIR` | 25 |
| `CAP-RWF-15` | Hak lihat rupiah berbasis permission | `REPAIR` | 26 |

### 5.3 `P2` — sebaiknya ada, boleh menyusul

| ID | Kemampuan | Status awal | Temuan |
|---|---|---|---|
| `CAP-RWF-16` | Serah terima klinis saat transfer antar-ruang | `MISSING` | 27 |
| `CAP-RWF-17` | Resume Medis ODC | `MISSING` | 28 |

### 5.4 `DEFERRED` — di luar MVP

| Kemampuan | Alasan | Perilaku selama MVP |
|---|---|---|
| Rehab Medik | Modul backend belum ada | Tetap *placeholder* sesuai `RWI-DEC-108` |
| Transfusi darah | V1 sendiri mematikan menunya; pemilik klinis belum ditetapkan | Tidak tampil |
| Notifikasi seketika (WebSocket) | Tidak menahan alur bisnis | Penyegaran berkala seperti sekarang |

---

## 6. Epic dan kebutuhan fungsional

Setiap kebutuhan diberi kriteria penerimaan (`AC-RWF-###`) yang bisa diuji di aplikasi berjalan.

### EPIC-RWF-01 — Gerbang pulang aman (`CAP-RWF-01`, `CAP-RWF-03`)

| ID | Kebutuhan |
|---|---|
| `FR-RWF-001` | Endpoint yang mengubah status izin pulang **wajib** memerlukan login dan permission. Endpoint tanpa login dilarang |
| `FR-RWF-002` | Status izin pulang hanya punya **satu sumber**. Usulan: milik Billing (`OD-RWF-01`); Rawat Inap membacanya, tidak menyimpan salinan yang bisa berbeda |
| `FR-RWF-003` | Saat kasir menyetujui atau mencabut izin pulang, layar bangsal menampilkan status baru paling lambat pada penyegaran berikutnya (10 detik di modal pulang) |
| `FR-RWF-004` | Jalur penandaan izin pulang manual lama (`InpFinancialClearance`) tidak boleh menjadi dasar pulang normal, sesuai `RWI-DEC-102` |
| `FR-RWF-005` | Supervisor override tetap tersedia untuk kedaruratan klinis, dengan alasan wajib dan jejak audit |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-001` | Memanggil endpoint perubahan izin pulang tanpa token ditolak `401` |
| `AC-RWF-002` | Memanggil dengan akun tanpa permission ditolak `403` |
| `AC-RWF-003` | Kasir menyetujui izin pulang pukul 10.00.00 → tombol "Konfirmasi Pasien Pulang Fisik" aktif paling lambat 10.00.10 |
| `AC-RWF-004` | Kasir mencabut izin pulang → tombol terkunci kembali dan banner merah tampil |
| `AC-RWF-005` | Pencarian kode tidak menemukan lagi dua tempat simpan status izin pulang yang aktif bersamaan |

### EPIC-RWF-02 — Tagihan rawat inap masuk kasir (`CAP-RWF-02`, `CAP-RWF-04`)

| ID | Kebutuhan |
|---|---|
| `FR-RWF-010` | Invoice rawat inap (jenis layanan `RANAP`) **terbentuk otomatis** saat episode menjadi `Admitted`, tanpa menunggu kasir menambah biaya manual |
| `FR-RWF-011` | Tindakan, pemeriksaan lab, radiologi, obat yang diserahkan farmasi, dan konsultasi untuk kunjungan rawat inap **masuk ke invoice** lewat jembatan folio yang sama dengan rawat jalan |
| `FR-RWF-012` | Tarif kamar dihitung oleh Billing dari data penempatan bed dan master kebijakan tarif kamar (`BKC-DEC-043`). Rawat Inap tidak menghitung tarif sendiri |
| `FR-RWF-013` | Layanan `InpatientRoomChargeCalculationService` beserta endpoint `occupancy-charges` dipensiunkan, termasuk seluruh tarif dan aturan jam yang tertanam di kode |
| `FR-RWF-014` | Pengirim event outbox dilarang menandai pesan "Published" bila tidak ada penerima. Pilihannya diputuskan di `OD-RWF-01` |
| `FR-RWF-015` | Biaya administrasi rawat inap dihitung menurut kebijakan Billing, termasuk untuk invoice yang baru berisi tarif kamar. Aturan pastinya dikonfirmasi pemilik Billing |
| `FR-RWF-016` | Kegagalan Billing tidak boleh menggagalkan penyimpanan klinis. Tagihan yang gagal tercatat sebagai "menunggu sinkron" dan dapat dikirim ulang |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-010` | Pasien samaran "Tn. Contoh A" dinyatakan `Admitted` → invoice `RANAP` terbuka di kasir dalam waktu kurang dari 1 menit |
| `AC-RWF-011` | Perawat menandai tindakan "Pasang infus" `Completed` → baris tindakan muncul di invoice dengan harga dari master tarif |
| `AC-RWF-012` | Tindakan yang dibatalkan → barisnya dibatalkan (`Voided`) di invoice, tidak dihapus fisik |
| `AC-RWF-013` | Pasien menempati kamar kelas 2 selama 2 hari 5 jam (lihat contoh 7.1.6) → tarif kamar di invoice sama dengan hasil kebijakan tarif kamar yang aktif |
| `AC-RWF-014` | Tidak ada lagi angka tarif tertanam di kode Billing maupun Rawat Inap |
| `AC-RWF-015` | Billing dimatikan sementara, perawat tetap bisa menyimpan tindakan. Setelah Billing hidup lagi, baris tagihan muncul tanpa dobel |

### EPIC-RWF-03 — Tagihan Pasien di bangsal (`CAP-RWF-05`, `CAP-RWF-15`)

| ID | Kebutuhan |
|---|---|
| `FR-RWF-020` | Menu Tagihan Pasien menampilkan **susunan rincian seperti V1**, dikelompokkan: Kamar Rawat Inap, Tindakan, Penunjang Medis, Obat & Alkes, Pemakaian Alat, Operasi, dan Biaya Administrasi |
| `FR-RWF-021` | Kelompok Kamar menampilkan nama kamar, periode tanggal, dan jumlah unit (misal "3 hari") |
| `FR-RWF-022` | Siapa yang melihat angka rupiah mengikuti `OD-RWF-04`. Usulan: perawat melihat rincian **tanpa** rupiah (`RWI-DEC-160`); pengguna ber-permission `InpatientBillingOperational : ViewBillingDetails` melihat rupiah |
| `FR-RWF-023` | Sumber rincian adalah perhitungan invoice Billing, bukan hitungan ulang di Rawat Inap |
| `FR-RWF-024` | Hak lihat rupiah di seluruh endpoint Billing rawat inap ditentukan permission, bukan nama peran |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-020` | Perawat tanpa permission rincian melihat "Kamar Melati 2 — 1 s.d. 4 Okt 2026 — 3 hari" tanpa angka rupiah di tampilan maupun respons API |
| `AC-RWF-021` | Petugas admisi ber-permission melihat "3 hari × Rp 750.000 = Rp 2.250.000" (angka contoh) |
| `AC-RWF-022` | Mengganti nama peran dari "Cashier" menjadi "Kasir Shift Pagi" tidak mengubah siapa yang boleh melihat rupiah |

### EPIC-RWF-04 — Penunjang Medis lengkap (`CAP-RWF-06`)

| ID | Kebutuhan |
|---|---|
| `FR-RWF-030` | Perawat dapat memesan Lab dan Radiologi atas instruksi dokter. Pesanan wajib mencatat dokter pemberi instruksi (`BE-RWI-104`, `RWI-DEC-153`) |
| `FR-RWF-031` | Dokter dan perawat dapat memesan Konsultasi Gizi lewat modul Gizi Final. Rawat Inap tidak membuat tabel gizi |
| `FR-RWF-032` | Dokter dan perawat dapat memesan darah lewat modul Bank Darah Final |
| `FR-RWF-033` | Setiap layanan penunjang menampilkan status pesanan dan hasil dari modul pemiliknya. Bangsal hanya membaca hasil |
| `FR-RWF-034` | Harga dan status tanggungan penjamin per pemeriksaan tampil saat memilih pemeriksaan, seperti V1 ("Tidak Di-cover") |
| `FR-RWF-035` | Rehab Medik tetap *placeholder* |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-030` | Perawat memesan "Darah Lengkap" dan memilih dokter pemberi instruksi → pesanan masuk worklist Laboratorium |
| `AC-RWF-031` | Perawat tidak bisa menyimpan pesanan Lab tanpa dokter pemberi instruksi; pesan: "Dokter pemberi instruksi wajib dipilih" |
| `AC-RWF-032` | Dokter memesan konsultasi gizi dari bangsal → pesanan tampil di layar Gizi `nutrition-management/orders` |
| `AC-RWF-033` | Perawat memesan 2 kantong PRC → pesanan tampil di layar Bank Darah `blood-bank-management/blood-orders` |

### EPIC-RWF-05 — Pemesanan ruangan bedah dan pasien operasi (`CAP-RWF-07`, `CAP-RWF-08`)

| ID | Kebutuhan |
|---|---|
| `FR-RWF-040` | Menu Pemesanan Ruangan Bedah punya dua tab seperti V1: **Bedah Operasi** dan **Bedah Obgyn (SC/Caesar)** |
| `FR-RWF-041` | Pemesanan dari bangsal membuat kasus di modul Kamar Operasi Final, dengan konteks pasien dan kunjungan terisi otomatis |
| `FR-RWF-042` | Isian pemesanan minimal: tanggal dan jam yang diinginkan, tindakan operasi, dokter operator, jenis anestesi, prioritas, indikasi, sisi tubuh bila relevan, dan keterangan |
| `FR-RWF-043` | Kasus Obgyn/SC dibedakan dari bedah umum sesuai `OD-RWF-06` |
| `FR-RWF-044` | Bangsal melihat daftar kasus operasi pasiennya beserta statusnya: Diminta, Terjadwal, Siap, Sedang Operasi, Selesai, Ditunda, Dibatalkan |
| `FR-RWF-045` | Perawat bangsal mengisi **Catatan Pra-Operasi**: tanda vital, nyeri, checklist persiapan (verifikasi pasien, persiapan fisik, hasil pemeriksaan, persiapan lain), dan penandaan area operasi. Setiap butir dikonfirmasi perawat pengirim (bangsal) dan perawat penerima (OK) |
| `FR-RWF-046` | Saat OK mengirim serah terima pasca operasi ke bangsal, bangsal melihatnya dan dapat **menerima** atau **menolak dengan alasan** |
| `FR-RWF-047` | Biaya operasi masuk invoice rawat inap saat kasus berstatus `Completed`, bukan saat dipesan |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-040` | Perawat memesan bedah untuk pasien samaran → kasus baru berstatus `Requested` tampil di daftar kasus OK |
| `AC-RWF-041` | OK menjadwalkan kasus → status di bangsal berubah menjadi "Terjadwal" beserta tanggal dan jamnya |
| `AC-RWF-042` | Catatan pra-operasi dengan butir wajib belum dikonfirmasi penerima → OK menampilkan kendala kesiapan |
| `AC-RWF-043` | OK mengirim serah terima → bangsal menerima → status serah terima `Accepted` dan pelaku penerima diambil dari akun login |
| `AC-RWF-044` | Kasus dibatalkan sebelum `Completed` → tidak ada biaya operasi di invoice |

### EPIC-RWF-06 — Catatan Keperawatan susunan V1 (`CAP-RWF-09` s.d. `CAP-RWF-12`)

| ID | Kebutuhan |
|---|---|
| `FR-RWF-050` | Tab Catatan Keperawatan berisi enam sub-menu V1: **Spooling Cairan, Observasi Pengeluaran Cairan WSD, Sliding Scale, Daftar Pemberian Obat, Catatan Pra-Operasi, Diet Medis** |
| `FR-RWF-051` | Setiap sub-menu memakai data dan komponen Final yang sudah ada. Dilarang membuat salinan data (lihat tabel 8.2) |
| `FR-RWF-052` | Daftar Pemberian Obat punya empat bagian seperti V1: Pemberian Obat (MAR), Riwayat Pemberian, Efek Samping, Rekonsiliasi Obat |
| `FR-RWF-053` | Efek Samping memakai endpoint ADR yang sudah ada dan tampil di riwayat alergi/reaksi pasien |
| `FR-RWF-054` | Observasi WSD mencatat per shift: jam awal, jam akhir, sisa cairan di tabung shift ini, sisa shift sebelumnya, dan jumlah cairan bertambah yang **dihitung server** |
| `FR-RWF-055` | Diet Medis membaca dan menulis diet pasien di modul Gizi: jenis diet, status diet, diagnosa, keterangan, dan riwayat |
| `FR-RWF-056` | Catatan naratif perawat yang sekarang ada tetap tersedia di CPPT/SOAP Keperawatan, tidak hilang |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-050` | Enam sub-menu tampil berurutan sesuai V1, tanpa *placeholder* |
| `AC-RWF-051` | Intake 500 ml yang dicatat lewat Spooling Cairan juga tampil di Pengawasan Harian, karena datanya satu |
| `AC-RWF-052` | Observasi WSD: sisa shift lalu 200 ml, sisa shift ini 350 ml, tanpa pengosongan → jumlah bertambah = 150 ml dihitung server |
| `AC-RWF-053` | Efek samping yang dicatat dari dosis MAR tampil sebagai reaksi obat di riwayat alergi pasien |

### EPIC-RWF-07 — Pemakaian Alat medis besar (`CAP-RWF-13`)

| ID | Kebutuhan |
|---|---|
| `FR-RWF-060` | Ada master **Alat Medis** untuk alat besar, misalnya ventilator, high flow nasal cannula, syringe pump, dan infusion pump. Isinya: kode, nama, kategori, dan satuan tagih (per pemakaian, per jam, atau per hari) |
| `FR-RWF-061` | Tarif alat diatur di master tarif per kelas perawatan dan penjamin. Layar tidak pernah mengirim harga |
| `FR-RWF-062` | Menu Pemakaian Alat punya dua tab seperti V1: **Order Alat Kesehatan** dan **History Alat Kesehatan** |
| `FR-RWF-063` | Alat bersatuan waktu dicatat dengan **waktu mulai dan waktu selesai**; jumlah unit dihitung server. Alat per pemakaian dicatat dengan jumlah |
| `FR-RWF-064` | Pemakaian dicatat bersama dokter penanggung jawab dan perawat pelaksana dari akun login |
| `FR-RWF-065` | Pemakaian masuk invoice rawat inap lewat jalur tagihan yang sama dengan tindakan |
| `FR-RWF-066` | Koreksi atau pembatalan pemakaian hanya membatalkan baris tagihan milik pemakaian itu sendiri |
| `FR-RWF-067` | Alkes kecil/BMHP tetap di Obat & Alkes → Alat Kesehatan (farmasi). Pemakaian Alat khusus alat besar |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-060` | Ventilator dipakai 1 Okt 08.00 s.d. 3 Okt 11.00, satuan per hari dengan pembulatan ke atas → 3 hari tertagih (contoh 7.4) |
| `AC-RWF-061` | Membatalkan satu pemakaian ventilator tidak mengubah baris tagihan alkes farmasi pasien yang sama |
| `AC-RWF-062` | Alat tanpa tarif untuk kelas pasien → pemakaian tetap tersimpan, baris tagihan berstatus "tarif belum ada", kasir melihat peringatan; tidak ada harga karangan |

### EPIC-RWF-08 — Perbaikan pendukung (`CAP-RWF-14`, `CAP-RWF-16`, `CAP-RWF-17`)

| ID | Kebutuhan |
|---|---|
| `FR-RWF-070` | Katalog tindakan di ruang kerja rawat inap memakai penanda `IsAvailableForInpatient`, bukan `IsAvailableForOutpatient` |
| `FR-RWF-071` | (`P2`) Transfer antar-ruang menyimpan serah terima klinis: kondisi pasien (SOAP), GCS, tanda vital, nyeri dan risiko jatuh, balance cairan, barang yang diserahkan, instruksi khusus, pelaksana, dan penerima. Butuh keputusan pembukaan ulang `RWI-DEC-113` |
| `FR-RWF-072` | (`P2`) Resume Medis ODC tersedia di ruang kerja dokter. Pemilik dan isinya diputuskan `OD-RWF-09` |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-070` | Tindakan bertanda "hanya rawat inap" muncul di katalog bangsal dan tidak muncul di poliklinik |

---

## 7. Proses bisnis

### 7.1 `BP-RWF-01` — Tagihan rawat inap terbentuk otomatis

1. **Tujuan.** Semua layanan pasien rawat inap tercatat di satu invoice tanpa diketik ulang.
2. **Pelaku.** Petugas admisi membuka episode. Dokter dan perawat memberi layanan. Farmasi menyerahkan obat. Kasir memeriksa dan menutup invoice.
3. **Pemicu.** Episode berstatus `Admitted`.
4. **Prasyarat.** Master tarif kamar, tindakan, pemeriksaan, obat, dan alat sudah terisi. Ada satu kebijakan tarif kamar yang aktif.
5. **Langkah utama.**
   1. Petugas admisi mengesahkan admisi.
   2. Billing membuka invoice `RANAP` untuk kunjungan itu.
   3. Perawat mengonfirmasi pasien menempati bed; masa hunian mulai dihitung.
   4. Setiap tindakan selesai, hasil lab/radiologi diterima, obat diserahkan, pemakaian alat dicatat, dan operasi selesai → baris tagihan masuk invoice.
   5. Billing menghitung ulang tarif kamar dan biaya administrasi setiap invoice dihitung.
   6. Kasir memeriksa invoice, menerima pembayaran atau jaminan, lalu memberi izin pulang.
6. **Aturan bisnis.**
   - Harga dari master tarif sesuai kelas dan penjamin.
   - Layanan yang hanya dipesan belum ditagih.
   - Tarif kamar mengikuti kebijakan aktif: menit minimum, panjang periode, cara pembulatan sisa, dan saat tarif dibaca.
7. **Perubahan status.** Lihat tabel di bawah.
8. **Jalur tidak normal.**
   - Tarif belum ada → baris tetap tercatat dengan penanda "tarif belum ada", invoice tidak bisa difinalkan.
   - Billing sedang gangguan → baris "menunggu sinkron" lalu dikirim ulang.
   - Layanan dibatalkan → baris dibatalkan, tidak dihapus.
9. **Hasil akhir.** Invoice `RANAP` berisi semua layanan dan siap dibayar.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Admisi disahkan | Invoice `OPEN` | Sistem | Episode `Admitted` |
| Baris tagihan "menunggu sinkron" | Billing menerima | Baris `Recognized` | Sistem | Tarif ditemukan |
| Baris `Recognized` | Layanan dibatalkan | Baris `Voided` | Sistem, mengikuti pembatalan klinis | Invoice masih `OPEN` |
| Invoice `OPEN` | Kasir finalkan | Invoice final | Kasir | Tidak ada baris tanpa tarif |
| Invoice final | Butuh koreksi | Adjustment | Supervisor kasir | Alasan wajib |

#### 7.1.6 Contoh hitungan tarif kamar

Kebijakan aktif (contoh angka, bukan kebijakan rumah sakit): periode 1.440 menit (24 jam), minimum 1.440 menit, pembulatan sisa **ke atas**. Tarif kamar kelas 2 Rp 750.000 per periode.

Pasien menempati bed 1 Okt 2026 10.00 sampai pulang fisik 3 Okt 2026 15.00. Masa hunian 2 hari 5 jam = 3.180 menit.

Unit tertagih = pembulatan ke atas dari 3.180 ÷ 1.440 = pembulatan ke atas dari 2,21 = **3 unit**.

Tarif kamar = 3 × Rp 750.000 = **Rp 2.250.000**.

Bila kebijakan memakai pembulatan proporsional, unitnya 2,21 dan tarifnya 2,21 × Rp 750.000 = Rp 1.656.250. Angka pastinya selalu mengikuti master kebijakan yang aktif. Tidak ada aturan jam yang ditulis di kode.

### 7.2 `BP-RWF-02` — Izin pulang dari kasir sampai pasien pulang fisik

1. **Tujuan.** Pasien hanya meninggalkan bangsal bila urusan tagihan sudah beres, kecuali keadaan darurat.
2. **Pelaku.** Dokter DPJP memberi izin pulang medis. Kasir memberi izin pulang keuangan. Perawat mengonfirmasi pasien pulang fisik. Supervisor bangsal memegang jalur darurat.
3. **Pemicu.** Dokter DPJP memutuskan pasien boleh pulang.
4. **Prasyarat.** Invoice `RANAP` ada (`BP-RWF-01`).
5. **Langkah utama.**
   1. Dokter mencatat keputusan pulang.
   2. Kasir menilai ulang izin pulang dari invoice dan pembayaran.
   3. Billing menerbitkan status "Disetujui".
   4. Layar bangsal membaca status itu dan mengaktifkan tombol pulang fisik.
   5. Perawat mengonfirmasi pasien pulang fisik; bed dilepas dan berubah "Perlu Dibersihkan".
6. **Aturan bisnis.**
   - Satu sumber status izin pulang.
   - Endpoint perubahan status wajib login dan permission.
   - Akhir masa hunian kamar = waktu pulang fisik (`RWI-DEC-159`).
7. **Perubahan status.** Lihat tabel.
8. **Jalur tidak normal.**
   - Kasir mencabut izin (misal ada resep susulan) → tombol terkunci lagi.
   - Rujukan darurat → supervisor override dengan alasan minimal 20 karakter dan PIN.
9. **Hasil akhir.** Pasien keluar, bed dilepas, masa hunian kamar final.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| Menunggu penilaian | Kasir menyetujui | Disetujui | Kasir | Tagihan lunas atau dijamin |
| Disetujui | Kasir mencabut | Dicabut | Kasir | Alasan wajib |
| Dicabut | Kasir menyetujui ulang | Disetujui | Kasir | Kendala selesai |
| Menunggu / Dicabut | Override darurat | Override | Supervisor bangsal | Alasan + PIN; tercatat di audit |
| Disetujui / Override | Konfirmasi pulang fisik | Pasien keluar | Perawat | Bed aktif ada |

### 7.3 `BP-RWF-03` — Pasien operasi di rawat inap

1. **Tujuan.** Pasien rawat inap yang perlu operasi dipesankan ruang bedah dari bangsal, dioperasi di OK, kembali ke bangsal dengan serah terima, dan biayanya masuk tagihan rawat inap.
2. **Pelaku.** Dokter operator menentukan rencana operasi. Perawat bangsal memesan ruang dan menyiapkan pasien. Petugas OK menjadwalkan dan melaksanakan. Perawat OK menerima pasien dan mengirim serah terima balik. Perawat bangsal menerima serah terima.
3. **Pemicu.** Dokter menginstruksikan operasi.
4. **Prasyarat.**
   - Tindakan operasi sudah dipesan sebagai tindakan pasien.
   - Persetujuan tindakan dan anestesi ada.
   - Pasien punya episode aktif.
5. **Langkah utama.**
   1. Perawat membuka Pemesanan Ruangan Bedah, lalu memilih tab Bedah Operasi atau Bedah Obgyn.
   2. Perawat mengisi pemesanan; kasus OK terbentuk berstatus Diminta.
   3. OK menjadwalkan kasus; status di bangsal menjadi Terjadwal.
   4. Perawat bangsal mengisi Catatan Pra-Operasi dan mengonfirmasi sebagai pengirim.
   5. Perawat OK mengonfirmasi butir pra-operasi sebagai penerima; kasus menjadi Siap.
   6. OK menjalankan checklist WHO, melaksanakan operasi, lalu memantau pemulihan.
   7. OK mengirim serah terima ke bangsal: ringkasan kondisi dan instruksi pasca bedah.
   8. Perawat bangsal menerima serah terima.
   9. Kasus selesai; biaya operasi masuk invoice rawat inap.
6. **Aturan bisnis.**
   - Pesanan bukan jadwal: status Diminta belum menjamin ruang.
   - Biaya ditagih saat `Completed`.
   - Pengirim dan penerima pra-operasi adalah dua akun berbeda.
7. **Perubahan status.** Lihat tabel.
8. **Jalur tidak normal.**
   - Operasi ditunda → status Ditunda dan bangsal melihat alasannya.
   - Operasi dibatalkan → tidak ada biaya.
   - Serah terima ditolak bangsal → OK wajib melengkapi lalu mengirim ulang.
   - Operasi darurat → jalur bypass checklist OK yang sudah ada, dengan alasan dan penanggung jawab.
9. **Hasil akhir.** Pasien kembali ke bangsal dengan instruksi pasca bedah tercatat; biaya operasi ada di invoice.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Pesan dari bangsal | `Requested` (Diminta) | Perawat/dokter bangsal | Tindakan operasi sudah dipesan |
| `Requested` | Jadwalkan | `Scheduled` (Terjadwal) | Petugas OK | Ruang dan tim tersedia |
| `Scheduled` | Kesiapan lengkap | `Ready` (Siap) | Petugas OK | Pra-operasi dan persetujuan lengkap |
| `Ready` | Mulai operasi | `InProgress` | Tim OK | Checklist sign-in |
| `InProgress` | Selesai | `Completed` | Tim OK | Laporan operasi final |
| `Requested`/`Scheduled` | Tunda | `Postponed` | Petugas OK | Alasan wajib |
| Belum `Completed` | Batalkan | `Cancelled` | Petugas OK | Alasan wajib |

| Serah terima: dari | Tindakan | Ke | Siapa yang boleh |
|---|---|---|---|
| `Draft` | Kirim | `Sent` | Perawat OK |
| `Sent` | Terima | `Accepted` | Perawat bangsal tujuan |
| `Sent` | Tolak dengan alasan | `Rejected` | Perawat bangsal tujuan |

### 7.4 `BP-RWF-04` — Pemakaian alat medis besar

1. **Tujuan.** Pemakaian alat besar tercatat dan tertagih sesuai lama pemakaian.
2. **Pelaku.** Dokter penanggung jawab menginstruksikan. Perawat memasang dan mencatat. Kasir menagih.
3. **Pemicu.** Alat mulai dipasang ke pasien.
4. **Prasyarat.** Alat terdaftar di master Alat Medis dan punya tarif untuk kelas pasien.
5. **Langkah utama.**
   1. Perawat membuka Pemakaian Alat → Order Alat Kesehatan.
   2. Perawat memilih alat dan dokter penanggung jawab, lalu mencatat waktu mulai.
   3. Saat alat dilepas, perawat mencatat waktu selesai.
   4. Server menghitung unit tertagih dan mengirim baris tagihan.
   5. Riwayat tampil di History Alat Kesehatan.
6. **Aturan bisnis.**
   - Satuan dan pembulatan unit mengikuti master alat.
   - Pemakaian yang masih berjalan saat pasien pulang fisik ditutup otomatis pada waktu pulang fisik dan ditandai untuk diperiksa perawat.
7. **Perubahan status.** Lihat tabel.
8. **Jalur tidak normal.**
   - Salah pilih alat → batalkan dengan alasan; tagihan batal.
   - Waktu salah → koreksi berversi.
9. **Hasil akhir.** Pemakaian tercatat dan baris tagihan alat ada di invoice.

**Contoh.** Ventilator bersatuan per hari dengan pembulatan ke atas, tarif kelas 2 Rp 1.200.000 per hari (contoh angka). Dipasang 1 Okt 08.00, dilepas 3 Okt 11.00 = 2 hari 3 jam → dibulatkan **3 hari** → Rp 3.600.000.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Catat mulai | Berjalan | Perawat | Episode aktif; alat bertarif |
| Berjalan | Catat selesai | Selesai | Perawat | Waktu selesai ≥ waktu mulai |
| Berjalan/Selesai | Batalkan | Dibatalkan | Perawat penanggung jawab atau kepala ruangan | Alasan wajib; invoice `OPEN` |
| Selesai | Koreksi waktu | Selesai (versi baru) | Kepala ruangan | Alasan wajib; invoice `OPEN` |

### 7.5 `BP-RWF-05` — Pesan penunjang medis dari bangsal

1. **Tujuan.** Pemeriksaan penunjang dapat dipesan dari bangsal seperti V1, dengan pemberi instruksi yang jelas.
2. **Pelaku.** Dokter memesan langsung. Perawat memesan atas instruksi dokter. Unit penunjang memproses dan menerbitkan hasil.
3. **Pemicu.** Instruksi pemeriksaan dari dokter.
4. **Prasyarat.** Episode aktif; katalog pemeriksaan tersedia.
5. **Langkah utama.**
   1. Pengguna memilih layanan: Lab, Radiologi, Gizi, Bank Darah, atau Hemodialisa.
   2. Pengguna memilih pemeriksaan dan melihat harga serta status tanggungan.
   3. Bila yang memesan perawat, perawat memilih dokter pemberi instruksi.
   4. Pesanan dikirim ke modul pemilik.
   5. Hasil tampil di bangsal setelah unit penunjang memfinalkan.
6. **Aturan bisnis.**
   - Pesanan perawat tanpa dokter pemberi instruksi ditolak.
   - Hasil hanya dibaca dari modul pemilik.
   - Tagihan muncul saat layanan diterima atau dikerjakan, bukan saat dipesan.
7. **Perubahan status.** Mengikuti modul pemilik masing-masing.
8. **Jalur tidak normal.**
   - Pesanan ganda dalam waktu dekat → modul pemilik meminta konfirmasi, seperti `confirm-duplicate` pada Bank Darah.
   - Pesanan dibatalkan sebelum diproses → tidak ada tagihan.
9. **Hasil akhir.** Pesanan dan hasil terhubung ke episode rawat inap.

### 7.6 `BP-RWF-06` — Observasi pengeluaran cairan WSD per shift

1. **Tujuan.** Jumlah cairan yang keluar dari WSD per shift tercatat dan ikut balance cairan.
2. **Pelaku.** Perawat shift.
3. **Pemicu.** Akhir shift atau saat observasi terjadwal.
4. **Prasyarat.** Pasien terpasang WSD.
5. **Langkah utama.**
   1. Perawat membuka Catatan Keperawatan → Observasi Pengeluaran Cairan WSD.
   2. Server menampilkan sisa cairan di tabung dari shift sebelumnya.
   3. Perawat mengisi jam awal, jam akhir, dan sisa cairan di tabung sekarang.
   4. Server menghitung jumlah bertambah dan mencatatnya sebagai output "Drain/WSD" di Pengawasan Harian.
6. **Aturan bisnis.**
   - Jumlah bertambah = sisa sekarang − sisa shift lalu.
   - Bila tabung dikosongkan atau diganti, perawat mencatat volume yang dibuang. Perhitungannya menjadi (volume dibuang + sisa sekarang) − sisa shift lalu.
   - Hasil negatif ditolak dengan pesan yang jelas.
7. **Perubahan status.** Mengikuti pola koreksi dan pembatalan Pengawasan Harian yang sudah ada.
8. **Jalur tidak normal.** Salah ketik → koreksi berversi dengan alasan.
9. **Hasil akhir.** Output WSD masuk balance cairan harian.

**Contoh.** Sisa shift lalu 200 ml. Pukul 14.00 tabung dikosongkan 300 ml. Sisa sekarang 150 ml. Jumlah bertambah = (300 + 150) − 200 = **250 ml**.

---

## 8. Susunan menu target

### 8.1 Delapan menu keperawatan

| Menu (urutan V1) | Sub-menu target | Sumber data Final |
|---|---|---|
| Pengkajian Pasien | Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Pengawasan Harian, Evaluasi Awal, Perencanaan Pulang | Sudah ada; tidak berubah |
| Asuhan Keperawatan | Vital Sign, SOAP, Catatan Terintegrasi, Tindakan Harian, Obat & Alkes, Catatan Keperawatan | Lihat 8.2 dan 8.3 |
| Tindakan | Order Tindakan, History Tindakan | Sudah ada; katalog diperbaiki (`FR-RWF-070`) |
| Penunjang Medis | Radiologi, Laboratorium, Rehab Medik, Konsultasi Gizi, Hemodialisa, Bank Darah | EPIC-RWF-04 |
| Pemakaian Alat | Order Alat Kesehatan, History Alat Kesehatan | EPIC-RWF-07 |
| Transfer Pasien | Form Transfer, History Transfer | Sudah ada; serah terima klinis `P2` |
| Pemesanan Ruangan Bedah | Bedah Operasi, Bedah Obgyn | EPIC-RWF-05 |
| Tagihan Pasien | Rincian per kelompok | EPIC-RWF-03 |

"Rencana Asuhan" yang kini ada di Asuhan Keperawatan Final tetap dipertahankan. Letaknya diputuskan di `OD-RWF-03`.

### 8.2 Catatan Keperawatan: enam sub-menu V1

| Sub-menu | Isi V1 | Data yang dipakai di Final |
|---|---|---|
| Spooling Cairan | Intake, output, cairan masuk/keluar/sisa, urin, keterangan, cetak laporan | Pengawasan Harian — cairan (`fluid-balance-entries`) |
| Observasi Pengeluaran Cairan WSD | Jam awal/akhir, sisa di tabung, sisa shift lalu, bertambah | Perluasan Pengawasan Harian (`BP-RWF-06`) |
| Sliding Scale | GDS, insulin, insulin drip, catatan | Order dan pelaksanaan sliding scale Farmasi; GDS dari Pengawasan Harian (`RWI-DEC-147`, `RWI-DEC-148`) |
| Daftar Pemberian Obat | Pemberian, Riwayat, Efek Samping, Rekonsiliasi | MAR Farmasi (`RWI-DEC-117`), ADR (`PatientAllergy`), rekonsiliasi Farmasi (`RWI-DEC-132`) |
| Catatan Pra-Operasi | Tanda vital, nyeri, checklist persiapan pengirim/penerima, informasi operasi, kondisi, penandaan area | EPIC-RWF-05, `FR-RWF-045` |
| Diet Medis | Tanggal, perawat, diet, status diet, diagnosa, keterangan, riwayat | Diet pasien modul Gizi (`nutrition-management/diets`) |

### 8.3 Obat & Alkes

Susunan V1: **Resep, Resep Harian, Alat Kesehatan, Summary**.

Final sekarang punya delapan sub-tab. MAR, Sliding Scale, dan Obat Bawaan dipindahkan ke Catatan Keperawatan sesuai 8.2, bila `OD-RWF-03` disetujui. Riwayat BMHP digabung ke Summary.

---

## 9. Kontrak API

Endpoint dikelompokkan per nilai `[Tags(...)]` di controller, apa adanya. Endpoint yang belum ada diberi label **Rencana (belum tersedia)**. Bentuk respons sukses selalu `ApiResponse<T>`.

### 9.1 Inpatient Discharge Clearance

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/{episodeId}/discharge-clearance/webhook` | Menerima sinyal izin pulang dari kasir. **Saat ini tanpa login — wajib diganti atau dihapus (`FR-RWF-001`)** | Saat ini `[AllowAnonymous]`; target: permission internal | `ClearanceSignalWebhookDto` | `object` |
| `POST` | `/{episodeId}/supervisor-override` | Membuka pulang darurat | `InpatientDischargeClearance : SupervisorOverride` | Alasan + PIN | `object` |
| `POST` | `/{episodeId}/confirm-physical-discharge` | Mengonfirmasi pasien pulang fisik dan melepas bed | `InpatientDischargeClearance : ConfirmPhysicalDischarge` | Waktu pulang fisik | `object` |

Kode status:

| Kode | Arti bagi pengguna |
|---|---|
| `400` | Isian tidak lengkap atau formatnya salah |
| `401` | Belum login. Webhook saat ini tidak pernah mengembalikan kode ini karena tanpa login — itulah yang harus diperbaiki |
| `403` | Akun tidak punya hak untuk tindakan ini |
| `404` | Episode rawat inap tidak ditemukan |
| `409` | Data sudah diubah pengguna lain; muat ulang lalu coba lagi |
| `422` | Ditolak aturan bisnis. Contoh: "Pelepasan fisik pasien ditolak: Tagihan kasir belum disetujui" |

### 9.2 Inpatient Billing Operational

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/{episodeId}/billing-status` | Status tagihan dan kendala tanpa rupiah | `InpatientBillingOperational : Read` | - | Status operasional |
| `GET` | `/{episodeId}/billing-details` | Rincian dengan rupiah | `InpatientBillingOperational : ViewBillingDetails` | - | Rincian finansial |

### 9.3 BillingInpatientIntegration

Base URL: `api/v1/health-services/billing-management/billing`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/invoices/occupancy-charges` | Menghitung tarif kamar tanpa menyimpan. **Usulan dipensiunkan (`FR-RWF-013`)** | `BillingInpatient : Create` | `OccupancyChargeRequest` | `OccupancyChargeResponse` |
| `GET` | `/invoices/encounter/{encounterId}/inpatient-summary` | Ringkasan tagihan rawat inap. **Hak lihat rupiah wajib diganti ke permission (`FR-RWF-024`)** | `BillingInpatient : Read` | `includeFinancial` | `InpatientBillingSummaryResponse` |
| `POST` | `/inpatient-clearance/reevaluate` | Kasir menilai ulang izin pulang | `BillingInpatient : Clearance` | Kunjungan | `InpatientClearanceHandoffResponse` |
| `GET` | `/inpatient-clearance/encounter/{encounterId}/latest` | Status izin pulang terakhir; usulan menjadi sumber tunggal yang dibaca Rawat Inap | `BillingInpatient : ReadLatest` | - | Status izin pulang |

### 9.4 Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/from-source` | Biaya dari modul lain dengan harga dari pemanggil. Domain klinis rawat jalan ditolak | `BillingInvoice : Create` | `UpsertChargeRequest` | `InvoiceDetailResponse` |
| `POST` | `/catalog-charges` | Kasir menambah biaya; harga dari master tarif | `BillingInvoice : Create` | `TariffId` + jumlah | `InvoiceDetailResponse` |
| — | Pembukaan invoice `RANAP` otomatis saat `Admitted` | **Rencana (belum tersedia)** — proses internal Billing, tanpa endpoint publik baru | Sistem | Event admisi | Invoice `OPEN` |

### 9.5 Health Services / Billing Management / Patient Billing Summary

Base URL: `api/v1/health-services/billing-management/patient-billing-summaries`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/episodes/{episodeId}` | Ringkasan tagihan untuk bangsal | `PatientBillingSummary : Read` | - | `PatientBillingSummaryResponse` |
| `GET` | `/episodes/{episodeId}/breakdown` | **Rencana (belum tersedia)** — rincian per kelompok seperti V1 (`FR-RWF-020`); rupiah hanya untuk yang berhak | `PatientBillingSummary : Read` | - | Rincian per kelompok |

### 9.6 Health Services / Operating Room Management / Cases

Base URL: `api/v1/health-services/operating-room-management/cases`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar kasus; bangsal menyaring dengan `EncounterId` | `OperatingRoomCase : Read` | `EncounterId`, `Status` | Halaman kasus |
| `GET` | `/{id}` | Detail kasus | `OperatingRoomCase : Read` | - | Detail kasus |
| `POST` | `/` | Membuat kasus dari bangsal. **Perlu tambahan jenis anestesi dan penanda Obgyn (`FR-RWF-042`, `FR-RWF-043`)** | `OperatingRoomCase : Create` | `CreateOprCaseRequest` | Detail kasus |
| `PATCH` | `/{id}/cancel` | Membatalkan kasus | `OperatingRoomCase : Cancel` | Alasan | Status kasus |

### 9.7 Health Services / Operating Room Management / Execution

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/execution`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/handovers` | Serah terima pasca operasi | `OperatingRoomHandover : Read` | - | Serah terima |
| `POST` | `/handovers` | OK mengirim serah terima ke bangsal | `OperatingRoomHandover : Update` | Unit tujuan + ringkasan kondisi | Serah terima |
| `PATCH` | `/handovers/{handoverId}/accept` | Bangsal menerima atau menolak | `OperatingRoomHandover : Update` | `Accept`, alasan penolakan | Status kasus |

### 9.8 Health Services / Operating Room Management / Preparation

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/preparation`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `PUT` | `/ward-pre-op` | **Rencana (belum tersedia)** — Catatan Pra-Operasi bangsal dengan konfirmasi pengirim/penerima (`FR-RWF-045`). Bentuk akhirnya menunggu `OD-RWF-05` | Diputuskan saat desain | Checklist + tanda vital + penandaan | Status kesiapan |

### 9.9 Gizi

**Health Services / Nutrition Management / Nutrition Order** — base URL `api/v1/health-services/nutrition-management/orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar pesanan gizi | `NutritionOrder : Read` | Filter | Daftar |
| `POST` | `/` | Pesan konsultasi gizi dari bangsal | `NutritionOrder : Create` | `CreateGzOrderRequest` (`EncounterId`) | Pesanan |
| `POST` | `/{id}/cancel` | Membatalkan pesanan | `NutritionOrder : Cancel` | Alasan | Pesanan |

**Health Services / Nutrition Management / Patient Diet** — base URL `api/v1/health-services/nutrition-management/diets`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/history/{encounterId}` | Riwayat diet pasien (Diet Medis) | `NutritionPatientDiet : Read` | - | Riwayat |
| `POST` | `/` | Menetapkan diet | `NutritionPatientDiet : Update` | Diet pasien | Diet |
| `POST` | `/{dietId}/stop` | Menghentikan diet | `NutritionPatientDiet : Update` | Alasan | Diet |

Catatan: apakah perawat boleh menetapkan diet atau hanya dokter/ahli gizi masuk `OD-RWF-07`.

### 9.10 Health Services / Blood Bank Management / Blood Order

Base URL: `api/v1/health-services/blood-bank-management/blood-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar pesanan darah | `BloodOrder : Read` | Filter | Daftar |
| `POST` | `/` | Pesan darah dari bangsal | `BloodOrder : Create` | `CreateBloodOrderRequest` (`EncounterId`) | Pesanan |
| `POST` | `/confirm-duplicate` | Konfirmasi pesanan yang mirip pesanan sebelumnya | `BloodOrder : Create` | Pesanan | Pesanan |
| `POST` | `/{id}/cancel` | Membatalkan pesanan | `BloodOrder : Cancel` | Alasan | Pesanan |

### 9.11 Laboratorium dan Radiologi

| Tag | Base URL | Method | Kegunaan | Hak akses |
|---|---|---|---|---|
| `Health Services / Laboratory Management / Lab Order` | `api/v1/health-services/laboratory-management/lab-orders` | `POST /` | Pesan lab; wajib membawa dokter pemberi instruksi | `LabOrder : Create` |
| `Health Services / Radiology Management / Rad Order` | `api/v1/health-services/radiology-management/rad-orders` | `POST /` | Pesan radiologi; wajib membawa dokter pemberi instruksi | `RadOrder : Create` |

### 9.12 Health Services / Clinical Management / Fluid Balance

Base URL: `api/v1/health-services/clinical-management/fluid-balance-entries`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/episodes/{episodeId}` | Entri cairan (Spooling Cairan) | `FluidBalance : Read` | Tanggal | Daftar |
| `GET` | `/episodes/{episodeId}/totals` | Total balance cairan | `FluidBalance : Read` | Tanggal | Total |
| `POST` | `/` | Mencatat entri cairan | `FluidBalance : Create` | Entri | Entri |
| `PUT` | `/{id}/correct` | Koreksi berversi | `FluidBalance : Update` | Alasan | Entri |
| `POST` | `/wsd-readings` | **Rencana (belum tersedia)** — pembacaan tabung WSD per shift; server menghitung jumlah bertambah (`FR-RWF-054`) | `FluidBalance : Create` | Jam, sisa tabung, volume dibuang | Entri output |

### 9.13 Health Services / Clinical Management / Patient Allergy

Base URL: `api/v1/health-services/clinical-management/patient-allergies`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/from-medication-administration` | Mencatat dugaan efek samping obat dari dosis MAR | `PatientAllergy : Create` | `CreateSuspectedAdverseDrugReactionRequest` | Reaksi obat |

### 9.14 Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/master-options` | Katalog tindakan. **Saat ini memakai filter rawat jalan (`FR-RWF-070`)** | `PatientProcedure : Read` | `search` | Opsi tindakan |
| `POST` | `/inpatient-orders` | Pesan tindakan rawat inap | `PatientProcedure : Create` | Pesanan | Tindakan |

### 9.15 Pemakaian Alat — **Rencana (belum tersedia)**

Nama tag dan pemilik modul ditetapkan setelah `OD-RWF-02` dan pendaftaran di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.

| Method | Path usulan | Kegunaan |
|---|---|---|
| `GET` | `api/v1/health-services/master-data/medical-equipments` | Daftar alat medis besar beserta satuan tagih |
| `GET` | `.../inpatient-management/episodes/{episodeId}/equipment-usages` | History Alat Kesehatan |
| `POST` | `.../inpatient-management/episodes/{episodeId}/equipment-usages` | Mulai pemakaian |
| `PATCH` | `.../equipment-usages/{id}/finish` | Selesai pemakaian |
| `PATCH` | `.../equipment-usages/{id}/cancel` | Batal pemakaian dengan alasan |

---

## 10. Data dan kepemilikan

| Data | Pemilik | Status | Catatan |
|---|---|---|---|
| Invoice, baris tagihan, tarif kamar, biaya administrasi | BillingManagement | Ada | Hanya perlu disambungkan untuk `RANAP` |
| Status izin pulang | Usulan BillingManagement | `CONFLICT` | Diputuskan `OD-RWF-01` |
| Kasus operasi, checklist, serah terima | OperatingRoomManagement | Ada | Tambah jenis anestesi, penanda Obgyn, pra-operasi bangsal |
| Pesanan gizi, diet pasien | NutritionManagement | Ada | Rawat Inap tidak membuat tabel |
| Pesanan darah | BloodBankManagement | Ada | Rawat Inap tidak membuat tabel |
| Cairan, WSD | ClinicalManagement | Ada sebagian | WSD per shift perlu perluasan |
| MAR, sliding scale, rekonsiliasi | PharmacyManagement | Ada | `RWI-DEC-117`, `132`, `147` |
| Master alat medis dan pemakaiannya | **Belum ditetapkan** | `MISSING` | Wajib terdaftar di registry sebelum model pertama dibuat (`QBE-MOD-002`, `QBE-MOD-003`) |

Setiap perubahan tabel, termasuk migration, butuh wewenang terpisah sesuai `AGENTS.md`.

---

## 11. Keputusan terbuka

Keputusan berikut didaftarkan lewat `grill-me` sebagai `RWI-OQ-098` dan seterusnya. Nomor `OD-RWF-##` hanya alias sementara.

### `OD-RWF-01` — Arah integrasi tagihan dan sumber izin pulang

| Pilihan | Isi | Konsekuensi |
|---|---|---|
| **A (rekomendasi)** | Billing menarik langsung. Jembatan folio→invoice diperluas untuk `RANAP`, invoice dibuka saat admisi, tarif kamar dan biaya admin dihitung Billing dari data penempatan bed. Status izin pulang milik Billing dan dibaca Rawat Inap. Outbox Rawat Inap menjadi jejak audit saja | Memakai perhitungan Billing yang sudah ada. Webhook tanpa login bisa dihapus. Butuh persetujuan pemilik Billing |
| B | Outbox Rawat Inap diteruskan ke penerima nyata di Billing | Dua jalur data untuk fakta yang sama. Perlu kontrak event baru |

### `OD-RWF-02` — Pemakaian Alat (membuka ulang `RWI-DEC-089`)

| Pilihan | Isi | Konsekuensi |
|---|---|---|
| **A (rekomendasi)** | Master Alat Medis sederhana (jenis alat, bukan unit aset) + tarif per kelas dan penjamin + pemakaian mulai/selesai + tagihan lewat jalur yang sama | Tidak menunggu modul aset. Butuh entri registry, model baru, dan migration |
| B | Alat dimodelkan sebagai tindakan bertarif "Pemakaian Alat" | Paling cepat, tetapi lama pemakaian per jam/hari tidak tercatat rapi |
| C | Tetap ditunda sampai modul aset ada | Menu tetap *placeholder*; tidak sama dengan V1 |

### `OD-RWF-03` — Susunan Asuhan Keperawatan

| Pilihan | Isi |
|---|---|
| **A (rekomendasi)** | Catatan Keperawatan memakai enam sub-menu V1. MAR, Sliding Scale, dan Obat Bawaan dipindah dari Obat & Alkes. Obat & Alkes kembali empat sub-tab V1. Catatan naratif tetap di CPPT/SOAP. Rencana Asuhan tetap sebagai sub-tab ketujuh |
| B | Susunan Final sekarang dipertahankan; enam sub-menu V1 ditambahkan sebagai tautan |

### `OD-RWF-04` — Rupiah di Tagihan Pasien bangsal

| Pilihan | Isi |
|---|---|
| **A (rekomendasi)** | Susunan rincian seperti V1 untuk semua; angka rupiah hanya untuk pemegang `ViewBillingDetails`. Konsisten dengan `RWI-DEC-160` |
| B | Seperti V1: perawat melihat rincian beserta rupiah. Membuka ulang `RWI-DEC-160` |
| C | Seperti sekarang: total rupiah tanpa rincian |

### `OD-RWF-05` — Pemilik Catatan Pra-Operasi

| Pilihan | Isi |
|---|---|
| **A (rekomendasi)** | Fase checklist baru di OperatingRoomManagement, diisi perawat bangsal sebagai pengirim dan dikonfirmasi perawat OK sebagai penerima |
| B | Dokumen ClinicalManagement milik bangsal; OK hanya membaca |

Sub-keputusan: penandaan area operasi berupa gambar tubuh beranotasi seperti V1, atau unggah foto.

### `OD-RWF-06` — Bedah Obgyn (SC/Caesar)

| Pilihan | Isi |
|---|---|
| **A (rekomendasi)** | Jenis layanan bedah "Obstetri" pada kasus OR, dengan lokasi ruang bersalin bila dipakai |
| B | Alur terpisah di luar modul OR |

### `OD-RWF-07` — Siapa boleh menulis Diet Medis

Dokter, ahli gizi, atau perawat atas instruksi. Harus sejalan dengan kewenangan di modul Gizi.

### `OD-RWF-08` — Serah terima klinis saat transfer (`P2`)

Membuka ulang `RWI-DEC-113` atau tetap ditunda.

### `OD-RWF-09` — Resume ODC (`P2`)

Pemilik dokumen (`dokter-rawat-inap` atau `episode-rawat-inap`) dan isinya.

---

## 12. Larangan

| No | Larangan | Alasan |
|---:|---|---|
| 1 | Endpoint yang mengubah data tanpa login | Celah keamanan nyata pada webhook izin pulang |
| 2 | Angka tarif tertanam di kode | Menimbulkan tagihan karangan |
| 3 | Menandai pesan terkirim padahal tidak ada penerima | Menyesatkan status task dan audit |
| 4 | Menentukan hak akses dari nama peran | Melanggar aturan hak akses Quilvian |
| 5 | Menagih saat dipesan | Pasien bisa tertagih layanan yang tidak terjadi |
| 6 | Menghapus tagihan milik modul lain saat mengoreksi data sendiri | Bug V1 Pemakaian Alat |
| 7 | Membuat tabel salinan untuk MAR, cairan, gizi, darah, atau operasi di Rawat Inap | Melanggar `RWI-DEC-081` dan prinsip satu sumber |
| 8 | Data contoh atau pesan berhasil palsu | `RWI-DEC-108` |
| 9 | Menyalin nama, nomor RM, atau data pasien asli dari V1 atau capture ke dokumen dan test | Privasi |

---

## 13. Kelemahan V1 yang tidak boleh ditiru

### 13.1 Pemesanan Ruangan Bedah V1

Tagihan tindakan operasi dibuat saat pemesanan, sebelum operasi terjadi.
Bukti: `QuilvianSystemBackendDev/Areas/ManajemenKesehatan/OperasiOK/Controllers/RuangBedahBookingController.cs:283-423@4be1499c`.

### 13.2 Pemakaian Alat V1

| Kelemahan | Bukti |
|---|---|
| Harga baris tagihan dimatikan, sehingga tagihan tanpa harga | `QuilvianSystemBackendDev/Areas/ManajemenKesehatan/Alkes/Controllers/AlatPemakaianController.cs:292-294@4be1499c` |
| Mengubah pemakaian menghapus semua tagihan "Alkes" kunjungan, termasuk alkes farmasi | `AlatPemakaianController.cs:417-424@4be1499c` |
| Dokter wajib dipilih di form, tetapi tidak dikirim ke server | `QuilvianSystemFrontendDev/src/components/view/Rawat-Inap/perawat/perawatan-pasien/Pemakaian-Alat/order-peralatan-kesehatan.jsx:349-388@86408f245` |
| Jenis tagihan "Alkes" dipakai bersama alkes kecil farmasi | `AlatPemakaianController.cs:296@4be1499c` |

---

## 14. Gelombang pengiriman

Nama gelombang memakai awalan `RWF-W` agar tidak bertabrakan dengan `MVP-0..4`, `DOK-MVP-*`, dan `Gelombang 1A`. Nomor task diberikan `plan-module-delivery` dari deret bebas yang berlaku.

| Gelombang | Isi | Prioritas |
|---|---|---|
| `RWF-W0` — Keamanan dan kebenaran data | Webhook wajib login atau dihapus; pensiun hitungan tarif kedua dan tarif karangan; katalog tindakan rawat inap; hak lihat rupiah berbasis permission; membuka tombol pesan Lab/Rad perawat | `P0`/`P1` |
| `RWF-W1` — Tagihan inti | Invoice `RANAP` otomatis; jembatan klinis untuk `RANAP`; satu sumber izin pulang dan sinyalnya; outbox jujur; aturan biaya admin | `P0` |
| `RWF-W2` — Menu yang modulnya sudah ada | Gizi, Bank Darah, Diet Medis, Efek Samping, Catatan Keperawatan susunan V1, WSD per shift, Tagihan Pasien rincian | `P1` |
| `RWF-W3` — Pasien operasi | Pemesanan dari bangsal termasuk Obgyn, status operasi di bangsal, pra-operasi, terima serah terima, biaya operasi | `P1` |
| `RWF-W4` — Pemakaian Alat | Master alat, tarif, pemakaian, tagihan | `P1` |
| `RWF-W5` — Menyusul | Serah terima klinis transfer, Resume ODC | `P2` |
| `RWF-W6` — UAT bersama kasir | `UAT-RWF-01` s.d. `UAT-RWF-10` | Wajib |

Urutan ketergantungan:

```text
RWF-W0 (keamanan & kebenaran data)
└──► RWF-W1 (tagihan inti) ── butuh OD-RWF-01
     ├──► RWF-W2 (menu modul yang sudah ada) ── butuh OD-RWF-03, OD-RWF-04, OD-RWF-07
     ├──► RWF-W3 (pasien operasi) ── butuh OD-RWF-05, OD-RWF-06; biaya operasi butuh kontrak Billing
     └──► RWF-W4 (pemakaian alat) ── butuh OD-RWF-02 + entri registry + wewenang migration
          └──► RWF-W6 (UAT bersama kasir)
RWF-W5 (P2) ── butuh OD-RWF-08, OD-RWF-09; boleh paralel setelah RWF-W1
```

Bagian `RWF-W2` yang tidak menyentuh tagihan, seperti Diet Medis, Efek Samping, dan WSD, boleh dikerjakan paralel dengan `RWF-W1`.

---

## 15. Skenario UAT

Seluruh data di bawah adalah data samaran.

| ID | Skenario | Hasil yang diharapkan |
|---|---|---|
| `UAT-RWF-01` | "Tn. Contoh A", kelas 2, masuk 1 Okt 10.00, pulang fisik 4 Okt 09.00. Menerima: darah lengkap, pasang infus, ceftriaxone 3 dosis, ventilator 2 hari, satu operasi | Invoice `RANAP` berisi kamar, tindakan, lab, obat, alat, operasi, dan biaya admin tanpa input manual kasir |
| `UAT-RWF-02` | Panggil endpoint perubahan izin pulang tanpa token | Ditolak `401`, status episode tidak berubah |
| `UAT-RWF-03` | Kasir menyetujui izin pulang, lalu mencabutnya karena ada resep susulan | Tombol pulang aktif lalu terkunci lagi; banner merah tampil |
| `UAT-RWF-04` | Perawat memesan lab tanpa memilih dokter pemberi instruksi | Ditolak dengan pesan yang jelas |
| `UAT-RWF-05` | Pesan Bedah Obgyn → dijadwalkan → pra-operasi lengkap → operasi selesai → serah terima diterima bangsal | Status terlihat di bangsal di setiap langkah; biaya operasi muncul setelah `Completed` |
| `UAT-RWF-06` | Ventilator 1 Okt 08.00 s.d. 3 Okt 11.00, satuan per hari, pembulatan ke atas | Tertagih 3 hari |
| `UAT-RWF-07` | Dokter memesan konsultasi gizi dari bangsal | Pesanan tampil di layar Gizi |
| `UAT-RWF-08` | Perawat memesan 2 kantong PRC | Pesanan tampil di layar Bank Darah |
| `UAT-RWF-09` | WSD: sisa lalu 200 ml, dibuang 300 ml, sisa sekarang 150 ml | Tercatat output 250 ml dan masuk balance cairan |
| `UAT-RWF-10` | Perawat biasa membuka Tagihan Pasien; lalu petugas ber-permission membuka layar yang sama | Perawat melihat rincian tanpa rupiah; petugas berizin melihat rupiah (bila `OD-RWF-04` = A) |

---

## 16. Definition of Done

1. Seluruh kemampuan `P0` dan `P1` berstatus selesai di roadmap, dengan laporan task tracked.
2. Tidak ada menu keperawatan *placeholder* kecuali Rehab Medik.
3. `UAT-RWF-01` s.d. `UAT-RWF-10` lulus **di aplikasi berjalan** dan disaksikan pemilik Rawat Inap dan pemilik Billing.
4. Tidak ada endpoint pengubah data tanpa login di modul Rawat Inap dan Billing rawat inap.
5. Tidak ada angka tarif atau aturan jam tertanam di kode.
6. Status izin pulang hanya punya satu sumber.
7. Setiap baris tagihan bisa ditelusuri ke layanan asalnya.
8. Butir verifikasi yang tidak dijalankan ditulis `NOT RUN` apa adanya di laporan task.
9. Migration diterapkan hanya dengan wewenang terpisah dan tercatat.

---

## 17. Risiko

| ID | Risiko | Dampak | Mitigasi |
|---|---|---|---|
| `RSK-RWF-01` | Pemilik Billing belum menyetujui perluasan untuk `RANAP` | `RWF-W1` tertahan | Daftarkan `OD-RWF-01` lebih dulu; libatkan pemilik Billing di `grill-me` |
| `RSK-RWF-02` | Penanda `IsSuperseded` pada penempatan bed dipakai untuk pindah kamar dan koreksi salah catat. Billing menghitung semua penempatan yang tidak dihapus | Tagihan kamar bisa dobel atau kurang setelah koreksi | Uji khusus: pindah kamar vs koreksi salah catat sebelum `RWF-W1` dinyatakan selesai |
| `RSK-RWF-03` | Master tarif kamar, alat, dan kebijakan tarif kamar belum terisi di lingkungan target (`RWI-UI-GAP-007`) | UAT gagal walau kode benar | Persiapan data master sebelum UAT; dilarang menanam data tiruan |
| `RSK-RWF-04` | Biaya admin tidak terhitung untuk invoice yang hanya berisi tarif kamar | Tagihan kurang | Konfirmasi aturan dengan pemilik Billing (`FR-RWF-015`) |
| `RSK-RWF-05` | Data episode lama tidak punya invoice `RANAP` | Pasien yang sedang dirawat saat rilis tidak punya tagihan | Langkah migrasi data terkontrol, dengan wewenang terpisah |
| `RSK-RWF-06` | Penyesuaian modul Kamar Operasi (jenis anestesi, Obgyn, pra-operasi) menyentuh modul milik tim lain | Jadwal bergeser | Koordinasi dengan pemilik modul OR |

---

## 18. Riwayat dan persetujuan

| Versi | Tanggal | Perubahan | Oleh |
|---|---|---|---|
| `0.1` | 1 Oktober 2026 | Draf pertama dari audit paritas 30 September 2026 | Disusun agent atas permintaan pemilik |

| Persetujuan | Nama | Tanggal | Cakupan |
|---|---|---|---|
| Product/Domain Owner Rawat Inap | — | — | Belum disetujui |
| Pemilik modul Billing | — | — | Belum disetujui |
| Pemilik modul Kamar Operasi | — | — | Belum disetujui |
