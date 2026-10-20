# Analisis Mendalam Sistem Kerja dan Transaksi Dokter Rawat Inap

| Field | Nilai |
| :--- | :--- |
| **Produk** | Quilvian Hospital Information System (QuilvianFinal) |
| **Modul / Sub-Modul** | Rawat Inap (`InPatientManagement`) / Dokter Rawat Inap (`dokter-rawat-inap`) |
| **Lokasi Dokumen** | `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/docs/` |
| **Tanggal Analisis** | 20 Oktober 2026 |
| **Sasaran Pembaca** | Pemilik Proses Bisnis, Manajemen Rumah Sakit, Dokter DPJP, Komite Medis, Tim IT & Engineering |
| **Status Dokumen** | **Laporan Analisis Bisnis & Arsitektur Sistem** |

---

## 1. Ringkasan Eksekutif

Dokumen ini menyajikan **analisis komprehensif mengenai sistem kerja dan transaksi klinis dokter rawat inap** yang saat ini telah diterapkan di Quilvian, ditinjau dari **kacamata proses bisnis rumah sakit** dan **arsitektur teknis perangkat lunak**.

Dalam operasional rumah sakit nyata, pelayanan rawat inap memiliki sifat **pelayanan berkesinambungan lintas-hari (*continuous multi-day care*)**. Pasien dirawat di bangsal selama berhari-hari bahkan berminggu-minggu, diperiksa oleh Dokter Penanggung Jawab Pelayanan (DPJP) setiap hari (visite), menerima terapi obat harian yang dinamis, serta menjalani tindakan medis dan pemeriksaan penunjang sewaktu-waktu sesuai perkembangan klinis.

Saat ini, Quilvian telah berhasil membangun ruang kerja dokter (*Physician Workspace*) modern yang mengintegrasikan rekam medis elektronik, peresepan obat, pemesanan tindakan, dan pemeriksaan penunjang. Namun, hasil penelusuran mendalam menemukan sebuah **benturan arsitektur kritis (*paradoks poliklinik*)**:
> **Beberapa modul transaksi inti (Klinis, Farmasi, dan Kasir) masih membawa aturan kanonikal milik Rawat Jalan (Poliklinik). Di poliklinik, pemeriksaan pasien selesai dalam satu kali pertemuan linier. Ketika dokter rawat inap menekan tombol *"Selesaikan & Kunci"* pada catatan visite harian, sistem secara keliru menganggap seluruh sesi perawatan pasien telah tamat, sehingga mengunci pembuatan resep obat baru dan memutasi status kunjungan admisi secara prematur.**

Dokumen ini membedah tuntas seluruh transaksi yang sudah berjalan, menjelaskan mengapa resep tersimpan dalam status *Draf*, membongkar akar masalah rantai transaksi, dan memberikan rekomendasi bisnis serta alur kerja ideal bagi rumah sakit.

---

## 2. Karakteristik Bisnis: Rawat Jalan vs Rawat Inap

Untuk memahami mengapa transaksi di sistem saat ini mengalami kendala, manajemen dan pengembang wajib memahami perbedaan mendasar antara model bisnis Rawat Jalan dan Rawat Inap:

```text
+----------------------------------------------------------------------------------------------------+
|                                PERBEDAAN PROSES BISNIS KLINIS                                      |
+------------------------------------+---------------------------------------------------------------+
| KARAKTERISTIK                      | RAWAT JALAN (POLIKLINIK)       | RAWAT INAP (BANGSAL/INPATIENT)|
+------------------------------------+--------------------------------+-------------------------------+
| Durasi Pelayanan                   | Singkat (10–30 menit)          | Panjang (3 hari s.d. berminggu)|
| Sifat Kunjungan                    | Sesi tunggal (sekali selesai)  | Berkesinambungan (multi-hari) |
| Frekuensi Pemeriksaan Dokter       | 1 kali per pertemuan           | 1–2 kali per hari (visite)    |
| Frekuensi Peresepan Obat           | 1 kali resep dibawa pulang     | Berkali-kali (resep harian,   |
|                                    |                                | obat infus, cito, obat pulang)|
| Makna "Selesai Konsultasi"         | Pasien selesai diperiksa,      | Dokter selesai mencatat visite|
|                                    | dipersilakan ke kasir/farmasi  | hari ini; pasien tetap dirawat|
| Siklus Tagihan (Billing)           | Seketika saat itu juga         | Akumulatif hingga discharge   |
+------------------------------------+--------------------------------+-------------------------------+
```

---

## 3. Enam Pilar Transaksi Dokter Rawat Inap yang Sudah Diterapkan

Sistem ruang kerja dokter rawat inap (*Inpatient Physician Workspace*) pada sub-modul `dokter-rawat-inap` dirancang sebagai **pusat kendali klinis satu pintu** bagi dokter. Terdapat enam transaksi utama yang telah dibangun di sistem:

```text
                        RUANG KERJA DOKTER RAWAT INAP (FE-DOK-09)
                                          │
    ┌────────────────┬────────────────────┼───────────────────┬────────────────┐
    ▼                ▼                    ▼                   ▼                ▼
[1. Asesmen     [2. Catatan          [3. Peresepan       [4. Pemesanan    [5. Pemeriksaan
 Awal Medis]     SOAP Visite]         Obat Bangsal]       Tindakan Medis]  Penunjang]
 (24 Jam Pertama)(Harian DPJP)        (Harian/Pulang)     (Dokter/Perawat) (Lab/Rad/Darah)
```

---

### Pilar 1: Konteks Pasien & Kewenangan DPJP

#### 1. Konsep Bisnis
Tidak sembarang dokter boleh mengubah data medis pasien rawat inap. Berdasarkan regulasi akreditasi rumah sakit (KARS/JCI) dan Permenkes, setiap pasien yang dirawat wajib memiliki satu **Dokter Penanggung Jawab Pelayanan (DPJP)** utama yang memegang kendali klinis tertinggi, dibantu oleh Dokter Jaga Ruangan dan Dokter Konsulen.

#### 2. Penerapan di Sistem Saat Ini
- **Komponen Frontend:** [`inpatient-episode-header.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/components/inpatient-episode-header.jsx) dan [`useInpatientPhysicianWorkspace.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-physician-workspace.jsx).
- **Logika Penegakan:** Fungsi [`resolveWorkspaceWriteAccess`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-physician-workspace-utils.jsx#L188-L282) memeriksa secara ketat:
  1. Apakah akun dokter yang sedang login terhubung dengan identitas dokter yang sah (`hasDoctorIdentity`).
  2. Apakah dokter tersebut adalah DPJP aktif yang ditugaskan pada episode rawat inap ini (`isActiveDoctor`).
  3. Apakah episode perawatan pasien masih aktif berjalan (bukan `Closed` atau `Cancelled`).
- **Peringatan Keselamatan Alergi:** Sistem menampilkan banner peringatan alergi (*Allergy Alert Badge*) di baris atas layar secara permanen. Jika data alergi gagal dimuat dari server, sistem menampilkan peringatan khusus dan tidak menganggapnya sebagai "bebas alergi", demi keselamatan pasien (*patient safety*).

---

### Pilar 2: Kajian Medis Awal Rawat Inap (*Admission Assessment*)

#### 1. Konsep Bisnis
Dalam 24 jam pertama sejak pasien masuk kamar perawatan, DPJP wajib menyelesaikan **Pengkajian Medis Awal Rawat Inap**. Kajian ini memuat riwayat penyakit sekarang, riwayat penyakit dahulu, pemeriksaan fisik lengkap *head-to-toe*, diagnosis kerja awal, dan rencana tatalaksana awal rawat inap.

#### 2. Penerapan di Sistem Saat Ini
- **Komponen:** Sub-tab Asesmen Medis Awal (`medical-assessment-tab.jsx`), controller backend [`PatientAssessmentController.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs).
- **Alur Data:** Dokter mengisi formulir asesmen medis, memilih diagnosis primer/sekunder dari master ICD-10, lalu menandatangani dokumen asesmen. Dokumen ini menjadi dasar penetapan rencana perawatan pasien selama menginap.

---

### Pilar 3: Visite Dokter & Catatan Perkembangan (SOAP / CPPT)

#### 1. Konsep Bisnis
Setiap hari (biasanya pagi hari), dokter melakukan pemeriksaan langsung ke tempat tidur pasien (*bedside examination*). Dokter mengevaluasi keluhan subjektif, hasil pemeriksaan fisik objektif terbaru, merumuskan penilaian klinis (*assessment*), dan menyusun instruksi/rencana medis (*plan*) ke dalam formulir SOAP. 

Sesuai **Permenkes No. 24 Tahun 2022 tentang Rekam Medis**:
- Catatan rekam medis elektronik yang telah dibuat dan ditandatangani oleh tenaga medis wajib **dikunci (*sealed/locked*)**.
- Catatan yang terkunci tidak boleh dihapus atau ditimpa secara diam-diam.
- Bila ada kekeliruan ketik atau revisi data, koreksi wajib dicatat lewat **catatan pembetulan beralasan (*Addendum*)** yang tersimpan secara transparan dengan jejak audit lengkap (*audit trail*).

#### 2. Penerapan di Sistem Saat Ini
- **Komponen:** [`physician-progress-tab.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/progress-note/physician-progress-tab.jsx) dan [`soap-editor.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/progress-note/soap-editor.jsx).
- **Struktur Formulir SOAP:**
  - **Subjective (S):** Keluhan yang dirasakan pasien (misal: pusing berkurang, mual muntah).
  - **Objective (O):** Terintegrasi otomatis dengan grafik TTV (Tanda-Tanda Vital) perawat bangsal (tekanan darah, nadi, suhu, pernapasan, saturasi oksigen, skala nyeri 1–10).
  - **Assessment (A):** Diagnosis kerja dan evaluasi perbaikan klinis berdasarkan master ICD-10.
  - **Plan (P):** Rencana instruksi dokter, panel rekomendasi terapi obat, tindakan penunjang, dan edukasi pasien.
- **Tombol Aksi Bawah:**
  - `[Reset]`: Mengembalikan formulir ke isian awal.
  - `[Simpan Draf]`: Menyimpan catatan perkembangan sementara tanpa mengunci dokumen. Dokter masih dapat menyunting kembali teks SOAP sewaktu-waktu.
  - `[Selesaikan & Kunci]`: Menandatangani catatan medis secara elektronik dan mendaftarkannya ke mesin keutuhan rekam medis ([`ClinicalDocumentIntegrityService.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs)). Begitu dikunci, catatan berpindah ke Riwayat SOAP dan hanya dapat dikoreksi melalui fitur addendum resmi.

---

### Pilar 4: Peresepan Obat Rawat Inap (*Inpatient Prescription*)

#### 1. Konsep Bisnis
Obat di rawat inap diberikan secara dinamis. Ada obat yang diberikan setiap hari secara rutin (*resep harian/daily*), obat yang harus segera diberikan dalam keadaan gawat darurat (*cito/PRN*), obat racikan khusus anak/lansia (*compounding*), obat insulin berkala (*sliding scale*), serta obat yang diresepkan saat pasien akan pulang (*discharge medication*).

#### 2. Penerapan di Sistem Saat Ini
- **Komponen:** [`inpatient-prescription-tab.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/inpatient-prescription-tab.jsx) dan [`prescription-builder-panel.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-builder-panel.jsx).
- **Fitur yang Tersedia:**
  1. **Buat Resep:** Katalog obat terintegrasi penjamin asuransi, perhitungan dosis, frekuensi, aturan pakai, dan rute pemberian (oral/injeksi/infus).
  2. **Obat Racikan (*Compound Builder*):** Peracikan beberapa bahan obat ke dalam bentuk kapsul, pulvis, atau sirup dengan instruksi pembuatannya.
  3. **Template Resep Pribadi (`ownerScope=Mine`):** Dokter dapat menyimpan paket obat favoritnya (misalnya: *Template DHF Dewasa*, *Template Post-Op Caesar*) untuk dipanggil ulang dengan 1 klik.
  4. **History Resep Episode:** Menampilkan daftar seluruh resep yang pernah diterbitkan pada episode rawat inap ini.
  5. **Resep Harian (*Daily Monitoring*):** Menampilkan pemantauan obat yang sedang aktif dikonsumsi pasien per hari.
  6. **Rekonsiliasi Obat (*Medication Reconciliation*):** Mencatat dan menyelaraskan obat yang dibawa pasien dari rumah/luar RS.
  7. **Sliding Scale:** Pengaturan protokol dosis insulin bertingkat berdasarkan hasil monitoring gula darah sewaktu (GDS).

---

### Pilar 5: Pemesanan Tindakan Medis (*Procedure Orders*)

#### 1. Konsep Bisnis
Selama dirawat, pasien menerima tindakan medis, baik tindakan besar oleh dokter (misal: pemasangan CVC, debridement, punksi pleura) maupun tindakan yang diinstruksikan dokter untuk didelegasikan pelaksanaannya kepada perawat bangsal (misal: nebulisasi, rawat luka, pasang selang NGT/kateter).

#### 2. Penerapan di Sistem Saat Ini
- **Komponen:** [`inpatient-procedure-tab.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/inpatient-procedure-tab.jsx) dan [`procedure-form-panel.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-form-panel.jsx).
- **Alur Kerja:**
  - **Pemesanan Tindakan Langsung:** Dokter memilih tindakan medis dari katalog, sistem memvalidasi tarif berdasarkan kelas kamar pasien dan cakupan asuransi penjamin, lalu dokter menyimpannya ke daftar tindakan episode.
  - **Instruksi Delegasi & Verifikasi DPJP:** Jika tindakan dilakukan oleh perawat berdasarkan instruksi lisan/telepon dokter, perawat mencatat pesanan tindakan tersebut, dan sistem memasukkannya ke dalam **Daftar Tunggu Verifikasi Instruksi Dokter** (*Instruction Verification Worklist*). DPJP kemudian membuka tab verifikasi dan menandatanganinya sebagai verifikasi resmi instruksi medis.

---

### Pilar 6: Pemesanan Pemeriksaan Penunjang (*Ancillary Orders*)

#### 1. Konsep Bisnis
Dokter bangsal membutuhkan pemeriksaan berkala seperti laboratorium darah rutin harian (evaluasi hemoglobin, leukosit, trombosit), foto rontgen radiologi, permintaan kantong darah ke Bank Darah Rumah Sakit (BDRS), atau penjadwalan cuci darah (hemodialisa).

#### 2. Penerapan di Sistem Saat Ini
- **Komponen:** [`supporting-service-tab.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx) dan modal order penunjang.
- **Integrasi Antar-Modul:** Pemesanan laboratorium diteruskan langsung ke antrean analis Laboratorium, radiologi ke unit Radiologi, dan permintaan darah ke sistem BDRS. Hasil pemeriksaan yang telah divalidasi oleh dokter spesialis patologi/radiologi dapat langsung dibaca oleh DPJP pada riwayat penunjang pasien.

---

## 4. Penjelasan Mendalam: Mengapa Status Resep Masih "Draf"?

Terkait tangkapan layar yang Anda kirimkan, di mana resep yang baru disimpan berstatus:
- **Status Resep:** `Draf`
- **Status Farmasi:** `Menunggu Finalisasi Klinis`

```text
[ Dokter Buat Resep ] ──► [ Simpan Draf Resep ] ──► Status: DRAF
                                                          │
                                                          ▼
                                            Farmasi: "Menunggu Finalisasi Klinis"
                                            (Apoteker belum boleh mengeluarkan obat)
                                                          │
                               ┌──────────────────────────┴──────────────────────────┐
                               ▼                                                     ▼
                  [ ALUR SAAT INI (TERGANTUNG SOAP) ]                  [ KEBUTUHAN ALUR MANDIRI ]
                 Dokter wajib ke form SOAP, lalu tekan                 Dokter menekan tombol langsung
                 tombol "Selesaikan & Kunci" di SOAP.                  "Finalkan & Kirim ke Farmasi"
                               │                                       di Tab Resep itu sendiri.
                               ▼                                                     ▼
                 Resep otomatis berubah ke:                            Resep berubah ke:
                 Status: SUBMITTED (Terkirim ke Farmasi)               Status: SUBMITTED (Terkirim ke Farmasi)
```

### 1. Alasan Klinis Status Draf
Sistem sengaja tidak langsung mengirim resep ke instalasi Farmasi saat tombol *"Simpan Draf Resep"* ditekan. Ini adalah mekanisme pengaman klinis (*clinical safeguard*):
- Dokter sering kali memilih obat pertama, lalu berpikir untuk menyesuaikan antibiotik kedua, atau perlu menambahkan cairan infus.
- Status `Draf` memungkinkan dokter merevisi, membatalkan item, atau menambahkan racikan tanpa mengacaukan antrean dispensing apotek di lantai bawah.
- Selama berstatus Draf, apoteker di Farmasi hanya dapat melihat daftar obat tersebut sebagai informasi awal (*read-only*), tetapi **belum berwenang menyiapkan atau memotong stok obat**.

### 2. Aturan Teknis di Balik Status Tersebut
Di kode backend [`PrescriptionWorkflowService.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkflowService.cs#L39-L46), setiap resep baru otomatis diberi tiga penanda status kanonikal:
1. `PrescriptionStatus = PrescriptionStatus.Draft` (Resep berstatus Draf).
2. `PaymentStatus = PrescriptionPaymentStatus.NotBilled` (Belum ditagihkan).
3. `FulfillmentStatus = PrescriptionFulfillmentStatus.WaitingForClinicalFinalization` (Menunggu Finalisasi Klinis dari dokter).

### 3. Kapan Resep Ini Berubah Menjadi "Terkirim ke Farmasi"?
Di sistem saat ini, **tidak ada tombol "Kirim Resep" terpisah pada tab resep**. Sesuai aturan kanonikal pada [`PrescriptionWorkflowService.cs:81-89`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkflowService.cs#L81-L89):
> *"Resep tidak diajukan secara terpisah. Selesaikan konsultasi dokter untuk memfinalkan resep."*

Artinya, resep draf tersebut **baru akan otomatis difinalkan dan terkirim ke Farmasi (*Submitted*) ketika dokter membuka formulir SOAP dan menekan tombol *"Selesaikan & Kunci"***.

---

## 5. Analisis Masalah Kritis: Mengapa Tombol "Selesaikan & Kunci" Melumpuhkan Transaksi Rawat Inap?

Inilah akar persoalan mendasar yang menjadi keluhan Anda:
> *"Kenapa pada saat menekan button Selesaikan & Kunci, seluruh transaksi pada dokter rawat inap itu selesai dan tidak bisa dilakukan lagi, ketika melakukan input resep dan lain-lain?"*

### 1. Rantai Peristiwa yang Terjadi di Balik Layar

Ketika dokter menekan tombol **"Selesaikan & Kunci"** di formulir SOAP:

```text
[ Dokter Tekan "Selesaikan & Kunci" di Form SOAP ]
                     │
                     ▼
    Panggilan ke Backend: CompleteConsultation
                     │
                     ▼
    Eksekusi: ConsultationFinalizationService.FinalizeAsync
                     │
                     ├── 1. Ubah Status SOAP menjadi Completed (Terkunci & Ditandatangani)
                     │
                     ├── 2. Finalkan Resep Draf yang sedang menempel menjadi Submitted
                     │
                     ├── 3. [FATAL] Ubah Status Kunjungan Pasien (Encounter.EncounterStatus)
                     │      menjadi ConsultationCompleted!
                     │
                     └── 4. [FATAL] Terbitkan Milestone Tagihan Jasa Poliklinik ke Kasir!
```

### 2. Tiga Akar Penyebab Teknis yang Melumpuhkan Transaksi

#### A. Penyebab 1: Jebakan Urutan Validasi Backend (`PrescriptionController.cs:749`)
Pada saat dokter kemudian berpindah ke tab Resep untuk membuat resep baru bagi pasien tersebut:
- Frontend [`use-inpatient-prescription-tab.jsx:165-167`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx#L165-L167) secara otomatis mengaitkan resep ke catatan SOAP terakhir yang baru saja di-*Completed*-kan oleh dokter.
- Ketika dokter menekan simpan resep, backend mengeksekusi pemeriksaan pada [`PrescriptionController.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs#L747-L760):
  ```csharp
  // Baris 749: LANGSUNG MENOLAK JIKA COMPLETED
  if (consultation.ConsultationStatus == DoctorConsultationStatus.Completed) 
      return (false, "Konsultasi yang sudah completed tidak dapat ditambahkan resep.");

  // Baris 759: Pengecualian rawat inap diletakkan TERLAMBAT di bawah!
  if (consultation.InpEpisodeId.HasValue)
      return (true, null);
  ```
- **Akibatnya:** Baris 749 langsung memblokir permintaan dengan pesan galat *"Konsultasi yang sudah completed tidak dapat ditambahkan resep."*. Padahal di rawat inap, catatan visite yang sudah `Completed` (sudah ditandatangani dokter) justru merupakan acuan instruksi medis yang sah untuk menerbitkan resep obat!

#### B. Penyebab 2: Kerusakan Status Kunjungan Pasien (`EncounterStatus`)
Pada [`ConsultationFinalizationService.cs:189-194`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs#L189-L194):
- Service finalisasi poliklinik mengubah status kunjungan admisi rawat inap menjadi `ConsultationCompleted`.
- Di mata sistem, status `ConsultationCompleted` berarti *"pemeriksaan dokter telah selesai, pasien keluar dari ruang periksa menuju apotek/kasir"*. Ini merusak konsep rawat inap, di mana pasien sebenarnya masih menginap di kamar (*Admitted*) dan masa rawatnya belum berakhir.

#### C. Penyebab 3: Tidak Ada Tombol Finalisasi Resep Mandiri
Karena sistem mengharuskan finalisasi resep menumpang pada penekanan tombol *"Selesaikan & Kunci"* di form SOAP, dokter bangsal terkunci dalam **lingkaran setan**:
- Jika dokter mengunci SOAP duluan: Dokter dilarang membuat resep baru karena konsultasi sudah selesai.
- Jika dokter membuat resep duluan: Resep tertahan selamanya di status Draf dan tidak pernah sampai ke Farmasi sebelum form SOAP dikunci.
- Jika ada dokter spesialis konsulen atau dokter jaga malam yang ingin meresepkan obat darurat tanpa membuat SOAP visite penuh: Mereka tidak bisa mengirim resep langsung ke Farmasi.

---

## 6. Skenario Nyata di Rumah Sakit: Sebelum vs Sesudah Perbaikan

Untuk melihat dampak bisnisnya secara nyata, mari kita tinjau simulasi skenario pelayanan pasien rawat inap berikut:

```text
Kasus: Pasien Ny. Siti (45 tahun), dirawat inap hari ke-2 karena Demam Tifoid di Ruang Mawar Bed 03.
DPJP: dr. Ahmad, Sp.PD.
```

### Skenario SEBELUM Perbaikan (Kondisi Bermasalah Saat Ini)

1. **Pukul 08:00 (Visite Pagi):**
   - dr. Ahmad memeriksa Ny. Siti di kamar rawat, lalu membuka HIS Quilvian di Nurse Station.
   - dr. Ahmad mengisi formulir SOAP visite pagi: mencatat suhu 38.5°C, lidah kotor, nyeri perut.
   - Sesuai instruksi legal rekam medis, dr. Ahmad menekan tombol **"Selesaikan & Kunci"** agar catatan medis visite hari ini sah ditandatangani.
   - *Sistem merespons:* Catatan SOAP terkunci. Namun di balik layar, status kunjungan Ny. Siti diubah menjadi `ConsultationCompleted` dan konsultasi ditandai selesai.
2. **Pukul 09:30 (Instruksi Obat Tambahan):**
   - Hasil lab darah Ny. Siti keluar dari laboratorium, menunjukkan leukopenia.
   - dr. Ahmad membuka Tab Resep untuk menambahkan injeksi antibiotik *Ceftriaxone 1g vial*.
   - dr. Ahmad memilih obat, aturan pakai 2x1 gram IV, lalu menekan **"Simpan Draf Resep"**.
   - *Sistem menolak:* Muncul peringatan merah:
     > *"Konsultasi yang sudah completed tidak dapat ditambahkan resep."*
   - dr. Ahmad mencoba memilih opsi catatan pengait lain di dropdown, tetapi semua catatan hari kemarin dan hari ini sudah berstatus `Completed`.
   - **Hasil Akhir:** dr. Ahmad tidak bisa meresepkan antibiotik lewat sistem komputer. Obat tertunda, perawat bingung, dan instruksi terpaksa ditulis manual di kertas resep cadangan.

---

### Skenario SESUDAH Perbaikan (Solusi `PLAN-REPAIR-007`)

1. **Pukul 08:00 (Visite Pagi):**
   - dr. Ahmad memeriksa Ny. Siti, mengisi SOAP visite pagi, lalu menekan tombol **"Selesaikan & Kunci"**.
   - *Sistem merespons:* Catatan SOAP pagi ditandatangani secara elektronik dan terkunci aman sesuai Permenkes 24/2022.
   - Status kunjungan admisi Ny. Siti **tetap berstatus aktif (*Admitted*)** dan tidak diubah menjadi selesai.
2. **Pukul 09:30 (Instruksi Obat Tambahan):**
   - dr. Ahmad membuka Tab Resep untuk meresepkan *Ceftriaxone 1g vial*.
   - Form resep mengaitkan resep ke catatan visite jam 08:00 yang berstatus `[Terkunci/Sah]`.
   - dr. Ahmad menekan **"Simpan Resep"**.
   - *Sistem menerima:* Backend mengenali bahwa pasien berada dalam episode rawat inap aktif (`consultation.InpEpisodeId.HasValue`), sehingga status catatan yang sudah `Completed` **diizinkan** menjadi dasar peresepan resmi. Resep langsung terbit dengan status sah.
3. **Pukul 09:35 (Pemenuhan Farmasi):**
   - Bagian Farmasi bangsal langsung menerima pesanan resep obat tersebut, menyiapkan vial antibiotik, dan menyerahkannya ke perawat bangsal untuk disuntikkan ke pasien tepat waktu.
   - **Hasil Akhir:** Pelayanan pasien berjalan mulus, cepat, aman secara medikolegal, dan seluruh transaksi tercatat rapi di rekam medis elektronik.

---

## 7. Rangkuman Solusi dan Rekomendasi Bisnis

Berdasarkan analisis di atas, telah disusun rencana perbaikan resmi pada dokumen [plan-repair-007-penyelesaian-soap-mengunci-seluruh-transaksi.md](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/docs/plan-repair/plan-repair-007-penyelesaian-soap-mengunci-seluruh-transaksi.md) yang mencakup tiga perbaikan terukur:

| ID Perbaikan | Tindakan Teknis | Manfaat Bisnis bagi Rumah Sakit |
| :--- | :--- | :--- |
| **`FIX-DOK-007-01`** | Memperbaiki urutan validasi pada `PrescriptionController.cs`: Jika resep dibuat untuk pasien rawat inap (`consultation.InpEpisodeId.HasValue`), sistem mengizinkan peresepan pada catatan visite yang sudah `Completed`. | Dokter dapat meresepkan obat kapan saja sepanjang episode rawat inap tanpa terblokir status catatan visite. |
| **`FIX-DOK-007-02`** | Menyaring logika di `ConsultationFinalizationService.cs` agar mutasi status kunjungan admisi (`EncounterStatus = ConsultationCompleted`) **tidak dijalankan** pada rawat inap. | Status perawatan pasien tetap aktif di kamar rawat inap dan tidak prematurely dianggap selesai oleh bagian kasir/pendaftaran. |
| **`FIX-DOK-007-03`** | Mengecualikan rawat inap dari emisi milestone tagihan jasa poliklinik di `ConsultationFinalizationService.cs`. | Mencegah tagihan ganda atau tagihan salah jenis masuk ke tagihan pasien rawat inap. Jasa dokter tetap ditagihkan sesuai tarif visite rawat inap yang benar. |

### Rekomendasi Jangka Menengah untuk Desain Produk (To-Be Enhancement)
Di masa mendatang, pada antarmuka Tab Resep Rawat Inap (`inpatient-prescription-tab.jsx`), disarankan untuk menyediakan tombol aksi mandiri:
1. **`[Simpan Draf]`**: Untuk menyimpan draf racikan yang masih ingin diedit kembali.
2. **`[Finalkan & Kirim ke Farmasi]`**: Untuk memfinalkan resep secara mandiri langsung dari tab Resep tanpa memaksa dokter membuka kembali dan mengunci formulir SOAP visite.

---

## 8. Kesimpulan

Sistem kerja dokter rawat inap di Quilvian saat ini telah memiliki fondasi fitur klinis yang sangat lengkap (asesmen awal, SOAP, TTV terintegrasi, katalog obat penjamin, racikan, template, tindakan, dan penunjang). 

Kendala transaksi yang terjadi saat ini bukan karena fitur belum dibuat, melainkan akibat **kebocoran aturan validasi Poliklinik Rawat Jalan ke alur Rawat Inap**. Dengan menerapkan tiga langkah perbaikan terisolasi pada `plan-repair-007`, seluruh transaksi dokter rawat inap akan berjalan lancar, berkesinambungan, dan sepenuhnya selaras dengan alur operasional rumah sakit modern.
