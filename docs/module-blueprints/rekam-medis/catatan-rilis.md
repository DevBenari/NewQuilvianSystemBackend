# Catatan Rilis — Modul Rekam Medis

> Berkas ini memuat dua rilis. **Rilis amandemen `0.2.0`** ada di atas; **rilis pertama `0.1.x`**
> tetap utuh di bawahnya karena perilakunya masih berlaku.

# Rilis amandemen `0.2.0` — berkas lintas modul dan kelengkapan berkas

| Field | Isi |
|---|---|
| Tanggal disusun | 2 November 2026 |
| Disusun pada task | `BE-41` |
| Kontrak | API, state, validation, integration, permission/audit **`0.2.0`** — disahkan 28 Oktober 2026 (`RM-DEC-062`) |
| Task yang dirilis | `BE-21`–`BE-40`, seluruhnya `SELESAI`; laporan per task di `task/report/backend/` |
| Status catatan ini | **Menunggu pengesahan** pemilik API (Yoga Aji Pratama) — lihat bagian A9 |
| Dampak skema | **Aditif.** Empat tabel baru dan satu kolom ber-bawaan dalam satu migration `20261028070658_AddMedicalRecordCompletenessFoundation`; **nol** perubahan kolom pada tabel modul lain |
| Dampak perilaku | Linimasa kini membaca seluruh modul; penutupan kunjungan mencatat waktu tutup klinis; berkas dinilai kelengkapannya. **Bentuk respons hanya bertambah** — klien `0.1.1` tetap berjalan |

> **BACA BAGIAN A1 DAN A7 LEBIH DULU.** A1 memuat perubahan perilaku pada endpoint yang sudah
> dipakai. A7 memuat keterbatasan yang wajib tampil di layar supaya pembaca tidak salah membaca
> berkas.

## A1. Perubahan perilaku pada endpoint yang sudah berjalan

### A1.1 Jenis 14 dan 15 tidak lagi kosong diam-diam

`GET .../medical-records/{patientId}/timeline` dan `.../summary`

| | |
|---|---|
| **Sebelumnya** | Jenis `14` (sesi hemodialisa) dan `15` (hasil laboratorium) sudah terdaftar, tetapi tidak punya pembaca. Linimasa menjawabnya **kosong**, seolah pasien tidak punya dokumen itu |
| **Sekarang** | Keduanya terbaca dari modulnya: sesi hemodialisa yang sudah disahkan dan hasil laboratorium PK/Mikrobiologi yang sudah dirilis |
| **Jenis tanpa pembaca** | Tidak lagi dijawab kosong. Jenis itu dilaporkan lewat field baru `unavailableKinds`, lengkap dengan kalimat untuk pembaca. Saat ini hanya satu: hasil Patologi Anatomi (`44`, `RM-DEC-058`) |

**Yang harus dilakukan klien:** menampilkan isi `unavailableKinds` sebagai "belum tersedia",
**bukan** "tidak ada dokumen". `isComplete` tidak dipengaruhi `unavailableKinds`; ia tetap hanya
menyatakan sumber gagal dan pemotongan.

> **Perbaikan pada `BE-41`.** Field `unavailableKinds` sudah dihitung sejak `BE-21`, tetapi baru
> sampai ke selubung respons `/timeline` (`MedicalRecordTimelineResponse`) pada `BE-41`. Sebelum
> itu ia hanya ada di respons `/summary`. Uji baru memastikan setiap field hasil linimasa sampai
> ke klien.

### A1.2 Satu sumber lambat tidak lagi membatalkan berkas

| | |
|---|---|
| **Sebelumnya** | Seluruh sumber dibaca tanpa batas waktu; satu sumber yang macet menahan seluruh linimasa |
| **Sekarang** | Setiap sumber dibatasi **3 detik**, seluruh berkas **20 detik**. Sumber yang melewati batas masuk `failedSources`, dan respons tetap `200` |
| **Pembatalan oleh pengguna** | Tetap membatalkan permintaan, sama seperti sebelumnya |
| **Konfigurasi** | `MedicalRecord:SourceTimeoutSeconds`, `MedicalRecord:FileTimeoutSeconds` |

### A1.3 Linimasa kini membaca seluruh modul

Jenis baru yang tampil, masing-masing hanya bila sudah **final menurut modul pemiliknya**:

| Modul | Jenis | Final bila | Task |
|---|---|---|---|
| Laboratorium | `15` Hasil laboratorium (PK, Mikrobiologi) | Dirilis | `BE-23` |
| Radiologi | `16` Laporan radiologi | Dirilis | `BE-24` |
| IGD | `17`–`20` Triase, observasi, resusitasi, disposisi | Selesai; disposisi `Confirmed` | `BE-29` |
| Rawat Inap | `21` Ringkasan pulang, `22` Dokumen admisi | Ditandatangani / `Completed` | `BE-30` |
| Kamar Operasi | `23`–`26` Laporan operasi, catatan anestesi, checklist keselamatan, catatan kelahiran | `Final` / `Completed` / saat tersimpan | `BE-31` |
| Hemodialisa | `14` Sesi hemodialisa | Disahkan (`Finalized`) | `BE-32` |
| Gizi | `27` Asuhan gizi | Saat tersimpan, **dapat berubah** | `BE-33` |
| Bank Darah | `28` Golongan darah, `29` Reaksi transfusi | `Validated` / `Acknowledged` | `BE-34` |
| Farmasi | `30`–`32` Resep, pemberian obat, rekonsiliasi obat | `Submitted` / `Administered` / ada keputusan | `BE-35` |
| MCU | `33` Resume, `34` Hasil pemeriksaan | Versi yang pernah diverifikasi | `BE-36` |
| Rekam medis (`ClinicalManagement`) | `35`–`42` Rencana asuhan, visite, intervensi, monitoring transfusi, surat dokter, evaluasi manajemen kasus, infeksi nosokomial, serah terima | Final menurut modelnya | `BE-22` |
| Rekam medis (`ClinicalManagement`) | `43` Ringkasan pencatatan berkala | Satu baris per jenis per hari (WIB) | `BE-22` |

Pembaca tidak memeriksa izin modul asal (`RM-DEC-040`): pengguna dengan `MedicalRecord : Read`
melihat dokumen final semua modul. Jejak aksesnya tetap dicatat berkas rekam medis.

Field tambahan pada setiap baris (`MedicalRecordTimelineItemResponse`):

| Field | Arti | Yang harus dilakukan klien |
|---|---|---|
| `sourceModule` | Modul pemilik dokumen | Tampilkan sebagai asal dokumen |
| `finalizedAt` | Waktu final menurut modul pemilik | — |
| `isCorrected` | Pernah diralat lewat mekanisme modul pemilik; yang tampil versi berlaku | Tampilkan tanda "pernah diralat" |
| `isProvisional` | Isinya dapat berubah tanpa riwayat — saat ini hanya asuhan gizi | **Wajib** berlabel "Dapat berubah" |
| `isLateEntry` | Dokumen final **sesudah** tutup klinis kunjungannya | Tampilkan tanda "Susulan" |
| `clinicalClosedAt`, `isClinicalClosureDerived` | Waktu tutup klinis pembanding; `true` bila diturunkan dari data lama | — |
| `periodicSummary` | Hanya pada jenis `43`: `kind`, `date`, `count` | Buka rinciannya lewat endpoint A2.1 |

`GET .../documents/{documentKind}/{documentId}` meneruskan jenis baru ke modul pemiliknya. Isinya
dibaca saat diminta dan **tidak** disimpan rekam medis.

### A1.4 Penutupan kunjungan mencatat waktu tutup klinis

Keenam jalur penutupan (PATCH status kunjungan, antrean dokter, nurse station, penutupan IGD,
pemulangan rawat inap, finalisasi tagihan) kini mencatat **satu** waktu tutup klinis per kunjungan,
yaitu waktu penutupan pertama. Jalur berikutnya tidak menggesernya (`RM-DEC-051`).

| Hal | Keadaan |
|---|---|
| Disposisi IGD ke rawat inap | **Kini mengunci** catatan IGD yang masih konsep dan mencatat tutup klinis dalam transaksi yang sama (`BE-27`) |
| Gagal mencatat tutup klinis | Penutupan pada jalur bertransaksi ikut gagal, sama seperti aturan penguncian rilis pertama (bagian 1.4) |
| Dua penutupan serentak pada PATCH status atau antrean dokter | Yang kedua dapat gagal dengan galat index unik lalu berhasil bila diulang. Kedua jalur itu tidak bertransaksi; menambahkannya butuh persetujuan Registrasi tersendiri |

**Yang harus dilakukan klien:** tidak ada perubahan bentuk. Tanda "Susulan" (`isLateEntry`)
dihitung terhadap waktu tutup klinis ini.

### A1.5 Addendum menerima `purpose`

`POST .../clinical-note-addendums/by-document/{documentKind}/{documentId}` dan `.../as-substitute`

| | |
|---|---|
| **Field baru** | `purpose` opsional: `Correction` (bawaan) atau `Attestation` |
| **`Attestation`** | Penulis mengesahkan isi catatan `LockedUnsigned` tanpa mengubah statusnya (`RM-DEC-049`) |
| **Kode status baru** | `409` bila pengesahan atas catatan yang bukan `LockedUnsigned`, atau catatan sudah disahkan |
| **Klien lama** | Tanpa `purpose` tetap membuat addendum koreksi |

Keterbatasan: addendum yang dibuat lewat `PatientAssessmentController` (`ClinicalManagement`)
selalu bertanda `Correction`, karena controller itu menyusun responsnya sendiri.

## A2. Endpoint baru

### A2.1 Health Services / Medical Record Management / Medical Record — tambahan

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/{patientId}/periodic-observations` | Rincian satu ringkasan pencatatan berkala. Query `kind` dan `date` (tanggal WIB); nilai tidak sah → `400` | `MedicalRecord : Read` |

### A2.2 Health Services / Medical Record Management / Medical Record Completeness

Base URL: `api/v1/health-services/medical-record-management/medical-record-completeness`

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan saringan: keadaan, jenis kunjungan, jenis kekurangan | `MedicalRecordCompleteness : Read` |
| `GET` | `/summary` | Jumlah kunjungan per keadaan | `MedicalRecordCompleteness : Read` |
| `GET` | `/` | Daftar kerja; bawaannya kunjungan yang belum `Complete` | `MedicalRecordCompleteness : Read` |
| `GET` | `/encounters/{encounterId}` | Penilaian tersimpan satu kunjungan beserta seluruh kekurangannya. `404` bila belum pernah dinilai | `MedicalRecordCompleteness : Read` |
| `POST` | `/encounters/{encounterId}/evaluate` | Menilai ulang sekarang. Kunjungan yang tutup sebelum rilis dinilai **tanpa disimpan** (`isPersisted = false`). `409` bila kunjungan masih berjalan, batal, atau `NoShow` | `MedicalRecordCompleteness : Evaluate` |

### A2.3 Health Services / Master Data / Medical Record Completeness Profile

Base URL: `api/v1/health-services/master-data/medical-record-completeness-profiles`

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan jenis kunjungan, jenis unit, kondisi | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/summary` | Jumlah profil aktif dan nonaktif | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/` | Daftar profil | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/criteria/options` | Katalog kriteria beserta keterangannya | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/{id}` | Satu profil beserta butirnya | `MedicalRecordCompletenessProfile : Read` |
| `POST` | `/` | Membuat profil | `MedicalRecordCompletenessProfile : Create` |
| `PUT` | `/{id}` | Mengubah profil dan mengganti butirnya | `MedicalRecordCompletenessProfile : Update` |
| `PATCH` | `/{id}/status` | Aktif atau nonaktif. Profil tidak pernah dihapus | `MedicalRecordCompletenessProfile : Update` |

**Tiga selisih terhadap kontrak `0.2.0` A3** (`BE-37`), seluruhnya ke arah yang lebih sempit:

1. `items[].sortOrder` tidak diterima. Isiannya diabaikan, tidak ditolak; kriteria diurutkan menurut nilai katalog.
2. `/criteria/options` membalas `value`, `name`, `label`, `description`, bukan `OptionResponse` generik.
3. Kode profil kembar dijawab "Kode profil X sudah dipakai profil lain."

## A3. Kelengkapan berkas

| Keadaan | Arti |
|---|---|
| `Complete` | Seluruh syarat terpenuhi |
| `Incomplete` | Ada dokumen wajib yang belum ada, atau catatan yang belum ditandatangani dan belum disahkan |
| `AwaitingResult` | Ada hasil penunjang yang belum dirilis (laboratorium termasuk PA, radiologi) |
| `Undetermined` | Ada sumber yang gagal dibaca atau profilnya tidak ditemukan. **Tidak pernah berarti lengkap** (`RM-DEC-044`) |

| Hal | Aturan |
|---|---|
| Dokumen wajib | Dari master profil: profil dasar menurut jenis kunjungan dan jenis unit, ditambah profil kondisi (`K-OK` operasi, `K-HD` sesi hemodialisa, `K-TRF` transfusi) bila kondisinya ada |
| Kondisi operasi | Ada kasus `Completed`. **Setiap** kasus selesai wajib punya laporan operasi, catatan anestesi, dan checklist Sign In, Time Out, Sign Out (`RM-DEC-076`) |
| Kondisi sesi hemodialisa | Ada sesi yang berakhir (`Stopped`, `Completed`, `AwaitingFinalization`, `Finalized`). **Setiap** sesi itu wajib disahkan |
| Kunjungan `Unknown` | Tidak punya profil → kekurangan `RuleNotFound`, keadaan `Undetermined` |
| Penilaian pertama | Sesudah tutup klinis, lewat antrean di memori, di luar transaksi penutupan |
| Penilaian berkala | Setiap 5 menit, paling banyak 200 kunjungan per putaran. Giliran per kunjungan: 60 menit untuk yang tutup ≤ 30 hari, 24 jam untuk yang lebih lama. Yang sudah `Complete` tidak diperiksa berkala |
| Pemicu penilaian ulang | Tanda tangan dokumen, addendum pengesahan, perubahan profil |
| Konfigurasi | `MedicalRecordCompleteness:RecheckEnabled`, `MedicalRecordCompleteness:RecheckIntervalSeconds` |
| Dampak pada alur lain | **Tidak ada.** Penutupan, pemulangan, dan tagihan tidak pernah tertahan berkas yang belum lengkap (`RM-DEC-046`) |

## A4. Hak akses baru

Terdaftar otomatis. Peran yang belum diberi izin tidak melihat perubahan apa pun.

| Izin | Untuk |
|---|---|
| `MedicalRecordCompleteness : Read` | Daftar kerja dan rincian kelengkapan berkas |
| `MedicalRecordCompleteness : Evaluate` | Menilai ulang satu kunjungan |
| `MedicalRecordCompletenessProfile : Read` / `Create` / `Update` | Master profil dokumen wajib |

`MedicalRecord : Read` **tidak berubah**, tetapi isinya kini lebih luas: dokumen final seluruh
modul ikut terbaca. Tinjau ulang siapa yang memegangnya.

## A5. Yang WAJIB disiapkan sebelum rilis dipakai

| Butir | Keadaan | Akibat bila belum siap |
|---|---|---|
| Migration `20261028070658_AddMedicalRecordCompletenessFoundation` | **Sudah diterapkan di devYoga** 28 Oktober 2026. Lingkungan lain belum; penerapannya butuh instruksi terpisah | Penutupan kunjungan pada jalur bertransaksi gagal karena tabel tutup klinis tidak ada |
| Sebelas profil dokumen wajib awal | Diisi seeder `MedicalRecordCompletenessProfileSeeder` saat aplikasi mulai; idempoten | Tanpa profil, setiap kunjungan `Undetermined` (`RuleNotFound`) |
| Penyelarasan profil dengan SOP rumah sakit | **Belum.** Isi awal mengikuti `RM-DEC-047`, `053`–`056`, `060`, `061`, **bukan** SOP | Daftar kerja menuntut dokumen menurut rancangan, belum menurut kebijakan rumah sakit |
| Pemberian izin A4 | Belum | Menu kelengkapan dan master profil tidak terlihat |
| Penilaian ulang manual kunjungan beroperasi atau ber-HD | Disarankan, lihat A7 butir 3 | Kunjungan yang dulu `Complete` karena asumsi sementara tetap `Complete` |
| Layar `FE-10`–`FE-14` | Direncanakan, berjalan paralel dengan API tiruan | Penanda baru (A1.3) tidak tampil sampai layarnya dirilis |

## A6. Yang TIDAK berubah

| Hal | Keadaan |
|---|---|
| Tabel modul sumber | **Nol** kolom baru. Setiap pembaca hanya satu berkas service di modul pemiliknya, ditambah pendaftaran DI, dan hanya membaca |
| Alur modul sumber | Tidak berubah, kecuali satu jalur IGD (`BE-27`, A1.4) |
| Bentuk respons yang sudah ada | Hanya bertambah field |
| Data klinis | Tidak disalin ke rekam medis; dibaca saat diminta |

## A7. Keterbatasan yang wajib diketahui

| No | Keterbatasan | Penanganan |
|---:|---|---|
| 1 | **Hasil Patologi Anatomi belum tersedia** di berkas (`RM-DEC-058`). Pemeriksaan PA yang belum dirilis tetap dihitung "menunggu hasil" | Tampil di `unavailableKinds` |
| 2 | **Asuhan gizi dapat berubah tanpa riwayat** (`RM-DEC-073`); waktu finalnya waktu simpan pertama, sehingga perubahan sesudah tutup klinis tidak bertanda susulan | `isProvisional = true`, wajib berlabel "Dapat berubah" |
| 3 | **Kunjungan yang dulu `Complete` karena asumsi sementara tidak dinilai ulang otomatis.** Sebelum `BE-31`/`BE-32`, kondisi operasi dan sesi hemodialisa dianggap tidak ada | Nilai ulang lewat `POST .../encounters/{encounterId}/evaluate`, atau terima apa adanya |
| 4 | **Golongan darah tidak terikat kunjungan.** Ia tidak muncul bila linimasa disaring per kunjungan | Tampilkan golongan darah di luar penyaring kunjungan, mis. di kepala berkas |
| 5 | **Golongan darah tervalidasi belum tentu berlaku.** Ringkasannya membawa keterangan "berlaku", "ditahan", atau "tidak berlaku lagi" dari Bank Darah. Berkas **bukan** sumber golongan darah untuk transfusi | Tampilkan keterangannya utuh |
| 6 | Antrean penilaian pertama hilang saat aplikasi restart | Disusul putaran berkala |
| 7 | Dokumen yang didaftarkan langsung bertanda tangan (sesi HD, hasil lab) tidak memicu penilaian ulang seketika | Disusul putaran berkala, paling lambat 60 menit |
| 8 | Label kekurangan tidak merinci bagian: checklist tanpa nama fase; sesi HD berbunyi "Catatan sesi hemodialisa ditandatangani belum ada." | Rincian ada di linimasa; perbaikan teks milik master profil |
| 9 | Resume MCU yang sedang dikoreksi hanya ditandai lewat teks ringkasan, belum lewat field | Calon field `isUnderCorrection` pada kontrak berikutnya |
| 10 | Kunjungan `NoShow` diperlakukan sama dengan batal (`409` saat dinilai) | Belum tercatat di validation matrix |

## A8. Keterangan di Swagger

Proyek ini tidak memuat keterangan tingkat controller ke Swagger (`includeControllerXmlComments`
dimatikan, `Program.cs`), dan action tidak memakai `<summary>` XML. Karena itu keterangan amandemen
diletakkan pada **schema** — field `unavailableKinds`, `encounterId`, `isProvisional`, `isCorrected`,
`finalizedAt`, `sourceModule` pada linimasa, dan `state` pada kelengkapan. Keterangan itu dibuktikan
terbit lewat uji `ReleaseContractTests`.

## A9. Status persetujuan

**Catatan rilis amandemen ini belum disahkan.** Kontraknya sudah (`RM-DEC-062`); yang belum adalah
pernyataan bahwa butir A1, A5, dan A7 diterima untuk diumumkan kepada pemakai.

| Peran | Nama | Tanggal setuju |
|---|---|---|
| Pemilik API | Yoga Aji Pratama | — |
| Pemilik frontend | Yoga Aji Pratama | — |

---

# Rilis pertama `0.1.x` — 27 Agustus 2026

| Field | Isi |
|---|---|
| Modul | Rekam Medis (`RM-BP-001`) |
| Tanggal disusun | 27 Agustus 2026 |
| Disusun pada task | `BE-18` |
| Status persetujuan | **Disetujui** 27 Agustus 2026 — Yoga Aji Pratama selaku pemilik API (`RM-DEC-028`) |
| Dampak skema | **Aditif.** Lima tabel baru; **nol perubahan kolom** pada tabel yang sedang dipakai |
| Dampak perilaku | **Empat perubahan** pada endpoint yang sudah berjalan — lihat bagian 1 |

> **BACA BAGIAN 1 LEBIH DULU.** Bagian itu memuat perubahan yang **tidak terlihat** dari bentuk
> permintaan maupun responsnya. Klien yang tidak membacanya dapat mengira permintaannya berhasil
> padahal sebagian nilainya diabaikan.

---

## 1. Perubahan perilaku pada endpoint yang sudah berjalan

Empat perubahan berikut menyentuh endpoint yang sudah dipakai. **Tidak ada perubahan pada bentuk
permintaan maupun respons** — yang berubah hanya perilakunya.

### 1.1 `PUT` CPPT menolak catatan yang sudah terkunci

`PUT api/v1/health-services/clinical-management/patient-integrated-progress-notes/{id}`

| | |
|---|---|
| **Sebelumnya** | Catatan dapat diubah kapan saja, tanpa batas |
| **Sekarang** | Catatan yang sudah ditandatangani, atau terkunci karena kunjungannya ditutup, dijawab `400` |
| **Pesan** | *"Catatan ini sudah ditandatangani dan tidak dapat diubah. Gunakan addendum untuk membetulkan."* |

**Yang harus dilakukan klien:** menangani kode `400` baru ini, dan mengarahkan pengguna ke jalur
addendum. Pembetulan catatan terkunci memang dilakukan lewat addendum, bukan dengan mengubah
isinya — riwayat koreksi harus tetap terbaca.

### 1.2 `ProviderUserId` diabaikan pada permintaan ubah CPPT

| | |
|---|---|
| **Sebelumnya** | Nilai dari permintaan menimpa penulis catatan |
| **Sekarang** | **Diabaikan.** Penulis ditetapkan sekali saat catatan dibuat |
| **Apakah ditolak?** | **Tidak.** Permintaan tetap berhasil, nilainya sekadar tidak berpengaruh |

### 1.3 `IsReadOnlyGenerated` diabaikan pada permintaan ubah CPPT

| | |
|---|---|
| **Sebelumnya** | Nilai dari permintaan dapat melepas penanda hanya-baca |
| **Sekarang** | **Diabaikan.** Penanda hanya-baca tidak dapat dilepas lewat permintaan ubah |
| **Apakah ditolak?** | **Tidak.** Permintaan tetap berhasil, nilainya sekadar tidak berpengaruh |

**Kenapa diabaikan, bukan ditolak.** Menolak permintaan akan memutus frontend yang sedang
berjalan. Mengabaikan nilai menutup celahnya tanpa memutus siapa pun.

**Tetapi mengabaikan kiriman klien tanpa pemberitahuan bukan praktik yang baik.** Itulah sebabnya
kedua perubahan ini dinyatakan di sini dan pada halaman Swagger endpoint-nya — bukan didiamkan.

**Yang harus dilakukan klien:** berhenti mengandalkan kedua kolom itu pada permintaan ubah. Bila
frontend menampilkan nilainya sebagai kolom yang dapat disunting, kolom itu perlu dijadikan
hanya-baca supaya pengguna tidak mengira suntingannya tersimpan.

### 1.4 `PATCH` status kunjungan mengunci dokumen yang masih terbuka

`PATCH api/v1/health-services/registration-management/patient-encounters/{id}/status`

| | |
|---|---|
| **Sebelumnya** | Status kunjungan berubah, dokumen klinisnya tidak tersentuh |
| **Sekarang** | Ketika status berpindah **menuju** `Completed`, seluruh catatan berstatus draf pada kunjungan itu dikunci |
| **Akibat baru** | **Penutupan kunjungan kini dapat gagal**, yaitu bila penguncian gagal. Bila itu terjadi, status kunjungan **tidak** ikut berubah |

**Yang harus dilakukan klien:** menangani kemungkinan penutupan kunjungan gagal, dan tidak
menganggap keberhasilannya sudah pasti.

Kenapa keduanya harus gagal bersamaan: kunjungan yang tertutup sementara dokumennya masih terbuka
adalah keadaan yang dilarang. Catatan yang tertinggal terbuka tidak akan pernah terkunci lagi,
karena pemicunya — penutupan kunjungan — sudah lewat.

Perlu diketahui pula: endpoint ini **tidak** memvalidasi perpindahan status, sehingga status
dapat melompat dari nilai mana pun ke `Completed`. Penguncian karena itu dipicu oleh perpindahan
**menuju** `Completed`, bukan oleh urutan tertentu. Ini bukan perbaikan atas celah tersebut —
celah itu tetap terbuka.

---

## 2. Cakupan aturan keutuhan pada rilis ini

**Baru CPPT yang tunduk aturan keutuhan rekam medis.**

Dua belas jenis dokumen klinis lain — konsultasi, asesmen, diagnosis, tindakan, tanda vital,
alergi, riwayat penyakit, riwayat keluarga, dokumen klinis, lampiran, surat keterangan, dan
persetujuan tindakan — sudah punya nomor jenisnya, tetapi aturannya **belum ditegakkan** pada
rilis ini.

Akibatnya untuk kedua belas jenis itu:

| Hal | Keadaan |
|---|---|
| Penandatanganan dan penguncian | Belum berlaku |
| Pemeriksaan sebelum perubahan | Belum berlaku — dokumen masih dapat diubah bebas |
| Status keutuhan pada balasan API | Kosong, disertai penanda bahwa jenis itu belum tunduk aturan |

**Yang harus dilakukan klien:** setiap baris riwayat dan detail dokumen membawa penanda apakah
jenisnya sudah tunduk aturan keutuhan. **Penanda itu wajib ditampilkan.** Menampilkan alergi
seolah-olah sudah terlindungi aturan keutuhan akan membuat pembacanya mempercayai dokumen yang
sebenarnya masih dapat diubah bebas.

---

## 3. Yang baru: endpoint berkas rekam medis

Seluruhnya endpoint baru; tidak menyentuh endpoint yang sudah ada.

### Health Services / Medical Record Management / Medical Record

Base URL: `api/v1/health-services/medical-record-management/medical-records`

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/{patientId}/summary` | Ringkasan berkas: identitas, alergi aktif, diagnosis aktif, jumlah dokumen per jenis | `MedicalRecord : Read` |
| `GET` | `/{patientId}/timeline` | Riwayat gabungan tiga belas sumber, urut waktu | `MedicalRecord : Read` |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}` | Detail dokumen beserta addendum | `MedicalRecord : Read` |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}/private-note` | Membuka catatan pribadi klinisi | `MedicalRecord : ReadPrivateNote` |
| `GET` | `/filters/metadata` | Daftar pilihan penyaring dan keperluan akses | `MedicalRecord : Read` |

Grup lain yang bertambah: **Clinical Document Integrity**, **Clinical Note Addendum**,
**Clinical Note Author Delegation**, **Medical Record Access Log**, dan **Medical Record Access
Purpose**.

### Tiga aturan yang mengikat seluruh endpoint berkas rekam medis

**1. Setiap pembukaan dicatat lebih dulu.** Jejak akses ditulis **sebelum** isi dikembalikan.
Bila pencatatan gagal, permintaan dijawab `503` dan isi **tidak** dikembalikan. Ini pilihan yang
menutup rapat: membaca diam-diam dinilai lebih berbahaya daripada tidak bisa membaca.

**2. Pasien di luar rawatan menuntut keperluan akses.** Bila pasien tidak sedang punya kunjungan
berjalan, `accessPurposeId` wajib diisi. Tanpa itu, permintaan dijawab `400`.

**3. Catatan pribadi selalu menuntut keperluan akses** — bahkan untuk pasien yang sedang dirawat
pengguna, dan dengan izin yang terpisah dari izin baca biasa.

### Kode status yang perlu ditangani klien

| Kode | Arti |
|---|---|
| `400` | Keperluan akses belum dipilih, atau penjelasannya belum diisi |
| `403` | Tidak punya hak akses ke menu rekam medis |
| `404` | Pasien tidak ditemukan, atau dokumen bukan milik pasien itu |
| `409` | Pasien hasil penggabungan nomor rekam medis — buka nomor penggantinya |
| `503` | Jejak akses gagal dicatat, sehingga isi tidak dikembalikan. Coba lagi |

### Perubahan bentuk balasan riwayat

Balasan `/timeline` **tidak** berbentuk `PagedResult` langsung. Ia membungkusnya:

```
data.page.items      ← isi halaman
data.page.totalData  ← jumlah seluruhnya
data.access          ← keterangan pembukaan yang baru saja terjadi
data.failedSources   ← sumber yang gagal dibaca. Kosong berarti lengkap
data.isTruncated     ← ada sumber yang datanya melampaui batas
data.isComplete      ← ringkasan dua penanda di atas
```

Alasannya: riwayat digabung dari tiga belas sumber, dan bila satu sumber gagal dibaca,
kekurangannya **wajib dinyatakan**. Bentuk `PagedResult` tidak punya tempat untuk itu — dan daftar
yang kurang satu jenis dokumen yang terbaca sebagai daftar lengkap adalah kekeliruan paling
berbahaya pada berkas rekam medis.

**Yang harus dilakukan klien:** membaca `data.page.items`, dan menampilkan peringatan bila
`data.isComplete` bernilai `false`.

---

## 4. Hak akses baru

Terdaftar otomatis. Peran yang belum diberi izin tidak melihat perubahan apa pun.

| Izin | Untuk |
|---|---|
| `MedicalRecord : Read` | Membuka berkas rekam medis |
| `MedicalRecord : ReadPrivateNote` | Membuka catatan pribadi klinisi |
| `ClinicalDocumentIntegrity : Read` / `Update` | Melihat dan menandatangani keutuhan dokumen |
| `ClinicalNoteAddendum : Read` / `Create` / `CreateAsSubstitute` | Koreksi catatan terkunci |
| `ClinicalNoteAuthorDelegation : Read` / `Create` / `Update` | Penetapan penulis berhalangan |
| `MedicalRecordAccessLog : Read` / `Update` | Tinjauan jejak akses |

**Dua izin yang menuntut kehati-hatian khusus:**

`MedicalRecord : ReadPrivateNote` **tidak boleh** diberikan seluas hak baca rekam medis. Bila
diberikan seluas itu, seluruh pengaman catatan pribadi kehilangan artinya.

`MedicalRecordAccessLog : Read` menampilkan alasan akses, yang dapat mengungkap keadaan pasien —
misalnya *"konsultasi kejiwaan"*. Haknya harus lebih sempit daripada hak baca rekam medis.

---

## 5. Yang WAJIB disiapkan sebelum modul dipakai

**Skema database sudah diterapkan 27 Agustus 2026** pada `QuilvianNewDevTim01`: lima tabel baru,
nol perubahan kolom pada tabel yang sedang dipakai. Rinciannya pada `roadmap/backend-roadmap.md`
bagian "Keadaan database". Yang berikut ini adalah hal-hal yang **belum** siap.

| Butir | Akibat bila belum siap |
|---|---|
| **Isi master keperluan akses** (`MstMedicalRecordAccessPurpose`) | **Seluruh** pembukaan rekam medis pasien di luar rawatan akan ditolak — bukan karena kesalahan pengguna, melainkan karena tidak ada keperluan yang dapat dipilih. **Keadaan ini berlaku sekarang:** tabelnya sudah ada, isinya masih nol |
| **Pemberitahuan kepada penulis CPPT** | Penulis CPPT selama ini menganggap kolom Catatan Pribadi sepenuhnya pribadi. Mereka berhak tahu bahwa kolom itu dapat dibuka lewat jalur sah. Bahannya pada `roadmap/BE-15-pemberitahuan-penulis-cppt.md` |
| **Penetapan siapa yang berhak `ReadPrivateNote`** | Lihat bagian 4 |
| **Pemeriksaan jumlah pasien bernomor rekam medis ganda** | Bila ada, berkas mereka tidak dapat dibuka lewat nomor lama sejak modul dipakai. Jumlahnya perlu diketahui agar unit rekam medis tidak terkejut |

Endpoint `/filters/metadata` mengembalikan penanda `isAccessPurposeMasterEmpty` beserta
peringatan tegas bila master keperluan akses masih kosong, supaya pengguna tidak mengira
penolakan akses adalah kesalahannya sendiri.

---

## 6. Yang TIDAK berubah

Dinyatakan supaya tidak ada yang mengira harus melakukan sesuatu.

| Hal | Keadaan |
|---|---|
| Kolom pada tiga belas tabel klinis | **Nol perubahan.** Alur IGD, antrean dokter, dan farmasi tidak tersentuh |
| Bentuk permintaan dan respons endpoint yang sudah ada | Tidak berubah |
| Empat model status lama pada dokumen klinis | Tetap berlaku. Status keutuhan berjalan berdampingan, bukan menggantikan |
| Isi data klinis | Tidak dipindahkan, tidak disalin, tidak diubah |

---

## 7. Status persetujuan

**Catatan rilis ini sudah disetujui**, beserta kontrak API `0.1.0` yang naik dari `draft` menjadi
`approved`. Dicatat pada `RM-DEC-028`.

Dua hal yang secara khusus ikut disahkan:

1. Perubahan bentuk balasan `/timeline` pada bagian 3.
2. Field `access` yang ditambahkan pada seluruh balasan endpoint berkas rekam medis.

| Peran | Nama | Tanggal setuju |
|---|---|---|
| Pemilik API | Yoga Aji Pratama | 27 Agustus 2026 |
| Pemilik frontend | Yoga Aji Pratama | 27 Agustus 2026 |
| Pemilik proses / clinical governance / keamanan | Yoga Aji Pratama | 26 Agustus 2026 (`RM-DEC-027`) |

**Batas yang tetap berlaku.** Sama seperti `RM-DEC-027`: tinjauan komite medik atas aturan
penguncian, addendum, dan penetapan berhalangan, serta tinjauan pihak perlindungan data atas
kewenangan `SuperAdmin`, tanda tangan elektronik, catatan pribadi, dan masa simpan jejak akses
**belum dilakukan**. Bila kedua pihak itu kelak ditunjuk dan menghasilkan keputusan berbeda,
bagian yang bergantung padanya wajib dirombak. Risiko ini diterima secara sadar.

**Akibat langsung pengesahan ini:** gerbang paralel frontend terbuka. Sepuluh task `FE-00`
sampai `FE-09` tidak lagi tertahan kontrak.
