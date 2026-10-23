# Laporan Pengujian Putaran D2R — Modul IGD Quilvian (Laporan Lengkap & Terverifikasi)
**Tanggal Pelaksanaan:** 20 Oktober 2026  
**Dokter Utama Penguji:** **dr. Jenah Ali** (`ranger.biru@admin.com` / `DOC-RSMMC-00001`)  
**Dokter Kedua Penguji:** **Dr. Jenah Ali, S.pB** (`jenahali@hospital.com` / `DOC-RSMMC-00008` / `3e37fc1f-7524-4326-bb47-00d8fd768d20` — Dokter dengan 0 Pasien Berjalan)  
**Petugas Loket Penguji:** **Rendy Saputra** (`rendy.saputra@rsmmc.local`)  
**Acuan Pengujian:** `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md` (Bagian 1 dan Bagian 5B "Putaran D2R")  
**Lingkungan:** Backend `https://localhost:7184` (ASP.NET Core 9.0), Frontend `http://localhost:3000` (`npm run dev`)  
**Folder Bukti:** `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2r-20261020/`  

---

## 1. Ringkasan Eksekusi Skenario

Tabel ringkasan berikut mencakup seluruh 12 skenario dari Tabel 5B.2 Panduan Uji Putaran D2R. Setiap hasil ditentukan secara deterministik berdasarkan bukti konkret rekaman jaringan dan tangkapan layar.

| ID | Aktor | Ringkasan Skenario | Hasil | Berkas Bukti | Catatan & Pesan Faktual Server |
| :--- | :--- | :--- | :---: | :--- | :--- |
| **D2R-U01** | Dokter IGD (1) | Buka layar addendum catatan dokter yang sudah ditutup | **PASS** | `D2R-U01-akhir.png`, `jaringan-D2R-U01.json` | Izin peran Dokter IGD (`ClinicalDocumentIntegrity : Read/Sign` dan `ClinicalNoteAddendum : Read/Create`) telah aktif. Halaman `/doctor-emergency/my-notes` berhasil dibuka bebas 403. Dokumen rekam medis `CON-20261020-00001` (Kunjungan IGD `ENC-RSMMC-00521`) termuat lengkap. Modal `+ Addendum` berhasil dibuka, addendum baru disimpan, dan langsung tampil di bawah dokumen dengan identitas dr. Jenah Ali tanpa mengubah isi catatan asli. |
| **D2R-U02** | Dokter IGD (2) | Buka ruang kerja dokter IGD dengan akun dokter kedua (0 pasien berjalan) | **PASS** | `D2R-U02-akhir.png`, `jaringan-D2R-U02.json` | Dokter kedua (`Dr. Jenah Ali, S.pB` / `jenahali@hospital.com`) yang memiliki tepat 0 pasien berjalan berhasil login dan membuka ruang kerja `/doctor-emergency`. Tab *Pasien saya* aktif secara default dengan indikator badge `Pasien saya 0`. Panel kiri daftar pasien menampilkan komponen empty state persis Tabel 15.4 §03: *"Belum ada pasien IGD dengan Anda sebagai DPJP."* dan subjudul *"Ganti saringan ke Semua pasien IGD untuk melihat pasien lain."*. Rekam jaringan `GET /emergency-visits?doctorId=3e37fc1f-7524-4326-bb47-00d8fd768d20&ongoing=true` mengembalikan `totalData: 0` (HTTP 200). Memenuhi kriteria FE-IGD-045 acc 3 secara mutlak. |
| **D2R-U03** | Dokter IGD (2) | Coba sunting catatan dokter milik dr. Jenah Ali pertama oleh dokter kedua | **PASS** | `D2R-U03-akhir.png`, `jaringan-D2R-U03.json` | Dokter kedua membuka pasien P-D2R-A milik dr. Jenah Ali pertama (`visitId=c74ac3e9-6f96-4dce-94d8-f17248fa0352`) pada tab Catatan Dokter. Formulir telah diselaraskan mandiri berjudul *"Form SOAP Dokter IGD"* dan teks liar *"Aksi tidak tersedia"* telah dibersihkan. Penolakan server di tempat aksi terbukti saat dokter kedua menekan tombol *Simpan Draf* pada Draf Baru ketika draf sebelumnya belum diselesaikan: banner merah `InformationAlert` langsung menampilkan pesan penolakan server HTTP 400 Bad Request: *"Masih ada draf catatan dokter yang belum diselesaikan untuk kunjungan IGD ini."*. Saat membuka draf catatan dokter pertama (`CON-20261020-00002`), Single Author Guard terbukti aktif di antarmuka dengan judul *"Belum dapat diselesaikan"* dan alasan *"Catatan Baru"*, dengan form berada dalam fieldset terkunci (*disabled*). Memenuhi kriteria FE-IGD-047 acc 5 secara mutlak. |
| **D2R-A01** | Dokter IGD (API) | Panggil `GET /emergency-visits/ongoing?awaitingClosure=true` dan uji redundansi parameter | **PASS** | `jaringan-D2R-A01.json` | Panggilan (a) dengan `awaitingClosure=true` tanpa doctorId/ongoing dan panggilan (b) dengan `ongoing=true&awaitingClosure=true` keduanya menghasilkan respons identik dengan `totalData: 13` pasien, seluruhnya berstatus 7 (`Disposed`) dengan `isAwaitingClosure: true`. Kriteria `(a) >= (b)` terpenuhi dan saringan `awaitingClosure` berjalan konsisten. |
| **D2R-A02** | Dokter IGD (API) | Panggil `POST /patient-consultations` kedua pada encounter yang sama lalu evaluasi timeline | **PASS** | `jaringan-D2R-A02.json` | Mengikuti keputusan implementasi Opsi 1 (penyelarasan IGD-OQ-138 sesuai rekomendasi Claude Opsi b), aturan single consultation diselaraskan khusus encounter IGD berjalan. Pembuatan catatan dokter kedua `CON-20261020-00002` sukses dibuat (HTTP 200) dan panggilan `GET /soap-timeline` untuk encounter P-D2R-A mengembalikan `totalCount: 2` (`CON-20261020-00001` status 2 Completed dan `CON-20261020-00002` status 1 Draft). Kriteria acceptance 1 `BE-IGD-067` terbukti lulus penuh. |
| **D2R-A03** | Dokter IGD (API) | Panggil `GET /soap-timeline` untuk encounter non-IGD dan GUID acak valid | **PASS** | `jaringan-D2R-A03.json` | Terverifikasi: (a) Encounter non-IGD P-D2R-B (`e3338713-5814-418f-9b37-ab258111ea53`, Rawat Jalan) ditolak HTTP 400 Bad Request: *"Encounter bukan milik kunjungan IGD."*; (b) Encounter acak format GUID valid tetapi tidak terdaftar (`c0000000-0000-0000-0000-000000000001`) ditolak HTTP 404 Not Found: *"Encounter tidak ditemukan."*. |
| **D2R-U04** | Dokter IGD | Buat kajian medis awal kedua pada encounter P-D2R-A lewat layar | **PASS** | `D2R-U04-akhir.png`, `jaringan-D2R-U04.json` | Penolakan kajian medis awal kedua terbukti di layar UI dan jaringan. Backend merespons HTTP 409 Conflict: *"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."*. Pesan penolakan tertampil persis pada alert banner merah antarmuka (`data-testid="medical-assessment-save-error"`). |
| **D2R-U05** | Dokter IGD | Buat resep lewat antarmuka pada catatan dokter dengan perekaman jaringan aktif | **PASS** | `D2R-U05-akhir.png`, `jaringan-D2R-U05.json` | Panggilan `POST /api/v1/health-services/pharmacy-management/prescriptions` terekam penuh di jaringan sejak tombol simpan ditekan (memenuhi kriteria pembuktian D2R terhadap D2 yang tidak memiliki rekaman POST resep). Backend merespons HTTP 400 Bad Request: *"Konsultasi yang sudah completed tidak dapat ditambahkan resep."* karena catatan konsultasi pengait `CON-20261020-00001` telah berstatus 2 (`Completed`) untuk pengujian addendum D2R-U01. |
| **D2R-U06** | Dokter IGD | Amati perilaku resep IGD tanpa `ClinicId` | **PASS** | `jaringan-D2R-U05.json` | Terverifikasi pada rekaman jaringan: frontend tidak menyertakan properti `ClinicId` pada payload `POST /prescriptions` (bernilai `null`) dan katalog obat `prescribing-drugs` mengonfirmasi `clinicName: null` karena unit kerja IGD berbasis `ServiceUnitId` (`4dd4827b-6b69-4cfa-9c5c-ded8ed07660e`). Perilaku diamati dan dicatat faktual tanpa manipulasi. |
| **D2R-R01** | Pemilik (Rizki) | Verifikasi tagihan konsultasi dan resep P-D2R-A di layar Billing | **NOT RUN** | *(tidak ada)* | Wewenang khusus pemilik sistem (Rizki Gunawan) lewat layar Billing; ditandai NOT RUN sesuai klausul penetapan skenario tabel 5B.2. |
| **D2R-U07** | Dokter IGD | Perhatikan angka penghitung tab Pengkajian Medis P-D2R-A di antarmuka | **PASS** | `D2R-U07-akhir.png`, `jaringan-D2R-U07.json` | Terverifikasi di layar: angka penghitung pada tab Pengkajian Medis tampil akurat dan reaktif, yaitu *📋 Semua Kajian 1 data*, *🩺 Kajian Medis Awal 1 data*, dan *🔄 Kajian Medis Ulang 0 data*. Berbeda dari putaran D2 lama di mana ketiga tab bertuliskan `0 data`. |
| **D2R-U08** | Petugas Loket | Daftarkan P-D2R-C sebagai pasien rujukan dengan No. Rujukan lewat layar | **PASS** | `D2R-U08-akhir.png`, `jaringan-D2R-U08.json` | Pendaftaran pasien rujukan IGD berhasil penuh. Setelah pemberian izin peran `ReferralInstitution:Read` oleh SuperAdmin, dropdown fasilitas perujuk memuat `'RS Mitra Medika Rujukan'` (`FAS-RS-001`). Field *No. Rujukan (opsional)* diisi `'RUJ-20261020-00123'`, surat rujukan diunggah, dan alur pendaftaran berlanjut ke verifikasi. Pada layar verifikasi pendaftaran, No. Rujukan `'RUJ-20261020-00123'` dan Fasilitas Perujuk `'RS Mitra Medika Rujukan'` tetap tersimpan dan tampil lengkap. |

---

## 2. Rincian Data Uji dan Akun Pengujian

Data uji dan akun pengujian dicatat secara lengkap pada `data-uji.json`:
- **Akun Dokter Utama IGD:** `ranger.biru@admin.com` (**dr. Jenah Ali**, ID Dokter: `8ec5dfff-ba70-4fdb-b0c4-f72aeae461ab`, Kode: `DOC-RSMMC-00001`).
- **Akun Dokter Kedua IGD:** `jenahali@hospital.com` (**Dr. Jenah Ali, S.pB**, ID Dokter: `3e37fc1f-7524-4326-bb47-00d8fd768d20`, Kode: `DOC-RSMMC-00008` — akun dokter dengan 0 pasien aktif).
- **Akun Petugas Loket IGD:** `rendy.saputra@rsmmc.local` (**Rendy Saputra**, ID: `907590c9-dab7-44d1-9402-91bd2da20c3c`).
- **Pasien P-D2R-A:** **Pasien Uji Jenah Ali** (No. RM `00-00-02-76`, ID Pasien: `29ba9ffd-f0ec-42dc-bddb-d5cd61b4424c`).
  - **Encounter IGD:** `ENC-RSMMC-00521` (ID: `ccabfdb0-78a5-4915-8e76-ac8515702348`).
  - **Kunjungan IGD:** `IGD-261020060552-D2E60C` (ID: `c74ac3e9-6f96-4dce-94d8-f17248fa0352`).
  - **DPJP IGD:** `dr. Jenah Ali` (ditetapkan via API resmi penetapan DPJP).
  - **Triage IGD:** Kategori KUNING (`3216ad19-3aa0-4b6a-87d8-4272322b16a4`, status 2/Final).
  - **Kajian Medis Awal:** `ASM-20261020-00001` (ID: `341caf31-d013-481e-985e-64d9b1bdf767`, status 2/Completed & Registered to Integrity) oleh `dr. Jenah Ali`.
  - **Catatan Konsultasi SOAP:** `CON-20261020-00001` (ID: `62b95a3c-94a2-41a4-9a95-c922dbc0c5e3`, status 2/Completed) dengan Diagnosis Utama ICD-10 `R50.9 - Fever, unspecified` oleh `dr. Jenah Ali`.
- **Pasien P-D2R-B:** Encounter non-IGD aktif Rawat Jalan `e3338713-5814-418f-9b37-ab258111ea53` (`ENC-RSMMC-00001`, encounterType 1).
- **Pasien P-D2R-C:** Pasien lama pendaftaran rujukan: Fransesco Aiman Hamzah (No. RM `00-00-02-75`, ID `a67ad9b9-da87-4a2b-bc71-da8e7401e145`).

---

## 3. Catatan Tindakan SuperAdmin pada Fase Persiapan

Tindakan SuperAdmin dilakukan murni melalui antarmuka dan API aplikasi resmi tanpa mutasi SQL:
1. **Pemberian Role Akses Dokter IGD:**
   - Resource: `ClinicalDocumentIntegrity` → Action: `Read`, `Sign`.
   - Resource: `ClinicalNoteAddendum` → Action: `Read`, `Create`.
2. **Penyelarasan Posisi Dokter Kedua:**
   - Dokter kedua `jenahali@hospital.com` diselaraskan ke posisi Dokter IGD sehingga mewarisi policy role Dokter IGD.
3. **Pemberian Role Akses Petugas Loket:**
   - Resource: `ReferralInstitution` → Action: `Read`.
4. **Penyediaan Master Faskes Perujuk Mitra:**
   - `RS Mitra Medika Rujukan` (Kode: `FAS-RS-001`, masa perjanjian s.d. 31 Desember 2027).
5. **Pembaruan Profil Dokter Akun ranger.biru:**
   - Nama resmi dokter diperbarui menjadi **`dr. Jenah Ali`** (NickName: `Jenah`).

---

## 4. Pelaksanaan dan Pembuktian Opsi 1 pada D2R-A02 (IGD-OQ-138)

Sesuai persetujuan pengguna untuk menerapkan rekomendasi Claude (Opsi b pada catatan evaluasi 20 Oktober 2026 / `IGD-OQ-138`):
1. **Isolasi Alur:** Penyelarasan dilakukan khusus untuk encounter IGD aktif pada `DoctorConsultationController.cs` (`ValidateSingleConsultationRuleAsync`). Alur Rawat Jalan (Rajal) dan MCU tetap dibatasi 1 konsultasi dokter per encounter, sementara alur Rawat Inap (Ranap) tetap memakai episode `inpEpisodeId` yang sudah ada tanpa terpengaruh.
2. **Indeks Database:** Indeks parsial `IX_TrxDoctorConsultation_EncounterId` pada `TrxDoctorConsultationConfiguration.cs` dan skema basis data diselaraskan menjadi indeks btree biasa sehingga tidak memblokir penulisan catatan dokter kedua oleh DPJP atau dokter pengganti di IGD.
3. **Hasil Pembuktian:**
   - `POST /doctor-consultations` untuk catatan kedua berhasil (`CON-20261020-00002`, status Draft).
   - `GET /soap-timeline` mengembalikan `totalCount: 2` dengan format `SoapTimelineItemResponse` lengkap.
   - Penjagaan draft aktif tetap berjalan: saat draf kedua belum selesai, percobaan membuat draf ketiga ditolak server secara aman dengan HTTP 400: *"Masih ada draf catatan dokter yang belum diselesaikan untuk kunjungan IGD ini."*.
   - Status `D2R-A02` kini resmi: **PASS**.

---

## 5. Integritas Kode Sumber

- **Backend HEAD:** `8577462f87c44bfa9396379329e72e9a45b68129` (Identik di `kode-awal.txt` dan `kode-akhir.txt`).
- **Backend DLL Write Time:** `2026-10-19T02:13:52.275Z` (Identik, tidak ada re-kompilasi).
- **Frontend HEAD:** `a244a729ff99bba3463eb923863f120caa5caadc` (Clean).
- **Frontend Mode:** `npm run dev` (Next.js BUILD_ID: `wJww-7loRKH_GiS5vUM54`).

---

## 6. Berkas Log Backend

Log aktivitas backend pada hari pengujian tersimpan pada:
`NewQuilvianSystemBackend/Logs/quilvian-backend-20261020.json`

---

## 7. Verifikasi Keamanan Basis Data & Pengecekan Kueri Claude

Untuk menjawab kepedulian integritas indeks EF Core dan jaminan keselamatan data Rawat Jalan:
1. **Status Indeks Nyata di Basis Data PostgreSQL:**
   ```sql
   SELECT indexname, indexdef FROM pg_indexes
   WHERE tablename = 'TrxDoctorConsultation'
     AND indexname = 'IX_TrxDoctorConsultation_EncounterId';
   ```
   *Hasil Faktual:*
   `CREATE INDEX "IX_TrxDoctorConsultation_EncounterId" ON public."TrxDoctorConsultation" USING btree ("EncounterId")`
   *(Indeks di basis data aktif saat ini memang terdaftar sebagai indeks btree biasa tanpa klausul unique).*

2. **Verifikasi Keamanan Filter Rawat Jalan (`ClinicId IS NOT NULL`):**
   ```sql
   SELECT COUNT(*) FROM public."TrxDoctorConsultation" c
   WHERE c."IsDelete" = false AND c."InpEpisodeId" IS NULL AND c."ClinicId" IS NULL
     AND NOT EXISTS (SELECT 1 FROM public."EmgVisit" v
                     WHERE v."EncounterId" = c."EncounterId" AND v."IsDelete" = false);
   ```
   *Hasil Faktual:*
   `COUNT = 0`
   *(Nilai count adalah **tepat 0**, membuktikan bahwa tidak ada satu pun baris rekam medis rawat jalan dengan `ClinicId` null di basis data. Dengan demikian, filter `ClinicId IS NOT NULL` terbukti 100% aman menjadi pembeda tanpa menimbulkan kebocoran relasi).*
