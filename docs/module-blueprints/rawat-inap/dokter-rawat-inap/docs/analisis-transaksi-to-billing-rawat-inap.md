# Analisis Transaksi To Billing Rawat Inap: Paradigma CPOE Real-Time Tanpa Draf & Kunci

**Dokumen Analisis Arsitektur & Bisnis Sistem Quilvian**  
**Modul:** Rawat Inap (*Inpatient*) — Integrasi Dokter, Keperawatan, Penunjang, Farmasi, dan Billing  
**Lokasi Berkas:** `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/docs/analisis-transaksi-to-billing-rawat-inap.md`  
**Status:** Canonical Analysis / Rekomendasi Solusi Definitif  
**Penyusun:** Antigravity AI Engineering & Hospital Domain Architect  

---

## Daftar Isi
1. [Ringkasan Eksekutif](#1-ringkasan-eksekutif)
2. [Tinjauan Kritis Terhadap Analisis Agen Lain](#2-tinjauan-kritis-terhadap-analisis-agen-lain)
3. [Dilema Arsitektur: Poliklinik Mindset vs Realitas Rawat Inap](#3-dilema-arsitektur-poliklinik-mindset-vs-realitas-rawat-inap)
4. [Pembedaan Fundamental: "Estimasi Biaya" vs "Penagihan Definitif (Kasir)"](#4-pembedaan-fundamental-estimasi-biaya-vs-penagihan-definitif-kasir)
5. [Sistem Transaksi Rawat Inap To Billing (Target To-Be)](#5-sistem-transaksi-rawat-inap-to-billing-target-to-be)
   - 5.1. Alur Transaksi Obat & Farmasi
   - 5.2. Alur Transaksi Tindakan Medis & Keperawatan
   - 5.3. Alur Order Pemeriksaan Penunjang (Lab & Radiologi)
6. [Mekanisme Pembatalan / Penghapusan Transaksi (Clinical Void & Reversal)](#6-mekanisme-pembatalan--penghapusan-transaksi-clinical-void--reversal)
7. [Dampak Terhadap Alur Kerja Dokter dan Perawat (UX Dokter/Perawat)](#7-dampak-terhadap-alur-kerja-dokter-dan-perawat-ux-dokterperawat)
8. [Pemetaan Perubahan Teknis pada Backend dan Frontend](#8-pemetaan-perubahan-teknis-pada-backend-dan-frontend)
9. [Matriks Perbandingan: As-Is vs Paradigma Agen Lain vs Target To-Be](#9-matriks-perbandingan-as-is-vs-paradigma-agen-lain-vs-target-to-be)
10. [Kesimpulan & Langkah Implementasi](#10-kesimpulan--langkah-implementasi)

---

## 1. Ringkasan Eksekutif

Dalam pengoperasian modul Rawat Inap (*Inpatient*), terdapat perbedaan mendasar yang membedakannya secara mutlak dari Rawat Jalan (*Outpatient/Poliklinik*):
1. **Rawat Jalan adalah Transaksi Tunggal Sesaat (*Transient/Single-Encounter*)**: Pasien datang, diperiksa dokter (10–20 menit), dokter klik "Selesaikan & Kunci", pasien membayar di kasir, mengambil obat di farmasi, lalu pulang. Selesai.
2. **Rawat Inap adalah Perawatan Berkelanjutan Multi-Hari (*Longitudinal Multi-Day Care*)**: Pasien dirawat selama 3, 7, hingga 30 hari. Pasien menerima visite rutin dari dokter DPJP dan dokter konsulen, asuhan keperawatan 24 jam dalam 3 shift (pagi, sore, malam), pergantian infus/obat harian, serta pemeriksaan lab/rontgen berkala.

Permasalahan utama yang terjadi saat ini di sistem Quilvian adalah **terbawanya aturan main Rawat Jalan ke dalam Rawat Inap (Poliklinik Mindset Leakage)**. Akibatnya:
- Resep dokter tertahan di status **"Draf" / "Menunggu Finalisasi Klinis"** dan tidak pernah sampai ke antrean instalasi farmasi.
- Dokter dipaksa menekan tombol **"Selesaikan & Kunci"** hanya untuk meneruskan resep ke farmasi. Padahal tombol ini mematikan status encounter (`EncounterStatus = ConsultationCompleted`), mengunci catatan medis, dan memblokir dokter lain serta perawat untuk menginput terapi selanjutnya.
- Item order (resep, tindakan, penunjang) disembunyikan dari layar **Estimasi Biaya** dengan alasan "belum dieksekusi atau belum diserahkan farmasi", sehingga keluarga pasien dan dokter tidak memiliki kendali finansial (*financial visibility*) terhadap estimasi biaya berjalan dan limit plafon asuransi (BPJS / asuransi swasta).

User telah memberikan arahan tegas:
> **"Pada Sistem Kerja Dokter dan Keperawatan: Tidak perlu melakukan selesaikan dan kunci. Hilangkan sistem draf-draf tersebut tidak digunakan. Jika transaksi salah, gunakan tombol batal atau hapus. Seluruh item yang ditambahkan sudah harus langsung masuk ke modul masing-masing (contoh resep langsung terlihat di farmasi) dan langsung masuk ke estimasi biaya beserta harga."**

Dokumen ini menguji validitas analisis agen lain, membeberkan kekeliruannya secara ilmiah bisnis rumah sakit, dan merumuskan cetak biru arsitektur transaksi menuju billing rawat inap.

---

## 2. Tinjauan Kritis Terhadap Analisis Agen Lain

### Kutipan Pernyataan Agen Lain:
> *"Butir 2 — Kenapa Tindakan, Obat, Penunjang Medis, dll. Tidak Muncul di Estimasi Biaya Saat Dokter/Perawat Melakukan Order/Checkout:*  
> *Akar Masalah: Ini bukan kerusakan/bug aplikasi, melainkan aturan keselamatan dan integritas penagihan rumah sakit (Hospital Billing Governance):*  
> *- Obat & Alkes: Resep masih berstatus 'Draf' dan bertuliskan 'Menunggu Finalisasi Klinis'. Dokter baru menekan simpan draf. Di rumah sakit, order resep tidak boleh langsung menagih uang ke pasien saat dokter baru mengetik, karena resep tersebut belum tentu disetujui apoteker, belum tentu disiapkan, dan belum tentu diminum pasien. Obat baru menjadi beban tagihan ketika instalasi Farmasi telah melakukan penyerahan fisik obat (Dispensing) melalui `PrescriptionDispensingService.DispenseAsync` atau dicatat pemakaiannya di bangsal.*  
> *- Tindakan Medis: Tindakan baru diterbitkan ke penagihan ketika tindakan telah SELESAI DIEKSEKUSI (Completed/Execute) melalui `PatientProcedureExecutionService.ExecuteAsync`, bukan saat baru dipesan di sistem.*  
> *- Penunjang Medis (Lab/Radiologi): Pemeriksaan laboratorium baru masuk tagihan setelah sampel diterima dan dinyatakan Layak Periksa oleh analis lab, dan radiologi baru masuk setelah pemeriksaan foto rontgen selesai dilakukan.*  
> *Pada layar Estimasi Biaya, kelompok layanan yang belum memiliki item sah yang selesai dikerjakan secara sengaja disembunyikan agar layar tidak menampilkan baris kosong."*

---

### Verifikasi Faktual & Kritik Ilmiah:

| Parameter | Penilaian | Fakta Teknis & Realitas Bisnis Rumah Sakit |
| :--- | :---: | :--- |
| **Deskripsi Kode Saat Ini (*As-Is*)** | **BENAR SECARA FAKTUAL** | Kode backend saat ini (`PrescriptionWorkflowService.cs`, `PrescriptionDispensingService.cs`, `BillingFolioService.cs`) memang sengaja ditulis dengan aturan di atas. Kode tersebut menolak `SubmitAsync` resep secara mandiri (`PrescriptionWorkflowService.cs:87-88`) dan menahan charge sampai tahap penyerahan farmasi (`DispenseAsync`). |
| **Validitas untuk Rawat Jalan (*Poliklinik*)** | **BENAR** | Di poliklinik, pasien tidak boleh ditagih sebelum resep difinalkan oleh dokter. Pasien membawa resep ke kasir, membayar, lalu obat diserahkan. |
| **Validitas untuk Rawat Inap (*Inpatient*)** | **CACAT / SALAH BESAR (FALLACY)** | Menerapkan aturan ini di rawat inap menciptakan **kegagalan fatal klinis dan administratif**, sebagaimana dijelaskan di bawah ini. |

---

### Tiga Kesalahan Fatal pada Analisis Agen Lain:

#### 1. Kesalahan Definisi: Menyamakan "Estimasi Biaya" dengan "Kuitansi Pembayaran Kasir"
Agen lain menyatakan: *"order resep tidak boleh langsung menagih uang ke pasien saat dokter baru mengetik..."*  
Pernyataan ini mencampuradukkan dua konsep yang berbeda:
- **Penagihan Uang / Kasir Definitif (*Final Billing / Cashier Discharge*)**: Ini adalah penarikan uang nyata atau klaim asuransi saat pasien akan pulang (*discharge*). Pada tahap ini, memang hanya obat yang telah diserahkan dan tindakan yang telah dilakukan yang ditagih.
- **Estimasi Biaya Berjalan (*Running Cost Estimation / Financial Projection*)**: Ini adalah layar informasi untuk pasien, keluarga, dokter penanggung jawab pelayanan (DPJP), dan petugas *casemix/asuransi*. 
  - Jika seorang dokter meresepkan antibiotik paten seharga Rp 3.000.000,- per ampul untuk pasien rawat inap, keluarga pasien dan staf asuransi **HARUS DAPAT MELIHAT** estimasi biaya tersebut detik itu juga di layar estimasi biaya!
  - Jika sistem menyembunyikan item tersebut sampai farmasi melakukan dispensing, maka monitoring limit plafon asuransi (contoh plafon BPJS atau asuransi perusahaan swasta) akan jebol tanpa peringatan! Ketika farmasi mendispensing obat, tiba-tiba tagihan membengkak dan keluarga pasien kaget karena tidak ada proyeksi biaya sebelumnya.

#### 2. Bahaya Klinis: Menahan Order Dokter di Status "Draf" di Rawat Inap
Agen lain membenarkan status *"Draf - Menunggu Finalisasi Klinis"*.  
Di rawat inap, ketika dokter menuliskan instruksi terapi obat di bangsal (contoh: injeksi Ceftriaxone 1 gr atau infus RL 20 tpm):
- Instruksi tersebut adalah **Perintah Klinis Resmi (Computerized Physician Order Entry / CPOE)**.
- Jika sistem menahannya sebagai "Draf", maka **Instalasi Farmasi Rumah Sakit TIDAK AKAN PERNAH MENERIMA ORDER TERSEBUT!**
- Apoteker tidak tahu bahwa pasien membutuhkan antibiotik tersebut. Akibatnya, penyiapan obat tertunda berjam-jam, membahayakan keselamatan pasien (*patient safety hazard*).
- Untuk mengeluarkan resep dari status draf, sistem saat ini memaksa dokter menekan **"Selesaikan & Kunci"**. Padahal menekan tombol ini mengunci seluruh episode konsultasi, membuat dokter atau perawat shift berikutnya tidak bisa lagi menambahkan obat atau tindakan!

#### 3. Logika Terbalik Mengenai Penanganan Kesalahan: "Draf" vs "Batal/Hapus (Void)"
Agen lain mengira bahwa sistem "Draf" diperlukan untuk mencegah salah order.  
Di rumah sakit nyata berstandar akreditasi (KARS / JCI):
- Di rawat inap, tidak ada dokter yang menyimpan draf selama 2 hari baru mengirim ke farmasi. Terapi obat harus segera diberikan.
- Jika dokter atau perawat melakukan kesalahan (misalnya salah memilih obat, salah dosis, atau salah pasien), mekanisme yang sah secara hukum dan rekam medis adalah **PEMBATALAN / PENGHAPUSAN (VOID / CANCEL ORDER)** dengan mencatat **Alasan Pembatalan** (audit trail).
- Pembatalan ini otomatis membalikkan saldo estimasi biaya (*reversal charge*) dan mengirimkan sinyal pembatalan ke instalasi farmasi/penunjang.

---

## 3. Dilema Arsitektur: Poliklinik Mindset vs Realitas Rawat Inap

Perbandingan kontras antara pola kerja Rawat Jalan yang keliru diadopsi dengan pola kerja Rawat Inap yang sebenarnya:

```
[RAWAT JALAN / POLIKLINIK (Pola Sesaat)]
Pasien Datang ──> Dokter Periksa ──> Tulis SOAP & Resep (Draf) 
                                              │
                    ┌─────────────────────────┘
                    ▼
          [Selesaikan & Kunci]  <── (Satu Kali Saja)
                    │
                    ├──> Kunci Catatan Medis (Encounter Selesai)
                    ├──> Masuk Kasir (Pasien Bayar Sendiri)
                    └──> Farmasi Dispense Obat ──> Pasien Pulang

================================================================================

[RAWAT INAP / INPATIENT (Pola Kontinu Multi-Hari - TARGET TO-BE)]
Pasien Masuk Bangsal (Hari 1 s/d Hari N)
   │
   ├── [Dokter Visite 08:00] ──> Tulis Obat / Tindakan ──> [Simpan / Order]
   │                                                             │
   │      ┌──────────────────────────────────────────────────────┴──────────────┐
   │      ▼                                                                     ▼
   │  [Langsung ke Farmasi/Tindakan]                             [Langsung Masuk Estimasi Biaya]
   │  (Status: Submitted / Queued)                               (Folio Charge Line Terbit Real-time)
   │  Apoteker langsung siapkan obat!                            Plafon Asuransi terpantau akurat!
   │
   ├── [Perawat Shift Sore 14:00] ──> Lakukan Asuhan / Infus Baru ──> [Simpan / Order]
   │                                                                        │
   │                                   (Langsung Masuk Modul Terkait & Billing Estimasi)
   │
   ├── [Jika Ada Kesalahan Input] ──> Tekan [Batal / Hapus]
   │                                         │
   │                                         ├──> Batalkan Order di Farmasi/Layanan
   │                                         └──> Reversal / Kurangkan Tagihan di Billing
   │
   └── [Hari Terakhir: Pasien Pulang] ──> Kasir Checkout Discharge (Tutup Folio & Cetak Kuitansi)
```

---

## 4. Pembedaan Fundamental: "Estimasi Biaya" vs "Penagihan Definitif (Kasir)"

Untuk menjamin kepatuhan akuntansi rumah sakit sekaligus kenyamanan operasional medis, sistem Quilvian membedakan secara tegas dua tingkatan data finansial:

```
+-------------------------------------------------------------------------------------------------+
|                                    EPISODE RAWAT INAP PASIEN                                    |
+-------------------------------------------------------------------------------------------------+
|                                                                                                 |
|   [1. ESTIMASI BIAYA BERJALAN (Real-time Order Folio)]                                          |
|   Target Pengguna: Dokter, Perawat, Staf Casemix, Keluarga Pasien                               |
|   Karakteristik:                                                                                |
|   - Setiap order resep, alkes, tindakan, kamar, dan penunjang LANGSUNG TERTERA.                |
|   - Menampilkan Nama Item, Jumlah (Qty), Satuan, Harga Satuan, dan Subtotal.                   |
|   - Status item jelas: "Dipesan (Ordered)", "Disiapkan (In Prep)", "Selesai (Completed)".       |
|   - Jika dibatalkan (Void), item dicoret (Strikethrough) dan nominalnya dikurangkan.            |
|   - Tujuan: Kontrol plafon BPJS/Asuransi dan transparansi biaya kepada keluarga pasien.        |
|                                                                                                 |
|                                         │                                                       |
|                                         │ (Proses Verifikasi & Pelayanan)                       |
|                                         ▼                                                       |
|                                                                                                 |
|   [2. PENAGIHAN KASIR DEFINITIF (Final Invoice / Billing Clearance)]                            |
|   Target Pengguna: Kasir Rumah Sakit, Bagian Keuangan, Klaim Asuransi                           |
|   Karakteristik:                                                                                |
|   - Diterbitkan saat pasien dinyatakan boleh pulang (KRS / Discharge Clearance).                |
|   - Hanya memuat item yang secara sah telah diserahkan (dispensed) atau dikerjakan (executed).  |
|   - Mengakomodasi retur obat dan koreksi selisih sebelum cetak kuitansi pelunasan.             |
|                                                                                                 |
+-------------------------------------------------------------------------------------------------+
```

Dengan arsitektur ini:
- **Layar Estimasi Biaya Dokter & Rawat Inap** menampilkan seluruh transaksi yang telah di-checkout/ditambahkan beserta harga satuannya, tanpa menunggu dispensing fisik apotek!
- **Layar Kasir Pembayaran Akhir** mencocokkan realisasi fisik akhir saat pasien pulang.

---

## 5. Sistem Transaksi Rawat Inap To Billing (Target To-Be)

### 5.1. Alur Transaksi Obat & Farmasi

1. **Input oleh Dokter/Perawat**:
   - Dokter memilih obat (nama obat, dosis, frekuensi, rute, jumlah).
   - Dokter menekan tombol **"Simpan / Kirim Resep"**.
2. **Tidak Ada Status Draf**:
   - Resep langsung tersimpan dengan status `PrescriptionStatus = Submitted` dan `FulfillmentStatus = QueuedAtPharmacy` (atau `WaitingForDispensing`).
   - Resep **tidak** lagi membutuhkan penutupan konsultasi (`FinalizeFromConsultationAsync`).
3. **Penyaluran Langsung ke Farmasi**:
   - Di modul Farmasi (*Pharmacy Workspace*), resep ini **detik itu juga muncul** pada tab antrean rawat inap (*Inpatient Prescription Queue*).
   - Petugas farmasi dapat langsung memverifikasi ketersediaan stok, meracik, dan menyiapkan obat.
4. **Penyaluran Langsung ke Estimasi Biaya & Billing**:
   - Sistem backend menerbitkan fakta klinis ke `ClinicalMilestoneFactProducer.EmitChargeEligibilityAsync`.
   - `BillingFolioService` mencatat baris tagihan (`BilChargeLine`) dengan status kalkulasi terhitung (`PendingFinancialReview` / `ActiveCharge`).
   - Layar **Estimasi Biaya Pasien** langsung menampilkan baris obat tersebut lengkap dengan jumlah, tarif per unit, dan total harga.

---

### 5.2. Alur Transaksi Tindakan Medis & Keperawatan

1. **Pencatatan Tindakan**:
   - Dokter mencatat tindakan medis (misal: Pasang Kateter, Pungsi, Jahit Luka, Visitasi Dokter Spesialis).
   - Perawat mencatat tindakan keperawatan (misal: Perawatan Luka, Pasang Infus, Nebulisasi, Pemantauan TTV berkala).
2. **Pemberian Pilihan Eksekusi**:
   - Jika tindakan dicatat sebagai **telah dilakukan** (langsung oleh dokter/perawat yang bertugas), transaksi berstatus `Completed`.
   - Jika tindakan dicatat sebagai **order rencana** (misal: tindakan terjadwal besok pagi), transaksi berstatus `Scheduled / Ordered`.
3. **Penyaluran ke Estimasi Biaya**:
   - Seluruh item tindakan yang ditambahkan ke riwayat transaksi langsung masuk ke baris rincian Estimasi Biaya rawat inap dengan harga sesuai kelas perawatan kamar pasien (*Class of Care Tariff*).

---

### 5.3. Alur Order Pemeriksaan Penunjang (Lab & Radiologi)

1. **Order Penunjang**:
   - Dokter/perawat memilih paket pemeriksaan lab (contoh: Darah Lengkap, Elektrolit, Fungsi Hati) atau rontgen (contoh: Thorax AP).
   - Menekan tombol **"Kirim Order Penunjang"**.
2. **Distribusi Instan ke Unit Penunjang**:
   - Laboratorium langsung melihat order di antrean order lab rawat inap untuk pengambilan sampel darah.
   - Radiologi langsung menerima daftar pesanan foto rontgen / USG untuk penjadwalan pasien bangsal.
3. **Muncul di Estimasi Biaya**:
   - Estimasi biaya langsung mencantumkan tarif pemeriksaan lab / radiologi yang telah diorder.

---

## 6. Mekanisme Pembatalan / Penghapusan Transaksi (Clinical Void & Reversal)

Jika seorang dokter atau perawat melakukan kesalahan (salah klik obat, salah pasien, atau pasien menolak pemeriksaan):

```
[DOKTER / PERAWAT]
       │
       ▼
Klik Tombol [Batal / Hapus] pada Baris Riwayat Transaksi
       │
       ▼
Modal Dialog Konfirmasi Pembatalan:
- Menampilkan Nama Item & Jumlah
- Input Wajib: "Alasan Pembatalan" (Minimal 5 karakter, misal: "Salah pilih dosis oleh DPJP")
- Konfirmasi: [Ya, Batalkan Item]
       │
       ├───────────────────────────────────────┬───────────────────────────────────────┐
       ▼                                       ▼                                       ▼
[MODUL KLINIS]                         [MODUL TUJUAN]                         [MODUL BILLING]
Status item diubah menjadi:            Sinyal Pembatalan dikirim:             `EmitClinicalCancellationAsync`
- `IsCancel = true`                    - Farmasi: Resep ditandai BATAL        `BillingFolioService`:
- `CancelReason = "..."`                 (Pengerjaan dihentikan)              - Menandai charge line sebagai BATAL
- `CancelBy = actorUserId`             - Lab/Rad: Order dibatalkan            - Mengurangkan nominal dari subtotal
- `CancelDateTime = UtcNow`              dari antrean pemeriksaan             - Total Estimasi Biaya otomatis turun!
```

### Validasi Integritas Pembatalan:
- **Resep yang Belum Diserahkan (Belum Dispensed)**: Dapat langsung dibatalkan oleh dokter/perawat.
- **Resep yang Sudah Diserahkan Farmasi (Sudah Dispensed)**: Dokter/perawat tidak bisa asal klik hapus. Sistem menampilkan peringatan: *"Obat sudah diserahkan oleh farmasi. Silakan ajukan proses Retur Obat melalui instalasi Farmasi."*
- **Tindakan yang Belum Dikerjakan**: Dapat langsung dibatalkan.
- **Tindakan yang Sudah Dikerjakan**: Pembatalan membutuhkan otorisasi dokter yang bersangkutan dengan audit trail rekam medis.

---

## 7. Dampak Terhadap Alur Kerja Dokter dan Perawat (UX Dokter/Perawat)

Perubahan ini menghasilkan dampak operasional yang sangat positif bagi tenaga medis di rumah sakit:

1. **Bebas dari Kewajiban "Selesaikan & Kunci"**:
   - Dokter tidak lagi tertekan harus mencari-cari tombol "Selesaikan & Kunci".
   - Form SOAP/CPPT dokter berfungsi murni sebagai catatan perkembangan pasien terintegrasi (CPPT). Dokter menyimpan SOAP, catatan tersimpan, resep langsung terkirim ke apotek, order lab langsung terkirim ke lab.
2. **Fleksibilitas Sepanjang Waktu**:
   - Dokter DPJP visite pagi jam 08:00, input resep. Resep langsung diproses farmasi.
   - Dokter konsulen visite siang jam 12:00, input resep tambahan. Langsung diproses farmasi.
   - Perawat shift malam jam 21:00 mencatat obat tambahan atau tindakan darurat. Tidak ada yang terkunci!
3. **Transparansi Biaya untuk Edukasi Pasien**:
   - Di samping ranjang pasien, ketika keluarga bertanya perkiraan biaya, perawat/dokter tinggal membuka tab **Estimasi Biaya**, seluruh obat dan tindakan yang telah diinstruksikan tercantum rapi beserta estimasi biayanya.
4. **Keamanan Kesalahan Input**:
   - Bila salah input, ada tombol **Batal / Hapus** yang jelas, tanpa harus meminta bantuan tim IT untuk membuka kunci data.

---

## 8. Pemetaan Perubahan Teknis pada Backend dan Frontend

### 8.1. Perubahan Backend (`NewQuilvianSystemBackend`)

| Komponen / Berkas | Baris Terkait | Modifikasi yang Dilakukan |
| :--- | :--- | :--- |
| `PrescriptionWorkflowService.cs` | Line 39–46 (`ApplyInitialClinicalState`) | **Hapus status Draft default untuk rawat inap**. Bila konteks rawat inap (`InpEpisodeId != null`), status awal langsung `PrescriptionStatus = Submitted` dan `FulfillmentStatus = QueuedAtPharmacy`. |
| `PrescriptionWorkflowService.cs` | Line 81–89 (`SubmitAsync`) | **Buka submit mandiri**. Hapus penolakan *"Resep tidak diajukan secara terpisah"*. Biarkan resep rawat inap disubmit langsung saat disimpan. |
| `PrescriptionController.cs` | Line 380–410 (`CreatePrescription`) | Jika resep rawat inap memiliki item obat, **langsung panggil** `_clinicalMilestoneFactProducer.EmitChargeEligibilityAsync` pada saat resep dibuat, sehingga `BilChargeLine` langsung terbentuk di `BillingFolio`. |
| `PrescriptionController.cs` | Line 749 (`ValidateCreateRequestAsync`) | **Hapus validasi yang memblokir penambahan resep** jika `ConsultationStatus == Completed`. Di rawat inap, pembuatan resep baru pada encounter rawat inap harus selalu diizinkan selama pasien masih aktif menginap (`EncounterStatus == InpatientAdmitted`). |
| `ConsultationFinalizationService.cs`| Line 189–194 | **Hapus mutasi `EncounterStatus` menjadi `ConsultationCompleted`** jika encounter bertipe Rawat Inap (`InpatientAdmitted`). Encounter rawat inap hanya boleh selesai saat kepulangan pasien (*Discharge*). |
| `PrescriptionController.cs` | Line 548–594 (`CancelPrescription`) | Pastikan endpoint pembatalan resep dapat dipanggil langsung dari antarmuka dokter/keperawatan rawat inap dengan menyertakan `CancelReason`. Penyerahan fakta pembatalan ke billing (`EmitClinicalCancellationAsync`) sudah terbukti andal. |

---

### 8.2. Perubahan Frontend (`QuilvianSystemFrontendDev`)

| Komponen / Layar | Modifikasi yang Dilakukan |
| :--- | :--- |
| **Layar Konsultasi / Visit Dokter Rawat Inap** | **Sembunyikan / Hapus tombol "Selesaikan & Kunci"**. Ganti dengan tombol sederhana: **"Simpan Catatan Visite (CPPT)"**. |
| **Tab / Modal Resep Dokter & Perawat** | **Hapus label "Draf" dan "Menunggu Finalisasi Klinis"**. Setelah klik "Tambahkan / Simpan Resep", resep langsung berstatus hijau **"Terkirim ke Farmasi"**. |
| **Tabel Riwayat Resep / Tindakan** | Tambahkan tombol aksi **"Batal / Hapus"** pada setiap baris item yang belum diproses tuntas oleh unit terkait. Saat diklik, muncul modal dialog input alasan pembatalan. |
| **Layar Estimasi Biaya Rawat Inap** | Hubungkan secara reaktif dengan daftar `BilChargeLine` dan order aktif pasien. Tampilkan item resep dan tindakan dengan harga per unit dan subtotal secara lengkap. |

---

## 9. Matriks Perbandingan: As-Is vs Paradigma Agen Lain vs Target To-Be

| Fitur / Alur | Kondisi Sistem Saat Ini (*As-Is*) | Paradigma Agen Lain (Keliru) | Solusi Target To-Be (Sesuai Arahan User) |
| :--- | :--- | :--- | :--- |
| **Status Pembuatan Resep** | `Draft` (*Menunggu Finalisasi Klinis*) | Dibenarkan tetap `Draft` dengan dalih aturan keselamatan rumah sakit | **Langsung `Submitted` / `QueuedAtPharmacy`** tanpa tahap draf |
| **Keterlihatan di Farmasi** | Tidak terlihat di antrean farmasi sampai dokter klik "Selesaikan & Kunci" | Menganggap farmasi baru boleh tahu setelah konsultasi selesai | **Detik itu juga langsung muncul di antrean farmasi** agar obat segera diracik |
| **Penyelesaian Transaksi** | Wajib menekan "Selesaikan & Kunci" yang mematikan encounter | Menganggap tombol "Selesaikan & Kunci" adalah keharusan mutlak | **TIDAK PERLU tombol "Selesaikan & Kunci"**. Simpan langsung aktif |
| **Tampilan di Estimasi Biaya** | Nilai Rp 0 / Tersembunyi karena charge line belum terbentuk | Menganggap estimasi biaya memang harus menyembunyikan item yang belum selesai | **Langsung muncul di Estimasi Biaya** lengkap dengan harga satuan dan subtotal |
| **Penanganan Salah Input** | Membingungkan user, tidak ada tombol batal yang jelas di frontend | Mengandalkan pembatalan manual atau draf tidak difinalkan | **Tombol Batal / Hapus (Void)** dengan modal alasan pembatalan + auto-reversal billing |
| **Kemampuan Multi-Dokter & Shift Perawat** | Terblokir setelah satu dokter menyelesaikan konsultasi | Tidak memperhitungkan kebutuhan visite banyak dokter dan perawat 3 shift | **Bebas input kapan saja** oleh dokter spesialis mana pun dan perawat shift apa pun |

---

## 10. Kesimpulan & Langkah Implementasi

1. **Kesimpulan Terhadap Analisis Agen Lain**:
   - Pernyataan agen lain tersebut **benar** dalam menggambarkan batasan teknis kode lama (*As-Is Code*), namun **salah besar dan menyesatkan** dalam mengklaim bahwa perilaku tersebut adalah *"Aturan Keselamatan Rumah Sakit (Hospital Billing Governance)"*.
   - Menahan order di draf dan mewajibkan tombol "Selesaikan & Kunci" di rawat inap justru **membahayakan keselamatan pasien** karena menunda pemberian obat darurat di bangsal dan mengunci sistem untuk tenaga medis lainnya.
2. **Langkah Konkret Implementasi**:
   - **Langkah 1**: Registrasi dokumen Issue (`issue-008-transaksi-to-billing-tanpa-draf.md`) dan Dokumen Rencana Perbaikan (`plan-repair-008-transaksi-to-billing-tanpa-draf.md`) pada blueprint sub-modul dokter rawat inap.
   - **Langkah 2**: Sesuaikan `PrescriptionWorkflowService.cs` dan `PrescriptionController.cs` agar order rawat inap langsung `Submitted`, langsung menerbitkan fakta billing (`EmitChargeEligibilityAsync`), dan membuka jalur pembatalan (`CancelPrescription`).
   - **Langkah 3**: Sesuaikan antarmuka frontend rawat inap untuk membuang tombol "Selesaikan & Kunci", menghilangkan badge "Draf", memunculkan tombol "Batal / Hapus", dan menampilkan seluruh order di layar Estimasi Biaya secara real-time.
