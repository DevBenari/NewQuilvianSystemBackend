# Radiologi — Konformansi PRD Eksternal terhadap Modul Terbangun

| Field | Value |
|---|---|
| Dokumen ID | `RAD-CONF-001` |
| Revision | `3` |
| Status | `approved` — `RAD-CONF-DEC-08` dan `RAD-CONF-DEC-09` ditutup; tujuh pertanyaan lain masih terbuka |
| `approved_by` / `approved_at` | Yoga Aji Pratama, 2026-09-11 |
| Blueprint | `RAD-BP-001` revision 13, status `approved` |
| Masukan baru | *Product Requirements Document (PRD) — Modul Radiologi*, versi PRD 1.0, status **Draft — Development Ready with Open Decisions** |
| Diperiksa terhadap | Source backend `Areas/HealthServices/RadiologyManagement` apa adanya pada 2026-09-11 |
| Tanggal | 2026-09-11 |

> **Kedudukan dokumen ini.** PRD tersebut **bukan** turunan dari blueprint yang sudah
> disetujui. Ia dokumen terpisah yang disusun dari evidence UI HiSys lama dan UI modern, dan
> sampai hari ini belum pernah masuk sebagai masukan sah ke `/qv-grill` maupun
> `requirement-completeness-gate`. Dokumen ini memetakan keduanya apa adanya, **tanpa**
> mengubah satu pun keputusan terkunci dan **tanpa** menyentuh source.

---

## 1. Temuan Utama

Blueprint `RAD-BP-001` dan PRD ini **memotret modul yang sama dari dua sudut yang berbeda**,
dan perbedaannya bukan perbedaan penulisan.

| | Blueprint `RAD-BP-001` | PRD 1.0 |
|---|---|---|
| Sumbu utama | Keselamatan pasien dan keabsahan hasil bacaan | Kelengkapan alur administrasi pemeriksaan |
| Satuan pekerjaan | `RadOrder` → `RadStudy` → `RadReport` berversi | Order → beberapa item pemeriksaan → hasil |
| Yang dijaga ketat | Gerbang keselamatan, pengesah bacaan, versi koreksi | Nomor foto, film, penjamin, cetak, PACS |
| Wewenang finansial | **Tidak ada sama sekali**, ditegakkan dengan meniadakan kolomnya | Status pembayaran tampil pada detail order |
| PACS | Dinonaktifkan `RJ-BIL-GATE-DEC-004` | Wajib ada lapisan integrasi dan `Kirim Ulang ke PACS` |

Akibatnya: **sebagian isi PRD sudah terpenuhi backend, sebagian belum pernah dibangun, dan
sebagian lagi bertentangan dengan keputusan yang sudah disetujui pemilik modul.** Ketiganya
harus dipisahkan sebelum satu baris frontend pun ditulis, karena frontend tidak dapat
menampilkan data yang tidak dimiliki backend.

---

## 2. Dua Persoalan Lintas Dokumen — **Diputuskan 2026-09-11**

Keduanya disetujui Yoga Aji Pratama selaku pemilik modul (`RAD-DEC-014`) pada 2026-09-11.

### `RAD-CONF-ISSUE-01` — Penomoran requirement bentrok — **DITUTUP**

PRD dan blueprint memakai **ruang ID yang sama untuk hal yang berbeda**.

| ID | Arti pada blueprint `RAD-PRD-001` | Arti pada PRD 1.0 |
|---|---|---|
| `FR-RAD-001` | Aturan keselamatan hanya berlaku setelah disahkan | Patient Context |
| `FR-RAD-002` | Pengesahan hanya oleh penanggung jawab klinis | Create Radiology Order |
| `FR-RAD-003` | Pengesahan menaikkan versi tepat satu kali | Multiple Examination Items |
| `FR-RAD-004` | Penolakan wajib beralasan | Examination Catalog |
| `FR-RAD-005` | Papan kesiapan alat | ICD-10 Diagnosis |
| `AC-001`…`AC-010` | tidak dipakai; blueprint memakai `AC-1`…`AC-42` | sepuluh skenario penerimaan produk |

Selama tidak diperbaiki, setiap task frontend yang menyebut `FR-RAD-003` **tidak dapat
dipastikan artinya**. Ini bukan persoalan kerapian: developer yang membaca "Definition of Done:
`FR-RAD-003`" akan mengerjakan hal yang salah separuh waktu.

**Keputusan `RAD-CONF-DEC-08`.** Seluruh requirement PRD 1.0 memakai awalan `PRD-`:
`PRD-FR-RAD-001` sampai `PRD-FR-RAD-012` dan `PRD-AC-001` sampai `PRD-AC-010`. ID blueprint
**tidak disentuh** karena sudah dirujuk 28 task roadmap dan 15 laporan task backend — memindahkan
yang sudah dirujuk jauh lebih berbahaya daripada memindahkan yang belum.

Awalan `BP-RAD-` dan `BR-RAD-` **tidak berubah**: keduanya tidak dipakai blueprint sama sekali,
sehingga tidak pernah bentrok.

Penerapan pada dokumen ini sudah selesai. Yang masih harus menyusul ada pada bagian 11.

### `RAD-CONF-ISSUE-02` — PRD lebih konservatif daripada yang sudah dibangun — **DITUTUP**

PRD bagian 25 melarang menganggap tiga hal berikut final:

| Larangan PRD §25 | Keadaan sebenarnya di backend |
|---|---|
| *workflow approval hasil* | **Sudah dibangun** — `validate` lalu `release`, `RAD-DEC-003` |
| *tanda tangan elektronik hasil* | Tidak dibangun — sejalan |
| *hasil amendment/addendum* | **Sudah dibangun** — `RadReportVersion` berversi, `RAD-DEC-004` |

Keduanya dibangun atas keputusan `RAD-DEC-003` dan `RAD-DEC-004` yang **sudah disetujui
pemilik modul pada 2026-09-09**. PRD disusun tanpa mengetahui keputusan itu.

**Keputusan `RAD-CONF-DEC-09`.** Tidak ada yang ditarik kembali. Pengesahan bertingkat hasil
bacaan dan koreksi berversi **tetap berlaku dan tetap wajib**. Ketika PRD 1.0 dan keputusan
terkunci berselisih, **keputusan terkunci yang menang** — PRD 1.0 belum pernah melewati
`/qv-grill` maupun `requirement-completeness-gate`, sedangkan `RAD-DEC-003` dan `RAD-DEC-004`
sudah disetujui pemilik modul.

Tanda tangan elektronik hasil **tetap di luar scope** dan tetap tunduk pada PRD §25.

---

## 3. Matriks Konformansi — Alur Bisnis PRD

Keterangan status:
**`SESUAI`** terpenuhi backend · **`SEBAGIAN`** sebagian terpenuhi · **`BELUM ADA`** perlu
dibangun · **`BENTROK`** bertentangan dengan keputusan terkunci · **`TBD PRD`** PRD sendiri
menyatakan belum diputuskan.

| PRD | Alur | Status | Bukti / yang kurang |
|---|---|---|---|
| BP-RAD-001 | Pemesanan pemeriksaan | `SEBAGIAN` | Ada `POST /rad-orders` dengan indikasi, jadwal, cito. **Kurang:** jenis rujukan, DPJP/perujuk, ICD-10, beberapa pemeriksaan per order, tampilan harga dan persiapan |
| BP-RAD-002 | Persiapan pasien | `BELUM ADA` | Tidak ada penanda persiapan pada katalog maupun order; daftar Pasien Persiapan tidak dapat dibentuk. Mekanismenya juga `TBD PRD` |
| BP-RAD-003 | Booking / perjanjian | **`SESUAI`** | `ScheduledAt` dan status `Scheduled` ada; penyaring `onlyScheduled`, `scheduledFrom`, dan `scheduledTo` membentuk daftar Pasien Perjanjian sejak bagian 8 |
| BP-RAD-004 | Konfirmasi dan pelaksanaan | `SEBAGIAN` | `accept` → `schedule` → `start` → `complete` ada; konfirmator dan waktunya tersimpan di `RadTransitionHistory`. **Kurang:** penetapan dokter pemeriksa dan petugas radiologi sebagai data, nomor foto, film |
| BP-RAD-005 | Pengisian hasil | `SEBAGIAN` | Draf, pengesahan, perilisan, dan koreksi berversi lengkap. **Kurang:** template hasil, unggah gambar, nomor foto, jumlah/pilihan film |
| BP-RAD-006 | PACS | `BENTROK` | `RJ-BIL-GATE-DEC-004` menyatakan integrasi RIS/PACS **tidak diaktifkan**. Kolom `RadStudy.ExternalStudyUid` ada tetapi sengaja tidak dipakai |
| BP-RAD-007 | Cetak hasil | `BELUM ADA` | Tidak ada endpoint pratinjau maupun cetak. Kolom harga dan total **tidak boleh** lahir di Radiologi — miliknya `billing-kasir` |
| BP-RAD-008 | Riwayat pemeriksaan | **`SESUAI`** | Sejak bagian 8, kedelapan kriteria pencarian PRD tersedia: nama pasien dan No. RM lewat `search`/`medicalRecordNumber`, No. registrasi, No. order, pemeriksaan lewat `procedureId`, status, No. foto lewat `studyNumber`, dan periode lewat `startDate`/`endDate` |
| BP-RAD-009 | Rujukan pemeriksaan luar | `BELUM ADA` | Tidak ada model, endpoint, maupun keputusan. `OPEN-RAD-003` masih terbuka |

---

## 4. Matriks Konformansi — Functional Requirement PRD

| PRD | Requirement | Status | Catatan |
|---|---|---|---|
| PRD-FR-RAD-001 | Patient Context | **`SESUAI`** sejak bagian 8 | Objek `Patient` pada balasan daftar dan rincian memuat No. RM, nama, jenis kelamin, tanggal lahir, umur saat kunjungan, No. registrasi, jenis kunjungan, unit, ruangan, kelas, dan penjamin. **Alergi belum ikut** — lihat bagian 8.2 |
| PRD-FR-RAD-002 | Create Radiology Order | `SEBAGIAN` | Field jenis rujukan, DPJP/perujuk, dokter pemeriksa, dan ICD-10 tidak ada pada `CreateRadOrderRequest` |
| PRD-FR-RAD-003 | Multiple Examination Items | **`BELUM ADA`** | `RadOrder.ProcedureId` dan `ModalityId` tunggal dan wajib. `RadStudy` dapat lebih dari satu, tetapi seluruhnya **mewarisi `ModalityId` pesanan** — satu order tidak dapat memuat X-Ray dan USG sekaligus. Harga, cito, dan dokter pemeriksa per item juga tidak ada |
| PRD-FR-RAD-004 | Examination Catalog | `SEBAGIAN` | `MstProcedure` menyediakan kode dan nama. **Kurang:** harga dan penanda kebutuhan persiapan |
| PRD-FR-RAD-005 | ICD-10 Diagnosis | `BELUM ADA` | `RadOrder` hanya punya `ClinicalIndication` berupa teks bebas |
| PRD-FR-RAD-006 | Consultation Context | `BELUM ADA` | Tidak ada penarikan DPJP kunjungan |
| PRD-FR-RAD-007 | Cito | **`SESUAI`** | `IsUrgent`, `UrgentMarkedByUserId`, `UrgentMarkedAt`, `PUT /{id}/urgency` — `RAD-DEC-013`. Konfirmasi sebelum perubahan adalah kewajiban frontend |
| PRD-FR-RAD-008 | Patient Preparation | `BELUM ADA` | Tidak ada indikator persiapan di mana pun |
| PRD-FR-RAD-009 | Appointment | **`SESUAI`** sejak bagian 8 | Tanggal tersimpan; daftar perjanjian dapat disaring `onlyScheduled` beserta periodenya, dan barisnya dapat dibuka ke rincian |
| PRD-FR-RAD-010 | Patient Queue by Visit Type | `SEBAGIAN` | Empat dari lima kategori dapat dibentuk sejak bagian 8: rawat jalan, rawat inap, dan pasien luar lewat `encounterType`; perjanjian lewat `onlyScheduled`. **Kurang:** Pasien Persiapan — tidak ada indikator persiapan, `RAD-CONF-DEC-05` |
| PRD-FR-RAD-011 | Radiology Order Detail | `SEBAGIAN` | Sejak bagian 8 ada konfirmator, waktu konfirmasi, status pemeriksaan, tanggal selesai, dan konteks pasien. **Kurang:** dokter perujuk, dokter pemeriksa, petugas radiologi, status pembayaran, nomor foto, film |
| PRD-FR-RAD-012 | Examination Confirmation | **`SESUAI`** | `PUT /{id}/accept` menulis `RadTransitionHistory` berisi `ActorUserId` dan `OccurredAt` |

---

## 5. Matriks Konformansi — Bagian Tematik PRD

| PRD | Tema | Status | Catatan |
|---|---|---|---|
| §7 | Examination Status | **Sudah terjawab, PRD tertinggal** | PRD mencatat empat status dan menyatakan urutannya belum ditetapkan (`OPEN-RAD-001`). Backend sudah punya 10 status pesanan dan 11 status study yang dikunci `RAD-STATE-001`. Yang dibutuhkan adalah **pemetaan label**, bukan perubahan backend |
| §8 | Payment Status | `BENTROK` sebagian | Backend sengaja **tidak memiliki satu pun kolom finansial** — `RJ-BIL-GATE-DEC-004`. Menampilkannya tetap mungkin **sebagai pembacaan read-only dari Billing**, dan itu tidak melanggar wewenang. Kontrak pembacaannya belum ada |
| §9 | Film and Photo Management | `BELUM ADA` | `RadAcquisitionConsumption` mencatat film sebagai **fakta pemakaian** tanpa harga — disengaja. Tidak ada nomor foto, tipe/ukuran film, maupun master film. `OPEN-RAD-002` dan `OD-RAD-010` masih terbuka |
| §10 | Radiology Result | `SEBAGIAN` | Isi, dokter pemeriksa, tanggal, dan riwayat versi lengkap. **Kurang:** template hasil dan gambar hasil |
| §11 | Printing | `BELUM ADA` | Cetak hasil, cetak label, dan rincian biaya belum ada. Rincian biaya **bukan milik Radiologi** |
| §12 | External Referral | `BELUM ADA` | `OPEN-RAD-003` terbuka |
| §13 | Obat / BHP / Paket Obat | `SEBAGIAN` | `POST /rad-studies/{id}/consumptions` mencatat pemakaian nyata. Pengurangan stok, charge, dan approval dilarang diasumsikan — sejalan `OPEN-RAD-004` |
| §14 | PACS Integration | `BENTROK` | Lihat BP-RAD-006 |
| §15 | Search and History | **`SESUAI`** sejak bagian 8 | Sama dengan BP-RAD-008. Rincian dapat dibuka dari baris pemeriksaan |
| §16 | Reporting | `BELUM ADA` / `TBD PRD` | `OPEN-RAD-005` |
| §17 | Setup | `TBD PRD` | PRD menyatakan placeholder; jangan bangun master baru |
| §18 | Equipment Identification | `BELUM ADA` | `MstRadModality` menyimpan kode dan nama alat, **bukan** merk, model, dan nomor seri. `OPEN-RAD-006` terbuka |
| §22 | Permissions Matrix | **Sudah terjawab, PRD tertinggal** | Matriks PRD penuh `TBD`. Backend sudah memasang `AccessPermission` pada setiap endpoint dan `RAD-PERM-001` sudah `approved` |

---

## 6. Matriks Konformansi — Business Rules PRD

| PRD | Aturan | Status |
|---|---|---|
| BR-RAD-001 | Indikator persiapan | `BELUM ADA` |
| BR-RAD-002 | Diagnosis awal mengikuti ICD-10, tetap dapat diedit | `BELUM ADA` |
| BR-RAD-003 | Dokter dari DPJP kunjungan saat Konsul | `BELUM ADA` |
| BR-RAD-004 | Kelas mengikuti kelas Standar | Di luar modul — `registration-management` |
| BR-RAD-005 | Promo tampilan tidak persisten | Tidak berlaku — tidak ada promo di Radiologi |
| BR-RAD-006 | Perubahan Cito meminta konfirmasi | Kewajiban frontend; backend siap |
| BR-RAD-007 | Satu order beberapa pemeriksaan | **`BELUM ADA`** — lihat PRD-FR-RAD-003 |
| BR-RAD-008 | Atribut per pemeriksaan | **`BELUM ADA`** |
| BR-RAD-009 | Status klinis ≠ status pembayaran | **`SESUAI` secara prinsip** — backend bahkan tidak menyimpan status pembayaran sama sekali |
| BR-RAD-010 | Perubahan dokter pemeriksa memicu kirim ulang PACS | `BENTROK` |

---

## 7. Matriks Konformansi — Acceptance Criteria PRD

| PRD | Skenario | Status | Penghalang |
|---|---|---|---|
| PRD-AC-001 | Create Order | `SESUAI` | — |
| PRD-AC-002 | Multi Examination | **`BELUM ADA`** | Struktur order satu pemeriksaan |
| PRD-AC-003 | Cito | `SESUAI` di backend | Konfirmasi dikerjakan frontend |
| PRD-AC-004 | Preparation | **`BELUM ADA`** | Tidak ada indikator persiapan |
| PRD-AC-005 | Confirmation | `SESUAI` | — |
| PRD-AC-006 | Examination Processing | `SEBAGIAN` | Konteks pasien, item pemeriksaan, Cito, dan status sudah dapat dilihat sejak bagian 8. Petugas, dokter pemeriksa, film, nomor foto, dan pembayaran belum ada |
| PRD-AC-007 | Result | `SESUAI` | — |
| PRD-AC-008 | Print | **`BELUM ADA`** | Tidak ada pratinjau cetak |
| PRD-AC-009 | History | **`SESUAI`** sejak bagian 8 | Pencarian No. RM, No. registrasi, No. order, dan periode seluruhnya dilayani |
| PRD-AC-010 | Clinical vs Payment Status | **`BELUM ADA`** | Status pembayaran tidak dimiliki Radiologi |

---

## 8. Yang Menghambat Frontend — **Sudah Dikerjakan 2026-09-11**

Empat butir berikut wajib selesai sebelum layar Daftar Pasien Radiologi, Riwayat Pemeriksaan,
dan Detail Order dapat dibangun sesuai PRD. Keempatnya tidak bertentangan dengan satu pun
keputusan terkunci, dan atas persetujuan pemilik modul pada 2026-09-11 keempatnya dikerjakan
lebih dulu.

| No | Yang kurang | Akibat bila dibiarkan | Keadaan |
|---:|---|---|---|
| 1 | **Konteks pasien pada balasan order** (PRD-FR-RAD-001) | Frontend harus memanggil registrasi satu per satu untuk setiap baris daftar. Daftar 50 pasien menjadi 51 panggilan, dan nama pasien berisiko tertukar antar baris | **Selesai** |
| 2 | **Nomor order yang terbaca manusia** | PRD memakai No. Order pada pencarian, label cetak, dan riwayat. Backend tidak punya kolomnya sama sekali — `RadStudy` punya `StudyNumber`, `RadReport` punya `ReportNumber`, `RadOrder` tidak punya apa pun | **Selesai** |
| 3 | **Penyaring daftar dan pencarian** (PRD-FR-RAD-010, §15) | `GET /rad-orders` hanya menerima `encounterId`. Lima kategori daftar pasien dan delapan kriteria riwayat tidak dapat dibentuk | **Selesai** |
| 4 | **Konfirmator dan waktu konfirmasi pada detail order** (PRD-FR-RAD-011) | Datanya ada di `RadTransitionHistory`, tetapi frontend harus memanggil endpoint riwayat terpisah hanya untuk menampilkan dua kolom | **Selesai** |

### 8.1 Apa yang berubah

| Berkas | Perubahan |
|---|---|
| `Models/RadOrder.cs` | Kolom `OrderNumber`, wajib, panjang 64 |
| `Services/RadOrderNumberService.cs` | **Baru.** Membentuk `RAD-ORD-yyMMddHHmmss-XXXXXX`, sebangun dengan `RadReportNumberService`. Tidak memakai `Count + 1` — `QBE-CODE-003` |
| `RadOrderConfiguration.cs` | Index unik `IX_RadOrder_OrderNumber` dengan penyaring `IsDelete = false` |
| `DTOs/RadiologyDtos.cs` | `RadOrderListQuery` berisi 19 penyaring dan `limit`; `RadOrderPatientContextResponse`; `OrderNumber` dan `Patient` pada balasan daftar; `ConfirmedByUserId` dan `ConfirmedAt` pada balasan rincian |
| `Services/RadOrderService.cs` | `TerapkanPenyaring` satu pintu; konteks pasien dibaca dalam satu perjalanan ke database; konfirmator dibaca dari `RadTransitionHistory`; metadata penyaring menyebut persis parameter yang dilayani |
| `Controllers/RadOrderController.cs` | `GET /` mengikat `RadOrderListQuery` |
| `Program.cs` | `RadOrderNumberService` didaftarkan |
| `Migrations/…_AddRadOrderNumber.cs` | Kolom, pengisian mundur, dan index unik |

### 8.2 Keputusan kecil yang diambil saat mengerjakan

| Hal | Yang dipilih | Sebabnya |
|---|---|---|
| Konfirmator | **Diturunkan** dari baris `Order.Accept` pada `RadTransitionHistory`, bukan kolom baru | Kolom salinan berarti dua sumber kebenaran untuk satu fakta, dan yang kedua pasti berselisih dengan riwayat. Yang diselesaikan hanya keharusan frontend memanggil endpoint riwayat terpisah |
| Konfirmasi mana yang dipakai | Konfirmasi **pertama** | Pesanan yang sempat ditahan lalu dilanjutkan tidak berganti konfirmator — yang ditanya layar adalah siapa yang dahulu menerima pesanan ini |
| Konteks pasien | **Dibaca**, tidak satu kolom pun disalin ke tabel radiologi | Nama pasien yang disalin akan berselisih dengan aslinya begitu pendaftaran memperbaiki ejaan nama |
| Alergi | **Tidak diikutkan** | Di repository ini alergi hanya tersimpan sebagai isi dokumen milik `medical-record-management`. Membacanya dari Radiologi adalah titik sentuh lintas modul yang belum diputuskan, dan PRD sendiri menyebutnya bersyarat |
| Batas baris | `limit`, bawaan `200`, paling banyak `1000` | Penyaring riwayat mengubah endpoint ini menjadi pencarian se-rumah-sakit. Perpindahan ke `PagedResult` merusak bentuk response dan tetap menjadi task tersendiri — `RAD-API-001`; batas baris menutup lubangnya tanpa mengubah bentuk apa pun. `GET /episodes/{episodeId}` sengaja tetap tanpa batas |
| Pengisian mundur nomor | Dari waktu baris dibuat + nomor urut dalam detik yang sama | Terjamin tidak kembar tanpa menebak, dan bentuknya sama persis dengan nomor yang diterbitkan sekarang |

### 8.3 Yang **tidak** berubah

Tidak ada kolom finansial yang ditambahkan, tidak ada status pembayaran yang disimpulkan, dan
tidak ada tabel salinan data pasien. `GET /rad-orders` tetap mengembalikan
`ApiResponse<List<…>>` — bentuk response tidak berubah sama sekali, sehingga seluruh pemanggil
yang sudah ada terus berjalan tanpa penyesuaian.

---

## 9. Yang Membutuhkan Keputusan Pemilik Modul

Tidak satu pun boleh dikerjakan sebelum diputuskan, karena semuanya mengubah batas scope yang
sudah dikunci `RAD-DEC-001`.

| ID | Pertanyaan | Mengapa tidak dapat diputuskan developer |
|---|---|---|
| `RAD-CONF-DEC-01` | Apakah satu order boleh memuat beberapa pemeriksaan lintas alat? | Mengubah identitas `RadOrder`, seluruh gerbang keselamatan yang terikat modalitas, dan fakta yang dikirim ke Billing. **Perubahan terbesar dalam daftar ini** |
| `RAD-CONF-DEC-02` | Apakah Radiologi menampilkan penjamin dan status pembayaran dari Billing? | Menampilkan tidak melanggar `RJ-BIL-GATE-DEC-004`, tetapi butuh kontrak pembacaan baru dan persetujuan pemilik Billing |
| `RAD-CONF-DEC-03` | Apakah PACS diaktifkan pada rilis ini? | `RJ-BIL-GATE-DEC-004` menonaktifkannya. PRD §14 dan BR-RAD-010 mengasumsikan sebaliknya. PRD sendiri mengakui kontraknya masih TBD (`OD-RAD-005`) |
| `RAD-CONF-DEC-04` | Apakah nomor foto dan model film masuk Rilis 1? | `OD-RAD-010` masih terbuka; harga film **tidak boleh** masuk Radiologi |
| `RAD-CONF-DEC-05` | Apakah persiapan pasien masuk Rilis 1? | `OD-RAD-003` masih terbuka — instruksi, status selesai, dan siapa yang memvalidasi belum ada |
| `RAD-CONF-DEC-06` | Apakah ICD-10, DPJP/perujuk, dan jenis rujukan ditambahkan ke order? | Menambah relasi lintas modul; ukurannya sedang, risikonya rendah |
| `RAD-CONF-DEC-07` | Apakah rujukan luar, cetak hasil, cetak label, dan laporan masuk Rilis 1? | Empat kemampuan baru; PRD sendiri menyatakan isinya belum final |

---

## 10. Yang Tidak Boleh Dikerjakan

| Hal | Alasan |
|---|---|
| Menambahkan kolom harga, tarif, atau total ke tabel mana pun di Radiologi | `RJ-BIL-GATE-DEC-004` — Radiologi tidak punya wewenang finansial. PRD §11.3 sendiri menyatakan kepemilikan perhitungan tarif belum ditentukan |
| Menarik kembali pengesahan bertingkat dan koreksi berversi hasil bacaan | Dibangun atas `RAD-DEC-003` dan `RAD-DEC-004` yang sudah disetujui; PRD §25 disusun tanpa mengetahuinya |
| Membuat master baru untuk menu Setup | PRD §17 menyatakan placeholder dan melarang master berdasarkan asumsi |
| Mengurangi stok atau membuat charge dari pemakaian obat/BHP | `OPEN-RAD-004` terbuka; stok milik `pharmacy`/`inventory` |
| Membuat state machine baru untuk empat status PRD | `RAD-STATE-001` sudah `approved` dan sudah terpasang di source. Yang dibutuhkan pemetaan label |

---

## 11. Tindak Lanjut `RAD-CONF-DEC-08` dan `RAD-CONF-DEC-09`

Keputusannya sudah diambil; penerapannya belum seluruhnya berada di tangan repository ini.

| Yang harus dilakukan | Pada apa | Pemilik | Keadaan |
|---|---|---|---|
| Memakai awalan `PRD-FR-RAD-` dan `PRD-AC-` | `05-prd-conformance-gap.md` | Modul Radiologi | **Selesai 2026-09-11** |
| Menyebut butir PRD dengan awalan `PRD-` | `blueprint-manifest.md` daftar blocker | Modul Radiologi | **Selesai 2026-09-11** |
| Menomori ulang requirement dan acceptance criteria di dalam berkas PRD itu sendiri | *Product Requirements Document — Modul Radiologi* versi 1.0 | Penyusun PRD | **Belum** — berkasnya PDF dan berada di luar repository ini |
| Memperbaiki §25 supaya tidak terbaca sebagai larangan atas pengesahan bertingkat dan koreksi berversi | PRD versi 1.0 | Penyusun PRD | **Belum** — alasan yang sama |

> **Sampai kedua baris terakhir dikerjakan, berkas PRD dan dokumen ini akan berbeda
> penomorannya.** Yang berlaku bagi pengembangan adalah penomoran pada dokumen ini. Setiap task
> yang menyebut `FR-RAD-xxx` **tanpa** awalan `PRD-` berarti requirement blueprint, bukan
> requirement PRD.

---

## 12. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 3 | 2026-09-11 | **Dua persoalan lintas dokumen ditutup atas persetujuan Yoga Aji Pratama.** `RAD-CONF-DEC-08` — requirement PRD memakai awalan `PRD-FR-RAD-` dan `PRD-AC-`; ID blueprint tidak disentuh. `RAD-CONF-DEC-09` — pengesahan bertingkat dan koreksi berversi tetap berlaku, dan ketika PRD berselisih dengan keputusan terkunci maka keputusan terkunci yang menang. Penomoran di dokumen ini sudah diterapkan; bagian 11 mencatat dua tindak lanjut yang berada di luar repository. Matriks bagian 3 sampai 7 **diselaraskan** dengan pekerjaan bagian 8 — tujuh butir naik status, di antaranya `PRD-FR-RAD-001`, `PRD-FR-RAD-009`, `BP-RAD-003`, `BP-RAD-008`, `PRD-AC-009`, dan §15. Tujuh pertanyaan `RAD-CONF-DEC-01` sampai `RAD-CONF-DEC-07` **tetap terbuka**. | `approved` |
| 2 | 2026-09-11 | Empat penghambat frontend pada bagian 8 dikerjakan atas persetujuan pemilik modul: konteks pasien, nomor pesanan, penyaring daftar, dan konfirmator. `RAD-CONF-DEC-01` — satu order beberapa pemeriksaan — **ditunda** dan tetap terbuka. Bagian 8 diperluas dengan apa yang berubah, keputusan kecil yang diambil, dan apa yang sengaja tidak berubah. `contracts/api-contract.md` dan `roadmap/frontend-roadmap.md` ikut diperbarui. | `draft` |
| 1 | 2026-09-11 | Dokumen dibuat. PRD 1.0 dipetakan terhadap source backend apa adanya: 9 alur bisnis, 12 functional requirement, 13 bagian tematik, 10 business rule, dan 10 acceptance criteria. Empat penghambat frontend dan tujuh pertanyaan keputusan diterbitkan. Dua persoalan lintas dokumen dicatat. | `draft` |
