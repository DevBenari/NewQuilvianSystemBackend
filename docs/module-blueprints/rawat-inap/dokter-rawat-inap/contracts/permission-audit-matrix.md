# Permission dan Audit Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | **`0.6.0`** |
| `last_changed_in` | **`0.6.0`** — bagian 6 s.d. 9 lahir: sembilan butir hak akses baru, peta peran perawat dan dokter diperbarui, sebelas kewenangan per pasien. Sebelumnya `0.5.0` bagian 3A |
| Status | **`draft`** untuk `0.6.0`. `0.5.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`) |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-09** untuk `0.4.0`; `0.3.0` disetujui 2026-09-03 |
| `input_revision` | `api-contract.md` `0.4.0`; arsitektur domain `0.2` bagian V dan W |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Compatibility impact | `0.4.0`: Resource `PatientDiagnosis` masuk peta peran, satu batas kewenangan per pasien ditambahkan pada bagian 3, dan tujuh kolom sensitif didaftarkan pada bagian 5. **Nol Resource baru dan nol Action baru** — seluruhnya sudah ada di source. Sebelumnya `0.3.0` memasukkan `ClinicalNoteAuthorDelegation` dan `CreateAsSubstitute` |
| Tanggal | 2 September 2026; diamendemen 9 September 2026 |

---

## 0. Yang tidak ada di dokumen ini

Dokumen ini **MUST NOT** memuat tabel seluruh endpoint. Pemetaan endpoint ke hak akses dipegang
kolom `Hak akses` pada [`api-contract.md`](./api-contract.md). Dua turunan berikut **dihitung**:

| Yang tidak ditulis ulang | Cara menurunkannya |
| --- | --- |
| String atribut | `[AccessPermission("<Resource>", "<Action>")]` disalin dari kolom `Hak akses` |
| Status pencatatan logger | `GET` tidak dicatat; selain `GET` dicatat |

**Pengecualian bernama:** tidak ada.

---

## 1. Cara kerja hak akses di repository ini

| Hal | Isinya |
| --- | --- |
| Atribut | `[AccessPermission("Resource", "Action")]` beserta `[AccessAction(...)]` pada setiap endpoint, dan `[AccessController]` pada kelasnya |
| Filter | `AccessPermissionFilter` mencocokkan pasangan Resource–Action terhadap baris hak akses peran |
| Jebakan yang sudah terjadi | `BE-RWI-034`: `[AccessAction]` dan `[AccessPermission]` menyebut nama berbeda, sehingga sembilan endpoint menjawab `403` bagi siapa pun kecuali SuperAdmin, dan menahan tujuh task frontend |

### 1.1 Yang ditambahkan sub-modul ini

| Tambahan | Bentuknya | Catatan |
| --- | --- | --- |
| Resource baru | `PhysicianVisit` | Dengan Action `Read`, `Create`, `Update`, dan **`Cancel`** |
| Action baru pada Resource yang sudah ada | `Verify` pada `PatientIntegratedProgressNote` | Hanya untuk DPJP |
| **Tidak ditambahkan** | Action `Amend` | Koreksi memakai `ClinicalNoteAddendum : Create` yang **sudah ada** |
| Butir yang dipakai dari modul lain | `ClinicalNoteAddendum : CreateAsSubstitute` | Koreksi atas nama dokter yang berhalangan. **Sudah ada** di source |
| Butir yang dipakai dari modul lain | `ClinicalNoteAuthorDelegation : Create`, `Read`, `Update` | Menerbitkan, membaca, dan mencabut penetapan berhalangan. **Sudah ada** di source |
| Butir yang dipakai dari modul lain ★ `0.4.0` | `PatientDiagnosis : Read`, `Create`, `Update` | Daftar masalah terstruktur pada kajian medis. **Sudah ada** di source beserta ketiga Action-nya; sub-modul ini **tidak** menambah Action apa pun. Pembatalan diagnosis memakai `Update`, bukan Action `Cancel` tersendiri |

> Ketiganya wajib memakai nama yang sama persis pada `[AccessAction]` dan `[AccessPermission]`, dan
> wajib diuji dengan peran non-SuperAdmin. Ini pelajaran langsung dari `BE-RWI-034`.

---

## 2. Peta peran ke butir hak akses

| Peran rumah sakit | Resource | Action |
| --- | --- | --- |
| DPJP | `DoctorConsultation` | `Read`, `Create`, `Update` |
| DPJP | `PatientAssessment` | `Read`, `Create`, `Update` — **terbatas jenis medis**, lihat bagian 3 |
| DPJP | `PatientIntegratedProgressNote` | `Read`, `Create`, `Update`, **`Verify`** |
| DPJP | `PatientProcedure` | `Read`, `Create`, `Update` |
| DPJP | **`PatientDiagnosis`** | `Read`, `Create`, `Update` — terbatas pasien yang menjadi tanggung jawabnya, lihat bagian 3 |
| DPJP | `Prescription` | `Read`, `Create` |
| DPJP | `LabOrder` | `Read`, `Create` |
| DPJP | `RadOrder` | `Read`, `Create` |
| DPJP | `PhysicianVisit` | `Read`, `Create`, `Update`, `Cancel` |
| DPJP | `ClinicalNoteAddendum` | `Read`, `Create`, **`CreateAsSubstitute`** |
| **Kepala unit rawat inap** | `ClinicalNoteAuthorDelegation` | `Create`, `Read`, `Update` — menerbitkan dan mencabut penetapan berhalangan |
| Dokter jaga ruangan | Sama dengan DPJP **kecuali** `Verify` | — |
| Dokter konsulen | `Read` pada seluruhnya; `Create` pada CPPT dan `PhysicianVisit` | — |
| Perawat | `Read` pada catatan dokter, kajian medis, tindakan, resep, lab, dan radiologi; `Create` pada CPPT | Tidak ada `Verify`; addendum hanya pada catatannya sendiri |
| Perawat | `PatientDiagnosis` | **`Read` saja.** Daftar masalah dibaca untuk asuhan, tidak ditulis — menegakkan diagnosis adalah kewenangan dokter |
| Ahli gizi | `Read` pada kajian medis dan CPPT, termasuk `PatientDiagnosis` | Daftar masalah menentukan bentuk diet |
| Supervisor klinis | `Read` pada seluruhnya; `Cancel` pada `PhysicianVisit` | Untuk membatalkan event salah catat bila dokternya berhalangan |
| Dokter jaga dan konsulen | `ClinicalNoteAddendum : Create` pada catatannya sendiri | **Tanpa** `CreateAsSubstitute`; koreksi atas nama dokter lain hanya milik DPJP aktif |
| Petugas Farmasi, Laboratorium, Radiologi | Milik modulnya sendiri | Sub-modul ini tidak mengaturnya |
| Petugas admisi, kasir | — | **Tidak ada** |

> **`Verify` hanya milik DPJP, dan itu inti `CAP-021` aturan 5.** Memberikannya kepada dokter jaga
> membuat verifikasi kehilangan artinya: yang diverifikasi adalah catatan yang menjadi tanggung
> jawab DPJP.
>
> **`Cancel` pada visite diberikan juga kepada supervisor klinis**, karena event salah catat bisa
> ditemukan setelah dokternya pulang. Yang tidak diberikan kepada siapa pun adalah penyuntingan
> waktu visite — jalur itu memang tidak ada.

---

## 3. Kewenangan yang tidak dapat dijaga mesin hak akses

Mesin hak akses tahu peran, tidak tahu pasien maupun jenis dokumen. **Tujuh** hal berikut dijaga di
tingkat aturan bisnis, sesuai arsitektur domain bagian V.2.

| Yang dijaga | Penjaganya | Yang **tidak** dijaganya | Risikonya |
| --- | --- | --- | --- |
| Dokter hanya menulis untuk pasien yang menjadi tanggung jawabnya — `INV-DOK-13` | `VAL-DOK-06`, memakai pemeriksaan dokter aktif per episode yang **sudah ada** di layanan episode | Dokter yang memang berwenang tetap dapat menulis apa saja | Kesalahan isi, bukan kewenangan. Dijaga jejak audit |
| **Dokter menulis kajian medis, perawat menulis pengkajian keperawatan** | `VAL-DOK-05`, bercabang menurut jenis kajian | Mesin hak akses melihat **satu** Resource untuk keduanya | **Ini akibat langsung berbagi satu tabel.** Bila pemilik memilih bentuk penyimpanan terpisah, penjagaan ini naik ke mesin hak akses |
| Verifikator adalah DPJP yang aktif saat verifikasi, dan bukan penulis asli — `INV-DOK-11` | `VAL-DOK-07` beserta kolom verifikator yang terpisah | Mesin hak akses hanya tahu peran DPJP secara umum | Verifikasi oleh DPJP episode lain |
| Visite hanya dicatat dokter | `VAL-DOK-08` | Kebijakan pencatatan atas nama dokter belum ada | Bawaan dipilih yang aman: hanya dokter |
| Dokumen dan hasil yang dibaca milik episode yang sedang dibuka — `INV-DOK-12` | `VAL-DOK-26`, `VAL-DOK-31` | Mesin hak akses tidak mengenal episode | **Risiko tertinggi pada scope ini**: membaca hasil pasien lain |
| **Diagnosis dari kajian medis hanya ditulis yang berwenang menulis kajian medis pasien itu** — `CAP-022`, `BE-RWI-068` kriteria 5 ★ `0.4.0` | `VAL-DOK-39`, memakai pemeriksaan dokter aktif per episode yang **sama** dengan `VAL-DOK-06` | Mesin hak akses hanya melihat Resource `PatientDiagnosis` dan **tidak tahu** apakah diagnosis itu lahir dari catatan dokter atau dari kajian medis, maupun pasien siapa | Diagnosis tertulis pada daftar masalah pasien yang bukan tanggung jawabnya. **Inilah harga membuka jalur kedua**: sebelum `0.4.0` jalur tulis hanya satu dan sudah dijaga kepemilikan konsultasi |
| **Koreksi atas nama dokter lain hanya oleh DPJP aktif episode itu** — `RWI-DEC-088` | `VAL-DOK-35`, memakai pemeriksaan dokter aktif per episode | **Penetapan berhalangan bersifat milik penulis**, bukan milik penggantinya — ia menyatakan "dokter ini berhalangan" tanpa menyebut siapa yang boleh menggantikan. Begitu penetapan berlaku, siapa pun pemegang butir pengganti dapat mengoreksi | **Blast radius lebih lebar dari yang diputuskan.** Tanpa penjaga tambahan, dokter mana pun yang memegang butir pengganti dapat mengoreksi catatan dokter yang berhalangan, termasuk pasien yang bukan tanggung jawabnya |

Baris kedua adalah harga yang dibayar jalan berbagi tabel pada `02-backend-architecture.md` bagian
4.2. Ia **ditulis di sini** supaya pemilik melihatnya sebelum menyetujui, bukan menemukannya saat
implementasi.

---


## 3A. Penulis klinis diambil dari pengguna terautentikasi — `0.5.0`

**Bagian baru 11 September 2026**, menyerap `RWI-DEC-099`. Ia melengkapi bagian 3, dan seperti
bagian itu, isinya adalah kewenangan yang **tidak dapat** dijaga mesin hak akses.

### 3A.1 Keadaan hari ini, dan kenapa ia berbahaya

`InpatientClinicalContextService.ResolveAsync` sudah memiliki pemeriksaan kewenangan yang benar.
Masalahnya, pemeriksaan itu **mati**: `isDoctorAuthorized` bernilai benar secara bawaan dan hanya
diuji ketika pemanggil mengirimkan dokter pelaku. Dari sembilan titik panggil, hanya satu yang
mengirimkannya.

Akibatnya dokter mana pun yang memegang butir hak akses `PatientIntegratedProgressNote : Create`
dapat menulis catatan untuk pasien mana pun di rumah sakit, dan penulis yang tersimpan diambil
dari antrean, payload, atau kunjungan, bukan dari siapa yang sedang login.

Ini bukan kelemahan mesin hak akses. Butir hak akses memang hanya menjawab "peran ini boleh
memanggil endpoint ini", dan tidak pernah dapat menjawab "dokter ini boleh menulis untuk pasien
ini". Jawabannya harus ditulis di dalam service.

### 3A.2 Tiga penjaga yang mengikat sub-modul ini

Ketiganya dimiliki bersama dengan `episode-rawat-inap`, dan definisinya dipegang
[`../episode-rawat-inap/contracts/permission-audit-matrix.md`](../../episode-rawat-inap/contracts/permission-audit-matrix.md)
bagian 4-A.3 supaya tidak ada dua salinan yang dapat berbeda isi.

| Penjaga | Yang wajib dipenuhi setiap jalur tulis sub-modul ini |
| --- | --- |
| `GUARD-INP-05` | Penulis diambil dari `ApplicationUser.DoctorId`. Tanpa pemetaan aktif, penulisan ditolak `403` |
| `GUARD-INP-06` | Kewenangan dinilai pada **waktu klinis** dokumen, bukan waktu penyimpanan |
| `GUARD-INP-07` | Berlaku bagi perawat; disebut di sini hanya karena `PatientAssessmentController` dipakai dua profesi |

### 3A.3 Jalur tulis yang wajib memanggil ketiganya

| Grup endpoint | Jalur tulis | Keadaan hari ini |
| --- | --- | --- |
| Doctor Consultation | `POST /`, pembaruan, finalisasi | **Tidak** mengirim dokter pelaku |
| Patient Assessment | Tiga jalur tulis | **Tidak** mengirim dokter pelaku |
| Patient Integrated Progress Note | `POST /`, `PATCH /{id}/verify`, `PATCH /{id}/cancel` | **Tidak** mengirim dokter pelaku |
| Patient Diagnosis | Dua jalur tulis | **Tidak** mengirim dokter pelaku |
| Patient Procedure | Satu jalur tulis | **Tidak** mengirim dokter pelaku |
| Physician Visit | `POST /` | **Sudah** mengirim dokter pelaku. Satu-satunya yang benar hari ini, dan menjadi pola bagi yang lain |

### 3A.4 Verifikasi CPPT — satu hal yang tidak berubah

Verifikasi CPPT **tidak** mengubah penulis aslinya, dan penegakan penulis ini tidak mengubah aturan
itu. Yang berubah hanya bahwa **verifikator** juga wajib dokter terautentikasi dengan penugasan
aktif, sedangkan sebelumnya identitas verifikator tunduk pada masalah yang sama.

| Yang disimpan verifikasi | Sumbernya |
| --- | --- |
| Penulis asli | **Tidak disentuh** |
| Verifikator | `ApplicationUser.DoctorId` pengguna yang memverifikasi |
| Waktu verifikasi | Waktu server |
| Hubungan penugasan verifikator | Dinilai pada waktu klinis catatan yang diverifikasi |

### 3A.5 Nol butir hak akses baru

`Gelombang 1A` **tidak** melahirkan satu pun Resource maupun Action baru pada sub-modul ini.
Seluruh perubahan bekerja di atas butir hak akses yang sudah ada. Yang bertambah adalah pemeriksaan
hubungan pelaku dengan pasien, dan itu memang bukan sesuatu yang dapat diwakili butir hak akses.

Konsekuensinya untuk pengujian: **test hak akses lama tetap lulus tanpa perubahan**, dan itu justru
berbahaya bila dianggap cukup. Bukti bahwa penjaga baru bekerja hanya datang dari skenario negatif
per-pasien, bukan dari test peran.

---
## 4. Audit

| Lapisan | Yang dicatat |
| --- | --- |
| Kolom warisan `IdentityModel` | Pembuat, waktu buat, pengubah, dan waktu ubah pada setiap baris |
| Custom logger | Seluruh permintaan selain `GET`, berisi id entity, controller, action, dan status |
| Mesin integritas dokumen | Penandatanganan, penguncian, pembatalan, dan setiap addendum beserta alasannya |
| Jejak tahan lama yang **wajib** | Penyelesaian kajian medis dan catatan dokter; **setiap verifikasi CPPT** beserta verifikatornya; **setiap event visite** beserta pencatatnya; **setiap pembatalan event visite** beserta alasannya; pelaksanaan tindakan beserta penerbitan faktanya |

| Kejadian | Kenapa wajib berjejak | Acceptance |
| --- | --- | --- |
| Verifikasi CPPT | Verifikator dapat diaudit dan **bukan** penulis asli | `AC-CAP021-03` |
| Pencatatan visite | Episode, dokter, peran, waktu, dan pencatat dapat diaudit | `RWI-AC-153` |
| Pembatalan visite | Riwayat tetap menampilkan event batal beserta alasannya; hitungan tidak menghitungnya | `INV-DOK-08` |
| Agregasi tagihan oleh Billing | Riwayat klinis **tidak berubah sedikit pun** setelah agregasi | `RWI-AC-156` |
| Koreksi dokumen final | Alasan, penulis, penulis pengganti, dan nomor urut tersimpan | `INV-DOK-10` |
| Pendaftaran dokumen ke mesin keutuhan saat finalisasi | Penanda tangan, waktu, dan jenis dokumen | `RWI-AC-157` |
| Koreksi atas nama dokter lain | Penulis asli **tidak berubah**; dokter pengganti dan penetapan yang mendasarinya tersimpan | `RWI-AC-163`, `RWI-AC-164` |
| Penerbitan dan pencabutan penetapan berhalangan | Penerbit, dokter yang berhalangan, alasan, masa berlaku, dan pencabutnya | `RWI-AC-165` |
| **Pencatatan diagnosis terstruktur** ★ `0.4.0` | Konteks asalnya tersimpan apa adanya: nomor konsultasi bila ada, perawatan rawat inap bila itu asalnya. **Diagnosis tanpa keduanya tidak pernah tersimpan** | `VAL-DOK-36` |
| **Pembatalan dan penyelesaian diagnosis** ★ `0.4.0` | Alasan, pelaku, dan waktunya tersimpan; barisnya **tetap terbaca** dan tidak dihapus | `VAL-DOK-40` |

> **Jumlah visite tidak boleh dihitung dari catatan aktivitas teknis**, melainkan hanya dari event
> berstatus `Recorded`. Menghitung dari sumber lain melahirkan angka kedua yang berpotensi
> berselisih — persis yang dilarang `RWI-DEC-085`.

---

## 5. Kolom sensitif dan masa simpan

Kolom bertanda **Sensitif** pada [`../data/data-dictionary.md`](../data/data-dictionary.md)
**MUST NOT** masuk payload custom logger.

| Kolom | Tabel | Kenapa |
| --- | --- | --- |
| Isi S/O/A/P beserta seluruh kolom rencana | `TrxDoctorConsultation` | Isi klinis lengkap |
| Isi catatan | `TrxPatientIntegratedProgressNote` | Isi klinis |
| Isian kajian | `TrxPatientAssessment` | Isi klinis |
| `Note` | `CliPhysicianVisit` | Catatan bebas dokter |
| `CancelReason` | `CliPhysicianVisit` | Sering memuat alasan klinis |
| Alasan koreksi dan teks addendum | `MrcClinicalNoteAddendum` | Sering memuat alasan klinis atau nama pihak ketiga |
| Hasil tindakan | `TrxPatientProcedure` | Isi klinis |
| `ClinicalNote`, `AssessmentNote`, `PlanNote` ★ `0.4.0` | `TrxPatientDiagnosis` | Isi klinis |
| `DifferentialDiagnosisNote`, `SupportingFindingNote` ★ `0.4.0` | `TrxPatientDiagnosis` | Dugaan yang belum tegak; paling berbahaya bila terbaca di luar konteks |
| `ResolvedReason`, `CancelReason` ★ `0.4.0` | `TrxPatientDiagnosis` | Sering memuat alasan klinis |

**Masa simpan belum ditetapkan** — `RWI-OQ-035` menunggu pemilik hukum. Tidak ada penghapusan
otomatis yang dirancang, dan itu lebih aman daripada menebak masa simpan rekam medis.

---

## 6. Perubahan pada `contract_version` `0.6.0` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status `draft`.** Dokumen ini tetap tidak mendaftar ulang endpoint; pemetaan endpoint ke hak akses ada pada kolom
`Hak akses` `api-contract.md` bagian 12. String atribut dan status logger diturunkan seperti bagian 0.

**Pengecualian logger bernama yang lahir `0.6.0`:** tidak ada. Endpoint `GET` baru — `verification-worklist`,
`instruction-verification-worklist`, `my-authored`, `summary-prefill` — tidak dicatat logger, sesuai konvensi.

### 6.1 Resource dan Action baru

| Resource | Action | Baru atau tambahan | Modul pemilik atribut | Kegunaan |
| --- | --- | --- | --- | --- |
| `Prescription` | **`Stop`** | Action baru pada Resource yang ada | `PharmacyManagement` | Menghentikan butir resep |
| `PatientProcedure` | **`Verify`** | Action baru | `ClinicalManagement` | Verifikasi instruksi pesanan perawat |
| `LabOrder` | **`Verify`** | Action baru — **menunggu persetujuan pemilik** | `LaboratoryManagement` | Sama |
| `RadOrder` | **`Verify`** | Action baru — **menunggu persetujuan pemilik** | `RadiologyManagement` | Sama |
| `MedicationReconciliation` | `Read`, `Create`, `Update`, **`Decide`** | Resource baru | `PharmacyManagement` | Obat bawaan dan keputusan dokter |
| `SlidingScaleTemplate` | `Read`, `Update`, **`Approve`** | Resource baru | `PharmacyManagement` | Protokol standar berversi |
| `SlidingScaleOrder` | `Read`, `Create`, `Update` | Resource baru | `PharmacyManagement` | Protokol per pasien |

> **Pelajaran `BE-RWI-034` berlaku untuk kesembilan butir.** Nama pada `[AccessAction]` dan `[AccessPermission]`
> wajib sama persis dan diuji dengan peran non-SuperAdmin. `Decide` dan `Approve` sengaja dipisah dari `Create` dan
> `Update` supaya pemisahan tugas dapat diatur di layar Akses Role tanpa kode.

## 7. Peta peran ke butir hak akses — tambahan `0.6.0`

Bagian 2 tetap berlaku. Baris di bawah **menambah**; baris bagian 2 yang digantikan disebut eksplisit.

| Peran rumah sakit | Resource | Action | Catatan |
| --- | --- | --- | --- |
| **Dokter** — DPJP, konsulen, dan dokter jaga | `PatientIntegratedProgressNote` | `Read`, `Create`, `Update`, **`Verify`** | **Menggantikan** baris bagian 2 "Dokter jaga ruangan: sama dengan DPJP kecuali `Verify`". Satu dokter bisa DPJP bagi Budi dan dokter jaga bagi Joko pada hari yang sama, sedangkan hak akses melekat pada peran pengguna, bukan pada pasien. `Verify` karena itu diberikan kepada seluruh dokter, dan pembatasan "hanya DPJP" dijaga per pasien oleh `VAL-DOK-44` — lihat 8.1 |
| Dokter | `Prescription` | `Read`, `Create`, `Update`, **`Stop`** | Penghentian dijaga penugasan aktif — `VAL-DOK-51` |
| Dokter | `PrescriptionTemplate` | `Read`, `Create`, `Update`, `Delete` | Pemilik dijaga service — `VAL-DOK-56` |
| Dokter | `MedicationReconciliation` | `Read`, **`Decide`** | — |
| Dokter | `SlidingScaleOrder` | `Read`, `Create`, `Update` | — |
| Dokter | `SlidingScaleTemplate` | `Read` | Memilih versi sah saat memesan |
| Dokter | `PatientProcedure`, `LabOrder`, `RadOrder` | **`Verify`** | Hanya bermakna bagi dokter pemberi instruksi — `VAL-DOK-50` |
| Dokter | `ClinicalDocumentIntegrity` | `Read` | "Catatan Saya" |
| **Perawat** | `PatientProcedure` | `Read`, **`Create`**, **`Update`** | **Menggantikan** baris bagian 2 "Perawat: `Read` pada tindakan". `Create` untuk pesanan atas instruksi; `Update` untuk mengubah pesanannya sendiri dan menandai pelaksanaan — `RWI-DEC-114`, `RWI-DEC-139` |
| Perawat | `LabOrder`, `RadOrder` | `Read`, **`Create`** | **Menggantikan** baris "Perawat: `Read` pada lab dan radiologi". Gerbang persetujuan pemilik kedua modul |
| Perawat | `MedicationReconciliation` | `Read`, `Create`, `Update` | **Tanpa** `Decide` — `RWI-AC-192` |
| Perawat | `Prescription` | `Read` | **Tanpa** `Stop` — Resep Harian hanya-baca di Obat & Alkes |
| Perawat | `PrescriptionTemplate` | **Tidak ada** | `RWI-DEC-122` butir (3); jalur pakai juga menolak bukan-dokter pada konteks rawat inap |
| Perawat | `SlidingScaleOrder`, `SlidingScaleTemplate` | `Read` | Membaca protokol saat pelaksanaan |
| Apoteker | `SlidingScaleOrder`, `MedicationReconciliation` | `Read` | Melihat protokol dan keputusan rekonsiliasi saat menyiapkan obat — `RWI-DEC-147` contoh |
| **Pengubah konfigurasi farmasi-klinis** | `SlidingScaleTemplate` | `Read`, `Update` | Nama jabatan **tidak** ditanam di kode; diberikan lewat Akses Role |
| **Pengesah konfigurasi farmasi-klinis** | `SlidingScaleTemplate` | `Read`, **`Approve`** | Pengesah ≠ pengubah terakhir dijaga service — `VAL-DOK-54c` |
| Pengguna yang boleh mendaftarkan obat | `Drug` | `Create` | **Nol butir baru**; pendaftaran non-formularium memakai butir yang sudah ada — `RWI-DEC-134` butir (3) |
| Kepala ruangan, supervisor | — | — | Penugasan singkat dan penugasan konsulen/dokter jaga dipegang `episode-rawat-inap` `InpatientEpisode : Update` |

## 8. Kewenangan yang tidak dapat dijaga mesin hak akses — tambahan `0.6.0`

Mesin hak akses hanya tahu pasangan Resource dan Action pada peran. Seluruh baris berikut **lolos** pemeriksaan hak
akses dan **ditolak** aturan bisnis. Test yang hanya memeriksa hak akses **tidak dapat** membuktikannya.

| Yang dijaga | Penjaga | Yang **tidak** dijaganya | Risiko dan penanganannya |
| --- | --- | --- | --- |
| 8.1 Verifikasi CPPT hanya DPJP aktif atau DPJP terakhir | `VAL-DOK-44`, `44a`, `44b`, di dalam `CpptVerificationService` | Kualitas isi yang diverifikasi | Hari ini penjaga memakai `IsDoctorAssignedAsync` yang menerima peran apa pun (`RLN3-CAP-25`). Penggantinya `IsActiveDpjpAtAsync`; pesan lama "hanya DPJP" akhirnya menjadi benar |
| 8.2 Konsep hanya disunting dan diselesaikan penulisnya | `VAL-DOK-41`, di dalam jalur `PUT`/`PATCH` | Penulis yang menyelesaikan konsep berisi keliru | Diterima; koreksi lewat addendum. **Hanya menyala untuk kunjungan rawat inap**; poliklinik tetap perilaku lama |
| 8.3 Dokumen baru butuh penugasan pada waktu klinis dan saat simpan | `VAL-DOK-42` | Dokter jaga yang menulis catatan berwaktu klinis saat ini selama penugasan singkat aktif | Diterima secara sadar — `RWI-DEC-130` konsekuensi (3) |
| 8.4 Pesanan hanya diubah penginput; dibatalkan penginput atau DPJP aktif | `VAL-DOK-48`, `VAL-DOK-49` | Baris lama tanpa `OrderedByUserId` | Baris lama jatuh ke aturan lama: dokter pemesan = `DoctorId`. Tidak ada pengisian tebakan |
| 8.5 Pemberi instruksi harus bertugas atas pasien | `VAL-DOK-47` di tiga modul | Kebenaran bahwa instruksi benar-benar diberikan lewat telepon | Dijaga verifikasi dokter setelahnya dan jejak penginput. Batas waktu verifikasi menunggu pemilik klinis |
| 8.6 Hanya pemberi instruksi yang memverifikasi | `VAL-DOK-50` | Pemberi instruksi yang penugasannya sudah berakhir | Tetap boleh memverifikasi; verifikasi bukan dokumen baru. Jalan masuknya daftar tunggu verifikasi, bukan daftar pasien |
| 8.7 Penghentian butir resep oleh dokter yang bertugas | `VAL-DOK-51` | — | — |
| 8.8 Template milik sendiri | `VAL-DOK-56` s.d. `56c` | Dokter yang membagikan template ke pemakai poliklinik | Fitur Bersama poliklinik memang tetap hidup — `RWI-DEC-135` butir (4) |
| 8.9 Keputusan rekonsiliasi hanya dokter berwenang | `VAL-DOK-52` | — | Perawat memegang `Create` pada Resource yang sama; `Decide` terpisah memastikan layar Akses Role tidak dapat keliru memberinya |
| 8.10 Pengesah protokol ≠ pengubah terakhir | `VAL-DOK-54c` | Dua orang yang bersekongkol | Diterima; jejak pengubah dan pengesah tersimpan per versi |
| 8.11 "Catatan Saya" hanya milik penulis | Saringan `AuthorUserId` di server `MedicalRecordManagement` | — | Detail dibuka lewat endpoint dokumen pemiliknya yang tetap memeriksa hak baca biasa |

## 9. Audit, kolom sensitif, dan jejak tahan lama — tambahan `0.6.0`

### 9.1 Kejadian yang wajib meninggalkan jejak yang tidak dapat dihapus

| Kejadian | Di mana jejaknya | Kenapa |
| --- | --- | --- |
| Penghentian butir resep | `PhmPrescriptionItem.StoppedAt`, `StoppedByUserId`, `StopReason` | `RWI-DEC-121` butir (3): riwayat butir tidak dihapus |
| Setiap keputusan rekonsiliasi, termasuk yang digantikan | `PhmMedicationReconciliationDecision`, baris tidak pernah diubah | Keputusan terapi obat bawaan harus dapat ditelusuri |
| Pembuatan, perubahan, dan pengesahan versi protokol | `PhmSlidingScaleTemplateVersion` pengubah terakhir, pengesah, waktu, hash | `RWI-DEC-146` butir (1) |
| Pemesanan dan penyesuaian order sliding scale | `PhmSlidingScaleOrderVersion` beserta alasan | `RWI-DEC-146` butir (2), (6) |
| Verifikasi CPPT | Kolom verifikasi yang sudah ada; verifikator bukan penulis | `INV-DOK-11` |
| Verifikasi instruksi | Kolom `InstructionVerified*` pada ketiga tabel | `RWI-DEC-114` butir (5) |
| Pembatalan pesanan, termasuk otomatis saat penutupan | `CancelledAt`, `CancelledByUserId`, `CancelReason`, `CancelledByEpisodeClosure` | `RWI-DEC-143` butir (3): tercatat sebagai akibat penutupan beserta petugas yang menutup |
| Registrasi, tanda tangan, dan penguncian konsep | `MrcClinicalDocumentIntegrity` yang sudah ada | `RWI-DEC-138`, `RWI-DEC-144` |

### 9.2 Kolom sensitif baru

Kolom berikut **MUST NOT** masuk payload custom logger dan **MUST NOT** dipakai sebagai contoh berisi data asli.

| Kolom | Tabel | Kenapa sensitif |
| --- | --- | --- |
| `StopReason` | `PhmPrescriptionItem` | Alasan klinis penghentian terapi |
| `Note` | `PhmMedicationReconciliationItem` | Keterangan obat bawaan, sering memuat riwayat penyakit |
| `DecisionNote` | `PhmMedicationReconciliationDecision` | Alasan klinis keputusan |
| `AdjustmentReason` | `PhmSlidingScaleOrderVersion` | Keadaan klinis pasien, misalnya "sensitif insulin" |
| `StopReason` | `PhmSlidingScaleOrder` | Alasan klinis |

`ClinicalReason` dan `Instruction` pada permintaan pesanan tindakan memakai kolom `ClinicalNote` dan `InstructionNote`
yang sudah ada dan sudah diperlakukan sensitif pada praktik logging project.
