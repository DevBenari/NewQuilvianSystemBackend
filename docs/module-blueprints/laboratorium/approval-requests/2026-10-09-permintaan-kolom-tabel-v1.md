# Permintaan Persetujuan — Kolom tabel v1 pada Daftar Pasien Lab serta Hasil dan Riwayat

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-024` |
| `tanggal` | 2026-10-09 |
| `pengaju` | Sesi kerja paritas kolom v1 (lanjutan `LAB-EVD-013`) |
| `rujukan` | `LAB-EVD-013`; tangkapan layar v1 `Daftar Pasien Lab-Patologi Anatomi` dan `Hasil / Riwayat Lab-Patologi Anatomi` yang dikirim pemilik modul 2026-10-09; `contracts/api-contract.md` bagian 42 |
| `status` | ✅ **`disetujui`** 2026-10-09 — Yoga Aji Pratama, pemilik modul, lewat tiga pilihan di sesi: *"Ya, tambahkan semua"*, *"Samakan persis v1"*, *"Tampilkan 'Belum Terkirim'"* |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Amandemen aditif `LAB-API-v1` `r47` beserta susunan kolom layar. Nol tabel, kolom, migration, endpoint, maupun izin |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Tabel v1 menampilkan Email pasien, Tanggal Sampling/Pemeriksaan, seluruh Nama Pemeriksaan, Poli, nama Penjamin
(mis. *Allianz*), dan User Order. Daftar pantau V2 belum mengirim satu pun dari data itu, padahal sumbernya sudah ada
di database. Amandemen ini menambah ruas baca saja pada `LabMonitoringItemResponse`, lalu layar menyusun kolomnya
persis seperti v1.

## 2. Butir yang disetujui

| No | Butir | Keputusan |
|---|---|---|
| 1 | Ruas baru daftar pantau | Tambah `patientEmail`, `clinicName`, `serviceUnitName`, `payerName`, `requestedByName`, `firstCollectedAt`, `firstExaminedAt`, `orderedProcedures` (`r47`) |
| 2 | Kolom V2 yang tidak ada di v1 (Daftar Pasien Lab) | **Samakan persis v1** — Wadah Layak, Kesegeraan, dan Diminta dicabut; CITO ditandai di kolom Order |
| 3 | Kolom *Terkirim ke Pasien* (Hasil dan Riwayat) | **Tampil berisi "Belum Terkirim"** — V2 belum memiliki layanan WhatsApp, sehingga belum ada hasil yang terkirim; diganti data kiriman begitu layanannya berdiri |

## 3. Di luar permintaan ini

- Kolom *Keadaan Order* (`FE-LAB-43`) di daftar Mikrobiologi tetap ada; tidak termasuk tiga kolom yang dicabut.
- Status konfirmasi dokter lewat WhatsApp di bawah *Dokter Konfirmator* v1 — menunggu layanan WhatsApp.
