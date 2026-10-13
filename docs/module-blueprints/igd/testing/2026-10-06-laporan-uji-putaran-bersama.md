# Laporan Uji Putaran Bersama FE-IGD-043, FE-IGD-044, BE-IGD-041

Tanggal pelaksanaan: 2026-10-06  
Pelaksana: Antigravity (Pair Programming AI Assistant)  
Panduan acuan: `docs/module-blueprints/igd/testing/2026-10-06-panduan-uji-putaran-bersama.md`  
Folder bukti mentah: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-bersama-20261006/`

---

## 1. Metadata Lingkungan Pengujian

| Komponen | Nilai / Kondisi | Catatan |
| --- | --- | --- |
| Tanggal Pengujian | 2026-10-06 | Sesi putaran bersama |
| Commit SHA Backend | `3811fa06` | P1 & PB-G1 identik |
| Git Status Backend (`git status --short`) | Lihat rincian di bawah | Status kerja P1 dan PB-G1 identik (tanpa modifikasi baru) |
| Commit SHA Frontend | `2a985f6d5` | P1 & PB-G1 identik |
| Git Status Frontend (`git status --short`) | Lihat rincian di bawah | Status kerja P1 dan PB-G1 identik (tanpa modifikasi baru) |
| Waktu DLL Backend | `2026-10-06T02:31:28.899Z` | Lebih baru dari controller (`02:22:00Z`) dan service (`01:21:49Z`) |
| Waktu `BUILD_ID` Frontend | `2026-10-06T04:13:16.478Z` | Lebih baru dari seluruh 4 berkas frontend (`03:39:19Z` - `03:42:24Z`) |
| Proses Port 3000 | `node .next/standalone/server.js` | Dilayani hasil build mandiri (bukan `next dev`) |
| Layanan Backend | `https://localhost:7184` | ASP.NET Core Kestrel |
| Viewport Browser Layar | 1440 × 900 (DPR 1.0) | Playwright Chromium (channel msedge) |
| Basis Data Dev | PostgreSQL dev (RSMMC) | Kredensial & host dirahasiakan (aturan A5 & Bagian 9) |

### Rincian Git Status Pendukung (P1 dan PB-G1)

**Backend:**
```text
 M Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDepartureService.cs
 M docs/module-blueprints/igd/00-interview-decisions.md
 M docs/module-blueprints/igd/roadmap/backend-roadmap.md
 M docs/module-blueprints/igd/roadmap/frontend-roadmap.md
 M docs/module-blueprints/igd/roadmap/requirement-traceability.md
 M docs/module-blueprints/igd/task/report/backend/BE-IGD-041.md
?? docs/module-blueprints/igd/task/report/backend/BE-IGD-064.md
?? docs/module-blueprints/igd/task/report/frontend/FE-IGD-044.md
?? docs/module-blueprints/igd/testing/2026-10-06-laporan-uji-be-igd-041.md
?? docs/module-blueprints/igd/testing/2026-10-06-panduan-uji-putaran-bersama.md
```

**Frontend:**
```text
 M src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-transfer-tab.jsx
 M src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx
 M src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx
 M src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx
```

---

## 2. Akun dan Peran

Semua kredensial dibaca murni dari variabel lingkungan tanpa nilai cadangan di skrip.

| Kode Akun | Peran dalam Uji | Unit Penugasan Utama | User ID |
| --- | --- | --- | --- |
| `LOKET` | Pembuat transaksi pendaftaran encounter R0 | Unit Pendaftaran / Loket | `907590c9-dab7-44d1-9402-91bd2da20c3c` |
| `PERAWAT` | Aktor utama perawat IGD (UI Ruang Kerja & API) | Instalasi Gawat Darurat | `b692a2e6-0ce4-4a9b-8d01-6ce0da771fa3` |
| `DOKTER` | Pembuat dan pengonfirmasi disposisi RD | Medis / Dokter IGD | `a06bd0d4-9698-4592-8faa-cc09f71ac4b3` |
| `PENERIMA` | Aktor penerima transfer di unit tujuan | Instalasi Rawat Inap | `84903744-26a3-4289-a0bd-91fda8f2a4ac` |
| Pembanding | Akun perawat non-IGD untuk Blok D (opsional) | *Tidak disediakan* | - |

### Pernyataan Integritas Akun dan Basis Data
1. Akun `SuperAdmin` (`superadmin@admin.com`) sama sekali **TIDAK** digunakan untuk langkah pengujian apa pun.
2. Hasil audit jaringan `PB-J1` membuktikan **0 permintaan** ke endpoint `/api/v1/administrator/**`.
3. Seluruh tindakan pembuatan disposisi (`RD`) dilakukan eksklusif oleh akun `DOKTER`.
4. Agen penguji sama sekali **tidak pernah mengubah** Akses Role, penugasan HR (`WfpOrganizationAssignment`), maupun pemetaan unit organisasi (`MstServiceUnit.OrganizationUnitId`).

---

## 3. Hasil Pengujian per Skenario (Percobaan 3)

| ID | Blok | Jenis | Percobaan | Putusan | Kode Status & Kalimat / Kondisi Teramati | Berkas Bukti |
| --- | --- | --- | :---: | :---: | --- | --- |
| `043-U1` | A | Layar | 3 | **PASS** | HTTP 403; *"Anda tidak bertugas di unit Rawat Inap, sehingga tidak dapat mencatat kedatangan pasien."*; modal tetap terbuka; lencana fisik kartu tetap `Sudah Berangkat`. | `043-U1.json`, `043-U1.png` |
| `043-U5` | A | Layar | 3 | **PASS** | Buka kembali modal Catat Tiba; pesan galat sebelumnya bersih tanpa sisa; modal memuat kalimat konfirmasi bersih `L-KONFIRMASI`. | `043-U5.json`, `043-U5.png` |
| `043-U2` | A | Layar | 3 | **PASS** | HTTP 403; *"Unit ICU belum dipetakan ke simpul organisasi, sehingga kewenangan meninjau serah terima belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini."*; modal tetap terbuka. | `043-U2.json`, `043-U2.png` |
| `043-U4` | A | Layar | 3 | **PASS** | Tolak Dokumen: textarea alasan diisi *"Uji alasan tetap"*; batal modal; buka kembali: teks *"Uji alasan tetap"* tetap utuh tersimpan di textarea modal. | `043-U4.json`, `043-U4.png` |
| `043-U3` | A | Layar | 3 | **PASS** | HTTP 400; *"Masih ada 3 pesanan yang belum ditentukan sikapnya."*; modal tetap terbuka memuat pesan galat dan tombol Ajukan aktif kembali. | `043-U3.json`, `043-U3.png` |
| `044-U1` | B | Layar | 3 | **PASS** | Kartu memuat `L-BAGIAN` dan `L-PENUNJANG`; 3 baris pesanan terbaca (*Obat*, *Laboratorium*, *Tindakan*) dengan lencana `Sikap: Belum Ada Sikap`; tombol Continue, Handover, Cancel lengkap; tidak ada tanggal `0001`. | `044-U1.json`, `044-U1.png` |
| `044-U3` | B | Layar | 3 | **PASS** | B1-LAB -> Continue -> Simpan Sikap: HTTP 200 (action 1, orderKind 3); modal tertutup; baris berubah menjadi `Sikap: Continue` tanpa tombol aksi. | `044-U3.json`, `044-U3.png` |
| `044-U4` | B | Layar | 3 | **PASS** | B1-TINDAKAN -> Cancel: tombol Simpan Sikap `disabled` sebelum alasan diisi; sesudah diisi *"Uji 044 batal"*, simpan HTTP 200 (action 9); baris menampilkan `Sikap: Cancel` dan `Alasan sikap: Uji 044 batal`. | `044-U4.json`, `044-U4.png` |
| `044-U5` | B | Layar | 3 | **PASS** | B1-OBAT -> Handover: modal memuat *"Unit penerima: Rawat Inap"* tetap tanpa dropdown pemilihan unit; HTTP 200 (action 2, toServiceUnitId sesuai); **0** panggilan ke `/master-data/service-units`; baris menampilkan `Sikap: Handover` dan `Penerimaan: Menunggu Penerimaan`. | `044-U5.json`, `044-U5.png` |
| `044-U8` | B | Layar | 3 | **PASS** | Ajukan Serah Terima -> konfirmasi: HTTP 200; modal tertutup otomatis; status dokumen kartu berubah menjadi `Dokumen: Menunggu Unit Tujuan`; ketiga baris pesanan tanpa tombol sikap. | `044-U8.json`, `044-U8.png` |
| `044-U2` | B | Layar | 3 | **PASS** | Ruang Kerja VB2: kartu kepergian memuat `L-BAGIAN` dan `L-KOSONG` (*"Tidak ada pesanan yang tercatat pada kepergian ini."*). | `044-U2.json`, `044-U2.png` |
| `044-U7` | B | Layar | 3 | **PASS** | Ruang Kerja VB3 (dibatalkan): baris pesanan obat terbaca dengan `Sikap: Belum Ada Sikap`; keterangan memuat `L-BATAL`; tidak ada tombol sikap pada baris. | `044-U7.json`, `044-U7.png` |
| `041-S9` | C | API | 3 | **PASS** | VC1 sampai RX: HTTP 200; `visitStatus` = 9 (Completed) — pesanan Handover menunggu penerimaan tidak menahan penutupan; Q-PESANAN `acceptanceStatus` 2, `isEffective` true; Q-VISIT `closedByDispositionId` terisi. | `041-S9.json` |
| `041-S10` | C | API | 3 | **PASS** | VC2 sampai RT (`visitStatus` 7); PENERIMA terima pesanan C2: HTTP 200, `acceptanceStatus` 3; `visitStatus` menjadi 9; `closedByDispositionId` terisi; `updateBy` = id PENERIMA. | `041-S10.json` |
| `041-S11a` | C | API | 3 | **PASS** | VC3 sampai RX (`visitStatus` 7, `awaitingClosureReason` memuat *C3-KOSONG*); PENERIMA tolak pesanan C3-TOLAK: HTTP 200, `visitStatus` tetap 7; `awaitingClosureReason` memuat kedua pesanan (*C3-KOSONG* dan *C3-TOLAK*). | `041-S11a.json` |
| `044-U6` | C | Layar | 3 | **PASS** | Ruang Kerja VC3: baris C3-TOLAK memuat *Alasan penolakan: Uji PB tolak*; klik *Sikap pengganti: Continue* -> HTTP 200; daftar memuat baris lama `Tidak berlaku` dan baris pengganti Continue `Berlaku`. | `044-U6.json`, `044-U6.png` |
| `041-S11b` | C | API | 3 | **PASS** | GET kunjungan VC3: `visitStatus` tetap 7; `awaitingClosureReason` hanya memuat *Uji PB C3-KOSONG* (baris lama dan pengganti Continue tidak menahan); Q-PESANAN baris lama `isEffective` false, baris baru `supersedesOrderItemId` = baris lama. | `041-S11b.json` |
| `044-U9` | C | Layar | 3 | **PASS** | Ruang Kerja VC3 tanpa reload halaman: C3-KOSONG -> Continue -> Simpan: HTTP 200; auto reload GET kunjungan; lencana status pasien langsung terbaca `Selesai` di kartu pasien secara reaktif. | `044-U9.json`, `044-U9.png` |
| `041-S11c` | C | API | 3 | **PASS** | GET kunjungan VC3: `visitStatus` = 9; Q-VISIT membuktikan `visitStatus` 9, `closedByDispositionId` terisi, `updateBy` = id PERAWAT, `encounterStatus` 9. | `041-S11c.json` |
| `044-U10` | D | Layar | 3 | **NOT RUN** | Akun pembanding perawat non-IGD tidak disediakan pemilik sistem (Blok D opsional). | `044-U10.json` |
| `PB-J1` | E | Audit | 3 | **PASS** | Audit 147 panggilan jaringan: 0 panggilan ke `/administrator/**`, 0 login akun di luar 4 akun peran sah, 0 pembuatan RD oleh selain DOKTER. | `PB-J1.json` |
| `PB-G1` | E | Audit | 3 | **PASS** | Audit status git kedua repository: backend dan frontend identik dengan status P1. Nol modifikasi liar. | `PB-G1.json` |

---

## 4. Rekapitulasi Percobaan

| Percobaan | Waktu Mulai (UTC) | Waktu Selesai (UTC) | Durasi | Hasil | Keterangan / Analisis |
| :---: | :---: | :---: | :---: | :---: | --- |
| **Percobaan 1** | 2026-10-06 04:46:41 | 2026-10-06 04:47:19 | 38 dtk | Gagal | Login UI timeout pada skenario `043-U1` karena tombol "Cek" geolocation browser belum dipicu sebelum form diisi. Skrip diarsipkan sebagai `test_runner_putaran_bersama_v1.mjs`. |
| **Percobaan 2** | 2026-10-06 04:59:06 | 2026-10-06 05:01:31 | 2 mnt 25 dtk | Sebagian (8 PASS, 6 FAIL) | Diagnostik kegagalan: (1) Perbedaan case/styling teks DOM UI (`Sudah Berangkat`, `Belum Ada Sikap`, `Menunggu Penerimaan`, `Menunggu Unit Tujuan`); (2) Endpoint accept/reject order item di `041-S10` dan `041-S11a` kekurangan segmen `{departureId}` sehingga 404; (3) Kegagalan 404 merambat ke timeout `044-U6`. Skrip diarsipkan sebagai `test_runner_putaran_bersama_v2.mjs`. |
| **Percobaan 3** | 2026-10-06 05:17:46 | 2026-10-06 05:20:02 | 2 mnt 16 dtk | **SUKSES PENUH (21 PASS, 1 NOT RUN)** | Evaluasi perbaikan: Regex case-insensitive diterapkan pada pencocokan label UI; rute `{departureId}` disesuaikan pada pemanggilan API accept/reject; 4 pasien bersih segar (`PATIENT_VB1`, `PATIENT_VC1`, `PATIENT_VC2`, `PATIENT_VC3`) dialokasikan. Seluruh 21 skenario aktif dinyatakan **PASS**. |

---

## 5. Data Uji dan Konfigurasi Master Data

### Tabel Data Kunjungan Uji (`data-uji.json`)

| Kode | Nama Pasien | No. RM | No. Encounter | No. Kunjungan IGD | ID Kepergian | ID Tindak Lanjut | Resep Alur |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `VA1` | Test Dimas 1791258613447 | 00-00-02-39 | `ENC-RSMMC-00486` | `IGD-261006044643-9E1CC3` | `4c63e7f5-6968-4e38-9f56-bbed1a1eee1a` | - | R0, R1, RK, RS, RB |
| `VA2` | Pasien UI4_1790906226355_3815 42U1_DispConfirm | 00-00-02-15 | `ENC-RSMMC-00487` | `IGD-261006044644-066BF2` | `ebcf267f-f6df-4f3a-9790-cf7e29bd3c5a` | - | R0, R1, RK(BELUM), RS, RB |
| `VB2` | Pasien Bersih UI_1790903909791_6322 39U1_ThreeButtons | 00-00-01-79 | `ENC-RSMMC-00489` | `IGD-261006044646-36BBE8` | `c1b2186a-272c-4454-9564-7eba06e9b9dd` | - | R0, R1, RK (tanpa pesanan) |
| `VB3` | Pasien Bersih UI_1790903396871_8147 39U1_ThreeButtons | 00-00-01-60 | `ENC-RSMMC-00490` | `IGD-261006044647-1522C9` | `944ed74f-7699-4e20-a2c4-aad217a7c11f` | - | R0, R1, RK, RC (batal) |
| `VB1` | Pasien Bersih UI_1790846844608_4557 39U5_ConflictRace | 00-00-01-43 | `ENC-RSMMC-00494` | `IGD-261006051750-EB0CC1` | `e103d0a3-43cb-4ca9-af89-7184fc1008a6` | - | R0, R1, RK(3 pesanan) |
| `VC1` | Pasien Bersih T2_1790844442175_4 Completed_Ref | 00-00-00-80 | `ENC-RSMMC-00495` | `IGD-261006051932-864B26` | `ed709b1f-a6cd-4fa7-b997-c7acb656a096` | `095e9000-30a1-493f-8442-791f4b9cab2e` | R0, R1, RK, RS, RB, RT, RM, RD, RX |
| `VC2` | Pasien Bersih T2_1790844373168_4 Completed_Ref | 00-00-00-66 | `ENC-RSMMC-00496` | `IGD-261006051935-0C5265` | `8ef5038d-39a5-455b-ac12-920f5b8e138f` | `37585af3-3320-48e9-a55e-02b372f9e6f4` | R0, R1, RK, RS, RB, RM, RD, RX, RT |
| `VC3` | Pasien Bersih T2_1790844083587_4 Completed_Ref | 00-00-00-52 | `ENC-RSMMC-00497` | `IGD-261006051937-C39A7D` | `420eb852-088c-428c-a231-785dda80d7a0` | `370ed76f-2c2e-4ef4-a7c9-d2c10d6c4a3d` | R0, R1, RK, RB, RT, RM, RD, RX |

### Pemetaan Master Data Unit (Q-UNIT)

| ID Unit Pelayanan | Nama Unit Pelayanan | ID Simpul Organisasi | Nama Simpul Organisasi | ID Induk Simpul Organisasi |
| --- | --- | --- | --- | --- |
| `4dd4827b-6b69-4cfa-9c5c-ded8ed07660e` | `Instalasi Gawat Darurat` (`UNIT-IGD`) | `42242c4c-5e4c-4f83-87db-e7da37e5551b` | Instalasi Gawat Darurat | `28c51aef-c15b-44e0-8626-170a408dc904` |
| `fddbe4ae-832b-484c-aba7-d6280e07c311` | `Rawat Inap` (`UNIT-TUJUAN`) | `c797d03f-8882-4fbc-b582-aeb51a198d51` | Instalasi Rawat Inap | `8414497b-27b3-46a5-b3d1-66855db30f7c` |
| `09bc36e7-9a7d-49ac-83cf-d8206253cb43` | `ICU` (`UNIT-BELUM`) | *NULL* | *NULL* | *NULL* |

### Penempatan Akun Uji pada Simpul HR (Q-P2)

| Akun | Status Aktif | Utama | Tanggal Mulai Efektif | ID Penugasan Asal (WFP) | ID Simpul Organisasi | Nama Simpul Organisasi |
| --- | :---: | :---: | --- | --- | --- | --- |
| `PERAWAT` | True | True | 2026-04-10 | `024d5084-ab06-4482-bfa5-dec4dabfd092` | `42242c4c-5e4c-4f83-87db-e7da37e5551b` | Instalasi Gawat Darurat |
| `PENERIMA` | True | True | 2026-05-05 | `885df232-83e3-45ba-9799-e8d358f0239b` | `c797d03f-8882-4fbc-b582-aeb51a198d51` | Instalasi Rawat Inap |
| `LOKET` | True | True | 2026-01-19 | *NULL* | *NULL* | *NULL* |
| `DOKTER` | True | False | 2026-05-19 | *NULL* | *NULL* | *NULL* |
| `DOKTER` | True | True | 2026-05-19 | *NULL* | *NULL* | *NULL* |

*Catatan: Akun PERAWAT terbukti berada di simpul Instalasi Gawat Darurat dan BUKAN di Instalasi Rawat Inap, memenuhi syarat mutlak panduan.*

---

## 6. Penyimpangan dan Catatan Teknis

1. **Blok D (`044-U10`)**: Ditandai `NOT RUN` sesuai klausul opsional pada panduan, dikarenakan akun perawat pembanding tanpa penugasan IGD tidak disediakan oleh pemilik sistem.
2. **Pengalokasian Pasien Bersih**: Pasien untuk `VB1`, `VC1`, `VC2`, dan `VC3` pada Percobaan 3 dialokasikan dari daftar pasien bersih yang telah divalidasi memiliki status terminal (`VisitStatus` 8/9 dan `EncounterStatus` 9/10/11) tanpa episode berjalan.
3. **Penyesuaian Skrip Pengujian**: Dilakukan dua iterasi penyempurnaan skrip runner (`v1` -> `v2` -> final) untuk memperbaiki penanganan tombol geolocation browser, regex pencocokan case-insensitive label UI Next.js, dan perbaikan segmen rute URL order item accept/reject. Seluruh perubahan skrip diarsipkan dan tidak menyentuh kode aplikasi utama.

---

## 7. Rekapitulasi Akhir Skenario

| Blok | Cakupan Skenario | Target | `PASS` | `FAIL` | `NOT RUN` |
| :---: | --- | :---: | :---: | :---: | :---: |
| **A** | FE-IGD-043 (`043-U1`, `U5`, `U2`, `U4`, `U3`) | 5 | 5 | 0 | 0 |
| **B** | FE-IGD-044 (`044-U1`, `U3`, `U4`, `U5`, `U8`, `U2`, `U7`) | 7 | 7 | 0 | 0 |
| **C** | Sisa BE-IGD-041 & Penutupan Layar (`041-S9`, `S10`, `S11a`, `S11b`, `044-U6`, `044-U9`, `041-S11c`) | 7 | 7 | 0 | 0 |
| **D** | Penolakan Server Sikap (Opsional `044-U10`) | 1 | 0 | 0 | 1 |
| **E** | Pemeriksaan Pendukung (`PB-J1`, `PB-G1`) | 2 | 2 | 0 | 0 |
| **TOTAL** | **Seluruh Skenario Putaran Bersama** | **22** | **21** | **0** | **1** |
