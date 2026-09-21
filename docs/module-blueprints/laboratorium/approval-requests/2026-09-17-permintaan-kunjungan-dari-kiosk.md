# Permintaan Persetujuan — Kunjungan Laboratorium dari Kiosk

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-012` |
| `tanggal` | 2026-09-17 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `rujukan` | `LAB-OPEN-025`, `LAB-OPEN-026`; `BE-EXT-05`; `LAB-DEC-053`, `LAB-DEC-054`, `LAB-DEC-058`, `LAB-DEC-059`; wewenang `LAB-REQ-006` |
| `status` | **Butir 3 `terjawab` 2026-09-17 — pilihan A.** Butir 4 `menunggu jawaban` |
| `ditujukan kepada` | Pemilik modul `registration-management`; pemilik modul `master-data`; pemegang wewenang kontrol akses |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

> ## Jawaban dan koreksi — ditulis menyusul 2026-09-17
>
> **Butir 3 dijawab: pilihan A**, oleh pemilik modul `registration-management` **Andry Zain**,
> disampaikan pemilik modul Laboratorium. `BE-EXT-05` dikerjakan atas wewenang itu dan
> **selesai** pada hari yang sama — [`BE-EXT-05.md`](../task/report/backend/BE-EXT-05.md).
>
> **Tetapi pelaksanaannya membantah dasar dokumen ini, dan itu perlu ditulis terbuka.**
> Pemeriksaan source sebelum satu baris pun ditulis menemukan **kedua penahan tidak ada**:
>
> | Yang dokumen ini tuliskan | Yang terbaca dari source 2026-09-17 |
> |---|---|
> | Bagian 3.1: *"Pembentukan kunjungan: `POST /patient-encounters/admin`, menuntut `AccessPermission("PatientEncounter", "Create")`"* | **Tidak lengkap.** Ada route kedua — `POST /patient-encounters/kiosk` (`PatientEncounterController.cs:406-417`) — dijaga **hanya** `[Authorize(Policy = KioskReadPolicy)]` dan **nol** memikul `[AccessPermission]`. Ia sudah menerima `KioskScanSessionId`, menandai `IsFromKiosk`, membentuk nomor antrean, menandai sesi terpakai, dan menolak Penjamin Perusahaan |
> | Bagian 3.2: *"bagaimana prinsipal perangkat memperoleh izin pada model izin untuk jabatan pegawai"* | **Pertanyaannya tidak pernah perlu dijawab.** Jalur kiosk memang sudah dikecualikan dari model izin itu, persis sebagaimana pilihan A menganjurkan |
> | Bagian 4: *"Selama penandanya `false`, kunjungan laboratorium dari kiosk tidak punya unit layanan yang sah"* | **Keliru.** `IsAvailableForKiosk` **nol disebut** `PatientEncounterController` maupun `EncounterIntakeService`; yang dituntut adalah **`IsAvailableForRegistration`**, dan pada `SU-LAB-001` nilainya **`true`** |
>
> **Pilihan A karena itu sudah berdiri di source sejak sebelum dokumen ini ditulis.** Yang
> benar-benar belum ada hanyalah butir 4 `BR-46` — penutupan otomatis akhir hari layanan — dan
> itulah yang dibangun `BE-EXT-05`.
>
> **Butir 4 tetap berlaku, dengan sifat yang berubah:** bukan penahan, melainkan syarat agar
> Laboratorium tampil pada **daftar pilihan** layar kiosk yang menyaring
> `?isAvailableForKiosk=true`. Dicatat sebagai `LAB-OPEN-026b`.
>
> **Sebab kekeliruannya sama pada keduanya, dan pantas dicatat:** dokumen ini menyebut nama yang
> benar pada **jalur yang salah**. Itu pola yang sama dengan `LAB-COORD-010` dan
> `DATA-MST-MEASUREMENT` — catatan penahan yang dipakai menghentikan pekerjaan tanpa pernah
> diverifikasi ulang terhadap source. Dua hari berhenti untuk penahan yang tidak ada.

---

> **Kedua penahan ini belum pernah diajukan kepada siapa pun.** Penelusuran seluruh dokumen
> `approval-requests/` pada 2026-09-17 menghasilkan **nol kemunculan** `LAB-OPEN-025` maupun
> `LAB-OPEN-026`; keduanya hanya tercatat pada dokumen internal Laboratorium sejak 2026-09-15.
>
> **Ini ketiga kalinya pola yang sama ditemukan pada modul ini dalam satu hari** — sesudah
> `LAB-COORD-010` (`LAB-REQ-008`) dan `DATA-MST-MEASUREMENT` (`LAB-REQ-010`). Tiga penahan,
> tiga kali sebab yang sama: disangka sedang menunggu jawaban, padahal belum pernah ditanyakan.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Pasien dapat memilih **Laboratorium** di kiosk — layarnya sudah berdiri sejak `FE-LAB-13`, dan
panel yang membacanya sejak `FE-LAB-14`. Yang belum: **pasien itu tidak memperoleh kunjungan
maupun nomor antrean.** Sesi kiosknya tercatat, tetapi berhenti di situ.

Pekerjaannya sendiri kecil dan sudah dirancang (`BE-EXT-05`). Yang menahannya **dua hal, dan
keduanya di luar wewenang Laboratorium**: apakah prinsipal kiosk boleh membentuk kunjungan, dan
satu penanda pada data induk unit layanan.

Yang diminta: **dua keputusan**. Bagian 3 dan 4.

---

## 2. Yang sudah siap, supaya cakupannya jelas

| Sudah ada | Bukti |
|---|---|
| Langkah **Tujuan Layanan** pada alur kiosk Pasien Lama | `FE-LAB-13` ✅ 2026-09-16 |
| Ruas `TargetService` pada sesi kiosk, beserta jalur tulisnya | `BE-EXT-04` ✅ dan `BE-EXT-04b` ✅ |
| Panel sesi kiosk pada layar pendaftaran lab | `FE-LAB-14` ✅ |
| Persetujuan lintas modul bahwa kiosk bertambah bagian Laboratorium | `LAB-REQ-006`, disetujui Andry Zain 2026-09-15 |
| Pukul penutupan hari layanan | `LAB-DEC-059` — **21:00 WIB**, sebagai konfigurasi |
| Rancangan `BE-EXT-05` | `backend-roadmap.md` bagian 6c |

**Satu hal yang perlu diketahui sebelum membaca lebih jauh.** Butir *"biaya pendaftaran gugur"*
pada rancangan `BE-EXT-05` **sudah terpenuhi tanpa kode apa pun**: Registrasi nol menerbitkan
fakta kelayakan tagih, dan `DefaultRegistrationFee` hanya hidup sebagai data induk. Tidak ada
tagihan yang perlu digugurkan karena tidak pernah ada yang terbit. Ditulis supaya tidak ada yang
membangun pembatalan tagihan yang tidak punya sasaran.

---

## 3. Butir 1 — `LAB-OPEN-025`: bolehkah prinsipal kiosk membentuk kunjungan?

### 3.1 Keadaan hari ini, dibaca dari source dan database

| Hal | Temuan |
|---|---|
| Kebijakan kiosk | `KioskRead` pada `Program.cs:717` — berbasis **peran dan klaim** |
| Peran `Kiosk` pada `AspNetRoles` | **Tidak ada.** Nol baris |
| Yang benar-benar dipakai | Klaim `is_kiosk`, `user_type_id = 6`, `user_type = SystemUser`, `is_kiosk_account`, `profile_type = KioskDevice` |
| Pembentukan kunjungan | `POST /patient-encounters/admin`, menuntut `AccessPermission("PatientEncounter", "Create")` |
| Kunci tabel izin `SysAccessPolicy` | `DepartmentId` + `PositionId` + `ControllerAccessId` + `ActionAccessId` |
| Perangkat kiosk terdaftar | **10** baris pada `MstKioskDevice` |

### 3.2 Kenapa ini bukan sekadar memberi centang izin

Catatan lama Laboratorium menuliskannya sebagai *"bila tidak dipegang, pasien kiosk gagal
didaftarkan"* — seolah tinggal memberikan satu izin. **Pemeriksaan 2026-09-17 menunjukkan
perkaranya lebih dalam.**

Izin pada sistem ini dikunci pada **Department dan Position** — bentuk yang mengandaikan
pemegangnya seorang **pegawai**. Prinsipal kiosk adalah **perangkat**, bukan pegawai: ia dikenali
lewat klaim, dan peran `Kiosk` bahkan tidak ada sebagai baris.

Pertanyaannya karena itu bukan *"berikan izinnya atau tidak"*, melainkan:

> **Bagaimana sebuah prinsipal perangkat memperoleh izin pada model izin yang dibangun untuk
> jabatan pegawai — dan apakah itu memang cara yang benar?**

Itu keputusan arsitektur kontrol akses, dan Laboratorium **tidak berwenang** menjawabnya.

### 3.3 Tiga kemungkinan, ditulis supaya jawabannya konkret

| # | Kemungkinan | Konsekuensinya |
|---:|---|---|
| A | Kiosk diberi jalur pembentukan kunjungan **tersendiri** yang terbatas | Wewenangnya sempit dan dapat diaudit. `POST /admin` yang menerima Penjamin Perusahaan **tidak** ikut terbuka — batas yang memang sudah dijaga `RWI-ENC-PAYER-001` |
| B | Kiosk memperoleh `PatientEncounter : Create` lewat model izin yang ada | Perlu menetapkan Department/Position untuk perangkat, atau memperluas model izinnya |
| C | Kunjungan **tidak** dibentuk kiosk; petugas lab yang membentuknya dari panel sesi | Nol perubahan kontrol akses. Pasien tetap perlu menghampiri petugas — yang mengurangi guna kiosk, tetapi tetap lebih baik daripada keadaan sekarang |

**Laboratorium condong ke A**, dan alasannya ditulis terbuka: ia memberi wewenang paling sempit
yang cukup, dan tidak menyentuh jalur admisi yang sudah dijaga. Tetapi **pilihan ada pada
pemilik `registration-management`** — Laboratorium tidak tahu beban rawatnya.

> **Pilihan C bukan pilihan buruk**, dan pantas dipertimbangkan bila A dan B mahal. Panel sesi
> kiosk `FE-LAB-14` **sudah berdiri dan sudah dapat terisi**; yang berubah hanya siapa yang
> menekan tombolnya.

---

## 4. Butir 2 — `LAB-OPEN-026`: satu penanda pada unit layanan

Dibaca dari database pada 2026-09-17:

| `ServiceUnitCode` | Nama | `IsAvailableForKiosk` | `IsActive` |
|---|---|:---:|:---:|
| `SU-LAB-001` | Laboratorium Klinik | **`false`** | `true` |

Selama penandanya `false`, kunjungan laboratorium dari kiosk **tidak punya unit layanan yang
sah** — bahkan bila butir 1 dijawab.

**Yang diminta:** menyetel `IsAvailableForKiosk = true` pada `SU-LAB-001`, **atau** pernyataan
bahwa unit itu memang tidak boleh dipilih dari kiosk beserta unit mana yang seharusnya dipakai.

Perubahan satu ruas, tetapi **data induk global** — wewenang `master-data`, bukan Laboratorium.

---

## 5. Yang **tidak** diminta

| Hal | Keadaan |
|---|---|
| Perluasan wewenang kiosk di luar pembentukan kunjungan | **Tidak.** Khususnya `POST /patient-encounters/admin` yang menerima Penjamin Perusahaan — batas itu sengaja tidak disentuh |
| Perubahan alur kiosk Pasien Baru | Tidak. `LAB-DEC-052` sudah mempersempitnya, dan `FE-LAB-13` mengikutinya |
| Penutupan otomatis kunjungan | **Dirancang, belum diminta diputuskan.** `LAB-DEC-058` dan `LAB-DEC-059` sudah menetapkan aturannya; pelaksanaannya menunggu butir 1 |
| Pembatalan tagihan pendaftaran | **Tidak ada yang perlu dibatalkan** — lihat bagian 2 |

---

## 6. Akibat bila belum dijawab

| Yang tertahan | Akibat nyatanya hari ini |
|---|---|
| `BE-EXT-05` | **Satu-satunya task Laboratorium yang belum selesai.** Nol baris source ditulis |
| Pengalaman pasien di kiosk | Pasien memilih Laboratorium, lalu **tidak terjadi apa-apa**: nol kunjungan, nol nomor antrean. Ia tetap harus menghampiri petugas — tanpa tahu bahwa pilihannya di kiosk tidak menghasilkan apa pun |
| `AC-92` | Tidak dapat dibuktikan |

**Satu peringatan yang perlu dibawa ke pelaksanaannya kelak**, dan ditulis di sini supaya tidak
ditemukan belakangan: penutupan otomatis akhir hari **wajib** menyaring pada
`TargetService = Laboratory`. Penyaring yang lebih longgar akan menyapu **15 kunjungan kiosk
nyata** dan menyentuh **91 kunjungan berstatus `WaitingForNurse`** — pasien poliklinik yang
sedang menunggu dipanggil. Kesalahan itu tidak muncul sebagai galat; ia muncul sebagai pasien
yang pendaftarannya hilang saat ia sedang duduk menunggu.

---

## 7. Cara menjawab

| Butir | Penjawab | Yang dibutuhkan |
|---|---|---|
| 3 | Pemilik `registration-management` beserta pemegang wewenang kontrol akses | A, B, C, atau bentuk lain |
| 4 | Pemilik `master-data` | Penanda disetel, atau unit penggantinya disebut |

Keduanya dapat dijawab terpisah. Butir 4 tidak berguna tanpa butir 3, tetapi butir 3 **dapat**
diputuskan lebih dulu.

Jawaban akan dicatat sebagai penutupan `LAB-OPEN-025` dan `LAB-OPEN-026` pada
`blueprint-manifest.md` dan `roadmap/traceability.md`, dan `BE-EXT-05` diturunkan menjadi task
yang dapat dikerjakan — **oleh pemilik `registration-management`**, karena kontrak dan kodenya
memang di sisi itu.
