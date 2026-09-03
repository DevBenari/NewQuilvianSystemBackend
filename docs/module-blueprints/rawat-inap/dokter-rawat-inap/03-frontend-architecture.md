# Arsitektur Frontend — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Revision | `0.1` |
| Status | `draft` |
| Tanggal | 2 September 2026 |
| Frontend SHA | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| Masukan | [`02-backend-architecture.md`](./02-backend-architecture.md) `0.1`; [`contracts/api-contract.md`](./contracts/api-contract.md) `0.1.0`; [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) `0.1.0` |
| Peta menu seluruh modul | [`../02-module-map.md`](../02-module-map.md) bagian 3 |
| Batas tulis | Hanya dokumen blueprint |

---

## 1. Kebutuhan layar

| ID | Layar | Tujuan | Pemakai utama | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-DOK-01` | **Ruang Kerja Dokter** | Satu tempat dokter melihat dan mengerjakan seluruh dokumentasi satu pasien | DPJP, dokter jaga, konsulen | **baru** |
| `FE-DOK-02` | **Kajian Medis Awal** | Menulis, menyelesaikan, dan mengamandemen kajian medis | DPJP | **baru** |
| `FE-DOK-03` | **Catatan Perkembangan (SOAP)** | Menulis catatan harian beserta lini masanya | DPJP, dokter jaga | **baru** |
| `FE-DOK-04` | **Catatan Terpadu (CPPT)** | Membaca catatan lintas profesi dan memverifikasinya | DPJP; perawat menulis dari ruang kerjanya | **baru** |
| `FE-DOK-05` | **Riwayat Visite** | Mencatat visite sebagai peristiwa dan membaca riwayatnya | DPJP, dokter jaga, konsulen | **baru** |
| `FE-DOK-06` | **Resep dan Tindakan** | Membuat resep, mencatat tindakan, membaca status pemenuhan dari Farmasi | DPJP, dokter jaga | **baru** |
| `FE-DOK-07` | **Pemeriksaan Penunjang** | Memesan pemeriksaan lab dan membaca hasil terverifikasi | DPJP, dokter jaga | **baru** |
| `FE-DOK-08` | **Daftar Pantau Verifikasi CPPT** | Menemukan catatan yang menunggu atau lewat batas verifikasi DPJP | DPJP, supervisor klinis | **baru** |

Delapan layar, seluruhnya baru.

> **`FE-DOK-04` dan ruang kerja perawat menulis ke tempat yang sama.** CPPT memang lintas profesi.
> Yang membedakan: perawat **menulis** dari `FE-KEP-01`, DPJP **membaca dan memverifikasi** dari
> sini. Kontraknya milik sub-modul ini — `CAP-021`, `RWI-DEC-083`.

---

## 2. Peta butir menu

> Peta butir menu **seluruh modul** dipegang [`../02-module-map.md`](../02-module-map.md)
> bagian 3. Yang di bawah ini butir milik sub-modul ini saja.

### 2.1 Nol butir menu tingkat dua

`IA-INP-05` membatasi menu tingkat dua Rawat Inap pada sembilan butir, dan kesembilannya sudah
habis dipakai `episode-rawat-inap`. Sub-modul ini **tidak menambah satu butir pun** — sama seperti
`keperawatan`, dan dengan alasan yang sama: pekerjaan dokter berputar pada **satu pasien yang
sedang dirawat**, bukan pada daftar dokumen.

### 2.2 Layar anak beserta jalan masuknya

| Layar | Induk yang menjadi jalan masuk | Butir hak akses penjaga |
| --- | --- | --- |
| `FE-DOK-01` Ruang Kerja Dokter | **`FE-INP-04` Detail Episode**, dan **`FE-INP-01` Census** baris pasien | `DoctorConsultation : Read` |
| `FE-DOK-02` Kajian Medis | `FE-DOK-01` | `PatientAssessment : Create` / `Read` |
| `FE-DOK-03` SOAP | `FE-DOK-01` | `DoctorConsultation : Create` / `Read` |
| `FE-DOK-04` CPPT | `FE-DOK-01` | `PatientIntegratedProgressNote : Read` / `Verify` |
| `FE-DOK-05` Riwayat Visite | `FE-DOK-01` | `PhysicianVisit : Read` / `Create` |
| `FE-DOK-06` Resep dan Tindakan | `FE-DOK-01` | `Prescription : Create`, `PatientProcedure : Create` |
| `FE-DOK-07` Penunjang | `FE-DOK-01` | `LabOrder : Read` / `Create` |
| `FE-DOK-08` Daftar Pantau Verifikasi | **`FE-INP-09` Daftar Pantau** sebagai daftar keempat operasional | `PatientIntegratedProgressNote : Read` |

`IA-INP-01` — paling banyak tiga klik dari Beranda:

```text
Beranda → Census → baris pasien → Ruang Kerja Dokter        = 3 klik  ✔
Beranda → Daftar Pantau → daftar verifikasi                  = 2 klik  ✔
```

> **Daftar Pantau `FE-INP-09` kini menampung tiga tambahan** dari dua sub-modul: kepatuhan
> pengkajian (`FE-KEP-06`) dan verifikasi CPPT (`FE-DOK-08`), di atas empat daftar yang sudah ada.
> Karena satu layar dipakai tiga sub-modul, **urutan dan pengelompokannya wajib ditetapkan di
> `02-module-map.md`**, bukan diputuskan sendiri-sendiri.

### 2.3 Route usulan

| Route usulan | Yang wajib terjangkau dari sana |
| --- | --- |
| `…/episodes/{id}/physician` | `FE-DOK-01`, dan dari sana `FE-DOK-02` s.d. `FE-DOK-07` |
| `…/monitoring` | `FE-DOK-08` sebagai salah satu daftar |

---

## 3. Skema fitur per layar

### 3.1 `FE-DOK-01` Ruang Kerja Dokter

```text
┌──────────────────────────────────────────────────────────────┐
│ KEPALA KONTEKS — selalu terlihat                             │
│ Nama pasien · No. episode · Kamar/bed · Hari rawat ke-N      │
│ DPJP · Diagnosis kerja · ⚠ Alergi                            │
├──────────────────────────────────────────────────────────────┤
│ [Kajian Medis] [SOAP] [CPPT] [Visite] [Resep & Tindakan]     │
│ [Penunjang]                                                  │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│   Isi bagian yang sedang dipilih                             │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Kepala konteks | Identitas, lokasi, hari rawat, DPJP | `GET /episodes/{id}` | `InpatientEpisode : Read` | Tidak pernah kosong | "Data pasien tidak dapat dimuat. Jangan menulis sebelum konteks pasien tampil." **+ coba lagi** |
| Penanda alergi | Alergi tercatat | `GET /patient-allergies` | `PatientAllergy : Read` | "Belum ada alergi tercatat" | **"Riwayat alergi tidak dapat dimuat"** — ditampilkan menonjol, tidak disembunyikan |
| Diagnosis kerja | Daftar masalah terkini | `GET /patient-diagnoses` | `PatientDiagnosis : Read` | "Belum ada diagnosis" | Pesan + coba lagi |
| Penanda kewenangan | Apakah pengguna DPJP episode ini | `GET /episodes/{id}/doctor-assignments` | `InpatientEpisode : Read` | — | Tombol tulis nonaktif |

> **Dua aturan keselamatan yang mengikat.**
> Bila kepala konteks gagal dimuat, **seluruh tombol tulis nonaktif** — menulis di atas konteks
> yang belum pasti adalah cara termudah mencatat pada pasien yang salah.
> Kegagalan memuat alergi **ditampilkan**, tidak disembunyikan: ketiadaan penanda terbaca sebagai
> "tidak ada alergi", dan bagi peresepan itu berbahaya.

### 3.2 `FE-DOK-02` Kajian Medis Awal

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Formulir | Keluhan utama, riwayat, pemeriksaan, kesimpulan, rencana | `GET/POST /patient-assessments` `AssessmentType=MedicalInitial` | `PatientAssessment : Create` | Formulir kosong siap diisi | Isian **tidak hilang** |
| Daftar masalah | Diagnosis terstruktur, bukan teks bebas | `POST /patient-diagnoses` | `PatientDiagnosis : Create` | "Belum ada diagnosis" | Pesan |
| Rujukan pengkajian keperawatan | **Hanya baca**, sebagai konteks | `GET /patient-assessments?assessmentType=Initial` | `PatientAssessment : Read` | "Pengkajian keperawatan belum ada" — **bukan penghalang** | Pesan; formulir tetap dapat diisi |
| `[Selesaikan]` | Menuntaskan | `POST` | `PatientAssessment : Update` | — | Bagian kosong disebut **satu per satu** |

> **Layar ini memisahkan kajian medis dari pengkajian keperawatan secara kasatmata**, walaupun
> keduanya tersimpan pada tabel yang sama. Itulah imbalan yang harus dibayar atas keputusan
> berbagi tabel pada `02-backend-architecture.md` bagian 4.2: pembaca **tidak boleh** dapat
> mengira keduanya satu dokumen.

### 3.3 `FE-DOK-03` Catatan Perkembangan (SOAP)

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Lini masa | Setiap SOAP terurut **waktu klinis** | `GET /doctor-consultations/episodes/{id}/soap-timeline` | `DoctorConsultation : Read` | "Belum ada catatan perkembangan" + tombol menulis | Pesan + coba lagi |
| Formulir S/O/A/P | Empat bagian | `POST /doctor-consultations` | `DoctorConsultation : Create` | — | Isian tidak hilang |
| Waktu klinis | **Dapat diisi berbeda dari waktu sekarang** | Sama | Sama | Bawaan: waktu sekarang | — |
| Penanda amandemen | Baris yang pernah diubah beserta alasannya | Sama | Sama | — | — |

> **Waktu klinis dapat diisi mundur, dan itu wajib.** Dokter visite pukul 07.00 lalu menulis pukul
> 11.00 adalah keadaan normal. Memaksa waktu penulisan sebagai waktu klinis membuat lini masa
> tidak menggambarkan urutan pemeriksaan yang sebenarnya.

### 3.4 `FE-DOK-04` Catatan Terpadu (CPPT)

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Lini masa lintas profesi | Catatan dokter, perawat, dan profesi lain — **masing-masing dengan penulis dan profesinya** | `GET /patient-integrated-progress-notes/episodes/{id}` | `PatientIntegratedProgressNote : Read` | "Belum ada catatan terpadu" | Pesan + coba lagi |
| Penanda verifikasi | Menunggu, terverifikasi, atau lewat batas | Sama | Sama | **"Verifikasi tidak diwajibkan"** bila kebijakan tidak aktif | Penanda disembunyikan; pencatatan tetap jalan |
| `[Verifikasi]` | DPJP memverifikasi | `PATCH /{id}/verify` | `PatientIntegratedProgressNote : Verify` | — | — |
| Penanda penulis | **Penulis asli tetap tampil setelah diverifikasi** | Sama | Sama | — | — |

> **Nama verifikator dan nama penulis ditampilkan terpisah, dan itu bukan detail rupa.**
> `AC-CAP021-03` menuntutnya. Menampilkan hanya satu nama membuat rekam medis tidak dapat
> menunjukkan siapa yang menulis dan siapa yang menyetujui.

### 3.5 `FE-DOK-05` Riwayat Visite

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Riwayat | Visite terurut waktu visite | `GET /physician-visits/episodes/{id}` | `PhysicianVisit : Read` | **"Belum ada visite tercatat"** | Pesan + coba lagi |
| `[Catat Visite]` | Waktu, peran, catatan singkat | `POST /physician-visits` | `PhysicianVisit : Create` | — | Tombol nonaktif selama permintaan berjalan |
| Peringatan visite berdekatan | Muncul bila sudah ada visite pada jam berdekatan | Sama | Sama | — | **Peringatan, bukan penolakan** — dapat dilanjutkan |
| Tautan ke catatan | Tautan **opsional** ke SOAP atau CPPT | Sama | Sama | "Tidak ditautkan" — **bukan kekurangan** | — |

> **Layar ini tidak pernah menghitung visite dari catatan SOAP.** `INV-DOK-03`. Keadaan kosong
> berbunyi "belum ada visite tercatat" walaupun sudah ada tiga SOAP, dan itu **benar**.

### 3.6 `FE-DOK-06` Resep dan Tindakan

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Daftar resep | Resep episode beserta **status pemenuhan dari Farmasi** | `GET /prescriptions/episodes/{id}` | `Prescription : Read` | "Belum ada resep" | Pesan + coba lagi |
| Penanda jenis order | Rutin, harian, atau **obat pulang** | Sama | Sama | — | — |
| `[Buat Resep]` | Obat, dosis, aturan pakai, jenis order | `POST /prescriptions` | `Prescription : Create` | — | Dapat diulang; tidak melahirkan resep ganda |
| Status pemenuhan | **Hanya baca** — tidak ada tombol menandai diserahkan | `GET /{id}/fulfillment-status` | `Prescription : Read` | "Menunggu Farmasi" | Pesan |
| Daftar tindakan | Tindakan episode | `GET /patient-procedures/episodes/{id}` | `PatientProcedure : Read` | "Belum ada tindakan" | Pesan |
| Penanda tagihan | Keadaan pengiriman ke Billing | `GET /{id}/billing-dispatch` | `PatientProcedure : Read` | "Tidak ditagihkan" | **Penanda, bukan galat halaman** |

> **Tidak ada tombol "tandai sudah diserahkan" di layar ini, dan itu bukan kelalaian.**
> `INV-DOK-04` dan PRD `CAP-023` aturan 6 melarangnya. Menambahkannya kelak berarti melanggar
> batas kepemilikan, bukan melengkapi layar.

### 3.7 `FE-DOK-07` Pemeriksaan Penunjang

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Daftar pesanan | Pesanan lab episode beserta statusnya | `GET /lab-orders/episodes/{id}` | `LabOrder : Read` | "Belum ada pemeriksaan dipesan" | Pesan + coba lagi |
| `[Pesan Pemeriksaan]` | Jenis pemeriksaan, indikasi, prioritas | `POST /lab-orders` | `LabOrder : Create` | — | Dapat diulang |
| Hasil terverifikasi | **Hanya baca**, dari modul Laboratorium | Sama | `LabOrder : Read` | "Hasil belum keluar" | Pesan |
| Radiologi | — | — | — | **"Pemeriksaan radiologi belum tersedia di sistem"** | — |

> Keadaan radiologi ditulis apa adanya, bukan disembunyikan. Modulnya memang belum ada, dan
> menyembunyikannya membuat dokter mengira ia lupa mencari.

### 3.8 `FE-DOK-08` Daftar Pantau Verifikasi CPPT

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Daftar | Catatan menunggu atau lewat batas verifikasi | `GET /patient-integrated-progress-notes/episodes/{id}/verification-status` | `PatientIntegratedProgressNote : Read` | **"Semua catatan sudah terverifikasi"** | Pesan + coba lagi |
| Keadaan khusus | Kebijakan verifikasi tidak aktif | — | — | **"Verifikasi DPJP tidak diwajibkan, sehingga tidak ada yang dipantau."** | — |
| Tindak lanjut | Setiap baris membuka `FE-DOK-04` pasien itu | — | — | — | — |

> Keadaan kosong dibedakan tegas: **"sudah terverifikasi"** berbeda dari **"tidak diwajibkan"**,
> dan keduanya berbeda dari "gagal dimuat". Ketiganya terlihat mirip di layar tetapi artinya jauh
> berbeda bagi supervisor klinis.

---

## 4. Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md)
bagian 2, **tidak dikarang ulang di sini**.

| Aksi | DPJP | Dokter jaga | Konsulen | Perawat | Ahli gizi |
| --- | :---: | :---: | :---: | :---: | :---: |
| Membaca ruang kerja | ✔ | ✔ | ✔ | ✔ | sebagian |
| Menulis kajian medis | ✔ | — | — | — | — |
| Menulis SOAP | ✔ | ✔ | — | — | — |
| Menulis CPPT | ✔ | ✔ | ✔ | ✔ dari ruang kerjanya | — |
| **Memverifikasi CPPT** | ✔ | — | — | — | — |
| Mencatat visite | ✔ | ✔ | ✔ | — | — |
| Membuat resep | ✔ | ✔ | — | — | — |
| Mencatat tindakan | ✔ | ✔ | — | — | — |
| Memesan penunjang | ✔ | ✔ | — | — | — |
| Menandai obat diserahkan | — | — | — | — | — |
| Mengisi hasil lab | — | — | — | — | — |

> Dua baris terakhir **kosong seluruhnya**, dan itu disengaja: `INV-DOK-04` dan `INV-DOK-05`.

---

## 5. Penanganan keadaan

| Keadaan | Aturannya |
| --- | --- |
| Memuat | Setiap layar daftar punya keadaan memuat tersendiri |
| Kosong | Wajib membedakan "belum ada", "tidak diwajibkan", dan "tidak dapat dimuat" |
| Gagal | Wajib ada tombol coba lagi. Kegagalan konteks pasien **menonaktifkan seluruh tombol tulis** |
| Data basi | Ruang kerja memuat ulang konteks saat difokuskan kembali. Episode yang ternyata `Closed` mengubah layar menjadi hanya-baca **kecuali** jalur amandemen, yang tetap terbuka |
| Pengiriman ganda | Visite, resep, dan tindakan memakai `Idempotency-Key`; tombol nonaktif selama permintaan berjalan |
| Penolakan `403` | Menyebut siapa yang berwenang, bukan sekadar "akses ditolak" |
| Penolakan `422` | Menyebut keadaan episodenya |
| Kegagalan modul tujuan | Kegagalan Farmasi, Laboratorium, atau Billing ditampilkan sebagai **penanda pada barisnya**, bukan sebagai galat halaman |

---

## 6. Privasi di layar

| Aturan | Isinya |
| --- | --- |
| Kolom sensitif | Isi S/O/A/P, catatan CPPT, catatan visite, dan hasil tindakan **tidak** ditampilkan pada daftar ringkas maupun tooltip |
| Daftar pantau | Menampilkan nama pasien, penulis, dan keterlambatan. **Tidak** menampilkan isi klinis |
| Cetak | Tidak ada layar cetak pada sub-modul ini. Resume pulang milik `episode-rawat-inap` |
| Log peramban | Payload berisi kolom sensitif **MUST NOT** ditulis ke console |

---

## 7. Kewenangan UI

| Hal | Wewenang |
| --- | --- |
| Keterjangkauan layar dan induknya | **Mengikat** — bagian 2.2 |
| Sumber data tiap wilayah | **Mengikat** — bagian 3 |
| Hak akses tiap tombol | **Mengikat** — bagian 4 |
| Pemisahan kajian medis dari pengkajian keperawatan di layar | **Mengikat** — bagian 3.2 |
| Penulis dan verifikator ditampilkan terpisah | **Mengikat** — bagian 3.4 |
| Tidak adanya tombol tandai-diserahkan dan isi-hasil | **Mengikat** — bagian 3.6, 3.7 |
| Bunyi keadaan kosong dan gagal | **Mengikat** untuk maknanya; kata persisnya `DEV_DISCRETION` |
| Bentuk tab, drawer, atau accordion | `DEV_DISCRETION` |
| Warna, jarak, ikon, component library | `DEV_DISCRETION` |
| Urutan daftar di dalam `FE-INP-09` | **Ditetapkan `02-module-map.md`**, bukan di sini |

---

## 8. Ketergantungan test

| Yang dibutuhkan | Kenapa |
| --- | --- |
| Episode `Admitted` beserta DPJP-nya | Seluruh layar butuh konteks dan kewenangan |
| Peran DPJP, dokter jaga, konsulen, dan perawat **terpisah** | Menguji matriks bagian 4, terutama `Verify` |
| Kebijakan verifikasi aktif **dan** satu skenario tanpa kebijakan sama sekali | `VAL-DOK-24` |
| Master obat, tindakan, dan pemeriksaan lab | **Sudah ada** dan sudah dipakai poliklinik |
| Data master rawat inap yang layak | `RWI-UI-GAP-007` masih terbuka dan ikut menahan sub-modul ini |

---

## 9. Traceability

| Bagian | Requirement | Kontrak |
| --- | --- | --- |
| 1 kebutuhan layar | PRD 18, 19 | — |
| 2 keterjangkauan | `IA-INP-01`, `IA-INP-05` | `../02-module-map.md` bagian 3 |
| 3.1 aturan keselamatan konteks | `INV-DOK-01` | `validation-matrix.md` `VAL-DOK-01` s.d. `04` |
| 3.2 pemisahan kajian medis | `AC-CAP022-02` | `02-backend-architecture.md` bagian 4.2 |
| 3.3 waktu klinis | PRD `CAP-020` aturan 2 | `api-contract.md` bagian 1 |
| 3.4 penulis vs verifikator | `AC-CAP021-03` | `state-transition-matrix.md` bagian 3 |
| 3.5 visite tidak disimpulkan | `AC-CAP025-02`, `INV-DOK-03` | `api-contract.md` bagian 4 |
| 3.6 tanpa tombol tandai-diserahkan | `INV-DOK-04` | `validation-matrix.md` `VAL-DOK-21` |
| 3.7 tanpa isi-hasil | `INV-DOK-05` | `validation-matrix.md` `VAL-DOK-23` |
| 4 aksi per peran | — | `permission-audit-matrix.md` bagian 2 |
