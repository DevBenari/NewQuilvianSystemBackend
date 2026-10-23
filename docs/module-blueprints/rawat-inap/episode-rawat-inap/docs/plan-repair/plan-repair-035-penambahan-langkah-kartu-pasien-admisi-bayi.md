# PLAN-REPAIR-035 — Penambahan Langkah Mandiri (Step 8: Kartu Pasien) pada Alur Admisi Bayi Baru Lahir

```yaml
plan_id: PLAN-REPAIR-EPS-035
issue: ../issue/issue-035-penambahan-langkah-kartu-pasien-admisi-bayi.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-22"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-22"
tanggal_selesai: "2026-10-22"
basis_source_backend: "6406fc76 (MHamzah)"
basis_source_frontend: "381ec25fa (HamzahV2)"
target_database: "QuilvianNewDevHamzah"
```

Dokumen ini memuat rencana implementasi penambahan dedicated step Langkah 8 **"Kartu Pasien"** pada alur admisi bayi baru lahir, menggunakan komponen baku `BasePatientCard` dan layout `InpatientAdmissionPatientCardStep`.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :--- | :---: | :--- |
| `FIX-EPS-035-01` | `ISS-EPS-035` | Menambahkan slug `patient-card` ("Kartu Pasien") ke dalam `NEWBORN_ADMISSION_STEPS` (menjadi 8 langkah) | Frontend | P1 (Tinggi) | `FIX-EPS-035` | ✅ SELESAI | `inpatient-newborn-admission-constants.js` memuat 8 langkah terurut |
| `FIX-EPS-035-02` | `ISS-EPS-035` | Mengubah tombol navigasi bawah Langkah 7 menjadi `Kembali ke Konfirmasi`, `Buka Dokumen Admisi`, dan `Lanjut ke Kartu Pasien` (`flow.goNext`) | Frontend | P1 (Tinggi) | `FIX-EPS-035` | ✅ SELESAI | `inpatient-newborn-admission-steps.jsx` diperbarui; navigasi antar-langkah mulus |
| `FIX-EPS-035-03` | `ISS-EPS-035` | Merender `InpatientAdmissionPatientCardStep` pada alur bayi dengan fallback data identitas tangguh dan tombol reset alur | Frontend | P1 (Tinggi) | `FIX-EPS-035` | ✅ SELESAI | `inpatient-admission-view.jsx` merender `BasePatientCard` untuk bayi dan mengizinkan resume episode aktif |
| `FIX-EPS-035-04` | `ISS-EPS-035` | Memperbarui test otomatis unit untuk alur 8 langkah dan menambahkan verifikasi Langkah 8 | Test | P1 (Tinggi) | `FIX-EPS-035` | ✅ SELESAI | `inpatient-newborn-admission.test.mjs` 16/16 test lulus 100% |

**Ringkasan: 4 dari 4 perbaikan selesai (100%).**

---

## 2. Skema Tampilan Alur Baru

```carousel
### Langkah 7 dari 8: Dokumen Bayi (Tetap & Terhubung)
```text
[LANGKAH 7 DARI 8 — Dokumen Bayi]
Heading: Dokumen Bayi

+-------------------------------------------------------------------------------+
| [SUCCESS ALERT] Admisi Bayi Aktif — No. Rekam Medis Resmi Diterbitkan        |
| Bayi telah resmi terdaftar dengan No. RM di bawah dan siap cetak dokumen.     |
+-------------------------------------------------------------------------------+

+-------------------------------------------------------------------------------+
| IDENTITAS PASIEN BAYI & REKAM MEDIS                    [Badge: RM 00-00-00-47]|
| • No. Rekam Medis (RM) : 00-00-00-47 (Tebal / Biru)                           |
| • Nama Pasien Bayi     : By. Ny. DEWI ANGGRAINI                              |
| • Urutan Kelahiran     : Bayi ke-6                                            |
| • No. Episode Admisi   : RI-261022035305-26C199                               |
| • Ibu Kandung          : Ny. DEWI ANGGRAINI (RM 00-00-00-01)                  |
| • Unit / Layanan       : Ruang Perinatologi / Kamar BAYI-1                    |
+-------------------------------------------------------------------------------+

[Gelang Bayi]  [Label Bayi]  [Lihat / Cetak Persetujuan Wali]
[Preview Dokumen Gelang / Label...]

[Kembali ke Konfirmasi]    [Buka Dokumen Admisi]    [Lanjut ke Kartu Pasien ->]
```
<!-- slide -->
### Langkah 8 dari 8: Kartu Pasien (Dedicated Step)
```text
[LANGKAH 8 DARI 8 — Kartu Pasien]
Heading: Cetak Kartu Pasien
Sub-heading: Cetak kartu pasien yang baru didaftarkan, lalu serahkan kepada keluarga.

+-------------------------------------------------------------------------------+
| [SUCCESS ALERT] Admisi selesai disiapkan                                      |
| Episode RI-261022035305-26C199 menunggu konfirmasi kedatangan di Papan        |
| Tempat Tidur.                                                                 |
+-------------------------------------------------------------------------------+

+-------------------------------------------------------------------------------+
| [KARTU FISIK PASIEN MMC - BasePatientCard]                                    |
| [LOGO RS MMC]                                           KARTU PASIEN MMC     |
|                                                                               |
|  Nama Pasien  : By. Ny. DEWI ANGGRAINI                  +------------------+  |
|  No. RM       : 00-00-00-47                             | [QR CODE RESMI]  |  |
|  Tgl Lahir    : 22 Oktober 2026                         | No. RM Terenkripsi| |
|  Jenis Kelamin: Perempuan                               +------------------+  |
|  Status       : Aktif                                                         |
|  Diterbitkan  : 22 Okt 2026                                                   |
+-------------------------------------------------------------------------------+

[INFO ALERT] Kartu pasien dapat dicetak ulang sewaktu-waktu dari menu rekam medis.

[Kembali]    [Cetak Kartu]    [Buka Papan Tempat Tidur]    [Admisi Baru]
```
````

---

## 3. Verifikasi Bukti Otomatis

Pengujian unit node test runner dijalankan pada berkas `tests/unit/inpatient-newborn-admission.test.mjs`:

```text
✔ FE-RWI-257 / FIX-EPS-028: opsi kartu Admisi Bayi Baru Lahir aktif pada menu utama (1.0235ms)
✔ FE-RWI-257: kandidat ibu memakai endpoint kasus persalinan (0.1094ms)
✔ FE-RWI-257 & FIX-EPS-035: alur bayi tepat delapan langkah dengan kartu pasien tanpa penjamin dan deposit pada mode newborn (1.7497ms)
✔ FE-RWI-258: identitas bayi mewajibkan kasus, tanggal, jam, dan jenis kelamin (18.782ms)
✔ FE-RWI-258: payload bayi tidak mengirim nama atau urutan manual (0.3217ms)
✔ FE-RWI-259: konteks episode tidak membawa keputusan invoice/deposit (0.1871ms)
✔ FE-RWI-259: rawat gabung wajib episode ibu dan semua perawatan wajib DPJP (0.2809ms)
✔ FE-RWI-259: query bed mempertahankan konteks bayi sebelum episode dibuat (0.2632ms)
✔ FE-RWI-259: tampilan bayi tidak menyediakan input invoice/deposit (1.1908ms)
✔ FIX-EPS-030: validasi rawat gabung mendeteksi kamar penempatan ibu closed (1.0905ms)
✔ FIX-EPS-030: hook bed mendukung pemuatan denah mandiri dan selektivitas kamar bayi (0.9641ms)
✔ FIX-EPS-031: kartu pilihan jenis perawatan bayi menyediakan 3 opsi eksplisit (Rawat Gabung, Perinatologi, NICU) (0.1717ms)
✔ FIX-EPS-031: komponen NewbornCareStep merender 3 kartu dan meniadakan centang intensif tersembunyi (0.9754ms)
✔ FIX-EPS-033: NewbornPlacementStep merender pemilihan bed jika bed belum dipilih dan menangani kembali internal (0.8863ms)
✔ FIX-EPS-034: NewbornDocumentsStep merender kartu identitas bayi dan nomor rekam medis serta alert sukses (0.7363ms)
✔ FIX-EPS-035: NewbornDocumentsStep menyediakan tombol Lanjut ke Kartu Pasien dan view merender InpatientAdmissionPatientCardStep untuk bayi (1.4784ms)
ℹ tests 16
ℹ suites 0
ℹ pass 16
ℹ fail 0
```
