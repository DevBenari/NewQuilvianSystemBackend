# Laporan Uji Putaran B7 Bagian 2 — Tolak Admisi dan Penutupan Kunjungan IGD Sesudah Admit

| Field | Isi |
| --- | --- |
| Tanggal | Rabu, 28 Oktober 2026, 16.18–16.27 WIB |
| Task | `BE-IGD-077` acceptance 3 dan 4; `AT-IGD-239`, `AT-IGD-240`; `FR-IGD-119`, `FR-IGD-120` |
| Pelaksana | Agent atas perintah Rizki Gunawan. Sisi Rawat Inap tidak melibatkan Muhammad Hamzah: aksi admisi dijalankan lewat API Rawat Inap yang sudah ada, tanpa mengubah kode Rawat Inap |
| Backend | `rizkiG` `1f4212e8` — memuat commit Rawat Inap `32e4f401` (panggilan `EvaluasiPenutupanKunjunganSusulanAsync` dan `POST admission-transfers/{id}/reject`) lewat PR #317 dan #318. Migration tidak ada yang tertunda; build dan run oleh Rizki |
| Frontend | `RizkiV2` `fc5af984f` — memuat commit Rawat Inap `b5f6f9927` (tombol dan modal _Tolak_). Tidak dipakai dalam uji ini |
| Basis data | `QuilvianNewDevRizki`, hanya lewat API; nol kueri SQL |
| Skrip | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b7-bagian2-20261028/skrip/b7b2.mjs` |
| Bukti | Folder yang sama: `<waktu>-<perintah>-hasil.json` dan `<waktu>-<perintah>-jaringan.json`, satu pasang per langkah |
| Status | Implementation ✅ · DevVerif ✅ · UAT belum dijalankan (milik tim terpisah) |

## 1. Tinjauan kode Rawat Inap sebelum uji

Kode Rawat Inap dibaca terhadap bentuk yang diminta [tindak lanjut `IGD-REQ-003`](../approval-requests/2026-10-26-tindak-lanjut-igd-req-003-rawat-inap.md) bagian 2.

| Butir | Temuan | Penilaian |
| --- | --- | --- |
| 1 — evaluasi penutupan sesudah _admit_ | `InpAdmissionTransferService.cs:231-235`, di blok `SourceType == "Emergency"`, sesudah `Executed` dan sebelum `SaveChangesAsync` | Sesuai. Aman karena `CariTindakLanjutTerlaksanaAsync` membaca `.Local` lebih dulu, dan `EmergencyVisitService` diambil dari scope yang sama sehingga perubahan kunjungan ikut satu `SaveChangesAsync` |
| 2 — endpoint _Tolak_ | `InpatientAdmissionTransferController.cs:78`, `POST {id}/reject`, body `{ reason }` wajib, maksimum 500, izin `InpatientAdmissionTransfer : Update`. `{id}` = `TransferId` = `EmgDisposition.Id` untuk sumber IGD (`InpAdmissionTransferService.Queries.cs:36`) | Sesuai |

Tiga catatan untuk pemilik Rawat Inap, tidak menghalangi uji dan bukan lingkup IGD (`IGD-DEC-370`):

1. Frontend menyalakan tombol _Tolak_ bila pengguna punya `InpatientAdmissionTransfer : Update` **atau** `InpatientEpisode : Create`; backend hanya menerima `Update`, sehingga pemegang `Create` saja melihat tombol yang berakhir `403`.
2. _Tolak_ tidak memeriksa scope unit (`UnitsAsync`), berbeda dengan _admit_.
3. _Admit_ mengunci `EmgDisposition` lalu memperbarui `EmgVisit`; _Tolak_ mengunci `EmgVisit` lalu `EmgDisposition`. Dua klik yang hampir serentak dapat berakhir deadlock yang diselesaikan PostgreSQL. Uji acceptance 3 yang berurutan tidak terkena.

## 2. Persiapan lingkungan

| Butir | Tindakan |
| --- | --- |
| Akun admisi | `admisi.b4@rsmmc.local` (Petugas Admisi B4, userId `25d57b5b…`), satu-satunya penempatan Pendaftaran / Petugas Pendaftaran. Jabatan Pendaftaran / Admission Officer ada tetapi tanpa pengguna. **Bukan SuperAdmin**, jadi jalan memutar `IGD-FACT-134` tidak lagi dipakai |
| Izin role | Dengan akun SuperAdmin: `POST role-access/policies` Pendaftaran / Petugas Pendaftaran + `InpatientAdmissionTransfer : Update`. 36 → 37 izin, nol izin lama hilang. Cadangan `policy-petugas-pendaftaran-sebelum.json` dan `-sesudah.json`. Izin admit lain sudah ada sebelumnya |
| Scope unit | `BedManagement:UnitAssignments:4` di user-secrets PC kantor (tidak masuk git): B4, unit Rawat Inap `fddbe4ae…`, operasi `InpatientEpisode:Create`, `InpatientBedOccupancy:Read`/`Create`/`Update`, `InpatientBedManagement:Read`, referensi `UJI-B7-BAGIAN2-20261028`. Terbaca tanpa restart backend (dibuktikan ADMIT-C `201`) |
| Sandi B4 | Tidak diubah. Sandi awal akun pegawai mengikuti pola tanggal lahir (`EmployeeController.GenerateInitialPasswordFromBirthDate`); disimpan ke variabel lingkungan pengguna lewat `setx`, tidak ke berkas |
| Pasien | `siapkan` memakai tindak lanjut Rawat Inap _Dikonfirmasi_ yang sudah ada; nol kunjungan atau tindak lanjut baru |

| Kode | Pasien | No. RM | Form |
| --- | --- | --- | --- |
| C | Pasien Uji P-B5-D 2002 | 00-00-02-60 | Form _Signed_ yang sudah ada dipakai ulang |
| D | Pasien P-B4-B 6503 | 00-00-02-55 | Form baru, ditandatangani DPJP IGD (`dokter2`) |
| E | Pasien P-B4-A 9921 | 00-00-02-54 | Form baru, ditandatangani DPJP IGD (`dokter2`) |

Keadaan awal ketiganya: kunjungan `6` (_AwaitingDisposition_), tindak lanjut `2` (_Confirmed_), form `3` (_Signed_), belum ada kepergian.

## 3. Hasil

| Langkah | Aktor | Yang dibuktikan | Hasil |
| --- | --- | --- | --- |
| S-persiapan | perawat, dokter2 | Tiga pasien siap, keadaan awal benar, semua GET `200` | PASS |
| ADMIT-C | admisi (B4) | Detail transfer, antrean, dan bed terbaca `200`; `POST admit` `201` | PASS |
| **B7-06a** | perawat | Admit sebelum pasien tiba: tindak lanjut `3` (_Executed_), kunjungan **`7` (_Disposed_)**, `visitCompletedAt` kosong, belum ada kepergian | PASS |
| **B7-06b** | perawat, penerima | Kepergian C: sesudah _Terima Dokumen_ kunjungan masih `7`; sesudah _Catat Tiba_ **`9` (_Completed_)**, `visitCompletedAt` terisi | PASS |
| B7-07-pra | perawat, penerima | Kepergian D sebelum admit: pasien tercatat tiba, kunjungan tetap terbuka, tindak lanjut tetap `2` | PASS |
| ADMIT-D | admisi (B4) | `POST admit` `201` | PASS |
| **B7-07** | perawat | Admit sesudah pasien tiba: tindak lanjut `3`, kunjungan **`9`**, `visitCompletedAt` berselisih **0 detik** dari `executedAt` — satu penyimpanan | PASS |
| **TOLAK-C** | admisi (B4) | `POST reject` atas C yang sudah di-_admit_: **`409`**, pesan persis *"Permintaan rawat inap tidak dapat ditolak karena proses admisi sudah dijalankan."* Setara klik _Tolak_ dari tab antrean yang belum dimuat ulang | PASS |
| **T3** | perawat | Data IGD C tidak berubah: tindak lanjut tetap `3`, form tidak `Cancelled` atau `CancelledByAdmission`, nol event `AdmissionRejected`, alasan tolak kosong, jumlah event tetap 3 | PASS |
| TOLAK-E | admisi (B4) | `POST reject` dengan alasan *"Uji B7 bagian 2: Kelas 2 Bangsal Melati penuh"*: `200` | PASS |
| **T1** | perawat | Form E `4` (_Cancelled_), tampilan `6` (_CancelledByAdmission_), alasan tersimpan persis, event `AdmissionRejected` tercatat, tindak lanjut DPJP tetap `2`, kunjungan tidak ditutup | PASS |

Setiap cek "tidak berubah" disertai cek GET `200` pada langkah yang sama. Semua permintaan di log jaringan berstatus seperti yang diharapkan; satu-satunya `409` adalah TOLAK-C yang memang diharapkan.

**Percobaan yang gagal dan diulang.** Percobaan pertama ADMIT-C (16.25) ditolak `400` *"Idempotency-Key wajib, maksimum 100 karakter."* karena skrip belum mengirim header itu. Penolakan terjadi sebelum transaksi, nol data berubah. Skrip ditambah header `Idempotency-Key`, lalu diulang. Bukti percobaan pertama tetap disimpan (`2026-10-28T09-25-45-164Z-admit-C-*`).

## 4. Akibat bagi `BE-IGD-077`

| # | Acceptance | Sebelum | Sesudah |
| ---: | --- | --- | --- |
| 3 | Tolak atas tindak lanjut _Executed_ → `409` dengan pesan persis | Cocok di source, respons runtime belum ada | ✅ TOLAK-C + T3 |
| 4 | Penutupan kunjungan IGD bila pasien sudah tiba sebelum admisi selesai | Fungsi IGD diperbaiki `BE-IGD-083`, pemanggil Rawat Inap belum ada | ✅ B7-07; pasangannya (admit lebih dulu) B7-06a + B7-06b |

Acceptance 1 dan 2 kini juga punya bukti runtime lewat endpoint Rawat Inap yang sebenarnya (T1), bukan hanya Putaran B3. `BE-IGD-077` menjadi **7 dari 7** acceptance.

## 5. Batas bukti

- Langkah perawat dan penerima memakai akun uji IGD yang sama dengan putaran sebelumnya; izin perawat bukan objek uji ini.
- Layar _Tolak_ Rawat Inap tidak diuji lewat browser; yang diuji kontrak API yang dipakai layar itu.
- Uji UAT oleh tim terpisah belum dijalankan.
