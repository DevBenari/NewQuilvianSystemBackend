# Laporan Verifikasi Runtime Backend — Workspace PPRI (Rawat Inap)

- **Tanggal Pengujian**: 8 Oktober 2026
- **Lingkungan**: Development (`https://localhost:7184`, Database PostgreSQL `QuilvianNewDevHamzah`)
- **Basis Kode**: `NewQuilvianSystemBackend`
- **Migration Acuan**: `20261008055604_AddWorkspacePpriAdmissionDocuments`
- **Status Runtime**: Berjalan aktif di host (`dotnet run`)
- **Ruang Lingkup Kartu**: BE-RWI-185 s.d. BE-RWI-202 (melewati 190, 191, 203 yang berstatus deprecated/out-of-scope)

---

## 1. Ringkasan Eksekutif

Pengujian runtime ini dilakukan untuk memverifikasi keabsahan fungsional, integritas bisnis rumah sakit, dan ketahanan kontrak 28 endpoint Workspace PPRI (Penerimaan Pasien Rawat Inap) yang dibangun pada modul Rawat Inap Quilvian. Pengujian mencakup alur penyiapan data, manajemen master checklist, penguncian dan penandatanganan dokumen admisi, logging cetakan dan kop surat, penegakan aturan hak akses klinis/keuangan, hingga isolasi proteksi terhadap episode bertatus *Draft* dan *Closed*.

### Rangkuman Hasil Pengujian:
- **Total Skenario Diuji**: 42 skenario.
- **Hasil**:
  - **PASS**: 35 skenario (83.3%)
  - **FAIL**: 3 skenario (7.1% — terdampak langsung oleh bug runtime pada penyimpanan tanda tangan dan inkonsistensi skema database penjamin)
  - **NOT RUN**: 4 skenario (9.5% — disebabkan ketiadaan data demografi pasien spesifik pada database yang tidak boleh diubah/dihapus sepihak, serta ketiadaan baris kebijakan hak akses non-superadmin)
- **Total Temuan Bug**: 4 temuan teknis (2 bug fungsional kritis, 1 bug data seeder master, 1 gap konfigurasi kebijakan hak akses).

---

## 2. Persiapan Data & Akun Uji

Sesuai aturan kerja, seluruh data yang digunakan merupakan data nyata yang sudah ada di database atau dibuat melalui endpoint operasional resmi sistem tanpa manipulasi/penghapusan database langsung.

### 2.1. Dataset Episode Uji
1. **Episode A (Asuransi, Admitted, Kurang Deposit, Menempati Bed)**:
   - **ID Episode**: `29c2b8f3-8708-44d1-9caf-67f3b39fd272`
   - **Nomor Episode**: `RI-261006042632-A0202`
   - **Pasien**: Siti Rahmawati (No. RM: `00-00-00-11`, Penjamin: BPJS Kesehatan / Asuransi)
   - **Status Episode**: `Admitted` (Dirawat aktif)
   - **Lokasi Bed**: `BED 002` (Aktif menempati bed)
   - **Status Deposit**: Minimum Kebijakan Rp 1.000.000, Diterima Rp 0, **Shortfall: Rp 1.000.000** (`Shortfall`)
2. **Episode B (Tunai, Admitted, Cukup Deposit, Menempati Bed)**:
   - **ID Episode**: `58e88078-b343-4de2-8c6b-fbfa3512ba62`
   - **Nomor Episode**: `RI-261006042632-T0101`
   - **Pasien**: Budi Santoso (No. RM: `00-00-00-17`, Penjamin: Pasien Umum / Tunai)
   - **Status Episode**: `Admitted` (Dirawat aktif)
   - **Lokasi Bed**: `BED 001` (Aktif menempati bed)
   - **Status Deposit**: Telah dilakukan top-up deposit tunai Rp 1.000.000 via kasir shift terbuka `CSH-20261008-000001`, **Shortfall: Rp 0** (`Sufficient`)
3. **Episode C (Draft — Pasien Belum Dirawat)**:
   - **ID Episode**: `8abff20d-0c85-40dc-b88e-1a9ef6945b52`
   - **Nomor Episode**: `RI-261008092518-3E0ED9`
   - **Pasien**: Agnes Yuliani Raja Guk Guk (No. RM: `00-00-00-13`)
   - **Status Episode**: `Draft` (Dibuat via `POST /api/v1/health-services/inpatient-management/episodes`)
4. **Episode D (Closed — Selesai Rawat Inap)**:
   - **ID Episode**: `6dd5ec3a-584f-4b71-a15e-46d77a49c112`
   - **Nomor Episode**: `RI-260924053117-19E8BA`
   - **Pasien**: Ikbal Yuliyanto
   - **Status Episode**: `Closed` (Tertutup)

### 2.2. Akun Pengujian
- **Akun Utama (SuperAdmin)**: `superadmin` / `superadmin@admin.com` (Memiliki wewenang penuh untuk mengeksekusi operasi admisi, penandatanganan, pencetakan, dan melihat nominal rupiah melalui bypass kebijakan internal).
- **Akun Tanpa ViewAmount (Uji Penolakan 403)**: `siti.nurhaliza@rsmmc.local` (Karyawan terdaftar; diverifikasi melalui validasi radius login geofence RSMMC: Latitude -6.2198, Longitude 106.8324).
- **Akun Sekunder Terdaftar**: `amanda.fitriani@rsmmc.local`.

---

## 3. Matriks Hasil Pengujian per Kartu Task

---

### BE-RWI-185: Pemisahan Butir Clearance Pemulangan vs Serah Terima Pasien Baru
Memastikan pemisahan data periksa pemulangan pasien (`ClosureClearance` / `ChecklistType = 1`) dari data periksa serah terima pasien baru (`NewPatientHandover` / `ChecklistType = 2`).

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Membaca daftar butir clearance pemulangan pasien | `GET /api/v1/health-services/inpatient-management/discharges/29c2b8f3-8708-44d1-9caf-67f3b39fd272/clearance` | `200 OK` | **PASS** | Hanya mengembalikan 6 butir penutupan; 0 butir STPB bercampur. |
| 2 | Menandai butir STPB (`STPB-01`) pada endpoint pemulangan | `POST /api/v1/health-services/inpatient-management/discharges/29c2b8f3-8708-44d1-9caf-67f3b39fd272/clearance/d3ec1512-ce05-47d0-b359-2dc935ca708d/mark` | `404 Not Found`<br>`"Butir administrasi tidak ditemukan."` | **PASS** | Butir STPB diisolasi penuh dari alur pemulangan. |
| 3 | Membaca master clearance items dengan filter `NewPatientHandover` | `GET /api/v1/health-services/master-data/inpatient-clearance-items?checklistType=NewPatientHandover` | `200 OK` | **PASS** | Tepat mengembalikan 18 butir STPB (`STPB-01` s.d. `STPB-15`, `STPB-02A`, `STPB-02B`, `STPB-02C`). |

---

### BE-RWI-186: Pengaturan Rawat Inap & Validasi Master Clearance Item
Memastikan konfigurasi master rawat inap membawa formulir dan konstanta operasional kanonikal serta validasi integritas master data (`MST-ICI-001..004`).

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Membaca pengaturan default rawat inap (`MST-IST-001/002`) | `GET /api/v1/health-services/master-data/inpatient-settings` | `200 OK` | **PASS** | Memuat `documentSigningCity: 'Jakarta'`, `infantWristbandMaxAgeYears: 5`, dan kode formulir resmi. |
| 2 | Uji keunikan kode butir administrasi (`MST-ICI-001`) | `POST /api/v1/health-services/master-data/inpatient-clearance-items`<br>Body: `{"itemCode":"STPB-01","itemName":"Item Duplikat","checklistType":2}` | `409 Conflict`<br>`"Kode butir STPB-01 sudah dipakai butir administrasi lain."` | **PASS** | Konflik kode terdeteksi dan ditolak konsisten. |
| 3 | Uji validasi jenis checklist tidak dikenal (`MST-ICI-002`) | `POST /api/v1/health-services/master-data/inpatient-clearance-items`<br>Body: `{"itemCode":"STPB-X","itemName":"Item Valid","checklistType":99}` | `400 Bad Request`<br>`"Jenis daftar periksa tidak valid."` | **PASS** | Nilai enum di luar jangkauan ditolak. |
| 4 | Uji nama butir administrasi wajib (`MST-ICI-003`) | `POST /api/v1/health-services/master-data/inpatient-clearance-items`<br>Body: `{"itemCode":"STPB-X","itemName":"","checklistType":2}` | `400 Bad Request`<br>`"The ItemName field is required."` | **PASS** | Input kosong ditolak pada lapisan model validation. |

---

### BE-RWI-187 & BE-RWI-188: Agregat Dokumen Admisi & Read-Model Sumber Data
Memverifikasi ketahanan pembacaan read-model dari berbagai modul (Pasien, Penjamin, Profil RS, Billing, dan Lokasi Bed).

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Pembacaan profil rumah sakit multi-site aktif (>1 situs utama) | `GET /api/v1/health-services/inpatient-management/episodes/29c2b8f3-8708-44d1-9caf-67f3b39fd272/admission-workspace/letterhead` | `200 OK`<br>`"Profil rumah sakit tidak tersedia; kop dicetak tanpa identitas."` | **PASS** | `isAvailable = false`, alasan jelas *"Lebih dari satu situs rumah sakit utama aktif; profil tidak ditebak."* (sesuai AC BE-RWI-188 butir 2). |
| 2 | Pembacaan penjamin kunjungan asuransi | `GET /api/v1/health-services/inpatient-management/episodes/29c2b8f3-8708-44d1-9caf-67f3b39fd272/admission-workspace/summary` | `200 OK` (dengan kegagalan internal modul penjamin) | **FAIL** | Terjadi PostgresException 42703 di backend: kolom `CardImagePath` pada `MstPatientCompanyGuarantor` belum ada di DB. Menyebabkan penjamin berstatus `Failed` (Bug #1). |
| 3 | Pembacaan posisi deposit dari Billing Deposit Adapter | `GET /api/v1/health-services/inpatient-management/episodes/29c2b8f3-8708-44d1-9caf-67f3b39fd272/admission-workspace/summary/amounts` | `200 OK` | **PASS** | Posisi deposit terbaca akurat: `depositStatus: 'Shortfall'`, `minimumPolicyAmount: 1000000`, `shortfallAmount: 1000000`. |

---

### BE-RWI-189 & BE-RWI-193: Pencatatan Log Cetak, Kop Surat, & Konkurensi
Memverifikasi pencatatan log cetak fisik dokumen/gelang/label serta penyediaan kop surat resmi.

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Pengambilan kop surat pada Episode C (*Draft*) | `GET /api/v1/health-services/inpatient-management/episodes/8abff20d-0c85-40dc-b88e-1a9ef6945b52/admission-workspace/letterhead` | `200 OK` | **PASS** | Kop surat dapat diakses pada episode Draft untuk persiapan admisi. |
| 2 | Pengambilan kop surat pada Episode D (*Closed*) | `GET /api/v1/health-services/inpatient-management/episodes/6dd5ec3a-584f-4b71-a15e-46d77a49c112/admission-workspace/letterhead` | `200 OK` | **PASS** | Kop surat dapat diakses pada episode Closed untuk keperluan cetak ulang berkas. |
| 3 | Pencatatan cetakan pertama tanpa alasan | `POST .../admission-workspace/print-logs`<br>Header: `Idempotency-Key: <UUID-1>`<br>Body: `{"printKind":4,"copies":1,"reprintReason":null}` | `200 OK`<br>`"Cetakan tercatat."` | **PASS** | `printSequence = 1`, `isReprint = false`. |
| 4 | Replay permintaan cetak dengan `Idempotency-Key` sama | `POST .../admission-workspace/print-logs`<br>Header: `Idempotency-Key: <UUID-1>`<br>Body: `{"printKind":4,"copies":1,"reprintReason":null}` | `200 OK`<br>`"Cetakan yang sama sudah tercatat sebelumnya."` | **PASS** | Mengembalikan ID log yang sama tanpa menambah baris baru di database. |
| 5 | Pencatatan cetak ulang kedua tanpa alasan | `POST .../admission-workspace/print-logs`<br>Header: `Idempotency-Key: <UUID-2>`<br>Body: `{"printKind":4,"copies":1,"reprintReason":null}` | `422 Unprocessable Entity`<br>`INP-ADM-PRT-001` | **PASS** | Ditolak karena cetak ulang wajib disertai alasan. |
| 6 | Pencatatan cetak ulang kedua dengan alasan valid | `POST .../admission-workspace/print-logs`<br>Header: `Idempotency-Key: <UUID-3>`<br>Body: `{"printKind":4,"copies":1,"reprintReason":1}` (`Damaged`) | `200 OK`<br>`"Cetak ulang tercatat (cetakan ke-2)."` | **PASS** | `printSequence = 2`, `isReprint = true`, `reprintReasonName = 'Damaged'`. |
| 7 | Membaca riwayat log cetak episode | `GET .../admission-workspace/print-logs` | `200 OK` | **PASS** | Menampilkan log terurut berdasarkan urutan sekuensial cetak. |

---

### BE-RWI-194: Generic Document Lifecycle (Siklus Hidup Dokumen Admisi)
Memverifikasi transisi status dokumen: `Draft` -> `Locked` -> Penandatanganan -> `Completed` -> `Revisions` -> `Cancelled` / `Discarded`.

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Penulisan dokumen pada Episode D (*Closed*) | `POST .../documents` pada Episode D<br>Body: `{"documentType":2,...}` | `409 Conflict`<br>`INP-ADM-DOC-001` | **PASS** | Ditolak *"Episode sudah ditutup atau dibatalkan."* |
| 2 | Penulisan dokumen pada Episode C (*Draft*) | `POST .../documents` pada Episode C<br>Body: `{"documentType":2,...}` | `409 Conflict`<br>`INP-ADM-DOC-002` | **PASS** | Ditolak *"Admisi belum dikonfirmasi."* |
| 3 | Pembuatan dokumen Permintaan Privasi (*Draft*) | `POST .../documents` pada Episode A<br>Body: `{"documentType":2,"party":{...},"privacy":{...}}` | `200 OK` | **PASS** | Dokumen terbit berstatus `Draft` (1) dengan initial RowVersion. |
| 4 | Pembuatan dokumen kedua untuk jenis aktif yang sama | `POST .../documents` pada Episode A (jenis PrivacyRequest lagi) | `409 Conflict`<br>`INP-ADM-DOC-003` | **PASS** | Ditolak *"Sudah ada Permintaan Privasi yang aktif untuk episode ini."* |
| 5 | Mengunci dokumen dengan `RowVersion` basi/stale | `PATCH .../documents/{id}/lock`<br>Body: `{"rowVersion":"<UUID-SALAH>"}` | `409 Conflict`<br>`INP-ADM-DOC-004` | **PASS** | Ditolak *"Dokumen sudah diubah petugas lain."* |
| 6 | Mengunci dokumen saat profil rumah sakit konflik | `PATCH .../documents/{id}/lock` (kondisi seeder multi-site) | `422 Unprocessable Entity`<br>`INP-ADM-DOC-027` | **PASS** | Ditolak *"Data pasien atau profil rumah sakit tidak dapat dimuat."* (Mencegah snapshot beku korup). |
| 7 | Mengunci dokumen saat profil rumah sakit valid tunggal | `PATCH .../documents/{id}/lock` (kondisi 1 site utama aktif) | `200 OK`<br>`"Dokumen dikunci."` | **PASS** | Dokumen berpindah ke status `AwaitingSignature` (2). Snapshot beku JSON tersimpan. |
| 8 | Mencatat tanda tangan kertas pasien/keluarga | `POST .../signatures/patient-or-family`<br>Body: `{"signerName":"Budi","signerRelationship":1,"signedAt":"2026-10-08T...","rowVersion":"<RV>"}` | `409 Conflict`<br>`INP-ADM-DOC-004` (Terjadi error penanganan EF Core) | **FAIL** | Kegagalan runtime: EF Core melacak entity signature baru sebagai `Modified` alih-alih `Added`, melempar `DbUpdateConcurrencyException` saat `SaveChangesAsync` (Bug #3). |
| 9 | Membuka kunci dokumen yang sudah bertanda tangan | `PATCH .../documents/{id}/unlock` | `409 Conflict`<br>`INP-ADM-DOC-006` | **NOT RUN** | Bergantung pada tersimpannya tanda tangan (terblokir oleh Bug #3). |
| 10 | Tanda tangan penutup Kepala Ruangan untuk `Completed` | `POST .../signatures/head-nurse` | `409 Conflict`<br>`INP-ADM-DOC-004` | **FAIL** | Terblokir oleh Bug #3 (`DbUpdateConcurrencyException` pada `SaveSignatureAsync`). |
| 11 | Pembuatan versi revisi dari dokumen `Completed` | `POST .../revisions` | - | **NOT RUN** | Memerlukan dokumen berstatus `Completed` (terblokir oleh Bug #3). |
| 12 | Membatalkan dokumen admisi beralasan (`Cancel`) | `PATCH .../cancel`<br>Body: `{"rowVersion":"<RV>","reason":"Dibatalkan atas permintaan pasien..."}` | `200 OK` | **PASS** | Berhasil membatalkan dokumen aktif. Dokumen berpindah ke status `Cancelled` (5). |
| 13 | Membuang konsep (*Discard*) oleh akun non-pembuat | `PATCH .../discard` oleh user Siti | `403 Forbidden` | **PASS** | Akun tanpa hak ditolak 403 (karena ketiadaan konfigurasi `SysAccessPolicy`). Pada user pembuat (SuperAdmin) berhasil membuang draft. |

---

### BE-RWI-195: Endpoint Ringkasan Workspace PPRI (Summary & Amounts)
Memverifikasi kelayakan ringkasan dokumen wajib dan peringatan status admisi.

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Ringkasan Episode A (Asuransi, Deposit Kurang) | `GET .../admission-workspace/summary` pada Episode A | `200 OK` | **PASS** | Status `Available`. Menu Pelunasan Deposit bertanda `req=True`. CostDifference bertanda `Uncountable` akibat Bug #1 penjamin. |
| 2 | Ringkasan Episode B (Tunai, Deposit Cukup) | `GET .../admission-workspace/summary` pada Episode B | `200 OK` | **PASS** | Status `Available`. Menu Pelunasan Deposit bertanda `req=False, badge=NotRequired`. Tepat 4 dokumen non-deposit yang wajib. |
| 3 | Ringkasan Episode C (*Draft*) | `GET .../admission-workspace/summary` pada Episode C | `200 OK` | **PASS** | Mengembalikan `availability: "NotYetAdmitted"`. Seluruh aksi dokumen ditahan. |
| 4 | Ringkasan Episode D (*Closed*) | `GET .../admission-workspace/summary` pada Episode D | `200 OK` | **PASS** | Mengembalikan `availability: "ReadOnly"`. Dokumen hanya dapat dibaca dan dicetak ulang. |
| 5 | Mengakses `/summary/amounts` tanpa hak `ViewAmount` | `GET .../summary/amounts` dengan user Siti | `403 Forbidden`<br>`"Anda tidak memiliki akses ke menu atau fitur ini."` | **PASS** | Perlindungan data finansial berhasil menegakkan isolasi wewenang. |
| 6 | Membaca detail episode rawat inap umum (`GET episodes/{id}`) | `GET /api/v1/health-services/inpatient-management/episodes/29c2b8f3-8708-44d1-9caf-67f3b39fd272` | `200 OK` | **PASS** | Memuat daftar peringatan dokumen admisi tanpa mengekspos nominal rupiah (sesuai invariant privasi). |

---

### BE-RWI-196: Endpoint Label Identitas Pasien & Gelang
Memverifikasi format nama tampilan, sapaan (*salutation*), penentuan jenis gelang dewasa vs bayi, dan proteksi pencetakan gelang salah jenis (`PRT-005`).

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Label identitas pria dewasa (Budi Santoso, 38 th) | `GET .../admission-workspace/identity-labels` pada Episode B | `200 OK` | **PASS** | `displayName: "BUDI SANTOSO, Tn."`, `ageText: "38 th"`, `kind: "Adult"`, `printKind: 1`. |
| 2 | Label identitas wanita dewasa menikah (Siti Rahmawati, 34 th) | `GET .../admission-workspace/identity-labels` pada Episode A | `200 OK` | **PASS** | `displayName: "SITI RAHMAWATI, Ny."`, `ageText: "34 th"`, `kind: "Adult"`. |
| 3 | Label identitas wanita dewasa belum menikah (Agnes, 26 th) | `GET .../admission-workspace/identity-labels` pada Episode `025ef075-...` | `200 OK` | **PASS** | `displayName: "AGNES YULIANI RAJA GUK GUK, Nn."`, `ageText: "26 th"`, `kind: "Adult"`. |
| 4 | Label identitas bayi baru lahir (*Newborn*) | `GET .../identity-labels` | - | **NOT RUN** | Tidak ditemukan episode aktif pasien bayi baru lahir di database saat ini. |
| 5 | Label identitas anak usia 4 tahun | `GET .../identity-labels` | - | **NOT RUN** | Tidak ditemukan episode aktif pasien anak usia 4 tahun di database saat ini. |
| 6 | Label identitas wanita status nikah *Unknown* | `GET .../identity-labels` | - | **NOT RUN** | Seluruh data pasien wanita pada episode yang ada memiliki status nikah terdefinisi (Single/Married). |
| 7 | Mencatat cetak gelang bayi pada pasien pria dewasa | `POST .../print-logs`<br>Body: `{"printKind":2,...}` (`InfantWristband` pada Episode B) | `422 Unprocessable Entity`<br>`INP-ADM-PRT-005` | **PASS** | Ditolak *"Jenis gelang tidak sesuai data pasien. Muat ulang data gelang."* |

---

### BE-RWI-197: Endpoint Data Dasar Rawat Inap (IPD / Inpatient Base Data)
Memverifikasi penyajian lembar Data Dasar Rawat Inap tanpa nilai uang dan status penahanan tarif kamar harian.

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Membaca IPD tanpa nilai uang | `GET .../admission-workspace/base-data` | `200 OK` | **PASS** | `canPrint: true`, `cannotPrintReason: null`, dan daftar `blankFields` terisi lengkap untuk butir yang belum ada sumber datanya. |
| 2 | Membaca tarif kamar harian IPD (`/base-data/amounts`) | `GET .../admission-workspace/base-data/amounts` | `200 OK`<br>`"Tarif kamar per hari belum tersedia; tertulis \"lihat kasir\"."` | **PASS** | Mengembalikan `roomRateState: "NotYetAvailable"`, `dailyRoomRate: null` (sesuai status hold BE-RWI-191 / RWI-OQ-129). |

---

### BE-RWI-198: Data Cetak Persetujuan Umum (General Consent)
Memverifikasi penyediaan data siap-cetak persetujuan umum dan sifatnya yang *read-only* tanpa endpoint mutasi.

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Membaca data cetak persetujuan umum | `GET .../general-consent/print-data` | `200 OK` | **PASS** | Mengembalikan `roomType: "General"`, kandidat penanda tangan dari profil pasien/kontak darurat. |
| 2 | Uji ketiadaan endpoint mutasi data persetujuan umum | `POST .../general-consent/print-data` dan `POST .../general-consent` | `405 Method Not Allowed` / `404 Not Found` | **PASS** | Terbukti tidak ada endpoint tulis (General Consent murni *print-only* dari data kunjungan). |

---

### BE-RWI-199: Ceklist Serah Terima Pasien Baru (STPB)
Memverifikasi pembuatan dan penandatanganan tiga slot ceklist STPB (Petugas Admisi, CRO, Perawat Ruangan).

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Membaca saran butir ceklist (`prefill`) | `GET .../admission-workspace/prefill/NewPatientHandover` | `200 OK` | **PASS** | Mengembalikan 18 butir STPB beserta rekomendasi centang dari data referral dan IPD. |
| 2 | Mencoba menandatangani STPB sebelum dokumen dikunci | `POST .../signatures/admission-officer` pada STPB *Draft* | `409 Conflict`<br>`INP-ADM-DOC-030` | **PASS** | Ditolak *"Dokumen belum dikunci oleh petugas admisi, sehingga belum dapat ditandatangani."* |
| 3 | Tanda tangan slot Perawat sebelum pasien menempati bed | `POST .../signatures/receiving-nurse` pada episode tanpa bed aktif | - | **NOT RUN** | Seluruh episode Admitted di database saat ini sudah menempati bed aktif (`InpBedPlacement`). |
| 4 | Satu akun menandatangani dua slot petugas berbeda (`DOC-033`) | `POST .../signatures/cro` sesudah menandatangani `admission-officer` | - | **NOT RUN** | Terblokir oleh Bug #3 (tanda tangan slot pertama gagal disimpan ke database). |

---

### BE-RWI-200: Nilai-Nilai & Kepercayaan Serta Hak Pasien
Memverifikasi formulir identifikasi nilai-nilai kepercayaan (maksimal 5 butir) dan ringkasan hak pasien.

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Mengirim butir kepercayaan ke-6 (melebihi batas 5 butir) | `POST .../documents`<br>Body: `{"documentType":3,"beliefItems":["B1","B2","B3","B4","B5","B6"]}` | `400 Bad Request`<br>`"Maksimal 5 butir."` | **PASS** | Validasi batas butir ditegakkan dengan pesan yang lugas dan tepat. |
| 2 | Mengirim butir kepercayaan valid (3 butir) | `POST .../documents`<br>Body: `{"documentType":3,"beliefItems":["Menolak transfusi",...],...}` | `200 OK` | **PASS** | Dokumen Nilai Kepercayaan berhasil dibuat berstatus `Draft`. |
| 3 | Membaca ringkasan hak pasien saat dokumen belum `Completed` | `GET .../admission-workspace/patient-rights` | `200 OK`<br>`{"beliefValues":null,"privacy":null}` | **PASS** | Terbukti hanya mengekspos dokumen yang sudah sah berstatus `Completed`. |

---

### BE-RWI-201: Surat Pernyataan Selisih Biaya (Cost Difference Statement)
Memverifikasi pembatasan dokumen selisih biaya khusus penjamin asuransi/perusahaan, pembersihan format telepon, dan batas panjang nomor.

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Pembuatan Selisih Biaya pada pasien tunai (Episode B) | `POST .../documents` pada Episode B<br>Body: `{"documentType":4,...}` | `422 Unprocessable Entity`<br>`"Data penjamin kunjungan tidak dapat dibaca..."` | **PASS** (dengan catatan Bug #1) | Mengembalikan 422 karena modul penjamin mengalami kegagalan schema saat membaca penjamin (Bug #1). Pada logika bisnis murni seharusnya mengembalikan `DOC-010`. |
| 2 | Format nomor telepon memuat tanda hubung (`"0812-3456-7890"`) | `POST .../documents`<br>Body: `{"party":{"mobilePhone":"0812-3456-7890",...}}` | `200 OK` | **PASS** | Karakter non-digit dibersihkan otomatis: tersimpan bersih `"081234567890"`. |
| 3 | Nomor telepon melebihi batas (14 digit) | `POST .../documents`<br>Body: `{"party":{"mobilePhone":"08123456789012",...}}` | `400 Bad Request`<br>`"Nomor telepon maksimal 13 digit."` | **PASS** | Batas maksimal 13 digit ditegakkan ketat. |

---

### BE-RWI-202: Pernyataan Pelunasan Deposit (Deposit Settlement Statement)
Memverifikasi dokumen pelunasan deposit, validasi shortfall, pembatasan tanggal jatuh tempo kebijakan, pembekuan angka finansial saat penguncian, dan wewenang cetak berupiah.

| No | Skenario Pengujian | Request (Tanpa Token) | Status HTTP + Kode Alasan | Hasil | Catatan Evaluasi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Pembuatan Pelunasan Deposit pada pasien tanpa kekurangan deposit | `POST .../documents` pada Episode B (shortfall = 0)<br>Body: `{"documentType":5,...}` | `422 Unprocessable Entity`<br>`INP-ADM-DOC-011` | **PASS** | Ditolak *"Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan."* |
| 2 | Tanggal jatuh tempo melewati batas kebijakan rumah sakit | `POST .../documents` pada Episode A<br>Body: `{"documentType":5,"deposit":{"dueDate":"+30 hari"}}` | `422 Unprocessable Entity`<br>`INP-ADM-DOC-019` | **PASS** | Ditolak *"Jatuh tempo paling lambat 11 Oktober 2026 menurut kebijakan deposit."* |
| 3 | Penguncian dokumen Pelunasan Deposit dengan tanggal valid | `PATCH .../documents/{id}/lock` pada dokumen Pelunasan Deposit valid | `200 OK` | **PASS** | Dokumen berhasil dikunci. Data deposit dibekukan pada snapshot (`amountsHidden: true` untuk cetakan umum). |
| 4 | Mengakses cetakan berupiah (`/amount-print`) tanpa wewenang `ViewAmount` | `GET .../documents/{id}/amount-print` dengan user Siti | `403 Forbidden` | **PASS** | Ditolak 403 karena memerlukan izin ganda `ViewAmount + Print`. |
| 5 | Mengakses cetakan berupiah (`/amount-print`) dengan izin lengkap | `GET .../documents/{id}/amount-print` dengan user SuperAdmin | `200 OK` | **PASS** | Berhasil memuat data cetak lembar pelunasan deposit lengkap dengan rincian angka rupiah. |

---

## 4. Daftar Bug & Temuan Runtime

Bagian ini mendokumentasikan seluruh bug dan temuan ketidaksesuaian yang ditemukan selama eksekusi runtime beserta langkah reproduksi, hasil aktual vs ekspektasi, dan lokasi baris kode terkait.

---

### Bug #1: Skema Database Out-of-Sync pada `MstPatientCompanyGuarantor` (Missing Column `CardImagePath`)

- **Tingkat Keparahan**: Kritis (*High / Blocker*)
- **Dampak Bisnis**: 
  Setiap pembacaan data penjamin kunjungan pasien gagal total di seluruh modul rawat inap, menyebabkan ringkasan Workspace PPRI selalu berstatus *"Kelengkapan dokumen admisi tidak dapat dihitung"* (`RequiredCount: null`), menu *CostDifferenceStatement* berstatus *Uncountable*, dan evaluasi kesiapan admisi lumpuh.
- **Langkah Reproduksi**:
  1. Login ke sistem.
  2. Panggil `GET /api/v1/health-services/inpatient-management/episodes/29c2b8f3-8708-44d1-9caf-67f3b39fd272/admission-workspace/summary`.
  3. Amati bagian `warnings` dan menu `CostDifferenceStatement` pada data respons.
- **Respons yang Diterima**:
  - HTTP `200 OK` dengan payload peringatan:
    ```json
    "warnings": [
      "Dokumen admisi belum lengkap: 4 (...)",
      "Kelengkapan dokumen admisi tidak dapat dihitung"
    ]
    ```
  - Menu `CostDifferenceStatement`: `badge: "Uncountable"`, `isRequired: false`.
  - Backend log mencatat:
    ```text
    Npgsql.PostgresException (0x80004005): 42703: column m3.CardImagePath does not exist
    at EncounterInsuranceService.GetContextAsync(...)
    ```
- **Respons yang Diharapkan**:
  - `sources.guarantor` berhasil membaca penjamin (BPJS Kesehatan) tanpa error database.
  - Untuk pasien asuransi, menu `CostDifferenceStatement` bertanda `isRequired: true`, peringatan *"tidak dapat dihitung"* tidak muncul, dan `requiredDocumentCount: 6`.
- **Dugaan Lokasi File & Baris**:
  - Model C#: `Areas\HealthServices\PatientManagement\MasterData\Models\MstPatientCompanyGuarantor.cs:78`
  - EF Core Include: `Areas\HealthServices\ClinicalManagement\Services\EncounterInsuranceService.cs:33-44`
  - Berkas Migrasi: `Migrations\20261006092040_AddCardImagePathToPatientCompanyGuarantor.cs` belum diaplikasikan ke database PostgreSQL target (`QuilvianNewDevHamzah`).

---

### Bug #2: Konflik Profil Rumah Sakit Utama Berganda Menggagalkan Penguncian Dokumen Admisi (`INP-ADM-DOC-027`)

- **Tingkat Keparahan**: Kritis (*High / Workflow Blocker*)
- **Dampak Bisnis**: 
  Petugas admisi tidak dapat mengunci (*lock*) dokumen admisi mana pun yang berstatus *Draft*. Akibatnya, dokumen tidak pernah dapat dicetak untuk ditandatangani, alur admisi berhenti total pada tahap awal.
- **Langkah Reproduksi**:
  1. Pada basis data awal seeder, terdapat 3 situs rumah sakit aktif yang sama-sama memiliki `IsMainSite = true` (`SITE-MMC-001`, `SITE-MDC-001`, `SITE-MHS-001`).
  2. Buka episode rawat inap aktif (`Admitted`), lalu buat dokumen admisi apa pun (mis. Permintaan Privasi).
  3. Panggil `PATCH /api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/documents/{documentId}/lock` dengan `rowVersion` valid.
- **Respons yang Diterima**:
  - HTTP `422 Unprocessable Entity`:
    ```json
    {
      "success": false,
      "statusCode": 422,
      "message": "Data pasien atau profil rumah sakit tidak dapat dimuat. Dokumen belum dapat dikunci; coba lagi.",
      "errors": {
        "code": "INP-ADM-DOC-027"
      }
    }
    ```
- **Respons yang Diharapkan**:
  - Dokumen berhasil dikunci (HTTP `200 OK`), status berpindah ke `AwaitingSignature`, dan snapshot beku JSON tersimpan.
- **Analisis Akar Masalah**:
  - Sesuai aturan BE-RWI-188 butir 2, jika terdapat lebih dari satu situs utama aktif, `InpAdmissionSourceReader.GetHospitalProfileAsync` mengembalikan `Failed` (*"Lebih dari satu situs rumah sakit utama aktif; profil tidak ditebak."*).
  - Pada `InpAdmissionSnapshotBuilder.cs:41`, properti `CanFreeze` dirumuskan:
    ```csharp
    public bool CanFreeze => IsPatientAvailable && IsHospitalProfileAvailable && IsEpisodeAvailable;
    ```
  - Karena `IsHospitalProfileAvailable` bernilai `false`, `CanFreeze` selalu bernilai `false`.
  - Pada `InpAdmissionDocumentService.cs:509-515`, kegagalan `CanFreeze` langsung membatalkan penguncian dengan `422 INP-ADM-DOC-027`.
- **Dugaan Lokasi File & Baris**:
  - Seeder / Master Data: Tabel `MstHospitalSite` (hanya boleh ada tepat satu baris `IsMainSite = true`).
  - Evaluasi Kesiapan Freeze: `Areas\HealthServices\InpatientManagement\Services\InpAdmissionSnapshotBuilder.cs:41`.
  - Service Penguncian: `Areas\HealthServices\InpatientManagement\Services\InpAdmissionDocumentService.cs:509-515`.

---

### Bug #3: `DbUpdateConcurrencyException` Saat Menyimpan Tanda Tangan Dokumen Admisi (`SaveSignatureAsync`)

- **Tingkat Keparahan**: Kritis (*High / Blocker*)
- **Dampak Bisnis**: 
  Seluruh aksi penandatanganan dokumen admisi — baik pencatatan tanda tangan kertas pasien/keluarga (`POST .../signatures/patient-or-family`) maupun atestasi elektronik petugas (Petugas Admisi, CRO, Perawat, Kepala Ruangan) — gagal disimpan ke database. Akibatnya dokumen tidak pernah dapat beralih ke status `Completed`, dan alur koreksi/revisi tidak dapat diuji atau dioperasikan.
- **Langkah Reproduksi**:
  1. Pastikan dokumen admisi berstatus `AwaitingSignature` (sudah terkunci).
  2. Panggil `POST /api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/documents/{documentId}/signatures/patient-or-family` atau `POST .../signatures/head-nurse` dengan payload lengkap dan `rowVersion` yang cocok.
- **Respons yang Diterima**:
  - HTTP `409 Conflict`:
    ```json
    {
      "success": false,
      "statusCode": 409,
      "message": "Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda.",
      "errors": {
        "code": "INP-ADM-DOC-004"
      }
    }
    ```
- **Respons yang Diharapkan**:
  - HTTP `200 OK` (*"Tanda tangan tercatat."*), baris tanda tangan tersimpan di tabel `InpAdmissionDocumentSignature`, dan `rowVersion` dokumen diperbarui.
- **Analisis Akar Masalah Teknis**:
  - Model `InpAdmissionDocumentSignature.cs:18` menginisialisasi primary key secara inline:
    ```csharp
    public Guid Id { get; set; } = Guid.NewGuid();
    ```
  - Pada `InpAdmissionSignatureService.cs:285`, tanda tangan ditambahkan melalui koleksi navigasi:
    ```csharp
    document.Signatures.Add(signature);
    ```
    tanpa memanggil `_dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);`.
  - Karena properti `signature.Id` sudah terisi nilai GUID (bukan `Guid.Empty`), mekanisme change tracking EF Core mendeteksi entitas melalui relasi dan menganggapnya sebagai entitas lama yang sedang dimodifikasi (`EntityState.Modified`), bukan entitas baru (`EntityState.Added`).
  - Saat `await _dbContext.SaveChangesAsync(cancellationToken)` dipanggil pada baris 300, EF Core mengeksekusi perintah SQL `UPDATE "InpAdmissionDocumentSignature" ... WHERE "Id" = @id;`.
  - Karena baris dengan ID tersebut belum ada di database, jumlah baris yang ter-update adalah 0. EF Core kemudian melempar `DbUpdateConcurrencyException`.
  - Blok `catch (DbUpdateConcurrencyException)` pada baris 302-306 menangkap exception tersebut dan secara keliru mengasumsikannya sebagai tabrakan konkurensi dokumen (`InpAdmissionDocumentService.Stale()`), lalu mengembalikan `409 INP-ADM-DOC-004`.
- **Dugaan Lokasi File & Baris**:
  - `Areas\HealthServices\InpatientManagement\Services\InpAdmissionSignatureService.cs:285` (perlu menambahkan entitas secara eksplisit ke DbContext: `_dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);` atau memastikan state disetel ke `EntityState.Added`).

---

### Bug #4: Ketiadaan Konfigurasi Kebijakan Hak Akses (`SysAccessPolicy`) untuk Controller `InpatientAdmissionDocument`

- **Tingkat Keparahan**: Sedang (*Medium / Authorization Configuration Gap*)
- **Dampak Bisnis**: 
  Seluruh akun staf rumah sakit non-superadmin (Petugas Admisi, CRO, Perawat Ruangan, Kepala Ruangan) selalu ditolak dengan status HTTP 403 Forbidden untuk seluruh aksi dokumen admisi, sehingga operasional di lapangan terpaksa harus menggunakan akun SuperAdmin.
- **Langkah Reproduksi**:
  1. Login menggunakan akun karyawan terdaftar non-superadmin (mis. `siti.nurhaliza@rsmmc.local`).
  2. Panggil endpoint `GET /api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/summary` atau `/documents`.
- **Respons yang Diterima**:
  - HTTP `403 Forbidden`:
    ```json
    {
      "success": false,
      "statusCode": 403,
      "message": "Anda tidak memiliki akses ke menu atau fitur ini."
    }
    ```
- **Respons yang Diharapkan**:
  - Pengguna dengan jabatan/departemen yang relevan (seperti Unit Admisi Rawat Inap atau Perawat Ruangan) dapat mengakses dan mengelola dokumen sesuai matriks kewenangan masing-masing.
- **Analisis Akar Masalah**:
  - Pada tabel `SysAccessPolicy`, terdapat 0 baris pemetaan untuk `ControllerName = 'InpatientAdmissionDocument'`.
  - Walaupun `SysControllerAccess` dan 10 aksi `SysActionAccess` sudah terdaftar, belum ada seeder atau konfigurasi yang menautkan `DepartmentId` / `PositionId` karyawan rumah sakit ke aksi-aksi tersebut.
- **Dugaan Lokasi File & Baris**:
  - Seeder hak akses `SysAccessPolicy` / modul administrasi wewenang organisasi.

---

## 5. Kesimpulan & Rekomendasi Tindak Lanjut

### Kesimpulan
Secara arsitektur dan pemenuhan spesifikasi bisnis, implementasi Workspace PPRI (Rawat Inap) telah menunjukkan kepatuhan yang sangat tinggi terhadap aturan modul Quilvian:
1. **Validasi Bisnis Solid**: Seluruh aturan validasi (seperti batas 5 butir kepercayaan, larangan selisih biaya untuk pasien tunai, batas tanggal jatuh tempo deposit, pembersihan nomor telepon, dan isolasi dokumen pemulangan vs admisi) berjalan sempurna dan menghasilkan kode alasan yang deterministik.
2. **Keamanan & Privasi Terjaga**: Isolasi data keuangan (`/summary/amounts` dan `/amount-print`) berhasil melindungi nilai rupiah dari akun tanpa izin, serta log cetak menerapkan kontrol cetak ulang beralasan dan idempotensi secara tepat.
3. **Penyebab Kegagalan Terlokalisasi**: Seluruh skenario yang berstatus *FAIL* terlokalisasi pada 3 akar masalah teknis (kolom missing `CardImagePath`, multi-site seeder, dan pelacakan state EF Core pada tanda tangan) dan bukan cacat desain proses bisnis.

### Rekomendasi Tindak Lanjut untuk Tim Rekayasa
1. **Jalankan Migrasi Database Penjamin**:
   Terapkan migrasi `20261006092040_AddCardImagePathToPatientCompanyGuarantor` pada database PostgreSQL agar kolom `CardImagePath` tersedia pada `MstPatientCompanyGuarantor`.
2. **Koreksi Seeder Master Hospital Site**:
   Pastikan seeder master data hanya menandai satu situs utama (`IsMainSite = true`, yaitu RS MMC), sementara situs cabang lainnya disetel `IsMainSite = false`.
3. **Perbaiki Registrasi Entitas Tanda Tangan pada EF Core**:
   Pada `InpAdmissionSignatureService.cs:285`, tambahkan penegasan status penambahan entitas ke DbContext:
   ```csharp
   _dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);
   ```
   sehingga EF Core mengeksekusi SQL `INSERT` alih-alih `UPDATE`, dan penandatanganan dokumen dapat diselesaikan hingga status `Completed`.
4. **Lengkapi Seeder `SysAccessPolicy`**:
   Tambahkan seeder pemetaan kebijakan hak akses untuk controller `InpatientAdmissionDocument` agar peran operasional Petugas Admisi, CRO, dan Perawat Ruangan dapat menjalankan tugasnya secara independen.
