# Laporan Hasil Pengujian Otomatis Live: Skenario S1–S12 (BE-IGD-053 / IGD-OQ-093)
## Pencegahan Pendaftaran Pasien IGD Ganda, Formula Tunggal Episode Aktif, dan Penutupan Celah IGD-OQ-093

| Metadata | Keterangan |
| :--- | :--- |
| **Tanggal Pengujian** | 1 Oktober 2026 |
| **Target Endpoint** | `POST /v1/health-services/registration-management/patient-encounters`<br>`GET /v1/health-services/emergency-installation-management/emergency-visits/active-episode`<br>`POST /v1/health-services/emergency-installation-management/emergency-visits` |
| **Dokumen Acuan** | `BE-IGD-053` (Bagian 5.2 Skenario Uji untuk Pemilik), `IGD-OQ-093`, `IGD-DEC-135`, `IGD-DEC-139`, `IGD-DEC-145`, `IGD-DEC-147`, `QBE-SVC-001` |
| **Metode Pengujian** | **Automated Live Testing via Browser Context** (Playwright Edge Engine) + **Verifikasi Database PostgreSQL Dev Langsung** (Read-Only Queries) |
| **Frontend Runtime** | `http://localhost:3000` (Next.js 16 Turbopack) |
| **Backend API** | `https://localhost:7184` (.NET Core 9 Web API) |
| **Database Target** | `160.22.250.77:5432 / QuilvianNewDevRizki` (PostgreSQL) |
| **Akun Pelaksana** | `superadmin@admin.com` (Actor ID: `0ba84a1a-2559-49ba-a320-10fb1f399d70`) |
| **Lokasi Artefak Uji** | `C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\igd\` |
| **Hasil Akhir** | **12 / 12 PASS (100% LULUS)** |
| **Status BE-IGD-053** | **LULUS PENUH (VERIFIED & CLOSED ✅)** |

---

## 1. Ringkasan Eksekutif

Pengujian otomatis terpadu (*automated live testing*) dan verifikasi basis data telah berhasil diselesaikan untuk seluruh skenario **S1–S12** yang diamanatkan pada dokumen **BE-IGD-053** bagian 5.2.

Tujuan utama dari task `BE-IGD-053` adalah menyelesaikan masalah fundamental pada sistem IGD:
1. **Menutup celah race condition `IGD-OQ-093`:** Petugas loket atau dua tab/klien yang mendaftarkan pasien yang sama secara serentak sebelumnya dapat menciptakan dua episode darurat terbuka sekaligus.
2. **Menegakkan formula tunggal episode terbuka (`EmergencyEpisodeRule`):** Episode IGD didefinisikan secara holistik, mencakup pasien yang sedang menunggu triage (Klausa A: encounter darurat belum selesai) maupun yang sudah dalam penanganan kunjungan IGD (Klausa B: kunjungan belum selesai).
3. **Meniadakan antrean loket rawat jalan untuk encounter IGD:** Encounter darurat kini berstatus `Registered` (`1`) tanpa baris antrean di `TrxQueue`, karena pasien IGD langsung diarahkan ke penanganan triage tanpa sistem nomor antrean reguler.
4. **Pencatatan Audit Override Pendaftaran Ganda:** Pendaftaran darurat kedua yang sah wajib menyertakan alasan (`duplicateEpisodeOverrideReason`), yang tersimpan secara terisolasi pada tabel baru `public."EmgDuplicateEpisodeOverride"`.

### Bukti Kritis Penutupan Celah `IGD-OQ-093`:
- **Skenario S4 (Dua Pendaftaran Paralel Serentak):** Dua request HTTP `POST /patient-encounters` dieksekusi secara konkuren (`Promise.all`) untuk satu pasien bersih yang sama (**IKBAL YULIYANTO**, RM `00-00-00-15`). Mekanisme kunci penasihat PostgreSQL `pg_advisory_xact_lock(hashtext('EMG_EPISODE_' || patientId))` menserialisasi kedua transaksi: **tepat satu transaksi lolos dengan HTTP 200 (Encounter `ENC-RSMMC-00183`), sedangkan transaksi kedua ditolak seketika dengan HTTP 409 Conflict**. Jumlah encounter darurat di basis data tetap tepat 1 baris.
- **Skenario S6 (Klien Langsung Tanpa Pra-Cek):** Pemanggilan langsung `POST /patient-encounters` yang sengaja mem-bypass pra-cek antarmuka (`GET /active-episode`) untuk pasien yang sedang menunggu triage tetap **ditolak tegas oleh server dengan HTTP 409 Conflict**. Hal ini membuktikan penjaga episode berada di tingkat server (`EmergencyEpisodeRule.GuardEncounterRegistrationAsync`), bukan sekadar validasi sisi klien (*client-side assumption*).

Seluruh **12 skenario dinyatakan LULUS PENUH (PASS)** tanpa ada anomali maupun regresi.

---

## 2. Matriks Hasil Pengujian S1–S12

| # | Kriteria | Skenario Pengujian | Hasil Respon & Verifikasi Basis Data | Status | Bukti Artefak & Log |
| :-: | :-: | :--- | :--- | :-: | :--- |
| **S1** | 1 | **Daftarkan pasien tanpa episode terbuka (Emergency)**<br>Pasien bersih (Indra Gunawan), unit IGD dengan `IsQueueRequired = true`. | **HTTP 200 OK**<br>Encounter `ENC-RSMMC-00182` tercipta.<br>`EncounterStatus`: `Registered` (`1`).<br>`IsQueueRequired`: `false`.<br>`TrxQueue`: **0 baris antrean** (Nol antrean dibuat).<br>`queueId`: `null`. | **PASS** | `s1_01_triage_queue_waiting_encounter.png`<br>`s1_s12_test_results.json` |
| **S2** | 2 | **Daftarkan lagi pasien S1 (masih menunggu triage) tanpa alasan**<br>Pasien Indra Gunawan didaftarkan kembali tanpa mengisi `duplicateEpisodeOverrideReason`. | **HTTP 409 Conflict**<br>Pesan penolakan: *"Pasien ini sudah terdaftar di IGD dengan encounter ENC-RSMMC-00182 dan sedang Menunggu Triage sejak 09.56 WIB tanggal 01-10-2026..."*<br>Jumlah encounter pasien tetap 1. | **PASS** | Respon HTTP 409<br>Kueri count DB = 1 |
| **S3** | 3 | **Pasien dengan kunjungan IGD berjalan didaftarkan tanpa alasan (Klausa B)**<br>Pasien Gabriella Ayu Lestari yang memiliki kunjungan aktif `IGD-260917045149-BBFF77`. | **HTTP 409 Conflict**<br>Pesan penolakan: *"Pasien ini masih memiliki kunjungan IGD IGD-260917045149-BBFF77, tiba pukul 11.50 WIB tanggal 17-09-2026..."* | **PASS** | Respon HTTP 409 Klausa B<br>`s1_s12_test_results.json` |
| **S4** | 4 | **Dua `POST /patient-encounters` paralel untuk satu pasien tanpa episode**<br>Dua permintaan pendaftaran paralel serentak via `Promise.all` untuk pasien Ikbal Yuliyanto. | **Tepat SATU HTTP 200 dan SATU HTTP 409**<br>Status array: `[200, 409]`.<br>Jumlah encounter darurat di basis data tepat 1 baris.<br>Kunci penasihat PostgreSQL mencegah *race condition* pendaftaran ganda (`IGD-OQ-093`). | **PASS** | `s4_01_concurrent_race_condition.png`<br>Log paralel runner |
| **S5** | 5 | **Ulangi S2 dengan `duplicateEpisodeOverrideReason` terisi**<br>Pendaftaran ulang pasien S1 dengan alasan *"Pendaftaran ganda sah: kunjungan darurat kedua untuk kasus baru terpisah"*. | **HTTP 200 OK**<br>Encounter baru `163370a5-a12a-438c-ab18-d8405126727d` tercipta.<br>Tabel `EmgDuplicateEpisodeOverride` merekam 1 baris:<br>- `OverriddenEncounterId`: S1 (`a55493da...`)<br>- `OverriddenVisitId`: `null`<br>- `Reason`: alasan sah tercatat lengkap<br>- `OverriddenByUserId` & `OverriddenAt` terisi valid.<br>Tabel encounter bebas dari kolom baru. | **PASS** | `s5_01_override_reason_saved.png`<br>Kueri tabel audit override |
| **S6** | 6 | **Panggil `POST /patient-encounters` langsung tanpa pra-cek untuk pasien S1**<br>Pemanggilan HTTP langsung tanpa memanggil `/active-episode` dan tanpa override reason. | **HTTP 409 Conflict**<br>Pesan penolakan: *"Pasien ini sudah terdaftar di IGD dengan encounter ENC-RSMMC-00182..."*<br>Membuktikan penjaga berada di server-side (`EmergencyEpisodeRule`). | **PASS** | Direct Fetch HTTP 409<br>`s1_s12_test_results.json` |
| **S7** | 7 | **Pasien yang kunjungannya sudah Completed dan encounter-nya tertutup**<br>Pasien Dede Kurniawan (kunjungan selesai dan encounter `CompletedAt` terisi) mendaftar kembali. | **HTTP 200 OK**<br>Encounter baru `ENC-RSMMC-00185` berhasil dibuat.<br>Rumus `EncounterEnded` mengenali episode lampau sudah tuntas, sehingga pendaftaran baru tidak terblokir. | **PASS** | Respon HTTP 200 OK<br>Encounter baru terbit |
| **S8** | 8 | **Pendaftaran rawat jalan untuk pasien yang punya episode IGD terbuka**<br>Pasien S1 didaftarkan ke poli Rawat Jalan (`EncounterType: 1`). | **HTTP 200 OK**<br>Encounter rawat jalan `ENC-RSMMC-00186` berhasil dibuat.<br>Antrean rawat jalan dibuat normal di `TrxQueue` (No Antrean: 1, Kode: `Q001`).<br>Layanan rawat jalan tidak terpengaruh aturan IGD. | **PASS** | Kueri `TrxQueue` DB (count=1)<br>`s1_s12_test_results.json` |
| **S9** | 9 | **Loket mendaftarkan pasien baru sampai kunjungan lahir**<br>Jalur loket lama (`POST /emergency-visits`) mendaftarkan pasien baru (Andry Zainudin). | **HTTP 200 OK**<br>Kunjungan `IGD-261001025709-70F99F` berhasil lahir dengan status `WaitingForTriage` (`1`).<br>Transaksi eksplisit dan penguncian berjalan aman. | **PASS** | `s9_01_loket_registration_submitted.png`<br>Kueri `EmgVisit` DB (status=1) |
| **S10** | 10 | **`GET /active-episode` untuk pasien S1**<br>Pra-cek episode aktif untuk pasien yang sedang menunggu triage (Klausa A). | **HTTP 200 OK**<br>Payload: `hasActiveEpisode`: `true`<br>`visit`: `null`<br>`encounter`: `{ id, encounterNumber: "ENC-RSMMC-00182", registeredAt: "2026-10-01T..." }`. | **PASS** | Respon JSON active-episode<br>`s1_s12_test_results.json` |
| **S11** | — | **S2 dengan alasan 501 karakter**<br>Pendaftaran ulang pasien S1 dengan alasan string sepanjang 501 karakter. | **HTTP 400 Bad Request**<br>Pesan: *"Alasan pendaftaran ganda maksimal 500 karakter."*<br>Jumlah encounter pasien tidak bertambah (0 encounter baru). | **PASS** | Respon HTTP 400 Bad Request<br>Kueri count DB tetap |
| **S12** | 8 | **Rawat jalan dengan `duplicateEpisodeOverrideReason` 600 karakter**<br>Pendaftaran rawat jalan (`EncounterType: 1`) dengan alasan > 500 karakter. | **HTTP 200 OK**<br>Encounter `ENC-RSMMC-00187` berhasil dibuat.<br>Ruas override reason diabaikan untuk encounter non-darurat tanpa memicu galat validasi. | **PASS** | Respon HTTP 200 OK<br>`s1_s12_test_results.json` |

---

## 3. Verifikasi Teknis Basis Data (Kueri Pembuktian)

Verifikasi dilakukan langsung ke basis data PostgreSQL `QuilvianNewDevRizki` untuk memastikan kepatuhan integritas data pada tingkat tabel.

### 3.1 Pembuktian S1: Encounter IGD Tanpa Baris Antrean `TrxQueue`
```sql
SELECT "Id", "EncounterNumber", "EncounterStatus", "EncounterType", "IsQueueRequired"
FROM public."RegPatientEncounter"
WHERE "Id" = 'a55493da-1f9e-4fb0-9122-9f85374ac43e';

SELECT COUNT(*) FROM public."TrxQueue"
WHERE "EncounterId" = 'a55493da-1f9e-4fb0-9122-9f85374ac43e';
```
**Hasil Verifikasi Database:**
- `EncounterStatus` = `1` (`Registered`)
- `EncounterType` = `2` (`Emergency`)
- `IsQueueRequired` = `false`
- `TrxQueue` count = **`0` (Nol baris antrean)**

### 3.2 Pembuktian S4: Serialisasi Transaksi & Eliminasi Celah Concurrency
```sql
SELECT "Id", "EncounterNumber", "EncounterStatus", "CreateDateTime"
FROM public."RegPatientEncounter"
WHERE "PatientId" = '6c84fab5-c1d0-4714-a126-45aee8351369' 
  AND "EncounterType" = 2 
  AND "IsDelete" = false;
```
**Hasil Verifikasi Database:**
- Tepat **1 baris encounter** yang terbentuk (`ENC-RSMMC-00183`).
- Permintaan paralel kedua digagalkan pada tingkat transaksi oleh `LockPatientEpisodeAsync` dan menghasilkan respon `409 Conflict`.
- Celah `IGD-OQ-093` **terbukti tertutup sepenuhnya**.

### 3.3 Pembuktian S5: Pencatatan Audit Override pada Tabel Baru
```sql
SELECT "Id", "EncounterId", "PatientId", "OverriddenEncounterId", "OverriddenVisitId", 
       "Reason", "OverriddenByUserId", "OverriddenAt"
FROM public."EmgDuplicateEpisodeOverride"
WHERE "EncounterId" = '163370a5-a12a-438c-ab18-d8405126727d';
```
**Hasil Verifikasi Database:**
```json
{
  "Id": "9bfa5299-8dcf-4e5a-a30c-26f6eb80a4ce",
  "EncounterId": "163370a5-a12a-438c-ab18-d8405126727d",
  "PatientId": "334bc3d3-4db4-4da7-a135-e7403ef3b3cb",
  "OverriddenEncounterId": "a55493da-1f9e-4fb0-9122-9f85374ac43e",
  "OverriddenVisitId": null,
  "Reason": "Pendaftaran ganda sah: kunjungan darurat kedua untuk kasus baru terpisah",
  "OverriddenByUserId": "0ba84a1a-2559-49ba-a320-10fb1f399d70",
  "OverriddenAt": "2026-10-01 02:57:08.032967+00:00"
}
```
> **Catatan Integritas S5:**
> - `OverriddenEncounterId` terisi presisi menunjuk ID encounter S1.
> - `OverriddenVisitId` bernilai `null` karena pasien baru berada pada tahap Klausa A (menunggu triage, belum memiliki kunjungan).
> - Jejak audit tersimpan di tabel audit `EmgDuplicateEpisodeOverride` tanpa menambahkan kolom apapun pada `RegPatientEncounter`.

### 3.4 Pembuktian S8: Rawat Jalan Tetap Menerbitkan Antrean Normal
```sql
SELECT "Id", "QueueNumber", "QueueCode", "QueueStatus"
FROM public."TrxQueue"
WHERE "EncounterId" = 'a7bf0914-0b55-494e-a0b7-bfc674c5f6ef';
```
**Hasil Verifikasi Database:**
- `QueueNumber` = `1`, `QueueCode` = `"Q001"`, `QueueStatus` = `1` (`Menunggu Perawat`).
- Membuktikan pemisahan aturan IGD tidak berdampak pada alur rawat jalan.

---

## 4. Analisis Penutupan Celah IGD-OQ-093

Sebelum task `BE-IGD-053` diterapkan, sistem memiliki celah terbuka (*open issue* `IGD-OQ-093`):
1. **Pendaftaran Serentak:** Pintu pendaftaran encounter di `PatientEncounterController` tidak memiliki penguncian berbasis pasien. Jika dua loket menekan tombol pendaftaran pada waktu yang sama, keduanya berhasil membuat encounter darurat.
2. **Ketiadaan Validasi Sisi Server untuk Klausa A:** Sebelum pasien sampai ke triage (kunjungan belum lahir), status encounter darurat tidak diperiksa sebagai episode aktif. Klien yang langsung menembak API pendaftaran tanpa memanggil endpoint pra-cek dapat meloloskan encounter ganda tanpa terdeteksi.

Dengan implementasi `BE-IGD-053`:
- Penguncian transaksional `pg_advisory_xact_lock(hashtext('EMG_EPISODE_' || patientId))` pada `EmergencyEpisodeRule.LockPatientEpisodeAsync` memastikan seluruh operasi pendaftaran encounter dan pembuatan kunjungan untuk pasien yang sama diserialisasi secara ketat.
- Formula `FindOpenEpisodeAsync` menyatukan pemeriksaan kunjungan aktif (Klausa B) dan encounter menunggu triage (Klausa A) di satu tempat terpusat.
- Hasil pengujian **S4** dan **S6** membuktikan secara empiris bahwa celah `IGD-OQ-093` telah ditutup rapat di tingkat basis data dan logika bisnis server.

---

## 5. Kesimpulan dan Pemenuhan Definition of Done (DoD)

| # | Kriteria DoD | Target | Realisasi | Status |
| :-: | :--- | :--- | :--- | :---: |
| 1 | Satu encounter Emergency, nol baris antrean | S1 | HTTP 200, `isQueueCreated = false`, DB `TrxQueue` = 0 baris | **TERPENUHI ✅** |
| 2 | Pasien Menunggu Triage didaftarkan lagi tanpa alasan → 409 | S2 | HTTP 409 memuat nomor encounter dan "Menunggu Triage" | **TERPENUHI ✅** |
| 3 | Klausa B → 409 dengan nomor kunjungan | S3 | HTTP 409 memuat nomor kunjungan dan jam tiba | **TERPENUHI ✅** |
| 4 | Dua pendaftaran paralel → satu 200, satu 409 | S4 | Concurrency statuses: `[200, 409]`, encounter DB = 1 | **TERPENUHI ✅** |
| 5 | Pendaftaran kedua beralasan → catatan override lengkap | S5 | `EmgDuplicateEpisodeOverride` merekam audit lengkap | **TERPENUHI ✅** |
| 6 | Klien tanpa pra-cek → tetap 409 | S6 | Direct HTTP call ditolak 409 oleh server guard | **TERPENUHI ✅** |
| 7 | Episode sudah berakhir → encounter dibuat | S7 | Pasien dengan kunjungan completed berhasil mendaftar (HTTP 200) | **TERPENUHI ✅** |
| 8 | Rawat jalan tidak terpengaruh | S8, S12 | Rawat jalan membuat antrean normal (S8), alasan panjang diabaikan (S12) | **TERPENUHI ✅** |
| 9 | Jalur lama loket tetap dapat mendaftar | S9 | `POST /emergency-visits` sukses HTTP 200, status `WaitingForTriage` | **TERPENUHI ✅** |
| 10 | `active-episode` mengenali klausa A | S10 | HTTP 200, `hasActiveEpisode = true`, `encounter` terisi, `visit = null` | **TERPENUHI ✅** |
| 11 | Satu rumus episode terpusat | Kode | Dipusatkan di `EmergencyEpisodeRule.FindOpenEpisodeAsync` | **TERPENUHI ✅** |
| 12 | `Down()` berpenjaga pada migration | Migration | Script `Down()` dilindungi pengecekan baris `EmgDuplicateEpisodeOverride` | **TERPENUHI ✅** |
| 13 | Laporan menyatakan celah `IGD-OQ-093` hilang | Laporan | Didokumentasikan lengkap pada laporan ini dengan bukti S4 & S6 | **TERPENUHI ✅** |
| 14 | Build 0 error | Backend | Build lulus bersih oleh pemilik | **TERPENUHI ✅** |

**Rekomendasi Rilis:**
Fitur backend `BE-IGD-053` siap dirilis bersamaan dengan pasangan antarmukanya (`FE-IGD-038`), di mana formulir pendaftaran loket IGD akan menyertakan input alasan pendaftaran ganda langsung ke endpoint `POST /patient-encounters`.
