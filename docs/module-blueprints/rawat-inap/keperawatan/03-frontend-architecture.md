# Arsitektur Frontend — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Revision | **`0.3`** — bagian 10 penyelarasan `PRD-RWI-V2-001`, blueprint revision `7` |
| Status | **`draft`** untuk `0.3` |
| Tanggal | 2 September 2026; amandemen `0.3` 15 September 2026 |
| Frontend SHA | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| Masukan | [`02-backend-architecture.md`](./02-backend-architecture.md) `0.1`; [`contracts/api-contract.md`](./contracts/api-contract.md) `0.1.0`; [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) `0.1.0` |
| Peta menu seluruh modul | [`../02-module-map.md`](../02-module-map.md) bagian 3 |
| Batas tulis | Hanya dokumen blueprint |

---

## 1. Kebutuhan layar

Nama layar di bawah adalah **nama fungsional**, bukan nama menu.

| ID | Layar | Tujuan | Pemakai utama | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-KEP-01` | **Ruang Kerja Keperawatan** | Satu tempat bagi perawat melihat dan mengerjakan seluruh dokumentasi satu pasien | Perawat, kepala ruangan | **baru** |
| `FE-KEP-02` | **Pengkajian Keperawatan** | Mengisi, menyelesaikan, dan **mengoreksi lewat addendum** pengkajian awal maupun ulang | Perawat, kepala ruangan | **baru** |
| `FE-KEP-03` | **Lini Masa Pengkajian** | Membaca perkembangan nyeri, risiko jatuh, dan gizi dari waktu ke waktu | Perawat, DPJP, kepala ruangan | **baru** |
| `FE-KEP-04` | **Rencana Asuhan Keperawatan** | Menetapkan masalah, tujuan, rencana, dan evaluasi | Perawat, kepala ruangan | **baru** |
| `FE-KEP-05` | **Catatan Tindakan Keperawatan** | Mencatat tindakan yang sudah dilakukan | Perawat | **baru** |
| `FE-KEP-06` | **Daftar Pantau Kepatuhan Pengkajian** | Menemukan episode yang pengkajiannya belum ada atau terlambat | Kepala ruangan, supervisor | **baru** |

Enam layar, seluruhnya baru. Tidak ada satu pun yang sudah ada di frontend hari ini.

> **`FE-KEP-06` menutup lubang yang sudah lama tercatat.** Roadmap `episode-rawat-inap` mencatat
> "daftar pantau ketiga `RWI-RULE-023` belum ada" sebagai gap yang tertahan. Ia tertahan karena
> bergantung pada dokumentasi klinis — yang kini menjadi milik sub-modul ini. Daftar pantau
> keempat dan kelima sudah ada di `FE-INP-09`; yang ketiga lahir di sini.

---

## 2. Peta butir menu

> Peta butir menu **seluruh modul** dipegang [`../02-module-map.md`](../02-module-map.md)
> bagian 3, karena sidebar hanya satu untuk tiga sub-modul. Yang di bawah ini butir milik
> sub-modul ini saja.

### 2.1 Nol butir menu tingkat dua, dan itu keputusan

`IA-INP-05` membatasi menu tingkat dua Rawat Inap pada **paling banyak sembilan** butir, dan
kesembilannya sudah habis dipakai `episode-rawat-inap`. Sub-modul ini karena itu **tidak menambah
satu butir menu pun**.

Alasannya bukan sekadar kuota. Seluruh pekerjaan perawat berputar pada **satu pasien yang sedang
dirawat**, bukan pada daftar dokumen. Perawat masuk lewat pasiennya, bukan lewat menu
"pengkajian".

### 2.2 Layar anak beserta jalan masuknya

Setiap layar wajib muncul sebagai butir menu **atau** dinyatakan layar anak beserta induknya.
Berikut yang kedua.

| Layar | Induk yang menjadi jalan masuk | Butir hak akses penjaga |
| --- | --- | --- |
| `FE-KEP-01` Ruang Kerja Keperawatan | **`FE-INP-04` Detail Episode**, dan **`FE-INP-01` Census** baris pasien | `PatientAssessment : Read` |
| `FE-KEP-02` Pengkajian | `FE-KEP-01` | `PatientAssessment : Create` untuk membuat; `Read` untuk membaca |
| `FE-KEP-03` Lini Masa | `FE-KEP-01` | `PatientAssessment : Read` |
| `FE-KEP-04` Rencana Asuhan | `FE-KEP-01` | `NursingCarePlan : Read` / `Create` / `Update` |
| `FE-KEP-05` Catatan Tindakan | `FE-KEP-01` | `NursingIntervention : Read` / `Create` |
| `FE-KEP-06` Daftar Pantau Kepatuhan | **`FE-INP-09` Daftar Pantau** sebagai daftar ketiga | `PatientAssessment : Read` |

`IA-INP-01` menuntut setiap layar tercapai dari Beranda dalam paling banyak tiga klik. Diperiksa:

```text
Beranda → Census → baris pasien → Ruang Kerja Keperawatan     = 3 klik  ✔
Beranda → Daftar Pantau → daftar ketiga                        = 2 klik  ✔
```

### 2.3 Route usulan

Mengikat pada kolom kanan, bebas pada kolom kiri.

| Route usulan | Yang wajib terjangkau dari sana |
| --- | --- |
| `…/episodes/{id}/nursing` | `FE-KEP-01`, dan dari sana `FE-KEP-02` s.d. `FE-KEP-05` |
| `…/monitoring` | `FE-KEP-06` sebagai salah satu daftar |

---

## 3. Skema fitur per layar

### 3.1 `FE-KEP-01` Ruang Kerja Keperawatan

```text
┌──────────────────────────────────────────────────────────────┐
│ KEPALA KONTEKS  — selalu terlihat, tidak ikut menggulung     │
│ Nama pasien · No. episode · Kamar/bed · DPJP · Perawat PJ    │
│ Status episode · Hari perawatan ke-N · ⚠ Alergi              │
├──────────────────────────────────────────────────────────────┤
│ [Pengkajian] [Rencana Asuhan] [Tindakan] [Lini Masa]         │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│   Isi bagian yang sedang dipilih                             │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Kepala konteks | Identitas pasien, lokasi, penanggung jawab, hari perawatan | `GET /episodes/{id}` | `InpatientEpisode : Read` | Tidak pernah kosong | "Data pasien tidak dapat dimuat. Jangan mengisi catatan sebelum konteks pasien tampil." **+ tombol coba lagi** |
| Penanda alergi | Alergi yang tercatat | `GET /patient-allergies` | `PatientAllergy : Read` | "Belum ada alergi tercatat" | "Riwayat alergi tidak dapat dimuat" — **ditampilkan menonjol**, bukan disembunyikan |
| Bagian isi | Mengikuti tab yang dipilih | Lihat 3.2 s.d. 3.5 | Per bagian | Per bagian | Per bagian |

> **Aturan keselamatan pada layar ini.** Bila kepala konteks gagal dimuat, seluruh tombol tulis
> **wajib** nonaktif. Formulir kosong di atas konteks yang belum pasti adalah cara paling mudah
> mencatat sesuatu pada pasien yang salah.
>
> **Kegagalan memuat alergi ditampilkan, bukan disembunyikan.** Ketiadaan penanda alergi terbaca
> sebagai "tidak ada alergi", dan itu berbahaya bila sebenarnya hanya gagal dimuat.

### 3.2 `FE-KEP-02` Pengkajian Keperawatan

```text
┌──────────────────────────────────────────────────────────────┐
│ Jenis: (•) Pengkajian Awal  ( ) Pengkajian Ulang             │
│ Status: Belum selesai · Tenggat: 12 Sep 14:00                │
├──────────────────────────────────────────────────────────────┤
│ ▸ Kajian Umum          ▸ Risiko Jatuh    ▸ Nyeri             │
│ ▸ Skrining Gizi        ▸ Kemandirian     ▸ Edukasi           │
│ ▸ Rencana Pemulangan                                         │
├──────────────────────────────────────────────────────────────┤
│                        [Simpan]  [Selesaikan Pengkajian]     │
└──────────────────────────────────────────────────────────────┘
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Pemilih jenis | Awal atau ulang | — | — | — | — |
| Penanda tenggat | Tenggat dan keterlambatan | `GET /episodes/{id}/due-status` | `PatientAssessment : Read` | **"Batas waktu belum ditetapkan"** bila kebijakan kosong | Penanda disembunyikan; **pengisian tetap jalan** |
| Bagian isian | Tujuh kelompok | `GET /patient-assessments/{id}` | `PatientAssessment : Read` | Formulir kosong siap diisi | "Isian sebelumnya tidak dapat dimuat" |
| `[Simpan]` | Menyimpan bertahap | `POST` / `PUT` | `PatientAssessment : Create` / `Update` | — | Isian **tidak hilang**; tombol dapat ditekan ulang |
| `[Selesaikan]` | Menuntaskan | `POST` | `PatientAssessment : Update` | — | Bagian yang kosong disebut **satu per satu**, bukan "data tidak valid" |

> Penanda tenggat berbunyi "belum ditetapkan" ketika `MstClinicalAssessmentPolicy` kosong —
> `VAL-KEP-17`. Ia **tidak** boleh menampilkan angka tebakan dan **tidak** boleh menahan pengisian.

### 3.3 `FE-KEP-03` Lini Masa Pengkajian

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Lini masa | Setiap pengkajian terurut waktu | `GET /patient-assessments/episodes/{id}/timeline` | `PatientAssessment : Read` | "Belum ada pengkajian untuk pasien ini" **+ tombol membuat** | "Lini masa tidak dapat dimuat" + coba lagi |
| Kolom perkembangan | Nyeri, risiko jatuh, gizi dari waktu ke waktu | Sama | Sama | — | — |
| Penanda koreksi | Baris yang pernah dikoreksi beserta **nomor urut addendum** dan alasannya. Isi asli tetap tampil apa adanya di atasnya | Sama | Sama | — | — |

> Layar ini yang membuat `AC-CAP012-02` terlihat pengguna: nilai lama **tidak** ditimpa, dan
> perkembangannya terbaca.

### 3.4 `FE-KEP-04` Rencana Asuhan Keperawatan

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Daftar masalah | Butir `Active`, `Resolved`, `Discontinued` | `GET /nursing-care-plans/episodes/{id}` | `NursingCarePlan : Read` | "Belum ada masalah keperawatan" + tombol menambah | Pesan + coba lagi |
| Satu butir | Masalah, tujuan, rencana, evaluasi | Sama | Sama | — | — |
| Riwayat versi | Versi sebelumnya beserta penulis dan waktu **aslinya** | `GET /nursing-care-plans/items/{id}/revisions` | `NursingCarePlan : Read` | "Belum pernah diubah" | Pesan |
| `[Nyatakan Tercapai]` | Menutup butir | `PATCH …/close` | `NursingCarePlan : Update` | — | Ditolak bila belum ada evaluasi, beserta alasannya |

### 3.5 `FE-KEP-05` Catatan Tindakan Keperawatan

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Daftar tindakan | Terurut waktu tindakan, bukan waktu pencatatan | `GET /nursing-interventions/episodes/{id}` | `NursingIntervention : Read` | "Belum ada tindakan tercatat hari ini" | Pesan + coba lagi |
| Formulir | Tindakan, waktu, hasil, rujukan rencana **opsional** | `POST /nursing-interventions` | `NursingIntervention : Create` | — | Isian tidak hilang |
| Penanda tagihan | Keadaan pengiriman ke Billing | `GET /{id}/billing-dispatch` | `NursingIntervention : Read` | "Tidak ditagihkan" | **Kegagalan tagihan ditampilkan sebagai penanda, bukan sebagai galat halaman** |

> **Penanda tagihan tidak boleh membuat layar terlihat rusak.** `AC-CAP014-02`: catatan klinisnya
> tersimpan; yang gagal hanya pengirimannya. Menampilkannya sebagai galat halaman akan membuat
> perawat mengira tindakannya tidak tercatat lalu mencatatnya dua kali.

### 3.6 `FE-KEP-06` Daftar Pantau Kepatuhan Pengkajian

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Daftar | Episode yang pengkajian awalnya belum ada atau lewat tenggat | `GET /patient-assessments/episodes/{id}/due-status` per episode census | `PatientAssessment : Read` | **"Seluruh pengkajian sudah tepat waktu"** — bukan "tidak ada data" | Pesan + coba lagi |
| Keadaan khusus | Kebijakan belum diisi | — | — | **"Batas waktu pengkajian belum ditetapkan, sehingga keterlambatan belum dapat dihitung."** | — |
| Tindak lanjut | Setiap baris membuka `FE-KEP-01` pasien itu | — | — | — | — |

> Keadaan kosong berbunyi **"sudah tepat waktu"**, bukan "tidak ada data". Keduanya terlihat sama
> di layar tetapi artinya berlawanan bagi kepala ruangan.

---

## 4. Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md)
bagian 2, **tidak dikarang ulang di sini**.

| Aksi | Perawat pelaksana | Kepala ruangan | DPJP | Ahli gizi | Admisi |
| --- | :---: | :---: | :---: | :---: | :---: |
| Membaca ruang kerja | ✔ | ✔ | ✔ | ✔ | — |
| Membuat pengkajian | ✔ | ✔ | — | — | — |
| Menyelesaikan pengkajian | ✔ | ✔ | — | — | — |
| **Menambah koreksi** pada pengkajian final | — | ✔ | — | — | — |
| Menyusun rencana asuhan | ✔ | ✔ | — | — | — |
| Mencatat tindakan | ✔ | ✔ | — | — | — |
| Menyunting catatan orang lain yang belum final | — | — | — | — | — |
| **Menambah koreksi** pada catatan final | penulisnya | ✔ | — | — | — |
| Membaca daftar pantau | ✔ | ✔ | ✔ | — | — |

> **Kolom DPJP hanya berisi baca.** Bukan kelalaian: `AC-CAP014-03` melarang pengguna yang bukan
> penulis menyunting catatan keperawatan final.

---

## 5. Penanganan keadaan

| Keadaan | Aturannya |
| --- | --- |
| Memuat | Setiap layar daftar wajib punya keadaan memuat tersendiri, bukan halaman kosong |
| Kosong | Wajib membedakan "belum ada" dari "tidak dapat dimuat". Lihat `FE-KEP-06` |
| Gagal | Wajib menyediakan tombol coba lagi. Kegagalan konteks pasien **menonaktifkan seluruh tombol tulis** |
| Data basi | Ruang kerja memuat ulang konteks episode saat difokuskan kembali. Episode yang ternyata sudah `Closed` mengubah seluruh layar menjadi hanya-baca **tanpa menunggu pengguna menekan apa pun** |
| Pengiriman ganda | Tindakan memakai `Idempotency-Key`; tombol dinonaktifkan selama permintaan berjalan. Tekanan kedua **tidak** melahirkan tindakan kedua |
| Penolakan `403` | Dibedakan dari galat halaman. Bunyinya menyebut siapa yang berwenang, bukan sekadar "akses ditolak" |
| Penolakan `422` | Menyebut keadaan episodenya, bukan istilah teknis |

---

## 6. Privasi di layar

| Aturan | Isinya |
| --- | --- |
| Kolom sensitif | Catatan bebas — catatan perawat, psikososial, edukasi, nyeri, hasil tindakan — **tidak** ditampilkan pada daftar ringkas maupun tooltip. Hanya pada layar detail |
| Daftar pantau | Menampilkan nama pasien, lokasi, dan keterlambatan. **Tidak** menampilkan isi klinis |
| Cetak | Tidak ada layar cetak pada sub-modul ini |
| Log peramban | Payload berisi kolom sensitif **MUST NOT** ditulis ke console |

---

## 7. Kewenangan UI

| Hal | Wewenang |
| --- | --- |
| Keterjangkauan layar dan induknya | **Mengikat** — bagian 2.2 |
| Sumber data tiap wilayah | **Mengikat** — bagian 3 |
| Hak akses tiap tombol | **Mengikat** — bagian 4 |
| Bunyi keadaan kosong dan gagal | **Mengikat** untuk maknanya; kata persisnya `DEV_DISCRETION` |
| Aturan keselamatan: tombol tulis mati saat konteks gagal | **Mengikat** |
| Bentuk tab, drawer, atau accordion pada ruang kerja | `DEV_DISCRETION` |
| Warna, jarak, ikon, component library | `DEV_DISCRETION` |
| Nama menu dan urutannya | `DEV_DISCRETION`, dalam batas `IA-INP-05` |

---

## 8. Ketergantungan test

| Yang dibutuhkan | Kenapa |
| --- | --- |
| Episode berstatus `Admitted` beserta perawat penanggung jawabnya | Seluruh layar butuh konteks |
| Sekurang-kurangnya satu baris `MstClinicalAssessmentPolicy` | Menguji penanda tenggat. **Dan satu skenario tanpa baris itu sama sekali**, untuk `VAL-KEP-17` |
| Peran perawat, kepala ruangan, dan DPJP terpisah | Menguji `AC-CAP014-03` dan kolom baca-saja DPJP |
| Data master rawat inap yang layak | `RWI-UI-GAP-007` masih terbuka dan **ikut menahan sub-modul ini** |

---

## 9. Traceability

| Bagian | Requirement | Kontrak |
| --- | --- | --- |
| 1 kebutuhan layar | PRD 16.1, 17 | — |
| 2 keterjangkauan | `IA-INP-01`, `IA-INP-05` | `../02-module-map.md` bagian 3 |
| 3.1 aturan keselamatan konteks | `INV-KEP-01` | `validation-matrix.md` `VAL-KEP-01` s.d. `04` |
| 3.2 penanda tenggat | PRD 16.2 aturan 11 | `validation-matrix.md` `VAL-KEP-17` |
| 3.3 lini masa | `AC-CAP012-02` | `api-contract.md` bagian 1 |
| 3.4 riwayat versi | `AC-CAP013-02` | `api-contract.md` bagian 2 |
| 3.5 penanda tagihan | `AC-CAP014-02` | `integration-contract.md` `INT-KEP-05` |
| 4 aksi per peran | — | `permission-audit-matrix.md` bagian 2 |

---

## 10. Amandemen revision `0.3` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

| Field | Nilai |
| --- | --- |
| Status | **`draft`** — belum disetujui manusia |
| Frontend SHA | `1ce219b40f8e411f3c4e66975626ab33ae81616a` |
| Masukan | `02-backend-architecture.md` `0.4` bagian 11; kontrak `0.5.0`; PRD v`2.0` bagian 24–49; `RWI-DEC-108`, `113` s.d. `120`, `137`, `140`, `141`, `145` |
| Peta menu seluruh modul | [`../02-module-map.md`](../02-module-map.md) bagian 3 revision `2` |

### 10.1 Yang digantikan dari bagian 1 s.d. 9

| Bagian lama | Keadaan pada `0.3` |
| --- | --- |
| 3.1 `FE-KEP-01` empat tab | **Digantikan** `FE-KEP-07`: layout V2 yang sudah dibangun (`FE-RWI-051` s.d. `056`) **dipertahankan** — kepala informasi pasien, navigasi internal kiri, isi utama, tab sekunder — tetapi navigasi kiri berisi **delapan menu** PRD bagian 25 |
| 3.2 `FE-KEP-02` satu formulir tujuh kelompok | **Digantikan** `FE-KEP-08` dan `FE-KEP-09`: tujuh sub-menu Pengkajian Pasien dengan progres |
| 3.3 `FE-KEP-03` Lini Masa | **Dilebur** menjadi tab Riwayat pada setiap sub-menu dan grafik pada Vital Sign |
| 3.4 `FE-KEP-04` Rencana Asuhan | **Dipertahankan** sebagai kemampuan di dalam Asuhan Keperawatan — PRD bagian 35 |
| 3.5 `FE-KEP-05` Catatan Tindakan | **Dipertahankan** sebagai sub-menu Tindakan Harian |
| 3.6 `FE-KEP-06` Daftar Pantau | Tetap |
| 2.1 "Nol butir menu tingkat dua" | **Tetap benar** untuk menu Rawat Inap. Tiga layar konfigurasi baru berada di grup **Master Data** dan **Farmasi** — bagian 10.3 |

### 10.2 Kebutuhan layar revision `0.3`

| ID | Layar | Tujuan | Pemakai utama | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-KEP-07` | **Ruang Kerja Keperawatan V2** | Satu pasien, delapan menu internal | Perawat, kepala ruangan, MPP | **Rework** `FE-KEP-01` — layout tetap, isi menu berubah |
| `FE-KEP-08` | **Pengkajian Pasien** | Progres lima bagian dan tujuh sub-menu | Perawat, MPP | **Rework** `FE-KEP-02`, `03` |
| `FE-KEP-09` | **Formulir berinstrumen** | Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Perencanaan Pulang — digambar dari definisi versi | Perawat | **Baru** — satu penggambar formulir, lima pemakaian |
| `FE-KEP-10` | **Pengawasan Harian Pasien** | Tanda vital, nyeri, intake, output, balance, GDS, diet dan mobilisasi dalam satu hari | Perawat | **Baru** |
| `FE-KEP-11` | **Evaluasi Awal (MPP)** | Delapan bagian MPP | MPP menulis; lainnya membaca | **Baru** |
| `FE-KEP-12` | **Asuhan Keperawatan** | Vital Sign, SOAP, Catatan Terintegrasi, Tindakan Harian, Obat & Alkes, Catatan Keperawatan, Rencana Asuhan | Perawat | **Rework** |
| `FE-KEP-13` | **Obat & Alkes** | Pemberian Obat (MAR), Sliding Scale, Obat Bawaan, Resep Aktif, Pemakaian Alkes | Perawat; kepala ruangan mengoreksi | **Baru** |
| `FE-KEP-14` | **Tindakan** | Order Tindakan atas instruksi dokter, History Tindakan | Perawat | **Baru** |
| `FE-KEP-15` | **Penunjang Medis** | Enam kartu; Laboratorium dan Radiologi berisi | Perawat | **Baru** — permukaan `CAP-015` |
| `FE-KEP-16` | **Transfer Pasien** | Form dan History perpindahan tempat tidur; Serah Terima Klinis belum tersedia | Perawat berwenang pindah | **Baru** — memakai `CAP-017` |
| `FE-KEP-17` | **Pemakaian Alat** dan **Pemesanan Ruangan Bedah** | Konteks pasien + "Integrasi belum tersedia" | Perawat | **Baru** — permukaan saja |
| `FE-KEP-18` | **Tagihan Pasien** | Ringkasan baca-saja bagi pemegang hak khusus | Petugas yang ditunjuk | **Baru** — "belum tersedia" sampai kontrak Billing |
| `FE-KEP-19` | **Instrumen & Formulir Klinis** | Kelola versi, uji hitung, sahkan | Admin konfigurasi klinis, komite keperawatan | **Baru** |
| `FE-KEP-20` | **Jam Shift Keperawatan** | Jam shift bawaan dan per unit | Admin sistem keperawatan | **Baru** |
| `FE-KEP-21` | **Jadwal Pemberian Obat** | Jam standar per frekuensi, pengaturan MAR, frekuensi tanpa jadwal | Apoteker / admin Farmasi | **Baru** |
| `FE-KEP-22` | **Daftar Tunggu Cek Ganda** | Dosis high-alert menunggu perawat kedua di unit pengguna | Perawat | **Baru** |

### 10.3 Peta butir menu dan jalan masuk

**Butir menu baru — tiga, seluruhnya di luar menu tingkat dua Rawat Inap.** Kuota `IA-INP-05` tidak terpakai; diperiksa
pada `menu-items.jsx`: grup Master Data baris 638–843 sudah memuat "Butir Administrasi Rawat Inap" dan "Pengaturan Rawat
Inap"; grup Farmasi baris 845.

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Instrumen & Formulir Klinis | 1 | **Master Data** (Pelayanan Kesehatan) | `/health-services/clinical-management/clinical-instruments` | `FE-KEP-19` | `ClinicalInstrumentConfiguration : Read` | **Baru** |
| Jam Shift Keperawatan | 1 | **Master Data** | `/health-services/clinical-management/nursing-shifts` | `FE-KEP-20` | `NursingShift : Read` | **Baru** |
| Jadwal Pemberian Obat | 1 | **Farmasi** | `/health-services/pharmacy-management/medication-schedule-settings` | `FE-KEP-21` | `MedicationScheduleSetting : Read` | **Baru** |

**Layar anak.**

| Layar | Jalan masuk | Butir hak akses penjaga |
| --- | --- | --- |
| `FE-KEP-07` | Baris Census `FE-INP-01` dan tombol "Buka Ruang Kerja Keperawatan" pada Detail Episode `FE-INP-04` → route yang **sudah ada** `…/episodes/{id}/nursing` | `InpatientEpisode : Read` |
| `FE-KEP-08` s.d. `FE-KEP-18` | Navigasi internal kiri `FE-KEP-07`; sub-bagian sebagai tab sekunder. Parameter `?section=` dan `?tab=` menyimpan posisi supaya tautan dapat dibagikan | Per layar, lihat 10.4 |
| `FE-KEP-22` | Penanda "Menunggu cek ganda (n)" pada `FE-KEP-13`, dan kartu keenam pada Daftar Pantau `FE-INP-09` | `MedicationAdministration : DoubleCheck` |

**Navigasi internal kiri `FE-KEP-07`** — urutan mengikat, PRD bagian 25:

| No | Menu | `section` | Tab sekunder | Layar |
| ---: | --- | --- | --- | --- |
| 1 | Pengkajian Pasien | `assessment` | Kajian Umum · Resiko Jatuh · Monitoring Nyeri · Assement Edukasi · Pengawasan Harian Pasien · Evaluasi Awal · Perencanaan Pulang | `FE-KEP-08`–`11` |
| 2 | Asuhan Keperawatan | `nursing-care` | Vital Sign · SOAP · Catatan Terintegrasi · Tindakan Harian · Obat & Alkes · Catatan Keperawatan · Rencana Asuhan | `FE-KEP-12`, `13` |
| 3 | Tindakan | `procedure` | Order Tindakan · History Tindakan | `FE-KEP-14` |
| 4 | Penunjang Medis | `ancillary` | Radiologi · Laboratorium · Rehab Medik · Konsultasi Gizi · Hemodialisa · Bank Darah | `FE-KEP-15` |
| 5 | Pemakaian Alat | `equipment` | Order Alat Kesehatan · History Alat Kesehatan | `FE-KEP-17` |
| 6 | Transfer Pasien | `transfer` | Form Transfer · History Transfer | `FE-KEP-16` |
| 7 | Pemesanan Ruangan Bedah | `surgery-booking` | Bedah Operasi · Bedah Obgyn | `FE-KEP-17` |
| 8 | Tagihan Pasien | `billing` | — | `FE-KEP-18` |

Label "Assement Edukasi" ditulis mengikuti PRD dan V1; perbaikan ejaan adalah `DEV_DISCRETION` bila pemilik setuju.
Route lama `?section=care-plan`, `intervention`, `timeline` dialihkan ke `nursing-care` tab Rencana Asuhan, Tindakan
Harian, dan Pengkajian Pasien → Riwayat.

```text
Beranda → Rawat Inap → Census → baris pasien → Ruang Kerja Keperawatan   = 3 klik  ✔ IA-INP-01
Beranda → Master Data → Instrumen & Formulir Klinis                      = 2 klik  ✔
Beranda → Farmasi → Jadwal Pemberian Obat                                = 2 klik  ✔
```

### 10.4 Skema fitur per layar

#### 10.4.1 `FE-KEP-07` Ruang Kerja Keperawatan V2

```text
┌────────────────────────────────────────────────────────────────────────┐
│ INFORMASI PASIEN — tetap terlihat                                      │
│ Budi S. · RM 00-12-34 · Melati 302/2 · DPJP dr. Rina · BPJS kls 2      │
│ Hari ke-2 · ⚠ Alergi: Amoksisilin · ⚠ Risiko Jatuh Tinggi              │
├──────────────────────┬─────────────────────────────────────────────────┤
│ Pengkajian Pasien 40%│  [tab sekunder menu terpilih]                   │
│ Asuhan Keperawatan   │                                                 │
│ Tindakan             │   Isi menu                                      │
│ Penunjang Medis      │                                                 │
│ Pemakaian Alat       │                                                 │
│ Transfer Pasien      │                                                 │
│ Pemesanan R. Bedah   │                                                 │
│ Tagihan Pasien       │                                                 │
└──────────────────────┴─────────────────────────────────────────────────┘
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Informasi pasien | Identitas, lokasi, DPJP, penjamin, hari rawat | `GET inpatient-management/episodes/{id}` | `InpatientEpisode : Read` | Tidak pernah kosong | Seluruh tombol tulis nonaktif — aturan 3.1 tetap |
| Alert alergi | Alergi aktif **termasuk dugaan reaksi obat** | `GET patient-allergies/active-alerts` | `PatientAllergy : Read` | "Belum ada alergi tercatat" | Ditampilkan menonjol |
| Alert klinis | Pita alert instrumen terakhir, misalnya Risiko Jatuh Tinggi | `GET patient-assessments/episodes/{id}/progress` | `PatientAssessment : Read` | Tanpa alert | "Alert klinis tidak dapat dimuat" — **bukan** disembunyikan |
| Angka progres | "40%" di samping Pengkajian Pasien | Sama | Sama | "0%" | Tanpa angka, ikon galat |
| Navigasi kiri | Delapan menu tetap tampil walau isinya belum tersedia | Statis | Menu tetap tampil; isinya menjaga hak | — | — |

#### 10.4.2 `FE-KEP-08` Pengkajian Pasien

```text
Pengkajian Awal — 2 dari 5 bagian selesai — 40%   ████████░░░░░░░░░░░░
─────────────────────────────────────────────────────────────────────
Kajian Umum ✓ │ Resiko Jatuh ✓ │ Monitoring Nyeri ! │ Assement Edukasi ○ │
Pengawasan Harian · terakhir 06.00 │ Evaluasi Awal · Belum diisi — milik MPP │ Perencanaan Pulang ○
─────────────────────────────────────────────────────────────────────
[ Riwayat ] [ Form ]          [+ Buat baru]  (tombol sesuai hak)
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Progres | Angka, bar, lima penanda ✓/!/○, dua isian tampil tanpa hitungan | `GET patient-assessments/episodes/{id}/progress` | `PatientAssessment : Read` | 0 dari 5 | "Gagal memuat progres pengkajian" + Coba Lagi. **Nol ○** — `AC-KEP-072` |
| Tab sekunder | Tujuh sub-menu dengan penanda | Sama | — | — | Penanda disembunyikan, tab tetap dapat dibuka |
| Riwayat | Daftar dokumen sub-menu terpilih: waktu klinis, penulis, status, skor/pita bila ada, addendum | `GET patient-assessments/episodes/{id}?assessmentType=` berpaginasi | `PatientAssessment : Read` | "Belum ada {nama bagian}" — **tanpa** kata "normal" atau warna hijau (`RWI-DEC-119` (1)) | "Riwayat tidak dapat dimuat" |
| Form | `FE-KEP-09` / `FE-KEP-10` / `FE-KEP-11` | Lihat di bawah | — | — | — |

**Penanda ○ tidak pernah hijau dan tidak pernah berbunyi "aman".** Warna penanda `DEV_DISCRETION` dalam batas itu.

#### 10.4.3 `FE-KEP-09` Formulir berinstrumen

```text
Resiko Jatuh — Morse Dewasa v2 (disahkan 12/09/2026)       [Konsep]
────────────────────────────────────────────────────────────────────
Riwayat jatuh 3 bulan terakhir     ( ) Tidak 0   (•) Ya 25
Diagnosis sekunder                 (•) Tidak 0   ( ) Ya 15
...
Skor 50 — Tinggi ⚠                (dihitung server saat disimpan)
────────────────────────────────────────────────────────────────────
[Simpan Konsep]                                        [Selesaikan]
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Kepala formulir | Nama instrumen, versi, tanggal sah; pita "Lingkungan uji — versi belum disahkan" bila berlaku | `GET clinical-instruments/resolve?instrumentKind=&episodeId=` | `PatientAssessment : Read` | "Tidak ada instrumen untuk usia pasien ini" — tombol simpan nonaktif | "Formulir tidak dapat dimuat" — **tidak** menampilkan formulir lama dari cache |
| Isian | Digambar dari `sections[]`; isian ber-`binding` tampil sama tetapi dikirim sebagai kolom | Definisi versi | — | — | — |
| Kondisi Umum (Kajian Umum) | Pilih tanda vital terakhir atau catat baru; angka tampil **hanya baca** dari baris tanda vital | `GET patient-vital-signs/episodes/{id}` | `PatientVitalSign : Read` / `Create` | "Belum ada tanda vital hari ini — catat dulu" | "Tanda vital tidak dapat dimuat" |
| Skor | Hasil **server** setelah simpan; layar tidak menghitung sendiri | Response simpan | — | "Belum dihitung" | — |
| Simpan Konsep | Selalu boleh, isian belum lengkap tetap tersimpan (`BR-RWI-004`) | `POST`/`PUT patient-assessments` | `PatientAssessment : Create`/`Update` | — | Pesan server; isian di layar **tidak** dihapus |
| Selesaikan | Nonaktif bila versi belum sah di produksi; menampilkan daftar isian wajib yang kurang | `PATCH /{id}/complete` | `PatientAssessment : Update` | — | `422` ditampilkan per isian |

**Susunan Kajian Umum** — delapan bagian `RWI-DEC-141`, dengan letak final isian tambahan:

| Bagian | Isian tambahan yang ditempatkan di sini |
| --- | --- |
| Sumber Data Pasien | **Catatan relevan** — riwayat jatuh di rumah, kebiasaan |
| Kondisi Umum | Keluhan, tanda vital (rujukan), kesadaran, kebutuhan oksigen, **kelompok Psikososial** |
| Pernapasan | — |
| Integritas Kulit | — |
| Skrining Nutrisi | — |
| Eliminasi | **Kateter urin** — satu-satunya tempat |
| Ketergantungan | **Kursi roda, tongkat, walker** — satu-satunya tempat |
| Status Fungsional | Tanpa isian alat bantu |

**Monitoring Nyeri** menampilkan pilihan wajib "Tidak nyeri / Nyeri / Tidak dapat dinilai" sebelum skala; skala dan
wajah/perilaku mengikuti instrumen nyeri yang berlaku bagi usia pasien. Setelah intervensi, layar menampilkan
"Kajian ulang pukul 09.00" dari `PainReassessmentDueAt`.

#### 10.4.4 `FE-KEP-10` Pengawasan Harian Pasien

```text
PENGAWASAN HARIAN — Selasa 16/09/2026 (07.00–07.00)    [◀] [▶]   [+ Catat Pengawasan ▾]
────────────────────────────────────────────────────────────────────────────────────
Tanda Vital      08.00 TD 130/80 N 88 S 37,2 RR 20 SpO2 97 · 14.00 ...     [grafik]
Nyeri            Terakhir 7 pukul 08.10 (Monitoring Nyeri)                  [buka]
Intake           Infus 1.500 · Oral 600 · NGT 0 · Darah 200 · Obat 200      = 2.500 ml
Output           Urin 1.800 · Feses 0 · NGT 0 · Lain 150                    = 1.950 ml
Balance          Pagi +220 · Siang +180 · Malam +150                        = +550 ml
Gula Darah       06.00 180 mg/dL · 11.00 280 mg/dL (dipakai sliding scale)
Diet & Mobilisasi  Makan ¾ porsi · Duduk di tempat tidur
Pengingat        Ceftriaxone 20.00 diberikan, belum ada entri intake        [catat]
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Pemilih hari | Hari klinis sesuai shift pertama unit | — | — | — | — |
| Seluruh baris ringkasan | Satu permintaan | `GET daily-monitoring/episodes/{id}/summary?date=` | `DailyObservation : Read` | Per baris "Belum dicatat" | "Pengawasan harian tidak dapat dimuat" — **tidak** menampilkan total 0 ml |
| Tanda Vital | Deret dan grafik | Ringkasan; catat → `POST patient-vital-signs` | `PatientVitalSign : Create` | "Belum dicatat" | — |
| Nyeri | Hanya baca; tautan ke Monitoring Nyeri | Ringkasan | — | "Belum dinilai" — **bukan** "tidak nyeri" | — |
| Intake / Output | Total per sumber; daftar entri beserta koreksi dan riwayat | `GET fluid-balance-entries/episodes/{id}`; catat `POST`; koreksi `PUT /{id}/correct` | `FluidBalance : Read`/`Create`/`Update` | "Belum ada entri" | — |
| Form intake Obat | Pilih dosis `Administered` hari itu yang belum punya entri; volume diketik termasuk pelarut | Ringkasan `AdministeredDosesWithoutIntake` | `FluidBalance : Create` | "Tidak ada dosis yang dapat ditautkan" | — |
| Balance | Per shift dan 24 jam | Ringkasan | — | "Jam shift belum dikonfigurasi — hanya total 24 jam" | — |
| Gula Darah | Nilai, satuan, penanda "dipakai sliding scale" | `GET blood-glucose-readings/episodes/{id}`; catat `POST` | `BloodGlucose : Read`/`Create` | "Belum dicatat" | — |
| Diet & Mobilisasi | Observasi terakhir dan riwayat | `GET/POST daily-observations` | `DailyObservation : Read`/`Create` | "Belum dicatat" | — |
| Pengingat | Dosis tanpa entri intake — tidak mewajibkan | Ringkasan | — | Disembunyikan | — |

Pilihan satuan GDS **tidak** punya bawaan terpilih. Entri berpenanda `DoseCorrectionFlaggedAt` tampil "perlu ditinjau".

#### 10.4.5 `FE-KEP-11` Evaluasi Awal (MPP)

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Status | "Belum diisi — milik MPP" / konsep / selesai, penulis, waktu | `GET case-management-evaluations/episodes/{id}` | `CaseManagementEvaluation : Read` | "Belum diisi — milik MPP" | "Evaluasi Awal tidak dapat dimuat" |
| Delapan bagian | Checklist dari definisi `CaseManagementChecklist` | `GET clinical-instruments/resolve?instrumentKind=CaseManagementChecklist` | Sama | "Checklist belum disahkan" | — |
| Tombol tulis | Simpan, Selesaikan, Batalkan, Addendum | `POST`/`PUT`/`PATCH` | `CaseManagementEvaluation : Create`/`Update`/`Amend` | Pengguna tanpa hak **tidak melihat** tombol; perawat melihat dokumen hanya baca | `403` unit → "Evaluasi Awal hanya ditulis MPP yang ditempatkan di unit pasien ini" |
| Addendum | Tombol nonaktif bertooltip "Addendum Evaluasi Awal belum tersedia" selama `INT-KEP-12` belum disetujui | — | — | — | — |

#### 10.4.6 `FE-KEP-12` Asuhan Keperawatan

| Tab sekunder | Isinya | Sumber data | Hak akses | Keadaan kosong / gagal |
| --- | --- | --- | --- | --- |
| Vital Sign | Riwayat, Input Vital Sign — **tanpa isian nyeri**, dengan tautan "Kaji nyeri" ke Monitoring Nyeri — dan Grafik Pemeriksaan | `GET patient-vital-signs/episodes/{id}`, `POST patient-vital-signs` | `PatientVitalSign : Read`/`Create` | "Belum ada tanda vital" / "Tanda vital tidak dapat dimuat" |
| SOAP | SOAP keperawatan, riwayat, addendum | `GET/POST patient-integrated-progress-notes` dengan `NoteKind = NursingSoap` | `PatientIntegratedProgressNote : Read`/`Create` | "Belum ada SOAP keperawatan" |
| Catatan Terintegrasi | Seluruh CPPT episode: penulis, profesi, waktu klinis, jenis catatan, status verifikasi | `GET patient-integrated-progress-notes/episodes/{id}` | `PatientIntegratedProgressNote : Read` | "Belum ada catatan" |
| Tindakan Harian | Layar `FE-KEP-05` apa adanya: waktu, tindakan, perawat, hasil | `nursing-interventions` yang sudah ada | `NursingIntervention : Read`/`Create` | Bagian 3.5 |
| Obat & Alkes | `FE-KEP-13` | — | — | — |
| Catatan Keperawatan | Kartu lini masa: tanggal-jam, nama perawat, catatan, Detail | `NoteKind = NursingNarrative` | `PatientIntegratedProgressNote : Read`/`Create` | "Belum ada catatan keperawatan" |
| Rencana Asuhan | Layar `FE-KEP-04` apa adanya | `nursing-care-plans` yang sudah ada | `NursingCarePlan` | Bagian 3.4 |

Angka cairan atau obat yang diketik di SOAP dan Catatan Keperawatan **tidak** mengubah total maupun MAR — `RWI-AC-206`.
Layar menampilkan catatan kecil "Catat intake/output di Pengawasan Harian dan pemberian obat di Obat & Alkes."

#### 10.4.7 `FE-KEP-13` Obat & Alkes

```text
Pemberian Obat │ Sliding Scale │ Obat Bawaan │ Resep Aktif │ Pemakaian Alkes
───────────────────────────────────────────────────────────────────────────────
Selasa 16/09   Menunggu cek ganda (1)                               [◀] [▶]
Obat                         06   08         14   20         22
Ceftriaxone 1 g IV q12h           ✓ 08.05          ● Due
Insulin aspart (sliding scale)    [Catat GDS & dosis]
Parasetamol 500 mg PRN       [+ Beri PRN]
Omeprazol q6h — Jadwal pemberian untuk frekuensi q6h belum dikonfigurasi  [+ Catat pemberian]
Amlodipin 5 mg — DIHENTIKAN 14.00 (dr. Rina)                     ✕ 20.00 dibatalkan
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Grid MAR | Butir sebagai baris, dosis sebagai sel; ikon status, penanda high-alert, lewat waktu, cek ganda | `GET medication-administrations/episodes/{id}?date=` | `MedicationAdministration : Read` | "Belum ada resep aktif untuk pasien ini" | "MAR tidak dapat dimuat. Jangan memberi obat berdasarkan tampilan lama." — grid dikosongkan |
| Klik sel `Due` | Dialog catat: status, dosis, rute, waktu, alasan, catatan penyimpangan | `PATCH /{id}/record` + `Idempotency-Key` | `MedicationAdministration : Create` | — | Tombol simpan terkunci sampai response; galat jaringan → "Status tidak diketahui, muat ulang MAR sebelum mengulang" |
| Cek ganda | Dialog perawat kedua dengan login ulang **tidak** diminta; akun login yang sedang aktif harus berbeda | `PATCH /{id}/double-check` | `MedicationAdministration : DoubleCheck` | — | `403` → "Cek ganda harus dilakukan perawat lain" |
| PRN / tanpa jadwal | Dialog dengan indikasi atau alasan wajib | `POST /as-needed`, `/unscheduled` | `MedicationAdministration : Create` | — | — |
| Koreksi | Menu sel yang sudah dicatat | `PUT /{id}/correct` | `MedicationAdministration : Update` | Tombol tidak tampil tanpa hak | — |
| Dugaan reaksi obat | Menu sel `Administered` | `POST patient-allergies/from-medication-administration` | `PatientAllergy : Create` | — | — |
| Sliding Scale | Order aktif, tombol "Catat GDS & dosis", pratinjau rentang dan dosis, riwayat pelaksanaan | `GET sliding-scale-orders/episodes/{id}`, `POST sliding-scale-executions/preview`, `POST sliding-scale-executions`, `GET sliding-scale-executions/episodes/{id}` | `SlidingScaleOrder : Read`, `SlidingScaleExecution : Read`/`Create` | "Tidak ada protokol sliding scale aktif" | Pratinjau gagal → tombol simpan nonaktif |
| Obat Bawaan | Daftar obat yang dibawa pasien dan keputusan dokter per obat (baca) | `GET/POST medication-reconciliations` milik `dokter-rawat-inap` | `MedicationReconciliation : Read`/`Create` | "Belum ada obat bawaan dicatat" | — |
| Resep Aktif | Resep Harian hanya baca | `GET prescriptions/episodes/{id}?period=` | `Prescription : Read` | "Belum ada resep" | — |
| Pemakaian Alkes | "Integrasi belum tersedia" | — | — | — | — |

**Dialog sliding scale** menampilkan satuan protokol besar-besar di samping isian GDS; satuan GDS dipilih sendiri tanpa
bawaan. Bila satuan berbeda, penolakan server ditampilkan apa adanya — layar **tidak** mengonversi.

#### 10.4.8 `FE-KEP-14` Tindakan, `FE-KEP-15` Penunjang Medis, `FE-KEP-16` Transfer Pasien

| Layar / tab | Isinya | Sumber data | Hak akses | Keadaan khusus |
| --- | --- | --- | --- | --- |
| Order Tindakan | Form pesanan dengan **dokter pemberi instruksi wajib** dipilih dari dokter berpenugasan aktif | `POST patient-procedures/inpatient-orders` | `PatientProcedure : Create` | Pesanan tampil "Menunggu verifikasi instruksi" |
| History Tindakan | Pesanan dan pelaksanaan; pesanan ≠ dilaksanakan | `GET patient-procedures/episodes/{id}` | `PatientProcedure : Read` | — |
| Penunjang Medis | Enam kartu. Laboratorium dan Radiologi: pesanan dengan dokter pemberi instruksi, dan hasil final | Kontrak `dokter-rawat-inap` bagian 12.12 | `LabOrder`, `RadOrder` | Bagian pesanan perawat nonaktif bertooltip "Menunggu persetujuan modul laboratorium/radiologi" sampai `INT-DOK-19` disetujui; hasil tetap tampil |
| Rehab Medik, Konsultasi Gizi, Hemodialisa, Bank Darah | Konteks pasien + "Integrasi belum tersedia" | — | — | **Nol** permintaan jaringan ke modul itu |
| Form Transfer | Perpindahan tempat tidur `CAP-017` apa adanya | Endpoint perpindahan `episode-rawat-inap` | Hak akses perpindahan yang sudah ada | Judul "Pindah Tempat Tidur", **bukan** "Serah Terima" |
| Serah Terima Klinis | Di bawah form: "Integrasi belum tersedia" | — | — | `RWI-DEC-113`: perpindahan tidak diberi label seolah serah terima klinis tercatat |
| History Transfer | Riwayat perpindahan | Endpoint riwayat `episode-rawat-inap` | Sama | — |

#### 10.4.9 `FE-KEP-17` Pemakaian Alat dan Pemesanan Ruangan Bedah, `FE-KEP-18` Tagihan Pasien

| Layar | Isinya | Sumber data | Hak akses | Keadaan |
| --- | --- | --- | --- | --- |
| Pemakaian Alat (dua tab) | Konteks pasien + "Integrasi belum tersedia" | — | — | Tanpa daftar contoh, tanpa tombol simpan |
| Pemesanan Ruangan Bedah (dua tab) | Sama | — | — | Sama |
| Tagihan Pasien — tanpa hak | "Anda tidak punya akses melihat ringkasan tagihan" | — | Tidak memegang `PatientBillingSummary : Read` | Tidak ada permintaan jaringan |
| Tagihan Pasien — dengan hak, kontrak belum ada | "Integrasi belum tersedia" | — | `PatientBillingSummary : Read` | — |
| Tagihan Pasien — kontrak ada | Penjamin dan kelayakan, total berjalan, deposit, sisa/kekurangan, jumlah item tidak ditanggung | Kontrak Billing `INT-KEP-14` | Sama | Gagal → pesan kegagalan, **bukan** Rp 0 |

#### 10.4.10 `FE-KEP-19` Instrumen & Formulir Klinis

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Daftar | Instrumen, jenis, rentang usia, versi sah, jumlah draft | `GET clinical-instruments` | `ClinicalInstrumentConfiguration : Read` | "Belum ada instrumen" | "Daftar tidak dapat dimuat" |
| Editor versi draft | Bagian, isian, pilihan dan skor, pita, isian wajib, pengikatan kolom | `POST /{id}/versions`, `PUT /versions/{versionId}` | `ClinicalInstrumentConfiguration : Update` | — | Galat validasi per isian |
| Uji hitung | Isi jawaban contoh → skor dan pita | `POST /versions/{versionId}/score-preview` | `Read` | — | — |
| Sahkan | Catatan pengesahan; nonaktif bila pengguna adalah pengubah terakhir, dengan penjelasan | `PATCH /versions/{versionId}/approve` | `ClinicalInstrumentConfiguration : Approve` | — | `403` ditampilkan apa adanya |
| Riwayat versi | Versi, status, pengubah, pengesah, tanggal, hash pendek | `GET /{id}` | `Read` | — | — |

#### 10.4.11 `FE-KEP-20` Jam Shift Keperawatan, `FE-KEP-21` Jadwal Pemberian Obat, `FE-KEP-22` Daftar Tunggu Cek Ganda

| Layar | Isinya | Sumber data | Hak akses | Keadaan khusus |
| --- | --- | --- | --- | --- |
| `FE-KEP-20` | Pilih unit atau Bawaan; tabel shift; garis waktu 24 jam yang menandai celah dan tumpang tindih sebelum simpan | `GET/PUT nursing-shifts` | `NursingShift : Read`/`Update` | Pita "Jam shift hanya dipakai menghitung total cairan, bukan menentukan siapa boleh mencatat" |
| `FE-KEP-21` | Tab Jadwal per frekuensi; tab Pengaturan MAR; tab "Frekuensi tanpa jadwal" | `GET/PUT medication-schedule-settings/*` | `MedicationScheduleSetting : Read`/`Update` | Pita "Perubahan berlaku bagi dosis yang dibentuk sesudahnya" |
| `FE-KEP-22` | Dosis menunggu cek ganda di unit: pasien, bed, obat, dosis, pencatat, waktu | `GET medication-administrations/double-check-worklist?serviceUnitId=` | `MedicationAdministration : DoubleCheck` | Baris milik pengguna sendiri tampil tanpa tombol konfirmasi |

### 10.5 Aksi per peran — tambahan

| Aksi | Perawat pelaksana | Kepala ruangan | MPP | Dokter | Apoteker | Admin konfigurasi |
| --- | :---: | :---: | :---: | :---: | :---: | :---: |
| Mengisi dokumen Pengkajian Pasien | ✔ | ✔ | Baca | Baca | — | — |
| Menulis Evaluasi Awal | Baca | Baca | ✔ | Baca | — | — |
| Mencatat Pengawasan Harian | ✔ | ✔ | Baca | Baca | — | — |
| Mencatat dosis MAR dan PRN | ✔ | ✔ | Baca | Baca | Baca | — |
| Cek ganda | ✔ perawat lain | ✔ perawat lain | — | — | — | — |
| Mengoreksi dosis MAR | — | ✔ | — | — | — | — |
| Pelaksanaan sliding scale | ✔ | ✔ | — | Baca | Baca | — |
| Mencatat obat bawaan | ✔ | ✔ | — | ✔ | — | — |
| Memutuskan obat bawaan | — | — | — | ✔ | — | — |
| Memesan tindakan atas instruksi | ✔ | ✔ | — | Dari ruang kerja dokter | — | — |
| Mengubah dan mengesahkan instrumen | — | — | — | — | — | Ubah / sahkan terpisah |
| Mengubah jam shift | — | — | — | — | — | ✔ |
| Mengubah jadwal pemberian obat | — | — | — | — | ✔ | — |

Peran adalah kumpulan hak pada Akses Role; tabel ini menjelaskan seeder usulan `permission-audit-matrix.md` bagian 6.2.

### 10.6 Penanganan keadaan — tambahan

| Keadaan | Perilaku wajib |
| --- | --- |
| Konteks pasien gagal | Seluruh tombol tulis delapan menu nonaktif |
| Progres gagal | Pesan galat + Coba Lagi; **nol** ○ |
| MAR gagal atau basi | Grid dikosongkan dengan pesan; tidak menampilkan data cache |
| Kiriman ganda | Setiap tombol simpan klinis mengirim `Idempotency-Key` dan terkunci sampai response |
| Status kiriman tidak diketahui (jaringan putus) | "Status tidak diketahui — muat ulang sebelum mengulang"; kunci yang sama dipakai bila pengguna mengulang dari dialog yang sama |
| `409` versi basi | Muat ulang data tanpa menghapus isian yang sedang diketik bila memungkinkan |
| `401` | Arahkan ke login — perbaikan `RLN3-CAP-23` |
| `403` | Pesan kewenangan dari server; formulir tidak dikosongkan |
| Menu belum tersedia | Konteks pasien + "Integrasi belum tersedia"; tanpa data tiruan, tanpa pesan berhasil palsu |
| Lingkungan uji dengan versi draft | Pita peringatan pada setiap formulir berinstrumen |

### 10.7 Privasi dan kewenangan UI — tambahan

| Hal | Aturan |
| --- | --- |
| Jawaban pengkajian, alasan, catatan | Tidak dicetak ke console dan tidak dikirim ke layanan pelacakan galat |
| Tombol tulis | Disembunyikan tanpa hak; nonaktif beralasan bila hak ada tetapi syarat belum terpenuhi. Server tetap penjaga akhir |
| Tagihan | Tanpa hak, tidak ada permintaan jaringan dan tidak ada angka apa pun |
| `DEV_DISCRETION` | Warna penanda selain larangan ○ hijau, ikon, jarak, komponen tabel dan grafik, pemakaian dialog atau panel samping |
| Mengikat | Urutan delapan menu dan tab sekunder PRD bagian 25; delapan bagian Kajian Umum; letak kateter dan kursi roda; tidak adanya isian nyeri pada input tanda vital; satuan GDS tanpa bawaan |

### 10.8 Ketergantungan test — tambahan

| Test | Bergantung pada |
| --- | --- |
| Progres dan formulir berinstrumen | Seeder draft instrumen dan pengaturan lingkungan uji |
| MAR | Seeder jadwal frekuensi, resep berbutir `q12h`, obat high-alert, dua akun perawat berbeda |
| Sliding scale | Template dan order milik `dokter-rawat-inap` pada rilis `DOK-V2-2` |
| Penunjang Laboratorium dan Radiologi | Persetujuan pemilik Lab/Rad; tanpa itu hanya tampilan hasil yang diuji |
| Evaluasi Awal addendum | `INT-KEP-12` |
| Tagihan Pasien | Kontrak Billing |
| E2E | Microsoft Edge sesuai kebiasaan repository |

### 10.9 Traceability revision `0.3`

| Layar | Requirement | Keputusan | Acceptance |
| --- | --- | --- | --- |
| `FE-KEP-07` | PRD 24–26, 48 | `RWI-DEC-108`, `113` | `AC-KEP-130` |
| `FE-KEP-08` | PRD 27, `FR-MVP-KEP-001` | `RWI-DEC-119`, `120` | `AC-KEP-069` s.d. `073` |
| `FE-KEP-09` | PRD 28–31, 34; `FR-MVP-KEP-002` s.d. `005` | `RWI-DEC-124`, `136`, `141` | `AC-KEP-051` s.d. `068` |
| `FE-KEP-10` | PRD 32; `FR-MVP-KEP-017` | `RWI-DEC-148`, `149` | `AC-KEP-080` s.d. `093` |
| `FE-KEP-11` | PRD 33 | `RWI-DEC-118`, `131` | `AC-KEP-074` s.d. `079` |
| `FE-KEP-12` | PRD 35–41 | `RWI-DEC-115`, `140` | `AC-KEP-133` |
| `FE-KEP-13`, `FE-KEP-22` | PRD 40; `FR-MVP-KEP-009` s.d. `014`, `018` | `RWI-DEC-116`, `117`, `132`, `133`, `145` s.d. `148` | `AC-KEP-094` s.d. `128`, `AC-KEP-134` |
| `FE-KEP-14`, `FE-KEP-15` | PRD 42, 43 | `RWI-DEC-113`, `114` | `AC-KEP-130` |
| `FE-KEP-16` | PRD 45 | `RWI-DEC-113` | — |
| `FE-KEP-17`, `FE-KEP-18` | PRD 44, 46, 47 | `RWI-DEC-108`, `137` | `AC-KEP-130` s.d. `132` |
| `FE-KEP-19` s.d. `FE-KEP-21` | `FR-MVP-KEP-005`, `012`, `017` | `RWI-DEC-124`, `136`; gate `G-06`, `G-12` | `AC-KEP-051` s.d. `053`, `091`, `092`, `096` |
