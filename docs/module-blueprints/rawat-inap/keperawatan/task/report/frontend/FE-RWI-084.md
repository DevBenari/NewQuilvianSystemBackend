# Laporan Perubahan Frontend — `FE-RWI-084`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-RWI-084` |
| **Judul** | Pengawasan Harian Pasien (TTV, Nyeri Terakhir, Cairan Masuk/Keluar, GDS Bangsal, Observasi Harian, Pengingat MAR) |
| **Slice** | Gelombang 3 — `FE-KEP-10` Pengawasan Harian Pasien (Layar baru terpadu) |
| **Roadmap** | [`../../../roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md), Bagian 2 & Kartu `FE-RWI-084` |
| **Traceability** | `FR-KEP-056` (deret & grafik TTV per episode), `FR-KEP-057` (entri cairan wajib arah, sumber, volume ml, waktu, pelaksana), `FR-KEP-058` (penautan dosis MAR pada intake obat), `FR-KEP-059` (total balance per shift & 24 jam murni server), `FR-KEP-060` (koreksi & pembatalan beralasan min 5 karakter), `FR-KEP-061` (satuan GDS wajib dipilih tanpa default bawaan, `RWI-DEC-148`, `VAL-KEP-25a`), `FR-KEP-062` (observasi harian diet, mobilisasi, lingkar perut, agitasi terstruktur), `FR-KEP-063` (pengingat dosis MAR tanpa intake); `FR-KEP-049` (TTV menolak isian nyeri langsung) |
| **Contract Version** | API 7.5 Pengawasan Harian (`DailyMonitoringSummaryResponse`, `FluidBalanceTotalsResponse`, `FluidBalanceEntryDto`, `BloodGlucoseReadingDto`, `DailyObservationDto`) |
| **Dependency** | `FE-RWI-081` ✅ (Workspace V2 Navigation), `FE-RWI-082` ✅ (Assessment Progress Tracker), `FE-RWI-083` ✅ (Clinical Instrument Form Renderer), `BE-RWI-112` ✅ (Summary harian terpadu), `BE-RWI-113` ✅ (Fluid balance & shift totals), `BE-RWI-114` ✅ (Blood glucose & sliding scale bridge), `BE-RWI-115` ✅ (Daily observation diet/mobilization) |
| **Klasifikasi** | `HIGH` — Pusat pengawasan klinis intensif harian bangsal rawat inap, penegakan keselamatan kritis satuan GDS tanpa nilai bawaan (*zero default unit*), kalkulasi balance cairan murni server (*authoritative server balance*), penautan asupan obat MAR, serta integritas audit revisi pengukuran |
| **Task Mode** | `CROSS-REPO` sempit — Kode implementasi di `QuilvianSystemFrontendDev`; laporan tracked dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| **Tanggal** | 18 September 2026 |
| **Status** | ✅ **SELESAI.** Seluruh 7 Acceptance Criteria (AC-1 s.d. AC-7) terbukti penuh. Pengujian unit otomatis lulus 7 dari 7 test (7/7 passing). Seluruh suite keperawatan lulus 100% (91/91 passing). ESLint 0 error 0 warning. Next.js build berhasil. |

---

## 1. Masalah yang Diselesaikan & Rasional Bisnis

### 1.1 Masalah Sebelumnya
1. **Pencatatan Klinis Terfragmentasi (*Fragmented Monitoring*):**
   Sebelumnya, perawat bangsal rawat inap harus berpindah-pindah antar menu dan formulir untuk memantau kondisi harian pasien. Tanda vital berada di satu layar, catatan cairan di buku grafik manual, hasil gula darah sewaktu (GDS) di catatan stiker glukometer, dan asupan makan/mobilisasi tercecer di catatan CPPT bebas. Hal ini memperlambat respon perawat saat terjadi perburukan klinis (*clinical deterioration*).
2. **Kalkulasi Keseimbangan Cairan di Klien (*Client-Side Balance Calculation Risk*):**
   Sistem lama kerap menghitung *fluid balance* (intake minus output) langsung pada peramban perawat menggunakan logika penjumlahan JavaScript. Ketika terjadi keterlambatan sinkronisasi data antar perawat shift pagi, siang, dan malam, angka *cumulative balance* 24 jam menjadi tidak sinkron dan berisiko salah tafsir pada pasien gagal ginjal (*CKD*), syok kardiogenik, atau demam berdarah dengue (*DBD*) yang menuntut restriksi cairan sangat ketat.
3. **Bahaya Maut Nilai Bawaan Satuan Gula Darah (`FR-KEP-061`, `RWI-DEC-148`):**
   Pada antarmuka lama, pilihan satuan gula darah sering diberi nilai bawaan (*default*) misalnya `mg/dL`. Jika seorang dokter spesialis atau glukometer impor menyajikan angka dalam satuan `mmol/L` (misalnya angka `10 mmol/L`), dan perawat langsung menyimpan tanpa mengganti satuan, sistem menganggap gula darah pasien adalah `10 mg/dL` (hipoglikemia berat). Sebaliknya, jika angka `180 mg/dL` tersimpan sebagai `mmol/L`, protokol *sliding scale insulin* akan menginstruksikan dosis insulin mematikan.
4. **Dosis Obat Cair Tanpa Pencatatan Intake Cairan (`FR-KEP-058`, `FR-KEP-063`):**
   Pada sistem sebelumnya, pemberian antibiotik intravena atau rehidrasi obat yang dicatat pada *Medication Administration Record* (MAR) sering terlewat tidak dihitung sebagai asupan cairan (*fluid intake*), sehingga perhitungan keseimbangan cairan harian mengalami defisit tersembunyi (*under-reported intake*).
5. **Pencemaran Data Tanda Vital oleh Input Nyeri Bebas (`FR-KEP-049`, `VAL-KEP-22c`):**
   Formulir tanda vital lama sering menyediakan kolom isian angka nyeri sembarangan tanpa instrumen baku (NRS/Wong-Baker/BPS), yang bertentangan dengan standar Joint Commission International (JCI) dan KARS bahwa pengkajian nyeri wajib terstruktur dan memiliki siklus evaluasi ulang (*reassessment*).
6. **Penghapusan Data Pengukuran Tanpa Jejak Audit:**
   Kesalahan entri volume cairan atau glukosa sering dihapus langsung dari database (*hard delete*), melanggar prinsip *legal medical record* di mana setiap koreksi wajib menyertakan alasan minimal 5 karakter dan riwayat versi lama tetap dapat ditelusuri (*immutable revisions*).

### 1.2 Solusi yang Dihadirkan
Melalui task `FE-RWI-084`:
1. **Layar Terpadu Pengawasan Harian (`FE-KEP-10` / `DailyMonitoringSection`):**
   - Menghadirkan konsol kerja satu hari klinis yang merangkum 6 komponen vital:
     1. Deret Waktu & Grafik Tanda Vital Episode (`FR-KEP-056`).
     2. Panel Monitoring Nyeri Terakhir (Baca-saja dari pengkajian nyeri resmi, `FR-KEP-049`).
     3. Total Keseimbangan Cairan Per Shift & 24 Jam Murni Server (`FR-KEP-059`).
     4. Riwayat Entri Cairan Masuk & Keluar dengan Penautan Dosis MAR (`FR-KEP-057`, `FR-KEP-058`).
     5. Pemeriksaan Gula Darah Sewaktu (GDS) Bangsal (`FR-KEP-061`).
     6. Observasi Terstruktur Diet, Mobilisasi, Lingkar Perut, dan Agitasi Pasien (`FR-KEP-062`).
2. **Penegakan Keselamatan Mutlak: Satuan GDS Wajib Dipilih Tanpa Bawaan (`VAL-KEP-25a`):**
   - Antarmuka formulir pencatatan GDS menginisialisasi satuan sebagai `null` (kosong).
   - Sistem mewajibkan perawat memilih salah satu tombol: `mg/dL` atau `mmol/L`.
   - Tombol simpan diblokir dan menampilkan peringatan keselamatan jika satuan belum dipilih secara sadar.
3. **Keseimbangan Cairan Murni Server (*Authoritative Server Totals*):**
   - Komponen `FluidBalanceTotalsCard` hanya menampilkan data hasil agregasi server (`shifts` dan `day`).
   - Frontend **sama sekali dilarang** menjumlahkan volume entri di sisi klien.
   - Pada unit/ruangan yang belum memiliki konfigurasi shift, sistem menampilkan total balance 24 jam saja dengan lencana khusus tanpa membuat shift fiktif (`AC-6`).
4. **Pengingat Dosis MAR Tanpa Intake Cairan (`FR-KEP-063`):**
   - Banner `AdministeredDosesReminderCard` menampilkan daftar obat injeksi/infus yang telah berstatus *Administered* di MAR namun belum dicatatkan volume cairannya hari ini.
   - Perawat dapat menekan tombol pintasan *"Catat Intake Cairan"* untuk langsung membuka formulir pencatatan cairan dengan dosis MAR terpilih otomatis.
5. **Pemisahan Tegas TTV vs Pengkajian Nyeri (`FR-KEP-049`):**
   - Formulir catat TTV rawat inap tidak memiliki kolom isian nyeri. Nyeri disajikan sebagai kartu ringkasan informasi yang menaut langsung ke sub-tab *Monitoring Nyeri*.
6. **Audit Trail Koreksi & Pembatalan Beralasan (`FR-KEP-060`):**
   - Pembatalan maupun koreksi entri cairan, GDS, dan observasi mewajibkan alasan minimal 5 karakter serta menyertakan `expectedRevisionNumber` untuk mencegah tabrakan edit konkuren (*concurrency control*).

---

## 2. Alur Proses Bisnis & Skenario Rumah Sakit

### 2.1 Skenario 1: Pemantauan Keseimbangan Cairan Ketat pada Pasien DBD Grade II
* **Konteks:** Tn. Budi (34 tahun) dirawat di Ruang Dahlia dengan diagnosis Dengue Hemorrhagic Fever (DHF) Grade II dengan trombositopenia (45.000/uL). Dokter spesialis penyakit dalam (DPJP) menginstruksikan pemantauan ketat asupan cairan infus RL 2.500 ml/24 jam dan pemantauan diuresis minimal 0,5–1 ml/kgBB/jam.
* **Alur Perawat Pagi (Pukul 08:00–14:00):**
  1. Perawat Ani membuka layar **Pengawasan Harian**. Tanggal otomatis terpilih hari ini.
  2. Ani mencatat asupan cairan botol pertama infus RL 500 ml melalui tombol *"+ Catat Cairan"*. Dipilih arah *"Cairan Masuk (Intake)"*, kategori sumber *"Infus Intravena"*, volume *"500 ml"*.
  3. Pukul 12:00, pasien buang air kecil di pispot. Ani mengukur volume urin 450 ml dan mencatatnya sebagai *"Cairan Keluar (Output)"* kategori *"Urin"*.
  4. Pukul 14:00 (pergantian shift), Ani melihat kartu **Keseimbangan Cairan**. Server menghitung secara otomatis:
     - *Shift Pagi (07:00–14:00):* Masuk 500 ml, Keluar 450 ml, Balance: `+50 ml`.
     - *Total 24 Jam Berjalan:* `+50 ml`.
  5. Seluruh angka bertanda *"(Kalkulasi Server)"*, menjamin tidak ada manipulasi data saat timbang terima jaga dengan perawat shift sore.

### 2.2 Skenario 2: Penautan Dosis Obat MAR ke Asupan Cairan (`FR-KEP-058`, `FR-KEP-063`)
* **Konteks:** Ny. Ratna (58 tahun) mendapat terapi antibiotik Ceftriaxone 1 gram yang dilarutkan dalam 100 ml NaCl 0,9% IV drip setiap 12 jam.
* **Alur Penautan Terarah:**
  1. Perawat mencatat pemberian obat Ceftriaxone pada rekam medis MAR (status berubah menjadi *Administered*).
  2. Saat membuka layar Pengawasan Harian, muncul banner kuning peringatan:
     > *"Pengingat Asupan Cairan Obat: Ceftriaxone 1 g (Intravena) telah diberikan pukul 09:15, namun belum dicatat sebagai cairan masuk."*
  3. Perawat menekan tombol *"Catat Intake Cairan"* di samping nama obat tersebut.
  4. Modal pencatatan cairan terbuka otomatis dengan arah *"Cairan Masuk"*, kategori sumber *"Obat / Injeksi"*, dan kolom penautan dosis terisi otomatis `#ADM-2026-09-0012 Ceftriaxone`.
  5. Perawat tinggal mengisikan volume aktual pelarut: *"100 ml"* lalu menekan *Simpan*.
  6. Banner pengingat otomatis hilang karena dosis tersebut kini telah resmi terikat dengan entri cairan.

### 2.3 Skenario 3: Penegakan Keselamatan Klinis Satuan GDS Bangsal (`FR-KEP-061`, `RWI-DEC-148`)
* **Konteks:** Tn. Hendra (52 tahun) mengidap Diabetes Melitus Tipe 2 dan sedang dalam protokol *Sliding Scale Insulin Rapid-Acting*.
* **Alur Pemeriksaan GDS Siang:**
  1. Pukul 11:30, perawat melakukan tes tusuk jari (*finger-prick glucose test*) dengan hasil `240`.
  2. Perawat menekan tombol *"+ Catat GDS"*.
  3. Pada formulir, kolom nilai diisi `240`.
  4. Perawat mencoba langsung menekan tombol *"Simpan Nilai GDS"*.
  5. **Respon Sistem:** Tombol penyimpanan menolak proses dan menampilkan peringatan bergaris merah:
     > *"Pilih satuan gula darah: mg/dL atau mmol/L."*
  6. Perawat sadar dan menekan tombol *"mg/dL"*. Banner peringatan hilang.
  7. Data disimpan. Angka `240 mg/dL` tersimpan secara sah dan menjadi rujukan valid bagi kalkulator *Sliding Scale Insulin* tanpa risiko salah tafsir satuan.

### 2.4 Skenario 4: Koreksi Beralasan atas Kesalahan Ketik Volume Urin (`FR-KEP-060`)
* **Konteks:** Perawat Budi keliru mengetik volume urin Tn. Hendra menjadi `1500 ml` padahal di catatan fisik tertera `150 ml`.
* **Alur Koreksi Terkendali:**
  1. Budi menemukan kekeliruan pada tabel riwayat cairan dan menekan tombol ikon pensil (*Koreksi*).
  2. Modal koreksi terbuka menampilkan data lama `1500 ml`.
  3. Budi mengubah nilai menjadi `150 ml`.
  4. Budi mengetikkan alasan singkat: *"typo"*.
  5. Budi menekan *"Simpan Koreksi"*.
  6. **Respon Sistem:** Sistem menolak penyimpanan dengan pesan: *"Alasan koreksi wajib diisi minimal 5 karakter (AC-3)."*
  7. Budi melengkapi alasan menjadi: *"Koreksi kesalahan input volume urin seharusnya 150 ml bukan 1500 ml"*.
  8. Data berhasil disimpan. Versi lama `1500 ml` diarsipkan sebagai revisi #1, data aktif kini bernilai `150 ml` (revisi #2), dan total balance 24 jam dihitung ulang secara otomatis oleh server.

---

## 3. Keputusan Penggunaan Base Component & Modul Desain

| Elemen Antarmuka | Keputusan | Komponen / Sumber | Rationale & Bukti |
| :--- | :--- | :--- | :--- |
| **Modal Dialog** | `REUSE` | `BaseModal` (`@/components/features/base-features/base-modal`) | Menggunakan modal standar sistem yang telah memiliki penanganan *focus-trap*, tombol tutup, responsivitas backdrop, dan ARIA accessibility. Digunakan pada 6 modal pengawasan harian. |
| **Tombol Utama & Sekunder** | `REUSE` | `BaseButton` (`@/components/features/base-features/base-button`) | Menjamin keseragaman visual variant `primary`, `outline`, ukuran `sm`/`md`, serta *loading spinner* bawaan saat submit. |
| **Navigasi Tanggal Klinis** | `NEW` | `DailyMonitoringDateHeader` | Komponen navigasi hari klinis dengan tombol pintas (*Kemarin*, *Besok*, *Hari Ini*), pemilih kalender cepat, dan 4 tombol aksi pencatatan cepat (*Quick Action Bar*). |
| **Keseimbangan Cairan** | `NEW` | `FluidBalanceTotalsCard` | Komponen penampil kartu ringkasan keseimbangan cairan murni dari server dengan penanganan adaptif ruang rawat tanpa shift (`AC-6`). |
| **Pengingat MAR** | `NEW` | `AdministeredDosesReminderCard` | Banner peringatan dosis obat MAR tanpa intake cairan yang tidak memblokir alur utama namun memberikan tombol pintas penautan cairan (`FR-KEP-063`). |
| **Layar Terpadu Monitoring** | `NEW` | `DailyMonitoringSection` | Komponen induk yang merangkum TTV, Nyeri, Cairan, GDS, Observasi, dan Pengingat dalam tata letak responsif 2 kolom dan tabel terstruktur. |
| **Styling & Token Desain** | `EXTENSION` | `nursing-workspace.module.css` | Menambahkan token warna, status badge (`badgeIntake`, `badgeOutput`, `badgeCancelledStatus`), tata letak grid, dan kartu monitoring modern yang konsisten dengan estetika Quilvian V2. |

---

## 4. Spesifikasi Endpoint API Bergaya Swagger

Seluruh interaksi data pengawasan harian dikonsolidasikan melalui service `daily-monitoring.service.js` sesuai kontrak backend API 7.5:

### `[Tags("Daily Monitoring")]` — Ringkasan Pengawasan Harian Terpadu
| Method | Path | Deskripsi | Auth | Request Body / Params | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/health-services/clinical-management/daily-monitoring/episodes/{episodeId}/summary` | Mengambil seluruh data monitoring 1 hari klinis (TTV, Nyeri terakhir, Cairan, GDS, Observasi, Pengingat MAR). | Bearer Token | `date` (Query, opsional, `YYYY-MM-DD`) | `DailyMonitoringSummaryResponse` (Status 200 OK) |

### `[Tags("Fluid Balance")]` — Manajemen Keseimbangan Cairan Masuk & Keluar
| Method | Path | Deskripsi | Auth | Request Body / Params | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/health-services/clinical-management/fluid-balance-entries/episodes/{episodeId}` | Memuat daftar riwayat entri cairan masuk/keluar untuk satu episode. | Bearer Token | `date` (Query, opsional) | `List<FluidBalanceEntryDto>` |
| `GET` | `/v1/health-services/clinical-management/fluid-balance-entries/episodes/{episodeId}/totals` | Memuat total cairan masuk, keluar, dan keseimbangan per shift serta 24 jam murni server. | Bearer Token | `date` (Query, opsional) | `FluidBalanceTotalsResponse` |
| `POST` | `/v1/health-services/clinical-management/fluid-balance-entries` | Mencatat entri cairan baru (wajib arah, kategori, volume, waktu, pelaksana). | Bearer Token | `CreateFluidBalanceEntryRequest` | `FluidBalanceEntryDto` (Status 201 Created) |
| `PUT` | `/v1/health-services/clinical-management/fluid-balance-entries/{id}/correct` | Mengoreksi entri cairan (wajib alasan min 5 karakter dan `expectedRevisionNumber`). | Bearer Token | `CorrectFluidBalanceEntryRequest` | `FluidBalanceEntryDto` (Status 200 OK) |
| `PATCH` | `/v1/health-services/clinical-management/fluid-balance-entries/{id}/cancel` | Membatalkan entri cairan (wajib alasan min 5 karakter dan `expectedRevisionNumber`). | Bearer Token | `CancelFluidBalanceEntryRequest` | `FluidBalanceEntryDto` (Status 200 OK) |
| `GET` | `/v1/health-services/clinical-management/fluid-balance-entries/{id}/revisions` | Membaca riwayat revisi dan jejak audit entri cairan. | Bearer Token | - | `List<FluidBalanceRevisionDto>` |

### `[Tags("Blood Glucose Readings")]` — Pemeriksaan Gula Darah Sewaktu (GDS) Bangsal
| Method | Path | Deskripsi | Auth | Request Body / Params | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/health-services/clinical-management/blood-glucose-readings/episodes/{episodeId}` | Memuat daftar pembacaan GDS bangsal pada episode. | Bearer Token | `date` (Query, opsional) | `List<BloodGlucoseReadingDto>` |
| `POST` | `/v1/health-services/clinical-management/blood-glucose-readings` | Mencatat hasil GDS baru (satuan wajib dipilih: 1=mg/dL, 2=mmol/L, tanpa default). | Bearer Token | `CreateBloodGlucoseReadingRequest` | `BloodGlucoseReadingDto` (Status 201 Created) |
| `PUT` | `/v1/health-services/clinical-management/blood-glucose-readings/{id}/correct` | Mengoreksi nilai atau satuan GDS beralasan min 5 karakter. | Bearer Token | `CorrectBloodGlucoseReadingRequest` | `BloodGlucoseReadingDto` (Status 200 OK) |
| `PATCH` | `/v1/health-services/clinical-management/blood-glucose-readings/{id}/cancel` | Membatalkan hasil GDS beralasan min 5 karakter (diblokir jika dipakai sliding scale). | Bearer Token | `CancelBloodGlucoseReadingRequest` | `BloodGlucoseReadingDto` (Status 200 OK) |
| `GET` | `/v1/health-services/clinical-management/blood-glucose-readings/{id}/revisions` | Membaca riwayat perubahan nilai GDS. | Bearer Token | - | `List<BloodGlucoseRevisionDto>` |

### `[Tags("Daily Observations")]` — Observasi Terstruktur Diet & Mobilisasi
| Method | Path | Deskripsi | Auth | Request Body / Params | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/health-services/clinical-management/daily-observations/episodes/{episodeId}` | Memuat daftar observasi harian diet dan mobilisasi. | Bearer Token | `date` (Query, opsional) | `List<DailyObservationDto>` |
| `POST` | `/v1/health-services/clinical-management/daily-observations` | Mencatat observasi terstruktur (diet %, mobilisasi 0-4, lingkar perut cm, agitasi). | Bearer Token | `CreateDailyObservationRequest` | `DailyObservationDto` (Status 201 Created) |
| `PUT` | `/v1/health-services/clinical-management/daily-observations/{id}/correct` | Mengoreksi parameter observasi harian beralasan min 5 karakter. | Bearer Token | `CorrectDailyObservationRequest` | `DailyObservationDto` (Status 200 OK) |
| `PATCH` | `/v1/health-services/clinical-management/daily-observations/{id}/cancel` | Membatalkan catatan observasi harian beralasan min 5 karakter. | Bearer Token | `CancelDailyObservationRequest` | `DailyObservationDto` (Status 200 OK) |

---

## 5. Bukti Verifikasi Kriteria Keberhasilan (AC-1 s.d. AC-7)

| Kriteria Keberhasilan (AC) | Status | Bukti Implementasi & Verifikasi Otomatis |
| :--- | :---: | :--- |
| **AC-1:** Tanda vital tampil sebagai deret dan grafik per episode (`FR-KEP-056`); menolak isian nyeri langsung (`FR-KEP-049`). | ✅ **LULUS** | Disediakan toggle tombol tabel (`toggle-ttv-table`) dan grafik tren (`toggle-ttv-chart`) pada `DailyMonitoringSection`. Formulir `RecordDailyVitalSignModal` diverifikasi tidak memiliki kolom input nyeri, dan nyeri hanya dibaca dari `latestPainAssessment`. Teruji pada unit test `inpatient-daily-monitoring.test.mjs` (Test 1). |
| **AC-2:** Entri cairan masuk dan keluar mewajibkan sumber, volume ml, waktu, dan pelaksana (`FR-KEP-057`), termasuk penautan dosis MAR pada intake obat (`FR-KEP-058`). | ✅ **LULUS** | Formulir `RecordFluidEntryModal` memvalidasi arah, kategori sumber, volume ml (>0 s.d. 10.000), dan waktu pencatatan. Pada intake obat (kategori 5), dropdown dosis MAR (`select-medication-dose`) wajib dipilih. Teruji pada unit test (Test 2). |
| **AC-3:** Pembatalan dan koreksi entri mewajibkan alasan min 5 karakter, nilai lamanya tetap tersimpan via revisi. | ✅ **LULUS** | `CancelMeasurementModal` dan `CorrectMeasurementModal` memvalidasi alasan `trimmedReason.length >= 5` dan memblokir pengiriman jika alasan kurang dari 5 karakter. Nilai lama tetap tersimpan melalui pengiriman `expectedRevisionNumber`. Teruji pada unit test (Test 3). |
| **AC-4:** Satuan GDS **wajib dipilih, tanpa nilai bawaan/default** (`FR-KEP-061`, `RWI-DEC-148`, `VAL-KEP-25a`). | ✅ **LULUS** | `RecordBloodGlucoseModal` menginisialisasi `glucoseUnit` sebagai `null`. Jika form disubmit tanpa memilih satuan, validasi memblokir dengan pesan `"Pilih satuan gula darah: mg/dL atau mmol/L."`. Hook `useDailyMonitoring` memvalidasi ulang sebelum API dipanggil. Teruji pada unit test (Test 4). |
| **AC-5:** Total balance per shift dan 24 jam murni dari server (`FR-KEP-059`), frontend dilarang menghitung ulang. | ✅ **LULUS** | `FluidBalanceTotalsCard` hanya membaca data agregasi `fluidTotals.shifts` dan `fluidTotals.day` dari respons backend. Tidak ada kalkulasi penjumlahan lokal di frontend. Teruji pada unit test (Test 5). |
| **AC-6:** Unit tanpa konfigurasi shift menampilkan balance 24 jam saja tanpa shift buatan (`ShiftConfigurationMissing = true`). | ✅ **LULUS** | Ketika properti `shiftConfigurationMissing === true`, komponen menampilkan banner peringatan `"Jam shift belum dikonfigurasi..."` dan menyajikan total balance 24 jam saja tanpa merender kolom shift kosong atau buatan. Teruji pada unit test (Test 5). |
| **AC-7:** Observasi harian (diet, mobilisasi, lingkar perut, agitasi) tercatat terstruktur (`FR-KEP-062`). | ✅ **LULUS** | `RecordDailyObservationModal` menyediakan kontrol asupan diet (0-100%), tingkat mobilisasi (0=Belum Dinilai s.d. 4=Mandiri), lingkar perut (20-250 cm), dan radio agitasi/gelisah. Data ditampilkan terstruktur pada kartu observasi harian. Teruji pada unit test (Test 6). |
| **Safety & Routing:** Banner pengingat dosis MAR (`FR-KEP-063`) & integrasi rute workspace keperawatan. | ✅ **LULUS** | `AdministeredDosesReminderCard` menyajikan pengingat dosis MAR tanpa memblokir pekerjaan lain dan menyediakan tombol pintas pencatatan intake cairan. Rute `NursingWorkspaceSections` menampilkan `DailyMonitoringSection` saat `activeTab === "daily-monitoring"`. Teruji pada unit test (Test 7). |

---

## 6. Ringkasan Eksekusi Pengujian Otomatis

```bash
# 1. Eksekusi Unit Test FE-RWI-084:
npx node --test tests/unit/inpatient-daily-monitoring.test.mjs
✔ FE-RWI-084 AC-1: Tanda vital tampil deret/grafik per episode & tolak input nyeri (4.2ms)
✔ FE-RWI-084 AC-2: Entri cairan mewajibkan arah, kategori sumber, volume ml, waktu & tautan MAR (0.9ms)
✔ FE-RWI-084 AC-3: Pembatalan dan koreksi entri mewajibkan alasan min 5 karakter dan simpan revisi (0.8ms)
✔ FE-RWI-084 AC-4: Satuan GDS WAJIB dipilih tanpa nilai bawaan/default (0.8ms)
✔ FE-RWI-084 AC-5 & AC-6: Total balance per shift & 24 jam murni dari server; unit tanpa shift hanya 24 jam (0.7ms)
✔ FE-RWI-084 AC-7: Observasi harian terstruktur: diet (%), mobilisasi (0-4), lingkar perut, agitasi (0.6ms)
✔ FE-RWI-084 Safety & Workflow: Banner pengingat dosis MAR tanpa intake & Rute Terpadu (0.6ms)
tests 7 | pass 7 | fail 0 | duration_ms 88ms

# 2. Eksekusi Seluruh Suite Keperawatan Rawat Inap (91 tests):
npx node --test tests/unit/inpatient-nursing-*.test.mjs tests/unit/inpatient-clinical-instrument-renderer.test.mjs tests/unit/inpatient-daily-monitoring.test.mjs
tests 91 | pass 91 | fail 0 | duration_ms 357ms

# 3. Validasi Kode & Linting:
npx eslint src/lib/services/health-services/clinical-management/daily-monitoring.service.js \
           src/lib/hooks/health-services/inpatient-management/use-daily-monitoring.js \
           src/components/view/health-services/inpatient-management/nursing-workspace/sections/daily-monitoring/ \
           tests/unit/inpatient-daily-monitoring.test.mjs
# Exit Code: 0 (0 errors, 0 warnings)
```
