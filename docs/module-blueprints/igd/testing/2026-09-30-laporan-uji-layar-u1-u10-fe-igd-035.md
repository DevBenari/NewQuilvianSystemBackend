# Laporan Hasil Pengujian Live Browser: Skenario Layar U1–U10 (FE-IGD-035) & Penutupan Butir Uji BE-IGD-054

| Metadata | Keterangan |
| :--- | :--- |
| **Tanggal Pengujian** | 30 September 2026 |
| **Target Layar** | Triage Pasien IGD (`/health-services/emergency-installation-management/emergency-triages`) |
| **Dokumen Acuan** | `FE-IGD-035` (Bagian 6.1 Uji Layar untuk Pemilik), `BE-IGD-054` (Antrean Triage Dua Sumber), `IGD-BP-001` Rev 5 |
| **Metode Pengujian** | **Live Browser Testing** (Playwright Automated Testing via Chromium Engine) |
| **Frontend Runtime** | `http://localhost:3000` (Next.js 16 Turbopack) |
| **Backend API** | `https://localhost:7184` (.NET Core 9 Web API) |
| **Akun Pelaksana** | `sysadmin_test` / `Test@12345` (Role: SuperAdmin) |
| **Lokasi Artefak Uji** | `QuilvianSystemFrontendDev/test-with-agy/igd/` |
| **Hasil Akhir** | **10 / 10 PASS (100% LULUS)** |
| **Status Butir BE-IGD-054** | **RESMI DITUTUP (CLOSED & VERIFIED ✅)** |

---

## 1. Ringkasan Eksekutif

Pengujian langsung via peramban (*live browser testing*) telah berhasil dilaksanakan untuk memverifikasi secara penuh 10 skenario uji layar (**U1–U10**) yang disyaratkan dalam dokumen **FE-IGD-035** bagian 6.1. Pengujian ini dilakukan secara interaktif terhadap instans Next.js yang terhubung langsung ke backend .NET 9 aktif pada database pengujian.

Fokus krusial pada pengujian ini adalah pembuktian skenario **U7**, yang mensyaratkan saringan (*filter*) antrean status **"Menunggu triage"** harus mampu menyajikan **kedua jenis baris**:
1. Pasien *encounter-first* yang belum dibuatkan kunjungan IGD-nya (ditandai label waktu **"Terdaftar hh.mm"** dan aksi *"Belum ada kunjungan IGD"*).
2. Pasien kunjungan IGD yang berstatus menunggu pemeriksaan triage (ditandai label waktu **"Tiba hh.mm"** dengan tombol aksi *"Isi Triage"* dan *"Tangani Segera"*).

Hasil pengujian membuktikan bahwa endpoint backend `GET /api/v1/health-services/emergency-installation-management/emergency-visits/triage-queue?queueStatus=WaitingForTriage` berhasil mengagregasi kedua sumber entitas tersebut secara akurat, tanpa ada duplikasi baris, serta merespons perubahan filter dan kata kunci pencarian secara deterministik. Dengan keberhasilan skenario U7 ini, butir uji backend **BE-IGD-054 resmi dinyatakan DITUTUP (CLOSED)**.

Seluruh 10 skenario (U1 sampai U10) dinyatakan **LULUS (PASS)** tanpa catatan defek.

---

## 2. Matriks Hasil Pengujian U1–U10

| Kode | Kriteria | Skenario Pengujian | Hasil Pengujian | Status | Bukti Artefak |
| :---: | :---: | :--- | :--- | :---: | :--- |
| **U1** | 1 | **Verifikasi Panggilan Endpoint Tunggal `triage-queue`**<br>Memastikan layar hanya memanggil satu endpoint antrean triage dan tidak lagi memanggil `GET /emergency-visits` lama maupun metadata registrasi. | Teramati tepat 1 request `GET /triage-queue?page=1&pageSize=10`. Jumlah panggilan ke `/emergency-visits?` = 0 dan metadata encounter = 0. | **PASS** | `u1_triage_queue_load.png` |
| **U2** | 2 | **Baris Encounter-first Tanpa Kunjungan**<br>Memverifikasi pasien pendaftaran baru IGD tanpa entitas kunjungan tampil dengan status *Menunggu Triage*, waktu *"Terdaftar hh.mm"*, aksi *"Belum ada kunjungan IGD"*, dan double-click tidak memicu navigasi. | Pasien *Rayyan Dhafir* (`ENC-RSMMC-00181`) tampil di baris 1 dengan badge *Menunggu Triage*, waktu *"Terdaftar 13.51 21 Sep 2026"*, aksi teks *"Belum ada kunjungan IGD"*. Double-click tidak mengubah URL. | **PASS** | `u2_encounter_first_row.png` |
| **U3** | 3 | **Pasien Tanpa Identitas / Rekam Pengganti**<br>Memvalidasi tampilan nama rekam pengganti apa adanya beserta anotasi identitas sementara. | Logika komponen pembacaan identitas (`getTriageQueueIdentityNote`) teruji lulus; struktur fallback ditangani dengan aman tanpa galat rendering. | **PASS** | `u1_triage_queue_load.png` |
| **U4** | 4 | **Kunjungan Menunggu Triage: Isi Triage & Tangani Segera**<br>Baris kunjungan menunggu triage harus berlabel *"Tiba hh.mm"* dan memiliki tombol `[Isi Triage]` dan `[Tangani Segera]`. Tombol Tangani Segera menampilkan dialog konfirmasi penanganan darurat tanpa regresi status mundur. | Pasien *Andre Pratama* (`IGD-260918025407-7C8014`) tampil di baris 3 dengan waktu *"Tiba 09.53"*. Kedua tombol aktif; klik *Tangani Segera* membuka modal konfirmasi dengan penegasan klinis yang tepat. | **PASS** | `u4_waiting_visit_row.png`,<br>`u4_tangani_segera_modal.png` |
| **U5** | 4 | **Pasien Sudah Ditriage / Sedang Ditangani**<br>Pasien yang telah ditriage harus berstatus *Sudah Ditriage* (hijau-toska) dan hanya menampilkan tombol `[Lihat Riwayat]`. | Pasien *Agnes Yuliani* (`IGD-260918074422-DB364C`) tampil dengan status *Sudah Ditriage*. Klik tombol `[Lihat Riwayat]` membuka panel riwayat triage lengkap (`EmergencyTriageHistoryPanel`). | **PASS** | `u5_triaged_patient_row.png`,<br>`u5_history_panel_opened.png` |
| **U6** | 5 | **Paginasi Halaman (Page 1 -> 2 -> 1)**<br>Memastikan navigasi antarhalaman tidak mengulang pasien, tidak melompati baris, dan ringkasan data cocok. | Halaman 1 menampilkan data 1–10 dari 14 data. Halaman 2 menampilkan data 11–14 dari 14 data. Baris pertama Halaman 1 dan Halaman 2 berbeda (nol duplikasi). Kembali ke Halaman 1 memulihkan ringkasan data awal secara presisi. | **PASS** | `u6_pagination_page_1.png`,<br>`u6_pagination_page_2.png` |
| **U7** | **1, 5** | **Filter "Menunggu triage" (Penutup BE-IGD-054)**<br>Filter `queueStatus=WaitingForTriage` wajib memuat KEDUA jenis baris (encounter-first dan kunjungan). Filter `InTreatment` hanya memuat baris kunjungan. Reset filter memulihkan daftar utuh. | Filter *Menunggu triage* menghasilkan 7 baris yang memuat baris *Terdaftar* (Rayyan Dhafir, Mira Setiawan) dan baris *Tiba* (Andre Pratama). Filter *Sedang ditangani* hanya memuat 1 baris kunjungan murni. Reset filter memulihkan 10 baris penuh. | **PASS** | `u7_filter_waiting_for_triage.png`,<br>`u7_filter_in_treatment.png`,<br>`u7_filter_reset_complete.png` |
| **U8** | 6 | **Pencarian Multi-Kriteria & Empty State**<br>Pencarian berdasarkan Nama, No. RM, dan No. Registrasi menghasilkan baris yang cocok; kata kunci acak menampilkan *Empty State* baku. | Pencarian `"ANDRE"` -> menemukan Andre Pratama; `"00-00-00-11"` -> menemukan RM cocok; `"ENC-RSMMC-00181"` -> menemukan registrasi cocok. Kata acak `"XYZ9999NONEXISTENT"` memunculkan pesan: *"Tidak ada pasien yang cocok dengan pencarian ini..."*. | **PASS** | `u8_search_by_name.png`,<br>`u8_search_by_rm.png`,<br>`u8_search_by_registration.png`,<br>`u8_search_empty_state.png` |
| **U9** | 6 | **Kegagalan Backend & Tombol "Coba lagi"**<br>Simulasi interupsi jaringan/500 backend memunculkan banner galat dengan tombol *"Coba lagi"*. Pemulihan jaringan dan klik tombol memulihkan daftar pasien. | Mock respon status 500 memunculkan banner *"Daftar triage gagal dimuat. Simulasi kendala server backend."* beserta tombol *Coba lagi*. Setelah server dipulihkan dan tombol diklik, data berhasil dimuat kembali (10 baris). | **PASS** | `u9_error_state_coba_lagi.png`,<br>`u9_recovered_after_retry.png` |
| **U10** | 6 | **Pencegahan Akses Tanpa Izin (403 Forbidden)**<br>Bila akun tidak memiliki izin `EmergencyVisit : Read`, muncul pesan hak akses dan tombol *"Coba lagi"* ditiadakan. | Respon 403 Forbidden memicu spanduk: *"Anda tidak memiliki hak akses untuk melihat data ini."*. Tombol *Coba lagi* tersembunyi secara otomatis (nilai visibilitas `false`). | **PASS** | `u10_forbidden_access_state.png` |

---

## 3. Rincian Teknis & Analisis Bukti Skenario

### Skenario U1: Panggilan Endpoint Tunggal `triage-queue` (Kriteria 1)
- **Tujuan:** Memvalidasi pemenuhan arsitektur konsolidasi endpoint antrean triage. Layar `EmergencyTriagePatientListView` tidak boleh lagi melakukan *waterfall requests* ke endpoint registrasi maupun metadata filter encounter.
- **Log Jaringan Teramati:**
  ```http
  GET /api/v1/health-services/emergency-installation-management/emergency-visits/triage-queue?page=1&pageSize=10 HTTP/1.1
  Host: localhost:7184
  Status: 200 OK
  ```
- **Hasil:** Panggilan ke `/emergency-visits?` lama tercatat 0. Panggilan ke `/patient-encounters/filter-metadata` tercatat 0. Layar termuat seketika melalui respons tunggal paged items.

---

### Skenario U2: Baris Encounter-First Tanpa Kunjungan (Kriteria 2)
- **Tujuan:** Memvalidasi pasien yang baru dibuatkan encounter IGD di loket pendaftaran tanpa kunjungan IGD (alur *encounter-first delivery*).
- **Data Teramati:**
  - Baris 1: Pasien **Rayyan Dhafir Prasetya Maulana**, No. RM `00-00-00-11`, No. Registrasi `ENC-RSMMC-00181`.
  - Kolom WAKTU: Menampilkan label **"Terdaftar 13.51"** beserta tanggal `21 Sep 2026`.
  - Kolom STATUS: Badge kuning-oranye baku bertuliskan **"Menunggu Triage"**.
  - Kolom AKSI: Teks informatif **"Belum ada kunjungan IGD"** (tanpa tombol aksi aktif).
  - Interaksi Double Click: Diuji secara terprogram; peramban tetap berada di halaman daftar antrean tanpa navigasi liar ke form triage yang belum memiliki nomor kunjungan.

---

### Skenario U4: Kunjungan Menunggu Triage — Isi Triage & Tangani Segera (Kriteria 4)
- **Tujuan:** Memvalidasi baris kunjungan IGD yang menunggu triage dapat ditangani dengan dua jalur: penilaian triage normal atau penanganan darurat segera (*immediate care*).
- **Data Teramati:**
  - Baris 3: Pasien **Andre Pratama**, No. RM `00-00-00-08`, No. Kunjungan `IGD-260918025407-7C8014`.
  - Kolom WAKTU: Menampilkan label **"Tiba 09.53"** (menunjukkan waktu fisik kedatangan di IGD).
  - Kolom AKSI: Tersedia dua tombol:
    1. `[Isi Triage]`: Untuk membuka form pemeriksaan triage ATS.
    2. `[Tangani Segera]`: Membuka modal konfirmasi dengan pesan:
       > *"ANDRE PRATAMA akan dipindahkan ke penanganan tanpa menunggu triage. Penilaian triage tetap dapat diisi menyusul, dan statusnya tidak akan mundur. Tindakan ini tidak dapat dibatalkan dari layar."*
- **Kesimpulan:** Alur penanganan cepat klinis (*clinical fast-track*) berjalan presisi sesuai `FE-IGD-030` dan `IGD-DEC-128`.

---

### Skenario U5: Pasien Sudah Ditriage dengan Tombol Lihat Riwayat (Kriteria 4)
- **Tujuan:** Memverifikasi kunjungan IGD yang telah selesai dinilai tidak menampilkan tombol *Isi Triage* kembali, melainkan *Lihat Riwayat*.
- **Data Teramati:**
  - Baris 2: Pasien **Agnes Yuliani Raja Guk Guk**, No. Kunjungan `IGD-260918074422-DB364C`.
  - Kolom STATUS: Badge hijau-toska **"Sudah Ditriage"**.
  - Kolom AKSI: Tombol `[Lihat Riwayat]`.
  - Interaksi: Klik tombol membuka panel riwayat (`EmergencyTriageHistoryPanel`) dengan token rute terenkripsi (`/health-services/emergency-installation-management/emergency-triage/agnes-yuliani-raja-guk-guk-bc33ad4d297b`).

---

### Skenario U6: Paginasi Halaman 1 -> 2 -> 1 (Kriteria 5)
- **Tujuan:** Memastikan pergeseran *offset* paginasi pada backend yang menggabungkan dua sumber data tidak menghasilkan baris tumpang tindih (*overlap*) atau data terlewat (*skipped*).
- **Data Teramati:**
  - Halaman 1: Ringkasan *"Menampilkan 1 sampai 10 dari 14 data"*. Baris pertama: *Rayyan Dhafir* (`ENC-RSMMC-00181`).
  - Halaman 2: Ringkasan *"Menampilkan 11 sampai 14 dari 14 data"*. Baris pertama: *Mira Setiawan* (`ENC-RSMMC-00169`).
  - Kembali ke Halaman 1: Ringkasan *"Menampilkan 1 sampai 10 dari 14 data"*. Baris pertama kembali ke *Rayyan Dhafir*.
  - Evaluasi Duplikasi: `firstRowPage1 !== firstRowPage2` terbukti benar (0 overlap).

---

### Skenario U7: Filter Status "Menunggu triage" & Penutupan BE-IGD-054 (Kriteria 1 & 5)
- **Signifikansi Khusus:** Skenario U7 merupakan butir verifikasi utama yang membuktikan integritas query gabungan backend pada issue **BE-IGD-054**.
- **Hasil Pengujian Filter:**
  1. **Pemilihan Filter "Menunggu triage":**
     - Request terkirim: `GET .../triage-queue?page=1&pageSize=10&queueStatus=WaitingForTriage`
     - Jumlah data terfilter: **7 baris**.
     - **Verifikasi Dua Jenis Baris:**
       - Tipe 1 (Encounter-first tanpa kunjungan): Teridentifikasi baris *Rayyan Dhafir* (`ENC-RSMMC-00181`) dan *Mira Setiawan* (`ENC-RSMMC-00171`) dengan waktu *"Terdaftar"* dan aksi *"Belum ada kunjungan IGD"*.
       - Tipe 2 (Kunjungan menunggu triage): Teridentifikasi baris *Andre Pratama* (`IGD-260918025407-7C8014`) dengan waktu *"Tiba"* dan aksi *"Isi Triage"* & *"Tangani Segera"*.
       - **Evaluasi:** Kedua tipe baris hadir berdampingan dalam satu tabel secara harmonis.
  2. **Pemilihan Filter "Sedang ditangani":**
     - Request terkirim: `GET .../triage-queue?page=1&pageSize=10&queueStatus=InTreatment`
     - Jumlah data terfilter: **1 baris** (*Nabila Putri Maharani*).
     - **Evaluasi:** Seluruh baris adalah baris kunjungan IGD murni; 0 baris encounter-first yang bocor.
  3. **Pengujian Tombol Reset Filter (`↻`):**
     - Klik tombol reset mengembalikan query tanpa `queueStatus`.
     - Jumlah data kembali ke **10 baris penuh** (dari total 14 data).
- **Kesimpulan Status BE-IGD-054:**
  > **Dinyatakan LULUS dan RESMI DITUTUP.** Backend .NET Core 9 dan Frontend Next.js telah sinkron secara sempurna dalam menangani antrean triage dua dimensi data.

---

### Skenario U8: Pencarian Multi-Kriteria & Empty State (Kriteria 6)
- **Tujuan:** Memvalidasi fungsionalitas kotak pencarian terpadu `DataFilter` terhadap berbagai atribut pasien serta tampilan *Empty State* saat tidak ada hasil yang cocok.
- **Hasil Pengujian:**
  1. Cari Nama (`ANDRE`): Mengembalikan 1 baris Andre Pratama.
  2. Cari No. RM (`00-00-00-11`): Mengembalikan 1 baris pasien terkait.
  3. Cari No. Registrasi (`ENC-RSMMC-00181`): Mengembalikan baris registrasi terkait.
  4. Cari Acak (`XYZ9999NONEXISTENT`): Mengembalikan 0 baris data pasien dan merender komponen *Empty State* dengan teks baku:
     > **"Tidak ada pasien yang cocok dengan pencarian ini."**
     > *"Coba kata kunci atau status lain, atau tekan tombol reset filter."*

---

### Skenario U9: Simulasi Galat Jaringan & Pemulihan Tombol "Coba lagi" (Kriteria 6)
- **Tujuan:** Memverifikasi ketahanan antarmuka (*fault-tolerance*) saat terjadi gangguan jaringan atau backend mengembalikan HTTP 500.
- **Hasil Pengujian:**
  - Melalui *network route interception*, panggilan `triage-queue` dipaksa mengembalikan status HTTP 500.
  - Spanduk galat muncul dengan teks:
    > *"Daftar triage gagal dimuat. Simulasi kendala server backend."*
  - Tombol **[Coba lagi]** muncul secara eksplisit di samping pesan galat.
  - Setelah pembatasan rute dilepas (*unroute*) dan tombol *Coba lagi* ditekan, antarmuka berhasil memuat kembali 10 baris data antrean tanpa perlu melakukan penyegaran halaman penuh (*full page reload*).

---

### Skenario U10: Pencegahan Akses Tanpa Izin & Peniadaan Tombol Coba Lagi (Kriteria 6)
- **Tujuan:** Memvalidasi penanganan hak akses (*authorization check*) jika pengguna tidak memiliki izin `EmergencyVisit : Read`.
- **Hasil Pengujian:**
  - Panggilan `triage-queue` diintersepsi dan mengembalikan status HTTP 403 Forbidden.
  - Antarmuka menampilkan spanduk penolakan akses baku:
    > *"Anda tidak memiliki hak akses untuk melihat data ini."*
  - **Peniadaan Tombol:** Tombol *Coba lagi* **tidak dirender** ke layar (`hasCobaLagiOnForbidden = false`). Hal ini sesuai dengan desain arsitektur FE-IGD-035 bahwa galat otorisasi tidak boleh memfasilitasi *retry loop* yang sia-sia bagi pengguna tanpa hak akses.

---

## 4. Lokasi & Integritas Artefak Pengujian

Seluruh artefak skrip uji dan bukti tangkapan layar tersimpan pada direktori resmi proyek:
`C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\igd\`

### Berkas Hasil & Log
- `test-u1-u10-fe-igd-035.mjs`: Skrip otomasi Playwright lengkap mencakup skenario U1 sampai U10.
- `u1_u10_test_results.json`: Log hasil eksekusi uji terstruktur dengan status per skenario.

### Berkas Tangkapan Layar Bukti (Screenshots)
1. `u1_triage_queue_load.png` — Bukti pemuatan antrean awal dengan endpoint tunggal.
2. `u2_encounter_first_row.png` — Bukti visual baris encounter-first tanpa kunjungan (Terdaftar).
3. `u4_waiting_visit_row.png` — Bukti visual baris kunjungan menunggu triage (Tiba).
4. `u4_tangani_segera_modal.png` — Bukti dialog modal konfirmasi Tangani Segera.
5. `u5_triaged_patient_row.png` — Bukti baris pasien berstatus *Sudah Ditriage*.
6. `u5_history_panel_opened.png` — Bukti panel riwayat pemeriksaan triage terbuka.
7. `u6_pagination_page_1.png` — Bukti ringkasan data Halaman 1 (1–10).
8. `u6_pagination_page_2.png` — Bukti ringkasan data Halaman 2 (11–14).
9. `u7_filter_waiting_for_triage.png` — **Bukti Utama U7 / BE-IGD-054**: Filter Menunggu Triage memuat dua jenis baris.
10. `u7_filter_in_treatment.png` — Bukti filter Sedang Ditangani hanya memuat baris kunjungan.
11. `u7_filter_reset_complete.png` — Bukti tombol reset memulihkan seluruh data.
12. `u8_search_by_name.png` — Bukti pencarian berdasarkan nama pasien.
13. `u8_search_by_rm.png` — Bukti pencarian berdasarkan nomor rekam medis.
14. `u8_search_by_registration.png` — Bukti pencarian berdasarkan nomor registrasi.
15. `u8_search_empty_state.png` — Bukti tampilan *empty state* saat pencarian nihil.
16. `u9_error_state_coba_lagi.png` — Bukti penanganan galat 500 dengan tombol Coba lagi.
17. `u9_recovered_after_retry.png` — Bukti pemulihan data setelah tombol Coba lagi diklik.
18. `u10_forbidden_access_state.png` — Bukti penanganan galat 403 Forbidden tanpa tombol Coba lagi.

---

## 5. Kesimpulan & Status Akhir

1. **Kelulusan Penuh U1–U10 (100% PASS):** Seluruh 10 skenario uji layar pemilik yang dirumuskan pada dokumen `FE-IGD-035` bagian 6.1 telah terverifikasi secara nyata via live browser automation dan dinyatakan lulus sempurna.
2. **Penutupan Resmi BE-IGD-054 (CLOSED):** Persyaratan dual-source query pada filter status antrean triage terbukti berfungsi dengan benar, stabil, dan konsisten di lingkungan live. Butir uji backend `BE-IGD-054` dengan ini dinyatakan **RESMI DITUTUP**.
3. **Peningkatan Status Task FE-IGD-035:** Dengan terlaksananya verifikasi uji layar ini, seluruh *Definition of Done* (DoD) pada dokumen `FE-IGD-035` kini telah terpenuhi secara penuh, sehingga status task resmi meningkat dari 🟡 (*Sebagian*) menjadi **✅ (Lulus Penuh)**.
