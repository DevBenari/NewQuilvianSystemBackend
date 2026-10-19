# Planning Perbaikan Admisi Bayi Baru Lahir

```yaml
plan_id: PLAN-RWI-ADMISI-BAYI-2026-10-16
module_id: rawat-inap
submodule: episode-rawat-inap
status_rencana: IMPLEMENTASI_SOURCE_SELESAI_VERIFIKASI_RUNTIME_TERTUNDA
tanggal_rencana: "2026-10-16"
sumber_kebutuhan:
  - "docs/module-blueprints/rawat-inap/revisi/Revisi-Rawat-Inap.md"
  - "Klarifikasi pemilik pada 16 Oktober 2026"
basis_source_backend: "ca09cae909597442385ce4513be9c803549be10e (MHamzah) + working tree belum dikomit"
basis_source_frontend: "4d9bf608dbbaddc96a2b4acfc8723753b5dd5028 (HamzahV2) + working tree belum dikomit"
target_alur: "7 langkah khusus bayi baru lahir"
implementasi_source: "SELESAI 2026-10-16; migration tersedia tetapi belum diterapkan"
```

> Register di bawah sudah diperbarui setelah implementasi. Source dan migration telah dibuat;
> database belum dimutasi dan pengujian live browser belum dijalankan.

## 1. Register Status Pekerjaan

| ID pekerjaan | Ringkasan | Area | Prioritas | Status | Bukti |
| --- | --- | --- | :---: | --- | --- |
| `PKJ-AB-01` | Selaraskan blueprint dan kontrak Admisi Bayi | Dokumen | 1 | SELESAI | `RWI-BAYI-022-2.0.0` dan amandemen blueprint |
| `PKJ-AB-02` | Sediakan daftar calon ibu dari kasus operasi persalinan | Backend | 1 | SELESAI SOURCE | `BE-RWI-244` |
| `PKJ-AB-03` | Buat identitas bayi dan nomor urut kelahiran otomatis | Backend + database | 1 | SELESAI SOURCE/MIGRATION | `BE-RWI-245`; migration belum diterapkan |
| `PKJ-AB-04` | Wariskan penjamin ibu ke encounter bayi secara aman | Backend + database | 1 | SELESAI SOURCE/MIGRATION | `BE-RWI-246`; migration belum diterapkan |
| `PKJ-AB-05` | Sederhanakan kontrak episode bayi tanpa input penagihan dan deposit | Backend | 2 | SELESAI SOURCE | `BE-RWI-247` |
| `PKJ-AB-06` | Ubah stepper bayi dari 9 menjadi 7 langkah | Frontend | 2 | SELESAI SOURCE | `FE-RWI-257` |
| `PKJ-AB-07` | Bangun layar pemilihan ibu dan identitas khusus bayi | Frontend | 2 | SELESAI SOURCE | `FE-RWI-257`, `FE-RWI-258` |
| `PKJ-AB-08` | Tampilkan ringkasan penjamin/deposit hanya pada konfirmasi | Frontend + integrasi | 3 | SELESAI SOURCE | `FE-RWI-259` |
| `PKJ-AB-09` | Pastikan hanya bed bayi dengan service unit valid yang dapat dipilih | Backend + frontend | 2 | SELESAI SOURCE | `BE-RWI-247`, `FE-RWI-259` |
| `PKJ-AB-10` | Tambahkan test otomatis, regresi, dan pengujian live browser | Backend + frontend + QA | 3 | SEBAGIAN | Build/lint/targeted unit lulus; live pending migration |
| `DEP-BIL-01` | Rencana terpisah Kasir/AR untuk pemakaian deposit ibu pada invoice bayi | Billing/Kasir/AR | 3 | BELUM DIKERJAKAN — di luar implementasi Admisi | — |

**Ringkasan: 9 dari 10 pekerjaan selesai pada level source; validasi QA live pada `PKJ-AB-10`
menunggu migration diterapkan. Dependency Billing/Kasir/AR tetap di luar scope Admisi.**

## 2. Tujuan dan Batas Scope

### 2.1 Tujuan

Menyediakan alur pendaftaran bayi baru lahir yang singkat, jelas, dan tidak meminta petugas
mengisi ulang data keuangan yang sudah dimiliki ibu. Bayi tetap memperoleh identitas pasien,
nomor rekam medis, encounter, episode rawat inap, bed, layanan, dan invoice sendiri. Hubungan
ibu–bayi disimpan agar penjamin dan deposit ibu dapat dipakai sebagai sumber informasi oleh
Billing, Kasir, dan Account Receivable (AR).

### 2.2 Termasuk dalam scope

1. Daftar ibu berasal dari pasien dengan kasus operasi persalinan atau bedah obstetri.
2. Ruangan ibu tidak harus ruangan operasi.
3. Identitas bayi disesuaikan untuk bayi baru lahir dan tidak memakai form pasien umum penuh.
4. Nama sementara bayi diturunkan dari nama ibu.
5. Urutan bayi ditentukan backend secara otomatis dan aman saat dua petugas bekerja bersamaan.
6. Penjamin encounter bayi mengikuti penjamin encounter ibu.
7. Tahap pemilihan metode pembayaran dan deposit dihapus dari stepper Admisi Bayi.
8. Deposit tunai bayi mengacu pada deposit ibu; Admisi tidak menerima atau mengalokasikan uang.
9. Bayi tetap mendapat bed khusus bayi dan service unit yang sah untuk registrasi rawat inap.
10. Ringkasan ibu, bayi, penjamin, dan referensi deposit tampil lengkap pada langkah konfirmasi.

### 2.3 Di luar scope

1. Tombol **Batalkan Pendaftaran** dan **Batalkan Admisi** tidak dibuat, dihapus, atau diubah.
   Perilaku yang sudah ada harus tetap berjalan seperti sebelum pekerjaan ini.
2. Pembuatan order operasi persalinan tidak dikerjakan oleh Admisi Bayi.
3. Penggabungan invoice ibu dan bayi tidak dikerjakan oleh Admisi.
4. Penerimaan uang, alokasi deposit, settlement, refund, dan piutang tetap menjadi kewenangan
   Billing, Kasir, dan AR.
5. Penerapan migration ke database tidak dilakukan otomatis.
6. Admisi ulang bayi yang sudah pernah pulang tidak menjadi bagian alur bayi baru lahir. Pasien
   tersebut masuk melalui alur pasien lama biasa dengan episode baru.

## 3. Keputusan Bisnis yang Sudah Dikunci

| No. | Keputusan | Ketentuan final |
| :---: | --- | --- |
| 1 | Data yang dipilih | Pengguna memilih **ibu**, bukan bayi. |
| 2 | Sumber ibu | Ibu mempunyai kasus operasi dengan `SurgicalServiceType = Obstetric`. Lokasi kamar ibu tidak dipakai sebagai syarat kasus operasi. |
| 3 | Status kasus yang diperbolehkan | Hanya `InProgress` dan `Completed`. `Requested`, `Scheduled`, `Ready`, `Postponed`, `Cancelled`, dan `Rejected` tidak ditampilkan. |
| 4 | Identitas bayi | Nama sementara dari nama ibu; tanggal lahir, jam lahir, dan jenis kelamin bayi diisi pada form khusus bayi. |
| 5 | Urutan kelahiran | Bayi pertama pada satu kasus persalinan mendapat urutan 1. Bayi berikutnya pada ibu dan kasus persalinan yang sama mendapat urutan berikutnya. |
| 6 | Penjamin | Penjamin encounter bayi mengikuti snapshot penjamin encounter ibu, tanpa pilihan ulang di Admisi. |
| 7 | Invoice | Bayi tetap mempunyai invoice sendiri. Pembentukan, pengelompokan, dan pembayaran invoice adalah ranah Kasir/AR. |
| 8 | Deposit | Tidak ada deposit tambahan untuk bayi. Jika tunai, Kasir dapat memakai deposit ibu berdasarkan hubungan episode ibu–bayi. |
| 9 | Informasi ibu dan bayi | Tidak ada kartu ringkasan besar yang berulang pada setiap langkah. Ringkasan lengkap hanya pada langkah konfirmasi. |
| 10 | Pembatalan | Tidak termasuk scope planning dan implementasi ini. |

### Contoh urutan bayi

Ibu `Pasien Contoh A` mempunyai kasus persalinan `OPR-DEMO-001`.

1. Pendaftaran pertama menghasilkan `BirthOrder = 1`.
2. Pendaftaran berikutnya untuk `OPR-DEMO-001` menghasilkan `BirthOrder = 2`.
3. Dua permintaan yang terkirim hampir bersamaan tidak boleh sama-sama menghasilkan urutan 2.
4. Bila ibu yang sama mempunyai persalinan lain pada masa mendatang dengan kasus
   `OPR-DEMO-099`, urutannya dimulai kembali dari 1 karena lingkupnya adalah satu kasus
   persalinan, bukan seluruh riwayat hidup ibu.

## 4. Kondisi Source Saat Ini dan Gap

Label status pada tabel berikut memakai taxonomy capability Quilvian.

| Kemampuan | Kondisi saat ini | Status | Arah perbaikan |
| --- | --- | --- | --- |
| Stepper bayi | Masih mempunyai 9 langkah, termasuk `payment` dan `deposit`, pada `inpatient-newborn-admission-constants.js:1-11`. | `CONFLICT` | Jadikan 7 langkah dan hapus dua tahap finansial dari cabang bayi. |
| Daftar ibu | Hook masih mencari seluruh pasien perempuan melalui `searchInpatientAdmissionPatients` pada `use-inpatient-newborn-admission.js:91-103`. | `EXTEND` | Gunakan projection khusus calon ibu dari kasus obstetri. |
| Verifikasi episode ibu | Endpoint `active-mothers` hanya memeriksa perempuan dengan episode `Admitted` pada `InpEpisodeService.Reads.cs:461-503`. | `REUSE WITH ADAPTER` | Pertahankan untuk validasi rawat gabung, tetapi jangan pakai sebagai sumber tunggal calon ibu. |
| Data kasus obstetri | `OprCase` sudah menyimpan `SurgicalServiceType`, tetapi query daftar belum mempunyai filter service type pada `OperatingRoomCaseDtos.cs:6-16`. | `EXTEND` | Buat projection Admisi yang memfilter obstetri dan status yang disetujui. |
| Hubungan ibu–bayi | `MstPatient` sudah mempunyai `IsNewborn`, `MotherPatientId`, dan `BirthOrder` pada `MstPatient.cs:98-102`. | `REUSE WITH ADAPTER` | Tambahkan sumber kasus persalinan dan alokasi urutan otomatis. |
| Urutan kembar | Frontend meminta petugas mengisi `birthOrder` pada `inpatient-newborn-admission-steps.jsx:155-157`; backend menyalin request pada `PatientController.cs:580-585`. | `CONFLICT` | Hapus input manual dan jadikan backend sumber kebenaran. |
| Nama bayi | Utility saat ini menghasilkan `Bayi [Nama Ibu]` pada `inpatient-newborn-admission-utils.js:8-11`. | `REPAIR` | Gunakan format sementara yang disetujui: `By. Ny. [Nama Ibu]`. |
| Penjamin bayi | Encounter saat ini memerlukan kartu asuransi/perusahaan milik pasien yang sama pada `PatientEncounterController.cs:2193-2338`. | `CONFLICT` | Tambahkan mode penjamin turunan dari encounter ibu beserta provenance dan snapshot audit. |
| Rencana penagihan | UI masih meminta pilihan invoice terpisah/gabungan pada `inpatient-newborn-admission-steps.jsx:204-212`. | `MISSING` di Kasir, `CONFLICT` di Admisi | Hilangkan input dari Admisi; buat dependency terpisah untuk Kasir/AR. |
| Deposit bayi | Frontend masih mengirim `plannedDepositAmount` pada `inpatient-newborn-admission-utils.js:31-38`; episode menyimpannya pada `InpEpisode.cs:74-75`. | `CONFLICT` | Jangan minta atau mengirim nominal deposit bayi. Pertahankan kolom lama hanya untuk kompatibilitas data historis. |
| Ringkasan ibu–bayi | Source terbaru sudah memusatkan kartu detail pada konfirmasi `inpatient-newborn-admission-steps.jsx:226-247`. | `READY TO REUSE` | Pertahankan dan sesuaikan isi finansial menjadi baca-saja. |
| Bed bayi | Filter `isForNewborn` dan aturan bed bayi sudah tersedia. Predikat service unit bersama terlihat pada `InpBedOccupancyService.cs:149` dan `PatientEncounterController.cs:1731,1818`. | `READY TO REUSE` dengan verifikasi runtime | Jangan buat jalur baru; tambah regresi dan pesan pemulihan bila unit berubah. |
| Gelang bayi | Format ibu dan urutan sudah ada pada `InpWristbandRules.cs:110-130`. | `REUSE WITH ADAPTER` | Ubah format kembar dan tampilkan penanda hanya bila terdapat lebih dari satu bayi pada kasus yang sama. |

## 5. Target Alur Pengguna

### 5.1 Alur 7 langkah

| Langkah | Nama | Pekerjaan petugas | Data yang disimpan |
| :---: | --- | --- | --- |
| 1 | Pilih Ibu Persalinan | Cari dan pilih ibu dari daftar kasus operasi persalinan yang memenuhi syarat. | `motherPatientId`, `motherEncounterId`, `motherEpisodeId` bila ada, dan `deliveryCaseId`. |
| 2 | Identitas Bayi | Isi tanggal lahir, jam lahir, jenis kelamin, serta data bayi yang memang belum dapat diturunkan. | Pasien bayi, No. RM, hubungan ibu, sumber persalinan, dan urutan kelahiran otomatis. |
| 3 | Perawatan dan DPJP | Pilih rawat gabung/neonatal dan DPJP bayi. | Konteks perawatan serta penugasan DPJP. |
| 4 | Penempatan dan Pemesanan | Pilih hanya bed bayi dari service unit rawat inap yang valid. | Encounter bayi, episode `Draft`, dan reservasi bed. Penjamin diwariskan server pada tahap pembentukan encounter. |
| 5 | Persetujuan Wali | Isi dan tanda tangani persetujuan oleh orang tua/wali. | Dokumen persetujuan bayi. |
| 6 | Tinjau dan Aktifkan | Periksa seluruh data; penjamin dan referensi deposit hanya ditampilkan sebagai informasi. | Episode berubah dari `Draft` menjadi `Admitted` dan penempatan diaktifkan. |
| 7 | Dokumen Bayi | Cetak gelang, label, dan dokumen yang tersedia. | Log cetak; tidak membuat admisi baru. |

### 5.2 Skema sebelum

```text
[1 Konteks]
  → [2 Identitas]
  → [3 Perawatan dan DPJP]
  → [4 Penjamin dan Penagihan]
  → [5 Rencana Deposit]
  → [6 Penempatan]
  → [7 Persetujuan Wali]
  → [8 Tinjau dan Aktifkan]
  → [9 Dokumen Bayi]
```

Masalah utama bentuk lama adalah petugas diminta mengambil keputusan keuangan di Admisi,
padahal penjamin dan deposit bayi sudah ditentukan dari ibu dan penyelesaian invoice menjadi
kewenangan Kasir/AR.

### 5.3 Skema sesudah

```text
[1 Pilih Ibu Persalinan]
  → [2 Identitas Bayi]
  → [3 Perawatan dan DPJP]
  → [4 Penempatan dan Pemesanan]
  → [5 Persetujuan Wali]
  → [6 Tinjau dan Aktifkan]
  → [7 Dokumen Bayi]
```

Tidak ada kartu ringkasan global di Langkah 1–5 dan 7. Langkah 6 menampilkan ringkasan lengkap
ibu, bayi, kasus persalinan, DPJP, bed, penjamin turunan, dan informasi bahwa deposit tunai
mengikuti ibu.

## 6. Proses Bisnis Target

### 6.1 Pelaku

| Pelaku | Tanggung jawab |
| --- | --- |
| Petugas Admisi/PPRI | Memilih ibu, mencatat identitas bayi, memilih DPJP dan bed, memeriksa persetujuan, lalu mengaktifkan admisi. |
| Tenaga klinis | Menetapkan kebutuhan rawat gabung, neonatal, intensif, dan DPJP. |
| Sistem Patient Management | Membentuk identitas bayi dan No. RM tersendiri. |
| Sistem Registration Management | Membentuk encounter bayi dan snapshot penjamin turunan. |
| Sistem Rawat Inap | Membentuk episode, reservasi, penempatan, persetujuan, serta event layanan ke Billing. |
| Billing/Kasir/AR | Membentuk invoice bayi, menerima pembayaran, memakai saldo deposit ibu bila tunai, dan mengelola piutang. |

### 6.2 Pemicu dan prasyarat

Proses dimulai setelah bayi lahir dan ibu mempunyai kasus operasi persalinan obstetri berstatus
`InProgress` atau `Completed`.

Prasyarat:

1. Ibu adalah pasien aktif dan tidak dihapus.
2. Kasus operasi adalah milik ibu yang dipilih.
3. `SurgicalServiceType` kasus adalah `Obstetric`.
4. Kasus tidak berstatus dibatalkan, ditolak, ditunda, atau belum memasuki pelaksanaan.
5. Encounter ibu dan snapshot penjamin ibu dapat dibaca.
6. Untuk rawat gabung, ibu mempunyai episode dan penempatan rawat inap aktif.
7. Untuk neonatal terpisah, ketiadaan penempatan aktif ibu tidak otomatis menolak identitas bayi.

### 6.3 Langkah utama

1. Backend memuat calon ibu dari kasus operasi persalinan yang memenuhi syarat, diurutkan dari
   kasus terbaru dan mendukung pencarian nama, No. RM, serta nomor kasus.
2. Petugas memilih satu ibu. Sistem menampilkan informasi ringkas kasus, status, lokasi ibu saat
   ini, penjamin, dan jumlah bayi yang sudah terdaftar pada kasus tersebut.
3. Petugas mengisi tanggal lahir, jam lahir, dan jenis kelamin bayi. Nama sementara otomatis
   menggunakan `By. Ny. [Nama Ibu]` dan tidak diedit pada alur ini.
4. Backend memvalidasi kembali kasus persalinan, mengalokasikan urutan kelahiran secara atomik,
   membuat pasien bayi, serta menghasilkan No. RM.
5. Petugas memilih konteks perawatan dan DPJP bayi.
6. Petugas memilih bed bayi. Backend memvalidasi ulang bed dan service unit ketika disimpan.
7. Backend membuat encounter bayi dan menyalin snapshot penjamin aktif dari encounter ibu.
8. Backend membuat episode bayi `Draft`, menautkannya ke episode ibu bila tersedia, lalu
   mereservasi bed.
9. Orang tua atau wali menandatangani persetujuan.
10. Petugas meninjau ringkasan akhir dan mengaktifkan admisi.
11. Rawat Inap mengirim event layanan milik episode bayi ke Billing. Nominal deposit tidak
    dikirim dari form Admisi.
12. Billing membentuk invoice bayi. Kasir/AR menangani penggunaan saldo deposit ibu bila
    penjamin tunai.

### 6.4 Perubahan status

| Data | Dari | Tindakan | Menjadi | Syarat |
| --- | --- | --- | --- | --- |
| Kasus persalinan | `InProgress`/`Completed` | Dipilih sebagai sumber kelahiran | Tidak berubah | Kasus obstetri dan milik ibu. |
| Pasien bayi | Belum ada | Simpan identitas bayi | Aktif dengan No. RM | Identitas minimum valid dan alokasi urutan berhasil. |
| Episode bayi | Belum ada | Simpan penempatan | `Draft` | Encounter, DPJP, dan bed valid. |
| Reservasi bed | Belum ada | Pesan bed bayi | Aktif | Bed masih tersedia dan service unit layak registrasi. |
| Episode bayi | `Draft` | Aktifkan setelah persetujuan | `Admitted` | Persetujuan wali valid dan reservasi masih sah. |
| Invoice bayi | Belum ada | Billing menerima event | Terbuka sesuai workflow Billing | Event idempotent diterima Billing. |

### 6.5 Jalur tidak normal

| Kondisi | Perilaku yang diharapkan |
| --- | --- |
| Kasus berubah menjadi `Cancelled` setelah daftar dimuat | Penyimpanan ditolak dengan pesan bahwa kasus persalinan tidak lagi dapat dipakai; input identitas belum dikirim ulang otomatis. |
| Dua petugas mendaftarkan bayi bersamaan | Backend memberi nomor urut berbeda dan tidak membuat dua respons untuk idempotency key yang sama. |
| Penjamin ibu tidak tersedia atau tidak aktif | Admisi berhenti sebelum encounter bayi dibuat dan mengarahkan petugas memeriksa penjamin ibu; petugas tidak memilih penjamin pengganti dari alur bayi. |
| Bed berubah tidak tersedia | Hanya pilihan bed dibersihkan, daftar dimuat ulang, dan data ibu/bayi/DPJP tetap ada. |
| Ibu tidak mempunyai penempatan aktif | Rawat gabung tidak dapat dipilih; neonatal terpisah masih dapat diproses bila aturan klinis mengizinkan. |
| Deposit tunai ibu nol atau tidak dapat dibaca | Admisi bayi tidak meminta deposit baru. Kasir/AR menerima status tersebut sebagai tindak lanjut finansial. |
| Bayi dari kasus yang sama sudah terdaftar | Daftar ibu tetap boleh dipilih untuk bayi kembar berikutnya; UI menampilkan jumlah bayi yang sudah ada dan urutan berikutnya sebagai perkiraan. |

## 7. Aturan Data dan Validasi

### 7.1 Calon ibu

Filter server wajib:

```text
Patient aktif
AND OprCase tidak dihapus
AND OprCase.SurgicalServiceType = Obstetric
AND OprCase.Status IN (InProgress, Completed)
```

Ruangan ibu tidak menjadi filter. Lokasi saat ini hanya ditampilkan sebagai informasi. Hasil
dipaginasi dan diurutkan dari kasus terbaru; tidak memakai batas waktu tersembunyi agar kasus
yang sah tidak hilang. Pengguna dapat menyaring berdasarkan tanggal bila daftar terlalu panjang.

### 7.2 Identitas minimum bayi

| Field | Sumber | Wajib | Aturan |
| --- | --- | :---: | --- |
| `MotherPatientId` | Calon ibu terpilih | Ya | Tidak dapat diketik manual sebagai UUID. |
| `DeliveryCaseId` | Kasus obstetri terpilih | Ya | Harus milik ibu dan berstatus layak. |
| `FullName` | Sistem | Ya | `By. Ny. [Nama Ibu]`; bukan input bebas pada alur ini. |
| `BirthDate` | Petugas | Ya | Tanggal valid dan tidak di masa depan. |
| `BirthTime` | Petugas | Ya | Jam valid sesuai zona waktu rumah sakit. |
| `Gender` | Petugas | Ya | Laki-laki atau perempuan. |
| `BirthOrder` | Backend | Ya | Dimulai dari 1 per `DeliveryCaseId`; tidak diterima dari browser. |
| `IdentityNumber`/NIK | — | Tidak | Tidak memakai NIK ibu. Dapat dilengkapi kemudian. |
| `MedicalRecordNumber` | Backend | Ya | Unik dan mengikuti allocator No. RM yang sudah ada. |

### 7.3 Alokasi urutan kelahiran

Solusi terpilih adalah allocator backend transaksional yang dilingkupi `DeliveryCaseId`.

Ketentuan teknis:

1. Jangan memakai `Count`, `Max`, `Last`, atau pola `nilai terbesar + 1` sebagai allocator.
2. Simpan state counter per kasus persalinan dan naikkan secara atomik dalam transaksi yang sama
   dengan pembuatan pasien bayi.
3. Tambahkan constraint unik untuk kombinasi sumber persalinan dan urutan bayi aktif.
4. Gunakan idempotency key agar pengiriman ulang request yang sama mengembalikan pasien yang
   sama, bukan mengonsumsi urutan berikutnya.
5. Nomor perkiraan pada UI bukan sumber kebenaran; respons backend adalah nilai final.
6. Jika transaksi gagal, pembuatan pasien dan kenaikan counter ikut dibatalkan.

### 7.4 Format nama dan gelang

1. Nama pasien sementara disimpan sebagai `By. Ny. [Nama Ibu]`.
2. Bayi pertama tetap memiliki `BirthOrder = 1` walaupun kelahiran tunggal.
3. Jika baru satu bayi tercatat pada kasus tersebut, gelang menampilkan
   `BY. NY. [NAMA IBU]` tanpa penanda kembar.
4. Jika terdapat dua atau lebih bayi, gelang menampilkan
   `BY. NY. [NAMA IBU] (KEMBAR 1)`, `(KEMBAR 2)`, dan seterusnya.
5. Perubahan nama resmi bayi dilakukan melalui workflow koreksi identitas pasien, bukan dengan
   mengubah hubungan persalinan atau urutan kelahiran.

### 7.5 Penjamin turunan

Penjamin bayi adalah snapshot turunan dari sumber pembayaran encounter ibu yang terkait kasus
persalinan. Browser tidak boleh menyalin sendiri seluruh field penjamin.

Aturan:

1. Backend memvalidasi bahwa encounter sumber benar-benar milik ibu terpilih.
2. Jenis penjamin, provider, masa berlaku, kelas, benefit, dan nomor referensi disalin sebagai
   snapshot audit pada encounter bayi.
3. Simpan provenance penjamin, misalnya referensi ke payment source encounter ibu.
4. Jangan memakai `PatientInsuranceId` atau `PatientCompanyGuarantorId` milik ibu seolah-olah
   kartu tersebut milik pasien bayi. Model harus mempunyai mode turunan yang eksplisit.
5. Jika bayi kemudian mempunyai kartu sendiri, perubahan dilakukan oleh workflow Billing atau
   koreksi penjamin yang berwenang; riwayat snapshot awal tidak dihapus.
6. Tunai tetap diwariskan sebagai jenis pembayaran tunai; Admisi tidak meminta metode pembayaran
   baru untuk bayi.

### 7.6 Deposit

1. Admisi bayi tidak mempunyai field nominal deposit.
2. `plannedDepositAmount` tidak dikirim untuk alur baru.
3. Bila penjamin ibu bukan tunai, informasi deposit tidak berlaku.
4. Bila penjamin ibu tunai, episode bayi menyimpan hubungan ke ibu; Billing/Kasir memakai
   hubungan tersebut untuk menemukan akun deposit ibu.
5. Admisi hanya mengirim event layanan/episode. Tidak ada mutasi saldo deposit oleh Admisi.
6. Invoice bayi tetap terpisah. Kasir/AR memilih dan mencatat alokasi saldo deposit ibu ke
   invoice bayi dengan audit yang sesuai.

## 8. Rencana Kontrak API

Endpoint berlabel **Rencana (belum tersedia)** belum boleh dianggap sudah ada pada Swagger.

### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/newborn-mother-candidates` | **Rencana (belum tersedia):** mengambil ibu dari kasus obstetri yang layak untuk pendaftaran bayi. | `InpatientEpisode : Read` | Query pencarian, status, tanggal, page, page size | `ApiResponse<PagedResult<NewbornMotherCandidateResponse>>` |
| `POST` | `/` | Existing: membuat episode bayi `Draft`; kontraknya disederhanakan agar tidak menerima keputusan invoice/deposit dari Admisi. | `InpatientEpisode : Create` | `OpenAdmissionRequest` dengan konteks bayi dan sumber kasus persalinan | `ApiResponse<InpatientEpisodeDetailResponse>` |
| `GET` | `/{id}` | Existing: membaca ulang hubungan ibu, sumber persalinan, perawatan, DPJP, dan status episode bayi. | `InpatientEpisode : Read` | `id` | `ApiResponse<InpatientEpisodeDetailResponse>` |

Respons calon ibu minimal memuat:

- identitas ibu yang aman ditampilkan;
- nomor kasus persalinan dan `deliveryCaseId`;
- status kasus dan waktu kasus;
- encounter ibu sumber penjamin;
- episode/ruang ibu saat ini bila tersedia;
- ringkasan jenis penjamin tanpa data rahasia;
- jumlah bayi yang sudah terdaftar pada kasus itu; dan
- urutan berikutnya sebagai preview, bukan nilai final.

Kode status:

- `200`: daftar berhasil dibaca, termasuk daftar kosong.
- `400`: query tidak valid.
- `403`: pengguna tidak mempunyai hak membaca kandidat Admisi Bayi.

### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/newborns` | **Rencana (belum tersedia):** membuat pasien bayi dari ibu dan kasus persalinan, sekaligus mengalokasikan urutan secara atomik. | `Patient : Create` | `CreateNewbornPatientRequest` + `Idempotency-Key` | `ApiResponse<PatientCreateResponse>` |

Request tidak menerima `BirthOrder` dari browser. Request memuat `MotherPatientId`,
`DeliveryCaseId`, `BirthDate`, `BirthTime`, `Gender`, dan data opsional bayi yang disetujui.

Kode status:

- `200` atau `201`: pasien bayi berhasil dibuat.
- `400`: format identitas tidak valid.
- `404`: ibu atau kasus persalinan tidak ditemukan.
- `409`: request duplikat dengan isi berbeda atau constraint urutan bertabrakan setelah retry.
- `422`: kasus bukan obstetri, status kasus tidak layak, atau kasus bukan milik ibu.

### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/admin` | Existing yang diperluas: membuat encounter bayi dengan mode penjamin turunan dari encounter ibu. | `PatientEncounter : Create` | `PatientEncounterCreateRequest` dengan referensi sumber penjamin ibu, bukan pilihan kartu bayi | `ApiResponse<PatientEncounterCreateResponse>` |

Kode status tambahan yang direncanakan:

- `409`: sumber penjamin berubah setelah kandidat dimuat; frontend meminta pengguna memuat ulang.
- `422`: pasien bukan bayi, hubungan ibu tidak cocok, atau encounter sumber bukan milik ibu.

## 9. Rincian Paket Pekerjaan

### PKJ-AB-01 — Selaraskan blueprint dan kontrak

**Langkah:**

1. Amendemen keputusan Admisi Bayi pada `00-interview-decisions.md`.
2. Revisi `05-skema-tampilan.md` menjadi alur 7 langkah.
3. Revisi `03-frontend-architecture.md` untuk state mother candidate dan penghapusan state
   payment/deposit dari cabang bayi.
4. Revisi `02-backend-architecture.md`, data dictionary, validation matrix, state transition,
   API contract, dan integration contract.
5. Tandai `FamilyBillingPreference` dan `PlannedDepositAmount` sebagai field historis/deprecated
   untuk alur baru; jangan langsung menghapus kolom yang masih dipakai data lama.

**Acceptance criteria:** seluruh dokumen menyatakan aturan yang sama dan tidak lagi menyebut
penjamin/deposit sebagai langkah input Admisi Bayi.

### PKJ-AB-02 — Daftar calon ibu dari kasus persalinan

**Langkah:**

1. Buat query projection khusus Admisi Bayi di boundary Rawat Inap.
2. Filter `Obstetric` dengan status `InProgress` dan `Completed`.
3. Gabungkan identitas ibu, kasus, encounter sumber, episode/penempatan saat ini, dan ringkasan
   penjamin.
4. Tambahkan pagination, pencarian, sort terbaru, dan pembatalan request.
5. Jangan mewajibkan current room bertipe Operating Room.

**Acceptance criteria:** ibu di ruang rawat inap non-operasi tetap muncul selama kasus
persalinannya memenuhi syarat; kasus `Cancelled` tidak muncul.

### PKJ-AB-03 — Identitas dan urutan bayi otomatis

**Langkah:**

1. Tambahkan sumber kasus persalinan pada data bayi sebagai logical reference yang diaudit.
2. Buat endpoint khusus pembuatan bayi.
3. Tambahkan allocator urutan per kasus persalinan yang transaksional dan idempotent.
4. Tambahkan constraint database untuk mencegah urutan ganda.
5. Hasilkan nama sementara dari nama ibu dan gunakan allocator No. RM existing.
6. Sesuaikan formatter gelang agar label kembar mengikuti jumlah bayi pada kasus yang sama.
7. Siapkan migration, Designer, dan model snapshot, tetapi jangan menerapkan migration.

**Acceptance criteria:** 20 request paralel untuk satu kasus tidak menghasilkan dua
`BirthOrder` yang sama; retry dengan idempotency key sama mengembalikan bayi yang sama.

### PKJ-AB-04 — Penjamin bayi mengikuti ibu

**Langkah:**

1. Tambahkan mode/provenance penjamin turunan pada kontrak encounter guarantor.
2. Buat service server-side untuk membaca snapshot penjamin encounter ibu.
3. Validasi hubungan kasus–ibu–bayi–encounter sumber.
4. Salin snapshot penjamin tanpa memalsukan kepemilikan kartu pasien.
5. Simpan jejak sumber agar audit dan koreksi dapat ditelusuri.
6. Tambahkan migration bila model guarantor memerlukan field provenance.

**Acceptance criteria:** encounter bayi tunai, asuransi, dan penjamin perusahaan masing-masing
mempunyai snapshot yang sama dengan ibu pada waktu admisi serta sumber pewarisan yang dapat
ditelusuri.

### PKJ-AB-05 — Sederhanakan kontrak episode bayi

**Langkah:**

1. Tambahkan `DeliveryCaseId` pada konteks bayi atau relasi sumber yang disetujui kontrak.
2. Hentikan penulisan baru `FamilyBillingPreference` dan `PlannedDepositAmount` dari Admisi.
3. Pertahankan pembacaan field historis agar episode lama tetap dapat dibuka.
4. Pastikan `MotherEpisodeId` tetap opsional untuk neonatal terpisah dan wajib untuk rawat
   gabung.
5. Pastikan event layanan Billing tetap berasal dari episode/encounter bayi sendiri.

**Acceptance criteria:** episode baru dapat dibuat tanpa field rencana penagihan/deposit dan
Billing tetap menerima event episode bayi.

### PKJ-AB-06 — Stepper 7 langkah

**Langkah:**

1. Ubah konstanta stepper menjadi tujuh langkah target.
2. Hapus render step `payment` dan `deposit` untuk mode bayi.
3. Ubah label tombol antar-langkah.
4. Sesuaikan validasi URL, browser back/forward, refresh, dan resume draft.
5. Jangan mengubah stepper pasien umum, pasien lama biasa, atau transfer.
6. Jangan mengubah tombol pembatalan yang sudah ada.

**Acceptance criteria:** memilih kategori bayi selalu masuk ke Langkah 1 dan tidak ada URL bayi
yang mengarahkan ke halaman payment/deposit.

### PKJ-AB-07 — Layar pemilihan ibu dan identitas bayi

**Langkah:**

1. Ganti pencarian seluruh pasien perempuan dengan service candidate baru.
2. Tampilkan nama ibu, No. RM tersamar, nomor/status kasus, lokasi saat ini, penjamin, dan jumlah
   bayi yang sudah terdaftar.
3. Hilangkan pilihan sumber luar RS/rujukan dan admisi ulang dari alur bayi baru lahir.
4. Jadikan nama bayi hasil sistem, bukan input bebas.
5. Wajibkan tanggal lahir, jam lahir, serta jenis kelamin.
6. Hapus pilihan tunggal/kembar dan input urutan manual.
7. Tampilkan urutan final setelah pasien bayi berhasil dibuat.

**Acceptance criteria:** petugas tidak pernah mengetik UUID, urutan kembar, penjamin, atau
deposit pada alur bayi.

### PKJ-AB-08 — Ringkasan final dan handoff finansial

**Langkah:**

1. Pertahankan ringkasan besar hanya pada langkah Tinjau dan Aktifkan.
2. Tampilkan penjamin bayi sebagai “Mengikuti penjamin ibu” beserta nama sumber yang aman.
3. Untuk tunai, tampilkan “Deposit mengikuti akun deposit ibu; alokasi dilakukan Kasir”.
4. Untuk non-tunai, tampilkan “Deposit tambahan bayi tidak diperlukan di Admisi”.
5. Hapus pilihan invoice terpisah/gabungan dari UI Admisi.
6. Dokumentasikan dependency `DEP-BIL-01` untuk Kasir/AR.

**Acceptance criteria:** tidak ada field finansial yang dapat diedit pada Admisi Bayi, tetapi
petugas dapat memverifikasi penjamin turunan sebelum aktivasi.

### PKJ-AB-09 — Bed dan service unit

**Langkah:**

1. Pertahankan query `isForNewborn=true`.
2. Pastikan daftar bed dan endpoint encounter memakai predikat service unit rawat inap yang
   sama.
3. Validasi ulang bed saat encounter/episode/reservasi dibuat.
4. Saat terjadi konflik, pertahankan data ibu, bayi, dan DPJP; bersihkan hanya bed lalu muat ulang.
5. Gunakan pesan ramah pengguna, bukan pesan teknis mentah.

**Acceptance criteria:** tidak ada bed non-bayi atau unit yang tidak tersedia untuk registrasi
yang dapat dipilih; perubahan status unit setelah daftar dimuat menghasilkan pemulihan yang
jelas tanpa kehilangan form lain.

### PKJ-AB-10 — Pengujian dan sign-off

**Langkah:**

1. Tambahkan unit test frontend untuk stepper, candidate mapping, identity, dan review.
2. Tambahkan test backend untuk eligibility ibu, allocator urutan, idempotency, penjamin turunan,
   dan validasi bed.
3. Jalankan regresi pasien umum, pasien lama, rujukan, transfer, dan resume draft.
4. Jalankan live browser dengan data uji non-pribadi.
5. Periksa request/response network dan data persistence yang dihasilkan.

**Acceptance criteria:** seluruh skenario pada Bagian 11 lulus dan hasil aktual ditautkan pada
laporan task masing-masing.

## 10. Urutan dan Dependency Pengerjaan

```text
PKJ-AB-01  Selaraskan keputusan dan kontrak
├── PKJ-AB-02  API calon ibu persalinan
├── PKJ-AB-03  Identitas + allocator urutan bayi
├── PKJ-AB-04  Penjamin turunan dari ibu
└── PKJ-AB-05  Kontrak episode tanpa input finansial
    ├── PKJ-AB-06  Stepper 7 langkah
    ├── PKJ-AB-07  UI calon ibu + identitas bayi
    └── PKJ-AB-08  Ringkasan final + handoff finansial

PKJ-AB-02 + PKJ-AB-03 + PKJ-AB-04 + PKJ-AB-05
└── PKJ-AB-09  Integrasi encounter, episode, dan bed
    └── PKJ-AB-10  Test menyeluruh dan sign-off

PKJ-AB-04 + PKJ-AB-05
└── DEP-BIL-01  Planning dan implementasi terpisah pada Billing/Kasir/AR
```

Urutan implementasi yang direkomendasikan adalah backend-first. Frontend baru menghapus step
payment/deposit setelah endpoint candidate, pembuatan bayi, dan penjamin turunan tersedia pada
kontrak yang disetujui.

## 11. Skenario Verifikasi

| No. | Skenario | Hasil yang diharapkan |
| :---: | --- | --- |
| 1 | Ibu mempunyai kasus `Requested` | Tidak muncul pada daftar calon ibu. |
| 2 | Ibu mempunyai kasus obstetri `InProgress` dan berada di ruang rawat inap biasa | Muncul dan dapat dipilih. |
| 3 | Ibu mempunyai kasus `Completed` | Muncul dan dapat dipilih untuk pendaftaran bayi berikutnya. |
| 4 | Kasus bedah umum, bukan obstetri | Tidak muncul. |
| 5 | Bayi pertama pada kasus | Backend memberi urutan 1. |
| 6 | Bayi kedua pada kasus yang sama | Backend memberi urutan 2 tanpa input petugas. |
| 7 | Dua petugas menyimpan bersamaan | Urutan unik; tidak ada bayi ganda dari request idempotent yang sama. |
| 8 | Ibu sama, kasus persalinan berbeda | Kasus baru dimulai dari urutan 1. |
| 9 | Penjamin ibu tunai | Encounter bayi tunai; tidak ada step deposit; konfirmasi menyebut deposit mengikuti ibu. |
| 10 | Penjamin ibu asuransi | Snapshot penjamin bayi diwariskan; tidak memakai kartu ibu sebagai kartu milik bayi. |
| 11 | Penjamin ibu perusahaan | Snapshot dan provenance tersimpan; validasi benefit tetap fail-safe. |
| 12 | Penjamin ibu berubah setelah daftar dimuat | Penyimpanan ditolak dengan pesan muat ulang; tidak membuat encounter parsial. |
| 13 | Bed non-bayi | Tidak tampil. |
| 14 | Bed bayi pada unit tidak aktif/tidak tersedia registrasi | Tidak tampil. |
| 15 | Unit menjadi tidak valid setelah dipilih | Bed di-refresh; data langkah lain tidak hilang. |
| 16 | Rawat gabung tanpa episode ibu aktif | Ditolak dengan penjelasan; neonatal terpisah tetap dapat dipilih bila valid. |
| 17 | Langkah 1–5 | Tidak ada kartu ringkasan ibu–bayi berulang. |
| 18 | Langkah 6 | Ringkasan ibu, bayi, urutan, DPJP, bed, penjamin, dan referensi deposit tampil baca-saja. |
| 19 | URL lama `step=payment` atau `step=deposit` pada mode bayi | Dialihkan ke langkah valid terdekat tanpa melompat atau crash. |
| 20 | Tombol pembatalan existing | Tetap berperilaku seperti baseline; tidak berubah akibat pekerjaan ini. |
| 21 | Admisi pasien umum | Step pembayaran dan deposit tetap tersedia sesuai alur umum. |
| 22 | Aktivasi bayi | Billing menerima event episode/encounter bayi, bukan invoice gabungan buatan Admisi. |

## 12. Perintah Validasi Saat Implementasi

### Backend

1. Jalankan targeted test untuk candidate, allocator, idempotency, guarantor inheritance, episode,
   dan bed eligibility.
2. Jalankan `dotnet build QuilvianSystemBackend.sln`.
3. Buat migration bila schema berubah, lalu periksa isi migration, Designer, dan model snapshot.
4. Jangan menjalankan `dotnet ef database update` tanpa instruksi terpisah.

### Frontend

1. Jalankan targeted unit test Admisi Bayi.
2. Jalankan `npm run lint:errors`.
3. Jalankan `npm run test:unit`.
4. Jalankan `npm run build`.

### Live browser

1. Jalankan alur bayi pertama dan bayi kedua dari ibu yang sama.
2. Periksa tampilan desktop dan mobile.
3. Periksa request network tidak mengirim `birthOrder`, pilihan penjamin, atau nominal deposit.
4. Periksa response memuat urutan final dan penjamin turunan.
5. Periksa kasus bed invalid menghasilkan pesan ramah dan form tidak terhapus.

## 13. Risiko dan Mitigasi

| Risiko | Dampak | Mitigasi |
| --- | --- | --- |
| Menyalin ID kartu penjamin ibu ke bayi | Kepemilikan kartu salah dan validasi encounter gagal | Gunakan snapshot turunan dengan provenance eksplisit; jangan memalsukan patient ownership. |
| Urutan kembar dihitung dengan `Max + 1` | Nomor ganda saat request bersamaan | Allocator atomik per `DeliveryCaseId`, constraint unik, dan idempotency. |
| Kasus `Completed` lama memenuhi filter | Daftar terlalu panjang | Pagination, pencarian, filter tanggal eksplisit, dan sort terbaru; jangan memakai cutoff tersembunyi. |
| Penghapusan step memutus resume draft lama | Draft lama tidak dapat dibuka | Adapter URL/state untuk slug lama dan pembacaan field historis. |
| Deposit ibu dipakai otomatis tanpa kewenangan Kasir | Mutasi finansial tanpa audit | Admisi hanya menyimpan hubungan; alokasi uang selalu dilakukan workflow Billing/Kasir. |
| Perubahan lintas Patient, Registration, Inpatient, Operating Room, dan Billing | Radius regresi besar | Pecah pekerjaan per boundary, backend-first, kontrak dikunci sebelum implementasi, lalu uji lintas modul. |
| Working tree saat ini sudah mempunyai perubahan lain | Perubahan pengguna tertimpa | Builder wajib membedakan baseline kotor, tidak mereset, dan hanya menyentuh file task yang disetujui. |

## 14. Definition of Done

Pekerjaan Admisi Bayi dianggap selesai bila seluruh kondisi berikut terpenuhi:

1. Blueprint, kontrak API, validation matrix, state transition, dan skema tampilan telah disetujui
   serta konsisten dengan alur 7 langkah.
2. Calon ibu hanya berasal dari kasus obstetri `InProgress` atau `Completed` dan tidak bergantung
   pada jenis ruangan ibu.
3. Identitas bayi memakai nama ibu, tanggal/jam lahir bayi, jenis kelamin, No. RM sendiri, serta
   urutan yang dialokasikan backend.
4. Tidak ada input manual urutan kembar, penjamin, rencana penagihan, atau deposit pada Admisi Bayi.
5. Encounter bayi mempunyai snapshot penjamin turunan dan provenance yang dapat diaudit.
6. Episode dan invoice bayi tetap terpisah dari ibu.
7. Deposit tambahan bayi tidak diminta; penggunaan deposit ibu tetap berada pada workflow
   Billing/Kasir/AR.
8. Hanya bed bayi pada service unit valid yang dapat dipilih.
9. Ringkasan lengkap hanya tampil pada konfirmasi.
10. Tombol pembatalan existing tidak berubah.
11. Test backend, frontend, build, regresi, dan live browser mempunyai bukti aktual.
12. Setiap paket implementasi mempunyai laporan task tracked dan register planning diperbarui.

## 15. Dokumen yang Harus Direvisi Sebelum Coding

| Dokumen | Perubahan |
| --- | --- |
| `00-interview-decisions.md` | Tambahkan keputusan final sumber ibu, status kasus, urutan otomatis, penjamin turunan, dan batas Billing/Kasir. |
| `02-backend-architecture.md` | Tambahkan candidate projection, allocator urutan, provenance penjamin, dan logical reference kasus persalinan. |
| `03-frontend-architecture.md` | Ubah state dan navigasi menjadi 7 langkah tanpa payment/deposit. |
| `05-skema-tampilan.md` | Gambar ulang pemilihan ibu, identitas khusus bayi, dan konfirmasi akhir. |
| `contracts/newborn-admission-repair-016.md` | Naikkan versi/amendemen; kontrak 9 langkah saat ini tidak lagi sesuai. |
| `contracts/api-contract.md` | Tambahkan candidate endpoint, create newborn endpoint, dan mode guarantor inheritance. |
| `contracts/integration-contract.md` | Tegaskan invoice bayi terpisah serta handoff hubungan ibu–bayi ke Billing. |
| `contracts/validation-matrix.md` | Tambahkan status kasus, urutan concurrency, penjamin turunan, dan bed eligibility. |
| `contracts/state-transition-matrix.md` | Perbarui transisi alur 7 langkah dan status episode. |
| `data/data-dictionary.md` | Tambahkan sumber kasus persalinan, state allocator, dan provenance guarantor bila disetujui. |
| `roadmap/requirement-traceability-admisi-bayi-016.md` | Petakan requirement baru ke task implementasi setelah task ID dialokasikan secara resmi. |

## 16. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Planning dibuat dari dokumen revisi, audit source frontend/backend, dan klarifikasi pemilik. Tombol Batalkan Pendaftaran dinyatakan di luar scope. | Codex |
| 2026-10-16 | Source backend/frontend, migration, test target, build, laporan task, roadmap, dan traceability diselesaikan. Penerapan database dan live browser belum dilakukan. | Codex |

