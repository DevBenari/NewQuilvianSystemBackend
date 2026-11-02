# PLAN-REPAIR-034 — Penyajian Kartu Identitas, Nomor Rekam Medis Bayi, dan Ringkasan Admisi pada Langkah 7 (Dokumen Bayi)

```yaml
plan_id: PLAN-REPAIR-EPS-034
issue: ../issue/issue-034-kartu-identitas-dan-no-rm-bayi-langkah-dokumen.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-22"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-22"
tanggal_selesai: "2026-10-22"
basis_source_backend: "6406fc76 (MHamzah)"
basis_source_frontend: "381ec25fa (HamzahV2)"
target_database: "QuilvianNewDevHamzah"
```

Dokumen ini memuat rencana perbaikan antarmuka untuk menyajikan kartu identitas pasien bayi lengkap dengan Nomor Rekam Medis (No. RM) resmi yang diterbitkan server pada Langkah 7 Dokumen Bayi.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :--- | :---: | :--- |
| `FIX-EPS-034-01` | `ISS-EPS-034` | Penambahan kartu identitas pasien bayi & No. RM resmi serta banner konfirmasi penerbitan rekam medis pada komponen `NewbornDocumentsStep` | Frontend | P1 (Tinggi) | `FIX-EPS-034` | ✅ SELESAI | `inpatient-newborn-admission-steps.jsx` diperbarui; kartu identitas bayi & No. RM terverifikasi tampil; 15/15 unit test lolos |

**Ringkasan: 1 dari 1 perbaikan selesai (100%).**

---

## 2. Skema Tampilan Sebelum vs Sesudah

```carousel
### Tampilan Sebelum (Hanya Waktu Layanan — No. RM Tidak Tampak)
```text
[LANGKAH 7 DARI 7 — Dokumen Bayi]
Heading: Dokumen Bayi
+-------------------------------------------------------------------------------+
| WAKTU LAYANAN                                                                 |
| • Waktu lahir       : 2026-10-22 10:51:00                                     |
| • Draft admisi dibuat : 22 Okt 2026, 10.53                                    |
| • Penempatan aktif  : 22 Okt 2026, 10.58                                      |
+-------------------------------------------------------------------------------+
[Gelang Bayi]  [Label Bayi]  [Lihat / Cetak Persetujuan Wali]
[Pratinjau Gelang di bawah lipatan layar...]
```
<!-- slide -->
### Tampilan Sesudah (Skema Terbaik — Lengkap dengan Kartu No. RM & Identitas)
```text
[LANGKAH 7 DARI 7 — Dokumen Bayi]
Heading: Dokumen Bayi

+-------------------------------------------------------------------------------+
| [SUCCESS ALERT] Admisi Bayi Aktif — No. Rekam Medis Resmi Diterbitkan        |
| Bayi telah resmi terdaftar dengan No. RM di bawah dan siap cetak dokumen.     |
+-------------------------------------------------------------------------------+

+-------------------------------------------------------------------------------+
| IDENTITAS PASIEN BAYI & REKAM MEDIS                    [Badge: RM 00-00-00-47]|
| • No. Rekam Medis (RM) : 00-00-00-47 (Tebal / Biru Primer)                   |
| • Nama Pasien Bayi     : By. Ny. DEWI ANGGRAINI                              |
| • Urutan Kelahiran     : Bayi ke-6                                           |
| • Jenis Kelamin & Lahir: Laki-laki · 22 Okt 2026, 10:51 WIB                  |
| • No. Episode Admisi   : RI-261022035305-26C199                              |
| • Ibu Kandung          : Ny. DEWI ANGGRAINI (RM 00-00-00-38)                 |
| • Kasus Persalinan     : Kasus Terpilih (VK / OK)                            |
| • Ruang & Bed Rawat    : Kamar BAYI-1 · Bed BD-RSMMC-BY1-01 (Ruang Bayi)     |
+-------------------------------------------------------------------------------+

+-------------------------------------------------------------------------------+
| WAKTU LAYANAN                                                                 |
| • Waktu lahir       : 2026-10-22 10:51:00                                     |
| • Draft admisi dibuat : 22 Okt 2026, 10.53                                    |
| • Penempatan aktif  : 22 Okt 2026, 10.58                                      |
+-------------------------------------------------------------------------------+

[Gelang Bayi]  [Label Bayi]  [Lihat / Cetak Persetujuan Wali]
[Panel Cetak Gelang Bayi & Riwayat Cetak...]

[Buka Dokumen Admisi]  [Selesai]
```
````

---

## 3. Rincian Perubahan Kode Frontend

Pada `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`:
- Tambahkan prop `newborn` pada `NewbornDocumentsStep({ patient, confirmation, doctor, flow, newborn })`.
- Ekstrak data No. RM dari `patient.selectedPatient?.medicalRecordNumber || episode?.medicalRecordNumber`.
- Render `InformationAlert` sukses di atas kartu.
- Render `BaseDetailCard` identitas pasien bayi sebelum kartu waktu layanan.

---

## 4. Acceptance Criteria & Verifikasi

1. **Acceptance Criteria**:
   - Langkah 7 ("Dokumen Bayi") menampilkan kartu "Identitas Pasien Bayi & Rekam Medis" dengan No. RM bayi (`00-00-00-47`), nama bayi, nama ibu, dan nomor episode.
   - Peringatan sukses mengonfirmasi nomor rekam medis terdaftar secara resmi.
   - Seluruh pengujian unit otomatis pada `tests/unit/inpatient-newborn-admission.test.mjs` tetap lulus 100%.

---

## 5. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-22 | Dokumen rencana perbaikan dibuat untuk menyajikan kartu No. RM bayi di Langkah 7. | `diagnose-module-issue` |
| 2026-10-22 | Perbaikan selesai diterapkan dan diverifikasi (100% SELESAI). 15/15 unit test otomatis lulus. | `build-module-frontend` |
