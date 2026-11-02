# ISSUE-041 — Temuan live testing Admisi Pasien Lama, Bayi Baru Lahir, dan Transfer IGD & Poliklinik (26 Oktober 2026)

```yaml
issue_id: ISSUE-EPS-041
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Admisi Rawat Inap (FE-INP-03) — jalur Pendaftaran Pasien Lama (/admissions/existing-patient), Admisi Bayi Baru Lahir (/admissions/newborn), dan Admisi Transfer IGD & Poliklinik (/admissions/transfer)"
sumber_laporan: "Tiga laporan live browser testing 26-10-2026 di testing/live-testing/: laporan-live-testing-admisi-pasien-lama.md (27 test case), laporan-live-testing-admisi-bayi-baru-lahir-20261026.md (22 test case), laporan-live-testing-admisi-transfer-igd-poliklinik.md (15 test case); 15 temuan, 207 screenshot"
tanggal_issue: "2026-10-26"
status: TERBUKA
keparahan_tertinggi: High
source_sha_backend: "e095f656 (MHamzah)"
source_sha_frontend: "2cd1c0610 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-041-temuan-uji-admisi-lama-bayi-transfer.md
```

---

## 1. Ringkasan

Issue ini menganalisis 15 temuan dari uji langsung di browser terhadap tiga jalur Admisi Rawat Inap: Pasien Lama, Bayi Baru Lahir, dan Transfer IGD & Poliklinik. Seluruh jalur dapat diselesaikan dan tidak ada galat server, tetapi empat temuan berkeparahan **High**: pasien IGD yang butuh isolasi ditawari bangsal biasa dan baru ditolak di langkah terakhir, jalur transfer tidak memeriksa apakah pasien sedang dirawat, konsulen dan tingkat ketergantungan pada jalur transfer hilang tanpa peringatan, serta RM bayi yang identitasnya sudah tersimpan tidak bisa dilanjutkan admisinya. Tiga temuan ternyata **sesuai desain atau kontrak yang disetujui**, sehingga yang dibutuhkan adalah keputusan pemilik, bukan perbaikan kode: admisi Draft ganda pasien lama (sudah ditangani ISSUE-EPS-022), admisi transfer yang dibatalkan tidak kembali ke antrean (kontrak EPS-006/K-02), dan kode situs di label bayi (pengaturan G-36). Penelusuran juga mengoreksi dua penilaian di laporan uji: footer melayang adalah perilaku layout global, dan tombol Lanjut yang tetap aktif pada pasien yang sedang dirawat memang disengaja.

---

## 2. Laporan asli

Pelapor adalah AI Agent penguji (live browser testing, 26-10-2026). Nomor laporan memakai ID temuan pada ketiga laporan uji.

| No. laporan | Keluhan (diringkas dari laporan uji) | Lampiran yang merujuk |
| --- | --- | --- |
| TR-01 | Pasien IGD suspek TB yang butuh isolasi: penanda isolasi tidak terbawa, denah menawarkan bangsal biasa, master tidak punya bed isolasi, ditolak 422 `BED_NOT_READY` di langkah 7 | `trf-50…`, `trf-53…`, `trf-55…`, `trf-58…` |
| TR-02 | Jalur transfer tidak memeriksa kelayakan; pasien yang sedang dirawat baru ditolak 409 di langkah 7 | `trf-10…`, `trf-12…`, `trf-16…` |
| TR-03 | Admisi transfer yang dibatalkan tidak kembali ke antrean | `trf-70…`, `trf-71…`, `trf-72…` |
| TR-04 | Konsulen dan tingkat ketergantungan tidak tersimpan pada jalur transfer | `trf-22c…`, `trf-52…`, `trf-65…` |
| TR-05 | Data rujukan IGD/Poli tidak terbawa (DPJP usulan IGD, kelas diminta Poli, diagnosis, kebutuhan O2/monitoring) | `trf-20a…`, `trf-40a…` |
| TR-06 | Teks, status, dan kode galat jalur transfer belum rapi | `trf-27…`, `trf-16…`, `trf-50a…`, `trf-32…` |
| TR-07 / PL-04 / BY-03 | Bilah footer melayang menutupi isian form | `ext-36…`, `trf-22c…` |
| BY-01 | RM bayi tanpa episode; tidak ada jalur melanjutkan; urutan "Bayi ke-n" menggelembung | `nwb-60b…`, `nwb-61…` |
| BY-02 | Catatan kecil jalur bayi: lantai denah bawaan, tanggal masa depan di picker, "Kelas KELAS I", format waktu lahir, kode situs di label, unit NICU, kode bed | `nwb-04…`, `nwb-15…`, `nwb-16…`, `nwb-26…`, `nwb-30…`, `nwb-45…` |
| PL-01 | Admisi kedua dapat dibuat untuk pasien yang masih punya admisi Draft; peringatan memakai jam UTC dan bulan berbahasa Inggris | `ext-43b…` |
| PL-02 | Nominal deposit tanpa batas atas wajar dan kehilangan presisi | `ext-24a…`–`ext-24c…` |
| PL-03 | Dua kamar berlabel "ODC 321" di denah Lantai 3 | `ext-27b…` |
| PL-05 | Catatan istilah dan umpan balik jalur Pasien Lama (9 butir) | `ext-05…`, `ext-08…`, `ext-27…`, `ext-32…`, `ext-34…`, `ext-41…` |
| TR-TC-xx (lulus) | Bukti runtime untuk perbaikan ISSUE-EPS-040 | `nwb-…` (lihat bagian 8) |

Seluruh lampiran berada di `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/`. Tidak ada lampiran yang tidak dirujuk butir mana pun.

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-041-01` | TR-01 | Pasien butuh isolasi ditawari bangsal biasa, baru ditolak di langkah 7 | `BUG`, `CONTRACT_MISMATCH`, `DATA` | FE + BE + Data | High | SUDAH-VERIFIKASI | `FIX-EPS-041-01`–`05` |
| `ISS-EPS-041-02` | TR-02 | Verifikasi transfer tidak memeriksa kelayakan pasien | `DESIGN_CHANGE` | FE | High | SUDAH-VERIFIKASI | `FIX-EPS-041-06` |
| `ISS-EPS-041-03` | TR-03 | Admisi transfer yang dibatalkan tidak dapat diproses ulang, tetapi layar menyarankan "batalkan dan buka ulang" | `BUG` (teks); perilaku sesuai kontrak | FE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-041-07` |
| `ISS-EPS-041-04` | TR-04 | Konsulen dan tingkat ketergantungan hilang pada jalur transfer | `BUG` | FE | High | SUDAH-VERIFIKASI | `FIX-EPS-041-08` |
| `ISS-EPS-041-05` | TR-05 | Data rujukan IGD/Poli tidak terbawa ke Verifikasi dan isian otomatis | `CONTRACT_MISMATCH` | BE + FE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-041-01`, `02` |
| `ISS-EPS-041-06` | TR-06 | Teks, status, dan kode galat jalur transfer | `BUG` | FE + BE | Low | SUDAH-VERIFIKASI (butir 6: DUGAAN) | `FIX-EPS-041-04`, `09` |
| `ISS-EPS-041-07` | BY-01 | RM bayi tanpa episode tidak bisa dilanjutkan; urutan kelahiran menggelembung | `BUG`, `DATA` | FE + BE + Data | High | SUDAH-VERIFIKASI; data dari capture | `FIX-EPS-041-10`, `11` |
| `ISS-EPS-041-08` | BY-02 | Catatan kecil jalur bayi | `BUG`, `DATA` | FE + Data | Low | SUDAH-VERIFIKASI | `FIX-EPS-041-12`, `13` |
| `ISS-EPS-041-09` | PL-01 | Admisi Draft ganda dan format waktu peringatan | `DESIGN_CHANGE` (ditangani ISSUE-EPS-022), `BUG` (format) | BE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-041-14`; `FIX-EPS-022-01` |
| `ISS-EPS-041-10` | PL-02 | Nominal deposit tanpa batas atas wajar | `BUG` | FE + BE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-041-15` |
| `ISS-EPS-041-11` | PL-03 | Dua kamar berlabel "ODC 321" | `DATA` | Data | Low | DARI-CAPTURE | `FIX-EPS-041-16` |
| `ISS-EPS-041-12` | PL-05 | Catatan istilah dan umpan balik jalur Pasien Lama | `BUG`, `DESIGN_CHANGE` | FE + BE | Low | SUDAH-VERIFIKASI (2 butir sesuai desain) | `FIX-EPS-041-17`, `18` |
| `ISS-EPS-041-T1` | TR-07 / PL-04 / BY-03 | Footer melayang menutupi isian saat digulir | catatan, milik layout global | FE (layout) | Low | SUDAH-VERIFIKASI | Tidak ada — diteruskan ke pemilik layout |

---

## 4. Rincian per temuan

### ISS-EPS-041-01 — Pasien butuh isolasi ditawari bangsal biasa, baru ditolak di langkah 7

| | |
| --- | --- |
| **No. laporan** | TR-01 |
| **Jenis** | `BUG` (denah transfer tanpa konteks pasien), `CONTRACT_MISMATCH` (antrean tidak membawa kebutuhan isolasi), `DATA` (tidak ada bed isolasi) |
| **Area** | Frontend, Backend, Data master |
| **Keparahan** | High — server pasti menolak bed yang ditawarkan layar, dan jalan pintas petugas membahayakan pasien lain |
| **Status bukti** | `SUDAH-VERIFIKASI` di source; jumlah bed isolasi `DARI-CAPTURE` (kueri baca-saja saat uji) |
| **Perbaikan** | `FIX-EPS-041-01`, `02`, `03`, `04`, `05` |

**Apa yang terjadi.** Dokter IGD meminta rawat inap untuk Tn. R (pseudonim), suspek TB paru, dengan tanda "butuh isolasi". Di layar admisi transfer, tanda itu tidak terlihat di antrean maupun di Verifikasi, dan sakelar isolasi di langkah Dokter tidak tercentang. Setelah petugas menyalakannya sendiri, denah tetap menawarkan bangsal Kelas II tiga-bed (termasuk kamar yang sudah dipesan untuk pasien lansia pneumonia), kamar ibu nifas, dan kamar ODC. Penolakan baru datang saat "Ya, Pesan": "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat tidur isolasi." Di master data, tidak ada satu pun bed isolasi, jadi pasien ini tidak bisa diadmisi ke mana pun.

**Kenapa terjadi.**

```text
denah transfer menawarkan bangsal biasa untuk pasien isolasi
  ← sebab langsung: bed transfer dianggap layak cukup karena muncul di available-beds
     use-inpatient-admission-bed.jsx:166-178  isSelectable = Boolean(serverAvailable)
    ← available-beds dipanggil tanpa episode dan tanpa konteks pasien
       use-inpatient-admission-bed.jsx:132-141  useInpatientBedBoard({ episodeId, …, newbornContext })
       use-inpatient-bed-board.jsx:87-101       patientContext hanya diisi dari newbornContext
      ← akar: ISSUE-EPS-024 memasang denah transfer dengan asumsi available-beds "sudah diloloskan
         server", padahal tanpa konteks pasien server hanya membuang bed yang sedang dipegang
         use-inpatient-bed-board.jsx:56-62 (komentar: "Tanpa episodeId, available-beds menjawab
         dengan arti yang berbeda dan hasilnya menyesatkan")
```

Server sebenarnya sudah mampu menilai kelayakan tanpa episode, asal diberi `patientId` dan `requiresIsolation` — jalur bayi sudah memakainya:

`NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:135-146`

```csharp
if (episode == null && query.PatientId.HasValue)
{
    patient = await _dbContext.Set<MstPatient>().AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == query.PatientId.Value && !x.IsDelete && x.IsActive, cancellationToken);
    ...
    episode = new InpEpisode
    {
        PatientId = patient.Id, MotherEpisodeId = query.MotherEpisodeId,
        NewbornCareMode = query.NewbornCareMode,
        RequiresNeonatalIntensiveCare = query.RequiresIntensiveCare,
        RequiresIsolation = query.RequiresIsolation
    };
}
```

`QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx:384-398` (`buildAvailableBedQuery`) sudah mengirim `patientId` dan `requiresIsolation` bila `patientContext` berisi `patientId` — jalur transfer saja yang tidak mengisinya.

Rantai kedua, kebutuhan isolasi tidak terbawa:

- `InpAdmissionTransferService.Queries.cs:33-85` memetakan `IsCito` dari `EmgAdmissionRequest`, tetapi tidak memetakan `NeedsIsolation`, `NeedsIntensiveCare`, `NeedsOxygenSupport`, maupun `NeedsSpecialMonitoring`.
- `InpatientAdmissionTransferDtos.cs:36-75` tidak memiliki kolom untuk kebutuhan itu.
- `use-inpatient-admission-doctor.jsx:205-212` mengisi awal form dokter dari sumber transfer (DPJP, unit, catatan) tetapi tidak menyentuh `requiresIsolation`.

Rantai ketiga, kode galat hilang: `InpAdmissionTransferService.cs:152-154` membungkus semua kegagalan reservasi sebagai `BED_NOT_READY`, sehingga kode asli Aturan 7 `ISOLATION_REQUIRED` (`InpBedOccupancyService.cs:1748-1756`) tidak sampai ke layar.

Rantai keempat, data: kueri baca-saja saat uji pada `MstBed` dengan `IsIsolationBed = true` mengembalikan 0 baris (`DARI-CAPTURE`, laporan uji transfer bagian 5 TR-01). Aturan "pasien isolasi hanya di bed isolasi" sendiri adalah keputusan sah (`InpBedOccupancyService.cs:1745-1765`, "kapasitas isolasi terjaga dua arah").

**Apakah ini menyimpang dari desain?** Ya. ISSUE-EPS-024 (status SELESAI) menetapkan overlay kelayakan denah transfer dari "tempat tidur yang sudah diloloskan oleh server pada `GET /bed-occupancies/available-beds`" (`docs/issue/issue-024-pemilihan-bed-denah-admisi-transfer.md:190`). Karena konteks pasien tidak dikirim, server tidak meloloskan apa pun menurut aturan pasien. Bagian ini `BUG`. Ketiadaan kebutuhan isolasi di respons antrean adalah `CONTRACT_MISMATCH` terhadap data sumber IGD yang sudah ada. Ketiadaan bed isolasi adalah `DATA`.

**Dampak nyata.** Petugas jaga malam yang terus ditolak cenderung mematikan sakelar isolasi supaya bisa memesan bed. Uji ini membuktikan jalan pintas itu berhasil: tanpa sakelar, admisi terbentuk di kamar VIP (kemudian dibatalkan penguji). Bila yang dipilih bangsal Kelas II, pasien TB airborne dirawat sekamar dengan dua pasien lain.

**Rekomendasi.** Kirim konteks pasien dari jalur transfer ke `available-beds` (reuse mekanisme `patientContext` jalur bayi), bawa kebutuhan IGD ke respons antrean dan isi otomatis sakelar isolasi, teruskan kode galat asli, dan tetapkan bed isolasi di master data (keputusan K-01). Rincian di plan repair.

---

### ISS-EPS-041-02 — Verifikasi transfer tidak memeriksa kelayakan pasien

| | |
| --- | --- |
| **No. laporan** | TR-02 |
| **Jenis** | `DESIGN_CHANGE` (perluasan cakupan pemeriksaan kelayakan ke jalur transfer) |
| **Area** | Frontend |
| **Keparahan** | High — server pasti menolak admisi yang ditawarkan layar setelah enam langkah diisi |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-EPS-041-06` |

**Apa yang terjadi.** Surat pengantar rawat inap Poli untuk Ny. S (pseudonim), yang ternyata sedang dirawat di kamar VIP, muncul di antrean tanpa peringatan. Layar Verifikasi menampilkan lencana "TERDAFTAR" dan membolehkan lanjut. Petugas mengisi pembayaran, deposit, DPJP, dan memilih bed; baru setelah "Ya, Pesan" server menolak dengan 409 `PATIENT_ALREADY_ADMITTED`. Data tetap aman karena server menolak sebelum menulis apa pun.

**Kenapa terjadi.**

`QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx:635-651`

```jsx
/**
 * `FE-RWI-244` — kelayakan admisi pasien terpilih (`PLAN-REPAIR-EPS-010`, `RWI-DEC-297`).
 * Hanya hidup pada langkah yang memilih pasien untuk admisi baru: Pasien Lama langkah 1 dan
 * Informasi Pasien jalur Kamar Pulih. Alur pelanjutan tidak memeriksa ulang karena episodenya
 * sudah ada; jalur transfer punya pemeriksaan sendiri.
 */
const isEligibilityStep =
  !flow.resumeEpisodeId &&
  (flow.currentStep?.slug === "existing-patient" ||
    flow.currentStep?.slug === "existing-patient-information");
```

"Pemeriksaan sendiri" jalur transfer ternyata hanya memeriksa status sumber, bukan status pasien: `inpatient-admission-transfer-utils.js:7-19` (`validateTransferSource`) menolak sumber yang tidak dikenal, pasien yang tidak cocok, atau sumber yang sudah diproses — tidak ada pemeriksaan episode yang sedang hadir. Lencana Verifikasi juga ditulis mati: `inpatient-admission-transfer-review-step.jsx:436-443` (`statusLabel="PASIEN TERDAFTAR"`, `badgeText="TERDAFTAR"`).

**Apakah ini menyimpang dari desain?** Tidak sepenuhnya. Amandemen PLAN-REPAIR-EPS-010 pada `05-skema-tampilan.md:1852-1866` hanya mencakup Pasien Lama dan Kamar Pulih. Karena itu jenisnya `DESIGN_CHANGE`: cakupan amandemen perlu diperluas ke langkah Verifikasi transfer. Perluasan ini sejalan dengan prinsip "validasi layar mencerminkan aturan server" dan tidak menyentuh keputusan bisnis baru — aturan servernya (`FindPresentEpisodeAsync`, BE-RWI-230) sudah berlaku.

**Dampak nyata.** Dokter Poli keliru menerbitkan surat pengantar untuk pasien yang sudah dirawat di lantai lain. Petugas admisi baru tahu setelah sekitar tiga menit mengisi formulir dan membicarakan deposit dengan keluarga.

**Rekomendasi.** Pakai ulang `useInpatientAdmissionEligibility` di langkah Verifikasi transfer dengan alert dan tombol tindak lanjut yang sama seperti Pasien Lama, dan lencana yang mengikuti hasil pemeriksaan.

---

### ISS-EPS-041-03 — Admisi transfer yang dibatalkan tidak dapat diproses ulang, tetapi layar menyarankan "batalkan dan buka ulang"

| | |
| --- | --- |
| **No. laporan** | TR-03 |
| **Jenis** | `BUG` pada teks layar; perilaku servernya **sesuai kontrak** |
| **Area** | Frontend |
| **Keparahan** | Medium — alur dapat diselesaikan, tetapi petugas diarahkan ke langkah yang tidak tersedia |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-EPS-041-07` |

**Apa yang terjadi.** Setelah admisi transfer Tn. R dibatalkan dengan alasan, episode menjadi *Cancelled* dan bed dilepas, tetapi permintaan IGD-nya hilang dari antrean. Padahal layar Konfirmasi berkata: "DPJP dan penjamin sudah melekat pada kunjungan. Jika keliru, admisi harus dibatalkan dan dibuka ulang." Pada jalur transfer, "dibuka ulang" tidak mungkin dari antrean.

**Kenapa terjadi.** Perilaku server disengaja:

- `contracts/admission-transfer-repair-006.md:10` (EPS-006/K-02): "Satu sumber hanya boleh membentuk satu kunjungan hasil, termasuk bila hasil kemudian dibatalkan."
- `InpAdmissionTransferService.Queries.cs:110` dan `InpAdmissionTransferService.cs:112-115` memakai `IgnoreQueryFilters()` sehingga kunjungan hasil yang dibatalkan tetap dihitung, dan `admit` ulang ditolak `TRANSFER_ALREADY_PROCESSED`.
- `InpEpisodeService.cs:1071-1100` (`CancelEpisodeInternalAsync`) membatalkan episode dan kunjungan jangkar, tanpa mengubah disposisi IGD.

Yang menyimpang adalah teksnya: `inpatient-admission-confirmation-step.jsx:605` menampilkan saran "dibatalkan dan dibuka ulang" untuk semua jalur, termasuk transfer; dialog pembatalan juga tidak memberi tahu bahwa permintaan IGD/Poli tidak akan kembali ke antrean.

**Apakah ini menyimpang dari desain?** Perilaku server tidak menyimpang (kontrak EPS-006/K-02, disahkan 12-10-2026). Teks layar menyimpang dari kontrak itu, sehingga jenisnya `BUG`. Laporan uji menilai butir ini "P1 · Tinggi"; penilaian itu dikoreksi menjadi Medium karena kontraknya memang demikian.

**Dampak nyata.** Petugas salah memilih DPJP untuk Tn. A (pseudonim, pasien IGD CITO), lalu mengikuti saran layar dan membatalkan admisi. Permintaan IGD lenyap dari antrean; petugas harus menelepon IGD agar dokter jaga menerbitkan disposisi baru, sementara pasien CITO menunggu.

**Rekomendasi.** Pada jalur transfer, ganti saran di Konfirmasi dan tambahkan peringatan di dialog pembatalan bahwa permintaan tidak kembali ke antrean dan IGD/Poli perlu menerbitkan permintaan baru. Bila pemilik justru ingin permintaan kembali ke antrean, itu perubahan kontrak (K-02).

---

### ISS-EPS-041-04 — Konsulen dan tingkat ketergantungan hilang pada jalur transfer

| | |
| --- | --- |
| **No. laporan** | TR-04 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High — data yang diisi petugas tidak tersimpan tanpa disadari |
| **Status bukti** | `SUDAH-VERIFIKASI` di source dan DB uji |
| **Perbaikan** | `FIX-EPS-041-08` |

**Apa yang terjadi.** Pada dua admisi transfer, petugas memilih tingkat ketergantungan (High dan Low) dan pada salah satunya menambahkan konsulen bedah. Setelah admisi tersimpan, episode hanya memiliki DPJP utama; tingkat ketergantungan kosong dan konsulen tidak ada. Layar Konfirmasi juga tidak menampilkan konsulen. Pada jalur Pasien Lama, tingkat ketergantungan yang sama tersimpan dengan benar.

**Kenapa terjadi.**

```text
konsulen dan ketergantungan tidak ada di episode transfer
  ← payload admit hanya membawa DPJP, catatan, dan isolasi
     inpatient-admission-transfer-utils.js:21-44 (buildTransferAdmissionPayload)
    ← setelah admit, layar hanya mengadopsi episode, tanpa penyimpanan lanjutan
       use-inpatient-admission-doctor.jsx:955-960 (adoptTransferEpisode)
      ← akar: penyimpanan dokter pendukung dan ketergantungan hanya ada di titik tulis jalur
         biasa, sesudah episode Draft terbentuk
         use-inpatient-admission-doctor.jsx:811-830 (Langkah 2b — dokter pendukung)
         use-inpatient-admission-doctor.jsx:863-880 (tingkat ketergantungan)
```

**Apakah ini menyimpang dari desain?** Ya. Langkah Dokter ditampilkan sama untuk semua jalur (`05-skema-tampilan.md` 3.8), termasuk isian konsulen dan ketergantungan; tidak ada desain yang membuang isian itu pada jalur transfer.

**Dampak nyata.** Tn. R (pseudonim) masuk dengan rencana konsul bedah toraks. Konsulen tidak pernah mendapat penugasan di episode, sehingga tidak muncul di daftar pasien konsulen; perawat ruangan menyusun beban kerja tanpa tahu pasien ini *High Dependency*.

**Rekomendasi.** Setelah `admit` berhasil, jalankan penyimpanan dokter pendukung dan tingkat ketergantungan yang sama dengan jalur biasa, lalu tampilkan konsulen di Konfirmasi.

---

### ISS-EPS-041-05 — Data rujukan IGD/Poli tidak terbawa ke Verifikasi dan isian otomatis

| | |
| --- | --- |
| **No. laporan** | TR-05 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Backend (respons antrean), Frontend (label dan isian otomatis) |
| **Keparahan** | Medium — DPJP terisi dokter yang tidak tepat; petugas harus mengoreksi manual |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-EPS-041-01`, `FIX-EPS-041-02` |

**Apa yang terjadi.** Di layar Verifikasi, kolom "DPJP DITUJU / PEMERIKSA" berisi dokter jaga IGD yang memutuskan rawat inap (dokter ortopedi), dan dokter itu pula yang terisi otomatis sebagai DPJP untuk pasien suspek NSTEMI. DPJP rawat inap yang diusulkan IGD (dokter umum lain) tidak tampil. Kolom "DIAGNOSA / INDIKASI RAWAT INAP" berisi alasan rujukan, sedangkan diagnosis surat pengantar Poli ("Demam Berdarah Dengue") dan kelas kamar yang diminta ("Kelas I") tidak tampil sama sekali.

**Kenapa terjadi.**

| Data sumber | Pemetaan sekarang | Bukti |
| --- | --- | --- |
| DPJP usulan IGD (`EmgAdmissionRequest.ProposedInpatientDoctorId`) | Tidak dipetakan; yang dikirim `DecidedByDoctorId` (dokter jaga) | `InpAdmissionTransferService.Queries.cs:43-44`; isian otomatis `use-inpatient-admission-doctor.jsx:210` |
| Diagnosis surat pengantar (`CliDoctorCertificate.Diagnosis` / `ReferralDiagnosis`) | Tidak dipetakan; label "Diagnosa" menampilkan `dispositionReason` (= `ReferralReason`) | `Queries.cs:106`; `inpatient-admission-transfer-review-step.jsx:352-355`, `:478-484` |
| Kelas kamar diminta (`RequestedRoomClass`) | Tidak dipetakan | `Queries.cs:97-143` |
| Kebutuhan O2, monitoring, intensif, perkiraan lama rawat | Tidak dipetakan | `Queries.cs:33-85` |
| `Gender`, `NationalId`, `PhoneNumber` | Kolom ada di DTO tetapi tidak pernah diisi query | `InpatientAdmissionTransferDtos.cs:61-67` |

**Apakah ini menyimpang dari desain?** Ya untuk DPJP: kontrak EPS-006/K-01 meminta isian awal dari rujukan yang benar (`plan-repair-006` FIX-05, "rujukan dr. Andi/unit Melati mengisi kontrol operasional"). Untuk diagnosis dan kelas, datanya sudah ada di sumber tetapi kontrak respons belum memuatnya.

**Dampak nyata.** Pasien jantung dari IGD terdaftar dengan DPJP ortopedi karena petugas tidak memperhatikan isian otomatis; visite pertama dilakukan dokter yang salah.

**Rekomendasi.** Tambahkan kolom yang hilang ke respons antrean (BE), tampilkan di Verifikasi dengan label yang benar, dan isi DPJP dari DPJP usulan (FE).

---

### ISS-EPS-041-06 — Teks, status, dan kode galat jalur transfer

| | |
| --- | --- |
| **No. laporan** | TR-06 |
| **Jenis** | `BUG` |
| **Area** | Frontend dan Backend |
| **Keparahan** | Low |
| **Status bukti** | `SUDAH-VERIFIKASI` untuk butir 1–5; butir 6 `DUGAAN` |
| **Perbaikan** | `FIX-EPS-041-04`, `FIX-EPS-041-09` |

| No | Gejala | Sebab | Bukti |
| ---: | --- | --- | --- |
| 1 | Status bed di Konfirmasi tampil "Active" | Server mengisi `ReservationStatusName` dari nama enum; layar menampilkan apa adanya, padahal helper label Indonesia sudah ada | `InpBedOccupancyService.cs:1587-1588`; `inpatient-admission-confirmation-step.jsx:389`, `:432-433`; helper `inpatient-bed-utils.jsx:489-492` |
| 2 | Judul "Gagal Memuat Permintaan Rawat Inap" untuk penolakan simpan | Satu state `error` dipakai untuk galat muat dan galat simpan, dengan satu judul | `use-inpatient-admission-transfer.js:51`, `:92`; `inpatient-admission-view.jsx:1286-1288` |
| 3 | Kode galat isolasi tampil `BED_NOT_READY` | Kegagalan reservasi dibungkus satu kode | `InpAdmissionTransferService.cs:152-154` |
| 4 | "Catatan Disposisi: ." | Pembersih sisa teks setelah TTV diurai hanya membuang koma, titik koma, dan spasi | `inpatient-admission-transfer-review-step.jsx:99-102` |
| 5 | `admit` menjawab HTTP 201 dengan `statusCode` 200 di badan | `StatusCode(201, ApiResponse.Ok(...))`; `Ok` mengisi 200 | `InpatientAdmissionTransferAdmitController.cs:22`, `:29` |
| 6 | Membuka ulang URL transfer yang sudah lengkap kembali ke Konfirmasi dan meminta "Lengkapi persetujuan" walau persetujuan sudah tersimpan | `DUGAAN`: langkah pelanjutan transfer ditentukan tanpa membaca status persetujuan. Cara membuktikan: buka `…/transfer?step=transfer-patient-review&transferId=<id>` untuk episode yang persetujuannya sudah ada, periksa apakah `GET …/consent` dipanggil sebelum langkah ditentukan | `trf-32-url-transfer-sudah-diproses` |

Catatan: nomor kunjungan hasil transfer berformat `ENC-RSMMC-<yyMMddHHmmss>-<acak>` (`InpEpisodeService.cs:1632`), berbeda dari pendaftaran kunjungan biasa (berurut). Pembentukan nomor dari waktu sudah ditangani `FIX-EPS-040-22`; penyeragaman formatnya di luar cakupan issue ini dan dicatat pada bagian 8.

**Apakah ini menyimpang dari desain?** Butir 1, 2, dan 4 melanggar daftar periksa UI (teks Inggris mentah, pesan yang menyesatkan). Butir 3 dan 5 tidak sesuai pola respons API. Semuanya `BUG`.

**Rekomendasi.** Pakai helper label status yang sudah ada, pisahkan galat muat dan galat simpan, bersihkan tanda baca sisa, teruskan kode galat asli, dan samakan kode status badan respons.

---

### ISS-EPS-041-07 — RM bayi tanpa episode tidak bisa dilanjutkan; urutan kelahiran menggelembung

| | |
| --- | --- |
| **No. laporan** | BY-01 |
| **Jenis** | `BUG` (alur pelanjutan tidak ada), `DATA` (RM bayi yatim yang sudah terbentuk) |
| **Area** | Frontend, Backend, Data |
| **Keparahan** | High — data tersimpan salah tanpa disadari (RM ganda, urutan kelahiran salah) |
| **Status bukti** | `SUDAH-VERIFIKASI` di source; jumlah RM yatim `DARI-CAPTURE` |
| **Perbaikan** | `FIX-EPS-041-10`, `FIX-EPS-041-11` |

**Apa yang terjadi.** Langkah 2 (Identitas Bayi) langsung membuat pasien bayi dan No. RM. Bila petugas meninggalkan admisi sebelum langkah Penempatan, pasien bayi itu tertinggal tanpa kunjungan dan episode. Saat ibu yang sama dipilih lagi, bayi lama tampil "Terdaftar" dan layar menyebutnya "sudah terdaftar di rawat inap" serta "telah selesai diadmisi". Tidak ada tombol untuk melanjutkan; satu-satunya jalan adalah mencentang "bayi kembar (Bayi ke-2)", yang membuat RM baru. Saat uji, DB pengembang memuat 11 pasien bayi tanpa kunjungan maupun episode; satu ibu tercatat memiliki enam "bayi".

**Kenapa terjadi.**

```text
petugas hanya bisa membuat "Bayi ke-2" untuk bayi yang sama
  ← gerbang kembar menghitung semua bayi terdaftar, termasuk yang belum punya episode
     use-inpatient-newborn-admission.js:259-264
     inpatient-newborn-admission-steps.jsx:70-72, :112-127
    ← jumlah dan pratinjau urutan dari server tidak membedakan bayi yang sudah diadmisi
       InpEpisodeService.NewbornCandidates.cs:218, :273-276
       (respons sebenarnya sudah membawa episodeId: null per bayi — registeredBabies[])
      ← urutan kelahiran diambil dari sekuens per kasus persalinan yang selalu naik
         NewbornPatientRegistrationService.cs:209-232 (PatNewbornBirthSequence)
        ← akar: identitas bayi disimpan sebelum admisi selesai (desain RWI-BAYI-022 langkah 2),
           tetapi desain itu tidak menyediakan jalur melanjutkan bayi yang identitasnya sudah ada
```

**Apakah ini menyimpang dari desain?** Penyimpanan identitas di langkah 2 sesuai desain (`05-skema-tampilan.md`, amandemen `RWI-BAYI-022-2.0.0`). Ketiadaan jalur melanjutkan adalah celah desain yang menyebabkan data salah, sehingga dicatat sebagai `BUG`. Teks "telah selesai diadmisi" bertentangan dengan data `episodeId: null`.

**Dampak nyata.** Bidan menyimpan identitas bayi Ny. F (pseudonim), lalu dipanggil ke Kamar Bersalin sebelum memesan boks. Petugas shift berikutnya mengulang dari awal dan mendaftarkan bayi yang sama sebagai "Bayi ke-2" dengan RM baru. Hasil laboratorium dan resep bayi terpecah di dua RM, dan gelang mencetak "BAYI 2" untuk anak tunggal.

**Rekomendasi.** Tampilkan bayi tanpa episode sebagai "Admisi belum selesai" dengan tombol "Lanjutkan admisi bayi ini" yang memakai pasien bayi yang sudah ada; gerbang kembar dan pratinjau urutan hanya menghitung bayi berbeda. Penanganan 11 RM yatim yang sudah ada diputuskan bersama Rekam Medis (K-03).

---

### ISS-EPS-041-08 — Catatan kecil jalur bayi

| | |
| --- | --- |
| **No. laporan** | BY-02 |
| **Jenis** | `BUG` (butir 1–4), `DATA` (butir 5–6) |
| **Area** | Frontend; data/pengaturan |
| **Keparahan** | Low |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-EPS-041-12`, `FIX-EPS-041-13` |

| No | Gejala | Sebab | Bukti |
| ---: | --- | --- | --- |
| 1 | Tanggal masa depan dapat diklik di picker tanggal lahir; daftar tahun menawarkan 2027–2031 | Field tidak mengirim batas, padahal `BaseDateField` mendukung `field.min`/`field.max` | `inpatient-newborn-admission-steps.jsx:196-197`; `base-form-control.jsx:500-535`; batas tahun bawaan `filter-date-picker.jsx:19-20` |
| 2 | "Kelas KELAS I · KELAS I 330" | Kata "Kelas" ditambahkan di depan nama kelas yang sudah memuat kata itu | `inpatient-newborn-admission-steps.jsx:400`, `:444` |
| 3 | Waktu lahir tampil "2026-10-26 · 09:30:00" | Tanggal ISO dipotong apa adanya, tanpa format tampilan | `inpatient-newborn-admission-steps.jsx:178`, `:710`, `:754` |
| 4 | Denah Perinatologi/NICU terbuka di lantai kamar dewasa | Lantai awal dibaca dari lantai terakhir yang disimpan peramban, tanpa memperhatikan mode perawatan | `use-floor-plan.js:31-45`, `:235-237` |
| 5 | Label pasien memuat "SITE-MMC-001" | Sesuai desain G-36: kode RS di label memakai `PatientLabelHospitalCode`, bila kosong jatuh ke `SiteCode` situs | `InpAdmissionWorkspaceQueryService.cs:385`; `02-backend-architecture.md:2409` |
| 6 | Bed NICU tercatat pada unit "Ruang Bayi", padahal master memiliki unit "NICU" | Data master kamar NICU | `DARI-CAPTURE` (`nwb-45-nicu-penempatan`) |

Kode bed umum di ringkasan (`BD-RSMMC-…`) sudah dicatat `ISS-EPS-040-15` dan tidak diulang di sini.

**Rekomendasi.** Kirim batas tanggal (hari ini sebagai maksimum, tanggal kasus dikurangi satu hari sebagai minimum — keputusan K-03 ISSUE-040), rapikan teks dan format waktu, buka denah pada lantai ruang bayi untuk mode neonatal, dan putuskan nilai `PatientLabelHospitalCode` serta unit layanan kamar NICU (K-04).

---

### ISS-EPS-041-09 — Admisi Draft ganda dan format waktu peringatan

| | |
| --- | --- |
| **No. laporan** | PL-01 |
| **Jenis** | `DESIGN_CHANGE` (bila pemilik memutuskan memblokir; ditangani ISSUE-EPS-022), `BUG` (format waktu peringatan) |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-EPS-041-14`; keputusan admisi ganda tetap pada `FIX-EPS-022-01` |

**Apa yang terjadi.** Tn. B (pseudonim, 68 th) sudah punya admisi Draft dengan bed dipesan. Saat dicari lagi, layar menampilkan lencana "Ada Admisi Disiapkan" tetapi petugas tetap bisa membuat admisi kedua. Teks peringatan dari server berbunyi "…sejak 26 October 2026 03:44", padahal admisi dibuat pukul 10.44 WIB.

**Kenapa terjadi.**

- Perilaku "peringatan, bukan penolakan" disengaja: `05-skema-tampilan.md:1858-1862` (Amandemen PLAN-REPAIR-EPS-010, `RWI-DEC-297`): "Lanjutkan Admisi {nomor}" (admisi lain masih Draft — peringatan, Lanjut tetap dapat dipakai). Celah ini sudah dianalisis di `ISS-EPS-022-01` dan `FIX-EPS-022-01` berstatus ⛔ TERBLOKIR menunggu K-01/K-02 ISSUE-022.
- Format waktu: `InpEpisodeService.AdmissionEligibility.cs:136-139`

```csharp
private static string BuildOtherDraftWarning(string episodeNumber, DateTime createdAt) =>
    "Pasien ini punya admisi lain yang sedang disiapkan, yaitu " +
    $"{episodeNumber} sejak {createdAt:dd MMMM yyyy HH:mm}. " +
    "Lanjutkan bila memang admisi terpisah, atau batalkan yang lama lebih dulu.";
```

`createdAt` adalah waktu UTC dan diformat dengan budaya server. Helper yang benar sudah tersedia: `InpAdmissionText.FormatDateTime(utc, timeZone)` (`Helpers/InpAdmissionText.cs:72`) memakai zona RS dan budaya `id-ID`.

**Apakah ini menyimpang dari desain?** Perilaku peringatan sesuai desain. Format waktunya `BUG`.

**Dampak nyata.** Petugas membaca "03:44" dan mengira admisi lama dibuat dini hari oleh shift malam, lalu menganggapnya sisa yang aman diabaikan.

**Rekomendasi.** Perbaiki format waktu peringatan dengan helper yang ada. Untuk perilakunya, percepat keputusan K-01/K-02 ISSUE-022; bukti runtime dari uji ini ditambahkan ke sana (bagian 8).

---

### ISS-EPS-041-10 — Nominal deposit tanpa batas atas wajar

| | |
| --- | --- |
| **No. laporan** | PL-02 |
| **Jenis** | `BUG` |
| **Area** | Frontend dan Backend |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-EPS-041-15` |

**Apa yang terjadi.** Di langkah Deposit jalur Pasien Lama, mengetik dua puluh angka 9 menghasilkan tampilan "Rp 1.000.000.000.000.000" dan petugas tetap dapat lanjut. Ketikan "12,5" (maksud petugas: 12,5 juta) menjadi Rp 125 tanpa peringatan.

**Kenapa terjadi.**

- `use-inpatient-admission-deposit.jsx:96-110` membatasi 16 digit lalu memprosesnya dengan `Number()`; di atas Rp 9.007.199.254.740.991 presisi hilang.
- Jalur biasa tidak punya penjaga: `InpatientEpisodeDtos.cs:85` (`RequestedDepositAmount`) tanpa `[Range]`, dan `InpEpisodeService.cs:545` hanya memeriksa `> 0`.
- Jalur transfer **sudah** punya penjaga di kedua sisi: `inpatient-admission-transfer-utils.js:33` (`Number.isSafeInteger`) dan `InpAdmissionTransferService.cs:85-88` (maksimum 9.999.999.999.999.999,99). Logika yang sama belum diterapkan ke jalur biasa.

**Apakah ini menyimpang dari desain?** Ya — dua jalur dengan langkah Deposit yang sama berperilaku berbeda.

**Dampak nyata.** Petugas tidak sengaja menekan angka nol berulang; keluarga pasien menerima order deposit bernilai triliunan rupiah di Kasir dan harus menunggu pembatalan order.

**Rekomendasi.** Terapkan penjaga yang sama ke jalur biasa (FE dan BE), ditambah batas atas wajar bila pemilik menetapkannya (K-05).

---

### ISS-EPS-041-11 — Dua kamar berlabel "ODC 321"

| | |
| --- | --- |
| **No. laporan** | PL-03 |
| **Jenis** | `DATA` |
| **Area** | Data master kamar / pemetaan denah |
| **Keparahan** | Low |
| **Status bukti** | `DARI-CAPTURE` (`ext-27b-pilih-bed-tunggu`; daftar `aria-label` denah memuat dua "ODC 321") |
| **Perbaikan** | `FIX-EPS-041-16` |

**Apa yang terjadi.** Denah Lantai 3 menampilkan kamar ODC berurutan 322, 321, 321, 320.

**Kenapa terjadi.** `DUGAAN`: dua baris `MstRoom` bernama sama atau satu kamar dipetakan dua kali di denah. Cara membuktikan: jalankan kueri P-01 (bagian 7).

**Rekomendasi.** Perbaiki nama kamar atau pemetaan denah lewat proses master data yang berwenang.

---

### ISS-EPS-041-12 — Catatan istilah dan umpan balik jalur Pasien Lama

| | |
| --- | --- |
| **No. laporan** | PL-05 |
| **Jenis** | `BUG` (butir 1–5), `DESIGN_CHANGE` (butir 6); butir 7–8 sesuai desain |
| **Area** | Frontend dan Backend |
| **Keparahan** | Low |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-EPS-041-17`, `FIX-EPS-041-18` |

| No | Gejala | Sebab | Jenis | Bukti |
| ---: | --- | --- | --- | --- |
| 1 | Tombol "Selesai" di langkah terakhir memunculkan "Tinggalkan Admisi?" | `onExit` langkah cetak memanggil penjaga keluar yang selalu bertanya bila episode sudah ada; komentar skema 3.10 menyatakan tombol ini "menutup alur" | `BUG` | `inpatient-admission-print-steps.jsx:230-236`; `inpatient-admission-view.jsx:532`; `use-inpatient-admission-exit-guard.jsx:82-90` |
| 2 | Saat denah memuat tertulis "Belum ada kamar pada lantai ini." | Ringkasan denah tidak membedakan "sedang memuat" dari "kosong" | `BUG` | `floor-plan-summary.jsx:14-19` |
| 3 | Setelah F5, "Dipesan Pada" menjadi "-" | Pemesanan dipulihkan dari papan tempat tidur yang hanya membawa waktu kedaluwarsa, tanpa waktu pesan | `BUG` | `use-inpatient-admission-bed.jsx:96-99`, `:477-478`; `InpatientBedOccupancyDtos.cs:276` (papan) vs `:318` (`ReservedAt` hanya di respons reservasi) |
| 4 | `PlannedDepositAmount` episode kosong di jalur biasa, terisi di jalur transfer | Jalur biasa hanya membuat order deposit | `BUG` (konsistensi data) | `InpEpisodeService.cs:545-570` vs `InpAdmissionTransferService.cs:156-163` |
| 5 | Lembar cetak menyatakan "Admisi sudah dikunci pada langkah Konfirmasi", padahal "Kunci Admisi" hanya menulis bila ada perubahan | Istilah "kunci" tidak berarti penguncian server | `BUG` (teks) | `use-inpatient-admission-confirmation.jsx:173-204` |
| 6 | Konfirmasi tidak menampilkan order deposit dan tingkat ketergantungan | Tidak termasuk isi Konfirmasi pada skema 3.11 | `DESIGN_CHANGE` (usulan) | `05-skema-tampilan.md` 3.11 |
| 7 | Lencana "Sedang Dirawat" tetapi tombol Lanjut tetap aktif | **Sesuai desain**: "Tombolnya tidak dimatikan karena hasil pemeriksaan" | — (tidak diperbaiki) | `05-skema-tampilan.md:1854-1856` |
| 8 | Pilihan bawaan di langkah 2 hanya ditandai warna ikon | Tampilan kartu pilihan; tidak ada aturan yang dilanggar | — (catatan visual saja) | `ext-08-langkah2-jenis-kunjungan` |

**Rekomendasi.** Perbaiki butir 1–5; butir 6 menunggu keputusan K-06. Butir 7 dan 8 tidak diperbaiki.

---

## 5. Tanya-jawab pelapor

> **T:** Apakah tombol Lanjut yang tetap aktif untuk pasien "Sedang Dirawat" adalah kesalahan?
>
> **J:** Bukan. Desain yang disetujui (Amandemen PLAN-REPAIR-EPS-010) sengaja membiarkan tombol aktif; menekannya menjalankan pemeriksaan dan menampilkan alasan beserta tindakan "Pindah Tempat Tidur". Hasil uji PL-TC-05 sudah menunjukkan perilaku itu.

> **T:** Kenapa permintaan IGD tidak kembali ke antrean setelah admisinya dibatalkan?
>
> **J:** Karena kontrak transfer EPS-006/K-02 menetapkan satu permintaan hanya boleh menghasilkan satu kunjungan, termasuk bila kunjungan itu kemudian dibatalkan. Yang salah adalah teks layar yang menyarankan "dibuka ulang"; itu yang diperbaiki.

> **T:** Apakah kode "SITE-MMC-001" di label bayi adalah bug?
>
> **J:** Bukan. Label sengaja memuat kode rumah sakit (G-36). Karena pengaturan `PatientLabelHospitalCode` kosong, sistem memakai kode situs dari master. Cukup isi pengaturan itu dengan kode yang diinginkan.

---

## 6. Temuan tambahan

### ISS-EPS-041-T1 — Footer melayang menutupi isian saat digulir

| | |
| --- | --- |
| **No. laporan** | TR-07 / PL-04 / BY-03 |
| **Kenapa dicantumkan** | Dilaporkan di ketiga laporan uji sebagai P4 Sedang, tetapi sumbernya komponen layout global, bukan modul Rawat Inap |
| **Jenis** | Catatan untuk pemilik layout global |
| **Area** | Frontend — `components/features/footers/footer.jsx` |
| **Keparahan** | Low (dikoreksi dari Sedang) |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | Tidak ada pada rencana ini |

**Apa yang terjadi.** Bilah "Privacy Policy · Terms of Use · Quilvian System" selalu menempel di bawah layar dan menutupi isian yang sedang berada di bagian bawah layar ketika halaman digulir.

**Kenapa terjadi.** Footer memang dirancang tetap (*fixed*), dan layout sudah memberi ruang aman di bawah konten supaya bagian paling bawah tetap dapat digulir ke atas footer: `footer.jsx:14-41` (`applyFooterSafeSpace`, variabel `--app-footer-safe-space`). Jadi tidak ada isian yang mustahil dijangkau; isian hanya tertutup sementara sampai digulir. Masalah yang dialami penguji (goresan tanda tangan tidak terbaca) terjadi karena skrip menggambar di area yang tertutup footer, bukan karena kanvas tidak dapat dijangkau.

**Rekomendasi.** Diteruskan ke pemilik layout global sebagai masukan kenyamanan. Tidak dimasukkan ke rencana modul ini.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor (atau pemilik DB pengembang):**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Mohon jalankan kueri baca-saja ini dan lampirkan hasilnya: `SELECT r."Id", r."RoomName", r."ServiceUnitId", u."ServiceUnitName", r."IsForNewborn", r."IsIntensiveCare" FROM "MstRoom" r LEFT JOIN "MstServiceUnit" u ON u."Id" = r."ServiceUnitId" WHERE NOT r."IsDelete" AND (r."RoomName" ILIKE '%321%' OR r."RoomName" ILIKE '%NICU%');` | Memastikan apakah "ODC 321" ganda berasal dari data kamar, serta unit layanan kamar NICU | `FIX-EPS-041-16` dan butir 6 `FIX-EPS-041-13` belum bisa ditetapkan sasarannya |
| P-02 | Mohon jalankan kueri baca-saja ini untuk daftar bayi tanpa episode: `SELECT p."Id", p."FullName", p."CreateDateTime", p."MotherPatientId" FROM "MstPatient" p WHERE p."IsNewborn" AND NOT p."IsDelete" AND NOT EXISTS (SELECT 1 FROM "RegPatientEncounter" e WHERE e."PatientId" = p."Id") ORDER BY p."CreateDateTime";` | Daftar resmi untuk keputusan K-03 bersama Rekam Medis | `FIX-EPS-041-11` belum dapat dikerjakan |

**Untuk pemilik (modul rawat-inap, sub-modul episode-rawat-inap):**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bed isolasi di master data | (a) Tetapkan kamar/bed tertentu sebagai bed isolasi / (b) Belum ada ruang isolasi: tetapkan kebijakan sementara (misalnya kamar VIP tunggal ditandai isolasi) / (c) Biarkan, pasien isolasi dirujuk keluar | (a) atau (b), karena aturan Aturan 7 sudah berlaku dan saat ini tidak ada satu bed pun yang memenuhinya | `FIX-EPS-041-05` |
| K-02 | Admisi transfer yang dibatalkan | (a) Pertahankan kontrak EPS-006/K-02: permintaan ditutup permanen, IGD/Poli menerbitkan permintaan baru / (b) Ubah kontrak: pembatalan mengembalikan permintaan ke antrean | (a) — kontrak sudah disahkan dan mencegah pemakaian ulang surat yang ambigu; cukup perbaiki teks layar | `FIX-EPS-041-07` (teks dikerjakan untuk pilihan a; pilihan b butuh revisi kontrak) |
| K-03 | Bayi terdaftar tanpa episode | (a) Lanjutkan admisi memakai pasien bayi yang sudah ada; RM yatim lama dinonaktifkan/digabung oleh Rekam Medis / (b) Selalu buat pasien baru; RM lama dinonaktifkan | (a) — mencegah RM ganda dan urutan kelahiran yang salah | `FIX-EPS-041-10`, `FIX-EPS-041-11` |
| K-04 | Pengaturan label dan unit NICU | Nilai `PatientLabelHospitalCode` (misalnya "RSMMC" atau dikosongkan); apakah kamar NICU dipindah ke unit layanan "NICU" | Isi kode singkat RS; pindahkan NICU ke unit "NICU" bila tarif/laporan memerlukannya — konfirmasi dengan Billing | `FIX-EPS-041-13` |
| K-05 | Batas atas nominal deposit yang diminta | Ikuti batas teknis (≈ Rp 9 kuadriliun) / batas kebijakan (misalnya Rp 1 miliar) | Batas kebijakan dari Billing; minimal batas teknis yang sama dengan jalur transfer | `FIX-EPS-041-15` (bagian batas kebijakan saja) |
| K-06 | Konfirmasi menampilkan order deposit dan tingkat ketergantungan | Tampilkan / tidak | Tampilkan, baca-saja | `FIX-EPS-041-18` |

Keputusan admisi Draft ganda (K-01/K-02 ISSUE-EPS-022) tidak ditanyakan ulang di sini; bukti runtime dari uji ini ditambahkan sebagai pertimbangan.

---

## 8. Catatan pola

1. **Jalur transfer berjalan di jalur tulisnya sendiri.** Tiga temuan (`041-01`, `041-02`, `041-04`) muncul karena jalur transfer memakai satu transaksi `admit` dan melewati titik-titik jalur biasa: pemeriksaan kelayakan, konteks kelayakan bed, dan penyimpanan lanjutan sesudah episode terbentuk. Pencegahan: setiap perubahan pada langkah bersama (Dokter, Pilih Bed, Konfirmasi) wajib dicek pada keempat jalur — Pasien Baru, Pasien Lama, Bayi, Transfer — dengan daftar periksa "apakah isian ini tersimpan di jalur transfer?".
2. **Asumsi "server sudah meloloskan".** ISSUE-EPS-024 benar memakai kelayakan dari server, tetapi tidak memastikan server menerima konteks pasien. Pencegahan: setiap pemanggilan `available-beds` tanpa `episodeId` wajib mengirim `patientId` — layak diusulkan sebagai aturan di `rules/frontend/` untuk modul ini.
3. **Format waktu UTC di teks server.** Setelah `FIX-EPS-040-05` dan `FIX-EPS-040-22`, masih ada teks yang memformat `DateTime` UTC secara langsung (`041-09`). Pencegahan: semua teks pengguna di modul ini memakai `InpAdmissionText.Format*`.
4. **Bukti runtime untuk ISSUE-EPS-040.** Uji 26-10-2026 membuktikan di layar perbaikan `FIX-EPS-040-01` s.d. `-04`, `-06`, `-07`, `-09` s.d. `-16`, `-19` s.d. `-22`, dan sebagian `-08` (validasi NIK/HP penanda tangan, diuji pada form persetujuan yang sama di jalur Pasien Lama). Yang **belum** terbukti di layar: `-05` (uji berlangsung siang hari; hanya terbukti di kode), `-17` (keterangan "Menyiapkan admisi bayi…" tidak diamati), `-18` (ringkasan masih memakai kode bed umum), dan `-23` (tag Swagger, hanya terbukti di kode). Register `plan-repair-040` masih mencatat "0/n terbukti"; builder dapat memperbarui kolom bukti dengan merujuk laporan `testing/live-testing/laporan-live-testing-admisi-bayi-baru-lahir-20261026.md`.
5. **Format nomor kunjungan.** Kunjungan rawat inap yang dibuat jalur admisi memakai pola waktu-acak, sedangkan registrasi biasa memakai nomor berurut. Belum menjadi temuan karena tidak ada aturan penomoran tunggal untuk kunjungan rawat inap; layak ditanyakan pada pemutakhiran kontrak berikutnya.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-26 | Issue dibuat dari tiga laporan live browser testing (15 temuan); dua penilaian laporan uji dikoreksi (TR-03 sesuai kontrak, footer milik layout global) | `diagnose-module-issue` |
