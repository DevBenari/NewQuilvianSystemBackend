# Rekam Medis — Requirement Completeness Assessment (Amandemen 28 Oktober 2026)

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Blueprint revision | `3` (arsitektur belum direvisi untuk amandemen) |
| Assessment ID | `RM-RCG-001` revision `2` |
| Tanggal | 28 Oktober 2026 |
| Input keputusan | `00-interview-decisions.md` revision `9`, `sha256:1d788ecd…` — `RM-DEC-037` sampai `RM-DEC-061` (revision 1 penilaian ini memakai revision `8`) |
| Input capability map | `01-existing-capability-map.md` revision `3`, `sha256:b840e22b…` — `RM-CAP-034` sampai `RM-CAP-070` |
| Source SHA | Backend `363de34f` (branch `yoga`); frontend `f0a3b07b8` (branch `YogaV2`) |
| Pemilik keputusan | Yoga Aji Pratama — product/domain, clinical governance, security/privacy rekam medis (`RM-DEC-027`) |
| Sifat | Read-only terhadap source aplikasi. Tidak membuat entity, ERD, endpoint, migration, UI, atau task |

> **Kesimpulan singkat.** Sambungan berkas ke modul lain (**S-A**) **siap** masuk arsitektur
> domain, kecuali hasil Patologi Anatomi yang memang sudah ditahan. Kelengkapan berkas (**S-B**)
> **sebagian siap**: satu keputusan pemblokir baru ditemukan gerbang ini — kunjungan penunjang
> langsung (pasien yang datang hanya untuk lab, radiologi, atau hemodialisa) tercatat sebagai
> kunjungan Rawat Jalan, sehingga daftar dokumen wajib Rawat Jalan akan selalu menyatakannya
> belum lengkap (`DEC-RM-001`).
>
> **Revision 2 (28 Oktober 2026).** `DEC-RM-001` **ditutup** oleh `RM-DEC-060` dan `RM-DEC-061`:
> master dokumen wajib dikunci per jenis kunjungan dan jenis unit layanan. S-B1 dan S-B2 naik
> menjadi `READY_FOR_DOMAIN_DESIGN`. **Kesiapan keseluruhan kini: siap, kecuali S-A1b**
> (hasil Patologi Anatomi). Bagian 3–7 di bawah adalah penilaian revision 1; keadaan terkininya
> pada bagian 9.

---

## 1. Scope penilaian

Modul `rekam-medis`, cakupan amandemen `RM-DEC-037`. Dinilai sebagai sebelas slice supaya satu
keputusan yang terbuka tidak menahan slice lain.

| Slice | Kemampuan | Keputusan acuan |
|---|---|---|
| **S-A1** | Hasil laboratorium Patologi Klinik dan Mikrobiologi di berkas | `RM-DEC-030`–`035`, `038`, `057` |
| **S-A1b** | Hasil laboratorium Patologi Anatomi di berkas | `RM-DEC-058` |
| **S-A2** | Laporan radiologi di berkas | `RM-DEC-038`, `041`, `042` |
| **S-A3** | Dokumen modul pelayanan: IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah | `RM-DEC-039` kelompok B, `RM-DEC-059` |
| **S-A4** | Farmasi dan model `ClinicalManagement` baru, termasuk ringkasan pencatatan berkala | `RM-DEC-039` kelompok C, `RM-DEC-052` |
| **S-A5** | Hasil dan resume MCU | `RM-DEC-039` kelompok D, `RM-DEC-040` |
| **S-A6** | Penanda `Susulan` dan "pernah diralat" lintas jenis | `RM-DEC-042`, `043`, `051` |
| **S-A7** | Perbaikan kekosongan diam-diam jenis 14 dan 15, dan pemberitahuan cakupan yang basi | `RM-CAP-035`, `041` |
| **S-B1** | Master dokumen wajib | `RM-DEC-045`, `047`, `053`–`056` |
| **S-B2** | Penilaian kelengkapan per kunjungan dan daftar kerja | `RM-DEC-045`, `046`, `050`, `051` |
| **S-B3** | Addendum pengesahan atas catatan `LockedUnsigned` | `RM-DEC-049` |

---

## 2. Bukti yang dipakai

| Wewenang | Bukti | Menjawab |
|---|---|---|
| 1 — requirement eksplisit pemilik | Jawaban Yoga Aji Pratama pada decision log revision 7 dan 8 (dua puluh delapan pertanyaan pilihan, 28 Oktober 2026) | Apa yang seharusnya dibangun |
| 2 — SOP rumah sakit | **Tidak ada.** SOP rekam medis belum pernah diserahkan (blocker 3 decision log) | — |
| 5 — baseline rumah sakit Indonesia | **Tidak dipanggil** pada penilaian ini. Pertimbangan indikator mutu kelengkapan rekam medis dicatat sebagai `PROPOSED` dengan tanda verifikasi regulasi | — |
| 6 — implementasi V2 | Capability map revision 3, ditambah tiga pemeriksaan source baru pada gerbang ini (bagian 3) | Apa yang ada saat ini |

### Fakta source baru dari gerbang ini

| ID | Fakta | Bukti |
|---|---|---|
| `RM-RCG-F01` | Pasien IGD yang dirawat inap mendapat **kunjungan rawat inap baru**: kunjungan IGD ditutup `Completed` saat disposisi (`EmergencyDispositionService.AdmissionRequest.cs:1585-1598`), lalu `InpEpisodeService` membuat `RegPatientEncounter` baru berjenis `Inpatient` bila kunjungan yang ada bukan rawat inap (`InpEpisodeService.cs:335`, `1606-1616`) | SHA `363de34f` |
| `RM-RCG-F02` | **Pendaftaran langsung Laboratorium membuat kunjungan `Outpatient`.** `LabPatientRegistrationService` meneruskan `EncounterIntakeRequest` tanpa mengisi `EncounterType` (`LabPatientRegistrationService.cs:271-285`), dan nilai bawaannya `Outpatient` (`EncounterIntakeDtos.cs:33`) | SHA `363de34f` |
| `RM-RCG-F03` | Sesi hemodialisa menyimpan `EncounterId` **nullable** (`HmdSession.EncounterId`), di samping `InpEpisodeId`. Jenis kunjungan untuk sesi hemodialisa rawat jalan tidak ditelusuri | SHA `363de34f` |

---

## 3. Temuan per dimensi

Keterangan status: **C** = `CONFIRMED`, **P** = `PROPOSED`, **M** = `MISSING`, **X** = `CONFLICT`,
**—** = tidak berlaku secara material (alasan pada catatan).

| No | Dimensi | S-A1 | S-A1b | S-A2 | S-A3 | S-A4 | S-A5 | S-A6 | S-A7 | S-B1 | S-B2 | S-B3 |
|---:|---|---|---|---|---|---|---|---|---|---|---|---|
| 01 | Tujuan | C | C | C | C | C | C | C | C | C | C | C |
| 02 | Aktor | C | C | C | C | C | C | C | C | P | C | C |
| 03 | Pemicu / prasyarat | C | X | C | C | C | C | C | C | C | **X** | C |
| 04 | Alur utama | C | C | C | C | C | C | C | C | C | C | C |
| 05 | Alternatif / exception | C | C | C | C | C | M | C | C | M | **X** | C |
| 06 | Data minimum | C | C | C | P | P | C | C | C | C | C | C |
| 07 | Aturan / validation | C | C | C | P | C | C | C | C | C | **X** | C |
| 08 | Status / perubahan | C | C | C | P | C | C | C | — | C | C | C |
| 09 | Peran / authorization | C | C | C | C | C | C | C | C | P | P | C |
| 10 | Dependency antarmodul | C | C | C | C | C | C | C | C | C | C | C |
| 11 | Integrasi | — | — | — | — | — | — | — | — | — | — | — |
| 12 | Hasil akhir | C | C | C | C | C | C | C | C | C | C | C |
| 13 | Pembatalan / koreksi | C | X | C | C | C | C | C | — | P | C | C |
| 14 | Audit / histori | C | C | C | C | C | C | C | — | P | C | C |
| 15 | Notifikasi | — | — | — | — | — | — | — | — | — | C | — |
| 16 | Billing | — | — | — | — | — | — | — | — | — | C | — |
| 17 | Keselamatan klinis | C | C | C | C | C | C | C | C | C | C | C |
| 18 | Pelaporan | — | — | — | — | — | — | — | — | — | P | — |

### Catatan per sel yang bukan `CONFIRMED`

| Slice / dimensi | Status | Isi | Dampak |
|---|---|---|---|
| S-A1b / 03, 13 | `CONFLICT` | Laporan PA dapat dibuka kembali tanpa pemeriksaan rilis (`RM-FACT-020`), bertentangan dengan `RM-DEC-030`–`032` yang mengandaikan hasil terrilis tidak berubah diam-diam | `BLOCKING` untuk **S-A1b saja**. Sudah ditangani `RM-DEC-058` (ditahan); penutupannya milik blueprint Laboratorium (open question 18) |
| S-A3 / 06, 07, 08 | `PROPOSED` | Arti final disposisi IGD, catatan asuhan gizi, dan catatan kelahiran adalah aturan sementara rekam medis (`RM-DEC-059`) yang belum dikonfirmasi pemilik modulnya | `NON_BLOCKING_STANDARD`. Bila pemilik menetapkan lain, yang berubah hanya kriteria satu sumber — bukan struktur domain |
| S-A4 / 06 | `PROPOSED` | Model `ClinicalManagement` mana yang "dokumen" dan mana yang "pencatatan berkala" baru disusun capability map (`RM-CAP-059`–`067`), belum disahkan per model | `NON_BLOCKING_STANDARD` — penggolongan ditetapkan saat arsitektur, mengikuti `RM-DEC-052` |
| S-A5 / 05 | `MISSING` | Peserta MCU tanpa nomor rekam medis (`McuOrder.PatientId` kosong) tidak punya berkas. Tidak ada keputusan apakah hasilnya menyusul masuk berkas ketika pasien kelak didaftarkan | `NON_BLOCKING_STANDARD`. **Usulan:** tidak masuk berkas sampai MCU menautkan pesertanya ke pasien; urusan penautan milik MCU (`DEC-MCU-004`) |
| S-B1 / 02, 09 | `PROPOSED` | Siapa yang boleh mengubah master dokumen wajib belum disebut; `RM-DEC-045` hanya menyebut "unit rekam medis" | `CONFIGURABLE_DEFAULT` — izin baru diberikan lewat kebijakan akses per departemen-posisi (`SysAccessPolicies`), seperti master keperluan akses |
| S-B1 / 05 | `MISSING` | Kunjungan berjenis `Unknown` (`EncounterType = 0`) tidak punya baris di master | `NON_BLOCKING_STANDARD`. **Usulan:** tidak dinilai, tampil di daftar kerja sebagai "jenis kunjungan tidak dikenal" supaya datanya dibetulkan, bukan dianggap lengkap |
| S-B1 / 13, 14 | `PROPOSED` | Perubahan isi master (siapa, kapan, sebelum-sesudah) belum diputuskan wajib tercatat | `NON_BLOCKING_STANDARD`. **Usulan:** wajib tercatat, karena perubahan master mengubah hasil penilaian ribuan kunjungan sekaligus (`AC-33`) |
| **S-B2 / 03, 05, 07** | **`CONFLICT`** | **Kunjungan penunjang langsung.** `RM-DEC-047` menuntut asesmen, diagnosis utama, dan CPPT/SOAP dokter untuk **setiap** kunjungan Rawat Jalan. Source membuat kunjungan `Outpatient` untuk pasien yang datang hanya ke laboratorium (`RM-RCG-F02`), dan kemungkinan juga radiologi serta hemodialisa rawat jalan (`RM-RCG-F03`, belum ditelusuri). Pada kunjungan seperti itu tidak ada dokter yang mengasesmen atau mendiagnosis, sehingga berkasnya **mustahil lengkap** — persis keadaan yang dihindari `RM-DEC-047` ("tidak ada syarat yang mustahil dipenuhi") | **`BLOCKING`** untuk S-B2 dan bentuk master S-B1: jawabannya menentukan apakah master cukup dikunci per jenis kunjungan atau perlu pembeda lain (unit layanan, jalur pendaftaran). → **`DEC-RM-001`** |
| S-B2 / 09 | `PROPOSED` | Siapa yang boleh melihat daftar kerja belum disebut; `RM-DEC-046` menyebut "petugas rekam medis" | `CONFIGURABLE_DEFAULT`. Daftar kerja memuat nama pasien dan nama penulis, sehingga perlu izin sendiri, bukan cukup `MedicalRecord : Read` |
| S-B2 / 15 | `CONFIRMED` tidak ada | `RM-DEC-046`: petugas menindaklanjuti di luar sistem; tidak ada pemberitahuan otomatis | — |
| S-B2 / 16 | `CONFIRMED` tidak ada | `RM-DEC-046`: tidak menahan tagihan | — |
| S-B2 / 18 | `PROPOSED` | Indikator mutu persentase berkas lengkap dalam jangka waktu tertentu sesudah pelayanan lazim dipantau unit rekam medis. Belum diminta pemilik; tenggat sudah ditolak sebagai penahan (`RM-DEC-046`) | `NON_BLOCKING_STANDARD`. Penilaian yang dihitung dari waktu final dokumen tetap memungkinkan laporan ini disusun belakangan tanpa mengubah struktur. Dasar regulasi angka tenggat: `VERIFY_CURRENT_REGULATION`, belum diverifikasi |
| Semua / 11 | — | Tidak ada integrasi eksternal. Sambungan antarmodul tercakup dimensi 10. SATUSEHAT di luar scope (`B7`) | — |
| S-A / 15, 16 | — | Membaca dokumen tidak memicu pemberitahuan maupun tagihan | — |

### Keselamatan klinis (dimensi 17) — dicatat terpisah karena paling menentukan

| Risiko | Penanganan | Status |
|---|---|---|
| Hasil lab dan sesi hemodialisa tampil kosong tanpa peringatan **hari ini** | S-A7 dan `RM-DEC-057` | `CONFIRMED` — wajib diperbaiki, tidak menunggu siapa pun |
| Laporan PA berubah diam-diam sesudah dibaca | Ditahan `RM-DEC-058` | `CONFIRMED` |
| Hasil lab diralat tanpa penanda setelah `S6` dibangun | Syarat penyerta `RM-DEC-057` mengikat Laboratorium | `CONFIRMED` |
| Satu modul gangguan dibaca sebagai "tidak ada dokumen" | `RM-DEC-044`, fondasinya sudah ada (`RM-CAP-034`) | `CONFIRMED` |
| Dokumen konsep dibaca sebagai hasil resmi | `RM-DEC-041` | `CONFIRMED` |
| Berkas kunjungan penunjang langsung selalu "belum lengkap", sehingga daftar kerja tidak lagi dipercaya | `DEC-RM-001` | `CONFLICT` — `BLOCKING` S-B2 |

---

## 4. Decision Log gerbang ini

### `DEC-RM-001` — Dokumen wajib untuk kunjungan penunjang langsung

| Field | Isi |
|---|---|
| Pertanyaan | Pasien yang datang **hanya** untuk pemeriksaan laboratorium atau radiologi (rujukan luar, tanpa dokter rumah sakit), dan pasien hemodialisa rawat jalan, tercatat sebagai kunjungan Rawat Jalan. Dokumen wajib apa yang berlaku bagi kunjungan seperti itu? |
| Kemampuan terdampak | S-B1 master dokumen wajib (bentuk pembeda), S-B2 penilaian dan daftar kerja |
| Bukti saat ini | `RM-RCG-F02` (lab langsung = `Outpatient`); `RM-RCG-F03` (hemodialisa); `RM-DEC-047` menyusun daftar per `EncounterType` tanpa pembeda unit layanan |
| Contoh | Ibu Rina membawa surat rujukan dari klinik luar untuk cek gula darah. Ia didaftarkan di loket laboratorium, diambil darahnya, hasil dirilis, pulang. Tidak ada asesmen, diagnosis, atau SOAP dokter rumah sakit — dan memang tidak seharusnya ada. Dengan aturan sekarang, kunjungannya tampil `Belum lengkap` selamanya |
| Usulan baseline | Kunjungan penunjang langsung dinilai dengan daftar wajibnya sendiri: hasil pemeriksaan yang dipesan sudah dirilis (sudah tercakup syarat 2), ditambah surat rujukan bila pembayarannya lewat rujukan. Dibedakan dari Rawat Jalan biasa berdasarkan **unit layanan** kunjungan |
| Dampak | Bentuk master dokumen wajib (cukup per jenis kunjungan, atau per jenis kunjungan + unit layanan); kepercayaan petugas pada daftar kerja |
| Pemilik | Yoga Aji Pratama — clinical governance rekam medis |
| Status | ~~`OPEN`~~ → **`CLOSED`** 28 Oktober 2026 oleh `RM-DEC-060` (pembeda = jenis kunjungan + `MstServiceUnit.ServiceUnitType`; unit Laboratorium/Radiologi tanpa dokumen dokter, unit Hemodialisa cukup catatan sesi bertanda tangan) dan `RM-DEC-061` (surat rujukan tidak wajib) — disahkan Yoga Aji Pratama |
| Dampak domain | ~~S-B1 dan S-B2 berhenti sebelum arsitektur.~~ Tidak lagi menahan. Arsitektur wajib memodelkan master dokumen wajib dengan kunci **jenis kunjungan + jenis unit layanan**, ditambah syarat bersyarat `RM-DEC-047` |

---

## 5. Kesiapan per slice

| Slice | Kesiapan | Alasan |
|---|---|---|
| S-A1 Lab PK/Mikro | **`READY_FOR_DOMAIN_DESIGN`** | Keputusan lengkap. Blocker 7 (jalur baca lab per pasien) menahan **implementasi**, bukan desain — justru desain yang akan menetapkannya |
| S-A1b Lab PA | **`BUSINESS_DECISION_REQUIRED`** | Open question 18 (`RM-FACT-020`), milik blueprint Laboratorium. Desain cukup mencantumkan keadaan "belum tersedia di berkas" |
| S-A2 Radiologi | **`READY_FOR_DOMAIN_DESIGN`** | Jalur baca sudah ada (`RM-CAP-058`) |
| S-A3 Pelayanan | **`READY_FOR_DOMAIN_DESIGN`** | Tiga aturan final sementara bersifat `NON_BLOCKING_STANDARD` |
| S-A4 Farmasi + `ClinicalManagement` | **`READY_FOR_DOMAIN_DESIGN`** | Penggolongan model diputuskan saat arsitektur mengikuti `RM-DEC-052` |
| S-A5 MCU | **`READY_FOR_DOMAIN_DESIGN`** | Peserta tanpa nomor rekam medis memakai usulan non-blocking |
| S-A6 Susulan + pernah diralat | **`READY_FOR_DOMAIN_DESIGN`** | `RM-DEC-051` menutup conflict waktu penutupan |
| S-A7 Perbaikan kekosongan diam-diam | **`READY_FOR_DOMAIN_DESIGN`** | Perbaikan murni, aturannya sudah ada sejak `RM-DEC-030` |
| S-B1 Master dokumen wajib | **`PARTIALLY_READY`** | Isi dan aturan butir sudah lengkap (`RM-DEC-047`, `053`–`056`); **bentuk pembedanya** menunggu `DEC-RM-001` |
| S-B2 Penilaian + daftar kerja | **`BUSINESS_DECISION_REQUIRED`** | `DEC-RM-001` |
| S-B3 Addendum pengesahan | **`READY_FOR_DOMAIN_DESIGN`** | `RM-DEC-049` lengkap; tidak bergantung pada bentuk master |

**Kesiapan keseluruhan amandemen: `PARTIALLY_READY`.**

---

## 6. Yang boleh berjalan dan yang harus berhenti

### Boleh berjalan ke `hospital-domain-architect`

S-A1, S-A2, S-A3, S-A4, S-A5, S-A6, S-A7, dan S-B3. Seluruhnya **independen** dari `DEC-RM-001`:
sambungan berkas tidak bergantung pada aturan kelengkapan, dan addendum pengesahan hanya
mengubah status satu kekurangan, bukan cara kekurangan itu dihitung.

**Dependency yang wajib dibawa ke arsitektur:**

| Dari | Ke | Sifat |
|---|---|---|
| S-A6 (waktu final per sumber) | S-B2 | Penilaian syarat 2 dan penanda `Susulan` memakai kolom waktu yang sama; arsitektur S-A wajib menyediakannya |
| S-A1–S-A5 (sumber terbaca) | S-B2 | Keadaan `Tidak dapat dipastikan` lahir dari gangguan sumber S-A |
| S-B3 | S-B2 | Syarat 1 menghitung addendum pengesahan sebagai penuntas |

### Harus berhenti

| Slice | Menunggu | Pemilik |
|---|---|---|
| S-B1 (bentuk pembeda master), S-B2 | `DEC-RM-001` | Yoga Aji Pratama |
| S-A1b | Open question 18 | Blueprint Laboratorium — Yoga Aji Pratama selaku pemilik modul Laboratorium |

---

## 7. Keputusan pemilik yang dibutuhkan

| ID | Keputusan | Menahan |
|---|---|---|
| `DEC-RM-001` | Dokumen wajib kunjungan penunjang langsung, dan pembeda yang dipakai | Desain S-B1, S-B2 |
| Open question 18 | Buka-ulang laporan PA sesudah rilis: celah atau disengaja | Desain S-A1b |
| Open question 17 | Konfirmasi arti final tiga sumber oleh pemilik IGD, Gizi, Kamar Operasi | Tidak menahan |
| — | Izin daftar kerja dan master dokumen wajib | Tidak menahan (`CONFIGURABLE_DEFAULT`) |

---

## 8. Langkah berikutnya

1. **`/grill-me`** lanjutan untuk `DEC-RM-001` — satu pertanyaan, dapat dijawab langsung
   pemilik. Bila terjawab, S-B1 dan S-B2 naik menjadi `READY_FOR_DOMAIN_DESIGN` dan seluruh
   amandemen (kecuali S-A1b) dapat masuk arsitektur sekaligus.
2. **`/hospital-domain-architect`** untuk slice yang siap. Boleh dijalankan sekarang untuk
   S-A dan S-B3, atau setelah langkah 1 supaya S-B ikut dirancang dalam satu putaran.
3. Open question 18 diteruskan ke blueprint Laboratorium.

## 9. Keadaan revision 2

| Slice | Revision 1 | Revision 2 | Alasan |
|---|---|---|---|
| S-A1, S-A2, S-A3, S-A4, S-A5, S-A6, S-A7, S-B3 | `READY_FOR_DOMAIN_DESIGN` | `READY_FOR_DOMAIN_DESIGN` | Tidak berubah |
| S-A1b Lab PA | `BUSINESS_DECISION_REQUIRED` | `BUSINESS_DECISION_REQUIRED` | Open question 18 tetap terbuka. Desain cukup memuat keadaan "belum tersedia di berkas" (`RM-DEC-058`) |
| S-B1 Master dokumen wajib | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`** | `RM-DEC-060` menetapkan kunci master; `RM-DEC-061` menetapkan rujukan bukan butir wajib |
| S-B2 Penilaian + daftar kerja | `BUSINESS_DECISION_REQUIRED` | **`READY_FOR_DOMAIN_DESIGN`** | `DEC-RM-001` tertutup |

**Kesiapan keseluruhan: `PARTIALLY_READY`** — sepuluh dari sebelas slice siap. Satu-satunya
yang berhenti adalah S-A1b, dan ia **independen**: slice lain tidak bergantung pada hasil PA.

**Butir non-blocking yang tetap dibawa ke arsitektur**, tanpa disajikan sebagai kebijakan
rumah sakit yang terkonfirmasi:

| Butir | Klasifikasi |
|---|---|
| Aturan final sementara disposisi IGD, asuhan gizi, catatan kelahiran (`RM-DEC-059`) | `NON_BLOCKING_STANDARD` |
| Izin daftar kerja dan master dokumen wajib | `CONFIGURABLE_DEFAULT` |
| Jejak perubahan isi master | `NON_BLOCKING_STANDARD` (usulan: wajib tercatat) |
| Peserta MCU tanpa nomor rekam medis | `NON_BLOCKING_STANDARD` (usulan: tidak masuk berkas) |
| Indikator mutu kelengkapan | `NON_BLOCKING_STANDARD`, `VERIFY_CURRENT_REGULATION` |

**Handoff berikutnya:** `hospital-domain-architect` untuk sepuluh slice siap.

## Riwayat

| Revision | Tanggal | Ringkasan |
|---:|---|---|
| 2 | 28 Oktober 2026 | `DEC-RM-001` ditutup (`RM-DEC-060`, `RM-DEC-061`). S-B1 dan S-B2 naik menjadi `READY_FOR_DOMAIN_DESIGN`. Sepuluh dari sebelas slice siap; S-A1b tetap menunggu open question 18 |
| 1 | 28 Oktober 2026 | Penilaian pertama atas amandemen revision 7–8. Sebelas slice: delapan `READY_FOR_DOMAIN_DESIGN`, satu `PARTIALLY_READY`, dua `BUSINESS_DECISION_REQUIRED`. Satu keputusan pemblokir baru, `DEC-RM-001`, ditemukan dari pemeriksaan source (`RM-RCG-F02`): pendaftaran langsung laboratorium membuat kunjungan Rawat Jalan |
