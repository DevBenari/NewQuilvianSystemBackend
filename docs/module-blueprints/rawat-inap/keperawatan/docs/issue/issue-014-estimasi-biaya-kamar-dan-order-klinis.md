# ISSUE-014 — Tarif Kamar Berstatus "Tarif Belum Ada" dan Ketiadaan Rincian Pesanan Klinis (Tindakan, Obat, Penunjang) pada Estimasi Biaya Pasien

```yaml
issue_id: ISSUE-KEP-014
module_id: rawat-inap
submodule: keperawatan
layar: "Ruang Kerja Keperawatan & Admisi Rawat Inap — Estimasi Biaya Pasien (FE-KEP-23 / FE-RWI-185)"
sumber_laporan: "Laporan pengguna 20-10-2026: 2 butir keluhan (tidak muncul harga kamar, dan kenapa tindakan/obat/penunjang tidak muncul di estimasi biaya setelah checkout) disertai 2 tangkapan layar antarmuka"
tanggal_issue: "2026-10-20"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd (MHamzah)"
source_sha_frontend: "a491b05afcb6b3ed70218ccaa557007719d45ac3 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-014-estimasi-biaya-kamar-dan-order-klinis.md
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Ringkasan

Pengguna menyampaikan dua keluhan utama pada layar **Estimasi Biaya Pasien** di dalam modul Rawat Inap (yang diakses melalui menu Dokumen Admisi maupun Ruang Kerja Keperawatan Bangsal). Pertama, kamar perawatan pasien (ODC 321B) tidak memunculkan harga satuan dan total harga (hanya bertanda `-`), serta berstatus *"Tarif Belum Ada"* sehingga subtotal kamar tidak dapat dihitung. Kedua, saat dokter atau perawat telah menginput pesanan klinis ("melakukan *checkout*") seperti Resep Obat, Tindakan Medis, dan Penunjang Medis (Laboratorium/Radiologi), pesanan-pesanan tersebut tidak muncul pada tabel Estimasi Biaya Pasien.

Penelusuran mendalam terhadap kode sumber backend, frontend, dan arsitektur domain rumah sakit Quilvian membuktikan bahwa:
1. **Masalah Tarif Kamar (Butir 1):** Merupakan masalah data master (`DATA`). Mesin penagihan rawat inap (`BillingCalculationService.CalculateRoomChargeAsync`) mencari tarif aktif di tabel `MstTariff` yang memiliki bendera `IsRoomCharge = true`, unit layanan yang sesuai (`placement.ServiceUnitId`), dan kelas pasien yang sesuai (`placement.PatientClassId` / Kelas ODC). Karena master tarif kamar untuk kombinasi unit dan kelas tersebut belum didaftarkan di master data rumah sakit, sistem secara sah menandai baris sebagai `TARIFF_NOT_FOUND` ("Tarif Belum Ada").
2. **Ketiadaan Tindakan, Obat, dan Penunjang (Butir 2):** Bukan disebabkan oleh bug kerusakan sistem (*Bukan Software Bug*), melainkan merupakan **aturan keselamatan dan integritas penagihan rumah sakit (Hospital Clinical Governance & Billing Integration Invariant)** yang sengaja dirancang:
   - Resep obat yang dibuat dokter pada tangkapan layar masih berstatus **"Draf"** (*Menunggu Finalisasi Klinis*) dan belum diserahkan oleh instalasi Farmasi. Tagihan obat hanya timbul saat Farmasi melakukan penyerahan fisik (*Dispensing*) atau dicatat pemakaiannya di bangsal.
   - Tindakan medis baru menjadi tagihan setelah tindakan selesai dieksekusi (*Completed*) oleh tenaga medis, bukan saat baru dipesan.
   - Penunjang medis (Lab/Radiologi) baru menagih saat sampel dinyatakan layak periksa oleh analis laboratorium atau saat akuisisi citra radiologi selesai dikerjakan.
   - Layar Estimasi Biaya memiliki aturan *Acceptance Criteria* (AC-4) di mana kelompok layanan tanpa baris tagihan aktif tidak akan ditampilkan.
3. Namun demikian, ketiadaan informasi edukatif mengenai siklus hidup layanan klinis pada antarmuka Estimasi Biaya menyebabkan kebingungan operasional (*UI/UX Communication Gap*), di mana pengguna mengira sistem belanja e-commerce yang langsung menagihkan barang saat dipesan di keranjang.

Isu ini diklasifikasikan dengan keparahan tertinggi **High** karena ketiadaan tarif kamar menahan kalkulasi subtotal biaya rawat inap pasien dan menghambat estimasi kelayakan kepulangan/deposit.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "tidak muncul Harga kamar ya" | Tangkapan layar antarmuka Estimasi Biaya Pasien (`media_1792469433018_72136ef2.png`) yang memperlihatkan baris kamar `ODC 321B`, jumlah `1`, satuan `unit tarif kamar`, harga satuan `-`, total harga `-`, waktu `19/10/2026 04.24 - berjalan`, status `Tarif Belum Ada`, dan subtotal `Subtotal belum dapat dihitung`. |
| 2 | "lalu kenapa pada saat dokter atau perawat ,melakukan checkout apapun itu seperti contoh : Tindakan Obat Penunjang Medis dan lain lain tidak muncul di estimasi biaya ya" | Tangkapan layar antarmuka Pengelolaan Resep Rawat Inap tab History Resep (`media_1792469394082_5689b32c.png`) yang memperlihatkan nomor resep `RX-20261020-00001`, dokter `dr. Rendy Pangalila`, jumlah `1 Item`, status resep `Draf`, farmasi `Menunggu Finalisasi Klinis`, serta banner informasi *"Draft resep berhasil disimpan."* |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-KEP-014-01` | 1 | Tarif kamar ODC 321B berstatus "Tarif Belum Ada" akibat ketiadaan Master Data Tarif kamar aktif pada kelas/unit terkait | `DATA` | Backend / Master Data | High | SUDAH-VERIFIKASI di source (`BillingCalculationService.cs:728-764`) + DARI-CAPTURE | `FIX-KEP-014-01` |
| `ISS-KEP-014-02` | 2 | Pesanan klinis (Obat, Tindakan, Penunjang) tidak muncul di Estimasi Biaya karena belum mencapai *Billing Milestone* (masih Draf / belum dieksekusi / belum diserahkan) | `DESIGN_CHANGE` / Aturan Bisnis | Backend & Frontend | Medium | SUDAH-VERIFIKASI di source (`PrescriptionDispensingService.cs:389`, `PatientProcedureExecutionService.cs:61`, `nursing-billing-section.jsx:38`) + DARI-CAPTURE | `FIX-KEP-014-02` |
| `ISS-KEP-014-03` | 2 | Ketiadaan petunjuk operasional di layar Estimasi Biaya mengenai syarat masuknya item layanan klinis ke dalam tagihan berjalan | `DESIGN_CHANGE` / UI-UX | Frontend | Low | SUDAH-VERIFIKASI di source (`nursing-billing-section.jsx:255-273`) | `FIX-KEP-014-03` |

---

## 4. Rincian per Temuan

### ISS-KEP-014-01 — Tarif Kamar ODC 321B Berstatus "Tarif Belum Ada" Akibat Ketiadaan Master Data Tarif Kamar

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DATA` |
| **Area** | Backend / Master Data Tarif |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; data dari capture pelapor |
| **Perbaikan** | `FIX-KEP-014-01` |

**Apa yang terjadi.**
Pada layar Estimasi Biaya Pasien rawat inap, kamar `ODC 321B` (Kelas ODC) yang ditempati pasien sejak 19/10/2026 menampilkan status *"Tarif Belum Ada"*, dengan harga satuan dan total harga berupa strip (`-`). Akibatnya, subtotal kamar rawat inap tidak dapat dihitung dan total estimasi tagihan menjadi parsial/tidak lengkap.

**Kenapa terjadi.**
Rantai sebab teknis dari antarmuka sampai basis data:
1. Komponen antarmuka `nursing-billing-section.jsx` (baris 196–198 dan 347–357) membaca field `priceStatus` dan `lineStatus`. Jika nilainya bernilai `"TARIFF_NOT_FOUND"`, badge yang dirender adalah status peringatan oranye bertuliskan *"Tarif Belum Ada"*, dan nilai nominal ditampilkan sebagai tanda `-`.
2. DTO tersebut dikirim oleh backend `PatientBillingSummaryService.Breakdown.cs` (baris 201–226 pada `GetEstimateAsync`), yang mengevaluasi properti `segment.MissingTariff` dari hasil engine kalkulasi tagihan kamar rawat inap.
3. Di dalam `BillingCalculationService.cs` baris 725–730 dan 760–766:
   ```csharp
   // BillingCalculationService.cs:728-730
   var tariffs = await _dbContext.Set<MstTariff>().AsNoTracking()
       .Where(x => !x.IsDelete && x.IsActive && x.IsRoomCharge)
       .ToListAsync(cancellationToken);

   // BillingCalculationService.cs:762-764
   var tariff = ResolveRoomTariff(
       tariffs, placement.ServiceUnitId, placement.PatientClassId, placement.StartDateTime);
   ```
4. Metode `ResolveRoomTariff` (baris 852–860) mencari entri tarif di tabel `MstTariff` yang memenuhi:
   - `IsRoomCharge == true` dan `IsActive == true` dan `!IsDelete`
   - `ServiceUnitId == placement.ServiceUnitId` (Unit Layanan Kamar ODC 321B)
   - `(PatientClassId == null || PatientClassId == placement.PatientClassId)` (Kelas Rawat ODC)
   - Tanggal penempatan berada di antara `EffectiveStartDate` dan `EffectiveEndDate`.
5. Karena di database belum terdapat data master tarif kamar yang cocok untuk unit ODC 321B pada kelas ODC, metode `ResolveRoomTariff` mengembalikan `null`. Akibatnya `MissingTariff` diset menjadi `true`, memicu status `TARIFF_NOT_FOUND`.

**Apakah ini menyimpang dari desain?**
Secara perilaku kode, ini **sesuai desain arsitektur**. Sistem Quilvian secara ketat melarang mengarang harga atau menetapkan harga kamar sembarangan bila master tarif tidak ditemukan (mencegah penagihan ilegal). Namun dari sisi kelengkapan data operasional rumah sakit, ini adalah **gap data master (`DATA`)** di mana tarif kamar ODC belum diinput oleh bagian Tarif/Keuangan RS.

**Dampak nyata.**
Petugas admisi dan perawat tidak dapat memberikan estimasi biaya rawat inap kepada pasien/penjamin (AdMedika). Pasien asuransi berisiko tertahan proses persetujuan jaminan (*eligibility / guarantee letter*) karena nilai plafon harian kamar tidak dapat divalidasi.

**Rekomendasi.**
1. **Solusi Utama:** Bagian Keuangan/Master Data RS menginput tarif kamar untuk Ruangan ODC (unit ODC 321B, kelas ODC) pada modul Master Data Tarif dengan mencentang `IsRoomCharge = true` dan menetapkan tanggal efektif yang mencakup tanggal admisi pasien.
2. **Penyempurnaan UI:** Menampilkan tooltip atau bantuan pada status *"Tarif Belum Ada"* yang menjelaskan unit dan kelas mana yang belum memiliki tarif agar petugas langsung tahu siapa yang harus dihubungi.

---

### ISS-KEP-014-02 — Pesanan Klinis (Obat, Tindakan, Penunjang) Tidak Muncul di Estimasi Biaya karena Belum Menembus Milestone Penagihan

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` / Aturan Bisnis |
| **Area** | Backend & Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source; data dari capture pelapor |
| **Perbaikan** | `FIX-KEP-014-02` |

**Apa yang terjadi.**
Saat dokter atau perawat melakukan peresepan obat, penginputan tindakan, atau permohonan laboratorium/radiologi, item-item tersebut tidak muncul sama sekali pada kelompok layanan di layar Estimasi Biaya Pasien.

**Kenapa terjadi.**
Rantai sebab proses bisnis dan teknis:
1. **Status Resep Masih Draf:** Pada tangkapan layar Gambar 1, resep nomor `RX-20261020-00001` masih berada pada status **"Draf"** dengan status verifikasi farmasi *"Menunggu Finalisasi Klinis"*. Dokter baru menekan tombol *"Simpan Draft Resep"*. Draf resep adalah catatan kerja dokter yang belum final, belum sah secara medis, dan belum dikirimkan ke instalasi Farmasi.
2. **Prinsip Dasar Penagihan Rumah Sakit (Hospital Billing Governance):**
   Di rumah sakit, **membuat pesanan klinis (order) BUKAN berarti pelayanan telah terjadi**. Tagihan hanya boleh timbul jika layanan telah diserahkan atau dilaksanakan secara nyata:
   - **Obat & Alkes (`PHARMACY`):** Diterbitkan ke billing (`ClinicalMilestoneFactProducer.EmitChargeEligibilityAsync`) HANYA ketika Farmasi telah menyiapkan dan menyerahkan fisik obat kepada pasien/perawat (`PrescriptionDispensingService.DispenseAsync` pada baris 389) atau dicatat sebagai pemakaian langsung di bangsal (`DrugUsageService.cs`). Bila baru berupa resep (apalagi draf), obat tersebut belum tentu diberikan, bisa ditolak farmasi karena stok habis/interaksi obat, atau dibatalkan. Menagihkan resep sebelum obat diserahkan adalah pelanggaran kepatuhan rumah sakit (*phantom billing*).
   - **Tindakan Medis (`PROCEDURE`):** Diterbitkan ke billing HANYA ketika tindakan telah **SELESAI DIEKSEKUSI** (`PatientProcedureExecutionService.ExecuteAsync` pada baris 61). Order tindakan yang baru dibuat dokter berstatus `Ordered`/`Scheduled`, belum dieksekusi.
   - **Penunjang Laboratorium (`SUPPORT`):** Diterbitkan ke billing HANYA ketika sampel darah/spesimen telah diterima di laboratorium dan diverifikasi berstatus **Layak Pemeriksaan** (`LabSpecimenStatus.EligibleForExamination` via `LabSpecimenService.cs`).
   - **Penunjang Radiologi (`SUPPORT`):** Diterbitkan ke billing HANYA ketika pemeriksaan rontgen/CT/USG telah **selesai dikerjakan** (`RadStudyStatus.AcquisitionCompleted` via `RadStudyService.cs`).
3. **Filter Kelompok Layanan Frontend (AC-4):**
   Komponen `nursing-billing-section.jsx` (baris 38 dan 414–417) secara sengaja menyembunyikan kelompok layanan yang memiliki nol baris aktif (`group.Lines.Count === 0`). Karena belum ada tindakan yang dieksekusi atau obat yang diserahkan, kelompok Tindakan, Obat, dan Penunjang secara otomatis tidak dirender.

**Apakah ini menyimpang dari desain?**
Tidak menyimpang dari arsitektur backend, melainkan **sesuai rancangan keamanan klinis dan finansial Quilvian**. Namun, pengguna memiliki kesalahpahaman konsep (*mental model mismatch*): pengguna mengira sistem bekerja seperti toko online di mana menekan simpan/order langsung menagih ke keranjang.

**Dampak nyata.**
Dokter dan perawat bingung dan menduga ada kerusakan data/sistem integrasi karena pesanan yang baru mereka ketik tidak kelihatan di estimasi biaya.

**Rekomendasi.**
1. Jelaskan kepada pengguna bahwa Estimasi Biaya mencerminkan pelayanan yang **sudah sah dilakukan/diserahkan**.
2. Tetap pertahankan integritas penagihan (jangan memasukkan draf atau order mentah ke dalam tagihan berjalan resmi).
3. Buat perbaikan UI/UX (lihat ISS-KEP-014-03).

---

### ISS-KEP-014-03 — Ketiadaan Petunjuk Operasional Siklus Layanan Klinis pada Layar Estimasi Biaya

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` / UI-UX |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source (`nursing-billing-section.jsx:255-273`) |
| **Perbaikan** | `FIX-KEP-014-03` |

**Apa yang terjadi.**
Pada banner informasi `nursing-billing-section.jsx`, hanya ada teks pedoman etika perawat bangsal (*"Perawat dilarang memperdebatkan atau merinci harga kepada pasien..."*). Tidak ada petunjuk operasional yang menjelaskan mengapa kelompok Tindakan, Obat, atau Penunjang tidak muncul.

**Kenapa terjadi.**
Pengembang sebelumnya hanya memfokuskan pesan privasi nominal harga perawat, tanpa menyertakan petunjuk alur hidup klinis (*clinical lifecycle workflow*) bagi staf ruangan.

**Dampak nyata.**
Setiap kali perawat atau dokter baru menginput pesanan klinis dan membuka tab Estimasi Biaya, mereka mengira sistem rusak atau data mereka hilang.

**Rekomendasi.**
Tambahkan catatan edukatif pada kotak informasi atau tooltip bantuan di header:
*"Catatan Alur Layanan: Rincian Tindakan, Obat, dan Penunjang Medis akan otomatis muncul setelah tindakan selesai dieksekusi, obat diserahkan oleh Farmasi, atau sampel penunjang dinyatakan layak periksa."*

---

## 5. Tanya-Jawab Pelapor

Setiap pertanyaan yang diajukan pengguna dijawab langsung pada kalimat pertama di bawah ini:

> **T1: Tidak muncul Harga kamar ya?**
>
> **J1:** Betul, harga kamar belum muncul dan saat ini berstatus *"Tarif Belum Ada"* karena **Master Data Tarif untuk Ruangan ODC 321B pada Kelas ODC belum didaftarkan di master data rumah sakit**. Sistem Billing Quilvian berjalan dengan proteksi ketat: jika master tarif kamar aktif tidak ditemukan di tabel `MstTariff` (dengan penanda `IsRoomCharge = true`), sistem tidak akan mengarang harga dan secara otomatis menetapkan status kamar sebagai belum memiliki tarif (`TARIFF_NOT_FOUND`). Solusinya adalah menginput tarif kamar ODC tersebut melalui menu Master Data Tarif Rumah Sakit.

> **T2: Kenapa pada saat dokter atau perawat melakukan checkout apapun itu seperti contoh Tindakan, Obat, Penunjang Medis, dan lain-lain tidak muncul di estimasi biaya ya?**
>
> **J2:** Hal itu terjadi karena pesanan-pesanan tersebut **belum selesai dilaksanakan atau belum diserahkan secara resmi**, sehingga belum menjadi beban biaya tagihan pasien. Di rumah sakit:
> 1. **Untuk Obat:** Pada tangkapan layar yang Anda lampirkan, resep masih berstatus **"Draf"** dan bertuliskan *"Menunggu Finalisasi Klinis"*. Resep baru menjadi tagihan setelah dokter memfinalisasi resep dan petugas **Farmasi melakukan penyerahan obat (Dispense)** atau dicatat pemakaiannya di bangsal. Resep yang baru diketik/berupa draf belum tentu diberikan kepada pasien sehingga tidak boleh langsung ditagihkan.
> 2. **Untuk Tindakan Medis:** Tindakan baru muncul di tagihan setelah dokter/perawat melakukan **eksekusi tindakan (*Complete/Execute*)**, bukan saat baru di-order.
> 3. **Untuk Penunjang Medis (Lab/Radiologi):** Pemeriksaan lab baru muncul setelah sampel diterima dan dinyatakan **layak periksa oleh analis laboratorium**, dan radiologi baru muncul setelah **pemeriksaan foto rontgen selesai dilakukan**.
> 4. Pada tampilan Estimasi Biaya, kelompok layanan yang belum memiliki item yang sah sengaja disembunyikan agar layar tetap bersih dan tidak menampilkan data kosong.

---

## 6. Temuan Tambahan

Tidak ada temuan tambahan di luar kedua isu yang dikeluhkan pelapor.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor:

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| :---: | --- | --- | --- |
| P-01 | Berapa besaran tarif nominal harian resmi untuk Kamar Rawat Inap ODC 321B (Kelas ODC) di rumah sakit Anda? | Diperlukan oleh administrator database / staf keuangan untuk menginput data master tarif `MstTariff` yang valid. | Status kamar akan tetap bertuliskan *"Tarif Belum Ada"* dan subtotal kamar belum dapat dihitung. |

### Untuk Pemilik Sistem:

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| :---: | --- | --- | --- | :---: |
| K-01 | Apakah pada layar Estimasi Biaya Pasien perlu disediakan panel/bagian informatif khusus **"Pesanan Masih Berjalan (Pending Orders)"** untuk melihat estimasi nilai dari order yang masih berstatus draf/pesanan aktif? | (A) Cukup tambahkan catatan petunjuk alur di banner informasi tanpa mengubah struktur data tagihan.<br>(B) Buat seksi terpisah "Estimasi Pesanan Belum Selesai" yang terisolasi dari Total Tagihan Resmi. | **Pilihan A** (mempertahankan kesederhanaan dan keselamatan klinis-finansial rumah sakit, serta mencegah perdebatan biaya draf obat dengan pasien). | `FIX-KEP-014-02`, `FIX-KEP-014-03` |

---

## 8. Catatan Pola

Isu ini serupa dengan pola pelaporan pada `ISSUE-KEP-011` (kebingungan antara status kalkulasi kasir dan tampilan di bangsal). Pengguna klinis sering kali menyamakan sistem CPOE/Admisi dengan aplikasi belanja e-commerce. Perlu dibuatkan panduan visual alur kerja (*Workflow Lifecycle Guide*) yang menjelaskan kapan suatu order klinis berubah menjadi beban tagihan kasir.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-20 | Dokumen issue dibuat berdasarkan laporan 2 butir keluhan dan 2 tangkapan layar antarmuka pengguna. | `diagnose-module-issue` |
| 2026-10-20 | Butir 1 (FIX-KEP-014-01) disetujui dan dieksekusi ke database `QuilvianNewDevHamzah`. Status issue berubah menjadi DALAM_PERBAIKAN. | `manage-database-operations` |
