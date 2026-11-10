# Laporan Perubahan Backend — `BE-OPT-061`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-061` |
| Judul | Penjamin Perusahaan pada kunjungan Optik |
| Slice | Amendment kesiapan — `EPIC OPT-01` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r5) — Rincian `BE-OPT-061` |
| Trace | `FR-OPT-012`, `FR-OPT-041`; `OPT-DEC-030`, `078`, `084`; `OPT-AC-036`, `027`; `UAT-30`, `31`; risiko `R3` |
| Contract version | `OPT-INT-v1.2` `INT-OPT-01`, `OPT-VAL-v1.2` bagian 8 — `approved` sisi Optik, Yoga Aji Pratama 2026-11-06 |
| Penyetuju modul lain | Muhammad Hamzah, pemilik Registrasi: `OPT-APR-013`, disampaikan Yoga Aji Pratama 2026-11-06 (`OPT-EVD-003`); bukti tertulis belum dilampirkan |
| Approval task | Yoga Aji Pratama, 2026-11-06 ("sudah disetujui hamzah dan yasmina kerjakan") |
| Dependency | `BE-OPT-010` ✅ |
| Klasifikasi | `MEDIUM`, skor 4: berkas diperiksa 1, logika bisnis 1, kontrak API 1, keamanan 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `06e40fb7` (branch `yoga`) + `BE-OPT-060`, `062` di working tree |
| Tanggal | 2026-11-06 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (titik perluasan modul lain); uji di `OpticalManagement` |
| Registry | `Reg` dan `Opt` terdaftar |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-VAL-001`, `QBE-API-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Registrasi menolak Penjamin Perusahaan pada semua kunjungan selain MCU. Karyawan perusahaan tidak bisa didaftarkan di Optik, padahal `OPT-DEC-030` memasukkan mereka ke MVP dan layarnya sudah punya isiannya. Admisi selalu melihat *"Penjamin perusahaan hanya dapat dipilih lewat pendaftaran loket."*

---

## 2. Proses bisnis

**Tujuan:** pasien perusahaan bisa dilayani di Optik dari kunjungan sampai uang muka.

**Pelaku:** Admisi atau petugas Optik.

**Langkah utama:**

1. Petugas memilih pasien dan cara bayar Penjamin Perusahaan, lalu memilih kartunya.
2. Registrasi menerima, karena unit kunjungannya berjenis `Optical`.
3. Seluruh pemeriksaan kartu tetap berjalan seperti jalur MCU.
4. Sumber bayar kunjungan menyimpan snapshot penjamin.
5. Billing menghitung tanggungan dari aturan penjamin, sehingga uang muka dihitung dari bagian pasien.

**Contoh:** dari total Rp 1.200.000, perusahaan menanggung Rp 1.000.000, sehingga bagian pasien Rp 200.000 dan uang muka minimal Rp 100.000. Terbayar Rp 90.000 ditolak; Rp 100.000 diterima.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Kunjungan Laboratorium, Radiologi, atau Farmasi | Tetap ditolak dengan pesan yang sama |
| Kartu kosong, tidak aktif, milik pasien lain, di luar masa berlaku, atau belum layak | Ditolak `400` `OPT-REG-REJECTED`; tidak ada kunjungan |
| Benefit karyawan internal | Tetap hanya lewat loket, karena verifikasi HR ada di sana |
| Ditanggung penuh | Bagian pasien Rp 0; kirim ke pengerjaan diterima tanpa uang muka |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EncounterIntakeService`, `OptVisitService`, `CompanyGuarantorCoverageService`, `OptOrderPaymentGate`, `RegPatientEncounterGuarantor`, `MstPatientCompanyGuarantor`, `MstCompanyGuarantor`, `MstCompanyGuarantorCoverageRule`, dan `PatientEncounterController`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs` | `LoadPaymentSourceReferenceAsync` menerima unit layanan; pengecualian `Outpatient` + `ServiceUnitType.Optical`. +8 / −4 baris |
| `Tests/QuilvianSystemBackend.OpticalTests/Be061CompanyGuarantorTests.cs` | **Baru.** 7 uji; Registrasi dan Billing sungguhan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk `POST visits` tidak berubah; kunjungan berpenjamin kini diterima |
| Database | `NOT APPLICABLE`: tanpa migration dan tanpa kolom baru |
| Keamanan/Auth | Pengecualian sempit menurut jenis unit dari master, bukan dari isian pengguna. Izin tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Optical Management / Opt Visit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/visits` | Daftarkan kunjungan Optik, kini juga dengan Kartu Penjamin Perusahaan | `OptVisit : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build backend | Tidak diperiksa. `tasklist` baru dijalankan menjelang build frontend | `NOT RUN` | Riwayat perintah sesi |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` (termasuk build) | 284 lulus, 0 gagal (277 lama + 7 baru); waktu uji 4 menit 52 detik, di luar build | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 1: syarat jenis unit dilepas | 2 uji gagal (Laboratorium dan unit lain); berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 2: pengecualian Optik dicabut | 4 uji gagal; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas berkas Registrasi, pemanggilan terpisah | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan yang sama) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

Uji manual: `NOT FEASIBLE` — mendaftarkan kunjungan menulis ke devYoga di luar wewenang task ini, dan data kartu penjamin uji belum disiapkan.

**Tidak dijalankan:** uji di PostgreSQL dan alur dengan data asli (`B5`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-036`: kunjungan unit Optik dengan kartu berlaku tersimpan | Terpenuhi | `OPT_AC_036_UAT_30_*` |
| (2) Kunjungan Laboratorium dengan kartu yang sama ditolak | Terpenuhi | `OPT_AC_036_UAT_31_*`, `Pengecualian_mengikuti_jenis_unit_*`; mutasi 1 |
| (3) Kartu berakhir, milik pasien lain, belum layak, dan benefit karyawan ditolak di unit Optik | Terpenuhi | `Pemeriksaan_kartu_tetap_berlaku_di_unit_Optik` |
| (4) Jalur MCU tidak berubah | Terpenuhi | `Jalur_MCU_tetap_menerima_penjamin_perusahaan` |
| (5) `OPT-AC-027` dengan penjamin sungguhan: Rp 90.000 ditolak, Rp 100.000 diterima | Terpenuhi di harness | `OPT_AC_027_*`; kunjungan lewat Registrasi, tanggungan dari `MstCompanyGuarantorCoverageRule` lewat rantai Billing |
| (6) `UAT-30`, `31` | Terpenuhi di harness; layar di `FE-OPT-042` | Uji di atas |
| DoD: laporan dan traceability diperbarui | Terpenuhi | Laporan ini; roadmap; traceability |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Registrasi kini menerima penjamin untuk kunjungan berunit Optik dari pemanggil mana pun, bukan hanya `OptVisitService`. Saat ini hanya layar Optik yang mengirim unit itu |
| Masalah yang diketahui | Tanggungan butuh `MstCompanyGuarantorCoverageRule` untuk barang Optik. Tanpa aturan yang cocok, tanggungan Rp 0 dan pasien menanggung penuh. Aturan ini data milik Master Data, belum ada di devYoga |
| Risiko tersisa | Belum berjalan di PostgreSQL; bukti tertulis persetujuan belum dilampirkan |
| Perubahan sampingan | `NONE`. Probe QBE dihapus, dan berkas mutasi dipulihkan |
| Interupsi | Regresi penuh melewati batas waktu perintah 600 detik saat digabung dengan dua mutasi; berjalan di latar dan selesai `PASS`, tanpa dijalankan ulang |
| Status Git | `M` `EncounterIntakeService.cs`, ditambah perubahan task lain yang belum di-commit. `Tests/` perlu `git add -f` |
| Langkah berikutnya | `FE-OPT-042` (selesai bersamaan); data aturan tanggungan perusahaan untuk barang Optik |
