# Rekam Medis — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Revision | `10` |
| Status | **`approved`** — 22 keputusan disahkan 26 Agustus 2026; 7 keputusan sambungan Laboratorium disahkan 10 September 2026; **14 keputusan amandemen sambungan seluruh modul disahkan 28 Oktober 2026** (`RM-DEC-037` sampai `RM-DEC-050`); **9 keputusan closure pass disahkan 28 Oktober 2026** (`RM-DEC-051` sampai `RM-DEC-059`); **2 keputusan penutup `DEC-RM-001` disahkan 28 Oktober 2026** (`RM-DEC-060`, `RM-DEC-061`); **pengesahan blueprint revision 4 dan kontrak `0.2.0`** (`RM-DEC-062`, `RM-DEC-063`) |
| Interview mode | `Amendment pass` (revision 3, 4, 5, 6, dan 7); `Closure pass` (revision 2, **8**, dan **9**); `Scope pass` (revision 1) |
| Input capability map | `01-existing-capability-map.md` revision `2` — **basi** sejak SHA berubah. Digantikan capability map sambungan Laboratorium ↔ Rekam Medis, audit 10 September 2026. **Revision 7 dijalankan tanpa capability audit baru** — hanya pembacaan source terarah pada SHA `363de34f`/`f0a3b07b8`, dicatat sebagai `RM-FACT-013` sampai `RM-FACT-018`. Audit lengkap per modul wajib dijalankan sebelum desain (lihat "Langkah berikutnya" pada amandemen 28 Oktober 2026). **Revision 8 bertumpu pada `01-existing-capability-map.md` revision 3** (impact scan 28 Oktober 2026, `sha256:b840e22b…`, `RM-CAP-034`–`070`), SHA sama |
| Product/domain owner | **Yoga Aji Pratama** — ditetapkan 26 Agustus 2026 |
| Clinical governance owner | **Yoga Aji Pratama** — ditetapkan 26 Agustus 2026 |
| Security/privacy owner | **Yoga Aji Pratama** — ditetapkan 26 Agustus 2026 |
| Catatan kepemilikan | Ketiga peran dipegang **satu orang**. Konsekuensi dan batasnya dicatat pada `RM-DEC-027` |
| Backend SHA | `ab37e3a` (revision 1–5); `0e2eb105` (revision 6); **`363de34f`** (revision 7, branch `yoga`) |
| Frontend SHA | `c4e2ef2a6` (revision 1–5); `0e5d3c36` (revision 6); **`f0a3b07b8`** (revision 7, branch `YogaV2`) |
| Tanggal pass | 24 Agustus 2026; pengesahan 26 Agustus 2026; amandemen sambungan Laboratorium 10 September 2026; **amandemen sambungan seluruh modul 28 Oktober 2026** |
| Primary evidence | Pembacaan source backend dan frontend pada SHA di atas; belum ada dokumen SOP rekam medis yang diserahkan |

> **Status pengesahan.** Dua puluh dua keputusan pada dokumen ini berstatus `approved`,
> disahkan Yoga Aji Pratama selaku penanggung jawab modul pada 26 Agustus 2026. Empat
> pertanyaan terbuka berstatus `superseded` karena sudah digantikan keputusan lain.
>
> **Pembaruan 10 September 2026 (revision 6).** Tujuh keputusan sambungan Laboratorium
> ditambahkan, `RM-DEC-030` sampai `RM-DEC-036`. Dengan `RM-DEC-035`, **tidak ada lagi
> pertanyaan pada dokumen ini yang berstatus terbuka** — `RM-DEC-007` yang selama ini
> menggantung akhirnya tertutup, karena pemicunya sudah tiba: modul Laboratorium kini ada.
>
> **Pembaruan 28 Oktober 2026 (revision 7).** Atas permintaan Yoga Aji Pratama — "lengkapi
> modul rekam medis terhadap semua modul yang sudah tersedia saat ini" — empat belas keputusan
> baru disahkan, `RM-DEC-037` sampai `RM-DEC-050`. Isinya: aturan sambungan Laboratorium
> (`RM-DEC-030` sampai `RM-DEC-035`) **diangkat menjadi aturan seluruh modul**, dan cakupan
> nomor 4 **kelengkapan berkas** ditarik masuk. Seluruhnya dijawab langsung oleh pemilik modul
> lewat sembilan belas pertanyaan pilihan. Delapan belas jawaban mengikuti opsi yang
> direkomendasikan; satu (memasukkan MCU) dipilih pemilik di luar rekomendasi.
>
> **Pembaruan 28 Oktober 2026 (revision 8, closure pass).** Delapan closure question dari
> capability map revision 3 dijawab, ditambah satu pertanyaan yang muncul dari pemeriksaan
> source di tengah wawancara (laporan Patologi Anatomi dapat dibuka kembali sesudah rilis).
> Hasilnya `RM-DEC-051` sampai `RM-DEC-059`, seluruhnya mengikuti opsi yang direkomendasikan.
> Tiga conflict capability map tertutup; tiga sumber modul lain memakai aturan sementara yang
> wajib dikonfirmasi pemiliknya (`RM-DEC-059`).
>
> **Pembaruan 28 Oktober 2026 (revision 9, closure pass lanjutan).** Menutup `DEC-RM-001`
> yang ditemukan gerbang kelengkapan requirement (`02-requirement-completeness-assessment.md`):
> kunjungan penunjang langsung tercatat Rawat Jalan sehingga daftar wajib Rawat Jalan mustahil
> dipenuhi. Dua keputusan, `RM-DEC-060` dan `RM-DEC-061`, keduanya mengikuti rekomendasi. Rinciannya pada bagian "Amandemen cakupan 28 Oktober 2026".
>
> **Batas yang tetap berlaku.** Pengesahan ini sah untuk memulai pembangunan. Ia **tidak**
> menggantikan tinjauan komite medik maupun pihak perlindungan data bila kelak keduanya
> ditunjuk — lihat `RM-DEC-027`. Batas yang sama melekat pada `RM-DEC-030`, `RM-DEC-032`,
> dan `RM-DEC-033` yang menyentuh keselamatan klinis dan kerahasiaan.

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

Ditambahkan 10 September 2026, hasil amandemen sambungan Laboratorium:

- **Entri hasil laboratorium.** Input nilai, verifikasi, dan rilis hasil adalah pekerjaan
  modul Laboratorium, bukan modul ini. `RM-DEC-036` menetapkannya sebagai prasyarat, bukan
  sebagai cakupan rekam medis.
- **Mengangkat pola "baca langsung, dilarang menyalin" menjadi aturan seluruh modul
  penunjang.** Scope amandemen ini sengaja dikunci hanya pada Laboratorium. `RM-DEC-030`
  menjadi preseden kedua setelah `RAD-DEC-006`, sehingga aturan lintas modul dapat disusun
  belakangan dengan dua contoh yang sudah sejalan.
- **Tujuh tabel `ClinicalManagement` yang belum terbaca layar penelusuran.**
  `CliNursingCarePlan`, `CliNursingCarePlanItem`, `CliNursingCarePlanItemRevision`,
  `CliNursingIntervention`, `CliPhysicianVisit`, `TrxNosocomialInfection`, dan
  `CliClinicalMilestoneFact`. Ini utang modul rekam medis sendiri, bukan soal laboratorium.
- **Sumber klinis modul lain yang juga belum terbaca.** Resume medis rawat inap, laporan
  operasi dan anestesi, rekam IGD, resep, serta asuhan gizi. Masing-masing proyek tersendiri.
- **Nilai kritis dan eskalasinya.** Alur kerja internal laboratorium.

### Amandemen cakupan 28 Oktober 2026 (revision 7)

**Kalimat batas scope tidak berubah** (`RM-DEC-001`): modul ini tetap tidak menulis isi
klinis. Yang berubah adalah **jumlah sumber yang dibaca** dan **satu kemampuan yang ditarik
masuk**, yaitu kelengkapan berkas.

Kembali ke perumpamaan map kertas: pada rilis pertama, map pasien baru berisi lembar dari
poliklinik — CPPT, asesmen, diagnosis, dan sejenisnya. Sejak itu rumah sakit sudah punya
laboratorium, radiologi, IGD, rawat inap, kamar operasi, hemodialisa, gizi, bank darah,
farmasi, dan MCU di sistem, tetapi lembar-lembar dari unit tersebut **belum masuk map**.
Amandemen ini memasukkan semuanya, lalu menambah satu petugas yang memeriksa apakah map
setiap kunjungan sudah lengkap.

Tiga butir pada daftar "Di luar scope — untuk modul lain" di atas **ditarik masuk** oleh
amandemen ini. Butirnya tidak dihapus supaya riwayatnya terbaca:

| Butir lama (10 September 2026) | Keadaan sejak 28 Oktober 2026 |
|---|---|
| Mengangkat pola "baca langsung, dilarang menyalin" menjadi aturan seluruh modul penunjang | **Masuk scope**, `RM-DEC-038`. Berlaku bukan hanya untuk penunjang, tetapi seluruh modul |
| Tujuh tabel `ClinicalManagement` yang belum terbaca layar penelusuran | **Masuk scope**, `RM-DEC-039` kelompok C |
| Sumber klinis modul lain yang juga belum terbaca | **Masuk scope**, `RM-DEC-039` kelompok A sampai D |

Butir "Modul Rawat Inap belum ada area-nya di backend" juga **sudah tidak benar**: area
`InPatientManagement` kini ada dengan 28 model (`RM-FACT-013`).

#### Di dalam scope amandemen

| No | Cakupan | Keputusan |
|---:|---|---|
| A1 | Membaca dokumen klinis **final** dari seluruh modul yang sudah ada ke dalam berkas pasien | `RM-DEC-037`, `RM-DEC-038`, `RM-DEC-039`, `RM-DEC-041` |
| A2 | Penanda "pernah diralat" untuk dokumen yang ralatnya diatur modul pemilik | `RM-DEC-042` |
| A3 | Penanda `Susulan` untuk seluruh jenis dokumen — utang `RM-DEC-006` yang belum pernah dibangun | `RM-DEC-043` |
| A4 | Pesan gangguan per bagian ketika salah satu modul tidak dapat dibaca | `RM-DEC-044` |
| A5 | **Kelengkapan berkas** (cakupan nomor 4): penilaian per kunjungan, daftar kerja petugas rekam medis, master dokumen wajib, dan penuntasan catatan yang tidak ditandatangani | `RM-DEC-045` sampai `RM-DEC-047`, `RM-DEC-049`, `RM-DEC-050` |
| A6 | Tampilan sambungan di layar penelusuran yang sudah ada dan entri menu daftar kerja kelengkapan | `RM-DEC-048` |

#### Di luar scope amandemen

| No | Yang tidak dikerjakan | Alasan atau pemilik |
|---:|---|---|
| B1 | Koding ICD, resume medis rekam medis, pelepasan informasi, retensi dan pemusnahan (cakupan 5 sampai 8) | Tetap rilis berikutnya (`RM-DEC-037`). Butuh SOP dan dasar regulasi yang belum diserahkan |
| B2 | Membuat atau mengubah isi dokumen modul lain | Modul pemilik masing-masing (`RM-DEC-001`) |
| B3 | Aturan ralat dan penguncian dokumen modul lain | Modul pemilik masing-masing (`RM-DEC-042`) |
| B4 | Penentuan kapan dokumen sebuah modul dianggap **final** | Modul pemilik; dibaca dari kontrak integrasinya (`RM-DEC-041`) |
| B5 | Menahan penutupan kunjungan, pemulangan pasien, atau tagihan karena berkas belum lengkap | Ditolak `RM-DEC-046` |
| B6 | Penyerahan hasil MCU ke perusahaan | Modul MCU, lewat `McuReleaseConsent` (`RM-DEC-040`) |
| B7 | Integrasi SATUSEHAT/FHIR | Tetap di luar, alasan sama seperti sebelumnya |

#### Sumber yang dibaca

Dikelompokkan sesuai urutan pengerjaan yang dipilih pada `RM-DEC-048`. Nama model di kolom
ketiga adalah **titik awal penelusuran**, bukan keputusan akhir tabel mana yang dibaca;
penentuannya milik `/trace-existing-capabilities` dan kontrak integrasi tiap modul.

| Kelompok | Modul | Dokumen yang kemungkinan menjadi sumber |
|---|---|---|
| A — Penunjang | Laboratorium | Hasil pemeriksaan yang dirilis (`LabExamination`, `LabPathologyReport`) |
| A — Penunjang | Radiologi | Laporan bacaan final (`RadReport`, `RadReportVersion`) |
| B — Pelayanan | IGD | Triase, observasi, resusitasi, disposisi |
| B — Pelayanan | Rawat Inap | Ringkasan pulang, dokumen admisi |
| B — Pelayanan | Kamar Operasi | Laporan operasi, catatan anestesi, checklist keselamatan, catatan kelahiran |
| B — Pelayanan | Hemodialisa | Catatan sesi (sudah terdaftar di mesin keutuhan sebagai `HemodialysisSession`) |
| B — Pelayanan | Gizi | Catatan asuhan gizi |
| B — Pelayanan | Bank Darah | Pemeriksaan golongan darah, pemberitahuan reaksi transfusi |
| C — Farmasi dan klinis baru | Farmasi | Resep, pemberian obat, rekonsiliasi obat |
| C — Farmasi dan klinis baru | `ClinicalManagement` | Tujuh tabel `RM-FACT-012` ditambah model yang lahir sesudahnya: rencana asuhan keperawatan, visite dokter, observasi harian, serah terima, monitoring transfusi, surat dokter, dan lainnya |
| D — MCU | MCU | Hasil pemeriksaan final dan resume MCU |

Bank Darah dimasukkan ke kelompok pelayanan sesuai jawaban pertanyaan urutan; pada
pertanyaan pemilihan modul ia termasuk penunjang. Perbedaan ini tidak mengubah apa pun selain
urutan, dan boleh disesuaikan saat perencanaan.

#### Daftar dokumen wajib awal (disahkan `RM-DEC-047`)

Jenis kunjungan diambil dari `EncounterType` yang ada di source (`RM-FACT-017`). Operasi,
hemodialisa, dan transfusi **bukan** jenis kunjungan — ketiganya kejadian di dalam sebuah
kunjungan — sehingga dijadikan **syarat bersyarat**: hanya berlaku bila kejadiannya ada.

| Berlaku untuk | Dokumen wajib |
|---|---|
| Rawat Jalan (`Outpatient`) | Asesmen; diagnosis utama; CPPT/SOAP dokter |
| Telemedicine (`Telemedicine`) | CPPT/SOAP dokter; diagnosis utama |
| IGD (`Emergency`) | Triase; asesmen; diagnosis utama; disposisi |
| Rawat Inap (`Inpatient`) | Persetujuan umum; asesmen awal; diagnosis utama; CPPT; ringkasan pulang |
| MCU (`MedicalCheckup`) | Resume MCU |
| Bila kunjungan memuat operasi | Laporan operasi; catatan anestesi; checklist keselamatan |
| Bila kunjungan memuat sesi hemodialisa | Catatan sesi yang sudah ditandatangani |
| Bila kunjungan memuat transfusi | Monitoring transfusi |

Daftar ini **isi awal** master, bukan aturan yang ditulis tetap di program (`RM-DEC-045`).
Unit rekam medis boleh menambah, mengurangi, atau mengubahnya lewat layar master kelak.

**Diperjelas closure pass revision 8** — dokumen apa tepatnya yang memenuhi tiap butir:

| Butir | Terpenuhi oleh | Keputusan |
|---|---|---|
| Asesmen (Rawat Jalan, IGD) | Salah satu asesmen awal keperawatan atau medis yang selesai | `RM-DEC-053` |
| Asesmen awal (Rawat Inap) | Asesmen awal keperawatan **dan** medis, keduanya selesai | `RM-DEC-053` |
| Diagnosis utama | Dicentang utama **atau** berjenis Primary; bukan dibatalkan/disingkirkan | `RM-DEC-054` |
| CPPT/SOAP dokter | CPPT berprofesi dokter **atau** konsultasi dokter | `RM-DEC-055` |
| Persetujuan umum (Rawat Inap) | Dokumen admisi General Consent yang selesai — **bukan** persetujuan registrasi | `RM-DEC-056` |
| Disposisi (IGD) | Disposisi yang sudah dikonfirmasi dokter (sementara) | `RM-DEC-059` |

**Diperluas closure pass revision 9** — Rawat Jalan dibedakan menurut **jenis unit layanan**
kunjungannya (`RM-DEC-060`). Baris "Rawat Jalan" pada tabel daftar awal di atas kini hanya
berlaku untuk unit selain Laboratorium, Radiologi, dan Hemodialisa:

| Berlaku untuk | Dokumen wajib | Keputusan |
|---|---|---|
| Rawat Jalan di unit **Laboratorium** atau **Radiologi** | Tidak ada dokumen dokter. Lengkap bila seluruh pemeriksaan yang dipesan sudah dirilis (syarat 2) dan syarat 1 terpenuhi | `RM-DEC-060` |
| Rawat Jalan di unit **Hemodialisa** | Catatan sesi yang sudah ditandatangani | `RM-DEC-060` |
| Rawat Jalan di unit lain, termasuk `Other` dan `Unknown` | Daftar Rawat Jalan biasa: asesmen, diagnosis utama, CPPT/SOAP dokter | `RM-DEC-047`, `053`–`055` |
| Kunjungan rujukan (`IsReferral`) | Surat rujukan **tidak** wajib | `RM-DEC-061` |

#### Alur kerja kelengkapan berkas

Contoh: Ibu Sari dirawat inap 3 hari, menjalani operasi usus buntu, dan sampel jaringannya
dikirim ke patologi anatomi. Kunjungan ditutup Jumat siang.

1. **Kunjungan ditutup.** Catatan yang belum ditandatangani terkunci otomatis bertanda
   `LockedUnsigned` (`RM-DEC-003`). Kunjungan kini masuk penilaian kelengkapan (`RM-DEC-046`).
2. **Sistem menilai tiga syarat** (`RM-DEC-045`):
   - Syarat 1 — semua dokumen yang tunduk keutuhan sudah ditandatangani? Satu CPPT perawat
     `LockedUnsigned` → **gagal**.
   - Syarat 2 — tidak ada hasil penunjang yang ditunggu? Patologi anatomi belum keluar →
     **gagal**.
   - Syarat 3 — dokumen wajib Rawat Inap dan syarat bersyarat Operasi ada? Ringkasan pulang
     ada, laporan operasi ada, **catatan anestesi belum ada** → **gagal**.
3. **Daftar kerja** menampilkan **satu keadaan utama** — `Menunggu hasil` — dengan tiga rincian di
   bawahnya: CPPT tidak ditandatangani, hasil patologi anatomi belum keluar, catatan anestesi
   belum ada (`RM-DEC-046`).
4. **Petugas rekam medis menindaklanjuti** di luar sistem: menghubungi perawat dan dokter
   anestesi. Tidak ada yang ditahan — pasien sudah pulang, tagihan tetap berjalan. CPPT
   perawat yang terkunci tidak bisa ditandatangani lagi, jadi perawat membuat **addendum
   pengesahan** yang menyatakan isinya benar; begitu addendum itu ditandatangani, rincian
   "tidak ditandatangani" tuntas, sementara status catatannya tetap `LockedUnsigned` sebagai
   jejak jujur (`RM-DEC-049`).
5. **Dua minggu kemudian hasil patologi anatomi dirilis.** Hasil tampil di berkas dengan
   penanda `Susulan` karena waktu rilisnya sesudah kunjungan ditutup (`RM-DEC-043`). Kunjungan
   tidak dibuka kembali (`RM-DEC-006`).
6. Bila saat itu modul Kamar Operasi sedang gangguan, keadaannya menjadi
   `Tidak dapat dipastikan` — **tidak pernah** `Lengkap` (`RM-DEC-044`).

#### Langkah berikutnya

Keputusan bisnis amandemen ini sudah lengkap. Yang belum ada adalah **bukti source per
modul**: arti final, kolom waktu final, tabel sumber untuk setiap butir dokumen wajib, dan
jalur baca berlingkup pasien yang sudah tersedia (open question 13 sampai 15, blocker 10).
Ketiganya pertanyaan source, bukan keputusan manusia, sehingga langkah berikutnya adalah
`/trace-existing-capabilities` mode impact scan atas dua belas area pada `RM-FACT-013`,
disusul `/requirement-completeness-gate`, lalu desain.

Dua pekerjaan **tidak perlu menunggu** audit itu, karena keduanya utang modul ini sendiri dan
aturannya sudah lengkap: blocker 11 (jenis 14 dan 15 kosong diam-diam) dan implementasi
penanda `Susulan` untuk 13 jenis lama (`RM-DEC-043`). Keduanya tetap harus masuk roadmap lebih
dulu sebelum dikerjakan.

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
| `RM-FE-017` | Bagian hasil laboratorium yang gagal dimuat wajib terbaca sebagai gangguan, dan wajib **dapat dibedakan** dari keadaan "pasien tidak punya pemeriksaan" | Clinical governance owner | `draft` | Wajib dibedakan; susunan kalimat bebas | `RM-DEC-030` |
| `RM-FE-018` | Penanda bahwa sebuah hasil laboratorium **pernah diralat** wajib terlihat pada berkas rekam medis | Clinical governance owner | `draft` | Wajib terlihat; bentuk visual bebas | `RM-DEC-032` |
| `RM-FE-019` | Bentuk tampilan hasil laboratorium pada berkas: tab tersendiri, panel, atau menyatu di daftar riwayat | Frontend | `DEV_DISCRETION` | Mengikuti konvensi proyek yang sudah ada | `RM-DEC-030`. **Dipersempit 28 Oktober 2026** oleh `RM-FE-024`: hasil laboratorium menyatu di linimasa penelusuran yang sudah ada |
| `RM-FE-020` | Bagian modul **mana pun** yang gagal dimuat wajib terbaca sebagai gangguan dan dapat dibedakan dari "tidak ada dokumen"; bagian modul lain tetap tampil | Clinical governance owner | `approved` 28 Oktober 2026 | Wajib per bagian; susunan kalimat bebas | `RM-DEC-044`; memperluas `RM-FE-017` |
| `RM-FE-021` | Penanda "pernah diralat" wajib terlihat untuk dokumen modul mana pun yang ralatnya diatur modul pemilik | Clinical governance owner | `approved` 28 Oktober 2026 | Wajib terlihat; bentuk visual bebas | `RM-DEC-042`; memperluas `RM-FE-018` |
| `RM-FE-022` | Penanda `Susulan` wajib terlihat pada dokumen jenis apa pun yang final sesudah kunjungan ditutup | Clinical governance owner | `approved` 28 Oktober 2026 | Wajib terlihat; bentuk visual bebas | `RM-DEC-043`; mewujudkan bagian `Susulan` pada `RM-FE-001` |
| `RM-FE-023` | Daftar kerja kelengkapan wajib menampilkan satu keadaan utama per kunjungan **dan** seluruh rincian kekurangannya; tidak boleh ada kekurangan yang tersembunyi di balik keadaan utama | Product/domain owner | `approved` 28 Oktober 2026 | Wajib; bentuk rincian bebas | `RM-DEC-046` |
| `RM-FE-024` | Dokumen modul lain tampil di **linimasa penelusuran yang sudah ada** sebagai jenis dokumen baru dengan saringan jenis; daftar kerja kelengkapan menjadi **entri menu baru di grup Rekam Medis** | Product/domain owner; frontend authority | `approved` 28 Oktober 2026 | Penempatan terkunci; tata letak, kolom, warna, ikon = `DEV_DISCRETION` dengan brief UI yang disahkan pemilik | `RM-DEC-048` |

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
| 18 | Nol tabel di `MedicalRecordManagement` menyimpan nilai hasil laboratorium — dibuktikan dengan menelusuri seluruh model modul (`RM-DEC-030`) |
| 19 | Nol kolom baru ditambahkan pada tabel `LaboratoryManagement` akibat sambungan ini (`RM-DEC-031`) |
| 20 | Ketika modul Laboratorium tidak dapat dihubungi, layar menampilkan pesan gangguan. Uji memalsukan kegagalan panggilan lalu memeriksa bahwa yang tampil **bukan** daftar kosong (`RM-DEC-030`, `RM-FE-017`) |
| 21 | Membuka berkas pasien yang memiliki tiga hasil laboratorium menghasilkan **tepat satu** baris `MrcAccessLog`, bukan empat (`RM-DEC-034`) |
| 22 | Pengguna dengan izin `MedicalRecord : Read` **tanpa** izin apa pun pada modul Laboratorium tetap melihat hasil laboratorium pada berkas (`RM-DEC-033`) |
| 23 | Hasil yang dirilis setelah kunjungan ditutup tetap tertaut ke kunjungan tersebut, dan status kunjungan itu tidak berubah (`RM-DEC-035`, turunan `RM-DEC-006`) |
| 24 | Berkas yang masih memiliki pemeriksaan tanpa hasil tidak dapat dinyatakan lengkap, dan tampil sebagai "menunggu hasil" (`RM-DEC-035`) |

Ditambahkan 28 Oktober 2026 (revision 7). **Kriteria nomor 8 diubah caranya** oleh
`RM-DEC-043`: penanda `Susulan` tidak lagi diuji sebagai nilai yang tersimpan, melainkan
diturunkan dari waktu final dokumen dan waktu penutupan kunjungan — lihat kriteria nomor 28.
Bunyi lamanya dibiarkan supaya riwayatnya terbaca.

| No | Kriteria |
|---:|---|
| 25 | Untuk setiap modul pada `RM-DEC-039`, berkas pasien yang memiliki satu dokumen final di modul itu menampilkan dokumen tersebut pada linimasa, dan **nol** baris salinannya tersimpan di tabel `MedicalRecordManagement` (`RM-DEC-038`) |
| 26 | Dokumen modul lain yang belum final menurut modul pemiliknya **tidak** tampil di linimasa (`RM-DEC-041`) |
| 27 | Pengguna dengan `MedicalRecord : Read` tanpa izin apa pun di Radiologi, IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah, Farmasi, dan MCU tetap melihat dokumen final modul-modul itu; satu pembukaan berkas menghasilkan **tepat satu** baris `MrcAccessLog` (`RM-DEC-040`) |
| 28 | Dokumen dengan waktu final sesudah waktu penutupan kunjungannya bertanda `Susulan`; dokumen yang final sebelum atau tepat saat penutupan tidak bertanda. Berlaku juga untuk 13 jenis lama (`RM-DEC-043`) |
| 29 | Dokumen yang pernah diralat di modul pemiliknya bertanda "pernah diralat" di berkas; rekam medis tidak menyediakan jalur ralat untuk dokumen itu (`RM-DEC-042`) |
| 30 | Bila panggilan ke satu modul dipalsukan gagal, berkas tetap terbuka, bagian modul itu menampilkan pesan gangguan, bagian lain tampil normal, dan kunjungan terdampak berkeadaan `Tidak dapat dipastikan` (`RM-DEC-044`) |
| 31 | Kunjungan Rawat Inap yang ditutup tanpa ringkasan pulang berkeadaan `Belum lengkap` dengan rincian "ringkasan pulang belum ada" (`RM-DEC-045`, `RM-DEC-047`) |
| 32 | Kunjungan Rawat Jalan **tanpa** operasi tidak dituntut laporan operasi; kunjungan yang **memuat** operasi tanpa catatan anestesi berkeadaan `Belum lengkap` (syarat bersyarat, `RM-DEC-047`) |
| 33 | Mengubah isi master dokumen wajib lewat layar master mengubah hasil penilaian tanpa rilis program (`RM-DEC-045`) |
| 34 | Kunjungan yang belum lengkap tetap dapat ditutup, dan pasiennya tetap dapat dipulangkan (`RM-DEC-046`) |
| 35 | Kunjungan yang masih berjalan dan kunjungan batal **tidak** muncul di daftar kerja kelengkapan (`RM-DEC-046`) |
| 36 | Kunjungan dengan kekurangan "ringkasan pulang belum ada" dan "kultur belum keluar" berkeadaan `Menunggu hasil` dan menampilkan kedua rincian (`RM-DEC-046`, `RM-FE-023`) |
| 37 | Setelah addendum pengesahan atas catatan `LockedUnsigned` ditandatangani, rincian "tidak ditandatangani" untuk catatan itu hilang dari penilaian, sedangkan status catatannya tetap `LockedUnsigned` (`RM-DEC-049`) |
| 38 | Kunjungan yang ditutup sebelum tanggal rilis fitur tidak muncul di daftar kerja, tetapi penilaiannya tetap dapat diminta untuk satu kunjungan (`RM-DEC-050`) |
| 39 | Jenis `HemodialysisSession` dan `LaboratoryResult` pada linimasa tidak pernah tampil sebagai daftar kosong diam-diam; bila sumbernya belum tersambung, layar menyatakan gangguan atau belum tersedia (`RM-FACT-014`, `RM-DEC-044`) |

Ditambahkan 28 Oktober 2026 (revision 8, closure pass).

| No | Kriteria |
|---:|---|
| 40 | Episode rawat inap ditutup Senin, tagihan final Kamis, hasil dirilis Selasa → hasil bertanda `Susulan`. Kunjungan masuk daftar kerja sejak Senin, bukan Kamis (`RM-DEC-051`) |
| 41 | Rekonsiliasi IGD yang mengubah `CompletedAt` tidak mengubah penanda `Susulan` dokumen mana pun pada kunjungan itu (`RM-DEC-051`) |
| 42 | Enam pengukuran gula darah pada satu hari tampil sebagai **satu** baris linimasa dengan jumlah 6, dan rinciannya menampilkan keenam pengukuran (`RM-DEC-052`) |
| 43 | Kunjungan Rawat Inap dengan asesmen awal keperawatan selesai tetapi tanpa asesmen awal medis berkeadaan `Belum lengkap`; kunjungan Rawat Jalan dengan salah satunya saja tidak dituntut yang lain (`RM-DEC-053`) |
| 44 | Asesmen ulang atau asesmen risiko jatuh tidak memenuhi syarat asesmen awal (`RM-DEC-053`) |
| 45 | Diagnosis berjenis `Primary` tanpa centang `IsPrimary` memenuhi syarat diagnosis utama; diagnosis utama berstatus `RuledOut` tidak memenuhi (`RM-DEC-054`) |
| 46 | Kunjungan Rawat Jalan yang hanya punya konsultasi dokter, tanpa CPPT, memenuhi syarat CPPT/SOAP dokter (`RM-DEC-055`) |
| 47 | Kunjungan Rawat Inap yang hanya punya persetujuan umum registrasi berkeadaan `Belum lengkap` dengan rincian "persetujuan umum rawat inap belum ada" (`RM-DEC-056`) |
| 48 | Hasil Patologi Klinik dan Mikrobiologi yang dirilis tampil di linimasa; hasil yang belum dirilis tidak tampil (`RM-DEC-057`, `RM-DEC-041`) |
| 49 | Bagian hasil Patologi Anatomi menampilkan keterangan "belum tersedia di berkas", dapat dibedakan dari "pasien tidak punya pemeriksaan PA" (`RM-DEC-058`) |
| 50 | Disposisi IGD berstatus `Draft` tidak tampil dan tidak memenuhi syarat; berstatus `Confirmed` tampil dan memenuhi (`RM-DEC-059`) |
| 51 | Catatan asuhan gizi tampil dengan label "dapat berubah" (`RM-DEC-059`) |

Ditambahkan 28 Oktober 2026 (revision 9).

| No | Kriteria |
|---:|---|
| 52 | Kunjungan Rawat Jalan di unit berjenis `Laboratory` tanpa asesmen, diagnosis, dan SOAP dokter berkeadaan `Lengkap` begitu seluruh pemeriksaannya dirilis; sebelum itu `Menunggu hasil` (`RM-DEC-060`) |
| 53 | Kunjungan Rawat Jalan di unit berjenis `Hemodialysis` dengan catatan sesi bertanda tangan berkeadaan `Lengkap` tanpa dituntut SOAP dokter (`RM-DEC-060`) |
| 54 | Kunjungan Rawat Jalan di poliklinik (unit berjenis `Outpatient`) tetap dituntut asesmen, diagnosis utama, dan CPPT/SOAP dokter (`RM-DEC-060`, `RM-DEC-047`) |
| 55 | Kunjungan rujukan tanpa dokumen surat rujukan tidak berkeadaan `Belum lengkap` karena alasan itu (`RM-DEC-061`) |

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
| `RM-DEC-007` | Open Question | Sampai kapan entri susulan masih diterima untuk sebuah kunjungan yang sudah ditutup, dan bagaimana pemeriksaan kelengkapan memperlakukan berkas yang masih menunggu hasil? Turunan langsung dari `RM-DEC-006`. | Product/domain owner | `superseded` | Digantikan `RM-DEC-035`, disahkan Yoga Aji Pratama 10 September 2026 | Wawancara Scope Pass pertanyaan 6 |
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
| `RM-FACT-009` | Fact | Modul Laboratorium **belum menyimpan hasil pemeriksaan sama sekali**. `LabExaminationStatus` hanya mengenal `Ordered`, `ChargeEligible`, `Voided`, dan `Cancelled`; tidak ada entity maupun kolom nilai hasil, tidak ada tahap verifikasi, dan tidak ada penerbitan hasil. `LabValueBound` dan `LabValueOption` adalah master nilai rujukan dan nilai kritis, **bukan** hasil pasien | `LaboratoryManagement` | `draft` | — | `LaboratoryEnums.cs:114-130`; source SHA `0e2eb105` |
| `RM-FACT-010` | Fact | **Nol `PatientId` pada seluruh model Laboratorium.** `LabOrder` hanya menyimpan `EncounterId`, dan `LabExamination` hanya `LabOrderId` serta `SpecimenId`. Identitas pasien hanya dapat dicapai lewat `TrxPatientEncounter`. Helper generik penarik sumber pada layar penelusuran menuntut entity menyediakan `PatientId` langsung, sehingga entity Laboratorium tidak dapat dipakai apa adanya | `LaboratoryManagement` | `draft` | — | `LabOrder.cs:21`; `MedicalRecordTimelineService.cs:495-505`; source SHA `0e2eb105` |
| `RM-FACT-011` | Fact | **`RM-DEC-006` disahkan tetapi belum diimplementasikan.** Penanda `Susulan` beserta tanggal masuk tidak ditemukan di mana pun — penelusuran kata `Susulan` dan `LateEntry` pada seluruh `Areas/`, `Services/`, dan `src/` frontend menghasilkan nol temuan. `RM-FE-001` yang mewajibkan penanda itu terlihat ikut menggantung | Modul rekam medis | `draft` | — | Penelusuran source 10 September 2026; SHA `0e2eb105` dan `0e5d3c36` |
| `RM-FACT-012` | Fact | **Layar penelusuran hanya membaca 13 dari 20 model `ClinicalManagement`.** Yang belum terbaca: `CliNursingCarePlan`, `CliNursingCarePlanItem`, `CliNursingCarePlanItemRevision`, `CliNursingIntervention`, `CliPhysicianVisit`, `TrxNosocomialInfection`, dan `CliClinicalMilestoneFact`. Jarak ini **melebar sendiri** setiap kali `ClinicalManagement` bertambah tabel, tanpa ada yang memberi tahu | Modul rekam medis | `draft` | — | `ClinicalManagement/Models/`; `MedicalRecordTimelineService.cs:1021-1289`; source SHA `0e2eb105` |
| `RM-DEC-027` | Decision | **Yoga Aji Pratama** ditetapkan memegang ketiga peran sekaligus: pemilik proses, pemilik tata kelola klinis, dan pemilik keamanan/privasi. Seluruh keputusan `RM-DEC-001` sampai `RM-DEC-026` disahkan atas namanya pada 26 Agustus 2026, sehingga pembangunan backend dapat dimulai. **Batas yang tetap berlaku:** pengesahan ini tidak menggantikan tinjauan komite medik atas `RM-DEC-003`, `RM-DEC-004`, dan `RM-DEC-020`, maupun tinjauan pihak perlindungan data atas `RM-DEC-017`, `RM-DEC-021`, `RM-DEC-022`, dan `RM-DEC-024`, bila kedua pihak itu kelak ditunjuk. Bila tinjauan tersebut menghasilkan keputusan berbeda, bagian desain yang bergantung padanya wajib dirombak. **Risiko ini diterima secara sadar.** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama — 26 Agustus 2026 | Penetapan kepemilikan modul 26 Agustus 2026 |
| `RM-DEC-028` | Decision | **Yoga Aji Pratama** ditetapkan pula sebagai **pemilik frontend dan pemilik API**, melengkapi ketiga peran pada `RM-DEC-027`. Dengan itu `api_authority` dan `frontend_authority` tidak lagi `OPEN`, dan `contracts/api-contract.md` naik dari `draft` menjadi **`approved`** pada versi `0.1.0`. Gerbang paralel frontend terbuka: sepuluh task `FE-00` sampai `FE-09` tidak lagi `TERTAHAN KONTRAK`. **Yang ikut disahkan** adalah dua delta kontrak yang diterapkan `BE-14` dan tercatat pada api-contract bagian 2: (1) bentuk balasan `/timeline` berubah dari `PagedResult` langsung menjadi selubung `MedicalRecordTimelineResponse` yang memuat halaman beserta `failedSources`, `isTruncated`, dan `isComplete`; (2) field `access` ditambahkan pada seluruh balasan endpoint berkas rekam medis. **Batas yang tetap berlaku:** sama seperti `RM-DEC-027`, pengesahan ini tidak menggantikan tinjauan komite medik maupun pihak perlindungan data bila kedua pihak itu kelak ditunjuk. | Yoga Aji Pratama | `approved` | Yoga Aji Pratama — 27 Agustus 2026 | Penetapan kepemilikan frontend 27 Agustus 2026 |
| `RM-DEC-029` | Decision | **Prefix modul `Mrc` dinaikkan dari `PLANNED` menjadi `ACTIVE`** pada Module Ownership & Prefix Registry, 31 Agustus 2026. Ini membuka normalisasi LEGACY MIGRATION empat entity operasional rekam medis dari `Trx*` ke `Mrc*` beserta tabel fisiknya, sesuai QBE-NAM-003 yang menuntut class, berkas, configuration, DbSet, rujukan, dan tabel dinormalkan bersama. Nama barunya: `MrcClinicalDocumentIntegrity`, `MrcClinicalNoteAddendum`, `MrcClinicalNoteAuthorDelegation`, dan **`MrcAccessLog`** — yang terakhir memendek karena registry melarang mengulang nama pemilik di dalam nama entity. **Yang ikut disahkan pada keputusan yang sama:** dua task backend baru di luar `BE-01`–`BE-18`, yaitu `BE-19` (penanda kunjungan aktif pada endpoint pencarian pasien) dan `BE-20` (`MedicalRecordAccessPurposeController`, enam endpoint api-contract bagian 7). **Batas yang tetap berlaku:** wewenang ini mencakup source dan pembuatan migration; eksekusi database di luar dev pemilik dan deployment tetap wewenang terpisah. | Yoga Aji Pratama | `approved` | Yoga Aji Pratama — 31 Agustus 2026 | Registry change log 31 Agustus 2026; migration `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix` |
| `RM-DEC-030` | Decision | **Hasil laboratorium dibaca langsung ke modul Laboratorium setiap kali berkas rekam medis dibuka. Menyalin isi hasil ke tabel modul lain dilarang.** Ketika modul Laboratorium tidak dapat dihubungi, bagian hasil laboratorium **wajib** menampilkan pesan gangguan dan **dilarang** menampilkan daftar kosong, karena daftar kosong terbaca sebagai "pasien tidak punya pemeriksaan" — kesimpulan yang salah dan berbahaya. Alasan pemilihan: salinan yang lupa diperbarui membuat dokter mengambil keputusan pengobatan atas hasil yang sudah diralat. Sejalan dengan `RAD-DEC-006` pada blueprint Radiologi, sehingga dua modul penunjang tidak berbeda aturan untuk masalah yang sama. | Product/domain owner | `approved` | Yoga Aji Pratama — 10 September 2026 | Amendment pass 10 September 2026; capability map sambungan Laboratorium `RK-03` |
| `RM-DEC-031` | Decision | **Rekam medis meminta hasil laboratorium dengan mengirim id pasien; modul Laboratorium yang menerjemahkannya menjadi kunjungan-kunjungan milik pasien tersebut.** Nol kolom ditambahkan pada tabel Laboratorium. Menutup conflict `RL-04`: tidak satu pun model Laboratorium menyimpan `PatientId`, sementara berkas rekam medis berpusat pada pasien. Dua alternatif ditolak: mengirim daftar id kunjungan dari rekam medis berisiko melewatkan kunjungan pada pasien hasil penggabungan nomor rekam medis, dan menambah kolom `PatientId` pada tabel laboratorium menciptakan dua sumber kebenaran yang dapat berselisih dengan kunjungannya. | Product/domain owner | `approved` | Yoga Aji Pratama — 10 September 2026 | Amendment pass 10 September 2026; capability map sambungan Laboratorium `RL-04` |
| `RM-DEC-032` | Decision | **Hasil laboratorium tidak tunduk pada aturan keutuhan dokumen rekam medis.** Laboratorium yang merilis hasil, Laboratorium pula yang mengatur ralat atas hasil yang sudah dirilis. Rekam medis hanya menampilkan penanda "hasil ini pernah diralat" yang dibacanya dari Laboratorium. Alasan: konsisten dengan `RM-DEC-030` — modul yang tidak menyimpan sebuah dokumen tidak berada pada posisi mengunci atau menandatanganinya. `RM-DEC-019` yang membatasi penegakan keutuhan pada CPPT tetap berlaku apa adanya. | Clinical governance owner | `approved` | Yoga Aji Pratama — 10 September 2026 | Amendment pass 10 September 2026 |
| `RM-DEC-033` | Decision | **Melihat hasil laboratorium di dalam berkas rekam medis cukup memerlukan izin `MedicalRecord : Read`.** Izin pada modul Laboratorium tidak diperlukan. Berkas rekam medis diperlakukan sebagai satu kesatuan: sekali seseorang berhak membukanya dan sudah melewati pemeriksaan keperluan akses, seluruh isinya terbaca. Alternatif "wajib punya kedua izin" ditolak karena menghasilkan bagian laboratorium kosong tanpa keterangan bagi dokter yang berhak atas berkas — bahaya yang sama persis dengan daftar kosong yang sudah ditolak `RM-DEC-030`. | Security/privacy owner | `approved` | Yoga Aji Pratama — 10 September 2026 | Amendment pass 10 September 2026 |
| `RM-DEC-034` | Decision | **Satu baris jejak akses per pembukaan berkas, sudah mencakup hasil laboratorium di dalamnya.** Tidak ada baris tambahan per hasil yang dibuka. Akibat yang diterima secara sadar: audit dapat menjawab "siapa membuka berkas pasien ini" tetapi tidak "siapa membaca hasil laboratorium yang mana". Alasan: `MrcAccessLog` dirancang bertahan **25 tahun** terbagi per tahun (`RM-DEC-024`), dan pencatatan per hasil melonjakkan volumenya jauh di atas rancangan kapasitas itu. | Security/privacy owner | `approved` | Yoga Aji Pratama — 10 September 2026 | Amendment pass 10 September 2026; turunan `RM-DEC-024` |
| `RM-DEC-035` | Decision | **Hasil susulan diterima tanpa batas waktu.** Kunjungan yang sudah ditutup tidak pernah dibuka kembali, dan hasil tetap tertaut padanya berapa pun jaraknya. **Berkas dinyatakan belum lengkap selama masih ada pemeriksaan yang hasilnya belum keluar**, dengan keadaan "menunggu hasil". Alasan: hasil kultur dan patologi anatomi wajar keluar berminggu-minggu setelah pasien pulang, sehingga batas waktu buatan akan membuang hasil yang sah dan justru memutus kaitannya dengan kunjungan yang melahirkannya. Konsekuensi yang diterima: unit rekam medis memerlukan daftar pantau berkas yang menunggu hasil, supaya berkas yang menggantung tidak hilang diam-diam. **Menggantikan `RM-DEC-007`.** | Product/domain owner | `approved` | Yoga Aji Pratama — 10 September 2026 | Amendment pass 10 September 2026; menutup `RM-DEC-007` |
| `RM-DEC-036` | Decision | **Entri hasil laboratorium — input nilai, verifikasi, dan rilis — dibangun lebih dulu di modul Laboratorium.** Aturan sambungan disahkan sekarang lewat `RM-DEC-030` sampai `RM-DEC-035`, tetapi **implementasinya tertahan prasyarat tersebut**. Dasar keputusan: audit 10 September 2026 membuktikan modul Laboratorium belum menyimpan hasil sama sekali — `LabExaminationStatus` hanya mengenal `Ordered`, `ChargeEligible`, `Voided`, dan `Cancelled`; `LabValueBound` dan `LabValueOption` adalah master nilai rujukan dan nilai kritis, bukan hasil pasien. Menyambungkan lebih dulu berarti membangun jalur kosong yang tetap harus dibongkar ketika bentuk hasil ditetapkan. **Pekerjaan entri hasil bukan bagian modul rekam medis** dan memerlukan wawancara serta desainnya sendiri pada blueprint Laboratorium. | Product/domain owner | `approved` | Yoga Aji Pratama — 10 September 2026 | Amendment pass 10 September 2026; capability map sambungan Laboratorium `RL-01`, `RL-02` |
| `RM-DEC-037` | Decision | **Amandemen 28 Oktober 2026 mencakup dua hal: sambungan berkas rekam medis ke dokumen klinis seluruh modul yang sudah ada, dan kelengkapan berkas (cakupan nomor 4).** Cakupan 5 sampai 8 — koding ICD, resume medis, pelepasan informasi, retensi — **tetap rilis berikutnya**. Alasan: sambungan dan kelengkapan saling membutuhkan (kelengkapan tidak dapat dinilai tanpa membaca dokumen modul lain), sedangkan cakupan 5 sampai 8 bergantung pada SOP dan dasar regulasi yang belum diserahkan. Alternatif "seluruh cakupan 4 sampai 8" dan "sambungan saja" ditolak | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Amendment pass revision 7, pertanyaan 1 |
| `RM-DEC-038` | Decision | **Aturan sambungan Laboratorium berlaku untuk seluruh modul.** Dokumen modul lain dibaca langsung ke modul pemiliknya setiap kali berkas dibuka; **menyalin isinya ke tabel rekam medis dilarang**. Rekam medis meminta dengan mengirim id pasien, dan modul pemilik yang menerjemahkannya menjadi kunjungan, episode, atau kasusnya sendiri. Memperluas `RM-DEC-030` dan `RM-DEC-031` dari satu modul menjadi seluruh modul. Akibat yang diterima: setiap modul wajib menyediakan jalur baca berlingkup pasien, dan berkas memanggil lebih banyak modul setiap kali dibuka. Alternatif "salin ringkasan saat final" ditolak karena bertentangan dengan `RM-DEC-030` dan berisiko salinan basi ketika ada ralat | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 2; preseden `RM-DEC-030`, `RAD-DEC-006` |
| `RM-DEC-039` | Decision | **Modul yang disambungkan, dalam empat kelompok:** (A) Laboratorium, Radiologi; (B) IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah; (C) Farmasi dan model `ClinicalManagement` yang belum terbaca, termasuk tujuh tabel `RM-FACT-012` dan model yang lahir sesudahnya; (D) MCU. Daftar model per modul pada bagian "Sumber yang dibaca" adalah titik awal penelusuran, bukan keputusan tabel. MCU dipilih pemilik **di luar** rekomendasi awal, dengan sadar membuka `MCU-GAP-04` | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 3 |
| `RM-DEC-040` | Decision | **Satu izin untuk seluruh isi berkas.** `MedicalRecord : Read` ditambah pemeriksaan keperluan akses sudah cukup untuk membaca dokumen modul mana pun di dalam berkas; izin pada modul asal tidak diperlukan. **Satu baris jejak akses per pembukaan berkas**, mencakup seluruh dokumen modul lain di dalamnya. Label kerahasiaan tetap hanya penanda (`RM-DEC-018`). Memperluas `RM-DEC-033` dan `RM-DEC-034`. **Hasil dan resume MCU tampil penuh** dengan izin yang sama; `McuReleaseConsent` tetap hanya mengatur penyerahan ke perusahaan, bukan pembacaan internal. Menutup `MCU-GAP-04` dari sisi rekam medis; dasar regulasi masa simpan resume MCU tetap tercatat terbuka (`VERIFY_CURRENT_REGULATION`) tetapi **tidak menahan**. Syarat penyerta: serologi HIV/hepatitis (Hemodialisa) dan hasil MCU kini terbaca oleh siapa pun yang berhak membuka berkas — keadaan ini wajib menjadi bahan tinjauan pihak perlindungan data bila kelak ditunjuk (`RM-DEC-027`) | Security/privacy owner; pemilik MCU | `approved` | Yoga Aji Pratama — 28 Oktober 2026, selaku pemilik rekam medis dan pemilik MCU | Pertanyaan 4 dan 8 |
| `RM-DEC-041` | Decision | **Hanya dokumen yang sudah final menurut modul pemiliknya yang tampil di berkas.** Contoh: hasil lab yang dirilis, laporan radiologi final, ringkasan pulang yang disahkan. Dokumen yang belum final **tidak** tampil, tetapi dihitung sebagai "menunggu" pada kelengkapan. Arti "final" untuk tiap modul **ditetapkan modul pemiliknya** dan dibaca dari kontrak integrasinya, bukan didefinisikan ulang di sini. Alternatif "final + konsep berlabel" ditolak karena isi yang belum diverifikasi dapat terbaca sebagai hasil resmi | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 7 |
| `RM-DEC-042` | Decision | **Ralat dan penguncian dokumen modul lain diatur modul pemiliknya.** Rekam medis hanya menampilkan penanda "pernah diralat" yang dibacanya dari modul pemilik. Lima jenis yang sudah terdaftar pada mesin keutuhan rekam medis — `ProgressNote`, `Consultation`, `Assessment`, `Procedure`, `HemodialysisSession` — **tetap** memakai mesin itu. Memperluas `RM-DEC-032`. Nol perubahan wajib pada modul lain akibat keputusan ini. Alternatif "wajib mesin keutuhan rekam medis untuk semua" ditolak karena menuntut perubahan di belasan modul dan berbenturan dengan mekanisme revisi yang sudah berjalan (`RadReportVersion`, `InpDischargeSummaryRevision`, `Cli*Revision`, `OprExecutionAddendum`, dan lainnya) | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 9 |
| `RM-DEC-043` | Decision | **Penanda `Susulan` dihitung saat berkas dibaca, tidak disimpan.** Sebuah dokumen bertanda `Susulan` bila waktu finalnya **sesudah** waktu penutupan kunjungannya. Berlaku untuk seluruh jenis dokumen, termasuk 13 jenis lama. Nol kolom baru. Mewujudkan `RM-DEC-006` yang disahkan sejak 26 Agustus tetapi nol implementasi (`RM-FACT-011`), dan **mengubah cara** — bukan isi — acceptance criteria nomor 8: penanda tidak lagi "tersimpan", melainkan diturunkan. Syarat penyerta: setiap modul wajib menyediakan waktu final dokumennya pada jalur baca `RM-DEC-038` | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 10 |
| `RM-DEC-044` | Decision | **Gangguan ditangani per bagian.** Bila satu modul tidak dapat dibaca, berkas tetap terbuka; bagian modul itu menampilkan pesan gangguan dan bagian lain tampil normal. Penilaian kelengkapan kunjungan yang terdampak berkeadaan **`Tidak dapat dipastikan`** dan **tidak pernah** `Lengkap`. Memperluas `RM-DEC-030`. Alternatif "seluruh berkas gagal" ditolak karena satu modul gangguan akan menutup akses ke seluruh riwayat pasien | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 12 |
| `RM-DEC-045` | Decision | **Berkas sebuah kunjungan dinyatakan lengkap bila tiga syarat terpenuhi seluruhnya:** (1) semua dokumen yang tunduk keutuhan sudah ditandatangani, atau kekurangannya sudah dituntaskan menurut `RM-DEC-049`; (2) tidak ada hasil penunjang yang masih ditunggu (`RM-DEC-035`); (3) dokumen wajib menurut jenis kunjungan dan syarat bersyaratnya ada. **Daftar dokumen wajib disimpan sebagai master yang diatur unit rekam medis**, bukan ditulis tetap di program, supaya perubahan SOP tidak menuntut rilis program | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 5 |
| `RM-DEC-046` | Decision | **Berkas belum lengkap hanya masuk daftar kerja petugas rekam medis, tidak menahan apa pun.** Penutupan kunjungan, pemulangan, dan tagihan tetap berjalan. **Yang dinilai:** hanya kunjungan yang sudah ditutup; kunjungan batal tidak dinilai, tetapi dokumennya tetap tampil di berkas. **Keadaan per kunjungan:** satu keadaan utama dengan urutan prioritas `Tidak dapat dipastikan` → `Menunggu hasil` → `Belum lengkap` → `Lengkap`, ditambah rincian **seluruh** kekurangan di bawahnya. Contoh: ringkasan pulang belum ada **dan** kultur belum keluar → keadaan `Menunggu hasil` dengan dua rincian. Alternatif "menahan penutupan kunjungan" ditolak karena bertentangan dengan `RM-DEC-003` dan `RM-DEC-035` | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 6, 16, dan 17 |
| `RM-DEC-047` | Decision | **Isi awal master dokumen wajib disahkan** sebagaimana tabel "Daftar dokumen wajib awal" pada bagian amandemen cakupan: lima jenis kunjungan dari `EncounterType` ditambah tiga syarat bersyarat (operasi, sesi hemodialisa, transfusi). Usulan disusun agent dari dokumen yang memang dihasilkan modul yang ada, sehingga tidak ada syarat yang mustahil dipenuhi. **Bukan** SOP rumah sakit; bila SOP diserahkan, daftar diselaraskan lewat layar master | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 11 dan 13 |
| `RM-DEC-048` | Decision | **Tampilan dan urutan.** Dokumen modul lain masuk **layar penelusuran yang sudah ada** sebagai jenis dokumen baru, dengan saringan jenis dan pesan gangguan per bagian; daftar kerja kelengkapan menjadi **entri menu baru di grup Rekam Medis**. Tata letak rinci = `DEV_DISCRETION` mengikuti konvensi project, dengan brief UI yang tetap disahkan pemilik. **Urutan pengerjaan:** (1) Laboratorium dan Radiologi, (2) kelompok pelayanan, (3) Farmasi dan klinis baru, (4) MCU, lalu kelengkapan berkas dan penanda `Susulan`. Alternatif "tab per modul" ditolak karena urutan waktu lintas modul hilang | Product/domain owner; frontend authority | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 14 dan 15 |
| `RM-DEC-049` | Decision | **Catatan `LockedUnsigned` dituntaskan lewat addendum pengesahan.** Penulis asli — atau kepala unit/DPJP bila penulis berhalangan menurut `RM-DEC-004` dan `RM-DEC-020` — membuat addendum yang menyatakan isi catatan benar. Begitu addendum itu ditandatangani, kekurangan "tidak ditandatangani" dianggap tuntas pada syarat 1 `RM-DEC-045`. **Status catatan tetap `LockedUnsigned`** sebagai jejak jujur; nol status keutuhan baru. Alternatif "petugas rekam medis menerima dengan catatan" ditolak karena petugas non-klinis akan menyatakan sah catatan klinis yang tidak pernah disahkan penulisnya | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 18 |
| `RM-DEC-050` | Decision | **Daftar kerja kelengkapan dimulai dari tanggal rilis fitur.** Hanya kunjungan yang ditutup sejak tanggal itu masuk daftar kerja. Kunjungan lama tetap dapat dibuka dan dinilai bila diminta, tetapi tidak memenuhi daftar kerja. Alasan: migration `RM-DEC-014` memberi `LockedUnsigned` pada seluruh catatan lama, sehingga tanpa batas ini daftar kerja sejak hari pertama berisi ribuan butir yang praktis tidak akan ditindaklanjuti | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 19 |
| `RM-FACT-013` | Fact | **Modul berdokumen klinis yang ada pada 28 Oktober 2026:** `BloodBankManagement` (16 model), `ClinicalManagement` (43), `EmergencyInstallationManagement` (24), `HemodialysisManagement` (22), `InPatientManagement` (28), `LaboratoryManagement` (30), `MedicalCheckupManagement` (21), `NutritionManagement` (8), `OperatingRoomManagement` (18), `PharmacyManagement` (36), `RadiologyManagement` (12), `RegistrationManagement` (10). `ClinicalManagement` naik dari 20 model (`RM-FACT-012`, 10 September) menjadi 43 | — | `draft` | — | Hitung berkas `Models/*.cs` per area, SHA `363de34f` |
| `RM-FACT-014` | Fact | **Layar penelusuran tetap hanya membaca 13 jenis lama.** `ClinicalDocumentKind` sudah memuat `HemodialysisSession = 14` dan `LaboratoryResult = 15`, tetapi `MedicalRecordTimelineService.AmbilJenisAsync` tidak punya cabang untuk keduanya, sehingga jatuh ke cabang kosong `_ =>`. Akibatnya kedua jenis **diam-diam kosong**, bukan gangguan — persis keadaan yang dilarang `RM-DEC-030`. Folder frontend `medical-record-management` tidak memuat bagian laboratorium | Modul rekam medis | `draft` | — | `MedicalRecordTimelineService.cs:236-470`, `ClinicalDocumentKind.cs`, SHA `363de34f`/`f0a3b07b8` |
| `RM-FACT-015` | Fact | **Prasyarat `RM-DEC-036` sudah terpenuhi sebagian.** Laboratorium kini merilis hasil (`BE-LAB-70`, `ClinicalDocumentKind.LaboratoryResult`, dipakai `LabResultValidationService`) dan punya master alasan ralat (`LabResultCorrectionReason`). **Jalur baca hasil berlingkup pasien untuk berkas belum ada**: satu-satunya rute berlingkup pasien adalah `kiosk/pending-by-patient/{patientId}` untuk kiosk | Blueprint Laboratorium | `draft` | — | `LabOrderController.cs:286`, SHA `363de34f` |
| `RM-FACT-016` | Fact | **Sebagian modul sudah punya jalur baca berlingkup pasien yang mungkin dapat dipakai ulang:** Radiologi `GET by-patient/{patientId}` pada `RadReportController`, Bank Darah `GET patient/{patientId}/valid` pada `BbkBloodGroupExamController`. Kecocokannya untuk `RM-DEC-038` — termasuk apakah keduanya hanya mengembalikan dokumen final dan menyertakan waktu final — **belum diperiksa** | — | `draft` | — | `RadReportController.cs:220`, `BbkBloodGroupExamController.cs:187`, SHA `363de34f` |
| `RM-FACT-017` | Fact | `EncounterType` bernilai `Unknown`, `Outpatient`, `Emergency`, `Inpatient`, `MedicalCheckup`, `Telemedicine`. Operasi dan hemodialisa **bukan** jenis kunjungan — dasar pemisahan syarat bersyarat pada `RM-DEC-047` | — | `draft` | — | `RegistrationManagement/Enums/EncounterType.cs`, SHA `363de34f` |
| `RM-FACT-018` | Fact | **`RM-DEC-006` masih nol implementasi** per 28 Oktober 2026 — penelusuran `Susulan`, `LateEntry`, dan `IsLate` pada `MedicalRecordManagement` backend serta folder frontend `medical-record-management` menghasilkan nol temuan. `RM-FACT-011` tetap berlaku | Modul rekam medis | `draft` | — | SHA `363de34f`/`f0a3b07b8` |
| `RM-DEC-051` | Decision | **"Kunjungan ditutup" berarti tutup klinis**, yaitu saat pelayanan selesai dan catatan dikunci otomatis: episode rawat inap ditutup, visit IGD selesai, atau status kunjungan `Completed` untuk jenis lain. Waktu ini menjadi acuan penanda `Susulan` (`RM-DEC-043`), cakupan daftar kerja (`RM-DEC-046`), dan batas tanggal rilis (`RM-DEC-050`). **Rekonsiliasi administratif IGD yang mengubah `CompletedAt` tidak mengubah waktu tutup klinis.** Contoh: Pak Budi pulang Senin, tagihan final Kamis, kultur dirilis Selasa → kultur bertanda `Susulan`. Menutup conflict `RM-CAP-036`. Alternatif "tutup administratif" ditolak karena penanda klinis akan bergantung pada urusan kasir | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Closure pass revision 8, pertanyaan 1; `RM-CAP-036` |
| `RM-DEC-052` | Decision | **Pencatatan berkala masuk linimasa sebagai ringkasan per hari per jenis**, misalnya "Gula darah — 6 pengukuran, 28 Oktober", yang dapat dibuka rinciannya. Berlaku untuk model `RM-CAP-066`: observasi harian, balans cairan, gula darah, alat terpasang, drain WSD, surveilans luka operasi. Ringkasan dihitung saat dibaca, tanpa salinan (`RM-DEC-038`). Alternatif "satu baris per pengukuran" ditolak karena dokumen penting tenggelam; "tidak masuk" ditolak karena bertentangan dengan `RM-DEC-039` | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 2; `RM-CAP-066` |
| `RM-DEC-053` | Decision | **Syarat asesmen pada dokumen wajib:** Rawat Inap wajib asesmen awal keperawatan (`Initial`) **dan** asesmen awal medis (`MedicalInitial`), keduanya `Completed`. Rawat Jalan dan IGD cukup **salah satu** asesmen awal (`Initial` atau `MedicalInitial`) yang `Completed`. Asesmen ulang, asesmen harian, dan asesmen khusus (risiko jatuh, nyeri, edukasi, perencanaan pulang, manajemen kasus) **tidak** dihitung sebagai asesmen awal. Memperjelas butir "asesmen" dan "asesmen awal" pada `RM-DEC-047` | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 3 |
| `RM-DEC-054` | Decision | **Diagnosis utama terpenuhi bila ada diagnosis yang dicentang utama (`IsPrimary`) atau berjenis `Primary`**, dan statusnya bukan `Cancelled` maupun `RuledOut`. Adanya dua penanda dicatat sebagai temuan untuk `ClinicalManagement`; merapikannya bukan pekerjaan modul ini. Menutup conflict pemetaan diagnosis utama pada capability map revision 3 | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 4 |
| `RM-DEC-055` | Decision | **"CPPT/SOAP dokter" terpenuhi oleh CPPT berprofesi dokter atau konsultasi dokter (`TrxDoctorConsultation`)**, salah satu cukup, yang tidak dibatalkan. Alasan: poliklinik yang mencatat lewat layar konsultasi tidak boleh salah dinyatakan kurang | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 5 |
| `RM-DEC-056` | Decision | **Persetujuan umum rawat inap hanya terpenuhi oleh dokumen admisi `GeneralConsent` berstatus `Completed`.** Persetujuan umum registrasi (`RegPatientGeneralConsent`) berlaku untuk kunjungan secara umum dan **tidak** menggantikannya. Contoh: pasien IGD yang menandatangani persetujuan registrasi lalu dirawat inap tetap belum lengkap sampai dokumen admisinya selesai. Menutup conflict pemetaan persetujuan umum | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 6 |
| `RM-DEC-057` | Decision | **Hasil laboratorium Patologi Klinik dan Mikrobiologi tampil di berkas sekarang, tanpa menunggu fitur ralat hasil terrilis (`S6`).** Dasarnya fakta `RM-FACT-019`: hasil non-PA yang sudah divalidasi tidak dapat dibuka kembali, sehingga penanda "pernah diralat" saat ini memang benar bernilai tidak. **Syarat penyerta yang mengikat Laboratorium:** `S6` tidak boleh dirilis sebelum penanda "pernah diralat" tersambung ke rekam medis — bila tidak, hasil yang diralat akan tampil tanpa penanda. Juga menutup kekosongan diam-diam `RM-CAP-035` untuk jenis `LaboratoryResult` | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 7; `RM-FACT-019` |
| `RM-DEC-058` | Decision | **Hasil Patologi Anatomi ditahan dari berkas** sampai Laboratorium memastikan laporan yang sudah dirilis tidak dapat berubah diam-diam. Selama ditahan, bagian itu wajib menyatakan "hasil Patologi Anatomi belum tersedia di berkas" — **bukan** kosong diam-diam (`RM-DEC-030`). Temuan `RM-FACT-020` diteruskan ke blueprint Laboratorium. Alternatif "tampilkan dengan penanda buka-ulang" ditolak karena `ReopenCount` tidak membedakan buka-ulang sebelum dan sesudah rilis | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan tambahan revision 8; `RM-FACT-020` |
| `RM-DEC-059` | Decision | **Aturan final sementara untuk tiga sumber yang belum tegas di source:** disposisi IGD final saat **`Confirmed`** (dikonfirmasi dokter; `Executed` adalah pelaksanaan, bukan pengesahan dokumen); catatan asuhan gizi dan catatan kelahiran final **saat tersimpan**, karena keduanya tidak punya status. Catatan asuhan gizi diberi label **"dapat berubah"**, karena diagnosis gizinya dapat diganti tanpa riwayat (`RM-CAP-054`). Aturan ini berlaku sebagai aturan rekam medis sekarang dan **diteruskan ke pemilik IGD, Gizi, dan Kamar Operasi untuk dikonfirmasi**; konfirmasi tidak menahan desain. Bila pemilik modul menetapkan arti final berbeda, keputusan itu menggantikan butir terkait (`RM-DEC-041`) | Product/domain owner; konfirmasi pemilik IGD, Gizi, Kamar Operasi | `approved` (sementara) | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 8; `RM-CAP-047`, `052`, `054` |
| `RM-DEC-060` | Decision | **Master dokumen wajib dikunci per jenis kunjungan dan jenis unit layanan.** Kunjungan Rawat Jalan yang unit layanannya berjenis `Laboratory` atau `Radiology` **tidak** dituntut asesmen, diagnosis utama, maupun CPPT/SOAP dokter; ia lengkap bila syarat 1 dan syarat 2 `RM-DEC-045` terpenuhi (catatan yang ada sudah ditandatangani, seluruh pemeriksaan yang dipesan sudah dirilis). Kunjungan Rawat Jalan di unit berjenis `Hemodialysis` wajib catatan sesi yang sudah ditandatangani saja. Unit berjenis lain — termasuk `Other` dan `Unknown` — mengikuti daftar Rawat Jalan biasa. Pembeda diambil dari `MstServiceUnit.ServiceUnitType`, master yang andal, bukan dari isian per kunjungan. Contoh: Ibu Rina datang dengan rujukan klinik luar hanya untuk cek gula darah → lengkap begitu hasilnya dirilis. **Menutup `DEC-RM-001`** dan **memperluas** `RM-DEC-047` (tidak menggantikannya). Alternatif "penanda dokter wajib" ditolak karena bergantung isian pendaftaran yang dapat salah; "tidak dinilai" ditolak karena hasil yang belum keluar tidak akan terpantau | Clinical governance owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Closure pass revision 9, pertanyaan 1; `RM-RCG-F02`, `RM-RCG-F03` |
| `RM-DEC-061` | Decision | **Surat rujukan tidak menjadi dokumen wajib**, termasuk bagi kunjungan rujukan (`IsReferral`). Rujukan diurus registrasi dan penjamin, bukan dokumen klinis berkas. Bila kelak diperlukan, butir ini ditambahkan lewat layar master tanpa perubahan struktur (`RM-DEC-045`) | Product/domain owner | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan 2 |
| `RM-DEC-062` | Decision | **Blueprint `RM-BP-001` revision 4 dan seluruh kontrak `0.2.0` disahkan** — API, state transition, validation, integration, permission/audit — beserta `04-prd-to-mvp.md`, ERD, kamus data, dan matriks uji amandemen. Revision 3 tetap menjadi dasar rilis pertama yang sudah berjalan; revision 4 menambah tanpa menimpanya. **Batas yang tetap berlaku:** persetujuan pemilik IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah, Farmasi, dan Radiologi atas berkas pembaca di folder modul mereka belum diberikan dan tidak tercakup pengesahan ini; tinjauan komite medik dan perlindungan data (`RM-DEC-027`) tetap belum dilakukan | Product/domain, clinical governance, security/privacy, API, frontend authority | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Jawaban "Sahkan semuanya" atas pertanyaan pengesahan |
| `RM-DEC-063` | Decision | **Selaku pemilik modul Laboratorium dan MCU, Yoga Aji Pratama menyetujui pembaca sumber ditulis di folder kedua modul itu** (`LabMedicalRecordDocumentSource`, `McuMedicalRecordDocumentSource`; `INT-RM-10a`, `INT-RM-10j`), termasuk kewajiban Laboratorium bahwa `S6` tidak dirilis sebelum penanda "pernah diralat" tersambung ke rekam medis (`RM-DEC-057`) | Pemilik Laboratorium; pemilik MCU | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Jawaban "Setuju keduanya" |
| `RM-DEC-064` | Decision | **Selaku pemilik modul Radiologi (`RAD-DEC-014`), Yoga Aji Pratama menyetujui pembaca `RadMedicalRecordDocumentSource` ditulis di folder Radiologi** (`INT-RM-10b`), memakai `RadReportService.GetByPatientAsync` dalam proses. Farmasi dan Gizi **tidak** disetujui pada kesempatan yang sama — pemiliknya belum dipastikan | Pemilik Radiologi | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Jawaban pertanyaan persetujuan perencanaan delivery |
| `RM-DEC-065` | Decision | **Resume MCU yang tampil di berkas adalah versi terakhir yang pernah diverifikasi** (`VerifiedAt` terisi), apa pun statusnya sekarang. Selama koreksi berjalan — versi lama berstatus `Corrected`, versi baru masih `Draft`/`Approved` — versi lama itu tetap tampil dengan keterangan "sedang dikoreksi". Begitu versi baru diverifikasi, versi baru yang tampil dan bertanda pernah diralat (versi > 1). Kriteria dokumen wajib `McuResume` terpenuhi dengan aturan yang sama. **Mengoreksi** `RM-DEC-040`/desain A4.10–A4.11 yang menganggap `Corrected` sebagai final (`RM-FACT-022`) | Product/domain owner; pemilik MCU | `approved` | Yoga Aji Pratama — 28 Oktober 2026, selaku pemilik rekam medis dan MCU | Pertanyaan saat `BE-36` |
| `RM-DEC-066` | Decision | **Hasil pemeriksaan MCU per komponen mengikuti resume**: tampil bila order punya versi terverifikasi, dengan waktu final = waktu verifikasi versi itu; selama koreksi berjalan, nilai yang tampil adalah nilai pada versi terverifikasi (direkonstruksi dari revisi nilai), dan hasil yang nilainya diubah lalu diverifikasi bertanda pernah diralat | Product/domain owner; pemilik MCU | `approved` | Yoga Aji Pratama — 28 Oktober 2026 | Pertanyaan saat `BE-36` |
| `RM-FACT-022` | Fact | **`McuResumeStatus.Corrected` berarti versi lama yang digantikan, bukan resume final yang diralat.** Membuka kembali resume menandai versi lama `Corrected` dan `IsCurrent = false`, lalu membuat versi baru `Draft` dengan `VersionNumber + 1`; `VerifiedAt` versi lama tidak dihapus | — | `draft` | — | `McuResumeService.cs:372-416`, SHA `363de34f` |
| `RM-FACT-023` | Fact | **Kesimpulan hasil pemeriksaan MCU ditimpa tanpa riwayat.** Nilai isian punya `McuExaminationResultValueRevision`, tetapi `McuExaminationResult.ConclusionText` diganti langsung saat koreksi. Kesimpulan versi terverifikasi tidak dapat dipulihkan selama koreksi berjalan; berkas menyembunyikannya dengan keterangan "sedang dikoreksi" | Blueprint MCU | `draft` | — | `McuExaminationService.cs:700-701`, SHA `363de34f` |
| `RM-FACT-021` | Fact | **Jenis unit layanan tersedia di master.** `MstServiceUnit.ServiceUnitType` bernilai `Unknown`, `Outpatient`, `Inpatient`, `Emergency`, `Laboratory`, `Radiology`, `Pharmacy`, `MedicalCheckup`, `OperatingRoom`, `DeliveryRoom`, `Hemodialysis`, `Other`. Kunjungan menautnya lewat `RegPatientEncounter.ServiceUnitId` | — | `draft` | — | `MasterData/Models/MstServiceUnit.cs`, SHA `363de34f` |
| `RM-FACT-019` | Fact | **Hasil laboratorium non-PA yang sudah divalidasi tidak dapat diubah.** Pengisian nilai menolak hasil yang sudah selesai (`EnsureResultNotFinalized`, `LabExaminationService.cs:1895-1901`); buka-ulang menolak hasil yang sudah divalidasi (`LabExaminationService.cs` sebelum baris 1664, pemeriksaan `ValidatedAt is not null`); validasi dan rilis menolak hasil yang sudah dirilis (`LabResultValidationService.cs:227`, `369`); pembatalan menolak hasil yang sudah dirilis (`LabExaminationService.cs:283-286`) | — | `draft` | — | SHA `363de34f` |
| `RM-FACT-020` | Fact | **Laporan Patologi Anatomi dapat dibuka kembali tanpa pemeriksaan status rilis.** `LabPathologyReportService.ReopenAsync` (`LabPathologyReportService.cs:344-372`) hanya memeriksa laporan pernah diselesaikan (`VAL-98`) dan alasan terisi (`VAL-97`); tidak ada pemeriksaan `ValidatedAt` atau `ReleasedAt`. Apakah ini celah atau disengaja belum dapat dipastikan dari source | Blueprint Laboratorium | `draft` | — | SHA `363de34f` |
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
| 9 | ~~Sampai kapan entri susulan diterima?~~ | **Tertutup** 10 September 2026 oleh `RM-DEC-035`: tanpa batas waktu, berkas berstatus "menunggu hasil". Pemicunya sudah tiba — modul Laboratorium kini ada |
| 10 | Alur penggabungan pasien duplikat dan dampaknya ke tampilan riwayat | Terbuka, `RM-CAP-007`. Perlu penelusuran source terarah, bukan keputusan manusia |
| 11 | Apakah perbaikan `RM-CAP-011` sampai `013` memerlukan uji otomatis lebih dulu? | Terbuka. Diteruskan ke `/plan-module-delivery` karena menyangkut penyusunan urutan kerja |
| 12 | Apakah rumah sakit sudah punya SOP rekam medis tertulis yang bisa dijadikan bukti? | Terbuka. Kini juga menjadi acuan penyelarasan master dokumen wajib (`RM-DEC-047`) |
| 13 | Arti "final" dan nama kolom waktu final untuk setiap modul pada `RM-DEC-039` | Terbuka. **Bukan keputusan rekam medis** (`RM-DEC-041`): dibaca dari kontrak integrasi tiap modul lewat `/trace-existing-capabilities`. Menahan **desain** jalur baca per modul. Bila suatu modul belum mendefinisikannya, pertanyaannya diteruskan ke pemilik modul itu |
| 14 | Dokumen mana tepatnya di tiap modul yang menjadi "diagnosis utama", "asesmen awal", "disposisi", "ringkasan pulang", dan seterusnya pada `RM-DEC-047` | Terbuka. Pemetaan nama dokumen ke tabel sumber adalah pekerjaan penelusuran source, bukan keputusan manusia. Menahan **desain** master dokumen wajib |
| 15 | Apakah jalur baca berlingkup pasien yang sudah ada (`RM-FACT-016`) cocok dipakai ulang | Terbuka, diteruskan ke `/trace-existing-capabilities` |
| 16 | Dasar regulasi masa simpan resume MCU | Terbuka, `VERIFY_CURRENT_REGULATION`. **Tidak menahan** (`RM-DEC-040`) |

Keadaan per 28 Oktober 2026 (revision 8):

| No | Pertanyaan | Status |
|---:|---|---|
| 13 | ~~Arti "final" dan kolom waktu final per modul~~ | **Tertutup** oleh capability map revision 3 kelompok F dan G, dilengkapi `RM-DEC-059` untuk tiga sumber yang belum tegas. Laporan Patologi Anatomi ditahan `RM-DEC-058` |
| 14 | ~~Pemetaan butir dokumen wajib ke tabel sumber~~ | **Tertutup** oleh capability map revision 3 kelompok I, dilengkapi `RM-DEC-053` sampai `RM-DEC-056` |
| 15 | ~~Kecocokan jalur baca berlingkup pasien yang sudah ada~~ | **Tertutup** oleh capability map revision 3 kelompok H: Radiologi cocok dengan syarat dipanggil dalam proses (`RM-CAP-058`); Bank Darah perlu dibaca isi kembaliannya saat desain (`RM-CAP-068`) |
| 17 | Konfirmasi arti final disposisi IGD, catatan asuhan gizi, dan catatan kelahiran oleh pemilik modulnya | **Terbuka. Tidak menahan desain** (`RM-DEC-059`). Pemilik: pemilik IGD, Gizi, Kamar Operasi |
| 18 | Apakah buka-ulang laporan Patologi Anatomi sesudah rilis adalah celah atau disengaja (`RM-FACT-020`) | **Terbuka.** Menahan **sambungan hasil PA saja** (`RM-DEC-058`). Pemilik: blueprint Laboratorium — Yoga Aji Pratama selaku pemilik modul Laboratorium |
| 19 | Apakah dua penanda diagnosis utama perlu dirapikan | Terbuka. Bukan pekerjaan modul ini (`RM-DEC-054`). Pemilik: `ClinicalManagement` |

### Blocker

| No | Blocker | Dampak |
|---:|---|---|
| 1 | ~~Tiga owner belum ditunjuk namanya~~ | **Tertutup** 26 Agustus 2026. Yoga Aji Pratama memegang ketiga peran pada `RM-DEC-027`, dilengkapi kepemilikan API dan frontend pada `RM-DEC-028` |
| 2 | ~~Angka masa simpan jejak akses~~ | **Tertutup** 24 Agustus 2026. Ditetapkan 25 tahun pada `RM-DEC-024` |
| 3 | Belum ada dokumen SOP rekam medis rumah sakit | Keputusan saat ini bertumpu pada praktik umum dan rekomendasi, bukan kebijakan setempat yang tertulis. Juga menahan pengisian awal master keperluan akses |
| 4 | ~~`RM-DEC-017` menunggu persetujuan security/privacy owner~~ | **Tertutup** 26 Agustus 2026 oleh `RM-DEC-027`. Yang tersisa bukan approval, melainkan tinjauan pihak perlindungan data bila kelak ditunjuk |
| 5 | Entri hasil laboratorium belum ada | Menahan **implementasi** seluruh sambungan `RM-DEC-030` sampai `RM-DEC-035`. Pemilik: blueprint Laboratorium. Lihat `RM-FACT-009` dan `RM-DEC-036` |
| 6 | Mekanisme ralat hasil laboratorium yang tercatat belum ada | Menahan implementasi `RM-DEC-032` dan `RM-FE-018`. Pemilik: blueprint Laboratorium |
| 7 | Endpoint Laboratorium berlingkup pasien belum ada | Menahan **desain** kontrak sambungan. Pemilik: blueprint Laboratorium. Turunan `RM-DEC-031` |
| 8 | `RM-DEC-006` disahkan tetapi nol implementasi | Menahan implementasi `RM-DEC-035` dan `RM-FE-001`. Pemilik: modul rekam medis sendiri, **tidak menunggu modul lain**. Lihat `RM-FACT-011` |

Keadaan per 28 Oktober 2026 (revision 7). Baris di atas tidak diubah supaya riwayatnya
terbaca; keadaan terkininya:

| No | Blocker | Keadaan 28 Oktober 2026 |
|---:|---|---|
| 5 | Entri hasil laboratorium belum ada | **Kemungkinan besar tertutup** — Laboratorium merilis hasil sejak `BE-LAB-70` (`RM-FACT-015`). Ditutup resmi setelah `/trace-existing-capabilities` memastikannya |
| 6 | Mekanisme ralat hasil laboratorium belum ada | **Sebagian** — master alasan ralat `LabResultCorrectionReason` ada. Apakah penanda "pernah diralat" dapat dibaca per hasil belum diperiksa |
| 7 | Endpoint Laboratorium berlingkup pasien belum ada | **Tetap terbuka** (`RM-FACT-015`). Kini menjadi satu contoh dari blocker 9 |
| 8 | `RM-DEC-006` nol implementasi | **Desainnya tertutup** oleh `RM-DEC-043` (dihitung saat dibaca). Implementasinya tetap terbuka (`RM-FACT-018`) |
| 9 | Jalur baca berlingkup pasien di setiap modul `RM-DEC-039` | **Baru.** Menahan **implementasi** sambungan modul yang belum menyediakannya, satu per satu — modul yang sudah siap tidak menunggu modul lain. Pemilik: blueprint masing-masing modul. Radiologi dan Bank Darah mungkin sudah punya (`RM-FACT-016`) |
| 10 | Capability audit lintas modul belum dijalankan untuk amandemen ini | **Baru.** Menahan **desain** seluruh amandemen. Diselesaikan `/trace-existing-capabilities` mode impact scan; menjawab open question 13, 14, dan 15 |
| 11 | Layar penelusuran memperlakukan `HemodialysisSession` dan `LaboratoryResult` sebagai daftar kosong diam-diam | **Baru**, `RM-FACT-014`. Utang modul ini sendiri; melanggar `RM-DEC-030` hari ini juga. Tidak menunggu modul lain |

Keadaan per 28 Oktober 2026 (revision 8, closure pass):

| No | Blocker | Keadaan |
|---:|---|---|
| 5 | Entri hasil laboratorium belum ada | **Tertutup** — capability map revision 3 memastikan hasil tersimpan dan dirilis |
| 6 | Mekanisme ralat hasil terrilis | **Tidak lagi menahan** sambungan PK/Mikrobiologi (`RM-DEC-057`). Yang tersisa adalah kewajiban Laboratorium: `S6` tidak dirilis sebelum penandanya tersambung |
| 7 | Jalur baca hasil lab berlingkup pasien | **Tetap terbuka** — menahan implementasi sambungan lab |
| 10 | Capability audit lintas modul | **Tertutup** — capability map revision 3 |
| 12 | Laporan Patologi Anatomi dapat dibuka kembali sesudah rilis | **Baru**, `RM-FACT-020`. Menahan sambungan hasil PA saja (`RM-DEC-058`). Pemilik: blueprint Laboratorium |

**Tidak ada blocker desain yang tersisa.** Blocker 7, 8, 9, 11, dan 12 menahan
**implementasi** bagian tertentu, bukan desain.

**Catatan penting — diperbarui 10 September 2026.** Kalimat pada revisi sebelumnya berbunyi
"tidak ada satu pun keputusan pada dokumen ini yang berstatus `approved`". Itu sudah **tidak
benar** sejak 26 Agustus 2026 dan dikoreksi di sini: dua puluh dua keputusan disahkan pada
`RM-DEC-027`, ditambah tujuh keputusan sambungan Laboratorium pada 10 September 2026.

Yang tetap perlu dibaca dengan hati-hati: seluruh peran owner dipegang **satu orang**, dan
tinjauan komite medik maupun pihak perlindungan data belum pernah dilakukan. Blocker 5 sampai
7 berada di modul Laboratorium, sedangkan blocker 8 milik modul ini sendiri dan dapat
dikerjakan tanpa menunggu siapa pun.

---

## Riwayat Pass

| Revision | Tanggal | Mode | Ringkasan |
|---:|---|---|---|
| 1 | 24 Agustus 2026 | Scope pass | Pass selesai. Tujuh pertanyaan dijawab menghasilkan `RM-DEC-001` sampai `RM-DEC-008`. Batas scope terkunci, cakupan rilis pertama ditetapkan, tiga invariant inti dirumuskan (penguncian, addendum, batas akses), satu exception path ditutup (data susulan). Lima pertanyaan turunan terbuka dan dua blocker tercatat. |
| 2 | 24 Agustus 2026 | Closure pass | Pass selesai setelah membaca `01-existing-capability-map.md`. Sebelas pertanyaan dijawab menghasilkan `RM-DEC-013` sampai `RM-DEC-023`. **Tiga `Conflict` tertutup**: model status (`RM-CAP-009`), kewenangan `SuperAdmin` (`RM-CAP-025`), tingkat kerahasiaan (`RM-CAP-026`). **Dua dari tiga `Unknown` tertutup**: bentuk tanda tangan (`RM-CAP-015`) dan `PrivateNote` (`RM-CAP-027`). Empat pertanyaan turunan Scope Pass tertutup. Acceptance criteria bertambah dari 9 menjadi 17. Tersisa satu `Unknown` (`RM-CAP-007`, penggabungan pasien duplikat) yang memerlukan penelusuran source, bukan keputusan, serta empat blocker. |
| 3 | 24 Agustus 2026 | Amendment pass | Penelusuran terarah `RM-CAP-007` selesai. Penggabungan pasien ternyata hanya penandaan, tidak memindahkan data klinis; statusnya naik menjadi `Conflict`. Ditemukan pula `RM-CAP-033`: fitur penggabungan tidak dapat dipakai dari antarmuka karena `mergeReason` tidak pernah dikirim. `RM-DEC-026` menetapkan `BE-16` sebagai pengaman berprioritas rendah dengan perilaku `409`. Dua fakta baru dicatat sebagai `RM-FACT-007` dan `RM-FACT-008`. |
| 4 | 24 Agustus 2026 | Amendment pass | `RM-DEC-024` ditutup: masa simpan jejak akses **25 tahun**. Tabel `TrxMedicalRecordAccessLog` dirancang terbagi per tahun berdasarkan `AccessedAt`, 25 bagian pada keadaan penuh. `BE-10` naik dari `TERTAHAN BLOCKER` menjadi `TERTAHAN APPROVAL`, sehingga **tidak ada lagi task berstatus `TERTAHAN BLOCKER`**. Risiko operasional baru dicatat: bagian tahun yang lupa dibuat akan menghentikan pembacaan rekam medis. |
| 5 | 26 Agustus 2026 | Amendment pass | **Kepemilikan modul ditetapkan.** Yoga Aji Pratama memegang ketiga peran owner sekaligus, dicatat pada `RM-DEC-027`. Dua puluh dua keputusan naik status dari `draft` menjadi `approved`. Empat pertanyaan terbuka yang sudah digantikan ditandai `superseded`. Seluruh gerbang pengesahan untuk pembangunan backend terbuka. Batas yang tetap berlaku: tinjauan komite medik dan pihak perlindungan data belum dilakukan. |
| 6 | 10 September 2026 | Amendment pass | **Sambungan Laboratorium ke berkas rekam medis diputuskan aturannya.** Dijalankan setelah capability audit lintas modul pada SHA `0e2eb105`/`0e5d3c36`. Enam pertanyaan dijawab menghasilkan `RM-DEC-030` sampai `RM-DEC-036`. **`RM-DEC-007` akhirnya tertutup** lewat `RM-DEC-035` setelah menggantung sejak 24 Agustus — pemicunya baru tiba ketika modul Laboratorium ada. Conflict `RL-04` (lab berpusat kunjungan, rekam medis berpusat pasien) ditutup `RM-DEC-031` tanpa mengubah satu kolom pun pada tabel laboratorium. Empat fakta audit dicatat sebagai `RM-FACT-009` sampai `RM-FACT-012`, dua di antaranya utang yang sebelumnya tidak terlihat: `RM-DEC-006` nol implementasi, dan layar penelusuran hanya membaca 13 dari 20 model `ClinicalManagement`. Acceptance criteria bertambah dari 17 menjadi 24. Tiga entri `RM-FE` baru. Empat blocker baru dicatat, tiga di antaranya milik blueprint Laboratorium. Dua pernyataan basi pada dokumen ini dikoreksi: blocker 1 dan 4 yang sudah tertutup sejak 26 Agustus, dan catatan penutup yang masih menyatakan nol keputusan `approved`. |
| 7 | 28 Oktober 2026 | Amendment pass | **Sambungan berkas rekam medis ke seluruh modul yang sudah ada, ditambah kelengkapan berkas.** Dipicu permintaan Yoga Aji Pratama "lengkapi modul rekam medis terhadap semua modul yang sudah tersedia saat ini". Dijalankan **tanpa** capability audit baru, hanya pembacaan source terarah pada SHA `363de34f`/`f0a3b07b8` (`RM-FACT-013` sampai `RM-FACT-018`). Sembilan belas pertanyaan dijawab menghasilkan `RM-DEC-037` sampai `RM-DEC-050`, seluruhnya `approved`. Aturan sambungan Laboratorium diangkat menjadi aturan seluruh modul (`RM-DEC-038`, `040`, `042`, `044`); cakupan nomor 4 kelengkapan berkas ditarik masuk dengan tiga syarat, master dokumen wajib, dan daftar kerja yang tidak menahan pelayanan (`RM-DEC-045` sampai `047`); `RM-DEC-006` akhirnya punya cara implementasi (`RM-DEC-043`). Satu celah ditemukan di tengah wawancara dan langsung ditutup: catatan `LockedUnsigned` tidak bisa ditandatangani lagi sehingga berkasnya mustahil lengkap — dituntaskan lewat addendum pengesahan (`RM-DEC-049`) dan batas tanggal rilis untuk data lama (`RM-DEC-050`). `MCU-GAP-04` ditutup dari sisi rekam medis. Acceptance criteria bertambah dari 24 menjadi 39; kriteria 8 diubah caranya. Lima entri `RM-FE` baru. Empat open question dan tiga blocker baru; blocker 5, 6, dan 8 diperbarui keadaannya. |
| 8 | 28 Oktober 2026 | Closure pass | **Delapan closure question capability map revision 3 dijawab**, menghasilkan `RM-DEC-051` sampai `RM-DEC-059`. Tiga conflict tertutup: waktu penutupan kunjungan (`RM-DEC-051`, tutup klinis), diagnosis utama (`RM-DEC-054`), dan persetujuan umum rawat inap (`RM-DEC-056`). Pemetaan dokumen wajib kini tuntas (`RM-DEC-053`–`056`). Satu temuan muncul **di tengah wawancara** saat klaim "hasil lab terrilis tidak dapat diubah" diperiksa ke source: benar untuk Patologi Klinik dan Mikrobiologi (`RM-FACT-019`), **tidak** untuk Patologi Anatomi (`RM-FACT-020`) — sehingga hasil PA ditahan (`RM-DEC-058`) sementara hasil lain tampil sekarang (`RM-DEC-057`). Tiga sumber modul lain memakai aturan final sementara (`RM-DEC-059`). Acceptance criteria bertambah dari 39 menjadi 51. Open question 13–15 dan blocker 5 serta 10 tertutup; **nol blocker desain tersisa**. |
| 9 | 28 Oktober 2026 | Closure pass | **`DEC-RM-001` ditutup.** Gerbang kelengkapan requirement (`RM-RCG-001` revision 1) menemukan bahwa pendaftaran langsung laboratorium membuat kunjungan Rawat Jalan, sehingga daftar wajib Rawat Jalan mustahil dipenuhi kunjungan penunjang. Dua pertanyaan dijawab menghasilkan `RM-DEC-060` (master per jenis kunjungan + jenis unit layanan) dan `RM-DEC-061` (surat rujukan tidak wajib). Satu fakta baru `RM-FACT-021`. Acceptance criteria bertambah dari 51 menjadi 55. Kelengkapan berkas (S-B1, S-B2) kini tanpa keputusan pemblokir. |
| 10 | 28 Oktober 2026 | Amendment pass (pengesahan) | Blueprint revision 4 dan kontrak `0.2.0` disahkan (`RM-DEC-062`); pembaca di modul Laboratorium dan MCU disetujui pemiliknya (`RM-DEC-063`). Persetujuan Radiologi menyusul hari yang sama (`RM-DEC-064`). Tujuh pemilik modul sumber lain belum memberi persetujuan. |
