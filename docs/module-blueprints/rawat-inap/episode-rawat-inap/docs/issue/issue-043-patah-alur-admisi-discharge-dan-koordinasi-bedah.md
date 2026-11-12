# ISSUE-043 — Verifikasi ulang empat dugaan titik putus admisi, kepulangan, dan kamar bedah: dua tidak terbukti, dua terbukti sebagian

```yaml
issue_id: ISSUE-EPS-043
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Detail Episode — Catat Kepergian (FE-INP-04, FE-INP-14), Resume Medis (FE-INP-06), Workspace PPRI — IPD dan Estimasi Biaya (FE-INP-35 s.d. 44), Pelayanan Bedah (IBS) pada Ruang Kerja Keperawatan"
sumber_laporan: "Dokumen audit alur bisnis rawat inap 02-11-2026: 4 butir, tanpa screenshot dan tanpa capture database"
tanggal_issue: "2026-11-02"
status: TERBUKA
keparahan_tertinggi: Medium
source_sha_backend: "396c8363 (MHamzah)"
source_sha_frontend: "9c4000724 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-043-patah-alur-admisi-discharge-dan-koordinasi-bedah.md
```

> **Catatan revisi 2 (2026-11-02).** Versi 1 dokumen ini menyebut empat titik putus sebagai
> `SUDAH-VERIFIKASI`. Saat dicocokkan ulang dengan source pada SHA yang sama, dua klaim ternyata
> keliru karena fiturnya sudah ada, dan dua lainnya hanya benar sebagian. Banyak berkas yang dikutip
> versi 1 juga tidak ada di repository, misalnya `InpDischargeService.Summary.cs`,
> `InpatientBedOccupancyService.cs`, `InpAdmissionWorkspaceService.cs`, dan
> `InpSurgeryBookingService.cs`. Kode galat `INP-CLS-003` dan service `IOprCaseService` juga tidak ada.
> Seluruh isi di bawah ini adalah hasil verifikasi ulang. Isi versi 1 tidak dihapus dari jejak:
> perubahannya dicatat pada bagian 9.

---

## 1. Ringkasan

Dokumen ini memeriksa empat dugaan titik putus pada sub-modul `episode-rawat-inap`. Dugaan pertama
mengatakan kasir menahan pelepasan tempat tidur. Dugaan ini **tidak terbukti**. Sejak keputusan
`RWI-DEC-186` (1 Oktober 2026), perawat sudah dapat mencatat pasien keluar ruangan tanpa menunggu
kasir. Tempat tidurnya langsung masuk status pembersihan. Dugaan keempat, pemesanan kamar operasi
dari bangsal yang disebut terputus, juga **tidak terbukti**. Pemesanan dari bangsal sudah membuat
kasus di modul Kamar Operasi dan statusnya tampil kembali di bangsal.

Dua dugaan lain **terbukti sebagian**. Rencana kontrol pada resume medis memang hanya berupa teks,
dan sampai sekarang tidak ada kemampuan janji temu poliklinik yang dapat dipanggil. Estimasi biaya
PPRI memang masih disembunyikan, tetapi penyebabnya keputusan tarif yang belum diambil, bukan
kesalahan program. Nama dokter perujuk luar sebenarnya sudah tersimpan di episode, tetapi lembar IPD
belum membacanya. Tidak ada butir yang menghentikan alur sama sekali. Keparahan tertinggi adalah
`Medium`.

---

## 2. Laporan asli

Laporan berasal dari dokumen audit, bukan dari pengamatan layar. Kolom terakhir mencatat apakah
lampiran yang dirujuk benar-benar ada.

| No. laporan | Keluhan (kata-kata pelapor) | Lampiran yang merujuk | Hasil pemeriksaan lampiran |
| ---: | --- | --- | --- |
| 1 | "Pelepasan tempat tidur dan kepulangan pasien mengalami benturan aturan (deadlock kasir vs operasional bangsal). Pasien tertahan tidak bisa keluar kamar atau bed tidak bisa dibersihkan untuk pasien IGD." | `00-interview-decisions.md:565` (`RWI-CON-015`), `InpDischargeService.Closure.cs:112-145`, `InpatientBedOccupancyService.cs` | Baris 565 memang mendefinisikan `RWI-CON-015`, tetapi konflik itu **sudah ditutup** `RWI-DEC-186` (baris 44 berkas yang sama). Baris 112-145 `Closure.cs` adalah daftar periksa administrasi, bukan pelepasan bed. `InpatientBedOccupancyService.cs` tidak ada; yang ada `InpBedOccupancyService.cs` |
| 2 | "Jadwal kontrol yang diinput dokter pada resume medis kepulangan tidak membuatkan antrean di poliklinik rawat jalan, pasien tetap harus antre manual seperti pasien baru." | `InpDischargeService.Summary.cs:88-120`, `inpatient-discharge-summary-view.jsx` | Kedua berkas **tidak ada** di repository |
| 3 | "Estimasi biaya pada Workspace PPRI terblokir/tidak tampil, dan nama dokter perujuk luar negeri/eksternal hanya berupa garis kosong strip." | `Daftar-Pekerjaan-Belum-Selesai.md:164-167`, `InpAdmissionWorkspaceService.cs` | Baris 164-167 ada dan benar. `InpAdmissionWorkspaceService.cs` tidak ada; yang ada `InpAdmissionWorkspaceQueryService.cs` |
| 4 | "Formulir persiapan pra-bedah bangsal sudah ada, tetapi belum tersambung langsung dengan sistem jadwal meja operasi kamar bedah (OK)." | `Pasca-Operasi-ke-Rawat-Inap.md:35-48`, `02-backend-architecture.md:531-535`, `OprWardPreOpNote.cs` | Berkas ada, tetapi tidak satu pun membuktikan pemesanan bangsal terputus |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Hasil verifikasi | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | :---: | --- | --- | --- | --- | :---: | --- | --- |
| `ISS-EPS-043-01` | 1 | Pelepasan tempat tidur dikira tertahan izin kasir | **TIDAK TERBUKTI** | Tidak ada — fitur sudah berjalan | — | — | `SUDAH-VERIFIKASI` | `FIX-EPS-043-01` (dibatalkan) |
| `ISS-EPS-043-02` | 2 | Rencana kontrol pada resume medis hanya teks dan tidak membuat janji temu | **TERBUKTI** | `DESIGN_CHANGE` | Backend, frontend, lintas modul Registrasi | Medium | `SUDAH-VERIFIKASI` | `FIX-EPS-043-02` |
| `ISS-EPS-043-03` | 3 | (A) Estimasi biaya PPRI disembunyikan; (B) dokter perujuk luar tercetak garis kosong | **TERBUKTI SEBAGIAN** | (A) `DESIGN_CHANGE` — menunggu keputusan tarif; (B) `DESIGN_CHANGE` | Backend, frontend | (A) Medium; (B) Low | `SUDAH-VERIFIKASI` | `FIX-EPS-043-03`, `FIX-EPS-043-05` |
| `ISS-EPS-043-04` | 4 | Pemesanan kamar operasi dari bangsal dikira tidak sampai ke Kamar Operasi | **TIDAK TERBUKTI** | Tidak ada — fitur sudah berjalan | — | — | `SUDAH-VERIFIKASI` | `FIX-EPS-043-04` (dibatalkan) |
| `ISS-EPS-043-T1` | — | Pembatalan reservasi tempat tidur dikira tanpa jejak pelaku | **TIDAK TERBUKTI** | Tidak ada | — | — | `SUDAH-VERIFIKASI` | Tidak ada |

---

## 4. Rincian per temuan

### ISS-EPS-043-01 — Pelepasan tempat tidur dikira tertahan izin kasir

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Hasil verifikasi** | TIDAK TERBUKTI |
| **Jenis** | Tidak ada. Perilaku yang diminta pelapor sudah menjadi desain dan sudah berjalan |
| **Area** | — |
| **Keparahan** | — |
| **Status bukti** | `SUDAH-VERIFIKASI` di source backend, source frontend, dan log keputusan |
| **Perbaikan** | `FIX-EPS-043-01` — dibatalkan |

**Apa yang sebenarnya terjadi di sistem.** Perawat dapat mencatat pasien keluar ruangan walaupun
kasir belum memberi izin. Sistem hanya menampilkan peringatan dan meminta perawat mengonfirmasi.
Begitu dikonfirmasi, tempat tidur dilepas, statusnya berubah menjadi **Pembersihan**, dan episode
tetap terbuka sampai kasir selesai. Izin kasir baru diperiksa ketika episode **ditutup**, bukan ketika
pasien keluar ruangan.

**Kenapa klaim versi 1 keliru.**

1. Konflik `RWI-CON-015` (`00-interview-decisions.md:565`) memang pernah ada, tetapi sudah ditutup
   pada 1 Oktober 2026 oleh `RWI-DEC-186` (`00-interview-decisions.md:2702`): *"Izin kasir diperiksa
   saat episode ditutup, bukan saat pasien meninggalkan ruangan."*
2. Pencatatan kepergian tidak menolak saat kasir belum `CLEARED`. Ia hanya meminta pengakuan
   peringatan — `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs:38`:

   ```csharp
   if ((!observed.IsReadable || observed.Status != "CLEARED") && request?.ClearanceWarningAcknowledged != true)
       return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.Conflict, ..., "INP-DEP-001");
   ```

3. Setelah dikonfirmasi, tempat tidur langsung dilepas (`InpDischargeService.Departure.cs:47`), lalu
   status bed menjadi `Cleaning` bila masih perlu dibersihkan
   (`Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:1991`).
4. Frontend sudah menangani alur ini. Dialog tetap terbuka saat server menjawab `INP-DEP-001`,
   lalu kiriman kedua membawa `clearanceWarningAcknowledged: true`
   (`src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx:594`,
   `src/utils/health-services/inpatient-management/inpatient-departure-utils.jsx:150`).
5. Pasien yang pulang sebelum izin kasir tetap terpantau lewat daftar khusus
   (`InpDischargeService.Departure.cs:62`, `GET .../monitoring/departures-before-clearance`).
6. Kode galat `INP-CLS-003` yang disebut versi 1 tidak ada di source. Status
   `DischargedPhysically_PendingBilling` dan `Vacated_PendingCleaning` yang diusulkan versi 1 hanya akan
   menduplikasi pasangan data yang sudah ada: `EpisodeStatus = DischargePending` ditambah
   `PhysicallyLeftAt` terisi.

**Proses bisnis yang berlaku sekarang.**

| Hal | Isi |
| --- | --- |
| Tujuan | Tempat tidur cepat kembali siap pakai, tanpa kehilangan kendali tagihan |
| Pelaku | DPJP memutuskan pulang; perawat mencatat kepergian; petugas kebersihan membersihkan; kasir memberi izin; petugas bangsal menutup episode |
| Pemicu | DPJP menyatakan pasien boleh pulang |
| Prasyarat | Episode berstatus `DischargePending` dan penempatan bed masih aktif |

Langkah utama:

1. DPJP mencatat keputusan pulang. Episode menjadi `DischargePending`.
2. Pasien berkemas dan keluar ruangan.
3. Perawat menekan **Catat Kepergian**.
4. Bila kasir belum memberi izin, sistem menampilkan peringatan `INP-DEP-001`. Perawat mengonfirmasi.
5. Sistem melepas tempat tidur. Bed tampil sebagai **Pembersihan** pada denah.
6. Petugas kebersihan menekan **Mulai Pembersihan** dari denah (`FE-RWI-264`). Setelah selesai, bed
   kembali **Tersedia** dan dapat dipesan pasien IGD.
7. Kasir menyelesaikan tagihan sampai berstatus `CLEARED`.
8. Petugas bangsal menutup episode. Episode menjadi `Closed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| Episode `Admitted` | Keputusan pulang | `DischargePending` | DPJP | — |
| Bed terisi | Catat kepergian | Bed `Cleaning`; episode tetap `DischargePending` | Perawat (`InpatientDischarge : RecordDeparture`) | Peringatan kasir diakui bila belum `CLEARED` |
| Episode `DischargePending` | Tutup episode | `Closed` | Petugas bangsal | Kasir `CLEARED` |
| Episode `DischargePending` | Tutup tanpa izin kasir | `Closed` dengan tanda `IsClosedWithoutFinancialClearance` | Supervisor | Alasan tertulis wajib (`INP-CLS-012`) |

Jalur tidak normal: kasir dapat mencabut izin setelah pasien pergi karena ada tagihan susulan. Dalam
kasus itu penutupan episode ditolak sampai izin kembali `CLEARED` atau supervisor menutup dengan
alasan tertulis (`RWI-AC-292`).

**Contoh.** Tn. A (pseudonim), kamar VIP 401. Pukul 10.00 DPJP memutuskan pasien boleh pulang. Pukul
10.45 pasien keluar ruangan. Kasir masih memeriksa klaim asuransi. Perawat menekan Catat Kepergian,
membaca peringatan, lalu mengonfirmasi. Saat itu juga bed 401 berubah menjadi Pembersihan. Pukul 11.15
bed 401 sudah tersedia untuk pasien IGD. Pukul 12.45 kasir memberi izin, dan petugas bangsal menutup
episode Tn. A.

**Apakah ini menyimpang dari desain?** Tidak. Perilaku ini sesuai `RWI-DEC-186`, `RWI-RULE-036`, dan
`BE-RWI-153`.

**Dampak nyata.** Tidak ada *bed blockage* (tempat tidur kosong yang tercatat masih terisi) akibat
kasir. Bila petugas lapangan masih merasa tombolnya terkunci, kemungkinan penyebabnya adalah hak akses
`InpatientDischarge : RecordDeparture` yang belum diberikan pada perannya. Hal ini perlu ditanyakan
kepada pelapor (P-01).

**Rekomendasi.** Tidak ada perubahan source. `FIX-EPS-043-01` dibatalkan.

---

### ISS-EPS-043-02 — Rencana kontrol pada resume medis hanya teks dan tidak membuat janji temu

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Hasil verifikasi** | TERBUKTI |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Backend dan frontend rawat inap, serta lintas modul `RegistrationManagement` |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source backend |
| **Perbaikan** | `FIX-EPS-043-02` |

**Apa yang terjadi.** Dokter menulis rencana kontrol pada resume medis, misalnya *"Kontrol 7 hari lagi
ke Poli Penyakit Dalam"*. Tulisan itu hanya tersimpan sebagai teks. Tidak ada janji temu yang terbit
di poliklinik. Pada hari kontrol, pasien mendaftar seperti kunjungan biasa.

**Kenapa terjadi.**

1. Resume medis hanya punya satu kolom teks bebas untuk rencana kontrol, paling banyak 2000 karakter —
   `Areas/HealthServices/InPatientManagement/Models/InpDischargeSummary.cs:30-31`:

   ```csharp
   [MaxLength(2000)]
   public string? FollowUpInstruction { get; set; }
   ```

   Tidak ada kolom tanggal, poli, atau dokter kontrol yang terstruktur. Versi 1 keliru menyebut
   kolom-kolom itu sudah ada.
2. Modul Registrasi **belum punya kemampuan janji temu poliklinik** yang dapat dipanggil. Yang ada
   hanya reservasi pasien prioritas, dan itu *"bukan kunjungan: tidak membuat antrean maupun
   tagihan"* (`Areas/HealthServices/RegistrationManagement/Models/RegPriorityReservation.cs`, ringkasan
   kelas). `RegistrationManagement.AppointmentService` yang disebut versi 1 tidak ada.
3. Tidak ada gateway WhatsApp di source backend. Kata "WhatsApp" hanya muncul sebagai kolom kontak
   pada master penjamin, asuransi, dan pemasok.

**Apakah ini menyimpang dari desain?** Tidak. Resume medis memang dirancang sebagai dokumen. Membuat
janji temu otomatis adalah perubahan desain yang melibatkan modul Registrasi.

**Dampak nyata.** Ny. B (pseudonim) pulang dengan catatan kontrol 7 hari. Pada hari ketujuh, kuota
dokter yang sama sudah penuh, dan Ny. B harus mengambil jadwal lain.

**Rekomendasi.** Kerjakan bertahap. Tahap pertama dapat dilakukan di dalam rawat inap sendiri:
rencana kontrol dipecah menjadi data terstruktur (tanggal, poli, dokter, catatan). Tahap kedua,
membuat janji temu otomatis, baru dapat dikerjakan setelah pemilik Registrasi menyediakan kemampuan
janji temu (K-01). Pengiriman WhatsApp adalah tahap ketiga dan menunggu penyedia layanan (K-02).

---

### ISS-EPS-043-03 — Estimasi biaya PPRI disembunyikan dan dokter perujuk luar tercetak garis kosong

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Hasil verifikasi** | TERBUKTI SEBAGIAN |
| **Jenis** | Bagian A: `DESIGN_CHANGE` yang menunggu keputusan tarif. Bagian B: `DESIGN_CHANGE` |
| **Area** | Backend dan frontend Workspace PPRI |
| **Keparahan** | Bagian A: Medium. Bagian B: Low |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-EPS-043-03` (bagian A), `FIX-EPS-043-05` (bagian B) |

**Bagian A — Estimasi biaya.**

*Apa yang terjadi.* Menu Estimasi Biaya (`FE-INP-42`) pada Workspace PPRI tidak tampil. Petugas
admisi tidak dapat memberi perkiraan biaya kepada keluarga pasien.

*Kenapa terjadi.* Ini disengaja. Service kalkulasinya, `BE-RWI-203`, berstatus `⛔` karena manajemen
dan kasir belum menetapkan daftar tarif visit dokter spesialis, tindakan cito, sewa kamar operasi, dan
anestesi (`DEC-INP-020`, `RWI-OQ-122`; `Daftar-Pekerjaan-Belum-Selesai.md:166`). Layarnya, `FE-RWI-221`,
ikut disembunyikan sampai rumusnya disahkan (`Daftar-Pekerjaan-Belum-Selesai.md:167`).

*Kenapa usulan versi 1 tidak boleh dipakai.* Versi 1 memilih sendiri rumus *"(Hari × Kamar) + Paket
Operasi + Visit"*. Menetapkan rumus tarif adalah keputusan billing milik kasir dan manajemen, bukan
milik analisis issue. Rumus yang dikarang dapat membuat keluarga pasien menerima angka yang jauh dari
tagihan akhir.

**Bagian B — Dokter perujuk luar.**

*Apa yang terjadi.* Pasien datang dengan rujukan dokter dari luar rumah sakit. Petugas admisi sudah
mengetik nama dokter dan fasilitas perujuk. Namun pada lembar IPD, baris "Dokter perujuk" tercetak
garis kosong.

*Kenapa terjadi.*

1. Nama dokter dan fasilitas perujuk luar **sudah tersimpan** pada episode —
   `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs:83-84`:

   ```csharp
   [MaxLength(200)] public string? ExternalReferralFacilityName { get; set; }
   [MaxLength(150)] public string? ExternalReferralDoctorName { get; set; }
   ```

   Keduanya diisi dari form admisi (`InpEpisodeService.cs:1770-1771`) dan tampil pada langkah
   konfirmasi admisi
   (`src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx:477`).
   Kolom `ExternalReferralDoctorName` yang diusulkan versi 1 **sudah ada**.
2. Lembar IPD tidak membaca kolom episode itu. Ia membaca perujuk luar dari sumber Registrasi
   (`InpAdmissionWorkspaceQueryService.cs:463`, `:505-510`). Sumber itu selalu menjawab "belum
   tersedia" — `Areas/HealthServices/InPatientManagement/Services/InpAdmissionSourceReader.cs:383-391`:

   ```csharp
   => Task.FromResult(InpAdmissionSourceResult<InpAdmissionExternalReferral?>.NotYetAvailable(
       "Service baca dokter perujuk luar menunggu persetujuan pemilik Registration (RWI-OQ-128)."));
   ```

*Apakah ini menyimpang dari desain?* Tidak. Desainnya memang begitu. `RWI-DEC-254` menetapkan urutan
sumber "surat pengantar, lalu perujuk luar kunjungan (Registrasi), selain itu garis kosong", dan
`RWI-AC-386` menerima garis kosong selama `BE-RWI-190` tertahan. Memakai kolom milik episode sebagai
sumber tambahan adalah perubahan desain yang perlu keputusan pemilik (K-04).

**Dampak nyata.** Bagian A: petugas admisi menjawab pertanyaan biaya secara lisan dari tarif kamar
saja. Bagian B: lembar IPD Tn. C (pseudonim), rujukan dr. D dari Klinik E, menampilkan "Dokter perujuk:
_______", padahal nama dr. D sudah diketik saat admisi.

**Rekomendasi.** Bagian A tetap `⛔` sampai keputusan tarif disahkan (K-03); jangan mengarang rumus.
Bagian B: bila K-04 disetujui, tambahkan kolom episode sebagai sumber ketiga lembar IPD. Perubahan ini
kecil dan tidak membutuhkan kolom database baru.

---

### ISS-EPS-043-04 — Pemesanan kamar operasi dari bangsal dikira tidak sampai ke Kamar Operasi

| | |
| --- | --- |
| **No. laporan** | 4 |
| **Hasil verifikasi** | TIDAK TERBUKTI |
| **Jenis** | Tidak ada. Fitur sudah berjalan |
| **Area** | — |
| **Keparahan** | — |
| **Status bukti** | `SUDAH-VERIFIKASI` di source backend dan frontend |
| **Perbaikan** | `FIX-EPS-043-04` — dibatalkan |

**Apa yang sebenarnya terjadi di sistem.** Perawat bangsal membuka **Pelayanan Bedah (IBS)** pada
ruang kerja keperawatan, memilih order tindakan bedah, lalu memesan ruang bedah. Sistem langsung
membuat **kasus operasi berstatus Diminta** di modul Kamar Operasi. Daftar kasus beserta statusnya
(dijadwalkan, ditunda, dibatalkan, ditolak, dan seterusnya) tampil kembali di bangsal.

**Kenapa klaim versi 1 keliru.**

1. Endpoint pemesanan dari bangsal sudah ada —
   `Areas/HealthServices/InPatientManagement/Controllers/InpatientSurgeryBookingController.cs:58-64`:

   ```csharp
   /// <summary>Memesan ruang bedah; membuat kasus OK berstatus Diminta.</summary>
   [HttpPost("{episodeId:guid}/surgery-bookings")]
   ```

2. Adapter rawat inap memanggil service Kamar Operasi untuk membuat kasus
   (`InpSurgeryBookingAdapter.cs:138`, `_caseService.CreateFromWardBookingAsync`). Tugas ini milik
   `BE-RWI-175`.
3. Frontend menampilkan status kasus dari Kamar Operasi
   (`src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/patient-surgery-cases-table.jsx:150`).
4. `InpSurgeryBookingService.cs`, `IOprCaseService`, `CreateElectiveCaseAsync`, dan status
   `RequestedFromWard` yang disebut versi 1 tidak ada di source.

**Apakah ini menyimpang dari desain?** Tidak.

**Dampak nyata.** Tidak ada. Bila di lapangan pemesanan tidak muncul di Kamar Operasi, penyebab yang
lebih mungkin adalah hak akses `OperatingRoomCase : Create` pada peran perawat, atau order tindakan
yang belum berstatus `Planned`/`Ordered` (`InpSurgeryBookingAdapter.cs:50`). Hal ini ditanyakan kepada
pelapor (P-02).

**Rekomendasi.** Tidak ada perubahan source. `FIX-EPS-043-04` dibatalkan.

---

## 5. Tanya-jawab pelapor

> **T:** Mengapa pasien yang sudah siap pulang dan kasirnya belum selesai tidak boleh keluar kamar?
>
> **J:** Pasien itu boleh keluar kamar. Sejak `RWI-DEC-186`, perawat mencatat kepergian dengan
> mengonfirmasi peringatan kasir. Tempat tidur langsung masuk pembersihan, dan yang menunggu kasir
> hanya penutupan episode.

> **T:** Apakah estimasi biaya admisi mengikat kasir saat kepulangan?
>
> **J:** Belum dapat dijawab dari source, karena fitur estimasinya belum dibangun. Sifat mengikat atau
> tidaknya akan ditetapkan bersama rumusnya pada keputusan K-03.

---

## 6. Temuan tambahan

### ISS-EPS-043-T1 — Pembatalan reservasi tempat tidur dikira tanpa jejak pelaku

| | |
| --- | --- |
| **Hasil verifikasi** | TIDAK TERBUKTI |
| **Kenapa dicantumkan** | Dicantumkan versi 1 sebagai temuan saat memeriksa alur transfer bed |
| **Status bukti** | `SUDAH-VERIFIKASI` |

Pembatalan reservasi tempat tidur menyimpan alasan dan pelakunya
(`Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:557`:
`CancellationReason`, `CancelledByUserId`). Kedua kolom itu ikut tampil pada riwayat
(`InpBedOccupancyService.cs:1581`). Setiap perubahan status bed juga dicatat pada
`InpBedLifecycleEvent`. Tidak ada perbaikan.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Pada butir 1, apakah tombol Catat Kepergian benar-benar terlihat terkunci di layar? Bila ya, dengan akun dan peran apa? | Source tidak mengunci tombol karena kasir. Gejalanya mungkin berasal dari hak akses peran | `ISS-EPS-043-01` tetap ditutup sebagai tidak terbukti |
| P-02 | Pada butir 4, apakah ada pemesanan bedah dari bangsal yang tidak muncul di Kamar Operasi? Bila ada, sebutkan nomor episode dan order tindakannya | Source sudah membuat kasus OK. Bila ada kasus gagal, penyebabnya perlu ditelusuri dari data | `ISS-EPS-043-04` tetap ditutup sebagai tidak terbukti |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Siapa yang membangun kemampuan janji temu poliklinik, dan apakah rawat inap boleh memanggilnya? | (a) Pemilik Registrasi membangun janji temu baru; (b) reservasi prioritas diperluas; (c) tetap manual | (a) — reservasi prioritas sengaja tidak membuat antrean | `FIX-EPS-043-02` tahap 2 |
| K-02 | Penyedia dan kebijakan pengiriman WhatsApp ke pasien | Pilih penyedia / tunda | Tunda sampai K-01 selesai | `FIX-EPS-043-02` tahap 3 |
| K-03 | Pengesahan rumus estimasi biaya PPRI (`DEC-INP-020`, `RWI-OQ-122`), termasuk sifat mengikat atau tidaknya | Disahkan kasir dan manajemen | Ambil keputusan dari tarif nyata, bukan rumus contoh | `FIX-EPS-043-03` |
| K-04 | Apakah nama dokter perujuk luar yang diketik petugas admisi sah dipakai pada lembar IPD? | (a) Ya, sebagai sumber ketiga sesudah surat pengantar dan Registrasi; (b) Tidak, tunggu `BE-RWI-190` | (a) — data sudah ada dan diketik petugas berwenang | `FIX-EPS-043-05` |
| K-05 | Pengesahan pembatalan `FIX-EPS-043-01` dan `FIX-EPS-043-04` | Setuju / tolak | Setuju — fiturnya sudah berjalan | Penutupan issue |

---

## 8. Catatan pola

Tiga dari empat butir versi 1 disusun dari dokumen roadmap dan analisis, bukan dari source. Akibatnya
dua fitur yang sudah berjalan dilaporkan rusak. Ada juga usulan yang mengarang keputusan billing.
Pencegahan untuk issue berikutnya:

1. setiap `path:baris` dibuka dan dibaca sebelum ditulis;
2. setiap konflik `RWI-CON-*` diperiksa status penutupannya pada tabel ringkasan
   `00-interview-decisions.md`;
3. setiap usulan rumus tarif dirujuk ke keputusan pemilik, tidak ditulis sebagai solusi terpilih.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-02 | Versi 1: issue diterbitkan dari analisis 4 titik putus bisnis proses rawat inap | AI Agent (`diagnose-module-issue`) |
| 2026-11-02 | Revisi 2: verifikasi ulang terhadap source SHA yang sama. `ISS-EPS-043-01`, `ISS-EPS-043-04`, dan `ISS-EPS-043-T1` dinyatakan tidak terbukti. `ISS-EPS-043-02` terbukti, tetapi rekomendasinya diganti karena service janji temu dan gateway WhatsApp tidak ada. `ISS-EPS-043-03` dipecah: bagian A ditetapkan menunggu keputusan tarif (rumus karangan versi 1 ditarik), bagian B dikoreksi karena kolom perujuk luar sudah ada. Keparahan tertinggi turun dari Blocker ke Medium. Pertanyaan P-01, P-02 dan keputusan K-01 s.d. K-05 ditambahkan | AI Agent (`diagnose-module-issue`) atas permintaan pemilik |
