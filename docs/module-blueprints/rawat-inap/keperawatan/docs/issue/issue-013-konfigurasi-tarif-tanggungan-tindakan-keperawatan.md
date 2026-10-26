# ISSUE-013 — Konfigurasi Master Tarif Tindakan Universal, Status Tanggungan Penjamin, dan Eliminasi Label Tagihan Kasir pada Ruang Kerja Keperawatan

```yaml
issue_id: ISSUE-KEP-013
module_id: rawat-inap
submodule: keperawatan
layar: "Ruang Kerja Keperawatan Rawat Inap — Form Tindakan Medis (FE-KEP-14 / FE-RWI-089)"
sumber_laporan: "Laporan pengguna 20-10-2026: 3 butir (1 screenshot layar Tindakan Rawat Inap, instruksi pengisian tarif dummy universal semua kelas, persetujuan perapihan badge kolom status, dan aturan pelapor eliminasi teks 'perkiraan — tagihan final di kasir')"
tanggal_issue: "2026-10-20"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "6e0f2b1b (MHamzah)"
source_sha_frontend: "a491b05a (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-013-konfigurasi-tarif-tanggungan-tindakan-keperawatan.md
ditulis_dengan: "skill diagnose-module-issue"
bukti_database: "Analisis skema MstTariff, MstCompanyGuarantorCoverageRule, MstInsuranceCoverageRule pada database development QuilvianNewDevHamzah"
```

---

## 1. Ringkasan

Pelapor membuka menu **Tindakan** pada lembar kerja Dokumentasi Asuhan Keperawatan Rawat Inap untuk pasien rawat inap Kelas II. Pada daftar katalog tindakan, seluruh baris tindakan menampilkan lencana *"Status Tanggungan Belum Tersedia"* dan kolom tarif bertuliskan *"tarif belum tersedia KELAS II"*, padahal pada modul Resep Obat status tanggungan ("Ditanggung" / "Tidak Di-cover") dapat berfungsi normal.

Penelusuran membuktikan bahwa masalah ini bersumber dari **ketergantungan kaku (*hard-coupling*) di backend**: resolver tanggungan tindakan menggembok evaluasi aturan penjamin di balik ketersediaan tarif dasar rumah sakit. Karena data master tarif tindakan (`MstTariff`) untuk kelas kamar tersebut belum terisi di database, backend mengembalikan status tanggungan kosong (`isCovered = null`), sehingga frontend beralih ke teks cadangan (*fallback*). Selain itu, teks lencana status tanggungan terlipat vertikal per suku kata akibat kolom tabel yang sempit.

Pelapor menyetujui pengisian data buatan (*dummy*) universal untuk seluruh master tarif tindakan dan aturan penjaminnya, menyetujui perapihan tampilan badge status, serta menetapkan aturan pelapor baru: **keterangan *"perkiraan — tagihan final di kasir"* wajib dihilangkan dari lembar kerja dokter dan perawat** karena informasi tersebut merupakan ranah kasir/billing dan tidak perlu dilihat oleh tenaga medis.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "Coba anda analisis pada item tindakan ini tidak bisa menentukan status cover dan tidak cover, sedangkan pada resep itu bisa. tolong lakukan analisis kenapa masalah itu bisa terjadi" | Screenshot layar Form Tindakan Medis Rawat Inap: 4 baris tindakan (`PR-RSMMC-00025`, `00019`, `00027`, `00029`) dengan tarif "tarif belum tersedia KELAS II" dan badge "Status Tanggungan Belum Tersedia" |
| 2 | "kenapa tidak diisi saja master tarif tindakan tersebut di semua kelas , dengan data ala kadar ya aja seperti data buatan saja, lalu tindakan tersebut di konfigurasi , ? ... iya langkah ini saya setujui" | Persetujuan eksplisit penyiapan script SQL seed data tarif dummy universal dan perapihan lebar kolom status di frontend |
| 3 | "tulisan ini : perkiraan — tagihan final di kasir. mengenai tulisan ini juga note harus di hilangkan , karena seorang keperawatan dan dokter tidak perlu tahu dan kata kata ini dihilangkan" | Teks arahan pelapor (Aturan Pelapor) terkait eliminasi catatan tagihan kasir pada lembar klinis |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | :---: | :---: | --- | --- |
| `ISS-KEP-013-01` | 1, 2 | Status tanggungan dan tarif "belum tersedia" pada seluruh katalog tindakan medis akibat ketiadaan master tarif universal dan aturan coverage | `DATA` + `BUG` | Database + Backend | High | `SUDAH-VERIFIKASI` di source dan capture | `FIX-KEP-013-01`, `FIX-KEP-013-02` |
| `ISS-KEP-013-02` | 2 | Lencana status tanggungan terlipat vertikal per suku kata akibat lebar kolom 15% terlalu sempit dan ketiadaan gaya `nowrap` | `BUG` | Frontend | Low | `DARI-CAPTURE` + `SUDAH-VERIFIKASI` | `FIX-KEP-013-03` |
| `ISS-KEP-013-03` | 3 | Label keterangan "perkiraan — tagihan final di kasir" tampil pada antarmuka dokter dan keperawatan | `RULE_VIOLATION` (Aturan Pelapor) | Frontend + Backend | Medium | `SUDAH-VERIFIKASI` di source | `FIX-KEP-013-04` |

---

## 4. Rincian per Temuan

### ISS-KEP-013-01 — Status Tanggungan dan Tarif "Belum Tersedia" pada Seluruh Katalog Tindakan Medis

| | |
| --- | --- |
| **No. laporan** | 1, 2 |
| **Jenis** | `DATA` (Ketiadaan data master) + `BUG` (Gating kaku pada backend resolver) |
| **Area** | Database (PostgreSQL) + Backend (`InpAncillaryOrderAdapter.cs`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code dan capture pelapor |
| **Perbaikan** | `FIX-KEP-013-01` (Script SQL Seed) & `FIX-KEP-013-02` (Decoupling Evaluasi Tanggungan) |

**Apa yang terjadi.**
Pada layar Form Tindakan Medis Rawat Inap, seluruh tindakan medis (43 tindakan tersedia) menampilkan harga *"tarif belum tersedia KELAS II"* dan status *"Status Tanggungan Belum Tersedia"*. Dokter dan perawat tidak dapat mengetahui apakah tindakan yang hendak diinstruksikan masuk tanggungan penjamin pasien (Ditanggung) atau menjadi beban pribadi pasien (Tidak Di-cover).

**Kenapa terjadi.**
1. **Pada Modul Resep (Berjalan Normal):**
   * Endpoint `GET .../prescribing-drugs` membaca katalog obat yang langsung dipadukan dengan konteks kunjungan pasien (`EncounterInsuranceContext`).
   * Fungsi `FindDrugTariffAsync` ([`InsuranceCoverageService.cs:513-524`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs#L513-L524)) memiliki **Global Fallback**: jika tarif spesifik kelas kamar tidak ditemukan, sistem otomatis mengambil tarif global rumah sakit (`PatientClassId = null`). Hasil evaluasi penjamin mengembalikan boolean pasti (`isCovered: true` atau `isCovered: false`).
2. **Pada Modul Tindakan (Mengalami Masalah):**
   * Endpoint pemuatan master tindakan `GET .../patient-procedures/master-options` tidak membawa konteks pasien.
   * Frontend memanggil endpoint kedua `GET .../episodes/{episodeId}/ancillary-orders/coverage-status?itemType=Procedure` ([`nursing-procedure-order-panel.jsx:96`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/procedure/nursing-procedure-order-panel.jsx#L96)).
   * Di backend ([`InpAncillaryOrderAdapter.cs:64-79`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs#L64-L79)):
     ```csharp
     var result = await _coverage.ResolveProcedureAsync(episode.EncounterId, itemId, 1, DateTime.UtcNow, cancellationToken);
     if (result.IsValid && result.TariffId.HasValue)
     {
         item.IsCovered = result.IsCovered;
         item.Label = result.IsCovered ? "Ditanggung" : "Tidak Di-cover";
         ...
     }
     ```
   * Jika tarif tindakan di kelas pasien (`KELAS II`) belum ada di tabel `MstTariff`, maka `result.IsValid = false` dan `result.TariffId = null`.
   * Blok pengisian status tanggungan **dilewati**, sehingga `item.IsCovered` dibiarkan bernilai `null` dan `item.Label` tetap bernilai *"Tarif belum tersedia"*.
   * Frontend membaca `typeof coverageFor(item)?.isCovered === "boolean"` yang menghasilkan `false`, lalu menampilkan fallback teks: *"Status tanggungan belum tersedia"*.

**Apakah ini menyimpang dari desain?**
`DATA` dan `BUG`. Sistem dirancang untuk menyajikan kepastian status pertanggungan pasien ("Ditanggung" atau "Tidak Di-cover"). Ketiadaan data master tarif di database memicu kegagalan berantai (*cascading failure*) yang mematikan logika penjaminan.

**Dampak nyata.**
Perawat atau dokter ragu memesan tindakan karena tidak mengetahui konsekuensi biaya terhadap pasien. Selain itu, pesanan tindakan berisiko gagal disimpan saat tombol Simpan ditekan jika validasi server mensyaratkan tarif aktif.

**Rekomendasi.**
1. Siapkan dan jalankan script SQL seed data dummy universal (`PatientClassId = NULL`, `ClinicId = NULL`, `ServiceUnitId = NULL`) untuk seluruh 43 master tindakan medis agar tarif berlaku di semua kelas kamar rawat inap (`FIX-KEP-013-01`).
2. Masukkan aturan tanggungan penjamin dummy pada `MstCompanyGuarantorCoverageRule` dan `MstInsuranceCoverageRule` dengan status `Covered` (100%).
3. Lepaskan ketergantungan kaku pada `InpAncillaryOrderAdapter.cs` agar status tanggungan tetap dievaluasi meskipun tarif nominal belum tersedia (`FIX-KEP-013-02`).

---

### ISS-KEP-013-02 — Lencana Status Tanggungan Terlipat Vertikal per Suku Kata

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Frontend (`nursing-procedure-order-panel.jsx` & CSS) |
| **Keparahan** | Low |
| **Status bukti** | `DARI-CAPTURE` + `SUDAH-VERIFIKASI` di source code |
| **Perbaikan** | `FIX-KEP-013-03` |

**Apa yang terjadi.**
Pada screenshot yang dilampirkan pelapor, teks lencana pada kolom STATUS terlipat vertikal per suku kata:
```text
Status
Tangg
ungan
Belum
Tersed
ia
```
Tampilan ini sangat sempit, tidak rapi, dan merusak tata letak tabel katalog tindakan.

**Kenapa terjadi.**
1. Di [`nursing-procedure-order-panel.jsx:252`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/procedure/nursing-procedure-order-panel.jsx#L252), kolom STATUS didefinisikan dengan lebar kaku: `<th style={{ width: "15%" }}>STATUS</th>`.
2. Lencana menggunakan komponen `<StatusBadge>` di dalam tag `<td>` tanpa properti perataan teks atau penahan pemenggalan kata (`white-space: nowrap`), sehingga peramban web (*browser*) memecah paksa teks 32 karakter tersebut menjadi 6 baris vertikal.

**Apakah ini menyimpang dari desain?**
`BUG`. Standar UI Quilvian ([`ui-consistency-checklist.md`](file:///c:/Users/Admin/.gemini/config/rules/frontend/ui-consistency-checklist.md)) melarang teks lencana status terpotong atau terlipat vertikal per suku kata yang merusak keterbacaan (*readability*).

**Dampak nyata.**
Tabel terlihat cacat secara visual, menyulitkan perawat membaca status tanggungan dengan cepat, dan memperbesar tinggi baris tabel (*row height*) secara tidak proporsional.

**Rekomendasi.**
Perlebar kolom STATUS (misal menjadi 18% - 20%) dan tambahkan inline style / class `whiteSpace: "nowrap"` pada kontainer lencana status di baris tabel katalog (`FIX-KEP-013-03`).

---

### ISS-KEP-013-03 — Label Keterangan "perkiraan — tagihan final di kasir" Tampil pada Lembar Medis

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `RULE_VIOLATION` (Aturan Pelapor) / `DESIGN_CHANGE` |
| **Area** | Frontend (`inpatient-coverage-utils.js`) + Backend (`InpAncillaryOrderAdapter.cs`) |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code |
| **Perbaikan** | `FIX-KEP-013-04` |

**Apa yang terjadi.**
Pada tampilan tarif tindakan (dan keranjang tindakan), sistem menyertakan label teks pendamping:
`Rp 100.000 (perkiraan — tagihan final di kasir)`.
Pelapor memberikan instruksi tegas bahwa catatan atau tulisan ini **harus dihilangkan** dari lembar kerja dokter dan perawat rawat inap.

**Kenapa terjadi.**
Pada perbaikan terdahulu (`BE-RWI-163` dan `FE-RWI-176`), disepakati konstanta global `ESTIMATE_PRICE_LABEL = "perkiraan — tagihan final di kasir"` di [`inpatient-coverage-utils.js:1`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-coverage-utils.js#L1) dan disisipkan pada setiap formatting harga via `describeCoveragePrice` dan `describeCoverageTotal`.
Namun menurut evaluasi operasional pelapor, tenaga perawat dan dokter hanya membutuhkan nilai tarif murni; penegasan mengenai kasir bersifat administratif dan membingungkan praktisi klinis.

**Apakah ini menyimpang dari desain?**
`RULE_VIOLATION` terhadap aturan baru pelapor. Teks ini sebelumnya memang sengaja dipasang sesuai blueprint `BE-RWI-163`, namun pemilik kebutuhan secara resmi meminta perilaku ini diubah (*DESIGN_CHANGE*).

**Dampak nyata.**
Tampilan kolom tarif menjadi terlalu panjang dan memuat informasi kasir yang tidak relevan bagi ruang lingkup asuhan keperawatan.

**Rekomendasi.**
Hapus string teks `ESTIMATE_PRICE_LABEL` dari fungsi `describeCoveragePrice` dan `describeCoverageTotal` di frontend, serta kosongkan `PriceLabel` pada respons DTO backend agar hanya nominal murni (misal: `Rp 100.000`) yang ditampilkan (`FIX-KEP-013-04`).

---

## 5. Tanya-Jawab Pelapor

> **T:** Kenapa pada item tindakan ini tidak bisa menentukan status cover dan tidak cover, sedangkan pada resep itu bisa?
>
> **J:** Masalah ini terjadi karena pada modul Resep Obat, pengecekan tanggungan dan pencarian obat digabung menjadi satu alur dengan fitur *tarif global otomatis* sehingga status penjaminan selalu terisi boolean pasti (`true` / `false`). Sementara pada modul Tindakan, penentuan status tanggungan digembok di balik ketersediaan tarif dasar rumah sakit. Karena data master tarif tindakan di kelas kamar pasien belum terisi di database, backend mengembalikan status tanggungan kosong (`null`), sehingga antarmuka menampilkan teks cadangan *"Status tanggungan belum tersedia"*.

> **T:** Kenapa tidak diisi saja master tarif tindakan tersebut di semua kelas dengan data ala kadar ya aja seperti data buatan saja, lalu tindakan tersebut dikonfigurasi?
>
> **J:** Pendekatan ini sangat tepat dan disetujui. Dengan mengisi data master tarif universal rumah sakit (`PatientClassId = NULL`) untuk seluruh 43 tindakan medis beserta aturan tanggungan penjamin (100% Ditanggung) di database development, seluruh tindakan akan langsung memiliki nominal tarif yang sah dan status penjaminan akan otomatis berubah menjadi "Ditanggung" di seluruh kelas kamar.

---

## 6. Temuan Tambahan

| ID | Judul | Area | Keparahan | Status bukti |
| --- | --- | :---: | :---: | --- |
| `ISS-KEP-013-T1` | Sinkronisasi Paritas Tampilan dengan Lembar Dokter (`procedure-form-panel.jsx`) | Frontend | Low | `SUDAH-VERIFIKASI` di source code |

* **Kenapa dicantumkan:** Lembar kerja dokter ([`procedure-form-panel.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-form-panel.jsx)) memiliki struktur tampilan katalog dan pembacaan utilitas yang 100% identik dengan lembar keperawatan. Perubahan lebar kolom dan penghapusan teks label kasir wajib diterapkan serentak agar tidak timbul disparitas antarmuka antara dokter dan perawat.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor / Pemilik:
| No | Pertanyaan | Status Keputusan | Catatan |
| :---: | --- | :---: | --- |
| 1 | Apakah tarif buatan disetujui menggunakan nilai dummy proporsional (misal: Rp 25.000 s.d. Rp 350.000) yang berlaku universal di semua kelas? | **DISETUJUI** | Diputuskan oleh pengguna pada 20-10-2026. |
| 2 | Apakah teks label *"perkiraan — tagihan final di kasir"* dihilangkan sepenuhnya dari lembar tindakan dokter dan perawat? | **DISETUJUI** | Diputuskan oleh pengguna pada 20-10-2026 (Aturan Pelapor). |

---

## 8. Catatan Pola

Masalah ini memiliki kemiripan pola dengan `ISSUE-DOK-005` terdahulu pada modul Dokter Rawat Inap. Pola pencegahan yang direkomendasikan adalah:
1. Menyediakan script migrasi/seeder awal otomatis untuk data master dasar (`MstTariff`) dengan skema universal `PatientClassId = NULL` untuk setiap entitas layanan baru (tindakan, penunjang, alat).
2. Menghindari pemaksaan gembok tarif (*tariff-gating*) pada pemeriksaan hak tanggungan klinis.

---

## 9. Riwayat Dokumen

| Tanggal | Versi | Penulis | Keterangan |
| --- | :---: | --- | --- |
| 20-10-2026 | 1.0.0 | AI Agent (`diagnose-module-issue`) | Dokumen awal diterbitkan berdasarkan laporan pengguna, audit source code, dan persetujuan langkah perbaikan. |
