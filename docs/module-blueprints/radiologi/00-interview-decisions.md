# Radiologi — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `radiologi` |
| Revision | `8` |
| Status | `draft` |
| Pass | `Scope pass` dan `Closure pass` selesai. `Amendment pass` **selesai** 2026-09-09 |
| Product/domain owner | **Yoga Aji Pratama** (`yogaaji452@gmail.com`) — bertindak sebagai pemilik modul pada sesi ini |
| Clinical governance owner | **Belum ditunjuk** (lihat `RAD-OPEN-001`) |
| Backend SHA | `64da911` |
| Frontend SHA | `f66ed1885` |
| Tanggal sesi | 2026-09-09 |
| Capability map | `01-existing-capability-map.md` revision 1, audit pada BE `64da911` + FE `f66ed1885` |
| Task mode | `MODULE BLUEPRINT MODE` — hanya `docs/module-blueprints/**` yang boleh ditulis |

> **Cara membaca dokumen ini.**
> Dokumen ini adalah catatan wawancara. Dokumen ini **bukan** desain, **bukan** rencana kerja,
> dan **bukan** izin menulis kode.
>
> Isinya memisahkan lima jenis catatan:
>
> | Jenis | Artinya |
> |---|---|
> | **Fact** | Fakta yang dibuktikan langsung dari source code atau dokumen yang sudah disetujui |
> | **Decision** | Keputusan manusia yang berwenang |
> | **Assumption** | Dugaan yang belum dikonfirmasi siapa pun |
> | **Conflict** | Dua sumber yang saling bertentangan dan harus diselesaikan |
> | **Open Question** | Pertanyaan yang masih menunggu jawaban pemilik proses |

---

## Peringatan Prasyarat

Tiga hal berikut wajib dibaca sebelum isi dokumen ini dipakai untuk apa pun.

### 1. Scope dikunci sebelum audit, lalu audit menyusul — dan hasilnya sejalan

Batas scope pada dokumen ini dikunci lewat `RAD-DEC-001` dan `RAD-DEC-002` **sebelum** audit
kemampuan existing dijalankan. Audit itu kemudian dijalankan pada hari yang sama dan hasilnya
tercatat di `01-existing-capability-map.md` revision 1.

Audit **membenarkan** daftar "sudah ada" dan "belum ada" di bawah, tetapi menambahkan tiga hal
penting yang tidak terlihat pada pembacaan cepat:

1. Modul ini **belum bisa menjalankan satu pun pemeriksaan** karena aturan keselamatan tidak
   dapat dimasukkan lewat cara apa pun (`RAD-CAP-003`, status `Repair`).
2. Konflik registry ternyata lebih dalam dari dugaan awal — lihat `RAD-CONFLICT-001` yang sudah
   diperbarui di bawah.
3. Muncul konflik kedua yang sebelumnya tidak diketahui: modul IGD masih menyatakan modul
   Radiologi belum ada (`RAD-CONFLICT-002`).

### 2. Sebagian besar aturan Radiologi sudah dikunci lebih dulu di modul lain

Amendment `RJ-BIL-GATE-DEC-004` pada blueprint `rawat-jalan` sudah mengunci siklus hidup
pesanan radiologi, study/acquisition, laporan hasil, gerbang keselamatan, pengulangan,
pembatalan di tengah jalan, dan kelayakan tagih.

Wawancara ini **tidak boleh membuka ulang** keputusan tersebut. Yang digali di sini hanya
bagian Radiologi yang belum pernah ditanyakan kepada siapa pun.

Contoh supaya jelas bedanya:

- **Tidak boleh ditanya ulang:** "Apakah gerbang keselamatan boleh dilewati?" — sudah dikunci,
  jawabannya tidak, kecuali ada pengesahan terpisah.
- **Boleh dan harus ditanya:** "Siapa yang berwenang mengesahkan pelewatan gerbang keselamatan
  dalam keadaan darurat, dan bukti apa yang wajib direkam?" — ini belum pernah diputuskan.

### 3. Status tata kelola `RJ-BIL-GATE-DEC-004` masih `OPEN`

Keputusan induk itu berstatus `locked-draft` dengan catatan
`Formal governance status: OPEN — Radiology, Clinical Governance, dan Billing/Finance sign-off
belum dilampirkan`.

Artinya: aturannya mengikat pekerjaan teknis, tetapi tanda tangan resmi dari pihak klinis dan
keuangan belum ada. Ini menjadi `RAD-OPEN-001`.

---

## Batas Scope Modul (dikunci pemilik modul 2026-09-09)

> **Status daftar di bawah: sudah dikonfirmasi** lewat `RAD-DEC-001` dan `RAD-DEC-002` pada
> 2026-09-09. Setiap pertanyaan wawancara berikutnya wajib berada di dalam batas ini.

**Modul:** Radiologi (`radiologi`).

**Satu kalimat batas scope:**
Modul Radiologi mengurus perjalanan pemeriksaan pencitraan mulai dari pesanan dokter,
penjadwalan, pemeriksaan identitas dan keselamatan pasien, pengambilan citra, penilaian mutu
citra, sampai hasil bacaan dokter radiolog dinyatakan sah dan dirilis — beserta bukti siapa
melakukan apa dan kapan.

Yang **tidak** termasuk kalimat itu: perhitungan uang, stok barang, dan penyimpanan berkas
citra di sistem eksternal.

### Di dalam scope

| No | Kemampuan | Penjelasan singkat untuk pembaca non-teknis | Keadaan di source `64da911` | Rilis |
|---:|---|---|---|---|
| 1 | Pesanan radiologi (*rad order*) | Dokter memesan foto atau pemindaian untuk satu kunjungan pasien | **Sudah ada** | Sudah ada |
| 2 | Siklus hidup pesanan | Diterima radiologi, dijadwalkan, dikerjakan, selesai, ditahan, ditolak, dibatalkan | **Sudah ada** | Sudah ada |
| 3 | Study atau pengambilan citra | Satu tindakan pencitraan nyata beserta status perjalanannya | **Sudah ada** | Sudah ada |
| 4 | Gerbang keselamatan (*safety gate*) | Tanya wajib sebelum foto: sedang hamil atau tidak, ada implan logam, alergi kontras | **Sudah ada** | Sudah ada |
| 5 | Verifikasi identitas pasien | Memastikan yang difoto benar-benar pasien yang dipesan | **Sudah ada** | Sudah ada |
| 6 | Penilaian mutu citra | Citra layak dibaca, atau harus diulang | **Sudah ada** | Sudah ada |
| 7 | Pengulangan dan penghentian di tengah jalan | Ulang foto, batal di tengah, beserta sebabnya | **Sudah ada** | Sudah ada |
| 8 | Pencatatan pemakaian bahan nyata | Kontras, film, BHP yang benar-benar terpakai — dicatat sebagai jumlah, bukan rupiah | **Sudah ada** | Sudah ada |
| 9 | Riwayat perpindahan status | Jejak audit yang tidak bisa diubah | **Sudah ada** | Sudah ada |
| 10 | **Hasil bacaan dokter radiolog (*ekspertise*)** | Isi bacaan, pengesahan, dan perilisan hasil | **BELUM ADA** | **Rilis 1** |
| 11 | **Koreksi hasil setelah dirilis (*amendment* berversi)** | Perbaikan hasil tanpa menghapus versi lama | **BELUM ADA** | **Rilis 1** |
| 12 | **Daftar kerja petugas radiologi (*worklist*)** | Antrian pekerjaan harian per alat atau per petugas | **BELUM ADA** | **Rilis 1** |
| 13 | **Temuan kritis** | Hasil yang berbahaya dan wajib dikabarkan segera ke dokter pengirim | **BELUM ADA** | **Rilis 1** |
| 14 | **Data induk radiologi** | Kelola alat (*modality*), jenis pemeriksaan, dan butir keselamatan | **Sebagian** — hanya bisa dibaca, belum bisa dikelola | **Rilis 1** |
| 15 | **Seluruh tampilan frontend Radiologi** | Dibangun dari nol; tidak ada satu pun halaman yang bisa dipakai ulang | **BELUM ADA** | **Rilis 1** |
| 16 | Pengiriman fakta ke Billing | Radiologi hanya mengirim fakta, contohnya "pemeriksaan dikerjakan dan citranya layak" | **Sebagian** | **Rilis 1** |

**Jenis pemeriksaan yang termasuk modul ini** (dikunci `RAD-DEC-002`): X-Ray, CT-Scan, MRI,
USG, mamografi, dan fluoroskopi.

Keenamnya masuk satu modul karena alur kerjanya identik — dipesan, dijadwalkan, pasien
diperiksa, citra diambil, dibaca, lalu hasilnya dirilis. Yang berbeda hanya butir
keselamatannya, dan perbedaan itu sudah ditampung tabel `MstRadModalitySafetyRule` yang
memang mengatur butir mana wajib untuk alat mana.

> **Contoh perbedaan butir keselamatan antar alat:**
> Pasien akan menjalani **MRI**. Butir wajibnya termasuk "ada implan logam atau alat pacu
> jantung?" karena medan magnet MRI dapat menarik logam di dalam tubuh. Butir "skrining
> kehamilan" tidak wajib untuk MRI karena MRI tidak memakai radiasi pengion.
> Pasien yang sama besoknya menjalani **CT-Scan**. Sekarang "skrining kehamilan" menjadi
> wajib karena CT memakai radiasi pengion, sedangkan butir implan logam tidak lagi wajib.
> Satu daftar pertanyaan yang seragam untuk semua alat akan salah di kedua kasus itu.

### Di luar scope — milik modul lain

| No | Kemampuan | Pemilik modul | Alasan |
|---:|---|---|---|
| 1 | Hitung tarif, tagihan, void, refund, pembayaran | `billing-kasir` | Sudah dikunci: Radiologi tidak punya wewenang finansial (`RJ-BIL-GATE-DEC-004`) |
| 2 | Pemeriksaan laboratorium | `laboratorium` | Alur berbeda, sudah dikunci di `RJ-BIL-GATE-DEC-003` |
| 3 | Pendaftaran pasien dan pembentukan kunjungan | `registration-management` | Radiologi hanya menempel pada kunjungan yang sudah ada |
| 4 | Stok dan pembelian kontras, film, BHP | `pharmacy` / `inventory` | Radiologi hanya mencatat **pemakaian**, bukan **persediaan** |
| 5 | Penomoran dan penyimpanan berkas rekam medis | `rekam-medis` | Radiologi hanya menyerahkan hasil sebagai isi rekam medis |
| 6 | Data induk umum: pasien, dokter, prosedur, tarif, unit layanan | `master-data` | Dipakai bersama, bukan milik Radiologi |
| 7 | Penyimpanan berkas citra di PACS dan pertukaran DICOM | Belum ada modul atau infrastruktur | `RJ-BIL-GATE-DEC-004` menyatakan integrasi RIS/PACS eksternal **tidak diaktifkan** |
| 8 | Radioterapi (terapi sinar) | Belum ada modul | Bukan pencitraan diagnostik |
| 9 | Kedokteran nuklir, misalnya sidik tulang dan terapi iodium | Belum ada modul | Dikeluarkan oleh `RAD-DEC-002`. Butuh pelacakan radiofarmaka, izin bahan radioaktif, dan aturan proteksi radiasi yang berbeda jauh dari pencitraan diagnostik biasa |
| 10 | Pemantauan dosis radiasi **petugas** dan perizinan alat | `human-resource` / K3 | Menyangkut kepegawaian dan perizinan, bukan pelayanan pasien |

### Di luar scope — untuk modul lain

Bagian ini menampung kebutuhan penting yang muncul saat wawancara tetapi bukan milik modul
Radiologi. Dicatat supaya tidak hilang, dan tidak dikejar di sesi ini.

| No | Kebutuhan | Ditujukan ke | Alasan singkat |
|---:|---|---|---|
| 1 | Registry kepemilikan modul masih mencatat `Rad` sebagai `PLANNED` padahal kodenya sudah rilis | Pemegang `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Menahan pembuatan entity `Rad*` baru berdasarkan `QBE-MOD-002`; lihat `RAD-CONFLICT-001` |

---

## Fakta Berbukti dari Source Code

Seluruh fakta di bawah dibaca langsung dari repository `NewQuilvianSystemBackend` pada commit
`64da911` dan `QuilvianSystemFrontendDev` pada commit `f66ed1885`.

### `RAD-FACT-001` — Modul Radiologi backend sudah ada dan sudah bermigrasi

Folder `Areas/HealthServices/RadiologyManagement/` berisi 16 berkas dengan total 3.492 baris
kode. Ada dua migration database:

- `Migrations/20260828093000_AddRadiologyManagement.cs`
- `Migrations/20260903095444_AddRadOrderInpatientContext.cs`

Migration kedua menunjukkan pesanan radiologi **sudah mendukung konteks rawat inap**, bukan
hanya rawat jalan.

Artinya bagi pembaca umum: modul ini **bukan proyek dari nol**. Sebagian besar mesin
pemeriksaannya sudah berdiri dan tabelnya sudah ada di database.

### `RAD-FACT-002` — Delapan tabel sudah terdaftar

Terdaftar di `Repositories/ApplicationDbContext.cs` baris 748 sampai 764:

| Nama tabel (class) | Isinya |
|---|---|
| `MstRadModality` | Daftar alat pencitraan, misalnya X-Ray, CT-Scan, MRI, USG |
| `MstRadSafetyRequirement` | Daftar butir pertanyaan keselamatan, misalnya "skrining kehamilan" |
| `MstRadModalitySafetyRule` | Butir keselamatan mana yang wajib untuk alat mana |
| `RadOrder` | Pesanan pemeriksaan dari dokter |
| `RadStudy` | Satu tindakan pengambilan citra |
| `RadStudySafetyCheck` | Jawaban tiap butir keselamatan pada satu study |
| `RadAcquisitionConsumption` | Bahan yang benar-benar terpakai saat pemeriksaan |
| `RadTransitionHistory` | Riwayat setiap perpindahan status |

### `RAD-FACT-003` — Status yang sudah dikunci di kode

Dibaca dari `Areas/HealthServices/RadiologyManagement/Enums/RadiologyEnums.cs`.

**Status pesanan (`RadOrderStatus`)** — 10 nilai. Jalur normalnya:
`Draft` → `Requested` → `Accepted` → `Scheduled` → `InProgress` → `Completed`.
Jalur tidak normalnya: `OnHold`, `CancelRequested`, `Cancelled`, `Rejected`.

**Status study (`RadStudyStatus`)** — 11 nilai. Jalur normalnya:
`Planned` → `PatientVerified` → `SafetyCleared` → `AcquisitionStarted` → `Acquired` →
`QualityAccepted`.
Jalur tidak normalnya: `OnHold`, `Aborted`, `QualityRejected`, `RepeatRequired`, `Cancelled`.

**Keadaan butir keselamatan (`RadSafetyCheckState`)** — 4 nilai:
`Pending`, `Passed`, `Failed`, `NotApplicable`.

> **Contoh supaya jelas bedanya `Passed` dan `NotApplicable`:**
> Pasien laki-laki tidak mungkin hamil. Butir "skrining kehamilan" untuk dia diisi
> `NotApplicable` — pertanyaannya memang tidak berlaku.
> Pasien perempuan usia 28 tahun yang sudah dites dan hasilnya negatif diisi `Passed` —
> pertanyaannya berlaku dan jawabannya aman.
> Keduanya sama-sama meloloskan pemeriksaan, tetapi jejak auditnya harus tetap bisa dibedakan.

**Sebab pengulangan (`RadRepeatCause`)** — 4 nilai: `InternalHospitalError`,
`PatientCondition`, `ExternalCause`, `NewClinicalRequirement`.

**Sebab penghentian (`RadAbortCause`)** — 6 nilai: `PatientCondition`, `PatientRefusal`,
`EquipmentFailure`, `SafetyConcern`, `InternalHospitalError`, `ExternalCause`.

**Jenis bahan terpakai (`RadConsumptionItemType`)** — 5 nilai: `Contrast`, `Material`, `Film`,
`Medication`, `Other`.

### `RAD-FACT-004` — Endpoint yang sudah tersedia

Disajikan mengikuti tampilan Swagger. Judul bagian diambil persis dari nilai atribut
`[Tags(...)]` pada controller.

#### Health Services / Radiology Management / Rad Order

Base URL: `api/v1/health-services/radiology-management/rad-orders`

Berkas: `Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Melihat daftar pesanan radiologi dengan penyaringan dan halaman | `RadOrder : Read` | Query | `PagedResult<T>` berisi ringkasan pesanan |
| `GET` | `/{id}` | Melihat rincian satu pesanan | `RadOrder : Read` | — | Rincian pesanan |
| `GET` | `/episodes/{episodeId}` | Melihat semua pesanan radiologi milik satu kunjungan pasien | `RadOrder : Read` | — | Daftar pesanan |
| `POST` | `/` | Dokter membuat pesanan baru | `RadOrder : Create` | Body | Pesanan yang dibuat |
| `PUT` | `/{id}/accept` | Radiologi menerima pesanan | `RadOrder : Process` | Body | Pesanan terbarui |
| `PUT` | `/{id}/schedule` | Radiologi menjadwalkan pemeriksaan | `RadOrder : Schedule` | Body | Pesanan terbarui |
| `PUT` | `/{id}/start` | Menandai pemeriksaan mulai dikerjakan | `RadOrder : Process` | Body | Pesanan terbarui |
| `PUT` | `/{id}/complete` | Menandai pesanan selesai dikerjakan | `RadOrder : Process` | Body | Pesanan terbarui |
| `PUT` | `/{id}/hold` | Menahan pesanan sementara | `RadOrder : Hold` | Body | Pesanan terbarui |
| `PUT` | `/{id}/resume` | Melanjutkan pesanan yang tertahan | `RadOrder : Hold` | Body | Pesanan terbarui |
| `PUT` | `/{id}/reject` | Radiologi menolak pesanan | `RadOrder : Update` | Body | Pesanan terbarui |
| `PUT` | `/{id}/cancel` | Membatalkan pesanan | `RadOrder : Cancel` | Body | Pesanan terbarui |

#### Health Services / Radiology Management / Rad Study

Base URL: `api/v1/health-services/radiology-management/rad-studies`

Berkas: `Areas/HealthServices/RadiologyManagement/Controllers/RadStudyController.cs`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/modalities` | Melihat daftar alat pencitraan yang tersedia | `RadStudy : Read` | — | Daftar alat |
| `GET` | `/safety-requirements` | Melihat daftar butir pertanyaan keselamatan | `RadStudy : Read` | — | Daftar butir |
| `GET` | `/by-order/{radOrderId}` | Melihat semua study milik satu pesanan | `RadStudy : Read` | — | Daftar study |
| `GET` | `/by-order/{radOrderId}/history` | Melihat riwayat perpindahan status | `RadStudy : Read` | — | Daftar riwayat |
| `POST` | `/by-order/{radOrderId}` | Membuat rencana pengambilan citra | `RadStudy : Create` | Body | Study baru |
| `POST` | `/{id}/verify-patient` | Petugas memastikan identitas pasien benar | `RadStudy : Verify` | Body | Study terbarui |
| `POST` | `/{id}/safety-checks` | Mengisi jawaban butir keselamatan | `RadStudy : Safety` | Body | Study terbarui |
| `POST` | `/{id}/clear-safety` | Menyatakan seluruh gerbang keselamatan lolos | `RadStudy : Safety` | Body | Study terbarui |
| `POST` | `/{id}/start-acquisition` | Mulai mengambil citra | `RadStudy : Acquire` | Body | Study terbarui |
| `POST` | `/{id}/complete-acquisition` | Selesai mengambil citra | `RadStudy : Acquire` | Body | Study terbarui |
| `POST` | `/{id}/abort-acquisition` | Menghentikan pengambilan citra di tengah jalan | `RadStudy : Acquire` | Body | Study terbarui |
| `POST` | `/{id}/decide-quality` | Menyatakan citra layak atau harus diulang | `RadStudy : Quality` | Body | Study terbarui |
| `POST` | `/{id}/repeat` | Membuat study pengulangan | `RadStudy : Repeat` | Body | Study baru |
| `POST` | `/{id}/consumptions` | Mencatat bahan yang benar-benar terpakai | `RadStudy : Consumption` | Body | Catatan pemakaian |

**Arti kode status yang mungkin muncul, bagi pengguna:**

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Permintaan berhasil dan datanya dikembalikan |
| `201` | Data baru berhasil dibuat |
| `400` | Isian yang dikirim tidak lengkap atau formatnya salah |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini |
| `404` | Data yang dicari tidak ditemukan |
| `409` | Tindakan bertabrakan dengan keadaan data saat ini, contohnya mencoba mulai foto padahal gerbang keselamatan belum lolos |

### `RAD-FACT-005` — Kemampuan hasil bacaan (*ekspertise*) belum ada sama sekali

Pencarian menyeluruh terhadap seluruh berkas `.cs` tidak menemukan satu pun model, controller,
DTO, atau service bernama `RadReport` atau sejenisnya.

Padahal `RJ-BIL-GATE-DEC-004` sudah mengunci siklus hidup laporan hasil:
`Pending` → `Drafted` → `Validated` → `Released`, dengan koreksi setelah rilis memakai versi
baru `AmendmentDrafted` → `AmendmentValidated` → `AmendmentReleased` yang **tidak menimpa**
versi sebelumnya.

Artinya bagi pembaca umum: saat ini sistem bisa mencatat bahwa foto sudah diambil dan citranya
layak, tetapi **belum bisa menampung bacaan dokter radiolog**. Padahal bacaan itulah yang
dipakai dokter pengirim untuk mengambil keputusan pengobatan. Ini lubang terbesar modul.

### `RAD-FACT-006` — Frontend Radiologi belum ada sama sekali

Pencarian pada `QuilvianSystemFrontendDev/src` tidak menemukan satu pun pemanggilan ke
`rad-orders`, `rad-studies`, atau `radiology-management`.

Yang ditemukan hanya sebutan kata "radiologi" pada tempat yang tidak berhubungan, yaitu menu
sidebar, kamus terjemahan, tab penunjang pada modul IGD, dan sebuah route bantuan AI
`src/app/api/AI/analyze-radiology/route.js`.

Artinya: seluruh tampilan Radiologi harus dibangun dari nol.

### `RAD-FACT-007` — Data induk radiologi baru bisa dibaca, belum bisa dikelola

Tiga tabel data induk (`MstRadModality`, `MstRadSafetyRequirement`, `MstRadModalitySafetyRule`)
sudah ada, tetapi hanya punya dua endpoint baca — `GET /modalities` dan
`GET /safety-requirements` — yang menumpang di `RadStudyController`.

Belum ada controller khusus untuk menambah, mengubah, atau menonaktifkan data induk itu.

Artinya bagi pembaca umum: kalau rumah sakit membeli alat CT-Scan baru, saat ini **tidak ada
layar maupun endpoint** untuk mendaftarkannya. Datanya harus dimasukkan langsung ke database.

### `RAD-FACT-008` — Belum ada jalur pelewatan gerbang keselamatan darurat

Tidak ada endpoint pelewatan (*override*) pada kedua controller. `RJ-BIL-GATE-DEC-004` memang
menyatakan pelewatan darurat "hanya boleh ada bila disahkan terpisah".

Artinya: keadaan kode saat ini **sesuai** dengan keputusan induk. Yang belum ada adalah
keputusan apakah pelewatan darurat itu memang dibutuhkan. Lihat `RAD-OPEN-003`.

---

## Aturan Bisnis dan Invariant yang Diputuskan Sesi Ini

### `RAD-DEC-003` — Siapa yang boleh mengesahkan hasil bacaan

**Keputusan.** Boleh atau tidaknya seseorang mengesahkan sebuah draf bacaan ditentukan oleh
**peran penulis drafnya**, bukan oleh identitas orangnya.

| Siapa yang menulis draf | Siapa yang boleh mengesahkan | Boleh mengesahkan draf sendiri? |
|---|---|---|
| Dokter radiolog | Dokter radiolog mana pun, termasuk dirinya sendiri | **Ya** |
| Residen / PPDS | Dokter radiolog — wajib orang lain | **Tidak** |
| Radiografer / petugas | Dokter radiolog — wajib orang lain | **Tidak** |
| Bantuan AI | Dokter radiolog — wajib manusia | **Tidak** |

**Mengapa dibedakan begitu.** Yang dijaga bukan "dua orang harus terlibat", melainkan
"bacaan yang sampai ke dokter pengirim harus sudah lewat mata seorang spesialis radiologi".
Kalau spesialisnya sendiri yang menulis, syarat itu sudah terpenuhi sejak awal.

Banyak rumah sakit di Indonesia hanya punya satu atau dua dokter radiolog. Mewajibkan dua
orang berbeda akan menahan hasil setiap kali radiolog bertugas sendirian — dan hasil yang
tertahan pada pasien mendesak lebih berbahaya daripada bacaan yang disahkan sendiri oleh
spesialis yang berkompeten.

**Contoh 1 — boleh.**
dr. Sinta, Sp.Rad bertugas sendirian hari Minggu. Ia membaca CT-Scan kepala Tn. B, menulis
draf, lalu langsung mengesahkan dan merilisnya. Sistem **mengizinkan**, karena penulis drafnya
adalah dokter radiolog. Riwayat mencatat dr. Sinta sebagai penulis sekaligus pengesah.

**Contoh 2 — ditolak.**
dr. Rian (residen radiologi tahun ke-3) menulis draf bacaan foto toraks Ny. C, lalu menekan
tombol Sahkan. Sistem **menolak dengan kode 403** dan pesan yang dipahami pengguna:
"Draf yang Anda tulis harus disahkan dokter radiolog." Draf tetap tersimpan di status
`Drafted` dan masuk antrian pengesahan dr. Sinta.

**Contoh 3 — ditolak walau pengesahnya spesialis.**
dr. Rian menulis draf, lalu meminta dr. Sinta login di komputer yang sama untuk mengesahkan.
Ini **sah**, karena pengesahnya memang dokter radiolog yang berbeda dari penulis. Yang
**tidak sah** adalah bila dr. Rian memakai akun dr. Sinta — dan sistem tidak bisa mencegah
itu, sehingga pencegahannya ada pada kebijakan akun, bukan pada modul ini.

**Akibat teknis yang mengikat.** Sistem wajib menyimpan **peran** penulis draf pada saat draf
dibuat, bukan hanya nama atau ID-nya. Alasannya: peran seseorang bisa berubah. Seorang residen
yang menulis draf hari ini bisa lulus menjadi Sp.Rad enam bulan lagi. Draf lamanya harus tetap
dinilai memakai peran saat draf itu ditulis, bukan peran terbarunya.

**Acceptance criteria yang lahir dari keputusan ini:**

1. Pengesahan draf yang ditulis bukan-radiolog oleh penulisnya sendiri ditolak dengan `403`.
2. Pengesahan draf yang ditulis dokter radiolog oleh dirinya sendiri berhasil.
3. Peran penulis draf tersimpan melekat pada draf dan tidak ikut berubah ketika peran orang
   tersebut diperbarui kemudian.
4. Riwayat menampilkan penulis dan pengesah secara terpisah, walaupun keduanya orang yang sama.

**Yang masih terbuka dari keputusan ini:** daftar peran mana persisnya yang dihitung sebagai
"dokter radiolog" di Quilvian belum dipetakan ke data peran yang ada. Dicatat sebagai
`RAD-OPEN-006`.

---

### `RAD-DEC-004` — Penanganan temuan kritis

**Keputusan.** Temuan kritis wajib melewati tiga hal sekaligus: **ditandai**, **dikirim**, dan
**diakui**. Ketiganya wajib, tidak boleh dipilih salah satu.

Penting dipahami lebih dulu: **sistem tidak menggantikan telepon.** Radiolog tetap wajib
menghubungi dokter pengirim secara langsung. Yang dilakukan sistem adalah merekam bahwa
kontak itu terjadi, dan memastikan ada jejak bila kelak dipertanyakan.

#### Proses bisnisnya

**Tujuan.** Memastikan temuan yang mengancam nyawa sampai ke dokter yang bisa bertindak, dalam
hitungan menit, dan ada buktinya.

**Pelaku.**

| Pelaku | Tugasnya |
|---|---|
| Dokter radiolog | Menandai temuan kritis, menghubungi dokter pengirim, mencatat hasil kontak |
| Dokter pengirim | Menerima pemberitahuan, mengakuinya, menindaklanjuti pasien |
| Clinical Governance | Menetapkan daftar apa saja yang dihitung temuan kritis |

**Pemicu.** Radiolog menemukan kelainan yang mengancam nyawa saat menulis draf bacaan.

**Prasyarat.** Sudah ada study dengan citra yang dinyatakan layak (`QualityAccepted`), dan
draf bacaan sedang ditulis.

**Langkah utama.**

1. Radiolog menandai bacaan sebagai temuan kritis dan menuliskan temuannya.
2. Radiolog mengesahkan dan merilis bacaan seperti biasa — **rilis tidak ditahan**.
3. Sistem otomatis membuat pemberitahuan di kotak masuk dokter pengirim, berstatus
   `Belum dibaca`.
4. Radiolog menghubungi dokter pengirim secara langsung, misalnya lewat telepon.
5. Radiolog mencatat hasil kontak itu di sistem: kapan dihubungi, lewat apa, dan siapa yang
   menerima.
6. Dokter pengirim membuka pemberitahuan dan menekan tombol pengakuan. Status berubah menjadi
   `Sudah diakui`.

**Aturan bisnis yang mengikat.**

- Rilis hasil **tidak boleh ditahan** menunggu pengakuan. Menahan informasi kritis untuk
  memaksa pengakuan justru memperlambat penanganan pasien.
- Bacaan bertanda kritis **tidak dianggap tuntas** selama pengakuan belum ada. Bacaan itu
  tetap muncul di daftar pantau sebagai tunggakan.
- Pencatatan kontak langsung wajib memuat waktu, cara menghubungi, dan nama penerima. Tanpa
  ketiganya, pencatatan ditolak.

**Perubahan status pemberitahuan temuan kritis:**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Bacaan bertanda kritis dirilis | `Belum dibaca` | Sistem, otomatis | Bacaan berstatus `Released` dan bertanda kritis |
| `Belum dibaca` | Dokter pengirim membuka | `Sudah dibaca` | Dokter pengirim | — |
| `Sudah dibaca` | Dokter pengirim menekan pengakuan | `Sudah diakui` | Dokter pengirim | — |
| `Belum dibaca` | Radiolog mencatat kontak langsung | Status tidak berubah | Dokter radiolog | Waktu, cara, dan nama penerima terisi |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
|---|---|
| Dokter pengirim tidak bisa dihubungi | Radiolog mencatat percobaan kontak yang gagal beserta waktunya. Pemberitahuan tetap `Belum dibaca` dan tetap jadi tunggakan |
| Dokter pengirim sudah ganti jaga | Pemberitahuan tetap menempel pada dokter pemesan. Pengalihan ke dokter jaga pengganti **belum diputuskan** — lihat `RAD-OPEN-008` |
| Bacaan bertanda kritis kemudian dikoreksi lewat *amendment* | Pemberitahuan baru dibuat untuk versi koreksi. Pemberitahuan lama tidak dihapus, karena keduanya adalah kejadian yang berbeda |
| Temuan ternyata salah tandai | Penandaan kritis tidak boleh dihapus diam-diam. Pembatalan penandaan harus lewat *amendment* berversi seperti koreksi hasil lainnya |

**Hasil akhir.** Dokter pengirim menerima temuan, mengakuinya, dan rumah sakit memiliki jejak
lengkap: siapa menemukan, kapan dihubungi, lewat apa, siapa menerima, dan kapan diakui.

#### Contoh berangka

Pukul 21.40 dr. Sinta, Sp.Rad membaca CT-Scan kepala Tn. B dan menemukan perdarahan otak.

1. 21.40 — ia menandai temuan kritis, menulis bacaan, mengesahkan, dan merilis.
2. 21.40 — sistem membuat pemberitahuan untuk dr. Andi (dokter IGD pemesan), status
   `Belum dibaca`.
3. 21.43 — dr. Sinta menelepon dr. Andi dan mencatat di sistem: waktu `21.43`, cara `Telepon`,
   penerima `dr. Andi`.
4. 21.51 — dr. Andi membuka pemberitahuan, status menjadi `Sudah dibaca`.
5. 21.52 — dr. Andi menekan pengakuan, status menjadi `Sudah diakui`.

Selisih 11 menit antara rilis dan pengakuan itu terekam dan dapat diaudit. Bila dr. Andi tidak
pernah mengakuinya, temuan tetap tampil di daftar pantau tunggakan sampai ada yang menangani.

**Acceptance criteria yang lahir dari keputusan ini:**

1. Bacaan bertanda kritis tetap dapat dirilis tanpa menunggu pengakuan siapa pun.
2. Perilisan bacaan bertanda kritis selalu menghasilkan tepat satu pemberitahuan berstatus
   `Belum dibaca` untuk dokter pemesan.
3. Pencatatan kontak langsung ditolak bila waktu, cara, atau nama penerima kosong.
4. Bacaan bertanda kritis yang belum diakui tetap muncul di daftar pantau tunggakan.
5. Koreksi berversi terhadap bacaan bertanda kritis menghasilkan pemberitahuan baru tanpa
   menghapus yang lama.

**Yang masih terbuka dari keputusan ini:** daftar apa saja yang dihitung sebagai temuan kritis
belum ada (`RAD-OPEN-007`), dan pengalihan pemberitahuan saat dokter pemesan berganti jaga
belum diputuskan (`RAD-OPEN-008`).

---

### `RAD-DEC-005` — Pengelolaan dan pengesahan aturan keselamatan

Menjawab `RAD-CQ-002`. Ini keputusan yang membuka penghambat terbesar modul.

**Keputusan.** Admin Radiologi menyusun dan mengubah aturan keselamatan. Perubahan **baru
berlaku setelah disetujui penanggung jawab klinis**. Setiap persetujuan menaikkan nomor versi
aturan.

**Mengapa dipilih begini.** Model ini sudah setengah terpasang di kode. `RadStudy` menyimpan
`SafetyRuleVersionAtClearance` — versi aturan dibekukan pada saat study dinyatakan lolos.
Kolom itu hanya masuk akal bila aturan memang berversi dan berubah secara terkendali. Memilih
"langsung berlaku tanpa persetujuan" akan membuat kolom itu merekam versi yang bisa berubah
kapan saja tanpa jejak siapa yang menyetujui.

**Alur perubahan aturan:**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Buat aturan baru | `Draf` | Admin Radiologi | Alat dan butir keselamatan sudah ada |
| `Draf` | Ajukan pengesahan | `Menunggu persetujuan` | Admin Radiologi | Isi aturan lengkap |
| `Menunggu persetujuan` | Setujui | `Aktif`, nomor versi naik | Penanggung jawab klinis | — |
| `Menunggu persetujuan` | Tolak | `Draf` | Penanggung jawab klinis | Alasan wajib diisi |
| `Aktif` | Nonaktifkan | `Nonaktif` | Penanggung jawab klinis | — |

**Contoh berangka.**
Rumah sakit memutuskan skrining kehamilan wajib untuk CT-Scan pada pasien perempuan usia 12
sampai 55 tahun.

1. Admin Radiologi membuat aturan: alat `CT-Scan`, butir `SKRINING-HAMIL`, wajib `Ya`.
   Statusnya `Draf`, versi belum ada.
2. Admin mengajukan pengesahan. Status menjadi `Menunggu persetujuan`.
3. dr. Sinta, Sp.Rad selaku penanggung jawab klinis menyetujui. Status menjadi `Aktif`,
   versi menjadi `3`.
4. Tn. B menjalani CT-Scan hari itu dan dinyatakan lolos gerbang keselamatan. Study-nya
   menyimpan `SafetyRuleVersionAtClearance = 3`.
5. Sebulan kemudian aturan direvisi menjadi versi `4`. Study Tn. B **tetap** tercatat dinilai
   dengan versi `3`, bukan versi `4`. Penilaian yang sudah terjadi tidak ditulis ulang.

**Peringatan yang wajib disampaikan ke pengguna.** Selama belum ada satu pun aturan `Aktif`
untuk sebuah alat, seluruh pengambilan citra dengan alat itu **akan ditolak**. Ini bukan
kerusakan, melainkan perilaku yang disengaja. Layar pengelolaan aturan wajib menampilkan
peringatan itu secara jelas, misalnya "Alat MRI belum punya aturan keselamatan aktif.
Pemeriksaan dengan alat ini akan ditolak sampai aturan disahkan."

**Acceptance criteria:**

1. Aturan berstatus `Draf` atau `Menunggu persetujuan` tidak ikut dinilai gerbang keselamatan.
2. Persetujuan oleh selain penanggung jawab klinis ditolak dengan `403`.
3. Setiap persetujuan menaikkan nomor versi aturan tepat satu kali.
4. Penolakan pengesahan tanpa alasan ditolak dengan `400`.
5. Study yang sudah lolos mempertahankan nomor versi aturan saat itu, walau aturannya berubah
   kemudian.
6. Layar pengelolaan menampilkan peringatan untuk setiap alat yang belum punya aturan aktif.

**Yang masih terbuka:** peran mana yang dihitung "penanggung jawab klinis" belum dipetakan —
digabung ke `RAD-OPEN-006`.

---

### `RAD-DEC-006` — Jalur hasil bacaan menuju rekam medis

Menjawab `RAD-CQ-006`.

**Keputusan.** Rekam medis dan CPPT **membaca langsung** dari modul Radiologi lewat API.
Tidak ada baris dokumen yang dibuat untuk hasil bacaan internal.

Slot `PatientClinicalDocumentSource.Radiology` yang sudah ada **tetap dipakai**, tetapi hanya
untuk berkas unggahan dari luar — misalnya hasil foto yang dibawa pasien dari rumah sakit
lain. Bukan untuk hasil bacaan yang lahir di modul ini.

**Mengapa dipilih begini.** Hasil bacaan bisa dikoreksi lewat *amendment* berversi
(`RAD-DEC-001` butir 11). Bila rekam medis menyimpan barisnya sendiri, setiap koreksi menuntut
baris itu ikut diperbarui. Sekali saja terlewat, rekam medis akan menampilkan versi yang sudah
digantikan — dan itu versi yang dipakai dokter untuk mengambil keputusan pengobatan.

Membaca langsung membuat pertanyaan "versi mana yang benar" tidak pernah muncul, karena hanya
ada satu tempat penyimpanan.

Keputusan ini juga sejalan dengan uji arsitektur yang sudah berjalan,
`RawatInapTidakMemilikiSatuPunTabelSalinanHasilPenunjang`, yang melarang modul lain menyimpan
salinan hasil penunjang.

**Contoh perbedaannya.**
dr. Andi membuka rekam medis Tn. B pukul 08.00 dan membaca hasil CT-Scan.
Pukul 09.00 dr. Sinta merilis koreksi berversi karena ada temuan tambahan.
Pukul 10.00 dr. Andi membuka rekam medis yang sama.

- Dengan keputusan ini: dr. Andi **melihat versi koreksi**, karena layar mengambil langsung
  dari modul Radiologi.
- Bila hasil disalin ke rekam medis dan penyalinan koreksi terlewat: dr. Andi **masih melihat
  versi pukul 08.00**, tanpa tahu ada koreksi.

**Konsekuensi yang harus diterima.** Layar rekam medis bergantung pada modul Radiologi dapat
dihubungi. Bila modul itu bermasalah, hasil radiologi tidak tampil. Layar wajib menampilkan
keadaan itu apa adanya — misalnya "Hasil radiologi sedang tidak dapat ditampilkan" — dan
**tidak boleh** menampilkan layar kosong yang terbaca seolah pasien memang tidak punya hasil.

**Acceptance criteria:**

1. Tidak ada satu pun tabel di luar modul Radiologi yang menyimpan isi hasil bacaan.
2. Koreksi berversi langsung terlihat di rekam medis tanpa langkah penyalinan apa pun.
3. Ketika modul Radiologi tidak dapat dihubungi, rekam medis menampilkan pesan gangguan, bukan
   daftar kosong.
4. Slot `PatientClinicalDocumentSource.Radiology` hanya terisi oleh berkas unggahan dari luar.

---

### `RAD-DEC-007` — Cara menyelesaikan konflik registry

Menjawab `RAD-CQ-001` dan menutup jalan keluar untuk `RAD-CONFLICT-001`.

**Keputusan.** Registry diperbaiki menjadi `ACTIVE`, disertai entri riwayat susulan yang
mencatat keadaan sebenarnya apa adanya:

| Yang wajib dicatat | Isinya |
|---|---|
| Tanggal keputusan | 28 Agustus 2026, `RJ-BIL-DEC-014`, Sukma Giri |
| Tanggal penerapan sebenarnya | Tanggal registry benar-benar diperbaiki |
| Catatan selisih | Delapan tabel `Rad*` dibuat sebelum penerapan itu, lewat migration 28 dan 31 Agustus 2026 |

**Mengapa dicatat jujur, bukan ditutup.** Selisih waktu antara keputusan dan penerapannya
adalah informasi yang berguna bagi audit: ia menunjukkan tata kelola sempat tertinggal dari
kode, dan itu pola yang perlu diketahui supaya tidak berulang. Menuliskan tanggal penerapan
seolah-olah 28 Agustus akan menghapus jejak itu.

Modul Laboratorium sudah diperlakukan dengan pola yang sama dan punya entri riwayatnya.
Radiologi hanya menyusul.

**Batas wewenang.** Keputusan ini menetapkan **cara penyelesaiannya**, bukan menjalankannya.
Perbaikan berkas registry adalah wewenang pemegang tata kelola, dan berkas itu berada di luar
`docs/module-blueprints/**`. Sesi wawancara tidak boleh menyentuhnya.

**Yang harus dilakukan pemegang registry:**

1. Ubah baris 22 `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dari `PLANNED` menjadi
   `ACTIVE`.
2. Tambahkan entri riwayat dengan tiga butir pada tabel di atas.
3. Samakan salinan registry pada suite Skill `QuilvianEngineeringSkills`, karena keduanya
   sekarang sama-sama tertulis `PLANNED`.

**Sampai ketiganya selesai, pembuatan tabel hasil bacaan tetap tertahan `QBE-MOD-002`.**

---

### `RAD-DEC-008` — Pelewatan gerbang keselamatan dalam keadaan darurat

Menjawab `RAD-OPEN-003`. Inilah pengesahan terpisah yang dimaksud `RJ-BIL-GATE-DEC-004`.

> **PERINGATAN KEWENANGAN.** Keputusan ini menyangkut keselamatan pasien dan disetujui pemilik
> modul **tanpa** tanda tangan pendamping dari penanggung jawab tata kelola klinis, karena
> peran itu belum ditunjuk (`RAD-OPEN-001`). Statusnya `approved` untuk keperluan desain,
> tetapi **wajib ditinjau ulang dan ditandatangani tata kelola klinis sebelum dipakai di
> lingkungan production.** Pola ini mengikuti `RJ-BIL-DEC-014` yang juga mencatat ketiadaan
> countersignature klinis secara terbuka.

**Keputusan.** Jalur pelewatan darurat **tersedia**, dengan syarat yang sengaja dibuat berat:

| Syarat | Ketentuan |
|---|---|
| Siapa yang boleh mengesahkan | Dokter penanggung jawab pasien (DPJP) atau dokter jaga senior. **Bukan** petugas radiologi |
| Alasan | Wajib ditulis, tidak boleh kosong, tidak boleh pilihan seragam |
| Penandaan | Study ditandai permanen "dilakukan dengan pelewatan gerbang keselamatan" |
| Butir yang dilewati | Dicatat satu per satu, bukan sebagai satu centang menyeluruh |
| Waktu dan pelaku | Direkam otomatis |
| Tinjauan | Seluruh pelewatan masuk daftar tinjauan berkala |

**Mengapa pelewatan diizinkan, bukan dilarang.** Alasannya bukan kemudahan, melainkan
kejujuran data.

Bayangkan pilihan yang dihadapi petugas bila pelewatan dilarang sama sekali. Pasien trauma
tidak sadar butuh CT-Scan kepala segera. Skrining kehamilan tidak mungkin dijawab — pasiennya
tidak bisa ditanya, dan menunggu hasil tes memakan waktu yang tidak dimiliki. Petugas hanya
punya dua pilihan: menunda pemeriksaan yang menyelamatkan nyawa, atau mengisi butir itu
sebagai "tidak berlaku" padahal sebenarnya berlaku.

Pilihan kedua yang lebih sering diambil. Dan begitu diambil, jejak auditnya **hilang
sepenuhnya** — rekam datanya terlihat seperti pemeriksaan normal yang lolos wajar. Pelewatan
yang tercatat jelas jauh lebih baik daripada pemalsuan yang tidak terlihat.

**Contoh penerapan.**
Pukul 02.15 Tn. D, korban kecelakaan, tiba di IGD dalam keadaan tidak sadar. dr. Bagus, dokter
jaga IGD, meminta CT-Scan kepala segera.

1. Petugas radiologi mengisi butir keselamatan. Butir `SKRINING-HAMIL` tidak dapat dijawab
   karena pasien tidak sadar dan jenis kelaminnya laki-laki — dalam kasus ini justru diisi
   `NotApplicable` secara sah.
2. Butir `RIWAYAT-ALERGI-KONTRAS` tetap tidak dapat dijawab karena pasien tidak sadar dan tidak
   ada keluarga. Butir ini **wajib** dan berkeadaan `Pending`, sehingga gerbang menolak.
3. dr. Bagus mengesahkan pelewatan untuk butir `RIWAYAT-ALERGI-KONTRAS` dengan alasan tertulis:
   "Pasien tidak sadar, tidak ada keluarga, indikasi CT kepala mendesak dugaan perdarahan
   intrakranial. Manfaat melebihi risiko."
4. Pemeriksaan berjalan. Study ditandai permanen sebagai pelewatan, mencantumkan butir yang
   dilewati, alasan, nama dr. Bagus, dan waktu 02.19.
5. Study itu masuk daftar tinjauan berkala.

**Yang tidak boleh terjadi.** Pelewatan **tidak** menghapus butir keselamatannya. Butir
`RIWAYAT-ALERGI-KONTRAS` tetap tercatat berkeadaan `Pending`, bukan diubah menjadi `Passed`.
Yang berubah hanyalah bahwa study diizinkan berjalan meski butir itu belum tuntas.

**Acceptance criteria:**

1. Pelewatan oleh petugas radiologi ditolak dengan `403`.
2. Pelewatan tanpa alasan tertulis ditolak dengan `400`.
3. Pelewatan tidak mengubah keadaan butir keselamatan yang dilewati.
4. Study hasil pelewatan tetap membawa penanda itu selamanya dan tidak dapat dihapus.
5. Setiap butir yang dilewati tercatat sendiri-sendiri, bukan sebagai satu penanda menyeluruh.
6. Daftar tinjauan menampilkan seluruh study yang berjalan lewat pelewatan.

---

### `RAD-DEC-009` — Titik sentuh dengan modul IGD

Menjawab `RAD-CQ-005` dan menetapkan jalan keluar `RAD-CONFLICT-002`.

**Keputusan.** IGD memesan radiologi lewat endpoint resmi
`POST api/v1/health-services/radiology-management/rad-orders`. Pesanan berjenis `External`
yang terlanjur tercatat **dibiarkan apa adanya sebagai riwayat** dan tidak dipindahkan.

**Mengapa riwayat lama tidak dipindahkan.** Pesanan lama itu tidak punya study, tidak punya
jawaban gerbang keselamatan, dan tidak punya penilaian mutu citra — karena pemeriksaannya
memang tidak pernah melewati sistem ini. Memindahkannya berarti membuat catatan pemeriksaan
untuk kejadian yang tidak pernah tercatat, atau mengisi data yang tidak pernah ada. Itu
mengarang fakta klinis.

Riwayat yang tidak seragam lebih jujur daripada riwayat yang rapi tetapi sebagian dikarang.

**Yang harus dilakukan modul IGD:**

1. Perbaiki teks di layar yang menyatakan "modul Radiologi belum ada" — pernyataan itu sudah
   tidak benar sejak 31 Agustus 2026.
2. Sambungkan pemesanan radiologi IGD ke endpoint resmi.
3. Tinjau `IGD-DEC-099` yang masih berstatus `draft` dengan alasan "pemilik
   `RadiologyManagement` belum ditunjuk" — alasan itu sudah gugur sejak `RJ-BIL-DEC-014`
   pada 28 Agustus 2026.

**Batas wewenang.** Ketiganya milik pemilik modul IGD. Modul Radiologi hanya menyediakan
endpoint dan menyatakan endpoint itu sah dipakai IGD.

**Catatan urutan kerja.** Endpoint sudah siap, tetapi petugas radiologi belum punya layar untuk
menindaklanjuti pesanan yang masuk. Penyambungan IGD sebaiknya menunggu frontend Radiologi
Rilis 1 agar pesanan tidak menumpuk tanpa ada yang mengerjakan. Perbaikan teks di layar IGD
tidak perlu menunggu apa pun.

**Acceptance criteria:**

1. Pesanan radiologi dari IGD memakai endpoint yang sama dengan pesanan dari poli dan rawat
   inap, tanpa perlakuan khusus.
2. Tidak ada satu pun pesanan `External` lama yang diubah bentuknya.
3. Teks yang menyatakan modul Radiologi belum ada tidak lagi muncul di layar mana pun.

---

### `RAD-DEC-010` — Pemberitahuan temuan kritis saat dokter pemesan ganti jaga

Menjawab `RAD-OPEN-008`. Melengkapi `RAD-DEC-004`.

**Keputusan.** Pemberitahuan tetap tercatat atas nama dokter pemesan, **dan sekaligus** muncul
di daftar pantau unit tempat pasien berada. Siapa pun yang bertugas di unit itu dapat
membukanya dan mengakuinya, dan nama pengakunya dicatat terpisah dari nama pemesan.

**Mengapa tidak dialihkan otomatis ke dokter jaga.** Pengalihan otomatis membutuhkan data
jadwal jaga yang selalu terisi dan selalu benar. Data itu milik modul kepegawaian, di luar
scope modul ini, dan belum tentu terisi akurat.

Bahayanya nyata: bila jadwal kosong atau belum diperbarui, pemberitahuan temuan kritis akan
dialihkan kepada orang yang sedang tidak bertugas — dan hilang lebih dalam daripada bila
dibiarkan menempel pada pemesan. Menggantungkan keselamatan pasien pada kelengkapan data
jadwal adalah risiko yang tidak perlu diambil.

Daftar pantau unit menyelesaikan masalah yang sama tanpa bergantung pada data itu.

**Contoh.**
dr. Andi memesan CT-Scan Tn. B pukul 20.00, lalu selesai jaga pukul 21.00.
Hasil bertanda kritis dirilis pukul 21.40.

1. Pemberitahuan tercatat atas nama dr. Andi, berstatus `Belum dibaca`.
2. Pemberitahuan yang sama muncul di daftar pantau IGD, tempat Tn. B masih dirawat.
3. Pukul 21.45 dr. Bagus, dokter jaga pengganti, melihatnya di daftar pantau IGD, membuka, dan
   mengakuinya.
4. Riwayat mencatat: dipesan dr. Andi, diakui dr. Bagus pukul 21.45.

Tanggung jawab tetap terlacak, dan pasien tidak menunggu sampai dr. Andi kembali bertugas.

**Acceptance criteria:**

1. Pemberitahuan temuan kritis muncul di dua tempat: kotak dokter pemesan dan daftar pantau
   unit pasien.
2. Pengakuan oleh dokter selain pemesan diterima, dan namanya dicatat terpisah.
3. Riwayat menampilkan nama pemesan dan nama pengaku sebagai dua hal yang berbeda.
4. Pemberitahuan hilang dari daftar pantau unit hanya setelah diakui, bukan setelah dibaca.

---

### `RAD-DEC-012` — Bentuk daftar kerja petugas radiologi

Menjawab sebagian `DEC-RAD-003`. Ditetapkan pada Amendment pass 2026-09-09.

**Keputusan.** Daftar kerja dikelompokkan **per alat pencitraan**. Petugas membuka daftar alat
tempat ia bertugas hari itu.

**Mengapa per alat, bukan per petugas.** Di radiologi, penempatan petugas mengikuti ruang alat,
bukan mengikuti pasien. Radiografer yang bertugas di ruang CT-Scan mengerjakan semua pemeriksaan
CT-Scan hari itu, siapa pun pasiennya.

**Akibat teknis yang menguntungkan.** Pilihan ini **tidak membutuhkan tabel baru sama sekali**.
Daftar kerja cukup berupa penyaringan atas pesanan dan study yang sudah ada, berdasarkan alat,
status, dan tanggal.

Artinya slice `S12` **tidak ikut tertahan** oleh `RAD-CONFLICT-001` — berbeda dari hasil bacaan
yang memang butuh tabel baru.

> **Contoh.** Radiografer Tono bertugas di ruang CT-Scan pada 10 September. Ia membuka daftar
> kerja CT-Scan dan melihat tujuh pemeriksaan: dua sudah selesai, satu sedang berjalan, empat
> menunggu. Pemeriksaan MRI dan USG hari itu tidak muncul di layarnya.

**Yang sengaja tidak dipilih.** Penugasan per petugas ditolak karena membutuhkan tabel
penugasan baru **dan** data jadwal jaga dari modul kepegawaian yang belum tentu terisi akurat.
Alasannya sama dengan yang membuat `RAD-DEC-010` menolak pengalihan otomatis ke dokter jaga.

**Acceptance criteria:**

1. Daftar kerja menampilkan hanya pemeriksaan pada alat yang dipilih.
2. Daftar kerja tidak membaca satu pun tabel di luar yang sudah ada.
3. Petugas dapat berpindah antar daftar alat tanpa berganti halaman.

---

### `RAD-DEC-013` — Penanda mendesak pada pesanan

Menjawab sisa `DEC-RAD-003`. Ditetapkan pada Amendment pass 2026-09-09.

**Keputusan.** Dokter pengirim menandai pesanan sebagai **cito** saat memesan. Daftar kerja
menampilkan pesanan bercito di urutan atas dengan penanda yang menyolok.

**Mengapa penandanya di pesanan, bukan di study.** Cito adalah keputusan klinis dokter
pengirim — dialah yang tahu pasiennya mendesak. Menaruh penandanya pada pesanan berarti
keputusan itu tersimpan di tempat keputusannya diambil, dan ikut terbawa ke seluruh study yang
lahir darinya.

> **Contoh.** dr. Andi memesan CT-Scan kepala Tn. D dari IGD dan menandainya cito pukul 02.15.
> Radiografer Tono membuka daftar kerja CT-Scan pukul 02.17 dan melihat pemeriksaan Tn. D di
> urutan paling atas dengan penanda merah, mendahului empat pemeriksaan rawat jalan yang
> dipesan lebih dulu.

**Akibat teknis.** Menambah satu kolom pada tabel pesanan yang **sudah ada**. Ini bukan tabel
baru, sehingga tidak tertahan `QBE-MOD-002` yang mengatur modul dan entity baru. Tetap
diperlukan migration, dan sebaiknya dikonfirmasi ke pemegang registry.

**Yang sengaja tidak dipilih.** Pemantauan keterlambatan pesanan cito — seperti yang dipakai
modul Laboratorium — ditolak untuk sekarang karena membutuhkan penetapan batas waktu per jenis
pemeriksaan. Itu keputusan klinis baru, dan penanggung jawab klinisnya belum ditunjuk.
Menambahkannya sekarang akan **menambah blocker, bukan menutupnya**. Dicatat sebagai
`RAD-OPEN-009`.

**Acceptance criteria:**

1. Pesanan bercito muncul di urutan atas daftar kerja, mendahului pesanan biasa yang lebih tua.
2. Penanda cito terlihat jelas tanpa perlu membuka rincian pesanan.
3. Penanda cito ikut terbawa ke seluruh study yang lahir dari pesanan itu.
4. Penanda cito tercatat siapa yang menetapkannya dan kapan.

---

### `RAD-DEC-011` — Status `Draft` pada pesanan tidak dipakai

Menjawab `RAD-CQ-004`.

**Keputusan.** Dokter tidak menyimpan draf pesanan. Pesanan langsung terkirim saat disimpan,
lahir berstatus `Requested`. Nilai `Draft` **dibiarkan ada** di daftar status tetapi ditandai
tidak dipakai.

**Mengapa tidak dihapus dari daftar status.** Nilai status disimpan sebagai angka di database.
`Draft` bernilai `1`, `Requested` bernilai `2`, dan seterusnya. Menghapus `Draft` akan
menggeser seluruh angka sesudahnya, sehingga data lama akan terbaca sebagai status yang salah.

> **Contoh bahayanya:** bila `Draft` dihapus dan `Requested` menjadi `1`, maka pesanan lama
> yang tersimpan dengan angka `2` — yang dulu berarti `Requested` — akan terbaca sebagai
> `Accepted`. Pesanan yang belum disentuh radiologi mendadak terlihat sudah diterima.

**Mengapa kemampuan draf tidak dibangun.** Pesanan radiologi isinya pendek: pemeriksaan apa,
alat apa, dan indikasi klinisnya. Berbeda dari pesanan laboratorium yang dapat memuat banyak
butir sekaligus. Tidak ada bukti seorang pun membutuhkan penyimpanan setengah jalan, dan
menambah status berarti menambah keadaan yang harus diuji dan dirawat.

**Acceptance criteria:**

1. Tidak ada satu pun endpoint yang menghasilkan pesanan berstatus `Draft`.
2. Nilai `Draft` tetap ada di daftar status dengan angka yang tidak berubah.
3. Dokumentasi siklus hidup menjelaskan bahwa `Draft` ada tetapi tidak dipakai, beserta
   alasannya.

---

## Konflik yang Harus Diselesaikan

### `RAD-CONFLICT-001` — Registry mencatat modul Radiologi masih `PLANNED`

| Sumber | Isinya |
|---|---|
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 22 | `HealthServices` , `RadiologyManagement / Radiology` , `BUSINESS DOMAIN / MODULE` , prefix `Rad` , status `PLANNED` |
| Source code pada `64da911` | Delapan tabel `Rad*` sudah rilis dan sudah bermigrasi ke database |

**Mengapa ini penting.** Aturan `QBE-MOD-002` menyatakan modul atau entity operasional baru
tanpa entri registry yang disetujui berstatus `BLOCKED`. Selama registry masih `PLANNED`,
pembuatan entity `Rad*` **baru** — misalnya tabel hasil bacaan `RadReport` — tertahan.

**Dampak nyata:** kemampuan nomor 10 dan 11 pada daftar "Di dalam scope" (hasil bacaan dan
koreksi berversi) **tidak bisa diimplementasikan** sebelum registry diperbarui, walaupun
aturan bisnisnya sudah dikunci sejak 2026-08-19.

**Diperdalam oleh audit 2026-09-09.** Ternyata kenaikan status itu **sudah pernah diputuskan
dan disetujui**. `RJ-BIL-DEC-014` (Sukma Giri, 28 Agustus 2026) memutuskan menaikkan
`RadiologyManagement` dari `PLANNED` ke `ACTIVE`, dan kode Billing bahkan menulis bahwa
kenaikan itu sudah terjadi. Yang tidak pernah terjadi adalah **penerapannya pada berkas
registry**. Modul Laboratorium yang diperlakukan dengan pola sama justru punya entri riwayat
`PLANNED → ACTIVE` tertanggal 2026-09-02 pada berkas yang sama.

Jadi ini bukan keputusan yang belum diambil, melainkan keputusan yang belum dijalankan.
Rinciannya di `01-existing-capability-map.md` bagian `RAD-CONF-001`.

**Bukan wewenang sesi ini.** Perbaikan registry milik pemegang berkas tata kelola, bukan modul
Radiologi. Sudah dicatat pada bagian `Di luar scope — untuk modul lain`.

---

### `RAD-CONFLICT-002` — Modul IGD masih menyatakan modul Radiologi belum ada

Ditemukan audit 2026-09-09.

| Sumber | Isinya |
|---|---|
| Kode IGD, ditulis 27 Agustus 2026 | `EmergencyOrderKind.RadiologyOrder` diberi keterangan "modul Radiologi belum ada, sehingga pesanannya dibuat di luar sistem" |
| Teks yang dilihat pengguna IGD | "Pemeriksaan radiologi juga belum dapat dipesan lewat sistem — modul Radiologi belum ada" |
| Kenyataan sejak 31 Agustus 2026 | Modul Radiologi rilis lengkap dengan endpoint `POST /rad-orders` |

**Mengapa ini terjadi.** Keputusan IGD `IGD-DEC-099` (26 Agustus 2026) menunda pemesanan
radiologi **sampai pemilik `RadiologyManagement` ditunjuk**. Pemilik itu ditunjuk dua hari
kemudian lewat `RJ-BIL-DEC-014`, dan modulnya rilis tiga hari setelah itu. Prasyarat penundaan
sudah gugur, tetapi IGD tidak pernah diberi tahu.

**Akibat nyatanya.** Perawat dan dokter IGD diarahkan memesan radiologi di luar sistem,
padahal jalurnya sudah ada. Pesanan yang ditempuh di luar sistem tidak menghasilkan study,
tidak melewati gerbang keselamatan, dan tidak menerbitkan fakta tagih.

**Bukan wewenang sesi ini.** Perbaikan milik pemilik modul IGD. Titik sentuhnya dengan
Radiologi tetap perlu diputuskan bersama — lihat `RAD-CQ-005`.

---

## Decision Log

| Decision ID | Type | Keputusan atau pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RAD-DEC-001` | Decision | Batas scope modul Radiologi dikunci. Rilis 1 berjalan **sampai hasil bacaan dirilis**, mencakup hasil bacaan radiolog, koreksi berversi, daftar kerja petugas, temuan kritis, kelola data induk, dan seluruh frontend Radiologi | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul pada sesi wawancara 2026-09-09 |
| `RAD-DEC-002` | Decision | Modul mencakup **seluruh pencitraan diagnostik**: X-Ray, CT-Scan, MRI, USG, mamografi, fluoroskopi. Kedokteran nuklir dan radioterapi **di luar** scope | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul pada sesi wawancara 2026-09-09; menutup `RAD-OPEN-002` |
| `RAD-DEC-003` | Decision | Dokter radiolog boleh mengesahkan draf bacaannya sendiri. Draf yang ditulis residen/PPDS, radiografer, atau bantuan AI wajib disahkan dokter radiolog yang berbeda. Peran penulis disimpan melekat pada draf | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul pada sesi wawancara 2026-09-09; menutup `RAD-OPEN-004` |
| `RAD-DEC-004` | Decision | Temuan kritis wajib ditandai, dikirim sebagai pemberitahuan berstatus, dan diakui dokter pengirim. Rilis hasil tidak ditahan. Radiolog wajib mencatat kontak langsung berisi waktu, cara, dan nama penerima | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul pada sesi wawancara 2026-09-09 |
| `RAD-FACT-001` | Fact | Modul backend Radiologi sudah ada: 16 berkas, 3.492 baris, 2 migration | — | `approved` | Source `64da911` | `Areas/HealthServices/RadiologyManagement/`, `Migrations/20260828093000_AddRadiologyManagement.cs` |
| `RAD-FACT-002` | Fact | Delapan tabel `Rad*` terdaftar di `ApplicationDbContext` | — | `approved` | Source `64da911` | `Repositories/ApplicationDbContext.cs:748-764` |
| `RAD-FACT-003` | Fact | Tujuh enum status dan sebab sudah terkunci di kode | — | `approved` | Source `64da911` | `Areas/HealthServices/RadiologyManagement/Enums/RadiologyEnums.cs` |
| `RAD-FACT-004` | Fact | 26 endpoint sudah tersedia pada dua controller | — | `approved` | Source `64da911` | `Controllers/RadOrderController.cs`, `Controllers/RadStudyController.cs` |
| `RAD-FACT-005` | Fact | Kemampuan hasil bacaan (`RadReport`) belum ada sama sekali | — | `approved` | Source `64da911` | Pencarian menyeluruh `*.cs` tidak menemukan `RadReport` |
| `RAD-FACT-006` | Fact | Frontend Radiologi belum ada; tidak ada pemanggil `rad-orders` maupun `rad-studies` | — | `approved` | Source `f66ed1885` | Pencarian menyeluruh `QuilvianSystemFrontendDev/src` |
| `RAD-FACT-007` | Fact | Data induk radiologi hanya bisa dibaca, belum bisa dikelola | — | `approved` | Source `64da911` | `Controllers/RadStudyController.cs:44,56` |
| `RAD-FACT-008` | Fact | Belum ada endpoint pelewatan gerbang keselamatan darurat | — | `approved` | Source `64da911` | Kedua controller tidak memuat endpoint pelewatan |
| `RAD-DEC-005` | Decision | Admin Radiologi mengelola aturan keselamatan; perubahan berlaku hanya setelah disahkan penanggung jawab klinis, dan setiap pengesahan menaikkan nomor versi | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul 2026-09-09; menjawab `RAD-CQ-002` |
| `RAD-DEC-006` | Decision | Rekam medis membaca hasil bacaan langsung dari modul Radiologi. Tidak ada salinan. Slot `PatientClinicalDocumentSource.Radiology` hanya untuk berkas unggahan dari luar | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul 2026-09-09; menjawab `RAD-CQ-006` |
| `RAD-DEC-007` | Decision | Registry diperbaiki menjadi `ACTIVE` dengan entri riwayat susulan yang mencatat tanggal keputusan, tanggal penerapan, dan selisihnya secara jujur | Pemilik modul, dijalankan pemegang registry | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul 2026-09-09; menjawab `RAD-CQ-001` |
| `RAD-CONFLICT-001` | Conflict | Registry mencatat `Rad` sebagai `PLANNED` padahal `RJ-BIL-DEC-014` sudah menyetujui kenaikan ke `ACTIVE` dan kodenya sudah rilis. **Jalan keluar sudah ditetapkan `RAD-DEC-007`; menunggu tindakan pemegang registry** | Pemegang registry | `draft` | — | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:22`; `BillingSourceContract.cs:11-13`; `rawat-jalan/00-interview-decisions.md:104` |
| `RAD-DEC-008` | Decision | Pelewatan gerbang keselamatan darurat tersedia, hanya oleh DPJP atau dokter jaga senior, wajib alasan tertulis, ditandai permanen, dan masuk daftar tinjauan. **Menunggu tanda tangan tata kelola klinis** | Pemilik modul | `approved` untuk desain, **belum** untuk production | Yoga Aji Pratama, 2026-09-09; countersignature klinis **tidak ada** | Jawaban pemilik modul 2026-09-09; menjawab `RAD-OPEN-003`; pengesahan terpisah yang dimaksud `RJ-BIL-GATE-DEC-004` |
| `RAD-DEC-009` | Decision | IGD memesan lewat endpoint resmi `POST /rad-orders`; pesanan `External` lama dibiarkan sebagai riwayat dan tidak dipindahkan | Pemilik modul, dijalankan pemilik modul IGD | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul 2026-09-09; menjawab `RAD-CQ-005` |
| `RAD-DEC-010` | Decision | Pemberitahuan temuan kritis tetap atas nama pemesan dan sekaligus muncul di daftar pantau unit pasien; pengakuan oleh dokter lain diterima dan namanya dicatat terpisah | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul 2026-09-09; menjawab `RAD-OPEN-008` |
| `RAD-DEC-011` | Decision | Status `Draft` pada pesanan tidak dipakai; nilainya dibiarkan ada agar angka status lain tidak bergeser | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Jawaban pemilik modul 2026-09-09; menjawab `RAD-CQ-004` |
| `RAD-DEC-012` | Decision | Daftar kerja petugas dikelompokkan **per alat pencitraan**; berupa penyaringan atas data yang sudah ada, **tanpa tabel baru** | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Amendment pass 2026-09-09; menutup sebagian `DEC-RAD-003` |
| `RAD-DEC-013` | Decision | Dokter pengirim menandai pesanan sebagai cito; daftar kerja mendahulukannya. Menambah satu kolom pada tabel pesanan yang sudah ada | Pemilik modul | `approved` | Yoga Aji Pratama, 2026-09-09 | Amendment pass 2026-09-09; menutup sisa `DEC-RAD-003` |
| `RAD-CONFLICT-002` | Conflict | Modul IGD masih menyatakan modul Radiologi belum ada, padahal sudah rilis sejak 31 Agustus 2026. **Jalan keluar sudah ditetapkan `RAD-DEC-009`; menunggu tindakan pemilik modul IGD** | Pemilik modul IGD | `draft` | — | `EmergencyOrderKind.cs`; `emergency-assessment-diagnostic-support-tab.jsx:189` |
| `RAD-FACT-009` | Fact | Modul tidak dapat menjalankan satu pun pemeriksaan: gerbang keselamatan menolak bila aturan belum ada, dan tidak ada cara memasukkan aturan itu | — | `approved` | Source `64da911` | `RadSafetyGateEvaluator.cs#Evaluate`; tidak ada endpoint pengelolaan maupun seeder |

---

## Open Questions dan Blocker

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `RAD-OPEN-001` | `RJ-BIL-GATE-DEC-004` masih `OPEN` secara tata kelola. Siapa yang menandatangani dari sisi Radiologi, Clinical Governance, dan Billing/Finance? | Pemilik modul + Clinical Governance | `DESIGN` untuk bagian keselamatan dan kelayakan tagih |
| ~~`RAD-OPEN-002`~~ | ~~Apakah kedokteran nuklir dan USG termasuk modul ini, atau dipisah?~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-002`.** USG masuk, kedokteran nuklir keluar | Pemilik modul | — |
| ~~`RAD-OPEN-003`~~ | ~~Apakah pelewatan gerbang keselamatan darurat dibutuhkan, dan siapa yang mengesahkan?~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-008`**, dengan catatan tanda tangan tata kelola klinis masih tertunda | Clinical Governance | Tersisa pada `RAD-OPEN-001` |
| ~~`RAD-OPEN-004`~~ | ~~Siapa yang berwenang mengesahkan hasil bacaan, dan bolehkah penulis draf mengesahkan bacaannya sendiri?~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-003`** | Pemilik modul | — |
| `RAD-OPEN-005` | Registry `Rad` masih `PLANNED`. Kapan diperbarui menjadi aktif? | Pemegang registry | `IMPLEMENTATION` seluruh entity `Rad*` baru |
| `RAD-OPEN-009` | Apakah pesanan cito perlu dipantau keterlambatannya, dan berapa batas waktunya per jenis pemeriksaan? Ditunda oleh `RAD-DEC-013` karena butuh penetapan klinis | Clinical Governance | Tidak memblokir apa pun. Perluasan `POST-MVP` |
| `RAD-OPEN-006` | **Diperluas 2026-09-09.** Empat peran kini dirujuk keputusan tetapi belum satu pun dipetakan ke data peran dan `AccessPermission` yang benar-benar ada: "dokter radiolog" (`RAD-DEC-003`), "penanggung jawab klinis" (`RAD-DEC-005`), serta "DPJP" dan "dokter jaga senior" (`RAD-DEC-008`) | Pemilik modul + Administrator | `IMPLEMENTATION` pengesahan hasil bacaan, pengesahan aturan keselamatan, dan pelewatan darurat |
| `RAD-OPEN-007` | Apa saja yang dihitung sebagai temuan kritis radiologi? Daftarnya harus ditetapkan supaya penandaan tidak bergantung selera masing-masing radiolog | Clinical Governance | `IMPLEMENTATION` bagian temuan kritis. **Tidak memblokir** `DESIGN`, karena mekanismenya sudah dikunci `RAD-DEC-004` |
| ~~`RAD-OPEN-008`~~ | ~~Ke siapa pemberitahuan temuan kritis dialihkan bila dokter pemesan ganti jaga?~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-010`** | — | — |

### Closure question dari audit capability

Tujuh pertanyaan berikut diterbitkan audit `01-existing-capability-map.md` dan **belum
dijawab**. Semuanya menjadi bahan `Closure pass`.

| ID | Pertanyaan | Pemilik yang dibutuhkan | Memblokir |
|---|---|---|---|
| ~~`RAD-CQ-001`~~ | ~~Registry `PLANNED` sementara tabel sudah rilis~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-007`** | — | — |
| ~~`RAD-CQ-002`~~ | ~~Siapa berwenang mengelola aturan keselamatan per alat~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-005`** | — | — |
| `RAD-CQ-003` | Nilai awal aturan keselamatan disiapkan tim sebagai data awal, atau diketik admin rumah sakit sendiri? | Clinical Governance | `IMPLEMENTATION` data awal |
| ~~`RAD-CQ-004`~~ | ~~Perlu simpan draf pesanan, atau `Draft` sengaja tidak dipakai?~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-011`** | — | — |
| ~~`RAD-CQ-005`~~ | ~~Siapa memperbaiki `RAD-CONFLICT-002`, dan riwayat `External` dipindahkan atau dibiarkan?~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-009`** | — | — |
| ~~`RAD-CQ-006`~~ | ~~Hasil bacaan masuk rekam medis lewat `TrxPatientClinicalDocument` atau dibaca langsung?~~ **DITUTUP 2026-09-09 oleh `RAD-DEC-006`** | — | — |
| `RAD-CQ-007` | Uji kontrak hak akses radiologi wajib ada sebelum Rilis 1, mengikuti pola Laboratorium dan Bank Darah? | Pemilik modul | `IMPLEMENTATION` — dapat ditunda |

---

## Acceptance Criteria yang Sudah Dapat Diuji

Kriteria berikut sudah cukup tajam untuk dijadikan bahan uji. Sumbernya keputusan sesi ini,
bukan tebakan.

| No | Kriteria | Sumber |
|---:|---|---|
| 1 | Pengesahan draf bacaan yang ditulis bukan-radiolog oleh penulisnya sendiri ditolak dengan kode `403` | `RAD-DEC-003` |
| 2 | Pengesahan draf bacaan yang ditulis dokter radiolog oleh dirinya sendiri berhasil | `RAD-DEC-003` |
| 3 | Peran penulis draf tersimpan melekat pada draf, dan tidak ikut berubah ketika peran orang tersebut diperbarui kemudian | `RAD-DEC-003` |
| 4 | Riwayat menampilkan penulis dan pengesah secara terpisah, walaupun keduanya orang yang sama | `RAD-DEC-003` |
| 5 | Data induk alat pencitraan dapat ditambah, diubah, dan dinonaktifkan lewat antarmuka, tanpa menyentuh database langsung | `RAD-DEC-001` butir 14 |
| 6 | Butir keselamatan yang wajib berbeda antar alat, contohnya skrining implan logam wajib untuk MRI tetapi tidak untuk USG | `RAD-DEC-002` |
| 7 | Bacaan bertanda kritis tetap dapat dirilis tanpa menunggu pengakuan siapa pun | `RAD-DEC-004` |
| 8 | Perilisan bacaan bertanda kritis selalu menghasilkan tepat satu pemberitahuan berstatus `Belum dibaca` untuk dokter pemesan | `RAD-DEC-004` |
| 9 | Pencatatan kontak langsung ditolak bila waktu, cara menghubungi, atau nama penerima kosong | `RAD-DEC-004` |
| 10 | Bacaan bertanda kritis yang belum diakui tetap muncul di daftar pantau tunggakan | `RAD-DEC-004` |
| 11 | Koreksi berversi terhadap bacaan bertanda kritis menghasilkan pemberitahuan baru tanpa menghapus yang lama | `RAD-DEC-004` |
| 12 | Aturan keselamatan berstatus `Draf` atau `Menunggu persetujuan` tidak ikut dinilai gerbang keselamatan | `RAD-DEC-005` |
| 13 | Pengesahan aturan keselamatan oleh selain penanggung jawab klinis ditolak dengan `403` | `RAD-DEC-005` |
| 14 | Setiap pengesahan menaikkan nomor versi aturan tepat satu kali | `RAD-DEC-005` |
| 15 | Penolakan pengesahan aturan tanpa alasan ditolak dengan `400` | `RAD-DEC-005` |
| 16 | Study yang sudah lolos mempertahankan nomor versi aturan saat itu, walau aturannya berubah kemudian | `RAD-DEC-005` |
| 17 | Layar pengelolaan menampilkan peringatan untuk setiap alat yang belum punya aturan keselamatan aktif | `RAD-DEC-005` |
| 18 | Tidak ada satu pun tabel di luar modul Radiologi yang menyimpan isi hasil bacaan | `RAD-DEC-006` |
| 19 | Koreksi berversi langsung terlihat di rekam medis tanpa langkah penyalinan apa pun | `RAD-DEC-006` |
| 20 | Ketika modul Radiologi tidak dapat dihubungi, rekam medis menampilkan pesan gangguan, bukan daftar kosong | `RAD-DEC-006` |
| 21 | Slot `PatientClinicalDocumentSource.Radiology` hanya terisi oleh berkas unggahan dari luar | `RAD-DEC-006` |
| 22 | Pelewatan gerbang keselamatan oleh petugas radiologi ditolak dengan `403` | `RAD-DEC-008` |
| 23 | Pelewatan tanpa alasan tertulis ditolak dengan `400` | `RAD-DEC-008` |
| 24 | Pelewatan tidak mengubah keadaan butir keselamatan yang dilewati | `RAD-DEC-008` |
| 25 | Study hasil pelewatan membawa penandanya selamanya dan tidak dapat dihapus | `RAD-DEC-008` |
| 26 | Setiap butir yang dilewati tercatat sendiri-sendiri, bukan sebagai satu penanda menyeluruh | `RAD-DEC-008` |
| 27 | Daftar tinjauan menampilkan seluruh study yang berjalan lewat pelewatan | `RAD-DEC-008` |
| 28 | Pesanan radiologi dari IGD memakai endpoint yang sama dengan poli dan rawat inap, tanpa perlakuan khusus | `RAD-DEC-009` |
| 29 | Tidak ada satu pun pesanan `External` lama yang diubah bentuknya | `RAD-DEC-009` |
| 30 | Teks yang menyatakan modul Radiologi belum ada tidak lagi muncul di layar mana pun | `RAD-DEC-009` |
| 31 | Pemberitahuan temuan kritis muncul di kotak dokter pemesan **dan** daftar pantau unit pasien | `RAD-DEC-010` |
| 32 | Pengakuan oleh dokter selain pemesan diterima, dan namanya dicatat terpisah | `RAD-DEC-010` |
| 33 | Pemberitahuan hilang dari daftar pantau unit hanya setelah diakui, bukan setelah dibaca | `RAD-DEC-010` |
| 34 | Tidak ada satu pun endpoint yang menghasilkan pesanan berstatus `Draft` | `RAD-DEC-011` |
| 35 | Nilai `Draft` tetap ada di daftar status dengan angka yang tidak berubah | `RAD-DEC-011` |
| 36 | Daftar kerja menampilkan hanya pemeriksaan pada alat yang dipilih | `RAD-DEC-012` |
| 37 | Daftar kerja tidak membaca satu pun tabel di luar yang sudah ada | `RAD-DEC-012` |
| 38 | Petugas dapat berpindah antar daftar alat tanpa berganti halaman | `RAD-DEC-012` |
| 39 | Pesanan bercito muncul di urutan atas, mendahului pesanan biasa yang lebih tua | `RAD-DEC-013` |
| 40 | Penanda cito terlihat tanpa perlu membuka rincian pesanan | `RAD-DEC-013` |
| 41 | Penanda cito ikut terbawa ke seluruh study yang lahir dari pesanan itu | `RAD-DEC-013` |
| 42 | Penanda cito tercatat siapa yang menetapkannya dan kapan | `RAD-DEC-013` |

Acceptance criteria warisan dari `RJ-BIL-GATE-DEC-004` tetap berlaku penuh dan tidak diulang
di sini.

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-09 | `Scope pass` dimulai. Fakta source code dikumpulkan, batas scope diusulkan, konflik registry dicatat. | `draft` |
| 2 | 2026-09-09 | Batas scope dikunci lewat `RAD-DEC-001` dan `RAD-DEC-002`. `RAD-OPEN-002` ditutup. | `draft` |
| 3 | 2026-09-09 | `RAD-DEC-003` mengunci kewenangan pengesahan hasil bacaan. `RAD-OPEN-004` ditutup, `RAD-OPEN-006` dibuka. Enam acceptance criteria pertama disusun. | `draft` |
| 4 | 2026-09-09 | `RAD-DEC-004` mengunci penanganan temuan kritis beserta proses bisnis dan tabel perubahan statusnya. `RAD-OPEN-007` dan `RAD-OPEN-008` dibuka. Acceptance criteria menjadi 11 butir. `Scope pass` dinyatakan selesai. | `draft` |
| 5 | 2026-09-09 | Disinkronkan dengan `01-existing-capability-map.md` revision 1. `RAD-FACT-009` dan `RAD-CONFLICT-002` ditambahkan, `RAD-CONFLICT-001` diperdalam, tujuh closure question `RAD-CQ-001` sampai `RAD-CQ-007` dicatat. | `draft` |
| 6 | 2026-09-09 | `Closure pass` putaran 1. `RAD-DEC-005`, `RAD-DEC-006`, dan `RAD-DEC-007` dikunci; `RAD-CQ-001`, `RAD-CQ-002`, dan `RAD-CQ-006` ditutup. Acceptance criteria menjadi 21 butir. | `draft` |
| 8 | 2026-09-09 | `Amendment pass`. `RAD-DEC-012` dan `RAD-DEC-013` mengunci bentuk daftar kerja petugas dan penanda cito. `DEC-RAD-003` ditutup, `RAD-OPEN-009` dibuka. Slice `S12` naik menjadi siap dan **tidak** tertahan blocker registry. Acceptance criteria menjadi 42 butir. | `draft` |
| 7 | 2026-09-09 | `Closure pass` putaran 2. `RAD-DEC-008` sampai `RAD-DEC-011` dikunci; `RAD-OPEN-003`, `RAD-OPEN-008`, `RAD-CQ-004`, dan `RAD-CQ-005` ditutup. Jalan keluar kedua conflict sudah ditetapkan. Acceptance criteria menjadi 35 butir. `Closure pass` dinyatakan selesai. | `draft` |
