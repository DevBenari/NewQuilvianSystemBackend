# `BD-UI-GAP-004` — Filter tanggal pada daftar kerja order darah

| Field | Nilai |
| --- | --- |
| Gap ID | `BD-UI-GAP-004` |
| Status | ✅ **DITUTUP — Opsi B, 23 September 2026.** Penyaring tanggal **dipertahankan**; backend yang menyesuaikan diri |
| Dibuka | 23 September 2026, saat review gap `FE-BD-002` sesudah implementasi kontrak `v5` |
| Layar | `FE-BD-01` — Daftar Order Darah |
| Task terkait | `BE-BD-019` (pelaksana), `FE-BD-002` (menunggu) |
| Contract version | `v5` — `approved` `Sukmagp` 19 September 2026. Penyaring tanggal menjadi amandemen berikutnya, dikerjakan `BE-BD-019` |
| Pemutus | `Sukmagp` (pemilik proses BDRS) |
| Diputuskan pada | **2026-09-23** |
| Keputusan | **Opsi B.** Ditulis sebagai `DEC-BD-059` dan `DEC-BD-060` pada [`00-interview-decisions.md`](00-interview-decisions.md) bagian 8.34 |

---

## 0. Keputusan — 23 September 2026

Pemilik, **`Sukmagp`**, memilih **Opsi B**. Penyaring tanggal pada daftar kerja order darah
**dipertahankan**, dan backend menambahkan dukungannya.

| Butir yang diputuskan | Isi |
| --- | --- |
| Kolom yang disaring | **`BbkBloodOrder.CreateDateTime`** |
| Parameter | `startDate`, `endDate` — keduanya opsional |
| Tempat penyaringan | **Server-side.** Frontend tidak pernah menyaring tanggal secara lokal |
| `endDate` | **Inklusif sampai akhir hari** |
| Zona waktu | Mengikuti **zona waktu aplikasi** — `Asia/Jakarta`, sebagaimana `AppDateTimeHelper` |

Satu turunan teknis yang ikut diputuskan dan **wajib dibaca bersama** keputusan di atas: kolom yang
disaring tersimpan dalam **UTC**, sehingga batas rentang harus **dikonversi** dari waktu Jakarta ke UTC
sebelum dibandingkan — bukan distempel `DateTimeKind.Utc` apa adanya seperti pola yang sudah ada di
repository. Rinciannya beserta contoh berangkanya ada pada `DEC-BD-060`. Tanpa konversi itu, setiap order
yang lahir antara tengah malam dan pukul tujuh pagi WIB akan jatuh ke hari yang keliru.

**Pelaksana:** task **`BE-BD-019`** — [laporan](task/report/backend/BE-BD-019.md). Belum dikerjakan.

**Sampai `BE-BD-019` selesai, keadaan pada bagian 2 di bawah tetap berlaku:** kontrol tanggal tayang dan
tidak berfungsi. Bagian 1–7 dipertahankan apa adanya sebagai catatan keadaan yang mendasari keputusan.

---

## 1. Keadaan

**Layar order darah punya kontrol tanggal yang tidak pernah sampai ke backend.**

Daftar kerja merender tiga kontrol penyaring waktu: pemilih Tanggal Awal, pemilih Tanggal Akhir, dan
dropdown Periode berisi Hari Ini, 7 Hari Terakhir, 30 Hari Terakhir, Bulan Ini, dan Rentang Tanggal.
Ketiganya menulis `startDate` dan `endDate` ke state penyaring, lalu mengirimnya sebagai query.

`GET /api/v1/health-services/blood-bank-management/blood-orders` **tidak menerima kedua parameter itu.**
Tanda tangan endpointnya hanya mengenal `search`, `patientId`, `encounterId`, `serviceUnitId`,
`bloodComponentId`, `orderStatus`, `orderSource`, `sortBy`, `sortDirection`, `pageNumber`, dan
`pageSize`. `BloodOrderDefaultFilterResponse` pada `GET /filters/metadata` juga tidak memuat isian
tanggal apa pun.

Akibatnya parameter itu **dibuang diam-diam** oleh model binder ASP.NET Core. Tidak ada `400`, tidak ada
peringatan, tidak ada jejak.

## 2. Akibat bagi petugas

Petugas BDRS membuka daftar kerja, memilih Periode **"Hari Ini"**, dan **daftarnya tidak berubah sama
sekali** — masih memuat seluruh order dari hari mana pun. Tidak ada satu pun pesan yang menjelaskan
mengapa.

Ini lebih buruk daripada tidak menyediakan penyaring tanggal. Penyaring yang hilang membuat petugas
mencari cara lain. **Penyaring yang berbohong membuat petugas percaya ia sudah menyaring**, lalu
mengambil kesimpulan dari daftar yang sebenarnya belum tersaring. Pada daftar kerja klinis, kesimpulan
seperti itu berakhir pada order yang terlewat.

## 3. Bukti

| Yang diperiksa | Hasil |
| --- | --- |
| `BbkBloodOrderController.GetAll` — parameter yang diterima | Nol parameter tanggal. Sebelas parameter lain ada |
| `BloodOrderDefaultFilterResponse` | Nol isian tanggal |
| `contracts/api-contract.md` grup Blood Order, termasuk Amendment `v5` D4 | `v5` menambah `bloodComponentId` saja. Nol penyaring tanggal pernah dikontrakkan |
| `BLOOD_ORDER_DEFAULT_FILTERS` di frontend | Memuat `startDate`, `endDate`, `customPeriod` |
| `blood-order-list-view.jsx` | Merender dua `FilterDatePicker` dan satu `FilterSelect` periode |
| `buildRequestFilters` | Membuang `customPeriod` saja; `startDate` dan `endDate` **tetap terkirim** |

## 4. Mengapa tidak diperbaiki sendiri

Frontend **tidak boleh** menyaring tanggal secara lokal. Daftarnya berhalaman di server: menyaring satu
halaman di layar akan menghasilkan halaman yang jumlah barisnya berubah-ubah, nomor halaman yang tidak
lagi berarti, dan angka total yang bertentangan dengan isinya. Penyaringan tanggal yang benar hanya bisa
terjadi di sisi yang memegang seluruh datanya.

Maka yang tersisa dua pilihan, dan keduanya menuntut keputusan pemilik — bukan keputusan pelaksana.

## 5. Pilihan

### Opsi A — hapus kontrol tanggal dari layar

Buang kedua pemilih tanggal dan dropdown periode; buang `startDate`, `endDate`, dan `customPeriod` dari
penyaring bawaan dan dari permintaan.

| Hal | Isi |
| --- | --- |
| Biaya | Kecil. Tiga berkas frontend, nol perubahan backend, nol kontrak |
| Waktu | Dapat selesai dalam satu sesi |
| Yang didapat | Layar berhenti berbohong. Yang tampil hanya penyaring yang benar-benar bekerja |
| Yang hilang | Petugas kehilangan penyaringan waktu. Untuk daftar kerja yang tumbuh setiap hari, ini terasa begitu ordernya menumpuk |
| Cocok bila | Penyaringan waktu memang bukan kebutuhan BDRS hari ini, atau ditunda ke rilis berikutnya |

### Opsi B — buka task backend untuk menambah penyaring tanggal

Tambah `startDate` dan `endDate` pada `GET /blood-orders`, sertakan pada `filters/metadata`, lalu
sambungkan layar apa adanya.

| Hal | Isi |
| --- | --- |
| Biaya | Sedang. Task backend baru, **amandemen kontrak** (aditif, nol klien lama rusak), lalu penyambungan frontend |
| Waktu | Satu task backend beserta verifikasinya, ditambah satu pass frontend |
| Yang didapat | Penyaring yang sudah ada di layar akhirnya berfungsi; daftar kerja dapat dipersempit ke rentang yang relevan |
| Yang perlu diputuskan ikut | **Tanggal apa yang disaring** — `CreateDateTime` order, atau tanggal lain? Inklusif sampai akhir hari `endDate`? Zona waktu mana yang dipakai membandingkan? Ketiganya keputusan bisnis, bukan teknis |
| Cocok bila | Penyaringan waktu memang dipakai BDRS sehari-hari |

## 6. Rekomendasi — *tercatat sebelum keputusan; tidak diikuti pemilik*

> Bagian ini dipertahankan apa adanya sebagai jejak. Rekomendasi pelaksana waktu itu **Opsi A dulu**;
> pemilik memilih **Opsi B**. Yang berlaku adalah bagian 0.


**Opsi A lebih dulu, Opsi B menyusul bila memang dibutuhkan.**

Alasannya bukan bahwa penyaringan tanggal tidak berguna — kemungkinan besar berguna. Alasannya adalah
keadaan hari ini: layar sudah menampilkan kontrol yang berbohong, dan setiap hari ia dibiarkan tayang
adalah hari ketika petugas bisa salah menyimpulkan dari daftar yang dikiranya tersaring. Opsi A
menghentikan itu sekarang dengan biaya kecil. Opsi B menyelesaikannya dengan benar, tetapi menuntut tiga
keputusan bisnis tambahan pada bagian 5 yang belum pernah ditanyakan kepada siapa pun.

Keduanya tidak saling meniadakan: mengambil A hari ini tidak menutup B besok.

## 7. Yang berubah setelah diputuskan

| Bila | Langkah |
| --- | --- |
| **Opsi A** | Perbaikan frontend dikerjakan di bawah `FE-BD-002`; gap ini ditutup; nol kontrak berubah |
| **Opsi B** | Task backend baru dibuka pada `roadmap/backend-roadmap.md`; `api-contract.md` grup Blood Order diamandemen; `FE-BD-002` menunggu task itu selesai sebelum penyaringnya disambungkan; gap ini ditutup sesudah keduanya terbukti runtime |

**Yang berlaku: Opsi B.** `BE-BD-019` dibuka, `api-contract.md` diamandemen di dalam task itu, dan
`FE-BD-002` menunggu penyelesaiannya untuk bagian penyaring tanggal saja. Sampai task itu selesai,
**kontrol tanggal tetap tayang dan tetap tidak berfungsi** — keadaan yang sekarang tercatat, bukan lagi
kejutan. Tercatat juga pada laporan [`FE-BD-002`](task/report/frontend/FE-BD-002.md) bagian 8.

## 8. Catatan pendaftaran

Gap ini ditutup pada tingkat keputusan lewat `DEC-BD-059` dan `DEC-BD-060`. Pendaftaran ke
`closed_ui_gaps` pada `roadmap/frontend-roadmap.md` dan ke `blueprint-manifest.md` **belum dikerjakan**;
keduanya dokumen berstatus `APPROVED` dan penyuntingannya di luar scope pass ini. Yang sudah disunting
pada pass ini hanya kartu `FE-BD-002` (dependency) dan penambahan kartu `BE-BD-019`, keduanya atas
instruksi pemilik.
