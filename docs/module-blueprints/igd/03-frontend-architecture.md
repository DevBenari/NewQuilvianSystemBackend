# Arsitektur Frontend — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `5`; **bagian 13 ditambahkan 22 September 2026** (encounter-first); **bagian 15 ditambahkan 6 Oktober 2026** (Ruang Kerja Dokter IGD, `approved` `IGD-DEC-230`) |
| Status | `draft` |
| Commit diaudit | frontend `96a9120111f6acc6b7c0f37973ea0c717ba41f17` |
| Kontrak yang diikuti | API `0.3.0`, state `0.3.0`, validation `0.3.0`, permission/audit `0.3.0` |

---

## 0. Hierarki kewenangan yang dipakai dokumen ini

```text
keamanan / privasi / invariant
  → brief produk atau UI yang disetujui
    → konvensi dan design system proyek
      → DEV_DISCRETION
```

Dokumen ini **tidak** menetapkan sidebar, urutan menu, route final, bentuk tab/modal/drawer,
warna, layout, maupun pustaka komponen. Hal-hal itu ditandai `DEV_DISCRETION` dan diputuskan
pelaksana mengikuti konvensi yang sudah ada.

Yang **ditetapkan** dokumen ini hanyalah hal yang berasal dari dua lapis teratas: apa yang
wajib terlihat, apa yang wajib ditolak, apa yang tidak boleh ditampilkan, dan apa yang tidak
boleh menahan pekerjaan perawat.

---

## 1. Konvensi yang wajib dipakai ulang

Layar IGD **tidak** boleh membuat komponen atau gaya tandingan. Yang sudah ada dan wajib
dipakai:

| Kebutuhan | Yang sudah ada | Lokasi |
| --- | --- | --- |
| Isian formulir | `BaseTextField`, `BaseSelectField`, `BaseTextareaField`, `BaseSimpleCheckbox` | `src/components/ui/form-pemeriksaan-ui` |
| Tabel dan penyaring | `DataTable`, `DataFilter` | `base-features` |
| Gaya layar IGD | Token desain, hero gradien, blok LIST TABLE, pagination | `src/style/health-services/emergency-installation-management/emergency-triage/emergency-triage.module.css` |
| SOAP, tanda vital, pengkajian nyeri | Bentuk yang sudah dipakai antrean dokter | folder `doctor-queue` |
| Pembungkus balasan API | `unwrap`, `unwrapPaged`, `normalizeError` | `emergency-assessment-slice.jsx` |

Alasannya bukan estetika. Perawat yang sama memakai beberapa layar dalam satu shift; layar
yang tampil berbeda terbaca sebagai dua aplikasi, dan urutan isian yang berbeda memperlambat
pekerjaan yang sudah dihafal.

---

## 2. Layar yang terdampak

| Layar | Status | Route sekarang |
| --- | --- | --- |
| Pendaftaran IGD | **Diperbarui** | `/health-services/registration-management/emergency-registration` |
| Triase | **Diperbarui** | `/health-services/emergency-installation-management/emergency-triage` |
| Pengkajian IGD | **Diperbarui** | `/health-services/emergency-installation-management/emergency-assessment` |
| Daftar pantau pengkajian ulang | **Baru** | `DEV_DISCRETION` |

Route untuk layar baru **tidak ditetapkan** di sini. Yang ditetapkan hanyalah bahwa ia harus
dapat dicapai dari layar pengkajian tanpa perawat kehilangan konteks pasien.

---

## 3. Pendaftaran IGD

### 3.1 Perubahan wajib

| Perubahan | Sebab | Sifat |
| --- | --- | --- |
| Payload encounter mengirim `EncounterType.Emergency` | `IGD-DEC-074` | **Memutus** — test `FE-IGD-001 K1` wajib diperbarui dalam task yang sama |
| Tidak lagi mengirim kelas pasien | `IGD-DEC-076` — backend menetapkannya sendiri | Menghapus field dari payload |
| Menangani penolakan `409` kunjungan ganda | `IGD-DEC-084` | Baru |

### 3.2 Penolakan kunjungan ganda

Ketika backend menolak `409` karena pasien masih punya kunjungan IGD aktif, layar **wajib**:

1. menampilkan nomor kunjungan yang sudah ada beserta waktu kedatangannya;
2. menyediakan cara membuka kunjungan itu tanpa mengetik ulang apa pun;
3. **tidak** menghapus isian yang sudah diketik petugas.

Bentuk tampilannya — dialog, panel, atau baris peringatan — `DEV_DISCRETION`.

Jalan keluar beralasan disediakan, tetapi **tidak** ditampilkan sebagai tombol setara. Ia
harus menuntut tindakan sadar dan alasan tertulis, karena memakainya menghasilkan dua episode
klinis untuk satu orang.

### 3.3 Kegagalan sebagian yang sudah ada

Layar sudah menangani keadaan "encounter berhasil, kunjungan IGD gagal" dengan menahan hasil
langkah pertama. Perilaku itu **dipertahankan** dan tidak boleh dihilangkan saat menambahkan
perubahan di atas.

---

## 4. Triase

| Perubahan | Sebab |
| --- | --- |
| Penetapan dokter memakai grup `Emergency Doctor Assignment`, bukan `PATCH /patient-encounters/{id}/doctor` | `IGD-DEC-082` |
| Menampilkan riwayat penugasan dokter, bukan hanya dokter sekarang | `IGD-DEC-082` |
| Pengalihan dokter menuntut alasan | `IGD-DEC-082` |
| Menangani `409` saat kunjungan sudah tertutup | `IGD-GAP-014` |

Riwayat dokter ditampilkan sebagai daftar berurutan waktu: dokter, sejak kapan, sampai kapan,
alasan pengalihan. Baris yang sedang aktif dibedakan. Bentuknya `DEV_DISCRETION`.

---

## 5. Pengkajian IGD

### 5.1 Tab yang berubah kemampuannya

| Tab | Sekarang | Setelah revisi ini | Bergantung pada |
| --- | --- | --- | --- |
| Assesmen Awal IGD | Hanya membaca | **Dapat menyimpan** | `IGD-DEC-068` — menunggu pemilik `ClinicalManagement` |
| Resep | Hanya membaca | **Dapat menyimpan** | `IGD-DEC-068` — menunggu pemilik `PharmacyManagement` |
| Tanda Vital | Dapat menyimpan | Ditambah riwayat versi | `IGD-DEC-080` |
| SOAP, Catatan Terintegrasi | Dapat menyimpan | Ditambah riwayat versi | `IGD-DEC-080` |
| Transfer Pasien | Dapat menyimpan | **Dirombak** menjadi kepergian dua rangkaian | `IGD-DEC-069`, `070` |
| Tindak Lanjut | Dapat menyimpan | Ditambah daftar sikap pesanan | `IGD-DEC-078` |
| Observasi, Nosokomial | Dapat menyimpan | Tidak berubah | — |

> Dua baris pertama **tidak dapat dikerjakan** sebelum pemilik modulnya ditunjuk. Layar boleh
> disiapkan, tetapi tombol simpannya tidak akan berfungsi. Menyembunyikan keterbatasan ini
> dari perawat dilarang — lihat bagian 8.

### 5.2 Riwayat versi catatan klinis

Catatan yang sudah dikoreksi wajib dapat ditelusuri. Yang **wajib** terlihat:

1. nilai yang berlaku sekarang, ditandai jelas sebagai yang berlaku;
2. nilai sebelumnya beserta pelaku, waktu, dan alasan koreksi;
3. urutan koreksi menurut waktu.

Nilai lama **tidak boleh** ditampilkan berdampingan dengan nilai berlaku tanpa pembeda yang
tegas — perawat harus dapat mengetahui mana fakta klinis yang berlaku dalam sekali lihat.

Bentuk penyajiannya `DEV_DISCRETION`.

### 5.3 Daftar sikap pesanan

Muncul sebelum dokumen serah terima diajukan. Setiap pesanan menampilkan nama, jenis, dan tiga
pilihan sikap: sudah dikerjakan, dibatalkan, diteruskan.

**Wajib** ditampilkan bersamanya: keterangan bahwa **pemeriksaan penunjang belum dapat
dihitung sistem** (`IGD-DEC-087`). Keterangan ini muncul di layar, bukan hanya di dokumen.
Tanpa itu perawat akan mengira daftarnya lengkap.

Pembatalan pesanan menuntut alasan. Tombol ajukan dokumen tetap tidak aktif selama masih ada
pesanan tanpa sikap — **tetapi** tombol berangkat dan tiba **tetap aktif**.

### 5.4 Kepergian pasien — dua rangkaian

Layar menampilkan **dua** rangkaian status berdampingan, bukan satu:

```text
Fisik    :  Disiapkan  →  Berangkat  →  Tiba
Dokumen  :  Diajukan   →  Tertunda   →  Diterima / Ditolak
```

| Aturan tampilan | Sebab |
| --- | --- |
| Kombinasi fisik `Tiba` + dokumen `Tertunda` ditampilkan **normal**, bukan sebagai galat | `IGD-DEC-070` |
| Pemilik klinis pasien saat ini **selalu** terlihat | `IGD-DEC-072`, `IGD-GAP-015` |
| Tombol tindakan fisik **tidak pernah** dinonaktifkan oleh keadaan dokumen | `IGD-DEC-070`, `078` |
| Tombol catat kedatangan hanya aktif bagi petugas berwenang atas unit tujuan | `IGD-DEC-086` |
| Penolakan `403` kewenangan unit menjelaskan sebabnya, bukan sekadar "tidak berhak" | Kegunaan |

Formulir SBAR memakai empat isian. Setiap isian punya penanda "tidak dapat diisi saat ini"
beserta kolom alasan. Tiga bagian otomatis — alergi, tanda vital terakhir, tingkat kegawatan —
ditampilkan sebagai isi yang tidak dapat diketik.

---

## 6. Daftar pantau pengkajian ulang

Layar baru. Meniru daftar pelampauan batas waktu triase yang sudah ada.

| Aturan | Sebab |
| --- | --- |
| Baris dengan interval belum dikonfigurasi **tetap ditampilkan**, ditandai "interval belum ditetapkan" | `IGD-DEC-083` |
| Baris itu **tidak** dihitung sebagai terlambat maupun patuh | `IGD-DEC-083` |
| Layar **tidak pernah** menonaktifkan tindakan klinis apa pun | `IGD-DEC-060`, `083` |
| Disaring menurut unit tempat pengguna bertugas | `IGD-DEC-086` |

---

## 7. Kontrak data, muat ulang, dan kegagalan

| Aspek | Aturan |
| --- | --- |
| Kesegaran data | Daftar pasien dan daftar pantau dimuat ulang saat layar dibuka dan saat penyaring berubah. Tidak ada polling otomatis pada revisi ini — `IGD-TRQ-07` |
| Pembatalan permintaan | Permintaan yang tertinggal saat pengguna berpindah pasien wajib dibatalkan agar data pasien lain tidak muncul |
| Kirim ganda | Tombol simpan dinonaktifkan selama permintaan berjalan. Untuk tindakan yang mengubah kepemilikan pasien — catat kedatangan, terima serah terima — kirim ganda **wajib** ditolak di backend juga |
| Data basi | Bila backend menolak `409` karena status sudah berubah pihak lain, layar memuat ulang data lalu menampilkan keadaan terbaru; isian pengguna tidak dibuang |
| Sedang memuat | Kerangka isi, bukan layar kosong |
| Kosong | Menyebutkan penyaring yang sedang aktif |
| Galat | Pesan dari backend ditampilkan apa adanya, ditambah tombol coba lagi |
| `403` | Menjelaskan apakah yang kurang adalah kemampuan atau penugasan unit |

---

## 8. Privasi dan hal yang tidak boleh disembunyikan

| Aturan | Sebab |
| --- | --- |
| Nama sementara pasien tanpa identitas ditampilkan apa adanya, tidak diganti tebakan | `IGD-DEC-007` |
| Isi klinis tidak masuk ke log peramban maupun `console` | `IGD-DEC-006` |
| Bagian yang belum tersambung ditandai apa adanya, **tidak** menampilkan data contoh | Sudah menjadi konvensi layar pengkajian |
| Keterbatasan penunjang dinyatakan di layar | `IGD-DEC-087` |
| Kolom yang backend-nya belum mengirim nama **tidak** ditampilkan sebagai tanda hubung tanpa penjelasan | Pelajaran `BE-IGD-016` |

Butir terakhir berasal dari cacat nyata: layar pernah menampilkan lima kolom yang tidak pernah
mungkin terisi, dan tidak ada yang menyadarinya selama berminggu-minggu.

---

## 9. Aksesibilitas dan perilaku layar

| Aspek | Aturan | Kewenangan |
| --- | --- | --- |
| Warna kategori triase | Diambil dari `ColorHex` master, bukan dipetakan di frontend. Warna teks dihitung dari kontras | Sudah dikunci, ada test-nya |
| Warna sebagai satu-satunya pembeda | Dilarang. Status wajib punya label teks | Invariant |
| Ukuran layar | Layar IGD dipakai di komputer meja dan tablet di sisi pasien | `DEV_DISCRETION` untuk titik hentinya |
| Urutan fokus papan ketik | Mengikuti urutan kerja perawat, bukan urutan kolom di kode | Konvensi |

---

## 10. Ketergantungan test

| Test | Keadaan | Tindakan |
| --- | --- | --- |
| `tests/unit/emergency-registration-payload.test.mjs` `FE-IGD-001 K1` | **Akan gagal** setelah `IGD-DEC-074` | Diperbarui dalam task yang sama; jangan dinonaktifkan |
| `tests/unit/emergency-visit-status.test.mjs` | Tetap berlaku | — |
| `tests/unit/emergency-triage-utils.test.mjs` | Tetap berlaku | — |
| Test baru untuk dua rangkaian status | Belum ada | Wajib dibuat bersama layar kepergian |
| Test baru untuk daftar sikap pesanan | Belum ada | Wajib dibuat |

---

## 11. Yang sengaja tidak ditetapkan

| Hal | Alasan |
| --- | --- |
| Route layar daftar pantau | `DEV_DISCRETION` |
| Bentuk tab, modal, atau drawer | `DEV_DISCRETION` |
| Urutan menu dan sidebar | `DEV_DISCRETION`; tidak ada brief yang sah |
| Palet warna baru | Dilarang — salin dari `emergency-triage.module.css` |
| Pustaka komponen baru | Dilarang — pakai `form-pemeriksaan-ui` dan `base-features` |
| Pembaruan realtime | `IGD-TRQ-07`, `LATER SLICE` |

---

## 12. Pemantauan observasi bertanda vital — 16 September 2026

Menurunkan `IGD-DEC-122` sampai `IGD-DEC-126`. Layar yang disentuh adalah tab **Observasi** di
dalam menu **Asuhan Keperawatan** pada layar Pengkajian Pasien IGD — layar anak yang sudah ada,
**bukan** butir menu baru.

### 12.1 Peta butir menu

| Butir menu | Tingkat | Induk | Route | Layar | Hak akses penjaga |
| --- | :-: | --- | --- | --- | --- |
| Pengkajian Pasien | 2 | Instalasi Gawat Darurat | `/health-services/emergency-installation-management/emergency-assessment` | Daftar pasien | `EmergencyVisit : Read` |
| — (layar anak) | — | Pengkajian Pasien | `.../emergency-assessment/{token}` | Workspace pengkajian, tab **Observasi** | `EmergencyObservation : Read`, `EmergencyObservationDetail : Read` |

**Tidak ada butir menu baru.** Bagian Tanda Vital pada pemantauan adalah wilayah di dalam tab
Observasi yang sudah ada.

### 12.2 Skema wilayah tab Observasi sesudah perubahan

```text
┌ Periode Observasi ─────────────────────────────────────────────┐
│ Buka periode: indikasi, rencana, lokasi, waktu mulai           │  (sudah ada)
│ Daftar periode + status + aksi Selesaikan/Eskalasi/Batalkan    │  (sudah ada)
│ Kesimpulan saat Selesaikan                                     │  (FE-IGD-024)
├ Konteks Klinis (baca saja) ────────────────────────────────────┤
│ Ringkasan ABCDE penilaian triase terakhir                      │  BARU, baca saja
├ Catat Pemantauan ──────────────────────────────────────────────┤
│ Waktu                                                          │  (sudah ada)
│ TANDA VITAL:  ( ) Catat baru   ( ) Pilih yang sudah ada        │  BARU
│    - Catat baru  → formulir tanda vital yang sudah dipakai     │
│                    tab Assesmen Awal                           │
│    - Pilih       → daftar tanda vital kunjungan ini            │
│ Keadaan klinis · Tindakan · Respons pasien                     │  (sudah ada)
│ Cairan masuk · urine · keluaran lain · perdarahan · muntah     │  (sudah ada)
│ Catatan                                                        │  (sudah ada)
├ Riwayat Pemantauan ────────────────────────────────────────────┤
│ Waktu · nama pencatat · angka tanda vital · GCS · kesadaran    │  diperluas
│ · oksigen · keluaran · keterangan                              │
└────────────────────────────────────────────────────────────────┘
```

### 12.3 Sumber data, keadaan kosong, dan keadaan gagal per wilayah

| Wilayah | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- |
| Ringkasan ABCDE | Penilaian triase terakhir yang sudah dimuat workspace; **tanpa endpoint baru** | `EmergencyTriage : Read` | "Belum ada penilaian triase pada kunjungan ini." | Bagian disembunyikan; tidak menahan pencatatan pemantauan |
| Tanda vital — catat baru | `POST /v1/health-services/clinical-management/patient-vital-signs` | `PatientVitalSign : Create` | — | Pesan backend apa adanya; pemantauan **tidak** dikirim sebelum tanda vital tersimpan |
| Tanda vital — pilih yang sudah ada | `GET /v1/health-services/clinical-management/patient-vital-signs?patientId=&encounterId=` | `PatientVitalSign : Read` | "Belum ada tanda vital pada kunjungan ini. Catat tanda vital baru." | Pesan backend; pilihan tetap boleh dikosongkan |
| Simpan pemantauan | `POST .../emergency-observation-details` | `EmergencyObservationDetail : Create` | — | Pesan backend apa adanya, termasuk `409` periode sudah ditutup |
| Riwayat pemantauan | `GET .../emergency-observation-details?emergencyObservationId=` beserta proyeksi `vitalSign` dan `recordedByName` | `EmergencyObservationDetail : Read` | "Belum ada pemantauan pada periode ini." | Pesan backend + tombol coba lagi |

### 12.4 Aturan layar yang mengikat

1. **Angka tanda vital tidak pernah dikirim** ke `emergency-observation-details`. Yang dikirim
   hanya `patientVitalSignId` (`IGD-DEC-122`).
2. Daftar pilih tanda vital **wajib** memakai penyaring `patientId` **dan** `encounterId`.
   Menyaring di browser dilarang — pelajaran `IGD-EV-112` dan `IGD-EV-117`.
3. Angka pada riwayat dibaca dari proyeksi backend, bukan dari isian formulir, sehingga koreksi
   tanda vital oleh pemiliknya ikut terbaca.
4. Nilai kosong ditampilkan sebagai tanda hubung. Kosong berarti **tidak diukur**, bukan nol
   (`IGD-DEC-056`).
5. GCS ditampilkan sebagai E/V/M beserta total **hanya bila** backend mengirimnya; layar
   **tidak** menghitung total sendiri dan **tidak** menyediakan GCS teks bebas seperti V1.
6. Periode `Completed` dan `Cancelled` **tidak** menampilkan jalan masuk pencatatan pemantauan
   normal; `409` backend tetap menjadi penjaga terakhir (`IGD-DEC-126`).
7. Nama pencatat ditampilkan dari `recordedByName`. Bila kosong, tampilkan tanda hubung —
   **bukan** GUID.
8. Tidak ada isian obat, gambaran EKG, atau DC Shock pada layar pemantauan.
9. Aksi Selesaikan beserta isian Kesimpulan milik `FE-IGD-024` **tidak berubah**.

### 12.5 Wewenang yang didelegasikan

| Hal | Wewenang |
| --- | --- |
| Bentuk pemilihan "catat baru" lawan "pilih yang sudah ada" — radio, tab, atau tombol | `DEV_DISCRETION`, mengikuti komponen yang sudah ada |
| Susunan kolom riwayat dan urutannya | `DEV_DISCRETION` |
| Menampilkan tanda vital sebagai ringkasan satu baris atau tabel | `DEV_DISCRETION` |
| Isi data yang ditampilkan, sumber datanya, dan aturan 12.4 | **Bukan** `DEV_DISCRETION` — dikunci keputusan |

---

## 13. Encounter-first — 22 September 2026 (**Rencana (belum tersedia)**)

Kontrak fungsional layar untuk sub-slice `S1`…`S6`, `S8` (`IGD-DEC-139`, `142`…`148`, `150`…`154`). Sumber
data: API `0.11.0` §8; pesan: validation `0.8.0` §10. Kelayakan dokter jaga (`S7`) tidak dirancang.

### 13.1 Hierarki kewenangan untuk bagian ini

| Lapis | Isi yang mengikat |
| --- | --- |
| Keamanan / privasi / invariant | Satu pasien satu episode terbuka ditegakkan **backend**; layar tidak menghitung episode sendiri. Waktu tiba **tidak pernah** berasal dari jam browser. Identitas pengguna ditampilkan sebagai nama, bukan GUID. Alasan override/NoShow tidak ditampilkan pada daftar umum di luar layar IGD |
| Brief produk yang disetujui | Keputusan pemilik 22 September 2026 (`IGD-DEC-142`…`154`) |
| Konvensi proyek | Komponen dan gaya yang sudah ada; nol CSS global baru |
| `DEV_DISCRETION` | Bagian 13.6 |

### 13.2 Peta butir menu

Nol butir menu baru. Seluruh layar slice ini adalah layar yang sudah ada atau layar anaknya.

```text
Instalasi Gawat Darurat                  <- tingkat 0 (sudah ada)
├── Pendaftaran Pasien                    -> /health-services/registration-management/emergency-registration
├── Triage Pasien                         -> /health-services/emergency-installation-management/emergency-triage
│   └── (layar anak) Detail triage        -> .../emergency-triage/[slug]
└── Pengkajian Pasien                     -> /health-services/emergency-installation-management/emergency-assessment
```

| Butir menu / layar | Tingkat | Induk | `pathname` | Layar pada bagian ini | Butir hak akses | Status |
| --- | :-: | --- | --- | --- | --- | --- |
| Instalasi Gawat Darurat | 0 | — | — | — | — | Sudah ada |
| Pendaftaran Pasien | 1 | Instalasi Gawat Darurat | `/health-services/registration-management/emergency-registration` | 13.3 A | `PatientEncounter : Create` | Sudah ada — **isi berubah** |
| Triage Pasien | 1 | Instalasi Gawat Darurat | `/health-services/emergency-installation-management/emergency-triage` | 13.3 B, C | `EmergencyVisit : Read` | Sudah ada — **sumber data dan aksi berubah** |
| Detail triage (layar anak) | — | Triage Pasien | `.../emergency-triage/[slug]` | 13.3 D | `EmergencyTriage : Read` | Sudah ada — **panel waktu tiba ditambahkan** |
| Rekonsiliasi encounter (admin) | — | — | — | **Tidak ada layar** pada slice ini | `EmergencyEncounterReconciliation : Read` | `IGD-DEC-162` (menjawab `IGD-OQ-107`) |

### 13.3 Skema fitur per layar

#### A. Pendaftaran Pasien — langkah Verifikasi

```text
+- Pendaftaran Pasien IGD — Verifikasi -------------------------------------+
| Pasien: <nama> · RM <no>          Penjamin: <...>     Keluhan: <...>       |
+---------------------------------------------------------------------------+
| [!] Episode IGD masih terbuka (dari pra-cek active-episode)               |
|     Kunjungan IGD-0012 · Tiba 09.35            [Buka Kunjungan IGD]        |
|     — atau — Encounter REG-…-0012 · Menunggu Triage sejak 09.35            |
|     [ ] Pendaftaran kedua memang sah                                       |
|         Alasan pendaftaran ganda: [____________________] (wajib bila dicentang) |
+---------------------------------------------------------------------------+
|                                           [Kembali]   [Simpan Pendaftaran] |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Pasien, penjamin, keluhan utama | State langkah sebelumnya | — | — |
| Peringatan episode | Kunjungan **atau** encounter yang masih terbuka | `GET /emergency-visits/active-episode` — ruas `visit` atau `encounter` baru | `EmergencyVisit : Create` | Pra-cek gagal → lanjut (`fail-open`, keputusan `FE-IGD-034`); penjaga backend tetap menolak |
| Alasan pendaftaran ganda | Isian teks, maks. 500 | Dikirim sebagai `duplicateEpisodeOverrideReason` pada `POST /patient-encounters` | `PatientEncounter : Create` | `409` backend ditampilkan **apa adanya** (validation §10.1 aturan 3) |
| Simpan | Satu permintaan pembuatan encounter; **tidak** ada `POST /emergency-visits` untuk pasien beridentitas | `POST /patient-encounters` | `PatientEncounter : Create` | Tombol nonaktif selama memproses (cegah kirim ganda) |

**Langkah "Kunjungan IGD" pada wizard.** Isian **waktu tiba dihapus** dari loket (`IGD-DEC-147`). Nasib ruas
kunjungan lain (cara datang, jenis kasus, lokasi/waktu trauma, penanda tanpa identitas) mengikuti
`IGD-DEC-161` (menjawab `IGD-OQ-106`): pindah ke Mulai Triage, dan keluhan utama tetap di loket karena encounter sudah
punya ruasnya.

**Pasien tanpa identitas** (`IGD-DEC-151`): dicari/didaftarkan lewat langkah Pasien yang sudah ada sebagai
rekam pengganti. Tidak ada jalur "lewati pasien".

#### B. Triage Pasien — daftar *Menunggu Triage*

```text
+- Triage Pasien ------------------------------------------------------------+
| [cari nama / RM / no. encounter / no. kunjungan]      [Status v]           |
+----------------------------------------------------------------------------+
| Pasien | No. | Status            | Waktu            | Aksi                  |
| Rayyan | REG-…-0012 | Menunggu Triage | Terdaftar 09.35 | [Mulai Triage] [Tangani Segera] [Pergi] |
| Budi   | IGD-0011   | Menunggu Triage | Tiba 09.20      | [Isi Triage] [Tangani Segera] |
| Tn. X  | IGD-0010   | Sedang ditangani| Tiba 09.05      | [Buka]                |
+----------------------------------------------------------------------------+
| memuat -> kerangka baris                                                    |
| kosong -> "Tidak ada pasien yang menunggu triage."                          |
| gagal  -> "Daftar triage gagal dimuat."                      [Coba lagi]   |
+- Halaman 1 dari n --------------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Tabel | Satu baris per episode | `GET /emergency-visits/triage-queue` (satu sumber; `GET /emergency-visits` **tidak** dipakai daftar ini) | `EmergencyVisit : Read` | Lihat skema |
| Kolom waktu | Baris tanpa kunjungan: **"Terdaftar hh.mm"** dari `registeredAt`; baris kunjungan: **"Tiba hh.mm"** dari `arrivalDateTime` | Sama | — | Label **wajib** berbeda |
| Tombol per baris | Diturunkan dari `availableActions` saja | Sama | `StartTriage`/`ImmediateCare`: `EmergencyVisit : Create`; `NoShow`: `EmergencyVisit : NoShow`; `FillTriage`: `EmergencyTriage : Create` | Tombol yang hak aksesnya tidak dimiliki tidak ditampilkan |
| Aksi **Pergi sebelum ditriage** | Konfirmasi + isian alasan wajib | `POST /emergency-visits/no-show` | `EmergencyVisit : NoShow` | Pesan backend apa adanya |

#### C. Mulai Triage (dari baris tanpa kunjungan)

```text
+- Mulai Triage — Rayyan · REG-…-0012 --------------------------------------+
| Waktu tiba*: [22-09-2026 09:35]   (terisi awal dari waktu terdaftar)       |
| Cara datang: [v]   Jenis kasus: [v]   Keluhan: [dari pendaftaran...]       |
| [ ] Pasien tanpa identitas   Nama sementara: [_______]                     |
+----------------------------------------------------------------------------+
|                                               [Batal]   [Mulai Triage]      |
+----------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Gagal |
| --- | --- | --- | --- | --- |
| Waktu tiba | Wajib; terisi awal `registeredAt` dari baris daftar; boleh dimundurkan; tidak boleh di masa depan | Dikirim ke `POST /start-triage` (`mode = Triage`) | `EmergencyVisit : Create` | `400` ditampilkan di bawah isian |
| Ruas opsional | Cara datang, jenis kasus, keluhan, penanda tanpa identitas | Master yang sudah ada; keluhan awal dari encounter | — | `IGD-DEC-161` |
| Hasil | `201`/`200` → langsung membuka **Detail triage** kunjungan itu | Respons `start-triage` | — | `409` (encounter berakhir, K4, kunjungan lain berjalan) → pesan apa adanya, baris dimuat ulang |

**Tangani Segera** dari baris tanpa kunjungan: konfirmasi pola yang sudah ada (`FE-IGD-030`), **tanpa isian**,
lalu `POST /start-triage` `mode = ImmediateCare`, lalu membuka Pengkajian IGD seperti hari ini.

#### D. Detail triage — panel waktu tiba

```text
+- Waktu tiba --------------------------------------------------------------+
| [!] Waktu tiba masih sementara: 09.35 (waktu terdaftar).                  |
|     Konfirmasi atau koreksi sebelum menyimpan triage.                     |
|     Waktu tiba: [22-09-2026 09:20]                  [Konfirmasi]           |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Gagal |
| --- | --- | --- | --- | --- |
| Panel | Tampil bila `arrivalTimeSource` = `0` atau `1`; bila `2`, tampil ringkas "Tiba 09.20 · dikonfirmasi <nama>" | `GET /emergency-visits/{id}` (ruas baru §8.3.4) | `EmergencyVisit : Read` | — |
| Konfirmasi | Menyimpan waktu tiba | `PATCH /emergency-visits/{id}/arrival-time` | `EmergencyVisit : Update` | `409` menyebut peristiwa penghalang — ditampilkan apa adanya |
| Simpan triage | **Layar** meminta konfirmasi lebih dulu bila masih sementara; **backend tidak menolak** triage karena ini (`IGD-DEC-160`, menjawab `IGD-OQ-105`) | — | — | — |

### 13.4 Aturan layar yang mengikat

| Aturan | Keputusan |
| --- | --- |
| Nilai waktu tiba **tidak pernah** dari jam browser; fungsi yang jatuh ke `new Date()` untuk waktu tiba dihapus dari jalur ini (`IGD-EV-141`) | `IGD-DEC-147` |
| Waktu terdaftar **tidak pernah** diberi label "Tiba" | `IGD-DEC-147` |
| Layar tidak menghitung sendiri "episode terbuka" maupun aksi yang tersedia | `IGD-DEC-139` |
| Pesan penolakan backend ditampilkan apa adanya | Validation §10 |
| Tombol simpan/aksi nonaktif selama permintaan berjalan | Duplikat kirim |
| Loket berhenti memanggil `POST /emergency-visits` **hanya sesudah** penjaga backend (`BE-IGD-053`) aktif | Integration §5; roadmap frontend R3.12 |

### 13.5 Label status

| Keadaan | Label tampilan (usulan desain, bunyi wajib diperiksa pemilik — gate §5.2) |
| --- | --- |
| Encounter tanpa kunjungan, belum berakhir | **Menunggu Triage** |
| Encounter `NoShow` bertipe Emergency | **Pergi sebelum ditriage** — **bukan** "Tidak Hadir" (`IGD-DEC-142`) |
| `arrivalTimeSource = 1` | **Waktu tiba sementara** |
| `arrivalTimeSource = 0` | **Waktu tiba belum dikonfirmasi** |

### 13.6 Wewenang yang didelegasikan (`DEV_DISCRETION`)

Bentuk Mulai Triage (dialog, laci, atau halaman); letak panel waktu tiba pada Detail triage; urutan
kolom dan tombol daftar; ikon; bentuk isian tanggal-jam — selama isi, sumber data, dan aturan 13.4 terpenuhi.

### 13.7 Ketergantungan test

| Test yang ada | Nasib |
| --- | --- |
| `tests/unit/emergency-registration-payload.test.mjs` — `FE-IGD-029 K1`/`K4` (payload kunjungan dari loket) | Usang begitu loket berhenti membuat kunjungan untuk pasien beridentitas — diperbarui pada task yang sama, bukan dihapus diam-diam |
| `tests/unit/emergency-registration-existing-visit.test.mjs` (`FE-IGD-034`) | Diperluas untuk ruas `encounter` |
| `tests/unit/emergency-visit-status.test.mjs` | Tetap; tambah kasus baris tanpa kunjungan |

## 14. Penutupan lewat disposisi — 23 September 2026

Slice `IGD-DEC-163`…`169` **tidak menambah layar dan tidak menambah butir menu** (`IGD-DEC-168`). Seluruh
perubahan frontend menumpang layar daftar kunjungan IGD yang sudah ada.

**Status bagian ini: `draft`**.

### 14.1 Peta butir menu

**Nol butir menu baru.** Daftar kunjungan IGD sudah ada di menu IGD beserta hak aksesnya
(`EmergencyVisit : Read`); saringan baru muncul di dalam layar itu.

### 14.2 Perubahan pada layar daftar kunjungan IGD

| Wilayah | Isi | Sumber data | Hak akses | Keadaan kosong / gagal |
| --- | --- | --- | --- | --- |
| Saringan | Satu pilihan baru "Menunggu penutupan" pada panel saringan yang sudah ada | `GET /emergency-visits?awaitingClosure=true` | `EmergencyVisit : Read` | Kosong: *"Tidak ada kunjungan yang menunggu penutupan."* Gagal: pesan galat umum daftar, saringan tidak mengubah keadaan lain |
| Baris daftar | Penanda pada baris yang menunggu penutupan, beserta alasan penahannya | Ruas `isAwaitingClosure` dan `awaitingClosureReason` pada response yang sama | `EmergencyVisit : Read` | Alasan kosong: penanda tetap tampil tanpa keterangan tambahan |
| Jumlah | Jumlah kunjungan yang menunggu penutupan terbaca dari `totalData` saat saringan aktif | Response yang sama | `EmergencyVisit : Read` | — |

**Rupa penanda dan letak saringan `DEV_DISCRETION`** — warna, ikon, bentuk lencana, dan posisi kolom mengikuti
design system yang sudah dipakai layar itu. Yang **tidak** boleh berubah: alasan penahan wajib terbaca petugas
tanpa membuka detail, karena gunanya justru memberitahu apa yang harus dibereskan.

### 14.3 Yang sengaja tidak dibuat di frontend

| Tidak dibuat | Sebab |
| --- | --- |
| Layar khusus "menunggu penutupan" | `IGD-DEC-168` memilih saringan pada layar yang sudah ada |
| Tombol "tutup sekarang" pada daftar | Penutupan bukan tindakan terpisah; ia mengikuti pembereskan penahan. Tombol semacam itu akan menabrak penjaga penutupan dan membingungkan petugas |
| Pemberitahuan otomatis saat kunjungan tertutup sendiri | Belum ada keputusan pemiliknya; kunjungan yang tertutup cukup hilang dari saringan |

## 15. Ruang Kerja Dokter IGD — 6 Oktober 2026 (**Rencana (belum tersedia)**)

Kontrak fungsional untuk `IGD-DEC-220`…`229`. Status **`approved`** (`IGD-DEC-230`, Rizki Gunawan, 6 Oktober 2026). Brief pemilik: layar
pemeriksaan IGD yang sekarang adalah layar **perawat**; dokter mendapat layar sendiri yang **mengikuti Ruang Kerja Dokter
Rawat Inap** (`/health-services/inpatient-management/doctor-inpatient`). Karena itu susunan layar dokter IGD — daftar
pasien di samping, kartu pasien, dan tab kerja — diambil dari layar rawat inap, bukan dirancang baru.

### 15.1 Hierarki kewenangan untuk bagian ini

| Lapis | Isi yang mengikat |
| --- | --- |
| Keamanan / privasi / invariant | Seluruh aturan klinis ditegakkan **backend**: penjaga diagnosis saat konfirmasi, status awal Draft, penulis tunggal catatan dokter, keunikan kajian medis awal, DPJP untuk tindakan keperawatan. Layar tidak menghitung sendiri dan menampilkan pesan server **apa adanya**. Pengguna ditampilkan dengan nama, bukan GUID |
| Brief produk yang disetujui | `IGD-DEC-220`…`229` — terutama `IGD-DEC-221` (tab), `222` (daftar), `223` (tindak lanjut), `224` (pemesan), `225` (tindakan), `227` (catatan dokter) |
| Konvensi proyek | Komponen dan gaya layar dokter rawat inap dipakai ulang; nol CSS global baru; komponen dasar dari `base-features` |
| `DEV_DISCRETION` | Bagian 15.8 |

### 15.2 Kebutuhan layar

| ID (diberikan pada bagian ini) | Layar | Pemakai | Status |
| --- | --- | --- | --- |
| `SCR-IGD-D01` | Ruang Kerja Dokter IGD — daftar pasien dan enam tab kerja | Dokter | **Baru** |
| `SCR-IGD-D02` | Catatan Saya — draf dan catatan terkunci milik dokter, beserta addendum | Dokter | **Baru** (memakai ulang tampilan rawat inap) |
| `SCR-IGD-D03` | Perlu Verifikasi — pesanan lab dan radiologi yang dibuat perawat atas instruksi dokter itu | Dokter | **Baru** — bergantung `IGD-OQ-117` |
| `SCR-IGD-P01` | Pengkajian Pasien IGD (layar perawat) — tab Tindak Lanjut, Penunjang, Tindakan | Perawat | Sudah ada — **isi berubah** |

### 15.3 Peta butir menu

```text
Instalasi Gawat Darurat                  <- tingkat 0 (sudah ada)
├── Pendaftaran Pasien                    -> /health-services/registration-management/emergency-registration
├── Triage Pasien                         -> /health-services/emergency-installation-management/emergency-triage
├── Pengkajian Pasien                     -> /health-services/emergency-installation-management/emergency-assessment   (layar perawat)
└── Ruang Kerja Dokter                    -> /health-services/emergency-installation-management/doctor-emergency       (BARU)
    ├── Catatan Saya                      -> .../doctor-emergency/my-notes                                             (BARU)
    └── Perlu Verifikasi                  -> .../doctor-emergency/needs-review                                         (BARU)
```

| Butir menu / layar | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :-: | --- | --- | --- | --- | --- |
| Ruang Kerja Dokter | 1 | Instalasi Gawat Darurat | `/health-services/emergency-installation-management/doctor-emergency` | `SCR-IGD-D01` | `EmergencyVisit : Read`, `DoctorConsultation : Read` | **Baru** |
| Catatan Saya | 2 | Ruang Kerja Dokter | `…/doctor-emergency/my-notes` | `SCR-IGD-D02` | `ClinicalDocumentIntegrity : Read` | **Baru** |
| Perlu Verifikasi | 2 | Ruang Kerja Dokter | `…/doctor-emergency/needs-review` | `SCR-IGD-D03` | `LabOrder : Verify` atau `RadOrder : Verify` | **Baru** |
| Pengkajian Pasien | 1 | Instalasi Gawat Darurat | `…/emergency-assessment` | `SCR-IGD-P01` | `EmergencyVisit : Read` | Sudah ada — isi berubah |

Nama route mengikuti pola rawat inap `doctor-inpatient` (`DEV_DISCRETION`). Butir menu didaftarkan admin, bukan oleh kode
(`IGD-UNK-14`). Layar dokter juga dapat dibuka dengan `?visitId=<id>` untuk langsung memilih satu pasien, sama seperti
`?episodeId=` di rawat inap.

### 15.4 Skema fitur per layar

#### A. `SCR-IGD-D01` — Ruang Kerja Dokter IGD

```text
+- Dokter - IGD ------------------------------------------------------------------------------+
| [Pasien IGD berjalan: 18]   [Pasien saya: 6]   [Perlu verifikasi: 2]                         |
+------------------------------+--------------------------------------------------------------+
| [Pasien saya ▾] [Cari _____] | Tn. Budi Santoso · RM 00-00-02-47 · IGD-2610…  DPJP dr. Ani   |
|------------------------------| Instalasi Gawat Darurat · Sedang ditangani · Tiba 09.15        |
| ● Budi Santoso   dr. Ani     | Alergi: Penisilin · Diagnosis kerja: J18.9 Pneumonia           |
|   Sedang ditangani · 09.15   +--------------------------------------------------------------+
| ○ Sari W.        (belum DPJP)| [Pengkajian Medis] [Catatan Dokter] [Resep] [Tindakan]         |
|   Menunggu triage · 09.40    | [Penunjang] [Tindak Lanjut]                                    |
| ○ ...                        |--------------------------------------------------------------|
|                              |  isi tab yang dipilih                                         |
+------------------------------+--------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Jumlah pasien IGD berjalan, pasien saya, perlu verifikasi | `GET /emergency-visits?ongoing=true` (total), `…&doctorId=` (total), worklist verifikasi lab + radiologi (total) | `EmergencyVisit : Read`; `LabOrder : Read`, `RadOrder : Read` | Angka worklist gagal → tampil *"—"*, daftar tetap jalan |
| Daftar pasien | Nama, status kunjungan, waktu tiba, nama DPJP atau *"belum ada DPJP"*; saringan bawaan *Pasien saya*, dapat diganti *Semua pasien IGD*; cari nama/No. RM/nomor kunjungan | `GET /emergency-visits?ongoing=true[&doctorId=<doctorId pengguna>]` | `EmergencyVisit : Read` | Kosong *Pasien saya*: *"Belum ada pasien IGD dengan Anda sebagai DPJP. Ganti saringan ke Semua pasien IGD untuk melihat pasien lain."* Akun tanpa identitas dokter: saringan *Pasien saya* nonaktif dengan keterangan, daftar *Semua* tetap tampil |
| Kartu pasien | Identitas, unit, status, waktu tiba, DPJP, alergi, diagnosis kerja | `GET /emergency-visits/{id}`; alergi dan diagnosis per encounter (modul klinis) | `EmergencyVisit : Read`, `PatientDiagnosis : Read` | Diagnosis kosong → *"Belum ada diagnosis"* |
| Tab Pengkajian Medis | Kajian medis awal dan kajian ulang; daftar diagnosis | `POST`/`GET /patient-assessments` (tipe kajian medis); `GET /patient-diagnoses?encounterId=` | `PatientAssessment : Create/Read`, `PatientDiagnosis : Read` | Penolakan `409` (kunjungan berakhir, kajian awal sudah ada) tampil di tempat aksi |
| Tab Catatan Dokter | Editor SOAP dengan bagian ICD; daftar riwayat catatan dokter; CPPT (dibaca dan ditulis) | `GET /doctor-consultations/encounters/{encounterId}/soap-timeline`; `POST /doctor-consultations`, `PATCH …/soap`, `PATCH …/complete`; diagnosis lewat catatan; `GET /patient-integrated-progress-notes?encounterId=` | `DoctorConsultation : Create/WriteSoap/Complete`, `PatientDiagnosis : Create`, `PatientIntegratedProgressNote : Read/Create` | Catatan selesai terkunci; koreksi lewat addendum (`IGD-DEC-227`) |
| Tab Resep | Resep encounter ini; tulis resep pada catatan dokter yang belum diselesaikan; template | `GET /prescriptions?encounterId=`; `POST /prescriptions`; template resep | `Prescription : Read/Create` | Tanpa catatan dokter terbuka: *"Buat atau buka catatan dokter lebih dulu — resep menempel pada catatan dokter."* |
| Tab Tindakan | Daftar tindakan dokter **dan** tindakan keperawatan; dokter mencatat tindakan pada catatan dokter terbuka | `GET /patient-procedures?encounterId=`; `POST /patient-procedures` | `PatientProcedure : Read/Create` | Sama dengan Resep untuk syarat catatan terbuka |
| Tab Penunjang | Lima jenis: laboratorium, radiologi, bank darah, hemodialisa, gizi — daftar per jenis beserta status dan hasil yang disediakan modulnya; tombol pesan per jenis | `GET` per modul `?encounterId=` (gizi: `?patientId=` lalu disaring encounter di layar); `POST` per modul | Butir `Read`/`Create` tiap modul | Satu jenis gagal dimuat tidak menyembunyikan jenis lain. Bank darah ditolak karena unit belum berizin → pesan modul tampil apa adanya |
| Tab Tindak Lanjut | Buat (Draft), konfirmasi, batalkan; riwayat | `POST /emergency-dispositions`; `PATCH …/disposition-status` | `EmergencyDisposition : Create/Update` | `409` tanpa diagnosis tampil di modal konfirmasi, dengan tautan ke tab Catatan Dokter |

#### B. `SCR-IGD-D02` — Catatan Saya

Memakai ulang tampilan *Catatan Saya* rawat inap (draf dan catatan terkunci, riwayat addendum, tambah addendum).
Sumber: `GET /medical-record-management/clinical-document-integrities/my-authored`, `my-unsigned`; addendum lewat
`…/clinical-note-addendums/by-document/{documentKind}/{documentId}`. Catatan dokter IGD tampil bersama catatan rawat inap
milik dokter yang sama; label asal pelayanan membedakannya.

#### C. `SCR-IGD-D03` — Perlu Verifikasi

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Daftar | Pesanan lab dan radiologi pasien IGD yang dibuat perawat atas instruksi dokter ini dan belum diverifikasi | Worklist verifikasi lab dan radiologi | `LabOrder : Read`, `RadOrder : Read` | *"Tidak ada pesanan yang menunggu verifikasi Anda."* |
| Verifikasi | Tombol verifikasi per baris | `PUT …/verify-instruction` | `LabOrder : Verify`, `RadOrder : Verify` | Penolakan modul tampil apa adanya |

Layar ini baru diaktifkan sesudah `IGD-OQ-117` terjawab.

#### D. `SCR-IGD-P01` — Pengkajian Pasien IGD (layar perawat): yang berubah

| Tab | Sebelum | Sesudah | Dasar |
| --- | --- | --- | --- |
| Tindak Lanjut | *Simpan Draf*, *Konfirmasi*, *Jalankan*, *Batalkan* | Daftar tindak lanjut beserta statusnya; **hanya *Jalankan*** untuk tindak lanjut yang sudah dikonfirmasi | `IGD-DEC-223` |
| Penunjang | Pesanan lab tanpa dokter pemberi instruksi | Pesan lab **dan radiologi** dengan **wajib memilih dokter pemberi instruksi**; daftar status kelima jenis penunjang (baca saja untuk bank darah, hemodialisa, gizi) | `IGD-DEC-224`; `IGD-OQ-117`, `118` |
| Tindakan | Membaca tabel tindakan IGD lama | Membaca tindakan klinis umum `?encounterId=` + formulir *Catat tindakan keperawatan* (`emergency-nursing-actions`); bila ada isi tabel lama, tampil di bagian *Riwayat tindakan lama* (baca saja) | `IGD-DEC-225` |
| Resep, Asuhan Keperawatan, Transfer | — | Tidak berubah | — |

### 15.5 Pemakaian ulang dari layar dokter rawat inap

| Bagian rawat inap | Status | Penyesuaian untuk IGD |
| --- | --- | --- |
| `doctor-inpatient-view.jsx`, `inpatient-physician-patient-list.jsx`, `inpatient-physician-patient-card.jsx`, `inpatient-physician-context-header.jsx` | Reuse with adapter | Sumber daftar = kunjungan IGD (`?ongoing=true&doctorId=`), bukan sensus rawat inap; tanpa tempat tidur dan lama rawat; istilah ringkasan IGD |
| `physician-workspace-context.jsx`, `use-inpatient-physician-workspace` | Reuse with adapter | Konteks = kunjungan IGD dan encounter-nya (`GET /emergency-visits/{id}`), bukan episode |
| Tab Pengkajian Medis (`medical-assessment-tab.jsx`, `use-inpatient-medical-assessment`) | Reuse with adapter | Kajian medis tanpa `inpEpisodeId`; diagnosis per encounter |
| Tab Catatan Dokter (`physician-progress-tab.jsx`, `use-inpatient-progress-note`) | Reuse with adapter | Timeline dari `…/encounters/{encounterId}/soap-timeline` (butir berbentuk sama) |
| CPPT (`integrated-progress-note-tab.jsx`, `use-inpatient-integrated-note`) | Reuse with adapter | `GET …?encounterId=`; verifikasi CPPT oleh DPJP tidak dibawa |
| Tab Resep (`inpatient-prescription-tab.jsx`, `use-inpatient-prescription-tab`) | Reuse with adapter | `GET /prescriptions?encounterId=`; daftar catatan dokter dari timeline encounter |
| Tab Tindakan (`inpatient-procedure-tab.jsx`, `use-inpatient-procedure-tab`) | Reuse with adapter | `GET /patient-procedures?encounterId=`; dokter memakai `POST /patient-procedures` (catatan terbuka), bukan `inpatient-orders` |
| Tab Penunjang (`supporting-service-tab.jsx`, `use-inpatient-supporting-service`) | Reuse with adapter | Daftar per encounter, bukan per episode; tindakan tidak ditawarkan di tab ini karena sudah punya tab sendiri |
| *Catatan Saya* (`my-notes/**`, `use-my-authored-notes`) | Ready to reuse | — |
| Tab Visite, Resume, Skrining | Tidak dibawa | `IGD-DEC-221` |
| Tab Tindak Lanjut | Reuse dari layar perawat IGD | Komponen tindak lanjut IGD yang sudah ada, dengan aksi buat/konfirmasi/batalkan untuk layar dokter |

Penyesuaian dibuat sebagai sumber data yang dapat dipilih komponen (konteks rawat inap atau kunjungan IGD), bukan salinan
komponen. Bila sebuah komponen terlalu terikat rawat inap untuk disesuaikan tanpa mengubah perilaku rawat inap, keputusan
reuse atau buat baru diambil saat build lewat gerbang keputusan komponen dasar.

### 15.6 Penanganan keadaan

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka daftar dan tab memakai indikator muat yang sudah ada |
| Kosong | Kalimat pada tabel 15.4 |
| Gagal | Pesan server apa adanya di tempat aksi; tombol *Coba lagi* pada daftar |
| Data basi | Sesudah aksi berhasil (catatan selesai, resep, tindakan, pesanan, tindak lanjut), tab terkait dan kartu pasien dimuat ulang; kunjungan yang tertutup oleh tindak lanjut ikut hilang dari daftar `ongoing` |
| Kirim ganda | Tombol nonaktif selama permintaan; tindakan keperawatan membawa `idempotencyKey` |
| Hak akses kurang | `403` tampil sebagai pesan, tombol tidak disembunyikan diam-diam bila butirnya belum diketahui |

### 15.7 Aturan layar yang mengikat

1. Layar dokter dan layar perawat **terpisah**; tidak ada tombol buat/konfirmasi tindak lanjut di layar perawat.
2. Resep dan tindakan dokter selalu menempel pada catatan dokter yang belum diselesaikan; layar menuntun dokter membuat
   atau membuka catatan lebih dulu.
3. Konfirmasi tindak lanjut tidak diperiksa di layar; layar mengirim dan menampilkan `409` bila diagnosis belum ada.
4. Pesanan perawat atas instruksi wajib memilih dokter; daftar dokter hanya dokter aktif.
5. Bank darah, hemodialisa, dan gizi tidak dapat dipesan dari layar perawat pada slice ini.
6. Tidak ada tanggal `0001` atau GUID yang tampil kepada pengguna.

### 15.8 Wewenang yang didelegasikan (`DEV_DISCRETION`)

Nama route dan folder komponen, label ringkasan, urutan kolom daftar, ikon tab, dan susunan detail di dalam tab — selama
mengikuti layar dokter rawat inap dan komponen dasar yang sudah ada.

### 15.9 Yang sengaja tidak dibuat di frontend

| Tidak dibuat | Sebab |
| --- | --- |
| Salinan komponen tab rawat inap untuk IGD | Penyesuaian sumber data cukup; salinan menciptakan dua layar yang menua berbeda |
| Tab Visite, Resume, Skrining | `IGD-DEC-221` |
| Pemesanan bank darah, hemodialisa, gizi di layar perawat | `IGD-OQ-118` |
| Unit test baru | Arahan pemilik: verifikasi lewat lint, build, dan uji layar Antigravity |

### 15.10 Ketergantungan uji

Uji layar pada hasil build dengan akun peran nyata: dokter (`DOKTER`), perawat IGD (`dimas.kurniawan`, `IGD-DEC-215`),
penerima. Skenario `AT-IGD-200`…`213` pada PRD §10.3. Bagian *Perlu Verifikasi* dan pesanan perawat atas instruksi baru
dapat diuji sesudah `IGD-OQ-117`.
