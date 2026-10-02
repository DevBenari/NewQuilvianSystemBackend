# Arsitektur Frontend — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Revision | **`0.4`** — amandemen penyelarasan `PRD-RWI-V2-001`, blueprint revision `7`; isi baru pada **bagian 10**, yang menggantikan butir tertentu bagian 0 s.d. 9 sebagaimana tabel 10.1. Revision `0.3` menyerap `RWI-DEC-086` s.d. `RWI-DEC-088` |
| Status | **`draft`** untuk `0.4`. `0.3` `approved` — disetujui Muhammad Hamzah, 2026-09-03 |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-03** |
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

---

## 10. Amandemen revision `0.4` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

| Field | Nilai |
| --- | --- |
| Status | **`draft`** — belum disetujui manusia |
| Frontend SHA | `1ce219b40f8e411f3c4e66975626ab33ae81616a` (branch `HamzahV2`); satu commit sesudah `147355f5`, hanya gaya sticky/scroll dan test |
| Masukan | `02-backend-architecture.md` `0.5` bagian 11; `contracts/api-contract.md` `0.6.0` bagian 12; `contracts/permission-audit-matrix.md` `0.6.0` bagian 7; decision log revision `21`; `PRD-RWI-V2-001` bagian 7–23, 49–55, 61, 65 |
| Bukti keadaan saat ini | `../01-existing-capability-map.md` bagian 17 `RLN3-CAP-03`, `RLN3-CAP-04`, `RLN3-CAP-23`, `RLN3-CAP-28`, `RLN3-CAP-31` |

### 10.1 Yang digantikan dari bagian 0 s.d. 9

| Bagian lama | Pernyataan lama | Pengganti | Dasar |
| --- | --- | --- | --- |
| 1 `FE-DOK-01` | Ruang kerja satu pasien, dicapai dari Census dan Detail Episode | **`FE-DOK-09`** — halaman berdiri sendiri, daftar pasien di kiri, ruang kerja pasien di kanan, susunan **sama persis** dengan Dokter Rawat Jalan V2 | `RWI-DEC-107` |
| 1 `FE-DOK-06` | Resep dan Tindakan satu layar | **Dua tab**: `FE-DOK-10` Resep dan `FE-DOK-11` Tindakan | PRD v`2.0` bagian 15, `RWI-DEC-110` |
| 1 `FE-DOK-07` | Penunjang laboratorium dan radiologi | **`FE-DOK-13`** — enam layanan; empat berupa "Integrasi belum tersedia" | `RWI-DEC-108`, `RWI-DEC-113` |
| 2.1 | Butir Dokter → Rawat Inap **dicabut** | **Dipertahankan** sebagai jalan masuk `FE-DOK-09` | `RWI-DEC-107` |
| 2.2 | `FE-DOK-01` layar anak Census/Detail Episode | Census dan Detail Episode **menautkan** ke `FE-DOK-09` dengan pasien terpilih; bukan lagi induk | `RWI-DEC-107` |
| 2.3 | Route `…/episodes/{id}/physician` usulan | Route tunggal `…/doctor-inpatient?episodeId={id}`; route lama dialihkan | Bagian 10.3 |
| 4 | Dokter jaga dan konsulen tidak memverifikasi karena peran | Tombol verifikasi tampil bagi setiap dokter, tetapi aktif hanya bila backend menyatakan dokter itu DPJP aktif atau DPJP terakhir | `RWI-DEC-125`, `RWI-DEC-126`; `permission-audit-matrix.md` bagian 7 |
| 4 | Perawat tidak memesan tindakan dan penunjang | Perawat memesan dari ruang kerja **keperawatan**; dari ruang kerja dokter perawat tetap hanya membaca | `RWI-DEC-114` |
| Seluruh ruang kerja | Komponen dari `src/components/ui/doctor-clinical-base` | Komponen tata letak Dokter Rawat Jalan yang diekstraksi menjadi komponen berbasis props — `INT-DOK-21` | `UI-AC-DOK-009`, `RLN3-CAP-03` |

Layar `FE-DOK-02` Kajian Medis, `FE-DOK-03` Catatan Perkembangan, `FE-DOK-04` Catatan Terpadu, `FE-DOK-05` Riwayat
Visite, dan `FE-DOK-08` Daftar Pantau Verifikasi **tetap** ada; isinya kini menjadi tab pada `FE-DOK-09` beserta
tambahan bagian 10.4.

### 10.2 Kebutuhan layar revision `0.4`

| ID | Layar | Tujuan | Pemakai utama | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-DOK-09` | **Ruang Kerja Dokter Rawat Inap** | Satu halaman: daftar pasien milik dokter, konteks pasien, delapan tab | DPJP, konsulen, dokter jaga | **Baru** — menggantikan `FE-DOK-01`; `Conflict` `RLN3-CAP-04` diselesaikan di sini |
| `FE-DOK-03` | Tab **SOAP** | Form SOAP, Riwayat SOAP, Koreksi | Dokter berpenugasan | Rework — penulis tunggal konsep, penguncian |
| `FE-DOK-04` | Tab **CPPT** | Lini masa lintas profesi dengan saring Semua/Dokter/Perawat/Profesi Lain; verifikasi | DPJP memverifikasi; semua dokter membaca | Rework — jenis catatan, verifikasi DPJP |
| `FE-DOK-02` | Tab **Kajian Pasien** | Riwayat kajian, Kajian Baru, rujukan pengkajian keperawatan | Dokter berpenugasan | Rework |
| `FE-DOK-10` | Tab **Resep** | Buat Resep, Template Resep, History Resep, Resep Harian, Rekonsiliasi Obat, Sliding Scale | Dokter berpenugasan | **Baru** sebagai tab tersendiri |
| `FE-DOK-11` | Tab **Tindakan** | Form Tindakan, Riwayat Tindakan, pesanan yang menunggu verifikasi instruksi | Dokter berpenugasan | **Baru** sebagai tab tersendiri |
| `FE-DOK-12` | Tab **Resume Medis** | Resume Rawat Inap, Resume ODC, History Resume | DPJP menandatangani; dokter lain membaca | **Baru** — permukaan `CAP-026` |
| `FE-DOK-05` | Tab **Visit** | Riwayat visit dan Catat Visit | Dokter berpenugasan | Tetap |
| `FE-DOK-13` | Tab **Penunjang Medis** | Landing enam kartu; Laboratorium dan Radiologi berisi pesanan dan hasil final | Dokter berpenugasan | **Baru** — menggantikan `FE-DOK-07` |
| `FE-DOK-14` | **Catatan Saya** | Konsep dan catatan terkunci milik dokter login; tambah addendum | Seluruh dokter | **Baru** — `RWI-DEC-127`, `RWI-DEC-142` |
| `FE-DOK-15` | **Perlu Review** | Gabungan entri CPPT menunggu verifikasi dan pesanan perawat menunggu verifikasi instruksi milik dokter login | DPJP dan dokter pemberi instruksi | **Baru** |
| `FE-DOK-16` | **Protokol Sliding Scale** | Kelola template berversi; ubah draft dan sahkan | Pengubah dan pengesah konfigurasi farmasi-klinis | **Baru** — butir menu di grup Farmasi |
| `FE-DOK-08` | Daftar Pantau Verifikasi | Tetap pada `FE-INP-09`; kini memuat episode `Closed` milik DPJP terakhir | DPJP, supervisor klinis | Rework |

### 10.3 Peta butir menu dan jalan masuk — butir milik sub-modul ini

Peta seluruh modul dipegang [`../02-module-map.md`](../02-module-map.md) bagian 3 revision `2`.

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Rawat Inap | 1 | **Dokter** (bukan menu tingkat dua Rawat Inap) | `/health-services/inpatient-management/doctor-inpatient` | `FE-DOK-09` | `InpatientCensus : Read` | **Sudah ada** di `menu-items.jsx` baris 972–977; dipertahankan, layarnya dibangun ulang |
| Protokol Sliding Scale | 1 | **Farmasi** | `/health-services/pharmacy-management/sliding-scale-templates` | `FE-DOK-16` | `SlidingScaleTemplate : Read` | **Baru** |

**Kuota `IA-INP-05` tidak terpakai.** Kedua butir berada di grup Dokter dan Farmasi, bukan menu tingkat dua Rawat Inap
yang sudah berisi sembilan butir — diverifikasi pada `menu-items.jsx`, sesuai catatan `RWI-DEC-107`.

| Layar anak | Jalan masuk | Butir hak akses penjaga |
| --- | --- | --- |
| Tab `FE-DOK-02`, `03`, `04`, `05`, `10`, `11`, `12`, `13` | `FE-DOK-09` setelah pasien dipilih | Per tab, lihat skema 10.4 |
| `FE-DOK-14` Catatan Saya | Tombol "Catatan Saya" pada kepala `FE-DOK-09`, terlihat tanpa memilih pasien; juga dari halaman Rekam Medis `my-unsigned-notes` yang sudah ada | `ClinicalDocumentIntegrity : Read` |
| `FE-DOK-15` Perlu Review | Kartu metrik "Perlu Review" pada kepala `FE-DOK-09` | `PatientIntegratedProgressNote : Read`, `PatientProcedure : Read` |
| `FE-DOK-09` dengan pasien terpilih | Baris Census `FE-INP-01` dan tombol "Buka Ruang Kerja Dokter" pada Detail Episode `FE-INP-04` → `…/doctor-inpatient?episodeId={id}` | Pasien hanya terpilih bila ada pada daftar dokter login; bila tidak, halaman menampilkan "Pasien ini tidak ada pada daftar Anda" |
| `…/episodes/{id}/physician` | Route lama dialihkan ke `…/doctor-inpatient?episodeId={id}` | — |

```text
Beranda → Dokter → Rawat Inap → kartu pasien → tab               = 3 klik  ✔ IA-INP-01
Beranda → Dokter → Rawat Inap → Catatan Saya                     = 3 klik  ✔
Beranda → Farmasi → Protokol Sliding Scale                       = 2 klik  ✔
```

### 10.4 Skema fitur per layar

Skema mengunci **isi dan sumber data**. Warna, jarak, ikon, dan component library tetap `DEV_DISCRETION`, **kecuali**
kesamaan tata letak dengan Dokter Rawat Jalan yang diwajibkan `UI-AC-DOK-001` s.d. `012`. Seluruh keadaan kosong dan
gagal ditulis maknanya; kata persisnya `DEV_DISCRETION`.

#### 10.4.1 `FE-DOK-09` Ruang Kerja Dokter Rawat Inap

```text
+- Dokter Rawat Inap ------------------------------------------------------------------------+
| Kelola pelayanan dan dokumentasi klinis pasien selama episode rawat inap.                  |
|                         [Total Pasien 2] [Dirawat 2] [Discharge Pending 0] [Perlu Review 3] |
|                                                                        [Catatan Saya]      |
+------------------------------+-------------------------------------------------------------+
| Daftar Pasien Rawat Inap [2] | Budi Santoso • RM 00-12-34-56 • Laki-laki • 57 tahun        |
| [cari nama / No.RM / kamar]  | Episode RWI-20260914-001 • Hari Rawat ke-3                  |
|------------------------------| Mawar 302 • Bed 2 • Kelas I • DPJP dr. Rina • BPJS          |
| > Budi Santoso    [AKTIF]    | Alergi: Penicillin • Diagnosis: Pneumonia                   |
|   RM 00-12-34-56             | ⚠ Alergi Penicillin  ⚠ Risiko Jatuh Tinggi  ⚠ Isolasi       |
|   Mawar 302 • Bed 2 • Kls I  |-------------------------------------------------------------|
|   DPJP: dr. Rina             | SOAP | CPPT | KAJIAN PASIEN | RESEP | TINDAKAN |            |
|   Hari Rawat ke-3            | RESUME MEDIS | VISIT | PENUNJANG MEDIS                      |
|   Peran saya: DPJP           |-------------------------------------------------------------|
|                              |                                                             |
|   Sari Wulandari  [ISOLASI]  |   isi tab terpilih                                          |
|   ...                        |                                                             |
+------------------------------+-------------------------------------------------------------+
| belum pilih -> "Belum Ada Pasien Dipilih. Pilih pasien Rawat Inap dari panel kiri..."       |
| daftar kosong -> "Tidak ada pasien yang sedang menjadi tanggung jawab Anda."                |
| daftar gagal -> "Daftar pasien gagal dimuat."  [Coba Lagi] — panel kanan tidak ikut hilang  |
+--------------------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala dan metrik | Judul, empat metrik; metrik tanpa data nyata **tidak ditampilkan** | `GET census/summary?assignedToMe=true` | `InpatientCensus : Read` | Gagal → metrik disembunyikan dengan penanda kecil "Ringkasan gagal dimuat"; bukan angka nol |
| Tombol Catatan Saya | Membuka `FE-DOK-14` | — | `ClinicalDocumentIntegrity : Read` | Tanpa hak → tombol disembunyikan |
| Panel daftar pasien | Nama, No. RM, kamar/bed, kelas, DPJP, hari rawat, peran saya, lencana AKTIF / DISCHARGE PENDING / ISOLASI | `GET census?assignedToMe=true&search=` | `InpatientCensus : Read` | Kosong → kalimat kosong di atas; gagal → pesan dan Coba Lagi. **Dilarang** menampilkan UUID |
| Kartu terpilih | Gaya terpilih Rawat Jalan | — | — | — |
| Konteks pasien | Identitas, episode, lokasi, kelas, DPJP, penjamin, alergi, diagnosis, alert klinis | `GET episodes/{id}`, `GET patient-allergies/active-alerts`, diagnosis utama, alert risiko jatuh dari pengkajian keperawatan | `InpatientEpisode : Read`, `PatientAllergy : Read` | Kegagalan sebagian (PRD bagian 53): alergi gagal → "Alergi gagal dimuat" **merah**, bukan "tidak ada alergi"; episode gagal → seluruh tombol tulis nonaktif |
| Delapan tab | Urutan tetap PRD bagian 15 | — | Per tab | Tab tanpa hak baca disembunyikan |
| Episode `Closed` | Pita "Episode Selesai — dokumentasi klinis hanya-baca" | `EpisodeStatus` | — | Tombol tulis disembunyikan **kecuali** addendum dan verifikasi DPJP terakhir |

**Perilaku pilihan pasien.** Mengganti pasien saat ada perubahan belum disimpan memunculkan dialog "Simpan Draft /
Tetap di Halaman / Buang Perubahan" (PRD bagian 55, `RLN3-CAP-28`). Tab yang punya perubahan belum disimpan diberi
tanda titik.

#### 10.4.2 `FE-DOK-03` SOAP — perubahan

```text
+- SOAP ------------------------------------------------------------ [+ SOAP Baru] -+
| Form SOAP | Riwayat SOAP | Koreksi                                                 |
| Status: Draft • Terakhir disimpan 14:35 • Penulis: dr. Yoga • Waktu klinis 06:30   |
| 1 Tanda Vital  2 Subjective  3 Objective  4 Assessment  5 Plan                     |
| [Simpan Draft]                                        [Selesaikan SOAP]           |
+-----------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Form | Konsep milik dokter login saja yang dapat disunting | `GET doctor-consultations/{id}`; simpan `PATCH /soap` | `DoctorConsultation : Update` | Konsep milik dokter lain tampil **hanya-baca** dengan kalimat "Konsep ini ditulis dr. X"; `403` dari server ditampilkan sama |
| Selesaikan | Menandatangani; menampilkan efeknya: "Setelah diselesaikan, catatan tidak dapat diubah dan hanya dapat dilengkapi addendum" | `PATCH /complete` | `DoctorConsultation : Update` | `409` terkunci → arahkan ke addendum |
| Riwayat SOAP | Kartu per catatan: waktu klinis, waktu tanda tangan bila berbeda, penulis, status, ringkas S/O/A/P, penanda "Tidak Ditandatangani" | `GET doctor-consultations/episodes/{id}/soap-timeline` | `DoctorConsultation : Read` | Kosong → "Belum ada SOAP pada episode ini." |
| Koreksi | Daftar addendum dan tombol tambah addendum pada catatan final/terkunci milik sendiri | Grup Clinical Note Addendum | `ClinicalNoteAddendum : Create` | Konsep → tombol tidak tampil |

#### 10.4.3 `FE-DOK-04` CPPT — perubahan

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Saring | Semua / Dokter / Perawat / Profesi Lain; tanggal; status verifikasi. Saring Perawat membedakan SOAP perawat dan narasi | `GET patient-integrated-progress-notes/episodes/{id}?professionType=&noteKind=` | `PatientIntegratedProgressNote : Read` | — |
| Lini masa | Kartu vertikal: waktu klinis, profesi, penulis, jenis catatan, sumber, status verifikasi tidak dominan | Sama | Sama | Kosong → "Belum ada catatan terpadu." Entri lama `Unspecified` berlabel "Jenis tidak tercatat" |
| Tombol Verifikasi | Tampil pada entri `Pending`/`Overdue`; aktif hanya bila respons menandai dokter login DPJP aktif atau DPJP terakhir | `PATCH /{id}/verify` | `PatientIntegratedProgressNote : Verify` | `403` → "Hanya DPJP yang sedang bertugas atas pasien ini yang dapat memverifikasi." |

#### 10.4.4 `FE-DOK-02` Kajian Pasien — perubahan

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Riwayat Kajian | Kajian Awal Medis dan Kajian Ulang #n dengan waktu, status, dokter | `GET patient-assessments/episodes/{id}?assessmentType=MedicalInitial,MedicalReassessment` | `PatientAssessment : Read` | Kosong → "Belum ada kajian medis." |
| Form | Tujuh bagian PRD bagian 18 | `POST`/`PUT`/`PATCH complete` | `PatientAssessment : Create`/`Update` | Konsep milik dokter lain hanya-baca |
| Rujukan pengkajian keperawatan | Sumber, waktu klinis, penulis, status — **hanya-baca**, tidak diimpor ke form | `GET patient-assessments/episodes/{id}?assessmentType=Initial` | `PatientAssessment : Read` | Gagal → penanda pada panel rujukan saja |

#### 10.4.5 `FE-DOK-10` Resep

```text
+- RESEP ----------------------------------------------------------------------------+
| Buat Resep | Template Resep | History Resep | Resep Harian | Rekonsiliasi | Sliding Scale |
|-----------------------------------------------------------------------------------|
| Buat Resep:                                                                        |
| +- DAFTAR OBAT ---------------+-- DRAFT RESEP ------------------------------------+ |
| | [cari obat...]              | Paracetamol 500 mg • 3×1 • 5 hari • Oral        | |
| | Paracetamol          [+]    |   ⚠ bentrok alergi: Paracetamol   [hapus]       | |
| | Cefixime             [+]    | [+ Tambah Item]    [Simpan Draft] [Selesaikan]  | |
| +-----------------------------+--------------------------------------------------+ |
| Resep Harian: [Hari ini | Minggu ini | Bulan ini | Rentang]                        |
|   Ceftriaxone 1 g IV/12 jam  — dihentikan dr. Rina, 4 Sep 09.10         [—]        |
|   Omeprazole 40 mg IV/24 jam                                     [Hentikan]        |
| Rekonsiliasi: Amlodipin 10 mg 1×1 (dicatat Ns. Siti 14.20)                          |
|   [Lanjut Sama] [Lanjut Ubah] [Hentikan]                                           |
| Sliding Scale: Insulin aspart • v2 disesuaikan • Aktif   [Sesuaikan] [Hentikan]    |
+-----------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Buat Resep | Dua kolom daftar obat dan draft; sepuluh isian minimum PRD bagian 19; resep tanpa butir tidak dapat diselesaikan | Grup Prescription yang ada | `Prescription : Create`/`Update` | Tidak ada draft → "Belum ada draft resep" dan tombol Buat |
| Template Resep | Template milik dokter login saja; buat, ubah, hapus, pakai; butir hasil pakai bertanda bentrok alergi atau tidak tersedia | `GET prescription-templates?ownerScope=Mine`, `POST /{id}/apply` | `PrescriptionTemplate : Read`/`Create`/`Update`/`Delete` | Kosong → "Anda belum punya template." Gagal pakai sebagian → butir bertanda, bukan galat halaman |
| History Resep | Resep episode per tanggal dengan status Farmasi **hanya-baca** | `GET prescriptions/episodes/{id}` | `Prescription : Read` | Kosong → "Belum ada resep." |
| Resep Harian | Saring periode; racikan terbaca; obat pulang terbedakan; butir dihentikan tetap tampil dengan penghenti dan waktunya | `GET prescriptions/episodes/{id}?period=` | `Prescription : Read` | — |
| Tombol Hentikan | Membuka isian alasan wajib | `PATCH prescriptions/items/{itemId}/stop` | `Prescription : Stop` | `403` → "Anda tidak sedang bertugas atas pasien ini." |
| Rekonsiliasi | Obat bawaan yang dicatat perawat beserta keputusan terakhir dan riwayatnya; tombol tiga keputusan | `GET medication-reconciliations/episodes/{id}`, `POST /{id}/decisions` | `MedicationReconciliation : Read`/`Decide` | Kosong → "Perawat belum mencatat obat yang dibawa pasien." Keputusan terkunci setelah resep aktif → tombol disembunyikan dengan penjelasan |
| Sliding Scale | Order episode; pesan dari versi protokol sah; rentang tersalin dan dapat disesuaikan dengan alasan wajib; sesuaikan dan hentikan | `GET sliding-scale-orders/episodes/{id}`, `GET sliding-scale-templates` | `SlidingScaleOrder : Read`/`Create`/`Update`, `SlidingScaleTemplate : Read` | Belum ada protokol sah → "Belum ada protokol sliding scale yang disahkan"; nol form yang dapat disimpan |

#### 10.4.6 `FE-DOK-11` Tindakan

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Form Tindakan | Tindakan, jumlah, prioritas, alasan klinis, instruksi | `POST patient-procedures/inpatient-orders` | `PatientProcedure : Create` | `403` konteks |
| Riwayat Tindakan | Tabel: tindakan, waktu, status klinis (`Ordered`, `Completed`, `Cancelled`), penginput, pelaksana, pemberi instruksi, status verifikasi instruksi. **Status tagihan tidak dicampur** | `GET patient-procedures/episodes/{id}` | `PatientProcedure : Read` | Kosong → "Belum ada tindakan." |
| Aksi baris | Ubah (hanya penginput), Batalkan (penginput atau DPJP aktif, alasan wajib), Laksanakan, Verifikasi instruksi (hanya pemberi instruksi) | `PUT /{id}`, `PATCH /{id}/cancel`, `PATCH /{id}/execute`, `PATCH /{id}/verify-instruction` | `PatientProcedure : Update`/`Verify` | Tombol yang tidak berwenang **disembunyikan** berdasarkan penginput dan peran yang dikembalikan respons; `403` tetap ditampilkan bila terjadi |

#### 10.4.7 `FE-DOK-12` Resume Medis

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Pilihan | Resume Rawat Inap / Resume ODC / History Resume | — | — | — |
| Resume Rawat Inap | Status; delapan bagian: Diagnosis, Ringkasan Perawatan (`ClinicalSummary`), Pemeriksaan Penting, Tindakan, Obat/Terapi, Kondisi Saat Pulang, Rencana Kontrol, Edukasi; label sumber pada isian usulan | `GET discharges/{episodeId}/summary`, `GET /summary-prefill` | `InpatientDischarge : Read` | Belum ada → form kosong dengan tombol "Isi dari data klinis" |
| Simpan dan tanda tangan | Simpan Draft; Tandatangani hanya bagi DPJP aktif; kalimat "Menandatangani resume tidak menutup episode" | `PUT /summary`, `PATCH /summary/sign` | `InpatientDischarge : Update`/`Sign` | `403` → "Resume ditandatangani DPJP yang aktif saat pasien pulang." |
| Resume ODC | "Integrasi belum tersedia" | — | — | Nol form, nol data contoh |
| History Resume | Versi resume yang pernah ditandatangani | `GET /summary?includeRevisions=true` | `InpatientDischarge : Read` | Kosong → "Belum ada versi sebelumnya." |

#### 10.4.8 `FE-DOK-13` Penunjang Medis

```text
+- PENUNJANG MEDIS ------------------------------------------------+
| [Radiologi: 2 order • 1 hasil baru] [Laboratorium: 5 order • 2 baru] |
| [Gizi — Integrasi belum tersedia]    [Rehab Medik — Integrasi belum tersedia] |
| [Hemodialisa — Integrasi belum tersedia] [Bank Darah — Integrasi belum tersedia] |
+------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kartu Laboratorium | Jumlah pesanan dan hasil final baru; daftar pesanan; hasil final dengan penanda di atas/di bawah nilai rujukan; "Lihat Detail Hasil" | `GET lab-orders/episodes/{id}` | `LabOrder : Read`/`Create` | Gagal → penanda pada kartu; kartu lain tetap tampil |
| Kartu Radiologi | Sama | `GET rad-orders/episodes/{id}` | `RadOrder : Read`/`Create` | Sama |
| Empat kartu lain | Konteks pasien dan "Integrasi belum tersedia" | **Tidak ada** | — | Nol permintaan jaringan ke modul itu |

#### 10.4.9 `FE-DOK-14` Catatan Saya

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Tab Konsep | Nama pasien, No. RM, nomor episode, jenis catatan, waktu klinis, status episode, lamanya sejak waktu klinis; **tanpa** label terlambat | `GET clinical-document-integrities/my-unsigned?serviceContext=Inpatient` | `ClinicalDocumentIntegrity : Read` | Kosong → "Tidak ada konsep yang belum ditandatangani." Gagal → pesan dan Coba Lagi, bukan daftar kosong |
| Tab Terkunci | Catatan `Signed` dan `LockedUnsigned` milik sendiri; tombol Tambah Addendum | `GET /my-authored` | `ClinicalDocumentIntegrity : Read`, `ClinicalNoteAddendum : Create` | Selama `my-authored` belum tersedia → tab menampilkan "Menunggu integrasi Rekam Medis" |
| Detail | Isi catatan milik sendiri beserta addendum | Endpoint dokumen pemiliknya | Hak baca dokumen | Detail milik penulis lain → "Anda tidak berwenang membuka catatan ini" |
| Catatan kaki sementara | "Konsep SOAP dan kajian medis belum tercakup" selama `INT-DOK-15` belum berjalan | — | — | — |

#### 10.4.10 `FE-DOK-15` Perlu Review

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| CPPT menunggu verifikasi | Per pasien: jumlah entri, entri tertua, terlambat atau tidak, termasuk episode `Closed` sebagai DPJP terakhir | `GET patient-integrated-progress-notes/verification-worklist` | `PatientIntegratedProgressNote : Read` | Kosong → "Tidak ada catatan yang menunggu verifikasi Anda." |
| Pesanan menunggu verifikasi instruksi | Tiga daftar digabung di layar: tindakan, laboratorium, radiologi; penginput dan waktu | Tiga endpoint `instruction-verification-worklist` | `PatientProcedure : Read`, `LabOrder : Read`, `RadOrder : Read` | Satu sumber gagal → penanda pada kelompoknya saja |

#### 10.4.11 `FE-DOK-16` Protokol Sliding Scale

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Daftar template | Kode, nama, versi sah berlaku, draft terbuka | `GET sliding-scale-templates` | `SlidingScaleTemplate : Read` | Kosong → "Belum ada protokol." |
| Editor versi | Satuan gula darah wajib; tabel rentang bawah-inklusif/atas-eksklusif, dosis, instruksi, penanda lapor dokter; peringatan tumpuk/lubang dari server | `POST /{id}/versions`, `PUT /versions/{versionId}` | `SlidingScaleTemplate : Update` | `400` → tunjuk baris bermasalah |
| Sahkan | Tombol tampil bagi pemegang `Approve`; nonaktif dengan penjelasan bila pengguna adalah pengubah terakhir | `POST /versions/{versionId}/approve` | `SlidingScaleTemplate : Approve` | `403` → "Pengesahan harus dilakukan pengguna lain." |
| Gerbang produksi | Pita "Isi protokol wajib disahkan pemilik klinis sebelum dipakai pasien sungguhan" | — | — | — |

### 10.5 Aksi per peran — tambahan

Diturunkan dari `contracts/permission-audit-matrix.md` bagian 7 dan 8.

| Aksi | DPJP | Konsulen | Dokter jaga | Penugasan singkat | Perawat | Apoteker |
| --- | :---: | :---: | :---: | :---: | :---: | :---: |
| Melihat pasien pada daftar | ✔ penugasan aktif | ✔ | ✔ | ✔ | — | — |
| Menulis SOAP, kajian, CPPT dokter | ✔ | ✔ | ✔ | ✔ | — | — |
| Menyelesaikan konsep **milik sendiri** | ✔ | ✔ | ✔ | ✔ | — | — |
| Menyelesaikan konsep dokter lain | — | — | — | — | — | — |
| Memverifikasi CPPT | ✔ aktif atau terakhir | — | — | — | — | — |
| Menghentikan butir resep | ✔ | ✔ | ✔ | ✔ | — | — |
| Keputusan rekonsiliasi | ✔ | ✔ | ✔ | ✔ | — | — |
| Memesan dan menyesuaikan sliding scale | ✔ | ✔ | ✔ | ✔ | — | Baca |
| Memesan tindakan dari ruang kerja dokter | ✔ | ✔ | ✔ | ✔ | Dari ruang kerja keperawatan | — |
| Membatalkan pesanan orang lain | ✔ aktif | — | — | — | — | — |
| Memverifikasi instruksi | Bila pemberi instruksi | Bila pemberi instruksi | Bila pemberi instruksi | — | — | — |
| Menandatangani resume | ✔ aktif | — | — | — | — | — |
| Menambah addendum catatan sendiri setelah penugasan berakhir | ✔ | ✔ | ✔ | ✔ | — | — |

Kewenangan menulis resep bagi konsulen dan dokter jaga mengikuti aturan menulis resep rawat inap yang berlaku —
`RWI-DEC-132` butir (2), `RWI-DEC-146` butir (3). Bila kelak aturan itu membatasi konsulen, kolom konsulen ikut
berubah tanpa mengubah layar.

### 10.6 Penanganan keadaan — tambahan

| Keadaan | Aturannya |
| --- | --- |
| Hak ditolak `401`/`403` | Tiap tab menampilkan "Anda tidak punya akses ke bagian ini" dengan nama butir hak akses, **bukan** galat umum — menutup `RLN3-CAP-23` |
| Perubahan belum disimpan | Penjaga saat pindah tab, pindah pasien, dan menutup halaman — `RLN3-CAP-28`, `AC-RWI-017` |
| Kiriman ganda | Setiap perintah tulis membawa kunci permintaan baru per aksi dan kunci yang sama saat mencoba ulang — `BR-RWI-011` |
| Data basi | Memilih kembali pasien yang sama memuat ulang konteks; `409` versi order sliding scale menampilkan "Protokol sudah diubah dokter lain" dan memuat ulang |
| Episode ditutup saat layar terbuka | Tombol tulis berubah hanya-baca pada pemuatan ulang berikutnya; konsep yang sedang disunting menampilkan "Catatan ini terkunci karena perawatan ditutup" dengan tautan Catatan Saya |
| Enum | Seluruh nilai status dibaca dari nama enum respons, bukan angka tertanam — PRD bagian 64 "magic status number" |

### 10.7 Kewenangan UI — tambahan

| Hal | Wewenang |
| --- | --- |
| Kesamaan tata letak dengan Dokter Rawat Jalan | **Mengikat** — PRD bagian 7–15, `UI-AC-DOK-001` s.d. `012` |
| Urutan delapan tab | **Mengikat** — PRD bagian 15 |
| Sumber daftar pasien dari penugasan dokter login | **Mengikat** — `RWI-DEC-111` |
| Metrik hanya dari data nyata | **Mengikat** — PRD bagian 10 |
| Empat layanan penunjang tanpa data contoh | **Mengikat** — `RWI-DEC-108` |
| Penanda "Tidak Ditandatangani" pada catatan terkunci | **Mengikat** — `RWI-DEC-138` |
| Kalimat efek menyelesaikan SOAP dan menandatangani resume | **Mengikat** untuk maknanya |
| Bentuk sub-tab di dalam Resep dan Tindakan | `DEV_DISCRETION`, dengan batas pola tab Rawat Jalan |
| Letak tombol Catatan Saya dan Perlu Review di kepala | `DEV_DISCRETION`, asal terlihat tanpa memilih pasien |

### 10.8 Ketergantungan test — tambahan

| Yang dibutuhkan | Kenapa |
| --- | --- |
| Satu dokter dengan tiga penugasan berbeda peran pada tiga pasien | Daftar pasien, verifikasi, dan tombol per peran |
| Penugasan singkat `LateDocumentation` | `RWI-AC-184`, `RWI-AC-190` |
| Episode `Closed` dengan konsep dan pesanan tertunda | Penguncian, Catatan Saya, DPJP terakhir |
| Protokol sliding scale `Draft` dan `Approved` oleh dua pengguna | `VAL-DOK-54c`, keadaan belum ada protokol |
| Pasien beralergi dan template berisi obatnya | Penanda bentrok alergi |
| Komponen Rawat Jalan yang diekstraksi | `UI-AC-DOK-*` — bergantung `INT-DOK-21` |
| Peramban Edge untuk pemeriksaan tampilan | Konvensi repository frontend |

### 10.9 Traceability revision `0.4`

| Bagian | Requirement | Kontrak |
| --- | --- | --- |
| 10.2, 10.3 | PRD v`2.0` bagian 7–15, `RWI-DEC-107` | `api-contract.md` 12.1 |
| 10.4.1 | PRD bagian 6, 10–14, 52–55; `RWI-DEC-111` | `integration-contract.md` 12.1, 12.11 |
| 10.4.2 | PRD bagian 16; `RWI-DEC-128`, `138`, `144` | `api-contract.md` 12.2; `state-transition-matrix.md` 8.1 |
| 10.4.3 | PRD bagian 17; `RWI-DEC-125`, `126`, `140` | `api-contract.md` 12.4 |
| 10.4.5 | PRD bagian 19; `RWI-DEC-121`, `122`, `132`–`135`, `145`–`147` | `api-contract.md` 12.6–12.11 |
| 10.4.6 | PRD bagian 20; `RWI-DEC-114`, `139`, `143` | `api-contract.md` 12.5 |
| 10.4.7 | PRD bagian 21; `RWI-DEC-112`, `123` | `api-contract.md` 12.14 |
| 10.4.8 | PRD bagian 23; `RWI-DEC-108`, `113` | `api-contract.md` 12.12, 12.15 |
| 10.4.9 | `RWI-DEC-127`, `142` | `api-contract.md` 12.13 |
| 10.4.11 | `RWI-DEC-146`, `147` | `api-contract.md` 12.10 |
