# Permission dan Audit Matrix — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.4.0` |
| `last_changed_in` | `0.4.0` — bagian 3A lahir: kewenangan menulis perawat ditentukan unit |
| Status | **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.1`; `PRD-RWI-FINAL-001` v1.0.0 |
| Tanggal | 2 September 2026 |

---

## 0. Yang **tidak** ada di dokumen ini

Dokumen ini **MUST NOT** memuat tabel seluruh endpoint. Pemetaan endpoint ke hak akses sudah
dipegang kolom `Hak akses` pada [`api-contract.md`](./api-contract.md). Dua hal berikut
**dihitung**, bukan ditulis ulang:

| Yang tidak ditulis ulang | Cara menurunkannya |
| --- | --- |
| String atribut | `[AccessPermission("<Resource>", "<Action>")]`, disalin dari kolom `Hak akses` |
| Status pencatatan logger | Konvensi project: `GET` tidak dicatat, selain `GET` dicatat |

**Pengecualian yang ditulis bernama:** tidak ada. Seluruh endpoint pada sub-modul ini mengikuti
kedua turunan itu apa adanya.

---

## 1. Cara kerja hak akses di repository ini

| Hal | Isinya | Rujukan source |
| --- | --- | --- |
| Atribut controller | `[AccessPermission("Resource", "Action")]` beserta `[AccessAction(...)]` yang mendaftarkan aksinya | `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` |
| Filter | `AccessPermissionFilter` mencocokkan pasangan Resource–Action terhadap baris hak akses peran | — |
| Jebakan yang sudah pernah terjadi | `BE-RWI-034` menemukan sembilan endpoint yang `[AccessAction]` dan `[AccessPermission]`-nya menyebut nama berbeda, sehingga filter tidak pernah menemukan barisnya dan menjawab `403` bagi siapa pun kecuali SuperAdmin | `episode-rawat-inap/task/report/backend/` |

> **Pelajaran yang wajib dibawa ke sub-modul ini.** Resource baru `NursingCarePlan` dan
> `NursingIntervention` **wajib** memakai nama yang sama persis pada kedua atribut, dan wajib
> diuji dengan peran non-SuperAdmin. Kesalahan yang sama menahan tujuh task frontend selama
> hampir seminggu.

---

## 2. Peta peran ke butir hak akses

| Peran rumah sakit | Resource | Action |
| --- | --- | --- |
| Perawat pelaksana | `PatientAssessment` | `Read`, `Create`, `Update` |
| Perawat pelaksana | `NursingCarePlan` | `Read`, `Create`, `Update` |
| Perawat pelaksana | `NursingIntervention` | `Read`, `Create`, `Update` |
| Perawat pelaksana | `PatientIntegratedProgressNote` | `Read`, `Create` |
| Kepala ruangan | Seluruh baris perawat pelaksana | ditambah `Amend` pada ketiga resource |
| DPJP dan dokter jaga | `PatientAssessment`, `NursingCarePlan`, `NursingIntervention` | `Read` **saja** |
| Ahli gizi | `PatientAssessment` | `Read` |
| Petugas admisi | — | **Tidak ada.** Dokumentasi klinis bukan wewenangnya |
| Kasir dan billing | — | **Tidak ada** |

> **Dokter hanya membaca, dan itu disengaja.** `AC-CAP014-03` melarang pengguna yang bukan penulis
> menyunting catatan keperawatan final. Memberi dokter `Update` akan membuat catatan perawat dapat
> diubah atas nama orang lain.

---

## 3. Kewenangan yang **tidak dapat** dijaga mesin hak akses

Mesin hak akses hanya tahu peran, tidak tahu pasien. Tiga hal berikut dijaga di tingkat aturan
bisnis, dan **wajib diketahui** karena ketiadaannya tidak akan terlihat dari daftar hak akses.

| Yang dijaga | Penjaganya | Yang **tidak** dijaganya | Risikonya |
| --- | --- | --- | --- |
| Perawat hanya menulis untuk pasien yang menjadi tanggung jawabnya | `VAL-KEP-05`, membaca `InpNurseAssignment` | Perawat yang **memang** ditugaskan tetap dapat menulis apa saja pada episode itu | Kesalahan isi, bukan kesalahan kewenangan. Dijaga jejak audit, bukan hak akses |
| Catatan final hanya diubah penulisnya atau kepala ruangan | `VAL-KEP-06`, `VAL-KEP-07` | Kepala ruangan dapat mengamandemen catatan siapa pun | Diterima: kepala ruangan memang penanggung jawab ruangan. Setiap amandemen wajib beralasan dan tercatat |
| Pengkajian tidak dibuat untuk pasien yang tidak dirawat | `INV-KEP-01`, `VAL-KEP-01` s.d. `04` | Bila episode salah dipilih di layar, sistem tidak tahu | Dikurangi dengan menyalakan konteks pasien di kepala ruang kerja pada setiap layar |

---


## 3A. Kewenangan menulis perawat ditentukan unit — `0.4.0`

**Bagian baru 11 September 2026**, menyerap `RWI-DEC-100`. Ia melengkapi bagian 3, dan seperti
bagian itu, isinya kewenangan yang **tidak dapat** dijaga mesin hak akses.

### 3A.1 Keadaan hari ini

`InpatientClinicalContextService` **nol menyebut perawat** pada seluruh berkasnya. Satu-satunya
pemeriksaan kewenangan yang tersedia adalah `IsDoctorAssignedAsync`. Akibatnya
`PatientAssessmentController`, yang dipakai perawat, tidak punya penjaga kewenangan apa pun,
bahkan bila dokter pelaku dikirimkan. Dasarnya `RWI-FACT-022`.

Artinya hari ini pengguna mana pun yang memegang `PatientAssessment : Create` dapat menulis
pengkajian untuk pasien mana pun di rumah sakit.

### 3A.2 Penjaga yang berlaku

| Penjaga | Isinya |
| --- | --- |
| `GUARD-INP-07` | Penulis diambil dari `ApplicationUser.EmployeeId` pengguna terautentikasi. `nurseId` pada payload **tidak** menentukan penulis |
| `GUARD-INP-08` | Perawat boleh menulis untuk pasien yang episodenya berada di **unit tempat perawat itu bertugas**. Penugasan per episode **bukan** syarat |

Definisi kanonik `GUARD-INP-07` dipegang
[`../episode-rawat-inap/contracts/permission-audit-matrix.md`](../../episode-rawat-inap/contracts/permission-audit-matrix.md)
bagian 4-A.3. `GUARD-INP-08` lahir di sini karena hanya menyangkut perawat.

### 3A.3 Kenapa gerbang perawat lebih longgar daripada gerbang dokter

Perbedaan ini **disengaja** dan wajib dibaca sebagai keputusan, bukan sebagai kelalaian.

| Hal | Dokter | Perawat |
| --- | --- | --- |
| Lama melekat pada pasien | Berhari-hari | Satu shift |
| Berapa pasien sekaligus | Beberapa | 20 sampai 30 satu bangsal |
| Pergantian per hari | Jarang | Tiga kali |
| Sumber kewenangan | Penugasan per episode | Unit tempat episode berada |

Menuntut penugasan per episode bagi perawat berarti membuat sekitar 90 baris penugasan manual per
hari per bangsal, karena sistem **tidak mempunyai konsep shift perawat sama sekali** yang dapat
membangkitkannya. Dasarnya `RWI-DEC-100`.

### 3A.4 `InpNurseAssignment` tetap ada, dan maknanya tidak berubah

| Yang ia lakukan | Yang ia **tidak** lakukan |
| --- | --- |
| Menandai perawat penanggung jawab yang tampil pada kepala konteks pasien | Mengunci siapa yang boleh menulis |
| Menjadi bahan daftar pantau episode tanpa perawat | Menjadi gerbang hak akses |

### 3A.5 Satu hal yang belum ditetapkan dan menjadi pekerjaan desain teknis

Sumber data "unit tempat perawat bertugas" **belum ditetapkan**. `InpNurseAssignment` tidak
menyimpannya, dan penetapannya adalah pekerjaan pada task implementasi.

**Satu jalan yang dilarang.** Menambahkan kolom unit ke `InpNurseAssignment` **tidak boleh**
dipakai sebagai jalan pintas. Unit episode dapat berubah ketika pasien dipindahkan, sehingga
menyalin unit ke baris penugasan melahirkan sumber kebenaran kedua yang akan berbeda dari unit
episode begitu perpindahan pertama terjadi.

### 3A.6 Nol butir hak akses baru

Sama seperti sub-modul dokter, `Gelombang 1A` tidak melahirkan Resource maupun Action baru di sini.
Test hak akses lama akan tetap lulus tanpa disentuh, sehingga **tidak dapat** dipakai sebagai bukti
bahwa penjaga baru bekerja.

---
## 4. Audit

| Lapisan | Yang dicatat |
| --- | --- |
| Kolom warisan `IdentityModel` | `CreateBy`, `CreateDate`, `UpdateBy`, `UpdateDate` pada setiap baris |
| Custom logger | Seluruh permintaan selain `GET` |
| Jejak tahan lama yang **wajib** ada | Penyelesaian pengkajian; **setiap amandemen** beserta alasannya; penutupan butir asuhan; finalisasi dan amandemen catatan tindakan |

Kejadian yang **wajib** meninggalkan jejak yang tidak dapat dihapus:

| Kejadian | Kenapa |
| --- | --- |
| Amandemen pengkajian final | `CAP-012` aturan 13 menuntut aktor, waktu, alasan, dan perubahannya |
| Perubahan butir rencana asuhan | `AC-CAP013-02` menuntut versi lama mempertahankan penulis dan waktunya |
| Amandemen catatan tindakan final | `AC-CAP014-03` |

---

## 5. Kolom sensitif dan masa simpan

Kolom bertanda **Sensitif** pada [`../data/data-dictionary.md`](../data/data-dictionary.md)
**MUST NOT** masuk payload custom logger dan **MUST NOT** dipakai sebagai contoh berisi data asli.

| Kolom | Tabel | Kenapa sensitif |
| --- | --- | --- |
| `ResultNote` | `TrxNursingIntervention` | Berisi keadaan klinis pasien |
| `NurseNote`, `PsychosocialNote`, `EducationNote` | `TrxPatientAssessment` | Catatan bebas; sering memuat keadaan sosial dan keluarga |
| `PainNote`, `NutritionNote`, `FallRiskNote`, `FunctionalNote` | `TrxPatientAssessment` | Sama |
| `AmendReason` | Ketiga tabel | Sering memuat alasan klinis atau nama pihak ketiga |

**Masa simpan belum ditetapkan.** `RWI-OQ-035` sudah dijawab `RWI-DEC-060` tetapi menunggu pemilik
hukum. Sampai itu turun, tidak ada penghapusan otomatis yang dirancang — dan itu keadaan yang
lebih aman daripada menebak masa simpan rekam medis.
