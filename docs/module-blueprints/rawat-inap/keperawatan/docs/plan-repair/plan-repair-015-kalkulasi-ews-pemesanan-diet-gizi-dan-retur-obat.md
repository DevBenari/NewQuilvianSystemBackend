# PLAN-REPAIR-015 — Tampilan EWS yang konsisten dengan server, daftar "Perlu Lapor Dokter" per bangsal, dan pintu retur obat dari e-MAR; kalkulator EWS baru dan endpoint diet dibatalkan

```yaml
plan_id: PLAN-REPAIR-KEP-015
issue: ../issue/issue-015-kalkulasi-ews-pemesanan-diet-gizi-dan-retur-obat.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-11-02"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "396c8363 (MHamzah)"
basis_source_frontend: "9c4000724 (HamzahV2)"
```

> **Catatan revisi 2 (2026-11-02).** Versi 1 berisi tiga perbaikan. `FIX-KEP-015-01` (kalkulator MEWS dan
> kolom skor baru) akan menggantikan rumus yang sudah berjalan dengan ambang yang belum disahkan, dan
> menambah kolom ganda. `FIX-KEP-015-02` (endpoint `diet-orders`) akan menduplikasi endpoint diet yang
> sudah ada. Keduanya **dibatalkan**. `FIX-KEP-015-03` dipertahankan dengan isi yang dikoreksi: memakai alur
> retur Farmasi yang sudah ada. Lima perbaikan baru ditambahkan dari hasil verifikasi ulang.

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-KEP-015-01` | `ISS-KEP-015-01` | **DIBATALKAN.** Evaluator MEWS baru dan kolom `EwsTotalScore`/`EwsRiskCategory` tidak dibuat; skor sudah dihitung `PatientVitalSignCalculation` dan disimpan pada `TrxPatientVitalSign` | — | — | — | ⛔ TERBLOKIR — dibatalkan, jangan dikerjakan; menunggu pemilik mengesahkan pembatalan (K-05) | `PatientVitalSignCalculation.cs:36-38`, `TrxPatientVitalSign.cs:173-178` |
| `FIX-KEP-015-02` | `ISS-KEP-015-02` | **DIBATALKAN.** Endpoint `POST .../diet-orders` tidak dibuat; diet sudah lewat `POST .../episodes/{episodeId}/diets` (`BE-RWI-166`, `FE-RWI-184`) | — | — | — | ⛔ TERBLOKIR — dibatalkan, jangan dikerjakan; menunggu pemilik mengesahkan pembatalan (K-05) | `InpatientDietController.cs:15-23`, `inpatient-diet.service.js:21` |
| `FIX-KEP-015-03` | `ISS-KEP-015-03` | Aksi **Ajukan Retur** pada e-MAR yang membuat draf retur di Farmasi lewat `POST .../drug-returns` | FE (+ hak akses) | 3 | — | ✅ SELESAI 9 November 2026 — `FE-RWI-285`; K-04 diputuskan 2026-11-02; sisi tagihan AC 3 belum dapat dibuktikan di DB dev | [FE-RWI-285](../../task/report/frontend/FE-RWI-285.md) |
| `FIX-KEP-015-04` | `ISS-KEP-015-01` | Tab Monitoring **"EWS Perlu Lapor Dokter"** per bangsal, baca-saja | BE + FE + dokumen | 2 | — | 🟡 SEBAGIAN — `BE-RWI-255` ✅ dan `FE-RWI-284` ✅ 9 November 2026; revisi `api-contract.md` dan skema tampilan belum (Slice 6 MVP) | [BE-RWI-255](../../task/report/backend/BE-RWI-255.md), [FE-RWI-284](../../task/report/frontend/FE-RWI-284.md) |
| `FIX-KEP-015-05` | `ISS-KEP-015-01`, `ISS-KEP-015-T4` | Skor EWS anak dan obstetri, serta penyesuaian acuan EWS dewasa | BE + FE | 4 | — | ⛔ TERBLOKIR — menunggu K-01 dan K-02 dari pemilik klinis keperawatan | — |
| `FIX-KEP-015-06` | `ISS-KEP-015-T1` | Serah terima shift perawat (SBAR) | BE + FE | 5 | — | ⛔ TERBLOKIR — menunggu `requirement-completeness-gate` (`Sisa-Pekerjaan-Rawat-Inap.md` bagian 7) | — |
| `FIX-KEP-015-07` | `ISS-KEP-015-T2`, `ISS-KEP-015-T3` | Tampilan EWS mengikuti server: Pengawasan Harian membaca `earlyWarningScore`; tab Vital Sign memakai `ewsRiskLevel` dan `ewsMonitoringRecommendation` | FE | 1 | — | ✅ SELESAI 9 November 2026 — `FE-RWI-283` | [FE-RWI-283](../../task/report/frontend/FE-RWI-283.md) |
| `FIX-KEP-015-08` | `ISS-KEP-015-01` | Tanda "sudah dilaporkan ke dokter" dan pemberitahuan aktif ke dokter jaga | BE + FE | 4 | — | ⛔ TERBLOKIR — menunggu K-03 (protokol eskalasi) dari pemilik klinis | — |

**Ringkasan: 0 dari 6 perbaikan aktif selesai. Dua perbaikan dibatalkan dan tidak dihitung.**

---

## 2. Solusi terpilih per temuan

### ISS-KEP-015-01 — Peringatan EWS tidak sampai ke siapa pun

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Kalkulator MEWS baru di backend (usulan versi 1) | — | Skor sudah dihitung. Rumus baru dengan ambang yang belum disahkan akan mengubah golongan risiko semua pasien tanpa keputusan klinis |
| B | Pakai `GET .../patient-vital-signs/critical-alerts` apa adanya | Tanpa backend baru | Endpoint itu tidak dapat disaring per bangsal, tidak punya batas waktu, mengembalikan paling banyak 100 baris dari semua modul (IGD, poliklinik, rawat inap), dan memuat penanda lama yang tidak pernah padam (`PatientVitalSignController.cs:128-185`) |
| **C** | **Tab Monitoring rawat inap baru, "EWS Perlu Lapor Dokter": satu baris per pasien rawat inap aktif di bangsal terpilih, dari tanda vital terakhirnya** | Mengikuti pola delapan tab Monitoring yang sudah ada (`monitoring-tabs.jsx`, `InpatientMonitoringController`); milik sub-modul rawat inap sendiri; tidak mengubah endpoint bersama `ClinicalManagement` | Butuh satu endpoint baca baru |

**Solusi terpilih: Opsi C**, ditambah perbaikan tampilan `FIX-KEP-015-07` lebih dulu.

1. Perawat kepala dan dokter jaga butuh **satu daftar per bangsal**. Opsi B tidak dapat memberikannya.
2. Tanda vital **terakhir** per pasien yang dipakai, sehingga pasien yang sudah membaik tidak terus muncul
   karena penanda lama.
3. Skor anak/obstetri (`FIX-KEP-015-05`) dan pemberitahuan aktif (`FIX-KEP-015-08`) menunggu keputusan
   klinis. Daftar baca-saja tidak menunggu keduanya.

**Kenapa bukan opsi lain.** Opsi A membangun ulang yang sudah ada dan mengarang ambang. Opsi B memberi
daftar yang salah cakupan.

### ISS-KEP-015-02 — Diet bangsal

Tidak ada opsi yang perlu dipilih. Fiturnya sudah berjalan. **Solusi terpilih: tidak mengubah source.**
Yang perlu diperbarui hanya `02-module-map.md` yang usang (bagian 7).

### ISS-KEP-015-03 — Pintu retur dari e-MAR

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Slip retur, method `ProcessMedicationReturnAsync`, dan *credit note* baru (usulan versi 1) | — | Menduplikasi `DrugReturnController` dan `BillingDrugReturnService` yang sudah berjalan |
| **B** | **Aksi Ajukan Retur pada e-MAR yang membuat draf lewat `POST .../drug-returns`** | Stok dan tagihan tetap diurus alur Farmasi dan Billing yang sudah ada; pemeriksaan apoteker tetap terjaga | Butuh hak akses dan depo penerima yang jelas (K-04) |
| C | Tetap manual di menu Farmasi | Tanpa perubahan | Bergantung pada ingatan perawat |

**Solusi terpilih: Opsi B.** Satu-satunya yang hilang adalah pintu masuknya. Stok baru bertambah setelah
apoteker memeriksa (`DrugReturnController.cs:14-18`), dan aturan itu tidak boleh dilompati dari bangsal.

### ISS-KEP-015-T2 dan T3 — Tampilan EWS tidak konsisten

Hanya ada satu cara yang wajar: **layar memakai field server apa adanya.** Golongan dari `ewsRiskLevel`,
anjuran dari `ewsMonitoringRecommendation`, dan label dari konstanta yang sudah dipakai kartu dokter
(`EWS_RISK_LABEL`, `EWS_RISK_TONE` pada
`src/lib/constants/health-services/inpatient-management/inpatient-progress-note-constants.jsx:87-99`).
Menghitung ulang golongan di layar adalah sumber T3.

### ISS-KEP-015-T1 — SBAR

Tidak dipilih di sini. Kemampuan ini berstatus `Missing` dan wajib melewati `requirement-completeness-gate`
lebih dulu.

---

## 3. Skema tampilan sebelum → sesudah

### A. Tab Vital Sign — ringkasan dan tabel EWS (`FIX-KEP-015-07`)

**SEBELUM** — `nursing-vital-sign-tab.jsx:65-105`, `:325-327`, `:526-536`

```text
┌ Observasi Terkini Pasien ─────────────────────────────────────────────┐
│ EWS (Server): Skor 6 — Risiko Sedang                                   │  ← ditafsirkan di layar
│ (Konsultasi dokter jaga / eskalasi asuhan klinis)                      │  ← anjuran ditulis di layar
└────────────────────────────────────────────────────────────────────────┘
Tabel: skor 1–2 berwarna kuning; skor 5–6 berwarna "sedang"
```

**SESUDAH**

```text
┌ Observasi Terkini Pasien ─────────────────────────────────────────────┐
│ EWS: 6 · [ Tinggi ]  ⚠ Perlu lapor dokter                              │
│ Monitoring lebih sering dan informasikan dokter penanggung jawab.      │  ← EwsMonitoringRecommendation
└────────────────────────────────────────────────────────────────────────┘
Tabel: warna badge mengikuti ewsRiskLevel tiap baris
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Skor | Angka EWS | `earlyWarningScore` | Teks yang sudah ada |
| Golongan | Rendah / Sedang / Tinggi / Kritis | `ewsRiskLevel` → `EWS_RISK_LABEL` | Badge dengan `EWS_RISK_TONE` |
| Penanda lapor | "⚠ Perlu lapor dokter" | `needDoctorNotification` bila dikirim seri tanda vital; bila tidak, golongan `High`/`Critical` | Teks peringatan |
| Anjuran | Kalimat anjuran | `ewsMonitoringRecommendation` | Teks kecil |

| Keadaan | Tampilan |
| --- | --- |
| Belum ada tanda vital | "Belum ada tanda vital" seperti sekarang |
| Skor kosong (`ewsRiskLevel` = `Unknown`) | "EWS: -" tanpa badge |
| Gagal memuat | Pesan galat dan tombol Muat Ulang seperti sekarang |

### B. Pengawasan Harian — kolom EWS / MAP (`FIX-KEP-015-07`)

```text
SEBELUM  (daily-monitoring-section.jsx:294)      SESUDAH
EWS: -    MAP: 62                                 EWS: 15 [ Kritis ]    MAP: 62
```

### C. Monitoring Rawat Inap — tab baru "EWS Perlu Lapor Dokter" (`FIX-KEP-015-04`)

**SEBELUM.** Tab ini belum ada. Tab Monitoring yang ada: Penutupan Tertunda, Ditutup Tanpa Izin Kasir,
Pesanan Tertagih, Pulang Sebelum Izin Kasir, Tanpa Perawat PJ, Penempatan Tidak Sesuai Isolasi, Serah
Terima Pasca Operasi, dan Permintaan Admisi.

**SESUDAH**

```text
Monitoring Rawat Inap
[ … ] [ Pulang Sebelum Izin Kasir ] [ EWS Perlu Lapor Dokter (3) ] [ … ]
Bangsal ▾ [ Lantai 3 — Mawar ]          Golongan ▾ [ Tinggi & Kritis ]
┌──────────┬───────────────┬──────┬────────┬──────────┬─────────────────┬──────────────┐
│ Kamar    │ Pasien        │ EWS  │ Gol.   │ Diukur   │ Diukur oleh     │              │
├──────────┼───────────────┼──────┼────────┼──────────┼─────────────────┼──────────────┤
│ 302-A    │ Tn. J · RM-01 │ 15   │ Kritis │ 02:10    │ Ns. K           │ [ Buka ]     │
│ 305-B    │ Ny. L · RM-02 │ 6    │ Tinggi │ 01:40    │ Ns. K           │ [ Buka ]     │
└──────────┴───────────────┴──────┴────────┴──────────┴─────────────────┴──────────────┘
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Penyaring | Bangsal, golongan | Master unit yang sudah dipakai tab Monitoring lain | Pola penyaring tab Monitoring yang ada |
| Tabel | Satu baris per pasien rawat inap aktif, dari tanda vital **terakhir**-nya | Endpoint baru `FIX-KEP-015-04` | Tabel tab Monitoring yang ada |
| Jumlah pada tab | Banyak baris | Total dari endpoint | Badge jumlah seperti tab lain |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Buka | Tautan | Selalu | Membuka ruang kerja keperawatan pasien pada tab Vital Sign |

| Keadaan | Tampilan |
| --- | --- |
| Kosong | "Tidak ada pasien dengan EWS Tinggi atau Kritis pada bangsal ini" |
| Gagal memuat | Pesan galat dan tombol Muat Ulang |
| Tidak berhak | Tab tidak tampil, mengikuti aturan hak akses Monitoring |

### D. e-MAR — aksi Ajukan Retur (`FIX-KEP-015-03`)

**Belum digambar.** Isi formnya bergantung pada K-04: siapa yang mengajukan dan depo mana yang menerima.
Rangka disusun setelah K-04 diputuskan. Yang sudah pasti: form hanya membuat **draf**, lalu menampilkan
pesan "Retur diajukan; stok dan tagihan berubah setelah diperiksa Farmasi".

### E. Diet Medis dan Konsultasi Gizi

**Tidak ada perubahan tampilan.** Fiturnya sudah berjalan (`FE-RWI-174`, `FE-RWI-184`). Rangka "SEBELUM"
versi 1 ("Integrasi belum tersedia" pada tab gizi) tidak sesuai source.

---

## 4. Rincian perbaikan

### FIX-KEP-015-01 — DIBATALKAN

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-015-01` |
| **Alasan pembatalan** | Skor sudah dihitung dan disimpan. Rumus usulan versi 1 juga berbeda dari rumus yang berjalan tanpa keputusan klinis. Contoh: sistolik ≤ 70 bernilai 3 menurut usulan, padahal sistolik ≤ 90 sudah bernilai 3 di source |

Tidak ada langkah. `EwsCalculator` dan kolom `EwsTotalScore`/`EwsRiskCategory` **tidak boleh** dibuat.
Perubahan rumus hanya lewat `FIX-KEP-015-05` setelah K-01 dan K-02.

### FIX-KEP-015-02 — DIBATALKAN

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-015-02` |
| **Alasan pembatalan** | Diet dan konsultasi gizi sudah berjalan: `InpatientDietController.cs:15-23`, `InpDietOrderAdapter.cs:29-35`, `InpatientNutritionOrderController.cs:22`; task `BE-RWI-164`, `BE-RWI-166`, `FE-RWI-174`, `FE-RWI-184` ✅ |

Tidak ada langkah.

---

### FIX-KEP-015-03 — Aksi Ajukan Retur pada e-MAR

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-015-03` |
| **Area** | Frontend keperawatan; memakai API Farmasi yang sudah ada |
| **Jenis perubahan** | Source frontend; konfigurasi hak akses peran; revisi skema tampilan |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/medication/panels/medication-administration-chart-panel.jsx` beserta modal baru di folder yang sama; memakai `src/lib/services/health-services/pharmacy-management/drug-return.service.js` yang sudah ada |
| **Radius dampak** | e-MAR keperawatan; antrean draf retur di menu Retur Obat Farmasi (milik `PharmacyManagement`) |
| **Bergantung pada** | K-04; langkah 1 di bawah |

**API yang dipakai** — grup Swagger `Health Services / Pharmacy Management / Drug Return`, base URL
`api/v1/health-services/pharmacy-management/drug-returns`:

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menyusun retur sebagai draf; belum menyentuh stok | `DrugReturn : Create` | `CreateDrugReturnRequest`: `EncounterId`, `StorageLocationId`, `ReturnedByWorkforceId`, `Reason`, `Items`, `IdempotencyKey` | Detail retur |
| `POST` | `/{id}/submit` | Mengajukan draf untuk diperiksa Farmasi | `DrugReturn : Update` | — | Detail retur |

Kode 403 berarti peran pengguna belum diberi hak mengajukan retur. Kode 409 berarti retur yang sama sudah
diajukan (kunci *idempotency*, yaitu penanda agar kiriman ganda tidak tercatat dua kali).

**Langkah.**

1. Buktikan dulu `DUGAAN` pada issue: buat satu retur uji di menu Retur Obat Farmasi untuk pasien rawat
   inap uji yang menerima obat UDD. Periksa dua hal setelah diperiksa apoteker: stok kembali, dan invoice
   pasien berkurang. Bila salah satunya gagal, berhenti dan laporkan ke pemilik Farmasi/Billing.
2. Setelah K-04 diputuskan, revisi `keperawatan/05-skema-tampilan.md` untuk e-MAR.
3. Tambahkan aksi **Ajukan Retur** per item obat pada e-MAR. Aksinya membuka modal berisi item, jumlah,
   dan alasan, dengan `EncounterId` dan item terisi otomatis dari episode.
4. Kirim draf lewat `POST /drug-returns`, lalu `POST /{id}/submit`.

**Acceptance criteria.**

1. DPJP menghentikan Meropenem 1 g pada hari kedua. Di kotak obat masih ada 9 vial. Perawat menekan Ajukan
   Retur pada baris Meropenem, mengisi jumlah 9 dan alasan "Terapi diganti DPJP", lalu mengirim. Retur
   muncul di antrean Retur Obat Farmasi berstatus diajukan.
2. Sebelum apoteker memeriksa, stok Meropenem di depo belum bertambah, dan invoice pasien belum berubah.
3. Setelah apoteker memeriksa dan menerima 9 vial, stok bertambah 9 dan invoice pasien berkurang sebesar
   harga 9 vial.
4. Perawat menekan Kirim dua kali dengan cepat. Hanya satu retur yang tercatat.
5. Perawat tanpa hak `DrugReturn : Create` dan `DrugReturn : Update` tidak melihat aksi Ajukan Retur.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`, lalu uji langsung skenario
kriteria 1-3.

**Risiko.** Jumlah retur melebihi yang pernah dikeluarkan. Validasinya tetap milik Farmasi saat
memeriksa. Layar hanya membatasi jumlah agar tidak melebihi jumlah yang tercatat diserahkan.

---

### FIX-KEP-015-04 — Tab Monitoring "EWS Perlu Lapor Dokter"

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-015-01` (bagian daftar peringatan) |
| **Area** | Backend dan frontend rawat inap, ditambah revisi dokumen desain |
| **Jenis perubahan** | Endpoint baca baru, tab Monitoring baru, revisi skema tampilan |
| **Berkas yang diubah** | Backend: `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs`, query monitoring rawat inap yang sudah dipakai controller itu, DTO monitoring. Frontend: `src/components/view/health-services/inpatient-management/monitoring/components/monitoring-tabs.jsx`, `src/lib/constants/health-services/inpatient-management/inpatient-monitoring-constants.jsx`, beserta tabel tabnya |
| **Radius dampak** | Halaman Monitoring Rawat Inap saja. Endpoint bersama `critical-alerts` milik `ClinicalManagement` **tidak** diubah |
| **Bergantung pada** | Persetujuan rencana ini dan revisi skema tampilan |

**API yang ditambahkan** — grup Swagger `Health Services / Inpatient Management / Inpatient Monitoring`,
base URL `api/v1/health-services/inpatient-management/monitoring`:

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/ews-alerts` — **Rencana (belum tersedia)** | Pasien rawat inap aktif yang tanda vital **terakhirnya** berisiko Tinggi atau Kritis, atau menyalakan penanda lapor dokter | `InpatientMonitoring : Read`, sama dengan tab Monitoring lain | Query: `ServiceUnitId`, `MinimumRiskLevel`, `PageNumber`, `PageSize` | `PagedResult` berisi episode, kamar, pasien, skor, golongan, waktu ukur, dan pengukur |

**Langkah.**

1. Revisi `keperawatan/05-skema-tampilan.md` (dan bagian Monitoring pada `episode-rawat-inap/05-skema-tampilan.md`)
   sesuai rangka 3.C.
2. Tambahkan query: ambil episode rawat inap aktif di bangsal terpilih. Untuk tiap episode, ambil tanda vital
   terakhir yang aktif dan bukan `Cancelled`/`EnteredInError`. Saring yang `EwsRiskLevel` ≥ golongan minimum
   atau `NeedDoctorNotification` bernilai benar.
3. Tambahkan tab dan tabelnya pada halaman Monitoring.

**Acceptance criteria.**

1. Bangsal Mawar memuat 3 pasien: Tn. J (EWS terakhir 15), Ny. L (6), dan Tn. M (2). Dengan golongan
   minimum "Tinggi", tab menampilkan Tn. J dan Ny. L, diurutkan dari skor tertinggi.
2. Ny. L diukur ulang pukul 03.00 dengan EWS 2. Ny. L hilang dari daftar walaupun pengukuran pukul 01.40
   masih menyalakan penanda lapor dokter.
3. Tanda vital Tn. J pukul 02.10 dibatalkan sebagai salah input (`EnteredInError`). Daftar memakai
   pengukuran sebelumnya.
4. Pasien yang episodenya sudah `Closed` tidak tampil.
5. Pasien IGD dan poliklinik tidak pernah tampil.

**Verifikasi.** `dotnet build`; frontend `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji
manual kelima kriteria.

**Risiko.** Query per bangsal berat bila tanda vital sangat banyak. Batasi pada episode aktif lebih dulu,
lalu ambil satu baris terakhir per episode.

---

### FIX-KEP-015-05 — Skor EWS anak dan obstetri, serta penyesuaian acuan dewasa (tertahan)

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-015-01` (bagian skor anak/obstetri), `ISS-KEP-015-T4` |
| **Bergantung pada** | K-01, K-02 |
| **Radius dampak bila dikerjakan** | `PatientVitalSignCalculation` dipakai bersama IGD, poliklinik, dan rawat inap. `PatientAssessmentController.cs:2851` juga memiliki salinan `CalculateEwsScore` sendiri. Keduanya harus diubah bersama, atau salinannya dihapus lebih dulu |

Rincian disusun setelah pemilik klinis menetapkan acuan, ambang, dan cara mengenali pasien anak atau ibu
hamil. Tidak ada ambang yang boleh ditulis sebelum itu.

### FIX-KEP-015-06 — Serah terima shift SBAR (tertahan)

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-015-T1` |
| **Bergantung pada** | `requirement-completeness-gate`, lalu `design-business-module` dan `plan-module-delivery` |

Tidak ada langkah di plan repair ini. SBAR adalah kemampuan baru, bukan perbaikan.

---

### FIX-KEP-015-07 — Tampilan EWS mengikuti server

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-015-T2`, `ISS-KEP-015-T3` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `nursing-workspace/sections/daily-monitoring/daily-monitoring-section.jsx` (baris 294); `nursing-workspace/sections/nursing-care/tabs/nursing-vital-sign-tab.jsx` (baris 65-105, 325-327, 526-536) |
| **Radius dampak** | Dua layar keperawatan itu saja. Konstanta `EWS_RISK_LABEL`/`EWS_RISK_TONE` hanya dibaca, tidak diubah, sehingga kartu dokter tidak terdampak |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Pengawasan Harian: baca `earlyWarningScore` dan `ewsRiskLevel`, bukan `ewsScore`.
2. Tab Vital Sign: hapus penafsiran skor di layar (`getEwsInterpretation` dan ambang tabel). Pakai
   `ewsRiskLevel` dengan `EWS_RISK_LABEL`/`EWS_RISK_TONE`, dan `ewsMonitoringRecommendation` untuk anjuran.
3. Hapus teks anjuran yang ditulis di layar, misalnya "Aktivasi Tim Emergensi Medis (Code Blue)", sampai
   protokol K-03 disahkan.

**Acceptance criteria.**

1. Tn. J tercatat EWS 15. Pengawasan Harian menampilkan "EWS: 15 [Kritis]", bukan "EWS: -".
2. Pasien dengan EWS 6 tampil "Tinggi" pada ringkasan dan tabel tab Vital Sign, sama dengan kartu dokter.
3. Pasien dengan EWS 2 tampil "Rendah", tidak berwarna kuning.
4. Pasien dengan EWS 4 tampil "Sedang".
5. Kalimat anjuran di ringkasan sama persis dengan `ewsMonitoringRecommendation` dari server.
6. Tanda vital tanpa skor tampil "EWS: -" tanpa badge.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit` (tambahkan unit test pemetaan golongan bila pola
test layar itu sudah ada), `npm run build`.

**Risiko.** Tidak ada perubahan data. Yang berubah hanya cara tampil.

---

### FIX-KEP-015-08 — Tanda "sudah dilaporkan" dan pemberitahuan aktif (tertahan)

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-015-01` (bagian eskalasi) |
| **Bergantung pada** | K-03 |

Saat ini penanda `NeedDoctorNotification` hanya padam bila tanda vital dihapus atau dibatalkan
(`PatientVitalSignController.cs:805`, `:861`). Belum ada aksi "sudah dilaporkan ke dokter". Siapa yang
menandai, kapan penanda padam, dan lewat kanal apa dokter jaga diberi tahu semuanya bagian dari protokol
eskalasi K-03. Rincian disusun setelah protokol itu disahkan.

---

## 5. Urutan pengerjaan

```text
FIX-KEP-015-07   tanpa dependency — boleh dikerjakan setelah rencana disetujui
FIX-KEP-015-04   tanpa dependency teknis — memakai label dari FIX-07 bila sudah selesai
K-03 (protokol eskalasi)
└── ⛔ FIX-KEP-015-08
K-01 + K-02 (acuan skor klinis)
└── ⛔ FIX-KEP-015-05
K-04 (peran, hak akses, depo penerima)
└── ⛔ FIX-KEP-015-03   langkah 1 (retur uji) boleh dilakukan lebih dulu
requirement-completeness-gate
└── ⛔ FIX-KEP-015-06
K-05 (pengesahan pembatalan)
├── ⛔ FIX-KEP-015-01   dibatalkan
└── ⛔ FIX-KEP-015-02   dibatalkan
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- |
| K-01 | Acuan skor EWS anak (PEWS) dan obstetri, cara mengenali jenis pasien, dan ambangnya | Tetapkan lewat `grill-me` bersama pemilik klinis keperawatan | `FIX-KEP-015-05` |
| K-02 | Acuan EWS dewasa yang resmi, termasuk aturan oksigen tambahan dan aturan satu parameter bernilai 3 | Konfirmasi dengan SOP EWS rumah sakit | `FIX-KEP-015-05` |
| K-03 | Protokol eskalasi: penerima peringatan, kanal, siapa yang menandai "sudah dilaporkan", dan teks anjuran resmi | Daftar di layar lebih dulu (`FIX-KEP-015-04`), pemberitahuan aktif setelah protokol disahkan | `FIX-KEP-015-08` |
| K-04 | Peran pengaju retur dari bangsal, pemberian hak `DrugReturn : Create` (menyusun draf) dan `DrugReturn : Update` (mengajukan), dan depo Farmasi penerima | Perawat mengajukan draf, apoteker bangsal memeriksa, depo penerima sama dengan depo yang mengeluarkan | `FIX-KEP-015-03` |
| K-05 | Pengesahan pembatalan `FIX-KEP-015-01` dan `FIX-KEP-015-02` | Setuju | Penutupan issue |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi | Untuk |
| --- | --- | --- | --- | --- |
| `rawat-inap/02-module-map.md` | Baris 438 (`CAP-015-GIZ`) dan catatan baris 456 | "Integrasi belum tersedia" | "Berjalan lewat adapter rawat inap (`BE-RWI-164`, `FE-RWI-174`); diet medis lewat `BE-RWI-166`, `FE-RWI-184`" | Koreksi dokumen usang |
| `keperawatan/05-skema-tampilan.md` | Tab Vital Sign dan Pengawasan Harian | Belum mengatur sumber golongan EWS | "Golongan, warna, dan anjuran EWS diambil dari field server; layar tidak menghitung ulang" | `FIX-KEP-015-07` |
| `keperawatan/05-skema-tampilan.md` dan bagian Monitoring `episode-rawat-inap/05-skema-tampilan.md` | Monitoring | Belum ada tab EWS | Tab "EWS Perlu Lapor Dokter" sesuai rangka 3.C | `FIX-KEP-015-04` |
| `episode-rawat-inap/contracts/api-contract.md` | Monitoring | Belum ada `GET /monitoring/ews-alerts` | Endpoint baru beserta query dan respons | `FIX-KEP-015-04` |
| `keperawatan/05-skema-tampilan.md` | e-MAR | Belum ada aksi retur | Aksi Ajukan Retur setelah K-04 | `FIX-KEP-015-03` |
| `keperawatan/02-backend-architecture.md`, `keperawatan/contracts/api-contract.md` | — | Versi 1 meminta skema EWS dan endpoint diet baru | **Tidak direvisi** — keduanya dibatalkan | — |

Revisi dokumen desain dikerjakan lewat jalur blueprint yang berwenang, bukan oleh dokumen ini.

---

## 8. Verifikasi menyeluruh

1. **EWS ujung ke ujung.** Perawat menyimpan tanda vital Tn. J (sistolik 85, nadi 128, napas 30, saturasi
   91%, suhu 38,9 °C, kesadaran menurun). Hasil yang diharapkan: tab Vital Sign menampilkan "EWS: 15
   [Kritis]", Pengawasan Harian menampilkan skor yang sama, kartu dokter menampilkan golongan yang sama,
   dan Tn. J muncul di tab Monitoring "EWS Perlu Lapor Dokter" bangsalnya.
2. **Perbaikan klinis.** Tn. J diukur ulang dengan EWS 2. Ia hilang dari tab Monitoring, dan semua layar
   menampilkan "Rendah".
3. **Diet (perilaku yang sudah ada, untuk menutup P-01).** Perawat menetapkan Diet DM 1700 kkal atas
   instruksi DPJP dari tab Diet Medis. Diet muncul di modul Gizi dengan status verifikasi `Pending` sampai
   DPJP memverifikasi.
4. **Retur obat.** Jalankan acceptance criteria 1-3 `FIX-KEP-015-03` setelah K-04.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-02 | Versi 1: rencana diterbitkan, 3 perbaikan, menunggu persetujuan | AI Agent (`diagnose-module-issue`) |
| 2026-11-02 | Revisi 2: verifikasi ulang terhadap source SHA yang sama. `FIX-KEP-015-01` (kalkulator MEWS baru) dan `FIX-KEP-015-02` (endpoint `diet-orders`) dibatalkan karena fiturnya sudah ada. `FIX-KEP-015-03` dikoreksi agar memakai alur retur Farmasi dan Billing yang sudah ada. Ditambahkan `FIX-KEP-015-04` (tab Monitoring EWS), `05` (skor anak/obstetri, tertahan), `06` (SBAR, tertahan), `07` (tampilan EWS mengikuti server), dan `08` (eskalasi aktif, tertahan). Rangka "SEBELUM/SESUDAH" versi 1 yang tidak sesuai source diganti | AI Agent (`diagnose-module-issue`) atas permintaan pemilik |
