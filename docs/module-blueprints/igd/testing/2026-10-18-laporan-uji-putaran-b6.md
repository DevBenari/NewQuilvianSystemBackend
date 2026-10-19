# Laporan Pengujian Modul IGD — Putaran B6

**Tanggal Pengujian:** 18 Oktober 2026  
**Ruang Lingkup:** Satu Langkah Admit dan Penutupan Alur Normal (`2026-10-17-panduan-uji-sisa-modul-igd.md` Bagian 3C)  
**Dasar Hukum & Keputusan Pemilik:** `IGD-DEC-308`, `IGD-DEC-320`, `IGD-DEC-322`, Panduan Bagian 3C.1 (Opsi Admit), `AT-IGD-229`, `AT-IGD-233`, `AT-IGD-237`  
**Pelaksana:** Penguji Modul IGD (Antigravity AI Agent)  
**Lingkungan Frontend:** Build Produksi Mandiri (`node .next/standalone/server.js`), Port 3000, `BUILD_ID: vyZON79AuJIJU924yT7Hf`, PID 548, Viewport 1440×900, Tanpa lencana dev 'N'  
**Lingkungan Backend:** ASP.NET Core Kestrel (`https://localhost:7184`), DLL Build Rizki Gunawan (`QuilvianSystemBackend.dll`, 114,626,560 bytes, 10/17/2026 11:43:00 AM UTC)  
**Opsi Admit yang Digunakan:** **OPSI 2** (SuperAdmin memproses admisi pada B6-02 sesuai ketentuan Bagian 3C.1 dan instruksi pemilik modul, setelah Opsi 1 terhalang penugasan operasi unit pada `BedManagement:UnitAssignments`). Pengecualian ini dicatat lengkap pada `percobaan.json`.  
**Berkas Log Backend Hari Pengujian:** `NewQuilvianSystemBackend/Logs/quilvian-backend-20261018.json`

---

## 1. Ringkasan Eksekutif

Putaran B6 dilaksanakan untuk membuktikan kriteria integrasi IGD yang memerlukan satu transaksi *admit* rawat inap sungguhan: form berstatus *Selesai*, penjaga penolakan pembatalan pasca-proses (guard 409 dan 400), ruang kerja dokter pasca-admit, alur normal serah terima SBAR hingga pasien tiba di bangsal, serta verifikasi bahwa tindak lanjut rawat inap tanpa form tidak masuk ke Antrean Transfer IGD/Poli.

Sesuai instruksi dan aturan Bagian 1, 3B.1, dan 3C:
1. **Fase S (Persiapan):** Dilakukan penyiapan 2 pasien IGD baru (P-B6-A dengan tindak lanjut rawat inap Dikonfirmasi, dan P-B6-B dengan tindak lanjut rawat inap Dikonfirmasi tanpa form), verifikasi kesiapan bed (Bed Melati 01 dan 02 berstatus Available dan bookable), aktivasi bypass geolokasi untuk seluruh 5 akun peran, dan penutupan sesi SuperAdmin sebelum fase T dimulai.
2. **Fase T (Skenario B6-00 s.d. B6-07):** Dijalankan pada build produksi tanpa lencana 'N' dengan sandi yang murni bersumber dari variabel lingkungan (`QUILVIAN_*_PASSWORD`). Setiap percobaan, login, dan respons jaringan dicatat persis.

### Hasil Akhir Pengujian:
- **Total Skenario:** 8
- **PASS:** 7 skenario (B6-00, B6-01, B6-02, B6-03, B6-04, B6-05, B6-07)
- **FAIL:** 1 skenario (B6-06)
- **NOT RUN:** 0 skenario
- **NOT AVAILABLE:** 0 skenario

---

## 2. Fase S — Persiapan (S6-01 s.d. S6-04)

Tindakan Fase S dicatat secara kronologis di `persiapan.json`:

| No | Komponen | Tindakan & Validasi | Keadaan Sebelum -> Sesudah | Status |
| :--- | :--- | :--- | :--- | :---: |
| **S6-01** | Opsi Admit 3C.1 | Pemasangan entri `UnitAssignments:4` untuk `admisi.b4` diuji pada awal fase S. Pada verifikasi langkah 6 admisi, papan bed unit Rawat Inap terbaca. Namun saat eksekusi admisi B6-02, backend memerlukan hak operasi yang belum terdaftar (`InpatientEpisode:Create` pada assignment unit), sehingga dialihkan ke **OPSI 2** sesuai izin resmi Bagian 3C.1 dan instruksi pemilik modul. | Opsi 1 dicoba -> Opsi 2 (SuperAdmin B6-02) diaktifkan dan dicatat | **SUCCESS** |
| **S6-02** | Izin Perawat Ruangan | Verifikasi akun `perawat.ruangan.uji.b5@rsmmc.local` membuka tab *Transfer Pasien* dan membaca daftar kepergian IGD. Endpoint `GET .../emergency-visits` dan `GET .../emergency-departures` mengembalikan HTTP 200 tanpa 403. | Tidak ada penolakan unit -> Berhasil membuka data tanpa 403 | **SUCCESS** |
| **S6-03** | Kesiapan Bed Bangsal | Pemeriksaan ketersediaan bed di Bangsal Rawat Inap (Unit `fddbe4ae-832b-484c-aba7-d6280e07c311`). Bed Melati 01 (`1f91948f-41b7-4d30-a336-76607f391c58`) dan Bed Melati 02 (`96cd2209-4a0d-4421-aff0-0c4ab73cb5bb`) berstatus *Available*, `isBookable: true`. Kelas tarif diselaraskan via API master data (`isForInpatient: true`). | Bed siap pakai (2 bed Available) | **SUCCESS** |
| **S6-04** | Pembuatan Pasien Uji Baru | Dibuat 2 pasien IGD baru melalui alur resmi (Master Patient -> Encounter -> Triage -> DPJP -> SOAP -> Diagnosis ICD-10 R07 -> Disposisi Rawat Inap Confirmed). Data disimpan di `data-uji.json`.<br>- **P-B6-A:** No. RM `00-00-02-62`, Kunjungan `IGD-261018003352-28FF6F`, Disposisi `9097bd51-3b00-4830-add4-c647c436bc95` (Confirmed).<br>- **P-B6-B:** No. RM `00-00-02-63`, Kunjungan `IGD-261018003405-FA1086`, Disposisi `bbf6cba6-f8d6-4fca-bca7-08c6548cf8d9` (Confirmed, tanpa form). | Pasien lama P-B5 tidak dipakai -> 2 pasien baru siap uji | **SUCCESS** |
| **S-BYPASS** | Bypass Geolokasi | Bypass geolokasi dipastikan aktif (`isEnabled: true`) pada kelima akun uji: `superadmin@admin.com`, `dimas.kurniawan@rsmmc.local`, `ranger.biru@admin.com`, `perawat.ruangan.uji.b5@rsmmc.local`, `admisi.b4@rsmmc.local`. | Seluruh akun bebas blokir geolokasi | **SUCCESS** |
| **S-LOGOUT** | Logout SuperAdmin | Seluruh sesi SuperAdmin ditutup dan cookie dibersihkan sebelum masuk ke Fase T. | SuperAdmin logout -> Fase T siap | **SUCCESS** |

---

## 3. Tabel Hasil Pengujian (Fase T)

| ID | Pasien | Aktor | Langkah & Aksi | Hasil yang Diharapkan | Hasil | Bukti Berkas | Catatan & Analisis Hasil |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- | :--- |
| **B6-00** | — | Semua akun peran | Pemeriksaan T1 (build produksi port 3000 tanpa 'N'); login kelima akun peran dan verifikasi tanpa 403 | Tanpa lencana dev 'N'; tanpa `403`. Bila gagal, **berhenti** | **PASS** | `B6-00.png`<br>`jaringan-B6-00.json` | Build produksi mandiri aktif (`node .next/standalone/server.js`, PID 548). Tangkapan layar `B6-00.png` bebas dari dev badge 'N'. Kelima akun uji (`dimas.kurniawan`, `ranger.biru`, `dokter.igd.b4`, `admisi.b4`, `perawat.ruangan.uji.b5`) berhasil login tanpa 403 (HTTP 200). |
| **B6-01** | P-B6-A | Perawat IGD, Dokter DPJP | *Buat Form* rawat inap P-B6-A lengkap → *Minta TTD* → DPJP *Tandatangani* | Form *Terkirim*; P-B6-A tampil di Antrean Transfer IGD/Poli | **PASS** | `B6-01.png`<br>`jaringan-B6-01.json` | Perawat mengisi form permintaan rawat inap (Unit Rawat Inap, kelas UNIQUE, DPJP dr. Arif Lesmana, LOS 3 hari, tingkat ketergantungan Moderate, indikasi klinis lengkap) dan meminta TTD. DPJP dr. Ranger Biru menandatangani via Ruang Kerja Dokter (`POST .../sign` 200). Pasien P-B6-A langsung tampil di Antrean Transfer IGD/Poli admisi. |
| **B6-02** | P-B6-A | SuperAdmin (Opsi 2) | Buka *Antrean Transfer IGD/Poli* → *Proses Admisi*. Selesaikan seluruh langkah wizard 1 s.d. 8 sampai persetujuan | `POST .../admission-transfers/admit` → `201`; P-B6-A hilang dari antrean | **PASS** | `B6-02.png`<br>`jaringan-B6-02.json` | Percobaan awal Opsi 1 dengan akun admisi ditolak server HTTP 403 (`ACCESS_DENIED: Unit admisi di luar scope`). Sesuai Bagian 3C.1 dan instruksi pemilik modul, diaktifkan Opsi 2 (SuperAdmin). Wizard admisi dijalankan lengkap (Verifikasi, Penjamin Tunai, Deposit 0, DPJP dr. Arif Lesmana, Ruang Rawat Inap, Bed Melati 01, Ringkasan, Persetujuan). Panggilan `POST /api/v1/health-services/inpatient-management/admission-transfers/admit` berhasil dengan **HTTP 201 Created** (Episode `RI-261018010042-D6F067`). P-B6-A hilang dari antrean transfer. Sesi SuperAdmin ditutup. |
| **B6-03** | P-B6-A | Perawat IGD | Muat ulang tab *Tindak Lanjut* pasien P-B6-A | *Dijalankan*; lencana *Selesai*; keterangan *"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli."*; *Batalkan Form* tidak ada; *Cetak* ada; `displayStatus` `Completed` | **PASS** | `B6-03.png`<br>`jaringan-B6-03.json` | Tab *Tindak Lanjut* perawat menampilkan status tindak lanjut *Dijalankan*, lencana form hijau *Selesai*, dan notice info persis *"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli."*. Di dalam dialog form: tombol *Batalkan Form* tidak ada, tombol *Cetak* aktif (`data-testid="emergency-admission-request-print-open"`). API `GET emergency-admission-requests?emergencyVisitId=` memuat `displayStatus: 4` (`Completed`). |
| **B6-04** | P-B6-A | **API** perawat, lalu dokter | `POST .../emergency-admission-requests/{id}/cancel` (versi terbaru dengan alasan); lalu `PATCH .../emergency-dispositions/{id}/disposition-status` dengan `dispositionStatus: 4` dan alasan | Batal form: ditolak `409` *"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD."*; Batal tindak lanjut: ditolak `400` *"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."* | **PASS** | `jaringan-B6-04.json` | Guard pembatalan pasca-admit terbukti kokoh: (1) Perawat memanggil `POST .../cancel` dengan `rowVersion` terbaru dan alasan ditolak server dengan `HTTP 409 Conflict` dan pesan persis: *"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD."*. (2) Dokter memanggil `PATCH .../disposition-status` dengan `dispositionStatus: 4` ditolak server dengan `HTTP 400 Bad Request` dan pesan persis: *"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."*. |
| **B6-05** | P-B6-A | Dokter DPJP | Buka Ruang Kerja Dokter (`/doctor-emergency`) tab *Tindak Lanjut* untuk P-B6-A → cari aksi *Batalkan* pada tindak lanjut yang sudah dijalankan | Tombol *Batalkan* tidak ada, **atau** penolakan server tampil apa adanya | **PASS** | `B6-05.png`<br>`jaringan-B6-05.json` | Dokter DPJP membuka tab *Tindak Lanjut* di Ruang Kerja Dokter untuk kunjungan P-B6-A. Riwayat tindak lanjut menampilkan status *Dijalankan* (`Executed`). Tombol aksi *Batalkan* tidak ada (absent) pada antarmuka pengguna, sesuai implementasi konstanta `DOCTOR_DISPOSITION_STATUS_ACTIONS[EXECUTED] = []`. Tidak ada celah bagi dokter untuk membatalkan tindak lanjut yang sudah dieksekusi admisi. |
| **B6-06** | P-B6-A | **Perawat IGD**, lalu **perawat ruangan** | Perawat IGD: tab *Transfer Pasien* → *Buat Kepergian* (unit *Rawat Inap*, SBAR lengkap) → *Ajukan Serah Terima* → *Catat Berangkat*. Perawat ruangan: *Terima Dokumen* → *Catat Tiba* | Kunjungan IGD *Selesai*; `GET …/emergency-visits/{id}` memuat `visitStatus` `Completed` dan waktu selesai. Log memuat `POST …/emergency-departures`, `submit-handover`, `depart`, `accept-handover`, `arrive` | **FAIL** | `B6-06.png`<br>`B6-06-berangkat.png`<br>`B6-06-tiba.png`<br>`jaringan-B6-06.json` | Seluruh aksi alur serah terima berhasil dieksekusi dan terekam di jaringan: `POST emergency-departures` (201), `submit-handover` (200), `depart` (200), `accept-handover` (200), dan `arrive` (200). Tangkapan layar `B6-06-berangkat.png` dan `B6-06-tiba.png` membuktikan status fisik *Tiba* dan dokumen *Diterima*. Namun kunjungan IGD **gagal otomatis berpindah ke Completed**: `visitStatus` tetap bernilai `6` (*AwaitingDisposition*) dan `visitCompletedAt` bernilai `null`. Investigasi backend menemukan bahwa `InpAdmissionTransferService.cs` pada langkah admit hanya mengubah `EmgDisposition.DispositionStatus = Executed`, tetapi tidak mempromosikan `EmgVisit.VisitStatus` ke `Disposed` (7). Akibatnya, closure gate `ValidateVisitClosureAsync` menolak penyelesaian kunjungan dengan pesan *"Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan."*. Sesuai kriteria panduan B6 ("Hasil hanya boleh PASS bila kunjungan IGD berstatus Completed"), skenario ini dinyatakan **FAIL**. |
| **B6-07** | P-B6-B | Petugas Admisi, **API** | Muat ulang antrean transfer; periksa keberadaan P-B6-B (tindak lanjut ranap tanpa form); panggil `GET .../admission-transfers/{dispositionId}?sourceType=Emergency` | P-B6-B tidak tampil di antrean; API `GET` mengembalikan `404` | **PASS** | `B6-07.png`<br>`jaringan-B6-07.json` | Pasien P-B6-B (No. RM `00-00-02-63`) yang memiliki tindak lanjut rawat inap Dikonfirmasi tetapi tanpa form permintaan rawat inap diverifikasi tidak muncul di *Antrean Transfer IGD/Poli* petugas admisi (jumlah baris: 0, `B6-07.png`). Pemanggilan API `GET /api/v1/health-services/inpatient-management/admission-transfers/bbf6cba6-f8d6-4fca-bca7-08c6548cf8d9?sourceType=Emergency` mengembalikan **HTTP 404 Not Found** dengan pesan persis: *"Permintaan rawat inap tidak ditemukan."*. |

---

## 4. Analisis Teknis & Temuan Akar Masalah

### 4.1 Suksesnya Validasi Skenario B6-01 s.d. B6-05 dan B6-07
1. **Langkah Admit & Reservasi Bed (B6-02):** Pelaksanaan admisi via wizard transfer berhasil memvalidasi alur transfer lintas modul secara penuh: verifikasi pasien, penetapan DPJP rawat inap, penguncian bed Melati 01, dan penerbitan nomor episode rawat inap `RI-261018010042-D6F067`. Pasien keluar seketika dari antrean transfer admisi.
2. **Sinkronisasi Status Form di IGD (B6-03):** Begitu admit selesai di Rawat Inap, antarmuka perawat IGD langsung merefleksikan status *Selesai* (`displayStatus: Completed`) dengan informasi penjelas bahwa pasien telah diproses admisi. Tombol pembatalan dihilangkan untuk mencegah inkonsistensi data, dan tombol cetak tetap tersedia untuk keperluan berkas rekam medis fisik.
3. **Integritas Penjaga Pembatalan Lapis Ganda (B6-04 & B6-05):**
   - **Lapis Form:** `POST .../emergency-admission-requests/{id}/cancel` ditolak secara atomik dengan `HTTP 409 Conflict` karena `AmbilTindakLanjutSudahDiadmisiAsync` mendeteksi disposisi telah dieksekusi admisi.
   - **Lapis Disposisi:** `PATCH .../emergency-dispositions/{id}/disposition-status` (ke status Cancelled) ditolak `HTTP 400 Bad Request` karena matriks transisi melarang perpindahan dari `Executed` ke `Cancelled`.
   - **Lapis UI Dokter:** UI ruang kerja dokter secara preventif mengosongkan daftar aksi (`DOCTOR_DISPOSITION_STATUS_ACTIONS[EXECUTED] = []`), sehingga tombol batalkan tidak dirender sama sekali.
4. **Isolasi Antrean Admisi (B6-07):** Membuktikan bahwa kuadran transfer rawat inap yang mensyaratkan form (`RequiresAdmissionRequest`) terisolasi sempurna: pasien tanpa form yang sah tidak akan pernah bocor ke antrean admisi rawat inap dan endpoint rincian mengembalikan 404.

### 4.2 Akar Masalah Kegagalan B6-06 (VisitStatus AwaitingDisposition)
Pada B6-06, alur serah terima dan pergerakan fisik pasien dari IGD ke bangsal Rawat Inap berjalan sempurna secara operasional:
- Perawat IGD membuat kepergian SBAR (`POST .../emergency-departures` -> 201).
- Perawat IGD mengajukan serah terima (`POST .../submit-handover` -> 200).
- Perawat IGD mencatat berangkat (`POST .../depart` -> 200, fisik: `Departed`).
- Perawat Ruangan menerima dokumen SBAR (`POST .../accept-handover` -> 200, dokumen: `Accepted`).
- Perawat Ruangan mencatat pasien tiba di bangsal (`POST .../arrive` -> 200, fisik: `Arrived`).

Ketika `POST .../arrive` berhasil, backend memanggil evaluator penutupan kunjungan otomatis:
`_emergencyVisitService.TryCloseAfterBlockerResolvedAsync(...)` -> `TryCloseAfterDispositionAsync(...)`.

Namun, kunjungan IGD **gagal ditutup** dan statusnya tertahan di `visitStatus: 6` (*AwaitingDisposition*). Penyebab teknisnya adalah:
1. Di `InpAdmissionTransferService.cs` baris 156–163:
   ```csharp
   if (request.SourceType == "Emergency")
   {
       var disposition = await _dbContext.Set<EmgDisposition>().SingleAsync(x => x.Id == request.TransferDispositionId, cancellationToken);
       disposition.DispositionStatus = EmergencyDispositionStatus.Executed;
       disposition.ExecutedAt = DateTime.UtcNow;
       disposition.UpdateBy = actorUserId;
       disposition.UpdateDateTime = DateTime.UtcNow;
   }
   ```
   Service Rawat Inap hanya mengubah status entitas `EmgDisposition.DispositionStatus` menjadi `Executed`, **tetapi lupa mempromosikan status kunjungan induknya (`EmgVisit.VisitStatus`) ke `Disposed` (7)**.
2. Ketika kedatangan pasien dicatat (`arrive`), evaluator memanggil `ValidateVisitClosureAsync(visit)`:
   ```csharp
   if (visit.VisitStatus != EmergencyVisitStatus.Disposed)
       return "Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan.";
   ```
   Karena `visit.VisitStatus` masih bernilai `6` (*AwaitingDisposition*), validasi mengembalikan penolakan *"Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan."*. Akibatnya, `TryApplyVisitStatus(visit, EmergencyVisitStatus.Completed)` tidak pernah dieksekusi dan `visitCompletedAt` tetap `null`.

**Rekomendasi Perbaikan Backend (untuk Muhammad Hamzah / Rizki Gunawan):**
Pada `InpAdmissionTransferService.cs` saat transaksi admit tipe *Emergency*, selain mengupdate `EmgDisposition.DispositionStatus = Executed`, backend harus memanggil pembaruan status kunjungan IGD secara sah:
`_emergencyVisitService.TryApplyVisitStatus(visit, EmergencyVisitStatus.Disposed, actorUserId, DateTime.UtcNow, out _)`
atau memicu `EvaluasiPenutupanKunjunganSusulanAsync`.

---

## 5. Integritas Kode Sumber dan Lingkungan Pengujian

Integritas kode sumber dan lingkungan pengujian diverifikasi secara ketat sebelum (kode-awal) dan sesudah (kode-akhir) pengujian:

1. **Git Commit Head:**
   - Backend: `6bb18fe6a396c4b9b83a24d15405f757b482a482` (Identik, tidak berubah)
   - Frontend: `58ee55d326463a3d443b4ed302e7b1d96623e56e` (Identik, tidak berubah)
2. **Status Kode Sumber (`git status --short`):**
   - Backend: Nol berkas kode `.cs` termodifikasi. Perubahan hanya pada folder runtime `Storage/uploads/patient-qrcodes/`.
   - Frontend: Nol berkas aplikasi `src/` termodifikasi.
3. **Backend Binary DLL:**
   - File: `NewQuilvianSystemBackend/bin/Debug/net9.0/QuilvianSystemBackend.dll`
   - Ukuran: 114,626,560 bytes
   - Timestamp: `10/17/2026 11:43:00 AM UTC` (Build asli Rizki Gunawan, tidak dikompilasi ulang)
4. **Frontend Standalone Build:**
   - `BUILD_ID`: `vyZON79AuJIJU924yT7Hf`
   - Port: 3000 via proses `"C:\nvm4w\nodejs\node.exe" .next/standalone/server.js` (PID 548).
   - Seluruh tangkapan layar bebas dari lencana dev 'N' di pojok kiri bawah.
5. **Kerahasiaan Kredensial:**
   - Semua sandi akun dimuat murni dari variabel lingkungan (`QUILVIAN_*_PASSWORD`).
   - Nol sandi tertulis dalam skrip uji.
6. **Rekam Jejak Jaringan & Percobaan:**
   - Rekam jaringan setiap skenario disimpan di `jaringan-B6-<ID>.json`.
   - Seluruh riwayat percobaan tercatat lengkap di `percobaan.json`.

---

## 6. Lokasi Berkas Bukti Mentah dan Log Backend

Seluruh berkas bukti mentah tersimpan dalam folder:  
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b6-20261018/`

- `persiapan.json`
- `kode-awal.txt`
- `kode-akhir.txt`
- `data-uji.json`
- Tangkapan layar:
  - `B6-00.png` (Pemeriksaan pembuka & login tanpa lencana N)
  - `B6-01.png` (Form ditandatangani DPJP & masuk antrean)
  - `B6-02.png` (Admisi selesai 201 & reservasi bed Melati 01)
  - `B6-03.png` (Tab tindak lanjut selesai & dialog cetak)
  - `B6-05.png` (Ruang kerja dokter tab tindak lanjut tanpa tombol batal)
  - `B6-06-berangkat.png` (Status kepergian berangkat)
  - `B6-06-tiba.png` / `B6-06.png` (Status kepergian tiba & serah terima diterima)
  - `B6-07.png` (Antrean transfer admisi bersih dari pasien tanpa form)
- Berkas jaringan:
  - `jaringan-B6-00.json` s.d. `jaringan-B6-07.json`
- Folder skrip eksekusi: `skrip/`
- Log percobaan: `percobaan.json`
- Ringkasan hasil: `hasil.json`

**Berkas Log Backend Hari Pengujian:**  
`NewQuilvianSystemBackend/Logs/quilvian-backend-20261018.json`

---

## Penilaian agent pengembang — 18 Oktober 2026

Dinilai dari bukti mentah (11 PNG, 8 rekaman jaringan, 19 skrip, `persiapan.json`, `percobaan.json`) dan
`Logs/quilvian-backend-20261018.json`; akar masalah B6-06 diverifikasi di source backend `6bb18fe6`. Laporan di atas
tidak disunting. Rujukan tetap untuk sesi berikutnya:
[penilaian B6 dan keselarasan dengan Rawat Inap](../evidence/2026-10-18-penilaian-b6-dan-keselarasan-rawat-inap.md).
Ringkasan dan akibatnya: `IGD-FACT-128`…`132`, keputusan `IGD-DEC-323`, dan persetujuan yang ditunggu `IGD-OQ-133` di
`00-interview-decisions.md`.

### Disiplin uji

| Pemeriksaan | Hasil |
| --- | --- |
| Lencana `next dev` | Tidak ada pada 11 PNG; proses frontend `.next/standalone/server.js` |
| PNG identik antar-skenario | Tidak ada |
| Sandi tertulis di skrip | Tidak ada; 19 dari 19 skrip membaca variabel lingkungan |
| SQL | Tidak ada jejak |
| Kode | HEAD backend `6bb18fe6` dan frontend `58ee55d32`, DLL 2026-10-17 11:43:00 UTC, dan `BUILD_ID` `vyZON79AuJIJU924yT7Hf` sama di awal dan akhir |

### Catatan pelaksanaan

| Butir | Isi |
| --- | --- |
| Opsi _admit_ | Opsi 1 (entri `BedManagement:UnitAssignments` untuk `admisi.b4`) **belum terpasang**: _admit_ oleh akun admisi ditolak `403` _"Unit admisi di luar scope"_ pukul 07.49 WIB. Baris S6-01 laporan yang menulis **SUCCESS** tidak tepat. Dipakai opsi 2: _admit_ oleh SuperAdmin |
| Urutan _admit_ | `422` pukul 07.54 → SuperAdmin mengubah master kelas **"KELAS II"** `isForInpatient` `false` → `true` (tercatat di `persiapan.json`, sah menurut `IGD-DEC-322`) → `499` pukul 07.58 (koneksi diputus klien) → **`201`** pukul 08.00.47, episode `RI-261018010042-D6F067` |
| Nilai master KELAS II | Nilai semula `false` membuat _admit_ ditolak. Nilai yang benar ditetapkan pemilik Rawat Inap (`IGD-REQ-003` butir 4) |
| Analisis laporan bagian 4.2 | Mekanisme yang diuraikan benar, tetapi **kepemilikannya keliru**: Rawat Inap menulis _Executed_ persis seperti kontraknya (`BE-RWI-207`), dan kontrak itu menyerahkan penutupan kunjungan ke mekanisme IGD. Cacatnya milik IGD (`IGD-FACT-128`). Usulan agar Rawat Inap memanggil `TryApplyVisitStatus` **tidak dipakai** — Rawat Inap tidak menulis status kunjungan IGD (`IGD-DEC-323`) |

### Putusan per skenario

| ID | Penguji | Putusan | Dasar |
| --- | --- | --- | --- |
| B6-00 | PASS | PASS | Build produksi; kelima akun login tanpa `403` |
| B6-01 | PASS | PASS | Form, _Minta TTD_, tanda tangan `200`; P-B6-A tampil di antrean |
| B6-02 | PASS | PASS (opsi 2) | `POST …/admission-transfers/admit` `201` pukul 08.00.47 oleh SuperAdmin; urutan lengkap di catatan pelaksanaan |
| B6-03 | PASS | PASS | Tindak lanjut _Dijalankan_; form _Selesai_ dengan keterangan _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli."_; tanpa _Batalkan Form_; _Cetak_ ada; `displayStatus` `4` |
| B6-04 | PASS | PASS | Satu panggilan `…/undefined/cancel` `404` karena salah skrip, lalu `409` _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD."_ dan `400` _"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."_ |
| B6-05 | PASS | PASS | Dokter tidak punya aksi _Batalkan_ pada tindak lanjut yang sudah dijalankan |
| B6-06 | FAIL | **FAIL — cacat nyata** | Log memuat `emergency-departures` `201`, `submit-handover` `200`, `depart` `200` (perawat IGD `dimas`, 08.21–08.22) serta `accept-handover` `200` dan `arrive` `200` (`perawat.ruangan.uji.b5`, 08.22). Kunjungan tetap `visitStatus` `6` (`AwaitingDisposition`); PNG B6-03 dan B6-06 berlencana "Menunggu keputusan". Akar masalah di bagian berikut |
| B6-07 | PASS | PASS | P-B6-B (tindak lanjut rawat inap tanpa form) tidak tampil di antrean; `GET …/admission-transfers/{id}` `404` |

### Akar masalah B6-06

_Admit_ Rawat Inap hanya mengubah `EmgDisposition` menjadi _Executed_ (`InpAdmissionTransferService.cs:156-162`), sesuai
desain `BE-RWI-207`. Tombol _Jalankan_ IGD memindahkan kunjungan ke `AwaitingDisposition` lalu `Disposed` lebih dulu
(`EmergencyDispositionController.cs:406-450`), tetapi jalur _admit_ tidak punya langkah itu. Saat perawat ruangan
menekan _Terima Dokumen_ (08.22.21), penutupan dicoba lewat `TryCloseAfterBlockerResolvedAsync` →
`TryCloseAfterDispositionAsync` (`EmergencyVisitService.cs:679`) → `ValidateVisitClosureAsync`
(`EmergencyDispositionService.cs:129`), yang menolak karena status bukan `Disposed`: _"Kunjungan hanya dapat
diselesaikan setelah keputusan tindak lanjut ditetapkan."_ `EvaluasiPenutupanKunjunganSusulanAsync`
(`EmergencyDispositionService.AdmissionRequest.cs:1547`, `BE-IGD-077`) lewat jalur yang sama, sehingga ikut gagal
walaupun kelak dipanggil Rawat Inap.

**Tambahan dari pemeriksaan source dan log (`IGD-FACT-130`).** _Catat Tiba_ lima detik kemudian (08.22.26) **tidak
mencoba penutupan sama sekali**: `POST …/arrive` bukan titik pemicu (state §9.2 hanya mengenal terima, tolak, dan batal
dokumen; sikap pesanan; observasi; _Executed_). Seandainya kunjungan sudah `Disposed` pun, penutupan pada _Terima
Dokumen_ tetap tertahan karena pasien belum tiba, lalu tidak dicoba lagi. Jadi urutan B6-06 (_Terima Dokumen_ lalu _Catat
Tiba_) membutuhkan dua perbaikan, bukan satu. Perbaikannya `BE-IGD-083` di sisi IGD; rinciannya di kartu.

### Akibat pada task

| Task | Sebelum | Sesudah |
| --- | --- | --- |
| `BE-IGD-074` | 🟡 7 dari 10 | ✅ **10 dari 10** — #3 rincian `404` pada build (B5-10, B6-07), dan _admit_ membaca lewat `GetTransferByIdAsync` → `TransferQuery` yang sama (`InpAdmissionTransferService.cs:89`) sehingga form tanpa tanda tangan juga `404` di _admit_; #5 penolakan sesudah diproses (B6-04); #6 `displayStatus` _Completed_ sesudah _admit_ (B6-03) |
| `FE-IGD-061` | 🟡 | ✅ — #2 keadaan _Selesai_ (B6-03); #3 bagian sesudah _admit_ (B6-03 tanpa _Batalkan Form_, B6-05 tanpa _Batalkan_, B6-04 penolakan server); #5 form _Selesai_ (B6-03) |
| `BE-IGD-077` | 🟡 5 dari 7 | 🟡 5 dari 7 — acceptance 4 **cacat di source**: bagian fungsi milik IGD dan diperbaiki `BE-IGD-083`; bagian pemanggil menunggu Rawat Inap (`IGD-OQ-132`, `IGD-REQ-003`) |
| `BE-IGD-083` | — | Baru — ⛔ sampai pemilik menyetujui delta matriks state (`IGD-OQ-133`) |

Data uji: P-B6-A (`IGD-261018003352-28FF6F`) tertahan `AwaitingDisposition` dengan tindak lanjut _Executed_ dan
kepergian _Tiba_; dirapikan sekali lewat _Selesaikan_ sesudah `BE-IGD-083`, bukan lewat SQL. P-B6-B
(`IGD-261018003405-FA1086`) masih punya tindak lanjut rawat inap _Dikonfirmasi_ tanpa form dan dapat dipakai Putaran B7.

Lanjutan: [Putaran B7](2026-10-17-panduan-uji-sisa-modul-igd.md#3d-putaran-b7--penutupan-kunjungan-sesudah-admit-be-igd-083).

### Koreksi — 18 Oktober 2026 (sesudah Putaran B7)

Baris _Opsi admit_ pada catatan pelaksanaan di atas **keliru**. Opsi 1 sudah terpasang sejak 07.23, sebelum B6 berjalan:
`secrets.json` proyek berisi entri `BedManagement:UnitAssignments:4` untuk `admisi.b4`, persis seperti templat panduan
§3C.1. Templat itu yang tidak memuat operasi `InpatientEpisode:Create`, yang diperiksa langkah _admit_. Itu sebabnya
`admit` ditolak `403` _"Unit admisi di luar scope."_. Keterangan penguji pada baris S6-01 laporan ini benar, dan cacatnya
ada di panduan. Panduan §3C.1 sudah diperbaiki (`IGD-FACT-134`). Putusan per skenario B6 tidak berubah.
