# Laporan Live Browser Testing — Riwayat Assesmen Awal IGD Tampil Langsung Setelah Simpan (ISSUE-IGD-003)

| Metadata | Nilai |
| :--- | :--- |
| **Tanggal pengujian** | 4 November 2026 |
| **Menu** | Instalasi Gawat Darurat › Ruang Kerja Pemeriksaan IGD › Asuhan Keperawatan › tab **Assesmen Awal IGD** |
| **Yang diuji** | `FIX-IGD-003-01` (daftar riwayat dimuat ulang setelah simpan berhasil) dan `FIX-IGD-003-02` (teks keterangan segmen Riwayat) — [plan repair 003](../../../docs/plan-repair/plan-repair-003-riwayat-assesmen-awal-kosong-setelah-simpan.md) |
| **Source yang diuji** | Frontend `RizkiV2` @ `739e05f68` + perubahan belum di-commit pada `emergency-assessment-initial-tab.jsx`; backend `rizkiG` @ `30f1396d` |
| **Lingkungan** | Frontend `http://localhost:3000` (`next dev`); backend `https://localhost:7184`; database pengembang bersama |
| **Peramban** | Chromium build 1234, mode tampak (*headed*), `slowMo` 300 ms, layar penuh |
| **Akun** | Peran **Perawat IGD** (kredensial dari variabel lingkungan) — bukan SuperAdmin |
| **Pasien uji** | Kunjungan `IGD-261023022832-A42BE4` (pasien uji pengembang, status `InTreatment`, belum punya pengkajian awal sebelum uji) |
| **Skrip** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-issue-003-riwayat-assesmen-20261104/uji-riwayat-assesmen.mjs` |
| **Bukti** | Folder yang sama: `screenshots/`, `hasil-*.json`, `jaringan-*.json` |
| **Hasil akhir** | **7 dari 7 skenario LULUS** pada putaran 2. Putaran 1: 6 lulus, 1 gagal karena kesalahan skrip uji (bukan aplikasi) — lihat bagian 4 |

---

## 1. Ringkasan

Laporan pelapor (ISSUE-IGD-003): setelah perawat menyimpan Assesmen Awal IGD, segmen **Riwayat
Dokumen Pengkajian** tetap menampilkan "Belum ada assesmen awal untuk pasien ini." padahal data
sudah tersimpan. Penyebabnya, layar tidak meminta ulang daftar riwayat ke server setelah simpan.

Perbaikan mengembalikan pemanggilan muat ulang daftar (`reload()`) setelah simpan berhasil, dan
mengganti teks keterangan "Dimuat ulang dari backend…" dengan kalimat untuk petugas.

Pengujian membuktikan: begitu perawat menekan **Simpan Pengkajian**, layar pindah ke Riwayat dan
pengkajian baru **langsung tampil** — tanpa pindah tab dan tanpa memuat ulang halaman (F5). Badge
jumlah dan baris "Terakhir dikaji" ikut benar. Penyimpanan yang ditolak server tidak menghapus
isian perawat.

---

## 2. Alur proses yang diuji

1. Perawat IGD masuk aplikasi dan membuka ruang kerja pemeriksaan pasien uji. Tab **Assesmen Awal
   IGD** terbuka sebagai bawaan.
2. Perawat membuka segmen **Riwayat Dokumen Pengkajian** untuk melihat keadaan awal, lalu kembali ke
   **Formulir Pengkajian Aktif**.
3. Perawat mengisi tanda vital (bila kosong) dan keluhan utama, lalu menekan **Simpan Pengkajian**.
4. Aplikasi menyimpan data kunjungan (`PUT /emergency-visits/{id}`), lalu menyimpan pengkajian
   (`POST /patient-assessments`).
5. Bila server menerima (`201`), aplikasi menyegarkan data kunjungan, **memuat ulang daftar
   pengkajian** (`GET /patient-assessments?patientId=…&encounterId=…`), dan memindahkan layar ke
   segmen Riwayat.
6. Perawat menekan **Pengkajian baru** untuk kembali ke formulir; baris **Terakhir dikaji**
   menampilkan pengkajian terbaru.

---

## 3. Endpoint yang terlibat

**[Tags("Health Services / Clinical Management / Patient Assessment")]**

| Method | Path | Deskripsi | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/patient-assessments` | Menyimpan pengkajian awal baru | Bearer; izin `PatientAssessment` tulis | Isian formulir + `encounterId`, `queueId: null`, `completeImmediately` | `201` + nomor dokumen (contoh `ASM-20261104-00003`) |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments` | Daftar pengkajian pasien | Bearer; izin `PatientAssessment` `Read` | Query `patientId`, `encounterId`, `sortDirection=desc`, `pageNumber=1`, `pageSize=50` | `200` + daftar berhalaman |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments/{id}` | Detail satu pengkajian (status lokalis, dll.) | Bearer; `Read` | — | `200` |

**[Tags("Health Services / Emergency Installation Management / Emergency Visit")]**

| Method | Path | Deskripsi | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/api/v1/health-services/emergency-installation-management/emergency-visits/{id}` | Menyimpan data kedatangan & kunjungan (ikut terkirim saat Simpan Pengkajian) | Bearer | Data kunjungan | `200` |
| `GET` | `/api/v1/health-services/emergency-installation-management/emergency-visits/{id}` | Data kunjungan untuk kartu pasien (`onVisitUpdated`) | Bearer | — | `200` |

---

## 4. Hasil per skenario

| ID | Prioritas | Skenario | Hasil yang diharapkan | Hasil putaran 2 | Status | Bukti |
| --- | :---: | --- | --- | --- | :---: | --- |
| S1 | P0 | Simpan pengkajian → Riwayat langsung tampil | Pindah ke Riwayat, kartu baru tampil, badge bertambah satu, tanpa pindah tab/F5 | `POST` `201`; kartu dengan keluhan "Uji ISSUE-IGD-003 simpan-1 …" tampil; kartu 1 → 2, badge 2 | LULUS | `02-riwayat-setelah-simpan-1.png` |
| S2 | P0 | Daftar dimuat ulang dari server | Setelah `POST 201` ada `GET /patient-assessments` dengan `patientId` dan `encounterId` | 1 `GET` `200` sesudah `POST`, keduanya ada | LULUS | `jaringan-*.json` |
| S3 | P1 | Baris "Terakhir dikaji" (`IGD-DEC-133`) | Setelah *Pengkajian baru*, baris menampilkan pengkajian yang baru disimpan | Tampil: waktu 4 Nov 2026 11.49, pencatat perawat penguji, keluhan simpan-1 | LULUS | `03-terakhir-dikaji.png` |
| S4 | P1 | Simpan kedua, terbaru di atas | Kartu bertambah satu lagi, kartu teratas = simpan kedua | `POST` `201`; kartu 3, badge 3, teratas "simpan-2" | LULUS | `05-riwayat-setelah-simpan-2.png` |
| S5 | P4 | Teks keterangan Riwayat | Persis "Daftar diperbarui otomatis setiap kali pengkajian berhasil disimpan.", tanpa "backend" | Sesuai | LULUS | `01-riwayat-awal.png` |
| S6 | P2 | Simpan ditolak server | Tetap di Formulir, isian tidak hilang, tidak ada `GET` daftar | Segmen `form`, isian utuh, pesan penolakan tampil, 0 `GET` daftar | LULUS | `04-simpan-ditolak.png` |
| S7 | P1 | Status kunjungan setelah pengkajian | Kunjungan berstatus `InTreatment` (4, "Sedang ditangani") | Awal 4, akhir 4 | LULUS (terbatas) | DB baca-saja |

**Catatan per skenario.**

- **S1 putaran 1** dimulai dari pasien yang benar-benar belum punya pengkajian (0 kartu, tanpa
  badge). Hasilnya 1 kartu, badge "1" — sama persis dengan keadaan pelapor, kini sudah benar.
- **S6** memakai penolakan **simulasi**: Playwright mencegat `POST /patient-assessments` dan
  menjawab `400` tanpa meneruskannya ke server. Cara ini dipilih supaya tidak perlu membuat data
  rusak di server. Akibatnya, pesan galat yang diuji adalah pesan simulasi, bukan pesan validasi
  server yang sebenarnya.
- **S7 terbatas**: pasien uji sudah berstatus `InTreatment` sebelum uji, jadi transisi
  `Triaged → InTreatment` (`IGD-DEC-311`) tidak teramati di sini. Perbaikan ini tidak menyentuh blok
  transisi tersebut; yang dibuktikan hanya bahwa status tetap benar.
- **Putaran 1, S4 GAGAL karena skrip uji.** `page.unroute` dipanggil dengan fungsi pencocok baru,
  sehingga simulasi `400` dari S6 masih aktif saat simpan kedua. Bukti: log jaringan mencatat
  `POST 400` kedua, dan database hanya bertambah satu dokumen pada putaran itu. Skrip diperbaiki
  (pencocok disimpan dalam variabel), lalu seluruh skenario diulang pada putaran 2.

### 4.1 Bukti bahwa muat ulang berasal dari perbaikan

Hook halaman juga bisa memuat daftar pengkajian, tetapi selalu bersamaan dengan daftar tanda vital
dan triase. Garis waktu permintaan putaran 2 (detik sejak halaman dibuka):

| Detik | Permintaan | Asal |
| ---: | --- | --- |
| 0,33–0,34 | `GET /patient-assessments` + `GET /patient-vital-signs` + `GET /emergency-triages` | Hook halaman saat tab dibuka |
| 9,04 | `POST /patient-assessments` → `201` | Simpan pertama |
| 9,36 | `GET /emergency-visits/{id}` + `GET /patient-assessments` | `onVisitUpdated` + **`reload()`** — tanpa tanda vital/triase, jadi bukan dari hook |
| 16,26 | `POST /patient-assessments` → `400` (simulasi) | Simpan ditolak — **tidak** diikuti `GET` daftar |
| 21,39 | `POST /patient-assessments` → `201` | Simpan kedua |
| 21,70 | `GET /emergency-visits/{id}` + `GET /patient-assessments` | `onVisitUpdated` + **`reload()`** |

Pada putaran 1 tercatat **dua** `GET` daftar sesudah simpan pertama (selisih 20 ms). Log putaran 1
belum mencatat tanda vital/triase, jadi asal `GET` kedua tidak dapat dipastikan. Dugaannya: hook
halaman ikut memuat karena `PUT` kunjungan pertama mengubah keluhan utama kunjungan (bagian dari
`context`). Muatan itu berpacu dengan `POST` dan tidak dapat diandalkan — itu sebabnya pelapor
melihat daftar kosong sebelum perbaikan.

---

## 5. Temuan sampingan (di luar cakupan perbaikan)

| No | Temuan | Prioritas | Bukti | Saran |
| ---: | --- | :---: | --- | --- |
| T-01 | Kolom **STATUS** pada kartu riwayat menampilkan angka mentah `2`, bukan label seperti "Selesai" | P4 | `02-riwayat-setelah-simpan-1.png`; `emergency-assessment-initial-tab.jsx` memetakan `assessmentStatus` apa adanya ke `EmergencyAssessmentRecordTab` | Petakan nilai status ke label Bahasa Indonesia; layak dijadikan issue baru |
| T-02 | Saat *Simpan Pengkajian* ditolak, `PUT /emergency-visits/{id}` tetap sudah terkirim lebih dulu dan berhasil | P5 | Garis waktu detik 15,95 (`PUT`) sebelum `POST` simulasi `400` | Perilaku lama (data kunjungan disimpan terpisah); dicatat untuk dipertimbangkan, tidak berbahaya untuk uji ini |
| T-03 | Playwright proyek (1.57) mencari Chromium build 1200, sedangkan mesin ini punya build 1234 | — | Galat peluncuran pertama | Skrip memakai `executablePath` ke build 1234; tidak ada unduhan |

---

## 6. Data uji yang tercipta

Seluruh data dibuat lewat aplikasi dengan akun Perawat IGD — tidak ada `INSERT`/`UPDATE` langsung.
Query database hanya dipakai untuk membaca (sesi `default_transaction_read_only=on`).

| Nomor dokumen | Status | Putaran | Keluhan (penanda uji) |
| --- | :---: | :---: | --- |
| `ASM-20261104-00002` | 2 | 1 | "Uji ISSUE-IGD-003 simpan-1 11:46:23" |
| `ASM-20261104-00003` | 2 | 2 | "Uji ISSUE-IGD-003 simpan-1 11:48:57" |
| `ASM-20261104-00004` | 2 | 2 | "Uji ISSUE-IGD-003 simpan-2 11:48:57" |

Ketiganya melekat pada kunjungan uji `IGD-261023022832-A42BE4`. Data kunjungan pasien itu juga ikut
tersimpan ulang lewat `PUT` setiap kali *Simpan Pengkajian* ditekan (perilaku bawaan layar). Bila
ingin dibersihkan, batalkan lewat aksi Batal pengkajian di aplikasi.

Pengkajian asli pelapor (`ASM-20261104-00001`, kunjungan lain) tunggal — **tidak ada dokumen
ganda** (menjawab pertanyaan P-02 pada issue).

---

## 7. Kesimpulan

`FIX-IGD-003-01` dan `FIX-IGD-003-02` bekerja sesuai acceptance criteria 1–6 pada plan repair.
Kriteria 7 (`IGD-DEC-311`) hanya teruji terbatas karena pasien uji sudah berstatus `InTreatment`.
Laporan ini adalah bukti uji pengembang, **bukan** UAT; UAT tetap dikerjakan tim UAT.

Screenshot di bawah berasal dari putaran 2; putaran 2 menimpa berkas bernama sama dari putaran 1.
Bukti putaran 1 (mulai dari 0 kartu) ada pada `hasil-2026-11-04T04-46-23-749Z.json` dan
`jaringan-2026-11-04T04-46-23-749Z.json`.

| Berkas bukti | Isi |
| --- | --- |
| `screenshots/00-ruang-kerja-terbuka.png` | Ruang kerja terbuka, tab Assesmen Awal |
| `screenshots/01-riwayat-awal.png` | Riwayat sebelum simpan + teks keterangan baru |
| `screenshots/02-riwayat-setelah-simpan-1.png` | Riwayat langsung terisi setelah simpan pertama |
| `screenshots/03-terakhir-dikaji.png` | Baris "Terakhir dikaji" + badge |
| `screenshots/04-simpan-ditolak.png` | Tetap di formulir setelah penolakan |
| `screenshots/05-riwayat-setelah-simpan-2.png` | Dua pengkajian baru, terbaru di atas |
