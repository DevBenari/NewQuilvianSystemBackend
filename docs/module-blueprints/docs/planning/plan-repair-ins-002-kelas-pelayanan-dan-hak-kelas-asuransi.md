# PLAN-REPAIR-INS-002 — Penetapan Kelas Pelayanan, Hak Kelas Polis, dan Perhitungan Naik Kelas pada Coverage Asuransi

```yaml
plan_id: PLAN-REPAIR-INS-002
issue: ../issue/issue-ins-002-kelas-pelayanan-dan-hak-kelas-asuransi.md
status_rencana: MENUNGGU_KEPUTUSAN
tanggal_rencana: "2026-10-16"
diputuskan_oleh: "-"
tanggal_keputusan: "-"
basis_source_backend: "5a6de202 (sukmagp)"
basis_data_verifikasi: "QuilvianNewDevSukma"
perubahan_backend: "Ya — registrasi kunjungan, master asuransi pasien, master penjamin, mesin coverage, billing coverage adapter, penempatan bed, katalog tindakan"
perubahan_frontend: "Ya — form kartu asuransi pasien, master penjamin, modal penempatan bed, picker tindakan dokter"
perubahan_data: "Ya — backfill kelas kunjungan, migrasi hak kelas kartu lama, penonaktifan aturan kelas per penjamin"
```

Rencana ini menutup tujuh issue pada [ISSUE-INS-002](../issue/issue-ins-002-kelas-pelayanan-dan-hak-kelas-asuransi.md). Prinsip yang dipegang:

> **Kelas pelayanan menentukan tarif. Hak kelas polis menentukan batas tanggungan.**
> Setiap kunjungan wajib punya kelas pelayanan. Setiap kartu asuransi wajib punya hak kelas yang berelasi ke master kelas. Bila kelas pelayanan lebih tinggi dari hak kelas, sistem menghitung selisihnya sebagai tanggungan pasien, sesuai aturan penjamin.

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Gerbang keputusan | Status |
| --- | --- | --- | --- | :-: | --- | --- |
| `FIX-INS-002-01` | `ISS-INS-002-01` | Tetapkan kelas pelayanan default untuk kunjungan IGD, MCU, dan Telemedicine | BE | 1 | K-01 | ⏳ BELUM DIMULAI |
| `FIX-INS-002-02` | `ISS-INS-002-02` | Tambah relasi hak kelas (`PatientClassId`) pada kartu asuransi pasien dan snapshot-nya di sumber pembayaran kunjungan | BE + FE + Data | 2 | K-02, K-03 | ⏳ BELUM DIMULAI |
| `FIX-INS-002-03` | `ISS-INS-002-03` | Hitung naik kelas di mesin coverage dan billing; minta persetujuan naik kelas saat penempatan bed | BE + FE | 3 | K-04, K-05, K-06, K-07 | ⏳ BELUM DIMULAI |
| `FIX-INS-002-04` | `ISS-INS-002-04` | Nonaktifkan aturan coverage kamar per penjamin yang digantikan perhitungan naik kelas | Data | 4 | Setelah FIX-03 | ⏳ BELUM DIMULAI |
| `FIX-INS-002-05` | `ISS-INS-002-05` | Tampilkan status coverage di picker tindakan sebelum tindakan disimpan | BE + FE | 2 (paralel) | — | ⏳ BELUM DIMULAI |
| `FIX-INS-002-06` | `ISS-INS-002-06` | Backfill kelas kunjungan lama dan perbaiki tipe pembayaran yang tidak konsisten | Data | 5 | K-08 | ⏳ BELUM DIMULAI |
| `FIX-INS-002-07` | `ISS-INS-002-07` | Tambahkan catatan koreksi pada ISSUE-INS-001 | Dokumen | 6 (paralel) | — | ⏳ BELUM DIMULAI |

**Ringkasan: 0 dari 7 perbaikan selesai. FIX-01, FIX-05, dan FIX-07 dapat dimulai segera. FIX-01 cukup menunggu jawaban K-01, yang sudah punya rekomendasi.**

---

## 2. Solusi terpilih per temuan

### 2.1 ISS-INS-002-01 — Kunjungan IGD tanpa kelas pelayanan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Rekomendasi)** | Perluas `ResolvePatientClassAsync` agar semua kunjungan non-rawat-inap (`Emergency`, `MedicalCheckup`, `Telemedicine`) otomatis memakai kelas **RAWAT JALAN**, sama seperti `Outpatient` | Satu titik perubahan. Konsisten dengan pola rawat jalan. Tarif IGD langsung tersedia karena semua tarif punya baris kelas RAWAT JALAN. | Bila suatu saat IGD punya tarif berbeda dari poli, perlu kelas IGD tersendiri |
| B | Buat kelas baru "GAWAT DARURAT" | Tarif IGD bisa dibedakan | Butuh 18.513 baris tarif baru di `MstTariff` dan buku tarif lima penjamin. Tidak ada data tarif IGD khusus saat ini. |
| C | Fallback di mesin coverage: kelas kosong memakai kelas RAWAT JALAN | Tidak menyentuh registrasi | Kunjungan tetap tersimpan tanpa kelas. Billing, laporan, dan modul lain tetap melihat NULL. Masalah hanya disembunyikan. |

**Solusi terpilih: Opsi A.** Kelas ditetapkan sekali saat registrasi, sehingga semua konsumen (resep, tindakan, billing, laporan) membaca nilai yang sama.

### 2.2 ISS-INS-002-02 — Hak kelas polis berupa teks bebas

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Rekomendasi)** | Tambah `PatientClassId` (FK ke `MstPatientClass`) pada `MstPatientInsurance`. Form memakai dropdown master kelas. `ClassName` dipertahankan sementara sebagai label tampilan lama, lalu dihapus setelah migrasi data. Hak kelas ikut disalin ke sumber pembayaran kunjungan sebagai snapshot. | Relasi formal. Snapshot menjaga kunjungan lama tetap konsisten walau kartu diubah. | Perlu migration dan pemetaan data lama |
| B | Tetap teks, tambah validasi daftar nilai | Tanpa migration | Tetap rapuh. Tidak bisa dibandingkan dengan urutan kelas. |

**Solusi terpilih: Opsi A.**

### 2.3 ISS-INS-002-03 — Tidak ada validasi naik kelas

Pola perhitungan naik kelas berbeda antar penjamin, sehingga disimpan sebagai **pengaturan per penjamin** di `MstInsuranceProvider`:

| Kode pola | Nama | Cara hitung | Cocok untuk |
| --- | --- | --- | --- |
| `ItemTariffDifference` | Selisih tarif per item | Penjamin menanggung setiap item sampai nilai tanggungan pada **tarif kelas hak**. Selisih tarif kelas pelayanan − kelas hak dibayar pasien. | Asuransi swasta/TPA dengan tarif berbeda per kelas (kamar, visite) |
| `RoomDifferenceOnly` | Selisih kamar saja | Hanya item kamar (`IsRoomCharge`) yang dibatasi ke tarif kamar kelas hak. Item lain ditanggung normal. | Polis yang membatasi plafon kamar saja |
| `Prorata` | Prorata seluruh tagihan | Semua item dikalikan rasio *tarif kamar kelas hak / tarif kamar kelas pelayanan* | Polis dengan klausul prorata |
| `NotCovered` | Tidak ditanggung | Naik kelas membuat seluruh perawatan tidak ditanggung | Polis yang melarang naik kelas |

| Opsi teknis | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Rekomendasi)** | Buat satu layanan bersama `InsuranceClassEntitlementService` yang dipanggil oleh `InsuranceCoverageService` (saat order resep/tindakan) **dan** `RegistrationBillingCoverageAdapter` (saat billing). Urutan kelas memakai `InpBedClassComparisonService` yang sudah ada. | Satu sumber kebenaran untuk dua jalur. Memakai ulang pembanding kelas yang sudah diatur lewat SK direksi. | Perlu menyelaraskan dua jalur coverage yang selama ini terpisah |
| B | Perhitungan hanya di billing | Perubahan lebih sedikit | Dokter dan perawat tetap melihat "ditanggung 100%" saat order. Masalah baru terlihat di kasir. |
| C | Aturan coverage per kelas per penjamin (kondisi saat ini, ISS-04) | Tanpa kode baru | Tidak melihat hak kelas pasien. Hasilnya keliru (kasus Andi dan Rina). |

**Solusi terpilih: Opsi A.** Ditambah peringatan dan persetujuan naik kelas saat penempatan bed, sehingga naik kelas tercatat sebagai keputusan sadar pasien atau keluarga.

### 2.4 ISS-INS-002-05 — Picker tindakan tanpa status coverage

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Rekomendasi)** | `GET patient-procedures/master-options` menerima `encounterId` opsional. Bila ada, setiap opsi dihitung coverage-nya dengan `ResolveProcedureAsync` memakai satu konteks penjamin bersama (`explicitContext`). | Sama dengan pola katalog obat (`prescribing-drugs`). Satu panggilan API. | Waktu respons naik. Dibatasi `take ≤ 50`. |
| B | Endpoint terpisah `POST coverage-preview` untuk daftar `procedureId`, dipanggil frontend setelah daftar tampil | Daftar tampil lebih cepat | Dua panggilan. Badge coverage muncul belakangan. |

**Solusi terpilih: Opsi A.** Bila waktu respons di atas 1 detik saat diukur, pindah ke Opsi B.

---

## 3. Skema sebelum → sesudah

### 3.1 Katalog obat IGD (FIX-01)

```text
SEBELUM (kunjungan IGD, PatientClassId = NULL)
+---------------------------------------------------------------+
| Cari obat: panadol                                            |
| PANADOL 500 MG CAPLET                                         |
|   [!] Cakupan belum dapat ditentukan karena tarif belum        |
|       tersedia.                         (tidak bisa disimpan) |
+---------------------------------------------------------------+

SESUDAH (kunjungan IGD otomatis berkelas RAWAT JALAN)
+---------------------------------------------------------------+
| Cari obat: panadol                                            |
| PANADOL 500 MG CAPLET        Rp1.620   [Sebagian 80%]         |
|   Ditanggung AdMedika Rp1.296 · Pasien Rp324                  |
+---------------------------------------------------------------+
```

### 3.2 Form kartu asuransi pasien (FIX-02)

```text
SEBELUM                                    SESUDAH
+------------------------------------+     +------------------------------------+
| Penjamin    [AdMedika          v]  |     | Penjamin    [AdMedika          v]  |
| Nomor Polis [ADM-0098812        ]  |     | Nomor Polis [ADM-0098812        ]  |
| Nama Plan   [Corporate Gold     ]  |     | Nama Plan   [Corporate Gold     ]  |
| Kelas       [                   ]  |     | Hak Kelas * [KELAS I           v]  |
|  (teks bebas, opsional)            |     |  (dropdown master kelas, wajib)    |
+------------------------------------+     +------------------------------------+
```

### 3.3 Penempatan bed rawat inap (FIX-03)

```text
SESUDAH — pasien AdMedika hak KELAS I memilih bed VIP
+---------------------------------------------------------------------+
| Pilih Tempat Tidur                                                  |
| [ ] Kelas I  - R.Anggrek 203-A      Sesuai hak kelas                |
| [x] VIP      - R.Melati 101         [Naik kelas]                    |
|                                                                     |
| [!] Naik kelas dari KELAS I ke VIP.                                 |
|     AdMedika memakai pola "Selisih tarif per item".                 |
|     Estimasi selisih kamar: Rp990.000 per malam ditanggung pasien.  |
|     Selisih visite dan layanan lain dihitung saat order.            |
|                                                                     |
| Persetujuan naik kelas *                                            |
| (o) Atas permintaan pasien/keluarga   ( ) Kamar hak kelas penuh     |
| Nama penyetuju * [Ahmad (suami)          ]                          |
|                                                                     |
| [ Batal ]                                  [ Tempatkan Pasien ]     |
+---------------------------------------------------------------------+
```

### 3.4 Hasil coverage untuk kasus Siti (FIX-03)

| Item (3 malam) | Tarif VIP | Sebelum perbaikan | Sesudah (`ItemTariffDifference`) |
| --- | ---: | --- | --- |
| Room Charge VIP | 3 × Rp2.310.000 | Ditanggung penuh | Ditanggung 3 × Rp1.320.000 = Rp3.960.000. Pasien Rp2.970.000. Status **Sebagian**. Catatan: "Naik kelas dari KELAS I ke VIP; selisih ditanggung pasien." |
| Visite (tarif RS VIP Rp484.000 vs Kelas I Rp363.000) | tarif kontrak VIP | Ditanggung penuh | Ditanggung sampai tarif kontrak Kelas I. Selisih ke pasien. |
| PANADOL 500 MG CAPLET (tarif sama di semua kelas) | Rp1.620 | Mengikuti aturan obat | Tidak berubah, karena tarif kelas hak sama |

### 3.5 Picker tindakan dokter (FIX-05)

```text
SEBELUM                                     SESUDAH (pasien AdMedika)
+--------------------------------------+    +----------------------------------------------+
| GS3 BLEACHING IN OFFICE   [Tarif RS] |    | GS3 BLEACHING IN OFFICE   [Tidak ditanggung] |
| GU2 SCALING               [Tarif RS] |    |   Perawatan gigi estetik dikecualikan polis  |
| Nebulisasi Dewasa         [Tarif RS] |    | GU2 SCALING               [Sebagian 50%]     |
+--------------------------------------+    | Nebulisasi Dewasa         [Ditanggung 100%]  |
                                            +----------------------------------------------+
```

---

## 4. Rincian perbaikan

### FIX-INS-002-01 — Kelas default untuk kunjungan non-rawat-inap

**Berkas yang diubah (backend):**
- [`PatientEncounterController.cs`](../../../../Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs) — `ResolvePatientClassAsync` (baris ±2140) dan komentar di baris ±752.

**Perubahan spesifik:**
1. Ganti kondisi `request.EncounterType == EncounterType.Outpatient` menjadi `request.EncounterType is EncounterType.Outpatient or EncounterType.Emergency or EncounterType.MedicalCheckup or EncounterType.Telemedicine`.
2. `Inpatient` tidak diubah, karena kelasnya ditentukan dari kamar melalui `InpBedOccupancyService.ResolveBilledPatientClassId`.
3. Perbarui pesan galat dan komentar supaya menyebut semua jenis kunjungan yang dicakup.
4. Pastikan `EmergencyVisitService` dan `EncounterIntakeService` (kiosk) tidak membuat encounter dengan jalur lain yang melewati resolver ini. Dari penelusuran, encounter baru hanya dibuat di `PatientEncounterController`, `EncounterIntakeService` (sudah memakai `ResolveOutpatientPatientClassIdAsync`), dan `InpEpisodeService`.

**Tanpa perubahan skema dan tanpa migration.**

**Acceptance criteria:**
- **AC-01:** Kunjungan IGD baru tersimpan dengan `PatientClassId` = kelas RAWAT JALAN, walaupun request tidak mengirim kelas.
- **AC-02:** Katalog obat (`GET prescribing-drugs?encounterId=`) untuk kunjungan IGD baru menampilkan harga dan status coverage, bukan `ConfigurationMissing`.
- **AC-03:** Kunjungan IGD pasien tunai menampilkan status `NotApplicable` dengan harga tarif RS.
- **AC-04:** Kunjungan rawat inap tetap memakai kelas kamar.

---

### FIX-INS-002-02 — Hak kelas polis berelasi ke master kelas

**Berkas yang diubah (backend):**
- [`MstPatientInsurance.cs`](../../../../Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientInsurance.cs): tambah `Guid? PatientClassId` dan navigasi `MstPatientClass? PatientClass`.
- [`MstPatientInsuranceConfiguration.cs`](../../../../Repositories/Configurations/HealthServices/MstPatientInsuranceConfiguration.cs): FK `Restrict` dan index `PatientClassId`.
- `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientInsuranceDtos.cs`: field `PatientClassId` dan `PatientClassName` pada request/response.
- `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientInsuranceController.cs`:
  - validasi kelas aktif, tidak terhapus, dan bertipe kelas rawat (lihat K-07);
  - wajib diisi sesuai K-02.
- `RegPatientEncounterGuarantor`: tambah `EntitledPatientClassIdSnapshot` (prefix `Reg` sudah sesuai registry). Diisi di [`EncounterPaymentSourceService.cs`](../../../../Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs) (baris ±347 dan ±492) dan [`EncounterIntakeService.cs`](../../../../Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs) (baris ±469), di samping `ClassNameSnapshot` yang sudah ada.
- [`EmergencyDispositionService.AdmissionRequest.cs`](../../../../Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.AdmissionRequest.cs) (baris ±1210): `ClassEntitlementText` diisi dari nama master kelas.
- Migration EF: `AddPatientInsuranceEntitledClass` untuk menambah kolom, FK, dan index. Tidak ada kolom yang dihapus pada tahap ini.

**Berkas yang diubah (frontend):**
- `src/lib/constants/health-services/patient-management/master-data/patient-insurance-constants.jsx` (baris ±119): ganti field `className` teks menjadi `patientClassId` bertipe `select`, dengan `optionsSource` master kelas.
- `src/utils/health-services/patient-management/master-data/patient-insurance/patient-insurance-editor-utils.jsx`, `...-display-utils.jsx`, `...-detail-utils.jsx`: mapping payload dan tampilan.

**Migrasi data (skrip terpisah, dengan backup):**
1. Petakan `ClassName` teks ke master kelas dengan normalisasi: "Kelas 1", "kelas 1", "KELAS I", dan "1" menjadi KELAS I.
2. Kartu yang kosong atau tidak terpetakan dibiarkan `NULL` dan ditandai "perlu dilengkapi". Registrasi berikutnya menampilkan peringatan (lihat K-03).
3. Di QuilvianNewDevSukma, 23 dari 23 kartu kosong, sehingga semuanya masuk kategori "perlu dilengkapi".

**Acceptance criteria:**
- **AC-01:** Form kartu asuransi menampilkan dropdown "Hak Kelas" dari master kelas. Nilai tersimpan sebagai `PatientClassId`.
- **AC-02:** Menyimpan kartu tanpa hak kelas ditolak dengan pesan yang jelas (bila K-02 = wajib).
- **AC-03:** Registrasi kunjungan menyalin hak kelas ke `EntitledPatientClassIdSnapshot`. Mengubah kartu sesudahnya tidak mengubah snapshot kunjungan yang sudah berjalan.
- **AC-04:** `dotnet ef migrations has-pending-model-changes` bersih dan migration bersifat additive.

---

### FIX-INS-002-03 — Perhitungan naik kelas

**Berkas baru (backend):**
- `Areas/HealthServices/ClinicalManagement/Services/InsuranceClassEntitlementService.cs`

**Berkas yang diubah (backend):**
- `MstInsuranceProvider` + konfigurasi + DTO + `InsuranceProviderController`:
  - tambah `ClassUpgradePolicy` (enum `ItemTariffDifference`, `RoomDifferenceOnly`, `Prorata`, `NotCovered`);
  - tambah `ClassUpgradeMaxCoveragePercent` (opsional, untuk batas khusus BPJS, lihat K-06).
- [`EncounterInsuranceService.cs`](../../../../Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs): `EncounterInsuranceContext` membawa `EntitledPatientClassId`, `EntitledPatientClassName`, dan `ClassUpgradePolicy`. Sumbernya snapshot sumber pembayaran, dengan fallback ke kartu.
- [`InsuranceCoverageService.cs`](../../../../Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs) (`ResolveTariffInternalAsync`, baris ±141): setelah perhitungan normal, panggil layanan entitlement. Tambah field hasil `IsClassUpgrade`, `EntitledPatientClassName`, dan `ClassUpgradeAmount` pada `InsuranceCoverageResult`.
- [`BillingCoverageAdapter.cs`](../../../../Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs) (`RegistrationBillingCoverageAdapter`): terapkan layanan yang sama per komponen, supaya billing dan order menghasilkan angka identik.
- [`InpBedOccupancyService.cs`](../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs): saat place/transfer, bandingkan kelas kamar dengan hak kelas. Bila hasilnya naik kelas, wajib ada data persetujuan. Simpan di `InpBedPlacement` dengan kolom baru `IsClassUpgrade`, `ClassUpgradeConsentType`, `ClassUpgradeConsentByName`, `ClassUpgradeConsentAt`.
- Daftar bed (`ComparisonResult`, baris ±313) menambah perbandingan terhadap hak kelas, bukan hanya terhadap bed saat ini.

**Algoritma `InsuranceClassEntitlementService`:**
1. Lewati perhitungan bila:
   - penjamin tidak memakai hak kelas;
   - hak kelas kosong (tambahkan peringatan "Hak kelas polis belum diisi");
   - kelas pelayanan bukan kelas rawat berjenjang (`PatientClassType` ∉ {Class3, Class2, Class1, VIP, VVIP, Executive, Suite, Luxury}). ICU, NICU, Isolasi, Perinatologi, VK, Bayi, ODC, dan RAWAT JALAN ditempatkan atas indikasi medis, bukan permintaan pasien (lihat K-07).
2. Bandingkan dengan `InpBedClassComparisonService.CompareAsync(hakKelas, kelasPelayanan)`:
   - `UpGrade`: lanjut ke langkah 3;
   - `SameGrade` atau `DownGrade`: tidak ada penyesuaian;
   - `Unknown`: tidak ada penyesuaian, tambahkan peringatan "Urutan kelas belum disahkan".
3. Cari **tarif setara di kelas hak**, dengan urutan:
   1. tarif dengan `ProcedureId`/`DrugId` sama dan `PatientClassId` = kelas hak;
   2. tarif dengan `ExternalServiceCode` sama dan `PatientClassId` = kelas hak (di Sukma, `ExternalServiceCode` + kelas unik untuk 351.750 tarif);
   3. untuk item kamar (`IsRoomCharge`): tarif kamar kelas hak (`Room Charge <kelas>`, kode `KAMAR_<ExternalClassCode>`).
   Lalu ambil tarif kontraknya di `MstInsuranceTariff` untuk penjamin yang sama.
4. Terapkan pola penjamin:
   - `ItemTariffDifference`: `covered = min(covered_normal, hitung_ulang(tarif_kontrak_kelas_hak))`;
   - `RoomDifferenceOnly`: seperti di atas, hanya untuk `IsRoomCharge`;
   - `Prorata`: `covered = covered_normal × (tarif_kamar_kontrak_kelas_hak / tarif_kamar_kontrak_kelas_pelayanan)`;
   - `NotCovered`: `covered = 0`.
5. Bila tarif setara tidak ditemukan, jangan menebak. Tandai `IsNeedGuarantorApproval = true` dengan catatan "Tarif kelas hak tidak ditemukan; perlu verifikasi penjamin".
6. Status akhir: `PartiallyCovered` bila ada selisih. Catatan: "Naik kelas dari {hak} ke {pelayanan}; selisih Rp{x} ditanggung pasien."

**Snapshot transaksi:** hasil disimpan lewat field yang sudah ada di order resep/tindakan (`CoverageStatus`, `CoveragePercent`, `CoveredAmount`, `PatientPayAmount`, `CoverageNote`). Rencana ini **tidak menambah kolom pada entity `Trx*`**. Bila kemudian dibutuhkan, entity tersebut di-rename ke prefix registry dalam task yang sama, mengikuti aturan larangan prefix `Trx`.

**Berkas yang diubah (frontend):**
- Master penjamin: field "Pola naik kelas" dan "Batas tanggungan naik kelas (%)".
- Modal penempatan dan pindah bed rawat inap: label "Naik kelas", estimasi selisih kamar, isian persetujuan (skema 3.3).
- Badge coverage resep/tindakan: tampilkan catatan naik kelas dari `coverageNote`.

**Migration EF:** `AddInsuranceClassUpgradePolicy` (kolom `MstInsuranceProvider`, `InpBedPlacement`). Bersifat additive.

**Acceptance criteria:**
- **AC-01 (Siti):** Pasien AdMedika hak KELAS I di bed VIP, pola `ItemTariffDifference`: Room Charge VIP per malam ditanggung Rp1.320.000, pasien Rp990.000, status `PartiallyCovered`, catatan naik kelas tampil.
- **AC-02:** Pasien yang sama di bed KELAS I: tidak ada penyesuaian.
- **AC-03:** Pasien di ICU: tidak dihitung naik kelas walaupun level ICU lebih tinggi dari hak kelas.
- **AC-04:** Pola `Prorata` dengan tagihan uji Rp20.000.000 menghasilkan tanggungan penjamin Rp11.428.571.
- **AC-05:** Angka coverage di order resep/tindakan sama dengan angka di billing untuk item yang sama.
- **AC-06:** Penempatan bed naik kelas tanpa persetujuan ditolak (400) dengan pesan yang menjelaskan isian yang kurang.
- **AC-07:** Hak kelas kosong: coverage dihitung seperti sekarang, ditambah peringatan "Hak kelas polis belum diisi".

---

### FIX-INS-002-04 — Penonaktifan aturan kamar per penjamin

**Perubahan data (skrip dengan backup, dijalankan sesudah FIX-03 aktif):**
- Nonaktifkan (`IsActive = false`, tidak dihapus) aturan `MstInsuranceCoverageRule` bertipe `ServiceCategory` dengan `PatientClassId` terisi pada kategori Room Charge yang dibuat 16 Okt 2026:

| Penjamin | Kelas | Persentase lama |
| --- | --- | --- |
| BPJS Kesehatan | VIP, VIP DELUXE | 75% |
| AdMedika | SUITE, LUXURY, VVIP | 80% |
| Mandiri Inhealth | SUITE, LUXURY, VVIP | 75% |
| Prudential Indonesia | SUITE, LUXURY, VVIP | 70% |
| Allianz Indonesia | VVIP | 80% |

- Isi `ClassUpgradePolicy` setiap penjamin sesuai keputusan K-04.

**Acceptance criteria:**
- **AC-01 (Andi):** Pasien AdMedika hak VIP di bed Suite dihitung dari selisih VIP → Suite, bukan flat 80%.
- **AC-02 (Rina):** Pasien AdMedika hak KELAS II di bed VIP dihitung naik kelas, bukan ditanggung 100%.

---

### FIX-INS-002-05 — Status coverage di picker tindakan

**Berkas yang diubah (backend):**
- [`PatientProcedureController.cs`](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs) `GetMasterProcedureOptions` (baris ±117–260):
  - tambah `[FromQuery] Guid? encounterId`;
  - bila terisi: ambil konteks penjamin sekali (`EncounterInsuranceService.GetContextAsync`), lalu untuk setiap opsi panggil `ResolveProcedureAsync(..., explicitContext: context)`;
  - isi `CoverageStatus`, `CoveragePercent`, `CoverageNote`, `CoveredAmount`, dan `PatientPayAmount` pada `PatientProcedureMasterOptionResponse`.
- Tanpa `encounterId`, perilaku tetap seperti sekarang ("Tarif RS" / "Belum Ditetapkan") supaya pemanggil lain tidak terdampak.

**Berkas yang diubah (frontend):**
- `src/lib/services/health-services/clinical-management/patient-procedure.service.js`: kirim `encounterId`.
- `src/components/view/health-services/registration-management/doctor-queues/tabs/procedure/doctor-procedure-tab.jsx`, `src/components/ui/doctor-clinical-base/DoctorProcedureStatusBadge.jsx`, `doctor-procedure-presentation-utils.js`: tampilkan badge dan catatan coverage dengan pola yang sama seperti picker obat.

**Acceptance criteria:**
- **AC-01:** Pasien AdMedika: "GS3 BLEACHING IN OFFICE" tampil "Tidak ditanggung" beserta alasannya sebelum dipilih.
- **AC-02:** Pasien tunai: badge menampilkan "Tarif RS" dengan harga.
- **AC-03:** Waktu respons `master-options` dengan `take = 50` dan `encounterId` terisi di bawah 1 detik di lingkungan dev. Bila melebihi, beralih ke Opsi B (bagian 2.4).

---

### FIX-INS-002-06 — Backfill data kunjungan lama

**Perubahan data (skrip dengan backup, atas izin pemilik data):**

| Data | Jumlah di Sukma | Tindakan yang diusulkan |
| --- | :-: | --- |
| Kunjungan IGD dengan `PatientClassId` NULL | 4 | Isi kelas RAWAT JALAN |
| Kunjungan rawat jalan berkelas KELAS I | 117 | Ubah ke RAWAT JALAN hanya untuk kunjungan yang belum punya invoice final. Kunjungan dengan invoice final dibiarkan (K-08). |
| Kunjungan Mandiri Inhealth `cfb02314…` (encounter `PaymentType` 3, guarantor 1) | 1 | Selaraskan tipe pembayaran sesuai keputusan pemilik data |

**Acceptance criteria:**
- **AC-01:** Tidak ada kunjungan non-rawat-inap aktif dengan `PatientClassId` NULL.
- **AC-02:** Kunjungan Inhealth dapat dihitung coverage-nya tanpa galat "Tipe pembayaran pada encounter dan payment source tidak konsisten".
- **AC-03:** Total invoice final sebelum dan sesudah backfill tidak berubah.

---

### FIX-INS-002-07 — Koreksi ISSUE-INS-001

- Tambahkan bagian "Koreksi" di ISSUE-INS-001 yang merujuk ke ISSUE-INS-002 bagian ISS-INS-002-07. Riwayat dokumen asli tidak diubah.

---

## 5. Urutan pengerjaan

```text
              [K-01]                               (tanpa gerbang)
                │                                        │
                ▼                                        ▼
       [FIX-01] Kelas default IGD/MCU/Tele      [FIX-05] Coverage di picker tindakan
                │                               [FIX-07] Koreksi ISSUE-INS-001
                ▼
       [FIX-06] Backfill kunjungan lama (K-08)

  [K-02, K-03, K-07]
          │
          ▼
  [FIX-02] Hak kelas berelasi + snapshot + migrasi kartu
          │
          ▼        [K-04, K-05, K-06]
  [FIX-03] InsuranceClassEntitlementService + billing + persetujuan bed
          │
          ▼
  [FIX-04] Nonaktifkan aturan kamar per penjamin, isi pola naik kelas
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan |
| :-: | --- | --- | --- | --- |
| K-01 | Kelas default kunjungan IGD, MCU, Telemedicine | A. RAWAT JALAN. B. Kelas baru per layanan. | **A** | FIX-01 |
| K-02 | Hak kelas pada kartu asuransi | A. Wajib, dari master kelas. B. Opsional. | **A**, untuk semua penjamin yang punya hak kelas (BPJS dan swasta/TPA) | FIX-02 |
| K-03 | Penanganan kartu lama tanpa hak kelas | A. Peringatan saat registrasi, tetap bisa dipakai. B. Blokir registrasi sampai dilengkapi. | **A** pada masa transisi, lalu **B** setelah tanggal yang ditetapkan | FIX-02 |
| K-04 | Pola naik kelas per penjamin | `ItemTariffDifference` / `RoomDifferenceOnly` / `Prorata` / `NotCovered` | Swasta/TPA: `ItemTariffDifference`. BPJS: `ItemTariffDifference` dengan batas K-06. Dikonfirmasi dengan PKS masing-masing. | FIX-03, FIX-04 |
| K-05 | Persetujuan naik kelas saat penempatan bed | A. Wajib dicatat. B. Tidak perlu. | **A** | FIX-03 |
| K-06 | Ketentuan BPJS naik ke VIP | Nilai batas tanggungan sesuai regulasi JKN yang berlaku | Dikonfirmasi oleh bagian casemix/JKN rumah sakit | FIX-03 |
| K-07 | Urutan `ClassLevel` dan daftar kelas yang dievaluasi naik kelas | Lihat catatan di bawah tabel | Sahkan urutan kelas rawat berjenjang. ICU/NICU/Isolasi/Perinatologi/VK/Bayi/ODC dikecualikan dari evaluasi naik kelas. | FIX-02, FIX-03 |
| K-08 | Backfill kunjungan rawat jalan lama berkelas KELAS I | A. Ubah yang belum punya invoice final. B. Biarkan semua. | **A** | FIX-06 |

**Catatan K-07:** `InpBedClassComparisonService` hanya bekerja bila:
- `ClassLevel` > 0;
- digest urutan kelas cocok dengan `BedManagement:ClassOrder:ApprovedSourceDigest` di `appsettings.json` (rujukan `SK-DIREKSI-BED-MGMT-2026-001`).

Hasil verifikasi di Sukma:
- Digest di konfigurasi **tidak cocok** dengan data, baik sebelum maupun sesudah pengisian `ClassLevel` tanggal 16 Okt 2026. Pembanding kelas saat ini selalu menghasilkan `Unknown`.
- Nilai `ClassLevel` yang diisi tanggal 16 Okt 2026 (RAWAT JALAN 0, ODC 1, KELAS III 3, KELAS II 4, KELAS I 5, VIP 6, VIP DELUXE 7, VVIP 8, SUITE 9, LUXURY 10) **masih berupa usulan**.

Setelah urutan disahkan, digest baru dihitung ulang dan dicatat di konfigurasi.

---

## 7. Risiko dan mitigasi

| Risiko | Dampak | Mitigasi |
| --- | --- | --- |
| Dua jalur coverage (order vs billing) menghasilkan angka berbeda | Tagihan berbeda dari yang dilihat dokter | Satu layanan bersama (`InsuranceClassEntitlementService`). AC-05 FIX-03 membandingkan keduanya. |
| Tarif setara di kelas hak tidak ditemukan | Perhitungan menebak | Tidak menebak. Item ditandai perlu verifikasi penjamin (langkah 5 algoritma). |
| `master-options` lambat setelah menghitung coverage | Picker tindakan terasa berat | Batas `take ≤ 50`, satu konteks bersama, ukur waktu (AC-03 FIX-05). Siapkan Opsi B. |
| Migration modul lain tertinggal (pending) di dev | Galat 500 `42703` pada endpoint yang tidak terkait | Jalankan `dotnet ef migrations list` sebelum uji runtime. Terapkan migration pending atas izin pemilik DB. |
| Server dev 7184 memakai biner lama setelah migration diterapkan | Endpoint gagal saat uji layar | Uji runtime memakai build terbaru di port 7185 |
| Backfill mengubah tagihan yang sudah final | Selisih laporan keuangan | Kunjungan dengan invoice final tidak diubah (AC-03 FIX-06) |

---

## 8. Strategi verifikasi

Mengikuti pola verifikasi Bank Darah. **Tidak membuat folder atau project `Tests/`.** `dotnet test` dicatat `NOT RUN — tidak ada project test`.

Per perbaikan:
1. `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>`
2. `dotnet ef migrations has-pending-model-changes --no-build` (FIX-02, FIX-03)
3. `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict`
4. Uji runtime HTTP ke build dari scratchpad terhadap `QuilvianNewDevSukma`, dicatat per skenario:

| Skenario | Perbaikan | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- |
| R1 | FIX-01 | Buat kunjungan IGD tunai dan AdMedika tanpa kelas di request, lalu buka katalog obat | Kelas RAWAT JALAN, harga tampil, status coverage tampil |
| R2 | FIX-02 | Simpan kartu asuransi tanpa hak kelas, lalu dengan hak KELAS I | Pertama ditolak (bila K-02 = wajib), kedua tersimpan dengan `PatientClassId` |
| R3 | FIX-02 | Registrasi kunjungan, lalu ubah hak kelas kartu | Snapshot kunjungan tidak berubah |
| R4 | FIX-03 | Kasus Siti: hak KELAS I, tempatkan di VIP tanpa persetujuan, lalu dengan persetujuan | Pertama 400, kedua tersimpan `IsClassUpgrade = true` |
| R5 | FIX-03 | Order Room Charge dan Visite untuk Siti | Ditanggung sampai tarif kontrak KELAS I, status `PartiallyCovered` |
| R6 | FIX-03 | Hitung billing kunjungan Siti | Angka sama dengan R5 |
| R7 | FIX-03 | Pasien hak KELAS I di ICU | Tidak ada penyesuaian naik kelas |
| R8 | FIX-04 | Kasus Andi (hak VIP di Suite) dan Rina (hak KELAS II di VIP) | Selisih dihitung dari hak kelas masing-masing |
| R9 | FIX-05 | Buka picker tindakan pasien AdMedika, cari "bleaching" | Badge "Tidak ditanggung" dengan alasan, sebelum disimpan |
| R10 | FIX-06 | Jalankan backfill | Tidak ada kunjungan non-rawat-inap tanpa kelas. Total invoice final tidak berubah. |

Keadaan DB sesudah setiap run dicatat. Data uji memakai pasien samaran.

---

## 9. Riwayat dokumen

| Tanggal | Versi | Catatan | Oleh |
| :-: | :-: | --- | --- |
| 2026-10-16 | 1.0.0 | Rencana perbaikan diterbitkan untuk ISSUE-INS-002. Menunggu keputusan K-01 sampai K-08. | sukmagp (dibantu Claude Code) |
