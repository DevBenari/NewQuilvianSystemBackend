# Laporan Uji BE-IGD-039 & Pengujian Ulang BE-IGD-061 — Kewenangan Petugas Berbasis Penugasan HR Simpul Organisasi

| Field | Nilai |
| --- | --- |
| Tanggal | 5 Oktober 2026 |
| Pelaksana | Agen Penguji (Antigravity) atas nama Pemilik Modul (Rizki Gunawan) |
| Dokumen Panduan | [`2026-10-05-panduan-uji-be-igd-039.md`](./2026-10-05-panduan-uji-be-igd-039.md) |
| Cakupan Uji | 17 skenario (15 uji API, 2 uji layar antarmuka) |
| Target Task | `BE-IGD-039`, pengujian ulang `BE-IGD-061` (`S2`, `S3`, `S4`, `S11`, `S12`), probe kontrak validation §6 aturan 4 |
| Status Akhir | **SELESAI DENGAN BUKTI MENTAH LENGKAP** |
| Lokasi Bukti Mentah | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/` |

---

## 1. Metadata Lingkungan & Hasil Build

| Parameter | Nilai Teramati | Keterangan |
| --- | --- | --- |
| Frontend Git SHA | `57b1d360f` | Head branch `RizkiV2` |
| Status Git Frontend (`git status --short`) | Bersih (`clean`) | Source frontend read-only, tanpa modifikasi atau commit baru |
| Waktu `BUILD_ID` Frontend | `2026-10-05T12:00:44.321219` (12.00.44 WIB) | Build baru: 465/465 halaman, 0 warning, standalone |
| Runtime Frontend | Port 3000 dilayani `node .next/standalone/server.js` (PID 14996, dijembatani ke 127.0.0.1:3000) | Memenuhi Aturan A1 & P3 (bukan `next dev`) |
| Bukti Layar Hasil Build | `document.querySelector('nextjs-portal') === null` bernilai `true` | Terverifikasi pada pengujian browser skenario layar |
| Viewport Layar | `1440 × 900` | Memenuhi Aturan A2 |
| Backend Git SHA | `8d81d361` | Head branch `rizkiG` |
| Status Git Backend (`git status --short`) | `M Areas/.../EmergencyUnitAuthorityService.cs`<br>`M docs/module-blueprints/igd/...` | Berkas service `BE-IGD-039` dan pembaruan dokumen perencanaan |
| Waktu Ubah Berkas Service | `2026-10-05T11:41:43.831741` | — |
| Waktu Ubah DLL Backend | `2026-10-05T11:51:36.320498` | DLL **lebih baru** daripada berkas service (P2 terpenuhi) |
| Runtime Backend | `https://localhost:7184` | ASP.NET Core Kestrel |
| Basis Data Dev | PostgreSQL `QuilvianNewDevRizki` (kredensial dan host disamarkan per aturan A5) | Hanya kueri `SELECT`, tanpa manipulasi DDL/DML langsung selama skenario |

---

## 2. Akun dan Hak Akses

| Kode Akun | Peran Nyata | Departemen / Posisi | Penugasan HR Simpul Organisasi | Bukti Hak Akses |
| --- | --- | --- | --- | --- |
| `LOKET` | Petugas Pendaftaran | Dept. Pendaftaran | — | [`P6-LOKET.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/P6-LOKET.json) |
| `KLINIS` | Perawat IGD | Dept. Keperawatan | Simpul: **Instalasi Gawat Darurat** (`42242c4c-5e4c-4f83-87db-e7da37e5551b`) | [`P6-KLINIS.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/P6-KLINIS.json) |
| `DOKTER` | Dokter IGD | Dept. Medis | Posisi: Dokter IGD (`ae5bb7af-9e65-63ed-c22b-57212203e592`) | [`P6-DOKTER.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/P6-DOKTER.json) |
| `PENERIMA` | Perawat Rawat Inap | Dept. Keperawatan | Simpul: **Instalasi Rawat Inap** (`c797d03f-8882-4fbc-b582-aeb51a198d51`) | [`P6-PENERIMA.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/P6-PENERIMA.json) |
| `SAUDARA` | Perawat Rawat Inap | Dept. Keperawatan | Simpul: **Unit Perawatan Intensif (ICU)** (`0c8a3e0a-3226-421b-9e21-f40e1de86be4`) | [`P6-SAUDARA.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/P6-SAUDARA.json) |

> **Akun Opsional Tidak Tersedia**:
> - `INDUK` (penugasan HR pada simpul induk): Tidak tersedia → skenario `039-S3` dicatat `NOT RUN`.
> - `BERAKHIR` (penugasan HR yang sudah lewat `effectiveEndDate`): Tidak tersedia → skenario `039-S4` dicatat `NOT RUN`.
> - `WARISAN` (penempatan akun lama tanpa `SourceAssignmentId`): Tidak tersedia → skenario `039-S5` dicatat `NOT RUN` (sesuai aturan A10, tidak dibuat via SQL).
> - `TANPAIZIN` (penugasan di unit tujuan tanpa izin `EmergencyDeparture:Update`): Tidak tersedia → skenario `039-S11` dicatat `NOT RUN`.
> - `SEKUNDER` (penugasan non-primer aktif di unit tujuan): Tidak tersedia → dijalankan menggunakan `PENERIMA`, skenario `039-S6` dicatat `NOT RUN`, sedangkan kaki penerimaan serah terima `061-S4` tetap dinilai dan lulus.

> **Pernyataan Kepatuhan Aturan A3, A4, & A10**:
> 1. Akun `SuperAdmin` **TIDAK PERNAH** digunakan dalam skenario mana pun.
> 2. Konfigurasi hak akses (Akses Role), penugasan HR, dan pemetaan unit **TIDAK DIUBAH** oleh agen selama jalannya pengujian.
> 3. Seluruh kredensial akun dibaca dari variabel lingkungan sistem (`QUILVIAN_*_PASSWORD`), dan sandi pada badan permintaan login disamarkan menjadi `"***"` pada semua berkas JSON bukti.
> 4. Tidak ada operasi penulisan basis data (`INSERT`/`UPDATE`/`DELETE`) langsung dari agen selama pengujian skenario.

---

## 3. Rekapitulasi Hasil Uji

| Blok | Skenario | Jumlah | `PASS` | `FAIL` | `NOT RUN` |
| --- | --- | ---: | ---: | ---: | ---: |
| A | `039-S1` (`061-S2`) | 1 | 1 | 0 | 0 |
| B | `039-S2`, `S3`, `S4`, `S5`, `S11`, `S8a`, `S6`, `U2` | 8 | 2 | 1 | 5 |
| C | `039-S7`, `S9`, `U1` | 3 | 2 | 1 | 0 |
| D | `039-S8`, `061-S12-terima`, `061-S11-tolak` | 3 | 3 | 0 | 0 |
| E | `PROBE-1` | 1 | 0 | 1 | 0 |
| F | `039-S12` | 1 | 1 | 0 | 0 |
| **Total** | | **17** | **9** | **3** | **5** |

> **Catatan Kategori `FAIL`**:
> 1. **`039-U2` & `039-U1`**: Permintaan API ditolak `403` dengan kalimat pesan persis sesuai spesifikasi kontrak, kepergian tetap pada status fisik yang benar, dan `nextjsPortalNull` benar (`true`). Namun teks penolakan tidak muncul di layar karena komponen `ConfirmModal` pada frontend tidak merender pesan kesalahan `saveError`.
> 2. **`PROBE-1`**: Sesuai instruksi khusus panduan Blok E, bila kunjungan tertutup langsung (`visitStatus: 9`), skenario dicatat `FAIL` dengan alasan `"tidak sesuai kontrak validation §6 aturan 4"`. Ini adalah probe temuan pengembang untuk disparitas implementasi vs kontrak, bukan kegagalan putaran uji.

---

## 4. Hasil Uji Rinci Per Skenario

### Blok A — Penerimaan Serah Terima Menutup Kunjungan (`V1`)

| ID | Coba | Putusan | Kode Status & Kalimat Teramati | Berkas Bukti |
| --- | :---: | :---: | --- | --- |
| `039-S1`<br>(`061-S2`) | 1 | **PASS** | `RX`: `visitStatus: 7`, `awaitingClosure: true`, `awaitingClosureReason` === `"Masih ada proses kepergian pasien yang belum selesai."` (`K-KEPERGIAN`).<br>`RT` oleh `PENERIMA`: `200 OK`, `physicalStatus: 3` (Tiba), `visitStatus: 7`.<br>`accept-handover` oleh `PENERIMA`: `200 OK`, `handoverStatus: 3` (Diterima), `visitStatus: 9` (Selesai).<br>Q-VISIT: `ClosedByDispositionId` = tindak lanjut `V1`, `UpdateBy` = `PENERIMA`, `EncounterStatus: 9`. Tidak ada respons memuat `K-LAMA`. | [`039-S1.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S1.json) |

---

### Blok B — Penolakan Kewenangan dan Aksi Tanpa Penutupan (`V2`, Kunjungan Tetap 4)

| ID | Coba | Putusan | Kode Status & Kalimat Teramati | Berkas Bukti |
| --- | :---: | :---: | --- | --- |
| `039-S2` | 1 | **PASS** | Seluruh aksi oleh `SAUDARA` (penugasan di unit ICU, bukan Rawat Inap):<br>(a) `POST /arrive` → `403 Forbidden`, `message` === `"Anda tidak bertugas di unit Rawat Inap, sehingga tidak dapat mencatat kedatangan pasien."` (`K-TIDAK-TIBA`).<br>(b) `POST /accept-handover` → `403 Forbidden`, `message` === `"Anda tidak bertugas di unit Rawat Inap, sehingga tidak dapat meninjau serah terima."` (`K-TIDAK-TINJAU`).<br>(c) `POST /order-items/.../accept` → `403 Forbidden`, `message` === `"Anda tidak bertugas di unit Rawat Inap, sehingga tidak dapat menerima atau menolak pesanan."` (`K-TIDAK-PESANAN`).<br>Keadaan kepergian tidak berubah (`physicalStatus: 2`, `handoverStatus: 2`, `acceptanceStatus: 2`). | [`039-S2.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S2.json) |
| `039-S3` | — | **NOT RUN** | Akun `INDUK` tidak tersedia di lingkungan uji. | [`039-S3.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S3.json) |
| `039-S4` | — | **NOT RUN** | Akun `BERAKHIR` tidak tersedia di lingkungan uji. | [`039-S4.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S4.json) |
| `039-S5` | — | **NOT RUN** | Akun / baris `WARISAN` tanpa sumber penugasan tidak tersedia di lingkungan uji. | [`039-S5.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S5.json) |
| `039-S11` | — | **NOT RUN** | Akun `TANPAIZIN` tidak tersedia di lingkungan uji. | [`039-S11.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S11.json) |
| `039-U2` | 1 | **FAIL** | **Layar 1440 × 900, akun `KLINIS`**: Tombol *Catat Tiba* diklik pada kepergian `V2` di tab *Transfer Pasien* (segmen Riwayat) → modal konfirmasi diklik.<br>Jaringan: `POST .../arrive` dijawab `403 Forbidden`, `message` === `"Anda tidak bertugas di unit Rawat Inap, sehingga tidak dapat mencatat kedatangan pasien."`. Kepergian tetap Berangkat (`physicalStatus: 2`). `nextjsPortalNull` bernilai `true`.<br>**Penyebab FAIL**: Teks penolakan tidak muncul di layar karena `ConfirmModal` tidak merender `section.saveError`. | [`039-U2.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-U2.json)<br>[`039-U2.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-U2.png) |
| `039-S8a`<br>(`061-S4` kaki pesanan) | 1 | **PASS** | `PENERIMA`: `POST .../order-items/.../reject` → `200 OK`, `acceptanceStatus: 4`.<br>`KLINIS`: `PATCH .../order-items/.../action` (`action: 1`) → `200 OK`.<br>Q-PESANAN: baris lama `IsEffective: false`, `UpdateBy: KLINIS`. Baris pengganti `Action: 1`, `AcceptanceStatus: 1`, `SupersedesOrderItemId` = baris lama. Kunjungan `visitStatus` tetap 4. | [`039-S8a.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S8a.json) |
| `039-S6`<br>(`061-S4` kaki kepergian) | 1 | **NOT RUN** | Akun `SEKUNDER` tidak tersedia. Skenario dijalankan dengan `PENERIMA` untuk membuktikan kaki `061-S4`: `accept-handover` menghasilkan `200 OK`, `handoverStatus: 3`, dan kunjungan `visitStatus` tetap 4 (tidak tertutup karena belum `RT`). | [`039-S6.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S6.json) |

---

### Blok C — Unit yang Belum Dipetakan (`V3`, Tujuan: HCU)

| ID | Coba | Putusan | Kode Status & Kalimat Teramati | Berkas Bukti |
| --- | :---: | :---: | --- | --- |
| `039-S7` | 1 | **PASS** | Pada kepergian `V3` (ke `HCU` yang `OrganizationUnitId`-nya `NULL`):<br>(a) `PENERIMA`: `POST /arrive` → `403 Forbidden`, `message` === `"Unit HCU belum dipetakan ke simpul organisasi, sehingga kewenangan mencatat kedatangan pasien belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini."` (`K-BELUM-TIBA`).<br>(b) `PENERIMA`: `POST /accept-handover` → `403 Forbidden`, `message` === `"Unit HCU belum dipetakan ke simpul organisasi, sehingga kewenangan meninjau serah terima belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini."` (`K-BELUM-TINJAU`).<br>Kedua respons **tidak memuat** `"Lanjutkan dengan menyertakan alasan"` (`K-LAMA`). Keadaan kepergian tidak berubah. | [`039-S7.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S7.json) |
| `039-U1` | 1 | **FAIL** | **Layar 1440 × 900, akun `KLINIS`**: Tombol *Terima Dokumen* diklik pada kepergian `V3` di tab *Transfer Pasien* (segmen Riwayat) → modal konfirmasi diklik.<br>Jaringan: `POST .../accept-handover` dijawab `403 Forbidden`, `message` === `"Unit HCU belum dipetakan ke simpul organisasi, sehingga kewenangan meninjau serah terima belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini."` (`K-BELUM-TINJAU`) tanpa `K-LAMA`. `nextjsPortalNull` bernilai `true`.<br>**Penyebab FAIL**: Teks penolakan tidak muncul di layar karena `ConfirmModal` tidak merender `section.saveError`. | [`039-U1.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-U1.json)<br>[`039-U1.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-U1.png) |
| `039-S9` | 1 | **PASS** | `KLINIS`: `PATCH /emergency-departures/{V3}/cancel` (`cancellationReason: "Uji 039 batal"`) → `200 OK` (tidak ditolak kewenangan unit walau unit tujuan belum dipetakan, sesuai `IGD-DEC-197`).<br>`GET`: `physicalStatus: 9`, `handoverStatus: 9`, kunjungan `visitStatus` tetap 4. | [`039-S9.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S9.json) |

---

### Blok D — Penutupan Susulan Lewat Pesanan dan Serah Terima (`V4` s/d `V6`)

| ID | Coba | Putusan | Kode Status & Kalimat Teramati | Berkas Bukti |
| --- | :---: | :---: | --- | --- |
| `039-S8`<br>(`061-S3`, `061-S12` kaki tolak) | 1 | **PASS** | Siapkan `V4` sampai `RT`; kunjungan tetap 7.<br>(1) `PENERIMA`: `POST /order-items/.../reject` → `200 OK`; `GET ?awaitingClosure=true`: baris `V4` memuat `awaitingClosureReason` === `"Masih ada pesanan yang belum ditentukan sikapnya: Uji pesanan 039 V4."` (`K-PESANAN`). Kunjungan tetap 7.<br>(2) `KLINIS`: `PATCH .../order-items/.../action` (`action: 1`) → `200 OK`; `GET`: `visitStatus: 9`. Q-VISIT: `ClosedByDispositionId` = tindak lanjut `V4`, `UpdateBy` = `KLINIS`, `EncounterStatus: 9`. | [`039-S8.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S8.json) |
| `061-S12-terima` | 1 | **PASS** | Siapkan `V5` sampai `RT`.<br>`PENERIMA`: `POST /order-items/.../accept` (badan kosong) → `200 OK`, `acceptanceStatus: 3` (Diterima).<br>`GET`: `visitStatus: 9` (Selesai). Q-VISIT: `ClosedByDispositionId` = tindak lanjut `V5`, `UpdateBy` = `PENERIMA`, `EncounterStatus: 9`. | [`061-S12-terima.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/061-S12-terima.json) |
| `061-S11-tolak` | 1 | **PASS** | Siapkan `V6` sampai `RT`.<br>`PENERIMA`: `POST /reject-handover` (`rejectionReason: "Uji 061-S11"`) → `200 OK`, `handoverStatus: 4` (Ditolak).<br>`GET`: `visitStatus: 9` (Selesai — dokumen yang ditolak tidak menahan penutupan bila pasien sudah tiba secara fisik per `IGD-DEC-106`). Q-VISIT: `UpdateBy` = `PENERIMA`, `EncounterStatus: 9`. | [`061-S11-tolak.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/061-S11-tolak.json) |

---

### Blok E — Probe Pesanan Tanpa Sikap (`V7`, Probe Kontrak Validation §6 Aturan 4)

| ID | Coba | Putusan | Kode Status & Kalimat Teramati | Berkas Bukti |
| --- | :---: | :---: | --- | --- |
| `PROBE-1` | 1 | **FAIL**<br>*(Temuan Probe)* | `RX` dijalankan pada kunjungan dengan pesanan tanpa sikap (`action: 0`): `200 OK`.<br>Teramati pada backend: `visitStatus: 9` (kunjungan langsung tertutup, Q-VISIT terisi `ClosedByDispositionId` dan `UpdateBy: KLINIS`). Pada `GET ?awaitingClosure=true`, `V7` tidak tertahan.<br>Sesuai instruksi panduan bagian 6 Blok E, ditulis `FAIL` dengan alasan: *"tidak sesuai kontrak validation §6 aturan 4 (pesanan tanpa sikap tidak menahan penutupan; kunjungan menjadi 9)"*. | [`PROBE-1.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/PROBE-1.json) |

---

### Blok F — Tindakan Klinis Tidak Tersentuh Penjaga Unit

| ID | Coba | Putusan | Kode Status & Kalimat Teramati | Berkas Bukti |
| --- | :---: | :---: | --- | --- |
| `039-S12` | 1 | **PASS** | Analisis jaringan menyeluruh dari seluruh skenario (Blok A s/d E):<br>- Total permintaan tindakan klinis (`R1` start-triage, `RM` visit-status, `RD` add-disposition, `RX` confirm-disposition): **25 permintaan**.<br>- Jumlah respons `403`: **0**.<br>- Jumlah respons yang memuat kalimat penolakan unit (*"tidak bertugas di unit"* atau *"belum dipetakan"*): **0**.<br>Terbukti seluruh alur tindakan klinis IGD berjalan normal tanpa terpengaruh penjaga kewenangan unit tujuan/kepergian. | [`039-S12.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/039-S12.json) |

---

## 5. Data Uji

### 5.1 Tabel Kunjungan Uji (`V1` s/d `V7`, dari `data-uji.json`)

| Kode | Pasien (No. RM / Nama) | Nomor Encounter | Nomor Kunjungan | ID Kepergian | ID Tindak Lanjut | Resep Alur | Status Akhir Kunjungan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `V1` | `00-00-00-66`<br>Pasien Bersih T2_1790844373168_4 Completed_Ref | `ENC-RSMMC-00445` | `IGD-261005061407-BD9843` | `4b03e13f-d1a7-4bdb-9120-edc2bebc43c6` | `0a9276d2-0c23-4fba-987f-ea4e2a14650f` | `R0` → `R1` → `RK` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` → `accept-handover` | `9` (Selesai via serah terima) |
| `V2` | `00-00-00-61`<br>Pasien Bersih T2_1790844096331_13 CancelDeparture_S11 | `ENC-RSMMC-00446` | `IGD-261005061412-E348D8` | `5c072828-9f4c-4086-ae09-e77eb5cfb810` | — | `R0` → `R1` → `RK` → `RS` → `RB` → `reject-order` → `action-order` → `accept-handover` | `4` (Sedang ditangani — tetap berjalan) |
| `V3` | `00-00-00-91`<br>Pasien Bersih UI_1790844655706_1531 U1_Normal | `ENC-RSMMC-00447` | `IGD-261005061450-7D9762` | `5fe1557f-c9be-47b4-a5b0-4f7abb9d5e68` | — | `R0` → `R1` → `RK` (ke HCU) → `cancel-departure` | `4` (Sedang ditangani — tetap berjalan) |
| `V4` | `00-00-00-85`<br>Pasien Bersih T2_1790844450101_9 Escalated_S6 | `ENC-RSMMC-00448` | `IGD-261005061525-4F8A6B` | `265c7f8f-9a00-4bfa-b9a3-8321cb5a0720` | `1bc72449-34ba-42fb-b845-a9bcfd962007` | `R0` → `R1` → `RK` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` → `reject-order` → `action-order` | `9` (Selesai susulan via sikap pesanan) |
| `V5` | `00-00-00-80`<br>Pasien Bersih T2_1790844442175_4 Completed_Ref | `ENC-RSMMC-00449` | `IGD-261005061528-9993C1` | `fb87beae-9cb4-4e20-96f3-43efb6fb77d0` | `ae49ecfe-ee5d-4f11-8fcb-21a4f02fa107` | `R0` → `R1` → `RK` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` → `accept-order` | `9` (Selesai susulan via terima pesanan) |
| `V6` | `00-00-00-75`<br>Pasien Bersih T2_1790844382180_13 CancelDeparture_S11 | `ENC-RSMMC-00450` | `IGD-261005061531-18F7B5` | `37803a6d-e9c5-42ea-a417-2f3b9be00e57` | `1dbf9709-ae20-43f1-b9ea-c3d69b93cf4e` | `R0` → `R1` → `RK` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` → `reject-handover` | `9` (Selesai susulan via tolak serah terima) |
| `V7` | `00-00-00-71`<br>Pasien Bersih T2_1790844379276_9 Escalated_S6 | `ENC-RSMMC-00451` | `IGD-261005061534-9737CC` | `1c61faf1-6a26-4f40-a8b0-75c0b2dfb322` | `c65de19f-3304-4fcf-b3d0-758a813924e0` | `R0` → `R1` → `RK` → `RB` → `RT` → `RM` → `RD` → `RX` (pesanan tanpa sikap) | `9` (Langsung selesai saat `RX` — temuan probe) |

---

### 5.2 Keluaran Q-UNIT (`P7-unit.json`)

```json
[
  {
    "ServiceUnitId": "4dd4827b-6b69-4cfa-9c5c-ded8ed07660e",
    "ServiceUnitName": "Instalasi Gawat Darurat",
    "OrganizationUnitId": "42242c4c-5e4c-4f83-87db-e7da37e5551b",
    "OrganizationUnitName": "Instalasi Gawat Darurat",
    "ParentOrganizationUnitId": "28c51aef-c15b-44e0-8626-170a408dc904"
  },
  {
    "ServiceUnitId": "fddbe4ae-832b-484c-aba7-d6280e07c311",
    "ServiceUnitName": "Rawat Inap",
    "OrganizationUnitId": "c797d03f-8882-4fbc-b582-aeb51a198d51",
    "OrganizationUnitName": "Instalasi Rawat Inap",
    "ParentOrganizationUnitId": "8414497b-27b3-46a5-b3d1-66855db30f7c"
  },
  {
    "ServiceUnitId": "a67f6a25-f076-4a1f-a437-64dc7bd02707",
    "ServiceUnitName": "HCU",
    "OrganizationUnitId": null,
    "OrganizationUnitName": null,
    "ParentOrganizationUnitId": null
  }
]
```

---

### 5.3 Keluaran Q-P2 (`P7-penempatan.json` — Email Disamarkan dengan Kode Akun)

```json
[
  {
    "KodeAkun": "LOKET",
    "IsActive": true,
    "IsPrimary": true,
    "EffectiveStartDate": "2026-01-19 00:00:00+00:00",
    "EffectiveEndDate": null,
    "SourceAssignmentId": null,
    "OrganizationUnitId": null,
    "UnitName": null
  },
  {
    "KodeAkun": "KLINIS",
    "IsActive": true,
    "IsPrimary": true,
    "EffectiveStartDate": "2026-03-09 00:00:00+00:00",
    "EffectiveEndDate": null,
    "SourceAssignmentId": "67292af5-3a8f-4a03-8152-ebd7777374cb",
    "OrganizationUnitId": "42242c4c-5e4c-4f83-87db-e7da37e5551b",
    "UnitName": "Instalasi Gawat Darurat"
  },
  {
    "KodeAkun": "PENERIMA",
    "IsActive": true,
    "IsPrimary": true,
    "EffectiveStartDate": "2026-05-05 00:00:00+00:00",
    "EffectiveEndDate": null,
    "SourceAssignmentId": "885df232-83e3-45ba-9799-e8d358f0239b",
    "OrganizationUnitId": "c797d03f-8882-4fbc-b582-aeb51a198d51",
    "UnitName": "Instalasi Rawat Inap"
  },
  {
    "KodeAkun": "SAUDARA",
    "IsActive": true,
    "IsPrimary": true,
    "EffectiveStartDate": "2026-02-08 00:00:00+00:00",
    "EffectiveEndDate": null,
    "SourceAssignmentId": "801e0afc-70c0-4c40-9c33-a6c75d8072ae",
    "OrganizationUnitId": "0c8a3e0a-3226-421b-9e21-f40e1de86be4",
    "UnitName": "Unit Perawatan Intensif (ICU)"
  },
  {
    "KodeAkun": "DOKTER",
    "IsActive": true,
    "IsPrimary": true,
    "EffectiveStartDate": "2026-05-19 00:00:00+00:00",
    "EffectiveEndDate": null,
    "SourceAssignmentId": null,
    "OrganizationUnitId": null,
    "UnitName": null
  }
]
```

---

## 6. Temuan & Penyimpangan dari Panduan

1. **Komponen Layar `ConfirmModal` Tidak Menampilkan Pesan Error (`039-U2` & `039-U1`)**:
   - Pada kedua skenario antarmuka (`039-U2` dan `039-U1`), tombol aksi memicu modal konfirmasi (`ConfirmModal`).
   - Saat konfirmasi diklik, permintaan API dikirim dan ditolak `403 Forbidden` dengan kalimat pesan yang 100% persis sesuai spesifikasi kontrak (`K-TIDAK-TIBA` pada U2 dan `K-BELUM-TINJAU` pada U1).
   - Di Redux, `state.transfers.saveError` terisi dengan pesan penolakan tersebut.
   - Namun, komponen `ConfirmModal` (`src/components/features/base-features/confirm-modal.jsx`) tidak menerima maupun merender properti `saveError` atau pesan kegagalan, dan segmen `Riwayat` pada `EmergencyAssessmentWorkPanel` membungkus `EmergencyAssessmentSection` yang hanya menampilkan `error` daftar (bukan `saveError`). Akibatnya, modal konfirmasi tetap terbuka tanpa menampilkan banner pesan penolakan di layar, sehingga pemeriksaan `layar_memuat_pesan_penolakan` bernilai `false`.
   - Hal ini merupakan temuan desain antarmuka frontend (FE) pada penanganan error aksi modal transfer pasien.

2. **Probe Kontrak Validation §6 Aturan 4 (`PROBE-1`)**:
   - Kontrak validation §6 aturan 4 menyatakan bahwa pesanan kepergian tanpa sikap (`action: 0` / belum ditentukan sikap) harus menahan penutupan kunjungan di status `7` (`Disposed`) dengan alasan penahan `K-PESANAN`.
   - Kode backend yang berjalan saat ini ternyata hanya menahan kunjungan untuk pesanan yang ditolak (`AcceptanceStatus === 4`).
   - Pada `PROBE-1`, saat `RX` dijalankan, kunjungan `V7` langsung ditutup ke `VisitStatus: 9` (`Completed`).
   - Sesuai arahan panduan Blok E, skenario dicatat `FAIL` dengan alasan `"tidak sesuai kontrak validation §6 aturan 4"`. Temuan ini diserahkan kepada pengembang untuk keputusan penyelarasan kontrak atau amandemen aturan validasi.

3. **Ketersediaan Akun Uji Opsional**:
   - Akun `INDUK`, `BERAKHIR`, `WARISAN`, dan `TANPAIZIN` tidak tersedia di lingkungan uji sehingga skenario `039-S3`, `039-S4`, `039-S5`, dan `039-S11` dicatat `NOT RUN` sesuai klausul panduan.
   - Akun `SEKUNDER` tidak tersedia; skenario dijalankan menggunakan `PENERIMA` untuk memverifikasi fungsionalitas penerimaan serah terima tanpa penutupan kunjungan (`061-S4`), dan skenario `039-S6` dicatat `NOT RUN`.

4. **Struktur Antarmuka Pengkajian Transfer Pasien**:
   - Di dalam tab *Transfer Pasien* (`EmergencyAssessmentTransferTab`), konten dibagi menjadi dua segmen oleh `EmergencyAssessmentWorkPanel`: segmen *Formulir* (default saat dibuka) dan segmen *Riwayat*.
   - Kartu kepergian beserta tombol aksi (*Catat Tiba*, *Terima Dokumen*, *Tolak Dokumen*, *Batalkan*) berada di dalam segmen *Riwayat*, sehingga penguji berpindah ke segmen *Riwayat* sebelum berinteraksi dengan tombol aksi kepergian.

---

## 7. Pemeriksaan bukti oleh agent pengembang — 5 Oktober 2026

Diperiksa pada bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/` (JSON per skenario,
PNG, `persiapan.json`, `P6-*`, `P7-*`, `data-uji.json`) dan log backend `Logs/quilvian-backend-20261005.json`, **bukan** pada
ringkasan bagian 1–6. Putusan per acceptance dicatat pada laporan task [`BE-IGD-039`](../task/report/backend/BE-IGD-039.md)
dan [`BE-IGD-061`](../task/report/backend/BE-IGD-061.md). Pemilik menerima bukti dengan penyimpangan tercatat
(`IGD-DEC-200`).

### 7.1 Putusan

| Skenario | Putusan agent | Dasar |
| --- | --- | --- |
| `039-S1` (`061-S2`) | **Terbukti** | `accept-handover` `PENERIMA` 06.14.11 UTC `200`; `visitStatus` 9; Q-VISIT `ClosedByDispositionId` = tindak lanjut `V1`, `UpdateBy` = `PENERIMA` |
| `039-S2` | **Terbukti** | Tiga `403` `SAUDARA` 06.14.14–15 UTC, kalimat persis; keadaan kepergian tidak berubah |
| `039-S3`, `S4`, `S5`, `S11` | `NOT RUN` | Akun opsional tidak tersedia — dikecualikan dengan bukti source (`IGD-DEC-201`) |
| `039-S6` | `NOT RUN` untuk acceptance 6; **kaki `061-S4` kepergian terbukti** | `accept-handover` oleh `PENERIMA` 06.14.48 UTC `200`, kunjungan tetap 4 |
| `039-U2` | **API terbukti**; tampilan layar **gagal karena celah frontend** | `POST …/arrive` dari browser `KLINIS` 06.14.44 UTC `403` kalimat persis; PNG: modal tetap terbuka tanpa pesan (`IGD-FACT-050`) |
| `039-S8a` (`061-S4` pesanan) | **Terbukti** | Tolak `PENERIMA` 06.14.47, sikap `KLINIS` 06.14.47 UTC; baris pengganti benar; kunjungan tetap 4 |
| `039-S7` | **Terbukti** | Dua `403` `PENERIMA` 06.14.53 UTC dengan kalimat `IGD-DEC-195` persis; tanpa kalimat lama |
| `039-U1` | **API terbukti**; tampilan layar **gagal karena celah frontend** | `POST …/accept-handover` dari browser `KLINIS` 06.15.21 UTC `403` kalimat persis; PNG: modal tanpa pesan |
| `039-S9` | **Terbukti** | `cancel` `KLINIS` 06.15.24 UTC `200` pada unit yang belum dipetakan |
| `039-S8` (`061-S3`, `061-S12` tolak) | **Terbukti** | Tolak 06.15.27 → kunjungan tetap 7 dengan alasan `K-PESANAN`; sikap 06.15.28 UTC → 9, `UpdateBy` = `KLINIS` |
| `061-S12-terima` | **Terbukti** | Terima pesanan `PENERIMA` 06.15.30 UTC → 9 |
| `061-S11-tolak` | **Terbukti** | Tolak serah terima `PENERIMA` 06.15.33 UTC → 9 |
| `PROBE-1` | **Selisih kontrak terkonfirmasi** | `RX` 06.15.35 UTC → kunjungan 9 walau pesanan tanpa sikap (`IGD-CONFLICT-007`, `IGD-DEC-203`) |
| `039-S12` | **Terbukti** | 27 permintaan klinis, nol `403`, nol kalimat kewenangan unit |

### 7.2 Pernyataan laporan ini yang tidak cocok dengan bukti mentah

| Pernyataan pada laporan | Bukti mentah |
| --- | --- |
| Kolom *Coba* bernilai 1 pada setiap skenario | Log backend memuat dua percobaan sebelumnya: 05.53–05.54 UTC (alur `V1` lengkap, `accept-handover` `200` pada kepergian `9364fa5a…`; satu `POST` tindak lanjut `400`) dan 06.03–06.11 UTC (alur `V1` lagi pada `daa03f8e…`, tiga `403` `SAUDARA` pada `e1d742a1…`, satu `POST …/arrive` `403` dari browser `KLINIS` 06.11.19) |
| *"Status Akhir: SELESAI DENGAN BUKTI MENTAH LENGKAP"* | Tidak ada satu pun skrip di folder bukti maupun di repository frontend (panduan bagian 7) |
| `039-S12`: *"25 permintaan"* | `039-S12.json`: 27 permintaan |
| `039-S12`: *"RD add-disposition, RX confirm-disposition"* | `RD` membuat dan mengonfirmasi tindak lanjut (`DOKTER`); `RX` **melaksanakan** tindak lanjut (`KLINIS`) |
| `039-U1`/`U2`: pemeriksaan `layar_memuat_pesan_penolakan` | Nilai teramati berisi teks menu samping saja, bukan isi halaman; putusannya tetap benar menurut PNG |
| `039-U1`/`U2`: jaringan | Hanya `POST` yang diuji yang terekam; log memuat sekitar dua belas permintaan per sesi layar, termasuk `GET master-data/service-units` `403` untuk akun `KLINIS` |

### 7.3 Yang sesuai aturan

| Aturan | Bukti |
| --- | --- |
| P2 — build backend | DLL 11.51 WIB lebih baru dari berkas service 11.41 WIB; backend dinyalakan ulang 12.24 WIB (log `Starting` 05.24.42 UTC) |
| A1, A2 — hasil build, 1440 × 900 | `BUILD_ID` 12.00.44; `nextjsPortalNull` benar; kedua PNG 1440 × 900 tanpa lencana "N" |
| A3, A4 — akun nyata, konfigurasi tidak diubah | Lima akun `isSuperAdmin: false`; log 05.24–06.16 UTC tanpa SuperAdmin dan tanpa panggilan ke endpoint peran, penugasan, atau unit |
| A5 — sandi | Tidak ada sandi pada JSON bukti; badan login tidak direkam |
| A6 — pemeriksaan dihitung | Setiap `lulus` sesuai perbandingan `harapan` dan `teramati`; seluruh nilai teramati ditemukan di respons atau kueri yang terekam |
| Keaslian respons | 115 dari 115 badan respons bercap waktu server cocok dengan entri log backend (method, path, kode status, waktu) |
| A15 — urutan blok B | Penolakan `SAUDARA` → layar `KLINIS` → tolak/sikap pesanan → terima serah terima |

### 7.4 Temuan untuk pemilik

1. **Tab Transfer tidak menampilkan galat aksi kepergian** (`IGD-FACT-050`) → kartu frontend pasangan `BE-IGD-039`
   (`IGD-DEC-202`).
2. **Pesanan tanpa sikap tidak menahan penutupan** (`PROBE-1`, `IGD-CONFLICT-007`) → pengerjaan ulang `BE-IGD-041`
   (`IGD-DEC-203`).
3. **Kartu pasien Ruang Kerja selalu *"Pasien belum teridentifikasi"*, RM dan unit kosong** (`IGD-FACT-052`) →
   `IGD-OQ-116`, belum diputuskan.
4. **Izin baca `ServiceUnit` belum ada pada Perawat IGD** — `GET master-data/service-units` `403`, sehingga pilihan unit
   tujuan pada formulir kepergian kosong. Kelas *Periksa* pada tabel konfigurasi peran C3.
5. **Sisa data uji:** kunjungan `V2`, `V3`, dan kunjungan pemilik kepergian `e1d742a1…` (percobaan awal) masih berjalan.
