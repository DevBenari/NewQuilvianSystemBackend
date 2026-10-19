# ISSUE-022 — Alur Admisi Bayi Melompat, Berulang, dan Gagal Saat Membentuk Kunjungan

```yaml
issue_id: ISSUE-EPS-022
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — alur admisi bayi pasien baru dan pasien lama (FE-INP-03)"
sumber_laporan: "Laporan pengguna 15 Oktober 2026: tujuh keluhan bernomor, enam lampiran layar, dan satu tambahan tombol pembatalan"
tanggal_issue: "2026-10-15"
status: SUPERSEDED_BY_RWI_BAYI_022_SOURCE_IMPLEMENTED_RUNTIME_PENDING
tanggal_selesai_source: "2026-10-16"
keparahan_tertinggi: High
source_sha_backend: "ca09cae909597442385ce4513be9c803549be10e (MHamzah)"
source_sha_frontend: "4d9bf608dbbaddc96a2b4acfc8723753b5dd5028 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-022-perbaikan-alur-admisi-bayi.md
```

---

> **Koreksi scope final, 16 Oktober 2026.** Setelah dokumen issue ini dibuat, pemilik menetapkan
> bahwa alur bayi tidak mempunyai input deposit tambahan dan perubahan **Batalkan Pendaftaran**
> tidak perlu dikerjakan. Kontrak final adalah
> [`RWI-BAYI-022-2.0.0`](../../contracts/newborn-admission-repair-022.md). Uraian deposit dan
> pembatalan di bawah dipertahankan sebagai riwayat laporan awal, bukan scope implementasi final.

## 1. Ringkasan

Alur admisi bayi mempunyai dua masalah fungsional berkeparahan tinggi dan beberapa masalah konsistensi UI/UX. Masalah pertama terjadi ketika pengguna memilih kategori **bayi** pada pendaftaran pasien baru: daftar langkah langsung diganti dari alur umum ke alur bayi, sedangkan fungsi pencari langkah berikutnya masih membaca posisi pada daftar lama. Karena slug pertama yang sama pada kedua daftar adalah `payment`, sistem mengarahkan pengguna ke **Langkah 4 — Penjamin dan Penagihan**, melewati konteks, identitas, dan perawatan bayi.

Masalah kritis kedua berada pada hubungan pemilihan bed dengan pembentukan kunjungan. Daftar bed dapat menampilkan bed dari unit layanan yang tidak memenuhi syarat registrasi, sedangkan endpoint pembentukan kunjungan secara benar menolak unit yang tidak aktif, dihapus, tidak tersedia untuk registrasi, atau bukan unit rawat inap. Ketidaksamaan penyaringan ini menghasilkan HTTP 400 dengan pesan `Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.`

Keluhan lain merupakan perubahan desain yang sah terhadap rancangan admisi bayi sebelumnya: kartu ringkasan bayi–ibu tidak boleh diulang pada setiap langkah; input asal `luar RS/rujukan` tidak boleh ditanyakan kembali karena sudah ditentukan pada kategori kunjungan; istilah rawat ulang perlu dijelaskan; dan layar identitas perlu dikelompokkan lebih rapi. Usulan awal agar deposit memakai komponen umum kemudian digantikan keputusan final: bayi tidak mempunyai langkah/input deposit tambahan.

Permintaan tambahan mengenai pola pembatalan sempat dianalisis dalam dokumen ini, tetapi kemudian ditarik oleh pemilik dengan instruksi **“Batalkan Pendaftaran ini skip tidak perlu dikerjakan”**. Karena itu perilaku pembatalan existing dipertahankan tanpa perubahan.

---

## 2. Laporan Asli Pelapor

| No. Laporan | Ringkasan Keluhan Pelapor | Lampiran |
| :---: | :--- | :--- |
| **1** | Pendaftaran pasien baru dengan episode bayi langsung menuju Langkah 4. | Lampiran 1 — layar berada di Langkah 4 dari 9. |
| **2 (pertama)** | Informasi bayi dan ibu memenuhi tampilan karena diulang pada setiap langkah; cukup tampil pada konfirmasi terakhir. | Lampiran 1 dan 2 — kartu `Bayi dan ibu` berulang. |
| **2 (kedua)** | Konteks admisi bayi belum jelas; pilihan `Datang dari luar RS / rujukan` menduplikasi kategori yang sudah diisi pada langkah awal. | Lampiran 3 — tiga pilihan konteks. |
| **3** | Kalimat `Kembali dirawat setelah pulang` tidak menjelaskan kondisi kapan harus dipilih. | Lampiran 3. |
| **4** | Tampilan identitas bayi dan ibu perlu dirapikan. | Lampiran 4 — pemilih ibu, kartu informasi, dan formulir bayi terpisah-pisah. |
| **5** | Tampilan deposit bayi harus mengikuti admisi umum, bukan membuat tampilan baru. | Lampiran 5 — layar `Rencana Deposit Bayi`. |
| **6** | Pendaftaran bed gagal ketika POST pembentukan kunjungan mengembalikan HTTP 400 karena service unit tidak valid/tidak tersedia. Pesan perlu ditangani dengan rapi dan ramah pengguna. | Lampiran 6 — response API HTTP 400. |
| **Tambahan 15-10-2026** | Tambahkan tombol pembatalan dengan mencontoh admisi baru pasien umum dan tentukan langkah penempatannya. | Pesan tambahan pelapor. |

> Catatan privasi: nama, nomor rekam medis, dan identitas lain yang tampak pada lampiran tidak disalin ke dokumen ini. Seluruh contoh pengujian wajib memakai data uji seperti `Bayi A`, `Ibu A`, dan `RM-DEMO-001`.

---

## 3. Ringkasan Temuan

| ID Temuan | No. Laporan | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | Perbaikan |
| :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-EPS-022-01` | 1 | Pergantian daftar langkah secara prematur mengarahkan alur bayi ke slug bersama `payment` | `BUG` | Frontend | High | Terverifikasi pada source | `FIX-EPS-022-01` |
| `ISS-EPS-022-02` | 2 pertama | Kartu bayi–ibu dirender secara global pada setiap langkah | `DESIGN_CHANGE` | Frontend/UI | Medium | Terverifikasi pada source | `FIX-EPS-022-02` |
| `ISS-EPS-022-03` | 2 kedua | Konteks `luar RS/rujukan` menduplikasi kategori kunjungan awal | `DESIGN_CHANGE` / `RULE_VIOLATION` | Alur/UI | Medium | Terverifikasi pada source dan blueprint | `FIX-EPS-022-03` |
| `ISS-EPS-022-04` | 3 | Istilah `Kembali dirawat setelah pulang` tidak menjelaskan episode baru dan syarat pemakaiannya | `DESIGN_CHANGE` | Konten/UI | Medium | Terverifikasi pada source | `FIX-EPS-022-03` |
| `ISS-EPS-022-05` | 4 | Identitas ibu dan bayi terfragmentasi dan menduplikasi informasi | `DESIGN_CHANGE` | Frontend/UI | Medium | Terverifikasi pada source | `FIX-EPS-022-04` |
| `ISS-EPS-022-06` | 5 | Deposit bayi memakai tampilan khusus yang lebih miskin aturan daripada komponen admisi umum | `DESIGN_CHANGE` / `RULE_VIOLATION` | Frontend/UI | Medium | Superseded: langkah deposit dihapus | `RWI-BAYI-022-2.0.0` |
| `ISS-EPS-022-07` | 6 | Daftar bed dan endpoint pembentukan kunjungan memakai syarat kelayakan unit yang berbeda | `BUG` / `CONTRACT_MISMATCH` | Backend + Frontend | High | Terverifikasi pada source; data master unit spesifik belum tersedia | `FIX-EPS-022-06` |
| `ISS-EPS-022-08` | Tambahan | Komponen langkah bayi tidak menerima aksi pembatalan yang sudah tersedia pada alur umum | `BUG` / `RULE_VIOLATION` | Frontend + lifecycle | High | Ditarik pemilik; tidak dikerjakan | Di luar scope |

---

## 4. Rincian Per Temuan

### ISS-EPS-022-01 — Alur Bayi Melompat Langsung ke Langkah 4

**Apa yang terjadi.** Ketika kartu pasien bayi dipilih pada alur pasien baru, URL dan stepper berpindah langsung ke `payment`, yang dalam alur bayi adalah Langkah 4. Langkah `newborn-context`, `newborn-identity`, dan `doctor` tidak dijalankan.

**Akar penyebab.** Daftar langkah aktif berubah segera setelah `patientType` menjadi bayi. Fungsi penentu langkah berikutnya kemudian mencoba mencari slug setelah `patient-type` pada daftar langkah umum. Karena `registration` tidak ada pada daftar bayi dan slug bersama pertama yang ditemukan adalah `payment`, fallback memilih Langkah 4.

**Bukti source frontend pada SHA `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028`:**

| Lokasi | Bukti |
| :--- | :--- |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx:90-115` | Alur pasien baru umum menempatkan `registration` lalu `payment`. |
| `src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js:1-11` | Alur bayi memakai `newborn-context`, `newborn-identity`, `doctor`, lalu `payment`. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx:73-105` | Daftar langkah diganti berdasar `patientType`, lalu fallback mencari slug bersama berikutnya. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx:266-285` | Efek sinkronisasi menulis slug fallback ke URL. |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx:243-260` | Pilihan tipe pasien langsung mengubah `patientType`. |

**Dampak.** Data konteks kelahiran, identitas bayi, dan penetapan perawatan/DPJP dapat terlewati. Ini bukan sekadar kesalahan penomoran tampilan, melainkan risiko ketidaklengkapan data sebelum pembentukan kunjungan.

**Rekomendasi.** Pisahkan `patientType` yang baru dipilih dari cabang alur yang sudah dikomit. Saat pengguna menekan **Lanjut**, lakukan satu transisi atomik: simpan tipe bayi dan navigasikan eksplisit ke `newborn-context`. Jangan memakai pencarian slug bersama untuk memasuki cabang baru.

---

### ISS-EPS-022-02 — Ringkasan Bayi dan Ibu Diulang pada Setiap Langkah

**Apa yang terjadi.** Kartu besar berisi identitas bayi, nomor RM, ibu, perawatan, dan episode selalu muncul di atas isi langkah bayi.

**Akar penyebab.** `NewbornAdmissionHeader` diletakkan di induk tampilan dan dirender setiap kali `newborn.enabled`, bukan hanya pada langkah konfirmasi.

**Bukti source:**

- `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx:41-49` mendefinisikan kartu `Bayi dan ibu`.
- `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx:1229` merender kartu sebelum setiap `AdmissionStep` ketika mode bayi aktif.

**Dampak.** Ruang vertikal terpakai untuk informasi berulang, isi utama terdorong ke bawah, dan petugas sulit memusatkan perhatian pada pekerjaan langkah aktif.

**Rekomendasi.** Hapus render global. Tampilkan satu ringkasan identitas yang ringkas hanya pada **Langkah 8 — Tinjau dan Aktifkan**, tepat sebelum pengguna mengaktifkan admisi. Form identitas lengkap tetap berada pada Langkah 2 karena merupakan tugas utama langkah tersebut.

---

### ISS-EPS-022-03 — Konteks Luar RS/Rujukan Ditanyakan Dua Kali

**Apa yang terjadi.** Pengguna sudah memilih jenis kunjungan `Umum` atau `Rujukan` pada awal alur, tetapi pada konteks admisi bayi kembali diminta memilih `Datang dari luar RS / rujukan`.

**Akar penyebab desain.** Model `newbornAdmission.source` saat ini menyatukan dua konsep berbeda:

1. asal kedatangan (`HospitalBirth` atau `External`); dan
2. siklus episode (`Readmission`).

**Bukti source dan dokumen:**

- `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx:207-225` sudah menyediakan jenis kunjungan umum/rujukan.
- `src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js:13-17` masih menyediakan `HospitalBirth`, `External`, dan `Readmission`.
- `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx:52-68` merender ulang ketiga pilihan tersebut.
- `src/components/view/health-services/inpatient-management/inpatient-admission-visit-category-step.jsx:52-87` menyimpan jenis kunjungan dan pada pasien lama dapat mengarahkan secara eksplisit ke konteks bayi.
- `contracts/newborn-admission-repair-016.md` mempertahankan enum sumber lama; perubahan yang diminta pelapor perlu menjadi amendemen desain, bukan penyisipan diam-diam.

**Rekomendasi.** Jangan tampilkan pilihan `Datang dari luar RS / rujukan` lagi. Tampilkan kategori awal sebagai informasi baca-saja dan adaptasikan ke kontrak backend: kunjungan rujukan menjadi `External`; kelahiran di RS menjadi `HospitalBirth`. Status admisi ulang ditentukan terpisah dari asal kedatangan.

---

### ISS-EPS-022-04 — Makna “Kembali Dirawat Setelah Pulang” Tidak Jelas

**Apa yang terjadi.** Label saat ini tidak menyebut apakah episode lama dibuka kembali, apakah bayi harus sudah memiliki RM, atau kapan pilihan boleh digunakan.

**Rekomendasi konten.** Ubah menjadi:

> **Admisi ulang bayi setelah pernah pulang**  
> Gunakan bila bayi sudah memiliki No. RM dan episode rawat inap sebelumnya sudah ditutup. Sistem membuat episode baru; episode lama tidak dibuka kembali.

Pilihan ini sebaiknya hanya muncul untuk pasien bayi lama yang mempunyai episode selesai. Bila riwayat tersebut dapat dipastikan server, sistem langsung menurunkannya dari data episode dan tidak perlu memberi pilihan bebas kepada pengguna.

---

### ISS-EPS-022-05 — Tampilan Identitas Ibu dan Bayi Terfragmentasi

**Apa yang terjadi.** Pemilih ibu, tombol daftar ibu, keterangan, kartu verifikasi, kartu `Informasi Utama`, dan formulir bayi tersusun sebagai blok-blok terpisah. Informasi ibu ditampilkan ulang, sementara hubungan visual dengan data bayi kurang kuat.

**Bukti source.** `inpatient-newborn-admission-steps.jsx:71-142` membentuk seluruh layar identitas dengan tombol daftar ibu penuh, kartu detail umum, lalu grid form bayi.

**Rekomendasi.** Susun dua bagian responsif:

1. **Ibu bayi** — pencarian ibu dan aksi sekunder `Daftarkan Ibu` dalam satu baris; hasil pilihan ditampilkan sebagai baris identitas ringkas berisi avatar/inisial, nama, RM tersamar, dan badge verifikasi.
2. **Identitas bayi** — kelompokkan nama, tanggal/jam lahir, jenis kelamin, tempat lahir/NIK, kelahiran tunggal/kembar, serta alamat/kontak. Gunakan pesan validasi dekat field dan satu kolom pada layar kecil.

Tidak diperlukan base component baru; gunakan `ResourceFilterSelect`, `BaseFormControl`, `BaseButton`, badge status, dan alert informasi yang sudah tersedia.

---

### ISS-EPS-022-06 — Deposit Bayi Membuat Pola Baru

**Apa yang terjadi.** `NewbornDepositStep` hanya menyediakan nominal dan dua banner, sedangkan komponen admisi umum sudah menangani policy loading/error, format rupiah, status kekurangan/pemenuhan, dan aksi pembatalan.

**Bukti source:**

- `inpatient-newborn-admission-steps.jsx:182-191` berisi implementasi deposit khusus bayi.
- `inpatient-admission-deposit-step.jsx:21-166` merupakan komponen deposit admisi umum yang lebih lengkap dan telah dipakai alur utama.

**Keputusan final.** Rekomendasi reuse ini superseded oleh `RWI-BAYI-022-2.0.0`: langkah dan input deposit bayi dihapus. Konfirmasi hanya menjelaskan bahwa bayi tidak membutuhkan deposit tambahan dan alokasi deposit ibu merupakan ranah Kasir/AR.

---

### ISS-EPS-022-07 — Bed Tersedia Dapat Mengarah ke Service Unit yang Ditolak Registrasi

**Apa yang terjadi.** Pengguna dapat memilih bed, tetapi POST `/api/v1/health-services/registration-management/patient-encounters/admin` ditolak dengan HTTP 400 karena unit layanan tidak memenuhi syarat registrasi.

**Akar penyebab terverifikasi.** Query ketersediaan bed tidak menggunakan seluruh syarat kelayakan yang digunakan endpoint pembentukan kunjungan.

**Bukti frontend pada SHA `4d9bf608...`:**

| Lokasi | Bukti |
| :--- | :--- |
| `inpatient-admission-view.jsx:754-778` | Mode bayi menginisialisasi board bed dengan `serviceUnitId` kosong. |
| `use-inpatient-admission-doctor.jsx:666-688` | Saat submit, unit diambil dari `roomContext.serviceUnitId` milik bed terpilih. |
| `inpatient-admission-encounter-utils.jsx:232-250` | Nilai tersebut dikirim sebagai `serviceUnitId` pada payload encounter. |
| `inpatient-admission-encounter-utils.jsx:284-305` | Validasi lokal hanya memastikan ID bed/unit/kelas tersedia, tidak memeriksa kesiapan registrasi unit. |
| `inpatient-admission-encounter.service.js:177-190` dan `use-inpatient-admission-doctor.jsx:699-704` | Pesan backend diteruskan ke UI tanpa pemulihan pilihan bed. |

**Bukti backend pada SHA `ca09cae...`:**

| Lokasi | Bukti |
| :--- | :--- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:146-191` | Query bed hanya memeriksa bed aktif/tidak dihapus dan scope unit; belum mensyaratkan unit aktif, tidak dihapus, tersedia untuk registrasi, dan bertipe rawat inap. |
| `InpBedOccupancyService.cs:206-235` | Perhitungan eligibility juga belum memakai predikat kesiapan registrasi yang sama. |
| `InpBedOccupancyService.cs:256-277` | Response mengembalikan `ServiceUnitId` dari ruangan. |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:1800-1822` | Endpoint encounter secara benar menolak unit yang tidak aktif/dihapus/tidak tersedia dan bukan tipe rawat inap. |

**Dampak.** Admisi berhenti setelah pengguna sudah memilih tempat tidur. Pesan teknis tidak memberi pilihan tindakan yang jelas, dan pengguna berpotensi mengulang input atau memilih bed yang sama.

**Rekomendasi.** Buat satu predikat backend yang menjadi sumber kebenaran: unit aktif, tidak dihapus, `IsAvailableForRegistration = true`, dan tipe `Inpatient`. Terapkan predikat yang sama pada daftar/eligibility bed dan pembentukan encounter. Validasi tulis tetap dipertahankan untuk menangani perubahan status setelah daftar dimuat. Jika race condition terjadi, frontend memperbarui daftar bed, mempertahankan input lain, dan menampilkan pesan:

> Tempat tidur ini tidak lagi dapat dipakai karena unit layanannya belum tersedia untuk registrasi. Daftar tempat tidur sudah diperbarui; pilih tempat lain atau minta admin mengaktifkan unit tersebut.

Status flag unit spesifik pada lampiran belum dapat dipastikan karena payload request dan record master unit tidak disertakan. Hal ini tidak membatalkan temuan kontrak yang berbeda, tetapi diperlukan untuk memastikan apakah unit pelapor gagal karena nonaktif, terhapus, tidak tersedia untuk registrasi, atau tipe unit salah.

---

### ISS-EPS-022-08 — Tombol Pembatalan Belum Terpasang pada Langkah Bayi

**Apa yang terjadi.** Handler reset lokal dan pembatalan episode sudah ada pada induk alur, tetapi komponen langkah bayi umumnya hanya menerima aksi kembali/lanjut. Akibatnya, petugas tidak memiliki jalan keluar resmi yang setara dengan admisi umum.

**Bukti source dan desain:**

- `inpatient-admission-view.jsx:982-1069` sudah mempunyai handler reset lokal dan pembatalan episode.
- `inpatient-admission-view.jsx:1259-1332` sudah mempunyai modal pembatalan.
- Cabang newborn pada `AdmissionStep` (`inpatient-admission-view.jsx:337-351`) belum meneruskan aksi reset/pembatalan ke seluruh langkah bayi.
- Helper `Actions` pada `inpatient-newborn-admission-steps.jsx:28-33` hanya menampilkan **Kembali** dan **Lanjut**.
- `plan-repair-011-tombol-batalkan-admisi.md:164-172` telah menetapkan pembedaan reset lokal dan pembatalan resmi.
- `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs:560-660` mengizinkan petugas admisi membatalkan episode `Draft`; episode `Admitted` mengikuti kewenangan supervisor/kepala ruangan.

**Penempatan yang ditetapkan:**

| Langkah admisi bayi | Kondisi data | Tombol | Perilaku |
| :--- | :--- | :--- | :--- |
| 1 Konteks, 2 Identitas, 3 Perawatan/DPJP, 4 Penjamin, 5 Deposit | Episode belum ada | **Batal Pendaftaran** | Konfirmasi sederhana, bersihkan form/session lokal, kembali ke awal. Tidak memanggil API cancel. |
| 6 Penempatan dan Pemesanan — sebelum submit berhasil | Episode belum ada | **Batal Pendaftaran** | Sama seperti langkah 1–5. |
| 6 Penempatan dan Pemesanan — setelah episode `Draft`/reservasi terbentuk | Episode dan/atau reservasi sudah ada | **Batalkan Admisi** | Alasan wajib, panggil endpoint cancel, lepaskan bed, simpan audit, lalu reset alur. |
| 7 Persetujuan Wali | Episode `Draft` | **Batalkan Admisi** | Pembatalan resmi di server. |
| 8 Tinjau dan Aktifkan | Episode `Draft` | **Batalkan Admisi** | Pembatalan resmi di server. |
| 9 Dokumen Bayi | Episode sudah aktif | Tidak ada pembatalan pendaftaran lokal | Pembatalan mengikuti aksi berwenang pada Daftar Kerja/Detail Episode. |
| Resume episode `Draft` | Episode sudah ada sejak layar dibuka | **Batalkan Admisi** pada setiap langkah resume | Tidak boleh melakukan reset lokal yang meninggalkan episode/bed. |

---

## 5. Jawaban Langsung kepada Pelapor

1. **Mengapa langsung ke Langkah 4?** Karena pergantian daftar langkah terjadi ketika pilihan bayi diklik, sebelum navigasi cabang selesai. Fallback menemukan `payment` sebagai slug bersama pertama. Solusinya adalah transisi atomik ke `newborn-context` ketika tombol Lanjut ditekan.
2. **Apakah kartu bayi–ibu bisa dihilangkan?** Ya. Kartu global dihapus dan ringkasan hanya tampil pada Langkah 8, yaitu halaman Tinjau dan Aktifkan.
3. **Mengapa masih ada luar RS/rujukan?** Itu duplikasi desain. Nilainya harus diturunkan dari jenis kunjungan awal dan tidak ditanyakan ulang.
4. **Apa arti kembali dirawat?** Bayi sudah memiliki RM, episode lama sudah selesai/pulang, dan sistem membuat episode baru. Istilahnya akan diubah menjadi `Admisi ulang bayi setelah pernah pulang` dengan penjelasan tersebut.
5. **Bagaimana deposit?** Komponen admisi umum digunakan ulang; alur bayi hanya mengubah label tujuan berikutnya bila perlu.
6. **Mengapa bed gagal?** Bed list dan API encounter memakai aturan kelayakan unit yang tidak sama. Daftar bed harus menyaring unit dengan aturan yang sama dengan endpoint registrasi.
7. **Di langkah mana tombol pembatalan muncul?** Langkah 1–5 dan Langkah 6 sebelum episode memakai `Batal Pendaftaran`; setelah episode terbentuk sampai Langkah 8 memakai `Batalkan Admisi`; Langkah 9 tidak menyediakan reset lokal.

---

## 6. Keputusan dan Informasi yang Masih Dibutuhkan

| ID | Kebutuhan | Rekomendasi | Dampak |
| :--- | :--- | :--- | :--- |
| `K-01` | Cara memastikan admisi ulang bayi | Tampilkan/aktifkan hanya untuk pasien bayi lama yang mempunyai episode rawat inap sebelumnya berstatus selesai; idealnya diturunkan dari riwayat server. | Menahan detail implementasi `FIX-EPS-022-03`, tidak menahan perbaikan lain. |
| `P-01` | Payload request gagal dan record master `serviceUnitId` pada kejadian lampiran | Ambil `serviceUnitId`, status aktif, status hapus, `IsAvailableForRegistration`, dan tipe unit tanpa data pasien. | Hanya untuk memastikan flag data mana yang memicu kejadian spesifik; perbaikan kontrak tetap dapat dikerjakan. |

Keputusan final pelapor: ringkasan bayi–ibu hanya pada konfirmasi; rujukan tidak ditanyakan dua kali; tidak ada input deposit bayi; dan perilaku tombol pembatalan existing tidak diubah.

---

## 7. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-15 | Issue dibuat dari tujuh keluhan, enam lampiran, dan tambahan permintaan tombol pembatalan; akar masalah ditelusuri sampai source frontend/backend serta rancangan sebelumnya. | Codex |

