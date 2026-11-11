# Laporan Task FE-HRD-212: Kalkulator Rapor Komposit 3 Tahun Rekredensialing & Visualisasi Tier

## 1. Informasi Task
* **Task ID**: `FE-HRD-212`
* **Slice / MVP**: `S-C1-OPPE / MVP-47`
* **Nama Fitur**: Kalkulator Rapor Komposit 3 Tahun Rekredensialing & Visualisasi Tier (Layar FE-HRD-207)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Mengimplementasikan modul kalkulator evaluasi mutu komposit 3 tahunan bagi seluruh dokter yang mendekati jatuh tempo perpanjangan SPK-RKK 3 tahun:
1. **Algoritma Perhitungan Bobot Komposit 3 Tahun (`HRD-DEC-156`, `VR-OPP-18`)**:
   * Tahun ke-1: Bobot 25% dari skor OPPE tahun bersangkutan.
   * Tahun ke-2: Bobot 35% dari skor OPPE tahun bersangkutan.
   * Tahun ke-3: Bobot 40% dari skor OPPE tahun berjalan.
   * Rumus: `Komposit = (0.25 * Y1) + (0.35 * Y2) + (0.40 * Y3)`.
2. **Visualisasi Tier Kelayakan Otomatis**:
   * **Tier Hijau (Fast-Track Renewal)**: Komposit ≥ 85 dan 0 insiden sentinel -> Berhak perpanjangan kewenangan kilat tanpa perlu sidang pleno ulang.
   * **Tier Kuning (Klarifikasi KSM)**: Komposit 70 - 84.9 -> Memerlukan konfirmasi logbook dan verifikasi berkas oleh KSM.
   * **Tier Merah (Investigasi Pleno / FPPE)**: Komposit < 70 atau tercatat memiliki insiden sentinel berat -> Wajib audit investigasi mendalam oleh Komite Medik.
3. **Penyajian Data Riwayat Komparatif**: Tabel perbandingan capaian 3 tahun untuk audit komprehensif KARS.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/oppe/recred-composite-scorecard-view.jsx`
* `QuilvianSystemFrontendDev/src/app/corporate/hrd/credentialing/recred-composite-scorecard/page.jsx`
* `QuilvianSystemFrontendDev/src/app/admin/credentialing/recred-composite-scorecard/page.jsx`

## 4. Bukti Verifikasi
* Algoritma kalkulasi komposit menghasilkan nilai presisi sesuai kriteria `AT-HRD-OPP-19`, `AT-HRD-OPP-20`, dan `UAT-OPP-05`.
* Penentuan kartu tier menampilkan rekomendasi alur rekredensialing secara visual dan jelas.
* Audit linter CI/CD (`npm run lint:errors`): 0 error.
