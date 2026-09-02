# Laboratorium — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Revision | `19` |
| Status | `draft` |
| Pass | `Scope pass` selesai; `Closure pass` selesai (tiga putaran); `Amendment pass` selesai |
| Product/domain owner | **Yoga Aji Pratama** (`yogaaji452@gmail.com`), ditetapkan 2026-09-01 |
| Backend SHA | `9124900` |
| Frontend SHA | `688daff90` (diperbarui 2026-09-01; revision 1-7 tercatat pada `c79bb6ee4`) |
| Tanggal sesi | 2026-09-01 |
| Capability map | `01-existing-capability-map.md` revision 1, audit pada BE `9124900` + FE `688daff90` |

> **Catatan penting soal cara membaca dokumen ini.**
> Dokumen ini adalah catatan wawancara, bukan desain dan bukan izin menulis kode.
> Isinya memisahkan empat hal: **Fact** (fakta yang dibuktikan dari kode atau dokumen yang
> sudah disetujui), **Decision** (keputusan manusia yang berwenang), **Assumption** (dugaan
> yang belum dikonfirmasi), dan **Open Question** (pertanyaan yang masih menunggu jawaban
> pemilik proses).

---

## Peringatan Prasyarat

1. **Scope dikunci tanpa audit kemampuan existing yang formal.**
   Berkas `docs/module-blueprints/laboratorium/01-existing-capability-map.md` belum ada.
   Artinya kemungkinan tumpang tindih (duplikasi) dengan modul lain belum diperiksa secara
   menyeluruh. Bukti yang dipakai di dokumen ini adalah pembacaan langsung terhadap source
   code pada SHA di atas, bukan hasil audit `/qv-trace`.

2. **Sebagian keputusan Laboratorium sudah dikunci lebih dulu di modul lain.**
   Amendment `RJ-BIL-GATE-DEC-003` pada blueprint `rawat-jalan` sudah mengunci siklus hidup
   pesanan lab, spesimen, hasil, pembatalan, dan kelayakan tagih. Wawancara ini **tidak boleh
   membuka ulang** keputusan tersebut; yang digali adalah bagian Laboratorium yang belum
   pernah ditanyakan.

3. **Dokumen tata kelola backend canonical ada dan tetap berlaku, tetapi bukan di `docs/engineering/`.**
   `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` tidak dicabut.
   Keduanya tinggal di repository suite Skill `QuilvianEngineeringSkills`, bukan di dalam
   repository backend. Path `docs/engineering/` yang masih disebut `AGENTS.md` sudah usang.
   `LAB-OPEN-002` **ditutup** 2026-09-01 oleh `LAB-FACT-007`. Penutupannya membuka dua
   penghambat implementasi yang baru dan nyata: rules root yang terpasang belum memuat kedua
   dokumen itu (`LAB-OPEN-018`), dan registry masih mencatat modul Laboratorium berstatus
   `PLANNED` sehingga `QBE-MOD-002` menahan pembuatan entity `Lab*` (`LAB-OPEN-019`).

---

## Batas Scope Modul (disetujui pemilik modul 2026-09-01)

**Modul:** Laboratorium (`laboratorium`).

**Satu kalimat batas scope:**
Modul Laboratorium mengurus perjalanan pemeriksaan laboratorium mulai dari pesanan dokter,
pengambilan dan penerimaan sampel, proses pemeriksaan, sampai hasil dinyatakan sah dan
dirilis — beserta bukti siapa melakukan apa dan kapan.

### Di dalam scope

| No | Kemampuan | Penjelasan singkat | Rilis |
|---:|---|---|---|
| 1 | Pesanan laboratorium (*lab order*) | Dokter memesan pemeriksaan untuk satu kunjungan pasien | Sudah ada |
| 2 | Siklus hidup sampel | Rencana ambil, pengambilan, penerimaan di lab, penerimaan/penolakan sampel, ambil ulang | Sudah ada |
| 3 | Daftar kerja petugas lab (*worklist*) | Antrian pekerjaan lab per hari atau per bagian | **Rilis 1** |
| 4 | Hasil pemeriksaan | Pengisian nilai hasil, verifikasi, validasi, dan rilis hasil | **Rilis 1** |
| 5 | Nilai kritis | Hasil yang berbahaya bagi pasien dan wajib dikabarkan segera ke dokter | **Rilis 1** |
| 6 | Koreksi hasil setelah rilis | Perbaikan hasil dengan tetap menyimpan riwayat versi lama | **Rilis 1** |
| 7 | Penyajian hasil ke dokter dan pasien | Melihat dan mencetak hasil | **Rilis 1** |
| 8 | Seluruh tampilan frontend Laboratorium | Dibangun dari nol, tidak ada satu pun yang bisa dipakai ulang | **Rilis 1** |
| 9 | Pengiriman fakta ke Billing | Laboratorium hanya **mengirim fakta**, misalnya "sampel diterima", tanpa menghitung uang | **Rilis 1** |
| 10 | Riwayat perubahan status yang tidak bisa diubah | Jejak audit untuk seluruh perpindahan status | Sudah ada |
| 11 | Tabel batas nilai per pemeriksaan | Satuan hasil, batas normal bawah/atas, batas kritis bawah/atas, batas waktu cito, pembeda jenis kelamin dan kelompok umur | **Rilis 1** |
| 12 | Pemberitahuan tersimpan untuk dokter | Kotak pemberitahuan berisi nilai kritis dan koreksi hasil, dengan status sudah dibaca atau belum | **Rilis 1** |
| 13 | Penandaan cito dan daftar pantau keterlambatan | Dokter menandai cito, sistem memantau pesanan cito yang lewat batas waktu | **Rilis 1** |
| 14 | Sisa katalog pemeriksaan laboratorium mandiri | Jenis sampel, wadah, volume minimal, metode, paket/panel | Rilis 2 |

### Di luar scope — milik modul lain

| No | Kemampuan | Pemilik modul | Alasan |
|---:|---|---|---|
| 1 | Perhitungan tarif, tagihan, void, refund, pembayaran | `billing-kasir` | Sudah dikunci: Laboratorium tidak punya wewenang finansial (`RJ-BIL-GATE-DEC-003`) |
| 2 | Pemeriksaan radiologi | Modul Radiologi | Sudah dikunci terpisah di `RJ-BIL-GATE-DEC-004` |
| 3 | Pendaftaran pasien dan pembentukan kunjungan (*encounter*) | `registration-management` | Laboratorium hanya menempel pada kunjungan yang sudah ada |
| 4 | Resep dan obat | `pharmacy` | Alur berbeda |
| 5 | Penyimpanan dokumen rekam medis dan penomoran berkas | `rekam-medis` | Laboratorium hanya menyerahkan hasil sebagai isi rekam medis |
| 6 | Data induk umum: pasien, dokter, unit layanan, tarif | `master-data` | Dipakai bersama, bukan dimiliki Laboratorium |
| 7 | Bank darah dan transfusi | Belum ada modul | Dikeluarkan oleh `LAB-DEC-002` |
| 8 | Mikrobiologi (kultur dan uji kepekaan antibiotik) | Belum ada modul | Dikeluarkan oleh `LAB-DEC-002` |
| 9 | Patologi Anatomi (blok parafin, slide, sitologi) | Belum ada modul | Dikeluarkan oleh `LAB-DEC-002` |
| 10 | Stok, pembelian, dan pencatatan pemakaian reagen | `pharmacy`/`inventory` | Dikeluarkan oleh `LAB-DEC-014` |

> Daftar di atas sudah dikonfirmasi lewat `LAB-DEC-001`, `LAB-DEC-002`, dan `LAB-DEC-014`
> pada 2026-09-01.

### Di luar scope — untuk modul lain

| Kebutuhan | Alasan dikeluarkan | Diserahkan kepada |
|---|---|---|
| Mikrobiologi: kultur bakteri dan uji kepekaan antibiotik | Hasilnya bertahap, bukan sekali jadi. Hasil sementara keluar hari ke-2, hasil akhir hari ke-5. Alur hasil `Pending → InProcess → Completed → Validated → Released` yang sudah dikunci `LAB-INH-003` tidak cukup untuk pola ini dan akan memerlukan amendment tersendiri | Slice/modul Mikrobiologi berikutnya |
| Patologi Anatomi: blok parafin, pemotongan slide, sitologi | Alur fisik sampelnya berbeda total dari sampel cair, dan hasilnya berupa narasi diagnosis, bukan angka | Slice/modul Patologi Anatomi berikutnya |
| Bank Darah: uji cocok serasi (*crossmatch*), penelusuran kantong darah, pelaporan reaksi transfusi | Punya aturan keselamatan pasien tersendiri yang berakibat fatal bila salah tangani. Tidak boleh ditumpangkan pada alur sampel biasa | Modul Bank Darah tersendiri |
| Katalog pemeriksaan lab mandiri | Tetap milik Laboratorium, tetapi ditunda ke Rilis 2 oleh `LAB-DEC-001` | Rilis 2 modul ini |

---

## Glossary

| Istilah | Arti dalam bahasa sehari-hari |
|---|---|
| *Lab order* / pesanan lab | Permintaan resmi dari dokter agar pasien diperiksa laboratorium |
| Spesimen / sampel | Bahan yang diambil dari pasien untuk diperiksa, misalnya darah, urin, dahak |
| *Barcode* spesimen | Label unik yang ditempel di tabung sampel agar tidak tertukar |
| *Received* (diterima) | Sampel sudah sampai secara fisik di laboratorium |
| *Accepted* (dinyatakan layak) | Sampel sudah diperiksa kelayakannya dan dinyatakan boleh dikerjakan |
| *Rejected* (ditolak) | Sampel tidak layak diperiksa, misalnya darah menggumpal atau jumlahnya kurang |
| *Recollection* | Pengambilan ulang sampel karena sampel sebelumnya tidak bisa dipakai |
| *Validated* (divalidasi) | Hasil sudah diperiksa kebenarannya oleh petugas yang berwenang |
| *Released* (dirilis) | Hasil sudah boleh dibaca dokter dan pasien |
| Nilai kritis | Hasil yang menandakan bahaya dan wajib dilaporkan segera, misalnya kalium sangat tinggi |
| Nilai rujukan | Batas normal suatu pemeriksaan, misalnya hemoglobin dewasa pria 13–17 g/dL |
| *Turnaround time* (TAT) | Lama waktu dari sampel diterima sampai hasil keluar |
| *Charge eligibility* | Titik saat suatu pemeriksaan sudah sah untuk ditagihkan |

---

## Fakta dari Source Code (bukti, bukan keputusan)

Seluruh fakta di bawah dibaca langsung dari backend pada SHA `9124900`.

### F1 — Modul Laboratorium backend sudah ada sebagian

Folder `Areas/HealthServices/LaboratoryManagement` berisi 12 berkas: 2 controller, 2 service,
4 model, 1 enum, 2 DTO, dan 1 konfigurasi Entity Framework.

Model yang sudah ada:

| Model | Isi pokok |
|---|---|
| `LabOrder` | Pesanan lab; punya `EncounterId`, `ProcedureId`, `OrderStatus`, `StatusBeforeHold`, `Version` |
| `TrxLabSpecimen` | Sampel; punya `SpecimenBarcode`, `SpecimenSequence`, salinan tarif, jejak siapa mengambil/menerima/memutuskan, sebab ambil ulang, dan tautan ke sampel yang digantikan |
| `MstLabRejectionReason` | Daftar alasan penolakan sampel yang terkendali |
| `TrxLabTransitionHistory` | Riwayat perpindahan status yang tidak bisa diubah |

### F2 — Status yang sudah dikodekan

| Enum | Nilai |
|---|---|
| `LabOrderStatus` | `Draft`, `Requested`, `Accepted`, `InProcess`, `Completed`, `OnHold`, `CancelRequested`, `Cancelled` |
| `LabSpecimenStatus` | `Planned`, `Collected`, `Received`, `Accepted`, `Rejected`, `RecollectionRequired`, `Cancelled`, `OnHold` |
| `LabRecollectionCause` | `InternalHospitalError`, `PatientOrSpecimenCondition`, `ExternalCause` |
| `LabTransitionScope` | `LabOrder`, `LabSpecimen` |

### F3 — Endpoint yang sudah tersedia

#### `[Tags("Health Services / Laboratory Management / Lab Order")]`

Base route: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Ringkasan |
|---|---|---|
| `GET` | `/` | Menampilkan daftar pesanan lab |
| `GET` | `/{id}` | Menampilkan satu pesanan lab |
| `POST` | `/` | Membuat pesanan lab baru |
| `PUT` | `/{id}/start-process` | Menandai pesanan mulai dikerjakan |
| `PUT` | `/{id}/complete` | Menandai pesanan selesai |
| `PUT` | `/{id}/hold` | Menahan sementara pesanan |
| `PUT` | `/{id}/resume` | Melanjutkan pesanan yang ditahan |
| `PUT` | `/{id}/cancel` | Membatalkan pesanan |

#### `[Tags("Health Services / Laboratory Management / Lab Specimen")]`

Base route: `api/v1/health-services/laboratory-management/lab-specimens`

| Method | Path | Ringkasan |
|---|---|---|
| `GET` | `/rejection-reasons` | Daftar alasan penolakan sampel |
| `GET` | `/by-order/{labOrderId}` | Daftar sampel milik satu pesanan |
| `GET` | `/by-order/{labOrderId}/history` | Riwayat perpindahan status satu pesanan |
| `POST` | `/by-order/{labOrderId}` | Menambah sampel pada pesanan |
| `POST` | `/{id}/collect` | Mencatat pengambilan sampel |
| `POST` | `/{id}/receive` | Mencatat sampel diterima di lab |
| `POST` | `/{id}/accept` | Menyatakan sampel layak diperiksa |
| `POST` | `/{id}/reject` | Menolak sampel |
| `POST` | `/{id}/request-recollection` | Meminta pengambilan ulang sampel |
| `POST` | `/{id}/hold` | Menahan sementara sampel |
| `POST` | `/{id}/resume` | Melanjutkan sampel yang ditahan |
| `POST` | `/{id}/cancel` | Membatalkan sampel |

### F4 — Yang belum ada sama sekali di backend

| Kemampuan | Bukti |
|---|---|
| Hasil pemeriksaan (`LabResult`) | Tidak ada model, service, controller, maupun enum status hasil |
| Nilai kritis dan notifikasinya | Tidak ada |
| Nilai rujukan normal | Tidak ada tabel penyimpan batas normal |
| Katalog pemeriksaan lab khusus | Tidak ada. Yang dipakai adalah `MstProcedure` dengan penanda `IsLaboratory` |
| Paket/panel pemeriksaan | Tidak ada |
| Daftar kerja (*worklist*) petugas lab | Tidak ada endpoint khusus |
| Integrasi alat laboratorium | Tidak ada |
| Pengukuran waktu penyelesaian (TAT) | Tidak ada |

### F5 — Frontend belum punya modul Laboratorium sama sekali

Pada SHA `c79bb6ee4`, folder `src/components/view/health-services/` berisi
`billing-management`, `emergency-installation-management`, `inpatient-management`,
`master-data`, `medical-record-management`, `operating-room-management`,
`patient-management`, `pharmacy-management`, dan `registration-management`.
**Tidak ada** `laboratory-management`. Tidak ada pula route `src/app/health-services/laboratory-*`.
Artinya seluruh tampilan Laboratorium harus dibangun dari nol.

### F6 — Katalog pemeriksaan lab saat ini menumpang pada `MstProcedure`

`MstProcedure` memiliki kolom penanda `IsLaboratory`. Tidak ada kolom untuk jenis sampel,
wadah, volume minimal, metode, satuan hasil, atau nilai rujukan. Semua atribut khas
laboratorium belum punya tempat.

### F7 — Platform belum punya sarana notifikasi umum

Diperiksa pada 2026-09-01 untuk menjawab `LAB-OPEN-008`.

| Yang dicari | Hasil |
|---|---|
| Layanan notifikasi umum (`INotificationService` atau sejenisnya) | **Tidak ada** |
| Tabel notifikasi tersimpan (kotak masuk pemberitahuan per pengguna) | **Tidak ada** |
| Surel (SMTP/MailKit) | **Tidak ada** |
| Pesan singkat atau WhatsApp (Twilio, Fonnte, dan sejenisnya) | **Tidak ada** |
| Sarana realtime | **Ada, tetapi khusus antrean.** `Hubs/QueueHub.cs` dipetakan ke `/hubs/queues` lewat SignalR, dan pengelompokannya per *nurse station cluster* untuk kebutuhan antrean nurse station serta doctor queue |

Artinya: teknologi realtime sudah terpasang dan terbukti jalan di produksi, tetapi **belum ada
pemberitahuan yang tersimpan**. Pemberitahuan lewat SignalR saja hanya sampai kepada dokter
yang kebetulan sedang membuka aplikasi. Dokter yang sedang menutup aplikasi tidak akan pernah
tahu ada nilai kritis. Fakta ini menjadi dasar pilihan pada `LAB-OPEN-008`.

---

## Keputusan yang Sudah Dikunci di Modul Lain (diwarisi, tidak dibuka ulang)

Sumber: `docs/module-blueprints/rawat-jalan/00-interview-decisions.md`,
amendment `RJ-BIL-GATE-DEC-003` tanggal 2026-08-19, status `locked-draft`,
bukti approval SHA-256 `4d44472028622f6a9c460f78c4ff61c6fa29d57b6ce5f46f8c6d2b820373c2cc`.

> Status tata kelola formal amendment itu masih `OPEN`: tanda tangan Laboratorium, Clinical
> Governance, dan Billing/Finance belum dilampirkan.

| Kode | Isi yang sudah dikunci |
|---|---|
| `LAB-INH-001` | Alur pesanan: `Draft → Requested → Accepted → InProcess → Completed`, dengan pengecualian `OnHold`, `CancelRequested`, `Cancelled` |
| `LAB-INH-002` | Alur sampel: `Planned → Collected → Received → Accepted`, dengan pengecualian `Rejected`, `RecollectionRequired`, `Cancelled`, `OnHold` |
| `LAB-INH-003` | Alur hasil: `Pending → InProcess → Completed → Validated → Released`; koreksi lewat `Corrected/Amended → Revalidated → Released` dan tetap menyimpan riwayat rilis lama |
| `LAB-INH-004` | `Validated` dan `Released` adalah status **hasil**, bukan status pesanan, dan bukan pemicu awal penagihan |
| `LAB-INH-005` | Satu pesanan boleh punya banyak sampel. Ambil ulang membuat identitas sampel **baru** dan menyimpan sampel lama beserta tautan sebabnya |
| `LAB-INH-006` | Dokter boleh mengubah langsung hanya sampai status `Requested`. Setelah itu dokter hanya melihat, menambah keterangan klinis, atau **mengajukan** pembatalan/koreksi |
| `LAB-INH-007` | Pengambilan, penerimaan, penerimaan/penolakan, pemrosesan, validasi, dan rilis memakai kewenangan yang berbeda-beda. Jabatan tidak otomatis memberi kewenangan |
| `LAB-INH-008` | "Sampel sampai di lab" (`Received`) **tidak sama dengan** "sampel dinyatakan layak" (`Accepted`) |
| `LAB-INH-009` | Titik sah untuk ditagihkan adalah `Accepted`. `Requested`, `Collected`, dan `Received` **bukan** pemicu tagihan pemeriksaan |
| `LAB-INH-010` | Laboratorium hanya mengirim fakta klinis dan pemakaian bahan. Billing adalah satu-satunya pemilik akibat finansial |
| `LAB-INH-011` | Ambil ulang karena kesalahan internal rumah sakit **tidak boleh** otomatis menambah tanggungan pasien |
| `LAB-INH-012` | Laboratorium tidak punya `Paid`, penyelesaian pembayaran, persetujuan penjamin, void, refund, maupun pembalikan transaksi |
| `LAB-INH-013` | Setiap perpindahan status yang penting menghasilkan riwayat yang tidak bisa diubah, memuat identitas, status asal/tujuan, tindakan, alasan, pelaku, waktu, dan ID korelasi |

**Contoh penerapan `LAB-INH-009` dan `LAB-INH-011` agar jelas:**

> Pasien Budi dipesankan pemeriksaan Hemoglobin. Perawat mengambil darah pukul 08.00
> (`Collected`), sampel sampai di lab pukul 08.20 (`Received`), lalu petugas lab memeriksa
> kelayakannya dan menyatakan layak pukul 08.25 (`Accepted`). **Baru pada pukul 08.25** itu
> pemeriksaan Hemoglobin sah untuk ditagihkan.
>
> Bila ternyata pukul 08.30 tabung sampel pecah karena kelalaian petugas lab, sampel harus
> diambil ulang dengan sebab `InternalHospitalError`. Pemeriksaan Hemoglobin **tetap satu
> kali** ditagihkan kepada Budi. Biaya pengambilan ulang menjadi tanggungan rumah sakit,
> bukan Budi.

---

## Aktor dan Tanggung Jawab

Daftar sementara, menunggu konfirmasi pemilik proses.

| Aktor | Tanggung jawab yang diperkirakan | Status |
|---|---|---|
| Dokter pemesan | Membuat pesanan lab, membaca hasil, mengajukan pembatalan | `assumption` |
| Perawat / flebotomis | Mengambil sampel dari pasien | `assumption` |
| Petugas penerimaan lab | Menerima sampel, memutuskan layak atau ditolak | `assumption` |
| Analis laboratorium | Mengerjakan pemeriksaan dan mengisi hasil | `assumption` |
| Penanggung jawab teknis / dokter patologi klinik | Memvalidasi dan merilis hasil | `assumption` |
| Kepala instalasi laboratorium | Menyetujui pengecualian dan kebijakan lokal | `assumption` |
| Petugas Billing | Menerima fakta dari Laboratorium dan menentukan tagihan | `fact` (`LAB-INH-010`) |

---

## Business Rules dan Invariants

Selain warisan `LAB-INH-001` sampai `LAB-INH-013`, sesi ini mengunci aturan berikut.

### BR-01 — Prinsip empat mata pada validasi hasil (`LAB-DEC-003`)

**Aturan:** Petugas yang mengetik angka hasil **tidak boleh** menjadi petugas yang
memvalidasi dan merilis hasil yang sama. Sistem menolak percobaan itu pada keadaan normal.

**Jalur pengecualian:** Bila keadaan memaksa, misalnya shift malam hanya ada satu analis,
sistem tetap mengizinkan dengan tiga syarat wajib:

1. Petugas mengisi alasan pengecualian dari daftar alasan yang terkendali.
2. Hasil diberi penanda permanen "divalidasi oleh pengisi sendiri".
3. Penanda itu ikut tercetak pada lembar hasil dan tersimpan di riwayat yang tidak bisa diubah.

**Contoh:**

> Analis Sari bertugas malam sendirian. Pukul 23.40 ia mengetik hasil Natrium pasien Andi
> sebesar 128 mmol/L. Ia menekan tombol Validasi. Sistem menahan dan menampilkan pesan:
> "Anda yang mengisi hasil ini. Validasi oleh orang yang sama memerlukan alasan."
> Sari memilih alasan "Shift tunggal, tidak ada validator lain bertugas" dan menekan Lanjut.
> Hasil dirilis pukul 23.42, dan pada lembar hasil tercetak keterangan
> "Divalidasi oleh pengisi sendiri — Sari — Shift tunggal". Keesokan paginya kepala
> instalasi melihat hasil ini di daftar pantau pengecualian.

**Konsekuensi bila dilanggar:** Kesalahan ketik satu digit pada Kalium — misalnya 3,5 diketik
menjadi 7,5 — bisa membuat dokter memberikan terapi yang salah. Karena itu aturan ini
diperlakukan sebagai invariant keselamatan, bukan sekadar preferensi.

### BR-02 — Nilai kritis wajib dilaporkan dan dicatat (`LAB-DEC-004`)

**Aturan:** Hasil bernilai kritis **tetap dirilis** agar dokter segera melihatnya. Namun
pemeriksaan itu **belum dianggap tuntas** sampai catatan pelaporan terisi lengkap:

| Isian wajib | Contoh |
|---|---|
| Siapa yang melapor | Analis Sari |
| Kepada siapa | dr. Rina, DPJP pasien Andi |
| Kapan | 2026-09-01 pukul 23.45 |
| Lewat apa | Telepon |
| Bukti pembacaan ulang | dr. Rina mengulang "Kalium tujuh koma dua", dicentang Sari |

**Aturan turunan:** Sistem menyediakan daftar pantau berisi hasil kritis yang **belum**
dilaporkan, agar kepala instalasi bisa menegur sebelum menjadi insiden.

**Contoh:**

> Hasil Kalium pasien Andi keluar 7,2 mmol/L pukul 23.44, sedangkan batas kritis atas yang
> ditetapkan adalah 6,0 mmol/L. Sistem langsung merilis hasil sehingga dr. Rina bisa
> membukanya, sekaligus memunculkan formulir pelaporan yang harus diisi Sari. Selama formulir
> itu kosong, pemeriksaan Kalium tetap muncul di daftar "nilai kritis belum dilaporkan".

**Alasan pilihan ini:** Menahan hasil kritis justru memperlambat penanganan pasien, sedangkan
pelaporan lisan tanpa catatan tidak bisa dibuktikan saat ditelusuri auditor atau saat terjadi
sengketa.

### BR-03 — Hasil Rilis 1 diketik manual (`LAB-DEC-005`)

**Aturan:** Pada Rilis 1, seluruh angka hasil diketik oleh analis. Tidak ada sambungan
otomatis ke alat laboratorium.

**Risiko yang harus disadari:** Pengguna memilih pengetikan manual **tanpa** menyiapkan tempat
data untuk hasil yang dikirim alat. Artinya sistem Rilis 1 tidak menyimpan asal hasil, nomor
seri alat, maupun waktu kirim alat. Bila kelak alat disambungkan, struktur data hasil harus
diubah dan riwayat hasil lama tidak akan bisa membedakan mana yang diketik orang dan mana yang
dikirim alat. Risiko ini dicatat sebagai `LAB-RISK-001`.

**Pengaman yang sudah ada:** Risiko salah ketik ditutup oleh BR-01 (prinsip empat mata).

### BR-04 — Batas nilai disimpan sebagai data induk sejak Rilis 1 (`LAB-DEC-006`)

**Latar belakang conflict:** `LAB-DEC-001` menunda katalog lab mandiri ke Rilis 2, sedangkan
`LAB-DEC-004` mewajibkan sistem mengenali nilai kritis. `MstProcedure` tidak punya kolom
satuan, batas normal, maupun batas kritis (F6). Tanpa penyelesaian, Rilis 1 tidak akan pernah
tahu bahwa Kalium 7,2 mmol/L itu berbahaya.

**Aturan:** Rilis 1 tetap mendapat **satu tabel batas nilai** per jenis pemeriksaan. Isinya
hanya yang benar-benar dibutuhkan untuk menilai hasil:

| Isian | Contoh Kalium | Contoh Hemoglobin |
|---|---|---|
| Satuan hasil | mmol/L | g/dL |
| Batas normal bawah | 3,5 | 13,0 (pria dewasa) |
| Batas normal atas | 5,1 | 17,0 (pria dewasa) |
| Batas kritis bawah | 2,5 | 7,0 |
| Batas kritis atas | 6,0 | 20,0 |
| Pembeda jenis kelamin | Tidak | Ya |
| Pembeda kelompok umur | Tidak | Ya |

**Yang tetap ditunda ke Rilis 2:** jenis sampel, jenis wadah/tabung, volume minimal, metode
pemeriksaan, dan paket/panel.

**Contoh:**

> Analis Sari mengetik hasil Kalium 7,2 mmol/L. Sistem membandingkannya dengan tabel batas
> nilai: 7,2 lebih besar dari batas kritis atas 6,0. Sistem langsung menandai hasil sebagai
> nilai kritis dan memunculkan formulir pelaporan BR-02. Bila hasilnya 5,3 — di atas normal
> tetapi belum kritis — sistem hanya menandainya "di atas nilai rujukan" tanpa formulir
> pelaporan.

### BR-05 — Koreksi hasil setelah rilis (`LAB-DEC-007`)

**Aturan:**

1. Koreksi hasil yang sudah dirilis hanya boleh dilakukan petugas yang punya kewenangan
   validasi atau rilis. Analis biasa tidak boleh.
2. Begitu hasil perbaikan dirilis ulang, dokter pemesan **otomatis** mendapat pemberitahuan
   bahwa hasil pasiennya berubah.
3. Versi hasil yang lama **tetap terlihat**, diberi tanda "sudah diperbaiki", dan tidak
   dihapus. Ini melanjutkan `LAB-INH-003`.

**Contoh:**

> Hasil Hemoglobin pasien Andi dirilis 9,4 g/dL pukul 10.00. Pukul 14.00 ketahuan angka yang
> benar adalah 4,9 g/dL karena tertukar dengan sampel lain. Tono, yang berwenang validasi,
> membuat koreksi. Sistem menyimpan versi lama 9,4 dengan tanda "sudah diperbaiki", merilis
> versi baru 4,9, lalu mengirim pemberitahuan ke dr. Rina. dr. Rina yang tadinya menganggap
> pasien tidak perlu transfusi kini tahu keadaan sebenarnya.

**Alasan pilihan ini:** Dokter mungkin sudah terlanjur memberi terapi berdasarkan angka yang
salah. Tanpa pemberitahuan, koreksi tidak ada gunanya bagi pasien.

### BR-06 — Hasil boleh dirilis sebagian (`LAB-DEC-008`)

**Aturan:** Setiap pemeriksaan dalam satu pesanan punya status hasilnya sendiri dan dirilis
begitu selesai divalidasi, tanpa menunggu pemeriksaan lain. Lembar hasil **wajib** menandai
mana yang sudah keluar dan mana yang masih diproses.

**Contoh:**

> dr. Rina memesan Hemoglobin, Leukosit, dan Kalium sekaligus untuk pasien Andi. Kalium selesai
> pukul 09.10 dengan nilai kritis 7,2 mmol/L, sedangkan Hemoglobin dan Leukosit baru selesai
> pukul 10.30. Kalium dirilis pukul 09.12 sehingga dr. Rina bisa langsung bertindak. Lembar
> hasil saat itu menampilkan Kalium 7,2 (kritis) dan keterangan "Hemoglobin: masih diproses,
> Leukosit: masih diproses".

**Aturan turunan yang wajib:** Selama masih ada pemeriksaan yang belum keluar, lembar hasil
harus memuat peringatan "hasil belum lengkap" agar dokter tidak salah mengira pemeriksaan
sudah tuntas.

### BR-07 — Laboratorium melayani seluruh unit sejak Rilis 1 (`LAB-DEC-009`)

**Aturan:** Modul Laboratorium menerima pesanan dari Rawat Jalan, Rawat Inap, dan IGD sekaligus
sejak Rilis 1. Tidak ada pembedaan alur kerja berdasarkan unit asal.

**Dasar teknis:** `LabOrder` sudah memakai `EncounterId` yang mengacu ke `TrxPatientEncounter`
umum, sehingga tidak ada penghalang di sisi data (F1).

**Aturan turunan yang wajib:** Karena IGD ikut dilayani, pesanan harus punya **penanda tingkat
kesegeraan** — sekurang-kurangnya "biasa" dan "cito/segera". Pesanan cito tidak boleh mengantre
di belakang pesanan rawat jalan rutin pada daftar kerja petugas.

**Contoh:**

> Pukul 10.00 ada 14 pesanan rawat jalan rutin menunggu dikerjakan. Pukul 10.05 masuk pesanan
> Kalium cito dari IGD untuk pasien tidak sadar. Pesanan IGD itu harus muncul di urutan paling
> atas daftar kerja analis, bukan di urutan ke-15.

Definisi resmi "cito" dan batas waktunya ditutup oleh BR-09.

### BR-08 — Pemberitahuan disimpan, bukan sekadar dikirim (`LAB-DEC-012`)

**Latar belakang:** `LAB-DEC-004` mewajibkan dokter tahu ada nilai kritis, dan `LAB-DEC-007`
mewajibkan dokter tahu hasilnya dikoreksi. Fakta F7 menunjukkan platform belum punya sarana
apa pun untuk itu, kecuali SignalR khusus antrean.

**Aturan:**

1. Setiap pemberitahuan disimpan sebagai baris data milik dokter tujuan, bukan sekadar dikirim
   sekilas lalu hilang.
2. Setiap pemberitahuan menyimpan sekurang-kurangnya: dokter tujuan, jenis pemberitahuan
   (nilai kritis atau koreksi hasil), pesanan dan pemeriksaan yang dimaksud, pasien, waktu
   dibuat, serta status sudah dibaca atau belum beserta waktu dibacanya.
3. Pemberitahuan muncul di kotak pemberitahuan dokter saat ia membuka aplikasi, tanpa
   memandang apakah ia sedang online ketika pemberitahuan itu dibuat.
4. SignalR dipakai sebagai pelengkap agar pemberitahuan muncul seketika bila dokter kebetulan
   sedang membuka aplikasi. SignalR **bukan** satu-satunya jalur.

**Contoh:**

> Hasil Kalium pasien Andi keluar 7,2 mmol/L pukul 23.44. dr. Rina sedang tidak membuka
> aplikasi. Pemberitahuan tetap tersimpan. Pukul 05.30 dr. Rina membuka aplikasi dan langsung
> melihat satu pemberitahuan belum dibaca: "Nilai kritis — Kalium 7,2 mmol/L — pasien Andi —
> 2026-09-01 23.44". Sistem mencatat pemberitahuan itu dibaca pukul 05.31.
>
> Bandingkan bila hanya memakai SignalR: pemberitahuan pukul 23.44 itu hilang begitu saja, dan
> rumah sakit tidak punya bukti apa pun bahwa dokter pernah dikabari.

**Hubungan dengan BR-02:** Pemberitahuan otomatis ini **tidak menggantikan** kewajiban
pelaporan lisan pada BR-02. Keduanya berjalan bersama: pelaporan lisan untuk penanganan
segera, pemberitahuan tersimpan sebagai bukti dan jaring pengaman.

### BR-09 — Aturan cito dan batas waktu penyelesaian (`LAB-DEC-013`)

**Aturan:**

1. Dokter pemesan menandai sendiri pesanan sebagai cito ketika membuat pesanan. Tidak ada
   penandaan otomatis berdasarkan unit asal.
2. Setiap jenis pemeriksaan punya **batas waktu penyelesaian cito** sendiri, disimpan bersama
   tabel batas nilai pada `LAB-DEC-006`.
3. Batas waktu dihitung sejak sampel dinyatakan layak (`Accepted`) sampai hasil dirilis
   (`Released`).
4. Sistem menampilkan pesanan cito yang sudah melewati batas waktunya sebagai daftar pantau
   tersendiri.

**Contoh:**

| Pemeriksaan | Batas waktu cito | Contoh perhitungan |
|---|---|---|
| Kalium | 60 menit | Sampel layak pukul 09.00, hasil dirilis pukul 09.45 → **memenuhi**, sisa 15 menit |
| Kalium | 60 menit | Sampel layak pukul 09.00, hasil dirilis pukul 10.20 → **lewat batas** 20 menit, masuk daftar pantau |
| Hemoglobin | 45 menit | Sampel layak pukul 14.00, belum dirilis sampai pukul 14.50 → **lewat batas**, muncul di daftar pantau meski hasil belum keluar |

**Alasan pilihan ini:** Dokter yang paling tahu kondisi pasiennya, sehingga penandaan cito
diserahkan kepadanya. Namun tanpa batas waktu yang terukur, "cito" hanya menjadi label — dan
lama-lama semua dokter menandai cito sehingga prioritas kehilangan arti. Batas waktu membuat
kepatuhan bisa dibuktikan.

### BR-10 — Reagen bukan urusan Laboratorium (`LAB-DEC-014`)

**Aturan:** Modul Laboratorium **tidak** mengelola stok, pembelian, maupun pencatatan
pemakaian reagen. Seluruhnya menjadi urusan modul Farmasi atau Inventory.

**Akibat yang harus disadari:** Laporan biaya bahan per pemeriksaan belum akan tersedia sampai
modul Inventory menanganinya. Bila kelak Billing membutuhkan angka pemakaian bahan nyata untuk
Actual Consumption Rule, kebutuhan itu harus diajukan ke modul Inventory, bukan ditambahkan ke
Laboratorium.

### BR-11 — Ruang koreksi dokter tanpa tahap draf (`LAB-DEC-015`)

**Latar belakang conflict `CONF-01`.** Capability map menemukan pertentangan: keputusan warisan
`LAB-INH-001` menyebut pesanan dimulai dari status `Draft`, tetapi kode tidak pernah membuat
status itu. Bukti: `Services/LabOrderService.cs:136@9124900` selalu menetapkan `Requested` saat
pesanan dibuat, dan tidak ada satu pun kode yang menetapkan `Draft`. Akibatnya `LAB-INH-006` —
yang memberi dokter ruang menyunting "sampai `Requested`" — menjadi kosong, karena pesanan
sudah `Requested` sejak detik pertama.

**Aturan yang menutup conflict ini:**

1. Status `Draft` **dihapus** dari siklus hidup pesanan laboratorium. Pesanan tetap dibuat
   langsung berstatus `Requested`, persis seperti perilaku kode saat ini.
2. Dokter pemesan **boleh menyunting** pesanannya — menambah, mengurangi, atau mengganti
   pemeriksaan — **selama belum ada satu pun sampel yang diambil** pada pesanan itu.
3. Begitu sampel pertama berpindah ke status `Collected`, seluruh pesanan terkunci bagi dokter.
   Sejak titik itu dokter hanya boleh melihat, menambah keterangan klinis, atau **mengajukan**
   pembatalan sesuai `LAB-INH-006`.
4. Setiap penyuntingan pesanan tetap menghasilkan baris riwayat yang tidak bisa diubah, sama
   seperti perpindahan status lain.

**Contoh:**

> dr. Rina memesan Hemoglobin, Leukosit, dan Kalium untuk pasien Andi pukul 08.00. Pukul 08.03
> ia sadar seharusnya Natrium, bukan Kalium. Perawat belum mengambil darah. dr. Rina mengganti
> Kalium menjadi Natrium, dan sistem mengizinkannya sambil mencatat perubahan itu di riwayat.
>
> Bandingkan: pukul 08.10 perawat Dewi sudah mengambil darah dan sampel berstatus `Collected`.
> Pukul 08.12 dr. Rina ingin menambah pemeriksaan Trombosit. Sistem menolak penyuntingan.
> dr. Rina harus membuat pesanan baru, atau mengajukan pembatalan bila memang seluruh pesanan
> keliru.

**Kenapa batas "belum ada sampel diambil" yang dipilih.** Batas ini jelas, mudah diperiksa
mesin, dan masuk akal secara nyata: begitu darah pasien sudah diambil, mengubah daftar
pemeriksaan berarti tabung yang sudah terisi bisa tidak cocok lagi dengan yang dipesan.

**Akibat ke blueprint lain.** `LAB-INH-001` dan `LAB-INH-006` diwarisi dari
`RJ-BIL-GATE-DEC-003` milik blueprint `rawat-jalan`. Keduanya perlu diamandemen di sana:
`Draft` dihapus dari siklus hidup, dan batas kewenangan dokter diubah dari "sampai `Requested`"
menjadi "sampai sampel pertama diambil". Dicatat sebagai `LAB-AMD-001`.

### BR-12 — Pemberitahuan adalah kemampuan platform, bukan milik Laboratorium (`LAB-DEC-016`)

**Latar belakang.** Capability map `CAP-18` membuktikan platform belum punya sarana
pemberitahuan apa pun: tidak ada tabel penyimpan pemberitahuan, tidak ada surel, tidak ada
pesan singkat. Yang ada hanya `Hubs/QueueHub.cs@9124900` yang khusus melayani antrean.

**Aturan:**

1. Pemberitahuan tersimpan dibangun sebagai **kemampuan platform bersama**, bukan milik modul
   Laboratorium. Modul mana pun boleh memakainya: Farmasi, Radiologi, Billing, dan lainnya.
2. Laboratorium menjadi **pemakai pertama** yang membuktikan bentuknya, dengan dua jenis
   pemberitahuan: nilai kritis dan koreksi hasil.
3. Dokter memiliki **satu kotak masuk** untuk seluruh pemberitahuan dari semua modul, bukan
   satu kotak per modul.
4. Bentuk data pemberitahuan harus cukup umum sejak awal: pengguna tujuan, jenis, judul, isi,
   penunjuk ke data sumber, waktu dibuat, status sudah dibaca, dan waktu dibaca. Istilah khas
   laboratorium tidak boleh masuk ke struktur umum itu.

**Contoh kenapa ini penting.**

> Bila pemberitahuan dibangun khusus Laboratorium, lalu Farmasi membangun versinya sendiri, dan
> Radiologi membangun versinya sendiri lagi, maka dr. Rina harus memeriksa tiga tempat berbeda
> untuk tahu apakah ada hal mendesak. Untuk pemberitahuan biasa itu merepotkan; untuk nilai
> kritis itu berbahaya.

**Akibat pada pelaksanaan.** Karena kemampuan ini milik platform, pembangunannya memerlukan
kesepakatan dengan pemilik platform sebelum masuk roadmap Laboratorium. Dicatat sebagai
`LAB-COORD-001`.

### BR-13 — Hasil lab terdaftar sebagai dokumen klinis di rekam medis (`LAB-DEC-017`)

**Latar belakang.** Modul `rekam-medis` tidak menyimpan isi dokumen klinis. Ia mencatat
**keutuhan** dokumen milik modul lain lewat `Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalDocumentIntegrity.cs@9124900`,
yang menunjuk dokumen memakai pasangan `DocumentKind` dan `DocumentId`. Daftar jenis dokumen
pada `ClinicalDocumentKind@9124900` berisi 13 nilai — `ProgressNote`, `Consultation`,
`Assessment`, `Diagnosis`, `Procedure`, `VitalSign`, `Allergy`, `MedicalHistory`,
`FamilyHistory`, `ClinicalDocument`, `NoteAttachment`, `MedicalCertificate`, dan `Consent` —
dan **tidak ada** nilai untuk hasil laboratorium.

**Aturan:**

1. Isi hasil pemeriksaan **tetap disimpan di tabel Laboratorium**. Tidak ada penggandaan angka
   hasil ke tabel rekam medis.
2. Setiap hasil yang dirilis **didaftarkan** ke rekam medis sebagai jenis dokumen klinis baru,
   sehingga ikut memiliki catatan penulis, waktu penandatanganan, dan status penguncian.
3. Rekam medis menjadi tempat menelusuri "dokumen apa saja yang dimiliki pasien ini", termasuk
   hasil laboratorium.

**Contoh:**

> Hasil Hemoglobin pasien Andi dirilis pukul 10.00 oleh Tono. Angka 9,4 g/dL tersimpan di tabel
> hasil milik Laboratorium. Bersamaan dengan itu, rekam medis mencatat satu baris keutuhan:
> jenis dokumen "hasil laboratorium", penunjuk ke hasil tersebut, penulis Tono, ditandatangani
> pukul 10.00. Ketika petugas rekam medis menelusuri berkas pasien Andi, hasil lab itu ikut
> terlihat sebagai bagian berkasnya, tanpa angka 9,4 pernah disalin ke mana pun.

**Titik yang harus dicocokkan dan belum diputuskan.** Rekam medis mengunci dokumen ketika
kunjungan pasien ditutup, lihat `ClinicalDocumentLockTrigger.EncounterClosed@9124900`.
Sementara `LAB-DEC-007` mengizinkan koreksi hasil setelah dirilis. Kedua aturan ini bertemu
ketika hasil perlu dikoreksi **setelah** kunjungan pasien ditutup. Perilaku yang benar untuk
keadaan itu belum diputuskan dan dicatat sebagai `LAB-OPEN-011`.

**Akibat pada pelaksanaan.** Daftar `ClinicalDocumentKind` adalah milik modul `rekam-medis`.
Penambahan nilai baru memerlukan kesepakatan dengan pemiliknya. Dicatat sebagai `LAB-COORD-002`.

### BR-14 — Batas nilai menjadi tabel tersendiri milik Laboratorium (`LAB-DEC-018`)

**Latar belakang.** Katalog pemeriksaan laboratorium menumpang
`Areas/HealthServices/MasterData/Models/MstProcedure.cs@9124900` lewat penanda `IsLaboratory`.
Tabel itu dipakai bersama seluruh tindakan rumah sakit, termasuk bedah, terapi, dan radiologi.

**Aturan:**

1. Batas nilai disimpan pada **tabel tersendiri milik modul Laboratorium**, bukan sebagai kolom
   tambahan pada `MstProcedure`.
2. Setiap baris batas nilai menunjuk ke satu jenis pemeriksaan di `MstProcedure`.
3. **Satu jenis pemeriksaan boleh memiliki lebih dari satu baris batas**, dibedakan menurut
   jenis kelamin dan kelompok umur.
4. Isi setiap baris sekurang-kurangnya: penunjuk ke jenis pemeriksaan, satuan hasil, batas
   normal bawah dan atas, batas kritis bawah dan atas, batas waktu penyelesaian cito, pembatas
   jenis kelamin, dan pembatas kelompok umur.
5. Kepala instalasi laboratorium dapat mengubah isinya lewat layar pengelolaan, tanpa
   menerbitkan versi aplikasi baru.

**Contoh kenapa bentuk tabel terpisah yang dipilih:**

| Pemeriksaan | Jenis kelamin | Kelompok umur | Normal bawah | Normal atas | Kritis bawah | Kritis atas |
|---|---|---|---:|---:|---:|---:|
| Hemoglobin | Pria | Dewasa | 13,0 | 17,0 | 7,0 | 20,0 |
| Hemoglobin | Wanita | Dewasa | 12,0 | 15,0 | 7,0 | 20,0 |
| Hemoglobin | Semua | Anak | 11,0 | 14,0 | 6,0 | 18,0 |
| Kalium | Semua | Semua | 3,5 | 5,1 | 2,5 | 6,0 |

> Perhatikan Hemoglobin punya **tiga baris**. Bila batas nilai ditaruh sebagai kolom pada
> `MstProcedure`, Hemoglobin hanya punya satu baris sehingga ketiga batas itu tidak mungkin
> disimpan sekaligus. Itulah alasan pokok bentuk tabel terpisah dipilih, di luar soal menjaga
> `MstProcedure` tetap bersih.

**Batas kepemilikan.** Tabel batas nilai milik Laboratorium. `MstProcedure` tetap milik
`master-data` dan **tidak diubah**. Laboratorium hanya menunjuk ke sana.

### BR-15 — Pengelolaan alasan penolakan sampel dengan dua tingkat kewenangan (`LAB-DEC-019`)

**Latar belakang.** Tabel `Areas/HealthServices/LaboratoryManagement/Models/MstLabRejectionReason.cs@9124900`
sudah ada dan dipakai, tetapi hanya punya endpoint baca
(`Controllers/LabSpecimenController.cs#GetRejectionReasons@9124900`). Tidak ada layar
pengelolaan dan tidak ditemukan pengisian data awal.

Yang membuat tabel ini tidak sesederhana daftar biasa: kolom `IsInternalHospitalError@9124900`
menentukan apakah pengambilan ulang ditanggung rumah sakit atau boleh dibebankan kepada pasien,
sesuai `LAB-INH-011`.

**Aturan:**

| Kolom | Boleh diubah kepala instalasi lab | Alasan |
|---|:---:|---|
| Kode alasan | Ya, saat membuat baru | Penanda teknis, tidak berdampak biaya |
| Nama alasan | Ya | Sekadar penamaan |
| Keterangan | Ya | Sekadar penjelasan |
| Urutan tampil | Ya | Kenyamanan pemakaian |
| Aktif atau tidak | Ya | Alasan yang tidak dipakai boleh disembunyikan |
| **Penanda kesalahan internal rumah sakit** | **Tidak** | Menentukan siapa menanggung biaya. Menurut `LAB-INH-010`, akibat finansial bukan wewenang Laboratorium |
| **Penanda wajib disertai catatan** | **Tidak** | Menentukan kelengkapan bukti saat penolakan; melemahkannya berarti melemahkan jejak audit |

Dua kolom terakhir hanya dapat disetel admin sistem.

**Contoh:**

> Kepala instalasi Pak Hendra menemukan alasan penolakan baru yang sering terjadi: "Sampel
> tidak diberi label". Ia menambahkannya sendiri lewat layar pengelolaan, memberi nama dan
> urutan tampil. Tetapi kolom "kesalahan internal rumah sakit" pada alasan itu tampil terkunci
> dan bertanda gembok — pengisiannya harus lewat admin sistem, karena jawabannya menentukan
> apakah pengambilan darah ulang gratis bagi pasien atau tidak.

**Yang tetap harus disiapkan.** Karena tidak ditemukan pengisian data awal, daftar alasan
penolakan harus terisi sebelum modul dipakai. Bila kosong, petugas tidak bisa menolak sampel
sama sekali. Kebutuhan data awal ini dicatat sebagai bagian pekerjaan Rilis 1.

### BR-16 — Koreksi hasil setelah kunjungan ditutup memakai addendum (`LAB-DEC-020`)

**Latar belakang pertemuan dua aturan.** `LAB-DEC-017` mendaftarkan hasil lab ke rekam medis.
Rekam medis mengunci dokumen ketika kunjungan pasien ditutup, lihat
`ClinicalDocumentLockTrigger.EncounterClosed@9124900`. Sementara `LAB-DEC-007` mengizinkan
koreksi hasil kapan pun. Ketiganya bertemu ketika hasil ketahuan salah setelah kunjungan
ditutup.

**Kemampuan yang sudah ada dan dipakai ulang.** Modul rekam medis sudah punya mekanisme
koreksi untuk dokumen terkunci:
`Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalNoteAddendum.cs@9124900`,
memuat `CorrectionReason`, `AuthorUserId`, `SignedAt`, `Sequence`, dan menempel pada
`IntegrityId`. Mekanisme ini berlaku untuk **semua** jenis dokumen, bukan hanya catatan
perkembangan — dibuktikan oleh
`Controllers/ClinicalNoteAddendumController.cs@9124900` yang menerima `ClinicalDocumentKind`
sebagai parameter.

**Aturan:**

1. Dokumen hasil yang asli **tetap terkunci dan tidak diubah isinya**.
2. Hasil perbaikan didaftarkan sebagai **addendum** pada dokumen asli, wajib menyebutkan alasan
   koreksi dan ditandatangani petugas yang berwenang.
3. Tidak ada pembukaan kunci dokumen. Tidak ada kemampuan baru yang perlu ditambahkan ke modul
   rekam medis.
4. Aturan ini melengkapi, bukan menggantikan, `LAB-DEC-007`: di dalam modul Laboratorium hasil
   tetap berjalan lewat `Corrected/Amended → Revalidated → Released`, dan dokter pemesan tetap
   otomatis diberi tahu.

**Contoh:**

> Hasil Hemoglobin pasien Andi dirilis 9,4 g/dL pada 1 September. Kunjungan Andi ditutup pada
> 2 September, sehingga dokumen hasil terkunci. Pada 4 September ketahuan angka yang benar
> adalah 4,9 g/dL. Tono, yang berwenang validasi, membuat koreksi.
>
> Yang terjadi: dokumen hasil 1 September tetap ada apa adanya, terkunci, memuat 9,4. Di
> atasnya menempel satu addendum bertanda tangan Tono tertanggal 4 September, beralasan
> "tertukar dengan sampel pasien lain", memuat angka 4,9. dr. Rina menerima pemberitahuan
> otomatis bahwa hasil pasiennya berubah. Siapa pun yang membuka berkas Andi melihat keduanya
> beserta urutan waktunya.

**Kenapa pilihan ini yang terbaik.** Tidak ada satu pun kemampuan baru yang perlu dibangun,
janji "terkunci berarti terkunci" tetap utuh, dan bentuknya persis sesuai `LAB-INH-003` yang
menyebut jalur koreksi harus mempertahankan riwayat rilis lama.

### BR-17 — Hasil punya dua bentuk: angka dan pilihan terbatas (`LAB-DEC-021`)

**Latar belakang gap `DEC-LAB-002`.** Gerbang kelengkapan requirement menemukan bahwa seluruh
tabel batas nilai pada BR-04 berbentuk angka — satuan, batas bawah, batas atas. Padahal
`LAB-DEC-002` membatasi modul pada Patologi Klinik, dan Patologi Klinik **tidak seluruhnya
berupa angka**.

**Aturan:**

1. Setiap jenis pemeriksaan ditetapkan **bentuk hasilnya** sejak awal, tepat satu dari dua:
   **hasil angka** atau **hasil pilihan terbatas**.
2. Pemeriksaan berhasil angka memakai batas normal bawah dan atas serta batas kritis bawah dan
   atas, persis seperti BR-04.
3. Pemeriksaan berhasil pilihan menyimpan **daftar pilihan yang sah**, beserta penanda pilihan
   mana yang dianggap **di luar rujukan** dan mana yang dianggap **kritis**.
4. Analis tidak boleh mengetik bebas pada pemeriksaan berhasil pilihan. Ia hanya memilih dari
   daftar yang sah.
5. `LAB-DEC-004` tentang nilai kritis berlaku untuk **kedua bentuk**, bukan hanya bentuk angka.

**Contoh bentuk angka:**

| Pemeriksaan | Satuan | Normal | Kritis bawah | Kritis atas |
|---|---|---|---:|---:|
| Kalium | mmol/L | 3,5 – 5,1 | 2,5 | 6,0 |

**Contoh bentuk pilihan terbatas:**

| Pemeriksaan | Pilihan sah | Di luar rujukan | Kritis |
|---|---|---|---|
| Protein urin | Negatif, +1, +2, +3, +4 | +1, +2 | +3, +4 |
| Glukosa urin | Negatif, +1, +2, +3 | +1 | +2, +3 |
| Tes kehamilan | Positif, Negatif | — | — |
| Golongan darah | A, B, AB, O | — | — |

**Contoh penerapan:**

> Protein urin pasien Andi keluar +4. Analis memilih "+4" dari daftar, bukan mengetiknya.
> Sistem mencocokkan pilihan itu dengan daftar kritis, menemukan +4 termasuk kritis, lalu
> memunculkan formulir pelaporan BR-02 persis seperti pada Kalium 7,2 mmol/L.
>
> Bandingkan bila hasil diketik bebas: analis pertama menulis "+4", analis kedua menulis
> "Positif kuat (4+)", analis ketiga menulis "protein +4". Sistem tidak akan bisa mengenali
> ketiganya sebagai hal yang sama, sehingga nilai kritis tidak pernah terdeteksi.

**Catatan untuk golongan darah dan tes kehamilan.** Keduanya berbentuk pilihan tetapi tidak
punya nilai kritis — tidak ada golongan darah yang "berbahaya". Kolom kritisnya dibiarkan
kosong, dan itu sah.

### BR-18 — Kewenangan validasi dan rilis diberikan per orang (`LAB-DEC-022`)

**Latar belakang gap `DEC-LAB-001`.** `LAB-INH-007` menyatakan validasi dan rilis memakai
kewenangan berbeda, dan jabatan organisasi tidak otomatis memberi kewenangan. `BR-01`
menyatakan pengisi hasil tidak boleh memvalidasi hasil yang sama. Keduanya mengatur **hubungan
antar kewenangan**, tetapi tidak satu pun menyebut **siapa yang memegangnya**.

**Aturan:**

1. Kewenangan **validasi** dan kewenangan **rilis** tetap dua hal terpisah, sesuai
   `LAB-INH-007`.
2. Keduanya diberikan **kepada orang per orang**, bukan melekat pada jabatan. Seorang analis
   senior boleh memegangnya, seorang kepala ruangan boleh tidak.
3. Rumah sakit menjamin **setiap shift memiliki sekurang-kurangnya dua orang pemegang
   kewenangan validasi**.
4. Sistem menampilkan peringatan kepada kepala instalasi bila suatu shift hanya memiliki satu
   pemegang kewenangan validasi, karena pada shift itu prinsip empat mata pasti akan gagal.
5. Sistem tidak menetapkan siapa yang berwenang. Ia hanya menegakkan aturan atas penetapan yang
   dibuat rumah sakit.

**Kenapa butir 3 dan 4 penting.**

> Bila sebuah shift hanya punya satu pemegang kewenangan validasi, maka setiap hasil yang ia
> kerjakan sendiri akan lewat jalur pengecualian `BR-01`. Dalam sebulan, "pengecualian" itu
> berubah menjadi kebiasaan, dan prinsip empat mata berhenti berarti apa pun — padahal
> pengujiannya tetap lulus dan tidak ada aturan yang dilanggar.
>
> Peringatan pada butir 4 membuat keadaan itu terlihat sebelum menjadi kebiasaan.

**Contoh penerapan:**

> Shift malam Sabtu dijadwalkan berisi analis Sari dan analis Budi. Keduanya memegang
> kewenangan validasi, Budi juga memegang kewenangan rilis. Sari mengerjakan Kalium pasien
> Andi dan mengisi hasilnya. Budi memvalidasi — `BR-01` terpenuhi karena Budi bukan Sari — lalu
> merilisnya.
>
> Bila Budi mendadak berhalangan dan Sari bertugas sendirian, kepala instalasi mendapat
> peringatan bahwa shift itu hanya punya satu pemegang kewenangan validasi. Sari tetap dapat
> bekerja lewat jalur pengecualian `BR-01`, tetapi keadaannya sudah diketahui, bukan
> tersembunyi.

**Yang tetap milik rumah sakit, bukan sistem.** Penetapan siapa saja yang layak memegang
kewenangan validasi adalah keputusan kepegawaian dan kompetensi. Sistem tidak ikut menilainya.

### BR-19 — Batas kritis lebih terlindungi daripada batas normal (`LAB-DEC-023`)

**Latar belakang gap `DEC-LAB-003`.** `LAB-DEC-019` mengunci kolom penanda kesalahan internal
pada tabel alasan penolakan agar hanya dapat disetel admin sistem, karena kolom itu menentukan
siapa menanggung biaya. Sementara `LAB-DEC-018` membiarkan kepala instalasi mengubah seluruh
isi tabel batas nilai dengan bebas — termasuk batas kritis, yang menentukan kapan seorang
pasien dinyatakan dalam bahaya. Perlindungan atas angka keselamatan justru lebih longgar
daripada perlindungan atas angka biaya.

**Aturan:**

| Yang diubah | Siapa yang boleh | Perlu persetujuan | Riwayat disimpan |
|---|---|:---:|:---:|
| Satuan hasil | Kepala instalasi | Tidak | Ya |
| Batas normal bawah dan atas | Kepala instalasi | Tidak | Ya |
| Daftar pilihan sah dan penanda di luar rujukan | Kepala instalasi | Tidak | Ya |
| **Batas kritis bawah dan atas** | Kepala instalasi mengajukan | **Ya, persetujuan klinis** | Ya |
| **Penanda pilihan yang dianggap kritis** | Kepala instalasi mengajukan | **Ya, persetujuan klinis** | Ya |
| Batas waktu penyelesaian cito | Kepala instalasi | Tidak | Ya |

Riwayat perubahan menyimpan sekurang-kurangnya: kolom apa yang berubah, nilai lama, nilai baru,
siapa yang mengubah atau mengajukan, siapa yang menyetujui bila diperlukan, waktu, dan alasan.

**Kenapa batas normal dibedakan dari batas kritis.**

> Batas normal memang wajar berubah. Ketika laboratorium mengganti alat atau metode
> pemeriksaan, rentang normal bisa bergeser sedikit, dan itu penyesuaian teknis biasa yang
> memang menjadi keahlian kepala instalasi.
>
> Batas kritis berbeda sifatnya. Ia bukan soal metode, melainkan soal pada angka berapa seorang
> pasien dianggap terancam. Itu penilaian klinis, bukan penilaian teknis laboratorium.

**Contoh yang dicegah aturan ini:**

> Kepala instalasi merasa terlalu banyak peringatan nilai kritis mengganggu pekerjaan harian,
> lalu menaikkan batas kritis atas Kalium dari 6,0 menjadi 8,0. Sejak saat itu pasien dengan
> Kalium 7,2 mmol/L tidak lagi memicu kewajiban pelaporan `BR-02`. Tidak ada aturan yang
> dilanggar dan tidak ada yang menyadarinya.
>
> Dengan `BR-19`, perubahan itu berhenti sebagai pengajuan sampai pihak klinis menyetujuinya,
> dan seluruh jejaknya tersimpan.

**Hubungan dengan keputusan sebelumnya.** Aturan ini **mempersempit** `LAB-DEC-018`, tidak
membatalkannya. Janji "kepala instalasi dapat mengubah tanpa menerbitkan versi aplikasi baru"
tetap berlaku untuk seluruh kolom kecuali dua kolom keselamatan di atas.

### BR-20 — Wadah fisik dipisahkan dari pemeriksaan terpesan (`LAB-DEC-024`)

**Latar belakang gap `DEC-LAB-008`.** Arsitektur domain menemukan bahwa model yang berjalan
menyatukan dua hal yang berbeda. Satu baris sampel membawa tepat satu jenis pemeriksaan, satu
barcode, satu keputusan layak atau tolak, dan satu baris tagihan — bukti pada
`TrxLabSpecimen.ProcedureId@9124900`.

Selama tiap pemeriksaan memang memakai wadah berbeda, model itu tampak benar. Masalahnya muncul
ketika dua pemeriksaan berbagi satu wadah yang sama, misalnya fungsi hati dan fungsi ginjal yang
keduanya diperiksa dari satu tabung serum hasil sekali tusuk.

**Aturan:**

1. **Wadah Fisik** menjadi konsep tersendiri. Satu wadah berarti satu tabung atau satu pot
   nyata: satu barcode, satu peristiwa pengambilan, dan **satu** keputusan layak atau tolak.
2. **Pemeriksaan Terpesan** menjadi konsep tersendiri. Satu pemeriksaan berarti satu jenis
   pemeriksaan yang diminta: satu tarif, satu baris tagihan, dan kelak satu hasil.
3. **Satu wadah dapat melayani beberapa pemeriksaan.** Satu pemeriksaan ditopang tepat satu
   wadah.
4. Keputusan layak atau tolak diambil atas **wadah**, dan berlaku serentak bagi seluruh
   pemeriksaan yang ditopangnya. Menolak sebagian tidak lagi mungkin.
5. Kelayakan tagih tetap terbit **per pemeriksaan**, dipicu oleh dinyatakan layaknya wadah yang
   menopangnya.
6. Ambil ulang menciptakan **wadah** baru, dan seluruh pemeriksaan yang ditopangnya ikut
   berpindah ke wadah baru itu. Wadah lama tetap terlihat beserta tautan sebabnya.

**Contoh:**

> dr. Rina memesan Fungsi hati Rp150.000 dan Fungsi ginjal Rp120.000 untuk pasien Andi.
> Keduanya diperiksa dari satu tabung serum. Perawat Dewi menusuk sekali, mengisi satu tabung,
> menempel **satu** barcode.
>
> Petugas Budi memeriksa tabung itu dan menyatakannya layak. Pada saat itu terbit **dua**
> kejadian kelayakan tagih: Fungsi hati Rp150.000 dan Fungsi ginjal Rp120.000. Satu wadah, dua
> tagihan — dan itu memang benar, karena pasien memang menjalani dua pemeriksaan.
>
> Bandingkan bila tabung itu ternyata keruh dan Budi menolaknya. **Kedua** pemeriksaan gugur
> serentak, karena memang tidak ada bahan yang bisa dikerjakan. Model lama mengizinkan Budi
> menolak Fungsi hati sambil menerima Fungsi ginjal — sesuatu yang tidak mungkin terjadi di
> meja kerja.

**Kenapa diputuskan sekarang.** Hasil pemeriksaan melekat pada **pemeriksaan**, bukan pada
wadah. Slice hasil belum ditulis sebaris pun. Memutuskan pemisahan ini setelah tabel hasil
terbentuk dan terisi angka pasien jauh lebih mahal dan lebih berisiko.

**Kesesuaian dengan keputusan terkunci.** Pemisahan ini **tidak** melanggar `LAB-INH-005`
(satu pesanan boleh punya banyak sampel — tetap berlaku), `LAB-INH-009` (titik kelayakan tagih
tetap pada dinyatakan layak), maupun `LAB-INH-010` (Billing tetap satu-satunya pemilik akibat
finansial). Yang berubah adalah **satuan** tempat kelayakan tagih menempel, bukan aturannya.

**Yang wajib diperiksa sebelum dikerjakan.** Perubahan ini menyentuh struktur data yang sudah
berjalan. Sebelum dikerjakan, wajib dipastikan berapa banyak data laboratorium yang benar-benar
sudah terisi di basis data produksi. Bukti `01-existing-capability-map.md#CAP-21` menunjukkan
frontend Laboratorium masih nol, sehingga kemungkinan besar belum ada data pasien sungguhan —
tetapi itu **dugaan, bukan bukti**. Dicatat sebagai `LAB-OPEN-012`.

---

### BR-21 — Cakupan diperluas menjadi tiga disiplin (`LAB-DEC-025`)

**Menggantikan `LAB-DEC-002`.**

**Latar belakang.** `LAB-DEC-002` membatasi modul pada Patologi Klinik. Analisis konsolidasi
bukti lapangan menunjukkan laboratorium rumah sakit ini menjalankan **tiga disiplin sejajar**,
masing-masing dengan daftar pasien, alur hasil, dan laporan tersendiri.

**Aturan:**

| Disiplin | Status | Bukti |
|---|---|---|
| Patologi Klinik | **Di dalam scope** | Monitoring pada aplikasi baru; alur hasil pada workstation HCLAB |
| Patologi Anatomi | **Di dalam scope** | Daftar pasien tersendiri; nomor PA/Sitologi/FNAB; makroskopik, mikroskopik, kesimpulan |
| Mikrobiologi | **Di dalam scope** | Daftar pasien tersendiri; organisme, sensitivitas antibiotik, laporan R/I/S |
| **Bank Darah** | **Tetap di luar scope** | Analisis konsolidasi memisahkannya secara eksplisit. Kemunculannya sebagai pilihan workstation HCLAB **tidak** memasukkannya ke scope |

**Akibat.** Ketiga disiplin berbagi konsep yang sama untuk pesanan, wadah, dan kelayakan tagih,
tetapi **berbeda pada bentuk hasilnya**. Perbedaan itu diatur BR-23.

**Contoh perbedaan yang harus ditampung:**

> Satu pasien menjalani Hemoglobin (Patologi Klinik), kultur darah (Mikrobiologi), dan biopsi
> kulit (Patologi Anatomi). Ketiganya berangkat dari pesanan dan wadah yang bentuknya sama,
> tetapi hasilnya berbeda total: angka bersatuan, daftar bakteri beserta kepekaan antibiotik,
> dan uraian naratif beserta gambar.

### BR-22 — Cito dan Duplo melekat pada pemeriksaan, bukan pesanan (`LAB-DEC-026`)

**Mengubah `LAB-DEC-013` dan BR-09.** Aturan cito tetap berlaku; yang berubah adalah **letaknya**.

**Latar belakang.** BR-09 menaruh penanda kesegeraan pada pesanan. Bukti lapangan menunjukkan
Cito adalah kolom **per baris pemeriksaan**, sejajar dengan harga dan subtotal, dan muncul pula
pada form hasil. Duplo mengikuti pola yang sama.

**Aturan:**

1. Penanda **Cito** melekat pada **pemeriksaan terpesan**, bukan pada pesanan.
2. Penanda **Duplo** juga melekat pada pemeriksaan terpesan.
3. Satu pesanan boleh memuat pemeriksaan cito dan pemeriksaan biasa sekaligus.
4. Batas waktu penyelesaian cito tetap disimpan pada tabel batas nilai per jenis pemeriksaan,
   sesuai `LAB-DEC-013`.
5. Daftar kerja mendahulukan **pemeriksaan** bertanda cito, bukan seluruh isi pesanannya.

**Contoh:**

> dr. Rina memesan Kalium cito bersama Kolesterol rutin dalam satu pesanan. Dengan aturan lama,
> seluruh pesanan menjadi cito sehingga Kolesterol ikut menyita antrean prioritas. Dengan
> BR-22, hanya Kalium yang naik ke urutan atas daftar kerja; Kolesterol tetap di antrean biasa.

**Yang belum diputuskan:** apakah penanda Cito dan Duplo berdampak pada tarif. Bukti
menempatkan keduanya pada baris yang sama dengan harga, tetapi dampaknya tidak diperagakan.
Dicatat sebagai `LAB-OPEN-013`.

### BR-23 — Hasil punya empat bentuk (`LAB-DEC-027`)

**Menggantikan `LAB-DEC-021`.** Dua bentuk yang sudah diputuskan tetap berlaku; dua bentuk
ditambahkan.

| Bentuk | Dipakai oleh | Isi | Dapat dinilai kritis otomatis |
|---|---|---|:---:|
| **Angka bersatuan** | Patologi Klinik | Hasil, satuan, batas normal, batas kritis, penanda rendah/tinggi | **Ya** |
| **Pilihan terbatas** | Patologi Klinik, Mikrobiologi | Daftar pilihan sah beserta penanda di luar rujukan dan penanda kritis | **Ya** |
| **Mikrobiologi berstruktur** | Mikrobiologi | Penanda definitif, status Normal/Positif/Negatif, organisme per bakteri, antibiotik, kadar, zona dalam mm, dan hasil `R`/`I`/`S` | **Tidak** — lihat catatan |
| **Narasi Patologi Anatomi** | Patologi Anatomi | Makroskopik, mikroskopik, dan kesimpulan — ketiganya wajib — beserta gambar contoh | **Tidak** — lihat catatan |

**Arti kode kepekaan antibiotik**, sesuai bukti lapangan:

| Kode | Arti |
|---|---|
| `R` | *Resistent* — bakteri kebal terhadap antibiotik itu |
| `I` | *Intermediate* — kepekaan berada di antara, perlu pertimbangan dosis |
| `S` | *Sensitive* — bakteri peka, antibiotik itu diperkirakan bekerja |

**Aturan turunan:**

1. Setiap hasil bakteri disimpan sebagai baris tersendiri yang dapat ditambah dan dikurangi.
2. Gambar pada hasil Patologi Anatomi dibatasi ukurannya; batas yang terlihat pada bukti adalah
   2 MB.
3. Makroskopik, mikroskopik, dan kesimpulan **wajib** terisi untuk Patologi Anatomi.

**Catatan penting tentang penilaian kritis.** Bentuk ketiga dan keempat **tidak dapat** dinilai
kritis dengan mekanisme batas nilai. Bakteri resisten dan kesimpulan patologi yang mengkhawatirkan
adalah penilaian klinis, bukan perbandingan angka. Bagaimana keduanya masuk alur nilai kritis
**belum diputuskan** dan dicatat sebagai `LAB-OPEN-014`.

### BR-24 — Laboratorium memiliki jalur pendaftaran pasien sendiri (`LAB-DEC-028`)

**Mengubah batas scope yang sebelumnya menyerahkan seluruh pendaftaran ke Registrasi.**

**Latar belakang.** Blueprint mengasumsikan setiap pesanan menempel pada kunjungan yang sudah
dibuat modul Registrasi. Bukti lapangan menunjukkan laboratorium menerima **pasien datang
langsung** dan **pasien rujukan dari luar** yang belum punya kunjungan sama sekali.

**Aturan:**

| Jalur | Asal pasien | Yang dilakukan Laboratorium |
|---|---|---|
| Kunjungan yang sudah ada | Rawat Jalan, Rawat Inap, IGD | Menempel pada kunjungan itu, seperti rancangan semula |
| **Pasien datang langsung** | Datang sendiri ke laboratorium | Laboratorium mendaftarkan pasien dan membuat konteks kunjungannya |
| **Pasien rujukan luar** | Dikirim dokter atau institusi lain | Laboratorium mendaftarkan pasien beserta data perujuknya |

**Data rujukan yang wajib ditampung:** dokter perujuk, instansi atau rumah sakit perujuk,
kontak instansi, surat rujukan, dan diagnosis awal.

**Batas yang tetap dipegang.** Identitas pasien tetap **milik** modul Patient Management, dan
kunjungan tetap **milik** modul Registrasi. Laboratorium **tidak** membuat salinan pasien.
Bagaimana tepatnya Laboratorium membuat kunjungan tanpa mengambil alih kepemilikannya
**belum diputuskan** dan dicatat sebagai `LAB-OPEN-015`.

### BR-28 — Laboratorium meminta Registrasi membuat kunjungan (`LAB-DEC-032`)

**Menutup `LAB-OPEN-015`.** Melengkapi BR-24, tidak menggantikannya.

**Bukti yang menentukan.** Modul Registrasi **sudah memiliki** seluruh yang dibutuhkan pada
`9124900`:

| Yang sudah ada | Lokasi |
|---|---|
| Nilai `WalkIn` pada sumber pendaftaran | `EncounterRegistrationSource.WalkIn = 5` |
| Penanda pasien datang langsung | `TrxPatientEncounter.IsWalkIn` |
| Penanda dan nomor rujukan | `TrxPatientEncounter.IsReferral`, `ReferralNumber`, `IsReferralRequired`, `IsReferralVerified` |
| Pembuatan kunjungan datang langsung | `PatientEncounterController@9124900` |

**Aturan:**

1. **Layar pendaftaran tetap milik Laboratorium.** Petugas lab tidak berpindah aplikasi untuk
   menerima pasien datang langsung atau pasien rujukan luar.
2. Saat pendaftaran disimpan, Laboratorium **memanggil Registrasi** dengan mengirim identitas
   pasien, penanda datang langsung, dan data rujukan.
3. Registrasi yang **membuat** kunjungan, menjalankan aturannya sendiri, lalu mengembalikan
   penunjuk kunjungan yang baru dibuat.
4. Laboratorium menyimpan penunjuk itu pada pesanan, persis seperti pesanan dari poliklinik.
5. Laboratorium **tidak menulis** satu baris pun ke tabel milik Registrasi maupun Patient
   Management.

**Akibat bagi invariant.** `INV-01` — setiap pesanan terikat pada tepat satu kunjungan yang
sudah ada — **tetap utuh**. Tidak ada pengecualian yang dibuat untuk pasien datang langsung.

**Contoh:**

> Ibu Sari datang sendiri ke laboratorium membawa surat rujukan dari klinik luar, tanpa pernah
> mendaftar di loket. Petugas lab membuka layar pendaftaran di aplikasi Laboratorium, mengisi
> identitas Ibu Sari beserta nama klinik dan dokter perujuknya, lalu menyimpan.
>
> Di balik layar, Laboratorium meminta Registrasi membuat kunjungan bertanda datang langsung
> dan bertanda rujukan. Registrasi menjalankan aturannya — penomoran kunjungan, pemeriksaan
> kelengkapan — lalu mengembalikan penunjuk kunjungan. Pesanan lab Ibu Sari menempel pada
> kunjungan itu, sama seperti pesanan pasien poliklinik.
>
> Ketika kelak hasilnya perlu ditelusuri, atau Billing perlu menagihkannya, konteks kunjungan
> sudah ada dan tidak ada yang berbeda.

**Akibat pada pelaksanaan.** Kontrak pemanggilan antarmodul memerlukan kesepakatan dengan
pemilik `registration-management`. Dicatat sebagai `LAB-COORD-003`.

### BR-30 — Penempatan data induk mengikuti cakupan pemakaiannya (`LAB-DEC-034`)

> **Aturan ini berlaku untuk backend saja.** Frontend **tidak mengikutinya** — menu data induk
> di frontend tetap memakai konvensi yang sudah berjalan, yaitu seluruhnya berada di
> `health-services/master-data/`. Rinciannya ada pada `03-frontend-architecture.md` bagian 2.1.

| Cakupan data induk | Letaknya di backend |
|---|---|
| **Khusus Laboratorium** — hanya dipakai modul ini | `Areas/HealthServices/LaboratoryManagement/Models/` |
| **Global** — dipakai lebih dari satu modul | `Areas/HealthServices/MasterData/Models/` |

**Penerapan pada modul ini:**

| Data induk | Cakupan | Letaknya |
|---|---|---|
| `MstLabRejectionReason` | Khusus Laboratorium | `LaboratoryManagement/Models/` — **sudah benar**, tetap di sana |
| `MstLabValueBound` | Khusus Laboratorium | `LaboratoryManagement/Models/` |
| `MstLabValueOption` | Khusus Laboratorium | `LaboratoryManagement/Models/` |
| `MstProcedure` | Global — dipakai seluruh layanan | `MasterData/Models/` — **tidak disentuh** |
| `MstTariff`, `MstInsuranceTariff` | Global | `MasterData/Models/` — **tidak disentuh** |
| `MstAgeCategory` | Global | `MasterData/Models/` — **tidak disentuh** |

**Bukti bahwa aturan ini memang pola yang berlaku.** Pada `9124900` terdapat **20 data induk
khusus modul** yang sudah berada di folder modulnya masing-masing:

| Modul | Contoh |
|---|---|
| HR Service Management | `MstEmployeeDocumentType`, `MstHrServiceCategory`, `MstHrServiceType` |
| Lifecycle Management | `MstOnboardingTemplate`, `MstOffboardingTemplate` |
| Recruitment Management | `MstCandidateStatus`, `MstInterviewTemplate`, `MstRecruitmentStage` |
| Workforce Planning | `MstStaffingRatio`, `MstStaffingStandard`, `MstWorkforceRequirement` |
| Pharmacy Management | `MstPrescriptionReviewCriterion`, `MstPrescriptionTemplate` |
| **Laboratory Management** | `MstLabRejectionReason` |

Sementara `Areas/HealthServices/MasterData/Models/` berisi 61 data induk yang memang dipakai
lintas modul.

**Koreksi terhadap dokumen aturan.** `backend-structure-rules.md` menyatakan seluruh data induk
berada di `Areas/HealthServices/MasterData/Models/`, dengan contoh `MstEmergencyTriageLevel`.
Dua hal keliru pada pernyataan itu:

1. `MstEmergencyTriageLevel` **tidak ditemukan** di source pada `9124900`.
2. Pola nyata yang berlaku adalah pemisahan menurut cakupan, bukan penyeragaman ke satu folder.

Karena itu penempatan `MstLabRejectionReason` di folder Laboratorium **bukan utang teknis**.
Catatan utang teknis pada `02-backend-architecture.md` revision 1 dicabut.

**Cara menilai cakupan sebuah data induk:**

> Pertanyaannya sederhana: *apakah modul selain Laboratorium akan pernah membacanya?*
>
> Alasan penolakan sampel — hanya Laboratorium. Batas nilai pemeriksaan — hanya Laboratorium.
> Keduanya khusus.
>
> Jenis tindakan, tarif, kategori umur — dibaca Rawat Jalan, Rawat Inap, IGD, Farmasi, dan
> Billing. Ketiganya global, dan Laboratorium **tidak boleh** memindahkannya.

### BR-31 — Sumber rujukan menjadi data induk global (`LAB-DEC-035`)

**Menutup `DEC-LAB-009`.** Membuka slice `S13b` pendaftaran pasien rujukan luar.

**Latar belakang.** `TrxPatientEncounter@9124900` hanya menyimpan **penanda dan nomor**
rujukan — `IsReferral`, `ReferralNumber`, `IsReferralRequired`, `IsReferralVerified`. Tidak ada
nama dokter perujuk, nama instansi, alamat, maupun telepon. Tidak ada pula data induk instansi
perujuk; `MstHospitalSite` adalah lokasi milik rumah sakit ini sendiri, bukan institusi luar.

**Aturan:**

1. **Instansi perujuk** menjadi data induk **global** di bawah Master Data: nama klinik atau
   rumah sakit, alamat, telepon, dan penanda aktif.
2. **Dokter perujuk** juga menjadi data induk global, tertaut ke instansinya.
3. **Kunjungan menunjuk** ke keduanya. Nama tidak disimpan sebagai teks bebas pada kunjungan.
4. Laboratorium **tidak memiliki** dan **tidak menyalin** keduanya. Ia hanya memilih dari daftar
   saat mendaftarkan pasien rujukan.

**Kenapa global, bukan khusus Laboratorium.** Menurut `LAB-DEC-034`, penempatan mengikuti
cakupan pemakaian. Rujukan **bukan** hal khusus laboratorium:

| Bukti | Isi |
|---|---|
| Kunjungan sudah punya penanda rujukan sejak awal | `TrxPatientEncounter.IsReferral@9124900` — dipakai seluruh jenis kunjungan |
| Rawat Jalan dan IGD juga menerima pasien rujukan | Penanda itu tidak dibatasi pada kunjungan laboratorium |

Karena itu instansi dan dokter perujuk berada di `Areas/HealthServices/MasterData/Models/`,
bukan di folder Laboratorium.

**Contoh yang dicegah aturan ini:**

> Klinik Sehat Sentosa mengirim rata-rata 40 pasien per bulan. Bila namanya diketik bebas,
> tiga petugas berbeda akan menulis "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan
> "sehat sentosa". Laporan dokter pengirim akan menghitungnya sebagai tiga institusi dengan
> masing-masing belasan pasien, dan kerja sama dengan klinik itu tidak akan pernah terlihat
> nilainya.
>
> Dengan daftar terkendali, ketiganya menunjuk satu baris yang sama.

**Akibat pada pelaksanaan:**

| Yang diperlukan | Pemiliknya |
|---|---|
| Dua data induk baru: instansi perujuk dan dokter perujuk | Master Data |
| Kolom penunjuk pada kunjungan | Registrasi |
| Layar pemilihan saat mendaftarkan pasien rujukan | Laboratorium |
| Pengisian daftar instansi perujuk sebelum dipakai | Master Data bersama Laboratorium |

Karena dua di antaranya milik modul lain, diperlukan kesepakatan. Dicatat sebagai
`LAB-COORD-004`.

### BR-32 — Disiplin melekat pada jenis pemeriksaan di katalog (`LAB-DEC-036`)

**Menutup `DEC-LAB-010`.** Mengamandemen AC-25.

**Latar belakang.** `MstProcedure@9124900` sudah memiliki penanda jenis tindakan —
`IsLaboratory`, `IsRadiology`, `IsSurgery`, `IsTherapy` — tetapi **tidak ada pembeda** antara
Patologi Klinik, Patologi Anatomi, dan Mikrobiologi. Yang tersedia hanya `ProcedureGroupName`
dan `ProcedureCategoryName` berupa teks bebas, yang tidak dapat diandalkan.

**Aturan:**

1. Satu kolom penanda disiplin ditambahkan pada `MstProcedure`, hanya bermakna bila
   `IsLaboratory` bernilai benar.
2. Nilainya: Patologi Klinik, Patologi Anatomi, atau Mikrobiologi.
3. Sistem menolak pemeriksaan yang disiplinnya tidak sesuai dengan disiplin pesanan
   (`INV-22`).
4. Kolom itu **satu-satunya** tambahan Laboratorium pada `MstProcedure`. Satuan hasil, batas
   nilai, jenis wadah, dan atribut operasional lain **tetap tidak boleh** masuk ke sana —
   seluruhnya berada di tabel milik Laboratorium sesuai `LAB-DEC-018`.

**Kenapa ini boleh, sementara batas nilai tidak boleh.**

| Yang ditambahkan | Sifatnya | Boleh di `MstProcedure`? |
|---|---|---|
| Penanda disiplin | **Klasifikasi** jenis tindakan, sejenis `IsLaboratory` dan `IsRadiology` yang sudah ada | **Ya** |
| Satuan hasil, batas normal, batas kritis | **Data operasional** yang berbeda menurut jenis kelamin dan umur, sehingga tidak muat satu baris per pemeriksaan | **Tidak** |

**Amandemen AC-25.** Bunyi lama: *"`MstProcedure` tidak bertambah satu kolom pun akibat
pekerjaan modul Laboratorium."* Bunyi baru ada pada AC-25 yang diperbarui — yang dilarang
adalah kolom **operasional**, bukan kolom klasifikasi.

**Contoh penerapan `INV-22`:**

> Petugas membuat pesanan berdisiplin Mikrobiologi, lalu mencoba menambahkan Hemoglobin.
> Hemoglobin bertanda disiplin Patologi Klinik pada katalog, sehingga sistem menolaknya dengan
> pesan bahwa pemeriksaan itu bukan bagian Mikrobiologi.
>
> Tanpa kolom ini, sistem tidak punya cara mengetahuinya, dan pesanan campur aduk baru
> ketahuan saat petugas laboratorium kebingungan di meja kerja.

**Akibat pada pelaksanaan.** `MstProcedure` milik Master Data. Penambahan kolom memerlukan
izin pemiliknya, dan pengisian nilainya untuk seluruh pemeriksaan berpenanda `IsLaboratory`
yang sudah ada. Dicatat sebagai `LAB-COORD-005`.

### BR-25 — Tarif ditampilkan, keputusan uang tetap milik Billing (`LAB-DEC-029`)

**Menegaskan `LAB-INH-010` dan `LAB-INH-012`, tidak melemahkannya.**

**Aturan:**

| Yang **boleh** dilakukan Laboratorium | Yang **tetap dilarang** |
|---|---|
| Menampilkan harga satuan, jumlah, subtotal, dan total saat memesan | Membuat, mengubah, atau membatalkan tagihan |
| Menampilkan status cakupan penjamin, termasuk penanda tidak tercakup | Memutuskan apakah pasien membayar |
| Menyimpan salinan tarif saat kejadian pada baris pemeriksaan | Menyimpan status pembayaran sebagai kebenaran |
| Mengelola daftar tarif laboratorium sebagai data induk modul | Menghitung tagihan akhir |

**Contoh yang menjelaskan batas ini:**

> Saat memesan, petugas melihat Hemoglobin Rp50.000, Kultur darah Rp350.000, total
> Rp400.000, dan penanda bahwa kultur darah tidak tercakup penjamin. Angka itu **membantu
> petugas dan pasien mengambil keputusan sebelum pemeriksaan dimulai**. Ia bukan tagihan.
> Tagihan tetap dibentuk Billing berdasarkan fakta kelayakan tagih yang dikirim Laboratorium.

**Kepemilikan tarif ditutup `LAB-DEC-033`** — lihat BR-29.

### BR-29 — Tarif tetap milik Master Data; Laboratorium hanya menyajikannya (`LAB-DEC-033`)

**Menutup `LAB-OPEN-016`.** Mempertajam BR-25.

**Bukti yang menentukan.** `MstTariff@9124900` adalah tabel tarif **bersama seluruh rumah
sakit**, bukan tabel khusus satu modul:

| Yang sudah ditampung `MstTariff` | Keterangan |
|---|---|
| `ProcedureId` | Menunjuk jenis tindakan atau pemeriksaan |
| `DrugId` | Tabel yang sama juga melayani obat |
| `ServiceUnitId`, `ClinicId`, `PatientClassId` | Tarif dapat berbeda menurut unit, klinik, dan kelas pasien |
| `EffectiveStartDate`, `EffectiveEndDate` | Masa berlaku tarif |
| `IsRoomCharge`, `IsAdministrationFee`, `IsRegistrationFee`, `IsConsultationFee` | Tabel yang sama juga melayani biaya kamar, administrasi, pendaftaran, dan konsultasi |

Laboratorium sudah membacanya lewat `LabSpecimenService#ResolveTariffAsync@9124900`, yang
memilih tarif berlaku menurut `ProcedureId` dan masa berlakunya.

**Aturan:**

1. Tarif pemeriksaan laboratorium **tetap milik** Master Data. Laboratorium **tidak** membuat
   tabel tarif sendiri.
2. Menu **`Tarif Laboratorium`** pada modul Laboratorium adalah **tampilan tersaring**: ia
   memperlihatkan baris `MstTariff` yang menunjuk pemeriksaan berpenanda `IsLaboratory`, agar
   kepala instalasi dapat memeriksanya tanpa berpindah modul.
3. Menu itu bersifat **baca saja**. Perubahan tarif tetap dilakukan lewat Master Data.
4. Salinan tarif saat kejadian tetap disimpan pada baris pemeriksaan, sesuai `LAB-DEC-024`.

**Kenapa satu sumber harga itu penting.**

> Bila Laboratorium punya tabel tarifnya sendiri, rumah sakit akan punya dua harga untuk
> pemeriksaan yang sama. Petugas lab menyebut Hemoglobin Rp50.000 kepada pasien, sementara
> Billing menagihkan Rp65.000 dari tabel yang berbeda. Yang dirugikan bukan sistemnya,
> melainkan kepercayaan pasien di loket.

**Akibat yang harus disadari.** Kepala instalasi laboratorium **tidak dapat** mengubah tarif
sendiri. Setiap penyesuaian tarif pemeriksaan melewati petugas Master Data. Ini konsekuensi
yang disengaja dari memilih satu sumber harga.

**Perbedaan dengan sistem yang berjalan.** Aplikasi laboratorium yang ada memperlihatkan
`Tarif Laboratorium` sebagai menu tersendiri. Pada Quilvian, menu itu tetap ada tetapi
**maknanya berubah** — dari pengelolaan menjadi penyajian.

### BR-26 — Kemampuan tambahan yang masuk scope modul (`LAB-DEC-030`)

Sebelas kemampuan yang terlihat pada bukti lapangan dinyatakan **milik modul Laboratorium**,
dengan pembagian rilis sebagai berikut.

| Kemampuan | Rilis | Alasan penempatan |
|---|---|---|
| Pendaftaran pasien datang langsung dan rujukan luar | **Rilis 1** | Tanpa ini, sebagian pasien tidak dapat dilayani sama sekali |
| Katalog pemeriksaan beserta tarif dan cakupan | **Rilis 1** | Dibutuhkan sejak layar pemesanan pertama |
| Monitoring per disiplin | **Rilis 1** | Tiga daftar sejajar, sesuai `LAB-DEC-025` |
| Penanda Duplo | **Rilis 1** | Melekat pada pemeriksaan, sekalian dengan Cito |
| Penanda Definitif pada Mikrobiologi | Rilis 2 | Maknanya belum diputuskan, lihat `LAB-OPEN-017` |
| Nota Lab, Label Lab, Label Golongan Darah | Rilis 2 | Kemampuan cetak, tidak memblokir alur kerja |
| Kirim hasil ke pasien | Rilis 2 | Menyentuh privasi, perlu keputusan tersendiri |
| Ekspor Excel daftar order dan nilai kritis | Rilis 2 | Kenyamanan, bukan prasyarat |
| Laporan operasional laboratorium | Rilis 2 | Sebelas jenis laporan; besar dan tidak memblokir alur |
| Penautan hasil laboratorium eksternal berupa berkas PDF | Rilis 2 | Bergantung `LAB-COORD-002` |
| Order dari MCU | Rilis 3 | Sumber pesanan tambahan; MCU belum dibahas sama sekali |

**Yang tetap di luar scope modul:** Bank Darah, dan seluruh kemampuan Quality Control serta
pemantauan beban kerja alat yang terlihat pada perangkat lunak pihak ketiga.

### BR-27 — Baseline alur ujung ke ujung (`LAB-DEC-031`)

Alur berikut diadopsi sebagai baseline resmi modul, mengikuti kesimpulan analisis konsolidasi:

> Registrasi dan Pemesanan → Monitoring → Penerimaan Wadah → Pemeriksaan → Pengisian Hasil
> sesuai disiplin → Validasi, Rilis, dan Otorisasi → Nilai Kritis dan Komunikasi bila
> diperlukan → Riwayat dan Distribusi Hasil → Pelaporan → Penautan Hasil Eksternal.

**Peringatan yang wajib dibawa.** Analisis konsolidasi menyatakan sendiri bahwa baseline ini
**belum boleh dijadikan kontrak implementasi final** sebelum delapan hal berikut diputuskan:
matriks kewenangan per peran, urutan status resmi, aturan pembatalan dan koreksi hasil, alur
dan batas waktu nilai kritis, integrasi teknis alat laboratorium, kebijakan jejak audit, aturan
tagihan dan cakupan, serta mekanisme penyelarasan antaraplikasi.

Kedelapannya dicatat sebagai `LAB-P0-001` sampai `LAB-P0-008` pada bagian Open Questions.

---

## State dan Transition

Kerangka mengikuti `LAB-INH-001`, `LAB-INH-002`, dan `LAB-INH-003`. Bagian yang ditambahkan
sesi ini adalah kewenangan pada alur **hasil**.

### Alur hasil pemeriksaan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Pemeriksaan mulai dikerjakan | `Pending` | Sistem | Sampel sudah `Accepted` |
| `Pending` | Mulai mengerjakan | `InProcess` | Analis | Sampel masih layak |
| `InProcess` | Menyimpan angka hasil | `Completed` | Analis | Seluruh nilai wajib terisi |
| `Completed` | Memvalidasi | `Validated` | Petugas berwenang validasi, **bukan** pengisi hasil | BR-01 terpenuhi atau pengecualian tercatat |
| `Validated` | Merilis | `Released` | Petugas berwenang rilis | Bila nilai kritis, formulir pelaporan BR-02 wajib muncul |
| `Released` | Mengoreksi | `Corrected/Amended` | Petugas berwenang validasi atau rilis. Analis biasa **tidak boleh** | Riwayat rilis lama tetap disimpan (`LAB-INH-003`, BR-05) |
| `Corrected/Amended` | Memvalidasi ulang | `Revalidated` | Petugas berwenang validasi | BR-01 tetap berlaku |
| `Revalidated` | Merilis ulang | `Released` | Petugas berwenang rilis | Versi lama tetap terlihat bertanda "sudah diperbaiki", dan dokter pemesan otomatis diberi tahu (BR-05) |

Status hasil di atas berlaku **per pemeriksaan**, bukan per pesanan (BR-06). Dalam satu
pesanan, Kalium boleh sudah `Released` sementara Hemoglobin masih `InProcess`.

---

## Skenario Normal dan Exception

### Skenario normal — pemeriksaan Hemoglobin rawat jalan

1. **Tujuan:** dokter memperoleh angka Hemoglobin pasien untuk menentukan terapi.
2. **Pelaku:** dr. Rina (pemesan), perawat Dewi (pengambil sampel), petugas penerimaan lab
   Budi, analis Sari (pengisi hasil), penanggung jawab teknis Tono (validator dan perilis).
3. **Pemicu:** dr. Rina memesan pemeriksaan Hemoglobin saat pasien Andi berkunjung.
4. **Prasyarat:** pasien Andi sudah punya kunjungan (*encounter*) aktif dari Registrasi.
5. **Langkah utama:**
   1. dr. Rina membuat pesanan lab. Status pesanan `Requested`.
   2. Perawat Dewi mengambil darah dan memindai barcode tabung. Sampel `Collected`.
   3. Sampel diantar ke lab. Petugas Budi memindai barcode. Sampel `Received`.
   4. Budi memeriksa kelayakan sampel dan menyatakan layak. Sampel `Accepted`.
      **Pada titik ini pemeriksaan sah untuk ditagihkan** (`LAB-INH-009`).
   5. Analis Sari mengerjakan pemeriksaan, lalu mengetik hasil 9,4 g/dL. Hasil `Completed`.
   6. Tono memvalidasi hasil. Hasil `Validated`. BR-01 terpenuhi karena Tono bukan Sari.
   7. Tono merilis hasil. Hasil `Released`. dr. Rina dapat membacanya.
6. **Hasil akhir:** dr. Rina melihat Hemoglobin 9,4 g/dL, Billing menerima fakta bahwa
   pemeriksaan Hemoglobin sah ditagihkan sejak langkah 4.

### Jalur tidak normal

| Kejadian | Yang terjadi di sistem | Acuan |
|---|---|---|
| Sampel darah menggumpal saat diperiksa kelayakannya | Budi menolak sampel dengan alasan terkendali. Sampel `Rejected`. Secara *default* tidak ada tagihan pemeriksaan | `LAB-INH-009` |
| Sampel harus diambil ulang karena tabung pecah oleh petugas lab | Sebab diisi `InternalHospitalError`. Sampel baru dibuat, sampel lama tetap terlihat dan tertaut. Pasien **tidak** ditagih dua kali | `LAB-INH-005`, `LAB-INH-011` |
| Analis shift malam sendirian dan harus memvalidasi hasilnya sendiri | Diizinkan dengan alasan tercatat dan penanda permanen pada lembar hasil | BR-01 |
| Hasil Kalium 7,2 mmol/L (kritis) | Hasil tetap dirilis, formulir pelaporan wajib diisi, masuk daftar pantau bila belum dilaporkan | BR-02 |
| dr. Rina ingin membatalkan pesanan setelah sampel `Accepted` | Dokter hanya bisa **mengajukan** pembatalan. Lab yang memproses. Tagihan tidak otomatis hilang; Billing yang memutuskan | `LAB-INH-006`, `LAB-INH-012` |
| Hasil sudah dirilis lalu ketahuan salah | Hanya petugas berwenang validasi/rilis yang boleh mengoreksi. Versi lama tetap terlihat bertanda "sudah diperbaiki", dokter pemesan otomatis diberi tahu | BR-05 |
| Kalium selesai lebih dulu, Hemoglobin masih diproses | Kalium langsung dirilis. Lembar hasil menampilkan peringatan "hasil belum lengkap" | BR-06 |
| Hasil di luar batas normal tetapi belum kritis, misalnya Kalium 5,3 mmol/L | Ditandai "di atas nilai rujukan". Formulir pelaporan nilai kritis **tidak** muncul | BR-04 |

---

## Frontend Decision Authority

Urutan wewenang yang berlaku, dari yang paling kuat:
keamanan/privasi/invariant → arahan produk/UI yang disetujui → konvensi project → keleluasaan
developer (`DEV_DISCRETION`).

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `LAB-FE-001` | Letak menu dan penamaan route | Konvensi project | `decided` | Wajib mengikuti pola modul Health Services yang sudah ada, misalnya `pharmacy-management` dan `inpatient-management`. Tidak boleh membuat pola penamaan baru | `LAB-DEC-010`; F5 menunjukkan frontend Laboratorium masih nol |
| `LAB-FE-002` | Tata letak layar, pemilihan tab/modal/drawer, warna, komponen | Developer | `DEV_DISCRETION` | Bebas selama mengikuti komponen dan gaya yang sudah dipakai modul lain | `LAB-DEC-010` |
| `LAB-FE-003` | Peringatan "hasil belum lengkap" pada lembar hasil | Invariant keselamatan | `decided` | **Wajib ada.** Bukan `DEV_DISCRETION`. Bentuk visualnya boleh dipilih developer, keberadaannya tidak boleh dihapus | BR-06 |
| `LAB-FE-004` | Penanda "divalidasi oleh pengisi sendiri" pada lembar hasil | Invariant keselamatan | `decided` | **Wajib terlihat** di layar dan di cetakan. Bentuk visualnya boleh dipilih developer | BR-01 |
| `LAB-FE-005` | Penandaan nilai kritis dan formulir pelaporannya | Invariant keselamatan | `decided` | **Wajib ada dan wajib menonjol.** Formulir pelaporan tidak boleh bisa dilewati begitu saja | BR-02 |
| `LAB-FE-006` | Urutan daftar kerja: pesanan cito di atas pesanan biasa | Invariant keselamatan | `decided` | **Wajib.** Aturan urutannya tidak boleh diserahkan pada selera tampilan | BR-07, BR-09 |
| `LAB-FE-007` | Kotak pemberitahuan dokter berisi nilai kritis dan koreksi hasil | Invariant keselamatan | `decided` | **Wajib ada.** Pemberitahuan yang belum dibaca harus terlihat jelas. Bentuk visual dan letaknya boleh dipilih developer | BR-08 |
| `LAB-FE-008` | Tombol atau penanda cito pada layar pembuatan pesanan | Konvensi project | `DEV_DISCRETION` | Bebas, asalkan hanya dokter pemesan yang bisa menandainya | BR-09 |

Catatan: butir bertanda `DEV_DISCRETION` boleh diputuskan developer. Butir bertanda `decided`
dengan alasan keselamatan **tidak boleh** dihapus atau diperlemah oleh keputusan tampilan.

---

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `LAB-FACT-001` | Fact | Backend Laboratorium sudah punya pesanan, sampel, alasan penolakan, dan riwayat transisi | — | `fact` | — | F1, F2, F3 pada SHA `9124900` |
| `LAB-FACT-002` | Fact | Hasil pemeriksaan, nilai kritis, nilai rujukan, panel, worklist, integrasi alat, dan TAT belum ada di backend | — | `fact` | — | F4 |
| `LAB-FACT-003` | Fact | Frontend belum punya modul Laboratorium sama sekali | — | `fact` | — | F5 pada SHA `c79bb6ee4` |
| `LAB-FACT-004` | Fact | Katalog pemeriksaan lab masih menumpang `MstProcedure.IsLaboratory` tanpa atribut khas lab | — | `fact` | — | F6 |
| `LAB-FACT-005` | Fact | Siklus hidup pesanan, sampel, hasil, dan titik kelayakan tagih sudah dikunci di `RJ-BIL-GATE-DEC-003` | Billing + Clinical Governance | `locked-draft` | Approval SHA-256 `4d4447...`; tanda tangan formal Lab masih `OPEN` | `rawat-jalan/00-interview-decisions.md` |
| `LAB-DEC-001` | Decision | **Rilis 1 = menyelesaikan rantai sampai hasil dirilis.** Isinya: pengisian hasil, verifikasi, validasi, rilis hasil, nilai kritis, daftar kerja petugas, penyajian hasil ke dokter dan pasien, serta seluruh tampilan frontend dari nol. Katalog pemeriksaan sementara tetap memakai `MstProcedure`. Katalog lab mandiri ditunda ke Rilis 2 | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Jawaban wawancara sesi ini; alasan: rantai pesanan-sampel sudah berjalan tetapi mati di ujung, sehingga modul belum berguna bagi dokter maupun pasien |
| `LAB-DEC-002` | Decision | **Cakupan modul dibatasi pada Patologi Klinik saja** — darah, urin, feses, kimia klinik, hematologi, imunologi. Mikrobiologi, Patologi Anatomi, dan Bank Darah dikeluarkan menjadi modul atau slice terpisah | Pemilik proses Laboratorium | `superseded` oleh `LAB-DEC-025` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Jawaban wawancara sesi ini; alasan: alur sampel yang sudah dikodekan memang cocok untuk pola hasil sekali jadi |
| `LAB-SCOPE-001` | Open Question | Apa batas resmi modul Laboratorium untuk rilis pertama? | Pemilik proses Laboratorium | `superseded` | Digantikan `LAB-DEC-001` dan `LAB-DEC-002` | Pertanyaan pembuka sesi ini |
| `LAB-DEC-003` | Decision | **Prinsip empat mata pada validasi hasil.** Pengisi hasil tidak boleh memvalidasi dan merilis hasil yang sama. Pengecualian diizinkan dengan alasan terkendali, penanda permanen "divalidasi oleh pengisi sendiri", dan jejak audit | Pemilik proses Laboratorium + Clinical Governance | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01. **Menunggu tanda tangan klinis terpisah** sesuai `LAB-DEC-011` | Lihat BR-01 |
| `LAB-DEC-004` | Decision | **Nilai kritis tetap dirilis, pelaporan wajib tercatat.** Pemeriksaan belum tuntas sampai catatan pelapor, penerima, waktu, sarana, dan bukti pembacaan ulang terisi. Ada daftar pantau nilai kritis yang belum dilaporkan | Pemilik proses Laboratorium + Clinical Governance | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01. **Menunggu tanda tangan klinis terpisah** sesuai `LAB-DEC-011` | Lihat BR-02 |
| `LAB-DEC-005` | Decision | **Hasil Rilis 1 diketik manual oleh analis.** Sambungan otomatis ke alat laboratorium ditunda, dan struktur data Rilis 1 tidak menyiapkan tempat untuk hasil dari alat | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-03 |
| `LAB-RISK-001` | Assumption | Karena `LAB-DEC-005` tidak menyiapkan tempat data asal hasil, penyambungan alat di kemudian hari akan memerlukan perubahan struktur data hasil, dan riwayat hasil lama tidak dapat membedakan hasil ketikan dari hasil kiriman alat | Pemilik proses Laboratorium | `draft` | Diberitahukan pada sesi ini, pengguna tetap memilih pengetikan manual murni | Konsekuensi langsung `LAB-DEC-005` |
| `LAB-OPEN-001` | Open Question | Siapa pemilik modul Laboratorium yang berwenang menyetujui keputusan klinis dan operasional? | Manajemen rumah sakit | `closed` | Ditutup 2026-09-01: **Yoga Aji Pratama** ditetapkan sebagai pemilik modul | Pernyataan pengguna pada sesi 2026-09-01 |
| `LAB-FACT-006` | Fact | Platform belum punya sarana notifikasi umum maupun pemberitahuan tersimpan. Yang ada hanya SignalR `QueueHub` khusus antrean | — | `fact` | — | F7 pada SHA `9124900` |
| `LAB-FACT-007` | Fact | **Kedua dokumen tata kelola backend ditemukan dan masih berlaku.** Sumber canonical lintas vendor: `QuilvianEngineeringSkills/agents/rules/backend/engineering/`. Edisi Claude: `QuilvianEngineeringSkills/Claude/.claude/rules/backend/engineering/`. Kedua salinan identik byte-per-byte (md5 `ad549762…` untuk kontrak, `6d11c0de…` untuk registry), tercommit pada `59bd3e2` di `DevBenari/QuilvianEngineeringSkills` | — | `fact` | — | Menutup `LAB-OPEN-002` |
| `LAB-FACT-008` | Fact | **`AGENTS.md` backend bertentangan dengan dirinya sendiri.** Baris 11 dan 20 masih menunjuk `docs/engineering/…`, sedangkan baris 40 menunjuk `rules/backend/engineering/…` dan menyatakan repository ini “tidak lagi memiliki folder `agents/rules/`”. Folder `agents/rules/` tetap ada di working tree berisi 7 berkas — persis peninggalan tercabut yang `AGENTS.md` perintahkan untuk dilaporkan | Pemilik repository backend | `fact` | — | `AGENTS.md:11`, `AGENTS.md:20`, `AGENTS.md:40`, `AGENTS.md:53` |
| `LAB-FACT-009` | Fact | **Rules root yang benar-benar terpasang tidak memuat kedua dokumen itu.** `AGENTS.md` menetapkan rules root Claude Code di `${CLAUDE_PLUGIN_ROOT}/.claude/rules/`. Plugin terpasang `quilvian-engineering-skills@quilvian` versi `0.1.0` hanya punya `rules/backend/` tanpa subfolder `engineering/`, dan tanpa `GLOBAL_RULES.md`. Marketplace terpasang menunjuk `MHamzah1/QuilvianEngineeringSkillsClaude` pada `f0136df` — repository yang **berbeda** dari sumber canonical `DevBenari/QuilvianEngineeringSkills` | Pemilik repository backend | `fact` | — | `~/.claude/plugins/installed_plugins.json`, `~/.claude/plugins/known_marketplaces.json`, `git ls-tree` pada klon marketplace |
| `LAB-FACT-010` | Fact | **Registry mencatat `HealthServices / LaboratoryManagement / Laboratory`, prefix `Lab`, lifecycle `PLANNED`.** Prefix `Lab` = *Laboratory* sudah terdaftar sehingga hak penamaan sudah ada, tetapi registry secara eksplisit menyatakan persetujuan registry **tidak** memberi wewenang implementasi, migration, pekerjaan database, deployment, maupun aktivasi modul berstatus `PLANNED` | — | `fact` | — | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris tabel Laboratorium dan paragraf 3 |
| `LAB-CONFLICT-001` | Conflict | `LAB-DEC-001` menunda katalog lab ke Rilis 2, tetapi `LAB-DEC-004` mewajibkan pengenalan nilai kritis yang butuh batas nilai. `MstProcedure` tidak punya kolomnya | Pemilik proses Laboratorium | `resolved` | Diselesaikan 2026-09-01 oleh `LAB-DEC-006` | F6 dan tabrakan antara `LAB-DEC-001` dan `LAB-DEC-004` |
| `LAB-DEC-006` | Decision | **Tabel batas nilai ditarik maju ke Rilis 1.** Isinya satuan hasil, batas normal bawah/atas, batas kritis bawah/atas, serta pembeda jenis kelamin dan kelompok umur. Sisa katalog lab tetap Rilis 2 | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-04. Menutup `LAB-CONFLICT-001` |
| `LAB-DEC-007` | Decision | **Koreksi hasil setelah rilis hanya oleh petugas berwenang validasi/rilis, dan dokter pemesan otomatis diberi tahu.** Versi lama tetap terlihat bertanda "sudah diperbaiki" | Pemilik proses Laboratorium + Clinical Governance | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01. **Menunggu tanda tangan klinis terpisah** sesuai `LAB-DEC-011` | Lihat BR-05. Menutup `LAB-OPEN-005` |
| `LAB-DEC-008` | Decision | **Hasil boleh dirilis sebagian per pemeriksaan.** Status hasil melekat pada pemeriksaan, bukan pesanan. Lembar hasil wajib memberi peringatan "hasil belum lengkap" | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-06 |
| `LAB-OPEN-005` | Open Question | Siapa yang berwenang mengoreksi hasil yang sudah dirilis, dan apakah dokter pemesan wajib diberi tahu? | Pemilik proses Laboratorium + Clinical Governance | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-007` | `LAB-INH-003` mengunci alur statusnya, tetapi tidak kewenangan dan pemberitahuannya |
| `LAB-CONFLICT-002` | Conflict | `LAB-INH-001` mengunci alur dimulai dari `Draft`, tetapi `Services/LabOrderService.cs:136@9124900` selalu membuat pesanan berstatus `Requested` dan tidak ada kode yang menetapkan `Draft`. `LAB-INH-006` menjadi kosong | Yoga Aji Pratama | `resolved` | Diselesaikan 2026-09-01 oleh `LAB-DEC-015` | `01-existing-capability-map.md#CONF-01` |
| `LAB-DEC-015` | Decision | **Status `Draft` dihapus.** Pesanan tetap dibuat langsung `Requested`, tetapi dokter pemesan boleh menyunting pesanannya selama belum ada sampel berstatus `Collected`. Setelah itu pesanan terkunci dan dokter hanya boleh mengajukan pembatalan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-11. Menutup `LAB-CONFLICT-002` |
| `LAB-DEC-016` | Decision | **Pemberitahuan tersimpan dibangun sebagai kemampuan platform bersama**, bukan milik Laboratorium. Laboratorium menjadi pemakai pertama. Dokter punya satu kotak masuk untuk seluruh modul | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-12. Menutup `Q-LAB-02` dan `01-existing-capability-map.md#UNK-02` |
| `LAB-COORD-001` | Open Question | Kesepakatan dengan pemilik platform mengenai bentuk data, lokasi kode, dan urutan pengerjaan kemampuan pemberitahuan bersama | Yoga Aji Pratama + pemilik platform | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-016`; kemampuan ini di luar kepemilikan modul Laboratorium |
| `DEC-LAB-008` | Open Question | Apakah satu wadah fisik dapat melayani beberapa pemeriksaan? Model sekarang menyatukan wadah dan pemeriksaan menjadi satu konsep, sehingga dua pemeriksaan dari satu tabung serum memaksa dua barcode, dan penolakan tabung yang keruh dapat dilakukan sebagian — sesuatu yang tidak mungkin secara fisik | Yoga Aji Pratama + kepala instalasi laboratorium | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-024` | Ditemukan `03-domain-architecture.md#DEC-LAB-008`. Bukti: `TrxLabSpecimen.ProcedureId@9124900` dan pengujian `#DuaKomponenLayakSatuDitolak_MenagihTigaRatusLimaPuluhRibu@9124900` |
| `LAB-DEC-017` | Decision | **Hasil lab didaftarkan ke rekam medis sebagai jenis dokumen klinis baru.** Isi hasil tetap di tabel Laboratorium, tidak digandakan. Rekam medis mencatat penulis, penandatanganan, dan penguncian | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-13. Menutup `Q-LAB-03` dan `01-existing-capability-map.md#UNK-01` |
| `LAB-COORD-002` | Open Question | Kesepakatan dengan pemilik modul `rekam-medis` untuk menambah nilai baru pada `ClinicalDocumentKind` | Yoga Aji Pratama + pemilik `rekam-medis` | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-017`; enum itu milik modul lain |
| `LAB-DEC-025` | Decision | **Cakupan diperluas menjadi tiga disiplin**: Patologi Klinik, Patologi Anatomi, dan Mikrobiologi. Bank Darah tetap di luar scope. Menggantikan `LAB-DEC-002` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-21. Bukti: `Analisis_Konsolidasi_Modul_Laboratorium.md` bagian 3.1 dan 13.1; menutup `REC-CONF-001` |
| `LAB-DEC-026` | Decision | **Cito dan Duplo melekat pada pemeriksaan terpesan, bukan pesanan.** Satu pesanan boleh memuat pemeriksaan cito dan biasa sekaligus | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-22. Bukti: `LAB-RULE-006`; menutup `REC-CONF-002` |
| `LAB-DEC-027` | Decision | **Hasil punya empat bentuk**: angka bersatuan, pilihan terbatas, mikrobiologi berstruktur, dan narasi Patologi Anatomi. Menggantikan `LAB-DEC-021` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-23. Bukti: `LAB-CAP-016`, `LAB-CAP-017`; menutup `REC-CONF-003` |
| `LAB-DEC-028` | Decision | **Laboratorium memiliki jalur pendaftaran pasien datang langsung dan rujukan luar.** Identitas pasien dan kunjungan tetap milik modul lain | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-24. Bukti: `LAB-CAP-002`; menutup `REC-CONF-004` |
| `LAB-DEC-032` | Decision | **Layar pendaftaran milik Laboratorium, pembuatan kunjungan tetap milik Registrasi.** Laboratorium memanggil Registrasi lalu menyimpan penunjuk kunjungan yang dikembalikan. `INV-01` tetap utuh | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-28. Bukti: `EncounterRegistrationSource.WalkIn`, `TrxPatientEncounter.IsWalkIn`, `IsReferral@9124900`. Menutup `LAB-OPEN-015` |
| `LAB-DEC-036` | Decision | **Satu kolom penanda disiplin ditambahkan pada `MstProcedure`**, hanya bermakna bila `IsLaboratory` benar. Satu-satunya tambahan Laboratorium pada tabel itu; atribut operasional tetap dilarang. Mengamandemen AC-25 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-32. Menutup `DEC-LAB-010` |
| `LAB-COORD-005` | Open Question | Izin pemilik `master-data` untuk menambah kolom disiplin pada `MstProcedure`, dan pengisian nilainya untuk pemeriksaan lab yang sudah ada | Yoga Aji Pratama + pemilik `master-data` | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-036` |
| `DEC-LAB-010` | Open Question | Bagaimana disiplin melekat pada jenis pemeriksaan? | Yoga Aji Pratama + pemilik `master-data` | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-036` | `03-domain-architecture.md#DEC-LAB-010` |
| `LAB-DEC-035` | Decision | **Instansi dan dokter perujuk menjadi data induk global milik Master Data.** Kunjungan menunjuk ke sana; nama tidak disimpan sebagai teks bebas. Laboratorium hanya memilih dari daftar | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-31. Menutup `DEC-LAB-009`, membuka `S13b` |
| `LAB-COORD-004` | Open Question | Kesepakatan dengan pemilik `master-data` dan `registration-management`: dua data induk baru, kolom penunjuk pada kunjungan, dan pengisian daftar instansi perujuk | Yoga Aji Pratama + pemilik `master-data` + pemilik `registration-management` | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-035` |
| `DEC-LAB-009` | Open Question | Di mana identitas dokter dan instansi perujuk disimpan? | Yoga Aji Pratama + pemilik `registration-management` | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-035` | `03-domain-architecture.md#DEC-LAB-009` |
| `LAB-DEC-034` | Decision | **Penempatan data induk mengikuti cakupan pemakaiannya — backend saja.** Khusus Laboratorium diletakkan di folder Laboratorium; global diletakkan di folder Master Data. **Frontend tidak mengikutinya**; menu data induk frontend tetap di `health-services/master-data/` sesuai konvensi yang sudah ada | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-30. Bukti: 20 data induk khusus modul sudah berada di folder modulnya pada `9124900` |
| `LAB-DEC-033` | Decision | **Tarif tetap milik Master Data.** Menu `Tarif Laboratorium` menjadi tampilan tersaring yang bersifat baca saja. Laboratorium tidak membuat tabel tarif sendiri | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-29. Bukti: `MstTariff@9124900` adalah tabel bersama; `ResolveTariffAsync@9124900`. Menutup `LAB-OPEN-016` |
| `LAB-FACT-007` | Fact | **Dokumen tata kelola canonical ditemukan.** `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` ada pada commit `c9692d0` "Repair QBE canonical governance paths". Checkout lokal berada di cabang `yoga` pada `9124900`, **7 commit tertinggal** dari `origin/yoga`. Ketujuh commit itu **tidak menyentuh Laboratorium** | — | `fact` | — | `git log HEAD..c9692d0`; `git diff --name-only HEAD..c9692d0 \| grep -i lab` kosong |
| `LAB-OPEN-002` | Open Question | Di mana `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`? | Pemilik repository backend | `closed` | Ditutup 2026-09-01 oleh `LAB-FACT-007` — dokumennya ada, checkout lokal yang tertinggal | Folder `docs/engineering/` tidak ada pada `9124900`, tetapi ada pada `c9692d0` |
| `LAB-OPEN-018` | Open Question | Prefix mana yang berlaku untuk data induk milik Laboratorium: `Mst` mengikuti baris Master/Reference, atau `Lab` mengikuti aturan `<PrefixPemilik><Konsep>`? Menyentuh penamaan `MstLabValueBound` dan `MstLabValueOption` | Pemilik registry prefix + Yoga Aji Pratama | `open` | — | `QBE-NAM-002` mewajibkan prefix registry; `QBE-NAM-004` melarang menyimpulkannya sendiri |
| `LAB-OPEN-019` | Open Question | Lifecycle `LaboratoryManagement` pada registry masih **`PLANNED`**, yang menurut registry **tidak memberi wewenang implementasi, migration, maupun deployment**. Padahal `LabOrder` dan siklus hidup wadah sudah berjalan di produksi. Perlu dinaikkan ke `ACTIVE` atau dijelaskan dasar pekerjaan yang sudah berjalan | Pemilik registry prefix | `open` | — | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md@c9692d0` baris `LaboratoryManagement / Laboratory \| Lab \| PLANNED` |
| `LAB-COORD-003` | Open Question | Kesepakatan kontrak pemanggilan antarmodul dengan pemilik `registration-management`: bentuk permintaan, bentuk jawaban, dan perilaku saat gagal | Yoga Aji Pratama + pemilik `registration-management` | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-032` |
| `LAB-DEC-029` | Decision | **Tarif dan cakupan ditampilkan saat memesan; keputusan uang tetap milik Billing.** Menegaskan `LAB-INH-010` dan `LAB-INH-012` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-25. Bukti: `LAB-CAP-004`; menutup `REC-CONF-005` |
| `LAB-DEC-030` | Decision | **Sebelas kemampuan tambahan masuk scope modul** dengan pembagian Rilis 1, 2, dan 3. Bank Darah, Quality Control, dan pemantauan beban alat tetap di luar | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-26 |
| `LAB-DEC-031` | Decision | **Baseline alur ujung ke ujung diadopsi** mengikuti kesimpulan analisis konsolidasi, disertai peringatan bahwa delapan hal `LAB-P0-*` belum diputuskan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-27 |
| `LAB-DEC-024` | Decision | **Wadah fisik dipisahkan dari pemeriksaan terpesan.** Satu wadah = satu barcode = satu keputusan layak atau tolak, dan dapat melayani beberapa pemeriksaan. Kelayakan tagih tetap terbit per pemeriksaan. Penolakan berlaku serentak bagi seluruh pemeriksaan pada wadah itu | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-20. Menutup `DEC-LAB-008` dari `03-domain-architecture.md` |
| `LAB-OPEN-013` | Open Question | Apakah penanda Cito dan Duplo berdampak pada tarif? Bukti menempatkan keduanya sebaris dengan harga, tetapi dampaknya tidak diperagakan | Yoga Aji Pratama + Billing | `open` | — | Konsekuensi `LAB-DEC-026` |
| `LAB-OPEN-014` | Open Question | Bagaimana hasil mikrobiologi dan patologi anatomi masuk alur nilai kritis? Keduanya tidak dapat dinilai dengan perbandingan batas angka | Yoga Aji Pratama + Clinical Governance | `open` | — | Konsekuensi `LAB-DEC-027` |
| `LAB-OPEN-015` | Open Question | Bagaimana Laboratorium membuat kunjungan untuk pasien datang langsung dan rujukan luar tanpa mengambil alih kepemilikan kunjungan dari modul Registrasi? | Yoga Aji Pratama + pemilik `registration-management` | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-032` | Konsekuensi `LAB-DEC-028` |
| `LAB-OPEN-016` | Open Question | Apakah daftar tarif laboratorium dimiliki modul Laboratorium atau tetap milik Master Data? | Yoga Aji Pratama + pemilik `master-data` | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-033` | Konsekuensi `LAB-DEC-029` |
| `LAB-OPEN-017` | Open Question | Apa makna penanda Definitif pada hasil mikrobiologi, dan kapan ia wajib diisi? | Yoga Aji Pratama | `open` | — | Konsekuensi `LAB-DEC-030` |
| `LAB-P0-001` | Open Question | Matriks kewenangan per peran: siapa boleh menerima wadah, memproses, membatalkan, mengisi hasil, memvalidasi, mengotorisasi, menghapus pesanan, dan mengoreksi hasil yang sudah diotorisasi | Yoga Aji Pratama + Clinical Governance | `open` | — | `Analisis_Konsolidasi` bagian 14 P0-1. Sebagian sudah dijawab `LAB-DEC-022`, sisanya terbuka |
| `LAB-P0-002` | Open Question | Urutan status resmi lintas tiga aplikasi, termasuk posisi `Confirmed` yang belum punya padanan pada rancangan | Yoga Aji Pratama | `open` | — | `Analisis_Konsolidasi` bagian 7 dan 14 P0-2 |
| `LAB-P0-003` | Open Question | Aturan pembatalan dan koreksi: alasan wajib, jejak audit, dampak tagihan, dampak wadah, dampak hasil yang sudah masuk | Yoga Aji Pratama + Billing | `open` | — | `Analisis_Konsolidasi` bagian 14 P0-3 |
| `LAB-P0-004` | Open Question | Alur nilai kritis: ambang, batas waktu tanggap, eskalasi, bukti penerimaan, dan tindakan bila penerima tidak dapat dihubungi | Yoga Aji Pratama + Clinical Governance | `open` | — | `Analisis_Konsolidasi` bagian 14 P0-4; memperluas `LAB-DEC-004` |
| `LAB-P0-005` | Open Question | Integrasi alat laboratorium: pemetaan kode pemeriksaan, pemetaan wadah, penerimaan hasil, pengulangan, pencegahan ganda, antrean kesalahan, dan arah penyelarasan | Yoga Aji Pratama + pemilik platform | `open` | — | `Analisis_Konsolidasi` bagian 14 P0-5. Bertentangan arah dengan `LAB-DEC-005` yang menunda integrasi alat |
| `LAB-P0-006` | Open Question | Kebijakan jejak audit resmi laboratorium | Yoga Aji Pratama | `open` | — | `Analisis_Konsolidasi` bagian 15 |
| `LAB-P0-007` | Open Question | Aturan tagihan dan cakupan penjamin, termasuk arti `Tidak Tercover` dan perubahan penjamin setelah pesanan dibuat | Billing | `open` | — | `Analisis_Konsolidasi` bagian 14 P1-9 dan P1-10 |
| `LAB-P0-008` | Open Question | Mekanisme penyelarasan data antara aplikasi Laboratorium baru, HiSys, HCLAB, dan RS MMC App | Yoga Aji Pratama + pemilik platform | `open` | — | `Analisis_Konsolidasi` bagian 13.2 dan 18 |
| `LAB-OPEN-012` | Open Question | Berapa banyak data laboratorium yang sudah terisi di basis data produksi? Menentukan biaya dan risiko pemindahan data akibat `LAB-DEC-024` | Pemilik repository backend + DBA | `open` | — | Frontend Laboratorium nol (`CAP-21`) menjadi dugaan kuat bahwa data sungguhan belum ada, tetapi belum diverifikasi |
| `LAB-DEC-023` | Decision | **Batas normal bebas diubah kepala instalasi; batas kritis memerlukan persetujuan klinis.** Seluruh perubahan batas nilai disimpan sebagai riwayat lengkap. Mempersempit `LAB-DEC-018`, tidak membatalkannya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-19. Menutup `DEC-LAB-003` dari `02-requirement-completeness-assessment.md` |
| `LAB-DEC-022` | Decision | **Kewenangan validasi dan rilis tetap terpisah dan diberikan per orang, bukan per jabatan.** Setiap shift wajib punya minimal dua pemegang kewenangan validasi; sistem memperingatkan kepala instalasi bila hanya ada satu | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-18. Menutup `DEC-LAB-001` dari `02-requirement-completeness-assessment.md` |
| `LAB-DEC-021` | Decision | **Hasil punya dua bentuk: angka dan pilihan terbatas.** Pemeriksaan berhasil pilihan menyimpan daftar pilihan sah beserta penanda mana yang di luar rujukan dan mana yang kritis. Analis memilih, tidak mengetik bebas. `LAB-DEC-004` berlaku untuk kedua bentuk | Yoga Aji Pratama | `superseded` oleh `LAB-DEC-027` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-17. Menutup `DEC-LAB-002` dari `02-requirement-completeness-assessment.md` |
| `LAB-DEC-020` | Decision | **Koreksi hasil setelah kunjungan ditutup memakai mekanisme addendum rekam medis yang sudah ada.** Dokumen asli tetap terkunci dan tidak diubah; hasil perbaikan menempel sebagai addendum bertanda tangan dengan alasan koreksi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-16. Menutup `LAB-OPEN-011`. Dasar bukti: `MrcClinicalNoteAddendum.cs@9124900` |
| `LAB-DEC-019` | Decision | **Alasan penolakan sampel dikelola kepala instalasi lewat layar pengelolaan, kecuali penanda kesalahan internal dan penanda wajib catatan** yang hanya dapat disetel admin sistem. Data awal wajib disiapkan pada Rilis 1 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-15. Menutup `Q-LAB-04` |
| `LAB-DEC-018` | Decision | **Batas nilai menjadi tabel tersendiri milik Laboratorium** yang menunjuk ke `MstProcedure`. Satu pemeriksaan boleh punya beberapa baris batas menurut jenis kelamin dan kelompok umur. `MstProcedure` tidak diubah | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-14. Menutup `Q-LAB-05` |
| `LAB-OPEN-011` | Open Question | Apa yang terjadi bila hasil perlu dikoreksi setelah kunjungan pasien ditutup dan dokumennya sudah terkunci oleh `ClinicalDocumentLockTrigger.EncounterClosed`? | Yoga Aji Pratama + pemilik `rekam-medis` + Clinical Governance | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-020` | Pertemuan antara `LAB-DEC-007` dan aturan penguncian rekam medis |
| `LAB-AMD-001` | Open Question | `LAB-INH-001` dan `LAB-INH-006` pada `RJ-BIL-GATE-DEC-003` perlu diamandemen: `Draft` dihapus, dan batas kewenangan dokter diubah menjadi "sampai sampel pertama diambil" | Pemilik blueprint `rawat-jalan` + Billing | `open` | — | Konsekuensi `LAB-DEC-015`; keputusan aslinya milik blueprint lain sehingga tidak boleh diubah dari sini |
| `LAB-OPEN-002` | Open Question | Di mana `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` yang disebut `AGENTS.md`? | Pemilik repository backend | `closed` | Ditutup 2026-09-01 oleh `LAB-FACT-007`: keduanya **masih berlaku**, berada di `QuilvianEngineeringSkills/agents/rules/backend/engineering/`; path `docs/engineering/` usang | Membuka `LAB-OPEN-018` dan `LAB-OPEN-019` |
| `LAB-OPEN-018` | Open Question | Kapan suite Skill yang memuat `rules/backend/engineering/` dipublikasikan ke marketplace yang benar-benar terpasang, sehingga rules root runtime memenuhi `AGENTS.md`? Selama belum, setiap task backend wajib berhenti dengan `BLOCKED — canonical governance unavailable` menurut gerbang kegagalan `AGENTS.md` sendiri | Pemilik repository backend + pemilik suite Skill | `open` | — | `LAB-FACT-009`. Dua repo berbeda: terpasang `MHamzah1/QuilvianEngineeringSkillsClaude@f0136df`, canonical `DevBenari/QuilvianEngineeringSkills@59bd3e2` |
| `LAB-OPEN-019` | Open Question | Apakah lifecycle `LaboratoryManagement` pada registry dinaikkan dari `PLANNED` menjadi `ACTIVE`? Tanpa itu `QBE-MOD-002` dan `QBE-MOD-003` menahan pembuatan entity operasional `Lab*` pertama | Pemilik repository backend | `open` | — | `LAB-FACT-010`. Preseden: `RWI-DEC-068` menaikkan `InPatientManagement` dari `PLANNED` ke `ACTIVE` |
| `LAB-OPEN-003` | Open Question | Apakah bank darah dan transfusi masuk modul Laboratorium? | Pemilik proses Laboratorium | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-002`: **tidak masuk** | Belum ada modul bank darah di backend |
| `LAB-DEC-009` | Decision | **Laboratorium melayani Rawat Jalan, Rawat Inap, dan IGD sekaligus sejak Rilis 1.** Konsekuensinya pesanan wajib punya penanda tingkat kesegeraan minimal "biasa" dan "cito" | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-07 |
| `LAB-DEC-010` | Decision | **Wewenang UI:** letak menu dan penamaan route mengikuti pola modul Health Services yang sudah ada. Rincian tata letak, tab/modal/drawer, dan warna menjadi `DEV_DISCRETION`. Penanda keselamatan pada BR-01, BR-02, BR-06, dan BR-07 tetap wajib dan tidak boleh diperlemah | Pemilik produk/UI | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat tabel Frontend Decision Authority |
| `LAB-OPEN-004` | Open Question | Apakah pemakaian dan stok reagen dikelola Laboratorium atau Farmasi/Inventory? | Laboratorium + Farmasi | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-014` | Belum ada tabel stok reagen |
| `LAB-DEC-011` | Decision | **Wewenang klinis terpisah.** Yoga Aji Pratama mengesahkan sisi produk dan operasional. `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007` tetap memerlukan tanda tangan dokter penanggung jawab laboratorium atau Komite Medis sebelum desain final | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Ketiga keputusan itu menentukan perilaku sistem saat hasil salah atau pasien dalam bahaya |
| `LAB-DEC-012` | Decision | **Pemberitahuan tersimpan dibangun di Rilis 1.** Setiap pemberitahuan nilai kritis dan koreksi hasil disimpan sebagai data milik dokter tujuan, lengkap dengan status sudah dibaca. SignalR hanya pelengkap agar muncul seketika | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-08. Dasar bukti: F7 |
| `LAB-DEC-013` | Decision | **Cito ditandai dokter pemesan, dengan batas waktu penyelesaian per jenis pemeriksaan** yang disimpan bersama tabel batas nilai. Ada daftar pantau pesanan cito yang lewat batas | Yoga Aji Pratama | `amended` oleh `LAB-DEC-026` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-09. Menutup `LAB-OPEN-007` |
| `LAB-DEC-014` | Decision | **Stok, pembelian, dan pencatatan pemakaian reagen berada di luar modul Laboratorium**, diserahkan ke Farmasi/Inventory | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-10. Menutup `LAB-OPEN-004` |
| `LAB-OPEN-007` | Open Question | Apa definisi resmi "cito", siapa yang boleh menandai pesanan sebagai cito, dan berapa batas waktu penyelesaiannya? | Yoga Aji Pratama | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-013` | Konsekuensi langsung `LAB-DEC-009`; belum ada kolom kesegeraan pada `LabOrder` (F1) |
| `LAB-OPEN-008` | Open Question | Lewat sarana apa pemberitahuan koreksi hasil dan pemberitahuan nilai kritis sampai ke dokter? | Yoga Aji Pratama + pemilik platform | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-012` | Konsekuensi `LAB-DEC-004` dan `LAB-DEC-007`. Bukti F7: platform belum punya sarana notifikasi tersimpan |
| `LAB-OPEN-009` | Open Question | Apakah Yoga Aji Pratama sebagai pemilik modul juga memegang wewenang Clinical Governance untuk mengesahkan `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007`, atau ketiganya masih memerlukan tanda tangan pihak klinis terpisah? | Manajemen rumah sakit | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-011` | Ketiga keputusan itu menyangkut keselamatan pasien, bukan sekadar operasional |

---

## Acceptance Criteria

Kriteria berikut sudah dapat diuji. Nomor acuannya dipakai kembali saat menyusun roadmap dan
pengujian.

| No | Kriteria yang dapat diuji | Acuan |
|---:|---|---|
| AC-01 | Petugas yang mengisi hasil, ketika menekan Validasi pada hasil yang sama, ditolak sistem kecuali ia mengisi alasan pengecualian dari daftar terkendali | BR-01 |
| AC-02 | Hasil yang divalidasi lewat jalur pengecualian menampilkan penanda "divalidasi oleh pengisi sendiri" di layar, di cetakan, dan di riwayat | BR-01 |
| AC-03 | Hasil dengan nilai melewati batas kritis tetap berpindah ke status `Released`, dan sekaligus memunculkan formulir pelaporan wajib | BR-02, BR-04 |
| AC-04 | Selama formulir pelaporan nilai kritis belum terisi lengkap, pemeriksaan itu muncul pada daftar pantau "nilai kritis belum dilaporkan" | BR-02 |
| AC-05 | Hasil dengan nilai di luar batas normal tetapi belum melewati batas kritis ditandai "di atas/di bawah nilai rujukan" **tanpa** memunculkan formulir pelaporan | BR-04 |
| AC-06 | Analis biasa yang mencoba mengoreksi hasil berstatus `Released` ditolak sistem | BR-05 |
| AC-07 | Setelah hasil koreksi dirilis ulang, versi lama tetap dapat dilihat dengan tanda "sudah diperbaiki", dan dokter pemesan menerima pemberitahuan | BR-05 |
| AC-08 | Dalam satu pesanan berisi beberapa pemeriksaan, satu pemeriksaan dapat berstatus `Released` sementara pemeriksaan lain masih `InProcess` | BR-06 |
| AC-09 | Lembar hasil yang belum lengkap menampilkan peringatan "hasil belum lengkap" | BR-06 |
| AC-10 | Pesanan bertanda cito muncul di urutan lebih atas daripada pesanan biasa pada daftar kerja petugas, tanpa memandang jam masuk | BR-07 |
| AC-11 | Pesanan lab dapat dibuat dari kunjungan Rawat Jalan, Rawat Inap, maupun IGD dengan alur kerja yang sama | BR-07 |
| AC-12 | Fakta kelayakan tagih diterbitkan tepat pada transisi sampel ke `Accepted`, dan tidak bergantung pada dirilisnya hasil | `LAB-INH-009` |
| AC-13 | Tidak ada satu pun endpoint atau kolom Laboratorium yang menghitung, mengubah, membatalkan, atau mengembalikan uang | `LAB-INH-012` |
| AC-14 | Pemberitahuan nilai kritis tetap tersimpan dan terbaca oleh dokter meskipun dokter itu tidak sedang membuka aplikasi saat hasil keluar | BR-08 |
| AC-15 | Setiap pemberitahuan menyimpan status sudah dibaca atau belum, beserta waktu dibacanya | BR-08 |
| AC-16 | Pemberitahuan koreksi hasil terkirim ke dokter pemesan setiap kali hasil perbaikan dirilis ulang | BR-05, BR-08 |
| AC-17 | Pesanan cito yang melewati batas waktu penyelesaiannya muncul pada daftar pantau keterlambatan, dihitung sejak sampel `Accepted` sampai hasil `Released` | BR-09 |
| AC-18 | Penandaan cito hanya dapat dilakukan dokter pemesan saat membuat pesanan, bukan otomatis berdasarkan unit asal | BR-09 |
| AC-19 | Tidak ada satu pun tabel atau endpoint Laboratorium yang menyimpan stok, pembelian, atau pemakaian reagen | BR-10 |
| AC-20 | Dokter pemesan dapat menyunting daftar pemeriksaan pada pesanannya selama belum ada sampel berstatus `Collected`, dan ditolak sistem setelah sampel pertama diambil | BR-11 |
| AC-21 | Tidak ada satu pun pesanan yang pernah berstatus `Draft`; nilai itu tidak lagi ada pada siklus hidup pesanan | BR-11 |
| AC-22 | Struktur data pemberitahuan tidak memuat satu pun istilah khas laboratorium, sehingga modul lain dapat memakainya tanpa perubahan | BR-12 |
| AC-23 | Setiap hasil yang dirilis memiliki satu baris pencatatan keutuhan di rekam medis, dan angka hasilnya tidak digandakan ke tabel rekam medis mana pun | BR-13 |
| AC-24 | Satu jenis pemeriksaan dapat memiliki lebih dari satu baris batas nilai yang dibedakan menurut jenis kelamin dan kelompok umur | BR-14 |
| AC-25 | `MstProcedure` tidak bertambah satu pun kolom **operasional** laboratorium — satuan, batas nilai, jenis wadah. Satu-satunya tambahan yang diizinkan adalah kolom klasifikasi disiplin | BR-14, BR-32 |
| AC-26 | Kepala instalasi dapat menambah dan menonaktifkan alasan penolakan, tetapi percobaan mengubah penanda kesalahan internal atau penanda wajib catatan ditolak sistem | BR-15 |
| AC-27 | Koreksi hasil pada kunjungan yang sudah ditutup menghasilkan addendum bertanda tangan beralasan, sementara dokumen hasil aslinya tetap terkunci dan isinya tidak berubah | BR-16 |
| AC-28 | Pemeriksaan berhasil pilihan hanya menerima nilai dari daftar pilihan yang sah; pengetikan bebas ditolak sistem | BR-17 |
| AC-29 | Protein urin bernilai `+4` memicu formulir pelaporan nilai kritis persis seperti Kalium 7,2 mmol/L memicunya | BR-17, BR-02 |
| AC-30 | Pemeriksaan berhasil pilihan tanpa nilai kritis, seperti golongan darah, tidak pernah memunculkan formulir pelaporan | BR-17 |
| AC-31 | Kewenangan validasi dan kewenangan rilis dapat diberikan kepada orang yang berbeda, dan pemberiannya tidak mengikuti jabatan | BR-18 |
| AC-32 | Kepala instalasi menerima peringatan bila suatu shift hanya memiliki satu pemegang kewenangan validasi | BR-18 |
| AC-33 | Perubahan batas normal oleh kepala instalasi langsung berlaku, sedangkan perubahan batas kritis tertahan sebagai pengajuan sampai disetujui pihak klinis | BR-19 |
| AC-34 | Setiap perubahan batas nilai menyimpan kolom yang berubah, nilai lama, nilai baru, pelaku, penyetuju, waktu, dan alasan | BR-19 |
| AC-35 | Satu wadah fisik dapat menopang lebih dari satu pemeriksaan terpesan, dan hanya memiliki satu barcode | BR-20 |
| AC-36 | Menolak sebuah wadah menggugurkan seluruh pemeriksaan yang ditopangnya; percobaan menolak sebagian ditolak sistem | BR-20 |
| AC-37 | Menyatakan satu wadah layak menerbitkan kelayakan tagih sebanyak pemeriksaan yang ditopangnya, masing-masing dengan tarifnya sendiri | BR-20, `LAB-INH-009` |
| AC-38 | Ambil ulang menciptakan wadah baru yang menampung seluruh pemeriksaan dari wadah lama, sementara wadah lama tetap terlihat beserta tautan sebabnya | BR-20, `LAB-INH-005` |
| AC-39 | Satu pesanan dapat memuat pemeriksaan bertanda cito dan pemeriksaan biasa sekaligus; hanya yang bertanda cito naik ke urutan atas daftar kerja | BR-22 |
| AC-40 | Penanda cito dan duplo hanya dapat disetel pada baris pemeriksaan; percobaan menyetelnya pada pesanan ditolak sistem | BR-22 |
| AC-41 | Pesanan menyimpan disiplinnya, dan daftar pantau dapat disaring per disiplin: Patologi Klinik, Patologi Anatomi, atau Mikrobiologi | BR-21 |
| AC-42 | Tidak ada satu pun tabel atau endpoint Laboratorium yang melayani Bank Darah | BR-21 |
| AC-43 | Layar pemesanan menampilkan harga satuan, subtotal, total, dan status cakupan, tanpa membentuk tagihan apa pun | BR-25 |
| AC-44 | Pendaftaran pasien datang langsung dari layar Laboratorium menghasilkan kunjungan yang dibuat modul Registrasi, bertanda datang langsung, dan pesanan lab menempel padanya | BR-28 |
| AC-45 | Tidak ada satu pun kode Laboratorium yang menulis ke tabel kunjungan maupun tabel pasien | BR-28 |
| AC-46 | Pendaftaran pasien rujukan luar menyimpan dokter perujuk, instansi perujuk, dan nomor surat rujukan pada kunjungan yang dibuat Registrasi | BR-28 |
| AC-47 | Modul Laboratorium tidak memiliki tabel tarif sendiri; harga selalu berasal dari `MstTariff` | BR-29 |
| AC-48 | Menu `Tarif Laboratorium` bersifat baca saja; percobaan mengubah tarif dari modul Laboratorium ditolak sistem | BR-29 |
| AC-49 | Pada **backend**, data induk khusus Laboratorium berada di folder Laboratorium dan data induk global tidak disalin ke sana. Pada **frontend**, seluruh menu data induk berada di `health-services/master-data/` | BR-30 |
| AC-50 | Nama instansi dan dokter perujuk dipilih dari daftar terkendali, bukan diketik bebas; kunjungan menyimpan penunjuk, bukan teks | BR-31 |
| AC-51 | Menambahkan pemeriksaan yang disiplinnya tidak sesuai disiplin pesanan ditolak sistem | BR-32, `INV-22` |

---

## Open Questions dan Blocker

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
> **Perubahan besar sejak revision 13.** Analisis konsolidasi bukti lapangan diadopsi sebagai
> baseline requirement (`LAB-DEC-025` sampai `LAB-DEC-031`). Cakupan modul **bertambah tiga
> kali lipat**, dan delapan hal tata kelola yang sebelumnya tidak terlihat kini terbuka sebagai
> `LAB-P0-001` sampai `LAB-P0-008`. Daftar di bawah sudah memperhitungkannya.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-P0-001` | Matriks kewenangan per peran | Yoga Aji Pratama + Clinical Governance | `DESIGN` — seluruh slice hasil dan pembatalan |
| `LAB-P0-002` | Urutan status resmi, termasuk posisi `Confirmed` | Yoga Aji Pratama | `DESIGN` — siklus hidup pesanan |
| `LAB-P0-003` | Aturan pembatalan dan koreksi | Yoga Aji Pratama + Billing | `DESIGN` — pembatalan dan koreksi |
| `LAB-P0-004` | Alur nilai kritis lengkap | Yoga Aji Pratama + Clinical Governance | `DESIGN` — slice nilai kritis |
| `LAB-P0-005` | Integrasi alat laboratorium | Yoga Aji Pratama + pemilik platform | `DESIGN` — slice hasil Patologi Klinik |
| `LAB-P0-006` | Kebijakan jejak audit | Yoga Aji Pratama | `IMPLEMENTATION` |

### Koordinasi lintas modul — disetujui 2026-09-01

Lima butir ditutup lewat `LAB-REQ-001`, disetujui `andryzainhome` (`andryzain01@gmail.com`) dan
`sukmagp` — Sukma Giri Pratama (`sukmagiri11@gmail.com`) selaku pemilik repository.

| ID | Isi | Membuka |
|---|---|---|
| `LAB-COORD-001` | Pemberitahuan sebagai kemampuan platform | Prasyarat `S5` dan `S8` |
| `LAB-COORD-002` | Jenis dokumen klinis baru pada `rekam-medis` | Prasyarat `S6`, `S9`, `S18` |
| `LAB-COORD-003` | Kontrak pemanggilan Registrasi | `MVP-1` |
| `LAB-COORD-004` | Data induk perujuk + kolom penunjuk pada kunjungan | `MVP-1` |
| `LAB-COORD-005` | Kolom disiplin pada `MstProcedure` | `MVP-0` |

**Yang tetap terbuka meski permintaannya disetujui:**

| ID | Kenapa persetujuan belum cukup |
|---|---|
| `LAB-OPEN-012` | Yang dibutuhkan adalah **satu angka** — jumlah baris `TrxLabSpecimen` di produksi. Persetujuan tidak memberi tahu berapa |
| `LAB-OPEN-018` | Lokasi dokumen sudah diketahui, tetapi rules root yang **terpasang** belum memuatnya. Yang dibutuhkan adalah publikasi/pembaruan suite Skill, bukan persetujuan |
| `LAB-OPEN-019` | Yang dibutuhkan adalah **kenaikan lifecycle registry** `PLANNED` → `ACTIVE` untuk `LaboratoryManagement`, dan itu wewenang pemilik repository backend |
| `LAB-SIGN-001` | Pemilik repository **bukan** wewenang klinis. `LAB-DEC-011` yang disetujui pemilik modul sendiri mensyaratkan tanda tangan dokter penanggung jawab laboratorium atau Komite Medis |
| `LAB-P0-007` | Aturan tagihan dan cakupan | Billing | `DESIGN` — tampilan tarif dan cakupan |
| `LAB-P0-008` | Penyelarasan antaraplikasi | Yoga Aji Pratama + pemilik platform | `DESIGN` — batas integrasi |
| `LAB-OPEN-013` | Dampak Cito dan Duplo pada tarif | Yoga Aji Pratama + Billing | `DESIGN` — bagian penandaan pemeriksaan |
| `LAB-OPEN-014` | Nilai kritis untuk hasil mikrobiologi dan patologi anatomi | Yoga Aji Pratama + Clinical Governance | `DESIGN` — slice nilai kritis |
| `LAB-COORD-003` | Kesepakatan kontrak pemanggilan Registrasi dari Laboratorium | Yoga Aji Pratama + pemilik `registration-management` | `DESIGN` — slice pendaftaran `S13` |
| `LAB-OPEN-017` | Makna penanda Definitif | Yoga Aji Pratama | `LATER SLICE` |
| `DEC-LAB-009` | Di mana identitas dokter dan instansi perujuk disimpan? | Yoga Aji Pratama + pemilik `registration-management` | `DESIGN` — memblokir `S13b` pendaftaran rujukan luar |
| `DEC-LAB-010` | Bagaimana disiplin melekat pada jenis pemeriksaan? `MstProcedure` hanya punya `IsLaboratory` tanpa pembeda disiplin | Yoga Aji Pratama + pemilik `master-data` | `DESIGN` — memblokir penegakan `INV-22` |
| `DEC-LAB-008` | Apakah satu wadah fisik dapat melayani beberapa pemeriksaan? | Yoga Aji Pratama + kepala instalasi laboratorium | **Ditutup** `LAB-DEC-024` |
| `LAB-SIGN-001` | Tanda tangan klinis untuk `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007`, sesuai `LAB-DEC-011` | Dokter penanggung jawab laboratorium atau Komite Medis | `DESIGN` — **hanya bagian validasi hasil, nilai kritis, dan koreksi hasil**. Bagian lain boleh maju |
| `LAB-AMD-001` | Amandemen `LAB-INH-001` dan `LAB-INH-006` pada blueprint `rawat-jalan`: `Draft` dihapus, batas kewenangan dokter menjadi "sampai sampel pertama diambil" | Pemilik blueprint `rawat-jalan` + Billing | `DESIGN` — **hanya bagian pembuatan dan penyuntingan pesanan** |
| `LAB-COORD-001` | Kesepakatan dengan pemilik platform soal kemampuan pemberitahuan bersama | Yoga Aji Pratama + pemilik platform | `DESIGN` — **hanya bagian pemberitahuan** |
| `LAB-COORD-002` | Kesepakatan dengan pemilik `rekam-medis` untuk menambah jenis dokumen klinis baru | Yoga Aji Pratama + pemilik `rekam-medis` | `DESIGN` — **hanya bagian penyajian hasil ke rekam medis** |
| `LAB-OPEN-018` | Rules root terpasang belum memuat `rules/backend/engineering/`; gerbang `AGENTS.md` memaksa `BLOCKED — canonical governance unavailable` | Pemilik repository backend + pemilik suite Skill | `IMPLEMENTATION` |
| `LAB-OPEN-019` | Lifecycle registry `LaboratoryManagement` masih `PLANNED`; `QBE-MOD-002`/`QBE-MOD-003` menahan entity `Lab*` pertama | Pemilik repository backend | `IMPLEMENTATION` |

Keempat butir `DESIGN` di atas bersifat **sebagian**: masing-masing hanya menahan bagian yang
disebutkan, bukan seluruh desain. Bagian lain — siklus hidup sampel, batas nilai, cito, daftar
kerja, dan batas kewenangan finansial — sudah bebas hambatan.

Seluruh open question wawancara sudah tertutup pada sesi ini:

| ID | Cara ditutup |
|---|---|
| `LAB-SCOPE-001` | Digantikan `LAB-DEC-001` dan `LAB-DEC-002` |
| `LAB-OPEN-001` | Pemilik modul ditetapkan: Yoga Aji Pratama |
| `LAB-OPEN-003` | Ditutup `LAB-DEC-002` — bank darah di luar scope |
| `LAB-OPEN-004` | Ditutup `LAB-DEC-014` — reagen di luar scope |
| `LAB-OPEN-005` | Ditutup `LAB-DEC-007` — kewenangan koreksi hasil |
| `LAB-OPEN-007` | Ditutup `LAB-DEC-013` — aturan cito |
| `LAB-OPEN-008` | Ditutup `LAB-DEC-012` — pemberitahuan tersimpan |
| `LAB-OPEN-009` | Ditutup `LAB-DEC-011` — wewenang klinis terpisah |
| `LAB-CONFLICT-001` | Diselesaikan `LAB-DEC-006` — tabel batas nilai ditarik ke Rilis 1 |
| `LAB-OPEN-011` | Ditutup `LAB-DEC-020` — koreksi setelah kunjungan ditutup memakai addendum |
| `LAB-CONFLICT-002` | Diselesaikan `LAB-DEC-015` — status `Draft` dihapus, dokter menyunting sampai sampel diambil |
| `Q-LAB-01` sampai `Q-LAB-05` | Pertanyaan penutup dari capability map, ditutup `LAB-DEC-015` sampai `LAB-DEC-019` |

Yang tersisa bukan pertanyaan wawancara, melainkan dua hal administratif: tanda tangan klinis
dan kelengkapan dokumen tata kelola repository.

### Catatan approval

**Pemilik modul:** Yoga Aji Pratama (`yogaaji452@gmail.com`), ditetapkan 2026-09-01.

Keputusan `LAB-DEC-001` sampai `LAB-DEC-036` berstatus `approved` oleh pemilik modul, kecuali `LAB-DEC-002` dan `LAB-DEC-021` yang `superseded` serta `LAB-DEC-013` yang `amended`
pada 2026-09-01. Yang perlu dicatat jujur tentang persetujuan ini:

1. Persetujuan diberikan lisan dalam sesi wawancara, bukan lewat dokumen bertanda tangan.
   Bila rumah sakit memerlukan bukti tertulis untuk keperluan audit atau akreditasi, dokumen
   ini perlu dicetak dan ditandatangani.
2. Pemilik modul menyatakan lewat `LAB-DEC-011` bahwa **wewenang klinis berada di pihak lain**.
   Karena itu tiga keputusan berikut sudah disetujui dari sisi produk dan operasional, tetapi
   masih **menunggu tanda tangan dokter penanggung jawab laboratorium atau Komite Medis**:

   | Keputusan | Isi | Kenapa perlu tanda tangan klinis |
   |---|---|---|
   | `LAB-DEC-003` | Prinsip empat mata pada validasi hasil | Menentukan siapa yang boleh menyatakan sebuah angka hasil benar |
   | `LAB-DEC-004` | Nilai kritis tetap dirilis, pelaporan wajib tercatat | Menentukan apa yang terjadi ketika pasien dalam bahaya |
   | `LAB-DEC-007` | Kewenangan koreksi hasil dan pemberitahuan ke dokter | Menentukan apa yang terjadi ketika hasil yang sudah dipakai ternyata salah |

   Dicatat sebagai `LAB-SIGN-001`. Ini **tidak** memblokir seluruh desain — hanya bagian
   validasi hasil, nilai kritis, dan koreksi hasil yang harus menunggu.
3. Keputusan warisan `RJ-BIL-GATE-DEC-003` berstatus `locked-draft` dengan tata kelola formal
   `OPEN` di blueprint `rawat-jalan`. Statusnya tidak berubah oleh sesi ini.

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 19 | 2026-09-01 | **`LAB-OPEN-002` ditutup lewat temuan faktual.** Kedua dokumen tata kelola backend ditemukan di `QuilvianEngineeringSkills/agents/rules/backend/engineering/` dan dinyatakan **masih berlaku**; path `docs/engineering/` pada `AGENTS.md` usang. `LAB-FACT-007` sampai `LAB-FACT-010` ditulis. Penutupan ini membuka dua penghambat implementasi baru: `LAB-OPEN-018` (rules root terpasang belum memuat kedua dokumen) dan `LAB-OPEN-019` (lifecycle registry Laboratorium masih `PLANNED`) | `draft` |
| 1 | 2026-09-01 | Scope pass dibuka. Fakta source code dicatat, keputusan warisan dari `RJ-BIL-GATE-DEC-003` dikutip, batas scope diajukan untuk dikonfirmasi | `draft` |
| 2 | 2026-09-01 | Batas scope dikunci lewat `LAB-DEC-001` (rilis 1 sampai hasil dirilis) dan `LAB-DEC-002` (Patologi Klinik saja). `LAB-SCOPE-001` dan `LAB-OPEN-003` ditutup | `draft` |
| 3 | 2026-09-01 | Invariant hasil dikunci: `LAB-DEC-003` prinsip empat mata, `LAB-DEC-004` nilai kritis, `LAB-DEC-005` hasil diketik manual. Risiko `LAB-RISK-001` dicatat | `draft` |
| 4 | 2026-09-01 | `LAB-CONFLICT-001` ditemukan dan diselesaikan `LAB-DEC-006` (tabel batas nilai ditarik ke Rilis 1). `LAB-DEC-007` koreksi hasil dan `LAB-DEC-008` rilis sebagian ditambahkan | `draft` |
| 5 | 2026-09-01 | `LAB-DEC-009` cakupan unit dan `LAB-DEC-010` wewenang UI ditambahkan. Acceptance criteria AC-01 sampai AC-13 ditulis. Scope pass ditutup | `draft` |
| 6 | 2026-09-01 | Yoga Aji Pratama ditetapkan sebagai pemilik modul, `LAB-OPEN-001` ditutup. `LAB-DEC-001` sampai `LAB-DEC-010` naik status menjadi `approved`. Fakta F7 tentang ketiadaan sarana notifikasi ditambahkan. `LAB-OPEN-009` dibuka | `draft` |
| 7 | 2026-09-01 | `LAB-DEC-011` wewenang klinis terpisah, `LAB-DEC-012` pemberitahuan tersimpan, `LAB-DEC-013` aturan cito, dan `LAB-DEC-014` reagen di luar scope ditambahkan. BR-08 sampai BR-10 dan AC-14 sampai AC-19 ditulis. Scope pass ditutup | `draft` |
| 8 | 2026-09-01 | **Closure pass dibuka** setelah `01-existing-capability-map.md` revision 1 terbit. Frontend SHA diperbarui dari `c79bb6ee4` menjadi `688daff90` | `draft` |
| 19 | 2026-09-01 | `LAB-OPEN-002` ditutup: dokumen tata kelola ternyata ada pada `c9692d0`, checkout lokal 7 commit tertinggal. Pembacaan dokumen itu membongkar pelanggaran `QBE-NAM-001` pada rancangan sendiri — tiga entity baru berawalan `Trx*` diganti menjadi `LabExamination`, `LabValueBoundChangeRequest`, `LabValueBoundHistory`. Dua temuan baru dibuka: `LAB-OPEN-018` prefix data induk, `LAB-OPEN-019` lifecycle `PLANNED` | `draft` |
| 18 | 2026-09-01 | Lima koordinasi lintas modul `LAB-COORD-001` sampai `LAB-COORD-005` ditutup, disetujui `andryzainhome` dan `sukmagp` lewat `LAB-REQ-001`. `LAB-OPEN-012`, `LAB-OPEN-002`, dan `LAB-SIGN-001` tetap terbuka karena memerlukan jawaban faktual atau wewenang klinis | `draft` |
| 17 | 2026-09-01 | `LAB-DEC-034` dipersempit menjadi **backend saja** atas arahan pemilik modul. Frontend tetap memakai konvensi yang sudah ada: seluruh menu data induk di `health-services/master-data/`. AC-49 dan `LAB-FE-014` disesuaikan | `draft` |
| 16 | 2026-09-01 | `DEC-LAB-009` ditutup `LAB-DEC-035` (sumber rujukan jadi data induk global) dan `DEC-LAB-010` ditutup `LAB-DEC-036` (kolom disiplin pada `MstProcedure`). BR-31 dan BR-32 ditulis; AC-25 diamandemen; AC-49 sampai AC-51 ditambahkan. `LAB-COORD-004` dan `LAB-COORD-005` dibuka | `draft` |
| 15 | 2026-09-01 | `LAB-DEC-034` penempatan data induk menurut cakupan pemakaian, berlaku backend dan frontend. BR-30 ditulis. Catatan utang teknis "master di dalam folder submodul" dicabut karena penempatan itu justru yang benar | `draft` |
| 14 | 2026-09-01 | `LAB-OPEN-015` ditutup `LAB-DEC-032`: layar pendaftaran milik Laboratorium, pembuatan kunjungan tetap milik Registrasi. `LAB-OPEN-016` ditutup `LAB-DEC-033`: tarif tetap milik Master Data, menu `Tarif Laboratorium` menjadi tampilan baca saja. BR-28 dan BR-29 serta AC-44 sampai AC-48 ditulis. `LAB-COORD-003` dibuka | `draft` |
| 13 | 2026-09-01 | **Amendment pass.** Analisis konsolidasi bukti lapangan diadopsi sebagai baseline requirement. `LAB-DEC-025` sampai `LAB-DEC-031` ditambahkan; `LAB-DEC-002` dan `LAB-DEC-021` ditandai `superseded`, `LAB-DEC-013` ditandai `amended`. BR-21 sampai BR-27 ditulis. Tiga belas open question baru dibuka, delapan di antaranya tata kelola `LAB-P0-*` | `draft` |
| 12 | 2026-09-01 | `DEC-LAB-008` ditutup `LAB-DEC-024`: wadah fisik dipisahkan dari pemeriksaan terpesan. BR-20 dan AC-35 sampai AC-38 ditulis. `LAB-OPEN-012` dibuka untuk memverifikasi jumlah data lab yang sudah terisi | `draft` |
| 11 | 2026-09-01 | `DEC-LAB-008` dibuka dari `03-domain-architecture.md`: wadah fisik dan pemeriksaan terpesan menyatu dalam satu konsep. Memblokir arsitektur target `S2`, `S7`, `S10`, dan menentukan bentuk data hasil `S4` | `draft` |
| 10 | 2026-09-01 | Closure pass putaran kedua, menutup tiga gap yang ditemukan `02-requirement-completeness-assessment.md`. `LAB-DEC-021` bentuk hasil dua macam, `LAB-DEC-022` pemegang kewenangan validasi, `LAB-DEC-023` perlindungan batas kritis. BR-17 sampai BR-19 dan AC-28 sampai AC-34 ditulis | `draft` |
| 9 | 2026-09-01 | Closure pass selesai. `LAB-CONFLICT-002` diselesaikan `LAB-DEC-015`. `LAB-DEC-016` sampai `LAB-DEC-020` menutup `Q-LAB-01` sampai `Q-LAB-05` dan `LAB-OPEN-011`. BR-11 sampai BR-16 dan AC-20 sampai AC-27 ditulis. Empat butir koordinasi lintas modul dibuka: `LAB-AMD-001`, `LAB-COORD-001`, `LAB-COORD-002`, dan `LAB-SIGN-001` | `draft` |
