# ISSUE-INS-002 — Kelas Pelayanan Kunjungan dan Hak Kelas Polis Asuransi Tidak Terkelola, Menyebabkan Tarif dan Status Coverage Keliru

```yaml
issue_id: ISSUE-INS-002
module_id: cross-module
submodule: insurance-patient-coverage
layar:
  - "Registrasi Kunjungan IGD"
  - "Master Data Asuransi Pasien"
  - "Penempatan Bed Rawat Inap"
  - "Resep & Tindakan Dokter (Rawat Jalan, IGD, Rawat Inap)"
  - "Billing / Kasir"
tanggal_issue: "2026-10-16"
status: TERBUKA
keparahan_tertinggi: Critical
basis_data_verifikasi: "QuilvianNewDevSukma"
source_sha_backend: "5a6de202 (sukmagp)"
menggantikan_sebagian: "ISSUE-INS-001 (mekanisme dikoreksi, lihat ISS-INS-002-07)"
rencana_perbaikan: "../planning/plan-repair-ins-002-kelas-pelayanan-dan-hak-kelas-asuransi.md"
```

---

## 1. Ringkasan Eksekutif

Status coverage asuransi di Quilvian dihitung oleh [`InsuranceCoverageService`](../../../../Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs) dalam tiga langkah:

1. Mencari **tarif rumah sakit** (`MstTariff`) yang sesuai dengan **kelas pelayanan kunjungan** (`RegPatientEncounter.PatientClassId`, atau kelas bed untuk rawat inap).
2. Mencari tarif tersebut di **buku tarif asuransi** (`MstInsuranceTariff`). Item yang tidak tercantum dianggap tidak ditanggung (*positive list*).
3. Menerapkan **aturan coverage** (`MstInsuranceCoverageRule`) yang paling spesifik untuk menentukan persentase tanggungan.

Kelas pelayanan menentukan tarif mana yang dipakai. Masalahnya ada di dua sisi:

- **Kelas pelayanan bisa kosong.** Kunjungan IGD tidak pernah diberi kelas. Akibatnya obat dan hampir semua tindakan IGD tidak punya harga sama sekali, baik untuk pasien tunai maupun asuransi.
- **Hak kelas polis tidak pernah dibandingkan dengan kelas pelayanan.** Hak kelas di kartu asuransi hanya disimpan sebagai teks bebas. Jika pasien dirawat di atas hak kelasnya (naik kelas), sistem tetap menyatakan biaya ditanggung penuh. Selisihnya baru ketahuan di kasir, atau saat klaim ditolak penjamin.

Dokumen ini menguraikan tujuh issue dari dua masalah tersebut, diurutkan sesuai prioritas perbaikan.

---

## 2. Daftar Issue dan Urutan Prioritas

| Urutan | ID | Judul | Jenis | Keparahan | Bergantung pada |
| :-: | :--- | :--- | :--- | :-: | :--- |
| 1 | `ISS-INS-002-01` | Kunjungan IGD tidak diberi kelas pelayanan, sehingga obat/tindakan tidak punya harga | `BUG` | **Critical** | — |
| 2 | `ISS-INS-002-02` | Hak kelas polis disimpan sebagai teks bebas tanpa relasi ke master kelas | `DESIGN_CHANGE` | **High** | — |
| 3 | `ISS-INS-002-03` | Tidak ada validasi naik kelas (kelas pelayanan vs hak kelas polis) | `LOGIC` | **High** | 02 + keputusan bisnis |
| 4 | `ISS-INS-002-04` | Aturan coverage kelas di master berlaku sama untuk semua peserta penjamin | `DATA` / `DESIGN` | Medium | 03 |
| 5 | `ISS-INS-002-05` | Picker tindakan belum menampilkan status coverage sebelum tindakan disimpan | `UX` / `LOGIC` | Medium | — |
| 6 | `ISS-INS-002-06` | Data kunjungan lama tidak konsisten (kelas dan tipe pembayaran) | `DATA` | Low | 01 |
| 7 | `ISS-INS-002-07` | Dokumen ISSUE-INS-001 salah menjelaskan mekanisme kegagalan | `DOC` | Low | — |

Alur pengerjaan:

- **Dapat langsung dikerjakan:** 01, kemudian 06.
- **Menunggu keputusan bisnis:** 02, kemudian 03, kemudian 04.
- **Dapat dikerjakan paralel:** 05 dan 07.

---

## 3. Rincian Issue

### ISS-INS-002-01 — Kunjungan IGD Tidak Diberi Kelas Pelayanan

| Atribut | Keterangan |
| :--- | :--- |
| Jenis | `BUG` |
| Area | Backend (registrasi kunjungan) |
| Keparahan | **Critical** |
| Status bukti | Terverifikasi di source, basis data, dan runtime HTTP |

#### A. Apa yang terjadi

Setiap kunjungan IGD tersimpan tanpa kelas pelayanan (`PatientClassId = NULL`). Saat dokter IGD memilih obat, sistem menampilkan:

> *"Cakupan belum dapat ditentukan karena tarif belum tersedia."* (status `ConfigurationMissing`)

Masalah ini muncul untuk **pasien tunai maupun asuransi**, karena kegagalannya terjadi saat mencari tarif rumah sakit, sebelum logika asuransi berjalan.

#### B. Akibat bisnis — contoh

> **Kasus Budi (IGD, pasien AdMedika, pukul 02.00).**
> Budi datang dengan sesak napas. Dokter IGD memberikan nebulisasi, memasang infus, dan meresepkan PANADOL 500 MG CAPLET.
>
> - **Nebulisasi Dewasa (Rp150.000)** dan **Pemasangan Infus IV Line Dewasa (Rp75.000)** tetap memiliki harga. Keduanya termasuk 35 tarif tindakan dasar yang tidak terikat kelas.
> - **PANADOL 500 MG CAPLET** tidak memiliki harga. Sistem menampilkan pesan "Lengkapi tarif sebelum menyimpan resep". Hal yang sama terjadi pada semua obat, alkes, dan sebagian besar tindakan IGD lainnya, karena 326.231 dari 326.266 tarif obat/tindakan terikat kelas.
>
> Dampak:
> - Pelayanan gawat darurat tertunda karena resep dan tindakan tidak bisa dicatat dengan harga.
> - Petugas terpaksa mencatat biaya secara manual di luar sistem, sehingga rawan kebocoran pendapatan.
> - Bagi pasien asuransi, tagihan IGD tidak bisa diklaim karena item tidak tercatat dengan tarif kontrak.

#### C. Kenapa terjadi

1. [`PatientEncounterController.ResolvePatientClassAsync`](../../../../Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs#L2140) hanya menetapkan kelas otomatis untuk `EncounterType.Outpatient` (kelas "RAWAT JALAN"). Untuk `EncounterType.Emergency`, kelas diambil dari request. Bila request kosong, kunjungan tersimpan tanpa kelas.
2. [`InsuranceCoverageService.FindDrugTariffAsync`](../../../../Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs#L477) dan `FindProcedureTariffAsync` hanya menerima tarif yang kelasnya sama dengan kelas kunjungan, atau tarif tanpa kelas. Hampir semua tarif obat dan tindakan terikat kelas, sehingga tidak ada kandidat yang cocok.

#### D. Bukti

| Bukti | Nilai |
| :--- | :--- |
| Kunjungan IGD (`EncounterType = 2`) di `RegPatientEncounter` | 4 kunjungan, 4 dari 4 `PatientClassId = NULL` |
| Tarif obat/tindakan di `MstTariff` yang terikat kelas | 326.231 dari 326.266 (99,99%) |
| Runtime `GET /prescribing-drugs?encounterId=18a2b7b7…` (IGD, tunai) | `PANADOL 500 MG CAPLET → ConfigurationMissing` |

#### E. Keputusan yang dibutuhkan

| No | Pertanyaan | Rekomendasi |
| :-: | :--- | :--- |
| K-01 | Kelas pelayanan default untuk kunjungan IGD | Gunakan kelas **RAWAT JALAN**, mengikuti pola rawat jalan. Saat pasien dirawat inap, kelas berpindah ke kelas bed. |

---

### ISS-INS-002-02 — Hak Kelas Polis Disimpan sebagai Teks Bebas

| Atribut | Keterangan |
| :--- | :--- |
| Jenis | `DESIGN_CHANGE` |
| Area | Backend (model, DTO) + Frontend (form kartu asuransi) |
| Keparahan | **High** |
| Status bukti | Terverifikasi di source dan basis data |

#### A. Apa yang terjadi

Setiap polis asuransi menjamin pasien sampai kelas tertentu. Contohnya peserta BPJS kelas 2, atau peserta AdMedika dengan plan kelas I. Di Quilvian, hak kelas ini hanya berupa isian teks opsional **"Kelas"** di kartu asuransi pasien. Tidak ada relasi ke master kelas (`MstPatientClass`), dan isian ini tidak wajib.

#### B. Akibat bisnis — contoh

> **Kasus Siti (peserta AdMedika, plan kelas I).**
> Saat kartu didaftarkan, petugas membiarkan kolom "Kelas" kosong. Petugas lain di kartu lain mengetik "Kelas 1", "kelas 1", atau "KELAS I".
>
> Dampak:
> - Petugas admisi tidak bisa melihat hak kelas Siti secara pasti saat memilih kamar. Hak kelas hanya tampil sebagai teks (`ClassEntitlementText`) pada permintaan rawat inap dari IGD, tanpa validasi.
> - Sistem tidak punya data untuk membandingkan hak kelas dengan kamar yang dipakai, sehingga ISS-INS-002-03 tidak bisa diperbaiki.
> - Laporan "pasien per hak kelas" tidak bisa dibuat karena isinya tidak seragam.

#### C. Kenapa terjadi

1. [`MstPatientInsurance.cs:33`](../../../../Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientInsurance.cs#L33) hanya memiliki `public string? ClassName`, tanpa `PatientClassId` dan navigasi ke `MstPatientClass`.
2. Nilai teks ini hanya disalin ke `ClassNameSnapshot` saat registrasi ([`EncounterPaymentSourceService.cs:347`](../../../../Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs#L347)), lalu dicetak di invoice ([`BillingInsuranceInvoiceDocumentService.cs:212`](../../../../Areas/HealthServices/BillingManagement/Billing/Services/BillingInsuranceInvoiceDocumentService.cs#L212)). Nilai ini tidak pernah dipakai dalam perhitungan.

#### D. Bukti

| Bukti | Nilai |
| :--- | :--- |
| Kartu asuransi aktif (`MstPatientInsurance`, tidak terhapus) | 23 kartu |
| Kartu dengan `ClassName` terisi | 0 kartu (0%) |

#### E. Keputusan yang dibutuhkan

| No | Pertanyaan | Rekomendasi |
| :-: | :--- | :--- |
| K-02 | Apakah hak kelas wajib diisi pada kartu asuransi? | Wajib, dipilih dari master kelas (dropdown), bukan teks bebas. |
| K-03 | Bagaimana menangani kartu lama yang hak kelasnya kosong atau berupa teks? | Lakukan migrasi pemetaan teks ke master kelas. Kartu yang tidak terpetakan ditandai "perlu dilengkapi" saat registrasi berikutnya. |

---

### ISS-INS-002-03 — Tidak Ada Validasi Naik Kelas

| Atribut | Keterangan |
| :--- | :--- |
| Jenis | `LOGIC` |
| Area | Backend (`InsuranceCoverageService`, penempatan bed rawat inap) |
| Keparahan | **High** |
| Status bukti | Terverifikasi di source |

#### A. Apa yang terjadi

Kelas pelayanan menentukan **tarif** yang dipakai. Hak kelas polis seharusnya menentukan **batas tanggungan**. Saat ini sistem hanya memakai kelas pelayanan. Jika pasien dirawat di kelas yang lebih tinggi dari hak kelasnya, sistem tetap menghitung biaya sebagai ditanggung penuh.

Masalah ini terjadi di **rawat inap**, karena kelasnya diambil dari bed yang ditempati. Rawat jalan tidak terdampak karena backend selalu memaksa kelas RAWAT JALAN.

#### B. Akibat bisnis — contoh

> **Kasus Siti (peserta AdMedika, hak kelas I) dirawat di kamar VIP selama 3 malam.**
>
> | Komponen | Tarif kontrak AdMedika per malam |
> | :--- | ---: |
> | Room Charge KELAS I | Rp1.320.000 |
> | Room Charge VIP | Rp2.310.000 |
>
> **Yang dihitung sistem saat ini:** kamar VIP 3 × Rp2.310.000 = **Rp6.930.000, ditanggung penuh (Covered 100%)**. Kasir dan perawat tidak mendapat peringatan apa pun.
>
> **Yang terjadi di dunia nyata**, tergantung aturan penjamin:
>
> | Pola penjamin | Perhitungan | Dibayar pasien |
> | :--- | :--- | ---: |
> | Selisih kamar saja | 3 × (Rp2.310.000 − Rp1.320.000) | **Rp2.970.000** |
> | Prorata seluruh tagihan | Rasio 1.320.000 / 2.310.000 = 57,14%. Untuk total tagihan Rp20.000.000, penjamin membayar Rp11.428.571. | **Rp8.571.429** |
>
> Dampak:
> - Pasien dan keluarga baru mengetahui ada biaya tambahan saat pulang di kasir, sehingga terjadi komplain dan sengketa.
> - Jika kasir tidak menagih selisih, rumah sakit mengajukan klaim penuh. Penjamin lalu memotong atau menolak sebagian klaim, dan rumah sakit menanggung kerugian atau harus menagih pasien yang sudah pulang.
> - Tidak ada jejak bahwa pasien atau keluarga menyetujui naik kelas.

> **Kasus BPJS: peserta kelas 2 meminta dirawat di VIP.**
> Ketentuan JKN mengatur bahwa naik kelas atas permintaan peserta menjadikan selisih biaya tanggungan peserta, dengan ketentuan khusus untuk naik ke VIP. Sistem saat ini tidak memiliki cara untuk menandai atau menghitung kondisi tersebut.

#### C. Kenapa terjadi

1. [`EncounterInsuranceService`](../../../../Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs) hanya membawa kelas pelayanan (`PatientClassId`) ke dalam konteks coverage. Hak kelas polis tidak ikut dibawa.
2. [`InsuranceCoverageService.ResolveTariffInternalAsync`](../../../../Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs#L141) tidak memiliki langkah perbandingan "kelas pelayanan vs hak kelas".
3. Tidak ada logika naik kelas di backend. Pencarian `UpgradeClass`, `NaikKelas`, dan `IsClassUpgrade` tidak menemukan apa pun.

#### D. Keputusan yang dibutuhkan

| No | Pertanyaan | Pilihan |
| :-: | :--- | :--- |
| K-04 | Pola perhitungan naik kelas per penjamin | **A.** Selisih kamar saja. **B.** Prorata seluruh tagihan. **C.** Tidak ditanggung sama sekali. Disimpan sebagai pengaturan per penjamin. |
| K-05 | Apakah naik kelas wajib disertai persetujuan tertulis pasien/keluarga saat penempatan bed? | Rekomendasi: wajib, dicatat di sistem saat penempatan bed. |
| K-06 | Ketentuan khusus BPJS untuk naik ke kelas VIP | Perlu konfirmasi kebijakan rumah sakit dan regulasi JKN yang berlaku. |

---

### ISS-INS-002-04 — Aturan Coverage Kelas Berlaku Sama untuk Semua Peserta Penjamin

| Atribut | Keterangan |
| :--- | :--- |
| Jenis | `DATA` / `DESIGN` |
| Area | Master data `MstInsuranceCoverageRule` |
| Keparahan | Medium |
| Status bukti | Terverifikasi di basis data |

#### A. Apa yang terjadi

Untuk keperluan pengujian, aturan coverage kamar per kelas sudah diisi per penjamin. Contohnya: AdMedika 80% untuk Suite/Luxury/VVIP, Mandiri Inhealth 75%, Prudential 70%, Allianz 80% untuk VVIP, dan BPJS 75% untuk VIP/VIP Deluxe. Aturan ini berlaku untuk **semua peserta** penjamin tersebut, tanpa melihat hak kelas masing-masing.

#### B. Akibat bisnis — contoh

> - **Andi**, peserta AdMedika plan eksekutif dengan hak kelas VIP, dirawat di Suite. Sistem menghitung kamar ditanggung 80%. Seharusnya batasnya mengikuti hak kelas VIP.
> - **Rina**, peserta AdMedika plan kelas II, dirawat di VIP. Sistem menghitung kamar ditanggung 100%, karena aturan 80% hanya untuk Suite/Luxury/VVIP. Seharusnya Rina menanggung selisih dari kelas II ke VIP.
>
> Kedua hasil tersebut keliru karena patokannya penjamin, bukan hak kelas pasien.

#### C. Tindak lanjut

Setelah ISS-INS-002-03 selesai, aturan kelas per penjamin ini diganti atau dikurangi supaya batas tanggungan mengikuti hak kelas per pasien.

---

### ISS-INS-002-05 — Picker Tindakan Belum Menampilkan Status Coverage

| Atribut | Keterangan |
| :--- | :--- |
| Jenis | `UX` / `LOGIC` |
| Area | Backend (`PatientProcedureController`) + Frontend (tab tindakan dokter) |
| Keparahan | Medium |
| Status bukti | Terverifikasi di source |

#### A. Apa yang terjadi

Saat dokter memilih **obat**, status coverage langsung tampil (ditanggung, sebagian, perlu approval, tidak ditanggung). Saat memilih **tindakan**, daftar pilihan hanya menampilkan "Tarif RS" atau "Belum Ditetapkan". Status coverage tindakan baru dihitung setelah tindakan disimpan.

#### B. Akibat bisnis — contoh

> **Pasien AdMedika di poli gigi.** Dokter memilih tindakan **GS3 BLEACHING IN OFFICE**. Daftar pilihan hanya menampilkan "Tarif RS", tanpa keterangan bahwa AdMedika tidak menanggung perawatan gigi estetik. Status "Tidak ditanggung" baru muncul setelah tindakan disimpan. Pada saat itu dokter mungkin sudah menjelaskan rencana tindakan kepada pasien, atau tindakan sudah dikerjakan.
>
> Dampak:
> - Pasien tidak mendapat informasi biaya sebelum menyetujui tindakan, sehingga terjadi komplain di kasir.
> - Pengalaman dokter tidak konsisten antara memilih obat dan memilih tindakan.

#### C. Kenapa terjadi

Endpoint `GET patient-procedures/master-options` ([`PatientProcedureController.cs:254`](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs#L254)) hanya mengisi `CoverageStatus` dengan "Tarif RS" atau "Belum Ditetapkan", dan tidak menerima `encounterId`. `InsuranceCoverageService.ResolveProcedureAsync` baru dipanggil saat tindakan dibuat.

---

### ISS-INS-002-06 — Data Kunjungan Lama Tidak Konsisten

| Atribut | Keterangan |
| :--- | :--- |
| Jenis | `DATA` |
| Area | Basis data `RegPatientEncounter`, `RegPatientEncounterGuarantor` |
| Keparahan | Low |
| Status bukti | Terverifikasi di basis data |

#### A. Apa yang terjadi

| Temuan | Jumlah | Keterangan |
| :--- | :-: | :--- |
| Kunjungan rawat jalan berkelas `KELAS I` | 117 (114 tunai, 1 asuransi, 2 penjamin perusahaan) | Data lama. Kode saat ini selalu memaksa kelas RAWAT JALAN untuk kunjungan rawat jalan baru. |
| Kunjungan IGD tanpa kelas | 4 | Akibat ISS-INS-002-01 |
| Kunjungan dengan tipe pembayaran berbeda dengan sumber pembayarannya | 1 (pasien Mandiri Inhealth: encounter = 3, guarantor = 1) | Coverage gagal dengan pesan "Tipe pembayaran pada encounter dan payment source tidak konsisten." |

#### B. Akibat bisnis — contoh

> - Kunjungan poli lama yang berkelas KELAS I memakai tarif dan aturan kelas I. Jika suatu saat tarif kontrak kelas I berbeda dari rawat jalan, penagihan dan klaim kunjungan lama akan berbeda dari kunjungan baru untuk layanan yang sama.
> - Kunjungan Mandiri Inhealth yang tidak konsisten tidak bisa ditagihkan ke penjamin sama sekali, dan saat ini menjadi satu-satunya kunjungan Inhealth untuk pengujian.

#### C. Tindak lanjut

Lakukan backfill setelah ISS-INS-002-01 diperbaiki: tetapkan kelas IGD, selaraskan kelas rawat jalan lama, dan perbaiki tipe pembayaran yang tidak konsisten. Backfill dikerjakan dengan persetujuan pemilik data.

---

### ISS-INS-002-07 — Koreksi atas Dokumen ISSUE-INS-001

| Atribut | Keterangan |
| :--- | :--- |
| Jenis | `DOC` |
| Keparahan | Low |

ISSUE-INS-001 benar bahwa kelas yang kosong membuat coverage tidak dapat ditentukan, tetapi beberapa penjelasannya keliru:

| Pernyataan di ISSUE-INS-001 | Kondisi sebenarnya |
| :--- | :--- |
| Kartu asuransi tanpa kelas menyebabkan coverage gagal | Coverage tidak membaca kelas di kartu asuransi. Penyebabnya adalah **kunjungan tanpa kelas** (ISS-INS-002-01). Hak kelas di kartu relevan untuk validasi naik kelas (ISS-INS-002-03). |
| Item jatuh ke status `NotCovered` ("Item tidak terdapat pada buku tarif asuransi") | Kegagalan terjadi lebih awal, di tarif rumah sakit, dengan status `ConfigurationMissing`. Pasien tunai juga terdampak. |
| Tidak ada fallback saat kelas kosong | Fallback ke tarif tanpa kelas ada, tetapi hanya tersedia 35 tarif tindakan dasar. |
| 165 kunjungan rawat jalan "berpenjamin asuransi" memakai filter `PaymentType = 1` | Pada enum `EncounterPaymentType`, nilai 1 = **Tunai** (Asuransi = 2), sehingga angka tersebut tidak menggambarkan kunjungan asuransi. |

---

## 4. Catatan Arsitektur

Kelas memiliki dua peran yang berbeda dan keduanya harus dikelola:

| Peran | Sumber data | Fungsi | Kondisi saat ini |
| :--- | :--- | :--- | :--- |
| **Kelas pelayanan** | Kunjungan (`RegPatientEncounter.PatientClassId`) atau bed rawat inap (`InpBedPlacement.PatientClassId`) | Menentukan tarif rumah sakit dan tarif kontrak yang dipakai | Rawat jalan dan rawat inap terisi. **IGD kosong** (ISS-01). |
| **Hak kelas polis** | Kartu asuransi pasien (`MstPatientInsurance`) | Menentukan batas tanggungan penjamin | **Hanya teks bebas** (ISS-02), **tidak pernah dibandingkan** (ISS-03). |

Coverage yang benar membutuhkan keduanya: tarif diambil dari kelas pelayanan, dan batas tanggungan dibatasi oleh hak kelas polis. Selisih di antara keduanya adalah biaya naik kelas yang menjadi tanggungan pasien, sesuai aturan penjamin.

---

## 5. Riwayat Dokumen

| Tanggal | Versi | Catatan | Oleh |
| :-: | :-: | :--- | :--- |
| 2026-10-16 | 1.0.0 | Dokumen issue diterbitkan berdasarkan verifikasi source, basis data `QuilvianNewDevSukma`, dan runtime HTTP. Mengoreksi mekanisme ISSUE-INS-001. | sukmagp (dibantu Claude Code) |
