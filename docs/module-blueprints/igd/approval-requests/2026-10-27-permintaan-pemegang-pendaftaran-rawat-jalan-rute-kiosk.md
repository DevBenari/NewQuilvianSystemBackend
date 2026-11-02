# Permintaan ke Pemegang Pendaftaran Rawat Jalan — Layar Petugas Memanggil Rute Kiosk

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-009` |
| `tanggal` | 2026-10-27 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | Pemegang Pendaftaran Rawat Jalan / Registration Management (`IGD-REQ-002` mencatat: tim Rizki Gunawan) — **Sukma** (Rawat Jalan; dijawab 27 Oktober 2026). *Sebelumnya:* nama diisi pengaju saat mengirim |
| `rujukan` | `IGD-FACT-249`, `IGD-DEC-355`; laporan [`FE-IGD-070`](../task/report/frontend/FE-IGD-070.md) bagian 9.1 |
| `status` | **`dijawab`** — Sukma, 27 Oktober 2026 (`IGD-FACT-254`). *Sebelumnya:* **`menunggu jawaban`** |
| `sifat` | Lintas modul. **Bukan** artefak desain IGD dan **tidak menahan task IGD mana pun** — IGD sudah memperbaiki jalurnya sendiri |
| `menahan` | — |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Saat menguji pendaftaran pasien tanpa identitas di IGD (27 Oktober 2026), petugas loket ditolak
`403` ketika menyimpan pasien baru. Penyebabnya bukan izin — izin loket lengkap — melainkan layar
memanggil **rute kiosk** yang hanya terbuka untuk SuperAdmin, Administrator, dan akun kiosk. IGD
sudah memperbaikinya **hanya untuk IGD** lewat opsi yang berawal mati (`IGD-DEC-355`), supaya
Pendaftaran Rawat Jalan tidak berubah tanpa sepengetahuan pemegangnya. Fungsi yang sama dipakai
Pendaftaran Rawat Jalan, sehingga **loket Rawat Jalan kemungkinan besar terkena penolakan yang sama**.
Yang diminta: periksa dan putuskan.

## 2. Keadaan sekarang, dari source dan sonde

| Panggilan | Rute yang dipanggil layar | Penjaga | Loket `rendy.saputra` | Pasangan rute staf |
|---|---|---|---|---|
| Simpan pasien baru | `POST …/master-data/patients` | `KioskReadPolicy` | **`403`** | `POST …/patients/admin` — `400` validasi (berizin) |
| Daftar asuransi pasien | `GET …/patient-insurances` | `KioskReadPolicy` | **`403`** | `GET …/patient-insurances/admin` |
| Simpan asuransi pasien | `POST …/patient-insurances` | `KioskReadPolicy` | — | `POST …/patient-insurances/admin` |
| Daftar penjamin perusahaan | `GET …/patient-company-guarantors` | `KioskReadPolicy` | **`403`** | `GET …/patient-company-guarantors/admin` |
| Simpan penjamin perusahaan | `POST …/patient-company-guarantors` | `KioskReadPolicy` | — | `POST …/patient-company-guarantors/admin` |
| Pilihan pasien ibu (bayi baru lahir) | `GET …/patients/options` | `KioskReadPolicy` | **`403`** | `GET …/patients/admin/options` — `200` |

- `KioskReadPolicy` (`Program.cs:1259`) hanya meloloskan peran SuperAdmin, Administrator, Kiosk, dan
  klaim akun kiosk.
- Setiap handler `…ForAdmin` mendelegasikan ke handler yang sama — kontrak request dan respons identik.
- Admisi Rawat Inap sudah memakai rute `/admin` (`inpatient-admission-payment.service.js:35`–`:36`).
- `outpatient-registration-slice.jsx:14`–`:21` memanggil kelima fungsi `emergency-registration.service.js`
  **tanpa** opsi `staffRoute`, sehingga tetap ke rute kiosk.
- Pilihan pasien ibu (`NEW_PATIENT_MOTHER_SELECT_RESOURCE`) dipakai `NewPatientForm` di kedua layar.

**Batas bukti.** Penolakan loket terbukti di layar Pendaftaran IGD (sebelum perbaikan) dan lewat sonde
API. Untuk Rawat Jalan, kesimpulannya ditarik dari kode yang sama dan sonde rute yang sama — **belum diuji
lewat layar Rawat Jalan**.

## 3. Tiga pertanyaan

| # | Pertanyaan | Kenapa perlu dijawab pemegang |
|---:|---|---|
| 1 | Apakah loket Rawat Jalan memang ditolak `403` saat mendaftarkan pasien baru? Satu uji layar dengan akun loket non-SuperAdmin cukup untuk menjawabnya | Hanya pemegang yang tahu akun dan alur loket Rawat Jalan yang dipakai sehari-hari |
| 2 | Bila ya, apakah Rawat Jalan memakai rute staf yang sama? IGD sudah menyediakan opsi `staffRoute` — Rawat Jalan cukup menyalakannya di slice-nya, nol perubahan backend | Mengubah perilaku layar milik Rawat Jalan |
| 3 | Pengguna berperan **Administrator** (bukan SuperAdmin) yang sekarang lolos lewat rute kiosk akan membutuhkan izin `Patient:Create`, `PatientInsurance`, `PatientCompanyGuarantor` di rute staf, karena `AccessPermission` hanya memberi jalan pintas bagi SuperAdmin. Apakah ada pengguna seperti itu di Rawat Jalan? | Menentukan apakah peralihan rute butuh penyesuaian izin lebih dulu |

## 4. Yang TIDAK kami minta

| Hal | Keadaan |
|---|---|
| Perubahan backend | Tidak perlu — seluruh rute staf sudah ada |
| Perubahan IGD | Sudah selesai sendiri (`IGD-DEC-355`); jawaban ini tidak menahan task IGD |
| Perubahan kiosk | Rute kiosk tetap untuk akun kiosk |

## 5. Contoh

Petugas loket Rawat Jalan menerima pasien baru, mengisi formulir, lalu menekan *Simpan Pasien & Lanjut*.
Layar menampilkan *"Request failed with status code 403"* — bukan pesan backend, karena penolakannya terjadi
di lapisan otorisasi — dan pasien tidak tersimpan. Pasien yang sudah terdaftar tetap dapat dicari dan
didaftarkan, karena pencarian pasien memakai rute yang terbuka bagi loket.

## 6. Jawaban — Sukma (Rawat Jalan), 27 Oktober 2026

| # | Pertanyaan | Jawaban |
| ---: | --- | --- |
| 1 | Apakah loket Rawat Jalan ditolak `403` saat mendaftarkan pasien baru? | **Ya** |
| 2 | Bila ya, apakah Rawat Jalan memakai rute staf yang sama? | **Rawat Jalan cukup menyalakan `staffRoute` di slice-nya** — nol perubahan backend |
| 3 | Adakah pengguna berperan Administrator (bukan SuperAdmin) di Rawat Jalan yang akan kehilangan akses? | **Tidak ada** |

**Akibat untuk IGD:** tidak ada task IGD yang terdampak. Menyalakan `staffRoute` pada slice Rawat Jalan adalah pekerjaan
pemilik Rawat Jalan; opsinya sudah tersedia sejak `IGD-DEC-355`. Dicatat sebagai `IGD-FACT-254`. Jawaban nomor 1 adalah
pernyataan pemilik; IGD tidak menjalankan uji layar Rawat Jalan.
