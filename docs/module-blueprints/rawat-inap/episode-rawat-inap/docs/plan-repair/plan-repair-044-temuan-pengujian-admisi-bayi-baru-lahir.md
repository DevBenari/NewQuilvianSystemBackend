# PLAN-REPAIR-044 — Rencana Perbaikan Temuan Pengujian Admisi Bayi Baru Lahir

```yaml
plan_id: PLAN-REPAIR-EPS-044
issue: ../issue/issue-044-temuan-pengujian-admisi-bayi-baru-lahir.md
status_rencana: SELESAI
tanggal_rencana: "2026-11-02"
diputuskan_oleh: "Pengguna / Pemilik Alur"
tanggal_keputusan: "2026-11-02"
basis_source_backend: "396c8363 (MHamzah)"
basis_source_frontend: "9c4000724 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | :---: | :---: | --- |
| `FIX-EPS-044-01` | `ISS-EPS-044-01` | Aktivasi `IsAvailableForRegistration` Ruang Bayi (`SU-BABY-001`) dan guard denah | Master / BE | 1 | `BE-RWI-258` | ✅ SELESAI | DB Master aktif (4 unit rawat khusus); guard `EvaluatePlacementEligibilityCore` selesai ([BE-RWI-258.md](../../task/report/backend/BE-RWI-258.md)) |
| `FIX-EPS-044-02` | `ISS-EPS-044-02` | Dekomposisi galat database Postgres dan reset idempotensi pendaftaran bayi | BE + FE | 1 | `BE-RWI-258` / `FE-RWI-287` | ✅ SELESAI | Tangkap `Npgsql.PostgresException` spesifik dan reset ref idempotensi saat error ([BE-RWI-258.md](../../task/report/backend/BE-RWI-258.md), [FE-RWI-287.md](../../task/report/frontend/FE-RWI-287.md)) |
| `FIX-EPS-044-03` | `ISS-EPS-044-03` | Reset lengkap formulir identitas kelahiran saat ganti ibu persalinan | FE | 2 | `FE-RWI-287` | ✅ SELESAI | Residu field lahir dibersihkan saat ganti ibu ([FE-RWI-287.md](../../task/report/frontend/FE-RWI-287.md)) |
| `FIX-EPS-044-04` | `ISS-EPS-044-04` | Pemisahan status dan pesan ketersediaan boks bayi kamar ibu (bukan selalu occupied) | FE | 2 | `FE-RWI-288` | ✅ SELESAI | Status `unavailable` dipisahkan dari `occupied`; alert pemeliharaan ([FE-RWI-288.md](../../task/report/frontend/FE-RWI-288.md)) |
| `FIX-EPS-044-05` | `ISS-EPS-044-05` | Penyegaran aktif denah saat terjadi kegagalan registrasi service unit | FE | 3 | `FE-RWI-288` | ✅ SELESAI | `effectiveFloorPlan?.refresh?.()` dipanggil saat error unit ([FE-RWI-288.md](../../task/report/frontend/FE-RWI-288.md)) |
| `FIX-EPS-044-06` | `ISS-EPS-044-06` | Sinkronisasi pembersihan kata kunci pencarian server-side pada `FilterSelect` | FE | 3 | `FE-RWI-287` | ✅ SELESAI | Panggilan reset kata kunci saat menu ditutup ([FE-RWI-287.md](../../task/report/frontend/FE-RWI-287.md)) |
| `FIX-EPS-044-07` | `ISS-EPS-044-07` | Penambahan batas atas dinamis waktu (maksimal jam saat ini) pada picker jam lahir | FE | 3 | `FE-RWI-287` | ✅ SELESAI | Utilitas `buildNewbornBirthTimeLimits` terintegrasi ([FE-RWI-287.md](../../task/report/frontend/FE-RWI-287.md)) |
| `FIX-EPS-044-08` | `ISS-EPS-044-08` | Koreksi saltik teks "bounding" menjadi "bonding" | FE | 4 | `FE-RWI-287` | ✅ SELESAI | Teks opsi kartu diperbaiki menjadi "bonding" ([FE-RWI-287.md](../../task/report/frontend/FE-RWI-287.md)) |

**Ringkasan: 8 dari 8 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-EPS-044-01 — Penolakan Registrasi Service Unit `SU-BABY-001`

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| **A (Terpilih)** | Aktifkan `IsAvailableForRegistration = true` pada master data dan perbarui kueri denah `InpFloorPlanQueryService.cs` agar hanya menyajikan bed dengan unit layanan yang eligible registrasi | Solutif menyeluruh: alur admisi pulih, dan denah tidak lagi menjanjikan bed palsu | Perlu sentuhan master data dan 1 baris guard kueri backend |
| B | Hanya mengaktifkan data master tanpa menyentuh kode backend denah | Sangat cepat | Jika ada unit lain yang non-registrasi, denah tetap memajang bed "Tersedia" yang berujung error |
| C | Bypass validasi unit layanan di `PatientEncounterController.cs` khusus bayi | Menghilangkan error seketika | Merusak integritas aturan tata kelola registrasi rawat inap |

**Solusi terpilih: Opsi A.**
1. Memulihkan kelayakan transaksi registrasi rawat inap untuk unit layanan bayi sesuai desain resmi.
2. Menghilangkan akar asimetri antara tampilan denah (menjanjikan tersedia) dan penerimaan registrasi backend.

---

### ISS-EPS-044-02 — Konflik Pendaftaran Bayi Ny. Dewi Lestari (HTTP 409)

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| **A (Terpilih)** | Tangkap `PostgresException` spesifik pada `NewbornPatientRegistrationService`, log constraint name, sinkronkan pembacaan urutan dengan `!IsDelete`, dan reset `newbornIdempotencyKey` di FE saat gagal | Menyingkap akar masalah database sebenarnya, memperbaiki benturan urutan anak, dan memulihkan retry di frontend | Memerlukan penyesuaian penanganan exception di backend dan reset ref di frontend |
| B | Hanya mereset idempotency key di frontend | Percobaan kedua mengirim UUID baru | Jika akar masalahnya adalah unique index constraint pada database, pendaftaran tetap akan gagal 409 |

**Solusi terpilih: Opsi A.**
1. Memberikan observabilitas yang jelas jika terjadi galat data database di masa depan.
2. Menghilangkan kunci idempotensi stale di memori browser setelah terjadi kegagalan.

---

### ISS-EPS-044-03 — Retensi Form Identitas Bayi Saat Ibu Diganti

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| **A (Terpilih)** | Panggil `patient.resetNewPatient()` atau reset seluruh atribut kelahiran (`birthDate`, `birthTime`, `gender`, `birthPlace`, dll.) di dalam `selectMother` | Mencegah kontaminasi data rekam medis lintas pasien secara total | Petugas harus mengisi ulang data jika tidak sengaja mengklik ibu lain |
| B | Hanya mengosongkan tanggal lahir | Sebagian data terhapus | Tempat lahir dan jenis kelamin tetap berisiko tertukar |

**Solusi terpilih: Opsi A.**
Integritas data klinis dan keselamatan pasien adalah prioritas mutlak; data lahir bayi wajib steril saat berpindah ibu.

---

### ISS-EPS-044-04 — Pesan Palsu Boks Rawat Gabung Terisi Bayi

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| **A (Terpilih)** | Bedakan status `pickRoomingInCrib`: jika boks ada tapi tidak tersedia, periksa `bed.isOccupied`. Bila false, kembalikan status `unavailable` dengan label status asli | Pesan jujur dan akurat sesuai fakta fisik kamar ibu | Menambah satu kondisi status baru di utilitas |
| B | Pertahankan status `occupied` tetapi ubah kalimatnya menjadi netral | Perubahan teks minimal | Informasi status boks (misal sedang dibersihkan vs rusak) tidak tersampaikan |

**Solusi terpilih: Opsi A.**
Petugas admisi berhak mengetahui status sebenarnya dari boks di kamar ibu (apakah kotor, rusak, atau memang ada bayi lain).

---

### ISS-EPS-044-05 — Jam Data dan Penyegaran Denah Setelah Ditolak

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| **A (Terpilih)** | Panggil `floorPlan.retryFloorPlanPreparation?.()` dan refetch denah saat terjadi kegagalan registrasi unit layanan | Denah langsung dimutakhirkan dan stempel waktu data diperbarui | Menambah pemanggilan fungsi refetch di blok catch hook bed |

**Solusi terpilih: Opsi A.**

---

### ISS-EPS-044-06 — Sinkronisasi Pencarian Server-Side pada `FilterSelect`

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| **A (Terpilih)** | Panggil `onSearchChange("")` saat menu ditutup pada mode `serverSide` jika kata kunci lokal direset | Kolom cari kosong selaras dengan daftar opsi yang kembali tanpa filter | Komponen base component `FilterSelect` disentuh (radius terkontrol) |

**Solusi terpilih: Opsi A.**

---

### ISS-EPS-044-07 — Pembatasan Waktu Masa Depan pada Picker Jam Lahir

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| **A (Terpilih)** | Berikan prop batasan jam (`maxTime` = waktu sekarang) pada `BaseTimeField` ketika `birthDate` adalah hari ini | Mencegah salah pilih waktu masa depan sebelum menekan simpan | Memerlukan penyaluran batas jam dinamis dari parent |

**Solusi terpilih: Opsi A.**

---

### ISS-EPS-044-08 — Koreksi Saltik "bounding"

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| **A (Terpilih)** | Ganti kata `"Mendukung bounding ibu & bayi"` menjadi `"Mendukung bonding ibu & bayi"` pada file konstanta | Koreksi langsung dan bersih | Tidak ada risiko |

**Solusi terpilih: Opsi A.**

---

## 3. Skema Tampilan Sebelum → Sesudah

### A. Langkah 4 — Status Boks Kamar Ibu (ISS-EPS-044-04)

**SEBELUM:**
```text
+--------------------------------------------------------------------------------------------------+
| ADMISI BAYI — Penempatan dan Pemesanan Bayi                                                      |
| Bayi ditempatkan pada boks bayi di kamar perawatan ibu (Rawat Gabung).                           |
+--------------------------------------------------------------------------------------------------+
| ⚠ Boks Bayi di Kamar Ibu Sudah Terisi                                                           |
| Boks 310-BOKS di Kamar 310 saat ini berstatus "Unavailable" (sudah ditempati oleh pasien bayi).  |
| Untuk bayi kembar berikutnya, silakan alihkan jenis perawatan ke Ruang Perinatologi atau NICU... |
+--------------------------------------------------------------------------------------------------+
| [ Periksa Ulang ]  [ Alihkan ke Perawatan Perinatologi ]                                        |
+--------------------------------------------------------------------------------------------------+
```

**SESUDAH:**
```text
+--------------------------------------------------------------------------------------------------+
| ADMISI BAYI — Penempatan dan Pemesanan Bayi                                                      |
| Bayi ditempatkan pada boks bayi di kamar perawatan ibu (Rawat Gabung).                           |
+--------------------------------------------------------------------------------------------------+
| ⚠ Boks Bayi di Kamar Ibu Sedang Tidak Tersedia                                                   |
| Boks 310-BOKS di Kamar 310 saat ini berstatus "Tidak Siap Pakai" (sedang dalam pemeliharaan/     |
| penataan ruangan). Boks tidak sedang ditempati pasien. Silakan hubungi bangsal atau alihkan...  |
+--------------------------------------------------------------------------------------------------+
| [ Periksa Ulang ]  [ Alihkan ke Perawatan Perinatologi ]                                        |
+--------------------------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber Data | Komponen |
| --- | --- | --- | --- |
| Banner Status Boks | Judul dan keterangan status ketersediaan boks | `roomingIn.status` & `crib.statusLabel` | `InformationAlert` (`warning` / `danger`) |
| Tombol Aksi | Tombol periksa ulang atau beralih ke perinatologi | Aksi pengguna | `BaseButton` |

---

## 4. Rincian Perbaikan

### FIX-EPS-044-01 — Aktivasi Service Unit Registrasi dan Guard Denah

- **Target Berkas**:
  - Master Data DB: Eksekusi SQL pembaruan `IsAvailableForRegistration = true` pada unit `SU-BABY-001` (Ruang Bayi) dan `SU-NICU-001` (NICU).
  - Backend: `Areas/HealthServices/InPatientManagement/Services/InpFloorPlanQueryService.cs`.
- **Rincian Perubahan**:
  Pada `InpFloorPlanQueryService.cs`, saat memuat kamar/bed untuk `purpose == "Admission"`, pastikan service unit memenuhi syarat `unit.IsAvailableForRegistration`.
- **Acceptance Criteria**:
  1. Bed `BAYI-1-A` dan `BAYI-2-A` dapat diproses melalui `POST /patient-encounters/admin` tanpa galat 400.
  2. Denah rawat inap tidak menampilkan bed berstatus "Available" jika unit layanannya ditutup untuk registrasi.

---

### FIX-EPS-044-02 — Penanganan Database Exception & Idempotensi Pendaftaran Bayi

- **Target Berkas**:
  - Backend: `Areas/HealthServices/PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs`.
  - Frontend: `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx`.
- **Rincian Perubahan**:
  1. Di backend, buka exception asli Postgres dari `DbUpdateException`. Tangani pelanggaran `UX_MstPatient_DeliveryCase_BirthOrder` dengan pesan eksplisit mengenai urutan kelahiran.
  2. Di frontend, kosongkan `newbornIdempotencyKey.current = ""` pada blok `catch` pendaftaran bayi agar pengulangan tidak terkunci oleh kunci lama.
- **Acceptance Criteria**:
  1. Pendaftaran bayi Ny. Dewi Lestari memberikan log jelas dan pesan yang informatif.
  2. Percobaan kedua setelah gagal tidak terkunci oleh idempotency key lama.

---

### FIX-EPS-044-03 — Reset Lengkap Formulir Identitas Kelahiran

- **Target Berkas**:
  - Frontend: `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js`.
- **Rincian Perubahan**:
  Di dalam `selectMother`, lakukan pembersihan field:
  ```javascript
  patient.form.setValue("birthDate", "");
  patient.form.setValue("birthTime", "");
  patient.form.setValue("gender", "");
  patient.form.setValue("birthPlace", "");
  patient.form.setValue("identityNumber", "");
  patient.form.setValue("birthWeightGram", "");
  patient.form.setValue("birthLengthCm", "");
  ```
- **Acceptance Criteria**:
  1. Mengganti ibu di Langkah 1 membersihkan seluruh isian bayi di Langkah 2.
  2. Tidak ada kebocoran data bayi antar-pasien ibu.

---

### FIX-EPS-044-04 — Penyempurnaan Status Boks Kamar Ibu

- **Target Berkas**:
  - Frontend: `src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js`.
  - Frontend: `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`.
- **Rincian Perubahan**:
  Perbaiki `pickRoomingInCrib` agar mengembalikan `status: "unavailable"` jika boks tidak terisi tetapi tidak siap ditempati. Perbarui pesan peringatan di UI agar tidak menyebut "sudah ditempati pasien bayi".
- **Acceptance Criteria**:
  1. Boks kosong yang unavailable menampilkan pesan pemeliharaan/ketidaksiapan, bukan tuduhan bayi kembar.

---

### FIX-EPS-044-05 — Penyegaran Denah Saat Penolakan Service Unit

- **Target Berkas**:
  - Frontend: `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`.
- **Rincian Perubahan**:
  Panggil pemutakhiran denah aktif di samping penyegaran board legacy ketika penolakan registrasi terjadi.
- **Acceptance Criteria**:
  1. Jam data pada denah diperbarui saat pesan muncul.

---

### FIX-EPS-044-06 — Sinkronisasi Reset Pencarian `FilterSelect`

- **Target Berkas**:
  - Frontend: `src/components/features/base-features/filter-select.jsx`.
- **Rincian Perubahan**:
  Panggil `onSearchChangeRef.current?.("")` pada `resetTransientState` jika `serverSide` bernilai true.
- **Acceptance Criteria**:
  1. Membuka kembali dropdown ibu setelah ditutup memuat seluruh daftar ibu tanpa tersaring kata kunci lama.

---

### FIX-EPS-044-07 — Pembatasan Maksimal Waktu Sekarang di Picker Jam Lahir

- **Target Berkas**:
  - Frontend: `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`.
- **Rincian Perubahan**:
  Kirimkan batas waktu maksimum ke kontrol jam saat tanggal lahir adalah hari ini.
- **Acceptance Criteria**:
  1. Jam di masa depan untuk tanggal hari ini tidak dapat dipilih atau langsung dicegah di picker.

---

### FIX-EPS-044-08 — Koreksi Penulisan Istilah "bonding"

- **Target Berkas**:
  - Frontend: `src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js`.
- **Rincian Perubahan**:
  Ubah teks string literal menjadi `"Mendukung bonding ibu & bayi"`.
- **Acceptance Criteria**:
  1. Tampilan Langkah 3 memuat kata "bonding" yang baku.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-044-01 (Master Data & Backend Service Unit)  [P1 - Blocker]
       │
       ▼
FIX-EPS-044-02 (Backend & Frontend Pendaftaran Bayi) [P1 - High]
       │
       ▼
FIX-EPS-044-03 (Reset Form Ibu Berganti)             [P2 - Medium]
       │
       ▼
FIX-EPS-044-04 (Pesan Boks Kamar Ibu)                [P2 - Medium]
       │
       ├──► FIX-EPS-044-05 (Penyegaran Denah)        [P3 - Low]
       ├──► FIX-EPS-044-06 (FilterSelect Search)     [P3 - Low]
       ├──► FIX-EPS-044-07 (Batas Jam Picker)        [P3 - Low]
       └──► FIX-EPS-044-08 (Saltik bonding)          [P4 - Low]
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan yang Dibutuhkan | Menahan Perbaikan | Rekomendasi |
| :---: | --- | :--- | --- |
| 1 | Persetujuan eksekusi SQL pembaruan `IsAvailableForRegistration = true` pada unit `SU-BABY-001` dan unit rawat inap khusus lainnya di DB dev/master | `FIX-EPS-044-01` | **Setujui segera** agar pengujian alur Langkah 5–8 dapat dilanjutkan |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak ada perubahan arsitektur atau domain model baru; hanya pemulihan data konfigurasi dan perbaikan bug antarmuka/validasi lokal.

---

## 8. Verifikasi Menyeluruh

Setelah perbaikan selesai dikerjakan:
1. Jalankan pengujian ulang dari Langkah 1 hingga Langkah 8:
   - Pilih Ny. Putri Ayu, lanjutkan bayi `RM 00-00-00-48`.
   - Pilih bed `BAYI-2-A` atau `BAYI-1-A` di Langkah 4.
   - Tekan "Lanjut ke Pemesanan" -> verifikasi HTTP 200 (encounter berhasil dibentuk).
   - Jalankan Langkah 5 (Persetujuan Wali), Langkah 6 (Tinjau & Aktifkan), Langkah 7 (Dokumen Bayi), dan Langkah 8 (Kartu Pasien).
2. Verifikasi kasus pendaftaran baru untuk Ny. Dewi Lestari di Langkah 2.
3. Verifikasi pergantian ibu di Langkah 1 membersihkan field formulir di Langkah 2.
4. Verifikasi teks boks rawat gabung dan saltik kata "bonding".

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-11-02 | Dokumen rencana perbaikan dibuat untuk 8 temuan pengujian admisi bayi baru lahir. | AI Agent (`diagnose-module-issue`) |
| 2026-11-02 | Seluruh 8 perbaikan selesai dikerjakan (`BE-RWI-258`, `FE-RWI-287`, `FE-RWI-288`); status diperbarui ke SELESAI. | AI Agent (`diagnose-module-issue`) |

