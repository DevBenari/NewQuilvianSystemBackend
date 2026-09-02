# Roadmap Delivery Backend — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `1` |
| Status | `DRAFT` |
| Bentuk blueprint | `SINGLE` |
| Ditulis oleh | `plan-module-delivery` |
| Tanggal | 2026-09-02 |
| Manifest | `blueprint-manifest.md` revision `19` |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Contract version | `LAB-API-v1` r3, `LAB-STATE-v1` r2, `LAB-VAL-v1` r3, `LAB-INT-v1` r3, `LAB-PERM-v1` r3 — seluruhnya `approved`, dikunci 2026-09-02 |
| Masukan | Decisions rev `21`; capability map rev `2`; `LAB-RCG-001` rev 5; `LAB-DA-001` rev 4 |
| Input hash | `sha256:75d285252aa5bce7fcaf5d90242da0d30fbd58a92a16aca3377683243be45f61` (decisions), dihitung 2026-09-02 |
| Slice in scope | `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |

> **Dokumen ini bukan izin menulis kode.** Ia daftar pekerjaan beserta syaratnya. Satu task baru
> boleh dikerjakan setelah disetujui satu per satu, lewat
> `quilvian-engineering-skills:build-module-backend`.

---

## 1. Gerbang yang Berlaku untuk Seluruh Task

Empat penghambat berikut **tidak** menghalangi penerbitan roadmap ini, tetapi menghalangi
**eksekusi**-nya. Selama belum dicabut, setiap task di bawah tidak boleh dikerjakan, walaupun
rencananya sudah lengkap.

| ID | Isi | Siapa yang mencabut | Yang tertahan |
|---|---|---|---|
| `LAB-OPEN-018` | Rules root runtime hanya memuat 13 dari 29 berkas; `GLOBAL_RULES.md` dan `rules/backend/engineering/` hilang | Pemasang suite Skill di mesin masing-masing | **Seluruh** task backend. `AGENTS.md` memaksa berhenti dengan `BLOCKED — canonical governance unavailable` |
| `LAB-OPEN-019` | Lifecycle registry `LaboratoryManagement / Laboratory` masih `PLANNED` | Pemilik registry prefix | **Seluruh** task yang membuat atau mengubah entity `Lab*`, menurut `QBE-MOD-002` |
| `LAB-OPEN-020` | `Invoke-QbeConformanceCheck.ps1` gagal `TOOL ERROR` karena menunjuk `agents/rules/engineering/` yang sudah dicabut | **Andry Zain** (`andryzain01@gmail.com`), pemilik repository backend | Pemeriksaan konformansi QBE, termasuk gerbang CI. Bukan penahan task, tetapi membuat pemeriksaannya tidak berjalan |
| `LAB-OPEN-021` | Prefix dua tabel batas nilai: `Mst` atau `Lab` | Pemilik registry prefix | `BE-LAB-02`, dan penamaan resource pada `BE-LAB-04` |

**Yang tetap bisa berjalan sekarang tanpa menunggu satu pun di atas:** penyusunan DTO dan bentuk
request/response di atas kertas, penulisan skenario pengujian dari matriks acceptance, dan
koordinasi tiga task eksternal `BE-EXT-01` sampai `BE-EXT-03`.

### Catatan wajib pada setiap handoff implementasi

QBE preflight dan kesesuaian engineering **diselesaikan pada waktu eksekusi**, dibaca dari
`AGENTS.md` repository backend target beserta dokumen engineering canonical — bukan dari roadmap
ini. Roadmap tidak menetapkan Area, prefix, maupun applicability; ia hanya menyebut apa yang
sudah diketahui saat perencanaan.

---

## 2. Urutan Gelombang

| Gelombang | Task backend | Slice | Kenapa urutannya begini |
|---|---|---|---|
| `MVP-0` | `BE-LAB-01` .. `BE-LAB-07`, `BE-EXT-01` | `S3`, `S11`, `S14` | Murni penambahan dan penyajian. Tidak menyentuh satu baris pun perilaku yang sudah berjalan |
| `MVP-1` | `BE-LAB-08` .. `BE-LAB-10`, `BE-EXT-02`, `BE-EXT-03` | `S13a`, `S13b`, `S1a` | Pendaftaran adalah hulu alur; penanda cito melekat pada pemeriksaan yang dibuat di situ |
| `MVP-2` | `BE-LAB-11` .. `BE-LAB-13` | `S2`, `S10` | Satu perubahan struktural yang tidak dapat dipecah; fakta tagih mengikuti satuan barunya |
| `MVP-3` | `BE-LAB-14`, `BE-LAB-15` | `S7`, `S15` | Membutuhkan penanda cito dari `MVP-1` dan satuan pekerjaan dari `MVP-2` |

**Perubahan terhadap urutan pada `04-prd-to-mvp.md` bagian 14.** PRD menempatkan seluruh layar
pada `MVP-4`. Sejak kontrak dikunci 2026-09-02, `plan-module-delivery` langkah 2 mengizinkan
kerja backend dan frontend berjalan **paralel** untuk kontrak yang sudah `approved` dan
versioned. Karena itu task frontend dipasangkan ke gelombang backendnya masing-masing pada
`frontend-roadmap.md`, dan `MVP-4` tidak lagi berdiri sebagai gelombang tersendiri.

---

## 3. Task Gelombang `MVP-0`

### `BE-LAB-01` — Kolom disiplin pada pesanan laboratorium

| Butir | Isi |
|---|---|
| **Outcome** | Setiap pesanan laboratorium menyimpan disiplinnya — Patologi Klinik, Patologi Anatomi, atau Mikrobiologi — dan disiplin itu tidak dapat berpindah setelah pesanan dibuat |
| **Requirement/decision** | `FR-10.3`, `LAB-DEC-025` |
| **Kontrak** | `LAB-API-v1` r3 — `LabOrderDetailResponse` bertambah ruas `discipline` |
| **Reuse** | `CAP-01` `Extend`. `LabOrder` sudah ada beserta migrationnya |
| **Cakupan** | Satu kolom `Discipline` bertipe enum pada `LabOrder`, satu migration penambahan kolom, penyesuaian DTO respons |
| **Dependency** | — |
| **Acceptance criteria** | `AC-11`, `AC-41`; disiplin tidak dapat diubah setelah pesanan dibuat |
| **Verifikasi** | Uji integrasi: buat pesanan berdisiplin Mikrobiologi, pastikan `discipline` terisi pada respons detail dan upaya mengubahnya ditolak |
| **Risiko/pemilik** | Rendah. Penambahan kolom pada tabel berisi data — kolom boleh kosong untuk baris lama. Pemilik: Laboratorium |
| **DoD** | Kolom ada, migration jalan maju dan mundur, DTO respons memuat `discipline`, uji integrasi hijau, tidak ada endpoint lain yang berubah perilakunya |

### `BE-LAB-02` — Tabel batas nilai dan pilihan hasil

| Butir | Isi |
|---|---|
| **Outcome** | Satu jenis pemeriksaan dapat memiliki beberapa baris batas nilai menurut jenis kelamin dan kelompok umur, dalam dua bentuk hasil: angka dan pilihan terbatas |
| **Requirement/decision** | `FR-03.1`, `FR-03.2`, `FR-03.6`, `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Value Bound; `LAB-VAL-v1` r3 `VAL-21` .. `VAL-24` |
| **Reuse** | `CAP-07` `Missing`. Menunjuk `MstProcedure` dan `MstAgeCategory` yang sudah ada |
| **Cakupan** | Dua entity beserta configuration, DbSet, dan migration. **`MstProcedure` tidak bertambah satu kolom pun** (`FR-03.6`) |
| **Dependency** | `LAB-OPEN-021` **wajib dijawab lebih dulu** |
| **Acceptance criteria** | `AC-24`, `AC-25`, `AC-28`, `AC-49` |
| **Verifikasi** | Uji integrasi: tiga baris batas Hemoglobin — pria dewasa, wanita dewasa, anak — tersimpan berdampingan; baris keempat berkombinasi sama ditolak `409` dengan pesan `VAL-21`. Uji unit `AC-25`: telusuri skema `MstProcedure` setelah seluruh migration, pastikan nol kolom baru |
| **Risiko/pemilik** | **`BLOCKED`** oleh `LAB-OPEN-021`. Nama tabelnya belum ditetapkan: `MstLabValueBound`/`MstLabValueOption` atau `LabValueBound`/`LabValueOption`. Menebak sendiri melanggar `QBE-NAM-004`. Pemilik pencabutan: pemilik registry prefix |
| **DoD** | Nama tabel sesuai jawaban registry, dua entity ada beserta configuration di `Repositories/Configurations/HealthServices/LaboratoryManagement/`, migration jalan dua arah, `AC-25` terbukti, checker QBE lolos |

### `BE-LAB-03` — Riwayat dan pengajuan perubahan batas kritis

| Butir | Isi |
|---|---|
| **Outcome** | Setiap perubahan batas menghasilkan riwayat permanen, dan batas kritis hanya berubah lewat pengajuan yang disetujui pihak klinis |
| **Requirement/decision** | `FR-03.4`, `FR-03.5`, `LAB-DEC-023` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Critical Bound Approval; `LAB-STATE-v1` r2 daur hidup pengajuan |
| **Reuse** | `CAP-04` sebagai pola riwayat; `CAP-17` `Version` sebagai pola perlindungan konkurensi |
| **Cakupan** | Entity `LabValueBoundChangeRequest` dan `LabValueBoundHistory` beserta configuration, DbSet, dan migration |
| **Dependency** | `BE-LAB-02` |
| **Acceptance criteria** | `AC-33`, `AC-34` |
| **Verifikasi** | Uji integrasi: perubahan batas normal langsung berlaku dan menerbitkan satu baris riwayat tanpa penyetuju; pengajuan perubahan batas kritis berstatus `Submitted` sementara batas lama **tidak berubah** |
| **Risiko/pemilik** | Sedang. Kedua entity ini paling mudah keliru dibuat berawalan `Trx*` — `QBE-NAM-001` melarangnya untuk kode baru, dan rancangan revision 1 memang sempat keliru di sini. Pemilik: Laboratorium |
| **DoD** | Kedua entity ada dengan nama benar, riwayat memuat kolom, nilai lama, nilai baru, pelaku, waktu, dan alasan; `AC-34` terbukti |

### `BE-LAB-04` — Endpoint pengelolaan batas nilai

| Butir | Isi |
|---|---|
| **Outcome** | Kepala instalasi dapat membuat, mengubah, menonaktifkan, dan menelusuri riwayat batas nilai lewat enam endpoint |
| **Requirement/decision** | `FR-03.1` .. `FR-03.3`, `FR-03.5` |
| **Kontrak** | `LAB-API-v1` r3, base `api/v1/health-services/laboratory-management/lab-value-bounds` |
| **Reuse** | `CAP-13` kewenangan per aksi, `CAP-14` pendaftaran permission otomatis lewat `AccessMenuSeeder` |
| **Cakupan** | `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `PUT /{id}/deactivate`, `GET /{id}/history`. Hak akses `LabValueBound : Read`, `: Create`, `: Update` |
| **Dependency** | `BE-LAB-02`, `BE-LAB-03` |
| **Acceptance criteria** | `AC-24`, `AC-28`, `AC-33` jalur tolak, `AC-34` |
| **Verifikasi** | Uji integrasi per endpoint. Jalur gagal wajib diuji: batas angka tanpa satuan ditolak `422` `VAL-22`; batas pilihan tanpa satu pun pilihan ditolak `422` `VAL-23`; batas angka disertai daftar pilihan ditolak `422` `VAL-24`; **upaya mengubah batas kritis lewat `PUT /{id}` biasa ditolak `422` `VAL-28`** |
| **Risiko/pemilik** | Sedang. `VAL-28` adalah pengaman keselamatan — tanpa itu batas kritis dapat diubah diam-diam lewat jalur ubah biasa. Nama resource permission mengikuti jawaban `LAB-OPEN-021`. Pemilik: Laboratorium |
| **DoD** | Enam endpoint tersedia dan terdokumentasi Swagger, `[AccessPermission]` terpasang sehingga permissionnya terdaftar sendiri, seluruh jalur gagal di atas terbukti |

### `BE-LAB-05` — Endpoint pengajuan dan persetujuan batas kritis

| Butir | Isi |
|---|---|
| **Outcome** | Perubahan batas kritis menempuh jalur pengajuan: diajukan kepala instalasi, diputuskan pihak berwenang, dan tidak dapat disetujui oleh pengajunya sendiri |
| **Requirement/decision** | `FR-03.4`, `LAB-DEC-023` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Critical Bound Approval; `LAB-PERM-v1` r3 |
| **Reuse** | `CAP-13`, `CAP-15` identitas pelaku lewat `IHttpContextAccessor`, `CAP-17` konkurensi |
| **Cakupan** | `GET /`, `POST /`, `POST /{requestId}/approve`, `POST /{requestId}/reject`, `POST /{requestId}/withdraw`. Hak akses `LabCriticalBound : Read`, `: Approve`, dan `LabValueBound : Update` |
| **Dependency** | `BE-LAB-03`, `BE-LAB-04` |
| **Acceptance criteria** | `AC-33` seluruh jalur |
| **Verifikasi** | Uji integrasi: penyetujuan mengubah batas kritis dan mengisi penyetuju pada riwayat; **pengaju menyetujui pengajuannya sendiri ditolak `403` `VAL-33`**; pengajuan kedua saat yang pertama belum diputuskan ditolak `409` `VAL-32` |
| **Risiko/pemilik** | **Tinggi.** Larangan menyetujui pengajuan sendiri adalah invariant keselamatan, dan `CAP-16` sudah membuktikan sistem permission yang ada **tidak dapat** menegakkannya: `AccessPermissionService.HasAccessAsync` hanya menjawab boleh atau tidak, tidak pernah membandingkan pelaku sebelumnya. Aturan ini wajib ditulis di dalam service. Pemilik: Laboratorium |
| **DoD** | Lima endpoint tersedia, `VAL-32` dan `VAL-33` terbukti lewat uji, larangan menyetujui sendiri ada sebagai kode di service dan bukan sekadar konfigurasi permission |

> **Terbuka, dan bukan wewenang roadmap.** Siapa pemegang `LabCriticalBound : Approve` di rumah
> sakit ini belum ditetapkan — lihat `04-prd-to-mvp.md` bagian 15. Task ini dapat dibangun,
> tetapi tidak dapat dinyatakan siap pakai sebelum peran itu ditetapkan manajemen rumah sakit.

### `BE-LAB-06` — Pengelolaan alasan penolakan sampel

| Butir | Isi |
|---|---|
| **Outcome** | Kepala instalasi dapat menambah, mengubah, mengurutkan, dan menonaktifkan alasan penolakan; penanda kesalahan internal hanya dapat disetel administrator sistem |
| **Requirement/decision** | `FR-06.1` .. `FR-06.3`, `LAB-DEC-019` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Rejection Reason; `LAB-PERM-v1` r3 `LabRejectionReason : SystemFlag` |
| **Reuse** | `CAP-05` `Reuse with adapter`. `MstLabRejectionReason` sudah ada tetapi hanya punya jalur baca dan tidak punya seeder |
| **Cakupan** | Lima endpoint pengelolaan, satu seeder data awal, dan pemisahan tegas antara kolom yang boleh diubah kepala instalasi dan kolom yang terkunci. `GET /lab-specimens/rejection-reasons` yang sudah ada **tetap dipertahankan** sebagai jalur baca saat menolak sampel |
| **Dependency** | — |
| **Acceptance criteria** | `AC-26` seluruh jalur |
| **Verifikasi** | Uji integrasi: kepala instalasi menambah alasan "Sampel tidak diberi label" dan langsung dapat memakainya. Jalur gagal: **kepala instalasi mengubah penanda kesalahan internal ditolak `403` `VAL-37`**; kode ganda ditolak `409` `VAL-36`; menonaktifkan alasan aktif terakhir ditolak `422` `VAL-38` |
| **Risiko/pemilik** | Sedang. Penanda kesalahan internal menentukan **siapa menanggung biaya** ambil ulang — itulah sebabnya ia terkunci dari kepala instalasi. Bila tabel kosong di lingkungan baru, petugas tidak bisa menolak sampel sama sekali; karena itu seeder masuk cakupan. Pemilik: Laboratorium |
| **DoD** | Lima endpoint tersedia, seeder mengisi data awal, `VAL-36` sampai `VAL-38` terbukti, jalur baca lama tidak berubah perilakunya |

### `BE-LAB-07` — Katalog, harga, dan cakupan penjamin — baca saja

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat katalog pemeriksaan tersaring per disiplin beserta harga satuan dan status cakupan penjamin, tanpa satu pun jalur ubah |
| **Requirement/decision** | `FR-09.1` .. `FR-09.5`, `LAB-DEC-033`, `LAB-DEC-036` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Catalog; `LAB-INT-v1` r3 `INT-06` |
| **Reuse** | `CAP-06` `MstProcedure`, `CAP-10` `MstTariff` beserta pola salinan tarif, dan `MstInsuranceTariff`. **Nol tabel baru** |
| **Cakupan** | `GET /examinations`, `GET /examinations/{procedureId}/price`, `GET /tariffs`. Ditambah penegakan `INV-22`: pemeriksaan yang disiplinnya tidak sesuai pesanan ditolak |
| **Dependency** | `BE-LAB-01`, `BE-EXT-01` |
| **Acceptance criteria** | `AC-43`, `AC-47`, `AC-48`, `AC-51` |
| **Verifikasi** | Uji integrasi: memilih tiga pemeriksaan menampilkan harga satuan, subtotal, dan total, **tanpa** satu baris tagihan pun terbentuk. Uji unit `AC-47`: telusuri seluruh tabel milik Laboratorium, pastikan tidak ada tabel tarif. Jalur gagal: menambahkan Hemoglobin ke pesanan berdisiplin Mikrobiologi ditolak `422` `VAL-46`; upaya mengubah tarif lewat endpoint Laboratorium ditolak `403` `VAL-50` |
| **Risiko/pemilik** | Rendah untuk penyajian harga, **sedang** untuk `INV-22` — penegakannya bergantung pada `BE-EXT-01` yang bukan milik Laboratorium. Pemilik: Laboratorium |
| **DoD** | Tiga endpoint tersedia dan seluruhnya baca saja — tidak ada `POST`, `PUT`, maupun `DELETE` pada grup ini; `AC-47` dan `AC-48` terbukti; `VAL-46` terbukti setelah `BE-EXT-01` selesai |

### `BE-EXT-01` — [Master Data] Kolom disiplin pada `MstProcedure`

| Butir | Isi |
|---|---|
| **Outcome** | `MstProcedure` memiliki satu kolom klasifikasi disiplin, terisi untuk pemeriksaan berpenanda `IsLaboratory` yang sudah ada |
| **Requirement/decision** | `LAB-DEC-036`, `LAB-COORD-005` — **disetujui** 2026-09-01 |
| **Kontrak** | `erd/data-dictionary.md` bagian 9b.1 |
| **Reuse** | `MstProcedure` sudah punya `IsLaboratory`, `IsRadiology`, `IsSurgery`, dan `IsTherapy` — kolom ini sejenis dengan keempatnya |
| **Cakupan** | Kolom `LabDiscipline` bertipe enum boleh kosong, satu index, dan pengisian nilai untuk data yang sudah ada |
| **Dependency** | **Bukan milik Laboratorium.** Dikerjakan pemilik `master-data` |
| **Acceptance criteria** | `AC-51` bergantung padanya |
| **Verifikasi** | Kolom ada, terisi untuk seluruh pemeriksaan berpenanda `IsLaboratory`, dan `BE-LAB-07` dapat menyaring dengannya |
| **Risiko/pemilik** | Dependency eksternal. Persetujuannya sudah ada; pelaksanaannya belum dijadwalkan. Pemilik: pemilik `master-data` |
| **DoD** | Kolom ada, nilainya terisi, dan penyaringan katalog per disiplin terbukti bekerja |

---

## 4. Task Gelombang `MVP-1`

### `BE-EXT-02` — [Master Data] Dua data induk perujuk

| Butir | Isi |
|---|---|
| **Outcome** | Instansi perujuk dan dokter perujuk menjadi data induk global, bukan teks bebas |
| **Requirement/decision** | `LAB-DEC-035`, `LAB-COORD-004` — **disetujui** 2026-09-01 |
| **Kontrak** | `erd/data-dictionary.md` bagian 9b.2 dan 9b.3 |
| **Reuse** | Kunjungan pasien sudah punya penanda `IsReferral` sejak awal; Rawat Jalan dan IGD juga menerima pasien rujukan |
| **Cakupan** | `MstReferralInstitution` dan `MstReferralDoctor` beserta relasi antar keduanya dan penanda aktif |
| **Dependency** | **Bukan milik Laboratorium.** Dikerjakan pemilik `master-data` |
| **Acceptance criteria** | `AC-46`, `AC-50` bergantung padanya |
| **Verifikasi** | Kedua data induk dapat dipilih dari daftar; `AC-50` membuktikan teks bebas ditolak |
| **Risiko/pemilik** | Dependency eksternal. Tanpa ini, "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan "sehat sentosa" terhitung tiga institusi berbeda, dan laporan dokter pengirim tidak akan pernah dapat dipercaya. Pemilik: pemilik `master-data` |
| **DoD** | Kedua tabel ada, dokter tertaut ke instansinya, dan keduanya dapat dibaca modul mana pun |

### `BE-EXT-03` — [Registrasi] Penunjuk perujuk pada kunjungan dan kontrak pemanggilan

| Butir | Isi |
|---|---|
| **Outcome** | Kunjungan menyimpan penunjuk instansi dan dokter perujuk, dan Registrasi menyediakan jalur pemanggilan idempoten bagi Laboratorium |
| **Requirement/decision** | `LAB-DEC-032`, `LAB-DEC-035`, `LAB-COORD-003`, `LAB-COORD-004` — **disetujui** 2026-09-01 |
| **Kontrak** | `LAB-INT-v1` r3 `INT-05` |
| **Reuse** | Registrasi **sudah punya** `EncounterRegistrationSource.WalkIn`, `IsWalkIn`, `IsReferral`, `ReferralNumber`, `IsReferralRequired`, `IsReferralVerified`, dan `PatientEncounterController` yang menangani pembuatan kunjungan datang langsung |
| **Cakupan** | Dua kolom penunjuk pada `TrxPatientEncounter`, ditambah kesepakatan bentuk permintaan dan jawaban beserta perilaku idempotensi dan penolakan |
| **Dependency** | `BE-EXT-02`. **Bukan milik Laboratorium.** Dikerjakan pemilik `registration-management` |
| **Acceptance criteria** | `AC-44`, `AC-45`, `AC-46` bergantung padanya |
| **Verifikasi** | Menekan Simpan dua kali tidak menghasilkan dua kunjungan untuk satu pasien pada hari yang sama; penolakan Registrasi diteruskan apa adanya tanpa data setengah jadi |
| **Risiko/pemilik** | Dependency eksternal, tetapi **sebagian besar sudah ada**. Yang belum hanya dua kolom dan kesepakatan bentuk pemanggilannya. Pemilik: pemilik `registration-management` |
| **DoD** | Dua kolom ada, kontrak `INT-05` disepakati tertulis, idempotensi terbukti lewat uji |

### `BE-LAB-08` — Endpoint pendaftaran pasien laboratorium

| Butir | Isi |
|---|---|
| **Outcome** | Pasien yang datang langsung ke laboratorium dapat dilayani tanpa mengantre lebih dulu di loket pendaftaran |
| **Requirement/decision** | `FR-08.1` .. `FR-08.5`, `LAB-DEC-032`, `LAB-DEC-035` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Patient Registration; `LAB-INT-v1` r3 `INT-05` |
| **Reuse** | `CAP-08` kunjungan, `CAP-09` identitas pasien dan dokter. Laboratorium **tidak menulis** ke tabel kunjungan maupun tabel pasien |
| **Cakupan** | `GET /patient-search`, `POST /walk-in`, `POST /external-referral`. Ketiganya meneruskan isian ke Registrasi, menunggu jawabannya, lalu mengembalikan penunjuk kunjungan |
| **Dependency** | `BE-EXT-02`, `BE-EXT-03` |
| **Acceptance criteria** | `AC-44`, `AC-45`, `AC-46`, `AC-50` |
| **Verifikasi** | Uji integrasi: pendaftaran datang langsung membentuk kunjungan ber-`IsWalkIn` benar; rujukan luar menyimpan penunjuk instansi, penunjuk dokter, dan nomor surat rujukan. **Uji unit `AC-45`: telusuri seluruh kode Laboratorium, pastikan nol penulisan ke tabel kunjungan maupun tabel pasien.** Jalur gagal: mengetik nama instansi perujuk sebagai teks bebas ditolak `422` `VAL-43` |
| **Risiko/pemilik** | **Tinggi.** Ini titik yang paling mudah dilanggar — batas kewenangan menggoda untuk ditembus demi kemudahan implementasi. `AC-45` adalah penjaganya. Pemilik: Laboratorium |
| **DoD** | Tiga endpoint tersedia, `AC-45` terbukti lewat uji unit, idempotensi terbukti, penolakan Registrasi diteruskan tanpa menyimpan data setengah jadi |

### `BE-LAB-09` — Entity pemeriksaan terpesan

| Butir | Isi |
|---|---|
| **Outcome** | Pemeriksaan terpesan menjadi satuan tersendiri, terpisah dari wadah fisik yang menopangnya |
| **Requirement/decision** | `FR-02.1`, `LAB-DEC-024`, `LAB-DEC-026` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Examination; `02-backend-architecture.md` bagian 4.3 |
| **Reuse** | `CAP-02` `Extend`. Menunjuk `LabOrder`, `TrxLabSpecimen`, dan `MstProcedure` |
| **Cakupan** | Entity `LabExamination` beserta configuration, DbSet, dan migration. Memuat salinan tarif, penanda kesegeraan, dan penanda duplo |
| **Dependency** | `BE-LAB-01` |
| **Acceptance criteria** | `AC-35`, `AC-40` |
| **Verifikasi** | Uji integrasi: satu wadah menopang dua pemeriksaan, keduanya tersimpan sebagai baris tersendiri dengan salinan tarifnya masing-masing |
| **Risiko/pemilik** | Sedang. Namanya **wajib** `LabExamination`, bukan `TrxLabExamination` — `QBE-NAM-001` melarang `Trx*` untuk kode baru, dan rancangan revision 1 sempat keliru di sini sebelum dikoreksi. Pemilik: Laboratorium |
| **DoD** | Entity ada dengan nama benar, configuration berada di folder submodul, migration jalan dua arah, checker QBE lolos |

### `BE-LAB-10` — Penanda cito dan duplo per pemeriksaan

| Butir | Isi |
|---|---|
| **Outcome** | Kesegeraan melekat pada **pemeriksaan**, bukan pada pesanan, sehingga satu pesanan dapat memuat Kalium cito dan Kolesterol biasa sekaligus |
| **Requirement/decision** | `FR-01.1` .. `FR-01.4`, `LAB-DEC-013`, `LAB-DEC-026` |
| **Kontrak** | `LAB-API-v1` r3 — `PUT /lab-examinations/{id}/urgency` dan `PUT /lab-examinations/{id}/duplo` |
| **Reuse** | `CAP-04` riwayat perpindahan status, `CAP-15` identitas pelaku |
| **Cakupan** | Dua endpoint. `LabExaminationResponse` memuat `urgency`, `urgencyMarkedAt`, `urgencyMarkedByUserName`, dan `isDuplo` |
| **Dependency** | `BE-LAB-09` |
| **Acceptance criteria** | `AC-18`, `AC-39`, `AC-40` |
| **Verifikasi** | Uji integrasi: penandaan menyimpan waktu dan pelaku serta menerbitkan satu baris riwayat; mengembalikan menjadi biasa menambah satu baris riwayat lagi. Jalur gagal: **dokter lain menandai cito pesanan yang bukan miliknya ditolak `403` `VAL-03`**; menandai pesanan berstatus `Completed` ditolak `409` `VAL-04`. `AC-40` membuktikan **tidak ada** endpoint kesegeraan pada tingkat pesanan |
| **Risiko/pemilik** | Sedang. `PUT /lab-orders/{id}/urgency` dari kontrak revision 1 **dibatalkan** oleh `LAB-DEC-026`; memasangnya kembali melanggar keputusan itu. Pemilik: Laboratorium |
| **DoD** | Dua endpoint tersedia, `VAL-03` dan `VAL-04` terbukti, `AC-40` terbukti, riwayat terbentuk pada setiap penandaan |

---

## 5. Task Gelombang `MVP-2`

### `BE-LAB-11` — Migration pemisahan wadah dan pemeriksaan

| Butir | Isi |
|---|---|
| **Outcome** | Salinan tarif dan penunjuk pemeriksaan berpindah dari wadah ke baris pemeriksaan, tanpa memutus tautan tagihan yang sudah ada |
| **Requirement/decision** | `FR-02.4`, `FR-02.6`, `LAB-DEC-024` |
| **Kontrak** | `erd/data-dictionary.md`; `02-backend-architecture.md` bagian 6 |
| **Reuse** | `CAP-10` — pola salinan tarif yang sudah benar tinggal dipindahkan satuannya |
| **Cakupan** | Migration menghapus `ProcedureId`, `ProcedureCodeSnapshot`, `ProcedureNameSnapshot`, `TariffId`, `TariffCodeSnapshot`, dan `UnitPriceSnapshot` dari `TrxLabSpecimen`, setelah memindahkan isinya ke `LabExamination` |
| **Dependency** | `BE-LAB-09`. **`LAB-OPEN-012` wajib dijawab lebih dulu** |
| **Acceptance criteria** | `AC-35`, `AC-38` |
| **Verifikasi** | Perhitungan baris sebelum dan sesudah wajib cocok; tidak ada fakta kelayakan tagih yang kehilangan sumbernya |
| **Risiko/pemilik** | **Tinggi, dan `BLOCKED`.** Ini satu-satunya perubahan struktural yang menghapus kolom berisi data. Jumlah baris `TrxLabSpecimen` di produksi belum diketahui. Bila nol, seluruh kerumitan pemindahan gugur dan migration menjadi biasa — tetapi itu **dugaan, bukan bukti**. Pemilik pencabutan: pemilik repository backend atau DBA |
| **DoD** | Jumlah baris produksi diketahui, rencana pemindahan disusun sesuai angka itu, migration jalan dua arah, tidak ada tautan tagihan yang putus |

### `BE-LAB-12` — Endpoint wadah: rencana, layak, tolak

| Butir | Isi |
|---|---|
| **Outcome** | Keputusan layak atau tolak diambil atas **wadah**, dan menolak wadah menggugurkan seluruh pemeriksaan yang ditopangnya |
| **Requirement/decision** | `FR-02.1` .. `FR-02.3`, `FR-02.5`, `LAB-DEC-024` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Specimen — **breaking**; `LAB-STATE-v1` r2 |
| **Reuse** | `CAP-02` `Ready to reuse` sebagai dasar, `CAP-17` `Version` untuk konkurensi |
| **Cakupan** | Tiga endpoint berubah bentuk: `POST /by-order/{labOrderId}`, `POST /{id}/accept`, `POST /{id}/reject`. Sembilan endpoint sampel lainnya tetap apa adanya |
| **Dependency** | `BE-LAB-11` |
| **Acceptance criteria** | `AC-35`, `AC-36`, `AC-37`, `AC-38` |
| **Verifikasi** | Uji integrasi: menolak wadah dua pemeriksaan menjadikan **kedua** pemeriksaan `Voided` dan tidak menerbitkan fakta apa pun; ambil ulang membentuk wadah baru yang menampung seluruh pemeriksaan wadah lama. Jalur gagal: **menolak satu pemeriksaan saja pada wadah berisi dua ditolak `422` `VAL-13`**; merencanakan wadah tanpa pemeriksaan ditolak `422` `VAL-05`; jenis pemeriksaan sama dua kali pada satu wadah ditolak `422` `VAL-07`; menyatakan layak wadah yang belum pernah diterima ditolak `409` `VAL-08`; ambil ulang tanpa mengisi sebab ditolak `422` `VAL-14` |
| **Risiko/pemilik** | **Tinggi.** Perubahan ini `breaking` — bentuk permintaan dan jawaban ketiga endpoint berubah, sehingga pemakai lama wajib diidentifikasi lebih dulu. Pemilik: Laboratorium |
| **DoD** | Tiga endpoint berperilaku baru, `VAL-05`, `VAL-07`, `VAL-08`, `VAL-13`, dan `VAL-14` terbukti, sembilan endpoint lain tidak berubah perilakunya, dampak breaking tercatat pada `contracts/api-contract.md` bagian 3 |

### `BE-LAB-13` — Fakta kelayakan tagih per pemeriksaan

| Butir | Isi |
|---|---|
| **Outcome** | Satu wadah yang dinyatakan layak menerbitkan fakta sebanyak pemeriksaan yang ditopangnya, masing-masing dengan salinan tarifnya sendiri |
| **Requirement/decision** | `FR-05.1` .. `FR-05.4`, `LAB-INH-013` |
| **Kontrak** | `LAB-INT-v1` r3 `INT-01` |
| **Reuse** | `CAP-11` `Ready to reuse` — `ClinicalMilestoneFactProducer`, `EmitChargeEligibilityAsync`, `EmitClinicalCancellationAsync`, dan enum `ClinicalMilestoneKind` seluruhnya sudah terpasang, terhubung, dan teruji. Hanya **satuannya** yang berubah |
| **Cakupan** | Penyesuaian pemanggilan agar `SourceItemId` menunjuk identitas pemeriksaan, bukan wadah |
| **Dependency** | `BE-LAB-11`, `BE-LAB-12` |
| **Acceptance criteria** | `AC-12`, `AC-13`, `AC-37` |
| **Verifikasi** | Uji integrasi: wadah dua pemeriksaan bertarif Rp150.000 dan Rp120.000 menerbitkan dua fakta dengan salinan tarif masing-masing, total rujukan Rp270.000; menekan tombol layak dua kali tetap menghasilkan dua fakta, bukan empat; wadah ditolak tidak menerbitkan fakta apa pun; waktu fakta sama dengan waktu perpindahan ke `Accepted`. **Uji unit `AC-13`: telusuri seluruh model dan service Laboratorium, pastikan nol properti dan nol method finansial** |
| **Risiko/pemilik** | Sedang. `CAP-12` sudah menjaga `AC-13` lewat pengujian otomatis yang ada di `LaboratoryAuthorityTests.cs`; pengujian itu **wajib tetap hijau** setelah perubahan ini. Pemilik: Laboratorium |
| **DoD** | Fakta terbit per pemeriksaan, idempotensi terbukti, `LaboratoryAuthorityTests.cs` tetap hijau, `AC-13` terbukti |

---

## 6. Task Gelombang `MVP-3`

### `BE-LAB-14` — Daftar kerja dan pemantauan keterlambatan cito

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat pekerjaan yang belum selesai dengan cito di urutan atas, dan kepala instalasi melihat pesanan cito yang melewati batas waktunya |
| **Requirement/decision** | `FR-04.1` .. `FR-04.4`, `LAB-DEC-013` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Worklist |
| **Reuse** | Diturunkan dari data yang sudah ada. **Tidak ada tabel daftar kerja** (`FR-04.4`) |
| **Cakupan** | `GET /pending` dan `GET /cito-overdue`. Keterlambatan dihitung sejak wadah dinyatakan layak |
| **Dependency** | `BE-LAB-10`, `BE-LAB-12`, dan batas waktu cito dari `BE-LAB-02` |
| **Acceptance criteria** | `AC-10`, `AC-17`, `AC-39` |
| **Verifikasi** | Uji integrasi: 14 pesanan biasa pukul 10.00 dan satu cito pukul 10.05 — yang cito berada di urutan pertama; dua pesanan cito berbeda waktu masuk sama-sama di atas yang biasa, di antara keduanya urut menurut waktu masuk. Kalium cito berbatas 60 menit, wadah layak pukul 09.00, belum dirilis sampai 10.20 → muncul di daftar pantau dengan kelebihan 20 menit; bila selesai pukul 09.45 → **tidak** muncul. `AC-39`: pada satu pesanan berisi Kalium cito dan Kolesterol biasa, hanya Kalium naik ke urutan atas |
| **Risiko/pemilik** | Sedang. Godaan terbesarnya menyimpan daftar kerja sebagai tabel demi kecepatan — `FR-04.4` melarangnya. Pemilik: Laboratorium |
| **DoD** | Dua endpoint tersedia, urutan cito terbukti, perhitungan keterlambatan terbukti pada kedua jalur, tidak ada tabel daftar kerja yang dibuat |

### `BE-LAB-15` — Monitoring tiga disiplin

| Butir | Isi |
|---|---|
| **Outcome** | Tiga daftar pantau sejajar — Patologi Klinik, Patologi Anatomi, Mikrobiologi — masing-masing sebagai jalur tersendiri |
| **Requirement/decision** | `FR-10.1` .. `FR-10.3`, `LAB-DEC-025` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Monitoring, ditambah `GET /lab-orders/by-discipline/{discipline}` |
| **Reuse** | `CAP-01`, `CAP-08`. Seluruhnya diturunkan dari `LabOrder.Discipline` |
| **Cakupan** | Tiga endpoint dengan penyaring yang sama: pasien, nomor rekam medis, nomor pesanan, periode, jenis kunjungan, unit atau ruangan, penjamin, status pesanan, status wadah, dan penanda cito |
| **Dependency** | `BE-LAB-01`, `BE-LAB-14` |
| **Acceptance criteria** | `AC-41`, `AC-42`, `AC-19` |
| **Verifikasi** | Uji integrasi: ketiga daftar dibuka dengan data campuran, masing-masing hanya menampilkan pesanan berdisiplin sesuai jalurnya. **Uji unit `AC-42`: telusuri seluruh endpoint dan tabel Laboratorium, pastikan tidak ada satu pun yang melayani Bank Darah** |
| **Risiko/pemilik** | Rendah. Tiga jalur terpisah adalah keputusan sadar, bukan duplikasi — bukti lapangan menunjukkan laboratorium memakai tiga daftar sejajar sebagai tiga menu berbeda karena petugasnya pun berbeda. Pemilik: Laboratorium |
| **DoD** | Tiga endpoint tersedia dengan penyaring identik, `AC-41` dan `AC-42` terbukti |

---

## 7. Ringkasan Status Task

| Task | Gelombang | Slice | Status rencana | Penahan spesifik |
|---|---|---|---|---|
| `BE-LAB-01` | `MVP-0` | `S15` | Siap direncanakan | Gerbang global saja |
| `BE-LAB-02` | `MVP-0` | `S3` | **`BLOCKED`** | `LAB-OPEN-021` |
| `BE-LAB-03` | `MVP-0` | `S3` | Siap direncanakan | `BE-LAB-02` |
| `BE-LAB-04` | `MVP-0` | `S3` | Sebagian **`BLOCKED`** | Nama resource menunggu `LAB-OPEN-021` |
| `BE-LAB-05` | `MVP-0` | `S3` | Siap direncanakan | Peran penyetuju belum ditetapkan |
| `BE-LAB-06` | `MVP-0` | `S11` | Siap direncanakan | Gerbang global saja |
| `BE-LAB-07` | `MVP-0` | `S14` | Siap direncanakan | `BE-EXT-01` |
| `BE-EXT-01` | `MVP-0` | `S14` | Menunggu `master-data` | Dependency eksternal |
| `BE-EXT-02` | `MVP-1` | `S13b` | Menunggu `master-data` | Dependency eksternal |
| `BE-EXT-03` | `MVP-1` | `S13a`, `S13b` | Menunggu `registration-management` | Dependency eksternal |
| `BE-LAB-08` | `MVP-1` | `S13a`, `S13b` | Siap direncanakan | `BE-EXT-02`, `BE-EXT-03` |
| `BE-LAB-09` | `MVP-1` | `S2` | Siap direncanakan | Gerbang global saja |
| `BE-LAB-10` | `MVP-1` | `S1a` | Siap direncanakan | `BE-LAB-09` |
| `BE-LAB-11` | `MVP-2` | `S2` | **`BLOCKED`** | `LAB-OPEN-012` |
| `BE-LAB-12` | `MVP-2` | `S2` | Siap direncanakan | `BE-LAB-11` |
| `BE-LAB-13` | `MVP-2` | `S10` | Siap direncanakan | `BE-LAB-11`, `BE-LAB-12` |
| `BE-LAB-14` | `MVP-3` | `S7` | Siap direncanakan | `BE-LAB-10`, `BE-LAB-12` |
| `BE-LAB-15` | `MVP-3` | `S15` | Siap direncanakan | `BE-LAB-01`, `BE-LAB-14` |

**Dua task berstatus `BLOCKED` penuh dan satu sebagian**, di luar gerbang global yang berlaku
untuk semuanya. Tidak satu pun `BLOCKED` itu dapat dicabut oleh modul Laboratorium sendiri.

---

## 8. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-02 | Roadmap backend pertama. 15 task Laboratorium dan 3 task dependency eksternal disusun untuk empat gelombang. Diterbitkan setelah kelima kontrak dikunci dan penanda `STALE` pada capability map dicabut | `DRAFT` |
