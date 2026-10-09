# ISSUE-010 — Penggunaan Formulir Inline Memadati Layar Penunjang Medis pada Konsultasi Gizi dan Bank Darah

```yaml
issue_id: ISSUE-KEP-010
module_id: rawat-inap
submodule: keperawatan
layar: "Ruang Kerja Keperawatan Rawat Inap — Penunjang Medis → Sub-tab Konsultasi Gizi & Bank Darah (FE-KEP-15 / FE-RWI-174)"
sumber_laporan: "Laporan pengguna 06-10-2026: 'Konsultasi Gizi dan bank darah gunakan tampilan modal untuk menambahkan (create)'"
tanggal_issue: "2026-10-06"
status: SELESAI
keparahan_tertinggi: Medium
source_sha_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
source_sha_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-010-modal-pemesanan-konsultasi-gizi-dan-bank-darah.md
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Ringkasan

Laporan pengguna meminta agar proses penambahan (*create*) pesanan pada sub-tab **Konsultasi Gizi** dan **Bank Darah** di menu Penunjang Medis Keperawatan dialihkan ke **tampilan modal**, selaras dengan pola pemesanan tindakan Hemodialisa (`HemodialysisOrderModal`).

Berdasarkan telaah kode pada `nursing-ancillary-section.jsx`, sebelumnya kedua sub-tab tersebut merender formulir pembuatan pesanan secara *inline* tepat di atas tabel riwayat pesanan (`SupportingNutritionForm` dan `SupportingBloodBankForm`). Akibatnya, setiap kali perawat membuka tab Konsultasi Gizi atau Bank Darah, layar langsung didominasi formulir isian yang tinggi dan lebar, mendorong tabel riwayat pesanan dan panel pemantauan transfusi darah ke bagian bawah layar. Tata letak ini tidak konsisten dengan sub-tab Hemodialisa yang langsung menyajikan tabel daftar pesanan bersih beserta tombol aksi `+ Pesan Hemodialisa` di pojok kanan atas yang memicu jendela modal pop-up.

Isu ini berstatus keparahan **Medium** karena formulir lama dapat berfungsi namun pengalaman antarmuka pengguna (*UI/UX ergonomics*) menjadi berat, memecah perhatian perawat yang ingin meninjau riwayat pesanan, dan tidak seragam antar sub-tab penunjang medis.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "Konsultasi Gizi dan bank darah gunakan tampilan modal untuk menambahkan (create)" | Konteks layar Penunjang Medis rawat inap yang telah berhasil menerapkan modal formulir pemesanan berukuran besar (`size="lg"`) pada Hemodialisa. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-KEP-010-01` | 1 | Formulir pembuatan asuhan gizi dirender *inline* permanen di atas tabel riwayat | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-KEP-010-01` |
| `ISS-KEP-010-02` | 1 | Formulir pemesanan darah BDRS dirender *inline* permanen dan menutupi riwayat transfusi | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-KEP-010-02` |
| `ISS-KEP-010-T1` | — | Tombol `+ Buat Pesanan Baru` pada `SupportingHistorySection` belum dipicu untuk membuka modal pop-up | `DESIGN_CHANGE` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-KEP-010-03` |

---

## 4. Rincian per Temuan

### ISS-KEP-010-01 — Formulir Pembuatan Asuhan Gizi Dirender Inline Permanen di Atas Tabel Riwayat

| | |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada `nursing-ancillary-section.jsx:137–144` |
| **Perbaikan** | `FIX-KEP-010-01` |

**Apa yang terjadi.**
Pada sub-tab Konsultasi Gizi, komponen `SupportingNutritionForm` dirender langsung di halaman. Ini menyebabkan formulir besar memakan ruang vertikal layar, sehingga daftar riwayat konsultasi gizi terdorong ke bawah.

**Rekomendasi.**
Ganti pemanggilan formulir *inline* dengan komponen modal berukuran besar `NutritionOrderModal` (`size="lg"`), yang dibuka saat pengguna menekan tombol `+ Buat Pesanan Baru` pada header `SupportingHistorySection`.

---

### ISS-KEP-010-02 — Formulir Pemesanan Darah BDRS Dirender Inline Permanen

| | |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada `nursing-ancillary-section.jsx:146–153` |
| **Perbaikan** | `FIX-KEP-010-02` |

**Apa yang terjadi.**
Pada sub-tab Bank Darah, komponen `SupportingBloodBankForm` dirender langsung di atas tabel riwayat darah dan panel pemantauan reaksi transfusi. Hal ini menyulitkan perawat dalam memantau tanda vital transfusi secara cepat.

**Rekomendasi.**
Ganti formulir *inline* dengan komponen modal `BloodBankOrderModal` (`size="lg"`), lengkap dengan pemilihan golongan darah, repeater komponen kantong darah, estimasi penjaminan (*coverage*), dan dialog konfirmasi duplikasi pesanan.

---

### ISS-KEP-010-T1 — Penyatuan Antarmuka Pemesanan Penunjang pada Ruang Kerja Dokter

| | |
| :--- | :--- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI pada `supporting-service-tab.jsx` |
| **Perbaikan** | `FIX-KEP-010-03` |

**Apa yang terjadi.**
Ruang kerja dokter pada tab riwayat (`activeSubTab === "history"`) sebelumnya mengalihkan sub-tab ke tab formulir penuh saat tombol `+ Buat Pesanan Baru` ditekan.

**Rekomendasi.**
Dukung pembukaan modal pop-up `NutritionOrderModal` dan `BloodBankOrderModal` langsung dari tampilan riwayat dokter, sembari tetap mempertahankan tab formulir penuh untuk kompatibilitas alur kerja dokter.

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa pemesanan Konsultasi Gizi dan Bank Darah lebih baik menggunakan modal dibandingkan ditaruh langsung di layar?
>
> **J:** Karena sebagian besar waktu perawat membuka tab penunjang medis adalah untuk memantau status pesanan (apakah diet sudah diverifikasi ahli gizi, atau apakah kantong darah dari BDRS sudah siap diambil). Meletakkan form permanen di atas tabel membuat perawat harus selalu menggulir layar (*scrolling*). Dengan modal, tampilan utama menjadi ringkas dan bersih, sementara aksi pembuatan pesanan baru dapat dipanggil kapan saja secara fokus via tombol `+ Buat Pesanan Baru`.

---

## 6. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-06 | Dokumen issue dibuat berdasarkan arahan pengguna; 2 temuan utama dan 1 temuan penyelarasan diselesaikan via implementasi modal `size="lg"`. Status: SELESAI. | `diagnose-module-issue` (Antigravity) |
