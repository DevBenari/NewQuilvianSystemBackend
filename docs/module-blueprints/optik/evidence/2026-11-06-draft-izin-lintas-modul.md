# Usulan Amendment — Izin Lintas Modul Optik

| Field | Nilai |
| --- | --- |
| Status | Diterapkan di `OPT-PERM` (`OPT-DEC-085`). Pertanyaan 2 dijawab "cukup saya" (`OPT-DEC-086`) |
| Kontrak yang diusulkan berubah | `contracts/permission-audit-matrix.md`, dari `OPT-PERM-v1.2` menjadi `OPT-PERM-v1.3` |
| Asal | Temuan `T1`, [`testing/readiness-report.md`](../testing/readiness-report.md) |
| Penyetuju | Yoga Aji Pratama, pemilik Optik |
| Tanggal usulan | 2026-11-06 |

Berkas ini mencatat usulannya. Yang berlaku adalah `contracts/permission-audit-matrix.md`.

## 1. Masalah

Layar Optik memanggil endpoint modul lain, tetapi izinnya tidak tercantum pada tabel peran `OPT-PERM-v1.2`. Pada matriks itu, "—" berarti izin tidak diberikan.

Akibatnya, admin akses yang mengikuti matriks akan memberi petugas layar yang tidak bisa dipakai.

**Contoh:** petugas penjualan punya `OptVisit : Create` dan bisa membuka Daftarkan Kunjungan. Saat mencari pasien, server menjawab `403`, karena petugas tidak punya `Patient : Read`. Kunjungan tidak bisa didaftarkan. Uji dengan superadmin tidak menemukan hal ini, karena superadmin melewati pemeriksaan izin.

Tidak ada perubahan kode atau migration. Yang berubah hanya daftar izin yang harus diberikan.

## 2. Usulan baris baru

Baris berikut ditambahkan ke tabel peran `OPT-PERM`. Aturannya: izin hanya diberikan kepada peran yang benar-benar memakai layar pemanggilnya.

| Resource : Action | RO | Sp.M | Petugas penjualan | Kepala Optik | Admin Optik | Admisi | Dipakai untuk |
| --- | :---: | :---: | :---: | :---: | :---: | :---: | --- |
| `Patient : Read` (Pasien) | — | — | ✔ | ✔ | — | ✔ | Cari pasien saat mendaftar |
| `PatientInsurance : Read` (Pasien) | — | — | ✔ | ✔ | — | ✔ | Pilih kartu asuransi |
| `PatientCompanyGuarantor : Read` (Pasien) | — | — | ✔ | ✔ | — | ✔ | Pilih Kartu Penjamin Perusahaan |
| `PatientEncounter : Read` (Registrasi) | — | — | ✔ | ✔ | — | ✔ | Pilih kunjungan asal rujukan |
| `BillingInvoice : Read` (Billing) | — | — | ✔ | ✔ | — | — | Harga dan diskon baris di detail pesanan |
| `Supplier : Read` (Administrator) | — | — | ✔ | ✔ | — | — | Pilih lab lensa luar saat kirim ke pengerjaan |

Empat baris pertama mengikuti `OptVisit : Create`. `BillingInvoice : Read` mengikuti Ajukan Diskon dan status bayar. `Supplier : Read` mengikuti `OptOrder : SendToWork`.

## 3. Endpoint terkait

#### Patient Management / Master Data

Base URL: `/v1/health-services/patient-management/master-data`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/patients/admin/options` | Cari pasien | `Patient : Read` |
| `GET` | `/patient-insurances/admin/options` | Kartu asuransi pasien | `PatientInsurance : Read` |
| `GET` | `/patient-company-guarantors/admin/options` | Kartu penjamin pasien | `PatientCompanyGuarantor : Read` |

#### Registration Management / Patient Encounters

Base URL: `/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/admin/options` | Kunjungan asal rujukan | `PatientEncounter : Read` |

#### Billing Management / Billing / Invoices

Base URL: `/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}` | Harga dan diskon baris | `BillingInvoice : Read` |

#### Administrator / Master Data / Suppliers

Base URL: `/v1/administrator/master-data/suppliers`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options` | Pilihan lab lensa luar | `Supplier : Read` |

## 4. Keputusan

Yoga Aji Pratama menjawab "lanjutkan sesuai saran kamu", 2026-11-06. Pertanyaan 1 mengikuti saran A. Pertanyaan 2 tidak punya saran, sehingga tidak dianggap terjawab dan dicatat sebagai `OPT-OQ-024`. Yoga Aji Pratama kemudian menjawabnya "cukup saya" (`OPT-DEC-086`).

**Koreksi sesudah persetujuan.** Rumusan awal pilihan A menyebut RO dan Sp.M "tidak melihat rincian uang". Itu kurang tepat: Status Bayar tetap tampil bagi mereka, karena memakai `OptOrder : Read`. Keputusannya tidak menambah akses siapa pun. Rumusan di bawah sudah diperbaiki.

| No. | Pertanyaan | Pilihan | Saran |
| --- | --- | --- | --- |
| 1 | RO dan Sp.M punya `OptOrder : Read`. Saat membuka detail pesanan, layar juga membaca tagihan menurut status pesanan, bukan menurut peran. Apakah keduanya diberi `BillingInvoice : Read`? | **A.** Tidak diberi: RO dan Sp.M tidak membaca rincian tagihan kunjungan, dan bagian harga menampilkan pesan penolakan. Status Bayar tetap tampil. **B.** Diberi: tampilan bersih, tetapi petugas klinis bisa membaca seluruh tagihan | **A**, karena tugas klinis tidak butuh data tagihan. Bila pesannya mengganggu, layar bisa menyembunyikan bagian itu lewat task kecil |
| 2 | Apakah pemilik Pasien, Registrasi, Billing, dan Administrator perlu menyetujui pemberian izin baca ini? | **A.** Cukup Yoga dan admin akses, lalu pemilik modul diberi tahu. **B.** Persetujuan per pemilik | Keputusan Yoga. `05-screen-brief.md` baris 423 yang sudah disetujui baru menyebut `PatientEncounter : Read` dan izin baca tagihan |

## 5. Yang belum dipastikan

| Hal | Keterangan |
| --- | --- |
| `PatientEncounter : Create` | Laporan `FE-OPT-010` menyebut izin ini perlu. Audit tidak menemukan pemeriksaannya di `OptVisitController`, `OptVisitService`, maupun `EncounterIntakeService`, sehingga tidak diusulkan. Pastikan dengan akun petugas |
| Izin yang sudah dimiliki Admisi | Belum diperiksa per jabatan. POST Akses Role menimpa seluruh set: baca dulu, lalu kirim ulang utuh |
| Layar lintas modul lain | Daftar ini berasal dari penelusuran kode hook dan konstanta Optik. Bukti akhirnya adalah uji dengan akun tiap peran |

## 6. Langkah sesudah disetujui

Langkah 1 sampai 4 selesai 2026-11-06. Langkah 5 menunggu penetapan jabatan, data, dan izin menulis ke basis data.

1. Catat keputusan baru di `00-interview-decisions.md`.
2. Terbitkan `OPT-PERM-v1.3`, lalu sesuaikan baris izin lintas modul di `05-screen-brief.md`.
3. Terapkan sekaligus penyegaran decision log bagian 14.
4. Kunci ulang hash manifest.
5. Isi kebijakan izin 5 peran, lalu uji dengan akun tiap peran, bukan superadmin.
