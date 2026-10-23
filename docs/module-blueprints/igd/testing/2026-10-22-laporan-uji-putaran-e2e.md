# Laporan Resmi Hasil Pengujian End-to-End (Putaran E2E) Modul Instalasi Gawat Darurat (IGD) Quilvian

**Tanggal Pengujian:** 22 Oktober 2026  
**Pelaksana Pengujian:** AI Assistant (Pengujian Berbasis Bukti Otomatis & Visual Peramban)  
**Tipe Putaran:** End-to-End (E2E) Terpadu — 1 Pasien Baru dari Pendaftaran Loket hingga Rincian Kasir / Billing  
**Lokasi Berkas Pengujian:** `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-e2e-20261022/`  

---

## 1. Ringkasan Eksekutif

Putaran pengujian End-to-End (E2E) dilaksanakan untuk membuktikan keutuhan alur pelayanan (*happy path*) instalasi gawat darurat rumah sakit secara menyeluruh pasca penutupan regresi triase L1R (`BE-IGD-019`). Pengujian mencakup perjalanan satu orang pasien gawat darurat baru dari pendaftaran di loket admisi IGD, penilaian triase prioritas tinggi (merah), pengkajian keperawatan gawat darurat, penugasan DPJP dan pengkajian awal medis dokter, pencatatan rekam medis SOAP, peresepan obat farmasi darurat, tindakan keperawatan darurat, perumusan dan eksekusi disposisi pemulangan pasien, penutupan kunjungan IGD, hingga pembentukan tagihan dan verifikasi rincian tagihan di modul kasir / billing.

Pengujian ini dijalankan dengan kepatuhan penuh terhadap **Aturan Integritas Kode (Tanpa Modifikasi Sumber Aplikasi)**: tidak ada berkas kode backend maupun frontend yang diubah untuk meluluskan skenario uji.

### Ringkasan Skor Kelulusan

| Kategori | Jumlah Skenario | Persentase |
| :--- | :---: | :---: |
| **Lulus (PASS)** | **9** | **90%** |
| **Gagal (FAIL)** | **1** | **10%** |
| **Belum Dijalankan (NOT RUN)** | 0 | 0% |
| **Tidak Tersedia (NOT AVAILABLE)** | 0 | 0% |
| **TOTAL** | **10** | **100%** |

> **Catatan Temuan Kritis (E2E-10):**  
> Alur klinis IGD (E2E-01 sampai E2E-09) berjalan dengan sempurna (100% PASS). Namun, pada skenario akhir E2E-10, modul kasir mengalami kegagalan saat kasir membuka rute detail billing (`/detail-billing`). Endpoint backend `GET /api/v1/health-services/billing-management/billing/invoices/{id}` mengalami `NullReferenceException` (HTTP 500) pada `BillingInvoiceClosureService.CalculateOutstandingAsync` karena invoice IGD yang baru diterbitkan belum memiliki versi kalkulasi awal (`currentVersion` bernilai `null`). Temuan ini dilaporkan apa adanya secara jujur dan transparan untuk ditindaklanjuti oleh tim pengembang backend.

---

## 2. Integritas Lingkungan dan Baseline Sistem

Seluruh pengujian dijalankan pada server pengembang aktif dengan status commit dan stempel biner sebagai berikut:

* **Backend Service:**
  * **Runtime:** Microsoft .NET 9.0 (`https://localhost:7184`)
  * **Commit HEAD:** `025bfc6d0bd0364170211d01f1039b216760ce08`
  * **DLL Binary:** `bin/Debug/net9.0/QuilvianSystemBackend.dll`
  * **Stempel Waktu DLL:** `2026-10-22 12:15:33.845` (terverifikasi identik antara `kode-awal.txt` dan `kode-akhir.txt`, membuktikan tidak ada kompilasi ulang selama pengujian E2E).
* **Frontend Web Application:**
  * **Runtime:** Next.js 15 (Turbopack Dev Mode pada `http://localhost:3000`)
  * **Commit HEAD:** `52624e6cbefe828d42acd732d9c7508198484a54`
  * **Git Status:** Bersih (*clean*), 0 berkas berubah.

---

## 3. Identitas Entitas Pasien & Pelayanan Uji

Semua skenario pengujian E2E menggunakan satu entitas data uji tunggal yang saling bertautan (*traceable*):

* **Nama Pasien:** `Pasien Uji E2E Alur Penuh 199769`
* **Nomor Rekam Medis (No. RM):** `00-00-02-79`
* **Patient ID:** `e5608bb0-b250-4a5a-8752-40661534b1e5`
* **Nomor Encounter Kunjungan:** `ENC-RSMMC-00523` (ID: `ee7fc7ac-a1f8-413e-9959-7cc65f71a107`)
* **Nomor Kunjungan IGD:** `IGD-261022062940-6661BF` (ID: `4ce5a3ac-4cd2-426c-b709-2df0822f0575`)
* **Tenaga Medis yang Terlibat:**
  * **Petugas Admisi / Kasir:** SuperAdmin (`superadmin@admin.com`)
  * **Perawat IGD:** Ns. Dimas Setiawan (`perawat.dimas@hospital.com`)
  * **Dokter DPJP IGD:** dr. Jenah Ali, Sp.PD (`dokter.jenah@hospital.com`)

---

## 4. Hasil Rinci Pengujian Per Skenario (E2E-01 s.d. E2E-10)

### E2E-01: Pendaftaran Loket Pasien Baru IGD
* **Tujuan:** Mendaftarkan pasien gawat darurat baru di loket admisi IGD secara resmi, membentuk master pasien, nomor RM baru, encounter rawat darurat, dan kunjungan IGD.
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-01-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-01.json`
  * **Panggilan API:** `POST /api/v1/health-services/registration-management/patient-registrations/emergency-direct`
  * **Status HTTP:** `201 Created`
  * **Respons Verbatim:** `{"medicalRecordNumber": "00-00-02-79", "encounterNumber": "ENC-RSMMC-00523", "emergencyVisitNumber": "IGD-261022062940-6661BF"}`
* **Deskripsi Alur:** Petugas admisi mengisi identitas pasien darurat dengan keluhan sesak dan nyeri dada berat. Sistem berhasil menerbitkan No. RM `00-00-02-79` dan nomor encounter `ENC-RSMMC-00523`. Pasien langsung masuk ke antrean triage perawat IGD.

---

### E2E-02: Triase Perawat IGD Prioritas 1 (Merah / ATS 1)
* **Tujuan:** Melakukan penilaian kegawatdaruratan oleh perawat IGD dengan kriteria Australasian Triage Scale (ATS) kategori 1 (Resusitasi Segera / Merah).
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-02-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-02.json`
  * **Panggilan API:** `POST /api/v1/health-services/emergency-installation-management/emergency-triages`
  * **Status HTTP:** `200 OK`
  * **ID Triase:** `0f563df7-affa-4999-8a3f-ec69027b52cc`
  * **Status Kunjungan Baru:** `Triaged` (kode status: `3`)
* **Deskripsi Alur:** Perawat Dimas mengkaji jalan napas, laju pernapasan (32x/mnt), saturasi O2 (88%), dan kesadaran somnolen. Sistem mengklasifikasikan triase ke ATS 1 (Merah). Status kunjungan pasien di IGD berhasil diperbarui menjadi `Triaged`.

---

### E2E-03: Pengkajian Awal Keperawatan IGD Simpan & Buka Ulang
* **Tujuan:** Mencatat hasil anamnesis dan pemeriksaan fisik keperawatan gawat darurat, menyimpannya ke database rekam medis, dan memverifikasi kelengkapan data saat form dibuka kembali.
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-03-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-03.json`
  * **Panggilan API:** `POST /api/v1/health-services/clinical-management/patient-assessments`
  * **Status HTTP:** `201 Created`
  * **ID Pengkajian:** `a36cdaca-5696-46fe-a2a1-55a6a0134973`
* **Deskripsi Alur:** Pengkajian keperawatan mencatat tanda vital lengkap, skala nyeri numeric rating scale 8/10, riwayat alergi, dan status risiko jatuh Morse Fall Scale. Form ditutup dan dibuka ulang; seluruh nilai data terbukti utuh tersimpan tanpa ada *field* yang hilang.

---

### E2E-04: Penugasan DPJP & Kajian Awal Medis IGD oleh Dokter
* **Tujuan:** Dokter IGD mengambil alih tanggung jawab pasien (DPJP Assignment) dan melaksanakan pemeriksaan medis awal dokter, mengubah status kunjungan menjadi `InTreatment`.
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-04-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-04.json`
  * **Panggilan API Penugasan DPJP:** `PUT /api/v1/health-services/emergency-installation-management/emergency-visits/{id}/dpjp` (Status `200 OK`)
  * **Panggilan API Pengkajian Medis:** `POST /api/v1/health-services/clinical-management/patient-assessments` (Status `201 Created`, ID: `1093421a-885c-4b40-b04b-395bdec4dccf`)
  * **Status Kunjungan Baru:** `InTreatment` (kode status: `4`)
* **Deskripsi Alur:** dr. Jenah Ali, Sp.PD ditetapkan sebagai DPJP utama. Dokter melakukan pemeriksaan fisik toraks dan kardiovaskular serta menginstruksikan tata laksana kedaruratan. Status antrean IGD pasien beralih ke `Sedang Dilayani` (`InTreatment`).

---

### E2E-05: Pencatatan SOAP Konsultasi Dokter IGD
* **Tujuan:** Merekam catatan perkembangan pasien terintegrasi (CPPT) dengan format SOAP (Subjektif, Objektif, Asesmen, Plan) resmi rumah sakit.
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-05-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-05.json`
  * **Panggilan API:** `POST /api/v1/health-services/registration-management/doctor-consultations`
  * **Status HTTP:** `200 OK`
  * **Nomor Konsultasi:** `CON-20261022-00001` (ID: `dd7472fa-19a5-454a-af7e-2f12f9d81ae3`)
* **Deskripsi Alur:** Catatan SOAP memuat keluhan nyeri retrosternal menjalar ke lengan kiri, ronki basal minimal, kecurigaan sindrom koroner akut, dan rencana pemberian terapi antiiskemik dan observasi ketat. CPPT tersimpan permanen dan terhubung ke encounter pasien.

---

### E2E-06: Peresepan Obat IGD oleh Dokter
* **Tujuan:** Dokter meresepkan obat injeksi darurat untuk penanganan simtomatik pasien di IGD dan mengirimkannya ke sistem farmasi.
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-06-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-06.json`
  * **Panggilan API:** `POST /api/v1/health-services/pharmacy-management/prescriptions`
  * **Status HTTP:** `201 Created`
  * **ID Resep:** `f7eb8b1f-835b-42c7-9b7f-33694639f422`
  * **Rincian Obat:** Ondansetron 4mg Injeksi (1 Ampul)
* **Deskripsi Alur:** Resep elektronik IGD diterbitkan oleh dr. Jenah Ali dan berhasil masuk ke daftar antrean resep farmasi IGD untuk penyiapan obat darurat.

---

### E2E-07: Tindakan Keperawatan IGD & Penerbitan Tagihan
* **Tujuan:** Perawat IGD mencatat tindakan medis operasional keperawatan darurat, yang secara otomatis memicu pencatatan tagihan tindakan ke modul billing.
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-07-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-07.json`
  * **Panggilan API Tindakan:** `POST /api/v1/health-services/clinical-management/patient-procedures/emergency-nursing-actions` (Status `200 OK`, ID: `e0e9f45a-6226-4cbe-81f8-0dc670875990`)
  * **Panggilan API Idempotent Charge Billing:** `POST /api/v1/health-services/billing-management/billing/invoices/from-source` (Status `200 OK`)
  * **Tagihan Tindakan Terbentuk:** Rp 260.000,00 pada Invoice `BIL-20261022-00000001` (ID: `6a739ca1-fb53-482e-95c6-bb8b62fae898`)
* **Deskripsi Alur:** Perawat Dimas menginput tindakan gawat darurat (pemasangan akses IV dan rekam EKG 12 lead). Sistem clinical-to-billing bridge berhasil mencatat tagihan tindakan ke running invoice pasien tanpa hambatan.

---

### E2E-08: Disposisi Pulang IGD (Dokter Konfirmasi & Perawat Eksekusi)
* **Tujuan:** Menuntaskan rencana kepulangan pasien gawat darurat secara multi-disiplin: dokter menetapkan diagnosis kerja ICD-10, merumuskan & mengonfirmasi disposisi Pulang, dilanjutkan perawat mengeksekusi serah terima kepulangan pasien.
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-08-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-08.json`
  * **Penetapan Diagnosis Primer:** `POST /api/v1/health-services/clinical-management/patient-diagnoses` (Status `200 OK`, Diagnosis ICD-10: `I20.0 — Unstable angina`, ID: `a4c62b45-4716-480f-87f9-263b6f521ba9`)
  * **Pembuatan Draf Disposisi Dokter:** `POST /api/v1/health-services/emergency-installation-management/emergency-dispositions` (Status `200 OK`, Kategori: `PLG — Pulang / Rawat Jalan`, ID: `75965c77-19bd-4387-be93-741fb194c75a`)
  * **Konfirmasi Disposisi oleh Dokter:** `PATCH /api/v1/health-services/emergency-installation-management/emergency-dispositions/{id}/disposition-status` (Status `200 OK`, Status: `2 — Confirmed`)
  * **Eksekusi Disposisi oleh Perawat Dimas:** `PATCH /api/v1/health-services/emergency-installation-management/emergency-dispositions/{id}/disposition-status` (Status `200 OK`, Status: `3 — Executed`)
* **Deskripsi Alur:** Fitur *clinical safety guard* sistem mewajibkan penetapan diagnosis kerja primer sebelum disposisi dapat dikonfirmasi. Dokter menetapkan diagnosis `I20.0`, mengonfirmasi disposisi pemulangan rawat jalan, dan Perawat Dimas menekan tombol "Jalankan" pada tab Tindak Lanjut untuk menyelesaikan penanganan pasien.

---

### E2E-09: Verifikasi Penutupan Kunjungan IGD
* **Tujuan:** Memvalidasi bahwa seluruh sistem pelayanan gawat darurat telah menutup siklus hidup kunjungan pasien secara paripurna.
* **Hasil:** **PASS**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-09-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-09.json`
  * **Panggilan API Pemantauan Antrean:** `GET /api/v1/health-services/emergency-installation-management/emergency-visits`
  * **Status Kunjungan:** `9` (`Completed` / Selesai)
  * **Waktu Selesai Kunjungan (Verbatim):** `2026-10-22T07:48:52.930834Z`
* **Deskripsi Alur:** Pada layar antrean pelayanan IGD perawat dan dokter, status badge pasien berubah warna menjadi hijau dengan label verbatim **Selesai**. Pasien tidak lagi tertahan di antrean tindakan aktif.

---

### E2E-10: Verifikasi Rincian Billing Kasir
* **Tujuan:** Kasir membuka modul pembayaran / billing rumah sakit, mencari tagihan pasien berdasarkan No. RM `00-00-02-79`, membuka rincian tagihan, dan memeriksa item biaya tindakan keperawatan IGD yang diterbitkan.
* **Hasil:** **FAIL**
* **Bukti Rekaman:**
  * **Tangkapan Layar:** `E2E-10-akhir.png`
  * **Log Jaringan:** `jaringan-E2E-10.json`
  * **Log Backend Sistem:** `NewQuilvianSystemBackend/Logs/quilvian-backend-20261022.json`
  * **Panggilan API Cashier Overview:** `GET /api/v1/health-services/billing-management/billing/invoices/cashier-overview?pageNumber=1&pageSize=25&periodPreset=ALL` (Status `200 OK`)
  * **Panggilan API Detail Invoice:** `GET /api/v1/health-services/billing-management/billing/invoices/6a739ca1-fb53-482e-95c6-bb8b62fae898`
  * **Status HTTP:** `500 Internal Server Error`
  * **Pesan Layar (Verbatim):** `"Terjadi kesalahan pada server."`
  * **Pesan Respons JSON:** `{"success": false, "statusCode": 500, "message": "Terjadi kesalahan pada server.", "data": null, "errors": null}`
* **Analisis Akar Penyebab (*Root Cause Analysis*):**
  1. Pada layar Cashier Overview, baris tagihan pasien No. RM `00-00-02-79` berhasil muncul dengan nomor invoice `BIL-20261022-00000001`, status pembayaran `UNPAID`, total biaya Rp 260.000,00, dan status kalkulasi `NOT_CALCULATED`.
  2. Saat kasir mengklik menu aksi dropdown **Detail Pembayaran** untuk memeriksa rincian biaya, halaman web membuka rute `/health-services/billing-management/billing/invoices/bil-20261022-00000001-.../detail-billing?tab=details`.
  3. Frontend memanggil endpoint `GET /api/v1/health-services/billing-management/billing/invoices/6a739ca1-fb53-482e-95c6-bb8b62fae898`.
  4. Pada backend `BillingInvoiceService.cs` baris 1112:
     ```csharp
     decimal outstanding;
     if (_closureService != null)
     {
         outstanding = await _closureService.CalculateOutstandingAsync(invoice, currentVersion, cancellationToken);
     }
     ```
  5. Karena invoice IGD baru saja terbentuk dari tindakan klinis dan belum pernah dihitung ulang oleh kasir, variabel `currentVersion` bernilai `null`.
  6. Pada `BillingInvoiceClosureService.cs` baris 67-68:
     ```csharp
     return Math.Max(
         calculation.PatientAmount - paidAmount + allocationExcess - writeOffTotal - adjustmentNet, 0);
     ```
     Parameter `calculation` bernilai `null`, sehingga pemanggilan properti `calculation.PatientAmount` memicu pengecualian **`System.NullReferenceException: Object reference not set to an instance of an object.`**.
  7. Server menangkap error ini melalui `GlobalExceptionMiddleware` dan mengembalikan HTTP 500. Akibatnya, tabel rincian tagihan kasir tidak dapat dirender.

---

## 5. Ringkasan Temuan & Rekomendasi Perbaikan

| Kode | Area | Komponen | Deskripsi Masalah | Rekomendasi Solusi Pengembang |
| :---: | :---: | :---: | :--- | :--- |
| **BUG-E2E-001** | Backend Billing | `BillingInvoiceService.cs` (L1110-1117) & `BillingInvoiceClosureService.cs` (L67) | Endpoint `GET /invoices/{id}` melempar `NullReferenceException` (HTTP 500) saat memuat invoice baru yang belum memiliki baris `BilCalculationVersion` (`currentVersion == null`). | Tambahkan pengecekan null sebelum memanggil closure service: `if (_closureService != null && currentVersion != null)` atau tangani kalkulasi outstanding fallback berbasis item berjalan bila `currentVersion == null`. |

---

## 6. Pernyataan Penutup

Alur pelayanan klinis IGD (Pendaftaran loket, Triase ATS 1 Merah, Pengkajian Keperawatan, DPJP & Pengkajian Medis Dokter, CPPT SOAP, Peresepan Obat, Tindakan Keperawatan, Disposisi Pulang, dan Penutupan Kunjungan) dinyatakan **SELESAI dan LULUS (PASS)** dengan bukti rekaman utuh. Penutupan regresi triase L1R terbukti kokoh dan tidak mengganggu alur pelayanan normal gawat darurat.

Kelemahan integrasi pada modul kasir/billing (E2E-10) telah didokumentasikan secara rinci berbasis berkas log biner dan rekaman jaringan untuk diselesaikan oleh tim rekayasa backend pada siklus perbaikan berikutnya.
