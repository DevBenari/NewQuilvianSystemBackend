# ISSUE-015 — Verifikasi ulang: skor EWS dan diet bangsal sudah ada; yang kurang adalah peringatan kritis yang terlihat, skor anak/obstetri, tampilan EWS yang konsisten, dan pintu retur obat dari e-MAR

```yaml
issue_id: ISSUE-KEP-015
module_id: rawat-inap
submodule: keperawatan
layar: "Ruang Kerja Keperawatan V2 — Vital Sign (nursing-vital-sign-tab), Pengawasan Harian Pasien (daily-monitoring-section), Diet Medis dan Konsultasi Gizi, Obat & Alkes / Daftar Pemberian Obat (e-MAR)"
sumber_laporan: "Dokumen audit klinis keperawatan 02-11-2026: 3 butir, tanpa screenshot dan tanpa capture database"
tanggal_issue: "2026-11-02"
status: TERBUKA
keparahan_tertinggi: High
source_sha_backend: "396c8363 (MHamzah)"
source_sha_frontend: "9c4000724 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-015-kalkulasi-ews-pemesanan-diet-gizi-dan-retur-obat.md
```

> **Catatan revisi 2 (2026-11-02).** Versi 1 menyatakan sistem tidak menghitung skor EWS dan tab gizi
> bangsal belum terhubung. Keduanya tidak sesuai source. Skor EWS sudah dihitung server dan disimpan,
> dan diet medis serta konsultasi gizi sudah dapat dipesan dari bangsal. Versi 1 juga mengutip berkas
> yang tidak ada: `NursingVitalSignController.cs`, `NursingVitalSignService.cs`,
> `nursing-vital-sign-view.jsx`, `nursing-nutrition-view.jsx`, `nursing-mar-view.jsx`, dan
> `InpatientMedicationService.cs`. Saat verifikasi ulang ditemukan kekurangan yang nyata dan lebih
> spesifik. Kekurangan itu dicatat di bawah, termasuk tiga temuan tambahan baru.

---

## 1. Ringkasan

Dokumen ini memeriksa tiga butir pada ruang kerja keperawatan. Butir pertama, skor EWS (*Early Warning
Score*, skor peringatan dini perburukan pasien), **sudah dihitung otomatis** setiap kali tanda vital
disimpan. Server juga sudah menandai pasien yang perlu dilaporkan ke dokter. Masalah sebenarnya: **tidak
ada layar yang menampilkan daftar penanda itu**, skor untuk anak dan ibu hamil belum ada, dan dua layar
menampilkan EWS tidak konsisten dengan server. Inilah butir paling berbahaya (`High`).

Butir kedua, pemesanan diet dari bangsal, **tidak terbukti**. Diet Medis dan Konsultasi Gizi sudah
berjalan. Label "Integrasi belum tersedia" ada pada tab **Rehab Medik**, bukan tab gizi. Butir ketiga,
retur obat, **terbukti sebagian**. Farmasi sudah punya alur retur yang mengembalikan stok dan
mengurangi tagihan, termasuk untuk sisa obat bangsal. Yang belum ada hanya pintu masuknya dari e-MAR.

---

## 2. Laporan asli

| No. laporan | Keluhan (kata-kata pelapor) | Lampiran yang merujuk | Hasil pemeriksaan lampiran |
| ---: | --- | --- | --- |
| 1 | "Data TTV yang diinput perawat setiap shift tidak menghasilkan skor EWS/MEWS otomatis. Sistem tidak memberi peringatan saat kondisi pasien memburuk ke arah kritis." | `TrxPatientVitalSign.cs`, `NursingVitalSignController.cs`, `nursing-vital-sign-view.jsx` | `TrxPatientVitalSign.cs` ada dan **sudah memuat** kolom EWS. Dua berkas lain **tidak ada**; layar yang sebenarnya `nursing-vital-sign-tab.jsx` |
| 2 | "Tab Gizi di ruang kerja keperawatan bertuliskan integrasi belum tersedia. Perawat tidak bisa memilih jenis diet dan memesan menu makanan pasien ke dapur gizi." | `02-module-map.md:438` (`CAP-015-GIZ`), `GziPatientDiet.cs`, `nursing-nutrition-view.jsx` | Baris 438 ada tetapi **usang**: ditulis sebelum `BE-RWI-164`, `BE-RWI-166`, `FE-RWI-174`, dan `FE-RWI-184` selesai. `nursing-nutrition-view.jsx` **tidak ada** |
| 3 | "Jika dokter menyetop pemberian obat injeksi yang sudah diantar farmasi, tidak ada tombol retur obat di layar MAR untuk mengembalikan stok dan membatalkan tagihannya di kasir." | `PhmMedicationAdministration.cs`, `nursing-mar-view.jsx` | `PhmMedicationAdministration.cs` ada. `nursing-mar-view.jsx` **tidak ada**; layar yang sebenarnya `medication-administration-chart-panel.jsx` |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Hasil verifikasi | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | :---: | --- | --- | --- | --- | :---: | --- | --- |
| `ISS-KEP-015-01` | 1 | Skor EWS dikira tidak dihitung; penanda lapor dokter tidak terlihat dan skor anak/obstetri belum ada | **TERBUKTI SEBAGIAN** | `DESIGN_CHANGE` | Frontend, backend, keputusan klinis | High | `SUDAH-VERIFIKASI` | `FIX-KEP-015-04`, `FIX-KEP-015-05` (`FIX-KEP-015-01` dibatalkan) |
| `ISS-KEP-015-02` | 2 | Pemesanan diet bangsal dikira belum terhubung | **TIDAK TERBUKTI** | Tidak ada — fitur sudah berjalan | — | — | `SUDAH-VERIFIKASI` | `FIX-KEP-015-02` (dibatalkan) |
| `ISS-KEP-015-03` | 3 | Tidak ada pintu retur obat dari e-MAR | **TERBUKTI SEBAGIAN** | `DESIGN_CHANGE` | Frontend keperawatan, lintas modul Farmasi | Medium | `SUDAH-VERIFIKASI`; kecocokan dengan obat UDD `DUGAAN` | `FIX-KEP-015-03` |
| `ISS-KEP-015-T1` | — | Belum ada lembar serah terima shift perawat (SBAR) | **TERBUKTI** | `DESIGN_CHANGE` | Backend dan frontend | Medium | `SUDAH-VERIFIKASI` | `FIX-KEP-015-06` |
| `ISS-KEP-015-T2` | — | Kolom EWS pada Pengawasan Harian selalu tampil "-" | **TEMUAN BARU** | `CONTRACT_MISMATCH` | Frontend | High | `SUDAH-VERIFIKASI` | `FIX-KEP-015-07` |
| `ISS-KEP-015-T3` | — | Tab Vital Sign menafsirkan skor EWS dengan ambang yang berbeda dari server | **TEMUAN BARU** | `BUG` | Frontend | High | `SUDAH-VERIFIKASI` | `FIX-KEP-015-07` |
| `ISS-KEP-015-T4` | — | Acuan skor EWS belum ditetapkan; dua aturan yang lazim pada tabel acuannya tidak diterapkan | **TEMUAN BARU** | `PERLU_KONFIRMASI` | Keputusan klinis | Medium | `SUDAH-VERIFIKASI` di source; acuan klinisnya `DUGAAN` | Ditahan K-02 |

---

## 4. Rincian per temuan

### ISS-KEP-015-01 — Skor EWS sudah dihitung, tetapi peringatannya tidak sampai ke siapa pun

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Hasil verifikasi** | TERBUKTI SEBAGIAN |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend keperawatan dan dokter, backend skor, keputusan klinis |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-KEP-015-04`, `FIX-KEP-015-05`; `FIX-KEP-015-01` dibatalkan |

**Apa yang terjadi.** Perawat menyimpan tanda vital Tn. J (pseudonim, 58 tahun): tekanan darah sistolik
85, nadi 128, napas 30, saturasi 91%, suhu 38,9 °C, kesadaran menurun. Server menghitung skor EWS
per parameter dengan urutan yang sama, 3+2+3+3+1+3 = **15**, menggolongkannya **Kritis**, dan menyalakan
penanda "perlu lapor dokter". Namun
tidak ada layar yang mengumpulkan penanda itu menjadi daftar peringatan. Tidak ada pula pemberitahuan
yang dikirim ke dokter jaga. Perawat baru tahu bila ia sendiri membuka tab Vital Sign pasien tersebut.

**Kenapa klaim "tidak ada skor" versi 1 keliru.**

1. Skor dihitung server setiap kali tanda vital disimpan —
   `Areas/HealthServices/ClinicalManagement/Services/PatientVitalSignCalculation.cs:36-38`, rumus pada
   baris 102-175, dan penggolongan risiko pada baris 177-192:

   ```csharp
   if (ewsScore.Value >= 7) return EwsRiskLevel.Critical;
   if (ewsScore.Value >= 5) return EwsRiskLevel.High;
   if (ewsScore.Value >= 3) return EwsRiskLevel.Medium;
   return EwsRiskLevel.Low;
   ```

2. Hasilnya disimpan pada `TrxPatientVitalSign` (`TrxPatientVitalSign.cs:173-178`: `EarlyWarningScore`,
   `EwsRiskLevel`, `EwsMonitoringRecommendation`). Kolom `EwsScore` dan `EwsRiskCategory` yang diusulkan
   versi 1 akan menjadi kolom ganda.
3. Skor tampil sebagai badge di tab Vital Sign keperawatan (`nursing-vital-sign-tab.jsx:325-327`) dan di
   kartu tanda vital ruang kerja dokter (`physician-workspace/tabs/progress-note/vital-signs-card.jsx:204-210`).

**Kenapa peringatannya tidak sampai (bagian yang benar).**

1. Server menyalakan `NeedDoctorNotification` secara otomatis bila tanda vital kritis atau EWS berisiko
   `High`/`Critical` — `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs:564`
   (simpan baru) dan `:713` (ubah):

   ```csharp
   NeedDoctorNotification = request.NeedDoctorNotification || calculated.IsCritical
       || calculated.EwsRiskLevel == EwsRiskLevel.High || calculated.EwsRiskLevel == EwsRiskLevel.Critical,
   ```

2. Server juga sudah menyediakan daftar peringatan: `GET .../patient-vital-signs/critical-alerts`
   (`PatientVitalSignController.cs:128`, hak akses `PatientVitalSign : Read`), yang menyaring tanda vital
   kritis, abnormal, atau perlu lapor dokter (baris 143 dan 151-152).
3. **Tidak ada satu pun layar frontend yang memanggil `critical-alerts`.** Penelusuran `critical-alerts`
   pada `QuilvianSystemFrontendDev/src` tidak menemukan hasil apa pun. Dokumen desain keperawatan juga
   belum merancang tampilan peringatan ini.
4. Tidak ada pemberitahuan aktif (*push notification*, SMS, atau panggilan Tim Reaksi Cepat). Penelusuran
   `CodeBlue`, `RapidResponse`, dan `EwsAlert` di backend tidak menemukan hasil apa pun.
5. Rumus tidak menerima usia, kehamilan, atau jenis pasien (`PatientVitalSignCalculation.cs:18-30`).
   Akibatnya pasien anak dan ibu hamil dinilai dengan ambang dewasa.

**Apakah ini menyimpang dari desain?** Tidak. Tampilan peringatan, pemberitahuan aktif, dan skor anak
atau obstetri memang belum pernah dirancang, sehingga butir ini `DESIGN_CHANGE`.

**Dampak nyata.** Perburukan Tn. J tercatat dengan benar, tetapi hanya terlihat oleh orang yang membuka
tab Vital Sign miliknya. Perawat kepala dan dokter jaga tidak punya satu daftar untuk memantau semua
pasien berisiko di bangsal. Pada anak 2 tahun dengan nadi normal 130 kali per menit, skornya naik 2
hanya karena ambang dewasa, sehingga peringatan palsu dapat membuat petugas mengabaikan peringatan
yang sungguhan.

**Rekomendasi.** Tampilkan daftar "Perlu Lapor Dokter" memakai endpoint `critical-alerts` yang sudah ada
(`FIX-KEP-015-04`). Tetapkan acuan skor anak dan obstetri serta protokol eskalasi bersama pemilik klinis
sebelum membangun apa pun (`FIX-KEP-015-05`, K-01, K-02). Algoritma MEWS yang ditulis versi 1 dibatalkan,
karena akan menggantikan rumus yang sudah berjalan dengan ambang yang belum disahkan siapa pun.

---

### ISS-KEP-015-02 — Pemesanan diet bangsal dikira belum terhubung

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Hasil verifikasi** | TIDAK TERBUKTI |
| **Jenis** | Tidak ada. Fitur sudah berjalan |
| **Area** | — |
| **Keparahan** | — |
| **Status bukti** | `SUDAH-VERIFIKASI` di source backend, frontend, dan roadmap |
| **Perbaikan** | `FIX-KEP-015-02` — dibatalkan |

**Apa yang sebenarnya terjadi di sistem.** Perawat membuka **Catatan Keperawatan → Diet Medis** untuk
menetapkan diet atas instruksi dokter. Setelah dokter memverifikasi, diet itu tercatat di modul Gizi.
Perawat juga dapat memesan **Penunjang Medis → Konsultasi Gizi**.

**Kenapa klaim versi 1 keliru.**

1. Endpoint diet rawat inap sudah ada:
   `POST api/v1/health-services/inpatient-management/episodes/{episodeId}/diets` dan `.../{dietId}/stop`
   (`InpatientDietController.cs:15`, `:20`, `:23`, hak akses `NutritionPatientDiet : Update`).
2. Adapternya menulis diet ke tabel Gizi `GziPatientDiet`. Status verifikasinya `Pending` bila perawat
   yang menginput atas instruksi dokter (`InpDietOrderAdapter.cs:29-35`), sesuai `RWI-DEC-178`.
3. Frontend memanggil endpoint itu (`src/lib/services/health-services/inpatient-management/inpatient-diet.service.js:21`, `:30`),
   lewat tab "Diet Medis" (`nursing-care/tabs/nursing-narrative-tab.jsx:48`).
4. Konsultasi gizi dikirim lewat `POST .../ancillary-orders/nutrition-consultations`
   (`InpatientNutritionOrderController.cs:22`; `inpatient-ancillary-order.service.js:15`).
5. Roadmap mencatat keempat task ini `✅`: `BE-RWI-164` (`dokter-rawat-inap/roadmap/backend-roadmap-finishing.md:127`),
   `BE-RWI-166` (`keperawatan/roadmap/backend-roadmap-finishing.md:100`), `FE-RWI-174` (Playwright 11/11
   PASS), dan `FE-RWI-184`.
6. Label "Integrasi belum tersedia" pada penunjang keperawatan hanya ada di tab **Rehab Medik**
   (`nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx:64`).
7. `02-module-map.md:438` dan catatan pemilik 15 September 2026 pada baris 456 ditulis sebelum task-task di
   atas selesai, sehingga sudah usang. Perbaikannya lewat jalur blueprint (plan repair bagian 7).

**Apakah ini menyimpang dari desain?** Tidak.

**Dampak nyata.** Tidak ada untuk diet medis. Satu hal yang memang belum ada: **pilihan menu per waktu
makan** oleh pasien (pagi, siang, malam) dan jatah makan penunggu. Keduanya bukan klaim butir ini dan
masih menunggu aturan instalasi gizi (`Daftar-Pekerjaan-Belum-Selesai.md` bagian 6 butir 4).

**Rekomendasi.** Tidak ada perubahan source. `FIX-KEP-015-02` dibatalkan. Endpoint `diet-orders` usulan
versi 1 tidak dibuat, karena akan menduplikasi `.../diets`.

---

### ISS-KEP-015-03 — Tidak ada pintu retur obat dari e-MAR

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Hasil verifikasi** | TERBUKTI SEBAGIAN |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend keperawatan; memakai modul Farmasi dan Billing yang sudah ada |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI`; kecocokan alur retur dengan obat UDD rawat inap `DUGAAN` |
| **Perbaikan** | `FIX-KEP-015-03` |

**Apa yang terjadi.** DPJP mengganti antibiotik pada hari kedua, dan masih ada 9 vial obat lama di kotak
obat pasien. Di layar e-MAR tidak ada tombol untuk mengembalikan 9 vial itu ke Farmasi. Perawat harus
membawanya ke Farmasi dan meminta petugas Farmasi membuat retur di menu Farmasi.

**Kenapa terjadi.**

1. **Alur retur sudah ada di Farmasi**, termasuk untuk sisa obat bangsal. Lihat
   `Areas/HealthServices/PharmacyManagement/Models/PhmDrugReturn.cs:45-53`:

   ```csharp
   /// Boleh kosong: obat dapat dikembalikan tanpa dokumen pemakaian yang jelas, misalnya
   /// sisa dari bangsal. ...
   public Guid? SourceDrugUsageId { get; set; }
   ```

2. Alurnya: retur disusun sebagai draf, diajukan, lalu diperiksa petugas Farmasi. Stok baru bertambah
   setelah diperiksa (`DrugReturnController.cs:14-18`, `:67`, `:87`, `:104`). Retur terikat pada kunjungan
   pasien (`PhmDrugReturn.cs:35`, `EncounterId`).
3. **Tagihan ikut berkurang otomatis.** Billing membaca retur lewat
   `BillingClinicalChargeBridgeService.DrugReturn.cs:23` dan menerapkannya pada invoice melalui
   `BillingDrugReturnService` (baris 83).
4. **Yang tidak ada:** pintu masuk dari ruang kerja keperawatan. Folder
   `nursing-workspace/sections/medication/` tidak memuat aksi retur apa pun.
5. Status dosis e-MAR yang sebenarnya adalah `Due`, `Administered`, `Held`, dan `Refused`
   (`PharmacyManagement/Enums/MedicationDoseStatus.cs:9-21`). Status `Given`, `Missed`, dan `Discontinued`
   yang disebut versi 1 tidak ada.

**Apakah ini menyimpang dari desain?** Tidak. Pintu retur dari e-MAR belum pernah dirancang.

**Dampak nyata.** Sisa obat dapat tetap tertagih bila perawat lupa membawanya ke Farmasi. Pengembalian
juga bergantung pada kunjungan fisik ke Farmasi.

**Rekomendasi.** Tambahkan aksi **Ajukan Retur** pada e-MAR yang membuat **draf retur** lewat endpoint
Farmasi yang sudah ada. Pemeriksaan, pengembalian stok, dan pengurangan tagihan tetap dikerjakan Farmasi
dan Billing seperti sekarang. Slip retur, method `ProcessMedicationReturnAsync`, dan *credit note* baru
usulan versi 1 dibatalkan, karena ketiganya menduplikasi alur yang sudah ada. Satu hal perlu dibuktikan
lebih dulu: apakah retur Farmasi menemukan baris tagihan obat UDD rawat inap (`DUGAAN`). **Cara
membuktikan:** buat satu retur uji pada menu Retur Obat Farmasi untuk pasien rawat inap uji, lalu periksa
invoice pasien setelah retur diperiksa.

---

## 5. Tanya-jawab pelapor

> **T:** Apakah skor EWS bisa dihitung otomatis tanpa perawat mengklik tombol kalkulasi?
>
> **J:** Ya, dan itu sudah berjalan. Server menghitung skor setiap kali tanda vital disimpan, lalu
> menampilkannya pada tab Vital Sign.

> **T:** Bagaimana jika pasien anak-anak dirawat di bangsal? Apakah rumus EWS-nya sama dengan dewasa?
>
> **J:** Saat ini sama, dan itu kekurangan nyata. Rumus tidak menerima usia, sehingga anak dinilai dengan
> ambang dewasa. Acuan skor anak (PEWS) dan obstetri perlu ditetapkan pemilik klinis lebih dulu (K-01).

---

## 6. Temuan tambahan

### ISS-KEP-015-T1 — Belum ada lembar serah terima shift perawat (SBAR)

| | |
| --- | --- |
| **Hasil verifikasi** | TERBUKTI |
| **Jenis** | `DESIGN_CHANGE` |
| **Keparahan** | Medium |
| **Kenapa dicantumkan** | Dicantumkan versi 1; terbukti saat diperiksa ulang |
| **Status bukti** | `SUDAH-VERIFIKASI` |

`CliNursingShift` hanya master jam dinas, dengan kolom kode, nama, jam mulai, dan jam selesai
(`Areas/HealthServices/ClinicalManagement/Models/CliNursingShift.cs:13-31`). Tidak ada tabel atau layar
serah terima pasien antar-shift. Kemampuan ini juga tercatat `Missing` pada audit 11 September 2026
(`Sisa-Pekerjaan-Rawat-Inap.md` bagian 7). Karena itu ia perlu melewati `requirement-completeness-gate`
sebelum menjadi task, sesuai catatan dokumen itu.

### ISS-KEP-015-T2 — Kolom EWS pada Pengawasan Harian selalu tampil "-"

| | |
| --- | --- |
| **Hasil verifikasi** | TEMUAN BARU |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Keparahan** | High — skor keselamatan tampil kosong tanpa disadari |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri tampilan EWS untuk butir 1 |
| **Status bukti** | `SUDAH-VERIFIKASI` |

Layar membaca nama field yang tidak dikirim server —
`src/components/view/health-services/inpatient-management/nursing-workspace/sections/daily-monitoring/daily-monitoring-section.jsx:294`:

```jsx
<span className={styles.metricEws}>EWS: {v.ewsScore !== null && v.ewsScore !== undefined ? v.ewsScore : "-"}</span>
```

Data tanda vital Pengawasan Harian bertipe `PatientVitalSignSeriesItem`
(`DailyMonitoringDtos.cs:434`), dan skornya bernama `EarlyWarningScore` (`PatientVitalSignDtos.cs:32`),
sehingga sampai di frontend sebagai `earlyWarningScore`. Nama `ewsScore` tidak dipakai di tempat lain mana
pun di frontend. **Contoh:** skor Tn. J 15 (Kritis), tetapi tabel Pengawasan Harian menampilkan "EWS: -".

### ISS-KEP-015-T3 — Tab Vital Sign menafsirkan skor EWS dengan ambang yang berbeda dari server

| | |
| --- | --- |
| **Hasil verifikasi** | TEMUAN BARU |
| **Jenis** | `BUG` |
| **Keparahan** | High — golongan risiko di layar lebih ringan daripada keputusan server |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri tampilan EWS untuk butir 1 |
| **Status bukti** | `SUDAH-VERIFIKASI` |

Layar bertuliskan "EWS (Server)" (`nursing-vital-sign-tab.jsx:325`) dan "dihitung murni oleh server"
(baris 205). Namun golongan dan warnanya dihitung ulang di frontend dengan ambangnya sendiri
(`getEwsInterpretation`, baris 65-105, dan warna tabel baris 526-536):

| Skor | Golongan server (`PatientVitalSignCalculation.cs:177-192`) | Tampilan tab Vital Sign |
| :---: | --- | --- |
| 0 | Low | Risiko Rendah |
| 1-2 | Low | **Risiko Rendah-Sedang (kuning)** |
| 3-4 | Medium | Risiko Rendah-Sedang (kuning) |
| 5-6 | **High** — server menyalakan "perlu lapor dokter" | **Risiko Sedang** |
| ≥ 7 | Critical | Risiko Tinggi / Kritis |

**Contoh:** pasien dengan skor 6 ditandai server sebagai `High` dan perlu dilaporkan ke dokter, tetapi
perawat membaca "Skor 6 — Risiko Sedang". Kartu dokter tidak bermasalah, karena ia memakai `ewsRiskLevel`
dari server (`vital-signs-card.jsx:207-210`). Teks anjuran tindakan di layar keperawatan, misalnya
*"Aktivasi Tim Emergensi Medis (Code Blue)"*, juga ditulis di frontend tanpa protokol yang disahkan.
Server sendiri sudah mengirim `EwsMonitoringRecommendation`.

### ISS-KEP-015-T4 — Acuan skor EWS belum ditetapkan

| | |
| --- | --- |
| **Hasil verifikasi** | TEMUAN BARU |
| **Jenis** | `PERLU_KONFIRMASI` |
| **Keparahan** | Medium |
| **Kenapa dicantumkan** | Ditemukan saat membaca rumus untuk butir 1 |
| **Status bukti** | Isi rumus `SUDAH-VERIFIKASI`; kemiripan dengan NEWS2 `DUGAAN` |

Tabel nilai pada `PatientVitalSignCalculation.cs:121-172` (napas, saturasi, suhu, sistolik, nadi,
kesadaran) sama dengan tabel NEWS2 (*National Early Warning Score 2*, skor peringatan dini yang banyak
dipakai rumah sakit). Namun dua aturan yang lazim menyertai tabel itu tidak diterapkan:

1. **Oksigen tambahan menambah skor.** Data `IsUsingOxygen` sudah dicatat
   (`PatientVitalSignDtos.cs:26`), tetapi tidak masuk perhitungan. Pemakaiannya hanya untuk merapikan
   isian (`PatientVitalSignCalculation.cs:272`).
2. **Satu parameter bernilai 3 memicu tinjauan segera**, walaupun total skornya rendah.

Dokumen desain keperawatan tidak menyebut acuan EWS mana yang dipakai rumah sakit. Ini bukan bug sampai
acuannya ditetapkan (K-02). **Cara membuktikan acuan:** pemilik klinis menyerahkan SOP EWS rumah sakit.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Pada butir 2, di menu mana tepatnya label "Integrasi belum tersedia" terlihat? | Source hanya memasang label itu pada tab Rehab Medik | `ISS-KEP-015-02` tetap ditutup sebagai tidak terbukti |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Acuan skor EWS anak (PEWS) dan obstetri, cara sistem mengenali jenis pasien, dan ambangnya | Ditetapkan pemilik klinis keperawatan berdasarkan SOP rumah sakit | Tetapkan lewat `grill-me` bersama pemilik klinis | `FIX-KEP-015-05` |
| K-02 | Acuan EWS dewasa yang resmi dipakai rumah sakit, termasuk aturan oksigen tambahan dan aturan satu parameter bernilai 3 | NEWS2 penuh / rumus sekarang / acuan lain | Konfirmasi dengan SOP rumah sakit | `FIX-KEP-015-05`; teks anjuran pada `FIX-KEP-015-07` |
| K-03 | Protokol eskalasi: siapa yang menerima peringatan dan lewat apa | (a) Daftar peringatan di layar saja; (b) ditambah pemberitahuan aktif ke dokter jaga | Mulai dari (a) memakai endpoint yang sudah ada; (b) setelah protokol dan kanalnya disahkan | `FIX-KEP-015-04` (bagian notifikasi aktif) |
| K-04 | Siapa yang boleh mengajukan retur obat dari bangsal (perawat, apoteker bangsal), dan apakah perannya diberi hak akses `DrugReturn : Create` (menyusun draf) dan `DrugReturn : Update` (mengajukan) | Perawat / apoteker bangsal | Perawat mengajukan draf, apoteker memeriksa | `FIX-KEP-015-03` |
| K-05 | Pengesahan pembatalan `FIX-KEP-015-01` dan `FIX-KEP-015-02` | Setuju / tolak | Setuju | Penutupan issue |

---

## 8. Catatan pola

Dua dari tiga butir versi 1 disalin dari dokumen analisis yang usang (`02-module-map.md:438` dan dokumen
analisis RS MMC), tanpa membuka source. Pada saat yang sama, penelusuran yang sebenarnya menemukan pola
lain yang lebih berbahaya: **frontend menafsirkan ulang data keselamatan pasien** (T3) atau **salah membaca
nama field** (T2), sehingga dua layar keperawatan menampilkan EWS berbeda dari server. Pencegahan:

1. golongan risiko klinis selalu diambil dari field server (`ewsRiskLevel`), bukan dihitung ulang di
   layar;
2. nama field dicocokkan secara harfiah dengan DTO backend;
3. `02-module-map.md` diperbarui setiap kali task penunjang selesai.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-02 | Versi 1: issue diterbitkan untuk kalkulasi EWS, diet gizi, dan retur obat | AI Agent (`diagnose-module-issue`) |
| 2026-11-02 | Revisi 2: verifikasi ulang terhadap source SHA yang sama. `ISS-KEP-015-01` dikoreksi: skor dan penanda lapor dokter sudah ada; kekurangannya tampilan peringatan, skor anak/obstetri, dan protokol eskalasi. `ISS-KEP-015-02` dinyatakan tidak terbukti. `ISS-KEP-015-03` dikoreksi: retur Farmasi dan pengurangan tagihan sudah ada; yang kurang pintu dari e-MAR. Temuan baru `T2` (field `ewsScore`), `T3` (ambang EWS di layar), dan `T4` (acuan skor) ditambahkan. Keparahan tertinggi turun dari Blocker ke High. P-01 dan K-01 s.d. K-05 ditambahkan | AI Agent (`diagnose-module-issue`) atas permintaan pemilik |
