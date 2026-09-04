# Catatan Rilis — Modul Rekam Medis

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
