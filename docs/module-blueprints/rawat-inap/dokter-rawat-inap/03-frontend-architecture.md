# Arsitektur Frontend — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Revision | `0.3` — amendment atas `0.2`, menyerap `RWI-DEC-086` s.d. `RWI-DEC-088` |
| Status | `draft` |
| Tanggal | 2 September 2026 |
| Frontend SHA | `863f24b0d1617069310c04e5770b47fd1b518b5b` (branch `HamzahV2`) — **naik dari `dec4fdeff`** |
| Masukan | [`02-backend-architecture.md`](./02-backend-architecture.md) `0.2`; [`contracts/api-contract.md`](./contracts/api-contract.md) `0.2.0`; [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) `0.2.0` |
| Masukan arsitektur domain | [`../evidence/03-hospital-domain-architecture.md`](../evidence/03-hospital-domain-architecture.md) `0.2` bagian U.2 dan Z |
| Peta menu seluruh modul | [`../02-module-map.md`](../02-module-map.md) bagian 3 |
| Batas tulis | Hanya dokumen blueprint |

---

## 0. Yang berubah dari revision `0.1`, dan kenapa ini yang paling penting

Revision `0.1` menulis delapan layar yang seluruhnya **baru**. Itu tidak lagi benar. Ruang kerja
Dokter Rawat Inap **sudah ter-commit** pada `FE@863f24b`, sudah terjangkau dari menu, dan
**memakai kontrak yang salah**.

| Yang ditemukan | Buktinya |
| --- | --- |
| Daftar pasien memakai **antrean dokter rawat jalan** | `doctor-inpatient-view.jsx` mengimpor `useDoctorQueue`, `useDoctorQueueBoard`, `useInfiniteQueueScroll`, dan `useDoctorConsultationWorkspace` dari `registration-management/doctor-queue` |
| Aksi antrean tersedia di layar rawat inap | Panggil, lewati, dan tidak hadir — aksi yang tidak punya makna bagi pasien yang berbaring di kamar |
| Butir menu berada di tempat yang salah | `menu-items.jsx` menaruh "Rawat Inap" sebagai anak butir **"Dokter"**, bersebelahan dengan "Rawat Jalan" — bukan sebagai layar anak konteks episode |
| Episode tidak pernah dibaca | Tidak ada pemanggilan census maupun episode pada seluruh berkas ruang kerja |

**Akibat yang harus dinyatakan terus terang:** layar ini dapat menampilkan pasien **rawat jalan**
dengan label "Rawat Inap", dan mengirim aksi antrean terhadap mereka. Itu bukan cacat tampilan,
melainkan risiko salah pasien. Statusnya `Conflict` pada `DOK-TRC-FE-01`, dan ia **menahan
sign-off serta rilis**.

| Yang **tidak** salah dan tetap dipakai | Buktinya |
| --- | --- |
| Komponen dasar klinis | `src/components/ui/doctor-clinical-base/` berisi kepala halaman, ringkasan, kartu pasien, konteks, tab, tabel, panel, badge, dan keadaan kosong — `Reuse with adapter` |
| Tab klinis yang sudah ada | Tab SOAP, CPPT, resep, dan tindakan dapat dipakai ulang setelah **sumber datanya** diganti |

> **Pembedaan ini menentukan besar pekerjaan.** Yang salah adalah **sumber data dan pintu masuk**,
> bukan seluruh layar. Membuang semuanya lalu menulis ulang dari nol adalah pemborosan; membiarkan
> apa adanya adalah risiko klinis.

---

## 1. Kebutuhan layar

| ID | Layar | Tujuan | Pemakai utama | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-DOK-01` | **Ruang Kerja Dokter** | Satu tempat dokter melihat dan mengerjakan seluruh dokumentasi satu pasien | DPJP, dokter jaga, konsulen | **`Conflict` — wajib rework** |
| `FE-DOK-02` | **Kajian Medis Awal** | Menulis, menyelesaikan, dan mengoreksi kajian medis | DPJP | **baru** |
| `FE-DOK-03` | **Catatan Perkembangan** | Menulis catatan harian beserta lini masanya | DPJP, dokter jaga | **rework** — tab sudah ada, sumber datanya diganti |
| `FE-DOK-04` | **Catatan Terpadu** | Membaca catatan lintas profesi dan memverifikasinya | DPJP; perawat menulis dari ruang kerjanya | **rework** — verifikasi belum ada |
| `FE-DOK-05` | **Riwayat Visite** | Mencatat visite sebagai kejadian, membatalkannya bila salah, dan membaca riwayatnya | DPJP, dokter jaga, konsulen | **baru** |
| `FE-DOK-06` | **Resep dan Tindakan** | Membuat resep, mencatat tindakan, membaca status pemenuhan dari Farmasi | DPJP, dokter jaga | **rework** |
| `FE-DOK-07` | **Pemeriksaan Penunjang** | Memesan pemeriksaan laboratorium **dan radiologi**, lalu membaca hasil final | DPJP, dokter jaga | **baru** |
| `FE-DOK-08` | **Daftar Pantau Verifikasi** | Menemukan catatan yang menunggu atau lewat batas verifikasi DPJP | DPJP, supervisor klinis | **baru** |

Delapan layar: satu `Conflict`, tiga rework, empat baru.

> **`FE-DOK-04` dan ruang kerja perawat menulis ke tempat yang sama.** Catatan terpadu memang
> lintas profesi. Yang membedakan: perawat **menulis** dari ruang kerjanya, DPJP **membaca dan
> memverifikasi** dari sini. Kontraknya milik sub-modul ini — `CAP-021`, `RWI-DEC-083`.

---

## 2. Peta butir menu

> Peta butir menu **seluruh modul** dipegang [`../02-module-map.md`](../02-module-map.md)
> bagian 3. Yang di bawah ini butir milik sub-modul ini saja.

### 2.1 Nol butir menu baru, dan satu butir yang harus dipindahkan

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| ~~Rawat Inap~~ di bawah butir **Dokter** | 2 | Dokter | `/health-services/inpatient-management/doctor-inpatient` | `FE-DOK-01` | — | **Dicabut** |

Sub-modul ini **tidak menambah satu butir menu pun**. Alasannya sama dengan `keperawatan`:
pekerjaan dokter berputar pada **satu pasien yang sedang dirawat**, bukan pada daftar dokumen.

**Butir yang sudah ter-commit di bawah "Dokter" wajib dicabut**, karena menempatkannya bersebelahan
dengan "Rawat Jalan" mengundang tepat kekeliruan yang sedang kita cegah: dokter menyangka kedua
layar itu dua rasa dari hal yang sama, padahal yang satu berbasis antrean dan yang lain berbasis
episode.

### 2.2 Layar anak beserta jalan masuknya

| Layar | Induk yang menjadi jalan masuk | Butir hak akses penjaga |
| --- | --- | --- |
| `FE-DOK-01` Ruang Kerja Dokter | **`FE-INP-01` Census** baris pasien, dan **`FE-INP-04` Detail Episode** | `InpatientCensus : Read` lalu `DoctorConsultation : Read` |
| `FE-DOK-02` Kajian Medis | `FE-DOK-01` | `PatientAssessment : Create` / `Read` |
| `FE-DOK-03` Catatan Perkembangan | `FE-DOK-01` | `DoctorConsultation : Create` / `Read` |
| `FE-DOK-04` Catatan Terpadu | `FE-DOK-01` | `PatientIntegratedProgressNote : Read` / `Verify` |
| `FE-DOK-05` Riwayat Visite | `FE-DOK-01` | `PhysicianVisit : Read` / `Create` / `Cancel` |
| `FE-DOK-06` Resep dan Tindakan | `FE-DOK-01` | `Prescription : Create`, `PatientProcedure : Create` |
| `FE-DOK-07` Penunjang | `FE-DOK-01` | `LabOrder : Read` / `Create`, `RadOrder : Read` / `Create` |
| `FE-DOK-08` Daftar Pantau Verifikasi | **`FE-INP-09` Daftar Pantau** sebagai daftar tambahan | `PatientIntegratedProgressNote : Read` |

`IA-INP-01` — paling banyak tiga klik dari Beranda:

```text
Beranda → Census → baris pasien → Ruang Kerja Dokter   = 3 klik  ✔
Beranda → Daftar Pantau → daftar verifikasi            = 2 klik  ✔
```

> **Daftar Pantau `FE-INP-09` menampung tambahan dari dua sub-modul**: kepatuhan pengkajian dan
> verifikasi catatan terpadu. Karena satu layar dipakai tiga sub-modul, **urutan dan
> pengelompokannya ditetapkan `02-module-map.md`**, bukan diputuskan sendiri-sendiri.

### 2.3 Route

| Route | Yang wajib terjangkau dari sana | Keadaan |
| --- | --- | --- |
| `…/episodes/{id}/physician` | `FE-DOK-01`, dan dari sana `FE-DOK-02` s.d. `FE-DOK-07` | **Usulan** |
| `…/doctor-inpatient` | — | **Ter-commit hari ini.** Dipertahankan sementara hanya bila dialihkan ke route berbasis episode; **tidak boleh dirilis** dalam bentuk sekarang |
| `…/monitoring` | `FE-DOK-08` sebagai salah satu daftar | Usulan |

---

## 3. Skema fitur per layar

### 3.1 `FE-DOK-01` Ruang Kerja Dokter

```text
+- Ruang Kerja Dokter - Tn. Budi ---------------------------- FE-DOK-01 -+
| EP-2026-0912  MELATI-3B  hari rawat ke-3  DPJP dr. Andi              |
| Diagnosis kerja: ...            (!) Alergi: amoksisilin              |
+-----------------------------------------------------------------------+
| [Kajian Medis] [Catatan] [Terpadu] [Visite] [Resep & Tindakan]        |
| [Penunjang]                                                          |
+-----------------------------------------------------------------------+
|                                                                       |
|   isi bagian terpilih                                                 |
|                                                                       |
+-----------------------------------------------------------------------+
| memuat -> kerangka isi, tombol tulis nonaktif                         |
| gagal  -> "Data pasien tidak dapat dimuat."          [Coba lagi]      |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Daftar pasien pintu masuk | Pasien rawat inap yang menjadi tanggung jawab dokter yang masuk | `GET /census?doctorId=...` | `InpatientCensus : Read` | Kosong → "Tidak ada pasien rawat inap atas nama Anda hari ini." Gagal → pesan beserta tombol coba lagi |
| Kepala konteks | Nomor episode, nama pasien, lokasi, hari rawat, DPJP | `GET /episodes/{id}` | `InpatientEpisode : Read` | Gagal → **seluruh tombol tulis nonaktif** beserta pesan dan tombol coba lagi |
| Penanda alergi | Alergi tercatat | `GET /patient-allergies` | `PatientAllergy : Read` | Kosong → "Belum ada alergi tercatat". Gagal → **"Riwayat alergi tidak dapat dimuat"**, ditampilkan menonjol |
| Diagnosis kerja | Daftar masalah terkini | `GET /patient-diagnoses` | `PatientDiagnosis : Read` | Kosong → "Belum ada diagnosis" |
| Penanda kewenangan | Apakah pengguna berwenang atas pasien ini | `GET /episodes/{id}/doctor-assignments` | `InpatientEpisode : Read` | Tidak berwenang → tombol tulis nonaktif beserta keterangan siapa yang berwenang |

> **Dua aturan keselamatan yang mengikat.**
> Bila kepala konteks gagal dimuat, **seluruh tombol tulis nonaktif** — menulis di atas konteks
> yang belum pasti adalah cara termudah mencatat pada pasien yang salah.
> Kegagalan memuat alergi **ditampilkan**, tidak disembunyikan: ketiadaan penanda terbaca sebagai
> "tidak ada alergi", dan bagi peresepan itu berbahaya.

#### 3.1.1 Yang wajib dihapus dari ruang kerja yang sudah ter-commit

| Yang dihapus | Kenapa |
| --- | --- |
| Sumber daftar dari antrean dokter rawat jalan | Pasien menginap tidak pernah masuk antrean — `RWI-RULE-026` aturan 2 |
| Aksi panggil, lewati, dan tidak hadir | Tidak punya makna bagi pasien yang berbaring di kamar, dan mengubah baris antrean milik alur lain |
| Penyaring "tanggal hari ini" bawaan antrean | Perawatan berjalan berhari-hari; menyaring pada hari ini menyembunyikan pasien yang sedang dirawat |
| Ketergantungan pada nomor antrean sebagai kunci baris | Kunci yang benar adalah episode dan kunjungan |

| Yang dipertahankan | Kenapa |
| --- | --- |
| Komponen dasar klinis pada `doctor-clinical-base` | Bentuknya netral terhadap sumber data — `Reuse with adapter` |
| Tab SOAP, catatan terpadu, resep, dan tindakan | Isinya benar; yang diganti adalah sumber datanya |

### 3.2 `FE-DOK-02` Kajian Medis Awal

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Formulir | Anamnesis, pemeriksaan fisik, penilaian, rencana | `POST /patient-assessments` jenis kajian medis | `PatientAssessment : Create` | Gagal → isian **tidak hilang** |
| Daftar masalah | Diagnosis terstruktur, bukan teks bebas | `POST /patient-diagnoses` | `PatientDiagnosis : Create` | Kosong → "Belum ada diagnosis" |
| Rujukan pengkajian keperawatan | **Hanya baca**, sebagai konteks | `GET /patient-assessments?assessmentType=Initial` | `PatientAssessment : Read` | Kosong → "Pengkajian keperawatan belum ada" — **bukan penghalang** |
| Tombol Selesaikan | Menuntaskan kajian | `PATCH /{id}/complete` | `PatientAssessment : Update` | Gagal → bagian yang kosong disebut **satu per satu** |

> **Layar ini memisahkan kajian medis dari pengkajian keperawatan secara kasatmata**, walaupun
> keduanya tersimpan pada tabel yang sama. Itulah imbalan yang dibayar atas keputusan berbagi tabel
> pada `02-backend-architecture.md` bagian 4.2: pembaca **tidak boleh** dapat mengira keduanya satu
> dokumen.

### 3.3 `FE-DOK-03` Catatan Perkembangan

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Lini masa | Setiap catatan terurut **waktu klinis** | `GET /doctor-consultations/episodes/{id}/soap-timeline` | `DoctorConsultation : Read` | Kosong → "Belum ada catatan perkembangan" beserta tombol menulis |
| Formulir S/O/A/P | Empat bagian | `POST /doctor-consultations`, `PATCH /{id}/soap` | `DoctorConsultation : Create` / `Update` | Gagal → isian tidak hilang |
| Waktu pemeriksaan | **Dapat diisi berbeda dari waktu sekarang** | Sama | Sama | Bawaan waktu sekarang; di luar batas wajar ditolak beserta keterangannya |
| Penanda koreksi | Baris yang pernah dikoreksi beserta alasannya. **Penulis asli tetap tampil sebagai penulis catatan**; dokter pengganti hanya tampil pada baris koreksinya beserta penandanya | `GET /clinical-note-addendums/by-document/...` | `ClinicalNoteAddendum : Read` | Kosong → tidak ada penanda |
| Tombol Koreksi | Muncul hanya bila pengguna berwenang mengoreksi dokumen itu | `GET /clinical-note-addendums/authority/...` | `ClinicalNoteAddendum : Create` atau `CreateAsSubstitute` | Tidak berwenang → tombol **disembunyikan**, bukan ditampilkan lalu ditolak |

> **Waktu pemeriksaan dapat diisi mundur, dan itu wajib.** Dokter visite pukul 07.40 lalu menulis
> pukul 11.00 adalah keadaan normal. Memaksa waktu penulisan sebagai waktu klinis membuat lini masa
> tidak menggambarkan urutan pemeriksaan yang sebenarnya.
>
> **Tidak ada tombol Sunting setelah catatan diselesaikan.** Menekan Selesai adalah tanda tangan
> penulis — `RWI-DEC-086`. Sejak saat itu satu-satunya jalan membetulkan adalah tombol Koreksi,
> dan layar wajib mengatakannya sebelum dokter menekan Selesai, bukan sesudahnya. Sebelum
> diselesaikan, keadaannya justru terbalik: catatan disunting langsung, dan tombol Koreksi tidak
> ada.
>
> **Layar penetapan berhalangan bukan milik sub-modul ini.** Penerbitannya milik kepala unit lewat
> grup penetapan pada `MedicalRecordManagement` — `api-contract.md` bagian 9.1. Yang menjadi
> tanggung jawab ruang kerja dokter hanya menampilkan tombol Koreksi ketika kewenangannya memang
> ada.

### 3.4 `FE-DOK-04` Catatan Terpadu

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Lini masa lintas profesi | Catatan seluruh profesi, masing-masing beserta penulis dan profesinya | `GET /patient-integrated-progress-notes/episodes/{id}` | `PatientIntegratedProgressNote : Read` | Kosong → "Belum ada catatan terpadu" |
| Penanda verifikasi | Menunggu, terverifikasi, atau lewat batas | Sama | Sama | Kebijakan tidak aktif → **"Verifikasi tidak diwajibkan"** |
| Tombol Verifikasi | DPJP memverifikasi | `PATCH /{id}/verify` | `PatientIntegratedProgressNote : Verify` | Tidak berhak → tombol **disembunyikan**, bukan ditampilkan lalu ditolak |
| Penanda penulis | **Penulis asli tetap tampil setelah diverifikasi** | Sama | Sama | — |

> **Nama verifikator dan nama penulis ditampilkan terpisah, dan itu bukan detail rupa.**
> `AC-CAP021-03` menuntutnya. Menampilkan hanya satu nama membuat rekam medis tidak dapat
> menunjukkan siapa yang menulis dan siapa yang menyetujui.

### 3.5 `FE-DOK-05` Riwayat Visite

```text
+- Riwayat Visite - Tn. Budi -------------------------------- FE-DOK-05 -+
| [+ Catat Visite]                                                      |
+-----------------------------------------------------------------------+
| 12 Sep 07:40  dr. Andi   DPJP       tertaut: catatan pagi   [Batalkan]|
| 12 Sep 16:10  dr. Andi   DPJP       tidak ditautkan         [Batalkan]|
| 11 Sep 08:05  dr. Sinta  Konsulen   tertaut: catatan        [Batalkan]|
| 11 Sep 19:20  dr. Andi   DPJP       DIBATALKAN - salah ketik jam      |
+-----------------------------------------------------------------------+
| kosong -> "Belum ada visite tercatat."                                |
| gagal  -> "Riwayat visite tidak dapat dimuat."      [Coba lagi]       |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Riwayat | Kejadian terurut waktu visite, **termasuk yang dibatalkan beserta alasannya** | `GET /physician-visits/episodes/{id}` | `PhysicianVisit : Read` | Kosong → **"Belum ada visite tercatat"** |
| Tombol Catat Visite | Waktu kedatangan, peran, catatan singkat | `POST /physician-visits` beserta kunci permintaan | `PhysicianVisit : Create` | Tombol nonaktif selama permintaan berjalan |
| Peringatan visite berdekatan | Muncul bila sudah ada visite pada jam berdekatan | Sama | Sama | **Peringatan, bukan penolakan** — dapat dilanjutkan |
| Tombol Batalkan | Membatalkan kejadian salah catat; **alasan wajib** | `PATCH /{id}/cancel` | `PhysicianVisit : Cancel` | Alasan kosong → tombol simpan nonaktif |
| Tautan ke catatan | Tautan **opsional** ke catatan, catatan terpadu, atau tindakan | `PATCH /{id}/links` | `PhysicianVisit : Update` | "Tidak ditautkan" — **bukan kekurangan** |

> **Layar ini tidak pernah menghitung visite dari catatan.** `INV-DOK-07`. Keadaan kosong berbunyi
> "belum ada visite tercatat" walaupun sudah ada tiga catatan perkembangan, dan itu **benar**.
>
> **Tidak ada tombol Sunting.** Kejadian yang salah dibatalkan beralasan lalu dicatat ulang —
> `RWI-DEC-085`. Baris yang dibatalkan **tetap terlihat**, karena menghilangkannya berarti
> menghapus jejak yang justru dibutuhkan auditor.

### 3.6 `FE-DOK-06` Resep dan Tindakan

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Daftar resep | Resep sepanjang perawatan beserta **status pemenuhan dari Farmasi** | `GET /prescriptions/episodes/{id}` | `Prescription : Read` | Kosong → "Belum ada resep" |
| Penanda jenis resep | Rutin, harian, atau **obat pulang** | Sama | Sama | — |
| Tombol Buat Resep | Obat, dosis, aturan pakai, jenis resep | `POST /prescriptions` | `Prescription : Create` | Dapat diulang; tidak melahirkan resep ganda |
| Status pemenuhan | **Hanya baca** — tidak ada tombol menandai diserahkan | Sama | `Prescription : Read` | "Menunggu Farmasi" |
| Daftar tindakan | Tindakan sepanjang perawatan | `GET /patient-procedures/episodes/{id}` | `PatientProcedure : Read` | Kosong → "Belum ada tindakan" |
| Penanda tagihan | Keadaan penerbitan fakta ke Billing | Sama | `PatientProcedure : Read` | Gagal terbit → **penanda pada barisnya, bukan galat halaman** |

> **Tidak ada tombol "tandai sudah diserahkan" di layar ini, dan itu bukan kelalaian.**
> `RUL-DOK-01` melarangnya. Menambahkannya kelak berarti melanggar batas kepemilikan, bukan
> melengkapi layar.

### 3.7 `FE-DOK-07` Pemeriksaan Penunjang

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Daftar pesanan laboratorium | Pesanan beserta statusnya | `GET /lab-orders/episodes/{id}` | `LabOrder : Read` | Kosong → "Belum ada pemeriksaan laboratorium dipesan" |
| Daftar pesanan radiologi | Pesanan, modalitas, dan jadwalnya | `GET /rad-orders/episodes/{id}` | `RadOrder : Read` | Kosong → "Belum ada pemeriksaan radiologi dipesan" |
| Tombol Pesan | Jenis pemeriksaan, indikasi, prioritas | `POST /lab-orders`, `POST /rad-orders` | `LabOrder : Create`, `RadOrder : Create` | Dapat diulang |
| Hasil final | **Hanya baca**, dari modul pemiliknya | Sama | Sama | Belum final → penanda **"belum final"**, tidak disajikan sebagai hasil sah |

> **Radiologi kini ada, dan layar ini berubah karenanya.** Revision `0.1` menuliskan
> "Pemeriksaan radiologi belum tersedia di sistem" sebagai keadaan kosong. Kalimat itu **dicabut**:
> modulnya berjalan, pesanan dan penjadwalannya sudah ada.

### 3.8 `FE-DOK-08` Daftar Pantau Verifikasi

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Daftar | Catatan menunggu atau lewat batas verifikasi | `GET /patient-integrated-progress-notes/episodes/{id}/verification-status` | `PatientIntegratedProgressNote : Read` | Kosong → **"Semua catatan sudah terverifikasi"** |
| Keadaan khusus | Kebijakan verifikasi tidak aktif | — | — | **"Verifikasi DPJP tidak diwajibkan, sehingga tidak ada yang dipantau."** |
| Tindak lanjut | Setiap baris membuka `FE-DOK-04` pasien itu | — | — | — |

> Keadaan kosong dibedakan tegas: **"sudah terverifikasi"** berbeda dari **"tidak diwajibkan"**,
> dan keduanya berbeda dari "gagal dimuat". Ketiganya terlihat mirip di layar tetapi artinya jauh
> berbeda bagi supervisor klinis.

---

## 4. Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md)
bagian 2, **tidak dikarang ulang di sini**.

| Aksi | DPJP | Dokter jaga | Konsulen | Perawat | Supervisor klinis |
| --- | :---: | :---: | :---: | :---: | :---: |
| Membaca ruang kerja | ✔ | ✔ | ✔ | ✔ | ✔ |
| Menulis kajian medis | ✔ | — | — | — | — |
| Menulis catatan perkembangan | ✔ | ✔ | — | — | — |
| Menulis catatan terpadu | ✔ | ✔ | ✔ | ✔ dari ruang kerjanya | — |
| **Memverifikasi catatan terpadu** | ✔ | — | — | — | — |
| Mencatat visite | ✔ | ✔ | ✔ | — | — |
| **Membatalkan visite** | ✔ | ✔ | ✔ | — | ✔ |
| Membuat resep | ✔ | ✔ | — | — | — |
| Mencatat tindakan | ✔ | ✔ | — | — | — |
| Memesan penunjang | ✔ | ✔ | — | — | — |
| Menandai obat diserahkan | — | — | — | — | — |
| Mengisi hasil penunjang | — | — | — | — | — |

> Dua baris terakhir **kosong seluruhnya**, dan itu disengaja: `RUL-DOK-01` dan `RUL-DOK-02`.

---

## 5. Penanganan keadaan

| Keadaan | Aturannya |
| --- | --- |
| Memuat | Setiap layar daftar punya keadaan memuat tersendiri berupa kerangka baris, bukan layar kosong |
| Kosong | Wajib membedakan "belum ada", "tidak diwajibkan", dan "tidak dapat dimuat" |
| Gagal | Wajib ada tombol coba lagi. Kegagalan konteks pasien **menonaktifkan seluruh tombol tulis** |
| Data basi | Ruang kerja memuat ulang konteks saat difokuskan kembali. Perawatan yang ternyata sudah ditutup mengubah layar menjadi hanya-baca **kecuali** jalur koreksi, yang tetap terbuka |
| Pengiriman ganda | Visite, resep, dan tindakan memakai kunci permintaan; tombol nonaktif selama permintaan berjalan |
| Penolakan `403` | Menyebut siapa yang berwenang, bukan sekadar "akses ditolak" |
| Penolakan `422` | Menyebut keadaan perawatannya |
| Kegagalan modul tujuan | Kegagalan Farmasi, Laboratorium, Radiologi, atau Billing ditampilkan sebagai **penanda pada barisnya**, bukan sebagai galat halaman |
| Hasil belum final | Ditampilkan dengan penanda; **tidak boleh** terlihat sama dengan hasil final |

---

## 6. Privasi di layar

| Aturan | Isinya |
| --- | --- |
| Kolom sensitif | Isi catatan, catatan terpadu, catatan visite, alasan pembatalan, dan hasil tindakan **tidak** ditampilkan pada daftar ringkas maupun tooltip |
| Daftar pantau | Menampilkan nama pasien, penulis, dan keterlambatan. **Tidak** menampilkan isi klinis |
| Cetak | Tidak ada layar cetak pada sub-modul ini. Resume pulang milik `episode-rawat-inap` |
| Log peramban | Payload berisi kolom sensitif **MUST NOT** ditulis ke console |

---

## 7. Kewenangan UI

| Hal | Wewenang |
| --- | --- |
| Sumber daftar pasien adalah census episode, bukan antrean | **Mengikat** — bagian 0 dan 3.1.1 |
| Ketiadaan aksi antrean pada ruang kerja rawat inap | **Mengikat** — bagian 3.1.1 |
| Keterjangkauan layar dan induknya | **Mengikat** — bagian 2.2 |
| Sumber data tiap wilayah | **Mengikat** — bagian 3 |
| Hak akses tiap tombol | **Mengikat** — bagian 4 |
| Pemisahan kajian medis dari pengkajian keperawatan di layar | **Mengikat** — bagian 3.2 |
| Penulis dan verifikator ditampilkan terpisah | **Mengikat** — bagian 3.4 |
| Baris visite yang dibatalkan tetap terlihat | **Mengikat** — bagian 3.5 |
| Ketiadaan tombol Sunting pada catatan yang sudah diselesaikan | **Mengikat** — bagian 3.3 |
| Penulis asli tetap tampil sebagai penulis walaupun koreksinya ditulis dokter pengganti | **Mengikat** — bagian 3.3 |
| Ketiadaan tombol sunting visite, tandai-diserahkan, dan isi-hasil | **Mengikat** — bagian 3.5, 3.6, 3.7 |
| Bunyi keadaan kosong dan gagal | **Mengikat** untuk maknanya; kata persisnya `DEV_DISCRETION` |
| Bentuk tab, drawer, atau accordion | `DEV_DISCRETION` |
| Warna, jarak, ikon, component library | `DEV_DISCRETION` |
| Urutan daftar di dalam `FE-INP-09` | **Ditetapkan `02-module-map.md`**, bukan di sini |

---

## 8. Ketergantungan test

| Yang dibutuhkan | Kenapa |
| --- | --- |
| Episode berjalan beserta DPJP-nya | Seluruh layar butuh konteks dan kewenangan |
| Peran DPJP, dokter jaga, konsulen, perawat, dan supervisor **terpisah** | Menguji matriks bagian 4, terutama `Verify` dan `Cancel` |
| Kebijakan verifikasi aktif **dan** satu skenario tanpa kebijakan sama sekali | `VAL-DOK-24` |
| Master obat, tindakan, pemeriksaan laboratorium, dan modalitas radiologi | **Sudah ada** dan sudah dipakai poliklinik |
| Data master rawat inap yang layak | `RWI-UI-GAP-007` masih terbuka dan ikut menahan sub-modul ini |

---

## 9. Traceability

| Bagian | Requirement | Kontrak |
| --- | --- | --- |
| 0 konflik ruang kerja | `DOK-TRC-FE-01` | Arsitektur domain bagian Z.1 dan `AA` `ARCH-GAP-013` |
| 2 keterjangkauan | `IA-INP-01`, `IA-INP-05` | `../02-module-map.md` bagian 3 |
| 3.1 aturan keselamatan konteks | `INV-DOK-01`, `INV-DOK-02` | `validation-matrix.md` `VAL-DOK-01` s.d. `VAL-DOK-04`, `VAL-DOK-26` |
| 3.2 pemisahan kajian medis | `AC-CAP022-02` | `02-backend-architecture.md` bagian 4.2 |
| 3.3 waktu klinis | PRD `CAP-020` aturan 2 | `api-contract.md` bagian 1 |
| 3.4 penulis dan verifikator | `AC-CAP021-03`, `INV-DOK-11` | `state-transition-matrix.md` bagian 3 |
| 3.5 visite | `RWI-AC-150` s.d. `RWI-AC-156` | `api-contract.md` bagian 4; `flowcharts/02-visite-dokter.md` |
| 3.6 tanpa tombol tandai-diserahkan | `RUL-DOK-01` | `validation-matrix.md` `VAL-DOK-21` |
| 3.7 radiologi dan hasil final | `AC-CAP015-01`, `AC-CAP015-02`, `INV-DOK-12` | `api-contract.md` bagian 7 dan 8 |
| 4 aksi per peran | — | `permission-audit-matrix.md` bagian 2 |
