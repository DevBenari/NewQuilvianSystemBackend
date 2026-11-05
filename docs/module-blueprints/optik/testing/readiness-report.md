# Optik — Laporan Kesiapan Modul

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `OPT-BP-001`, revisi `1`, status `approved` sisi Optik |
| Tanggal audit | 2026-11-05 |
| Sifat audit | Baca-saja. Tidak ada perubahan source dan tidak ada tulisan ke basis data |
| Source backend | `fedd5e986a80f503a64f512c16d77c2154d3af9f` (branch `yoga`) |
| Source frontend | `086bb452fc77910c6872c77a006355d3c3771f06` (branch `YogaV2`) + 103 berkas Optik belum di-commit |
| Lingkungan yang diperiksa | Basis data devYoga (`QuilvianNewDevYoga`, PostgreSQL 15.15). Server dev bersama dan produksi **tidak** diperiksa |
| Kontrak | `OPT-API-v1` + delta `v1.1`, `OPT-STATE-v1`, `OPT-VAL-v1`, `OPT-INT-v1`, `OPT-PERM-v1`, `OPT-PRD-v1` |
| Keputusan acuan | `00-interview-decisions.md` r8; `OPT-DEC-001`..`076` |

## 1. Kesimpulan

**Verdict: `NOT_READY`**

**Kode Optik sudah lengkap dan teruji, tetapi modul belum bisa dipakai melayani satu pasien pun dari awal sampai akhir.** Alur tulis berhenti pada dua titik yang tidak bisa dilewati, karena data dan konfigurasi prasyaratnya kosong. Rantai lengkapnya juga belum pernah dijalankan pada PostgreSQL.

| Pertanyaan | Jawaban | Bukti |
| --- | --- | --- |
| Apakah semua task selesai? | Ya: backend 23/23, frontend 14/14 | Roadmap |
| Apakah kodenya lulus uji? | Ya: backend 247/247 pada HEAD, frontend E2E 146/146 | Dijalankan ulang atau dijalankan pada 2026-11-05 |
| Apakah RO bisa memeriksa pasien sekarang? | **Tidak** | Kode profesi tidak ada; profesi RO, spesialisasi mata, dan izin praktik di devYoga 0 |
| Apakah petugas bisa membuat pesanan sekarang? | **Tidak** | Barang Optik, kategori, profil, depo, dan tarif barang 0 |
| Apakah petugas non-superadmin bisa membuka layar Optik? | **Tidak** | Kebijakan izin Optik 0 |
| Apakah alur lengkap pernah berjalan di PostgreSQL? | **Tidak** | Hanya pendaftaran kunjungan (1 baris) dan baca |

`READY_WITH_CONDITIONS` tidak dipilih, karena jalur utama tertutup sepenuhnya, bukan terbatas pada risiko pinggir. Empat blocker pertama (bagian 5) bisa selesai tanpa mengubah kode, sehingga jarak ke kesiapan sebagian besar berupa data, konfigurasi, dan satu putaran uji.

## 2. Skor

Skor menilai bukti yang ada, bukan jumlah berkas. Skor gabungan hanya gambaran: **verdict ditentukan blocker, bukan rata-rata.**

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| ---: | ---: | ---: | --- | --- |
| Fondasi | 15% | 90 | Hash artefak cocok 17/18; registry `Opt` `ACTIVE`; 3/3 migration Optik diterapkan (riwayat 380); 7/7 controller Optik terdaftar di `SysControllerAccess` | Manifest basi (bagian 8) |
| Backend | 25% | 85 | Task 23/23; `OpticalTests` 247/247 di `fedd5e98`; matriks izin 45/45 endpoint (`OpticalPermissionRegistryTests.cs#L46@fedd5e98`) | R1 idempoten, R2 tarif tertinggal |
| Frontend | 20% | 85 | Task 14/14; layar `N01`..`N15`; E2E 146/146; uji unit Optik lulus | B6 belum di-commit; Daftarkan Barang menunggu keputusan |
| Integrasi/runtime | 25% | 20 | Data prasyarat terisi 3/12; konfigurasi 0/3; kebijakan izin 0 | B1..B5 |
| Verifikasi | 15% | 40 | UAT 2/27 dengan backend asli, 21/27 harness + layar; matriks penerimaan 38/47 berbukti penuh | B5; `UAT-19`, `21`, `26`, `27` |
| **Gabungan** | 100% | **63** | | **Tidak menentukan verdict** |

### Kemajuan kode dibanding kesiapan nyata

| Ukuran | Kemajuan kode | Kesiapan nyata |
| --- | --- | --- |
| Requirement (27 `FR`) | 25/27 punya kode dan uji | **4/27** terbukti dengan backend asli: `FR-OPT-001`, `002`, `010`, `011` |
| UAT (27) | 23/27 lulus pada lapis layar atau harness | **2/27** dengan backend asli: `UAT-01`, `25` |
| Definition of Done (12 butir) | 8 terpenuhi di harness | **1/12** terpenuhi penuh; 8 di harness; 3 belum (bagian 7) |
| Data master (12 jenis) | Semua punya layar atau cara mengisi | **3/12** terisi |

## 3. Yang terjadi bila dicoba hari ini

**Contoh:** Ibu Sari (samaran) datang ke Optik RSMMC membawa rujukan Poli Mata, memeriksakan mata, lalu memesan kacamata.

| Langkah | Pelaku | Hasil hari ini | Sebab |
| --- | --- | --- | --- |
| 1. Daftarkan kunjungan | Admisi | ✅ Berhasil, bila cara bayar Tunai atau Asuransi | Sudah terbukti (`ENC-RSMMC-00186`) |
| 1b. Penjamin Perusahaan | Admisi | ⛔ Ditolak *"Penjamin perusahaan hanya dapat dipilih lewat pendaftaran loket."* | R3 |
| 1c. Membuka menu Optik | Petugas | ⛔ Menu tidak tampil | B3: kebijakan izin 0 |
| 2. Mulai pemeriksaan | RO | ⛔ `403` *"Anda tidak berwenang membuat resep atau pemeriksaan refraksi."* | B1 |
| 3. Tulis resep | RO | ⛔ Tertahan oleh sebab yang sama | B1 |
| 4. Tagih pemeriksaan | Sistem | ⛔ Tarif tidak masuk tagihan | B4 |
| 5. Buat pesanan | Petugas | ⛔ Katalog kosong, tidak ada barang untuk dipilih | B2 |
| 6. Kirim, siap, serahkan | Petugas | ⛔ Tidak tercapai; depo juga belum ada | B2 |
| 7. Keluhan dan remake | Petugas, RO, Kepala Optik | ⛔ Tidak tercapai | B1, B2 |
| 8. Laporan, pengaturan | Kepala/Admin Optik | ✅ Terbuka untuk superadmin; berisi nol | Hanya terbukti untuk baca |

## 4. Bukti data dan konfigurasi

Hasil kueri baca-saja devYoga, 2026-11-05. Sesi dipaksa `default_transaction_read_only = on`, dan hanya agregat yang diambil.

### 4.1 Data master (rencana `02-backend-architecture.md` bagian 8)

| Master | Kebutuhan minimum | Isi devYoga | Status | Pemilik |
| --- | --- | --- | --- | --- |
| `MstServiceUnit` | Satu unit Optik | 1 aktif | ✅ | — |
| `MstDrugStorageLocation` | Depo Optik siap layan | **0** | ⛔ | Farmasi (Ikbal) |
| `MstDrugCategory` | 4 kategori Optik | **0** | ⛔ | Master Data (Muhammad Hamzah) |
| `MstDrug` | Barang Optik, `IsPrescribable = false` | **0** dari 7.862 barang aktif | ⛔ | Master Data, Farmasi |
| `OptItemProfile` | Satu per barang | **0** | ⛔ | Admin Optik (sesudah barang ada) |
| `MstProcedure` + `MstTariff` | Tindakan Pemeriksaan Refraksi + tarif | **0** | ⛔ | Master Data, Tarif |
| `MstTariff` | Tarif per barang | **0** (belum ada barang) | ⛔ | Tarif |
| `MstSupplier` | Lab lensa luar | 2 aktif | ✅ | — |
| `OptSetting` | Satu per unit | 1, nilai bawaan; BC/DIA masih `PROPOSED` | ✅ | Yoga + RO/Sp.M untuk konfirmasi BC/DIA |
| `MstDiscountPolicy` | Kebijakan Diskon Optik | **0** | ⛔ | Billing (Yasmina) |
| `SysAccessPolicy` | Izin 5 peran Optik | **0** pada ketujuh controller | ⛔ | Admin akses, Yoga |
| `WfpCredentialLicense` | STR + SIP RO/Sp.M | **0** seluruh pegawai | ⛔ | SDM |

**Terisi 3 dari 12.** `NumberSeries` tidak dihitung, karena dibuat otomatis saat pertama dipakai.

Tambahan yang menentukan gerbang izin praktik: `MstProfession` tidak punya profesi Refraksionis/Optometris (0 dari 10), `MstSpecialization` tidak punya spesialisasi mata (0), dan dokter spesialis mata 0.

### 4.2 Konfigurasi

| Kunci | Fungsi | Di `appsettings.json` / `Development` | Bila kosong |
| --- | --- | --- | --- |
| `OpticalManagement:Practitioner:RefractionistProfessionCodes` | Kode profesi RO | Tidak ada | Setiap akun ditolak `OPT-VAL-001` |
| `OpticalManagement:Practitioner:OphthalmologistSpecializationCodes` | Kode spesialisasi Sp.M | Tidak ada | Sama |
| `OpticalManagement:Billing:RefractionExamProcedureCode` | Tindakan tarif pemeriksaan | Tidak ada | Final tetap berhasil, **tarif tidak masuk tagihan** |

**Terisi 0 dari 3.** Pengisian lewat variabel lingkungan di server tidak dapat dilihat dari audit ini.

## 5. Blocker, urut menurut dampak

| ID | Blocker | Dampak | Bukti | Pemilik | Arah penyelesaian |
| --- | --- | --- | --- | --- | --- |
| **B1** | Gerbang izin praktik tidak bisa dilewati siapa pun: kode profesi kosong, profesi RO dan spesialisasi mata tidak ada, izin praktik 0 | Pemeriksaan, resep, dan periksa ulang keluhan ditolak, sehingga seluruh alur klinis tertutup | `OptPractitionerGuard.cs#RefractionistCodesKey@fedd5e98`; bagian 4 | Yoga Aji Pratama (kode, `OPT-DEC-058`); SDM (data); Master Data (profesi) | Konfigurasi + data; tanpa kode. Sekalian jawab `OPT-CAP-07`: pembaca izin mencocokkan awalan `SIP`/`STR` (`BE-OPT-004`) |
| **B2** | Barang Optik belum ada: kategori, barang, profil, depo, dan tarif 0 | Pesanan dan penjualan langsung tidak bisa dibuat; `UAT-26` tidak dapat diuji | Bagian 4.1 | Farmasi (Ikbal), Master Data (Muhammad Hamzah), Tarif; lalu Admin Optik | Data; tanpa kode. Barang wajib `IsPrescribable = false`, karena itu syarat tampil di Daftarkan Barang |
| **B3** | Kebijakan izin Optik 0 pada ketujuh controller | Hanya superadmin yang bisa memakai; superadmin juga menutupi penolakan, sehingga uji izin nyata tertunda | `SysAccessPolicy` | Admin akses, Yoga Aji Pratama | Tetapkan jabatan untuk 5 peran `OPT-PERM-v1`, isi lewat Akses Role. POST menimpa seluruh set: baca dulu, kirim ulang utuh |
| **B4** | Tindakan Pemeriksaan Refraksi, tarifnya, dan kuncinya kosong | Pemeriksaan selesai tanpa pendapatan; lihat juga R2 | `OptBillingGateway.cs#ProcedureNotConfiguredMessage@fedd5e98`; `BE-OPT-031` | Master Data, Tarif, Yoga Aji Pratama | Data + konfigurasi; tanpa kode |
| **B5** | Alur lengkap belum pernah berjalan di PostgreSQL. Harness memakai SQLite, dan laporan backend sampai `BE-OPT-053`, `054` mencatat alurnya belum dicoba di PostgreSQL | Token konkurensi, indeks unik, kunci idempoten stok, dan transaksi belum terbukti pada mesin yang dipakai. `OPT-AC-008` serentak belum diuji | `Infrastructure/TestDatabase.cs#SqliteConnection`; laporan `BE-OPT-053` | Yoga Aji Pratama + pemilik mitra | Sesudah B1..B4: jalankan `FLOW-OPT-MVP-001` dan `UAT-01`..`26` dengan akun asli |
| **B6** | Frontend Optik belum di-commit (103 berkas), dan kedua branch tertinggal dari integrasi (backend 27, frontend 24 commit, menurut referensi lokal tanpa `fetch`) | Layar belum bisa dipasang di lingkungan bersama. Penggabungan bisa bentrok pada menu, `store.jsx`, dan urutan migration | `git status`, `git log` | Yoga Aji Pratama | Commit, gabungkan, jalankan ulang uji. Periksa `migrations list` sesudah penggabungan |

## 6. Risiko dan keputusan terbuka

| ID | Risiko | Akibat bila diabaikan | Pemilik | Mitigasi atau arah | Menahan rilis? |
| --- | --- | --- | --- | --- | --- |
| **R1** | **Idempoten tidak sesuai kontrak.** `api-contract.md` baris 17, `NFR-003`, dan brief §6 meminta `Idempotency-Key` pada tiap aksi, tetapi backend hanya membacanya di `POST visits`. Aksi lain mengandalkan `ExpectedVersion` dan kunci stok per baris | `POST orders`, `POST prescriptions`, dan `POST warranty-claims` bisa membuat data ganda bila jaringan putus sesudah server menyimpan, lalu petugas mengulang. Aksi berversi aman | Yoga Aji Pratama | Putuskan: tambah kunci pada ketiga `POST` pembuat (`build-module-backend`), atau ubah kontrak dan `NFR-003` (`design-business-module`) | Ya, perlu keputusan |
| **R2** | **Tarif pemeriksaan bisa tertinggal permanen.** Bila penyerahan gagal sebelum fakta tercatat, tarif hanya dicoba lagi saat pemeriksaan direvisi | Pemeriksaan final tanpa pendapatan, dan tidak ada jalan ulang. Laporan `BE-OPT-031` menawarkan tombol atau pekerja latar; `BE-OPT-053` tidak menambahkannya | Yoga Aji Pratama, Yasmina | Satu task backend + layar kecil: kirim ulang tarif. Mitigasi sementara: pantau `BillingFactEmittedAt` kosong | Ya, sesudah B4 berkurang tetapi tidak hilang |
| **R3** | **Penjamin Perusahaan selalu ditolak Registrasi,** padahal `OPT-DEC-030` memasukkan pasien perusahaan ke MVP | Pasien perusahaan tidak bisa dilayani di Optik. Rumus tanggungan `OPT-AC-027` juga belum teruji dengan penjamin sungguhan | Muhammad Hamzah (Registrasi), Yoga Aji Pratama | Perluas `EncounterIntakeService`, atau keluarkan dari MVP lewat amendment | Ya, perlu keputusan |
| **R4** | **`UAT-19` belum terpenuhi.** `OPT-AC-011`, `016` meminta diskon 15% menunggu persetujuan, tetapi Billing memberlakukan kebijakan `PROMO_ITEM` langsung, dan kebijakan diskon tidak bisa dibatasi untuk Optik. `OPT-APR-004` berstatus `POST-MVP` | Bila kebijakan dibuat, diskon langsung mengurangi tagihan tanpa persetujuan. `04-prd-to-mvp.md` masih memuat `UAT-19` dan menyebut `OPT-APR-004` sebagian di `MVP-3` | Yoga Aji Pratama, Yasmina | Putuskan: keluarkan resmi `UAT-19`, `OPT-AC-011`, `016` dari MVP dan tetapkan aturan pakai sementara, atau tarik `OPT-APR-004` ke MVP | Ya, perlu keputusan |
| **R5** | **`UAT-21` belum terbukti pada penjaga asli.** Layar Rekam Medis lulus dengan `403` tiruan | Izin berkas Rekam Medis untuk dokumen Optik belum dibuktikan | Yoga Aji Pratama (pemilik Rekam Medis) | Uji bersama B5 dengan akun tanpa izin | Tidak, bila dijadwalkan di B5 |
| **R6** | **Aksi Daftarkan Barang di luar brief** (`FE-OPT-031`) | Tanpa aksi ini katalog tidak pernah terisi lewat layar | Yoga Aji Pratama | Setujui dan segarkan brief §5.12 | Ya, keputusan kecil |
| **R7** | **Keputusan pelaksanaan belum dikonfirmasi:** lunas dinilai per tagihan kunjungan (`BE-OPT-040`), batas garansi termasuk hari terakhir (`BE-OPT-050`), kunjungan asal berstatus batal tetap diterima (`BE-OPT-003`), power lensa kontak tidak terisi otomatis (`FE-OPT-012`) | Perilaku bisa berbeda dari harapan rumah sakit. **Contoh:** pesanan siap tidak bisa diserahkan selama pesanan lain di kunjungan sama belum lunas | Yoga Aji Pratama | Konfirmasi saat B5 | Tidak |
| **R8** | **Stok kembali saat batal belum dikonfirmasi Farmasi** (`OPT-DEC-060`), dan bukti tertulis persetujuan mitra belum dilampirkan (`OPT-OQ-016`) | Cara stok kembali bisa ditolak Farmasi sesudah berjalan | Ikbal; Yoga Aji Pratama | Konfirmasi tertulis sebelum merge | Tidak, tetapi sebelum produksi |
| **R9** | **Celah pantauan:** penyaring Rekonsiliasi Billing tidak punya pilihan Optik; hanya tampil di *Semua pelayanan*. Laporan penjualan tidak berkurang oleh kredit baris batal | Kasir lebih sulit menemukan fakta Optik yang gagal; angka laporan bruto bisa lebih tinggi dari yang diterima | Yasmina; Yoga Aji Pratama | Tambah pilihan; laporan sudah berketerangan bruto | Tidak |
| **R10** | **Selisih data backend:** `OptOrderResponse` tanpa `ParentOrderId` (brief §5.8 meminta tautan ke pesanan asal), `OptWarrantyClaimResponse` tanpa nama pemeriksa, riwayat pengaturan tanpa nama pelaku | Penelusuran remake dan audit di layar kurang lengkap; data tetap tersimpan | Yoga Aji Pratama | Task backend + frontend kecil, atau amendment brief | Tidak |
| **R11** | **`UAT-27` belum dijalankan** | `Down` hanya terbukti lewat analisis isi migration | Yoga Aji Pratama | Uji pada basis data sekali pakai, bukan devYoga | Tidak |

## 7. Penelusuran requirement

### 7.1 Requirement (27 `FR`)

Rantai keputusan → kontrak → task → kode → uji tersambung pada seluruh baris `roadmap/requirement-traceability.md`. Yang berbeda adalah kekuatan buktinya.

| Kekuatan bukti | Jumlah | Requirement |
| --- | ---: | --- |
| Terbukti dengan backend asli | 4 | `FR-OPT-001`, `002`, `010`, `011` |
| Harness, dan layar bila ada; belum dengan data asli | 21 | `FR-OPT-020`, `021`, `030`, `031`, `032`, `040`, `041`, `042`, `043`, `050`, `060`, `061`, `062`, `070`, `071`, `080`, `081`, `090`, `091`, `100`, `111` |
| Tertahan | 2 | `FR-OPT-082` (R4), `FR-OPT-110` (B2) |

`NFR-001`..`007`: terbukti di harness per task. `NFR-003` hanya sebagian (R1). `NFR-001`, `002` belum terbukti di PostgreSQL (B5).

### 7.2 UAT (27)

| Hasil | Jumlah | UAT |
| --- | ---: | --- |
| Dengan backend asli | 2 | `UAT-01`, `UAT-25` |
| Harness + layar API tiruan | 21 | `UAT-02`..`18`, `20`, `22`, `23`, `24` |
| Hanya layar | 1 | `UAT-21` (R5) |
| Belum terpenuhi | 1 | `UAT-19` (R4) |
| Belum dapat diuji | 1 | `UAT-26` (B2). Kode `PrescribingDrugController` memang menyaring `IsPrescribable`, tetapi belum ada barang untuk membuktikannya |
| Belum dijalankan | 1 | `UAT-27` (R11) |

Rincian per UAT ada di laporan `task/report/frontend/FE-OPT-033.md` bagian 2. Stok, piutang, dan tagihan Rp 0 pada `UAT-07`, `10`, `14`, `16` hanya terbukti di harness.

### 7.3 Matriks penerimaan (47 baris `testing/acceptance-test-matrix.md`)

| Hasil | Jumlah | Baris |
| --- | ---: | --- |
| Berbukti penuh pada jenis uji yang diminta | 38 | Selain di bawah |
| Sebagian | 6 | `OPT-AC-008` serentak; `OPT-AC-017` piutang menunggu kredit Billing; `OPT-AC-027` (2 baris) tanpa penjamin sungguhan; `OPT-AC-010` dan E2E penuh, yang diminta jenis E (UI + backend) tetapi hanya harness lewat controller dan layar terpisah |
| Tanpa bukti | 3 | `OPT-AC-011`, `016` (R4), `020` (B2) |

Matriks belum memuat `OPT-AC-030`..`034` dari decision log. Kelimanya punya bukti: `030`, `032`, `033` di E2E frontend, `031`, `034` di `BE-OPT-054`. `OPT-AC-015` milik slice BPJS, di luar MVP.

### 7.4 Definition of Done (`04-prd-to-mvp.md` bagian 19)

| Butir | Status | Catatan |
| --- | --- | --- |
| Prefix `Opt` tercatat | ✅ Penuh | `OPT-EVD-001`; `BE-OPT-002` |
| Satu kasus `FLOW-OPT-MVP-001` berjalan | ⚠ Harness | `Be053EndToEndTests`; belum dengan layar dan data asli |
| Resep tidak berubah sesudah dikerjakan | ⚠ Harness | `UAT-06` |
| Izin kedaluwarsa menolak | ⚠ Harness | `UAT-04` |
| Uang muka kurang menolak | ⚠ Harness | `UAT-08` |
| Stok dipotong tepat sekali dan kembali | ⚠ Harness | Serentak belum; `OPT-DEC-060` belum dikonfirmasi |
| Kunjungan tidak tertutup selama pesanan berjalan | ⚠ Harness | `UAT-18` |
| Batal hanya dengan persetujuan | ⚠ Harness | `UAT-15` |
| Remake hanya dalam garansi, tanpa tagihan | ⚠ Harness | `UAT-16`, `17` |
| **Seluruh master MVP terisi** | ⛔ Belum | 3/12 di devYoga |
| **Persetujuan mitra tercatat** | ⛔ Belum | 10/12; `OPT-APR-003`, `004` menunggu sebagai `POST-MVP`; bukti tertulis belum dilampirkan |
| **Seluruh baris matriks punya bukti** | ⛔ Belum | 38/47 penuh |

## 8. Bukti yang basi

| Artefak | Keadaan | Dampak | Arah |
| --- | --- | --- | --- |
| `blueprint-manifest.md` | Hash `00-interview-decisions.md` masih r4, padahal berkasnya r8. Status masih menyebut persetujuan mitra menunggu. Tabel roadmap masih r1, padahal backend r3 dan frontend r2 | `STALE`. Keputusan r5..r8 sudah disetujui, tetapi hash belum dikunci ulang. Tidak mengubah verdict | `manage-module-blueprint`: kunci ulang hash |
| Delta `OPT-API-v1.1` | Belum terkunci hash; bentuknya mengikuti laporan `BE-OPT-054` | Penyaring dan pratinjau harga berdasar kontrak yang belum terkunci | `manage-module-blueprint` |
| `testing/acceptance-test-matrix.md` | Revisi 1 `draft`, belum memuat `OPT-AC-030`..`034` | Daftar penerimaan tidak lengkap | Segarkan bersama penguncian hash |
| Traceability, roadmap, laporan frontend | Belum di-commit; baseline tertulis `2a5094ca` / `b6dd6c304` | Status terkini hanya ada di working tree | Ikut B6 |
| Bukti harness | **Tidak basi:** 247/247 dijalankan ulang pada HEAD. Laporan `BE-OPT-053` mencatat 239, sebelum `BE-OPT-054` menambah uji | — | — |

Sebanyak 17 dari 18 hash artefak manifest cocok dengan isi berkas.

## 9. Endpoint yang diaudit

Seluruhnya di bawah `/v1/health-services/optical-management`; total 45 endpoint, sama dengan uji registri izin.

| Grup Swagger | Endpoint | Baca dengan backend asli | Tulis dengan backend asli | Tulis di harness |
| --- | ---: | --- | --- | --- |
| Opt Visit | 3 | ✅ | ✅ `POST visits` | ✅ |
| Opt Refraction Exam | 7 | ✅ kosong | ⛔ B1 | ✅ |
| Opt Prescription | 7 | ✅ kosong | ⛔ B1 | ✅ |
| Opt Order | 14 | ✅ kosong | ⛔ B2 | ✅ |
| Opt Warranty Claim | 5 | ✅ kosong | ⛔ B1, B2 | ✅ |
| Opt Reference Data | 5 | ✅ | Tidak dijalankan, agar devYoga tidak berubah; penolakan `409` dan `400` terbukti | ✅ |
| Opt Report | 4 | ✅ kosong | — | ✅ |

## 10. Langkah berikutnya

Urutan disusun agar uji asli dapat dimulai secepat mungkin. Langkah 1..3 tidak mengubah kode.

| Urutan | Pekerjaan | Pemilik | Alur |
| ---: | --- | --- | --- |
| 1 | Putuskan R1, R3, R4, R6; isi kode profesi RO/Sp.M dan kode tindakan | Yoga Aji Pratama | Keputusan; `qv-grill` bila perlu amendment |
| 2 | Lengkapi data: profesi, spesialisasi, STR/SIP; kategori, barang, depo, stok, tarif; tindakan refraksi | SDM, Master Data, Farmasi, Tarif | Input data sesuai bagian 4.1 |
| 3 | Isi kebijakan izin 5 peran dan buat akun uji; daftarkan barang lewat katalog | Admin akses, Admin Optik | Layar Akses Role; `FE-OPT-031` |
| 4 | Commit dan gabungkan frontend; segarkan manifest | Yoga Aji Pratama | B6; `manage-module-blueprint` |
| 5 | Task perbaikan hasil keputusan: `Idempotency-Key` pada `POST` pembuat, kirim ulang tarif | Pengembang | Masing-masing satu task `build-module-backend` |
| 6 | Jalankan `FLOW-OPT-MVP-001` dan `UAT-01`..`26` dengan akun asli di PostgreSQL, termasuk dua petugas serentak | Yoga Aji Pratama, pemilik mitra | Bukti baru untuk `testing/` |
| 7 | Audit ulang | — | `verify-module-readiness` |

Sesudah langkah 6, verdict paling mungkin naik menjadi `READY_WITH_CONDITIONS`, dengan R5, R7..R11 sebagai kondisi bila tidak ada temuan baru.

## 11. Batas audit

| Hal | Keterangan |
| --- | --- |
| Dijalankan ulang | `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False`: 247/247, tanpa server menyala. Hanya menyentuh `bin/obj` |
| Diambil dari sesi yang sama | E2E frontend 146/146 dan uji unit (72 gagal dari 4.470, seluruhnya modul lain, daftar berkas identik baseline) dari `FE-OPT-033`; source frontend tidak berubah sesudahnya |
| Tidak diperiksa | Server dev bersama, produksi, variabel lingkungan server, kinerja pada data besar, dan isi berkas Rekam Medis asli karena pembukaannya menulis jejak akses |
| Referensi Git | Selisih branch dibaca dari referensi lokal tanpa `git fetch`, sehingga angkanya bisa lebih besar |
| Penulisan | Hanya laporan ini. Tidak ada perubahan source dan tidak ada tulisan ke basis data |
