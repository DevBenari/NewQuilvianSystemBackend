# Rawat Inap — Arsitektur Frontend

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | `0.5` |
| Status | `draft` |
| Sub-modul | `episode-rawat-inap` — satu dari tiga sub-modul modul `rawat-inap`, bentuk `COMPOSITE` sejak `RWI-DEC-082`. [Manifest sub-modul](./blueprint-manifest.md), [peta modul](../02-module-map.md) |
| Apa yang berubah pada `0.5` | **Hanya batas dokumen, bukan isi desain.** Peta butir menu seluruh modul naik ke [`../02-module-map.md`](../02-module-map.md) bagian 3, karena sidebar hanya satu untuk tiga sub-modul. Nol layar, endpoint, dan aturan keterjangkauan yang bergerak |
| Frontend SHA | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` |
| Backend SHA | `5afb54bd75281648010e50ef14f43ca1f80d8efd` |
| Masukan | `02-backend-architecture.md` revision `0.3`; `04-prd-to-mvp.md` revision `0.4.0` bagian 9; `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`, dan `contracts/validation-matrix.md` revision `0.4.0` |
| Dasar revision ini | `RWI-DEC-075` s.d. `RWI-DEC-079`, dijawab pemilik 2026-08-27 |
| Batas tulis | Hanya dokumen blueprint |

> **Batas kewenangan dokumen ini.** Dokumen ini menetapkan **kontrak fungsional**: layar apa yang
> dibutuhkan, siapa boleh melakukan apa, data dan status apa yang dikonsumsi, urutan langkah pada
> alur yang berlangkah, dan bagaimana keadaan gagal ditangani.
>
> Dokumen ini **tidak** menetapkan warna, tata letak, pustaka komponen, nama menu, maupun nama
> route. Seluruhnya adalah wewenang pelaksana frontend selama tidak melanggar keamanan, privasi,
> invariant, atau **keterjangkauan** — lihat bagian 2B.

Urutan wewenang yang dipakai:

```text
keamanan / privasi / invariant / keterjangkauan
  -> brief produk atau UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

---

## 0. Kenapa revision `0.4` ada

Revision `0.3` menghasilkan 18 task frontend yang seluruhnya selesai, tetapi hasilnya **tidak dapat
menjalankan alur bisnis yang PRD-nya sendiri tetapkan**. Sebabnya bukan mutu pelaksanaan, melainkan
tiga cacat pada dokumen ini:

| Cacat revision `0.3` | Akibat nyata di layar | Ditutup oleh |
| --- | --- | --- |
| Daftar layar tidak pernah diadu dengan `FLOW-RI-MVP-001` | Dua langkah alur yang PRD tandai **Wajib** tidak punya layar sama sekali: memilih penjamin dan memesan tempat tidur | bagian **2A** |
| Tidak ada model keterjangkauan; route dan menu diserahkan penuh ke `DEV_DISCRETION` | Beranda modul hanya berisi kalimat penantian; episode `Draft` dan `Closed` tidak dapat ditemukan dari layar mana pun; layar sesi koreksi jadi tetapi praktis mati | bagian **2B** |
| Aksi disebut pada matriks peran tetapi tidak dimiliki layar mana pun | "Memesan tempat tidur" dan "Membatalkan admisi" tidak pernah menjadi task | bagian **2A** dan **11A** |

Akibat gabungannya: **sembilan** operasi HTTP yang sudah jadi di backend tidak pernah dipanggil satu
pun layar. Daftarnya ada pada bagian 11A.

Revision `0.4` juga menyerap keputusan bentuk admisi. Admisi berhenti menjadi satu formulir dan
menjadi **alur berlangkah dua jalur** — pasien baru dan pasien lama — sesuai `RWI-DEC-075`.

### 0.1 Pertentangan di dalam revision `0.3` yang dibetulkan di sini

| Tempat | Isi lama | Betulnya |
| --- | --- | --- |
| bagian 2, `FE-INP-09` | "Tiga daftar pantau pada MVP" | **Empat** — konsisten dengan bagian 4.4 |
| bagian 9 | "Empat belas layar pada bagian 2" | Bagian 2 revision `0.3` memuat lima belas; revision ini memuat **sembilan belas** |
| bagian 2, `FE-INP-03` | "Membuka admisi, memilih penjamin, …" padahal `OpenAdmissionRequest` tidak punya kolom penjamin | Penjamin dipilih pada langkah **Pembayaran** dan ditulis lewat `POST /patient-encounters` milik Registrasi — lihat 3A |

---

## 1. Keadaan frontend saat ini

Delapan belas task revision `0.3` sudah dikerjakan, sehingga keadaan awal modul ini **bukan lagi nol**.

| Hal | Keadaannya |
| --- | --- |
| Route Rawat Inap | **13 route ada** di `src/app/health-services/inpatient-management/` |
| Menu Rawat Inap | **Ada**, delapan butir tingkat dua di `src/utils/menu-sidebar/menu-items.jsx:902` |
| Beranda modul | **Ada tetapi kosong** — hanya `Hero` dan satu kalimat penantian |
| Daftar kerja episode | **Tidak ada.** Census hanya memuat pasien yang sedang dirawat |
| Admisi | Satu formulir tunggal. Tanpa penjamin, tanpa pendaftaran pasien, tanpa pemesanan tempat tidur |
| Pemesanan tempat tidur | **Tidak ada layarnya.** Endpoint-nya ada dan menganggur |
| Pembatalan admisi | **Tidak ada layarnya.** Endpoint-nya ada dan menganggur |
| Cetak persetujuan dan kartu pasien | **Tidak ada** pada modul ini |
| Layar master tempat tidur | Tombol nonaktifkan sudah diperbaiki lewat `FE-RWI-001` |

Pola yang sudah terbukti berjalan di repository dan **wajib** dipakai ulang ada pada bagian 8.

---

## 2. Kebutuhan layar

Nama layar di bawah adalah **nama fungsional**, bukan nama menu. Pelaksana bebas menamai ulang dan
menggabungkan selama seluruh kemampuannya tercapai **dan** aturan keterjangkauan bagian 2B dipenuhi.

| ID | Layar | Tujuan | Pemakai utama | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-INP-01` | Daftar pasien dirawat (census) | Melihat siapa dirawat, di mana, oleh siapa, dan sudah berapa hari | Perawat, kepala ruangan, admisi, DPJP | ada |
| `FE-INP-02` | Papan ketersediaan tempat tidur | Melihat tempat tidur kosong, dipesan, terisi, ditutup — **dan mengonfirmasi pasien masuk** | Admisi, kepala ruangan, supervisor | ada, **bertambah aksi** |
| `FE-INP-03` | **Admisi pasien** | Alur berlangkah dua jalur: mendaftarkan atau menemukan pasien, menetapkan penjamin dan kelas, menetapkan unit dan DPJP, mencari lalu memesan tempat tidur, mencetak persetujuan dan kartu | Petugas admisi | **diganti total** |
| `FE-INP-04` | Detail episode | Melihat satu episode utuh: status, lokasi terkini, DPJP, perawat, riwayat | Semua peran klinis dan admisi | ada |
| `FE-INP-05` | Perpindahan pasien | Memindahkan pasien ke tempat tidur lain beserta alasannya | Kepala ruangan, perawat, supervisor, DPJP | ada |
| `FE-INP-06` | Keputusan pulang dan resume | DPJP menyatakan pasien boleh pulang lalu menyusun dan menandatangani resume | DPJP | ada |
| `FE-INP-07` | Penutupan episode | Menandai butir administrasi, melihat kelima syarat, lalu menutup episode | Petugas admisi, supervisor | ada |
| `FE-INP-08` | Penandaan kelayakan keuangan | Kasir menandai `Cleared` atau `Blocked` beserta catatannya | Kasir, billing | ada |
| `FE-INP-09` | Daftar pantau | **Empat** daftar pantau yang tersedia pada MVP | Admisi, kepala ruangan, supervisor | ada |
| `FE-INP-10` | Laporan selisih tempat tidur | Menemukan tempat tidur yang statusnya tidak cocok dengan penghuninya | Admin, supervisor | ada |
| `FE-INP-11` | Sesi koreksi episode | Supervisor membuka, mengoreksi, lalu menutup sesi | Supervisor | ada, **belum terjangkau** |
| `FE-INP-12` | Pengaturan Rawat Inap | Mengubah batas waktu dan ambang | Admin master data | ada |
| `FE-INP-13` | Master butir administrasi | Menambah, mengubah, dan menonaktifkan butir daftar periksa | Admin master data | ada |
| `FE-INP-14` | Pencatatan kepergian pasien | Menandai bahwa pasien sudah meninggalkan ruangan | Admisi, perawat, kepala ruangan, supervisor | ada |
| `FE-INP-15` | Penetapan kebutuhan isolasi | Merekam atau mengubah kebutuhan isolasi episode | Admisi selagi `Draft`; DPJP aktif setelah episode aktif | ada |
| `FE-INP-16` | **Daftar kerja episode** | Menemukan episode apa pun menurut status, unit, kelas, tanggal, dan kata kunci — termasuk `Draft` yang tertinggal dan `Closed` yang perlu dikoreksi | Semua peran ber-`InpatientEpisode : Read` | **baru** |
| `FE-INP-17` | **Pembatalan admisi** | Membatalkan admisi beserta pemesanan dan penempatannya dalam satu tindakan | Petugas admisi (`Draft`), kepala ruangan (`Admitted`), supervisor | **baru** |
| `FE-INP-18` | **Cetak persetujuan rawat inap** | Mencetak formulir persetujuan umum berisi data pasien, penjamin, dan episode | Petugas admisi | **baru** |
| `FE-INP-19` | **Beranda Rawat Inap** | Ringkasan operasional dan pintu masuk pekerjaan hari ini | Semua peran | **baru** |

Sembilan belas layar. `FE-INP-15` dan `FE-INP-17` **tidak wajib** berupa halaman tersendiri, tetapi
kedua jalur masuknya wajib ada — lihat 2B.

---

## 2A. Peta alur, layar, aksi, dan endpoint

**Bagian ini mengikat.** Setiap langkah `FLOW-RI-MVP-001` pada `04-prd-to-mvp.md` bagian 9 wajib
menunjuk tepat satu layar yang memilikinya. Langkah tanpa pemilik adalah **cacat blueprint**, bukan
`DEV_DISCRETION`, dan tidak boleh dibiarkan lewat seperti pada revision `0.3`.

| Langkah `FLOW-RI-MVP-001` | Layar pemilik | Endpoint |
| --- | --- | --- |
| 1. Memilih pasien terdaftar, atau mendaftarkannya | `FE-INP-03` langkah 1–2 | `GET /patients/options`, `POST /patients`, `POST /patient-identity-documents`, `POST /patient-emergency-contacts` |
| 2. Sistem membuat kunjungan bertipe rawat inap | `FE-INP-03` **titik tulis 1** | `POST /patient-encounters` dengan `EncounterType=Inpatient`, `RegistrationSource=InpatientAdmission` |
| 3. Memilih penjamin, kelas, unit layanan, dan DPJP | `FE-INP-03` langkah 3–4 | `POST /patient-insurances` atau `/patient-company-guarantors` bila perlu, lalu `POST /patient-encounters`, lalu `POST /episodes` |
| 4. Catatan awal kebutuhan isolasi selagi `Draft` | `FE-INP-03` langkah 4 | `PATCH /episodes/{id}/isolation-requirement` |
| 5. Mencari lalu **memesan** tempat tidur, `Reserved` selama `BedReservationMinutes` | `FE-INP-03` langkah 5–6 | `GET /bed-occupancies/available-beds`, `POST /bed-occupancies/reservations` |
| 6. Pasien sampai di kamar; **konfirmasi masuk**; kelayakan diperiksa ulang | **`FE-INP-02`** | `POST /bed-occupancies/placements` |
| 7. Menugaskan perawat penanggung jawab | `FE-INP-04` | `POST /episodes/{id}/nurse-assignments` |
| 8. DPJP mengubah kebutuhan isolasi di tengah perawatan | `FE-INP-15` di dalam `FE-INP-04` | `PATCH /episodes/{id}/isolation-requirement` |
| 9. Memindahkan pasien | `FE-INP-05` | `POST /bed-occupancies/placements/transfer` |
| 10. Mengalihkan DPJP | `FE-INP-04` | `POST /episodes/{id}/doctor-assignments` |
| 11. Menyatakan pasien boleh pulang | `FE-INP-06` | `POST /discharges/{episodeId}/decide` |
| 12. Menyusun dan menandatangani resume | `FE-INP-06` | `PUT /discharges/{episodeId}/summary`, `PATCH …/summary/sign` |
| 13. Menandai butir administrasi | `FE-INP-07` | `POST /discharges/{episodeId}/clearance/{itemId}/mark` |
| 14. Menandai kelayakan keuangan | `FE-INP-08` | `POST /discharges/{episodeId}/financial-clearance` |
| 15. Mencatat kepergian pasien | `FE-INP-14` | `POST /discharges/{episodeId}/record-departure` |
| 16. Menutup episode | `FE-INP-07` | `POST /discharges/{episodeId}/close` |
| 17. Menutup menembus gerbang keuangan | `FE-INP-07` | `POST /discharges/{episodeId}/close-with-override` |
| 18. Sesi koreksi | `FE-INP-11` | `POST /episodes/{id}/correction-sessions`, `PATCH …/close` |

### Aksi di luar alur utama, yang tetap wajib punya pemilik

| Aksi | Layar pemilik | Endpoint |
| --- | --- | --- |
| Menemukan episode apa pun menurut status | `FE-INP-16` | `GET /episodes`, `GET /episodes/filters/metadata` |
| Melanjutkan admisi `Draft` yang tertinggal | `FE-INP-16` → `FE-INP-03` | `GET /episodes/{id}`, `PUT /episodes/{id}` |
| Membatalkan admisi | `FE-INP-17` | `PATCH /episodes/{id}/cancel` |
| Membatalkan pemesanan sebelum dipakai | `FE-INP-03` langkah 6 dan `FE-INP-02` | `PATCH /bed-occupancies/reservations/{id}/cancel` |
| Ringkasan operasional | `FE-INP-19` | `GET /episodes/summary`, `GET /census/summary` |
| Mencetak persetujuan rawat inap | `FE-INP-18` | tidak ada — cetak dari data yang sudah dibaca |
| Mencetak kartu pasien | `FE-INP-03` langkah 9 | dipakai ulang dari cetak kartu kiosk |

---

## 2B. Arsitektur informasi dan keterjangkauan

> **Pindah tempat 2026-09-02 — `RWI-DEC-082`.** Sidebar hanya satu untuk seluruh modul, sedangkan
> modul ini kini punya tiga sub-modul. Peta butir menu **seluruh modul** karena itu naik ke
> [`../02-module-map.md`](../02-module-map.md) bagian 3, supaya tiga sub-modul tidak merancang
> sidebar sendiri-sendiri.
>
> Yang tinggal di bawah ini adalah **aturan keterjangkauan** dan **peta route** milik sub-modul
> `episode-rawat-inap`. Kesembilan butir menu tingkat dua yang dihitung `IA-INP-05` seluruhnya
> milik sub-modul ini — **kuotanya sudah penuh**. Karena itu `keperawatan` dan `dokter-rawat-inap`
> tidak dapat menambah butir menu tingkat dua tanpa lebih dulu mengubah `IA-INP-05`, dan hasil
> penetapannya ditulis kembali ke `02-module-map.md`, bukan ke berkas ini.

**Nama route, nama menu, dan urutan menu tetap `DEV_DISCRETION`. Keterjangkauan tidak.**

### Aturan yang mengikat

| Aturan | Isinya |
| --- | --- |
| `IA-INP-01` | Setiap layar pada bagian 2 wajib dapat dicapai dari Beranda `FE-INP-19` dalam **paling banyak tiga klik** |
| `IA-INP-02` | Setiap layar per-episode wajib punya **paling sedikit satu** jalur masuk dari `FE-INP-16` |
| `IA-INP-03` | `FE-INP-16` wajib dapat menampilkan **kelima** nilai status episode, termasuk `Draft`, `Cancelled`, dan `Closed`. Census **tidak boleh** dipakai sebagai penggantinya, karena census berarti "sedang dirawat" dan mencampurnya mengaburkan arti itu |
| `IA-INP-04` | Layar yang tidak terjangkau dari mana pun dihitung **belum selesai**, walaupun kodenya ada dan test-nya lulus |
| `IA-INP-05` | Menu tingkat dua Rawat Inap dibatasi **paling banyak sembilan** butir. Layar per-episode tidak mendapat butir menu; ia dicapai lewat `FE-INP-16` atau `FE-INP-01` |

### Peta route usulan

Mengikat pada kolom kanan, bebas pada kolom kiri.

| Route usulan | Yang wajib terjangkau dari sana |
| --- | --- |
| `/health-services/inpatient-management` | `FE-INP-19` — ringkasan, dan tautan ke seluruh layar tingkat dua |
| `…/admissions` | `FE-INP-03` seluruh langkah, termasuk melanjutkan `Draft` lewat parameter episode |
| `…/episodes` | `FE-INP-16` |
| `…/episodes/{id}` | `FE-INP-04`, dan dari sana `FE-INP-05`, `FE-INP-14`, `FE-INP-15`, `FE-INP-17` |
| `…/episodes/{id}/discharge` | `FE-INP-06` |
| `…/episodes/{id}/financial-clearance` | `FE-INP-08` |
| `…/episodes/{id}/closure` | `FE-INP-07` |
| `…/episodes/{id}/correction` | `FE-INP-11` |
| `…/episodes/{id}/consent-print` | `FE-INP-18` |
| `…/bed-board` | `FE-INP-02` termasuk konfirmasi masuk |
| `…/census` | `FE-INP-01` |
| `…/monitoring`, `…/bed-drift` | `FE-INP-09`, `FE-INP-10` |
| `…/settings`, `…/clearance-items` | `FE-INP-12`, `FE-INP-13` |

### Isi Beranda `FE-INP-19`

Yang **wajib** terbaca, bentuknya bebas:

1. Jumlah pasien dirawat per unit layanan dan per kelas — dari `GET /census/summary`.
2. Jumlah episode per status — dari `GET /episodes/summary`. Angka `Draft` adalah **penanda kerja
   tertinggal** dan wajib dapat diklik menuju `FE-INP-16` yang sudah tersaring `Draft`.
3. Jumlah baris pada keempat daftar pantau, masing-masing dapat diklik.

Beranda **tidak boleh** menjadi halaman yang hanya berisi judul dan kalimat penantian.

---

## 2C. Kontrak komposisi layar

| Bentuk | Dipakai untuk | Aturannya |
| --- | --- | --- |
| **Daftar** | `FE-INP-01`, `FE-INP-09`, `FE-INP-10`, `FE-INP-13`, `FE-INP-16` | Wajib memenuhi keempat keadaan bagian 5.1 |
| **Papan** | `FE-INP-02` | Wajib memuat ulang saat difokuskan kembali dan sebelum dialog konfirmasi |
| **Alur berlangkah** | `FE-INP-03` | Kontraknya ada pada bagian 3A |
| **Ruang kerja per episode** | `FE-INP-04` | Menjadi induk `FE-INP-05`, `FE-INP-14`, `FE-INP-15`, `FE-INP-17` |
| **Layar anak per episode** | `FE-INP-06`, `FE-INP-07`, `FE-INP-08`, `FE-INP-11`, `FE-INP-18` | Wajib membaca ulang detail episode saat dibuka |
| **Cetak** | `FE-INP-18` dan kartu pasien | Halaman cetak sendiri, tanpa navigasi aplikasi |

Aturan tambahan: **satu kemampuan, satu tempat.** Bila sebuah aksi sudah dimiliki satu layar, layar
lain hanya boleh menautkannya, tidak boleh menyediakan jalur keduanya. Pelajaran ini datang dari
koreksi resume yang sengaja hanya ada di `FE-INP-11`.

---

## 3. Aksi per peran

Kolom ini menurunkan langsung dari `contracts/permission-audit-matrix.md` revision `0.4.0`. Tombol
yang tidak diizinkan **harus disembunyikan atau dinonaktifkan**, bukan ditampilkan lalu ditolak
server.

| Aksi di layar | Petugas admisi | Perawat | Kepala ruangan | DPJP | Kasir | Supervisor | Admin |
| --- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| Melihat census dan daftar kerja | Ya | Ya | Ya | Ya | Ya | Ya | – |
| Mencari tempat tidur kosong | Ya | Ya | Ya | – | – | Ya | – |
| Mendaftarkan pasien baru | Ya | – | – | – | – | Ya | – |
| Membuka admisi | Ya | – | – | – | – | Ya | – |
| **Memesan tempat tidur** | Ya | – | – | – | – | Ya | – |
| **Membatalkan pemesanan** | Ya | – | – | – | – | Ya | – |
| **Mengonfirmasi pasien masuk (penempatan)** | Ya | – | – | – | – | Ya | – |
| Membatalkan admisi `Draft` | Ya | – | – | – | – | Ya | – |
| Membatalkan admisi `Admitted` | – | – | Ya | – | – | Ya | – |
| Memindahkan pasien | – | Ya | Ya | Ya, bila DPJP aktif | – | Ya | – |
| Mengalihkan DPJP | – | – | Ya | – | – | Ya | – |
| Menugaskan perawat | – | – | Ya | – | – | Ya | – |
| Menyatakan pasien boleh pulang | – | – | – | Ya, bila DPJP aktif | – | – | – |
| Menyusun dan menandatangani resume | – | – | – | Ya, bila DPJP aktif | – | – | – |
| Menandai butir administrasi | Ya | – | – | – | – | Ya | – |
| Menandai kelayakan keuangan | – | – | – | – | Ya | – | – |
| Menutup episode | Ya | – | – | – | – | Ya | – |
| Mencatat pasien sudah meninggalkan ruangan | Ya | Ya | Ya | – | – | Ya | – |
| Menutup menembus gerbang keuangan | – | – | – | – | – | Ya | – |
| Membuka sesi koreksi | – | – | – | – | – | Ya | – |
| Menetapkan kebutuhan isolasi | Ya, hanya selagi `Draft` | – | – | Ya, bila DPJP aktif | – | – | – |
| **Mencetak persetujuan rawat inap** | Ya | – | Ya | – | – | Ya | – |
| Mengubah pengaturan dan butir | – | – | – | – | – | – | Ya |

### 3.1 Tiga tombol yang paling perlu diperhatikan

| Tombol | Aturan tampilnya |
| --- | --- |
| Pindahkan pasien, untuk pengguna berperan dokter | Hanya aktif bila dokter itu **DPJP aktif episode tersebut**. Bila bukan, tombol dinonaktifkan disertai keterangan "Anda bukan DPJP episode ini" |
| Tutup menembus gerbang keuangan | **Tidak boleh** ditampilkan berdampingan dengan tombol tutup biasa seolah dua pilihan setara. Ia baru muncul setelah tombol tutup biasa ditolak karena kelayakan keuangan, dan hanya untuk supervisor |
| Ubah kebutuhan isolasi | Kewenangannya **berpindah** mengikuti status episode. Selagi `Draft`, aktif bagi petugas admisi dan bagi DPJP. Begitu `Admitted`, tombol itu **nonaktif** bagi petugas admisi, disertai keterangan "Setelah pasien dirawat, kebutuhan isolasi hanya dapat diubah DPJP". Bagi dokter yang bukan DPJP aktif, keterangannya "Anda bukan DPJP episode ini" |

### 3.2 Konfirmasi masuk — satu butir hak akses yang perlu diputuskan pemilik

`RWI-DEC-076` memisahkan penempatan dari alur admisi: alur admisi berhenti pada tempat tidur
`Reserved`, dan pasien baru menjadi `Admitted` ketika kedatangannya dikonfirmasi.

Kalimat usulan yang disetujui menyebut **perawat ruangan** sebagai pelakunya. Kontrak hak akses
revision `0.4.0` **tidak mengizinkannya**: `POST /bed-occupancies/placements` menuntut
`InpatientBedOccupancy : Create`, dan butir itu hanya dimiliki **petugas admisi** dan **supervisor**.
Perawat pelaksana serta kepala ruangan hanya memiliki `Read` dan `Transfer`.

| Yang diberlakukan revision ini | Alasannya |
| --- | --- |
| Konfirmasi masuk dijalankan **petugas admisi** dan **supervisor** dari `FE-INP-02` | Dokumen frontend tidak berwenang melonggarkan kontrak hak akses yang sudah terkunci. Melonggarkannya diam-diam akan menghasilkan tombol yang pasti ditolak server — cacat yang sama seperti tombol isolasi pada revision `0.3` |

**Butir terbuka `RWI-OQ-045`:** apakah `InpatientBedOccupancy : Create` perlu diberikan kepada
kepala ruangan supaya konfirmasi masuk dapat dilakukan dari ruangan. Kalau ya, perubahannya ada di
`contracts/permission-audit-matrix.md` dan seeder hak akses backend — **di luar** dokumen ini.
Owner: Product/Domain bersama Backend/API.

---

## 3A. Kontrak alur admisi berlangkah — `FE-INP-03`

Dasar: `RWI-DEC-075` sampai `RWI-DEC-079`.

### 3A.1 Dua jalur masuk

Layar dibuka dengan pilihan tipe pendaftaran, persis pola yang sudah berjalan pada
`patient-entry-choice-step.jsx` milik pendaftaran IGD:

| Jalur | Kapan dipakai |
| --- | --- |
| **Pendaftaran pasien baru** | Pasien belum pernah terdaftar di rumah sakit |
| **Pendaftaran pasien lama** | Pasien sudah punya nomor rekam medis |

Keduanya bermuara pada langkah yang sama sejak langkah Pembayaran.

### 3A.2 Langkah jalur pasien baru

| # | Langkah | Isi yang wajib ada | Titik tulis | Endpoint |
| --- | --- | --- | :---: | --- |
| 1 | Tipe Pasien | Pilihan jalur; jenis pasien (umum, ibu, bayi baru lahir, anak, pegawai, korporat). Bila **bayi baru lahir**, episode ibu dipilih di sini dan mengisi `MotherEpisodeId` | – | – |
| 2 | Pendaftaran | Scan KTP bila tersedia, lalu formulir pasien baru, dokumen identitas, dan kontak darurat | tulis pasien | `POST /patients`, `POST /patient-identity-documents`, `POST /patient-emergency-contacts` |
| 3 | Pembayaran | Cara bayar: tunai, asuransi, atau penjamin perusahaan. Bila asuransi atau perusahaan, kartunya dipilih atau didaftarkan. **Kelas perawatan dipilih di sini**, karena hak kelas mengikuti penjaminnya | tulis penjamin bila baru | `POST /patient-insurances` atau `POST /patient-company-guarantors` |
| 4 | Dokter | **Unit layanan rawat inap tujuan**, **DPJP**, catatan admisi, dan **kebutuhan isolasi beserta keterangannya** | **titik tulis 1** | `POST /patient-encounters` → `POST /episodes` → `PATCH /episodes/{id}/isolation-requirement` bila isolasi menyala |
| 5 | Pilih Bed | Hasil `available-beds` yang **sudah tersaring server**. Tempat tidur yang tersaring keluar boleh tampil sebagai baris nonaktif beserta alasannya | – | `GET /bed-occupancies/available-beds` |
| 6 | Booking Bed | Memesan tempat tidur terpilih. Sisa waktu pemesanan wajib terbaca. Membatalkan pemesanan dan memilih ulang wajib mungkin | **titik tulis 2** | `POST /bed-occupancies/reservations`, `PATCH …/reservations/{id}/cancel` |
| 7 | Konfirmasi | Ringkasan seluruh isian. Menyimpan perubahan isian admisi bila ada | **titik tulis 3** | `PUT /episodes/{id}` bila ada yang berubah |
| 8 | Cetak Persetujuan Pasien Ranap | Formulir persetujuan umum berisi data pasien, penjamin, unit, kelas, DPJP, dan nomor episode | – | – |
| 9 | Kartu Pasien | Cetak kartu pasien | – | dipakai ulang dari cetak kartu kiosk |

### 3A.3 Langkah jalur pasien lama

Sama persis sejak langkah Pembayaran. Yang berbeda hanya di depan dan di belakang:

| # | Langkah | Isi |
| --- | --- | --- |
| 1 | Pasien Lama | Pencarian dengan nomor rekam medis atau NIK, atau scan kartu pasien |
| 2 | Informasi Pasien Lama | Peninjauan data pasien yang ditemukan sebelum dilanjutkan |
| 3 | Tipe Pasien | Sama seperti langkah 1 jalur pasien baru |
| 4–8 | Pembayaran, Dokter, Pilih Bed, Booking Bed, Konfirmasi, Cetak Persetujuan | Sama persis |

Langkah **Kartu Pasien tidak ada** pada jalur ini; pasien lama sudah memilikinya. Bila kartunya
hilang, cetak ulang dilakukan lewat layar cetak kartu yang sudah ada, bukan dari alur admisi.

### 3A.4 Tiga titik tulis, dan kenapa tidak ditahan sampai akhir

`RWI-DEC-076` menetapkan tulisan terjadi **bertahap**, bukan ditahan sampai Konfirmasi.

| Titik tulis | Terjadi di | Yang tersimpan |
| :---: | --- | --- |
| pasien | akhir langkah Pendaftaran | `MstPatient` beserta dokumen identitas dan kontak darurat |
| **1** | akhir langkah Dokter | Kunjungan rawat inap **beserta baris penjaminnya**, lalu episode berstatus `Draft` |
| **2** | langkah Booking Bed | `InpBedReservation` aktif; tempat tidur terbaca `Reserved` |
| **3** | langkah Konfirmasi | Perubahan isian admisi, bila ada |

**Kenapa bertahap.** Menahan semuanya sampai Konfirmasi membuat `Reserved` kehilangan gunanya.
`RWI-CAP-006` ditandai wajib justru dengan alasan "tanpa ini dua petugas merebut tempat tidur yang
sama". Pemesanan yang baru terjadi di detik terakhir tidak menahan apa pun.

**Akibat yang wajib ditangani.** Karena tulisan terjadi bertahap, alur yang ditinggal di tengah
meninggalkan jejak di server. Penanganannya ada pada 3A.6, dan `FE-INP-16` karena itu **wajib**,
bukan tambahan.

### 3A.5 Aturan mundur

| Mundur dari | Ke | Yang boleh berubah |
| --- | --- | --- |
| Booking Bed | Pilih Bed | Bebas, selama pemesanan yang sudah terbentuk **dibatalkan lebih dulu**. Tidak boleh ada dua pemesanan aktif untuk satu episode |
| Pilih Bed | Dokter | Unit layanan, kelas, dan catatan boleh berubah lewat `PUT /episodes/{id}`. **DPJP tidak** — pengalihan DPJP adalah `POST /episodes/{id}/doctor-assignments` dan bukan wewenang alur admisi. Bila DPJP salah, admisi dibatalkan lalu dibuka ulang |
| Dokter | Pembayaran | **Tidak boleh** setelah titik tulis 1. Kunjungan sudah terbentuk beserta penjaminnya. Bila penjamin salah, admisi dibatalkan lewat `FE-INP-17` lalu dibuka ulang. Layar wajib mengatakan ini **sebelum** langkah Dokter disimpan, bukan sesudah |
| Pembayaran | Pendaftaran | Bebas selama titik tulis 1 belum lewat |

### 3A.6 Alur yang ditinggal

| Ditinggal setelah | Yang tertinggal di server | Cara menemukannya kembali |
| --- | --- | --- |
| Langkah Pendaftaran | Pasien terdaftar tanpa kunjungan | Jalur pasien lama |
| Titik tulis 1 | Episode `Draft` tanpa tempat tidur | `FE-INP-16` tersaring `Draft` → melanjutkan ke `FE-INP-03` langkah Pilih Bed |
| Titik tulis 2 | Episode `Draft` dengan tempat tidur `Reserved` | Sama; sisa waktu pemesanan wajib terbaca |
| Pemesanan kedaluwarsa | Episode `Draft` tanpa tempat tidur | Sama. `FE-INP-16` wajib membedakan `Draft` yang masih memegang pemesanan dari yang pemesanannya sudah gugur, karena keduanya menuntut tindakan berbeda |

Server menggugurkan pemesanan yang lewat waktu secara lazim pada setiap pemanggilan tempat tidur
berikutnya. Layar **tidak boleh** menghitung sendiri kapan sebuah pemesanan gugur; ia menampilkan
apa yang dijawab server.

### 3A.7 Batas alur ini

| Yang **tidak** dilakukan alur admisi | Alasannya |
| --- | --- |
| Menempatkan pasien menjadi `Admitted` | `RWI-DEC-076`. Kelayakan Penempatan diperiksa **ulang** saat pasien benar-benar tiba; memeriksanya di meja admisi meloloskan tempat tidur yang keburu tidak layak |
| Mengalihkan DPJP | Ada pada `FE-INP-04` |
| Menyimpan persetujuan umum | `RWI-DEC-077`. Formulir dicetak, tanda tangan tetap di atas kertas |
| Membuat antrean | Rawat inap tidak berantrean. Unit layanan rawat inap wajib disetel `IsQueueRequired = false` pada master data — **prasyarat data, bukan pekerjaan frontend** |

### 3A.8 Persetujuan rawat inap — `FE-INP-18`

`RWI-DEC-077` memilih **cetak tanpa menyimpan**.

| Aspek | Ketetapannya |
| --- | --- |
| Yang **wajib** | Formulir memuat identitas pasien, penjamin, unit layanan, kelas, DPJP, nomor episode, dan tanggal. Ketiga isi minimal `RWI-DEC-035` tercetak: persetujuan tindakan kedokteran umum, persetujuan pemberian informasi kepada penjamin, dan penunjukan penerima informasi |
| Yang **tidak boleh** | Menyatakan di layar bahwa persetujuan "sudah tersimpan" atau "sudah ditandatangani". Sistem tidak menyimpan apa pun |
| Hubungannya dengan penutupan | Butir daftar periksa administrasi tetap ditandai manual pada `FE-INP-07`, seperti sekarang |
| Yang tetap terbuka | `RWI-CAP-031` dan `DEC-INP-003` **tidak** tertutup oleh keputusan ini |

---

## 4. Data dan status yang dikonsumsi

### 4.0 Sumber data lintas modul

Setiap isian pilihan wajib punya satu sumber yang disebut namanya. Isian tanpa sumber yang tertulis
adalah cara paling mudah sebuah kemampuan hilang tanpa disadari.

| Isian | Pemilik data | Sumber |
| --- | --- | --- |
| Pasien | PatientManagement | `patients/options`, atau pendaftaran baru pada alur admisi |
| Penjamin dan cara bayar | PatientManagement dan RegistrationManagement | `patient-insurances`, `patient-company-guarantors`, `insurance-providers`, `company-guarantors` |
| Dokter (DPJP) | MasterData HealthServices | isian pilihan sumber daya yang sudah ada |
| Perawat | HR / MasterData | isian pilihan sumber daya yang sudah ada |
| Unit layanan | MasterData HealthServices | isian pilihan sumber daya; **wajib bertipe `Inpatient`** |
| Kelas perawatan | MasterData HealthServices | isian pilihan sumber daya |
| Kamar dan tempat tidur | MasterData HealthServices | **hanya** lewat `available-beds` dan `bed-board`, tidak pernah dari master langsung — lihat 4.3A |
| Butir administrasi | Rawat Inap | `inpatient-clearance-items` |
| Pengaturan | Rawat Inap | `inpatient-settings` |

### 4.1 Status episode dan cara menampilkannya

| Nilai dari backend | Kata yang diusulkan | Yang wajib terbaca pengguna |
| --- | --- | --- |
| `Draft` | Admisi sedang disiapkan | Pasien belum tentu ada di kamar. **Bila tempat tidur sudah dipesan, sisa waktu pemesanan wajib terbaca** |
| `Admitted` | Sedang dirawat | Pasien menempati tempat tidur |
| `DischargePending`, kepergian belum dicatat | Rencana pulang | Sudah boleh pulang, episode belum ditutup, tempat tidur **masih dipegang** |
| `DischargePending`, kepergian sudah dicatat | Sudah pulang, menunggu penutupan | Pasien sudah tidak di ruangan dan tempat tidur **sudah bebas**, tetapi episodenya belum ditutup |
| `Closed` | Selesai | Episode ditutup, tempat tidur sudah dilepas |
| `Cancelled` | Batal | Admisi tidak jadi berjalan |

Kata pada kolom kedua adalah **usulan**. Yang mengikat hanya kolom ketiga.

### 4.2 Status tempat tidur

| Nilai | Kata yang diusulkan | Catatan penting |
| --- | --- | --- |
| `Available` | Tersedia | — |
| `Reserved` | Dipesan | **Wajib menampilkan sisa waktu pemesanan** dan, pada layar yang berhak, episode yang memegangnya |
| `Occupied` | Terisi | Wajib menampilkan nama pasien pada layar yang berhak |
| `Cleaning`, `Maintenance`, `Blocked` | Pembersihan, Perbaikan, Diblokir | Disetel admin, bukan oleh Rawat Inap |
| `Inactive` | Nonaktif | — |

**Jebakan penamaan yang wajib dihindari.** Backend punya dua hal berbeda yang sama-sama bernama
"status": `IsActive` pada tempat tidur, dan `BedStatus`. Layar **tidak boleh** memakai kata "status"
sendirian. Pakai "keadaan tempat tidur" untuk `BedStatus` dan "aktif/nonaktif" untuk `IsActive`.
Dasarnya `RWI-CON-TRC-002`.

### 4.3 Lama dirawat — `RWI-FE-001`, `DEV_DISCRETION`

`RWI-RULE-019` menghitung lama dirawat dari **selisih tanggal**, bukan selisih jam, dengan hasil
paling sedikit 1 hari.

| Aspek | Ketetapannya |
| --- | --- |
| Yang **wajib** | Angka itu wajib terbaca jelas sebagai **hitungan hari rawat**, bukan lama waktu sebenarnya |
| Yang **bebas** | Bentuk kalimat, singkatan, penempatan, dan gaya tampilan |
| Kenapa penting | Pasien masuk 21 Sept pukul 22:30 dan pulang 22 Sept pukul 06:00 tercatat **1 hari**, padahal hanya 7,5 jam |

### 4.3A Kelayakan penempatan pada papan tempat tidur, pemesanan, dan perpindahan

| Aspek | Ketetapannya |
| --- | --- |
| Hasil `GET /available-beds` | Sudah tersaring server memakai aturan Kelayakan Penempatan. Layar **tidak boleh** menyaring ulang sendiri |
| Tempat tidur yang tersaring keluar | **Boleh** ditampilkan sebagai baris nonaktif disertai alasannya, dan ini dianjurkan |
| Pemesanan | Memakai daftar dan penyaring yang sama. Tambahannya: tempat tidur ber-`IsReservable` salah ditolak dengan pesan "Tempat tidur ini tidak dapat dipesan." |
| Konfirmasi masuk | Kelayakan diperiksa **ulang** di sini. Penolakan pada tahap ini **wajar** dan wajib terbaca sebagai keadaan yang berubah, bukan sebagai kesalahan petugas |
| Perpindahan | Aturan yang sama persis |
| Kamar yang terhalang pencampuran | Pesan penolakan dari server **menyebut nama kamarnya** dan ditampilkan apa adanya |

### 4.4 Bentuk daftar pantau — `RWI-FE-002`, `DEV_DISCRETION`

**Empat** daftar pantau tersedia pada MVP: penutupan tertunda, penutupan menembus gerbang keuangan,
episode tanpa perawat penanggung jawab, dan penempatan tidak sesuai kebutuhan isolasi. Daftar pantau
kepatuhan pengkajian dan CPPT **belum ada** karena bergantung pada slice yang menunggu `DEC-INP-001`.

| Aspek | Ketetapannya |
| --- | --- |
| Yang **wajib** | Lama keterlambatan terbaca; daftar **tidak boleh** menghalangi tindakan apa pun |
| Yang **bebas** | Satu halaman gabungan atau beberapa halaman terpisah; urutan kolom; cara menandai keterlambatan |

Daftar penempatan tidak sesuai berbeda sifat dari tiga lainnya: isinya bukan keterlambatan petugas,
melainkan akibat wajar dari perubahan kondisi klinis. Nadanya tidak boleh menuduh.

---

## 5. Penanganan keadaan

### 5.1 Keadaan wajib pada setiap layar daftar

| Keadaan | Yang wajib terjadi |
| --- | --- |
| Sedang memuat | Penanda memuat, bukan layar kosong yang menyesatkan |
| Kosong | Kalimat yang menjelaskan kenapa kosong dan apa yang bisa dilakukan |
| Gagal | Pesan dari server ditampilkan apa adanya bila ada, ditambah tombol coba lagi |
| Tidak berhak | Layar tidak dibuka sama sekali, bukan dibuka lalu kosong |

### 5.2 Data basi

| Layar | Risiko basi | Cara menanganinya |
| --- | --- | --- |
| `FE-INP-03` langkah Pilih Bed | **Tinggi** | Muat ulang sebelum dialog konfirmasi pemesanan |
| `FE-INP-02` papan tempat tidur | **Tinggi** | Muat ulang saat difokuskan kembali, dan wajib muat ulang sebelum dialog konfirmasi masuk |
| `FE-INP-16` daftar kerja | Sedang | Muat ulang saat difokuskan kembali. Sisa waktu pemesanan pada baris `Draft` wajib berasal dari jawaban server terakhir |
| `FE-INP-01` census | Sedang | Muat ulang saat difokuskan kembali |
| `FE-INP-07` penutupan | Sedang | Panggil `closure-readiness` **tepat sebelum** tombol tutup dijalankan |
| `FE-INP-14` pencatatan kepergian | Sedang | Muat ulang detail episode sebelum dialog konfirmasi |

**Yang tidak boleh:** menyembunyikan tombol berdasarkan data yang dimuat lima menit lalu.

### 5.3 Pengiriman ganda

Seluruh aksi yang mengubah data wajib mencegah penekanan tombol dua kali: tombol dinonaktifkan
selama permintaan berjalan.

| Aksi | Akibat bila terkirim dua kali | Perlakuan |
| --- | --- | --- |
| Menyimpan langkah Dokter (titik tulis 1) | Dua kunjungan dan dua episode untuk satu pasien | Tombol dikunci; hasil server ditampilkan apa adanya. `INV-INP-01` menolak episode aktif kedua, tetapi pesan yang terbaca petugas wajib jelas |
| Memesan tempat tidur | Permintaan kedua ditolak server | Konfirmasi sebelum kirim |
| Mengonfirmasi pasien masuk | Permintaan kedua ditolak 409 | Konfirmasi menyebut nama pasien dan tempat tidur |
| Memindahkan pasien | Sama | Konfirmasi menyebut tempat tidur asal dan tujuan |
| Membatalkan admisi | Sama | Konfirmasi menyebut bahwa pemesanan dan penempatan ikut dilepas |
| Menutup episode | Sama | Konfirmasi menyebut nama pasien dan cara pulang |
| Menutup menembus gerbang keuangan | Sama, ditambah baris laporan pengecualian | Konfirmasi **wajib** menyebut bahwa episode akan masuk laporan pengecualian |
| Mencatat kepergian pasien | Sama | Konfirmasi **wajib** menyebut bahwa tindakan **tidak dapat dibatalkan** |

### 5.4 Menangani kode 409 dan 422

| Kode | Artinya | Yang harus dilakukan layar |
| --- | --- | --- |
| 409 | Keadaan sudah berubah, misalnya tempat tidur direbut pasien lain | **Muat ulang data**, tampilkan pesan server, biarkan pengguna memilih ulang. Isian yang sudah diketik **tidak boleh hilang** |
| 422 | Aturan bisnis menolak | Tampilkan **daftar** aturan yang gagal, bukan satu kalimat umum |

Untuk 422 pada penutupan episode, layar wajib menampilkan kelima syarat beserta tanda sudah atau
belum. Untuk 422 pada pemesanan dan konfirmasi masuk, alasan penolakan Kelayakan Penempatan wajib
terbaca apa adanya, termasuk nama kamar.

### 5.5 Keadaan khusus alur berlangkah

| Keadaan | Yang wajib terjadi |
| --- | --- |
| Langkah gagal disimpan | Alur **berhenti di langkah itu**, isian utuh, pesan server apa adanya. Tidak boleh melompat ke langkah berikutnya |
| Halaman dimuat ulang di tengah alur | Langkah dan episode yang sedang dikerjakan wajib pulih dari URL, bukan hilang |
| Pemesanan gugur saat alur masih terbuka | Layar wajib menyatakannya dan mengembalikan pengguna ke langkah Pilih Bed |
| Pengguna menutup alur setelah titik tulis 1 | Wajib ada peringatan yang menyebut bahwa episode `Draft` sudah terbentuk dan dapat dilanjutkan dari daftar kerja |

---

## 6. Privasi di layar

| Aturan | Isinya |
| --- | --- |
| Isi resume pulang | Hanya pada layar detail bagi peran ber-`InpatientDischarge : Read`. **Tidak** pada daftar |
| Daftar episode dan census | Hanya nomor episode, nama pasien, lokasi, DPJP, lama dirawat, dan status. Tanpa diagnosis |
| Catatan episode | Bertanda sensitif. Tidak ditampilkan pada daftar |
| Keterangan kebutuhan isolasi | Bertanda sensitif. **Tidak** pada census maupun papan tempat tidur; hanya pada detail episode bagi peran yang berhak. Penandanya boleh tampil sebagai ikon tanpa alasannya |
| Data penjamin | Nomor kartu asuransi dan nomor peserta **tidak** ditampilkan pada daftar mana pun; hanya pada langkah Pembayaran dan pada formulir cetak |
| Formulir persetujuan tercetak | Memuat data pribadi. Halaman cetak **tidak boleh** dapat dijangkau tanpa hak akses, dan tidak boleh menyimpan salinannya di peramban |
| Contoh dan data uji | **Tidak boleh** memakai data pasien atau pegawai asli |

---

## 7. Cacat yang wajib diperbaiki lebih dulu

| Cacat | Keadaannya |
| --- | --- |
| Tombol aktifkan dan nonaktifkan tempat tidur | **Sudah diperbaiki** lewat `FE-RWI-001`. Pemanggilan `PATCH /beds/{id}/status` terbukti berjalan; nol respons 404 |

Tidak ada cacat prasyarat yang tersisa pada revision ini.

---

## 8. Pola yang dipakai ulang

Bagian ini **mengikat**. Ketiganya sudah berjalan di repository, dan mengarang pola keempat untuk
pekerjaan yang sama adalah cara paling mudah menghasilkan dua jalur yang berselisih.

| Pola | Sumber di frontend | Dipakai untuk |
| --- | --- | --- |
| **Alur pendaftaran sisi petugas** | `src/components/view/health-services/registration-management/emergency-registration/` — `patient-entry-choice-step`, `patient-selection-step`, `new-patient-form`, `payment-method-step`, `verification-step`, `registration-success-step`, `emergency-registration-stepper`, `plustek-scan-panel` | Kerangka `FE-INP-03`, termasuk pilihan penjamin |
| **Alur kiosk baru dan lama** | `src/components/view/kiosk/registration/new-patient/`, `…/old-patient/` | Urutan langkah, scan KTP, pemisahan jalur |
| **Cetak kartu pasien** | `src/components/view/kiosk/registration/patient-card/print/` | Langkah 9 jalur pasien baru |
| `InstanceAxios` beserta pembungkus `ApiResponse` | `src/lib/axiosInstance/InstanceAxios` | Seluruh pemanggilan |
| Pembungkus jawaban dan pemakluman 404 | `doctor-consultation.service.js:15-23` | Layanan yang boleh mengembalikan "belum ada" |
| Slice Redux master data | `src/lib/state/slice/health-services/master-data/` | Layar master pengaturan dan butir administrasi |
| Isian pilihan sumber daya | `health-service-select-resources.js` | Pilihan unit layanan, kelas, dokter |
| Kerangka pemanggilan Rawat Inap | `src/lib/services/health-services/inpatient-management/inpatient-api.service.js` | Seluruh pemanggilan modul ini |
| Papan tempat tidur dan penolakan penempatan | `inpatient-bed-board.jsx`, `placement-failure-list.jsx`, `use-inpatient-bed-board.jsx` | Langkah Pilih Bed dan Booking Bed |

**Yang tidak dipakai ulang:** ruang kerja antrean dokter (`useDoctorConsultationWorkspace.js`). Ruang
kerja itu berputar pada `queueId`, sedangkan pasien rawat inap tidak punya antrean.

---

## 9. Keputusan yang didelegasikan

| Decision ID | Area | Status | Batas yang diizinkan |
| --- | --- | --- | --- |
| `RWI-FE-001` | Kata untuk angka hari rawat | `DEV_DISCRETION` | Wajib menyebut jelas bahwa itu hitungan hari rawat |
| `RWI-FE-002` | Bentuk tampilan daftar pantau | `DEV_DISCRETION` | Lama keterlambatan terbaca; tidak menghalangi tindakan |
| `RWI-FE-003` | Nama dan label kesembilan langkah alur admisi | `DEV_DISCRETION` | **Urutan dan isinya mengikat** sesuai 3A.2 dan 3A.3; katanya bebas |
| `RWI-FE-004` | Bentuk penanda langkah — garis, angka, atau tab | `DEV_DISCRETION` | Langkah yang sedang berjalan dan yang sudah lewat wajib terbeda |
| `RWI-FE-005` | Tata letak Beranda | `DEV_DISCRETION` | Ketiga isi wajib pada 2B tercapai dan dapat diklik |
| Baru | Nama menu, urutan menu, dan route final | `DEV_DISCRETION` | Mengikuti konvensi `src/app/health-services/`, tunduk pada `IA-INP-01` s.d. `IA-INP-05` |
| Baru | Pemakaian tab, modal, atau drawer | `DEV_DISCRETION` | Bebas, selama aturan 5.3 dipenuhi |
| Baru | Penggabungan layar | `DEV_DISCRETION` | Sembilan belas layar bagian 2 boleh digabung, selama kemampuannya tercapai dan 2B dipenuhi |

**Yang bukan `DEV_DISCRETION`:** peta 2A, aturan keterjangkauan 2B, aturan tombol bagian 3,
kontrak alur bagian 3A, penanganan 409 dan 422 pada 5.4, dan privasi bagian 6.

---

## 10. Ketergantungan test

| Yang diuji | Jenis | Catatan |
| --- | --- | --- |
| Layar dapat dijangkau peran yang berhak, dan tidak dapat dijangkau yang tidak berhak | e2e | Menambah kasus pada `tests/e2e/route-smoke.spec.mjs` |
| Alur admisi jalur pasien baru berjalan dari langkah 1 sampai tempat tidur `Reserved` | e2e | **Wajib** — ini alur bisnis utama modul |
| Alur admisi jalur pasien lama berjalan dari pencarian sampai tempat tidur `Reserved` | e2e | **Wajib** |
| Alur yang ditinggal setelah titik tulis 1 dapat ditemukan kembali dari `FE-INP-16` dan dilanjutkan | e2e | **Wajib** — membuktikan 3A.6 |
| Kunjungan yang terbentuk membawa penjamin yang dipilih, bukan `Cash` bawaan | e2e atau pemeriksaan jaringan | **Wajib** — inilah cacat yang revision ini tutup |
| Mundur dari Dokter ke Pembayaran ditolak disertai penjelasan | e2e | Membuktikan 3A.5 |
| Konfirmasi masuk menolak tempat tidur yang keburu tidak layak | e2e | Membuktikan pemeriksaan ulang 4.3A |
| Tombol pindah nonaktif bagi dokter yang bukan DPJP aktif | e2e | Sudah ada |
| Daftar syarat penutupan tampil lengkap saat 422 | e2e | Sudah ada |

Dasar kewajiban test: `RWI-DEC-051`.

---

## 11. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| 2A | `04-prd-to-mvp.md` bagian 9 `FLOW-RI-MVP-001`; `contracts/api-contract.md` `0.4.0` |
| 2B, 2C | `RWI-DEC-078`; temuan keterjangkauan revision `0.3` |
| 3 | `contracts/permission-audit-matrix.md` `0.4.0` |
| 3.2 | `RWI-DEC-076`; `RWI-OQ-045` |
| 3A | `RWI-DEC-075`, `RWI-DEC-076`, `RWI-DEC-079` |
| 3A.8 | `RWI-DEC-077`; `RWI-DEC-035`; `RWI-CAP-031` dan `DEC-INP-003` tetap terbuka |
| 4.0 | `01-existing-capability-map.md` `RWI-CAP-002`; `02-backend-architecture.md` bagian 5 |
| 4.3 | `RWI-RULE-019`, `RWI-FE-001` |
| 4.3A | `RWI-RULE-012`, `RWI-DEC-064` s.d. `RWI-DEC-066`; `contracts/validation-matrix.md` |
| 4.4 | `RWI-RULE-023`, `RWI-FE-002`, `RWI-AC-138` |
| 5.3, 5.4 | `RWI-RULE-008`, `RWI-RULE-010`, `RWI-RULE-015`, `RWI-RULE-021` |
| 6 | Kolom sensitif pada `data/data-dictionary.md` |
| `FE-INP-14` | `RWI-RULE-036`, `RWI-DEC-055` |
| 10 | `RWI-DEC-051`, `RWI-RISK-002` |

---

## 11A. Cakupan endpoint

**Bagian ini mengikat.** Setiap operasi pada `contracts/api-contract.md` wajib dimiliki tepat satu
layar, atau dinyatakan **sengaja tidak dipakai** beserta alasannya. Endpoint tak bertuan adalah
cacat blueprint — revision `0.3` meninggalkan sembilan di antaranya.

### Yang menganggur pada revision `0.3`, dan pemiliknya sekarang

| Operasi | Pemilik baru |
| --- | --- |
| `GET /episodes` | `FE-INP-16` |
| `GET /episodes/filters/metadata` | `FE-INP-16` |
| `GET /episodes/summary` | `FE-INP-19` |
| `GET /census/summary` | `FE-INP-19` |
| `GET /census/filters/metadata` | `FE-INP-01` |
| `PUT /episodes/{id}` | `FE-INP-03` titik tulis 3 |
| `PATCH /episodes/{id}/cancel` | `FE-INP-17` |
| `POST /bed-occupancies/reservations` | `FE-INP-03` langkah Booking Bed |
| `PATCH /bed-occupancies/reservations/{id}/cancel` | `FE-INP-03` langkah Booking Bed, dan `FE-INP-02` |

### Endpoint milik modul lain yang dikonsumsi modul ini

| Operasi | Pemilik data | Dipakai di |
| --- | --- | --- |
| `POST /patients`, `POST /patient-identity-documents`, `POST /patient-emergency-contacts` | PatientManagement | `FE-INP-03` langkah Pendaftaran |
| `POST /patient-insurances`, `POST /patient-company-guarantors` | PatientManagement | `FE-INP-03` langkah Pembayaran |
| `POST /patient-encounters` | RegistrationManagement | `FE-INP-03` titik tulis 1 |

**Catatan penting tentang `POST /patient-encounters`.** Inilah satu-satunya alasan cacat penjamin
tertutup tanpa perubahan backend: endpoint itu membuat baris kunjungan **dan** baris penjamin
sekaligus, lalu menyetel ringkasan pembayaran kunjungan. Karena `EncounterId` menjadi terisi,
`POST /episodes` tidak pernah lagi menempuh jalur yang membuat kunjungan sendiri.

**Butir terbuka `RWI-OQ-046`:** jalur "buka admisi tanpa `EncounterId`" masih ada di backend dan
masih menanam cara bayar tunai beserta kunjungan tanpa penjamin. Setelah revision ini tidak ada
layar yang menempuhnya, tetapi jalurnya tetap terbuka bagi pemanggil lain. Perlu diputuskan apakah
jalur itu ditutup. Owner: Backend/API bersama Product/Domain. **Di luar** wewenang dokumen ini.
