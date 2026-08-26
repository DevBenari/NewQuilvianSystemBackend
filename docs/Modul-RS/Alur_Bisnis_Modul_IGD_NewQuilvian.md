##### **NEW QUILVIAN SYSTEM BACKEND** 

Health Services • Emergency Installation Management 

# **ALUR BISNIS MODUL INSTALASI GAWAT DARURAT (IGD)** 

Pendaftaran Pasien, Pelayanan Kesehatan, dan Penyelesaian Kunjungan 

###### **Keputusan arsitektur utama** 

Jenis kunjungan pasien IGD menggunakan OP, asal kunjungan menggunakan IGD, dan unit pelayanan mengacu pada Service Unit IGD. Data klinis umum disimpan pada Clinical Management, sedangkan proses yang hanya dimiliki IGD disimpan pada Emergency Installation Management. 

|**Dokumen**|Alur Bisnis Modul IGD|
|---|---|
|**Versi**|1.0|
|**Tanggal**|5 Agustus 2026|
|**Cakupan**|Registrasi, triage, yanmed, observasi/resusitasi,<br>disposition, dan penyelesaian kunjungan|



## **1  Ringkasan Dokumen** 

Dokumen ini menjelaskan alur bisnis end-to-end modul IGD mulai pasien tiba, proses triage dan pendaftaran, pelayanan kesehatan, observasi atau resusitasi, hingga pasien pulang, dirawat inap, dirujuk, atau menyelesaikan kunjungan dengan kondisi lainnya. 

#### **Prinsip yang wajib dipertahankan** 

- Encounter pendaftaran IGD menggunakan jenis kunjungan OP dan asal kunjungan IGD. 

- TrxPatientEncounter menjadi pusat episode pelayanan; TrxEmergencyVisit menjadi extension proses khusus IGD. 

- Pasien level Merah atau Oranye boleh langsung ditangani dengan encounter provisional; kelengkapan administrasi diselesaikan setelah kondisi stabil. 

- SOAP, assessment umum, tanda vital, diagnosis, tindakan utama, dan CPPT menggunakan Clinical Management agar tidak diduplikasi per jenis yanmed. 

- Triage, resusitasi, observasi IGD, transfer, dan disposition disimpan pada Emergency Installation Management. 

- Satu episode IGD tidak boleh menghasilkan encounter baru hanya karena administrasi dilengkapi belakangan. 

### **Daftar Bagian** 

|**Bagian**|**Isi**|
|---|---|
|2|Arsitektur proses dan modul yang terlibat|
|3|Alur end-to-end pasien IGD|
|4|Pendaftaran dan penanganan pasien|
|5|Triage dan retriage|
|6|Pelayanan kesehatan/yanmed IGD|
|7|Observasi, resusitasi, dan perpindahan|
|8|Disposition dan proses pasien pulang|
|9|Status, validasi, dan aturan bisnis|
|10|Pemetaan data ke class/modul|
|11|Skenario uji penerimaan|



## **2  Arsitektur Proses dan Modul yang Terlibat** 

Alur IGD tidak berdiri sendiri. Pendaftaran, data klinis, farmasi, penunjang, billing, dan rawat inap terhubung melalui EncounterId yang sama atau hubungan encounter lanjutan yang terkontrol. 

**Emergency Registration Clinical Management → Installation → Management Management ↓ Pharmacy / Lab / Radiology → Billing / Inpatient Management** 

### **2.1 Peran modul** 

|**Modul**|**Tanggung jawab utama**|**Data kunci**|
|---|---|---|
|Registration Management|Membuat dan melengkapi episode<br>kunjungan pasien.|TrxPatientEncounter, guarantor,<br>identitas pasien|
|Emergency Installation Management|Menyimpan proses yang hanya<br>dimiliki IGD.|Emergency visit, triage, resusitasi,<br>observasi, disposition, transfer|
|Clinical Management|Menyimpan transaksi klinis lintas<br>yanmed.|SOAP, assessment, vital sign,<br>diagnosis, procedure, CPPT|
|Pharmacy Management|Menyimpan transaksi resep dan<br>proses farmasi.|Prescription, item, racikan, telaah|
|Laboratory/Radiology Management|Menyimpan order, proses, dan hasil<br>penunjang.|Booking/order dan hasil<br>pemeriksaan|
|Billing/Inpatient Management|Menyelesaikan biaya atau<br>melanjutkan pasien ke rawat inap.|Billing, invoice, admission, room/bed|



###### **Pembeda konteks pelayanan** 

Meskipun jenis kunjungan IGD menggunakan OP, konteks klinisnya tetap IGD melalui asal kunjungan, ServiceUnitId IGD, TrxEmergencyVisit, dan bila tersedia CareSetting = Emergency. 

## **3  Alur End-to-End Pasien IGD** 

Secara umum, satu kunjungan IGD melewati tujuh fase berikut. Pada pasien gawat, urutan registrasi administrasi dapat bergeser tanpa memutus hubungan episode pelayanan. 



<!-- Start of picture text -->
Encounter &<br>Pelayanan<br>Pasien Tiba → Triage Awal → Emergency  →<br>Klinis<br>Visit<br>↓<br>Observasi /  Pulang / Rawat /<br>→ Disposition →<br>Resusitasi Rujuk<br><!-- End of picture text -->

|**Fase**|**Proses bisnis**|**Hasil sistem**|
|---|---|---|
|1. Kedatangan|Pasien diterima di IGD dan cara<br>kedatangan dicatat.|Arrival mode, waktu tiba, keluhan<br>awal|
|2. Triage|Perawat menilai ATS/ESI, ABCDE,<br>tanda bahaya, dan kebutuhan<br>respons.|TrxEmergencyTriage dan detail<br>indikator|
|3. Registrasi|Sistem membuat atau melengkapi<br>encounter OP asal IGD.|TrxPatientEncounter +<br>TrxEmergencyVisit|
|4. Yanmed|Dokter dan perawat melakukan<br>assessment, SOAP, diagnosis,<br>tindakan, resep, dan order<br>penunjang.|Transaksi lintas modul terhubung<br>EncounterId|
|5. Monitoring|Pasien dapat diobservasi, diretriage,<br>atau masuk proses resusitasi.|Observation, resuscitation, CPPT,<br>vital sign|
|6. Keputusan|Dokter menentukan pulang, rawat<br>inap, rujuk, meninggal, atau kondisi<br>lain.|TrxEmergencyDisposition|
|7. Penyelesaian|Administrasi dan billing diselesaikan,<br>transfer dilakukan bila diperlukan,<br>kunjungan ditutup.|Status encounter dan emergency<br>visit menjadi selesai|



## **4  Pendaftaran dan Penanganan Pasien** 

### **4.1 Jalur standar: pasien Level 3-5** 

**1.** Petugas menerima pasien dan melakukan identifikasi awal atau pencarian pasien lama. 

**2.** Perawat triage melakukan penilaian awal dan menentukan level ATS/ESI. 

**3.** Petugas registrasi memilih jenis kunjungan OP, asal kunjungan IGD, dan Service Unit IGD. 

**4.** Sistem membuat TrxPatientEncounter dan TrxEmergencyVisit dengan status Registered/Active. 

**5.** Penjamin, penanggung jawab, identitas, serta informasi rujukan dilengkapi sesuai kebutuhan. 

**6.** Pasien masuk antrean atau ruang pelayanan IGD dan proses klinis dimulai. 

### **4.2 Jalur cepat: pasien Merah atau Oranye** 

###### **Prioritas keselamatan pasien** 

Pasien level 1-2 tidak boleh menunggu proses administrasi lengkap. Sistem harus mengizinkan pelayanan langsung dengan encounter provisional dan identitas minimum. 

**Tiba dalam kondisi gawat** 



<!-- Start of picture text -->
→<br><!-- End of picture text -->



<!-- Start of picture text -->
Triage cepat Level<br>1-2<br><!-- End of picture text -->



<!-- Start of picture text -->
Buat provisional<br>→ encounter<br><!-- End of picture text -->



<!-- Start of picture text -->
↓<br><!-- End of picture text -->

|**Pelayanan /**<br>**resusitasi**|**→**|**Stabilisasi**<br>**→**<br>**Lengkapi**<br>**administrasi**|
|---|---|---|
||**Tahap**|**Ketentuan bisnis**|
|Identitas minimum||Gunakan PatientId existing atau temporary patient bila<br>identitas belum diketahui.|
|Encounter provisional||Tetap dibuat sebelum atau bersamaan dengan<br>pencatatan klinis agar seluruh transaksi memiliki<br>EncounterId.|
|Jenis dan asal||EncounterType = Outpatient/OP; asal kunjungan dan<br>ServiceUnitId = IGD.|
|Pelayanan langsung||Assessment, vital sign, procedure, resusitasi, resep, dan<br>order penunjang boleh dicatat.|
|Pelengkapan data||Setelah pasien stabil, data pasien, penjamin, dan<br>administrasi dilengkapi pada encounter yang sama.|
|Larangan duplikasi||Jangan membuat encounter kedua setelah registrasi<br>final.|



### **4.3 Pasien tanpa identitas** 

- MstEmergencySetting.AllowUnknownPatient harus aktif. 

- Sistem menghasilkan nomor pasien sementara menggunakan TemporaryPatientNumberPrefix. 

- Gunakan alias sementara, jenis kelamin perkiraan, usia perkiraan, dan ciri identifikasi yang relevan. 

- Setelah identitas ditemukan, lakukan proses merge/rekonsiliasi pasien sesuai kebijakan master pasien; jangan menghapus riwayat klinis awal. 

## **5  Triage dan Retriage** 

Triage menentukan prioritas klinis, bukan urutan kedatangan. Master level dan indikator menghindari hardcode aturan di controller maupun frontend. 

|**Level**|**Warna**|**Target respons**|**Implikasi proses**|
|---|---|---|---|
|1|Merah|0 menit|Resusitasi langsung;<br>registrasi administratif<br>dapat menyusul.|
|2|Oranye|≤ 10 menit|Penanganan sangat<br>cepat; provisional<br>encounter diperbolehkan.|
|3|Kuning|≤ 30 menit|Darurat, membutuhkan<br>observasi ketat.|
|4|Hijau|≤ 60 menit|Stabil, dapat mengikuti<br>registrasi standar.|



|**Level**|**Warna**|**Target respons**|**Implikasi proses**|
|---|---|---|---|
|5|Biru/Putih|≤ 120 menit|Tidak gawat; dapat<br>diarahkan sesuai<br>kebijakan rumah sakit.|



### **5.1 Data yang dicatat** 

- Sistem triage: ATS atau ESI. 

- Level, warna, target waktu respons, dan flag pelayanan langsung. 

- Ringkasan ABCDE, keluhan utama, red flag, alasan penetapan level, serta petugas triage. 

- Indikator detail seperti gangguan airway, perdarahan, penurunan kesadaran, nyeri dada, atau kejang. 

- Referensi tanda vital dari Clinical Management bila pencatatannya sudah dilakukan. 

### **5.2 Retriage** 

Retriage dibuat sebagai transaksi baru yang mereferensikan triage sebelumnya. Data triage lama tidak ditimpa karena diperlukan untuk audit klinis dan analisis perubahan kondisi pasien. 



<!-- Start of picture text -->
Triage awal  Kondisi  Retriage  Eskalasi<br>Level 3 → memburuk → Level 2 → penanganan<br><!-- End of picture text -->

## **6  Proses Pelayanan Kesehatan (Yanmed) IGD** 

Setelah encounter dan emergency visit tersedia, tenaga kesehatan mencatat pelayanan klinis menggunakan modul lintas yanmed. Modul IGD hanya menyimpan konteks proses yang unik bagi IGD. 

### **6.1 Data klinis inti** 

|**Aktivitas**|**Penyimpanan utama**|**Keterangan**|
|---|---|---|
|Assessment pasien|ClinicalManagement.TrxPatientAsse<br>ssment|Pengkajian umum dokter/perawat;<br>QueueId sebaiknya nullable untuk<br>IGD.|
|SOAP/konsultasi dokter|ClinicalManagement.TrxDoctorCons<br>ultation|Menyimpan catatan konsultasi<br>dokter terkait EncounterId.|
|Tanda vital|ClinicalManagement.TrxPatientVital<br>Sign|Dapat dicatat berulang dan<br>direferensikan oleh observasi IGD.|
|Diagnosis|ClinicalManagement.TrxPatientDiag<br>nosis|Diagnosis utama/sekunder dan<br>status diagnosis.|
|Tindakan|ClinicalManagement.TrxPatientProc<br>edure|Sumber utama tindakan dan billing;<br>detail khusus IGD berada pada<br>extension.|
|CPPT|ClinicalManagement.TrxPatientInteg<br>ratedProgressNote|Catatan perkembangan lintas<br>profesi.|
|Resep|PharmacyManagement|Transaksi resep final, racikan,<br>telaah, dan substitusi.|



|**Aktivitas**|**Penyimpanan utama**|**Keterangan**|
|---|---|---|
|Laboratorium|LaboratoryManagement|Order/booking, sampling, proses,<br>dan hasil.|
|Radiologi|RadiologyManagement|Order, proses pemeriksaan, dan<br>hasil.|



### **6.2 Tindakan IGD** 

Tindakan utama tetap disimpan sebagai TrxPatientProcedure. Jika terdapat atribut yang hanya berlaku di IGD, sistem menambahkan TrxEmergencyProcedureDetail sebagai extension satu-ke-satu atau satu-kebanyak sesuai desain final. 

|**Contoh**|**Tabel utama**|**Detail khusus IGD**|
|---|---|---|
|Pemberian ATS/TT|TrxPatientProcedure|Hasil skin test, dosis, satuan, rute,<br>waktu pemberian|
|Tindakan saat resusitasi|TrxPatientProcedure|EmergencyResuscitationId dan hasil<br>khusus emergensi|
|Tindakan saat observasi|TrxPatientProcedure|EmergencyObservationId dan<br>konteks monitoring|



### **6.3 Urutan yanmed yang disarankan** 

**1.** Catat vital sign dan assessment awal sesuai kondisi pasien. 

**2.** Dokter membuat konsultasi/SOAP dan diagnosis sementara atau final. 

**3.** Buat tindakan klinis dan detail IGD bila diperlukan. 

**4.** Buat resep atau order penunjang yang terhubung ke EncounterId. 

**5.** Catat perkembangan melalui CPPT dan lakukan retriage jika kondisi berubah. 

**6.** Pastikan billing memperoleh item tindakan, obat, dan pemeriksaan dari sumber transaksi masing-masing. 

## **7  Observasi, Resusitasi, dan Perpindahan** 

### **7.1 Observasi IGD** 

TrxEmergencyObservation menjadi header periode observasi. Catatan berkala disimpan pada 

TrxEmergencyObservationDetail dan dapat mengacu pada vital sign serta CPPT tanpa menyalin ulang data klinis umum. 

|**Header observasi**|**Detail berkala**|
|---|---|
|Nomor observasi, waktu mulai/selesai, indikasi, rencana,<br>lokasi, dokter/perawat penanggung jawab, kesimpulan.|Waktu pencatatan, kondisi klinis, intervensi, respons,<br>intake-output, perdarahan, muntah, referensi vital sign<br>dan CPPT.|



### **7.2 Resusitasi** 

TrxEmergencyResuscitation menyimpan episode resusitasi seperti waktu mulai/selesai, ketua tim, CPR, ROSC, defibrilasi, airway, breathing, circulation, neurologis, dan outcome. Tindakan individual tetap dicatat sebagai TrxPatientProcedure. 

### **7.3 Transfer** 



<!-- Start of picture text -->
Requested → Accepted → Departed → Arrived<br><!-- End of picture text -->

- TrxEmergencyTransfer mencatat unit, ruangan, bed, waktu permintaan, penerimaan, keberangkatan, dan kedatangan. 

- Transfer dapat ditolak; alasan penolakan wajib disimpan. 

- Serah terima klinis harus memuat ringkasan kondisi, terapi berjalan, alat yang terpasang, dan risiko pasien. 

- Transfer bukan pengganti disposition. Disposition adalah keputusan klinis; transfer adalah eksekusi perpindahan. 

## **8  Disposition dan Proses Pasien Pulang** 

Disposition adalah keputusan akhir dokter terhadap kelanjutan pasien setelah pelayanan IGD. Jenisnya mengacu pada MstEmergencyDispositionType dan menghasilkan validasi berbeda. 

|**Disposition**|**Proses lanjutan**|**Syarat sebelum selesai**|
|---|---|---|
|Pulang|Berikan instruksi, obat, kontrol,<br>tanda bahaya, dan edukasi.|Diagnosis dan tindakan final, resep<br>siap, billing selesai/ditindaklanjuti,<br>administrasi lengkap.|
|Rawat inap|Buat admission, tentukan ruang/bed,<br>lalu transfer pasien.|Unit tujuan menerima, handover<br>selesai, transfer berstatus Arrived.|
|Dirujuk|Tentukan fasilitas tujuan, alasan,<br>surat rujukan, dan moda<br>transportasi.|Fasilitas tujuan dan ringkasan klinis<br>tersedia; administrasi rujukan<br>lengkap.|
|Meninggal|Catat waktu dan kondisi meninggal<br>serta proses jenazah/medikolegal.|Keputusan dokter, dokumentasi<br>klinis, dan kebutuhan visum tercatat.|
|Menolak perawatan/PAPS|Catat edukasi risiko dan persetujuan<br>penolakan.|Alasan, saksi/consent, kondisi<br>pasien saat keluar, dan instruksi<br>keselamatan tersedia.|
|Kabur|Catat waktu diketahui, kondisi<br>terakhir, dan tindakan pencarian<br>sesuai SOP.|Audit kejadian dan pelaporan<br>internal selesai.|



### **8.1 Alur pulang normal** 

|**Dokter**<br>**menetapkan**<br>**Pulang**<br>**→**|**Finalisasi**<br>**diagnosis &**<br>**tindakan**|**→**<br>**Resep / hasil /**<br>**edukasi**|
|---|---|---|
||**↓**||
|**Billing & administrasi**|**→**|**Close visit & encounter**|



**1.** Dokter memilih disposition Pulang dan mengisi kondisi pasien, diagnosis akhir, serta instruksi tindak lanjut. 

**2.** Sistem memastikan seluruh assessment, tindakan, resep, hasil penting, dan CPPT sudah tersimpan. 

**3.** Perawat memberikan edukasi, jadwal kontrol, tanda bahaya, dan memastikan pasien memahami instruksi. 

**4.** Billing melakukan finalisasi sesuai penjamin; proses dapat mengikuti aturan tunai, asuransi, atau penagihan. 

**5.** Petugas menyelesaikan administrasi yang masih provisional atau belum lengkap. 

**6.** TrxEmergencyDisposition menjadi Executed/Completed, TrxEmergencyVisit menjadi Completed, dan encounter ditutup sesuai aturan registrasi/billing. 

###### **Validasi penutupan** 

Pasien tidak boleh ditandai selesai hanya karena meninggalkan ruangan. Sistem harus memverifikasi disposition final, kelengkapan administrasi, status billing sesuai kebijakan, dan transfer bila tujuan akhirnya bukan pulang. 

## **9  Status, Validasi, dan Aturan Bisnis** 

### **9.1 Status utama kunjungan** 



<!-- Start of picture text -->
Provisional In Service Disposition<br>Registered Completed<br>Identitas/admin belum  Yanmed sedang  Keputusan akhir<br>Registrasi telah lengkap Kunjungan selesai<br>lengkap berjalan diproses<br><!-- End of picture text -->

### **9.2 Aturan validasi wajib** 

|**Aturan**|**Tujuan**|
|---|---|
|Satu EncounterId hanya memiliki satu<br>TrxEmergencyVisit aktif.|Mencegah duplikasi episode IGD.|
|Encounter IGD harus OP dan berasal dari Service Unit<br>IGD.|Menjaga konsistensi registrasi.|
|Level 1-2 boleh provisional; level lain mengikuti setting.|Mengutamakan keselamatan tanpa kehilangan integritas<br>data.|
|Retriage harus berasal dari emergency visit yang sama.|Menjaga histori kondisi pasien.|
|Observation/resuscitation tidak boleh diakhiri sebelum<br>waktu mulai.|Menjaga validitas kronologi.|
|Disposition Rawat Inap membutuhkan destination<br>unit/admission.|Mencegah pasien ditutup sebelum tempat tujuan<br>tersedia.|
|Disposition Rujuk membutuhkan fasilitas tujuan.|Memastikan proses rujukan dapat<br>dipertanggungjawabkan.|
|Close emergency visit hanya setelah disposition<br>dieksekusi.|Mencegah penutupan prematur.|
|Soft delete dan audit field wajib digunakan.|Mempertahankan audit trail data klinis.|



### **9.3 Logging** 

Controller menggunakan LoggerService untuk operasi yang mengubah data. GET daftar dan detail tidak perlu dicatat ke custom logger agar volume log tetap terkendali. 

|**Dicatat**|**Tidak dimasukkan ke payload log**|
|---|---|
|Create, update, perubahan status, delete/soft delete,<br>dan kegagalan workflow penting.|Nama pasien, nomor rekam medis, isi SOAP, diagnosis,<br>hasil pemeriksaan, atau payload klinis lengkap.|



## **10  Pemetaan Data ke Class dan Modul** 

|**Tahap proses**|**Class utama**|**Modul**|
|---|---|---|
|Episode kunjungan|TrxPatientEncounter|Registration Management|
|Header IGD|TrxEmergencyVisit|Emergency Installation Management|
|Triage|TrxEmergencyTriage,<br>TrxEmergencyTriageDetail|Emergency Installation Management|
|Resusitasi|TrxEmergencyResuscitation|Emergency Installation Management|
|Observasi|TrxEmergencyObservation,<br>TrxEmergencyObservationDetail|Emergency Installation Management|
|Assessment/SOAP|TrxPatientAssessment,<br>TrxDoctorConsultation|Clinical Management|
|Vital sign/CPPT|TrxPatientVitalSign,<br>TrxPatientIntegratedProgressNote|Clinical Management|
|Diagnosis|TrxPatientDiagnosis|Clinical Management|
|Tindakan|TrxPatientProcedure +<br>TrxEmergencyProcedureDetail|Clinical + Emergency Installation<br>Management|
|Resep|TrxPrescription dan detailnya|Pharmacy Management|
|Penunjang|Order/booking dan hasil|Laboratory/Radiology Management|
|Keputusan akhir|TrxEmergencyDisposition|Emergency Installation Management|
|Perpindahan|TrxEmergencyTransfer|Emergency Installation Management|



### **10.1 Master data IGD** 

|**Master**|**Fungsi**|
|---|---|
|MstEmergencyTriageLevel|Definisi level ATS/ESI, warna, target respons, dan izin<br>pelayanan sebelum registrasi.|



|**Master**|**Fungsi**|
|---|---|
|MstEmergencyTriageIndicator|Indikator klinis yang mendukung penetapan level triage.|
|MstEmergencyArrivalMode|Cara pasien datang: mandiri, keluarga, ambulans, polisi,<br>atau rujukan.|
|MstEmergencyCaseType|Klasifikasi kasus seperti trauma, non-trauma,<br>kecelakaan, obstetri, keracunan, dan bencana.|
|MstEmergencyDispositionType|Pilihan hasil akhir dan aturan field wajib untuk setiap<br>disposition.|
|MstEmergencySetting|Kebijakan provisional registration, unknown patient, level<br>cepat, prefix nomor, dan default unit IGD.|



## **11  Skenario Uji Penerimaan** 

|**No.**|**Skenario**|**Hasil yang diharapkan**|
|---|---|---|
|1|Pasien lama Level 4 datang mandiri.|Registrasi OP asal IGD selesai<br>sebelum yanmed; emergency visit<br>aktif.|
|2|Pasien tidak dikenal Level 1 tiba<br>dengan ambulans.|Temporary patient dan provisional<br>encounter dibuat; resusitasi dapat<br>dicatat tanpa menunggu<br>administrasi.|
|3|Pasien awal Level 3 memburuk<br>menjadi Level 2.|Retriage baru dibuat, triage lama<br>tetap tersimpan, pelayanan<br>dieskalasi.|
|4|Pasien diobservasi dan vital sign<br>dicatat setiap 30 menit.|Observation detail mereferensikan<br>vital sign; tidak terjadi duplikasi data.|
|5|Dokter memutuskan rawat inap.|Disposition membutuhkan<br>admission/unit tujuan; transfer<br>sampai Arrived sebelum IGD<br>selesai.|
|6|Dokter memutuskan pulang.|Instruksi, resep, billing, dan<br>administrasi selesai sebelum close<br>visit.|
|7|Pasien menolak perawatan.|Alasan, consent, kondisi saat keluar,<br>dan audit log tersimpan.|
|8|Pengguna mencoba membuat<br>emergency visit kedua pada<br>encounter yang sama.|Sistem menolak duplikasi.|



### **11.1 Checklist selesai implementasi** 

- Migration berhasil dan seluruh foreign key/navigation property terbentuk sesuai configuration. 

- Setting default IGD tersedia dan menunjuk Service Unit IGD aktif. 

- Level triage ATS/ESI dan indikator awal sudah di-seed. 

- Controller memiliki AccessController, AccessAction, AccessPermission, dan LoggerService. 

- Alur pasien Merah/Oranye dapat berjalan tanpa administrasi lengkap tetapi tetap memiliki EncounterId. 

- Clinical Management menerima transaksi IGD tanpa ketergantungan QueueId wajib. 

- Disposition, transfer, billing, dan close encounter diuji end-to-end. 

- Data audit dan soft delete terverifikasi. 

###### **Hasil akhir yang diharapkan** 

Satu episode IGD dapat ditelusuri penuh dari pasien tiba sampai selesai melalui EncounterId, tanpa menduplikasi data klinis umum dan tanpa menghambat pelayanan pasien gawat. 

_— Akhir Dokumen —_ 

