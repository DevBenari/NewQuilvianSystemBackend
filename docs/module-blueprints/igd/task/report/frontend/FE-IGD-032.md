# Laporan Perubahan Frontend — `FE-IGD-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-032` |
| Judul | Tata letak tab Observasi dan lembar pemantauan |
| Slice | `IGD-S04` · `EPIC IGD-09` (pemantauan observasi) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.9 |
| Trace | `IGD-DEC-134`, `IGD-DEC-133`; `IGD-DEC-122`…`126` tetap berlaku; evidence [`2026-09-16-tata-letak-riwayat-pemeriksaan.md`](../../../evidence/2026-09-16-tata-letak-riwayat-pemeriksaan.md). **Coverage gap:** tanpa `FR-IGD-*` |
| Contract version | API `0.6.0` bagian 7 dan validation `0.6.0` bagian 9 — **nol perubahan, nol kenaikan versi, nol endpoint baru, nol perubahan backend** |
| Wewenang UI | `03-frontend-architecture.md:330` — penyajian tanda vital sebagai ringkasan satu baris **atau tabel** adalah `DEV_DISCRETION`. Batas `:331` dan aturan bagian 12.4 dipatuhi |
| Dependency | `IGD-DEC-134` ✅; `FE-IGD-031` (pembungkus segmen); `FE-IGD-028` ✅ |
| Klasifikasi | `HIGH` — berkas terbesar modul ini, memuat dua alur tulis yang sudah terbukti |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source) + laporan ini pada blueprint IGD |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `36f122af9` (branch `RizkiV2`) + working tree |
| Commit backend yang dijadikan rujukan | `8544af1c` (branch `rizkiG`), strict read-only |
| Tanggal | 16 September 2026 |
| Status | 🟡 **SEBAGIAN — 16 September 2026.** Empat belas acceptance criteria **terpetakan ke source**, dengan **satu delta pada kriteria 4** dan **satu delta pada kriteria 3** yang dijelaskan di bagian 8. `npm run lint:errors` **PASS**; unit test **866/866 lulus**. **`npm run build` LULUS 17 September 2026** (exit 0, 0 error, 0 warning). **Uji lewat layar belum dijalankan** — dan untuk task ini uji layar adalah syarat yang paling menentukan, karena kriteria 8 dan 9 menahan perilaku yang sudah terbukti sebelumnya. Bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

Tab Observasi (`emergency-assessment-observation-tab.jsx`, 1.398 baris sebelum task ini)
menumpuk lima blok berurutan pada satu kolom:

| Urutan | Blok | Baris lama |
| ---: | --- | --- |
| 1 | Kartu formulir **Buka Periode Observasi** | 876 |
| 2 | Bagian **Periode Observasi** — daftar kartu, satu kartu per periode | 928 |
| 3 | Bagian **Primary Survey Terakhir** — kartu penuh berisi ABCDE baca-saja | 1036 |
| 4 | Kartu formulir **Catat Pemantauan** | 1096 |
| 5 | Bagian **Riwayat Pemantauan** — daftar kartu, satu kartu per putaran | 1252 |

Masalahnya bukan datanya, melainkan bentuknya. Satu putaran pemantauan memakan hampir satu
layar penuh sebagai kartu, sehingga **dua putaran berurutan tidak pernah terlihat bersamaan**.
Padahal yang dicari perawat pada lembar pemantauan bukan satu kejadian, melainkan arah
perubahannya: tekanan darah turun, nadi naik, saturasi turun.

---

## 2. Proses bisnis dari sisi pengguna

**Sebelum.** Perawat membuka tab Observasi, menggulir melewati formulir buka periode, melewati
daftar kartu periode, melewati kartu primary survey, melewati formulir catat pemantauan, lalu
sampai ke riwayat. Untuk membandingkan putaran 14.00 dan 15.00, ia menggulir naik-turun.

**Sesudah.** Perawat membuka tab yang sama dan langsung melihat: baris pemilih periode di atas
dengan periode berjalan **sudah terpilih**, satu baris ringkas primary survey, lalu segmen
**Lembar Pemantauan** yang aktif secara bawaan berisi tabel — empat sampai enam putaran
sekaligus, angkanya berjajar dari baris ke baris. Untuk mencatat putaran baru ia menekan segmen
**Catat Pemantauan**; sesudah tersimpan, layar kembali sendiri ke Lembar Pemantauan dengan baris
barunya di paling atas.

Bawaan **Lembar Pemantauan** dipilih karena membaca tren lebih sering dilakukan daripada
menambah baris.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `ui/doctor-clinical-base/ClinicalDataTable.jsx` | Calon komponen tabel; hasil telaahnya di bagian 8.1 |
| `emergency-assessment-section.jsx` | Memastikan keadaan loading, galat + **Coba lagi**, larangan akses, dan kosong memang ditangani di sana |
| `emergency-assessment-work-panel.jsx` | Pembungkus segmen dari `FE-IGD-031` |
| `emergency-assessment-observation-tab.jsx` | Berkas yang ditata ulang |

### 3.2 Berkas yang berubah

| Berkas | Sifat | Perubahan (tanpa indentasi) |
| --- | --- | --- |
| `…/emergency-assessment-observation-tab.jsx` | Ubah | 359 |
| `…/emergency-assessment-work-panel.jsx` | Ubah | +2 prop: `historyFirst` dan `defaultSegment` |
| `style/…/emergency-assessment.module.css` | Ubah | +251 (pemilih periode, baris primary survey, tabel lembar) |

### 3.3 Yang ditata ulang, satu per satu

| Blok | Sebelum | Sesudah |
| --- | --- | --- |
| Daftar periode | Kartu memanjang, satu kartu per periode | **Baris pemilih** `role="radiogroup"` berisi nomor, status, dan waktu mulai. Rincian dan aksi milik periode terpilih berada tepat di bawahnya |
| Primary survey | Kartu penuh berisi enam ruas | **Satu baris ringkas** A/B/C/D/E, waktu penilaian, dan tanda bahaya |
| Riwayat pemantauan | Daftar kartu | **Tabel 16 kolom** dengan kepala yang menempel saat digulir |
| Formulir catat pemantauan | Kartu keempat pada kolom | Isi segmen **Catat Pemantauan** |

### 3.4 Kolom tabel lembar pemantauan

Enam kolom pertama adalah yang dipakai membaca tren, dan sengaja diletakkan paling kiri supaya
terbaca tanpa menggulir mendatar.

| Kolom | Sumber | Catatan |
| --- | --- | --- |
| Waktu | `recordedAt` | Membawa badge tanda vital kritis/abnormal dari backend |
| Tekanan darah, Nadi, Napas, Suhu, SpO₂ | `vitalSign.*` | Dibaca dari proyeksi backend |
| GCS | `vitalSign.gcsTotal` dan komponennya | **Tidak dihitung layar** |
| Kesadaran, Oksigen | `vitalSign.*` | Lewat peta label yang sudah ada |
| Masuk, Urine, Keluaran lain, Perdarahan, Muntah | `fluidIntakeMl` dan seterusnya | |
| Keadaan, tindakan, dan respons | `clinicalConditionSummary`, `interventionSummary`, `patientResponseSummary`, `notes` | Satu kolom yang membungkus teks; kosong seluruhnya tampil sebagai tanda hubung |
| Pencatat | `recordedByName` | Kosong tampil sebagai tanda hubung, **bukan** GUID |

Satu koreksi kecil yang lahir dari bentuk tabel: pada daftar kartu lama, blok tanda vital hanya
dirender bila tautannya ada. Pada tabel, kolomnya selalu ada. Karena `formatOxygen(null)`
mengembalikan **"Tanpa oksigen tambahan"**, baris tanpa tautan tanda vital akan terbaca seolah
pasien diperiksa dan hasilnya tanpa oksigen. Seluruh kolom tanda vital karena itu dijaga
`vital ? … : "-"`.

---

## 4. State yang ditangani di layar

| State | Keterangan |
| --- | --- |
| Periode terpilih | **Tidak berubah.** Effect pemilihan otomatis milik `FE-IGD-028` dipakai apa adanya: periode berjalan lebih dulu, lalu yang terbaru; pilihan manual dihormati selama periodenya masih ada |
| Segmen aktif | Bawaan **Lembar Pemantauan** lewat `defaultSegment` |
| `detailSavedSignal` | Naik hanya pada `createObservationDetail.fulfilled`. Penolakan `409` **tidak** menaikkannya |

---

## 5. Endpoint yang dikonsumsi

**Nol perubahan.** Tidak ada thunk, URL, parameter, atau bentuk payload yang berubah, dan tidak
ada pemanggilan baru.

---

## 6. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint` pada tab Observasi | **0 error.** Satu peringatan `react-hooks/set-state-in-effect` pada baris 499 — **lama**, milik `FE-IGD-028`, tidak disentuh |
| `npm run lint:errors` (seluruh repo) | **PASS** |
| `node --import ./tests/helpers/register.mjs --test tests/unit` | **866/866 lulus** |

### Yang **belum** dijalankan

| Butir | Keadaan |
| --- | --- |
| `npm run build` | ✅ **LULUS 17 September 2026** — `npm run build` exit 0, **0 error, 0 warning**, postbuild standalone siap; keempat route IGD terkompilasi |
| Uji lewat layar | **Belum**, dan untuk task ini paling menentukan — lihat bagian 7 kriteria 8 dan 9 |
| UAT | **Belum**, bukan milik pekerjaan ini |

---

## 7. Acceptance criteria dan Definition of Done

| No | Kriteria | Keadaan | Bukti pada source |
| ---: | --- | :-: | --- |
| 1 | Periode tampil sebagai baris pemilih ringkas | ✅ source | `styles.periodPicker`, `role="radiogroup"` |
| 2 | Periode berjalan terpilih otomatis | ✅ source | Effect lama `FE-IGD-028` — **sudah ada sebelum task ini**, diperiksa dan tidak diubah |
| 3 | Primary survey menjadi satu baris ringkas, tetap baca saja | 🟡 delta | `styles.primarySurveyStrip`. **Delta:** keadaan memuat dan keadaan kosong ikut hilang — bagian 8.2 |
| 4 | Putaran pemantauan sebagai tabel memakai `ClinicalDataTable` | 🟡 delta | Tabel ✅; **komponennya tidak dipakai** — alasan di bagian 8.1 |
| 5 | Tabel tidak menghitung apa pun sendiri | ✅ source | `formatGcs` memakai `gcsTotal` backend apa adanya |
| 6 | Pencatat dari `recordedByName`, kosong jadi tanda hubung | ✅ source | Kolom terakhir tabel |
| 7 | Segmen Lembar/Catat, Lembar sebagai bawaan | ✅ source | `historyFirst` + `defaultSegment` |
| 8 | Aksi Selesaikan dan isian Kesimpulan `FE-IGD-024` tetap bekerja | ✅ source, **belum terbukti runtime** | `OBSERVATION_STATUS_ACTIONS` dipanggil dari rincian periode; `ConfirmModal` tidak disentuh |
| 9 | Penautan tanda vital `FE-IGD-028` tetap bekerja | ✅ source, **belum terbukti runtime** | Isi formulir dipindahkan apa adanya, nol perubahan di dalamnya |
| 10 | Periode tertutup tetap menolak, `409` tampil apa adanya | ✅ source | Blok `periodeTertutup` dan cabang `409` tidak diubah |
| 11 | Tabel bergulir mendatar tanpa menyeret halaman | ✅ source | `.sheetWrap { overflow-x: auto }`, lebar minimum ada pada `.sheet`, bukan pada pembungkusnya |
| 12 | Nilai kosong tampil sebagai tanda hubung | ✅ source | `formatMl`, `formatNumber`, dan penjaga `vital ? … : "-"` |
| 13 | Nol isian obat, EKG, DC Shock; Resusitasi tidak disentuh | ✅ source | `git status` — satu berkas tab dan satu modul CSS |
| 14 | Nol palet baru, nol pustaka baru, nol CSS global | ✅ source | Warna diambil dari nilai yang sudah dipakai modul CSS ini |

| Butir DoD | Keadaan |
| --- | --- |
| Acceptance criteria terpetakan ke source | ✅ dengan dua delta tercatat |
| `npm run lint:errors` | ✅ PASS |
| Unit test | ✅ 866/866 |
| `npm run build` | ✅ **lulus 17 September 2026** — 0 error, 0 warning |
| Uji lewat layar | 🟡 **belum** |
| Laporan tracked | ✅ berkas ini |
| Roadmap dan traceability diperbarui | ✅ |
| UAT PASS | ❌ **tidak diklaim** |

---

## 8. Catatan penutup

### 8.1 Delta kriteria 4 — `ClinicalDataTable` dipakai sebagai acuan bentuk, bukan sebagai komponen

Kartu task menyebut tabelnya dibangun memakai `ClinicalDataTable`. Saat dikerjakan, komponen itu
ternyata **membawa kepala, badge jumlah, dan empty state sendiri**, dan ketiganya tidak dapat
dimatikan — judulnya selalu dirender sebagai `<h3>`, kosong sekalipun.

Sementara itu `EmergencyAssessmentSection` yang membungkus bagian ini sudah menangani **empat
keadaan yang tidak dimiliki `ClinicalDataTable`**: sedang memuat, galat beserta tombol **Coba
lagi**, larangan akses, dan kosong.

Memakai `ClinicalDataTable` berarti memilih salah satu dari dua kerugian: membuang keadaan
memuat dan galat + Coba lagi, atau menumpuk dua kepala bagian dengan satu `<h3>` kosong di
dalamnya.

Yang dipilih: **`EmergencyAssessmentSection` tetap menangani keadaan**, dan tabelnya ditulis
sebagai `<table>` biasa dengan kelas pada modul CSS layar ini. Hasil yang diminta keputusan —
putaran berjajar sehingga tren terbaca — tercapai penuh. Nol komponen bersama diubah, dan nol
komponen bersama baru dibuat.

**Ini menyimpang dari bunyi kriteria 4 dan pemilik berhak menolaknya.** Bila `ClinicalDataTable`
tetap diwajibkan, jalan yang bersih adalah menambahkan prop untuk mematikan kepalanya pada
komponen bersama itu — dan itu mengubah komponen bersama, yang justru dilarang kriteria 14.

### 8.2 Delta kriteria 3 — baris primary survey menghilang saat tidak ada triase

Kartu lama menampilkan keadaan memuat dan pesan *"Belum ada penilaian triase pada kunjungan
ini"*. Sebagai baris ringkas, keduanya tidak lagi ditampilkan: baris itu hanya muncul bila
penilaian triase memang ada.

Polanya sama dengan baris "terakhir dikaji" pada `FE-IGD-031`, dan alasannya sama: baris ringkas
yang kosong lebih membingungkan daripada tidak ada. Data yang hilang **nol** — ABCDE tetap
dapat dibaca penuh pada layar triase.

### 8.3 Konsekuensi yang perlu dilihat pemilik saat uji layar

Rincian periode — indikasi, rencana, kesimpulan, alasan eskalasi — kini tampil **hanya untuk
periode yang sedang dipilih**. Sebelumnya seluruh periode menampilkan rinciannya sekaligus.
Tidak ada data yang hilang: memilih periode lain menampilkan rinciannya. Tetapi bila pemilik
memang terbiasa membandingkan dua periode berdampingan, ini perlu dinilai ulang.

### 8.4 Yang menahan status ✅

Kriteria 8 dan 9 menahan perilaku milik `FE-IGD-024` dan `FE-IGD-028` yang sudah terbukti
sebelumnya. Keduanya terpetakan ke source dan isinya dipindahkan tanpa diubah, tetapi
**pemindahan tetap pemindahan**. Build bersih sudah didapat 17 September 2026, jadi status ✅
kini menunggu **satu hal saja**: dua percobaan lewat layar — menyelesaikan satu periode observasi
beserta isian Kesimpulan, dan mencatat satu putaran pemantauan bertanda vital.
