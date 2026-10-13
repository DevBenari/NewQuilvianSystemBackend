# Panduan Uji Antigravity Putaran 1 — MVP-9 Ruang Kerja Dokter IGD

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 8 Oktober 2026 |
| Untuk | Agen penguji (Antigravity) yang dijalankan pemilik modul (Rizki) |
| Cakupan task | `BE-IGD-065`, `BE-IGD-066`, `BE-IGD-067`, `BE-IGD-068`, `FE-IGD-045`, `FE-IGD-046`, `FE-IGD-047`, `FE-IGD-048`, `FE-IGD-049` |
| Tujuan | Membuktikan kriteria penerimaan gelombang MVP-9 (Ruang Kerja Dokter IGD, Pengkajian Medis, Catatan Dokter SOAP + ICD-10, Catatan Saya + Addendum, dan Peresepan IGD) pada putaran 1 |
| Jumlah skenario | 28 skenario — 13 uji API, 15 uji layar |
| Source backend | Backend branch `rizkiG` (`939e37c9`) + working tree `EmergencyDoctorAssignmentService.cs`, `EmergencyVisitController.cs`, `PatientAssessmentController.cs`, `DoctorConsultationController.cs`, `DoctorConsultationDtos.cs`, `ConsultationFinalizationService.cs` — **wajib hasil build pemilik** |
| Source frontend | Frontend branch `RizkiV2` + working tree `doctor-emergency/**`, hook mandiri IGD (`use-emergency-*`), tab mandiri IGD (`emergency-doctor-*`), `my-notes/**` (`IGD-DEC-234`), berkas rawat inap 100% utuh identik HEAD — **wajib hasil build** |
| Kontrak | API §10.1–§10.8; validation §12.1–§12.6; architectural blueprints §15 |
| Status | Panduan siap eksekusi. Hasil uji akan dicatat pada berkas laporan terpisah |

---

## 1. Aturan Pengujian yang Mengikat Agen

1. **Layar dilayani hasil build produksi** (`node .next/standalone/server.js` atau `npm run start`), bukan `next dev`. Bukti: `document.querySelector('nextjs-portal') === null` pada setiap pengujian layar.
2. **Viewport 1440 × 900** dengan menu samping (sidebar) terbuka untuk seluruh uji layar.
3. **Akun peran nyata** (tanpa SuperAdmin):
   - Dokter IGD: `ranger.biru@admin.com` (`QUILVIAN_DOKTER_EMAIL`)
   - Perawat IGD: `dimas.kurniawan@rsmmc.local` (`QUILVIAN_PERAWAT_EMAIL`)
   - Petugas Loket: `QUILVIAN_LOKET_EMAIL`
4. **Sandi dan kredensial dilarang keras ditulis di skrip, berkas bukti, atau laporan.** Selalu gunakan `process.env`.
5. **Konfigurasi Akses Role dan data HR tidak diubah agen.** Bila izin ditolak `403`, catat `NOT RUN` dan laporkan ke pemilik.
6. **Pemeriksaan teks teliti dan eksak.** Wadah teks dibatasi pada kartu, modal, atau formulir terkait.
7. **Tangkapan layar viewport diambil** setelah respons selesai dimuat (delay minimal 1000 ms).
8. **Tidak ada penulisan data langsung (SQL INSERT/UPDATE/DELETE).** Kueri hanya `SELECT`.
9. **Kompilasi backend dijalankan oleh Rizki.** Agen tidak menjalankan `dotnet build`.

---

## 2. Persiapan Uji

### 2.1 Verifikasi Build & Layanan
- **Backend:** Pastikan Rizki telah menjalankan build backend:
  `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false`
  dan backend berjalan di `https://localhost:7184`.
- **Frontend:** Pastikan proses frontend di port 3000 adalah hasil build standalone (`node .next/standalone/server.js`) atau `npm run start`.
- **Kredensial Environment:**
  - `QUILVIAN_DOKTER_EMAIL=ranger.biru@admin.com`
  - `QUILVIAN_DOKTER_PASSWORD=<password>`
  - `QUILVIAN_PERAWAT_EMAIL=dimas.kurniawan@rsmmc.local`
  - `QUILVIAN_PERAWAT_PASSWORD=<password>`
  - `QUILVIAN_LOKET_EMAIL=<email>`
  - `QUILVIAN_LOKET_PASSWORD=<password>`

---

## 3. Matriks Skenario Uji Putaran 1

### 3.1 Skenario Backend (API)

#### A. Saringan & Informasi Dokter IGD (`BE-IGD-065`)
- **`A-065-1`**: `GET /api/v1/health-services/emergency-installation-management/emergency-visits?ongoing=true`
  - **Harapan:** HTTP 200, mengembalikan daftar kunjungan berstatus aktif (`Arrived`, `WaitingForTriage`, `Triaged`, `InTreatment`, `UnderObservation`, `AwaitingDisposition`, `Disposed`).
- **`A-065-2`**: `GET /api/v1/health-services/emergency-installation-management/emergency-visits?ongoing=false`
  - **Harapan:** HTTP 200, mengembalikan daftar kunjungan berstatus selesai/batal (`Completed`, `Cancelled`).
- **`A-065-3`**: `GET /api/v1/health-services/emergency-installation-management/emergency-visits?doctorId={validDoctorId}&ongoing=true`
  - **Harapan:** HTTP 200, butir kunjungan yang memiliki DPJP aktif sesuai `doctorId` menyertakan `activeDoctorId` dan `activeDoctorName`.
- **`A-065-4`**: `GET /api/v1/health-services/emergency-installation-management/emergency-visits/{id}`
  - **Harapan:** HTTP 200, objek detail memuat `activeDoctorId` dan `activeDoctorName` yang konsisten dengan penugasan DPJP berjalan.

#### B. Validasi Pengkajian Medis IGD (`BE-IGD-066`)
- **`A-066-1`**: `POST /api/v1/health-services/clinical-management/patient-assessments` (kajian medis awal pertama untuk kunjungan IGD aktif)
  - **Harapan:** HTTP 201 Created, tersimpan dengan `encounterId` dan tanpa `inpEpisodeId`.
- **`A-066-2`**: `POST /api/v1/health-services/clinical-management/patient-assessments` (kajian medis awal kedua untuk kunjungan IGD yang sama)
  - **Harapan:** HTTP 409 Conflict, pesan: *"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."*
- **`A-066-3`**: `POST /api/v1/health-services/clinical-management/patient-assessments` (kajian medis awal/ulang pada kunjungan IGD yang sudah berakhir `Completed`/`Cancelled`)
  - **Harapan:** HTTP 409 Conflict, pesan: *"Kunjungan IGD sudah berakhir. Pengkajian medis tidak dapat ditambah atau diubah."*

#### C. Lini Masa SOAP per Encounter (`BE-IGD-067`)
- **`A-067-1`**: `GET /api/v1/health-services/clinical-management/doctor-consultations/encounters/{encounterId}/soap-timeline` (kunjungan IGD dengan catatan draf dan selesai)
  - **Harapan:** HTTP 200, objek berstruktur `EncounterSoapTimelineResponse` (`encounterId`, `emergencyVisitId`, `patientId`, `totalCount`, `items` bertipe `SoapTimelineItemResponse`).
- **`A-067-2`**: `GET /api/v1/health-services/clinical-management/doctor-consultations/encounters/{encounterId}/soap-timeline` (kunjungan IGD tanpa catatan dokter)
  - **Harapan:** HTTP 200, `totalCount: 0`, `items: []`.
- **`A-067-3`**: `GET /api/v1/health-services/clinical-management/doctor-consultations/encounters/{encounterId}/soap-timeline` (encounter bukan milik kunjungan IGD)
  - **Harapan:** HTTP 400 Bad Request, pesan: *"Lini masa catatan dokter per encounter hanya berlaku untuk kunjungan IGD."*
- **`A-067-4`**: `GET /api/v1/health-services/clinical-management/doctor-consultations/encounters/{nonExistentGuid}/soap-timeline`
  - **Harapan:** HTTP 404 Not Found, pesan: *"Encounter pasien tidak ditemukan."*

#### D. Penolakan Fakta Jasa Konsultasi IGD (`BE-IGD-068`)
- **`A-068-1`**: `PATCH /api/v1/health-services/clinical-management/doctor-consultations/{id}/complete` pada encounter IGD dengan resep
  - **Harapan:** HTTP 200, catatan selesai dan terkunci. Fakta tagih Billing memuat fakta resep tetapi **tidak ada** fakta `ConsultationCompleted` untuk konsultasi tersebut (`IGD-DEC-229`).
- **`A-068-2`**: `PATCH /api/v1/health-services/clinical-management/doctor-consultations/{id}/complete` pada encounter rawat jalan / poliklinik (uji regresi)
  - **Harapan:** HTTP 200, tetap memancarkan fakta tagih `ConsultationCompleted` seperti sedia kala.

---

### 3.2 Skenario Frontend (Layar)

#### A. Ruang Kerja Dokter IGD (`FE-IGD-045`)
- **`U-045-1`**: Buka route `/health-services/emergency-installation-management/doctor-emergency` dengan akun dokter `ranger.biru@admin.com`.
  - **Harapan:** Header halaman menampilkan judul "Dokter - IGD", tombol "Catatan Saya", dan rekapitulasi ringkasan pasien berjalan.
- **`U-045-2`**: Alihkan saringan antara "Pasien saya" dan "Semua pasien IGD".
  - **Harapan:** Daftar pasien berganti secara responsif; kartu pasien memuat nama, No. RM, status triase/layanan, dan nama dokter DPJP.
- **`U-045-3`**: Pilih salah satu pasien dari daftar.
  - **Harapan:** Panel kanan membuka shell ruang kerja pasien lengkap dengan tab navigasi: Pengkajian Medis, Catatan Dokter, Resep, Tindakan, Penunjang, Tindak Lanjut.

#### B. Tab Pengkajian Medis IGD (`FE-IGD-046`)
- **`U-046-1`**: Buka tab "Pengkajian Medis" pasien IGD aktif.
  - **Harapan:** Sub-tab Formulir Pengkajian dan Riwayat Pengkajian aktif. Informasi rujukan keperawatan IGD terbaca bila perawat sudah mengisi pengkajian.
- **`U-046-2`**: Tulis kajian medis awal dokter dan simpan.
  - **Harapan:** Notifikasi sukses muncul; kajian masuk ke riwayat pengkajian; diagnosis tersinkronisasi.
- **`U-046-3`**: Coba buat kajian medis awal kedua pada kunjungan yang sama.
  - **Harapan:** Muncul penolakan 409 dengan pesan: *"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."*

#### C. Tab Catatan Dokter & CPPT Terintegrasi (`FE-IGD-047`)
- **`U-047-1`**: Buka tab "Catatan Dokter" pasien IGD aktif.
  - **Harapan:** Sub-tab menampilkan "Form SOAP", "Riwayat SOAP", "Catatan Dokter", dan "Catatan Terpadu (CPPT)".
- **`U-047-2`**: Buat draf catatan SOAP baru, tambahkan diagnosis ICD-10 kerja, lalu simpan sebagai draf.
  - **Harapan:** Draf tersimpan, muncul di riwayat catatan dokter kunjungan tersebut (`IGD-DEC-231`).
- **`U-047-3`**: Selesaikan catatan SOAP tersebut hingga berstatus `Completed`.
  - **Harapan:** Modal konfirmasi penguncian muncul; setelah dikonfirmasi, catatan berstatus terkunci dan tidak dapat diubah lagi secara inline.
- **`U-047-4`**: Buka sub-tab "Catatan Terpadu (CPPT)".
  - **Harapan:** Timeline CPPT menampilkan kronologi catatan perkembangan pasien dari dokter dan perawat untuk encounter IGD terkait. Banner verifikasi DPJP rawat inap tidak dimunculkan.

#### D. Layar Catatan Saya Dokter IGD (`FE-IGD-048`)
- **`U-048-1`**: Klik tombol "Catatan Saya" di header ruang kerja dokter IGD (menuju `/health-services/emergency-installation-management/doctor-emergency/my-notes`).
  - **Harapan:** Halaman membuka daftar "Catatan Terkunci & Addendum" milik dokter yang sedang login, dengan banner penjelasan bahwa draf IGD dibuka langsung dari tab Catatan Dokter pasien (`IGD-DEC-231`).
- **`U-048-2`**: Periksa daftar catatan terkunci.
  - **Harapan:** Menampilkan catatan dokter IGD yang telah diselesaikan (via `serviceContext: Outpatient`).
- **`U-048-3`**: Klik tombol "Addendum" pada salah satu catatan terkunci, isi alasan dan catatan koreksi, lalu simpan.
  - **Harapan:** Addendum tersimpan permanen di bawah catatan asli tanpa menimpa teks SOAP awal.

#### E. Tab Resep Dokter IGD (`FE-IGD-049`)
- **`U-049-1`**: Buka tab "Resep" saat pasien BELUM memiliki catatan dokter terbuka.
  - **Harapan:** Tampil pesan penuntun eksak: *"Buat atau buka catatan dokter lebih dulu — resep menempel pada catatan dokter."*
- **`U-049-2`**: Buka tab "Resep" saat ada catatan dokter draf aktif.
  - **Harapan:** Pemilih catatan dokter terisi catatan pengait; dokter dapat memilih obat dari katalog, mengisi signa dan jumlah, serta menyimpan resep sebagai draf.
- **`U-049-3`**: Periksa sub-tab "Riwayat Resep".
  - **Harapan:** Resep yang telah dibuat untuk kunjungan IGD tersebut tercantum lengkap dengan status dan rincian obatnya.

---

## 4. Pelaporan Hasil Uji

Hasil pengujian wajib dituliskan ke berkas baru:
`NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-08-laporan-uji-antigravity-putaran-1.md`
dengan melampirkan rekaman jaringan (JSON request/response) dan tangkapan layar (PNG) sesuai konvensi pengujian modul IGD.
