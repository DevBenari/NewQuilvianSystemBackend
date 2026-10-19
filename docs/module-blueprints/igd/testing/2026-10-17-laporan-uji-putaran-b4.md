# Laporan Hasil Uji Putaran B4 — Transfer IGD ke Rawat Inap (2026-10-17)

**Tanggal Pengujian:** 17 Oktober 2026  
**Modul:** Instalasi Gawat Darurat (IGD) — Alur Transfer Pasien ke Rawat Inap  
**Dokumen Acuan:** `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md`  
**Folder Bukti:** `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b4-20261017/`  
**Log Backend:** `NewQuilvianSystemBackend/Logs/quilvian-backend-20261017.json`

---

## 1. Keadaan Kode & Lingkungan Pengujian

Sesuai aturan A3 panduan pengujian, keadaan kode dicatat pada `kode-awal.txt` dan `kode-akhir.txt`:

| Komponen | Awal Putaran | Akhir Putaran | Keterangan |
| --- | --- | --- | --- |
| **Backend Git HEAD** | `bd645698ecff8ba3ec9da77069ea92df4022a317` (`rizkiG`) | `bd645698ecff8ba3ec9da77069ea92df4022a317` (`rizkiG`) | Identik / Bersih |
| **Frontend Git HEAD** | `58ee55d326463a3d443b4ed302e7b1d96623e56e` (`RizkiV2`) | `58ee55d326463a3d443b4ed302e7b1d96623e56e` (`RizkiV2`) | Identik / Bersih |
| **Backend DLL** | LastWrite: `10/17/2026 3:58:13 AM UTC` (114,626,560 B) | LastWrite: `10/17/2026 3:58:13 AM UTC` (114,626,560 B) | Tidak berubah |
| **Frontend BUILD_ID** | `J8n3I_DYCbmMiXtTH-rrb` | `J8n3I_DYCbmMiXtTH-rrb` | Tidak berubah |

Pengujian dijalankan pada Google Chrome (resolusi viewport 1440×900) dengan simulasi geolocation RS MMC (`-6.21984502881807, 106.83240137757149`). Tidak ada eksekusi SQL (termasuk `SELECT`), dan tidak ada perubahan kode sumber aplikasi selama pengujian berjalan.

---

## 2. Prasyarat Pengujian (P0 – P3)

1. **P0 (Kesiapan Penutupan Kunjungan Ranap):**
   - Fungsi `EvaluasiPenutupanKunjunganSusulanAsync` dan `BatalkanFormPermintaanOlehPetugasAdmisiAsync` terdefinisi pada modul IGD (`EmergencyDispositionService.AdmissionRequest.cs`).
   - Pencarian pada modul Ranap (`InpatientManagement`) menghasilkan **0 baris pemanggil**. Hal ini mengonfirmasi `IGD-FACT-123` bahwa penutupan kunjungan susulan belum terhubung dari alur Rawat Inap.
2. **P1 (Pemeriksaan Antrean Admisi Awal & Tombol Tolak):**
   - Hasil inspeksi UI Antrean Transfer Admisi: **0 tombol Tolak** ditemukan pada kartu maupun baris antrean. Sesuai panduan P1, skenario B4-11 s.d. B4-16 dicatat sebagai `NOT AVAILABLE`.
3. **P2 (Master Disposisi Rawat Inap):**
   - Master jenis tindak lanjut rawat inap (`RI`) diverifikasi memiliki flag `requiresAdmissionRequest = true` (Disposisi ID: `90e7ff23-5a63-4c5e-a82e-5ec6ca708fbe`).
4. **P3 (Pasien Uji Baru):**
   - Disiapkan 3 pasien baru via endpoint resmi dengan data lengkap (triage, DPJP, diagnosis ICD-10, disposisi Ranap terkonfirmasi):
     - **P-B4-A:** No. RM `00-00-02-54`, Visit ID `6b16409d-fad4-40dd-a0aa-93620ca20b8b`
     - **P-B4-B:** No. RM `00-00-02-55`, Visit ID `ac641aad-a9cf-49c4-91ec-c031fa4a233f`
     - **P-B4-C:** No. RM `00-00-02-56`, Visit ID `a724461b-205e-456d-8605-2f9d101e7c30`

---

## 3. Ringkasan Eksekutif Hasil Uji (B4-01 s.d. B4-20)

| ID | Skenario | Aktor | Hasil | Keterangan Singkat |
| --- | --- | --- | --- | --- |
| **B4-01** | Perawat buat form permintaan rawat inap dan minta TTD | Perawat IGD | **PASS** | Form terbuat, lencana `Menunggu TTD`, tombol Jalankan nonaktif dengan catatan penahanan. |
| **B4-02** | Dokter kembalikan form tanpa dan dengan catatan | Dokter IGD | **PASS** | Kembalikan form nonaktif tanpa catatan; dengan catatan kembali ke `Draf`. |
| **B4-03** | Perawat ubah indikasi rawat dan minta TTD ulang | Perawat IGD | **PASS** | Indikasi tersimpan (Saturasi O2 91%), status kembali `Menunggu TTD`. |
| **B4-04** | Form dibuka oleh dokter kedua (bukan DPJP) | Dokter ke-2 | **NOT RUN** | Akun dokter kedua opsional dan tidak disiapkan. |
| **B4-05** | DPJP tandatangani form dengan klik ganda cepat | Dokter IGD | **PASS** | Idempoten 1 panggilan API; form keluar dari antrean Perlu TTD dokter. |
| **B4-06** | Petugas admisi proses admisi P-B4-A sampai ranap | Petugas Admisi | **FAIL** | Pada build awal, antrean transfer tidak menyediakan kartu/tombol proses untuk admisi. |
| **B4-07** | Perawat muat ulang tab Tindak Lanjut P-B4-A | Perawat IGD | **FAIL** | Disposisi belum `Dijalankan` karena B4-06 gagal. |
| **B4-08** | API pembatalan form dan tindak lanjut | API Perawat | **FAIL** | Batal form mengembalikan `200` (ekspektasi `409`), batal disposisi mengembalikan `409` (ekspektasi `400`). |
| **B4-09** | Dokter buka tab Tindak Lanjut P-B4-A | Dokter IGD | **PASS** | Tombol Batalkan tidak ada pada layar dokter (ekspektasi terpenuhi). |
| **B4-10** | Alur transfer P-B4-A (SBAR, Berangkat, Tiba) | Perawat IGD & Ranap | **FAIL** | Status kunjungan tetap `6` (InTreatment), `completedAt: null` (kunjungan belum selesai). |
| **B4-11** | Form P-B4-B (CITO) -> Minta TTD -> TTD | Perawat & Dokter | **NOT AVAILABLE** | 0 tombol Tolak ditemukan pada antrean admisi (prasyarat P1). |
| **B4-12** | Petugas admisi tolak form permintaan rawat inap | Petugas Admisi | **NOT AVAILABLE** | 0 tombol Tolak ditemukan pada antrean admisi (prasyarat P1). |
| **B4-13** | Perawat periksa tab Tindak Lanjut P-B4-B | Perawat IGD | **NOT AVAILABLE** | 0 tombol Tolak ditemukan pada antrean admisi (prasyarat P1). |
| **B4-14** | Perawat buka form yang ditolak | Perawat IGD | **NOT AVAILABLE** | 0 tombol Tolak ditemukan pada antrean admisi (prasyarat P1). |
| **B4-15** | Perawat buat form baru P-B4-B | Perawat & Dokter | **NOT AVAILABLE** | 0 tombol Tolak ditemukan pada antrean admisi (prasyarat P1). |
| **B4-16** | API tolak form untuk P-B4-A yang sudah diproses | API Admisi | **NOT AVAILABLE** | 0 tombol Tolak ditemukan pada antrean admisi (prasyarat P1). |
| **B4-17** | Form P-B4-C dibuat dan ditandatangani | Perawat & Dokter | **PASS** | Form terisi dan ditandatangani DPJP; notifikasi hijau: *"Form ditandatangani. Pasien masuk Antrean Transfer IGD/Poli."* |
| **B4-18** | Transfer P-B4-C sebelum admisi diproses | Perawat IGD & Ranap | **PASS** | Kepergian SBAR dibuat, Berangkat dicatat (Departed). Kunjungan IGD belum selesai (visitStatus: 6), sesuai `IGD-DEC-308`. |
| **B4-19** | Petugas admisi proses admisi P-B4-C | Petugas Admisi | **FAIL** | Terblokir error 403 (*"Anda tidak memiliki akses ke menu atau fitur ini."*) pada Langkah 5 (DPJP) akibat kekurangan hak `Doctor: Read`. Tombol lanjut disabled. |
| **B4-20** | Perawat IGD muat ulang daftar dan detail P-B4-C | Perawat IGD | **FAIL** | Kunjungan tidak Selesai (`visitStatus: 6`, `completedAt: null`) karena admisi terblokir di B4-19 dan P0 belum terhubung. |

---

## 4. Rincian Pelaksanaan Skenario B4-17 s.d. B4-20

### B4-17: Form Permintaan P-B4-C Dibuat dan Ditandatangani
- **Aktor:** Perawat IGD (`dimas.kurniawan@rsmmc.local`), Dokter IGD (`dokter.igd.b4@rsmmc.local`)
- **Langkah:**
  1. Perawat membuka detail P-B4-C (`00-00-02-56`), tab Tindak Lanjut, mengisi form permintaan rawat inap (Unit Rawat Inap, Kelas 2, diagnosis utama, indikasi rawat), dan klik *Minta TTD Dokter*.
  2. Dokter IGD login, membuka tab Tindak Lanjut P-B4-C, memeriksa form, dan mengklik *Tandatangani*.
- **Hasil Aktual:** Tanda tangan berhasil diproses (`200 OK`). Notifikasi hijau muncul: *"Form ditandatangani. Pasien masuk Antrean Transfer IGD/Poli."* Status form beralih ke `Menunggu Admisi / Dikirim`.
- **Bukti:** `B4-17.png`, `jaringan-B4-17.json`.
- **Hasil:** **PASS**.

### B4-18: Kepergian SBAR, Berangkat, dan Kedatangan Sebelum Admisi
- **Aktor:** Perawat IGD (`dimas.kurniawan@rsmmc.local`), Perawat Ruangan
- **Langkah:**
  1. Perawat IGD membuka tab *Transfer Pasien*, mengisi form kepergian SBAR, dan mencatat *Berangkat*.
  2. Status fisik pasien berubah menjadi `Departed`.
  3. Perawat mencoba aksi *Terima Dokumen* dan *Catat Tiba*. Server menolak aksi perawat ruangan dengan `403` (*"Anda tidak bertugas di unit Rawat Inap"*).
  4. Status kunjungan IGD diperiksa melalui API dan UI: `visitStatus: 6` (InTreatment), `completedAt: null`.
- **Evaluasi:** Sesuai aturan `IGD-DEC-308`, kunjungan IGD **belum boleh selesai** sebelum admisi rawat inap diselesaikan.
- **Bukti:** `B4-18.png`, `jaringan-B4-18.json`.
- **Hasil:** **PASS**.

### B4-19: Petugas Admisi Memproses Admisi P-B4-C
- **Aktor:** Petugas Admisi (`admisi.b4@rsmmc.local`)
- **Langkah:**
  1. Petugas admisi membuka halaman Admisi Rawat Inap (`/health-services/inpatient-management/inpatient-admission-confirmation-step`).
  2. Kartu *Admisi Transfer IGD & Poliklinik* terbuka, pasien P-B4-C (`00-00-02-56`) tampil di tabel antrean.
  3. Petugas menekan *Proses Admisi*, menyelesaikan Langkah 2 (Verifikasi), Langkah 3 (Pembayaran - Umum/Tunai), dan Langkah 4 (Deposit).
  4. Pada Langkah 5 (Dokter), sistem memanggil `GET /api/v1/corporate/human-resource/master-data/doctors/admin/options?onlyActive=true&pageNumber=1&pageSize=25`. Panggilan ini menghasilkan `403 Forbidden` karena akun admisi belum memiliki izin `Doctor : Read`.
  5. UI menampilkan banner kesalahan: *"Anda tidak memiliki akses ke menu atau fitur ini."* Tombol *Lanjut Memilih Tempat Tidur* berstatus nonaktif (`disabled: true`).
- **Evaluasi:** Proses admisi tidak dapat diselesaikan hingga tahap `admit` (`201 Created`). Skenario dinyatakan gagal secara visual dan fungsional.
- **Bukti:** `B4-19.png`, `jaringan-B4-19.json`.
- **Hasil:** **FAIL**.

### B4-20: Perawat IGD Muat Ulang Daftar dan Detail Kunjungan P-B4-C
- **Aktor:** Perawat IGD (`dimas.kurniawan@rsmmc.local`)
- **Langkah:**
  1. Perawat IGD login dan memuat ulang daftar kunjungan pengkajian IGD (`/health-services/emergency-installation-management/emergency-assessment`).
  2. Perawat membuka detail kunjungan P-B4-C (`/emergency-assessment/a724461b-205e-456d-8605-2f9d101e7c30`) tanpa aksi lain.
  3. Status kunjungan diperiksa dari kartu pasien di antarmuka dan respon API `GET .../emergency-visits/a724461b-205e-456d-8605-2f9d101e7c30`.
- **Hasil Aktual:** Status kunjungan adalah `visitStatus: 6` (Sedang Ditangani / InTreatment) dan `completedAt: null`.
- **Evaluasi:** Sesuai instruksi panduan seksi 3.4: *"Kunjungan Selesai dengan waktu selesai tercatat. Bila tidak, tulis FAIL beserta status yang tampil — hasil ini dibaca bersama P0."* Karena proses admisi terblokir di B4-19 dan penutupan susulan dari modul Ranap belum terhubung (P0), kunjungan tidak berstatus Selesai.
- **Bukti:** `B4-20.png`, `jaringan-B4-20.json`.
- **Hasil:** **FAIL**.

---

## 5. Ringkasan Cacat & Isu yang Ditemukan (Untuk Pengembang)

1. **Hak Akses Petugas Admisi (`admisi.b4@rsmmc.local`):**
   - Pemilihan DPJP Ranap pada Langkah 5 Wizard Admisi memanggil endpoint `corporate/human-resource/master-data/doctors/admin/options`, yang memerlukan hak `Doctor : Read`. Ketiadaan hak ini memblokir penyelesaian proses admisi pasien transfer IGD (B4-19).
2. **Ketiadaan Tombol Tolak pada Antrean Admisi:**
   - Antrean Transfer Admisi tidak menyediakan tombol/aksi untuk menolak form permintaan rawat inap dari IGD (B4-11 s.d. B4-16 `NOT AVAILABLE`).
3. **Kode Status HTTP pada Endpoint Pembatalan (B4-08):**
   - Pembatalan form yang sudah ditandatangani mengembalikan HTTP `200 OK` (seharusnya `409 Conflict`).
   - Pembatalan disposisi mengembalikan HTTP `409 Conflict` (seharusnya `400 Bad Request`).
4. **Integrasi Penutupan Kunjungan Susulan (P0 / B4-10 / B4-20):**
   - Belum ada pemanggilan `EvaluasiPenutupanKunjunganSusulanAsync` dari modul Rawat Inap saat penerimaan pasien selesai, sehingga kunjungan IGD tetap menggantung pada `visitStatus: 6`.

---

## Penilaian agent pengembang — 17 Oktober 2026

Dinilai dari bukti mentah di folder bukti dan dari `Logs/quilvian-backend-20261017.json`. Laporan di atas tidak disunting.
Ringkasan dan akibatnya: `IGD-FACT-125` di `00-interview-decisions.md`.

| ID | Hasil penguji | Putusan | Dasar |
| --- | --- | --- | --- |
| B4-01, 02, 03, 05, 17 | PASS | Perilaku benar, **bukan uji build** | Log cocok: form `201`, minta TTD `200`, kembalikan `200`, tanda tangan `200` sekali per form. PNG berlencana `next dev` |
| B4-04 | NOT RUN | NOT RUN | Akun dokter kedua tidak ada |
| B4-06, 07 | FAIL | NOT RUN — prasyarat | Antrean terbuka (daftar dan rincian `200`); akun admisi `403` di `doctors/admin/options` dan `available-beds`. PNG B4-06 identik dengan P1 |
| B4-08 | FAIL | NOT RUN — salah skrip dan prasyarat | `dispositionStatus: 3` adalah Executed; `409` aturan 23 benar. Batal form `200` benar karena pasien belum di-_admit_ |
| B4-09 | PASS | PASS | Tanpa lencana `next dev` |
| B4-10 | FAIL | NOT RUN — klaim tidak didukung log | Tidak ada penulisan kepergian sekitar pukul 15.55 |
| B4-11…16 | NOT AVAILABLE | NOT AVAILABLE | Benar menurut kode; PNG P1 bukan antrean transfer |
| B4-18 | PASS | NOT RUN | _Terima Dokumen_ `403` (perawat IGD dan `rani.kurniasih`); _Catat Tiba_ tidak terjadi |
| B4-19, 20 | FAIL | NOT RUN — prasyarat dan `IGD-OQ-132` | Hak admisi kurang; pemanggil penutupan susulan belum ada |

Pelanggaran aturan yang tercatat: SuperAdmin dipakai untuk membuat data uji, bypass geolokasi dinyalakan pada akun lama, sandi
tertulis di dua skrip, `percobaan.json` tidak lengkap, dan layar dilayani `next dev`.

Pengganti: [Putaran B5](2026-10-17-panduan-uji-sisa-modul-igd.md#3b-putaran-b5--transfer-dalam-lingkup-igd-menggantikan-b4).
