# ISSUE-035 — Penambahan Langkah Mandiri (Step 8: Kartu Pasien) pada Alur Admisi Bayi Baru Lahir

```yaml
issue_id: ISSUE-EPS-035
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 8 Kartu Pasien Bayi (FE-INP-027)"
sumber_laporan: "Permintaan pengguna 22 Oktober 2026: 'apakah bisa buat step sendiri untuk kartu , seperti pada pendaftaran pasien lama atau baru , mengikuti tampilan yang sudah ada, untuk dokumen bayi tetap seperti itu' — Pengguna menyetujui pemisahan kartu pasien bayi ke dalam dedicated step terpisah (Langkah 8) mengikuti pola standar pendaftaran pasien baru rawat inap."
tanggal_issue: "2026-10-22"
status: SELESAI
tanggal_selesai: "2026-10-22"
keparahan_tertinggi: Sedang
source_sha_backend: "6406fc76 (MHamzah)"
source_sha_frontend: "381ec25fa (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-035-penambahan-langkah-kartu-pasien-admisi-bayi.md
```

---

## 1. Ringkasan

Pada alur pendaftaran pasien baru rawat inap (`INPATIENT_ADMISSION_NEW_PATIENT_STEPS`), langkah penutup admisi menyediakan tahapan terdedikasi **"Kartu Pasien"** yang merender kartu visual terstandar rumah sakit (`BasePatientCard`) dilengkapi tombol cetak kartu, navigasi ke Papan Tempat Tidur, serta tombol Admisi Baru.

Sebaliknya, pada alur admisi bayi baru lahir (`NEWBORN_ADMISSION_STEPS`), sebelumnya alur berhenti pada Langkah 7 ("Dokumen Bayi"). Dokumen Bayi berfokus pada percetakan gelang bayi, label bayi, dan surat persetujuan wali. Pengguna dan staf rumah sakit menghendaki kartu fisik pasien bayi baru lahir juga dapat ditampilkan dan dicetak dalam sebuah **langkah mandiri khusus**, persis seperti alur pendaftaran pasien dewasa baru, tanpa mengganggu atau mengubah tata letak dokumen bayi yang telah ada di Langkah 7.

---

## 2. Analisis Kebutuhan Bisnis & Pengguna

1. **Pemisahan Konteks Kerja Petugas**:
   - **Langkah 7 (Dokumen Bayi)**: Berfungsi untuk penyiapan kelengkapan klinis identitas fisik internal bangsal/ruang rawat (Gelang Pengenal Bayi Biru/Merah Muda, Label Botol/Spesimen Laboratorium, dan Surat Persetujuan Orang Tua/Wali).
   - **Langkah 8 (Kartu Pasien)**: Berfungsi untuk pencetakan kartu tanda pengenal rekam medis resmi pasien (`BasePatientCard`) yang diserahkan kepada orang tua/keluarga pasien bayi saat pulang atau sebagai arsip keluarga.

2. **Konsistensi UI/UX Lintas Modul**:
   - Mengikuti pola baku modul rawat inap yang telah lulus audit (`InpatientAdmissionPatientCardStep` dan `BasePatientCard`).
   - Stepper otomatis menghitung 8 tahapan (`Langkah 8 dari 8: Kartu Pasien`).

---

## 3. Titik Integrasi Teknis Frontend

1. **Konstanta Langkah (`inpatient-newborn-admission-constants.js`)**:
   - Menambahkan `{ slug: "patient-card", title: "Kartu Pasien" }` pada array `NEWBORN_ADMISSION_STEPS` sehingga total langkah menjadi 8.
2. **Navigasi Langkah 7 (`inpatient-newborn-admission-steps.jsx`)**:
   - Tombol utama Langkah 7 diarahkan untuk memanggil `flow.goNext` dengan label *"Lanjut ke Kartu Pasien"*.
   - Menyediakan tombol *"Kembali ke Konfirmasi"* (`flow.goBack`) dan *"Buka Dokumen Admisi"* (`buildInpatientAdmissionWorkspaceRoute`).
3. **Penyajian Langkah 8 (`inpatient-admission-view.jsx`)**:
   - Menerima `stepSlug === "patient-card"` saat `newborn?.enabled` aktif.
   - Mengompilasi data identitas bayi (`babyCardPatient`) dari gabungan `patient.selectedPatient`, `confirmation.episode`, dan konteks ibu bayi.
   - Merender `InpatientAdmissionPatientCardStep` dengan aksi cetak (`window.print()`), navigasi Papan Tempat Tidur (`/bed-board`), dan tombol *"Admisi Baru"* yang mereset status alur bayi dengan aman.
4. **Resiliesi Pelanjutan URL (`useInpatientAdmissionResume`)**:
   - Mengizinkan pembukaan langsung rute `step=patient-card` pada episode bayi yang berstatus aktif (`allowAdmitted`).
