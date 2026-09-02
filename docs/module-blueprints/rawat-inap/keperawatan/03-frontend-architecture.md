# Arsitektur Frontend — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Revision | `0.1` |
| Status | `draft` |
| Tanggal | 2 September 2026 |
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
| `FE-KEP-02` | **Pengkajian Keperawatan** | Mengisi, menyelesaikan, dan mengamandemen pengkajian awal maupun ulang | Perawat, kepala ruangan | **baru** |
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
| Penanda amandemen | Baris yang pernah diamandemen beserta alasannya | Sama | Sama | — | — |

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
| Mengamandemen pengkajian final | — | ✔ | — | — | — |
| Menyusun rencana asuhan | ✔ | ✔ | — | — | — |
| Mencatat tindakan | ✔ | ✔ | — | — | — |
| Menyunting catatan orang lain yang belum final | — | — | — | — | — |
| Mengamandemen catatan final | penulisnya | ✔ | — | — | — |
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
