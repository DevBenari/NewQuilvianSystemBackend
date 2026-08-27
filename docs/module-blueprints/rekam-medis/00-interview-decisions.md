# Rekam Medis — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Revision | `5` |
| Status | **`approved`** — 22 keputusan disahkan 26 Agustus 2026 |
| Interview mode | `Amendment pass` (revision 3, 4, dan 5); `Closure pass` (revision 2); `Scope pass` (revision 1) |
| Input capability map | `01-existing-capability-map.md` revision `2`, penelusuran lanjutan 24 Agustus 2026 |
| Product/domain owner | **Yoga Aji Pratama** — ditetapkan 26 Agustus 2026 |
| Clinical governance owner | **Yoga Aji Pratama** — ditetapkan 26 Agustus 2026 |
| Security/privacy owner | **Yoga Aji Pratama** — ditetapkan 26 Agustus 2026 |
| Catatan kepemilikan | Ketiga peran dipegang **satu orang**. Konsekuensi dan batasnya dicatat pada `RM-DEC-027` |
| Backend SHA | `ab37e3a` |
| Frontend SHA | `c4e2ef2a6` |
| Tanggal pass | 24 Agustus 2026; pengesahan 26 Agustus 2026 |
| Primary evidence | Pembacaan source backend dan frontend pada SHA di atas; belum ada dokumen SOP rekam medis yang diserahkan |

> **Status pengesahan.** Dua puluh dua keputusan pada dokumen ini berstatus `approved`,
> disahkan Yoga Aji Pratama selaku penanggung jawab modul pada 26 Agustus 2026. Empat
> pertanyaan terbuka berstatus `superseded` karena sudah digantikan keputusan lain, dan satu
> (`RM-DEC-007`) masih terbuka tetapi tidak memblokir pekerjaan mana pun.
>
> **Batas yang tetap berlaku.** Pengesahan ini sah untuk memulai pembangunan. Ia **tidak**
> menggantikan tinjauan komite medik maupun pihak perlindungan data bila kelak keduanya
> ditunjuk — lihat `RM-DEC-027`.

---

## Scope dan Outcome

### Kalimat batas scope (dikonfirmasi pemohon sesi, `RM-DEC-001`)

> Modul Rekam Medis mengelola **berkas rekam medis pasien sebagai satu kesatuan** — penelusuran
> riwayat lintas kunjungan, keutuhan dan keabsahan dokumen, kerahasiaan akses, serta kelengkapan
> berkas. Modul ini **tidak menulis isi klinis**; isi klinis tetap dibuat oleh modul pelayanan.

Penjelasan untuk pembaca non-teknis: bayangkan rumah sakit versi kertas. Perawat dan dokter
menulis di lembar asesmen, lembar CPPT, dan lembar resep — itu pekerjaan unit pelayanan.
Unit rekam medis tidak ikut menulis isi lembar tersebut. Yang dikerjakan unit rekam medis
adalah menyatukan semua lembar itu ke dalam satu map atas nama satu pasien, memastikan map
tersebut lengkap dan tidak bisa diubah diam-diam, mencatat siapa saja yang meminjam map itu,
dan menyimpannya sesuai masa retensi. Modul ini adalah versi digital dari pekerjaan tersebut.

### Outcome yang diharapkan (usulan)

- **Assumption:** Satu pasien memiliki satu berkas rekam medis yang dapat ditelusuri lintas
  seluruh kunjungan, terlepas dari unit pelayanan mana yang mencatat.
- **Assumption:** Catatan klinis yang sudah final tidak dapat diubah diam-diam; koreksi
  meninggalkan jejak.
- **Assumption:** Setiap pembukaan berkas rekam medis pasien tercatat, sehingga kerahasiaan
  dapat diaudit.
- **Assumption:** Modul dapat dipakai sejak awal dengan data yang ada sekarang, lalu makin
  lengkap ketika modul Laboratorium, Radiologi, MCU, dan Optik menyusul.

### Di dalam scope (usulan, menunggu konfirmasi)

| No | Cakupan | Penjelasan singkat |
|---:|---|---|
| 1 | Penelusuran berkas rekam medis pasien | Menampilkan seluruh riwayat pasien lintas kunjungan dalam satu tempat, urut waktu |
| 2 | Keutuhan dokumen klinis | Penguncian dokumen setelah final, koreksi lewat addendum, tanda tangan elektronik |
| 3 | Jejak audit akses | Mencatat siapa membuka berkas rekam medis siapa, kapan, dan untuk keperluan apa |
| 4 | Kelengkapan berkas | Memeriksa dokumen wajib yang belum terisi setelah kunjungan selesai |
| 5 | Verifikasi koding diagnosis dan tindakan | Petugas koder memeriksa dan membetulkan kode ICD sebelum berkas dinyatakan lengkap |
| 6 | Resume medis | Ringkasan pelayanan yang dapat dicetak dan diserahkan ke pasien atau pihak berwenang |
| 7 | Peminjaman dan pelepasan informasi | Permintaan salinan rekam medis oleh pasien, asuransi, atau penegak hukum |
| 8 | Retensi dan pemusnahan | Aturan berapa lama berkas disimpan dan bagaimana pemusnahannya dicatat |

### Di luar scope (usulan, menunggu konfirmasi)

| No | Yang tidak dikerjakan modul ini | Pemilik yang seharusnya |
|---:|---|---|
| 1 | Pembuatan isi klinis: asesmen, SOAP, CPPT, diagnosis, tindakan | `ClinicalManagement` (sudah ada) |
| 2 | Master data pasien dan penomoran rekam medis awal | `PatientManagement` (sudah ada, `MstPatient.MedicalRecordNumber`) |
| 3 | Pendaftaran dan kunjungan | `RegistrationManagement` (sudah ada) |
| 4 | Produksi hasil laboratorium, radiologi, MCU, optik | Modul masing-masing (belum ada) |
| 5 | Penagihan, klaim, dan pembayaran | Billing (baru ada master data) |
| 6 | Resep dan penyerahan obat | `PharmacyManagement` (sudah ada) |

### Di luar scope — untuk modul lain

- **Integrasi SATUSEHAT/FHIR.** Belum ada implementasi apa pun di source; hanya disebut pada
  dokumen keputusan IGD. Perlu modul integrasi tersendiri dengan owner tersendiri.
- **Modul Rawat Inap.** Belum ada area-nya di backend. Rekam medis rawat inap akan bergantung
  pada modul ini ketika dibangun.

---

## Glossary

Istilah berikut dipakai konsisten di seluruh blueprint. Seluruh makna kerja di bawah ikut
disahkan bersama keputusan induknya pada 26 Agustus 2026 (`RM-DEC-027`); kolom status yang
masih tertulis `draft` merujuk pada revisi sebelumnya dan tidak lagi berlaku.

| Istilah | Makna kerja saat ini | Status |
|---|---|---|
| Berkas rekam medis | Seluruh catatan klinis milik satu pasien, dari semua kunjungan, dilihat sebagai satu kesatuan | `draft` |
| Catatan klinis | Satu dokumen yang dibuat tenaga klinis: asesmen, CPPT, catatan SOAP, surat medis, dan sejenisnya | `draft` |
| Terkunci | Kondisi catatan yang tidak dapat lagi diubah isinya. Perubahan hanya mungkin dengan menambah addendum. Ditetapkan pada `RM-DEC-003` | `draft` |
| Ditandatangani | Pernyataan penulis bahwa catatannya sudah final dan menjadi tanggung jawabnya. Menandatangani otomatis mengunci | `draft` |
| `TidakDitandatangani` | Penanda pada catatan yang terkunci otomatis saat kunjungan ditutup karena penulisnya belum sempat menandatangani. Catatan tetap sah dibaca, tetapi ditandai kurang lengkap | `draft` |
| Addendum | Catatan koreksi atau tambahan yang ditempelkan pada catatan yang sudah terkunci. Isi lama tetap terbaca; addendum tidak menimpa, hanya menambah | `draft` |
| Entri susulan | Hasil atau dokumen yang masuk setelah kunjungan ditutup, tetapi memang milik kunjungan tersebut. Ditetapkan pada `RM-DEC-006` | `draft` |
| Pasien rawatan | Pasien yang sedang ditangani pengguna. Definisi yang dapat diuji sistem belum ditetapkan — lihat `RM-DEC-009` | `draft` |
| Akses beralasan | Pembukaan rekam medis pasien di luar rawatan pengguna. Diizinkan, tetapi wajib mengisi alasan dan ditandai untuk ditinjau. Ditetapkan pada `RM-DEC-005` | `draft` |
| Jejak akses | Catatan permanen berisi siapa membuka berkas rekam medis siapa, kapan, dan dengan alasan apa | `draft` |
| Berhalangan | Kondisi penulis catatan yang membuat ia tidak dapat membuat addendum sendiri. Definisi yang dapat diuji belum ditetapkan — lihat `RM-DEC-010` | `draft` |

---

## State dan Transition (usulan, turunan `RM-DEC-003`)

Status berikut berlaku untuk **satu catatan klinis**, bukan untuk kunjungan.

| Status | Arti bagi pengguna | Bisa diubah isinya? |
|---|---|---|
| `Draft` | Sedang ditulis, belum selesai | Ya, oleh penulisnya |
| `Signed` | Penulis sudah menandatangani dan menyatakan final | Tidak. Hanya bisa ditambah addendum |
| `LockedUnsigned` | Terkunci otomatis karena kunjungan ditutup, tetapi penulis belum sempat menandatangani | Tidak. Hanya bisa ditambah addendum |
| `Cancelled` | Dibatalkan sebelum final, misalnya salah pasien | Tidak. Tetap tersimpan dan terbaca sebagai catatan yang dibatalkan |

Perpindahan yang diizinkan:

| Dari | Ke | Pemicu |
|---|---|---|
| `Draft` | `Signed` | Penulis menandatangani |
| `Draft` | `LockedUnsigned` | Kunjungan ditutup sementara catatan belum ditandatangani |
| `Draft` | `Cancelled` | Penulis membatalkan dengan alasan |
| `Signed` | — | Tidak ada. Perubahan hanya lewat addendum |
| `LockedUnsigned` | — | Tidak ada. Perubahan hanya lewat addendum |

Yang **tidak pernah** boleh terjadi: kembali dari `Signed` atau `LockedUnsigned` ke `Draft`.
Ini turunan langsung dari `RM-DEC-006` yang menyatakan kunjungan tidak pernah dibuka kembali.

---

## Skenario Normal dan Exception

### Skenario normal — pasien rawat jalan

1. Pasien mendaftar, kunjungan terbentuk di modul Pendaftaran.
2. Perawat mengisi asesmen awal. Selesai mengisi, perawat menandatangani. Asesmen menjadi
   `Signed` dan terkunci.
3. Dokter memeriksa, menulis catatan SOAP dan diagnosis, lalu menandatangani. Catatan menjadi
   `Signed`.
4. Kunjungan ditutup. Tidak ada catatan yang tertinggal berstatus `Draft`.
5. Seluruh catatan tersebut muncul pada layar berkas rekam medis pasien, urut waktu.

### Exception 1 — catatan lupa ditandatangani

Dokter menulis catatan SOAP, dipanggil ke tindakan lain, dan lupa menandatangani. Kunjungan
ditutup sore harinya. Catatan otomatis menjadi `LockedUnsigned` dan diberi penanda. Isinya
tetap sah dibaca sebagai bagian rekam medis. Pemeriksaan kelengkapan akan menampilkannya
sebagai kekurangan yang perlu ditindaklanjuti. Dokter tidak bisa lagi mengedit; bila ada yang
perlu dibetulkan, ia menambah addendum.

### Exception 2 — salah tulis diketahui belakangan

Dokter menandatangani CPPT hari Senin. Rabu ia sadar menulis dosis yang salah. Ia membuka
catatan itu, menambah addendum berisi pembetulan dan alasannya. Isi Senin tetap terbaca apa
adanya, dengan addendum tertempel di bawahnya. Pembaca melihat keduanya dan tahu urutan
kejadiannya.

### Exception 3 — penulis catatan berhalangan

Perawat menemukan kesalahan pada catatan dokter yang sedang cuti dua minggu. Perawat tidak
berhak menambah addendum pada catatan dokter tersebut. Yang berhak adalah kepala unit atau
DPJP, dan addendum dibuat atas nama orang tersebut, bukan atas nama dokter yang cuti, dengan
alasan wajib diisi. Turunan `RM-DEC-004`.

### Exception 4 — hasil pemeriksaan keluar setelah kunjungan ditutup

Pasien pulang Senin, kunjungan ditutup. Rabu hasil kultur darah keluar. Hasil tersebut
tercatat sebagai entri susulan yang tertaut ke kunjungan Senin, diberi penanda `Susulan` dan
tanggal masuk. Kunjungan tidak dibuka kembali dan catatan lama tidak tersentuh. Turunan
`RM-DEC-006`.

### Exception 5 — membuka rekam medis pasien yang bukan rawatannya

Dokter jaga malam perlu melihat riwayat pasien yang baru masuk dan belum terdaftar sebagai
rawatannya. Sistem tetap membuka berkas tersebut, tetapi meminta alasan singkat lebih dulu.
Akses itu tercatat dan ditandai untuk ditinjau unit rekam medis. Pelayanan tidak pernah
tertahan oleh mekanisme ini. Turunan `RM-DEC-005`.

---

## Frontend Decision Authority

Urutan kewenangan yang berlaku: keamanan/privasi/invariant → brief produk atau UI yang
disetujui → konvensi proyek → kebijakan developer.

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `RM-FE-001` | Layar wajib menampilkan penanda status catatan (`Draft`, `Signed`, `LockedUnsigned`, `Cancelled`) dan penanda `Susulan` | Clinical governance owner | `draft` | Wajib terlihat; bentuk visual bebas | `RM-DEC-003`, `RM-DEC-006` |
| `RM-FE-002` | Addendum wajib ditampilkan menempel pada catatan asalnya, bukan sebagai catatan terpisah yang berdiri sendiri | Clinical governance owner | `draft` | Wajib; tata letak bebas | `RM-DEC-004` |
| `RM-FE-003` | Isian alasan pada akses beralasan wajib muncul sebelum isi rekam medis terlihat | Security/privacy owner | `draft` | Wajib mendahului tampilan isi | `RM-DEC-005` |
| `RM-FE-006` | Layar wajib memberi keterangan bahwa label tingkat kerahasiaan belum membatasi akses | Security/privacy owner | `draft` | Wajib ada; susunan kalimat bebas | `RM-DEC-018` |
| `RM-FE-007` | `PrivateNote` tidak boleh muncul pada tampilan rutin; hanya tampil setelah jalur akses beralasan ditempuh | Security/privacy owner | `draft` | Wajib; bentuk tampilannya bebas | `RM-DEC-022` |
| `RM-FE-008` | Status keutuhan dan status alur kerja harus dapat dibedakan pembaca, tidak boleh tampil sebagai satu penanda tunggal | Clinical governance owner | `draft` | Wajib dapat dibedakan; bentuk visual bebas | `RM-DEC-013` |
| `RM-FE-004` | Bentuk navigasi berkas rekam medis: menu, rute, tab, modal, atau drawer | Frontend | `DEV_DISCRETION` | Mengikuti konvensi proyek yang sudah ada | Belum ada brief UI yang disetujui |
| `RM-FE-005` | Tata letak, warna, ikon, dan komponen tabel | Frontend | `DEV_DISCRETION` | Mengikuti konvensi proyek yang sudah ada | Belum ada brief UI yang disetujui |

---

## Acceptance Criteria (dapat diuji, masih `draft`)

| No | Kriteria |
|---:|---|
| 1 | Catatan berstatus `Signed` atau `LockedUnsigned` menolak permintaan perubahan isi, dan menjawab dengan pesan bahwa koreksi harus lewat addendum |
| 2 | Menandatangani catatan mengubah statusnya menjadi `Signed` dan mencatat siapa serta kapan |
| 3 | Menutup kunjungan mengubah seluruh catatan `Draft` di dalamnya menjadi `LockedUnsigned` |
| 4 | Addendum yang dibuat pihak selain penulis asli ditolak, kecuali pembuatnya kepala unit atau DPJP dan alasannya terisi |
| 5 | Isi catatan sebelum addendum tetap dapat dibaca utuh setelah addendum ditambahkan |
| 6 | Setiap pembukaan berkas rekam medis menghasilkan satu baris jejak akses berisi pengguna, pasien, waktu, dan alasan bila ada |
| 7 | Membuka rekam medis di luar pasien rawatan tanpa mengisi alasan tidak menampilkan isi rekam medis |
| 8 | Entri susulan pada kunjungan yang sudah ditutup tersimpan dengan penanda `Susulan` dan tanggal masuk, tanpa mengubah status kunjungan |
| 9 | Layar berkas rekam medis menampilkan catatan dari seluruh kunjungan pasien, urut waktu, tanpa perlu membuka kunjungan satu per satu |
| 10 | Setiap dokumen klinis memiliki status keutuhan bernilai salah satu dari `Draft`, `Signed`, `LockedUnsigned`, `Cancelled`, terpisah dari status alur kerjanya (`RM-DEC-013`) |
| 11 | Setelah migration dijalankan, catatan pada kunjungan yang sudah selesai atau batal bernilai `LockedUnsigned`, dan catatan pada kunjungan berjalan bernilai `Draft` (`RM-DEC-014`) |
| 12 | Membuka rekam medis pasien **tanpa** kunjungan aktif tanpa mengisi alasan tidak menampilkan isi rekam medis; membuka rekam medis pasien **dengan** kunjungan aktif berjalan tanpa isian alasan (`RM-DEC-016`) |
| 13 | Pengguna ber-role `SuperAdmin` yang membuka rekam medis pasien tanpa kunjungan aktif tetap diminta alasan, dan aksesnya tercatat (`RM-DEC-017`) |
| 14 | Jalur addendum pengganti hanya terbuka bila akun penulis nonaktif, atau bila ada penetapan kepala unit yang menyimpan penetap, waktu, dan alasan (`RM-DEC-020`) |
| 15 | Menandatangani catatan menyimpan identitas pengguna yang sedang masuk, waktu, dan perangkat, tanpa meminta pengesahan ulang (`RM-DEC-021`) |
| 16 | `PrivateNote` tidak muncul pada tampilan rekam medis rutin, dan hanya tampil setelah pengguna menempuh jalur akses beralasan yang tercatat (`RM-DEC-022`) |
| 17 | Layar menampilkan keterangan bahwa label tingkat kerahasiaan belum membatasi akses (`RM-DEC-018`) |

---

## Fakta dari source code (bukan keputusan manusia)

Fakta berikut dibaca langsung dari source pada SHA yang tercatat di atas. Fakta ini dipakai
untuk menyusun pertanyaan, bukan sebagai keputusan yang sudah disetujui.

### Isi rekam medis yang sudah tersedia

**Fact:** Area `Areas/HealthServices/ClinicalManagement` sudah berisi 13 model transaksi dan
15 controller yang isinya merupakan komponen rekam medis.

| Komponen rekam medis | Model yang sudah ada |
|---|---|
| Asesmen pasien | `TrxPatientAssessment` |
| Catatan Perkembangan Pasien Terintegrasi (CPPT) | `TrxPatientIntegratedProgressNote` |
| Konsultasi dokter dan catatan SOAP | `TrxDoctorConsultation` |
| Diagnosis pasien | `TrxPatientDiagnosis` |
| Tanda vital | `TrxPatientVitalSign` |
| Alergi | `TrxPatientAllergy` |
| Riwayat penyakit dan riwayat keluarga | `TrxPatientMedicalHistory`, `TrxPatientFamilyHistory` |
| Tindakan | `TrxPatientProcedure` |
| Dokumen klinis dan lampiran | `TrxPatientClinicalDocument`, `TrxClinicalNoteAttachment` |
| Surat keterangan medis | `TrxMedicalCertificate` |
| Persetujuan tindakan | `TrxPatientConsent` |

**Fact:** Nomor rekam medis sudah ada sebagai `MedicalRecordNumber` pada `MstPatient`.

**Fact:** Kode diagnosis ICD-10 sudah tersedia lewat `Seeders/Icd10DiagnosisSeeder.cs`, dan
`MstDiagnosis` menyimpan `DiagnosisType` (`ICD10`, `Local`, `Custom`) serta `IcdVersion`.

**Fact:** `TrxPatientIntegratedProgressNote` sudah menyediakan tiga kolom penyambung ke modul
lain: `SourceModule`, `SourceReferenceId`, dan `SourceReferenceNumber`. Artinya CPPT memang
dirancang untuk menerima titipan entri dari modul mana pun, termasuk modul yang belum
dibangun.

### Kekosongan yang ditemukan

**Fact:** Tidak ada mekanisme penguncian, tanda tangan, atau addendum pada catatan klinis.
Penelusuran kata `SignedAt`, `IsSigned`, `IsLocked`, `Amendment`, `Addendum`, dan
`IsFinalized` di seluruh `ClinicalManagement` hanya menemukan `SignedAt` pada
`TrxPatientConsent`. CPPT dan konsultasi dokter tidak memilikinya.

**Fact:** Kelas dasar `Models/IdentityModel.cs` hanya menyimpan `CreateDateTime`, `CreateBy`,
`UpdateDateTime`, `UpdateBy`, penanda batal, dan penanda hapus. Nilai lama sebuah data tidak
disimpan di mana pun. Bahasa awamnya: kalau catatan diubah, isi sebelumnya hilang dan tidak
bisa ditelusuri lagi.

**Fact:** `DoctorConsultationStatus` hanya memiliki empat nilai: `Draft`, `InProgress`,
`Completed`, `Cancelled`. Tidak ada status yang membedakan "sudah selesai ditulis" dari
"sudah ditandatangani dan terkunci".

**Fact:** Tidak ditemukan pencatatan jejak akses baca. Sistem saat ini tidak menyimpan siapa
membuka rekam medis pasien mana.

**Fact:** Di frontend, isi rekam medis hanya dapat dilihat dari dalam layar antrean dokter,
melalui tab `cppt/`, `soap/`, `procedure/`, `prescription/`, dan `certificate/` di bawah
`src/components/view/health-services/registration-management/doctor-queues/tabs/`. Tidak ada
halaman rekam medis pasien yang berdiri sendiri dan tidak ada penelusuran lintas kunjungan.

**Fact:** Tidak ada area `MedicalRecordManagement` di backend, dan tidak ada rute
`rekam-medis` di frontend.

---

## Aktor dan Tanggung Jawab (usulan awal)

| Aktor | Tanggung jawab yang diusulkan | Status |
|---|---|---|
| Petugas rekam medis | Memeriksa kelengkapan berkas, melayani peminjaman, mengelola retensi | `draft` |
| Koder | Memverifikasi dan membetulkan kode diagnosis dan tindakan | `draft` |
| Kepala unit rekam medis | Menyetujui pelepasan informasi dan pemusnahan berkas | `draft` |
| Dokter | Menandatangani dan mengunci catatan klinis miliknya; membuat addendum bila perlu koreksi | `draft` |
| Perawat | Menandatangani catatan miliknya sesuai kewenangan | `draft` |
| Auditor internal | Membaca jejak akses dan jejak perubahan | `draft` |
| Pasien atau wali | Meminta salinan rekam medis atau resume medis | `draft` |
| Product/domain owner | Memutus scope dan aturan bisnis rekam medis | `OPEN — belum ditetapkan` |
| Clinical governance owner | Menyetujui aturan penandatanganan, penguncian, dan addendum | `OPEN — belum ditetapkan` |
| Security/privacy owner | Menyetujui aturan akses, jejak audit, dan pelepasan informasi | `OPEN — belum ditetapkan` |

---

## Bahan audit untuk `/trace-existing-capabilities`

Pertanyaan berikut **tidak ditanyakan ke pemilik proses** karena jawabannya ada di source
code. Semuanya diteruskan ke tahap audit kemampuan.

| No | Yang perlu dibuktikan dari source |
|---:|---|
| 1 | Seberapa jauh `TrxPatientProcedure` sudah dipakai alur berjalan, dan apakah aman diperluas |
| 2 | Apakah `DoctorConsultationLifecycleService` sudah punya titik finalisasi yang bisa dijadikan tempat penguncian |
| 3 | Bagaimana pola autorisasi berjalan: `SysAccessPolicy`, `SysActionAccess`, `SysControllerAccess`, dan apakah mendukung batasan per pasien |
| 4 | Apakah sudah ada middleware atau filter yang mencatat aktivitas pengguna |
| 5 | Bagaimana pola penyimpanan berkas pada `Storage/` dan `TrxClinicalNoteAttachment` |
| 6 | Apakah `MedicalRecordNumber` dijamin unik dan bagaimana cara pembentukannya |
| 7 | Pola endpoint agregasi yang sudah dipakai, misalnya `PrescriptionWorkspaceController` |
| 8 | Cara frontend memuat data klinis pada tab antrean dokter, untuk menilai kemungkinan pemakaian ulang |

---

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RM-DEC-001` | Decision | Modul rekam medis berperan sebagai **pengelola berkas dan keabsahannya**, bukan pembuat isi klinis. Pembuatan asesmen, CPPT, SOAP, diagnosis, dan tindakan tetap milik `ClinicalManagement`. Dua modul ini terhubung lewat kontrak: kapan sebuah dokumen dianggap final dan diserahkan ke pengelolaan rekam medis. | Product/domain owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Wawancara Scope Pass pertanyaan 1 |
| `RM-DEC-002` | Decision | Rilis pertama mencakup tiga hal: **penelusuran berkas lintas kunjungan**, **keutuhan dokumen** (penguncian, addendum, tanda tangan), dan **jejak audit akses**. Cakupan 4 sampai 8 tetap menjadi bagian modul, dikerjakan pada rilis berikutnya. Alasan: keutuhan dan jejak akses adalah aturan yang wajib dipatuhi modul Laboratorium, Radiologi, dan MCU, sehingga harus ada sebelum ketiganya dibangun. | Product/domain owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Wawancara Scope Pass pertanyaan 2 |
| `RM-DEC-003` | Decision | Penguncian catatan klinis memakai **dua lapis**. Lapis pertama: catatan terkunci saat penulisnya menandatangani. Lapis kedua: catatan yang belum ditandatangani sampai kunjungan ditutup terkunci otomatis dan diberi penanda `TidakDitandatangani`. Setelah terkunci, koreksi hanya lewat addendum. | Clinical governance owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Wawancara Scope Pass pertanyaan 3 |
| `RM-DEC-004` | Decision | Addendum pada catatan terkunci hanya boleh dibuat **penulis asli**. Bila penulis berhalangan, **kepala unit atau DPJP** boleh membuat addendum atas namanya sendiri dengan alasan wajib. Isi lama tidak pernah dihapus pada kondisi apa pun. | Clinical governance owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Wawancara Scope Pass pertanyaan 4 |
| `RM-DEC-005` | Decision | Akses rekam medis bersifat **terbuka bagi tenaga klinis berwenang dengan rem**. Membuka rekam medis pasien yang sedang dirawat pengguna berjalan tanpa hambatan. Membuka rekam medis di luar rawatannya tetap diizinkan, tetapi wajib mengisi alasan dan akses tersebut ditandai untuk ditinjau unit rekam medis. Seluruh pembukaan dicatat. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Wawancara Scope Pass pertanyaan 5 |
| `RM-DEC-006` | Decision | Kunjungan yang sudah ditutup **tidak pernah dibuka kembali**. Hasil atau dokumen susulan dicatat sebagai entri baru yang tertaut ke kunjungan tersebut, diberi penanda `Susulan` beserta tanggal masuk. Catatan yang sudah terkunci tetap terkunci. | Product/domain owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Wawancara Scope Pass pertanyaan 6 |
| `RM-DEC-007` | Open Question | Sampai kapan entri susulan masih diterima untuk sebuah kunjungan yang sudah ditutup, dan bagaimana pemeriksaan kelengkapan memperlakukan berkas yang masih menunggu hasil? Turunan langsung dari `RM-DEC-006`. | Product/domain owner | `draft` | — | Wawancara Scope Pass pertanyaan 6 |
| `RM-DEC-008` | Decision | Tiga owner ditetapkan **sebelum** audit kemampuan dijalankan: pemilik proses (unit rekam medis), pemilik tata kelola klinis, dan pemilik keamanan/privasi. Keputusan `RM-DEC-001` sampai `RM-DEC-007` baru boleh naik status menjadi `approved` setelah owner yang sesuai bidangnya menyetujui. | Pemohon sesi | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Preseden blueprint IGD: tiga gate go-live tertahan karena owner ditunjuk belakangan |
| `RM-DEC-009` | Open Question | Definisi **pasien rawatan** yang dapat diuji sistem: apakah berdasarkan DPJP yang tercatat, unit pelayanan pengguna, keterlibatan pada kunjungan aktif, atau gabungan? Menentukan kapan sistem meminta alasan pada `RM-DEC-005`. | Security/privacy owner | `superseded` | Digantikan `RM-DEC-016`, disahkan Yoga Aji Pratama 26 Agustus 2026 | Turunan `RM-DEC-005` |
| `RM-DEC-010` | Open Question | Definisi **berhalangan** yang dapat diuji sistem: akun nonaktif, cuti terdaftar, atau penetapan manual kepala unit? Menentukan kapan jalur addendum pengganti pada `RM-DEC-004` terbuka. | Clinical governance owner | `superseded` | Digantikan `RM-DEC-020`, disahkan Yoga Aji Pratama 26 Agustus 2026 | Turunan `RM-DEC-004` |
| `RM-DEC-011` | Open Question | Apakah tanda tangan elektronik pada `RM-DEC-003` cukup berupa pencatatan identitas pengguna yang sedang masuk, atau memerlukan pengesahan ulang seperti memasukkan kata sandi atau sidik jari? Sistem sudah memiliki `ApplicationUserFingerprintCredential`. | Security/privacy owner | `superseded` | Digantikan `RM-DEC-021`, disahkan Yoga Aji Pratama 26 Agustus 2026 | Turunan `RM-DEC-003` |
| `RM-DEC-012` | Open Question | Apakah kolom `PrivateNote` pada CPPT ikut tampil di berkas rekam medis, dan siapa yang boleh membacanya? Kolom ini sudah ada di source tetapi aturan kerahasiaannya belum pernah diputuskan. | Security/privacy owner | `superseded` | Digantikan `RM-DEC-022`, disahkan Yoga Aji Pratama 26 Agustus 2026 | `TrxPatientIntegratedProgressNote.PrivateNote` |
| `RM-DEC-013` | Decision | Keragaman empat model status diselesaikan dengan **menambah satu status keutuhan baru yang berdampingan**, bukan menyeragamkan enum yang sudah ada. Status lama tetap mengurus alur kerja tiap dokumen; status keutuhan baru (`Draft`, `Signed`, `LockedUnsigned`, `Cancelled`) berlaku seragam untuk semua dokumen dan menjadi satu-satunya acuan aturan penguncian. Alasan: menghindari perubahan pada entity yang sedang dipakai IGD, antrean dokter, dan farmasi, yang tidak punya uji otomatis sebagai jaring pengaman. | Product/domain owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 1; menutup `RM-CAP-009` |
| `RM-DEC-014` | Decision | Catatan klinis yang sudah tersimpan diberi status keutuhan awal berdasarkan keadaan kunjungannya. Catatan pada kunjungan yang **sudah selesai atau batal** diberi `LockedUnsigned`. Catatan pada kunjungan yang **masih berjalan** tetap `Draft` agar dapat diselesaikan. Konsekuensi yang diterima: laporan kelengkapan akan menampilkan banyak catatan bertanda tidak ditandatangani sejak hari pertama, dan unit rekam medis perlu diberi penjelasan lebih dulu. | Product/domain owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 2 |
| `RM-DEC-015` | Decision | Jejak akses rekam medis disimpan pada **tabel database khusus**, satu baris per pembukaan berkas. Bukan pada log teks. Alasan: hanya bentuk ini yang dapat ditampilkan pada layar tinjauan yang disyaratkan `RM-DEC-005`, dihubungkan ke data pasien dan pengguna, serta dikendalikan masa simpannya. Konsekuensi yang diterima: tabel tumbuh cepat, sehingga rencana pengarsipan dan pembagian tabel per periode harus disiapkan sejak desain. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 3; menutup `RM-CAP-022` dan `RM-CAP-023` |
| `RM-DEC-016` | Decision | **Pasien rawatan** didefinisikan sebagai pasien yang sedang memiliki **kunjungan aktif** (kunjungan yang belum ditutup). Alasan akses hanya diminta ketika pengguna membuka rekam medis pasien tanpa kunjungan aktif. Definisi ini dipilih karena datanya sudah tersedia dan andal, serta menutup skenario pelanggaran privasi yang paling sering terjadi. Definisi dapat diperketat pada rilis berikutnya setelah data penugasan pengguna ke unit pelayanan tersedia. Menggantikan pertanyaan terbuka `RM-DEC-009`. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 4 |
| `RM-FACT-006` | Fact | Tidak ada data penugasan pengguna ke unit pelayanan. `Models/ApplicationUserOrganization.cs` hanya menyimpan `DepartmentId` dan `PositionId`, dan tidak ada kaitan departemen ke `MstServiceUnit`. Satu-satunya jalur tidak langsung adalah `MstNurseStationClusterStaff.EmployeeId` menuju kluster lalu `ServiceUnitId`, dan itu hanya mencakup perawat kluster serta dikunci pada `EmployeeId`, bukan `UserId` | — | `draft` | — | Source SHA `ab37e3a` |
| `RM-DEC-017` | Decision | `SuperAdmin` **tetap** melewati pemeriksaan kewenangan fungsi seperti sekarang, sehingga pemeliharaan sistem tidak terganggu. Namun untuk membuka rekam medis pasien, `SuperAdmin` tunduk pada aturan yang sama dengan pengguna lain: tercatat pada jejak akses, dan wajib mengisi alasan bila pasien tidak memiliki kunjungan aktif. **Keputusan ini mengubah perilaku authorization di luar modul rekam medis**, sehingga memerlukan persetujuan security/privacy owner sebelum dibangun. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 5; menutup `RM-CAP-025` |
| `RM-DEC-018` | Decision | Tingkat kerahasiaan dokumen **tetap berupa label** pada rilis pertama, tidak menegakkan pembatasan akses. Layar wajib memberi keterangan jujur bahwa label ini adalah penanda dan belum membatasi siapa yang boleh membuka. Penegakan dijadwalkan pada rilis berikutnya bersama kewenangan per dokumen, dan didahului pendataan ulang label yang sudah ada. Keadaan ini wajib dinyatakan terbuka kepada unit rekam medis, tidak boleh didiamkan. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 6; menutup `RM-CAP-026` |
| `RM-DEC-019` | Decision | Penutupan tiga celah keutuhan (`RM-CAP-011`, `RM-CAP-012`, `RM-CAP-013`) menjadi **potongan kerja pertama** modul rekam medis, dikerjakan di area `ClinicalManagement`, **sebelum** halaman penelusuran dibangun. Alasan: halaman rekam medis menyajikan catatan sebagai berkas resmi, sehingga keutuhannya harus dijamin lebih dulu; ketiga celah juga merupakan prasyarat teknis status keutuhan pada `RM-DEC-013`. Konsekuensi yang diterima: slice pertama tidak menghasilkan layar baru, sehingga kemajuannya sulit ditunjukkan ke pihak non-teknis. | Product/domain owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 7 |
| `RM-DEC-020` | Decision | Penulis dianggap **berhalangan** bila akun penggunanya sudah nonaktif, atau bila kepala unit menetapkannya secara manual disertai alasan yang tercatat. Setiap penetapan manual wajib menyimpan siapa yang menetapkan, kapan, dan alasannya, serta ikut menjadi bahan tinjauan unit rekam medis. Menggantikan pertanyaan terbuka `RM-DEC-010`. | Clinical governance owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 8 |
| `RM-DEC-021` | Decision | Tanda tangan elektronik **cukup memakai identitas pengguna yang sedang masuk**, tanpa pengesahan ulang kata sandi atau sidik jari. Yang dicatat: siapa, kapan, dan dari perangkat apa. Syarat penyerta: kebijakan batas waktu sesi dan larangan berbagi akun harus ditegakkan sungguh-sungguh, karena kekuatan bukti bertumpu pada keduanya. Menggantikan pertanyaan terbuka `RM-DEC-011`. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 9 |
| `RM-DEC-022` | Decision | `PrivateNote` **tidak ditampilkan** pada halaman rekam medis sehari-hari, tetapi **dapat dibuka lewat jalur akses beralasan** yang sudah ditetapkan `RM-DEC-005`, disertai alasan dan pencatatan pada jejak akses. Tidak ada bagian rekam medis yang benar-benar tidak dapat dijangkau secara sah. Syarat penyerta: penulis wajib diberi tahu bahwa kolom ini tidak sepenuhnya bersifat pribadi. Menggantikan pertanyaan terbuka `RM-DEC-012`. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 10 |
| `RM-DEC-023` | Decision | Masa simpan jejak akses **ditetapkan berupa angka tetap sebelum desain dimulai**, hasil pemeriksaan pemilik proses terhadap regulasi dan kebijakan rumah sakit yang berlaku. Angka itu dipakai merancang pembagian tabel per periode sejak awal. Ketika kebijakan retensi rekam medis menyeluruh diputuskan pada rilis berikutnya, angka ini diselaraskan. **Angka pastinya belum diisi** dan menjadi prasyarat masuk tahap desain. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Closure Pass pertanyaan 11 |
| `RM-DEC-024` | Decision | Masa simpan jejak akses rekam medis ditetapkan **25 tahun**. Angka ini menjadi dasar rancangan pembagian tabel `TrxMedicalRecordAccessLog` per periode. Menutup blocker desain nomor 2. **Catatan penelusuran:** angka diberikan pemohon sesi; dasar regulasi atau kebijakan rumah sakitnya wajib dilampirkan owner saat pengesahan, karena agent tidak memverifikasi regulasi dari ingatan. | Security/privacy owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Turunan `RM-DEC-023` |
| `RM-DEC-025` | Decision | Tahap desain (`/design-business-module`) dijalankan **sekarang**, di atas keputusan yang masih berstatus `draft`, tanpa menunggu penunjukan owner. Risiko yang diterima secara sadar: bila owner kelak menolak keputusan yang menyentuh wilayah di luar modul — terutama `RM-DEC-017` (kewenangan `SuperAdmin`) dan `RM-DEC-014` (perlakuan catatan lama) — bagian desain yang bergantung padanya harus dirombak. Seluruh artefak desain wajib memuat peringatan bahwa dasarnya belum disetujui. | Pemohon sesi | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Penutupan Closure Pass |
| `RM-DEC-026` | Decision | `BE-16` diperlakukan sebagai **pengaman**, bukan kebutuhan mendesak. Perilakunya tetap seperti rancangan semula: berkas pasien yang ditandai digabung ditolak dengan kode `409` disertai nomor rekam medis pengganti, bukan ditampilkan riwayat sebagiannya. Prioritasnya diturunkan ke akhir milestone B3. **Tafsir yang dipakai:** dari tiga pilihan pada closure question nomor 8, yang dipilih adalah pilihan pertama — menolak membuka, bukan menyatukan saat dibaca maupun memindahkan data klinis. | Product/domain owner | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Penelusuran `RM-CAP-007` 24 Agustus 2026 |
| `RM-FACT-007` | Fact | Penggabungan pasien di sistem ini hanya berupa penandaan. Menyetel `MergedToPatientId` tidak memindahkan data klinis apa pun, dan tidak ada query di modul mana pun yang mengikutinya. `PatientStatus.Merged` tersedia tetapi tidak pernah ditetapkan kode mana pun | — | `draft` | — | Source SHA `ab37e3a`; rincian pada `01-existing-capability-map.md` revision 2 |
| `RM-FACT-008` | Fact | Fitur penggabungan pasien **tidak dapat dipakai dari antarmuka**. Layar mengirim `mergedToPatientId` tanpa `mergeReason`, sementara `PatientController.cs:2380` mewajibkannya, sehingga permintaan selalu ditolak dengan kode 400. Akibat sampingannya: selama celah ini terbuka, tidak ada pasien bernomor rekam medis ganda baru yang tercipta | `PatientManagement` | `draft` | — | Source SHA `ab37e3a` dan `c4e2ef2a6`; dicatat sebagai `RM-CAP-033` dan `GAP-07` |
| `RM-DEC-027` | Decision | **Yoga Aji Pratama** ditetapkan memegang ketiga peran sekaligus: pemilik proses, pemilik tata kelola klinis, dan pemilik keamanan/privasi. Seluruh keputusan `RM-DEC-001` sampai `RM-DEC-026` disahkan atas namanya pada 26 Agustus 2026, sehingga pembangunan backend dapat dimulai. **Batas yang tetap berlaku:** pengesahan ini tidak menggantikan tinjauan komite medik atas `RM-DEC-003`, `RM-DEC-004`, dan `RM-DEC-020`, maupun tinjauan pihak perlindungan data atas `RM-DEC-017`, `RM-DEC-021`, `RM-DEC-022`, dan `RM-DEC-024`, bila kedua pihak itu kelak ditunjuk. Bila tinjauan tersebut menghasilkan keputusan berbeda, bagian desain yang bergantung padanya wajib dirombak. **Risiko ini diterima secara sadar.** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Penetapan kepemilikan modul 26 Agustus 2026 |
| `RM-DEC-028` | Decision | **Yoga Aji Pratama** ditetapkan pula sebagai **pemilik frontend dan pemilik API**, melengkapi ketiga peran pada `RM-DEC-027`. Dengan itu `api_authority` dan `frontend_authority` tidak lagi `OPEN`, dan `contracts/api-contract.md` naik dari `draft` menjadi **`approved`** pada versi `0.1.0`. Gerbang paralel frontend terbuka: sepuluh task `FE-00` sampai `FE-09` tidak lagi `TERTAHAN KONTRAK`. **Yang ikut disahkan** adalah dua delta kontrak yang diterapkan `BE-14` dan tercatat pada api-contract bagian 2: (1) bentuk balasan `/timeline` berubah dari `PagedResult` langsung menjadi selubung `MedicalRecordTimelineResponse` yang memuat halaman beserta `failedSources`, `isTruncated`, dan `isComplete`; (2) field `access` ditambahkan pada seluruh balasan endpoint berkas rekam medis. **Batas yang tetap berlaku:** sama seperti `RM-DEC-027`, pengesahan ini tidak menggantikan tinjauan komite medik maupun pihak perlindungan data bila kedua pihak itu kelak ditunjuk. | Yoga Aji Pratama | `approved` | Yoga Aji Pratama — 27 Agustus 2026 | Penetapan kepemilikan frontend 27 Agustus 2026 |
| `RM-FACT-001` | Fact | Isi rekam medis sudah tersedia sebagai 13 model di `ClinicalManagement` | — | `draft` | — | Source SHA `ab37e3a` |
| `RM-FACT-002` | Fact | Tidak ada penguncian, tanda tangan, maupun addendum pada catatan klinis selain consent | — | `draft` | — | Source SHA `ab37e3a` |
| `RM-FACT-003` | Fact | Tidak ada jejak audit akses baca | — | `draft` | — | Source SHA `ab37e3a` |
| `RM-FACT-004` | Fact | Tidak ada halaman rekam medis berdiri sendiri di frontend | — | `draft` | — | Source SHA `c4e2ef2a6` |
| `RM-FACT-005` | Fact | CPPT sudah punya penyambung `SourceModule` dan `SourceReferenceId` untuk titipan dari modul lain | — | `draft` | — | Source SHA `ab37e3a` |

---

## Open Questions dan Blocker

| No | Pertanyaan | Status |
|---:|---|---|
| 1 | ~~Batas scope modul~~ | **Tertutup** oleh `RM-DEC-001` |
| 2 | ~~Cakupan rilis pertama~~ | **Tertutup** oleh `RM-DEC-002` |
| 3 | Siapa yang ditunjuk sebagai pemilik proses, pemilik tata kelola klinis, dan pemilik keamanan/privasi? | **Blocker**, lihat `RM-DEC-008` |
| 4 | ~~Definisi pasien rawatan~~ | **Tertutup** oleh `RM-DEC-016` |
| 5 | ~~Definisi berhalangan~~ | **Tertutup** oleh `RM-DEC-020` |
| 6 | ~~Bentuk tanda tangan elektronik~~ | **Tertutup** oleh `RM-DEC-021` |
| 7 | ~~Kerahasiaan kolom `PrivateNote`~~ | **Tertutup** oleh `RM-DEC-022` |
| 8 | ~~Angka masa simpan jejak akses~~ | **Tertutup** oleh `RM-DEC-024`: 25 tahun |
| 9 | Sampai kapan entri susulan diterima? | Terbuka, `RM-DEC-007`. Tidak memblokir rilis pertama karena baru relevan setelah modul Laboratorium ada |
| 10 | Alur penggabungan pasien duplikat dan dampaknya ke tampilan riwayat | Terbuka, `RM-CAP-007`. Perlu penelusuran source terarah, bukan keputusan manusia |
| 11 | Apakah perbaikan `RM-CAP-011` sampai `013` memerlukan uji otomatis lebih dulu? | Terbuka. Diteruskan ke `/plan-module-delivery` karena menyangkut penyusunan urutan kerja |
| 12 | Apakah rumah sakit sudah punya SOP rekam medis tertulis yang bisa dijadikan bukti? | Terbuka |

### Blocker

| No | Blocker | Dampak |
|---:|---|---|
| 1 | Tiga owner belum ditunjuk namanya | Seluruh keputusan `RM-DEC-001` sampai `RM-DEC-023` tertahan di status `draft`. Sesuai `RM-DEC-008`, penunjukan ini mendahului tahap berikutnya |
| 2 | ~~Angka masa simpan jejak akses~~ | **Tertutup** 24 Agustus 2026. Ditetapkan 25 tahun pada `RM-DEC-024` |
| 3 | Belum ada dokumen SOP rekam medis rumah sakit | Keputusan saat ini bertumpu pada praktik umum dan rekomendasi, bukan kebijakan setempat yang tertulis |
| 4 | `RM-DEC-017` mengubah perilaku authorization di luar modul rekam medis | Memerlukan persetujuan security/privacy owner yang belum ada, dan berdampak pada seluruh aplikasi termasuk IGD |

**Catatan penting.** Tidak ada satu pun keputusan pada dokumen ini yang berstatus `approved`.
Seluruhnya dijawab pemohon sesi dan menunggu pengesahan owner berwenang. Dokumen ini belum
boleh dipakai sebagai dasar implementasi.

---

## Riwayat Pass

| Revision | Tanggal | Mode | Ringkasan |
|---:|---|---|---|
| 1 | 24 Agustus 2026 | Scope pass | Pass selesai. Tujuh pertanyaan dijawab menghasilkan `RM-DEC-001` sampai `RM-DEC-008`. Batas scope terkunci, cakupan rilis pertama ditetapkan, tiga invariant inti dirumuskan (penguncian, addendum, batas akses), satu exception path ditutup (data susulan). Lima pertanyaan turunan terbuka dan dua blocker tercatat. |
| 2 | 24 Agustus 2026 | Closure pass | Pass selesai setelah membaca `01-existing-capability-map.md`. Sebelas pertanyaan dijawab menghasilkan `RM-DEC-013` sampai `RM-DEC-023`. **Tiga `Conflict` tertutup**: model status (`RM-CAP-009`), kewenangan `SuperAdmin` (`RM-CAP-025`), tingkat kerahasiaan (`RM-CAP-026`). **Dua dari tiga `Unknown` tertutup**: bentuk tanda tangan (`RM-CAP-015`) dan `PrivateNote` (`RM-CAP-027`). Empat pertanyaan turunan Scope Pass tertutup. Acceptance criteria bertambah dari 9 menjadi 17. Tersisa satu `Unknown` (`RM-CAP-007`, penggabungan pasien duplikat) yang memerlukan penelusuran source, bukan keputusan, serta empat blocker. |
| 3 | 24 Agustus 2026 | Amendment pass | Penelusuran terarah `RM-CAP-007` selesai. Penggabungan pasien ternyata hanya penandaan, tidak memindahkan data klinis; statusnya naik menjadi `Conflict`. Ditemukan pula `RM-CAP-033`: fitur penggabungan tidak dapat dipakai dari antarmuka karena `mergeReason` tidak pernah dikirim. `RM-DEC-026` menetapkan `BE-16` sebagai pengaman berprioritas rendah dengan perilaku `409`. Dua fakta baru dicatat sebagai `RM-FACT-007` dan `RM-FACT-008`. |
| 4 | 24 Agustus 2026 | Amendment pass | `RM-DEC-024` ditutup: masa simpan jejak akses **25 tahun**. Tabel `TrxMedicalRecordAccessLog` dirancang terbagi per tahun berdasarkan `AccessedAt`, 25 bagian pada keadaan penuh. `BE-10` naik dari `TERTAHAN BLOCKER` menjadi `TERTAHAN APPROVAL`, sehingga **tidak ada lagi task berstatus `TERTAHAN BLOCKER`**. Risiko operasional baru dicatat: bagian tahun yang lupa dibuat akan menghentikan pembacaan rekam medis. |
| 5 | 26 Agustus 2026 | Amendment pass | **Kepemilikan modul ditetapkan.** Yoga Aji Pratama memegang ketiga peran owner sekaligus, dicatat pada `RM-DEC-027`. Dua puluh dua keputusan naik status dari `draft` menjadi `approved`. Empat pertanyaan terbuka yang sudah digantikan ditandai `superseded`. Seluruh gerbang pengesahan untuk pembangunan backend terbuka. Batas yang tetap berlaku: tinjauan komite medik dan pihak perlindungan data belum dilakukan. |
