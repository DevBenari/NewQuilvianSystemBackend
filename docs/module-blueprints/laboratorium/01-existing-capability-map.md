# Laboratorium — Peta Kemampuan Existing

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Revision | `1` |
| Status | `draft` |
| Jenis audit | Audit penuh (bukan *impact scan*) |
| Sifat audit | **Read-only.** Tidak ada satu baris source aplikasi yang diubah |
| Product/domain owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| Backend SHA | `9124900` |
| Frontend SHA | `688daff90` |
| Masukan | `00-interview-decisions.md` revision 7, keputusan `LAB-DEC-001` sampai `LAB-DEC-014` |
| Tanggal audit | 2026-09-01 |

> **Cara membaca dokumen ini.**
> Dokumen ini menjawab pertanyaan "apa yang sudah ada di sistem", bukan "aturan bisnisnya
> bagaimana". Setiap baris membawa bukti berupa lokasi berkas dan nama simbol pada commit
> tertentu, supaya siapa pun bisa memeriksa ulang. Dokumen ini **tidak** merancang arsitektur
> dan **tidak** memberi izin menulis kode.

---

## Peringatan: SHA frontend sudah berubah

Decision log revision 7 mencatat frontend SHA `c79bb6ee4`. Saat audit ini dijalankan, frontend
sudah berada di `688daff90`.

| Repository | SHA di decision log | SHA saat audit | Keterangan |
|---|---|---|---|
| `NewQuilvianSystemBackend` | `9124900` | `9124900` | Sama, tidak ada pergeseran |
| `QuilvianSystemFrontendDev` | `c79bb6ee4` | `688daff90` | **Berubah** |

Audit ini memakai SHA terkini. Temuan pokok frontend tidak berubah: pada kedua SHA, modul
Laboratorium sama-sama **tidak ada sama sekali**. Jadi pergeseran ini tidak membatalkan
keputusan mana pun, tetapi angka SHA di decision log perlu diperbarui saat revisi berikutnya.

---

## Batas Audit

### Yang diaudit

Kemampuan yang dibutuhkan Rilis 1 menurut `LAB-DEC-001` sampai `LAB-DEC-014`, dikelompokkan
menjadi sepuluh klaster:

| Klaster | Yang dicari |
|---|---|
| Order/Result | Pesanan lab, sampel, hasil pemeriksaan |
| Identity/Master Owner | Katalog pemeriksaan, batas nilai, alasan penolakan |
| Episode/Transaction Owner | Kunjungan pasien dari Rawat Jalan, Rawat Inap, dan IGD |
| Actor/Workforce | Identitas petugas yang melakukan tindakan |
| Workflow/Status | Perpindahan status, riwayat, konkurensi |
| Documentation/Record | Penyajian dan penyimpanan hasil |
| Financial | Pengiriman fakta kelayakan tagih ke Billing |
| Authorization/Audit | Permission per aksi, jejak audit |
| External Integration | Pemberitahuan kepada dokter |
| Frontend consumer | Route, menu, layar, state, API service |

### Yang tidak diaudit

Mikrobiologi, Patologi Anatomi, Bank Darah, stok reagen, dan Radiologi — seluruhnya berada di
luar scope menurut `LAB-DEC-002` dan `LAB-DEC-014`.

---

## Ringkasan Hasil

| Status | Jumlah | Arti singkat |
|---|---:|---|
| `Ready to reuse` | 11 | Sudah ada, terbukti jalan, bisa langsung dipakai |
| `Reuse with adapter` | 3 | Sudah ada, tetapi perlu penyesuaian kecil |
| `Extend` | 1 | Sudah ada, tetapi perlu tambahan kolom atau perilaku |
| `Missing` | 6 | Belum ada sama sekali |
| `Conflict` | 1 | Ada pertentangan antara kode dan keputusan terkunci |
| `Unknown` | 2 | Belum bisa dipastikan tanpa keputusan manusia |

**Kesimpulan singkat:** separuh perjalanan Laboratorium sudah dibangun dan terbukti dengan
pengujian, yaitu dari pesanan sampai sampel dinyatakan layak beserta pengiriman fakta tagihan.
Yang belum ada adalah **seluruh bagian hasil pemeriksaan**, **pemberitahuan kepada dokter**,
dan **seluruh tampilan frontend**.

---

## Tabel Kemampuan

Format bukti: `repository/path#simbol@SHA`.

### Klaster Order dan Result

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-01` | Pesanan laboratorium beserta siklus hidupnya | Laboratorium | `NewQuilvianSystemBackend/Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs#LabOrder@9124900`; `Services/LabOrderService.cs@9124900`; `Controllers/LabOrderController.cs@9124900`; migration `Migrations/20260815103436_initializeLabOrder.cs@9124900` | `Extend` | Tidak ada kolom tingkat kesegeraan. `LAB-DEC-013` mewajibkan penanda cito dan batas waktunya | Sedang. Penambahan kolom memerlukan migration baru pada tabel yang sudah berisi data |
| `CAP-02` | Siklus hidup sampel: rencana, ambil, terima, layak/tolak, ambil ulang | Laboratorium | `Models/TrxLabSpecimen.cs#TrxLabSpecimen@9124900`; `Services/LabSpecimenService.cs@9124900`; `Controllers/LabSpecimenController.cs@9124900`; migration `Migrations/20260824091610_AddLaboratorySpecimenLifecycle.cs@9124900` | `Ready to reuse` | Tidak ada | Rendah. Sudah lengkap dan teruji |
| `CAP-03` | Hasil pemeriksaan: isi nilai, verifikasi, validasi, rilis, koreksi | Laboratorium | Tidak ditemukan model, service, controller, enum, maupun migration mana pun yang menyimpan nilai hasil pada `Areas/HealthServices/LaboratoryManagement/@9124900` | `Missing` | Seluruhnya harus dibangun. Ini inti Rilis 1 menurut `LAB-DEC-001` | Tinggi. Bagian terbesar pekerjaan Rilis 1 |
| `CAP-04` | Riwayat perpindahan status yang tidak bisa diubah | Laboratorium | `Models/TrxLabTransitionHistory.cs#TrxLabTransitionHistory@9124900` — memuat `Scope`, `Action`, `FromStatus`, `ToStatus`, `ReasonCode`, `ReasonNote`, `ActorUserId`, `OccurredAt`, `CorrelationId` | `Ready to reuse` | Tidak ada. `LabTransitionScope` cukup ditambah nilai baru untuk hasil bila diperlukan | Rendah. Sudah memenuhi seluruh isian yang diminta `LAB-INH-013` |

**Penjelasan `CAP-01` untuk pembaca non-teknis.** Pesanan lab sudah bisa dibuat, ditahan,
dilanjutkan, dan dibatalkan. Yang belum ada hanyalah cara menandai sebuah pesanan sebagai
"cito" alias segera. Karena `LAB-DEC-013` mewajibkan penandaan itu, tabel pesanan perlu
ditambah kolom, dan itulah sebabnya statusnya `Extend` dan bukan `Ready to reuse`.

### Klaster Identity dan Master Owner

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-05` | Daftar alasan penolakan sampel yang terkendali | Laboratorium | `Models/MstLabRejectionReason.cs#MstLabRejectionReason@9124900` — punya `ReasonCode`, `IsInternalHospitalError`, `RequiresNote`; dibaca lewat `Controllers/LabSpecimenController.cs#GetRejectionReasons@9124900` | `Reuse with adapter` | Hanya tersedia endpoint baca. **Tidak ada** endpoint tambah, ubah, atau nonaktifkan, dan tidak ditemukan seeder yang mengisinya | Sedang. Bila tabel kosong di lingkungan baru, petugas tidak bisa menolak sampel sama sekali |
| `CAP-06` | Katalog jenis pemeriksaan laboratorium | `master-data` | `Areas/HealthServices/MasterData/Models/MstProcedure.cs#IsLaboratory@9124900`; dipakai sebagai komponen pemeriksaan di `TrxLabSpecimen.ProcedureId@9124900` | `Reuse with adapter` | Berfungsi sebagai katalog, tetapi tidak punya satuan hasil, jenis sampel, wadah, volume minimal, maupun metode. `LAB-DEC-001` memang menunda sisa katalog ke Rilis 2 | Rendah untuk Rilis 1, karena penundaannya sudah disetujui |
| `CAP-07` | Tabel batas nilai: satuan, batas normal, batas kritis, batas waktu cito | Laboratorium | Tidak ditemukan kolom maupun tabel penyimpan batas nilai di seluruh `Areas/@9124900` | `Missing` | Seluruhnya harus dibangun. Diwajibkan `LAB-DEC-006` dan `LAB-DEC-013` | Tinggi. Tanpa ini, `LAB-DEC-004` tidak bisa dijalankan — sistem tidak akan tahu sebuah angka itu kritis |

### Klaster Episode dan Identitas Pasien

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-08` | Kunjungan pasien dari Rawat Jalan, Rawat Inap, dan IGD | `registration-management` | `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs#EncounterType@9124900`; `Enums/EncounterType.cs@9124900` bernilai `Outpatient = 1`, `Emergency = 2`, `Inpatient = 3`, `MedicalCheckup = 4`, `Telemedicine = 5`. `LabOrder.EncounterId` menunjuk ke entity ini | `Ready to reuse` | Tidak ada | Rendah. `LAB-DEC-009` sudah terpenuhi di tingkat data tanpa perubahan apa pun |
| `CAP-09` | Identitas pasien dan dokter | `master-data` / `patient-management` | Diakses lewat `TrxPatientEncounter.PatientId` dan `TrxPatientEncounter.DoctorId@9124900` | `Ready to reuse` | Tidak ada. Laboratorium cukup menempel pada kunjungan | Rendah |
| `CAP-10` | Tarif pemeriksaan beserta salinannya | `master-data` / `billing-kasir` | `Services/LabSpecimenService.cs#ResolveTariffAsync@9124900`; salinan disimpan pada `TrxLabSpecimen.TariffId`, `TariffCodeSnapshot`, `UnitPriceSnapshot@9124900` | `Ready to reuse` | Tidak ada | Rendah. Pola salinan tarif sudah benar: harga saat itu ikut tersimpan sehingga tidak berubah bila tarif induk diubah kemudian |

**Contoh kenapa `CAP-08` penting.** `LAB-DEC-009` memutuskan Laboratorium melayani ketiga unit
sekaligus. Sering kali keputusan seperti ini mahal karena data kunjungan tiap unit terpisah.
Di sini ternyata tidak: satu tabel `TrxPatientEncounter` sudah menampung ketiganya lewat kolom
`EncounterType`. Jadi keputusan itu bisa dijalankan tanpa tambahan pekerjaan data.

### Klaster Financial — batas kewenangan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-11` | Pengiriman fakta kelayakan tagih dan pembatalan klinis ke Billing | `billing-kasir` (penerima), Laboratorium (pengirim) | `Areas/HealthServices/ClinicalBillingIntegration/Services/ClinicalMilestoneFactProducer.cs@9124900`; dipanggil dari `LabSpecimenService.cs#EmitChargeEligibilityAsync` dan `#EmitClinicalCancellationAsync@9124900`; jenis fakta di `Enums/ClinicalMilestoneFactEnums.cs#ClinicalMilestoneKind@9124900` bernilai `ChargeEligibility` dan `ClinicalCancellation` | `Ready to reuse` | Tidak ada | Rendah. Sudah terpasang, terhubung, dan teruji |
| `CAP-12` | Laboratorium tidak boleh punya kolom atau method finansial | Laboratorium | Diuji otomatis oleh `tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratoryAuthorityTests.cs#ModelLaboratorium_TidakMemilikiPropertiFinansialApaPun@9124900` dan `#ServiceLaboratorium_TidakMemilikiMethodKewenanganFinansial@9124900` | `Ready to reuse` | Tidak ada | Rendah. `AC-13` sudah dijaga pengujian otomatis, bukan sekadar niat |

**Penjelasan `CAP-11` dengan contoh.** Saat petugas menyatakan sampel layak periksa, sistem
otomatis mengirim satu "fakta" ke Billing yang berbunyi kira-kira: pemeriksaan ini sudah sah
untuk ditagihkan, kodenya sekian, harga saat itu sekian. Billing yang memutuskan apa yang
terjadi dengan uangnya. Laboratorium tidak pernah menyentuh angka tagihan. Mekanisme ini sudah
berjalan, jadi `AC-12` tidak perlu dibangun ulang.

### Klaster Authorization dan Audit

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-13` | Kewenangan berbeda untuk tiap tindakan lab | Platform | `Attributes/AccessPermissionAttribute.cs@9124900`; `Filters/AccessPermissionFilter.cs@9124900`; `Services/Security/AccessPermissionService.cs#HasAccessAsync@9124900`. Lab memakai `[AccessPermission("LabSpecimen","Collect")]`, `("LabSpecimen","Receive")`, `("LabSpecimen","Accept")`, dan seterusnya pada `Controllers/LabSpecimenController.cs@9124900` | `Ready to reuse` | Tidak ada | Rendah |
| `CAP-14` | Pendaftaran otomatis permission ke basis data | Platform | `Seeders/AccessMenuSeeder.cs@9124900`, dijalankan saat aplikasi mulai lewat `Program.cs:974@9124900`. Controller lab sudah membawa `[AccessController(...)]` dan `[AccessAction(...)]` sehingga ikut terdaftar sendiri | `Ready to reuse` | Tidak ada | Rendah. Permission untuk endpoint hasil yang baru akan terdaftar otomatis asalkan atributnya dipasang |
| `CAP-15` | Identitas petugas pelaku tindakan | Platform | `Services/LabSpecimenService.cs#GetCurrentUserId@9124900` lewat `IHttpContextAccessor`; tersimpan pada `TrxLabSpecimen.CollectedByUserId`, `ReceivedByUserId`, `DecidedByUserId@9124900` | `Ready to reuse` | Tidak ada | Rendah |
| `CAP-16` | Penegakan prinsip empat mata: pengisi hasil tidak boleh memvalidasi hasil yang sama | Laboratorium | Tidak ditemukan. Sistem permission bekerja per aksi, **bukan** per orang pada satu baris data. `AccessPermissionService.HasAccessAsync@9124900` hanya menjawab "boleh atau tidak", tidak pernah membandingkan pelaku sebelumnya | `Missing` | Harus dibangun sebagai aturan di dalam service hasil, bukan lewat permission | **Tinggi.** Ini invariant keselamatan `LAB-DEC-003`. Bila keliru dianggap bisa ditutup permission, `AC-01` tidak akan terpenuhi |
| `CAP-17` | Perlindungan dua petugas bertindak bersamaan | Laboratorium | `LabOrder.Version` dan `TrxLabSpecimen.Version@9124900`; diuji oleh `LaboratorySpecimenLifecycleTests.cs#DuaPetugasMenetapkanLayakBersamaan_SalahSatuDitolak@9124900` | `Ready to reuse` | Tidak ada. Pola yang sama tinggal diterapkan pada tabel hasil | Rendah |

**Penjelasan `CAP-16`, temuan paling penting dalam audit ini.** Sistem izin yang ada menjawab
pertanyaan "apakah orang ini boleh memvalidasi hasil?". Ia tidak bisa menjawab "apakah orang
ini yang tadi mengetik hasilnya?". Padahal `LAB-DEC-003` justru menuntut pertanyaan kedua.
Artinya prinsip empat mata harus ditulis sebagai aturan di dalam layanan hasil, dengan
membandingkan pengisi dan validator pada baris hasil yang sama. Ini juga berarti jalur
pengecualian beserta penandanya harus disimpan di tabel hasil, bukan di tabel izin.

### Klaster Notifikasi dan Integrasi Eksternal

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-18` | Pemberitahuan tersimpan untuk dokter, dengan status sudah dibaca | Platform | Tidak ditemukan layanan notifikasi umum, tabel notifikasi, surel, SMS, maupun WhatsApp di seluruh `NewQuilvianSystemBackend@9124900` | `Missing` | Seluruhnya harus dibangun. Diwajibkan `LAB-DEC-012` | **Tinggi.** Ini kemampuan milik platform, bukan khusus Laboratorium. Membangunnya di dalam modul Laboratorium berisiko menjadi duplikasi ketika modul lain membutuhkan hal yang sama |
| `CAP-19` | Pengiriman seketika ke pengguna yang sedang online | Platform | `Hubs/QueueHub.cs@9124900` dipetakan ke `/hubs/queues` oleh `Program.cs:1091@9124900`; pengelompokan peserta per *nurse station cluster* lewat `QueueHub#JoinNurseStationCluster@9124900` | `Reuse with adapter` | Hub yang ada khusus antrean dan mengelompokkan peserta berdasarkan nurse station, bukan berdasarkan dokter. Perlu hub baru atau perluasan pengelompokan | Sedang. Teknologinya sudah terbukti jalan, tinggal pola pengelompokannya yang berbeda |
| `CAP-20` | Klien realtime di sisi frontend | Platform | `QuilvianSystemFrontendDev/src/lib/signalr/signalrHubClient.jsx@688daff90` (klien umum) dan `src/lib/realtime/queue-realtime-client.js@688daff90` (khusus antrean) | `Ready to reuse` | Tidak ada. Klien umumnya sudah terpisah dari kebutuhan antrean | Rendah |

### Klaster Frontend

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-21` | Seluruh tampilan Laboratorium | Frontend | Pencarian `laboratory-management`, `lab-order`, `labOrder`, dan `labSpecimen` pada `QuilvianSystemFrontendDev/src@688daff90` **tidak menghasilkan satu berkas pun**. Tidak ada route `src/app/health-services/laboratory-*` | `Missing` | Seluruhnya dibangun dari nol: route, layar, state, API service, dan konstanta | **Tinggi.** Porsi pekerjaan frontend Rilis 1 adalah seratus persen |
| `CAP-22` | Pola berlapis untuk membangun modul baru | Frontend | Modul `pharmacy-management@688daff90` memakai tujuh lapis konsisten: `src/app/health-services/pharmacy-management/`, `src/components/features/health-services/pharmacy-management/`, `src/components/view/health-services/pharmacy-management/`, `src/lib/constants/health-services/pharmacy-management/`, `src/lib/hooks/health-services/pharmacy-management/`, `src/lib/services/health-services/pharmacy-management/`, `src/style/health-services/pharmacy-management/` | `Ready to reuse` | Tidak ada | Rendah. `LAB-DEC-010` memang memerintahkan mengikuti pola modul sejenis, dan polanya jelas |
| `CAP-23` | Pemanggilan API dan pengelolaan state | Frontend | `src/lib/axiosInstance@688daff90`; potongan state Redux di `src/lib/state/slice/@688daff90` | `Ready to reuse` | Tidak ada | Rendah |

### Klaster Pengujian

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-24` | Bukti otomatis bahwa aturan sampel dan batas kewenangan ditegakkan | Laboratorium | `tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratorySpecimenLifecycleTests.cs@9124900` berisi 19 pengujian, antara lain `#SebelumDinyatakanLayak_TidakAdaTagihanYangTerbentuk`, `#PengambilanUlangKesalahanInternal_HanyaMenghasilkanSatuTagihan`, `#PembatalanSetelahLayak_TidakMenghapusTagihanDanMemakaiRevisiBaru`, `#SampelDitolak_TidakMenerbitkanFaktaApaPun`. `LaboratoryAuthorityTests.cs@9124900` berisi 12 pengujian batas kewenangan | `Ready to reuse` | Tidak ada. Pengujian hasil pemeriksaan harus ditambahkan sendiri | Rendah. Justru menjadi contoh gaya pengujian yang bisa ditiru untuk slice hasil |

---

## Conflict

### `CONF-01` — Status `Draft` pada pesanan lab tidak pernah bisa tercapai

| Field | Isi |
|---|---|
| Status | `Conflict` |
| Tingkat | Sedang |
| Memblokir | `DESIGN` bagian pembuatan pesanan |

**Apa yang ditemukan.** `LAB-INH-001` — keputusan terkunci dari `RJ-BIL-GATE-DEC-003` —
menyatakan alur pesanan dimulai dari `Draft`, lalu ke `Requested`. Di dalam kode, nilai `Draft`
memang ada pada `Enums/LaboratoryEnums.cs#LabOrderStatus.Draft@9124900`, tetapi:

1. Pembuatan pesanan **selalu** langsung berstatus `Requested`, lihat
   `Services/LabOrderService.cs:136#OrderStatus = LabOrderStatus.Requested@9124900`.
2. Tidak ada satu pun endpoint atau method yang menetapkan status menjadi `Draft`.
3. Satu-satunya tempat `Draft` disebut adalah pemeriksaan penjagaan di
   `Services/LabSpecimenService.cs:228@9124900`, yaitu baris yang berbunyi
   "jika status pesanan `Draft` atau `Requested`". Baris itu tidak pernah benar-benar bertemu
   nilai `Draft` karena tidak ada yang membuatnya.

**Kenapa ini penting.** `LAB-INH-006` menyatakan dokter boleh mengubah pesanan secara langsung
**sampai** status `Requested`. Bila `Draft` tidak pernah ada, maka praktis dokter tidak punya
ruang menyunting sama sekali: begitu pesanan dibuat, ia langsung terkunci. Pertanyaannya
menjadi keputusan bisnis, bukan keputusan teknis.

**Contoh nyata.** dr. Rina sedang menyusun pesanan berisi lima pemeriksaan untuk pasien Andi.
Di tengah pengisian ia sadar salah memilih satu pemeriksaan. Dengan keadaan kode saat ini,
pesanan sudah berstatus `Requested` sejak tombol Simpan ditekan, sehingga koreksi harus lewat
jalur pembatalan — bukan sekadar menyunting draf.

**Yang perlu diputuskan manusia:** lihat pertanyaan penutup `Q-LAB-01`.

---

## Unknown

### `UNK-01` — Apakah hasil laboratorium harus masuk ke dokumen rekam medis

| Field | Isi |
|---|---|
| Status | `Unknown` |
| Memblokir | `DESIGN` bagian penyajian hasil |

Modul `rekam-medis` ada dan aktif di `Areas/HealthServices/MedicalRecordManagement/@9124900`.
Decision log Laboratorium menempatkan penyimpanan dokumen rekam medis **di luar scope**, dan
menyebut Laboratorium hanya "menyerahkan hasil sebagai isi rekam medis". Namun tidak ada
keputusan yang menyatakan bentuk penyerahan itu: apakah hasil lab cukup dibaca lewat layar
Laboratorium, ataukah harus tersalin menjadi dokumen di rekam medis. Audit tidak boleh
menebaknya. Lihat `Q-LAB-03`.

### `UNK-02` — Siapa pemilik kemampuan pemberitahuan tersimpan

| Field | Isi |
|---|---|
| Status | `Unknown` |
| Memblokir | `DESIGN` bagian pemberitahuan |

`LAB-DEC-012` mewajibkan pemberitahuan tersimpan dibangun pada Rilis 1. Audit membuktikan
kemampuan itu belum ada di mana pun (`CAP-18`). Yang belum jelas adalah siapa yang memilikinya:
bila dibangun di dalam modul Laboratorium, modul lain yang kelak membutuhkan pemberitahuan akan
membangun versinya sendiri dan terjadi duplikasi. Ini pertanyaan kepemilikan modul, bukan
pertanyaan teknis. Lihat `Q-LAB-02`.

---

## Kontrak As-Is

Endpoint yang benar-benar ada pada `9124900`, disajikan seperti tampilan Swagger.

### `[Tags("Health Services / Laboratory Management / Lab Order")]`

Base route: `api/v1/health-services/laboratory-management/lab-orders`
Seluruh endpoint memerlukan login (`[Authorize]`).

| Method | Path | Permission | Ringkasan |
|---|---|---|---|
| `GET` | `/` | `LabOrder / Read` | Menampilkan daftar pesanan lab |
| `GET` | `/{id}` | `LabOrder / Read` | Menampilkan satu pesanan lab |
| `POST` | `/` | `LabOrder / Create` | Membuat pesanan lab baru, langsung berstatus `Requested` |
| `PUT` | `/{id}/start-process` | `LabOrder / Process` | Menandai pesanan mulai dikerjakan |
| `PUT` | `/{id}/complete` | `LabOrder / Process` | Menandai pesanan selesai |
| `PUT` | `/{id}/hold` | `LabOrder / Hold` | Menahan sementara pesanan |
| `PUT` | `/{id}/resume` | `LabOrder / Hold` | Melanjutkan pesanan yang ditahan |
| `PUT` | `/{id}/cancel` | `LabOrder / Update` | Membatalkan pesanan |

### `[Tags("Health Services / Laboratory Management / Lab Specimen")]`

Base route: `api/v1/health-services/laboratory-management/lab-specimens`
Seluruh endpoint memerlukan login (`[Authorize]`).

| Method | Path | Permission | Ringkasan |
|---|---|---|---|
| `GET` | `/rejection-reasons` | `LabSpecimen / Read` | Daftar alasan penolakan sampel |
| `GET` | `/by-order/{labOrderId}` | `LabSpecimen / Read` | Daftar sampel milik satu pesanan |
| `GET` | `/by-order/{labOrderId}/history` | `LabSpecimen / Read` | Riwayat perpindahan status |
| `POST` | `/by-order/{labOrderId}` | `LabSpecimen / Plan` | Menambah sampel pada pesanan |
| `POST` | `/{id}/collect` | `LabSpecimen / Collect` | Mencatat pengambilan sampel |
| `POST` | `/{id}/receive` | `LabSpecimen / Receive` | Mencatat sampel tiba di lab |
| `POST` | `/{id}/accept` | `LabSpecimen / Accept` | Menyatakan sampel layak periksa |
| `POST` | `/{id}/reject` | `LabSpecimen / Accept` | Menolak sampel dengan alasan terkendali |
| `POST` | `/{id}/request-recollection` | `LabSpecimen / Accept` | Meminta pengambilan ulang sampel |
| `POST` | `/{id}/hold` | `LabSpecimen / Hold` | Menahan sampel sementara |
| `POST` | `/{id}/resume` | `LabSpecimen / Hold` | Melanjutkan sampel yang ditahan |
| `POST` | `/{id}/cancel` | `LabSpecimen / Update` | Membatalkan sampel |

**Catatan kontrak.** Menolak sampel dan meminta pengambilan ulang memakai permission yang sama
dengan menyatakan layak, yaitu `LabSpecimen / Accept`. Ini **sesuai** `LAB-INH-007`, yang
memang menyebut "penerimaan/penolakan" sebagai satu kewenangan. Yang dipisah tegas adalah
pengambilan dan penetapan layak — dan pemisahan itu dijaga pengujian
`LaboratoryAuthorityTests.cs#PermissionPengambilanDanPenetapanLayak_TidakBolehSama@9124900`.

### Tabel basis data yang sudah ada

| Tabel | Model | Migration |
|---|---|---|
| `LabOrder` | `LabOrder` | `20260815103436_initializeLabOrder` |
| `TrxLabSpecimen` | `TrxLabSpecimen` | `20260824091610_AddLaboratorySpecimenLifecycle` |
| `TrxLabTransitionHistory` | `TrxLabTransitionHistory` | `20260824091610_AddLaboratorySpecimenLifecycle` |
| `MstLabRejectionReason` | `MstLabRejectionReason` | `20260824091610_AddLaboratorySpecimenLifecycle` |

Terdaftar di `Repositories/ApplicationDbContext.cs:648-654@9124900`.
Layanan terdaftar di `Program.cs:285-286@9124900`.

---

## Ketidakcocokan Frontend dan Backend

| Temuan | Keterangan |
|---|---|
| Seluruh 20 endpoint Laboratorium **tidak punya satu pun pemakai di frontend** | Backend Laboratorium sudah berjalan selama beberapa bulan tanpa layar. Artinya alur pesanan dan sampel selama ini hanya bisa dijalankan lewat pemanggilan API langsung, bukan oleh petugas lab lewat aplikasi |
| Tidak ada menu Laboratorium | Permission `LabOrder` dan `LabSpecimen` terdaftar otomatis lewat `AccessMenuSeeder`, tetapi tidak ada menu yang menampilkannya kepada pengguna |

Ini bukan kerusakan, melainkan konsekuensi wajar dari urutan pengerjaan: backend dibangun lebih
dulu sebagai bagian dari pekerjaan Billing. Tetapi konsekuensinya perlu dicatat: **belum ada
bukti sama sekali bahwa alur ini pernah dipakai petugas sungguhan.**

---

## Pemicu Impact Scan

Peta ini menjadi basi dan wajib dipindai ulang secara terbatas bila salah satu terjadi:

| Pemicu | Yang perlu diperiksa ulang |
|---|---|
| Backend bergerak dari `9124900` | `CAP-01` sampai `CAP-07`, `CAP-11` sampai `CAP-19`, `CAP-24`, dan seluruh kontrak as-is |
| Frontend bergerak dari `688daff90` | `CAP-20` sampai `CAP-23` |
| Ada migration baru menyentuh tabel berawalan `Lab` | `CAP-01` sampai `CAP-05` dan tabel basis data |
| `ClinicalMilestoneFactProducer` berubah | `CAP-11` dan pengujian `CAP-24` |
| `AccessMenuSeeder` atau `AccessPermissionService` berubah | `CAP-13` dan `CAP-14` |
| Blueprint `rawat-jalan` menerbitkan amendment baru menyentuh Laboratorium | Seluruh keputusan warisan `LAB-INH-001` sampai `LAB-INH-013` |

---

## Pertanyaan Penutup untuk `/grill-me`

Pertanyaan berikut **tidak dijawab oleh audit ini**. Semuanya memerlukan keputusan Yoga Aji
Pratama sebagai pemilik modul, sebagian bersama pihak lain.

### `Q-LAB-01` — Apakah pesanan lab perlu tahap draf?

**Latar:** `CONF-01`. Keputusan terkunci `LAB-INH-001` menyebut alur dimulai dari `Draft`,
tetapi kode tidak pernah membuat status itu, sehingga pesanan langsung terkunci begitu dibuat.

**Yang harus diputuskan:** apakah dokter diberi tahap draf untuk menyusun pesanan sebelum
dikirim ke lab, atau alur langsung `Requested` seperti sekarang diterima sebagai perilaku sah
dan `LAB-INH-001` yang perlu diamandemen.

**Pemilik keputusan:** Yoga Aji Pratama, dengan rujukan ke pemilik blueprint `rawat-jalan`
karena `LAB-INH-001` diwarisi dari sana.

### `Q-LAB-02` — Pemberitahuan tersimpan dibangun sebagai milik siapa?

**Latar:** `CAP-18` dan `UNK-02`. Kemampuan ini belum ada di mana pun, sedangkan `LAB-DEC-012`
mewajibkannya di Rilis 1.

**Yang harus diputuskan:** apakah pemberitahuan tersimpan dibangun sebagai kemampuan platform
yang bisa dipakai semua modul, atau dibangun khusus untuk Laboratorium lebih dulu dengan risiko
duplikasi di kemudian hari.

**Pemilik keputusan:** Yoga Aji Pratama bersama pemilik platform.

### `Q-LAB-03` — Apakah hasil lab harus tersalin ke rekam medis?

**Latar:** `UNK-01`. Modul `rekam-medis` sudah ada, tetapi bentuk penyerahan hasil belum pernah
diputuskan.

**Yang harus diputuskan:** apakah hasil laboratorium cukup dibaca dari layar Laboratorium, atau
harus menjadi dokumen tersimpan di rekam medis pasien.

**Pemilik keputusan:** Yoga Aji Pratama bersama pemilik modul `rekam-medis`.

### `Q-LAB-04` — Siapa yang mengisi daftar alasan penolakan sampel?

**Latar:** `CAP-05`. Tabel alasan penolakan hanya punya endpoint baca. Tidak ada layar
pengelolaan dan tidak ditemukan pengisian awal.

**Yang harus diputuskan:** apakah daftar alasan diisi sekali oleh tim teknis sebagai data awal
yang tetap, atau perlu layar pengelolaan agar kepala instalasi bisa menambah dan menonaktifkan
alasan sendiri.

**Pemilik keputusan:** Yoga Aji Pratama.

### `Q-LAB-05` — Di mana batas nilai dan batas waktu cito disimpan?

**Latar:** `CAP-06` dan `CAP-07`. Katalog pemeriksaan saat ini menumpang `MstProcedure` milik
`master-data`, sedangkan tabel batas nilai yang diwajibkan `LAB-DEC-006` dan `LAB-DEC-013`
belum ada.

**Yang harus diputuskan:** apakah batas nilai ditambahkan sebagai kolom pada `MstProcedure`
milik `master-data`, atau menjadi tabel tersendiri milik Laboratorium yang menunjuk ke
`MstProcedure`.

**Pemilik keputusan:** Yoga Aji Pratama bersama pemilik `master-data`.

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-01 | Audit penuh pertama pada backend `9124900` dan frontend `688daff90`. 24 kemampuan diklasifikasikan, 1 conflict dan 2 unknown dicatat, 5 pertanyaan penutup diajukan | `draft` |
