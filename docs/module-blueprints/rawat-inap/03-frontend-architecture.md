# Rawat Inap — Arsitektur Frontend

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | `0.1` |
| Status | `draft` |
| Frontend SHA | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` |
| Backend SHA | `5afb54bd75281648010e50ef14f43ca1f80d8efd` |
| Masukan | `02-backend-architecture.md` revision `0.1`; `contracts/api-contract.md` revision `0.1.0` |
| Batas tulis | Hanya dokumen blueprint |

> **Batas kewenangan dokumen ini.** Dokumen ini menetapkan **kontrak fungsional**: layar apa yang
> dibutuhkan, siapa boleh melakukan apa, data dan status apa yang dikonsumsi, dan bagaimana keadaan
> gagal ditangani.
>
> Dokumen ini **tidak** menetapkan menu sidebar, urutan menu, route final, pemakaian tab atau modal
> atau drawer, warna, tata letak, maupun pustaka komponen. Seluruhnya adalah wewenang pelaksana
> frontend selama tidak melanggar keamanan, privasi, atau invariant.

Urutan wewenang yang dipakai:

```text
keamanan / privasi / invariant
  -> brief produk atau UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

---

## 1. Keadaan frontend saat ini

Berdasarkan capability map revision `1.2`:

| Hal | Keadaannya |
| --- | --- |
| Route Rawat Inap | **Tidak ada satu pun.** `src/app/health-services/` hanya memuat enam folder, tidak ada inpatient |
| Menu Rawat Inap | **Tidak ada.** Menu hanya mengenal "Rawat Jalan" dan "Instalasi Gawat Darurat" |
| Layar master tempat tidur | Ada, dan **satu tombolnya rusak** — lihat bagian 7 |
| Pola yang dapat dipakai ulang | Slice Redux master data, pola `InstanceAxios`, pola pembungkus `ApiResponse`, pola isian pilihan `health-service-select-resources.js` |

Artinya seluruh layar modul ini berstatus **baru**, tanpa satu pun yang tinggal diperbarui.

---

## 2. Kebutuhan layar

Nama layar di bawah adalah **nama fungsional**, bukan nama menu. Pelaksana bebas menamai ulang dan
menggabungkan selama seluruh kemampuannya tercapai.

| ID | Layar | Tujuan | Pemakai utama |
| --- | --- | --- | --- |
| `FE-INP-01` | Daftar pasien dirawat (census) | Melihat siapa dirawat, di mana, oleh siapa, dan sudah berapa hari | Perawat, kepala ruangan, admisi, DPJP |
| `FE-INP-02` | Papan ketersediaan tempat tidur | Melihat tempat tidur kosong, dipesan, terisi, dan sedang ditutup | Admisi, kepala ruangan |
| `FE-INP-03` | Admisi pasien | Membuka admisi, memilih penjamin, DPJP, kelas, dan tempat tidur | Petugas admisi |
| `FE-INP-04` | Detail episode | Melihat satu episode utuh: status, lokasi terkini, DPJP, perawat, riwayat | Semua peran klinis dan admisi |
| `FE-INP-05` | Perpindahan pasien | Memindahkan pasien ke tempat tidur lain beserta alasannya | Kepala ruangan, perawat, supervisor, DPJP |
| `FE-INP-06` | Keputusan pulang dan resume | DPJP menyatakan pasien boleh pulang lalu menyusun dan menandatangani resume | DPJP |
| `FE-INP-07` | Penutupan episode | Menandai butir administrasi, melihat kelima syarat, lalu menutup episode | Petugas admisi, supervisor |
| `FE-INP-08` | Penandaan kelayakan keuangan | Kasir menandai `Cleared` atau `Blocked` beserta catatannya | Kasir, billing |
| `FE-INP-09` | Daftar pantau | Tiga daftar pantau yang tersedia pada MVP | Admisi, kepala ruangan, supervisor |
| `FE-INP-10` | Laporan selisih tempat tidur | Menemukan tempat tidur yang statusnya tidak cocok dengan penghuninya | Admin, supervisor |
| `FE-INP-11` | Sesi koreksi episode | Supervisor membuka, mengoreksi, lalu menutup sesi | Supervisor |
| `FE-INP-12` | Pengaturan Rawat Inap | Mengubah batas waktu dan ambang | Admin master data |
| `FE-INP-13` | Master butir administrasi | Menambah, mengubah, dan menonaktifkan butir daftar periksa | Admin master data |

---

## 3. Aksi per peran

Kolom ini menurunkan langsung dari `contracts/permission-audit-matrix.md`. Tombol yang tidak
diizinkan **harus disembunyikan atau dinonaktifkan**, bukan ditampilkan lalu ditolak server.

| Aksi di layar | Petugas admisi | Perawat | Kepala ruangan | DPJP | Kasir | Supervisor | Admin |
| --- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| Melihat census | Ya | Ya | Ya | Ya | Ya | Ya | – |
| Mencari tempat tidur kosong | Ya | Ya | Ya | – | – | Ya | – |
| Membuka admisi | Ya | – | – | – | – | Ya | – |
| Memesan tempat tidur | Ya | – | – | – | – | Ya | – |
| Menempatkan pasien | Ya | – | – | – | – | Ya | – |
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
| Menutup menembus gerbang keuangan | – | – | – | – | – | Ya | – |
| Membuka sesi koreksi | – | – | – | – | – | Ya | – |
| Mengubah pengaturan dan butir | – | – | – | – | – | – | Ya |

**Dua tombol yang paling perlu diperhatikan:**

| Tombol | Aturan tampilnya |
| --- | --- |
| Pindahkan pasien, untuk pengguna berperan dokter | Hanya aktif bila dokter itu **DPJP aktif episode tersebut**. Bila bukan, tombol dinonaktifkan disertai keterangan "Anda bukan DPJP episode ini" |
| Tutup menembus gerbang keuangan | **Tidak boleh** ditampilkan berdampingan dengan tombol tutup biasa seolah dua pilihan setara. Ia baru muncul setelah tombol tutup biasa ditolak karena kelayakan keuangan, dan hanya untuk supervisor |

---

## 4. Data dan status yang dikonsumsi

### 4.1 Status episode dan cara menampilkannya

| Nilai dari backend | Kata yang dipakai di layar | Yang wajib terbaca pengguna |
| --- | --- | --- |
| `Draft` | Admisi sedang disiapkan | Pasien belum tentu ada di kamar |
| `Admitted` | Sedang dirawat | Pasien menempati tempat tidur |
| `DischargePending` | Rencana pulang | Sudah boleh pulang, episode belum ditutup, tempat tidur masih dipegang |
| `Closed` | Selesai | Episode ditutup, tempat tidur sudah dilepas |
| `Cancelled` | Batal | Admisi tidak jadi berjalan |

Kata pada kolom kedua adalah **usulan**, bukan keputusan. Yang mengikat hanya kolom ketiga: makna
itu wajib terbaca, apa pun kata yang dipilih.

### 4.2 Status tempat tidur

| Nilai | Kata yang diusulkan | Catatan penting |
| --- | --- | --- |
| `Available` | Tersedia | — |
| `Reserved` | Dipesan | Wajib menampilkan sisa waktu pemesanan |
| `Occupied` | Terisi | Wajib menampilkan nama pasien pada layar yang berhak |
| `Cleaning`, `Maintenance`, `Blocked` | Pembersihan, Perbaikan, Diblokir | Disetel admin, bukan oleh Rawat Inap |
| `Inactive` | Nonaktif | — |

**Jebakan penamaan yang wajib dihindari.** Backend punya dua hal berbeda yang sama-sama bernama
"status": `IsActive` pada tempat tidur, dan `BedStatus`. Layar **tidak boleh** memakai kata "status"
sendirian. Pakai "keadaan tempat tidur" untuk `BedStatus` dan "aktif/nonaktif" untuk `IsActive`.
Dasarnya `RWI-CON-TRC-002` pada capability map.

### 4.3 Lama dirawat — `RWI-FE-001`, `DEV_DISCRETION`

`RWI-RULE-019` menghitung lama dirawat dari **selisih tanggal**, bukan selisih jam, dengan hasil
paling sedikit 1 hari.

| Aspek | Ketetapannya |
| --- | --- |
| Yang **wajib** | Angka itu wajib terbaca jelas sebagai **hitungan hari rawat**, bukan lama waktu sebenarnya |
| Yang **bebas** | Bentuk kalimat, singkatan, penempatan, dan gaya tampilan |
| Kenapa penting | Pasien masuk 21 Sept pukul 22:30 dan pulang 22 Sept pukul 06:00 tercatat **1 hari**, padahal hanya 7,5 jam. Kalau labelnya "lama dirawat 1 hari" tanpa penjelasan, pengguna akan menyangka sistem salah hitung |

### 4.4 Bentuk daftar pantau — `RWI-FE-002`, `DEV_DISCRETION`

| Aspek | Ketetapannya |
| --- | --- |
| Yang **wajib** | Lama keterlambatan terbaca; daftar **tidak boleh** menghalangi tindakan apa pun |
| Yang **bebas** | Satu halaman gabungan atau beberapa halaman terpisah; urutan kolom; cara menandai keterlambatan; penempatan menu |

Tiga daftar pantau yang tersedia pada MVP: penutupan tertunda, penutupan menembus gerbang keuangan,
dan episode tanpa perawat penanggung jawab. Daftar pantau kepatuhan pengkajian dan CPPT **belum
ada** karena bergantung pada slice yang masih menunggu `DEC-INP-001`.

---

## 5. Penanganan keadaan

### 5.1 Keadaan wajib pada setiap layar daftar

| Keadaan | Yang wajib terjadi |
| --- | --- |
| Sedang memuat | Penanda memuat, bukan layar kosong yang menyesatkan |
| Kosong | Kalimat yang menjelaskan kenapa kosong dan apa yang bisa dilakukan. Contoh census kosong: "Belum ada pasien yang dirawat di unit ini." |
| Gagal | Pesan dari server ditampilkan apa adanya bila ada, ditambah tombol coba lagi |
| Tidak berhak | Layar tidak dibuka sama sekali, bukan dibuka lalu kosong |

### 5.2 Data basi

| Layar | Risiko basi | Cara menanganinya |
| --- | --- | --- |
| `FE-INP-02` papan tempat tidur | Tinggi. Tempat tidur bisa direbut petugas lain dalam hitungan detik | Muat ulang saat layar difokuskan kembali, dan wajib muat ulang sebelum menampilkan dialog konfirmasi penempatan |
| `FE-INP-01` census | Sedang | Muat ulang saat layar difokuskan kembali |
| `FE-INP-07` penutupan | Sedang. Kelayakan keuangan bisa berubah saat layar terbuka | Panggil `closure-readiness` **tepat sebelum** tombol tutup dijalankan, bukan hanya saat layar dibuka |

**Yang tidak boleh dilakukan:** menyembunyikan tombol tutup berdasarkan data yang dimuat lima menit
lalu, lalu menganggap keadaan tidak berubah.

### 5.3 Pengiriman ganda

Seluruh aksi yang mengubah data wajib mencegah penekanan tombol dua kali: tombol dinonaktifkan
selama permintaan berjalan.

Aksi berikut **paling berbahaya** bila terkirim dua kali, dan wajib diberi perlakuan tambahan
berupa dialog konfirmasi:

| Aksi | Akibat bila terkirim dua kali | Perlakuan |
| --- | --- | --- |
| Menempatkan pasien | Permintaan kedua ditolak 409 oleh server, tetapi pengguna bingung | Konfirmasi sebelum kirim; tampilkan hasil server apa adanya |
| Memindahkan pasien | Sama | Konfirmasi menyebut tempat tidur asal dan tujuan |
| Menutup episode | Permintaan kedua ditolak 409 | Konfirmasi menyebut nama pasien dan cara pulang |
| Menutup menembus gerbang keuangan | Sama, ditambah baris laporan pengecualian | Konfirmasi **wajib** menyebut bahwa episode akan masuk laporan pengecualian |

### 5.4 Menangani kode 409 dan 422

Dua kode ini adalah yang paling sering muncul pada modul ini, dan artinya berbeda:

| Kode | Artinya | Yang harus dilakukan layar |
| --- | --- | --- |
| 409 | Keadaan sudah berubah. Contoh: tempat tidur direbut pasien lain | **Muat ulang data**, tampilkan pesan server, biarkan pengguna memilih ulang. Isian yang sudah diketik **tidak boleh hilang** |
| 422 | Aturan bisnis menolak. Contoh: masih ada syarat penutupan yang belum terpenuhi | Tampilkan **daftar** syarat yang belum terpenuhi, bukan satu kalimat umum |

Untuk 422 pada penutupan episode, server mengembalikan daftar syarat lewat `closure-readiness`.
Layar wajib menampilkan kelimanya beserta tanda sudah atau belum, supaya petugas tahu apa yang
harus dikejar.

---

## 6. Privasi di layar

| Aturan | Isinya |
| --- | --- |
| Isi resume pulang | Hanya tampil pada layar detail bagi peran yang punya `InpatientDischarge : Read`. **Tidak** ditampilkan pada daftar |
| Daftar episode dan census | Hanya nomor episode, nama pasien, lokasi, DPJP, lama dirawat, dan status. Tanpa diagnosis |
| Catatan episode | Bertanda sensitif. Tidak ditampilkan pada daftar |
| Contoh dan data uji | **Tidak boleh** memakai data pasien atau pegawai asli |

---

## 7. Cacat yang wajib diperbaiki lebih dulu

Ini prasyarat, bukan pekerjaan yang boleh menyusul.

| Cacat | Keadaannya | Perbaikan yang diputuskan |
| --- | --- | --- |
| Tombol aktifkan dan nonaktifkan tempat tidur | Memanggil `PATCH /beds/{id}/activate` dan `/deactivate` yang **tidak ada** di backend. Selalu gagal 404 | Ubah pemanggilan menjadi `PATCH /beds/{id}/status` yang sudah ada, sesuai `RWI-DEC-049`. **Tidak ada perubahan backend** |

Berkas yang disentuh: `src/lib/state/slice/health-services/master-data/master-data-bed-slice.jsx`
baris 315-322 dan 334-341.

**Kenapa ini prasyarat.** `RWI-RULE-027` mencabut wewenang admin atas `Reserved` dan `Occupied`.
Setelah itu, satu-satunya cara admin menutup tempat tidur yang rusak adalah menonaktifkannya atau
menyetel `Maintenance`. Kalau tombol nonaktif tidak berfungsi, admin kehilangan kemampuan itu, dan
pencarian tempat tidur kosong akan menampilkan tempat tidur yang seharusnya tidak boleh dipakai.

---

## 8. Pola yang dipakai ulang

| Pola | Sumber di frontend | Dipakai untuk |
| --- | --- | --- |
| `InstanceAxios` beserta pembungkus `ApiResponse` | `src/lib/axiosInstance/InstanceAxios` | Seluruh pemanggilan |
| Pembungkus jawaban dan pemakluman 404 | `src/lib/services/health-services/clinical-management/doctor-consultation.service.js:15-23` | Layanan yang boleh mengembalikan "belum ada" |
| Slice Redux master data | `src/lib/state/slice/health-services/master-data/` | Layar master pengaturan dan butir administrasi |
| Isian pilihan sumber daya | `src/lib/hooks/select/health-service/health-service-select-resources.js` | Pilihan tempat tidur, kamar, unit layanan, kelas |

**Yang tidak dipakai ulang:** ruang kerja antrean dokter
(`useDoctorConsultationWorkspace.js`). Ruang kerja itu berputar pada `queueId`, sedangkan pasien
rawat inap tidak punya antrean. Menyalinnya akan membawa masalah yang sama.

---

## 9. Keputusan yang didelegasikan

| Decision ID | Area | Status | Batas yang diizinkan |
| --- | --- | --- | --- |
| `RWI-FE-001` | Kata untuk angka hari rawat pada census | `DEV_DISCRETION` | Wajib menyebut jelas bahwa itu hitungan hari rawat, bukan lama waktu sebenarnya |
| `RWI-FE-002` | Bentuk tampilan daftar pantau | `DEV_DISCRETION` | Lama keterlambatan terbaca; daftar tidak menghalangi tindakan apa pun |
| Baru | Nama menu, urutan menu, dan route final | `DEV_DISCRETION` | Mengikuti konvensi `src/app/health-services/` yang sudah ada |
| Baru | Pemakaian tab, modal, atau drawer | `DEV_DISCRETION` | Bebas, selama aturan pengiriman ganda pada 5.3 dipenuhi |
| Baru | Penggabungan layar | `DEV_DISCRETION` | Tiga belas layar pada bagian 2 boleh digabung, selama seluruh kemampuannya tercapai |

**Yang bukan `DEV_DISCRETION`:** aturan tombol pada bagian 3, penanganan 409 dan 422 pada bagian
5.4, privasi pada bagian 6, dan perbaikan pada bagian 7. Keempatnya menyentuh keamanan, privasi,
atau invariant.

---

## 10. Ketergantungan test

| Yang diuji | Jenis | Catatan |
| --- | --- | --- |
| Layar dapat dijangkau peran yang berhak, dan tidak dapat dijangkau yang tidak berhak | e2e | Menambah kasus pada `tests/e2e/route-smoke.spec.mjs` yang sudah ada |
| Tombol pindah nonaktif bagi dokter yang bukan DPJP aktif | e2e | Membuktikan `GUARD-INP-01` terlihat di layar |
| Daftar syarat penutupan tampil lengkap saat 422 | e2e | — |
| Perbaikan tombol tempat tidur | unit atau e2e | **Wajib**, karena hari ini tidak ada satu pun test yang menyentuh layar bed |

Dasar kewajiban test: `RWI-DEC-051`.

---

## 11. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| 2, 3 | `contracts/api-contract.md`, `contracts/permission-audit-matrix.md` |
| 4.3 | `RWI-RULE-019`, `RWI-FE-001` |
| 4.4 | `RWI-RULE-023`, `RWI-FE-002` |
| 5.3, 5.4 | `RWI-RULE-008`, `RWI-RULE-010`, `RWI-RULE-015` |
| 6 | Kolom sensitif pada `erd/data-dictionary.md` |
| 7 | `RWI-CON-TRC-001`, `RWI-DEC-049` |
| 10 | `RWI-DEC-051`, `RWI-RISK-002` |
