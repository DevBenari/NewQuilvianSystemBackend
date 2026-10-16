# Panduan Uji Putaran B3 — Transfer IGD → Rawat Inap, Penolakan Admisi, & Penutupan Dua Arah (`BE-IGD-074`, `BE-IGD-077`, `FE-IGD-060`, `FE-IGD-061`, `FE-IGD-064`)

| Field | Nilai |
| --- | --- |
| Task Diuji | `BE-IGD-074`, `BE-IGD-077` (Backend) · `FE-IGD-060`, `FE-IGD-061`, `FE-IGD-064` (Frontend) |
| Requirement | `FR-IGD-109`…`116`, `FR-IGD-119`, `FR-IGD-120`; `AT-IGD-225`…`240` |
| Keputusan | `IGD-DEC-304`…`309` |
| Kontrak | API `0.19.0` §12.2, §12.7, §12.8; validation `0.18.0` §14 aturan 13–19, 23–25, 27–31; state `0.13.0` §11.5, §11.6; integration `0.10.0` §9.5, §9.6; `03-frontend-architecture.md` §17.10, §17.12 |
| Lingkungan | **Build Produksi (Standalone)**: Backend ASP.NET Core .NET 9 (`rizkiG`, build 0 error), Frontend Next.js Standalone (`RizkiV2`, build 0 error 503 halaman). Server frontend dijalankan dengan `npm run start` dari PowerShell (bukan `next dev`, tanpa lencana *N*). |
| Tanggal | 16 Oktober 2026 |

Panduan ini adalah instrumen pengujian terpadu *DevVerif* untuk membuktikan seluruh siklus transfer pasien IGD ke Rawat Inap, termasuk penolakan antrean oleh admisi dan penutupan kunjungan dua arah.

---

## 1. Aturan Pengujian

1. **Hak Akses**: Diberikan oleh pengurus Akses Role (bukan SuperAdmin):
   - Perawat IGD: `EmergencyAdmissionRequest:Read/Create/Update/Cancel`, `EmergencyDisposition:Execute`, `EmergencyVisit:Read`
   - Dokter IGD (`ranger.biru@admin.com`): `EmergencyAdmissionRequest:Read/Sign`, `DoctorConsultation:Read`
   - Petugas Admisi: `InpatientAdmissionTransfer:Read/Reject`, `InpatientEpisode:Create`, `InpatientBedOccupancy:Read/Create/Update`, `Doctor:Read`
2. **Kredensial**: Sandi tidak ditulis dalam skrip atau laporan; dibaca dari variabel lingkungan atau sesi login aktif.
3. **Bukti Mentah**: Seluruh tangkapan layar PNG dan log JSON disimpan di folder:
   `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b3-20261016/`
4. **Validitas Bukti**: Menggunakan peramban visual (Chromium headed), tanpa lencana "N", dan mencocokkan log backend `NewQuilvianSystemBackend/Logs/quilvian-backend-20261016.json`.
5. **Dilarang**: Jangan menggunakan pasien `4411cf93-fcca-49b5-8e63-1b08c15a9e32` (sisa jalur lama).

---

## 2. Prasyarat Data

| ID Pasien | Kondisi Awal | Tujuan Pengujian |
| --- | --- | --- |
| **P-B3-A** | Pasien IGD baru, DPJP IGD aktif = `ranger.biru@admin.com`, diagnosis ICD-10 ada, tindak lanjut rawat inap terkonfirmasi dengan unit bangsal rawat inap valid. | Jalur Regresi Utama (Alur normal sukses sampai pasien diadmisi dan transfer selesai). |
| **P-B3-B** | Pasien IGD baru, DPJP IGD aktif = `ranger.biru@admin.com`, diagnosis ICD-10 ada, tindak lanjut rawat inap terkonfirmasi. | Skenario Penolakan Admisi (`IGD-DEC-309`): Penolakan oleh admisi → Status `CancelledByAdmission` → Panduan Aturan 30 → Pembuatan Form Baru. |
| **P-B3-C** | Pasien IGD baru, DPJP IGD aktif = `ranger.biru@admin.com`, diagnosis ICD-10 ada, tindak lanjut rawat inap terkonfirmasi. | Skenario Penutupan Kunjungan IGD Dua Arah (`IGD-DEC-308`): Pasien diantar fisik lebih dahulu → Admisi memproses admisi → Evaluasi penutupan susulan otomatis menutup kunjungan IGD. |

---

## 3. Matriks Skenario Pengujian

### 3.1 Skenario Khusus Penolakan Admisi Rawat Inap (`IGD-DEC-309` · `FE-IGD-064` + `BE-IGD-077`) — Pasien P-B3-B

| Langkah | Aktor & Layar | Aksi Pengujian | Hasil yang Diharapkan (Acceptance Criteria) |
| ---: | --- | --- | --- |
| B3-01 | Perawat IGD (`SCR-IGD-P02`) | Buat form permintaan rawat inap P-B3-B, isi unit layanan, kelas, DPJP usulan, lama rawat, indikasi, CITO, lalu tekan *Minta TTD*. | Form berstatus `Menunggu TTD · CITO`. Tombol *Jalankan* tertahan dengan panduan admisi. |
| B3-02 | Dokter IGD (`ranger.biru`) (`SCR-IGD-D04`) | Buka *Ruang Kerja Dokter*, pilih tab/saringan *Perlu TTD*, buka dialog form P-B3-B, lalu tekan *Tandatangani*. | Form ditandatangani (`Signed`). Pasien masuk Antrean Transfer IGD/Poli. Butir *Perlu TTD* berkurang. |
| B3-03 | Petugas Admisi Rawat Inap (`InpatientAdmissionTransfer`) | Buka *Admisi Rawat Inap* → *Antrean Transfer IGD/Poli*. Temukan P-B3-B. Tekan tombol aksi **Tolak Permintaan**, masukkan alasan: *"Ruang rawat inap Melati penuh, alihkan ke isolasi"*, lalu konfirmasi penolakan. | Pasien berhasil ditolak. Permintaan hilang dari antrean transfer. API mengembalikan `200 OK`. Audit `AdmissionRejected (8)` tercatat di backend. |
| B3-04 | Perawat IGD (`SCR-IGD-P02`) | Buka kembali tab *Tindak Lanjut* pasien P-B3-B di layar pengkajian IGD. | **(Acceptance 1)** Lencana status form menampilkan `Dibatalkan admisi — Ruang rawat inap Melati penuh, alihkan ke isolasi` dengan tone merah tegas (`danger`).<br>**(Acceptance 2)** Teks deskripsi panel menampilkan: *"Ditolak oleh petugas admisi dengan alasan: Ruang rawat inap Melati penuh, alihkan ke isolasi"*.<br>**(Acceptance 3)** Tombol **"Buat Form Baru"** aktif dan siap ditekan.<br>**(Acceptance 4)** Tombol *Jalankan* tetap nonaktif dengan teks helper:<br>   *"Form permintaan rawat inap ditolak oleh petugas admisi (Ruang rawat inap Melati penuh, alihkan ke isolasi). Buat form permintaan baru sebelum pasien diproses."* |
| B3-05 | Perawat IGD (`SCR-IGD-P02`) | Tekan tombol **Lihat Form** pada form yang ditolak tersebut. | Form terbuka dalam mode baca-saja. Pada kelompok *Isian form*, baris *Alasan penolakan admisi* menampilkan *"Ruang rawat inap Melati penuh, alihkan ke isolasi"*. Pada seksi *Riwayat*, tercatat kejadian *"Ditolak petugas admisi"*. |
| B3-06 | Perawat IGD (`SCR-IGD-P02`) | Tekan tombol **Buat Form Baru**. Ubah unit layanan ke Unit Isolasi, lalu minta tanda tangan ulang ke DPJP IGD. | Draf form baru berhasil dibuat. Tombol *Jalankan* kembali ke status penahanan draf. Setelah DPJP menandatangani, form baru masuk kembali ke Antrean Transfer Rawat Inap. |

---

### 3.2 Skenario Penutupan Kunjungan IGD Dua Arah (`IGD-DEC-308` · `BE-IGD-077`) — Pasien P-B3-C

| Langkah | Aktor & Layar | Aksi Pengujian | Hasil yang Diharapkan |
| ---: | --- | --- | --- |
| B3-07 | Perawat & Dokter IGD | Lengkapi form transfer P-B3-C dan tandatangani. | Pasien masuk Antrean Transfer IGD/Poli. |
| B3-08 | Perawat IGD | Di tab *Transfer Pasien*, buat dokumen serah terima kepergian SBAR ke bangsal tujuan dan selesaikan seluruh pesanan/observasi. | Kunjungan IGD berada pada status siap tutup (`AwaitingClosure` atau `Disposed`). |
| B3-09 | Petugas Admisi Rawat Inap | Dari Antrean Transfer IGD/Poli, tekan *Proses Admisi* untuk P-B3-C, selesaikan langkah 2–10 (pembayaran, kelas, bed occupancy), dan finalkan transaksi admisi. | Transaksi admit berhasil di Rawat Inap. Disposisi IGD menjadi `Executed`. Fungsi `EvaluasiPenutupanKunjunganSusulanAsync` terpicu otomatis. |
| B3-10 | Perawat / Dokter IGD | Periksa status kunjungan IGD P-B3-C pada daftar kunjungan IGD. | Status kunjungan IGD otomatis berpindah menjadi `Completed` (Selesai), waktu penyelesaian tercatat, dan encounter pasien ditutup tanpa ada status menggantung. |

---

### 3.3 Skenario Regresi Jalur Utama Putaran B2 — Pasien P-B3-A

| Langkah | Aktor & Layar | Aksi Pengujian | Hasil yang Diharapkan |
| ---: | --- | --- | --- |
| B3-11 | Perawat IGD | Buat form transfer P-B3-A dengan CITO dan bantuan oksigen → Minta TTD. | Form berstatus `Menunggu TTD · CITO`. Tombol *Jalankan* nonaktif. |
| B3-12 | Dokter IGD | Buka Ruang Kerja Dokter → Perlu TTD → Buka Form → Tandatangani. | Form `Signed`. Pasien tampil di Antrean Transfer IGD/Poli dengan badge CITO. |
| B3-13 | Perawat IGD | Buka tab Tindak Lanjut P-B3-A → Lihat Form → Cetak Surat Pengantar Transfer. | Pratinjau cetak dan PDF memuat seluruh identitas, informasi klinis, rencana penempatan, dan blok tanda tangan elektronik WIB. |
| B3-14 | Petugas Admisi | Tekan *Proses Admisi* pada P-B3-A di Antrean Transfer, selesaikan seluruh langkah admisi. | Pasien berhasil diadmisi ke bangsal rawat inap. Tindak lanjut IGD otomatis menjadi `Executed`. Form IGD berstatus `Completed` (Selesai). |

---

## 4. Format Laporan Hasil Pengujian

Laporan hasil pengujian diisi pada berkas ini atau di `docs/module-blueprints/igd/perawat-igd/testing/test-by-agy/laporan-uji-putaran-b3-20261016.md` dengan tabel kesimpulan:

```markdown
| ID | Skenario | Hasil | Bukti Tangkapan Layar / Log |
| --- | --- | :---: | --- |
| B3-01 | Pengisian Form & Minta TTD (P-B3-B) | PASS | PNG `b3-01-minta-ttd.png` |
| B3-02 | Tanda Tangan DPJP IGD (P-B3-B) | PASS | PNG `b3-02-signed.png` |
| B3-03 | Penolakan Admisi oleh Petugas Admisi | PASS | PNG `b3-03-admisi-tolak.png`, Log backend `200 OK` |
| B3-04 | Tampilan Lencana & Helper Aturan 30 di IGD | PASS | PNG `b3-04-lencana-dibatalkan-admisi.png` |
| B3-05 | Rincian Alasan Penolakan pada Form | PASS | PNG `b3-05-detail-penolakan.png` |
| B3-06 | Buat Form Baru Pasca-Tolak | PASS | PNG `b3-06-form-baru.png` |
| B3-07..10 | Penutupan Kunjungan IGD Dua Arah (P-B3-C) | PASS | PNG `b3-10-kunjungan-completed.png` |
| B3-11..14 | Regresi Jalur Utama Putaran B2 (P-B3-A) | PASS | PNG `b3-14-admisi-selesai.png` |
```
