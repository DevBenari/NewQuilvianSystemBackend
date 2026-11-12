# Optik — Laporan Kesiapan Modul

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `OPT-BP-001`, revisi `3`, status `approved` sisi Optik |
| Tanggal audit | 2026-11-06 |
| Sifat audit | Baca-saja. Tidak ada perubahan source, dan sesi basis data dipaksa `default_transaction_read_only = on` |
| Source backend | `06e40fb702b85295bafc930b706b64db2d14aaa7` (branch `yoga`) + 48 baris status Git belum di-commit, termasuk `BE-OPT-060`..`062` |
| Source frontend | `811f22159ca74e117f4754553086fa58f130b209` (branch `YogaV2`) + 19 berkas belum di-commit, termasuk `FE-OPT-040`..`042` |
| Lingkungan yang diperiksa | Basis data devYoga (`QuilvianNewDevYoga`, PostgreSQL 15.15). Server dev bersama dan produksi **tidak** diperiksa |
| Kontrak | `OPT-API-v1.3`, `OPT-STATE-v1.1`, `OPT-VAL-v1.2`, `OPT-INT-v1.2`, `OPT-PERM-v1.2`, `OPT-PRD-v1.2` |
| Keputusan acuan | `00-interview-decisions.md` r11; `OPT-DEC-001`..`084` |
| Hash masukan | 19 dari 19 artefak manifest cocok |
| Menggantikan | Audit 2026-11-05 pada `fedd5e98` (verdict `NOT_READY`) |
| Sesudah audit | 2026-11-06: blueprint r4 mencantumkan izin lintas modul (`OPT-PERM-v1.3`, `OPT-DEC-085`) dan menerapkan penyegaran bagian 14. Verdict dan angka laporan ini tetap keadaan saat audit, termasuk versi kontrak dan 16 butir Definition of Done. `OPT-OQ-024` dijawab: cukup Yoga Aji Pratama dan admin akses, pemilik modul diberi tahu (`OPT-DEC-086`, blueprint r5); izinnya belum diisi di data. `BE-OPT-063` menutup `T2` di harness: 9 kasus baru, total `OpticalTests` 293/293, tanpa selisih kontrak |

## 1. Kesimpulan

**Verdict: `NOT_READY`**

**Risiko desain dan kode dari audit lalu sudah ditutup, tetapi modul masih belum bisa melayani satu pasien pun dari awal sampai akhir.** Yang tersisa terutama data, konfigurasi, izin, penggabungan kode, dan satu putaran uji dengan backend asli.

| Pertanyaan | Jawaban | Bukti |
| --- | --- | --- |
| Apakah semua task selesai? | Ya: backend 26/26, frontend 17/17 | Laporan per task di `task/report/` |
| Apakah kodenya lulus uji? | Ya: backend 284/284, E2E Optik 152/152 | Backend dijalankan ulang di audit ini. E2E berasal dari `FE-OPT-041`, sesudah suntingan terakhir frontend |
| Apakah RO bisa memeriksa pasien sekarang? | **Tidak** | Kode profesi kosong; profesi RO, spesialisasi mata, dan izin praktik 0 |
| Apakah petugas bisa membuat pesanan sekarang? | **Tidak** | Barang Optik, kategori, profil, depo, dan tarif barang 0 |
| Apakah petugas non-superadmin bisa membuka layar Optik? | **Tidak** | Kebijakan izin Optik 0 pada 7 controller |
| Apakah alur lengkap pernah berjalan di PostgreSQL? | **Tidak** | `OptRefractionExam`, `OptPrescription`, `OptOrder`, dan `OptWarrantyClaim` berisi 0 baris |

`READY_WITH_CONDITIONS` tidak dipilih, karena jalur utama tertutup sepenuhnya, bukan terbatas pada risiko pinggir.

### Yang berubah sejak audit lalu

| Butir | Dulu | Sekarang |
| --- | --- | --- |
| `R1` idempoten | Terbuka | Ditutup: `BE-OPT-060`, `FE-OPT-040`; lima `POST` pembuat membaca `Idempotency-Key`; kolomnya ada di devYoga. Kiriman serentak sungguhan belum diuji |
| `R2` tarif tertinggal | Terbuka | Ditutup di kode: `BE-OPT-062`, disetujui `OPT-APR-015` |
| `R3` Penjamin Perusahaan | Selalu ditolak | Ditutup di harness: `BE-OPT-061`, disetujui `OPT-APR-013` |
| `R4` diskon | Terbuka | Keputusan ditutup (`OPT-DEC-079`, `084`) dan layar menyaring. Billing belum diuji (temuan `T2`) |
| `R6` Daftarkan Barang | Di luar brief | Ditutup: brief r3, `OPT-DEC-080`, `FE-OPT-042` |
| Hash manifest | 17/18 | 19/19 |

## 2. Skor

Skor menilai bukti yang ada, bukan jumlah berkas. Skor gabungan hanya gambaran: **verdict ditentukan blocker, bukan rata-rata.**

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| ---: | ---: | ---: | --- | --- |
| Fondasi | 15% | 90 | Hash 19/19; registry `Opt` `ACTIVE`; riwayat migration 381 termasuk `AddOpticalCreateIdempotencyKey`; 7/7 controller terdaftar di `SysControllerAccess` | Keterangan basi (bagian 8) |
| Backend | 25% | 90 | `OpticalTests` 284/284; 45 endpoint cocok dengan kontrak; 5/5 `POST` pembuat berkunci; pekerja latar terdaftar (`Program.cs#OptExamChargeRetryHostedService`) | `T3`: folder uji tidak ter-track; `T2`: diskon tanpa uji Billing |
| Frontend | 20% | 85 | Task 17/17; E2E 152/152; lint dan build bersih; uji unit: 72 gagal dari 4.481, daftar nama sama dengan run sebelumnya, tanpa nama Optik | `B6`: belum masuk cabang integrasi |
| Integrasi/runtime | 25% | 20 | Data prasyarat 3/12; konfigurasi 0/3; kebijakan izin 0 | `B1`..`B5`, `T1` |
| Verifikasi | 15% | 45 | UAT 2/36 dengan backend asli, 27/36 harness (25 di antaranya dengan layar); matriks penerimaan 59/78 berbukti penuh | `B5` |
| **Gabungan** | 100% | **65** | Audit lalu 63 | **Tidak menentukan verdict** |

### Kemajuan kode dibanding kesiapan nyata

| Ukuran | Kemajuan kode | Kesiapan nyata |
| --- | --- | --- |
| Requirement (34 `FR`) | 33/34 punya kode dan uji | **4/34** terbukti dengan backend asli: `FR-OPT-001`, `002`, `010`, `011` |
| UAT MVP (36, tanpa `UAT-19`) | 34/36 lulus pada lapis layar atau harness | **2/36** dengan backend asli: `UAT-01`, `25` |
| Definition of Done (16 butir) | 12 terpenuhi di harness atau layar | **2/16** penuh; 2 belum |
| Data master (12 jenis) | Semua punya layar atau cara mengisi | **3/12** terisi |

## 3. Yang terjadi bila dicoba hari ini

**Contoh:** Ibu Sari (samaran), karyawan PT Contoh (samaran), datang ke Optik RSMMC membawa rujukan Poli Mata, memeriksakan mata, lalu memesan kacamata.

| Langkah | Pelaku | Hasil hari ini | Sebab |
| --- | --- | --- | --- |
| 1. Daftarkan kunjungan tunai atau asuransi | Admisi | ✅ Berhasil dengan superadmin (`ENC-RSMMC-00186`) | Akun petugas belum diuji (`T1`) |
| 1b. Penjamin Perusahaan | Admisi | ⚠ Kode menerima, belum dicoba di server asli | Belum ada aturan tanggungan; tanggungan akan Rp 0 (`T4`) |
| 1c. Membuka menu Optik | Petugas | ⛔ Menu tidak tampil | `B3` |
| 2. Mulai pemeriksaan | RO | ⛔ `403` `OPT-VAL-001` | `B1` |
| 3. Tulis resep | RO | ⛔ Sebab yang sama | `B1` |
| 4. Tagih pemeriksaan | Sistem | ⛔ Tarif menunggu tindakan diatur; pekerja latar mengirimnya kemudian | `B4` |
| 5. Buat pesanan | Petugas | ⛔ Katalog kosong | `B2` |
| 6. Ajukan diskon 10% | Petugas | ⛔ Pilihan kosong | Kebijakan `OPT-DISC-10` belum ada |
| 7. Kirim, siap, serahkan | Petugas | ⛔ Tidak tercapai; depo juga belum ada | `B2` |
| 8. Keluhan dan remake | Petugas, RO, Kepala Optik | ⛔ Tidak tercapai | `B1`, `B2` |

## 4. Bukti data dan konfigurasi

Hasil kueri baca-saja devYoga, 2026-11-06. Hanya agregat yang diambil.

### 4.1 Data master

| Master | Kebutuhan minimum | Isi devYoga | Status | Pemilik |
| --- | --- | --- | --- | --- |
| `MstServiceUnit` | Satu unit Optik | 1 aktif | ✅ | — |
| `MstDrugStorageLocation` | Depo Optik siap layan | **0** | ⛔ | Farmasi (Ikbal) |
| `MstDrugCategory` | 4 kategori Optik | **0** | ⛔ | Master Data (Muhammad Hamzah) |
| `MstDrug` | Barang Optik, `IsPrescribable = false` | **0** dari 7.862 barang | ⛔ | Master Data, Farmasi |
| `OptItemProfile` | Satu per barang | **0** | ⛔ | Admin Optik, sesudah barang ada |
| `MstProcedure` + `MstTariff` | Tindakan Pemeriksaan Refraksi + tarif | **0** | ⛔ | Master Data, Tarif |
| `MstTariff` | Tarif per barang | **0**, belum ada barang | ⛔ | Tarif |
| `MstSupplier` | Lab lensa luar | 2 aktif | ✅ | — |
| `OptSetting` | Satu per unit | 1, nilai bawaan | ✅ | Yoga; RO/Sp.M untuk BC/DIA |
| `MstDiscountPolicy` | Kebijakan `OPT-DISC-10` | **0**, seluruh kebijakan aktif juga 0 | ⛔ | Billing (Yasmina) |
| `SysAccessPolicy` | Izin 5 peran Optik | **0** pada 7 controller | ⛔ | Admin akses, Yoga |
| `WfpCredentialLicense` | STR + SIP RO/Sp.M | **0** seluruh pegawai | ⛔ | SDM |

**Terisi 3 dari 12.** Tambahan di luar 12 jenis itu:

| Data | Isi | Dampak |
| --- | --- | --- |
| `MstCompanyGuarantorCoverageRule` | **0** aturan | Tanggungan perusahaan Rp 0 (`T4`) |
| `MstCompanyGuarantor` / `MstPatientCompanyGuarantor` | 5 penjamin aktif / 1 kartu | Cukup sebagai awal uji `UAT-30` |
| `MstProfession`, `MstSpecialization` | Profesi RO 0 dari 10; spesialisasi mata 0; dokter mata 0 | Menahan `OPT-OQ-022` |

### 4.2 Konfigurasi

| Kunci | Fungsi | Di `appsettings` | Bila kosong |
| --- | --- | --- | --- |
| `OpticalManagement:Practitioner:RefractionistProfessionCodes` | Kode profesi RO | Tidak ada | Setiap akun ditolak `OPT-VAL-001` |
| `OpticalManagement:Practitioner:OphthalmologistSpecializationCodes` | Kode spesialisasi Sp.M | Tidak ada | Sama |
| `OpticalManagement:Billing:RefractionExamProcedureCode` | Tindakan tarif pemeriksaan | Tidak ada | Final tetap berhasil; tarif menunggu, pekerja latar memberi peringatan per putaran |

**Terisi 0 dari 3.** Pengisian lewat variabel lingkungan di server tidak dapat dilihat dari audit ini.

`OpticalManagement:ExamChargeRetry` tidak wajib diisi. Bawaannya menyala, tiap 5 menit, 50 pemeriksaan per putaran, dengan jeda 5 menit sesudah final.

## 5. Blocker, urut menurut dampak

| ID | Blocker | Dampak | Pemilik | Arah penyelesaian |
| --- | --- | --- | --- | --- |
| **B1** | Gerbang izin praktik tidak bisa dilewati: kode profesi kosong, profesi RO dan spesialisasi mata tidak ada, izin praktik 0 | Pemeriksaan, resep, dan periksa ulang keluhan ditolak | Yoga (kode, `OPT-OQ-022`); SDM; Master Data | Data + konfigurasi; tanpa kode |
| **B2** | Barang Optik belum ada: kategori, barang, profil, depo, tarif 0 | Pesanan dan penjualan langsung tidak bisa dibuat; `UAT-26`, `34`, `35` tidak bisa diuji asli | Ikbal, Muhammad Hamzah, Tarif; lalu Admin Optik | Data; barang wajib `IsPrescribable = false` |
| **B3** | Kebijakan izin Optik 0, dan daftar izinnya belum lengkap (`T1`) | Hanya superadmin yang bisa memakai; superadmin juga menutupi penolakan | Admin akses, Yoga | Tetapkan jabatan 5 peran, isi `OPT-PERM`, ditambah izin lintas modul `T1`. POST Akses Role menimpa seluruh set: baca dulu, kirim ulang utuh |
| **B4** | Tindakan Pemeriksaan Refraksi, tarifnya, dan kuncinya kosong | Pemeriksaan selesai tanpa pendapatan sampai diatur; tarif tidak hilang berkat `BE-OPT-062` | Master Data, Tarif, Yoga | Data + konfigurasi; tanpa kode |
| **B5** | Alur lengkap belum pernah berjalan di PostgreSQL | Token konkurensi, indeks unik, kunci stok, kunci idempoten serentak, dan transaksi hanya terbukti di SQLite | Yoga + pemilik mitra | Sesudah `B1`..`B4`: jalankan `FLOW-OPT-MVP-001` dan seluruh UAT dengan akun asli |
| **B6** | Kode belum sampai ke cabang integrasi | Layar belum bisa dipasang di lingkungan bersama | Yoga | Rincian di `T3` dan bagian 7 |

## 6. Temuan baru audit ini

| ID | Temuan | Akibat | Pemilik | Arah | Menahan rilis? |
| --- | --- | --- | --- | --- | --- |
| **T1** | **`OPT-PERM-v1.2` tidak memuat seluruh izin lintas modul yang dipakai layar.** Daftar lengkapnya ada di bagian 9; catatannya tersebar di brief baris 423 dan laporan `FE-OPT-010`, `020` | Admin akses yang mengikuti matriks akan memberi petugas layar pendaftaran yang tidak bisa mencari pasien (`403`), karena "—" berarti tidak diberikan. Superadmin menutupi masalah ini | Yoga Aji Pratama | Tambahkan ke matriks lewat pass blueprint (usulan: `evidence/2026-11-06-draft-izin-lintas-modul.md`), lalu buktikan dengan akun petugas | Ya |
| **T2** | **Diskon belum diuji pada Billing.** Uji backend Optik tidak memuat diskon sama sekali. `OPT-AC-016` dan `OPT-AC-037` baris kedua meminta uji kontrak, tetapi `UAT-28`, `29` hanya lulus dengan API tiruan. Dari pembacaan kode, `BillingDiscountService#ResolveApplicationAsync` menghitung dari jumlah × harga baris tanpa membedakan sumber, sehingga Rp 450.000 × 10% seharusnya Rp 45.000 | Perilakunya masih kesimpulan dari membaca kode | Yoga; Yasmina | Satu task backend kecil: uji kontrak diskon pada baris tagihan Optik | Ya, atau dibuktikan di `B5` |
| **T3** | **Uji backend Optik tidak ter-track Git:** `git ls-files` 0 berkas, sedangkan empat modul lain punya pengecualian di `.gitignore` | Seluruh 284 uji hanya ada di mesin ini. Pemeriksa dan CI tidak bisa mengulang bukti, dan uji hilang bila folder terhapus | Yoga | Tambah pengecualian `.gitignore` seperti modul lain, atau `git add -f` saat commit | Ya, sebelum merge |
| **T4** | **Aturan tanggungan perusahaan 0** | Pasien perusahaan terdaftar, tetapi menanggung penuh. Tidak ada galat yang memberi tahu petugas | Muhammad Hamzah | Data untuk barang Optik sebelum `UAT-30` asli | Ya, untuk pasien perusahaan |

## 7. Status kode terhadap cabang integrasi

Dibaca dari referensi lokal tanpa `git fetch`, sehingga angkanya bisa lebih besar.

| Repository | Maju / tertinggal | Optik di cabang integrasi | Berkas yang juga berubah di working tree |
| --- | --- | --- | --- |
| Backend, `origin/QuilvianIntegrationBackend` | 3 / 30 | 34 berkas, sedangkan `yoga` 46 berkas ter-track | `Program.cs`, `Migrations/ApplicationDbContextModelSnapshot.cs` |
| Frontend, `origin/QuilvianIntegrationFrontend` | 2 / 27 | 0 berkas hook Optik | Tidak ada |

Cabang integrasi membawa migration modul lain sampai `20261104071700`. Sesudah penggabungan, periksa `migrations list`.

## 8. Risiko dan bukti yang basi

### 8.1 Risiko

| ID | Risiko | Akibat | Pemilik | Mitigasi | Menahan rilis? |
| --- | --- | --- | --- | --- | --- |
| **R5** | `UAT-21` belum terbukti pada penjaga Rekam Medis asli | Izin berkas Rekam Medis untuk dokumen Optik belum dibuktikan | Yoga | Uji bersama `B5` dengan akun tanpa izin | Tidak, bila dijadwalkan |
| **R7** | Keputusan pelaksanaan belum dikonfirmasi: lunas per tagihan kunjungan, batas garansi termasuk hari terakhir, kunjungan asal batal diterima, power lensa kontak tidak terisi otomatis | Perilaku bisa berbeda dari harapan rumah sakit | Yoga | Konfirmasi saat `B5` | Tidak |
| **R8** | Stok kembali belum dikonfirmasi Ikbal (`OPT-DEC-060`). Bukti tertulis persetujuan mitra belum dilampirkan (`OPT-OQ-016`, `OPT-EVD-003`), dan `OPT-OQ-020` dicatat mengikuti usulan awalan `OPT-` | Persetujuan bisa dipertanyakan sesudah berjalan | Yoga; Ikbal; Muhammad Hamzah; Yasmina | Lampirkan bukti tertulis sebelum merge | Tidak, tetapi sebelum produksi |
| **R9** | Penyaring Rekonsiliasi Billing belum punya pilihan Optik. Laporan penjualan bruto | Kasir lebih sulit menemukan fakta Optik yang gagal | Yasmina; Yoga | Tambah pilihan | Tidak |
| **R10** | `OptOrderResponse` tanpa `ParentOrderId`; `OptWarrantyClaimResponse` hanya membawa id pemeriksa | Penelusuran remake di layar kurang lengkap; data tetap tersimpan | Yoga | Task kecil atau amendment brief | Tidak |
| **R11** | `UAT-27` belum dijalankan | `Down` hanya terbukti lewat analisis isi | Yoga | Uji pada basis data sekali pakai, bukan devYoga | Tidak |
| **R12** | `OPT-OQ-023`: tarif untuk kunjungan tertutup tanpa tagihan membuat tagihan `OPEN` baru | Kasir melihat tagihan baru pada kunjungan selesai. Tarif tidak hilang | Yasmina; disampaikan Yoga | Konfirmasi sebelum rilis | Disarankan dijawab dulu |
| **R13** | Modal diskon memuat paling banyak 100 kebijakan aktif sebelum menyaring. Batas 10% hanya dijaga nilai data, karena kebijakan `OPT-` bernilai lain tetap tampil dan langsung berlaku | Kebijakan Optik bisa terpotong; salah isi nilai langsung memengaruhi tagihan | Yasmina | Tinjau saat membuat `OPT-DISC-10` | Tidak |
| **R14** | Pekerja latar menyala bawaan begitu backend berjalan | Pada basis data bersama, tarif pemeriksaan tertinggal akan terkirim otomatis. devYoga saat ini 0 pemeriksaan | Yoga | Matikan lewat `OpticalManagement:ExamChargeRetry:Enabled` bila tidak diinginkan | Tidak |

### 8.2 Bukti yang basi

Artefak terkunci hash tidak disunting saat delivery. Yang tertulis "tidak terkunci" boleh diperbaiki langsung.

| Artefak | Keadaan | Pemilik |
| --- | --- | --- |
| `blueprint-manifest.md` baris Roadmap, Snapshot r3 | Masih tertulis `BE-OPT-061` ⛔, `FE-OPT-041` ⛔, dan snapshot tanpa task 061, 062, 041, 042. Tidak terkunci. **Diperbaiki sesudah audit, 2026-11-06** | `manage-module-blueprint` |
| `roadmap/requirement-traceability.md` baris 13 dan `FR-OPT-041` | Baris 13 masih "Tidak ada task amendment r9 yang selesai". `FR-OPT-041` masih "belum dengan penjamin sungguhan", padahal `BE-OPT-061` membuktikannya di harness. Tidak terkunci. **Diperbaiki sesudah audit, 2026-11-06** | `plan-module-delivery` |
| `testing/readiness-report.md` | Masih audit `fedd5e98`. Tidak terkunci. **Digantikan laporan ini** | `verify-module-readiness` |
| Kontrak, PRD, brief, arsitektur r3 | Keterangan "menunggu", "usulan", dan penunjuk `FinalizedAt` (decision log bagian 14). Isi rancangan tidak berubah | `design-business-module` |
| `contracts/permission-audit-matrix.md` | Tanpa izin lintas modul (`T1`). **Isi kontrak perlu ditambah.** Usulannya: `evidence/2026-11-06-draft-izin-lintas-modul.md`, masih `draft` | `design-business-module` |
| `01-existing-capability-map.md` | `OPT-CAP-07`, `OPT-CAP-17` belum disegarkan | `trace-existing-capabilities` |
| `03-domain-architecture.md` | Kepala berkas masih `draft` | `hospital-domain-architect` |
| Bukti harness | **Tidak basi:** 284/284 dijalankan ulang di audit ini | — |

## 9. Endpoint lintas modul yang dipakai layar Optik

Seluruh endpoint Optik di bawah `/v1/health-services/optical-management` berjumlah 45: Visit 3, Refraction Exam 7, Prescription 7, Order 14, Warranty Claim 5, Reference Data 5, Report 4. Jumlahnya sama dengan kontrak.

Tabel berikut adalah endpoint modul lain. Kolom terakhir menunjukkan apakah izinnya tercantum pada tabel peran `OPT-PERM-v1.2`.

#### Patient Management / Master Data

| Method | Path | Kegunaan di Optik | Hak akses | Di matriks? |
| --- | --- | --- | --- | --- |
| `GET` | `/patients/admin/options` | Cari pasien saat mendaftar | `Patient : Read` | **Tidak** |
| `GET` | `/patient-insurances/admin/options` | Pilih kartu asuransi | `PatientInsurance : Read` | **Tidak** |
| `GET` | `/patient-company-guarantors/admin/options` | Pilih kartu penjamin (`UAT-30`) | `PatientCompanyGuarantor : Read` | **Tidak** |

#### Registration Management / Patient Encounters

| Method | Path | Kegunaan di Optik | Hak akses | Di matriks? |
| --- | --- | --- | --- | --- |
| `GET` | `/admin/options` | Pilih kunjungan asal rujukan | `PatientEncounter : Read` | **Tidak**; ada di brief |

#### Billing Management

| Method | Path | Kegunaan di Optik | Hak akses | Di matriks? |
| --- | --- | --- | --- | --- |
| `GET` | `/billing/invoices/{id}` | Harga dan diskon baris pesanan | `BillingInvoice : Read` | **Tidak**; brief menyebut "izin baca tagihan" |
| `POST` | `/billing/invoices/{id}/discounts` | Terapkan diskon | `BillingDiscount : Create` | Ya |
| `GET` | `/master-data/discount-policies` | Pilihan modal diskon | `DiscountPolicy : Read` | Ya |

#### Master Data

| Method | Path | Kegunaan di Optik | Hak akses | Di matriks? |
| --- | --- | --- | --- | --- |
| `GET` | `/health-services/master-data/drugs/options` | Pilih barang di Daftarkan Barang | `Drug : Read` | Ya |
| `GET` | `/administrator/master-data/suppliers/options` | Pilih lab lensa luar | `Supplier : Read` | **Tidak** |

Enam dari sembilan izin belum tercantum di matriks. Laporan `FE-OPT-010` juga menyebut `PatientEncounter : Create`; audit ini tidak menemukan pemeriksaan izin itu di `OptVisitController`, `OptVisitService`, maupun `EncounterIntakeService`, sehingga perlu dipastikan dengan akun petugas.

## 10. Penelusuran requirement

### 10.1 Requirement (34 `FR`)

Rantai keputusan → kontrak → task → kode → uji tersambung pada seluruh baris traceability.

| Kekuatan bukti | Jumlah | Requirement |
| --- | ---: | --- |
| Backend asli | 4 | `FR-OPT-001`, `002`, `010`, `011` |
| Harness, dan layar bila ada; belum data asli | 26 | `FR-OPT-012`, `020`, `021`, `022`, `030`..`033`, `040`..`044`, `050`, `060`..`062`, `070`..`072`, `080`, `081`, `090`, `091`, `100`, `111` |
| Layar API tiruan saja | 3 | `FR-OPT-082`, `083` (`T2`), `112` |
| Belum dapat diuji | 1 | `FR-OPT-110` (`B2`) |

### 10.2 UAT MVP (36)

`UAT-19` dikeluarkan dari MVP oleh `OPT-DEC-079`.

| Hasil | Jumlah | UAT |
| --- | ---: | --- |
| Backend asli | 2 | `UAT-01`, `25` |
| Harness + layar API tiruan | 25 | `UAT-02`..`18`, `20`, `22`..`24`, `30`..`33` |
| Harness saja | 2 | `UAT-36`, `37` |
| Layar API tiruan saja | 5 | `UAT-21`, `28`, `29`, `34`, `35` |
| Belum dapat diuji | 1 | `UAT-26` (`B2`) |
| Belum dijalankan | 1 | `UAT-27` (`R11`) |

### 10.3 Matriks penerimaan (78 baris aktif)

Klasifikasi ini diturunkan dari label dan nama uji serta laporan task, bukan dari menjalankan tiap baris satu per satu.

| Hasil | Jumlah | Baris |
| --- | ---: | --- |
| Berbukti penuh pada jenis uji yang diminta | 59 | Selain di bawah |
| Sebagian: meminta UI + backend, tetapi layar dan backend diuji terpisah | 13 | `OPT-AC-010`; `027` baris 1; `030`; `031` baris 1; `032`; `033`; `036` baris 1; `037` kedua baris; `038` ketiga baris; E2E penuh |
| Sebagian: alasan lain | 4 | `OPT-AC-008` dua petugas serentak; `035` serentak di PostgreSQL; `017` piutang; `039` tarif berlaku sesudah tanggal pemeriksaan |
| Tanpa bukti | 2 | `OPT-AC-016` (`T2`), `020` (`B2`) |

### 10.4 Definition of Done (`04-prd-to-mvp.md` bagian 19)

| Butir | Status | Catatan |
| --- | --- | --- |
| Prefix `Opt` tercatat | ✅ Penuh | `OPT-EVD-001` |
| Satu kasus `FLOW-OPT-MVP-001` berjalan | ⚠ Harness | `Be053EndToEndTests` |
| Resep tidak berubah sesudah dikerjakan | ⚠ Harness | `UAT-06` |
| Izin kedaluwarsa menolak | ⚠ Harness | `UAT-04` |
| Uang muka kurang menolak | ⚠ Harness | `UAT-08` |
| Stok dipotong tepat sekali dan kembali | ⚠ Harness | Serentak belum; `OPT-DEC-060` belum dikonfirmasi Ikbal |
| Kunjungan tidak tertutup selama pesanan berjalan | ⚠ Harness | `UAT-18` |
| Batal hanya dengan persetujuan | ⚠ Harness | `UAT-15` |
| Remake hanya dalam garansi, tanpa tagihan | ⚠ Harness | `UAT-16`, `17` |
| **Seluruh master MVP terisi** | ⛔ Belum | 3/12 |
| Persetujuan mitra tercatat | ✅ Penuh menurut definisi PRD | Status disetujui di decision log; bukti tertulis belum (`R8`) |
| Simpan dua kali tidak ganda | ⚠ Harness + layar | Serentak sungguhan belum |
| Pasien perusahaan dilayani sampai uang muka | ⚠ Harness + layar | `T4` |
| Diskon sampai 10%, hanya kebijakan Optik | ⚠ Layar saja | `T2`; kebijakan belum ada |
| Tarif tertinggal tertagih tanpa ganda | ⚠ Harness | `UAT-36`, `37`; `R12` |
| **Seluruh baris matriks punya bukti** | ⛔ Belum | 59/78 penuh |

## 11. Langkah berikutnya

Langkah 1, 2, dan 3 tidak mengubah kode aplikasi.

| Urutan | Pekerjaan | Pemilik | Alur |
| ---: | --- | --- | --- |
| 1 | Lampirkan bukti tertulis persetujuan; tanyakan `OPT-OQ-023`; pastikan awalan `OPT-` ke Yasmina | Yoga Aji Pratama | Mitra |
| 2 | Isi data: profesi, spesialisasi, STR/SIP; kategori, barang, depo, stok, tarif; tindakan refraksi; `OPT-DISC-10`; aturan tanggungan; lalu isi kode `OPT-OQ-022` | SDM, Master Data, Farmasi, Tarif, Billing, Yoga | Input data sesuai bagian 4 |
| 3 | Lengkapi `OPT-PERM` dengan izin lintas modul (`T1`), segarkan bagian 14, kunci ulang hash; lalu isi kebijakan izin 5 peran dan buat akun uji | Yoga; admin akses | `design-business-module`, `manage-module-blueprint`; layar Akses Role |
| 4 | Sertakan uji Optik di Git (`T3`), commit, gabungkan dengan integrasi, lalu jalankan ulang uji | Yoga Aji Pratama | `B6` |
| 5 | Uji kontrak diskon pada baris tagihan Optik (`T2`) | Pengembang | Satu task `build-module-backend`, atau dibuktikan di langkah 6 |
| 6 | Jalankan `FLOW-OPT-MVP-001` dan seluruh UAT dengan akun asli di PostgreSQL, termasuk dua petugas serentak | Yoga Aji Pratama, pemilik mitra | Bukti baru untuk `testing/` |
| 7 | Audit ulang | — | `verify-module-readiness` |

Sesudah langkah 6, verdict paling mungkin naik menjadi `READY_WITH_CONDITIONS`, dengan `R5`, `R7`..`R14` sebagai kondisi, bila tidak ada temuan baru.

## 12. Batas audit

| Hal | Keterangan |
| --- | --- |
| Dijalankan ulang | `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False`: 284/284; waktu uji 4 menit 50 detik. Hanya menyentuh `bin/obj`. Tidak ada server menyala |
| Diambil dari sesi yang sama | E2E frontend 152/152, lint, build, dan uji unit dari `FE-OPT-041`. Berkas frontend terakhir disunting 11.57, sebelum E2E penuh selesai pukul 12.01 |
| Kueri basis data | Hanya agregat, lewat sesi baca-saja. Tidak ada data pasien yang dibaca |
| Tidak diperiksa | Server dev bersama, produksi, variabel lingkungan server, kinerja pada data besar, isi berkas Rekam Medis asli, serta kebijakan izin jabatan Admisi pada izin lintas modul |
| Referensi Git | Selisih cabang dibaca tanpa `git fetch` |
| Penulisan | Audit tidak mengubah apa pun. Sesudahnya, atas persetujuan Yoga Aji Pratama ("simpan"), laporan ini disimpan, baris basi manifest dan traceability diperbaiki, dan draf izin lintas modul ditulis. Source aplikasi dan basis data tetap tidak berubah |
